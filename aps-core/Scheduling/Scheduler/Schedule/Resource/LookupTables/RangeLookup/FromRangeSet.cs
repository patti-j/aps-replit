namespace PT.Scheduler.RangeLookup;

/// <summary>
/// This is a set of From ranges, where each from range contains 0 or more To ranges.
/// For Example
/// From        Tos
/// 11-20       11-20; 21-30; 31-40
/// 21-30       11-20; 21-30; 31-40
/// </summary>
public class FromRangeSet : SetupRangeSet<FromRange>
{
    #region IPTSerializable Members
    internal FromRangeSet(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 258)
        {
            reader.Read(out m_ptAttributeExternalId);
            reader.Read(out description);
            reader.Read(out eligibilityConstraint);
        }

        #region Version 1
        else if (reader.VersionNumber >= 1)
        {
            reader.Read(out m_ptAttributeExternalId);
            reader.Read(out description);
        }
        #endregion
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(m_ptAttributeExternalId);
        writer.Write(description);
        writer.Write(eligibilityConstraint);
    }

    public override int UniqueId => -1;
    #endregion

    private void Init(string a_attributeExternalId, string aDescription, bool aEligibilityConstraint)
    {
        m_ptAttributeExternalId = a_attributeExternalId;
        description = aDescription;
        eligibilityConstraint = aEligibilityConstraint;
    }

    internal FromRangeSet() { }

    public FromRangeSet(string a_attributeExternalId, string aDescription, bool aEligibilityConstraint)
    {
        Init(a_attributeExternalId, aDescription, aEligibilityConstraint);
    }

    internal FromRangeSet(Transmissions.SetupRangeAttributeUpdate aValues)
    {
        Init(aValues.m_ptAttributeExternalId, aValues.attributeDescription, aValues.eligibilityConstraint);

        for (int fromRangeI = 0; fromRangeI < aValues.fromRanges.Count; ++fromRangeI)
        {
            Add(new FromRange(aValues.fromRanges[fromRangeI]));
        }

        DataChangesCompleted();
    }

    internal FromRangeSet(FromRangeSet aOrig)
        : base(aOrig)
    {
        Init(aOrig.m_ptAttributeExternalId, aOrig.Description, aOrig.EligibilityConstraint);
    }

    /// <summary>
    /// Retrieves the closest range values to the attribute number.
    /// </summary>
    /// <param name="a_number">Operation Attribute Number</param>
    /// <param name="a_maxRanges">Maximum ranges to return. Limited by total ranges</param>
    /// <returns></returns>
    internal List<Tuple<decimal, decimal>> GetClosestRanges(decimal a_number, int a_maxRanges)
    {
        a_maxRanges = Math.Min(a_maxRanges, Count); //limit the ranges

        //Calulate the score for each range.
        List<Tuple<int, decimal>> rangeScoreList = new ();
        for (int i = 0; i < Count; i++)
        {
            rangeScoreList.Add(new Tuple<int, decimal>(i, CalculateRangeDistance(a_number, i)));
        }

        //Sort ranges by score
        rangeScoreList.Sort((t1, t2) => t1.Item2.CompareTo(t2.Item2));

        //Create a list of ranges with the closest score
        List<Tuple<decimal, decimal>> closestRanges = new ();
        for (int i = 0; i < a_maxRanges; i++)
        {
            closestRanges.Add(new Tuple<decimal, decimal>(this[rangeScoreList[i].Item1].Start, this[rangeScoreList[i].Item1].End));
        }

        return closestRanges;
    }

    /// <summary>
    /// Sum of the distance between start and number and end and number. This could be calculated in several different ways
    /// </summary>
    private decimal CalculateRangeDistance(decimal a_number, int a_rangeIndex)
    {
        return Math.Abs(this[a_rangeIndex].Start - a_number + (this[a_rangeIndex].End - a_number));
    }

    private string m_ptAttributeExternalId;
    public string PTAttributeExternalId => m_ptAttributeExternalId;

    internal void SetName(string aAttributeName)
    {
        m_ptAttributeExternalId = aAttributeName;
    }

    private string description;
    public string Description => description;

    private bool eligibilityConstraint;

    /// <summary>
    /// If true then all Attributes specified for an Operation which are listed in a Resource's Attribute Range table must have an Attribute Number value that is within the table's ranges.
    /// </summary>
    public bool EligibilityConstraint => eligibilityConstraint;

    public override string ToString()
    {
        return string.Format("AttributeName '{0}'; Description={1}; Contains {2} setup ranges.", PTAttributeExternalId, Description, Count);
    }

    internal override void Validate()
    {
        base.Validate();

        for (int i = 0; i < Count; ++i)
        {
            this[i].Validate();
        }
    }
}