using CarServer.Services.Media;
using FFmpeg.AutoGen;
using System;
using System.Net.WebSockets;
using System.Text;


namespace CarServer.Services.WebSockets;

public class WebSocketUser
{
    private readonly WebSocket _webSocket;
    private readonly Guid _guidCar;

    public WebSocketHandler.OnMessageReceive onMessageReceive = null!;
    public WebSocketHandler.OnWebSocketClose onClose = null!;

    private VideoRecorder _videoRecorder = null!;
    private bool isScreenShot = false;
    private bool isInitVideoRecorder = false;
    private bool isRecording = false;    

    public WebSocketUser(WebSocket webSocket, Guid guidCar)
    {
        _webSocket = webSocket;
        _guidCar = guidCar;
    }

    public async Task ConnectToWebSocket()
    {
        byte[] buffer = new byte[128];
        while (_webSocket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed connection", CancellationToken.None);
                onClose?.Invoke();
                return;
            }

            if (result.MessageType == WebSocketMessageType.Text)
            {
                ArraySegment<byte> messageReceived = new ArraySegment<byte>(buffer, 0, result.Count);
                // Browser require recording
                if (messageReceived[0] == 'r' && messageReceived[1] == 'e' && messageReceived[2] == 'c' && messageReceived[3] == 'o' && messageReceived[4] == 'r' && messageReceived[5] == 'd')
                {
                    isRecording = true;
                    continue;
                }

                // Browser require stop recording
                if (messageReceived[0] == 's' && messageReceived[1] == 't' && messageReceived[2] == 'o' && messageReceived[3] == 'p' && messageReceived[4] == 'r' && messageReceived[5] == 'e' && messageReceived[6] == 'c' && messageReceived[7] == 'o' && messageReceived[8] == 'r' && messageReceived[9] == 'd')
                {
                    isRecording = false;
                    continue;
                }

                // Browser require screenshot
                if (messageReceived[0] == 's' && messageReceived[1] == 'c' && messageReceived[2] == 'r' && messageReceived[3] == 'e' && messageReceived[4] == 'e' && messageReceived[5] == 'n' && messageReceived[6] == 's' && messageReceived[7] == 'h' && messageReceived[8] == 'o' && messageReceived[9] == 't')
                {
                    isScreenShot = true;
                    continue;
                }

                // Browser send message to ESP32
                onMessageReceive?.Invoke(messageReceived, result.Count);
            }
        }
    }

    public async Task SendDataToBrowserAsync(ArraySegment<byte> dataFromEsp32Camera, int length, WebSocketMessageType messageType)
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            await _webSocket.SendAsync(dataFromEsp32Camera, messageType, true, CancellationToken.None);
            await Screenshot(dataFromEsp32Camera);
            Recording(dataFromEsp32Camera, length);
        }
    }

    public void CloseWebSocket()
    {
        if (_webSocket.State == WebSocketState.Open || _webSocket.State == WebSocketState.CloseSent || _webSocket.State == WebSocketState.CloseReceived)
        {
            _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed connection", CancellationToken.None);
        }
    }

    private async Task Screenshot(ArraySegment<byte> dataFromEsp32Camera)
    {
        if (!isScreenShot)
            return;

        if (await ImageCapturer.ScreenshotAsync(dataFromEsp32Camera.Array!, _guidCar))
            await _webSocket.SendAsync(Encoding.UTF8.GetBytes("screenshot taken"), WebSocketMessageType.Text, true, CancellationToken.None);

        isScreenShot = false;
    }

    private void Recording(ArraySegment<byte> dataFromEsp32Camera, int length)
    {
        // Browser require recording
        if (isRecording)
        {
            if (!isInitVideoRecorder)
            {
                _videoRecorder = new VideoRecorder(_guidCar);
                isInitVideoRecorder = true;
            }
            if (length <= 4)
                return;

            byte[] buffer = dataFromEsp32Camera.Take(length).ToArray();

            // JPEG format: Start of JPEG: 0xFF 0xD8 - End of JPEG: 0xFF 0xD9
            if (buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[length - 2] == 0xFF && buffer[length - 1] == 0xD9)
                _videoRecorder.AddBuffer(buffer);

        }
        else
        {
            if (!isInitVideoRecorder)
                return;

            _videoRecorder.SaveVideo();
            isInitVideoRecorder = false;
            _videoRecorder = null!;
        }
    }
}
