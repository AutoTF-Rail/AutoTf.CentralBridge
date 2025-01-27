using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Services.Sync;

public class MacSync : Sync
{
	private string[] _latestList;
	
	public MacSync(Logger logger, FileManager fileManager) : base(logger, fileManager)
	{
		Statics.SyncEvent += Sync;
		_latestList = File.ReadAllLines("/etc/hostapd/accepted_macs.txt");
	}

	private async void Sync()
	{
		try
		{
			if (await CheckForNewMacAddresses())
				await SyncMac();
		}
		catch (Exception e)
		{
			_logger.Log("ERROR: Failed to sync mac addresses.");
			_logger.Log("ERROR: " + e.Message);
		}
	}
	
	private async Task SyncMac()
	{
		try
		{
			_logger.Log("SYNC: Syncing MAC Addresses.");
			string[] result = await SendGetArray("/sync/mac/macAddress");

			if (result.SequenceEqual(_latestList))
				return;

			_logger.Log($"{result.Length} MAC addresses received.");
			
			_latestList = result;
			
			File.WriteAllLines("/etc/hostapd/accepted_macs.txt", result);
			CommandExecuter.ExecuteSilent("sudo systemctl restart hostapd", false);
		}
		catch (Exception e)
		{
			_logger.Log("SYNC: ERROR: An error occured while syncing MAC Addresses.");
			_logger.Log(e.Message);
		}
	}
	
	private async Task<bool> CheckForNewMacAddresses()
	{
		try
		{
			_logger.Log("SYNC: Checking for new MAC addresses.");
			
			string response = await SendGet("/sync/mac/lastmacaddrsupdate");

			string lastNewKeysTimestamp = _fileManager.ReadFile("lastKeysTimestamp", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
			
			if (response == lastNewKeysTimestamp)
				return false;

			_fileManager.WriteAllText("lastKeysTimestamp", response);
			return true;
		}
		catch (Exception e)
		{
			_logger.Log("SYNC: ERROR: An error occured while checking for new MAC addresses.");
			_logger.Log(e.Message);
			return false;
		}
	}
}