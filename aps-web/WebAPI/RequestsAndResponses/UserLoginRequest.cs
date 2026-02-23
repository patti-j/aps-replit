namespace WebAPI.RequestsAndResponses
{
    public record UserLoginRequest(
        string PlanningAreaKey,
        string? Email,
        int? UserId);
}
