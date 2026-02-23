using System.Collections;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.ERPTransmissions;
using PT.Scheduler.ErrorReporting;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of Capability objects.
/// </summary>
public partial class CapabilityManager : ScenarioBaseObjectManager<Capability>, IPTSerializable, ICloneable
{
    #region IPTSerializable Members
    public CapabilityManager(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_idGen)
    {
        if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                Capability c = new (a_reader);
                Add(c);
            }
        }
    }
    
    public new const int UNIQUE_ID = 334;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    public class CapabilityManagerException : PTException
    {
        public CapabilityManagerException(string message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(message, a_stringParameters, a_appendHelpUrl) { }
    }
    #endregion

    #region Construction
    public CapabilityManager(BaseIdGenerator idGen)
        : base(idGen) { }

    private CapabilityManager(CapabilityManager cm, BaseIdGenerator idGen)
        : base(cm, idGen) { }
    #endregion

    #region Find functions
    /// <summary>
    /// cm.Count(n(log2(n)))
    /// Determine whether this CapabilityManager contains all the elements in another CapabilityManager.
    /// If there are no elements in the passed in CapabilityManager the return value of this function is true.
    /// </summary>
    /// <param name="cm">The capabilities you are testing containment on.</param>
    /// <returns>Whether all the capabilities are contained within this CapabilityManager.</returns>
    internal bool Contains(CapabilityManager cm)
    {
        for (int capabilityI = 0; capabilityI < cm.Count; ++capabilityI)
        {
            Capability c = cm.GetByIndex(capabilityI);

            if (!Contains(c))
            {
                return false;
            }
        }

        return true;
    }
    #endregion

    #region Capability Edit Functions
    private Capability AddDefault(CapabilityDefaultT t)
    {
        Capability m = new (NextID());
        ValidateAdd(m);
        return base.Add(m);
    }

    private Capability AddCopy(CapabilityCopyT t)
    {
        ValidateCopy(t);
        Capability m = GetById(t.originalId);
        Capability copy = base.AddCopy(m, m.Clone(), NextID());
        for (int i = 0; i < m.Resources.Count; i++)
        {
            InternalResource resource = m.Resources.GetByIndex(i);
            copy.AddResourceAssociation(resource);
            resource.AddCapability(copy);
        }

        return copy;
    }

    private Capability Delete(CapabilityDeleteT t, ProductRuleManager a_productRuleManager)
    {
        Capability m = GetById(t.machineCapabilityId);
        Delete(m, a_productRuleManager);
        return m;
    }

    private Capability Delete(BaseId a_capabilityToDeleteId, ProductRuleManager a_productRuleManager)
    {
        Capability m = GetById(a_capabilityToDeleteId);
        Delete(m, a_productRuleManager);
        return m;
    }
    #endregion

    #region Transmissions
    private void ValidateAdd(Capability m)
    {
        if (Contains(m.Id))
        {
            throw new CapabilityManagerException("2747", new object[] { m.Id.ToString() });
        }
    }

    private void ValidateCopy(CapabilityCopyT t)
    {
        ValidateExistence(t.originalId);
    }

    private void ValidateDelete(CapabilityDeleteT t)
    {
        Capability capability = ValidateExistence(t.machineCapabilityId);
        ResourceRequirement rr = m_scenarioDetail.JobManager.GetFirstRequirementUsingCapability(capability);
        if (rr != null)
        {
            throw new CapabilityManagerException("2748", new object[] { capability.Name, rr.Operation.ManufacturingOrder.Job.Name, rr.Operation.ManufacturingOrder.Name, rr.Operation.Name });
        }
    }

    public void Receive(CapabilityBaseT t, ProductRuleManager a_productRuleManager, IScenarioDataChanges a_dataChanges)
    {
        Capability m = null;
        ScenarioEvents se;
        if (t is CapabilityDefaultT)
        {
            m = AddDefault((CapabilityDefaultT)t);
            a_dataChanges.CapabilityChanges.AddedObject(m.Id);
            a_dataChanges.AuditEntry(new AuditEntry(m.Id, m), true);
        }
        else if (t is CapabilityCopyT)
        {
            m = AddCopy((CapabilityCopyT)t);
            a_dataChanges.CapabilityChanges.AddedObject(m.Id);
            a_dataChanges.AuditEntry(new AuditEntry(m.Id, m), true);
        }
        else if (t is CapabilityDeleteT capabilityDeleteT)
        {
            //TODO: Do we need to add resource data changes?
            if (capabilityDeleteT.IsMultiDelete)
            {
                foreach (BaseId capabilityToDeleteId in capabilityDeleteT.CapabilityIdsToDelete)
                {
                    m = Delete(capabilityToDeleteId, a_productRuleManager);
                    if (m != null)
                    {
                        a_dataChanges.AuditEntry(new AuditEntry(m.Id, m), false, true);
                        a_dataChanges.CapabilityChanges.DeletedObject(m.Id);
                    }
                }
            }
            else
            {
                m = Delete(capabilityDeleteT, a_productRuleManager);
                a_dataChanges.AuditEntry(new AuditEntry(m.Id, m), false, true);
                a_dataChanges.CapabilityChanges.DeletedObject(m.Id);
            }
        }
        else if (t is CapabilityDeleteAllT)
        {
            Hashtable affectedResourcesHash = new ();

            for (int i = Count - 1; i >= 0; i--)
            {
                m = GetByIndex(i);
                for (int resI = 0; resI < m.Resources.Count; resI++)
                {
                    InternalResource resource = m.Resources.GetByIndex(resI);
                    if (!affectedResourcesHash.Contains(resource))
                    {
                        a_dataChanges.MachineChanges.UpdatedObject(resource.Id);
                        affectedResourcesHash.Add(resource, resource);
                    }
                }

                Delete(m, a_productRuleManager);
                a_dataChanges.AuditEntry(new AuditEntry(m.Id, m), false, true);
                a_dataChanges.CapabilityChanges.DeletedObject(m.Id);
            }
        }
    }

    internal void Receive(CapabilityT a_t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        ApplicationExceptionList errList = new ();
        List<PostProcessingAction> actions = new ();

        Hashtable affectedCapabilitys = new ();

        HashSet<BaseId> affectedResourcesHash = new ();

        try
        {
            InitFastLookupByExternalId();

            for (int i = 0; i < a_t.Count; ++i)
            {
                try
                {
                    CapabilityT.Capability machineCapabilityNode = a_t[i];
                    Capability machineCapability;
                    if (machineCapabilityNode.IdSet)
                    {
                        machineCapability = GetById(machineCapabilityNode.Id);
                        if (machineCapability == null)
                        {
                            throw new ValidationException("2272", new object[] { machineCapabilityNode.Id });
                        }
                    }
                    else
                    {
                        machineCapability = GetByExternalId(machineCapabilityNode.ExternalId);
                    }

                    if (machineCapability == null)
                    {
                        machineCapability = new Capability(NextID(), machineCapabilityNode, a_t);
                        Add(machineCapability);
                        a_dataChanges.CapabilityChanges.AddedObject(machineCapability.Id);
                        a_dataChanges.AuditEntry(new AuditEntry(machineCapability.Id, machineCapability), true);
                    }
                    else
                    {
                        AuditEntry capAuditEntry = new AuditEntry(machineCapability.Id, machineCapability);
                        a_dataChanges.CapabilityChanges.UpdatedObject(machineCapability.Id);
                        machineCapability.Update(machineCapabilityNode, a_t, a_sd);
                        a_dataChanges.AuditEntry(capAuditEntry);
                    }

                    if (!affectedCapabilitys.Contains(machineCapability.Id)) //could have duplicate records in the transmission
                    {
                        affectedCapabilitys.Add(machineCapability.Id, null);
                    }
                }
                catch (PTHandleableException err)
                {
                    errList.Add(err);
                }
            }

            if (a_t.AutoDeleteMode)
            {
                List<Capability> toDelete = new ();

                foreach (Capability c in this)
                {
                    if (!affectedCapabilitys.ContainsKey(c.Id))
                    {
                        toDelete.Add(c);

                        //Keep track of which Resources were affected by the delete of Capabilities
                        for (int resI = 0; resI < c.Resources.Count; resI++)
                        {
                            InternalResource resource = c.Resources.GetByIndex(resI);
                            if (!affectedResourcesHash.Contains(resource.Id))
                            {
                                a_dataChanges.MachineChanges.UpdatedObject(resource.Id);
                                affectedResourcesHash.Add(resource.Id);
                            }
                        }
                    }
                }

                actions.Add(new PostProcessingAction(a_t, false, () =>
                    {
                        ApplicationExceptionList delErrs = new ();

                        foreach (Capability c in toDelete)
                        {
                            try
                            {
                                Delete(c, a_sd.ProductRuleManager);
                                a_dataChanges.AuditEntry(new AuditEntry(c.Id, c), false, true);
                                a_dataChanges.CapabilityChanges.DeletedObject(c.Id);
                            }
                            catch (PTValidationException err)
                            {
                                delErrs.Add(err);
                            }
                        }

                        if (delErrs.Count > 0)
                        {
                            ScenarioExceptionInfo sei = new ();
                            sei.Create(a_sd);
                            m_errorReporter.LogException(delErrs, a_t, sei, ELogClassification.PtInterface, false);
                        }
                    }));
            }
        }
        catch (PTHandleableException err)
        {
            errList.Add(err);
        }
        finally
        {
            actions.Add(new PostProcessingAction(a_t, true, () => { DeInitFastLookupByExternalId(); }));
            m_scenarioDetail.AddProcessingAction(actions);

            if (errList.Count > 0)
            {
                ScenarioExceptionInfo sei = new ();
                sei.Create(a_sd);
                m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
            }
        }
    }

    public new void Add(Capability c)
    {
        base.Add(c);
    }

    /// <summary>
    /// Adds a Capablilty with the specified Name.  Used to auto create Capabilities that are referenced during Job create but don't yet exist.
    /// </summary>
    internal Capability AutoAdd(string capabilityExternalId, string capabilityName, IScenarioDataChanges a_dataChanges)
    {
        Capability mc = new (NextID());
        mc.ExternalId = capabilityExternalId;
        mc.Name = capabilityName;
        mc.Description = "AUTO-CREATED for one or more Jobs that referenced this Capability (See 'System | Options' to disable Auto-Creation)".Localize();
        Add(mc);

        BaseId scenarioId;
        ScenarioSummary ss;
        using (m_scenario.ScenarioSummaryLock.EnterRead(out ss))
        {
            scenarioId = ss.Id;
        }

        CapabilityDefaultT defaultT = new (scenarioId);
        a_dataChanges.CapabilityChanges.AddedObject(mc.Id);

        return mc;
    }
    #endregion

    #region ERP Transmissions
    internal void Delete(Capability c, ProductRuleManager a_productRuleManager)
    {
        if (c == null)
        {
            return;
        }

        ValidateDelete(c);
        c.Deleting(a_productRuleManager);
        Remove(c.Id);
    }

    public new void Remove(Capability c)
    {
        Remove(c.Id);
    }

    private void ValidateDelete(Capability c)
    {
        ResourceRequirement rr = m_scenarioDetail.JobManager.GetFirstRequirementUsingCapability(c);
        if (rr != null)
        {
            throw new ValidationException("2273", new object[] { c.ExternalId, rr.Operation.ManufacturingOrder.Job.ExternalId, rr.Operation.ManufacturingOrder.ExternalId, rr.Operation.ExternalId });
        }
    }

    /// <summary>
    /// Delete All Capabilities.
    /// </summary>
    internal void Clear(ProductRuleManager a_productRuleManager, IScenarioDataChanges a_dataChanges)
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            a_dataChanges.CapabilityChanges.DeletedObject(this[i].Id);
            Delete(this[i], a_productRuleManager);
        }
    }
    #endregion

    #region ICopyTable
    public override Type ElementType => typeof(Capability);
    #endregion

    #region ICloneable Members
    public object Clone()
    {
        // Calling clone is okay on the next line because the key is a BaseId struct and the value is a capability.
        return new CapabilityManager(this, IdGen);
    }
    #endregion
}
