using AutoTf.Logging;
using DotnetBleServer.Advertisements;
using DotnetBleServer.Core;

namespace AutoTf.CentralBridgeOS.Services;

public class BluetoothService : IDisposable
{
	private readonly Logger _logger = Statics.Logger;
	
	private ServerContext serverContext;

	public async void StartBeacon()
	{
		try
		{
			serverContext = new ServerContext();
		
			AdvertisementProperties advertisementProperties = new AdvertisementProperties
			{
				Type = "peripheral",
				ServiceUUIDs = new[] { "12345678-1234-5678-1234-56789abcdef0" },
				LocalName = "ExampleBeacon"
			};

			AdvertisingManager advertisingManager = new AdvertisingManager(serverContext);
			await advertisingManager.CreateAdvertisement(advertisementProperties);
		}
		catch (Exception e)
		{
			_logger.Log("Error: Bluetooth beacon threw an error");
			_logger.Log($"Error: {e.Message}");
		}
	}

	public void Dispose()
	{
		serverContext.Dispose();
	}
}