using AutoTf.CentralBridgeOS.Models;

namespace AutoTf.CentralBridgeOS.TrainModels;

public interface ITrainModel
{
	public int LeverCount();
	public void SetLever(int index, double percentage);
	public LeverType GetLeverType(int index);
	public double? GetLeverPercentage(int index);
	void Initialize();
}