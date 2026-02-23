using System.Collections;
using System.Drawing;

namespace PT.Common;

/// <summary>
/// Summary description for BinaryWriter.
/// </summary>
public class BinaryStreamWriter : IWriter
{
    protected BinaryWriter m_bw;

    public virtual void Close()
    {
        m_bw.Close();
    }

    public virtual void Flush()
    {
        m_bw.Flush();
    }

    #region IWriter Members
    #region Primitive data types.
    public void Write(sbyte data)
    {
        m_bw.Write(data);
    }

    public void Write(byte data)
    {
        m_bw.Write(data);
    }

    public void Write(short data)
    {
        m_bw.Write(data);
    }

    public void Write(ushort data)
    {
        m_bw.Write(data);
    }

    public void Write(int data)
    {
        m_bw.Write(data);
    }

    public void Write(uint data)
    {
        m_bw.Write(data);
    }

    public void Write(long data)
    {
        m_bw.Write(data);
    }

    public void Write(long? data)
    {
        m_bw.Write(data.HasValue);
        if (data.HasValue)
        {
            m_bw.Write(data.Value);
        }
    }

    public void Write(ulong data)
    {
        m_bw.Write(data);
    }

    public void Write(char data)
    {
        m_bw.Write(data);
    }

    public void Write(float data)
    {
        m_bw.Write(data);
    }

    public void Write(double data)
    {
        m_bw.Write(data);
    }

    public void Write(bool data)
    {
        m_bw.Write(data);
    }

    public void Write(decimal data)
    {
        m_bw.Write(data);
    }

    public void Write(string data)
    {
        if (data != null)
        {
            m_bw.Write(true);
            m_bw.Write(data);
        }
        else
        {
            m_bw.Write(false);
        }
    }

    public void Write(byte[] data)
    {
        if (data != null)
        {
            m_bw.Write(true);
            m_bw.Write(data.Length);
            if (data.Length > 0)
            {
                m_bw.Write(data);
            }
        }
        else
        {
            m_bw.Write(false);
        }
    }

    public void Write(decimal[] data)
    {
        if (data != null)
        {
            m_bw.Write(true);
            m_bw.Write(data.Length);
            for (int i = 0; i < data.Length; ++i)
            {
                m_bw.Write(data[i]);
            }
        }
        else
        {
            m_bw.Write(false);
        }
    }

    public void Write(string[] data)
    {
        if (data != null)
        {
            m_bw.Write(true);
            m_bw.Write(data.Length);
            for (int i = 0; i < data.Length; ++i)
            {
                Write(data[i]);
            }
        }
        else
        {
            m_bw.Write(false);
        }
    }
    #endregion Primitive data types.

    #region System structures.
    public void Write(TimeSpan data)
    {
        m_bw.Write(data.Ticks);
    }

    public void Write(DateTime data)
    {
        m_bw.Write(data.Ticks);
    }

    public void Write(DateTimeOffset data)
    {
        m_bw.Write(data.Ticks);
        m_bw.Write(data.Offset.Ticks);
    }

    public void Write(Color data)
    {
        // Storing the data in 1 int will save 3 write operations.

        // Store the alpha component in an int.
        int x = data.A;

        // Store the blue component in the int.
        x <<= Definitions.Bits.BITS_PER_BYTE;
        x |= data.B;

        // Store the green component in the int.
        x <<= Definitions.Bits.BITS_PER_BYTE;
        x |= data.G;

        // Store the green component in the int.
        x <<= Definitions.Bits.BITS_PER_BYTE;
        x |= data.R;

        m_bw.Write(x);
    }
    #endregion System structures.

    #region System classes.
    #region Exceptions
    public void Write(Exception data)
    {
        WriteException(data);
    }

    public void Write(ApplicationException data)
    {
        WriteException(data);
    }

    public void Write(Guid a_data)
    {
        Write(a_data.ToString());
    }

    private void WriteException(Exception e)
    {
        Write(e.Message);
    }
    #endregion

    public void Write(HashSet<string> a_data)
    {
        Write(a_data.Count);
        foreach (string s in a_data)
        {
            Write(s);
        }
    }

    public void Write<T>(List<T> a_data) where T : IPTSerializable
    {
        Write(a_data.Count);
        foreach (T item in a_data)
        {
            item.Serialize(this);
        }
    }

    public void WriteList(List<string> a_data)
    {
        Write(a_data.Count);
        foreach (string item in a_data)
        {
            Write(item);
        }
    }

    /// <summary>
    /// Serialize a generic HashSet that contains IPTSerializable objects.<T">
    /// 
    /// 
    /// 
    /// </summary>
    /// <typeparam name="T">The type of values stored in the HashSet</typeparam>
    /// <param name="a_data">The HashSet that contains IPTSerializable objects.</param>
    public void Write<T>(HashSet<T> a_data) where T : IPTSerializable
    {
        Write(a_data.Count);
        foreach (IPTSerializable d in a_data)
        {
            d.Serialize(this);
        }
    }

    /// <summary>
    /// Serialize generic Dictionary<Key>,Value>
    /// </summary>
    /// <typeparam name="Key">The type of the dictionary's keys.</typeparam>
    /// <typeparam name="Value">The type of the dictionary's values.</typeparam>
    /// <param name="a_serializer">A class that can deserialize both the key and value.</param>
    /// <param name="a_dict">The dictionary to serialize.</param>
    public void Write<Key, Value>(IReaderDictionaryKeyValueSerializer a_serializer, Dictionary<Key, Value> a_dict)
    {
        Write(a_dict.Count);
        foreach (KeyValuePair<Key, Value> item in a_dict)
        {
            a_serializer.WriteKey(this, item.Key);
            a_serializer.WriteValue(this, item.Value);
        }
    }
    #endregion

    #region Write boxed Primitive data types.
    public void WriteBoxedPrimitiveAndCommonSystemStructs(object data)
    {
        if (data is sbyte)
        {
            Write(IO.Constants.c_sbyteId);
            Write((sbyte)data);
        }
        else if (data is byte)
        {
            Write(IO.Constants.c_byteId);
            Write((byte)data);
        }
        else if (data is short)
        {
            Write(IO.Constants.c_shortId);
            Write((short)data);
        }
        else if (data is ushort)
        {
            Write(IO.Constants.c_ushortId);
            Write((ushort)data);
        }
        else if (data is int)
        {
            Write(IO.Constants.c_intId);
            Write((int)data);
        }
        else if (data is uint)
        {
            Write(IO.Constants.c_uintId);
            Write((uint)data);
        }
        else if (data is long)
        {
            Write(IO.Constants.c_longId);
            Write((long)data);
        }
        else if (data is ulong)
        {
            Write(IO.Constants.c_ulongId);
            Write((ulong)data);
        }
        else if (data is char)
        {
            Write(IO.Constants.c_charId);
            Write((char)data);
        }
        else if (data is float)
        {
            Write(IO.Constants.c_floatId);
            Write((float)data);
        }
        else if (data is double)
        {
            Write(IO.Constants.c_doubleId);
            Write((double)data);
        }
        else if (data is bool)
        {
            Write(IO.Constants.c_boolId);
            Write((bool)data);
        }
        else if (data is decimal)
        {
            Write(IO.Constants.c_decimalId);
            Write((decimal)data);
        }
        else if (data is string)
        {
            Write(IO.Constants.c_stringId);
            Write((string)data);
        }
        else if (data is DateTime)
        {
            Write(IO.Constants.c_dateTimeId);
            Write((DateTime)data);
        }
        else if (data is TimeSpan)
        {
            Write(IO.Constants.c_timeSpanId);
            Write((TimeSpan)data);
        }
        else
        {
            string msg = "BinaryStreamWriter.WriteBoxedPrimitive error. The type you're trying to box isn't a primitive.";
            throw new ApplicationException(msg);
        }
    }
    #endregion Primitive data types.

    #region Duplicate Errorchecking
    #if DEBUG

    private readonly ArrayList m_objectList = new ();

    #endif

    public void DuplicateErrorCheck(object o)
    {
        #if DEBUG
        if (Testing.Debug.UsePTSerializationDuplicateErrorChecking)
        {
            // It's okay for this code to be unreachable most of the time.
            // We only turn on this type of error checking once and a while.
            for (int i = 0; i < m_objectList.Count; ++i)
            {
                if (ReferenceEquals(o, m_objectList[i]))
                {
                    const string message = "The object you are attempting to serialize has already been serialized by this writer.";
                    throw new Exception(message);
                }
            }

            if (m_objectList.Contains(o))
            {
                const string message = "I don't know if this is an error or not. It will require further evaluation. ArrayList uses object.Equals not ReferenceEquals.";
                throw new Exception(message);
            }

            m_objectList.Add(o);
        }
        #endif
    }
    #endregion
    #endregion IWriter Members
}