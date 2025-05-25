using System.Text;
using System.Text.Json;
using AutoTf.CentralBridge.Models.Interfaces;
using AutoTf.CentralBridge.Models.Static;
using AutoTf.Logging;
using Microsoft.Extensions.Logging;

namespace AutoTf.CentralBridge.Sync;

public class DataSync : Sync
{
	private readonly List<string> _collectedLogs = new List<string>();
	
	public DataSync(ILogger logger, Logger baseLogger, IFileManager fileManager, ITrainSessionService trainSessionService) : base(logger, fileManager, trainSessionService)
	{
		baseLogger.NewLog += log => _collectedLogs.Add(log);
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
			Logger.LogError(e, "Failed to sync data.");
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
		
		Logger.LogTrace($"Uploading {list.Count} videos.");
		
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
		
		Logger.LogTrace("Uploaded all videos.");
		
		foreach (string recording in list)
		{
			File.Delete(recording);
		}
	}

	private async Task UpdateStatus()
	{
		try
		{
			await SendPostContent("/sync/device/updatestatus", new StringContent(JsonSerializer.Serialize("Online"), Encoding.UTF8, "application/json"));
		}
		catch (Exception e)
		{
			Logger.LogError(e, "An error occured while updating the status.");
		}
	}
	
	// TODO: This is kind of problematic, it only uploads the logs of the current session, but not the logs from the previous day or so on because it doesn't go through old files.
	private async Task UploadLogs()
	{
		try
		{
			if (_collectedLogs.Count == 0)
			{
				return;
			}
			Logger.LogTrace("Uploading logs");
			
			List<string> tempLogStorage = [.._collectedLogs];
			_collectedLogs.Clear();
		
			string jsonBody = JsonSerializer.Serialize(tempLogStorage);

			await SendPostContent("/sync/device/logs/upload", new StringContent(jsonBody, Encoding.UTF8, "application/json"));
			
			Logger.LogTrace("Successfully uploaded logs.");
		}
		catch (Exception e)
		{
			Logger.LogError(e, "Failed to upload logs.");
		}
	}
}