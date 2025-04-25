using AutoTf.CentralBridge.Models.CameraService;
using AutoTf.CentralBridge.Models.Interfaces;
using AutoTf.CentralBridge.Services.Gps;
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
    public ActionResult<List<KeyValuePair<DisplayType, bool>>> DisplayStatus()
    {
        return _proxy.DisplaysStatus();
    }
    
    [HttpGet("mainCamera")]
    public ActionResult<bool?> MainCameraStatus()
    {
        return _proxy.MainCameraStatus();
    }
    
    [HttpGet("localise")]
    public ActionResult<bool?> Localise()
    {
        return _localiseService.StartSuccess;
    }
    
    [HttpGet("gpsAvailable")]
    public ActionResult<bool> GpsAvailable()
    {
        return _motionService.IsGpsAvailable;
    }
    
    [HttpGet("gpsConnected")]
    public ActionResult<bool> GpsConnected()
    {
        return _motionService.IsGpsConnected;
    }
}