using System.Collections;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.Scheduler.ErrorReporting;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Stores a list of ItemCleanout Tables.
/// </summary>
public class ItemCleanoutTableManager : BaseObjectManager<ItemCleanoutTable>, IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 1133;

    internal ItemCleanoutTableManager(IReader a_reader, BaseIdGenerator a_idGen) : base (a_idGen)
    {
        a_reader.Read(out int count);
        for (int i = 0; i < count; i++)
        {
            Add(new ItemCleanoutTable(a_reader));
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        a_writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(a_writer);
        }
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    internal ItemCleanoutTableManager(BaseIdGenerator a_idGen)
        : base(a_idGen)
    {

    }

    #region list maintenance

    public override Type ElementType => typeof(ItemCleanoutTable);

    
    private void Delete(ScenarioDetail a_sd, ItemCleanoutTable a_table, IScenarioDataChanges a_dataChanges)
    {
        a_table.UpdateResourcesForDelete(a_sd, a_dataChanges);
        Remove(a_table);
    }

    private ItemCleanoutTable Copy(ItemCleanoutTable a_sourceTable)
    {
        ItemCleanoutTable newTable = new(IdGen.NextID(), a_sourceTable);
        return newTable;
    }
    #endregion

    #region Transmissions
    internal void Receive(ItemCleanoutTableBaseT a_t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        if (a_t is ItemCleanoutTableIdBaseT idT)
        {
            ItemCleanoutTable table = GetById(idT.Id);
            if (table == null)
            {
                throw new PTValidationException("3128", [idT.Id]);
            }

            if (a_t is ItemCleanoutTableUpdateT updateT)
            {
                table.Update(a_sd, updateT.ItemCleanoutTable, a_dataChanges);
                a_dataChanges.LookupItemCleanoutTablesUpdated.UpdatedObject(table.Id);
            }
            else if (a_t is ItemCleanoutTableDeleteT)
            {
                Delete(a_sd, table, a_dataChanges);
                a_dataChanges.LookupItemCleanoutTablesUpdated.DeletedObject(table.Id);
            }
            else if (a_t is ItemCleanoutTableCopyT)
            {
                ItemCleanoutTable copyTable = Copy(table);
                Add(copyTable);
                a_dataChanges.LookupItemCleanoutTablesUpdated.AddedObject(copyTable.Id);
            }
        }
        else if (a_t is ItemCleanoutTableNewT newTableT)
        {
            ItemCleanoutTable newTable = AddNew(a_sd, newTableT.ItemCleanoutTable, a_dataChanges);
            a_dataChanges.LookupItemCleanoutTablesUpdated.AddedObject(newTable.Id);
        }
    }

    /// <summary>
    /// ERP Transmission processing
    /// </summary>
    /// <param name="a_t"></param>
    /// <param name="a_s"></param>
    /// <param name="a_sd"></param>
    /// <param name="a_dataChanges"></param>
    internal void Receive(ERPTransmissions.LookupItemCleanoutTableT a_t, Scenario a_s, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        List<PostProcessingAction> actions = new();
        ApplicationExceptionList errList = new();

        HashSet<string> tablesInT = new();

        try
        {
            foreach (Transmissions.CleanoutTrigger.ItemCleanoutTable itemCleanoutTable in a_t.ItemCleanoutTableList)
            {
                try
                {
                    tablesInT.Add(itemCleanoutTable.Name);

                    // Move ResourceExternalIdList to PTLinkedList<ResourceKey>
                    PTLinkedList<ResourceKey> resourceKeyList = new();
                    ArrayList resExternalIds = itemCleanoutTable.ResourceExternalIdKeyList.CreateArrayListShallowCopy();
                    foreach (ResourceKeyExternal resExternalIdKey in resExternalIds)
                    {
                        Resource resource = a_sd.PlantManager.GetResource(resExternalIdKey.PlantExternalId, resExternalIdKey.DepartmentExternalId, resExternalIdKey.ResourceExternalId);
                        if (resource != null)
                        {
                            ResourceKey resourceKey = new(resource.PlantId, resource.DepartmentId, resource.Id);
                            resourceKeyList.Add(resourceKey);
                        }
                        else
                        {
                            errList.Add(new PTValidationException("3052", [itemCleanoutTable.Name, resExternalIdKey.ResourceExternalId, resExternalIdKey.DepartmentExternalId, resExternalIdKey.PlantExternalId]));
                        }
                    }

                    itemCleanoutTable.AssignedResources = resourceKeyList;
                    ItemCleanoutTable table = FindByName(itemCleanoutTable.Name);
                    // New Table, add
                    if (table == null)
                    {
                        ItemCleanoutTable newTable = AddNew(a_sd, itemCleanoutTable, a_dataChanges);
                        a_dataChanges.LookupItemCleanoutTablesUpdated.AddedObject(newTable.Id);
                    }
                    // Existing table, remove and add
                    else
                    {
                        Delete(a_sd, table, a_dataChanges);
                        ItemCleanoutTable newTable = AddNew(a_sd, itemCleanoutTable, a_dataChanges);
                        a_dataChanges.LookupItemCleanoutTablesUpdated.UpdatedObject(newTable.Id);
                    }
                }
                catch (PTHandleableException err)
                {
                    errList.Add(err);
                }
            }

            // remove for omitted records
            if (a_t.AutoDeleteMode)
            {
                // Determine deleted records, can't delete here because enumerator can't be modified
                foreach (ItemCleanoutTable itemCleanoutTable in this)
                {
                    if (!tablesInT.Contains(itemCleanoutTable.Name))
                    {
                        a_dataChanges.LookupItemCleanoutTablesUpdated.DeletedObject(itemCleanoutTable.Id);
                    }
                }
               
                if (a_dataChanges.LookupItemCleanoutTablesUpdated.TotalDeletedObjects > 0)
                {
                    ScenarioExceptionInfo sei = new();
                    sei.Create(a_sd);
                    actions.Add(new PostProcessingAction(a_t, false, () =>
                    {
                        try
                        {
                            //Now delete
                            foreach (BaseId id in a_dataChanges.LookupItemCleanoutTablesUpdated.Deleted)
                            {
                                ItemCleanoutTable tbl = GetById(id);
                                Delete(a_sd, tbl, a_dataChanges);
                            }
                        }
                        catch (PTHandleableException err)
                        {
                            m_errorReporter.LogException(err, a_t, sei, ELogClassification.PtInterface, false);
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
            a_sd.AddProcessingAction(actions);

            if (errList.Count > 0)
            {
                ScenarioExceptionInfo sei = new();
                sei.Create(a_sd);
                m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
            }
        }
    }

    /// <summary>
    /// Return the attribute code table if it already exists
    /// </summary>
    /// <param name="a_itemCleanoutTableName"></param>
    /// <returns></returns>
    internal ItemCleanoutTable FindByName(string a_itemCleanoutTableName)
    {
        foreach (ItemCleanoutTable itemCleanoutTable in this)
        {
            if (itemCleanoutTable.Name.ToUpperInvariant() == a_itemCleanoutTableName.ToUpperInvariant())
            {
                return itemCleanoutTable;
            }
        }

        return null;
    }

    /// <summary>
    /// Delete all tables and fire ScenarioEvents update event.
    /// </summary>
    internal void DeleteAll(ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            ItemCleanoutTable delTable = this[i];
            Delete(a_sd, delTable, a_dataChanges);
            a_dataChanges.LookupItemCleanoutTablesUpdated.DeletedObject(delTable.Id);
        }
    }

    private ItemCleanoutTable AddNew(ScenarioDetail a_sd, PT.Transmissions.CleanoutTrigger.ItemCleanoutTable a_t, IScenarioDataChanges a_dataChanges)
    {
        ItemCleanoutTable newTable = new(IdGen.NextID(), a_t, a_sd, a_dataChanges);
        Add(newTable);
        return newTable;
    }
    #endregion

    private ISystemLogger m_errorReporter;

    internal void RestoreReferences(ISystemLogger a_errorReporter)
    {
        m_errorReporter = a_errorReporter;
    }
}