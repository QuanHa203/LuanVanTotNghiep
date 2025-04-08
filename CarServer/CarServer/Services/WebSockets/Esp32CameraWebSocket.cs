using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;

namespace CarServer.Services.WebSockets;

public class Esp32CameraWebSocket : IDisposable
{
    private readonly WebSocket _webSocket;
    private readonly WebSocketHandler _webSocketHandler;
    private readonly Guid _guid;
    private readonly HashSet<GuestWebSocket> _subscribedGuestWS = new();

    public event WebSocketHandler.OnMessageReceive? OnMessageReceive;
    public event WebSocketHandler.OnWebSocketClose? OnClose;

    private Stopwatch stopwatch = Stopwatch.StartNew();

    private int _webSocketTimeOut = 10000;
    private long _lastPingTime;
    private bool _isWebSocketOpen = true;

    public Esp32CameraWebSocket(WebSocket webSocket, WebSocketHandler webSocketHandler, Guid guid)
    {
        _webSocket = webSocket;
        _webSocketHandler = webSocketHandler;
        _guid = guid;        
        _lastPingTime = stopwatch.ElapsedMilliseconds;

        if (!_webSocketHandler.AddEsp32CameraSocketEvent(this))
            return;

        Task.Run(async () =>
        {
            while (_isWebSocketOpen)
            {
                var a = stopwatch.ElapsedMilliseconds;
                if (a - _lastPingTime > _webSocketTimeOut)
                    CloseWebSocket();

                await Task.Delay(3000);
            }
        });

        ConnectToWebSocket().Wait();
    }

    public void SubscribeToGuestWebSocket(GuestWebSocket guestWebSocket)
    {
        if (guestWebSocket.GetGuidCar() != _guid)
            return;

        if (_subscribedGuestWS.Contains(guestWebSocket))
            return;

        guestWebSocket.OnMessageReceive += async (message, length) => await SendDataToEsp32CameraAsync(message);
    }

    public void UnsubscribeFromGuestWebSocket(GuestWebSocket guestWebSocket)
    {
        if (guestWebSocket.GetGuidCar() != _guid)
            return;

        if (!_subscribedGuestWS.Contains(guestWebSocket))
            return;

        guestWebSocket.OnMessageReceive -= async (message, length) => await SendDataToEsp32CameraAsync(message);
        _subscribedGuestWS.Remove(guestWebSocket);
    }

    public Guid GetGuid() => _guid;

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

    public void Dispose()
    {
        _webSocketHandler.RemoveEsp32CameraSocketEvent(this);
        _subscribedGuestWS.Clear();
        OnMessageReceive = null;
        OnClose = null;
    }

    private async Task ConnectToWebSocket()
    {
        // Send require Browser connect to WS
        foreach (var mainGuestWS in _webSocketHandler._mainGuestWebSockets.Values)
            await mainGuestWS.RequireBrowserConnectToWebSocket(_guid);

        byte[] pongData = Encoding.UTF8.GetBytes("pong");
        byte[] pingData = Encoding.UTF8.GetBytes("PingFromEsp32Camera");
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

                        SendMessageToBrowser(pingData, result.Count);
                        continue;
                    }

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

    private async Task SendDataToEsp32CameraAsync(ArraySegment<byte> dataToSend)
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            await _webSocket.SendAsync(dataToSend, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }


    private void SendMessageToBrowser(ArraySegment<byte> message, int length)
        => OnMessageReceive?.Invoke(message, length);
}
