using AutoTf.CentralBridgeOS.Services;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS;

public class CentralBridge : IDisposable
{
	private readonly Logger _logger = Statics.Logger;
	private Hotspot _hotspot;

	public void Initialize()
	{
		_hotspot = new Hotspot();
		_logger.Log("Startup");
		
		string interfaceName = "wlan1";
		string ssid = "CentralBridge-" + GenerateRandomString();
		string password = "CentralBridgePW";
		
		try
		{
			NetworkConfigurator netConf = new NetworkConfigurator();
			netConf.SetStaticIpAddress("192.168.0.1", "24");
			netConf.SetStaticIpAddress("192.168.1.1", "24", "wlan1");
			_logger.Log("Successfully set local IP.");
			
			_hotspot.StartWifi(interfaceName, ssid, password);
			_hotspot.SetupDhcpConfig(interfaceName);
			_logger.Log($"Started WIFI as:  {ssid}");
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
		_hotspot.Dispose();
		_logger.Dispose();
	}
}