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
/// Stores a list of SetupCode Tables.
/// </summary>
public class CompatibilityCodeTableManager : BaseObjectManager<CompatibilityCodeTable>
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 1111;
    
    public override int UniqueId => UNIQUE_ID;
    #endregion

    internal CompatibilityCodeTableManager(BaseIdGenerator a_idGen) : base(a_idGen)
    {
    }

    public CompatibilityCodeTableManager(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_idGen)
    {
        if (a_reader.VersionNumber >= 12320)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                CompatibilityCodeTable r = new (a_reader);
                Add(r);
            }
        }
    }

    #region list maintenance
    public override Type ElementType => typeof(CompatibilityCodeTable);

    private void Delete(ScenarioDetail a_sd, CompatibilityCodeTable a_table, IScenarioDataChanges a_dataChanges)
    {
        a_table.UpdateResourcesForDelete(a_sd, a_dataChanges);
        Remove(a_table.Id);
    }
    
    /// <summary>
    /// Delete all tables and fire ScenarioEvents update event.
    /// </summary>
    internal void DeleteAll(ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            CompatibilityCodeTable delTable = this[i];
            Delete(a_sd, delTable, a_dataChanges);
            a_dataChanges.CompatibilityCodeChanges.DeletedObject(delTable.Id);
        }
        Clear();
    }

    private CompatibilityCodeTable AddNew(ScenarioDetail sd, Transmissions.CompatibilityCodeTable tTable, IScenarioDataChanges a_dataChanges)
    {
        CompatibilityCodeTable newTable = new (NextID(), tTable, sd, a_dataChanges);
        Add(newTable);
        return newTable;
    }
    private CompatibilityCodeTable Copy(CompatibilityCodeTable a_sourceTable)
    {
        CompatibilityCodeTable newTable = new (NextID(), a_sourceTable);
        return newTable;
    }
    #endregion

    #region Transmissions
    public void Receive(ERPTransmissions.CompatibilityCodeTableT a_t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        ApplicationExceptionList errList = new();
        List<PostProcessingAction> actions = new();

        try
        {
            for (int tI = 0; tI < a_t.Count; tI++)
            {
                PT.Transmissions.CompatibilityCodeTable tTable = a_t[tI];

                // Move ResourceExternalidList to ResourceKeyList
                PTLinkedList<ResourceKey> resourceKeyList = new();
                ArrayList resourceKeyExternalidList = tTable.AssignedResourcesExternalId.CreateArrayListShallowCopy();
                for (int j = 0; j < resourceKeyExternalidList.Count; j++)
                {
                    ResourceKeyExternal resourceKeyExternalId = (ResourceKeyExternal)resourceKeyExternalidList[j];
                    Resource resource = a_sd.PlantManager.GetResource(resourceKeyExternalId.PlantExternalId, resourceKeyExternalId.DepartmentExternalId, resourceKeyExternalId.ResourceExternalId);
                    if (resource != null)
                    {
                        ResourceKey resourceKey = new(resource.PlantId, resource.DepartmentId, resource.Id);
                        resourceKeyList.Add(resourceKey);
                    }
                    else
                    {
                        errList.Add(new PTValidationException("2753", new object[] { tTable.Name, resourceKeyExternalId.ResourceExternalId, resourceKeyExternalId.DepartmentExternalId, resourceKeyExternalId.PlantExternalId }));
                    }
                }

                tTable.AssignedResources = resourceKeyList;

                CompatibilityCodeTable sTable = GetByName(tTable.Name, true);
                if (sTable != null)
                {
                    sTable.Update(a_sd, tTable, a_dataChanges);
                    a_dataChanges.CompatibilityCodeChanges.UpdatedObject(sTable.Id);
                }
                else //New
                {
                    CompatibilityCodeTable newTable = new CompatibilityCodeTable(NextID(), tTable, a_sd, a_dataChanges);
                    Add(newTable);
                    a_dataChanges.CompatibilityCodeChanges.AddedObject(newTable.Id);
                }
            }

            if (a_t.AutoDeleteMode)
            {
                for (int i = Count - 1; i >= 0; i--)
                {
                    CompatibilityCodeTable table = this[i];
                    if (!a_dataChanges.CompatibilityCodeChanges.Updated.Contains(table.Id) 
                        && !a_dataChanges.CompatibilityCodeChanges.Added.Contains(table.Id))
                    {
                        a_dataChanges.CompatibilityCodeChanges.DeletedObject(table.Id);
                    }
                }

                if (a_dataChanges.CompatibilityCodeChanges.TotalDeletedObjects > 0)
                {
                    ScenarioExceptionInfo sei = new();
                    sei.Create(a_sd);
                    actions.Add(new PostProcessingAction(a_t, false, () =>
                        {
                            try
                            {
                                foreach (BaseId id in a_dataChanges.CompatibilityCodeChanges.Deleted)
                                {
                                    Delete(a_sd, GetById(id), a_dataChanges);
                                }
                            }
                            catch (PTHandleableException err)
                            {
                                m_errorReporter.LogException(err, a_t, sei, ELogClassification.PtSystem,err.LogToSentry);
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

    public void Receive(CompatibilityCodeTableBaseT t, ScenarioDetail sd, IScenarioDataChanges a_dataChanges)
    {
        if (t is CompatibilityCodeTableIdBaseT)
        {
            CompatibilityCodeTableIdBaseT idT = (CompatibilityCodeTableIdBaseT)t;
            CompatibilityCodeTable table = GetById(idT.Id);
            if (table == null)
            {
                throw new PTValidationException("2754", new object[] { idT.Id });
            }

            if (t is CompatibilityCodeTableUpdateT)
            {
                table.Update(sd, ((CompatibilityCodeTableUpdateT)idT).CompatibilityCodeTable, a_dataChanges);
                a_dataChanges.CompatibilityCodeChanges.UpdatedObject(table.Id);
            }
            else if (t is CompatibilityCodeTableDeleteT)
            {
                Delete(sd, table, a_dataChanges);
                a_dataChanges.CompatibilityCodeChanges.DeletedObject(table.Id);
            }
            else if (t is CompatibilityCodeTableCopyT)
            {
                CompatibilityCodeTable copyTable = Copy(table);
                Add(copyTable);
                a_dataChanges.CompatibilityCodeChanges.AddedObject(copyTable.Id);
            }
        }
        else if (t is CompatibilityCodeTableNewT)
        {
            CompatibilityCodeTable newTable = AddNew(sd, ((CompatibilityCodeTableNewT)t).CompatibilityCodeTable, a_dataChanges);
            a_dataChanges.CompatibilityCodeChanges.AddedObject(newTable.Id);
        }
    }
    #endregion

    private ISystemLogger m_errorReporter;

    public void RestoreReferences(ISystemLogger a_errorReporter)
    {
        m_errorReporter = a_errorReporter;
    }

    internal void ResetSimulationStateVariables()
    {
        foreach (CompatibilityCodeTable codeTable in this)
        {
            codeTable.ResetSimulationStateVariables();
        }
    }
}