using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS;

public class CentralBridge : IDisposable
{
	private readonly Logger _logger = new Logger();
	private Hotspot _hotspot;

	public void Initialize()
	{
		_hotspot = new Hotspot(_logger);
		_logger.Log("Startup");
		string interfaceName = "wlan1";
		string ssid = "CentralBridge-" + GenerateRandomString();
		string password = "CentralBridgePW";
		Console.WriteLine("Startup");
		
		try
		{
			_hotspot.StartWifi(interfaceName, ssid, password);
			_logger.Log($"Started WIFI as:  {ssid}");
			Console.WriteLine($"WiFi hotspot started successfully as {ssid}!");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
			_logger.Log($"Error: {ex.Message}");
		}
	}
	
	public static string GenerateRandomString()
	{
		Random random = new Random();
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		return new string(Enumerable.Repeat(chars, 10)
			.Select(s => s[random.Next(s.Length)]).ToArray());
	}

	public void Dispose()
	{
		// TODO release managed resources here
	}
}