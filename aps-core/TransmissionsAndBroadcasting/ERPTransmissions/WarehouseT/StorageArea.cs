using PT.APSCommon;
using PT.Transmissions;
using static PT.ERPTransmissions.PtImportDataSet;

namespace PT.ERPTransmissions;

public partial class WarehouseT
{
    public partial class StorageArea : PTObjectBase, IPTSerializable
    {
        internal StorageArea(IReader a_reader)
            : base(a_reader)
        {
            if (a_reader.VersionNumber >= 13001)
            {
                m_bool = new BoolVector32(a_reader);
                a_reader.Read(out m_warehouseExternalId);

                a_reader.Read(out int itemStorageCount);
                for (int i = 0; i < itemStorageCount; i++)
                {
                    Add(new ItemStorage(a_reader));
                }

                a_reader.Read(out m_storageInFlowLimit);
                a_reader.Read(out m_storageOutFlowLimit);
                a_reader.Read(out m_counterFlowLimit);

                a_reader.Read(out m_plantExternalId);
                a_reader.Read(out m_departmentExternalId);
                a_reader.Read(out m_resourceExternalId);
            }
            else if(a_reader.VersionNumber >= 12555)
            {
                m_bool = new BoolVector32(a_reader);
                a_reader.Read(out m_warehouseExternalId);

                a_reader.Read(out int itemStorageCount);
                for (int i = 0; i < itemStorageCount; i++)
                {
                    Add(new ItemStorage(a_reader));
                }

                a_reader.Read(out m_storageInFlowLimit);
                a_reader.Read(out m_storageOutFlowLimit);
                a_reader.Read(out m_counterFlowLimit);
            }
            else
            {
                m_bool = new BoolVector32(a_reader);
                a_reader.Read(out m_warehouseExternalId);

                a_reader.Read(out int itemStorageCount);
                for (int i = 0; i < itemStorageCount; i++)
                {
                    Add(new ItemStorage(a_reader));
                }
            }
        }

        public override void Serialize(IWriter a_writer)
        {
            base.Serialize(a_writer);
            m_bool.Serialize(a_writer);

            a_writer.Write(m_warehouseExternalId);
            a_writer.Write(ItemStorageCount);
            for (int i = 0; i < ItemStorageCount; i++)
            {
                GetItemStorageByIndex(i).Serialize(a_writer);
            }

            a_writer.Write(m_storageInFlowLimit);
            a_writer.Write(m_storageOutFlowLimit);
            a_writer.Write(m_counterFlowLimit);

            a_writer.Write(m_plantExternalId);
            a_writer.Write(m_departmentExternalId);
            a_writer.Write(m_resourceExternalId);
        }

        public int UniqueId => 1119;

        public StorageArea(PtImportDataSet.StorageAreasRow a_storageAreaRow)
            : base(a_storageAreaRow.ExternalId, a_storageAreaRow.Name, 
                !a_storageAreaRow.IsDescriptionNull()? a_storageAreaRow.Description : string.Empty, 
                !a_storageAreaRow.IsNotesNull() ?a_storageAreaRow.Notes : string.Empty,
                !a_storageAreaRow.IsUserFieldsNull()? a_storageAreaRow.UserFields : string.Empty)
        {
            if (!a_storageAreaRow.IsSingleItemStorageNull())
            {
                SingleItemStorage = a_storageAreaRow.SingleItemStorage;
            }

            m_warehouseExternalId = a_storageAreaRow.WarehouseExternalId;

            AddItemStorages(a_storageAreaRow);

            if (!a_storageAreaRow.IsStorageInFlowLimitNull())
            {
                StorageInFlowLimit = a_storageAreaRow.StorageInFlowLimit;
            }

            if (!a_storageAreaRow.IsStorageOutFlowLimitNull())
            {
                StorageOutFlowLimit = a_storageAreaRow.StorageOutFlowLimit;
            }

            if (!a_storageAreaRow.IsCounterFlowLimitNull())
            {
                CounterFlowLimit = a_storageAreaRow.CounterFlowLimit;
            }

            if (!a_storageAreaRow.IsConstrainInFlowNull())
            {
                ConstrainInFlow = a_storageAreaRow.ConstrainInFlow;
            }

            if (!a_storageAreaRow.IsConstrainOutFlowNull())
            {
                ConstrainOutFlow = a_storageAreaRow.ConstrainOutFlow;
            }

            if (!a_storageAreaRow.IsConstrainCounterFlowNull())
            {
                ConstrainCounterFlow = a_storageAreaRow.ConstrainCounterFlow;
            }

            if (!a_storageAreaRow.IsPlantExternalIdNull())
            {
                PlantExternalId = a_storageAreaRow.PlantExternalId;
            }

            if (!a_storageAreaRow.IsDepartmentExternalIdNull())
            {
                DepartmentExternalId = a_storageAreaRow.DepartmentExternalId;
            }

            if (!a_storageAreaRow.IsResourceExternalIdNull())
            {
                ResourceExternalId = a_storageAreaRow.ResourceExternalId;
            }

            //Validate that  Plant/Dept/Resource external IDs are either all set or none are.
            if (!(PlantExternalIdIsSet == DepartmentExternalIdIsSet && DepartmentExternalIdIsSet == ResourceExternalIdIsSet))
            {
                throw new PTValidationException("3130", [a_storageAreaRow.ExternalId, a_storageAreaRow.WarehouseExternalId]);
            }
        }

        private void AddItemStorages(StorageAreasRow a_storageAreaRow)
        {
            try
            {
                ItemStorageRow[] itemStorageRows = a_storageAreaRow.GetItemStorageRows();

                for (int i = 0; i < itemStorageRows.Length; i++)
                {
                    ItemStorageRow itemStorageRow = itemStorageRows[i];
                    ItemStorage itemStorage = new (itemStorageRow);
                    Add(itemStorage);
                }
            }
            catch (APSCommon.PTValidationException ptErr)
            {
                throw ptErr;
            }
            catch (Exception err)
            {
                throw new APSCommon.PTValidationException("3088", err, false, new object[] { a_storageAreaRow.WarehouseExternalId, a_storageAreaRow.ExternalId });
            }
        }

        private readonly string m_warehouseExternalId;
        public string WarehouseExternalId => m_warehouseExternalId;
        #region ItemStorage
        private readonly List<ItemStorage> m_itemStorage = new();

        public StorageArea(string a_externalId) : base(a_externalId, a_externalId)
        {
            m_warehouseExternalId = a_externalId;
        }

        public int ItemStorageCount => m_itemStorage.Count;

        public ItemStorage GetItemStorageByIndex(int a_index)
        {
            return m_itemStorage[a_index];
        }

        public void Add(ItemStorage a_itemStorage)
        {
            m_itemStorage.Add(a_itemStorage);
        }
        #endregion

        #region Bool Vector
        private BoolVector32 m_bool;

        private const short c_singleItemStorage = 0;
        private const short c_singleItemStorageIsSetIdx = 1;
        private const short c_storageInFlowLimitIsSetIdx = 2;
        private const short c_storageOutFlowLimitIsSetIdx = 3;
        private const short c_counterFlowLimitIsSetIdx = 4;

        private const short c_constraintInFlowIdx = 5;
        private const short c_constraintOutFlowIdx = 6;
        private const short c_constraintCounterFlowIdx = 7;

        private const short c_constraintInFlowIsSetIdx = 8;
        private const short c_constraintOutFlowIsSetIdx = 9;
        private const short c_constraintCounterFlowIsSetIdx = 10;
        
        private const short c_plantExternalIdIsSetIdx = 11;
        private const short c_departmentExternalIdIsSetIdx = 12;
        private const short c_resourceExternalIdIsSetIdx = 13;

        #endregion

        public bool SingleItemStorage
        {
            get => m_bool[c_singleItemStorage];
            set
            {
                m_bool[c_singleItemStorage] = value;
                m_bool[c_singleItemStorageIsSetIdx] = true;
            }
        }

        public bool SingleItemStorageIsSet => m_bool[c_singleItemStorageIsSetIdx];

        private int m_storageInFlowLimit;
        /// <summary>
        /// Indicates how many objects can store material into storage at the same time
        /// </summary>
        public int StorageInFlowLimit
        {
            get => m_storageInFlowLimit;
            private set
            {
                if (value < 0)
                {
                    throw new PTValidationException("3123", new object[] { ExternalId });
                }

                m_storageInFlowLimit = value;
                m_bool[c_storageInFlowLimitIsSetIdx] = true;
            }
        }

        public bool StorageInFlowLimitIsSet => m_bool[c_storageInFlowLimitIsSetIdx];

        private int m_storageOutFlowLimit;
        /// <summary>
        /// Indicates how many objects can withdraw material from storage at the same time
        /// </summary>
        public int StorageOutFlowLimit
        {
            get => m_storageOutFlowLimit;
            private set
            {
                if (value < 0)
                {
                    throw new PTValidationException("3124", new object[] { ExternalId });
                }

                m_storageOutFlowLimit = value;
                m_bool[c_storageOutFlowLimitIsSetIdx] = true;
            }
        }

        public bool StorageOutFlowLimitIsSet => m_bool[c_storageOutFlowLimitIsSetIdx];

        private int m_counterFlowLimit;
        /// <summary>
        /// Indicates the total limit of storing and withdrawing
        /// </summary>
        public int CounterFlowLimit
        {
            get => m_counterFlowLimit;
            private set
            {
                if (value < 0)
                {
                    throw new PTValidationException("3125", new object[] { ExternalId });
                }

                m_counterFlowLimit = value;
                m_bool[c_counterFlowLimitIsSetIdx] = true;
            }
        }

        public bool CounterFlowLimitIsSet => m_bool[c_counterFlowLimitIsSetIdx];

        public bool ConstrainInFlow
        {
            get => m_bool[c_constraintInFlowIdx];
            set
            {
                m_bool[c_constraintInFlowIdx] = value;
                m_bool[c_constraintInFlowIsSetIdx] = true;
            }
        }

        public bool ConstrainInFlowIsSet => m_bool[c_constraintInFlowIsSetIdx];

        public bool ConstrainOutFlow
        {
            get => m_bool[c_constraintOutFlowIdx];
            set
            {
                m_bool[c_constraintOutFlowIdx] = value;
                m_bool[c_constraintOutFlowIsSetIdx] = true;
            }
        }

        public bool ConstrainOutFlowIsSet => m_bool[c_constraintOutFlowIsSetIdx];

        public bool ConstrainCounterFlow
        {
            get => m_bool[c_constraintCounterFlowIdx];
            set
            {
                m_bool[c_constraintCounterFlowIdx] = value; 
                m_bool[c_constraintCounterFlowIsSetIdx] = true; 
            }
        }

        public bool ConstrainCounterFlowIsSet => m_bool[c_constraintCounterFlowIsSetIdx];

        private string m_plantExternalId;
        public string PlantExternalId
        {
            get => m_plantExternalId;
            set
            {
                m_plantExternalId = value;
                m_bool[c_plantExternalIdIsSetIdx] = true;
            }
        }
        public bool PlantExternalIdIsSet => m_bool[c_plantExternalIdIsSetIdx];

        private string m_departmentExternalId;
        public string DepartmentExternalId
        {
            get => m_departmentExternalId;
            set
            {
                m_departmentExternalId = value;
                m_bool[c_departmentExternalIdIsSetIdx] = true;
            }
        }

        public bool DepartmentExternalIdIsSet => m_bool[c_departmentExternalIdIsSetIdx];

        private string m_resourceExternalId;
        public string ResourceExternalId
        {
            get => m_resourceExternalId;
            set
            {
                m_resourceExternalId = value;
                m_bool[c_resourceExternalIdIsSetIdx] = true;
            }
        }

        public bool ResourceExternalIdIsSet => m_bool[c_resourceExternalIdIsSetIdx];
    }
}