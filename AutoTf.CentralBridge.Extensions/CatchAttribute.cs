using AutoTf.CentralBridge.Models.Static;
using AutoTf.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AutoTf.CentralBridge.Extensions;

public class CatchAttribute : ExceptionFilterAttribute
{
    private readonly Logger _logger = Statics.Logger;

    public override void OnException(ExceptionContext context)
    {
        _logger.Log("An exception occured in a catched controller.");
        _logger.Log(context.Exception.ToString());
        
        context.Result = new BadRequestObjectResult(new { error = context.Exception.Message });
        context.ExceptionHandled = true;
    }
}