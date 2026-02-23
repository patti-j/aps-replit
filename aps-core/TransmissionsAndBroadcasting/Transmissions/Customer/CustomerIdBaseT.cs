using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all Customer related transmissions.
/// </summary>
public abstract class CustomerIdBaseT : CustomerBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 63;

    #region IPTSerializable Members
    public CustomerIdBaseT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12039)
        {
            a_reader.Read(out int customerCount);
            for (int i = 0; i < customerCount; i++)
            {
                CustomerIds.Add(i, new BaseId(a_reader));
            }
        }
        else if (a_reader.VersionNumber >= 1)
        {
            BaseId customerId = new (a_reader);
            CustomerIds.Add(0, customerId);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(CustomerIds.Count);
        for (int i = 0; i < CustomerIds.Count; i++)
        {
            CustomerIds[i].Serialize(a_writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public SortedList<int, BaseId> CustomerIds = new ();

    protected CustomerIdBaseT() { }

    protected CustomerIdBaseT(BaseId a_scenarioId, IEnumerable<BaseId> a_customerIds)
        : base(a_scenarioId)
    {
        int count = 0;
        foreach (BaseId customerId in a_customerIds)
        {
            CustomerIds.Add(count, customerId);
            count++;
        }
    }
}