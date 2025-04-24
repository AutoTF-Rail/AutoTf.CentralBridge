using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridgeOS.Models.Interfaces;

/// <summary>
/// This class is only a bridge of sorts to make life easier, you should still make sure that the display is available yourself.
/// This class is also thread safe, so you could read the current speed limit and get the location marker at the same time (One will just block the other in the mean time)
/// </summary>
public interface IEbuLaService : IHostedService
{
    public bool Started { get; } 
    public bool Disabled { get; }
    public IReadOnlyList<KeyValuePair<string, IRowContent>> CurrentTimetable { get; }
    
    /// <summary>
    /// This bool pretty much inverts IsAutoDetectionTurnedOff
    /// If IsAutoDetectionTurnedOff == true && ManualAutoDetectionState == true: Auto detection has been turned on for this session
    /// If IsAutoDetectionTurnedOff == true && ManualAutoDetectionState == false: Auto detection is permanently off.
    /// If IsAutoDetectionTurnedOff == false && ManualAutoDetectionState == true: Auto detection has been turned off for this session.
    /// If IsAutoDetectionTurnedOff == false && ManualAutoDetectionState == false: Auto detection is permanently on.
    /// </summary>
    public bool ManualDetectionState { get; }
    
    /// <summary>
    /// Explained above ManualDetectionState, might rework in the future to make it a bit more simple.
    /// </summary>
    public ServiceState AutoDetectState { get; set; }

    public void OverwriteTable(List<KeyValuePair<string, IRowContent>> newTable);

    /// <summary>
    /// Scans the entire timetable (and automatically scrolls (TBA)) and resets the CurrentTimetable col to it.
    /// </summary>
    public void ScanTimetable();

    /// <summary>
    /// To scan a entire timetable without a motor, turn off localisation, reset to page one, scan, manually press to get to the next page, scan again etc. using this method
    /// This method does NOT lock localisation
    /// </summary>
    public void ScanCurrentPage();

    public void LockScan();

    public void UnlockScan();
    
    public bool LockState();

    public string? LocationMarker();

    public string CurrentSpeedLimit();
}