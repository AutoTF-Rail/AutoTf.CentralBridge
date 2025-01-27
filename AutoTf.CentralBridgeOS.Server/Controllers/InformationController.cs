using System.ComponentModel.DataAnnotations;
using AutoTf.CentralBridgeOS.Extensions;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.CentralBridgeOS.Services.Sync;
using AutoTf.Logging;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace AutoTf.CentralBridgeOS.Server.Controllers;

[ApiController]
[Route("/information")]
public class InformationController : ControllerBase
{
	private readonly NetworkManager _networkManager;
	private readonly CodeValidator _codeValidator;
	private readonly FileManager _fileManager;
	private readonly CameraService _cameraService;
	private readonly Logger _logger = Statics.Logger;

	public InformationController(NetworkManager networkManager, CodeValidator codeValidator, FileManager fileManager, CameraService cameraService)
	{
		_networkManager = networkManager;
		_codeValidator = codeValidator;
		_fileManager = fileManager;
		_cameraService = cameraService;
		// TODO: Sync notification. Check for next sync date, and then notify tablet users, or admins.
	}

	[HttpGet("latestFramePreview")]
	public IActionResult LatestFramePreview()
	{
		try
		{
			Mat frame = _cameraService.LatestFrame;
			Mat frame = _cameraService.LatestFramePreview;

			byte[] imageBytes = CvInvoke.Imencode(".png", frame);

			return File(imageBytes, "image/png");
		}
		catch (Exception e)
		{
			Console.WriteLine("Failed to supply preview frame:");
			Console.WriteLine(e.Message);
			return BadRequest(e.Message);
		}
	}

	[HttpGet("latestFrame")]
	public IActionResult LatestFrame()
	{
		try
		{
			Mat frame = _cameraService.LatestFrame;

			byte[] imageBytes = CvInvoke.Imencode(".png", frame);

			return File(imageBytes, "image/png");
		}
		catch (Exception e)
		{
			Console.WriteLine("Failed to supply frame:");
			Console.WriteLine(e.Message);
			return BadRequest(e.Message);
		}
	}

	[HttpGet("trainId")]
	public IActionResult TrainId()
	{
		return Content(_fileManager.ReadFile("trainId"));
	}

	[HttpGet("trainName")]
	public IActionResult TrainName()
	{
		return Content(_fileManager.ReadFile("TrainName"));
	}

	[HttpGet("lastsynctry")]
	public IActionResult LastSyncTry()
	{
		return Content(SyncManager.LastSyncTry.ToString("dd.MM.yyyy HH:mm:ss"));
	}

	[HttpGet("lastsynced")]
	public IActionResult LastSynced()
	{
		return Content(SyncManager.LastSynced.ToString("dd.MM.yyyy HH:mm:ss"));
	}

	[HttpGet("evuname")]
	public IActionResult EvuName()
	{
		return Content(Statics.EvuName);
	}

	[HttpGet("issimavailable")]
	public IActionResult IsSimAvailable()
	{
		if (!Request.Headers.IsAllowedDevice())
			return Unauthorized();
		
		// To be implemented and tested.
		return Content("False");
	}

	[HttpGet("isinternetavailable")]
	public IActionResult IsInternetAvailable()
	{
		if (!Request.Headers.IsAllowedDevice())
			return Unauthorized();
		
		return Content(NetworkConfigurator.IsInternetAvailable().ToString());
	}
	
	[HttpPost("login")]
	public IActionResult Login([FromQuery, Required] string macAddr, [FromQuery, Required] string serialNumber, [FromQuery, Required] string code, [FromQuery, Required] DateTime timestamp)
	{
		try
		{
			Console.WriteLine("Processing login...");
			if (!_codeValidator.ValidateCode(code, serialNumber, timestamp))
			{
				_logger.Log($"Device: {macAddr} tried to login with key {code} and timestamp {timestamp} but failed.");
				return NotFound();
			}

			Statics.AllowedDevices.Add(macAddr);
			_logger.Log($"Device: {macAddr} logged in with key {serialNumber} successfully.");
			return Ok();
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error during login: ");
			Console.WriteLine(ex.Message);
			return BadRequest(ex.Message);
		}
	}
}