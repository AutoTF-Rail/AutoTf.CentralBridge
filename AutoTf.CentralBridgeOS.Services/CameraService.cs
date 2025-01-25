using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace AutoTf.CentralBridgeOS.Services;

public class CameraService : IDisposable
{
    private readonly VideoCapture _videoCapture = new VideoCapture(0, VideoCapture.API.V4L2);
    private readonly VideoWriter _videoWriter;
    private Mat _latestFrame = null!;
    private readonly object _frameLock = new object();
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private Task? _frameCaptureTask;

    public CameraService(int frameWidth = 1920, int frameHeight = 1080)
    {
        Statics.ShutdownEvent += Dispose;
        _videoCapture.Set(CapProp.FrameWidth, frameWidth);
        _videoCapture.Set(CapProp.FrameHeight, frameHeight);
        _videoCapture.Set(CapProp.FourCC, VideoWriter.Fourcc('M', 'J', 'P', 'G'));
        _videoCapture.Set(CapProp.Fps, 30);
        Console.WriteLine("Starting capture at " + _videoCapture.Get(CapProp.Fps) + " fps.");

        Directory.CreateDirectory("recordings");
        _videoWriter = new VideoWriter("recordings/" + DateTime.Now.ToString("dd.MM.yyyy-HH:mm:ss") + ".mp4",
            VideoWriter.Fourcc('m', 'p', '4', 'v'), _videoCapture.Get(CapProp.Fps), new Size(frameWidth, frameHeight), true);
        
        _frameCaptureTask = Task.Run(() => ReadFramesAsync(_cancellationTokenSource.Token));
    }
    
    public Mat LatestFrame
    {
        get
        {
            lock (_frameLock)
            {
                return _latestFrame.Clone();
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
                    await Task.Delay(50);
                    continue;
                }

                if (frame.IsEmpty)
                {
                    Console.WriteLine("Frame was empty.");
                    await Task.Delay(50);
                    continue;
                }

                lock (_frameLock)
                {
                    if(_latestFrame != null)
                        _latestFrame.Dispose();
                    
                    _latestFrame = frame.Clone();
                }

                _videoWriter.Write(frame);
                frame.Dispose();

            } while (!cancellationToken.IsCancellationRequested);
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