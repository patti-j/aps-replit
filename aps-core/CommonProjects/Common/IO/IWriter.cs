using System.Drawing;

namespace PT.Common;

public interface IWriter
{
    #region Primitive data types.
    void Write(sbyte a_data);
    void Write(byte a_data);
    void Write(short a_data);
    void Write(ushort a_data);
    void Write(int a_data);
    void Write(uint a_data);
    void Write(long a_data);
    void Write(long? a_data);
    void Write(ulong a_data);
    void Write(char a_data);
    void Write(float a_data);
    void Write(double a_data);
    void Write(bool a_data);
    void Write(decimal a_data);
    void Write(string a_data);

    void Write(byte[] a_data);
    void Write(decimal[] a_data);
    void Write(string[] a_data);
    #endregion Primitive data types.

    #region System structures.
    void Write(TimeSpan a_data);
    void Write(DateTime a_data);
    void Write(DateTimeOffset a_data);
    void Write(Color a_data);

    void Write<T>(List<T> a_data) where T : IPTSerializable;

    void WriteList(List<string> a_data);

    /// <summary>
    /// Write a generic dictionary using a IReaderDictionaryKeyValueSerializer.
    /// </summary>
    /// <typeparam name="Key">The type of the Dictionary's keys.</typeparam>
    /// <typeparam name="Value">The type of the Dictionary's values.</typeparam>
    /// <param name="a_serializer">An object that can serialize and deserialize the Dictionary's keys and values.</param>
    /// <param name="a_dict"></param>
    void Write<Key, Value>(IReaderDictionaryKeyValueSerializer a_serializer, Dictionary<Key, Value> a_dict);

    void Write(HashSet<string> a_data);

    /// <summary>
    /// Write a generic HashSet of object of IPTSerializable.
    /// </summary>
    /// <typeparam name="T">The type of objects in the HashSet which implement IPTSerializable.</typeparam>
    /// <param name="a_hs"></param>
    void Write<T>(HashSet<T> a_hs) where T : IPTSerializable;
    #endregion

    #region System classes
    void Write(Exception a_data);
    void Write(ApplicationException a_data);

    void Write(Guid a_data);
    #endregion

    #region Write boxed Primitive data types.
    void WriteBoxedPrimitiveAndCommonSystemStructs(object a_data);
    #endregion

    #region Utility functions
    void DuplicateErrorCheck(object a_o);
    #endregion
}