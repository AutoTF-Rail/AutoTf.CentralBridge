using AutoTf.CentralBridgeOS.Models;

namespace AutoTf.CentralBridgeOS.Services;

public class TrainSessionService
{
	private readonly FileManager _fileManager;
	
	private static string? _username;
	private static string? _password;
	private static string? _evuName;

	public TrainSessionService(FileManager fileManager)
	{
		_fileManager = fileManager;
	}
	
	// Yes these two things are stored in plain text. If you get access to the file system/RAM, you have access to the train. Or have broken into it...
	public string Username => _username ??= _fileManager.ReadFile("username");
	public string Password => _password ??= _fileManager.ReadFile("password");
	public string EvuName => _evuName ??= _fileManager.ReadFile("evuName");
	
	public BridgeServiceState LocalServiceState { get; set; } = BridgeServiceState.Unknown;
}