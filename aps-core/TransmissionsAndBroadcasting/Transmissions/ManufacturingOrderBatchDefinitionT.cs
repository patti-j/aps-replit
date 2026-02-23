using PT.APSCommon;

namespace PT.Transmissions;

public class ManufacturingOrderBatchDefinitionT : IPTSerializable
{
    #region IPTSerializable
    public ManufacturingOrderBatchDefinitionT(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            int val;

            m_id = new BaseId(a_reader);

            a_reader.Read(out m_externalId);
            a_reader.Read(out m_name);
            a_reader.Read(out m_notes);
            a_reader.Read(out m_description);

            a_reader.Read(out m_batchWindowTicks);
            a_reader.Read(out m_maxBatchQty);

            a_reader.Read(out val);
            m_moBatchMethod = (SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods)val;
        }
    }

    public void Serialize(IWriter a_writer)
    {
        m_id.Serialize(a_writer);

        a_writer.Write(m_externalId);
        a_writer.Write(m_name);
        a_writer.Write(m_notes);
        a_writer.Write(m_description);

        a_writer.Write(m_batchWindowTicks);
        a_writer.Write(m_maxBatchQty);

        a_writer.Write((int)m_moBatchMethod);
    }

    public const int UNIQUE_ID = 673;

    public int UniqueId => UNIQUE_ID;
    #endregion

    public ManufacturingOrderBatchDefinitionT(
        BaseId a_id,
        string a_externalId,
        string a_name,
        string a_notes,
        string a_description,
        long a_batchWindowTicks,
        decimal a_maxNbrBatches,
        SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods a_moBatchMethod)
    {
        m_id = a_id;

        m_externalId = a_externalId;
        m_name = a_name;
        m_notes = a_notes;
        m_description = a_description;

        m_batchWindowTicks = a_batchWindowTicks;
        m_maxBatchQty = a_maxNbrBatches;
        m_moBatchMethod = a_moBatchMethod;
    }

    public readonly BaseId m_id;

    public readonly string m_externalId;
    public readonly string m_name;
    public readonly string m_notes;
    public readonly string m_description;

    public readonly long m_batchWindowTicks;
    public readonly decimal m_maxBatchQty;
    public readonly SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods m_moBatchMethod;
}