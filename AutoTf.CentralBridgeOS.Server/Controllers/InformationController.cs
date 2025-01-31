using System.ComponentModel.DataAnnotations;
using AutoTf.CentralBridgeOS.Extensions;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.CentralBridgeOS.Services.Sync;
using AutoTf.Logging;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridgeOS.Server.Controllers;

[ApiController]
[Route("/information")]
public class InformationController : ControllerBase
{
	private readonly CodeValidator _codeValidator;
	private readonly FileManager _fileManager;
	private readonly Logger _logger;

	public InformationController(Logger logger, CodeValidator codeValidator, FileManager fileManager)
	{
		_logger = logger;
		_codeValidator = codeValidator;
		_fileManager = fileManager;
		// TODO: Sync notification. Check for next sync date, and then notify tablet users, or admins.
	}

	[HttpGet("version")]
	public IActionResult Version()
	{
		try
		{
			_logger.Log("ROOT-C: Version was requested.");
			
			return Ok(Statics.GetGitVersion());
		}
		catch (Exception e)
		{
			_logger.Log("ROOT-C: Could not report version:");
			_logger.Log(e.Message);
			return BadRequest("ROOT-C: Could not report version.");
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
			_logger.Log("INFO-C: Could not supply train id:");
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
			_logger.Log("INFO-C: Could not supply train name:");
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
			_logger.Log("INFO-C: Could not supply last sync try:");
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
			_logger.Log("INFO-C: Could not supply last sync:");
			_logger.Log(e.Message);
			return BadRequest("Could not supply last sync.");
		}
	}

	[HttpGet("evuname")]
	public IActionResult EvuName()
	{
		try
		{
			return Content(Statics.EvuName);
		}
		catch (Exception e)
		{
			_logger.Log("INFO-C: Could not supply EVU name:");
			_logger.Log(e.Message);
			return BadRequest("Could not supply EVU name.");
		}
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
			_logger.Log("INFO-C: Error during login:");
			_logger.Log(ex.Message);
			return BadRequest("Internal server error.");
		}
	}
}