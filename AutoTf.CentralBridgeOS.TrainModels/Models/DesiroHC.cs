using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Services;
using Yubico.Core.Logging;
using Logger = AutoTf.Logging.Logger;

namespace AutoTf.CentralBridgeOS.TrainModels.Models;

public sealed class DesiroHC : DefaultModel
{
	public DesiroHC(MotorManager motorManager, Logger logger) : base(motorManager, logger)
	{
		Initialize();
	}

	public override void EasyControl(int power)
	{
		Logger.Log($"EC: Setting power to {power}%.");
		if (power == 0)
		{
			SetLever(0, 0);
			SetLever(1, 0);
		}
		// TODO: See if the train is just starting the move, if the train doesn't already do it: only slowly release the breaks to avoid rolling
		else if (power > 0)
		{
			SetLever(0, power);
			SetLever(1, 0);
		}
		else if (power < 0)
		{
			SetLever(0, 0);
			SetLever(1, power * -1);
		}

		// If power is > 0, this is the angle for the throttle, if its < 0, then for breaks. If power == 0, just release everything.
		// double angle = CalculateAngle(Levers[0], power);

		
		// Logger.Log($"EC: Setting motor angle to {angle} for channel 0");
		// MotorManager.SetMotorAngle(0, angle);
	}

	public override void Initialize()
	{
		// TODO: This clear should not be needed
		Levers.Clear();
		
		if (!MotorManager.AreMotorsAvailable)
			return;
		
		Levers.Add(0, new LeverModel()
		{
			Type = CentralBridgeOS.Models.LeverType.Throttle,
			MaximumAngle = 90, // -45 from middle
			MiddleAngle = 135,
			MinimumAngle = 180, // +45 from middle
			IsInverted = false
		});
		Levers.Add(1, new LeverModel()
		{
			Type = LeverType.MainBrake,
			MaximumAngle = 90, // -45 from middle
			MiddleAngle = 135,
			MinimumAngle = 180, // +45 from middle
			IsInverted = true // This is inverted, because the release position is the top of the lever
		});
		
		// TODO: Reset lever position to release location, or tell user what the current state is?
	}
	
	// TODO: Do we even need this anymore?
	double CalculateAngle(LeverModel lever, int power)
	{
		int totalPowerRange = 200; // Default for CombinedThrottle

		if (lever.Type == LeverType.Throttle || lever.Type == LeverType.TwoStageBrake || lever.Type == LeverType.MainBrake)
			totalPowerRange = 100;

		Logger.Log($"EC: Using range of {totalPowerRange} for power {power} with values: MaxA: {lever.MaximumAngle}, MidA: {lever.MiddleAngle}, MinA: {lever.MinimumAngle}.");

		double angle = lever.MiddleAngle + (lever.MinimumAngle - lever.MiddleAngle) * (power / 100.0);
    
		Logger.Log($"EC: Calculated angle: {angle}");
		return angle;
	}
}