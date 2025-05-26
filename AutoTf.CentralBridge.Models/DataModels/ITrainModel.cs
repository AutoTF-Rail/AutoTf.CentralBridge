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
	public bool AreMotorsEngaged();
	public Action<bool>? MotorPowerHasChanged { get; }
	public void EasyControl(int power);
	public void EmergencyBrake();
	void Initialize();
}