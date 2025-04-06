using AutoTf.CentralBridgeOS.CameraService;
using AutoTf.CentralBridgeOS.FahrplanParser;
using AutoTf.CentralBridgeOS.Models.CameraService;
using AutoTf.CentralBridgeOS.TrainModels;
using AutoTf.Logging;
using Emgu.CV;
using Emgu.CV.OCR;
using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridgeOS.Localise;

/// <summary>
/// This class is only a bridge of sorts to make life easier, you should still make sure that the display is available yourself.
/// </summary>
public class EbuLaService : IHostedService
{
    private readonly Logger _logger;
    private readonly ITrainModel _train;
    private readonly ProxyManager _proxy;
    private Tesseract _engine;
    private Parser _parser;

    public EbuLaService(Logger logger, ITrainModel train, ProxyManager proxy)
    {
        _logger = logger;
        _train = train;
        _proxy = proxy;

        _engine = new Tesseract(Path.Combine(AppContext.BaseDirectory, "tessdata"), "deu", OcrEngineMode.LstmOnly);
        _parser = new Parser(_engine);
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // TODO: e.g. enter train number via _train (motors)
        // Read from screen etc
        
        // For now we just imagine it's already entered in and we see the current screen
        return Task.CompletedTask;
    }

    public string? LocationMarker()
    {
        // TODO: Press button to go to current location on EbuLa

        using Mat frame = _proxy.GetLatestFrameFromDisplay(DisplayType.EbuLa);

        string? location = _parser.Location(frame);
        
        if(string.IsNullOrEmpty(location))
            _logger.Log("EbuLa: Warning: Could not read location.");

        return location;
    }

    public string CurrentSpeedLimit()
    {
        // TODO: Press button to go to current location on EbuLa

        using Mat frame = _proxy.GetLatestFrameFromDisplay(DisplayType.EbuLa);

        string limit = _parser.SpeedLimit(frame, RegionMappings.Rows.Last());
        
        return limit;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _engine.Dispose();
        return Task.CompletedTask;
    }
}