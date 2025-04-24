using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;
using AutoTf.CentralBridgeOS.Models.Interfaces;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Markers;

public class Bahnuebergang : RowContent
{
	private Bahnuebergang(string kilometer, string mechanism)
	{
		Kilometer = kilometer;
		Mechanism = mechanism;
	}

	public string Kilometer { get; }
	
	// TODO: Enum?
	/// <summary>
	/// Representing the type of securing mechanism the Bahnübergang has. (I think?)
	/// </summary>
	public string Mechanism { get; }
	
	public override string GetPrint()
	{
		return $"Bü km {Kilometer} {Mechanism}";
	}

	public static bool TryParse(string additionalText, out IRowContent? content)
	{
		content = null;

		if (!additionalText.Contains("Bü"))
			return false;

		string[] infos = additionalText.Split("km");

		string kilometer = infos[0].Replace("Bü", "").Trim();
		string mechanism = infos[1].Trim();
		
		content = new Bahnuebergang(kilometer, mechanism);
		return true;
	}
}