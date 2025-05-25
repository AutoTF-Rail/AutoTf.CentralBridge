using System.ComponentModel.DataAnnotations;
using System.Net;
using AutoTf.CentralBridge.Extensions;
using AutoTf.CentralBridge.Services.Camera;
using AutoTf.CentralBridge.Sync;
using AutoTf.Logging;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridge.Server.Controllers;

[ApiController]
[Route("/camera")]
public class CameraController : ControllerBase
{
	private readonly Logger _logger;
	private readonly SyncManager _syncManager;
	private readonly MainCameraProxyService _mainCameraProxy;
	
	public CameraController(Logger logger, SyncManager syncManager, MainCameraProxyService mainCameraProxy)
	{
		_logger = logger;
		_syncManager = syncManager;
		_mainCameraProxy = mainCameraProxy;
	}
	
	[Catch]
	[HttpPost("startStream")]
	public IActionResult StartStream([FromQuery, Required] int port)
	{
		IPAddress? ipAddress = HttpContext.Connection.RemoteIpAddress;
		
		if (ipAddress != null)
		{
			IPEndPoint receiverEndpoint = new IPEndPoint(ipAddress, port);
			_mainCameraProxy.AddClient(receiverEndpoint);
            
			_logger.Log($"Added receiver: {receiverEndpoint}");
			return Ok("Receiver added successfully.");
		}
		
		return BadRequest("Could not retrieve client IP address.");
	}
	
	[Catch]
	[HttpPost("stopStream")]
	public IActionResult StopStream()
	{
		IPAddress? ipAddress = HttpContext.Connection.RemoteIpAddress;
		
		if (ipAddress != null)
		{
			_mainCameraProxy.RemoveClient(ipAddress);
			
			_logger.Log($"Removed receiver: {ipAddress}");
			return Ok("Receiver removed successfully.");
		}
		
		return BadRequest("Could not retrieve client IP address.");
	}

	[HttpGet("nextSave")]
	public ActionResult<DateTime> NextSave()
	{
		return _syncManager.NextInterval();
	}
}