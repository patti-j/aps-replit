using PT.APSCommon;
using PT.ERPTransmissions;

namespace PT.Transmissions2;

public class ScenarioDetailDepartmentT : DepartmentT, IScenarioIdBaseT, IPTSerializable
{
    private readonly BaseId scenarioId;

    public ScenarioDetailDepartmentT() { }

    public ScenarioDetailDepartmentT(BaseId aScenarioId)
    {
        scenarioId = aScenarioId;
    }

    #region IScenarioIdBaseT Members
    public BaseId ScenarioId => scenarioId;
    #endregion

    #region IPTSerializable Members
    public ScenarioDetailDepartmentT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            scenarioId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        scenarioId.Serialize(writer);
    }

    public new const int UNIQUE_ID = 678;

    public override int UniqueId => UNIQUE_ID;
    #endregion
}