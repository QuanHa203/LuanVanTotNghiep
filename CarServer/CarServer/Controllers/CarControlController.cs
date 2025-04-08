using System.Net;
using System.Net.WebSockets;
using System.Text;
using CarServer.Databases;
using CarServer.Models;
using CarServer.Services.WebSockets;
using Microsoft.AspNetCore.Mvc;

namespace CarServer.Controllers;

public class CarControlController : Controller
{
    private readonly ILogger<CarControlController> _logger;
    private readonly CarServerDbContext _context;
    private readonly PendingWebSocketRequests _pendingWebSocketRequests;
    private readonly WebSocketHandler _webSocketHandler;

    public CarControlController(ILogger<CarControlController> logger, CarServerDbContext context, PendingWebSocketRequests pendingWebSocketRequests, WebSocketHandler webSocketHandler)
    {
        _logger = logger;
        _context = context;
        _pendingWebSocketRequests = pendingWebSocketRequests;
        _webSocketHandler = webSocketHandler;
    }

    [HttpGet]
    public IActionResult CheckOnline(Guid guid)
    {
        Esp32Control? esp32Control = _context.Esp32Controls.Find(guid);
        if (esp32Control == null)
            return BadRequest();

        esp32Control.IsOnline = true;
        esp32Control.LastSeen = DateTime.Now;
        _context.Update(esp32Control);
        _context.SaveChanges();

        // Require Esp32Control connect to WebSocket or not
        if (_pendingWebSocketRequests.GetEsp32ControlRequest(guid))
            return StatusCode((int)HttpStatusCode.UpgradeRequired);
        else
            return Ok();
    }

    [HttpGet]
    public async Task Control(Guid guid)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        using (WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
        {
            using Esp32ControlWebSocket webSocketEsp32Control = new Esp32ControlWebSocket(webSocket, _webSocketHandler, guid);
        }
    }
}