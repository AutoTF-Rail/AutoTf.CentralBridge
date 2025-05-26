using System.ComponentModel.DataAnnotations;
using AutoTf.CentralBridge.Extensions;
using AutoTf.CentralBridge.Models.Enums;
using AutoTf.CentralBridge.Models.Interfaces;
using AutoTf.CentralBridge.Models.Static;
using AutoTf.CentralBridge.Services;
using AutoTf.CentralBridge.Shared.Models;
using AutoTf.CentralBridge.Shared.Models.Enums;
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
		return _trainSessionService.LocalServiceState;
	}

	[Catch]
	[HttpGet("logdates")]
	public ActionResult<List<string?>> LogDates()
	{
		string[] files = Directory.GetFiles(_logDir).Order().ToArray();
		return files.Select(Path.GetFileNameWithoutExtension).ToList();
	}

	[Catch]
	[HttpGet("logs")]
	public Result<List<string>> Logs([FromQuery, Required] string date)
	{
		string path = Path.Combine(_logDir, date + ".txt");
		
		if (!System.IO.File.Exists(path))
			return Result<List<string>>.Fail(ResultCode.NotFound);
			
		return System.IO.File.ReadAllLines(path).ToList();
	}

	[Catch]
	[HttpGet("version")]
	public ActionResult<string> Version()
	{
		return Statics.GetGitVersion();
	}

	[Catch]
	[HttpGet("trainId")]
	public ActionResult<string> TrainId()
	{
		return _fileManager.ReadFile("trainId");
	}

	[Catch]
	[HttpGet("trainName")]
	public ActionResult<string> TrainName()
	{
		return _fileManager.ReadFile("TrainName");
	}

	[HttpGet("lastsynctry")]
	public ActionResult<DateTime> LastSyncTry()
	{
		return SyncManager.LastSyncTry;
	}

	[HttpGet("lastsynced")]
	public ActionResult<DateTime> LastSynced()
	{
		return SyncManager.LastSynced;
	}

	[HttpGet("evuname")]
	public ActionResult<string> EvuName()
	{
		return _trainSessionService.EvuName;
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
	
	[Catch]
	[HttpPost("login")]
	public IActionResult Login([FromQuery, Required] string macAddr, [FromQuery, Required] string serialNumber, [FromQuery, Required] string code, [FromQuery, Required] DateTime timestamp)
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
}