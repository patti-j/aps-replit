using PT.Transmissions;
using System.Data;

using PT.SchedulerDefinitions;

using static PT.ERPTransmissions.PtImportDataSet;

namespace PT.ERPTransmissions;
public partial class WarehouseT 
{
    public class StorageAreaConnector : PTObjectBase
    {
        public new const int UNIQUE_ID = 2019;

        #region PT Serialization
        public StorageAreaConnector(IReader a_reader)
            : base(a_reader)
        {
            if (a_reader.VersionNumber >= 12511)
            {
                m_bools = new BoolVector32(a_reader);

                a_reader.Read(out m_warehouseExternalId);

                a_reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    a_reader.Read(out string storageAreaInExtId);
                    StorageAreaConnectorIn.Add(storageAreaInExtId);
                }

                a_reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    a_reader.Read(out string storageAreaOutExtId);
                    StorageAreaConnectorOut.Add(storageAreaOutExtId);
                }

                a_reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    ResourceStorageAreaConnectorIn.Add(new ResourceKeyExternal(a_reader));
                }

                a_reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    ResourceStorageAreaConnectorOut.Add(new ResourceKeyExternal(a_reader));
                }

                a_reader.Read(out m_storageInFlowLimit);
                a_reader.Read(out m_storageOutFlowLimit);
                a_reader.Read(out m_counterFlowLimit);
            }
        }

        public override void Serialize(IWriter a_writer)
        {
            base.Serialize(a_writer);
            m_bools.Serialize(a_writer);

            a_writer.Write(m_warehouseExternalId);

            a_writer.Write(StorageAreaConnectorIn.Count);
            foreach (string storageAreaConnectorIn in StorageAreaConnectorIn)
            {
                a_writer.Write(storageAreaConnectorIn);
            }

            a_writer.Write(StorageAreaConnectorOut.Count);
            foreach (string storageAreaConnectorOut in StorageAreaConnectorOut)
            {
                a_writer.Write(storageAreaConnectorOut);
            }

            a_writer.Write(ResourceStorageAreaConnectorIn.Count);
            foreach (ResourceKeyExternal resourceStorageAreaConnectorIn in ResourceStorageAreaConnectorIn)
            {
                resourceStorageAreaConnectorIn.Serialize(a_writer);
            }

            a_writer.Write(ResourceStorageAreaConnectorOut.Count);
            foreach (ResourceKeyExternal resourceStorageAreaConnectorOut in ResourceStorageAreaConnectorOut)
            {
                resourceStorageAreaConnectorOut.Serialize(a_writer);
            }

            a_writer.Write(m_storageInFlowLimit);
            a_writer.Write(m_storageOutFlowLimit);
            a_writer.Write(m_counterFlowLimit);
        }

        private string m_warehouseExternalId;

        public string WarehouseExternalId => m_warehouseExternalId;

        private BoolVector32 m_bools;
        private const short c_autoDeleteStorageAreaConnectorInIdx = 0;

        private const short c_autoDeleteStorageAreaConnectorOutIdx = 1;

        private const short c_autoDeleteResourceStorageAreaConnectorInIdx = 2;

        private const short c_autoDeleteResourceStorageAreaConnectorOutIdx = 3;
        private const short c_counterFlowIdx = 4;
        private const short c_counterFlowSetIdx = 5;

        public bool AutoDeleteStorageAreaConnectorIn
        {
            get { return m_bools[c_autoDeleteStorageAreaConnectorInIdx]; }
            set
            {
                m_bools[c_autoDeleteStorageAreaConnectorInIdx] = value;
            }
        }

        public bool AutoDeleteStorageAreaConnectorOut
        {
            get { return m_bools[c_autoDeleteStorageAreaConnectorOutIdx]; }
            set
            {
                m_bools[c_autoDeleteStorageAreaConnectorOutIdx] = value;
            }
        }

        public bool AutoDeleteResourceStorageAreaConnectorIn
        {
            get { return m_bools[c_autoDeleteResourceStorageAreaConnectorInIdx]; }
            set
            {
                m_bools[c_autoDeleteResourceStorageAreaConnectorInIdx] = value;
            }
        }

        public bool AutoDeleteResourceStorageAreaConnectorOut
        {
            get { return m_bools[c_autoDeleteResourceStorageAreaConnectorOutIdx]; }
            set
            {
                m_bools[c_autoDeleteResourceStorageAreaConnectorOutIdx] = value;
            }
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion
        public StorageAreaConnector() { } // reqd. for xml serialization

        public StorageAreaConnector(string a_externalId, string a_name, string a_description, string a_notes)
            : base(a_externalId, a_name, a_description, a_notes, string.Empty) { }

        public StorageAreaConnector(PtImportDataSet.StorageAreaConnectorRow a_areaConnectorRow)
            : base(a_areaConnectorRow.ExternalId, a_areaConnectorRow.Name, !a_areaConnectorRow.IsDescriptionNull() ? a_areaConnectorRow.Description : string.Empty
                , !a_areaConnectorRow.IsNotesNull() ? a_areaConnectorRow.Notes : string.Empty, !a_areaConnectorRow.IsUserFieldsNull()? a_areaConnectorRow.UserFields : string.Empty)
        {
            ExternalId = a_areaConnectorRow.ExternalId;
            foreach (ResourceStorageAreaConnectorInRow resStorageConnectorInRow in a_areaConnectorRow.GetResourceStorageAreaConnectorInRows())
            {
                ResourceStorageAreaConnectorIn.Add(new ResourceKeyExternal(resStorageConnectorInRow.PlantExternalId,resStorageConnectorInRow.DepartmentExternalId,resStorageConnectorInRow.ResourceExternalId));
            }

            foreach (ResourceStorageAreaConnectorOutRow resStorageAreaConnectorOutRow in a_areaConnectorRow.GetResourceStorageAreaConnectorOutRows())
            {
                ResourceStorageAreaConnectorOut.Add(new ResourceKeyExternal(resStorageAreaConnectorOutRow.PlantExternalId,resStorageAreaConnectorOutRow.DepartmentExternalId,resStorageAreaConnectorOutRow.ResourceExternalId));
            }

            foreach (StorageAreaConnectorInRow storageAreaConnectorInRow in a_areaConnectorRow.GetStorageAreaConnectorInRows())
            {
                StorageAreaConnectorIn.Add(storageAreaConnectorInRow.StorageAreaExternalId);
            }

            foreach (StorageAreaConnectorOutRow storageAreaConnectorOutRow in a_areaConnectorRow.GetStorageAreaConnectorOutRows())
            {
                StorageAreaConnectorOut.Add(storageAreaConnectorOutRow.StorageAreaExternalId);
            }

            if (!a_areaConnectorRow.IsCounterFlowLimitNull())
            {
                CounterFlowLimit = a_areaConnectorRow.CounterFlowLimit;
            }

            if (!a_areaConnectorRow.IsCounterFlowNull())
            {
                CounterFlow = a_areaConnectorRow.CounterFlow;
            }
            
            if (!a_areaConnectorRow.IsStorageInFlowLimitNull())
            {
                StorageInFlowLimit = a_areaConnectorRow.StorageInFlowLimit;
            }

            if (!a_areaConnectorRow.IsStorageOutFlowLimitNull())
            {
                StorageOutFlowLimit = a_areaConnectorRow.StorageOutFlowLimit;
            }
        }

        public List<string> StorageAreaConnectorIn = new();
        public List<string> StorageAreaConnectorOut = new();
        public List<ResourceKeyExternal> ResourceStorageAreaConnectorIn = new();
        public List<ResourceKeyExternal> ResourceStorageAreaConnectorOut = new();

        
        /// <summary>
        /// Flag to indicate a material can flow in both directions at once.
        /// </summary>
        public bool CounterFlow
        {
            get => m_bools[c_counterFlowIdx];
            set
            {
                m_bools[c_counterFlowIdx] = value;
                m_bools[c_counterFlowSetIdx] = true;
            }
        }

        public bool CounterFlowSet => m_bools[c_counterFlowSetIdx];
        private int m_storageInFlowLimit;
        /// <summary>
        /// Indicates how many objects can store material into storage at the same time
        /// </summary>
        public int StorageInFlowLimit
        {
            get => m_storageInFlowLimit;
            set => m_storageInFlowLimit = value;
        }

        private int m_storageOutFlowLimit;
        /// <summary>
        /// Indicates how many objects can withdraw material from storage at the same time
        /// </summary>
        public int StorageOutFlowLimit
        {
            get => m_storageOutFlowLimit;
            set => m_storageOutFlowLimit = value;
        }

        private int m_counterFlowLimit;
        /// <summary>
        /// Indicates the total limit of storing and withdrawing
        /// </summary>
        public int CounterFlowLimit
        {
            get => m_counterFlowLimit;
            set => m_counterFlowLimit = value;
        }
    }

}
