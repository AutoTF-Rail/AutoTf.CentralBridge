using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Sync;

public abstract class Sync
{
	protected readonly Logger Logger;
	protected internal readonly string RootDomain;
	protected readonly FileManager FileManager;
	private readonly TrainSessionService _trainSessionService;

	public Sync(Logger logger, FileManager fileManager, TrainSessionService trainSessionService)
	{
		Logger = logger;
		FileManager = fileManager;
		_trainSessionService = trainSessionService;

		RootDomain = $"https://{_trainSessionService.EvuName}.server.autotf.de";
	}

	protected async Task<string> SendGet(string endpoint)
	{
		try
		{
			string url = RootDomain + endpoint;
			
			using HttpClient client = new HttpClient();
			
			string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_trainSessionService.Username}:{_trainSessionService.Password}"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
			
			HttpResponseMessage response = await client.GetAsync(url);
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadAsStringAsync();
		}
		catch (Exception e)
		{
			Logger.Log($"SYNC: ERROR: An error occured while sending a request to: {endpoint}.");
			Logger.Log(e.ToString());
			throw;
		}
	}

	protected async Task<string[]> SendGetArray(string endpoint)
	{
		try
		{
			string url = RootDomain + endpoint;
			
			using HttpClient client = new HttpClient();
			
			string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_trainSessionService.Username}:{_trainSessionService.Password}"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
			
			HttpResponseMessage response = await client.GetAsync(url);
			response.EnsureSuccessStatusCode();

			return JsonSerializer.Deserialize<string[]>(await response.Content.ReadAsStringAsync())!;
		}
		catch (Exception e)
		{
			Logger.Log($"SYNC: ERROR: An error occured while sending a request to: {endpoint}.");
			Logger.Log(e.ToString());
			throw;
		}
	}

	protected async Task<string> SendGetString(string endpoint)
	{
		try
		{
			string url = RootDomain + endpoint;
			
			using HttpClient client = new HttpClient();
			
			string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_trainSessionService.Username}:{_trainSessionService.Password}"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
			
			HttpResponseMessage response = await client.GetAsync(url);
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadAsStringAsync();
		}
		catch (Exception e)
		{
			Logger.Log($"SYNC: ERROR: An error occured while sending a request to: {endpoint}.");
			Logger.Log(e.ToString());
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
			
			string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_trainSessionService.Username}:{_trainSessionService.Password}"));
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
	
	protected async Task<bool> SendPostVideo(string endpoint, string path)
	{
		try
		{
			using (HttpClient httpClient = new HttpClient())
			{
				string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_trainSessionService.Username}:{_trainSessionService.Password}"));
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

				using (MultipartFormDataContent form = new MultipartFormDataContent())
				{
					ByteArrayContent fileContent = new ByteArrayContent(File.ReadAllBytes(path));
					fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("video/mp4");
					form.Add(fileContent, "file", Path.GetFileName(path));

					HttpResponseMessage response = await httpClient.PostAsync(RootDomain + endpoint, form);

					return response.IsSuccessStatusCode;
				}
			}
		}
		catch (Exception e)
		{
			Logger.Log($"SYNC: ERROR: An error occured while sending a request to: {endpoint}.");
			Logger.Log(e.Message);
			throw;
		}	
	}
}