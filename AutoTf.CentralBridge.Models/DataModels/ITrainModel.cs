using AutoTf.CentralBridge.Models.Enums;
using AutoTf.CentralBridge.Models.Interfaces;

namespace AutoTf.CentralBridge.Models.DataModels;

public interface ITrainModel
{
	public ICcdDisplayBase CcdDisplay { get; }
	public RegionMappings Mappings { get; }
	public Action? OnEmergencyBrake { get; set; }
	public bool IsEasyControlAvailable { get; }
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
	void Initialize();
}