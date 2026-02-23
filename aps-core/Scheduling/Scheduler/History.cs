using PT.APSCommon;

namespace PT.Scheduler;

public class History : IPTSerializable
{
    public const int UNIQUE_ID = 617;

    #region IPTSerializable Members
    public History(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out timeStamp);
            reader.Read(out transmissionNbr);
            reader.Read(out description);
            instigator = new BaseId(reader);
        }
    }

    public virtual void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif
        writer.Write(timeStamp);
        writer.Write(transmissionNbr);
        writer.Write(description);
        instigator.Serialize(writer);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public History(Transmissions.PTTransmission t, string aDescription)
    {
        timeStamp = t.TimeStamp.Ticks;
        instigator = t.Instigator;
        transmissionNbr = t.TransmissionNbr;
        description = aDescription;
    }

    private readonly long timeStamp;

    /// <summary>
    /// The TimeStamp on the Transmission that instigated the change.  This is based on the Server's PC clock at the time the Transmission was received.
    /// </summary>
    public DateTimeOffset ServerTimeStamp => new (timeStamp, TimeSpan.Zero);

    private readonly BaseId instigator;

    /// <summary>
    /// The user or external service that initiated the action.
    /// </summary>
    public BaseId Instigator => instigator;

    private ulong transmissionNbr;

    /// <summary>
    /// If a transmission triggered the history then this is the number associated with it.
    /// </summary>
    public ulong TransmissionNbr
    {
        get => transmissionNbr;
        set => transmissionNbr = value;
    }

    private string description;

    /// <summary>
    /// Text description of what history event occurred.
    /// </summary>
    public string Description
    {
        get => description;
        set => description = value;
    }

    public override string ToString()
    {
        return string.Format("{0} UID={1} {2}", ServerTimeStamp.ToDisplayTime(), Instigator.ToString(), Description);
    }
}