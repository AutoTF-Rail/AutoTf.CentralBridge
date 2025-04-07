using AutoTf.CentralBridgeOS.FahrplanParser.Models;
using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.CentralBridgeOS.TrainModels.Models.DesiroHC;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.TrainModels.Models;

public class FallBackTrain : DefaultModel
{
	// TODO: Handle this case, or just check if the current model is "FallBack" in the classes that access this?
	public override RegionMappings Mappings { get; } = new Mapping();
	
	public FallBackTrain(MotorManager motorManager, Logger logger) : base(motorManager, logger)
	{
		Task.Run(Initialize);
	}

	public override void EasyControl(int power)
	{
		// TODO: Report to user that this is not available, if it's the default train model.
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