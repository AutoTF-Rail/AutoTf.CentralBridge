using System.Net;
using AutoTf.CentralBridge.Models.CameraService;
using AutoTf.CentralBridge.Models.DataModels;
using AutoTf.CentralBridge.Shared.Models.Enums;
using AutoTf.Logging;
using Emgu.CV;

namespace AutoTf.CentralBridge.Models.Interfaces;

public interface IProxyManager : IDisposable
{
    public Task CreateProxy(int port, bool b, Logger logger, ITrainModel train);
    public void StartListeningForDisplay(DisplayType type, IPEndPoint endpoint);
    
    public void StopListeningForDisplay(DisplayType type, IPAddress address);
    
    public bool IsDisplayRegistered(DisplayType type);

    /// <summary>
    /// Returns a keyvaluepair representing the type of display, and if it is running or not.
    /// </summary>
    public List<KeyValuePair<DisplayType, bool>> DisplaysStatus();

    public bool? MainCameraStatus();

    public bool IsCameraAvailable();

    public void StartListeningForCamera(IPEndPoint endPoint);

    public Mat GetLatestFrameFromDisplay(DisplayType type);

    public void StopListeningForCamera(IPAddress address);
}