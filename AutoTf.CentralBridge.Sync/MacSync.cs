using AutoTf.CentralBridge.Models.Interfaces;
using AutoTf.CentralBridge.Models.Static;
using AutoTf.Logging;

namespace AutoTf.CentralBridge.Sync;

public class MacSync : Sync
{
	private string[] _latestList;
	
	public MacSync(Logger logger, IFileManager fileManager, ITrainSessionService trainSessionService) : base(logger, fileManager, trainSessionService)
	{
		Statics.SyncEvent += Sync;
		_latestList = File.ReadAllLines("/etc/hostapd/accepted_macs.txt");
	}

	private async void Sync()
	{
		try
		{
			if (!NetworkConfigurator.IsInternetAvailable())
				return;
			
			if (await CheckForNewMacAddresses())
				await SyncMac();
		}
		catch (Exception e)
		{
			Logger.Log("ERROR: Failed to sync mac addresses.");
			Logger.Log(e.ToString());
		}
	}
	
	private async Task SyncMac()
	{
		try
		{
			Logger.Log("Syncing MAC Addresses.");
			string[] result = await SendGetArray("/sync/mac/all");

			if (result.SequenceEqual(_latestList))
				return;

			Logger.Log($"{result.Length} MAC addresses received.");
			
			_latestList = result;
			
			File.WriteAllLines("/etc/hostapd/accepted_macs.txt", result);
			CommandExecuter.ExecuteSilent("sudo systemctl restart hostapd", false);
		}
		catch (Exception e)
		{
			Logger.Log("ERROR: An error occured while syncing MAC Addresses.");
			Logger.Log(e.ToString());
		}
	}
	
	private async Task<bool> CheckForNewMacAddresses()
	{
		try
		{
			Logger.Log("Checking for new MAC addresses.");
			
			string response = await SendGet("/sync/mac/lastUpdate");

			string lastNewKeysTimestamp = FileManager.ReadFile("lastKeysTimestamp", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
			
			if (response == lastNewKeysTimestamp)
				return false;

			FileManager.WriteAllText("lastKeysTimestamp", response);
			return true;
		}
		catch (Exception e)
		{
			Logger.Log("ERROR: An error occured while checking for new MAC addresses.");
			Logger.Log(e.ToString());
			return false;
		}
	}
}