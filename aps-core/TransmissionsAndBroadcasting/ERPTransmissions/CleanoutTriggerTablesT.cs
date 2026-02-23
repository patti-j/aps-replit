using System.Collections;
using System.Data;

using PT.APSCommon;
using PT.Common.File;
using PT.SchedulerDefinitions;
using PT.Transmissions;
using PT.Transmissions.CleanoutTrigger;

namespace PT.ERPTransmissions;

public class CleanoutTriggerTablesT : ERPTransmission, IEnumerable<BaseCleanoutTriggerTable>
{
    public CleanoutTriggerTablesT() { }

    #region IPTSerializable Members
    public const int UNIQUE_ID = 1079;

    public CleanoutTriggerTablesT(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12411)
        {
            a_reader.Read(out int opCountTableCount);
            for (int i = 0; i < opCountTableCount; i++)
            {
                OperationCountCleanoutTriggerTable tbl = new(a_reader);
                AddOperationCountCleanoutTriggerTable(tbl);
            }

            a_reader.Read(out int prodUnitsTableCount);
            for (int i = 0; i < prodUnitsTableCount; i++)
            {
                ProductionUnitsCleanoutTriggerTable tbl = new(a_reader);
                AddProductionUnitsCleanoutTriggerTable(tbl);
            }

            a_reader.Read(out int timeTableCount);
            for (int i = 0; i < timeTableCount; i++)
            {
                TimeCleanoutTriggerTable tbl = new(a_reader);
                AddTimeCleanoutTriggerTable(tbl);
            }

            a_reader.Read(out m_autoDelete);
            a_reader.Read(out m_autoDeleteOperationCountCleanoutTriggers);
            a_reader.Read(out m_autoDeleteProductionUnitsCleanoutTriggers);
            a_reader.Read(out m_autoDeleteTimeCleanoutTriggers);
        }
        else if (a_reader.VersionNumber >= 12400)
        {
            a_reader.Read(out int opCountTableCount);
            for (int i = 0; i < opCountTableCount; i++)
            {
                OperationCountCleanoutTriggerTable tbl = new(a_reader);
                AddOperationCountCleanoutTriggerTable(tbl);
            }

            a_reader.Read(out int prodUnitsTableCount);
            for (int i = 0; i < prodUnitsTableCount; i++)
            {
                ProductionUnitsCleanoutTriggerTable tbl = new(a_reader);
                AddProductionUnitsCleanoutTriggerTable(tbl);
            }

            a_reader.Read(out int timeTableCount);
            for (int i = 0; i < timeTableCount; i++)
            {
                TimeCleanoutTriggerTable tbl = new(a_reader);
                AddTimeCleanoutTriggerTable(tbl);
            }
        }
        else if (a_reader.VersionNumber >= 12328)
        {
            a_reader.Read(out int opCountTableCount);
            for (int i = 0; i < opCountTableCount; i++)
            {
                OperationCountCleanoutTriggerTable tbl = new(a_reader);
                AddOperationCountCleanoutTriggerTable(tbl);
            }

            a_reader.Read(out int prodUnitsTableCount);
            for (int i = 0; i < prodUnitsTableCount; i++)
            {
                ProductionUnitsCleanoutTriggerTable tbl = new(a_reader);
                AddProductionUnitsCleanoutTriggerTable(tbl);
            }

            a_reader.Read(out int timeTableCount);
            for (int i = 0; i < timeTableCount; i++)
            {
                TimeCleanoutTriggerTable tbl = new(a_reader);
                AddTimeCleanoutTriggerTable(tbl);
            }

            a_reader.Read(out m_autoDelete);
            a_reader.Read(out m_autoDeleteOperationCountCleanoutTriggers);
            a_reader.Read(out m_autoDeleteProductionUnitsCleanoutTriggers);
            a_reader.Read(out m_autoDeleteTimeCleanoutTriggers);
        }
        else if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out int opCountTableCount);
            for (int i = 0; i < opCountTableCount; i++)
            {
                OperationCountCleanoutTriggerTable tbl = new (a_reader);
                AddOperationCountCleanoutTriggerTable(tbl);
            }

            a_reader.Read(out int prodUnitsTableCount);
            for (int i = 0; i < prodUnitsTableCount; i++)
            {
                ProductionUnitsCleanoutTriggerTable tbl = new(a_reader);
                AddProductionUnitsCleanoutTriggerTable(tbl);
            }

            a_reader.Read(out int timeTableCount);
            for (int i = 0; i < timeTableCount; i++)
            {
                TimeCleanoutTriggerTable tbl = new(a_reader);
                AddTimeCleanoutTriggerTable(tbl);
            }
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_operationCountCleanoutTriggerTables.Count);
        foreach (OperationCountCleanoutTriggerTable tbl in m_operationCountCleanoutTriggerTables)
        {
            tbl.Serialize(a_writer);
        }

        a_writer.Write(m_productionUnitsCleanoutTriggerTables.Count);
        foreach (ProductionUnitsCleanoutTriggerTable tbl in m_productionUnitsCleanoutTriggerTables)
        {
            tbl.Serialize(a_writer);
        }

        a_writer.Write(m_timeCleanoutTriggerTables.Count);
        foreach (TimeCleanoutTriggerTable tbl in m_timeCleanoutTriggerTables)
        {
            tbl.Serialize(a_writer);
        }

        a_writer.Write( m_autoDelete);
        a_writer.Write( m_autoDeleteOperationCountCleanoutTriggers);
        a_writer.Write( m_autoDeleteProductionUnitsCleanoutTriggers);
        a_writer.Write(m_autoDeleteTimeCleanoutTriggers);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Database
    /// <summary>
    /// Fill dataset from database
    /// </summary>
    public void Fill(IDbCommand a_tableListCmd,
                     IDbCommand a_assignedResourceCmd,
                     IDbCommand a_operationCountCleanoutTriggerTables,
                     bool a_importOperationCountCleanoutTriggerTables,
                     IDbCommand a_timeCleanoutTriggerTables,
                     bool a_importTimeCleanoutTriggerTables,
                     IDbCommand a_productionUnitsCleanoutTriggerTables,
                     bool a_importProductionUnitsCleanoutTriggerTables,
                     ApplicationExceptionList a_errors)
    {
        CleanoutTriggerTablesDataSet ds = new ();
        FillTable(ds.TableList, a_tableListCmd);
        FillTable(ds.AssignedResources, a_assignedResourceCmd);
        if (a_importOperationCountCleanoutTriggerTables)
        {
            FillTable(ds.OperationCountCleanoutTriggers, a_operationCountCleanoutTriggerTables);
        }

        if (a_importTimeCleanoutTriggerTables)
        {
            FillTable(ds.TimeCleanoutTriggers, a_timeCleanoutTriggerTables);
        }

        if (a_importProductionUnitsCleanoutTriggerTables)
        {
            FillTable(ds.ProductionUnitsCleanoutTriggers, a_productionUnitsCleanoutTriggerTables);
        }
        
        FillFromDataSet(ds, a_errors);
    }

    protected void FillTable(DataTable a_table, IDbCommand a_cmd)
    {
        FillDataTable(a_table, a_cmd, GetType().Name);
    }

    /// <summary>
    /// Fill the transmission with data from the DataSet.
    /// </summary>
    /// <param name="a_ds"></param>
    /// <param name="a_errors"></param>
    public void FillFromDataSet(CleanoutTriggerTablesDataSet a_ds, ApplicationExceptionList a_errors)
    {
        ValidateOpCountCleanoutTriggerTables(a_ds, a_errors);
        ValidateTimeCleanoutTriggerTables(a_ds, a_errors);
        ValidateProductionUnitTriggerTables(a_ds, a_errors);

        for (int i = 0; i < a_ds.TableList.Count; i++)
        {
            CleanoutTriggerTablesDataSet.TableListRow tableListRow = a_ds.TableList[i];

            if (!ValidateTableName(tableListRow, a_errors))
            {
                continue;
            }

            CleanoutTriggerTablesDataSet.OperationCountCleanoutTriggersRow[] opCountTableRows = tableListRow.GetOperationCountCleanoutTriggersRows();
            if (opCountTableRows.Length > 0)
            {
                OperationCountCleanoutTriggerTable newTable = new ();
                // Tables           
                newTable.Name = tableListRow.TableName;
                if (!tableListRow.IsDescriptionNull())
                {
                    newTable.Description = tableListRow.Description;
                }

                foreach (CleanoutTriggerTablesDataSet.OperationCountCleanoutTriggersRow operationCountCleanoutTriggersRow in opCountTableRows)
                {
                    try
                    {
                        newTable.Add(new OperationCountCleanoutTriggerTableRow(
                            TimeSpan.FromHours(operationCountCleanoutTriggersRow.DurationHours),
                            operationCountCleanoutTriggersRow.IsCleanoutGradeNull() ? 0 : operationCountCleanoutTriggersRow.CleanoutGrade,
                            operationCountCleanoutTriggersRow.CleanCost,
                            operationCountCleanoutTriggersRow.TriggerValue));
                    }
                    catch (Exception e)
                    {
                        a_errors.Add(new PTValidationException("3057", e, false, new object[] { newTable.Name }));
                        continue;
                    }
                   

                    CleanoutTriggerTablesDataSet.AssignedResourcesRow[] resourceList = tableListRow.GetAssignedResourcesRows();
                    for (int k = 0; k < resourceList.Count(); k++)
                    {
                        newTable.ResourceExternalIdKeyList.Add(new ResourceKeyExternal(resourceList[k].PlantExternalId, resourceList[k].DepartmentExternalId, resourceList[k].ResourceExternalId));
                    }

                    AddOperationCountCleanoutTriggerTable(newTable);
                }
            }

            CleanoutTriggerTablesDataSet.ProductionUnitsCleanoutTriggersRow[] productionUnitsCleanoutTriggersRows = tableListRow.GetProductionUnitsCleanoutTriggersRows();
            if (productionUnitsCleanoutTriggersRows.Length > 0)
            {
                ProductionUnitsCleanoutTriggerTable newTable = new ();
                // Tables           
                newTable.Name = tableListRow.TableName;
                if (!tableListRow.IsDescriptionNull())
                {
                    newTable.Description = tableListRow.Description;
                }

                foreach (CleanoutTriggerTablesDataSet.ProductionUnitsCleanoutTriggersRow productionUnitsCleanoutTriggersRow in productionUnitsCleanoutTriggersRows)
                {
                    try
                    {
                        CleanoutDefs.EProductionUnitsCleanType productionUnitsCleanType = Enum.Parse<CleanoutDefs.EProductionUnitsCleanType>(productionUnitsCleanoutTriggersRow.ProductionUnit);
                                
                        newTable.Add(new ProductionUnitsCleanoutTriggerTableRow(
                            TimeSpan.FromHours(productionUnitsCleanoutTriggersRow.DurationHours),
                            productionUnitsCleanoutTriggersRow.IsCleanoutGradeNull() ? 0 : productionUnitsCleanoutTriggersRow.CleanoutGrade,
                            productionUnitsCleanoutTriggersRow.IsCleanCostNull() ? 0 : productionUnitsCleanoutTriggersRow.CleanCost,
                            productionUnitsCleanoutTriggersRow.TriggerValue,
                            productionUnitsCleanType));
                    }
                    catch (Exception e)
                    {
                        a_errors.Add(new PTValidationException("3058", e, false, new object[] { newTable.Name }));
                        continue;
                    }

                    CleanoutTriggerTablesDataSet.AssignedResourcesRow[] resourceList = tableListRow.GetAssignedResourcesRows();
                    for (int k = 0; k < resourceList.Count(); k++)
                    {
                        newTable.ResourceExternalIdKeyList.Add(new ResourceKeyExternal(resourceList[k].PlantExternalId, resourceList[k].DepartmentExternalId, resourceList[k].ResourceExternalId));
                    }

                    AddProductionUnitsCleanoutTriggerTable(newTable);
                }
            }

            CleanoutTriggerTablesDataSet.TimeCleanoutTriggersRow[] timeCleanoutTriggersRows = tableListRow.GetTimeCleanoutTriggersRows();
            if (timeCleanoutTriggersRows.Length > 0)
            {
                TimeCleanoutTriggerTable newTable = new();
                // Tables           
                newTable.Name = tableListRow.TableName;
                if (!tableListRow.IsDescriptionNull())
                {
                    newTable.Description = tableListRow.Description;
                }

                foreach (CleanoutTriggerTablesDataSet.TimeCleanoutTriggersRow timeCleanoutTriggersRow in timeCleanoutTriggersRows)
                {
                    try
                    {
                        newTable.Add(new TimeCleanoutTriggerTableRow(
                            TimeSpan.FromHours(timeCleanoutTriggersRow.DurationHours),
                            timeCleanoutTriggersRow.IsCleanoutGradeNull() ? 0 : timeCleanoutTriggersRow.CleanoutGrade,
                            timeCleanoutTriggersRow.IsCleanCostNull() ? 0m : timeCleanoutTriggersRow.CleanCost,
                            TimeSpan.FromHours(timeCleanoutTriggersRow.TriggerValueHours),
                            !timeCleanoutTriggersRow.IsUseProcessingTimeNull() && timeCleanoutTriggersRow.UseProcessingTime,
                            !timeCleanoutTriggersRow.IsUsePostProcessingTimeNull() && timeCleanoutTriggersRow.UsePostProcessingTime,
                            !timeCleanoutTriggersRow.IsTriggerAtEndNull() && timeCleanoutTriggersRow.TriggerAtEnd));
                    }
                    catch (Exception e)
                    {
                        a_errors.Add(new PTValidationException("3059", e, false, new object[] { newTable.Name }));
                        continue;
                    }

                    CleanoutTriggerTablesDataSet.AssignedResourcesRow[] resourceList = tableListRow.GetAssignedResourcesRows();
                    for (int k = 0; k < resourceList.Count(); k++)
                    {
                        newTable.ResourceExternalIdKeyList.Add(new ResourceKeyExternal(resourceList[k].PlantExternalId, resourceList[k].DepartmentExternalId, resourceList[k].ResourceExternalId));
                    }

                    AddTimeCleanoutTriggerTable(newTable);
                }
            }
        }
    }

    /// <summary>
    /// Validates that the parent table has an entry for each Operation Count Cleanout Trigger Table table name
    /// </summary>
    /// <param name="a_ds"></param>
    /// <param name="a_errors"></param>
    private void ValidateOpCountCleanoutTriggerTables(CleanoutTriggerTablesDataSet a_ds, ApplicationExceptionList a_errors)
    {
        HashSet<CleanoutTriggerTablesDataSet.TableListRow> tables = a_ds.TableList.ToHashSet();
        HashSet<CleanoutTriggerTablesDataSet.OperationCountCleanoutTriggersRow> opCountCleanoutTriggers = a_ds.OperationCountCleanoutTriggers.ToHashSet();

        foreach (CleanoutTriggerTablesDataSet.OperationCountCleanoutTriggersRow opCountTriggerRow in opCountCleanoutTriggers)
        {
            if (!tables.Any(t => t.TableName == opCountTriggerRow.TableName))
            {
                a_errors.Add(new PTValidationException("3054", new object[] { opCountTriggerRow.TableName }));
            }
        }
    }

    /// <summary>
    /// Validates that the parent table has an entry for each Time Cleanout Trigger Table table name
    /// </summary>
    /// <param name="a_ds"></param>
    /// <param name="a_errors"></param>
    private void ValidateTimeCleanoutTriggerTables(CleanoutTriggerTablesDataSet a_ds, ApplicationExceptionList a_errors)
    {
        HashSet<CleanoutTriggerTablesDataSet.TableListRow> tables = a_ds.TableList.ToHashSet();
        HashSet<CleanoutTriggerTablesDataSet.TimeCleanoutTriggersRow> opCountCleanoutTriggers = a_ds.TimeCleanoutTriggers.ToHashSet();

        foreach (CleanoutTriggerTablesDataSet.TimeCleanoutTriggersRow timeCleanoutTriggersRow in opCountCleanoutTriggers)
        {
            if (!tables.Any(t => t.TableName == timeCleanoutTriggersRow.TableName))
            {
                a_errors.Add(new PTValidationException("3055", new object[] { timeCleanoutTriggersRow.TableName }));
            }
        }
    }

    /// <summary>
    /// Validates that the parent table has an entry for each Production Unit Cleanout Trigger Table table name
    /// </summary>
    /// <param name="a_ds"></param>
    /// <param name="a_errors"></param>
    private void ValidateProductionUnitTriggerTables(CleanoutTriggerTablesDataSet a_ds, ApplicationExceptionList a_errors)
    {
        HashSet<CleanoutTriggerTablesDataSet.TableListRow> tables = a_ds.TableList.ToHashSet();
        HashSet<CleanoutTriggerTablesDataSet.ProductionUnitsCleanoutTriggersRow> opCountCleanoutTriggers = a_ds.ProductionUnitsCleanoutTriggers.ToHashSet();

        foreach (CleanoutTriggerTablesDataSet.ProductionUnitsCleanoutTriggersRow productionUnitsCleanoutTriggersRow in opCountCleanoutTriggers)
        {
            if (!tables.Any(t => t.TableName == productionUnitsCleanoutTriggersRow.TableName))
            {
                a_errors.Add(new PTValidationException("3056", new object[] { productionUnitsCleanoutTriggersRow.TableName }));
            }
        }
    }

    /// <summary>
    /// Makes sure that each of the tables being imported have unique names
    /// </summary>
    /// <param name="a_tableListRow"></param>
    /// <param name="a_errors"></param>
    /// <returns></returns>
    private static bool ValidateTableName(CleanoutTriggerTablesDataSet.TableListRow a_tableListRow, ApplicationExceptionList a_errors)
    {
        int count = a_tableListRow.GetProductionUnitsCleanoutTriggersRows().Length;
        count += a_tableListRow.GetOperationCountCleanoutTriggersRows().Length;
        count += a_tableListRow.GetTimeCleanoutTriggersRows().Length;

        if (count > 1)
        {
            a_errors.Add(new PTValidationException("3053", new object[] { a_tableListRow.TableName }));
            return false;
        }

        return true;
    }
    #endregion

    /// <summary>
    /// Auto deletes all clean out triggers omitted from the new data regardless of the clean out type
    /// </summary>
    public bool AutoDelete
    {
        get => m_autoDelete;
        set
        {
            m_autoDelete = value;
        }
    }

    private bool m_autoDelete;

    /// <summary>
    /// Auto deletes all Operation clean out triggers omitted from the new data
    /// </summary>
    public bool AutoDeleteOperationCountCleanoutTriggers
    {
        get => m_autoDeleteOperationCountCleanoutTriggers;
        set
        {
            m_autoDeleteOperationCountCleanoutTriggers = value;
        }
    }
    private bool m_autoDeleteOperationCountCleanoutTriggers;
    /// <summary>
    /// Auto deletes all Production Unit clean out triggers omitted from the new data
    /// </summary>
    public bool AutoDeleteProductionUnitsCleanoutTriggers 
    { 
        get => m_autoDeleteProductionUnitsCleanoutTriggers;

        set
        {
            m_autoDeleteProductionUnitsCleanoutTriggers = value;
        }
    }
    private bool m_autoDeleteProductionUnitsCleanoutTriggers;

    /// <summary>
    /// Auto deletes all Time clean out triggers omitted from the new data
    /// </summary>
    public bool AutoDeleteTimeCleanoutTriggers
    {
        get => m_autoDeleteTimeCleanoutTriggers;
        set
        {
            m_autoDeleteTimeCleanoutTriggers = value;
        }
    }
    private bool m_autoDeleteTimeCleanoutTriggers;

    private List<OperationCountCleanoutTriggerTable> m_operationCountCleanoutTriggerTables = new();
    private List<ProductionUnitsCleanoutTriggerTable> m_productionUnitsCleanoutTriggerTables = new();
    private List<TimeCleanoutTriggerTable> m_timeCleanoutTriggerTables = new();

    private void AddOperationCountCleanoutTriggerTable(OperationCountCleanoutTriggerTable a_tbl)
    {
        m_operationCountCleanoutTriggerTables.Add(a_tbl);
    }

    private void AddProductionUnitsCleanoutTriggerTable(ProductionUnitsCleanoutTriggerTable a_tbl)
    {
        m_productionUnitsCleanoutTriggerTables.Add(a_tbl);
    }

    private void AddTimeCleanoutTriggerTable(TimeCleanoutTriggerTable a_tbl)
    {
        m_timeCleanoutTriggerTables.Add(a_tbl);
    }

    public IEnumerator<BaseCleanoutTriggerTable> GetEnumerator()
    {
        foreach (OperationCountCleanoutTriggerTable tbl in m_operationCountCleanoutTriggerTables)
        {
            yield return tbl;
        }

        foreach (ProductionUnitsCleanoutTriggerTable tbl in m_productionUnitsCleanoutTriggerTables)
        {
            yield return tbl;
        }

        foreach (TimeCleanoutTriggerTable tbl in m_timeCleanoutTriggerTables)
        {
            yield return tbl;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}