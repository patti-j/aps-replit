using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.PlanetTogetherAPI;

/// <summary>
/// Provides default error handling for all API actions. Any unhandled exceptions will be processed here.
/// If an exception is fine to be handled as below (our conventional default), no try/catch structures are needed in the endpoint itself.
/// </summary>
public class PTHttpExceptionHandlingFilter : IActionFilter, IOrderedFilter
{
    // Run as the last filter in the pipeline. This value can be reduced to allow for other filters to succeed it if need be.
    public int Order => int.MaxValue;

    public const string c_apiLogSubpath = "Alerts/APIErrors";

    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception != null)
        {
            string controller = context.ActionDescriptor.RouteValues["controller"];
            string action = context.ActionDescriptor.RouteValues["action"];

            string routeIdentifier = $"{controller}/{action}";
            ApiLogger al = new (routeIdentifier);
            al.LogGenericException(context.Exception);

            if (context.Exception is AuthorizationException authEx)
            {
                RespondToAuthorizationException(context, routeIdentifier, authEx);
            }
            else
            {
                RespondToGenericException(context, routeIdentifier);
            }

            context.ExceptionHandled = true;
        }
    }

    private static void RespondToGenericException(ActionExecutedContext context, string routeIdentifier)
    {
        string responseMessage = string.Format("An unexpected error occurred on route {0}. Details have been logged to the server.".Localize(), routeIdentifier);
        context.Result = new ObjectResult(responseMessage)
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
    }

    private static void RespondToAuthorizationException(ActionExecutedContext context, string routeIdentifier, AuthorizationException authEx)
    {
        string responseMessage = string.Format("An action was cancelled on route {0} due to insufficient permissions: {1}".Localize(), routeIdentifier, authEx.Message);

        context.Result = new ObjectResult(responseMessage)
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }
}