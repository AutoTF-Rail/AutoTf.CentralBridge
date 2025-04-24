using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;
using AutoTf.CentralBridgeOS.Models.Interfaces;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Signals;

/// <summary>
/// Asig
/// </summary>
public class AusfahrSignal : SignalContent
{
	private AusfahrSignal(string stationName, string speed = "40")
	{
		StationName = stationName;
		Speed = speed;
	}

	public string StationName { get; set; }
	public string Speed { get; set; }
	
	public override string GetPrint()
	{
		string speed = string.Empty;
		
		if (Speed != "40")
			speed = $" A{Speed}";

		return $"Asig{speed} {StationName}";
	}

	public static bool TryParse(string additionalText, out IRowContent? content)
	{
		content = null;
		
		if (!TryParse(additionalText, "Asig", "A", out string speed, out string stationName))
			return false;

		content = new AusfahrSignal(stationName, speed);
		return true;
	}
}