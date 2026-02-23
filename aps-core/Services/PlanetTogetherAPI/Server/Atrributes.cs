using System.Net;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;

namespace PT.PlanetTogetherAPI.Server;

[AttributeUsage(AttributeTargets.Method)]
public class LocalHostConstraintAttribute : ActionMethodSelectorAttribute
{
    public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
    {
        return IsLocalRequest(routeContext.HttpContext);
    }

    private static bool IsLocalRequest(HttpContext context)
    {
        //Test this by going to you IP address, not localhost or ::1 or 127.0.0.1
        if (context.Connection.RemoteIpAddress.Equals(context.Connection.LocalIpAddress))
        {
            return true;
        }

        if (IPAddress.IsLoopback(context.Connection.RemoteIpAddress))
        {
            return true;
        }

        return false;
    }
}