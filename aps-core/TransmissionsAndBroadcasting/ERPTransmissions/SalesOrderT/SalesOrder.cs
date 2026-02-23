using System.ComponentModel;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

public partial class SalesOrderT
{
    public class SalesOrder : PTObjectBase, IPTSerializable
    {
        #region IPTSerializable Members
        public new const int UNIQUE_ID = 642;

        public SalesOrder(IReader reader)
            : base(reader)
        {
            if (reader.VersionNumber >= 277)
            {
                reader.Read(out m_customerExternalId);
                reader.Read(out estimate);
                reader.Read(out salesAmount);
                reader.Read(out salesOffice);
                reader.Read(out salesPerson);
                reader.Read(out planner);
                reader.Read(out project);

                int soLinesCount;
                reader.Read(out soLinesCount);
                for (int i = 0; i < soLinesCount; i++)
                {
                    m_salesOrderLines.Add(new SalesOrderLine(reader));
                }

                bools = new BoolVector32(reader);
                reader.Read(out m_expirationDate);
            }
        }

        public override void Serialize(IWriter writer)
        {
            base.Serialize(writer);

            writer.Write(m_customerExternalId);
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

            bools.Serialize(writer);
            writer.Write(m_expirationDate);
        }

        [Browsable(false)]
        public override int UniqueId => UNIQUE_ID;
        #endregion IPTSerializable

        public SalesOrder() { } // reqd. for xml serialization

        public SalesOrder(SalesOrderTDataSet.SalesOrderRow row)
            : base(row.ExternalId, row.Name, row.IsDescriptionNull() ? "" : row.Description, row.IsNotesNull() ? "" : row.Notes, row.IsUserFieldsNull() ? null : row.UserFields)
        {
            if (!row.IsCustomerExternalIdNull())
            {
                m_customerExternalId = row.CustomerExternalId;
            }

            if (!row.IsPlannerNull())
            {
                planner = row.Planner;
            }

            if (!row.IsProjectNull())
            {
                project = row.Project;
            }

            if (!row.IsEstimateNull())
            {
                estimate = row.Estimate;
            }

            if (!row.IsSalesAmountNull())
            {
                salesAmount = row.SalesAmount;
            }

            if (!row.IsSalesOfficeNull())
            {
                salesOffice = row.SalesOffice;
            }

            if (!row.IsSalesPersonNull())
            {
                salesPerson = row.SalesPerson;
            }

            if (!row.IsCancelledNull())
            {
                Cancelled = row.Cancelled;
            }

            if (!row.IsCancelAtExpirationDateNull())
            {
                CancelAtExpirationDate = row.CancelAtExpirationDate;
            }

            if (!row.IsExpirationDateNull())
            {
                ExpirationDate = row.ExpirationDate;
            }

            SalesOrderTDataSet.SalesOrderLineRow[] soLineRows = row.GetSalesOrderLineRows();
            for (int i = 0; i < soLineRows.Length; i++)
            {
                m_salesOrderLines.Add(new SalesOrderLine((SalesOrderTDataSet.SalesOrderLineRow)soLineRows.GetValue(i)));
            }
        }

        #region Shared Properties
        private string m_customerExternalId;

        public string CustomerExternalId
        {
            get => m_customerExternalId;
            set => m_customerExternalId = value;
        }

        private bool estimate;

        /// <summary>
        /// If true then this is a quote, not a firm order.
        /// </summary>
        public bool Estimate
        {
            get => estimate;
            set => estimate = value;
        }

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
        /// </summary>
        public bool Cancelled
        {
            get => bools[cancelledIdx];
            set => bools[cancelledIdx] = value;
        }

        /// <summary>
        /// If true then the Sales Order is marked as Cancelled when the PlanetTogether Clock passes the Expiration Date.
        /// </summary>
        public bool CancelAtExpirationDate
        {
            get => bools[cancelAtExpireDateIdx];
            set => bools[cancelAtExpireDateIdx] = value;
        }

        private DateTime m_expirationDate = PTDateTime.MaxDateTime;

        /// <summary>
        /// If CancelAtExpirationDate is true then the Sales Order is marked as Cancelled when the PlanetTogether Clock passes this date.
        /// </summary>
        public DateTime ExpirationDate
        {
            get => m_expirationDate;
            set => m_expirationDate = value;
        }
        #endregion Shared Properties

        public new void Validate()
        {
            HashSet<string> lineNumbersAlreadyAddedHash = new ();
            for (int i = 0; i < m_salesOrderLines.Count; i++)
            {
                SalesOrderLine sol = SalesOrderLines[i];
                sol.Validate();
                if (lineNumbersAlreadyAddedHash.Contains(sol.LineNumber))
                {
                    throw new PTValidationException("2117", new object[] { ExternalId, sol.LineNumber });
                }

                lineNumbersAlreadyAddedHash.Add(sol.LineNumber);
            }
        }

        private readonly List<SalesOrderLine> m_salesOrderLines = new ();

        public List<SalesOrderLine> SalesOrderLines => m_salesOrderLines;

        public class SalesOrderLine : IPTSerializable
        {
            #region IPTSerializable Members
            public const int UNIQUE_ID = 643;

            public SalesOrderLine(IReader reader)
            {
                if (reader.VersionNumber >= 364)
                {
                    reader.Read(out lineNumber);
                    reader.Read(out description);
                    reader.Read(out unitPrice);
                    reader.Read(out m_itemExternalId);

                    int lineDistCount;
                    reader.Read(out lineDistCount);
                    for (int i = 0; i < lineDistCount; i++)
                    {
                        m_lineDistributions.Add(new SalesOrderLineDistribution(reader));
                    }
                }

                #region Version 1
                else
                {
                    int oldIntLineNumber;
                    reader.Read(out oldIntLineNumber);
                    lineNumber = oldIntLineNumber.ToString();
                    reader.Read(out description);
                    reader.Read(out unitPrice);
                    reader.Read(out m_itemExternalId);

                    int lineDistCount;
                    reader.Read(out lineDistCount);
                    for (int i = 0; i < lineDistCount; i++)
                    {
                        m_lineDistributions.Add(new SalesOrderLineDistribution(reader));
                    }
                }
                #endregion
            }

            public void Serialize(IWriter writer)
            {
                writer.Write(lineNumber);
                writer.Write(description);
                writer.Write(unitPrice);
                writer.Write(m_itemExternalId);

                writer.Write(m_lineDistributions.Count);
                for (int i = 0; i < m_lineDistributions.Count; i++)
                {
                    m_lineDistributions[i].Serialize(writer);
                }
            }

            [Browsable(false)]
            public int UniqueId => UNIQUE_ID;
            #endregion IPTSerializable

            public SalesOrderLine() { } // reqd. for xml serialization

            public SalesOrderLine(SalesOrderTDataSet.SalesOrderLineRow a_row)
            {
                m_itemExternalId = a_row.ItemExternalId;
                lineNumber = a_row.LineNumber;

                if (!a_row.IsDescriptionNull())
                {
                    description = a_row.Description;
                }

                if (!a_row.IsUnitPriceNull())
                {
                    unitPrice = a_row.UnitPrice;
                }

                SalesOrderTDataSet.SalesOrderLineDistRow[] lineDistRows = a_row.GetSalesOrderLineDistRows();
                for (int i = 0; i < lineDistRows.Length; i++)
                {
                    SalesOrderTDataSet.SalesOrderLineDistRow lineDistRow = (SalesOrderTDataSet.SalesOrderLineDistRow)lineDistRows.GetValue(i);
                    m_lineDistributions.Add(new SalesOrderLineDistribution(lineDistRow));
                }
            }

            private readonly string m_itemExternalId;

            /// <summary>
            /// The item being ordered.
            /// </summary>
            public string ItemExternalId => m_itemExternalId;

            #region Shared Properties
            private string lineNumber;

            /// <summary>
            /// The identifier for a line item within the Sales Order.
            /// </summary>
            public string LineNumber
            {
                get => lineNumber;
                set => lineNumber = value;
            }

            private string description;

            public string Description
            {
                get => description;
                set => description = value;
            }

            private decimal unitPrice;

            /// <summary>
            /// The sale price per unit.  This can be used to maximize sales revenue.
            /// </summary>
            public decimal UnitPrice
            {
                get => unitPrice;
                set => unitPrice = value;
            }
            #endregion Shared Properties

            public void Validate()
            {
                if (lineNumber == null)
                {
                    throw new PTValidationException("2118");
                }

                if (ItemExternalId == null || ItemExternalId.Trim() == "")
                {
                    throw new PTValidationException("2119");
                }

                foreach (SalesOrderLineDistribution lineDistribution in m_lineDistributions)
                {
                    lineDistribution.Validate();
                }
            }

            private readonly List<SalesOrderLineDistribution> m_lineDistributions = new ();

            public List<SalesOrderLineDistribution> LineDistributions => m_lineDistributions;

            public class SalesOrderLineDistribution : IPTSerializable
            {
                #region IPTSerializable Members
                public const int UNIQUE_ID = 644;

                public SalesOrderLineDistribution(IReader a_reader)
                {
                    #region 739
                    if (a_reader.VersionNumber >= 739)
                    {
                        m_bools = new BoolVector32(a_reader);
                        a_reader.Read(out qtyOrdered);
                        a_reader.Read(out qtyShipped);
                        a_reader.Read(out requiredAvailableDate);
                        a_reader.Read(out shipToZone);
                        a_reader.Read(out salesRegion);
                        a_reader.Read(out closed);
                        a_reader.Read(out hold);
                        a_reader.Read(out holdReason);
                        a_reader.Read(out priority);
                        a_reader.Read(out maximumLateness);
                        a_reader.Read(out allowPartialAllocations);
                        a_reader.Read(out minAllocationQty);

                        a_reader.Read(out int stockShortageRuleInt);
                        stockShortageRule = (SalesOrderDefs.StockShortageRules)stockShortageRuleInt;
                        a_reader.Read(out m_mustSupplyFromWarehouseExternalId);
                        a_reader.Read(out int val);
                        for (int i = 0; i < val; i++)
                        {
                            a_reader.Read(out string lotCode);
                            m_allowedLotCodes.Add(lotCode);
                        }

                        a_reader.Read(out val);
                        m_materialAllocation = (ItemDefs.MaterialAllocation)val;
                        a_reader.Read(out m_minSourceQty);
                        a_reader.Read(out m_maxSourceQty);
                        a_reader.Read(out val);
                        m_materialSourcing = (ItemDefs.MaterialSourcing)val;
                    }
                    #endregion

                    else
                    {
                        #region 731
                        if (a_reader.VersionNumber >= 731)
                        {
                            a_reader.Read(out qtyOrdered);
                            a_reader.Read(out qtyShipped);
                            a_reader.Read(out requiredAvailableDate);
                            a_reader.Read(out shipToZone);
                            a_reader.Read(out salesRegion);
                            a_reader.Read(out closed);
                            a_reader.Read(out hold);
                            a_reader.Read(out holdReason);
                            a_reader.Read(out priority);
                            a_reader.Read(out maximumLateness);
                            a_reader.Read(out allowPartialAllocations);
                            a_reader.Read(out minAllocationQty);

                            int stockShortageRuleInt;
                            a_reader.Read(out stockShortageRuleInt);
                            stockShortageRule = (SalesOrderDefs.StockShortageRules)stockShortageRuleInt;
                            a_reader.Read(out m_mustSupplyFromWarehouseExternalId);
                            a_reader.Read(out int val);
                            for (int i = 0; i < val; i++)
                            {
                                string lotCode;
                                a_reader.Read(out lotCode);
                                m_allowedLotCodes.Add(lotCode);
                            }

                            a_reader.Read(out val);
                            m_materialAllocation = (ItemDefs.MaterialAllocation)val;
                            a_reader.Read(out m_minSourceQty);
                            a_reader.Read(out m_maxSourceQty);
                            a_reader.Read(out val);
                            m_materialSourcing = (ItemDefs.MaterialSourcing)val;
                        }
                        #endregion

                        #region 680
                        else if (a_reader.VersionNumber >= 680)
                        {
                            a_reader.Read(out qtyOrdered);
                            a_reader.Read(out qtyShipped);
                            a_reader.Read(out requiredAvailableDate);
                            a_reader.Read(out shipToZone);
                            a_reader.Read(out salesRegion);
                            a_reader.Read(out closed);
                            a_reader.Read(out hold);
                            a_reader.Read(out holdReason);
                            a_reader.Read(out priority);
                            a_reader.Read(out maximumLateness);
                            a_reader.Read(out allowPartialAllocations);
                            a_reader.Read(out minAllocationQty);

                            int stockShortageRuleInt;
                            a_reader.Read(out stockShortageRuleInt);
                            stockShortageRule = (SalesOrderDefs.StockShortageRules)stockShortageRuleInt;
                            a_reader.Read(out m_mustSupplyFromWarehouseExternalId);
                            a_reader.Read(out int val);
                            for (int i = 0; i < val; i++)
                            {
                                string lotCode;
                                a_reader.Read(out lotCode);
                                m_allowedLotCodes.Add(lotCode);
                            }
                        }
                        #endregion

                        #region 670
                        else if (a_reader.VersionNumber >= 670)
                        {
                            a_reader.Read(out qtyOrdered);
                            a_reader.Read(out qtyShipped);
                            a_reader.Read(out requiredAvailableDate);
                            a_reader.Read(out shipToZone);
                            a_reader.Read(out salesRegion);
                            a_reader.Read(out closed);
                            a_reader.Read(out hold);
                            a_reader.Read(out holdReason);
                            a_reader.Read(out priority);
                            a_reader.Read(out maximumLateness);
                            a_reader.Read(out allowPartialAllocations);
                            a_reader.Read(out minAllocationQty);

                            int stockShortageRuleInt;
                            a_reader.Read(out stockShortageRuleInt);
                            stockShortageRule = (SalesOrderDefs.StockShortageRules)stockShortageRuleInt;
                            a_reader.Read(out m_mustSupplyFromWarehouseExternalId);
                        }
                        #endregion

                        else
                        {
                            a_reader.Read(out qtyOrdered);
                            decimal orderToShip;
                            a_reader.Read(out orderToShip);
                            QtyShipped = Math.Max(0, qtyOrdered - orderToShip);
                            a_reader.Read(out requiredAvailableDate);
                            a_reader.Read(out shipToZone);
                            a_reader.Read(out salesRegion);
                            a_reader.Read(out closed);
                            a_reader.Read(out hold);
                            a_reader.Read(out holdReason);
                            a_reader.Read(out priority);
                            a_reader.Read(out maximumLateness);
                            a_reader.Read(out allowPartialAllocations);
                            a_reader.Read(out minAllocationQty);

                            int stockShortageRuleInt;
                            a_reader.Read(out stockShortageRuleInt);
                            stockShortageRule = (SalesOrderDefs.StockShortageRules)stockShortageRuleInt;
                            a_reader.Read(out m_mustSupplyFromWarehouseExternalId);
                        }

                        if (!string.IsNullOrEmpty(m_mustSupplyFromWarehouseExternalId))
                        {
                            UseMustSupplyFromWarehouseExternalId = true;
                        }
                    }
                }

                public void Serialize(IWriter a_writer)
                {
                    m_bools.Serialize(a_writer);
                    a_writer.Write(qtyOrdered);
                    a_writer.Write(qtyShipped);
                    a_writer.Write(requiredAvailableDate);
                    a_writer.Write(shipToZone);
                    a_writer.Write(salesRegion);
                    a_writer.Write(closed);
                    a_writer.Write(hold);
                    a_writer.Write(holdReason);
                    a_writer.Write(priority);
                    a_writer.Write(maximumLateness);
                    a_writer.Write(allowPartialAllocations);
                    a_writer.Write(minAllocationQty);

                    a_writer.Write((int)stockShortageRule);
                    a_writer.Write(m_mustSupplyFromWarehouseExternalId);
                    a_writer.Write(m_allowedLotCodes.Count);
                    foreach (string allowedLotCode in m_allowedLotCodes)
                    {
                        a_writer.Write(allowedLotCode);
                    }

                    a_writer.Write((int)m_materialAllocation);
                    a_writer.Write(m_minSourceQty);
                    a_writer.Write(m_maxSourceQty);
                    a_writer.Write((int)m_materialSourcing);
                }

                [Browsable(false)]
                public int UniqueId => UNIQUE_ID;
                #endregion IPTSerializable

                public SalesOrderLineDistribution() { } // reqd. for xml serialization

                public SalesOrderLineDistribution(SalesOrderTDataSet.SalesOrderLineDistRow row)
                {
                    qtyOrdered = row.QtyOrdered;

                    requiredAvailableDate = row.RequiredAvailableDate.ToServerTime();

                    if (!row.IsAllowPartialAllocationsNull())
                    {
                        allowPartialAllocations = row.AllowPartialAllocations;
                    }

                    if (!row.IsClosedNull())
                    {
                        closed = row.Closed;
                    }

                    if (!row.IsHoldNull())
                    {
                        hold = row.Hold;
                    }

                    if (!row.IsHoldReasonNull())
                    {
                        holdReason = row.HoldReason;
                    }

                    if (!row.IsMaximumLatenessDaysNull())
                    {
                        maximumLateness = TimeSpan.FromDays(row.MaximumLatenessDays);
                    }

                    if (!row.IsMinAllocationQtyNull())
                    {
                        minAllocationQty = row.MinAllocationQty;
                    }

                    if (!row.IsPriorityNull())
                    {
                        priority = row.Priority;
                    }

                    if (!row.IsQtyShippedNull())
                    {
                        QtyShipped = row.QtyShipped;
                    }

                    if (!row.IsSalesRegionNull())
                    {
                        salesRegion = row.SalesRegion;
                    }

                    if (!row.IsShipToZoneNull())
                    {
                        shipToZone = row.ShipToZone;
                    }

                    if (!row.IsStockShortageRuleNull())
                    {
                        stockShortageRule = (SalesOrderDefs.StockShortageRules)Enum.Parse(typeof(SalesOrderDefs.StockShortageRules), row.StockShortageRule);
                    }

                    if (!row.IsAllowedLotCodesNull())
                    {
                        if (!string.IsNullOrEmpty(row.AllowedLotCodes))
                        {
                            AllowedLotCodes.AddRange(row.AllowedLotCodes.Split(','));
                        }
                    }

                    if (!row.IsMinAllocationQtyNull())
                    {
                        m_materialAllocation = (ItemDefs.MaterialAllocation)Enum.Parse(typeof(ItemDefs.MaterialAllocation), row.MaterialAllocation);
                    }

                    if (!row.IsMaterialSourcingNull())
                    {
                        m_materialSourcing = (ItemDefs.MaterialSourcing)Enum.Parse(typeof(ItemDefs.MaterialSourcing), row.MaterialSourcing);
                    }

                    if (!row.IsMinSourceQtyNull())
                    {
                        m_minSourceQty = row.MinSourceQty;
                    }

                    if (!row.IsMaxSourceQtyNull())
                    {
                        m_maxSourceQty = row.MaxSourceQty;
                    }

                    if (!row.IsUseMustSupplyFromWarehouseExternalIdNull())
                    {
                        UseMustSupplyFromWarehouseExternalId = row.UseMustSupplyFromWarehouseExternalId;

                        if (row.IsMustSupplyFromWarehouseExternalIdNull() || string.IsNullOrEmpty(row.MustSupplyFromWarehouseExternalId))
                        {
                            MustSupplyFromWarehouseExternalId = null;
                        }
                        else
                        {
                            MustSupplyFromWarehouseExternalId = row.MustSupplyFromWarehouseExternalId;
                        }
                    }
                }

                #region Shared Properties
                private BoolVector32 m_bools;
                private const int c_useMustSupplyFromWarehouseExternalIdIsSetIdx = 0;

                private string m_mustSupplyFromWarehouseExternalId;

                /// <summary>
                /// The demand must be satisfied by the specified Warehouse only.
                /// </summary>
                public string MustSupplyFromWarehouseExternalId
                {
                    get => m_mustSupplyFromWarehouseExternalId;
                    private set => m_mustSupplyFromWarehouseExternalId = value;
                }

                public bool UseMustSupplyFromWarehouseExternalId
                {
                    get => m_bools[c_useMustSupplyFromWarehouseExternalIdIsSetIdx];
                    set => m_bools[c_useMustSupplyFromWarehouseExternalIdIsSetIdx] = value;
                }

                private decimal qtyOrdered;

                /// <summary>
                /// The total qty for this Line Item Distribution on the order.  This remains the same even if there is a partial shipment made.
                /// </summary>
                public decimal QtyOrdered
                {
                    get => qtyOrdered;
                    set => qtyOrdered = value;
                }

                private decimal qtyShipped;

                /// <summary>
                /// This is the remaining qty that must be planned for.  If a partial shipment is made then this is the QtyOrdered minus partial shipments.
                /// </summary>
                public decimal QtyShipped
                {
                    get => qtyShipped;
                    set => qtyShipped = value;
                }

                private readonly decimal m_minSourceQty;

                public decimal MinSourceQty => m_minSourceQty;

                private readonly decimal m_maxSourceQty;

                public decimal MaxSourceQty => m_maxSourceQty;

                private ItemDefs.MaterialSourcing m_materialSourcing = ItemDefs.MaterialSourcing.NotSet;

                public ItemDefs.MaterialSourcing MaterialSourcing
                {
                    get => m_materialSourcing;
                    private set => m_materialSourcing = value;
                }

                private DateTime requiredAvailableDate;

                /// <summary>
                /// The date when the material must be available in stock in order to reach the customer by the Promised Delivery Date.
                /// </summary>
                public DateTime RequiredAvailableDate
                {
                    get => requiredAvailableDate;
                    set => requiredAvailableDate = value;
                }

                private string shipToZone;

                /// <summary>
                /// Specifies the geographic are where the shipment is going.  For information only.
                /// </summary>
                public string ShipToZone
                {
                    get => shipToZone;
                    set => shipToZone = value;
                }

                private string salesRegion;

                /// <summary>
                /// The geographic region for the shipment.  For information only.
                /// </summary>
                public string SalesRegion
                {
                    get => salesRegion;
                    set => salesRegion = value;
                }

                private bool closed;

                /// <summary>
                /// If marked as Closed then the it no longer affects planning, regardless of the QtyOpenToShip.
                /// </summary>
                public bool Closed
                {
                    get => closed;
                    set => closed = value;
                }

                private bool hold;

                /// <summary>
                /// If true then the shipment is ignored in planning.
                /// </summary>
                public bool Hold
                {
                    get => hold;
                    set => hold = value;
                }

                private string holdReason;

                public string HoldReason
                {
                    get => holdReason;
                    set => holdReason = value;
                }

                private int priority;

                /// <summary>
                /// Indicates the importance of the shipment.  Used during the allocation process to determine which requirements to allocate to first.
                /// Shipments with lower numbers are considered more important and receive allocation before shipments with higher numbers.
                /// Allocation is based upon the Allocation Rule specified during the time a Simulation is performed.
                /// </summary>
                public int Priority
                {
                    get => priority;
                    set => priority = value;
                }

                private SalesOrderDefs.StockShortageRules stockShortageRule;

                /// <summary>
                /// Specifies what should be done in stock planning when the shipments full QtyOpenToShip cannot be satisfied.
                /// This can also be overridden during Optimizes with a global rule.
                /// </summary>
                public SalesOrderDefs.StockShortageRules StockShortageRule
                {
                    get => stockShortageRule;
                    set => stockShortageRule = value;
                }

                private TimeSpan maximumLateness;

                /// <summary>
                /// If using StockShortageRule of PushLater and the demand has been pushed this amount past the Required Available Date then it is marked as a Missed Sale.
                /// </summary>
                public TimeSpan MaximumLateness
                {
                    get => maximumLateness;
                    set => maximumLateness = value;
                }

                private bool allowPartialAllocations;

                /// <summary>
                /// If true then if there is not enough stock to satisfy a shipment, a portion of the requirement can't be met from stock and a portion from
                /// either Backlog or MissedSale.  If false, then the entire qty must be allocated.
                /// </summary>
                public bool AllowPartialAllocations
                {
                    get => allowPartialAllocations;
                    set => allowPartialAllocations = value;
                }

                private decimal minAllocationQty;

                /// <summary>
                /// If AllowPartialAllocations is true then this is the minimum amount that must be allocated to the shipment.
                /// If this amount is not available then the shipment's
                /// </summary>
                public decimal MinAllocationQty
                {
                    get => minAllocationQty;
                    set => minAllocationQty = value;
                }

                private List<string> m_allowedLotCodes = new ();

                public List<string> AllowedLotCodes
                {
                    get => m_allowedLotCodes;
                    set => m_allowedLotCodes = value ?? new List<string>();
                }

                private ItemDefs.MaterialAllocation m_materialAllocation = ItemDefs.MaterialAllocation.NotSet; // 2020.11.04: UseOldestFirst was the default prior to the change for task 12010 for a specific customer. It has been chosen again as the default because other than the one customer, no one had ever had an issue with it before. 

                /// <summary>
                /// How material should be used either use the earliest created or the latest created.
                /// </summary>
                public ItemDefs.MaterialAllocation MaterialAllocation
                {
                    get => m_materialAllocation;
                    set => m_materialAllocation = value;
                }
                #endregion Shared Properties

                public void Validate()
                {
                    if (UseMustSupplyFromWarehouseExternalId && string.IsNullOrEmpty(MustSupplyFromWarehouseExternalId))
                    {
                        throw new PTValidationException("3042", new object[] { "MustSupplyFromWarehouseExternalId".Localize(), "UseMustSupplyFromWarehouseExternalId".Localize() });
                    }
                }
            }
        }
    }
}