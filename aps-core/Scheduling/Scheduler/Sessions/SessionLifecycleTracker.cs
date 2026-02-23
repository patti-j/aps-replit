using PT.APSCommon.Extensions;

namespace PT.Scheduler.Sessions;

/// <summary>
/// Tracks significant steps in the session lifecycle to provide more information on logoff and diagnose any issues.
/// </summary>
public class SessionLifecycleTracker
{
    #region Logout Reasons
    /// <summary>
    /// A normal logoff occurred.
    /// </summary>
    private const string c_loggedOff = "The user was logged off normally.";

    /// <summary>
    /// The session was logged off after the session timeout was exceeded. If the login process did not complete, one of the subsequent two messages will show.
    /// </summary>
    private const string c_timedOut = "The user's session timed out due to inactivity.";

    /// <summary>
    /// The initial login process did not complete, and the scenario was not retrieved.
    /// </summary>
    private const string c_loginFailureNoScenario = "The user's login process did not complete.";

    /// <summary>
    /// The user was deleted, causing its session to be cleared.
    /// </summary>
    private const string c_userDeleted = "The user was deleted and their active session was subsequently removed.";
    #endregion

    private bool m_sessionFlaggedForDeletion;

    public void SetSessionFlaggedForDeletion()
    {
        m_sessionFlaggedForDeletion = true;
    }

    private bool m_sessionHasTimedOut;

    public void SetSessionHasTimedOut()
    {
        m_sessionHasTimedOut = true;
    }

    public string GetLogoutReason(int a_nbrOfReceiveCalls)
    {
        string reason = string.Empty;

        if (m_sessionFlaggedForDeletion)
        {
            reason = c_userDeleted;
        }
        else if (a_nbrOfReceiveCalls == 0)
        {
            reason = c_loginFailureNoScenario;
        }
        else if (m_sessionHasTimedOut)
        {
            reason = c_timedOut;
        }
        else
        {
            reason = c_loggedOff;
        }

        return reason.Localize();
    }
}