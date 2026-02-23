using System.Collections;
using System.ComponentModel;

using PT.APSCommon;
using PT.ERPTransmissions;
using PT.Transmissions;

namespace PT.Scheduler.Demand;

public partial class SalesOrderLine : BaseIdObject, IEnumerable<SalesOrderLineDistribution>
{
    #region IPTSerializable Members
    public new const int UNIQUE_ID = 640;

    internal SalesOrderLine(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_reader)
    {
        m_restoreInfo = new RestoreInfo();

        if (a_reader.VersionNumber >= 364)
        {
            a_reader.Read(out m_lineNumber);
            a_reader.Read(out m_description);
            a_reader.Read(out m_unitPrice);

            m_restoreInfo.itemId = new BaseId(a_reader);

            a_reader.Read(out int lineDistCount);
            for (int i = 0; i < lineDistCount; i++)
            {
                m_lineDistributions.Add(new SalesOrderLineDistribution(a_reader, a_idGen));
            }
        }
    }

    private RestoreInfo m_restoreInfo;

    private class RestoreInfo
    {
        internal BaseId itemId;
    }

    internal void RestoreReferences(ScenarioDetail a_sd, ItemManager aItems, WarehouseManager aWarehouses, SalesOrder aSO)
    {
        _salesOrder = aSO;

        m_item = aItems.GetById(m_restoreInfo.itemId);
        if (m_item == null)
        {
            throw new PTValidationException("Null Item"); //fix for bad data we had from allowing delete of in-use items
        }


        for (int i = 0; i < m_lineDistributions.Count; ++i)
        {
            m_lineDistributions[i].RestoreReferences(a_sd, aWarehouses, this);
        }

        m_restoreInfo = null;
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_lineNumber);
        writer.Write(m_description);
        writer.Write(m_unitPrice);

        m_item.Id.Serialize(writer);

        writer.Write(m_lineDistributions.Count);
        for (int i = 0; i < m_lineDistributions.Count; i++)
        {
            m_lineDistributions[i].Serialize(writer);
        }
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion IPTSerializable

    private SalesOrder _salesOrder;

    public SalesOrder SalesOrder => _salesOrder;

    public SalesOrderLine(SalesOrderT.SalesOrder.SalesOrderLine sol, SalesOrder aSO, BaseId aId, BaseIdGenerator aIdGen, ScenarioDetail a_sd, PTTransmissionBase a_t)
        : base(aId)
    {
        _salesOrder = aSO;

        Update(sol, aSO, aIdGen, a_sd, a_t, false);
    }

    internal void Update(SalesOrderT.SalesOrder.SalesOrderLine tSoLine, SalesOrder aSo, BaseIdGenerator a_idGen, ScenarioDetail a_sd, PTTransmissionBase a_t, bool a_updatingAnExistingLine)
    {
        if (Description != tSoLine.Description)
        {
            m_description = tSoLine.Description;
        }

        bool itemChanged = false;
        if (Item == null || Item.ExternalId != tSoLine.ItemExternalId)
        {
            Item item = a_sd.ItemManager.GetByExternalId(tSoLine.ItemExternalId);
            if (item == null)
            {
                throw new PTValidationException("2162", new object[] { aSo.ExternalId, tSoLine.ItemExternalId });
            }

            m_item = item;
            itemChanged = true;
            SetNetChangeMrpFlagForDistributions(a_sd.WarehouseManager);
        }

        if (LineNumber != tSoLine.LineNumber)
        {
            m_lineNumber = tSoLine.LineNumber;
        }

        if (UnitPrice != tSoLine.UnitPrice)
        {
            m_unitPrice = tSoLine.UnitPrice;
        }

        //Line Distributions
        if (LineDistributions.Count != tSoLine.LineDistributions.Count)
        {
            if (a_updatingAnExistingLine)
            {
                a_sd.PurchaseToStockManager.DeletingDistributionsFromLine(this, a_t);
                a_sd.JobManager.DeletingDemand(this, a_t);
            }

            LineDistributions.Clear();
            for (int i = 0; i < tSoLine.LineDistributions.Count; i++)
            {
                LineDistributions.Add(new SalesOrderLineDistribution(aSo.ExternalId, tSoLine.LineDistributions[i], a_sd.WarehouseManager, this, a_idGen.NextID(), a_idGen));
            }
        }
        else //count is the same so just update them
        {
            for (int i = 0; i < LineDistributions.Count; i++)
            {
                SalesOrderLineDistribution dist = LineDistributions[i];
                SalesOrderT.SalesOrder.SalesOrderLine.SalesOrderLineDistribution tDist = tSoLine.LineDistributions[i];
                dist.Update(tDist, a_sd.WarehouseManager, aSo.ExternalId, itemChanged || a_updatingAnExistingLine);
            }
        }
    }

    /// <summary>
    /// Remove distributions from job and purchase order demands.
    /// Removes the distributions from the line
    /// </summary>
    internal void DeleteDistributions(ScenarioDetail a_sd, BaseIdList a_lineIdList, PTTransmissionBase a_t)
    {
        a_sd.PurchaseToStockManager.DeletingDistributionsFromLine(this, a_lineIdList, a_t);
        a_sd.JobManager.DeletingDemand(this, a_t, a_lineIdList);
        for (int soldI = LineDistributions.Count - 1; soldI >= 0; soldI--)
        {
            foreach (BaseId lineId in a_lineIdList)
            {
                if (LineDistributions[soldI].Id == lineId)
                {
                    LineDistributions.Remove(LineDistributions[soldI]);
                }
            }
        }
    }

    public SalesOrderLine(BaseId a_id, SalesOrder so, string lineNbr, Item item)
        : base(a_id)
    {
        _salesOrder = so;
        m_lineNumber = lineNbr;
        m_item = item;
    }

    /// <summary>
    /// Iterates the list fo Distributions to find a match.
    /// Can be slow for long lists.
    /// Returns null if not found.
    /// </summary>
    public SalesOrderLineDistribution FindDistribution(BaseId a_distributionId)
    {
        for (int i = 0; i < LineDistributions.Count; i++)
        {
            SalesOrderLineDistribution dist = LineDistributions[i];
            if (dist.Id.CompareTo(a_distributionId) == 0)
            {
                return dist;
            }
        }

        return null;
    }

    public void Edit(ScenarioDetail a_sd, SalesOrderLineEdit a_edit, out bool o_itemChanged)
    {
        o_itemChanged = false;

        if (a_edit.DescriptionSet /*Description != a_edit.Description*/)
        {
            m_description = a_edit.Description;
        }

        if (a_edit.ItemExternalIdSet /*Item == null || Item.ExternalId != a_edit.ItemExternalId*/)
        {
            Item item = a_sd.ItemManager.GetByExternalId(a_edit.ItemExternalId);
            if (item == null)
            {
                throw new PTValidationException("2162", new object[] { SalesOrder.ExternalId, a_edit.ItemExternalId });
            }

            m_item = item;
            o_itemChanged = true;
            SetNetChangeMrpFlagForDistributions(a_sd.WarehouseManager);
        }

        if (a_edit.LineNumberSet /*LineNumber != a_edit.LineNumber*/)
        {
            m_lineNumber = a_edit.LineNumber;
        }

        if (a_edit.UnitPriceSet /*UnitPrice != a_edit.UnitPrice*/)
        {
            m_unitPrice = a_edit.UnitPrice;
        }
    }

    #region Shared Properties
    private string m_lineNumber;

    /// <summary>
    /// The identifier for a line item within the sales order.
    /// </summary>
    public string LineNumber
    {
        get => m_lineNumber;
        set => m_lineNumber = value;
    }

    private Item m_item;

    /// <summary>
    /// Item ordered.
    /// </summary>
    public Item Item
    {
        get => m_item;
        set => m_item = value;
    }

    private string m_description;

    public string Description
    {
        get => m_description;
        set => m_description = value;
    }

    private decimal m_unitPrice;

    /// <summary>
    /// The sale price per unit.  This can be used to maximize sales revenue.
    /// </summary>
    public decimal UnitPrice
    {
        get => m_unitPrice;
        set => m_unitPrice = value;
    }
    #endregion Shared Properties

    private readonly List<SalesOrderLineDistribution> m_lineDistributions = new ();

    public List<SalesOrderLineDistribution> LineDistributions => m_lineDistributions;

    internal void SetNetChangeMrpFlagForDistributions(WarehouseManager aWarehouseManager)
    {
        for (int i = 0; i < LineDistributions.Count; i++)
        {
            LineDistributions[i].SetNetChangeMrpFlag(aWarehouseManager);
        }
    }

    #region IAfterRestoreReferences Members
    public override void AfterRestoreReferences_1(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        AfterRestoreReferences.Helpers.IEnumerableHelperFor_AfterRestoreReferences_1(serializationVersionNbr, _salesOrder.IdGen, m_lineDistributions, this, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
    }

    public override void AfterRestoreReferences_2(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        AfterRestoreReferences.Helpers.IEnumerableHelperFor_AfterRestoreReferences_2(serializationVersionNbr, m_lineDistributions, this, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
    }
    #endregion

    public List<SalesOrderLineDistribution>.Enumerator GetEnumerator()
    {
        return LineDistributions.GetEnumerator();
    }

    IEnumerator<SalesOrderLineDistribution> IEnumerable<SalesOrderLineDistribution>.GetEnumerator()
    {
        return ((IEnumerable<SalesOrderLineDistribution>)m_lineDistributions).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<SalesOrderLineDistribution>)m_lineDistributions).GetEnumerator();
    }
}