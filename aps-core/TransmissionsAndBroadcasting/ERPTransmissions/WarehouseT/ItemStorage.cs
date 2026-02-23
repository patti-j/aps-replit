using PT.Transmissions;
using static PT.ERPTransmissions.PtImportDataSet;

namespace PT.ERPTransmissions;

public partial class WarehouseT
{
    public partial class ItemStorage: PTObjectBase, IPTSerializable
    {
        public new int UniqueId => 1120;

        internal ItemStorage(IReader a_reader)
            : base(a_reader)
        {
            if (a_reader.VersionNumber >= 12527)
            {
                m_bools = new BoolVector32(a_reader);
                a_reader.Read(out m_warehouseExternalId);
                a_reader.Read(out m_itemExternalId);
                a_reader.Read(out m_storageAreaExternalId);
                a_reader.Read(out m_maxQty);
                a_reader.Read(out m_disposalQty);
            }
            else
            {
                m_bools = new BoolVector32(a_reader);
                a_reader.Read(out m_warehouseExternalId);
                a_reader.Read(out m_itemExternalId);
                a_reader.Read(out m_storageAreaExternalId);
                a_reader.Read(out m_maxQty);
                a_reader.Read(out m_disposalQty);

                a_reader.Read(out int itemStorageLotCount);
                for (int i = 0; i < itemStorageLotCount; i++)
                {
                    _ = new ItemStorageLot(a_reader);
                }
            }
        }

        public override void Serialize(IWriter a_writer)
        {
            base.Serialize(a_writer);
            m_bools.Serialize(a_writer);

            a_writer.Write(m_warehouseExternalId);
            a_writer.Write(m_itemExternalId);
            a_writer.Write(m_storageAreaExternalId);
            a_writer.Write(m_maxQty);
            a_writer.Write(m_disposalQty);
        }

        public ItemStorage(PtImportDataSet.ItemStorageRow a_itemStorageRow)
            : base(string.Empty, string.Empty, string.Empty, string.Empty,!a_itemStorageRow.IsUserFieldsNull()? a_itemStorageRow.UserFields: string.Empty)
        {
            m_warehouseExternalId = a_itemStorageRow.WarehouseExternalId;
            m_storageAreaExternalId = a_itemStorageRow.StorageAreaExternalId;
            m_itemExternalId = a_itemStorageRow.ItemExternalId;
            m_maxQty = a_itemStorageRow.MaxQty;
            if (!a_itemStorageRow.IsDisposalQtyNull())
            {
                m_disposalQty = a_itemStorageRow.DisposalQty;
            }

            if (!a_itemStorageRow.IsDisposeImmediatelyNull())
            {
                DisposeImmediately = a_itemStorageRow.DisposeImmediately;
            }
        }


        #region bool constants
        private BoolVector32 m_bools;
        private const int c_disposeImmediatelyIdx = 0;
        #endregion

        private readonly string m_warehouseExternalId;
        private readonly string m_itemExternalId;
        private readonly string m_storageAreaExternalId;
        private readonly decimal m_maxQty;
        private readonly decimal m_disposalQty;
        

        public string WarehouseExternalId => m_warehouseExternalId;
        public string ItemExternalId => m_itemExternalId;
        public string StorageAreaExternalId => m_storageAreaExternalId;
        public decimal MaxQty => m_maxQty;
        public decimal DisposalQty => m_disposalQty;

        public bool DisposeImmediately
        {
            get => m_bools[c_disposeImmediatelyIdx];
            set => m_bools[c_disposeImmediatelyIdx] = value;
        }
    }
}