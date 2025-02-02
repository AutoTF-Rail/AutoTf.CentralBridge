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
    
    private readonly VideoCapture _videoCapture;
    private VideoWriter? _videoWriter;
    
    private Mat? _latestFrame;
    private Mat? _latestFramePreview;
    
    private readonly object _frameLockPreview = new object();
    private readonly object _frameLock = new object();

    public CameraService(Logger logger)
    {
        _logger = logger;
        
        Statics.ShutdownEvent += Dispose;
        Directory.CreateDirectory("recordings");

        _videoCapture = new VideoCapture("/dev/video0");
        _videoCapture.Set(CapProp.HwAcceleration, 1);
        _videoCapture.Set(CapProp.FrameWidth, 1280);
        _videoCapture.Set(CapProp.FrameHeight, 720);
        
        _frameWidth = (int)_videoCapture.Get(CapProp.FrameWidth);
        _frameHeight = (int)_videoCapture.Get(CapProp.FrameHeight);
        
        _videoCapture.ImageGrabbed += VideoCaptureOnImageGrabbed;
        _videoCapture.Start();
        
        _logger.Log($"CS: Starting video capture from {_videoCapture.CaptureSource} with: {_frameWidth}x{_frameHeight}/{_videoCapture.Get(CapProp.Fps)}fps.");
    }

    private void VideoCaptureOnImageGrabbed(object? sender, EventArgs e)
    {
        try
        {
            lock (_frameLock)
            {
                if (_latestFrame != null)
                    _latestFrame.Dispose();
                
                _latestFrame = new Mat();
                _videoCapture.Retrieve(_latestFrame);
            }
        
            lock (_frameLockPreview)
            {
                if (_latestFramePreview != null && !_latestFramePreview.IsEmpty)
                    _latestFramePreview.Dispose();
        
                _latestFramePreview = new Mat();
                CvInvoke.Resize(_latestFrame, _latestFramePreview, new Size(640, 360));
            }
            
            if(!restartingCapture)
                _videoWriter?.Write(_latestFrame);
        }
        catch (Exception ex)
        {
            _logger.Log("CS: Error during image grab:");
            _logger.Log(ex.Message);
        }
    }

    private bool restartingCapture = false;

    // We don't need to call this method on startup, because the sync does it.
    public bool IntervalCapture()
    {
        try
        {
            restartingCapture = true;
            _logger.Log("Intervaling capture.");
            
            if (_videoWriter != null)
                _videoWriter.Dispose();

            StartVideoWriter();

            restartingCapture = false;
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
            _videoWriter = new VideoWriter("recordings/" + DateTime.Now.ToString("dd.MM.yyyy-HH:mm:ss") + ".mp4",
                VideoWriter.Fourcc('a', 'v', 'c', '1'), _videoCapture.Get(CapProp.Fps), new Size((int)_videoCapture.Get(CapProp.FrameWidth), (int)_videoCapture.Get(CapProp.FrameHeight)), true);
            _logger.Log($"Starting capture with {_videoCapture.Get(CapProp.Fps)}FPS {_videoCapture.Get(CapProp.FrameWidth)}x{_videoCapture.Get(CapProp.FrameHeight)}");
        }
        catch (Exception e)
        {
            _logger.Log("CS: Error during capture restart:");
            _logger.Log(e.Message);
        }
    }

    public Mat? LatestFramePreview
    {
        get
        {
            
            lock (_frameLockPreview)
            {
                return _latestFramePreview?.Clone();
            }
        }
    }

    public Mat? LatestFrame
    {
        get
        {
            lock (_frameLock)
            {
                return _latestFrame?.Clone();
            }
        }
    }

    public void Dispose()
    {
        try
        {
            _logger.Log("CS: Disposing camera service.");
            restartingCapture = true;
            _videoCapture.Stop();
            _videoWriter!.Dispose();

            _videoCapture.Release();
            _videoCapture.Dispose();
            _logger.Log("CS: Disposed camera service.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}