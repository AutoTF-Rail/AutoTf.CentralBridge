using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using AutoTf.CentralBridgeOS.Extensions;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.CentralBridgeOS.Services.Sync;
using AutoTf.Logging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
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
	private readonly UdpClient _udpClient;
	private readonly List<IPEndPoint> _clients;

	public CameraController(CameraService cameraService, Logger logger, SyncManager syncManager)
	{
		_cameraService = cameraService;
		_logger = logger;
		_syncManager = syncManager;
		_udpClient = new UdpClient(5000);
		_clients = new List<IPEndPoint>();
	}
	
	[HttpGet("startStream")]
	public IActionResult StartStream()
	{
		try
		{
			if (!Request.Headers.IsAllowedDevice())
			{
				return Unauthorized();
			}
			
			IPAddress clientAddress = IPAddress.Parse(Request.HttpContext.Connection.RemoteIpAddress!.ToString());
			IPEndPoint clientEndPoint = new IPEndPoint(clientAddress, 5001);

			_clients.Add(clientEndPoint);

			Task.Run(() => SendFramesToClients());

			return Ok("Stream started.");
		}
		catch (Exception ex)
		{
			_logger.Log("CAM-C: Error during UDP stream request.");
			_logger.Log(ex.Message);
			return BadRequest("Could not start the stream.");
		}
	}

	private async Task SendFramesToClients()
	{
		try
		{
			while (true)
			{
				byte[]? frame = _cameraService.LatestFramePreview.Convert(".jpeg");
				
				if (frame != null)
				{
					foreach (IPEndPoint client in _clients)
					{
						await _udpClient.SendAsync(frame, frame.Length, client);
					}
				}

				await Task.Delay(30);
				// TODO: Clean up clients: udpClients.Close()
			}
		}
		catch (Exception ex)
		{
			_logger.Log("CAM-C: Error during UDP frame sending.");
			_logger.Log(ex.Message);
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
	
	
	
	[Route("stream")]
	public async Task GetStream(CancellationToken cancellationToken)
	{
		try
		{
			if (!Request.Headers.IsAllowedDevice())
			{
				Response.StatusCode = 401;
				return;
			}
			
			if (!HttpContext.WebSockets.IsWebSocketRequest)
			{
				Response.StatusCode = 400;
				return;
			}

			WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
			await HandleWebSocketAsync(webSocket, cancellationToken);
		}
		catch (Exception e)
		{
			_logger.Log("CAM-C: Error during WebSocket initialization.");
		}
	}

	[HttpGet("latestFramePreview")]
	public IActionResult LatestFramePreview()
	{
		try
		{
			if (!Request.Headers.IsAllowedDevice())
				return Unauthorized();
			
			Mat? frame = _cameraService.LatestFramePreview;
			
			if (frame == null)
			{
				frame = new Mat(360, 640, DepthType.Cv8U, 3);
				frame.SetTo(new MCvScalar(0, 0, 0));
			}

			byte[] imageBytes = CvInvoke.Imencode(".png", frame);

			frame.Dispose();
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
			
			Mat? frame = _cameraService.LatestFrame;
			
			if (frame == null)
			{
				frame = new Mat(360, 640, DepthType.Cv8U, 3);
				frame.SetTo(new MCvScalar(0, 0, 0));
			}

			byte[] imageBytes = CvInvoke.Imencode(".png", frame);

			frame.Dispose();
			return File(imageBytes, "image/png");
		}
		catch (Exception e)
		{
			_logger.Log("CAM-C: Failed to supply frame:");
			_logger.Log(e.Message);
			return BadRequest(e.Message);
		}
	}
	
	private async Task HandleWebSocketAsync(WebSocket webSocket, CancellationToken cancellationToken)
	{
		try
		{
			TimeSpan frameInterval = TimeSpan.FromMilliseconds(30);

			while (!cancellationToken.IsCancellationRequested)
			{
				byte[]? frame = _cameraService.LatestFramePreview.Convert(".jpeg");
				if (frame != null)
				{
					await webSocket.SendAsync(new ArraySegment<byte>(frame), WebSocketMessageType.Binary, true, cancellationToken);
				}

				await Task.Delay(frameInterval, cancellationToken);
			}
		}
		catch (Exception ex)
		{
			_logger.Log("CAM-C: Error in WebSocket communication:");
			_logger.Log(ex.Message);
		}
		finally
		{
			await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", cancellationToken);
		}
	}
}