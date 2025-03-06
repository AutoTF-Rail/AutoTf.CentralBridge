using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.TrainModels.Models;

public sealed class DesiroHC : DefaultModel
{
	public DesiroHC(MotorManager motorManager, Logger logger) : base(motorManager, logger)
	{
		Initialize();
	}

	public override void EasyControl(int power)
	{
		// Power can be +100 or -100; 100 = MinimumAngle; -100 = MaximumAngle
		Logger.Log($"EC: Easy control requested for power: {power}");

		double angle = CalculateAngle(Levers[0], power);
		
		Logger.Log($"EC: Setting motor angle to: {angle}");
		MotorManager.SetMotorAngle(0, angle);
	}

	public override void Initialize()
	{
		// TODO: This clear should not be needed
		Levers.Clear();
		
		if (!MotorManager.AreMotorsAvailable)
			return;
		
		// This is only for the UI pretty much so the tablet knows what it can control.
		Levers.Add(0, new LeverModel()
		{
			Type = CentralBridgeOS.Models.LeverType.CombinedThrottle,
			MaximumAngle = 90, // -45 from middle
			MiddleAngle = 135,
			MinimumAngle = 180 // +45 from middle
		});
		// Levers.Add(1, new LeverModel()
		// {
		// 	Type = CentralBridgeOS.Models.LeverType.MainBrake,
		// 	MaximumAngle = 90, // -45 from middle
		// 	MinimumAngle = 180 // +45 from middle
		// });
	}
	
	double CalculateAngle(LeverModel lever, int power)
	{
		int totalPowerRange = 200; // Default for CombinedThrottle

		if (lever.Type == LeverType.Throttle || lever.Type == LeverType.TwoStageBrake)
			totalPowerRange = 100;

		Logger.Log($"EC: Using range of {totalPowerRange} for power {power} with values: MaxA: {lever.MaximumAngle}, MidA: {lever.MiddleAngle}, MinA: {lever.MinimumAngle}.");

		double angle = lever.MiddleAngle + (lever.MinimumAngle - lever.MiddleAngle) * (power / 100.0);
    
		Logger.Log($"EC: Calculated angle: {angle}");
		return angle;
	}
}