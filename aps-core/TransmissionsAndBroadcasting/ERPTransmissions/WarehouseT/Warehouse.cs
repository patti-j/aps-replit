using PT.Transmissions;

namespace PT.ERPTransmissions;

public partial class WarehouseT
{
    public class Warehouse : PTObjectBase, IPTSerializable
    {
        public new const int UNIQUE_ID = 526;

        #region PT Serialization
        public Warehouse(IReader a_reader)
            : base(a_reader)
        {
            if (a_reader.VersionNumber >= 12526)
            {
                a_reader.Read(out m_storageCapacity);
                a_reader.Read(out m_storageCapacitySet);

                a_reader.Read(out int inventoryCount);
                for (int i = 0; i < inventoryCount; i++)
                {
                    Add(new Inventory(a_reader));
                }

                a_reader.Read(out int storageAreaCount);
                for (int i = 0; i < storageAreaCount; i++)
                {
                    Add(new StorageArea(a_reader));
                }

                a_reader.Read(out int storageAreaConnectorCount);
                for (int i = 0; i < storageAreaConnectorCount; i++)
                {
                    Add(new StorageAreaConnector(a_reader));
                }

                a_reader.Read(out int plantCount);
                for (int i = 0; i < plantCount; i++)
                {
                    a_reader.Read(out string plantExternalId);
                    suppliedPlants.Add(plantExternalId);
                }

                a_reader.Read(out m_annualPercentageRate);
                a_reader.Read(out m_annualPercentageRateSet);
            }
            #region 12511
            else if (a_reader.VersionNumber >= 12511)
            {
                a_reader.Read(out m_storageCapacity);
                a_reader.Read(out m_storageCapacitySet);

                int inventoryCount;
                a_reader.Read(out inventoryCount);
                for (int i = 0; i < inventoryCount; i++)
                {
                    Add(new Inventory(a_reader));
                }

                a_reader.Read(out int storageAreaCount);
                for (int i = 0; i < storageAreaCount; i++)
                {
                    Add(new StorageArea(a_reader));
                }

                a_reader.Read(out int storageAreaConnectorCount);
                for (int i = 0; i < storageAreaConnectorCount; i++)
                {
                    Add(new StorageAreaConnector(a_reader));
                }

                a_reader.Read(out int plantCount);
                for (int i = 0; i < plantCount; i++)
                {
                    a_reader.Read(out string plantExternalId);
                    suppliedPlants.Add(plantExternalId);
                }

                a_reader.Read(out m_annualPercentageRate);
                a_reader.Read(out m_annualPercentageRateSet);
                a_reader.Read(out bool m_tankWarehouse);
                a_reader.Read(out bool m_tankWarehouseSet);
            }
            #endregion
            else
            {
                StorageArea storageArea = new StorageArea(this.ExternalId);
                Add(storageArea);
                int nbrOfDocksObsolete;
                bool nbrOfDocksSetObsolete;

                #region 628
                if (a_reader.VersionNumber >= 628)
                {
                    a_reader.Read(out nbrOfDocksObsolete);
                    a_reader.Read(out nbrOfDocksSetObsolete);
                    a_reader.Read(out m_storageCapacity);
                    a_reader.Read(out m_storageCapacitySet);

                    int inventoryCount;
                    a_reader.Read(out inventoryCount);
                    for (int i = 0; i < inventoryCount; i++)
                    {
                        Add(new Inventory(a_reader));
                    }

                    int plantCount;
                    a_reader.Read(out plantCount);
                    for (int i = 0; i < plantCount; i++)
                    {
                        string plantExternalId;
                        a_reader.Read(out plantExternalId);
                        suppliedPlants.Add(plantExternalId);
                    }

                    a_reader.Read(out m_annualPercentageRate);
                    a_reader.Read(out m_annualPercentageRateSet);
                    a_reader.Read(out bool m_tankWarehouse);
                    a_reader.Read(out bool m_tankWarehouseSet);
                }
                #endregion
            }
        }
    

    public override void Serialize(IWriter writer)
        {
            base.Serialize(writer);

            writer.Write(m_storageCapacity);
            writer.Write(m_storageCapacitySet);
           
            writer.Write(InventoryCount);
            for (int i = 0; i < InventoryCount; i++)
            {
                GetInventoryByIndex(i).Serialize(writer);
            }
            
            writer.Write(StorageAreaCount);
            for (int i = 0; i < StorageAreaCount; i++)
            {
                GetStorageAreaByIndex(i).Serialize(writer);
            }

            writer.Write(StorageAreaConnectorCount);
            foreach (StorageAreaConnector storageAreaConnector in m_storageAreaConnectors)
            {
                storageAreaConnector.Serialize(writer);
            }

            writer.Write(SuppliedPlantsCount);
            for (int i = 0; i < SuppliedPlantsCount; i++)
            {
                writer.Write(GetSuppliedPlantExternalIdFromIndex(i));
            }

            writer.Write(m_annualPercentageRate);
            writer.Write(m_annualPercentageRateSet);
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        public Warehouse() { } // reqd. for xml serialization

        public Warehouse(string externalId, string name, string description, string notes, string userFields)
            : base(externalId, name, description, notes, userFields) { }

        public Warehouse(PtImportDataSet.WarehousesRow row, PtImportDataSet.LotsDataTable lotTable)
            : base(row.ExternalId, row.IsNameNull() ? null : row.Name, row.IsDescriptionNull() ? null : row.Description, row.IsNotesNull() ? null : row.Notes, row.IsUserFieldsNull() ? null : row.UserFields)
        {
            if (!row.IsStorageCapacityNull())
            {
                StorageCapacity = row.StorageCapacity;
            }

            if (!row.IsAnnualPercentageRateNull())
            {
                AnnualPercentageRate = row.AnnualPercentageRate;
            }
            
            AddInventories(row);
            AddStorageAreas(row);
            AddStorageAreaConnectors(row);
            AddSuppliedPlants(row);
        }

        /// <summary>
        /// Given a WarehouseRow, adds Inventories associated with it to this Warehouse.
        /// </summary>
        /// <param name="a_warehouseRow"></param>
        private void AddInventories(PtImportDataSet.WarehousesRow a_warehouseRow)
        {
            try
            {
                PtImportDataSet.InventoriesRow[] inventories = a_warehouseRow.GetInventoriesRows();

                for (int i = 0; i < inventories.Length; i++)
                {
                    PtImportDataSet.InventoriesRow invRow = (PtImportDataSet.InventoriesRow)inventories.GetValue(i);
                    Inventory inv = new Inventory(invRow);
                    Add(inv);
                }
            }
            catch (APSCommon.PTValidationException ptErr)
            {
                throw ptErr;
            }
            catch (Exception err)
            {
                throw new APSCommon.PTValidationException("2847", err, false, new object[] { a_warehouseRow.ExternalId });
            }
        }
        /// <summary>
        /// Given a WarehouseRow, adds StorageAreas associated with it to this Warehouse.
        /// </summary>
        /// <param name="a_warehouseRow"></param>
        private void AddStorageAreas(PtImportDataSet.WarehousesRow a_warehouseRow)
        {
            try
            {
                PtImportDataSet.StorageAreasRow[] storageAreaRows = a_warehouseRow.GetStorageAreasRows();
                foreach (PtImportDataSet.StorageAreasRow storageAreaRow in storageAreaRows)
                {
                    StorageArea storageArea = new StorageArea(storageAreaRow);
                    Add(storageArea);
                }

            }
            catch (APSCommon.PTValidationException ptErr)
            {
                throw ptErr;
            }
            catch (Exception err)
            {
                throw new APSCommon.PTValidationException("2847", err, false, new object[] { a_warehouseRow.ExternalId });
            }
        }

        /// <summary>
        /// Given a WarehouseRow, adds StorageAreaConnectors associated with it to this Warehouse.
        /// </summary>
        /// <param name="a_warehouseRow"></param>
        private void AddStorageAreaConnectors(PtImportDataSet.WarehousesRow a_warehouseRow)
        {
            try
            {
                PtImportDataSet.StorageAreaConnectorRow[] storageAreaConnectorRows = a_warehouseRow.GetStorageAreaConnectorRows();
                foreach (PtImportDataSet.StorageAreaConnectorRow storageAreaConnectorRow in storageAreaConnectorRows)
                {
                    StorageAreaConnector storageAreaConnector = new StorageAreaConnector(storageAreaConnectorRow);
                    Add(storageAreaConnector);
                }
            }
            catch (APSCommon.PTValidationException ptErr)
            {
                throw ptErr;
            }
            catch (Exception err)
            {
                throw new APSCommon.PTValidationException("2847", err, false, new object[] { a_warehouseRow.ExternalId });
            }
        }

        /// <summary>
        /// Add Plant Associations
        /// </summary>
        /// <param name="a_warehouseRow"></param>
        private void AddSuppliedPlants(PtImportDataSet.WarehousesRow a_warehouseRow)
        {
            try
            {
                PtImportDataSet.SuppliedPlantsRow[] plants = a_warehouseRow.GetSuppliedPlantsRows();
                for (int i = 0; i < plants.Length; i++)
                {
                    PtImportDataSet.SuppliedPlantsRow plantRow = (PtImportDataSet.SuppliedPlantsRow)plants.GetValue(i);
                    AddSuppliedPlant(plantRow.PlantExternalId);
                }
            }
            catch (APSCommon.PTValidationException ptErr)
            {
                throw ptErr;
            }
            catch (Exception err)
            {
                throw new APSCommon.PTValidationException("2845", err, false, new object[] { a_warehouseRow.ExternalId });
            }
        }

        #region Shared Properties
        private decimal m_storageCapacity;

        /// <summary>
        /// Can be used by a Scheduling Add-In to limit scheduling based upon availability of storage space.
        /// </summary>
        public decimal StorageCapacity
        {
            get => m_storageCapacity;
            set
            {
                m_storageCapacity = value;
                m_storageCapacitySet = true;
            }
        }

        private bool m_storageCapacitySet;

        public bool StorageCapacitySet => m_storageCapacitySet;

        private bool m_annualPercentageRateSet;

        public bool AnnualPercentageRateSet => m_annualPercentageRateSet;

        private decimal m_annualPercentageRate = 10;

        /// <summary>
        /// APR for calculating carring cost.
        /// </summary>
        public decimal AnnualPercentageRate
        {
            get => m_annualPercentageRate;
            set
            {
                m_annualPercentageRate = value;
                m_annualPercentageRateSet = true;
            }
        }
        #endregion Shared Properties

        #region Supplied Plants
        private readonly List<string> suppliedPlants = new ();

        public int SuppliedPlantsCount => suppliedPlants.Count;

        public void AddSuppliedPlant(string plantExternalId)
        {
            suppliedPlants.Add(plantExternalId);
        }

        public string GetSuppliedPlantExternalIdFromIndex(int index)
        {
            return suppliedPlants[index];
        }

        public bool ContainsPlant(string plantExternalId)
        {
            return suppliedPlants.Contains(plantExternalId);
        }
        #endregion

        #region Storage Area List
        private readonly List<StorageArea> m_storageAreas = new ();

        public int StorageAreaCount => m_storageAreas.Count;

        public StorageArea GetStorageAreaByIndex(int index)
        {
            return m_storageAreas[index];
        }
        public void Add(StorageArea a_storageArea)
        {
            m_storageAreas.Add(a_storageArea);
        }
        #endregion
        #region Storage Area Connector List
        private readonly List<StorageAreaConnector> m_storageAreaConnectors = new ();

        public int StorageAreaConnectorCount => m_storageAreaConnectors.Count;

        public StorageAreaConnector GetStorageAreaConnectorByIndex(int index)
        {
            return m_storageAreaConnectors[index];
        }

        public void Add(StorageAreaConnector a_storageAreaConnector)
        {
            m_storageAreaConnectors.Add(a_storageAreaConnector);
        }
        #endregion
        #region Inventory List
        private readonly List<Inventory> inventories = new();

        public int InventoryCount => inventories.Count;

        public Inventory GetInventoryByIndex(int index)
        {
            return inventories[index];
        }

        public void Add(Inventory inventory)
        {
            inventories.Add(inventory);
        }
        #endregion

    }
}