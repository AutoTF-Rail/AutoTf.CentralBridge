using System.Diagnostics;
using System.Drawing;
using AutoTf.Logging;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace AutoTf.CentralBridgeOS.Services;

public class CameraService : IDisposable
{
    private readonly Logger _logger;

    private readonly int _frameWidth;
    private readonly int _frameHeight;

    private byte[]? _latestFrameBytes;
    private byte[]? _latestFramePreviewBytes;

    private readonly object _frameLockPreview = new object();
    private readonly object _frameLock = new object();

    private Process? _ffmpegProcess;

    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public CameraService(Logger logger)
    {
        _logger = logger;
        
        Statics.ShutdownEvent += Dispose;
        Directory.CreateDirectory("recordings");

        _frameWidth = 1920;
        _frameHeight = 1080;

        _logger.Log($"CS: Starting video capture with { _frameWidth}x{_frameHeight}, 15 FPS.");

        StartFFmpegProcess();
    }

    private void StartFFmpegProcess()
    {
        string ffmpegArgs =
            $"-f v4l2 -framerate 30 -video_size {_frameWidth}x{_frameHeight} -input_format yuyv422 -i /dev/video0 -loglevel error --c:v mjpeg -f mjpeg udp://127.0.0.1:5000";
                            // $"-f v4l2 -framerate 15 -video_size 1280x720 recordings/output_{DateTime.Now:dd.MMyyyy_HH:mm:ss}.mp4"; // TODO: Save to file

        _ffmpegProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = ffmpegArgs,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        _ffmpegProcess.Start();
    }

    public byte[]? LatestFramePreview
    {
        get
        {
            lock (_frameLockPreview)
            {
                return _latestFramePreviewBytes?.ToArray();
            }
        }
    }

    public byte[]? LatestFrame
    {
        get
        {
            lock (_frameLock)
            {
                return _latestFrameBytes?.ToArray();
            }
        }
    }

    public void Dispose()
    {
        try
        {
            _logger.Log("CS: Disposing camera service.");

            _cancellationTokenSource.Cancel();
            _ffmpegProcess?.WaitForExit();
            _ffmpegProcess?.Dispose();

            _logger.Log("CS: Disposed camera service.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}