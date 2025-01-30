using AutoTf.CentralBridgeOS.Extensions;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.Logging;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridgeOS.Server.Controllers;

[ApiController]
[Route("/system")]
public class SystemController : ControllerBase
{
	private readonly CameraService _cameraService;
	private readonly Logger _logger;

	public SystemController(CameraService cameraService, Logger logger)
	{
		_cameraService = cameraService;
		_logger = logger;
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
		
			CommandExecuter.ExecuteSilent("eval $(\"ssh-agent\")", true);
			CommandExecuter.ExecuteSilent("ssh-add /home/CentralBridge/github", true);
		
			Directory.SetCurrentDirectory("/home/CentralBridge/AutoTf.CentralBridgeOS/AutoTf.CentralBridgeOS.Server");
		
			CommandExecuter.ExecuteSilent("git reset --hard", true);
			CommandExecuter.ExecuteSilent("git pull", true);
			CommandExecuter.ExecuteSilent("dotnet build -c RELEASE -m", true);
		
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
		if (!Request.Headers.IsAllowedDevice())
			return Unauthorized();
		
		_cameraService.IntervalCapture();
		CommandExecuter.ExecuteSilent("shutdown now", true);
		return Ok();
	}

	[HttpPost("restart")]
	public IActionResult Restart()
	{
		if (!Request.Headers.IsAllowedDevice())
			return Unauthorized();
		
		_cameraService.IntervalCapture();
		CommandExecuter.ExecuteSilent("reboot now", true);
		return Ok();
	}
}