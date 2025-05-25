using System.ComponentModel.DataAnnotations;
using System.Net;
using AutoTf.CentralBridge.Extensions;
using AutoTf.CentralBridge.Models.Interfaces;
using AutoTf.CentralBridge.Shared.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridge.Server.Controllers;

/// <summary>
/// Might be offloaded to the processing PC in the future by connecting the cameras to it directly
/// </summary>
[ApiController]
[Route("display")]
public class DisplayController : ControllerBase
{
    private readonly ILogger<DisplayController> _logger;
    private readonly IProxyManager _proxy;

    public DisplayController(ILogger<DisplayController> logger, IProxyManager proxy)
    {
        _logger = logger;
        _proxy = proxy;
    }

    [Catch]
    [HttpGet("isDisplayRegistered")]
    public ActionResult<bool> IsDisplayRegistered([FromQuery, Required] DisplayType type)
    {
        return _proxy.IsDisplayRegistered(type);
    }

    [Catch]
    [HttpPost("startStream")]
    public IActionResult StartStream([FromQuery, Required] DisplayType type, [FromQuery, Required] int port)
    {
        IPAddress? ipAddress = HttpContext.Connection.RemoteIpAddress;
		
        if (ipAddress != null)
        {
            IPEndPoint receiverEndpoint = new IPEndPoint(ipAddress, port);
            _proxy.StartListeningForDisplay(type, receiverEndpoint);
            
            _logger.LogTrace($"Added receiver: {receiverEndpoint} for display type \"{type}\"");
            return Ok("Receiver added successfully.");
        }
		
        return BadRequest("Could not retrieve client IP address.");
    }
	
    [Catch]
    [HttpPost("stopStream")]
    public IActionResult StopStream([FromQuery, Required] DisplayType type)
    {
        IPAddress? ipAddress = HttpContext.Connection.RemoteIpAddress;
		
        if (ipAddress != null)
        {
            _proxy.StopListeningForDisplay(type, ipAddress);
			
            _logger.LogTrace($"Removed receiver: {ipAddress} for display type \"{type}\"");
            return Ok("Receiver removed successfully.");
        }
		
        return BadRequest("Could not retrieve client IP address.");
    }
}