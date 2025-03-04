using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Services.Sync;

public class DataSync : Sync
{
	private readonly CameraService _cameraService;
	private readonly List<string> _collectedLogs = new List<string>();
	
	public DataSync(Logger logger, FileManager fileManager, CameraService cameraService) : base(logger, fileManager)
	{
		_cameraService = cameraService;
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
				// TODO: Enable when implemented
				// await UploadVideo();
			}
		}
		catch (Exception e)
		{
			Logger.Log("SYNC-D: ERROR: Failed to sync data.");
			Logger.Log(e.ToString());
		}
	}

	private async Task UploadVideo()
	{
		Logger.Log("SYNC-D: Uploading videos.");
		// string[] recordings = Directory.GetFiles("recordings/");
		// TODO: Upload
		// Upload all previous collected recordings
		
		// Delete files
		// foreach (string recording in recordings)
		// {
		// 	File.Delete(recording);
		// }
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