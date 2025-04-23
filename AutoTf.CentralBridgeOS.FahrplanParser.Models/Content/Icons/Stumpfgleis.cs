using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;
using Emgu.CV;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Icons;

/// <summary>
/// Sideways T (Icons/StumpfgleisIcon.png)
/// </summary>
public class Stumpfgleis : IconContent
{
	private const string FileName = "Icons/StumpfgleisIcon.png";
	
	public override string GetPrint()
	{
		return "Stumpfgleis";
	}
	
	public static bool TryParseIcon(Mat area)
	{
		return TryParseIcon(FileName, area);
	}
}