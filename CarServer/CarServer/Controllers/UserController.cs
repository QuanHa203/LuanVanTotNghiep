using CarServer.Services.WebSockets;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;

namespace CarServer.Controllers;

public class UserController : Controller
{
    private readonly PendingWebSocketRequests _pendingWebSocketRequests;
    public UserController(PendingWebSocketRequests pendingWebSocketRequests)
    {
        _pendingWebSocketRequests = pendingWebSocketRequests;
    }

    public IActionResult ManageCar()
    {

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> RequireConnectToCar(Guid guid)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
            return BadRequest();

        _pendingWebSocketRequests.AddEsp32ControlRequest(guid);
        _pendingWebSocketRequests.AddEsp32CameraRequest(guid);

        WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        while (webSocket.State == WebSocketState.Open)
        {

        }

        return Ok();
    }
}