using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Services;

public class NetworkConfigurator
{
	private static readonly Logger _logger = Statics.Logger;
	
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

		_logger.Log("Got no internet connection.");
		return false;
	}
	
	public static void SetStaticIpAddress(string ipAddress, string subnetMask, string newInterface = "eth0")
	{
		try
		{
			if (CheckIpAddress(newInterface))
				return;
			_logger.Log("Setting Static IP.");
			string setIpCommand = $"sudo ip addr add {ipAddress}/{subnetMask} dev {newInterface}";
			string bringUpInterfaceCommand = $"sudo ip link set {newInterface} up";

			CommandExecuter.ExecuteSilent(setIpCommand, false);
			CommandExecuter.ExecuteSilent(bringUpInterfaceCommand, false);

			_logger.Log($"Set {ipAddress} on {newInterface} with subnet mask {subnetMask}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"An error occurred while setting IP: {ex.Message}");
			throw;
		}
	}
	
	public static bool CheckIpAddress(string interfaceName)
	{
		string checkIpCommand = $"ip addr show {interfaceName}";
		string output = CommandExecuter.ExecuteCommand(checkIpCommand);

		if (output.Contains("inet"))
		{
			_logger.Log($"Current IP settings for {interfaceName}:");
			_logger.Log(output.Split('\n')[1].Trim());
			return true;
		}

		_logger.Log($"{interfaceName} does not have an IP address set.");
		return false;
	}
	
	
	public static string? FindUsbEthernetAdapter()
	{
		_logger.Log("Checking for USB Lan Adapter");
		string command = "udevadm info -e | grep -B20 -A10 'ID_VENDOR_ID=0bda' | grep 'INTERFACE='";
		string output = CommandExecuter.ExecuteCommand(command);

		Match match = Regex.Match(output, "INTERFACE=(\\w+)");
		
		if (match.Success)
		{
			_logger.Log("Found interface at: " + match.Groups[1].Value);
			return match.Groups[1].Value;
		}

		return null;
	}
}