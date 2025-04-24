using System.ComponentModel.DataAnnotations;
using System.Globalization;
using AutoTf.CentralBridgeOS.Extensions;
using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Models.DataModels;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.CentralBridgeOS.Services.Gps;
using AutoTf.CentralBridgeOS.TrainModels;
using AutoTf.Logging;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridgeOS.Server.Controllers;

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
	public IActionResult GetSpeed()
	{
		try
		{
			return Content(_motionService.CurrentSpeed.ToString()!);
		}
		catch (Exception e)
		{
			_logger.Log("Error while supplying speed:");
			_logger.Log(e.ToString());
			return BadRequest(e.Message);
		}
	}

	[MacAuthorize]
	[HttpGet("lastspeedtime")]
	public IActionResult GetLastSpeedTime()
	{
		try
		{
			return Content(_motionService.LastSpeedTime.ToString("o"));
		}
		catch (Exception e)
		{
			_logger.Log("Error while supplying last speed time:");
			_logger.Log(e.ToString());
			return BadRequest(e.Message);
		}
	}

	[MacAuthorize]
	[HttpGet("areMotorsReleased")]
	public IActionResult AreMotorsReleased()
	{
		try
		{
			return Content(_trainModel.AreMotorsReleased().ToString());
		}
		catch (Exception e)
		{
			_logger.Log("Error while supplying AreMotorsReleased:");
			_logger.Log(e.ToString());
			return BadRequest(e.Message);
		}
	}

	[HttpGet("isEasyControlAvailable")]
	public ActionResult<bool> IsEasyControlAvailable()
	{
		try
		{
			return _trainModel.IsEasyControlAvailable;
		}
		catch (Exception e)
		{
			_logger.Log("Error while supplying IsEasyControlAvailable:");
			_logger.Log(e.ToString());
			return BadRequest(e.Message);
		}
	}

	[MacAuthorize]
	[HttpGet("leverCount")]
	public IActionResult LeverCount()
	{
		try
		{
			return Content(_trainModel.LeverCount().ToString());
		}
		catch (Exception e)
		{
			_logger.Log("Error while supplying lever count:");
			_logger.Log(e.ToString());
			return BadRequest(e.Message);
		}
	}

	[MacAuthorize]
	[HttpGet("leverPosition")]
	public IActionResult LeverPosition([FromQuery, Required] int leverIndex)
	{
		try
		{
			return Content(_trainModel.GetLeverPercentage(leverIndex).ToString() ?? string.Empty);
		}
		catch (Exception e)
		{
			_logger.Log("Error while supplying lever position:");
			_logger.Log(e.ToString());
			return BadRequest(e.Message);
		}
	}

	[MacAuthorize]
	[HttpGet("leverType")]
	public IActionResult LeverType([FromQuery, Required] int leverIndex)
	{
		try
		{
			return Content(_trainModel.GetLeverType(leverIndex).ToString());
		}
		catch (Exception e)
		{
			_logger.Log("Error while supplying lever type:");
			_logger.Log(e.ToString());
			return BadRequest(e.Message);
		}
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
			_trainModel.EasyControl(-100);
			return BadRequest(e.Message);
		}
	}

	[MacAuthorize]
	[HttpPost("easyControl")]
	public IActionResult EasyControl([FromBody, Required] int power)
	{
		try
		{
			_trainModel.EasyControl(power);
			return Ok();
		}
		catch (Exception e)
		{
			_logger.Log("Error while setting easy control:");
			_logger.Log(e.ToString());
			return BadRequest(e.Message);
		}
	}

	[MacAuthorize]
	[HttpPost("releaseMotor")]
	public IActionResult ReleaseMotor([FromBody, Required] int motorIndex)
	{
		try
		{
			_trainModel.ReleaseMotor(motorIndex);
			return Ok();
		}
		catch (Exception e)
		{
			_logger.Log($"Error while releasing motor {motorIndex}:");
			_logger.Log(e.ToString());
			return BadRequest(e.Message);
		}
	}

	[MacAuthorize]
	[HttpPost("releaseMotors")]
	public IActionResult ReleaseMotors()
	{
		try
		{
			_trainModel.ReleaseMotors();
			return Ok();
		}
		catch (Exception e)
		{
			_logger.Log("Error while releasing motors:");
			_logger.Log(e.ToString());
			return BadRequest(e.Message);
		}
	}

	[MacAuthorize]
	[HttpPost("engageMotor")]
	public IActionResult EngageMotor([FromBody, Required] int motorIndex)
	{
		try
		{
			_trainModel.EngageMotor(motorIndex);
			return Ok();
		}
		catch (Exception e)
		{
			_logger.Log($"Error while engaging motor {motorIndex}:");
			_logger.Log(e.ToString());
			return BadRequest(e.Message);
		}
	}

	[MacAuthorize]
	[HttpPost("engageMotors")]
	public IActionResult EngageMotors()
	{
		try
		{
			_trainModel.EngageMotors();
			return Ok();
		}
		catch (Exception e)
		{
			_logger.Log("Error while engaging motors:");
			_logger.Log(e.ToString());
			return BadRequest(e.Message);
		}
	}

	[MacAuthorize]
	[HttpPost("setLever")]
	public IActionResult SetLever([FromBody, Required] LeverSetModel data)
	{
		try
		{
			_logger.Log($"Setting lever: {data.LeverIndex} to {data.Percentage}%");
			
			_trainModel.SetLever(data.LeverIndex, data.Percentage);
			return Ok();
		}
		catch (Exception e)
		{
			_logger.Log("Error while supplying lever type:");
			_logger.Log(e.ToString());
			return BadRequest(e.Message);
		}
	}
}