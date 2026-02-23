namespace PT.ServerManagerSharedLib.DTOs.Entities
{
    /// <summary>
    /// Provides default ERP settings values based on the integration selected at instance creation
    /// (from the integration.json file in that integration directory).
    /// This is used to provide default values for new instances on creation, after which they
    /// are managed manually and stored in the instance's <see cref="ErpDataBaseSettings"/>
    /// and <see cref="InterfaceServiceSettings"/>.
    /// </summary>
    public class Integration
    {
        public Integration() { }

        public string ConnectionString { get; set; } = "";

        public string DatabaseName { get; set; } = "";

        public string ERPDatabaseName { get; set; } = "";

        public string PreImportSQL { get; set; } = "";

        public string SQLServerConnectionString { get; set; } = "";

        public bool RunPreImportSQL { get; set; } = false;

        public string UserName { get; set; } = "";

        public string ERPServerName { get; set; } = "";

        public string ERPUserName { get; set; } = "";

        public string ERPPassword { get; set; } = "";
    }
}
