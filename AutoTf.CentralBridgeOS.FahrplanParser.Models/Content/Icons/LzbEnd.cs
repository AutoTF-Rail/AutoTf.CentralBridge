using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;
using Emgu.CV;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Icons;

public class LzbEnd : IconContent
{
	private const string FileName = "Icons/LzbEndeIcon.png";
	
	public override string GetPrint()
	{
		return "LZB Ende";
	}
	
	public static bool TryParseIcon(Mat area)
	{
		return TryParseIcon(FileName, area);
	}
}