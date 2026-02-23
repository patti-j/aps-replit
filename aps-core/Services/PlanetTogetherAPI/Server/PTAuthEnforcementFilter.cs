using System.Reflection;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

using PT.APIDefinitions;

namespace PT.PlanetTogetherAPI.Server;

/// <summary>
/// Startup middleware for the Web Server that ensures all API endpoints implement one of a selection of PT Authorization types.
/// </summary>
public class PTAuthEnforcementFilter : IStartupFilter
{
    /// <summary>
    /// List of all accepted authentication schemes. This should be hardcoded to ensure it is maintained internal to the Core product.
    /// </summary>
    private readonly List<string> m_ptAuthSchemes = new ()
    {
        PTSessionAuthSchemeOptions.SchemeName,
        PTServerAuthSchemeOptions.SchemeName
    };

    /// <summary>
    /// Checks for authorization before moving to the next middleware in the pipeline.
    /// </summary>
    /// <param name="next"></param>
    /// <returns></returns>
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return builder =>
        {
            AssertEndpointsAreAuthorized(builder.ApplicationServices);
            next(builder);
        };
    }

    private void AssertEndpointsAreAuthorized(IServiceProvider serviceProvider)
    {
        Assembly coreApiAssembly = typeof(PTHttpServer).Assembly;

        IEnumerable<ActionDescriptor> endpoints = serviceProvider.GetRequiredService<IActionDescriptorCollectionProvider>().ActionDescriptors.Items;
        foreach (ActionDescriptor actionDescriptor in endpoints)
        {
            // Allow endpoints defined within our PlanetTogetherAPI project's assembly to exist without validation.
            // This is done because certain existing endpoints don't implement the authorization above.
            // TODO: We may want to update them in a way that allows them to pass validation in this class, while remaining airtight for packages.
            if (actionDescriptor is ControllerActionDescriptor endpoint && endpoint.ControllerTypeInfo.Assembly == coreApiAssembly)
            {
                continue;
            }

            IEnumerable<AuthorizeAttribute> authorizeAttributes = actionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>();

            if (!authorizeAttributes.Any(aa =>
                    m_ptAuthSchemes.Contains(aa.AuthenticationSchemes)))
            {
                throw new NotImplementedException($"Endpoint {actionDescriptor.DisplayName} does not implement PT Authorization and cannot be loaded.");
            }
        }
    }
}