using System.ComponentModel.DataAnnotations;
using AutoTf.CentralBridgeOS.Extensions;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.CentralBridgeOS.Services.Sync;
using AutoTf.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace AutoTf.CentralBridgeOS.Server.Controllers;

[ApiController]
[Route("/information")]
public class InformationController : ControllerBase
{
	private readonly NetworkManager _networkManager;
	private readonly CodeValidator _codeValidator;
	private readonly Logger _logger = Statics.Logger;

	public InformationController(NetworkManager networkManager, CodeValidator codeValidator)
	{
		_networkManager = networkManager;
		_codeValidator = codeValidator;
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
			if (!_codeValidator.ValidateCode(code, serialNumber, timestamp))
			{
				_logger.Log($"Device: {macAddr} tried to login with key {code} and timestamp {timestamp} but failed.");
				return NotFound();
			}

			Statics.AllowedDevices.Add(macAddr);
			_logger.Log($"Device: {macAddr} logged in with key {serialNumber} successfully.");
			return Ok();
		}
		catch
		{
			return BadRequest();
		}
	}
	
	[HttpPost("hello")]
	public IActionResult Hello([FromQuery, Required] string macAddr, [FromQuery, Required] string loginUsername)
	{
		try
		{
			_logger.Log($"Device {macAddr} said hello as loginUsername");
			_networkManager.DeviceSaidHelloEvent.Invoke(macAddr);
			
			return Ok();
		}
		catch
		{
			return BadRequest();
		}
	}
}