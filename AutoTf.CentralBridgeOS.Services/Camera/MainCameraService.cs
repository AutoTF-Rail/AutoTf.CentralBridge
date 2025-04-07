using System.Net;
using AutoTf.CentralBridgeOS.CameraService;
using AutoTf.CentralBridgeOS.Models;
using AutoTf.Logging;
using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridgeOS.Services.Camera;

public class MainCameraService : IHostedService
{
    private readonly Logger _logger;
    private readonly TrainSessionService _trainSessionService;
    private readonly ProxyManager _proxy;

    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public MainCameraService(Logger logger, TrainSessionService trainSessionService, ProxyManager proxy)
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
        
        _logger.Log($"Using port {port} for main camera proxy.");

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
            _logger.Log("Failed to start up the main camera after 10 tries.");
            return;
        }
        
        _logger.Log($"Main camera is now available after {retryCount} retries.");
        _proxy.StartListeningForCamera(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        try
        {
            _logger.Log("Disposing camera service.");

            _cancellationTokenSource.Cancel();
            _proxy.StopListeningForCamera(IPAddress.Parse("127.0.0.1"));

            _logger.Log("Disposed camera service.");
        }
        catch (Exception e)
        {
            _logger.Log("Failed to dispose main camera service:");
            _logger.Log(e.ToString());
            throw;
        }
    }
}