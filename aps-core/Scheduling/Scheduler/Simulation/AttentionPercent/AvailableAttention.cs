using System.Text;

namespace PT.Scheduler.Simulation;

/// <summary>
/// Used to help allocate and track available attention of multi-tasking resources.
/// </summary>
internal class AvailableAttention
{
    internal AvailableAttention(decimal a_availableAttention, bool a_online, long a_startTicks, long a_endTicks, ResourceCapacityInterval a_rci)
    {
        m_startTicks = a_startTicks;
        m_endTicks = a_endTicks;
        m_online = a_online;
        m_availableAttentionPercent = a_availableAttention;
        m_rci = a_rci;
        if (m_online)
        {
            m_allocationList = new LinkedList<Node>();
            m_allocationList.AddFirst(new Node(this));
        }
        #if DEBUG
        Common.PTMath.Interval.ValidateInterval(m_startTicks, m_endTicks);
        AvailableAttentionHelpers.ValidateAttentionPercent(m_availableAttentionPercent);
        #endif
    }

    /// <summary>
    /// Call this function if you want to cancel the allocations.
    /// </summary>
    internal void CancelAllocations()
    {
        #if DEBUG
        ValidateAllocations();
        #endif
        Node firstNode = m_allocationList.First.Value;
        firstNode.Init(this);
        m_allocationList.Clear();
        m_allocationList.AddFirst(firstNode);
    }

    /// <summary>
    /// Set the attention of this object to what's left in the first allocation node. All other allocations should be handled before calling this function since the allocation list will be cleared out.
    /// The start time will not be copied over; it's assumed the first allocation node's start time is always equal to its AvailableAttention.
    /// </summary>
    internal void ConsumeAttentionToFirstAllocationNode()
    {
        Node firstNode = m_allocationList.First.Value;
        #if DEBUG
        ValidateAllocations();
        #endif
        m_endTicks = firstNode.m_endTicks;
        m_availableAttentionPercent = firstNode.m_availableAttentionPercent;

        m_allocationList.Clear();
        m_allocationList.AddFirst(firstNode);
    }

    /// <summary>
    /// This presumes there's one allcation whose start and end times are equal to the AvailableAttention.
    /// </summary>
    internal void ConsumeSingleAllocation()
    {
        #if DEBUG
        ValidateAllocations();
        if (m_allocationList.Count != 1)
        {
            throw new Exception("There should only be one allocation.");
        }
        #endif
        m_availableAttentionPercent = m_allocationList.First.Value.m_availableAttentionPercent;
    }

    /// <summary>
    /// Start of the interval covered.
    /// </summary>
    internal long m_startTicks;

    /// <summary>
    /// End of the interval covered.
    /// </summary>
    internal long m_endTicks;

    /// <summary>
    /// The attention still available
    /// </summary>
    internal decimal m_availableAttentionPercent;

    /// <summary>
    /// The corresponding interval of this attention.
    /// </summary>
    internal ResourceCapacityInterval m_rci;

    /// <summary>
    /// This is null for Offline intervals.
    /// During the allocation process attention may be broken up. This list is initialized with a single element equal the remaining attention.
    /// As allocations occur it's broken up to track what's left for potential use by multiple resource requirements in the same activity.
    /// It's then either reset or the allocations are committed.
    /// This is used to handle the special case where an activity has multiple resource requirements and a single resource is able to satisfy more than one of them.
    /// The typical case will be a single node per AvailableAttention.
    /// </summary>
    internal LinkedList<Node> m_allocationList;

    /// <summary>
    /// Whether the attention is for an online resourceCapacityInterval.
    /// </summary>
    internal bool m_online;

    /// <summary>
    /// This is for class AvailableAttentionSet and is completely managed by AvailableAttentionSet. It uses it to keep track of whether it has added a node to a list.
    /// </summary>
    internal bool m_addedToAVLNodeAllocationList;

    /// <summary>
    /// Used to track allocations. AvailableAttention starts off with one of these. It breaks up as attention is allocated for one or more resource requirements of an activity.
    /// </summary>
    internal class Node
    {
        internal long m_startTicks;
        internal long m_endTicks;
        internal decimal m_availableAttentionPercent;

        internal Node(long a_startTicks, long a_endTicks, decimal a_availableAttentionPercent)
        {
            Construct(a_startTicks, a_endTicks, a_availableAttentionPercent);
        }

        internal Node(AvailableAttention a_attentionAvailable)
        {
            Init(a_attentionAvailable);
        }

        internal void Init(AvailableAttention a_attentionAvailable)
        {
            Construct(a_attentionAvailable.m_startTicks, a_attentionAvailable.m_endTicks, a_attentionAvailable.m_availableAttentionPercent);
        }

        private void Construct(long a_startTicks, long a_endTicks, decimal a_availableAttentionPercent)
        {
            m_startTicks = a_startTicks;
            m_endTicks = a_endTicks;
            m_availableAttentionPercent = a_availableAttentionPercent;
        }

        public override string ToString()
        {
            return string.Format("avail={0, 3}%; start/end=[{1, 3}, {2, 3})", m_availableAttentionPercent, m_startTicks, m_endTicks);
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new ();

        if (m_online)
        {
            sb.AppendFormat("online; avail={0, 3}%; ", m_availableAttentionPercent);
        }
        else
        {
            sb.Append("offline           ; ");
        }

        sb.AppendFormat("start/end=[{0, 3}, {1, 3}); ", m_startTicks, m_endTicks);

        if (m_online)
        {
            sb.AppendFormat("allocation count={0, 2}; ", m_allocationList.Count);
        }
        else
        {
            sb.AppendFormat("                     ");
        }

        sb.AppendFormat("m_addedToAVLNodeAllocationList={0}", m_addedToAVLNodeAllocationList);

        return sb.ToString();
    }

    #if DEBUG
    internal void ValidateAllocations()
    {
        // Validate Intervals
        if (m_allocationList.First.Value.m_startTicks != m_startTicks)
        {
            throw new Exception("The first allocation doesn't start at the right time.");
        }

        LinkedListNode<Node> currentNode = m_allocationList.First;
        while (currentNode.Next != null)
        {
            Node next = currentNode.Next.Value;
            if (currentNode.Value.m_endTicks != next.m_startTicks)
            {
                throw new Exception("The allocations for this interval aren't back to back.");
            }

            currentNode = currentNode.Next;
        }

        if (m_allocationList.Last.Value.m_endTicks != m_endTicks)
        {
            throw new Exception("The last allocation doesn't end at the right time.");
        }

        // Validate Attention percent
        currentNode = m_allocationList.First;
        while (currentNode != null)
        {
            AvailableAttentionHelpers.ValidateAttentionPercent(currentNode.Value.m_availableAttentionPercent);
            if (currentNode.Value.m_availableAttentionPercent > m_availableAttentionPercent)
            {
                throw new Exception("The available attention of the allocation node is higher than this AvailableAttention.");
            }

            currentNode = currentNode.Next;
        }

        if (!m_online && m_addedToAVLNodeAllocationList)
        {
            throw new Exception("The offline attention has it's m_addedToAVLNodeAllocationList flag set to true.");
        }
    }

    /// <summary>
    /// Use this function if you want to verify the attention and the only allocation in the list are equal.
    /// </summary>
    internal void ValidateAllocationsEqualAvailableAttention()
    {
        if (m_online)
        {
            if (m_allocationList.Count != 1)
            {
                throw new Exception("The allocation list should have 1 element.");
            }

            Node n = m_allocationList.First.Value;

            if (n.m_availableAttentionPercent != m_availableAttentionPercent || n.m_startTicks != m_startTicks || n.m_endTicks != m_endTicks)
            {
                throw new Exception("The only allocation in the list isn't equal to the attention percent.");
            }
        }
    }
    #endif
}