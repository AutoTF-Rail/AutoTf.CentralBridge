using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.TrainModels;

public class DefaultModel : ITrainModel
{
	internal readonly MotorManager MotorManager;
	internal readonly Logger Logger;

	protected Dictionary<int, LeverModel> Levers = new Dictionary<int, LeverModel>();
	
	internal int _currentPower = 0;

	public DefaultModel(MotorManager motorManager, Logger logger)
	{
		MotorManager = motorManager;
		Logger = logger;
	}

	public virtual void EasyControl(int power)
	{
		// TODO: Report to user that this is not available, if it's the default train model.
	}

	public virtual void Initialize()
	{
		if (!MotorManager.AreMotorsAvailable)
			return;
		
		Levers.Add(0, new LeverModel()
		{
			Type = LeverType.CombinedThrottle,
			MaximumAngle = 45, // -90 from middle
			MiddleAngle = 135,
			MinimumAngle = 225, // +90 from middle
			IsPrimary = true
		});
		Levers.Add(1, new LeverModel()
		{
			Type = LeverType.MainBrake,
			MaximumAngle = 90, // -45 from middle
			MinimumAngle = 180, // +45 from middle
			IsPrimary = true
		});
	}

	public int LeverCount()
	{
		return Levers.Count;
	}

	public LeverType GetLeverType(int index)
	{
		return Levers[index].Type;
	}

	public double? GetLeverPercentage(int index)
	{
		double motorAngle = MotorManager.GetMotorAngle(index);
		LeverModel lever = Levers[index];

		if (lever.Type == LeverType.CombinedThrottle)
		{
			return (motorAngle - lever.MiddleAngle) / (lever.MaximumAngle - lever.MinimumAngle) * 200;
		}
		else if (lever.Type == LeverType.MainBrake)
		{
			return (motorAngle - lever.MinimumAngle) / (lever.MaximumAngle - lever.MinimumAngle) * 100;
		}

		return null;
	}

	public bool AreMotorsReleased()
	{
		return MotorManager.AreMotorsReleased;
	}

	public void EngageMotors()
	{
		MotorManager.AreMotorsReleased = false;
	}

	public void ReleaseMotor(int index)
	{
		MotorManager.TurnOffMotor(index);
	}

	public void EngageMotor(int index)
	{
		MotorManager.TurnOnMotor(index);
	}

	public void ReleaseMotors()
	{
		MotorManager.AreMotorsReleased = true;
	}

	public void SetLever(int index, double percentage)
	{
		LeverModel lever = Levers[index];

		int leverMaximumAngle = lever.MaximumAngle;
		int leverMiddleAngle = lever.MiddleAngle;
		int leverMinimumAngle = lever.MinimumAngle;
		
		// The release location for such lever doesn't matter, as a combined lever should always have the release location at the middle.
		if (lever.Type == LeverType.CombinedThrottle)
		{
			double angle;
			
			if (percentage >= 0)
				angle = leverMiddleAngle + (percentage / 100) * (leverMiddleAngle - leverMaximumAngle);
			else
				angle = leverMiddleAngle + (percentage / 100) * (leverMinimumAngle - leverMiddleAngle);
			
			Logger.Log($"Default Train: Setting Combined Lever to: {angle}");
			MotorManager.SetMotorAngle(index, angle);
		}
		else if (lever.Type == LeverType.MainBrake || lever.Type == LeverType.Throttle)
		{
			// The default release location is minimum, but if its maximum, then min and max need to be switched.
			if(lever.ReleaseLocation == ReleaseLocation.Maximum)
				(leverMinimumAngle, leverMaximumAngle) = (leverMaximumAngle, leverMinimumAngle);

			double angle = leverMinimumAngle + (percentage / 100) * (leverMaximumAngle - leverMinimumAngle);
			Logger.Log($"Default Train: Setting {lever.Type.ToString()} to: {angle}");
			MotorManager.SetMotorAngle(index, angle);
		}
	}
}