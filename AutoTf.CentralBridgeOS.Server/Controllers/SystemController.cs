using System.ComponentModel.DataAnnotations;
using System.Globalization;
using AutoTf.CentralBridgeOS.Extensions;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.Logging;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridgeOS.Server.Controllers;

[ApiController]
[Route("/system")]
public class SystemController : ControllerBase
{
	private readonly Logger _logger;
	private readonly IHostApplicationLifetime _lifetime;

	public SystemController(Logger logger, IHostApplicationLifetime lifetime)
	{
		_logger = logger;
		_lifetime = lifetime;
	}
	
	[HttpPatch("setdate")]
	public IActionResult SetDate([FromBody, Required] DateTime date)
	{
		try
		{
			if (!Request.Headers.IsAllowedDevice())
				return Unauthorized();
			
			_logger.Log($"ROOT-C: Date set was requested for date {date.ToString(CultureInfo.InvariantCulture)}.");
			
			_logger.Log(CommandExecuter.ExecuteCommand($"date -s \"{date:yyyy-MM-dd HH:mm:ss}\""));

			_logger.Log("ROOT-C: Restarting after date set.");
			
			_lifetime.StopApplication();
			CommandExecuter.ExecuteSilent("bash -c \"sleep 30; shutdown -h now\"&", true);
			
			return Ok();
		}
		catch (Exception e)
		{
			_logger.Log("ROOT-C: Could not update:");
			_logger.Log(e.ToString());
			return BadRequest("ROOT-C: Could not update.");
		}
	}
	
	[HttpPost("update")]
	public IActionResult Update()
	{
		try
		{
			if (!Request.Headers.IsAllowedDevice())
				return Unauthorized();
			
			_logger.Log("ROOT-C: Update was requested.");
			string prevDir = Directory.GetCurrentDirectory();
		
			Directory.SetCurrentDirectory("/home/CentralBridge/AutoTf.CentralBridgeOS/AutoTf.CentralBridgeOS.Server");
			_logger.Log(CommandExecuter.ExecuteCommand("bash -c \"eval $(ssh-agent) && ssh-add /home/CentralBridge/github && git reset --hard && git pull && dotnet build -c RELEASE -m\""));
		
			Directory.SetCurrentDirectory(prevDir);

			_logger.Log("ROOT-C: Update finished. Restart pending.");
			return Ok();
		}
		catch (Exception e)
		{
			_logger.Log("ROOT-C: Could not update:");
			_logger.Log(e.Message);
			return BadRequest("ROOT-C: Could not update.");
		}
	}

	[HttpPost("shutdown")]
	public IActionResult Shutdown()
	{
		// TODO: Notify user of shutdown
		if (!Request.Headers.IsAllowedDevice())
			return Unauthorized();
		
		_logger.Log("SC: Shutdown was requested.");
		
		// While the application calls this on shutdown, we need to do so as well. We cannot just exit the app here instead because then shutdown now wouldn't be called. That's why we also can't use IHostedService.
		
		_lifetime.StopApplication();
		CommandExecuter.ExecuteSilent("bash -c \"sleep 30; shutdown -h now\"&", true);
		return Ok();
	}

	[HttpPost("restart")]
	public IActionResult Restart()
	{
		// TODO: Notify user of restart
		if (!Request.Headers.IsAllowedDevice())
			return Unauthorized();
		
		_lifetime.StopApplication();
		CommandExecuter.ExecuteSilent("bash -c \"sleep 30; reboot now\"&", true);
		return Ok();
	}
}