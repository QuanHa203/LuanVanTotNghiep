using System.Collections.Concurrent;
namespace CarServer.Services.WebSockets;

public class WebSocketHandler
{
    public delegate void OnMessageReceive(ArraySegment<byte> message, int length);
    public delegate void OnWebSocketClose();

    public ConcurrentDictionary<int, MainGuestWebSocket> _mainGuestWebSockets { get; private set; } = new();
    public ConcurrentDictionary<int, GuestWebSocket> _guestWebSockets { get; private set; } = new();
    public ConcurrentDictionary<Guid, Esp32ControlWebSocket> _esp32ControlWebSockets { get; private set; } = new();
    public ConcurrentDictionary<Guid, Esp32CameraWebSocket> _esp32CameraWebSockets { get; private set; } = new();

    public bool AddMainGuestWebSocket(MainGuestWebSocket mainGuestWebSocket) => _mainGuestWebSockets.TryAdd(mainGuestWebSocket.GetHashCode(), mainGuestWebSocket);

    public void RemoveMainGuestWebSocket(MainGuestWebSocket mainGuestWebSocket) => _mainGuestWebSockets.TryRemove(mainGuestWebSocket.GetHashCode(), out _);

    public bool AddGuestWebSocketEvent(GuestWebSocket guestWebSocket)
    {
        if (!_guestWebSockets.TryAdd(guestWebSocket.GetHashCode(), guestWebSocket))
            return false;

        RegisterGuestWebSocketEvent(guestWebSocket);
        return true;
    }

    public void RemoveGuestWebSocketEvent(GuestWebSocket guestWebSocket)
    {
        if (!_guestWebSockets.TryRemove(guestWebSocket.GetHashCode(), out _))
            return;

        UnregisterGuestWebSocketEvent(guestWebSocket);
    }

    public bool AddEsp32ControlWebSocketEvent(Esp32ControlWebSocket esp32ControlWebSocket)
    {
        if (!_esp32ControlWebSockets.TryAdd(esp32ControlWebSocket.GetGuid(), esp32ControlWebSocket))
            return false;

        RegisterEsp32ControlWebSocketEvent(esp32ControlWebSocket);
        return true;
    }

    public void RemoveEsp32ControlWebSocketEvent(Esp32ControlWebSocket esp32ControlWebSocket)
    {
        if (!_esp32ControlWebSockets.TryRemove(esp32ControlWebSocket.GetGuid(), out _))
            return;

        UnregisterEsp32ControlWebSocketEvent(esp32ControlWebSocket);
    }

    public bool AddEsp32CameraSocketEvent(Esp32CameraWebSocket esp32CameraWebSocket)
    {
        if (!_esp32CameraWebSockets.TryAdd(esp32CameraWebSocket.GetGuid(), esp32CameraWebSocket))
            return false;

        RegisterEsp32CameraWebSocketEvent(esp32CameraWebSocket);
        return true;
    }

    public void RemoveEsp32CameraSocketEvent(Esp32CameraWebSocket esp32CameraWebSocket)
    {
        if (!_esp32CameraWebSockets.TryRemove(esp32CameraWebSocket.GetGuid(), out _))
            return;

        UnregisterEsp32CameraWebSocketEvent(esp32CameraWebSocket);
    }

    private void RegisterGuestWebSocketEvent(GuestWebSocket guestWebSocket)
    {
        foreach (var item in _esp32ControlWebSockets.Values)
        {
            guestWebSocket.SubscribeToEsp32ControlWebSocket(item);
            item.SubscribeToGuestWebSocket(guestWebSocket);
        }

        foreach (var item in _esp32CameraWebSockets.Values)
        {
            guestWebSocket.SubscribeToEsp32CameraWebSocket(item);
            item.SubscribeToGuestWebSocket(guestWebSocket);
        }
    }

    private void RegisterEsp32ControlWebSocketEvent(Esp32ControlWebSocket esp32ControlWebSocket)
    {
        foreach (var item in _guestWebSockets.Values)
        {
            esp32ControlWebSocket.SubscribeToGuestWebSocket(item);
            item.SubscribeToEsp32ControlWebSocket(esp32ControlWebSocket);
        }
    }

    private void RegisterEsp32CameraWebSocketEvent(Esp32CameraWebSocket esp32CameraWebSocket)
    {
        foreach (var item in _guestWebSockets.Values)
        {
            esp32CameraWebSocket.SubscribeToGuestWebSocket(item);
            item.SubscribeToEsp32CameraWebSocket(esp32CameraWebSocket);
        }
    }

    private void UnregisterGuestWebSocketEvent(GuestWebSocket guestWebSocket)
    {
        foreach (var item in _esp32ControlWebSockets.Values)
        {
            guestWebSocket.UnsubscribeFromEsp32ControlWebSocket(item);
            item.UnsubscribeFromGuestWebSocket(guestWebSocket);
        }

        foreach (var item in _esp32CameraWebSockets.Values)
        {
            guestWebSocket.UnsubscribeFromEsp32CameraWebSocket(item);
            item.UnsubscribeFromGuestWebSocket(guestWebSocket);
        }
    }

    private void UnregisterEsp32ControlWebSocketEvent(Esp32ControlWebSocket esp32ControlWebSocket)
    {
        foreach (var item in _guestWebSockets.Values)
        {
            esp32ControlWebSocket.UnsubscribeFromGuestWebSocket(item);
            item.UnsubscribeFromEsp32ControlWebSocket(esp32ControlWebSocket);
        }
    }

    private void UnregisterEsp32CameraWebSocketEvent(Esp32CameraWebSocket esp32CameraWebSocket)
    {
        foreach (var item in _guestWebSockets.Values)
        {
            esp32CameraWebSocket.UnsubscribeFromGuestWebSocket(item);
            item.UnsubscribeFromEsp32CameraWebSocket(esp32CameraWebSocket);
        }
    }
}
