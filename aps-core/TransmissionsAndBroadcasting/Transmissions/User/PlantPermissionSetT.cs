using PT.APSCommon;
using PT.SchedulerDefinitions.PermissionTemplates;

namespace PT.Transmissions;

/// <summary>
/// Create a new User by copying the specified User.
/// </summary>
public class PlantPermissionSetT : UserBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 829;

    public override int UniqueId => UNIQUE_ID;

    #region IPTSerializable Members
    public PlantPermissionSetT(IReader a_reader) : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12000)
        {
            m_bools = new BoolVector32(a_reader);
            m_plantPermissionSet = new PlantPermissionSet(a_reader);
            m_replacementId = new BaseId(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_plantPermissionSet.Serialize(a_writer);
        m_replacementId.Serialize(a_writer);
    }
    #endregion

    private readonly PlantPermissionSet m_plantPermissionSet;
    private BoolVector32 m_bools;
    private const short c_deleteIdx = 0;
    private const short c_defaultIdx = 1;
    private BaseId m_replacementId;

    public PlantPermissionSet PlantPermissionSet => m_plantPermissionSet;

    public bool Delete
    {
        get => m_bools[c_deleteIdx];
        set => m_bools[c_deleteIdx] = value;
    }

    public bool Default
    {
        get => m_bools[c_defaultIdx];
        set => m_bools[c_defaultIdx] = value;
    }

    public BaseId DeleteReplacementId
    {
        get => m_replacementId;
        set => m_replacementId = value;
    }

    public PlantPermissionSetT() { }

    public PlantPermissionSetT(PlantPermissionSet a_permissionSets)
    {
        m_plantPermissionSet = a_permissionSets;
    }

    public override string Description => "A plant permission group is created, modified, or deleted";
}