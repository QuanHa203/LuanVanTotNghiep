using CarServer.Databases;
using CarServer.Models;
using CarServer.Services.WebSockets;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CarServer.Controllers;

public class CarCameraController : Controller
{
    private readonly CarServerDbContext _context;
    private readonly PendingWebSocketRequests _pendingWebSocketRequests;
    public CarCameraController(CarServerDbContext context, PendingWebSocketRequests pendingWebSocketRequests)
    {
        _context = context;
        _pendingWebSocketRequests = pendingWebSocketRequests;
    }

    public IActionResult CheckOnline([FromRoute] Guid guid)
    {
        Esp32Camera? esp32Camera = _context.Esp32Cameras.Find(guid);
        if (esp32Camera == null)
            return BadRequest();

        esp32Camera.IsOnline = true;
        esp32Camera.LastSeen = DateTime.Now;
        _context.Update(esp32Camera);
        _context.SaveChanges();

        // If need to Esp32Camera connect to WebSocket or not
        if (_pendingWebSocketRequests.GetEsp32CameraRequest(guid))
            return StatusCode((int)HttpStatusCode.UpgradeRequired);
        else
            return Ok();        
    }

    public IActionResult Camera([FromRoute] Guid guid)
    {
        
        return Ok();
    }
}