using PT.Transmissions;

namespace PT.ERPTransmissions;

public class LookupAttributeCodeTableT : ERPMaintenanceTransmission<AttributeCodeTable>, IPTSerializable
{
    public LookupAttributeCodeTableT() { }

    #region IPTSerializable Members
    public new const int UNIQUE_ID = 716;

    public LookupAttributeCodeTableT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                AttributeCodeTable tbl = new(reader);
                m_attributeCodeTableList.Add(tbl);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            m_attributeCodeTableList[i].Serialize(writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Properties
    /// <summary>
    /// List attribute code tables
    /// </summary>
    private List<AttributeCodeTable> m_attributeCodeTableList = new();

    public List<AttributeCodeTable> AttributeCodeTableList
    {
        get => m_attributeCodeTableList;
        set => m_attributeCodeTableList = value;
    }

    public override int Count => AttributeCodeTableList.Count;
    #endregion

    #region Database
    /// <summary>
    /// Fill dataset from database
    /// </summary>
    public void Fill(System.Data.IDbCommand a_tableListCmd,
                     System.Data.IDbCommand a_attributeExternalIdCmd,
                     System.Data.IDbCommand a_attributeCodes,
                     System.Data.IDbCommand a_assignedResourceCmd)
    {
        LookupAttributeCodeTableDataSet ds = new();
        FillTable(ds.TableList, a_tableListCmd);
        FillTable(ds.AssignedResources, a_assignedResourceCmd);
        FillTable(ds.AttributeExternalId, a_attributeExternalIdCmd);
        FillTable(ds.AttributeCodes, a_attributeCodes);
        FillFromDataSet(ds);
    }

    /// <summary>
    /// Fill the transmission with data from the DataSet.
    /// </summary>
    /// <param name="a_ds"></param>
    public void FillFromDataSet(LookupAttributeCodeTableDataSet a_ds)
    {
        for (int i = 0; i < a_ds.TableList.Count; i++)
        {
            LookupAttributeCodeTableDataSet.TableListRow tableListRow = a_ds.TableList[i];

            AttributeCodeTable newTable = new();
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

            // Attribute Names
            LookupAttributeCodeTableDataSet.AttributeExternalIdRow[] attributeNames = tableListRow.GetAttributeExternalIdRows();
            for (int j = 0; j < attributeNames.Count(); j++)
            {
                LookupAttributeCodeTableDataSet.AttributeCodesRow[] attributeCodes = attributeNames[j].GetAttributeCodesRows();
                for (int l = 0; l < attributeCodes.Count(); l++)
                {
                    //SetupCost can be null. If so, use 0
                    decimal setupCost = 0m;
                    if (!attributeCodes[l].IsCostNull())
                    {
                        setupCost = attributeCodes[l].Cost;
                    }

                    int cleanoutGrade = 0;
                    if (!attributeCodes[l].IsCleanoutGradeNull())
                    {
                        cleanoutGrade = attributeCodes[l].CleanoutGrade;
                    }

                    newTable.Add(new AttributeCodeTable.AttributeCodeTableRow(attributeCodes[l].AttributeExternalId, attributeCodes[l].PreviousOpAttributeCode, attributeCodes[l].NextOpAttributeCode, TimeSpan.FromHours(attributeCodes[l].DurationHours), setupCost, cleanoutGrade));
                }
            }

            LookupAttributeCodeTableDataSet.AssignedResourcesRow[] resourceList = tableListRow.GetAssignedResourcesRows();
            for (int k = 0; k < resourceList.Count(); k++)
            {
                newTable.AssignedResourcesExternalId.Add(new SchedulerDefinitions.ResourceKeyExternal(resourceList[k].PlantExternalId, resourceList[k].DepartmentExternalId, resourceList[k].ResourceExternalId));
            }

            AttributeCodeTableList.Add(newTable);
        }
    }
    #endregion
}