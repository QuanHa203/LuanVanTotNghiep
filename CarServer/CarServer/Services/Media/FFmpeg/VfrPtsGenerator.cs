using System.Diagnostics;

namespace CarServer.Services.Media.FFmpeg;

public class VfrPtsGenerator
{
    private readonly Stopwatch _stopwatch;
    private readonly long _timeBaseDen;
    private long _lastPts = -1;

    public VfrPtsGenerator(int timeBaseDen = 90000)
    {
        _stopwatch = Stopwatch.StartNew();
        _timeBaseDen = timeBaseDen;
    }

    public long GetPts()
    {
        long pts = (long)(_stopwatch.Elapsed.TotalSeconds * _timeBaseDen);

        // Đảm bảo pts luôn tăng (không bị duplicate)
        if (pts <= _lastPts)
        {
            pts = _lastPts + 1;
        }

        _lastPts = pts;
        return pts;
    }
}
