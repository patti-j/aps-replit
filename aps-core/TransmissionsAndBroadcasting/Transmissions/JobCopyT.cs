using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Creates a new Job by copying the specified Job.
/// </summary>
public class JobCopyT : JobBaseT
{
    public new const int UNIQUE_ID = 75;

    #region IPTSerializable Members
    public JobCopyT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12024)
        {
            JobIdToCopy = new BaseId(a_reader);

            a_reader.Read(out m_needDateTicks);
            a_reader.Read(out m_commitmentType);
            a_reader.Read(out m_holdDateChangeType);
            a_reader.Read(out m_quantity);
            a_reader.Read(out m_name);
            a_reader.Read(out int numOfCustomers);
            for (int i = 0; i < numOfCustomers; i++)
            {
                a_reader.Read(out string customer);
                m_customers.Add(customer);
            }
        }
        else if (a_reader.VersionNumber >= 12000)
        {
            JobIdToCopy = new BaseId(a_reader);

            a_reader.Read(out m_needDateTicks);
            a_reader.Read(out m_commitmentType);
            a_reader.Read(out m_holdDateChangeType);
            a_reader.Read(out m_quantity);
            a_reader.Read(out m_name);
            a_reader.Read(out string customer);
            m_customers.Add(customer);
        }
        else if (a_reader.VersionNumber >= 492)
        {
            JobIdToCopy = new BaseId(a_reader);

            a_reader.Read(out m_needDateTicks);
            a_reader.Read(out m_commitmentType);
            a_reader.Read(out m_holdDateChangeType);
        }
        else if (a_reader.VersionNumber >= 1)
        {
            JobIdToCopy = new BaseId(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        JobIdToCopy.Serialize(a_writer);

        a_writer.Write(m_needDateTicks);
        a_writer.Write(m_commitmentType);
        a_writer.Write(m_holdDateChangeType);
        a_writer.Write(m_quantity);
        a_writer.Write(m_name);
        a_writer.Write(m_customers.Count);
        foreach (string customer in m_customers)
        {
            a_writer.Write(customer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public JobCopyT() { }

    public JobCopyT(BaseId a_scenarioId, BaseId a_originalId)
        : base(a_scenarioId)
    {
        JobIdToCopy = a_originalId;
    }

    public BaseId JobIdToCopy; //Id of the Job to copy.

    private long m_needDateTicks;

    public DateTime NewNeedDate
    {
        get => new (m_needDateTicks);
        set => m_needDateTicks = value.Ticks;
    }

    private short m_commitmentType;

    public SchedulerDefinitions.JobDefs.commitmentTypes NewCommitment
    {
        get => (SchedulerDefinitions.JobDefs.commitmentTypes)m_commitmentType;
        set => m_commitmentType = (short)value;
    }

    public enum EHoldDateChangeType { Keep, Remove, AdjustBasedOnNeedDate }

    private short m_holdDateChangeType;

    public EHoldDateChangeType HoldDateChangeType
    {
        get => (EHoldDateChangeType)m_holdDateChangeType;
        set => m_holdDateChangeType = (short)value;
    }

    private decimal m_quantity;

    public decimal Quantity
    {
        get => m_quantity;
        set => m_quantity = value;
    }

    private string m_name;

    public string NewName
    {
        get => m_name;
        set => m_name = value;
    }

    private List<string> m_customers = new ();

    public List<string> Customers
    {
        get => m_customers;
        set => m_customers = value;
    }

    public override string Description => "Job copied";
}