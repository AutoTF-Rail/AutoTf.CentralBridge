using AutoTf.Logging;
using DotnetBleServer.Advertisements;
using DotnetBleServer.Core;

namespace AutoTf.CentralBridgeOS.Services;

public class BluetoothService : IDisposable
{
	private readonly Logger _logger = Statics.Logger;
	private CancellationTokenSource _cancellationTokenSource;
	private ServerContext serverContext;

	public async Task StartBeacon(CancellationToken cancellationToken)
	{
		try
		{
			serverContext = new ServerContext();
			await serverContext.Connect();
		
			AdvertisementProperties advertisementProperties = new AdvertisementProperties
			{
				Type = "peripheral",
				ServiceUUIDs = new[] { "12345678-1234-5678-1234-56789abcdef0" },
				LocalName = "ExampleBeacon",
				ManufacturerData = new Dictionary<string, object>()
				{
					{"meow", "meow"}
				}
			};

			AdvertisingManager advertisingManager = new AdvertisingManager(serverContext);
			await advertisingManager.CreateAdvertisement(advertisementProperties);
			
			_logger.Log("Bluetooth beacon started successfully!");
			await Task.Delay(Timeout.Infinite, cancellationToken);
		}
		catch (TaskCanceledException)
		{
			_logger.Log("Bluetooth beacon stopped.");
		}
		catch (Exception e)
		{
			_logger.Log("Error: Bluetooth beacon threw an error");
			_logger.Log($"Error: {e.Message}");
			_logger.Log($"StackTrace: {e.StackTrace}");
		}
	}
	
	public void StopBeacon()
	{
		_cancellationTokenSource?.Cancel();
		_logger.Log("Stopping Bluetooth beacon...");
	}

	public void Dispose()
	{
		_cancellationTokenSource?.Cancel();
		serverContext.Dispose();
	}
}