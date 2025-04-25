using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridge.Models.Interfaces;

public interface ICcdService : IHostedService
{
    public bool Initialized { get; }
    public int CurrentSpeed();
}