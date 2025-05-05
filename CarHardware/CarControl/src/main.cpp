#include <Arduino.h>
#include "WiFi.h"
#include "DHT.h"

#include "HTTPClient.h"
#include "ArduinoWebsockets.h"

#include "L298N_Control.h"
#include "L298N.h"
#include "L293D_Control.h"
#include "IDevice_Control.h"

#define DHTPIN 15 // GPIO15
#define DHTTYPE DHT11
// #define DHTTYPE DHT22

#define LED_1 2 // GPIO2
#define LED_2 4 // GPIO4

/* L298N Pinout (ESP32 30 Pin) */
#define IN1A 16 // GPIO16 (RX2)
#define IN2A 17 // GPIO17 (TX2)
#define IN3A 5  // GPIO5
#define IN4A 18 // GPIO18

#define IN1B 32 // GPIO32
#define IN2B 33 // GPIO33
#define IN3B 25 // GPIO25
#define IN4B 26 // GPIO26

/* L293D Pinout (ESP32 38 Pin) */
#define PWM_2B 12           // D3
#define DIR_CLK 14          // D4
#define PWM_0B 27           // D5
#define PWM_0A 26           // D6
#define DIR_EN 25           // D7
#define DIR_SER 19          // D8  - GPIO 16(RX2)
// #define PWM_1A 18        // D9  - GPIO 17(TX2)
// #define PWM_1B 5         // D10
#define PWM_2A 17           // D11
#define DIR_LATCH 16        // D12

const char *ssid = "TestPhone";
const char *password = "88888888";

// const String guid = "6bc780d5-199a-4bb9-bb30-511a25c307de";
const String guid = "9d6c11d7-26dd-4903-b6a5-a50c23cc3883";
const String checkOnlineUrl = "http://192.168.53.100:1234/CarCheckOnline/CheckEsp32ControlOnline?guid=" + guid;
const String webSocketUrl = "ws://192.168.53.100:1234/WebSocket/Esp32ControlWebSocket?guid=" + guid;

const int maxRetries = 4;
bool isWebSocketConnected = false;
unsigned long lastPongTime = 0;
unsigned long timeOut = 5000;

websockets::WebsocketsClient webSocketClient;

// L298N l298nA(IN1A, IN2A, IN3A, IN4A);
// L298N l298nB(IN1B, IN2B, IN3B, IN4B);
// IDevice_Control* deviceControl = new L298N_Control(l298nA, l298nB);

IDevice_Control* deviceControl = new L293D_Control(DIR_LATCH, DIR_SER, DIR_EN, DIR_CLK, PWM_0A, PWM_0B, PWM_2A, PWM_2B);

DHT dht(DHTPIN, DHTTYPE);

void beginConnectToWiFi();

void beginConnectToWebSocket();
void onEventCallback(websockets::WebsocketsEvent event, String data);
void onMessageCallback(websockets::WebsocketsMessage message);

void handleWiFiDisconnectTask(void *pvParameters);
void handleHttpRequestToServerTask(void *pvParameters);
void handleWebSocketTask(void *pvParameters);
void handleCheckWebSocketAliveTask(void *pvParameters);
void handleSendDHT11ToClientTask(void *pvParameters);

void setupLed();
void ledOn();
void ledOff();

void setup()
{
  Serial.begin(115200);
  deviceControl->brake();
  dht.begin();
  setupLed();
  beginConnectToWiFi();

  xTaskCreatePinnedToCore(handleHttpRequestToServerTask, "handleHttpRequestToServerTask", 2048, NULL, 1, NULL, 0);
  xTaskCreatePinnedToCore(handleWiFiDisconnectTask, "handleWiFiDisconnectTask", 2048, NULL, 1, NULL, 1);
}

void loop() {}

void handleWiFiDisconnectTask(void *pvParameters)
{
  while (1)
  {
    if (WiFi.status() != WL_CONNECTED)
    {
      Serial.println("WiFi lost. Reconnecting...");
      WiFi.disconnect();
      vTaskDelay(1000 / portTICK_PERIOD_MS);
      beginConnectToWiFi();
    }
    vTaskDelay(5000 / portTICK_PERIOD_MS);
    // Serial.printf("Stack remaining in WiFi Disconnect Task: %d words\n", uxTaskGetStackHighWaterMark(NULL));
  }
}

void handleHttpRequestToServerTask(void *pvParameters)
{
  while (1)
  {
    if (WiFi.status() == WL_CONNECTED)
    {
      int retryCount = 0;
      HTTPClient httpClient;
      httpClient.begin(checkOnlineUrl);

      while (retryCount < maxRetries)
      {
        int httpStatusCode = httpClient.GET();

        if (httpStatusCode > 0)
        {
          if (httpStatusCode == 426 && isWebSocketConnected == false)
          {
            xTaskCreatePinnedToCore(handleWebSocketTask, "handleWebSocketTask", 3072, NULL, 1, NULL, 0);
          }
          break;
        }
        else
        {
          retryCount++;
          vTaskDelay(1000 / portTICK_PERIOD_MS);
        }
      }

      httpClient.end();
    }

    vTaskDelay(6000 / portTICK_PERIOD_MS);
    // Serial.printf("Stack remaining in Http Request To Server Task: %d words\n", uxTaskGetStackHighWaterMark(NULL));
  }
}

void handleWebSocketTask(void *pvParameters)
{
  Serial.println("WebSocket connected");
  beginConnectToWebSocket();
  xTaskCreatePinnedToCore(handleCheckWebSocketAliveTask, "handleCheckWebSocketAliveTask", 2048, NULL, 2, NULL, 1);
  xTaskCreatePinnedToCore(handleSendDHT11ToClientTask, "handleSendDHT11ToClientTask", 2048, NULL, 2, NULL, 1);

  while (isWebSocketConnected)
  {
    if (webSocketClient.available())
      webSocketClient.poll();

    vTaskDelay(10 / portTICK_PERIOD_MS);
    // Serial.printf("Stack remaining in Handle WebSocket Task: %d words\n", uxTaskGetStackHighWaterMark(NULL));
  }

  webSocketClient.close();
  Serial.println("WebSocket closed");
  deviceControl->brake();
  vTaskDelete(NULL);
}

void handleCheckWebSocketAliveTask(void *pvParameters)
{
  while (isWebSocketConnected)
  {
    webSocketClient.sendBinary("ping");

    // Check timeout
    if (millis() - lastPongTime > timeOut)
      isWebSocketConnected = false;

    vTaskDelay(2000 / portTICK_PERIOD_MS);
    // Serial.printf("Stack remaining in handle Send Ping WebSocket Task: %d words\n", uxTaskGetStackHighWaterMark(NULL));
  }
  vTaskDelete(NULL);
}

void handleSendDHT11ToClientTask(void *pvParameters)
{
  float temperature, humidity;
  while (isWebSocketConnected)
  {
    temperature = dht.readTemperature();
    humidity = dht.readHumidity();

    if (isnan(temperature) || isnan(humidity))
    {
      vTaskDelay(2000 / portTICK_PERIOD_MS);
      continue;
    }

    String json = "{\"temperature\":";
    json += String(temperature);
    json += ",\"humidity\":";
    json += String(humidity);
    json += "}";
    webSocketClient.send(json);

    vTaskDelay(2000 / portTICK_PERIOD_MS);
    // Serial.printf("Stack remaining in Send DHT11 To Client Task: %d words\n", uxTaskGetStackHighWaterMark(NULL));
  }
  vTaskDelete(NULL);
}

void beginConnectToWiFi()
{
  Serial.print("Connecting to WiFi...");
  WiFi.begin(ssid, password);

  unsigned long startAttemptTime = millis();

  while (WiFi.status() != WL_CONNECTED && millis() - startAttemptTime < 5000) // Wait maximum 5s
  {
    delay(500);
    Serial.print(".");
  }

  Serial.print("\nConnected to WiFi: ");
  Serial.println(WiFi.localIP());
}

void beginConnectToWebSocket()
{
  webSocketClient.onMessage(onMessageCallback);
  webSocketClient.onEvent(onEventCallback);

  while (WiFi.status() != WL_CONNECTED)
    ;

  webSocketClient.connect(webSocketUrl);
}

void onMessageCallback(websockets::WebsocketsMessage message)
{
  if (message.isText())
  {
    String command = message.data();
    Serial.println(command);

    if (command == "brake")
      deviceControl->brake();

    else if (command == "forward")
      deviceControl->forward();

    else if (command == "backward")
      deviceControl->backward();

    else if (command == "turnleft")
      deviceControl->turnLeft();

    else if (command == "turnright")
      deviceControl->turnRight();

    else if (command == "ledon")
      ledOn();

    else if (command == "ledoff")
      ledOff();
  }

  else if (message.isBinary())
  {
    String data = message.data();

    if (data == "pong")
      lastPongTime = millis();
  }
}

void onEventCallback(websockets::WebsocketsEvent event, String data)
{
  if (event == websockets::WebsocketsEvent::ConnectionOpened)
  {
    isWebSocketConnected = true;
    lastPongTime = millis();
  }
  else if (event == websockets::WebsocketsEvent::ConnectionClosed)
  {
    isWebSocketConnected = false;
  }
}

void setupLed()
{
  pinMode(LED_1, OUTPUT);
  pinMode(LED_2, OUTPUT);
}

void ledOn()
{
  digitalWrite(LED_1, HIGH);
  digitalWrite(LED_2, HIGH);
}

void ledOff()
{
  digitalWrite(LED_1, LOW);
  digitalWrite(LED_2, LOW);
}
