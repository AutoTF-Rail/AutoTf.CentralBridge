using System.Device.I2c;
using AutoTf.Logging;
using Iot.Device.Pwm;

namespace AutoTf.CentralBridgeOS.Services;

// Pins config (RPI4, pins on the right side view, top to bottom):
// 1: EMPTY 2: VCC
// 3: SDA 4: V+
// 5: SCL 6: GND
public class MotorManager : IDisposable
{
	private readonly Logger _logger;
	private const int PwmFrequency = 50;
	private const double MinPulseWidth = 500.0;
	private const double MaxPulseWidth = 2500.0;
	private const double PwmPeriodMicroseconds = 1_000_000.0 / PwmFrequency;
	private const int MaxServoAngle = 270;
	
	private I2cDevice _i2CDevice = null!;
	private I2cConnectionSettings _i2CSettings = null!;
	private Pca9685? _pca;
	
	private bool? _areMotorsAvailable;

	private bool _areMotorsReleased;
	
	// TODO: Listen on a certain pin for a button press to physically disable the motors.
	public MotorManager(Logger logger)
	{
		_logger = logger;
		Initialize();
	}

	private void Initialize()
	{
		Statics.ShutdownEvent += Dispose;
		InitializeI2CConnection();
	}

	private void InitializeI2CConnection()
	{
		_i2CSettings = new I2cConnectionSettings(1, Pca9685.I2cAddressBase);
		_i2CDevice = I2cDevice.Create(_i2CSettings);

		if (!AreMotorsAvailable)
			return;
		
		_pca = new Pca9685(_i2CDevice);
		MoveToMiddle();
	}

	public void SetMotorAngle(int channel, double angle)
	{
		if (!AreMotorsAvailable)
			return;
		if (_areMotorsReleased)
			return;

		double pulseWidth = MinPulseWidth + (angle / MaxServoAngle) * (MaxPulseWidth - MinPulseWidth);

		double dutyCycle = pulseWidth / PwmPeriodMicroseconds;

		_pca!.SetDutyCycle(channel, dutyCycle);
	}
	
	public void MoveToMiddle()
	{
		SetMotorAngle(0, 135);
	}

	public double GetMotorAngle(int channel)
	{
		if (!AreMotorsAvailable)
			return -800;
		if (_areMotorsReleased)
			return -900;
		
		double dutyCycle = _pca!.GetDutyCycle(channel);

		double pulseWidth = dutyCycle * PwmPeriodMicroseconds;

		double angle = (pulseWidth - MinPulseWidth) / (MaxPulseWidth - MinPulseWidth) * MaxServoAngle;

		return Math.Clamp(angle, 0, MaxServoAngle);
	}

	public void TurnOffMotor(int channel)
	{
		if (!AreMotorsAvailable)
			return;
		
		_pca!.SetDutyCycle(channel, 0);
	}

	public void TurnOnMotor(int channel)
	{
		if (!AreMotorsAvailable)
			return;
		
		_pca!.SetDutyCycle(channel, 1.0);
	}
	
	/// <summary>
	/// This is a bool given from the actual i2C connection.
	/// </summary>
	public bool AreMotorsAvailable
	{
		get
		{
			if (_areMotorsAvailable == null)
			{
				try
				{
					 _i2CDevice.ReadByte();
					 _areMotorsAvailable = true;
				}
				catch
				{
					_areMotorsAvailable = false;
				}
			}

			return (bool)_areMotorsAvailable;
		}
	}

	/// <summary>
	/// This is like a overall block, to turn off all motors. If you set this to true, motors will be turned off by setting their pwm to 0.
	/// They can then not be moved again until this value is set to false.
	/// When turned back on, their pwm is set to 4096
	/// </summary>
	public bool AreMotorsReleased
	{
		get => _areMotorsReleased;
		set
		{
			_areMotorsReleased = value;

			if (!_areMotorsReleased)
			{
				for (int i = 0; i < 16; i++)
				{
					TurnOnMotor(i);
					// TODO: Add sound feedback?
				}
				_logger.Log("Motors have been engaged.");
				return;
			}
			
			_logger.Log("Turning off all motors.");
			for (int i = 0; i < 16; i++)
			{
				TurnOffMotor(i);
				// TODO: Add sound feedback?
			}
		}
	}

	public void Dispose()
	{
		_i2CDevice.Dispose();
		_pca?.Dispose();
	}
}