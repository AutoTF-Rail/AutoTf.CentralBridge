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
			return (serviceProvider.GetService(type) as ITrainModel)!;
		}

		return (serviceProvider.GetService(typeof(DefaultModel)) as ITrainModel)!;
	}
}