using System.Net;
using System.Net.Sockets;

namespace AutoTf.CentralBridgeOS.Services;

public class UdpProxyService
{
	private readonly UdpClient _ffmpegInput;
	private readonly List<IPEndPoint> _clients = new List<IPEndPoint>();
	private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();

	public UdpProxyService()
	{
		Statics.ShutdownEvent += _cancelToken.Cancel;
		
		_ffmpegInput = new UdpClient(5000);
		
		Task.Run(ForwardPackets, _cancelToken.Token);
	}

	public void AddClient(IPEndPoint clientEndpoint)
	{
		if (!_clients.Contains(clientEndpoint))
		{
			_clients.Add(clientEndpoint);
		}
	}

	public void RemoveClient(IPEndPoint clientEndpoint)
	{
		_clients.Remove(clientEndpoint);
	}

	private async Task ForwardPackets()
	{
		while (true)
		{
			try
			{
				UdpReceiveResult received = await _ffmpegInput.ReceiveAsync();

				foreach (IPEndPoint client in _clients)
				{
					using UdpClient udpClient = new UdpClient();
					await udpClient.SendAsync(received.Buffer, received.Buffer.Length, client);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error forwarding packets: {ex.Message}");
			}
		}
	}
}