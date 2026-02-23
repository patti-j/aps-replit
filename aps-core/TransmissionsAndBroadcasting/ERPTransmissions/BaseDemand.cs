using PT.Transmissions;

namespace PT.ERPTransmissions;

/// <summary>
/// Base Class for Foreast and Sales Order.
/// </summary>
public class BaseDemand : PTObjectBase, IPTSerializable
{
    public new const int UNIQUE_ID = 605;

    #region PT Serialization
    public BaseDemand(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            bools = new BoolVector32(reader);
            reader.Read(out itemExternalId);
            reader.Read(out warehouseExternalId);
            reader.Read(out shipToZipCode);
            reader.Read(out shipToCountry);
            reader.Read(out transitWorkingDays);
            reader.Read(out priority);
            reader.Read(out latePenaltyCostPerDay);
            reader.Read(out salesEmployee);
            reader.Read(out project);
            reader.Read(out planner);
            int val;
            reader.Read(out val);
            lateTreatment = (lateTreatments)val;
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        bools.Serialize(writer);
        writer.Write(itemExternalId);
        writer.Write(warehouseExternalId);
        writer.Write(shipToZipCode);
        writer.Write(shipToCountry);
        writer.Write(transitWorkingDays);
        writer.Write(priority);
        writer.Write(latePenaltyCostPerDay);
        writer.Write(salesEmployee);
        writer.Write(project);
        writer.Write(planner);
        writer.Write((int)lateTreatment);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion PT Serialization

    #region Bools
    private BoolVector32 bools;
    private const int ShipToZipCodeSetIdx = 0;
    private const int ShipToCountrySetIdx = 1;
    private const int TransitWorkingDaysSetIdx = 2;
    private const int PrioritySetIdx = 3;
    private const int LatePenaltyCostPerDaySetIdx = 4;
    private const int SalesEmployeeSetIdx = 5;
    private const int ProjectSetIdx = 6;
    private const int LateTreatmentSetIdx = 7;
    private const int PlannerSetIdx = 8;
    #endregion

    private string itemExternalId;

    /// <summary>
    /// The Item that the Forecast is for.
    /// </summary>
    public string ItemExternalId
    {
        get => itemExternalId;
        set => itemExternalId = value;
    }

    private string warehouseExternalId;

    /// <summary>
    /// The Warehouse from which the Forecast will be satisfied.
    /// </summary>
    public string WarehouseExternalId
    {
        get => warehouseExternalId;
        set => warehouseExternalId = value;
    }

    #region Shared Properties
    private string shipToZipCode;

    /// <summary>
    /// The zip code the demand will ship to.  This can be used to calculate transit time.
    /// </summary>
    public string ShipToZipCode
    {
        get => shipToZipCode;
        set
        {
            shipToZipCode = value;
            bools[ShipToZipCodeSetIdx] = true;
        }
    }

    public bool ShipToZipCodeSet => bools[ShipToZipCodeSetIdx];

    private string shipToCountry;

    /// <summary>
    /// The country to which the shipment is destined.  This can be used to calculate transit time.
    /// </summary>
    public string ShipToCountry
    {
        get => shipToCountry;
        set
        {
            shipToCountry = value;
            bools[ShipToCountrySetIdx] = true;
        }
    }

    public bool ShipToCountrySet => bools[ShipToCountrySetIdx];

    private TimeSpan transitWorkingDays;

    public TimeSpan TransitWorkingDays
    {
        get => transitWorkingDays;
        set
        {
            transitWorkingDays = value;
            bools[TransitWorkingDaysSetIdx] = true;
        }
    }

    public bool TransitWorkingDaysSet => bools[TransitWorkingDaysSetIdx];

    private int priority;

    /// <summary>
    /// Inventory is allocated to Demand based upon Priority and Required Ship Date.
    /// When comparing Demands a difference of 1 in Priorty is considered equivalent to a difference of one week in Required Ship Date.
    /// Priority 1 is the most important.
    /// </summary>
    public int Priority
    {
        get => priority;
        set
        {
            priority = value;
            bools[PrioritySetIdx] = true;
        }
    }

    public bool PrioritySet => bools[PrioritySetIdx];

    private decimal latePenaltyCostPerDay;

    /// <summary>
    /// Used to calculate schedule profitability.
    /// </summary>
    public decimal LatePenaltyCostPerDay
    {
        get => latePenaltyCostPerDay;
        set
        {
            latePenaltyCostPerDay = value;
            bools[LatePenaltyCostPerDaySetIdx] = true;
        }
    }

    public bool LatePenaltyCostPerDaySet => bools[LatePenaltyCostPerDaySetIdx];

    private string salesEmployee;

    /// <summary>
    /// The employee in sales who is responsible for this demand.
    /// </summary>
    public string SalesEmployee
    {
        get => salesEmployee;
        set
        {
            salesEmployee = value;
            bools[SalesEmployeeSetIdx] = true;
        }
    }

    public bool SalesEmployeeSet => bools[SalesEmployeeSetIdx];

    private string project;

    /// <summary>
    /// Can be used for tracking multiple demands tied to one project.
    /// </summary>
    public string Project
    {
        get => project;
        set
        {
            project = value;
            bools[ProjectSetIdx] = true;
        }
    }

    public bool ProjectSet => bools[ProjectSetIdx];

    public enum lateTreatments
    {
        /// <summary>
        /// The customer will accept the product late.
        /// </summary>
        ShipLate,

        /// <summary>
        /// Assume production will adjust to make up the shortfall and meet the demand.
        /// </summary>
        DriveInventoryNegative,

        /// <summary>
        /// The customer will not accept late shipment so consider this sale a missed opportunity.
        /// </summary>
        LostSale
    }

    private lateTreatments lateTreatment;

    /// <summary>
    /// Specifies how the system should treat the demand if there is inadequate inventory to satisfy it.
    /// </summary>
    public lateTreatments LateTreatment
    {
        get => lateTreatment;
        set
        {
            lateTreatment = value;
            bools[LateTreatmentSetIdx] = true;
        }
    }

    public bool LateTreatmentSet => bools[LateTreatmentSetIdx];

    private string planner;

    /// <summary>
    /// The User responsible for planning this demand.
    /// </summary>
    public string Planner
    {
        get => planner;
        set
        {
            planner = value;
            bools[PlannerSetIdx] = true;
        }
    }

    public bool PlannerSet => bools[PlannerSetIdx];
    #endregion Shared Properties
}