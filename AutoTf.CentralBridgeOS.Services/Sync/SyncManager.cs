using System.Timers;
using AutoTf.Logging;
using Timer = System.Timers.Timer;

namespace AutoTf.CentralBridgeOS.Services.Sync;

public class SyncManager
{
	// ReSharper disable once NotAccessedField.Local
	private readonly KeySync _keySync;
	// ReSharper disable once NotAccessedField.Local
	private readonly MacSync _macSync;
	// ReSharper disable once NotAccessedField.Local
	private readonly DataSync _dataSync;
	
	private readonly Logger _logger = Statics.Logger;
	private readonly Timer _syncTimer = new Timer(150000);

	public static DateTime LastSynced = DateTime.Now;
	public static DateTime LastSyncTry = DateTime.Now;

	public SyncManager(FileManager fileManager)
	{
		Statics.ShutdownEvent += Dispose;
		_keySync = new KeySync(_logger, fileManager);
		_macSync = new MacSync(_logger, fileManager);
		_dataSync = new DataSync(_logger, fileManager);
		
		if (NetworkConfigurator.IsInternetAvailable())
		{
			TrySync();
		}
		StartInternetListener();
	}
	
	private void StartInternetListener()
	{
		_syncTimer.Elapsed += SyncSyncTimerElapsed;
		_syncTimer.Start();
		
		_logger.Log("Started Sync timer.");
	}

	private void SyncSyncTimerElapsed(object? sender, ElapsedEventArgs e)
	{
		LastSyncTry = DateTime.Now;
		_logger.Log("Checking for internet.");
		if (NetworkConfigurator.IsInternetAvailable())
		{
			_logger.Log("Got internet connection.");
			_logger.Log("Periodic sync check started.");
			TrySync();
			return;
		}
		
		_logger.Log("VERBOSE: Train has left internet connection. Could not sync.");
	}

	private void TrySync()
	{
		LastSynced = DateTime.Now;
		
		Statics.SyncEvent?.Invoke();
		
		// TODO: send recorded data (Only if on wifi, not cellular), and maybe logs only on wifi too?
	}
	
	public void Dispose()
	{
		_logger.Log("Disposed sync timer.");
		_syncTimer.Dispose();
	}
}