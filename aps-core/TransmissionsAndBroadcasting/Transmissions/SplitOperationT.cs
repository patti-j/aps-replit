using PT.APSCommon;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Transmission sent to split an Operation.
/// </summary>
public class SplitOperationT : OperationIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 438;

    #region IPTSerializable Members
    public SplitOperationT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 240)
        {
            reader.Read(out splitPercent);
            reader.Read(out newActivityCount);
            reader.Read(out maxActivityRunSpan);
            decimal nxtQty;
            int qtyCount;
            reader.Read(out qtyCount);
            for (int i = 0; i < qtyCount; i++)
            {
                reader.Read(out nxtQty);
                qtiesToSplit.Add(nxtQty);
            }

            splitSettings = new SplitSettings(reader);
        }

        #region Version 1
        else if (reader.VersionNumber >= 1)
        {
            reader.Read(out splitPercent);
            double splitQtyDeprecated;
            reader.Read(out splitQtyDeprecated);
            reader.Read(out newActivityCount);
            reader.Read(out maxActivityRunSpan);

            splitSettings = new SplitSettings(reader);
        }
        #endregion
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(splitPercent);
        writer.Write(newActivityCount);
        writer.Write(maxActivityRunSpan);
        writer.Write(qtiesToSplit.Count);
        for (int i = 0; i < qtiesToSplit.Count; i++)
        {
            writer.Write(qtiesToSplit[i]);
        }

        splitSettings.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public SplitSettings splitSettings;

    //Only one of these will be set.  The others will remain null.
    //The HowToSplit setting indicates which one to use.
    public decimal splitPercent;
    public int newActivityCount;
    public TimeSpan maxActivityRunSpan;

    private readonly List<decimal> qtiesToSplit = new ();

    /// <summary>
    /// Lists the end resulting split quantities.  Must sum to the Operation's Remaining Finish qties or a validation error will occur.
    /// </summary>
    public List<decimal> QtiesToSplit => qtiesToSplit;

    public SplitOperationT() { }

    public SplitOperationT(BaseId scenarioId, SplitSettings splitSettings, BaseId jobId, BaseId moId, BaseId opId)
        : base(scenarioId, jobId, moId, opId)
    {
        this.splitSettings = splitSettings;
    }

    public override string Description => "Operation Split";
}