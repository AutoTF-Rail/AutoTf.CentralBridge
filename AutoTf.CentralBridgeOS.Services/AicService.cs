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
    
    private readonly Timer _onlineTimer = new Timer(15000);

    public AicService(Logger logger)
    {
        _logger = logger;
    }
    
    #region Implementations
    
    public bool Online { get; private set; }
    
    public async Task<bool?> IsAvailable() => await HttpHelper.SendGet<bool?>(AicEndpoint + "/system/available", false);
    public async Task<bool> IsOnline()
    {
        return await HttpHelper.SendGet<bool>(AicEndpoint + "/system/", false, 2);
    }

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
        OnlineTimerOnElapsed(null, null!);
        _onlineTimer.Elapsed += OnlineTimerOnElapsed;
        _onlineTimer.Start();
    }

    private async void OnlineTimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        bool newState = await IsOnline();

        if (Online != newState)
        {
            _logger.Log($"Verbose: AIC online state has changed to: Online: {newState}.");
        }

        Online = newState;
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _onlineTimer.Stop();
        _onlineTimer.Dispose();
        return Task.CompletedTask;
    }
}