using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.TrainModels.Models;

public class FallBackTrain : DefaultModel
{
	public FallBackTrain(MotorManager motorManager, Logger logger) : base(motorManager, logger)
	{
		Initialize();
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
		Levers.Add(0, new LeverModel()
		{
			Name = "Combined Example",
			Type = LeverType.CombinedLever,
			MaximumAngle = 45, // -90 from middle
			MiddleAngle = 135,
			MinimumAngle = 225, // +90 from middle
			IsPrimary = true
		});
		Levers.Add(1, new LeverModel()
		{
			Name = "Main Brake",
			Type = LeverType.RangedLever,
			MaximumAngle = 90, // -45 from middle
			MinimumAngle = 180, // +45 from middle
			IsPrimary = true
		});
	}
}