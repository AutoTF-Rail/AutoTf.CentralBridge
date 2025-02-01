using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace AutoTf.CentralBridgeOS.Services;

public class CameraService : IDisposable
{
    private readonly int _frameWidth;
    private readonly int _frameHeight;
    private readonly VideoCapture _videoCapture;
    private VideoWriter? _videoWriter;
    private Mat? _latestFrame;
    private readonly object _frameLock = new object();
    private Mat? _latestFramePreview;
    private readonly object _frameLockPreview = new object();
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private Task? _frameCaptureTask;
    private int _failedReads = 0;

    public CameraService(int frameWidth = 1920, int frameHeight = 1080)
    {
        try
        {
            _frameWidth = frameWidth;
            _frameHeight = frameHeight;
            
            Statics.ShutdownEvent += Dispose;

            _videoCapture = new VideoCapture(0, VideoCapture.API.V4L2);
            _videoCapture.Set(CapProp.FrameWidth, _frameWidth);
            _videoCapture.Set(CapProp.FrameHeight, _frameHeight);
            _videoCapture.Set(CapProp.FourCC, VideoWriter.Fourcc('M', 'J', 'P', 'G'));
                
            Directory.CreateDirectory("recordings");
            
            if(!NetworkConfigurator.IsInternetAvailable())
                IntervalCapture();
            
            Console.WriteLine("Is capture open: " + _videoCapture.IsOpened);
            Console.WriteLine("Starting capture at " + _videoCapture.Get(CapProp.Fps) + " fps.");

        }
        catch (Exception e)
        {
            Console.WriteLine("Error during ctor.");
            Console.WriteLine(e);
        }
    }

    // We don't need to call this method on startup, because the sync does it (Only if internet is available)
    public bool IntervalCapture(bool restart = true)
    {
        try
        {
            Console.WriteLine("Intervaling capture.");
            
            _failedReads = 0;

            if (_videoWriter != null)
            {
                _videoWriter.Dispose();
                _cancellationTokenSource.Cancel();
                _frameCaptureTask!.Wait();
                _frameCaptureTask!.Dispose();
            }

            if(restart)
                StartVideoWriter();
            
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error during capture interval:");
            Console.WriteLine(e);
            return false;
        }
    }

    // This needs to be a seperate method, so that we don't start 
    private void StartVideoWriter()
    {
        try
        {
            Console.WriteLine("Starting capture.");
            _videoWriter = new VideoWriter("recordings/" + DateTime.Now.ToString("dd.MM.yyyy-HH:mm:ss") + ".mp4",
                VideoWriter.Fourcc('m', 'p', '4', 'v'), _videoCapture.Get(CapProp.Fps), new Size(_frameWidth, _frameHeight), true);
        
            _frameCaptureTask = Task.Run(() => ReadFramesAsync(_cancellationTokenSource.Token));
        }
        catch (Exception e)
        {
            Console.WriteLine("Error during capture restart:");
            Console.WriteLine(e);
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

    private async Task ReadFramesAsync(CancellationToken cancellationToken)
    {
        try
        {
            do
            {
                Mat frame = new Mat();
                if (!_videoCapture.Read(frame))
                {
                    Console.WriteLine("Could not read frame from device.");
                    _failedReads++;
                    await Task.Delay(500);
                    continue;
                }

                if (frame.IsEmpty)
                {
                    Console.WriteLine("Frame was empty.");
                    _failedReads++;
                    await Task.Delay(500);
                    continue;
                }

                lock (_frameLock)
                {
                    if(_latestFrame != null)
                        _latestFrame.Dispose();
                    
                    _latestFrame = frame.Clone();
                }
                
                lock (_frameLockPreview)
                {
                    if (_latestFramePreview != null && !_latestFramePreview.IsEmpty)
                        _latestFramePreview.Dispose();

                    _latestFramePreview = new Mat();
                    CvInvoke.Resize(_latestFrame, _latestFramePreview, new Size(640, 360));
                }

                _videoWriter?.Write(frame);
                frame.Dispose();

            } while (!cancellationToken.IsCancellationRequested && _failedReads < 5);

            if (_failedReads == 5)
            {
                Console.WriteLine("Stopped capture due to failed reads.");
                _videoWriter.Dispose();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occured while saving a frame:");
            Console.WriteLine(ex.Message);
        }
    }

    public void Dispose()
    {
        Console.WriteLine("Disposing video capture");
        _videoCapture.Stop();
        _cancellationTokenSource.Cancel();

        _frameCaptureTask?.Wait();

        _videoCapture.Release();
        _videoWriter.Dispose();
        _videoCapture.Dispose();
        _cancellationTokenSource.Dispose();
    }
}