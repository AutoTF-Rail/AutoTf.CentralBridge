using AutoTf.CentralBridgeOS.Models.CameraService;

namespace AutoTf.CentralBridgeOS.CameraService;

internal static class CommandParser
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
					if (currentDevice != string.Empty)
					{
						currentPaths.Sort();
						devices.Add(new VideoDevice(currentDevice, currentPaths.First(), currentDevice.Contains("Camera") ? DeviceType.Display : DeviceType.Camera));
					}
					
					currentDevice = line.Split(':')[0].Trim();
				}

				if (currentDevice == string.Empty)
					continue;
				
				if(!currentDevice.Contains("USB Video") && !currentDevice.Contains("HD Camera"))
					continue;

				if (!line.Contains("/dev/video"))
					continue;
			
				string devicePath = line.Trim();
			
				currentPaths.Add(devicePath);
			}
		}
		catch
		{
			// TODO: Log?
			// TODO: Maybe not return anything when this fails? So we don't have some corrupt state?
		}
		return devices;
	}
}