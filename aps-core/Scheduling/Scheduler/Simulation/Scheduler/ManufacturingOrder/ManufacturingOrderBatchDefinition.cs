using PT.APSCommon;

namespace PT.Scheduler.Simulation;
[Obsolete("Class is obsolete do not use, kept around for backward compatibility")]
public class ManufacturingOrderBatchDefinition : BaseObject
{
    #region IPTSerializable Members
    public ManufacturingOrderBatchDefinition(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 0)
        {
            int val;

            a_reader.Read(out long _);
            a_reader.Read(out decimal _);

            a_reader.Read(out int _);

            //a_reader.Read(out  m_batchWindowTicks);
            //a_reader.Read(out m_maxBatchQty);

            //a_reader.Read(out val);
            //m_moBatchMethod = (SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods)val;
        }
    }

    //public override void Serialize(IWriter a_writer)
    //{
    //    base.Serialize(a_writer);

    //    a_writer.Write(m_batchWindowTicks);
    //    a_writer.Write(m_maxBatchQty);
    //    a_writer.Write((int)m_moBatchMethod);
    //}

    public new const int UNIQUE_ID = 670;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    //internal ManufacturingOrderBatchDefinition(BaseId a_id, Transmissions.ManufacturingOrderBatchDefinitionT a_t)
    //    : base(a_id)
    //{
    //    Update(a_t.m_externalId, a_t.m_name, a_t.m_notes, a_t.m_description, a_t.m_batchWindowTicks, a_t.m_maxBatchQty, a_t.m_moBatchMethod);
    //}

    ///// <summary>
    ///// This is used for maintaining CustomOrderedListOptimized, don't use to instantiate an instance for other use.
    ///// </summary>
    //internal ManufacturingOrderBatchDefinition()
    //    : base(new BaseId(0)) { }

    ///// <summary>
    ///// This constructor is used by the comparer which only required the name of definition.
    ///// Do not use to instantiate a new object.
    ///// </summary>
    ///// <param name="a_id"></param>
    ///// <param name="a_name"></param>
    //internal ManufacturingOrderBatchDefinition(BaseId a_id, string a_name)
    //    : base(a_id)
    //{
    //    Name = a_name;
    //}

    //internal void Update(Transmissions.ManufacturingOrderBatchDefinitionT a_t)
    //{
    //    Update(a_t.m_externalId, a_t.m_name, a_t.m_notes, a_t.m_description, a_t.m_batchWindowTicks, a_t.m_maxBatchQty, a_t.m_moBatchMethod);
    //}

    //private void Update(string a_externalId, string a_name, string a_notes, string a_description, long a_batchWindowsTicks, decimal a_maxNbrOfBatches, SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods a_moBatchMethod)
    //{
    //    ExternalId = a_externalId;
    //    Name = a_name;
    //    Notes = a_notes;
    //    Description = a_description;

    //    m_batchWindowTicks = a_batchWindowsTicks;
    //    m_maxBatchQty = a_maxNbrOfBatches;

    //    m_moBatchMethod = a_moBatchMethod;
    //}

    //private long m_batchWindowTicks;

    //public long BatchWindowTicks => m_batchWindowTicks;

    //public TimeSpan BatchWindow => new(m_batchWindowTicks);

    //private decimal m_maxBatchQty;

    //public decimal MaxBatchQty => m_maxBatchQty;

    //private SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods m_moBatchMethod;

    //public SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods MOBatchMethod => m_moBatchMethod;

    public override string DefaultNamePrefix => "Manufacturing Order Batch Definition";
}