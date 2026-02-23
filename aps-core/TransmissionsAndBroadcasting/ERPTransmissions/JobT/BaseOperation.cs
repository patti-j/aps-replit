using System.Collections;

using PT.Common.Collections;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

public partial class JobT
{
    public abstract class BaseOperation : PTObjectBase, IPTSerializable
    {
        public new const int UNIQUE_ID = 240;

        #region PT Serialization
        protected BaseOperation(IReader a_reader)
            : base(a_reader)
        {
            if (a_reader.VersionNumber >= 12511)
            {
                a_reader.Read(out holdUntilDate);
                a_reader.Read(out holdReason);
                a_reader.Read(out int val);
                omitted = (BaseOperationDefs.omitStatuses)val;
                a_reader.Read(out onHold);
                a_reader.Read(out isRework);
                m_bools = new BoolVector32(a_reader);
                m_isSetBools = new BoolVector32(a_reader);
                a_reader.Read(out planningScrapPercent);
                a_reader.Read(out requiredFinishQty);
                a_reader.Read(out uom);
                a_reader.Read(out outputName);
                a_reader.Read(out m_commitStartDate);
                a_reader.Read(out m_commitEndDate);

                a_reader.Read(out int matlCount);
                for (int m = 0; m < matlCount; m++)
                {
                    MaterialRequirement material = new(a_reader);
                    materialRequirements.Add(material);
                }

                a_reader.Read(out int prodCount);
                for (int p = 0; p < prodCount; p++)
                {
                    Product product = new Product(a_reader);
                    products.Add(product);
                }

                a_reader.Read(out int predCount);
                for (int p = 0; p < predCount; p++)
                {
                    PredecessorOperationAttributes pred = new(a_reader);
                    Add(pred);
                }

                m_productionInfoFlags = new BaseOperationProductionInfoFlags(a_reader);
                a_reader.Read(out m_minOperationBufferTicks);
                a_reader.Read(out m_plannedScrapQty);
                a_reader.Read(out m_nowFinishUtcTime);
                a_reader.Read(out m_productCode);
            }
            else if (a_reader.VersionNumber >= 12404)
            {
                a_reader.Read(out holdUntilDate);
                a_reader.Read(out holdReason);
                a_reader.Read(out int val);
                omitted = (BaseOperationDefs.omitStatuses)val;
                a_reader.Read(out onHold);
                a_reader.Read(out isRework);
                m_bools = new BoolVector32(a_reader);
                m_isSetBools = new BoolVector32(a_reader);
                a_reader.Read(out planningScrapPercent);
                a_reader.Read(out requiredFinishQty);
                a_reader.Read(out uom);
                a_reader.Read(out outputName);
                a_reader.Read(out m_commitStartDate);
                a_reader.Read(out m_commitEndDate);

                a_reader.Read(out int matlCount);
                for (int m = 0; m < matlCount; m++)
                {
                    MaterialRequirement material = new(a_reader);
                    materialRequirements.Add(material);
                }

                a_reader.Read(out int prodCount);
                for (int p = 0; p < prodCount; p++)
                {
                    a_reader.Read(out val);
                    Product product = new Product(a_reader);
                    products.Add(product);
                }

                a_reader.Read(out int predCount);
                for (int p = 0; p < predCount; p++)
                {
                    PredecessorOperationAttributes pred = new(a_reader);
                    Add(pred);
                }

                m_productionInfoFlags = new BaseOperationProductionInfoFlags(a_reader);
                a_reader.Read(out m_minOperationBufferTicks);
                a_reader.Read(out m_plannedScrapQty);
                a_reader.Read(out m_nowFinishUtcTime);
                a_reader.Read(out m_productCode);
            }
            else if (a_reader.VersionNumber >= 12300)
            {
                a_reader.Read(out int val); // Use to be ConstraintType, and it's been removed
                a_reader.Read(out holdUntilDate);
                a_reader.Read(out holdReason);
                a_reader.Read(out val);
                omitted = (BaseOperationDefs.omitStatuses)val;
                a_reader.Read(out onHold);
                a_reader.Read(out isRework);
                m_bools = new BoolVector32(a_reader);
                m_isSetBools = new BoolVector32(a_reader);
                a_reader.Read(out planningScrapPercent);
                a_reader.Read(out requiredFinishQty);
                a_reader.Read(out uom);
                a_reader.Read(out outputName);
                a_reader.Read(out m_commitStartDate);
                a_reader.Read(out m_commitEndDate);

                a_reader.Read(out int matlCount);
                for (int m = 0; m < matlCount; m++)
                {
                    MaterialRequirement material = new (a_reader);
                    materialRequirements.Add(material);
                }

                a_reader.Read(out int prodCount);
                for (int p = 0; p < prodCount; p++)
                {
                    a_reader.Read(out val);
                    Product product = new Product(a_reader);
                    products.Add(product);
                }

                a_reader.Read(out int predCount);
                for (int p = 0; p < predCount; p++)
                {
                    PredecessorOperationAttributes pred = new (a_reader);
                    Add(pred);
                }

                m_productionInfoFlags = new BaseOperationProductionInfoFlags(a_reader);
                a_reader.Read(out m_minOperationBufferTicks);
                a_reader.Read(out m_plannedScrapQty);
                a_reader.Read(out m_nowFinishUtcTime);
                a_reader.Read(out m_productCode);
            }
            else if (a_reader.VersionNumber >= 745)
            {
                a_reader.Read(out int val);  // Use to be ConstraintType, and it's been removed
                a_reader.Read(out holdUntilDate);
                a_reader.Read(out holdReason);
                a_reader.Read(out val);
                omitted = (BaseOperationDefs.omitStatuses)val;
                a_reader.Read(out onHold);
                a_reader.Read(out isRework);
                m_bools = new BoolVector32(a_reader);
                m_isSetBools = new BoolVector32(a_reader);
                a_reader.Read(out planningScrapPercent);
                a_reader.Read(out requiredFinishQty);
                a_reader.Read(out uom);
                a_reader.Read(out outputName);
                a_reader.Read(out m_commitStartDate);
                a_reader.Read(out m_commitEndDate);

                a_reader.Read(out int matlCount);
                for (int m = 0; m < matlCount; m++)
                {
                    MaterialRequirement material = new (a_reader);
                    materialRequirements.Add(material);
                }

                a_reader.Read(out int prodCount);
                for (int p = 0; p < prodCount; p++)
                {
                    a_reader.Read(out val);
                    Product product = new Product(a_reader);
                    products.Add(product);
                }

                a_reader.Read(out int predCount);
                for (int p = 0; p < predCount; p++)
                {
                    PredecessorOperationAttributes pred = new (a_reader);
                    Add(pred);
                }

                m_productionInfoFlags = new BaseOperationProductionInfoFlags(a_reader);
                a_reader.Read(out m_minOperationBufferTicks);
                a_reader.Read(out m_plannedScrapQty);
                a_reader.Read(out m_nowFinishUtcTime);
            }
        }

        public override void Serialize(IWriter writer)
        {
            base.Serialize(writer);

            writer.Write(holdUntilDate);
            writer.Write(holdReason);
            writer.Write((int)omitted);
            writer.Write(onHold);
            writer.Write(isRework);
            m_bools.Serialize(writer);
            m_isSetBools.Serialize(writer);
            writer.Write(planningScrapPercent);
            writer.Write(requiredFinishQty);
            writer.Write(uom);
            writer.Write(outputName);
            writer.Write(m_commitStartDate);
            writer.Write(m_commitEndDate);

            writer.Write(materialRequirements.Count);
            for (int m = 0; m < materialRequirements.Count; m++)
            {
                GetMaterialRequirement(m).Serialize(writer);
            }

            writer.Write(products.Count);
            for (int p = 0; p < products.Count; p++)
            {
                Product product = GetProduct(p);
                product.Serialize(writer);
            }

            writer.Write(PredecessorOperationAttributesCount);
            for (int p = 0; p < PredecessorOperationAttributesCount; p++)
            {
                GetPredecessorOperationAttribute(p).Serialize(writer);
            }

            m_productionInfoFlags.Serialize(writer);
            writer.Write(m_minOperationBufferTicks);
            writer.Write(m_plannedScrapQty);
            writer.Write(m_nowFinishUtcTime);
            writer.Write(m_productCode);
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        protected BaseOperation()
        {
            m_nowFinishUtcTime = DateTime.UtcNow;
        }

        #region BoolVector32
        private BoolVector32 m_bools;
        private const int UseExpectedFinishQtyIdx = 0;
        private const int AutoFinishIdx = 1;
        private const int AutoReportProgressIdx = 12;
        private const int PreventSplitsFromIncurringSetupIdx = 3;
        private const int PreventSplitsFromIncurringCleanIdx = 4;
        private const int c_allowSameLotInNonEmptyStorageAreaIdx = 5;

        private BoolVector32 m_isSetBools;

        private const int ConstraintTypeSetIdx = 1;
        private const int HoldUntilDateTimeSetIdx = 2;
        private const int HoldReasonSetIdx = 3;
        private const int OmittedSetIdx = 4;
        private const int OnHoldSetIdx = 5;
        private const int IsReworkSetIdx = 6;
        private const int PlanningScrapPercentSetIdx = 7;
        private const int RequiredFinishQtySetIdx = 8;
        private const int UOMSetIdx = 9;
        private const int UseExpectedFinishQtySetIdx = 10;
        private const int OutputNameSetIdx = 11;
        private const int AutoReportProgressSetIdx = 12;
        private const int AutoFinishSetIdx = 13;
        private const int commitStartDateSetIdx = 14;
        private const int commitEndDateSetIdx = 15;
        private const int c_minOperationBufferTicksIsSetIdx = 16;
        private const int c_plannedScrapQtySetIdx = 17;
        private const int c_preventSplitsFromIncurringSetupSetIdx = 18;
        private const int c_preventSplitsFromIncurringCleanSetIdx = 19;
        private const int c_allowSameLotInNonEmptyStorageAreaSetIdx = 20;

        #endregion

        protected BaseOperation(string externalId, string name, decimal requiredFinishQty)
            :
            base(externalId, name)
        {
            m_nowFinishUtcTime = DateTime.UtcNow;
            RequiredFinishQty = requiredFinishQty;
            ValidateRequiredFinishQty();
        }

        protected BaseOperation(string externalId, string name, decimal requiredFinishQty, string description, string notes, string userFields)
            : base(externalId, name, description, notes, userFields)
        {
            m_nowFinishUtcTime = DateTime.UtcNow;
            RequiredFinishQty = requiredFinishQty;
            ValidateRequiredFinishQty();
        }

        #region Shared Properties
        private long holdUntilDate = PTDateTime.MinDateTime.Ticks;

        /// <summary>
        /// If the Operation is on hold then Activities for this Operation cannot start before this date.
        /// </summary>
        [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
        public DateTime HoldUntilDateTime
        {
            get => new (holdUntilDate);
            set
            {
                holdUntilDate = PTDateTime.GetValidDateTime(value.Ticks);
                m_isSetBools[HoldUntilDateTimeSetIdx] = true;
            }
        }

        public bool HoldUntilDateTimeSet => m_isSetBools[HoldUntilDateTimeSetIdx];

        private string holdReason = "";

        /// <summary>
        /// If the Operation is on hold then this is the reason why.
        /// </summary>
        public string HoldReason
        {
            get => holdReason;
            set
            {
                holdReason = value;
                m_isSetBools[HoldReasonSetIdx] = true;
            }
        }

        public bool HoldReasonSet => m_isSetBools[HoldReasonSetIdx];

        private BaseOperationDefs.omitStatuses omitted = BaseOperationDefs.omitStatuses.NotOmitted;

        /// <summary>
        /// Omitted Operations are treated as if they take zero time.  \nSpecify 'OmittedByUser' to omit the Operation or 'NotOmitted' to include it as normal.
        /// </summary>
        public BaseOperationDefs.omitStatuses Omitted
        {
            get => omitted;
            set
            {
                omitted = value;
                m_isSetBools[OmittedSetIdx] = true;
            }
        }

        public bool OmittedSet => m_isSetBools[OmittedSetIdx];

        private bool onHold;

        /// <summary>
        /// Whether the Operation should not be worked on for some reason until the HoldUntilDate.
        /// </summary>
        public bool OnHold
        {
            get => onHold;
            set
            {
                onHold = value;
                m_isSetBools[OnHoldSetIdx] = true;
            }
        }

        public bool OnHoldSet => m_isSetBools[OnHoldSetIdx];

        private bool isRework;

        public bool IsRework
        {
            get => isRework;
            set
            {
                isRework = value;
                m_isSetBools[IsReworkSetIdx] = true;
            }
        }

        public bool IsReworkSet => m_isSetBools[IsReworkSetIdx];

        private decimal planningScrapPercent;

        /// <summary>
        /// Percent of parts expected to be scrapped.  Used to calculate expectedGoodQty, exptectedScrapQty, and requiredStartQuantity.
        /// </summary>
        public decimal PlanningScrapPercent
        {
            get => planningScrapPercent;
            set
            {
                if (value >= 0 && value < 1)
                {
                    planningScrapPercent = value;
                    m_isSetBools[PlanningScrapPercentSetIdx] = true;
                }
                else
                {
                    throw new ValidationException("2931", new object[] { value });
                }
            }
        }

        public bool PlanningScrapPercentSet => m_isSetBools[PlanningScrapPercentSetIdx];

        private decimal requiredFinishQty;

        /// <summary>
        /// This is the quantity of good product that must be completed at this Operation.
        /// </summary>
        [Required(true)]
        public decimal RequiredFinishQty
        {
            get => requiredFinishQty;
            set
            {
                requiredFinishQty = value;
                m_isSetBools[RequiredFinishQtySetIdx] = true;
            }
        }

        public bool RequiredFinishQtySet => m_isSetBools[RequiredFinishQtySetIdx];

        private string uom = "units";

        public string UOM
        {
            get => uom;
            set
            {
                uom = value;
                m_isSetBools[UOMSetIdx] = true;
            }
        }

        public bool UOMSet => m_isSetBools[UOMSetIdx];

        /// <summary>
        /// Whether the finish quantities of predecessor operations will have an influence on the expected finish quantity of this operation. In the event of
        /// multiple predecessors the most limiting one determines the finish quantity.
        /// </summary>
        public bool UseExpectedFinishQty
        {
            get => m_bools[UseExpectedFinishQtyIdx];

            set
            {
                m_bools[UseExpectedFinishQtyIdx] = value;
                m_isSetBools[UseExpectedFinishQtySetIdx] = true;
            }
        }

        public bool UseExpectedFinishQtySet => m_isSetBools[UseExpectedFinishQtySetIdx];

        private string outputName;

        /// <summary>
        /// The name of the output product produced by this operation. This is useful in cases where several predecessor operations producing the same type of material
        /// supply the same successor operation. The successor will use this field to recognise quantities of identicle materials available. If this field isn't set then the
        /// material will not be grouped with any other predecessor operations. Groups of material are used for things such as calculating the ExpectedFinishQuantity.
        /// </summary>
        public string OutputName
        {
            get => outputName;

            set
            {
                outputName = value;
                m_isSetBools[OutputNameSetIdx] = true;
            }
        }

        public bool OutputNameSet => m_isSetBools[OutputNameSetIdx];

        /// <summary>
        /// If true then when the Clock is advanced the Operation has progress reported according to schedule.
        /// </summary>
        public bool AutoReportProgress
        {
            get => m_bools[AutoReportProgressIdx];
            set
            {
                m_bools[AutoReportProgressIdx] = value;
                m_isSetBools[AutoReportProgressSetIdx] = true;
            }
        }

        public bool AutoReportProgressSet => m_isSetBools[AutoReportProgressSetIdx];

        /// <summary>
        /// If true then when the Clock is advanced past the Operation it is marked as Finished automatically.
        /// </summary>
        public bool AutoFinish
        {
            get => m_bools[AutoFinishIdx];
            set
            {
                m_bools[AutoFinishIdx] = value;
                m_isSetBools[AutoFinishSetIdx] = true;
            }
        }

        public bool AutoFinishSet => m_isSetBools[AutoFinishSetIdx];

        private long m_commitStartDate = PTDateTime.MaxDateTimeTicks;

        /// <summary>
        /// The planned Operation start date to be used to measure schedule conformity.
        /// </summary>
        public DateTime CommitStartDate
        {
            get => new (m_commitStartDate);
            set
            {
                m_commitStartDate = value.Ticks;
                ;
                m_isSetBools[commitStartDateSetIdx] = true;
            }
        }

        public bool CommitStartDateSet => m_isSetBools[commitStartDateSetIdx];

        private long m_commitEndDate = PTDateTime.MaxDateTimeTicks;

        /// <summary>
        /// The planned Operation end date to be used to measure schedule conformity.
        /// </summary>
        public DateTime CommitEndDate
        {
            get => new (m_commitEndDate);
            set
            {
                m_commitEndDate = value.Ticks;
                m_isSetBools[commitEndDateSetIdx] = true;
            }
        }

        public bool CommitEndDateSet => m_isSetBools[commitEndDateSetIdx];

        private long m_minOperationBufferTicks;

        /// <summary>
        /// a buffer or slack to release an Operation earlier per Operation bases.
        /// </summary>
        public long MinOperationBufferTicks
        {
            get => m_minOperationBufferTicks;
            set
            {
                m_minOperationBufferTicks = value;
                m_isSetBools[c_minOperationBufferTicksIsSetIdx] = true;
            }
        }

        public bool MinOperationBufferSet => m_isSetBools[c_minOperationBufferTicksIsSetIdx];

        private decimal m_plannedScrapQty;

        public decimal PlannedScrapQty
        {
            get => m_plannedScrapQty;
            internal set
            {
                m_plannedScrapQty = value;
                m_isSetBools[c_plannedScrapQtySetIdx] = true;
            }
        }

        public bool PlannedScrapQtySet => m_isSetBools[c_plannedScrapQtySetIdx];

        private readonly DateTime m_nowFinishUtcTime;

        /// <summary>
        /// A default current date time for use in setting reported end date if one is not provided
        /// </summary>
        public DateTime NowFinishUtcTime => m_nowFinishUtcTime;

        private string m_productCode = "";

        /// <summary>
        /// Used to restrict Resources to only perform compatible work at simulataneous times. If specified, then any scheduled Operation's CompatibilityCode must match the CompatibilityCode of other Operations
        /// scheduled on Resources with the same CompatibilityGroup.
        /// </summary>
        public string ProductCode
        {
            get => m_productCode;
            internal set => m_productCode = value;
        }

        public bool PreventSplitsFromIncurringClean
        {
            get => m_bools[PreventSplitsFromIncurringCleanIdx];
            set
            {
                m_bools[PreventSplitsFromIncurringCleanIdx] = value;
                m_isSetBools[c_preventSplitsFromIncurringCleanSetIdx] = true;
            }
        }
       
        public bool PreventSplitsFromIncurringSetup
        {
            get => m_bools[PreventSplitsFromIncurringSetupIdx];
            set
            {
                m_bools[PreventSplitsFromIncurringSetupIdx] = value;
                m_isSetBools[c_preventSplitsFromIncurringSetupSetIdx] = true;
            }
        }

        public bool AllowSameLotInNonEmptyStorageArea
        {
            get => m_bools[c_allowSameLotInNonEmptyStorageAreaIdx];
            set
            {
                m_bools[c_allowSameLotInNonEmptyStorageAreaIdx] = value;
                m_isSetBools[c_allowSameLotInNonEmptyStorageAreaSetIdx] = true;
            }
        }

        public bool PreventSplitsFromIncurringSetupIsSet => m_isSetBools[AutoFinishSetIdx];
        public bool PreventSplitsFromIncurringCleanIsSet => m_isSetBools[AutoFinishSetIdx];
        public bool AllowSameLotInNonEmptyStorageAreaIsSet => m_isSetBools[c_allowSameLotInNonEmptyStorageAreaSetIdx];
        #endregion Shared Properties

        private readonly ArrayList materialRequirements = new ();
        private readonly ArrayList products = new ();
        private readonly ArrayList predecessorOperationAttributesArray = new ();
        private readonly Hashtable predecessorOperationAttributesHash = new ();

        private readonly BaseOperationProductionInfoFlags m_productionInfoFlags = new ();

        public BaseOperationProductionInfoFlags ProductionInfoFlagsBaseOperation => m_productionInfoFlags;

        [NonSerialized] internal List<string> m_predecessorOperationAttributesSortedList = new ();

        internal void ResetPredecessorOperationAttributesSortedList()
        {
            m_predecessorOperationAttributesSortedList = new List<string>();

            for (int i = 0; i < PredecessorOperationAttributesCount; ++i)
            {
                PredecessorOperationAttributes atts = GetPredecessorOperationAttribute(i);
                m_predecessorOperationAttributesSortedList.Add(atts.OperationExternalId);
            }
        }

        #region Material Requirements
        private readonly Hashtable materialRequirementsHash = new ();
        private readonly DictionaryCollection<string, string> m_materialRequirementItemDict = new ();

        public int Add(MaterialRequirement materialRequirement)
        {
            //Validate for make to stock mrs.
            if (materialRequirement.ItemExternalId != null)
            {
                int add = materialRequirements.Add(materialRequirement);
                ValidateMRs();
                return add;
            }

            if (materialRequirement.RequirementType == MaterialRequirementDefs.requirementTypes.BuyDirect)
            {
                if (materialRequirementsHash.Contains(materialRequirement.ExternalId))
                {
                    //TODO: Improve message to explain this is a buy direct material
                    throw new APSCommon.PTValidationException("2059", new object[] { ExternalId, materialRequirement.ExternalId, materialRequirement.MaterialName });
                }

                materialRequirementsHash.Add(materialRequirement.ExternalId, null);
                return materialRequirements.Add(materialRequirement);
            }

            return -1;
        }

        public int MaterialRequirementCount => materialRequirements.Count;

        public MaterialRequirement GetMaterialRequirement(int i)
        {
            return (MaterialRequirement)materialRequirements[i];
        }
        #endregion

        #region Products
        private readonly Hashtable m_productsHash = new ();

        public int Add(Product product)
        {
            product.Validate();
            if (m_productsHash.Contains(product.ExternalId))
            {
                throw new APSCommon.PTValidationException("2060", new object[] { ExternalId, product.ExternalId });
            }

            m_productsHash.Add(product.ExternalId, null);

            return products.Add(product);
        }

        public int ProductCount => products.Count;

        public Product GetProduct(int i)
        {
            return (Product)products[i];
        }
        #endregion

        private void Add(PredecessorOperationAttributes predecessorOperationAttributes)
        {
            if (predecessorOperationAttributesHash.Contains(predecessorOperationAttributes.OperationExternalId))
            {
                throw new ValidationException("2061", new object[] { ExternalId, predecessorOperationAttributes.OperationExternalId });
            }

            predecessorOperationAttributesArray.Add(predecessorOperationAttributes);
            predecessorOperationAttributesHash.Add(predecessorOperationAttributes.OperationExternalId, null);
        }

        internal int PredecessorOperationAttributesCount => predecessorOperationAttributesArray.Count;

        internal PredecessorOperationAttributes GetPredecessorOperationAttribute(int i)
        {
            return (PredecessorOperationAttributes)predecessorOperationAttributesArray[i];
        }

        internal void ClearPredecessorOperationAttributes_version_209_fix()
        {
            predecessorOperationAttributesArray.Clear();
        }

        private void ValidateRequiredFinishQty()
        {
            if (RequiredFinishQty <= 0)
            {
                throw new ValidationException("2062", new object[] { ExternalId, RequiredFinishQty });
            }
        }

        private void ValidateMRs()
        {
            Dictionary<string, MaterialRequirement> duplicateTracker = new Dictionary<string, MaterialRequirement>();
            m_materialRequirementItemDict.Clear();
            for (int i = 0; i < materialRequirements.Count; ++i)
            {
                MaterialRequirement mr = (MaterialRequirement)materialRequirements[i];
                if (mr.ItemExternalId != null)
                {
                    string lotCodes = mr.AllowedLotCodes.Any() ? string.Join(",", mr.AllowedLotCodes) : string.Empty;
                    //Validate for make to stock mrs.
                    if (m_materialRequirementItemDict.TryGetValue(mr.ItemExternalId, out List<string> existingItemRequirementLotCodes))
                    {
                        if (existingItemRequirementLotCodes.Contains(lotCodes))
                        {
                            // You can't have multiple material requirements for the same item and lot sourcing. Material overlap won't work if you do this.
                            // TODO: The determination of the earliest time when enough material is available won't be correct since the MRs are checked one by one.
                            throw new APSCommon.PTValidationException("2063", new object[] { ExternalId, mr.ItemExternalId, lotCodes });
                        }
                    }
                    else
                    {
                        m_materialRequirementItemDict.Add(mr.ItemExternalId, lotCodes);
                    }
                }

                if (duplicateTracker.ContainsKey(mr.ExternalId))
                {
                    throw new APSCommon.PTValidationException("3078", new object[] {ExternalId ,mr.ExternalId});
                }

                duplicateTracker.Add(mr.ExternalId, mr);

            }
        }

        public override void Validate()
        {
            base.Validate();
            ValidateRequiredFinishQty();
            ValidateMRs();
        }
    }
}