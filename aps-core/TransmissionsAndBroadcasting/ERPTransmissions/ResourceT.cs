using System.ComponentModel;
using System.Data;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

public class ResourceT : ERPMaintenanceTransmission<ResourceT.Resource>, IPTSerializable
{
    public new const int UNIQUE_ID = 258;

    #region PT Serialization
    public ResourceT(IReader a_reader)
        : base(a_reader)
    {
        #region 12431
        if (a_reader.VersionNumber >= 12431)
        {
            m_bools = new BoolVector32(a_reader);
            int count;
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                Resource node = new(a_reader);
                Add(node);
            }
        }
        #endregion
        else
        {
            bool autoDeleteCapabilityAssociations = false;
            bool updateAllowedHelpers = false;

            if (a_reader.VersionNumber >= 12315)
            {
                a_reader.Read(out autoDeleteCapabilityAssociations);
                a_reader.Read(out updateAllowedHelpers);
                int count;
                a_reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    Resource node = new(a_reader);
                    Add(node);
                }
            }
            else if (a_reader.VersionNumber >= 352)
            {
                a_reader.Read(out autoDeleteCapabilityAssociations);
                a_reader.Read(out bool m_autoDeleteConnectors);
                a_reader.Read(out bool m_updateConnectors);
                a_reader.Read(out updateAllowedHelpers);

                int count;
                a_reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    Resource node = new (a_reader);
                    Add(node);
                }
            }
            AutoDeleteCapabilityAssociations = autoDeleteCapabilityAssociations;
            UpdateAllowedHelpers = updateAllowedHelpers;
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_bools.Serialize(a_writer);

        a_writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(a_writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ResourceT() { }
    private BoolVector32 m_bools = new BoolVector32();
    private const short c_includeCapabilityAssociationsIdx = 0;
    private const short c_autoDeleteCapabilityAssociationsIdx = 1;
    private const short c_updateAllowedHelpersIdx = 2;
    private const short c_autoDeleteAllowedHelpersIdx = 3;
    public bool IncludeCapabilityAssociations
    {
        get => m_bools[c_includeCapabilityAssociationsIdx];
        set => m_bools[c_includeCapabilityAssociationsIdx] = value;
    }
    /// <summary>
    /// Whether to remove missing Capability Associations when updating.
    /// </summary>
    public bool AutoDeleteCapabilityAssociations
    {
        get => m_bools[c_autoDeleteCapabilityAssociationsIdx];
        set => m_bools[c_autoDeleteCapabilityAssociationsIdx] = value;
    }

    /// <summary>
    /// Whether to update the Allowed Helper Resources on Resources.
    /// </summary>
    public bool UpdateAllowedHelpers
    {
        get => m_bools[c_updateAllowedHelpersIdx];
        set => m_bools[c_updateAllowedHelpersIdx] = value;
    }
    /// <summary>
    /// Whether to remove missing Allowed Helper Resources when updating.
    /// </summary>
    public bool AutoDeleteAllowedHelpers
    {
        get => m_bools[c_autoDeleteAllowedHelpersIdx];
        set => m_bools[c_autoDeleteAllowedHelpersIdx] = value;
    }

    public new void Validate()
    {
        HashSet<string> addedHash = new ();

        for (int i = 0; i < Count; i++)
        {
            Resource res = this[i];
            if (addedHash.Contains(GetId(res)))
            {
                throw new ValidationException("2734", new object[] { res.ExternalId, res.DepartmentExternalId, res.PlantExternalId });
            }

            addedHash.Add(GetId(res));
        }
    }

    private string GetId(Resource res)
    {
        return string.Format("{0}~~{1}~~{2}", res.ExternalId, res.DepartmentExternalId, res.PlantExternalId);
    }

    public class Resource : InternalResource, IPTSerializable
    {
        public new const int UNIQUE_ID = 259;

        #region PT Serialization
        public Resource(IReader a_reader)
            : base(a_reader)
        {
            if (a_reader.VersionNumber >= 12320)
            {
                m_isSetBools = new BoolVector32(a_reader);
                
                a_reader.Read(out postActivityRestSpanSet);
                a_reader.Read(out postActivityRestSpan);

                a_reader.Read(out minQty);
                a_reader.Read(out minQtySet);
                a_reader.Read(out maxQty);
                a_reader.Read(out maxQtySet);
                a_reader.Read(out minQtyPerCycle);
                a_reader.Read(out minQtyPerCycleSet);
                a_reader.Read(out maxQtyPerCycle);
                a_reader.Read(out maxQtyPerCycleSet);
                a_reader.ReadList(out m_capabilities);
                a_reader.Read(out sequential);
                a_reader.Read(out sequentialSet);

                a_reader.Read(out m_minVolume);
                a_reader.Read(out m_maxVolume);
            }
            else if (a_reader.VersionNumber >= 12106)
            {
                m_isSetBools = new BoolVector32(a_reader);

                a_reader.Read(out string compatibilityGroup);
                a_reader.Read(out bool compatibilityGroupSet);

                a_reader.Read(out postActivityRestSpanSet);
                a_reader.Read(out postActivityRestSpan);

                a_reader.Read(out minQty);
                a_reader.Read(out minQtySet);
                a_reader.Read(out maxQty);
                a_reader.Read(out maxQtySet);
                a_reader.Read(out minQtyPerCycle);
                a_reader.Read(out minQtyPerCycleSet);
                a_reader.Read(out maxQtyPerCycle);
                a_reader.Read(out maxQtyPerCycleSet);
                a_reader.ReadList(out m_capabilities);
                a_reader.Read(out sequential);
                a_reader.Read(out sequentialSet);

                a_reader.Read(out m_minVolume);
                a_reader.Read(out m_maxVolume);
            }
            else if (a_reader.VersionNumber >= 359)
            {
                a_reader.Read(out string compatibilityGroup);
                a_reader.Read(out bool compatibilityGroupSet);

                a_reader.Read(out postActivityRestSpanSet);
                a_reader.Read(out postActivityRestSpan);

                a_reader.Read(out minQty);
                a_reader.Read(out minQtySet);
                a_reader.Read(out maxQty);
                a_reader.Read(out maxQtySet);
                a_reader.Read(out minQtyPerCycle);
                a_reader.Read(out minQtyPerCycleSet);
                a_reader.Read(out maxQtyPerCycle);
                a_reader.Read(out maxQtyPerCycleSet);
                a_reader.ReadList(out m_capabilities);
                a_reader.Read(out sequential);
                a_reader.Read(out sequentialSet);
            }
        }

        public override void Serialize(IWriter a_writer)
        {
            base.Serialize(a_writer);

            m_isSetBools.Serialize(a_writer);
            
            a_writer.Write(postActivityRestSpanSet);
            a_writer.Write(postActivityRestSpan);

            a_writer.Write(minQty);
            a_writer.Write(minQtySet);
            a_writer.Write(maxQty);
            a_writer.Write(maxQtySet);
            a_writer.Write(minQtyPerCycle);
            a_writer.Write(minQtyPerCycleSet);
            a_writer.Write(maxQtyPerCycle);
            a_writer.Write(maxQtyPerCycleSet);

            a_writer.WriteList(m_capabilities);
            a_writer.Write(sequential);
            a_writer.Write(sequentialSet);

            a_writer.Write(m_minVolume);
            a_writer.Write(m_maxVolume);
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        #region Shared Properties
        //NEWOPFIELD Bookmark
        
        private TimeSpan postActivityRestSpan = new (0);

        /// <summary>
        /// Specifies the amount of idle time to schedule between Activities to provide time for rest or miscellaneous tasks.
        /// </summary>
        public TimeSpan PostActivityRestSpan
        {
            get => postActivityRestSpan;
            set
            {
                postActivityRestSpan = value;
                postActivityRestSpanSet = true;
            }
        }

        private bool postActivityRestSpanSet;

        public bool PostActivityRestSpanSet => postActivityRestSpanSet;

        private decimal minQty;

        /// <summary>
        /// For the Resource to be considered eligible for an Activity the Activity RequiredFinishQty must be at least this amount.
        /// </summary>
        public decimal MinQty
        {
            get => minQty;
            set
            {
                minQty = value;
                minQtySet = true;
            }
        }

        private bool minQtySet;

        public bool MinQtySet => minQtySet;

        private decimal maxQty = decimal.MaxValue;

        /// <summary>
        /// For the Resource to be considered eligible for an Activity the Activity RequiredFinishQty must be less than or equal to this amount.
        /// </summary>
        public decimal MaxQty
        {
            get => maxQty;
            set
            {
                maxQty = value;
                maxQtySet = true;
            }
        }

        private bool maxQtySet;

        public bool MaxQtySet => maxQtySet;

        private decimal minQtyPerCycle;

        /// <summary>
        /// For the Resource to be considered eligible for an Operation the Operation's QtyPerCyle must be at least this amount.
        /// This is often used for batch processes where a Resource can hold a certain volume and there is a desire to use smaller Resources for smaller orders to avoid wasting capacity.
        /// </summary>
        public decimal MinQtyPerCycle
        {
            get => minQtyPerCycle;
            set
            {
                minQtyPerCycle = value;
                minQtyPerCycleSet = true;
            }
        }

        private bool minQtyPerCycleSet;

        public bool MinQtyPerCycleSet => minQtyPerCycleSet;

        private decimal maxQtyPerCycle = decimal.MaxValue;

        /// <summary>
        /// For the Resource to be considered eligible for an Operation the Operation's QtyPerCyle must be less than or equal to this amount.
        /// This is often used for batch processes where a Resource can hold a certain volume and that volume cannot be exceeded.
        /// </summary>
        public decimal MaxQtyPerCycle
        {
            get => maxQtyPerCycle;
            set
            {
                maxQtyPerCycle = value;
                maxQtyPerCycleSet = true;
            }
        }

        private bool maxQtyPerCycleSet;

        public bool MaxQtyPerCycleSet => maxQtyPerCycleSet;

        private bool sequential;

        /// <summary>
        /// This property was added to help model conveyors. Model the minimum length of time an activity must stay on the conveyor (transfer time) using a Infinite
        /// sequential resource. And model the unloading of the conveyor as a finite resource. You may also need to place the 2 resources used to model the conveyor
        /// and predecessor resource in the same cell. Setup the unloading resource and operation so that the length of the unload matches the length of the load.
        /// </summary>
        public bool Sequential
        {
            get => sequential;
            set
            {
                sequential = value;
                sequentialSet = true;
            }
        }

        private bool sequentialSet;

        public bool SequentialSet => sequentialSet;

        private BoolVector32 m_isSetBools;

        private const short c_minVolumeIsSetIdx = 0;
        private const short c_maxVolumeIsSetIdx = 1;

        private decimal m_minVolume;

        /// <summary>
        /// For the Resource to be considered eligible for an Activity the Operation's ProductVolume must be at least this amount.
        /// </summary>
        public decimal MinVolume
        {
            get => m_minVolume;
            internal set
            {
                m_minVolume = value;
                m_isSetBools[c_minVolumeIsSetIdx] = true;
            }
        }

        public bool MinVolumeSet => m_isSetBools[c_minVolumeIsSetIdx];

        private decimal m_maxVolume = decimal.MaxValue;

        /// <summary>
        /// For the Resource to be considered eligible for an Activity the Operation's ProductVolume must be less than or equal to this amount.
        /// </summary>
        public decimal MaxVolume
        {
            get => m_maxVolume;
            internal set
            {
                m_maxVolume = value;
                m_isSetBools[c_maxVolumeIsSetIdx] = true;
            }
        }

        public bool MaxVolumeSet => m_isSetBools[c_maxVolumeIsSetIdx];
        #endregion Shared Properties

        public Resource(string externalId, string name, string description, string notes, string userFields, string plantExternalId, string departmentExternalId)
            : base(externalId, name, description, notes, userFields, plantExternalId, departmentExternalId) { }

        private List<string> m_capabilities = new ();

        [Browsable(false)]
        public List<string> Capabilities
        {
            get => m_capabilities;
            set => m_capabilities = value;
        }

        public Resource(ResourceTDataSet.ResourceRow row)
            : base(row)
        {
           
            if (!row.IsDisallowDragAndDropsNull())
            {
                DisallowDragAndDrops = row.DisallowDragAndDrops;
            }

            if (!row.IsMaxQtyNull())
            {
                MaxQty = row.MaxQty;
            }

            if (!row.IsMaxQtyPerCycleNull())
            {
                MaxQtyPerCycle = row.MaxQtyPerCycle;
            }

            if (!row.IsMinQtyNull())
            {
                MinQty = row.MinQty;
            }

            if (!row.IsMinQtyPerCycleNull())
            {
                MinQtyPerCycle = row.MinQtyPerCycle;
            }

            if (!row.IsSequentialNull())
            {
                Sequential = row.Sequential;
            }

            if (!row.IsMinVolumeNull())
            {
                MinVolume = row.MinVolume;
            }

            if (!row.IsMaxVolumeNull())
            {
                MaxVolume = row.MaxVolume;
            }

            //Add capabilities
            ResourceTDataSet.CapabilityAssignmentsRow[] capAssignments = row.GetCapabilityAssignmentsRows();
            for (int i = 0; i < capAssignments.Length; i++)
            {
                ResourceTDataSet.CapabilityAssignmentsRow capAssignRow = (ResourceTDataSet.CapabilityAssignmentsRow)capAssignments.GetValue(i);
                Capabilities.Add(capAssignRow.CapabilityExternalId);
            }

            //Allowed Helpers
            ResourceTDataSet.AllowedHelperResourcesRow[] ahRows = row.GetAllowedHelperResourcesRows();
            for (int i = 0; i < ahRows.Length; i++)
            {
                ResourceTDataSet.AllowedHelperResourcesRow ahRow = (ResourceTDataSet.AllowedHelperResourcesRow)ahRows.GetValue(i);
                AllowedHelpers.Add(new AllowedHelper(ahRow.AllowedHelperPlantExternalId, ahRow.AllowedHelperDepartmentExternalId, ahRow.AllowedHelperResourceExternalId));
            }
        }

        public Tuple<string, string, string> GetExternalKey()
        {
            return new Tuple<string, string, string>(PlantExternalId, DepartmentExternalId, ExternalId);
        }
    }

    #region Database Loading
    public void Fill(IDbCommand a_resourceTableCmd,
                     IDbCommand a_capAssignCmd,
                     bool a_includeCapabilityAssignments,
                     bool a_includeAllowedHelperResources,
                     IDbCommand a_allowedHelperResoucesCmd)
    {
        ResourceTDataSet ds = new ();
        FillTable(ds.Resource, a_resourceTableCmd);
        IncludeCapabilityAssociations = a_includeCapabilityAssignments;
        if (a_includeCapabilityAssignments)
        {
            FillTable(ds.CapabilityAssignments, a_capAssignCmd);

            foreach (ResourceTDataSet.CapabilityAssignmentsRow capRow in ds.CapabilityAssignments)
            {
                if (!ds.Resource.Any(r => r.ExternalId == capRow.ResourceExternalId && r.DepartmentExternalId == capRow.DepartmentExternalId && r.PlantExternalId == capRow.PlantExternalId))
                {
                    throw new PTValidationException("4474", new object[] { capRow.ResourceExternalId, capRow.DepartmentExternalId, capRow.PlantExternalId });
                }
            }
        }

        if (a_includeAllowedHelperResources)
        {
            FillTable(ds.AllowedHelperResources, a_allowedHelperResoucesCmd);
        }

        Fill(ds);
    }

    /// <summary>
    /// Fill the transmission with data from the DataSet.
    /// </summary>
    /// <param name="ds"></param>
    public void Fill(ResourceTDataSet ds)
    {
        for (int i = 0; i < ds.Resource.Count; i++)
        {
            Add(new Resource(ds.Resource[i]));
        }
    }
    #endregion Database Loading

    public new Resource this[int i] => Nodes[i];

    public override string Description => string.Format("Resources updated ({0})".Localize(), Count);
}