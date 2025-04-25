using System.ComponentModel.DataAnnotations;
using AutoTf.CentralBridge.Extensions;
using AutoTf.CentralBridge.Models.Enums;
using AutoTf.CentralBridge.Models.Interfaces;
using AutoTf.CentralBridge.Models.Static;
using AutoTf.CentralBridge.Services;
using AutoTf.CentralBridge.Sync;
using AutoTf.Logging;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridge.Server.Controllers;

[ApiController]
[Route("/information")]
public class InformationController : ControllerBase
{
	private readonly CodeValidator _codeValidator;
	private readonly IFileManager _fileManager;
	private readonly ITrainSessionService _trainSessionService;
	private readonly Logger _logger;
	private readonly string _logDir = "/var/log/AutoTF/AutoTf.CentralBridge.Server/";

	public InformationController(Logger logger, CodeValidator codeValidator, IFileManager fileManager, ITrainSessionService trainSessionService)
	{
		_logger = logger;
		_codeValidator = codeValidator;
		_fileManager = fileManager;
		_trainSessionService = trainSessionService;
		// TODO: Sync notification. Check for next sync date, and then notify tablet users, or admins.
	}

	[HttpGet("serviceState")]
	public ActionResult<BridgeServiceState> GetServiceState()
	{
		try
		{
			return _trainSessionService.LocalServiceState;
		}
		catch (Exception e)
		{
			_logger.Log("Could not supply service state:");
			_logger.Log(e.ToString());
			return BadRequest("Could not supply service state.");
		}
	}

	[HttpGet("logdates")]
	public ActionResult<List<string?>> LogDates()
	{
		try
		{
			string[] files = Directory.GetFiles(_logDir).Order().ToArray();
			return files.Select(Path.GetFileNameWithoutExtension).ToList();
		}
		catch (Exception e)
		{
			_logger.Log("Could not get log dates:");
			_logger.Log(e.ToString());
			return BadRequest("Could not get log dates.");
		}
	}

	[HttpGet("logs")]
	public ActionResult<List<string>> Logs([FromQuery, Required] string date)
	{
		try
		{
			// TODO: We could also just return an empty array here if the file doesn't exist?
			return System.IO.File.ReadAllLines(_logDir + date + ".txt").ToList();
		}
		catch (Exception e)
		{
			_logger.Log($"Could not get logs for date {date}:");
			_logger.Log(e.ToString());
			return BadRequest($"Could not get logs for date {date}j.");
		}
	}

	[HttpGet("version")]
	public ActionResult<string> Version()
	{
		try
		{
			_logger.Log("Version was requested.");
			
			return Statics.GetGitVersion();
		}
		catch (Exception e)
		{
			_logger.Log("Could not report version:");
			_logger.Log(e.Message);
			return BadRequest("Could not report version.");
		}
	}

	[HttpGet("trainId")]
	public ActionResult<string> TrainId()
	{
		try
		{
			return _fileManager.ReadFile("trainId");
		}
		catch (Exception e)
		{
			_logger.Log("Could not supply train id:");
			_logger.Log(e.Message);
			return BadRequest("Could not supply train id.");
		}
	}

	[HttpGet("trainName")]
	public ActionResult<string> TrainName()
	{
		try
		{
			return _fileManager.ReadFile("TrainName");
		}
		catch (Exception e)
		{
			_logger.Log("Could not supply train name:");
			_logger.Log(e.Message);
			return BadRequest("Could not supply train name.");
		}
	}

	[HttpGet("lastsynctry")]
	public ActionResult<DateTime> LastSyncTry()
	{
		try
		{
			return SyncManager.LastSyncTry;
		}
		catch (Exception e)
		{
			_logger.Log("Could not supply last sync try:");
			_logger.Log(e.Message);
			return BadRequest("Could not supply last sync try.");
		}
	}

	[HttpGet("lastsynced")]
	public ActionResult<DateTime> LastSynced()
	{
		try
		{
			return SyncManager.LastSynced;
		}
		catch (Exception e)
		{
			_logger.Log("Could not supply last sync:");
			_logger.Log(e.Message);
			return BadRequest("Could not supply last sync.");
		}
	}

	[HttpGet("evuname")]
	public ActionResult<string> EvuName()
	{
		try
		{
			return _trainSessionService.EvuName;
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
	public ActionResult<bool> IsSimAvailable()
	{
		// To be implemented and tested.
		return false;
	}

	[MacAuthorize]
	[HttpGet("isinternetavailable")]
	public ActionResult<bool> IsInternetAvailable()
	{
		return NetworkConfigurator.IsInternetAvailable();
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