using AutoTf.CentralBridgeOS;

internal class Program
{
	public static void Main(string[] args)
	{
		CentralBridge _bridge = new CentralBridge();
		_bridge.Initialize();
		Thread.Sleep(-1);
	}
}