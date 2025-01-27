using AutoTf.CentralBridgeOS.Services;
using AutoTf.CentralBridgeOS.Services.Sync;
using AutoTf.Logging;
using AutoTf.SerialProtocol;
using AutoTf.SerialProtocol.Models;

namespace AutoTf.CentralBridgeOS.Server;

public class Program
{
	private static readonly Logger _logger = Statics.Logger;
	private static readonly HotspotService _hotspot = new HotspotService();
	private static readonly BluetoothService _bluetoothService = new BluetoothService();
	private static readonly NetworkManager _netManager = new NetworkManager();
	private static readonly FileManager _fileManager = new FileManager();
	private static readonly CameraService _cameraService = new CameraService();
	
	public static void Main(string[] args)
	{
		_logger.Log("Starting up----------------------------");
		_logger.Log("Starting for EVU: " + Statics.EvuName);
		
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
		
		builder.Services.AddControllers();
		builder.Services.AddSingleton(_cameraService);
		builder.Services.AddSingleton<BluetoothService>();
		builder.Services.AddSingleton(_fileManager);
		builder.Services.AddSingleton<CodeValidator>();
		builder.Services.AddSingleton(_netManager);
		builder.Services.AddSingleton<SyncManager>(new SyncManager(_fileManager, _cameraService));
		builder.Services.AddSingleton<ISerialService>(new SerialProtocol.SerialProtocol(_logger));
		

		WebApplication app = builder.Build();
		
		app.MapControllers();

		if (!ConfigureNetwork())
			_logger.Log("Could not start hotspot.");
		
		BluetoothService bluetoothService = app.Services.GetRequiredService<BluetoothService>();
		bluetoothService.StartBeaconAsync();

		app.Lifetime.ApplicationStopping.Register(() =>
		{
			Statics.ShutdownEvent?.Invoke();
			bluetoothService.RemoveBeacon();
			_netManager.Dispose();
		});
		
		app.Run("http://0.0.0.0:80");
	}

	private static bool ConfigureNetwork()
	{
		_logger.Log("Configuring network");
		
		string interfaceName = "wlan1";
		string ssid = "CentralBridge-" + _fileManager.ReadFile("trainNumber", Statics.GenerateRandomString());
		
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
}