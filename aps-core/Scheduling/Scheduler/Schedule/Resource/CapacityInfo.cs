using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Calculates bucketed information about the capacity of a Resource.
/// </summary>
public class ResourceCapacityInfo : CapacityInfoBase
{
    private readonly Resource m_resource;

    public ResourceCapacityInfo(DateTime a_start, DateTime a_endInclusive, TimeSpan a_bucketLength, Resource a_resource, GroupChooser a_chooser, MainResourceDefs.usageEnum a_usage)
    {
        m_resource = a_resource;
        Calculate(a_start, a_endInclusive, a_bucketLength, a_chooser,a_usage);
    }

    /// <summary>
    /// Clear the old data and recalculate the CapacityInfo based on the Resource's current data.
    /// </summary>
    private void Calculate(DateTime start, DateTime endInclusive, TimeSpan bucketLength, GroupChooser chooser, MainResourceDefs.usageEnum a_usage)
    {
        m_capacity = m_resource.GetBucketedCapacity(start, endInclusive, bucketLength);
        m_usage = m_resource.GetBucketedUsage(start, endInclusive, bucketLength, chooser, a_usage);
        m_wastedCapacity = m_resource.GetBucketedWastedCapacity(start, endInclusive, bucketLength,chooser);
        m_productionUnits = m_resource.GetBucketedProduction(start, endInclusive, bucketLength, chooser);
        m_usageByNbrPeople = m_resource.GetBucketedPeopleUtilization(start, endInclusive, bucketLength, chooser, a_usage);
    }
}

/// <summary>
/// Calculates bucketed capacity info for multiple resources.
/// </summary>
public class MultiResourceCapacityInfo : CapacityInfoBase
{
    /// <summary>
    /// Create an instance summarizing data for the specified parameters.
    /// </summary>
    public MultiResourceCapacityInfo(ScenarioDetail sd, DateTime start, DateTime endInclusive, TimeSpan bucketLength, Dictionary<BaseId, BaseId> resourcesToInclude, GroupChooser chooser, MainResourceDefs.usageEnum a_usage = MainResourceDefs.usageEnum.Unspecified)
    {
        Calculate(sd, start, endInclusive, bucketLength, resourcesToInclude, chooser,a_usage);
    }

    /// <summary>
    /// Clear the old data and recalculate the CapacityInfo based on the Resource's current data.
    /// </summary>
    private void Calculate(ScenarioDetail sd, DateTime start, DateTime endInclusive, TimeSpan bucketLength, Dictionary<BaseId, BaseId> resourcesToInclude, GroupChooser chooser, MainResourceDefs.usageEnum a_usage)
    {
        m_capacity = new TimeBucketList(start, endInclusive, bucketLength);
        m_usage = new TimeBucketList(start, endInclusive, bucketLength);
        m_wastedCapacity = new TimeBucketList(start, endInclusive, bucketLength);
        m_usageByNbrPeople =  new TimeBucketList(start, endInclusive, bucketLength);

        for (int plantI = 0; plantI < sd.PlantManager.Count; plantI++)
        {
            Plant plant = sd.PlantManager[plantI];
            for (int deptI = 0; deptI < plant.DepartmentCount; deptI++)
            {
                Department dept = plant.Departments[deptI];
                for (int resI = 0; resI < dept.ResourceCount; resI++)
                {
                    Resource resource = dept.Resources[resI];
                    if (resourcesToInclude.ContainsKey(resource.Id))
                    {
                        ResourceCapacityInfo resCapInfo = resource.GetCapacityInfo(start, endInclusive, bucketLength, chooser, a_usage);
                        m_capacity.Absorb(resCapInfo.Capacity);
                        m_usage.Absorb(resCapInfo.Usage);
                        m_usageByNbrPeople.Absorb(resCapInfo.UsageByNbrPeople);
                        m_wastedCapacity.Absorb(resCapInfo.GetWastedCapacity());
                    }
                }
            }
        }
    }
}

/// <summary>
/// Stores bucketed capacity info about one or more Resources.
/// </summary>
public abstract class CapacityInfoBase
{
    protected TimeBucketList m_capacity;

    /// <summary>
    /// This is the raw capacity taking only the capacity intervals into account, not any usage of the capacity.
    /// </summary>
    public TimeBucketList Capacity => m_capacity;

    protected TimeBucketList m_usage;
    protected TimeBucketList m_usageByNbrPeople;

    protected TimeBucketList m_wastedCapacity;

    /// <summary>
    /// The scheduled usage of the resource(s) based on scheduled blocks.
    /// </summary>
    public TimeBucketList Usage => m_usage;
    public TimeBucketList UsageByNbrPeople => m_usageByNbrPeople;

    protected decimal[] m_productionUnits;

    /// <summary>
    /// The scheduled production quantities of the resource(s) based on scheduled blocks.
    /// Each value in the TimeBucketList is a quantity instead of an hour.
    /// </summary>
    public decimal[] ProductionUnits => m_productionUnits;

    /// <summary>
    /// Sets the AvailableCapacity by subtracting the usage and wasted capacity from the capacity.
    /// </summary>
    public TimeBucketList GetAvailableCapacity()
    {
        if (m_capacity.Count != m_usage.Count)
        {
            throw new PTValidationException("2274");
        }

        TimeBucketList availableCapacity = new (m_capacity.Start, m_capacity.End, m_capacity.BucketLength);
        for (int i = 0; i < availableCapacity.Count; i++)
        {
            decimal capacityHrs = (decimal)m_capacity[i].TotalHours;
            decimal availableCapacityHrs = capacityHrs - (decimal)m_usage[i].TotalHours - (decimal)m_wastedCapacity[i].TotalHours;
            availableCapacity[i] = TimeSpan.FromHours(Convert.ToDouble(availableCapacityHrs));
        }
        
        return availableCapacity;
    }

    /// <summary>
    /// Gets the AvailableCapacityPercent by subtracting the usage and wasted capacity from the capacity and expressing as a percent of total capacity.
    /// Returns a TimeBucketList of TimeSpans but the hours really represent percent.
    /// </summary>
    public TimeBucketList GetAvailableCapacityPercents()
    {
        if (m_capacity.Count != m_usage.Count)
        {
            throw new PTValidationException("2274");
        }

        TimeBucketList availableCapacityPercent = new (m_capacity.Start, m_capacity.End, m_capacity.BucketLength);
        for (int i = 0; i < availableCapacityPercent.Count; i++)
        {
            decimal capacityHrs = (decimal)m_capacity[i].TotalHours;
            decimal availableCapacityHrs = capacityHrs - (decimal)m_usage[i].TotalHours - (decimal)m_wastedCapacity[i].TotalHours;
            if (capacityHrs != 0)
            {
                availableCapacityPercent[i] = TimeSpan.FromHours(Convert.ToDouble(availableCapacityHrs / capacityHrs * 100));
            }
            else
            {
                availableCapacityPercent[i] = TimeSpan.FromHours(0);
            }
        }

        return availableCapacityPercent;
    }

    /// <summary>
    /// Returns wasted or un-utilized capacity
    /// </summary>
    public TimeBucketList GetWastedCapacity()
    {
        return m_wastedCapacity;
    }
    /// <summary>
    /// Gets the Scheduled Usage as a percentage of total capacity.
    /// Returns a TimeBucketList of TimeSpans but the hours really represent percent.
    /// </summary>
    public TimeBucketList GetUsagePercents()
    {
        if (m_capacity.Count != m_usage.Count)
        {
            throw new PTValidationException("2274");
        }

        TimeBucketList usagePercent = new (m_capacity.Start, m_capacity.End, m_capacity.BucketLength);
        for (int i = 0; i < usagePercent.Count; i++)
        {
            decimal capacityHrs = (decimal)m_capacity[i].TotalHours;
            decimal usageHrs = (decimal)m_usage[i].TotalHours;
            if (capacityHrs != 0)
            {
                usagePercent[i] = TimeSpan.FromHours(Convert.ToDouble(usageHrs / capacityHrs * 100));
            }
            else
            {
                usagePercent[i] = TimeSpan.FromHours(0);
            }
        }

        return usagePercent;
    }
    /// <summary>
    /// Gets the Scheduled Usage as a percentage of total capacity.
    /// Returns a TimeBucketList of TimeSpans but the hours really represent percent.
    /// </summary>
    public TimeBucketList GetUsageByNbrPeoplePercents()
    {
        if (m_capacity.Count != m_usage.Count)
        {
            throw new PTValidationException("2274");
        }

        TimeBucketList usagePercent = new (m_capacity.Start, m_capacity.End, m_capacity.BucketLength);
        for (int i = 0; i < usagePercent.Count; i++)
        {
            decimal capacityHrs = (decimal)m_capacity[i].TotalHours;
            decimal usageHrs = (decimal)m_usageByNbrPeople[i].TotalHours;
            if (capacityHrs != 0)
            {
                usagePercent[i] = TimeSpan.FromHours(Convert.ToDouble(usageHrs / capacityHrs * 100));
            }
            else
            {
                usagePercent[i] = TimeSpan.FromHours(0);
            }
        }

        return usagePercent;
    }

    /// <summary>
    /// Gets the Scheduled Usage Hours by dividing the hours by the hours per bucket.
    /// Returns a TimeBucketList of TimeSpans but the hours really represent a count.
    /// </summary>
    public TimeBucketList GetUsageCounts(decimal bucketLengthHrs)
    {
        TimeBucketList usageCount = new (m_capacity.Start, m_capacity.End, m_capacity.BucketLength);
        for (int i = 0; i < usageCount.Count; i++)
        {
            decimal usageHrs = (decimal)m_usage[i].TotalHours;
            if (bucketLengthHrs != 0)
            {
                usageCount[i] = TimeSpan.FromHours(Convert.ToDouble(usageHrs / bucketLengthHrs));
            }
            else
            {
                usageCount[i] = TimeSpan.FromHours(0);
            }
        }

        return usageCount;
    }

    /// <summary>
    /// Gets the Scheduled Usage Concurrent Blocks (hours)
    /// Returns a TimeBucketList of TimeSpans but the hours really represent a count.
    /// </summary>
    public TimeBucketList GetUsageConcurrentCounts()
    {
        TimeBucketList usageCount = new (m_capacity.Start, m_capacity.End, m_capacity.BucketLength);
        for (int i = 0; i < usageCount.Count; i++)
        {
            decimal usageHrs = (decimal)m_usage[i].TotalHours;
            usageCount[i] = TimeSpan.FromHours(Convert.ToDouble(usageHrs));
        }

        return usageCount;
    }

    /// <summary>
    /// Gets the Scheduled Capacity Hours divided by dividing the hours by the hours per bucket.
    /// Returns a TimeBucketList of TimeSpans but the hours really represent a count.
    /// </summary>
    public TimeBucketList GetCapacityCounts(decimal bucketLengthHrs)
    {
        TimeBucketList capacityCount = new (m_capacity.Start, m_capacity.End, m_capacity.BucketLength);
        for (int i = 0; i < capacityCount.Count; i++)
        {
            decimal capacityHrs = (decimal)m_capacity[i].TotalHours;
            if (bucketLengthHrs != 0)
            {
                capacityCount[i] = TimeSpan.FromHours(Convert.ToDouble(capacityHrs / bucketLengthHrs));
            }
            else
            {
                capacityCount[i] = TimeSpan.FromHours(0);
            }
        }

        return capacityCount;
    }

    /// <summary>
    /// Gets the AvailableCapacity Count by subtracting the usage from the capacity and dividing by the hours per bucket.
    /// Returns a TimeBucketList of TimeSpans but the hours really represent counts.
    /// </summary>
    public TimeBucketList GetAvailableCapacityCounts(decimal bucketLengthHrs)
    {
        if (m_capacity.Count != m_usage.Count)
        {
            throw new PTValidationException("2274");
        }

        TimeBucketList availableCapacityPercent = new (m_capacity.Start, m_capacity.End, m_capacity.BucketLength);
        for (int i = 0; i < availableCapacityPercent.Count; i++)
        {
            decimal capacityHrs = (decimal)m_capacity[i].TotalHours;
            decimal availableCapacityHrs = capacityHrs - (decimal)m_usage[i].TotalHours;
            if (capacityHrs != 0)
            {
                TimeSpan ts = TimeSpan.FromHours(Convert.ToDouble(availableCapacityHrs / bucketLengthHrs));
                availableCapacityPercent[i] = ts;
            }
            else
            {
                availableCapacityPercent[i] = TimeSpan.FromHours(0);
            }
        }

        return availableCapacityPercent;
    }

    /// <summary>
    /// Used to create usage TimeBuckets based on a specific CapacityType.
    /// </summary>
    public class GroupChooser
    {
        public GroupChooser(valueTypes a_valueType, groupTypes a_groupType, bool a_includeFirm, bool a_includePlanned, bool a_includeEstimate, bool a_includeReleased)
        {
            ValueType = a_valueType;
            GroupType = a_groupType;
            includeReleased = a_includeReleased;
            includeFirm = a_includeFirm;
            includePlanned = a_includePlanned;
            includeEstimate = a_includeEstimate;

            GroupValue = "";
        }

        public bool ChooseBlock(ResourceBlock block)
        {
            bool include = false;
            //First check Commitment
            if (IncludeBasedOnCommitment(block.Activity.Operation.ManufacturingOrder))
            {
                switch (GroupType)
                {
                    case groupTypes.All:
                        include = true;
                        break;
                    case groupTypes.Customer:
                        //This will still work in cases where there is only one customer. Grouping will break in multi-customer cases.
                        include = block.Activity.Operation.ManufacturingOrder.Job.Customers.GetCustomerExternalIdsList() == GroupValue;
                        break;
                    case groupTypes.Family:
                        include = block.Activity.Operation.ManufacturingOrder.Family == GroupValue;
                        break;
                    case groupTypes.Priority:
                        include = block.Activity.Operation.ManufacturingOrder.Job.Priority.ToString() == GroupValue;
                        break;
                    case groupTypes.Commitment:
                        include = block.Activity.Operation.ManufacturingOrder.Job.Commitment.ToString() == GroupValue;
                        break;
                    case groupTypes.Product:
                        include = block.Activity.Operation.ManufacturingOrder.ProductName == GroupValue;
                        break;
                    case groupTypes.Classification:
                        include = block.Activity.Operation.ManufacturingOrder.Job.Classification.ToString() == GroupValue;
                        break;
                    case groupTypes.OrderNumber:
                        include = block.Activity.Operation.ManufacturingOrder.Job.OrderNumber == GroupValue;
                        break;
                    case groupTypes.Type:
                        include = block.Activity.Operation.ManufacturingOrder.Job.Type == GroupValue;
                        break;
                    case groupTypes.BottleneckOp:
                        if (block.Activity.Operation.Bottleneck)
                        {
                            include = BOTTLENECK_STRING == GroupValue;
                        }
                        else
                        {
                            include = NON_BOTTLENECK_STRING == GroupValue;
                        }

                        break;
                    case groupTypes.PrimaryCapability1:
                        include = block.SatisfiedRequirement.CapabilityManager[0].Name == GroupValue;
                        break;
                    case groupTypes.PrimaryCapability2:
                        if (block.SatisfiedRequirement.CapabilityManager.Count > 1)
                        {
                            include = block.SatisfiedRequirement.CapabilityManager[1].Name == GroupValue;
                        }
                        else
                        {
                            include = "none" == GroupValue;
                        }

                        break;
                }
            }

            return include;
        }

        private bool IncludeBasedOnCommitment(ManufacturingOrder aMO)
        {
            return (includeReleased && aMO.Job.Commitment == JobDefs.commitmentTypes.Released) || (includeFirm && aMO.Job.Commitment == JobDefs.commitmentTypes.Firm) || (includePlanned && aMO.Job.Commitment == JobDefs.commitmentTypes.Planned) || (includeEstimate && aMO.Job.Commitment == JobDefs.commitmentTypes.Estimate);
        }

        private const string BOTTLENECK_STRING = "Bottleneck Ops";
        private const string NON_BOTTLENECK_STRING = "Non-Bottleneck Ops";

        /// <summary>
        /// Returns the GroupValue for the block based upon the GroupType.
        /// </summary>
        public string GetGroupValue(ResourceBlock block)
        {
            switch (GroupType)
            {
                case groupTypes.All:
                    return ""; //not used
                case groupTypes.Customer:
                    //TODO: is this correct?
                    return block.Activity.Operation.ManufacturingOrder.Job.Customers.GetCustomerExternalIdsList();
                case groupTypes.Family:
                    return block.Activity.Operation.ManufacturingOrder.Family;
                case groupTypes.Priority:
                    return block.Activity.Operation.ManufacturingOrder.Job.Priority.ToString();
                case groupTypes.Commitment:
                    return block.Activity.Operation.ManufacturingOrder.Job.Commitment.ToString();
                case groupTypes.Product:
                    return block.Activity.Operation.ManufacturingOrder.ProductName;
                case groupTypes.Classification:
                    return block.Activity.Operation.ManufacturingOrder.Job.Classification.ToString();
                case groupTypes.OrderNumber:
                    return block.Activity.Operation.ManufacturingOrder.Job.OrderNumber;
                case groupTypes.Type:
                    return block.Activity.Operation.ManufacturingOrder.Job.Type;
                case groupTypes.BottleneckOp:
                    if (block.Activity.Operation.Bottleneck)
                    {
                        return BOTTLENECK_STRING;
                    }

                    return NON_BOTTLENECK_STRING;
                case groupTypes.PrimaryCapability1:
                    return block.SatisfiedRequirement.CapabilityManager[0].Name;
                case groupTypes.PrimaryCapability2:
                {
                    if (block.SatisfiedRequirement.CapabilityManager.Count > 1)
                    {
                        return block.SatisfiedRequirement.CapabilityManager[1].Name;
                    }

                    return "none";
                }
                default:
                    return "";
            }
        }

        public enum groupTypes
        {
            All,
            Customer,
            Priority,
            Family,
            Commitment,
            Product,
            Classification,
            OrderNumber,
            Type,
            BottleneckOp,
            PrimaryCapability1,
            PrimaryCapability2
        }

        public groupTypes GroupType;
        public string GroupValue;
        public bool includeReleased;
        public bool includeFirm;
        public bool includePlanned;
        public bool includeEstimate;

        public enum valueTypes { UsageHours, UtilizedShiftCosts, ConcurrentCount }

        public valueTypes ValueType;
    }
}