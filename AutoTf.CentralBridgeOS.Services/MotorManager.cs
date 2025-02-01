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
	private const double MinPulseWidth = 500.0;
	private const double MaxPulseWidth = 2500.0;
	private const double PwmPeriodMicroseconds = 1_000_000.0 / PwmFrequency;
	private const int MaxServoAngle = 270;
	
	private I2cDevice _i2CDevice;
	private I2cConnectionSettings _i2CSettings;
	private Pca9685? _pca;
	
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
	}

	private void InitializeI2CConnection()
	{
		_i2CSettings = new I2cConnectionSettings(1, Pca9685.I2cAddressBase);
		_i2CDevice = I2cDevice.Create(_i2CSettings);

		if (!AreMotorsAvailable())
			return;
		
		_pca = new Pca9685(_i2CDevice);
		MoveToMiddle();
	}

	public void SetMotorAngle(int channel, double angle)
	{
		if (_pca == null)
			return;

		double pulseWidth = MinPulseWidth + (angle / MaxServoAngle) * (MaxPulseWidth - MinPulseWidth);

		double dutyCycle = pulseWidth / PwmPeriodMicroseconds;

		_pca.SetDutyCycle(channel, dutyCycle);
	}
	
	public void MoveToMiddle()
	{
		SetMotorAngle(0, 135);
	}

	public double GetMotorAngle(int channel)
	{
		if (_pca == null)
			return -800;
		
		double dutyCycle = _pca.GetDutyCycle(channel);

		double pulseWidth = dutyCycle * PwmPeriodMicroseconds;

		double angle = (pulseWidth - MinPulseWidth) / (MaxPulseWidth - MinPulseWidth) * MaxServoAngle;

		return Math.Clamp(angle, 0, MaxServoAngle);
	}
	
	public bool AreMotorsAvailable()
	{ 
		try
		{
			_i2CDevice.ReadByte(); 
			return true; 
		}
		catch
		{
			return false;
		}
	}

	public void Dispose()
	{
		_i2CDevice.Dispose();
		_pca?.Dispose();
	}
}