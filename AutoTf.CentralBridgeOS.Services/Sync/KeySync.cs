using System.Text.Json;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Services.Sync;

public class KeySync : Sync
{
	private string[] _latestList;
	
	public KeySync(Logger logger, FileManager fileManager) : base(logger, fileManager)
	{
		Statics.SyncEvent += Sync;
		_latestList = _fileManager.ReadAllLines("keys");
	}

	private async void Sync()
	{
		try
		{
			// If there are any new, it just syncs them all. It's easier this way.
			if (await CheckForNewKeys())
				await SyncKeys();
		}
		catch (Exception e)
		{
			_logger.Log("ERROR: Failed to sync keys.");
			_logger.Log("ERROR: " + e.Message);
		}
	}
	
	private async Task SyncKeys()
	{
		try
		{
			_logger.Log("SYNC: Syncing Keys Addresses.");
			List<KeyData> result = JsonSerializer.Deserialize<List<KeyData>>(await SendGetString("/sync/keys/allkeys"))!;

			string[] keys = result.Select(x => x.SerialNumber + ":" + x.Secret).ToArray();
			if (keys == _latestList)
				return;

			_latestList = keys;
			_fileManager.WriteAllLines("keys",  keys);
			_logger.Log("Finished syncing keys.");
		}
		catch (Exception e)
		{
			_logger.Log("SYNC: ERROR: An error occured while syncing keys.");
			_logger.Log(e.Message);
		}
	}
	
	private async Task<bool> CheckForNewKeys()
	{
		try
		{
			_logger.Log("SYNC: Checking for new keys.");

			string response = await SendGet("/sync/keys/lastkeysupdate");

			string lastNewKeysTimestamp = _fileManager.ReadFile("lastKeysTimestamp", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
			
			if (response == lastNewKeysTimestamp)
				return false;

			_fileManager.WriteAllText("lastKeysTimestamp", response);
			return true;
		}
		catch (Exception e)
		{
			_logger.Log("SYNC: ERROR: An error occured while checking for new keys.");
			_logger.Log(e.Message);
			return false;
		}
	}
}