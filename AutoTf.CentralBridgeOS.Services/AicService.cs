using System.Timers;
using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Models.Interfaces;
using AutoTf.Logging;
using Timer = System.Timers.Timer;

namespace AutoTf.CentralBridgeOS.Services;

public class AicService : IAicService
{
    private readonly Logger _logger;
    private const string AicEndpoint = "http://192.168.0.3";
    
    private readonly Timer _availabilityTimer = new Timer(15000);

    public AicService(Logger logger)
    {
        _logger = logger;
    }
    
    #region Implementations
    
    public bool? Availability { get; private set; }
    
    public async Task<bool?> IsAvailable() => await HttpHelper.SendGet<bool?>(AicEndpoint + "/system/available", false);

    public async Task<string> Version() => await HttpHelper.SendGet<string>(AicEndpoint + "/system/version", false) ?? "";

    public void Shutdown() => _ = HttpHelper.SendPost(AicEndpoint + "/system/shutdown", new StringContent(""), false);

    public void Restart() => _ = HttpHelper.SendPost(AicEndpoint + "/system/restart", new StringContent(""), false);
    
    #endregion

    public Task StartAsync(CancellationToken cancellationToken)
    {
        ConfigureTimer();
        return Task.CompletedTask;
    }

    private void ConfigureTimer()
    {
        AvailabilityTimerOnElapsed(null, null!);
        _availabilityTimer.Elapsed += AvailabilityTimerOnElapsed;
        _availabilityTimer.Start();
    }

    private async void AvailabilityTimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        bool? newState = await IsAvailable();

        if (Availability != newState)
        {
            _logger.Log($"Verbose: AIC online state has changed to: Online: {newState}.");
        }

        Availability = newState;
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _availabilityTimer.Stop();
        _availabilityTimer.Dispose();
        return Task.CompletedTask;
    }
}