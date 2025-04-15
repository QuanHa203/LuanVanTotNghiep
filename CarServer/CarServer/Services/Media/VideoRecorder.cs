using CarServer.Services.Media.FFmpeg;
using System.Collections.Concurrent;

namespace CarServer.Services.Media;

public class VideoRecorder
{
    private VideoCreator _videoCreator = null!;
    private static readonly ConcurrentDictionary<Guid, bool> _videoRecordings = new();

    public VideoRecorder(Guid guid, string videoPath)
    {
        if (_videoRecordings.ContainsKey(guid))
            return;

        string outputPath = InitializeVideoPath(guid, videoPath);
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

    private string InitializeVideoPath(Guid guid, string videoPath)
    {
        videoPath = Path.Combine(videoPath, DateTime.Now.ToString("yyyy-MM-dd"));
        if (!Directory.Exists(videoPath))
            Directory.CreateDirectory(videoPath);

        return Path.Combine(videoPath, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.mp4");
    }
}
