using AutoTf.CentralBridgeOS.TrainModels.Models;

namespace AutoTf.CentralBridgeOS.TrainModels;

public static class TrainResolver
{
	public static ITrainModel Resolve(IServiceProvider serviceProvider, string trainType)
	{
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