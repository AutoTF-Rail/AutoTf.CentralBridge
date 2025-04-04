namespace AutoTf.CentralBridgeOS.Models.CameraService;

public class VideoDevice
{
	public VideoDevice(string name, string path, DeviceType type)
	{
		Name = name;
		Path = path;
		Type = type;
	}

	public string Name { get; set; }
	
	public DeviceType Type { get; set; }
	
	/// <summary>
	/// This represents only the first path
	/// </summary>
	public string Path { get; set; }
}