using PT.Common.File;

namespace PT.Scheduler;

/// <summary>
/// Used as a place to store any options necessary for startup. The PTSystem keeps one instance of this class on hand.
/// </summary>
[Obsolete("Removed in 12.1.3, kept only for compatibility with older serializations.")]
public class SystemStartupOptions : IPTSerializable
{
    #region Construction
    public SystemStartupOptions()
    {
        biDirectionalConnectionTimeout = DEFAULT_BIDIRECTIONALCONNECTIONTIMEOUT;
    }

    public SystemStartupOptions(int biDirectionalConnectionTimeout)
    {
        this.biDirectionalConnectionTimeout = biDirectionalConnectionTimeout;
    }
    #endregion

    #region IPTSerializable Members
    public const int DEFAULT_BIDIRECTIONALCONNECTIONTIMEOUT = 300; //Better to make it larger to avoid being able to login.

    public SystemStartupOptions(IReader reader)
    {
        if (reader.VersionNumber >= 12005)
        {
            reader.Read(out biDirectionalConnectionTimeout);
            reader.Read(out m_compressedKeyXml);
        }
        else if (reader.VersionNumber >= 508)
        {
            reader.Read(out biDirectionalConnectionTimeout);
            reader.Read(out int uniDirectionalConnectionTimeout);
            reader.Read(out m_compressedKeyXml);
        }
        else if (reader.VersionNumber >= 506)
        {
            reader.Read(out biDirectionalConnectionTimeout);
            reader.Read(out int uniDirectionalConnectionTimeout);
        }
        else if (reader.VersionNumber >= 243)
        {
            reader.Read(out biDirectionalConnectionTimeout);
            reader.Read(out int uniDirectionalConnectionTimeout);

            m_keyFile = new TextFile(reader);
        }

        #region 36
        else if (reader.VersionNumber >= 36)
        {
            reader.Read(out biDirectionalConnectionTimeout);
            reader.Read(out int uniDirectionalConnectionTimeout);

            m_keyFile = new TextFile(reader);

            TextFile configFile_DELETED;
            configFile_DELETED = new TextFile(reader);
        }
        #endregion

        #region 35
        else if (reader.VersionNumber >= 35)
        {
            reader.Read(out biDirectionalConnectionTimeout);
            reader.Read(out int uniDirectionalConnectionTimeout);
        }
        #endregion

        #region 1+
        else if (reader.VersionNumber >= 1)
        {
            biDirectionalConnectionTimeout = DEFAULT_BIDIRECTIONALCONNECTIONTIMEOUT;
        }
        #endregion
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(biDirectionalConnectionTimeout);
        writer.Write(m_compressedKeyXml);
    }

    public const int UNIQUE_ID = 456;

    public int UniqueId => UNIQUE_ID;
    #endregion

    #region Properties
    // this used for scenarios with older serialization version than 508
    private readonly TextFile m_keyFile;

    private byte[] m_compressedKeyXml;

    private int biDirectionalConnectionTimeout = 120;

    /// <summary>
    /// Seconds after which the connection times out.
    /// </summary>
    public int BiDirectionalConnectionTimeout
    {
        get => biDirectionalConnectionTimeout;

        internal set => biDirectionalConnectionTimeout = value;
    }
    #endregion

    /// <summary>
    /// uncompress key xml and write to disk
    /// </summary>
    /// <param name="a_path"></param>
    internal void WriteKeyFile(string a_path)
    {
        if (m_compressedKeyXml != null && m_compressedKeyXml.Length > 0)
        {
            byte[] unCompress = Common.Compression.Optimal.Decompress(m_compressedKeyXml);
            using (FileStream fs = new (a_path, FileMode.Create))
            {
                fs.Write(unCompress, 0, unCompress.Length);
            }
        }
        else if (m_keyFile != null) // old versions
        {
            m_keyFile.WriteFile(a_path);
        }
    }

    /// <summary>
    /// Read file in path and store it's compressed byte array.
    /// </summary>
    /// <param name="a_path"></param>
    internal void ReadKeyFile(string a_path)
    {
        m_compressedKeyXml = Common.Compression.Optimal.Compress(File.ReadAllBytes(a_path));
    }
}