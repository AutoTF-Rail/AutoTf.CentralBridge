using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;

namespace AutoTf.CentralBridge.Models.Static;

public static class NetworkConfigurator
{
	private static readonly ILogger Logger = Statics.Logger;
	
	public static bool IsInternetAvailable()
	{
		try
		{
			using var ping = new Ping();

			PingReply reply = ping.Send("1.1.1.1", 1500);
			if (reply.Status == IPStatus.Success)
			{
				return true;
			}
		}
		catch
		{
			// ignored
		}

		return false;
	}
	
	public static void SetStaticIpAddress(string ipAddress, string subnetMask, string newInterface = "eth0")
	{
		try
		{
			if (CheckIpAddress(newInterface))
				return;
			
			Logger.LogTrace("Setting Static IP.");
			string setIpCommand = $"ip addr add {ipAddress}/{subnetMask} dev {newInterface}";
			string bringUpInterfaceCommand = $"ip link set {newInterface} up";

			CommandExecuter.ExecuteSilent(setIpCommand, false);
			CommandExecuter.ExecuteSilent(bringUpInterfaceCommand, false);

			Logger.LogTrace($"Set {ipAddress} on {newInterface} with subnet mask {subnetMask}");
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, $"An error occurred while setting IP: {ex.Message}");
			throw;
		}
	}
	
	public static bool CheckIpAddress(string interfaceName)
	{
		string checkIpCommand = $"ip addr show {interfaceName}";
		string output = CommandExecuter.ExecuteCommand(checkIpCommand);

		if (output.Contains("inet"))
		{
			// string currIp = output.Split('\n').FirstOrDefault(x => x.Contains("inet"))?.Split("brd")[0].Replace("inet", "").Trim()!;
			return true;
		}

		Logger.LogInformation($"{interfaceName} does not have an IP address set.");
		return false;
	}
}