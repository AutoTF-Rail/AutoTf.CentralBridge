using System.ComponentModel.DataAnnotations;
using AutoTf.CentralBridgeOS.Extensions;
using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.TrainModels;
using AutoTf.Logging;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridgeOS.Server.Controllers;

[ApiController]
[Route("/control")]
public class TrainController : ControllerBase
{
	private readonly Logger _logger;
	private readonly ITrainModel _trainModel;

	public TrainController(Logger logger, ITrainModel trainModel)
	{
		_logger = logger;
		_trainModel = trainModel;
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
			_logger.Log("TC-C: Error while setting easy control:");
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
			_logger.Log("TC-C: Error while supplying AreMotorsReleased:");
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
			_logger.Log($"TC-C: Error while releasing motor {motorIndex}:");
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
			_logger.Log("TC-C: Error while releasing motors:");
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
			_logger.Log($"TC-C: Error while engaging motor {motorIndex}:");
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
			_logger.Log("TC-C: Error while engaging motors:");
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
			_logger.Log("TC-C: Error while supplying lever count:");
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
			_logger.Log("TC-C: Error while supplying lever position:");
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
			_logger.Log("TC-C: Error while supplying lever type:");
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
			_logger.Log($"TC-C: Setting lever: {data.LeverIndex} to {data.Percentage}%");
			
			_trainModel.SetLever(data.LeverIndex, data.Percentage);
			return Ok();
		}
		catch (Exception e)
		{
			_logger.Log("TC-C: Error while supplying lever type:");
			_logger.Log(e.ToString());
			return BadRequest(e.Message);
		}
	}
}