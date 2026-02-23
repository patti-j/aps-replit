using PT.APSCommon;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Transmission marketing Jobs as Printed or NotPrinted.
/// </summary>
public class JobsPrintedT : JobBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 657;

    #region IPTSerializable Members
    public JobsPrintedT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            jobs = new BaseIdList(reader);
            reader.Read(out markedAsPrinted);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        jobs.Serialize(writer);
        writer.Write(markedAsPrinted);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public JobsPrintedT() { }

    public JobsPrintedT(BaseId scenarioId, BaseIdList jobs, bool aMarkedAsPrinted)
        : base(scenarioId)
    {
        this.jobs = jobs;
        markedAsPrinted = aMarkedAsPrinted;
    }

    private readonly BaseIdList jobs;

    public BaseIdList Jobs => jobs;

    private readonly bool markedAsPrinted;

    /// <summary>
    /// If true then Jobs were marked as Printed.  Else they were marked as Not Printed.
    /// </summary>
    public bool MarkedAsPrinted => markedAsPrinted;

    public override string Description => "Jobs printed";
}