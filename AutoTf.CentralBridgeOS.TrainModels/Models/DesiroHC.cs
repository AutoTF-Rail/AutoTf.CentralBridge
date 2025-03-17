using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Services;
using Logger = AutoTf.Logging.Logger;

namespace AutoTf.CentralBridgeOS.TrainModels.Models;

// ReSharper disable once InconsistentNaming
public class DesiroHC : DefaultModel
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
		// TODO: See if the train is just starting the move, if the train doesn't already do it: only slowly release the breaks to avoid rolling?
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
	}

	public override void EmergencyBrake()
	{
		SetLever(0, 0);
		SetLever(1, 100);
		OnEmergencyBrake?.Invoke();
		Logger.Log("EC: Emergency brake has been initiated.");
	}

	public sealed override void Initialize()
	{
		if (!MotorManager.AreMotorsAvailable)
			return;
		
		Levers.Add(0, new LeverModel
		{
			Name = "Throttle",
			Type = LeverType.RangedLever,
			MaximumAngle = 90, // -45 from middle
			MiddleAngle = 135,
			MinimumAngle = 180, // +45 from middle
			IsInverted = false
		});
		Levers.Add(1, new LeverModel
		{
			Name = "Main Brake", 
			Type = LeverType.RangedLever,
			MaximumAngle = 90, // -45 from middle
			MiddleAngle = 135,
			MinimumAngle = 180, // +45 from middle
			IsInverted = false, // Usually on a brake this should be true, but since the motor is on the bottom of the lever, this has to be false
		});
		
		// TODO: Reset lever position to release location, or tell user what the current state is?
		// Only reset levers to "idle" when train is not moving? e.g. move throttle to 0 and apply a bit of brakes
	}
}