using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all internal PurchaseToStock related transmissions.
/// </summary>
public abstract class MaterialIdBaseT : ScenarioIdBaseT, IPTSerializable
{
    public static readonly int UNIQUE_ID = 833;

    #region IPTSerializable Members
    protected MaterialIdBaseT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12052)
        {
            int count = 0;

            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_jobIds.Add(new BaseId(a_reader));
            }

            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_moIds.Add(new BaseId(a_reader));
            }

            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_operationIds.Add(new BaseId(a_reader));
            }

            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                m_materialIds.Add(new BaseId(a_reader));
            }
        }
        else if (a_reader.VersionNumber >= 1)
        {
            m_jobIds.Add(new BaseId(a_reader));
            m_moIds.Add(new BaseId(a_reader));
            m_operationIds.Add(new BaseId(a_reader));
            m_materialIds.Add(new BaseId(a_reader));
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(m_jobIds);
        a_writer.Write(m_moIds);
        a_writer.Write(m_operationIds);
        a_writer.Write(m_materialIds);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    protected MaterialIdBaseT() { }

    protected MaterialIdBaseT(BaseId a_scenarioId, List<(BaseId a_jobId, BaseId a_moId, BaseId a_opId, BaseId a_materialId)> a_ids)
        : base(a_scenarioId)
    {
        //I don't like tuples, but I didn't want to make a separate class for this.
        foreach ((BaseId jobId, BaseId moId, BaseId opId, BaseId materialId) in a_ids)
        {
            Add(jobId, moId, opId, materialId);
        }
    }

    public void Add(BaseId a_jobId, BaseId a_moId, BaseId a_opId, BaseId a_materialId)
    {
        m_jobIds.Add(a_jobId);
        m_moIds.Add(a_moId);
        m_operationIds.Add(a_opId);
        m_materialIds.Add(a_materialId);
    }

    public List<(BaseId a_jobId, BaseId a_moId, BaseId a_opId, BaseId a_materialId)> GetIdsList()
    {
        List<(BaseId a_jobId, BaseId a_moId, BaseId a_opId, BaseId a_materialId)> idsList = new ();
        for (int i = 0; i < Count; i++)
        {
            (BaseId jobId, BaseId moId, BaseId opId, BaseId materialId) ids = new ();
            ids.jobId = m_jobIds[i];
            ids.moId = m_moIds[i];
            ids.opId = m_operationIds[i];
            ids.materialId = m_materialIds[i];
            idsList.Add(ids);
        }

        return idsList;
    }

    //Since you can only add through the Add function above, the count should be the same for each list.
    public int Count => m_jobIds.Count;

    public void Clear()
    {
        m_jobIds.Clear();
        m_moIds.Clear();
        m_operationIds.Clear();
        m_materialIds.Clear();
    }

    private readonly List<BaseId> m_jobIds = new ();
    private readonly List<BaseId> m_moIds = new ();
    private readonly List<BaseId> m_operationIds = new ();
    private readonly List<BaseId> m_materialIds = new ();
}