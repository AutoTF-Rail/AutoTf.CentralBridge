using System.Drawing;
using AutoTf.CentralBridge.FahrplanParser.Extensions;
using AutoTf.CentralBridge.Models.DataModels;
using Emgu.CV;
using Emgu.CV.OCR;

namespace AutoTf.CentralBridge.FahrplanParser;

public abstract class InfoParser : ParserBase
{
	protected InfoParser(Tesseract engine, ITrainModel train) : base(engine, train) { }

	public string Date(Mat mat) => ExtractTextClean(Train.Mappings.Date, mat, Engine).Replace("\n", "");

	public string Time(Mat mat) => ExtractTextClean(Train.Mappings.Time, mat, Engine).Replace("\n", "");

	public string TrainNumber(Mat mat) => ExtractTextClean(Train.Mappings.TrainNumber, mat, Engine).Replace("\n", "");

	public string PlanValid(Mat mat) => ExtractTextClean(Train.Mappings.PlanValidity, mat, Engine).Replace("\n", "");

	public string Delay(Mat mat) => ExtractTextClean(Train.Mappings.Delay, mat, Engine).Replace("\n", "");

	public string Hektometer(Mat mat, Rectangle row) => ExtractTextClean(Train.Mappings.Hektometer(row), mat, Engine).Replace("\n", "");

	public string AdditionalText(Mat mat, Rectangle row) => ExtractTextClean(Train.Mappings.AdditionalText(row), mat, Engine).Replace("\n", "");

	public string Arrival(Mat mat, Rectangle row) => ExtractTextClean(Train.Mappings.Arrival(row), mat, Engine).Replace("\n", "");

	public string Departure(Mat mat, Rectangle row) => ExtractTextClean(Train.Mappings.Departure(row), mat, Engine).Replace("\n", "");

	public string SpeedLimit(Mat mat, Rectangle row) => ExtractTextClean(Train.Mappings.SpeedLimit(row), mat, Engine).Replace("\n", "");

	/// <summary>
	/// Gets the current train location by the small black dot on the side.
	/// </summary>
	/// <param name="mat">The current page of the EbuLa display (reached by pressing a certain button)</param>
	/// <returns></returns>
	public string? Location(Mat mat)
	{
		for (int i = 0; i < Train.Mappings.LocationPoints.Count; i++)
		{
			Rectangle checkRoi = new Rectangle(Train.Mappings.LocationPoints[i].X + 17, Train.Mappings.LocationPoints[i].Y + 12, 5, 5);
			Mat checkMat = new Mat(mat, checkRoi);
					
			if(!checkMat.IsMoreBlackThanWhite())
				continue;

			return ExtractText(Train.Mappings.LocationPointsHektometer[i], mat, Engine).TrimEnd();
		}

		return null;
	}
}