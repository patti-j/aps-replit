namespace PT.APSCommon.Compression;

/// <summary>
/// A container for IPTSerializable objects. The data will remain compressed until it is retrieved.
/// Data will be re-serialized and compressed again on demand or when the container is serialized.
/// When retrieved, the return class is created using the class' Constructor using a single IReader parameter.
/// A constructor of that type must exist for T, or this container will fail.
/// </summary>
/// <typeparam name="T"></typeparam>
public class CompressedDataContainer<T> : IPTSerializable, ICloneable where T : class, IPTSerializable
{
    private T m_data;
    private byte[] m_compressedData;
    private bool m_dataCompressed;
    private readonly System.Reflection.ConstructorInfo m_typeConstructor;

    /// <summary>
    /// Create a new container with an already created object.
    /// </summary>
    /// <param name="a_object"></param>
    public CompressedDataContainer(T a_object)
    {
        m_data = a_object;
        m_dataCompressed = false;
        m_typeConstructor = typeof(T).GetConstructor(new[] { typeof(IReader) });
    }

    /// <summary>
    /// Create a new container with serialized compressed data
    /// </summary>
    /// <param name="a_compressedData"></param>
    public CompressedDataContainer(byte[] a_compressedData)
    {
        m_compressedData = a_compressedData;
        m_dataCompressed = true;
        m_typeConstructor = typeof(T).GetConstructor(new[] { typeof(IReader) });
    }

    /// <summary>
    /// Create a new container by deserializing
    /// </summary>
    /// <param name="a_reader"></param>
    public CompressedDataContainer(IReader a_reader)
    {
        m_typeConstructor = typeof(T).GetConstructor(new[] { typeof(IReader) });
        a_reader.Read(out m_compressedData);
        m_dataCompressed = true;
    }

    /// <summary>
    /// Creates a new instance of the serialized object.
    /// </summary>
    /// <returns>Returns the same reference until the data is re-compressed</returns>
    public T LoadData()
    {
        if (!m_dataCompressed)
        {
            return m_data;
        }

        //decompress and return
        return Decompress();
    }

    /// <summary>
    /// Whether the data has been decompressed and an object instance created
    /// </summary>
    public bool IsDataLoaded => !m_dataCompressed;

    /// <summary>
    /// Interface implementation. Don't use manually. The data can be compressed by calling CompressData()
    /// </summary>
    /// <param name="a_writer"></param>
    public void Serialize(IWriter a_writer)
    {
        if (m_dataCompressed)
        {
            //data was never decompressed. Just serialize
            a_writer.Write(m_compressedData);
        }
        else
        {
            CompressData();
            a_writer.Write(m_compressedData);
        }
    }

    /// <summary>
    /// Serializes and compresses the data. The reference to the previously created object will be lost to this container.
    /// The next data retrieval will return a new instance.
    /// </summary>
    public void CompressData()
    {
        if (m_dataCompressed)
        {
            return;
        }

        byte[] data;
        using (BinaryMemoryWriter writer = new ())
        {
            m_data.Serialize(writer);
            data = writer.GetBuffer();
        }

        m_compressedData = Common.Compression.Optimal.Compress(data);
        m_data = null;
        m_dataCompressed = true;
    }

    /// <summary>
    /// Decompresses and deserializes the object. Creates a new instance using the required constructor
    /// </summary>
    /// <returns>A new object instance</returns>
    private T Decompress()
    {
        if (!m_dataCompressed)
        {
            return m_data;
        }

        byte[] decompress = Common.Compression.Optimal.Decompress(m_compressedData);
        using (BinaryMemoryReader reader = new (decompress))
        {
            m_data = (T)m_typeConstructor.Invoke(new object[] { reader });
        }

        m_dataCompressed = false;
        m_compressedData = null;
        return m_data;
    }

    public int UniqueId => 1019;

    /// <summary>
    /// Cloning the container, will result in the current data reference being lost.
    /// </summary>
    /// <returns></returns>
    public object Clone()
    {
        //Serialize the data without compressing the current data.
        byte[] bytes;
        using (BinaryMemoryWriter writer = new ())
        {
            Decompress().Serialize(writer);
            bytes = writer.GetBuffer();
        }

        byte[] compressedData = Common.Compression.Optimal.Compress(bytes);
        return new CompressedDataContainer<T>(compressedData);
    }
}