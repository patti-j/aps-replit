using System.ComponentModel;

using PT.APSCommon;
using PT.APSCommon.Collections;
using PT.APSCommon.Extensions;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler.Demand;

/// <summary>
/// A list of ForecastShipments for a particular Inventory.
/// </summary>
public class Forecast : BaseObject
{
    #region IPTSerializable Members
    public new const int UNIQUE_ID = 630;

    public Forecast(IReader a_reader)
        : base(a_reader)
    {
        m_referenceInfo = new ReferenceInfo();
        if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out planner);
            a_reader.Read(out salesOffice);
            a_reader.Read(out salesPerson);
            a_reader.Read(out priority);
            m_shipments = new ForecastShipmentList(a_reader, new ForecastShipmentList.ForecastShipmentComparer());
            m_referenceInfo.CustomerId = new BaseId(a_reader);
        }

        #region 410
        else if (a_reader.VersionNumber >= 410)
        {
            a_reader.Read(out m_referenceInfo.CustomerExternalId);
            a_reader.Read(out planner);
            a_reader.Read(out salesOffice);
            a_reader.Read(out salesPerson);
            a_reader.Read(out priority);
            m_shipments = new ForecastShipmentList(a_reader, new ForecastShipmentList.ForecastShipmentComparer());
        }
        #endregion

        #region 272
        else if (a_reader.VersionNumber >= 272)
        {
            a_reader.Read(out m_referenceInfo.CustomerExternalId);
            a_reader.Read(out planner);
            a_reader.Read(out salesOffice);
            a_reader.Read(out salesPerson);
            a_reader.Read(out priority);
            int shipmentCount;
            a_reader.Read(out shipmentCount);
            for (int i = 0; i < shipmentCount; i++)
            {
                ForecastShipment shipment = new (a_reader);
                Add(shipment);
            }
        }
        #endregion

        #region version 257
        else if (a_reader.VersionNumber >= 257)
        {
            a_reader.Read(out m_referenceInfo.CustomerExternalId);
            a_reader.Read(out planner);
            a_reader.Read(out salesOffice);
            a_reader.Read(out salesPerson);
            int shipmentCount;
            a_reader.Read(out shipmentCount);
            for (int i = 0; i < shipmentCount; i++)
            {
                ForecastShipment shipment = new (a_reader);
                Add(shipment);
            }
        }
        else
        {
            a_reader.Read(out m_referenceInfo.CustomerExternalId);
            a_reader.Read(out planner);
            a_reader.Read(out salesOffice);
            a_reader.Read(out salesPerson);
            int shipmentCount;
            a_reader.Read(out shipmentCount);
            for (int i = 0; i < shipmentCount; i++)
            {
                ForecastShipment shipment = new (a_reader, null);
                Add(shipment);
            }
        }
        #endregion

        #if TEST
            CompareDesyncResults();
        #endif
    }

    public void RestoreReferences(CustomerManager a_cm, ForecastVersion aFV)
    {
        _forecastVersion = aFV;

        if (m_referenceInfo.CustomerId != BaseId.NULL_ID)
        {
            m_customer = a_cm.GetById(m_referenceInfo.CustomerId);
        }

        //For backwards compatibility
        if (!string.IsNullOrEmpty(m_referenceInfo.CustomerExternalId))
        {
            m_customer = a_cm.GetByExternalId(m_referenceInfo.CustomerExternalId);
        }

        using (IEnumerator<ForecastShipment> enumerator = m_shipments.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                ForecastShipment fs = enumerator.Current;
                fs.RestoreReferences(this);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(planner);
        writer.Write(salesOffice);
        writer.Write(salesPerson);
        writer.Write(priority);

        m_shipments.Serialize(writer);

        if (m_customer != null)
        {
            m_customer.Id.Serialize(writer);
        }
        else
        {
            BaseId.NULL_ID.Serialize(writer);
        }
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion IPTSerializable

    internal void RestoreReferences(CustomerManager a_customerManager)
    {
        if (m_referenceInfo.CustomerId != BaseId.NULL_ID)
        {
            m_customer = a_customerManager.GetById(m_referenceInfo.CustomerId);
        }

        m_referenceInfo = null;
    }

    internal Forecast(ForecastVersion aFV, BaseId id)
        : base(id)
    {
        _forecastVersion = aFV;
    }

    internal Forecast(ForecastVersion aFV, ForecastT.Forecast tForecast, Customer a_customer, BaseId id, PTTransmission t, UserFieldDefinitionManager a_udfManager)
        : base(id, tForecast)
    {
        _forecastVersion = aFV;
        Update(tForecast, t, a_udfManager, UserField.EUDFObjectType.Forecasts);

        //Update member values.
        priority = tForecast.Priority;
        planner = tForecast.Planner;
        salesOffice = tForecast.SalesOffice;
        salesPerson = tForecast.SalesPerson;
        Customer = a_customer;

        for (int i = 0; i < tForecast.Shipments.Count; i++)
        {
            ForecastShipment shipment = new (_forecastVersion.IdGen.NextID(), tForecast.Shipments[i].RequiredDate, tForecast.Shipments[i].RequiredQty, tForecast.Shipments[i].WarehouseExternalId, this);
            m_shipments.Add(shipment);
        }
    }

    internal void Update(UserFieldDefinitionManager a_udfManager, ERPTransmissions.ForecastT.Forecast tForecast, out bool mrpNetChangeCriticalUpdates, ScenarioDetail a_sd, PTTransmission a_t)
    {
        mrpNetChangeCriticalUpdates = false;
        base.Update(tForecast, a_t, a_udfManager, UserField.EUDFObjectType.Forecasts);

        if (Customer == null || Customer.ExternalId != tForecast.Customer)
        {
            Customer customer = a_sd.CustomerManager.GetByExternalId(tForecast.Customer);
            m_customer = customer;
        }

        planner = tForecast.Planner;
        salesOffice = tForecast.SalesOffice;
        salesPerson = tForecast.SalesPerson;

        if (priority != tForecast.Priority)
        {
            priority = tForecast.Priority;
            mrpNetChangeCriticalUpdates = true;
        }

        //See if the list of shipments has changed
        bool shipmentsChanged = false;
        if (tForecast.Shipments.Count != m_shipments.Count)
        {
            shipmentsChanged = true;
        }
        else
        {
            IEnumerator<ForecastShipment> shipmentEnumerator = m_shipments.GetEnumerator();
            int i = 0;
            while (shipmentEnumerator.MoveNext())
            {
                ForecastShipment shipment = shipmentEnumerator.Current;
                ERPTransmissions.ForecastT.ForecastShipment tShipment = tForecast.Shipments[i];
                i++;
                if (tShipment.RequiredDate.Ticks != shipment.RequireDateTicks || tShipment.RequiredQty != shipment.RequiredQty)
                {
                    shipmentsChanged = true;
                    break;
                }
            }
        }

        if (shipmentsChanged)
        {
            mrpNetChangeCriticalUpdates = true;
            DeletingOrClearingShipments(a_sd, a_t);
            m_shipments.Clear();

            for (int i = 0; i < tForecast.Shipments.Count; i++)
            {
                ERPTransmissions.ForecastT.ForecastShipment tShipment = tForecast.Shipments[i];
                ForecastShipment newShipment = new (_forecastVersion.IdGen.NextID(), tShipment.RequiredDate, tShipment.RequiredQty, tShipment.WarehouseExternalId, this);
                m_shipments.Add(newShipment);
            }
        }
    }

    internal void DeletingOrClearingShipments(ScenarioDetail a_sd, PTTransmissionBase a_t)
    {
        a_sd.PurchaseToStockManager.DeletingForecastShipments(this, a_t);
        a_sd.JobManager.DeletingDemand(this, a_t);
    }

    internal void RemoveShipment(ForecastShipment a_shipment, ScenarioDetail a_sd, PTTransmissionBase a_t)
    {
        a_sd.PurchaseToStockManager.DeletingForecastShipment(a_shipment, a_t);
        a_sd.JobManager.DeletingDemand(a_shipment, a_t);
        Remove(a_shipment);
    }

    private ForecastVersion _forecastVersion;

    public ForecastVersion ForecastVersion => _forecastVersion;

    #region Shared Properties
    private string planner;

    /// <summary>
    /// The scheduler who will manage the Forecast.
    /// </summary>
    public string Planner
    {
        get => planner;
        set => planner = value;
    }

    private string salesOffice;

    /// <summary>
    /// Specifies the sales office or other physical location that created the demand.
    /// This has no effect on the Warehouse that satisfies the Forecast.
    /// It is for reference only.
    /// </summary>
    public string SalesOffice
    {
        get => salesOffice;
        set => salesOffice = value;
    }

    private string salesPerson;

    /// <summary>
    /// The employee in sales who is responsible for this demand.
    /// </summary>
    public string SalesPerson
    {
        get => salesPerson;
        set => salesPerson = value;
    }

    private int priority;

    /// <summary>
    /// Sets the Priority for Jobs created by MRP to satisify this demand.
    /// </summary>
    public int Priority
    {
        get => priority;
        set => priority = value;
    }

    private Customer m_customer;

    public Customer Customer
    {
        get => m_customer;
        set => m_customer = value;
    }

    private ReferenceInfo m_referenceInfo;

    private class ReferenceInfo
    {
        internal BaseId CustomerId;

        //For Backwards Compatibility
        internal string CustomerExternalId;
    }
    #endregion Shared Properties

    #region list maintenance
    private readonly ForecastShipmentList m_shipments = new (new ForecastShipmentList.ForecastShipmentComparer());

    public IReadOnlyList<ForecastShipment> Shipments => m_shipments.ReadOnlyList;

    internal void Add(ForecastShipment a_shipment)
    {
        m_shipments.Add(a_shipment);
    }

    internal void Remove(ForecastShipment a_shipment)
    {
        m_shipments.RemoveByKey(a_shipment.Id);
    }

    /// <summary>
    /// Given a shipmentId returns a shipment within this forecast.
    /// </summary>
    /// <param name="a_shipmentId"></param>
    /// <returns></returns>
    public ForecastShipment this[BaseId a_shipmentId] => m_shipments.GetValue(a_shipmentId);

    internal IEnumerator<ForecastShipment> GetEnumerator()
    {
        return m_shipments.GetEnumerator();
    }
    #endregion

    [Browsable(false)]
    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    public override string DefaultNamePrefix => "Forecast";

    #region IAfterRestoreReferences Members
    public override void AfterRestoreReferences_1(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        AfterRestoreReferences.Helpers.IEnumerableHelperFor_AfterRestoreReferences_1(serializationVersionNbr, _forecastVersion.IdGen, m_shipments.ReadOnlyList, this, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
    }

    public override void AfterRestoreReferences_2(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        AfterRestoreReferences.Helpers.IEnumerableHelperFor_AfterRestoreReferences_2(serializationVersionNbr, m_shipments.ReadOnlyList, this, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
    }
    #endregion

    public class ForecastShipmentList : CustomSortedList<ForecastShipment>
    {
        public ForecastShipmentList(IReader a_reader, IKeyObjectComparer<ForecastShipment> a_comparer)
            : base(a_reader, a_comparer) { }

        public ForecastShipmentList(IKeyObjectComparer<ForecastShipment> a_comparer)
            : base(a_comparer) { }

        protected override ForecastShipment CreateInstance(IReader a_reader)
        {
            return new ForecastShipment(a_reader);
        }

        public class ForecastShipmentComparer : IKeyObjectComparer<ForecastShipment>
        {
            public int Compare(ForecastShipment x, ForecastShipment y)
            {
                return CompareForecastShipments(x, y);
            }

            internal static int CompareForecastShipments(ForecastShipment a_x, ForecastShipment a_y)
            {
                return Comparison.Compare(a_x.Id.Value, a_y.Id.Value);
            }

            public object GetKey(ForecastShipment a_forecastship)
            {
                return a_forecastship.Id;
            }
        }
    }

    public override string ToString()
    {
        string s = string.Format("Shipments={0}:".Localize(), m_shipments.Count);
        return s;
    }
}