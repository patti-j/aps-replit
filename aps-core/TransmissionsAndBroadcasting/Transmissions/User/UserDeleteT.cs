using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Transmission for deleting a user. This is a ScenarioBaseT instead of a UserBaseT because
/// we need to route this through the ScenarioManager first to see if the user owns any scenario.
/// If they do, the scenarios need to be deleted, or ownership needs to be re-assigned
/// since a scenario must have an owner that needs to exist in the system.
/// </summary>
public class UserDeleteT : UserIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 157;

    #region IPTSerializable Members
    public UserDeleteT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12206)
        {
            m_bools = new BoolVector32(a_reader);
            m_newOwnerId = new BaseId(a_reader);
            m_userToBeDeleteId = new BaseId(a_reader);
        }
        else if (a_reader.VersionNumber >= 1)
        {
            DeleteScenarios = false;
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_bools.Serialize(a_writer);
        m_newOwnerId.Serialize(a_writer);
        m_userToBeDeleteId.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public UserDeleteT() { }

    public UserDeleteT(BaseId userId)
        : base(userId)
    {
    }

    /// <summary>
    /// Public Constructor for UserDeleteT. If a_deleteScenarios is true, then a_newOwnerId is ignored
    /// </summary>
    public UserDeleteT(BaseId a_userId, bool a_deleteScenarios, BaseId a_newOwnerId) : base(a_userId)
    {
        DeleteScenarios = a_deleteScenarios;
        m_newOwnerId = a_newOwnerId;
        m_userToBeDeleteId = a_userId;
    }

    private BoolVector32 m_bools;

    private const short c_deleteScenariosIdx = 0;

    private readonly BaseId m_newOwnerId; // This user is the new owner of the scenarios owned by the user being deleted

    private readonly BaseId m_userToBeDeleteId; //The user being deleted

    /// <summary>
    /// Indicates whether the scenarios owned by the user being deleted will be
    /// deleted or have their owner reassigned to m_newOwnerId
    /// </summary>
    public bool DeleteScenarios
    {
        get => m_bools[c_deleteScenariosIdx];
        internal set => m_bools[c_deleteScenariosIdx] = value;
    }

    public BaseId NewOwnerId => m_newOwnerId;
    public BaseId UserToBeDelete => m_userToBeDeleteId;

    public override string Description => "A user was deleted";
}