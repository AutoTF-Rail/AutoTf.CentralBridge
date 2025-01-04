using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Services;

public static class Statics
{
	public static Logger Logger = new Logger();
	public static string CurrentSsid { get; set; }
}