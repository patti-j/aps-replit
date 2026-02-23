using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

/// <summary>
/// Clear Resource Performances for all Resources in the system.
/// </summary>
public class ScenarioDetailClearResourcePerformancesT : PlantBaseT, IPTSerializable
{
    #region IPTSerializable Members
    public new const int UNIQUE_ID = 443;

    public ScenarioDetailClearResourcePerformancesT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailClearResourcePerformancesT() { }

    public ScenarioDetailClearResourcePerformancesT(BaseId scenarioId)
        : base(scenarioId)
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public override string Description => "Resource Performances Cleared".Localize();
}