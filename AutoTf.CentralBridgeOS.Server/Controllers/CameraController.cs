using System.ComponentModel.DataAnnotations;
using System.Net;
using AutoTf.CentralBridgeOS.Extensions;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.CentralBridgeOS.Services.Sync;
using AutoTf.Logging;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridgeOS.Server.Controllers;

// TODO: Secondary endpoint for second camera /other side
[ApiController]
[Route("/camera")]
public class CameraController : ControllerBase
{
	private readonly Logger _logger;
	private readonly SyncManager _syncManager;
	private readonly UdpProxyService _udpProxy;
	
	public CameraController(Logger logger, SyncManager syncManager, UdpProxyService udpProxy)
	{
		_logger = logger;
		_syncManager = syncManager;
		_udpProxy = udpProxy;
	}
	
	[MacAuthorize]
	[HttpPost("startStream")]
	public IActionResult StartStream([FromQuery, Required] int port)
	{
		try
		{
			// TODO: Implement camera index for multiple cameras
			// If another is already running, turn that one off and turn the new one on.
			IPAddress? ipAddress = HttpContext.Connection.RemoteIpAddress;
			
			if (ipAddress != null)
			{
				IPEndPoint receiverEndpoint = new IPEndPoint(ipAddress, port);
				_udpProxy.AddClient(receiverEndpoint);
                
				_logger.Log($"Added receiver: {receiverEndpoint}");
				return Ok("Receiver added successfully.");
			}
			
			return BadRequest("Could not retrieve client IP address.");
		}
		catch (Exception ex)
		{
			_logger.Log("CAM-C: Error in startStream.");
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
				_udpProxy.RemoveClient(ipAddress);
				
				_logger.Log($"Removed receiver: {ipAddress}");
				return Ok("Receiver removed successfully.");
			}
			
			return BadRequest("Could not retrieve client IP address.");
		}
		catch (Exception ex)
		{
			_logger.Log("CAM-C: Error in stopStream.");
			_logger.Log(ex.ToString());
			return BadRequest("Failed to remove receiver.");
		}
	}

	[MacAuthorize]
	[HttpGet("nextSave")]
	public IActionResult NextSave()
	{
		try
		{
			return Content(_syncManager.NextInterval().ToString("o"));
		}
		catch (Exception e)
		{
			_logger.Log("CAM-C: Could not supply next save:");
			_logger.Log(e.Message);
			return BadRequest("Could not supply next save.");
		}
	}
}