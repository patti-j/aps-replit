using PT.APSCommon;
using PT.Scheduler.Schedule.Operation;

namespace PT.Scheduler;

public class TankOperation : ResourceOperation
{
    #region IPTSerializable Members
    internal TankOperation(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_reader, a_idGen) { }


    protected override ProductionInfo DeserializeProductionInfo(IReader a_reader)
    {
        ProductionInfo baseProdInfo = base.DeserializeProductionInfo(a_reader);

        if (a_reader.VersionNumber >= 408)
        {
            a_reader.Read(out long m_storagePostProcessingTicks);
            new BoolVector32(a_reader);
        }

        return baseProdInfo;
    }

    public new const int UNIQUE_ID = 742;

    public override int UniqueId => UNIQUE_ID;
    #endregion
}