using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using AutoTf.CentralBridgeOS.FahrplanParser;
using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Models.CameraService;
using AutoTf.Logging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;

namespace AutoTf.CentralBridgeOS.CameraService;

internal class CameraProxy : IDisposable
{
	private readonly byte[] _jpegFrameStart = [0xFF, 0xD8];
	private readonly byte[] _jpegFrameEnd = [0xFF, 0xD9];
	
	private readonly List<byte> _buffer = new List<byte>();
	
	private readonly List<IPEndPoint> _clients = new List<IPEndPoint>();

	public DisplayType DisplayType = DisplayType.Unknown;

	private readonly int _port;
	private bool _isDisplay;
	private readonly Logger _logger;
	private readonly ITrainModel _train;

	internal bool CanStream = true;
	private bool _isFirstLoop = true;
	
	private readonly UdpClient _input;
	private readonly UdpClient _output = new UdpClient();

	private readonly TaskCompletionSource _started = new();

	private Mat? _latestMat = null;

	public CameraProxy(int port, bool isDisplay, Logger logger, ITrainModel train)
	{
		_port = port;
		_isDisplay = isDisplay;
		_logger = logger;
		_train = train;
		_input = new UdpClient(port);
		
		Task.Run(StartListening);
	}

	public Mat? GetFrame()
	{
		return _latestMat?.Clone();
	}
	
	public Task WaitUntilStarted() => _started.Task;
	
	internal void AddClient(IPEndPoint clientEndpoint)
	{
		if (!_clients.Contains(clientEndpoint))
		{
			_clients.Add(clientEndpoint);
		}
	}

	internal void RemoveClient(IPAddress clientIp)
	{
		_clients.RemoveAll(x => Equals(x.Address, clientIp));
	}

	private async Task StartListening()
	{
		try
		{
			_started.SetResult();
			while (CanStream)
			{
				UdpReceiveResult received = await _input.ReceiveAsync();
				byte[] receivedData = received.Buffer;

				_buffer.AddRange(receivedData);
				
				if (_buffer.Count > 10_000_000) // 10MB
				{
					_logger.Log("CP: Warning: Clearing oversized buffer");
					_buffer.Clear();
				}
				
				// For testing with VLC:
				// udpClient.Client.SendBufferSize = 1316;
				while (_buffer.Count > 0)
				{
					int startIndex = IndexOfSequence(_buffer, _jpegFrameStart);
					int endIndex = IndexOfSequence(_buffer, _jpegFrameEnd);

					if (startIndex >= 0 && endIndex > startIndex)
					{
						byte[] frameBytes = _buffer.GetRange(startIndex, endIndex - startIndex + 2).ToArray();

						// It's not worth it to convert it here if it's not a display (TODO: Maybe we can offload display reading too to the other pc?), because we give the tablet the raw output too, and the PC that runs the segmentation can handle the converssion itself.
						if (_isDisplay)
						{
							using Mat? mat = ConvertYuvToMat(frameBytes);
							if(mat == null)
								continue;
							
							_latestMat?.Dispose();
							_latestMat = new Mat(mat, new Rectangle(new Point(80, 0), new Size(mat.Width - (80 + 118), mat.Height)));
							
							if (_isFirstLoop && DisplayType == DisplayType.Unknown)
							{
								_isFirstLoop = false;
								ReadDisplayType(_latestMat);
							}
							
							frameBytes = CvInvoke.Imencode(".jpg", _latestMat);
						}

						foreach (IPEndPoint client in _clients)
						{
							await _output.SendAsync(frameBytes, frameBytes.Length, client);
						}

						_buffer.RemoveRange(0, endIndex + 2); 
					}
					else
					{
						break;
					}
				}
			}
		}
		catch (Exception e)
		{
			// We only want to handle exceptions, if the proxy wasn't disposed.
			if (CanStream)
			{
				// TODO: Tell parent that it's unavailable? So other services can't use this anymore?
				_logger.Log($"CP: Failed while listening for a camera on port {_port}:");
				_logger.Log(e.ToString());
			}
		}
	}
	
	private Mat? ConvertYuvToMat(byte[] yuvData)
	{
		try
		{
			using Mat temp = new Mat();
			
			CvInvoke.Imdecode(yuvData, ImreadModes.Color, temp);
			return temp.Clone();
		}
		catch (Exception ex)
		{
			_logger.Log("Failed to decode JPEG frame: " + ex.Message);
			return null;
		}
	}

	private void ReadDisplayType(Mat frame)
	{
		using Tesseract engine = new Tesseract(Path.Combine(AppContext.BaseDirectory, "tessdata"), "deu", OcrEngineMode.LstmOnly);
		string date = new Parser(engine, _train).Date(frame);
		
		// This doesn't work if we use a example Fahrplan picture, but IRL this would work
		// DisplayType = date.Trim().Contains(DateTime.Now.Year.ToString()) ? DisplayType.EbuLa : DisplayType.CCD;
		DisplayType = Regex.IsMatch(date.Trim(), @"^[0-9.]+$") ? DisplayType.EbuLa : DisplayType.CCD;
		_logger.Log($"Registered display camera as {DisplayType.ToString()}.");
	}

	private int IndexOfSequence(List<byte> source, byte[] sequence)
	{
		int maxFirstIndex = source.Count - sequence.Length + 1;
		for (int i = 0; i < maxFirstIndex; i++)
		{
			bool isMatch = true;
			for (int j = 0; j < sequence.Length; j++)
			{
				if (source[i + j] != sequence[j])
				{
					isMatch = false;
					break;
				}
			}
			if (isMatch) return i;
		}
		return -1;
	}

	public void Dispose()
	{
		CanStream = false;
		_input.Dispose();
		_output.Dispose();
	}
}