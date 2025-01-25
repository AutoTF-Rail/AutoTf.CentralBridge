using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace AutoTf.CentralBridgeOS.Services;

public class CameraService : IDisposable
{
    private Process ffmpegProcess;
    private Stream ffmpegOutputStream;
    private StreamReader ffmpegErrorStream;
    private readonly int _frameWidth;
    private readonly int _frameHeight;
    private readonly int _frameSize;
    private byte[] latestFrameBuffer;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public CameraService(int frameWidth = 1920, int frameHeight = 1080)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        VideoCapture videoCapture = new VideoCapture(0, VideoCapture.API.Any);
        videoCapture.Set(CapProp.FrameWidth, 1920);
        videoCapture.Set(CapProp.FrameHeight, 1080);
        while (true)
        {
            Mat frame = new Mat();
            videoCapture.Read(frame);

            CvInvoke.Imwrite("/home/CentralBridge/meow/" + DateTime.Now.ToString("HH:mm:ss zz") + ".png",
                frame);
            Console.WriteLine("Captured");
            Thread.Sleep(250);
        }
        _frameWidth = frameWidth;
        _frameHeight = frameHeight;

        _frameSize = _frameWidth * _frameHeight * 3 / 2;

        StartCaptureAsync();
    }

    private Task StartCaptureAsync()
    {
        Console.WriteLine("Starting capture");
        // string ffmpegArgs = $"-f v4l2 -input_format mjpeg  -framerate 30 -video_size 1920x1080 -i /dev/video0 -c:v libx264 -pix_fmt yuv420p -preset fast -crf 22 output.mp4 -y";
        string ffmpegArgs = $"-f v4l2 -input_format mjpeg -framerate 30 -video_size 1920x1080 -i /dev/video0 -c:v h264_v4l2m2m -pix_fmt yuv420p -preset fast -crf 22 -f rawvideo pipe:1 -loglevel error";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = ffmpegArgs,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        ffmpegProcess = new Process { StartInfo = startInfo };
        ffmpegProcess.Start();
        ffmpegOutputStream = ffmpegProcess.StandardOutput.BaseStream;
        ffmpegErrorStream = ffmpegProcess.StandardError;

        latestFrameBuffer = new byte[_frameSize];
        Task.Run(ReadFramesAsync);

        return Task.CompletedTask;
    }

    public byte[] GetLatestFrame() => (byte[])latestFrameBuffer.Clone();

    public Mat GetLatestFrameMat() => ConvertYuvToMat((byte[])latestFrameBuffer.Clone());

    private async Task ReadFramesAsync()
    {
        while (true)
        {
            try
            {
                byte[] buffer = new byte[_frameSize];
                int bytesRead = 0;

                while (bytesRead < _frameSize)
                {
                    int read = await ffmpegOutputStream.ReadAsync(buffer, bytesRead, _frameSize - bytesRead);
                    if (read == 0)
                    {
                        string errorOutput = await ffmpegErrorStream.ReadToEndAsync();
                        throw new EndOfStreamException($"End of stream reached before reading a full frame. FFmpeg error output: {errorOutput}");
                    }
                    bytesRead += read;
                }

                Array.Copy(buffer, latestFrameBuffer, _frameSize);
                CvInvoke.Imwrite("/home/CentralBridge/meow/" + Statics.GenerateRandomString() + ".png",
                    GetLatestFrameMat());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading frame: " + ex.Message);
                throw;
            }
        }
    }

    private Mat ConvertYuvToMat(byte[] yuvData)
    {
        Mat yuvMat = new Mat(_frameHeight + _frameHeight / 2, _frameWidth, DepthType.Cv8U, 1);
        yuvMat.SetTo(yuvData);

        CvInvoke.CvtColor(yuvMat, yuvMat, ColorConversion.Yuv2BgrI420);

        return yuvMat;
    }

    public void Dispose()
    {
        ffmpegProcess?.Kill();
        ffmpegProcess?.Dispose();
        ffmpegOutputStream?.Dispose();
        ffmpegErrorStream?.Dispose();
    }
}