using System.Text;

using PT.APSCommon;
using PT.Common.Debugging;
using PT.Common.Exceptions;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Events;
using PT.SchedulerDefinitions;

using static PT.SchedulerDefinitions.InternalActivityDefs;
using static PT.SchedulerDefinitions.MainResourceDefs;

namespace PT.Scheduler;

/// <summary>
/// Abstract class the defines attributes that are common across all resources that are internal to the organization.
/// </summary>
public abstract partial class InternalResource
{
    #region Simulation
    internal void ClearAvailabilityVars()
    {
        if (AvailableInSimulation != null)
        {
            AvailableInSimulation.Clear();
            AvailableInSimulation = null;
        }

        if (AvailableNode != null)
        {
            AvailableNode.Clear();
            AvailableNode = null;
        }

        if (AvailablePrimaryNode != null)
        {
            AvailablePrimaryNode.Clear();
            AvailablePrimaryNode = null;
        }
    }

    internal void SetAvailabilityVars(CalendarResourceAvailableEventList.Node a_availableNode, CalendarResourceAvailableEventList.Node a_availablePrimaryNode)
    {
        AvailableInSimulation = a_availableNode.Data;
        AvailableNode = a_availableNode;
        AvailablePrimaryNode = a_availablePrimaryNode;
    }

    /// <summary>
    /// Used as a temporary variable during simulation.
    /// It's the stored position of the resource in the resource available list.
    /// </summary>
    internal CalendarResourceAvailableEventList.Node AvailableNode { get; set; }

    /// <summary>
    /// This value will be null if the resource isn't used as a primary resource.
    /// Used as a temporary variable during simulation.
    /// It's the stored position of the resource in the resource available list.
    /// </summary>
    internal CalendarResourceAvailableEventList.Node AvailablePrimaryNode { get; set; }

    private ResourceAvailableEvent m_availableInSimulation;

    /// <summary>
    /// Whether this resource is available within the simulation. That is whether the resource
    /// is on the set of resources that are currently available at the current simulation clock.
    /// This variable should not be serialized. If it's used to satisfy a non-primary resouce requirement, this value
    /// will be set to null to indicate it's no longer available.
    /// </summary>
    internal ResourceAvailableEvent AvailableInSimulation
    {
        get => m_availableInSimulation;
        private set => m_availableInSimulation = value;
    }

    private LinkedListNode<ResourceCapacityInterval> m_lastResultantCapacityNode;

    internal LinkedListNode<ResourceCapacityInterval> LastResultantCapacityNode
    {
        get => m_lastResultantCapacityNode;
        set => m_lastResultantCapacityNode = value;
    }

    private ResourceCapacityIntervalList m_resultantCapacity = new ();

    protected ResourceCapacityIntervalList ResultantCapacity => m_resultantCapacity;

    protected ResourceUnavailableEvent m_lastScheduledResourceUnavailableEvent;

    /// <summary>
    /// This is only used for simulation purposes. It's a reference to the last ResourceUnavailableEvent scheduled for the Resource. Sometimes these need to be cancelled. For instance when an activity is
    /// scheduled or a multi tasking Resource
    /// runs out of attention, the time of the next ResourceUnavailableEvent needs to be recalculated.
    /// </summary>
    internal ResourceUnavailableEvent LastScheduledResourceUnavailableEvent
    {
        get => m_lastScheduledResourceUnavailableEvent;
        set => m_lastScheduledResourceUnavailableEvent = value;
    }

    private ResourceAvailableEvent m_lastScheduledResourceAvailableEvent;

    /// <summary>
    /// This is only used for simulation purposes. It's a reference to the last ResourceAvailableEvent scheduled for the resource. sometimes these need to be cancelled.
    /// </summary>
    internal ResourceAvailableEvent LastScheduledResourceAvailableEvent
    {
        get => m_lastScheduledResourceAvailableEvent;
        set => m_lastScheduledResourceAvailableEvent = value;
    }

    internal LinkedListNode<ResourceCapacityInterval> FindContainingCapacityIntervalNode(long a_dt, LinkedListNode<ResourceCapacityInterval> a_searchStartNode)
    {
        return m_resultantCapacity.Find(a_dt, a_searchStartNode);
    }

    private readonly long MaxDate = DateTime.MaxValue.Ticks;
    private readonly long MinDate = PTDateTime.MinDateTime.Ticks;

    internal void CopyToResultantCapacity(long a_clock)
    {
        if (ResultantCapacity == null)
        {
            m_resultantCapacity = new ResourceCapacityIntervalList();
        }
        else
        {
            ResultantCapacity.Clear();
        }

        for (int i = 0; i < capacityProfile.ProfileIntervals.Count; ++i)
        {
            ResourceCapacityInterval origRCI = capacityProfile.ProfileIntervals[i];
            if (origRCI.EndDate > a_clock)
            {
                ResourceCapacityInterval rci = new (capacityProfile.ProfileIntervals[i]);
                #if DEBUG
                if (rci.StartDate < MinDate ||
                    rci.EndDate > MaxDate ||
                    rci.StartDate >= rci.EndDate ||
                    rci.NbrOfPeople == 0)
                {
                    throw new PTException(string.Format("Bad date interval. Start={0}, End={1}, CapacityMultipler={2}.", rci.StartDate, rci.EndDate, rci.NbrOfPeople));
                }
                #endif
                ResultantCapacity.AddLast(rci);
            }
        }
    }

    /// <summary>
    /// Side affects: Sets LastResultantCapacityNode to the return value.
    /// Finds the first online capacity interval that is equal to or greater the date passed in.
    /// </summary>
    /// <param name="earliestStartDate">The first online period on or after this date.</param>
    /// <param name="nextOnlineTime">The later of the the variable earliestStartDate and the interval found. </param>
    /// <param name="o_rciNode">The Resource Capacity Interval node the next online time is part of. </param>
    internal void FindNextOnlineTime(long a_earliestStartDate, LinkedListNode<ResourceCapacityInterval> a_lastUsedCapcityNode, out long o_nextOnlineTime, out LinkedListNode<ResourceCapacityInterval> o_rciNode)
    {
        LinkedListNode<ResourceCapacityInterval> lastUsedCapacityNode;
        if (a_lastUsedCapcityNode != null)
        {
            lastUsedCapacityNode = a_lastUsedCapcityNode;
        }
        else
        {
            if (LastResultantCapacityNode != null)
            {
                if (a_earliestStartDate < LastResultantCapacityNode.Value.StartDate)
                {
                    LinkedListNode<ResourceCapacityInterval> currentNode = LastResultantCapacityNode;
                    do
                    {
                        currentNode = currentNode.Previous;
                    } while (currentNode != null && currentNode.Value.StartDate > a_earliestStartDate);

                    lastUsedCapacityNode = currentNode;
                }
                else
                {
                    lastUsedCapacityNode = LastResultantCapacityNode;
                }
            }
            else
            {
                lastUsedCapacityNode = LastResultantCapacityNode;
            }
        }

        LinkedListNode<ResourceCapacityInterval> node = ResultantCapacity.FindFirstOnline(a_earliestStartDate, lastUsedCapacityNode);

        o_nextOnlineTime = node.Value.StartDate;

        if (o_nextOnlineTime < a_earliestStartDate)
        {
            o_nextOnlineTime = a_earliestStartDate;
        }

        LastResultantCapacityNode = node;
        o_rciNode = LastResultantCapacityNode;
    }

    internal LinkedListNode<ResourceCapacityInterval> FindNextCleanoutInterval(long a_earliestStartDate)
    {
        LinkedListNode<ResourceCapacityInterval> node = ResultantCapacity.First;

        while (node != null)
        {
            if (node.Value.ClearChangeovers && (node.Value.StartDate >= a_earliestStartDate || node.Value.Contains(a_earliestStartDate)))
            {
                return node;
            }

            node = node.Next;
        }

        return null;
    }

    /// <summary>
    /// Starting from the beggining of capacity, find the first online interval on or after the point.
    /// </summary>
    /// <param name="startTicks"></param>
    /// <returns></returns>
    public LinkedListNode<ResourceCapacityInterval> FindRCIForward(long a_startTicks)
    {
        return ResultantCapacity.Find(a_startTicks, null);
    }

    /// <summary>
    /// Returns the first online capacity interval on or after the specified ticks or null if one isn't found.
    /// </summary>
    /// <param name="a_startTicks"></param>
    /// <returns></returns>
    internal LinkedListNode<ResourceCapacityInterval> FindFirstOnlineRCIForward(long a_startTicks)
    {
        LinkedListNode<ResourceCapacityInterval> cur = FindRCIForward(a_startTicks);
        while (cur != null && !cur.Value.Active)
        {
            cur = cur.Next;
        }

        return cur;
    }

    /// <summary>
    /// Calculate the end date of an activity given the start date and capacity profile
    /// </summary>
    /// <param name="a_startDate"></param>
    /// <param name="a_capacity"></param>
    /// <param name="a_ia"></param>
    /// <returns></returns>
    public DateTime CalculateEndDateExternally(long a_startDate, RequiredCapacity a_capacity, InternalActivity a_ia)
    {
        RequiredCapacityPlus capacityPlus = a_capacity as RequiredCapacityPlus;
        if (capacityPlus == null)
        {
            capacityPlus = new RequiredCapacityPlus(a_capacity.CleanBeforeSpan, a_capacity.SetupSpan, a_capacity.ProcessingSpan, a_capacity.PostProcessingSpan, a_capacity.StorageSpan, a_capacity.CleanAfterSpan, 0, 0);
        }

        FindCapacityResult result = FindFullCapacity(a_startDate, capacityPlus, a_ia.Operation.CanPause, null, a_ia, false);
        if (result.ResultStatus == SchedulableSuccessFailureEnum.Success)
        {
            return new DateTime(result.FinishDate);
        }

        return PTDateTime.InvalidDateTime;
    }

    /// <summary>
    /// Determine when the capacity can be satisfied based on the required capacity
    /// /// </summary>
    /// <param name="a_startDate"></param>
    /// <param name="a_capacity"></param>
    /// <param name="a_canPause">Whether the capacity can pause for inactive intervals.
    /// If false, any gap in online intervals will cause the capacity to be unusable
    /// It's possible this is not equal to the operation value, for example cleanout that must be added regardless of gaps</param>
    /// <param name="a_startingInterval"></param>
    /// <param name="a_ia"></param>
    /// <param name="a_ignoreRciProfile"></param>
    /// <returns></returns>
    internal FindCapacityResult FindFullCapacity(long a_startDate, RequiredCapacity a_capacity, bool a_canPause, LinkedListNode<ResourceCapacityInterval> a_startingInterval, InternalActivity a_ia, bool a_ignoreRciProfile)
    {
        //This helper is used for MaxDelay which only checks Primary requirements
        string capacityCode = a_ia.Operation.ResourceRequirements.PrimaryResourceRequirement.CapacityCode;
        ActivityResourceBufferInfo bufferInfo = a_ia.GetBufferResourceInfo(Id);

        FindCapacityResult result = FindFullCapacityHelper(a_startDate, a_capacity, a_canPause, a_startingInterval, a_ia, false, a_ignoreRciProfile, capacityCode, out bool willSpanLateOnlyRci);
        if (result.ResultStatus == SchedulableSuccessFailureEnum.Success)
        {
            //Now that we have an end date, we need to verify that the capacity found is valid if the activity will be late and the capacity found spans a UseOnlyWhenLate interval
            if (willSpanLateOnlyRci && result.CapacityUsageProfile[^1].EndTicks > bufferInfo.BufferEndDate)
            {
                result = FindFullCapacityHelper(a_startDate, a_capacity, a_canPause, a_startingInterval, a_ia, true, a_ignoreRciProfile, capacityCode, out willSpanLateOnlyRci);
            }
        }

        return result;
    }

    /// <summary>
    /// Determine when a generic duration of capacity can be defined. This will not account for interval profiles or number of people
    /// /// </summary>
    /// <param name="a_startDate"></param>
    /// <param name="a_capacity"></param>
    /// <param name="a_canPause">Whether the capacity can pause for inactive intervals.
    /// If false, any gap in online intervals will cause the capacity to be unusable
    /// It's possible this is not equal to the operation value, for example cleanout that must be added regardless of gaps</param>
    /// <param name="a_startingInterval"></param>
    /// <param name="a_ia"></param>
    /// <param name="a_capacityForProcessing">Whether the capacity will be affected by interval type, or whether it's strictly active capacity time</param>
    /// <returns></returns>
    internal FindCapacityResult FindCapacity(long a_startDate, long a_capacity, bool a_canPause, LinkedListNode<ResourceCapacityInterval> a_startingInterval, InternalActivity a_ia)
    {
        return FindCapacity(a_startDate, a_capacity, a_canPause, a_startingInterval, usageEnum.Unspecified, false, false, string.Empty, false, a_ia.PeopleUsage, a_ia.NbrOfPeople, out bool _);
    }

    internal FindCapacityResult FindFullCapacityHelper(long a_startDate, RequiredCapacity a_capacity, bool a_canPause, LinkedListNode<ResourceCapacityInterval> a_startingInterval, InternalActivity a_ia, bool a_isLate, bool a_ignoreRciProfile, string a_capacityCode, out bool o_willSpanLateOnlyRci)
    {
        o_willSpanLateOnlyRci = false;

        FindCapacityResult result = FindCapacity(a_startDate, a_capacity.CleanBeforeSpan.TimeSpanTicks, a_canPause, a_startingInterval, usageEnum.Clean, false, a_isLate, a_capacityCode, a_ignoreRciProfile, a_ia.PeopleUsage, a_ia.NbrOfPeople, out o_willSpanLateOnlyRci);
        if (result.ResultStatus != SchedulableSuccessFailureEnum.Success)
        {
            return result;
        }

        result = FindCapacity(result.FinishDate, a_capacity.SetupSpan.TimeSpanTicks, a_canPause, a_startingInterval, usageEnum.Setup, true, a_isLate, a_capacityCode, a_ignoreRciProfile, a_ia.PeopleUsage, a_ia.NbrOfPeople, out o_willSpanLateOnlyRci);

        if (result.ResultStatus != SchedulableSuccessFailureEnum.Success)
        {
            return result;
        }

        result = FindCapacity(result.FinishDate, a_capacity.ProcessingSpan.TimeSpanTicks, a_canPause, a_startingInterval, usageEnum.Run, a_capacity.SetupSpan.TimeSpanTicks == 0, a_isLate, a_capacityCode, a_ignoreRciProfile, a_ia.PeopleUsage, a_ia.NbrOfPeople, out o_willSpanLateOnlyRci);

        if (result.ResultStatus != SchedulableSuccessFailureEnum.Success)
        {
            return result;
        }

        result = FindCapacity(result.FinishDate, a_capacity.PostProcessingSpan.TimeSpanTicks, a_canPause, a_startingInterval, usageEnum.PostProcessing, a_capacity.ProcessingSpan.TimeSpanTicks == 0, a_isLate, a_capacityCode, a_ignoreRciProfile, a_ia.PeopleUsage, a_ia.NbrOfPeople, out o_willSpanLateOnlyRci);

        return result;
    }

    /// <summary>
    /// Determine when the capacity can be satisfied.
    /// /// </summary>
    /// <param name="a_startDate"></param>
    /// <param name="a_capacity"></param>
    /// <param name="a_canPause">Whether the capacity can pause for inactive intervals.
    /// If false, any gap in online intervals will cause the capacity to be unusable
    /// It's possible this is not equal to the operation value, for example cleanout that must be added regardless of gaps</param>
    /// <param name="a_startingInterval"></param>
    /// <param name="a_productionUsage"></param>
    /// <param name="a_startOfActivity">Whether this is the start of the activity and can be constrained by capacity CanStartActivity</param>
    /// <param name="a_activityIsLate">Whether this activity would schedule past its JIT start date, meaning it would be late when using this interval</param>
    /// <param name="a_capacityCode">The capacity code this RR is constrained by. If a capacity interval has a group code set, it will need to match this RR in order to be used.
    /// If either are blank, no constraint is enforced</param>
    /// <param name="a_ignoreRciProfile"></param>
    /// <param name="a_peopleUsage"></param>
    /// <param name="a_actNumberOfPeople"></param>
    /// <param name="o_willSpanLateOnlyRci"></param>
    /// <returns></returns>
    internal FindCapacityResult FindCapacity(long a_startDate,
                                             long a_capacity,
                                             bool a_canPause,
                                             LinkedListNode<ResourceCapacityInterval> a_startingInterval,
                                             usageEnum a_productionUsage,
                                             bool a_startOfActivity,
                                             bool a_activityIsLate,
                                             string a_capacityCode,
                                             bool a_ignoreRciProfile,
                                             peopleUsages a_peopleUsage,
                                             decimal a_actNumberOfPeople, 
                                             out bool o_willSpanLateOnlyRci)
    {
        o_willSpanLateOnlyRci = false;

        if (a_capacity < 0)
        {
            throw new PTException("The required capacity<0.");
        }

        CapacityUsageProfile capacityUsageProfile = new ();

        if (a_capacity == 0) //Capacity not required
        {
            return new FindCapacityResult(a_startDate, capacityUsageProfile);
        }

        LinkedListNode<ResourceCapacityInterval> current = a_startingInterval;
        if (current == null)
        {
            //The existing node is not known. The scheduler tracks this for performance, but external calls may not have the interval node
            //First check the current node, most likely the start date is at the sim clock
            current = ResultantCapacity.Find(a_startDate, LastResultantCapacityNode);
            if (current == null)
            {
                //We couldn't find the node in the future, start from the beginning
                current = ResultantCapacity.Find(a_startDate, null);
                if (current == null)
                {
                    //Something has gone wrong, or this resource doesn't have intervals at the start date
                    return new FindCapacityResult(SchedulableSuccessFailureEnum.LackCapacity, 0);
                }
            }
        }

        long capacityNeeded = a_capacity;

        //Verify usage constraint
        if (a_peopleUsage == peopleUsages.UseMultipleOfSpecifiedNbr && current.Value.CalculateCapacityMultiple(a_actNumberOfPeople) == 0)
        {
            //Don't start a multiple activity on an interval that doesn't have that multiple
            return new FindCapacityResult(SchedulableSuccessFailureEnum.LackCapacity, current.Next != null ? current.Next.Value.StartDate : 0);
        }

        bool firstInterval = true;
        while (current != null)
        {
            ResourceCapacityInterval rci = current.Value;

            //TODO: we can short-circuit here if the start date of this RCI is already past the buffer end date. We would return "Success" so that we can re-run this code with the late flag set to true;

            //Check whether we can start on this interval
            if (firstInterval && (!current.Value.CanStartActivity || !current.Value.Active) && a_startOfActivity)
            {
                return new FindCapacityResult(SchedulableSuccessFailureEnum.HitCleanoutInterval, current.Value.EndDate);
            }

            //This section calculates whether the interval can schedule this activity based on whether it's late
            bool scheduleDueToLateness = true;
            if (rci.Active && rci.UseOnlyWhenLate)
            {
                scheduleDueToLateness = a_activityIsLate;
            }

            bool canScheduleResourceRequirement = true;
            //TODO: Is comparing first, and then checking for empty if equal faster here?
            if (rci.Active && !string.IsNullOrWhiteSpace(a_capacityCode) && !string.IsNullOrWhiteSpace(rci.CapacityCode))
            {
                if (rci.CapacityCode != a_capacityCode)
                {
                    canScheduleResourceRequirement = false;
                }
            }

            bool canScheduleProductionUsage = rci.CanScheduleProductionUsage(a_productionUsage);

            //Whether this capacity profile is eligible to schedule the activity capacity
            bool eligibleCapacityProfile = scheduleDueToLateness && canScheduleResourceRequirement && canScheduleProductionUsage;

            if (firstInterval && !eligibleCapacityProfile
                              && a_productionUsage != usageEnum.Clean) //Cleans are allowed to start since there is no other option, the block is already scheduled
            {
                //No reason to span this interval when the capacity isn't used.
                return new FindCapacityResult(SchedulableSuccessFailureEnum.HitCleanoutInterval, current.Value.EndDate);
            }


            if (rci.Active)
            {
                if (eligibleCapacityProfile || a_ignoreRciProfile)
                {
                    long capacityAvailable = GetRciAvailableCapacity(a_startDate, a_peopleUsage, a_actNumberOfPeople, a_productionUsage, rci, capacityUsageProfile, capacityNeeded);
                    if (rci.PreventOperationsFromSpanning && capacityAvailable < capacityNeeded)
                    {
                        //We can't span this interval
                        if (GetInvalidCapacityResult(a_canPause, a_productionUsage, rci, capacityUsageProfile, out FindCapacityResult invalidCapacityResult))
                        {
                            return invalidCapacityResult;
                        }
                    }

                    capacityNeeded -= capacityAvailable;
                }
                else
                {
                    if (!scheduleDueToLateness && canScheduleResourceRequirement && canScheduleProductionUsage)
                    {
                        //this only use when late rci is being used, track it
                        o_willSpanLateOnlyRci = true;
                    }

                    if (GetInvalidCapacityResult(a_canPause, a_productionUsage, rci, capacityUsageProfile, out FindCapacityResult invalidCapacityResult))
                    {
                        return invalidCapacityResult;
                    }
                }
            }
            else if (a_ignoreRciProfile || a_startDate == rci.EndDate && a_startDate == current.Next?.Value?.StartDate)
            {
                //This is a gap or offline interval, we won't use the capacity. It is also possible that there is an online capacity interval that starts at exactly the same time 
                //as the end date of the inactive interval.
            }
            else
            {
                if (GetInvalidCapacityResult(a_canPause, a_productionUsage, rci, capacityUsageProfile, out FindCapacityResult invalidCapacityResult))
                {
                    return invalidCapacityResult;
                }
            }

            firstInterval = false;

            if (capacityNeeded <= 0)
            {
                break;
            }

            current = current.Next;
        }

        if (capacityNeeded == 0)
        {
            return new FindCapacityResult(capacityUsageProfile[^1].EndTicks, capacityUsageProfile);
        }
        else if (capacityNeeded < 0)
        {
            #if DEBUG
            DebugException.ThrowInDebug("I have failed. Somehow, there was excess capacity on the RCIs");
            #endif
        }

        return new FindCapacityResult(SchedulableSuccessFailureEnum.LackCapacity, 0, capacityUsageProfile);
    }

    private static bool GetInvalidCapacityResult(bool a_canPause, usageEnum a_productionUsage, ResourceCapacityInterval a_rci, CapacityUsageProfile a_ocp, out FindCapacityResult o_invalidCapacityResult)
    {
        o_invalidCapacityResult = null;
        if (!a_canPause)
        {
            //No retry time because end of the interval doesn't make sense since it can't span, it can't finish in one tick.
            //The scheduler will already retry at the next interval without a retry event.
            if (a_productionUsage == usageEnum.Clean)
            {
                o_invalidCapacityResult = new FindCapacityResult(SchedulableSuccessFailureEnum.FailedDueToCleanBeforeSpan, 0 /*a_rci.EndDate*/, a_ocp);
                return true;
            }

            // This is done here because you could run into back to back online intervals.
            o_invalidCapacityResult = new FindCapacityResult(SchedulableSuccessFailureEnum.CanPause, 0 /*a_rci.EndDate*/, a_ocp);
            return true;
        }

        if (a_rci.PreventOperationsFromSpanning)
        {
            if (a_productionUsage == usageEnum.Clean)
            {
                o_invalidCapacityResult = new FindCapacityResult(SchedulableSuccessFailureEnum.FailedDueToCleanBeforeSpan, a_rci.EndDate, a_ocp);
                return true;
            }

            o_invalidCapacityResult = new FindCapacityResult(SchedulableSuccessFailureEnum.LackCapacityWithRetry, a_rci.EndDate, a_ocp);
            return true;
        }

        if (a_rci.IntervalType == CapacityIntervalDefs.capacityIntervalTypes.Occupied)
        {
            o_invalidCapacityResult = new FindCapacityResult(SchedulableSuccessFailureEnum.Occupied, a_rci.EndDate, a_ocp);
            return true;
        }

        return false;
    }

    private long GetRciAvailableCapacity(long a_startDate,
                                         peopleUsages a_peopleUsage,
                                         decimal a_actNumberOfPeople,
                                         usageEnum a_productionUsage,
                                         ResourceCapacityInterval a_rci,
                                         CapacityUsageProfile a_capacityUsageProfile,
                                         long a_capacityNeeded)
    {
        long rciAvailableCapacity = 0;
        decimal resourceCapacityCost = a_rci.Overtime ? OvertimeHourlyCost : StandardHourlyCost;
        decimal efficiencyMultiplier = a_productionUsage == usageEnum.Run ? CycleEfficiencyMultiplier : 1;
        long cupStartDate = a_rci.StartDate;
        long cupEndDate = a_rci.EndDate;

        bool capacityAdjusted = false;
        decimal adjustmentMultiplier = 1m;
        if (a_rci.StartDate >= a_startDate)
        {
            //This interval starts after the start date of the activity production usage, we can use the whole rci capacity
            if (a_productionUsage == usageEnum.Run)
            {
                rciAvailableCapacity = CalculateCapacity(a_rci.StartDate, a_rci.EndDate, a_peopleUsage, a_actNumberOfPeople, a_rci, out capacityAdjusted, out adjustmentMultiplier);
            }
            else
            {
                rciAvailableCapacity = a_rci.EndDate - a_rci.StartDate;
            }
        }
        else
        {
            if (a_rci.EndDate > a_startDate)
            {
                //This interval ends after the start date of the activity production usage, we can use as much capacity as needed from 
                //the end start of the usage to the end of the interval.
                if (a_productionUsage == usageEnum.Run)
                {
                    rciAvailableCapacity = CalculateCapacity(a_startDate, a_rci.EndDate, a_peopleUsage, a_actNumberOfPeople, a_rci, out capacityAdjusted, out adjustmentMultiplier);
                }
                else
                {
                    rciAvailableCapacity = a_rci.EndDate - a_startDate;
                }

                //the operation capacity profile starts from the start of the activity usage
                cupStartDate = a_startDate;
            }
        }

        if (rciAvailableCapacity > 0)
        {
            if (rciAvailableCapacity > a_capacityNeeded)
            {
                //we have found more capacity than required, only use what's required and set the ocp end date appropriately
                rciAvailableCapacity = a_capacityNeeded;
                long capacityTimeUsed = capacityAdjusted ? (long)Math.Round(a_capacityNeeded / adjustmentMultiplier, MidpointRounding.AwayFromZero) : a_capacityNeeded;
                cupEndDate = cupStartDate + capacityTimeUsed;
            }

            decimal capacityHrs = (decimal)rciAvailableCapacity / TimeSpan.TicksPerHour;
            decimal cost = capacityHrs * resourceCapacityCost;
            a_capacityUsageProfile.Add(new OperationCapacity(cupStartDate, cupEndDate, rciAvailableCapacity, efficiencyMultiplier, cost, a_rci.Overtime));
        }

        return rciAvailableCapacity;
    }

    /// <summary>
    /// Finds the starting interval for a resource's capacity in reverse order, considering various capacity spans 
    /// (storage, post-processing, processing, and setup) and activity requirements.
    /// </summary>
    /// <param name="a_clockDate">The current clock date in ticks.</param>
    /// <param name="a_finishDate">The finish date in ticks, starting point of the back calculating.</param>
    /// <param name="a_capacity">The required capacity, including spans for storage, post-processing, processing, and setup.</param>
    /// <param name="a_startingInterval">The cached capacity interval to start on.</param>
    /// <param name="a_ia">The internal activity containing operation and resource requirement details.</param>
    /// <returns>
    /// A <see cref="Resource.FindStartFromEndResult"/> representing the result of the reverse capacity search, 
    /// including the start ticks for the setup span.
    /// </returns>
    public Resource.FindStartFromEndResult FindCapacityReverse(long a_clockDate, long a_finishDate, RequiredCapacity a_capacity, LinkedListNode<ResourceCapacityInterval> a_startingInterval, InternalActivity a_ia)
    {
        if (a_capacity == null)
        {
            return new Resource.FindStartFromEndResult(false, a_finishDate, 0);
        }

        string capacityCode = a_ia.Operation.ResourceRequirements.PrimaryResourceRequirement.CapacityCode;
        bool canPause = a_ia.Operation.CanPause;

        Resource.FindStartFromEndResult storageResult = FindCapacityReverse(a_clockDate,
            a_finishDate,
            a_capacity.StorageSpan.TimeSpanTicks,
            canPause,
            a_startingInterval,
            usageEnum.Storage,
            false,
            false,
            capacityCode,
            a_ia.PeopleUsage,
            a_ia.NbrOfPeople);
       
        if (!storageResult.Success)
        {
            return storageResult;
        }
        
        Resource.FindStartFromEndResult postProcessingResult = FindCapacityReverse(a_clockDate,
            storageResult.StartTicks,
            a_capacity.PostProcessingSpan.TimeSpanTicks,
            canPause,
            a_startingInterval,
            usageEnum.PostProcessing,
            false,
            false,
            capacityCode,
            a_ia.PeopleUsage,
            a_ia.NbrOfPeople);
      
        if (!postProcessingResult.Success)
        {
            return postProcessingResult;
        }

        Resource.FindStartFromEndResult productionResult = FindCapacityReverse(a_clockDate,
            postProcessingResult.StartTicks,
            a_capacity.ProcessingSpan.TimeSpanTicks,
            canPause,
            a_startingInterval,
            usageEnum.Run,
            a_capacity.SetupSpan.TimeSpanTicks == 0,
            false,
            capacityCode,
            a_ia.PeopleUsage,
            a_ia.NbrOfPeople);
       
        if (!productionResult.Success)
        {
            return productionResult;
        }

        Resource.FindStartFromEndResult setupResult = FindCapacityReverse(a_clockDate,
            productionResult.StartTicks,
            a_capacity.SetupSpan.TimeSpanTicks,
            canPause,
            a_startingInterval,
            usageEnum.Setup,
            true,
            false,
            capacityCode,
            a_ia.PeopleUsage,
            a_ia.NbrOfPeople);
        
        return setupResult;
    }

    /// <summary>
    /// A helper function to back calculate a duration without specifying details about the type of capacity. 
    /// </summary>
    /// <param name="a_clockDate"></param>
    /// <param name="a_finishDate"></param>
    /// <param name="a_capacity"></param>
    /// <returns>A start date calculated backwards based on online capacity for this resource</returns>
    internal Resource.FindStartFromEndResult FindUnconstrainedCapacityReverse(long a_clockDate, long a_finishDate, long a_capacity)
    {
        return FindCapacityReverse(a_clockDate, a_finishDate, a_capacity,
            true,
            null,
            usageEnum.Unspecified,
            false,
            false,
            string.Empty,
            peopleUsages.UseSpecifiedNbr,
            1m);
    }

    /// <summary>
    /// Determine when the capacity can be satisfied.
    /// /// </summary>
    /// <param name="a_clockDate"></param>
    /// <param name="a_finishDate">The date to back calculate from</param>
    /// <param name="a_capacity"></param>
    /// <param name="a_canPause">Whether the capacity can pause for inactive intervals.
    /// If false, any gap in online intervals will cause the capacity to be unusable
    /// It's possible this is not equal to the operation value, for example cleanout that must be added regardless of gaps</param>
    /// <param name="a_startingInterval"></param>
    /// <param name="a_ia"></param>
    /// <param name="a_productionUsage"></param>
    /// <param name="a_startOfActivity"></param>
    /// <param name="a_activityIsLate">Whether this activity would schedule past its JIT start date, meaning it would be late when using this interval</param>
    /// <param name="a_capacityCode">The capacity code this RR is constrained by. If a capacity interval has a group code set, it will need to match this RR in order to be used.
    /// If either are blank, no constraint is enforced</param>
    /// <param name="a_peopleUsage"></param>
    /// <param name="a_actNumberOfPeople"></param>
    /// <returns></returns>
    private Resource.FindStartFromEndResult FindCapacityReverse(long a_clockDate,
                                                                long a_finishDate,
                                                                long a_capacity,
                                                                bool a_canPause,
                                                                LinkedListNode<ResourceCapacityInterval> a_startingInterval,
                                                                usageEnum a_productionUsage,
                                                                bool a_startOfActivity,
                                                                bool a_activityIsLate,
                                                                string a_capacityCode,
                                                                peopleUsages a_peopleUsage,
                                                                decimal a_actNumberOfPeople)
    {
        if (a_capacity < 0)
        {
            throw new PTException("The required capacity<0.");
        }

        if (a_capacity == 0) //Capacity not required
        {
            return new Resource.FindStartFromEndResult(true, a_finishDate, 0);
        }

        if (a_finishDate <= a_clockDate) //Capacity not required
        {
            return new Resource.FindStartFromEndResult(false, a_finishDate, 0);
        }
        
        LinkedListNode<ResourceCapacityInterval> current = a_startingInterval;
        if (current == null)
        {
            //The existing node is not known. The scheduler tracks this for performance, but external calls may not have the interval node
            //First check the current node, most likely the start date is at the sim clock
            if (ResultantCapacity.Count == 0)
            {
                //TODO: Check if we should just use the ResourceCapacity list, it's better for JIT anyways
                //TODO: We already check for clock date below when going backwards
                CopyToResultantCapacity(a_clockDate);
            }
            
            current = ResultantCapacity.FindBackwards(a_finishDate, null);
            if (current == null)
            {
                //Something has gone wrong, or this resource doesn't have intervals at the start date
                return new Resource.FindStartFromEndResult(false, a_finishDate, 0);
            }
        }

        ResourceCapacityInterval final = current.Value;
        long capacityNeeded = a_capacity;
        
        ////Verify usage constraint
        //if (a_ia.PeopleUsage == InternalActivityDefs.peopleUsages.UseMultipleOfSpecifiedNbr && a_ia.CalculateCapacityMultiple(current.Value) == 0)
        //{
        //    //Don't start a multiple activity on an interval that doesn't have that multiple
        //    return new FindCapacityResult(SchedulableSuccessFailureEnum.LackCapacity, current.Next != null ? current.Next.Value.StartDate : 0);
        //}
        bool firstInterval = true;
        while (current != null)
        {
            ResourceCapacityInterval rci = current.Value;
            final = rci;

            if (rci.EndDate <= a_clockDate)
            {
                return new Resource.FindStartFromEndResult(false, a_clockDate, a_capacity - capacityNeeded);
            }

            long constrainedStart = Math.Max(a_clockDate, rci.StartDate);

            bool canScheduleResourceRequirement = true;
            //TODO: Is comparing first, and then checking for empty if equal faster here?
            if (rci.Active && !string.IsNullOrWhiteSpace(a_capacityCode) && !string.IsNullOrWhiteSpace(rci.CapacityCode))
            {
                if (rci.CapacityCode != a_capacityCode)
                {
                    canScheduleResourceRequirement = false;
                }
            }

            bool canScheduleProductionUsage = rci.CanScheduleProductionUsage(a_productionUsage);

            //Whether this capacity profile is eligible to schedule the activity capacity
            bool eligibleCapacityProfile = canScheduleResourceRequirement && canScheduleProductionUsage;

            //This block is to check that 
            if (!firstInterval && rci.PreventOperationsFromSpanning)
            {
                //We already started on an online interval, we can't span into this one. Reset the capacity
                capacityNeeded = a_capacity;
                firstInterval = true;
                continue;
            }

            //TODO: Cavan, check for UseOnlyWhenLate constraint
            if (rci.Active && eligibleCapacityProfile)
            {
                if (rci.StartDate >= a_finishDate)
                {
                    //We are at a future date, or the finish date is the start of the interval.
                    //There isn't any capacity on the start tick, use the last tick of the previous interval instead.
                    current = current.Previous;
                    continue;
                }

                long rciCapacity;
                bool capacityAdjusted = false;
                decimal adjustmentMultiplier = 1;

                if (rci.EndDate > a_finishDate) //Partial
                {

                    if (a_productionUsage == usageEnum.Run)
                    {
                        rciCapacity = CalculateCapacity(rci.StartDate, a_finishDate, a_peopleUsage, a_actNumberOfPeople, rci, out capacityAdjusted, out adjustmentMultiplier);
                    }
                    else
                    {
                        rciCapacity = a_finishDate - rci.StartDate;
                    }
                }
                else
                {
                    //Use the full interval, up to the clock date
                    if (a_productionUsage == usageEnum.Run)
                    {
                        rciCapacity = CalculateCapacity(constrainedStart, rci.EndDate, a_peopleUsage, a_actNumberOfPeople, rci, out capacityAdjusted, out adjustmentMultiplier);
                    }
                    else
                    {
                        rciCapacity = rci.EndDate - constrainedStart;
                    }
                }

                if (rciCapacity >= capacityNeeded)
                {
                    //We have found enough capacity, we need to check if the RCI is able to start the activity, or we would need to ignore this RCI's capacity and go to the 
                    //previous interval if activity can pause and the current interval allows spanning.
                    if (a_startOfActivity && !rci.CanStartActivity)
                    {
                        if (!a_canPause || rci.IntervalType == CapacityIntervalDefs.capacityIntervalTypes.Occupied)
                        {
                            capacityNeeded = a_capacity;
                            firstInterval = true;
                        }
                    }
                    else
                    {
                        long endDate = long.Min(rci.EndDate, a_finishDate);
                        long capacityUsed = capacityAdjusted ? (long)Math.Round(capacityNeeded / adjustmentMultiplier, MidpointRounding.AwayFromZero) : capacityNeeded;
                        long backCalculatedStart = endDate - capacityUsed;
                        return new Resource.FindStartFromEndResult(true, Math.Max(a_clockDate, backCalculatedStart), a_capacity);
                    }
                }
                else
                {
                    capacityNeeded -= rciCapacity;
                }

                firstInterval = false;
            }
            else if (!a_canPause)
            {
                // This is done here because you could run into back to back online intervals.
                capacityNeeded = a_capacity;
                firstInterval = true;
            }
            else if (rci.IntervalType == CapacityIntervalDefs.capacityIntervalTypes.Occupied)
            {
                capacityNeeded = a_capacity;
                firstInterval = true;
            }

            current = current.Previous;
        }

        return new Resource.FindStartFromEndResult(false, Math.Max(a_clockDate, final.StartDate), a_capacity - capacityNeeded);
    }

    /// <summary>
    /// Calculate the capacity available for an activity within a start and end date within a resource capacity interval.
    /// </summary>
    /// <param name="a_startDate">The start within the capacity interval.</param>
    /// <param name="a_endDate">The end within the capacity interval.</param>
    /// <param name="a_actNumberOfPeople"></param>
    /// <param name="a_rci"></param>
    /// <param name="o_capacityAdjusted"></param>
    /// <param name="o_adjustmentMultiplier"></param>
    /// <param name="a_peopleUsage"></param>
    /// <returns></returns>
    protected long CalculateCapacity(long a_startDate,
                                     long a_endDate,
                                     peopleUsages a_peopleUsage,
                                     decimal a_actNumberOfPeople,
                                     ResourceCapacityInterval a_rci,
                                     out bool o_capacityAdjusted,
                                     out decimal o_adjustmentMultiplier)
    {
        o_capacityAdjusted = false;
        o_adjustmentMultiplier = 1;
        if (!a_rci.Active)
        {
            #if DEBUG
            //throw new PTException("Attempt to calculate online time within an offline interval.");
            #endif

            return 0;
        }
        #if DEBUG
        if (a_endDate <= a_rci.StartDate || a_endDate > a_rci.EndDate)
        {
            throw new PTException("The end date isn't within the interval.");
        }

        if (a_startDate < a_rci.StartDate || a_startDate >= a_rci.EndDate)
        {
            throw new PTException("The start date isn't within the interval.");
        }
        #endif

        
        long timeSpan = a_endDate - a_startDate;

        decimal timeSpanTemp = timeSpan;

        if (a_peopleUsage == InternalActivityDefs.peopleUsages.UseAllAvailable)
        {
            if (a_rci.NbrOfPeople != 1)
            {
                timeSpanTemp *= a_rci.NbrOfPeople;
                o_adjustmentMultiplier = a_rci.NbrOfPeople;
                o_capacityAdjusted = true;
            }
        }
        else if (a_peopleUsage == peopleUsages.UseSpecifiedNbr)
        {
            decimal peopleMultiplier = Math.Min(a_actNumberOfPeople, a_rci.NbrOfPeople);
            if (peopleMultiplier != a_actNumberOfPeople)
            {
                peopleMultiplier /= a_actNumberOfPeople;
                timeSpanTemp *= peopleMultiplier;
                o_adjustmentMultiplier = peopleMultiplier;
                o_capacityAdjusted = true;
            }
        }
        else if (a_peopleUsage == peopleUsages.UseMultipleOfSpecifiedNbr)
        {
            decimal nbrOfPeople = a_rci.CalculateCapacityMultiple(a_actNumberOfPeople);
            timeSpanTemp *= nbrOfPeople;
            o_adjustmentMultiplier = nbrOfPeople;
            o_capacityAdjusted = true;
        }
        else
        {
            #if DEBUG
            throw new PTException("An unhandled type of people useage was encountered.");
            #endif
        }

        if (o_capacityAdjusted)
        {
            if (long.MaxValue < timeSpanTemp)
            {
                timeSpan = long.MaxValue;
            }
            else
            {
                timeSpan = (long)Math.Round(timeSpanTemp, MidpointRounding.AwayFromZero);
            }
        }

        return timeSpan;
    }

    /// <summary>
    /// The default value is int.MinValue. Also when SimulationInitialization() is executed this variable is reset to its default value.
    /// </summary>
    private int m_simStage = int.MinValue;

    /// <summary>
    /// Stages are converted to simStages, a continuous set of numbers starting from 0. The default value is int.MinValue. Also when SimulationInitialization() is executed this variable is reset to its
    /// default value.
    /// </summary>
    internal int SimStage
    {
        get => m_simStage;
        set => m_simStage = value;
    }
    #endregion

    #region Simulation
    #region m_activeDispatcher set function wrappers. Tight control over m_activeDispatcher must be maintained for debuggin purposes. Breakpoints can be set on these acces to see how the variable is being used.
    private void SetActiveDispatcherNull()
    {
        m_activeDispatcher = null;
    }

    private void SetActiveDispatcher(ReadyActivitiesDispatcher a_activeDispatcher)
    {
        m_activeDispatcher = a_activeDispatcher;
    }

    private void SetActiveDispatchersResource(InternalResource a_res)
    {
        m_activeDispatcher.Resource = a_res;
    }

    [NonSerialized] private ReadyActivitiesDispatcher m_activeDispatcher;

    /// <summary>
    /// This is the dispatcher that is used during simulation.
    /// It should only be changed just prior to a simulation.
    /// </summary>
    public ReadyActivitiesDispatcher ActiveDispatcher
    {
        get => m_activeDispatcher;

        internal set
        {
            SetActiveDispatcher(value);
            SetActiveDispatchersResource(this);
        }
    }
    #endregion

    public bool IsCapable(BaseId a_capabilityId)
    {
        return m_capabilityManager.Contains(a_capabilityId);
    }

    internal void ResetSimulationStateVariables(ResourceConnectorManager a_resourceConnectors, long a_clock, ScenarioOptions a_so)
    {
        ResetSimulationStateVariables(a_clock);
    }

    internal override void ResetSimulationStateVariables(long a_clock)
    {
        base.ResetSimulationStateVariables(a_clock);
        ClearAvailabilityVars();
        m_cleanoutHistoryData.ResetSimulationVariables();
    }

    #region sim bools
    private BoolVector32 m_simBools;

    private const int c_codesClearIdx = 0;
    private const int c_initialCapacityProfileRegenerationDoneIdx = 1;

    internal bool CodesClear
    {
        get => m_simBools[c_codesClearIdx];

        set => m_simBools[c_codesClearIdx] = value;
    }

    internal bool InitialCapacityProfileRegenerationDone
    {
        get => m_simBools[c_initialCapacityProfileRegenerationDoneIdx];
        set => m_simBools[c_initialCapacityProfileRegenerationDoneIdx] = value;
    }
    #endregion

    #region Clear Codes
    private long m_scheduledCountSinceLastClearCodes;

    internal long ScheduledCountSinceLastClearCodes => m_scheduledCountSinceLastClearCodes;

    protected void IncrementScheduledCountSinceLastClearCodes()
    {
        ++m_scheduledCountSinceLastClearCodes;
        CodesClear = false;
    }

    internal void Cleanout()
    {
        m_scheduledCountSinceLastClearCodes = 0;
        CodesClear = true;
    }
    #endregion

    internal override void SimulationInitialization(long a_clock, ScenarioDetail a_sd, long a_planningHorizonEndTicks)
    {
        base.SimulationInitialization(a_clock, a_sd, a_planningHorizonEndTicks);

        m_simBools.Clear();
        RegenerateCapacityProfile(a_planningHorizonEndTicks, false);

        LastScheduledResourceUnavailableEvent = null;
        LastScheduledResourceAvailableEvent = null;

        CreateDispatcher(a_sd.DispatcherDefinitionManager.GetById(NormalDispatcherId));

        ExperimentalDispatcherOne = a_sd.DispatcherDefinitionManager.GetById(ExperimentalDispatcherIdOne).CreateDispatcher();
        ExperimentalDispatcherTwo = a_sd.DispatcherDefinitionManager.GetById(ExperimentalDispatcherIdTwo).CreateDispatcher();
        ExperimentalDispatcherThree = a_sd.DispatcherDefinitionManager.GetById(ExperimentalDispatcherIdThree).CreateDispatcher();
        ExperimentalDispatcherFour = a_sd.DispatcherDefinitionManager.GetById(ExperimentalDispatcherIdFour).CreateDispatcher();

        SetActiveDispatcherNull();
        LastResultantCapacityNode = null;
        ClearAvailabilityVars();
        m_scheduledCountSinceLastClearCodes = 0;

        // [TANK_CODE] 
        if (m_tank != null)
        {
            m_tank.ResetSimulationStateVariables(a_clock, a_sd);
        }
    }

    internal virtual decimal CalculateNearestSawtoothSetupScore(decimal a_expectedMinSetupNbr, decimal a_expectedMaxSetupNbr, InternalActivity a_activity)
    {
        //*LRH*TODO*Make this abstract forcing everyone to implement it
        return 0;
    }
    #endregion

    #region Diagnostics
    /// <summary>
    /// Prints the resultant capacity to
    /// </summary>
    internal void PrintResultantCapacity(string a_filePath)
    {
        StreamWriter sw;

        if (a_filePath.Length != 0)
        {
            if (Directory.Exists(a_filePath))
            {
                Directory.Delete(a_filePath);
            }

            sw = File.CreateText(a_filePath);
        }
        else
        {
            sw = null;
        }

        StringBuilder sb = new ();

        sb.AppendFormat("Resource Name: {0}\n", Name);
        sb.AppendFormat("Resource ExternalId: {0}", ExternalId);

        System.Diagnostics.Trace.WriteLine(sb.ToString());

        LinkedListNode<ResourceCapacityInterval> node = ResultantCapacity.First;

        while (node != null)
        {
            ResourceCapacityInterval data = node.Value;

            string msg;

            if (data.Active)
            {
                msg = string.Format("On: \t{0}\tTO\t{1}", data.StartDateTime.ToLocalTime(), data.EndDateTime.ToLocalTime());
            }
            else
            {
                msg = string.Format("Off:\t{0}\tTO\t{1}", data.StartDateTime.ToLocalTime(), data.EndDateTime.ToLocalTime());
            }

            if (sw != null)
            {
                sw.WriteLine(msg);
            }
            else
            {
                System.Diagnostics.Trace.WriteLine(msg);
            }

            node = node.Next;
        }
    }
    #endregion

    #region Miscellaneous
    protected abstract int BlockCount { get; }
    #endregion

    /// <summary>
    /// Given two points: are they and all the points between them in online intervals.
    /// </summary>
    /// <returns></returns>
    internal bool ArePointsWithinAContinuousOnlineInterval(long a_startPointDate, long a_endPointDate)
    {
        return ResultantCapacity.ArePointsWithinAContinuousOnlineInterval(a_startPointDate, a_endPointDate);
    }

    internal long FindOnlineCapacityBetweenTwoDates(long a_startTicks, long a_endTicks)
    {
        return ResultantCapacity.FindOnlineCapacityBetweenTwoDates(a_startTicks, a_endTicks);
    }

    internal Resource.FindStartFromEndResult GetStartOfBufferFromEndDate(long a_clockTicks, long a_bufferEndTicks, long a_bufferDuration)
    {
        if (a_bufferDuration == 0)
        {
            //no buffer, return original start date
            return new Resource.FindStartFromEndResult(true, a_bufferEndTicks, 0);
        }

        if (a_bufferEndTicks <= a_clockTicks)
        {
            //before the clock date, assume infinite capacity
            return new Resource.FindStartFromEndResult(true, a_bufferEndTicks - a_bufferDuration, 0);
        }

        Resource.FindStartFromEndResult bufferStartResult = FindCapacityReverse(a_clockTicks, a_bufferEndTicks, a_bufferDuration, true, null, usageEnum.Unspecified, true, false, string.Empty, 0, 0);
        return bufferStartResult;
    }

    internal bool HasCleanoutIntervals()
    {
        for (int i = 0; i < CapacityIntervals.Count; ++i)
        {
            if (CapacityIntervals[i].CleanOutSetups)
            {
                return true;
            }
        }

        for (int i = 0; i < RecurringCapacityIntervals.Count; ++i)
        {
            if (RecurringCapacityIntervals[i].CleanOutSetups)
            {
                return true;
            }
        }

        return false;
    }

    #region Dispatching, calaculate nearest, lower, higher, sawtooth attribute number score.
    internal virtual decimal CalculateNearestAttributeNumberSawtoothScore(decimal a_expectedMinSetupNbr, decimal a_expectedMaxSetupNbr, InternalActivity a_activity, string a_attrName)
    {
        //*LRH*TODO*Make this abstract forcing everyone to implement it
        return 0;
    }
    #endregion

    //TODO: Move Costs region below to Scheduler.InternalResource when available

    #region Costs
    /// <summary>
    /// Sum of Overtime durations.
    /// </summary>
    public decimal OvertimeHours => ResourceCapacityIntervals.GetTotalOvertimeHours();

    /// <summary>
    /// Sum of Non-Overtime durations.
    /// </summary>
    public decimal OnlineNonOvertimeHours => ResourceCapacityIntervals.GetTotalOnlineNonOvertimeHours();

    /// <summary>
    /// Overtime Hours times Overtime Hourly Cost.
    /// </summary>
    public decimal OvertimeCosts => Convert.ToDecimal(OvertimeHours) * OvertimeHourlyCost;

    /// <summary>
    /// NonOvertime Hours times Standard Hourly Cost.
    /// </summary>
    public decimal OnlineNonOvertimeCosts => Convert.ToDecimal(OnlineNonOvertimeHours) * StandardHourlyCost;

    /// <summary>
    /// Returns a list of Dates indicating which days have net online time of some sort.
    /// </summary>
    /// <returns></returns>
    public Dictionary<DateTime, DateTime> GetOperatingDays(ScenarioDetail aSd)
    {
        Dictionary<DateTime, DateTime> days = new ();
        for (int i = 0; i < capacityProfile.ProfileIntervals.Count; i++)
        {
            ResourceCapacityInterval rci = capacityProfile.ProfileIntervals[i];
            if (rci.IntervalType != CapacityIntervalDefs.capacityIntervalTypes.Offline)
            {
                DateTime rciStartDateOnly = rci.StartDateTime.Date;
                if (!days.ContainsKey(rciStartDateOnly))
                {
                    days.Add(rciStartDateOnly, rciStartDateOnly);
                }
            }
        }

        return days;
    }
    #endregion Costs

    protected internal readonly CleanoutHistoryData m_cleanoutHistoryData = new();

    public void AddCleanoutHistory(DateTime a_reportedStartDate, DateTime a_reportedEndDate, InternalActivity a_activity)
    {
        m_cleanoutHistoryData.AddCleanoutHistory(a_reportedStartDate.Ticks, a_reportedEndDate.Ticks, a_activity);
    }
}