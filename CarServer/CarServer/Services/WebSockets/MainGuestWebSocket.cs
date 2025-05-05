using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace CarServer.Services.WebSockets;

public class MainGuestWebSocket
{
    private ConcurrentDictionary<Guid, bool> _carConnects = new();

    private readonly WebSocket _webSocket;        

    private bool _isWebSocketOpen = false;

    public MainGuestWebSocket(WebSocket webSocket)
    {
        _isWebSocketOpen = true;
        _webSocket = webSocket;            
    }

    public async Task RequireBrowserConnectToWebSocketAsync(Guid guidCar)
    {
        // Check car connected or not. If connect, prevent send require connect to WS
        if (_carConnects.TryGetValue(guidCar, out _))
            return;

        _carConnects.TryAdd(guidCar, true);

        byte[] message = Encoding.UTF8.GetBytes($"RequireConnectToWebSocket: {guidCar.ToString()}");
        await SendDataToBrowserAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text);
    }

    public async Task ConnectToWebSocketAsync()
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
                ArraySegment<byte> messageReceived = new(buffer, 0, result.Count);
                string message = Encoding.UTF8.GetString(messageReceived);

                // String example:
                // Reconnect: hashCode - carGuid
                if (message.StartsWith("Reconnect: "))
                {
                    try
                    {
                        string[] hashCodeAndCarGuid = message.Split(": ")[1].Split(" - ");
                        string hashCodeString = hashCodeAndCarGuid[0];
                        string carGuidString = hashCodeAndCarGuid[1];
                        if (!int.TryParse(hashCodeString, out int hashCode))
                            continue;

                        if (!Guid.TryParse(carGuidString, out Guid carGuid))
                            continue;

                        // Reconnect
                    }
                    catch { }
                }
            }
        }
    }

    private async Task SendDataToBrowserAsync(ArraySegment<byte> data, WebSocketMessageType messageType)
    {
        if (_webSocket.State == WebSocketState.Open)
            await _webSocket.SendAsync(data, messageType, true, CancellationToken.None);
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
    }
}
