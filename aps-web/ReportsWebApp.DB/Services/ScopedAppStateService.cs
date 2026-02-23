
using ReportsWebApp.DB.Models;

namespace ReportsWebApp.DB.Services;

/// <summary>
/// Holds state data that can be updated and consumed by pages in the application.
/// Unlike components, pages can't have their own parameters other than as strings in their url. This class allows storing more complex data, while keeping it out of the visible url.
/// This service is injected as Scoped - in Blazor server, that means every session has one instance, so the values set by a user in one browser won't impact anyone else.
/// </summary>
public class ScopedAppStateService
{
    /// <summary>
    /// Some pages are added in the context of a particular Server Entity being managed. This allows that 
    /// </summary>
    public CompanyServer? Server { get; set; }

    public string BackPath { get; set; }  = "servers";
}