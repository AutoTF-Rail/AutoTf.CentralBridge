using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Services.Sync;

public class KeySync : Sync
{
	private string[] _latestList;
	
	public KeySync(Logger logger, FileManager fileManager) : base(logger, fileManager)
	{
		Statics.SyncEvent += Sync;
		_latestList = FileManager.ReadAllLines("keys");
	}

	private async void Sync()
	{
		try
		{
			if (NetworkConfigurator.IsInternetAvailable())
				return;
			
			// If there are any new, it just syncs them all. It's easier this way.
			if (await CheckForNewKeys())
				await SyncKeys();
		}
		catch (Exception e)
		{
			Logger.Log("ERROR: Failed to sync keys.");
			Logger.Log("ERROR: " + e.Message);
		}
	}
	
	private async Task SyncKeys()
	{
		try
		{
			Logger.Log("SYNC: Syncing Keys Addresses.");
			List<KeyData> result = JsonSerializer.Deserialize<List<KeyData>>(await SendGetString("/sync/keys/allkeys"))!;

			string[] keys = result.Select(x => x.SerialNumber + ":" + x.Secret).ToArray();
			
			if (keys.SequenceEqual(_latestList))
				return;

			_latestList = keys;
			FileManager.WriteAllLines("keys",  keys);
			Logger.Log("Finished syncing keys.");
		}
		catch (Exception e)
		{
			Logger.Log("SYNC: ERROR: An error occured while syncing keys.");
			Logger.Log(e.Message);
		}
	}
	
	private async Task<bool> CheckForNewKeys()
	{
		try
		{
			Logger.Log("SYNC: Checking for new keys.");

			string response = await SendGet("/sync/keys/lastkeysupdate");

			string lastNewKeysTimestamp = FileManager.ReadFile("lastKeysTimestamp", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
			
			if (response == lastNewKeysTimestamp)
				return false;

			FileManager.WriteAllText("lastKeysTimestamp", response);
			return true;
		}
		catch (Exception e)
		{
			Logger.Log("SYNC: ERROR: An error occured while checking for new keys.");
			Logger.Log(e.Message);
			return false;
		}
	}
}