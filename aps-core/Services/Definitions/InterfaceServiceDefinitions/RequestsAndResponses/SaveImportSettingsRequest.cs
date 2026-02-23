namespace PT.ImportDefintions.RequestsAndResponses;

public class SaveImportSettingsRequest
{
    public NewImportSettings ImportSettings { get; set; }
    public long ScenarioId { get; set; }
}