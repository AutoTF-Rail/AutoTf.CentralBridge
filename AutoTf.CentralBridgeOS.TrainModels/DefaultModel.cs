using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.TrainModels;

public class DefaultModel : ITrainModel
{
	internal readonly MotorManager MotorManager;
	internal readonly Logger Logger;

	protected Dictionary<int, LeverModel> Levers = new Dictionary<int, LeverModel>();

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

		if (lever.Type == LeverType.CombinedThrottle)
		{
			double angle;
			if (percentage >= 0)
				angle = lever.MiddleAngle + (percentage / 100) * (lever.MiddleAngle - lever.MaximumAngle);
			else
				angle = lever.MiddleAngle + (percentage / 100) * (lever.MinimumAngle - lever.MiddleAngle);
			
			Logger.Log($"Default Train: Setting Combined Lever to: {angle}");
			MotorManager.SetMotorAngle(index, angle);
		}
		else if (lever.Type == LeverType.MainBrake)
		{
			double angle = lever.MinimumAngle + (percentage / 100) * (lever.MaximumAngle - lever.MinimumAngle);
			Logger.Log($"Default Train: Setting MainBrake to: {angle}");
			MotorManager.SetMotorAngle(index, angle);
		}
	}
}