using System.Diagnostics;
using AutoTf.CentralBridge.Models;
using AutoTf.CentralBridge.Models.CameraService;
using AutoTf.CentralBridge.Models.DataModels;
using AutoTf.CentralBridge.Models.Interfaces;
using AutoTf.CentralBridge.Models.Static;
using AutoTf.Logging;
using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridge.CameraService;

public class CameraManager : IHostedService
{
	private readonly Logger _logger;
	private readonly IProxyManager _proxy;
	private readonly ITrainModel _train;

	// Used for the port of the displays, the first display is 4001, the second 4002, etc.
	private int _nextDisplayIndex = 1;
	
	private readonly List<KeyValuePair<VideoDevice, Process>> _ffmpegProcesses = new List<KeyValuePair<VideoDevice, Process>>();

	public CameraManager(Logger logger, IProxyManager proxy, ITrainModel train)
	{
		_logger = logger;
		_proxy = proxy;
		_train = train;

		Directory.CreateDirectory("recordings");
	}
	
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		string command = "v4l2-ctl --list-devices";
		string output = CommandExecuter.ExecuteCommand(command);

		List<VideoDevice> videoDevices = DeviceParser.ParseVideoDevices(output);
		
		foreach (VideoDevice videoDevice in videoDevices)
		{
			_logger.Log($"Found camera {videoDevice.Name} at {videoDevice.Path}.");
			await StartStream(videoDevice);
		}

		Statics.AreCamerasStarted = true;
		_logger.Log("Started up all cameras");
	}
	
	private async Task StartStream(VideoDevice videoDevice)
	{
		// Default config for main camera:
		int port = 4000;
		bool record = true;
		int framerate = 15;
		int frameWidth = 1280;
		int frameHeight = 720;

		if (videoDevice.Type == DeviceType.Display)
		{
			port = 4000 + _nextDisplayIndex;
			_nextDisplayIndex++;
			record = false;
			framerate = 10;
			frameHeight = 720;
		}

		_logger.Log($"Starting stream for camera at {videoDevice.Path}: Port {port} Record: {record} Resolution: {frameWidth}x{frameHeight}:{framerate} ");
		await _proxy.CreateProxy(port, videoDevice.Type == DeviceType.Display, _logger, _train);
		await Task.Delay(25);
		_ffmpegProcesses.Add(new KeyValuePair<VideoDevice, Process>(videoDevice, StartFfmpegProcess(videoDevice.Path, framerate, frameWidth, frameHeight, port, record)));
	}

	private Process StartFfmpegProcess(string devicePath, int framerate, int frameWidth, int frameHeight, int port, bool record)
	{
		string format = record ? "yuyv422" : "mjpeg";
		
		string ffmpegArgs = $"-f v4l2 -framerate {framerate} -video_size {frameWidth}x{frameHeight} -loglevel error -input_format {format} -i {devicePath} -y -map 0:v -c:v mjpeg -pix_fmt yuvj420p ";
		
		if (record)
			ffmpegArgs += $"-rtbufsize 3000k -probesize 32 -analyzeduration 0 -flush_packets 1 -f tee \"[f=segment:segment_time=150:reset_timestamps=1:strftime=1]recordings/%Y-%m-%d_%H:%M:%S.mp4|[f=mjpeg]udp://127.0.0.1:{port}\"";
		else
			ffmpegArgs += $"-f mjpeg udp://127.0.0.1:{port}";
		
		Process process = new Process
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
		
		process.ErrorDataReceived += (_, e) =>
		{
			if (e.Data != null)
				_logger.Log($"[{port}] FFMPEG Error: {e.Data}");
		};

		process.Start();
		process.BeginErrorReadLine();

		return process;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_proxy.Dispose();
		_ffmpegProcesses.ForEach(x =>
		{
			x.Value.Kill();
			x.Value.Dispose();
		});

		return Task.CompletedTask;
	}
}