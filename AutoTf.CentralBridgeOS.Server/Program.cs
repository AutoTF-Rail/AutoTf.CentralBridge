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
			Logger.Log($"Assembling at {DateTime.Now:hh:mm:ss}.");
			
			WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
			
			ConfigureServices(builder);
			
			WebApplication app = builder.Build();
			
			app.MapControllers();

			app.UseWebSockets();
			
			// app.Lifetime.ApplicationStopping.Register(() =>
			// {
			// 	Statics.ShutdownEvent.Invoke();
			// });
			
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

	private static void ConfigureServices(WebApplicationBuilder builder)
	{
		builder.Services.AddControllers();
			
		builder.Services.AddSingleton(Logger);
		builder.Services.AddSingleton<TrainSessionService>();
		builder.Services.AddSingleton<FileManager>();
		builder.Services.AddSingleton<CodeValidator>();
		
		builder.Services.AddHostedService<NetworkManager>();
		builder.Services.AddHostedService<CameraService>();
		builder.Services.AddHostedService<HotspotService>();
		builder.Services.AddHostedService<UdpProxyService>();
		builder.Services.AddHostedService<MotorManager>();
		builder.Services.AddHostedService<SyncManager>();
		builder.Services.AddHostedService<BluetoothService>();
		
			
		builder.Services.AddSingleton<ITrainModel>(provider =>
		{
			FileManager fileManager = provider.GetRequiredService<FileManager>();
			string trainName = fileManager.ReadFile("TrainName");
    
			return TrainResolver.Resolve(provider, trainName);
		});

		RegisterTrains(builder.Services);

		builder.Services.Configure<HostOptions>(x => x.ShutdownTimeout = TimeSpan.FromSeconds(20));
	}

	private static void RegisterTrains(IServiceCollection builderServices)
	{
		builderServices.AddSingleton<DesiroHC>();
		builderServices.AddSingleton<DesiroML>();
		builderServices.AddSingleton<DefaultModel>();
	}
}