using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all PTAttribute related transmissions.
/// </summary>
public abstract class PTAttributeIdBaseT : PTAttributeBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 986;

    #region IPTSerializable Members
    public PTAttributeIdBaseT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12303)
        {
            a_reader.Read(out int ptAttributeCount);
            for (int i = 0; i < ptAttributeCount; i++)
            {
                PTAttributeIds.Add(i, new BaseId(a_reader));
            }
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(PTAttributeIds.Count);
        for (int i = 0; i < PTAttributeIds.Count; i++)
        {
            PTAttributeIds[i].Serialize(a_writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public SortedList<int, BaseId> PTAttributeIds = new ();

    protected PTAttributeIdBaseT() { }

    protected PTAttributeIdBaseT(BaseId a_scenarioId, IEnumerable<BaseId> a_udfDefinitionIds)
        : base(a_scenarioId)
    {
        int count = 0;
        foreach (BaseId ptAttributeId in a_udfDefinitionIds)
        {
            PTAttributeIds.Add(count, ptAttributeId);
            count++;
        }
    }
}