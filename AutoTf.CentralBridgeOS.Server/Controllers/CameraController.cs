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
	private readonly CameraService _cameraService;
	private readonly Logger _logger;
	private readonly SyncManager _syncManager;
	private readonly UdpProxyService _udpProxy;
	private const int UdpPort = 12345;

	private bool _canStream = true;
	
	public CameraController(CameraService cameraService, Logger logger, SyncManager syncManager, UdpProxyService udpProxy)
	{
		_cameraService = cameraService;
		_logger = logger;
		_syncManager = syncManager;
		_udpProxy = udpProxy;

		Statics.ShutdownEvent += () =>
		{
			_canStream = false;
		};
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

	[HttpGet("latestFramePreview")]
	public IActionResult LatestFramePreview()
	{
		try
		{
			if (!Request.Headers.IsAllowedDevice())
				return Unauthorized();
			
			byte[]? imageBytes = _cameraService.LatestFrame;

			if (imageBytes == null)
				return BadRequest();
			
			return File(imageBytes, "image/png");
		}
		catch (Exception e)
		{
			_logger.Log("CAM-C: Failed to supply preview frame:");
			_logger.Log(e.Message);
			return BadRequest(e.Message);
		}
	}

	[HttpGet("latestFrame")]
	public IActionResult LatestFrame()
	{
		try
		{
			if (!Request.Headers.IsAllowedDevice())
				return Unauthorized();

			byte[]? imageBytes = _cameraService.LatestFrame;

			if (imageBytes == null)
				return BadRequest();
			
			return File(imageBytes, "image/png");
		}
		catch (Exception e)
		{
			_logger.Log("CAM-C: Failed to supply frame:");
			_logger.Log(e.Message);
			return BadRequest(e.Message);
		}
	}
}