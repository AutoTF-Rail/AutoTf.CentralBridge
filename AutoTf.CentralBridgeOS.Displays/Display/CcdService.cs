using AutoTf.CentralBridgeOS.CameraService;
using AutoTf.CentralBridgeOS.FahrplanParser;
using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Models.CameraService;
using AutoTf.Logging;
using Emgu.CV;
using Emgu.CV.OCR;
using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridgeOS.Localise.Display;

public class CcdService : IHostedService
{
    private readonly Logger _logger;
    private readonly ProxyManager _proxy;
    private CcdDisplayBase _displayBase;

    public bool Initialized = false;
    
    public CcdService(Logger logger, ITrainModel train, ProxyManager proxy)
    {
        _logger = logger;
        _proxy = proxy;

        _displayBase = train.CcdDisplay;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(Initialize, cancellationToken);
        return Task.CompletedTask;
    }
    
    private async Task Initialize()
    {
        _logger.Log("Waiting for CCD display to be available.");

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
            _logger.Log("Failed to wait for CCD display after 10 tries.");
            return;
        }
        
        _logger.Log($"CCD is now available after {retryCount} retries.");

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