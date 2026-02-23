namespace PT.Scheduler;

/// <summary>
/// Stores the user-specific settings for Splitting Operations and Activities.
/// </summary>
[Serializable]
public class SplitSettings : ICloneable, IPTSerializable
{
    public const int UNIQUE_ID = 438;

    #region IPTSerializable Members
    public SplitSettings(IReader reader)
    {
        if (reader.VersionNumber >= 419)
        {
            reader.Read(out integerSplits);
            int val;
            reader.Read(out val);
            howToSplit = (howToSplitTypes)val;
            reader.Read(out m_expediteAfterSplit);
        }

        #region 1
        else if (reader.VersionNumber >= 1)
        {
            reader.Read(out integerSplits);
            int val;
            reader.Read(out val);
            howToSplit = (howToSplitTypes)val;
        }
        #endregion
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(integerSplits);
        writer.Write((int)howToSplit);
        writer.Write(m_expediteAfterSplit);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public enum howToSplitTypes
    {
        PercentOfQty = 0,
        ActivityCount,
        MaxActivityDuration,
        EligibleResourceCount,
        ListOfSplitQties,
        EligibleResourceByMaxVolume
    }

    public SplitSettings() { }

    #region Properties
    private howToSplitTypes howToSplit = howToSplitTypes.PercentOfQty;

    public howToSplitTypes HowToSplit
    {
        get => howToSplit;
        set => howToSplit = value;
    }

    private bool integerSplits = true;

    public bool IntegerSplits
    {
        get => integerSplits;
        set => integerSplits = value;
    }

    //Specifies if the MO should be expedited after a split or join
    private bool m_expediteAfterSplit;

    public bool ExpediteAfterSplit
    {
        get => m_expediteAfterSplit;
        set => m_expediteAfterSplit = value;
    }
    #endregion

    object ICloneable.Clone()
    {
        return Clone();
    }

    public SplitSettings Clone()
    {
        return (SplitSettings)MemberwiseClone();
    }
}