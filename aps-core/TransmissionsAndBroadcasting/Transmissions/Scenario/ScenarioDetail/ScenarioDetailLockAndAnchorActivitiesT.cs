using PT.APSCommon;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Transmission for Locking or Unlocking a list of Activitys.
/// </summary>
public class ScenarioDetailLockAndAnchorActivitiesT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 586;

    #region IPTSerializable Members
    public ScenarioDetailLockAndAnchorActivitiesT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out _setLockAndAnchor);
            _activitys = new ActivityKeyList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(_setLockAndAnchor);

        _activitys.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailLockAndAnchorActivitiesT() { }

    public ScenarioDetailLockAndAnchorActivitiesT(BaseId scenarioId, ActivityKeyList activitys, bool aSetLockAndAnchor)
        : base(scenarioId)
    {
        _activitys = activitys;
        _setLockAndAnchor = aSetLockAndAnchor;
    }

    private readonly ActivityKeyList _activitys;

    public ActivityKeyList Activitys => _activitys;

    private readonly bool _setLockAndAnchor;

    public bool SetLockAndAnchor => _setLockAndAnchor;

    public override string Description => "Locked and Anchored Activities";
}