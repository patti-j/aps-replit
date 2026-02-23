namespace PT.SchedulerDefinitions;

public class TripleLongKey
{
    public TripleLongKey(long aK1_16Bit, long aK2_8bit, long aK3_8bit)
    {
        k1_16bit = aK1_16Bit;
        k2_8bit = aK2_8bit;
        k3_8bit = aK3_8bit;

        int k1_temp = (int)k1_16bit & BitHelper.BITMASK_16;
        int k2_temp = (int)k2_8bit & BitHelper.BITMASK_8;
        k2_temp <<= 16;
        int K3_temp = (int)k3_8bit & BitHelper.BITMASK_8;
        K3_temp <<= 24;
        hashcode = k1_temp | k2_temp;
        hashcode = hashcode | K3_temp;
    }

    private readonly long k1_16bit;
    private readonly long k2_8bit;
    private readonly long k3_8bit;

    private readonly int hashcode;

    public override int GetHashCode()
    {
        return hashcode;
    }

    public override bool Equals(object obj)
    {
        TripleLongKey tbk = (TripleLongKey)obj;
        if (k1_16bit != tbk.k1_16bit || k2_8bit != tbk.k2_8bit || k3_8bit != tbk.k3_8bit)
        {
            return false;
        }

        return true;
    }

    public override string ToString()
    {
        return BitHelper.GetBitString(hashcode);
    }
}