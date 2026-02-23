namespace PT.Scheduler;

public class BaseOperationProductionInfo : IPTSerializable
{
    #region Serialization
    internal BaseOperationProductionInfo(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            m_onlyAllowManualUpdates = new BaseOperationProductionInfoFlags(reader);
        }
    }

    public virtual void Serialize(IWriter writer)
    {
        m_onlyAllowManualUpdates.Serialize(writer);
    }

    public const int UNIQUE_ID = 713;

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    internal BaseOperationProductionInfo()
    {
        m_onlyAllowManualUpdates = new BaseOperationProductionInfoFlags();
    }

    private readonly BaseOperationProductionInfoFlags m_onlyAllowManualUpdates;

    /// <summary>
    /// Use these flags to indicate whether the values should only be updatable by manual modifications through changes made from the APS user interface.
    /// ERP updates and product rule updates will be ignored.
    /// </summary>
    public BaseOperationProductionInfoFlags OnlyAllowManualupdates => m_onlyAllowManualUpdates;
}

public class BaseOperationProductionInfoFlags : IPTSerializable
{
    #region Serialization
    internal BaseOperationProductionInfoFlags(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            m_flags = new BoolVector32(reader);
        }
    }

    public void Serialize(IWriter writer)
    {
        m_flags.Serialize(writer);
    }

    public const int UNIQUE_ID = 714;

    public int UniqueId => UNIQUE_ID;
    #endregion

    internal BaseOperationProductionInfoFlags()
    {
        m_flags = new BoolVector32();
    }

    internal void Copy(BaseOperationProductionInfoFlags a_flags)
    {
        m_flags = new BoolVector32(a_flags.m_flags);
    }

    private BoolVector32 m_flags;

    private const int c_materialsIdx = 0;
    private const int c_productsIdx = 1;

    public bool Materials
    {
        get => m_flags[c_materialsIdx];
        internal set => m_flags[c_materialsIdx] = value;
    }

    public bool Products
    {
        get => m_flags[c_productsIdx];
        internal set => m_flags[c_productsIdx] = value;
    }

    public override string ToString()
    {
        return m_flags.ToString();
    }
}