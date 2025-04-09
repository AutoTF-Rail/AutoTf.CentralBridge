using AutoTf.CentralBridgeOS.CameraService;
using AutoTf.CentralBridgeOS.FahrplanParser;
using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;
using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Models.CameraService;
using AutoTf.CentralBridgeOS.Models.Interfaces;
using AutoTf.Logging;
using Emgu.CV;
using Emgu.CV.OCR;
using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridgeOS.Localise.Display;

/// <summary>
/// This class is only a bridge of sorts to make life easier, you should still make sure that the display is available yourself.
/// This class is also thread safe, so you could read the current speed limit and get the location marker at the same time (One will just block the other in the mean time)
/// </summary>
public class EbuLaService : IHostedService
{
    private readonly Logger _logger;
    private readonly ITrainModel _train;
    private readonly ProxyManager _proxy;
    private readonly IFileManager _fileManager;
    private Tesseract _engine;
    private Parser _parser;
    private bool? _cachedTurnedOffState;

    private ManualResetEvent _ebulaScanBlock = new ManualResetEvent(true);

    private ManualResetEvent _ebulaReadBlock = new ManualResetEvent(true);

    public bool Started = false;

    public bool Disabled = false;

    public IReadOnlyList<KeyValuePair<string, RowContent>> CurrentTimetable { get; private set; } = new List<KeyValuePair<string, RowContent>>();

    /// <summary>
    /// This bool pretty much inverts IsAutoDetectionTurnedOff
    /// If IsAutoDetectionTurnedOff == true && ManualAutoDetectionState == true: Auto detection has been turned on for this session
    /// If IsAutoDetectionTurnedOff == true && ManualAutoDetectionState == false: Auto detection is permanently off.
    /// If IsAutoDetectionTurnedOff == false && ManualAutoDetectionState == true: Auto detection has been turned off for this session.
    /// If IsAutoDetectionTurnedOff == false && ManualAutoDetectionState == false: Auto detection is permanently on.
    /// </summary>
    public bool ManualDetectionState = false;

    private bool IsAutoDetectionTurnedOff
    {
        get
        {
            if (_cachedTurnedOffState == null)
                _cachedTurnedOffState = bool.Parse(_fileManager.ReadFile("autoDetectionEnabled", "false"));

            return (bool)_cachedTurnedOffState;
        }
        set
        {
            if (_cachedTurnedOffState == value)
                return;

            _cachedTurnedOffState = value;
            _fileManager.WriteAllText("autoDetectionEnabled", value.ToString());
        }
    }

    /// <summary>
    /// Explained above ManualDetectionState, might rework in the future to make it a bit more simple.
    /// </summary>
    public ServiceState AutoDetectState
    {
        get
        {
            if (IsAutoDetectionTurnedOff)
            {
                return ManualDetectionState ? ServiceState.TemporaryOn : ServiceState.AlwaysOff;
            }
            else
            {
                return ManualDetectionState ? ServiceState.TemporaryOff : ServiceState.AlwaysOn;
            }
        }
        set
        {
            if (AutoDetectState == value) 
                return;

            if (IsAutoDetectionTurnedOff)
            {
                switch (value)
                {
                    case ServiceState.TemporaryOn:
                        ManualDetectionState = true;
                        break;
                    case ServiceState.AlwaysOff:
                        return;
                }
            }
            else
            {
                if (value == ServiceState.TemporaryOff)
                    ManualDetectionState = true;
                else if (value == ServiceState.AlwaysOn)
                    return;
            }
            
            switch (value)
            {
                case ServiceState.AlwaysOn:
                    IsAutoDetectionTurnedOff = false;
                    ManualDetectionState = false;
                    break;
                case ServiceState.AlwaysOff:
                    IsAutoDetectionTurnedOff = true;
                    ManualDetectionState = false;
                    break;
                case ServiceState.TemporaryOn:
                    IsAutoDetectionTurnedOff = false;
                    ManualDetectionState = true;
                    break;
                case ServiceState.TemporaryOff:
                    IsAutoDetectionTurnedOff = true;
                    ManualDetectionState = true;
                    break;
            }
        }
    }
    
    public EbuLaService(Logger logger, ITrainModel train, ProxyManager proxy, IFileManager fileManager)
    {
        _logger = logger;
        _train = train;
        _proxy = proxy;
        _fileManager = fileManager;

        _engine = new Tesseract(Path.Combine(AppContext.BaseDirectory, "tessdata"), "deu", OcrEngineMode.LstmOnly);
        _parser = new Parser(_engine, train);
    }
    
    private async Task Initialize()
    {
        _logger.Log("Waiting for EbuLa display to be available.");

        bool isCamAvailable = _proxy.IsDisplayRegistered(DisplayType.EbuLa);
        int retryCount = 0;

        while (!isCamAvailable && retryCount < 10)
        {
            await Task.Delay(1500);
            retryCount++;
            isCamAvailable = _proxy.IsDisplayRegistered(DisplayType.EbuLa);
        }

        if (retryCount == 10)
        {
            _logger.Log("Failed to wait for EbuLa display after 10 tries.");
            return;
        }
        
        _logger.Log($"EbuLa is now available after {retryCount} retries.");

        Started = true;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // TODO: e.g. enter train number via _train (motors)
        // Read from screen etc
        
        // For now we just imagine it's already entered in and we see the current screen
        Task.Run(Initialize, cancellationToken);
        return Task.CompletedTask;
    }

    public void OverwriteTable(List<KeyValuePair<string, RowContent>> newTable)
    {
        _logger.Log("Overwriting current timetable.");
        CurrentTimetable = newTable;
    }

    public void ScanTimetable()
    {
        // TODO: Check if train is moving, if true: return
        _ebulaReadBlock.WaitOne();
        _ebulaScanBlock.Reset();
        
        // TODO: Reset to page one
        // TODO: Somehow figure out how many pages the timetable has, and then skip though each and take a picture. (But take all of the pictures first, and then process them, as to not block the other tasks)

        using Mat frame = _proxy.GetLatestFrameFromDisplay(DisplayType.EbuLa);
        _ebulaScanBlock.Set();

        List<KeyValuePair<string, RowContent>> rows = new List<KeyValuePair<string, RowContent>>();
        List<KeyValuePair<string, string>> speedChanges = new List<KeyValuePair<string, string>>();
        
        lock (_parser)
        {
            // TODO: Create a different parser just for this task? So that other tasks can continue without being blocked by this.
            _parser.ReadPage(frame, ref rows, ref speedChanges);
        }

        // TODO: Include speed changes, or save them separately 
        // TODO: Notify depending services of a change?
        CurrentTimetable = rows;
    }

    public string? LocationMarker()
    {
        // TODO: Press button to go to current location on EbuLa
        _ebulaScanBlock.WaitOne();
        
        _ebulaReadBlock.Reset();
        using Mat frame = _proxy.GetLatestFrameFromDisplay(DisplayType.EbuLa);
        _ebulaReadBlock.Set();

        string? location;
        lock (_parser)
        {
            location = _parser.Location(frame);
        }

        return location;
    }

    public string CurrentSpeedLimit()
    {
        // TODO: Press button to go to current location on EbuLa
        
        _ebulaScanBlock.WaitOne();
        
        _ebulaReadBlock.Reset();
        using Mat frame = _proxy.GetLatestFrameFromDisplay(DisplayType.EbuLa);
        _ebulaReadBlock.Set();

        string limit;
        lock (_parser)
        {
             limit = _parser.SpeedLimit(frame, _train.Mappings.Rows.Last()); 
        }
        
        return limit;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _engine.Dispose();
        return Task.CompletedTask;
    }
}