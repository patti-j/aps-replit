namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class SqlConnectionStringRequest
    {
        public string DataSource { get; set; }
        public string InitialCatalog { get; set; }
        public bool IntegratedSecurity { get; set; }
        public string UserID { get; set; }
        public bool Encrypt { get; set; }
        public string Password { get; set; }
    }
}