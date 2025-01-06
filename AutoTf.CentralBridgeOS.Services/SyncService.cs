using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Timers;
using AutoTf.Logging;
using Timer = System.Timers.Timer;

namespace AutoTf.CentralBridgeOS.Services;

public class SyncService
{
	private readonly Logger _logger = Statics.Logger;
	private List<string> _collectedLogs = new List<string>();
	private readonly Timer _syncTimer = new Timer(150000);
	private readonly string _rootDomain;
	
	public SyncService()
	{
		_rootDomain = $"https://{Statics.EvuName}.server.autotf.de";
		_logger.NewLog += LoggerOnNewLog;
		_logger.Log("Set server domain to: " + _rootDomain);
		
		if (NetworkConfigurator.IsInternetAvailable())
		{
			TrySync();
		}
		StartInternetListener();
	}

	private void LoggerOnNewLog(string log)
	{
		Console.WriteLine("New Log.");
		_collectedLogs.Add(log);
	}

	private void StartInternetListener()
	{
		_syncTimer.Elapsed += SyncSyncTimerElapsed;
		_syncTimer.Start();
		
		_logger.Log("Started Sync timer.");
	}

	private void SyncSyncTimerElapsed(object? sender, ElapsedEventArgs e)
	{
		_logger.Log("Checking for internet.");
		if (NetworkConfigurator.IsInternetAvailable())
		{
			_logger.Log("Got internet connection.");
			_logger.Log("Periodic sync check started.");
			TrySync();
			return;
		}
		
		_logger.Log("VERBOSE: Train has left internet connection. Could not sync.");
	}

	private async void TrySync()
	{
		await SyncMac();
		await UploadLogs();
		// TODO: send recorded data (Only if on wifi, not cellular), and maybe logs only on wifi too?
	}

	private async Task SyncMac()
	{
		try
		{
			_logger.Log("SYNC: Syncing MAC Addresses.");
			string url = _rootDomain + "/sync/macAddress";
			
			using HttpClient client = new HttpClient();
			
			string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Statics.Username}:{Statics.Password}"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
			
			HttpResponseMessage response = await client.GetAsync(url);
			response.EnsureSuccessStatusCode();

			string responseBody = await response.Content.ReadAsStringAsync();

			if (string.IsNullOrEmpty(responseBody))
			{
				_logger.Log("SYNC: Got 0 MAC Addresses from server.");
				return;
			}
			string[] result = JsonSerializer.Deserialize<string[]>(responseBody)!;

			_logger.Log($"{result.Length} MAC addresses received:");
			foreach (string item in result)
			{
				_logger.Log(item);
			}
		}
		catch (Exception e)
		{
			_logger.Log("SYNC: ERROR: An error occured while syncing MAC Addresses.");
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
		
			string url = _rootDomain + "/sync/uploadlogs";
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

	public void Dispose()
	{
		_logger.Log("Disposed sync timer.");
		_syncTimer.Dispose();
	}
}