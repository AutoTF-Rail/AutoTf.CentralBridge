using System.Drawing;
using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content;
using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Models.Interfaces;
using Emgu.CV;
using Emgu.CV.OCR;

namespace AutoTf.CentralBridgeOS.FahrplanParser;

public abstract class ParserBase
{
	protected readonly Tesseract Engine;
	protected readonly ITrainModel Train;

	protected ParserBase(Tesseract engine, ITrainModel train)
	{
		Engine = engine;
		Train = train;
	}

	protected bool TryParseTunnel(Mat mat, Rectangle row, string additionalText, out IRowContent? content) =>
		ContentResolver.TryParseTunnel(mat, row, additionalText, out content);

	protected bool TryParseIcon(Mat mat, Rectangle row, out IRowContent? content) =>
		ContentResolver.TryParseIcon(mat, row, out content);

	protected IRowContent? ResolveContent(string additionalText, string arrivalTime, string departureTime)
	{
		if (arrivalTime.Contains("*1)")) // Are multiple numbers stacked on the arrival time?
			return null;
		
		if (ContentResolver.TryParseSignal(additionalText, out IRowContent? signalContent))
			return signalContent!;
		
		if (ContentResolver.TryParseMarker(additionalText, out IRowContent? markerContent))
			return markerContent!;
		
		// Important to do this AFTER the markers, because Abzw and others could have a departure time too
		if (ContentResolver.TryParseStation(additionalText, arrivalTime, departureTime, out IRowContent? stationContent))
			return stationContent!;
		
		return new UnknownContent(additionalText);
	}

	protected static string ExtractTextClean(Rectangle roi, Mat mat, Tesseract engine) => ExtractText(roi, mat, engine).Replace("\n", "");

	protected static string ExtractText(Rectangle roi, Mat mat, Tesseract engine)
	{
		using Mat roiMat = new Mat(mat, roi);
		using Pix pix = new Pix(roiMat);
		
		engine.SetImage(pix);
		
		return engine.GetUTF8Text().Trim();
	}
	
	public IRowContent? CheckForDuplicateContent(IRowContent content, string hektometer, List<KeyValuePair<string, IRowContent>> rows)
	{
		if (rows.Count == 0) 
			return content;
		
		// TODO: Limit the check to the last 5? Since the last 5 are always only repeated?
		return rows.Any(x => x.Key == hektometer && x.Value.GetType() == content.GetType()) ? null : content;
	}
}