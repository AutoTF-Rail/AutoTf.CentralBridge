using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Models.DataModels;
using AutoTf.CentralBridgeOS.Models.Enums;
using AutoTf.CentralBridgeOS.Models.Interfaces;
using AutoTf.CentralBridgeOS.TrainModels.CcdDisplays;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.TrainModels;

public abstract class DefaultModel : ITrainModel
{
	protected Dictionary<int, LeverModel> Levers = new Dictionary<int, LeverModel>();
	
	internal readonly IMotorManager MotorManager;
	internal readonly Logger Logger;
	
	internal int _currentPower = 0;

	public DefaultModel(IMotorManager motorManager, Logger logger)
	{
		MotorManager = motorManager;
		Logger = logger;
	}

	public bool IsEasyControlAvailable
	{
		get
		{
			// TODO: Implement some proper check in the future, also for errors on the motors etc.
			if (LeverCount() != 0)
				return true;

			return false;
		}
	}
	
	public abstract void Initialize();

	public abstract ICcdDisplayBase CcdDisplay { get; }
	public abstract RegionMappings Mappings { get; }

	public Action? OnEmergencyBrake { get; set; }

	public abstract void EasyControl(int power);

	public abstract void EmergencyBrake();
	
	public int LeverCount() => Levers.Count;
	

	public LeverType GetLeverType(int index) => Levers[index].Type;

	public double? GetLeverPercentage(int index)
	{
		double motorAngle = MotorManager.GetMotorAngle(index);
		LeverModel lever = Levers[index];

		if (lever.Type == LeverType.CombinedLever)
		{
			return (motorAngle - lever.MiddleAngle) / (lever.MaximumAngle - lever.MinimumAngle) * 200;
		}
		else if (lever.Type == LeverType.RangedLever)
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
		if (lever.Type == LeverType.CombinedLever)
		{
			double angle;
			
			if (percentage >= 0)
				angle = leverMiddleAngle + (percentage / 100) * (leverMiddleAngle - leverMaximumAngle);
			else
				angle = leverMiddleAngle + (percentage / 100) * (leverMinimumAngle - leverMiddleAngle);
			
			Logger.Log($"Default Train: Setting Combined Lever \"{lever.Name}\" to: {angle}");
			MotorManager.SetMotorAngle(index, angle);
		}
		else if (lever.Type == LeverType.RangedLever)
		{
			// The default release location is minimum, but if its maximum, then min and max need to be switched.
			double angle = lever.IsInverted 
				? leverMaximumAngle - (percentage / 100) * (leverMaximumAngle - leverMinimumAngle)
				: leverMinimumAngle + (percentage / 100) * (leverMaximumAngle - leverMinimumAngle);

			Logger.Log($"Default Train: Setting {lever.Type.ToString()} \"{lever.Name}\" to: {angle}");
			MotorManager.SetMotorAngle(index, angle);
		}
	}
}