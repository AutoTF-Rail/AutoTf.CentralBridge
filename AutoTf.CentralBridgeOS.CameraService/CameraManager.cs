using System.Diagnostics;
using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Models.CameraService;
using AutoTf.Logging;
using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridgeOS.CameraService;

public class CameraManager : IHostedService
{
	private readonly Logger _logger;
	private readonly ProxyManager _proxy;

	// Used for the port of the displays, the first display is 4001, the second 4002, etc.
	private int _nextDisplayIndex = 1;
	
	private List<KeyValuePair<VideoDevice, Process>> _ffmpegProcesses = new List<KeyValuePair<VideoDevice, Process>>();

	public CameraManager(Logger logger, ProxyManager proxy)
	{
		_logger = logger;
		_proxy = proxy;

		Directory.CreateDirectory("recordings");
	}
	
	public Task StartAsync(CancellationToken cancellationToken)
	{
		string command = "v4l2-ctl --list-devices";
		string output = CommandExecuter.ExecuteCommand(command);

		List<VideoDevice> videoDevices = CommandParser.ParseVideoDevices(output);
		
		foreach (VideoDevice videoDevice in videoDevices)
		{
			_logger.Log($"Found camera {videoDevice.Name} at {videoDevice.Path}.");
			StartStream(videoDevice);
		}

		Statics.AreCamerasStarted = true;
		_logger.Log("CM: Started up all cameras");
		
		return Task.CompletedTask;
	}
	
	private void StartStream(VideoDevice videoDevice)
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
			// TODO: Record the displays too? e.g. for Logging
			record = false;
			framerate = 10;
			frameHeight = 800;
		}

		_logger.Log($"CM: Starting stream for camera at {videoDevice.Path}: Port {port} Record: {record} Resolution: {frameWidth}x{frameHeight}:{framerate} ");
		_proxy.CreateProxy(port, videoDevice.Type == DeviceType.Display, _logger);
		_ffmpegProcesses.Add(new KeyValuePair<VideoDevice, Process>(videoDevice, StartFfmpegProcess(videoDevice.Path, framerate, frameWidth, frameHeight, port, record)));
	}

	private Process StartFfmpegProcess(string devicePath, int framerate, int frameWidth, int frameHeight, int port, bool record)
	{
		// TODO: Implement logger
		string ffmpegArgs =
			$"-f v4l2 -framerate {framerate} -video_size {frameWidth}x{frameHeight} -input_format yuyv422 " +
			$"-i {devicePath} -map 0:v -loglevel error -c:v mjpeg -pix_fmt yuvj420p -rtbufsize 1500k -preset ultrafast -tune zero_latency -max_delay 0  -flush_packets 1 -g 1 -analyzeduration 1000000 -probesize 32 ";
		
		if (record)
			ffmpegArgs += $"-f tee \"[f=segment:segment_time=150:reset_timestamps=1:strftime=1]recordings/%Y-%m-%d_%H:%M:%S.mp4|[f=mjpeg]udp://127.0.0.1:{port}\"";
		else
			ffmpegArgs +=
				$"-f mjpeg udp://127.0.0.1:{port}";
		
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
		process.EnableRaisingEvents = true;
        
		process.ErrorDataReceived += (sender, e) =>
		{
			if (e.Data != null)
				_logger.Log($"FFmpeg Error: {e.Data}");
		};
		process.Start();
		process.Exited += (sender, args) => _logger.Log("Ffmpeg has exited.");

		return process;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_ffmpegProcesses.ForEach(x =>
		{
			x.Value.Kill();
			x.Value.Dispose();
		});
		_proxy.Dispose();

		return Task.CompletedTask;
	}
}