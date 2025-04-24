using AutoTf.CentralBridgeOS.Models.Enums;

namespace AutoTf.CentralBridgeOS.Models.DataModels;

public class LeverModel
{
	public LeverModel(string name, LeverType type, int maximumAngle, int middleAngle, int minimumAngle, bool isInverted)
	{
		Name = name;
		Type = type;
		MaximumAngle = maximumAngle;
		MinimumAngle = minimumAngle;
		MiddleAngle = middleAngle;
		IsInverted = isInverted;
	}
	
	public LeverModel(string name, LeverType type, int maximumAngle, int minimumAngle, bool isInverted)
	{
		Name = name;
		Type = type;
		MaximumAngle = maximumAngle;
		MinimumAngle = minimumAngle;
		IsInverted = isInverted;
	}

	public string Name { get; init; }
	public LeverType Type { get; init; }
	
	// The angle where the lever is at 100%
	public int MaximumAngle { get; init; }
	
	// The angle where the lever is at 0%, or if combined: -100%
	public int MinimumAngle { get; init; }
	
	// The angle where the lever is at 0% if its a combined lever
	public int MiddleAngle { get; init; }
	
	public bool IsInverted { get; init; }
}