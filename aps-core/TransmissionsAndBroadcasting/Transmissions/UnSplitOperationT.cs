using PT.APSCommon;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Unsplits a Split Operation.
/// </summary>
public class UnSplitOperationT : OperationIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 439;

    #region IPTSerializable Members
    public UnSplitOperationT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 419)
        {
            splitSettings = new SplitSettings(reader);
        }

        #region Version 1
        else if (reader.VersionNumber >= 1) { }
        #endregion
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        splitSettings.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public SplitSettings splitSettings;

    public UnSplitOperationT() { }

    public UnSplitOperationT(BaseId scenarioId, BaseId jobId, BaseId moId, BaseId opId, SplitSettings a_splitSettings)
        : base(scenarioId, jobId, moId, opId)
    {
        splitSettings = a_splitSettings;
    }

    public override string Description => "Unsplit Operation";
}