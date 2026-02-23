using PT.Transmissions;

namespace PT.ERPTransmissions;

public partial class WarehouseT
{
    public partial class ItemStorageLot : PTObjectBase, IPTSerializable
    {
        public new int UniqueId => 1122;
        
        internal ItemStorageLot(IReader a_reader)
            : base(a_reader)
        {
            a_reader.Read(out m_warehouseExternalId);
            a_reader.Read(out m_itemExternalId);
            a_reader.Read(out m_storageAreaExternalId);
            a_reader.Read(out m_qty);
        }

        public override void Serialize(IWriter a_writer)
        {
            base.Serialize(a_writer);

            a_writer.Write(m_warehouseExternalId);
            a_writer.Write(m_itemExternalId);
            a_writer.Write(m_storageAreaExternalId);
            a_writer.Write(m_qty);
        }

        public ItemStorageLot(PtImportDataSet.ItemStorageLotsRow a_itemStorageLotsRow)
        {
            ExternalId = a_itemStorageLotsRow.LotExternalId;
            m_warehouseExternalId = a_itemStorageLotsRow.WarehouseExternalId;
            m_storageAreaExternalId = a_itemStorageLotsRow.StorageAreaExternalId;
            m_itemExternalId = a_itemStorageLotsRow.ItemExternalId;
            m_qty = a_itemStorageLotsRow.Qty;
        }

        private readonly string m_warehouseExternalId;
        private readonly string m_itemExternalId;
        private readonly string m_storageAreaExternalId;
        private readonly decimal m_qty;

        public string WarehouseExternalId => m_warehouseExternalId;

        public string ItemExternalId => m_itemExternalId;

        public string StorageAreaExternalId => m_storageAreaExternalId;

        public decimal Qty => m_qty;

        #if DEBUG
        public override string ToString()
        {
            return $"Warehouse External Id:{m_warehouseExternalId} | Item External Id: {m_itemExternalId} | SA External Id: {m_storageAreaExternalId} | Quantity: {m_qty}";
        }
        #endif
    }
}