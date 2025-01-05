using System.Timers;
using AutoTf.Logging;
using Timer = System.Timers.Timer;

namespace AutoTf.CentralBridgeOS.Services;

public class NetworkManager : IDisposable
{
	private readonly Logger _logger = Statics.Logger;
	private readonly Timer _syncTimer = new Timer(150000);
	private readonly FileSystemWatcher _watcher = new FileSystemWatcher();
	
	public void Initialize()
	{
		// Check for internet
		// Sync MAC Addresses
		// Start listening for new devices
		if (NetworkConfigurator.IsInternetAvailable())
		{
			TrySync();
			StartInternetListener();
		}

		StartConnectionListener();
	}

	private void StartConnectionListener()
	{
		const string leasesFilePath = "/var/lib/misc/dnsmasq.leases";

		_watcher.Path = Path.GetDirectoryName(leasesFilePath)!;
		_watcher.Filter = Path.GetFileName(leasesFilePath);
		_watcher.Changed += OnLeasesFileChanged;

		_watcher.EnableRaisingEvents = true;
	}

	private void OnLeasesFileChanged(object sender, FileSystemEventArgs e)
	{
		_logger.Log("A new device has connected on the hotspot at " + e.FullPath);
		_logger.Log("Device information:");
		string[] lines = File.ReadAllLines(e.FullPath);
		foreach (string line in lines)
		{
			// time, MAC, IP, name, id
			string[] parts = line.Split(' ');

			if (parts.Length <= 1) 
				continue;
			
			_logger.Log("MAC: " + parts[1]);
			_logger.Log("IP: " + parts[2]);
			_logger.Log("Host: " + parts[3]);
		}
	}

	private void StartInternetListener()
	{
		_syncTimer.Elapsed += SyncSyncTimerElapsed;
		_syncTimer.Start();
		
		_logger.Log("Started Sync timer.");
	}

	private void SyncSyncTimerElapsed(object? sender, ElapsedEventArgs e)
	{
		if (NetworkConfigurator.IsInternetAvailable())
		{
			_logger.Log("Periodic sync check started.");
			TrySync();
			return;
		}
		
		_logger.Log("VERBOSE: Train has left internet connection. Could not sync.");
	}

	private void TrySync()
	{
		_logger.Log("Trying to sync...");
	}

	public void Dispose()
	{
		_syncTimer.Dispose();
	}
}