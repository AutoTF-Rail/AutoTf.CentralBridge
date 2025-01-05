using AutoTf.CentralBridgeOS.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridgeOS.Server.Controllers;

[Route("/information")]
public class InformationController : ControllerBase
{
	private readonly NetworkManager _networkManager;

	public InformationController(NetworkManager networkManager)
	{
		_networkManager = networkManager;
	}
	
	[HttpPost("hello")]
	public IActionResult Index()
	{
		_networkManager.DeviceSaidHelloEvent.Invoke(HttpContext.Connection.RemoteIpAddress!.ToString());
		return Ok();
	}
}