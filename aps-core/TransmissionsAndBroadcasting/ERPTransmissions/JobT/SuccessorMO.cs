using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

public partial class JobT
{
    public class SuccessorMO : IPTSerializable
    {
        #region IPTSerializable Members
        public SuccessorMO(IReader reader)
        {
            reader.Read(out externalId);

            reader.Read(out successorJobExternalId);
            reader.Read(out successorManufacturingOrderExternalId);

            reader.Read(out alternatePathExternalId);

            reader.Read(out operationExternalId);

            reader.Read(out transferSpan);

            reader.Read(out usageQtyPerCycle);
        }

        void IPTSerializable.Serialize(IWriter writer)
        {
            writer.Write(ExternalId);

            writer.Write(SuccessorJobExternalId);
            writer.Write(successorManufacturingOrderExternalId);

            writer.Write(AlternatePathExternalId);

            writer.Write(OperationExternalId);

            writer.Write(TransferSpan);

            writer.Write(UsageQtyPerCycle);
        }

        public const int UNIQUE_ID = 479;

        int IPTSerializable.UniqueId => UNIQUE_ID;
        #endregion

        #region Construction
        public SuccessorMO() { }

        public SuccessorMO(string externalId, string successorJobExternalId, string successorMOExternalId)
        {
            this.externalId = externalId;

            this.successorJobExternalId = successorJobExternalId;
            successorManufacturingOrderExternalId = successorMOExternalId;
        }

        public SuccessorMO(JobDataSet.SuccessorMORow row)
        {
            externalId = row.ExternalId;
            successorJobExternalId = row.SuccessorJobExternalId;
            successorManufacturingOrderExternalId = row.SuccessorManufacturingOrderExternalId;

            if (!row.IsSuccessorPathExternalIdNull())
            {
                alternatePathExternalId = row.SuccessorPathExternalId;
            }

            if (!row.IsSuccessorOperationExternalIdNull())
            {
                operationExternalId = row.SuccessorOperationExternalId;
            }

            if (!row.IsTransferHrsNull())
            {
                transferSpan = PTDateTime.GetSafeTimeSpan(row.TransferHrs).Ticks;
            }

            if (!row.IsUsageQtyPerCycleNull())
            {
                usageQtyPerCycle = row.UsageQtyPerCycle;
            }
        }
        #endregion

        #region Shared Properties
        protected string externalId;

        [Required(true)]
        public string ExternalId => externalId;

        protected string successorJobExternalId;

        /// <summary>
        /// The external id of the job the successor Manufacturing Order is in.
        /// </summary>
        public string SuccessorJobExternalId => successorJobExternalId;

        protected string successorManufacturingOrderExternalId;

        /// <summary>
        /// The external id of the successor Manufacturing Order.
        /// </summary>
        public string SuccessorManufacturingOrderExternalId => successorManufacturingOrderExternalId;

        protected string alternatePathExternalId;

        public string AlternatePathExternalId
        {
            get => alternatePathExternalId;

            set => alternatePathExternalId = value;
        }

        protected string operationExternalId;

        public string OperationExternalId
        {
            get => operationExternalId;

            set => operationExternalId = value;
        }

        protected long transferSpan;

        public long TransferSpan
        {
            get => transferSpan;

            set => transferSpan = value;
        }

        protected decimal usageQtyPerCycle;

        public decimal UsageQtyPerCycle
        {
            get => usageQtyPerCycle;

            set => usageQtyPerCycle = value;
        }
        #endregion
    }
}