using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridgeOS.Server.Controllers;

[Route("/")]
public class RootController : ControllerBase
{
	[HttpGet]
	public IActionResult Index()
	{
		return Content("Meow");
	}
}