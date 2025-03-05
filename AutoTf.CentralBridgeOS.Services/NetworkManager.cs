using AutoTf.Logging;
using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridgeOS.Services;

public class NetworkManager : IHostedService
{
	private readonly Logger _logger = Statics.Logger;
	private readonly FileSystemWatcher _watcher = new FileSystemWatcher();

	private HashSet<string> _knownDevices = new HashSet<string>();

	public Task StartAsync(CancellationToken cancellationToken)
	{
		StartConnectionListener();
		return Task.CompletedTask;
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
		if (!File.Exists(e.FullPath))
		{
			_logger.Log("Leases file missing. Skipping device update.");
			return;
		}
		
		_logger.Log("Leases file changed. Checking connected devices...");
		HashSet<string> currentDevices = new HashSet<string>();
		string[] lines = File.ReadAllLines(e.FullPath);
		
		foreach (string line in lines)
		{
			string[] parts = line.Split(' ');
			if (parts.Length <= 1) 
				continue;
        
			string mac = parts[1];
			currentDevices.Add(mac);
        
			if (!_knownDevices.Contains(mac))
			{
				_logger.Log("New device connected:");
				_logger.Log($"MAC: {mac}");
				_logger.Log($"IP: {parts[2]}");
				_logger.Log($"Host: {parts[3]}");
			}
		}

		_logger.Log($"Previous devices: {_knownDevices.Count} | New devices: {currentDevices.Count}");
		foreach (string mac in _knownDevices)
		{
			if (!currentDevices.Contains(mac))
			{
				_logger.Log($"Device disconnected: {mac}");
				if (Statics.AllowedDevices.Contains(mac))
					Statics.AllowedDevices.Remove(mac);
			}
		}

		_knownDevices = currentDevices;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_watcher.Dispose();
		return Task.CompletedTask;
	}
}