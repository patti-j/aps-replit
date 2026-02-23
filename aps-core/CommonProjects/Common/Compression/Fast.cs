using LZ4;

namespace PT.Common.Compression;

/// <summary>
/// This class uses LZ4 fast compression algorithm. Use regular compression if
/// smaller compressed files are desired.
/// https://code.google.com/p/lz4/
/// </summary>
public class Fast
{
    public class FastCompressionException : CommonException
    {
        internal FastCompressionException(string a_msg)
            : base(a_msg) { }
    }

    public static byte[] Compress(byte[] a_input)
    {
        if (a_input == null || a_input.Length == 0)
        {
            throw new FastCompressionException("input byte array was null or empty. Can't compress bytes.");
        }

        return LZ4Codec.Wrap(a_input);
    }

    public static byte[] Decompress(byte[] a_input)
    {
        if (a_input == null || a_input.Length == 0)
        {
            throw new FastCompressionException("input byte array was null or empty. Can't decompress bytes.");
        }

        return LZ4Codec.Unwrap(a_input);
    }
}