using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

public class ScenarioDetailJoinJobOrMOT : ScenarioIdBaseT, IPTSerializable
{
    #region IPTSerializable Members
    public ScenarioDetailJoinJobOrMOT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            BlockKey = new BlockKey(reader);
            ResourceKey = new ResourceKey(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        BlockKey.Serialize(writer);
        ResourceKey.Serialize(writer);
    }

    public const int UNIQUE_ID = 584;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailJoinJobOrMOT() { }

    /// <summary>
    /// Split an MO at the start or end of the cycle determined by the time passed in.
    /// </summary>
    /// <param name="aScenarioId"></param>
    /// <param name="aBlockKey">The block that was clicked.</param>
    /// <param name="aResourceKey">The id of the machine that contains the block that was clicked.</param>
    /// <param name="aDate">
    /// The date that determines where the split should occur. This is rounded either to the clicked cycle or the end of the clicked cycle based on the splitFromStartOfClickedCycle
    /// parameter.
    /// </param>
    /// <param name="aSplitFromStartOfCycle">If true, everything on or after the clicked cycle will be split. Otherwise the split will occur at the end of the clicked cycle.</param>
    public ScenarioDetailJoinJobOrMOT(BaseId aScenarioId, BlockKey aBlockKey, ResourceKey aResourceKey)
        : base(aScenarioId)
    {
        BlockKey = aBlockKey;
        ResourceKey = aResourceKey;
    }

    private BlockKey blockKey;

    public BlockKey BlockKey
    {
        get => blockKey;

        private set => blockKey = value;
    }

    private ResourceKey resourceKey;

    public ResourceKey ResourceKey
    {
        get => resourceKey;

        private set => resourceKey = value;
    }

    public override string Description => "Jobs or ManufacturingOrders joined";
}