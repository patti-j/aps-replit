using PT.Scheduler;
using PT.Transmissions;

namespace PT.ERPTransmissions;

public class CompatibilityCodeTableT : ERPMaintenanceTransmission<CompatibilityCodeTable>, IPTSerializable
{
    public CompatibilityCodeTableT() { }

    #region IPTSerializable Members
    public new const int UNIQUE_ID = 1105;

    public CompatibilityCodeTableT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                CompatibilityCodeTable tbl = new (a_reader);
                Add(tbl);
            }
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(a_writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Database
    /// <summary>
    /// Fill dataset from database
    /// </summary>
    public void Fill(System.Data.IDbCommand a_tableListCmd, System.Data.IDbCommand a_compatibilityCodesCmd, System.Data.IDbCommand a_assignedResourceCmd)
    {
        CompatibilityCodeTableDataSet ds = new ();
        FillTable(ds.CompatibilityCodeTableList, a_tableListCmd);
        FillTable(ds.CompatibilityCodeTableAssignedResources, a_assignedResourceCmd);
        FillTable(ds.CompatibilityCodes, a_compatibilityCodesCmd);
        FillFromDataSet(ds);
    }

    /// <summary>
    /// Fill the transmission with data from the DataSet.
    /// </summary>
    /// <param name="a_ds"></param>
    public void FillFromDataSet(CompatibilityCodeTableDataSet a_ds)
    {
        for (int i = 0; i < a_ds.CompatibilityCodeTableList.Count; i++)
        {
            CompatibilityCodeTableDataSet.CompatibilityCodeTableListRow tableListRow = a_ds.CompatibilityCodeTableList[i];

            CompatibilityCodeTable newTable = new ();
            // Tables           
            newTable.Name = tableListRow.TableName;
            if (!tableListRow.IsDescriptionNull())
            {
                newTable.Description = tableListRow.Description;
            }

            newTable.AllowedList = tableListRow.AllowedList;

            // Attribute Names
            CompatibilityCodeTableDataSet.CompatibilityCodesRow[] compatibilityCodesRows = tableListRow.GetCompatibilityCodesRows();

            for (int j = 0; j < compatibilityCodesRows.Count(); j++)
            {
                CompatibilityCodeTableDataSet.CompatibilityCodesRow compatibilityCodesRow = (CompatibilityCodeTableDataSet.CompatibilityCodesRow)compatibilityCodesRows.GetValue(j);
                newTable.Add(new CompatibilityCodeTable.CompatibilityCodeTableRow(compatibilityCodesRow.CompatibilityCode));
            }

            CompatibilityCodeTableDataSet.CompatibilityCodeTableAssignedResourcesRow[] resourceList = tableListRow.GetAssignedResourcesRows();
            for (int k = 0; k < resourceList.Count(); k++)
            {
                newTable.AssignedResourcesExternalId.Add(new SchedulerDefinitions.ResourceKeyExternal(resourceList[k].PlantExternalId, resourceList[k].DepartmentExternalId, resourceList[k].ResourceExternalId));
            }

            Add(newTable);
        }
    }
    #endregion
}