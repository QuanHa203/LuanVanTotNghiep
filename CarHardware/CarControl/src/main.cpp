#include <Arduino.h>
#include "WiFi.h"
#include "DHT.h"
#include "HTTPClient.h"
#include "ArduinoWebsockets.h"

#define DHTPIN 15 // GPIO15
#define DHTTYPE DHT11

const char *ssid = "VIETTEL_Thu Quan";
const char *password = "hoibochau";

const String guid = "9d6c11d7-26dd-4903-b6a5-a50c23cc3883";
const String checkOnlineUrl = "http://192.168.1.100:1234/CarControl/CheckOnline?guid=" + guid;
const String webSocketUrl = "ws://192.168.1.100:1234/CarControl/Control?guid=" + guid;

const int maxRetries = 4;
bool isWebSocketConnected = false;

websockets::WebsocketsClient webSocketClient;

DHT dht(DHTPIN, DHTTYPE);


void beginConnectToWiFi();

void beginConnectToWebSocket();
void onEventCallback(websockets::WebsocketsEvent event, String data);
void onMessageCallback(websockets::WebsocketsMessage message);

void handleWiFiDisconnectTask(void *pvParameters);
void httpRequestToServerTask(void *pvParameters);
void handleWebSocketTask(void *pvParameters);

void setup()
{
  Serial.begin(115200);
  beginConnectToWiFi();

  xTaskCreatePinnedToCore(httpRequestToServerTask, "httpRequestToServerTask", 2048, NULL, 1, NULL, 0);
  xTaskCreatePinnedToCore(handleWiFiDisconnectTask, "handleWiFiDisconnectTask", 2048, NULL, 1, NULL, 1);
}

void loop()
{
}

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

void httpRequestToServerTask(void *pvParameters)
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
  beginConnectToWebSocket();
  while (isWebSocketConnected)
  {
    if (webSocketClient.available())
      webSocketClient.poll();

    vTaskDelay(10 / portTICK_PERIOD_MS);
    // Serial.printf("Stack remaining in Handle WebSocket Task: %d words\n", uxTaskGetStackHighWaterMark(NULL));
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

  while (WiFi.status() != WL_CONNECTED);

  webSocketClient.connect(webSocketUrl);
}

void onMessageCallback(websockets::WebsocketsMessage message)
{
  Serial.print("Got Message: ");
  Serial.println(message.data());
}

void onEventCallback(websockets::WebsocketsEvent event, String data)
{
  if (event == websockets::WebsocketsEvent::ConnectionOpened)
  {
    isWebSocketConnected = true;
  }
  else if (event == websockets::WebsocketsEvent::ConnectionClosed)
  {
    isWebSocketConnected = false;    
  }
  else if (event == websockets::WebsocketsEvent::GotPing)
  {
    Serial.println("Got a Ping!");
  }
  else if (event == websockets::WebsocketsEvent::GotPong)
  {
    Serial.println("Got a Pong!");
  }
}