using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes the Cell (and all of its Resources).
/// </summary>
public class CellDeleteT : CellIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 54;

    public bool IsMultiDelete => m_isMultiDelete;

    #region IPTSerializable Members
    private readonly bool m_isMultiDelete;
    private readonly List<BaseId> m_cellIds;
    public List<BaseId> CellsToDeleteIds => m_cellIds;

    public CellDeleteT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12107)
        {
            a_reader.Read(out m_isMultiDelete);
            if (IsMultiDelete)
            {
                m_cellIds = new List<BaseId>();
                a_reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    m_cellIds.Add(new BaseId(a_reader));
                }
            }
        }
        else if (a_reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(IsMultiDelete);
        if (IsMultiDelete)
        {
            writer.Write(m_cellIds.Count);
            foreach (BaseId capabilityId in m_cellIds)
            {
                capabilityId.Serialize(writer);
            }
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public CellDeleteT() { }

    public CellDeleteT(BaseId a_scenarioId, BaseId a_cellId)
        : base(a_scenarioId, a_cellId)
    {
        m_isMultiDelete = false;
    }

    public CellDeleteT(BaseId a_scenarioId, List<BaseId> a_cellIds)
        : base(a_scenarioId, a_cellIds.First())
    {
        m_isMultiDelete = true;
        m_cellIds = a_cellIds;
    }

    public override string Description => "Cell deleted";
}