using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;
using Emgu.CV;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Icons;

// TODO: Rename? To: "Weichenbereich ende"? or so?
/// <summary>
/// ¥
/// </summary>
public class YenMarker : IconContent
{
	private const string FileName = "Icons/YenIcon.png";
	
	public override string GetPrint()
	{
		return "\u00a5";
	}
	
	public static bool TryParseIcon(Mat area)
	{
		return TryParseIcon(FileName, area);
	}
}