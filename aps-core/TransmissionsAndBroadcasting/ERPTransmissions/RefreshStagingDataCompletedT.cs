namespace PT.ERPTransmissions;

public class RefreshStagingDataCompletedT : PerformImportBaseT, IPTSerializable
{
    public RefreshStagingDataCompletedT() { }

    #region IPTSerializable Members
    public RefreshStagingDataCompletedT(IReader reader)
        : base(reader)
    {
        exceptions = new Transmissions.ApplicationExceptionList(reader);
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        exceptions.Serialize(writer);
    }

    public new const int UNIQUE_ID = 1023;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private Transmissions.ApplicationExceptionList exceptions = new ();

    public Transmissions.ApplicationExceptionList Exceptions
    {
        get => exceptions;

        set => exceptions = value;
    }

    public override string Description
    {
        get
        {
            if (exceptions.Count > 0)
            {
                return "Refresh Staging Data Failed";
            }

            return "Refresh Staging Data";
        }
    }
    
}