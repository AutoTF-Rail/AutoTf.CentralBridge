namespace AutoTf.CentralBridgeOS.Models;

public interface ITrainModel
{
	public RegionMappings Mappings { get; }
	public int LeverCount();
	public void SetLever(int index, double percentage);
	public LeverType GetLeverType(int index);
	public double? GetLeverPercentage(int index);
	public void ReleaseMotors();
	public void EngageMotors();
	public void ReleaseMotor(int index);
	public void EngageMotor(int index);
	public bool AreMotorsReleased();
	public void EasyControl(int power);
	public void EmergencyBrake();
	public Action? OnEmergencyBrake { get; set; }
	void Initialize();
}