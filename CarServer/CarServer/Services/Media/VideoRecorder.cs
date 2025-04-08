using CarServer.Services.Media.FFmpeg;
using System.Collections.Concurrent;

namespace CarServer.Services.Media;

public class VideoRecorder
{
    private VideoCreator _videoCreator = null!;
    private static readonly ConcurrentDictionary<Guid, bool> _videoRecordings = new();

    public VideoRecorder(Guid guid)
    {
        if (_videoRecordings.ContainsKey(guid))
            return;

        string outputPath = InitializeVideoPath(guid);
        _videoRecordings.TryAdd(guid, true);
        _videoCreator = new VideoCreator(outputPath, 320, 240, 30);
    }

    public void AddBuffer(byte[] buffer)
        => _videoCreator?.AddFrame(buffer);

    public void SaveVideo(Guid guid)
    {
        _videoCreator?.Finish();
        _videoRecordings.TryRemove(guid, out _);
    }

    private string InitializeVideoPath(Guid guid)
    {
        string recordingPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "Medias", guid.ToString(), "Recordings", DateTime.Now.ToString("yyyy-MM-dd"));
        if (!Directory.Exists(recordingPath))
            Directory.CreateDirectory(recordingPath);

        return Path.Combine(recordingPath, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.mp4");
    }
}
