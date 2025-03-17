using AutoTf.CentralBridgeOS.Services;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.TrainModels.Models;

// ReSharper disable once InconsistentNaming
public sealed class DesiroML : DefaultModel
{
	public DesiroML(MotorManager motorManager, Logger logger) : base(motorManager, logger)
	{
		Initialize();
	}

	public override void EasyControl(int power)
	{
		Logger.Log($"EC: Setting power to {power}%.");
	}

	public override void EmergencyBrake()
	{
		OnEmergencyBrake?.Invoke();
		Logger.Log("EC: Emergency brake has been initiated.");
	}

	public override void Initialize()
	{
		
	}
}