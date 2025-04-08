using AutoTf.CentralBridgeOS.CameraService;
using AutoTf.CentralBridgeOS.Models.CameraService;
using AutoTf.Logging;
using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridgeOS.Localise;

public class LocaliseService : IHostedService
{
    private readonly Logger _logger;
    private readonly ProxyManager _proxy;
    private readonly EbuLaService _ebuLaService;

    public bool? StartSuccess;

    public string LocationMarker { get; private set; }

    private bool _canRun = true;
    
    public LocaliseService(Logger logger, ProxyManager proxy, EbuLaService ebuLaService)
    {
        _logger = logger;
        _proxy = proxy;
        _ebuLaService = ebuLaService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(Initialize, cancellationToken);
        
        return Task.CompletedTask;
    }

    private async Task Initialize()
    {
        _logger.Log("Waiting for EbuLa display to be available.");

        bool isCamAvailable = _proxy.IsDisplayRegistered(DisplayType.EbuLa);
        int retryCount = 0;

        while (!isCamAvailable && retryCount < 10)
        {
            await Task.Delay(1500);
            retryCount++;
            isCamAvailable = _proxy.IsDisplayRegistered(DisplayType.EbuLa);
        }

        if (retryCount == 10)
        {
            _logger.Log("Failed to wait for EbuLa display after 10 tries.");
            StartSuccess = false;
            return;
        }
        
        _logger.Log($"EbuLa is now available after {retryCount} retries.");

        string? locationMarker = _ebuLaService.LocationMarker();
        
        if (locationMarker != null)
            _logger.Log($"Found location marker at {locationMarker}.");
        
        _logger.Log($"Current speed limit: {_ebuLaService.CurrentSpeedLimit()}");

        StartSuccess = true;
        await Task.Run(LoopLocationMarker);
    }

    private void LoopLocationMarker()
    {
        while (_canRun)
        {
            string? locationMarker = _ebuLaService.LocationMarker();
            if (!string.IsNullOrEmpty(locationMarker))
            {
                locationMarker = locationMarker.Trim();

                if (locationMarker != LocationMarker)
                {
                    LocationMarker = locationMarker;
                }
            }
            
            Thread.Sleep(500);
        }
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _canRun = false;
        return Task.CompletedTask;
    }
}