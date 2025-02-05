using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Services.Sync;

public abstract class Sync
{
	protected readonly Logger Logger;
	protected readonly string RootDomain;
	protected readonly FileManager FileManager;

	public Sync(Logger logger, FileManager fileManager)
	{
		Logger = logger;
		FileManager = fileManager;
		
		RootDomain = $"https://{Statics.EvuName}.server.autotf.de";
		Logger.Log("Set server domain to: " + RootDomain);
	}

	protected async Task<string> SendGet(string endpoint)
	{
		try
		{
			string url = RootDomain + endpoint;
			
			using HttpClient client = new HttpClient();
			
			string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Statics.Username}:{Statics.Password}"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
			
			HttpResponseMessage response = await client.GetAsync(url);
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadAsStringAsync();
		}
		catch (Exception e)
		{
			Logger.Log($"SYNC: ERROR: An error occured while sending a request to: {endpoint}.");
			Logger.Log(e.Message);
			throw;
		}
	}

	protected async Task<string[]> SendGetArray(string endpoint)
	{
		try
		{
			string url = RootDomain + endpoint;
			
			using HttpClient client = new HttpClient();
			
			string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Statics.Username}:{Statics.Password}"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
			
			HttpResponseMessage response = await client.GetAsync(url);
			response.EnsureSuccessStatusCode();

			return JsonSerializer.Deserialize<string[]>(await response.Content.ReadAsStringAsync())!;
		}
		catch (Exception e)
		{
			Logger.Log($"SYNC: ERROR: An error occured while sending a request to: {endpoint}.");
			Logger.Log(e.Message);
			throw;
		}
	}

	protected async Task<string> SendGetString(string endpoint)
	{
		try
		{
			string url = RootDomain + endpoint;
			
			using HttpClient client = new HttpClient();
			
			string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Statics.Username}:{Statics.Password}"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
			
			HttpResponseMessage response = await client.GetAsync(url);
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadAsStringAsync();
		}
		catch (Exception e)
		{
			Logger.Log($"SYNC: ERROR: An error occured while sending a request to: {endpoint}.");
			Logger.Log(e.Message);
			throw;
		}
	}

	/// <summary>
	/// Sends a post request with content to a given server endpoint with auth headers.
	/// </summary>
	/// <param name="endpoint">the endpoint (not including server url/host)</param>
	/// <param name="content">The actual content</param>
	/// <returns>A bool indicating if it was successfull.</returns>
	protected async Task<bool> SendPostContent(string endpoint, StringContent content)
	{
		try
		{
			string url = RootDomain + endpoint;

			using HttpClient client = new HttpClient();
			
			string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Statics.Username}:{Statics.Password}"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
			
			HttpResponseMessage response = await client.PostAsync(url, content);

			return response.IsSuccessStatusCode;
		}
		catch (Exception e)
		{
			Logger.Log($"SYNC: ERROR: An error occured while sending a request to: {endpoint}.");
			Logger.Log(e.Message);
			throw;
		}	
	}
}