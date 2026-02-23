using System.ComponentModel;

namespace PT.ERPTransmissions;

public partial class ForecastT
{
    /// <summary>
    /// Specifies a qty of an Item to be shipped at a particular datetime.
    /// </summary>
    public class ForecastShipment : IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 633;

        public ForecastShipment(IReader reader)
        {
            if (reader.VersionNumber >= 12049)
            {
                reader.Read(out requiredQty);
                reader.Read(out requiredDate);
                reader.Read(out m_warehouseExternalId);
            }
            else if (reader.VersionNumber >= 12000)
            {
                reader.Read(out requiredQty);
                reader.Read(out requiredDate);
            }
            else if (reader.VersionNumber >= 754)
            {
                reader.Read(out requiredQty);
                reader.Read(out requiredDate);
                reader.Read(out m_warehouseExternalId);
            }
            else
            {
                reader.Read(out requiredQty);
                reader.Read(out requiredDate);
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(requiredQty);
            writer.Write(requiredDate);
            writer.Write(m_warehouseExternalId);
        }

        [Browsable(false)]
        public int UniqueId => UNIQUE_ID;
        #endregion IPTSerializable

        public ForecastShipment() { }

        public ForecastShipment(ForecastTDataSet.ForecastShipmentsRow row)
        {
            requiredQty = row.RequiredQty;
            requiredDate = row.RequiredDate.ToServerTime();
            m_warehouseExternalId = row.WarehouseExternalId;
        }

        public ForecastShipment(decimal a_requiredQty, DateTime a_requiredDate, string a_warehouseExternalId)
        {
            requiredQty = a_requiredQty;
            requiredDate = a_requiredDate;
            m_warehouseExternalId = a_warehouseExternalId;
        }

        private decimal requiredQty;

        /// <summary>
        /// The qty demanded.
        /// </summary>
        public decimal RequiredQty
        {
            get => requiredQty;
            set => requiredQty = value;
        }

        private DateTime requiredDate;

        /// <summary>
        /// The date/time when the material must be in stock to satisfy the demand.
        /// </summary>
        public DateTime RequiredDate
        {
            get => requiredDate;
            set => requiredDate = value;
        }

        private string m_warehouseExternalId;

        public string WarehouseExternalId
        {
            get => m_warehouseExternalId;
            set => m_warehouseExternalId = value;
        }
    }
}