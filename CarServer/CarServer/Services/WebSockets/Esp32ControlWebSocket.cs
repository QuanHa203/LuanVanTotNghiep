using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;

namespace CarServer.Services.WebSockets;

public class Esp32ControlWebSocket : IDisposable
{
    private readonly WebSocket _webSocket;
    private readonly Guid _guid;
    private readonly HashSet<GuestWebSocket> _subscribedGuestWS = new();

    public event WebSocketHandler.OnMessageReceive? OnMessageReceive;
    public event WebSocketHandler.OnWebSocketClose? OnClose;

    private Stopwatch stopwatch = Stopwatch.StartNew();
    private int _webSocketTimeOut = 5000;
    private bool _isWebSocketOpen = true;
    private long _lastPingTime;

    public Esp32ControlWebSocket(WebSocket webSocket, Guid guid)
    {
        _webSocket = webSocket;
        _lastPingTime = stopwatch.ElapsedMilliseconds;
        _guid = guid;
    }

    public async Task ConnectToWebSocketAsync()
    {
        byte[] pongData = Encoding.UTF8.GetBytes("pong");
        byte[] pingData = Encoding.UTF8.GetBytes("PingFromEsp32Control");
        byte[] buffer = new byte[128];

        _ = Task.Run(async () =>
        {
            while (_isWebSocketOpen)
            {
                if (stopwatch.ElapsedMilliseconds - _lastPingTime > _webSocketTimeOut)
                    CloseWebSocket();

                await Task.Delay(2000);
            }
        });

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

                        SendMessageToBrowser(pingData, pingData.Length);
                    }
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    ArraySegment<byte> messageReceived = new ArraySegment<byte>(buffer, 0, result.Count);
                    SendMessageToBrowser(messageReceived, result.Count);
                }
            }
            catch
            {
                CloseWebSocket();
            }
        }
    }

    public Guid GetGuid() => _guid;

    public void SubscribeToGuestWebSocket(GuestWebSocket guestWebSocket)
    {
        if (guestWebSocket.GetGuidCar() != _guid)
            return;

        if (_subscribedGuestWS.Contains(guestWebSocket))
            return;

        guestWebSocket.OnMessageReceive += async (message, length) => await SendDataToEsp32ControlAsync(message);
    }

    public void UnsubscribeFromGuestWebSocket(GuestWebSocket guestWebSocket)
    {
        if (guestWebSocket.GetGuidCar() != _guid)
            return;

        if (!_subscribedGuestWS.Contains(guestWebSocket))
            return;

        guestWebSocket.OnMessageReceive -= async (message, length) => await SendDataToEsp32ControlAsync(message);
        _subscribedGuestWS.Remove(guestWebSocket);
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

    public void Dispose()
    {
        _subscribedGuestWS.Clear();
        OnMessageReceive = null;
        OnClose = null;
    }


    private async Task SendDataToEsp32ControlAsync(ArraySegment<byte> dataToSend)
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            await _webSocket.SendAsync(dataToSend, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    private void SendMessageToBrowser(ArraySegment<byte> message, int length)
        => OnMessageReceive?.Invoke(message, length);

}
