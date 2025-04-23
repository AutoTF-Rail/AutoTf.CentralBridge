using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;
using Emgu.CV;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Icons;

public class LzbStart : IconContent
{
	private const string FileName = "Icons/LzbStartIcon.png";

	public override string GetPrint()
	{
		return "LZB Start";
	}
	
	public static bool TryParseIcon(Mat area)
	{
		return TryParseIcon(FileName, area);
	}
}