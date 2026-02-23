using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace PT.PlanetTogetherAPI.Server;

/// <summary>
/// Filters hosted controls to a subset with the [RunInConfigMode] attribute.
/// </summary>
internal class ConfigModeControllerFilterConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        List<ControllerModel> controllersToRemove = new ();

        foreach (ControllerModel controller in application.Controllers)
        {
            object[] configModeAttributes = controller.ControllerType.GetCustomAttributes(typeof(UseInConfigModeAttribute), false);
            if (!configModeAttributes.Any())
            {
                controllersToRemove.Add(controller);
            }
        }

        foreach (ControllerModel controllerToRemove in controllersToRemove)
        {
            application.Controllers.Remove(controllerToRemove);
        }
    }
}

/// <summary>
/// Flag used to indicate whether a controller should be loaded as part of the slimmed-down configuration mode.
/// Generally, this should only require endpoints needed to complete initial instance creation and setup.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class UseInConfigModeAttribute : Attribute { }