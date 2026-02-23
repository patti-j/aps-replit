using System.Diagnostics;

namespace PT.Common;

/// <summary>
/// Helper class containing methods for bytes manipulation.
/// </summary>
public class ByteHelper
{
    /// <summary>
    /// The number of bytes in an int.
    /// </summary>
    public static readonly int c_bytesPerInt = 4;

    /// <summary>
    /// The number of bytes in a long.
    /// </summary>
    public static readonly int c_bytesPerLong = 8;

    /// <summary>
    /// The number of bytes in a float.
    /// </summary>
    public static readonly int c_bytesPerFloat = 4;

    /// <summary>
    /// The number of bytes in a double.
    /// </summary>
    public static readonly int c_bytesPerDouble = 8;

    /// <summary>
    /// Check the correctness of each constant.
    /// </summary>
    [Conditional("DEBUG")]
    public static void Test()
    {
        if (c_bytesPerInt != BitConverter.GetBytes(0).Length)
        {
            throw new Exception("bytes per int wrong.");
        }

        if (c_bytesPerLong != BitConverter.GetBytes((long)0).Length)
        {
            throw new Exception("bytes per long wrong.");
        }

        if (c_bytesPerFloat != BitConverter.GetBytes((float)0).Length)
        {
            throw new Exception("bytes per float wrong.");
        }

        if (c_bytesPerDouble != BitConverter.GetBytes((double)0).Length)
        {
            throw new Exception("bytes per double wrong.");
        }
    }
}