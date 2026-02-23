namespace PT.APSCommon;

/// <summary>
/// Summary description for BaseId.
/// </summary>
public readonly struct BaseId : IEquatable<BaseId>, IComparable, IComparable<BaseId>, IPTDeserializable, System.Runtime.Serialization.ISerializable
{
    #region IPTSerializable Members
    public BaseId(IReader a_reader)
    {
        a_reader.Read(out m_id);
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_id);
    }

    public int UniqueId => 172;
    #endregion

    #region ISerializable Members
    /// <summary>
    /// This ISerializable region is required for backwards compatibility with Grid Layouts. In old version they were serialized to xml.
    /// Starting in version 11, gridlayouts no longer require IISerializable. This can be removed once we can enough time has elapsed for users to migrate their layouts.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    public BaseId(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
    {
        m_id = info.GetInt64("id");
    }

    public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
    {
        info.AddValue("id", m_id, typeof(long));
    }
    #endregion

    private readonly long m_id;

    public static BaseId NULL_ID = new (long.MinValue);
    public static BaseId ERP_ID = new (long.MinValue + 1);
    public static BaseId NEW_OBJECT_ID = new (long.MinValue + 4);
    public static BaseId ServerId => NULL_ID;

    public bool Equals(BaseId a_otherId)
    {
        return m_id == a_otherId.m_id;
    }

    public override bool Equals(object a_o)
    {
        return m_id == (a_o as BaseId?)?.m_id;
    }

    public override string ToString()
    {
        return m_id.ToString();
    }

    public long ToBaseType()
    {
        return m_id;
    }

    public override int GetHashCode()
    {
        return m_id.GetHashCode();
    }

    public BaseId(long a_id)
    {
        m_id = a_id;
    }

    public BaseId(ulong a_id)
    {
        m_id = Convert.ToInt64(a_id);
    }

    public BaseId(BaseId a_id)
    {
        m_id = a_id.m_id;
    }

    public BaseId(string a_id)
    {
        m_id = long.Parse(a_id);
    }

    public static bool operator ==(BaseId a_a, BaseId a_b)
    {
        return a_a.m_id == a_b.m_id;
    }

    public static bool operator !=(BaseId a_a, BaseId a_b)
    {
        return a_a.m_id != a_b.m_id;
    }

    public static bool operator ==(BaseId a_a, long a_id)
    {
        return a_a.m_id == a_id;
    }

    public static bool operator !=(BaseId a_a, long a_id)
    {
        return a_a.m_id != a_id;
    }

    public static bool operator >(BaseId a_a, BaseId a_b)
    {
        return a_a.m_id > a_b.m_id;
    }

    public static bool operator >=(BaseId a_a, BaseId a_b)
    {
        return a_a.m_id >= a_b.m_id;
    }

    public static bool operator <(BaseId a_a, BaseId a_b)
    {
        return a_a.m_id < a_b.m_id;
    }

    public static bool operator <=(BaseId a_a, BaseId a_b)
    {
        return a_a.m_id <= a_b.m_id;
    }

    //public static BaseId operator ++(BaseId a_a)
    //{
    //    ++a_a.m_id;
    //    return a_a;
    //}

    public static Type GetIdType()
    {
        return typeof(long);
    }

    public int CompareTo(BaseId a_id)
    {
        return Compare(this, a_id);
    }

    int IComparable.CompareTo(object a_o)
    {
        return Compare(this, (BaseId)a_o);
    }

    public BaseId NextId => GetNextId();

    public BaseId GetNextId()
    {
        return new BaseId(m_id + 1);
    }

    public static int Compare(BaseId a_a, BaseId a_b)
    {
        // There is code in the system that is locked in on 
        // the -1, 0, or 1 that this function returns.
        // Trying to simply return a negative number, 0, or a positive number (return (int)(a_a.m_id - a_b.m_id);)
        // will cause Optimize to fail.
        return a_a.m_id.CompareTo(a_b.m_id);
    }

    // Commented to prevent it from being used in place of a long.
    //public static implicit operator long(BaseId a_id)
    //{
    //    return a_id.m_id;
    //}

    /// <summary>
    /// The primitive representation of this id.
    /// </summary>
    public long Value => m_id;
}