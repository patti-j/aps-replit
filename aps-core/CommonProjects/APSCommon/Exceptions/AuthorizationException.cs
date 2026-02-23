namespace PT.APSCommon;

public enum AuthorizationType { LicenseKey, UserSettings }

/// <summary>
/// Exception indicating that a request was denied due to lack of Permissions. This is
/// to make sure a request isn't ignored silently, but whenever possible we should stop
/// the request from being made in the first place (UI).
/// </summary>
public class AuthorizationException : PTValidationException
{
    public AuthorizationException(string a_action, AuthorizationType a_authType, string a_limitName, string a_limitValue)
        : base("2844", new object[] { a_action, a_authType.ToString(), a_limitName, a_limitValue }) { }

    public AuthorizationException(string a_problem, string a_limitModified, string a_newLimitValue)
        : base("7011", new object[] { a_problem, a_limitModified, a_newLimitValue }) { }

    public AuthorizationException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
        : base(a_message, a_stringParameters, a_appendHelpUrl) { }
}