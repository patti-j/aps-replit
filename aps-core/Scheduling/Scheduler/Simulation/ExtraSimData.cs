using System.Collections;

namespace PT.Scheduler.Simulation;

/// <summary>
/// Used to help minimize the amount of extra data used during a simulation. Only memory for the data that is
/// needed is consumed instead of always consuming all of the memory all of the time (which would be the case if the
/// variable were directly declared as members of the classes that need the members).
/// You could consider changing this maxtrix to an object matrix, which could also contain byte arrays.
/// </summary>
internal class ExtraSimData
{
    protected ExtraSimData(int a_nbrOfMaxtrixElements)
    {
        ByteHelper.Test();
        m_bytes = new byte[a_nbrOfMaxtrixElements][];
        m_bytesHaveBeenSetFlags = new BitArray(a_nbrOfMaxtrixElements);
    }

    protected BitArray m_bytesHaveBeenSetFlags;

    private readonly byte[][] m_bytes = new byte[1][];

    private void TestByteArrayAccess(int a_byteMatrixIdx)
    {
        if (!m_bytesHaveBeenSetFlags[a_byteMatrixIdx])
        {
            throw new Exception("Error the byte index hasn't been set.");
        }
    }

    private void InitBytesIfNecessary(int a_byteMatrixIdx, int a_byteLength)
    {
        if (!m_bytesHaveBeenSetFlags[a_byteMatrixIdx])
        {
            m_bytes[a_byteMatrixIdx] = new byte[a_byteLength];
        }
    }

    protected int GetInt(int a_byteMatrixIdx)
    {
        TestByteArrayAccess(a_byteMatrixIdx);
        return BitConverter.ToInt32(m_bytes[a_byteMatrixIdx], 0);
    }

    protected void Set(int a_value, int a_byteMatrixIdx)
    {
        InitBytesIfNecessary(a_byteMatrixIdx, ByteHelper.c_bytesPerInt);

        byte[] bytes = BitConverter.GetBytes(a_value);
        bytes.CopyTo(m_bytes[a_byteMatrixIdx], 0);

        m_bytesHaveBeenSetFlags[a_byteMatrixIdx] = true;
    }

    protected long GetLong(int a_byteMatrixIdx)
    {
        TestByteArrayAccess(a_byteMatrixIdx);
        return BitConverter.ToInt64(m_bytes[a_byteMatrixIdx], 0);
    }

    protected void Set(long a_value, int a_byteMatrixIdx)
    {
        InitBytesIfNecessary(a_byteMatrixIdx, ByteHelper.c_bytesPerLong);

        byte[] bytes = BitConverter.GetBytes(a_value);
        bytes.CopyTo(m_bytes[a_byteMatrixIdx], 0);

        m_bytesHaveBeenSetFlags[a_byteMatrixIdx] = true;
    }

    protected float GetFloat(int a_byteMatrixIdx)
    {
        TestByteArrayAccess(a_byteMatrixIdx);
        return BitConverter.ToSingle(m_bytes[a_byteMatrixIdx], 0);
    }

    protected void Set(float a_value, int a_byteMatrixIdx)
    {
        InitBytesIfNecessary(a_byteMatrixIdx, ByteHelper.c_bytesPerFloat);

        byte[] bytes = BitConverter.GetBytes(a_value);
        bytes.CopyTo(m_bytes[a_byteMatrixIdx], 0);
    }

    protected double GetDouble(int a_byteMatrixIdx)
    {
        TestByteArrayAccess(a_byteMatrixIdx);
        return BitConverter.ToDouble(m_bytes[a_byteMatrixIdx], 0);
    }

    protected void Set(double a_value, int a_byteMatrixIdx)
    {
        InitBytesIfNecessary(a_byteMatrixIdx, ByteHelper.c_bytesPerDouble);

        byte[] bytes = BitConverter.GetBytes(a_value);
        bytes.CopyTo(m_bytes[a_byteMatrixIdx], 0);
    }

    protected decimal GetDecimal(int a_byteMatrixIdx)
    {
        return (decimal)GetDouble(a_byteMatrixIdx);
    }

    protected void Set(decimal a_value, int a_byteMatrixIdx)
    {
        InitBytesIfNecessary(a_byteMatrixIdx, ByteHelper.c_bytesPerDouble);
        Set((double)a_value, a_byteMatrixIdx);
    }
}