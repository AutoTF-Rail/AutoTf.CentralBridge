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

        _videoCapture = new VideoCapture("/dev/video0", VideoCapture.API.V4L2, new []
        {
            new Tuple<CapProp, int>(CapProp.FourCC, VideoWriter.Fourcc('M', 'J', 'P', 'G')),
            new Tuple<CapProp, int>(CapProp.FrameWidth, 1280),
            new Tuple<CapProp, int>(CapProp.FrameHeight, 720),
            new Tuple<CapProp, int>(CapProp.Fps, 15)
        });

        
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
            
            if(!_restartingCapture)
                _videoWriter?.Write(_latestFrame);
        }
        catch (Exception ex)
        {
            _logger.Log("CS: Error during image grab:");
            _logger.Log(ex.Message);
        }
    }

    private bool _restartingCapture = false;

    // We don't need to call this method on startup, because the sync does it.
    public bool IntervalCapture()
    {
        try
        {
            _logger.Log("Intervaling capture.");

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
            _restartingCapture = true;
            _videoWriter?.Dispose();
            _videoWriter = new VideoWriter("recordings/" + DateTime.Now.ToString("dd.MM.yyyy-HH:mm:ss") + ".avi",
                VideoWriter.Fourcc('M', 'J', 'P', 'G'), 15, new Size((int)_videoCapture.Get(CapProp.FrameWidth), (int)_videoCapture.Get(CapProp.FrameHeight)), true);
            _logger.Log($"Starting capture with {15}FPS {_videoCapture.Get(CapProp.FrameWidth)}x{_videoCapture.Get(CapProp.FrameHeight)}");
        
            _restartingCapture = false;
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
            _restartingCapture = true;
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