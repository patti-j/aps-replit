using System.Text;

using PT.Common.Exceptions;

namespace PT.Scheduler;

/// <summary>
/// When a predecessor MO is finished the operation or ManufacturingOrder that constrains needs to store the
/// predecessor's completion information.
/// The primary interest here is when there's transfer time between the precessor and successor. In the event
/// that there is no transfer time then storing this information isn't useful.
/// </summary>
public class FinishedPredecessorMOReleaseInfo : IPTSerializable
{
    #region IPTSerializable Members
    public void Serialize(IWriter writer)
    {
        writer.Write(predecessorJobExternalId);
        writer.Write(predecessorMOExternalId);
        writer.Write(readyDateTicks);
    }

    public FinishedPredecessorMOReleaseInfo(IReader reader)
    {
        if (reader.VersionNumber >= 60)
        {
            reader.Read(out predecessorJobExternalId);
            reader.Read(out predecessorMOExternalId);
            reader.Read(out readyDateTicks);
        }
        else
        {
            throw new PTException("Deserialization version is too low for FinishedPredecessorMOReleaseInfo.");
        }
    }

    public int UniqueId =>
        // TODO:  Add FinishedPredecessorMOReleaseInfo.UniqueId getter implementation
        0;
    #endregion

    public FinishedPredecessorMOReleaseInfo(string aPredecessorJobExternalId, string aPredecessorMOExternalId, long aReadyDateTicks)
    {
        predecessorJobExternalId = aPredecessorJobExternalId;
        predecessorMOExternalId = aPredecessorMOExternalId;
        readyDateTicks = aReadyDateTicks;
    }

    private readonly string predecessorJobExternalId;

    public string PredecessorJobExternalId => predecessorJobExternalId;

    private readonly string predecessorMOExternalId;

    public string PredecessorMOExternalId => predecessorMOExternalId;

    private readonly long readyDateTicks;

    /// <summary>
    /// The date this predecessor releases the successor operation or MO.
    /// </summary>
    public long ReadyDateTicks => readyDateTicks;

    /// <summary>
    /// Whether this object and the object being comparted are for the same MO.
    /// </summary>
    /// <param name="aRI"></param>
    /// <returns></returns>
    internal bool SameMO(FinishedPredecessorMOReleaseInfo aRI)
    {
        return aRI.PredecessorJobExternalId == PredecessorJobExternalId && aRI.PredecessorMOExternalId == PredecessorMOExternalId;
    }

    public override string ToString()
    {
        StringBuilder sb = new ();
        sb.AppendFormat("FinishedPredecessorMOReleaseInfo: predecessorJobExternalId={0}; predecessorMOExternalId={1}; readyDateTicks={2}", predecessorJobExternalId, predecessorMOExternalId, readyDateTicks);
        return sb.ToString();
    }
}