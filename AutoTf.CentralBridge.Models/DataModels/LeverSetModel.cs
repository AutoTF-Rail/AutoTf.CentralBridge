using System.Text.Json.Serialization;

namespace AutoTf.CentralBridge.Models.DataModels;

public class LeverSetModel
{
	[JsonPropertyName("LeverIndex")]
	public int LeverIndex { get; set; }
	
	[JsonPropertyName("Percentage")]
	public double Percentage { get; set; }
}