using AutoTf.CentralBridge.Extensions;
using AutoTf.CentralBridge.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridge.Server.Controllers.Aic;

[ApiController]
[Route("/aic")]
public class AicController : ControllerBase
{
    private readonly IAicService _aicService;

    public AicController(IAicService aicService)
    {
        _aicService = aicService;
    }

    [HttpGet("online")]
    public ActionResult<bool> Online() => _aicService.Online;

    [HttpGet("isAvailable")]
    public async Task<ActionResult<bool?>> IsAvailable() => await _aicService.IsAvailable();

    [HttpGet("isOnline")]
    public async Task<ActionResult<bool>> IsOnline() => await _aicService.IsOnline();

    [HttpGet("version")]
    public async Task<ActionResult<string>> Version() => await _aicService.Version();

    [MacAuthorize]
    [HttpPost("shutdown")]
    public IActionResult Shutdown()
    {
        _aicService.Shutdown();
        return Ok();
    }

    [MacAuthorize]
    [HttpPost("restart")]
    public IActionResult Restart()
    {
        _aicService.Restart();
        return Ok();
    }
}