using AutoTf.CentralBridge.FahrplanParser;
using AutoTf.CentralBridge.Models.CameraService;
using AutoTf.CentralBridge.Models.DataModels;
using AutoTf.CentralBridge.Models.Enums;
using AutoTf.CentralBridge.Models.Interfaces;
using AutoTf.FahrplanParser.Content;
using AutoTf.Logging;
using Emgu.CV;
using Emgu.CV.OCR;

namespace AutoTf.CentralBridge.Localise.Display;

public class EbuLaService : IEbuLaService
{
    private readonly Logger _logger;
    private readonly ITrainModel _train;
    private readonly IProxyManager _proxy;
    private readonly IFileManager _fileManager;
    private Tesseract _engine;
    private Parser _parser;
    private bool? _cachedTurnedOffState;

    private ManualResetEvent _ebulaScanBlock = new ManualResetEvent(true);
    private ManualResetEvent _ebulaManualScanBlock = new ManualResetEvent(true);
    private bool _isManuallyBlocked = false;

    private ManualResetEvent _ebulaReadBlock = new ManualResetEvent(true);
    
    public EbuLaService(Logger logger, ITrainModel train, IProxyManager proxy, IFileManager fileManager)
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

    public bool Started { get; private set; } = false;

    public bool Disabled { get; } = false;

    public IReadOnlyList<KeyValuePair<string, IRowContent>> CurrentTimetable { get; private set; } = new List<KeyValuePair<string, IRowContent>>();

    public bool ManualDetectionState { get; private set; }

    private bool IsAutoDetectionTurnedOff
    {
        get
        {
            if (_cachedTurnedOffState == null)
                _cachedTurnedOffState = bool.Parse(_fileManager.ReadFile("autoDetectionTurnedOff", "false"));

            return (bool)_cachedTurnedOffState;
        }
        set
        {
            if (_cachedTurnedOffState == value)
                return;

            _cachedTurnedOffState = value;
            _fileManager.WriteAllText("autoDetectionTurnedOff", value.ToString());
        }
    }

    public ServiceState AutoDetectState
    {
        get
        {
            if (IsAutoDetectionTurnedOff)
            {
                return ManualDetectionState ? ServiceState.TemporaryOn : ServiceState.AlwaysOff;
            }

            return ManualDetectionState ? ServiceState.TemporaryOff : ServiceState.AlwaysOn;
        }
        set
        {
            // TODO: Make this setter better/smaller
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
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // TODO: e.g. enter train number via _train (motors)
        // Read from screen etc
        
        // For now we just imagine it's already entered in and we see the current screen
        Task.Run(Initialize, cancellationToken);
        return Task.CompletedTask;
    }

    public void OverwriteTable(List<KeyValuePair<string, IRowContent>> newTable)
    {
        _logger.Log("Overwriting current timetable.");
        CurrentTimetable = newTable;
    }

    public void ScanTimetable()
    {
        // TODO: Check if train is moving, if true: return
        
        // We are blocking the other methods here, to ensure that they only grab the current page, and not a random one when we switch though them here.
        _ebulaReadBlock.WaitOne();
        _ebulaManualScanBlock.WaitOne();
        _ebulaScanBlock.Reset();
        
        // Here would be the point to scroll to the next page, and take another frame.
        using Mat frame = _proxy.GetLatestFrameFromDisplay(DisplayType.EbuLa);
        _ebulaScanBlock.Set();

        List<KeyValuePair<string, IRowContent>> rows = CurrentTimetable.ToList();
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

    public void ScanCurrentPage()
    {
        using Mat frame = _proxy.GetLatestFrameFromDisplay(DisplayType.EbuLa);
        List<KeyValuePair<string, IRowContent>> rows = CurrentTimetable.ToList();
        List<KeyValuePair<string, string>> speedChanges = new List<KeyValuePair<string, string>>();
       
        lock (_parser)
        {
            _parser.ReadPage(frame, ref rows, ref speedChanges);
        }

        // TODO: Include speed changes, or save them separately 
        // TODO: Notify depending services of a change?
        CurrentTimetable = rows;
    }

    public void LockScan()
    {
        _ebulaManualScanBlock.Set();
        _isManuallyBlocked = true;
    }

    public void UnlockScan()
    {
        _ebulaManualScanBlock.Reset();
        _isManuallyBlocked = false;
    }

    public bool LockState()
    {
        return _isManuallyBlocked;
    }

    public string? LocationMarker()
    {
        // TODO: Press button to go to current location on EbuLa
        _ebulaScanBlock.WaitOne();
        _ebulaManualScanBlock.WaitOne();
        
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
        _ebulaManualScanBlock.WaitOne();
        
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