using System.Net;
using AutoTf.CentralBridge.Models;
using AutoTf.CentralBridge.Models.CameraService;
using AutoTf.CentralBridge.Models.DataModels;
using AutoTf.CentralBridge.Models.Interfaces;
using AutoTf.CentralBridge.Shared.Models.Enums;
using AutoTf.Logging;
using Emgu.CV;

namespace AutoTf.CentralBridge.CameraService;

// This class doesn't need to be a HostedService, because its dispose is being called by the CameraManager
public class ProxyManager : IProxyManager
{
	private readonly List<CameraProxy> _displays = new List<CameraProxy>();

	private CameraProxy? _mainCamera;

	public async Task CreateProxy(int port, bool isDisplay, Logger logger, ITrainModel train)
	{
		CameraProxy proxy = new CameraProxy(port, isDisplay, logger, train);
		
		if (isDisplay)
			_displays.Add(proxy);
		else
			_mainCamera = proxy;

		await proxy.WaitUntilStarted();
	}

	public void StartListeningForDisplay(DisplayType type, IPEndPoint endpoint)
	{
		_displays.FirstOrDefault(x => x.DisplayType == type)?.AddClient(endpoint);
	}

	public void StopListeningForDisplay(DisplayType type, IPAddress address)
	{
		_displays.FirstOrDefault(x => x.DisplayType == type)?.RemoveClient(address);
	}

	public bool IsDisplayRegistered(DisplayType type)
	{
		return _displays.Any(x => x.DisplayType == type);
	}

	public List<KeyValuePair<DisplayType, bool>> DisplaysStatus()
	{
		return _displays.Select(x => new KeyValuePair<DisplayType, bool>(x.DisplayType, x.IsRunning)).ToList();
	}

	public bool? MainCameraStatus()
	{
		return _mainCamera?.IsRunning;
	}

	public bool IsCameraAvailable()
	{
		return _mainCamera != null;
	}

	public void StartListeningForCamera(IPEndPoint endPoint)
	{
		_mainCamera?.AddClient(endPoint);
	}

	public Mat GetLatestFrameFromDisplay(DisplayType type)
	{
		// When someone gets this frame, they should be sure that the display is actually available.
		return _displays.FirstOrDefault(x => x.DisplayType == type)?.GetFrame()!;
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