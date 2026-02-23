namespace PT.Scheduler;

public partial class WearRequirement : Simulation.ILotEligibility
{
    /// <summary>
    /// Whether material must be supplied by specific lots. Sine WareRequirements don't use Eligible Lot this function
    /// always returns true.
    /// </summary>
    /// <param name="a_lot"></param>
    /// <param name="a_data"></param>
    /// <returns></returns>
    public bool IsLotElig(Lot a_lot, object a_data)
    {
        return true;
    }

    /// <summary>
    /// Whether the material must be supplied by specific lots. Since WearRequirement don't use Eligible Lots this function
    /// always returns false.
    /// </summary>
    public bool MustUseEligLot => false;

    public bool ContainsEligibleLot(string a_lotCode)
    {
        // It would have been better to separate this function into its own interface, except
        // this is the only case where IUsability is used but this function doesn't need to 
        // be defined; this is the 1 of 2 special case where this function isn't necessary. The other is ShelfLifeRequirement.
        throw new Exception("This function should never be called on class WearRequirement.");
    }
}