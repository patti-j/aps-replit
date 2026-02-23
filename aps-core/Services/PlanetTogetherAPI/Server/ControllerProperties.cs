namespace PT.PlanetTogetherAPI;

public static class ControllerProperties
{
    //private static string s_serviceURL;

    //public static string ServiceURL => s_serviceURL;

    private const string m_servicePreSharedKey = "xmqZsZqW7XKGVlLUkbDTC4nEgY8iw52g";

    public static string ServicePreSharedKey => m_servicePreSharedKey;

    private static bool m_apiDiagnosticsOn;

    public static bool ApiDiagnosticsOn => m_apiDiagnosticsOn;

    public static void SetControllerProperties(/*string a_serviceURL, */bool a_apiDiagnosticsOn)
    {
        //s_serviceURL = a_serviceURL;
        m_apiDiagnosticsOn = a_apiDiagnosticsOn;
    }
}