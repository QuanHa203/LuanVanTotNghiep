using CarServer.Models;
using CarServer.Repositories.Interfaces;
using CarServer.Services.WebSockets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Security.Claims;

namespace CarServer.Controllers;

public class WebSocketController : Controller
{
    private readonly PendingWebSocketRequests _pendingWebSocketRequests;
    private readonly WebSocketHandler _webSocketHandler;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IGenericRepository<Car> _carRepository;

    public WebSocketController(PendingWebSocketRequests pendingWebSocketRequests, WebSocketHandler webSocketHandler, IGenericRepository<Car> carRepository, IWebHostEnvironment webHostEnvironment)
    {
        _pendingWebSocketRequests = pendingWebSocketRequests;
        _webSocketHandler = webSocketHandler;
        _carRepository = carRepository;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Operation,Viewer")]
    public async Task<IActionResult> ManageCar() => View(await _carRepository.GetAllAsync());

    [HttpGet]
    [Authorize(Roles = "Admin,Operation,Viewer")]
    public async Task MainGuestWebSocket()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        await RequireCarConnectToWebSocket();
        using (WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
        {
            MainGuestWebSocket mainGuestWebSocket = new(webSocket);
            if (!_webSocketHandler.AddMainGuestWebSocket(mainGuestWebSocket))
                return;

            await mainGuestWebSocket.ConnectToWebSocketAsync();

            _webSocketHandler.RemoveMainGuestWebSocket(mainGuestWebSocket);
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Operation,Viewer")]
    public async Task GuestWebSocket(Guid guidCar)
    {
        if (Guid.Empty == guidCar)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        if (!await IsCarExistAsync(guidCar))
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        string? roleClaim = User.FindFirstValue(ClaimTypes.Role);
        WebSocketHandler.WebSocketRole webSocketRole;

        if (roleClaim == null)
        {
            HttpContext.Response.StatusCode = 404;
            return;
        }

        if (roleClaim == "Admin" || roleClaim == "Operation")
            webSocketRole = WebSocketHandler.WebSocketRole.Operation;
        else if (roleClaim == "Viewer")
            webSocketRole = WebSocketHandler.WebSocketRole.Viewer;
        else
            return;

        using (WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
        {
            using (GuestWebSocket guestWebSocket = new(webSocket, guidCar, _webHostEnvironment))
            {
                if (!_webSocketHandler.AddGuestWebSocketEvent(guestWebSocket, webSocketRole))
                    return;

                await guestWebSocket.ConnectToWebSocketAsync();

                _webSocketHandler.RemoveGuestWebSocketEvent(guestWebSocket);
            }
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Operation,Viewer")]
    [EnableCors("AllowSpecificOrigin")]
    public async Task<IActionResult> ReconnectToEsp32Control(Guid guidCar)
    {
        if (Guid.Empty == guidCar)
            return BadRequest();

        if (!await IsCarExistAsync(guidCar))
            return BadRequest();

        _pendingWebSocketRequests.AddEsp32ControlRequest(guidCar);
        return Ok();
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Operation,Viewer")]
    [EnableCors("AllowSpecificOrigin")]
    public async Task<IActionResult> ReconnectToEsp32Camera(Guid guidCar)
    {
        if (Guid.Empty == guidCar)
            return BadRequest();

        if (!await IsCarExistAsync(guidCar))
            return BadRequest();

        _pendingWebSocketRequests.AddEsp32CameraRequest(guidCar);
        return Ok();
    }    

    [HttpGet]
    public async Task Esp32ControlWebSocket(Guid guid)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        using (WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
        {
            using (Esp32ControlWebSocket webSocketEsp32Control = new Esp32ControlWebSocket(webSocket, guid))
            {
                if (!_webSocketHandler.AddEsp32ControlWebSocketEvent(webSocketEsp32Control))
                    return;

                await webSocketEsp32Control.ConnectToWebSocketAsync();

                _webSocketHandler.RemoveEsp32ControlWebSocketEvent(webSocketEsp32Control);
            }
        }
    }

    [HttpGet]
    public async Task Esp32CameraWebSocket(Guid guid)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        using (WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
        {
            using (Esp32CameraWebSocket webSocketEsp32Camera = new Esp32CameraWebSocket(webSocket, guid))
            {
                if (!_webSocketHandler.AddEsp32CameraSocketEvent(webSocketEsp32Camera))
                    return;

                await webSocketEsp32Camera.ConnectToWebSocketAsync();

                _webSocketHandler.RemoveEsp32CameraSocketEvent(webSocketEsp32Camera);
            }
        }
    }

    private async Task<bool> IsCarExistAsync(Guid guidCar) => await _carRepository.IsExistAsync(c => c.Id == guidCar);

    private async Task RequireCarConnectToWebSocket()
    {
        var cars = await _carRepository.GetAllAsync();
        foreach (var car in cars)
        {
            _pendingWebSocketRequests.AddEsp32ControlRequest(car.Id);
            _pendingWebSocketRequests.AddEsp32CameraRequest(car.Id);
        }
    }
}