namespace PT.Common;

/// <summary>
/// Summary description for UniqueStringArrayList.
/// </summary>
[Serializable]
public class UniqueStringArrayList : StringArrayList
{
    public new const int UNIQUE_ID = 226;

    #region PT Serialization
    public UniqueStringArrayList(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                string s;
                reader.Read(out s);
                Add(s);
            }
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public UniqueStringArrayList() { }

    public class UniqueStringArrayListException : CommonException
    {
        internal UniqueStringArrayListException(string s)
            : base(s) { }
    }

    private HashSet<string> m_stringHash = new ();
    private const string c_alreadyExistsErrMsg = "'{0}' has already been added to this collection.";

    public override void Add(string s)
    {
        if (m_stringHash.Contains(s))
        {
            throw new UniqueStringArrayListException(string.Format(c_alreadyExistsErrMsg, s));
        }

        base.Add(s);
        m_stringHash.Add(s);
    }

    public bool Contains(string s)
    {
        return m_stringHash.Contains(s);
    }
}