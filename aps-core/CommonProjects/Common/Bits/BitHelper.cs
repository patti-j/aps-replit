namespace PT.Common;

public class BitHelper
{
    public static readonly int BITMASK_8 = 255;
    public static readonly int BITMASK_16 = 65535;
    public static readonly int BITMASK_24 = 16777215;
    public static readonly int BITMASK_32 = -2147483648;

    public static string GetBitString(int a_value)
    {
        System.Text.StringBuilder sb = new ();

        for (int i = 31; i >= 0; --i)
        {
            sb.Append((a_value & (1 << i)) != 0 ? '1' : '0');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Count the number of bits set in an integer.
    /// Using divide and conquer bit counting. O(n)=lg(n).
    /// From Hacker's Delight.
    /// </summary>
    /// <param name="a_x"></param>
    /// <returns></returns>
    public static int CountBitsSet(int a_x)
    {
        int x = a_x;

        // For every 2 bit pair, count the number of bits set and set the 2 bits equal to that number
        x = x - ((x >> 1) & 0x55555555);

        // Same but with 4 bits
        x = (x & 0x33333333) + ((x >> 2) & 0x33333333);

        // Count # of bits within each 8 bits. The number of &'s has been reduced by the distributive property.
        // Clears out everything except for the every other set of 4 bits, which is large enough to hold the number of bits set in 8 bits.
        x = (x + (x >> 4)) & 0x0F0F0F0F;

        x = x + (x >> 8);

        x = x + (x >> 16);

        // Zero out everything except the first 6 bits. The addition above may have left some high orders bits set.
        x = x & 0x0000003F;

        return x;
    }
}