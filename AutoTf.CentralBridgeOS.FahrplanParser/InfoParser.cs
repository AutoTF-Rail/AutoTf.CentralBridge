using System.Drawing;
using AutoTf.CentralBridgeOS.FahrplanParser.Extensions;
using Emgu.CV;
using Emgu.CV.OCR;

namespace AutoTf.CentralBridgeOS.FahrplanParser;

public abstract class InfoParser : ParserBase
{
	protected InfoParser(Tesseract engine) : base(engine) { }

	public string Date(Mat mat) => ExtractTextClean(RegionMappings.Date, mat, Engine).Replace("\n", "");

	public string Time(Mat mat) => ExtractTextClean(RegionMappings.Time, mat, Engine).Replace("\n", "");

	public string TrainNumber(Mat mat) => ExtractTextClean(RegionMappings.TrainNumber, mat, Engine).Replace("\n", "");

	public string PlanValid(Mat mat) => ExtractTextClean(RegionMappings.PlanValidity, mat, Engine).Replace("\n", "");

	public string Delay(Mat mat) => ExtractTextClean(RegionMappings.Delay, mat, Engine).Replace("\n", "");

	public string Hektometer(Mat mat, Rectangle row) => ExtractTextClean(RegionMappings.Hektometer(row), mat, Engine).Replace("\n", "");

	public string AdditionalText(Mat mat, Rectangle row) => ExtractTextClean(RegionMappings.AdditionalText(row), mat, Engine).Replace("\n", "");

	public string Arrival(Mat mat, Rectangle row) => ExtractTextClean(RegionMappings.Arrival(row), mat, Engine).Replace("\n", "");

	public string Departure(Mat mat, Rectangle row) => ExtractTextClean(RegionMappings.Departure(row), mat, Engine).Replace("\n", "");

	public string SpeedLimit(Mat mat, Rectangle row) => ExtractTextClean(RegionMappings.SpeedLimit(row), mat, Engine).Replace("\n", "");

	public string? Location(Mat mat)
	{
		// TODO: Does this maybe make a problem, if we are already on "page two" by location, so the point won't be on the first page?
		for (int i = 0; i < RegionMappings.LocationPoints.Count; i++)
		{
			Rectangle checkRoi = new Rectangle(RegionMappings.LocationPoints[i].X + 25, RegionMappings.LocationPoints[i].Y + 10, 6, 25);
			Mat checkMat = new Mat(mat, checkRoi);
					
			if(!checkMat.IsMoreBlackThanWhite())
				continue;

			return ExtractText(RegionMappings.LocationPointsHektometer[i], mat, Engine).TrimEnd();
		}

		return null;
	}
}