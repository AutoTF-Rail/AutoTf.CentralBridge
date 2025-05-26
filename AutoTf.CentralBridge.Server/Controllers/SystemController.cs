using System.ComponentModel.DataAnnotations;
using System.Globalization;
using AutoTf.CentralBridge.Extensions;
using AutoTf.CentralBridge.Models.Static;
using AutoTf.Logging;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridge.Server.Controllers;

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
	
	[Catch]
	[MacAuthorize]
	[HttpPost("setdate")]
	public IActionResult SetDate([FromBody, Required] DateTime date)
	{
		_logger.Log($"Date set was requested for date {date.ToString(CultureInfo.InvariantCulture)}.");
		
		string output = CommandExecuter.ExecuteCommand($"date -s \"{date:yyyy-MM-dd HH:mm:ss}\"");
		if(!string.IsNullOrEmpty(output))
			_logger.Log(output);

		_logger.Log("Restarting after date set.");
		
		_lifetime.StopApplication();
		CommandExecuter.ExecuteSilent("(sleep 30; shutdown -h now) &", true);
		
		return Ok();
	}
	
	[Catch]
	[MacAuthorize]
	[HttpPost("update")]
	public IActionResult Update()
	{
		_logger.Log("Update was requested.");
		string prevDir = Directory.GetCurrentDirectory();
	
		Directory.SetCurrentDirectory("/home/CentralBridge/AutoTf.CentralBridge/AutoTf.CentralBridge.Server");
		_logger.Log(CommandExecuter.ExecuteCommand("bash -c \"git reset --hard && git pull && dotnet build -c RELEASE -m\""));
		//TODO:
	
		Directory.SetCurrentDirectory(prevDir);

		_logger.Log("Update finished. Restart pending.");
		return Ok();
	}

	[MacAuthorize]
	[HttpPost("shutdown")]
	public IActionResult Shutdown()
	{
		// TODO: Notify user of shutdown
		_logger.Log("Shutdown was requested.");
		
		// While the application calls this on shutdown, we need to do so as well. We cannot just exit the app here instead because then shutdown now wouldn't be called. That's why we also can't use IHostedService.
		
		_lifetime.StopApplication();
		CommandExecuter.ExecuteSilent("(sleep 30; shutdown -h now) &", true);
		return Ok();
	}

	[MacAuthorize]
	[HttpPost("restart")]
	public IActionResult Restart()
	{
		// TODO: Notify user of restart
		_lifetime.StopApplication();
		CommandExecuter.ExecuteSilent("(sleep 30; reboot now) &", true);
		return Ok();
	}
}