namespace PT.APIDefinitions.RequestsAndResponses.Webapp;

public class UserRequest : WebAppPlanningAreaRequest
{
    /// <summary>
    /// The email corresponding to the user to check for. As in the instance, this uniquely identifies a user.
    /// </summary>
    public string? Email { get; set; }
    public int? UserId { get; set; }
}