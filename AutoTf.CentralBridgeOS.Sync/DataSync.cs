using System.Text;
using System.Text.Json;
using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Models.Interfaces;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.CentralBridgeOS.Services.Network;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Sync;

public class DataSync : Sync
{
	private readonly List<string> _collectedLogs = new List<string>();
	
	public DataSync(Logger logger, IFileManager fileManager, TrainSessionService trainSessionService) : base(logger, fileManager, trainSessionService)
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
			Logger.Log("ERROR: Failed to sync data.");
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
		
		Logger.Log($"Uploading {list.Count} videos.");
		
		const int maxConcurrency = 5;
		using (SemaphoreSlim semaphore = new SemaphoreSlim(maxConcurrency))
		{
			foreach (string recording in list)
			{
				await semaphore.WaitAsync(); 
				uploadTasks.Add(SendPostVideo("/sync/device/video/upload", recording, semaphore));
			}
	    
			await Task.WhenAll(uploadTasks);
		}
		
		Logger.Log("Uploaded all videos.");
		
		foreach (string recording in list)
		{
			File.Delete(recording);
		}
	}

	private async Task UpdateStatus()
	{
		try
		{
			Logger.Log("Updating status.");

			if (!await SendPostContent("/sync/device/updatestatus",
				    new StringContent(JsonSerializer.Serialize("Online"), Encoding.UTF8, "application/json")))
				throw new Exception("Failed to update status.");
			
			Logger.Log("Successfully updated status.");
		}
		catch (Exception e)
		{
			Logger.Log("ERROR: An error occured while updating the status.");
			Logger.Log(e.ToString());
		}
	}
	
	private async Task UploadLogs()
	{
		try
		{
			if (_collectedLogs.Count == 0)
			{
				Logger.Log("No logs to upload. Skipping.");
				return;
			}
			Logger.Log("Uploading logs");
			
			List<string> tempLogStorage = [.._collectedLogs];
			_collectedLogs.Clear();
		
			string jsonBody = JsonSerializer.Serialize(tempLogStorage);
			
			if (!await SendPostContent("/sync/device/logs/upload",
				    new StringContent(jsonBody, Encoding.UTF8, "application/json")))
				throw new Exception("Failed to upload logs.");
			
			Logger.Log("Successfully uploaded logs.");
		}
		catch (Exception e)
		{
			Logger.Log("ERROR: Failed to upload logs.");
			Logger.Log(e.ToString());
		}
	}
}