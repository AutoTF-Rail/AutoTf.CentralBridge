using AutoTf.CentralBridgeOS.Services;
using AutoTf.CentralBridgeOS.Services.Sync;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Server;

public static class Program
{
	private static readonly Logger Logger = Statics.Logger;
	
	public static void Main(string[] args)
	{
		Logger.Log("Starting up----------------------------");
		Logger.Log("Starting for EVU: " + Statics.EvuName);
		
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
		
		NetworkManager unused = new NetworkManager();
		
		FileManager fileManager = new FileManager();
		CameraService cameraService = new CameraService();
		HotspotService hotspotService = new HotspotService(fileManager);
		
		builder.Services.AddControllers();
		builder.Services.AddSingleton(fileManager);
		builder.Services.AddSingleton(cameraService);
		builder.Services.AddSingleton<CodeValidator>();
		builder.Services.AddSingleton(new SyncManager(fileManager, cameraService));
		
		if (!hotspotService.Configure())
			Logger.Log("HOTSPOT: Could not start hotspot.");
		
		// Bluetooth needs the SSID to be set 
		BluetoothService unused1 = new BluetoothService();

		builder.Services.Configure<HostOptions>(x => x.ShutdownTimeout = TimeSpan.FromSeconds(20));

		WebApplication app = builder.Build();
		
		app.MapControllers();

		app.UseWebSockets();
		
		app.Lifetime.ApplicationStopping.Register(() =>
		{
			Statics.ShutdownEvent.Invoke();
		});
		
		app.Run("http://0.0.0.0:80");
	}
}