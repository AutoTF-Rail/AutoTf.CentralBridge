namespace AutoTf.CentralBridge.Models.DataModels;

public class KeyData
{
	public required string SerialNumber { get; set; }
	public DateTime LastUsed { get; set; }
	public DateTime? DeletedOn { get; set; } = null;
	public bool Verified { get; set; }
	public required string Secret { get; set; }
	public required DateTime CreatedOn { get; set; }
}