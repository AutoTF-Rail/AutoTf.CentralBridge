using System.ComponentModel.DataAnnotations;
using AutoTf.CentralBridgeOS.Extensions;
using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;
using AutoTf.CentralBridgeOS.Localise.Display;
using AutoTf.CentralBridgeOS.Models;
using AutoTf.Logging;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridgeOS.Server.Controllers.EbuLa;

/// <summary>
/// Documented in OpenApi
/// </summary>
[ApiController]
[Route("ebula/timetable")]
public class TimetableController : ControllerBase
{
    private readonly Logger _logger;
    private readonly EbuLaService _ebula;
    
    public TimetableController(Logger logger, EbuLaService ebula)
    {
        _logger = logger;
        _ebula = ebula;
    }

    [HttpGet("currentTable")]
    public ActionResult<List<KeyValuePair<string, RowContent>>> CurrentTable()
    {
        try
        {
            // TODO: Merge in the speed changes (if we decide to keep this seperate
            return _ebula.CurrentTimetable.ToList();
        }
        catch (Exception e)
        {
            _logger.Log("Something went wrong while supplying the current Table:");
            _logger.Log(e.ToString());
            return BadRequest(e.ToString());
        }
    }

    [MacAuthorize]
    [HttpPost("edit")]
    public IActionResult Edit([FromBody, Required] List<KeyValuePair<string, RowContent>> newTable)
    {
        try
        {
            // TODO: Notify other services that it has changed and location service should recalculate the current position. (This probably isn't allowed to happen while moving)
            _ebula.OverwriteTable(newTable);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.Log("Something went wrong while editing the current timetable:");
            _logger.Log(e.ToString());
            return BadRequest(e.ToString());
        }
    }

    [HttpGet("currentConditions")]
    public ActionResult<List<KeyValuePair<string, RowContent>>> CurrentConditions()
    {
        try
        {
            // TODO: Grab current conditions from _ebula, and maybe rework the return type, so that we can represent things like speed changes better? (Or just supply the next change too?)
            return new List<KeyValuePair<string, RowContent>>();
        }
        catch (Exception e)
        {
            _logger.Log("Something went wrong when supplying the current conditions:");
            _logger.Log(e.ToString());
            return BadRequest(e.ToString());
        }
    }

    /// <summary>
    /// Disables the auto detection of EbuLa changes for this session.
    /// (TODO: does this even ever happen? But it could disable the "validation" of the EbuLa while driving)
    /// </summary>
    [MacAuthorize]
    [HttpPost("disableAutoDetection")]
    public IActionResult DisableAutoDetection()
    {
        try
        {
            // TODO: save the bool in _ebula, so that others can access it via that. This doesn't need to be saved to a file.
            _ebula.AutoDetectState = ServiceState.TemporaryOff;
            return Ok();
        }
        catch (Exception e)
        {
            _logger.Log("Something went wrong when disabling auto timetable detection:");
            _logger.Log(e.ToString());
            return BadRequest(e.ToString());
        }
    }
    
    [MacAuthorize]
    [HttpPost("enableAutoDetection")]
    public IActionResult EnableAutoDetection()
    {
        try
        {
            // TODO: Save this as a bool in _ebula and notify the needed services. This doesn't need to be saved to a file.
            // TODO: Invoke a full rescan of the timetable
            _ebula.AutoDetectState = ServiceState.TemporaryOn;
            return Ok();
        }
        catch (Exception e)
        {
            _logger.Log("Something went wrong when enabling auto timetable detection:");
            _logger.Log(e.ToString());
            return BadRequest(e.ToString());
        }
    }
    
    /// <summary>
    /// Permanently turns off the auto detection of a timetable from the EbuLa.
    /// (TODO: Although we will probably still want to enter the train number into the device on startup)
    /// </summary>
    [MacAuthorize]
    [HttpPost("turnOffAutoDetection")]
    public IActionResult TurnOffAutoDetection()
    {
        try
        {
            // TODO: Notify the corresponding services, that this has changed.
            // TODO: Save/update this bool in _ebula.
            _ebula.AutoDetectState = ServiceState.AlwaysOff;
            return Ok();
        }
        catch (Exception e)
        {
            _logger.Log("Something went wrong when turning off auto timetable detection:");
            _logger.Log(e.ToString());
            return BadRequest(e.ToString());
        }
    }
    
    [MacAuthorize]
    [HttpPost("turnOnAutoDetection")]
    public IActionResult TurnOnAutoDetection()
    {
        try
        {
            // TODO: Notify _ebula and needed services that this has changed.
            // TODO: Invoke a full rescan of the timetable
            _ebula.AutoDetectState = ServiceState.AlwaysOn;
            return Ok();
        }
        catch (Exception e)
        {
            _logger.Log("Something went wrong when turning on auto timetable detection:");
            _logger.Log(e.ToString());
            return BadRequest(e.ToString());
        }
    }
    
    [MacAuthorize]
    [HttpPost("autoDetectionState")]
    public ActionResult<ServiceState> AutoDetectionState()
    {
        try
        {
            return _ebula.AutoDetectState;
        }
        catch (Exception e)
        {
            _logger.Log("Something went wrong when supplying the auto timetable detection state:");
            _logger.Log(e.ToString());
            return BadRequest(e.ToString());
        }
    }
    
    [MacAuthorize]
    [HttpPost("rescan")]
    public IActionResult Rescan()
    {
        try
        {
            // TODO: Implement check for if the train is currently moving. Can't rescan if it is moving
            _ebula.ScanTimetable();
            return Ok();
        }
        catch (Exception e)
        {
            _logger.Log("Something went wrong when rescanning the timetable:");
            _logger.Log(e.ToString());
            return BadRequest(e.ToString());
        }
    }
}