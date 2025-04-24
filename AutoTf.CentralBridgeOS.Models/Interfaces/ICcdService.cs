using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridgeOS.Models.Interfaces;

public interface ICcdService : IHostedService
{
    public bool Initialized { get; }
    public int CurrentSpeed();
}