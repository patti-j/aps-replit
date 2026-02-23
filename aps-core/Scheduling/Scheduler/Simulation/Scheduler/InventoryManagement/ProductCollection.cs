using PT.SchedulerDefinitions;

namespace PT.Scheduler;

public partial class ProductsCollection : ICopyTable, IPTSerializable
{
    /// <summary>
    /// For example a ratio of 3/2 and quantity of 10 would result in a quantity of 15.
    /// </summary>
    /// <param name="ratio"></param>
    internal void AdjustOutputQtys(decimal a_ratio, decimal a_newRequiredMOQty, ScenarioOptions a_so, Product a_primaryProduct)
    {
        for (int pI = 0; pI < Count; ++pI)
        {
            Product p = this[pI];
            p.AdjustOutputQty(a_ratio, a_newRequiredMOQty, a_so, a_primaryProduct != null && a_primaryProduct.Item.Id.Value == p.Item.Id.Value);
        }
    }
    
    internal long GetLongestProductShelfLife()
    {
        long longestShelfLife = 0;
        for (int pI = 0; pI < Count; ++pI)
        {
            Product p = this[pI];
            if (p.TotalOutputQty > 0)
            {
                longestShelfLife = Math.Max(p.Item.ShelfLife.Ticks, longestShelfLife);
            }
        }

        return longestShelfLife;
    }

    internal void ResetSimulationStateVariables()
    {
        foreach (Product p in m_productList)
        {
            p.ResetSimulationStateVariables();
        }
    }
}