using AutoTf.Logging;
using Timer = System.Timers.Timer;

namespace AutoTf.CentralBridgeOS.Services;

public class NetworkManager : IDisposable
{
	private readonly Logger _logger = Statics.Logger;
	private readonly FileSystemWatcher _watcher = new FileSystemWatcher();

	public NetworkManager()
	{
		Statics.ShutdownEvent += Dispose;
		Initialize();
	}
	
	private void Initialize()
	{
		// Check for internet
		// Sync MAC Addresses
		// Start listening for new devices

		StartConnectionListener();
	}

	private void StartConnectionListener()
	{
		const string leasesFilePath = "/var/lib/misc/dnsmasq.leases";

		_watcher.Path = Path.GetDirectoryName(leasesFilePath)!;
		_watcher.Filter = Path.GetFileName(leasesFilePath);
		_watcher.Changed += OnLeasesFileChanged;
		_watcher.Deleted += OnDeleted;

		_watcher.EnableRaisingEvents = true;
	}

	private void OnDeleted(object sender, FileSystemEventArgs e)
	{
		_logger.Log("A device has disconnected from the hotspot.");
		string[] lines = File.ReadAllLines(e.FullPath);
		
		foreach (string line in lines)
		{
			// time, MAC, IP, name, id
			string[] parts = line.Split(' ');

			if (parts.Length <= 1) 
				continue;
			
			_logger.Log("Device:");
			_logger.Log("MAC: " + parts[1]);
			_logger.Log("IP: " + parts[2]);
			_logger.Log("Host: " + parts[3]);
			if (Statics.AllowedDevices.Contains(parts[1]))
				Statics.AllowedDevices.Remove(parts[1]);
		}
	}

	private void OnLeasesFileChanged(object sender, FileSystemEventArgs e)
	{
		_logger.Log("A new device has connected on the hotspot.");
		string[] lines = File.ReadAllLines(e.FullPath);
		
		foreach (string line in lines)
		{
			// time, MAC, IP, name, id
			string[] parts = line.Split(' ');

			if (parts.Length <= 1) 
				continue;
			
			_logger.Log("New device:");
			_logger.Log("MAC: " + parts[1]);
			_logger.Log("IP: " + parts[2]);
			_logger.Log("Host: " + parts[3]);
		}
	}

	private void PendingDeviceElapsed(string macAddr)
	{
		_logger.Log($"Timer elapsed for {macAddr}. Kicking device from network.");
		CommandExecuter.ExecuteSilent($"hostapd_cli deauthenticate {macAddr}", true);
	}

	public void Dispose()
	{
		_watcher.Dispose();
	}
}