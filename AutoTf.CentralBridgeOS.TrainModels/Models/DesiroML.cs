using AutoTf.CentralBridgeOS.Services;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.TrainModels.Models;

public sealed class DesiroML : DefaultModel
{
	public DesiroML(MotorManager motorManager, Logger logger) : base(motorManager, logger)
	{
		Initialize();
	}
}