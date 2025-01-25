using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Services;

public static class Statics
{
	public static readonly Logger Logger = new Logger(true);
	public static string CurrentSsid { get; set; }

	public static string EvuName { get; set; }
	// Yes these two things are stored in plain text, anyone who gets access to the ram, or even the system will have access anyway, due to the code being open. 
	// There is nothing "to destroy" on the server anyway.
	public static string Username { get; set; }
	public static string Password { get; set; }
	public static Action? ShutdownEvent;
	public static Action? SyncEvent;
	
	// A list of all devices (tablets) MAC addresses that have logged in and are allowed to use the API.
	// This list should be checked against the given "macAddr" header before every request.
	// Just because a device is on the network, doesn't mean it's an authorized user.
	public static List<string> AllowedDevices { get; set; } = new List<string>();
	
	public static string GenerateRandomString()
	{
		Random random = new Random();
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		return new string(Enumerable.Repeat(chars, 10)
			.Select(s => s[random.Next(s.Length)]).ToArray());
	}
}