using AutoTf.CentralBridgeOS.CameraService;
using AutoTf.CentralBridgeOS.Localise;
using AutoTf.CentralBridgeOS.Models.CameraService;
using AutoTf.CentralBridgeOS.Services.Gps;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.CentralBridgeOS.Server.Controllers;

[ApiController]
[Route("/status")]
public class StatusController : ControllerBase
{
    private readonly ProxyManager _proxy;
    private readonly LocaliseService _localiseService;
    private readonly MotionService _motionService;

    public StatusController(ProxyManager proxy, LocaliseService localiseService, MotionService motionService)
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
    
    [HttpGet("localiser")]
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
    public ActionResult<bool?> GpsConnected()
    {
        return _motionService.IsGpsConnected;
    }
}