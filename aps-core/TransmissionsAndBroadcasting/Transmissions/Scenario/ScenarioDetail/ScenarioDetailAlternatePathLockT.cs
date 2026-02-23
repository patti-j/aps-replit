using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Used to force the MO to be scheduled using the MO's CurrentPath.
/// </summary>
public class ScenarioDetailAlternatePathLockT : ScenarioDetailMOBaseT
{
    #region IPTSerializable Members
    public ScenarioDetailAlternatePathLockT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            m_bools = new BoolVector32(reader);
            m_currentAlternatePathId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        m_bools.Serialize(writer);
        m_currentAlternatePathId.Serialize(writer);
    }

    public const int UNIQUE_ID = 682;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailAlternatePathLockT() { }

    /// <summary>
    /// When you use this constructor, you pass in the id of the Alternate Path you want to the MO locked to.
    /// </summary>
    /// <param name="a_scenarioId">The id of the scenario the job is in.</param>
    /// <param name="a_jobId">The id of the Job whose CurrentPath you want to lock.</param>
    /// <param name="a_moId">The id of the MO whose CurrentPath you want to lock.</param>
    /// <param name="a_lockAlternatePathId">
    /// The current path. This is used to validate the current path is still the same as when the tranmissions was sent. If it's changed between the transmission creation
    /// and processing of this transmission, the tranmission will fail.
    /// </param>
    /// <param name="a_lockUnlock">Whether to lock or unlock the current path.</param>
    public ScenarioDetailAlternatePathLockT(BaseId a_scenarioId, BaseId a_jobId, BaseId a_moId, BaseId a_lockAlternatePathId, bool a_lockUnlock)
        : base(a_scenarioId, a_jobId, a_moId)
    {
        Lock = a_lockUnlock;
        CurrentAlternatePathId = a_lockAlternatePathId;
    }

    private BaseId m_currentAlternatePathId;

    /// <summary>
    /// Used to validate the current alternate path when the lock is made, if the current path doesn't match what's in this tranmission, the lock will fail.
    /// </summary>
    public BaseId CurrentAlternatePathId
    {
        get => m_currentAlternatePathId;
        private set => m_currentAlternatePathId = value;
    }

    public override string Description => "ManufacturingOrder LockedPath updated";

    #region bools
    private BoolVector32 m_bools;

    private const int c_Lock = 0;

    /// <summary>
    /// True if the path is locked, false if the path is to be unlocked.
    /// </summary>
    public bool Lock
    {
        get => m_bools[c_Lock];
        private set => m_bools[c_Lock] = value;
    }
    #endregion
}