using System.Text.Json;
using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Models.Interfaces;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.CentralBridgeOS.Services.Network;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Sync;

public class KeySync : Sync
{
	private string[] _latestList;
	
	public KeySync(Logger logger, IFileManager fileManager, TrainSessionService trainSessionService) : base(logger, fileManager, trainSessionService)
	{
		Statics.SyncEvent += Sync;
		_latestList = FileManager.ReadAllLines("keys");
	}

	private async void Sync()
	{
		try
		{
			if (!NetworkConfigurator.IsInternetAvailable())
				return;
			
			// If there are any new, it just syncs them all. It's easier this way.
			if (await CheckForNewKeys())
				await SyncKeys();
		}
		catch (Exception e)
		{
			Logger.Log("ERROR: Failed to sync keys.");
			Logger.Log(e.ToString());
		}
	}
	
	private async Task SyncKeys()
	{
		try
		{
			Logger.Log("Syncing Keys Addresses.");
			List<KeyData> result = JsonSerializer.Deserialize<List<KeyData>>(await SendGetString("/sync/keys/all"))!;

			string[] keys = result.Select(x => x.SerialNumber + ":" + x.Secret).ToArray();
			
			if (keys.SequenceEqual(_latestList))
				return;

			_latestList = keys;
			FileManager.WriteAllLines("keys",  keys);
			Logger.Log("Finished syncing keys.");
		}
		catch (Exception e)
		{
			Logger.Log("ERROR: An error occured while syncing keys.");
			Logger.Log(e.ToString());
		}
	}
	
	private async Task<bool> CheckForNewKeys()
	{
		try
		{
			Logger.Log("Checking for new keys.");

			string response = await SendGet("/sync/keys/lastupdate");

			string lastNewKeysTimestamp = FileManager.ReadFile("lastKeysTimestamp", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
			
			if (response == lastNewKeysTimestamp)
				return false;

			FileManager.WriteAllText("lastKeysTimestamp", response);
			return true;
		}
		catch (Exception e)
		{
			Logger.Log("ERROR: An error occured while checking for new keys.");
			Logger.Log(e.ToString());
			return false;
		}
	}
}