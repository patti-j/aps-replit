namespace PT.ERPTransmissions;

public class RefreshStagingDataStartedT : PerformImportBaseT, IPTSerializable
{
    public RefreshStagingDataStartedT() { }

    #region IPTSerializable Members
    public RefreshStagingDataStartedT(IReader reader)
        : base(reader) { }

    public new const int UNIQUE_ID = 1022;

    public override int UniqueId => UNIQUE_ID;
    #endregion
    
}