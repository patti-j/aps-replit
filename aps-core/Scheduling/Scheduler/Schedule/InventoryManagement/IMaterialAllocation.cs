using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Scheduler.Schedule.InventoryManagement;

public interface IMaterialAllocation
{
    ItemDefs.MaterialAllocation MaterialAllocation { get; }

    decimal MinSourceQty { get; }

    decimal MaxSourceQty { get; }

    ItemDefs.MaterialSourcing MaterialSourcing { get; }

    BaseId DemandId { get; }
}