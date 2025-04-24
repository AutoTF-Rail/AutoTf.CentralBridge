using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AutoTf.CentralBridgeOS.Models.Interfaces;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.Sync;

public abstract class Sync
{
	protected readonly Logger Logger;
	protected internal readonly string RootDomain;
	protected readonly IFileManager FileManager;
	private readonly ITrainSessionService _trainSessionService;

	public Sync(Logger logger, IFileManager fileManager, ITrainSessionService trainSessionService)
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
			Logger.Log($"ERROR: An error occured while sending a request to: {endpoint}.");
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
			Logger.Log($"ERROR: An error occured while sending a request to: {endpoint}.");
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
			Logger.Log($"ERROR: An error occured while sending a request to: {endpoint}.");
			Logger.Log(e.ToString());
			throw;
		}
	}

	/// <summary>
	/// Sends a post request with content to a given server endpoint with auth headers.
	/// </summary>
	/// <param name="endpoint">the endpoint (not including server url/host)</param>
	/// <param name="content">The actual content</param>
	protected async Task SendPostContent(string endpoint, StringContent content)
	{
		try
		{
			string url = RootDomain + endpoint;

			using HttpClient client = new HttpClient();
			
			string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_trainSessionService.Username}:{_trainSessionService.Password}"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
			
			HttpResponseMessage response = await client.PostAsync(url, content);

			response.EnsureSuccessStatusCode();
		}
		catch (Exception e)
		{
			Logger.Log($"ERROR: An error occured while sending a request to: {endpoint}.");
			Logger.Log(e.Message);
			throw;
		}	
	}
	
	protected async Task<bool> SendPostVideo(string endpoint, string path, SemaphoreSlim semaphore)
	{
		try
		{
			using (HttpClient httpClient = new HttpClient())
			{
				string authValue =
					Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_trainSessionService.Username}:{_trainSessionService.Password}"));
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
			Logger.Log($"ERROR: An error occured while sending a request to: {endpoint}.");
			Logger.Log(e.Message);
			throw;
		}
		finally
		{
			semaphore.Release();
		}
	}
}