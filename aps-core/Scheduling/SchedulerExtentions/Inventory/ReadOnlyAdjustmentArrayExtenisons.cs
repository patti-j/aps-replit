using PT.Scheduler;

namespace PT.SchedulerExtensions.Inventory;

public static class ReadOnlyAdjustmentArrayExtenisons
{
    public static decimal CalcCurrentQuantity(this ReadOnlyAdjutmentArray a_array)
    {
        decimal currentQty = 0;
        for (int i = 0; i < a_array.Count; i++)
        {
            currentQty += a_array[i].ChangeQty;
        }

        return currentQty;
    }
}