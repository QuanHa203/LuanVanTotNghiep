using CarServer.Databases;

namespace CarServer.BackgroundServices;

public class OnlineStatusChecker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public OnlineStatusChecker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var thresholdTime = DateTime.Now.AddSeconds(-10);

            Task offlineEsp32ControlTask = Task.Run(async () =>
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<CarServerDbContext>();
                    var offlineEsp32Controls = dbContext.Esp32Controls
                        .Where(esp32 => esp32.LastSeen < thresholdTime && esp32.IsOnline)
                        .ToList();

                    if (offlineEsp32Controls.Any())
                    {
                        foreach (var esp32Control in offlineEsp32Controls)
                        {
                            esp32Control.IsOnline = false;
                        }
                    }
                    await dbContext.SaveChangesAsync();
                }
            });

            Task offlineEsp32CameraTask = Task.Run(async () =>
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<CarServerDbContext>();
                    var offlineEsp32Cameras = dbContext.Esp32Cameras
                        .Where(esp32 => esp32.LastSeen < thresholdTime && esp32.IsOnline)
                        .ToList();

                    if (offlineEsp32Cameras.Any())
                    {
                        foreach (var esp32Camera in offlineEsp32Cameras)
                        {
                            esp32Camera.IsOnline = false;
                        }
                    }
                    await dbContext.SaveChangesAsync();
                }
            });

            await Task.WhenAll(offlineEsp32ControlTask, offlineEsp32CameraTask);


            await Task.Delay(10000, stoppingToken);
        }
    }
}
