namespace PT.APIDefinitions.RequestsAndResponses.Webapp;

/// <summary>
/// Basic request to the webapp, that validates against a "Planning Area" (ie instance)
/// </summary>
public class WebAppPlanningAreaRequest
{
    /// <summary>
    /// This is the InstanceId GUID for the instance, pulled from StartupVals - webapps has its own lingo
    /// </summary>
    public string PlanningAreaKey { get; set; }
}