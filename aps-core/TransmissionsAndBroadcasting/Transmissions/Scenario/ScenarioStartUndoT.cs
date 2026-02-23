using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

public class ScenarioStartUndoT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 141;
    
    #region IPTSerializable Members
    public ScenarioStartUndoT(IReader a_reader)
        : base(a_reader)
    {
        m_bools = new BoolVector32(a_reader);
        a_reader.Read(out m_undosCount);
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_bools.Serialize(a_writer);
        a_writer.Write(m_undosCount);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioStartUndoT() { }
    public ScenarioStartUndoT(BaseId id) : base(id) { }
    #region Bool Vectors
    private BoolVector32 m_bools;
    private const int c_isUndoIdx = 0;
    private const int c_isCancellingSimulationIdx = 1;

    public bool IsCancellingSimulation
    {
        get => m_bools[c_isCancellingSimulationIdx];
        set => m_bools[c_isCancellingSimulationIdx] = value;
    }
    public bool IsUndo
    {
        get => m_bools[c_isUndoIdx];
        set => m_bools[c_isUndoIdx] = value;
    }
    #endregion
    private int m_undosCount;
    public int Count
    {
        get => m_undosCount;
        set => m_undosCount = value;
    }

    public override string Description
    {
        get
        {
            string action = m_undosCount > 1 ? "actions" : "action";
            
            return IsUndo? string.Format("Undoing {0} {1}".Localize(), m_undosCount, action.Localize()) : string.Format("Redoing {0} {1}".Localize(), m_undosCount, action.Localize()) ;
        }
    }
}