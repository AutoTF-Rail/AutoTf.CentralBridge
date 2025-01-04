using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Services;

public class BluetoothService
{
	private readonly Logger _logger = Statics.Logger;
	private int _instanceId = 2;

	public void StartBeaconAsync()
	{
		try
		{
			string hexMessage = StringToHex(Statics.CurrentSsid);

			string command = $"btmgmt add-adv -d 09{hexMessage} {_instanceId}";

			CommandExecuter.ExecuteSilent(command, false);
		}
		catch (Exception e)
		{
			_logger.Log("Error: Bluetooth beacon threw an error");
			_logger.Log($"Error: {e.Message}");
			_logger.Log($"StackTrace: {e.StackTrace}");
		}
	}
	
	public void RemoveBeacon()
	{
		string command = $"btmgmt remove-adv {_instanceId}";
		CommandExecuter.ExecuteSilent(command, true);
		Console.WriteLine("Beacon removed.");
	}
	
	private static string StringToHex(string input)
	{
		char[] chars = input.ToCharArray();
		string hexOutput = string.Empty;

		foreach (char c in chars)
		{
			hexOutput += ((int)c).ToString("X2");
		}

		return hexOutput;
	}
}