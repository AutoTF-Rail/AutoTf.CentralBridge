using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using AutoTf.CentralBridgeOS.FahrplanParser;
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

	private bool _isDisplay;
	private readonly Logger _logger;

	internal bool _canStream = true;
	
	private readonly UdpClient _input;

	private Size _frameSize;

	private bool isFirstLoop = true;

	public CameraProxy(int port, bool isDisplay, Logger logger)
	{
		_isDisplay = isDisplay;
		_logger = logger;
		_input = new UdpClient(port);

		// TODO: Get this from the proxy manager
		if (_isDisplay)
			_frameSize = new Size(1280, 800);
		else
			_frameSize = new Size(1280, 720);
		
		Task.Run(StartListening);
	}

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
			while (_canStream)
			{
				UdpReceiveResult received = await _input.ReceiveAsync();
				byte[] receivedData = received.Buffer;

				_buffer.AddRange(receivedData);

				using UdpClient udpClient = new UdpClient();
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
							// Convert to mat, crop offset, turn back into bytes
							Console.WriteLine("Test");
							Mat mat = ConvertYuvToMat(frameBytes);
							// TODO: Enter the actual ROI here:
							Mat cropped = new Mat(mat,
								new Rectangle(new Point(100, 0), new Size(mat.Width - 200, mat.Height)));
							
							if (isFirstLoop && DisplayType == DisplayType.Unknown)
							{
								isFirstLoop = false;
								ReadDisplayType(cropped);
							}
							
							mat.Dispose();
							frameBytes = CvInvoke.Imencode(".jpg", cropped);
							cropped.Dispose();
						}

						foreach (IPEndPoint client in _clients)
						{
							await udpClient.SendAsync(frameBytes, frameBytes.Length, client);
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
			// TODO: Handle
		}
	}
        
	private Mat ConvertYuvToMat(byte[] yuvData)
	{
		Mat frame = new Mat();
		CvInvoke.Imdecode(yuvData, ImreadModes.Color, frame);

		return frame;
	}

	private void ReadDisplayType(Mat frame)
	{
		using Tesseract engine = new Tesseract(Path.Combine(AppContext.BaseDirectory, "tessdata"), "deu", OcrEngineMode.LstmOnly);
		string date = new Parser(engine).Date(frame);

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
		_canStream = false;
		_input.Dispose();
	}
}