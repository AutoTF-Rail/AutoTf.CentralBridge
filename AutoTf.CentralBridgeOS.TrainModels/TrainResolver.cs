using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Services;
using AutoTf.CentralBridgeOS.TrainModels.Models;

namespace AutoTf.CentralBridgeOS.TrainModels;

public static class TrainResolver
{
	public static ITrainModel Resolve(IServiceProvider serviceProvider, string trainType)
	{
		// if (trainType == "Desiro HC")
		// 	return new DesiroHC(motorManager);
		// else if (trainType == "Desiro ML")
		// 	return new DesiroML(motorManager);
		// return new DefaultModel(motorManager);
		IDictionary<string, Type> trainModelMap = new Dictionary<string, Type>
		{
			{ "Desiro HC", typeof(DesiroHC) },
			{ "Desiro ML", typeof(DesiroML) }
		};
		
		if (trainModelMap.TryGetValue(trainType, out Type? type))
		{
			Statics.Logger.Log($"Starting with train model {trainType}.");
			ITrainModel? trainModel = serviceProvider.GetService(type) as ITrainModel;
			return trainModel!;
		}

		Statics.Logger.Log("Starting with fall back train model.");
		ITrainModel? train = serviceProvider.GetService(typeof(FallBackTrain)) as ITrainModel;
		return train;
	}
}