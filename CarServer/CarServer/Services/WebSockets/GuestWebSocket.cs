using CarServer.Services.Media;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;


namespace CarServer.Services.WebSockets;

public class GuestWebSocket : IDisposable
{
    private readonly WebSocket _webSocket;
    private readonly WebSocketHandler _webSocketHandler;
    private readonly Guid _guidCar;
    private readonly HashSet<Esp32CameraWebSocket> _subscribedEsp32CameraWS = new();
    private readonly HashSet<Esp32ControlWebSocket> _subscribedEsp32ControlWS = new();

    public event WebSocketHandler.OnMessageReceive? OnMessageReceive;
    public event WebSocketHandler.OnWebSocketClose? OnClose;

    private VideoRecorder _videoRecorder = null!;
    private bool isScreenShot = false;
    private bool isInitVideoRecorder = false;
    private bool isRecording = false;

    private Stopwatch stopwatch = Stopwatch.StartNew();
    private long _lastPingTime;
    private int _webSocketTimeOut = 10000;
    private bool _isWebSocketOpen = false;

    private readonly string _imagePath;
    private readonly string _videoPath;

    public GuestWebSocket(WebSocket webSocket, WebSocketHandler webSocketHandler, Guid guidCar, IWebHostEnvironment webHostEnvironment)
    {
        _webSocket = webSocket;
        _webSocketHandler = webSocketHandler;
        _guidCar = guidCar;

        _isWebSocketOpen = true;
        _lastPingTime = stopwatch.ElapsedMilliseconds;

        _imagePath = Path.Combine(webHostEnvironment.WebRootPath, "Medias", guidCar.ToString(), "Screenshots");
        _videoPath = Path.Combine(webHostEnvironment.WebRootPath, "Medias", guidCar.ToString(), "Recordings");

        if (!_webSocketHandler.AddGuestWebSocketEvent(this))
            return;

        Task.Run(async () =>
        {
            while (_isWebSocketOpen)
            {
                if (stopwatch.ElapsedMilliseconds - _lastPingTime > _webSocketTimeOut)
                    CloseWebSocket();

                await Task.Delay(3000);
            }
        });

        ConnectToWebSocket().Wait();
    }

    public void SubscribeToEsp32ControlWebSocket(Esp32ControlWebSocket esp32ControlWebSocket)
    {
        if (esp32ControlWebSocket.GetGuid() != _guidCar)
            return;

        if (_subscribedEsp32ControlWS.Contains(esp32ControlWebSocket))
            return;

        esp32ControlWebSocket.OnMessageReceive += async (message, length) => await SendDataToBrowserAsync(message, length, WebSocketMessageType.Text);
        esp32ControlWebSocket.OnClose += async () =>
        {
            byte[] message = Encoding.UTF8.GetBytes($"Esp32ControlClosed: {_guidCar}");
            await SendDataToBrowserAsync(message, message.Length, WebSocketMessageType.Text);
        };
    }

    public void UnsubscribeFromEsp32ControlWebSocket(Esp32ControlWebSocket esp32ControlWebSocket)
    {
        if (esp32ControlWebSocket.GetGuid() != _guidCar)
            return;

        if (!_subscribedEsp32ControlWS.Contains(esp32ControlWebSocket))
            return;

        esp32ControlWebSocket.OnMessageReceive -= async (message, length) => await SendDataToBrowserAsync(message, length, WebSocketMessageType.Text);
        esp32ControlWebSocket.OnClose -= async () =>
        {
            byte[] message = Encoding.UTF8.GetBytes($"Esp32ControlClosed: {_guidCar}");
            await SendDataToBrowserAsync(message, message.Length, WebSocketMessageType.Text);
        };
        _subscribedEsp32ControlWS.Remove(esp32ControlWebSocket);
    }

    public void SubscribeToEsp32CameraWebSocket(Esp32CameraWebSocket esp32CameraWebSocket)
    {
        if (esp32CameraWebSocket.GetGuid() != _guidCar)
            return;

        if (_subscribedEsp32CameraWS.Contains(esp32CameraWebSocket))
            return;

        esp32CameraWebSocket.OnMessageReceive += async (message, length) => await SendDataToBrowserAsync(message, length, WebSocketMessageType.Binary);
        esp32CameraWebSocket.OnClose += async () =>
        {
            byte[] message = Encoding.UTF8.GetBytes($"Esp32ControlClosed: {_guidCar}");
            await SendDataToBrowserAsync(message, message.Length, WebSocketMessageType.Text);
        };
    }

    public void UnsubscribeFromEsp32CameraWebSocket(Esp32CameraWebSocket esp32CameraWebSocket)
    {
        if (esp32CameraWebSocket.GetGuid() != _guidCar)
            return;

        if (!_subscribedEsp32CameraWS.Contains(esp32CameraWebSocket))
            return;

        esp32CameraWebSocket.OnMessageReceive -= async (message, length) => await SendDataToBrowserAsync(message, length, WebSocketMessageType.Binary);
        esp32CameraWebSocket.OnClose -= async () =>
        {
            byte[] message = Encoding.UTF8.GetBytes($"Esp32ControlClosed: {_guidCar}");
            await SendDataToBrowserAsync(message, message.Length, WebSocketMessageType.Text);
        };

        _subscribedEsp32CameraWS.Remove(esp32CameraWebSocket);
    }

    public void CloseWebSocket()
    {
        if (!_isWebSocketOpen)
            return;

        _isWebSocketOpen = false;

        if (_webSocket.State == WebSocketState.Open || _webSocket.State == WebSocketState.CloseSent || _webSocket.State == WebSocketState.CloseReceived)
        {
            _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        }
        OnClose?.Invoke();
    }

    public Guid GetGuidCar() => _guidCar;

    public void Dispose()
    {
        _webSocketHandler.RemoveGuestWebSocketEvent(this);
        _subscribedEsp32ControlWS.Clear();
        _subscribedEsp32CameraWS.Clear();
        OnMessageReceive = null;
        OnClose = null;
    }

    private async Task ConnectToWebSocket()
    {
        byte[] buffer = new byte[128];
        while (_isWebSocketOpen)
        {
            WebSocketReceiveResult result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                CloseWebSocket();
                break;
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

                // Browser send ping
                if (messageReceived[0] == 'p' && messageReceived[1] == 'i' && messageReceived[2] == 'n' && messageReceived[3] == 'g')
                {
                    _lastPingTime = stopwatch.ElapsedMilliseconds;
                    continue;
                }

                // Browser send message to ESP32
                OnMessageReceive?.Invoke(messageReceived, result.Count);
            }
        }
    }

    private async Task SendDataToBrowserAsync(ArraySegment<byte> data, int length, WebSocketMessageType messageType)
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            try
            {
                await _webSocket.SendAsync(data, messageType, true, CancellationToken.None);
                await Screenshot(data);
                Recording(data, length);
            }
            catch { }
        }
    }

    private async Task Screenshot(ArraySegment<byte> dataFromEsp32Camera)
    {
        if (!isScreenShot)
            return;

        if (await ImageCapturer.ScreenshotAsync(dataFromEsp32Camera.Array!, _imagePath))
            await _webSocket.SendAsync(Encoding.UTF8.GetBytes("ScreenshotTaken"), WebSocketMessageType.Text, true, CancellationToken.None);

        isScreenShot = false;
    }

    private void Recording(ArraySegment<byte> dataFromEsp32Camera, int length)
    {
        // Browser require recording
        if (isRecording)
        {
            if (!isInitVideoRecorder)
            {
                _videoRecorder = new VideoRecorder(_guidCar, _videoPath);
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

            _videoRecorder.SaveVideo(_guidCar);
            isInitVideoRecorder = false;
            _videoRecorder = null!;
        }
    }

}
