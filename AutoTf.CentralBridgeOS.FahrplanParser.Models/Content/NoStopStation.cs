using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content;

/// <summary>
/// Station where there is no planned stop. (Not factoring in Abzw or others)
/// </summary>
public class NoStopStation : RowContent
{
	// TODO: Could save departure time, to calculate delays.
	private NoStopStation(string name)
	{
		Name = name;
	}

	public string Name { get; set; }
	
	public override string GetPrint()
	{
		return $"{Name} - No stop";
	}

	public static bool TryParse(string additionalText, string arrival, string departure, out RowContent? content)
	{
		content = null;
		
		// TODO: Gibt es stationen die notiert sind, aber auch keine abfahrts zeit haben?
		if (!string.IsNullOrEmpty(arrival.Trim()) || string.IsNullOrEmpty(departure))
			return false;

		content = new NoStopStation(additionalText);
		return true;
	}
}