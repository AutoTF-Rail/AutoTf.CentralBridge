using System.Diagnostics;
using AutoTf.Logging;

namespace AutoTf.CentralBridge.Models.Static;

public static class Statics
{
	public static readonly Logger Logger = new Logger(true);
	
	public static bool AreCamerasStarted { get; set; }
	
	public static Action? SyncEvent;
	
	// A list of all devices (tablets) MAC addresses that have logged in and are allowed to use the API.
	// This list should be checked against the given "macAddr" header before every request.
	// Just because a device is on the network, doesn't mean it's an authorized user.
	public static List<string> AllowedDevices { get; set; } = new List<string>();
	
	public static string GenerateRandomString()
	{
		Random random = new Random();
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		return new string(Enumerable.Repeat(chars, 10)
			.Select(s => s[random.Next(s.Length)]).ToArray());
	}
	
	public static string GetGitVersion()
	{
		try
		{
			ProcessStartInfo psi = new ProcessStartInfo
			{
				FileName = "git",
				Arguments = "describe --tags --always",
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true,
			};

			using (Process process = new Process { StartInfo = psi })
			{
				process.Start();
				string output = process.StandardOutput.ReadToEnd().Trim();
				string error = process.StandardError.ReadToEnd().Trim();

				process.WaitForExit();

				if (!string.IsNullOrWhiteSpace(error))
				{
					throw new Exception($"Git Error: {error}");
				}

				return output;
			}
		}
		catch (Exception ex)
		{
			return $"Error retrieving Git version: {ex.Message}";
		}
	}
}