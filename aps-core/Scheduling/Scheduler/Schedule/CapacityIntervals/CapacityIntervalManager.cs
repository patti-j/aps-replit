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
/// Manages a sorted list of CapacityInterval objects.
/// </summary>
public class CapacityIntervalManager : ScenarioBaseObjectManager<CapacityInterval>, IPTSerializable
{
    #region IPTSerializable Members
    public CapacityIntervalManager(IReader reader, BaseIdGenerator idGen)
        : base(idGen)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                CapacityInterval c = new (reader);
                Add(c);
            }
        }
    }

    public new const int UNIQUE_ID = 383;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    public class CapacityIntervalManagerException : PTException
    {
        public CapacityIntervalManagerException(string message, object[] a_stringParameters = null, bool a_appendHelpUrl = true)
            : base(message, a_stringParameters, a_appendHelpUrl) { }
    }
    #endregion

    #region Construction
    public CapacityIntervalManager(BaseIdGenerator idGen)
        : base(idGen) { }
    #endregion

    #region CapacityInterval Edit Functions
    internal CapacityInterval AddNew(CapacityIntervalDefs.capacityIntervalTypes intervalType,
                                     CapacityIntervalDefs.capacityIntervalAdditionScopes scope,
                                     BaseId plantId,
                                     BaseId departmentId,
                                     BaseId resourceId,
                                     DateTime intervalStart,
                                     DateTime intervalEnd,
                                     IntervalProfile a_intervalProfile,
                                     System.Drawing.Color a_color,
                                     bool a_canDragAndResize,
                                     bool a_canDelete,
                                     IScenarioDataChanges a_dataChanges = null)
    {
        CapacityInterval ci = new (
            NextID(), 
            intervalType, 
            intervalStart, 
            intervalEnd, 
            a_intervalProfile, 
            a_color,
            a_canDragAndResize,
            a_canDelete);

        //Validate then add to collection
        ValidateAdd(ci);
        base.Add(ci); //Need to add before firing event since this is where the object gets named

        using (m_scenario.ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
        {
            //Set the references between the InternalResource and the CapacityInterval depending on the scope
            InternalResource resource = FindResourceWithValidation(plantId, departmentId, resourceId);
            switch (scope)
            {
                case CapacityIntervalDefs.capacityIntervalAdditionScopes.ResourceOnly:
                    resource.AddCapacityInterval(ci);
                    resource.RegenerateCapacityProfile(sd.GetPlanningHorizonEndTicks(), true);

                    a_dataChanges?.MachineChanges.UpdatedObject(resource.Id);
                    break;
                case CapacityIntervalDefs.capacityIntervalAdditionScopes.Department:
                    for (int i = 0; i < resource.Department.Resources.Count; i++)
                    {
                        resource = resource.Department.Resources[i];
                        resource.AddCapacityInterval(ci);
                        resource.RegenerateCapacityProfile(sd.GetPlanningHorizonEndTicks(), true);

                        a_dataChanges?.MachineChanges.UpdatedObject(resource.Id);
                    }

                    break;
                case CapacityIntervalDefs.capacityIntervalAdditionScopes.Plant:
                    for (int w = 0; w < resource.Department.Plant.Departments.Count; w++)
                    {
                        Department dept = resource.Department.Plant.Departments[w];
                        for (int i = 0; i < dept.Resources.Count; i++)
                        {
                            resource = dept.Resources[i];
                            resource.AddCapacityInterval(ci);
                            resource.RegenerateCapacityProfile(sd.GetPlanningHorizonEndTicks(), true);

                            a_dataChanges?.MachineChanges.UpdatedObject(resource.Id);
                        }
                    }

                    break;
                case CapacityIntervalDefs.capacityIntervalAdditionScopes.AllPlants:
                    for (int p = 0; p < sd.PlantManager.Count; p++)
                    {
                        Plant plant = sd.PlantManager[p];
                        for (int w = 0; w < plant.Departments.Count; w++)
                        {
                            Department dept = plant.Departments[w];
                            for (int i = 0; i < dept.Resources.Count; i++)
                            {
                                resource = dept.Resources[i];
                                resource.AddCapacityInterval(ci);
                                resource.RegenerateCapacityProfile(sd.GetPlanningHorizonEndTicks(), true);

                                a_dataChanges?.MachineChanges.UpdatedObject(resource.Id);
                            }
                        }
                    }

                    break;
            }
        }

        return ci;
    }

    /// <summary>
    /// Will throw a PTValidationException if the resource can't be found.
    /// </summary>
    private InternalResource FindResourceWithValidation(BaseId plantId, BaseId departmentId, BaseId resourceId)
    {
        ScenarioDetail sd;
        using (m_scenario.ScenarioDetailLock.EnterRead(out sd))
        {
            Plant p = sd.PlantManager.GetById(plantId);

            if (p == null)
            {
                throw new PTValidationException("2141", new object[] { plantId });
            }

            Department d = p.Departments.GetById(departmentId);

            if (d == null)
            {
                throw new PTValidationException("2142", new object[] { plantId, departmentId });
            }

            InternalResource r = d.Resources.GetById(resourceId);

            if (r == null)
            {
                throw new PTValidationException("2143", new object[] { plantId, departmentId, resourceId });
            }

            return r;
        }
    }

    private CapacityInterval AddDefault(CapacityIntervalDefaultT t)
    {
        CapacityInterval ci = new (NextID(), m_scenarioDetail.Clock);
        ValidateAdd(ci);
        return base.Add(ci);
    }

    private CapacityInterval AddCopy(BaseId originalId)
    {
        CapacityInterval ci = GetById(originalId);
        CapacityInterval newCi = new (NextID(), ci);
        newCi.Name = string.Format("Copy of {0}".Localize(), ci.Name);
        newCi.ExternalId = ExternalBaseIdObject.MakeExternalId(newCi.Id.Value);
        Add(newCi);
        return newCi;
    }

    /// <summary>
    /// Creates a new Capacity Interval using the specified RecurringCapacityInterval.
    /// </summary>
    internal CapacityInterval AddConvert(UserFieldDefinitionManager a_udfManager, CapacityIntervalConvertT a_t, RecurringCapacityInterval a_rci, IScenarioDataChanges a_dataChanges)
    {
        CapacityInterval ci = new (a_udfManager, a_t.capacityInterval, NextID(), m_scenarioDetail.ClockDate, m_scenarioDetail.GetPlanningHorizonEnd(), a_t, a_dataChanges);
        Add(ci);
        //Set the references between the CalendarResources and the new CapacityInterval
        using (m_scenario.ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
        {
            for (int i = 0; i < a_rci.CalendarResources.Count; i++)
            {
                InternalResource r = a_rci.CalendarResources[i];
                r.AddCapacityInterval(ci);
                r.RegenerateCapacityProfile(sd.GetPlanningHorizonEndTicks(), true);
            }

            return ci;
        }
    }

    private CapacityInterval Delete(CapacityInterval a_ci)
    {
        a_ci.DeleteFromResources(m_scenarioDetail.GetPlanningHorizonEndTicks()); //Give the CapacityInterval a chance to delete references to other objects.
        Remove(a_ci.Id); //Now remove it from the Manager.
        return a_ci;
    }

    /// <summary>
    /// Deletes all Capacity Intervals.
    /// </summary>
    public void Clear(IScenarioDataChanges a_dataChanges)
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            a_dataChanges.CapacityIntervalChanges.DeletedObject(this[i].Id);
            Delete(this[i]);
        }
    }

    /// <summary>
    /// Delete the CapacityInterval without causing a regeneration of the Resource Capacity Profiles since the CapacityInterval is in the past.
    /// </summary>
    private CapacityInterval Purge(CapacityInterval ci)
    {
        ci.PurgeFromResources(); //Give the CapacityInterval a chance to delete references to other objects.
        Remove(ci.Id); //Now remove it from the Manager.
        return ci;
    }

    /// <summary>
    /// Delete Capacity Intervals that end before the specified time.
    /// </summary>
    internal void PurgeOldCapacityIntervals(long purgeTime)
    {
        for (int ciI = Count - 1; ciI >= 0; ciI--)
        {
            CapacityInterval capacityInterval = this[ciI];
            if (capacityInterval.EndDateTime.Ticks <= purgeTime)
            {
                Purge(capacityInterval);
            }
        }
    }
    #endregion

    #region Transmissions
    private void ValidateAdd(CapacityInterval ci)
    {
        if (GetById(ci.Id) != null)
        {
            throw new CapacityIntervalManagerException("2144", new object[] { ci.Id.ToString() });
        }

        if (ci.EndDateTime <= ci.StartDateTime || ci.StartDateTime < PTDateTime.MinDateTime || ci.EndDateTime > PTDateTime.MaxDateTime)
        {
            throw new CapacityInterval.CapacityIntervalException("2929", new object[] { ci.ExternalId, ci.StartDateTime, ci.EndDateTime });
        }

        if (ci.NbrOfPeople <= 0)
        {
            throw new CapacityInterval.CapacityIntervalException("2930", new object[] { ci.ExternalId });
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
                throw new PTValidationException("2145", new object[] { key.Plant, key.Department, key.Resource });
            }

            node = node.Next;
        }
    }

    private void ValidateCopy(CapacityIntervalCopyT t)
    {
        ValidateExistence(t.originalId);
    }

    private CapacityInterval ValidateDelete(BaseId id)
    {
        return ValidateExistence(id);
    }

    private void ValidateUpdate(CapacityIntervalUpdateT t)
    {
        //the base transmission uses a list of CI Ids now so that the DeleteT can remove multiple
        //objects at once. This transmission only processes one at a time so we can just get
        //first and only entry in the list.
        ValidateExistence(t.CapacityIntervalIds[0]);
    }

    private InternalResource ValidateShare(CapacityIntervalShareT t, out InternalResource oldResource, out InternalResource newResource)
    {
        //the base transmission uses a list of CI Ids now so that the DeleteT can remove multiple
        //objects at once. This transmission only processes one at a time so we can just get
        //first and only entry in the list.
        ValidateExistence(t.CapacityIntervalIds[0]);
        oldResource = FindResource(t.oldPlantId, t.oldDepartmentId, t.oldResourceId);

        if (oldResource == null)
        {
            throw new TransmissionValidationException(t, "2146");
        }

        newResource = FindResource(t.newPlantId, t.newDepartmentId, t.newResourceId);

        if (newResource == null)
        {
            throw new TransmissionValidationException(t, "2147");
        }

        return newResource;
    }

    private void ValidateMove(CapacityIntervalMoveT t)
    {
        //the base transmission uses a list of CI Ids now so that the DeleteT can remove multiple
        //objects at once. This transmission only processes one at a time so we can just get
        //first and only entry in the list.
        ValidateExistence(t.CapacityIntervalIds[0]);
        FindResource(t.newPlantId, t.newDepartmentId, t.newResourceId);
        FindResource(t.newPlantId, t.newDepartmentId, t.newResourceId);
    }

    private void ValidateMoveInTime(CapacityIntervalMoveInTimeT t)
    {
        //the base transmission uses a list of CI Ids now so that the DeleteT can remove multiple
        //objects at once. This transmission only processes one at a time so we can just get
        //first and only entry in the list.
        ValidateExistence(t.CapacityIntervalIds[0]);

        if (t.NewStartTime >= t.NewEndTime)
        {
            throw new TransmissionValidationException(t, "2148");
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

    public void Receive(UserFieldDefinitionManager a_udfManager, CapacityIntervalBaseT a_t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        ApplicationExceptionList errList = new ();
        try
        {
            CapacityInterval ci;
            if (a_t is CapacityIntervalNewT cinT)
            {
                IntervalProfile profile = GetIntervalProfileFromTransmission(cinT);
                ci = AddNew(cinT.intervalType, cinT.scope, cinT.plantId, cinT.departmentId, cinT.resourceId, cinT.intervalStart, cinT.intervalStart.Add(cinT.duration), profile, cinT.color, cinT.CanDragAndResize, cinT.CanDelete, a_dataChanges);
                a_dataChanges.CapacityIntervalChanges.AddedObject(ci.Id);
                a_dataChanges.FlagProductionChanges(ci.Id);
            }
            else if (a_t is CapacityIntervalDefaultT defaultT)
            {
                ci = AddDefault(defaultT);
                a_dataChanges.CapacityIntervalChanges.AddedObject(ci.Id);
            }
            else if (a_t is CapacityIntervalCopyT)
            {
                ci = AddCopy(((CapacityIntervalCopyT)a_t).originalId);
                a_dataChanges.CapacityIntervalChanges.AddedObject(ci.Id);
            }
            else if (a_t is CapacityIntervalDeleteT)
            {
                List<ValidationException> exceptions = new ();
                foreach (BaseId capacityIntervalId in ((CapacityIntervalDeleteT)a_t).CapacityIntervalIds)
                {
                    try
                    {
                        ci = ValidateDelete(capacityIntervalId);
                        for (int i = 0; i < ci.CalendarResources.Count; i++) //Copy resource collection so they can be updated.
                        {
                            a_dataChanges.MachineChanges.UpdatedObject(ci.CalendarResources[i].Id);
                        }

                        ci = Delete(ci);
                        a_dataChanges.FlagEligibilityChanges(ci.Id);
                        a_dataChanges.CapacityIntervalChanges.DeletedObject(ci.Id);
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

                if (a_dataChanges.CapacityIntervalChanges.TotalDeletedObjects > 0)
                {
                    a_sd.TimeAdjustment(a_t);
                }
            }
            else if (a_t is CapacityIntervalDeleteAllT)
            {
                for (int i = Count - 1; i >= 0; i--)
                {
                    ci = GetByIndex(i);
                    for (int j = 0; j < ci.CalendarResources.Count; j++) //Copy resource collection so they can be updated.
                    {
                        a_dataChanges.MachineChanges.UpdatedObject(ci.CalendarResources[j].Id);
                    }

                    Delete(ci);
                    a_dataChanges.FlagEligibilityChanges(ci.Id);
                    a_dataChanges.CapacityIntervalChanges.DeletedObject(ci.Id);
                }
            }
            else if (a_t is CapacityIntervalIdBaseT)
            {
                CapacityIntervalIdBaseT pt = (CapacityIntervalIdBaseT)a_t;

                //Only the CapacityIntervalDeleteT processes multiple objects for now. Here we can assume
                //that the list only contains one object for now so get the first one.
                ci = ValidateExistence(pt.CapacityIntervalIds[0]);
                if (a_t is CapacityIntervalShareT)
                {
                    ValidateShare((CapacityIntervalShareT)a_t, out InternalResource oldResource, out InternalResource newResource);
                    //If the new resource already has the interval then REMOVE it from the OLD resource.  This is how UNSHARING is done in the UI.
                    if (newResource.CapacityIntervals.Contains(ci.Id))
                    {
                        oldResource.RemoveCapacityInterval(ci);
                        oldResource.RegenerateCapacityProfile(a_sd.GetPlanningHorizonEndTicks(), true);
                        a_dataChanges.MachineChanges.UpdatedObject(oldResource.Id);
                    }

                    newResource.AddCapacityInterval(ci);
                    newResource.RegenerateCapacityProfile(a_sd.GetPlanningHorizonEndTicks(), true);
                    a_dataChanges.CapacityIntervalChanges.UpdatedObject(ci.Id);
                    a_dataChanges.MachineChanges.UpdatedObject(newResource.Id);
                    a_dataChanges.FlagEligibilityChanges(ci.Id);
                    a_dataChanges.FlagConstraintChanges(BaseId.NULL_ID);
                }
                else if (a_t is CapacityIntervalCopyToResourceT)
                {
                    CapacityIntervalCopyToResourceT copyT = (CapacityIntervalCopyToResourceT)a_t;
                    InternalResource resource = FindResource(copyT.newPlantId, copyT.newDepartmentId, copyT.newResourceId);
                    CapacityInterval newCi = a_sd.CapacityIntervalManager.AddCopy(copyT.CapacityIntervalIds[0]);

                    resource.AddCapacityInterval(newCi);
                    resource.RegenerateCapacityProfile(a_sd.GetPlanningHorizonEndTicks(), true);
                    a_dataChanges.CapacityIntervalChanges.AddedObject(ci.Id);
                    a_dataChanges.MachineChanges.UpdatedObject(resource.Id);
                    a_dataChanges.FlagConstraintChanges(BaseId.NULL_ID);
                    a_dataChanges.FlagEligibilityChanges(ci.Id);
                }

                if (a_t is CapacityIntervalMoveInTimeT)
                {
                    ci = ValidateExistence(pt.CapacityIntervalIds[0]);
                    CapacityIntervalMoveInTimeT moveInTimeT = (CapacityIntervalMoveInTimeT)a_t;
                    ValidateMoveInTime(moveInTimeT);
                    ci.Receive(a_udfManager, pt, a_sd, a_dataChanges); //Need to handle date changes

                    for (int i = 0; i < ci.CalendarResources.Count; i++) //Copy resource collection so they can be updated.
                    {
                        a_dataChanges.MachineChanges.UpdatedObject(ci.CalendarResources[i].Id);
                    }

                    a_dataChanges.FlagConstraintChanges(BaseId.NULL_ID);
                }

                if (a_t is CapacityIntervalMoveT)
                {
                    ci = ValidateExistence(pt.CapacityIntervalIds[0]);
                    CapacityIntervalMoveT moveT = (CapacityIntervalMoveT)a_t;
                    ValidateMove(moveT);

                    InternalResource newResource = FindResource(moveT.newPlantId, moveT.newDepartmentId, moveT.newResourceId);
                    InternalResource oldResource = FindResource(moveT.oldPlantId, moveT.oldDepartmentId, moveT.oldResourceId);
                    if (newResource != oldResource) //Moved resource so need to remove reference to ci from old resource and add to new one
                    {
                        newResource.AddCapacityInterval(ci);
                        oldResource.RemoveCapacityInterval(ci);
                        oldResource.RegenerateCapacityProfile(a_sd.GetPlanningHorizonEndTicks(), true);
                        a_dataChanges.MachineChanges.UpdatedObject(oldResource.Id);
                    }

                    ci.Receive(a_udfManager, pt, a_sd, a_dataChanges); //Need to handle date changes
                    
                    //TODO: is this needed?
                    newResource.RegenerateCapacityProfile(a_sd.GetPlanningHorizonEndTicks(), true);
                    a_dataChanges.MachineChanges.UpdatedObject(newResource.Id);

                    a_dataChanges.FlagConstraintChanges(BaseId.NULL_ID);
                }
                else if (a_t is CapacityIntervalUpdateT)
                {
                    CapacityIntervalUpdateT updateT = (CapacityIntervalUpdateT)a_t;
                    ValidateUpdate((CapacityIntervalUpdateT)a_t);
                    ci.Receive(a_udfManager, updateT, a_sd, a_dataChanges);
                    for (int i = 0; i < ci.CalendarResources.Count; i++) //Copy resource collection so they can be updated.
                    {
                        a_dataChanges.MachineChanges.UpdatedObject(ci.CalendarResources[i].Id);
                    }
                }
                else if (a_t is CapacityIntervalSetResourcesT setResourcesT)
                {
                    ci = ValidateExistence(setResourcesT.CapacityIntervalIds[0]);
                    ValidateResources(setResourcesT.resources);
                    CalendarResourcesCollection effectedResources = new ();
                    Hashtable oldResourceKeys = new ();
                    Hashtable newResourceKeys = new ();
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
                            Resource newResource = a_sd.PlantManager.GetResource(key);
                            newResource.AddCapacityInterval(ci);
                            a_dataChanges.MachineChanges.UpdatedObject(newResource.Id);
                            effectedResources.Add(newResource);
                        }

                        node = node.Next;
                    }

                    //make sure the Resources that have this capacity interval should keep it
                    for (int rI = ci.CalendarResources.Count - 1; rI >= 0; rI--)
                    {
                        InternalResource res = ci.CalendarResources[rI];
                        if (!newResourceKeys.Contains(res.GetKey()))
                        {
                            res.RemoveCapacityInterval(ci);
                            effectedResources.Add(res);
                            a_dataChanges.MachineChanges.UpdatedObject(res.Id);
                        }
                    }

                    //Need to regenerate to resource's capacity profiles
                    for (int i = 0; i < effectedResources.Count; i++)
                    {
                        effectedResources[i].RegenerateCapacityProfile(a_sd.GetPlanningHorizonEndTicks(), true);
                    }

                    a_dataChanges.CapacityIntervalChanges.UpdatedObject(ci.Id);
                    a_dataChanges.FlagConstraintChanges(ci.Id);
                    a_dataChanges.FlagEligibilityChanges(ci.Id);
                }
            }
        }
        catch (PTHandleableException e)
        {
            errList.Add(new PTValidationException("Error processing CapacityInterval", e));
        }

        if (errList.Count > 0)
        {
            ScenarioExceptionInfo sei = new ();
            sei.Create(a_sd);
            m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
        }
    }

    public void Receive(RecurringCapacityIntervalConvertT t, IScenarioDataChanges a_dataChanges)
    {
        CapacityInterval ci = ValidateDelete(t.originalCapacityIntervalId);

        ci = Delete(ci);
        a_dataChanges.CapacityIntervalChanges.DeletedObject(ci.Id);
    }

    //		internal void Receive(ERPTransmission t)
    //		{
    //			if(t is CapacityIntervalT)
    //			{
    //				ScenarioEvents se;
    //				CapacityIntervalT pT=(CapacityIntervalT)t;
    //				ArrayList addedCapacityIntervals=new ArrayList();
    //				ArrayList updatedCapacityIntervals=new ArrayList();
    //				ArrayList deletedCapacityIntervals=new ArrayList();
    //
    //				Hashtable affectedCapacityIntervals=new Hashtable();
    //
    //				for(int i=0; i<pT.Count; ++i)
    //				{
    //					CapacityIntervalT.CapacityInterval capacityIntervalNode=pT[i];
    //					CapacityInterval capacityInterval=this.FindByExternalId(capacityIntervalNode.ExternalId);
    //					if(capacityInterval==null)
    //					{
    //						capacityInterval=new CapacityInterval(this.NextID(),capacityIntervalNode);
    //						Add(capacityInterval);
    //						addedCapacityIntervals.Add(capacityInterval);
    //						this.GanttViewerLayout.Add(capacityInterval);
    //					}
    //					else
    //						updatedCapacityIntervals.Add(capacityInterval);
    //					capacityInterval.Name=capacityIntervalNode.Name;
    //					capacityInterval.Notes=capacityIntervalNode.Notes;
    //					this.GanttViewerLayout.Change(capacityInterval);
    //					affectedCapacityIntervals.Add(capacityInterval.Id, null);
    //				}
    //
    //				if(pT.AutoDeleteMode)
    //				{
    //					for(int i=this.Count-1;i>=0;i--)
    //					{
    //						CapacityInterval capacityInterval=(CapacityInterval)this.GetByIndex(i);
    //						BaseId id=capacityInterval.Id;
    //						if(!affectedCapacityIntervals.ContainsKey(id))
    //						{
    //							this.GanttViewerLayout.Delete(capacityInterval);
    //							this.Remove(id);
    //							deletedCapacityIntervals.Add(capacityInterval);
    //						}
    //					}
    //				}
    //				using(scenario.ScenarioEventsLock.EnterRead(out se))
    //				{
    //					se.FireCapacityIntervalChangesEvent(addedCapacityIntervals, updatedCapacityIntervals, deletedCapacityIntervals);
    //				}
    //
    //			}
    //		}

    internal new void Add(CapacityInterval ci)
    {
        base.Add(ci);
    }

    internal new void Remove(CapacityInterval c)
    {
        Remove(c.Id);
    }
    #endregion

    #region ERP Transmissions
    internal void CapacityIntervalTHandler(CapacityIntervalT t, ScenarioDetail sd, Dictionary<BaseId, InternalResource> a_affectedResources, IScenarioDataChanges a_dataChanges)
    {
        //Return if not in autodelete mode.  Nothing to do.
        if (!t.AutoDeleteMode && !t.AutoDeleteResourceAssociations)
        {
            return;
        }

        //Create a hashtable of the RCIs in the Transmission
        Hashtable transRCIs = new ();
        for (int i = 0; i < t.Count; i++)
        {
            CapacityIntervalT.CapacityIntervalDef rciDef = t[i];
            if (transRCIs.ContainsKey(rciDef.m_ci.ExternalId))
            {
                throw new PTValidationException("2149", new object[] { rciDef.m_ci.ExternalId });
            }

            transRCIs.Add(rciDef.m_ci.ExternalId, rciDef);
        }

        //Iterate through the existing RCIs and if not found in the Transmission then delete it
        for (int i = Count - 1; i >= 0; --i)
        {
            CapacityInterval rci = this[i];
            if (!transRCIs.Contains(rci.ExternalId))
            {
                if (t.AutoDeleteMode)
                {
                    //Store the affected Resources
                    for (int resI = 0; resI < rci.CalendarResources.Count; resI++)
                    {
                        InternalResource afRes = rci.CalendarResources[resI];
                        if (!a_affectedResources.ContainsKey(afRes.Id))
                        {
                            a_affectedResources.Add(afRes.Id, afRes);
                        }

                        a_dataChanges.MachineChanges.UpdatedObject(afRes.Id);
                    }

                    a_dataChanges.CapacityIntervalChanges.DeletedObject(rci.Id);
                    Delete(rci);
                    a_dataChanges.FlagEligibilityChanges(rci.Id);
                }
            }
            else if (t.AutoDeleteResourceAssociations)
            {
                CapacityIntervalT.CapacityIntervalDef ciDefT = (CapacityIntervalT.CapacityIntervalDef)transRCIs[rci.ExternalId];
                for (int resI = rci.CalendarResources.Count - 1; resI >= 0; --resI)
                {
                    InternalResource resource = rci.CalendarResources[resI];
                    //Need to know if resource is in transmission
                    Tuple<string, string, string> resExternalId = new (resource.Plant.ExternalId, resource.Department.ExternalId, resource.ExternalId);
                    bool transmissionContainsAssociation = ciDefT.ResourceExternalIds.Contains(resExternalId);
                    if (!transmissionContainsAssociation)
                    {
                        if (!a_affectedResources.ContainsKey(resource.Id))
                        {
                            a_affectedResources.Add(resource.Id, resource);
                        }

                        a_dataChanges.MachineChanges.UpdatedObject(resource.Id);
                        a_dataChanges.CapacityIntervalChanges.UpdatedObject(rci.Id);
                        resource.RemoveCapacityInterval(rci);
                        a_dataChanges.FlagEligibilityChanges(rci.Id);
                    }
                }
            }
        }
    }

    //If you call this function, make sure to update affectedResources RegenerateCapacityProfile.
    internal void CapacityIntervalTHandler(UserFieldDefinitionManager a_udfManager, CapacityIntervalT.CapacityIntervalDef rciDef, Resource[] machines, ScenarioDetail a_sd, CapacityIntervalT t, Dictionary<BaseId, InternalResource> a_affectedResources, IScenarioDataChanges a_dataChanges)
    {
        CapacityInterval ci = GetByExternalId(rciDef.m_ci.ExternalId);
        
        if (ci != null) //so update it
        {
            bool updated = ci.Update(a_udfManager, rciDef.m_ci, out bool timeChange, t, a_sd.ClockDate, a_sd.GetPlanningHorizonEnd(), a_dataChanges);

            for (int i = 0; i < machines.Length; i++) // Add new resource associations
            {
                Resource machine = machines[i];

                //If the interval does not already specify the machine then add it.
                if (!ci.CalendarResources.Contains(machine))
                {
                    machine.AddCapacityInterval(ci);
                    a_dataChanges.FlagEligibilityChanges(ci.Id);
                    if (!a_affectedResources.ContainsKey(machine.Id))
                    {
                        //If new resources are added to the CI's collection of resources we need to flag the ci as updated
                        //regardless of whether it has been flagged as updated by the base Update function
                        updated = true;
                        a_affectedResources.Add(machine.Id, machine);
                    }
                }
            }

           
            if (updated)
            {
                foreach (Resource r in machines)
                {
                    if (a_affectedResources.TryAdd(r.Id, r))
                    {
                        a_dataChanges.MachineChanges.UpdatedObject(r.Id);
                    }
                }

                a_dataChanges.CapacityIntervalChanges.UpdatedObject(ci.Id);
            }

            //TODO : If the rciDef does not specify the machine but the existing rci does then remove the association 

        }
        else //Add a new CapacityInterval
        {
            ci = new CapacityInterval(a_udfManager, rciDef.m_ci, NextID(), m_scenarioDetail.ClockDate, m_scenarioDetail.GetPlanningHorizonEnd(), t, a_dataChanges);
            Add(ci);
            for (int i = 0; i < machines.Length; i++)
            {
                Resource machine = machines[i];
                machine.AddCapacityInterval(ci);
                a_dataChanges.FlagEligibilityChanges(ci.Id);
                if (a_affectedResources.TryAdd(machine.Id, machine))
                {
                    a_dataChanges.MachineChanges.UpdatedObject(machine.Id);
                }
            }

            a_dataChanges.CapacityIntervalChanges.AddedObject(ci.Id);
        }
    }
    #endregion ERP Transmissions

    #region ICopyTable
    public override Type ElementType => typeof(CapacityInterval);
    #endregion

    #region Restore References
    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        foreach (CapacityInterval capacityInterval in this)
        {
            a_udfManager.RestoreReferences(capacityInterval, UserField.EUDFObjectType.CapacityIntervals);
        }
    }
    #endregion

    public new CapacityInterval this[int index] => GetByIndex(index);

    /// <summary>
    /// This function is used to create an IntervalProfile depending on the values on the transmission.
    /// Currently, only CapacityIntervalNewT is supported, but maybe it's worth moving
    /// all the bool values into the base. 
    /// </summary>
    /// <param name="a_transmission"> The transmission to pull IntervalProfile information from</param>
    /// <returns></returns>
    private IntervalProfile GetIntervalProfileFromTransmission(CapacityIntervalBaseT a_transmission)
    {
        if (a_transmission is CapacityIntervalNewT newIntervalTransmission)
        {
            IntervalProfile profile = new IntervalProfile(
                newIntervalTransmission.ClearChangeovers,
                newIntervalTransmission.PreventSpanning,
                newIntervalTransmission.CanStartActivity,
                newIntervalTransmission.UsedForSetup,
                newIntervalTransmission.UsedForRun,
                newIntervalTransmission.UsedForPostProcessing,
                newIntervalTransmission.UsedForClean,
                newIntervalTransmission.UsedForStoragePostProcessing,
                newIntervalTransmission.Overtime,
                newIntervalTransmission.UseOnlyWhenLate,
                string.Empty 
                );
            return profile;
        }
        
        return IntervalProfile.DefaultProfile;
    }
}