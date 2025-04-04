using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;
using Emgu.CV;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Icons.Tunnels;

// TODO: Remove this? Does it even matter? We know when a tunnel starts and ends, so we can "imagine" where this is? It's sort of hard to detect it cause of the middle line
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