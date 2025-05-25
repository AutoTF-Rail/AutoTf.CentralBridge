using AutoTf.CentralBridge.Models.Interfaces;
using AutoTf.CentralBridge.Models.Static;
using Microsoft.Extensions.Logging;

namespace AutoTf.CentralBridge.Sync;

public class MacSync : Sync
{
	private string[] _latestList;
	
	public MacSync(ILogger logger, IFileManager fileManager, ITrainSessionService trainSessionService) : base(logger, fileManager, trainSessionService)
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
			Logger.LogError(e, "Failed to sync mac addresses.");
		}
	}
	
	private async Task SyncMac()
	{
		try
		{
			string[] result = await SendGetArray("/sync/mac/all");

			if (result.SequenceEqual(_latestList))
				return;
			
			_latestList = result;
			
			File.WriteAllLines("/etc/hostapd/accepted_macs.txt", result);
			CommandExecuter.ExecuteSilent("sudo systemctl restart hostapd", false);
		}
		catch (Exception e)
		{
			Logger.LogError(e, "An error occured while syncing MAC Addresses.");
		}
	}
	
	private async Task<bool> CheckForNewMacAddresses()
	{
		try
		{
			string response = await SendGet("/sync/mac/lastUpdate");

			string lastNewKeysTimestamp = FileManager.ReadFile("lastKeysTimestamp", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
			
			if (response == lastNewKeysTimestamp)
				return false;

			FileManager.WriteAllText("lastKeysTimestamp", response);
			return true;
		}
		catch (Exception e)
		{
			Logger.LogError(e, "An error occured while checking for new MAC addresses.");
			return false;
		}
	}
}