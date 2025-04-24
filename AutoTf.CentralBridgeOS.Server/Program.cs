using AutoTf.CentralBridgeOS.CameraService;
using AutoTf.CentralBridgeOS.Extensions;
using AutoTf.CentralBridgeOS.Localise;
using AutoTf.CentralBridgeOS.Localise.Display;
using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Models.DataModels;
using AutoTf.CentralBridgeOS.Models.Interfaces;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.CentralBridgeOS.Services.Camera;
using AutoTf.CentralBridgeOS.Services.Gps;
using AutoTf.CentralBridgeOS.Services.Network;
using AutoTf.CentralBridgeOS.Sync;
using AutoTf.CentralBridgeOS.TrainModels;
using AutoTf.CentralBridgeOS.TrainModels.Models;
using AutoTf.CentralBridgeOS.TrainModels.Models.DesiroHC;
using AutoTf.CentralBridgeOS.TrainModels.Models.DesiroML;
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
			Statics.Logger.Log("Root Error:");
			Statics.Logger.Log(e.ToString());
		}
	}

	private static void ConfigureServices(WebApplicationBuilder builder)
	{
		builder.Services.AddControllers();
			
		builder.Services.AddSingleton(Logger);
		builder.Services.AddSingleton<TrainSessionService>();
		builder.Services.AddSingleton<IFileManager, FileManager>();
		builder.Services.AddSingleton<CodeValidator>();
		builder.Services.AddSingleton<IProxyManager, ProxyManager>();
		
		builder.Services.AddHostedService<NetworkManager>();
		builder.Services.AddHostedService<MainCameraService>();
		builder.Services.AddHostedService<HotspotService>();
		builder.Services.AddHostedService<BluetoothService>();
		builder.Services.AddHostedService<CameraManager>();
		
		builder.Services.AddHostedSingleton<IAicService, AicService>();
		builder.Services.AddHostedSingleton<MotionService>();
		builder.Services.AddHostedSingleton<MainCameraProxyService>();
		builder.Services.AddHostedSingleton<IMotorManager, MotorManager>();
		builder.Services.AddHostedSingleton<SyncManager>();
		builder.Services.AddHostedSingleton<IEbuLaService, EbuLaService>();
		builder.Services.AddHostedSingleton<ICcdService, CcdService>();
		builder.Services.AddHostedSingleton<LocaliseService>();
			
		builder.Services.AddSingleton<ITrainModel>(provider =>
		{
			IFileManager fileManager = provider.GetRequiredService<IFileManager>();
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
		builderServices.AddSingleton<FallBackTrain>();
	}
}