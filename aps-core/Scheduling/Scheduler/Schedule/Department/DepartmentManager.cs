using System.ComponentModel;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.ERPTransmissions;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of Department objects.
/// </summary>
public partial class DepartmentManager : BaseObjectManager<Department>, IPTSerializable
{
    #region IPTSerializable Members
    public DepartmentManager(IReader reader, BaseIdGenerator idGen)
        : base(idGen)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                Department d = new (reader, idGen);
                Add(d);
            }
        }
    }

    public new const int UNIQUE_ID = 331;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    internal void RestoreReferences(CustomerManager a_cm, CapabilityManager capabilities, CapacityIntervalManager cis, RecurringCapacityIntervalManager rcis, DispatcherDefinitionManager dispatcherDefs, PlantManager plantManager, ItemManager itemManager, ScenarioDetail aScenarioDetail)
    {
        m_scenarioDetail = aScenarioDetail;

        for (int i = 0; i < Count; i++)
        {
            this[i].RestoreReferences(a_cm, capabilities, cis, rcis, dispatcherDefs, plantManager, itemManager, aScenarioDetail);
        }
    }

    internal void RestoreReferences(Scenario a_scenario, ISystemLogger a_errorReporter)
    {
        for (int i = 0; i < Count; i++)
        {
            this[i].RestoreReferences(a_scenario, m_scenarioDetail, a_errorReporter);
        }
    }

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        foreach (Department department in this)
        {
            department.RestoreReferences(a_udfManager);
        }
    }
    #endregion

    #region Declarations
    public class DepartmentManagerException : PTException
    {
        public DepartmentManagerException(string message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(message, a_stringParameters, a_appendHelpUrl) { }
    }
    #endregion

    #region Construction
    public DepartmentManager(ScenarioDetail aScenarioDetail, BaseIdGenerator idGen)
        : base(idGen)
    {
        m_scenarioDetail = aScenarioDetail;
    }
    #endregion

    #region Find functions
    public Resource GetResource(ResourceKey a_key)
    {
        Department dept = GetById(a_key.Department);
        if (dept != null)
        {
            return dept.Resources.GetById(a_key.Resource);
        }

        return null;
    }

    public Resource GetResource(BaseId a_resId)
    {
        for (int di = 0; di < Count; ++di)
        {
            Department d = this[di];
            Resource r = d.GetResource(a_resId);
            if (r != null)
            {
                return r;
            }
        }

        return null;
    }
    #endregion

    #region Department Edit Functions
    internal new Department Add(Department a_dept)
    {
        return base.Add(a_dept);
    }

    private ScenarioDetail m_scenarioDetail;

    /// <summary>
    /// Add a new department with default values and one default resource
    /// </summary>
    /// <param name="p"></param>
    /// <param name="a_t"></param>
    /// <param name="a_dataChanges"></param>
    /// <returns></returns>
    internal Department AddDefault(Plant p, DepartmentDefaultT a_t, IScenarioDataChanges a_dataChanges)
    {
        Department d = new (NextID(), p, m_scenarioDetail);
        ValidateAdd(d);
        a_dataChanges.DepartmentChanges.AddedObject(d.Id);
        a_dataChanges.AuditEntry(new AuditEntry(d.Id, d.Plant.Id, d), true);

        d.Resources.AddDefault(d, a_dataChanges);

        return Add(d);
    }

    private Department AddCopy(DepartmentCopyT t)
    {
        ValidateCopy(t);
        Department department = GetById(t.originalId);
        return AddCopy(department, department.Clone(NextID(), m_scenarioDetail));
    }

    private Department Delete(DepartmentDeleteT t, IScenarioDataChanges a_dataChanges)
    {
        Department d = GetById(t.departmentId);
        Delete(d, a_dataChanges);
        return d;
    }

    internal void DeleteTry(Department d, IScenarioDataChanges a_dataChanges)
    {
        Delete(d, a_dataChanges);
    }

    internal void Delete(Department d, IScenarioDataChanges a_dataChanges)
    {
        a_dataChanges.AuditEntry(new AuditEntry(d.Id, d.Plant.Id, d), false, true);
        d.Deleting(a_dataChanges);
        a_dataChanges.DepartmentChanges.DeletedObject(d.Id);
        Remove(d.Id); //Now remove it from the Manager.
    }

    /// <summary>
    /// Call when deleting the DepartmentManager itself.
    /// </summary>
    internal void Deleting(IScenarioDataChanges a_dataChanges)
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            Department d = this[i];
            Delete(d, a_dataChanges);
        }
    }
    #endregion

    #region Internal Transmissions
    private void ValidateAdd(Department w)
    {
        if (Contains(w.Id))
        {
            throw new DepartmentManagerException("2744", new object[] { w.Id.ToString() });
        }
    }

    private void ValidateCopy(DepartmentCopyT t)
    {
        ValidateExistence(t.originalId);
    }

    internal void Receive(Plant a_p, DepartmentBaseT a_t, IScenarioDataChanges a_dataChanges)
    {
        Department d;
        if (a_t is DepartmentDefaultT defaultT)
        {
            d = AddDefault(a_p, defaultT, a_dataChanges);
            a_dataChanges.PlantChanges.UpdatedObject(d.Plant.Id);
        }
        else if (a_t is DepartmentCopyT copyT)
        {
            d = AddCopy(copyT);
            a_dataChanges.PlantChanges.UpdatedObject(d.Plant.Id);
            a_dataChanges.DepartmentChanges.AddedObject(d.Id);
            a_dataChanges.AuditEntry(new AuditEntry(d.Id, d.Plant.Id, d), true);
        }
        else if (a_t is DepartmentDeleteAllT)
        {
            a_dataChanges.PlantChanges.UpdatedObject(a_p.Id);
            for (int i = Count - 1; i >= 0; i--)
            {
                d = GetByIndex(i);
                Delete(d, a_dataChanges);
                a_dataChanges.DepartmentChanges.DeletedObject(d.Id);
            }
        }
        else if (a_t is DepartmentIdBaseT idBaseT)
        {
            if (idBaseT is DepartmentDeleteT deleteT)
            {
                ValidateExistence(idBaseT.departmentId);
                Department deletedDepartment = Delete(deleteT, a_dataChanges);
                a_dataChanges.DepartmentChanges.DeletedObject(deletedDepartment.Id);
                a_dataChanges.PlantChanges.UpdatedObject(deletedDepartment.Plant.Id);
            }
            else if (idBaseT is BaseResourceT baseResourceT)
            {
                if (idBaseT is ResourceDeleteMultiT resourceDeleteMultiT)
                {
                    List<Department> uniqueDepartments = new List<Department>();
                    foreach (ResourceKey key in resourceDeleteMultiT.ResourceKeys)
                    {
                        if(key.Plant == a_p.Id)
                        {
                            try
                            {
                                d = ValidateExistence(key.Department);
                            }
                            catch (PTValidationException e)
                            {
                                continue;
                            }
                            uniqueDepartments.AddIfNew(d);
                            
                        }
                    }

                    foreach (Department department in uniqueDepartments)
                    {
                        department.Receive(resourceDeleteMultiT, a_dataChanges);

                        if (department.ResourceCount == 0)
                        {
                            DeleteTry(department, a_dataChanges);
                        }
                    }
                }
                else
                {
                    d = ValidateExistence(idBaseT.departmentId);
                    d.Receive(baseResourceT, a_dataChanges);
                }
            }
        }
    }

    internal void Receive(ScenarioDetailClearT t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        for (int i = 0; i < Count; i++)
        {
            this[i].Receive(t,a_sd, a_dataChanges);
        }

        if (t.ClearDepartments)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                Department department = this[i];
                if (department.ResourceCount == 0) //Only delete departments that are empty
                {
                    Delete(department, a_dataChanges);
                }
            }
        }
    }
    #endregion Internal Transmissions

    #region ERP Transmissions
    internal enum DepartmentTHandlerResult { added, updated }

    internal DepartmentTHandlerResult DepartmentTHandler(ScenarioDetail a_sd, UserFieldDefinitionManager a_udfManager, DepartmentT.Department wcn, Plant plant, out Department dept, PTTransmission t, IScenarioDataChanges a_dataChanges)
    {
        if (wcn.IdSet)
        {
            dept = GetById(wcn.Id);
            if (dept == null)
            {
                throw new ValidationException("2172", new object[] { wcn.Id });
            }
        }
        else
        {
            dept = GetByExternalId(wcn.ExternalId);
        }

        if (dept == null)
        {
            dept = new Department(NextID(), wcn, plant, m_scenarioDetail, a_udfManager);
            Add(dept);
            a_dataChanges.AuditEntry(new AuditEntry(dept.Id, dept.Plant.Id, dept), true);
            return DepartmentTHandlerResult.added;
        }

        AuditEntry deptAuditEntry = new AuditEntry(dept.Id, dept.Plant.Id, dept);
        dept.Update(a_sd, a_udfManager, wcn, t);
        a_dataChanges.AuditEntry(deptAuditEntry);
        return DepartmentTHandlerResult.updated;
    }

    internal new void Remove(Department dept)
    {
        base.Remove(dept.Id);
    }
    #endregion ERP Transmissions

    #region ICopyTable
    public override Type ElementType => typeof(Department);
    #endregion

    //TODO: Finish deep copy
    //public DepartmentManager DeepCopy(ScenarioDetail a_sd, BaseIdGenerator a_idGen, Plant a_plant)
    //{
    //    DepartmentManager newDepartmentManager = new DepartmentManager(a_sd, a_idGen);
    //    foreach (Department department in this)
    //    {
    //        Department d = department.Clone(a_idGen, a_sd, a_plant);
    //        newDepartmentManager.Add(d);
    //    }

    //    return newDepartmentManager;
    //}
}
