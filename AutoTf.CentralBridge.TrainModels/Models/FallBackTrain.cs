using AutoTf.CentralBridge.Models;
using AutoTf.CentralBridge.Models.DataModels;
using AutoTf.CentralBridge.Models.Enums;
using AutoTf.CentralBridge.Models.Interfaces;
using AutoTf.CentralBridge.TrainModels.CcdDisplays;
using AutoTf.CentralBridge.TrainModels.CcdDisplays.DesiroHc;
using AutoTf.Logging;
using Mapping = AutoTf.CentralBridge.TrainModels.Models.DesiroHC.Mapping;

namespace AutoTf.CentralBridge.TrainModels.Models;

using Mapping = DesiroHC.Mapping;

public class FallBackTrain : DefaultModel
{
	// TODO: Handle this case, or just check if the current model is "FallBack" in the classes that access this?

	public override CcdDisplayBase CcdDisplay { get; } = new Base();
	public override RegionMappings Mappings { get; } = new Mapping();
	
	public FallBackTrain(IMotorManager motorManager, Logger logger) : base(motorManager, logger)
	{
		Task.Run(Initialize);
	}

	/// <summary>
	/// It's easier to just let the endpoint be and log that something has been done, even when EC is unavailable.
	/// </summary>
	public override void EasyControl(int power)
	{
		Logger.Log($"EC: Setting power to {power}%.");
	}

	public override void EmergencyBrake()
	{
		// TODO: Report to user that this is not available, if it's the default train model.
		// Or have some generalised option to emergency brake, that is the same on all trains.
		OnEmergencyBrake?.Invoke();
		Logger.Log("EC: Emergency brake has been initiated.");
	}
	
	public sealed override void Initialize()
	{
		if (!MotorManager.AreMotorsAvailable)
			return;
		
		// TODO: remove this?
		// 45 = -90 from middle
		// 225 = +90 from middle
		Levers.Add(0,
			new LeverModel("Combined Example", LeverType.CombinedLever, maximumAngle: 45, middleAngle: 135, minimumAngle: 225, false));
		// 90 = -45 from middle
		// 180 = +45 from middle
		Levers.Add(1, new LeverModel("Main Brake", LeverType.RangedLever, maximumAngle: 90, minimumAngle: 180, false));
	}
}