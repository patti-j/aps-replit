using System.Data;

using PT.APSCommon;
using PT.Transmissions;
using PT.Transmissions.CleanoutTrigger;

namespace PT.ERPTransmissions;

public class LookupItemCleanoutTableT : ERPMaintenanceTransmission<ItemCleanoutTable>
{
    public LookupItemCleanoutTableT() { }

    #region IPTSerializable Members
    public new const int UNIQUE_ID = 1131;

    public LookupItemCleanoutTableT(IReader a_reader)
        : base(a_reader)
    {
        a_reader.Read(out int count);
        for (int i = 0; i < count; i++)
        {
            ItemCleanoutTable tbl = new(a_reader);
            m_itemCleanoutTableList.Add(tbl);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            m_itemCleanoutTableList[i].Serialize(a_writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private bool m_autoDelete;
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
    
    #region Properties
    /// <summary>
    /// List Item Cleanout tables
    /// </summary>
    private List<ItemCleanoutTable> m_itemCleanoutTableList = new();

    public List<ItemCleanoutTable> ItemCleanoutTableList
    {
        get => m_itemCleanoutTableList;
        set => m_itemCleanoutTableList = value;
    }

    public override int Count => ItemCleanoutTableList.Count;
    #endregion

    #region Database
    /// <summary>
    /// Fill dataset from database
    /// </summary>
    public void Fill(IDbCommand a_tableListCmd,
                     IDbCommand a_itemCleanoutsCmd,
                     IDbCommand a_assignedResourceCmd,
                     ApplicationExceptionList a_errors)
    {
        LookupItemCleanoutTableDataSet ds = new();
        FillTable(ds.TableList, a_tableListCmd);
        FillTable(ds.AssignedResources, a_assignedResourceCmd);
        FillTable(ds.ItemCleanouts, a_itemCleanoutsCmd);
        FillFromDataSet(ds, a_errors);
    }

    /// <summary>
    /// Fill the transmission with data from the DataSet.
    /// </summary>
    /// <param name="a_ds"></param>
    /// <param name="a_errors"></param>
    public void FillFromDataSet(LookupItemCleanoutTableDataSet a_ds, ApplicationExceptionList a_errors)
    {
        for (int i = 0; i < a_ds.TableList.Count; i++)
        {
            LookupItemCleanoutTableDataSet.TableListRow tableListRow = a_ds.TableList[i];

            ItemCleanoutTable newTable = new ();
            // Tables       
            newTable.Name = tableListRow.TableName;
            if (!tableListRow.IsDescriptionNull())
            {
                newTable.Description = tableListRow.Description;
            }

            if (!tableListRow.IsWildcardNull())
            {
                newTable.Wildcard = tableListRow.Wildcard;
            }

            if (!tableListRow.IsPreviousPrecedenceNull())
            {
                newTable.PreviousPrecedence = tableListRow.PreviousPrecedence;
            }

            // Item Cleanouts
            LookupItemCleanoutTableDataSet.ItemCleanoutsRow[] itemCleanouts = tableListRow.GetItemCleanoutsRows();
            foreach (LookupItemCleanoutTableDataSet.ItemCleanoutsRow itemCleanoutsRow in itemCleanouts)
            {
                try
                {
                    //Cleanout can be null. If so, use 0
                    decimal cleanCost = 0m;
                    if (!itemCleanoutsRow.IsCostNull())
                    {
                        cleanCost = itemCleanoutsRow.Cost;
                    }

                    newTable.Add(new ItemCleanoutTable.ItemCleanoutTableRow(itemCleanoutsRow.FromItemExternalId, itemCleanoutsRow.ToItemExternalId, TimeSpan.FromHours(itemCleanoutsRow.DurationHours), cleanCost));

                }
                catch (Exception e)
                {
                    a_errors.Add(new PTValidationException("3129", e, false, [newTable.Name]));
                }
            }

            LookupItemCleanoutTableDataSet.AssignedResourcesRow[] resourceList = tableListRow.GetAssignedResourcesRows();
            for (int k = 0; k < resourceList.Count(); k++)
            {
                newTable.ResourceExternalIdKeyList.Add(new SchedulerDefinitions.ResourceKeyExternal(resourceList[k].PlantExternalId, resourceList[k].DepartmentExternalId, resourceList[k].ResourceExternalId));
            }

            ItemCleanoutTableList.Add(newTable);
        }
    }
    #endregion
}