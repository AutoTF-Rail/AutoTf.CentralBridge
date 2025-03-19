using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Services.Sync;

public class DataSync : Sync
{
	private readonly List<string> _collectedLogs = new List<string>();
	
	public DataSync(Logger logger, FileManager fileManager, TrainSessionService trainSessionService) : base(logger, fileManager, trainSessionService)
	{
		Logger.NewLog += log => _collectedLogs.Add(log);
		Statics.SyncEvent += Sync;
	}

	private async void Sync()
	{
		try
		{
			if (NetworkConfigurator.IsInternetAvailable())
			{
				await UpdateStatus();
				await UploadLogs();
				await UploadVideos();
			}
		}
		catch (Exception e)
		{
			Logger.Log("SYNC-D: ERROR: Failed to sync data.");
			Logger.Log(e.ToString());
		}
	}

	private async Task UploadVideos()
	{
		string[] recordings = Directory.GetFiles("recordings/");
		
		List<Task> uploadTasks = new List<Task>();
		List<string> list = recordings.ToList();
		list.Sort();
		
		if (list.Count > 0)
			list.RemoveAt(list.Count - 1);

		if (list.Count == 0)
			return;
		
		Logger.Log($"SYNC-D: Uploading {list.Count} videos.");
		
		foreach (string recording in list)
		{
			uploadTasks.Add(SendPostVideo("/sync/device/uploadvideo", recording));
		}
		
		await Task.WhenAll(uploadTasks);
		
		Logger.Log("SYNC-D: Uploaded all videos.");
		
		foreach (string recording in list)
		{
			File.Delete(recording);
		}
	}

	private async Task UpdateStatus()
	{
		try
		{
			Logger.Log("SYNC-D: Updating status.");

			if (!await SendPostContent("/sync/device/updatestatus",
				    new StringContent(JsonSerializer.Serialize("Online"), Encoding.UTF8, "application/json")))
				throw new Exception("SYNC-D: Failed to update status.");
			
			Logger.Log("SYNC-D: Successfully updated status.");
		}
		catch (Exception e)
		{
			Logger.Log("SYNC-D: ERROR: An error occured while updating the status.");
			Logger.Log(e.ToString());
		}
	}
	
	private async Task UploadLogs()
	{
		try
		{
			if (_collectedLogs.Count == 0)
			{
				Logger.Log("SYNC-D: No logs to upload. Skipping.");
				return;
			}
			Logger.Log("SYNC-D: Uploading logs");
			
			List<string> tempLogStorage = new List<string>(_collectedLogs);
			_collectedLogs.Clear();
		
			string jsonBody = JsonSerializer.Serialize(tempLogStorage);
			
			if (!await SendPostContent("/sync/device/uploadlogs",
				    new StringContent(jsonBody, Encoding.UTF8, "application/json")))
				throw new Exception("SYNC-D: Failed to upload logs.");
			
			Logger.Log("SYNC-D: Successfully uploaded logs.");
		}
		catch (Exception e)
		{
			Logger.Log("SYNC-D: ERROR: Failed to upload logs.");
			Logger.Log(e.ToString());
		}
	}
}