#include "esp_camera.h"
#include "WiFi.h"
#include "HTTPClient.h"
#include "ArduinoWebsockets.h"

#define PWDN_GPIO_NUM 32
#define RESET_GPIO_NUM -1
#define XCLK_GPIO_NUM 0
#define SIOD_GPIO_NUM 26
#define SIOC_GPIO_NUM 27

#define Y9_GPIO_NUM 35
#define Y8_GPIO_NUM 34
#define Y7_GPIO_NUM 39
#define Y6_GPIO_NUM 36
#define Y5_GPIO_NUM 21
#define Y4_GPIO_NUM 19
#define Y3_GPIO_NUM 18
#define Y2_GPIO_NUM 5
#define VSYNC_GPIO_NUM 25
#define HREF_GPIO_NUM 23
#define PCLK_GPIO_NUM 22

// const char *ssid = "VIETTEL_Thu Quan";
// const char *password = "hoibochau";

const char *ssid = "TestPhone";
const char *password = "88888888";

const String guid = "6bc780d5-199a-4bb9-bb30-511a25c307de";
// const String guid = "9d6c11d7-26dd-4903-b6a5-a50c23cc3883";
const String checkOnlineUrl = "http://192.168.53.100:1234/CarCheckOnline/CheckEsp32CameraOnline?guid=" + guid;
const String webSocketUrl = "ws://192.168.53.100:1234/WebSocket/Esp32CameraWebSocket?guid=" + guid;

const int maxRetries = 4;
bool isWebSocketConnected = false;
unsigned long lastPongTime = 0;
unsigned long timeOut = 10000;
unsigned long timeToSendPing = 3000;

websockets::WebsocketsClient webSocketClient;

void beginConnectToWebSocket();
void beginConnectToWiFi();
void setupCamera();

void handleWiFiDisconnectTask(void *pvParameters);
void handleHttpRequestToServerTask(void *pvParameters);
void handleWebSocketTask(void *pvParameters);
void handleSendVideoStreamWebSocketTask(void *pvParameters);
void handleCheckWebSocketAliveTask(void *pvParameters);

void onMessageCallback(websockets::WebsocketsMessage message);
void onEventCallback(websockets::WebsocketsEvent event, String data);

void setup()
{
  Serial.begin(115200);
  Serial.setDebugOutput(true);

  setupCamera();
  beginConnectToWiFi();

  xTaskCreatePinnedToCore(handleWiFiDisconnectTask, "handleWiFiDisconnectTask", 2048, NULL, 1, NULL, 1);
  xTaskCreatePinnedToCore(handleHttpRequestToServerTask, "handleHttpRequestToServerTask", 2048, NULL, 2, NULL, 1);
}

void loop() {}

void setupCamera()
{
  camera_config_t config;
  config.ledc_channel = LEDC_CHANNEL_0;
  config.ledc_timer = LEDC_TIMER_0;
  config.pin_d0 = Y2_GPIO_NUM;
  config.pin_d1 = Y3_GPIO_NUM;
  config.pin_d2 = Y4_GPIO_NUM;
  config.pin_d3 = Y5_GPIO_NUM;
  config.pin_d4 = Y6_GPIO_NUM;
  config.pin_d5 = Y7_GPIO_NUM;
  config.pin_d6 = Y8_GPIO_NUM;
  config.pin_d7 = Y9_GPIO_NUM;
  config.pin_xclk = XCLK_GPIO_NUM;
  config.pin_pclk = PCLK_GPIO_NUM;
  config.pin_vsync = VSYNC_GPIO_NUM;
  config.pin_href = HREF_GPIO_NUM;
  config.pin_sccb_sda = SIOD_GPIO_NUM;
  config.pin_sccb_scl = SIOC_GPIO_NUM;
  config.pin_pwdn = PWDN_GPIO_NUM;
  config.pin_reset = RESET_GPIO_NUM;
  config.xclk_freq_hz = 20000000;
  config.pixel_format = PIXFORMAT_JPEG; // Chọn định dạng JPEG để tối ưu kích thước
  config.frame_size = FRAMESIZE_QVGA;   // QVGA (320x240) -> Nhẹ và nhanh
  config.jpeg_quality = 10;             // Chất lượng JPEG (0 = tốt nhất, 63 = kém nhất)
  config.fb_count = 2;                  // Sử dụng 2 buffer để tránh lag khi gửi ảnh

  if (esp_camera_init(&config) != ESP_OK)
  {
    Serial.println("Lỗi khởi tạo camera!");
    return;
  }

  sensor_t *s = esp_camera_sensor_get();
  s->set_framesize(s, FRAMESIZE_QVGA);     // QVGA (320x240)
  s->set_quality(s, 4);                    // Quality = 4
  s->set_brightness(s, 0);                 // Brightness = 0
  s->set_contrast(s, 0);                   // Contrast = 0
  s->set_saturation(s, 0);                 // Saturation = 0
  s->set_special_effect(s, 0);             // No Effect
  s->set_whitebal(s, 1);                   // AWB Enabled
  s->set_awb_gain(s, 1);                   // AWB Gain Enabled
  s->set_wb_mode(s, 0);                    // WB Mode = Auto
  s->set_exposure_ctrl(s, 1);              // AEC Sensor Enabled
  s->set_aec2(s, 0);                       // AEC DSP Disabled
  s->set_ae_level(s, 0);                   // AE Level = 0
  s->set_gain_ctrl(s, 1);                  // AGC Enabled
  s->set_gainceiling(s, (gainceiling_t)2); // Gain Ceiling = 2x
  s->set_bpc(s, 0);                        // BPC Disabled
  s->set_wpc(s, 1);                        // WPC Enabled
  s->set_raw_gma(s, 1);                    // Raw GMA Enabled
  s->set_lenc(s, 1);                       // Lens Correction Enabled
  s->set_hmirror(s, 0);                    // H-Mirror Disabled
  s->set_vflip(s, 0);                      // V-Flip Disabled
  s->set_dcw(s, 1);                        // DCW (Downsize Enable) Enabled
  s->set_colorbar(s, 0);                   // Color Bar Disabled
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
            xTaskCreatePinnedToCore(handleWebSocketTask, "handleWebSocketTask", 4096, NULL, 3, NULL, 1);
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
    // Serial.printf("Free Internal Heap: %d bytes\n", heap_caps_get_free_size(MALLOC_CAP_INTERNAL));
    // Serial.printf("Stack remaining in Http Request To Server Task: %d words\n", uxTaskGetStackHighWaterMark(NULL));
  }
}

void handleWebSocketTask(void *pvParameters)
{
  Serial.println("WebSocket connected");
  beginConnectToWebSocket();
  xTaskCreatePinnedToCore(handleCheckWebSocketAliveTask, "handleCheckWebSocketAliveTask", 2048, NULL, 1, NULL, 1);
  xTaskCreatePinnedToCore(handleSendVideoStreamWebSocketTask, "handleSendVideoStreamWebSocketTask", 4096, NULL, 4, NULL, 0);

  while (isWebSocketConnected)
  {
    if (webSocketClient.available())
      webSocketClient.poll();

    vTaskDelay(300 / portTICK_PERIOD_MS);
    // Serial.printf("Stack remaining in Handle WebSocket Task: %d words\n\n", uxTaskGetStackHighWaterMark(NULL));
  }

  webSocketClient.close();
  Serial.println("WebSocket closed");
  vTaskDelete(NULL);
}

void handleSendVideoStreamWebSocketTask(void *pvParameters)
{
  unsigned long lastTimePing = millis();

  while (isWebSocketConnected)
  {
    if (WiFi.status() != WL_CONNECTED)
    {
      Serial.println("WiFi disconnect");
      continue;
    }

    camera_fb_t *fb = esp_camera_fb_get();

    if (!fb)
    {
      Serial.println("Error: Camera frame is NULL");
      vTaskDelay(20 / portTICK_PERIOD_MS);
      continue;
    }
    if (!fb->buf)
    {
      Serial.println("Error: Camera buffer is NULL");
      esp_camera_fb_return(fb);
      vTaskDelay(20 / portTICK_PERIOD_MS);
      continue;
    }

    if (!webSocketClient.sendBinary((char *)fb->buf, fb->len))
      Serial.println("Send image error!");

    if (millis() - lastTimePing >= timeToSendPing)
    {
      vTaskDelay(10 / portTICK_PERIOD_MS);
      lastTimePing = millis();
      if (!webSocketClient.sendBinary("ping"))
        Serial.println("Send ping error!");
    }

    esp_camera_fb_return(fb);
    vTaskDelay(30 / portTICK_PERIOD_MS);
    Serial.printf("Free Internal Heap: %d bytes\n", heap_caps_get_free_size(MALLOC_CAP_INTERNAL));
    Serial.printf("Stack remaining in Handle Send Video Stream WebSocket Task Task: %d bytes \n\n", uxTaskGetStackHighWaterMark(NULL));
  }

  vTaskDelete(NULL);
}

void handleCheckWebSocketAliveTask(void *pvParameters)
{
  while (isWebSocketConnected)
  {
    // Check timeout
    if (millis() - lastPongTime > timeOut)
    {
      Serial.println("Timeout");
      isWebSocketConnected = false;
    }

    vTaskDelay(3000 / portTICK_PERIOD_MS);
    // Serial.printf("Stack remaining in handle Send Ping WebSocket Task: %d words\n", uxTaskGetStackHighWaterMark(NULL));
  }
  vTaskDelete(NULL);
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
  webSocketClient.onEvent(onEventCallback);
  webSocketClient.onMessage(onMessageCallback);

  while (WiFi.status() != WL_CONNECTED)
    ;

  webSocketClient.connect(webSocketUrl);
}

void onMessageCallback(websockets::WebsocketsMessage message)
{
  if (message.isBinary())
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
  else if (event == websockets::WebsocketsEvent::GotPing)
  {
    Serial.println("Got a ping");
  }
  else if (event == websockets::WebsocketsEvent::GotPong)
  {
    Serial.println("Got a pong");
  }
}
