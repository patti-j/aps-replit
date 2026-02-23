using PT.APSCommon;

namespace PT.ERPTransmissions;

/// <summary>
/// A transmission that acts as a container for other types of ERP transmissions.
/// </summary>
public class ImportT : ERPMaintenanceTransmission<ERPTransmission>, IPTSerializable
{
    public new const int UNIQUE_ID = 402;

    #region PT Serialization
    public ImportT(IReader reader, ObjectCreatorDelegate a_deserializeDelegate)
        : base(reader)
    {
        if (reader.VersionNumber >= 12506)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                Add((ERPTransmission)a_deserializeDelegate(reader));
            }

            m_scenarioId = new BaseId(reader);
            reader.Read(out m_configId);
        }
        else if (reader.VersionNumber >= 641)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                Add((ERPTransmission)a_deserializeDelegate(reader));
            }

            m_scenarioId = new BaseId(reader);
        }
        else if (reader.VersionNumber >= 485)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                Add((ERPTransmission)a_deserializeDelegate(reader));
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(Count);
        for (int c = 0; c < Count; c++)
        {
            writer.Write(this[c].UniqueId);
            this[c].Serialize(writer);
        }

        m_scenarioId.Serialize(writer);
        writer.Write(m_configId);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    //Todo Ensure new ImportT's handle m_configId
    public ImportT() { }

    //Note this architecture is odd because it is using a scenario id on a erptransmission base. Ideally ERP tranmissions should be merged to have a scenario id
    private BaseId m_scenarioId = BaseId.NULL_ID;

    public BaseId SpecificScenarioId
    {
        get => m_scenarioId;
        set => m_scenarioId = value;
    }

    private int m_configId = -1; //Default for Imports that don't use V2Configs

    public int SpecificConfigId 
    {
        get => m_configId;
        set => m_configId = value;
    }

    public bool UseSpecificScenarioId => m_scenarioId != BaseId.NULL_ID;
    public bool UseSpecificConfigId => m_configId != -1;

    public override string Description => "Data Imported";
}