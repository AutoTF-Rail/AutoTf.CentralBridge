using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.TrainModels;

public class DefaultModel : ITrainModel
{
	private readonly MotorManager _motorManager;
	private readonly Logger _logger;

	protected Dictionary<int, LeverModel> Levers = new Dictionary<int, LeverModel>();

	public DefaultModel(MotorManager motorManager, Logger logger)
	{
		_motorManager = motorManager;
		_logger = logger;
	}

	public virtual void Initialize()
	{
		// TODO: Log which train model is being used.
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
		double motorAngle = _motorManager.GetMotorAngle(index);
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

	public void SetLever(int index, double percentage)
	{
		LeverModel lever = Levers[index];

		if (lever.Type == LeverType.CombinedThrottle)
		{
			double angle;
			
			if (percentage >= 0)
				angle = lever.MiddleAngle + (percentage / 100) * (lever.MaximumAngle - lever.MiddleAngle);
			else
				angle = lever.MiddleAngle + (percentage * -1 / 100) * (lever.MinimumAngle - lever.MiddleAngle);
			
			_logger.Log($"Default Train: Setting Combined Lever to: {angle}");
			_motorManager.SetMotorAngle(index, angle);
		}
		else if (lever.Type == LeverType.MainBrake)
		{
			double angle = lever.MinimumAngle + (percentage / 100) * (lever.MaximumAngle - lever.MinimumAngle);
			_logger.Log($"Default Train: Setting MainBrake to: {angle}");
			_motorManager.SetMotorAngle(index, angle);
		}
	}
}