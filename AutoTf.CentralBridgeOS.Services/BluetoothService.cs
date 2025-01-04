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
			Connection connection = new Connection(Address.System);
			await connection.ConnectAsync();

			Advertisement advertisement = new Advertisement();
			await connection.RegisterObjectAsync(advertisement);

			ILEAdvertisingManager? manager = connection.CreateProxy<ILEAdvertisingManager>("org.bluez", new ObjectPath("/org/bluez/hci0"));
			await manager.RegisterAdvertisementAsync(new ObjectPath(BeaconPath), new Dictionary<string, object>());

			await Task.Delay(Timeout.Infinite, cancellationToken);
		}
		catch (TaskCanceledException)
		{
			Console.WriteLine("Beacon stopped.");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
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

public class Advertisement : IDBusObject, ILEAdvertisement
{
	public ObjectPath ObjectPath => new ObjectPath("/org/bluez/example/advertisement");

	public Task ReleaseAsync()
	{
		Console.WriteLine("Advertisement released.");
		return Task.CompletedTask;
	}

	public IDictionary<string, object> GetProperties()
	{
		return new Dictionary<string, object>
		{
			{ "Type", "broadcast" },
			{ "ServiceUUIDs", new[] { "12345678-1234-5678-1234-56789abcdef0" } },
			{ "LocalName", "SimpleBeacon" },
			{ "ManufacturerData", new Dictionary<ushort, object>
				{
					{ 0x004C, Encoding.UTF8.GetBytes("Hello World!") }
				}
			}
		};
	}
}

[DBusInterface("org.bluez.LEAdvertisement1")]
public interface ILEAdvertisement : IDBusObject
{
	Task ReleaseAsync();
}