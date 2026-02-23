using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

/// <summary>
/// Imports a new BalancedCompositeDispatcherDefinition to the specified Scenario.
/// </summary>
public class BalancedCompositeDispatcherDefinitionImportT : BalancedCompositeDispatcherDefinitionBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 311;

    #region IPTSerializable Members
    public BalancedCompositeDispatcherDefinitionImportT(IReader a_reader)
        : base(a_reader)
    {
        a_reader.Read(out m_name);
        a_reader.Read(out m_compressedContainerBytes);
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(m_name);
        a_writer.Write(m_compressedContainerBytes);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion
    private string m_name;
    public string Name
    {
        get => m_name;
        set => m_name = value;
    }
    private readonly byte[] m_compressedContainerBytes;
    public byte[] BalancedCompositeDispatcherDefinitionData => m_compressedContainerBytes;
    public BalancedCompositeDispatcherDefinitionImportT() { }
    public BalancedCompositeDispatcherDefinitionImportT(BaseId a_scenarioId, string a_name, byte[] a_compressedContainerBytes)
        : base(a_scenarioId)
    {
        m_compressedContainerBytes = a_compressedContainerBytes;
        Name = a_name;
    }

    public override string Description => "Sequence Plan Import".Localize();
}