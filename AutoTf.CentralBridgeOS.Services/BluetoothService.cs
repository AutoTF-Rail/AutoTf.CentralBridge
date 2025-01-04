using System.Text;
using AutoTf.Logging;
using Tmds.DBus;

namespace AutoTf.CentralBridgeOS.Services;

public class BluetoothService : IDisposable
{
	private readonly Logger _logger = Statics.Logger;
	private CancellationTokenSource _cancellationTokenSource;
	private const string BeaconPath = "/org/bluez/example/advertisement";

	public async Task StartBeaconAsync(CancellationToken cancellationToken)
	{
		try
		{
			_logger.Log("Connecting to BlueZ D-Bus...");

			Connection connection = new Connection(Address.System);
			await connection.ConnectAsync();

			Advertisement advertisement = new Advertisement(BeaconPath, _logger);
			await connection.RegisterObjectAsync(advertisement);
			
			var bluezPath = "/org/bluez/hci0";
			var manager = connection.CreateProxy<ILEAdvertisingManager>("org.bluez", new ObjectPath(bluezPath));
			await manager.RegisterAdvertisementAsync(new ObjectPath(BeaconPath), new Dictionary<string, object>());

			_logger.Log("Bluetooth beacon started successfully!");

			await Task.Delay(Timeout.Infinite, cancellationToken);

			await manager.UnregisterAdvertisementAsync(new ObjectPath(BeaconPath));
			connection.UnregisterObject(new ObjectPath(BeaconPath));
			_logger.Log("Bluetooth beacon stopped.");
			
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
	}
}

[DBusInterface("org.bluez.LEAdvertisingManager1")]
public interface ILEAdvertisingManager : IDBusObject
{
	Task RegisterAdvertisementAsync(ObjectPath advertisement, IDictionary<string, object> options);
	Task UnregisterAdvertisementAsync(ObjectPath advertisement);
}

[DBusInterface("org.bluez.LEAdvertisement1")]
public interface ILEAdvertisement : IDBusObject
{
	Task ReleaseAsync();
}

[DBusInterface("org.freedesktop.DBus.Properties")]
public interface IProperties : IDBusObject
{
	Task<IDictionary<string, object>> GetAllAsync(string interfaceName);
}

public class Advertisement : IDBusObject, ILEAdvertisement, IProperties
{
	private readonly Logger _logger;
	private readonly ObjectPath _path;

	public Advertisement(string path, Logger logger)
	{
		_path = new ObjectPath(path);
		_logger = logger;
	}

	public ObjectPath ObjectPath => _path;

	public Task ReleaseAsync()
	{
		_logger.Log("Advertisement released.");
		return Task.CompletedTask;
	}
	
	public async Task<IDictionary<string, object>> GetAllAsync(string interfaceName)
	{
		_logger.Log($"GetAll called for {interfaceName}");
		return GetProperties();
	}
	
	public IDictionary<string, object> GetProperties()
	{
		return new Dictionary<string, object>
		{
			{ "Type", "broadcast" },
			{ "ServiceUUIDs", new[] { "12345678-1234-5678-1234-56789abcdef0" } },
			{ "LocalName", "ExampleBeacon" },
			{ "ManufacturerData", new Dictionary<ushort, object>
				{
					{ 0x004C, Encoding.UTF8.GetBytes("Meow") }
				}
			}
		};
	}
}