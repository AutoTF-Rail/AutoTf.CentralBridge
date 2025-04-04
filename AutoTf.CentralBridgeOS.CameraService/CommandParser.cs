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


			string currentDevice = string.Empty;
			foreach (string line in lines)
			{
				if (line.Contains(':'))
					currentDevice = line.Split(':')[0].Trim();

				if (currentDevice == string.Empty)
					continue;
				
				if(!currentDevice.Contains("USB Video") && !currentDevice.Contains("HD Camera"))
					continue;

				if (!line.Contains("/dev/video"))
					continue;
			
				string devicePath = line.Trim();
			
				// In theory we could just add the devicePath to a list of paths in the class VideoDevice, then we would have all paths, but I don't think we need that.
				devices.Add(new VideoDevice(currentDevice, devicePath, currentDevice == "HD Camera" ? DeviceType.Camera : DeviceType.Display));
				currentDevice = string.Empty;
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