using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

/// <summary>
/// For creating an BaseResource via ERP transmission.
/// </summary>
public class BaseResource : PTObjectBase, IPTSerializable
{
    public new const int UNIQUE_ID = 209;

    #region IPTSerializable Members
    public BaseResource(IReader reader)
        : base(reader)
    {
        bool obsoleteBool;
        bool obsoleteActive;

        #region 12000
        if (reader.VersionNumber >= 12401)
        {
            m_bools = new BoolVector32(reader);

            reader.Read(out m_workcenter);
            reader.Read(out workcenterExternalId);
            reader.Read(out int val);
            m_resourceType = (BaseResourceDefs.resourceTypes)val;
            reader.Read(out m_numberOfResources);
            reader.Read(out m_ganttRowHeightFactor);
            reader.Read(out m_imageFileName);
            reader.Read(out m_maxCumulativeQty);

            reader.Read(out int allowedHelperCount);
            for (int i = 0; i < allowedHelperCount; i++)
            {
                m_allowedHelpers.Add(new AllowedHelper(reader));
            }

            reader.Read(out m_normalSequencingPlanExternalId);
            reader.Read(out m_experimentalSequencingPlanOneExternalId);
            reader.Read(out m_experimentalSequencingPlanTwoExternalId);
            reader.Read(out m_experimentalSequencingPlanThreeExternalId);
            reader.Read(out m_experimentalSequencingPlanFourExternalId);
            reader.Read(out m_cellExternalId);
        }
        else if (reader.VersionNumber >= 12400)
        {
            m_bools = new BoolVector32(reader);

            reader.Read(out m_workcenter);
            reader.Read(out workcenterExternalId);
            reader.Read(out int val);
            m_resourceType = (BaseResourceDefs.resourceTypes)val;
            reader.Read(out m_numberOfResources);
            reader.Read(out m_ganttRowHeightFactor);
            reader.Read(out m_imageFileName);
            reader.Read(out m_maxCumulativeQty);

            reader.Read(out int connectorCount);
            for (int i = 0; i < connectorCount; i++)
            {
                new Connector(reader); //Deprecated
            }

            reader.Read(out int allowedHelperCount);
            for (int i = 0; i < allowedHelperCount; i++)
            {
                m_allowedHelpers.Add(new AllowedHelper(reader));
            }

            reader.Read(out m_normalSequencingPlanExternalId);
            reader.Read(out m_experimentalSequencingPlanOneExternalId);
            reader.Read(out m_experimentalSequencingPlanTwoExternalId);
            reader.Read(out m_experimentalSequencingPlanThreeExternalId);
            reader.Read(out m_experimentalSequencingPlanFourExternalId);
            reader.Read(out m_cellExternalId);
        }
        else if (reader.VersionNumber >= 12325)
        {
            m_bools = new BoolVector32(reader);

            reader.Read(out m_workcenter);
            reader.Read(out workcenterExternalId);
            reader.Read(out int val);
            m_resourceType = (BaseResourceDefs.resourceTypes)val;
            reader.Read(out m_numberOfResources);
            reader.Read(out m_ganttRowHeightFactor);
            reader.Read(out m_imageFileName);
            reader.Read(out m_maxCumulativeQty);

            reader.Read(out int allowedHelperCount);
            for (int i = 0; i < allowedHelperCount; i++)
            {
                m_allowedHelpers.Add(new AllowedHelper(reader));
            }

            reader.Read(out m_normalSequencingPlanExternalId);
            reader.Read(out m_experimentalSequencingPlanOneExternalId);
            reader.Read(out m_experimentalSequencingPlanTwoExternalId);
            reader.Read(out m_experimentalSequencingPlanThreeExternalId);
            reader.Read(out m_experimentalSequencingPlanFourExternalId);
            reader.Read(out m_cellExternalId);
        }
        else if (reader.VersionNumber >= 12208)
        {
            m_bools = new BoolVector32(reader);

            reader.Read(out m_workcenter);
            reader.Read(out workcenterExternalId);
            reader.Read(out int val);
            m_resourceType = (BaseResourceDefs.resourceTypes)val;
            reader.Read(out m_numberOfResources);
            reader.Read(out m_ganttRowHeightFactor);
            reader.Read(out m_imageFileName);
            reader.Read(out m_maxCumulativeQty);

            reader.Read(out int connectorCount);
            for (int i = 0; i < connectorCount; i++)
            {
                new Connector(reader); //Deprecated
            }

            reader.Read(out int allowedHelperCount);
            for (int i = 0; i < allowedHelperCount; i++)
            {
                m_allowedHelpers.Add(new AllowedHelper(reader));
            }

            reader.Read(out m_normalSequencingPlanExternalId);
            reader.Read(out m_experimentalSequencingPlanOneExternalId);
            reader.Read(out m_experimentalSequencingPlanTwoExternalId);
            reader.Read(out m_experimentalSequencingPlanThreeExternalId);
            reader.Read(out m_experimentalSequencingPlanFourExternalId);
            reader.Read(out m_cellExternalId);
        }
        else if (reader.VersionNumber >= 12112)
        {
            m_bools = new BoolVector32(reader);

            reader.Read(out m_workcenter);
            reader.Read(out workcenterExternalId);
            reader.Read(out int val);
            m_resourceType = (BaseResourceDefs.resourceTypes)val;
            reader.Read(out m_numberOfResources);
            reader.Read(out m_ganttRowHeightFactor);
            reader.Read(out m_imageFileName);
            reader.Read(out m_maxCumulativeQty);

            reader.Read(out int connectorCount);
            for (int i = 0; i < connectorCount; i++)
            {
                new Connector(reader); //Deprecated
            }

            reader.Read(out int allowedHelperCount);
            for (int i = 0; i < allowedHelperCount; i++)
            {
                m_allowedHelpers.Add(new AllowedHelper(reader));
            }

            reader.Read(out m_normalSequencingPlanExternalId);
            reader.Read(out m_experimentalSequencingPlanOneExternalId);
            m_experimentalSequencingPlanTwoExternalId = m_experimentalSequencingPlanOneExternalId;
            m_experimentalSequencingPlanThreeExternalId = m_experimentalSequencingPlanOneExternalId;
            m_experimentalSequencingPlanFourExternalId = m_experimentalSequencingPlanOneExternalId;
            reader.Read(out m_cellExternalId);
        }
        else if (reader.VersionNumber >= 12000)
        {
            m_bools = new BoolVector32(reader);

            reader.Read(out m_workcenter);
            reader.Read(out workcenterExternalId);
            reader.Read(out int val);
            m_resourceType = (BaseResourceDefs.resourceTypes)val;
            reader.Read(out int discardUseToBeSort);
            reader.Read(out m_numberOfResources);
            reader.Read(out m_ganttRowHeightFactor);
            reader.Read(out m_imageFileName);
            reader.Read(out m_maxCumulativeQty);

            reader.Read(out int connectorCount);
            for (int i = 0; i < connectorCount; i++)
            {
                new Connector(reader); //Deprecated
            }

            reader.Read(out int allowedHelperCount);
            for (int i = 0; i < allowedHelperCount; i++)
            {
                m_allowedHelpers.Add(new AllowedHelper(reader));
            }

            reader.Read(out m_normalSequencingPlanExternalId);
            reader.Read(out m_experimentalSequencingPlanOneExternalId);
            m_experimentalSequencingPlanTwoExternalId = m_experimentalSequencingPlanOneExternalId;
            m_experimentalSequencingPlanThreeExternalId = m_experimentalSequencingPlanOneExternalId;
            m_experimentalSequencingPlanFourExternalId = m_experimentalSequencingPlanOneExternalId;
            reader.Read(out m_cellExternalId);
        }
        #endregion

        #region 360
        else if (reader.VersionNumber >= 360)
        {
            reader.Read(out obsoleteActive);
            reader.Read(out m_workcenter);
            reader.Read(out workcenterExternalId);
            int val;
            reader.Read(out val);
            m_resourceType = (BaseResourceDefs.resourceTypes)val;
            reader.Read(out int discardUseToBeSort);
            reader.Read(out m_numberOfResources);
            reader.Read(out m_ganttRowHeightFactor);
            reader.Read(out m_imageFileName);
            reader.Read(out m_maxCumulativeQty);

            m_bools = new BoolVector32(reader);

            m_bools[c_activeIdx] = obsoleteActive;

            //Was set flags
            reader.Read(out obsoleteBool);
            m_bools[c_activeSetIdx] = obsoleteBool;
            reader.Read(out obsoleteBool);
            m_bools[c_workcenterSetIdx] = obsoleteBool;
            reader.Read(out obsoleteBool);
            m_bools[c_workcenterExternalIdSetIdx] = obsoleteBool;
            reader.Read(out obsoleteBool);
            m_bools[c_resourceTypeSetIdx] = obsoleteBool;
            reader.Read(out obsoleteBool);
            //End was set flags

            int connectorCount;
            reader.Read(out connectorCount);
            for (int i = 0; i < connectorCount; i++)
            {
                new Connector(reader); //Deprecated
            }

            int allowedHelperCount;
            reader.Read(out allowedHelperCount);
            for (int i = 0; i < allowedHelperCount; i++)
            {
                m_allowedHelpers.Add(new AllowedHelper(reader));
            }

            reader.Read(out m_normalSequencingPlanExternalId);
            reader.Read(out m_experimentalSequencingPlanOneExternalId);
            m_experimentalSequencingPlanTwoExternalId = m_experimentalSequencingPlanOneExternalId;
            m_experimentalSequencingPlanThreeExternalId = m_experimentalSequencingPlanOneExternalId;
            m_experimentalSequencingPlanFourExternalId = m_experimentalSequencingPlanOneExternalId;
            reader.Read(out m_cellExternalId);
        }
        #endregion
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_bools.Serialize(writer);

        writer.Write(m_workcenter);
        writer.Write(workcenterExternalId);
        writer.Write((int)m_resourceType);
        writer.Write(m_numberOfResources);
        writer.Write(m_ganttRowHeightFactor);
        writer.Write(m_imageFileName);
        writer.Write(m_maxCumulativeQty);
        writer.Write(m_allowedHelpers.Count);
        for (int i = 0; i < m_allowedHelpers.Count; i++)
        {
            m_allowedHelpers[i].Serialize(writer);
        }

        writer.Write(m_normalSequencingPlanExternalId);
        writer.Write(m_experimentalSequencingPlanOneExternalId);
        writer.Write(m_experimentalSequencingPlanTwoExternalId);
        writer.Write(m_experimentalSequencingPlanThreeExternalId);
        writer.Write(m_experimentalSequencingPlanFourExternalId);
        writer.Write(m_cellExternalId);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseResource() { }

    public BaseResource(string externalId, string name, string description, string notes, string userFields)
        : base(externalId, name, description, notes, userFields) { }

    #region Bools
    private BoolVector32 m_bools;
    private const short DisallowDragAndDropsIdx = 0;
    private const short DisallowDragAndDropsSetIdx = 1;
    private const short GanttRowHeightFactorSetIdx = 2;
    private const short ImageFileNameSetIdx = 3;
    private const short MaxCumulativeQtySetIdx = 4;

    private const short ExcludeFromGantsIdx = 5;

    private const short ExcludeFromGanttsIsSetIdx = 6;

    //const short ExcludeFromCapacityPlanIdx = 7; //no longer used, just here as a placeholder for the index
    //const short ExcludeFromCapacityPlanIsSetIdx = 8;
    private const short ManualAssignmentOnlyIdx = 9;

    private const short ManualAssignmentOnlyIsSetIdx = 10;

    //const short ExcludeFromReportsIdx = 11;
    //const short ExcludeFromReportsSetIdx = 12;
    private const short c_normalSequencingPlanExternalIdIsSetIdx = 13;
    private const short c_experimentalDispatcherOneExternalIdIsSetIdx = 14;
    private const short CellSetIdx = 15;
    private const short ExcludeExceptFromDepartmentIdx = 16;
    private const short ExcludeExceptFromDepartmentSetIdx = 17;
    private const short c_activeSetIdx = 18;
    private const short c_workcenterSetIdx = 19;

    private const short c_resourceTypeSetIdx = 20;

    //const short c_sortSetIdx = 21; // Sort was removed from code, this comment is just a placeholder here for when we need to reuse the index
    private const short c_activeIdx = 22;
    private const short c_workcenterExternalIdSetIdx = 23;
    private const short c_experimentalDispatcherTwoExternalIdIsSetIdx = 24;
    private const short c_experimentalDispatcherThreeExternalIdIsSetIdx = 25;
    private const short c_experimentalDispatcherFourExternalIdIsSetIdx = 26;
    #endregion Bools

    #region Shared Properties
    /// <summary>
    /// If not Active, then no new Activities can be scheduled to use this Resource.  This does not affect Activiites already scheduled to use the Resource.
    /// </summary>
    public bool Active
    {
        get => m_bools[c_activeIdx];
        set
        {
            m_bools[c_activeIdx] = value;
            m_bools[c_activeSetIdx] = true;
        }
    }

    public bool ActiveSet => m_bools[c_activeSetIdx];

    private string m_workcenter = "";

    /// <summary>
    /// The name of the workcenter in the ERP system.  For information only.
    /// </summary>
    public string Workcenter
    {
        get => m_workcenter;
        set
        {
            m_workcenter = value;
            m_bools[c_workcenterSetIdx] = true;
        }
    }

    public bool WorkcenterSet => m_bools[c_workcenterSetIdx];

    private string workcenterExternalId = "";

    /// <summary>
    /// The ID of the workcenter in the ERP system.  For information only.
    /// </summary>
    public string WorkcenterExternalId
    {
        get => workcenterExternalId;
        set
        {
            workcenterExternalId = value;
            m_bools[c_workcenterExternalIdSetIdx] = true;
        }
    }

    public bool WorkcenterExternalIdSet => m_bools[c_workcenterExternalIdSetIdx];

    private BaseResourceDefs.resourceTypes m_resourceType = BaseResourceDefs.resourceTypes.Machine;

    /// <summary>
    /// The type of physical entity represented by the Resource.  Used for custom reports and other display groupings.
    /// </summary>
    public BaseResourceDefs.resourceTypes ResourceType
    {
        get => m_resourceType;
        set
        {
            m_resourceType = value;
            m_bools[c_resourceTypeSetIdx] = true;
        }
    }

    public bool ResourceTypeSet => m_bools[c_resourceTypeSetIdx];

    /// <summary>
    /// If true then drag and drop of Activities on the Resource are not allowed.
    /// </summary>
    public bool DisallowDragAndDrops
    {
        get => m_bools[DisallowDragAndDropsIdx];

        set
        {
            m_bools[DisallowDragAndDropsIdx] = value;
            m_bools[DisallowDragAndDropsSetIdx] = true;
        }
    }

    public bool DisallowDragAndDropsSet => m_bools[DisallowDragAndDropsSetIdx];

    private int m_ganttRowHeightFactor = 10;

    /// <summary>
    /// Controls how tall the row is relative to other rows.  Valid ranges is from 1 (shortest) to 10 (tallest).
    /// </summary>
    public int GanttRowHeightFactor
    {
        get => m_ganttRowHeightFactor;
        set
        {
            if (value < 1 || value > 10)
            {
                throw new APSCommon.PTValidationException("2043");
            }

            m_ganttRowHeightFactor = value;
            m_bools[GanttRowHeightFactorSetIdx] = true;
        }
    }

    public bool GanttRowHeightFactorSet => m_bools[GanttRowHeightFactorSetIdx];

    private string m_imageFileName;

    /// <summary>
    /// Name (with suffix but not path) of which image to use in the Gantt, etc to represent this Resource.
    /// </summary>
    public string ImageFileName
    {
        get => m_imageFileName;
        set
        {
            m_imageFileName = value;
            m_bools[ImageFileNameSetIdx] = true;
        }
    }

    public bool ImageFileNameSet => m_bools[ImageFileNameSetIdx];

    private decimal m_maxCumulativeQty;

    /// <summary>
    /// Often used to model "tank capacity" or "subcontractor capacity", this is the maximum quantity of simultaneous Activities allowed on the Resource.
    /// Violations create Block Flags. A Block is Flagged if the total Start Quantity of Activities beginning but not yet ending on the Resource exceeds this quantity.
    /// Set this value to zero to turn it off.
    /// This is not a constraint -- it is a Flag only.
    /// </summary>
    public decimal MaxCumulativeQty
    {
        get => m_maxCumulativeQty;
        set
        {
            m_maxCumulativeQty = value;
            m_bools[MaxCumulativeQtySetIdx] = true;
        }
    }

    public bool MaxCumulativeQtySet => m_bools[MaxCumulativeQtySetIdx];

    /// <summary>
    /// Whether the Resource should be excluded from Resource Gantts.
    /// This is often set to true for inactive resources, etc.
    /// </summary>
    public bool ExcludeFromGantts
    {
        get => m_bools[ExcludeFromGantsIdx];

        set
        {
            m_bools[ExcludeFromGantsIdx] = value;
            m_bools[ExcludeFromGanttsIsSetIdx] = true;
        }
    }

    public bool ExcludeFromGanttsSet => m_bools[ExcludeFromGanttsIsSetIdx];

    /// <summary>
    /// Whether the Resource only be assigned manually and not during Optimizations.
    /// </summary>
    public bool ManualAssignmentOnly
    {
        get => m_bools[ManualAssignmentOnlyIdx];

        set
        {
            m_bools[ManualAssignmentOnlyIdx] = value;
            m_bools[ManualAssignmentOnlyIsSetIdx] = true;
        }
    }

    public bool ManualAssignmentOnlySet => m_bools[ManualAssignmentOnlyIsSetIdx];

    private string m_normalSequencingPlanExternalId;

    public string NormalSequencingPlanExternalId
    {
        get => m_normalSequencingPlanExternalId;
        set
        {
            m_normalSequencingPlanExternalId = value;
            m_bools[c_normalSequencingPlanExternalIdIsSetIdx] = true;
        }
    }

    public bool NormalSequencingPlanExternalIdIsSet => m_bools[c_normalSequencingPlanExternalIdIsSetIdx];

    private string m_experimentalSequencingPlanOneExternalId;

    public string ExperimentalSequencingPlanOneExternalId
    {
        get => m_experimentalSequencingPlanOneExternalId;
        set
        {
            m_experimentalSequencingPlanOneExternalId = value;
            m_bools[c_experimentalDispatcherOneExternalIdIsSetIdx] = true;
        }
    }

    private string m_experimentalSequencingPlanTwoExternalId;

    public string ExperimentalSequencingPlanTwoExternalId
    {
        get => m_experimentalSequencingPlanTwoExternalId;
        set
        {
            m_experimentalSequencingPlanTwoExternalId = value;
            m_bools[c_experimentalDispatcherTwoExternalIdIsSetIdx] = true;
        }
    }

    private string m_experimentalSequencingPlanThreeExternalId;

    public string ExperimentalSequencingPlanThreeExternalId
    {
        get => m_experimentalSequencingPlanThreeExternalId;
        set
        {
            m_experimentalSequencingPlanThreeExternalId = value;
            m_bools[c_experimentalDispatcherThreeExternalIdIsSetIdx] = true;
        }
    }

    private string m_experimentalSequencingPlanFourExternalId;

    public string ExperimentalSequencingPlanFourExternalId
    {
        get => m_experimentalSequencingPlanFourExternalId;
        set
        {
            m_experimentalSequencingPlanFourExternalId = value;
            m_bools[c_experimentalDispatcherFourExternalIdIsSetIdx] = true;
        }
    }

    public bool ExperimentalDispatcherOneExternalIdIsSet => m_bools[c_experimentalDispatcherOneExternalIdIsSetIdx];
    public bool ExperimentalDispatcherTwoExternalIdIsSet => m_bools[c_experimentalDispatcherTwoExternalIdIsSetIdx];
    public bool ExperimentalDispatcherThreeExternalIdIsSet => m_bools[c_experimentalDispatcherThreeExternalIdIsSetIdx];
    public bool ExperimentalDispatcherFourExternalIdIsSet => m_bools[c_experimentalDispatcherFourExternalIdIsSetIdx];

    private string m_cellExternalId;

    public string CellExternalId
    {
        get => m_cellExternalId;
        set
        {
            m_cellExternalId = value;
            m_bools[CellSetIdx] = true;
        }
    }

    public bool CellSet => m_bools[CellSetIdx];
    #endregion Shared Properties

    #region Transmission Only Properties
    private int m_numberOfResources = 1;

    /// <summary>
    /// The number of Resources that should be created from this Resource definition.
    /// The value must be at least 1.  If the value is greater than 1 then copies of the Resource will be created with an ExternalId
    /// and Name that have a '-n' appended where n is the index number.
    /// </summary>
    public int NumberOfResources
    {
        get => m_numberOfResources;
        set
        {
            if (value < 1)
            {
                throw new APSCommon.PTValidationException("2044");
            }

            m_numberOfResources = value;
        }
    }
    #endregion

    #region Connectors
    /// <summary>
    /// Identifies a successor resource in a resource connection relationship.
    /// </summary>
    public class Connector : ConnectorBase
    {
        #region IPTSerializable Members
        public Connector(IReader reader)
            : base(reader)
        {
            if (reader.VersionNumber >= 1)
            {
                reader.Read(out string deprecated);
                reader.Read(out deprecated);
                reader.Read(out deprecated);
            }
        }

        public new const int UNIQUE_ID = 666;
        public override int UniqueId => UNIQUE_ID;
        #endregion
    }
    #endregion Connectors

    private readonly List<AllowedHelper> m_allowedHelpers = new ();
    public List<AllowedHelper> AllowedHelpers => m_allowedHelpers;

    public class AllowedHelper
    {
        public const int UNIQUE_ID = 705;

        #region IPTSerializable Members
        public AllowedHelper(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                reader.Read(out _plantExternalId);
                reader.Read(out _deptExternalId);
                reader.Read(out _resExternalId);
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(_plantExternalId);
            writer.Write(_deptExternalId);
            writer.Write(_resExternalId);
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public AllowedHelper(string aPlantExternalId, string aDeptExternalId, string aResourceExternalId)
        {
            _plantExternalId = aPlantExternalId;
            _deptExternalId = aDeptExternalId;
            _resExternalId = aResourceExternalId;
        }

        private string _plantExternalId;

        public string PlantExternalId
        {
            get => _plantExternalId;
            set => _plantExternalId = value;
        }

        private string _deptExternalId;

        public string DepartmentExternalId
        {
            get => _deptExternalId;
            set => _deptExternalId = value;
        }

        private string _resExternalId;

        public string ResourceExternalId
        {
            get => _resExternalId;
            set => _resExternalId = value;
        }
    }
}