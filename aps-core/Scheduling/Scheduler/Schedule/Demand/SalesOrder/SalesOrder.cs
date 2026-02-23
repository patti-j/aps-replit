using System.ComponentModel;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler.Demand;

public partial class SalesOrder : BaseObject
{
    #region IPTSerializable Members
    public new const int UNIQUE_ID = 639;

    internal SalesOrder(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_reader)
    {
        m_idGen = a_idGen;
        if (a_reader.VersionNumber >= 12001)
        {
            bools = new BoolVector32(a_reader);
            a_reader.Read(out estimate);
            a_reader.Read(out salesAmount);
            a_reader.Read(out salesOffice);
            a_reader.Read(out salesPerson);
            a_reader.Read(out planner);
            a_reader.Read(out project);

            a_reader.Read(out int soLinesCount);
            for (int i = 0; i < soLinesCount; i++)
            {
                m_salesOrderLines.Add(new SalesOrderLine(a_reader, a_idGen));
            }

            m_referenceInfo = new ReferenceInfo();
            m_referenceInfo.JobId = new BaseId(a_reader);
            m_referenceInfo.CustomerId = new BaseId(a_reader);
            a_reader.Read(out expirationDate);
        }
        else if (a_reader.VersionNumber >= 277)
        {
            m_referenceInfo = new ReferenceInfo();
            a_reader.Read(out m_referenceInfo.CustomerExternalId);
            a_reader.Read(out estimate);
            a_reader.Read(out salesAmount);
            a_reader.Read(out salesOffice);
            a_reader.Read(out salesPerson);
            a_reader.Read(out planner);
            a_reader.Read(out project);

            a_reader.Read(out int soLinesCount);
            for (int i = 0; i < soLinesCount; i++)
            {
                m_salesOrderLines.Add(new SalesOrderLine(a_reader, a_idGen));
            }

            m_referenceInfo.JobId = new BaseId(a_reader);
            bools = new BoolVector32(a_reader);
            a_reader.Read(out expirationDate);
        }
    }

    internal void RestoreReferences(ScenarioDetail a_sd, WarehouseManager a_warehouses, ItemManager a_items, JobManager a_jobManager, CustomerManager a_customerManager)
    {
        if (m_referenceInfo.JobId != BaseId.NULL_ID)
        {
            m_ctpJob = a_jobManager.GetById(m_referenceInfo.JobId);
        }

        if (m_referenceInfo.CustomerId != BaseId.NULL_ID)
        {
            m_customer = a_customerManager.GetById(m_referenceInfo.CustomerId);
        }

        //For Backwards Compatibility
        if (!string.IsNullOrEmpty(m_referenceInfo.CustomerExternalId))
        {
            m_customer = a_customerManager.GetByExternalId(m_referenceInfo.CustomerExternalId);
        }

        m_referenceInfo = null;

        for (int i = 0; i < m_salesOrderLines.Count; ++i)
        {
            m_salesOrderLines[i].RestoreReferences(a_sd, a_items, a_warehouses, this);
        }
    }

    internal void AfterRestoreAdjustmentReferences()
    {
        foreach (SalesOrderLine line in SalesOrderLines)
        {
            foreach (SalesOrderLineDistribution sod in line.LineDistributions)
            {
                sod.AfterRestoreAdjustmentReferences();
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        bools.Serialize(writer);
        writer.Write(estimate);
        writer.Write(salesAmount);
        writer.Write(salesOffice);
        writer.Write(salesPerson);
        writer.Write(planner);
        writer.Write(project);

        writer.Write(m_salesOrderLines.Count);
        for (int i = 0; i < m_salesOrderLines.Count; i++)
        {
            m_salesOrderLines[i].Serialize(writer);
        }

        if (m_ctpJob != null)
        {
            m_ctpJob.Id.Serialize(writer);
        }
        else
        {
            BaseId.NULL_ID.Serialize(writer);
        }

        if (m_customer != null)
        {
            m_customer.Id.Serialize(writer);
        }
        else
        {
            BaseId.NULL_ID.Serialize(writer);
        }

        writer.Write(expirationDate);
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion IPTSerializable

    public SalesOrder(UserFieldDefinitionManager a_udfManager, BaseId aId, SalesOrderT.SalesOrder tSO, BaseIdGenerator idGen, SalesOrderT a_t, ScenarioDetail a_sd)
        : base(aId, tSO)
    {
        m_idGen = idGen;

        Update(a_udfManager, tSO, a_sd, a_t);
    }

    public SalesOrder(ScenarioDetail sd) : base(sd.IdGen.NextID())
    {
        m_idGen = sd.IdGen;
    }

    public void Update(UserFieldDefinitionManager a_udfManager, SalesOrderT.SalesOrder a_tSo, ScenarioDetail a_sd, SalesOrderT a_t)
    {
        base.Update(a_tSo, a_t, a_udfManager, UserField.EUDFObjectType.SalesOrders);
        if (m_customer == null || m_customer.ExternalId != a_tSo.CustomerExternalId)
        {
            Customer customer = null;
            if (!string.IsNullOrEmpty(a_tSo.CustomerExternalId))
            {
                //Try to set the customer
                customer = a_sd.CustomerManager.GetByExternalId(a_tSo.CustomerExternalId);
                if (customer == null)
                {
                    throw new TransmissionValidationException(a_t, "3025", new object[] { a_tSo.ExternalId, a_tSo.CustomerExternalId });
                }
            }

            m_customer = customer;
        }

        if (Estimate != a_tSo.Estimate)
        {
            Estimate = a_tSo.Estimate;
        }

        if (SalesAmount != a_tSo.SalesAmount)
        {
            SalesAmount = a_tSo.SalesAmount;
        }

        if (SalesOffice != a_tSo.SalesOffice)
        {
            SalesOffice = a_tSo.SalesOffice;
        }

        if (SalesPerson != a_tSo.SalesPerson)
        {
            SalesPerson = a_tSo.SalesPerson;
        }

        if (Planner != a_tSo.Planner)
        {
            Planner = a_tSo.Planner;
        }

        if (Project != a_tSo.Project)
        {
            Project = a_tSo.Project;
        }

        if (Cancelled != a_tSo.Cancelled)
        {
            Cancelled = a_tSo.Cancelled;
            SetItemsReferencedAsNeedingNetChangeMRP(a_sd.WarehouseManager);
        }

        if (CancelAtExpirationDate != a_tSo.CancelAtExpirationDate)
        {
            CancelAtExpirationDate = a_tSo.CancelAtExpirationDate;
        }

        if (ExpirationDate != a_tSo.ExpirationDate)
        {
            ExpirationDate = a_tSo.ExpirationDate;
        }

        //Update lines
        if (SalesOrderLines.Count != a_tSo.SalesOrderLines.Count)
        {
            SetItemsReferencedAsNeedingNetChangeMRP(a_sd.WarehouseManager); //the old items may be changed.
            DeletingSalesOrderOrAllDistributions(a_sd, a_t); //must call before clearing
            m_salesOrderLines.Clear();
            for (int i = 0; i < a_tSo.SalesOrderLines.Count; i++)
            {
                SalesOrderLines.Add(new SalesOrderLine(a_tSo.SalesOrderLines[i], this, m_idGen.NextID(), m_idGen, a_sd, a_t));
            }
        }
        else //same number, so update them individually
        {
            for (int i = 0; i < SalesOrderLines.Count; i++)
            {
                SalesOrderLine soLine = SalesOrderLines[i];
                SalesOrderT.SalesOrder.SalesOrderLine tSoLine = a_tSo.SalesOrderLines[i];
                soLine.Update(tSoLine, this, m_idGen, a_sd, a_t, true);
            }
        }
    }

    private readonly BaseIdGenerator m_idGen;

    internal BaseIdGenerator IdGen => m_idGen;

    #region Shared Properties
    private bool estimate;

    /// <summary>
    /// If true then this is a quote, not a firm order.
    /// </summary>
    public bool Estimate
    {
        get => estimate;
        set => estimate = value;
    }

    //TODO: Replace Estimate with Firm
    public bool Firm => !Estimate;

    private decimal salesAmount;

    public decimal SalesAmount
    {
        get => salesAmount;
        set => salesAmount = value;
    }

    private string salesOffice;

    /// <summary>
    /// Specifies the sales office or other physical location that created the demand.
    /// This has no effect on the Warehouse that satisfies the order.
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

    private string planner;

    /// <summary>
    /// The User responsible for planning this demand if planning by demand rather than by product or location.
    /// </summary>
    public string Planner
    {
        get => planner;
        set => planner = value;
    }

    private string project;

    /// <summary>
    /// Can be used for tracking multiple demands tied to one project.
    /// </summary>
    public string Project
    {
        get => project;
        set => project = value;
    }

    private BoolVector32 bools;
    private const int cancelledIdx = 0;
    private const int cancelAtExpireDateIdx = 1;

    /// <summary>
    /// Whether the Sales Order has been cancelled either by setting this value explicitly or by the Expiration Date passsing.
    /// Once a Sales Order is cancelled, its demands are ignored.
    /// If the Sales Order has a CTP Job then the Job's Cancelled status is set to the new value as well.
    /// </summary>
    public bool Cancelled
    {
        get => bools[cancelledIdx];
        set
        {
            bools[cancelledIdx] = value;
            if (CtpJob != null)
            {
                CtpJob.Cancelled = value;
            }
        }
    }

    /// <summary>
    /// If true then the Sales Order is marked as Cancelled when the PlanetTogether Clock passes the Expiration Date.
    /// </summary>
    public bool CancelAtExpirationDate
    {
        get => bools[cancelAtExpireDateIdx];
        set => bools[cancelAtExpireDateIdx] = value;
    }

    private DateTime expirationDate = new (PTDateTime.MaxDateTicks);

    /// <summary>
    /// If CancelAtExpirationDate is true then the Sales Order is marked as Cancelled when the PlanetTogether Clock passes this date.
    /// </summary>
    public DateTime ExpirationDate
    {
        get => expirationDate;
        set => expirationDate = value;
    }
    #endregion Shared Properties

    [Browsable(false)]
    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    public override string DefaultNamePrefix => "SO-";

    private readonly List<SalesOrderLine> m_salesOrderLines = new ();

    public List<SalesOrderLine> SalesOrderLines => m_salesOrderLines;

    /// <summary>
    /// Iterates the list for a matching Sales Order Line.
    /// Could be slow for long lists.
    /// </summary>
    public SalesOrderLine FindSalesOrderLine(BaseId a_lineId)
    {
        for (int i = 0; i < SalesOrderLines.Count; i++)
        {
            SalesOrderLine line = SalesOrderLines[i];
            if (line.Id.CompareTo(a_lineId) == 0)
            {
                return line;
            }
        }

        return null;
    }

    private Job m_ctpJob;

    /// <summary>
    /// If the Sales Order was created from a CTP and a Job was created to satisify the CTP then this is the Job.
    /// If the Sales Order is deleted then the Job is also deleted.
    /// </summary>
    public Job CtpJob
    {
        get => m_ctpJob;
        set => m_ctpJob = value;
    }

    #region CTP
    internal void AdvanceClock(DateTime a_newClock)
    {
        if (CancelAtExpirationDate && a_newClock.Ticks >= ExpirationDate.Ticks && !Cancelled)
        {
            Cancelled = true;
        }
    }
    #endregion CTP

    private Customer m_customer;

    public Customer Customer
    {
        get => m_customer;
        set => m_customer = value;
    }

    private ReferenceInfo m_referenceInfo;

    private class ReferenceInfo
    {
        internal BaseId JobId;
        internal BaseId CustomerId;

        //For Backwards Compatibility
        internal string CustomerExternalId;
    }

    internal void SetItemsReferencedAsNeedingNetChangeMRP(WarehouseManager aWarehouseManager)
    {
        for (int i = 0; i < SalesOrderLines.Count; i++)
        {
            SalesOrderLine sol = SalesOrderLines[i];
            sol.SetNetChangeMrpFlagForDistributions(aWarehouseManager);
        }
    }

    internal void DeletingSalesOrderOrAllDistributions(ScenarioDetail a_sd, PTTransmissionBase a_t)
    {
        //Remove any Demand references from Job Products or Purchase to this Sales Order.
        a_sd.PurchaseToStockManager.DeletingSalesOrderOrAllDistributions(this, a_t);
        a_sd.JobManager.DeletingDemand(this, a_t);
        foreach (SalesOrderLine line in SalesOrderLines)
        {
            foreach (SalesOrderLineDistribution dist in line.LineDistributions)
            {
                dist.Deleting();
            }
        }
    }

    /// <summary>
    /// Removes any Demand references from Job Products or Purchase to this Sales Order Line
    /// Removes the line from the sales order
    /// </summary>
    internal void DeleteSalesOrderLine(ScenarioDetail a_sd, SalesOrderLine a_soLine, PTTransmissionBase a_t)
    {
        a_sd.PurchaseToStockManager.DeletingDistributionsFromLine(a_soLine, a_t);
        a_sd.JobManager.DeletingDemand(a_soLine, a_t);
        foreach (SalesOrderLineDistribution dist in a_soLine)
        {
            dist.Deleting();
        }
        SalesOrderLines.Remove(a_soLine);
    }

    public void Edit(ScenarioDetail a_sd, SalesOrderEdit a_edit)
    {
        base.Edit(a_edit);

        if (a_edit.CustomerSet)
        {
            Customer customer = a_sd.CustomerManager.GetByExternalId(a_edit.CustomerExternalId);
            m_customer = customer;
        }

        if (a_edit.EstimateSet /*Estimate != a_edit.Estimate*/) //Add Set checks
        {
            Estimate = a_edit.Estimate;
        }

        if (a_edit.SalesAmountSet /*SalesAmount != a_edit.SalesAmount*/)
        {
            SalesAmount = a_edit.SalesAmount;
        }

        if (a_edit.SalesOfficeSet /*SalesOffice != a_edit.SalesOffice*/)
        {
            SalesOffice = a_edit.SalesOffice;
        }

        if (a_edit.SalesPersonSet /*SalesPerson != a_edit.SalesPerson*/)
        {
            SalesPerson = a_edit.SalesPerson;
        }

        if (a_edit.PlannerSet /*Planner != a_edit.Planner*/)
        {
            Planner = a_edit.Planner;
        }

        if (a_edit.ProjectSet /*Project != a_edit.Project*/)
        {
            Project = a_edit.Project;
        }

        if (a_edit.CancelledSet /*Cancelled != a_edit.Cancelled*/)
        {
            Cancelled = a_edit.Cancelled;
            SetItemsReferencedAsNeedingNetChangeMRP(a_sd.WarehouseManager);
        }

        if (a_edit.CancelAtExpirationDateSet /*CancelAtExpirationDate != a_edit.CancelAtExpirationDate*/)
        {
            CancelAtExpirationDate = a_edit.CancelAtExpirationDate;
        }

        if (a_edit.ExpirationDateSet /*ExpirationDate != a_edit.ExpirationDate*/)
        {
            ExpirationDate = a_edit.ExpirationDate;
        }
    }

    #region IAfterRestoreReferences Members
    public override void AfterRestoreReferences_1(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        AfterRestoreReferences.Helpers.IEnumerableHelperFor_AfterRestoreReferences_1(serializationVersionNbr, m_idGen, m_salesOrderLines, this, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
    }

    public override void AfterRestoreReferences_2(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        AfterRestoreReferences.Helpers.IEnumerableHelperFor_AfterRestoreReferences_2(serializationVersionNbr, m_salesOrderLines, this, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
    }
    #endregion
}