using System.ComponentModel.DataAnnotations;
using AutoTf.CentralBridgeOS.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridgeOS.Server.Controllers.Aic;

[ApiController]
[Route("/aic/information")]
public class AicInformationController : ControllerBase
{
    private readonly IAicService _aicService;

    public AicInformationController(IAicService aicService)
    {
        _aicService = aicService;
    }
    
    [HttpGet("logDates")]
    public async Task<ActionResult<string[]>> LogDates() => await _aicService.LogDates();

    [HttpGet("logs")]
    public async Task<ActionResult<string[]>> Logs([FromQuery, Required] string date) => await _aicService.Logs(date);
}