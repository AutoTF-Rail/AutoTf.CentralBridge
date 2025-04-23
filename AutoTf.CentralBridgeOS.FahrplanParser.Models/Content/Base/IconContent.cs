using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;

public abstract class IconContent : RowContent
{
	internal static bool TryParseIcon(string filename, Mat area)
	{
		Mat icon = CvInvoke.Imread(Path.Combine(AppContext.BaseDirectory, filename), ImreadModes.Grayscale);
		
		bool result = SearchForIcon(area, icon);
		
		icon.Dispose();
		return result;
	}

	private static bool SearchForIcon(Mat area, Mat icon)
	{
		if (icon.NumberOfChannels != 1)
			CvInvoke.CvtColor(icon, icon, ColorConversion.Bgr2Gray);

		int resultCols = area.Cols - icon.Cols + 1;
		int resultRows = area.Rows - icon.Rows + 1;
		Mat result = new Mat(resultRows, resultCols, DepthType.Cv32F, 1);

		CvInvoke.MatchTemplate(area, icon, result, TemplateMatchingType.CcoeffNormed);

		double minVal = 0;
		double maxVal = 0;
		Point minLoc = Point.Empty;
		Point maxLoc = Point.Empty;

		CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);
		result.Dispose();

		double threshold = 0.8;
		return maxVal >= threshold;
	}
}