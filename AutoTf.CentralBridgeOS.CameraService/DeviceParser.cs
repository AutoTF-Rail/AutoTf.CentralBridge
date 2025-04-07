using AutoTf.CentralBridgeOS.Models.CameraService;

namespace AutoTf.CentralBridgeOS.CameraService;

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
					if (!line.Contains("USB Video") && !line.Contains("HD Camera") && !currentDevice.Contains("USB Video") &&
					    !currentDevice.Contains("HD Camera"))
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
			Console.WriteLine(ex.ToString());
			// TODO: Log?
			// TODO: Maybe not return anything when this fails? So we don't have some corrupt state?
		}
		
		return devices;
	}
}