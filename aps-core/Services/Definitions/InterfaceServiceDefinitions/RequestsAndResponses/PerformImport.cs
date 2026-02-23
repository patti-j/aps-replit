namespace PT.ImportDefintions.RequestsAndResponses;

public class PerformImportRequest
{
    public bool TestOnly { get; set; }
    public string UserName { get; set; }
    public long Instigator { get; set; }
    public int ConnectionNbr { get; set; }
    public long SpecificScenarioId { get; set; }
    public int SpecificConfigId { get; set; } = -1;
    public ImportSettings? TestSettings { get; set; }
    public NewImportSettings? NewTestSettings { get; set; }
    public string TypeToRun { get; set; }
}

public class PerformImportResponse
{
    public PerformImportResult Content { get; set; }
}

public enum PerformImportResult
{
    /// <summary>
    /// The import was started.
    /// </summary>
    Started,

    /// <summary>
    /// The import couldn't be started because an import was already in process.
    /// </summary>
    Busy,

    /// <summary>
    /// The import couldn't be started because the call to the importer service failed.
    /// Possible causes include not being able to access the importer service, the importer service not running, ...
    /// </summary>
    Failed
}