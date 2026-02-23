namespace PT.ServerManagerSharedLib.DTOs.Responses
{
    public class GetPreImportSettingsResponse
    {
        public string PreImportProgramPath { get; set; }
        public string PreImportProgramArgs { get; set; }
        public bool RunPreImportSQL { get; set; }
        public string PreImportSQL { get; set; }
        public string PreImportURL { get; set; }
        public int WebApiTimeoutSeconds { get; set; }
    }
}
