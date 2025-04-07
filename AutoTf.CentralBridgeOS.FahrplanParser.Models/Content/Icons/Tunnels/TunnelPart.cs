using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;
using Emgu.CV;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Icons.Tunnels;

// TODO: Hard to detect
/// <summary>
/// <remarks>Important when: https://www.dbinfrago.com/resource/blob/12596114/aa3a393dbd75f84f3c96e4af8aaa6acf/Ril-408-21-27-NBN-2025-data.pdf#page=134 (Section 4)</remarks>
/// </summary>
public class TunnelPart : TunnelContent
{
	private const string FileName = "Icons/TunnelPartIcon.png";
	
	public override string GetPrint()
	{
		return "Tunnel Part";
	}

	public override TunnelType GetTunnelType()
	{
		return TunnelType.Part;
	}

	public static bool TryParseIcon(Mat area)
	{
		return TryParseIcon(FileName, area);
	}
}