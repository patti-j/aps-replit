using LZ4;
using PT.Common.Compression;
using PT.Common.Compression.LZMA;
using PT.Common.Encryption;
using System.IO.Compression;
using System.Security.Cryptography;

namespace PT.Common;

public class BinaryMemoryReader : BinaryStreamReader, IDisposable
{
    private MemoryStream m_ms;
    private CryptoStream m_cryptoStream;

    #region compression
    private ECompressionType m_compressionType = ECompressionType.None;

    public ECompressionType CompressionType => m_compressionType;

    private Stream m_compressStream;
    private readonly byte[] m_symmetricKey;
    #endregion

    public BinaryMemoryReader(byte[] a_symmetricKey, byte[] a_buffer)
    {
        m_symmetricKey = a_symmetricKey;
        InitByteArray(a_buffer);
    }

    public BinaryMemoryReader(byte[] a_buffer)
    {
        InitByteArray(a_buffer);
    }

    /// <summary>
    /// Read the contents of a BinaryMemoryWriter.
    /// </summary>
    /// <param name="a_writer"></param>
    public BinaryMemoryReader(BinaryMemoryWriter a_writer)
    {
        InitByteArray(a_writer.GetBuffer());
    }

    private void InitByteArray(byte[] a_buffer)
    {
        m_ms = new MemoryStream(a_buffer);
        m_br = new BinaryReader(m_ms);
        

        Read(out m_versionNumber);

        #region 12533
        if (m_versionNumber >= 12533)
        {
            Read(out int compressionType);
            m_compressionType = (ECompressionType)compressionType;
            Read(out bool encrypted);
            if (encrypted)
            {
                Read(out byte[] iv);
                m_cryptoStream = DataEncryption.GetCryptoStreamReader(m_ms, m_symmetricKey, iv);
                m_br = new BinaryReader(m_cryptoStream);
            }

            if (m_compressionType == ECompressionType.Fast)
            {
                m_compressStream = encrypted
                    ? new LZ4Stream(m_cryptoStream, LZ4StreamMode.Decompress)
                    : new LZ4Stream(m_ms, LZ4StreamMode.Decompress);

                m_br = new BinaryReader(m_compressStream);
            }
            else if (m_compressionType == ECompressionType.Normal || m_compressionType == ECompressionType.High)
            {
                m_compressStream = encrypted
                    ? new BrotliStream(m_cryptoStream, CompressionMode.Decompress)
                    : new BrotliStream(m_ms, CompressionMode.Decompress);

                m_br = new BinaryReader(m_compressStream);
            }
            else if (m_compressionType == ECompressionType.Full)
            {
                //Read compressed byte arrays
                int totalParts;
                Read(out totalParts);
                List<byte[]> array = new();
                for (int i = 0; i < totalParts; i++)
                {
                    byte[] compressedBytes;
                    Read(out compressedBytes);
                    array.Add(compressedBytes);
                }

                //Decompress to orginal data
                byte[] bytes = SevenZipHelper.Decompress(array);

                m_ms = new MemoryStream(bytes);
                m_br = new BinaryReader(m_ms);
            }
        }
        #endregion
        #region 717
        else if (m_versionNumber >= 717)
        {
            if (m_versionNumber >= 717)
            {
                Read(out int compressionType);
                m_compressionType = (ECompressionType)compressionType;
                Read(out bool encrypted);
                if (encrypted)
                {
                    Read(out byte[] iv);
                    m_cryptoStream = DataEncryption.GetCryptoStreamReader(m_ms, m_symmetricKey, iv);
                    m_br = new BinaryReader(m_cryptoStream);
                }

                if (m_compressionType == ECompressionType.Fast)
                {
                    m_compressStream = encrypted
                        ? new LZ4Stream(m_cryptoStream, LZ4StreamMode.Decompress)
                        : new LZ4Stream(m_ms, LZ4StreamMode.Decompress);

                    m_br = new BinaryReader(m_compressStream);
                }
                else if (m_compressionType == ECompressionType.Normal)
                {
                    m_compressStream = encrypted
                        ? new LZ4Stream(m_cryptoStream, LZ4StreamMode.Decompress, true)
                        : new LZ4Stream(m_ms, LZ4StreamMode.Decompress, true);
                    m_br = new BinaryReader(m_compressStream);
                }
                else if (m_compressionType == ECompressionType.High)
                {
                    //Read compressed byte arrays
                    int totalParts;
                    Read(out totalParts);
                    List<byte[]> array = new();
                    for (int i = 0; i < totalParts; i++)
                    {
                        byte[] compressedBytes;
                        Read(out compressedBytes);
                        array.Add(compressedBytes);
                    }

                    //Decompress to orginal data
                    byte[] bytes = SevenZipHelper.Decompress(array);

                    m_ms = new MemoryStream(bytes);
                    m_br = new BinaryReader(m_ms);
                }
            }
        }
        #endregion
    }

    public override void Close()
    {
        base.Close();
        m_compressStream?.Close();
        m_cryptoStream?.Close();
        m_ms.Close();
    }

    public long Length => m_ms.Length;

    private bool m_disposed;

    public void Dispose()
    {
        if (!m_disposed)
        {
            m_compressStream?.Dispose();
            m_cryptoStream?.Dispose();
            m_ms.Dispose();
            m_br.Dispose();
            m_disposed = true;
        }
    }
}