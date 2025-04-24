using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Models.Enums;
using AutoTf.CentralBridgeOS.Models.Interfaces;
using AutoTf.CentralBridgeOS.Models.Static;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Services;

public class TrainSessionService : ITrainSessionService
{
	private readonly IFileManager _fileManager;

	private static string? _username;
	private static string? _password;
	private static string? _evuName;
	private static string? _ssid;
	private static BridgeServiceState? _localServiceState;

	public TrainSessionService(IFileManager fileManager)
	{
		_fileManager = fileManager;
	}
	
	// Yes these two things are stored in plain text. If you get access to the file system/RAM, you have access to the train. Or have broken into it...
	public string Username => _username ??= _fileManager.ReadFile("username");
	public string Password => _password ??= _fileManager.ReadFile("password");
	public string EvuName => _evuName ??= _fileManager.ReadFile("evuName");
	public string Ssid => _ssid ??= "CentralBridge-" + _fileManager.ReadFile("trainId", Statics.GenerateRandomString());
	
	public BridgeServiceState LocalServiceState => _localServiceState ??= LoadServiceState();
	
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
}