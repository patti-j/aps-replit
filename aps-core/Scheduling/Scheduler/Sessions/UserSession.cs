using PT.APSCommon;

namespace PT.Scheduler.Sessions;

internal interface IUserSession
{
    public BaseId UserId { get; }
    public void SetUser(BaseId a_userId);

    public HashSet<BaseId> LoadedScenarioIds { get; }
}

public class UserSession : TransmissionSession, IUserSession
{
    protected BaseId m_userId = BaseId.NULL_ID;

    internal UserSession(string a_sessionToken) : base(a_sessionToken)
    {
        LoadedScenarioIds = new HashSet<BaseId>();
        SessionLifecycle = new SessionLifecycleTracker();
    }

    internal UserSession(string a_sessionToken, HashSet<BaseId> a_loadedScenarioIds) : base(a_sessionToken)
    {
        LoadedScenarioIds = a_loadedScenarioIds;
        SessionLifecycle = new SessionLifecycleTracker();
    }

    public BaseId UserId => m_userId;

    public void SetUser(BaseId a_userId)
    {
        m_userId = a_userId;
    }

    public SessionLifecycleTracker SessionLifecycle { get; }

    private HashSet<BaseId> m_loadedScenarioIds;
    public HashSet<BaseId> LoadedScenarioIds
    {
        get { return m_loadedScenarioIds; }
        set { m_loadedScenarioIds = value; }
    }
}