using Newtonsoft.Json;

using PT.Common.Exceptions;

namespace PT.APIDefinitions.RequestsAndResponses;

public class CtpRequest : ApsWebServiceScenarioRequest
{
    public long ScenarioId { get; set; }

    #region WebaApp-Only fields
    /// <summary>
    /// Identifier for the request in the Webapp's system. Used to keep track when communicating back and forth with that system.
    /// </summary>
    public int? RequestId { get; set; }
    public string InstanceId { get; set; }
    public bool WaitForResponse { get; set; } = true;
    #endregion

    /// <summary>
    /// Each line specifies an item, qty and desired date. At least one is needed.
    /// </summary>
    [JsonRequired]
    public List<CtpRequestLine> CtpRequestLines { get; set; }

    /// <summary>
    /// Whether to reserve the material and capacity necessary for the quoted ScheduledEnd date.
    /// </summary>

    public bool ReserveMaterialsAndCapacity { get; set; }

    public string SchedulingType { get; set; }

    public int Priority { get; set; }

    [JsonRequired]
    public double TimeoutMinutes { get; set; } = 5;

    private void Validate()
    {
        if (CtpRequestLines.Count == 0)
        {
            throw new PTException("At least one CtpRequestLine is needed");
        }
    }
}

public class CtpRequestLine : ApsWebServiceScenarioRequest
{
    /// <summary>
    /// Identifies a request line. Has to be equal or greater than zero.
    /// </summary>
    [JsonRequired]
    public int LineId { get; set; }

    /// <summary>
    /// ExternalId of the item. Cannot be empty string or null.
    /// </summary>
    [JsonRequired]
    public string ItemExternalId { get; set; }

    /// <summary>
    /// ExternalId of the warehouse. Cannot be empty string or null.
    /// </summary>
    [JsonRequired]
    public string WarehouseExternalId { get; set; }

    /// <summary>
    /// Qty being requested. Has to be larger than 0
    /// </summary>
    [JsonRequired]
    public decimal RequiredQty { get; set; }

    /// <summary>
    /// Desired available date of this item at the specified RequiredQty.
    /// </summary>
    [JsonRequired]
    public DateTime NeedDate { get; set; }

    private void Validate()
    {
        if (LineId < 0)
        {
            throw new PTException("LineId has to be greater than or equal to zero.");
        }

        if (string.IsNullOrWhiteSpace(ItemExternalId))
        {
            throw new PTException("ItemExternalId cannot be empty string or null.");
        }

        if (string.IsNullOrWhiteSpace(WarehouseExternalId))
        {
            throw new PTException("WarehouseExternalId cannot be empty string or null.");
        }

        if (RequiredQty <= 0)
        {
            throw new PTException("QtyNeeded must be greater than zero.");
        }
    }
}

public class CtpResponse
{
    public CtpResponse()
    {
        CtpResponseLines = new List<CtpResponseLine>(1);
    }

    /// <summary>
    /// whether an error occured when processing the request.
    /// </summary>

    public EApsWebServicesResponseCodes ReturnCode { get; set; }

    public string ErrorMessage { get; set; }

    public List<CtpResponseLine> CtpResponseLines { get; set; }

    #region WebaApp-Only fields
    public int? RequestId { get; set; }
    public string InstanceId { get; set; }
    #endregion


    private void Validate()
    {
        if (CtpResponseLines.Count == 0)
        {
            throw new PTException("At least one CtpResponseLine is needed.");
        }
    }
}

public class CtpResponseLine
{
    /// <summary>
    /// Id specifying this line item.
    /// </summary>
    [JsonRequired]
    public int LineId { get; set; }

    /// <summary>
    /// When production for this line item will start.
    /// </summary>

    public DateTime ScheduledStartDate { get; set; }

    /// <summary>
    /// When production for this line item will end.
    /// </summary>

    public DateTime ScheduledEndDate { get; set; }

    private void Validate()
    {
        if (LineId < 0)
        {
            throw new PTException("LineId must be greater than or equal to zero.");
        }
    }
}