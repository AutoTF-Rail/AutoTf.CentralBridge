using AutoTf.CentralBridge.Models.DataModels;
using AutoTf.CentralBridge.Models.Interfaces;
using AutoTf.CentralBridge.Shared.Models.Enums;
using Emgu.CV;
using Microsoft.Extensions.Logging;

namespace AutoTf.CentralBridge.Localise.Display;

public class CcdService : ICcdService
{
    private readonly ILogger _logger;
    private readonly IProxyManager _proxy;
    private ICcdDisplayBase _displayBase;
    
    public CcdService(ILogger logger, ITrainModel train, IProxyManager proxy)
    {
        _logger = logger;
        _proxy = proxy;

        _displayBase = train.CcdDisplay;
    }
    
    public bool Initialized { get; private set; }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(Initialize, cancellationToken);
        return Task.CompletedTask;
    }
    
    private async Task Initialize()
    {
        _logger.LogTrace("Waiting for CCD display to be available.");

        bool isCamAvailable = _proxy.IsDisplayRegistered(DisplayType.CCD);
        int retryCount = 0;

        while (!isCamAvailable && retryCount < 10)
        {
            await Task.Delay(1500);
            retryCount++;
            isCamAvailable = _proxy.IsDisplayRegistered(DisplayType.CCD);
        }

        if (retryCount == 10)
        {
            _logger.LogInformation("Failed to wait for CCD display after 10 tries.");
            return;
        }
        
        _logger.LogInformation($"CCD is now available after {retryCount} retries.");

        Initialized = true;
    }

    public int CurrentSpeed()
    {
        using Mat frame = _proxy.GetLatestFrameFromDisplay(DisplayType.CCD);
        
        // string? speed = _parser.
        return -1;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}