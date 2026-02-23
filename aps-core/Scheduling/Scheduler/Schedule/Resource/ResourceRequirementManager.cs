using System.Collections;

using PT.APSCommon;
using PT.APSCommon.Collections;
using PT.Common.Exceptions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Contains a list of ResourceRequirements
/// </summary>
public partial class ResourceRequirementManager : IPTSerializable, IEnumerable
{
    private readonly BaseIdGenerator m_idGen;

    #region IPTSerializable Members
    public ResourceRequirementManager(IReader a_reader, BaseIdGenerator a_idGen)
    {
        m_idGen = a_idGen;
        if (a_reader.VersionNumber >= 13002)
        {
            BaseId primaryId = new(a_reader);
            //The list serializes the RRs
            m_resourceRequirements = new SortedResourceRequirementList(a_reader, primaryId);
        }
        else if (a_reader.VersionNumber >= 12033)
        {
            a_reader.Read(out long mLastId);
            BaseId primaryId = new (a_reader);
            //The list serializes the RRs
            m_resourceRequirements = new SortedResourceRequirementList(a_reader, primaryId);
        }
        else if (a_reader.VersionNumber >= 113)
        {
            a_reader.Read(out long mLastId);
            a_reader.Read(out int count);
            BaseId primaryRequirementId = new (a_reader);

            m_resourceRequirements = new SortedResourceRequirementList(primaryRequirementId);

            for (int i = 0; i < count; i++)
            {
                ResourceRequirement rr = new (a_reader);

                m_resourceRequirements.Add(rr);
            }
        }
    }

    public void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif
        m_resourceRequirements.GetByIndex(0).Id.Serialize(a_writer);
        m_resourceRequirements.Serialize(a_writer);
    }

    public const int UNIQUE_ID = 310;

    public int UniqueId => UNIQUE_ID;

    internal void RestoreReferences(InternalOperation a_op, CapabilityManager a_capabilities, PlantManager a_plants)
    {
        m_operation = a_op;
        for (int i = 0; i < Count; i++)
        {
            GetByIndex(i).RestoreReferences(a_capabilities, a_op, a_plants);
        }
    }
    #endregion

    #region Declarations
    public class ResourceRequirementManagerException : PTException
    {
        public ResourceRequirementManagerException(string a_message)
            : base(a_message) { }
    }

    private readonly SortedResourceRequirementList m_resourceRequirements;
    public SortedResourceRequirementList Requirements => m_resourceRequirements;
    #endregion

    #region Construction
    internal ResourceRequirementManager(InternalOperation a_internalOperation, BaseId a_primaryId)
    {
        m_operation = a_internalOperation;
        m_resourceRequirements = new SortedResourceRequirementList(a_primaryId);
    }

    internal ResourceRequirementManager(InternalOperation a_internalOperation, ResourceRequirementManager a_sourceRrm)
    {
        m_operation = a_internalOperation;
        m_resourceRequirements = new SortedResourceRequirementList(a_sourceRrm.PrimaryResourceRequirement.Id);

        //Add Resource Requirements
        for (int rr = 0; rr < a_sourceRrm.Count; rr++)
        {
            ResourceRequirement sourceRR = a_sourceRrm.GetByIndex(rr);
            ResourceRequirement newRR = new (sourceRR, a_internalOperation);
            Add(newRR);
        }
    }
    #endregion

    #region Properties
    private InternalOperation m_operation;

    internal bool RequirementsUseSetup => PrimaryResourceRequirement.UsageStart <= MainResourceDefs.usageEnum.Setup;
    internal bool RequirementsUsePostProcessing => PrimaryResourceRequirement.UsageEnd >= MainResourceDefs.usageEnum.PostProcessing;
    internal bool RequirementsUseStorage => PrimaryResourceRequirement.UsageEnd >= MainResourceDefs.usageEnum.Storage;

    internal bool RequirementsUseCleanTime => PrimaryResourceRequirement.UsageEnd >= MainResourceDefs.usageEnum.Clean;
    #endregion

    #region Operations
    internal void Add(ResourceRequirement a_requirement)
    {
        m_resourceRequirements.Add(a_requirement);
    }

    internal void Remove(ResourceRequirement a_rr)
    {
        m_resourceRequirements.RemoveByKey(a_rr.Id);
    }

    public ResourceRequirement this[BaseId a_requirementId] => m_resourceRequirements.GetValue(a_requirementId);

    /// <summary>
    /// The number of resource requirements.
    /// </summary>
    public int Count => m_resourceRequirements.Count;

    public ResourceRequirement GetByIndex(int a_index)
    {
        return m_resourceRequirements.GetByIndex(a_index);
    }

    private void Clear()
    {
        m_resourceRequirements.Clear();
    }
    #endregion

    #region Primary Resource Requirement
    internal void SetPrimaryResourceRequirement(ResourceRequirement a_rr)
    {
        ResourceRequirement primaryResourceRequirement = m_resourceRequirements.GetValue(a_rr.Id);

        if (primaryResourceRequirement != null)
        {
            m_resourceRequirements.UpdatePrimaryIndex(a_rr.Id);
            return;
        }

        //Primary RR not in the collection
        throw new PTValidationException("2279");
    }

    public ResourceRequirement PrimaryResourceRequirement => GetByIndex(0);

    [Obsolete("The primary index is always 0")]
    public int PrimaryResourceRequirementIndex => 0;
    #endregion

    public void PopulateJobDataSet(ref JobDataSet a_dataSet)
    {
        for (int i = 0; i < Count; i++)
        {
            ResourceRequirement rr = GetByIndex(i);
            rr.PopulateJobDataSet(ref a_dataSet, PrimaryResourceRequirement == rr);
        }
    }

    /// <summary>
    /// Among all the plants and eligible resources or the primary resource requirement, the first eligible resource is returned or null if there are no eligible resources.
    /// </summary>
    /// <returns></returns>
    internal InternalResource GetFirstEligiblePrimaryResource()
    {
        return PrimaryResourceRequirement.GetFirstEligibleResource();
    }

    #region Update
    internal bool Update(ResourceRequirementManager a_tRRManager,
                                 IScenarioDataChanges a_dataChanges,
                                 ProductRuleManager a_productRuleManager,
                                 out bool o_unscheduledJob,
                                 out string o_unscheduleInfo,
                                 out bool o_jitChanges)
    {
        o_unscheduledJob = false;
        o_unscheduleInfo = String.Empty;
        o_jitChanges = false;
        HashSet<BaseId> affectedRRs = new ();

        bool newPrimaryResourceRequirement = PrimaryResourceRequirement.ExternalId != a_tRRManager.PrimaryResourceRequirement.ExternalId;
        bool updated = false;
        bool requirementAdded = false;

        bool opScheduled = m_operation.Scheduled;

        for (int requirementI = 0; requirementI < a_tRRManager.Count; requirementI++)
        {
            ResourceRequirement newRR = a_tRRManager.GetByIndex(requirementI);
            //Validate that there aren't activities locked to a resource that's in a different plant than the default resource
            if (newRR.HasDefaultResource)
            {
                if (newRR.DefaultResource is Resource defaultRes)
                {
                    if (m_operation.Locked != lockTypes.Unlocked)
                    {
                        Resource lockedRes = m_operation.GetFirstLockedResource();
                        if (lockedRes.PlantId != defaultRes.PlantId)
                        {
                            throw new PTValidationException("2763", new object[] { m_operation.ExternalId, m_operation.Job.ExternalId, lockedRes.ExternalId, defaultRes.ExternalId });
                        }
                    }
                }
            }

            ResourceRequirement rr = GetByExternalId(newRR.ExternalId);

            if (rr != null)
            {
                updated |= rr.Update(newRR, a_dataChanges, out o_jitChanges);
                affectedRRs.Add(rr.Id);
            }
            else
            {
                ResourceRequirement copyOfRR = new (m_idGen.NextID(), newRR, m_operation);
                Add(copyOfRR);
                updated = true;
                affectedRRs.Add(copyOfRR.Id);
                requirementAdded = true;
            }
        }

        for (int i = Count - 1; i >= 0; i--)
        {
            ResourceRequirement rr = GetByIndex(i);
            if (!affectedRRs.Contains(rr.Id))
            {
                Remove(rr);
                updated = true;
            }
        }

        //Check if the new primary is scheduled
        if (opScheduled && (newPrimaryResourceRequirement || requirementAdded))
        {
            o_unscheduledJob = true;
            o_unscheduleInfo = string.Format("Job '{0}' was unscheduled because the primary resource requirement for Operation '{1}' in ManufacturingOrder '{2}' has been changed and the operation is not scheduled on the new primary resource.".Localize(), m_operation.ManufacturingOrder.Job.Name, m_operation.Name, m_operation.ManufacturingOrder.Name);
        }

        ResourceRequirement rrNewPrimary = GetByExternalId(a_tRRManager.PrimaryResourceRequirement.ExternalId);
        if (rrNewPrimary != null)
        {
            SetPrimaryResourceRequirement(rrNewPrimary); //********* Evaluate the importance of this change. //*********
        }
        else
        {
            throw new PTValidationException("2763", new object[] { a_tRRManager.PrimaryResourceRequirement.ExternalId });
        }

        if (!o_unscheduledJob)
        {
            //Check newly added or updated Resource Requirements result in the scheduled activities being ineligible, if so mark the job to unschedule
            for (int activityI = 0; activityI < m_operation.Activities.Count; ++activityI)
            {
                InternalActivity ia = m_operation.Activities.GetByIndex(activityI);
                for (int resourceRequirementBlockI = 0; resourceRequirementBlockI < ia.ResourceRequirementBlockCount; ++resourceRequirementBlockI)
                {
                    ResourceBlock rb = ia.GetResourceRequirementBlock(resourceRequirementBlockI);
                    if (rb != null)
                    {
                        //this block is scheduled since it's not null
                        try
                        {
                            ResourceRequirement rr = GetByIndex(resourceRequirementBlockI);
                            if (affectedRRs.Contains(rr.Id))
                            {
                                Resource res = rb.ScheduledResource;
                                if (!rr.IsEligible(res, a_productRuleManager))
                                {
                                    o_unscheduledJob = true;
                                    o_unscheduleInfo = string.Format("Job '{0}' was unscheduled because Operation '{1}' in ManufacturingOrder '{2}' is no longer eligible to schedule on Resource '{3}'.".Localize(), ia.ManufacturingOrder.Job.Name, ia.Operation.Name, ia.ManufacturingOrder.Name, res.Name);
                                    break;
                                }
                            }
                        }
                        catch (ResourceBlock.NullBatchException) { }
                    }
                }

                if (!string.IsNullOrEmpty(o_unscheduleInfo))
                {
                    break;
                }
            }
        }

        return updated;
    }

    /// <summary>
    /// O(n).
    /// Get a ResourceRequirement by external id.
    /// </summary>
    /// <param name="a_externalId">The external id of the ResourceRequirement that you want.</param>
    /// <returns>The ResourceRequirement that you want or null if it's not here.</returns>
    private ResourceRequirement GetByExternalId(string a_externalId)
    {
        for (int resourceRequirementI = 0; resourceRequirementI < Count; ++resourceRequirementI)
        {
            ResourceRequirement rr = GetByIndex(resourceRequirementI);
            if (rr.ExternalId == a_externalId)
            {
                return rr;
            }
        }

        return null;
    }
    #endregion

    #region General functions
    internal List<MainResourceDefs.Usage> GetUsages()
    {
        return m_resourceRequirements.ReadOnlyList.Select(a_rr => a_rr.GetUsage()).ToList();
    }

    //internal void GetRequirementTypes(out bool a_setup, out bool a_run, out bool a_postProc)
    //{
    //    a_setup = false;
    //    a_run = false;
    //    a_postProc = false;
    //    for (int rrI = 0; rrI < Count; ++rrI)
    //    {
    //        ResourceRequirement rr = GetByIndex(rrI);

    //        a_setup = rr.UsageEnd >= MainResourceDefs.usageEnum.Setup;
    //        a_run = rr.UsageEnd >= MainResourceDefs.usageEnum.Run;

    //        a_postProc = rr.UsageEnd >= MainResourceDefs.usageEnum.PostProcessing;
    //    }
    //}

    /// <summary>
    /// returns the minimum UsageEnd among all the ResourceRequirements.
    /// </summary>
    internal MainResourceDefs.usageEnum GetMinUsageEnd()
    {
        MainResourceDefs.usageEnum minUsageEnd = MainResourceDefs.usageEnum.Clean;
        for (int rrI = 0; rrI < Count; ++rrI)
        {
            ResourceRequirement rr = GetByIndex(rrI);
            if (rr.UsageEnd < minUsageEnd)
            {
                minUsageEnd = rr.UsageEnd;
            }
        }

        return minUsageEnd;
    }
    #endregion

    public override string ToString()
    {
        return string.Format("{0} Resource Requirements", Count);
    }

    public IEnumerator GetEnumerator()
    {
        return m_resourceRequirements.GetEnumerator();
    }

    /// <summary>
    /// Whether the primary resource requirement has a default resource.
    /// </summary>
    public bool HasDefaultResource => PrimaryResourceRequirement.HasDefaultResource;

    public class SortedResourceRequirementList : BaseIdCustomSortedList<ResourceRequirement>
    {
        public SortedResourceRequirementList(BaseId a_primaryRrId) : base(new ResourceRequirementComparer(a_primaryRrId)) { }
        public SortedResourceRequirementList(IReader a_reader, BaseId a_primaryRrId, BaseIdGenerator a_idGenerator = null) : base(a_reader, new ResourceRequirementComparer(a_primaryRrId), a_idGenerator) { }

        protected override ResourceRequirement CreateInstance(IReader a_reader)
        {
            return new ResourceRequirement(a_reader);
        }

        protected override ResourceRequirement CreateKeyValue(object a_key)
        {
            throw new NotImplementedException();
        }

        internal void UpdatePrimaryIndex(BaseId a_baseId)
        {
            //Update the primary index, resort so primary is first
            ((ResourceRequirementComparer)m_comparer).UpdatePrimaryId(a_baseId);
            ClearSortedList();
        }

        private class ResourceRequirementComparer : IBaseIdKeyObjectComparer<ResourceRequirement>
        {
            private BaseId m_primaryRequirementId;

            internal ResourceRequirementComparer(BaseId a_primaryRequirementId)
            {
                m_primaryRequirementId = a_primaryRequirementId;
            }

            /// <summary>
            /// Order RR by Primary Index, then BaseId
            /// </summary>
            /// <param name="a_x"></param>
            /// <param name="a_y"></param>
            /// <returns></returns>
            public int Compare(ResourceRequirement a_x, ResourceRequirement a_y)
            {
                //Assuming no nulls

                //Return primary requirement as lowest to sort first
                if (a_x.Id == m_primaryRequirementId)
                {
                    return -1;
                }

                if (a_y.Id == m_primaryRequirementId)
                {
                    return 1;
                }

                //Otherwise sort by BaseId
                return a_x.Id.CompareTo(a_y.Id);
            }

            public BaseId GetKey(ResourceRequirement a_rr)
            {
                return a_rr.Id;
            }

            internal void UpdatePrimaryId(BaseId a_baseId)
            {
                m_primaryRequirementId = a_baseId;
            }
        }
    }
}