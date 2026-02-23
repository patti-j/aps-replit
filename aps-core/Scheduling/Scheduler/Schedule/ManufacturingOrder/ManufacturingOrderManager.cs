using System.Collections;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.ERPTransmissions;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.Scheduler.Schedule.Storage;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of ManufacturingOrder objects.
/// </summary>
public partial class ManufacturingOrderManager : ScenarioBaseObjectManager<ManufacturingOrder>
{
    #region PTSerializable
    internal ManufacturingOrderManager(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_idGen)
    {
        a_reader.Read(out int moCount);

        for (int i = 0; i < moCount; i++)
        {
            ManufacturingOrder mo = new (a_reader, a_idGen);
            Add(mo);
        }
    }
    
    /// <summary>
    /// Work that can only be done after RestoreReferences() has been called on every job.
    /// </summary>
    internal void RestoreReferences(ScenarioDetail a_sd, Job a_job, PlantManager a_plants, CapabilityManager a_capabilities, WarehouseManager a_aWarehouses, ItemManager a_aItems, ISystemLogger a_errorReporter)
    {
        m_scenarioDetail = a_sd;
        this.m_job = a_job;
        m_errorReporter = a_errorReporter;

        for (int i = 0; i < Count; i++)
        {
            this[i].RestoreReferences(a_job, a_plants, a_capabilities, a_sd, a_aWarehouses, a_aItems);
        }
    }

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        foreach (ManufacturingOrder mo in this)
        {
            mo.RestoreReferences(a_udfManager);
        }
    }

    /// <summary>
    /// Work that can only be done after RestoreReferences() has been called on every job.
    /// </summary>
    internal void RestoreReferences2()
    {
        for (int i = 0; i < Count; i++)
        {
            this[i].RestoreReferences2();
        }
    }
    #endregion

    #region Declarations
    public class ManufacturingOrderManagerException : PTException
    {
        public ManufacturingOrderManagerException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(a_message, a_stringParameters, a_appendHelpUrl) { }
    }
    #endregion

    #region Construction
    public ManufacturingOrderManager(Job a_job, ScenarioDetail a_sd, BaseIdGenerator a_idGen, ISystemLogger a_errorReporter)
        : base(a_idGen)
    {
        this.m_job = a_job;
        m_scenarioDetail = a_sd;
        m_errorReporter = a_errorReporter;
    }
    #endregion

    #region Properties
    private Job m_job;
    #endregion

    #region Edit functionality.
    /// <summary>
    /// Adds the specified <see cref="ManufacturingOrder"/> to the collection.
    /// </summary>
    /// <param name="a_mo">The <see cref="ManufacturingOrder"/> to add. Cannot be <see langword="null"/>.</param>
    /// <returns>The added <see cref="ManufacturingOrder"/> instance.</returns>
    internal new ManufacturingOrder Add(ManufacturingOrder a_mo)
    {
        return base.Add(a_mo);
    }

    /// <summary>
    /// unschedule and remove an MO.
    /// Call this function when an MO is being deleted from a job.
    /// If it's being replaced use ReplaceMO().
    /// </summary>
    /// <param name="a_id">MO Id to remove.</param>
    /// <param name="a_unscheduleType">Before removing MO, the MO is unscheduled. This provides a reason for unscheduling/removing</param>
    internal void Delete(BaseId a_id, IScenarioDataChanges a_dataChanges, ManufacturingOrder.UnscheduleType a_unscheduledType = ManufacturingOrder.UnscheduleType.Normal)
    {
        Delete(GetById(a_id), a_dataChanges, a_unscheduledType);
    }

    /// <summary>
    /// unschedule and remove an MO.
    /// Call this function when an MO is being deleted from a job.
    /// If it's being replaced use ReplaceMO().
    /// </summary>
    /// <param name="a_mo">The MO to remove.</param>
    /// <param name="a_unscheduleType">Before removing MO, the MO is unscheduled. This provides a reason for unscheduling/removing</param>
    internal void Delete(ManufacturingOrder a_mo, IScenarioDataChanges a_dataChanges, ManufacturingOrder.UnscheduleType a_unscheduleType = ManufacturingOrder.UnscheduleType.Normal)
    {
        bool unscheduled = a_mo.Unschedule(a_unscheduleType);
        if (unscheduled)
        {
            a_dataChanges.FlagConstraintChanges(a_mo.Job.Id);
        }
        a_mo.Deleting(a_dataChanges);
        base.Remove(a_mo.Id);

        a_dataChanges.AuditEntry(new AuditEntry(a_mo.Id, a_mo.Job.Id, a_mo), false, true);
    }

    /// <summary>
    /// Replace one MO with another. This was written to support changes to alternate paths.
    /// It's easier to replace the temporary MO for the original MO when the alterate path is changed.
    /// </summary>
    /// <param name="a_mo">Original MO</param>
    /// <param name="a_newMO">Replacement MO</param>
    private void ReplaceMO(ManufacturingOrder a_mo, ManufacturingOrder a_newMO, BaseId a_newMOId, IScenarioDataChanges a_dataChanges)
    {
        if (a_mo.UsedAsTemplateInInventories != null)
        {
            a_newMO.UsedAsTemplateForInventories(a_mo.UsedAsTemplateInInventories);
        }

        Delete(a_mo, a_dataChanges, ManufacturingOrder.UnscheduleType.Replacement);
        m_job.MOReplaced = true;

        // Add the replacement MO to the manager.
        a_newMO.Id = a_newMOId;

        Add(a_newMO);

        //Preserve the MO's hold status so it's not lost in the case of a new structure
        if (a_mo.OnHold == holdTypes.OnHold)
        {
            a_newMO.HoldIt(true, a_mo.HoldUntil, a_mo.HoldReason);
        }
    }
    #endregion

    #region ManufacturingOrder Edit Functions
    private ManufacturingOrder AddCopy(ManufacturingOrderCopyT a_t)
    {
        ValidateCopy(a_t);
        ManufacturingOrder m = GetById(a_t.originalId);
        return AddCopy(m, m.Clone(), NextID());
    }

    private ManufacturingOrder DeleteManufacturingOrder(ManufacturingOrderDeleteT a_t, IScenarioDataChanges a_dataChanges)
    {
        ValidateDelete(a_t);
        ManufacturingOrder mo = GetById(a_t.manufacturingOrderId);
        if (mo != null)
        {
            Delete(mo, a_dataChanges, ManufacturingOrder.UnscheduleType.Deletion);
        }

        return mo;
    }
    #endregion

    #region Transmissions
    private void ValidateCopy(ManufacturingOrderCopyT a_t)
    {
        ValidateExistence(a_t.originalId);
    }

    private void ValidateDelete(ManufacturingOrderDeleteT a_t)
    {
        if (Count < 2)
        {
            throw new ValidationException("2946");
        }

        ValidateExistence(a_t.manufacturingOrderId);
    }

    public void Receive(ManufacturingOrderBaseT a_t, ProductRuleManager a_productRuleManager, IScenarioDataChanges a_dataChanges)
    {
        ManufacturingOrder m;

        if (!m_job.Template)
        {
            m_scenarioDetail.SignalMOChanges();
        }

        if (a_t is ManufacturingOrderCopyT copyT)
        {
            m = AddCopy(copyT);
            if (m_job.Template)
            {
                a_dataChanges.TemplateChanges.AddedObject(m.Job.Id);
            }
            else
            {
                a_dataChanges.JobChanges.AddedObject(m.Job.Id);
            }
        }
        else if (a_t is ManufacturingOrderDeleteT deleteT)
        {
            m = DeleteManufacturingOrder(deleteT, a_dataChanges);
            if (m_job.Template)
            {
                a_dataChanges.TemplateChanges.DeletedObject(m.Job.Id);
            }
            else
            {
                a_dataChanges.JobChanges.DeletedObject(m.Job.Id);
            }
        }
        else if (a_t is ManufacturingOrderIdBaseT baseT)
        {
            m = ValidateExistence(baseT.manufacturingOrderId);
            m.Receive(baseT, a_productRuleManager, a_dataChanges);

            if (m_job.Template)
            {
                a_dataChanges.TemplateChanges.UpdatedObject(m.Job.Id);
            }
            else
            {
                a_dataChanges.JobChanges.UpdatedObject(m.Job.Id);
            }
        }
    }

    /// <summary>
    /// Process a JobT
    /// </summary>
    /// <param name="a_plantManager"></param>
    /// <param name="a_erpUpdate">Whether this is a JobT from an ERP system or is internally generated.</param>
    /// <param name="a_jobTJob"></param>
    /// <param name="a_scenarioDetail"></param>
    /// <param name="a_newMOsAdded">
    /// In the case of an existing job this is used to return whether any new MOs were added to the job. In the case of a new Job it should always return true since new MOs are
    /// always added.
    /// </param>
    /// <param name="a_t"></param>
    /// <param name="a_dataChanges"></param>
    /// <param name="a_udfManager"></param>
    internal bool Receive(UserFieldDefinitionManager a_udfManager, PlantManager a_plantManager, bool a_erpUpdate, JobT.Job a_jobTJob, ScenarioDetail a_scenarioDetail, out bool a_newMOsAdded, PTTransmission a_t, IScenarioDataChanges a_dataChanges)
    {
        Hashtable jobMOExternalIds = new ();
        a_newMOsAdded = false;

        List<ManufacturingOrder> mosToRemoveIfMissingDueToQtyChange = new ();
        List<ManufacturingOrder> mosToRemoveIfMissingDueToRoutingChange = new ();
        Dictionary<string, JobT.ManufacturingOrder> jobTJobMOs = new ();
        for (int moNbr = 0; moNbr < a_jobTJob.ManufacturingOrderCount; ++moNbr) //create a list for use during Split updates
        {
            JobT.ManufacturingOrder jobTmo = a_jobTJob[moNbr];
            jobTJobMOs.TryAdd(jobTmo.ExternalId, jobTmo);
        }

        bool jobUpdated = false;
        for (int moNbr = 0; moNbr < a_jobTJob.ManufacturingOrderCount; ++moNbr)
        {
            JobT.ManufacturingOrder jobTmo = a_jobTJob[moNbr];
            ManufacturingOrder mo = GetByExternalId(jobTmo.ExternalId);
            if (!jobMOExternalIds.Contains(jobTmo.ExternalId))
            {
                jobMOExternalIds.Add(jobTmo.ExternalId, null);
            }
            
            if (mo != null)
            {
                AuditEntry moAuditEntry = new AuditEntry(mo.Id, mo.Job.Id, mo);

                ManufacturingOrder moTemp = new (new BaseId(long.MaxValue), m_job, jobTmo, a_scenarioDetail.CapabilityManager, a_scenarioDetail, a_erpUpdate, a_dataChanges, a_udfManager, m_errorReporter, mo, false, ((JobT)a_t).AutoDeleteOperationAttributes);
                List<ManufacturingOrder> moSplits = mo.GetSplitsRecursively();
                RoutingChanges routingChanges = mo.IdenticalPathStructureAndObjectTypes(moTemp);

                //compare temp mo Required Qty to existing mo original qty.
                //1---> same : Keep the resized value for ERP updates.
                if (a_erpUpdate && moTemp.ResizeForStorage && moTemp.RequiredQty == mo.OriginalQty)
                {
                    decimal ratio = mo.RequiredQty / moTemp.RequiredQty;
                    moTemp.AdjustQtyRequiredByRatioForStorage(ratio, null, a_scenarioDetail.ProductRuleManager);
                }
                else
                {
                    //2---> not same: flag scheduled routing change so MO is replaced
                    //TODO: Solved?
                    //routingChanges.ScheduledRoutingChanged = true;
                }

                if (routingChanges.ScheduledRoutingChanged)
                {
                    mo.FlagAffectedItemsForNetChangeMRP(a_scenarioDetail.WarehouseManager);
                    moTemp.FlagAffectedItemsForNetChangeMRP(a_scenarioDetail.WarehouseManager);
                    ReplaceMO(mo, moTemp, NextID(), a_dataChanges);

                    //Record the history since it can cause major schedule change
                    moTemp.Job.ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(new[] { moTemp.Job.Id }, "Routing changed for scheduled M.O.".Localize() + " " + moTemp.Name + ". " + routingChanges.RoutingChangeDescription, typeof(Job), ScenarioHistory.historyTypes.JobRoutingChanged);

                    //Remove splits due to routing change.
                    if (a_erpUpdate && moSplits.Count > 0) //if coming from UI then treat all MOs the same since the user can adjust as desired.   
                    {
                        for (int splitI = 0; splitI < moSplits.Count; splitI++)
                        {
                            ManufacturingOrder oldSplitMO = moSplits[splitI];
                            if (!oldSplitMO.Finished) //preserve finished MOs in case the information is needed
                            {
                                mosToRemoveIfMissingDueToRoutingChange.Add(oldSplitMO);
                            }
                        }
                    }

                    a_dataChanges.JobChanges.UpdatedObject(mo.Job.Id);
                    a_dataChanges.FlagEligibilityChanges(mo.Job.Id);
                    jobUpdated = true;
                }
                else //no Significant routing change
                {
                    // Update an existing MO
                    bool preserveSplits = false;
                    decimal totalOldQty = mo.RequiredQty;
                    if (a_erpUpdate && moSplits.Count > 0)
                    {
                        decimal totalNewQty = moTemp.RequiredQty;
                        for (int splitI = 0; splitI < moSplits.Count; splitI++)
                        {
                            ManufacturingOrder oldSplitMO = moSplits[splitI];
                            totalOldQty += oldSplitMO.RequiredQty;
                            if (jobTJobMOs.TryGetValue(oldSplitMO.ExternalId, out JobT.ManufacturingOrder jobTMoForOldSplit))
                            {
                                totalNewQty += jobTMoForOldSplit.RequiredQty;
                            }
                        }

                        preserveSplits = totalNewQty == totalOldQty;
                        if (preserveSplits)
                        {
                            moTemp.PreserveRequiredQty = true; //prevent the MO from being updated to the new qty, which is the sum of all split qties.
                        }
                    }

                    bool updated = mo.Update(a_udfManager, a_plantManager, a_erpUpdate, jobTmo, moTemp, a_scenarioDetail.ScenarioOptions, routingChanges.AlternatePathChanged, a_t, a_dataChanges);

                    if (!preserveSplits) //don't know what the user wants to do with split qties so remove them
                    {
                        for (int splitI = 0; splitI < moSplits.Count; splitI++)
                        {
                            ManufacturingOrder oldSplitMO = moSplits[splitI];
                            if (!oldSplitMO.Finished) //preserve finished MOs in case the information is needed
                            {
                                mosToRemoveIfMissingDueToQtyChange.Add(oldSplitMO);
                            }
                        }
                    }
                    else //no qty change so MO splits were preserved
                    {
                        //MO Split status updating
                        if (mo.SplitUpdateMode != ManufacturingOrderDefs.SplitUpdateModes.UpdateSplitsIndividually)
                        {
                            mo.AllocateStatusAcrossSplits(totalOldQty);
                        }
                    }

                    if (updated)
                    {
                        jobUpdated = true;
                        a_dataChanges.AuditEntry(moAuditEntry);
                    }
                }
            }
            else
            {
                // Add a new MO
                mo = new ManufacturingOrder(NextID(), m_job, a_jobTJob[moNbr], a_scenarioDetail.CapabilityManager, a_scenarioDetail, a_erpUpdate, a_dataChanges, a_udfManager, m_errorReporter, null, true, ((JobT)a_t).AutoDeleteOperationAttributes, true);
                Add(mo);
                a_newMOsAdded = true;

                a_dataChanges.AuditEntry(new AuditEntry(mo.Id, mo.Job.Id, mo), true);

                a_dataChanges.FlagEligibilityChanges(mo.Job.Id);
                jobUpdated = true;
            }
        }

        // Delete MO's that are no longer a part of the Job
        for (int moI = Count - 1; moI >= 0; --moI)
        {
            ManufacturingOrder mo = this[moI];

            if (!jobMOExternalIds.Contains(mo.ExternalId)) ////TEMPORORY && !(mo.Split && erpUpdate)) //Split MOs don't usually get imported but they need to be preserved.
            {
                if (!mo.Split) //splits are handled below
                {
                    Delete(mo, a_dataChanges, ManufacturingOrder.UnscheduleType.Deletion);
                    jobUpdated = true;
                }
            }
        }

        //Remove any splits that are missing and need to be removed due to MO changes
        int mosRemovedDueToRoutingChange = 0;
        for (int i = 0; i < mosToRemoveIfMissingDueToRoutingChange.Count; i++)
        {
            ManufacturingOrder mo = mosToRemoveIfMissingDueToRoutingChange[i];
            if (!jobMOExternalIds.Contains(mo.ExternalId))
            {
                Delete(mo, a_dataChanges, ManufacturingOrder.UnscheduleType.Deletion);
                mosRemovedDueToRoutingChange++;
                jobUpdated = true;
            }
        }

        int mosRemovedDueToQtyChange = 0;
        for (int i = 0; i < mosToRemoveIfMissingDueToQtyChange.Count; i++)
        {
            ManufacturingOrder mo = mosToRemoveIfMissingDueToQtyChange[i];
            if (!jobMOExternalIds.Contains(mo.ExternalId))
            {
                if (Contains(mo.Id)) //might have been removed already due to a routing change above.
                {
                    Delete(mo, a_dataChanges, ManufacturingOrder.UnscheduleType.Deletion);
                    mosRemovedDueToQtyChange++;
                    jobUpdated = true;
                }
            }
        }

        if (mosRemovedDueToRoutingChange > 0)
        {
            m_job.ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(new[] { m_job.Id }, string.Format("{0} Unfinished Split MOs were deleted during import due to a routing change on the original MO.".Localize(), mosRemovedDueToRoutingChange), typeof(Job), ScenarioHistory.historyTypes.JobRoutingChanged);
        }

        if (mosRemovedDueToQtyChange > 0)
        {
            m_job.ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(new[] { m_job.Id }, string.Format("{0} Unfinished Split MOs were deleted during import due to a Required Qty change on the original MO.".Localize(), mosRemovedDueToQtyChange), typeof(Job), ScenarioHistory.historyTypes.JobChanged);
        }

        return jobUpdated;
    }
    #endregion

    #region Indexer
    public new ManufacturingOrder this[int a_index] => GetByIndex(a_index);
    #endregion

    #region ICopyTable
    public override Type ElementType => typeof(ManufacturingOrder);
    #endregion

    #region UI/Interfacing
    internal void PopulateJobDataSet(JobManager a_jobs, ref JobDataSet a_dataSet)
    {
        //Set sort to scheduled start and the Operation Id (in case of it being unscheduled)
        a_dataSet.ManufacturingOrder.DefaultView.Sort = string.Format("{0} ASC,{1} ASC", a_dataSet.ManufacturingOrder.ScheduledStartColumn.ColumnName, a_dataSet.ManufacturingOrder.IdColumn.ColumnName);

        for (int i = 0; i < Count; i++)
        {
            this[i].PopulateJobDataSet(a_jobs, ref a_dataSet);
        }
    }
    #endregion

    #region Delete validation
    /// <summary>
    /// Checks to make sure the Warehouse is not in use.
    /// </summary>
    internal void ValidateWarehouseDelete(Warehouse a_warehouse)
    {
        for (int m = 0; m < Count; m++)
        {
            this[m].ValidateWarehouseDelete(a_warehouse);
        }
    }
    /// <summary>
    /// Checks to make sure the StorageArea is not in use.
    /// </summary>
    internal void ValidateStorageAreaDelete(StorageArea a_storageArea, StorageAreasDeleteProfile a_deleteProfile)
    {
        for (int m = 0; m < Count; m++)
        {
            this[m].ValidateStorageAreaDelete(a_storageArea, a_deleteProfile);
        }
    }

    /// <summary>
    /// Checks to make sure the Inventory is not in use.
    /// </summary>
    internal void ValidateInventoryDelete(InventoryDeleteProfile a_deleteProfile)
    {
        for (int m = 0; m < Count; m++)
        {
            this[m].ValidateInventoryDelete(a_deleteProfile);
        }
    }

    /// <summary>
    /// Checks to make sure the Item is not in use.
    /// </summary>
    internal void ValidateItemDelete(ItemDeleteProfile a_itemDeleteProfile)
    {
        for (int m = 0; m < Count; m++)
        {
            this[m].ValidateItemDelete(a_itemDeleteProfile);
        }
    }
    /// <summary>
    /// Checks to make sure the StorageArea is not in use.
    /// </summary>
    internal void ValidateItemStorageDelete(ItemStorageDeleteProfile a_itemStorageDeleteProfile)
    {
        for (int m = 0; m < Count; m++)
        {
            this[m].ValidateItemStorageDelete(a_itemStorageDeleteProfile);
        }
    }

    internal void ResourceDeleteNotification(Resource a_r) { }
    #endregion

    #region Demo Data
    /// <summary>
    /// Adjust values to update Demo Data for clock advance so good relative dates are maintained.
    /// </summary>
    internal void AdjustDemoDataForClockAdvance(long a_clockAdvanceTicks)
    {
        for (int i = 0; i < Count; i++)
        {
            this[i].AdjustDemoDataForClockAdvance(a_clockAdvanceTicks);
        }
    }
    #endregion

    /// <summary>
    /// Return the number of manufacturing orders that are scheduled.
    /// </summary>
    /// <returns>The number of MOs in this object that are scheduled.</returns>
    public int GetScheduledCount()
    {
        int scheduledCount = 0;
        for (int mI = 0; mI < Count; ++mI)
        {
            ManufacturingOrder mo = this[mI];
            if (mo.Scheduled)
            {
                ++scheduledCount;
            }
        }

        return scheduledCount;
    }
}