using System;
using System.Timers;
using AutoTf.Logging;
using Timer = System.Timers.Timer;

namespace AutoTf.CentralBridgeOS.Services.Sync;

public class SyncManager
{
	private readonly FileManager _fileManager;

	// ReSharper disable once NotAccessedField.Local
	private readonly KeySync _keySync;
	// ReSharper disable once NotAccessedField.Local
	private readonly MacSync _macSync;
	// ReSharper disable once NotAccessedField.Local
	private readonly DataSync _dataSync;
	
	private readonly Logger _logger = Statics.Logger;
	private readonly Timer _syncTimer = new Timer(150000);
	private DateTime _nextElapseTime;

	public static DateTime LastSynced;
	public static DateTime LastSyncTry;

	public SyncManager(FileManager fileManager, CameraService cameraService)
	{
		_fileManager = fileManager;
		
		LastSynced = DateTime.Parse(fileManager.ReadFile("lastSync", DateTime.Now.Subtract(TimeSpan.FromDays(1999)).ToString("o")));
		LastSynced = DateTime.Parse(fileManager.ReadFile("lastSyncTry", DateTime.Now.Subtract(TimeSpan.FromDays(1999)).ToString("o")));

		Statics.ShutdownEvent += Dispose;
		_keySync = new KeySync(_logger, fileManager);
		_macSync = new MacSync(_logger, fileManager);
		_dataSync = new DataSync(_logger, fileManager, cameraService);
		
		if (NetworkConfigurator.IsInternetAvailable())
		{
			TrySync();
		}
		StartInternetListener();
	}

	public DateTime NextInterval()
	{
		return _nextElapseTime;
	}
	
	private void StartInternetListener()
	{
		_nextElapseTime = DateTime.Now.AddMilliseconds(150000);
		_syncTimer.Elapsed += SyncSyncTimerElapsed;
		_syncTimer.Start();
		
		_logger.Log("Started Sync timer.");
	}

	private void SyncSyncTimerElapsed(object? sender, ElapsedEventArgs e)
	{
		_nextElapseTime = DateTime.Now.AddMilliseconds(150000);
		LastSyncTry = DateTime.Now;
		_fileManager.WriteAllText("lastSyncTry", LastSyncTry.ToString("o"));
		
		_logger.Log("Checking for internet.");
		
		_logger.Log("Periodic sync check started.");
		TrySync();
		// }
		
		// _logger.Log("VERBOSE: Train has left internet connection. Could not sync.");
	}

	private void TrySync()
	{
		LastSynced = DateTime.Now;
		if (NetworkConfigurator.IsInternetAvailable())
			_fileManager.WriteAllText("lastSync", LastSynced.ToString("o"));
		
		Statics.SyncEvent?.Invoke();
		
		// TODO: send recorded data (Only if on wifi, not cellular), and maybe logs only on wifi too?
	}
	
	public void Dispose()
	{
		_logger.Log("Disposed sync timer.");
		_syncTimer.Dispose();
	}
}