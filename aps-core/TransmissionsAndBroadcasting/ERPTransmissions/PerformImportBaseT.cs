using PT.APSCommon;

namespace PT.ERPTransmissions;

public class PerformImportBaseT : ERPTransmission, IPTSerializable
{
    public PerformImportBaseT() { }

    #region IPTSerializable Members
    protected PerformImportBaseT(IReader reader)
        : base(reader)
    {
        importingInstigator = new BaseId(reader);
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        importingInstigator.Serialize(writer);
    }

    public new const int UNIQUE_ID = 512;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private BaseId importingInstigator;

    public BaseId ImportingInstigator
    {
        get => importingInstigator;

        set => importingInstigator = value;
    }
}