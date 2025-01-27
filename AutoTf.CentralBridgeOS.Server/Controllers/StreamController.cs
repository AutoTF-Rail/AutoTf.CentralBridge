using System.Net.WebSockets;
using AutoTf.CentralBridgeOS.Services;
using Emgu.CV;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridgeOS.Server.Controllers;

[ApiController]
public class StreamController : ControllerBase
{
	private readonly CameraService _cameraService;

	public StreamController(CameraService cameraService)
	{
		_cameraService = cameraService;
	}
	
	[Route("/stream")]
	public async Task GetStream(CancellationToken cancellationToken)
	{
		if (!HttpContext.WebSockets.IsWebSocketRequest)
		{
			Response.StatusCode = 400;
			return;
		}

		WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
		await HandleWebSocketAsync(webSocket, cancellationToken);
	}
	
	private async Task HandleWebSocketAsync(WebSocket webSocket, CancellationToken cancellationToken)
	{
		try
		{
			TimeSpan frameInterval = TimeSpan.FromMilliseconds(100);

			while (!cancellationToken.IsCancellationRequested)
			{
				byte[]? frame = GetLatestFrame();
				if (frame != null)
				{
					await webSocket.SendAsync(new ArraySegment<byte>(frame), WebSocketMessageType.Binary, true, cancellationToken);
				}

				await Task.Delay(frameInterval, cancellationToken);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error in WebSocket communication: {ex.Message}");
		}
		finally
		{
			await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", cancellationToken);
		}
	}

	private byte[]? GetLatestFrame()
	{
		Mat? frame = _cameraService.LatestFramePreview;
            
		if (frame != null && !frame.IsEmpty)
		{
			return CvInvoke.Imencode(".png", frame);
		}

		return null;
	}
}