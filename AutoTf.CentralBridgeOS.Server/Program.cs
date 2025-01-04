using AutoTf.CentralBridgeOS.Services;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Server;

public class Program
{
	private static readonly Logger _logger = Statics.Logger;
	private static readonly HotspotService _hotspot = new HotspotService();
	private static readonly BluetoothService _bluetoothService = new BluetoothService();
	
	public static void Main(string[] args)
	{
		_logger.Log("Starting up----------------------------");
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
		
		builder.Services.AddControllers();
		builder.Services.AddSingleton<BluetoothService>();

		WebApplication app = builder.Build();
		
		app.MapControllers();

		if (!ConfigureNetwork())
			return;
		
		BluetoothService bluetoothService = app.Services.GetRequiredService<BluetoothService>();
		bluetoothService.StartBeaconAsync();

		app.Lifetime.ApplicationStopping.Register(() =>
		{
			bluetoothService.RemoveBeacon();
		});
		
		app.Run();
	}

	private static bool ConfigureNetwork()
	{
		_logger.Log("Configuring network");
		
		string interfaceName = "wlan1";
		string ssid = "CentralBridge-" + GenerateRandomString();
		Statics.CurrentSsid = ssid;
		string password = "CentralBridgePW";
		
		try
		{
			NetworkConfigurator.SetStaticIpAddress("192.168.0.1", "24");
			NetworkConfigurator.SetStaticIpAddress("192.168.1.1", "24", "wlan1");
			_logger.Log("Successfully set local IP.");
			
			_hotspot.StartWifi(interfaceName, ssid, password);
			_hotspot.SetupDhcpConfig(interfaceName);
			
			_logger.Log($"Started WIFI as: {ssid}");
		}
		catch (Exception ex)
		{
			_logger.Log("Error: Could not configure network");
			_logger.Log($"Error: {ex.Message}");
			return false;
		}

		return true;
	}
	
	public static string GenerateRandomString()
	{
		Random random = new Random();
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		return new string(Enumerable.Repeat(chars, 10)
			.Select(s => s[random.Next(s.Length)]).ToArray());
	}
}