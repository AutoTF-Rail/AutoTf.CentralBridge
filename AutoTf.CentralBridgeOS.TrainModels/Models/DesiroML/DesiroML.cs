using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Models.Interfaces;
using AutoTf.CentralBridgeOS.TrainModels.CcdDisplays;
using AutoTf.CentralBridgeOS.TrainModels.CcdDisplays.DesiroHc;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.TrainModels.Models.DesiroML;

// ReSharper disable once InconsistentNaming
public sealed class DesiroML : DefaultModel
{
	public DesiroML(IMotorManager motorManager, Logger logger) : base(motorManager, logger)
	{
		Task.Run(Initialize);
	}

	public override CcdDisplayBase CcdDisplay { get; } = new Base();
	public override RegionMappings Mappings { get; } = new Mapping();

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