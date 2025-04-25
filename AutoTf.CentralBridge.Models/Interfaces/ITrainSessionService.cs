using AutoTf.CentralBridge.Models.Enums;
using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridge.Models.Interfaces;

public interface ITrainSessionService
{
    public string Username { get; }

    public string Password { get; }

    public string EvuName { get; }
        
    public string Ssid { get; }
    
    public BridgeServiceState LocalServiceState { get; }
}