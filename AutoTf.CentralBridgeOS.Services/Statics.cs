using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Services;

public static class Statics
{
	public static Logger Logger = new Logger(true);
	public static string CurrentSsid { get; set; }

	public static string EvuName { get; set; }
	// Yes these two things are stored in plain text, anyone who gets access to the ram, or even the system will have access anyway, due to the code being open. 
	// There is nothing "to destroy" on the server anyway.
	public static string Username { get; set; }
	public static string Password { get; set; }
}