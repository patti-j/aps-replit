using PT.SchedulerDefinitions;

namespace PT.ERPTransmissions;

/// <summary>
/// Self-contained representation of InternalActivity. Meaning it specifies the Job, Mo and Operation
/// ExternalIds to identify itself and doesn't need to be part of an Operation's Activities Manager.
/// This is used in ActivityUpdateT to report progress against Activities without importing the whole Job.
/// </summary>
public class Activity : JobT.InternalActivity, IPTSerializable
{
    public new const int UNIQUE_ID = 799;

    public Activity(JobDataSet.ActivityRow a_row) :
        base(a_row)
    {
        m_jobExternalId = a_row.JobExternalId;
        m_moExternalId = a_row.MoExternalId;
        m_opExternalId = a_row.OpExternalId;
    }

    public Activity(IReader a_reader)
        : base(a_reader)
    {
        a_reader.Read(out m_jobExternalId);
        a_reader.Read(out m_moExternalId);
        a_reader.Read(out m_opExternalId);
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(m_jobExternalId);
        a_writer.Write(m_moExternalId);
        a_writer.Write(m_opExternalId);
    }

    private readonly string m_jobExternalId;

    public string JobExternalId => m_jobExternalId;

    private readonly string m_moExternalId;

    public string MoExternalId => m_moExternalId;

    private readonly string m_opExternalId;

    public string OpExternalId => m_opExternalId;

    /// <summary>
    /// key to uniquely identify Activity (JobExternalid:MoExternalId:OpExternalId:ActivityExternalId)
    /// </summary>
    public string GetUniqueKey()
    {
        return GetUniqueKey(JobExternalId, MoExternalId, OpExternalId, ExternalId);
    }

    public override void Validate()
    {
        base.Validate();

        string missingIdType = null;
        if (string.IsNullOrEmpty(JobExternalId))
        {
            missingIdType = "Job ExternalId";
        }
        else if (string.IsNullOrEmpty(MoExternalId))
        {
            missingIdType = "ManufacturingOrder ExternalId";
        }
        else if (string.IsNullOrEmpty(OpExternalId))
        {
            missingIdType = "Operation ExternalId";
        }

        if (missingIdType != null)
        {
            throw new APSCommon.PTValidationException("4121", new object[] { ExternalId, missingIdType });
        }
    }

    public override string ToString()
    {
        return string.Format("JobExternalId '{0}'; MoExternalId '{1}'; OpExternalId '{2}'; ExternalId '{3}'", JobExternalId, MoExternalId, OpExternalId, ExternalId);
    }

    /// <summary>
    /// returns JobExternalid:MoExternalId:OpExternalId:ActivityExternalId which should identify Activity uniquely
    /// </summary>
    /// <param name="a_row"></param>
    /// <returns></returns>
    public static string GetUniqueKey(JobDataSet.ActivityRow a_row)
    {
        return GetUniqueKey(a_row.JobExternalId, a_row.MoExternalId, a_row.OpExternalId, a_row.ExternalId);
    }

    /// <summary>
    /// returns JobExternalid:MoExternalId:OpExternalId:ActivityExternalId which should identify Activity uniquely
    /// </summary>
    /// <param name="a_jobExternalId"></param>
    /// <param name="a_moExternalId"></param>
    /// <param name="a_opExternalId"></param>
    /// <param name="a_actExternalId"></param>
    /// <returns></returns>
    public static string GetUniqueKey(string a_jobExternalId, string a_moExternalId, string a_opExternalId, string a_actExternalId)
    {
        return string.Join(":", a_jobExternalId, a_moExternalId, a_opExternalId, a_actExternalId);
    }
}

/// <summary>
/// ERP Transmission used to report progress against Activities. Use this when not importing corresponding
/// Job.
/// </summary>
public class ActivityUpdateT : ERPMaintenanceTransmission<Activity>, IPTSerializable
{
    public new const int UNIQUE_ID = 798;

    public ActivityUpdateT() { }

    public ActivityUpdateT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 501)
        {
            int count;
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                Activity act = new (a_reader);
                Add(act);
            }
        }
    }

    public ActivityUpdateT(JobDataSet.ActivityDataTable a_actTable)
    {
        Fill(a_actTable);
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(a_writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;

    /// <summary>
    /// Create Activity from JobDataSet.ActivityRow and add it to this transmission
    /// </summary>
    /// <param name="a_row"></param>
    public void Add(JobDataSet.ActivityRow a_row)
    {
        Add(new Activity(a_row));
    }

    /// <summary>
    /// Add each row in table to this transmission
    /// </summary>
    /// <param name="a_actTable"></param>
    public void Fill(JobDataSet.ActivityDataTable a_actTable)
    {
        foreach (JobDataSet.ActivityRow row in a_actTable.Rows)
        {
            Add(row);
        }
    }

    /// <summary>
    /// Not calling base since it checks for duplicate ExternalIds where it should check for UniqueKey.
    /// </summary>
    public override void Validate()
    {
        HashSet<string> hash = new ();

        foreach (Activity act in Nodes)
        {
            act.Validate();

            string key = act.GetUniqueKey();
            if (hash.Contains(key))
            {
                throw new APSCommon.PTValidationException("4129", new object[] { act.ToString() });
            }

            hash.Add(key);
        }
    }
}