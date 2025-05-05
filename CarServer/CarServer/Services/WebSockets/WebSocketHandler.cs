using FFmpeg.AutoGen;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Concurrent;
using static CarServer.Services.WebSockets.WebSocketHandler;
namespace CarServer.Services.WebSockets;

public class WebSocketHandler
{
    public delegate void OnMessageReceive(ArraySegment<byte> message, int length);
    public delegate void OnWebSocketClose();

    public enum WebSocketRole
    {
        Operation,
        Viewer
    }

    public ConcurrentDictionary<int, MainGuestWebSocket> _mainGuestWebSockets { get; private set; } = new();
    public ConcurrentDictionary<int, GuestWebSocket> _guestWebSockets { get; private set; } = new();
    public ConcurrentDictionary<int, WebSocketRole> _guestWebSocketsRole { get; private set; } = new();
    public ConcurrentDictionary<Guid, Esp32ControlWebSocket> _esp32ControlWebSockets { get; private set; } = new();
    public ConcurrentDictionary<Guid, Esp32CameraWebSocket> _esp32CameraWebSockets { get; private set; } = new();

    public bool AddMainGuestWebSocket(MainGuestWebSocket mainGuestWebSocket)
    {
        if (!_mainGuestWebSockets.TryAdd(mainGuestWebSocket.GetHashCode(), mainGuestWebSocket))
            return false;

        // Send require Browser connect to WS
        foreach (var guidCar in _esp32ControlWebSockets.Keys)
            _ = mainGuestWebSocket.RequireBrowserConnectToWebSocketAsync(guidCar);

        foreach (var guidCar in _esp32CameraWebSockets.Keys)
            _ = mainGuestWebSocket.RequireBrowserConnectToWebSocketAsync(guidCar);

        return true;
    }

    public void RemoveMainGuestWebSocket(MainGuestWebSocket mainGuestWebSocket)
    {
        _mainGuestWebSockets.TryRemove(mainGuestWebSocket.GetHashCode(), out _);
        RemoveCarWhenNoOneConnected();
    }

    public bool AddGuestWebSocketEvent(GuestWebSocket guestWebSocket, WebSocketRole webSocketRole)
    {
        if (!_guestWebSockets.TryAdd(guestWebSocket.GetHashCode(), guestWebSocket))
            return false;

        if (!_guestWebSocketsRole.TryAdd(guestWebSocket.GetHashCode(), webSocketRole))
            return false;

        RegisterGuestWebSocketEvent(guestWebSocket, webSocketRole);
        return true;
    }

    public void RemoveGuestWebSocketEvent(GuestWebSocket guestWebSocket)
    {
        if (!_guestWebSockets.TryRemove(guestWebSocket.GetHashCode(), out _))
            return;

        if (!_guestWebSocketsRole.TryRemove(guestWebSocket.GetHashCode(), out _))
            return;

        UnregisterGuestWebSocketEvent(guestWebSocket);
    }

    public bool AddEsp32ControlWebSocketEvent(Esp32ControlWebSocket esp32ControlWebSocket)
    {
        if (!_esp32ControlWebSockets.TryAdd(esp32ControlWebSocket.GetGuid(), esp32ControlWebSocket))
            return false;

        // Send require Browser connect to WS
        foreach (var mainGuestWS in _mainGuestWebSockets.Values)
            _ = mainGuestWS.RequireBrowserConnectToWebSocketAsync(esp32ControlWebSocket.GetGuid());

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

        // Send require Browser connect to WS
        foreach (var mainGuestWS in _mainGuestWebSockets.Values)
            _ = mainGuestWS.RequireBrowserConnectToWebSocketAsync(esp32CameraWebSocket.GetGuid());

        RegisterEsp32CameraWebSocketEvent(esp32CameraWebSocket);
        return true;
    }

    public void RemoveEsp32CameraSocketEvent(Esp32CameraWebSocket esp32CameraWebSocket)
    {
        if (!_esp32CameraWebSockets.TryRemove(esp32CameraWebSocket.GetGuid(), out _))
            return;

        UnregisterEsp32CameraWebSocketEvent(esp32CameraWebSocket);
    }

    private void RemoveCarWhenNoOneConnected()
    {
        // Remove Esp32Control and Esp32Camera WebSocket when nothing guest connect
        if (_mainGuestWebSockets.IsEmpty)
        {
            // Wait 10s and then remove
            Task.Delay(10000).Wait();
            if (_mainGuestWebSockets.IsEmpty)
            {
                foreach (var item in _esp32ControlWebSockets.Values)
                    item.CloseWebSocket();

                foreach (var item in _esp32CameraWebSockets.Values)
                    item.CloseWebSocket();

                _esp32ControlWebSockets.Clear();
                _esp32CameraWebSockets.Clear();
            }
        }
    }

    private void RegisterGuestWebSocketEvent(GuestWebSocket guestWebSocket, WebSocketRole webSocketRole)
    {
        // If Viewer, only register event from ESP32 Control to get data
        if (webSocketRole == WebSocketRole.Viewer)
        {
            foreach (var item in _esp32ControlWebSockets.Values)
            {
                guestWebSocket.SubscribeToEsp32ControlWebSocket(item);
            }
        }
        // If Operation, register event from ESP32 Control and GuestWebSocket
        else if (webSocketRole == WebSocketRole.Operation)
        {
            foreach (var item in _esp32ControlWebSockets.Values)
            {
                guestWebSocket.SubscribeToEsp32ControlWebSocket(item);
                item.SubscribeToGuestWebSocket(guestWebSocket);
            }
        }


        foreach (var item in _esp32CameraWebSockets.Values)
        {
            guestWebSocket.SubscribeToEsp32CameraWebSocket(item);
            item.SubscribeToGuestWebSocket(guestWebSocket);
        }
    }

    private void RegisterEsp32ControlWebSocketEvent(Esp32ControlWebSocket esp32ControlWebSocket)
    {
        foreach (var key in _guestWebSockets.Keys)
        {
            if (!_guestWebSocketsRole.TryGetValue(key, out var webSocketRole))
                continue;

            if (webSocketRole == WebSocketRole.Viewer)
                continue;

            var guestWebSocket = _guestWebSockets[key];
            esp32ControlWebSocket.SubscribeToGuestWebSocket(guestWebSocket);
            guestWebSocket.SubscribeToEsp32ControlWebSocket(esp32ControlWebSocket);
        }
    }

    private void RegisterEsp32CameraWebSocketEvent(Esp32CameraWebSocket esp32CameraWebSocket)
    {
        foreach (var key in _guestWebSockets.Keys)
        {
            if (!_guestWebSocketsRole.TryGetValue(key, out var webSocketRole))
                continue;

            if (webSocketRole == WebSocketRole.Viewer)
                continue;

            var guestWebSocket = _guestWebSockets[key];
            esp32CameraWebSocket.SubscribeToGuestWebSocket(guestWebSocket);
            guestWebSocket.SubscribeToEsp32CameraWebSocket(esp32CameraWebSocket);
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
        foreach (var key in _guestWebSockets.Keys)
        {
            if (!_guestWebSocketsRole.TryGetValue(key, out var webSocketRole))
                continue;

            if (webSocketRole == WebSocketRole.Viewer)
                continue;

            var guestWebSocket = _guestWebSockets[key];
            esp32ControlWebSocket.UnsubscribeFromGuestWebSocket(guestWebSocket);
            guestWebSocket.UnsubscribeFromEsp32ControlWebSocket(esp32ControlWebSocket);
        }
    }

    private void UnregisterEsp32CameraWebSocketEvent(Esp32CameraWebSocket esp32CameraWebSocket)
    {
        foreach (var key in _guestWebSockets.Keys)
        {
            if (!_guestWebSocketsRole.TryGetValue(key, out var webSocketRole))
                continue;

            if (webSocketRole == WebSocketRole.Viewer)
                continue;

            var guestWebSocket = _guestWebSockets[key];

            esp32CameraWebSocket.UnsubscribeFromGuestWebSocket(guestWebSocket);
            guestWebSocket.UnsubscribeFromEsp32CameraWebSocket(esp32CameraWebSocket);
        }
    }
}
