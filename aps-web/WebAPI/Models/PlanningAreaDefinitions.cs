namespace WebAPI.Models;

public enum ECompressionType
{
    /// <summary>
    /// Indicates no compression
    /// </summary>
    None = 0,

    /// <summary>
    /// Normal Compression (currently LZ4)
    /// </summary>
    Normal = 1,

    /// <summary>
    /// Fast Compression (currently LZ4)
    /// </summary>
    Fast = 2,

    /// <summary>
    /// High Compression (currently LMZA)
    /// </summary>
    High = 3
}
