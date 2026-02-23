namespace PT.Common;

/// <summary>
/// Summary description for Id.
/// </summary>
public struct Id : IPTSerializable
{
    public const int UNIQUE_ID = 177;
    private ulong m_id;

    #region IPTSerializable Members
    public Id(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out m_id);
        }
        else
        {
            m_id = 0; //Must set to something or else won't build.
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_id);
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public override bool Equals(object o)
    {
        if (o is Id)
        {
            return this == (Id)o;
        }

        throw new ApplicationException("Invalid comparison between Id and another type");
    }

    public override string ToString()
    {
        return m_id.ToString();
    }

    public ulong ToBaseType()
    {
        return ulong.Parse(m_id.ToString());
    }

    public override int GetHashCode()
    {
        return (int)m_id;
    }

    public Id(ulong id)
    {
        this.m_id = id;
    }

    public Id(int id)
    {
        this.m_id = (ulong)id;
    }

    public Id(string id)
    {
        this.m_id = ulong.Parse(id);
    }

    public static bool operator ==(Id a, Id b)
    {
        return a.m_id == b.m_id;
    }

    public static bool operator !=(Id a, Id b)
    {
        return a.m_id != b.m_id;
    }

    public static Id operator ++(Id a)
    {
        a.m_id++;
        return a;
    }

    public static Id operator --(Id a)
    {
        a.m_id--;
        return a;
    }

    public static bool operator <(Id a, Id b)
    {
        return a.m_id < b.m_id;
    }

    public static bool operator >(Id a, Id b)
    {
        return a.m_id > b.m_id;
    }

    public static Type GetIdType()
    {
        return typeof(ulong);
    }

    public int CompareTo(object o)
    {
        Id c = (Id)o;

        if (m_id < c.m_id)
        {
            return -1;
        }

        if (m_id == c.m_id)
        {
            return 0;
        }

        return 1;
    }

    public Id NextId => new (m_id + 1);

    public static Id MaxValue => new (ulong.MaxValue);
}