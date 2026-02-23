using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

/// <summary>
/// Creates a new BalancedCompositeDispatcherDefinition in the specified Scenario using default values.
/// </summary>
public class BalancedCompositeDispatcherDefinitionDefaultT : BalancedCompositeDispatcherDefinitionBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 23;

    #region IPTSerializable Members
    public BalancedCompositeDispatcherDefinitionDefaultT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12313)
        {
            a_reader.Read(out m_name);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(m_name);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private readonly string m_name = string.Empty;

    public string Name => m_name;

    public BalancedCompositeDispatcherDefinitionDefaultT() { }
    public BalancedCompositeDispatcherDefinitionDefaultT(BaseId a_scenarioId, string a_name)
        : base(a_scenarioId)
    {
        m_name = a_name;
    }

    public override string Description => "Optimize Rule Created".Localize();
}