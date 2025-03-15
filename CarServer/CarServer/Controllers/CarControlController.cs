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
    
    public CarControlController(ILogger<CarControlController> logger, CarServerDbContext context, PendingWebSocketRequests pendingWebSocketRequests)
    {
        _logger = logger;
        _context = context;
        _pendingWebSocketRequests = pendingWebSocketRequests;
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

        // If need to Esp32Control connect to WebSocket or not
        if (_pendingWebSocketRequests.GetEsp32ControlRequest(guid))
            return StatusCode((int)HttpStatusCode.UpgradeRequired);
        else
            return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Control(Guid guid)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            try
            {
                byte[] buffer = new byte[1024];
                while (webSocket.State == WebSocketState.Open)
                {
                    string data = "turnleft";
                    await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(data)), WebSocketMessageType.Text, true, CancellationToken.None);

                    //var rs = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    //string message = Encoding.UTF8.GetString(buffer, 0, rs.Count);
                    //_logger.LogInformation(message);
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                
                _logger.LogInformation("Error: " + ex.Message);
            }
        }

        return Ok();
    }
}