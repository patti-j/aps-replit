namespace PT.Transmissions;

public class PerformExportCompletedT : PerformExportBaseT, IPTSerializable
{
    public PerformExportCompletedT() { }

    #region IPTSerializable Members
    public PerformExportCompletedT(IReader reader)
        : base(reader)
    {
        exceptions = new ApplicationExceptionList(reader);
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        exceptions.Serialize(writer);
    }

    public new const int UNIQUE_ID = 595;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private ApplicationExceptionList exceptions = new ();

    public ApplicationExceptionList Exceptions
    {
        get => exceptions;

        set => exceptions = value;
    }
}