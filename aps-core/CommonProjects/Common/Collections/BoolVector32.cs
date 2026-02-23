using PT.Common.Debugging;

namespace PT.Common;

/// <summary>
/// This is a memory efficient data structure for storing up to 32 bools. Each bool only uses 1 bit.
/// </summary>
public struct BoolVector32 : IPTSerializable, IComparable<BoolVector32>
{
    private int m_bits;

    #region IPTSerializable Members
    public BoolVector32(IReader a_reader)
    {
        a_reader.Read(out m_bits);
    }

    public BoolVector32(bool a_defaultForAllFlags)
    {
        m_bits = a_defaultForAllFlags ? 1 : 0;
    }

    public BoolVector32(BoolVector32 a_v)
    {
        m_bits = a_v.m_bits;
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_bits);
    }

    public int UniqueId =>
        // TODO:  Add BoolVector32.UniqueId getter implementation
        0;
    #endregion

    /// <summary>
    /// Set all the booleans to false.
    /// </summary>
    public void Clear()
    {
        m_bits = 0;
    }

    /// <summary>
    /// Allows you to use the index operator to get or set a boolean.
    /// The index you pass in must be: idx>=0 && idx less than or equal to 31.
    /// Usage example:
    /// BoolVector32 bv=new BoolVector32();
    /// bv[0]=true;
    /// bv[1]=false;
    /// if(bv[0])
    /// {
    /// Console.WriteLine("The first boolean is true");
    /// }
    /// </summary>
    public bool this[int a_idx]
    {
        get => (m_bits & (1 << a_idx)) != 0;

        set
        {
            if (value)
            {
                m_bits = m_bits | (1 << a_idx);
            }
            else
            {
                m_bits = m_bits & ~(1 << a_idx);
            }
        }
    }

    /// <summary>
    /// Gets the specified boolean.
    /// </summary>
    /// <param name="a_idx">0 to 31</param>
    /// <returns></returns>
    public bool GetBool(int a_idx)
    {
        return (m_bits & (1 << a_idx)) != 0;
    }

    /// <summary>
    /// Sets the specified boolean.
    /// </summary>
    /// <param name="a_idx">0 to 31</param>
    /// <param name="a_value">The value to set the boolean to.</param>
    public void SetBool(int a_idx, bool a_value)
    {
        if (a_value)
        {
            m_bits = m_bits | (1 << a_idx);
        }
        else
        {
            m_bits = m_bits & ~(1 << a_idx);
        }
    }

    /// <summary>
    /// Sets the specified boolean to true. This provides an alternate
    /// way to set a boolean that may be slightly faster than the indexer
    /// and set function. This speed improvement is only true if you
    /// don't have to decide in your code whether to call SetBool or
    /// ClearBool.
    /// </summary>
    /// <param name="a_idx">0 to 31</param>
    public void SetBool(int a_idx)
    {
        m_bits = m_bits | (1 << a_idx);
    }

    /// <summary>
    /// Sets the specfied boolean to false. This provides an alternate
    /// way to set a boolean that may be slightly faster than the indexer
    /// and set function. This speed improvement is only true if you
    /// don't have to decide in your code whether to call SetBool or
    /// ClearBool.
    /// </summary>
    /// <param name="a_idx">0 to 31</param>
    public void ClearBool(int a_idx)
    {
        m_bits = m_bits & ~(1 << a_idx);
    }

    /// <summary>
    /// Returns a string representing 32 booleans.
    /// Below is an example of what would be returned
    /// if you had set every other boolean to true.
    /// 01010101010101010101010101010101
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        System.Text.StringBuilder sb = new ();

        for (int i = 0; i < 32; ++i)
        {
            if (this[i])
            {
                sb.Append('1');
            }
            else
            {
                sb.Append('0');
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns true if any of the 32 flags are set to true.
    /// </summary>
    public bool AnyFlagsSet => m_bits != 0;

    /// <summary>
    /// Returns true if any of the flags in the specified range are set to true.
    /// </summary>
    /// <param name="a_start"></param>
    /// <param name="a_endInclusive"></param>
    /// <returns></returns>
    /// <exception cref="DebugException"></exception>
    public bool AnyFlagSetInRange(int a_start, int a_endInclusive)
    {
        if ((a_start < 0 || a_start > 31) ||
            (a_endInclusive < 0 || a_endInclusive > 31) ||
            (a_endInclusive < a_start || a_start > a_endInclusive))
        {
            #if DEBUG
            throw new DebugException("Invalid range.");
            #endif
            return false;
        }

        int count = a_endInclusive - a_start + 1;
        int bitMask = ((1 << count) - 1) << a_start;
        return (m_bits & bitMask) != 0;
    }

    public int CompareTo(BoolVector32 a_v)
    {
        return m_bits.CompareTo(a_v.m_bits);
    }

    public bool Equals(BoolVector32 a_boolVector)
    {
        return m_bits == a_boolVector.m_bits;
    }

    #if TEST
        public int ToInt()
        {
            return bits;
        }
    #endif
}