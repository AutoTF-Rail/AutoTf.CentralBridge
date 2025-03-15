namespace AutoTf.CentralBridgeOS.Models;

public class LeverModel
{
	public LeverType Type { get; set; }
	// The angle where the lever is at 100%
	public int MaximumAngle { get; set; }
	// The angle where the lever is at 0%, or if combined: -100%
	public int MinimumAngle { get; set; }
	// The angle where the lever is at 0% if its a combined lever
	public int MiddleAngle { get; set; }
	
	public bool IsPrimary { get; set; }
	public bool IsInverted { get; set; }
}