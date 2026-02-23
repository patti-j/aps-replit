namespace PT.ERPTransmissions;

public class PerformImportStartedT : PerformImportBaseT, IPTSerializable
{
    public PerformImportStartedT() { }

    #region IPTSerializable Members
    public PerformImportStartedT(IReader reader)
        : base(reader) { }

    public new const int UNIQUE_ID = 513;

    public override int UniqueId => UNIQUE_ID;
    #endregion
}