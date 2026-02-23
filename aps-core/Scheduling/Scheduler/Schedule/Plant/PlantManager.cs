using System.ComponentModel;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.Common.File;
using PT.ERPTransmissions;
using PT.Scheduler.ErrorReporting;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of Plant objects.
/// </summary>
public partial class PlantManager : ScenarioBaseObjectManager<Plant>, IPTSerializable
{
    #region IPTSerializable Members
    public PlantManager(IReader reader, BaseIdGenerator idGen)
        : base(idGen)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                Plant plant = new (reader, idGen);
                Add(plant);
            }
        }
    }

    public new const int UNIQUE_ID = 335;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    internal override void RestoreReferences(Scenario scenario, ScenarioDetail sd, ISystemLogger a_errorReporter)
    {
        base.RestoreReferences(scenario, sd, a_errorReporter);
        for (int i = 0; i < Count; i++)
        {
            this[i].RestoreReferences(scenario, a_errorReporter);
        }
    }

    internal void RestoreReferences(CustomerManager a_cm, CapabilityManager capabilities, CapacityIntervalManager cis, RecurringCapacityIntervalManager rcis, DispatcherDefinitionManager dispatcherDefs, WarehouseManager warehouseManager, ItemManager itemManager, ScenarioDetail sd)
    {
        itemManager.InitFastLookupByExternalId(); // product rules looksup items by ExternalId (TFS 5820).

        try
        {
            for (int i = 0; i < Count; i++)
            {
                this[i].RestoreReferences(a_cm, capabilities, cis, rcis, dispatcherDefs, this, warehouseManager, itemManager, sd);
            }
        }
        finally
        {
            itemManager.DeInitFastLookupByExternalId();
        }
    }

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        foreach (Plant plant in this)
        {
            plant.RestoreReferences(a_udfManager);
        }
    }
    #endregion

    #region Declarations
    public class PlantManagerException : PTException
    {
        public PlantManagerException(string message)
            : base(message) { }
    }
    #endregion

    #region Construction
    public PlantManager(BaseIdGenerator idGen)
        : base(idGen) { }
    #endregion

    #region Find functions
    public Resource GetResource(ResourceKey key)
    {
        Plant plant = GetById(key.Plant);
        if (plant != null)
        {
            return plant.Departments.GetResource(key);
        }

        return null;
    }

    /// <summary>
    /// Returns the Resource or null.
    /// </summary>
    public Resource GetResource(string plantExternalId, string deptExternalId, string resourceExternalId)
    {
        Plant plant = GetByExternalId(plantExternalId);
        if (plant != null)
        {
            Department dept = plant.Departments.GetByExternalId(deptExternalId);
            if (dept != null)
            {
                return dept.Resources.GetByExternalId(resourceExternalId);
            }
        }

        return null;
    }

    public InternalResource GetResource(BaseId a_resId)
    {
        for (int plantI = 0; plantI < Count; plantI++)
        {
            Plant p = this[plantI];
            InternalResource r;
            if ((r = p.FindResource(a_resId)) != null)
            {
                return r;
            }
        }

        return null;
    }

    public Department FindDepartment(BaseId deptId)
    {
        Department dept;
        for (int plantI = 0; plantI < Count; plantI++)
        {
            Plant p = this[plantI];
            dept = p.Departments.GetById(deptId);
            if (dept != null)
            {
                return dept;
            }
        }

        return null;
    }
    #endregion

    #region Plant Edit Functions
    internal Plant AddDefault(ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        Plant p = new (NextID(), a_sd);
        ValidateAdd(p);
        a_dataChanges.PlantChanges.AddedObject(p.Id);
        a_dataChanges.AuditEntry(new AuditEntry(p.Id, p), true);

        p.Departments.AddDefault(p, new DepartmentDefaultT(a_sd.Scenario.Id, p.Id), a_dataChanges);

        return Add(p, a_sd);
    }

    internal Plant Add(Plant p, ScenarioDetail sd)
    {
        ValidateAdd(p);
        return base.Add(p);
    }

    private Plant AddCopy(PlantCopyT t)
    {
        ValidateCopy(t);
        Plant p = GetById(t.originalId);
        return AddCopy(p, p.Clone(), NextID());
    }

    private Plant Delete(PlantDeleteT t, Scenario scenario, IScenarioDataChanges a_dataChanges)
    {
        Plant p = GetById(t.plantId);
        Delete(p, scenario, a_dataChanges);
        return p;
    }

    internal void Delete(Plant p, Scenario s, IScenarioDataChanges a_dataChanges)
    {
        a_dataChanges.AuditEntry(new AuditEntry(p.Id, p), false, true);
        p.Deleting(a_dataChanges);
        a_dataChanges.PlantChanges.DeletedObject(p.Id);
        Remove(p.Id);
    }
    #endregion

    #region Transmissions
    private void ValidateAdd(Plant p)
    {
        EditionAddPlantCheck();
        if (GetById(p.Id) != null)
        {
            throw new ValidationException("2253", new object[] { p.Id.ToString() });
        }
    }

    private void ValidateCopy(PlantCopyT t)
    {
        EditionAddPlantCheck();
        ValidateExistence(t.originalId);
    }

    /// <summary>
    /// Validate that the PT Key isn't preventing the addition of another Plant.
    /// </summary>
    private void EditionAddPlantCheck()
    {
        if (Count >= PTSystem.LicenseKey.MaxNbrPlants)
        {
            throw new ValidationException("2254", new object[] { PTSystem.LicenseKey.MaxNbrPlants });
        }
    }

    internal void Receive(ScenarioDetail a_sd, PlantBaseT a_t, Scenario a_scenario, IScenarioDataChanges a_dataChanges)
    {
        Plant p = null;
        if (a_t is PlantDefaultT)
        {
            AddDefault(a_sd, a_dataChanges);
        }
        else if (a_t is PlantCopyT)
        {
            p = AddCopy((PlantCopyT)a_t);
            a_dataChanges.PlantChanges.AddedObject(p.Id);
            a_dataChanges.AuditEntry(new AuditEntry(p.Id, p), true);
        }
        else if (a_t is PlantDeleteAllT)
        {
            a_sd.JobManager.UnscheduleAllJobs();

            for (int i = Count - 1; i >= 0; i--)
            {
                p = this[i];
                a_dataChanges.PlantChanges.DeletedObject(p.Id);
                Delete(p, a_scenario, a_dataChanges);
            }
        }
        else if (a_t is ScenarioDetailClearResourcePerformancesT)
        {
            for (int i = 0; i < Count; i++)
            {
                this[i].ClearResourcePerformances(a_t.TimeStamp);
            }
        }
        else if (a_t is PlantSetResourceDeptT plantSetResT)
        {
            SetResourceDepartment(plantSetResT, a_dataChanges);
        }
        else if (a_t is AllowedHelperResourcesT allowedHelperT)
        {
            UpdateAllowedHelpers(allowedHelperT, a_dataChanges);
        }
        else if (a_t is PlantIdBaseT pt)
        {
            p = ValidateExistence(pt.plantId);
            if (a_t is PlantDeleteT plantDeleteT)
            {
                Delete(plantDeleteT, a_scenario, a_dataChanges);
                a_dataChanges.PlantChanges.DeletedObject(p.Id);
            }
            else if (a_t is DepartmentBaseT)
            {
                if (a_t is ResourceDeleteMultiT resourceDeleteMultiT)
                {
                    List<Plant> uniquePlants = new List<Plant>();
                    foreach(ResourceKey key in resourceDeleteMultiT.ResourceKeys){

                        p = ValidateExistence(key.Plant);
                        uniquePlants.AddIfNew(p);
                    }

                    foreach (Plant plant in uniquePlants)
                    {
                        plant.Receive(a_sd, resourceDeleteMultiT, a_dataChanges);

                        if (plant.DepartmentCount == 0)
                        {
                            Delete(plant, a_scenario, a_dataChanges );

                            foreach (Job job in a_sd.JobManager)
                            {
                                job.AdjustEligiblePlants(new PlantDeleteT(a_scenario.Id, p.Id));
                            }
                        }
                    }
                }
                else{
                    p.Receive(a_sd, (DepartmentBaseT)a_t, a_dataChanges);
                }
            }
        }
    }

    internal void Receive(ScenarioDetailClearT t, ScenarioDetail sd, IScenarioDataChanges a_dataChanges)
    {
        for (int i = 0; i < Count; i++)
        {
            this[i].Receive(t, sd, a_dataChanges);
        }

        if (t.ClearPlants)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                Plant plant = this[i];
                if (plant.DepartmentCount == 0) //Only delete plants that are empty
                {
                    Delete(plant, sd.Scenario, a_dataChanges);
                }
            }
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="a_t"></param>
    /// <returns>true if any plants were deleted. This is to update Job's EligiblePlants list.</returns>
    internal bool Receive(UserFieldDefinitionManager a_udfManager, PlantT a_t, IScenarioDataChanges a_dataChanges, ScenarioExceptionInfo a_sei)
    {
        ApplicationExceptionList errList = new ();
        List<PostProcessingAction> actions = new ();

        HashSet<BaseId> affectedPlants = new ();

        try
        {
            for (int i = 0; i < a_t.Count; ++i)
            {
                try
                {
                    PlantT.Plant plantNode = a_t[i];
                    Plant plant;
                    if (plantNode.IdSet)
                    {
                        plant = GetById(plantNode.Id);
                        if (plant == null)
                        {
                            throw new ValidationException("2255", new object[] { plantNode.Id });
                        }
                    }
                    else
                    {
                        plant = GetByExternalId(plantNode.ExternalId);
                    }

                    if (plant == null)
                    {
                        plant = new Plant(NextID(), plantNode, m_scenarioDetail, a_t, a_udfManager, UserField.EUDFObjectType.Plants);
                        Add(plant, m_scenarioDetail);
                        a_dataChanges.PlantChanges.AddedObject(plant.Id);
                        a_dataChanges.AuditEntry(new AuditEntry(plant.Id, plant), true);
                    }
                    else
                    {
                        AuditEntry plantAuditEntry = new AuditEntry(plant.Id, plant);
                        plant.Update(a_udfManager, plantNode, a_t);
                        a_dataChanges.PlantChanges.UpdatedObject(plant.Id);
                        a_dataChanges.AuditEntry(plantAuditEntry);
                    }

                    if (!affectedPlants.Contains(plant.Id))
                    {
                        affectedPlants.Add(plant.Id);
                    }
                }
                catch (PTValidationException err)
                {
                    errList.Add(err);
                }
            }

            if (a_t.AutoDeleteMode)
            {
                List<Plant> plantsToDelete = new ();
                foreach (Plant p in this)
                {
                    if (!affectedPlants.Contains(p.Id))
                    {
                        plantsToDelete.Add(p);
                    }
                }

                if (plantsToDelete.Count > 0)
                {
                    actions.Add(new PostProcessingAction(a_t, false, () =>
                        {
                            ApplicationExceptionList delErrs = new ();

                            foreach (Plant p in plantsToDelete)
                            {
                                try
                                {
                                    Delete(p, m_scenario, a_dataChanges);
                                }
                                catch (PTValidationException err)
                                {
                                    delErrs.Add(err);
                                }
                            }

                            if (delErrs.Count > 0)
                            {
                                m_errorReporter.LogException(delErrs, a_t, a_sei, ELogClassification.PtInterface, false);
                            }
                        }));
                }
            }
        }
        catch (PTValidationException e)
        {
            errList.Add(e); //need to use try catch so the UI gets updated below.
        }
        finally
        {
            m_scenarioDetail.AddProcessingAction(actions);

            if (errList.Count > 0)
            {
                m_errorReporter.LogException(errList, a_t, a_sei, ELogClassification.PtInterface, false);
            }
        }

        return a_dataChanges.PlantChanges.TotalDeletedObjects > 0;
    }

    internal void Receive(ScenarioDetail a_sd, UserFieldDefinitionManager a_udfManager, ResourceEditT a_resEditT, IScenarioDataChanges a_dataChanges, ISystemLogger a_errorReporter)
    {
        ApplicationExceptionList errList = new();

        try
        {
            foreach (PlantEdit plantEdit in a_resEditT.PlantEdits)
            {
                try
                {
                    Plant plant = GetById(plantEdit.Id);
                    AuditEntry plantAuditEntry = new AuditEntry(plant.Id, plant);
                    plant.Update(plantEdit);
                    a_dataChanges.PlantChanges.UpdatedObject(plant.Id);
                    a_dataChanges.AuditEntry(plantAuditEntry);
                }
                catch (Exception e)
                {
                    errList.Add(e);
                }
            }

            foreach (DepartmentEdit departmentEdit in a_resEditT.DepartmentEdits)
            {
                try
                {
                    Plant plant = GetById(departmentEdit.PlantId);
                    Department dept = plant.Departments.GetById(departmentEdit.Id);
                    AuditEntry deptAuditEntry = new AuditEntry(dept.Id, plant.Id, dept);
                    dept.Update(departmentEdit);
                    a_dataChanges.DepartmentChanges.UpdatedObject(dept.Id);
                    a_dataChanges.AuditEntry(deptAuditEntry);
                }
                catch (Exception e)
                {
                    errList.Add(e);
                }
            }

            foreach (ResourceEdit resourceEdit in a_resEditT.ResourceEdits)
            {
                try
                {
                    Resource res = GetResource(new ResourceKey(resourceEdit.PlantId, resourceEdit.DepartmentId, resourceEdit.Id));
                    AuditEntry resAuditEntry = new AuditEntry(res.Id, res.Department.Id, res);
                    bool updated = res.Edit(a_sd, a_udfManager, resourceEdit, a_resEditT, a_errorReporter, a_dataChanges);

                    if (updated)
                    {
                        a_dataChanges.MachineChanges.UpdatedObject(res.Id);
                        a_dataChanges.AuditEntry(resAuditEntry);
                    }
                }
                catch (Exception e)
                {
                    errList.Add(e);
                }
            }
        }
        catch (Exception final)
        {
            errList.Add(final);
        }
        finally
        {
            if (errList.Count > 0)
            {
                ScenarioExceptionInfo sei = new();
                sei.Create(a_sd);
                m_errorReporter.LogException(errList, a_resEditT, sei, ELogClassification.PtInterface, false);
            }
        }
    }

    private void SetResourceDepartment(PlantSetResourceDeptT t, IScenarioDataChanges a_dataChanges)
    {
        //Validate that the Resource exists
        ResourceKey resourceKey = new (t.oldPlantId, t.oldDeptId, t.resourceId);
        Resource resource = GetResource(resourceKey);
        if (resource != null)
        {
            //If the same Plant/Dept was selected don't do anything
            if (resource.Department.Id == t.newDeptId && resource.Department.Plant.Id == t.newPlantId)
            {
                return;
            }

            //Find the new Department
            Plant newPlant = GetById(t.newPlantId);
            if (newPlant == null)
            {
                throw new PTValidationException("2256", new object[] { t.ScenarioId, t.newPlantId });
            }

            Department newDept = newPlant.Departments.GetById(t.newDeptId);
            if (newDept == null)
            {
                throw new PTValidationException("2257", new object[] { t.ScenarioId, t.newPlantId, t.newDeptId });
            }

            //Remove the Resource from the UI.  Then we'll add it with the new key info
            Department oldDept = resource.Department; //store for updating

            for (int cIdx = 0; cIdx < resource.CapabilityCount; cIdx++)
            {
                a_dataChanges.CapabilityChanges.UpdatedObject(resource.GetCapabilityByIndex(cIdx).Id);
            }

            //Remove the Resource from the old Department and add it to the new Department
            resource.Department.RemoveMachineAssociation(resource, a_dataChanges, false, false, false, false);
            newDept.AddMachineAssociationWithNewId(resource, a_dataChanges);
            resource.Department = newDept;

            //Add the Resource to the UI now that it has its new Dept.
            a_dataChanges.MachineChanges.AddedObject(resource.Id);
            a_dataChanges.DepartmentChanges.UpdatedObject(newDept.Id);
            a_dataChanges.DepartmentChanges.UpdatedObject(oldDept.Id);
        }
        else
        {
            throw new PTValidationException("2258", new object[] { t.ScenarioId, resourceKey.Plant, resourceKey.Department, resourceKey.Resource });
        }
    }

    internal void ClearAllowedHelpers()
    {
        for (int i = 0; i < Count; ++i)
        {
            this[i].ResetAllowedHelperManager();
        }
    }

    private void UpdateAllowedHelpers(AllowedHelperResourcesT a_t, IScenarioDataChanges a_scenarioDataChanges)
    {
        // The key plant Id.
        // The value's key is Primary Resource id of resources that specify allowed helpers.
        Dictionary<BaseId, Dictionary<BaseId, AllowedHelperResourcesT.HelperRelation>> relationsByPlant = new ();

        Dictionary<BaseId, Resource> resDictionary = GetResourceDictionary();
        foreach (AllowedHelperResourcesT.HelperRelation hr in a_t.AllowedHelperResources)
        {
            if (hr.AllowedHelperResourceIds.Count == 0)
            {
                throw new ValidationException("2259", new object[] { hr.PrimaryResourceId });
            }

            if (resDictionary.TryGetValue(hr.PrimaryResourceId, out Resource primaryRes))
            {
                if (relationsByPlant.TryGetValue(primaryRes.PlantId, out Dictionary<BaseId, AllowedHelperResourcesT.HelperRelation> helperRelationDictionary))
                {
                    if (helperRelationDictionary.ContainsKey(primaryRes.Id))
                    {
                        throw new ValidationException("2260", new object[] { primaryRes.Id });
                    }
                }
                else
                {
                    helperRelationDictionary = new Dictionary<BaseId, AllowedHelperResourcesT.HelperRelation>();
                    relationsByPlant.Add(primaryRes.PlantId, helperRelationDictionary);
                }

                foreach (BaseId helperId in hr.AllowedHelperResourceIds)
                {
                    if (resDictionary.TryGetValue(helperId, out Resource helperRes))
                    {
                        if (primaryRes.Plant != helperRes.Plant)
                        {
                            throw new ValidationException("2261", new object[] { primaryRes.Id, helperRes.Id });
                        }
                    }
                    else
                    {
                        throw new ValidationException("2262", new object[] { helperId });
                    }
                }

                helperRelationDictionary.Add(primaryRes.Id, hr);
            }
            else
            {
                throw new ValidationException("2263", new object[] { hr.PrimaryResourceId });
            }
        }

        ClearAllowedHelpers();

        Dictionary<BaseId, Dictionary<BaseId, AllowedHelperResourcesT.HelperRelation>>.Enumerator plantEtr = relationsByPlant.GetEnumerator();
        while (plantEtr.MoveNext())
        {
            Dictionary<BaseId, AllowedHelperResourcesT.HelperRelation>.Enumerator resEtr = plantEtr.Current.Value.GetEnumerator();
            while (resEtr.MoveNext())
            {
                Resource primaryRes = resDictionary[resEtr.Current.Key];
                List<Resource> helperResources = new ();
                foreach (BaseId helperId in resEtr.Current.Value.AllowedHelperResourceIds)
                {
                    Resource helperRes = resDictionary[helperId];
                    helperResources.Add(helperRes);
                }

                a_scenarioDataChanges.MachineChanges.UpdatedObject(primaryRes.Id);
                Plant p = GetById(plantEtr.Current.Key);
                AllowedHelperManager.AllowedHelperRelation helperRelation = new (primaryRes, helperResources);
                p.AllowedHelperManager.Add(helperRelation);
            }
        }
    }

    internal void UpdateAllowedHelpers(ResourceT a_t)
    {
        ClearAllowedHelpers();

        for (int resI = 0; resI < a_t.Count; resI++)
        {
            ResourceT.Resource tResource = a_t[resI];
            Resource resource = GetResource(tResource.PlantExternalId, tResource.DepartmentExternalId, tResource.ExternalId);
            if (resource == null)
            {
                throw new ValidationException("2264", new object[] { tResource.PlantExternalId, tResource.DepartmentExternalId, tResource.ExternalId });
            }

            HashSet<BaseId> addedHelpersDictionary = new ();
            List<Resource> helpers = new ();
            for (int helperI = 0; helperI < tResource.AllowedHelpers.Count; helperI++)
            {
                ERPTransmissions.BaseResource.AllowedHelper tAllowedHelper = tResource.AllowedHelpers[helperI];
                Resource helperResource = GetResource(tAllowedHelper.PlantExternalId, tAllowedHelper.DepartmentExternalId, tAllowedHelper.ResourceExternalId);
                if (helperResource == null)
                {
                    throw new ValidationException("2265", new object[] { tAllowedHelper.PlantExternalId, tAllowedHelper.DepartmentExternalId, tAllowedHelper.ResourceExternalId });
                }

                if (resource.PlantId != helperResource.PlantId)
                {
                    throw new ValidationException("2266", new object[] { resource.Plant.ExternalId, resource.Department.ExternalId, resource.ExternalId, helperResource.Plant.ExternalId, helperResource.Department.ExternalId, helperResource.ExternalId });
                }

                if (resource.Id == helperResource.Id)
                {
                    throw new ValidationException("2267", new object[] { resource.Plant.ExternalId, resource.Department.ExternalId, resource.ExternalId });
                }

                if (addedHelpersDictionary.Contains(helperResource.Id))
                {
                    throw new ValidationException("2268", new object[] { resource.Plant.ExternalId, resource.Department.ExternalId, resource.ExternalId, helperResource.Plant.ExternalId, helperResource.Department.ExternalId, helperResource.ExternalId });
                }

                //Validation done, ok.  
                helpers.Add(helperResource);
                addedHelpersDictionary.Add(helperResource.Id);
            }

            if (helpers.Count > 0)
            {
                Plant p = resource.Plant;
                AllowedHelperManager.AllowedHelperRelation helperRelation = new (resource, helpers);
                p.AllowedHelperManager.Add(helperRelation);
            }
        }
    }

    internal void HandleDeletedResources()
    {
        Dictionary<BaseId, Resource> resDictionary = GetResourceDictionary();

        for (int plantI = 0; plantI < Count; ++plantI)
        {
            Plant plant = this[plantI];
            plant.HandleDeletedResources(resDictionary);
        }
    }
    #endregion

    #region ICopyTable
    public override Type ElementType => typeof(Plant);
    #endregion

    /// <summary>
    /// All resources in all plants and departments are added to a Dictionary based on keys created by the function BaseResource.CreateResourceKey.
    /// </summary>
    /// <returns></returns>
    internal Dictionary<string, Resource> GetResourceHash()
    {
        Dictionary<string, Resource> h = new ();

        for (int pI = 0; pI < Count; ++pI)
        {
            Plant p = this[pI];
            for (int dI = 0; dI < p.Departments.Count; ++dI)
            {
                Department d = p.Departments[dI];
                for (int rI = 0; rI < d.Resources.Count; ++rI)
                {
                    Resource r = d.Resources[rI];
                    h.Add(BaseResource.CreateResourceKey(p, d, r), r);
                }
            }
        }

        return h;
    }

    public List<Department> GetDepartments()
    {
        List<Department> departmentList = new ();

        for (int pI = 0; pI < Count; ++pI)
        {
            Plant p = this[pI];

            for (int dI = 0; dI < p.Departments.Count; ++dI)
            {
                departmentList.Add(p.Departments[dI]);
            }
        }

        return departmentList;
    }

    /// <summary>
    /// Creates a list of all resources in all plants and departments.
    /// </summary>
    /// <returns></returns>
    public List<Resource> GetResourceList()
    {
        List<Resource> resList = new ();
        foreach (Plant p in this)
        {
            foreach (Department d in p.Departments)
            {
                foreach (Resource r in d.Resources)
                {
                    resList.Add(r);
                }
            }
        }

        return resList;
    }

    /// <summary>
    /// Creates a list of all resources in all plants and departments.
    /// </summary>
    /// <returns></returns>
    public ResourceKeyList GetResourceKeyList()
    {
        ResourceKeyList list = new ();
        for (int pI = 0; pI < Count; ++pI)
        {
            Plant p = this[pI];
            for (int dI = 0; dI < p.Departments.Count; ++dI)
            {
                Department d = p.Departments[dI];
                for (int rI = 0; rI < d.Resources.Count; ++rI)
                {
                    Resource r = d.Resources[rI];
                    list.Add(new ResourceKey(r.Department.Plant.Id, r.Department.Id, r.Id));
                }
            }
        }

        return list;
    }

    internal List<Plant> GetPlantsSuppliedByWarehouse(Warehouse soughtWarehouse)
    {
        List<Plant> list = new ();
        for (int pI = 0; pI < Count; ++pI)
        {
            Plant plant = this[pI];
            for (int wI = 0; wI < plant.WarehouseCount; wI++)
            {
                Warehouse w = plant.GetWarehouseAtIndex(wI);
                if (w == soughtWarehouse)
                {
                    list.Add(plant);
                    break;
                }
            }
        }

        return list;
    }

    /// <summary>
    /// Creates a dictionary of all the resources.
    /// </summary>
    /// <returns></returns>
    private Dictionary<BaseId, Resource> GetResourceDictionary()
    {
        Dictionary<BaseId, Resource> dictionary = new ();

        for (int pI = 0; pI < Count; ++pI)
        {
            Plant p = this[pI];
            for (int dI = 0; dI < p.Departments.Count; ++dI)
            {
                Department d = p.Departments[dI];
                for (int rI = 0; rI < d.Resources.Count; ++rI)
                {
                    Resource r = d.Resources[rI];
                    dictionary.Add(r.Id, r);
                }
            }
        }

        return dictionary;
    }

    /// <summary>
    /// Creates a list of all the plants.
    /// </summary>
    /// <returns></returns>
    private List<Department> GetDepartmentList()
    {
        List<Department> departments = new ();

        for (int pI = 0; pI < Count; ++pI)
        {
            Plant p = this[pI];
            for (int dI = 0; dI < p.Departments.Count; ++dI)
            {
                departments.Add(p.Departments[dI]);
            }
        }

        return departments;
    }

    /// <summary>
    /// Sum of Invested Capital across all Plants.
    /// </summary>
    /// <returns></returns>
    public decimal GetPlantsInvestedCapital()
    {
        decimal capital = 0;
        for (int pI = 0; pI < Count; pI++)
        {
            capital += this[pI].InvestedCapital;
        }

        return capital;
    }

    /// <summary>
    /// Sum of Operating Expenses across all Plants.
    /// </summary>
    /// <returns></returns>
    public decimal GetOperatingExpensesForOnlineDaysAcrossPlants()
    {
        decimal expense = 0;
        for (int pI = 0; pI < Count; pI++)
        {
            Plant plant = this[pI];
            expense += plant.GetOperatingCostForOnlineDays(m_scenarioDetail);
        }

        return expense;
    }

    /// <summary>
    /// Sum of OnlineNonOvertimeCosts for Active Resources.
    /// </summary>
    /// <returns></returns>
    public decimal GetResourceOnlineNonOvertimeCost()
    {
        decimal cost = 0;
        List<Resource> resources = GetResourceArrayList();
        for (int r = 0; r < resources.Count; r++)
        {
            Resource resource = resources[r];
            if (resource.Active)
            {
                cost += resource.OnlineNonOvertimeCosts;
            }
        }

        return cost;
    }

    /// <summary>
    /// Sum of OvertimeCosts for Active Resources.
    /// </summary>
    /// <returns></returns>
    public decimal GetResourceOvertimeCost()
    {
        decimal cost = 0;
        List<Resource> resources = GetResourceArrayList();
        for (int r = 0; r < resources.Count; r++)
        {
            Resource resource = resources[r];
            if (resource.Active)
            {
                cost += resource.OvertimeCosts;
            }
        }

        return cost;
    }

    #region Pruning
    internal void PruneResourcesWithoutActivities(IScenarioDataChanges a_dataChanges)
    {
        foreach (Plant p in this)
        {
            foreach (Department d in p.Departments)
            {
                for (int i = d.Resources.Count - 1; i >= 0; i--)
                {
                    Resource r = d.Resources[i];
                    if (r.Blocks.Count > 0)
                    {
                        d.Resources.Delete(r, a_dataChanges);
                    }
                }
            }
        }
    }

    internal void PruneResourcesWithoutCapabilities(IScenarioDataChanges a_dataChanges)
    {
        foreach (Plant p in this)
        {
            foreach (Department d in p.Departments)
            {
                for (int i = d.Resources.Count - 1; i >= 0; i--)
                {
                    Resource r = d.Resources[i];
                    if (r.CapabilityCount == 0)
                    {
                        d.Resources.Delete(r, a_dataChanges);
                    }
                }
            }
        }
    }
    #endregion

    /// <summary>
    /// For backwards compatibility
    /// </summary>
    /// <param name="a_scenarioOptionsLegacyFrozenSpan">Deserialized obsolete frozen span</param>
    public void SetDepartmentFrozenSpanForLegacyScenarios(TimeSpan a_scenarioOptionsLegacyFrozenSpan)
    {
        foreach (Plant plant in this)
        {
            foreach (Department department in plant.Departments)
            {
                department.SetDepartmentFrozenSpanForLegacyScenarios(a_scenarioOptionsLegacyFrozenSpan);
            }
        }
    }

    /// <summary>
    /// Enumerator for resources that have at least one block scheduled
    /// </summary>
    public IEnumerable<Resource> ScheduledResources
    {
        get
        {
            foreach (Plant plant in this)
            {
                foreach (Department department in plant.Departments)
                {
                    foreach (Resource resource in department.Resources)
                    {
                        if (resource.Blocks.Count > 0)
                        {
                            yield return resource;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Enumerator for resources 
    /// </summary>
    public IEnumerable<Resource> ResourcesEnumerator
    {
        get
        {
            foreach (Plant plant in this)
            {
                foreach (Department department in plant.Departments)
                {
                    foreach (Resource resource in department.Resources)
                    {
                        yield return resource;
                    }
                }
            }
        }
    }
}
