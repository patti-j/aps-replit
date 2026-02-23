using Microsoft.Data.SqlClient;
using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Attributes;
using PT.Common.Exceptions;
using PT.ERPTransmissions;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;
using System.ComponentModel;
using System.Data;

namespace PT.Scheduler;

/// <summary>
/// A plant defines a set of Resources.  Many viewing and scheduling
/// options relate to whether Resources are in the same Plant, such
/// as whether Jobs are permitted to span Plants.
/// </summary>
public partial class Plant : BaseObject, ICloneable, IPTSerializable
{
    public new const int UNIQUE_ID = 330;

    #region IPTSerializable Members
    public Plant(IReader reader, BaseIdGenerator idGen)
        : base(reader)
    {
        m_restoreInfo = new RestoreInfo();

        if (reader.VersionNumber >= 12054)
        {
            reader.Read(out m_bottleneckThreshold);
            reader.Read(out m_heavyLoadThreshold);

            Departments = new DepartmentManager(reader, idGen);
            for (int i = 0; i < Departments.Count; i++)
            {
                Departments[i].Plant = this;
            }

            int warehouseCount;
            reader.Read(out warehouseCount);
            for (int i = 0; i < warehouseCount; i++)
            {
                BaseId warehouseId = new (reader);
                m_restoreInfo._warehouses.Add(warehouseId);
            }

            reader.Read(out m_stableSpanTicks);
            AllowedHelperManager = new AllowedHelperManager(reader);
            reader.Read(out m_dailyOperatingExpense);
            reader.Read(out m_investedCapital);
            reader.Read(out m_annualPercentageRate);
        }
        // Sort was removed, so all these readers just discard the value
        else if (reader.VersionNumber >= 479)
        {
            reader.Read(out m_bottleneckThreshold);
            reader.Read(out m_heavyLoadThreshold);
            reader.Read(out int sort);

            Departments = new DepartmentManager(reader, idGen);
            for (int i = 0; i < Departments.Count; i++)
            {
                Departments[i].Plant = this;
            }

            int warehouseCount;
            reader.Read(out warehouseCount);
            for (int i = 0; i < warehouseCount; i++)
            {
                BaseId warehouseId = new (reader);
                m_restoreInfo._warehouses.Add(warehouseId);
            }

            reader.Read(out m_stableSpanTicks);
            AllowedHelperManager = new AllowedHelperManager(reader);
            reader.Read(out m_dailyOperatingExpense);
            reader.Read(out m_investedCapital);
            reader.Read(out m_annualPercentageRate);
        }

        #region 412
        else if (reader.VersionNumber >= 412)
        {
            reader.Read(out m_bottleneckThreshold);
            reader.Read(out m_heavyLoadThreshold);
            reader.Read(out int sort);

            Departments = new DepartmentManager(reader, idGen);
            for (int i = 0; i < Departments.Count; i++)
            {
                Departments[i].Plant = this;
            }

            int warehouseCount;
            reader.Read(out warehouseCount);
            for (int i = 0; i < warehouseCount; i++)
            {
                BaseId warehouseId = new (reader);
                m_restoreInfo._warehouses.Add(warehouseId);
            }

            reader.Read(out m_stableSpanTicks);
            AllowedHelperManager = new AllowedHelperManager(reader);
            reader.Read(out m_dailyOperatingExpense);
            reader.Read(out m_investedCapital);
        }
        #endregion

        #region Version 347
        else if (reader.VersionNumber >= 347)
        {
            reader.Read(out m_bottleneckThreshold);
            reader.Read(out m_heavyLoadThreshold);
            reader.Read(out int sort);

            Departments = new DepartmentManager(reader, idGen);
            for (int i = 0; i < Departments.Count; i++)
            {
                Departments[i].Plant = this;
            }

            int warehouseCount;
            reader.Read(out warehouseCount);
            for (int i = 0; i < warehouseCount; i++)
            {
                BaseId warehouseId = new (reader);
                m_restoreInfo._warehouses.Add(warehouseId);
            }

            reader.Read(out m_stableSpanTicks);
            AllowedHelperManager = new AllowedHelperManager(reader);
        }
        #endregion

        #region 246
        else if (reader.VersionNumber >= 246)
        {
            reader.Read(out m_bottleneckThreshold);
            reader.Read(out m_heavyLoadThreshold);
            reader.Read(out int sort);

            Departments = new DepartmentManager(reader, idGen);
            for (int i = 0; i < Departments.Count; i++)
            {
                Departments[i].Plant = this;
            }

            int warehouseCount;
            reader.Read(out warehouseCount);
            for (int i = 0; i < warehouseCount; i++)
            {
                BaseId warehouseId = new (reader);
                m_restoreInfo._warehouses.Add(warehouseId);
            }

            reader.Read(out m_stableSpanTicks);
        }
        #endregion

        #region 188
        else if (reader.VersionNumber >= 188)
        {
            reader.Read(out m_bottleneckThreshold);
            reader.Read(out m_heavyLoadThreshold);
            reader.Read(out int sort);

            Departments = new DepartmentManager(reader, idGen);
            for (int i = 0; i < Departments.Count; i++)
            {
                Departments[i].Plant = this;
            }

            int warehouseCount;
            reader.Read(out warehouseCount);
            for (int i = 0; i < warehouseCount; i++)
            {
                BaseId warehouseId = new (reader);
                m_restoreInfo._warehouses.Add(warehouseId);
            }

            StableSpanTicks = SerializationHelper.ReadTimeSpanAsLong(reader);
        }
        #endregion

        #region Version 168
        else if (reader.VersionNumber >= 168)
        {
            reader.Read(out m_bottleneckThreshold);
            reader.Read(out m_heavyLoadThreshold);

            Departments = new DepartmentManager(reader, idGen);
            for (int i = 0; i < Departments.Count; i++)
            {
                Departments[i].Plant = this;
            }

            int warehouseCount;
            reader.Read(out warehouseCount);
            for (int i = 0; i < warehouseCount; i++)
            {
                BaseId warehouseId = new (reader);
                m_restoreInfo._warehouses.Add(warehouseId);
            }

            StableSpanTicks = SerializationHelper.ReadTimeSpanAsLong(reader);
        }
        #endregion

        #region 86
        else if (reader.VersionNumber >= 86)
        {
            reader.Read(out m_bottleneckThreshold);
            reader.Read(out m_heavyLoadThreshold);

            Departments = new DepartmentManager(reader, idGen);
            for (int i = 0; i < Departments.Count; i++)
            {
                Departments[i].Plant = this;
            }

            int warehouseCount;
            reader.Read(out warehouseCount);
            for (int i = 0; i < warehouseCount; i++)
            {
                BaseId warehouseId = new (reader);
                m_restoreInfo._warehouses.Add(warehouseId);
            }
        }
        #endregion

        #region 1
        else if (reader.VersionNumber >= 1)
        {
            reader.Read(out m_bottleneckThreshold);
            reader.Read(out m_heavyLoadThreshold);

            Departments = new DepartmentManager(reader, idGen);
            for (int i = 0; i < Departments.Count; i++)
            {
                Departments[i].Plant = this;
            }
        }
        #endregion
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_bottleneckThreshold);
        writer.Write(m_heavyLoadThreshold);

        Departments.Serialize(writer);

        writer.Write(m_warehouses.Count);
        for (int i = 0; i < m_warehouses.Count; i++)
        {
            m_warehouses[i].Id.Serialize(writer); //don't serialize the Warehouse, just its id.
        }

        writer.Write(StableSpanTicks);
        AllowedHelperManager.Serialize(writer);
        writer.Write(m_dailyOperatingExpense);
        writer.Write(m_investedCapital);
        writer.Write(m_annualPercentageRate);
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    private class RestoreInfo
    {
        internal readonly List<BaseId> _warehouses = new ();
    }

    private RestoreInfo m_restoreInfo;

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        a_udfManager.RestoreReferences(this, UserField.EUDFObjectType.Plants);
        Departments.RestoreReferences(a_udfManager);
    }

    internal void RestoreReferences(Scenario scenario, ISystemLogger a_errorReporter)
    {
        Departments.RestoreReferences(scenario, a_errorReporter);
        AllowedHelperManager.RestoreReferences(this);
    }

    internal void RestoreReferences(CustomerManager a_cm, CapabilityManager capabilities, CapacityIntervalManager cis, RecurringCapacityIntervalManager rcis, DispatcherDefinitionManager dispatcherDefs, PlantManager plantManager, WarehouseManager warehouseManager, ItemManager itemManager, ScenarioDetail sd)
    {
        Departments.RestoreReferences(a_cm, capabilities, cis, rcis, dispatcherDefs, plantManager, itemManager, sd);

        for (int i = 0; i < m_restoreInfo._warehouses.Count; ++i)
        {
            BaseId id = m_restoreInfo._warehouses[i];
            Warehouse warehouse = warehouseManager.GetById(id);
            AddWarehouseAssociation(warehouse, false);
        }

        m_restoreInfo = null;
    }
    #endregion

    #region Declarations
    //Property names for DataTables.
    public const string DEPARTMENT_COUNT = "DepartmentCount";

    public class PlantException : PTException
    {
        public PlantException(string message)
            : base(message) { }
    }
    #endregion

    #region Construction
    internal Plant(BaseId id, ScenarioDetail aScenarioDetail)
        : base(id)
    {
        Init(id, aScenarioDetail);
    }

    /// <summary>
    /// Sets the field values for an ERP transmission.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="wcn"></param>
    internal Plant(BaseId id, PlantT.Plant plant, ScenarioDetail aScenarioDetail, PTTransmission t, UserFieldDefinitionManager a_udfManager, UserField.EUDFObjectType a_typeUdf)
        : base(id)
    {
        Init(id, aScenarioDetail);
        Update(plant, t, a_udfManager, a_typeUdf);
    }

    private BaseId Init(BaseId id, ScenarioDetail aScenarioDetail)
    {
        m_departments = new DepartmentManager(aScenarioDetail, aScenarioDetail.IdGen);
        return id;
    }
    #endregion

    #region Shared Properties
    private decimal m_bottleneckThreshold = 10;

    /// <summary>
    /// If more than this percentage of Activities on a Resource's schedule are Capacity Bottlenecked then the Resource is flagged as a bottleneck.
    /// </summary>
    public decimal BottleneckThreshold
    {
        get => m_bottleneckThreshold;
        private set => m_bottleneckThreshold = value;
    }

    private decimal m_heavyLoadThreshold = 80;

    /// <summary>
    /// If a Resource has more than this percentage of its capacity allocated in the short-term then it is considered 'Heavy Loaded' and is flagged as such.
    /// </summary>
    [Browsable(false)] //JMC Not ready yet
    public decimal HeavyLoadThreshold
    {
        get => m_heavyLoadThreshold;
        private set => m_heavyLoadThreshold = value;
    }

    private long m_stableSpanTicks;
    [DoNotAuditProperty]
    internal long StableSpanTicks
    {
        get => m_stableSpanTicks;
        private set => m_stableSpanTicks = value;
    }

    /// <summary>
    /// This time span begins at the end of the FrozenSpan.  It signifies the period of the schedule, in addition to the FrozenSpan, that should be changed as little as possible in order to provide stability
    /// to the users of the schedule.  It serves as a visual guideline and global Optimizations can be set to start outside of this period.
    /// </summary>
    public TimeSpan StableSpan
    {
        get => new (StableSpanTicks);
        private set => StableSpanTicks = value.Ticks;
    }

    private decimal m_annualPercentageRate = 10;

    /// <summary>
    /// APR for calculating carrying cost.
    /// </summary>
    public decimal AnnualPercentageRate
    {
        get => m_annualPercentageRate;
        set
        {
            if (value > 100 || value < 0)
            {
                throw new PTHandleableException("APR should be a number between 0 and 100.".Localize());
            }

            m_annualPercentageRate = value;
        }
    }

    /// <summary>
    /// APR / (100 * 365)
    /// </summary>
    public decimal DailyInterestRate => AnnualPercentageRate / 36500;

    private decimal m_dailyOperatingExpense;

    /// <summary>
    /// Monetary costs of operating the plant for a day
    /// (Direct and Indirect Labor, SG&A except Commissions, etc.)
    /// Note that Resource hourly costs are also added into the Operating Expense value in the KPIs so they should either be included here or in the Resource costs but not both.
    /// </summary>
    public decimal DailyOperatingExpense
    {
        get => m_dailyOperatingExpense;
        internal set => m_dailyOperatingExpense = value;
    }

    private decimal m_investedCapital;

    /// <summary>
    /// Monetary value encapsulating all moneys invested in the plant.
    /// </summary>
    public decimal InvestedCapital
    {
        get => m_investedCapital;
        internal set => m_investedCapital = value;
    }
    #endregion Shared Properties

    #region Overrides
    //Override ExternalId so it can be made editable for Plant so that the user can set it to match their 
    //   system ids even if they create the Plant in PT manually.
    /// <summary>
    /// Identifier for external system references.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.AllowEdit)]
    public override string ExternalId
    {
        get => base.ExternalId;
        internal set => base.ExternalId = value;
    }

    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    [Browsable(false)]
    public override string DefaultNamePrefix => "Plant";
    #endregion

    #region Object Accessors
    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private DepartmentManager m_departments;

    [Browsable(false)]
    [DoNotAuditProperty]
    public DepartmentManager Departments
    {
        get => m_departments;
        private set => m_departments = value;
    }

    [ListSource(ListSourceAttribute.ListSources.Department, true)]
    /// <summary>
    /// The Departments that are in this Plant.
    /// </summary>
    public int DepartmentCount => m_departments.Count;

    public int GetVisibleResourceCount()
    {
        int total = 0;
        for (int deptI = 0; deptI < Departments.Count; deptI++)
        {
            foreach (Resource res in Departments[deptI].Resources)
            {
                if (!res.ExcludeFromGantts)
                {
                    total++;
                }
            }
        }

        return total;
    }
    #endregion

    #region Transmission functionality
    public void Receive(ScenarioDetail a_sd, DepartmentBaseT a_t, IScenarioDataChanges a_dataChanges)
    {
        Departments.Receive(this, a_t, a_dataChanges);
    }

    internal void ClearResourcePerformances(DateTimeOffset transmissionDT)
    {
        for (int i = 0; i < Departments.Count; i++)
        {
            Departments[i].ClearResourcePerformances(transmissionDT);
        }
    }

    public int GetAverageResourceRunPerformance()
    {
        decimal sumOfPerformances = 0;
        decimal deptsIncluded = 0;
        for (int i = 0; i < Departments.Count; i++)
        {
            Department department = Departments[i]; //all departments are included even if there's no activity or resources.                
            sumOfPerformances += department.GetAverageResourceRunPerformance();
            deptsIncluded++;
        }

        if (deptsIncluded > 0)
        {
            return (int)Math.Round(sumOfPerformances / deptsIncluded * 100, MidpointRounding.AwayFromZero);
        }

        return 100;
    }

    /// <summary>
    /// Returns a list of dates indicating which days at least one Active Resource has online time.
    /// </summary>
    /// <param name="aSd"></param>
    /// <returns></returns>
    public Dictionary<DateTime, DateTime> GetOperatingDays(ScenarioDetail aSd)
    {
        Dictionary<DateTime, DateTime> days = new ();
        for (int d = 0; d < Departments.Count; d++)
        {
            Department dept = Departments[d];
            for (int r = 0; r < dept.Resources.Count; r++)
            {
                Resource resource = dept.Resources[r];
                if (resource.Active)
                {
                    Dictionary<DateTime, DateTime> resourceDays = resource.GetOperatingDays(aSd);
                    foreach (KeyValuePair<DateTime, DateTime> dt in resourceDays)
                    {
                        if (!days.ContainsKey(dt.Value))
                        {
                            days.Add(dt.Value, dt.Value);
                        }
                    }
                }
            }
        }

        return days;
    }

    public decimal GetOperatingCostForOnlineDays(ScenarioDetail aSd)
    {
        return DailyOperatingExpense * GetOperatingDays(aSd).Count;
    }

    internal void Receive(ScenarioDetailClearT t,ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        if (t.ClearAllowedHelpers)
        {
            ResetAllowedHelperManager();
        }

        Departments.Receive(t, a_sd, a_dataChanges);
    }
    #endregion

    #region ERP Transmissions
    internal void Update(UserFieldDefinitionManager a_udfManager, PlantT.Plant p, PTTransmission t)
    {
        Update(p, t, a_udfManager, UserField.EUDFObjectType.Plants);

        if (p.BottleneckThresholdSet)
        {
            BottleneckThreshold = p.BottleneckThreshold;
        }

        if (p.HeavyLoadThresholdSet)
        {
            HeavyLoadThreshold = p.HeavyLoadThreshold;
        }

        if (p.StableSpanSet)
        {
            StableSpan = p.StableSpan;
        }

        if (p.DailyOperatingExpenseSet)
        {
            DailyOperatingExpense = p.DailyOperatingExpense;
        }

        if (p.InvestedCapitalSet)
        {
            InvestedCapital = p.InvestedCapital;
        }

        if (p.AnnualPercentageRateSet)
        {
            AnnualPercentageRate = p.AnnualPercentageRate;
        }
    }

    internal void Update(PlantEdit a_plantEdit)
    {
        Edit(a_plantEdit);

        if (a_plantEdit.BottleneckThresholdSet)
        {
            BottleneckThreshold = a_plantEdit.BottleneckThreshold;
        }

        if (a_plantEdit.HeavyLoadThresholdSet)
        {
            HeavyLoadThreshold = a_plantEdit.HeavyLoadThreshold;
        }

        if (a_plantEdit.StableSpanSet)
        {
            StableSpan = a_plantEdit.StableSpan;
        }

        if (a_plantEdit.DailyOperatingExpenseSet)
        {
            DailyOperatingExpense = a_plantEdit.DailyOperatingExpense;
        }

        if (a_plantEdit.InvestedCapitalSet)
        {
            InvestedCapital = a_plantEdit.InvestedCapital;
        }

        if (a_plantEdit.AnnualPercentageRateSet)
        {
            AnnualPercentageRate = a_plantEdit.AnnualPercentageRate;
        }
    }

    /// <summary>
    /// Call when deleting a Plant to give it a chance to delete Departments.
    /// </summary>
    internal void Deleting(IScenarioDataChanges a_dataChanges)
    {
        Departments.Deleting(a_dataChanges);
    }
    #endregion

    #region Cloning
    public Plant Clone()
    {
        return (Plant)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }

    //TODO: Finish deepcopy
    //public Plant DeepCopy(ScenarioDetail a_sd, BaseIdGenerator a_idGen)
    //{
    //    Plant newPlant = new Plant(a_idGen.NextID());
    //    newPlant.Name = Name;
    //    newPlant.ExternalId = MakeExternalId(newPlant.Id.Value);
    //    newPlant.m_bottleneckThreshold = m_bottleneckThreshold;
    //    newPlant.m_heavyLoadThreshold = m_heavyLoadThreshold;
    //    newPlant.m_sort = m_sort;
    //    newPlant.m_stableSpanTicks = m_stableSpanTicks;
    //    newPlant.m_dailyOperatingExpense = m_dailyOperatingExpense;
    //    newPlant.m_investedCapital = m_investedCapital;
    //    newPlant.m_annualPercentageRate = m_annualPercentageRate;
    //    if (UserFields != null)
    //    {
    //        newPlant.UserFields = new UserFieldList(UserFields.GetUserFieldImportString());
    //    }
    //    newPlant.Departments = Departments.DeepCopy(a_sd, a_idGen, newPlant);

    //    //TODO: Consider trying to copy this
    //    newPlant.AllowedHelperManager = new AllowedHelperManager();

    //    return newPlant;
    //}
    #endregion

    #region Department Associations
    public class DepartmentAssociationException : PTException
    {
        public DepartmentAssociationException(string m)
            : base(m) { }
    }

    internal void AddDepartmentAssociation(Department dept)
    {
        if (dept == null)
        {
            throw new DepartmentAssociationException("2814");
        }

        if (!m_departments.Contains(dept.Id))
        {
            m_departments.Add(dept);
        }
    }

    internal void RemoveDepartmentAssociation(Department dept)
    {
        if (dept == null)
        {
            throw new DepartmentAssociationException("2814");
        }

        if (m_departments.Contains(dept.Id))
        {
            m_departments.Remove(dept);
        }
    }
    #endregion Department Associations

    #region PT Database
    public static void SetPtDbCommand(SqlDataAdapter da, SqlConnection conn)
    {
        //INSERT Command
        string sql = "INSERT Plants(PlantId, Name,Description,BottleneckThreshold,HeavyLoadThreshold) " +
                     "VALUES (@PlantID, @Name,@Description,@BottleneckThreshold,@HeavyLoadThreshold);"; // +
        //				"SET @PlantID=Scope_Identity(); " +
        //				"SELECT @PlantID PlantID;";
        da.InsertCommand = new SqlCommand(sql, conn);

        //INSERT Command Parameters
        SqlParameterCollection cparams = da.InsertCommand.Parameters;
        SqlParameter plantId = cparams.Add("@PlantID", SqlDbType.Int, 0, "PlantID");
        //			plantId.Direction=ParameterDirection.Output;
        cparams.Add("@Name", SqlDbType.NVarChar, 30, "Name");
        cparams.Add("@Description", SqlDbType.NVarChar, 255, "Description");
        cparams.Add("@BottleneckThreshold", SqlDbType.Float, 0, "BottleneckThreshold");
        cparams.Add("@HeavyLoadThreshold", SqlDbType.Float, 0, "HeavyLoadThreshold");
    }
    #endregion PT Database

    #region Warehouses
    private readonly WarehouseArrayList m_warehouses = new ();

    /// <summary>
    /// This list of Warehouses associated with the Plant.
    /// </summary>
    [Browsable(false)]
    private WarehouseArrayList Warehouses => m_warehouses;

    public int WarehouseCount => Warehouses.Count;

    public Warehouse GetWarehouseAtIndex(int index)
    {
        return Warehouses[index];
    }

    public bool ContainsWarehouse(Warehouse warehouse)
    {
        return Warehouses.Contains(warehouse);
    }
    [DoNotAuditProperty]
    public IEnumerable<Warehouse> WarehouseEnumerator
    {
        get
        {
            foreach (Warehouse warehouse in m_warehouses)
            {
                yield return warehouse;
            }
        }
    }

    /// <summary>
    /// Add a Warehouse that this Plant can access for addition and retrieval of material.
    /// </summary>
    /// <param name="warehouse"></param>
    /// <param name="a_performLicenseCheck">don't throw exception during deserialization. System is set to read-only from a different place if this data violates license limits.</param>
    internal void AddWarehouseAssociation(Warehouse warehouse, bool a_performLicenseCheck = true)
    {
        if (a_performLicenseCheck && !PTSystem.LicenseKey.IncludeCrossWarehousePlanning && m_warehouses.Count > 0) // can't use multiple warehouses due to license
        {
            throw new AuthorizationException("Add Plant Warehouse Association".Localize(), AuthorizationType.LicenseKey, "IncludeCrossWarehousePlanning", PTSystem.LicenseKey.IncludeCrossWarehousePlanning.ToString());
        }

        m_warehouses.Add(warehouse);
    }

    /// <summary>
    /// Remove the specified Warehouse form the Plant's list of available Warehouses.
    /// </summary>
    /// <param name="warehouse"></param>
    internal void RemoveWarehouseAssociation(Warehouse warehouse)
    {
        m_warehouses.Remove(warehouse);
    }

    /// <summary>
    /// Remove this Warehouse if the Plant currently has it in the list.
    /// </summary>
    /// <param name="warehouse"></param>
    internal void WarehouseDeleted(Warehouse warehouse)
    {
        if (m_warehouses.Contains(warehouse))
        {
            m_warehouses.Remove(warehouse);
        }
    }

    public decimal GetATPFromAllWarehouses(Item item, DateTimeOffset atpDate, DateTime a_clockDate)
    {
        decimal atp = 0;
        for (int i = 0; i < WarehouseCount; i++)
        {
            Warehouse w = GetWarehouseAtIndex(i);
            Inventory inv = w.Inventories[item.Id];
            if (inv != null)
            {
                atp += inv.GetAtpQty(atpDate.ToServerTime(), a_clockDate);
            }
        }

        return atp;
    }

    public decimal GetOnHandInventoryFromAllWarehouses(Item item)
    {
        decimal onHandQty = 0;
        for (int i = 0; i < WarehouseCount; i++)
        {
            Warehouse w = GetWarehouseAtIndex(i);
            Inventory inv = w.Inventories[item.Id];
            if (inv != null)
            {
                onHandQty += inv.OnHandQty;
            }
        }

        return onHandQty;
    }
    #endregion

    public override string ToString()
    {
        return string.Format("PlantName '{0}'; PlantExtId={1}; PlantId={2}", Name, ExternalId, Id);
    }

    public void PopulateImportDataSet(PtImportDataSet.PlantsDataTable table)
    {
            table.AddPlantsRow(
                ExternalId,
                Name,
                Description,
                Notes,
                StableSpan.TotalHours,
                BottleneckThreshold,
                HeavyLoadThreshold,
                DailyOperatingExpense,
                InvestedCapital,
                //TODO: Update this
                "",//this.UserFields == null ? "" : this.UserFields.GetUserFieldImportString(),
                AnnualPercentageRate);
    }
        
    AllowedHelperManager m_allowedHelperManager = new AllowedHelperManager();

    internal void ResetAllowedHelperManager()
    {
        m_allowedHelperManager = new AllowedHelperManager();
    }

    public AllowedHelperManager AllowedHelperManager
    {
        get => m_allowedHelperManager;
        private set => m_allowedHelperManager = value;
    }

    internal void HandleDeletedResources(Dictionary<BaseId, Resource> a_resDictionary)
    {
        AllowedHelperManager.HandleDeletedResources(a_resDictionary);
    }

    public DateTime GetEarliestFrozenSpanEnd(DateTime a_clock)
    {
        return new DateTime(GetEarliestFrozenSpanEnd(a_clock.Ticks));
    }

    /// <summary>
    /// Returns the earliest Department frozen span end
    /// </summary>
    /// <param name="a_clock"></param>
    /// <returns></returns>
    public long GetEarliestFrozenSpanEnd(long a_clock)
    {
        long earliestFrozenSpanEnd = PTDateTime.MaxDateTicks;

        foreach (Department department in Departments)
        {
            earliestFrozenSpanEnd = Math.Min(a_clock + department.FrozenSpanTicks, earliestFrozenSpanEnd);
        }

        if (earliestFrozenSpanEnd == PTDateTime.MaxDateTicks)
        {
            //No departments had a frozen span.
            return a_clock;
        }

        return earliestFrozenSpanEnd;
    }
    [DoNotAuditProperty]
    public IEnumerable<Resource> ActiveResourceEnumerator
    {
        get
        {
            foreach (Department department in Departments)
            {
                foreach (Resource res in department.Resources)
                {
                    if (res.Active)
                    {
                        yield return res;
                    }
                }
            }
        }
    }
}