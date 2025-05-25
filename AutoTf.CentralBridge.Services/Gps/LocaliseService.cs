using AutoTf.CentralBridge.Models.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoTf.CentralBridge.Services.Gps;

public class LocaliseService : IHostedService
{
    private readonly ILogger<LocaliseService> _logger;
    private readonly IEbuLaService _ebuLaService;
    private readonly ICcdService _ccdService;

    public bool? StartSuccess;

    public string LocationMarker { get; private set; } = string.Empty;

    private bool _canRun = true;
    
    public LocaliseService(ILogger<LocaliseService> logger, IEbuLaService ebuLaService, ICcdService ccdService)
    {
        _logger = logger;
        _ebuLaService = ebuLaService;
        _ccdService = ccdService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(Initialize, cancellationToken);
        
        return Task.CompletedTask;
    }

    private async Task Initialize()
    {
        int retryCount = 0;

        while (!_ebuLaService.Started && !_ccdService.Initialized && retryCount < 15)
        {
            await Task.Delay(1500);
            retryCount++;
        }

        // Maybe we would want to use this in the future, to ensure system safety, that everything is available, but for development this is just annoying
        // if (retryCount == 15 || !_ebuLaService.Initialized || !_ccdService.Initialized)
        // {
        //     _logger.Log("Exited localise service startup due to EbuLa or CCD display not being available.");
        //     StartSuccess = false;
        //     return;
        // }
        
        if(_ebuLaService.Started)
            await Task.Run(LoopLocationMarker);
        
        // if(_ccdService.Initialized)
        //     await Task.Run(LoopLocationMarker);
        //
        StartSuccess = true;
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
                    _logger.LogTrace("Location marker has changed to: " + LocationMarker);
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