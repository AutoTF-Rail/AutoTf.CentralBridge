using AutoTf.CentralBridge.Models;
using AutoTf.CentralBridge.Models.Static;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
// ReSharper disable HeuristicUnreachableCode
#pragma warning disable CS0162 // Unreachable code detected

namespace AutoTf.CentralBridge.Extensions;

public class MacAuthorizeAttribute : Attribute, IAuthorizationFilter
{
	public void OnAuthorization(AuthorizationFilterContext context)
	{
		IHeaderDictionary? headers = context.HttpContext.Request.Headers;

		if (!IsAllowedDevice(headers) && !IsLocalDevice(context.HttpContext.Connection.RemoteIpAddress.ToString()))
		{
			context.Result = new UnauthorizedResult();
		}
	}

	private bool IsLocalDevice(string address)
	{
		return address.StartsWith("192.168.0.");
	}

	private static bool IsAllowedDevice(IHeaderDictionary headers)
	{
		try
		{
			#if DEBUG
			return true;
			#endif
			if (!headers.TryGetValue("macAddr", out StringValues addr))
				// ReSharper disable once HeuristicUnreachableCode
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