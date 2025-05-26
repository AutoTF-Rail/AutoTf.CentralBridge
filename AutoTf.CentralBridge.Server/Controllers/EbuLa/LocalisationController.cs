using AutoTf.CentralBridge.Extensions;
using AutoTf.CentralBridge.Models.Enums;
using AutoTf.CentralBridge.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridge.Server.Controllers.EbuLa;

/// <summary>
/// Documented in OpenApi
/// It's important to note that this doesn't turn off general localisation, but instead just the localisation via the EbuLa with the location marker.
/// </summary>
[ApiController]
[Route("ebula/localisation")]
public class LocalisationController : ControllerBase
{
    private readonly IEbuLaService _ebula;
    
    public LocalisationController(IEbuLaService ebula)
    {
        _ebula = ebula;
    }
    
    [MacAuthorize]
    [HttpPost("disable")]
    public IActionResult DisableLocalisation()
    {
        // TODO: Save this as a bool in the corresponding services. This doens't need to be saved to a file.
        return Ok();
    }
    
    [MacAuthorize]
    [HttpPost("enable")]
    public IActionResult EnableLocalisation()
    {
        // TODO: Save this as a bool in the corresponding and notify the needed services. This doesn't need to be saved to a file.
        // TODO: Invoke jump to current ebula page, and scan marker
        // TODO: Be careful that the localisation doesn't happen while the ebula is being scanned.
        return Ok();
    }
    
    [MacAuthorize]
    [HttpPost("turnOff")]
    public IActionResult TurnOffLocalisation()
    {
        // TODO: Notify the corresponding services, that this has changed.
        // TODO: Save/cache this value in the corresponding service
        // _fileManager.WriteAllText("localisationEnabled", "false");
        return Ok();
    }
    
    [MacAuthorize]
    [HttpPost("turnOn")]
    public IActionResult TurnOnLocalisation()
    {
        // TODO: notify the corresponding services, and cache this value
        // TODO: Invoke jump to current ebula page, and scan marker
        // TODO: Be careful that the localisation doesn't happen while the ebula is being scanned.
        // _fileManager.WriteAllText("localisationEnabled", "true");
        return Ok();
    }
    
    [HttpGet("state")]
    public ActionResult<ServiceState> LocalisationState()
    {
        // TODO: Implement
        return Ok();
    }
}