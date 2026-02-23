using PT.ServerManagerSharedLib.Definitions;

namespace PT.ServerManagerSharedLib.DTOs.Entities
{
    /// <summary>
    /// Used for accessing Registry entries for integrations with an ERP Database.
    /// </summary>
    public class ErpDatabase
    {
        public ErpDatabase() {}

        public ErpDatabase(ErpDatabaseSettings a_databaseSettings)
        {
            DatabaseName = a_databaseSettings.DatabaseName;
            ErpDatabaseName = a_databaseSettings.ErpDatabaseName;
            ConnectionString = a_databaseSettings.ConnectionString;
            ErpPassword = a_databaseSettings.ErpPassword;
            ErpServerName = a_databaseSettings.ErpServerName;
            ErpUserName = a_databaseSettings.ErpUserName;
            ServerName = a_databaseSettings.ServerName;
            Password = a_databaseSettings.Password;
            ConnectionType = (EConnectionTypesEntity)a_databaseSettings.ConnectionType;
            UserName = a_databaseSettings.UserName;
        }


        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ConnectionString { get; set; }
        public EConnectionTypesEntity ConnectionType { get; set; }
        public enum EConnectionTypesEntity
        {
            SQL7,
            ODBC,
            ORACLE,
            OLEDB
        }

        /// <summary>
        /// This is for logging into the ERP sytem if needed for custom SQL Exports.
        /// </summary>
        public string ErpUserName { get; set; }

        /// <summary>
        /// This is for logging into the ERP sytem if needed for custom SQL Exports.
        /// </summary>
        public string ErpPassword { get; set; }

        /// <summary>
        /// This is for logging into the ERP sytem if needed for custom SQL Exports.
        /// </summary>
        public string ErpServerName { get; set; }

        /// <summary>
        /// This is for logging into the ERP sytem if needed for custom SQL Exports.
        /// </summary>
        public string ErpDatabaseName { get; set; }
    }
}
