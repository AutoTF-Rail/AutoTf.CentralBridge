using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Markers;

public class ZugFunkEnde : RowContent
{
	public override string GetPrint()
	{
		return "- ZF ENDE -";
	}
	// There is no TryParse here, because it's handled by ZugFunk.cs
}