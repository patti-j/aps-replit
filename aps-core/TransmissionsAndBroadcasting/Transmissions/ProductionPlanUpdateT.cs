using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Updates the production plans for one or more Resources.
/// </summary>
public class ProductionPlanUpdateT : ScenarioIdBaseT, IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 625;

    public ProductionPlanUpdateT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ProductionPlanUpdateT() { }

    public ProductionPlanUpdateT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "Production Plan updated";

    private readonly List<ResourceProductPlan> productPlan = new ();

    public List<ResourceProductPlan> ProductPlans => productPlan;

    /// <summary>
    /// Specifies product plans for one Resource.
    /// This does NOT specify all intervals, just those that have changed and should be updated.
    /// </summary>
    public class ResourceProductPlan
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 628;

        public ResourceProductPlan(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                plantId = new BaseId(reader);
                departmentId = new BaseId(reader);
                resourceId = new BaseId(reader);

                int count;
                reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    ProductPlan productPlan = new (reader);
                    productPlans.Add(productPlan);
                }
            }
        }

        public void Serialize(IWriter writer)
        {
            plantId.Serialize(writer);
            departmentId.Serialize(writer);
            resourceId.Serialize(writer);

            writer.Write(productPlans.Count);
            for (int i = 0; i < productPlans.Count; i++)
            {
                productPlans[i].Serialize(writer);
            }
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public ResourceProductPlan(BaseId aPlantId, BaseId aDeptId, BaseId aResourceId)
        {
            plantId = aPlantId;
            departmentId = aDeptId;
            resourceId = aResourceId;
        }

        public BaseId plantId;
        public BaseId departmentId;
        public BaseId resourceId;

        private readonly List<ProductPlan> productPlans = new ();

        public List<ProductPlan> ProductPlans => productPlans;

        /// <summary>
        /// Specifies the qties to be produced for one product on this resource.
        /// </summary>
        public class ProductPlan
        {
            #region IPTSerializable Members
            public const int UNIQUE_ID = 626;

            public ProductPlan(IReader reader)
            {
                if (reader.VersionNumber >= 1)
                {
                    reader.Read(out productName);
                    int count;
                    reader.Read(out count);
                    for (int i = 0; i < count; i++)
                    {
                        ProductPlanInterval interval = new (reader);
                        intervals.Add(interval);
                    }
                }
            }

            public void Serialize(IWriter writer)
            {
                writer.Write(productName);
                writer.Write(intervals.Count);
                for (int i = 0; i < intervals.Count; i++)
                {
                    intervals[i].Serialize(writer);
                }
            }

            public int UniqueId => UNIQUE_ID;
            #endregion

            public ProductPlan(string aProductName)
            {
                productName = aProductName;
            }

            private readonly string productName;

            /// <summary>
            /// The Product Name (from the Manufacturing Order) being produced.
            /// </summary>
            public string ProductName => productName;

            private readonly List<ProductPlanInterval> intervals = new ();

            public List<ProductPlanInterval> Intervals => intervals;

            public class ProductPlanInterval
            {
                #region IPTSerializable Members
                public const int UNIQUE_ID = 627;

                public ProductPlanInterval(IReader reader)
                {
                    if (reader.VersionNumber >= 1)
                    {
                        reader.Read(out rangeStart);
                        reader.Read(out rangeEnd);
                        reader.Read(out changeQty);
                    }
                }

                public void Serialize(IWriter writer)
                {
                    writer.Write(rangeStart);
                    writer.Write(rangeEnd);
                    writer.Write(changeQty);
                }

                public int UniqueId => UNIQUE_ID;
                #endregion

                public ProductPlanInterval(decimal aChangeQty, DateTime aRangeStart, DateTime aRangeEnd)
                {
                    rangeStart = aRangeStart;
                    rangeEnd = aRangeEnd;
                    changeQty = aChangeQty;
                }

                private readonly DateTime rangeStart;

                /// <summary>
                /// The beginning of the interval.
                /// </summary>
                public DateTime RangeStart => rangeStart;

                private readonly DateTime rangeEnd;

                /// <summary>
                /// The end of the interval.
                /// </summary>
                public DateTime RangeEnd => rangeEnd;

                private readonly decimal changeQty;

                /// <summary>
                /// The amount by which the qty produced changed in the interval.
                /// </summary>
                public decimal ChangeQty => changeQty;
            }
        }
    }
}