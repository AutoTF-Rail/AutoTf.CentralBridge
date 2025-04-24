using AutoTf.CentralBridgeOS.Models.CameraService;
using AutoTf.CentralBridgeOS.Models.DataModels;
using AutoTf.CentralBridgeOS.Models.Interfaces;
using AutoTf.Logging;
using Emgu.CV;

namespace AutoTf.CentralBridgeOS.Localise.Display;

public class CcdService : ICcdService
{
    private readonly Logger _logger;
    private readonly IProxyManager _proxy;
    private ICcdDisplayBase _displayBase;
    
    public CcdService(Logger logger, ITrainModel train, IProxyManager proxy)
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