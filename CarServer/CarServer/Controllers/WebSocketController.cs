using CarServer.Databases;
using CarServer.Services.WebSockets;
using Microsoft.AspNetCore.Cors;
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

    public async Task MainClientWebSocket()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        RequireCarConnectToWebSocket();
        using (WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
        {
            MainGuestWebSocket mainGuestWebSocket = new(webSocket, _webSocketHandler);
        }
    }

    public async Task ClientWebSocket(Guid guidCar)
    {
        if (Guid.Empty == guidCar)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        if (_context.Cars.FirstOrDefault(car => car.Id == guidCar) == null)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        using (WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
        {
            using GuestWebSocket guestWebSocket = new(webSocket, _webSocketHandler, guidCar);
        }
    }

    [HttpGet]
    [EnableCors("AllowSpecificOrigin")]    
    public IActionResult ReconnectToEsp32Control(Guid guidCar)
    {
        if (Guid.Empty == guidCar)
            return BadRequest();

        if (_context.Cars.FirstOrDefault(car => car.Id == guidCar) == null)
            return BadRequest();

        _pendingWebSocketRequests.AddEsp32ControlRequest(guidCar);
        return Ok();
    }

    [HttpGet]
    [EnableCors("AllowSpecificOrigin")]
    public IActionResult ReconnectToEsp32Camera(Guid guidCar)
    {
        if (Guid.Empty == guidCar)
            return BadRequest();

        if (_context.Cars.FirstOrDefault(car => car.Id == guidCar) == null)
            return BadRequest();

        _pendingWebSocketRequests.AddEsp32CameraRequest(guidCar);
        return Ok();
    }
    private void RequireCarConnectToWebSocket()
    {
        foreach (var car in _context.Cars)
        {
            _pendingWebSocketRequests.AddEsp32ControlRequest(car.Id);
            _pendingWebSocketRequests.AddEsp32CameraRequest(car.Id);
        }
    }
}