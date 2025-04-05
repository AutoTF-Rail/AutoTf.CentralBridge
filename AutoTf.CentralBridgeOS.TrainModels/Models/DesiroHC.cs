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
			SetLever(1, 0);
			// Wait a moment for the brakes to release, before applying power. (This is only a delay for the lever to be moved TODO: Maybe do a proper wait for the release?)
			if (_currentPower < 0)
				Thread.Sleep(700);
			SetLever(0, power);
		}
		else // if (power < 0)
		{
			SetLever(0, 0);
			SetLever(1, power * -1);
		}
		
		_currentPower = power;
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
		
		// I know a Desiro HC uses a combined lever, but this is for the demo.
		Levers.Add(0, new LeverModel("Throttle", LeverType.RangedLever, maximumAngle: 90, middleAngle: 135, minimumAngle: 180, false));
		// Usually on a brake "isInverted" should be true, but since the motor is on the bottom of the lever, this has to be false
		Levers.Add(1, new LeverModel("Main Brake", LeverType.RangedLever, maximumAngle: 90, middleAngle: 135, minimumAngle: 180, false));
		
		// TODO: Reset lever position to release location, or tell user what the current state is?
		// Only reset levers to "idle" when train is not moving? e.g. move throttle to 0 and apply a bit of brakes
	}
}