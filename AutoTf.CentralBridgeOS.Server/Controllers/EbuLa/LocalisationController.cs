using AutoTf.CentralBridgeOS.Extensions;
using AutoTf.CentralBridgeOS.Localise.Display;
using AutoTf.CentralBridgeOS.Models;
using AutoTf.Logging;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridgeOS.Server.Controllers.EbuLa;

/// <summary>
/// Documented in OpenApi
/// It's important to note that this doesn't turn off general localisation, but instead just the localisation via the EbuLa with the location marker.
/// </summary>
[ApiController]
[Route("ebula/localisation")]
public class LocalisationController : ControllerBase
{
    private readonly Logger _logger;
    private readonly EbuLaService _ebula;
    
    public LocalisationController(Logger logger, EbuLaService ebula)
    {
        _logger = logger;
        _ebula = ebula;
    }
    
    [MacAuthorize]
    [HttpPost("disable")]
    public IActionResult DisableLocalisation()
    {
        try
        {
            // TODO: Save this as a bool in the corresponding services. This doens't need to be saved to a file.
            return Ok();
        }
        catch (Exception e)
        {
            _logger.Log("Something went wrong when disabling localisation:");
            _logger.Log(e.ToString());
            return BadRequest(e.ToString());
        }
    }
    
    [MacAuthorize]
    [HttpPost("enable")]
    public IActionResult EnableLocalisation()
    {
        try
        {
            // TODO: Save this as a bool in the corresponding and notify the needed services. This doesn't need to be saved to a file.
            // TODO: Invoke jump to current ebula page, and scan marker
            // TODO: Be careful that the localisation doesn't happen while the ebula is being scanned.
            return Ok();
        }
        catch (Exception e)
        {
            _logger.Log("Something went wrong when enabling localisation:");
            _logger.Log(e.ToString());
            return BadRequest(e.ToString());
        }
    }
    
    [MacAuthorize]
    [HttpPost("turnOff")]
    public IActionResult TurnOffLocalisation()
    {
        try
        {
            // TODO: Notify the corresponding services, that this has changed.
            // TODO: Save/cache this value in the corresponding service
            // _fileManager.WriteAllText("localisationEnabled", "false");
            return Ok();
        }
        catch (Exception e)
        {
            _logger.Log("Something went wrong when turning off localisation:");
            _logger.Log(e.ToString());
            return BadRequest(e.ToString());
        }
    }
    
    [MacAuthorize]
    [HttpPost("turnOn")]
    public IActionResult TurnOnLocalisation()
    {
        try
        {
            // TODO: notify the corresponding services, and cache this value
            // TODO: Invoke jump to current ebula page, and scan marker
            // TODO: Be careful that the localisation doesn't happen while the ebula is being scanned.
            // _fileManager.WriteAllText("localisationEnabled", "true");
            return Ok();
        }
        catch (Exception e)
        {
            _logger.Log("Something went wrong when turning on localisation:");
            _logger.Log(e.ToString());
            return BadRequest(e.ToString());
        }
    }
    
    [HttpGet("state")]
    public ActionResult<ServiceState> LocalisationState()
    {
        try
        {
            // TODO: Implement
            return Ok();
        }
        catch (Exception e)
        {
            _logger.Log("Something went wrong when supplying the localisation state:");
            _logger.Log(e.ToString());
            return BadRequest(e.ToString());
        }
    }
}