using System.ComponentModel;

using PT.APSCommon;
using PT.APSCommon.Compression;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of DispatcherDefinition objects.
/// </summary>
public class DispatcherDefinitionManager : ScenarioBaseIdObjectManager<DispatcherDefinition>, IPTSerializable
{
    #region IPTSerializable Members
    internal DispatcherDefinitionManager(IReader reader, BaseIdGenerator a_idGen)
        : base(a_idGen)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                int dispatcherType;
                reader.Read(out dispatcherType);
                if (dispatcherType == BalancedCompositeDispatcherDefinition.UNIQUE_ID)
                {
                    BalancedCompositeDispatcherDefinition d = new (reader);
                    Add(d);
                }
                else
                {
                    throw new PTException("Tried to deserialize an invalid DispatcherDefinition type.");
                }
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        //base.Serialize(writer); Don't use base serializer

        writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            DispatcherDefinition definition = GetByIndex(i);
            if (definition is BalancedCompositeDispatcherDefinition)
            {
                writer.Write(BalancedCompositeDispatcherDefinition.UNIQUE_ID);
            }
            else
            {
                throw new PTException("Tried to Serialize an invalid DispatcherDefintion type.");
            }

            definition.Serialize(writer);
        }
    }

    public new const int UNIQUE_ID = 412;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    internal class DispatcherDefinitionManagerException : PTValidationException
    {
        internal DispatcherDefinitionManagerException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(a_message, a_stringParameters, a_appendHelpUrl) { }
    }

    internal class DeleteValidationException : PTValidationException
    {
        internal DeleteValidationException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(a_message, a_stringParameters, a_appendHelpUrl) { }
    }
    #endregion

    #region Construction
    internal DispatcherDefinitionManager(BaseIdGenerator a_idGen)
        : base(a_idGen)
    {
        BalancedCompositeDispatcherDefinition dispatcher = new (NextID(), "Default".Localize());
        Add(dispatcher);
    }
    #endregion

    #region Find functions
    /// <summary>
    /// Find by Name (case in-senstive name).
    /// </summary>
    /// <param name="a_name"></param>
    /// <returns>First Dispatcher Definition with provided Name or null if none exists with provided Name</returns>
    internal DispatcherDefinition GetByName(string a_name)
    {
        for (int i = 0; i < Count; i++)
        {
            DispatcherDefinition def = this[i];
            if (def.Name.Equals(a_name, StringComparison.CurrentCultureIgnoreCase))
            {
                return def;
            }
        }

        return null;
    }
    #endregion

    #region DispatcherDefinition Edit Functions
    private BalancedCompositeDispatcherDefinition AddDefault(string a_name)
    {
        BalancedCompositeDispatcherDefinition d = new (NextID(), a_name);
        ValidateAdd(d);

        return (BalancedCompositeDispatcherDefinition)base.Add(d);
    }

    private BalancedCompositeDispatcherDefinition AddCopy(BalancedCompositeDispatcherDefinitionCopyT a_t)
    {
        ValidateCopy(a_t);
        BalancedCompositeDispatcherDefinition d = (BalancedCompositeDispatcherDefinition)GetById(a_t.originalId);
        BalancedCompositeDispatcherDefinition newDD = new (d, NextID());
        BalancedCompositeDispatcherDefinition bcdd = (BalancedCompositeDispatcherDefinition)AddCopy(d, newDD);
        bcdd.Name = string.Format("{0} {1}".Localize(), "Copy of".Localize(), d.Name); //Not a BaseObject so name copy must be done here.
        return bcdd;
    }

    /// <summary>
    /// The first dispatcher.  Can't be deleted.
    /// </summary>
    private DispatcherDefinition ZerothDispatcherDef => GetByIndex(0);

    private BalancedCompositeDispatcherDefinition Delete(BalancedCompositeDispatcherDefinitionDeleteT a_t, ScenarioDetail a_sd)
    {
        BalancedCompositeDispatcherDefinition d = (BalancedCompositeDispatcherDefinition)GetById(a_t.balancedCompositeDispatcherDefinitionId);
        Delete(d, a_sd);
        return d;
    }

    private void Delete(BalancedCompositeDispatcherDefinition a_d, ScenarioDetail a_sd)
    {
        if (ValidateDelete(a_d, a_sd))
        {
            Remove(a_d.Id);
        }
    }

    /// <summary>
    /// Sets all the resources DispatcherDefinitions to the ZerothDispatcherDefinition, which
    /// cannot be deleted. This is currently used to prepare for clearing all the data
    /// </summary>
    /// <param name="a_dispatcherDefinition">The dispatcher definition for all the resources</param>
    /// <param name="a_sd">The scenario detail containing all the relevant resources</param>
    private static void SetAllResourcesDispatcherDefinitions(DispatcherDefinition a_dispatcherDefinition, ScenarioDetail a_sd)
    {
        for (int plantI = 0; plantI < a_sd.PlantManager.Count; plantI++)
        {
            Plant plant = a_sd.PlantManager[plantI];
            for (int deptI = 0; deptI < plant.DepartmentCount; deptI++)
            {
                Department dept = plant.Departments[deptI];
                for (int resI = 0; resI < dept.Resources.Count; resI++)
                {
                    Resource res = dept.Resources[resI];
                    res.CreateDispatcher(a_dispatcherDefinition);
                    res.CreateExperimentalDispatcherOne(a_dispatcherDefinition);
                    res.CreateExperimentalDispatcherTwo(a_dispatcherDefinition);
                    res.CreateExperimentalDispatcherThree(a_dispatcherDefinition);
                    res.CreateExperimentalDispatcherFour(a_dispatcherDefinition);
                }
            }
        }
    }

    public new BalancedCompositeDispatcherDefinition this[int index] => (BalancedCompositeDispatcherDefinition)GetByIndex(index);

    public void Clear(ScenarioDetail a_sd, IScenarioDataChanges a_scenarioDataChanges)
    {
        // Need to reset the zeroth DispatcherDefinition to truly "clear" the data
        // This BalancedCompositeDispatcherDefinition constructor defaults the name to "New Sequence Plan"
        BalancedCompositeDispatcherDefinition newDefaultDefinition = new (NextID()); 
        BaseId definitionToBeDeletedId;
        SetAllResourcesDispatcherDefinitions(newDefaultDefinition, a_sd);
        for (int i = Count - 1; i > 0; i--)
        {
            definitionToBeDeletedId = this[i].Id;
            Delete(this[i], a_sd);
            a_scenarioDataChanges.BalancedCompositeDispatcherDefinitionChanges.DeletedObject(definitionToBeDeletedId);
        }

        // Remove does not validate so it'll allow us to remove the zeroth dispatcher
        definitionToBeDeletedId = ZerothDispatcherDef.Id;
        Remove(ZerothDispatcherDef);
        a_scenarioDataChanges.BalancedCompositeDispatcherDefinitionChanges.DeletedObject(definitionToBeDeletedId);
        Add(newDefaultDefinition);
        a_scenarioDataChanges.BalancedCompositeDispatcherDefinitionChanges.AddedObject(newDefaultDefinition.Id);
    }
    #endregion

    #region Transmissions
    private void ValidateAdd(DispatcherDefinition a_d)
    {
        if (Contains(a_d.Id))
        {
            throw new DispatcherDefinitionManagerException("2355", new object[] { a_d.Id.ToString() });
        }

        foreach (DispatcherDefinition dispatcherDefinition in this)
        {
            if (dispatcherDefinition.Name == a_d.Name)
            {
                throw new DispatcherDefinitionManagerException("4479", new object[] { a_d.Name.ToString() });
            }
        }
    }

    private void ValidateCopy(BalancedCompositeDispatcherDefinitionCopyT a_t)
    {
        ValidateExistence(a_t.originalId);
    }

    /// <summary>
    /// Returns true if the DispatcherDefinition passed in can be deleted, false if not
    /// </summary>
    /// <param name="a_dispatcher">DispatcherDefinition to be deleted</param>
    /// <param name="a_sd">scenario detail</param>
    /// <returns></returns>
    /// <exception cref="DeleteValidationException"></exception>
    private bool ValidateDelete(DispatcherDefinition a_dispatcher, ScenarioDetail a_sd)
    {
        if (Count == 1)
        {
            throw new DeleteValidationException("2282");
        }

        if (IsDispatcherDefinitionInUse(a_dispatcher.Id, a_sd.PlantManager.GetResourceList()))
        {
            throw new DeleteValidationException("3041", new object[] { a_dispatcher.Id });
        }

        ValidateExistence(a_dispatcher.Id);

        return true;
    }

    /// <summary>
    /// Checks if ths DispatcherDefinition is being used by any resources as a sequencing plan.
    /// </summary>
    /// <param name="a_definitionId"></param>
    /// <param name="a_resources"></param>
    /// <returns>true if DispatcherDefinition is in-used by another resource, false if not</returns>
    private static bool IsDispatcherDefinitionInUse(BaseId a_definitionId, List<Resource> a_resources)
    {
        foreach (Resource resource in a_resources)
        {
            if (resource.IsDispatcherUsedByResource(a_definitionId) || resource.NormalDispatcherId == a_definitionId)
            {
                return true;
            }
        }

        return false;
    }

    internal void Receive(BalancedCompositeDispatcherDefinitionBaseT a_t, IScenarioDataChanges a_dataChanges)
    {
        BalancedCompositeDispatcherDefinition d = null;

        if (a_t is BalancedCompositeDispatcherDefinitionDefaultT newT)
        {
            CreateNewDefinition(newT.Name, a_dataChanges);
        }
        else if (a_t is BalancedCompositeDispatcherDefinitionCopyT)
        {
            d = AddCopy((BalancedCompositeDispatcherDefinitionCopyT)a_t);
            a_dataChanges.BalancedCompositeDispatcherDefinitionChanges.AddedObject(d.Id);
        }
        else if (a_t is BalancedCompositeDispatcherDefinitionDeleteT)
        {
            d = Delete((BalancedCompositeDispatcherDefinitionDeleteT)a_t, m_scenarioDetail);
            a_dataChanges.BalancedCompositeDispatcherDefinitionChanges.DeletedObject(d.Id);
        }
        else if (a_t is BalancedCompositeDispatcherDefinitionDeleteAllT)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                d = (BalancedCompositeDispatcherDefinition)GetByIndex(i);
                BalancedCompositeDispatcherDefinitionDeleteT deleteT = new (a_t.scenarioId, d.Id);
                deleteT.Instigator = a_t.Instigator;
                Delete(deleteT, m_scenarioDetail);
                a_dataChanges.BalancedCompositeDispatcherDefinitionChanges.DeletedObject(d.Id);
            }
        }
        else if (a_t is BalancedCompositeDispatcherDefinitionUpdateT updateT)
        {
            d = (BalancedCompositeDispatcherDefinition)GetById(updateT.balancedCompositeDispatcherDefinitionId);
            d.Receive(updateT, a_dataChanges);
        }
        else if (a_t is BalancedCompositeDispatcherDefinitionImportT importT)
        {
            using (BinaryMemoryReader reader = new (importT.BalancedCompositeDispatcherDefinitionData))
            {
                CompressedDataContainer<BalancedCompositeDispatcherDefinition> compressedDataContainer = new(reader);

                BalancedCompositeDispatcherDefinition newDispatcherDefinition = new BalancedCompositeDispatcherDefinition(compressedDataContainer.LoadData(), IdGen.NextID());
                newDispatcherDefinition.Name = importT.Name;
                Add(newDispatcherDefinition);
                a_dataChanges.BalancedCompositeDispatcherDefinitionChanges.AddedObject(newDispatcherDefinition.Id);
            }

        }
    }

    private void CreateNewDefinition(string a_name, IScenarioDataChanges a_dataChanges)
    {
        BalancedCompositeDispatcherDefinition newDispatcherDefinition = AddDefault(a_name);
        a_dataChanges.BalancedCompositeDispatcherDefinitionChanges.AddedObject(newDispatcherDefinition.Id);
    }

    private void ValidateDefault(SetDefaultDispatcherDefinitionOfDefinitionManagerT a_t)
    {
        DispatcherDefinition dispatcherDefinition = GetById(m_defaultDefinitionId);

        if (dispatcherDefinition == null)
        {
            throw new DispatcherDefinitionManagerException("2356");
        }
    }

    internal void Receive(DispatcherDefinitionManagerBaseT a_t)
    {
        if (a_t is SetDefaultDispatcherDefinitionOfDefinitionManagerT)
        {
            SetDefaultDispatcherDefinitionOfDefinitionManagerT defaultTransmission = (SetDefaultDispatcherDefinitionOfDefinitionManagerT)a_t;
            ValidateDefault(defaultTransmission);
            m_defaultDefinitionId = defaultTransmission.defaultDefinitionId;
        }
    }
    #endregion Internal Transmissions

    #region ICopyTable
    public override Type ElementType => typeof(DispatcherDefinition);
    #endregion

    private BaseId m_defaultDefinitionId;

    internal DispatcherDefinition DefaultDispatcherDefinition
    {
        get
        {
            DispatcherDefinition definition = GetById(m_defaultDefinitionId);
            if (definition == null)
            {
                return GetByIndex(0);
            }

            return definition;
        }
    }

    #region Simulation
    internal void SimulationInitialization(ScenarioDetail a_sd)
    {
        for (int i = 0; i < Count; ++i)
        {
            BalancedCompositeDispatcherDefinition dd = this[i];
            dd.SimulationInitialization(a_sd);
        }
    }
    #endregion
}