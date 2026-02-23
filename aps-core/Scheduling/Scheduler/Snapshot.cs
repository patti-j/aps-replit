using System.Collections;

using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Contains data to be used for comparing scenarios at different points in time to analyze the impact of changes.
/// </summary>
[Serializable]
public class Snapshot : IPTSerializable
{
    public const int UNIQUE_ID = 346;

    #region IPTSerializable Members
    public Snapshot(IReader reader)
    {
        if (reader.VersionNumber >= 665)
        {
            reader.Read(out m_transmissionNbr);

            jobInfoList = new JobInfoList(reader);
            reader.Read(out m_type);
            reader.Read(out m_creationDateTicks);
        }
        else if (reader.VersionNumber >= 1)
        {
            reader.Read(out m_transmissionNbr);

            jobInfoList = new JobInfoList(reader);
        }
    }

    public void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif
        writer.Write(m_transmissionNbr);

        jobInfoList.Serialize(writer);
        writer.Write(m_type);
        writer.Write(m_creationDateTicks);
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public class SnapshotException : ApplicationException
    {
        public SnapshotException(string msg)
            : base(msg) { }
    }

    public Snapshot(ulong a_lastTransmissionNbr, KpiOptions.ESnapshotType a_type, DateTimeOffset a_creationDateTime)
    {
        m_transmissionNbr = a_lastTransmissionNbr;
        Type = a_type;
        CreationDate = a_creationDateTime.ToDateTime();
    }

    private readonly ulong m_transmissionNbr;

    public ulong TransmissionNbr => m_transmissionNbr;

    private short m_type;

    public KpiOptions.ESnapshotType Type
    {
        get => (KpiOptions.ESnapshotType)m_type;
        set => m_type = (short)value;
    }

    private long m_creationDateTicks;

    public DateTime CreationDate
    {
        get => new (m_creationDateTicks);
        set => m_creationDateTicks = value.Ticks;
    }

    private JobInfoList jobInfoList = new ();

    public JobInfoList JobInfoList => jobInfoList;

    /// <summary>
    /// Compares the Snapshot to another Snapshot and returns a new SnapshotComparison.
    /// </summary>
    /// <param name="beforeSnapshot">A Snapshot created due to an earlier transmission than the current Snapshot.</param>
    internal SnapshotComparison CompareTo(Snapshot beforeSnapshot)
    {
        if (this == beforeSnapshot)
        {
            throw new SnapshotException("Can't compare a Snapshot to itself.");
        }

        SnapshotComparison comparison = new ();
        JobInfoList beforeList = beforeSnapshot.JobInfoList;
        JobInfoList afterList = JobInfoList;

        JobInfo beforeJobInfo;
        JobInfo afterJobInfo;

        beforeList.ResetIndex();
        afterList.ResetIndex();
        beforeJobInfo = beforeList.Next();
        afterJobInfo = afterList.Next();
        while (beforeJobInfo != null || afterJobInfo != null) //Do until both are null
        {
            if (beforeJobInfo != null && afterJobInfo != null) //Have both
            {
                if (beforeJobInfo.Id == afterJobInfo.Id) //Same Job
                {
                    if (beforeJobInfo.Changed(afterJobInfo))
                    {
                        comparison.AddJobInfoComparison(new JobInfoComparison(beforeJobInfo, afterJobInfo));
                    }

                    beforeJobInfo = beforeList.Next();
                    afterJobInfo = afterList.Next();
                }
                else if (afterJobInfo.Id.CompareTo(beforeJobInfo.Id) == 1) //means afterJobInfo.JobId>beforeJobInfo.JobId
                {
                    comparison.AddDeletedJobInfo(beforeJobInfo);
                    beforeJobInfo = beforeList.Next();
                }
                else
                {
                    afterJobInfo = afterList.Next();
                }
            }
            else if (beforeJobInfo != null) //Have only before, so at the end of the afters.
            {
                comparison.AddDeletedJobInfo(beforeJobInfo);
                beforeJobInfo = beforeList.Next();
            }
            else //(afterJobInfo!=null) //Have only after, so at the end of the befores.
            {
                comparison.AddAddedJobInfo(afterJobInfo);
                afterJobInfo = afterList.Next();
            }
        }

        return comparison;
    }

    public string GetDescription()
    {
        return $"{Type.Localize()} : {CreationDate.ToDisplayTime()}";
    }
}

/// <summary>
/// Stores an ArrayList of Snapshots.
/// </summary>
[Serializable]
public class SnapshotList : IPTSerializable
{
    public const int UNIQUE_ID = 350;

    #region IPTSerializable Members
    public SnapshotList(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                Snapshot s = new (reader);
                Add(s);
            }
        }
    }

    public void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif

        writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(writer);
        }
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public SnapshotList() { }

    private ArrayList snapshots = new ();

    public Snapshot Add(Snapshot s)
    {
        snapshots.Add(s);
        return s;
    }

    public void RemoveAt(int index)
    {
        snapshots.RemoveAt(index);
    }

    /// <summary>
    /// Returns the Snapshot that contains the specified transmission.  If not found, returns null.
    /// </summary>
    /// <param name="transmissionNbr"></param>
    public Snapshot Find(ulong a_transmissionNbr)
    {
        for (int i = snapshots.Count - 1; i >= 0; i--)
        {
            Snapshot s = (Snapshot)snapshots[i];
            if (s.TransmissionNbr == a_transmissionNbr)
            {
                return s;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the Snapshot just before the specified transmission.  If not found, returns null.
    /// </summary>
    /// <param name="transmissionNbr"></param>
    public Snapshot FindBefore(ulong transmissionNbr)
    {
        for (int i = snapshots.Count - 1; i >= 0; i--)
        {
            Snapshot s = (Snapshot)snapshots[i];
            if (s.TransmissionNbr == transmissionNbr && i > 0)
            {
                return (Snapshot)snapshots[i - 1];
            }
        }

        return null;
    }

    public Snapshot this[int index] => (Snapshot)snapshots[index];

    public int Count => snapshots.Count;

    public void Clear()
    {
        snapshots.Clear();
    }

    /// <summary>
    /// Whether there are at least two snapshots thus enabling a comparison.
    /// </summary>
    public bool HaveMultipleSnapshotsToCompare => snapshots.Count >= 2;
}