namespace PT.SystemServiceDefinitions.Headers;

/// <summary>
/// Contains a BaseId that can be used to get a Scenario or 
/// </summary>
public class GetScenarioRequest
{
    public long ScenarioId { get; set; }
}

/// <summary>
/// Contains a Guid that can be used to get a transmission
/// </summary>
public class GetTransmissionRequest
{
    public string LastProcessedTransmissionId { get; set; }
}

public class UpdateLoadedScenarioIdsRequest
{
    public UpdateLoadedScenarioIdsRequest() {}

    public UpdateLoadedScenarioIdsRequest(long a_scenarioId, bool a_isAddId)
    {
        ScenarioId = a_scenarioId;
        IsAddId = a_isAddId;
    }
    public long ScenarioId { get; set; }
    public bool IsAddId { get; set; } 
}