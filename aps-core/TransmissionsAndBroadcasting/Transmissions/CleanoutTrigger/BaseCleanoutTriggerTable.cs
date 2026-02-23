using System.Collections;

namespace PT.Transmissions.CleanoutTrigger;

public class BaseCleanoutTriggerTable : PTObjectBase
{
    #region IPTSerializable Members
    public BaseCleanoutTriggerTable(IReader a_reader)
        : base(a_reader)
    {
        AssignedResources = new Scheduler.ResourceKeyList(a_reader);
        ResourceExternalIdKeyList = new Scheduler.ResourceKeyExternalIdList(a_reader);
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        AssignedResources.Serialize(a_writer);
        ResourceExternalIdKeyList.Serialize(a_writer);
    }

    public new const int UNIQUE_ID = 1064;
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Construction
    public BaseCleanoutTriggerTable() { }
    #endregion

    #region Shared Properties
    public Scheduler.ResourceKeyList AssignedResources = new ();
    public Scheduler.ResourceKeyExternalIdList ResourceExternalIdKeyList = new ();

    public string DefaultNamePrefix => "Cleanout Trigger";
    #endregion

    #region Rows
    // TODO: Since we only enumerate rows on subtypes, and those types work with different row classes/ keys, I don't think we need any inherited methods here.

    //Hashtable m_rows = new Hashtable();

    //public void Add(BaseCleanoutTriggerTableRow row)
    //{
    //    if (!m_rows.Contains(row.GetHashCode()))
    //        m_rows.Add(row.GetHashCode(), row);
    //}

    //public int Count
    //{
    //    get { return m_rows.Count; }
    //}

    //public virtual BaseCleanoutTriggerTableRow GetRow(string previousOpCode, string nextOpCode)
    //{
    //    throw new NotImplementedException();
    //}

    //public IEnumerator<BaseCleanoutTriggerTableRow> GetEnumerator()
    //{
    //    foreach (BaseCleanoutTriggerTableRow row in m_rows.Values)
    //    {
    //        yield return row;
    //    }
    //}

    //IEnumerator IEnumerable.GetEnumerator()
    //{
    //    return GetEnumerator();
    //}
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
            else if (a_reader.VersionNumber >= 12305)
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

        public const int UNIQUE_ID = 1065;
        public int UniqueId => UNIQUE_ID;
        #endregion

        public BaseCleanoutTriggerTableRow(TimeSpan a_duration, int a_cleanoutGrade, decimal a_cleanCost)
        {
            m_duration = a_duration;
            m_cleanoutGrade = a_cleanoutGrade;
            m_cleanCost = a_cleanCost;
        }

        public BaseCleanoutTriggerTableRow(BaseCleanoutTriggerTableRow a_sourceRow)
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