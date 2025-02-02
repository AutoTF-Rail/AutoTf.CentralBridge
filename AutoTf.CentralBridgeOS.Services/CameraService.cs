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

    private VideoWriter? _videoWriter;
    private byte[]? _latestFrameBytes;
    private byte[]? _latestFramePreviewBytes;

    private readonly object _frameLockPreview = new object();
    private readonly object _frameLock = new object();

    private Process? _ffmpegProcess;
    private StreamReader? _ffmpegOutput;

    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public CameraService(Logger logger)
    {
        _logger = logger;
        
        Statics.ShutdownEvent += Dispose;
        Directory.CreateDirectory("recordings");

        _frameWidth = 1280;
        _frameHeight = 720;

        _logger.Log($"CS: Starting video capture with { _frameWidth}x{_frameHeight}, 15 FPS.");

        StartFFmpegProcess();
    }

    private void StartFFmpegProcess()
    {
        string ffmpegArgs = $"-f v4l2 -framerate 15 -video_size {_frameWidth}x{_frameHeight} -input_format yuyv422 -i /dev/video0 -f image2pipe -loglevel error -vcodec mjpeg -";

        _ffmpegProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = ffmpegArgs,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        _ffmpegProcess.Start();
        _ffmpegOutput = _ffmpegProcess.StandardOutput;

        Task.Run(() => CaptureFrames(_cancellationTokenSource.Token));
    }

    private async Task CaptureFrames(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                byte[] buffer = new byte[1024 * 1024];
                int bytesRead = await _ffmpegOutput!.BaseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                if (bytesRead > 0)
                {
                    lock (_frameLock)
                    {
                        _latestFrameBytes = new byte[bytesRead];
                        Array.Copy(buffer, _latestFrameBytes, bytesRead);
                    }

                    CreatePreviewFrame(_latestFrameBytes);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Log("CS: Error during frame capture.");
            _logger.Log(ex.Message);
        }
    }

    private void CreatePreviewFrame(byte[] frameBytes)
    {
        lock (_frameLockPreview)
        {
            _latestFramePreviewBytes = frameBytes;
        }
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

    public bool IntervalCapture()
    {
        try
        {
            _logger.Log("Interval capture starting.");
            StartVideoWriter();
            return true;
        }
        catch (Exception e)
        {
            _logger.Log("CS: Error during capture interval:");
            _logger.Log(e.Message);
            return false;
        }
    }

    private void StartVideoWriter()
    {
        try
        {
            _videoWriter?.Dispose();
            _videoWriter = new VideoWriter("recordings/" + DateTime.Now.ToString("dd.MM.yyyy-HH:mm:ss") + ".mp4",
                VideoWriter.Fourcc('a', 'v', 'c', '1'), 15, new Size(_frameWidth, _frameHeight), true);

            _logger.Log($"Starting capture with {15} FPS {_frameWidth}x{_frameHeight}");
        }
        catch (Exception e)
        {
            _logger.Log("CS: Error during capture restart:");
            _logger.Log(e.Message);
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
            _ffmpegOutput?.Dispose();

            _videoWriter?.Dispose();

            _logger.Log("CS: Disposed camera service.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}