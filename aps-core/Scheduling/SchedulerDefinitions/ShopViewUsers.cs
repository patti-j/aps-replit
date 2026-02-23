using PT.APSCommon;

namespace PT.SchedulerDefinitions;

public class ShopViewUsers : IPTSerializable
{
    #region IPTSerializable
    public const int UNIQUE_ID = 492;

    public ShopViewUsers(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                ShopViewUser svUser = new (reader);
                Add(svUser);
            }
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            GetByIndex(i).Serialize(writer);
        }
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public ShopViewUsers() { }

    #region List Maintenance
    private readonly SortedList<BaseId, ShopViewUser> m_svUsers = new ();

    public void Add(ShopViewUser a_svUser)
    {
        if (Contains(a_svUser.UserId))
        {
            throw new CommonException("4031");
        }

        m_svUsers.Add(a_svUser.UserId, a_svUser);
    }

    public bool Contains(BaseId a_userId)
    {
        return m_svUsers.ContainsKey(a_userId);
    }

    public ShopViewUser this[BaseId a_userId] => m_svUsers[a_userId];

    public int Count => m_svUsers.Count;

    public ShopViewUser GetByIndex(int a_index)
    {
        return m_svUsers.Values[a_index];
    }

    public void Remove(BaseId a_userId)
    {
        if (m_svUsers.ContainsKey(a_userId))
        {
            m_svUsers.Remove(a_userId);
        }
    }
    #endregion

    public class ShopViewUser : IPTSerializable
    {
        #region IPTSerializable
        public const int UNIQUE_ID = 491;

        public ShopViewUser(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                m_userId = new BaseId(reader);
                reader.Read(out m_canReassign);
                reader.Read(out m_canResequence);
                reader.Read(out m_canUpdateStatus);
            }
        }

        public void Serialize(IWriter writer)
        {
            m_userId.Serialize(writer);
            writer.Write(m_canReassign);
            writer.Write(m_canResequence);
            writer.Write(m_canUpdateStatus);
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public ShopViewUser(BaseId a_userId)
        {
            m_userId = a_userId;
        }

        private readonly BaseId m_userId;
        public BaseId UserId => m_userId;

        #region Rights
        private bool m_canUpdateStatus;

        public bool CanUpdateStatus
        {
            get => m_canUpdateStatus;
            set => m_canUpdateStatus = value;
        }

        private bool m_canResequence;

        public bool CanResequence
        {
            get => m_canResequence;
            set => m_canResequence = value;
        }

        private bool m_canReassign;

        public bool CanReassign
        {
            get => m_canReassign;
            set => m_canReassign = value;
        }
        #endregion
    }
}