using PT.APSCommon.Extensions;

namespace PT.Scheduler;

public class ShelfLifeRequirement : IEquatable<ShelfLifeRequirement>
{
    #region Serialization
    internal ShelfLifeRequirement(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12048) //The m_nonConstraint change in V11 for new V12 versions
        {
            a_reader.Read(out m_minRemainingShelfLife);
            a_reader.Read(out m_minAgeTicks);
            a_reader.Read(out m_nonConstraint);
        }
        else if (a_reader.VersionNumber >= 12000) //For v12 backwards compatibility
        {
            a_reader.Read(out m_minRemainingShelfLife);
            a_reader.Read(out m_minAgeTicks);
        }
        else if (a_reader.VersionNumber >= 755) //m_nonConstraint change in V11
        {
            a_reader.Read(out m_minRemainingShelfLife);
            a_reader.Read(out m_minAgeTicks);
            a_reader.Read(out m_nonConstraint);
        }
        else if (a_reader.VersionNumber >= 698)
        {
            a_reader.Read(out m_minRemainingShelfLife);
            a_reader.Read(out m_minAgeTicks);
        }
    }

    /// <summary>
    /// Default non constraint
    /// </summary>
    internal ShelfLifeRequirement()
    {
        m_minRemainingShelfLife = 0;
        m_minAgeTicks = 0;
        m_nonConstraint = true;
    }

    /// <summary>
    /// Create a shelfLifeRequirement that has both a minimum shelf life requirement and a minimum age requirement.
    /// </summary>
    /// <param name="a_MinRemainingShelfLife">The minimum length of remaining shelf life on the part.</param>
    /// <param name="a_minAge">The minimum length of time that must have elapsed since the part was produced for the part to be eligible to satisfy this material requirement.</param>
    internal ShelfLifeRequirement(TimeSpan a_MinRemainingShelfLife, TimeSpan a_minAge, bool a_nonConstraint)
    {
        m_minRemainingShelfLife = a_MinRemainingShelfLife.Ticks;
        m_minAgeTicks = a_minAge.Ticks;
        m_nonConstraint = a_nonConstraint;
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(m_minRemainingShelfLife);
        writer.Write(m_minAgeTicks);
        writer.Write(m_nonConstraint);
    }

    public const int UNIQUE_ID = 745;

    public int UniqueId => UNIQUE_ID;
    #endregion

    private long m_minRemainingShelfLife;

    public long MinRemainingShelfLife
    {
        get => m_minRemainingShelfLife;
        private set => m_minRemainingShelfLife = value;
    }

    private readonly long m_minAgeTicks;

    /// <summary>
    /// The minimum length of time that must have elapsed since the part was produced for the part
    /// to be eligible to satisfy this material requirement.
    /// </summary>
    internal long MinAgeTicks => m_minAgeTicks;

    /// <summary>
    /// The minimum length of time that must have elapsed since the part was produced for the part
    /// to be eligible to satisfy this material requirement.
    /// </summary>
    public TimeSpan MinAge => new (m_minAgeTicks);

    private readonly bool m_nonConstraint;

    /// <summary>
    /// Whether this requirement is allowed to be used after it expires
    /// </summary>
    internal bool NonConstraint => m_nonConstraint;

    public bool isUsable(Lot a_lot)
    {
        throw new NotImplementedException();
    }

    internal class IsUsableArgs
    {
        internal IsUsableArgs(ScenarioDetail a_sd)
        {
            ScenarioDetail = a_sd;
        }

        internal ScenarioDetail ScenarioDetail { get; set; }
    }

    /// <summary>
    /// Implementation of the minimum remaining shelf life, minimum age, and material expiration.
    /// </summary>
    /// <param name="a_lot"></param>
    /// <param name="a_args"></param>
    /// <returns></returns>
    public bool IsUsable(Lot a_lot, long a_firstDemandDate)
    {
        bool minAgeTest = true;
        bool shelfLifeTest = true;
        bool expirationTest = true;
        if (a_lot.ShelfLifeData.Expirable && !m_nonConstraint)
        {
            ShelfLifeLotData shelfLife = a_lot.ShelfLifeData;
            expirationTest = shelfLife.ExpirationTicks >= a_firstDemandDate;

            long remainingShelfLife = shelfLife.ExpirationTicks - a_firstDemandDate;
            shelfLifeTest = remainingShelfLife >= MinRemainingShelfLife;
        }

        //// If this customization point is implemented, it completely overrides the this functionality.
        //if (args.ScenarioDetail.ExtensionController.RunEligibleMaterialExtension)
        //{
        //    bool? ret = args.ScenarioDetail.ExtensionController.IsLotUsable(args.ScenarioDetail.SimClock, shelfLife.ExpirationTicks, MinRemainingShelfLife, a_lot, args.ScenarioDetail);
        //    if (ret.HasValue)
        //    {
        //        return ret.Value;
        //    }
        //}

        if (MinAgeTicks > 0)
        {
            long age = a_firstDemandDate - a_lot.ProductionTicks;

            if (age < MinAgeTicks)
            {
                minAgeTest = false;
            }
        }

        return minAgeTest && shelfLifeTest && expirationTest;
    }

    /// <summary>
    /// Shelf Life controlled lots currently don't discriminate based on lot eligibility.
    /// </summary>
    /// <param name="a_lot"></param>
    /// <param name="a_data"></param>
    /// <returns></returns>
    public bool IsLotElig(Lot a_lot, object a_data)
    {
        return true;
    }

    /// <summary>
    /// Whether the MaterialRequirement must be supplied by the lots specified as Eligible Lots.Since shelf life requirements
    /// don't use eligible lots, this function always returns false.
    /// </summary>
    public bool MustUseEligLot => false;

    public bool ContainsEligibleLot(string a_lotCode)
    {
        // It would have been better to separate this function into its own interface, except
        // this is the only case where IUsability is used but this function doesn't need to 
        // be defined; this is the 1 of 2 special case where this function isn't necessary. The other is WearRequirement.
        throw new Exception("This function should never be called on class ShelfLifeRequirement.".Localize());
    }

    public bool Equals(ShelfLifeRequirement a_other)
    {
        if (a_other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, a_other))
        {
            return true;
        }

        return m_minRemainingShelfLife == a_other.m_minRemainingShelfLife && m_minAgeTicks == a_other.m_minAgeTicks && m_nonConstraint == a_other.m_nonConstraint;
    }

    public override bool Equals(object a_obj)
    {
        if (a_obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, a_obj))
        {
            return true;
        }

        if (a_obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((ShelfLifeRequirement)a_obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(m_minRemainingShelfLife, m_minAgeTicks, m_nonConstraint);
    }
}