namespace PT.Scheduler;

public partial class PurchaseToStockManager
{
    internal void ResetSimulationStateVariables()
    {
        for (int i = 0; i < Count; ++i)
        {
            PurchaseToStock pts = GetByIndex(i);
            pts.ResetSimulationStateVariables();
        }
    }
}