using PT.APSCommon;
using PT.Common.Attributes;
using PT.Common.Exceptions;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;
using System.ComponentModel;

namespace PT.Scheduler;

/// <summary>
/// Shows which ERP Work Center the Resource belongs too.   Can be used for creating GanttViews or in scheduling algorithms.
/// </summary>
public partial class Department : BaseObject, ICloneable, IPlantChild, IPTSerializable
{
    public new const int UNIQUE_ID = 329;

    #region IPTSerializable Members
    public Department(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12054)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_frozenSpan);

            Resources = new ResourceManager(a_reader, a_idGen);
            for (int i = 0; i < Resources.Count; i++)
            {
                Resources[i].Department = this;
            }
        }
        else if (a_reader.VersionNumber >= 445)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_frozenSpan);
            a_reader.Read(out int sort);

            Resources = new ResourceManager(a_reader, a_idGen);
            for (int i = 0; i < Resources.Count; i++)
            {
                Resources[i].Department = this;
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_bools.Serialize(writer);
        writer.Write(m_frozenSpan);

        Resources.Serialize(writer);
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    internal void RestoreReferences(CustomerManager a_cm, CapabilityManager capabilities, CapacityIntervalManager cis, RecurringCapacityIntervalManager rcis, DispatcherDefinitionManager dispatcherDefs, PlantManager plantManager, ItemManager itemManager, ScenarioDetail sd)
    {
        m_resources.RestoreReferences(a_cm, capabilities, cis, rcis, dispatcherDefs, plantManager, itemManager, sd);
    }

    internal void RestoreReferences(Scenario a_scenario, ScenarioDetail a_sd, ISystemLogger a_errorReporter)
    {
        m_resources.RestoreReferences(a_scenario, a_sd, a_errorReporter);
    }

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        a_udfManager.RestoreReferences(this, UserField.EUDFObjectType.Departments);
        m_resources.RestoreReferences(a_udfManager);
    }
    #endregion

    #region Declarations
    //Property names for DataTables.
    internal class DepartmentException : PTException
    {
        public DepartmentException(string message)
            : base(message) { }
    }

    public const string PLANT = "Plant";
    #endregion

    #region Construction
    internal Department(BaseId id, Plant plant, ScenarioDetail aScenarioDetail)
        : base(id)
    {
        Plant = plant;
        m_resources = new ResourceManager(aScenarioDetail, aScenarioDetail.IdGen);
    }

    /// <summary>
    /// Sets the field values for an ERP transmission.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="wcn"></param>
    /// <param name="plant"></param>
    /// <param name="aScenarioDetail"></param>
    /// <param name="a_udfManager"></param>
    internal Department(BaseId id, DepartmentT.Department wcn, Plant plant, ScenarioDetail aScenarioDetail, UserFieldDefinitionManager a_udfManager)
        : base(id, wcn, a_udfManager, UserField.EUDFObjectType.Departments)
    {
        // These lines are unique to this object type.
        this.plant = plant;
        m_resources = new ResourceManager(aScenarioDetail, aScenarioDetail.IdGen);
        m_frozenSpan = wcn.FrozenSpanDuration;
    }
    #endregion

    #region Overrides
    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    [Browsable(false)]
    public override string DefaultNamePrefix => "Department";

    //Override ExternalId so it can be made editable so that the user can set it to match their 
    //   system ids even if they create the object in PT manually.
    /// <summary>
    /// Identifier for external system references.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.AllowEdit)]
    public override string ExternalId
    {
        get => base.ExternalId;
        internal set => base.ExternalId = value;
    }
    #endregion

    #region Transmission functionality
    internal void Receive(BaseResourceT t, IScenarioDataChanges a_dataChanges)
    {
        if (t is ResourceBaseT)
        {
            Resources.Receive(this, (ResourceBaseT)t, a_dataChanges);
        }
        else
        {
            throw new TransmissionValidationException(t, "2170", new object[] { t.GetType().ToString() });
        }
    }

    internal void Update(ScenarioDetail a_sd, UserFieldDefinitionManager a_udfManager, DepartmentT.Department a_department, PTTransmission a_t)
    {
        base.Update(a_department, a_t, a_udfManager, UserField.EUDFObjectType.Departments);

        if (a_department.FrozenSpanSet)
        {
            FrozenSpanTicks = a_department.FrozenSpanDuration;
        }
    }

    internal void Update(DepartmentEdit a_deptEdit)
    {
        Edit(a_deptEdit);

        if (a_deptEdit.FrozenSpanSet)
        {
            FrozenSpanTicks = a_deptEdit.FrozenSpan.Ticks;
        }
    }

    internal void Receive(ScenarioDetailClearT t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    { 
        if (t.ClearResources)
        {
            for (int i = Resources.Count - 1; i >= 0; i--)
            {
                Resource resource = Resources[i];
                Resources.Delete(resource, a_dataChanges);
            }
        }
        else if (t.ClearResourcesWithoutCapabilities)
        {
            for (int i = Resources.Count - 1; i >= 0; i--)
            {
                Resource res = Resources[i];
                if (res.CapabilityCount == 0)
                {
                    Resources.Delete(res, a_dataChanges);
                }
            }
        }
        //Clear Tank inventories if we are removing items or inventories
        else if (t.ClearItems || t.ClearInventories)
        {
            for (int i = Resources.Count - 1; i >= 0; i--)
            {
                Resource resource = Resources[i];
                if(resource.IsTank)
                {
                    resource.ClearAllTankInventories(t, a_sd, a_dataChanges);
                }
            }
        }
    }

    //		void DeleteAllResources(DepartmentDeleteT t,Scenario scenario, ScenarioDetail scenarioDetail, GanttViewerLayout gvl)
    //		{
    //			ResourceDeleteAllT deleteAllT=new ResourceDeleteAllT(t.scenarioId,this.Plant.Id,this.Id);
    //			deleteAllT.ConnectionNbr=t.ConnectionNbr;
    //			deleteAllT.instigator=t.instigator;
    //			this.Resources.Receive(scenario,gvl,this,deleteAllT,scenarioDetail);
    //		}
    internal void Deleting(IScenarioDataChanges a_dataChanges)
    {
        Resources.Deleting(this, a_dataChanges);
    }

    internal void ClearResourcePerformances(DateTimeOffset transmissionDT)
    {
        for (int i = 0; i < Resources.Count; i++)
        {
            Resources[i].ClearPerformance(transmissionDT);
        }
    }

    public int GetAverageResourceRunPerformance()
    {
        decimal sumOfPerformances = 0;
        decimal resourcesIncluded = 0;
        for (int i = 0; i < Resources.Count; i++)
        {
            Resource resource = Resources[i];
            if (resource.RunsDone > 0)
            {
                sumOfPerformances += resource.RunPerformance;
                resourcesIncluded++;
            }
        }

        if (resourcesIncluded > 0)
        {
            return (int)Math.Round(sumOfPerformances / resourcesIncluded * 100, MidpointRounding.AwayFromZero);
        }

        return 100;
    }
    #endregion

    #region Cloning
    public Department Clone(BaseId newId, ScenarioDetail aScenarioDetail)
    {
        Department newDept = new (newId, plant, aScenarioDetail);
        return newDept;
    }

    //TODO: finish deep copy
    //public Department Clone(BaseIdGenerator a_idGen, ScenarioDetail a_sd, Plant a_plant)
    //{
    //    Department newDept = new Department(a_idGen.NextID(), a_plant, a_sd);
    //    newDept.Name = Name;
    //    newDept.sort = sort;
    //    newDept.m_bools = m_bools;
    //    newDept.m_frozenSpan = m_frozenSpan;
    //    newDept.Resources = Resources.DeepCopy(a_sd, a_idGen, newDept);
    //    return newDept;
    //}

    object ICloneable.Clone()
    {
        return new object();
    }
    #endregion

    #region Booleans
    private BoolVector32 m_bools;
    #endregion

    #region Object Accessors
    private Plant plant;

    [Browsable(false)]
    [DoNotAuditProperty]
    public Plant Plant
    {
        get => plant;
        set => plant = value;
    }

    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private ResourceManager m_resources;

    [Browsable(false)]
    [DoNotAuditProperty]
    public ResourceManager Resources
    {
        get => m_resources;
        set => m_resources = value;
    }

    [ListSource(ListSourceAttribute.ListSources.Resource, true)]
    /// <summary>
    /// The Resources that are in this Department.
    /// </summary>
    public int ResourceCount => m_resources.Count;

    private long m_frozenSpan = TimeSpan.FromHours(8).Ticks;
    [DoNotAuditProperty]
    internal long FrozenSpanTicks
    {
        get => m_frozenSpan;
        private set => m_frozenSpan = value;
    }

    public TimeSpan FrozenSpan => new (m_frozenSpan);
    #endregion

    #region Resource Associations
    internal class ResourceAssociationException : PTException
    {
        public ResourceAssociationException(string m)
            : base(m) { }
    }

    internal void AddMachineAssociationWithNewId(Resource r, IScenarioDataChanges a_dataChanges)
    {
        Resources.AddResWithNewId(r, a_dataChanges);
    }

    internal void AddMachineAssociation(Resource m, IScenarioDataChanges a_dataChanges)
    {
        if (m == null)
        {
            throw new ResourceAssociationException("2813");
        }

        if (!m_resources.Contains(m.Id))
        {
            m_resources.AddRes(m, a_dataChanges);
        }
    }

    internal void RemoveMachineAssociation(Resource m, IScenarioDataChanges a_dataChanges, bool a_removeCapabilities = true, bool a_removeCapacityIntervals = true, bool a_removeRecurringCapacityIntervals = true, bool a_removeAllowedHelpers = true)
    {
        if (m == null)
        {
            throw new ResourceAssociationException("2814");
        }

        if (m_resources.Contains(m.Id))
        {
            m_resources.Delete(m, a_dataChanges, a_removeCapabilities, a_removeCapacityIntervals, a_removeRecurringCapacityIntervals, a_removeAllowedHelpers);
        }
    }
    #endregion Resource Associations

    #region Find
    public Resource GetResource(BaseId a_resId)
    {
        return m_resources.GetById(a_resId);
    }
    #endregion

    #region IPlantChild
    [Browsable(true)]
    //		[ListSource(ListSourceAttribute.ListSources.Plant,false)]
    /// <summary>
    /// The Plant this Department is in.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public string PlantName => plant.Name;

    public new const string PLANT_ID = "PlantId"; //THIS MUST MATCH FIELD BELOW

    [Browsable(false)]
    [PartOfKey(true)]
    /// <summary>
    /// The Plant this Department is in.
    /// </summary>
    public BaseId PlantId => plant.Id;
    #endregion

    /// <summary>
    /// For backwards compatibility only.
    /// </summary>
    /// <param name="a_scenarioOptionsLegacyFrozenSpan">The old system frozen span value</param>
    public void SetDepartmentFrozenSpanForLegacyScenarios(TimeSpan a_scenarioOptionsLegacyFrozenSpan)
    {
        if (!m_bools[0]) //Deserialized an old version where UseDepartmentFrozenSpan was false
        {
            m_frozenSpan = a_scenarioOptionsLegacyFrozenSpan.Ticks;
        }
    }
}