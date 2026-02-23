
using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Scheduler.Schedule.Resource.LookupTables;
public class BaseCleanoutTriggerTable : BaseObject
{
    #region IPTSerializable Members
    public BaseCleanoutTriggerTable(IReader a_reader)
        : base(a_reader) { }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    public new const int UNIQUE_ID = 1056;
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Construction
    protected BaseCleanoutTriggerTable(BaseId a_baseId)
        : base(a_baseId) { }
    #endregion

    #region Shared Properties
    public override string DefaultNamePrefix => "Cleanout Trigger";
    #endregion

    #region Transmissions
    internal virtual void UpdateResourcesForDelete(ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        throw new NotImplementedException();
    }
    #endregion

    public class BaseCleanoutTriggerTableRow : IPTSerializable
    {
        #region IPTSerializable Members
        public BaseCleanoutTriggerTableRow(IReader a_reader)
        {
            if (a_reader.VersionNumber >= 12307)
            {
                a_reader.Read(out m_duration);
                a_reader.Read(out m_cleanoutGrade);
                a_reader.Read(out m_cleanCost);
            }
            else if (a_reader.VersionNumber >= 12303)
            {
                a_reader.Read(out m_duration);
                a_reader.Read(out m_cleanoutGrade);
            }
        }

        public virtual void Serialize(IWriter a_writer)
        {
            #if DEBUG
            a_writer.DuplicateErrorCheck(this);
            #endif
            a_writer.Write(m_duration);
            a_writer.Write(m_cleanoutGrade);
            a_writer.Write(m_cleanCost);
        }

        public const int UNIQUE_ID = 1060;
        public int UniqueId => UNIQUE_ID;
        #endregion

        public BaseCleanoutTriggerTableRow(BaseCleanoutTriggerTableRow a_sourceRow)
        {
            m_duration = a_sourceRow.Duration;
            m_cleanoutGrade = a_sourceRow.CleanoutGrade;
            m_cleanCost = a_sourceRow.CleanCost;
        }

        public BaseCleanoutTriggerTableRow(PT.Transmissions.CleanoutTrigger.BaseCleanoutTriggerTable.BaseCleanoutTriggerTableRow a_sourceRow)
        {
            m_duration = a_sourceRow.Duration;
            m_cleanoutGrade = a_sourceRow.CleanoutGrade;
            m_cleanCost = a_sourceRow.CleanCost;
        }

        #region Shared Properties
        protected TimeSpan m_duration;

        public TimeSpan Duration
        {
            get => m_duration;
            set => m_duration = value;
        }

        protected int m_cleanoutGrade;

        public int CleanoutGrade
        {
            get => m_cleanoutGrade;
            set => m_cleanoutGrade = value;
        }

        protected decimal m_cleanCost;

        public decimal CleanCost
        {
            get => m_cleanCost;
            set => m_cleanCost = value;
        }
        #endregion
    }
}