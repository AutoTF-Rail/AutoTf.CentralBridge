using System.Device.I2c;
using AutoTf.CentralBridgeOS.Models.Interfaces;
using AutoTf.Logging;
using Iot.Device.Pwm;
using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridgeOS.Services;

// Pins config (RPI4, pins on the right side view, top to bottom):
// 1: EMPTY 2: VCC
// 3: SDA 4: V+
// 5: SCL 6: GND
public class MotorManager : IMotorManager
{
	private readonly Logger _logger;
	
	private I2cDevice _i2CDevice = null!;
	private I2cConnectionSettings _i2CSettings = null!;
	private Pca9685? _pca;
	
	private bool? _areMotorsAvailable;

	private bool _areMotorsReleased;
	private volatile bool _isInitialized = false;
	
	// TODO: Listen on a certain pin for a button press to physically disable the motors.
	public MotorManager(Logger logger)
	{
		_logger = logger;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		InitializeI2CConnection();

		return Task.CompletedTask;
	}

	private void InitializeI2CConnection()
	{
		try
		{
			_i2CSettings = new I2cConnectionSettings(1, Pca9685.I2cAddressBase);
			_i2CDevice = I2cDevice.Create(_i2CSettings);
			
			ResetPcaBoard(_i2CDevice);

			_isInitialized = true;
			if (!AreMotorsAvailable)
				return;
		
			_pca = new Pca9685(_i2CDevice);
		}
		catch (Exception e)
		{
			_logger.Log("Initialization of Motor manager failed.");
			_logger.Log(e.ToString());
		}
	}
	
	public bool AreMotorsAvailable
	{
		get
		{
			int waited = 0;
			while (!_isInitialized && waited < 5000)
			{
				Thread.Sleep(50);
				waited += 50;
			}
			if (_areMotorsAvailable == null)
			{
				try
				{
					_i2CDevice.ReadByte();
					_logger.Log("Motors are available.");
					_areMotorsAvailable = true;
				}
				catch
				{
					_logger.Log("Motors are not available.");
					_areMotorsAvailable = false;
				}
			}

			return (bool)_areMotorsAvailable;
		}
	}
	
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
					// TODO: Add sound feedback? e.g. if disabled by button or so
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
	
	private void ResetPcaBoard(I2cDevice i2CDevice)
	{
		byte mode1RegisterAddress = 0x00;
		byte resetValue = 0x80; 
        
		i2CDevice.Write(new ReadOnlySpan<byte>([mode1RegisterAddress, resetValue]));

		Thread.Sleep(10);
	}

	public void SetMotorAngle(int channel, double angle)
	{
		if (!AreMotorsAvailable)
			return;
		if (_areMotorsReleased)
			return;
		
		_logger.Log($"Setting channel {channel} to {angle}deg.");
		
		angle = Math.Max(0, Math.Min(270, angle));
		double minDutyCycle = 0.065;
		double maxDutyCycle = 0.53;
		
		double dutyCycle = minDutyCycle + (angle / 270.0) * (maxDutyCycle - minDutyCycle);

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

		double minDutyCycle = 0.064;
		double maxDutyCycle = 0.54;

		double angle = (dutyCycle - minDutyCycle) / (maxDutyCycle - minDutyCycle) * 270;
		
		return angle;
	}

	public void TurnOffMotor(int channel)
	{
		if (!AreMotorsAvailable)
			return;
		
		_pca!.SetDutyCycle(channel, 0);
		// TODO: Change this to use a mosfet
	}

	public void TurnOnMotor(int channel)
	{
		if (!AreMotorsAvailable)
			return;
		
		_pca!.SetDutyCycle(channel, 1.0);
		// TODO: Change this to use a mosfet
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_i2CDevice.Dispose();
		_pca?.Dispose();

		return Task.CompletedTask;
	}
}