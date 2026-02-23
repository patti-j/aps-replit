using System.Collections;

using PT.SchedulerDefinitions;

namespace PT.Scheduler;

public class OperationCapacityProfile : IPTSerializable
{
    public CapacityUsageProfile SetupProfile = new ();
    public CapacityUsageProfile ProductionProfile = new ();
    public CapacityUsageProfile PostprocessingProfile = new ();
    public CapacityUsageProfile CleanProfile = new ();
    public CapacityUsageProfile StorageProfile = new ();

    internal OperationCapacityProfile() { }

    internal OperationCapacityProfile(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12521)
        {
            SetupProfile = new CapacityUsageProfile(a_reader);
            ProductionProfile = new CapacityUsageProfile(a_reader);
            PostprocessingProfile = new CapacityUsageProfile(a_reader);
            CleanProfile = new CapacityUsageProfile(a_reader);
            StorageProfile = new CapacityUsageProfile(a_reader);
        }
        else if (a_reader.VersionNumber >= 12415)
        {
            SetupProfile = new CapacityUsageProfile(a_reader);
            ProductionProfile = new CapacityUsageProfile(a_reader);
            PostprocessingProfile = new CapacityUsageProfile(a_reader);
            CleanProfile = new CapacityUsageProfile(a_reader);
            StorageProfile = new CapacityUsageProfile(a_reader);
            new CapacityUsageProfile(a_reader);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        SetupProfile.Serialize(a_writer);
        ProductionProfile.Serialize(a_writer);
        PostprocessingProfile.Serialize(a_writer);
        CleanProfile.Serialize(a_writer);
        StorageProfile.Serialize(a_writer);
    }

    public int UniqueId => 1116;

    public bool Empty => SetupProfile.Count == 0 &&
                         ProductionProfile.Count == 0 &&
                         PostprocessingProfile.Count == 0 &&
                         CleanProfile.Count == 0 &&
                         StorageProfile.Count == 0;

    public long GetUsageStart()
    {
        if (SetupProfile.Count > 0)
        {
            return SetupProfile[0].StartTicks;
        }

        if (ProductionProfile.Count > 0)
        {
            return ProductionProfile[0].StartTicks;
        }

        if (PostprocessingProfile.Count > 0)
        {
            return PostprocessingProfile[0].StartTicks;
        }

        //These are set later, so not sure if clean is after or before storage
        long start = long.MaxValue;
        if (CleanProfile.Count > 0)
        {
            start = CleanProfile[0].StartTicks;
        }

        if (StorageProfile.Count > 0)
        {
            start = Math.Min(StorageProfile[0].StartTicks, start);
        }

        return start;
    }

    public long GetUsageEnd()
    {
        long end = long.MinValue;

        if (SetupProfile.Count > 0)
        {
            end = Math.Max(SetupProfile[^1].EndTicks, end);
        }

        if (ProductionProfile.Count > 0)
        {
            end = Math.Max(ProductionProfile[^1].EndTicks, end);
        }

        if (PostprocessingProfile.Count > 0)
        {
            end = Math.Max(PostprocessingProfile[^1].EndTicks, end);
        }

        //These are set later, so not sure if clean is after or before storage
        if (CleanProfile.Count > 0)
        {
            end = Math.Max(CleanProfile[^1].EndTicks, end);
        }

        if (StorageProfile.Count > 0)
        {
            end = Math.Max(StorageProfile[^1].EndTicks, end);
        }

        return end;
    }

    internal void Add(CapacityUsageProfile a_profile, MainResourceDefs.usageEnum a_capacityUsage)
    {
        switch (a_capacityUsage)
        {
            case MainResourceDefs.usageEnum.Setup:
                SetupProfile = a_profile ?? new();
                break;
            case MainResourceDefs.usageEnum.Run:
                ProductionProfile = a_profile ?? new();
                break;
            case MainResourceDefs.usageEnum.PostProcessing:
                PostprocessingProfile = a_profile ?? new();
                break;
            case MainResourceDefs.usageEnum.Storage:
                StorageProfile = a_profile ?? new();
                break;
            case MainResourceDefs.usageEnum.Clean:
                CleanProfile = a_profile ?? new();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(a_capacityUsage), a_capacityUsage, null);
        }
    }

    /// <summary>
    /// Enumerates all capacity usages and specifies type. This is helpful when creating a capacity profile and all capacity usage is needed
    /// </summary>
    /// <returns></returns>
    public IEnumerable<(MainResourceDefs.usageEnum, OperationCapacity)> CapacityEnumerator()
    {
        foreach (OperationCapacity capacity in SetupProfile)
        {
            yield return (MainResourceDefs.usageEnum.Setup, capacity);
        }

        foreach (OperationCapacity capacity in ProductionProfile)
        {
            yield return (MainResourceDefs.usageEnum.Run, capacity);
        }

        foreach (OperationCapacity capacity in PostprocessingProfile)
        {
            yield return (MainResourceDefs.usageEnum.PostProcessing, capacity);
        }

        foreach (OperationCapacity capacity in StorageProfile)
        {
            yield return (MainResourceDefs.usageEnum.Storage, capacity);
        }

        foreach (OperationCapacity capacity in CleanProfile)
        {
            yield return (MainResourceDefs.usageEnum.Clean, capacity);
        }
    }

    /// <summary>
    /// Returns a new OCP reduced by the helper requirement usage
    /// </summary>
    public OperationCapacityProfile ReduceToRequirementUsage(ResourceRequirement a_rr)
    {
        OperationCapacityProfile newOcp = new OperationCapacityProfile();

        if (a_rr.ContainsUsage(MainResourceDefs.usageEnum.Setup))
        {
            newOcp.Add(SetupProfile, MainResourceDefs.usageEnum.Setup);
        }

        if (a_rr.ContainsUsage(MainResourceDefs.usageEnum.Run))
        {
            newOcp.Add(ProductionProfile, MainResourceDefs.usageEnum.Run);
        }

        if (a_rr.ContainsUsage(MainResourceDefs.usageEnum.PostProcessing))
        {
            newOcp.Add(PostprocessingProfile, MainResourceDefs.usageEnum.PostProcessing);
        }

        if (a_rr.ContainsUsage(MainResourceDefs.usageEnum.Clean))
        {
            newOcp.Add(CleanProfile, MainResourceDefs.usageEnum.Clean);
        }

        if (a_rr.ContainsUsage(MainResourceDefs.usageEnum.Storage))
        {
            newOcp.Add(StorageProfile, MainResourceDefs.usageEnum.Storage);
        }

        return newOcp;
    }

    public decimal CalcTotalCost()
    {
        decimal totalCost = 0;
        foreach ((MainResourceDefs.usageEnum usageEnum, OperationCapacity operationCapacity) in CapacityEnumerator())
        {
            totalCost += operationCapacity.Cost;
        }

        return totalCost;
    }
}


public class CapacityUsageProfile : IPTSerializable, IEnumerable<OperationCapacity>
{
    private readonly List<OperationCapacity> m_ocs = new ();

    internal CapacityUsageProfile() { }

    internal CapacityUsageProfile(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12415)
        {
            a_reader.Read(out int listCount);
            for (var i = 0; i < listCount; i++)
            {
                OperationCapacity capacity = new OperationCapacity(a_reader);
                m_ocs.Add(capacity);
            }
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_ocs);
    }

    public int UniqueId => 1117;


    internal void Add(OperationCapacity a_oc)
    {
        m_ocs.Add(a_oc);
    }

    public int Count => m_ocs.Count;

    public OperationCapacity this[int a_index] => m_ocs[a_index];

    /// <summary>
    /// Returns the end date of the last capacity chunk
    /// </summary>
    internal long CapacityEndTicks => m_ocs[^1].EndTicks;

    internal long CapacityFound => m_ocs.Sum(oc => oc.TotalCapacityTicks);

    internal decimal Cost => m_ocs.Sum(c => c.Cost);

    public IEnumerator<OperationCapacity> GetEnumerator()
    {
        foreach (OperationCapacity capacity in m_ocs)
        {
            yield return capacity;
        }
    }

    public override string ToString()
    {
        decimal capacity = 0;
        long time = 0;

        for (int i = 0; i < Count; ++i)
        {
            OperationCapacity oc = this[i];
            capacity += oc.TotalCapacityTicks;
            time += oc.EndTicks - oc.StartTicks;
        }

        long capacityLong = (long)capacity;
        bool capacityTruncated = capacity != capacityLong;
        TimeSpan capacityTS = new (capacityLong);
        TimeSpan timeTS = new (time);

        if (capacityTruncated)
        {
            return string.Format("OperationCapacityProfile: Truncated Capacity={0}; Real Time={1}; Count={2}", capacityTS, timeTS, Count);
        }

        return string.Format("OperationCapacityProfile: Capacity={0}; Real Time={1}; Count={2}", capacityTS, timeTS, Count);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}