using PT.APSCommon.Extensions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

public class PlantT : ERPMaintenanceTransmission<PlantT.Plant>, IPTSerializable
{
    public new const int UNIQUE_ID = 255;

    #region PT Serialization
    public PlantT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                Plant node = new (reader);
                Add(node);
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

    public PlantT() { }

    public class Plant : PTObjectBase, IPTSerializable
    {
        public new const int UNIQUE_ID = 256;

        #region PT Serialization
        public Plant() { } // reqd. for xml serialization

        public Plant(IReader reader)
            : base(reader)
        {
            if (reader.VersionNumber >= 12054)
            {
                reader.Read(out bottleneckThreshold);
                reader.Read(out heavyLoadThreshold);
                reader.Read(out stableSpan);
                reader.Read(out m_dailyOperatingExpense);
                reader.Read(out m_investedCapital);
                reader.Read(out m_annualPercentageRate);

                //Was set flags
                reader.Read(out bottleneckThresholdSet);
                reader.Read(out heavyLoadThresholdSet);
                reader.Read(out stableSpanSet);
                reader.Read(out m_dailyOperatingExpenseSet);
                reader.Read(out m_investedCapitalSet);
                reader.Read(out m_annualPercentageRateSet);
                //End was set flags
            }
            else if (reader.VersionNumber >= 479)
            {
                reader.Read(out bottleneckThreshold);
                reader.Read(out heavyLoadThreshold);
                // Discard sort and related variables since we no longer use them
                reader.Read(out int sort);
                reader.Read(out stableSpan);
                reader.Read(out m_dailyOperatingExpense);
                reader.Read(out m_investedCapital);
                reader.Read(out m_annualPercentageRate);

                //Was set flags
                reader.Read(out bottleneckThresholdSet);
                reader.Read(out heavyLoadThresholdSet);
                // Discard sort and related variables since we no longer use them
                reader.Read(out bool sortSet);
                reader.Read(out stableSpanSet);
                reader.Read(out m_dailyOperatingExpenseSet);
                reader.Read(out m_investedCapitalSet);
                reader.Read(out m_annualPercentageRateSet);
                //End was set flags
            }

            #region 412
            else if (reader.VersionNumber >= 412)
            {
                reader.Read(out bottleneckThreshold);
                reader.Read(out heavyLoadThreshold);
                // Discard sort and related variables since we no longer use them
                reader.Read(out int sort);
                reader.Read(out stableSpan);
                reader.Read(out m_dailyOperatingExpense);
                reader.Read(out m_investedCapital);

                //Was set flags
                reader.Read(out bottleneckThresholdSet);
                reader.Read(out heavyLoadThresholdSet);
                // Discard sort and related variables since we no longer use them
                reader.Read(out bool sortSet);
                reader.Read(out stableSpanSet);
                reader.Read(out m_dailyOperatingExpenseSet);
                reader.Read(out m_investedCapitalSet);
                //End was set flags
            }
            #endregion

            #region Version 358
            else if (reader.VersionNumber >= 358)
            {
                reader.Read(out bottleneckThreshold);
                reader.Read(out heavyLoadThreshold);
                // Discard sort and related variables since we no longer use them
                reader.Read(out int sort);
                reader.Read(out stableSpan);

                //Was set flags
                reader.Read(out bottleneckThresholdSet);
                reader.Read(out heavyLoadThresholdSet);
                // Discard sort and related variables since we no longer use them
                reader.Read(out bool sortSet);
                reader.Read(out stableSpanSet);
                //End was set flags
            }
            #endregion

            #region Version 188
            else if (reader.VersionNumber >= 188)
            {
                reader.Read(out bottleneckThreshold);
                reader.Read(out heavyLoadThreshold);
                reader.Read(out int sort);

                //Was set flags
                reader.Read(out bottleneckThresholdSet);
                reader.Read(out heavyLoadThresholdSet);
                reader.Read(out bool sortSet);
                //End was set flags
            }
            #endregion

            #region Version 1
            else if (reader.VersionNumber >= 1)
            {
                reader.Read(out bottleneckThreshold);
                reader.Read(out heavyLoadThreshold);

                //Was set flags
                reader.Read(out bottleneckThresholdSet);
                reader.Read(out heavyLoadThresholdSet);
                //End was set flags
            }
            #endregion
        }

        public override void Serialize(IWriter writer)
        {
            base.Serialize(writer);

            writer.Write(bottleneckThreshold);
            writer.Write(heavyLoadThreshold);
            writer.Write(stableSpan);
            writer.Write(m_dailyOperatingExpense);
            writer.Write(m_investedCapital);
            writer.Write(m_annualPercentageRate);

            //Was set flags
            writer.Write(bottleneckThresholdSet);
            writer.Write(heavyLoadThresholdSet);
            writer.Write(stableSpanSet);
            writer.Write(m_dailyOperatingExpenseSet);
            writer.Write(m_investedCapitalSet);
            writer.Write(m_annualPercentageRateSet);
            //End was set flags
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        #region Shared Properties
        private decimal bottleneckThreshold = 10;

        /// <summary>
        /// If more than this percentage of Activites on a Resource's schedule are Capacity Bottlenecked then the Resource is flagged as a bottleneck.
        /// </summary>
        public decimal BottleneckThreshold
        {
            get => bottleneckThreshold;
            set
            {
                bottleneckThreshold = value;
                bottleneckThresholdSet = true;
            }
        }

        private bool bottleneckThresholdSet;

        public bool BottleneckThresholdSet => bottleneckThresholdSet;

        private decimal heavyLoadThreshold = 80;

        /// <summary>
        /// If a Resource has more than this percentage of its capacity alllocated in the short-term then it is considered 'Heavy Loaded' and is flagged as such.
        /// </summary>
        public decimal HeavyLoadThreshold
        {
            get => heavyLoadThreshold;
            set
            {
                heavyLoadThreshold = value;
                heavyLoadThresholdSet = true;
            }
        }

        private bool heavyLoadThresholdSet;

        public bool HeavyLoadThresholdSet => heavyLoadThresholdSet;

        private TimeSpan stableSpan;

        /// <summary>
        /// The Stable Span starts at the end of the frozen span and can be used to prevent changes in the schedule during Optimize.
        /// </summary>
        public TimeSpan StableSpan
        {
            get => stableSpan;
            set
            {
                stableSpan = value;
                stableSpanSet = true;
            }
        }

        private bool stableSpanSet;

        public bool StableSpanSet => stableSpanSet;

        private decimal m_dailyOperatingExpense;

        /// <summary>
        /// Monetary costs of operating the plant for a day
        /// (Direct and Indirect Labor, SG&A except Commissions, etc.)
        /// Note that Resource hourly costs are also added into the Operating Expense value in the KPIs so they should either be included here or in the Resource costs but not both.
        /// </summary>
        public decimal DailyOperatingExpense
        {
            get => m_dailyOperatingExpense;
            set
            {
                m_dailyOperatingExpense = value;
                m_dailyOperatingExpenseSet = true;
            }
        }

        private bool m_dailyOperatingExpenseSet;

        public bool DailyOperatingExpenseSet => m_dailyOperatingExpenseSet;

        private decimal m_investedCapital;

        /// <summary>
        /// Monetary value encapsulating all moneys invested in the plant.
        /// </summary>
        public decimal InvestedCapital
        {
            get => m_investedCapital;
            set
            {
                m_investedCapital = value;
                m_investedCapitalSet = true;
            }
        }

        private bool m_investedCapitalSet;

        public bool InvestedCapitalSet => m_investedCapitalSet;

        private bool m_annualPercentageRateSet;

        public bool AnnualPercentageRateSet => m_annualPercentageRateSet;

        private decimal m_annualPercentageRate = 10;

        /// <summary>
        /// APR for calculating carring cost.
        /// </summary>
        public decimal AnnualPercentageRate
        {
            get => m_annualPercentageRate;
            set
            {
                m_annualPercentageRate = value;
                m_annualPercentageRateSet = true;
            }
        }
        #endregion Shared Properties

        public Plant(string externalId, string name, string description, string notes, string userFields)
            : base(externalId, name, description, notes, userFields) { }

        public Plant(PtImportDataSet.PlantsRow aRow)
            : base(aRow.ExternalId, aRow.IsNameNull() ? null : aRow.Name, aRow.IsDescriptionNull() ? null : aRow.Description, aRow.IsNotesNull() ? null : aRow.Notes, aRow.IsUserFieldsNull() ? null : aRow.UserFields)
        {
            if (!aRow.IsBottleneckThresholdNull())
            {
                BottleneckThreshold = aRow.BottleneckThreshold;
            }

            if (!aRow.IsHeavyLoadThresholdNull())
            {
                HeavyLoadThreshold = aRow.HeavyLoadThreshold;
            }

            if (!aRow.IsStableSpanHrsNull())
            {
                StableSpan = aRow.StableSpanHrs == TimeSpan.MaxValue.TotalHours ? TimeSpan.MaxValue : TimeSpan.FromHours(aRow.StableSpanHrs); //else overflows
            }

            if (!aRow.IsDailyOperatingExpenseNull())
            {
                DailyOperatingExpense = aRow.DailyOperatingExpense;
            }

            if (!aRow.IsInvestedCapitalNull())
            {
                InvestedCapital = aRow.InvestedCapital;
            }

            if (!aRow.IsAnnualPercentageRateNull())
            {
                AnnualPercentageRate = aRow.AnnualPercentageRate;
            }
        }
    }

    public new Plant this[int i] => Nodes[i];

    #region Database Loading
    public void Fill(System.Data.IDbCommand cmd)
    {
        PtImportDataSet.PlantsDataTable table = new ();

        FillTable(table, cmd);
        Fill(table);
    }

    /// <summary>
    /// Fill the transmission with data from the DataSet.
    /// </summary>
    public void Fill(PtImportDataSet.PlantsDataTable aTable)
    {
        for (int i = 0; i < aTable.Count; i++)
        {
            Add(new Plant(aTable[i]));
        }
    }
    #endregion

    public override string Description => string.Format("Plants updated ({0})".Localize(), Count);
}