using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;
using AutoTf.CentralBridgeOS.Models.Interfaces;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Markers;

public class ZugFunk : RowContent
{
	private ZugFunk(string info)
	{
		Info = info;
	}
	
	public string Info { get; set; }
	
	public override string GetPrint()
	{
		return $"- ZF {Info} -";
	}

	public static bool TryParse(string additionalText, out IRowContent? content)
	{
		content = null;

		if (!additionalText.Contains("ZF"))
			return false;
		
		additionalText = additionalText.Replace("ZF", "").Replace("-", "").Trim();
		
		if (additionalText.Contains("ENDE"))
			content = new ZugFunkEnde();
		else 
			content = new ZugFunk(additionalText);
		
		return true;
	}
}