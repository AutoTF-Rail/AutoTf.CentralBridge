using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;
using AutoTf.CentralBridgeOS.Models.Interfaces;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Signals.Vorsignal;

/// <summary>
/// Avsig
/// </summary>
public class AusfahrVorsignal : SignalContent
{
	private AusfahrVorsignal(string signalNummer)
	{
		SignalNummer = signalNummer;
	}

	public string SignalNummer { get; set; }
	
	public override string GetPrint()
	{
		return $"Avsig {SignalNummer}";
	}

	public static bool TryParse(string additionalText, out IRowContent? content)
	{
		content = null;
		
		if (!TryParse(additionalText, "Avsig", "U", out string _, out string signalNummer))
			return false;

		content = new AusfahrVorsignal(signalNummer);
		return true;
	}
}