namespace PT.Common;

/// <summary>
/// Use to create an object that can can deserialize a specific type of object for use with functions such as
/// IReader.Read
/// <T, T2>
/// (IObjectReader a_classFactory, out Dictionary
/// <T, T2>
/// a_data)
/// Where the body of the code the implements IReader.Read is reusable with different type of IObjectReaders.
/// </summary>
public interface IReaderClassFactory
{
    object Read(IReader a_reader);

    /// <summary>
    /// Implement if you want to read data into this object. and not return a new object.
    /// For instance if this object had members and you wanted to fill them in, implement this
    /// function to read into this.
    /// </summary>
    /// <param name="a_reader"></param>
    /// <returns></returns>
    void ReadThis(IReader a_reader);
}

/// <summary>
/// For use of some IReader and IWriter functions. For instance reading and
/// writing generic HashSets and Dictionaries.
/// </summary>
public interface IReaderDictionaryKeyValueSerializer
{
    /// <summary>
    /// Read a key. This function will be called by IReader to  read a key.
    /// </summary>
    /// <param name="a_reader"></param>
    /// <returns></returns>
    object ReadKey(IReader a_reader);

    /// <summary>
    /// Read a value. This function will be called by IReader to read a value.
    /// </summary>
    /// <param name="a_reader"></param>
    /// <returns></returns>
    object ReadValue(IReader a_reader);

    /// <summary>
    /// Write a key. This function will be called by IWriter to write a key.
    /// </summary>
    /// <param name="a_writer"></param>
    /// <param name="a_key"></param>
    void WriteKey(IWriter a_writer, object a_key);

    /// <summary>
    /// Write a value. This function will be called by IWriter to write a value.
    /// </summary>
    /// <param name="a_writer"></param>
    /// <param name="a_value"></param>
    void WriteValue(IWriter a_writer, object a_value);
}