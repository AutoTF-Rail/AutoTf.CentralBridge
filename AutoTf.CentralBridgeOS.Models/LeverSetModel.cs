using System.Text.Json.Serialization;

namespace AutoTf.CentralBridgeOS.Models;

public class LeverSetModel
{
	[JsonPropertyName("LeverIndex")]
	public int LeverIndex { get; set; }
	
	[JsonPropertyName("Percentage")]
	public double Percentage { get; set; }
}