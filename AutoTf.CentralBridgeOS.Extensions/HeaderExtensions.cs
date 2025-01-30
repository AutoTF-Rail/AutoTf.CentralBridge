using AutoTf.CentralBridgeOS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace AutoTf.CentralBridgeOS.Extensions;

public static class HeaderExtensions
{
	public static bool IsAllowedDevice(this IHeaderDictionary dict)
	{
		try
		{
			if (!dict.TryGetValue("macAddr", out StringValues addr))
				return false;
			if (addr.Count < 0)
				return false;
			
			return Statics.AllowedDevices.Contains(addr[0]!);
		}
		catch
		{
			// Ignored
		}

		return false;
	}

}