using AutoTf.CentralBridge.Models.Static;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoTf.CentralBridge.Services.Network;

public class NetworkManager : IHostedService
{
	private readonly ILogger<NetworkManager> _logger;
	private readonly FileSystemWatcher _watcher = new FileSystemWatcher();

	private HashSet<string> _knownDevices = new HashSet<string>();

	public NetworkManager(ILogger<NetworkManager> logger)
	{
		_logger = logger;
	}

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
			_logger.LogTrace("Leases file missing. Skipping device update.");
			return;
		}
		
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
				_logger.LogInformation($"New device connected with MAC {mac}. New device count: {currentDevices.Count}.");
			}
		}

		foreach (string mac in _knownDevices)
		{
			if (!currentDevices.Contains(mac))
			{
				_logger.LogTrace($"Device disconnected: {mac}");
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