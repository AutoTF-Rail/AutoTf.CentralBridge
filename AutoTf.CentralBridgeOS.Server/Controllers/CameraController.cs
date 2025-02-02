using System.Diagnostics;
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
	private List<WebSocket> _sockets = new List<WebSocket>();

	public CameraController(CameraService cameraService, Logger logger, SyncManager syncManager)
	{
		_cameraService = cameraService;
		_logger = logger;
		_syncManager = syncManager;

		Statics.ShutdownEvent += () => _sockets.ForEach(x => x.Dispose());
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
			_sockets.Add(webSocket);
			await HandleWebSocketAsync(webSocket, cancellationToken);
		}
		catch (Exception e)
		{
			_logger.Log("CAM-C: Error during WebSocket initialization.");
			_logger.Log(e.Message);
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

			byte[] imageBytes = CvInvoke.Imencode(".jpg", frame);

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
			
			while (!cancellationToken.IsCancellationRequested && webSocket.State == WebSocketState.Open)
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
			webSocket.Dispose();
			_sockets.Remove(webSocket);
		}
	}
}