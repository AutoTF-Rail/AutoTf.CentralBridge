using System.Net;
using AutoTf.CentralBridge.Models.Enums;
using AutoTf.CentralBridge.Models.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoTf.CentralBridge.Services.Camera;

public class MainCameraService : IHostedService
{
    private readonly ILogger<MainCameraService> _logger;
    private readonly ITrainSessionService _trainSessionService;
    private readonly IProxyManager _proxy;

    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public MainCameraService(ILogger<MainCameraService> logger, ITrainSessionService trainSessionService, IProxyManager proxy)
    {
        _logger = logger;
        _trainSessionService = trainSessionService;
        _proxy = proxy;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(StartDispatching, cancellationToken);
        
        return Task.CompletedTask;
    }

    private async Task StartDispatching()
    {
        int port = 5000;

        if (_trainSessionService.LocalServiceState == BridgeServiceState.Slave)
            port = 5001;
        
        _logger.LogTrace($"Using port {port} for main camera proxy.");

        bool isCamAvailable = _proxy.IsCameraAvailable();
        int retryCount = 0;

        while (!isCamAvailable && retryCount < 10)
        {
            await Task.Delay(1500);
            retryCount++;
            isCamAvailable = _proxy.IsCameraAvailable();
        }

        if (retryCount == 10)
        {
            _logger.LogError("Failed to start up the main camera after 10 tries.");
            return;
        }

        _logger.LogInformation($"Main camera is now available after {retryCount} retries.");
        _proxy.StartListeningForCamera(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    private void Dispose()
    {
        try
        {
            _logger.LogTrace("Disposing camera service.");

            _cancellationTokenSource.Cancel();
            _proxy.StopListeningForCamera(IPAddress.Parse("127.0.0.1"));

            _logger.LogTrace("Disposed camera service.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to dispose main camera service:");
            throw;
        }
    }
}