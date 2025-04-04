using System.Net;
using AutoTf.CentralBridgeOS.Models.CameraService;
using AutoTf.Logging;
using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridgeOS.CameraService;

// This class doesn't need to be a HostedService, because its dispose is being called by the CameraManager
public class ProxyManager
{
	private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();

	private readonly List<CameraProxy> _displays = new List<CameraProxy>();

	private CameraProxy? _mainCamera;
	
	internal void CreateProxy(int port, bool isDisplay, Logger logger)
	{
		CameraProxy proxy = new CameraProxy(port, isDisplay, logger);
		
		if (isDisplay)
			_displays.Add(proxy);
		else
			_mainCamera = proxy;
	}

	public void StartListeningForDisplay(DisplayType type, IPEndPoint endpoint)
	{
		_displays.FirstOrDefault(x => x.DisplayType == type)?.AddClient(endpoint);
	}

	public void StopListeningForDisplay(DisplayType type, IPAddress address)
	{
		_displays.FirstOrDefault(x => x.DisplayType == type)?.RemoveClient(address);
	}

	public bool IsDisplayAvailable(DisplayType type)
	{
		return _displays.Any(x => x.DisplayType == type);
	}

	public bool IsCameraAvailable()
	{
		return _mainCamera != null;
	}

	public void StartListeningForCamera(IPEndPoint endPoint)
	{
		_mainCamera?.AddClient(endPoint);
	}

	public void StopListeningForCamera(IPAddress address)
	{
		_mainCamera?.RemoveClient(address);
	}

	public void Dispose()
	{
		_mainCamera?.Dispose();
		_displays.ForEach(proxy => proxy.Dispose());
	}
}