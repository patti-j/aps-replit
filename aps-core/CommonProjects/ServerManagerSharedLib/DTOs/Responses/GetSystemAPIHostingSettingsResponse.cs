namespace PT.ServerManagerSharedLib.DTOs.Responses
{
    public class GetSystemAPIHostingSettingsResponse
    {
        public string SystemServiceUrl { get; set; }
        public bool DiagnosticsEnabled { get; set; }
        public int Port { get; set; }
    }
}
