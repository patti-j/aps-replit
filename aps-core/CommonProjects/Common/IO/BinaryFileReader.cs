using LZ4;
using PT.Common.Compression;
using PT.Common.Compression.LZMA;
using System.IO.Compression;

namespace PT.Common;

public class BinaryFileReader : BinaryStreamReader, IDisposable
{
    private readonly FileStream m_fs;

    #region compression
    private readonly ECompressionType m_compressionType = ECompressionType.None;

    public ECompressionType CompressionType => m_compressionType;

    private readonly Stream m_compressStream;
    #endregion

    private Stream m_fullCompressionMS;

    public BinaryFileReader(string a_path)
    {
        m_fs = new FileStream(a_path, FileMode.Open, FileAccess.Read);
        m_br = new BinaryReader(m_fs);
        Read(out m_versionNumber);

        if (m_versionNumber >= 12553)
        {
            Read(out int tmp);
            Read(out m_assemblyVersionMajor);
            Read(out m_assemblyVersionMinor);
            Read(out m_assemblyVersionHotfix);
            Read(out m_assemblyVersionRevision);

            m_compressionType = (ECompressionType)tmp;
            switch (m_compressionType)
            {
                case ECompressionType.None:
                    return;
                case ECompressionType.Fast:
                    m_compressStream = new LZ4Stream(m_fs, LZ4StreamMode.Decompress, false);
                    break;
                case ECompressionType.Normal:
                    m_compressStream = new BrotliStream(m_fs, CompressionMode.Decompress);
                    break;
                case ECompressionType.High:
                    m_compressStream = new BrotliStream(m_fs, CompressionMode.Decompress);
                    break;
                case ECompressionType.Full:
                    //Read compressed byte arrays
                    Read(out int totalParts);
                    List<byte[]> array = new();
                    for (int i = 0; i < totalParts; i++)
                    {
                        Read(out byte[] compressedBytes);
                        array.Add(compressedBytes);
                    }

                    //Decompress to orginal data
                    byte[] bytes = SevenZipHelper.Decompress(array);

                    m_fullCompressionMS = new MemoryStream(bytes);
                    m_br = new BinaryReader(m_fullCompressionMS);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            m_br = new BinaryReader(m_compressStream);
        }
        else if (m_versionNumber >= 12533)
        {
            Read(out int tmp);
            m_compressionType = (ECompressionType)tmp;

            //12533
            switch (m_compressionType)
            {
                case ECompressionType.None:
                    return;
                case ECompressionType.Fast:
                    m_compressStream = new LZ4Stream(m_fs, LZ4StreamMode.Decompress, false);
                    break;
                case ECompressionType.Normal:
                    m_compressStream = new BrotliStream(m_fs, CompressionMode.Decompress);
                    break;
                case ECompressionType.High:
                    m_compressStream = new BrotliStream(m_fs, CompressionMode.Decompress);
                    break;
                case ECompressionType.Full:
                    //Read compressed byte arrays
                    Read(out int totalParts);
                    List<byte[]> array = new();
                    for (int i = 0; i < totalParts; i++)
                    {
                        Read(out byte[] compressedBytes);
                        array.Add(compressedBytes);
                    }

                    //Decompress to orginal data
                    byte[] bytes = SevenZipHelper.Decompress(array);

                    m_fullCompressionMS = new MemoryStream(bytes);
                    m_br = new BinaryReader(m_fullCompressionMS);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            m_br = new BinaryReader(m_compressStream);
        }
        else if (m_versionNumber >= 671)
        {
            int tmp;
            Read(out tmp);
            m_compressionType = (Compression.ECompressionType)tmp;

            // input a stream in the middle that decompresses the bytes.
            if (m_compressionType == Compression.ECompressionType.Fast)
            {
                m_compressStream = new LZ4Stream(m_fs, LZ4StreamMode.Decompress);
                m_br = new BinaryReader(m_compressStream);
            }
            else if (m_compressionType == Compression.ECompressionType.Normal || m_compressionType == Compression.ECompressionType.High)
            {
                m_compressStream = new LZ4Stream(m_fs, LZ4StreamMode.Decompress, true);
                m_br = new BinaryReader(m_compressStream);
            }
        }
    }

    public override void Close()
    {
        base.Close();
        m_compressStream?.Close();
        m_fs.Close();
        m_fullCompressionMS?.Close();
    }

    public long Length => m_fs.Length;

    public void Dispose()
    {
        m_compressStream?.Dispose();
        m_fs.Dispose();
        m_br.Dispose();
        m_fullCompressionMS?.Dispose();
    }
}