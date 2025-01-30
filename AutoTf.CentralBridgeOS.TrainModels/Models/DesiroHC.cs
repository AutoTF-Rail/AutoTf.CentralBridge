using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.Logging;

namespace AutoTf.CentralBridgeOS.TrainModels.Models;

public sealed class DesiroHC : DefaultModel
{
	public DesiroHC(MotorManager motorManager, Logger logger) : base(motorManager, logger)
	{
		Initialize();
	}

	public override void Initialize()
	{
		Levers.Add(0, new LeverModel()
		{
			Type = CentralBridgeOS.Models.LeverType.CombinedThrottle,
			MaximumAngle = 45, // -90 from middle
			MiddleAngle = 135,
			MinimumAngle = 225 // +90 from middle
		});
		Levers.Add(1, new LeverModel()
		{
			Type = CentralBridgeOS.Models.LeverType.MainBrake,
			MaximumAngle = 90, // -45 from middle
			MinimumAngle = 180 // +45 from middle
		});
	}
}