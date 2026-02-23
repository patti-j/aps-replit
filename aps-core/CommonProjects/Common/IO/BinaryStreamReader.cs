using System.Drawing;

using PT.Common.IO;

namespace PT.Common;

public abstract class BinaryStreamReader : IReader
{
    protected BinaryReader m_br;

    public virtual void Close()
    {
        m_br.Close();
    }

    #region IReader Members
    #region Version
    protected int m_versionNumber;
    protected int m_assemblyVersionMajor;
    protected int m_assemblyVersionMinor;
    protected int m_assemblyVersionHotfix;
    protected int m_assemblyVersionRevision;

    public int VersionNumber => m_versionNumber;

    public int AssemblyVersionMajor => m_assemblyVersionMajor;
    public int AssemblyVersionMinor => m_assemblyVersionMinor;
    public int AssemblyVersionHotfix => m_assemblyVersionHotfix;
    public int AssemblyVersionRevision => m_assemblyVersionRevision;

    public string AssemblyVersion => $"{AssemblyVersionMajor}.{AssemblyVersionMinor}.{AssemblyVersionHotfix}.{AssemblyVersionRevision}";
    #endregion

    #region Primitive data types.
    public void Read(out sbyte o_data)
    {
        o_data = m_br.ReadSByte();
    }

    public void Read(out byte o_data)
    {
        o_data = m_br.ReadByte();
    }

    public void Read(out short o_data)
    {
        o_data = m_br.ReadInt16();
    }

    public void Read(out ushort o_data)
    {
        o_data = m_br.ReadUInt16();
    }

    public void Read(out int o_data)
    {
        o_data = m_br.ReadInt32();
    }

    public void Read(out uint o_data)
    {
        o_data = m_br.ReadUInt32();
    }

    public void Read(out long o_data)
    {
        o_data = m_br.ReadInt64();
    }

    public void Read(out long? o_data)
    {
        bool hasValue = m_br.ReadBoolean();
        if (hasValue)
        {
            o_data = m_br.ReadInt64();
        }
        else
        {
            o_data = null;
        }
    }

    public void Read(out ulong o_data)
    {
        o_data = m_br.ReadUInt64();
    }

    public void Read(out char o_data)
    {
        o_data = m_br.ReadChar();
    }

    public void Read(out float o_data)
    {
        o_data = m_br.ReadSingle();
    }

    public void Read(out double o_data)
    {
        o_data = m_br.ReadDouble();
    }

    /// <summary>
    /// Read a value stored as a double  as a decimal.
    /// This is useful when converting doule values to decimals.
    /// </summary>
    /// <param name="o_data"></param>
    public void ReadDateTimeToOffsetConversion(out DateTimeOffset o_data)
    {
        if (VersionNumber >= 12017)
        {
            Read(out o_data);
        }
        else
        {
            long dateTimeTicks = m_br.ReadInt64();
            o_data = new DateTimeOffset(dateTimeTicks, TimeSpan.Zero);
        }
    }

    public void Read(out bool o_data)
    {
        o_data = m_br.ReadBoolean();
    }

    public void Read(out decimal o_data)
    {
        o_data = m_br.ReadDecimal();
    }

    public void Read(out string o_data)
    {
        bool dataExists;
        Read(out dataExists);
        if (dataExists)
        {
            o_data = m_br.ReadString();
        }
        else
        {
            o_data = null;
        }
    }

    public void Read(out byte[] o_data)
    {
        bool dataExists;
        Read(out dataExists);
        if (dataExists)
        {
            int length;
            Read(out length);
            if (length > 0)
            {
                o_data = m_br.ReadBytes(length);
            }
            else
            {
                o_data = new byte[0];
            }
        }
        else
        {
            o_data = null;
        }
    }

    public void Read(out decimal[] o_data)
    {
        bool dataExists;
        Read(out dataExists);
        if (dataExists)
        {
            int length;
            Read(out length);
            o_data = new decimal[length];

            for (int i = 0; i < length; ++i)
            {
                o_data[i] = m_br.ReadDecimal();
            }
        }
        else
        {
            o_data = null;
        }
    }

    public void Read(out string[] o_data)
    {
        bool dataExists;
        Read(out dataExists);
        if (dataExists)
        {
            int length;
            Read(out length);
            o_data = new string[length];

            for (int i = 0; i < length; ++i)
            {
                Read(out o_data[i]);
            }
        }
        else
        {
            o_data = null;
        }
    }
    #endregion Primitive data types

    #region System structures.
    public void Read(out TimeSpan o_data)
    {
        long ticks = m_br.ReadInt64();
        o_data = new TimeSpan(ticks);
    }

    public void Read(out DateTime o_data)
    {
        long ticks = m_br.ReadInt64();
        o_data = new DateTime(ticks, DateTimeKind.Utc); //All serialized times should be in utc
    }

    public void Read(out DateTimeOffset o_data)
    {
        long ticks = m_br.ReadInt64();
        long offsetTicks = m_br.ReadInt64();
        #if TEST
            //All serialized times should be in utc
            if (offsetTicks != 0)
            {
                throw new DebugException($"DateTimeOffset serialized in non utc timezone. Offset was {TimeSpan.FromTicks(offsetTicks).TotalHours} hours");
            }
        #endif
        o_data = new DateTimeOffset(ticks, TimeSpan.FromTicks(offsetTicks));
    }

    public void Read(out Color o_data)
    {
        int x;
        Read(out x);

        int r = x & Definitions.Bits.BYTE_1_BITS;

        x >>= Definitions.Bits.BITS_PER_BYTE;
        int g = x & Definitions.Bits.BYTE_1_BITS;

        x >>= Definitions.Bits.BITS_PER_BYTE;
        int b = x & Definitions.Bits.BYTE_1_BITS;

        x >>= Definitions.Bits.BITS_PER_BYTE;
        int a = x & Definitions.Bits.BYTE_1_BITS;

        o_data = Color.FromArgb(a, r, g, b);
    }
    #endregion System structures.

    #region Write & Primitive data types.
    public void ReadBoxedPrimitiveAndCommonSystemStructs(out object o_data)
    {
        byte typeId;

        Read(out typeId);

        switch (typeId)
        {
            case Constants.c_sbyteId:
            {
                sbyte d;
                Read(out d);
                o_data = d;
                return;
            }

            case Constants.c_byteId:
            {
                byte d;
                Read(out d);
                o_data = d;
                return;
            }

            case Constants.c_shortId:
            {
                short d;
                Read(out d);
                o_data = d;
                return;
            }

            case Constants.c_ushortId:
            {
                ushort d;
                Read(out d);
                o_data = d;
                return;
            }

            case Constants.c_intId:
            {
                int d;
                Read(out d);
                o_data = d;
                return;
            }

            case Constants.c_uintId:
            {
                uint d;
                Read(out d);
                o_data = d;
                return;
            }

            case Constants.c_longId:
            {
                long d;
                Read(out d);
                o_data = d;
                return;
            }

            case Constants.c_ulongId:
            {
                ulong d;
                Read(out d);
                o_data = d;
                return;
            }

            case Constants.c_charId:
            {
                char d;
                Read(out d);
                o_data = d;
                return;
            }

            case Constants.c_floatId:
            {
                float d;
                Read(out d);
                o_data = d;
                return;
            }

            case Constants.c_doubleId:
            {
                double d;
                Read(out d);
                o_data = d;
                return;
            }

            case Constants.c_boolId:
            {
                bool d;
                Read(out d);
                o_data = d;
                return;
            }

            case Constants.c_decimalId:
            {
                decimal d;
                Read(out d);
                o_data = d;
                return;
            }

            case Constants.c_stringId:
            {
                string d;
                Read(out d);
                o_data = d;
                return;
            }

            case Constants.c_dateTimeId:
            {
                DateTime d;
                Read(out d);
                o_data = d;
                return;
            }

            case Constants.c_timeSpanId:
            {
                TimeSpan d;
                Read(out d);
                o_data = d;
                return;
            }
        }

        string msg = "BinaryStreamReader.ReadTypedData error. Type Id " + typeId + " isn't valid.";
        throw new ApplicationException(msg);
    }
    #endregion

    #region System classes.
    #region Exceptions
    public void Read(out Exception o_data)
    {
        string message = m_br.ReadString();
        o_data = new Exception(message);
    }

    public void Read(out ApplicationException o_data)
    {
        string message = m_br.ReadString();
        o_data = new ApplicationException(message);
    }
    #endregion

    public void Read(out HashSet<string> o_data)
    {
        o_data = new HashSet<string>();
        int count;
        Read(out count);
        for (int i = 0; i < count; ++i)
        {
            string s;
            Read(out s);
            o_data.Add(s);
        }
    }

    /// <summary>
    /// deserialize a generic dictionary.Note you must provide a class that knows how to deserialize the objects of the dictionary.
    /// </summary>
    /// <typeparam name="Key">The type of the dictionary's keys.</typeparam>
    /// <typeparam name="Value">The type of the dictionary's values.</typeparam>
    /// <param name="a_classFactory">A class that can deserialize both the key and value.</param>
    /// <param name="o_dict">The deserialized dictionary.</param>
    public void Read<Key, Value>(IReaderClassFactory a_classFactory, out Dictionary<Key, Value> o_dict) where Key : IPTSerializable where Value : IPTSerializable
    {
        o_dict = new Dictionary<Key, Value>();
        int count;
        Read(out count);
        for (int i = 0; i < count; ++i)
        {
            Key t1 = (Key)a_classFactory.Read(this);
            Value t2 = (Value)a_classFactory.Read(this);

            o_dict.Add(t1, t2);
        }
    }

    /// <summary>
    /// Deserialize a generic Dictionary<Key, Value>.
    /// </summary>
    /// <typeparam name="Key">The type of the dictionary's key</typeparam>
    /// <typeparam name="Value">The type of the value the dictionary holds.</typeparam>
    /// <param name="a_classFactory">A class capable of serizing the key and value.</param>
    /// <param name="o_dict">The dictionary to serialize.</param>
    public void Read<Key, Value>(IReaderDictionaryKeyValueSerializer a_classFactory, out Dictionary<Key, Value> o_dict)
    {
        o_dict = new Dictionary<Key, Value>();
        int count;
        Read(out count);
        for (int i = 0; i < count; ++i)
        {
            Key key = (Key)a_classFactory.ReadKey(this);
            Value val = (Value)a_classFactory.ReadValue(this);

            o_dict.Add(key, val);
        }
    }

    /// <summary>
    /// Deserialize a generic HashSet<T>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of object stored in the HashSet<></typeparam>
    /// <param name="a_classFactory"></param>
    /// <param name="o_data"></param>
    public void Read<T>(IReaderClassFactory a_classFactory, out HashSet<T> o_data)
    {
        o_data = new HashSet<T>();
        int count;
        Read(out count);
        for (int i = 0; i < count; ++i)
        {
            T t = (T)a_classFactory.Read(this);
            o_data.Add(t);
        }
    }

    public void Read(out Guid o_data)
    {
        Read(out string guid);
        o_data = Guid.Parse(guid);
    }
    #endregion

    #region LISTS
    public void ReadList(out List<string> o_data)
    {
        Read(out int count);
        o_data = new List<string>(count);
        for (int i = 0; i < count; i++)
        {
            Read(out string value);
            o_data.Add(value);
        }
    }
    #endregion
    #endregion IReader Members
}