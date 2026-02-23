using PT.APSCommon;
using PT.Common.Collections;
using PT.Scheduler.Demand;
using PT.Scheduler.Schedule;
using PT.Scheduler.Simulation.Events;

namespace PT.Scheduler;

/// <summary>
/// Used to track the total demand at a point in time.
/// </summary>
internal class DemandTtl
{
    internal DemandTtl(long a_date, decimal a_qty)
    {
        Date = a_date;
        Qty = a_qty;
    }

    internal long Date { get; set; }

    internal decimal Qty { get; set; }

    internal void Merge(DemandTtl a_dt)
    {
        Qty += a_dt.Qty;
    }

    public override string ToString()
    {
        return string.Format("Date:{0}; Qty:{1}", DateTimeHelper.ToLocalTimeFromUTCTicks(Date), Qty);
    }
}

/// <summary>
/// Used to compare demands for sorting.
/// </summary>
internal class DemandTotalComparer : IComparer<DemandTtl>
{
    public int Compare(DemandTtl x, DemandTtl y)
    {
        if (x.Date < y.Date)
        {
            return -1;
        }

        if (x.Date == y.Date)
        {
            return 0;
        }

        return 1;
    }
}

/// <summary>
/// Keeps track of the Items stored in a particular Warehouse.
/// </summary>
public partial class Inventory
{
    /// <summary>
    /// The earliest date there isn't enough quantity to satisfy a demand for this Inventory
    /// </summary>
    private long m_firstShortageDateTicks;

    public DateTime GetFirstShortageDate()
    {
        if (m_firstShortageDateTicks == long.MaxValue)
        {
            return new DateTime(PTDateTime.MaxDateTimeTicks);
        }

        return new DateTime(m_firstShortageDateTicks);
    }

    internal void TrackFirstShortageDate(long a_shortageDateTicks)
    {
        if (a_shortageDateTicks < m_firstShortageDateTicks || m_firstShortageDateTicks == PTDateTime.InvalidDateTimeTicks)
        {
            m_firstShortageDateTicks = a_shortageDateTicks;
        }
    }

    /// <summary>
    /// Call this right before the start of a simulation.
    /// </summary>
    internal void ResetSimulationStateVariables(long a_now, ScenarioDetail a_sd)
    {
        m_cachedAdjustments = null;
        m_adjustments.Clear();
        m_simAdjustments.Clear();
        //OnHandQty = onHandQty;

        m_inventoryLevelCalculated = false;

        a_sd.InitializeCache(ref m_onHandInvCache);

        if (m_demandTotals == null)
        {
            // [DaysOnHand:Synchronization]
            // If demand totals haven't beeen created yet, create them now.
            // Usually they will have been created by a previous simulation and
            // kept up to date as changes to demand have occurred.
            CalcDemandTotals(a_sd.SalesOrderManager);
        }

        //m_daysOnHandStartRangeEtr = m_demandTotals.GetEnumerator();
        //m_daysOnHandEndRangeEtr = m_demandTotals.GetEnumerator();
        a_sd.InitializeCache(ref m_requirementAdjustmentCache);

        Lots.ResetSimulationStateVariables();

        m_firstShortageDateTicks = PTDateTime.InvalidDateTimeTicks;
    }

    /// <summary>
    /// Cached OnHand inventory based on the lots that have storage before the clock.
    /// </summary>
    private ICalculatedValueCache<decimal> m_onHandInvCache;

    private AdjustmentArray m_cachedAdjustments;

    private readonly AdjustmentArray m_adjustments; //Adjustments stored on inventory that need to be serialized and restored
    private readonly AdjustmentArray m_simAdjustments; //Adjustments that are stored on another object but linked here for convenience

    /// <summary>
    /// This is a helper function to return adjustments for all lots for this inventory.
    /// </summary>
    internal AdjustmentArray Adjustments => GetAdjustmentArray();

    /// <summary>
    /// This is for access outside of this project.
    /// </summary>
    /// <returns></returns>
    public AdjustmentArray GetAdjustmentArray()
    {
        if (m_cachedAdjustments != null)
        {
            return m_cachedAdjustments;
        }

        lock (this)
        {
            if (m_cachedAdjustments != null)
            {
                return m_cachedAdjustments;
            }
            
            //Start with the simulation adjustments for this inventory
            AdjustmentArray allLotAdjustments = new (m_adjustments);
            //Add all linked adjustments
            allLotAdjustments.Add(m_simAdjustments);

            //Add all lot adjustments
            foreach (Lot lot in m_lots)
            {
                allLotAdjustments.Add(lot.GetAdjustmentArray());
            }
            
            allLotAdjustments.Sort();
            m_cachedAdjustments = allLotAdjustments;
            return m_cachedAdjustments;
        }
    }

    /// <summary>
    /// This is for access outside of this project. Accessing the unsorted array allows simulation to still sort and modify the list.
    /// </summary>
    /// <returns></returns>
    public ReadOnlyAdjutmentArray GetAdjustmentArrayUnsorted()
    {
        return new ReadOnlyAdjutmentArray(GetAdjustmentArray());
    }

    internal void ForecastsAddToAdjustmentProfile()
    {
        if (ForecastVersionActive != null)
        {
            for (int fvsI = 0; fvsI < ForecastVersionActive.Forecasts.Count; ++fvsI)
            {
                Forecast forecast = ForecastVersionActive.Forecasts[fvsI];
                using (IEnumerator<ForecastShipment> shipmentEnumerator = forecast.GetEnumerator())
                {
                    while (shipmentEnumerator.MoveNext())
                    {
                        ForecastShipment forecastShipment = shipmentEnumerator.Current;
                        Adjustments.Add(new ForecastMrpDemandAdjustment(forecastShipment, this, -forecastShipment.RequiredQty));
                    }
                }
            }
        }
    }

    

    /// <summary>
    /// The sum of all the demands (currently just sales orders and forecasts). Each entry in this sollection represents  the total demand at a point in time.
    /// This collection should rarly change and is reusable between simulations; it's not altered by the simulation.
    /// It's kept in sync with demands as they change.
    /// </summary>
    private AVLTree<long, DemandTtl> m_demandTotals;

    private AVLTree<long, DemandTtl>.Enumerator m_daysOnHandStartRangeEtr;
    private AVLTree<long, DemandTtl>.Enumerator m_daysOnHandEndRangeEtr;

    /// <summary>
    /// resynchronize the m_demandTotals collection with demands( Forecasts and sales orders).
    /// This function should be called after all changes to demand have been completed
    /// or if a simulation is being performed and the collection hasn't been initialized yet, which might happen
    /// to existing scenarios. Since new scenarios keep the collection up to date as demands change they don't need to be updated
    /// during simulations.
    /// [DaysOnHand:SyncDataStruct:1]
    /// </summary>
    /// <param name="a_sm"></param>
    internal void CalcDemandTotals(SalesOrderManager a_sm)
    {
        if (m_demandTotals == null)
        {
            m_demandTotals = new AVLTree<long, DemandTtl>(Comparer<long>.Default);
        }

        m_demandTotals.Clear();

        return;
        //This doesn't do anything in v12, but it is performance intensive
        IEnumerator<SalesOrder> soEtr = a_sm.GetEnumerator();

        // Add sales order demands.
        while (soEtr.MoveNext())
        {
            SalesOrder so = soEtr.Current;
            if (!so.Cancelled)
            {
                foreach (SalesOrderLine sol in so.SalesOrderLines)
                {
                    foreach (SalesOrderLineDistribution sod in sol)
                    {
                        if (sod.Item == Item)
                        {
                            AVLTree<long, DemandTtl>.TreeNode n = m_demandTotals.Find(sod.RequiredAvailableDateTicks);
                            if (n != null)
                            {
                                // The sales order is merged with another sales order at the same time. 
                                DemandTtl dt = n.Value;
                                dt.Qty += sod.QtyOpenToShip;
                            }
                            else
                            {
                                m_demandTotals.Add(sod.RequiredAvailableDateTicks, new DemandTtl(sod.RequiredAvailableDateTicks, sod.QtyOpenToShip));
                            }
                        }
                    }
                }
            }
        }

        // Merge forecast demands into the collection.
        if (ForecastVersionActive != null)
        {
            foreach (Forecast f in ForecastVersionActive.Forecasts)
            {
                IEnumerator<ForecastShipment> fsEtr = f.GetEnumerator();
                while (fsEtr.MoveNext())
                {
                    ForecastShipment fs = fsEtr.Current;

                    AVLTree<long, DemandTtl>.TreeNode n = m_demandTotals.Find(fs.RequireDateTicks);
                    if (n != null)
                    {
                        // The forecast is merged with an existing demands.
                        DemandTtl dt = n.Value;
                        dt.Qty += fs.RequiredQty;
                    }
                    else
                    {
                        m_demandTotals.Add(fs.RequireDateTicks, new DemandTtl(fs.RequireDateTicks, fs.GetUnconsumedQty()));
                    }
                }
            }
        }
    }

    /// <summary>
    /// A helper of CalcDaysOnHand used to move the iterators m_daysOnHandStartRangeEtr and m_daysOnHandEndRangeEtr to the
    /// start and end of the segment of nodes whose quantities need to be summed.
    /// Move an enumerator to the first node before a specified date.
    /// [DaysOnHand:CalcDaysOnHand:3]
    /// </summary>
    /// <param name="a_ticks">The date you want to enumerator to be positioned before.</param>
    /// <param name="a_etr">The enumerator to iterate through. This value is changed to the first node prior to </param>
    /// <returns>Whether the iterator was moved. Examples of when the iterator might not move include: the collection being empty, the first MoveNext resulting in a node past the desired date.</returns>
    private bool MoveNextOnOrBefore(long a_ticks, AVLTree<long, DemandTtl>.Enumerator a_etr)
    {
        bool initialized = false;
        // Save the value of the enumerator before applying MoveNext so the enumerator can
        // be reverted back to this value.

        AVLTree<long, DemandTtl>.Enumerator b4MoveNext = new (a_etr);
        while (a_etr.MoveNext())
        {
            if (a_etr.Current.Key >= a_ticks)
            {
                // A node was found that on or after the specified date.
                // The node before this Current node will be returned on breaking. 
                break;
            }

            // Save the previous enumerator position before MoveNext().
            b4MoveNext.CopyPosition(a_etr);
            initialized = true;
        }

        // In the case where the node is found, b4MoveNext is equal to the node before the current node.
        // In the case where the node isn't found, b4MoveNext is equal to the last node in the collection.
        a_etr.CopyPosition(b4MoveNext);
        return initialized;
    }

    /// <summary>
    /// Using the inventory availble at the simulation clock, determine how many days it will be able to fulfill the demands
    /// between the current simulation clock and this inventory's DaysOnHand.
    /// [DaysOnHand:CalcDaysOnHand:2]
    /// </summary>
    /// <param name="a_simClock"></param>
    /// <returns>The number of days on hand within </returns>
    internal long CalcDaysOnHand(long a_simClock)
    {
        long daysOnHand = DaysOnHandTicks;
        if (m_demandTotals.Count > 0)
        {
            // Initially presume there is enough quantity for the target days on hand.
            // If there isn't enough, this value will be adjusted below before being
            // returned.

            // Move the iterators to the range covered by the days on hand.
            // Each time this function is called, the iterators are advanced up to the point they are needed and sit 
            // there until the next time the simulation clock changes. So, it should never take much work to find
            // the next 
            // The start iterator is moved to the node right before the simulation clock. So on the first MoveNext()
            // will start off at the simulation clock or the first node after it.
            bool initializedEtr = MoveNextOnOrBefore(a_simClock, m_daysOnHandStartRangeEtr);

            if (initializedEtr)
            {
                // The end iterator is moved to the node right before the end of the range of days on hand. So its first Current
                // is on the node at the end of the days on hand range.

                // Remove this iterator. Instead, you can figure out where to stop while enumerating through the collection below.
                long daysOnHandEndTicks = a_simClock + DaysOnHandTicks;
                MoveNextOnOrBefore(daysOnHandEndTicks, m_daysOnHandEndRangeEtr);

                DemandTtl endRangeTtl = m_daysOnHandEndRangeEtr.Current.Value;

                AVLTree<long, DemandTtl>.Enumerator dayEtr = new (m_daysOnHandStartRangeEtr);

                decimal availQty = 0; //m_simQty;


                do
                {
                    DemandTtl dt = dayEtr.Current.Value;
                    if (dt.Date >= a_simClock)
                    {
                        availQty -= dt.Qty;
                        if (availQty <= 0)
                        {
                            daysOnHand -= dt.Date - a_simClock;
                            break;
                        }
                    }
                } while (dayEtr.Current.Value != endRangeTtl && dayEtr.MoveNext());
            }
        }

        return daysOnHand;
    }

    public override BaseId GetKey()
    {
        return m_item?.Id ?? m_restoreInfo.ItemId;
    }
    
    internal Lot CreateLot(Inventory a_inv, SupplyProfile a_productQtyProfile, BaseId a_partialProductionLot)
    {
        Lot lot;
        if (a_partialProductionLot != BaseId.NULL_ID && Lots.GetById(a_partialProductionLot) is Lot partialLot)
        {
            lot = partialLot;
        }
        else
        {
            lot = m_lots.AddLot(a_productQtyProfile, a_productQtyProfile.Source.LotCode, a_productQtyProfile.LotSource);
        }

        //Link the lot back to the object that created it
        a_productQtyProfile.Source.LinkCreatedLot(lot);
        a_productQtyProfile.SetLot(lot);

        return lot;
    }

    /// <summary>
    /// Add an adjustment that needs to be stored on Inventory
    /// </summary>
    /// <param name="a_invAdjustment"></param>
    internal void AddScheduleAdjustment(Adjustment a_invAdjustment)
    {
        m_adjustments.Add(a_invAdjustment);
    }

    /// <summary>
    /// Link an adjustment that is stored on another object
    /// </summary>
    /// <param name="a_invAdjustment"></param>
    internal void AddSimulationAdjustment(Adjustment a_invAdjustment)
    {
        m_simAdjustments.Add(a_invAdjustment);
    }

    internal void SimulationInitialization(long a_clockDate, ref List<QtyToStockEvent> r_retryEvents)
    {
        Lots.SimulationInitialization(a_clockDate, ref r_retryEvents);
    }
}