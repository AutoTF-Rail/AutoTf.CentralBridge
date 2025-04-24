using System.Text.RegularExpressions;
using AutoTf.CentralBridgeOS.FahrplanParser.Models.Content.Base;
using AutoTf.CentralBridgeOS.Models.Interfaces;

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

	public static bool TryParse(string additionalText, string arrival, string departure, out IRowContent? content)
	{
		content = null;

		if (string.IsNullOrWhiteSpace(arrival) || string.IsNullOrWhiteSpace(departure)) 
			return false;
		
		string timePattern = @"^\d{2}:\d{2}\.\d{1}$";
		if (!Regex.IsMatch(arrival.Trim(), timePattern))
		{
			// TODO: Handle?
			// x = Bedarfshalt (Prob doesn't matter for us)
			// Halt = Bü // TODO: This should probably be handled differently/We need to parse the Bü first, so it's not duplicated
			// + = Betriebshalt
			// H = Kein halt, solange Zp 9
			// U = Kein halt
			// N = nicht veröffentlichter halt (?)
			// A = Only exit
			// Z = Only entry
		}

		content = new Station(additionalText, arrival, departure);
		
		return true;
	}
}