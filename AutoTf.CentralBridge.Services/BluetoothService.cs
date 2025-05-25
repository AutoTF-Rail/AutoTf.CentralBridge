using AutoTf.CentralBridge.Models.Interfaces;
using AutoTf.CentralBridge.Models.Static;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoTf.CentralBridge.Services;

public class BluetoothService : IHostedService
{
	private readonly ITrainSessionService _trainSessionService;
	private readonly ILogger<BluetoothService> _logger;
	private int _instanceId = 2;

	public BluetoothService(ILogger<BluetoothService> logger, ITrainSessionService trainSessionService)
	{
		_logger = logger;
		_trainSessionService = trainSessionService;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		StartBeacon();
		return Task.CompletedTask;
	}

	private void StartBeacon()
	{
		try
		{
			string hexMessage = StringToHex(_trainSessionService.Ssid);
			int length = hexMessage.Length / 2 + 1;
			string lengthByte = length.ToString("X2");
			string adData = $"{lengthByte}09{hexMessage}";

			_logger.LogTrace($"Trying to start BLE with data: {adData}");
			
			string command = $"btmgmt add-adv -d {adData} {_instanceId}";

			CommandExecuter.ExecuteSilent(command, false);
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Bluetooth beacon threw an error.");
		}
	}
	
	private static string StringToHex(string input)
	{
		char[] chars = input.ToCharArray();
		string hexOutput = string.Empty;

		foreach (char c in chars)
		{
			hexOutput += ((int)c).ToString("X2");
		}

		return hexOutput;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		string command = $"btmgmt rm-adv {_instanceId}";
		CommandExecuter.ExecuteSilent(command, true);
		
		_logger.LogTrace("Beacon removed.");
		return Task.CompletedTask;
	}
}