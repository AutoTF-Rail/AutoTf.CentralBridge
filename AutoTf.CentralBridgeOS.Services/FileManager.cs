namespace AutoTf.CentralBridgeOS.Services;

public class FileManager
{
	private readonly string _dataDir = Path.Combine("/", "etc", "AutoTf");
	
	public FileManager()
	{
		Initialize();
	}

	// It is required that the evuName file exists, as well as the /etc/AutoTf dir
	private void Initialize()
	{
		Statics.EvuName = File.ReadAllText(Path.Combine(_dataDir, "evuName"));
	}
}