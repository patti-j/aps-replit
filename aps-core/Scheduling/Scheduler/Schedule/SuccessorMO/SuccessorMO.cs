using PT.Common.Text;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

public class SuccessorMO : IPTSerializable, IEquatable<SuccessorMO>
{
    #region IPTSerializable Members
    public SuccessorMO(IReader a_reader, ManufacturingOrder a_predecessorMO)
    {
        m_predecessorMO = a_predecessorMO;

        a_reader.Read(out m_externalId);

        a_reader.Read(out m_successorJobExternalId);
        a_reader.Read(out m_successorManufacturingOrderExternalId);

        a_reader.Read(out m_alternatePathExternalId);
        a_reader.Read(out m_operationExternalId);

        a_reader.Read(out m_transferSpan);
        a_reader.Read(out m_usageQtyPerCycle);
    }

    public virtual void Serialize(IWriter a_writer)
    {
        a_writer.Write(ExternalId);

        a_writer.Write(SuccessorJobExternalId);
        a_writer.Write(SuccessorManufacturingOrderExternalId);

        a_writer.Write(AlternatePathExternalId);
        a_writer.Write(OperationExternalId);

        a_writer.Write(TransferSpan);
        a_writer.Write(UsageQtyPerCycle);
    }

    public const int UNIQUE_ID = 0;
    int IPTSerializable.UniqueId => UNIQUE_ID;
    #endregion

    #region Construction
    public SuccessorMO(JobT.SuccessorMO a_successorMO, ManufacturingOrder a_predecessorMO)
    {
        m_predecessorMO = a_predecessorMO;

        m_externalId = a_successorMO.ExternalId;

        SuccessorJobExternalId = a_successorMO.SuccessorJobExternalId;
        SuccessorManufacturingOrderExternalId = a_successorMO.SuccessorManufacturingOrderExternalId;

        AlternatePathExternalId = a_successorMO.AlternatePathExternalId;

        OperationExternalId = a_successorMO.OperationExternalId;

        m_transferSpan = a_successorMO.TransferSpan;

        m_usageQtyPerCycle = a_successorMO.UsageQtyPerCycle;
    }

    public SuccessorMO(string a_externalId, ManufacturingOrder a_predecessorMo, ManufacturingOrder a_successorMo, AlternatePath a_path, BaseOperation a_operation)
    {
        m_externalId = a_externalId;
        m_predecessorMO = a_predecessorMo;
        SuccessorManufacturingOrder = a_successorMo;
        if (a_path != null && a_operation != null)
        {
            AlternatePathExternalId = a_path.ExternalId;
            m_alternatePath = a_path;
            OperationExternalId = a_operation.ExternalId;
            m_operation = a_operation;
        }
    }

    public SuccessorMO(SuccessorMO a_smTemp, ManufacturingOrder a_predecessorMO)
    {
        m_predecessorMO = a_predecessorMO;

        m_externalId = a_smTemp.ExternalId;

        SuccessorJobExternalId = a_smTemp.SuccessorJobExternalId;
        SuccessorManufacturingOrderExternalId = a_smTemp.SuccessorManufacturingOrderExternalId;

        AlternatePathExternalId = a_smTemp.AlternatePathExternalId;

        OperationExternalId = a_smTemp.OperationExternalId;

        m_transferSpan = a_smTemp.TransferSpan;

        m_usageQtyPerCycle = a_smTemp.UsageQtyPerCycle;

        HasNewOrUpdatedPredecessors = a_smTemp.HasNewOrUpdatedPredecessors;
    }

    public SuccessorMO(JobDataSet.SuccessorMORow a_row)
    {
        m_externalId = a_row.ExternalId;

        SuccessorJobExternalId = a_row.SuccessorJobExternalId;
        SuccessorManufacturingOrderExternalId = a_row.SuccessorManufacturingOrderExternalId;

        AlternatePathExternalId = a_row.SuccessorPathExternalId;

        OperationExternalId = a_row.SuccessorOperationExternalId;

        m_transferSpan = TimeSpan.FromHours(a_row.TransferHrs).Ticks;

        m_usageQtyPerCycle = a_row.UsageQtyPerCycle;
    }
    #endregion

    #region Properties
    protected string m_externalId;
    public string ExternalId => m_externalId;

    protected string m_successorJobExternalId;

    /// <summary>
    /// The external id of the job the successor Manufacturing Order is in.
    /// </summary>
    public string SuccessorJobExternalId
    {
        get => m_successorJobExternalId;

        internal set => m_successorJobExternalId = value;
    }

    protected string m_successorManufacturingOrderExternalId;

    /// <summary>
    /// The external id of the successor Manufacturing Order.
    /// </summary>
    public string SuccessorManufacturingOrderExternalId
    {
        get => m_successorManufacturingOrderExternalId;

        internal set => m_successorManufacturingOrderExternalId = value;
    }

    protected string m_alternatePathExternalId;

    /// <summary>
    /// The alternate path that is constrained. The successor is only constrained when
    /// it is using this AlternatePath.
    /// This value is optional.
    /// </summary>
    public string AlternatePathExternalId
    {
        get => m_alternatePathExternalId;

        private set => m_alternatePathExternalId = value;
    }

    protected string m_operationExternalId;

    /// <summary>
    /// Specifies a specific operation in the successor MO to constrain. This is optional.
    /// If it isn't specified then the entire successor MO is constrained.
    /// </summary>
    public string OperationExternalId
    {
        get => m_operationExternalId;

        private set => m_operationExternalId = value;
    }

    protected long m_transferSpan;

    /// <summary>
    /// The successor MO is constrained by the FinishDate of predecessor MO plus this number of ticks.
    /// </summary>
    public long TransferSpan => m_transferSpan;

    protected decimal m_usageQtyPerCycle;

    /// <summary>
    /// This much of the predecessor is used by the successor in each cycle of the successor.
    /// </summary>
    public decimal UsageQtyPerCycle => m_usageQtyPerCycle;
    #endregion

    private readonly ManufacturingOrder m_predecessorMO;

    /// <summary>
    /// This value is never NULL.
    /// </summary>
    public ManufacturingOrder PredecessorMO => m_predecessorMO;

    private ManufacturingOrder m_successorManufacturingOrder;

    /// <summary>
    /// NULL if the successor Manufacturing Order could not be found. This may be the case if the order has been finished, cancelled, never existed, etc.
    /// </summary>
    public ManufacturingOrder SuccessorManufacturingOrder
    {
        get => m_successorManufacturingOrder;
        private init
        {
            m_successorManufacturingOrder = value;
            if (value != null)
            {
                m_successorJobExternalId = value.Job.ExternalId;
                m_successorManufacturingOrderExternalId = value.ExternalId;
            }
        }
    }

    private AlternatePath m_alternatePath;

    /// <summary>
    /// NULL if the successor path wasn't found. This may be the case if the path wasn't found, if the successor operation was finished, cancelled, never existed, or the path wasn't specified.
    /// </summary>
    public AlternatePath AlternatePath => m_alternatePath;

    private BaseOperation m_operation;

    /// <summary>
    /// NULL if the successor operation was not found. This may be the case if the operation wasn't found, or if the successor operation was finished, cancelled, never existed, or if the operation wasn't
    /// specified.
    /// </summary>
    public BaseOperation Operation => m_operation;

    /// <summary>
    /// This is used when processing JobTs. The value is set to specifically point out successor MOs that need to be unscheduled.
    /// </summary>
    private bool m_hasNewOrUpdatedPredecessors;

    public bool HasNewOrUpdatedPredecessors
    {
        get => m_hasNewOrUpdatedPredecessors;

        internal set => m_hasNewOrUpdatedPredecessors = value;
    }

    /// <summary>
    /// Establishes links to successor MOs.
    /// It also defines the precise details of how using or exclusing optional values are handled.
    /// </summary>
    internal void LinkSuccessorMOs(ManufacturingOrder a_predMo)
    {
        ClearLinks();

        ScenarioDetail sd = m_predecessorMO.ScenarioDetail;
        Job job = sd.JobManager.GetByExternalId(SuccessorJobExternalId);

        if (job != null)
        {
            ManufacturingOrder moTemp = job.ManufacturingOrders.GetByExternalId(SuccessorManufacturingOrderExternalId);

            if (moTemp != null)
            {
                if (!moTemp.Finished && !moTemp.Job.Cancelled)
                {
                    m_successorManufacturingOrder = moTemp;

                    if (TextUtil.Length(AlternatePathExternalId) > 0)
                    {
                        m_alternatePath = m_successorManufacturingOrder.AlternatePaths.FindByExternalId(AlternatePathExternalId);
                    }

                    if (TextUtil.Length(OperationExternalId) > 0)
                    {
                        BaseOperation opTemp = (BaseOperation)m_successorManufacturingOrder.OperationManager.OperationsHash[OperationExternalId];

                        if (opTemp != null)
                        {
                            if (opTemp.Finished)
                            {
                                ClearLinks();
                            }
                            else
                            {
                                m_operation = opTemp;
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Call this function before creating links to successors.
    /// This is crucial since it is possible the references might not be able to be reestablished.
    /// </summary>
    private void ClearLinks()
    {
        m_successorManufacturingOrder = null;
        m_alternatePath = null;
        m_operation = null;
    }

    /// <summary>
    /// Determine whether two successor MOs have the same successor key fields: SuccessorJobExternalId, SuccessorManufacturingOrderExternalId, AlternatePathExternalId, and OperationExternalId.
    /// </summary>
    /// <param name="a_obj"></param>
    /// <returns></returns>
    public bool SameSuccessorKey(object a_obj)
    {
        if (a_obj == null)
        {
            return false;
        }

        if (ReferenceEquals(this, a_obj))
        {
            return true;
        }

        if (GetType() != a_obj.GetType())
        {
            return false;
        }

        SuccessorMO tempSuc = (SuccessorMO)a_obj;

        if (SuccessorJobExternalId != tempSuc.SuccessorJobExternalId)
        {
            return false;
        }

        if (SuccessorManufacturingOrderExternalId != tempSuc.SuccessorManufacturingOrderExternalId)
        {
            return false;
        }

        if (AlternatePathExternalId != tempSuc.AlternatePathExternalId)
        {
            return false;
        }

        if (OperationExternalId != tempSuc.OperationExternalId)
        {
            return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public bool Equals(SuccessorMO a_obj)
    {
        if (!SameSuccessorKey(a_obj))
        {
            return false;
        }

        if (GetType() != a_obj.GetType())
        {
            return false;
        }

        if (ExternalId != a_obj.ExternalId)
        {
            return false;
        }

        if (UsageQtyPerCycle != a_obj.UsageQtyPerCycle)
        {
            return false;
        }

        if (TransferSpan != a_obj.TransferSpan)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Determine whether two successor MOs are Equal.
    /// </summary>
    /// <param name="a_obj"></param>
    /// <returns></returns>
    public override bool Equals(object a_obj)
    {
        if (a_obj is SuccessorMO other)
        {
            return Equals(other);
        }

        return false;
    }

    /// <summary>
    /// Determine whether two successor MOs are equal.
    /// </summary>
    /// <param name="a_obj1"></param>
    /// <param name="a_obj2"></param>
    /// <returns></returns>
    public static bool operator ==(SuccessorMO a_obj1, SuccessorMO a_obj2)
    {
        // This also handles the case where 
        if (ReferenceEquals(a_obj1, a_obj2))
        {
            return true;
        }

        return a_obj1.Equals(a_obj2);
    }

    /// <summary>
    /// Determine whether two successor MOs are not equal.
    /// </summary>
    /// <param name="a_obj1"></param>
    /// <param name="a_obj2"></param>
    /// <returns></returns>
    public static bool operator !=(SuccessorMO a_obj1, SuccessorMO a_obj2)
    {
        return !(a_obj1 == a_obj2);
    }
}