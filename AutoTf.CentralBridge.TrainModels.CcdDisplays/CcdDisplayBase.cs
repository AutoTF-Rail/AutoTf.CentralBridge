using AutoTf.CentralBridge.Models.Interfaces;
using Emgu.CV.OCR;

namespace AutoTf.CentralBridge.TrainModels.CcdDisplays;

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