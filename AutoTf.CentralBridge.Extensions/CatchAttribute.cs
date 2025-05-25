using AutoTf.CentralBridge.Models.Static;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace AutoTf.CentralBridge.Extensions;

public class CatchAttribute : ExceptionFilterAttribute
{
    private readonly ILogger _logger = Statics.Logger;

    public override void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "An exception occured in a catched controller.");

        context.Result = new BadRequestObjectResult(new { error = context.Exception.Message });
        context.ExceptionHandled = true;
    }
}