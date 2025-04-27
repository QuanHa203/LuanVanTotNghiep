using CarServer.Models;
using CarServer.Repositories.Interfaces;
using CarServer.Services.WebSockets;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CarServer.Controllers
{
    public class CarCheckOnlineController : Controller
    {
        private readonly IGenericRepository<Esp32Control> _esp32ControlRepository;
        private readonly IGenericRepository<Esp32Camera> _esp32CameraRepository;
        private readonly PendingWebSocketRequests _pendingWebSocketRequests;

        public CarCheckOnlineController(IGenericRepository<Esp32Control> esp32ControlRepository, IGenericRepository<Esp32Camera> esp32CameraRepository, PendingWebSocketRequests pendingWebSocketRequests)
        {
            _esp32ControlRepository = esp32ControlRepository;
            _esp32CameraRepository = esp32CameraRepository;
            _pendingWebSocketRequests = pendingWebSocketRequests;
        }

        [HttpGet]
        public async Task<IActionResult> CheckEsp32ControlOnline(Guid guid)
        {
            Esp32Control? esp32Control = await _esp32ControlRepository.GetByIdAsync(guid);
            if (esp32Control == null)
                return BadRequest();

            esp32Control.IsOnline = true;
            esp32Control.LastSeen = DateTime.Now;
            _esp32ControlRepository.Update(esp32Control);
            await _esp32ControlRepository.SaveChangesAsync();

            // Require Esp32Control connect to WebSocket or not
            if (_pendingWebSocketRequests.GetEsp32ControlRequest(guid))
                return StatusCode((int)HttpStatusCode.UpgradeRequired);
            else
                return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> CheckEsp32CameraOnline(Guid guid)
        {
            Esp32Camera? esp32Camera = await _esp32CameraRepository.GetByIdAsync(guid);
            if (esp32Camera == null)
                return BadRequest();

            esp32Camera.IsOnline = true;
            esp32Camera.LastSeen = DateTime.Now;
            _esp32CameraRepository.Update(esp32Camera);
            await _esp32CameraRepository.SaveChangesAsync();

            // Require Esp32Camera connect to WebSocket or not
            if (_pendingWebSocketRequests.GetEsp32CameraRequest(guid))
                return StatusCode((int)HttpStatusCode.UpgradeRequired);
            else
                return Ok();

        }
    }
}
