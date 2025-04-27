using CarServer.Databases;
using CarServer.Models;
using CarServer.Repositories.Interfaces;
using CarServer.Services.WebSockets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;

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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ManageCar() => View(await _carRepository.GetAllAsync());

    [HttpGet]
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
            MainGuestWebSocket mainGuestWebSocket = new(webSocket, _webSocketHandler);
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
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

        using (WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
        {
            using GuestWebSocket guestWebSocket = new(webSocket, _webSocketHandler, guidCar, _webHostEnvironment);
        }
    }

    [HttpGet]
    [Authorize]
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
    [Authorize]
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
    public async Task Esp32CameraWebSocket(Guid guid)
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
            using Esp32ControlWebSocket webSocketEsp32Control = new Esp32ControlWebSocket(webSocket, _webSocketHandler, guid);
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