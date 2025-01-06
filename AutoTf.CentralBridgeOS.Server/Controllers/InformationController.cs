using AutoTf.CentralBridgeOS.Services;
using AutoTf.Logging;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridgeOS.Server.Controllers;

[Route("/information")]
public class InformationController : ControllerBase
{
	private readonly NetworkManager _networkManager;
	private readonly Logger _logger = Statics.Logger;

	public InformationController(NetworkManager networkManager)
	{
		_networkManager = networkManager;
	}

	[HttpGet("evuname")]
	public IActionResult EvuName()
	{
		return Content(Statics.EvuName);
	}

	[HttpGet("issimavailable")]
	public IActionResult IsSimAvailable()
	{
		// To be implemented and tested.
		return Content("False");
	}

	[HttpGet("isinternetavailable")]
	public IActionResult IsInternetAvailable()
	{
		return Content(NetworkConfigurator.IsInternetAvailable().ToString());
	}
	
	// Body:
	// 1: MAC Address
	// 2: Device type (service, tablet, other)
	// 3: Login info (username for tablets, login reason for service)
	// Example:
	// [
	// "meow",
	// "meow",
	// "meow"
	// ]
	[HttpPost("hello")]
	public IActionResult Hello([FromBody] string[] body)
	{
		try
		{
			if (body.Length != 3)
				return BadRequest();
			
			_logger.Log($"Device {body[0]} said hello:");
			_logger.Log("Device type: " + body[1]);
			_logger.Log("Device Login Info: " + body[2]);
			_networkManager.DeviceSaidHelloEvent.Invoke(body[0]);
			
			return Ok();
		}
		catch
		{
			return BadRequest();
		}
	}
}