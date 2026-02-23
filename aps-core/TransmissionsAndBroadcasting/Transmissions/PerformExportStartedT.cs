namespace PT.Transmissions;

public class PerformExportStartedT : PerformExportBaseT, IPTSerializable
{
    public PerformExportStartedT() { }

    #region IPTSerializable Members
    public PerformExportStartedT(IReader reader)
        : base(reader) { }

    public new const int UNIQUE_ID = 593;

    public override int UniqueId => UNIQUE_ID;
    #endregion
}