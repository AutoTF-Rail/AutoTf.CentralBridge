using System.ComponentModel.DataAnnotations;
using AutoTf.CentralBridge.Extensions;
using AutoTf.CentralBridge.Models.DataModels;
using AutoTf.CentralBridge.Models.Enums;
using AutoTf.CentralBridge.Services.Gps;
using AutoTf.Logging;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridge.Server.Controllers;

[ApiController]
[Route("/control")]
public class ControlController : ControllerBase
{
	private readonly Logger _logger;
	private readonly ITrainModel _trainModel;
	private readonly MotionService _motionService;

	public ControlController(Logger logger, ITrainModel trainModel, MotionService motionService)
	{
		_logger = logger;
		_trainModel = trainModel;
		_motionService = motionService;
	}

	// TODO: This is only gps speed atm
	[MacAuthorize]
	[HttpGet("speed")]
	public ActionResult<double?> GetSpeed()
	{
		return _motionService.CurrentSpeed;
	}

	[MacAuthorize]
	[HttpGet("lastspeedtime")]
	public ActionResult<DateTime> GetLastSpeedTime()
	{
		return _motionService.LastSpeedTime;
	}

	[MacAuthorize]
	[HttpGet("areMotorsReleased")]
	public ActionResult<bool> AreMotorsReleased()
	{
		return _trainModel.AreMotorsReleased();
	}

	[HttpGet("isEasyControlAvailable")]
	public ActionResult<bool> IsEasyControlAvailable()
	{
		return _trainModel.IsEasyControlAvailable;
	}

	[MacAuthorize]
	[HttpGet("leverCount")]
	public ActionResult<int> LeverCount()
	{
		return _trainModel.LeverCount();
	}

	[Catch]
	[MacAuthorize]
	[HttpGet("leverPosition")]
	public ActionResult<double?> LeverPosition([FromQuery, Required] int leverIndex)
	{
		return _trainModel.GetLeverPercentage(leverIndex);
	}

	[Catch]
	[MacAuthorize]
	[HttpGet("leverType")]
	public ActionResult<LeverType> LeverType([FromQuery, Required] int leverIndex)
	{
		return _trainModel.GetLeverType(leverIndex);
	}

	[MacAuthorize]
	[HttpPost("emergencybrake")]
	public IActionResult EmergencyBrake()
	{
		try
		{
			_trainModel.EmergencyBrake();
			return Ok();
		}
		catch (Exception e)
		{
			// TODO: Do we need a better solution here when it fails?
			_logger.Log("Error while emergency braking:");
			_logger.Log(e.ToString());
			// TODO: Release sifa padel when this happens
			_trainModel.EasyControl(-100);
			return BadRequest(e.Message);
		}
	}

	[Catch]
	[MacAuthorize]
	[HttpPost("easyControl")]
	public IActionResult EasyControl([FromBody, Required] int power)
	{
		_trainModel.EasyControl(power);
		return Ok();
	}

	[Catch]
	[MacAuthorize]
	[HttpPost("releaseMotor")]
	public IActionResult ReleaseMotor([FromBody, Required] int motorIndex)
	{
		_trainModel.ReleaseMotor(motorIndex);
		return Ok();
	}

	[Catch]
	[MacAuthorize]
	[HttpPost("releaseMotors")]
	public IActionResult ReleaseMotors()
	{
		_trainModel.ReleaseMotors();
		return Ok();
	}

	[Catch]
	[MacAuthorize]
	[HttpPost("engageMotor")]
	public IActionResult EngageMotor([FromBody, Required] int motorIndex)
	{
		_trainModel.EngageMotor(motorIndex);
		return Ok();
	}

	[Catch]
	[MacAuthorize]
	[HttpPost("engageMotors")]
	public IActionResult EngageMotors()
	{
		_trainModel.EngageMotors();
		return Ok();
	}

	[Catch]
	[MacAuthorize]
	[HttpPost("setLever")]
	public IActionResult SetLever([FromBody, Required] LeverSetModel data)
	{
		_logger.Log($"Setting lever: {data.LeverIndex} to {data.Percentage}%");
		
		_trainModel.SetLever(data.LeverIndex, data.Percentage);
		return Ok();
	}
}