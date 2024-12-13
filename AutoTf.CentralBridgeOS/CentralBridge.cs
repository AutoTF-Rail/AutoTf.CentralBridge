using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS;

public class CentralBridge : IDisposable
{
	private readonly Logger _logger = new Logger();
	private readonly Hotspot _hotspot;

	public void Initialize()
	{
		// _logger.Log("Startup");
		string interfaceName = "wlan1";
		string ssid = "MyHotspot";
		string password = "StrongPassword123";
		Console.WriteLine("Startup");
		try
		{
			_hotspot.StartWifi(interfaceName, ssid, password);
			Console.WriteLine("WiFi hotspot started successfully!");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
		}
	}


	public void Dispose()
	{
		// TODO release managed resources here
	}
}