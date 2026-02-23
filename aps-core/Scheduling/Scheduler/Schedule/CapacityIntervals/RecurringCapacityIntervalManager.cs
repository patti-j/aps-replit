using System.Collections;
using System.ComponentModel;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.Common.Localization;
using PT.ERPTransmissions;
using PT.Scheduler.ErrorReporting;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of RecurringCapacityInterval objects.
/// </summary>
[Serializable]
public class RecurringCapacityIntervalManager : ScenarioBaseObjectManager<RecurringCapacityInterval>, IPTSerializable
{
    #region IPTSerializable Members
    public RecurringCapacityIntervalManager(IReader reader, BaseIdGenerator idGen)
        : base(idGen)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                RecurringCapacityInterval rci = new (reader);
                Add(rci);
            }
        }
    }

    public new const int UNIQUE_ID = 336;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    public class RecurringCapacityIntervalManagerException : PTException
    {
        public RecurringCapacityIntervalManagerException(string message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(message, a_stringParameters, a_appendHelpUrl) { }
    }
    #endregion

    #region Construction
    public RecurringCapacityIntervalManager(BaseIdGenerator idGen)
        : base(idGen) { }
    #endregion

    #region Restore References
    internal void RestoreReferences(ScenarioDetail a_sd)
    {
        for (int i = 0; i < Count; i++)
        {
            RecurringCapacityInterval rci = GetByIndex(i);
            rci.RestoreReferences(a_sd);
        }
    }
    #endregion

    #region RecurringCapacityInterval Edit Functions
    internal RecurringCapacityInterval GetDefaultRCI(ScenarioDetail sd, IScenarioDataChanges a_dataChanges)
    {
        if (Count == 0)
        {
            RecurringCapacityInterval rci = AddDefaultOnlineInterval(sd);
            ScenarioEvents se;
            using (sd._scenario.ScenarioEventsLock.EnterRead(out se))
            {
                a_dataChanges.RecurringCapacityIntervalChanges.AddedObject(rci.Id);
            }
        }

        return this[0];
    }

    /// <summary>
    /// Creates an Online RCI for use by new Resources.
    /// </summary>
    /// <returns></returns>
    private RecurringCapacityInterval AddDefaultOnlineInterval(ScenarioDetail sd)
    {
        //Create a default rci
        RecurringCapacityInterval rci = new (NextID(), sd.Clock);
        rci.Name = "First Shift".Localize();
        //Get next weekday after clock at 7 am.
        DateTime displayClock = sd.ClockDate;
        DateTime start = displayClock;
        if (displayClock.Hour > 7 || (displayClock.Hour == 7 && displayClock.Minute > 0))
        {
            displayClock = displayClock.AddDays(1);
        }

        start = new DateTime(displayClock.Year, displayClock.Month, displayClock.Day, 7, 0, 0);

        if (start.DayOfWeek == DayOfWeek.Saturday)
        {
            start = start.AddDays(2);
        }
        else if (start.DayOfWeek == DayOfWeek.Sunday)
        {
            start = start.AddDays(1);
        }

        rci.StartDateTime = start;
        rci.EndDateTime = start.AddHours(8);
        rci.Recurrence = CapacityIntervalDefs.recurrences.Weekly;
        rci.Saturday = false;
        rci.Sunday = false;
        rci.Monday = true;
        rci.Tuesday = true;
        rci.Wednesday = true;
        rci.Thursday = true;
        rci.Friday = true;
        rci.CanDelete = true;
        rci.CanDragAndResize = true;

        rci.Expand(sd.GetPlanningHorizonEnd(), sd.Clock);
        Add(rci);
        return rci;
    }

    private RecurringCapacityInterval AddCopy(BaseId originalId, ScenarioDetail sd)
    {
        RecurringCapacityInterval rci = GetById(originalId);
        RecurringCapacityInterval newRci = new (NextID(), rci);
        newRci.Name = rci.Name + " Copy".Localize();
        newRci.ExternalId = ExternalBaseIdObject.MakeExternalId(newRci.Id.Value);
        Add(newRci);
        newRci.Expand(sd.GetPlanningHorizonEnd(), sd.Clock);
        return newRci;
    }

    /// <summary>
    /// Creates a new Recurring Capacity Interval using the specified CapacityInterval.
    /// </summary>
    internal RecurringCapacityInterval AddConvert(UserFieldDefinitionManager a_udfManager, RecurringCapacityIntervalConvertT a_t, CapacityInterval a_ci, IScenarioDataChanges a_dataChanges)
    {
        RecurringCapacityInterval rci = new (a_udfManager, a_t.recurringCapacityInterval, NextID(), m_scenarioDetail.ClockDate, m_scenarioDetail.GetPlanningHorizonEnd(), a_t, a_dataChanges);
        rci.Expand(m_scenarioDetail.GetPlanningHorizonEnd(), m_scenarioDetail.Clock);
        Add(rci);
        //Set the references between the CalendarResources and the new CapacityInterval
        for (int i = 0; i < a_ci.CalendarResources.Count; i++)
        {
            InternalResource r = a_ci.CalendarResources[i];
            if (r.AddRecurringCapacityInterval(rci))
            {
                r.RegenerateCapacityProfile(m_scenarioDetail.GetPlanningHorizonEnd().Ticks, true);
            }
        }

        return rci;
    }

    private RecurringCapacityInterval Delete(RecurringCapacityInterval a_rci)
    {
        using (m_scenario.ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
        {
            a_rci.DeleteFromResources(sd.GetPlanningHorizonEndTicks());
        }

        Remove(a_rci.Id); //Now remove it from the Manager.
        return a_rci;
    }

    /// <summary>
    /// Deletes the Expansion of Recurring Capacity Intervals that end on and before the Clock time and removes them from the Resources.
    /// Also deletes any RCIs whose EndDate is before the Clock or whose max expansion count has been reached.
    /// Also regenerate RCIs whose end time is dependent on the PlanningHorizon end and thus the Clock.
    /// </summary>
    /// <param name="a_newClockTime"></param>
    /// <param name="a_sd"></param>
    /// <param name="a_dataChanges"></param>
    internal void AdjustForChangedHorizon(long a_newClockTime, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            RecurringCapacityInterval rci = this[i];

            //Convert for Actuals -- even if not using this feature we need to store them for potential use later.
            IList<CapacityInterval> addedCapacityIntervalsForActuals;
            ConvertRecurringToNonRecurringForActuals(rci, a_sd, a_newClockTime, out addedCapacityIntervalsForActuals);
            for (int cI = 0; cI < addedCapacityIntervalsForActuals.Count; cI++)
            {
                a_dataChanges.CapacityIntervalChanges.AddedObject(addedCapacityIntervalsForActuals[cI].Id);
            }

            if (addedCapacityIntervalsForActuals.Count > 0) //made some changes so Resources were affected
            {
                for (int raI = 0; raI < rci.CalendarResources.Count; raI++)
                {
                    InternalResource nextRes = rci.CalendarResources[raI];
                    if (!a_dataChanges.MachineChanges.Updated.Contains(nextRes.Id))
                    {
                        a_dataChanges.MachineChanges.UpdatedObject(nextRes.Id);
                    }
                }
            }

            DateTime newClockDate = new (a_newClockTime);
            //Update the RCI with the new clock time.  This actually ill only affect RCIs that end based on time.
            DateTime planningHorizonEnd = newClockDate.Add(m_scenarioDetail.ScenarioOptions.PlanningHorizon);

            if (rci.RecurrenceEndType == CapacityIntervalDefs.recurrenceEndTypes.NoEndDate || rci.RecurrenceEndDateTime >= newClockDate)
            {
                //If there is an occurrence that spans the clock then use this as the new recurrence start time so that the current occurrence is not lost
                RecurringCapacityInterval.RCIExpansion currentExpansion = null;
                RecurringCapacityInterval.RCIExpansion previousExpansion = null;
                foreach (RecurringCapacityInterval.RCIExpansion expansion in rci)
                {
                    if (expansion.Start < newClockDate && expansion.End > newClockDate)
                    {
                        currentExpansion = expansion;
                        break;
                    }

                    //checks if the previous occurrence ends at the new clock date then uses the current occurrence as the start of the recurrence,
                    //so we don't end up generating an occurrence which may have already been handled by a capacity interval actual
                    if (previousExpansion?.End == newClockDate)
                    {
                        currentExpansion = expansion;
                        break;
                    }

                    if (expansion.Start >= newClockDate)
                    {
                        //This is the first expansion past the clock, this should be the new recurring capacity interval start date. 
                        currentExpansion = expansion;
                        break;
                    }

                    previousExpansion = expansion;
                }

                if (currentExpansion != null)
                {
                    rci.StartDateTime = currentExpansion.Start;
                    rci.EndDateTime = currentExpansion.End;
                }
                else if (newClockDate > rci.StartDateTime) //Startdate might be in the future
                {
                    //No Longer necessary. RecurrenceStart has been removed and the StartTime of the first online recurring interval is used as the start time of the recurrence
                    //rci.StartDateTime = newClockDate;
                }
                
                rci.ResetPlanningHorizon(planningHorizonEnd, a_newClockTime);
                rci.RegenerateCapacityProfiles(a_sd.GetPlanningHorizonEndTicks(), true);
                a_dataChanges.RecurringCapacityIntervalChanges.UpdatedObject(rci.Id);
            }

            for (int resI = 0; resI < rci.CalendarResources.Count; resI++)
            {
                InternalResource calRes = rci.CalendarResources[resI];
                if (!a_dataChanges.MachineChanges.Updated.Contains(calRes.Id))
                {
                    a_dataChanges.MachineChanges.UpdatedObject(calRes.Id);
                }
            }

            //Purge old Expansions from the Resources. 					
            if (rci.EndDateTime.Ticks <= a_newClockTime) //may need to do some purging
            {
                rci.PurgeOldExpansionsFromResources(a_newClockTime);
            }

            if (rci.ExpansionsCount == 0) //remove the entire RCI.  Can't look at EndDate because that is the end date of the single ci, not the end of the recurrence.
            {
                rci.PurgeFromResources();
                Remove(rci.Id);
                a_dataChanges.RecurringCapacityIntervalChanges.DeletedObject(rci.Id);
            }
        }
    }

    /// <summary>
    /// Deletes all Recurring Capacity Intervals.
    /// </summary>
    public void Clear(IScenarioDataChanges a_dataChanges)
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            a_dataChanges.RecurringCapacityIntervalChanges.DeletedObject(this[i].Id);
            Delete(this[i]);
        }
    }
    #endregion

    #region Transmissions
    private void ValidateAdd(RecurringCapacityInterval ci)
    {
        if (Contains(ci.Id))
        {
            throw new RecurringCapacityIntervalManagerException("2743", new object[] { ci.Id.ToString() });
        }
    }

    private void ValidateResources(ResourceKeyList list)
    {
        ResourceKeyList.Node node = list.First;
        while (node != null)
        {
            ResourceKey key = node.Data;

            Resource newResource = m_scenarioDetail.PlantManager.GetResource(key);
            if (newResource == null)
            {
                throw new PTValidationException("2155", new object[] { key.Plant, key.Department, key.Resource });
            }

            node = node.Next;
        }
    }

    private void ValidateCopy(RecurringCapacityIntervalCopyT t)
    {
        ValidateExistence(t.originalId);
    }

    private RecurringCapacityInterval ValidateDelete(BaseId id)
    {
        return ValidateExistence(id);
    }

    private void ValidateUpdate(RecurringCapacityIntervalUpdateT a_updateT)
    {
        ValidateExistence(a_updateT.RecurringCapacityIntervalIds[0]);
    }

    private InternalResource ValidateShare(RecurringCapacityIntervalShareT t, out InternalResource oldResource, out InternalResource newResource)
    {
        ValidateExistence(t.RecurringCapacityIntervalIds[0]);
        oldResource = FindResource(t.oldPlantId, t.oldDepartmentId, t.oldResourceId);

        if (oldResource == null)
        {
            throw new TransmissionValidationException(t, "2156");
        }

        newResource = FindResource(t.newPlantId, t.newDepartmentId, t.newResourceId);

        if (newResource == null)
        {
            throw new TransmissionValidationException(t, "2157");
        }

        return newResource;
    }

    private void ValidateMoveInTime(RecurringCapacityIntervalMoveInTimeT t)
    {
        ValidateExistence(t.RecurringCapacityIntervalIds[0]);

        if (t.newStartTime >= t.newEndTime)
        {
            throw new TransmissionValidationException(t, "2158");
        }
    }

    private void ValidateMove(RecurringCapacityIntervalMoveT t)
    {
        ValidateExistence(t.RecurringCapacityIntervalIds[0]);
        if (FindResource(t.newPlantId, t.newDepartmentId, t.newResourceId) == null)
        {
            throw new TransmissionValidationException(t, "4128", new object[] { t.newPlantId, t.newDepartmentId, t.newResourceId });
        }
    }

    private InternalResource FindResource(BaseId plantId, BaseId departmentId, BaseId resourceId)
    {
        ScenarioDetail sd;
        using (m_scenario.ScenarioDetailLock.EnterRead(out sd))
        {
            return sd.PlantManager.GetResource(new ResourceKey(plantId, departmentId, resourceId));
        }
    }

    public void Receive(UserFieldDefinitionManager a_udfManager, RecurringCapacityIntervalBaseT a_t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        ApplicationExceptionList errList = new ();

        try
        {
            RecurringCapacityInterval ci = null;
            if (a_t is RecurringCapacityIntervalDefaultT capacityIntervalDefaultT)
            {
                ci = AddDefaultOnlineInterval(a_sd);
                a_dataChanges.RecurringCapacityIntervalChanges.AddedObject(ci.Id);
                a_dataChanges.FlagConstraintChanges(ci.Id);
            }
            else if (a_t is RecurringCapacityIntervalCopyT intervalCopyT)
            {
                ci = AddCopy(intervalCopyT.originalId, a_sd);
                a_dataChanges.RecurringCapacityIntervalChanges.AddedObject(ci.Id);
                a_dataChanges.FlagConstraintChanges(ci.Id);
            }
            else
            {
                if (a_t is RecurringCapacityIntervalDeleteT)
                {
                    List<ValidationException> exceptions = new ();
                    foreach (BaseId capacityIntervalId in ((RecurringCapacityIntervalDeleteT)a_t).RecurringCapacityIntervalIds)
                    {
                        try
                        {
                            ci = ValidateDelete(capacityIntervalId);
                            CalendarResourcesCollection affectedResources = new ();
                            for (int i = 0; i < ci.CalendarResources.Count; i++) //Copy resource collection so they can be updated.
                            {
                                affectedResources.Add(ci.CalendarResources[i]);
                            }

                            ci = Delete(ci);
                            a_dataChanges.RecurringCapacityIntervalChanges.DeletedObject(ci.Id);
                        }
                        catch (Exception e)
                        {
                            if (e is ValidationException ve)
                            {
                                exceptions.Add(ve);
                            }
                        }
                    }

                    if (exceptions.Count > 0)
                    {
                        foreach (ValidationException validationException in exceptions)
                        {
                            errList.Add(validationException);
                        }
                    }

                    if (a_dataChanges.RecurringCapacityIntervalChanges.TotalDeletedObjects > 0)
                    {
                        a_dataChanges.FlagEligibilityChanges(ci.Id);
                        a_dataChanges.FlagConstraintChanges(ci.Id);
                    }
                }
                else if (a_t is RecurringCapacityIntervalDeleteAllT)
                {
                    CalendarResourcesCollection affectedResources = new ();
                    ArrayList emptyList = new ();

                    for (int i = Count - 1; i >= 0; i--)
                    {
                        RecurringCapacityInterval rci = this[i];
                        for (int j = 0; j < rci.CalendarResources.Count; j++) //Copy resource collection so they can be updated.
                        {
                            affectedResources.Add(rci.CalendarResources[j]);
                        }

                        Delete(rci);
                        a_dataChanges.RecurringCapacityIntervalChanges.DeletedObject(rci.Id);
                        a_dataChanges.FlagConstraintChanges(rci.Id);
                        a_dataChanges.FlagEligibilityChanges(rci.Id);
                    }

                }
                else if (a_t is RecurringCapacityIntervalUpdateMultiT multiT)
                {
                        //Now process each RCI definition in the Transmission
                        CalendarResourcesCollection affectedResources = new();

                        InitFastLookupByExternalId();

                    //First need to delete the RCIs that no longer exist if using auto-delete mode
                    if (multiT.AutoDelete)
                    {
                        
                        RecurringCapacityIntervalTHandler(multiT.Nodes, a_sd, affectedResources, a_dataChanges, multiT.AutoDelete, multiT.AutoDeleteResourceAssociations);

                    }

                    for (int i = 0; i < multiT.Count; i++)
                    {
                        RecurringCapacityIntervalDef capacityIntervalDef = multiT[i];
                        if (capacityIntervalDef.Rci != null && capacityIntervalDef.Rci.RecurrenceEndDateTime < a_sd.ClockDate && capacityIntervalDef.Rci.RecurrenceEndDateTimeSet)
                        {
                            return;
                        }

                        Resource[] resources = new Resource[capacityIntervalDef.ResourceKeyList.Count];

                        int count = 0;
                        foreach (ResourceKeyExternal resourceKeyExternal in capacityIntervalDef.ResourceKeyList)
                        {
                            Plant plant = a_sd.PlantManager.GetByExternalId(resourceKeyExternal.PlantExternalId);
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

                        RecurringCapacityIntervalTHandler(a_udfManager, capacityIntervalDef, resources, a_sd, multiT, affectedResources, a_dataChanges);
                    }

                    DeInitFastLookupByExternalId();

                    //Regenerate the Resources' capacity profiles and update the UI.
                    for (int i = 0; i < affectedResources.Count; i++)
                    {
                        affectedResources[i].RegenerateCapacityProfile(a_sd.GetPlanningHorizonEndTicks(), true);
                    }
                }
                else if (a_t is RecurringCapacityIntervalIdBaseT pt)
                {
                    //Only the RecurringCapacityIntervalDeleteT processes multiple objects for now. Here we can assume
                    //that the list only contains one object for now so get the first one.
                    ci = ValidateExistence(pt.RecurringCapacityIntervalIds[0]);

                    if (pt is RecurringCapacityIntervalShareT recurringCapacityIntervalShareT)
                    {
                        ValidateShare(recurringCapacityIntervalShareT, out InternalResource oldResource, out InternalResource newResource);

                        //If the new resource already has the interval then REMOVE it from the OLD resource.  This is how UNSHARING is done in the UI.
                        if (newResource.RecurringCapacityIntervals.Contains(ci.Id))
                        {
                            oldResource.RemoveRecurringCapacityInterval(ci);
                            oldResource.RegenerateCapacityProfile(a_sd.GetPlanningHorizonEndTicks(), true);
                            a_dataChanges.RecurringCapacityIntervalChanges.DeletedObject(ci.Id);
                        }

                        newResource.AddRecurringCapacityInterval(ci);
                        newResource.RegenerateCapacityProfile(a_sd.GetPlanningHorizonEndTicks(), true);

                        a_dataChanges.RecurringCapacityIntervalChanges.UpdatedObject(ci.Id);

                        a_dataChanges.FlagConstraintChanges(ci.Id);
                        a_dataChanges.FlagEligibilityChanges(ci.Id);
                    }
                    else if (pt is RecurringCapacityIntervalCopyToResourceT copyT)
                    {
                        InternalResource resource = FindResource(copyT.newPlantId, copyT.newDepartmentId, copyT.newResourceId);
                        InternalResource originalResource = FindResource(copyT.originalPlantId, copyT.originalDepartmentId, copyT.originalResourceId);
                        RecurringCapacityInterval rci = a_sd.RecurringCapacityIntervalManager.AddCopy(copyT.RecurringCapacityIntervalIds[0], a_sd);

                        resource.AddRecurringCapacityInterval(rci);
                        resource.RegenerateCapacityProfile(a_sd.GetPlanningHorizonEndTicks(), true);

                        CalendarResourcesCollection calResCol = new();
                        calResCol.Add(resource);
                        calResCol.Add(originalResource); //to fix block moved off of original resource in gantt.

                        a_dataChanges.RecurringCapacityIntervalChanges.AddedObject(rci.Id);
                        a_dataChanges.FlagEligibilityChanges(rci.Id);
                        a_dataChanges.FlagConstraintChanges(ci.Id);
                    }
                    else if (pt is RecurringCapacityIntervalMoveInTimeT moveInTimeT)
                    {
                        ci = ValidateExistence(moveInTimeT.RecurringCapacityIntervalIds[0]);
                        ValidateMoveInTime(moveInTimeT);
                        ci.Receive(a_udfManager, moveInTimeT, m_scenarioDetail.GetPlanningHorizonEnd(), a_sd.ClockDate, a_dataChanges);
                    }
                    else if (pt is RecurringCapacityIntervalMoveT moveT)
                    {
                        ci = ValidateExistence(moveT.RecurringCapacityIntervalIds[0]);
                        ValidateMove(moveT);
                        InternalResource newResource = FindResource(moveT.newPlantId, moveT.newDepartmentId, moveT.newResourceId);
                        InternalResource oldResource = FindResource(moveT.oldPlantId, moveT.oldDepartmentId, moveT.oldResourceId);
                        if (newResource != oldResource) //Moved resource so need to remove reference to ci from old resource and add to new one
                        {
                            newResource.AddRecurringCapacityInterval(ci);
                            oldResource.RemoveRecurringCapacityInterval(ci);
                            oldResource.RegenerateCapacityProfile(a_sd.GetPlanningHorizonEndTicks(), true);
                            a_dataChanges.RecurringCapacityIntervalChanges.DeletedObject(ci.Id);
                        }

                        ci.Receive(a_udfManager, moveT, m_scenarioDetail.GetPlanningHorizonEnd(), a_sd.ClockDate, a_dataChanges);
                        newResource.RegenerateCapacityProfile(a_sd.GetPlanningHorizonEndTicks(), true);

                        a_dataChanges.MachineChanges.UpdatedObject(newResource.Id);
                        a_dataChanges.MachineChanges.UpdatedObject(oldResource.Id);
                    }
                    else if (pt is RecurringCapacityIntervalUpdateT updateT)
                    {
                        ValidateUpdate(updateT);
                        ci.ValidateUpdate(updateT.recurringCapacityInterval);

                        if (updateT.ActOnSeries)
                        {
                            ci.Receive(a_udfManager, updateT, m_scenarioDetail.GetPlanningHorizonEnd(), a_sd.ClockDate, a_dataChanges);
                        }
                        else
                        {
                            Breakoff(updateT, ci, a_sd, a_dataChanges);
                            ci.RegenerateCapacityProfiles(a_sd.GetPlanningHorizonEndTicks(), true);
                            a_dataChanges.FlagConstraintChanges(ci.Id);
                        }
                    }
                    else if (pt is RecurringCapacityIntervalSetResourcesT setResourcesT)
                    {
                        RecurringCapacityInterval c = ValidateExistence(setResourcesT.RecurringCapacityIntervalIds[0]);
                        ValidateResources(setResourcesT.resources);
                        CalendarResourcesCollection affectedResources = new();

                        Hashtable oldResourceKeys = new();
                        Hashtable newResourceKeys = new();
                        for (int rI = 0; rI < ci.CalendarResources.Count; rI++)
                        {
                            oldResourceKeys.Add(ci.CalendarResources[rI].GetKey(), null);
                        }

                        ResourceKeyList.Node node = setResourcesT.resources.First;
                        while (node != null)
                        {
                            ResourceKey key = node.Data;
                            newResourceKeys.Add(key, null);
                            if (!oldResourceKeys.Contains(key))
                            {
                                Resource newResource = m_scenarioDetail.PlantManager.GetResource(key);
                                newResource.AddRecurringCapacityInterval(c);
                                affectedResources.Add(newResource);
                                if (newResource.Blocks?.Count > 0)
                                {
                                    a_dataChanges.FlagEligibilityChanges(newResource.Id);
                                }
                            }

                            node = node.Next;
                        }

                        //make sure the Resources that have this capacity interval should keep it
                        for (int rI = c.CalendarResources.Count - 1; rI >= 0; rI--)
                        {
                            InternalResource res = c.CalendarResources[rI];
                            if (!newResourceKeys.Contains(res.GetKey()))
                            {
                                res.RemoveRecurringCapacityInterval(c);
                                affectedResources.Add(res);
                            }
                        }

                        for (int i = 0; i < affectedResources.Count; ++i)
                        {
                            InternalResource affectedResource = affectedResources[i];
                            affectedResource.RegenerateCapacityProfile(a_sd.GetPlanningHorizonEndTicks(), true);
                            a_dataChanges.MachineChanges.UpdatedObject(affectedResource.Id);
                        }

                        a_dataChanges.RecurringCapacityIntervalChanges.UpdatedObject(ci.Id);

                        if (affectedResources.Count > 0)
                        {
                            a_dataChanges.FlagEligibilityChanges(ci.Id);
                            a_dataChanges.FlagConstraintChanges(ci.Id);
                        }
                    }
                }
            }
        }
        catch (PTHandleableException e)
        {
            errList.Add(new PTValidationException("Error processing CapacityInterval".Localize(), e));
        }

        if (errList.Count > 0)
        {
            ScenarioExceptionInfo sei = new ();
            sei.Create(a_sd);
            m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
        }
    }

    internal void Receive(CapacityIntervalConvertT t, IScenarioDataChanges a_dataChanges)
    {
        RecurringCapacityInterval rci = ValidateDelete(t.originalRecurringCapacityIntervalId);
        Delete(rci);
    }

    internal new void Add(RecurringCapacityInterval ci)
    {
        base.Add(ci);
    }
    #endregion

    #region ERP Transmissions
    internal void RecurringCapacityIntervalTHandler(List<RecurringCapacityIntervalDef> a_transmissionNodes, ScenarioDetail a_sd, CalendarResourcesCollection a_affectedResources, IScenarioDataChanges a_dataChanges, bool a_autoDelete, bool a_autoDeleteResources)
    {
        //Return if not in autodelete mode.  Nothing to do.
        if (!a_autoDelete && !a_autoDeleteResources)
        {
            return;
        }

        //Create a hashtable of the RCIs in the Transmission
        Hashtable transRCIs = new();
        for (int i = 0; i < a_transmissionNodes.Count; i++)
        {
            RecurringCapacityIntervalDef rciDef = a_transmissionNodes[i];
            if (transRCIs.ContainsKey(rciDef.Rci.ExternalId))
            {
                throw new PTValidationException("2997", new object[] { rciDef.Rci.ExternalId });
            }

            transRCIs.Add(rciDef.Rci.ExternalId, rciDef);
        }

        //Iterate through the existing RCIs and if not found in the Transmission then delete it
        for (int i = Count - 1; i >= 0; i--)
        {
            RecurringCapacityInterval rci = this[i];
            if (!transRCIs.Contains(rci.ExternalId))
            {
                if (a_autoDelete)
                {
                    //Store the affected Resources
                    for (int resI = 0; resI < rci.CalendarResources.Count; resI++)
                    {
                        InternalResource afRes = rci.CalendarResources[resI];
                        if (!a_affectedResources.Contains(afRes))
                        {
                            a_affectedResources.Add(afRes);
                        }
                    }

                    Delete(rci);
                    a_dataChanges.RecurringCapacityIntervalChanges.DeletedObject(rci.Id);
                    a_dataChanges.FlagConstraintChanges(rci.Id);
                    a_dataChanges.FlagEligibilityChanges(rci.Id);
                }
            }
            else if (a_autoDeleteResources)
            {
                RecurringCapacityIntervalDef ciDefT = (RecurringCapacityIntervalDef)transRCIs[rci.ExternalId];
                for (int resI = rci.CalendarResources.Count - 1; resI >= 0; --resI)
                {
                    InternalResource resource = rci.CalendarResources[resI];

                    //Need to know if resource is in transmission
                    bool transmissionContainsAssociation = ciDefT.ResourceKeyList.Contains(new ResourceKeyExternal(resource.Plant.ExternalId, resource.Department.ExternalId, resource.ExternalId));

                    if (!transmissionContainsAssociation)
                    {
                        if (!a_affectedResources.Contains(resource))
                        {
                            a_affectedResources.Add(resource);
                        }

                        a_dataChanges.RecurringCapacityIntervalChanges.UpdatedObject(rci.Id);
                        a_dataChanges.MachineChanges.UpdatedObject(resource.Id);
                        a_dataChanges.FlagConstraintChanges(resource.Id);
                        a_dataChanges.FlagEligibilityChanges(resource.Id);
                        resource.RemoveRecurringCapacityInterval(rci);
                    }
                }
            }
        }
    }

    internal void RecurringCapacityIntervalTHandler(UserFieldDefinitionManager a_udfManager, RecurringCapacityIntervalDef rciDef, Resource[] machines, ScenarioDetail sd, PTTransmission t, CalendarResourcesCollection a_affectedResources, IScenarioDataChanges a_dataChanges)
    {
        RecurringCapacityInterval rci = null;

        //Add or update each RCI
        rci = GetByExternalId(rciDef.Rci.ExternalId);
        if (rci != null) //so update it
        {
            for (int i = 0; i < machines.Length; i++)
            {
                Resource machine = machines[i];

                //If the interval does not already specify the machine then add it.
                if (!rci.CalendarResources.Contains(machine))
                {
                    machine.AddRecurringCapacityInterval(rci);
                    a_dataChanges.FlagEligibilityChanges(rci.Id);
                    if (!a_affectedResources.Contains(machine))
                    {
                        a_affectedResources.Add(machine);
                    }
                }
            }

            //TODO : If the rciDef does not specify the machine but the existing rci does then remove the association 
            bool updated = rci.RCIUpdate(a_udfManager, rciDef.Rci, t, m_scenarioDetail.ClockDate, m_scenarioDetail.GetPlanningHorizonEnd(), a_dataChanges);
        }
        else //Add a new RecurringCapacityInterval
        {
            rci = new RecurringCapacityInterval(a_udfManager, rciDef.Rci, NextID(), m_scenarioDetail.ClockDate, m_scenarioDetail.GetPlanningHorizonEnd(), t, a_dataChanges);
            Add(rci);
            rci.Expand(m_scenarioDetail.GetPlanningHorizonEnd(), m_scenarioDetail.Clock);
            for (int i = 0; i < machines.Length; i++)
            {
                Resource machine = machines[i];
                machine.AddRecurringCapacityInterval(rci);
                if (!a_affectedResources.Contains(machine))
                {
                    a_affectedResources.Add(machine);
                }
            }

            a_dataChanges.RecurringCapacityIntervalChanges.AddedObject(rci.Id);
            a_dataChanges.FlagConstraintChanges(rci.Id);
            a_dataChanges.FlagEligibilityChanges(rci.Id);
        }
    }
    #endregion ERP Transmissions

    #region ICopyTable
    public override Type ElementType => typeof(RecurringCapacityInterval);
    #endregion

    private void Breakoff(RecurringCapacityIntervalUpdateT a_t, RecurringCapacityInterval a_rci, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        PT.Transmissions.RecurringCapacityInterval breakOffRci = a_t.recurringCapacityInterval;
        RecurringCapacityInterval.RCIExpansion firstExpansion = a_rci.GetExpansionAtIdx(0);
        RecurringCapacityInterval.RCIExpansion lastExpansion = a_rci.GetExpansionAtIdx(a_rci.ExpansionsCount - 1);

        RecurringCapacityInterval beforeSplitRci = null;
        bool removeRci = false;
        if (firstExpansion == lastExpansion)
        {
            //This interval only has one expansion. We need to remove it
            removeRci = true;
        }
        else if (firstExpansion.End == breakOffRci.OriginalEndDateTime)
        {
            //Breaking off first expansion. New StartDate needs to be set to start date of the second expansion
            RecurringCapacityInterval.RCIExpansion secondExpansion = a_rci.GetExpansionAtIdx(1);
            a_rci.StartDateTime = secondExpansion.Start;
            a_rci.EndDateTime = secondExpansion.End;
        }
        else if (lastExpansion.End == breakOffRci.OriginalEndDateTime)
        {
            //Breaking off last expansion
            a_rci.RecurrenceEndDateTime = breakOffRci.OriginalStartDateTime.Subtract(TimeSpan.FromTicks(1));
        }
        else
        {
            //Make a copy of the RCI (pre-modification) to use as a new recurrence up to the start date of the 
            //RCI being broken off.
            beforeSplitRci = AddCopy(a_rci.Id, a_sd);
            beforeSplitRci.Name = a_rci.Name;
            beforeSplitRci.RecurrenceEndDateTime = breakOffRci.OriginalStartDateTime.Subtract(TimeSpan.FromTicks(1)); //don't include the modified interval
            RecurringCapacityInterval.RCIExpansion afterSplitExpansion = null;

            //Find the expansion after the RCI being broken off to use as a new recurrence starting after the end 
            //of the broken off RCI
            for (int i = 0; i < a_rci.ExpansionsCount; i++)
            {
                RecurringCapacityInterval.RCIExpansion expansion = a_rci.GetExpansionAtIdx(i);
                if (expansion.Start == breakOffRci.OriginalStartDateTime && expansion.End == breakOffRci.OriginalEndDateTime)
                {
                    //Store the expansion after the split to set up new recurrence
                    afterSplitExpansion = a_rci.GetExpansionAtIdx(i + 1);
                    break;
                }
            }

            if (afterSplitExpansion == null)
            {
                // If we can't find an afterSplitExpansion, it means the interval that is being modified was already modified 
                // by some other action before the transmission was sent.

                // It'd be nice to put this in a validation function, but I didn't want to have to do the various 
                // checks regarding which expansion was being broken off or iterate through the expansions an extra time just for validation. 
                throw new PTValidationException("3102", new object[] { breakOffRci.Name });
            }

            beforeSplitRci.RecurrenceEndType = CapacityIntervalDefs.recurrenceEndTypes.AfterRecurrenceEndDateTime;
            beforeSplitRci.Expand(a_sd.GetPlanningHorizonEnd(), a_sd.Clock);

            a_rci.StartDateTime = afterSplitExpansion.Start;
            a_rci.EndDateTime = afterSplitExpansion.End;
        }

        foreach (RecurringCapacityInterval.RCIExpansion rciExpansion in a_rci)
        {
            if (rciExpansion.Start == breakOffRci.OriginalStartDateTime && rciExpansion.End == breakOffRci.OriginalEndDateTime)
            { 
                //Add the newly updated interval and the copied recurring interval.
                DateTime newCiStart = a_t.OccurrenceStart;
                DateTime newCiEnd = a_t.OccurrenceEnd;

                string newCiDescription = a_rci.Description;
                string newCiName = a_rci.Name;
                CapacityIntervalDefs.capacityIntervalTypes newCiIntervalType = a_rci.IntervalType;
                decimal newCiNbrPeople = a_rci.NbrOfPeople;
                string newCiNotes = a_rci.Notes;
                System.Drawing.Color newCiColor = a_rci.Color;
                IntervalProfile originalProfile = a_rci.GetIntervalProfile();

                //This is the occurrence edited so change it's values accordingly.
                if (breakOffRci.StartDateTimSet)
                {
                    newCiStart = breakOffRci.StartDateTime;
                }

                if (breakOffRci.EndDateTimeSet)
                {
                    newCiEnd = breakOffRci.EndDateTime;
                }

                if (breakOffRci.IntervalTypeSet)
                {
                    newCiIntervalType = breakOffRci.IntervalType;
                }

                if (breakOffRci.NameSet)
                {
                    newCiName = breakOffRci.Name;
                }

                if (breakOffRci.DescriptionSet)
                {
                    newCiDescription = breakOffRci.Description;
                }

                if (breakOffRci.NbrOfPeopleSet)
                {
                    newCiNbrPeople = breakOffRci.NbrOfPeople;
                }

                if (breakOffRci.NotesSet)
                {
                    newCiNotes = breakOffRci.Notes;
                }

                if (breakOffRci.ColorSet)
                {
                    newCiColor = breakOffRci.Color;
                }

                if (breakOffRci.CleanOutSetupsSet)
                {
                    originalProfile.CleanOutSetups = breakOffRci.CleanOutSetups;
                }

                if (breakOffRci.PreventOperationsFromSpanningSet)
                {
                    originalProfile.PreventOperationsFromSpanning = breakOffRci.PreventOperationsFromSpanning;
                }

                if (breakOffRci.CanStartActivityIsSet)
                {
                    originalProfile.CanStartActivity = breakOffRci.CanStartActivity;
                }

                if (breakOffRci.UsedForSetupIsSet)
                {
                    originalProfile.RunSetup = breakOffRci.UsedForSetup;
                }

                if (breakOffRci.UsedForRunIsSet)
                {
                    originalProfile.RunProcessing = breakOffRci.UsedForRun;
                }

                if (breakOffRci.UsedForPostProcessingIsSet)
                {
                    originalProfile.RunPostProcessing = breakOffRci.UsedForPostProcessing;
                }

                if (breakOffRci.UsedForCleanIsSet)
                {
                    originalProfile.RunCleanout = breakOffRci.UsedForClean;
                }

                if (breakOffRci.UsedForStorageIsSet)
                {
                    originalProfile.RunStoragePostProcessing = breakOffRci.UsedForStorage;
                }

                if (breakOffRci.OvertimeIsSet)
                {
                    originalProfile.Overtime = breakOffRci.Overtime;
                }

                if (breakOffRci.UseOnlyWhenLateIsSet)
                {
                    originalProfile.UseOnlyWhenLate = breakOffRci.UseOnlyWhenLate;
                }

                if (breakOffRci.CapacityCodeIsSet)
                {
                    originalProfile.CapacityCode = breakOffRci.CapacityCode;
                }

                for (int i = 0; i < a_rci.CalendarResources.Count; ++i)
                {
                    InternalResource r = a_rci.CalendarResources[i];
                    CapacityInterval ci;
                    if (a_t.OccurrenceResource != null && a_t.OccurrenceResource.Equals(r.GetKey()))
                    {
                        //Use the new values since this is the occurrence clicked by the user.
                        ci = new CapacityInterval(a_sd.CapacityIntervalManager.NextID(), newCiIntervalType, newCiStart, newCiEnd, originalProfile, newCiColor, false, false);
                        ci.Name = newCiName;
                        ci.Description = newCiDescription;
                        ci.NbrOfPeople = newCiNbrPeople;
                        ci.Notes = newCiNotes;
                    }
                    else
                    {
                        //Use the original values since this is not the occurrence clicked by the user.
                        ci = new CapacityInterval(a_sd.CapacityIntervalManager.NextID(), a_rci.IntervalType, rciExpansion.Start, rciExpansion.End, a_rci.GetIntervalProfile(), a_rci.Color, a_rci.CanDragAndResize, a_rci.CanDelete);
                        ci.Name = a_rci.Name;
                        ci.Description = a_rci.Description;
                        ci.NbrOfPeople = a_rci.NbrOfPeople;
                        ci.Notes = a_rci.Notes;
                    }

                    if (beforeSplitRci != null)
                    {
                        r.AddRecurringCapacityInterval(beforeSplitRci); //add the copy recurring interval
                    }

                    r.AddCapacityInterval(ci);
                    a_sd.CapacityIntervalManager.Add(ci);
                    a_dataChanges.CapacityIntervalChanges.AddedObject(ci.Id);
                }

                break;
            }
        }

        if (removeRci)
        {
            for (int i = 0; i < a_rci.CalendarResources.Count; i++) //Copy resource collection so they can be updated.
            {
                a_dataChanges.MachineChanges.UpdatedObject(a_rci.CalendarResources[i].Id);
            }

            Delete(a_rci);
            a_dataChanges.RecurringCapacityIntervalChanges.DeletedObject(a_rci.Id);
        }
        else
        {
            //Create the new expansions
            a_rci.Expand(a_sd.GetPlanningHorizonEnd(), a_sd.Clock);
            a_dataChanges.RecurringCapacityIntervalChanges.UpdatedObject(a_rci.Id);
        }
    }

    /// <summary>
    /// When the Clock is advanced this function converts RCIs before the Clock to non-recurring so that changes in the RCIs after the Clock do not affect the ones in the past.
    /// This should be done before advancing the Clock because the individual Capacity Intervals created here may need to be purged (if earlier than the Actual Tracking limit).
    /// </summary>
    private void ConvertRecurringToNonRecurringForActuals(RecurringCapacityInterval a_rci, ScenarioDetail a_sd, long a_newClock, out IList<CapacityInterval> a_addedCapacityIntervals)
    {
        List<CapacityInterval> addedCapacityIntervals = new ();
        if (a_sd.ScenarioOptions.TrackActualsAgeLimit.Ticks <= 0)
        {
            a_addedCapacityIntervals = addedCapacityIntervals;
            return;
        }

        long minActualEndDate = a_newClock - a_sd.ScenarioOptions.TrackActualsAgeLimit.Ticks;


        foreach (RecurringCapacityInterval.RCIExpansion rciExpansion in a_rci)
        {
            if (rciExpansion.End.Ticks > a_newClock) //this interval and others after it will be kept as live intervals so no need to copy for an actual yet
            {
                //rci.UpdateStart(rciExpansion.Start, sd); //the expansion should start from here now
                break;
            }

            if (rciExpansion.End.Ticks >= minActualEndDate) //no sense creating Capacity Intervals that will be purged since they're too old.
            {
                string newCiName = string.Format("{0} {1}", a_rci.Name, Localizer.GetString("(Actual)"));
                
                //Use the original values since this is not the occurrence clicked by the user.
                CapacityInterval ci = new (a_sd.CapacityIntervalManager.NextID(), a_rci.IntervalType, rciExpansion.Start, rciExpansion.End, a_rci.GetIntervalProfile(), a_rci.Color, a_rci.CanDragAndResize, a_rci.CanDelete);
                ci.Name = newCiName;
                ci.Description = a_rci.Description;
                ci.NbrOfPeople = a_rci.NbrOfPeople;
                ci.Notes = a_rci.Notes;
                a_sd.CapacityIntervalManager.Add(ci);
                addedCapacityIntervals.Add(ci);
                for (int i = 0; i < a_rci.CalendarResources.Count; ++i)
                {
                    InternalResource r = a_rci.CalendarResources[i];
                    r.AddCapacityInterval(ci);
                }
            }
        }

        a_addedCapacityIntervals = addedCapacityIntervals;
    }
}