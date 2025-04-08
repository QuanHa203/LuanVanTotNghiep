using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace CarServer.Services.WebSockets
{
    public class PendingWebSocketRequests
    {
        private static ConcurrentDictionary<Guid, bool> _pendingEsp32ControlRequest = new();
        private static ConcurrentDictionary<Guid, bool> _pendingEsp32CameraRequest = new();

        /// <summary>
        /// Add guid of Esp32Control in pending request
        /// </summary>
        /// <param name="esp32ControlId">Guid needs add</param>
        public void AddEsp32ControlRequest(Guid esp32ControlId)
            => _pendingEsp32ControlRequest.TryAdd(esp32ControlId, true);

        /// <summary>
        /// Add guid of Esp32Camera in pending request
        /// </summary>
        /// <param name="esp32CameralId">Guid needs add</param>
        public void AddEsp32CameraRequest(Guid esp32CameralId)
            => _pendingEsp32CameraRequest.TryAdd(esp32CameralId, true);

        /// <summary>
        /// Check Guid of Esp32Control need to connect WebSocket or not
        /// </summary>
        /// <param name="esp32ControlId">Guid requires WebSocket connection</param>
        /// <returns>return false if no WebSocket request is required</returns>
        public bool GetEsp32ControlRequest(Guid esp32ControlId)
            => _pendingEsp32ControlRequest.TryRemove(esp32ControlId, out _);

        /// <summary>
        /// Check Guid of Esp32Camera need to connect WebSocket or not
        /// </summary>
        /// <param name="esp32CameralId">Guid requires WebSocket connection</param>
        /// <returns>return false if no WebSocket request is required</returns>
        public bool GetEsp32CameraRequest(Guid esp32CameralId)
            => _pendingEsp32CameraRequest.TryRemove(esp32CameralId, out _);
    }
}
