using Newtonsoft.Json;
using PT.APSCommon;
using System.ServiceModel;

using PT.Common.Exceptions;

namespace PT.APIDefinitions.RequestsAndResponses;

public class CopyScenarioRequest : ApsWebServiceScenarioRequest
{
    public string ScenarioName { get; set; }

    public bool CreateScenarioIfNew { get; set; }

    public bool IsBlackBoxScenario { get; set; }

    [JsonRequired]
    public double TimeoutMinutes { get; set; } = 5;
}
public class CopyScenarioRequestV2 : ApsWebServiceScenarioRequestV2
{
    public string ScenarioName { get; set; }

    public bool CreateScenarioIfNew { get; set; }

    public bool IsBlackBoxScenario { get; set; }

    [JsonRequired]
    public double TimeoutMinutes { get; set; } = 5;
}
public class CopyScenarioResponse : ApsWebServiceResponseBase
{
    public CopyScenarioResponse()
    {
        Confirmation = new ScenarioConfirmation();
    }

    public string ErrorMessage { get; set; }

    public ScenarioConfirmation Confirmation { get; set; }

    private void Validate()
    {
        Confirmation.Validate();
    }
}

public class ScenarioConfirmation
{
    [JsonRequired]
    public long ScenarioId { get; set; }

    [JsonRequired]
    public string ScenarioName { get; set; }

    [JsonRequired]
    public string ScenarioType { get; set; }

    internal void Validate()
    {
        if (ScenarioId == long.MinValue || string.IsNullOrEmpty(ScenarioName) || string.IsNullOrEmpty(ScenarioType))
        {
            throw new PTException("Invalid scenario data.");
        }
    }
}

public class GetScenariosRequest : ApsWebServiceRequestBase
{
    public string ScenarioType { get; set; }

    public double TimeoutMinutes { get; set; } = 5;
    public bool GetBlackBoxScenario { get; set; }
}
public class GetScenariosRequestV2 : ApsWebServiceRequestBaseV2
{
    public string ScenarioType { get; set; }

    public double TimeoutMinutes { get; set; } = 5;
    public bool GetBlackBoxScenario { get; set; }
}
public class GetScenariosResponse : ApsWebServiceResponseBase
{
    public GetScenariosResponse()
    {
        Confirmations = new List<ScenarioConfirmation>();
    }

    public string ErrorMessage { get; set; }

    public List<ScenarioConfirmation> Confirmations { get; set; }

    private void Validate()
    {
        foreach (ScenarioConfirmation sConf in Confirmations)
        {
            sConf.Validate();
        }
    }
}

public class DeleteScenarioRequest : ApsWebServiceScenarioRequest
{
    public string ScenarioName { get; set; }

    [JsonRequired]
    public double TimeoutMinutes { get; set; } = 5;
}
public class DeleteScenarioRequestV2 : ApsWebServiceScenarioRequestV2
{
    public string ScenarioName { get; set; }

    [JsonRequired]
    public double TimeoutMinutes { get; set; } = 5;
}
public class DeleteScenarioResponse : ApsWebServiceResponseBase
{
    public DeleteScenarioResponse()
    {
        Confirmation = new ScenarioConfirmation();
    }

    public string ErrorMessage { get; set; }

    public ScenarioConfirmation Confirmation { get; set; }

    private void Validate()
    {
        Confirmation.Validate();
    }
}
public class GetUndoIdxByTransmissionNbrRequest : ApsWebServiceScenarioRequest
{
    [JsonRequired]
    public ulong TransmissionNbr { get; set; }
}
public class GetUndoIdxByTransmissionNbrResponse : ApsWebServiceResponseBase
{
    [JsonRequired]
    public int UndoIdx { get; set; }
}
public class GetScenarioLastActionInfoResponse : ApsWebServiceResponseBase
{
    [JsonRequired]
    public string LastActionInfo { get; set; }
    [JsonRequired]
    public long LastActionTicks { get; set; }
    public bool HasLastActions { get; set; }
    public Guid LastActionTransmissionGuid { get; set; }
}
