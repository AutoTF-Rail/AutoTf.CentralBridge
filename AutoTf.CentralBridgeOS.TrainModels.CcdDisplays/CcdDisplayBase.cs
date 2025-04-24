using AutoTf.CentralBridgeOS.Models.Interfaces;
using Emgu.CV.OCR;

namespace AutoTf.CentralBridgeOS.TrainModels.CcdDisplays;

public abstract class CcdDisplayBase : ICcdDisplayBase
{
    private readonly Tesseract _engine;

    protected CcdDisplayBase()
    {
        _engine = new Tesseract(Path.Combine(AppContext.BaseDirectory, "tessdata"), "deu", OcrEngineMode.LstmOnly);
    }

    public void Dispose()
    {
        _engine.Dispose();
    }
}