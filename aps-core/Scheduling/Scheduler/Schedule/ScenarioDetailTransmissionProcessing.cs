using System.Collections;
using System.Diagnostics;
using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.Common.Localization;
using PT.ERPTransmissions;
using PT.PackageDefinitions;
using PT.PackageDefinitions.Settings;
using PT.Scheduler.ErrorReporting;
using PT.SchedulerDefinitions;
using PT.SchedulerDefinitions.Templates.Lists;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;
using Activity = PT.ERPTransmissions.Activity;

namespace PT.Scheduler;

public partial class ScenarioDetail
{
    private void ProcessSystemOptionsT(SystemOptionsT a_systemOptionsT, IScenarioDataChanges a_dataChanges)
    {
        AuditEntry auditEntry = new AuditEntry(new BaseId(ScenarioOptions.UniqueId), ScenarioOptions);
        //Update the Locking and Anchoring in the frozen zone if either value changed.
        if (a_systemOptionsT.ScenarioOptions.AnchorInFrozenZone != ScenarioOptions.AnchorInFrozenZone ||
            a_systemOptionsT.ScenarioOptions.LockInFrozenZone != ScenarioOptions.LockInFrozenZone)
        {
            UpdateLockAndAnchorInFrozenZone(a_systemOptionsT.ScenarioOptions, a_dataChanges);
        }

        if (ScenarioOptions.TrackSubComponentSourceMOs != a_systemOptionsT.ScenarioOptions.TrackSubComponentSourceMOs)
        {
            // October 16th, 2008: This was put here because there appears to be a copy of SystemOptions in Scenario (also SystemOptions doesn't have a reference to ScenarioDetail).
            ScenarioOptionsTrackSubComponentSourceMOsChanged();
        }

        bool updateCapacityInvervals = a_systemOptionsT.ScenarioOptions.PlanningHorizon.Ticks != ScenarioOptions.PlanningHorizon.Ticks;
        ScenarioOptions.Update(a_systemOptionsT.ScenarioOptions); //Need to update the planning horizion before calling AdjutForChangedHorizon below

        a_dataChanges.AuditEntry(auditEntry);
        //Only adjust for changed horizon if the horizon has changed
        if (updateCapacityInvervals)
        {
            RecurringCapacityIntervalManager.AdjustForChangedHorizon(Clock, this, a_dataChanges);
            TimeAdjustment(a_systemOptionsT);
        }

        using (_scenario.ScenarioEventsLock.EnterRead(out ScenarioEvents se))
        {
            se.FireScenarioOptionsChangedEvent(a_systemOptionsT, this);
        }
    }

    private void ProcessLookupAttributeNumberRangeT(LookupAttributeNumberRangeT a_lookupAttributeNumberRangeT, IScenarioDataChanges a_dataChanges)
    {
        List<PostProcessingAction> actions = new ();

        try
        {
            List<ResourceKeyList> resourceLists;
            HashSet<BaseId> tableIdsAffected;
            List<RangeLookup.FromRangeSets> frsList = FromRangeSetManager.Receive(this, a_lookupAttributeNumberRangeT, out resourceLists, out tableIdsAffected, a_dataChanges);
            for (int tI = 0; tI < frsList.Count; tI++)
            {
                RangeLookup.FromRangeSets attributeFromRangeSetList = frsList[tI];
                ResourceKeyList resKeyList = resourceLists[tI];
                UpdateAttributeFromRangeSetListReferences(attributeFromRangeSetList, resKeyList);
            }

            if (a_lookupAttributeNumberRangeT.AutoDeleteMode)
            {
                actions.Add(new PostProcessingAction(a_lookupAttributeNumberRangeT, false, () =>
                    {
                        Dictionary<string, Resource> dictionary = PlantManager.GetResourceHash();
                        try
                        {
                            //Delete entries not in the transmission
                            for (int i = FromRangeSetManager.Count - 1; i >= FromRangeSetManager.Count; i--)
                            {
                                BaseId tableId = FromRangeSetManager[i].Id;
                                if (!tableIdsAffected.Contains(tableId))
                                {
                                    FromRangeSetManager.Delete(tableId, a_dataChanges);
                                    ClearResourceFromToRanges(dictionary, tableId);
                                }
                            }
                        }
                        catch (PTHandleableException err)
                        {
                            ScenarioExceptionInfo sei = new ();
                            sei.Create(this);
                            m_errorReporter.LogException(err, a_lookupAttributeNumberRangeT, sei, ELogClassification.PtSystem, false);
                        }
                    }));
            }

            _eligibilitySignal_LookupAttributeNumberRangeUpdated();
        }
        catch (PTHandleableException err)
        {
            ScenarioExceptionInfo sei = new ();
            sei.Create(this);
            m_errorReporter.LogException(err, a_lookupAttributeNumberRangeT, sei, ELogClassification.PtSystem, false);
        }
        finally
        {
            AddProcessingAction(actions);
        }
    }

    //This function is called from ScenarioManager. It does not go through Receive because it has an additional parameter (Scenario)
    internal void ReceiveCtp(Transmissions.CTP.CtpT ctpT, Scenario scenarioToSendResultTo, IScenarioDataChanges a_dataChanges)
    {
        Ctp(ctpT, scenarioToSendResultTo, a_dataChanges);
    }

    #region Resource
    /// <summary>
    /// Process DepartmentT transmission.
    /// </summary>
    /// <param name="a_t"></param>
    private void ProcessDepartmentT(ScenarioDetail a_sd, UserFieldDefinitionManager a_udfManager, DepartmentT a_t, IScenarioDataChanges a_dataChanges)
    {
        ApplicationExceptionList errList = new ();
        List<PostProcessingAction> actions = new ();

        HashSet<BaseId> affectedPlantIds = new ();
        HashSet<BaseId> affectedDepartments = new ();

        try
        {
            for (int i = 0; i < a_t.Count; ++i)
            {
                try
                {
                    DepartmentT.Department transmissionDept = a_t[i];
                    Plant plant = PlantManager.GetByExternalId(transmissionDept.PlantExternalId);
                    if (plant == null)
                    {
                        throw new TransmissionValidationException(a_t, "2619", new object[] { transmissionDept.PlantExternalId });
                    }

                    DepartmentManager.DepartmentTHandlerResult result = plant.Departments.DepartmentTHandler(a_sd, a_udfManager, transmissionDept, plant, out Department dept, a_t, a_dataChanges);
                    if (result == DepartmentManager.DepartmentTHandlerResult.added)
                    {
                        plant.AddDepartmentAssociation(dept);

                        a_dataChanges.DepartmentChanges.AddedObject(dept.Id);
                        if (!affectedPlantIds.Contains(plant.Id))
                        {
                            affectedPlantIds.Add(plant.Id);
                            a_dataChanges.PlantChanges.UpdatedObject(plant.Id);
                        }
                    }
                    else
                    {
                        a_dataChanges.DepartmentChanges.UpdatedObject(dept.Id);
                    }

                    if (!affectedDepartments.Contains(dept.Id))
                    {
                        affectedDepartments.Add(dept.Id);
                    }
                }
                catch (PTHandleableException err)
                {
                    errList.Add(err);
                }
            }

            if (a_t.AutoDeleteMode)
            {
                List<Department> deptsToDelete = new ();
                foreach (Plant p in PlantManager)
                {
                    foreach (Department d in p.Departments)
                    {
                        if (!affectedDepartments.Contains(d.Id)) // not added or updated
                        {
                            deptsToDelete.Add(d);
                        }
                    }
                }

                if (deptsToDelete.Count > 0) // now remove them
                {
                    actions.Add(new PostProcessingAction(a_t, false, () =>
                        {
                            ApplicationExceptionList delErrs = new ();

                            foreach (Department delDept in deptsToDelete)
                            {
                                try
                                {
                                    delDept.Plant.Departments.DeleteTry(delDept, a_dataChanges);
                                    if (!affectedPlantIds.Contains(delDept.Plant.Id))
                                    {
                                        affectedPlantIds.Add(delDept.Plant.Id);
                                        a_dataChanges.PlantChanges.UpdatedObject(delDept.Plant.Id);
                                    }
                                }
                                catch (PTValidationException err)
                                {
                                    delErrs.Add(err);
                                }
                            }

                            if (delErrs.Count > 0)
                            {
                                ScenarioExceptionInfo sei = new ();
                                sei.Create(this);
                                m_errorReporter.LogException(delErrs, a_t, sei, ELogClassification.PtInterface, false);
                            }
                        }));
                }
            }
        }
        catch (PTHandleableException err)
        {
            errList.Add(err);
        }
        finally
        {
            AddProcessingAction(actions);

            if (errList.Count > 0)
            {
                ScenarioExceptionInfo sei = new ();
                sei.Create(this);
                m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
            }
        }
    }

    private Department GetResourceTDept(ResourceT a_t, ResourceT.Resource a_resT)
    {
        Plant p = m_plantManager.GetByExternalId(a_resT.PlantExternalId);
        if (p == null)
        {
            throw new TransmissionValidationException(a_t, "2620", new object[] { a_resT.plantExternalId });
        }

        Department d = p.Departments.GetByExternalId(a_resT.DepartmentExternalId);
        if (d == null)
        {
            throw new TransmissionValidationException(a_t, "2621", new object[] { a_resT.departmentExternalId, a_resT.plantExternalId });
        }

        return d;
    }

    private List<Capability> GetResourceTCapabilities(ResourceT a_t, ResourceT.Resource a_resT)
    {
        List<Capability> capList = new ();

        for (int j = 0; j < a_resT.Capabilities.Count; ++j)
        {
            Capability c = m_capabilityManager.GetByExternalId(a_resT.Capabilities[j]);
            if (c == null)
            {
                throw new TransmissionValidationException(a_t, "2622", new object[] { a_resT.Capabilities[j] });
            }

            capList.Add(c);
        }

        return capList;
    }

    private void ProcessResourceT(ScenarioDetail a_sd, UserFieldDefinitionManager a_udfManager, ResourceT a_resourceImportT, IScenarioDataChanges a_dataChanges)
    {
        ApplicationExceptionList errList = new ();
        List<PostProcessingAction> actions = new ();

        HashSet<Department> deptHash = new ();
        HashSet<Capability> capHash = new ();
        HashSet<Tuple<string, string, string>> resourceKeyHash = new ();
        try //need to wrap in try catch in case plant doesn't exist and add fails, still need to update UI with others already added`
        {
            a_resourceImportT.Validate();

            Resource res;
            for (int i = 0; i < a_resourceImportT.Count; ++i)
            {
                try
                {
                    ResourceT.Resource resourceTResource = a_resourceImportT[i];

                    resourceTResource.Validate();

                    Department dept = GetResourceTDept(a_resourceImportT, resourceTResource);
                    List<Capability> capabilities = GetResourceTCapabilities(a_resourceImportT, resourceTResource);

                    string originalResExternalId = resourceTResource.ExternalId;
                    string originalResName = resourceTResource.Name;
                    //May need to create multiples of the same Resource
                    for (int curResIdx = 0; curResIdx < resourceTResource.NumberOfResources; curResIdx++)
                    {
                        try
                        {
                            if (curResIdx > 0) //Add an index number to the Resource ExternalId since it's a copy being added
                            {
                                resourceTResource.ExternalId = string.Format("{0}-{1}", originalResExternalId, curResIdx + 1);
                                resourceTResource.Name = string.Format("{0}-{1}", originalResName, curResIdx + 1);
                            }

                            ResourceManager.ERPTHandlerResult handlerResult = dept.Resources.MachineTHandler(a_udfManager, resourceTResource, dept, m_dispatcherDefinitionManager.DefaultDispatcherDefinition, out res, ShopViewResourceOptionsManager.DefaultOptions, a_resourceImportT, a_dataChanges);

                            if (handlerResult == ResourceManager.ERPTHandlerResult.added)
                            {
                                dept.AddMachineAssociation(res, a_dataChanges);
                                if (!deptHash.Contains(dept))
                                {
                                    deptHash.Add(dept);
                                    a_dataChanges.DepartmentChanges.UpdatedObject(dept.Id);
                                }

                                if (a_resourceImportT.IncludeCapabilityAssociations)
                                {
                                    //Add a Machine association to each Capability for the new Machine and add each Capability to the Machine's Capabilities list.
                                    foreach (Capability c in capabilities)
                                    {
                                        res.AddCapability(c);
                                        c.AddResourceAssociation(res);
                                        if (!capHash.Contains(c))
                                        {
                                            capHash.Add(c);
                                            a_dataChanges.CapabilityChanges.UpdatedObject(c.Id);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (a_resourceImportT.IncludeCapabilityAssociations)
                                {
                                    SetMachineCapabilities(res, capabilities, a_resourceImportT.AutoDeleteCapabilityAssociations, out ArrayList affected, out bool jobsAffected);
                                    //Update the capabilities that were in the list
                                    for (int capI = 0; capI < affected.Count; capI++)
                                    {
                                        Capability curCapability = (Capability)affected[capI];
                                        if (!capHash.Contains(curCapability))
                                        {
                                            capHash.Add(curCapability);
                                            a_dataChanges.CapabilityChanges.UpdatedObject(curCapability.Id);
                                        }
                                    }

                                    if (jobsAffected)
                                    {
                                        a_dataChanges.FlagConstraintChanges(res.Id);
                                    }
                                }
                            }
                        }
                        catch (PTHandleableException err)
                        {
                            errList.Add(err);
                        }
                        finally
                        {
                            resourceKeyHash.Add(resourceTResource.GetExternalKey());
                        }
                    }
                }
                catch (PTHandleableException err)
                {
                    errList.Add(err);
                }
            }

            if (a_resourceImportT.AutoDeleteMode)
            {
                List<Resource> toDelete = new ();
                for (int p = 0; p < m_plantManager.Count; p++)
                {
                    Plant plant = m_plantManager.GetByIndex(p);
                    for (int w = 0; w < plant.Departments.Count; w++)
                    {
                        Department department = plant.Departments.GetByIndex(w);
                        for (int i = department.Resources.Count - 1; i >= 0; i--)
                        {
                            res = department.Resources[i];
                            if (!resourceKeyHash.Contains(res.GetExternalKey()))
                            {
                                toDelete.Add(res);
                            }
                        }
                    }
                }

                if (toDelete.Count > 0)
                {
                    actions.Add(new PostProcessingAction(a_resourceImportT, false, () =>
                        {
                            ApplicationExceptionList delErrs = new ();

                            foreach (Resource delRes in toDelete)
                            {
                                try
                                {
                                    delRes.Department.Resources.Delete(delRes, a_dataChanges);
                                }
                                catch (PTValidationException err)
                                {
                                    delErrs.Add(err);
                                }
                            }

                            if (delErrs.Count > 0)
                            {
                                ScenarioExceptionInfo sei = new ();
                                sei.Create(this);
                                m_errorReporter.LogException(delErrs, a_resourceImportT, sei, ELogClassification.PtInterface, false);
                            }
                        }));
                }
            }

            //After Resources are updated, update the Allowed Helpers
            if (a_resourceImportT.UpdateAllowedHelpers)
            {
                PlantManager.UpdateAllowedHelpers(a_resourceImportT);
            }
        }
        catch (PTHandleableException e)
        {
            errList.Add(e);
        }
        finally
        {
            AddProcessingAction(actions);

            if (errList.Count > 0)
            {
                ScenarioExceptionInfo sei = new ();
                sei.Create(this);
                m_errorReporter.LogException(errList, a_resourceImportT, sei, ELogClassification.PtInterface, false);
            }
        }
    }
    #endregion

    #region CapacityInterval
    private void ProcessCapacityIntervalT(UserFieldDefinitionManager a_udfManager, CapacityIntervalT a_t, IScenarioDataChanges a_dataChanges)
    {
        ApplicationExceptionList errList = new ();
        try
        {
            Dictionary<BaseId, InternalResource> affectedResources = new ();

            //First need to delete the RCIs that no longer exist if using auto-delete mode
            CapacityIntervalManager.CapacityIntervalTHandler(a_t, this, affectedResources, a_dataChanges);

            CapacityIntervalManager.InitFastLookupByExternalId();
            //Now process each RCI definition in the Transmission
            for (int i = 0; i < a_t.Count; i++)
            {
                CapacityIntervalTransmissionHelper(a_udfManager, a_t, a_t[i], affectedResources, a_dataChanges);
            }

            CapacityIntervalManager.DeInitFastLookupByExternalId();

            //Regenerate the Resources' capacity profiles and update the UI.
            foreach (InternalResource res in affectedResources.Values)
            {
                res.RegenerateCapacityProfile(this.GetPlanningHorizonEndTicks(), true);
            }
        }
        catch (PTHandleableException err)
        {
            errList.Add(err);
        }
        finally
        {
            if (errList.Count > 0)
            {
                ScenarioExceptionInfo sei = new ();
                sei.Create(this);
                m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
            }
        }
    }

    private void ProcessRecurringCapacityIntervalT(UserFieldDefinitionManager a_udfManager, RecurringCapacityIntervalT a_t, IScenarioDataChanges a_dataChanges)
    {
        ApplicationExceptionList errList = new ();
        try
        {
            //Now process each RCI definition in the Transmission
            CalendarResourcesCollection affectedResources = new ();

            RecurringCapacityIntervalManager.InitFastLookupByExternalId();

            //First need to delete the RCIs that no longer exist if using auto-delete mode
            if (a_t.AutoDeleteMode)
            {
                RecurringCapacityIntervalManager.RecurringCapacityIntervalTHandler(a_t.Nodes, this, affectedResources, a_dataChanges, a_t.AutoDeleteMode, a_t.AutoDeleteResourceAssociations);
            }

            for (int i = 0; i < a_t.Count; i++)
            {
                RecurringCapacityIntervalTransmissionHelper(a_udfManager, a_t, a_t[i], affectedResources, a_dataChanges);
            }

            RecurringCapacityIntervalManager.DeInitFastLookupByExternalId();

            //Regenerate the Resources' capacity profiles and update the UI.
            for (int i = 0; i < affectedResources.Count; i++)
            {
                affectedResources[i].RegenerateCapacityProfile(this.GetPlanningHorizonEndTicks(), true);
            }
        }
        catch (PTHandleableException err)
        {
            errList.Add(err);
        }
        finally
        {
            if (errList.Count > 0)
            {
                ScenarioExceptionInfo sei = new ();
                sei.Create(this);
                m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
            }
        }
    }

    private void CapacityIntervalTransmissionHelper(UserFieldDefinitionManager a_udfManager, CapacityIntervalT t, CapacityIntervalT.CapacityIntervalDef ciDef, Dictionary<BaseId, InternalResource> a_affectedResources, IScenarioDataChanges a_dataChanges)
    {
        Resource[] machines = new Resource[ciDef.ResourceExternalIds.Count];

        //Now make sure that each resources exists
        for (int i = 0; i < ciDef.ResourceExternalIds.Count; i++)
        {
            Tuple<string, string, string> resExternalId = ciDef.ResourceExternalIds[i];
            string plantExternalId = resExternalId.Item1;
            string deptExternalId = resExternalId.Item2;
            string resourceExternalId = resExternalId.Item3;
            Plant plant = m_plantManager.GetByExternalId(plantExternalId);
            if (plant == null)
            {
                throw new TransmissionValidationException(t, "2624", new object[] { plantExternalId });
            }

            Department dept = plant.Departments.GetByExternalId(deptExternalId);
            if (dept == null)
            {
                throw new TransmissionValidationException(t, "2625", new object[] { deptExternalId, plantExternalId });
            }

            Resource res = dept.Resources.GetByExternalId(resourceExternalId);
            machines[i] = res ?? throw new TransmissionValidationException(t, "2626", new object[] { resourceExternalId, deptExternalId, plantExternalId });
        }

        CapacityIntervalManager.CapacityIntervalTHandler(a_udfManager, ciDef, machines, this, t, a_affectedResources, a_dataChanges);
    }

    private void RecurringCapacityIntervalTransmissionHelper(UserFieldDefinitionManager a_udfManager, RecurringCapacityIntervalT a_t, RecurringCapacityIntervalDef a_rciDef, CalendarResourcesCollection a_affectedResources, IScenarioDataChanges a_dataChanges)
    {
        if (a_rciDef.Rci != null && a_rciDef.Rci.RecurrenceEndDateTime < ClockDate && a_rciDef.Rci.RecurrenceEndDateTimeSet)
        {
            return;
        }

        Resource[] resources = new Resource[a_rciDef.ResourceKeyList.Count];

        int count = 0;
        foreach (ResourceKeyExternal resourceKeyExternal in a_rciDef.ResourceKeyList)
        {
            Plant plant = m_plantManager.GetByExternalId(resourceKeyExternal.PlantExternalId);
            if (plant == null)
            {
                throw new TransmissionValidationException(a_t, "2628", new object[] { resourceKeyExternal.PlantExternalId });
            }

            Department dept = plant.Departments.GetByExternalId(resourceKeyExternal.DepartmentExternalId);
            if (dept == null)
            {
                throw new TransmissionValidationException(a_t, "2629", new object[] { resourceKeyExternal.DepartmentExternalId, resourceKeyExternal.PlantExternalId });
            }

            Resource resource = dept.Resources.GetByExternalId(resourceKeyExternal.ResourceExternalId);
            if (resource == null)
            {
                throw new TransmissionValidationException(a_t, "2630", new object[] { resourceKeyExternal.ResourceExternalId, resourceKeyExternal.DepartmentExternalId, resourceKeyExternal.PlantExternalId });
            }

            resources[count] = resource;
            count++;
        }

        RecurringCapacityIntervalManager.RecurringCapacityIntervalTHandler(a_udfManager, a_rciDef, resources, this, a_t, a_affectedResources, a_dataChanges);
    }
    #endregion

    #region Job Actions
    private void ProcessJobT(JobT a_jobT, UserFieldDefinitionManager a_udfManager, IScenarioDataChanges a_dataChanges, ScenarioExceptionInfo a_sei)
    {
        m_jobManager.Receive(a_jobT, a_udfManager, m_capabilityManager, this, a_dataChanges, a_sei);
    }

    private void ProcessAnchorJobsT(ScenarioDetailAnchorJobsT t, IScenarioDataChanges a_dataChanges)
    {
        LinkedList<Job> jobList = new ();

        foreach (BaseId jobId in t.Jobs)
        {
            Job job = JobManager.GetById(jobId);
            if (job == null)
            {
                throw new TransmissionValidationException(t, "2631", new object[] { jobId.NextId.ToString() });
            }

            jobList.AddLast(job);
        }

        foreach (Job job in jobList)
        {
            job.Anchor(t.Anchor, ScenarioOptions);
            a_dataChanges.JobChanges.UpdatedObject(job.Id);
        }
    }
    
    private void ProcessResetJobJITAndSubJobNeedDates(ScenarioDetailJobResetJITAndSubJobNeedDateT a_t, IScenarioDataChanges a_dataChanges)
    {
        if (a_t.RecalculateAllSettings)
        {
            RecalculateJITStartTimes(a_dataChanges);
            return;
        }
        
        SortedList<long, List<ManufacturingOrder>> sortedMoCollection = new ();
        MOKeyList.Node node = a_t.ManufacturingOrders.First;
        while (node != null)
        {
            Job destJob = JobManager.GetById(node.Data.JobId);
            ManufacturingOrder destMO = destJob.ManufacturingOrders.GetById(node.Data.MOId);
            if (destMO.Scheduled)
            {
                if (sortedMoCollection.TryGetValue(destMO.ScheduledEnd, out List<ManufacturingOrder> manufacturingOrders))
                {
                    manufacturingOrders.Add(destMO);
                }
                else
                {
                    sortedMoCollection.Add(destMO.ScheduledEnd, new List<ManufacturingOrder>{destMO});
                }
            }
            else
            {
                if (sortedMoCollection.TryGetValue(destMO.NeedDateTicks, out List<ManufacturingOrder> manufacturingOrders))
                {
                    manufacturingOrders.Add(destMO);
                }
                else
                {
                    sortedMoCollection.Add(destMO.NeedDateTicks, new List<ManufacturingOrder> { destMO });
                }
            }

            node = node.Next;
        }

        foreach (KeyValuePair<long, List<ManufacturingOrder>> moCollections in sortedMoCollection)
        {
            foreach (ManufacturingOrder destMO in moCollections.Value)
            {
                if (a_t.ResetSubJobNeedDates)
                {
                    for (int i = 0; i < destMO.OperationManager.Count; i++)
                    {
                        InternalOperation operation = (InternalOperation)destMO.OperationManager.GetByIndex(i);
                        InternalActivity destinationActivity = operation.Scheduled ? operation.GetLeadActivity() : operation.Activities.GetByIndex(0);

                        ScenarioOptions.ESubJobNeedDateResetPoint needDatePoint;
                        if (a_t.SubJobNeedDateResetPoint != ScenarioOptions.ESubJobNeedDateResetPoint.None)
                        {
                            needDatePoint = a_t.SubJobNeedDateResetPoint;
                        }
                        else
                        {
                            //Use system settings. The value did not come from a user setting or override
                            needDatePoint = ScenarioOptions.SetSubJobNeedDatePoint;
                        }

                        destMO.Job.UpdateSubJobSettings(Clock, destinationActivity, ScenarioOptions.SetSubJobHotFlags, needDatePoint, ScenarioOptions.SetSubJobPriorities, out bool updatedSubJobNeedDatesTmp, ScenarioOptions, a_dataChanges);
                    }
                }
                else
                {
                    destMO.Job.CalculateJitTimes(Clock, destMO, false, new HashSet<BaseId>());
                    a_dataChanges.JobChanges.UpdatedObject(destMO.Job.Id);
                }
            }
        }
    }

    private void ProcessLockJobsT(ScenarioDetailLockJobsT t, IScenarioDataChanges a_dataChanges)
    {
        LinkedList<Job> jobList = new ();
        foreach (BaseId jobId in t.Jobs)
        {
            Job job = JobManager.GetById(jobId);

            if (job == null)
            {
                throw new TransmissionValidationException(t, "2632", new object[] { jobId.NextId.ToString() });
            }

            jobList.AddLast(job);
        }

        foreach (Job job in jobList)
        {
            job.Lock(t.Lockit);
            a_dataChanges.JobChanges.UpdatedObject(job.Id);
        }
    }

    private void ProcessHoldJobsT(ScenarioDetailHoldJobsT t, IScenarioDataChanges a_dataChanges)
    {
        LinkedList<Job> jobList = new ();
        foreach (BaseId jobId in t.Jobs)
        {
            Job job = JobManager.GetById(jobId);
            if (job == null)
            {
                throw new TransmissionValidationException(t, "2633", new object[] { jobId.NextId.ToString() });
            }

            jobList.AddLast(job);
        }

        foreach (Job job in jobList)
        {
            job.HoldIt(t.Holdit, t.HoldUntilDate, t.HoldReason);
            a_dataChanges.JobChanges.UpdatedObject(job.Id);
        }

        //Requires constraint changes
        a_dataChanges.FlagConstraintChanges(BaseId.NULL_ID);
    }

    private void ProcessSetJobCommitmentsAndPrioritiesT(ScenarioDetailSetJobPropertiesT a_t, IScenarioDataChanges a_dataChanges)
    {
        if (a_t.Jobs.Count > 0) //else nothing to do
        {
            foreach (BaseId jobId in a_t.Jobs)
            {
                bool updated = false;
                bool recalculateEligibility = false;
                bool recalculateJit = false;
                Job job = JobManager.GetById(jobId);
                if (job == null)
                {
                    throw new PTValidationException(string.Format("Job not found. Id '{0}'".Localize(), jobId));
                }

                if (a_t.CommitmentsSet && job.Commitment != a_t.Commitment)
                {
                    job.Commitment = a_t.Commitment;
                    updated = true;
                }

                if (a_t.ClassificationSet && job.Classification != a_t.Classification)
                {
                    job.Classification = a_t.Classification;
                    updated = true;
                }

                if (a_t.PrioritiesSet && job.Priority != a_t.Priority)
                {
                    job.Priority = a_t.Priority;
                    updated = true;
                }

                if (a_t.SchedulabilitySet && job.DoNotSchedule != a_t.DoNotSchedule)
                {
                    a_dataChanges.FlagProductionChanges(job.Id);
                    job.DoNotSchedule = a_t.DoNotSchedule;
                    updated = true;
                }

                if (a_t.PrintedSet && job.Printed != a_t.Printed)
                {
                    job.Printed = a_t.Printed;
                    updated = true;
                }

                if (a_t.RevenueSet && job.Revenue != a_t.Revenue)
                {
                    job.Revenue = a_t.Revenue;
                    updated = true;
                }

                if (a_t.MoQtySet && job.ManufacturingOrders[0].RequiredQty != a_t.MoQty)
                {
                    job.ManufacturingOrders[0].SetRequiredQty(Clock, a_t.MoQty, m_productRuleManager);
                    a_dataChanges.FlagConstraintChanges(job.Id);
                    recalculateEligibility = true;
                    recalculateJit = true;
                    updated = true;
                }

                if (a_t.HotSet && job.Hot != a_t.Hot)
                {
                    job.Hot = a_t.Hot;
                    updated = true;
                }

                if (a_t.ImportanceSet && job.Importance != a_t.Importance)
                {
                    job.Importance = a_t.Importance;
                    updated = true;
                }

                if (a_t.CancelledSet && job.Cancelled != a_t.Cancelled)
                {
                    job.Cancelled = a_t.Cancelled;
                    updated = true;
                }

                if (a_t.InvoicedSet && job.Invoiced != a_t.Invoiced)
                {
                    job.Invoiced = a_t.Invoiced;
                    updated = true;
                }

                if (a_t.ReviewedSet && job.Reviewed != a_t.Reviewed)
                {
                    job.Reviewed = a_t.Reviewed;
                    updated = true;
                }

                if (a_t.ShippedSet && job.Shipped != a_t.Shipped)
                {
                    job.Shipped = a_t.Shipped;
                    updated = true;
                }

                if (a_t.NeedDateSet && job.NeedDateTime != a_t.NeedDate)
                {
                    job.NeedDateTime = a_t.NeedDate;
                    recalculateJit = true;
                    updated = true;
                }

                if (a_t.NotesSet && job.Notes != a_t.Notes)
                {
                    job.Notes = a_t.Notes;
                    updated = true;
                }

                if (a_t.DoNotDeleteSet && job.DoNotDelete != a_t.DoNotDelete)
                {
                    job.DoNotDelete = a_t.DoNotDelete;
                    updated = true;
                }

                if (a_t.AutoFinishSet && a_t.AutoFinish)
                {
                    List<InternalActivity> internalActivities = job.GetActivities();
                    foreach (InternalActivity act in internalActivities)
                    {
                        if (act.Scheduled)
                        {
                            act.AutoFinish("AutoFinished by user.".Localize());
                            updated = true;
                        }
                    }

                    a_dataChanges.FlagConstraintChanges(job.Id);
                    recalculateEligibility = true;
                }

                if (a_t.JobColorSet && job.Commitment != a_t.Commitment)
                {
                    job.ColorCode = a_t.JobColor;
                    updated = true;
                }

                if (a_t.NameSet && job.Commitment != a_t.Commitment)
                {
                    job.Name = a_t.Name;
                    updated = true;
                }

                if (a_t.JobDescriptionSet && job.Commitment != a_t.Commitment)
                {
                    job.Description = a_t.JobDescription;
                    updated = true;
                }

                if (a_t.AnchorDateSet && job.Commitment != a_t.Commitment)
                {
                    foreach (ManufacturingOrder mo in job.ManufacturingOrders)
                    {
                        for (int b = 0; b < mo.OperationManager.Count; b++)
                        {
                            InternalOperation op = mo.OperationManager.GetByIndex(b) as InternalOperation;
                            foreach (InternalActivity act in op.Activities)
                            {
                                if (act.AnchorDate != a_t.AnchorDate)
                                {
                                    act.ExternalAnchor(a_t.AnchorDate.Ticks);
                                    updated = true;
                                }
                            }
                        }
                    }
                }

                if (a_t.CanSpanPlantsSet && job.CanSpanPlants != a_t.CanSpanPlants)
                {
                    job.CanSpanPlants = a_t.CanSpanPlants;

                    foreach (ManufacturingOrder mo in job.ManufacturingOrders)
                    {
                        mo.CanSpanPlants = a_t.CanSpanPlants;
                        updated = true;
                    }

                    recalculateEligibility = true;
                }

                if (a_t.MOQtyRatioSet)
                {
                    job.ManufacturingOrders[0].SetRequiredQty(Clock, job.ManufacturingOrders[0].RequiredQty * a_t.MOQtyRatio, m_productRuleManager);
                    a_dataChanges.FlagConstraintChanges(job.Id);
                    recalculateEligibility = true;
                    updated = true;
                }

                if (updated)
                {
                    a_dataChanges.JobChanges.UpdatedObject(job.Id);
                }

                if (recalculateEligibility)
                {
                    job.ComputeEligibilityAndUnscheduleIfIneligible(m_productRuleManager);
                }

                if (recalculateJit)
                {
                    //Recalculate this job's Jit times since it could have been affected by the changes
                    job.CalculateJitTimes(Clock, false);
                }
            }
        }
    }
    #endregion Job Actions

    #region MO Actions
    private void ProcessAnchorMOsT(ScenarioDetailAnchorMOsT t, IScenarioDataChanges a_dataChanges)
    {
        MOKeyList.Node node = t.MOs.First;

        // Validation. Verify that the MOs specified in the transmission exist.
        while (node != null)
        {
            ManufacturingOrderKey moKey = node.Data;
            Job job = JobManager.GetById(moKey.JobId);
            if (job == null)
            {
                throw new TransmissionValidationException(t, "2634", new object[] { moKey.JobId });
            }

            ManufacturingOrder mo = job.ManufacturingOrders.GetById(moKey.MOId);
            if (mo == null)
            {
                throw new TransmissionValidationException(t, "2635", new object[] { moKey.JobId, moKey.MOId });
            }

            node = node.Next;
        }

        // Anchor the MOs.
        node = t.MOs.First;

        while (node != null)
        {
            ManufacturingOrderKey moKey = node.Data;
            Job job = JobManager.GetById(moKey.JobId);
            ManufacturingOrder mo = job.ManufacturingOrders.GetById(moKey.MOId);

            if (job.Scheduled)
            {
                mo.Anchor(t.Anchor, ScenarioOptions);
                a_dataChanges.JobChanges.UpdatedObject(job.Id);
            }

            node = node.Next;
        }
    }

    private void ProcessLockMOsT(ScenarioDetailLockMOsT t, IScenarioDataChanges a_dataChanges)
    {
        MOKeyList.Node node = t.MOs.First;
        while (node != null)
        {
            ManufacturingOrderKey moKey = node.Data;
            Job job = JobManager.GetById(moKey.JobId);
            if (job == null)
            {
                throw new TransmissionValidationException(t, "2636", new object[] { moKey.JobId });
            }

            ManufacturingOrder mo = job.ManufacturingOrders.GetById(moKey.MOId);
            if (mo == null)
            {
                throw new TransmissionValidationException(t, "2637", new object[] { moKey.JobId, moKey.MOId });
            }

            node = node.Next;
        }

        node = t.MOs.First;

        while (node != null)
        {
            ManufacturingOrderKey moKey = node.Data;
            Job job = JobManager.GetById(moKey.JobId);
            ManufacturingOrder mo = job.ManufacturingOrders.GetById(moKey.MOId);
            mo.Lock(t.Lockit);
            a_dataChanges.JobChanges.UpdatedObject(job.Id);

            node = node.Next;
        }
    }

    private void ProcessLockToPathMOsT(ScenarioDetailLockMOsToPathT t, IScenarioDataChanges a_dataChanges)
    {
        MOKeyList.Node node = t.MOs.First;
        while (node != null)
        {
            ManufacturingOrderKey moKey = node.Data;
            Job job = JobManager.GetById(moKey.JobId);
            if (job == null)
            {
                throw new TransmissionValidationException(t, "2636", new object[] { moKey.JobId });
            }

            ManufacturingOrder mo = job.ManufacturingOrders.GetById(moKey.MOId);
            if (mo == null)
            {
                throw new TransmissionValidationException(t, "2637", new object[] { moKey.JobId, moKey.MOId });
            }

            node = node.Next;
        }

        node = t.MOs.First;

        while (node != null)
        {
            ManufacturingOrderKey moKey = node.Data;
            Job job = JobManager.GetById(moKey.JobId);
            ManufacturingOrder mo = job.ManufacturingOrders.GetById(moKey.MOId);
            mo.LockToCurrentAlternatePath = t.Lockit;
            a_dataChanges.JobChanges.UpdatedObject(job.Id);

            node = node.Next;
        }
    }

    private void ProcessHoldMOsT(ScenarioDetailHoldMOsT t, IScenarioDataChanges a_dataChanges)
    {
        MOKeyList.Node node = t.MOs.First;

        while (node != null)
        {
            ManufacturingOrderKey moKey = node.Data;
            Job job = JobManager.GetById(moKey.JobId);
            if (job == null)
            {
                throw new TransmissionValidationException(t, "2638", new object[] { moKey.JobId });
            }

            ManufacturingOrder mo = job.ManufacturingOrders.GetById(moKey.MOId);
            if (mo == null)
            {
                throw new TransmissionValidationException(t, "2639", new object[] { moKey.JobId, moKey.MOId });
            }

            node = node.Next;
        }

        node = t.MOs.First;

        while (node != null)
        {
            ManufacturingOrderKey moKey = node.Data;
            Job job = JobManager.GetById(moKey.JobId);
            ManufacturingOrder mo = job.ManufacturingOrders.GetById(moKey.MOId);

            mo.HoldIt(t.Holdit, t.HoldUntilDate, t.HoldReason);
            a_dataChanges.JobChanges.UpdatedObject(job.Id);
            node = node.Next;
        }

        //Requires constraint change adjustments
        a_dataChanges.FlagConstraintChanges(BaseId.NULL_ID);
    }

    // !ALTERNATE_PATH!; ScenarioDetailAlternatePathLockT
    private void ProcessLockAlternatePathT(ScenarioDetailAlternatePathLockT a_t)
    {
        Job job = JobManager.GetById(a_t.JobId);
        job.Receive(a_t);
        if (job == null)
        {
            throw new PTValidationException("2280");
        }
    }
    #endregion MO Actions

    #region Operation Actions
    private void ProcessAnchorOperationsT(ScenarioDetailAnchorOperationsT t, IScenarioDataChanges a_dataChanges)
    {
        OperationKeyList.Node node = t.Operations.First;
        while (node != null)
        {
            OperationKey opKey = node.Data;
            Job job = JobManager.GetById(opKey.JobId);
            if (job == null)
            {
                throw new TransmissionValidationException(t, "2640", new object[] { opKey.JobId });
            }

            ManufacturingOrder mo = job.ManufacturingOrders.GetById(opKey.MOId);
            if (mo == null)
            {
                throw new TransmissionValidationException(t, "2641", new object[] { opKey.JobId, opKey.MOId });
            }

            BaseOperation op = (BaseOperation)mo.OperationManager.OperationsHashInternal[opKey.OperationId];
            if (op == null)
            {
                throw new TransmissionValidationException(t, "2642", new object[] { opKey.JobId, opKey.MOId, opKey.OperationId });
            }

            node = node.Next;
        }

        node = t.Operations.First;
        while (node != null)
        {
            OperationKey opKey = node.Data;
            Job job = JobManager.GetById(opKey.JobId);
            ManufacturingOrder mo = job.ManufacturingOrders.GetById(opKey.MOId);
            BaseOperation op = (BaseOperation)mo.OperationManager.OperationsHashInternal[opKey.OperationId];

            if (op is InternalOperation operation)
            {
                operation.SetAnchor(t.Anchor, ScenarioOptions);
                a_dataChanges.JobChanges.UpdatedObject(job.Id);
            }

            node = node.Next;
        }
    }

    private void ProcessLockOperationsT(ScenarioDetailLockOperationsT t, IScenarioDataChanges a_dataChanges)
    {
        OperationKeyList.Node node = t.Operations.First;

        while (node != null)
        {
            OperationKey opKey = node.Data;
            Job job = JobManager.GetById(opKey.JobId);
            if (job == null)
            {
                throw new TransmissionValidationException(t, "2643", new object[] { opKey.JobId });
            }

            ManufacturingOrder mo = job.ManufacturingOrders.GetById(opKey.MOId);
            if (mo == null)
            {
                throw new TransmissionValidationException(t, "2644", new object[] { opKey.JobId, opKey.MOId });
            }

            BaseOperation op = (BaseOperation)mo.OperationManager.OperationsHashInternal[opKey.OperationId];
            if (op == null)
            {
                throw new TransmissionValidationException(t, "2645", new object[] { opKey.JobId, opKey.MOId, opKey.OperationId });
            }

            node = node.Next;
        }

        node = t.Operations.First;

        while (node != null)
        {
            OperationKey opKey = node.Data;
            Job job = JobManager.GetById(opKey.JobId);
            ManufacturingOrder mo = job.ManufacturingOrders.GetById(opKey.MOId);
            BaseOperation op = (BaseOperation)mo.OperationManager.OperationsHashInternal[opKey.OperationId];
            if (op is InternalOperation operation)
            {
                operation.Lock(t.Lockit);
                a_dataChanges.JobChanges.UpdatedObject(job.Id);
            }

            node = node.Next;
        }
    }

    private void ProcessHoldOperationsT(ScenarioDetailHoldOperationsT t, IScenarioDataChanges a_dataChanges)
    {
        OperationKeyList.Node node = t.Operations.First;

        while (node != null)
        {
            OperationKey opKey = node.Data;
            Job job = JobManager.GetById(opKey.JobId);
            if (job == null)
            {
                throw new TransmissionValidationException(t, "2646", new object[] { opKey.JobId });
            }

            ManufacturingOrder mo = job.ManufacturingOrders.GetById(opKey.MOId);
            if (mo == null)
            {
                throw new TransmissionValidationException(t, "2647", new object[] { opKey.JobId, opKey.MOId });
            }

            BaseOperation op = (BaseOperation)mo.OperationManager.OperationsHashInternal[opKey.OperationId];
            if (op == null)
            {
                throw new TransmissionValidationException(t, "2648", new object[] { opKey.JobId, opKey.MOId, opKey.OperationId });
            }

            node = node.Next;
        }

        node = t.Operations.First;

        while (node != null)
        {
            OperationKey opKey = node.Data;
            Job job = JobManager.GetById(opKey.JobId);
            ManufacturingOrder mo = job.ManufacturingOrders.GetById(opKey.MOId);
            BaseOperation op = (BaseOperation)mo.OperationManager.OperationsHashInternal[opKey.OperationId];

            op.OnHold = t.Holdit;
            op.HoldReason = t.HoldReason;
            op.HoldUntil = t.HoldUntilDate;

            a_dataChanges.JobChanges.UpdatedObject(job.Id);

            node = node.Next;
        }

        //Requires constraint changes
        a_dataChanges.FlagConstraintChanges(BaseId.NULL_ID);
    }

    private void ProcessApiHoldT(ApiHoldT a_t, IScenarioDataChanges a_scenarioDataChanges)
    {
        JobManager.InitFastLookupByExternalId();

        foreach (ExternalIdObject externalIdObject in a_t.Objects)
        {
            Job job = JobManager.GetByExternalId(externalIdObject.JobExternalId);
            ManufacturingOrder mo = job.ManufacturingOrders.GetByExternalId(externalIdObject.MoExternalId);
            BaseOperation op = mo.OperationManager[externalIdObject.OperationExternalId];

            op.OnHold = a_t.Hold;
            op.HoldReason = a_t.HoldReason;
            op.HoldUntil = a_t.HoldUntilDate;
        }

        JobManager.DeInitFastLookupByExternalId();
        ConstraintsChangeAdjustment(a_t, a_scenarioDataChanges);
    }

    private void UnscheduleJobs(ApiUnscheduleT a_t, IScenarioDataChanges a_dataChanges)
    {
        try
        {
            bool jobUnscheduled = false;
            foreach (ExternalIdObject externalIdObject in a_t.Objects)
            {
                Job job = JobManager.GetByExternalId(externalIdObject.JobExternalId);
                //should not be null, validated in extra services
                if (job != null)
                {
                    if (job.Finished)
                    {
                        //skip
                    }
                    else //unschedule
                    {
                        // Create audit entry BEFORE unscheduling to capture pre-unscheduling state
                        if (job.Scheduled)
                        {
                            AuditEntry jobAuditEntry = new AuditEntry(job.Id, job);
                            job.Unschedule(false);
                            a_dataChanges.AuditEntry(jobAuditEntry);
                            a_dataChanges.JobChanges.UpdatedObject(job.Id);
                        }
                        else
                        {
                            job.Unschedule(false);
                        }

                        jobUnscheduled = true;
                    }
                }
            }

            if (jobUnscheduled)
            {
                TimeAdjustment(a_t);
            }
        }
        catch (SimulationValidationException e)
        {
            FireSimulationValidationFailureEvent(e, a_t);
            throw;
        }
    }

    private void ProcessApiLockT(ApiLockT a_t, IScenarioDataChanges a_scenarioDataChanges)
    {
        JobManager.InitFastLookupByExternalId();

        foreach (ExternalIdObject externalIdObject in a_t.Objects)
        {
            Job job = JobManager.GetByExternalId(externalIdObject.JobExternalId);
            ManufacturingOrder mo = job.ManufacturingOrders.GetByExternalId(externalIdObject.MoExternalId);
            InternalOperation op = (InternalOperation)mo.OperationManager[externalIdObject.OperationExternalId];
            foreach (InternalActivity act in op.Activities)
            {
                if (act.ExternalId == externalIdObject.ActivityExternalId)
                {
                    act.Lock(a_t.Lock);
                }
            }
        }

        JobManager.DeInitFastLookupByExternalId();
        ConstraintsChangeAdjustment(a_t, a_scenarioDataChanges);
    }

    private void ProcessApiAnchorT(ApiAnchorT a_t, IScenarioDataChanges a_scenarioDataChanges)
    {
        JobManager.InitFastLookupByExternalId();

        foreach (ExternalIdObject externalIdObject in a_t.Objects)
        {
            Job job = JobManager.GetByExternalId(externalIdObject.JobExternalId);
            ManufacturingOrder mo = job.ManufacturingOrders.GetByExternalId(externalIdObject.MoExternalId);
            InternalOperation op = (InternalOperation)mo.OperationManager[externalIdObject.OperationExternalId];
            InternalActivity activity = null;
            foreach (InternalActivity act in op.Activities)
            {
                if (act.ExternalId == externalIdObject.ActivityExternalId)
                {
                    activity = act;
                    break;
                }
            }

            //Should not be null, validated in Extra Services
            if (activity != null)
            {
                activity.Lock(a_t.Lock);

                if (a_t.AnchorDate != PTDateTime.MinDateTime)
                {
                    activity.ExternalAnchor(a_t.AnchorDate.Ticks);
                }
                else
                {
                    op.SetAnchor(activity, a_t.Anchor, ScenarioOptions);
                }
            }
        }

        JobManager.DeInitFastLookupByExternalId();
        ConstraintsChangeAdjustment(a_t, a_scenarioDataChanges);
    }
    #endregion Operation Actions

    #region Activity Actions
    private void ProcessAnchorActivitiesT(ScenarioDetailAnchorActivitiesT t, IScenarioDataChanges a_dataChanges)
    {
        LockAnchorValidation(t, t.Activitys);
        AnchorActivitiesHelper(t.Activitys, t.Anchor, a_dataChanges);
    }

    private void LockAnchorValidation(PTTransmission t, ActivityKeyList aActivities)
    {
        ActivityKeyList.Node node = aActivities.First;

        while (node != null)
        {
            ActivityKey activityKey = node.Data;
            Job job = JobManager.GetById(activityKey.JobId);
            if (job == null)
            {
                throw new TransmissionValidationException(t, "2649", new object[] { activityKey.JobId });
            }

            ManufacturingOrder mo = job.ManufacturingOrders.GetById(activityKey.MOId);
            if (mo == null)
            {
                throw new TransmissionValidationException(t, "2650", new object[] { activityKey.JobId, activityKey.MOId });
            }

            BaseOperation op = (BaseOperation)mo.OperationManager.OperationsHashInternal[activityKey.OperationId];
            if (op == null)
            {
                throw new TransmissionValidationException(t, "2651", new object[] { activityKey.JobId, activityKey.MOId, activityKey.OperationId });
            }

            node = node.Next;
        }
    }

    private void AnchorActivitiesHelper(ActivityKeyList aActivities, bool a_anchor, IScenarioDataChanges a_dataChanges)
    {
        ActivityKeyList.Node node = aActivities.First;

        while (node != null)
        {
            ActivityKey activityKey = node.Data;
            Job job = JobManager.GetById(activityKey.JobId);
            ManufacturingOrder mo = job.ManufacturingOrders.GetById(activityKey.MOId);
            BaseOperation op = (BaseOperation)mo.OperationManager.OperationsHashInternal[activityKey.OperationId];
            if (op is InternalOperation)
            {
                InternalOperation internalOp = (InternalOperation)op;
                InternalActivity internalActivity = internalOp.Activities[activityKey.ActivityId];
                internalOp.SetAnchor(internalActivity, a_anchor, ScenarioOptions);
                a_dataChanges.JobChanges.UpdatedObject(job.Id);
            }

            node = node.Next;
        }
    }

    private void ProcessLockActivitiesT(ScenarioDetailLockActivitiesT t, IScenarioDataChanges a_dataChanges)
    {
        LockAnchorValidation(t, t.Activitys);
        ProcessLockActivitiesHelper(t.Activitys, t.Lockit, a_dataChanges);
    }

    private void ProcessLockActivitiesHelper(ActivityKeyList aActivities, bool aLock, IScenarioDataChanges a_dataChanges)
    {
        ActivityKeyList.Node node = aActivities.First;

        while (node != null)
        {
            ActivityKey activityKey = node.Data;
            Job job = JobManager.GetById(activityKey.JobId);
            ManufacturingOrder mo = job.ManufacturingOrders.GetById(activityKey.MOId);
            BaseOperation op = (BaseOperation)mo.OperationManager.OperationsHashInternal[activityKey.OperationId];

            if (op is InternalOperation)
            {
                InternalOperation internalOp = (InternalOperation)op;
                InternalActivity internalActivity = internalOp.Activities[activityKey.ActivityId];
                internalActivity.Lock(aLock);
                a_dataChanges.JobChanges.UpdatedObject(job.Id);
            }

            node = node.Next;
        }
    }

    private void ProcessLockAndAnchorActivitiesT(ScenarioDetailLockAndAnchorActivitiesT t, IScenarioDataChanges a_dataChanges)
    {
        LockAnchorValidation(t, t.Activitys);
        ProcessLockActivitiesHelper(t.Activitys, t.SetLockAndAnchor, a_dataChanges);
        AnchorActivitiesHelper(t.Activitys, t.SetLockAndAnchor, a_dataChanges);
    }

    /// <summary>
    /// Update Activities. This doesn't Create or Remove any Activities.
    /// This is as an ERP update and is subject to protections that prevent updates.
    /// Causes validation exception if any Activities can't be found.
    /// </summary>
    private void ProcessActivityUpdateT(ActivityUpdateT a_t, IScenarioDataChanges a_dataChanges)
    {
        if (a_t == null)
        {
            return;
        }

        ApplicationExceptionList errList = new ();
        List<Activity> missingActivities = new ();

        try
        {
            for (int actIdx = 0; actIdx < a_t.Count; actIdx++)
            {
                try
                {
                    Activity actUpdate = a_t[actIdx];
                    bool activityFound = false;

                    Job job = m_jobManager.GetByExternalId(actUpdate.JobExternalId);
                    if (job != null && !a_dataChanges.JobChanges.Deleted.Contains(job.Id))
                    {
                        ManufacturingOrder mo = job.ManufacturingOrders.GetByExternalId(actUpdate.MoExternalId);

                        if (mo != null)
                        {
                            bool preserveRequiredQty = mo.PreserveRequiredQty;
                            InternalOperation op = mo.OperationManager[actUpdate.OpExternalId] as InternalOperation;

                            if (op != null)
                            {
                                InternalActivity existingAct = op.Activities.GetByExternalId(actUpdate.ExternalId);

                                if (existingAct != null)
                                {
                                    activityFound = true;
                                    JobDataSet jds = new JobDataSet();
                                    JobDataSetFiller filler = new JobDataSetFiller();
                                    filler.FillDataSet(jds, job, JobManager);
                                    JobT jobT = new JobT();
                                    JobT.InternalOperation opT = null;
                                    int errorsBefore = errList.Count;
                                    jobT.Fill(ref errList, jds, true);
                                    if (errList.Count > errorsBefore)
                                    {
                                        continue;
                                    }
                                    JobT.Job j = jobT[0];
                                    for (int i = 0; i < j.ManufacturingOrderCount; i++)
                                    {
                                        JobT.ManufacturingOrder moT = j[i];
                                        if (moT.ExternalId == mo.ExternalId)
                                        {
                                            opT = moT.GetOperation(op.ExternalId);
                                        }
                                    }

                                    InternalActivity newAct = new InternalActivity(existingAct.Id, this, op, opT, actUpdate, a_t.FromErp);
                                    bool updated = existingAct.Update(true, newAct, ScenarioOptions, preserveRequiredQty, null, true, true, actUpdate.NowFinishUtcTime, a_dataChanges);

                                    if (updated)
                                    {
                                        if (job.Template)
                                        {
                                            a_dataChanges.TemplateChanges.UpdatedObject(job.Id);
                                        }
                                        else
                                        {
                                            a_dataChanges.JobChanges.UpdatedObject(job.Id);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (!activityFound)
                    {
                        missingActivities.Add(actUpdate);
                    }
                }
                catch (PTHandleableException err)
                {
                    errList.Add(err);
                }
            }
        }
        catch (PTHandleableException err)
        {
            errList.Add(err);
        }
        finally
        {
            //Log an exception for the missing Activities. Don't throw an exception or PostProcessing will be skipped and errors could occur.
            if (missingActivities.Count > 0)
            {
                string errMsg = Localizer.GetErrorString("4120", new object[] { missingActivities.Count }); // 4120: {0} Activities were not found so they couldn't be updated
                if (missingActivities.Count > 20)
                {
                    errMsg += ". The first 20 are listed below".Localize();
                }

                errMsg += ":";

                int i = 0;
                foreach (Activity missingAct in missingActivities)
                {
                    i++;
                    errMsg += Environment.NewLine + missingAct;
                    if (i >= 20)
                    {
                        break;
                    }
                }

                if (missingActivities.Count > 20)
                {
                    errMsg += Environment.NewLine + "...";
                }

                errList.Add(new PTValidationException(errMsg));
            }

            if (errList.Count > 0)
            {
                ScenarioExceptionInfo sei = new ();
                sei.Create(this);
                m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
            }
        }
    }
    #endregion Activity Actions

    #region Block Actions
    /// <summary>
    /// Process a transmission that specifies a set of what to Resource Lock down to the activity's block.
    /// </summary>
    /// <param name="t">Specifies which activity's/blocks to lock.</param>
    private void ProcessLockBlocksT(ScenarioDetailLockBlocksT t, IScenarioDataChanges a_dataChanges)
    {
        BlockKeyList.Node node = t.Blocks.First;

        while (node != null)
        {
            BlockKey blockKey = node.Data;
            Job job = JobManager.GetById(blockKey.JobId);
            if (job == null)
            {
                throw new TransmissionValidationException(t, "2652", new object[] { blockKey.JobId });
            }

            ManufacturingOrder mo = job.ManufacturingOrders.GetById(blockKey.MOId);
            if (mo == null)
            {
                throw new TransmissionValidationException(t, "2653", new object[] { blockKey.JobId, blockKey.MOId });
            }

            BaseOperation op = (BaseOperation)mo.OperationManager.OperationsHashInternal[blockKey.OperationId];
            if (op == null)
            {
                throw new TransmissionValidationException(t, "2654", new object[] { blockKey.JobId, blockKey.MOId, blockKey.OperationId });
            }

            InternalOperation iOp = op as InternalOperation;
            if (iOp != null)
            {
                InternalActivity act = iOp.Activities[blockKey.ActivityId];
                if (!act.Scheduled || !act.ResourceRequirementBlockExists(blockKey.BlockId))
                {
                    throw new TransmissionValidationException(t, "2839", new object[] { job.Name, mo.Name, op.Name, act.ExternalId }, false);
                }
            }
            else
            {
                throw new TransmissionValidationException(t, "2840", new object[] { job.Name, mo.Name, op.Name }, false);
            }

            node = node.Next;
        }

        node = t.Blocks.First;

        while (node != null)
        {
            BlockKey blockKey = node.Data;
            Job job = JobManager.GetById(blockKey.JobId);
            ManufacturingOrder mo = job.ManufacturingOrders.GetById(blockKey.MOId);
            BaseOperation op = (BaseOperation)mo.OperationManager.OperationsHashInternal[blockKey.OperationId];
            if (op is InternalOperation)
            {
                InternalOperation internalOp = (InternalOperation)op;
                InternalActivity act = internalOp.Activities[blockKey.ActivityId];
                ResourceBlock block = act.GetResourceBlock(blockKey.BlockId);
                if (block != null)
                {
                    if (t.Lockit)
                    {
                        act.LockResourceRequirement(block.ResourceRequirementIndex);
                    }
                    else
                    {
                        act.UnlockResourceRequirement(block.ResourceRequirementIndex);
                    }

                    a_dataChanges.JobChanges.UpdatedObject(job.Id);
                }
            }

            node = node.Next;
        }
    }
    #endregion Block Actions

    /// <summary>
    /// Supports backwards compatibility with ItemTs. This is needed to support recordings
    /// </summary>
    /// <param name="a_itemT"></param>
    private void ProcessItemT(UserFieldDefinitionManager a_udfManager, ItemT a_itemT, IScenarioDataChanges a_dataChanges)
    {
        WarehouseT warehouseT = new ();

        warehouseT.ItemsList = a_itemT.Nodes;
        warehouseT.AutoDeleteItems = a_itemT.AutoDeleteMode;
        ScenarioExceptionInfo sei = new ();
        sei.Create(this);

        ItemManager.Receive(this, a_dataChanges, warehouseT, JobManager, WarehouseManager, PurchaseToStockManager, ProductRuleManager, SalesOrderManager, TransferOrderManager, a_udfManager, sei);
    }

    /// <summary>
    /// Processes import transmission, calling Process for all contained transmissions.
    /// This is used to process multiple transmissions as a batch without additional trasmission processing for each.
    /// </summary>
    /// <param name="a_t"></param>
    private void ProcessImportT(ImportT a_t, ref List<ScenarioBaseT> r_postProcessHash, IScenarioDataChanges a_dataChanges)
    {
        Stopwatch importTimer = Stopwatch.StartNew();
        if (PTSystem.EnableDiagnostics)
        {
            PTSystem.SystemLogger.Log("Import Logging", $"Beginning process of Import T {a_t.Count}");
        }
        for (int cI = 0; cI < a_t.Count; cI++)
        {
            ScenarioBaseT t = a_t[cI];
            if (PTSystem.EnableDiagnostics)
            {
                PTSystem.SystemLogger.Log("Import Logging", $"Processing ImportT Transmission {cI} / {a_t.Count} type = {t.GetType().Name}");
            }

            t.SetTimeStamp(a_t.TimeStamp); //Stored transmissions where never broadcast, so the timestamp was not set.
            t.ReplayForUndoRedo = a_t.ReplayForUndoRedo;
            ScenarioExceptionInfo sei = new ();
            sei.Create(this);

            if (!(t is UserT) && !(t is UserFieldDefinitionT)) //UserT will be processed in UserManager, UserFieldDefinitionT is processed in ScenarioManager
            {
                ProcessT(t, ref r_postProcessHash, a_dataChanges, sei);
            }
        }

        using (_scenario.ScenarioEventsLock.EnterRead(out ScenarioEvents se))
        {
            if (PTSystem.EnableDiagnostics)
            {
                PTSystem.SystemLogger.Log("Import Logging", $"ImportT Transmission firing import scenario complete event {a_t.TransmissionNbr}");
            }

            se.FireImportScenarioCompleteEvent(a_t);
        }
        importTimer.Stop();
        RecordActionDuration("Import", importTimer.ElapsedMilliseconds, a_dataChanges);
    }

    private void ProcessInventoryTransferRuleT(InventoryTransferRulesT a_t)
    {
        InventoryTransferRuleManager.Receive(a_t, this);
    }
}
