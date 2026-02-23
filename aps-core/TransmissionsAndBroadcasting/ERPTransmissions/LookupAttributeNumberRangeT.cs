using PT.APSCommon;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

public class LookupAttributeNumberRangeT : ERPMaintenanceTransmission<LookupAttributeNumberRangeT.LookupAttributeNumberRangeTable>, IPTSerializable
{
    #region IPTSerializable Members
    public new const int UNIQUE_ID = 624;

    public LookupAttributeNumberRangeT(IReader reader)
        : base(reader)
    {
        int count;
        reader.Read(out count);
        for (int i = 0; i < count; i++)
        {
            LookupAttributeNumberRangeTable tbl = new (reader);
            tableList.Add(tbl);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            tableList[i].Serialize(writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public LookupAttributeNumberRangeT() { }

    public List<LookupAttributeNumberRangeTable> tableList = new ();

    public override int Count => tableList.Count;

    #region Database Loading
    public void Fill(System.Data.IDbCommand tableListCmd,
                     System.Data.IDbCommand attributeRangesCmd,
                     System.Data.IDbCommand fromRangesCmd,
                     System.Data.IDbCommand toRangesCmd,
                     System.Data.IDbCommand assignedResourcesCmd)
    {
        LookupAttributeNumberRangeDataSet ds = new ();
        FillTable(ds.TableList, tableListCmd);
        FillTable(ds.AttributeRanges, attributeRangesCmd);
        FillTable(ds.From_Ranges, fromRangesCmd);
        FillTable(ds.To_Ranges, toRangesCmd);
        FillTable(ds.AssignedResources, assignedResourcesCmd);
        Fill(ds);
    }

    /// <summary>
    /// Fill the transmission with data from the DataSet.
    /// </summary>
    /// <param name="ds"></param>
    public void Fill(LookupAttributeNumberRangeDataSet ds)
    {
        for (int i = 0; i < ds.TableList.Count; i++)
        {
            tableList.Add(new LookupAttributeNumberRangeTable(ds.TableList[i]));
        }
    }
    #endregion Database Loading

    #region LookupAttributeNumberRangeTable
    /// <summary>
    /// This table specifies the setup time and cost incurred when switching between Operations with different Attribute Number values.
    /// During scheduling, the systems looks up these values by referring to the Operation's Attributes by Name.
    /// Each Attribute can have specified From and To ranges defined in this table.
    /// Each table can be linked to one or more Resources.  The schedules for these Resources will be adjusted accordingly.
    /// </summary>
    public class LookupAttributeNumberRangeTable : IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 623;

        public LookupAttributeNumberRangeTable(IReader reader)
        {
            reader.Read(out Name);
            reader.Read(out Description);
            Id = new BaseId(reader);

            int listCount;
            reader.Read(out listCount);
            for (int i = 0; i < listCount; i++)
            {
                SetupRangeAttributeUpdates.Add(new SetupRangeAttributeUpdate(reader));
            }

            reader.Read(out listCount);
            for (int i = 0; i < listCount; i++)
            {
                AssignedResources.Add(new ResourceKeyExternal(reader));
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(Name);
            writer.Write(Description);
            Id.Serialize(writer);

            writer.Write(SetupRangeAttributeUpdates.Count);
            for (int i = 0; i < SetupRangeAttributeUpdates.Count; i++)
            {
                SetupRangeAttributeUpdates[i].Serialize(writer);
            }

            writer.Write(AssignedResources.Count);
            for (int i = 0; i < AssignedResources.Count; i++)
            {
                AssignedResources[i].Serialize(writer);
            }
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public BaseId Id;
        public string Name;
        public string Description;
        public List<ResourceKeyExternal> AssignedResources = new ();

        public List<SetupRangeAttributeUpdate> SetupRangeAttributeUpdates = new ();

        public LookupAttributeNumberRangeTable(LookupAttributeNumberRangeDataSet.TableListRow tableListRow)
        {
            Id = new BaseId(tableListRow.TableId);
            Name = tableListRow.Name;
            if (!tableListRow.IsDescriptionNull())
            {
                Description = tableListRow.Description;
            }

            //Add the attributes
            for (int sraI = 0; sraI < tableListRow.GetAttributeRangesRows().Length; sraI++)
            {
                LookupAttributeNumberRangeDataSet.AttributeRangesRow arRow = (LookupAttributeNumberRangeDataSet.AttributeRangesRow)tableListRow.GetAttributeRangesRows().GetValue(sraI);
                string attDesc = "";
                if (!arRow.IsDescriptionNull())
                {
                    attDesc = arRow.Description;
                }

                SetupRangeAttributeUpdate sra = new (arRow.AttributeExternalId, attDesc, arRow.EligibilityConstraint);
                SetupRangeAttributeUpdates.Add(sra);

                //Add From Ranges to sra
                for (int fromIdx = 0; fromIdx < arRow.GetFrom_RangesRows().Length; fromIdx++)
                {
                    LookupAttributeNumberRangeDataSet.From_RangesRow fromRow = (LookupAttributeNumberRangeDataSet.From_RangesRow)arRow.GetFrom_RangesRows().GetValue(fromIdx);
                    SetupRangeUpdate tFrom = new (fromRow.FromRangeStart, fromRow.FromRangeEnd);
                    sra.fromRanges.Add(tFrom);

                    //Add To Ranges to From range
                    for (int toIdx = 0; toIdx < fromRow.GetTo_RangesRows().Length; toIdx++)
                    {
                        LookupAttributeNumberRangeDataSet.To_RangesRow toRow = (LookupAttributeNumberRangeDataSet.To_RangesRow)fromRow.GetTo_RangesRows().GetValue(toIdx);
                        double setupMinutes = 0;
                        if (!toRow.IsSetupMinutesNull())
                        {
                            setupMinutes = toRow.SetupMinutes;
                        }

                        decimal setupCost = 0;
                        if (!toRow.IsSetupCostNull())
                        {
                            setupCost = toRow.SetupCost;
                        }

                        SetupToRange tToRange = new (toRow.ToRangeStart, toRow.ToRangeEnd, setupCost, (long)(TimeSpan.TicksPerMinute * setupMinutes));
                        tFrom.toRanges.Add(tToRange);
                    }
                }
            }

            //Add the assigned Resources
            for (int resI = 0; resI < tableListRow.GetAssignedResourcesRows().Length; resI++)
            {
                LookupAttributeNumberRangeDataSet.AssignedResourcesRow resRow = (LookupAttributeNumberRangeDataSet.AssignedResourcesRow)tableListRow.GetAssignedResourcesRows().GetValue(resI);
                AssignedResources.Add(new ResourceKeyExternal(resRow.PlantExternalId, resRow.DepartmentExternalId, resRow.ResourceExternalId));
            }
        }
    }
    #endregion
}