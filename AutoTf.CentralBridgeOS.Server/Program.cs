using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.CentralBridgeOS.Services.Sync;
using AutoTf.CentralBridgeOS.TrainModels;
using AutoTf.CentralBridgeOS.TrainModels.Models;
using Microsoft.AspNetCore.Components;
using Logger = AutoTf.Logging.Logger;

namespace AutoTf.CentralBridgeOS.Server;

public static class Program
{
	private static readonly Logger Logger = Statics.Logger;
	
	public static void Main(string[] args)
	{
		try
		{
			LoadServiceState();
			Logger.Log($"Starting up at {DateTime.Now:hh:mm:ss} for EVU {Statics.EvuName} with service state {Statics.ServiceState}.");
			
			WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
			builder.Logging.AddDebug();
			
			NetworkManager unused = new NetworkManager();
			
			FileManager fileManager = new FileManager();
			CameraService cameraService = new CameraService(Logger);
			HotspotService hotspotService = new HotspotService(fileManager);
			
			builder.Services.AddControllers();
			builder.Services.AddSingleton(Logger);
			builder.Services.AddSingleton(fileManager);
			builder.Services.AddSingleton(cameraService);
			builder.Services.AddSingleton<CodeValidator>();
			builder.Services.AddSingleton<UdpProxyService>();
			builder.Services.AddSingleton<MotorManager>();
			builder.Services.AddSingleton(new SyncManager(fileManager, cameraService));

			builder.Services.AddSingleton<ITrainModel>(provider => TrainResolver.Resolve(provider, fileManager.ReadFile("TrainName")));
			
			RegisterTrains(builder.Services);
			
			if (!hotspotService.Configure())
				Logger.Log("HOTSPOT: Could not start hotspot.");
			// By here we know if we are a master bridge or not.
			
			// Bluetooth needs the SSID to be set. We don't need to register this as it just runs in the background.
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
		catch (Exception e)
		{
			// This can be viewed when running "journalctl -u startupScript.service | tail -150"
			// But the logger should work anyways, so we try to log to it.
			Console.WriteLine("Root error:");
			Console.WriteLine(e.ToString());
			Statics.Logger.Log("Root Error:");
			Statics.Logger.Log(e.ToString());
		}
	}

	private static void RegisterTrains(IServiceCollection builderServices)
	{
		builderServices.AddSingleton<DesiroHC>();
		builderServices.AddSingleton<DesiroML>();
		builderServices.AddSingleton<DefaultModel>();
	}
	
	private static void LoadServiceState()
	{
		string[] lines = File.ReadAllLines("/proc/meminfo");
        
		foreach (string line in lines)
		{
			if (!line.StartsWith("MemTotal:")) 
				continue;
            
			string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			long memTotalMb = long.Parse(parts[1]) / 1024;

			Statics.ServiceState = memTotalMb > 3000 ? BridgeServiceState.Master : BridgeServiceState.Slave;
		}
	}
}