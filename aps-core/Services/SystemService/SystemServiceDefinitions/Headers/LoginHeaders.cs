namespace PT.SystemServiceDefinitions.Headers;

public class UserLoginResponse
{
    public UserLoginResponse()
    {
        UserId = 0;
        SessionToken = string.Empty;
        LoadedScenarioIds = new HashSet<long>();
    }

    public UserLoginResponse(long a_userId, string a_sessionToken, HashSet<long> a_loadedScenarioIds)
    {
        UserId = a_userId;
        SessionToken = a_sessionToken;
        LoadedScenarioIds = a_loadedScenarioIds;
    }
    public long UserId { get; set; }
    public string SessionToken { get; set; }
    public HashSet<long> LoadedScenarioIds { get; set; }
}