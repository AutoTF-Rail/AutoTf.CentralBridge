using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;
using Emgu.CV;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Icons.Tunnels;

/// <summary>
/// Icons/tunnelStartIcon.png
/// </summary>
public class TunnelStart : TunnelContent
{
	private const string FileName = "Icons/TunnelStartIcon.png";

	public TunnelStart(string name)
	{
		Name = name.Replace("-T", "");
	}
	
	public string Name { get; set; }
	
	public override string GetPrint()
	{
		return $"Tunnel {Name}-T";
	}

	public override TunnelType GetTunnelType()
	{
		return TunnelType.Start;
	}

	public static bool TryParseIcon(Mat area)
	{
		return TryParseIcon(FileName, area);
	}
}