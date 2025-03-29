using CarServer.Services.Media.FFmpeg;

namespace CarServer.Services.Media;

public class VideoRecorder
{
    private VideoCreator _videoCreator;

    public VideoRecorder(Guid guid)
    {
        string outputPath = InitializeVideoPath(guid);
        _videoCreator = new VideoCreator(outputPath, 320, 240, 30);
    }
    
    public void AddBuffer(byte[] buffer)        
        => _videoCreator.AddFrame(buffer);

    public void SaveVideo()
        => _videoCreator.Finish();

    private string InitializeVideoPath(Guid guid)
    {
        string recordingPath = Path.Combine(AppContext.BaseDirectory, "Medias", guid.ToString(), "Recordings", DateTime.Now.ToString("yyyy-MM-dd"));
        if (!Directory.Exists(recordingPath))
            Directory.CreateDirectory(recordingPath);

        return Path.Combine(recordingPath, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.mp4");
    }
}
