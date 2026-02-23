using System.Drawing;

namespace PT.Common;

public interface IReader
{
    #region Version
    int VersionNumber { get; }
    #endregion

    #region Primitive data types.
    void Read(out sbyte o_data);
    void Read(out byte o_data);
    void Read(out short o_data);
    void Read(out ushort o_data);
    void Read(out int o_data);
    void Read(out uint o_data);
    void Read(out long o_data);
    void Read(out long? o_data);
    void Read(out ulong o_data);
    void Read(out char o_data);
    void Read(out float o_data);
    void Read(out double o_data);

    void ReadDateTimeToOffsetConversion(out DateTimeOffset o_data);
    void Read(out bool o_data);
    void Read(out decimal o_data);
    void Read(out string o_data);

    void Read(out byte[] o_data);
    void Read(out decimal[] o_data);
    void Read(out string[] o_data);
    #endregion

    #region System structures.
    void Read(out TimeSpan o_data);
    void Read(out DateTime o_data);
    void Read(out DateTimeOffset o_data);
    void Read(out Color o_data);
    #endregion System structures.

    #region Boxed Primitive data types.
    void ReadBoxedPrimitiveAndCommonSystemStructs(out object o_data);
    #endregion

    #region System classes.
    void Read(out Exception o_data);
    void Read(out ApplicationException o_data);

    void Read(out HashSet<string> o_data);

    void ReadList(out List<string> o_data);

    /// <summary>
    /// Read a generic HashSet written with IWriter.Write(IReaderClassFactory,, out HashSet<T>)
    /// </summary>
    /// <typeparam name="T">
    /// The type of object stored in the HashSet<></typeparam>
    /// <param name="a_classFactory">
    /// A class that can serialize and deserialize the data stored in the HashSet<></param>
    /// <param name="o_hs"></param>
    void Read<T>(IReaderClassFactory a_classFactory, out HashSet<T> o_hs);

    /// <summary>
    /// Read a generic Dictionary
    /// </summary>
    /// <typeparam name="Key">The type of the key.</typeparam>
    /// <typeparam name="Value">The type of the value.</typeparam>
    /// <param name="a_classFactory">An object able to read the key and value types of the Dictionary.</param>
    /// <param name="a_data"></param>
    void Read<Key, Value>(IReaderClassFactory a_classFactory, out Dictionary<Key, Value> a_data) where Key : IPTSerializable where Value : IPTSerializable;

    /// <summary>
    /// Read  a generic dictionary written by IWriter.Write<Key, Value>(IReaderDictionaryKeyValueSerializer a_serializer, Dictionary<Key, Value> a_dict)
    /// </summary>
    /// <typeparam name="Key">The type of the Dictionary's keys.</typeparam>
    /// <typeparam name="Value">The type of the Dictionary's Values.</typeparam>
    /// <param name="a_classFactory">An object able to read and write the Dictionary's keys and values.</param>
    /// <param name="o_dict">An out reference to the read Dictionary.</param>
    void Read<Key, Value>(IReaderDictionaryKeyValueSerializer a_classFactory, out Dictionary<Key, Value> o_dict);

    void Read(out Guid o_data);
    #endregion
}