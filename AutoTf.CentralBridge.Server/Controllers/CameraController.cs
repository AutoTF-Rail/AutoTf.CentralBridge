using System.ComponentModel.DataAnnotations;
using System.Net;
using AutoTf.CentralBridge.Services.Camera;
using AutoTf.CentralBridge.Sync;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridge.Server.Controllers;

[ApiController]
[Route("/camera")]
public class CameraController : ControllerBase
{
	private readonly ILogger<CameraController> _logger;
	private readonly SyncManager _syncManager;
	private readonly MainCameraProxyService _mainCameraProxy;
	
	public CameraController(ILogger<CameraController> logger, SyncManager syncManager, MainCameraProxyService mainCameraProxy)
	{
		_logger = logger;
		_syncManager = syncManager;
		_mainCameraProxy = mainCameraProxy;
	}
	
	[HttpPost("startStream")]
	public IActionResult StartStream([FromQuery, Required] int port)
	{
		try
		{
			IPAddress? ipAddress = HttpContext.Connection.RemoteIpAddress;
			
			if (ipAddress != null)
			{
				IPEndPoint receiverEndpoint = new IPEndPoint(ipAddress, port);
				_mainCameraProxy.AddClient(receiverEndpoint);
                
				_logger.LogTrace($"Added receiver: {receiverEndpoint}");
				return Ok("Receiver added successfully.");
			}
			
			return BadRequest("Could not retrieve client IP address.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in startStream.");
			return BadRequest("Failed to add receiver.");
		}
	}
	
	[HttpPost("stopStream")]
	public IActionResult StopStream()
	{
		try
		{
			IPAddress? ipAddress = HttpContext.Connection.RemoteIpAddress;
			
			if (ipAddress != null)
			{
				_mainCameraProxy.RemoveClient(ipAddress);
				
				_logger.LogTrace($"Removed receiver: {ipAddress}");
				return Ok("Receiver removed successfully.");
			}
			
			return BadRequest("Could not retrieve client IP address.");
		}
		catch (Exception ex)
		{
			_logger.LogTrace(ex, "Error in stopStream.");
			return BadRequest("Failed to remove receiver.");
		}
	}

	[HttpGet("nextSave")]
	public ActionResult<DateTime> NextSave()
	{
		return _syncManager.NextInterval();
	}
}