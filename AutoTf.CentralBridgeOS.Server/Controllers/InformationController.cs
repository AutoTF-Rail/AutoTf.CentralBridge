using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AutoTf.CentralBridgeOS.Extensions;
using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.CentralBridgeOS.Services.Network;
using AutoTf.CentralBridgeOS.Sync;
using AutoTf.Logging;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridgeOS.Server.Controllers;

[ApiController]
[Route("/information")]
public class InformationController : ControllerBase
{
	private readonly CodeValidator _codeValidator;
	private readonly FileManager _fileManager;
	private readonly TrainSessionService _trainSessionService;
	private readonly Logger _logger;
	private readonly string _logDir = "/var/log/AutoTF/AutoTf.CentralBridgeOS.Server/";

	public InformationController(Logger logger, CodeValidator codeValidator, FileManager fileManager, TrainSessionService trainSessionService)
	{
		_logger = logger;
		_codeValidator = codeValidator;
		_fileManager = fileManager;
		_trainSessionService = trainSessionService;
		// TODO: Sync notification. Check for next sync date, and then notify tablet users, or admins.
	}

	[HttpGet("serviceState")]
	public IActionResult GetServiceState()
	{
		try
		{
			return Content(_trainSessionService.LocalServiceState.ToString());
		}
		catch (Exception e)
		{
			_logger.Log("Could not supply service state:");
			_logger.Log(e.ToString());
			return BadRequest("Could not supply service state.");
		}
	}

	[HttpGet("logdates")]
	public IActionResult LogDates()
	{
		try
		{
			string[] files = Directory.GetFiles(_logDir).Order().ToArray();
			return Content(JsonSerializer.Serialize(files.Select(Path.GetFileNameWithoutExtension)));
		}
		catch (Exception e)
		{
			_logger.Log("Could not get log dates:");
			_logger.Log(e.ToString());
			return BadRequest("Could not get log dates.");
		}
	}

	[HttpGet("logs")]
	public IActionResult Logs([FromQuery, Required] string date)
	{
		try
		{
			return Content(JsonSerializer.Serialize(System.IO.File.ReadAllLines(_logDir + date + ".txt")));
		}
		catch (Exception e)
		{
			_logger.Log($"Could not get logs for date {date}:");
			_logger.Log(e.ToString());
			return BadRequest($"Could not get logs for date {date}j.");
		}
	}

	[HttpGet("version")]
	public IActionResult Version()
	{
		try
		{
			_logger.Log("Version was requested.");
			
			return Ok(Statics.GetGitVersion());
		}
		catch (Exception e)
		{
			_logger.Log("Could not report version:");
			_logger.Log(e.Message);
			return BadRequest("Could not report version.");
		}
	}

	[HttpGet("trainId")]
	public IActionResult TrainId()
	{
		try
		{
			return Content(_fileManager.ReadFile("trainId"));
		}
		catch (Exception e)
		{
			_logger.Log("Could not supply train id:");
			_logger.Log(e.Message);
			return BadRequest("Could not supply train id.");
		}
	}

	[HttpGet("trainName")]
	public IActionResult TrainName()
	{
		try
		{
			return Content(_fileManager.ReadFile("TrainName"));
		}
		catch (Exception e)
		{
			_logger.Log("Could not supply train name:");
			_logger.Log(e.Message);
			return BadRequest("Could not supply train name.");
		}
	}

	[HttpGet("lastsynctry")]
	public IActionResult LastSyncTry()
	{
		try
		{
			return Content(SyncManager.LastSyncTry.ToString("o"));
		}
		catch (Exception e)
		{
			_logger.Log("Could not supply last sync try:");
			_logger.Log(e.Message);
			return BadRequest("Could not supply last sync try.");
		}
	}

	[HttpGet("lastsynced")]
	public IActionResult LastSynced()
	{
		try
		{
			return Content(SyncManager.LastSynced.ToString("o"));
		}
		catch (Exception e)
		{
			_logger.Log("Could not supply last sync:");
			_logger.Log(e.Message);
			return BadRequest("Could not supply last sync.");
		}
	}

	[HttpGet("evuname")]
	public IActionResult EvuName()
	{
		try
		{
			return Content(_trainSessionService.EvuName);
		}
		catch (Exception e)
		{
			_logger.Log("Could not supply EVU name:");
			_logger.Log(e.Message);
			return BadRequest("Could not supply EVU name.");
		}
	}

	[MacAuthorize]
	[HttpGet("issimavailable")]
	public IActionResult IsSimAvailable()
	{
		// To be implemented and tested.
		return Content("False");
	}

	[MacAuthorize]
	[HttpGet("isinternetavailable")]
	public IActionResult IsInternetAvailable()
	{
		return Content(NetworkConfigurator.IsInternetAvailable().ToString());
	}
	
	[HttpPost("login")]
	public IActionResult Login([FromQuery, Required] string macAddr, [FromQuery, Required] string serialNumber, [FromQuery, Required] string code, [FromQuery, Required] DateTime timestamp)
	{
		try
		{
			_logger.Log("Processing Login.");
			CodeValidationResult result = _codeValidator.ValidateCode(code, serialNumber, timestamp);
			if (result != CodeValidationResult.Valid)
			{
				_logger.Log($"Device: {macAddr} tried to login with key {code} and timestamp {timestamp} but failed with reason {result.ToString()}.");
				return NotFound();
			}

			Statics.AllowedDevices.Add(macAddr);
			_logger.Log($"Device: {macAddr} logged in with key {serialNumber} successfully.");
			return Ok();
		}
		catch (Exception ex)
		{
			_logger.Log("Error during login:");
			_logger.Log(ex.Message);
			return BadRequest("Internal server error.");
		}
	}
}