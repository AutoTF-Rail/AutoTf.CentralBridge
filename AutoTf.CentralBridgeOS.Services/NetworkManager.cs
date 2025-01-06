using System.Timers;
using AutoTf.Logging;
using Timer = System.Timers.Timer;

namespace AutoTf.CentralBridgeOS.Services;

public class NetworkManager : IDisposable
{
	private readonly Logger _logger = Statics.Logger;
	private readonly Timer _syncTimer = new Timer(150000);
	private readonly FileSystemWatcher _watcher = new FileSystemWatcher();

	public List<string> AcceptedDevices { get; private set; } = new List<string>();
	public Dictionary<string, Timer> PendingDevices { get; private set; } = new Dictionary<string, Timer>();
	public Action<string> DeviceSaidHelloEvent = null!;
	
	public void Initialize()
	{
		DeviceSaidHelloEvent += OnDeviceSaidHelloEvent;
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

	private void OnDeviceSaidHelloEvent(string deviceIp)
	{
		if (PendingDevices.ContainsKey(deviceIp))
		{
			PendingDevices.Remove(deviceIp);
			PendingDevices[deviceIp].Dispose();
		}

		if(!AcceptedDevices.Contains(deviceIp))
			AcceptedDevices.Add(deviceIp);
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
		_logger.Log("A new device has connected on the hotspot.");
		string[] lines = File.ReadAllLines(e.FullPath);
		
		foreach (string line in lines)
		{
			// time, MAC, IP, name, id
			string[] parts = line.Split(' ');

			if (parts.Length <= 1) 
				continue;
			if (AcceptedDevices.Contains(parts[2]))
				return;
			_logger.Log("New device:");
			_logger.Log("MAC: " + parts[1]);
			_logger.Log("IP: " + parts[2]);
			_logger.Log("Host: " + parts[3]);
			_logger.Log("Starting 4 minute timer for " + parts[1]);
			Timer timer = new Timer(240000);
			timer.Elapsed += (_, _) => TimerOnElapsed(parts[1]);
			timer.Start();
			PendingDevices.Add(parts[1], timer);
		}
	}

	private void TimerOnElapsed(string macAddr)
	{
		_logger.Log($"Timer elapsed for {macAddr}. Kicking device from network.");
		CommandExecuter.ExecuteSilent($"hostapd_cli deauthenticate {macAddr}", true);
		PendingDevices[macAddr].Dispose();
		PendingDevices.Remove(macAddr);
	}

	private void StartInternetListener()
	{
		_syncTimer.Elapsed += SyncSyncTimerElapsed;
		_syncTimer.Start();
		
		_logger.Log("Started Sync timer.");
	}

	private void SyncSyncTimerElapsed(object? sender, ElapsedEventArgs e)
	{
		_logger.Log("Checking for internet.");
		if (NetworkConfigurator.IsInternetAvailable())
		{
			_logger.Log("Got internet connection.");
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
		_logger.Log("Disposed sync timer.");
		_syncTimer.Dispose();
	}
}