using System.Text.Json;
using AutoTf.CentralBridge.Models.DataModels;
using AutoTf.CentralBridge.Models.Interfaces;
using AutoTf.CentralBridge.Models.Static;
using Microsoft.Extensions.Logging;

namespace AutoTf.CentralBridge.Sync;

public class KeySync : Sync
{
	private string[] _latestList;
	
	public KeySync(ILogger logger, IFileManager fileManager, ITrainSessionService trainSessionService) : base(logger, fileManager, trainSessionService)
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
			Logger.LogError(e, "Failed to sync keys.");
		}
	}
	
	private async Task SyncKeys()
	{
		try
		{
			List<KeyData> result = JsonSerializer.Deserialize<List<KeyData>>(await SendGetString("/sync/keys/all"))!;

			string[] keys = result.Select(x => x.SerialNumber + ":" + x.Secret).ToArray();
			
			if (keys.SequenceEqual(_latestList))
				return;

			_latestList = keys;
			FileManager.WriteAllLines("keys",  keys);
		}
		catch (Exception e)
		{
			Logger.LogError(e, "An error occured while syncing keys.");
		}
	}
	
	private async Task<bool> CheckForNewKeys()
	{
		try
		{
			string response = await SendGet("/sync/keys/lastupdate");

			string lastNewKeysTimestamp = FileManager.ReadFile("lastKeysTimestamp", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
			
			if (response == lastNewKeysTimestamp)
				return false;

			FileManager.WriteAllText("lastKeysTimestamp", response);
			return true;
		}
		catch (Exception e)
		{
			Logger.LogError(e, "An error occured while checking for new keys.");
			return false;
		}
	}
}