using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;

namespace AutoTf.CentralBridgeOS.FahrplanParser.Models.Content;

public class Station : RowContent
{
	private Station(string name, string arrival, string departure)
	{
		Name = name;
		Arrival = arrival;
		Departure = departure;
	}

	public string Name { get; set; }
	public string Arrival { get; set; }
	public string Departure { get; set; }
	
	public override string GetPrint()
	{
		return $"{Name} Arrival: {Arrival} Departure: {Departure}";
	}

	public static bool TryParse(string additionalText, string arrival, string departure, out RowContent? content)
	{
		content = null;

		if (string.IsNullOrWhiteSpace(arrival) || string.IsNullOrWhiteSpace(departure)) 
			return false;
		
		if (arrival.Trim() == "X")
		{
			// TODO: Handle this case?
			// "Bedarfshalt"?
		}

		content = new Station(additionalText, arrival, departure);
		
		return true;
	}
}