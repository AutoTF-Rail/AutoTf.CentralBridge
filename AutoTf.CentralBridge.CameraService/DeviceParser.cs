using AutoTf.CentralBridge.Models;
using AutoTf.CentralBridge.Models.CameraService;
using AutoTf.CentralBridge.Models.Static;

namespace AutoTf.CentralBridge.CameraService;

internal static class DeviceParser
{
	internal static List<VideoDevice> ParseVideoDevices(string output)
	{
		List<VideoDevice> devices = new List<VideoDevice>();
		try
		{
			string[] lines = output.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

			List<string> currentPaths = new List<string>();
			string currentDevice = string.Empty;
			
			foreach (string line in lines)
			{
				if (line.Contains(':'))
				{
					if (!line.Contains("USB Video") && !line.Contains("USB Camera") && !currentDevice.Contains("USB Video") &&
					    !currentDevice.Contains("USB Camera"))
					{
						continue;
					}

					if (!string.IsNullOrEmpty(currentDevice) && currentPaths.Count != 0)
					{
						currentPaths.Sort();
						devices.Add(new VideoDevice(currentDevice, currentPaths.First(), currentDevice.Contains("Camera") ? DeviceType.Display : DeviceType.Camera));
						currentPaths.Clear();
					}
					
					currentDevice = line.Split(':')[0].Trim();
					continue;
				}

				if (currentDevice == string.Empty || !line.Contains("/dev/video"))
				{
					continue;
				}

				currentPaths.Add(line.Trim());
			}
			
			// Adding the last one in row too
			if (currentDevice != string.Empty && currentPaths.Count != 0)
			{
				currentPaths.Sort();
				devices.Add(new VideoDevice(currentDevice, currentPaths.First(), currentDevice.Contains("Camera") ? DeviceType.Display : DeviceType.Camera));
			}
		}
		catch (Exception ex)
		{
			// When it fails right here, we will just not have any devices, thus the other services just can't start, there is no need to handle this seperatly.
			Statics.Logger.Log("Could not resolve video devices:");
			Statics.Logger.Log(ex.ToString());
			return new List<VideoDevice>();
		}
		
		return devices;
	}
}