using System.IO.Compression;

namespace PT.Common.Compression;

/// <summary>
/// Different types of compression
/// </summary>
public enum ECompressionType
{
    /// <summary>
    /// Indicates no compression
    /// </summary>
    None = 0,

    /// <summary>
    /// Normal Compression (currently Brotli (optimal))
    /// </summary>
    Normal = 1,

    /// <summary>
    /// Fast Compression (currently LZ4)
    /// </summary>
    Fast = 2,

    /// <summary>
    /// High Compression (currently Brotli (Smallest) and is very slow)
    /// </summary>
    High = 3,
    
    /// <summary>
    /// Max Compression (currently LMZA). This does full array compression and has no benefits of streaming
    /// </summary>
    Full = 4
}

public class Optimal
{
    /// <summary>
    /// Compress an array of bytes using the deflate algorithm. The deflation is performed with System.IO.Compression.DeflateStream.
    /// </summary>
    /// <param name="a_uncompressedbytes">An uncompressed array of bytes.</param>
    /// <returns>A compressed array of bytes.</returns>
    public static byte[] Compress(byte[] a_uncompressedbytes)
    {
        using (MemoryStream ms = new ())
        {
            using (DeflateStream compressStream = new (ms, CompressionMode.Compress))
            {
                compressStream.Write(a_uncompressedbytes, 0, a_uncompressedbytes.Length);
            }

            return ms.ToArray();
        }
    }

    /// <summary>
    /// Decompress bytes compressed with this classes Compress method or by the Deflate algorithm. This function uses System.IO.Compression.DeflateStream to perform the decompression.
    /// </summary>
    /// <param name="a_compressedBytes">Bytes compressed with the deflate algorithm. Specifically using the same format that System.IO.Compression.DeflateStream uses.</param>
    /// <returns>An uncompressed array of bytes.</returns>
    public static byte[] Decompress(byte[] a_compressedBytes)
    {
        using (MemoryStream outstream = new ())
        {
            using (MemoryStream ms = new (a_compressedBytes))
            {
                using (DeflateStream decompressStream = new (ms, CompressionMode.Decompress))
                {
                    decompressStream.CopyTo(outstream);
                }
            }

            return outstream.ToArray();
        }
    }
}