using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.APSCommon.Properties;
using PT.Common.Compression;
using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.Common.File;
using PT.Common.Localization;
using PT.ERPTransmissions;
using PT.PackageDefinitions.Settings;
using PT.Scheduler.ErrorReporting;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.Scheduler.Schedule.Storage;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using static PT.ERPTransmissions.JobT;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of Job objects.
/// </summary>
public partial class JobManager : ScenarioBaseObjectManager<Job>, IPTSerializable
{
    // this is used during deserialization to help speed up linking Successor MOs. Its value is not maintained and should not be used after deserialization.
    private readonly bool m_hasSuccessorMos;

    #region IPTSerializable Members
    public JobManager(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_idGen)
    {
        if (a_reader.VersionNumber >= 12027)
        {
            a_reader.Read(out m_nextExternalIdNbr);
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                Job job = new (a_reader, IdGen);
                Add(job);
                m_hasSuccessorMos = m_hasSuccessorMos || job.HasSuccessorMOs();
            }
        }
        else if (a_reader.VersionNumber >= 2)
        {
            a_reader.Read(out m_nextExternalIdNbr);
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                Job job = new (a_reader, IdGen);
                Add(job);
                m_hasSuccessorMos = m_hasSuccessorMos || job.HasSuccessorMOs();
            }

            //TODO: Maintain Backwards compatibility
            new HoldSettings(a_reader);
        }

        if (a_reader.VersionNumber < 13012)
        {
            RecalculateJitForBackwardsCompatibility = true;
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_nextExternalIdNbr);

        base.Serialize(a_writer);
    }

    public new const int UNIQUE_ID = 411;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        foreach (Job job in this)
        {
            job.RestoreReferences(a_udfManager);
        }
    }

    internal void RestoreReferences(CustomerManager customers, PlantManager plants, CapabilityManager capabilities, ScenarioDetail sd, WarehouseManager aWarehouses, ItemManager aItems, ISystemLogger a_errorReporter)
    {
        m_scenarioDetail = sd;
        if (m_hasSuccessorMos)
        {
            InitFastLookupByExternalId();
        }

        try
        {
            for (int i = 0; i < Count; i++)
            {
                Job job = this[i];
                job.RestoreReferences(customers, plants, capabilities, sd, aWarehouses, aItems, a_errorReporter);

                foreach (InternalActivity act in job.GetActivities())
                {
                    act.InitializeProductionInfoForResources(sd.PlantManager, sd.ProductRuleManager, sd.ExtensionController);
                }

                if (RecalculateJitForBackwardsCompatibility)
                {
                    job.ComputeEligibility(sd.ProductRuleManager);
                    job.CalculateJitTimes(sd.Clock, false);
                }
            }

            // RestoreReferences2() is called after all job references have been restored.
            for (int i = 0; i < Count; i++)
            {
                this[i].RestoreReferences2();
            }

            sd.SuccessorMOsLinked(); // restoreReferences2 links successor MOs
        }
        finally
        {
            if (m_hasSuccessorMos)
            {
                DeInitFastLookupByExternalId();
            }
        }
    }

    internal void AfterRestoreAdjustmentReferences()
    {
        foreach (Job job in this)
        {
            foreach (InternalOperation op in job.GetOperations())
            {
                foreach (MaterialRequirement mr in op.MaterialRequirements)
                {
                    mr.MRSupply.AfterRestoreAdjustmentReferences();
                }
            }
        }
    }
    #endregion

    #region BackwardsCompatibility
    internal bool RecalculateJitForBackwardsCompatibility;
    #endregion

    #region Declarations
    public class JobManagerException : PTException
    {
        public JobManagerException(string message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(message, a_stringParameters, a_appendHelpUrl) { }
    }

    internal ScenarioDetail ScenarioDetail => m_scenarioDetail;
    #endregion

    #region Construction
    public JobManager(ScenarioDetail sd, BaseIdGenerator idGen)
        : base(idGen)
    {
        m_scenarioDetail = sd;
    }
    #endregion

    #region Find functions
    /// <summary>
    /// Returns the Block or null if there is no Block.
    /// </summary>
    public InternalOperation FindOperation(BaseId jobId, BaseId moId, BaseId opId)
    {
        Job job = GetById(jobId);
        return job?.FindOperation(moId, opId);
    }

    public InternalActivity FindActivity(ActivityKey a_ak)
    {
        InternalActivity act = null;
        Job j = GetById(a_ak.JobId);
        if (j != null)
        {
            ManufacturingOrder mo = j.ManufacturingOrders.GetById(a_ak.MOId);
            if (mo != null)
            {
                InternalOperation op = (ResourceOperation)mo.OperationManager[a_ak.OperationId];
                if (op != null)
                {
                    act = op.Activities.FindActivity(a_ak.ActivityId);
                }
            }
        }

        return act;
    }

    public InternalActivity FindActivityByExternalId(ActivityKeyExternal a_ake)
    {
        InitFastLookupByExternalId();
        InternalActivity act = null;

        Job j = GetByExternalId(a_ake.JobExternalId);
        if (j != null)
        {
            ManufacturingOrder mo = j.ManufacturingOrders.GetByExternalId(a_ake.MOExternalId);
            if (mo != null)
            {
                InternalOperation op = (ResourceOperation)mo.OperationManager[a_ake.OperationExternalId];
                if (op != null)
                {
                    act = op.Activities.GetByExternalId(a_ake.ActivityExternalId);
                }
            }
        }

        DeInitFastLookupByExternalId();

        return act;
    }

    internal ResourceBlock FindBlock(BaseId aJobId, BaseId aMoId, BaseId aOpId, BaseId aActivityId, BaseId aBlockId)
    {
        Job j = GetById(aJobId);
        ManufacturingOrder mo = j.ManufacturingOrders.GetById(aMoId);
        InternalOperation op = (ResourceOperation)mo.OperationManager[aOpId];
        InternalActivity act = op.Activities.FindActivity(aActivityId);
        return act.GetResourceBlock(aBlockId);
    }

    public class GetJobsException : PTException
    {
        public GetJobsException()
            : base("The job couldn't be found.") //***LRH***Localize
        { }
    }

    /// <summary>
    /// </summary>
    /// <param name="a_jobKeyList">The jobs that you want to find. Throws GetJobsException if a job doesn't exist.</param>
    /// <returns></returns>
    private List<Job> GetJobs(BaseIdList a_jobKeyList)
    {
        List<Job> jobs = new ();
        foreach (BaseId baseId in a_jobKeyList)
        {
            Job job = GetById(baseId);

            if (job == null)
            {
                throw new GetJobsException();
            }

            jobs.Add(job);
        }

        return jobs;
    }

    private List<Job> GetAllJobs()
    {
        return this.ToList();
    }

    internal List<Job> GetNotFinishedJobs()
    {
        List<Job> jobs = new ();
        foreach (Job j in this)
        {
            if (j.ScheduledStatus != JobDefs.scheduledStatuses.Finished && j.ScheduledStatus != JobDefs.scheduledStatuses.Template)
            {
                jobs.Add(j);
            }
        }

        return jobs;
    }

    public IEnumerable<Job> GetAllNewJobs()
    {
        return GetAllJobs().Where(j => j.ScheduledStatus == JobDefs.scheduledStatuses.New);
    }

    public IEnumerable<Job> GetLateJobs()
    {
        return GetAllJobs().Where(j => j.Late);
    }

    /// <summary>
    /// Iterates through all jobs and returns the total lateness in Ticks of all scheduled jobs
    /// </summary>
    /// <returns>Ticks</returns>
    public long CalcTotalJobLateness()
    {
        long lateness = 0;
        for (int i = 0; i < Count; i++)
        {
            if (this[i].ScheduledStatus == JobDefs.scheduledStatuses.Scheduled && this[i].Lateness.Ticks > 0)
            {
                lateness += this[i].Lateness.Ticks;
            }
        }

        return lateness;
    }

    /// <summary>
    /// Returns a list of Job Ids and their current total anchor drift.
    /// </summary>
    /// <returns></returns>
    public List<Tuple<BaseId, long>> CalculateJobAnchorDrift()
    {
        List<Tuple<BaseId, long>> anchorList = new ();

        for (int jobI = 0; jobI < Count; jobI++)
        {
            Job currentJob = this[jobI];
            if (currentJob.Anchored != anchoredTypes.Free && currentJob.Scheduled)
            {
                long totalAnchorDrift = 0;
                List<BaseOperation> opList = currentJob.GetOperations();
                foreach (BaseOperation baseOp in opList)
                {
                    InternalOperation op = baseOp as InternalOperation;
                    if (op != null)
                    {
                        for (int actI = 0; actI < op.Activities.Count; actI++)
                        {
                            InternalActivity act = op.Activities.GetByIndex(actI);
                            if (act.Scheduled && act.Anchored)
                            {
                                try
                                {
                                    totalAnchorDrift += act.AnchorDrift.Ticks;
                                }
                                catch
                                {
                                    //There can be an error in anchordrift if the anchor value hasn't been set correctly.
                                }
                            }
                        }
                    }
                }

                anchorList.Add(new Tuple<BaseId, long>(currentJob.Id, totalAnchorDrift));
            }
        }

        return anchorList;
    }

    public decimal GetAverageLateness()
    {
        return Count > 0 ? this.Average(x => x.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled ? (decimal)x.Lateness.TotalDays : 0) : 0;
    }
    #endregion

    #region Job Edit Functions
    private Job AddDefault(JobDefaultT t)
    {
        Job j = new (NextID(), m_scenarioDetail, t.TimeStamp, t.Instigator, m_errorReporter);
        j.MaintenanceMethod = JobDefs.EMaintenanceMethod.Manual;
        ValidateAdd(j);
        RecordHistoryNewJob(j);
        return Add(j);
    }

    private void RecordHistoryNewJob(Job job)
    {
        string customer = job.Customers.GetCustomerExternalIdsList();

        string desc = string.Format("Job {0} added for {1} {2}".Localize(), job.Name, job.Summary, customer);
        ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(new[] { job.Id }, desc, typeof(Job), ScenarioHistory.historyTypes.JobAdded);
    }
    #endregion

    #region Transmissions
    private void ValidateAdd(Job j)
    {
        if (Contains(j.Id))
        {
            throw new JobManagerException("2745", new object[] { j.Id.ToString() });
        }
    }

    private void ValidateCopy(JobCopyT t)
    {
        ValidateExistence(t.JobIdToCopy);
    }

    #region Capability Related Validations
    /// <summary>
    /// Returns the first ResourceRequirement encountered that is using the specified Capability or null if none is found.
    /// </summary>
    internal ResourceRequirement GetFirstRequirementUsingCapability(Capability capability)
    {
        for (int i = 0; i < Count; i++)
        {
            Job job = this[i];
            for (int moI = 0; moI < job.ManufacturingOrders.Count; moI++)
            {
                ManufacturingOrder mo = job.ManufacturingOrders[moI];
                IEnumerator enumerator = mo.OperationManager.OperationsHash.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DictionaryEntry de = (DictionaryEntry)enumerator.Current;
                    BaseOperation op = (BaseOperation)de.Value;
                    if (op is InternalOperation)
                    {
                        InternalOperation iOp = (InternalOperation)op;
                        for (int rrI = 0; rrI < iOp.ResourceRequirements.Count; rrI++)
                        {
                            ResourceRequirement rr = iOp.ResourceRequirements.GetByIndex(rrI);
                            if (rr.CapabilityManager.Contains(capability.Id))
                            {
                                return rr;
                            }
                        }
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the first Resource Requirement for which the Resource is the only eligible Resource.
    /// Null is returned if no such Resource Requirement exists or if the Resource is In-Active.
    /// </summary>
    internal ResourceRequirement GetFirstRequirementNeedingResource(InternalResource a_resource, ProductRuleManager a_productRuleManager)
    {
        if (!a_resource.Active)
        {
            return null; //The Resource can't be "needed" as defined here if it's already In-Active.
        }

        Resource resource = (Resource)a_resource;

        for (int i = 0; i < Count; i++)
        {
            Job job = this[i];
            for (int moI = 0; moI < job.ManufacturingOrders.Count; moI++)
            {
                ManufacturingOrder mo = job.ManufacturingOrders[moI];
                IEnumerator enumerator = mo.OperationManager.OperationsHash.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DictionaryEntry de = (DictionaryEntry)enumerator.Current;
                    BaseOperation op = (BaseOperation)de.Value;
                    if (op is InternalOperation)
                    {
                        InternalOperation iOp = (InternalOperation)op;
                        for (int rrI = 0; rrI < iOp.ResourceRequirements.Count; rrI++)
                        {
                            ResourceRequirement rr = iOp.ResourceRequirements.GetByIndex(rrI);
                            if (rr.DefaultResource == resource)
                            {
                                return rr;
                            }

                            EligibleResourceSet resSet = rr.GetEligibleResourcesForPlant(resource.Department.Plant.Id);
                            if (resSet != null && resSet.Count > 0)
                            {
                                int eligibleActiveResourceCount = 0;
                                for (int n = 0; n < resSet.Count; n++)
                                {
                                    InternalResource internalResource = resSet[n];
                                    if (internalResource.Active)
                                    {
                                        eligibleActiveResourceCount++;
                                    }
                                }

                                if (eligibleActiveResourceCount == 1 && rr.IsEligible(resource, a_productRuleManager)) //Then this is the only Active eligible Resource.
                                {
                                    return rr;
                                }
                            }
                        }
                    }
                }
            }
        }

        return null;
    }
    #endregion

    private int m_nextExternalIdNbr = 1;

    /// <summary>
    /// Returns the next unique ExternalId and increments the counter
    /// </summary>
    /// <returns></returns>
    public string NextExternalId()
    {
        int nextId = m_nextExternalIdNbr;
        while (GetByExternalId(ExternalBaseIdObject.MakeExternalId(nextId)) != null)
        {
            nextId++;
        }

        m_nextExternalIdNbr = nextId + 1;
        return ExternalBaseIdObject.MakeExternalId(nextId);
    }

    /// <summary>
    /// Initializes the transmission flags of every job. This needs to be called before processing a JotT.
    /// </summary>
    internal void InitJobTFlags()
    {
        for (int i = 0; i < Count; ++i)
        {
            Job job = this[i];

            if (!job.Cancelled && !job.Finished)
            {
                job.InitJobTFlags();
            }
        }
    }

    /// <summary>
    /// Perform appropriate actions (such as unscheduling) on jobs that were marked during reception of a JobT.
    /// </summary>
    internal bool HandleMarkedJobTJobs()
    {
        bool mosUnscheduled = false;
        for (int i = 0; i < Count; ++i)
        {
            Job job = this[i];
            mosUnscheduled |= job.HandleJobTJobMarks();
        }

        return mosUnscheduled;
    }

    public void Receive(JobBaseT a_t, IScenarioDataChanges a_dataChanges, ScenarioDetail a_sd)
    {
        Job j;
        if (a_t is JobDefaultT)
        {
            j = AddDefault((JobDefaultT)a_t);
            ProcessJobAddedDataChanges(j, a_dataChanges);
        }
        else if (a_t is JobRequestNewExternalIdT)
        {
            string newExternalId = NextExternalId();
            using (m_scenario.ScenarioEventsLock.EnterRead(out ScenarioEvents se))
            {
                se.FireNewJobExternalIdEvent(newExternalId, (JobRequestNewExternalIdT)a_t);
            }
        }
        else if (a_t is JobCopyT jobCopyT)
        {
            ProcessJobCopy(jobCopyT, a_dataChanges, a_sd);
        }
        else if (a_t is JobDeleteAllT)
        {
            IEnumerable<Job> jobs = GetAllJobs();
            DeleteJobs(jobs, a_t, true, a_dataChanges);
        }
        else if (a_t is ScheduleCommitT)
        {
            CommitSchedule(((ScheduleCommitT)a_t).Resources, true, m_scenarioDetail.ClockDate);
            using (m_scenario.ScenarioEventsLock.EnterRead(out ScenarioEvents se))
            {
                se.FireBlocksChangedEvent();
            }
        }
        else if (a_t is ScheduleClearCommitT)
        {
            CommitSchedule(((ScheduleClearCommitT)a_t).resources, false, m_scenarioDetail.ClockDate);
            using (m_scenario.ScenarioEventsLock.EnterRead(out ScenarioEvents se))
            {
                se.FireBlocksChangedEvent();
            }
        }
        else if (a_t is JobIdBaseT jobBaseT)
        {
            j = ValidateExistence(jobBaseT.jobId);
            if (jobBaseT is InternalActivityFinishT)
            {
                ResetERPStatusUpdateVariables();
                j.Receive(jobBaseT, this, ScenarioDetail.ProductRuleManager, a_dataChanges);
                //Status updates cause a constraint change adjustment. This refreshes the gantt with correct statuses and block lengths.
                //This reverts a previous workaround made for AZ where status updates would not cause a simulation.
                a_dataChanges.FlagConstraintChanges(j.Id);
            }
            else
            {
                j.Receive(jobBaseT, this, ScenarioDetail.ProductRuleManager, a_dataChanges);

                if (!j.Template)
                {
                    using (m_scenario.ScenarioEventsLock.EnterRead(out ScenarioEvents se))
                    {
                        se.FireBlocksChangedEvent();
                    }
                }
            }
        }
        else if (a_t is JobDeleteJobsT)
        {
            DeleteJobs((JobDeleteJobsT)a_t, a_dataChanges);
        }
        else if (a_t is JobsPrintedT)
        {
            JobsPrinted((JobsPrintedT)a_t, a_dataChanges);
        }
        else if (a_t is JobGenerateT)
        {
            JobGenerateT jgt = (JobGenerateT)a_t;

            // For a given transmission this will always produce the same set of random numbers.
            // So, recordings will produce the same results unless there is a change in Generate function or 
            // Random is updated or works differently on different computers.
            Random r = new ((int)a_t.TimeStamp.Ticks);
            int jobValueStart = jgt.JobStartValue;

            for (int i = 0; i < jgt.JobsToCopy.Count; i++)
            {
                string jobExternalId = jgt.JobsToCopy[i];
                GenerateJobs(jobExternalId, jgt, r, a_dataChanges, ref jobValueStart);
            }
        }

        m_scenarioDetail.SignalJobBaseT();
    }

    internal void Receive(MaterialIdBaseT a_t, WarehouseManager a_warehouses, IScenarioDataChanges a_dataChanges)
    {
        if (a_t is ScenarioDetailMaterialUpdateT materialUpdateT)
        {
            foreach ((BaseId jobId, BaseId moId, BaseId opId, BaseId materialId) in materialUpdateT.GetIdsList())
            {
                Job job = GetById(jobId);
                ManufacturingOrder mo = job.ManufacturingOrders.GetById(moId);
                InternalOperation operation = (InternalOperation)mo.OperationManager[opId];
                MaterialRequirement requirement = operation.MaterialRequirements.FindByBaseId(materialId);

                if (requirement != null)
                {
                    AuditEntry mrAuditEntry = new AuditEntry(requirement.Id, operation.Id, requirement);
                    if (materialUpdateT.IssueFromInventory)
                    {
                        a_warehouses.IssueMaterial(a_t, requirement, a_dataChanges);
                    }
                    else
                    {
                        requirement.IssuedQty += ScenarioDetail.ScenarioOptions.RoundQty(materialUpdateT.IssuedQty);
                    }
                    a_dataChanges.AuditEntry(mrAuditEntry);
                    a_dataChanges.FlagProductionChanges(job.Id);
                }
            }
        }
    }

    internal void Receive(ScenarioDetail a_sd, MaterialEditT a_t, IScenarioDataChanges a_dataChanges)
    {

        foreach (MaterialEdit materialEdit in a_t)
        {
            try
            {
                Job job = GetById(materialEdit.JobId);
                ManufacturingOrder mo = job.ManufacturingOrders.GetById(materialEdit.MOId);
                InternalOperation operation = (InternalOperation)mo.OperationManager[materialEdit.OperationId];
                MaterialRequirement requirement = operation.MaterialRequirements.FindByBaseId(materialEdit.RequirementId);
                
                AuditEntry mrAuditEntry = new AuditEntry(requirement.Id, operation.Id, requirement);
                a_sd.WarehouseManager.IssueMaterial(this, materialEdit, a_dataChanges);

                requirement.Edit(a_sd, materialEdit, a_dataChanges);

                ProcessJobUpdatedDataChanges(job.Id, job.Template, a_dataChanges);
                a_dataChanges.AuditEntry(mrAuditEntry);
            }
            catch (Exception e)
            {
                m_errorReporter.LogException(new PTHandleableException(String.Format("Job ID {0} not found by JobManager in MaterialEdit {1}", materialEdit.JobId, materialEdit.MaterialName), e), null);
            }
        }
    }

    internal void Receive(ScenarioDetail a_sd, JobEditT a_t, IScenarioDataChanges a_dataChanges)
    {
        bool updated = false;
        foreach (JobEdit jobEdit in a_t.JobEdits)
        {
            Job job = GetById(jobEdit.JobId);
            if (job == null)
            {
                throw new PTValidationException("2706", new object[] { jobEdit.JobId });
            }

            AuditEntry jobAuditEntry = new AuditEntry(job.Id, job);
            updated = job.Edit(jobEdit, a_dataChanges);

            if (updated)
            {
                ProcessJobUpdatedDataChanges(job.Id, job.Template, a_dataChanges);
                a_dataChanges.AuditEntry(jobAuditEntry);
            }
        }

        foreach (ManufacturingOrderEdit moEdit in a_t.MOEdits)
        {
            Job job = GetById(moEdit.JobId);
            if (job == null)
            {
                throw new PTValidationException("2706", new object[] { moEdit.JobId });
            }

            ManufacturingOrder mo = job.ManufacturingOrders.GetById(moEdit.MOId);
            if (mo == null)
            {
                throw new PTValidationException("2708", new object[] { moEdit.JobId, moEdit.MOId });
            }

            AuditEntry moAuditEntry = new AuditEntry(mo.Id, job.Id, mo);
            updated = mo.Edit(a_sd, moEdit, a_dataChanges);

            if (updated)
            {
                ProcessJobUpdatedDataChanges(mo.Job.Id, mo.Job.Template, a_dataChanges);
                a_dataChanges.AuditEntry(moAuditEntry);
            }
        }

        foreach (OperationEdit opEdit in a_t.OpEdits)
        {
            Job job = GetById(opEdit.JobId);
            if (job == null)
            {
                throw new PTValidationException("2706", new object[] { opEdit.JobId });
            }

            ManufacturingOrder mo = job.ManufacturingOrders.GetById(opEdit.MOId);
            if (mo == null)
            {
                throw new PTValidationException("2708", new object[] { opEdit.JobId, opEdit.MOId });
            }

            InternalOperation op = (InternalOperation)mo.OperationManager[opEdit.OpId];
            if (op == null)
            {
                throw new PTValidationException("3137", new object[] { opEdit.JobId, opEdit.MOId, opEdit.OpId });
            }

            AuditEntry operationAudit = new AuditEntry(op.Id, op.ManufacturingOrder.Id, op);
            updated = op.Edit(a_sd, opEdit, a_dataChanges);

            if (updated)
            {
                ProcessJobUpdatedDataChanges(op.ManufacturingOrder.Job.Id, op.ManufacturingOrder.Job.Template, a_dataChanges);
                a_dataChanges.AuditEntry(operationAudit);
            }
        }

        foreach (ActivityEdit actEdit in a_t.ActivityEdits)
        {
            Job job = GetById(actEdit.JobId);
            if (job == null)
            {
                throw new PTValidationException("2706", new object[] { actEdit.JobId });
            }

            ManufacturingOrder mo = job.ManufacturingOrders.GetById(actEdit.MOId);
            if (mo == null)
            {
                throw new PTValidationException("2708", new object[] { actEdit.JobId, actEdit.MOId });
            }

            InternalOperation op = (InternalOperation)mo.OperationManager[actEdit.OpId];
            if (op == null)
            {
                throw new PTValidationException("3137", new object[] { actEdit.JobId, actEdit.MOId, actEdit.OpId });
            }

            InternalActivity act = op.Activities[actEdit.ActivityId];
            if (act == null)
            {
                throw new PTValidationException("2709", new object[] { actEdit.JobId, actEdit.MOId, actEdit.OpId, actEdit.ActivityId });
            }

            AuditEntry actAuditEntry = new AuditEntry(act.Id, act.Operation.Id, act);
            updated = act.Edit(a_sd, actEdit, a_dataChanges);

            if (updated)
            {
                ProcessJobUpdatedDataChanges(job.Id, job.Template, a_dataChanges);
                a_dataChanges.AuditEntry(actAuditEntry);
            }
        }

        if (a_dataChanges.JobChanges.HasChanges)
        {
            foreach (BaseId jobId in a_dataChanges.JobChanges.Updated)
            {
                if (a_dataChanges.HasEligibilityChanges(jobId))
                {
                    Job job = GetById(jobId);
                    job.ComputeEligibilityAndUnscheduleIfIneligible(a_sd.ProductRuleManager);
                    job.UpdateScheduledStatus(); //need to call this in case activities were finished
                    job.CalculateJitTimes(ScenarioDetail.Clock, false); //dates and structures may have changed.
                }

                //Make sure we update the schedule and refresh grids
                a_dataChanges.FlagVisualChanges(jobId);
            }
        }
    }

    /// <summary>
    /// Validates and copies a Job. Changes settings as specified in the transmission
    /// </summary>
    private void ProcessJobCopy(JobCopyT a_copyT, IScenarioDataChanges a_dataChanges, ScenarioDetail a_sd)
    {
        ValidateCopy(a_copyT);
        Job sourceJob = GetById(a_copyT.JobIdToCopy);
        Job newJob = Copy(sourceJob);
        newJob.Template = false;

        newJob.NeedDateTime = a_copyT.NewNeedDate;
        newJob.Commitment = a_copyT.NewCommitment;
        newJob.Name = a_copyT.NewName;
        newJob.ExternalId = NextExternalId();
        newJob.MaintenanceMethod = JobDefs.EMaintenanceMethod.Manual;

        foreach (ManufacturingOrder newMO in newJob.ManufacturingOrders)
        {
            newMO.SetRequiredQty(a_sd.Clock, a_copyT.Quantity, a_sd.ProductRuleManager);
        }

        foreach (string customer in a_copyT.Customers)
        {
            Customer newCustomer = m_scenarioDetail.CustomerManager.GetByExternalId(customer);
            if (newCustomer != null)
            {
                newJob.Customers.Add(newCustomer);
            }
        }

        //Update Holds
        switch (a_copyT.HoldDateChangeType)
        {
            case JobCopyT.EHoldDateChangeType.Remove:
                List<BaseOperation> opList = newJob.GetOperations();
                foreach (BaseOperation op in opList)
                {
                    op.OnHold = false;
                }

                break;
            case JobCopyT.EHoldDateChangeType.AdjustBasedOnNeedDate:
                List<BaseOperation> opList2 = newJob.GetOperations();
                foreach (BaseOperation op in opList2)
                {
                    if (op.OnHold)
                    {
                        //Adjust the hold date based on the change in NeedDate
                        TimeSpan adjustedTime = newJob.NeedDateTime - sourceJob.NeedDateTime;
                        op.HoldUntil += adjustedTime;
                    }
                }

                break;
        }

        newJob.ComputeEligibility(a_sd.ProductRuleManager); //This is needed to allow copied jobs to schedule correctly.
        a_dataChanges.JobChanges.AddedObject(newJob.Id);
        Add(newJob);
    }

    protected override Job Add(Job a_job)
    {
        if (ScenarioDetail != null && !ScenarioDetail.Scenario.LicenseManager.VerifyDataLimit(Count + 1, "DataVolume"))
        {
            ScenarioExceptionInfo sei = new ScenarioExceptionInfo();
            sei.Create(ScenarioDetail.Scenario);
            m_errorReporter.LogException(new PTHandleableException("Job Data Volume Has been exceeded"), sei, false);

            return null;
        }

        return base.Add(a_job);
    }

    private void CommitSchedule(ResourceKeyList resources, bool commit, DateTime clock)
    {
        ResourceKeyList.Node node = resources.First;
        //create faster search of the resource ids
        Dictionary<BaseId, BaseId> resourcesDictionary = new ();
        while (node != null)
        {
            ResourceKey resourceKey = node.Data;
            resourcesDictionary.Add(resourceKey.Resource, resourceKey.Resource);
            node = node.Next;
        }

        //Commit all Scheduled Jobs            
        for (int i = 0; i < Count; i++)
        {
            Job job = this[i];
            if (job.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled)
            {
                job.Commit(commit, clock, resourcesDictionary);
            }
        }
    }

    //TODO: change to use customer IDs when jobs save customer by id instead of name
    /// <summary>
    /// Returns a list of jobs that have the specified customer
    /// </summary>
    /// <param name="a_customerId"></param>
    /// <returns></returns>
    internal List<Job> GetJobsForCustomer(BaseId a_customerId)
    {
        List<Job> customerJobs = new ();
        for (int jobI = 0; jobI < Count; jobI++)
        {
            Job currentJob = GetByIndex(jobI);
            if (currentJob.Customers.Contains(a_customerId))
            {
                ;
            }

            {
                customerJobs.Add(currentJob);
            }
        }

        return customerJobs;
    }

    /// <summary>
    /// Sets all jobs with a specific status to a specific commitment
    /// </summary>
    /// <param name="a_scheduledStatus"></param>
    /// <param name="a_commitment"></param>
    internal void SetjobCommitments(JobDefs.scheduledStatuses a_scheduledStatus, JobDefs.commitmentTypes a_commitment)
    {
        for (int jobI = 0; jobI < Count; jobI++)
        {
            Job currentJob = GetByIndex(jobI);
            if (currentJob.ScheduledStatus == a_scheduledStatus)
            {
                currentJob.Commitment = a_commitment;
            }
        }
    }

    /// <summary>
    /// Not for customer use.
    /// Create a set of jobs.
    /// For a given transmission this will always produce the same set of random numbers.
    /// So, recordings will produce the same results unless there is a change in Generate function or
    /// Random is updated or works differently on different computers.
    /// </summary>
    internal void GenerateJobs(string a_jobExternalId, JobGenerateT t, Random aR, IScenarioDataChanges a_dataChanges, ref int a_jobStartValue)
    {
        Job j = GetByExternalId(a_jobExternalId);

        if (j != null)
        {
            for (int i = 0; i < t.NbrOfCopiesPerJob; i++)
            {
                Job jCopy = Copy(j);
                jCopy.Name = string.IsNullOrEmpty(t.JobPrefixName) ? a_jobStartValue.ToString() : t.JobPrefixName + a_jobStartValue;

                if (jCopy.Template && t.ConvertTemplatesToJobs)
                {
                    jCopy.Template = false;
                }

                //Unschedule alternate paths. The above Copy function unschedules the job, but only the current path is unscheduled.
                //If there is more than one path, we need to unschedule all operations. Otherwise there can be serialization errors due to null references
                for (int moI = 0; moI < jCopy.ManufacturingOrderCount; moI++)
                {
                    long unused = 0;
                    ManufacturingOrder currentMo = jCopy.ManufacturingOrders[moI];
                    if (currentMo.AlternatePaths.Count > 1)
                    {
                        //There is more than one path. Unschedule extra paths.
                        for (int pathI = 0; pathI < currentMo.AlternatePaths.Count; pathI++)
                        {
                            if (currentMo.AlternatePaths[pathI] != currentMo.CurrentPath) //current path has already been unscheduled.
                            {
                                using IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = currentMo.AlternatePaths[pathI].AlternateNodeSortedList.GetEnumerator();

                                while (alternateNodesEnumerator.MoveNext())
                                {
                                    AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
                                    InternalOperation baseOperation = node.Operation;
                                    baseOperation.Unschedule();
                                }
                            }
                        }
                    }
                }

                if (t.randomizeNeedDate)
                {
                    DateTime origninalJobNeedDate = j.NeedDateTime;
                    DateTime newNeedDateTime = new (RandomHelper.GetNext(aR, t.MinNeedDate.Ticks, t.MaxNeedDate.Ticks + 1));
                    newNeedDateTime = DateTimeHelper.RoundToNearestHour(newNeedDateTime);
                    jCopy.NeedDateTime = newNeedDateTime;

                    //Reset Buy-direct material available dates
                    for (int mI = 0; mI < jCopy.ManufacturingOrders.Count; ++mI)
                    {
                        ManufacturingOrder mo = jCopy.ManufacturingOrders[mI];

                        IEnumerator enumerator = mo.OperationManager.OperationsHash.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            DictionaryEntry de = (DictionaryEntry)enumerator.Current;
                            BaseOperation op = (BaseOperation)de.Value;
                            for (int matI = 0; matI < op.MaterialRequirements.Count; matI++)
                            {
                                MaterialRequirement material = op.MaterialRequirements[matI];
                                if (material.BuyDirect)
                                {
                                    long dateDif = origninalJobNeedDate.Ticks - material.LatestSourceDate;
                                    material.LatestSourceDate = jCopy.NeedDateTime.Ticks - dateDif;
                                }
                            }
                        }
                    }
                }

                if (t.randomizePriority)
                {
                    jCopy.Priority = aR.Next(t.MinPriority, t.MaxPriority + 1);
                }

                if (t.randomizeCommitment)
                {
                    int rnd = aR.Next(1, 100);
                    if (rnd > 30)
                    {
                        jCopy.Commitment = JobDefs.commitmentTypes.Planned;
                    }
                    else
                    {
                        jCopy.Commitment = JobDefs.commitmentTypes.Firm;
                    }
                }

                if (t.randomizeCustomer && t.customers.Length > 0)
                {
                    int custIdx = aR.Next(0, t.customers.Length);
                    Customer customer = m_scenarioDetail.CustomerManager.GetByExternalId(t.customers.GetValue(custIdx).ToString());
                    jCopy.AddCustomer(customer);
                }

                if (t.randomizeProductNamesAndColors && t.productNames.Length > 0)
                {
                    int prodIdx = aR.Next(0, t.productNames.Length);
                    string productName = t.productNames.GetValue(prodIdx).ToString();
                    Color color = (Color)t.colors.GetValue(prodIdx);
                    jCopy.ColorCode = color;
                    string colorName;
                    if (!ColorUtils.GetKnownColor(color, out colorName))
                    {
                        colorName = color.Name;
                    }

                    for (int moI = 0; moI < jCopy.ManufacturingOrders.Count; moI++)
                    {
                        ManufacturingOrder mo = jCopy.ManufacturingOrders[moI];
                        mo.ProductName = productName;
                        mo.ProductColor = color;
                    }
                }

                if (t.randomizeRevenue)
                {
                    decimal newRevenue = RandomHelper.GetNext(aR, t.MinRevenue, t.MaxRevenue);
                    newRevenue = Math.Round(newRevenue, 2, MidpointRounding.AwayFromZero);
                    if (newRevenue < 0)
                    {
                        newRevenue = 0;
                    }

                    jCopy.Revenue = newRevenue;
                }

                if (t.randomizeSetupHours || t.randomizeMinutesPerCycle)
                {
                    for (int mI = 0; mI < jCopy.ManufacturingOrders.Count; ++mI)
                    {
                        ManufacturingOrder mo = jCopy.ManufacturingOrders[mI];
                        IEnumerator enumerator = mo.OperationManager.OperationsHash.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            DictionaryEntry de = (DictionaryEntry)enumerator.Current;
                            BaseOperation op = (BaseOperation)de.Value;
                            if (op is InternalOperation)
                            {
                                InternalOperation iOp = (InternalOperation)op;
                                if (t.randomizeSetupHours)
                                {
                                    decimal newSetupHours = RandomHelper.GetNext(aR, t.MinSetupHours, t.MaxSetupHours);
                                    newSetupHours = Math.Round(newSetupHours, 1, MidpointRounding.AwayFromZero);

                                    if (newSetupHours <= 0)
                                    {
                                        newSetupHours = 0;
                                    }

                                    iOp.SetupSpan = TimeSpan.FromHours((double)newSetupHours);
                                }

                                if (t.randomizeSetupNumber)
                                {
                                    decimal newSetupNumber = RandomHelper.GetNext(aR, t.MinSetupNumber, t.MaxSetupNumber);
                                    newSetupNumber = Math.Round(newSetupNumber, 1, MidpointRounding.AwayFromZero);

                                    if (newSetupNumber <= 0)
                                    {
                                        newSetupNumber = 0;
                                    }

                                    iOp.SetupNumber = newSetupNumber;
                                }

                                if (t.randomizeMinutesPerCycle)
                                {
                                    decimal newMinutesPerCycle = RandomHelper.GetNext(aR, t.MinMinutesPerCycle, t.MaxMinutesPerCycle);
                                    newMinutesPerCycle = Math.Round(newMinutesPerCycle, 1, MidpointRounding.AwayFromZero);

                                    if (newMinutesPerCycle <= 0)
                                    {
                                        newMinutesPerCycle = 1;
                                    }

                                    iOp.CycleSpan = TimeSpan.FromMinutes((double)newMinutesPerCycle);
                                }
                            }
                        }
                    }
                }

                if (t.randomizeRequiredQty)
                {
                    for (int mI = 0; mI < jCopy.ManufacturingOrders.Count; ++mI)
                    {
                        decimal newRequiredQty = RandomHelper.GetNext(aR, t.MinReqQty, t.MaxReqQty + 1);
                        newRequiredQty = Math.Round(newRequiredQty, 0, MidpointRounding.AwayFromZero);
                        if (newRequiredQty <= 0)
                        {
                            newRequiredQty = 1;
                        }

                        ManufacturingOrder mo = jCopy.ManufacturingOrders[mI];
                        decimal changeRatio = newRequiredQty / mo.RequiredQty;
                        mo.SetRequiredQty(ScenarioDetail.Clock, newRequiredQty, ScenarioDetail.ProductRuleManager);
                        mo.RequestedQty = mo.RequestedQty * changeRatio;
                        mo.ExpectedFinishQty = mo.ExpectedFinishQty * changeRatio;
                    }
                }

                if (t.randomizeColor)
                {
                    List<Color> colorList = ColorUtils.GetAllColors();
                    jCopy.ColorCode = colorList[aR.Next(colorList.Count - 1)];
                }

                jCopy.InitGeneratedJob(ScenarioDetail.Clock, ScenarioDetail.ProductRuleManager);

                Add(jCopy);
                ProcessJobAddedDataChanges(jCopy, a_dataChanges);
                jCopy.MaintenanceMethod = JobDefs.EMaintenanceMethod.JobGenerator;
                RecordHistoryNewJob(jCopy);
                a_jobStartValue++;
            }
        }
    }

    /// <summary>
    /// Copies the job Restores references, Sets Next ID, Creates a default Name and external id, and calls unschedule.
    /// </summary>
    /// <param name="j"></param>
    /// <returns></returns>
    internal Job Copy(Job j)
    {
        Job jCopy = j.CreateUnitializedCopy();

        jCopy.Id = NextID();

        AfterRestoreReferences.Helpers.ResetContainedIds(Serialization.VersionNumber, jCopy);
        jCopy.RestoreReferences(ScenarioDetail.CustomerManager, ScenarioDetail.PlantManager, ScenarioDetail.CapabilityManager, ScenarioDetail, ScenarioDetail.WarehouseManager, ScenarioDetail.ItemManager, m_errorReporter);
        jCopy.RestoreReferences2();

        jCopy.ExternalId = MakeDefaultName(j.ExternalId.TrimEnd(null), Simulation.Scheduler.Job.SplitHelpers.SPLIT_MO_SEPERATOR_CHARS, jCopy.Id.Value);
        jCopy.Name = MakeDefaultName(j.Name.TrimEnd(null), Simulation.Scheduler.Job.SplitHelpers.SPLIT_MO_SEPERATOR_CHARS, jCopy.Id.Value);

        jCopy.ManufacturingOrders.ResetAllMOExternalIdAndNames();

        jCopy.Unschedule();

        return jCopy;
    }

    internal void AddNewJob(Job job)
    {
        job.ComputeEligibility(ScenarioDetail.ProductRuleManager);
        base.Add(job);
        RecordHistoryNewJob(job);
    }

    public void Receive(InternalActivityUpdateT a_t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        ApplicationExceptionList errList = new ();
        List<PostProcessingAction> actions = new ();
        HashSet<string> badExternalIdsHash = new ();

        try
        {
            InitFastLookupByExternalId();

            for (int i = 0; i < a_t.Count; i++)
            {
                InternalActivityUpdateT.ActivityStatusUpdate update = a_t[i];
                Job job = GetByExternalId(update.JobExternalId);
                if (job != null)
                {
                    try //catch errors on individual job updates so we can process the valid ones.
                    {
                        job.Receive(a_sd._scenario, update, a_dataChanges);
                    }
                    catch (PTHandleableException e)
                    {
                        errList.Add(new PTValidationException(string.Format("Error processing InternalActivityUpdateT for Job {0}.".Localize(), job.ExternalId), e));
                    }
                }
                else
                {
                    //Store a list of bad externalids to log
                    if (!badExternalIdsHash.Contains(update.JobExternalId))
                    {
                        badExternalIdsHash.Add(update.JobExternalId);
                    }
                }
            }
        }
        finally
        {
            actions.Add(new PostProcessingAction(a_t, true, () => { DeInitFastLookupByExternalId(); }));
            a_sd.AddProcessingAction(actions);

            if (errList.Count > 0)
            {
                ScenarioExceptionInfo sei = new ();
                sei.Create(a_sd);
                m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
            }

            if (badExternalIdsHash.Count > 0)
            {
                ScenarioExceptionInfo sei = new ();
                sei.Create(a_sd);
                System.Text.StringBuilder badIds = new ();
                foreach (string badId in badExternalIdsHash)
                {
                    if (badIds.Length > 0)
                    {
                        badIds.Append(", " + badId);
                    }
                    else
                    {
                        badIds.Append(badId);
                    }
                }

                PTValidationException e = new ("2205", new object[] { a_sd._scenario.Id, badIds.ToString() });
                m_errorReporter.LogException(e, a_t, sei, ELogClassification.PtSystem, false);
            }

            ScenarioEvents se;
            using (m_scenario.ScenarioEventsLock.EnterRead(out se))
            {
                se.FireBlocksChangedEvent();
            }
        }
    }

    public void Receive(JobT jobT, UserFieldDefinitionManager a_udfManager, CapabilityManager machineCapabilities, ScenarioDetail scenarioDetail, IScenarioDataChanges a_dataChanges, ScenarioExceptionInfo a_sei)
    {
        if (jobT == null)
        {
            return;
        }

        ApplicationExceptionList errList = new ();
        List<PostProcessingAction> actions = new ();

        try
        {
            jobT.Validate();

            //TODO: Return to Parallel.Invoke
            //Temp solution until V12 datachages is in place to reduce number of threads
            Thread t1 = new (() => { scenarioDetail.ItemManager.InitFastLookupByExternalId(); });
            t1.Start();

            Thread t2 = new (() => { scenarioDetail.CapabilityManager.InitFastLookupByExternalId(); });
            t2.Start();

            Thread t3 = new (() => { scenarioDetail.CustomerManager.InitFastLookupByExternalId(); });
            t3.Start();

            Thread t4 = new (() => { InitFastLookupByExternalId(); });
            t4.Start();

            t1.Join();
            t2.Join();
            t3.Join();
            t4.Join();

            InitJobTFlags();
            ResetERPStatusUpdateVariables();

            int failedJobsCount = 0;

            for (int i = 0; i < jobT.Count; ++i)
            {
                try
                {
                    ProcessJobT(jobT[i], jobT.TimeStamp, a_udfManager, machineCapabilities, scenarioDetail, a_dataChanges, jobT.Instigator, jobT);
                }
                catch (PTHandleableException err)
                {
                    failedJobsCount++;
                    errList.Add(err);
                }
            }

            ScenarioEvents se;
            if (failedJobsCount > 0)
            {
                //Notify the user of the errors
                using (m_scenario.ScenarioEventsLock.EnterRead(out se))
                {
                    string msg = Localizer.GetErrorString("2206", new object[] { failedJobsCount, jobT.Count });
                    PTValidationException e = new ("2207");
                    se.FireTransmissionFailureEvent(e, jobT, new SystemMessage(jobT.Instigator, msg));
                }
            }

            if (jobT.AutoDeleteMode)
            {
                HashSet<string> jobExternalIds = new ();

                for (int jobTI = 0; jobTI < jobT.Count; ++jobTI)
                {
                    string externalId = jobT[jobTI].ExternalId;
                    jobExternalIds.Add(externalId);
                }

                List<Job> deleteJobs = new ();
                for (int jobI = Count - 1; jobI >= 0; --jobI)
                {
                    Job job = this[jobI];

                    bool jobIsActual = false;

                    if (scenarioDetail.ScenarioOptions.PreventAutoDeleteActuals)
                    {
                        jobIsActual = job.CalculateIsActual();
                    }

                    string externalId = job.ExternalId;
                    if (!jobExternalIds.Contains(externalId) &&
                        (job.MaintenanceMethod != JobDefs.EMaintenanceMethod.Manual || job.Finished) &&
                        job.MaintenanceMethod != JobDefs.EMaintenanceMethod.JobGenerator &&
                        job.MaintenanceMethod != JobDefs.EMaintenanceMethod.MrpGenerated
                        //&& !job.SplitOffFromOtherJob //Removed this condition because split job's MaintenanceMethod is set to Manual. Once imported they are now managed by data import and should be auto deleted.
                        &&
                        !job.DoNotDelete
                        &&
                        !jobIsActual)
                    {
                        try
                        {
                            ValidForERPDelete(scenarioDetail, job);
                            deleteJobs.Add(job);
                            a_dataChanges.JobChanges.DeletedObject(job.Id);
                        }
                        catch (PTValidationException err)
                        {
                            errList.Add(err);
                        }
                    }
                }

                if (deleteJobs.Count > 0)
                {
                    actions.Add(new PostProcessingAction(jobT, false, () => { DeleteJobs(deleteJobs, jobT, false, a_dataChanges); }));
                }
            }

            scenarioDetail.SignalJobBaseT();
            InternalActivityList newlyInProcessInternalActivities = GetInprocessActivities(true, false, false);
            if (newlyInProcessInternalActivities.Count > 0)
            {
                a_dataChanges.FlagConstraintChanges(BaseId.NULL_ID);
            }

            //If we get here with no errors that let the UI know that the JobT succeeded
            if (failedJobsCount == 0 && jobT.TransmissionSender == PTTransmissionBase.TransmissionSenderType.PTUser)
            {
                using (m_scenario.ScenarioEventsLock.EnterRead(out se))
                {
                    se.FireJobTSucceededEvent(jobT);
                }
            }

            //Validate Successor MOs for jobs that have been imported as well as successor MOs that were imported for jobs that don't exist.
            ValidateSuccessorMOs(jobT, scenarioDetail, a_dataChanges);
        }
        catch (PTHandleableException err)
        {
            errList.Add(err);
        }
        finally
        {
            actions.Add(new PostProcessingAction(jobT, true, () =>
                {
                    scenarioDetail.ItemManager.DeInitFastLookupByExternalId();
                    scenarioDetail.CapabilityManager.DeInitFastLookupByExternalId();
                    scenarioDetail.CustomerManager.DeInitFastLookupByExternalId();
                    DeInitFastLookupByExternalId();
                }));
            ScenarioDetail.AddProcessingAction(actions);

            if (errList.Count > 0)
            {
                m_errorReporter.LogException(errList, jobT, a_sei, ELogClassification.PtInterface, false);
            }
        }
    }

    /// <summary>
    /// Validates that all imported Successor MO relations are added to valid jobs and that their successor job, Mo, Path exists.
    /// Throws a PTValidation exception if the validation fails.
    /// </summary>
    private void ValidateSuccessorMOs(JobT jobT, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        //For every job being imported
        for (int jobI = 0; jobI < jobT.Count; jobI++)
        {
            //Find the job in the system
            Job currentJob = GetByExternalId(jobT[jobI].ExternalId);
            if (currentJob == null)
            {
                continue;
            }

            //For each MO in the job
            for (int moI = 0; moI < currentJob.ManufacturingOrderCount; moI++)
            {
                //Get currentMO of currentJob
                ManufacturingOrder currentMO = currentJob.ManufacturingOrders[moI];

                //Get its successor MOs
                SuccessorMOArrayList succMoList = currentMO.SuccessorMOs;

                //Validate(update) successor Mos
                SuccessorMOArrayList validatedSuccMoList = ValidateSuccMoList(succMoList, currentJob, a_sd);
                succMoList.Update(currentMO, validatedSuccMoList, a_sd, a_dataChanges);
            }
        }

        //Now check that every Successor MO imported exists in a job. An error will be thrown if one was imported for a job Mo that doens't exist.
        //For every job imported in the JobT
        for (int JobI = 0; JobI < jobT.Nodes.Count; JobI++)
        {
            //For each MO in that job
            for (int moI = 0; moI < jobT.Nodes[JobI].ManufacturingOrderCount; moI++)
            {
                JobT.ManufacturingOrder mo = jobT.Nodes[JobI][moI];

                //For every successor MO in that MO
                for (int succI = 0; succI < mo.SuccessorMOs.Count; succI++)
                {
                    //Store the imported ExternalIds
                    string jobExtId = mo.SuccessorMOs[succI].SuccessorJobExternalId;
                    string moExtId = mo.SuccessorMOs[succI].SuccessorManufacturingOrderExternalId;

                    //Get the systemJob matching the imported ExternalId
                    Job matchingJob = GetByExternalId(jobExtId);
                    if (matchingJob != null)
                    {
                        //The job exists. Get the MO with the ExternalId of the successorMo imported
                        ManufacturingOrder matchingMO = matchingJob.ManufacturingOrders.GetByExternalId(moExtId);
                        if (matchingMO != null)
                        {
                            //a match has been found. This imported Successor MO exists
                            continue;
                        }
                    }

                    throw new PTValidationException("2842", new object[] { jobExtId, moExtId });
                }
            }
        }
    }

    /// <summary>
    /// Returns list of successor mos with valid SuccessorJobExternalId jobs reference
    /// </summary>
    /// <param name="a_succMoList"></param>
    /// <param name="a_currentJob"></param>
    /// <param name="a_sd"></param>
    /// <param name="jobsList"></param>
    /// <returns></returns>
    private SuccessorMOArrayList ValidateSuccMoList(SuccessorMOArrayList a_succMoList, Job a_currentJob, ScenarioDetail a_sd)
    {
        SuccessorMOArrayList newSuccMoList = new();
        for (int succI = 0; succI < a_succMoList.Count; succI++)
        {
            SuccessorMO succMo = a_succMoList[succI];
            string succJobExternalId = succMo.SuccessorJobExternalId;

            //verify successor mo's Job exists
            Job job = GetByExternalId(succJobExternalId);
            if (job != null)
            {
                string succMoExternalId = succMo.SuccessorManufacturingOrderExternalId;
                //verify this job's successor mo's MO exists
                ManufacturingOrderManager moMgr = job.ManufacturingOrders;
                ManufacturingOrder mo = moMgr.GetByExternalId(succMoExternalId);
                if (mo != null)
                {
                    //if successor mo has Alt.Path verify it matches MO's Alt.Path
                    string succMoPathExternalId = succMo.AlternatePathExternalId;
                    if (!string.IsNullOrWhiteSpace(succMoPathExternalId))
                    {
                        AlternatePathsCollection moAltPaths = mo.AlternatePaths;
                        AlternatePath successorMoPath = moAltPaths.FindByExternalId(succMoPathExternalId);
                        if (successorMoPath == null)
                        {
                            throw new ValidationException("2086", new object[] { a_currentJob.ExternalId, "Successor M.O. not imported.".Localize(), $"Invalid AlternatePathExternalId: MO '{succMoExternalId}' does not Contain Path '{succMoPathExternalId}'.".Localize() });
                        }

                        //Path found, now check that OperationExternalId exists in that path (if specified)
                        string succOpExternalId = succMo.OperationExternalId;
                        if (!string.IsNullOrWhiteSpace(succMoPathExternalId))
                        {
                            if (!successorMoPath.ContainsOperation(succOpExternalId))
                            {
                                throw new ValidationException("2086", new object[] { a_currentJob.ExternalId, "Successor M.O. not imported.".Localize(), $"Invalid OperationExternalId: Path '{succMoPathExternalId}' on MO '{succMoExternalId}' does not Contain Operation '{succOpExternalId}'.".Localize() });
                            }
                        }
                    }

                    newSuccMoList.Add(succMo, a_sd);
                }
                else
                {
                    throw new ValidationException("2086", new object[] { a_currentJob.ExternalId, "Successor M.O. not imported.".Localize(), $"Invalid SuccessorManufacturingOrderExternalId: MO '{succMoExternalId}' does not exist.".Localize() });
                }
            }
            else
            {
                throw new ValidationException("2086", new object[] { a_currentJob.ExternalId, "Successor M.O. not imported.".Localize(), $"Invalid SuccessorJobExternalId - Job '{succJobExternalId}' does not exist.".Localize() });
            }
        }

        return newSuccMoList;
    }

    private void ValidForERPDelete(ScenarioDetail sd, Job job)
    {
        if (sd.ScenarioOptions.PreventErpJobCancelIfStarted && job.Started)
        {
            throw new PTValidationException("2208", new object[] { job.Name });
        }

        if (sd.ScenarioOptions.PreventErpJobCancelInStableSpan && job.InStableSpan())
        {
            throw new PTValidationException("2209", new object[] { job.Name });
        }
    }

    private bool ValidForERPChanges(ScenarioDetail sd, Job job, JobT.Job jobTJob)
    {
        bool jobInStableSpan = job.InStableSpan();
        bool noChangeDueToStableSpan = sd.ScenarioOptions.PreventErpJobChangesInStableSpan && jobInStableSpan;
        bool noChangeDueToStarted = sd.ScenarioOptions.PreventErpJobChangesIfStarted && job.Started;

        return !noChangeDueToStableSpan && !noChangeDueToStarted;
    }

    /// <summary>
    /// Get the in-process activities.
    /// </summary>
    /// <param name="newInProcsOnly">Whether to retrieve only the activities that are newly in-process. The result of a change to a received JobT.</param>
    /// <param name="scheduledOnly">Only get the activities that are scheduled.</param>
    /// <param name="withMOSchedulableFlagSet">Only activities whose MO.Schedulable flag has been set.</param>
    /// <returns></returns>
    internal InternalActivityList GetInprocessActivities(bool newInProcsOnly, bool scheduledOnly, bool withMOSchedulableFlagSet)
    {
        InternalActivityList newlyStartedActivities = new ();

        for (int jobI = 0; jobI < Count; ++jobI)
        {
            Job job = this[jobI];

            if (job.ScheduledStatus == JobDefs.scheduledStatuses.Finished ||
                job.ScheduledStatus == JobDefs.scheduledStatuses.Cancelled)
            {
                continue;
            }

            for (int moI = 0; moI < job.ManufacturingOrders.Count; ++moI)
            {
                ManufacturingOrder mo = job.ManufacturingOrders[moI];

                if (withMOSchedulableFlagSet && !mo.Schedulable)
                {
                    continue;
                }

                // Scan the operations in the alternate path searching for those whose status has recently been changed to in process.
                // These are added to the moAL list as the MOs that are to be expidited.
                using IEnumerator<KeyValuePair<string, AlternatePath.Node>> pathEnum = mo.CurrentPath.AlternateNodeSortedList.GetEnumerator();

                while (pathEnum.MoveNext())
                {
                    AlternatePath.Node apn = pathEnum.Current.Value;
                    BaseOperation baseOp = apn.Operation;

                    if (baseOp is InternalOperation)
                    {
                        InternalOperation io = (InternalOperation)baseOp;
                        for (int activityI = 0; activityI < io.Activities.Count; ++activityI)
                        {
                            InternalActivity ia = io.Activities.GetByIndex(activityI);

                            if ((!newInProcsOnly && ia.InProduction()) || (newInProcsOnly && ia.ChangedToRunningStatus))
                            {
                                if (!scheduledOnly || (scheduledOnly && ia.Scheduled))
                                {
                                    newlyStartedActivities.Add(ia);
                                }
                            }
                        }
                    }
                }
            }
        }

        return newlyStartedActivities;
    }

    public class JobValidationException : PTValidationException
    {
        public JobValidationException(string a_message, Exception a_innerException, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(a_message, a_innerException, a_appendHelpUrl, a_stringParameters) { }
    }

    /// <summary>
    /// Creates Job from JobT.Job transmissions or if it already exists, it updates it.
    /// </summary>
    private void ProcessJobT(JobT.Job jobTJob, DateTimeOffset jobTTimeStamp, UserFieldDefinitionManager a_udfManager, CapabilityManager machineCapabilities, ScenarioDetail scenarioDetail, IScenarioDataChanges a_dataChanges, BaseId instigator, JobT t)
    {
        jobTJob.Validate();
        bool fromERP = t.FromErp;
        Job job = GetByExternalId(jobTJob.ExternalId);

        bool foundByOldExternalId = false;
        if (job == null && jobTJob.OldExternalIdSet) //see if there is a Job with the Old ExternalId.  This may be updating that Job to the new Job's values (including it's new ExternalId).
        {
            job = GetByExternalId(jobTJob.OldExternalId);
            if (job != null)
            {
                foundByOldExternalId = true;
                //Log this event in the History (KSM requested the ability to see the time since the job was first created
                string delay = string.Format("{0} days".Localize(), Math.Round(jobTTimeStamp.Subtract(job.EntryDate).TotalDays, 2).ToString());
                string description = string.Format("Replaced existing Job {0} with new Job {1} {2} days after initially entered.".Localize(), jobTJob.OldExternalId, jobTJob.ExternalId, delay);
                scenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(new[] { job.Id }, description, typeof(Job), ScenarioHistory.historyTypes.JobChanged);
                //This job is now being managed by data imports
                job.MaintenanceMethod = JobDefs.EMaintenanceMethod.ERP;
            }
        }

        List<Customer> customers = GetCustomers(t, jobTJob, scenarioDetail.CustomerManager);

        if (job == null)
        {
            try
            {
                job = new Job(fromERP, NextID(), jobTJob, jobTTimeStamp, machineCapabilities, scenarioDetail, instigator, t, a_dataChanges, m_errorReporter, a_udfManager);

                foreach (Customer customer in customers)
                {
                    job.AddCustomer(customer);
                }

                if (job.Name == "")
                {
                    job.Name = job.ExternalId; //want Names to be unique
                }

                if (fromERP)
                {
                    job.MaintenanceMethod = JobDefs.EMaintenanceMethod.ERP;
                }
                else
                {
                    job.MaintenanceMethod = JobDefs.EMaintenanceMethod.Manual;
                }

                Add(job);
                ProcessJobAddedDataChanges(job, a_dataChanges);
                RecordHistoryNewJob(job);
                job.FlagAffectedItemsForNetChangeMRP(scenarioDetail.WarehouseManager);
                job.ComputeEligibility(ScenarioDetail.ProductRuleManager);
                job.CalculateJitTimes(ScenarioDetail.Clock, false);
            }
            catch (PTValidationException err)
            {
                throw new JobValidationException("2856", err, new object[] { jobTJob.ExternalId, jobTJob.Name });
            }
        }
        else
        {
            try
            {
                if (!fromERP || ValidForERPChanges(scenarioDetail, job, jobTJob))
                {
                    AuditEntry jobAuditEntry = new AuditEntry(job.Id, job);
                    bool updated;
                    if (foundByOldExternalId)
                    {
                        string oldExternalId = jobTJob.OldExternalId;
                        string newExternalId = jobTJob.ExternalId;
                        updated = job.Update(a_udfManager, fromERP, jobTJob, scenarioDetail, t, a_dataChanges);

                        //If this manager has ExternalId collection cached, it should be informed of the update.
                        UpdateObjectExternalId(oldExternalId, newExternalId, job);
                    }
                    else
                    {
                        updated = job.Update(a_udfManager, fromERP, jobTJob, scenarioDetail, t, a_dataChanges);
                    }

                    if (fromERP && job.MaintenanceMethod != JobDefs.EMaintenanceMethod.ERP)
                    {
                        // This allows manually created jobs from splits to turn into ERP jobs once the ERP system has processed them.
                        job.MaintenanceMethod = JobDefs.EMaintenanceMethod.ERP;
                        updated = true;
                    }

                    if (t.IncludeCustomerAssociations)
                    {
                        if (customers.Count != job.Customers.Count)
                        {
                            updated = true;
                        }
                        else
                        {
                            updated = !customers.ContainsAll(job.Customers);
                        }

                        if (updated)
                        {

                            if (t.AutoDeleteCustomerAssociations)
                            {
                                job.ClearCustomers();
                            }

                            foreach (Customer customer in customers)
                            {
                                job.AddCustomer(customer);
                            }
                        }
                    }

                    if (updated)
                    {
                        a_dataChanges.AuditEntry(jobAuditEntry);
                        a_dataChanges.JobChanges.UpdatedObject(job.Id);
                        if (a_dataChanges.HasJitChanges(job.Id))
                        {
                            job.CalculateJitTimes(ScenarioDetail.Clock, false);
                        }
                    }
                }
            }
            catch (PTValidationException err)
            {
                throw new JobValidationException("2857", err, new object[] { job.ExternalId, job.Name });
            }
        }
    }

    private static List<Customer> GetCustomers(JobT a_t, JobT.Job a_jobTJob, CustomerManager a_customerManager)
    {
        List<Customer> capList = new ();

        for (int j = 0; j < a_jobTJob.Customers.Count; ++j)
        {
            string customerExternalId = a_jobTJob.Customers[j];
            Customer c = a_customerManager.GetByExternalId(customerExternalId);
            if (c == null)
            {
                throw new TransmissionValidationException(a_t, "3023", new object[] { a_jobTJob.ExternalId, customerExternalId });
            }

            capList.Add(c);
        }

        return capList;
    }

    private void DeleteJobs(JobDeleteJobsT t, IScenarioDataChanges a_dataChanges)
    {
        List<Job> jobList;

        try
        {
            jobList = GetJobs(t.Jobs);
        }
        catch (GetJobsException)
        {
            throw new PTValidationException("2210");
        }

        DeleteJobs(jobList, t, true, a_dataChanges);
    }

    private void JobsPrinted(JobsPrintedT t, IScenarioDataChanges a_dataChanges)
    {
        BaseIdList jobList = t.Jobs;

        foreach (BaseId jobId in jobList)
        {
            Job job = GetById(jobId);
            if (job != null && job.Printed != t.MarkedAsPrinted)
            {
                job.Printed = t.MarkedAsPrinted;
                if (t.MarkedAsPrinted)
                {
                    job.PrintedDate = t.TimeStamp.ToDateTime();
                }

                ProcessJobUpdatedDataChanges(job.Id, job.Template, a_dataChanges);
            }
        }
    }

    /// <summary>
    /// Delete every job. Use this instead of the clear function. It makes sure all the jobs are unscheduled before they are deleted.
    /// </summary>
    /// <param name="t"></param>
    internal void UnscheduleAllJobs()
    {
        for (int i = 0; i < Count; ++i)
        {
            this[i].Unschedule();
        }
    }

    /// <summary>
    /// Deletes ALL Jobs.
    /// </summary>
    public void Clear(IScenarioDataChanges a_dataChanges)
    {
        for (int i = 0; i < Count; i++)
        {
            Job job = this[i];
            job.DeletingJobOrMOs(a_dataChanges);
            job.Unschedule(true);
            ProcessJobDeletedDataChanges(job, a_dataChanges);
        }

        base.Clear();
    }

    /// <summary>
    /// Delete jobs specified in a list.
    /// </summary>
    /// <param name="job">The jobs to delete.</param>
    /// <param name="t">The transmission that is being processed.</param>
    /// <param name="performTimeAdjustment">
    /// Whether to perform a time ajustment. If this is set to false, then it is assumed that the called will perform some kind of simulation that will fix up the
    /// schedule.
    /// </param>
    internal void DeleteJob(Job job, ScenarioBaseT t, bool performTimeAdjustment, IScenarioDataChanges a_dataChanges)
    {
        DeleteJobs(new List<Job> { job }, t, performTimeAdjustment, a_dataChanges);
    }

    internal void DeleteJob(Job job, IScenarioDataChanges a_dataChanges)
    {
        DeleteJob(job, null, false, a_dataChanges);
    }

    /// <summary>
    /// Delete jobs specified in a list.
    /// </summary>
    /// <param name="jobList">The jobs to delete.</param>
    /// <param name="t">The transmission that is being processed.</param>
    /// <param name="performTimeAdjustment">
    /// <param name="a_dataChanges"></param>
    /// Whether to perform a time ajustment. If this is set to false, then it is assumed that the called will perform some kind of simulation that will fix up the
    /// schedule.
    /// </param>
    internal void DeleteJobs(IEnumerable<Job> jobList, ScenarioBaseT t, bool performTimeAdjustment, IScenarioDataChanges a_dataChanges)
    {
        bool changes = false;
        foreach (Job job in jobList)
        {
            if (!job.DoNotDelete) //Don't want to remove the users template jobs when autodeleting after an import.
            {
                job.Unschedule(true);
                if (job.CtpSalesOrder != null)
                {
                    ScenarioDetail.SalesOrderManager.Delete(job.CtpSalesOrder, m_scenarioDetail, t);
                }

                job.FlagAffectedItemsForNetChangeMRP(m_scenarioDetail.WarehouseManager);

                job.DeletingJobOrMOs(a_dataChanges);
                //Remove from collection
                base.Remove(job.Id);

                ProcessJobDeletedDataChanges(job, a_dataChanges);

                if (!job.Template)
                {
                    changes = true;
                }
            }
        }

        //Alert other objects that activities are being deleted
        //ScenarioDetail.PlantManager.DeletingActivities(jobActivities);

        if (changes)
        {
            if (performTimeAdjustment)
            {
                m_scenarioDetail.TimeAdjustment(t);
            }
        }
    }

    internal void DeletingDemand(BaseIdObject a_demand, PTTransmissionBase a_t, BaseIdList a_distributionsToDelete = null)
    {
        try
        {
            Parallel.ForEach(this,
                j =>
                {
                    if (!j.Template)
                    {
                        j.DeletingDemand(a_demand, a_t, a_distributionsToDelete);
                    }
                });
        }
        catch (Exception err)
        {
            throw new PTException("Error clearing references to Demands on Jobs.".Localize(), err);
        }
    }

    /// <summary>
    /// Report to the Job that the clock has advanced.
    /// </summary>
    internal void AdvanceClock(TimeSpan clockAdvancedBy, DateTime newClock, bool autoFinishAllActivities, bool autoReportProgressOnAllActivities)
    {

        foreach (Job job in JobEnumerator)
        {
            if (!job.Finished && !job.Cancelled)
            {
                job.AdvanceClock(clockAdvancedBy, newClock, autoFinishAllActivities, autoReportProgressOnAllActivities);
            }
        }

        ScenarioDetail.SalesOrderManager.AdvanceClock(newClock); //Temporary, move to ScenarioDetail
    }
    #endregion

    #region ICopyTable
    public override Type ElementType => typeof(Job);
    #endregion

    #region Indexing
    /// <summary>
    /// Returns a copy of the Job or null if not found.
    /// </summary>
    public Job GetJobCopyById(BaseId a_id, ScenarioDetail a_sd)
    {
        Job job = GetById(a_id);
        if (job == null)
        {
            return null;
        }

        using (BinaryMemoryWriter writer = new (ECompressionType.Fast))
        {
            job.Serialize(writer);
            using (BinaryMemoryReader reader = new (writer.GetBuffer()))
            {
                Job newJob = new (reader, IdGen);
                newJob.RestoreReferences(a_sd.CustomerManager, a_sd.PlantManager, a_sd.CapabilityManager, a_sd, a_sd.WarehouseManager, a_sd.ItemManager, m_errorReporter);
                return newJob;
            }
        }
    }

    public new Job this[int a_index] => GetByIndex(a_index);
    #endregion

    #region JobDataSet
    public JobDataSet GetJobDataSet()
    {
        JobDataSet ds = new ();
        for (int i = 0; i < Count; i++)
        {
            Job job = this[i];
            job.PopulateJobDataSet(this, ds);
        }

        return ds;
    }
    #endregion

    #region ERP transmission status update
    /// <summary>
    /// Call this function before handling a JobT or some other transmission that updates the status of jobs.
    /// It resets the activity variables that indicate the type of updates that have occurred.
    /// </summary>
    private void ResetERPStatusUpdateVariables()
    {
        for (int i = 0; i < Count; ++i)
        {
            this[i].ResetERPStatusUpdateVariables();
        }
    }
    #endregion

    #region Constraint Violations
    /// <summary>
    /// Find constraint violations of the Job's activities and add them to the list parameter.
    /// </summary>
    /// <param name="violations">All constraint violations are added to this list.</param>
    internal void GetConstraintViolations(ConstraintViolationList violations)
    {
        for (int i = 0; i < Count; ++i)
        {
            Job job = this[i];

            if (job.Open)
            {
                job.GetConstraintViolations(violations);
            }
        }
    }
    #endregion

    #region For Testing and Demo Data
    /// <summary>
    /// Adjust values to update Demo Data for clock advance so good relative dates are maintained.
    /// </summary>
    internal void AdjustDemoDataForClockAdvance(long a_simClock, long clockAdvanceTicks)
    {
        for (int i = 0; i < Count; i++)
        {
            Job job = this[i];
            job.AdjustDemoDataForClockAdvance(a_simClock, clockAdvanceTicks);
            //Want some Jobs to be "Entered Today" so set the Create Date or the last 10
            if (i < Count - 10)
            {
                job.EntryDate = PTDateTime.UserDateTimeNow.ToDateNoTime();
            }
        }
    }
    #endregion

    #region Delete Validation
    /// <summary>
    /// Checks to make sure the Warehouse is not in use.
    /// </summary>
    internal void ValidateWarehouseDelete(Warehouse warehouse)
    {
        for (int i = 0; i < Count; i++)
        {
            this[i].ValidateWarehouseDelete(warehouse);
        }
    }
    /// <summary>
    /// Checks to make sure the StorageArea is not in use.
    /// </summary>
    internal void ValidateStorageAreaDelete(StorageArea a_storageArea, StorageAreasDeleteProfile a_deleteProfile)
    {
        for (int i = 0; i < Count; i++)
        {
            this[i].ValidateStorageAreaDelete(a_storageArea, a_deleteProfile);
        }
    }

    /// <summary>
    /// Checks to make sure the Inventory is not in use.
    /// </summary>
    internal void ValidateInventoryDelete(InventoryDeleteProfile a_deleteProfile)
    {
        if (a_deleteProfile.ClearingJobs)
        {
            return;
        }

        for (int i = 0; i < Count; i++)
        {
            this[i].ValidateInventoryDelete(a_deleteProfile);
        }
    }

    /// <summary>
    /// Checks to make sure the Item is not in use.
    /// </summary>
    internal void ValidateItemDelete(ItemDeleteProfile a_itemDeleteProfile)
    {
        for (int i = 0; i < Count; i++)
        {
            this[i].ValidateItemDelete(a_itemDeleteProfile);
        }
    }
    /// <summary>
    /// Checks to make sure the StorageArea is not in use.
    /// </summary>
    internal void ValidateItemStorageDelete(ItemStorageDeleteProfile a_itemStorageDeleteProfile)
    {
        if (a_itemStorageDeleteProfile.ClearJobs)
        {
            return;
        }

        for (int i = 0; i < Count; i++)
        {
            this[i].ValidateItemStorageDelete(a_itemStorageDeleteProfile);
        }
    }

    internal void ResourceDeleteNotification(BaseResource r)
    {
        Parallel.ForEach(this, j => { j.ResourceDeleteNotification(r); });
    }
    #endregion

    #region Templates
    /// <summary>
    /// returns a list of Jobs whose Primary Product is an Inventory with given itemId and warehouseId.
    /// </summary>
    /// <param name="a_itemId"></param>
    /// <param name="a_warehouseId"></param>
    /// <returns></returns>
    public List<Job> FindTemplatesWithPrimaryProduct(BaseId a_itemId, BaseId a_warehouseId)
    {
        List<Job> jobs = new ();

        foreach (Job job in this)
        {
            if (job.Template)
            {
                Product p = job.GetPrimaryProduct();
                if (p != null && p.Item.Id == a_itemId && p.Warehouse.Id == a_warehouseId)
                {
                    jobs.Add(job);
                }
            }
        }

        return jobs;
    }

    /// <summary>
    /// The number of Template Jobs.
    /// </summary>
    public int TemplatesCount
    {
        get { return this.Count(x => x.Template); }
    }

    /// <summary>
    /// The number of Jobs that are not Templates.
    /// </summary>
    public int NonTemplateJobsCount => Count - TemplatesCount;

    public IEnumerable<Job> JobEnumerator
    {
        get
        {
            foreach (Job job in this)
            {
                if (!job.Template)
                {
                    yield return job;
                }
            }
        }
    }

    public IEnumerable<Job> TemplateEnumerator
    {
        get
        {
            foreach (Job job in this)
            {
                if (job.Template)
                {
                    yield return job;
                }
            }
        }
    }

    public int GetScheduledCount()
    {
        return this.Count(x => x.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled);
    }

    /// <summary>
    /// Get the number of MOs that are scheduled in this MO.
    /// </summary>
    /// <returns>The number of MOs in the job that are scheduled.</returns>
    public int GetScheduledMOsCount()
    {
        int scheduledCount = 0;
        for (int jI = 0; jI < Count; ++jI)
        {
            Job job = this[jI];
            scheduledCount += job.ManufacturingOrders.GetScheduledCount();
        }

        return scheduledCount;
    }

    public int GetLateScheduledCount()
    {
        return this.Count(x => x.ScheduledStatus == JobDefs.scheduledStatuses.Scheduled && x.Late);
    }
    #endregion Templates

    /// <summary>
    /// Find all the jobs in the BaseIdList. Throws a PTValidationException if a job can't be found.
    /// </summary>
    /// <param name="a_jobKeyList"></param>
    /// <returns></returns>
    internal List<Job> FindJobs(BaseIdList a_jobKeyList)
    {
        List<Job> jobList = new ();
        foreach (BaseId jobId in a_jobKeyList)
        {
            Job job = GetById(jobId);
            if (job == null)
            {
                throw new PTValidationException("2212", new object[] { jobId.ToString() });
            }

            jobList.Add(job);
        }

        return jobList;
    }

    private static void ProcessJobDeletedDataChanges(Job a_job, IScenarioDataChanges a_dataChanges)
    {
        if (a_job.Template)
        {
            a_dataChanges.TemplateChanges.DeletedObject(a_job.Id);
        }
        else
        {
            a_dataChanges.JobChanges.DeletedObject(a_job.Id);
        }

        a_dataChanges.FlagVisualChanges(a_job.Id);
        a_dataChanges.AuditEntry(new AuditEntry(a_job.Id, a_job), false,true);
    }

    private static void ProcessJobAddedDataChanges(Job a_job, IScenarioDataChanges a_dataChanges)
    {
        if (a_job.Template)
        {
            a_dataChanges.TemplateChanges.AddedObject(a_job.Id);
        }
        else
        {
            a_dataChanges.JobChanges.AddedObject(a_job.Id);
        }

        a_dataChanges.AuditEntry(new AuditEntry(a_job.Id, a_job), true);
    }

    private static void ProcessJobUpdatedDataChanges(BaseId a_jobId, bool a_isTemplate, IScenarioDataChanges a_dataChanges)
    {
        if (a_isTemplate)
        {
            a_dataChanges.TemplateChanges.UpdatedObject(a_jobId);
        }
        else
        {
            a_dataChanges.JobChanges.UpdatedObject(a_jobId);
        }
    }
}
