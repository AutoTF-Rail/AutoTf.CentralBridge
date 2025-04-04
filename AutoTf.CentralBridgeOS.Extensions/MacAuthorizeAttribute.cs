using AutoTf.CentralBridgeOS.Models;
using AutoTf.CentralBridgeOS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace AutoTf.CentralBridgeOS.Extensions;

public class MacAuthorizeAttribute : Attribute, IAuthorizationFilter
{
	public void OnAuthorization(AuthorizationFilterContext context)
	{
		IHeaderDictionary? headers = context.HttpContext.Request.Headers;

		if (!IsAllowedDevice(headers))
		{
			context.Result = new UnauthorizedResult();
		}
	}
	
	private static bool IsAllowedDevice(IHeaderDictionary headers)
	{
		try
		{
			// TODO: Idk if we should do this if DEBUG
			#if DEBUG
			return true;
			#endif
			if (!headers.TryGetValue("macAddr", out StringValues addr))
				return false;
			
			if (addr.Count <= 0)
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