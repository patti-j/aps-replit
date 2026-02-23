using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Creates a new Job in the specified Scenario using default values.
/// </summary>
public class JobDefaultT : JobBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 76;

    #region IPTSerializable Members
    public JobDefaultT(IReader reader)
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

    public JobDefaultT() { }

    public JobDefaultT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "New Job created";
}