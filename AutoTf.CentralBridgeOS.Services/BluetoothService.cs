using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Services;

public class BluetoothService : IDisposable
{
	private readonly Logger _logger = Statics.Logger;
	private int _instanceId = 2;

	public BluetoothService()
	{
		Statics.ShutdownEvent += Dispose;
		StartBeacon();
	}

	private void StartBeacon()
	{
		try
		{
			string hexMessage = StringToHex(Statics.CurrentSsid);
			int length = hexMessage.Length / 2 + 1;
			string lengthByte = length.ToString("X2");
			string adData = $"{lengthByte}09{hexMessage}";

			_logger.Log("BLUETOOTH: Trying to start BLE as: " + adData);
			
			string command = $"btmgmt add-adv -d {adData} {_instanceId}";

			CommandExecuter.ExecuteSilent(command, false);
		}
		catch (Exception e)
		{
			_logger.Log("BLUETOOTH: ERROR: Bluetooth beacon threw an error");
			_logger.Log($"BLUETOOTH: ERROR: {e.Message}");
		}
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

	public void Dispose()
	{
		string command = $"btmgmt remove-adv {_instanceId}";
		CommandExecuter.ExecuteSilent(command, true);
		
		_logger.Log("BLUETOOTH: Beacon removed.");
	}
}