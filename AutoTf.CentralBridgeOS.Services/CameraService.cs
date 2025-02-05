using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using AutoTf.Logging;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace AutoTf.CentralBridgeOS.Services;

public class CameraService : IDisposable
{
    private readonly Logger _logger;

    private readonly int _frameWidth;
    private readonly int _frameHeight;

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
            $"-f v4l2 -framerate 15 -video_size {_frameWidth}x{_frameHeight} -input_format yuyv422 " +
            $"-i /dev/video0 -map 0:v -loglevel error -c:v mjpeg -pix_fmt yuvj420p -rtbufsize 1500k -preset ultrafast -tune zero_latency " +
            $"-f tee \"[f=segment:segment_time=150:strftime=1]recordings/output-%Y-%m-%d_%H:%M:%S.mp4|[f=mjpeg]udp://127.0.0.1:5000\"";
        // ffmpeg -f v4l2 -framerate 15 -video_size {_frameWidth}x{_frameHeight} -input_format yuyv422 -i /dev/video0 -map 0:v -loglevel error -c:v mjpeg -pix_fmt yuvj420p -rtbufsize 1500k -preset ultrafast -tune zero_latency 
        _ffmpegProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = ffmpegArgs,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            }
        };
        _ffmpegProcess.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                _logger.Log($"FFmpeg Error: {e.Data}");
        };
        _ffmpegProcess.Start();
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