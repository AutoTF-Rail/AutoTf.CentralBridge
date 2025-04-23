using Emgu.CV.OCR;

namespace AutoTf.CentralBridgeOS.Models;

public abstract class CcdDisplayBase : IDisposable
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