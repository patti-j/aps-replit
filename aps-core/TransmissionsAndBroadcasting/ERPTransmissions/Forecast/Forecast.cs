using System.ComponentModel;

using PT.Transmissions;
using static PT.Transmissions.CustomerT;

namespace PT.ERPTransmissions;

public partial class ForecastT
{
    /// <summary>
    /// A list of ForecastShipments for a particular Inventory.
    /// </summary>
    public class Forecast : PTObjectBase, IPTSerializable
    {
        #region IPTSerializable Members
        public new const int UNIQUE_ID = 634;

        public Forecast(PT.Common.IReader a_reader)
    : base(a_reader)
        {
            if (a_reader.VersionNumber >= 12323)
            {
                m_setBools = new BoolVector32(a_reader);
                a_reader.Read(out m_customer);
                a_reader.Read(out m_planner);
                a_reader.Read(out m_salesOffice);
                a_reader.Read(out m_salesPerson);
                a_reader.Read(out m_priority);
                int shipmentCount;
                a_reader.Read(out shipmentCount);
                for (int i = 0; i < shipmentCount; i++)
                {
                    ForecastShipment shipment = new ForecastShipment(a_reader);
                    m_shipments.Add(shipment);
                }
            }
            else if (a_reader.VersionNumber >= 272)
            {
                a_reader.Read(out m_customer);
                a_reader.Read(out m_planner);
                a_reader.Read(out m_salesOffice);
                a_reader.Read(out m_salesPerson);
                a_reader.Read(out m_priority);
                int shipmentCount;
                a_reader.Read(out shipmentCount);
                for (int i = 0; i < shipmentCount; i++)
                {
                    ForecastShipment shipment = new ForecastShipment(a_reader);
                    m_shipments.Add(shipment);
                }
            }
            #region Version 1
            else
            {
                a_reader.Read(out m_customer);
                a_reader.Read(out m_planner);
                a_reader.Read(out m_salesOffice);
                a_reader.Read(out m_salesPerson);
                int shipmentCount;
                a_reader.Read(out shipmentCount);
                for (int i = 0; i < shipmentCount; i++)
                {
                    ForecastShipment shipment = new ForecastShipment(a_reader);
                    m_shipments.Add(shipment);
                }
            }
            #endregion
        }

        public override void Serialize(PT.Common.IWriter a_writer)
        {
            base.Serialize(a_writer);
            m_setBools.Serialize(a_writer);

            a_writer.Write(m_customer);
            a_writer.Write(m_planner);
            a_writer.Write(m_salesOffice);
            a_writer.Write(m_salesPerson);
            a_writer.Write(m_priority);
            a_writer.Write(m_shipments.Count);
            for (int i = 0; i < m_shipments.Count; i++)
            {
                m_shipments[i].Serialize(a_writer);
            }
        }

        [Browsable(false)]
        public new int UniqueId => UNIQUE_ID;
        #endregion IPTSerializable

        public Forecast() { }

        public Forecast(ForecastTDataSet.ForecastsRow row)
            : base(row.ExternalId, row.Name, row.IsDescriptionNull() ? null : row.Description, row.IsNotesNull() ? null : row.Notes, row.IsUserFieldsNull() ? null : row.UserFields)
        {
            if (!row.IsCustomerExternalIdNull())
            {
                Customer = row.CustomerExternalId;
            }

            if (!row.IsPlannerNull())
            {
                Planner = row.Planner;
            }

            if (!row.IsSalesOfficeNull())
            {
                SalesOffice = row.SalesOffice;
            }

            if (!row.IsSalesPersonNull())
            {
                SalesPerson = row.SalesPerson;
            }

            if (!row.IsPriorityNull())
            {
                Priority = row.Priority;
            }

            ForecastTDataSet.ForecastShipmentsRow[] shipmentRows = row.GetForecastShipmentsRows();
            for (int shipmentI = 0; shipmentI < shipmentRows.Length; shipmentI++)
            {
                ForecastShipment shipment = new ((ForecastTDataSet.ForecastShipmentsRow)shipmentRows.GetValue(shipmentI));
                m_shipments.Add(shipment);
            }
        }

        #region Shared Properties
        private BoolVector32 m_setBools = new BoolVector32();
        private const short c_customerIsSetIdx = 0;
        private const short c_plannerIsSetIdx = 1;
        private const short c_salesOfficeIsSetIdx = 2;
        private const short c_salesPersonIsSetIdx = 3;
        private const short c_priorityIsSetIdx = 4;


        private string m_customer;

        /// <summary>
        /// The company the Forecast is for.
        /// </summary>
        public string Customer
        {
            get { return m_customer; }
            set
            {
                m_customer = value;
                m_setBools[c_customerIsSetIdx] = true;
            }
        }

        private string m_planner;

        /// <summary>
        /// The scheduler who will manage the Forecast.
        /// </summary>
        public string Planner
        {
            get { return m_planner; }
            set
            {
                m_planner = value;
                m_setBools[c_plannerIsSetIdx] = true;
            }
        }

        private string m_salesOffice;

        /// <summary>
        /// Specifies the sales office or other physical location that created the demand.
        /// This has no effect on the Warehouse that satisfies the Forecast.
        /// It is for reference only.
        /// </summary>
        public string SalesOffice
        {
            get { return m_salesOffice; }
            set
            {
                m_salesOffice = value;
                m_setBools[c_salesOfficeIsSetIdx] = true;
            }
        }

        private string m_salesPerson;

        /// <summary>
        /// The employee in sales who is responsible for this demand.
        /// </summary>
        public string SalesPerson
        {
            get { return m_salesPerson; }
            set
            {
                m_salesPerson = value;
                m_setBools[c_salesPersonIsSetIdx] = true;
            }
        }

        private int m_priority;

        /// <summary>
        /// Sets the Priority for Jobs created by MRP to satisify this demand.
        /// </summary>
        public int Priority
        {
            get { return m_priority; }
            set
            {
                m_priority = value;
                m_setBools[c_priorityIsSetIdx] = true;
            }
        }

        public bool CustomerSet => m_setBools[c_customerIsSetIdx];
        public bool PlannerSet => m_setBools[c_plannerIsSetIdx];
        public bool SalesOfficeSet => m_setBools[c_salesOfficeIsSetIdx];
        public bool SalesPersonSet => m_setBools[c_salesPersonIsSetIdx];
        public bool PrioritySet => m_setBools[c_priorityIsSetIdx];
        #endregion Shared Properties

        private List<ForecastShipment> m_shipments = new ();

        /// <summary>
        /// Lists the details of qties and dates making up the Forecast.
        /// </summary>
        public List<ForecastShipment> Shipments
        {
            get => m_shipments;
            set => m_shipments = value;
        }

        public override void Validate()
        {
            base.Validate();
        }
    }
}