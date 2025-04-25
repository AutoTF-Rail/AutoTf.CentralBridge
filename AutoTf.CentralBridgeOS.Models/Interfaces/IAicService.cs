using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridgeOS.Models.Interfaces;

/// <summary>
/// Pretty much just servces as a proxy to interact with the AIC (if available (it should))
/// </summary>
public interface IAicService : IHostedService
{
    /// <summary>
    /// Represents the newest cached online state (Rechecked every 15 seconds).
    /// </summary>
    public bool Online { get; }

    /// <summary>
    /// This is just a way to check if the AIC is aware of the CentralBridge being available, and not if the AIC is available.
    /// </summary>
    public Task<bool?> IsAvailable();
    
    /// <summary>
    /// To avoid too much network usage, the Online field serves as a cached alternative to this method. 
    /// </summary>
    public Task<bool> IsOnline();

    /// <summary>
    /// Returns the GIT version of the AIC. (Empty if unavailable)
    /// </summary>
    public Task<string> Version();

    /// <summary>
    /// Returns all available log dates.
    /// </summary>
    public Task<string[]> LogDates();

    /// <summary>
    /// Returns the logs for the given date
    /// </summary>
    public Task<string[]> Logs(string date);

    public void Shutdown();

    public void Restart();
}