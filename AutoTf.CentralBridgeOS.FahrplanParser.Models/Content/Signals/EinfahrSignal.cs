using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Signals;

/// <summary>
/// Esig
/// </summary>
public class EinfahrSignal : SignalContent
{
	private EinfahrSignal(string sectionName, string speed = "40")
	{
		Speed = speed;
		SectionName = sectionName;
	}
	
	/// <summary>
	/// Could be a station, or something else sometimes, no one knows.
	/// <remarks>https://www.dbinfrago.com/resource/blob/12841380/92952bc0d1ec6bdcff323ae1f9efb746/Handbuch-40820-data.pdf#page=68</remarks>
	/// </summary>
	public string SectionName { get; set; }
 
	/// <summary>
	/// If Hp 2 (Langsamfahrt): Exit with 40 km/h, unless defined otherwise in the Fahrplan
	/// </summary>
	public string Speed { get; set; }
	
	public override string GetPrint()
	{
		string speed = string.Empty;
		
		if (Speed != "40")
			speed = $" E{Speed}";

		return $"Esig{speed} {SectionName}";
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