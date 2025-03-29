using CarServer.Databases;
using CarServer.Services.WebSockets;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;

namespace CarServer.Controllers;

public class WebSocketController : Controller
{
    private readonly PendingWebSocketRequests _pendingWebSocketRequests;
    private readonly WebSocketHandler _webSocketHandler;
    private readonly CarServerDbContext _context;

    public WebSocketController(PendingWebSocketRequests pendingWebSocketRequests, WebSocketHandler webSocketHandler, CarServerDbContext context)
    {
        _pendingWebSocketRequests = pendingWebSocketRequests;
        _webSocketHandler = webSocketHandler;
        _context = context;
    }

    public IActionResult ManageCar()
    {
        return View(_context.Cars.ToList());
    }

    [HttpGet]
    public async Task RequireConnectToCar(Guid guid)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        _pendingWebSocketRequests.AddEsp32ControlRequest(guid);
        _pendingWebSocketRequests.AddEsp32CameraRequest(guid);

        using (WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
        {
            WebSocketUser webSocketUser = new WebSocketUser(webSocket, guid);
            await _webSocketHandler.AddWebSocketUser(guid, webSocketUser);
        }
    }

    [HttpGet]
    public IActionResult ReconnectToEsp32Control(Guid guid)
    {
        _pendingWebSocketRequests.AddEsp32ControlRequest(guid);
        return Ok();
    }

    [HttpGet]
    public IActionResult ReconnectToEsp32Camera(Guid guid)
    {
        _pendingWebSocketRequests.AddEsp32CameraRequest(guid);
        return Ok();
    }
}