using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

public class ScenarioDetailSplitJobOrMOT : ScenarioIdBaseT, IPTSerializable
{
    #region IPTSerializable Members
    public ScenarioDetailSplitJobOrMOT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 241)
        {
            BlockKey = new BlockKey(reader);
            ResourceKey = new ResourceKey(reader);

            long splitPoint;
            reader.Read(out splitPoint);
            SplitPoint = splitPoint;

            int tmp;
            reader.Read(out tmp);
            SplitScope = (JobDefs.SplitScopeEnum)tmp;

            reader.Read(out tmp);
            SplitType = (SplitTypeEnum)tmp;
        }

        #region 239
        else if (reader.VersionNumber >= 239)
        {
            BlockKey = new BlockKey(reader);
            ResourceKey = new ResourceKey(reader);

            long splitPoint;
            reader.Read(out splitPoint);
            SplitPoint = splitPoint;

            bool btmp;
            reader.Read(out btmp);

            int tmp;
            reader.Read(out tmp);
            SplitScope = (JobDefs.SplitScopeEnum)tmp;

            SplitType = SplitTypeEnum.AtClickTime;
        }
        #endregion

        #region 1
        else if (reader.VersionNumber >= 1)
        {
            BlockKey = new BlockKey(reader);
            ResourceKey = new ResourceKey(reader);

            long splitPoint;
            reader.Read(out splitPoint);
            SplitPoint = splitPoint;

            bool tmp;
            reader.Read(out tmp);

            SplitScope = JobDefs.SplitScopeEnum.MO;

            SplitType = SplitTypeEnum.AtClickTime;
        }
        #endregion
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        BlockKey.Serialize(writer);
        ResourceKey.Serialize(writer);
        writer.Write(SplitPoint);
        writer.Write((int)SplitScope);
        writer.Write((int)SplitType);
    }

    public const int UNIQUE_ID = 581;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailSplitJobOrMOT() { }

    /// <summary>
    /// Split an MO at the start or end of the cycle determined by the time passed in.
    /// </summary>
    public ScenarioDetailSplitJobOrMOT(BaseId a_scenarioId,
                                       BlockKey a_blockKey,
                                       ResourceKey a_resourceKey,
                                       JobDefs.SplitScopeEnum a_splitScope,
                                       SplitTypeEnum a_splitType,
                                       long a_value)
        : base(a_scenarioId)
    {
        BlockKey = a_blockKey;
        ResourceKey = a_resourceKey;

        SplitScope = a_splitScope;

        SplitType = a_splitType;
        SplitPoint = a_value;
    }

    public JobDefs.SplitScopeEnum SplitScope { get; private set; }

    public BlockKey BlockKey { get; private set; }

    public ResourceKey ResourceKey { get; private set; }

    public SplitTypeEnum SplitType { get; private set; }

    /// <summary>
    /// Depending on the split type, this is either the number of cycles to split off or a point in time that contains the first cycle where the split should be made.
    /// </summary>
    public long SplitPoint { get; private set; }

//#if!DEBUG
//        Move this to another class since it's used in stand alone mode by ScenarioDetail.
//#endif
    /// <summary>
    /// Used to inicate whether a value passed in for a split represents a Time or the number of cycles to split off.
    /// </summary>
    public enum SplitTypeEnum { AtClickTime, NbrOfCycles }

    public override string Description => "ManufacturingOrder split";
}