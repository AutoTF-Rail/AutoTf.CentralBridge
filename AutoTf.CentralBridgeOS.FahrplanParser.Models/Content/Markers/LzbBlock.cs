using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;
using AutoTf.CentralBridgeOS.Models.Interfaces;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Markers;

/// <summary>
/// LZB-Bk Num
/// </summary>
public class LzbBlock : RowContent
{
	private LzbBlock(string signalNummer)
	{
		SignalNummer = signalNummer;
	}

	public string SignalNummer { get; set; }
	
	public override string GetPrint()
	{
		return $"LZB-Bk {SignalNummer}";
	}

	public static bool TryParse(string additionalText, out IRowContent? content)
	{
		content = null;

		if (!additionalText.ToLower().Contains("lzb-bk"))
			return false;

		additionalText = additionalText.ToLower().Replace("lzb-bk", "").Trim();

		content = new LzbBlock(additionalText);
		return true;
	}
}