using PT.APSCommon;
using PT.Common.Http.Json;
using PT.ImportDefintions;
using PT.ImportDefintions.RequestsAndResponses;
using PT.Scheduler.PackageDefs;
using PT.Transmissions;

namespace PT.Scheduler;

public interface IImportingService
{
    public PerformImportResult RunImport(PerformImportRequest a_request);

    /// <summary>
    /// Gets table data for the provided SQL command text.
    /// </summary>
    /// <param name="a_commandText">Sql command text to run against the database.</param>
    /// <param name="a_includeValidation">Whether to add validation rules set on the properties. Defaults to false (for backward compatibility)</param>
    /// <param name="a_tableName">The table this query is being run against. Used for validation and not otherwise needed; defaults to null. Assumes a single table is being queried from.</param>
    /// <returns></returns>
    public DataTableJson GetBrowseTable(string a_commandText, bool a_includeValidation = false, string a_tableName = null, NewImportSettings a_importSettings = null);
    public ImportSettings GetImportSettings();
    public void SaveImportSettings(ImportSettings a_settings);
    public NewImportSettings GetNewImportSettings(long a_scenarioId);
    public int SaveNewImportSettings(long a_scenarioId, NewImportSettings a_settings);
    public ImportStatusMessage GetCurrentImportStatus();
}