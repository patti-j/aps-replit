namespace PT.Common;

public class Definitions
{
    #region Construction
    private Definitions() { }
    #endregion

    public class Bits
    {
        public static readonly int BITS_PER_BYTE = 8;
        public static readonly int BYTE_1_BITS = (1 << BITS_PER_BYTE) - 1;
        public static readonly int BYTE_2_BITS = BYTE_1_BITS << BITS_PER_BYTE;
        public static readonly int BYTE_3_BITS = BYTE_2_BITS << BITS_PER_BYTE;
        public static readonly int BYTE_4_BITS = BYTE_3_BITS << BITS_PER_BYTE;
    }
}