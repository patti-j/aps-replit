using PT.APSCommon;

namespace PT.Scheduler;

internal interface ILotGeneratorObject
{
    /// <summary>
    /// Returns a BaseId to use to create a Lot. If an activity has created a lot during another simulation,
    /// the same Id will be returned, otherwise a new BaseId will be generated.
    /// </summary>
    /// <param name="a_inventoryId">The BaseId of the inventory the lot is being created for. A unique Lot Id will be created for each inventory Id.</param>
    /// <param name="a_idGen">ID Generator in case the ID isn't cached</param>
    /// <returns>Existing ID for this source, or NullId if a new ID is needed</returns>
    internal BaseId CreateLotId(BaseId a_inventoryId, IIdGenerator a_idGen);
    internal BaseId Id { get; }
}