using System.Net;
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
	private const int UdpPort = 12345;
	
	public CameraController(Logger logger, SyncManager syncManager, UdpProxyService udpProxy)
	{
		_logger = logger;
		_syncManager = syncManager;
		_udpProxy = udpProxy;
	}
	
	[HttpPost("startStream")]
	public IActionResult StartStream()
	{
		try
		{
			IPAddress? ipAddress = HttpContext.Connection.RemoteIpAddress;
			
			if (ipAddress != null)
			{
				IPEndPoint receiverEndpoint = new IPEndPoint(ipAddress, UdpPort);
				_udpProxy.AddClient(receiverEndpoint);
                
				_logger.Log($"Added receiver: {receiverEndpoint}");
				return Ok("Receiver added successfully.");
			}
			
			return BadRequest("Could not retrieve client IP address.");
		}
		catch (Exception ex)
		{
			_logger.Log("CAM-C: Error in startStream.");
			_logger.Log(ex.Message);
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
				IPEndPoint receiverEndpoint = new IPEndPoint(ipAddress, UdpPort);
				_udpProxy.RemoveClient(receiverEndpoint);
				
				_logger.Log($"Removed receiver: {receiverEndpoint}");
				return Ok("Receiver removed successfully.");
			}
			
			return BadRequest("Could not retrieve client IP address.");
		}
		catch (Exception ex)
		{
			_logger.Log("CAM-C: Error in stopStream.");
			_logger.Log(ex.Message);
			return BadRequest("Failed to remove receiver.");
		}
	}

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