using AutoTf.CentralBridgeOS.Services;
using AutoTf.CentralBridgeOS.Services.Sync;
using AutoTf.CentralBridgeOS.TrainModels;
using AutoTf.CentralBridgeOS.TrainModels.Models;
using Logger = AutoTf.Logging.Logger;

namespace AutoTf.CentralBridgeOS.Server;

public static class Program
{
	private static readonly Logger Logger = Statics.Logger;
	
	public static void Main(string[] args)
	{
		try
		{
			Logger.Log("Starting up----------------------------");
			Logger.Log("Starting for EVU: " + Statics.EvuName);
			
			AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
			
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
		catch (Exception e)
		{
			Console.WriteLine("Root error:");
			Console.WriteLine(e);
		}
	}

	private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		throw new NotImplementedException();
	}

	private static void RegisterTrains(IServiceCollection builderServices)
	{
		builderServices.AddSingleton<DesiroHC>();
		builderServices.AddSingleton<DesiroML>();
		builderServices.AddSingleton<DefaultModel>();
	}
}