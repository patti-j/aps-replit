using System.Collections;
using System.Drawing;

using PT.APSCommon;
using PT.SchedulerDefinitions;
using PT.Transmissions;

using ColorUtils = PT.Common.ColorUtils;

namespace PT.ERPTransmissions;

public partial class JobT
{
    public class ManufacturingOrder : PTObjectBase, IPTSerializable
    {
        public new const int UNIQUE_ID = 223;

        #region PT Serialization
        public ManufacturingOrder(IReader reader)
            : base(reader)
        {
            if (reader.VersionNumber >= 13007)
            {
                reader.Read(out expectedFinishQty);
                reader.Read(out family);
                reader.Read(out productDescription);
                reader.Read(out productName);
                reader.Read(out copyRoutingFromTemplate);
                reader.Read(out requiredQty);
                reader.Read(out uom);
                reader.Read(out canSpanPlants);
                reader.Read(out moNeedDate);
                reader.Read(out needDate);
                reader.Read(out productColor);
                reader.Read(out holdReason);
                reader.Read(out holdUntilDateTime);
                reader.Read(out preserveRequiredQty);
                reader.Read(out lockedPlantExternalId);
                reader.Read(out m_lockToCurrentAlternatePath);
                reader.Read(out int splitUpdateModeTemp);
                m_splitUpdateMode = (ManufacturingOrderDefs.SplitUpdateModes)splitUpdateModeTemp;

                reader.Read(out int resOpCount);
                for (int i = 0; i < resOpCount; i++)
                {
                    ResourceOperation op = new(reader);
                    AddOperation(op);
                }

                reader.Read(out int pathCount);
                for (int p = 0; p < pathCount; p++)
                {
                    AlternatePath path = new(reader);
                    AddPath(path);
                }

                reader.Read(out bool haveDefaultPath);
                if (haveDefaultPath)
                {
                    reader.Read(out string defaultPathExternalId);
                    for (int pI = 0; pI < alternatePaths.Count; ++pI)
                    {
                        AlternatePath ap = (AlternatePath)alternatePaths[pI];
                        if (defaultPathExternalId == ap.ExternalId)
                        {
                            defaultPath = ap;
                            break;
                        }
                    }
                }
                else
                {
                    defaultPath = null;
                }

                m_successorMOs = new SuccessorMOArrayList(reader);

                bools = new BoolVector32(reader);
                reader.Read(out m_autoJoinGroup);

                reader.Read(out m_shippingBufferOverrideTicks);
            }
            #region 13005
            else if (reader.VersionNumber >= 13005)
            {
                reader.Read(out expectedFinishQty);
                reader.Read(out family);
                reader.Read(out productDescription);
                reader.Read(out productName);
                reader.Read(out copyRoutingFromTemplate);
                reader.Read(out requiredQty);
                reader.Read(out uom);
                reader.Read(out canSpanPlants);
                reader.Read(out moNeedDate);
                reader.Read(out needDate);
                reader.Read(out productColor);
                reader.Read(out holdReason);
                reader.Read(out holdUntilDateTime);
                reader.Read(out preserveRequiredQty);
                reader.Read(out lockedPlantExternalId);
                reader.Read(out string batchDefinitionName);
                reader.Read(out string batchGroupName);
                reader.Read(out m_lockToCurrentAlternatePath);
                reader.Read(out int splitUpdateModeTemp);
                m_splitUpdateMode = (ManufacturingOrderDefs.SplitUpdateModes)splitUpdateModeTemp;

                reader.Read(out int resOpCount);
                for (int i = 0; i < resOpCount; i++)
                {
                    ResourceOperation op = new (reader);
                    AddOperation(op);
                }

                reader.Read(out int pathCount);
                for (int p = 0; p < pathCount; p++)
                {
                    AlternatePath path = new (reader);
                    AddPath(path);
                }

                reader.Read(out bool haveDefaultPath);
                if (haveDefaultPath)
                {
                    reader.Read(out string defaultPathExternalId);
                    for (int pI = 0; pI < alternatePaths.Count; ++pI)
                    {
                        AlternatePath ap = (AlternatePath)alternatePaths[pI];
                        if (defaultPathExternalId == ap.ExternalId)
                        {
                            defaultPath = ap;
                            break;
                        }
                    }
                }
                else
                {
                    defaultPath = null;
                }

                m_successorMOs = new SuccessorMOArrayList(reader);

                bools = new BoolVector32(reader);
                reader.Read(out m_autoJoinGroup);

                reader.Read(out m_shippingBufferOverrideTicks);
            }
            #endregion            
            #region 600
            else if (reader.VersionNumber >= 600)
            {
                reader.Read(out expectedFinishQty);
                reader.Read(out family);
                reader.Read(out bool isReleased);
                reader.Read(out productDescription);
                reader.Read(out productName);
                reader.Read(out copyRoutingFromTemplate);
                reader.Read(out requiredQty);
                reader.Read(out uom);
                reader.Read(out canSpanPlants);
                reader.Read(out moNeedDate);
                reader.Read(out needDate);
                reader.Read(out productColor);
                reader.Read(out holdReason);
                reader.Read(out holdUntilDateTime);
                reader.Read(out preserveRequiredQty);
                reader.Read(out lockedPlantExternalId);
                reader.Read(out string batchDefinitionName);
                reader.Read(out string batchGroupName);
                reader.Read(out m_lockToCurrentAlternatePath);
                int splitUpdateModeTemp;
                reader.Read(out splitUpdateModeTemp);
                m_splitUpdateMode = (ManufacturingOrderDefs.SplitUpdateModes)splitUpdateModeTemp;

                int resOpCount;
                reader.Read(out resOpCount);
                for (int i = 0; i < resOpCount; i++)
                {
                    ResourceOperation op = new (reader);
                    AddOperation(op);
                }

                int pathCount;
                reader.Read(out pathCount);
                for (int p = 0; p < pathCount; p++)
                {
                    AlternatePath path = new (reader);
                    AddPath(path);
                }

                reader.Read(out long releaseDateTime);

                bool haveDefaultPath;
                reader.Read(out haveDefaultPath);
                if (haveDefaultPath)
                {
                    string defaultPathExternalId;
                    reader.Read(out defaultPathExternalId);
                    for (int pI = 0; pI < alternatePaths.Count; ++pI)
                    {
                        AlternatePath ap = (AlternatePath)alternatePaths[pI];
                        if (defaultPathExternalId == ap.ExternalId)
                        {
                            defaultPath = ap;
                            break;
                        }
                    }
                }
                else
                {
                    defaultPath = null;
                }

                m_successorMOs = new SuccessorMOArrayList(reader);

                bools = new BoolVector32(reader);
                reader.Read(out m_autoJoinGroup);

                reader.Read(out m_shippingBufferOverrideTicks);
            }
            #endregion
        }

        //TODO: Move the bools into boolvector
        public override void Serialize(IWriter writer)
        {
            base.Serialize(writer);

            writer.Write(expectedFinishQty);
            writer.Write(family);
            writer.Write(productDescription);
            writer.Write(productName);
            writer.Write(copyRoutingFromTemplate);
            writer.Write(requiredQty);
            writer.Write(uom);
            writer.Write(canSpanPlants);
            writer.Write(moNeedDate);
            writer.Write(needDate);
            writer.Write(productColor);
            writer.Write(holdReason);
            writer.Write(holdUntilDateTime);
            writer.Write(preserveRequiredQty);
            writer.Write(lockedPlantExternalId);
            writer.Write(m_lockToCurrentAlternatePath);
            writer.Write((int)m_splitUpdateMode);

            writer.Write(OperationCount);

            for (int i = 0; i < operations.Count; i++)
            {
                BaseOperation op = (BaseOperation)operations[i];
                op.Serialize(writer);
            }

            writer.Write(PathCount);
            for (int p = 0; p < PathCount; p++)
            {
                GetAlternatePath(p).Serialize(writer);
            }

            writer.Write(defaultPath != null);
            if (defaultPath != null)
            {
                writer.Write(defaultPath.ExternalId);
            }

            m_successorMOs.Serialize(writer);

            bools.Serialize(writer);
            writer.Write(m_autoJoinGroup);

            writer.Write(m_shippingBufferOverrideTicks);
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        #region BoolVector32
        private BoolVector32 bools;
        private const int obsoleteIdx = 0; //ReleaseDate
        private const int HoldIdx = 1;
        private const int HoldSetIdx = 2;
        private const int HoldReasonSetIdx = 3;
        private const int HoldDateSetIdx = 4;

        private const int PreserveRequiredQtySetIdx = 5;

        //const int DEPRECATED = 6;
        private const int AutoJoinGroupSetIdx = 7;
        private const int c_canSpanPlantsIsSetIdx = 8;
        private const int c_resizeForStorageIdx = 9;
        private const int c_resizeForStorageIsSetIdx = 10;
        #endregion

        public ManufacturingOrder() { } // reqd. for xml serialization

        public ManufacturingOrder(string externalId, string name, decimal requiredQty)
            :
            base(externalId, name)
        {
            RequiredQty = requiredQty;
            ExpectedFinishQty = requiredQty;
        }

        public ManufacturingOrder(JobDataSet.ManufacturingOrderRow row)
            : base(row.ExternalId)
        {
            //set base values
            Name = row.Name;
            if (!row.IsDescriptionNull())
            {
                Description = row.Description;
            }

            if (!row.IsNotesNull())
            {
                Notes = row.Notes;
            }

            if (!row.IsUserFieldsNull())
            {
                SetUserFields(row.UserFields);
            }

            if (!row.IsCanSpanPlantsNull())
            {
                CanSpanPlants = row.CanSpanPlants;
            }

            //JMC TODO Need to set Default path somehow
            if (!row.IsExpectedFinishQtyNull())
            {
                ExpectedFinishQty = row.ExpectedFinishQty;
            }

            if (!row.IsFamilyNull())
            {
                Family = row.Family;
            }
            
            if (!row.IsMoNeedDateNull())
            {
                MoNeedDate = row.MoNeedDate;
            }

            if (MoNeedDate)
            {
                NeedDate = row.NeedDate.ToServerTime();
            }

            if (!row.IsProductDescriptionNull())
            {
                ProductDescription = row.ProductDescription;
            }

            if (!row.IsProductNameNull())
            {
                ProductName = row.ProductName;
            }

            RequiredQty = row.RequiredQty;
            //if(!row.IsRequestedQtyNull()) //This is not setable in the UI
            if (!row.IsPreserveRequiredQtyNull())
            {
                PreserveRequiredQty = row.PreserveRequiredQty;
            }

            if (!row.IsUOMNull())
            {
                UOM = row.UOM;
            }

            if (!row.IsAutoJoinGroupNull())
            {
                AutoJoinGroup = row.AutoJoinGroup;
            }

            if (!row.IsCopyRoutingFromTemplateNull())
            {
                CopyRoutingFromTemplate = row.CopyRoutingFromTemplate;
            }

            if (!row.IsProductColorNull())
            {
                ProductColor = ColorUtils.GetColorFromHexString(row.ProductColor);
            }

            //Hold
            if (!row.IsHoldReasonNull())
            {
                HoldReason = row.HoldReason;
            }

            if (!row.IsHoldUntilDateNull())
            {
                HoldUntilDate = row.HoldUntilDate.ToServerTime();
            }

            if (!row.IsHoldNull())
            {
                Hold = row.Hold;
            }


            if (!row.IsLockedPlantExternalIdNull())
            {
                LockedPlantExternalId = row.LockedPlantExternalId;
            }

            if (!row.IsUserFieldsNull())
            {
                SetUserFields(row.UserFields);
            }

            if (!row.IsLockToCurrentAlternatePathNull())
            {
                LockToCurrentAlternatePath = row.LockToCurrentAlternatePath;
            }

            if (!row.IsShippingBufferOverrideDaysNull())
            {
                ShippingBufferOverride = TimeSpan.FromDays(row.ShippingBufferOverrideDays).Ticks;
            }
            else
            {
                ShippingBufferOverride = null;
            }

            if (!row.IsSplitUpdateModeNull())
            {
                try
                {
                    SplitUpdateMode = (ManufacturingOrderDefs.SplitUpdateModes)Enum.Parse(typeof(ManufacturingOrderDefs.SplitUpdateModes), row.SplitUpdateMode);
                }
                catch (Exception err)
                {
                    throw new APSCommon.PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            row.SplitUpdateMode, "ManufacturingOrder", "SplitUpdateMode",
                            string.Join(", ", Enum.GetNames(typeof(ManufacturingOrderDefs.SplitUpdateModes)))
                        });
                }
            }

            if (!row.IsResizeForStorageNull())
            {
                ResizeForStorage = row.ResizeForStorage;
            }
        }

        public class DuplicateOperationException : ValidationException
        {
            public DuplicateOperationException(string msg, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
                : base(msg, a_stringParameters, a_appendHelpUrl) { }
        }

        #region Shared Properties
        private decimal expectedFinishQty;

        /// <summary>
        /// Usually the same as promiseQty but production problems may result in more or less than required. externally specified.
        /// </summary>
        public decimal ExpectedFinishQty
        {
            get => expectedFinishQty;
            set => expectedFinishQty = value;
        }

        private string family = "";

        /// <summary>
        /// Provides a way for grouping MOs.  For display and custom Algorithms.
        /// </summary>
        public string Family
        {
            get => family;
            set => family = value;
        }

        private bool moNeedDate;

        /// <summary>
        /// If true then the Need Date can is set explicityly for the M.O..  Otherwise, the M.O. Need Date is set to the Job Need Date.
        /// </summary>
        public bool MoNeedDate
        {
            get => moNeedDate;
            set => moNeedDate = value;
        }

        private DateTime needDate;

        /// <summary>
        /// The date and time when the M.O. should be finished to be considered on-time.
        /// If MoNeedDate is true then this is the value stored for the M.O..  Otherwise, this is the same as the Job Need Date.
        /// </summary>
        public DateTime NeedDate
        {
            get => needDate;
            set => needDate = value;
        }

        private string productDescription = "";

        /// <summary>
        /// Description of the product being made.
        /// </summary>
        public string ProductDescription
        {
            get => productDescription;
            set => productDescription = value;
        }

        private string productName = "";

        /// <summary>
        /// Name of the product being made.
        /// </summary>
        public string ProductName
        {
            get => productName;
            set => productName = value;
        }

        private bool copyRoutingFromTemplate;

        /// <summary>
        /// If true then any Path information is ignored.  Instead, the Path information for an existing Template Job is copied and used.
        /// The Product Name specifies which Product to copy the Paths from.  If there is more than one Template for the same Product then the first one found by the system will be used.			///
        /// </summary>
        public bool CopyRoutingFromTemplate
        {
            get => copyRoutingFromTemplate;
            set => copyRoutingFromTemplate = value;
        }

        private Color productColor = Color.White;

        /// <summary>
        /// Color code of the product being made.
        /// </summary>
        public Color ProductColor
        {
            get => productColor;
            set => productColor = value;
        }
        
        private decimal requiredQty;

        /// <summary>
        /// The target quantity of good Product to be made.
        /// </summary>
        [Required(true)]
        public decimal RequiredQty
        {
            get => requiredQty;
            set => requiredQty = value;
        }

        private bool preserveRequiredQty;

        /// <summary>
        /// If true then the Required Qty can only be set manually, not via ERP imports.
        /// This value is set to true automatically when the Required Qty is set internally by a planner in order to preserve the change.
        /// </summary>
        public bool PreserveRequiredQty
        {
            get => preserveRequiredQty;
            set
            {
                preserveRequiredQty = value;
                bools[PreserveRequiredQtySetIdx] = true;
            }
        }

        public bool PreserveRequiredQtySet => bools[PreserveRequiredQtySetIdx];

        private string uom = "";

        /// <summary>
        /// Unit of measure.  For display and custom Algorithms.
        /// </summary>
        public string UOM
        {
            get => uom;
            set => uom = value;
        }

        private string m_autoJoinGroup;

        /// <summary>
        /// Specifies which Manufacturing Orders can be AutoJoined.  They must have non-blank, matching AutoJoinGroup values.
        /// </summary>
        public string AutoJoinGroup
        {
            get => m_autoJoinGroup;
            set
            {
                m_autoJoinGroup = value;
                AutoJoinGroupSet = true;
            }
        }

        public bool AutoJoinGroupSet
        {
            get => bools[AutoJoinGroupSetIdx];
            private set => bools[AutoJoinGroupSetIdx] = value;
        }

        /// <summary>
        /// Whether the MO should adjust its quantity up or down to fill storage
        /// </summary>
        public bool ResizeForStorage
        {
            get => bools[c_resizeForStorageIdx];
            set
            {
                bools[c_resizeForStorageIdx] = value;
                bools[c_resizeForStorageIsSetIdx] = true;
            }
        }

        public bool ResizeForStorageIsSet
        {
            get => bools[c_resizeForStorageIsSetIdx];
            private set => bools[c_resizeForStorageIsSetIdx] = value;
        }

        private AlternatePath defaultPath;

        /// <summary>
        /// The initial path to be used. The a scheduler user may select a different path.
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public AlternatePath DefaultPath
        {
            get => defaultPath;
            set => defaultPath = value;
        }

        /// <summary>
        /// Whether the Manufacturing Order was placed On-hold and work should not be done on it.
        /// </summary>
        public bool Hold
        {
            get => bools[HoldIdx];
            set
            {
                bools[HoldIdx] = value;
                bools[HoldSetIdx] = true;
            }
        }

        public bool HoldSet => bools[HoldSetIdx];

        private string holdReason = "";

        /// <summary>
        /// The reason the Manufacturing Order was placed On-Hold
        /// </summary>
        public string HoldReason
        {
            get => holdReason;
            set
            {
                holdReason = value;
                bools[HoldReasonSetIdx] = true;
            }
        }

        public bool HoldReasonSet => bools[HoldReasonSetIdx];

        private long holdUntilDateTime;

        /// <summary>
        /// No Activities are scheduled before this date/time.
        /// This value is only set if the Manufacturing Order itself is placed On Hold, not if Operations, etc. only are placed On Hold.
        /// </summary>
        public DateTime HoldUntilDate
        {
            get => new (holdUntilDateTime);
            set
            {
                holdUntilDateTime = value.Ticks;
                bools[HoldDateSetIdx] = true;
            }
        }

        public bool HoldUntilSet => bools[HoldDateSetIdx];

        private string lockedPlantExternalId;

        public string LockedPlantExternalId
        {
            get => lockedPlantExternalId;
            set => lockedPlantExternalId = value;
        }

        private bool m_lockToCurrentAlternatePath;

        public bool LockToCurrentAlternatePath
        {
            get => m_lockToCurrentAlternatePath;
            set => m_lockToCurrentAlternatePath = value;
        }

        private long? m_shippingBufferOverrideTicks;

        public long? ShippingBufferOverride
        {
            get
            {
                if (m_shippingBufferOverrideTicks.HasValue)
                {
                    return m_shippingBufferOverrideTicks.Value;
                }

                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    m_shippingBufferOverrideTicks = value.Value;
                }
                else
                {
                    m_shippingBufferOverrideTicks = null;
                }
            }
        }

        private ManufacturingOrderDefs.SplitUpdateModes m_splitUpdateMode;

        /// <summary>
        /// Specifies how updates to the Manufacturing Order are processed when the MO has been previously split.
        /// </summary>
        public ManufacturingOrderDefs.SplitUpdateModes SplitUpdateMode
        {
            get => m_splitUpdateMode;
            internal set => m_splitUpdateMode = value;
        }
        #endregion Shared Properties

        #region BaseOrder Shared Properties
        private bool canSpanPlants;

        /// <summary>
        /// If true, then the Operations can schedule in more than one plant.  Otherwise, all operations must be scheduled in only one Plant.
        /// </summary>
        public virtual bool CanSpanPlants
        {
            get => canSpanPlants;
            set
            {
                canSpanPlants = value;
                CanSpanPlantsSet = true;
            }
        }

        public bool CanSpanPlantsSet
        {
            get => bools[c_canSpanPlantsIsSetIdx];
            private set => bools[c_canSpanPlantsIsSetIdx] = value;
        }
        #endregion

        private readonly ArrayList operations = new ();
        private readonly ArrayList alternatePaths = new ();

        [NonSerialized] private Hashtable operationHash = new ();

        public void AddOperation(BaseOperation operation)
        {
            if (operationHash.Contains(operation.ExternalId))
            {
                throw new DuplicateOperationException("2733", new object[] { operation.ExternalId });
            }

            operations.Add(operation);
            operationHash.Add(operation.ExternalId, operation);
        }

        public int OperationCount => operations.Count;

        public BaseOperation GetOperation(int i)
        {
            return (BaseOperation)operations[i];
        }

        /// <summary>
        /// Returns the Operation or null if it doesn't exist.
        /// </summary>
        public InternalOperation GetOperation(string operationExternalId)
        {
            if (operationHash.Contains(operationExternalId))
            {
                return (InternalOperation)operationHash[operationExternalId];
            }

            return null;
        }

        public void AddPath(AlternatePath path)
        {
            ValidatePath(path);
            alternatePaths.Add(path);
        }

        public int PathCount => alternatePaths.Count;

        public AlternatePath GetAlternatePath(int i)
        {
            return (AlternatePath)alternatePaths[i];
        }

        private void CreateOperationHash()
        {
            operationHash = new Hashtable();

            for (int i = 0; i < operations.Count; ++i)
            {
                BaseOperation operation = (BaseOperation)operations[i];
                if (operationHash.Contains(operation.ExternalId))
                {
                    throw new ValidationException("2098", new object[] { operation.ExternalId, ExternalId });
                }

                operationHash.Add(operation.ExternalId, operation);
            }
        }

        private void ValidatePath(AlternatePath path)
        {
            if (path.Count == 0)
            {
                throw new ValidationException("2099", new object[] { path.ExternalId, ExternalId });
            }

            for (int i = 0; i < path.Count; i++)
            {
                AlternateNode n = path[i];
                if (!operationHash.Contains(n.OperationExternalId))
                {
                    throw new ValidationException("2100", new object[] { n.OperationExternalId, ExternalId });
                }
            }

            if (path.Cirularities())
            {
                throw new ValidationException("2101", new object[] { path.ExternalId });
            }
        }

        public override void Validate()
        {
            //No need to validate the routing if it will be copied from a Template.
            if (CopyRoutingFromTemplate)
            {
                return;
            }

            CreateOperationHash();

            base.Validate();

            if (operations.Count == 0)
            {
                throw new ValidationException("2102", new object[] { ExternalId });
            }

            if (PathCount == 0)
            {
                throw new ValidationException("2103", new object[] { ExternalId });
            }

            if (requiredQty <= 0)
            {
                throw new ValidationException("2104", new object[] { ExternalId });
            }

            SortedList<string, BaseOperation> sortedOps = new ();
            for (int opI = 0; opI < OperationCount; ++opI)
            {
                BaseOperation op = GetOperation(opI);
                sortedOps.Add(op.ExternalId, op);
                op.ResetPredecessorOperationAttributesSortedList();
            }

            ValidationException.ValidateArrayList(alternatePaths);

            Hashtable pathsValidationHash = new ();
            for (int pathI = 0; pathI < PathCount; ++pathI)
            {
                AlternatePath path = GetAlternatePath(pathI);

                try
                {
                    ValidatePath(path);
                }
                catch (ValidationException e)
                {
                    throw new ValidationException("2105", new object[] { ExternalId, e.Message });
                }

                if (pathsValidationHash.ContainsKey(path.ExternalId))
                {
                    throw new ValidationException("2106", new object[] { ExternalId, path.ExternalId });
                }

                pathsValidationHash.Add(path.ExternalId, null);

                // Verify that each Successor/Predecessor relationship has a set of PredecessorOperationAttributes
                for (int alternateNodeI = 0; alternateNodeI < path.Count; alternateNodeI++)
                {
                    AlternateNode alternateNode = path[alternateNodeI];
                    for (int successorI = 0; successorI < alternateNode.Count; ++successorI)
                    {
                        string predecessorId = alternateNode.OperationExternalId;
                        string successorId = alternateNode[successorI].OperationExternalId;
                        if (!sortedOps.ContainsKey(successorId))
                        {
                            ThrowExceptionForOperationDoesntExist(successorId);
                        }

                        if (!sortedOps.ContainsKey(predecessorId))
                        {
                            ThrowExceptionForOperationDoesntExist(predecessorId);
                        }
                    }
                }
            }
            
            //Validate all Operations are mapped to one and only one AlternatePath
            foreach (DictionaryEntry de in operationHash)
            {
                int count = 0;
                if (de.Key is string opExternalId)
                {
                    for (int pathI = 0; pathI < PathCount; ++pathI)
                    {
                        AlternatePath path = GetAlternatePath(pathI);
                        if (path.ContainsNode(opExternalId))
                        {
                            if (count > 0)
                            {
                                //op mapped in more than one path
                                throw new APSCommon.PTValidationException("3005", new object[] { opExternalId });
                            }

                            count++;
                        }
                    }

                    if (count == 0)
                    {
                        //op not mapped in any path
                        throw new APSCommon.PTValidationException("3006", new object[] { opExternalId });
                    }
                }
            }
           
        }

        public void ValidateSuccessorMOs(string a_jobExternalId)
        {
            for (int i = 0; i < m_successorMOs.Count; i++)
            {
                if (a_jobExternalId == m_successorMOs[i].SuccessorJobExternalId && ExternalId == m_successorMOs[i].ExternalId)
                {
                    throw new ValidationException("2867", new object[] { a_jobExternalId, ExternalId }, false);
                }
            }
        }

        private void ThrowExceptionForOperationDoesntExist(string operationId)
        {
            throw new ValidationException("2107", new object[] { operationId, ExternalId });
        }

        private readonly SuccessorMOArrayList m_successorMOs = new ();

        /// <summary>
        /// Specification of the MOs that this MO supplies material to. The start time of these successor MOs will be constrained by completion of material in this MO.
        /// </summary>
        public SuccessorMOArrayList SuccessorMOs => m_successorMOs;

        public void AddSuccessorMO(SuccessorMO mo)
        {
            m_successorMOs.Add(mo);
        }
    }
}