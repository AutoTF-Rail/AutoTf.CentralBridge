using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Services.Sync;

public class DataSync : Sync
{
	private readonly CameraService _cameraService;
	private List<string> _collectedLogs = new List<string>();
	
	public DataSync(Logger logger, FileManager fileManager, CameraService cameraService) : base(logger, fileManager)
	{
		_cameraService = cameraService;
		_logger.NewLog += log => _collectedLogs.Add(log);
		Statics.SyncEvent += Sync;
	}

	private async void Sync()
	{
		try
		{
			await UpdateStatus();
			await UploadLogs();
			await UploadVideo();
		}
		catch (Exception e)
		{
			_logger.Log("ERROR: Failed to sync mac addresses.");
			_logger.Log("ERROR: " + e.Message);
		}
	}

	private async Task UploadVideo()
	{
		_logger.Log("SYNC: Uploading videos.");
		// string[] recordings = Directory.GetFiles("recordings/");
		_cameraService.IntervalCapture();
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
			_logger.Log("SYNC: Updating status.");
		
			string url = _rootDomain + "/sync/device/updatestatus";

			using HttpClient client = new HttpClient();
			
			string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Statics.Username}:{Statics.Password}"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
			
			StringContent content = new StringContent(JsonSerializer.Serialize("Online"), Encoding.UTF8, "application/json");
			HttpResponseMessage response = await client.PostAsync(url, content);
			
			response.EnsureSuccessStatusCode();
			
			_logger.Log("SYNC: Successfully updated status.");
		}
		catch (Exception e)
		{
			_logger.Log("SYNC: ERROR: An error occured while updating the status.");
			_logger.Log(e.Message);
		}
	}
	
	private async Task UploadLogs()
	{
		try
		{
			if (_collectedLogs.Count == 0)
			{
				_logger.Log("SYNC: No logs to upload. Skipping.");
				return;
			}
			_logger.Log("SYNC: Uploading logs");
			
			List<string> tempLogStorage = new List<string>(_collectedLogs);
			_collectedLogs.Clear();
		
			string url = _rootDomain + "/sync/device/uploadlogs";
			string jsonBody = JsonSerializer.Serialize(tempLogStorage);

			using HttpClient client = new HttpClient();
			
			string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Statics.Username}:{Statics.Password}"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

			StringContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
			HttpResponseMessage response = await client.PostAsync(url, content);
			response.EnsureSuccessStatusCode();
			
			_logger.Log("SYNC: Successfully uploaded logs.");
		}
		catch (Exception e)
		{
			_logger.Log("SYNC: ERROR: Failed to upload logs.");
			_logger.Log(e.Message);
		}
	}
}