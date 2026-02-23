namespace PT.ERPTransmissions;

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

    public const int UNIQUE_ID = 715;

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
    private const int c_materialsSetIdx = 2;
    private const int c_productsSetIdx = 3;

    public bool Materials
    {
        get => m_flags[c_materialsIdx];
        internal set
        {
            m_flags[c_materialsIdx] = value;
            MaterialsSet = true;
        }
    }

    public bool MaterialsSet
    {
        get => m_flags[c_materialsSetIdx];
        private set => m_flags[c_materialsSetIdx] = value;
    }

    public bool Products
    {
        get => m_flags[c_productsIdx];
        internal set
        {
            m_flags[c_productsIdx] = value;
            ProductsSet = true;
        }
    }

    public bool ProductsSet
    {
        get => m_flags[c_productsSetIdx];
        private set => m_flags[c_productsSetIdx] = value;
    }

    public override string ToString()
    {
        return m_flags.ToString();
    }
}