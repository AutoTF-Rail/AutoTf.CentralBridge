using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Services.Sync;

public abstract class Sync
{
	protected readonly Logger _logger;
	protected readonly string _rootDomain;
	protected readonly FileManager _fileManager;

	public Sync(Logger logger, FileManager fileManager)
	{
		_logger = logger;
		_fileManager = fileManager;
		
		_rootDomain = $"https://{Statics.EvuName}.server.autotf.de";
		_logger.Log("Set server domain to: " + _rootDomain);
	}

	protected async Task<string> SendGet(string endpoint)
	{
		try
		{
			string url = _rootDomain + endpoint;
			
			using HttpClient client = new HttpClient();
			
			string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Statics.Username}:{Statics.Password}"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
			
			HttpResponseMessage response = await client.GetAsync(url);
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadAsStringAsync();
		}
		catch (Exception e)
		{
			_logger.Log($"SYNC: ERROR: An error occured while sending a request to: {endpoint}.");
			_logger.Log(e.Message);
			throw;
		}
	}

	protected async Task<string[]> SendGetArray(string endpoint)
	{
		try
		{
			string url = _rootDomain + endpoint;
			
			using HttpClient client = new HttpClient();
			
			string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Statics.Username}:{Statics.Password}"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
			
			HttpResponseMessage response = await client.GetAsync(url);
			response.EnsureSuccessStatusCode();

			return JsonSerializer.Deserialize<string[]>(await response.Content.ReadAsStringAsync())!;
		}
		catch (Exception e)
		{
			_logger.Log($"SYNC: ERROR: An error occured while sending a request to: {endpoint}.");
			_logger.Log(e.Message);
			throw;
		}
	}

	protected async Task<string> SendGetString(string endpoint)
	{
		try
		{
			string url = _rootDomain + endpoint;
			
			using HttpClient client = new HttpClient();
			
			string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Statics.Username}:{Statics.Password}"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
			
			HttpResponseMessage response = await client.GetAsync(url);
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadAsStringAsync();
		}
		catch (Exception e)
		{
			_logger.Log($"SYNC: ERROR: An error occured while sending a request to: {endpoint}.");
			_logger.Log(e.Message);
			throw;
		}
	}
}