using System.Net.WebSockets;
using System.Collections.Concurrent;
using System.Text;
namespace CarServer.Services.WebSockets;

public class WebSocketHandler
{
    public delegate void OnMessageReceive(ArraySegment<byte> message, int length);
    public delegate void OnWebSocketClose();

    private static readonly ConcurrentDictionary<Guid, WebSocketUser> _webSocketUsers = new();
    private static readonly ConcurrentDictionary<Guid, WebSocketEsp32Control> _webSocketEsp32Controls = new();
    private static readonly ConcurrentDictionary<Guid, WebSocketEsp32Camera> _webSocketEsp32Cameras = new();

    public async Task AddWebSocketUser(Guid guidCar, WebSocketUser webSocketUser)
    {
        if (_webSocketUsers.ContainsKey(guidCar))
            return;

        if (!_webSocketUsers.TryAdd(guidCar, webSocketUser))
            return;

        webSocketUser.onClose += () =>
        {
            _webSocketUsers.TryRemove(guidCar, out _);
        };
        await webSocketUser.ConnectToWebSocket();
    }

    public async Task AddWebSocketEsp32Control(Guid guidCar, WebSocketEsp32Control webSocketEsp32Control)
    {
        if (!_webSocketUsers.TryGetValue(guidCar, out var webSocketUser))
            return;

        _webSocketEsp32Controls.TryAdd(guidCar, webSocketEsp32Control);

        webSocketUser.onClose += () =>
        {
            _webSocketEsp32Controls.TryRemove(guidCar, out _);
        };
        webSocketUser.onMessageReceive += async (messageReceive, length) => await webSocketEsp32Control.SendDataToEsp32ControlAsync(messageReceive);


        webSocketEsp32Control.onClose += async () =>
        {
            byte[] closeData = Encoding.UTF8.GetBytes("Esp32ControlClosed");
            await webSocketUser.SendDataToBrowserAsync(new ArraySegment<byte>(closeData), closeData.Length, WebSocketMessageType.Text);
        };

        webSocketEsp32Control.onMessageReceive += async (messageReceive, length) 
            => await webSocketUser.SendDataToBrowserAsync(messageReceive, length, WebSocketMessageType.Text);

        await webSocketEsp32Control.ConnectToWebSocket();
    }

    public async Task AddWebSocketEsp32Camera(Guid guidCar, WebSocketEsp32Camera webSocketEsp32Camera)
    {
        if (!_webSocketUsers.TryGetValue(guidCar, out var webSocketUser))
            return;

        _webSocketEsp32Cameras.TryAdd(guidCar, webSocketEsp32Camera);

        webSocketUser.onClose += () =>
        {
            _webSocketEsp32Cameras.TryRemove(guidCar, out _);
        };

        
        webSocketEsp32Camera.onClose += async () =>
        {
            byte[] closeData = Encoding.UTF8.GetBytes("Esp32CameraClosed");
            await webSocketUser.SendDataToBrowserAsync(new ArraySegment<byte>(closeData), closeData.Length, WebSocketMessageType.Text);
        };

        webSocketEsp32Camera.onMessageReceive += async (messageReceive, length)
            => await webSocketUser.SendDataToBrowserAsync(messageReceive, length, WebSocketMessageType.Binary);
        
        await webSocketEsp32Camera.ConnectToWebSocket();
    }

}
