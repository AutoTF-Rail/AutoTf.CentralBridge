using System.ComponentModel.DataAnnotations;
using System.Net;
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
                
				_logger.Log($"Added receiver: {receiverEndpoint}");
				return Ok("Receiver added successfully.");
			}
			
			return BadRequest("Could not retrieve client IP address.");
		}
		catch (Exception ex)
		{
			_logger.Log("Error in startStream.");
			_logger.Log(ex.ToString());
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
				
				_logger.Log($"Removed receiver: {ipAddress}");
				return Ok("Receiver removed successfully.");
			}
			
			return BadRequest("Could not retrieve client IP address.");
		}
		catch (Exception ex)
		{
			_logger.Log("Error in stopStream.");
			_logger.Log(ex.ToString());
			return BadRequest("Failed to remove receiver.");
		}
	}

	[HttpGet("nextSave")]
	public ActionResult<DateTime> NextSave()
	{
		try
		{
			return _syncManager.NextInterval();
		}
		catch (Exception e)
		{
			_logger.Log("Could not supply next save:");
			_logger.Log(e.ToString());
			return BadRequest("Could not supply next save.");
		}
	}
}