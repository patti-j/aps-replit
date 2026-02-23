using PT.Common.Exceptions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

public class CapacityIntervalT : ERPMaintenanceTransmission<CapacityIntervalT.CapacityIntervalDef>, IPTSerializable
{
    public new const int UNIQUE_ID = 216;

    #region PT Serialization
    public CapacityIntervalT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 96)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                CapacityIntervalDef node = new (reader);
                Add(node);
            }
        }
        else if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                //Changed to CapacityIntervalDefs in version 96
                // just read the old objects in but can't do anthing with them since no resource info anymore.
                CapacityInterval ci = new (reader);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public CapacityIntervalT() { }

    /// <summary>
    /// Whether to remove capacity interval from resources that are not specified in the transmission
    /// </summary>
    private bool m_autoDeleteResourceAssociations = true;

    public bool AutoDeleteResourceAssociations
    {
        get => m_autoDeleteResourceAssociations;
        set => m_autoDeleteResourceAssociations = value;
    }

    public class CapacityIntervalDef : PTObjectBase, IPTSerializable
    {
        public new const int UNIQUE_ID = 539;

        #region PT Serialization
        public CapacityIntervalDef(IReader reader)
            : base(reader)
        {
            if (reader.VersionNumber >= 700)
            {
                m_ci = new CapacityInterval(reader);
                int c;
                reader.Read(out c);
                for (int i = 0; i < c; i++)
                {
                    reader.Read(out string plantExternalId);
                    reader.Read(out string deptExternalId);
                    reader.Read(out string resExternalId);
                    m_resourceExternalIds.Add(new Tuple<string, string, string>(plantExternalId, deptExternalId, resExternalId));
                }
            }
        }

        public override void Serialize(IWriter a_writer)
        {
            base.Serialize(a_writer);

            m_ci.Serialize(a_writer);
            a_writer.Write(m_resourceExternalIds.Count);
            foreach (Tuple<string, string, string> resExternalId in m_resourceExternalIds)
            {
                a_writer.Write(resExternalId.Item1);
                a_writer.Write(resExternalId.Item2);
                a_writer.Write(resExternalId.Item3);
            }
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        private readonly List<Tuple<string, string, string>> m_resourceExternalIds = new ();

        /// <summary>
        /// Contains the list of Resource externalIds that the CI is assigned to.
        /// Tuple Item1: PlantExternalId, Item2: DepartmentExternalId, Item3: ResourceExternalId
        /// </summary>
        public List<Tuple<string, string, string>> ResourceExternalIds => m_resourceExternalIds;

        public CapacityInterval m_ci;

        public CapacityIntervalDef(CapacityIntervalTDataSet.CapacityIntervalRow ciRow)
            : base(ciRow.ExternalId, ciRow.Name)
        {
            m_ci = new CapacityInterval(ciRow.ExternalId, ciRow.Name);
            if (!ciRow.IsDescriptionNull())
            {
                m_ci.Description = ciRow.Description;
            }

            if (!ciRow.IsNotesNull())
            {
                m_ci.Notes = ciRow.Notes;
            }

            m_ci.StartDateTime = ciRow.StartDateTime.ToServerTime().RemoveSeconds();
            m_ci.EndDateTime = ciRow.EndDateTime.ToServerTime().RemoveSeconds();
            if (!ciRow.IsNbrOfPeopleNull())
            {
                m_ci.NbrOfPeople = (decimal)ciRow.NbrOfPeople;
            }

            if (!ciRow.IsIntervalPresetNull())
            {
                switch (ciRow.IntervalPreset)
                {
                    case "Online":
                        m_ci.IntervalType = CapacityIntervalDefs.capacityIntervalTypes.Online;
                        m_ci.Color = ColorUtils.ColorCodes.CapacityIntervalOnlineColor;
                        m_ci.UsedForRun = true;
                        m_ci.UsedForSetup = true;
                        m_ci.UsedForPostProcessing = true;
                        m_ci.UsedForStorage = true;
                        m_ci.UsedForClean = true;
                        m_ci.CanStartActivity = true;
                        break;
                    case "Overtime":
                        m_ci.IntervalType = CapacityIntervalDefs.capacityIntervalTypes.Online;
                        m_ci.Color = ColorUtils.ColorCodes.CapacityIntervalOvertimeColor;
                        m_ci.UsedForRun = true;
                        m_ci.UsedForSetup = true;
                        m_ci.UsedForPostProcessing = true;
                        m_ci.UsedForStorage = true;
                        m_ci.UsedForClean = true;
                        m_ci.Overtime = true;
                        m_ci.CanStartActivity = true;
                        break;
                    case "PotentialOvertime":
                        m_ci.IntervalType = CapacityIntervalDefs.capacityIntervalTypes.Online;
                        m_ci.Color = ColorUtils.ColorCodes.CapacityIntervalPotentialOvertimeColor;
                        m_ci.UsedForRun = true;
                        m_ci.UsedForSetup = true;
                        m_ci.UsedForPostProcessing = true;
                        m_ci.UsedForStorage = true;
                        m_ci.UsedForClean = true;
                        m_ci.Overtime = true;
                        m_ci.UseOnlyWhenLate = true;
                        m_ci.CanStartActivity = true;
                        break;
                    case "Cleanout":
                        m_ci.IntervalType = CapacityIntervalDefs.capacityIntervalTypes.Online;
                        m_ci.Color = ColorUtils.ColorCodes.CapacityIntervalCleanoutColor;
                        m_ci.UsedForClean = true;
                        break;
                    case "Offline":
                        m_ci.IntervalType = CapacityIntervalDefs.capacityIntervalTypes.Offline;
                        m_ci.Color = ColorUtils.ColorCodes.CapacityIntervalOfflineColor;
                        break;
                    case "Holiday":
                        m_ci.IntervalType = CapacityIntervalDefs.capacityIntervalTypes.Offline;
                        m_ci.Color = ColorUtils.ColorCodes.CapacityIntervalHolidayColor;
                        m_ci.PreventOperationsFromSpanning = true;
                        break;
                    case "Maintenance":
                        m_ci.IntervalType = CapacityIntervalDefs.capacityIntervalTypes.Offline;
                        m_ci.Color = ColorUtils.ColorCodes.CapacityIntervalMaintenanceColor;
                        m_ci.PreventOperationsFromSpanning = true;
                        m_ci.CleanOutSetups = true;
                        break;
                    default:
                        // This will catch empty strings 
                        break;
                }
            }

            if (!ciRow.IsIntervalTypeNull())
            {
                try
                {
                    m_ci.IntervalType = (CapacityIntervalDefs.capacityIntervalTypes)Enum.Parse(typeof(CapacityIntervalDefs.capacityIntervalTypes), ciRow.IntervalType);
                }
                catch (Exception err)
                {
                    throw new APSCommon.PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            ciRow.IntervalType, "CapacityInterval", "IntervalType",
                            string.Join(", ", Enum.GetNames(typeof(CapacityIntervalDefs.capacityIntervalTypes)))
                        });
                }
            }

            if (!ciRow.IsColorNull())
            {
                m_ci.Color = ColorUtils.GetColorFromHexString(ciRow.Color);
            }

            if (!ciRow.IsResetAttributeChangeoversNull())
            {
                m_ci.CleanOutSetups = ciRow.ResetAttributeChangeovers;
            }

            if (!ciRow.IsPreventOperationsFromSpanningNull())
            {
                m_ci.PreventOperationsFromSpanning = ciRow.PreventOperationsFromSpanning;
            }

            if (!ciRow.IsCanStartActivityNull())
            {
                m_ci.CanStartActivity = ciRow.CanStartActivity;
            }

            if (!ciRow.IsUsedForSetupNull())
            {
                m_ci.UsedForSetup = ciRow.UsedForSetup;
            }

            if (!ciRow.IsUsedForRunNull())
            {
                m_ci.UsedForRun = ciRow.UsedForRun;
            }

            if (!ciRow.IsUsedForPostProcessingNull())
            {
                m_ci.UsedForPostProcessing = ciRow.UsedForPostProcessing;
            }

            if (!ciRow.IsUsedForCleanNull())
            {
                m_ci.UsedForClean = ciRow.UsedForClean;
            }

            if (!ciRow.IsUsedForStoragePostProcessingNull())
            {
                m_ci.UsedForStorage = ciRow.UsedForStoragePostProcessing;
            }

            if (!ciRow.IsCapacityCodeNull())
            {
                m_ci.CapacityCode = ciRow.CapacityCode;
            }

            //Link to all Resources that should have this capacity interval
            CapacityIntervalTDataSet.ResourcesRow[] resRows = ciRow.GetResourcesRows();
            for (int i = 0; i < resRows.Length; i++)
            {
                CapacityIntervalTDataSet.ResourcesRow resRow = (CapacityIntervalTDataSet.ResourcesRow)resRows.GetValue(i);
                AddResourceExternalId(resRow.PlantExternalId, resRow.DepartmentExternalId, resRow.ResourceExternalId);
            }
        }

        private void AddResourceExternalId(string a_plantExternalId, string a_deptExternalId, string a_resExternalId)
        {
            Tuple<string, string, string> resExternalId = new (a_plantExternalId, a_deptExternalId, a_resExternalId);
            if (m_resourceExternalIds.Contains(resExternalId))
            {
                throw new APSCommon.PTValidationException("2977", new object[] { ExternalId, a_plantExternalId, a_deptExternalId, a_resExternalId });
            }

            m_resourceExternalIds.Add(resExternalId);
        }

        public CapacityIntervalDef(string a_externalId, string a_name, CapacityInterval a_ci)
            : base(a_externalId, a_name)
        {
            m_ci = a_ci;
        }

        public CapacityIntervalDef()
            : base("", "") { }

        public override void Validate()
        {
            base.Validate();

            if (m_ci.StartDateTime >= m_ci.EndDateTime)
            {
                throw new APSCommon.PTValidationException("2864", new object[] { m_ci.ExternalId, m_ci.StartDateTime, m_ci.EndDateTime });
            }

            if (m_ci.IntervalType == CapacityIntervalDefs.capacityIntervalTypes.Offline)
            {
                if (m_ci.Overtime)
                {
                    throw new APSCommon.PTValidationException("3069", new object[] { m_ci.ExternalId, "Overtime" });
                }
                if (m_ci.CanStartActivity)
                {
                    throw new APSCommon.PTValidationException("3069", new object[] { m_ci.ExternalId, "CanStartActivity" });
                }
                if (m_ci.UseOnlyWhenLate)
                {
                    throw new APSCommon.PTValidationException("3069", new object[] { m_ci.ExternalId, "UseOnlyWhenLate" });
                }
                if (m_ci.UsedForSetup)
                {
                    throw new APSCommon.PTValidationException("3069", new object[] { m_ci.ExternalId, "UsedForSetup" });
                }
                if (m_ci.UsedForRun)
                {
                    throw new APSCommon.PTValidationException("3069", new object[] { m_ci.ExternalId, "UsedForRun" });
                }
                if (m_ci.UsedForPostProcessing)
                {
                    throw new APSCommon.PTValidationException("3069", new object[] { m_ci.ExternalId, "UsedForPostProcessing" });
                }
                if (m_ci.UsedForStorage)
                {
                    throw new APSCommon.PTValidationException("3069", new object[] { m_ci.ExternalId, "UsedForStoragePostProcessing" });
                }
                if (m_ci.UsedForClean)
                {
                    throw new APSCommon.PTValidationException("3069", new object[] { m_ci.ExternalId, "UsedForClean" });
                }
            }
        }
    }

    public new CapacityIntervalDef this[int i] => Nodes[i];

    #region Database Loading
    public void Fill(System.Data.IDbCommand capacityIntervalsCmd, System.Data.IDbCommand resoucesCmd)
    {
        CapacityIntervalTDataSet ds = new ();
        try
        {
            FillTable(ds.CapacityInterval, capacityIntervalsCmd);
        }
        catch (Exception e)
        {
            throw new PTException("4048", e, new object[] { e.Message });
        }

        try
        {
            FillTable(ds.Resources, resoucesCmd);
        }
        catch (Exception e)
        {
            throw new PTException("4049", e, new object[] { e.Message });
        }

        Fill(ds);
    }

    /// <summary>
    /// Fill the transmission with data from the DataSet.
    /// </summary>
    /// <param name="ds"></param>
    public void Fill(CapacityIntervalTDataSet ds)
    {
        for (int i = 0; i < ds.CapacityInterval.Count; i++)
        {
            CapacityIntervalDef ciDef = new (ds.CapacityInterval[i]);
            ciDef.Validate();
            Add(ciDef);
        }
    }
    #endregion Database Loading
}