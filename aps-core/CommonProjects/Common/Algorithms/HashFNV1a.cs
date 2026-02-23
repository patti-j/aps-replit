using System.Security.Cryptography;
using System.Text;

namespace PT.Common.Algorithms;

public class Fnv1a32 : HashAlgorithm
{
    private const uint c_fnvPrime = 16777619;

    private const uint c_fnvOffsetBasis = 2166136261;

    private uint m_hash;

    public Fnv1a32()
    {
        Reset();
    }

    public override void Initialize()
    {
        Reset();
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize)
    {
        for (int i = ibStart; i < cbSize; i++)
        {
            unchecked
            {
                m_hash ^= array[i];
                m_hash *= c_fnvPrime;
            }
        }
    }

    protected override byte[] HashFinal()
    {
        return BitConverter.GetBytes(m_hash);
    }

    private void Reset()
    {
        m_hash = c_fnvOffsetBasis;
    }

    public int ComputeHash(string a_key)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(a_key);
        byte[] hash = ComputeHash(bytes);
        return BitConverter.ToInt32(hash, 0);
    }
}

public sealed class Fnv1a64 : HashAlgorithm
{
    private const ulong FnvPrime = 1099511628211;

    private const ulong FnvOffsetBasis = 14695981039346656037;

    private ulong hash;

    public Fnv1a64()
    {
        Reset();
    }

    public override void Initialize()
    {
        Reset();
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize)
    {
        for (int i = ibStart; i < cbSize; i++)
        {
            unchecked
            {
                hash ^= array[i];
                hash *= FnvPrime;
            }
        }
    }

    protected override byte[] HashFinal()
    {
        return BitConverter.GetBytes(hash);
    }

    private void Reset()
    {
        hash = FnvOffsetBasis;
    }
}