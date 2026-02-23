using PT.APSCommon;

namespace PT.Transmissions;

public class PerformExportBaseT : PTTransmission, IPTSerializable
{
    public PerformExportBaseT() { }

    #region IPTSerializable Members
    protected PerformExportBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 179)
        {
            exportingInstigator = new BaseId(reader);
            reader.Read(out exportingConnectinNbr);
            reader.Read(out scenarioName);
        }

        #region Version 1
        else
        {
            exportingInstigator = new BaseId(reader);
            reader.Read(out exportingConnectinNbr);
        }
        #endregion
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        exportingInstigator.Serialize(writer);
        writer.Write(exportingConnectinNbr);
        writer.Write(scenarioName);
    }

    public const int UNIQUE_ID = 594;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private BaseId exportingInstigator;

    public BaseId ExportingInstigator
    {
        get => exportingInstigator;

        set => exportingInstigator = value;
    }

    private int exportingConnectinNbr;

    public int ExportingInstigatorsConnectionNbr
    {
        get => exportingConnectinNbr;

        set => exportingConnectinNbr = value;
    }

    private string scenarioName = "";

    /// <summary>
    /// The Scenario being exported.
    /// </summary>
    public string ScenarioName
    {
        get => scenarioName;
        set => scenarioName = value;
    }
}