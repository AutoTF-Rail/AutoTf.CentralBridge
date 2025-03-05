using System.Net;
using System.Net.Sockets;
using AutoTf.CentralBridgeOS.Models;
using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridgeOS.Services;

public class UdpProxyService : IHostedService
{
	private readonly TrainSessionService _trainSessionService;
	
	private readonly UdpClient _ffmpegInput;
	private readonly UdpClient _secondaryCamInput;
	
	private readonly List<IPEndPoint> _clients = new List<IPEndPoint>();
	private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();
	
	private readonly List<byte> _buffer = new List<byte>();
	private readonly List<byte> _secondaryBuffer = new List<byte>();

	private readonly byte[] _jpegFrameStart = [0xFF, 0xD8];
	private readonly byte[] _jpegFrameEnd = [0xFF, 0xD9];

	private readonly IPEndPoint _masterBridgeIp;
	private readonly IPEndPoint _slaveBridgeIp;
	
	private bool _canStream = true;
	
	public UdpProxyService(TrainSessionService trainSessionService)
	{
		_trainSessionService = trainSessionService;
		
		_ffmpegInput = new UdpClient(5000);
		_secondaryCamInput = new UdpClient(5001);
		
		// If we are the slave, we want to forward our own camera as the secondary to the master.
		_masterBridgeIp = new IPEndPoint(IPAddress.Parse("192.168.0.1"), 5001);
		// If we are master, we want to forward our own camera as the main cam to the slave.
		_slaveBridgeIp = new IPEndPoint(IPAddress.Parse("192.168.0.2"), 5000);
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		Task.Run(ForwardPacketsMain, _cancelToken.Token);
		Task.Run(ForwardPacketsSecondary, _cancelToken.Token);
		
		return Task.CompletedTask;
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

	private async Task ForwardPacketsMain()
	{
		while (_canStream)
		{
			UdpReceiveResult received = await _ffmpegInput.ReceiveAsync();
			byte[] receivedData = received.Buffer;

			_buffer.AddRange(receivedData);

			using UdpClient udpClient = new UdpClient();
			while (_buffer.Count > 0)
			{
				int startIndex = IndexOfSequence(_buffer, _jpegFrameStart);
				int endIndex = IndexOfSequence(_buffer, _jpegFrameEnd);

				if (startIndex >= 0 && endIndex > startIndex)
				{
					byte[] frameBytes = _buffer.GetRange(startIndex, endIndex - startIndex + 2).ToArray();

					foreach (IPEndPoint client in _clients)
					{
						await udpClient.SendAsync(frameBytes, frameBytes.Length, client);
					}

					// if master, we want to send our feed to the slave.
					if (_trainSessionService.LocalServiceState == BridgeServiceState.Master)
						await udpClient.SendAsync(frameBytes, frameBytes.Length, _slaveBridgeIp);

					_buffer.RemoveRange(0, endIndex + 2); 
				}
				else
				{
					break;
				}
			}
		}
	}

	private async Task ForwardPacketsSecondary()
	{
		while (_canStream)
		{
			UdpReceiveResult received = await _secondaryCamInput.ReceiveAsync();
			byte[] receivedData = received.Buffer;

			_secondaryBuffer.AddRange(receivedData);

			using UdpClient udpClient = new UdpClient();
			while (_secondaryBuffer.Count > 0)
			{
				int startIndex = IndexOfSequence(_secondaryBuffer, _jpegFrameStart);
				int endIndex = IndexOfSequence(_secondaryBuffer, _jpegFrameEnd);

				if (startIndex >= 0 && endIndex > startIndex)
				{
					byte[] frameBytes = _secondaryBuffer.GetRange(startIndex, endIndex - startIndex + 2).ToArray();

					foreach (IPEndPoint client in _clients)
					{
						client.Port += 1;
						await udpClient.SendAsync(frameBytes, frameBytes.Length, client);
					}
					
					// if slave, we want to send our feed to the msater.
					if (_trainSessionService.LocalServiceState == BridgeServiceState.Slave)
						await udpClient.SendAsync(frameBytes, frameBytes.Length, _masterBridgeIp);

					_secondaryBuffer.RemoveRange(0, endIndex + 2); 
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

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_canStream = false;
		_cancelToken.Cancel();
		
		return Task.CompletedTask;
	}
}