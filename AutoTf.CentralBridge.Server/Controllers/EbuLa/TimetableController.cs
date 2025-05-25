using System.ComponentModel.DataAnnotations;
using AutoTf.CentralBridge.Extensions;
using AutoTf.CentralBridge.Models.Enums;
using AutoTf.CentralBridge.Models.Interfaces;
using AutoTf.FahrplanParser.Content;
using AutoTf.FahrplanParser.Content.Content.Base;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridge.Server.Controllers.EbuLa;

/// <summary>
/// Documented in OpenApi
/// </summary>
[ApiController]
[Route("ebula/timetable")]
public class TimetableController : ControllerBase
{
    private readonly IEbuLaService _ebula;
    
    public TimetableController(IEbuLaService ebula)
    {
        _ebula = ebula;
    }

    [HttpGet("currentTable")]
    public ActionResult<List<KeyValuePair<string, IRowContent>>> CurrentTable()
    {
        // TODO: Merge in the speed changes (if we decide to keep this seperate
        return _ebula.CurrentTimetable.ToList();
    }

    [Catch]
    [MacAuthorize]
    [HttpPost("edit")]
    public IActionResult Edit([FromBody, Required] List<KeyValuePair<string, IRowContent>> newTable)
    {
        // TODO: Notify other services that it has changed and location service should recalculate the current position. (This probably isn't allowed to happen while moving)
        _ebula.OverwriteTable(newTable);
        return Ok();
    }

    [HttpGet("currentConditions")]
    public ActionResult<List<KeyValuePair<string, RowContent>>> CurrentConditions()
    {
        // TODO: Grab current conditions from _ebula, and maybe rework the return type, so that we can represent things like speed changes better? (Or just supply the next change too?)
        return new List<KeyValuePair<string, RowContent>>();
    }

    /// <summary>
    /// Disables the auto detection of EbuLa changes for this session.
    /// (TODO: does this even ever happen? But it could disable the "validation" of the EbuLa while driving)
    /// </summary>
    [MacAuthorize]
    [HttpPost("disable")]
    public IActionResult DisableAutoDetection()
    {
        // TODO: save the bool in _ebula, so that others can access it via that. This doesn't need to be saved to a file.
        _ebula.AutoDetectState = ServiceState.TemporaryOff;
        return Ok();
    }
    
    [Catch]
    [MacAuthorize]
    [HttpPost("enable")]
    public IActionResult EnableAutoDetection()
    {
        // TODO: Save this as a bool in _ebula and notify the needed services. This doesn't need to be saved to a file.
        // TODO: Invoke a full rescan of the timetable
        _ebula.AutoDetectState = ServiceState.TemporaryOn;
        return Ok();
    }
    
    /// <summary>
    /// Permanently turns off the auto detection of a timetable from the EbuLa.
    /// (TODO: Although we will probably still want to enter the train number into the device on startup)
    /// </summary>
    [MacAuthorize]
    [HttpPost("turnOff")]
    public IActionResult TurnOffAutoDetection()
    {
        // TODO: Notify the corresponding services, that this has changed.
        _ebula.AutoDetectState = ServiceState.AlwaysOff;
        return Ok();
    }
    
    [MacAuthorize]
    [HttpPost("turnOn")]
    public IActionResult TurnOnAutoDetection()
    {
        // TODO: Only invoke a rescan if the train isn't moving already
        _ebula.AutoDetectState = ServiceState.AlwaysOn;
        return Ok();
    }
    
    [MacAuthorize]
    [HttpPost("autoDetectionState")]
    public ActionResult<ServiceState> AutoDetectionState()
    {
        return _ebula.AutoDetectState;
    }
    
    /// <summary>
    /// Scans the current page and adds it to the existing table.
    /// </summary>
    [Catch]
    [MacAuthorize]
    [HttpPost("scan")]
    public IActionResult Scan()
    {
        _ebula.ScanCurrentPage();
        return Ok();
    }
    
    /// <summary>
    /// Locks the ebula to not be scanned by any other tasks.
    /// </summary>
    [Catch]
    [MacAuthorize]
    [HttpPost("lock")]
    public IActionResult Lock()
    {
        _ebula.ScanCurrentPage();
        return Ok();
    }
    
    /// <summary>
    /// Unlocks the ebula to be scanned by other tasks again.
    /// </summary>
    [Catch]
    [MacAuthorize]
    [HttpPost("unlock")]
    public IActionResult Unlock()
    {
        _ebula.ScanCurrentPage();
        return Ok();
    }
    
    [MacAuthorize]
    [HttpPost("lockState")]
    public ActionResult<bool> LockState()
    {
        return _ebula.LockState();
    }
    
    [Catch]
    [MacAuthorize]
    [HttpPost("rescan")]
    public IActionResult Rescan()
    {
        // TODO: Implement check for if the train is currently moving. Can't rescan if it is moving
        _ebula.ScanTimetable();
        return Ok();
    }
}