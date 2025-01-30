using AutoTf.CentralBridgeOS.Services;

namespace AutoTf.CentralBridgeOS.TrainModels.Models;

public sealed class DesiroML : DefaultModel
{
	public DesiroML(MotorManager motorManager) : base(motorManager)
	{
		Initialize();
	}
}