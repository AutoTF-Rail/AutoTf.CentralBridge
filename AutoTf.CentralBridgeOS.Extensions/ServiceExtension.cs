using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridgeOS.Extensions;

public static class ServiceExtension
{
	// This method is used to create a singleton that is also a hostedservice at the same time.
	// Singleton cause we need it in controllers, and hostedservice because it has to do something on startup (e.g. sync)
	public static void AddHostedSingleton<THostedService>(this IServiceCollection collection) 
		where THostedService : class, IHostedService
	{
		collection.AddSingleton<IHostedService>();
		collection.AddHostedService(provider => provider.GetRequiredService<THostedService>());
	}
}