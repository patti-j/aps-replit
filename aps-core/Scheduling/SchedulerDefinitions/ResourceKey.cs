using PT.APSCommon;

namespace PT.SchedulerDefinitions;

/// <summary>
/// Base class for keys identifying various resource types.
/// </summary>
public class ResourceKey : IComparable<ResourceKey>, IEquatable<ResourceKey>, IPTDeserializable
{
    #region IPTSerializable Members
    public ResourceKey(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            m_plant = new BaseId(a_reader);
            m_department = new BaseId(a_reader);
            m_resource = new BaseId(a_reader);
        }
    }

    public virtual void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif
        m_plant.Serialize(a_writer);
        m_department.Serialize(a_writer);
        m_resource.Serialize(a_writer);
    }

    public const int UNIQUE_ID = 182;

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public ResourceKey(ResourceKey a_k)
        : this(a_k.Plant, a_k.Department, a_k.Resource) { }

    public ResourceKey(BaseId a_plant, BaseId a_department, BaseId a_resource)
    {
        m_plant = new BaseId(a_plant);
        m_department = new BaseId(a_department);
        m_resource = new BaseId(a_resource);
    }

    private readonly BaseId m_plant;

    public BaseId Plant => m_plant;

    private readonly BaseId m_department;

    public BaseId Department => m_department;

    private readonly BaseId m_resource;

    public BaseId Resource => m_resource;

    #region IComparable Members
    public int CompareTo(ResourceKey a_key)
    {
        if (m_plant == a_key.m_plant && m_department == a_key.m_department && m_resource == a_key.m_resource)
        {
            return 0;
        }

        if (m_plant.CompareTo(a_key.m_plant) == -1)
        {
            return -1;
        }

        if (m_plant == a_key.m_plant && m_department.CompareTo(a_key.m_department) == -1)
        {
            return -1;
        }

        if (m_plant == a_key.m_plant && m_department == a_key.m_department && m_resource.CompareTo(a_key.m_resource) == -1)
        {
            return -1;
        }

        return 1;
    }
    #endregion

    public bool Equals(ResourceKey a_other)
    {
        return m_plant == a_other.m_plant && m_department == a_other.m_department && m_resource == a_other.m_resource;
    }

    public override bool Equals(object a_obj)
    {
        ResourceKey key = (ResourceKey)a_obj;
        return Equals(key);
    }

    public override int GetHashCode()
    {
        return (int)(m_plant.Value + m_department.Value + m_resource.Value);
    }

    public override string ToString()
    {
        return string.Format("{0} {1} {2}", m_plant.ToString(), m_department.ToString(), m_resource.ToString());
    }
}