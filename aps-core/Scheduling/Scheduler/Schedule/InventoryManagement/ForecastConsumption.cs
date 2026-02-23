using PT.Scheduler.Demand;

namespace PT.Scheduler;

internal interface IForecastConsumptionStrategy
{
    void ConsumeForecasts(ScenarioDetail a_sd, Inventory a_inv);
}

public class BackwardForecastConsumption : IForecastConsumptionStrategy
{
    /// <summary>
    /// Search for Forecasts with the same or earlier period. If none are found, no consumption occurs.
    /// If one is found and its quantity is larger than the sales order quantity, consume sales order quantity and stop.
    /// If forecast quantity is smaller than sales order quantity, consume the full forecast and search for another forecast to consume
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_inv"></param>
    public void ConsumeForecasts(ScenarioDetail a_sd, Inventory a_inv)
    {
        List<SalesOrderLineDistribution> sods = a_inv.GetSalesOrderLineDistributions(a_sd, true, true);
        List<ForecastShipment> shipments = a_inv.GetForecastShipments(true, true);
        if (shipments.Count == 0)
        {
            return;
        }

        foreach (SalesOrderLineDistribution sod in sods)
        {
            ConsumeForecastsForSod(shipments, a_inv, sod);
        }
    }

    /// <summary>
    /// consumes forecasts for a given SOD and returns the qty that was not consumed.
    /// </summary>
    /// <param name="a_shipments"></param>
    /// <param name="a_inv"></param>
    /// <param name="a_sod"></param>
    /// <returns></returns>
    internal decimal ConsumeForecastsForSod(List<ForecastShipment> a_shipments, Inventory a_inv, SalesOrderLineDistribution a_sod)
    {
        IEnumerable<ForecastShipment> shipmentsBeforeSod = a_shipments.Where(fs =>
        {
            DateTime lowerBound = a_inv.ForecastConsumptionWindowDays > 0 ? a_sod.RequiredAvailableDate.Subtract(TimeSpan.FromDays(a_inv.ForecastConsumptionWindowDays)) : PTDateTime.MinDateTime;
            return fs.RequiredDate <= a_sod.RequiredAvailableDate && fs.RequiredDate >= lowerBound && fs.GetUnconsumedQty() > 0;
        });
        decimal totalQtyToConsume = a_sod.QtyOpenToShip;
        if (shipmentsBeforeSod.Count() == 0)
        {
            return totalQtyToConsume;
        }

        foreach (ForecastShipment fs in shipmentsBeforeSod.Reverse())
        {
            decimal unconsumedQty = fs.GetUnconsumedQty();
            decimal qtyToConsume = Math.Min(unconsumedQty, totalQtyToConsume);
            fs.ConsumeSalesOrder(a_sod, qtyToConsume);
            totalQtyToConsume -= qtyToConsume;
            if (totalQtyToConsume <= 0)
            {
                break;
            }
        }

        return totalQtyToConsume;
    }
}

public class ForewardForecastConsumption : IForecastConsumptionStrategy
{
    /// <summary>
    /// Same as Backward, except forecasts with periods equal to or greater than the sales order period are considered.
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_inv"></param>
    public void ConsumeForecasts(ScenarioDetail a_sd, Inventory a_inv)
    {
        List<SalesOrderLineDistribution> sods = a_inv.GetSalesOrderLineDistributions(a_sd, true, true);
        List<ForecastShipment> shipments = a_inv.GetForecastShipments(true, true);
        if (shipments.Count == 0)
        {
            return;
        }

        foreach (SalesOrderLineDistribution sod in sods)
        {
            ConsumeForecastsForSod(shipments, a_inv, sod, sod.QtyOpenToShip);
        }
    }

    internal void ConsumeForecastsForSod(List<ForecastShipment> a_shipments, Inventory a_inv, SalesOrderLineDistribution a_sod, decimal a_qtyToConsume)
    {
        IEnumerable<ForecastShipment> shipmentsBeforeSod = a_shipments.Where(fs =>
        {
            DateTime upperBound = a_inv.ForecastConsumptionWindowDays > 0 ? a_sod.RequiredAvailableDate.Add(TimeSpan.FromDays(a_inv.ForecastConsumptionWindowDays)) : PTDateTime.MaxDateTime;
            return fs.RequiredDate >= a_sod.RequiredAvailableDate && fs.RequiredDate <= upperBound && fs.GetUnconsumedQty() > 0;
        });
        if (shipmentsBeforeSod.Count() == 0)
        {
            return;
        }

        foreach (ForecastShipment fs in shipmentsBeforeSod)
        {
            decimal unconsumedQty = fs.GetUnconsumedQty();
            decimal qtyToConsumeFromFs = Math.Min(unconsumedQty, a_qtyToConsume);
            fs.ConsumeSalesOrder(a_sod, qtyToConsumeFromFs);
            a_qtyToConsume -= qtyToConsumeFromFs;
            if (a_qtyToConsume <= 0)
            {
                break;
            }
        }
    }
}

public class BackwardThenForewardForecastConsumption : IForecastConsumptionStrategy
{
    /// <summary>
    /// Starts as Backward strategy and switches to Forward when no more backward forecasts are available and sales order is not fully consumed yet.
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_inv"></param>
    public void ConsumeForecasts(ScenarioDetail a_sd, Inventory a_inv)
    {
        List<SalesOrderLineDistribution> sods = a_inv.GetSalesOrderLineDistributions(a_sd, true, true);
        List<ForecastShipment> shipments = a_inv.GetForecastShipments(true, true);
        if (shipments.Count == 0)
        {
            return;
        }

        BackwardForecastConsumption backwardStrategy = new ();
        ForewardForecastConsumption forewardStrategy = new ();

        foreach (SalesOrderLineDistribution sod in sods)
        {
            decimal remainingQtyToConsume = backwardStrategy.ConsumeForecastsForSod(shipments, a_inv, sod);
            forewardStrategy.ConsumeForecastsForSod(shipments, a_inv, sod, remainingQtyToConsume);
        }
    }
}

public class SpreadForecastConsumption : IForecastConsumptionStrategy
{
    public void ConsumeForecasts(ScenarioDetail a_sd, Inventory a_inv)
    {
        List<SalesOrderLineDistribution> sods = a_inv.GetSalesOrderLineDistributions(a_sd, true, true);
        List<ForecastShipment> shipments = a_inv.GetForecastShipments(true, true);
        if (shipments.Count == 0)
        {
            return;
        }

        foreach (SalesOrderLineDistribution sod in sods)
        {
            decimal totalQtyToConsume = sod.QtyOpenToShip;
            decimal qtyConsumed = 0;
            while (a_sd.ScenarioOptions.IsStrictlyGreaterThanZero(totalQtyToConsume))
            {
                qtyConsumed = 0;

                // find a shipment before and after this sod that has unconsumed qty
                ForecastShipment shipmentBefore = FindOpenForecastBefore(sod.RequiredAvailableDate, shipments, a_inv.ForecastConsumptionWindowDays);
                ForecastShipment shipmentAfter = FindOpenForecastAfter(sod.RequiredAvailableDate, shipments, a_inv.ForecastConsumptionWindowDays);
                if (shipmentBefore == null && shipmentAfter == null)
                {
                    break;
                }

                // find a ratio by which to distribute the sod qty among the two shipments
                double beforeToAfterRatio;
                if (shipmentBefore == null)
                {
                    beforeToAfterRatio = 0;
                }
                else if (shipmentAfter == null)
                {
                    beforeToAfterRatio = 1;
                }
                else // find ratio based on distance in time between sod and each forecast.
                {
                    long spanBetweenTwoShipments = shipmentAfter.RequireDateTicks - shipmentBefore.RequireDateTicks;
                    long spanBetweenBeforeShipmentAndSod = sod.RequiredAvailableDateTicks - shipmentBefore.RequireDateTicks;
                    beforeToAfterRatio = 1 - spanBetweenBeforeShipmentAndSod / (double)spanBetweenTwoShipments;
                }

                // consume part of the quantity from each shipment that is not null.
                if (shipmentBefore != null)
                {
                    decimal qtyToConsume = totalQtyToConsume * (decimal)beforeToAfterRatio;
                    qtyToConsume = Math.Min(shipmentBefore.GetUnconsumedQty(), qtyToConsume);
                    qtyToConsume = a_sd.ScenarioOptions.RoundQty(qtyToConsume);
                    if (a_sd.ScenarioOptions.IsStrictlyGreaterThanZero(qtyToConsume))
                    {
                        shipmentBefore.ConsumeSalesOrder(sod, qtyToConsume);
                        qtyConsumed += qtyToConsume;
                    }
                }

                if (shipmentAfter != null)
                {
                    decimal qtyToConsume = totalQtyToConsume * (1 - (decimal)beforeToAfterRatio);
                    qtyToConsume = Math.Min(shipmentAfter.GetUnconsumedQty(), qtyToConsume);
                    qtyToConsume = a_sd.ScenarioOptions.RoundQty(qtyToConsume);
                    if (a_sd.ScenarioOptions.IsStrictlyGreaterThanZero(qtyToConsume))
                    {
                        shipmentAfter.ConsumeSalesOrder(sod, qtyToConsume);
                        qtyConsumed += qtyToConsume;
                    }
                }

                if (a_sd.ScenarioOptions.IsStrictlyGreaterThanZero(qtyConsumed))
                {
                    totalQtyToConsume -= qtyConsumed;
                }
                else
                {
                    break; // nothing was consumed, don't get into a infinite loop.
                }
            }
        }
    }

    private ForecastShipment FindOpenForecastBefore(DateTime a_dateTime, List<ForecastShipment> a_shipments, double a_forecastConsumptionWindowDays)
    {
        DateTime lowerBound = a_forecastConsumptionWindowDays == 0 ? PTDateTime.MinDateTime : a_dateTime.Subtract(TimeSpan.FromDays(a_forecastConsumptionWindowDays));
        IEnumerable<ForecastShipment> shipmentsBefore = a_shipments.Where(fs => fs.RequiredDate < a_dateTime && fs.RequiredDate > lowerBound && fs.GetUnconsumedQty() > 0);
        if (shipmentsBefore.Count() == 0)
        {
            return null;
        }

        return shipmentsBefore.Last();
    }

    private ForecastShipment FindOpenForecastAfter(DateTime a_dateTime, List<ForecastShipment> a_shipments, double a_forecastConsumptionWindowDays)
    {
        DateTime upperBound = a_forecastConsumptionWindowDays == 0 ? PTDateTime.MaxDateTime : a_dateTime.Add(TimeSpan.FromDays(a_forecastConsumptionWindowDays));
        IEnumerable<ForecastShipment> shipmentsAfter = a_shipments.Where(fs => fs.RequiredDate > a_dateTime && fs.RequiredDate < upperBound && fs.GetUnconsumedQty() > 0);
        if (shipmentsAfter.Count() == 0)
        {
            return null;
        }

        return shipmentsAfter.First();
    }
}