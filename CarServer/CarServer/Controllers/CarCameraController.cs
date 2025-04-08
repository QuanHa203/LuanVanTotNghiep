using CarServer.Databases;
using CarServer.Models;
using CarServer.Services.WebSockets;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.WebSockets;

namespace CarServer.Controllers;

public class CarCameraController : Controller
{
    private readonly CarServerDbContext _context;
    private readonly PendingWebSocketRequests _pendingWebSocketRequests;
    private readonly WebSocketHandler _webSocketHandler;
    public CarCameraController(CarServerDbContext context, PendingWebSocketRequests pendingWebSocketRequests, WebSocketHandler webSocketHandler)
    {
        _context = context;
        _pendingWebSocketRequests = pendingWebSocketRequests;
        _webSocketHandler = webSocketHandler;
    }

    [HttpGet]
    public IActionResult CheckOnline(Guid guid)
    {
        Esp32Camera? esp32Camera = _context.Esp32Cameras.Find(guid);
        if (esp32Camera == null)
            return BadRequest();

        esp32Camera.IsOnline = true;
        esp32Camera.LastSeen = DateTime.Now;
        _context.Update(esp32Camera);
        _context.SaveChanges();

        // Require Esp32Camera connect to WebSocket or not
        if (_pendingWebSocketRequests.GetEsp32CameraRequest(guid))
            return StatusCode((int)HttpStatusCode.UpgradeRequired);
        else
            return Ok();
    }

    [HttpGet]
    public async Task Camera(Guid guid)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        using (WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
        {
            using Esp32CameraWebSocket webSocketEsp32Camera = new Esp32CameraWebSocket(webSocket, _webSocketHandler, guid);
        }
    }
}