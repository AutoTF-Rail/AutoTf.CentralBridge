using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTf.CentralBridgeOS.Services;

public class UdpProxyService
{
	private readonly UdpClient _ffmpegInput;
	private readonly List<IPEndPoint> _clients = new List<IPEndPoint>();
	private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();
	private List<byte> _buffer = new List<byte>();

	private readonly byte[] jpegFrameStart = new byte[] { 0xFF, 0xD8 };
	private readonly byte[] jpegFrameEnd = new byte[] { 0xFF, 0xD9 };
	private bool _canStream = true;
	
	public UdpProxyService()
	{
		Statics.ShutdownEvent += _cancelToken.Cancel;
		
		_ffmpegInput = new UdpClient(5000);
		
		Statics.ShutdownEvent += () =>
		{
			_canStream = false;
		};
		
		Task.Run(ForwardPackets, _cancelToken.Token);
	}

	public void AddClient(IPEndPoint clientEndpoint)
	{
		if (!_clients.Contains(clientEndpoint))
		{
			_clients.Add(clientEndpoint);
		}
	}

	public void RemoveClient(IPAddress clientIp)
	{
		_clients.RemoveAll(x => Equals(x.Address, clientIp));
	}

	private async Task ForwardPackets()
	{
		while (_canStream)
		{
			UdpReceiveResult received = await _ffmpegInput.ReceiveAsync();
			byte[] receivedData = received.Buffer;

			_buffer.AddRange(receivedData);

			using UdpClient udpClient = new UdpClient();
			while (_buffer.Count > 0)
			{
				int startIndex = IndexOfSequence(_buffer, jpegFrameStart);
				int endIndex = IndexOfSequence(_buffer, jpegFrameEnd);

				if (startIndex >= 0 && endIndex > startIndex)
				{
					byte[] frameBytes = _buffer.GetRange(startIndex, endIndex - startIndex + 2).ToArray();

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
}