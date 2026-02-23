using PT.APSCommon;
using PT.SchedulerDefinitions;
using PT.SchedulerDefinitions.Templates.Lists;

namespace PT.Transmissions;

/// <summary>
/// Transmission for Lock  or Unlocking a list of Operations.
/// </summary>
public class ApiActivityUpdateT : ScenarioIdBaseT, IPTSerializable
{
    private readonly List<ActivityUpdate> m_activityUpdates;
    public const int UNIQUE_ID = 841;

    #region IPTSerializable Members
    public ApiActivityUpdateT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            m_activityUpdates = new List<ActivityUpdate>();
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                ActivityUpdate externalIdObject = new (a_reader);
                m_activityUpdates.Add(externalIdObject);
            }
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(m_activityUpdates.Count);
        foreach (ActivityUpdate activityUpdate in m_activityUpdates)
        {
            activityUpdate.Serialize(a_writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ApiActivityUpdateT() { }

    public ApiActivityUpdateT(BaseId a_scenarioId)
        : base(a_scenarioId)
    {
        m_activityUpdates = new List<ActivityUpdate>();
    }

    public List<ActivityUpdate> ActivityUpdates => m_activityUpdates;

    public void Add(ActivityUpdate a_update)
    {
        m_activityUpdates.Add(a_update);
    }

    public class ActivityUpdate : IPTSerializable
    {
        public const int UNIQUE_ID = 843;

        #region IPTSerializable Members
        public ActivityUpdate(IReader a_reader)
        {
            if (a_reader.VersionNumber >= 12532)
            {
                m_ptObjectId = new ExternalIdObject(a_reader);
                m_bools = new BoolVector32(a_reader);

                a_reader.Read(out int productionStatusNum);
                m_productionStatus = (InternalActivityDefs.productionStatuses)productionStatusNum;
                a_reader.Read(out m_reportedGoodQty);
                a_reader.Read(out m_reportedScrapQty);
                a_reader.Read(out m_reportedSetupHrs);
                a_reader.Read(out m_reportedRunHrs);
                a_reader.Read(out m_reportedPostProcessingHrs);
                a_reader.Read(out m_reportedCleanHrs);
                a_reader.Read(out m_reportedCleanOutGrade);
                a_reader.Read(out m_reportedStartDate);
                a_reader.Read(out m_reportedProcessingStartDate);
                a_reader.Read(out m_reportedProcessingEndDate);
                a_reader.Read(out m_reportedFinishDate);
                a_reader.Read(out m_comments);
                a_reader.Read(out m_paused);
                a_reader.Read(out m_onHold);
                a_reader.Read(out m_holdReason);
                a_reader.Read(out m_holdUntil);
            }
            else if (a_reader.VersionNumber >= 12500)
            {
                m_ptObjectId = new ExternalIdObject(a_reader);
                m_bools = new BoolVector32(a_reader);

                a_reader.Read(out int productionStatusNum);
                m_productionStatus = (InternalActivityDefs.productionStatuses)productionStatusNum;
                a_reader.Read(out m_reportedGoodQty);
                a_reader.Read(out m_reportedScrapQty);
                a_reader.Read(out m_reportedSetupHrs);
                a_reader.Read(out m_reportedRunHrs);
                a_reader.Read(out m_reportedPostProcessingHrs);
                a_reader.Read(out m_reportedCleanHrs);
                a_reader.Read(out m_reportedCleanOutGrade);
                a_reader.Read(out m_reportedStartDate);
                a_reader.Read(out m_reportedFinishDate);
                a_reader.Read(out m_comments);
                a_reader.Read(out m_paused);
                a_reader.Read(out m_onHold);
                a_reader.Read(out m_holdReason);
                a_reader.Read(out m_holdUntil);
                a_reader.Read(out int materialIssuesCount);
            }
            else if (a_reader.VersionNumber >= 12439)
            {
                m_ptObjectId = new ExternalIdObject(a_reader);
                m_bools = new BoolVector32(a_reader);

                a_reader.Read(out int productionStatusNum);
                m_productionStatus = (InternalActivityDefs.productionStatuses)productionStatusNum;
                a_reader.Read(out m_reportedGoodQty);
                a_reader.Read(out m_reportedScrapQty);
                a_reader.Read(out m_reportedSetupHrs);
                a_reader.Read(out m_reportedRunHrs);
                a_reader.Read(out m_reportedPostProcessingHrs);
                a_reader.Read(out m_reportedCleanHrs);
                a_reader.Read(out m_reportedCleanOutGrade);
                a_reader.Read(out m_reportedStartDate);
                a_reader.Read(out m_reportedProcessingStartDate);
                a_reader.Read(out m_reportedProcessingEndDate);
                a_reader.Read(out m_reportedFinishDate);
                a_reader.Read(out m_comments);
                a_reader.Read(out m_paused);
                a_reader.Read(out m_onHold);
                a_reader.Read(out m_holdReason);
                a_reader.Read(out m_holdUntil);
            }
            else if (a_reader.VersionNumber >= 12416)
            {
                m_ptObjectId = new ExternalIdObject(a_reader);
                m_bools = new BoolVector32(a_reader);

                a_reader.Read(out int productionStatusNum);
                m_productionStatus = (InternalActivityDefs.productionStatuses)productionStatusNum;
                a_reader.Read(out m_reportedGoodQty);
                a_reader.Read(out m_reportedScrapQty);
                a_reader.Read(out m_reportedSetupHrs);
                a_reader.Read(out m_reportedRunHrs);
                a_reader.Read(out m_reportedPostProcessingHrs);
                a_reader.Read(out m_reportedCleanHrs);
                a_reader.Read(out m_reportedCleanOutGrade);
                a_reader.Read(out m_reportedStartDate);
                a_reader.Read(out m_reportedFinishDate);
                a_reader.Read(out m_comments);
                a_reader.Read(out m_paused);
                a_reader.Read(out m_onHold);
                a_reader.Read(out m_holdReason);
                a_reader.Read(out m_holdUntil);
                a_reader.Read(out int materialIssuesCount);
            }
            else if (a_reader.VersionNumber >= 1)
            {
                m_ptObjectId = new ExternalIdObject(a_reader);
                m_bools = new BoolVector32(a_reader);

                a_reader.Read(out int productionStatusNum);
                m_productionStatus = (InternalActivityDefs.productionStatuses)productionStatusNum;
                a_reader.Read(out m_reportedGoodQty);
                a_reader.Read(out m_reportedScrapQty);
                a_reader.Read(out m_reportedSetupHrs);
                a_reader.Read(out m_reportedRunHrs);
                a_reader.Read(out m_reportedPostProcessingHrs);
                a_reader.Read(out m_reportedStartDate);
                a_reader.Read(out m_reportedFinishDate);
                a_reader.Read(out m_comments);
                a_reader.Read(out m_paused);
                a_reader.Read(out m_onHold);
                a_reader.Read(out m_holdReason);
                a_reader.Read(out m_holdUntil);
                a_reader.Read(out int materialIssuesCount);
            }
        }

        public ActivityUpdate(ExternalIdObject a_externalIdObject)
        {
            m_ptObjectId = a_externalIdObject;
        }

        public void Serialize(IWriter a_writer)
        {
            m_ptObjectId.Serialize(a_writer);
            m_bools.Serialize(a_writer);
            a_writer.Write((int)m_productionStatus);
            a_writer.Write(m_reportedGoodQty);
            a_writer.Write(m_reportedScrapQty);
            a_writer.Write(m_reportedSetupHrs);
            a_writer.Write(m_reportedRunHrs);
            a_writer.Write(m_reportedPostProcessingHrs);
            a_writer.Write(m_reportedCleanHrs);
            a_writer.Write(m_reportedCleanOutGrade);
            a_writer.Write(m_reportedStartDate);
            a_writer.Write(m_reportedProcessingStartDate);
            a_writer.Write(m_reportedProcessingEndDate);
            a_writer.Write(m_reportedFinishDate);
            a_writer.Write(m_comments);
            a_writer.Write(m_paused);
            a_writer.Write(m_onHold);
            a_writer.Write(m_holdReason);
            a_writer.Write(m_holdUntil);
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        private BoolVector32 m_bools;
        private const short c_productionStatusIsSetIdx = 0;
        private const short c_reportedGoodQtyIsSetIdx = 1;
        private const short c_reportedScrapQtyIsSetIdx = 2;
        private const short c_reportedSetupHrsIsSetIdx = 3;
        private const short c_reportedRunHrsIsSetIdx = 4;
        private const short c_reportedPostProcessingHrsIsSetIdx = 5;
        private const short c_reportedStartDateIsSetIdx = 6;
        private const short c_reportedFinishDateIsSetIdx = 7;
        private const short c_commentsIsSetIdx = 8;
        private const short c_pausedIsSetIdx = 9;
        private const short c_holdIsSetIdx = 10;
        private const short c_holdReasonIsSetIdx = 11;
        private const short c_holdUntilIsSetIdx = 12;
        private const short c_reportedCleanSpanIsSetIdx = 13;
        private const short c_reportedCleanoutGradeIsSetIdx = 14;
        private const short c_reportedProcessingStartDateIsSetIdx = 15;
        private const short c_reportedProcessingEndDateIsSetIdx = 16;

        private ExternalIdObject m_ptObjectId;

        public ExternalIdObject PtObjectId
        {
            get => m_ptObjectId;
            set => m_ptObjectId = value;
        }

        private InternalActivityDefs.productionStatuses m_productionStatus;

        public InternalActivityDefs.productionStatuses ProductionStatus
        {
            get => m_productionStatus;
            set
            {
                m_productionStatus = value;
                ProductionStatusIsSet = true;
            }
        }

        public bool ProductionStatusIsSet
        {
            get => m_bools[c_productionStatusIsSetIdx];
            set => m_bools[c_productionStatusIsSetIdx] = value;
        }

        private decimal m_reportedGoodQty;

        public decimal ReportedGoodQty
        {
            get => m_reportedGoodQty;
            set
            {
                m_reportedGoodQty = value;
                ReportedGoodQtyIsSet = true;
            }
        }

        public bool ReportedGoodQtyIsSet
        {
            get => m_bools[c_reportedGoodQtyIsSetIdx];
            private set => m_bools[c_reportedGoodQtyIsSetIdx] = value;
        }

        private decimal m_reportedScrapQty;

        public decimal ReportedScrapQty
        {
            get => m_reportedScrapQty;
            set
            {
                m_reportedScrapQty = value;
                ReportedScrapQtyIsSet = true;
            }
        }

        public bool ReportedScrapQtyIsSet
        {
            get => m_bools[c_reportedScrapQtyIsSetIdx];
            private set => m_bools[c_reportedScrapQtyIsSetIdx] = value;
        }

        private double m_reportedSetupHrs;

        public double ReportedSetupHrs
        {
            get => m_reportedSetupHrs;
            set
            {
                m_reportedSetupHrs = value;
                ReportedSetupHrsIsSet = true;
            }
        }

        public bool ReportedSetupHrsIsSet
        {
            get => m_bools[c_reportedSetupHrsIsSetIdx];
            private set => m_bools[c_reportedSetupHrsIsSetIdx] = value;
        }

        private double m_reportedRunHrs;

        public double ReportedRunHrs
        {
            get => m_reportedRunHrs;
            set
            {
                m_reportedRunHrs = value;
                ReportedRunHrsIsSet = true;
            }
        }

        public bool ReportedRunHrsIsSet
        {
            get => m_bools[c_reportedRunHrsIsSetIdx];
            private set => m_bools[c_reportedRunHrsIsSetIdx] = value;
        }

        private double m_reportedPostProcessingHrs;

        public double ReportedPostProcessingHrs
        {
            get => m_reportedPostProcessingHrs;
            set
            {
                m_reportedPostProcessingHrs = value;
                ReportedPostProcessingHrsIsSet = true;
            }
        }

        public bool ReportedPostProcessingHrsIsSet
        {
            get => m_bools[c_reportedPostProcessingHrsIsSetIdx];
            private set => m_bools[c_reportedPostProcessingHrsIsSetIdx] = value;
        }

        private double m_reportedCleanHrs;
        /// <summary>
        /// Clean time reported to have been spent so far in ticks.
        /// </summary>
        public double ReportedCleanHrs
        {
            get => m_reportedCleanHrs;

            set
            {
                m_reportedCleanHrs = value;
                m_bools[c_reportedCleanSpanIsSetIdx] = true;
            }
        }

        public bool ReportedCleanIsSet => m_bools[c_reportedCleanSpanIsSetIdx];

        private int m_reportedCleanOutGrade;
        public int ReportedCleanoutGrade
        {
            get => m_reportedCleanOutGrade;
            set
            {
                m_reportedCleanOutGrade = value;
                m_bools[c_reportedCleanoutGradeIsSetIdx] = true;
            }
        }

        public bool ReportedCleanGradeIsSet => m_bools[c_reportedCleanoutGradeIsSetIdx];

        private DateTime m_reportedStartDate;

        public DateTime ReportedStartDate
        {
            get => m_reportedStartDate;
            set => m_reportedStartDate = value;
        }

        public bool ReportedStartDateIsSet
        {
            get => m_bools[c_reportedStartDateIsSetIdx];
            private set => m_bools[c_reportedStartDateIsSetIdx] = value;
        }

        private DateTime m_reportedProcessingStartDate;
        public DateTime ReportedProcessingStartDate
        {
            get => m_reportedProcessingStartDate;
            set => m_reportedProcessingStartDate = value;
        }

        public bool ReportedProcessingStartDateIsSet
        {
            get => m_bools[c_reportedProcessingStartDateIsSetIdx];
            private set => m_bools[c_reportedProcessingStartDateIsSetIdx] = value;
        }

        private DateTime m_reportedProcessingEndDate;
        public DateTime ReportedProcessingEndDate
        {
            get => m_reportedProcessingEndDate;
            set => m_reportedProcessingEndDate = value;
        }

        public bool ReportedProcessingEndDateIsSet
        {
            get => m_bools[c_reportedProcessingEndDateIsSetIdx];
            private set => m_bools[c_reportedProcessingEndDateIsSetIdx] = value;
        }


        private DateTime m_reportedFinishDate;

        public DateTime ReportedFinishDate
        {
            get => m_reportedFinishDate;
            set
            {
                m_reportedFinishDate = value;
                ReportedFinishDateIsSet = true;
            }
        }

        public bool ReportedFinishDateIsSet
        {
            get => m_bools[c_reportedFinishDateIsSetIdx];
            private set => m_bools[c_reportedFinishDateIsSetIdx] = value;
        }

        private string m_comments;

        public string Comments
        {
            get => m_comments;
            set
            {
                m_comments = value;
                CommentsIsSet = true;
            }
        }

        public bool CommentsIsSet
        {
            get => m_bools[c_commentsIsSetIdx];
            private set => m_bools[c_commentsIsSetIdx] = value;
        }

        private bool m_paused;

        public bool Paused
        {
            get => m_paused;
            set
            {
                m_paused = value;
                PausedIsSet = true;
            }
        }

        public bool PausedIsSet
        {
            get => m_bools[c_pausedIsSetIdx];
            private set => m_bools[c_pausedIsSetIdx] = value;
        }

        private bool m_onHold;

        public bool OnHold
        {
            get => m_onHold;
            set
            {
                m_onHold = value;
                OnHoldIsSet = true;
            }
        }

        public bool OnHoldIsSet
        {
            get => m_bools[c_holdIsSetIdx];
            private set => m_bools[c_holdIsSetIdx] = value;
        }

        private string m_holdReason;

        public string HoldReason
        {
            get => m_holdReason;
            set
            {
                m_holdReason = value;
                HoldReasonIsSet = true;
            }
        }

        public bool HoldReasonIsSet
        {
            get => m_bools[c_holdReasonIsSetIdx];
            private set => m_bools[c_holdReasonIsSetIdx] = value;
        }

        private DateTime m_holdUntil;

        public DateTime HoldUntil
        {
            get => m_holdUntil;
            set
            {
                m_holdUntil = value;
                HoldUntilIsSet = true;
            }
        }

        public bool HoldUntilIsSet
        {
            get => m_bools[c_holdUntilIsSetIdx];
            private set => m_bools[c_holdUntilIsSetIdx] = value;
        }

        public void SetProductionStatus(string a_productionStatus)
        {
            if (a_productionStatus == "Started")
            {
                ProductionStatus = InternalActivityDefs.productionStatuses.Started;
            }
            else if (a_productionStatus == "Ready")
            {
                ProductionStatus = InternalActivityDefs.productionStatuses.Ready;
            }
            else if (a_productionStatus == "Running")
            {
                ProductionStatus = InternalActivityDefs.productionStatuses.Running;
            }
            else if (a_productionStatus == "PostProcessing")
            {
                ProductionStatus = InternalActivityDefs.productionStatuses.PostProcessing;
            }
            else if (a_productionStatus == "SettingUp")
            {
                ProductionStatus = InternalActivityDefs.productionStatuses.SettingUp;
            }
            else if (a_productionStatus == "Waiting")
            {
                ProductionStatus = InternalActivityDefs.productionStatuses.Waiting;
            }
            else
            {
                ProductionStatus = InternalActivityDefs.productionStatuses.Finished;
            }
        }
    }

    public override string Description => "Jobs Unscheduled";
}