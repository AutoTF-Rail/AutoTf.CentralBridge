using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace AutoTf.CentralBridgeOS.Services;

public class CameraService : IDisposable
{
    private readonly VideoCapture _videoCapture = new VideoCapture();
    private readonly VideoWriter _videoWriter;
    private Mat _latestFrame = null!;
    private readonly object _frameLock = new object();

    public CameraService(int frameWidth = 1920, int frameHeight = 1080)
    {
        _videoCapture.Set(CapProp.FrameWidth, frameWidth);
        _videoCapture.Set(CapProp.FrameHeight, frameHeight);

        Directory.CreateDirectory("recordings");
        _videoWriter = new VideoWriter("recordings/" + DateTime.Now.ToString("dd.MM.yyyy-HH:mm:ss") + ".mp4", 30,
            new Size(frameWidth, frameHeight), true);
        Task.Run(ReadFramesAsync);
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

    private void ReadFramesAsync()
    {
        while (true)
        {
            Mat frame = new Mat();
            _videoCapture.Read(frame);
            
            if (frame.IsEmpty)
            {
                Thread.Sleep(50);
                continue;
            }
            
            lock (_frameLock)
            {
                _latestFrame = frame.Clone();
            }
            
            _videoWriter.Write(frame);
            
            Thread.Sleep(50);
        }
    }

    public void Dispose()
    {
        _videoWriter.Dispose();
        _videoCapture.Dispose();
    }
}