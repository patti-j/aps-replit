using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

/// <summary>
/// Delete a Scenario.
/// </summary>
public class ScenarioDeleteT : ScenarioBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 123;

    #region IPTSerializable Members
    public ScenarioDeleteT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 13012)
        {
            scenarioId = new BaseId(reader);
            OriginalInstigatorId = new BaseId(reader);
        } 
        else if (reader.VersionNumber >= 1)
        {
            scenarioId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        scenarioId.Serialize(writer);
        OriginalInstigatorId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId scenarioId;

    public ScenarioDeleteT() { }

    public ScenarioDeleteT(BaseId scenarioId)
    {
        this.scenarioId = scenarioId;
    }
    public BaseId OriginalInstigatorId { get; set; } = BaseId.NULL_ID;
    public override string Description => "Scenario Deleted".Localize();
}