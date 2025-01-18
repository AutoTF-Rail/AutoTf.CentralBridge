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
		Statics.Username = ReadFile("username");
		Statics.Password = ReadFile("password");
	}
	
	public bool ReadFile(string fileName, out string content)
	{
		content = "";
		string path = Path.Combine(_dataDir, fileName);
		if (!File.Exists(path))
			return false;

		content = File.ReadAllText(path);
		return true;
	}
	
	public string ReadFile(string fileName, string replacement = "")
	{
		string path = Path.Combine(_dataDir, fileName);

		if (!File.Exists(path))
		{
			File.WriteAllText(path, replacement);
			return replacement;
		}

		return File.ReadAllText(path);
	}
	
	public string[] ReadAllLines(string fileName, string replacement = "")
	{
		string path = Path.Combine(_dataDir, fileName);

		if (!File.Exists(path))
		{
			File.WriteAllText(path, replacement);
			return [replacement];
		}

		return File.ReadAllLines(path);
	}

	public void WriteAllText(string fileName, string content)
	{
		string path = Path.Combine(_dataDir, fileName);
		File.WriteAllText(path, content);
	}

	public void WriteAllLines(string fileName, string[] content)
	{
		string path = Path.Combine(_dataDir, fileName);
		File.WriteAllLines(path, content);
	}

	public void AppendAllLines(string fileName, string[] content)
	{
		string path = Path.Combine(_dataDir, fileName);
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.AppendAllLines(path, content);
	}
}