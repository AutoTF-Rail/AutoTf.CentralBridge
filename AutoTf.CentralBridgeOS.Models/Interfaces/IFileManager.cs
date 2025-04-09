namespace AutoTf.CentralBridgeOS.Models.Interfaces;

public interface IFileManager
{
    public bool ReadFile(string fileName, out string content);
    	
    public string ReadFile(string fileName, string replacement = "");
    	
    public string[] ReadAllLines(string fileName, string replacement = "");
    
    public void WriteAllText(string fileName, string content);
    
    public void WriteAllLines(string fileName, string[] content);

	public void AppendAllLines(string fileName, string[] content);
}