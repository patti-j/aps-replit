namespace PT.SchedulerDefinitions;

/// <summary>
/// Base class for keys identifying various resource types.
/// </summary>
[Serializable]
public class ResourceKeyExternal : IComparable, IPTSerializable
{
    public const int UNIQUE_ID = 560;

    #region IPTSerializable Members
    public ResourceKeyExternal(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out m_plantExternalId);
            a_reader.Read(out m_departmentExternalId);
            a_reader.Read(out m_resourceExternalId);
        }
    }

    public virtual void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif
        a_writer.Write(m_plantExternalId);
        a_writer.Write(m_departmentExternalId);
        a_writer.Write(m_resourceExternalId);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public ResourceKeyExternal(string a_plantExternalId, string a_departmentExternalId, string a_resourceExternalId)
    {
        this.m_plantExternalId = a_plantExternalId;
        this.m_departmentExternalId = a_departmentExternalId;
        this.m_resourceExternalId = a_resourceExternalId;
    }

    public ResourceKeyExternal(string a_stringKey)
    {
        int splitLocation = a_stringKey.IndexOf(SPLIT_STRING);
        if (splitLocation > 0)
        {
            m_plantExternalId = a_stringKey.Substring(0, splitLocation);
            a_stringKey = a_stringKey.Substring(splitLocation + SPLIT_STRING.Length); //remove plant plus split string
            splitLocation = a_stringKey.IndexOf(SPLIT_STRING);
            if (splitLocation > 0)
            {
                m_departmentExternalId = a_stringKey.Substring(0, splitLocation);
                a_stringKey = a_stringKey.Substring(splitLocation + SPLIT_STRING.Length); //remove dept plus split string
                m_resourceExternalId = a_stringKey;
            }
        }
    }

    private string m_plantExternalId;

    public string PlantExternalId => m_plantExternalId;

    private string m_departmentExternalId;

    public string DepartmentExternalId => m_departmentExternalId;

    private string m_resourceExternalId;

    public string ResourceExternalId => m_resourceExternalId;

    #region IComparable Members
    public int CompareTo(object a_obj)
    {
        ResourceKeyExternal key = (ResourceKeyExternal)a_obj;
        if (m_plantExternalId == key.m_plantExternalId && m_departmentExternalId == key.m_departmentExternalId && m_resourceExternalId == key.m_resourceExternalId)
        {
            return 0;
        }

        if (m_plantExternalId.CompareTo(key.m_plantExternalId) == -1)
        {
            return -1;
        }

        if (m_plantExternalId == key.m_plantExternalId && m_departmentExternalId.CompareTo(key.m_departmentExternalId) == -1)
        {
            return -1;
        }

        if (m_plantExternalId == key.m_plantExternalId && m_departmentExternalId == key.m_departmentExternalId && m_resourceExternalId.CompareTo(key.m_resourceExternalId) == -1)
        {
            return -1;
        }

        return 1;
    }
    #endregion

    public override bool Equals(object a_obj)
    {
        ResourceKeyExternal key = (ResourceKeyExternal)a_obj;
        return m_plantExternalId == key.m_plantExternalId && m_departmentExternalId == key.m_departmentExternalId && m_resourceExternalId == key.m_resourceExternalId;
    }

    public static string SPLIT_STRING = "^$^%$&^$";

    public override string ToString()
    {
        return string.Format("{0}{1}{2}{3}{4}", m_plantExternalId, SPLIT_STRING, m_departmentExternalId, SPLIT_STRING, m_resourceExternalId);
    }
}