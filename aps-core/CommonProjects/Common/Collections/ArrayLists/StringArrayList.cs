using System.Collections;

namespace PT.Common;

/// <summary>
/// Summary description for StringArrayList.
/// </summary>
[Serializable]
public class StringArrayList : IPTSerializable, IEnumerable
{
    public const int UNIQUE_ID = 228;

    #region PT Serialization
    public StringArrayList(IReader reader)
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

    public StringArrayList() { }

    public StringArrayList(IEnumerable<string> a_strings)
    {
        foreach (string str in a_strings)
        {
            m_stringArrayList.Add(str);
        }
    }

    public virtual void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif
        a_writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            a_writer.Write(this[i]);
        }
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    private readonly ArrayList m_stringArrayList = new ();

    public virtual void Add(string s)
    {
        m_stringArrayList.Add(s);
    }

    public int Count => m_stringArrayList.Count;

    public string this[int i]
    {
        get => (string)m_stringArrayList[i];

        set => m_stringArrayList[i] = value;
    }

    public void Clear()
    {
        m_stringArrayList.Clear();
    }

    public IEnumerator GetEnumerator()
    {
        return m_stringArrayList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}