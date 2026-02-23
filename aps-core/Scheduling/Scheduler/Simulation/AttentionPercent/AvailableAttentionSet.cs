namespace PT.Scheduler.Simulation;

/// <summary>
/// Keep track of available attention for multi-tasking resources. There should be one instance of this for each multi-tasking resource.
/// This class should be initialized with ResourceCapacityIntervals's before being used. Plus, without gaps, it must span long.Min to long.Max.
/// The sequence to consume attention for an activity is as follows.
/// Call Allocate for each resource requirement
/// If it's not possible to allocate attention for all the resource requirements then call this class's CancelAllocations() method to clear out the allocations.
/// If all the resource requirements are satisfied and the activity will be added to the schedule (no other constraints will prevent it from scheduling), call ConsumeAllocations().
/// </summary>
internal class AvailableAttentionSet
{
    //Note version 9482 and earlier have the original AVLTree version of this code which was slightly slower.
    private readonly SortedDictionary<long, AvailableAttention> m_attentionTree = new (Comparer<long>.Default);

    /// <summary>
    /// A temporary list of AvailableAttentions that have some allocations against them.
    /// </summary>
    private readonly List<AvailableAttention> m_allocationList = new ();

    /// <summary>
    /// Special attention is required for the first and last capacity interval added. They must be setup so long.Min through long.Max are covered continuously.
    /// </summary>
    /// <param name="a_rci"></param>
    internal void Add(ResourceCapacityInterval a_rci)
    {
        int availableAttention = a_rci.Active ? 100 : 0;
        m_attentionTree.Add(a_rci.StartDate, new AvailableAttention(availableAttention, a_rci.Active, a_rci.StartDate, a_rci.EndDate, a_rci));
    }

    private struct AllocationInfo
    {
        internal AllocationInfo(LinkedListNode<AvailableAttention.Node> a_availableAttentionNode, decimal a_attentionPercent, long a_startAllocation, long a_endAllocation)
        {
            m_availableAttentionNode = a_availableAttentionNode;
            m_attentionPercentToAllocate = a_attentionPercent;
            m_startAllocationTicks = a_startAllocation;
            m_endAllocationTicks = a_endAllocation;
        }

        internal readonly LinkedListNode<AvailableAttention.Node> m_availableAttentionNode;
        internal readonly decimal m_attentionPercentToAllocate;
        internal readonly long m_startAllocationTicks;
        internal readonly long m_endAllocationTicks;

        public override string ToString()
        {
            return string.Format("%ToAlloc={0, 3}%; start/end=[{1, 3}, {2, 3})", m_attentionPercentToAllocate, m_startAllocationTicks, m_endAllocationTicks);
        }
    }

    #if DEBUG
    private InternalActivity m_z_currentActivity;
    #endif

    /// <summary>
    /// Allocate the required attention from the online capacity intervals. Returns whether the allocation succeeded.
    /// CancelAllocations or ConsumeAllocations must be called after it's determined whether the activity can be scheduled; otherwise the state of allocations will be ruined.
    /// </summary>
    /// <param name="a_attentionPercent">The attention percent to allocate.</param>
    /// <param name="a_startTicks">Must be within an online interval.</param>
    /// <param name="a_endTicks">Must be within an online interval.</param>
    /// <param name="a_allocationList">
    /// The nodes that have allocations are added to the end of this list. Other functions require this list to consume or cancel the allocations. If more than one of an
    /// activity's resource requirements need to allocate from the same resource, you'll need to make sure the same set of allocations are used.
    /// </param>
    /// <returns></returns>
    internal bool Allocate(ResourceRequirement a_rr, InternalActivity a_ia, long a_startTicks, long a_endTicks)
    {
        #if DEBUG
        AvailableAttentionHelpers.ValidateAttentionPercent(a_rr.AttentionPercent);
        Common.PTMath.Interval.ValidateInterval(a_startTicks, a_endTicks);

        if (m_z_currentActivity != null && m_z_currentActivity != a_ia)
        {
            if (m_allocationList.Count > 0)
            {
                throw new Exception("The activity has changed but there are still activities. ConsumeAllocations or CancelAllocations should have been called.");
            }
        }

        m_z_currentActivity = a_ia;
        #endif
        List<AvailableAttention> tempAllocatedAvailableAttentionList = new ();
        List<AllocationInfo> availableAttentionAllocationNodeList = new ();

        if (CanSatisfyAttention(a_rr, a_ia, a_startTicks, a_endTicks, tempAllocatedAvailableAttentionList, availableAttentionAllocationNodeList))
        {
            AllocateAttention(availableAttentionAllocationNodeList);

            foreach (AvailableAttention attn in tempAllocatedAvailableAttentionList)
            {
                if (!attn.m_addedToAVLNodeAllocationList)
                {
                    attn.m_addedToAVLNodeAllocationList = true;
                    m_allocationList.Add(attn);
                    #if DEBUG
                    if (!attn.m_online)
                    {
                        throw new Exception("The interval isn't online.");
                    }
                    #endif
                }
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Verify the required attention can be satisfied and store information on how to allocate attention.
    /// </summary>
    /// <param name="a_rr"></param>
    /// <param name="a_ia"></param>
    /// <param name="a_startTicks"></param>
    /// <param name="a_endTicks"></param>
    /// <param name="a_tempAllocatedAvailableAttentionList">
    /// This function adds AvailableAttentions to this list that will have some allocations made against them if this call is successful. If this call
    /// fails, ignore the values in this list.
    /// </param>
    /// <param name="a_availableAttention">If this function succeeds, this is a list of what to allocate.</param>
    /// <returns>Whether the attention can be satisfied. </returns>
    private bool CanSatisfyAttention(
        ResourceRequirement a_rr,
        InternalActivity a_ia,
        long a_startTicks,
        long a_endTicks,
        List<AvailableAttention> a_tempAllocatedAvailableAttentionList,
        List<AllocationInfo> a_availableAttention)
    {
        //********************************************************************************************************************
        // Use the new FindTest()
        // and scan the SortedDictionary for attention, filling in the 2 list data structures like below.
        //********************************************************************************************************************
        SortedDictionary<long, AvailableAttention>.Enumerator etr = m_attentionTree.GetEnumerator();
        if (Find(etr, a_startTicks))
        {
            while (true)
            {
                AvailableAttention currentAvailableAttention = etr.Current.Value;
                if (currentAvailableAttention.m_online)
                {
                    decimal attentionPercent = a_ia.GetAdjustedAttentionPercent(a_rr, currentAvailableAttention.m_rci);
                    AvailableAttentionHelpers.ValidateAttentionPercent(attentionPercent);
                    if (!currentAvailableAttention.m_online)
                    {
                        if (Common.PTMath.Interval.Contains(currentAvailableAttention.m_startTicks, currentAvailableAttention.m_endTicks, a_startTicks))
                        {
                            throw new Exception("The start ticks must be in an online interval.");
                        }

                        if (Common.PTMath.Interval.Contains(currentAvailableAttention.m_startTicks, currentAvailableAttention.m_endTicks, a_endTicks))
                        {
                            throw new Exception("The end ticks must be in an online interval.");
                        }
                    }

                    LinkedList<AvailableAttention.Node> allocationList = currentAvailableAttention.m_allocationList;
                    a_tempAllocatedAvailableAttentionList.Add(currentAvailableAttention);
                    // You'd only see multiple allocations in this loop when a resource is able to handle multiple resource requirements of the same activity.
                    LinkedListNode<AvailableAttention.Node> currentAllocationNode = allocationList.First;

                    do
                    {
                        long startIntersectionTicks;
                        long endIntersectionTicks;
                        AvailableAttention.Node currentAllocation = currentAllocationNode.Value;

                        if (Common.PTMath.Interval.Intersection(currentAllocation.m_startTicks, currentAllocation.m_endTicks, a_startTicks, a_endTicks, out startIntersectionTicks, out endIntersectionTicks))
                        {
                            if (currentAllocation.m_availableAttentionPercent >= attentionPercent)
                            {
                                a_availableAttention.Add(new AllocationInfo(currentAllocationNode, attentionPercent, startIntersectionTicks, endIntersectionTicks));
                            }
                            else
                            {
                                return false;
                            }

                            if (endIntersectionTicks == a_endTicks)
                            {
                                return true;
                            }
                        }
                    } while ((currentAllocationNode = currentAllocationNode.Next) != null);
                }

                Find(etr, currentAvailableAttention.m_endTicks);
            }
        }

        return false;

        // The advantage of the AVL tree implementation below is it can find teh start node in O log(n) as oppossed to n.
    }

    /// <summary>
    /// After a successful call to CanSatisfyAttention this function should be called to alter the data structures to allocate the attention.
    /// </summary>
    /// <param name="a_allocationList"></param>
    private void AllocateAttention(List<AllocationInfo> a_allocationList)
    {
        LinkedListNode<AvailableAttention.Node> allocationLinkedListNode;
        AvailableAttention.Node allocationNode;

        for (int allocationI = 0; allocationI < a_allocationList.Count; ++allocationI)
        {
            allocationLinkedListNode = a_allocationList[allocationI].m_availableAttentionNode;
            decimal attentionPercentToAllocate = a_allocationList[allocationI].m_attentionPercentToAllocate;
            long startIntersectionTicks = a_allocationList[allocationI].m_startAllocationTicks;
            long endIntersectionTicks = a_allocationList[allocationI].m_endAllocationTicks;

            allocationNode = allocationLinkedListNode.Value;

            if (startIntersectionTicks == allocationNode.m_startTicks && endIntersectionTicks == allocationNode.m_endTicks)
            {
                // attention from the entire segment.
                allocationNode.m_availableAttentionPercent -= attentionPercentToAllocate;
            }
            else if (startIntersectionTicks > allocationNode.m_startTicks && endIntersectionTicks < allocationNode.m_endTicks)
            {
                // attention comes off some of the middle of the segment.
                AvailableAttention.Node middle = new (startIntersectionTicks, endIntersectionTicks, allocationNode.m_availableAttentionPercent - attentionPercentToAllocate);
                AvailableAttention.Node last = new (endIntersectionTicks, allocationNode.m_endTicks, allocationNode.m_availableAttentionPercent);
                allocationNode.m_endTicks = middle.m_startTicks;
                LinkedListNode<AvailableAttention.Node> t = allocationLinkedListNode.List.AddAfter(allocationLinkedListNode, middle);
                allocationLinkedListNode = allocationLinkedListNode.List.AddAfter(t, last);
            }
            else if (startIntersectionTicks == allocationNode.m_startTicks)
            {
                // attention comes off a piece of the start of the segment.
                AvailableAttention.Node last = new (endIntersectionTicks, allocationNode.m_endTicks, allocationNode.m_availableAttentionPercent);
                allocationLinkedListNode = allocationLinkedListNode = allocationLinkedListNode.List.AddAfter(allocationLinkedListNode, last);
                allocationNode.m_availableAttentionPercent -= attentionPercentToAllocate;
                allocationNode.m_endTicks = endIntersectionTicks;
            }
            else
            {
                // attention from the end of the segment.
                AvailableAttention.Node last = new (startIntersectionTicks, endIntersectionTicks, allocationNode.m_availableAttentionPercent - attentionPercentToAllocate);
                allocationLinkedListNode = allocationLinkedListNode = allocationLinkedListNode.List.AddAfter(allocationLinkedListNode, last);
                allocationNode.m_endTicks = startIntersectionTicks;
            }
        }
    }

    /// <summary>
    /// This will only work if you're searching for nodes from lowest to highest and the data being searched for exists.
    /// </summary>
    /// <param name="a_ticks"></param>
    /// <returns></returns>
    /// <summary>
    /// A test version of the Find function written for a SortedDictionary.
    /// If it end up not being used, it should be deleted.
    /// </summary>
    /// <param name="a_etr"></param>
    /// <param name="a_startTicks"></param>
    /// <returns></returns>
    private bool Find(SortedDictionary<long, AvailableAttention>.Enumerator a_etr, long a_startTicks)
    {
        while (a_etr.MoveNext())
        {
            AvailableAttention attn = a_etr.Current.Value;

            if (a_startTicks < attn.m_startTicks)
            {
                // Keep going, this isn't the right node.
            }
            else if (a_startTicks >= attn.m_endTicks)
            {
                // Keep going, this isn't the right node.
            }
            else
            {
                return true;
            }

            return false;
        }

        return false;
    }

    /// <summary>
    /// If you're not going to be able to schedule the activity, this function must be called to delete the allocations and cleanup.
    /// </summary>
    internal void CancelAllocations()
    {
        for (int i = 0; i < m_allocationList.Count; ++i)
        {
            AvailableAttention availableAttention = m_allocationList[i];
            availableAttention.m_addedToAVLNodeAllocationList = false;
            #if DEBUG
            availableAttention.ValidateAllocations();
            #endif
            availableAttention.CancelAllocations();
        }

        m_allocationList.Clear();
        #if DEBUG
        m_z_currentActivity = null;
        ValidateNoAllocationsState();
        #endif
    }

    /// <summary>
    /// If the activity is going to be scheduled this must be called to consume the AvailableAttention, get rid of the allocations, and perform cleanup.
    /// </summary>
    internal void ConsumeAllocations()
    {
        for (int i = 0; i < m_allocationList.Count; ++i)
        {
            AvailableAttention availableAttention = m_allocationList[i];
            availableAttention.m_addedToAVLNodeAllocationList = false;
            #if DEBUG
            availableAttention.ValidateAllocations();
            #endif

            if (availableAttention.m_allocationList.Count == 1)
            {
                availableAttention.ConsumeSingleAllocation();
            }
            else
            {
                LinkedList<AvailableAttention.Node> allocationList = availableAttention.m_allocationList;
                LinkedListNode<AvailableAttention.Node> currentAllocationListNode = allocationList.First.Next;
                do
                {
                    AvailableAttention.Node availableAttentionNode = currentAllocationListNode.Value;
                    AvailableAttention newAvailableAttention = new (availableAttentionNode.m_availableAttentionPercent, true, availableAttentionNode.m_startTicks, availableAttentionNode.m_endTicks, availableAttention.m_rci);
                    m_attentionTree.Add(newAvailableAttention.m_startTicks, newAvailableAttention);
                } while ((currentAllocationListNode = currentAllocationListNode.Next) != null);

                availableAttention.ConsumeAttentionToFirstAllocationNode();
            }
        }

        m_allocationList.Clear();
        #if DEBUG
        m_z_currentActivity = null;
        ValidateNoAllocationsState();
        #endif
    }

    internal void GetTreeAsListInSortedOrder(List<AvailableAttention> a_availableAttentionList)
    {
        foreach (AvailableAttention attn in m_attentionTree.Values)
        {
            a_availableAttentionList.Add(attn);
        }
    }

    internal void WriteLine(bool a_includeAllocations)
    {
        int i = 0;

        foreach (AvailableAttention attn in m_attentionTree.Values)
        {
            Console.WriteLine(attn);
            if (attn.m_online)
            {
                Console.WriteLine("        {0}. {1}", ++i, attn);
            }
        }
    }
    #if DEBUG
    internal void ValidateTree(List<AvailableAttention> a_availableAttentionList)
    {
        // Traverse the tree from smallest to largest verifying each element's start time == the previous elements end time.
        List<AvailableAttention> availableAttentionList = a_availableAttentionList;
        availableAttentionList = CreateSortedTreeList(ref availableAttentionList);

        if (availableAttentionList[0].m_startTicks != long.MinValue)
        {
            throw new Exception("AttentionSet long.MinValue missing.");
        }

        for (int i = 1; i < m_attentionTree.Count; ++i)
        {
            AvailableAttention aa1 = availableAttentionList[i - 1];
            AvailableAttention aa2 = availableAttentionList[i];
            if (aa1.m_endTicks != aa2.m_startTicks)
            {
                throw new Exception("The attention set isn't continuous. It should run from long.min to long.Max without a break.");
            }
        }

        if (availableAttentionList[availableAttentionList.Count - 1].m_endTicks != long.MaxValue)
        {
            throw new Exception("AttentionSet long.MaxValue is missing.");
        }
    }

    private List<AvailableAttention> CreateSortedTreeList(ref List<AvailableAttention> availableAttentionList)
    {
        if (availableAttentionList == null)
        {
            availableAttentionList = new List<AvailableAttention>();
            GetTreeAsListInSortedOrder(availableAttentionList);
        }

        return availableAttentionList;
    }

    private void Validate_m_addedToAVLNodeAllocationList_are_false(List<AvailableAttention> a_availableAttentionList)
    {
        List<AvailableAttention> availableAttentionList = a_availableAttentionList;
        CreateSortedTreeList(ref availableAttentionList);

        foreach (AvailableAttention aa in availableAttentionList)
        {
            if (aa.m_addedToAVLNodeAllocationList)
            {
                throw new Exception("Some AvailableAttention.m_addedToAVLNodeAllocationList is true");
            }
        }
    }

    /// <summary>
    /// The tree should be in a clean state.
    /// Such as before performing allocations, after consuming or cancelling allocations.
    /// </summary>
    internal void ValidateNoAllocationsState()
    {
        List<AvailableAttention> availableAttentionList = new ();

        Validate_m_addedToAVLNodeAllocationList_are_false(availableAttentionList);
        ValidateTree(availableAttentionList);

        foreach (AvailableAttention aa in m_attentionTree.Values)
        {
            aa.ValidateAllocationsEqualAvailableAttention();
        }

        if (m_allocationList.Count != 0)
        {
            throw new Exception("The allocation list should be empty.");
        }
    }
    #endif
}