namespace PT.Scheduler.Simulation;

/// <summary>
/// Used to indicate whether a lot is eligible to satisfy some form of material requirement, such as MaterialRequirements, SalesOrderLineItems or TransferOrders.
/// This is different from IUsability in that this is only used for the Eligible Lots feature wheras IUsability is only used for WearRequirements and ShelfLifeRequirements.
/// </summary>
public interface ILotEligibility
{
    bool IsLotElig(Lot a_lot, object a_data);

    bool MustUseEligLot { get; }

    /// <summary>
    /// Whether a lot is contained within a set of eligible lots.
    /// </summary>
    /// <param name="a_lotCode"></param>
    /// <returns></returns>
    bool ContainsEligibleLot(string a_lotCode);
}