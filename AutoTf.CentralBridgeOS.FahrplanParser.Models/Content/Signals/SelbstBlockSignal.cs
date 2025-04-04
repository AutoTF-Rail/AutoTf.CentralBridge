using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Signals;

/// <summary>
/// Sbk
/// </summary>
public class SelbstBlockSignal : SignalContent
{
	private SelbstBlockSignal(string signalNummer)
	{
		SignalNummer = signalNummer;
	}

	public string SignalNummer { get; set; }
	
	public override string GetPrint()
	{
		return $"Sbk {SignalNummer}";
	}

	public static bool TryParse(string additionalText, out RowContent? content)
	{
		content = null;
		
		// The speed prefix doesn't matter here, as it won't find anything anyways
		if (!TryParse(additionalText, "Sbk", "U", out string _, out string signalNummer))
			return false;

		content = new SelbstBlockSignal(signalNummer);
		return true;
	}
}