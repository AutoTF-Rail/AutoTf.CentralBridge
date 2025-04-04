using System.Net;
using AutoTf.CentralBridgeOS.CameraService;
using AutoTf.CentralBridgeOS.Models;
using AutoTf.Logging;
using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridgeOS.Services;

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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await StartDispatching();
    }

    private async Task StartDispatching()
    {
        int port = 5000;

        if (_trainSessionService.LocalServiceState == BridgeServiceState.Slave)
            port = 5001;
        
        _logger.Log($"CS: Using port {port} for main camera proxy.");

        bool isCamAvailable = _proxy.IsCameraAvailable();

        while (!isCamAvailable)
        {
            _logger.Log("Main camera is not available yet, retrying in 1.5 seconds...");
            await Task.Delay(1500);
            isCamAvailable = _proxy.IsCameraAvailable();
        }
        
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
            _logger.Log("CS: Disposing camera service.");

            _cancellationTokenSource.Cancel();
            _proxy.StopListeningForCamera(IPAddress.Parse("127.0.0.1"));

            _logger.Log("CS: Disposed camera service.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}