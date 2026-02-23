namespace PT.ServerManagerSharedLib.DTOs.Responses
{
    public class SqlConnectionBuilderResponse
    {
        public string DataSource { get; set; }
        public string InitialCatalog { get; set; }
        public bool IntegratedSecurity { get; set; }
        public string UserID { get; set; }
        public bool Encrypt { get; set; }
        public string Password { get; set; }
    }
}
