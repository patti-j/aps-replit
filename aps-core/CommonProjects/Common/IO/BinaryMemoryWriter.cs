using LZ4;
using PT.Common.Compression;
using PT.Common.Encryption;
using System.IO.Compression;
using System.Security.Cryptography;

namespace PT.Common;

/// <summary>
/// Summary description for BinaryMemoryWriter.
/// </summary>
public class BinaryMemoryWriter : BinaryStreamWriter, IDisposable
{
    private readonly MemoryStream m_ms;
    private CryptoStream m_cryptoStream;
    private readonly byte[] m_symmetricKey;
    protected bool Encrypted => m_symmetricKey != null;
    private readonly byte[] m_iv;
    private readonly byte[] m_highCompressionHeader;

    #region compression
    private readonly ECompressionType m_compressionType;

    public ECompressionType CompressionType => m_compressionType;

    private readonly Stream m_compressStream;
    private readonly bool m_useArrayCompression;
    #endregion

    public BinaryMemoryWriter()
        : this(null, ECompressionType.Normal)
    {

    }

    public BinaryMemoryWriter(ECompressionType a_compressionType)
        : this(null, a_compressionType)
    {

    }

    public BinaryMemoryWriter(byte[] a_symmetricKey, ECompressionType a_compressionType)
    {
        m_ms = new MemoryStream();
        m_bw = new BinaryWriter(m_ms);
        m_compressionType = a_compressionType;
        m_symmetricKey = a_symmetricKey;

        Write(Serialization.VersionNumber);
        Write((int)m_compressionType);

        Write(Encrypted);

        if (Encrypted)
        {
            m_iv = DataEncryption.GenerateIV();
            Write(m_iv);
        }

        //12533
        switch (m_compressionType)
        {
            case ECompressionType.None:
                if (Encrypted)
                {
                    m_cryptoStream = DataEncryption.GetCryptoStreamWriter(m_ms, m_symmetricKey, m_iv);
                    m_bw = new BinaryWriter(m_cryptoStream);
                }

                break;
            case ECompressionType.Fast:
                if (Encrypted)
                {
                    m_cryptoStream = DataEncryption.GetCryptoStreamWriter(m_ms, m_symmetricKey, m_iv);
                    m_compressStream = new LZ4Stream(m_cryptoStream, LZ4StreamMode.Compress, false);
                }
                else
                {
                    m_compressStream = new LZ4Stream(m_ms, LZ4StreamMode.Compress, false);
                }

                m_bw = new BinaryWriter(m_compressStream);
                break;
            case ECompressionType.Normal:
                if (Encrypted)
                {
                    m_cryptoStream = DataEncryption.GetCryptoStreamWriter(m_ms, m_symmetricKey, m_iv);
                    m_compressStream = new BrotliStream(m_cryptoStream, CompressionLevel.Optimal, true);
                }
                else
                {
                    m_compressStream = new BrotliStream(m_ms, CompressionLevel.Optimal, true);
                }

                m_bw = new BinaryWriter(m_compressStream);
                break;
            case ECompressionType.High:
                if (Encrypted)
                {
                    m_cryptoStream = DataEncryption.GetCryptoStreamWriter(m_ms, m_symmetricKey, m_iv);
                    m_compressStream = new BrotliStream(m_cryptoStream, CompressionLevel.SmallestSize, true);

                }
                else
                {
                    m_compressStream = new BrotliStream(m_ms, CompressionLevel.SmallestSize, true);
                }

                m_bw = new BinaryWriter(m_compressStream);
                break;

            case ECompressionType.Full:
                m_ms.Flush();
                m_highCompressionHeader = m_ms.ToArray();
                m_ms.Dispose();
                m_compressStream = new MemoryStream();
                m_bw = new BinaryWriter(m_compressStream);
                m_useArrayCompression = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override void Close()
    {
        base.Close();
        m_compressStream?.Dispose();
        m_cryptoStream?.Dispose();
        m_ms.Close();
    }

    public override void Flush()
    {
        base.Flush();
        m_compressStream?.Flush();
        m_cryptoStream?.Flush();
        m_cryptoStream?.FlushFinalBlock();
        m_ms.Flush();
    }

    public byte[] GetBuffer()
    {
        Flush();
        byte[] compressedBytes = Compress();
        return compressedBytes;
    }

    private bool m_disposed;

    public void Dispose()
    {
        if (!m_disposed)
        {
            m_compressStream?.Dispose();
            m_cryptoStream?.Dispose();
            m_ms.Dispose();
            m_bw.Dispose();
            m_disposed = true;
        }
    }

    /// <summary>
    /// Performs array compression if needed. Returns the memory array.
    /// </summary>
    private byte[] Compress()
    {
        if (m_useArrayCompression)
        {
            //Compress outside of a stream. We need to recreate the writer and write initial values so the data can be read.
            List<byte[]> array = Compression.LZMA.SevenZipHelper.Compress(m_compressStream);
            using (MemoryStream compressedStream = new ())
            {
                WriteToCompressedStream(compressedStream, array);

                byte[] compressBytes = compressedStream.ToArray();

                byte[] rv = new byte[m_highCompressionHeader.Length + compressBytes.Length];
                Buffer.BlockCopy(m_highCompressionHeader, 0, rv, 0, m_highCompressionHeader.Length);
                Buffer.BlockCopy(compressBytes, 0, rv, m_highCompressionHeader.Length, compressBytes.Length);

                return rv;
            }
        }

        return m_ms.ToArray();
    }

    public void CompressAndCopyStream(MemoryStream a_outputStream)
    {
        Flush();
        m_ms.Position = 0;

        if (m_useArrayCompression)
        {
            List<byte[]> array = Compression.LZMA.SevenZipHelper.Compress(m_compressStream);
            WriteToCompressedStream(a_outputStream, array);
        }
        else
        {
            m_ms.CopyTo(a_outputStream);
        }
    }

    private void WriteToCompressedStream(MemoryStream a_outputStream, List<byte[]> a_compressedBytes)
    {
        if (Encrypted)
        {
            m_cryptoStream = DataEncryption.GetCryptoStreamWriter(a_outputStream, m_symmetricKey, m_iv);
            m_bw = new BinaryWriter(m_cryptoStream);
        }
        else
        {
            m_bw = new BinaryWriter(a_outputStream);
        }

        Write(a_compressedBytes.Count);
        foreach (byte[] bytes in a_compressedBytes)
        {
            Write(bytes);
        }

        Flush();
    }
}