using System.Collections;

using PT.APSCommon;

namespace PT.Transmissions.User;

public class UserAdminLogOffT : UserIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 844;

    #region IPTSerializable Members
    public UserAdminLogOffT(IReader reader) : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                AddUserLogOff(new BaseId(reader));
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(LogOffUsersCount);
        for (int i = 0; i < LogOffUsersCount; i++)
        {
            this[i].Serialize(writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public UserAdminLogOffT() { }

    public UserAdminLogOffT(BaseId userId) : base(userId)
    {
    }

    private readonly ArrayList m_users = new ();

    public bool Contains(BaseId a_id)
    {
        return m_users.Contains(a_id);
    }

    public void AddUserLogOff(BaseId a_id)
    {
        m_users.Add(a_id);
    }

    public int LogOffUsersCount => m_users.Count;

    public BaseId this[int index] => (BaseId)m_users[index];
}