
using System.ComponentModel.DataAnnotations;
using System.Net;
using AutoTf.CentralBridgeOS.Models.CameraService;
using AutoTf.CentralBridgeOS.Models.Interfaces;
using AutoTf.Logging;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridgeOS.Server.Controllers;

/// <summary>
/// Might be offloaded to the processing PC in the future by connecting the cameras to it directly
/// </summary>
[ApiController]
[Route("display")]
public class DisplayController : ControllerBase
{
    private readonly Logger _logger;
    private readonly IProxyManager _proxy;

    public DisplayController(Logger logger, IProxyManager proxy)
    {
        _logger = logger;
        _proxy = proxy;
    }

    [HttpGet("isDisplayRegistered")]
    public ActionResult<bool> IsDisplayRegistered([FromQuery, Required] DisplayType type)
    {
        try
        {
            return _proxy.IsDisplayRegistered(type);
        }
        catch (Exception e)
        {
            _logger.Log($"An error occured while supplying a registered state for display type \"{type}\".");
            _logger.Log(e.ToString());
            return BadRequest(e.ToString());
        }
    }

    [HttpPost("startStream")]
    public IActionResult StartStream([FromQuery, Required] DisplayType type, [FromQuery, Required] int port)
    {
        try
        {
            IPAddress? ipAddress = HttpContext.Connection.RemoteIpAddress;
			
            if (ipAddress != null)
            {
                IPEndPoint receiverEndpoint = new IPEndPoint(ipAddress, port);
                _proxy.StartListeningForDisplay(type, receiverEndpoint);
                
                _logger.Log($"Added receiver: {receiverEndpoint} for display type \"{type}\"");
                return Ok("Receiver added successfully.");
            }
			
            return BadRequest("Could not retrieve client IP address.");
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in start stream for display type \"{type}\".");
            _logger.Log(ex.ToString());
            return BadRequest("Failed to add receiver.");
        }
    }
	
    [HttpPost("stopStream")]
    public IActionResult StopStream([FromQuery, Required] DisplayType type)
    {
        try
        {
            IPAddress? ipAddress = HttpContext.Connection.RemoteIpAddress;
			
            if (ipAddress != null)
            {
                _proxy.StopListeningForDisplay(type, ipAddress);
				
                _logger.Log($"Removed receiver: {ipAddress} for display type \"{type}\"");
                return Ok("Receiver removed successfully.");
            }
			
            return BadRequest("Could not retrieve client IP address.");
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in stop stream for display type \"{type}\".");
            _logger.Log(ex.ToString());
            return BadRequest("Failed to remove receiver.");
        }
    }
}