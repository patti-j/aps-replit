using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

public class ScenarioDetailJitCompressT : ScenarioIdBaseT, IPTSerializable
{
    public ScenarioDetailJitCompressT() { }

    public ScenarioDetailJitCompressT(BaseId a_scenarioBaseId)
        : base(a_scenarioBaseId) { }

    #region IPTSerializable Members
    public ScenarioDetailJitCompressT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public const int UNIQUE_ID = 701;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public override string Description => "JIT Compressed".Localize();
}