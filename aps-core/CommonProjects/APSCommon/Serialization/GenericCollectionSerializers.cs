using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.APSCommon.Serialization;

/// <summary>
/// Used to generically serialize a Dictionary
/// whose key is a BaseID and value is BaseId.
/// </summary>
public class BaseIdBaseIdSerializer : IReaderDictionaryKeyValueSerializer
{
    public object ReadKey(IReader a_reader)
    {
        return ReadBaseId(a_reader);
    }

    private static object ReadBaseId(IReader a_reader)
    {
        BaseId id = new(a_reader);
        return id;
    }

    public object ReadValue(IReader a_reader)
    {
        return ReadBaseId(a_reader);
    }

    public void WriteKey(IWriter a_writer, object a_key)
    {
        SerializeBaseId(a_writer, a_key);
    }

    private static void SerializeBaseId(IWriter a_writer, object a_key)
    {
        BaseId s = (BaseId)a_key;
        s.Serialize(a_writer);
    }

    public void WriteValue(IWriter a_writer, object a_value)
    {
        SerializeBaseId(a_writer, a_value);
    }
}

