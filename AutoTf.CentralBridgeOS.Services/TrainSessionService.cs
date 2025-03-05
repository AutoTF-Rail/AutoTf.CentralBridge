using AutoTf.CentralBridgeOS.Models;
using AutoTf.Logging;
using Microsoft.Extensions.Hosting;

namespace AutoTf.CentralBridgeOS.Services;

public class TrainSessionService : IHostedService
{
	private readonly FileManager _fileManager;
	private readonly Logger _logger;

	private static string? _username;
	private static string? _password;
	private static string? _evuName;
	private static string? _ssid;

	public TrainSessionService(FileManager fileManager, Logger logger)
	{
		_fileManager = fileManager;
		_logger = logger;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		LocalServiceState = LoadServiceState();
		
		_logger.Log($"Starting up at {DateTime.Now:hh:mm:ss} for EVU {EvuName} with service state {LocalServiceState}.");
		
		return Task.CompletedTask;
	}
	
	// Yes these two things are stored in plain text. If you get access to the file system/RAM, you have access to the train. Or have broken into it...
	public string Username => _username ??= _fileManager.ReadFile("username");
	public string Password => _password ??= _fileManager.ReadFile("password");
	public string EvuName => _evuName ??= _fileManager.ReadFile("evuName");
	public string Ssid => _ssid ??= "CentralBridge-" + _fileManager.ReadFile("trainId", Statics.GenerateRandomString());
	
	public BridgeServiceState LocalServiceState { get; private set; }
	
	private BridgeServiceState LoadServiceState()
	{
		string[] lines = File.ReadAllLines("/proc/meminfo");
        
		foreach (string line in lines)
		{
			if (!line.StartsWith("MemTotal:")) 
				continue;
            
			string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			long memTotalMb = long.Parse(parts[1]) / 1024;

			return memTotalMb > 3000 ? BridgeServiceState.Master : BridgeServiceState.Slave;
		}

		return BridgeServiceState.Unknown;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}