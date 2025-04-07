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
    
    public async Task Initialize()
    {
        _logger.Log("LS: Waiting for EbuLa display to be available.");

        bool isCamAvailable = _proxy.IsDisplayAvailable(DisplayType.EbuLa);
        int retryCount = 0;

        while (!isCamAvailable && retryCount < 10)
        {
            await Task.Delay(1500);
            retryCount++;
            isCamAvailable = _proxy.IsDisplayAvailable(DisplayType.EbuLa);
        }

        if (retryCount == 10)
        {
            _logger.Log("Failed to wait for EbuLa display after 10 tries.");
            return;
        }
        
        _logger.Log($"LS: EbuLa is now available after {retryCount} retries.");

        string? locationMarker = _ebuLaService.LocationMarker();
        if (locationMarker != null)
            _logger.Log($"LS: Found location marker at {locationMarker}.");
        
        _logger.Log($"LS: Current speed limit: {_ebuLaService.CurrentSpeedLimit()}");
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}