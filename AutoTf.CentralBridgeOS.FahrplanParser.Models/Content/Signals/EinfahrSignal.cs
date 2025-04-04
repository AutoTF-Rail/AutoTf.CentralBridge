using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Signals;

/// <summary>
/// Esig
/// </summary>
public class EinfahrSignal : SignalContent
{
	private EinfahrSignal(string stationName, string speed = "40")
	{
		Speed = speed;
		StationName = stationName;
	}
	
	// TODO: Validate that the only additional content can be a station name
	public string StationName { get; set; }

	// TODO: Nur bei Hp2(langsamfahrt): Ausfahrt mit 40, solange nicht größer definiert im Fahrplan 
	public string Speed { get; set; }
	
	public override string GetPrint()
	{
		string speed = string.Empty;
		
		if (Speed != "40")
			speed = $" E{Speed}";

		return $"Esig{speed} {StationName}";
	}

	public static bool TryParse(string additionalText, out RowContent? content)
	{
		content = null;
		
		if (!TryParse(additionalText, "Esig", "E", out string speed, out string stationName))
			return false;

		content = new EinfahrSignal(stationName, speed);
		return true;
	}
}