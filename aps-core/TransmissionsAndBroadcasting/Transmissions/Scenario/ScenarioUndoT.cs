using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

public class ScenarioUndoT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 140;
    
    #region BoolVector32
    private BoolVector32 m_boolVector;
    private const int c_redoIdx = 0;
    private const int c_cancelSimulationIdx = 1;
    #endregion

    private readonly Id m_initialUndoSetId;

    /// <summary>
    /// The initial undoset index
    /// </summary>
    public Id InitialUndoSetId => m_initialUndoSetId;

    private HashSet<Guid> m_undoIds = new ();

    /// <summary>
    /// Collection of transmission Ids to Undo
    /// </summary>
    public HashSet<Guid> UndoIds => m_undoIds;

    #region IPTSerializable Members
    public ScenarioUndoT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12511)
        {
            m_boolVector = new BoolVector32(a_reader);

            m_initialUndoSetId = new Id(a_reader);
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out Guid undoTransmissionId);
                m_undoIds.Add(undoTransmissionId);
            }
        }
        else if (a_reader.VersionNumber >= 12414)
        {
            m_boolVector = new BoolVector32(a_reader);

            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                if (a_reader.VersionNumber >= 735)
                {
                    a_reader.Read(out bool Play);
                    new Id(a_reader);
                    a_reader.Read(out ulong m_transmissionNbr);
                }
                else if (a_reader.VersionNumber >= 1)
                {
                    a_reader.Read(out int TransmissionIdx);
                    a_reader.Read(out bool Play);

                    new Id(a_reader);
                }
            }
        }
        else if (a_reader.VersionNumber >= 1) 
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                if (a_reader.VersionNumber >= 735)
                {
                    a_reader.Read(out bool Play);
                    new Id(a_reader);
                    a_reader.Read(out ulong m_transmissionNbr);
                }
                else if (a_reader.VersionNumber >= 1)
                {
                    a_reader.Read(out int TransmissionIdx);
                    a_reader.Read(out bool Play);

                    new Id(a_reader);
                }
            }
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_boolVector.Serialize(a_writer);
        m_initialUndoSetId.Serialize(a_writer);
        a_writer.Write(m_undoIds.Count);
        foreach (Guid id in m_undoIds)
        {
            a_writer.Write(id);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioUndoT() { }

    public ScenarioUndoT(BaseId a_scenarioId, int a_initialUndoSetId)
        : this(a_scenarioId, new Id(a_initialUndoSetId)) { }    
    
    public ScenarioUndoT(BaseId a_scenarioId, Id a_initialUndoSetId) : base(a_scenarioId)
    {
        m_initialUndoSetId = a_initialUndoSetId;
    }
    
    /// <summary>
    /// Flag to indicate an undo was a result of a simulation cancelled event.
    /// <para><remarks>True if undo was triggered by a simulation cancellation action</remarks></para>
    /// </summary>
    public bool CancellingSimulation
    {
        get => m_boolVector[c_cancelSimulationIdx];
        set
        {
            m_boolVector[c_cancelSimulationIdx] = value;
        }
    }
    
    /// <summary>
    /// Whether this transmission is to undo or redo actions
    /// </summary>
    public bool Redo
    {
        get => m_boolVector[c_redoIdx];
        set
        {
            m_boolVector[c_redoIdx] = value;
        }
    }

    public override string Description
    {
        get
        {
            if (CancellingSimulation)
            {
                return "Undoing a cancelled action".Localize();
            }

            string action = m_undoIds.Count > 1 ? "actions" : "action";

                return Redo ? 
                    string.Format("Redoing {0} {1}".Localize(), m_undoIds.Count, action.Localize()) : 
                    string.Format("Undoing {0} {1}".Localize(), m_undoIds.Count, action.Localize());
        }
    }
}