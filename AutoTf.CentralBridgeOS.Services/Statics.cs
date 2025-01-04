using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Services;

public static class Statics
{
	public static Logger Logger = new Logger(true);
	public static string CurrentSsid { get; set; }
}