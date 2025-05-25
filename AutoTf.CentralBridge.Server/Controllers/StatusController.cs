using AutoTf.CentralBridge.Models.Interfaces;
using AutoTf.CentralBridge.Services.Gps;
using AutoTf.CentralBridge.Shared.Models;
using AutoTf.CentralBridge.Shared.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridge.Server.Controllers;

[ApiController]
[Route("/status")]
public class StatusController : ControllerBase
{
    private readonly IProxyManager _proxy;
    private readonly LocaliseService _localiseService;
    private readonly MotionService _motionService;

    public StatusController(IProxyManager proxy, LocaliseService localiseService, MotionService motionService)
    {
        _proxy = proxy;
        _localiseService = localiseService;
        _motionService = motionService;
    }
    
    [HttpGet("displays")]
    public Result<List<KeyValuePair<DisplayType, bool>>> DisplayStatus()
    {
        return Result<List<KeyValuePair<DisplayType, bool>>>.Ok(_proxy.DisplaysStatus());
    }
    
    [HttpGet("mainCamera")]
    public Result<bool> MainCameraStatus()
    {
        return _proxy.MainCameraStatus();
    }
    
    [HttpGet("localise")]
    public Result<bool?> Localise()
    {
        return _localiseService.StartSuccess;
    }
    
    [HttpGet("gpsAvailable")]
    public Result<bool> GpsAvailable()
    {
        return _motionService.IsGpsAvailable;
    }
    
    [HttpGet("gpsConnected")]
    public Result<bool> GpsConnected()
    {
        return _motionService.IsGpsConnected;
    }
}