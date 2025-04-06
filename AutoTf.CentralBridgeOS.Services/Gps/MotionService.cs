using System.IO.Ports;
using AutoTf.CentralBridgeOS.Models;
using AutoTf.Logging;
using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridgeOS.Services.Gps;

// Configuration required on chip:
// Baudrate: 115200
// Measurement period: 100ms
// See github/AutoTf.GpsTest repo for docs
public class MotionService : IHostedService
{
	public MotionService()
	{
		_gpsSerialPort = new SerialPort(_gpsPortName, _gpsBaudRate, Parity.None, 8, StopBits.One);
	}
	
	private readonly Logger _logger = Statics.Logger;

	private readonly string _gpsPortName = "/dev/ttyACM0";
	private readonly int _gpsBaudRate = 115200;

	private readonly SerialPort _gpsSerialPort;

	private Tuple<double, double>? _currentCoordinates;

	private bool _isGpsConnected;

	private double? _currentSpeed;
	
	public DateTime LastCoordinatesTime { get; private set; }
	
	public DateTime LastSpeedTime { get; private set; }
	
	public bool IsGpsAvailable { get; private set; }

	public bool IsGpsConnected
	{
		get => _isGpsConnected;
		private set
		{
			if (value == _isGpsConnected)
				return;
			if(!value)
				_logger.Log("GPS: Lost GPS connection.");
			else
				_logger.Log("GPS: Reconnected GPS.");

			_isGpsConnected = value;
		}
	}

	public double? CurrentSpeed
	{
		get => _currentSpeed;
		private set
		{
			if (Equals(value, _currentSpeed))
				return;

			_currentSpeed = value;
			LastSpeedTime = DateTime.Now;
			// TODO: Make like a timeout of a few seconds, if this doesn't change, set it to 0 cause of unknown speed?
		}
	}
	
	// TODO: Change this to class "GeographicalPosition"? 
	public Tuple<double, double>? CurrentCoordinates
	{
		get => _currentCoordinates;
		private set
		{
			if (Equals(value, _currentCoordinates))
				return;

			_currentCoordinates = value;
			LastCoordinatesTime = DateTime.Now;
		}
	}
	
	public Task StartAsync(CancellationToken cancellationToken)
	{
		// Check for camera pointed on screen
		StartGps();
		return Task.CompletedTask;
	}

	private void StartGps()
	{
		if (!Directory.Exists(_gpsPortName))
		{
			IsGpsAvailable = false;
			
			return;
		}
		_gpsSerialPort.Open();
		_gpsSerialPort.DataReceived += GpsDataReceived;
	}

	private void GpsDataReceived(object sender, SerialDataReceivedEventArgs e)
	{
		string content = _gpsSerialPort.ReadLine();

		if (content.StartsWith("$GPGLL"))
		{
			Tuple<double, double>? coordinates = NmeaParser.ExtractCoordinates(content);
			
			if (coordinates != null)
				CurrentCoordinates = coordinates;
		}
		else if (content.StartsWith("$GPRMC"))
		{
			IsGpsConnected = NmeaParser.ExtractGpsStatus(content);
			
			if (!IsGpsConnected)
				return;
			
			double speedKnots = NmeaParser. ExtractSpeed(content);
			
			double speedMps = speedKnots * 0.514444;
			if (speedMps < .23)
			{
				speedMps = 0;
			}
			
			CurrentSpeed = speedMps * 1.852;
		}
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_gpsSerialPort.Dispose();
		return Task.CompletedTask;
	}
}