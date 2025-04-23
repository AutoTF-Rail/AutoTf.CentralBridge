using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridgeOS.Models.Interfaces;

/// <summary>
/// Pretty much just servces as a proxy to interact with the AIC (if available (it should))
/// </summary>
public interface IAicService : IHostedService
{
    public bool? Availability { get; }

    /// <summary>
    /// This is just a way to check if the AIC is aware of the CentralBridge being available, and not if the AIC is available. (But it kind of also does both)
    /// </summary>
    public Task<bool?> IsAvailable();

    /// <summary>
    /// Returns the GIT version of the AIC. (Empty if unavailable)
    /// </summary>
    public Task<string> Version();

    public void Shutdown();

    public void Restart();
}