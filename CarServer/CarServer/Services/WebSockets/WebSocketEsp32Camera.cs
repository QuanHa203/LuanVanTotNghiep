using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;

namespace CarServer.Services.WebSockets;

public class WebSocketEsp32Camera
{
    private readonly WebSocket _webSocket;
    public WebSocketHandler.OnMessageReceive onMessageReceive = null!;
    public WebSocketHandler.OnWebSocketClose onClose = null!;

    private Stopwatch stopwatch = Stopwatch.StartNew();
    private long _lastPingTime;
    private int _webSocketTimeOut = 10000;
    private bool _isWebSocketOpen = false;

    public WebSocketEsp32Camera(WebSocket webSocket)
    {
        _webSocket = webSocket;
        _isWebSocketOpen = true;
        _lastPingTime = stopwatch.ElapsedMilliseconds;

        Task task = Task.Run(async () =>
        {
            while (_isWebSocketOpen)
            {
                var a = stopwatch.ElapsedMilliseconds;
                if (a - _lastPingTime > _webSocketTimeOut)
                    CloseWebSocket();

                await Task.Delay(3000);
            }
        });
    }

    public async Task ConnectToWebSocket()
    {
        byte[] pongData = Encoding.UTF8.GetBytes("pong");
        byte[] buffer = new byte[1024 * 20];

        while (_isWebSocketOpen)
        {
            try
            {
                WebSocketReceiveResult result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    CloseWebSocket();
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    ArraySegment<byte> messageReceived = new ArraySegment<byte>(buffer, 0, result.Count);
                    if (messageReceived[0] == 'p' && messageReceived[1] == 'i' && messageReceived[2] == 'n' && messageReceived[3] == 'g')
                    {
                        _lastPingTime = stopwatch.ElapsedMilliseconds;
                        await _webSocket.SendAsync(new ArraySegment<byte>(pongData), WebSocketMessageType.Binary, true, CancellationToken.None);
                        Console.WriteLine("Got a ping");
                    }
                    else
                        SendMessageToBrowser(messageReceived, result.Count);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                if (_webSocket.State == WebSocketState.Aborted)
                    CloseWebSocket();
            }
        }

        buffer = null!;
    }

    public async Task SendDataToEsp32CameraAsync(ArraySegment<byte> dataToSend)
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            await _webSocket.SendAsync(dataToSend, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    private void CloseWebSocket()
    {
        if (!_isWebSocketOpen)
            return;

        _isWebSocketOpen = false;

        if (_webSocket.State == WebSocketState.Open || _webSocket.State == WebSocketState.CloseSent || _webSocket.State == WebSocketState.CloseReceived)
        {
            _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        }
        onClose?.Invoke();
    }

    private void SendMessageToBrowser(ArraySegment<byte> message, int length)
        => onMessageReceive?.Invoke(message, length);
}
