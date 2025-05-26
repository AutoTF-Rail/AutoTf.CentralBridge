using System.Device.Gpio;
using AutoTf.CentralBridge.Models;
using AutoTf.CentralBridge.Models.DataModels;
using AutoTf.CentralBridge.Models.Enums;
using AutoTf.CentralBridge.Models.Interfaces;
using AutoTf.Logging;

namespace AutoTf.CentralBridge.TrainModels;

public abstract class DefaultModel : ITrainModel, IDisposable
{
	protected Dictionary<int, LeverModel> Levers = new Dictionary<int, LeverModel>();

	private readonly GpioController _gpioController;
	private const int MotorStatusPin = 4;
	
	internal readonly IMotorManager MotorManager;
	internal readonly Logger Logger;
	
	internal int _currentPower = 0;

	private bool? _areMotorsEngagedState;

	public DefaultModel(IMotorManager motorManager, Logger logger)
	{
		MotorManager = motorManager;
		Logger = logger;

		_gpioController = new GpioController();
		_gpioController.OpenPin(MotorStatusPin, PinMode.Input);
		
		_gpioController.RegisterCallbackForPinValueChangedEvent(MotorStatusPin, PinEventTypes.Falling | PinEventTypes.Rising, OnMotorPinChanged);
	}

	private void OnMotorPinChanged(object sender, PinValueChangedEventArgs args)
	{
		if (args.ChangeType == PinEventTypes.Falling)
		{
			_areMotorsEngagedState = false;
		}
		else if (args.ChangeType == PinEventTypes.Rising)
		{
			_areMotorsEngagedState = true;
			// TODO: Handle this case, and maybe set the motors, so that they wont go to a random position when they reinvoke?
			// TODO: Disable auto pilot when this happens?
		}
		MotorPowerHasChanged?.Invoke((bool)_areMotorsEngagedState!);
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

	public bool AreMotorsEngaged()
	{
		// TODO: Read pin state here and return as bool
		if (_areMotorsEngagedState == null)
			_areMotorsEngagedState = _gpioController.Read(MotorStatusPin) == PinValue.High;
		
		
		return (bool)_areMotorsEngagedState;
	}

	public Action<bool>? MotorPowerHasChanged { get; }

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

	public void Dispose()
	{
		_gpioController.UnregisterCallbackForPinValueChangedEvent(MotorStatusPin, OnMotorPinChanged);
		_gpioController.ClosePin(MotorStatusPin);
		_gpioController.Dispose();
	}
}