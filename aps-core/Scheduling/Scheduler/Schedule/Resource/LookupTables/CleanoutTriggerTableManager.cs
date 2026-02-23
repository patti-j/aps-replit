using System.Collections;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.ERPTransmissions;
using PT.Scheduler.ErrorReporting;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;
using PT.Transmissions.CleanoutTrigger;

namespace PT.Scheduler.Schedule.Resource.LookupTables;

public class CleanoutTriggerTableManager : BaseObjectManager<BaseCleanoutTriggerTable>, IPTSerializable
{
    #region IPTSerializable Members
    public CleanoutTriggerTableManager(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_idGen)
    {
        if (a_reader.VersionNumber >= 12322)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out int tableId);
                BaseCleanoutTriggerTable table = DeserializeTypedTable(tableId, a_reader);
                Add(table);
            }
        }
        else if (a_reader.VersionNumber >= 12305)
        {
            //This used to not be a BaseObjectManager
            BaseId nextId = new BaseId(a_reader);

            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out int tableId);
                BaseCleanoutTriggerTable table = DeserializeTypedTable(tableId, a_reader);
                Add(table);
            }
        }
    }

    private static BaseCleanoutTriggerTable DeserializeTypedTable(int a_tableId, IReader a_reader)
    {
        switch (a_tableId)
        {
            case TimeCleanoutTriggerTable.UNIQUE_ID:
                return new TimeCleanoutTriggerTable(a_reader);
            case OperationCountCleanoutTriggerTable.UNIQUE_ID:
                return new OperationCountCleanoutTriggerTable(a_reader);
            case ProductionUnitsCleanoutTriggerTable.UNIQUE_ID:
                return new ProductionUnitsCleanoutTriggerTable(a_reader);
        }

        throw new PTException($"Unable to load object with UniqueId '{a_tableId}'"); //TODO: Create a PTSerilizationException
    }

    public override void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif

        a_writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            a_writer.Write(this[i].UniqueId);
            this[i].Serialize(a_writer);
        }
    }

    public override int UniqueId => 1055;
    #endregion

    public CleanoutTriggerTableManager(BaseIdGenerator a_idGen)
        : base(a_idGen)
    {

    }

    #region List Maintenace

    public override Type ElementType => typeof(BaseCleanoutTriggerTable);

    private void Delete(ScenarioDetail a_sd, BaseCleanoutTriggerTable a_table, IScenarioDataChanges a_dataChanges)
    {
        a_table.UpdateResourcesForDelete(a_sd, a_dataChanges);
        base.Remove(a_table);
    }

    private BaseCleanoutTriggerTable Copy(BaseCleanoutTriggerTable a_sourceTable)
    {
        BaseCleanoutTriggerTable newTable = CreateCopy(a_sourceTable);
        return newTable;
    }

    #endregion

    #region Transmissions
    //An attempt at updating the tables generically. Not finished
    public void Receive(CleanoutTriggerTablesT a_t, Scenario a_s, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        ApplicationExceptionList errList = new();
        List<PostProcessingAction> actions = new();

        Dictionary<BaseId, BaseId> updatedTables = new();

        try
        {
            foreach (Transmissions.CleanoutTrigger.BaseCleanoutTriggerTable tTable in a_t)
            {
                // Move ResourceExternalidList to ResourceKeyList
                ResourceKeyList resourceKeyList = new();
                ArrayList resourceKeyExternalidList = tTable.ResourceExternalIdKeyList.CreateArrayListShallowCopy();
                for (int j = 0; j < resourceKeyExternalidList.Count; j++)
                {
                    ResourceKeyExternal resourceKeyExternalId = (ResourceKeyExternal)resourceKeyExternalidList[j];
                    Scheduler.Resource resource = a_sd.PlantManager.GetResource(resourceKeyExternalId.PlantExternalId, resourceKeyExternalId.DepartmentExternalId, resourceKeyExternalId.ResourceExternalId);
                    if (resource != null)
                    {
                        ResourceKey resourceKey = new(resource.PlantId, resource.DepartmentId, resource.Id);
                        resourceKeyList.Add(resourceKey);
                    }
                    else
                    {
                        errList.Add(new PTValidationException("3052", new object[] { tTable.Name, resourceKeyExternalId.ResourceExternalId, resourceKeyExternalId.DepartmentExternalId, resourceKeyExternalId.PlantExternalId }));
                    }
                }

                tTable.AssignedResources = resourceKeyList;

                BaseCleanoutTriggerTable existingTable = GetByName(tTable.Name);
                if (existingTable != null)
                {
                    updatedTables.Add(existingTable.Id, existingTable.Id);
                    if (existingTable is OperationCountCleanoutTriggerTable opTable)
                    {
                        opTable.Update(a_sd, (Transmissions.CleanoutTrigger.OperationCountCleanoutTriggerTable)tTable, a_dataChanges);
                    }
                    else if (existingTable is ProductionUnitsCleanoutTriggerTable prodTable)
                    {
                        prodTable.Update(a_sd, (Transmissions.CleanoutTrigger.ProductionUnitsCleanoutTriggerTable)tTable, a_dataChanges);
                    }
                    else if (existingTable is TimeCleanoutTriggerTable timeTable)
                    {
                        timeTable.Update(a_sd, (Transmissions.CleanoutTrigger.TimeCleanoutTriggerTable)tTable, a_dataChanges);
                    }
                    a_dataChanges.LookupCleanoutTriggerTablesUpdated.UpdatedObject(existingTable.Id);
                }
                else //New
                {
                    BaseCleanoutTriggerTable newTable = AddNew(a_sd, tTable, a_dataChanges);
                    a_dataChanges.LookupCleanoutTriggerTablesUpdated.AddedObject(newTable.Id);
                    updatedTables.Add(newTable.Id, newTable.Id);
                }
            }

            //AutoDelete
            for (var i = this.Count - 1; i >= 0; i--)
            {
                BaseCleanoutTriggerTable table = GetByIndex(i);
                if ((AutoDeleteTable(a_t, table) || a_t.AutoDelete) && !updatedTables.ContainsKey(table.Id))
                {
                    a_dataChanges.LookupCleanoutTriggerTablesUpdated.DeletedObject(table.Id);
                }
            }

            if (a_dataChanges.LookupCleanoutTriggerTablesUpdated.TotalDeletedObjects > 0)
            {
                ScenarioExceptionInfo sei = new();
                sei.Create(a_sd);
                actions.Add(new PostProcessingAction(a_t, false, () =>
                    {
                        try
                        {
                            foreach (BaseId id in a_dataChanges.LookupCleanoutTriggerTablesUpdated.Deleted)
                            {
                                Delete(a_sd, GetById(id), a_dataChanges);
                            }
                        }
                        catch (PTHandleableException err)
                        {
                            m_errorReporter.LogException(err, a_t, sei, ELogClassification.PtSystem, err.LogToSentry);
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
            a_sd.AddProcessingAction(actions);
            if (errList.Count > 0)
            {
                ScenarioExceptionInfo sei = new();
                sei.Create(a_sd);
                m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
            }
        }
    }

    private static bool AutoDeleteTable(CleanoutTriggerTablesT a_t, BaseCleanoutTriggerTable a_table)
    {
        Type tableType = a_table.GetType();

        if (tableType == typeof(OperationCountCleanoutTriggerTable))
        {
            return a_t.AutoDeleteOperationCountCleanoutTriggers;
        }

        if (tableType == typeof(ProductionUnitsCleanoutTriggerTable))
        {
            return a_t.AutoDeleteProductionUnitsCleanoutTriggers;
        }

        if (tableType == typeof(TimeCleanoutTriggerTable))
        {
            return a_t.AutoDeleteTimeCleanoutTriggers;
        }

        throw new PTException($"Unable to determine table type for transmission with type '{tableType.Name}'"); //TODO: Create a PTSerilizationException
    }

    // TODO: It would be nice to not have the duplication for these receives
    public void Receive(TimeCleanoutTriggerTableBaseT a_t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        if (a_t is TimeCleanoutTriggerTableIdBaseT)
        {
            TimeCleanoutTriggerTableIdBaseT idT = (TimeCleanoutTriggerTableIdBaseT)a_t;
            TimeCleanoutTriggerTable table = GetById(idT.Id) as TimeCleanoutTriggerTable;
            if (table == null)
            {
                throw new PTValidationException("3051", new object[] { idT.Id });
            }

            if (a_t is TimeCleanoutTriggerTableUpdateT)
            {
                table.Update(a_sd, ((TimeCleanoutTriggerTableUpdateT)idT).TimeCleanoutTriggerTable, a_dataChanges);
                a_dataChanges.LookupCleanoutTriggerTablesUpdated.UpdatedObject(table.Id);
            }
            else if (a_t is TimeCleanoutTriggerTableDeleteT)
            {
                Delete(a_sd, table, a_dataChanges);
                a_dataChanges.LookupCleanoutTriggerTablesUpdated.DeletedObject(table.Id);
            }
            else if (a_t is TimeCleanoutTriggerTableCopyT)
            {
                BaseCleanoutTriggerTable copyTable = Copy(table);
                Add(copyTable);
                a_dataChanges.LookupCleanoutTriggerTablesUpdated.AddedObject(copyTable.Id);
            }
        }
        else if (a_t is TimeCleanoutTriggerTableNewT)
        {
            BaseCleanoutTriggerTable newTable = AddNew(a_sd, ((TimeCleanoutTriggerTableNewT)a_t).CleanoutTriggerTable, a_dataChanges);
            a_dataChanges.LookupCleanoutTriggerTablesUpdated.AddedObject(newTable.Id);
        }
    }

    public void Receive(OperationCountCleanoutTriggerTableBaseT a_t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        if (a_t is OperationCountCleanoutTriggerTableIdBaseT)
        {
            OperationCountCleanoutTriggerTableIdBaseT idT = (OperationCountCleanoutTriggerTableIdBaseT)a_t;
            OperationCountCleanoutTriggerTable table = GetById(idT.Id) as OperationCountCleanoutTriggerTable;
            if (table == null)
            {
                throw new PTValidationException("3051", new object[] { idT.Id });
            }

            if (a_t is OperationCountCleanoutTriggerTableUpdateT updateT)
            {
                table.Update(a_sd, updateT.OperationCountCleanoutTriggerTable, a_dataChanges);
                a_dataChanges.LookupCleanoutTriggerTablesUpdated.UpdatedObject(table.Id);
            }
            else if (a_t is OperationCountCleanoutTriggerTableDeleteT)
            {
                Delete(a_sd, table, a_dataChanges);
                a_dataChanges.LookupCleanoutTriggerTablesUpdated.DeletedObject(table.Id);
            }
            else if (a_t is OperationCountCleanoutTriggerTableCopyT)
            {
                BaseCleanoutTriggerTable copyTable = Copy(table);
                Add(copyTable);
                a_dataChanges.LookupCleanoutTriggerTablesUpdated.AddedObject(copyTable.Id);
            }
        }
        else if (a_t is OperationCountCleanoutTriggerTableNewT)
        {
            BaseCleanoutTriggerTable newTable = AddNew(a_sd, ((OperationCountCleanoutTriggerTableNewT)a_t).CleanoutTriggerTable, a_dataChanges);
            a_dataChanges.LookupCleanoutTriggerTablesUpdated.AddedObject(newTable.Id);
        }
    }

    public void Receive(ProductionUnitsCleanoutTriggerTableBaseT a_t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        if (a_t is ProductionUnitsCleanoutTriggerTableIdBaseT)
        {
            ProductionUnitsCleanoutTriggerTableIdBaseT idT = (ProductionUnitsCleanoutTriggerTableIdBaseT)a_t;
            ProductionUnitsCleanoutTriggerTable table = GetById(idT.Id) as ProductionUnitsCleanoutTriggerTable;
            if (table == null)
            {
                throw new PTValidationException("3051", new object[] { idT.Id });
            }

            if (a_t is ProductionUnitsCleanoutTriggerTableUpdateT updateT)
            {
                table.Update(a_sd, updateT.ProductionUnitsCleanoutTriggerTable, a_dataChanges);
                a_dataChanges.LookupCleanoutTriggerTablesUpdated.UpdatedObject(table.Id);
            }
            else if (a_t is ProductionUnitsCleanoutTriggerTableDeleteT)
            {
                Delete(a_sd, table, a_dataChanges);
                a_dataChanges.LookupCleanoutTriggerTablesUpdated.DeletedObject(table.Id);
            }
            else if (a_t is ProductionUnitsCleanoutTriggerTableCopyT)
            {
                BaseCleanoutTriggerTable copyTable = Copy(table);
                Add(copyTable);
                a_dataChanges.LookupCleanoutTriggerTablesUpdated.AddedObject(copyTable.Id);
            }
        }
        else if (a_t is ProductionUnitsCleanoutTriggerTableNewT)
        {
            BaseCleanoutTriggerTable newTable = AddNew(a_sd, ((ProductionUnitsCleanoutTriggerTableNewT)a_t).CleanoutTriggerTable, a_dataChanges);
            a_dataChanges.LookupCleanoutTriggerTablesUpdated.AddedObject(newTable.Id);
        }
    }

    /// <summary>
    /// Delete all tables and fire ScenarioEvents update event.
    /// </summary>
    internal void DeleteAll(ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        for (var i = this.Count - 1; i >= 0; i--)
        {
            BaseCleanoutTriggerTable table = this[i];
            Delete(a_sd, table, a_dataChanges);
            a_dataChanges.LookupCleanoutTriggerTablesUpdated.DeletedObject(table.Id);
        }
    }

    private BaseCleanoutTriggerTable AddNew(ScenarioDetail a_sd, Transmissions.CleanoutTrigger.BaseCleanoutTriggerTable a_tTable, IScenarioDataChanges a_dataChanges)
    {
        BaseCleanoutTriggerTable newTable = CreateNewFromTransmission(NextID(), a_sd, a_tTable, a_dataChanges);
        Add(newTable);
        a_dataChanges.LookupCleanoutTriggerTablesUpdated.AddedObject(newTable.Id);
        return newTable;
    }
    #endregion

    #region Factory Helpers
    /// <summary>
    /// Factory method to create a copy of the right CleanoutTriggerTable type.
    /// </summary>
    /// <param name="a_ctt"></param>
    /// <returns></returns>
    /// <exception cref="PTException"></exception>
    private BaseCleanoutTriggerTable CreateCopy(BaseCleanoutTriggerTable a_ctt)
    {
        switch (a_ctt)
        {
            case TimeCleanoutTriggerTable timeCleanoutTriggerTable:
                return new TimeCleanoutTriggerTable(NextID(), timeCleanoutTriggerTable);
            case ProductionUnitsCleanoutTriggerTable productionUnitsCleanoutTriggerTable:
                return new ProductionUnitsCleanoutTriggerTable(NextID(), productionUnitsCleanoutTriggerTable);
            case OperationCountCleanoutTriggerTable operationCountCleanoutTriggerTable:
                return new OperationCountCleanoutTriggerTable(NextID(), operationCountCleanoutTriggerTable);
        }

        throw new PTException($"Unable to load object with UniqueId '{a_ctt.UniqueId}'"); //TODO: Create a PTSerilizationException
    }

    /// <summary>
    /// Factory method to create a new trigger table using the corresponding transmission.
    /// </summary>
    /// <param name="a_ctt"></param>
    /// <param name="a_tTable"></param>
    /// <param name="a_dataChanges"></param>
    /// <param name="a_sd"></param>
    /// <returns></returns>
    /// <exception cref="PTException"></exception>
    private BaseCleanoutTriggerTable CreateNewFromTransmission(BaseId a_newId, ScenarioDetail a_sd, Transmissions.CleanoutTrigger.BaseCleanoutTriggerTable a_tTable, IScenarioDataChanges a_dataChanges)
    {
        switch (a_tTable)
        {
            case Transmissions.CleanoutTrigger.TimeCleanoutTriggerTable timeCleanoutTriggerTable:
                return new TimeCleanoutTriggerTable(a_newId, timeCleanoutTriggerTable, a_sd, a_dataChanges);
            case Transmissions.CleanoutTrigger.ProductionUnitsCleanoutTriggerTable productionUnitsCleanoutTriggerTable:
                return new ProductionUnitsCleanoutTriggerTable(a_newId, productionUnitsCleanoutTriggerTable, a_sd, a_dataChanges);
            case Transmissions.CleanoutTrigger.OperationCountCleanoutTriggerTable operationCountCleanoutTriggerTable:
                return new OperationCountCleanoutTriggerTable(a_newId, operationCountCleanoutTriggerTable, a_sd, a_dataChanges);
        }

        throw new PTException($"Unable to load object with UniqueId '{a_tTable.UniqueId}'"); //TODO: Create a PTSerilizationException
    }

    /// <summary>
    /// Returns the Cleanout Table class in Scheduler that corresponds to the ERP Transmission's model.
    /// </summary>
    /// <param name="a_transmissionTableType"></param>
    /// <returns></returns>
    private Type GetCleanoutTableTypeFromTransmissionModel(Type a_transmissionTableType)
    {
        if (a_transmissionTableType == typeof(Transmissions.CleanoutTrigger.TimeCleanoutTriggerTable))
        {
            return typeof(TimeCleanoutTriggerTable);
        }

        if (a_transmissionTableType == typeof(Transmissions.CleanoutTrigger.ProductionUnitsCleanoutTriggerTable))
        {
            return typeof(ProductionUnitsCleanoutTriggerTable);
        }

        if (a_transmissionTableType == typeof(Transmissions.CleanoutTrigger.OperationCountCleanoutTriggerTable))
        {
            return typeof(OperationCountCleanoutTriggerTable);
        }

        throw new PTException($"Unable to determine table type for transmission with type '{a_transmissionTableType.Name}'"); //TODO: Create a PTSerilizationException
    }
    #endregion

    private ISystemLogger m_errorReporter;

    public void RestoreReferences(ISystemLogger a_errorReporter)
    {
        m_errorReporter = a_errorReporter;
    }
}