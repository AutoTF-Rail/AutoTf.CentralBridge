using System.Device.I2c;
using AutoTf.Logging;
using Iot.Device.Pwm;

namespace AutoTf.CentralBridgeOS.Services;

// Pins config:
// 1: EMPTY 2: VCC
// 3: SDA 4: V+
// 5: SCL 6: GND
public class MotorManager : IDisposable
{
	private readonly Logger _logger;
	private const int PwmFrequency = 50;
	private const double MinPulseWidthMs = 0.5;
	private const double MaxPulseWidthMs = 2.5;
	private const double NeutralPulseWidthMs = 1.5;
	
	private I2cDevice _i2CDevice;
	private I2cConnectionSettings _i2CSettings;
	private Pca9685 _pca;
	
	private bool? _areMotorsAvailable;
	
	public MotorManager(Logger logger)
	{
		_logger = logger;
		Initialize();
	}

	private void Initialize()
	{
		Statics.ShutdownEvent += Dispose;
		InitializeI2CConnection();
		MoveToMiddle();
	}

	private void InitializeI2CConnection()
	{
		_i2CSettings = new I2cConnectionSettings(1, Pca9685.I2cAddressBase);
		_i2CDevice = I2cDevice.Create(_i2CSettings);

		if (!AreMotorsAvailable())
			return;
		
		_pca = new Pca9685(_i2CDevice);
		_pca.PwmFrequency = 50;
	}

	public void SetMotorAngle(int channel, double angle)
	{
		angle = Math.Max(0, Math.Min(270, angle));

		double pulseWidth = 500 + (angle / 270.0) * (2500 - 500); 

		double dutyCycle = pulseWidth / 1000.0;
		_logger.Log($"Motor: Setting {channel} to {dutyCycle}");
		_pca.SetDutyCycle(channel, dutyCycle);
	}
	
	public void MoveToMiddle()
	{
		double dutyCycle = NeutralPulseWidthMs / (1000.0 / PwmFrequency) * 100;
		_pca.SetDutyCycle(0, dutyCycle / 100);
	}

	public double GetMotorAngle(int channel)
	{
		double dutyCycle = _pca.GetDutyCycle(channel) * 100;

		double pulseWidthMs = (dutyCycle / 100) * (1000.0 / PwmFrequency);

		double angle = (pulseWidthMs - MinPulseWidthMs) / (MaxPulseWidthMs - MinPulseWidthMs) * 270;
		
		return angle;
	}
	
	public bool AreMotorsAvailable()
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

	public void Dispose()
	{
		_i2CDevice.Dispose();
		_pca.Dispose();
	}
}