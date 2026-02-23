using System.Collections;
using System.ComponentModel;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of Machine objects.
/// </summary>
public partial class ResourceManager : ScenarioBaseObjectManager<Resource>, IPTSerializable
{
    #region IPTSerializable Members
    public ResourceManager(IReader reader, BaseIdGenerator a_idGen)
        : base(a_idGen)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                Resource r = new (reader, a_idGen);
                AddRes_ConstructorVersionForSerialization(r);
            }
        }
    }

    public new const int UNIQUE_ID = 332;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    internal void RestoreReferences(CustomerManager a_cm,
                                    CapabilityManager a_capabilities,
                                    CapacityIntervalManager a_cis,
                                    RecurringCapacityIntervalManager a_rcis,
                                    DispatcherDefinitionManager a_dispatcherDefs,
                                    PlantManager a_plantManager,
                                    ItemManager a_itemManager,
                                    ScenarioDetail a_scenarioDetail)
    {
        m_scenarioDetail = a_scenarioDetail;

        for (int i = 0; i < Count; i++)
        {
            Resource resource = this[i];
            resource.RestoreReferences(a_scenarioDetail);
            resource.RestoreReferences(a_cm, a_capabilities, a_cis, a_rcis, a_dispatcherDefs, a_itemManager, a_scenarioDetail, a_scenarioDetail.IdGen, m_errorReporter);
            resource.RestoreReferences(a_plantManager);
        }
    }

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        foreach (Resource resource in this)
        {
            resource.RestoreReferences(a_udfManager);
        }
    }
    #endregion

    #region Declarations
    public class MachineManagerException : PTException
    {
        public MachineManagerException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(a_message, a_stringParameters, a_appendHelpUrl) { }
    }
    #endregion

    #region Construction
    public ResourceManager(ScenarioDetail a_scenarioDetail, BaseIdGenerator a_idGen)
        : base(a_idGen)
    {
        m_scenarioDetail = a_scenarioDetail;
    }
    #endregion

    #region Machine Edit Functions
    internal Resource AddDefault(Department a_department, IScenarioDataChanges a_dataChanges)
    {
        Resource m = new (NextID(), a_department, m_scenarioDetail.DispatcherDefinitionManager.DefaultDispatcherDefinition.CreateDispatcher(), m_scenarioDetail.ShopViewResourceOptionsManager.GetByIndex(0));
        ValidateAdd(m);

        //Add a default RCI
        m.AddRecurringCapacityInterval(m_scenarioDetail.RecurringCapacityIntervalManager.GetDefaultRCI(m_scenarioDetail, a_dataChanges));
        m.RegenerateCapacityProfile(m_scenarioDetail.GetPlanningHorizonEndTicks(), true);
        m.CopyToResultantCapacity(m_scenarioDetail.ClockDate.Ticks);
        a_dataChanges.MachineChanges.AddedObject(m.Id);
        a_dataChanges.AuditEntry(new AuditEntry(m.Id, m.Department.Id, m), true);
        a_dataChanges.FlagConstraintChanges(m.Id);
        return base.Add(m);
    }

    private Resource AddCopy(ResourceCopyT a_t, IScenarioDataChanges a_dataChanges)
    {
        ValidateCopy(a_t);
        Resource m = GetById(a_t.originalId);
        BaseId newId = NextID();
        Resource mCopy = new (m, newId, m_scenarioDetail);
        a_dataChanges.FlagConstraintChanges(newId);
        Resource added = AddCopy(m, mCopy, newId);
        a_dataChanges.AuditEntry(new AuditEntry(added.Id, added.Department.Id, added), true);
        return added;
    }

    private Resource Delete(BaseId a_machineId, IScenarioDataChanges a_dataChanges)
    {
        Resource m = GetById(a_machineId);
        Delete(m, a_dataChanges);
        return m;
    }

    internal delegate void ResourceDeletedDelegate(Resource a_r);

    internal void Delete(Resource a_r, IScenarioDataChanges a_dataChanges, bool a_removeCapabilities = true, bool a_removeCapacityIntervals = true, bool a_removeRecurringCapacityIntervals = true, bool a_removeAllowedHelpers = true)
    {
        a_dataChanges.AuditEntry(new AuditEntry(a_r.Id, a_r.Department.Id, a_r), false, true);
        a_r.Deleting(m_scenarioDetail, a_removeCapabilities, a_removeCapacityIntervals, a_removeRecurringCapacityIntervals, a_removeAllowedHelpers);
        a_dataChanges.FlagConstraintChanges(BaseId.NULL_ID);
        a_dataChanges.FlagEligibilityChanges(BaseId.NULL_ID);
        Remove(a_r);
        a_dataChanges.MachineChanges.DeletedObject(a_r.Id);

        m_scenarioDetail.JobManager.ResourceDeleteNotification(a_r);
    }
    #endregion

    #region Transmissions
    private void ValidateAdd(Resource a_m)
    {
        if (GetById(a_m.Id) != null)
        {
            throw new MachineManagerException("2761", new object[] { a_m.Id.ToString() });
        }
    }

    private void ValidateCopy(ResourceCopyT a_t)
    {
        ValidateExistence(a_t.originalId);
    }

    public void Receive(Department a_w, ResourceBaseT a_t, IScenarioDataChanges a_dataChanges)
    {
        Resource m;
        if (a_t is ResourceDefaultT)
        {
            m = AddDefault(a_w, a_dataChanges);
            a_dataChanges.DepartmentChanges.UpdatedObject(m.Department.Id);
        }
        else if (a_t is ResourceCopyT)
        {
            //TODO: Extract resource copy out so Department can be cloned by copying resources. Issue is AddCopy does work that can't easily be extracted
            m = AddCopy((ResourceCopyT)a_t, a_dataChanges);
            m.RegenerateCapacityProfile(m_scenarioDetail.GetPlanningHorizonEndTicks(), true);
            m.CopyToResultantCapacity(m_scenarioDetail.ClockDate.Ticks);
            a_dataChanges.DepartmentChanges.UpdatedObject(m.Department.Id);
            for (int capI = 0; capI < m.CapabilityCount; capI++)
            {
                a_dataChanges.CapabilityChanges.UpdatedObject(m.GetCapabilityByIndex(capI).Id);
            }

            for (int ciI = 0; ciI < m.CapacityIntervals.Count; ciI++)
            {
                a_dataChanges.CapacityIntervalChanges.UpdatedObject(m.CapacityIntervals[ciI].Id);
            }

            for (int rciI = 0; rciI < m.RecurringCapacityIntervals.Count; rciI++)
            {
                a_dataChanges.RecurringCapacityIntervalChanges.UpdatedObject(m.RecurringCapacityIntervals[rciI].Id);
            }

            a_dataChanges.MachineChanges.AddedObject(m.Id);
            a_dataChanges.AuditEntry(new AuditEntry(m.Id, m.Department.Id, m), true);
        }
        else if (a_t is ResourceDeleteT resourceDeleteT)
        {
            m = ValidateExistence(resourceDeleteT.machineId);

            for (int capI = 0; capI < m.CapabilityCount; capI++)
            {
                a_dataChanges.CapabilityChanges.UpdatedObject(m.GetCapabilityByIndex(capI).Id);
            }

            for (int ciI = 0; ciI < m.CapacityIntervals.Count; ciI++)
            {
                a_dataChanges.CapacityIntervalChanges.UpdatedObject(m.CapacityIntervals[ciI].Id);
            }

            for (int rciI = 0; rciI < m.RecurringCapacityIntervals.Count; rciI++)
            {
                a_dataChanges.RecurringCapacityIntervalChanges.UpdatedObject(m.RecurringCapacityIntervals[rciI].Id);
            }

            DeleteMachine(m.Department, resourceDeleteT, a_dataChanges);
        }
        else if (a_t is ResourceDeleteMultiT resourceDeleteMultiT)
        {
            List<ResourceKey> keysToDelete = new List<ResourceKey>();
           
            foreach (ResourceKey key in resourceDeleteMultiT.ResourceKeys)
            {
                if (a_w.Id == key.Department)
                {
                    m = ValidateExistence(key.Resource);

                    for (int capI = 0; capI < m.CapabilityCount; capI++)
                    {
                        a_dataChanges.CapabilityChanges.UpdatedObject(m.GetCapabilityByIndex(capI).Id);
                    }

                    for (int ciI = 0; ciI < m.CapacityIntervals.Count; ciI++)
                    {
                        a_dataChanges.CapacityIntervalChanges.UpdatedObject(m.CapacityIntervals[ciI].Id);
                    }

                    for (int rciI = 0; rciI < m.RecurringCapacityIntervals.Count; rciI++)
                    {
                        a_dataChanges.RecurringCapacityIntervalChanges.UpdatedObject(m.RecurringCapacityIntervals[rciI].Id);
                    }
                    keysToDelete.Add(key);
                }
            }

            DeleteMachines(keysToDelete, a_dataChanges);
        }
        else if (a_t is ResourceDeleteAllT)
        {
            a_dataChanges.DepartmentChanges.UpdatedObject(a_w.Id);
            for (int i = 0; i < Count; i++)
            {
                m = this[i];
                for (int capI = 0; capI < m.CapabilityCount; capI++)
                {
                    a_dataChanges.CapabilityChanges.UpdatedObject(m.GetCapabilityByIndex(capI).Id);
                }

                for (int ciI = 0; ciI < m.CapacityIntervals.Count; ciI++)
                {
                    a_dataChanges.CapacityIntervalChanges.UpdatedObject(m.CapacityIntervals[ciI].Id);
                }

                for (int rciI = 0; rciI < m.RecurringCapacityIntervals.Count; rciI++)
                {
                    a_dataChanges.RecurringCapacityIntervalChanges.UpdatedObject(m.RecurringCapacityIntervals[rciI].Id);
                }
            }

            DeleteAllMachines(a_w, a_dataChanges);
            a_dataChanges.DepartmentChanges.UpdatedObject(a_w.Id);
        }
        else if (a_t is ResourceIdBaseT)
        {
            ResourceIdBaseT pt = (ResourceIdBaseT)a_t;
            m = ValidateExistence(pt.machineId);
            AuditEntry resAuditEntry = new AuditEntry(m.Id, m.Department.Id, m);
            m.Receive((ResourceIdBaseT)a_t, m_scenarioDetail, m_errorReporter, a_dataChanges);
            a_dataChanges.AuditEntry(resAuditEntry);
        }
    }

    #endregion Internal Transmissions

    #region ERP Transmissions
    internal ERPTHandlerResult MachineTHandler(
                                               UserFieldDefinitionManager a_udfManager,
                                               ResourceT.Resource a_mn,
                                               Department a_department,
                                               DispatcherDefinition a_dispatcherDefinition,
                                               out Resource o_m,
                                               ShopViewResourceOptions a_resourceOptions,
                                               ResourceT a_resourceImportT,
                                               IScenarioDataChanges a_dataChanges)
    {
        if (a_mn.IdSet)
        {
            o_m = GetById(a_mn.Id);
            if (o_m == null)
            {
                throw new ValidationException("2762", new object[] { a_mn.Id });
            }
        }
        else
        {
            o_m = GetByExternalId(a_mn.ExternalId);
        }

        if (o_m == null)
        {
            ReadyActivitiesDispatcher dispatcher = a_dispatcherDefinition.CreateDispatcher();
            o_m = new Resource(NextID(), a_mn, a_department, dispatcher, a_resourceOptions);
            o_m.Name = a_mn.Name;
            o_m.Update(a_mn, a_department, a_udfManager, m_scenarioDetail, a_resourceImportT, m_errorReporter, a_dataChanges);

            AddRes(o_m, a_dataChanges);
            //Add a default RCI unless instructed not to
            if (!a_mn.NoDefaultRecurringCapacityInterval)
            {
                o_m.AddRecurringCapacityInterval(m_scenarioDetail.RecurringCapacityIntervalManager.GetDefaultRCI(m_scenarioDetail, a_dataChanges));
                o_m.RegenerateCapacityProfile(m_scenarioDetail.GetPlanningHorizonEndTicks(), true);
            }

            a_dataChanges.MachineChanges.AddedObject(o_m.Id);
            a_dataChanges.AuditEntry(new AuditEntry(o_m.Id, o_m.Department.Id, o_m), true);

            return ERPTHandlerResult.added;
        }

        AuditEntry resourceAuditEntry = new AuditEntry(o_m.Id, o_m.Department.Id, o_m);
        if (o_m.Update(a_mn, a_department, a_udfManager, m_scenarioDetail, a_resourceImportT, m_errorReporter, a_dataChanges))
        {
            a_dataChanges.MachineChanges.UpdatedObject(o_m.Id);
            a_dataChanges.AuditEntry(resourceAuditEntry);
        }

        return ERPTHandlerResult.updated;
    }

    #region Resource Additions
    [Common.Attributes.DebugLogging(Common.Attributes.EDebugLoggingType.None)]
    internal delegate void ResourceAddedDelegate(Resource a_r);

    [Common.Attributes.DebugLogging(Common.Attributes.EDebugLoggingType.None)]
    internal event ResourceAddedDelegate ResourceAddedEvent;

    // All resource additions must pass through this function or the one below it.
    internal void AddRes(Resource a_m, IScenarioDataChanges a_dataChanges)
    {
        base.Add(a_m);
        a_dataChanges.FlagConstraintChanges(a_m.Id);
        if (ResourceAddedEvent != null)
        {
            ResourceAddedEvent(a_m);
        }
    }

    internal void AddRes_ConstructorVersionForSerialization(Resource a_m)
    {
        base.Add(a_m);
    }

    /// <summary>
    /// Adds an existing Resource and assigns it a new unique Id.
    /// </summary>
    /// <param name="a_m"></param>
    /// <param name="a_dataChanges"></param>
    /// <param name="m"></param>
    internal void AddResWithNewId(Resource a_m, IScenarioDataChanges a_dataChanges)
    {
        a_m.Id = NextID();
        AddRes(a_m, a_dataChanges);
    }
    #endregion
    #endregion ERP Transmissions

    private void DeleteMachine(Department a_w, ResourceDeleteT a_t, IScenarioDataChanges a_dataChanges)
    {
        //TODO: do we need to report capability changes here?
        //ArrayList capabilities = new ArrayList();
        Resource m = GetById(a_t.machineId);
        //for (int i = 0; i < m.NbrCapabilities; i++)
        //{
        //    capabilities.Add(m.GetCapabilityByIndex(i));
        //}

        Delete(a_t.machineId, a_dataChanges);
        a_dataChanges.MachineChanges.DeletedObject(m.Id);
    }

    private void DeleteMachines(List<ResourceKey> a_resourceKeys, IScenarioDataChanges a_dataChanges)
    {
        List<Department> departments = new List<Department>();
        List<Plant> plants = new List<Plant>();

        foreach (ResourceKey key in a_resourceKeys)
        {
            Resource r = GetById(key.Resource);
            departments.AddIfNew(r.Department);
            plants.AddIfNew(r.Plant);
            Delete(key.Resource, a_dataChanges);
            a_dataChanges.MachineChanges.DeletedObject(r.Id);
        }

        foreach (Department d in departments)
        {
            if (d.ResourceCount != 0)
            {
                a_dataChanges.DepartmentChanges.UpdatedObject(d.Id);
            }
        }
    }

    private void DeleteAllMachines(Department a_d, IScenarioDataChanges a_dataChanges)
    {
        Hashtable affectedCapabilitiesHash = new ();

        for (int i = Count - 1; i >= 0; i--)
        {
            Resource m = this[i];
            //Update the capabilities since their resource count will change
            for (int capabilityI = 0; capabilityI < m.NbrCapabilities; capabilityI++)
            {
                Capability cap = m.GetCapabilityByIndex(capabilityI);
                if (!affectedCapabilitiesHash.Contains(cap))
                {
                    affectedCapabilitiesHash.Add(cap, null);
                    a_dataChanges.CapabilityChanges.UpdatedObject(cap.Id);
                }
            }

            for (int ciI = 0; ciI < m.CapacityIntervals.Count; ciI++)
            {
                a_dataChanges.CapacityIntervalChanges.UpdatedObject(m.CapacityIntervals[ciI].Id);
            }

            for (int rciI = 0; rciI < m.RecurringCapacityIntervals.Count; rciI++)
            {
                a_dataChanges.RecurringCapacityIntervalChanges.UpdatedObject(m.RecurringCapacityIntervals[rciI].Id);
            }

            Delete(m.Id, a_dataChanges);
            a_dataChanges.MachineChanges.DeletedObject(m.Id);
        }
    }

    internal void Deleting(Department a_d, IScenarioDataChanges a_dataChanges)
    {
        DeleteAllMachines(a_d, a_dataChanges);
    }

    #region ICopyTable
    public override Type ElementType => typeof(Resource);
    #endregion

    //TODO: Finish deepcopy
    //public ResourceManager DeepCopy(ScenarioDetail a_sd, BaseIdGenerator a_idGen, Department a_department)
    //{
    //    ResourceManager newResManager = new ResourceManager(a_sd, a_idGen);
    //    newResManager.m_scenarioDetail = m_scenarioDetail;
    //    newResManager.m_errorReporter = m_errorReporter;
    //    newResManager.m_scenario = m_scenario;

    //    foreach (Resource resource in this)
    //    {
    //        Resource newResource = new Resource(resource, a_idGen.NextID(), a_sd);
    //        newResource.ExternalId = ExternalBaseIdObject.MakeExternalId(newResource.Id.Value);
    //        newResource.Department = a_department;
    //        newResManager.AddRes(newResource);
    //    }

    //    return newResManager;
    //}
}
