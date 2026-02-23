using System.Collections;
using System.Diagnostics;
using System.Text;

using PT.APSCommon;
using PT.Common.Debugging;
using PT.Common.Exceptions;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.Scheduler.Schedule.Resource;
using PT.Scheduler.Simulation;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

internal interface IQtyProfile
{
    //TODO: DO we really need this?
}

/// <summary>
/// Summary description for QtyProfile.
/// </summary>
internal abstract partial class QtyProfile<QtyNodeT> : IQtyProfile, IEnumerable<QtyNodeT> where QtyNodeT : QtyNode
{
    internal QtyProfile() { }

    /// <summary>
    /// Create a deep copy of the QtyProfile.
    /// </summary>
    /// <param name="a_sourceQtyProfile"></param>
    internal QtyProfile(QtyProfile<QtyNodeT> a_sourceQtyProfile)
    {
        TestStructure();

        using IEnumerator<QtyProfileTimePoint<QtyNodeT>> timePointEnumerator = a_sourceQtyProfile.m_nodes.GetEnumerator();
        while (timePointEnumerator.MoveNext())
        {
            QtyProfileTimePoint<QtyNodeT> timePoint = timePointEnumerator.Current;
            //Create the new time point to add
            QtyProfileTimePoint<QtyNodeT> newTimePoint = new ();

            //Copy each node and add it to the new time point
            using IEnumerator<QtyNodeT> nodeEnumerator = timePoint.GetEnumerator();
            while (nodeEnumerator.MoveNext())
            {
                QtyNodeT node = nodeEnumerator.Current;
                QtyNodeT newNode = CreateQtyNode(node); //Create a copy. We use this function since it can be overwritten
                newTimePoint.AddToEnd(newNode);
            }

            //Add the copied time point
            m_nodes.AddLast(newTimePoint);
        }

        m_changedSinceReset = true;
        TestIteration();
        TestStructure();
    }

    private LinkedList<QtyProfileTimePoint<QtyNodeT>> m_nodes = new ();

    internal QtyProfileTimePoint<QtyNodeT> First => m_nodes.First.Value;

    internal QtyProfileTimePoint<QtyNodeT> Last => m_nodes.Last.Value;

    /// <summary>
    /// This can only be called if this profile has elements, otherwise a null reference exception will occur.
    /// Returns the date ticks of the latest QtyNode.
    /// </summary>
    /// <returns></returns>
    internal long GetTimeOfLastAdjustment()
    {
        return Last.Date;
    }

    //public void AddToEnd(long a_simClock, MaterialRequirement a_reason, decimal a_neededQty)
    //{
    //    TestStructure();
    //    QtyNode node = CreateQtyNode(a_simClock, a_reason, a_neededQty);
    //    AddToEnd(node);
    //    TestStructure();
    //}

    /// <summary>
    /// The date must be greater than any other element in the profile.
    /// Within this file this function is sometimes used to add a potentially existing node.
    /// The QtyNode.next field of the node is not affected. This function and its caller are tightly linked. The field
    /// QtyNode.next is used again in the linked field and then cleared. To untighly link them change both functions.
    /// In your calling code you need to make sure this is handled correctly.
    /// </summary>
    /// <param name="node"></param>
    internal void AddToEnd(QtyNodeT a_node)
    {
        TestStructure();
        if (!HasNodes)
        {
            m_nodes.AddFirst(new QtyProfileTimePoint<QtyNodeT>(a_node, this));
            // There's no need to clear out the previous node because of the way this function is used by functions within this class.
            // The first node added to this list is either new or the first node of an existing list.
        }
        else
        {
            #if DEBUG
            if (Last.Date > a_node.Date)
            {
                throw new PTException("AddToEnd() can't be executed on a QtyProfile if it's date isn't greater than the last element in the profile.");
            }
            #endif

            //QtyProfileTimePoint existingTimePoint = FindByTime(a_node.Date);
            if (Last.Date == a_node.Date)
            {
                Last.AddToEnd(a_node);
            }
            else
            {
                // Add as the new Last node.
                QtyProfileTimePoint<QtyNodeT> newTimePoint = new (a_node, this);
                m_nodes.AddLast(newTimePoint);
            }
        }

        m_changedSinceReset = true;
        a_node.Removed = false;
        TestStructure();
    }

    internal void Insert(QtyNodeT a_node)
    {
        TestStructure();
        if (!HasNodes)
        {
            m_nodes.AddFirst(new QtyProfileTimePoint<QtyNodeT>(a_node, this));
            // There's no need to clear out the previous node because of the way this function is used by functions within this class.
            // The first node added to this list is either new or the first node of an existing list.
        }
        else
        {
            QtyProfileTimePoint<QtyNodeT> existingTimePoint = FindByTime(a_node.Date);
            if (existingTimePoint != null)
            {
                existingTimePoint.AddToEnd(a_node);
            }
            else
            {
                // Add as the new timepoint node.
                bool added = false;
                QtyProfileTimePoint<QtyNodeT> newTimePoint = new (a_node, this);
                foreach (QtyProfileTimePoint<QtyNodeT> qtyProfileTimePoint in m_nodes)
                {
                    if (qtyProfileTimePoint.Date > newTimePoint.Date)
                    {
                        LinkedListNode<QtyProfileTimePoint<QtyNodeT>> llNode = m_nodes.Find(qtyProfileTimePoint);
                        m_nodes.AddBefore(llNode, newTimePoint);
                        added = true;
                        break;
                    }
                }

                if (!added)
                {
                    m_nodes.AddLast(newTimePoint);
                }
            }
        }

        m_changedSinceReset = true;
        a_node.Removed = false;
        TestStructure();
    }

    /// <summary>
    /// The date must be greater than any other element in the profile.
    /// Within this file this function is sometimes used to add a potentially existing node.
    /// The QtyNode.next field of the node is not affected. This function and its caller are tightly linked. The field
    /// QtyNode.next is used again in the linked field and then cleared. To untighly link them change both functions.
    /// In your calling code you need to make sure this is handled correctly.
    /// </summary>
    /// <param name="node"></param>
    internal virtual void AddToFront(QtyNodeT a_node)
    {
        TestStructure();
        if (!HasNodes)
        {
            m_nodes.AddFirst(new QtyProfileTimePoint<QtyNodeT>(a_node, this));
            // There's no need to clear out the previous node because of the way this function is used by functions within this class.
            // The first node added to this list is either new or the first node of an existing list.
        }
        else
        {
            #if DEBUG
            if (First.Date < a_node.Date)
            {
                throw new PTException("AddToFront() can't be executed on a QtyProfile if it's date isn't less than the first element in the profile.");
            }
            #endif

            if (First.Date == a_node.Date)
            {
                First.AddToEnd(a_node);
            }
            else
            {
                // Add as the new Last node.
                QtyProfileTimePoint<QtyNodeT> newTimePoint = new (a_node, this);
                m_nodes.AddFirst(newTimePoint);
            }
        }

        m_changedSinceReset = true;
        a_node.Removed = false;
        TestStructure();
    }

    /// <summary>
    /// Release all the QtyNodes in this profile.
    /// </summary>
    internal void Clear()
    {
        TestStructure();
        ClearNodes();
        m_nodes.Clear();
        m_changedSinceReset = true;
        TestStructure();
    }

    /// <summary>
    /// Disconnect all the nodes in this profile.
    /// Values such as Previous, Next, and NextDangler are nulled out.
    /// Removed is set to true.
    /// </summary>
    private void ClearNodes()
    {
        TestStructure();
        using LinkedList<QtyProfileTimePoint<QtyNodeT>>.Enumerator enumerator = m_nodes.GetEnumerator();
        while (enumerator.MoveNext())
        {
            QtyProfileTimePoint<QtyNodeT> timePoint = enumerator.Current;
            timePoint.Clear();
        }
        //TestStructure();
    }

    private void Remove(QtyProfileTimePoint<QtyNodeT> a_timePoint)
    {
        a_timePoint.Clear();
        m_nodes.Remove(a_timePoint);
    }

    private void FlagDisconnect(QtyProfileTimePoint<QtyNodeT> a_timePoint)
    {
        a_timePoint.Disconnected = true;
    }

    internal void DisconnectNodes()
    {
        TestStructure();
        LinkedListNode<QtyProfileTimePoint<QtyNodeT>> node = m_nodes.First;
        while (node != null)
        {
            if (node.Value.Disconnected)
            {
                LinkedListNode<QtyProfileTimePoint<QtyNodeT>> nodeToRemove = node;
                node = node.Next;
                //nodeToRemove.Value.Clear(); Don't clear, it's just being disconnected, not 'removed'
                m_nodes.Remove(nodeToRemove);
                continue;
            }

            node = node.Next;
        }

        m_changedSinceReset = true;
        TestStructure();
    }

    internal bool Remove(QtyNodeT a_node)
    {
        TestStructure();
        if (!a_node.Removed) // skip removal if it's already been removed.
        {
            QtyProfileTimePoint<QtyNodeT> timePoint = FindByTime(a_node.Date);

            if (timePoint == null)
            {
                // The profile is empty. This shouldn't happen.
                return false;
            }


            a_node.Removed = timePoint.Remove(a_node);
            if (!timePoint.HasNodes)
            {
                Remove(timePoint);
            }

            m_changedSinceReset = true;
        }

        TestStructure();

        return a_node.Removed;
    }

    internal QtyProfileTimePoint<QtyNodeT> FindByTime(long a_time)
    {
        using IEnumerator<QtyProfileTimePoint<QtyNodeT>> enumerator = m_nodes.GetEnumerator();
        while (enumerator.MoveNext())
        {
            QtyProfileTimePoint<QtyNodeT> timePoint = enumerator.Current;
            if (timePoint.Date == a_time)
            {
                return timePoint;
            }

            if (timePoint.Date > a_time)
            {
                //We past where it would be, it's not in the list
                break;
            }
        }

        return null;
    }

    internal LinkedListNode<QtyProfileTimePoint<QtyNodeT>> FindByTime(LinkedListNode<QtyProfileTimePoint<QtyNodeT>> a_startingNode, long a_time)
    {
        LinkedListNode<QtyProfileTimePoint<QtyNodeT>> node = a_startingNode;
        if (node == null)
        {
            node = m_nodes.First;
        }

        while (node != null)
        {
            if (node.Value.Date == a_time)
            {
                return node;
            }

            if (node.Value.Date > a_time)
            {
                //We past where it would be, it's not in the list
                break;
            }

            node = node.Next;
        }

        return null;
    }

    private QtyProfileTimePoint<QtyNodeT> FindByTimeAtOrBefore(long a_time)
    {
        LinkedListNode<QtyProfileTimePoint<QtyNodeT>> node = m_nodes.Last;
        while (node != null)
        {
            if (node.Value.Date <= a_time)
            {
                return node.Value;
            }

            node = node.Previous;
        }

        return null;
    }

    /// <summary>
    /// Merge a QtyProfile into this profile. All the elements of the profile are taken from it, so expect the argument profile to have been altered.
    /// </summary>
    /// <param name="bProfile">The profile that is merged into this profile. All of this profiles elements are taken from it.</param>
    internal virtual void Merge(QtyProfile<QtyNodeT> a_consume)
    {
        TestStructure();

        // The current node of the profile that's being merged with this profile.
        using LinkedList<QtyProfileTimePoint<QtyNodeT>>.Enumerator consumedEnumerator = a_consume.m_nodes.GetEnumerator();

        //Cache the forward moving node so we don't have to start at the beginning of the list every time.
        LinkedListNode<QtyProfileTimePoint<QtyNodeT>> existingTimePoint = m_nodes.First;
        while (consumedEnumerator.MoveNext())
        {
            QtyProfileTimePoint<QtyNodeT> consumedTimePoint = consumedEnumerator.Current;

            existingTimePoint = FindByTime(existingTimePoint, consumedTimePoint.Date);
            if (existingTimePoint != null)
            {
                existingTimePoint.Value.Merge(consumedTimePoint);
            }
            else
            {
                QtyProfileTimePoint<QtyNodeT> newTimePoint = new (consumedTimePoint, this);
                existingTimePoint = InsertTimePoint(newTimePoint);
            }
        }

        m_changedSinceReset = true;
        TestStructure();
    }

    /// <summary>
    /// Merge a QtyProfile into this profile. All the elements of the profile are taken from it, so expect the argument profile to have been altered.
    /// </summary>
    /// <param name="bProfile">The profile that is merged into this profile. All of this profiles elements are taken from it.</param>
    internal void Merge(QtyNodeT a_newNode)
    {
        TestStructure();
        
        QtyProfileTimePoint<QtyNodeT> newTimePoint = new (a_newNode, this);
        InsertTimePoint(newTimePoint);
        m_changedSinceReset = true;

        TestStructure();
    }

    private LinkedListNode<QtyProfileTimePoint<QtyNodeT>> InsertTimePoint(QtyProfileTimePoint<QtyNodeT> a_newTimePoint)
    {
        m_changedSinceReset = true;

        if (!HasNodes)
        {
            return m_nodes.AddFirst(a_newTimePoint);
        }

        if (First.Date > a_newTimePoint.Date)
        {
            //Add to start
            return m_nodes.AddFirst(a_newTimePoint);
        }

        if (Last.Date < a_newTimePoint.Date)
        {
            //Add to End
            return m_nodes.AddLast(a_newTimePoint);
        }

        LinkedListNode<QtyProfileTimePoint<QtyNodeT>> timePoint = m_nodes.First;
        while (timePoint != null)
        {
            if (timePoint.Value.Date == a_newTimePoint.Date)
            {
                //TODO: Should this happen?
                //Merge
                timePoint.Value.Merge(a_newTimePoint);
                return timePoint;
            }

            if (a_newTimePoint.Date > timePoint.Value.Date && a_newTimePoint.Date < timePoint.Next.Value.Date)
            {
                //Insert as next
                return m_nodes.AddAfter(timePoint, a_newTimePoint);
            }

            timePoint = timePoint.Next;
        }

        throw new PTException("Inserted node was not added");
    }

    [Conditional("TEST")]
    protected void TestStructure()
    {
        //Check for empty qty timepoints
        if (m_nodes.Count > 0)
        {
            foreach (QtyProfileTimePoint<QtyNodeT> qtyProfileTimePoint in m_nodes)
            {
                if (!qtyProfileTimePoint.HasNodes)
                {
                    throw new DebugException("Timepoint does not have nodes");
                }
            }
        }

        //TestIteration();
        //QtyNode prevCur = First;
        //QtyNode cur = First;

        //while (cur != null)
        //{
        //    QtyNode prevDangler = cur;
        //    QtyNode dangler = cur;
        //    bool topLevelNode = true;

        //    while (!topLevelNode && dangler != null)
        //    {
        //        if (dangler != cur)
        //        {
        //            if (dangler.Previous != null || dangler.Next != null)
        //            {
        //                throw new Exception("A dangler has a Previous or Next.");
        //            }
        //        }
        //        if (prevDangler != dangler && prevDangler.Date != dangler.Date)
        //        {
        //            throw new Exception("The date of the dangler doesn't match the dangler above it. ");
        //        }
        //        QtyNodeExpirable qpe = dangler as QtyNodeExpirable;
        //        if (qpe != null)
        //        {
        //            if (qpe.QtyProfile != this)
        //            {
        //                throw new Exception("The dangler isn't pointing to the right QtyProfile.");
        //            }
        //        }

        //        prevDangler = dangler;
        //        dangler = dangler.NextDangler;
        //    }

        //    if (prevCur != cur && prevCur.Date >= cur.Date)
        //    {
        //        throw new Exception("The node's date is greater than its previous node.");
        //    }

        //    topLevelNode = false;
        //    prevCur = cur;
        //    cur = cur.Next;
        //}
    }

    [ConditionalAttribute("TEST")]
    private void TestIteration()
    {
        if (Count() == 1)
        {
            //if (m_first != m_last)
            //{
            //    int xxx = 0;

            //    if(m_first.Date== 637278291782160000)
            //    {
            //        if(m_last.Reason is InternalActivity)
            //        {
            //            InternalActivity act = (InternalActivity)m_last.Reason;
            //            if(act.Id== -2143191228)
            //            {
            //                int xxx = 0;
            //            }
            //        }
            //    }
            //}
            //throw new Exception("m_first and m_last aren't equal and there is suppossed to be 1 node in the QtyProfile. ");
        }

        //QtyProfileEnumeratorBase etr = null;

        //List<QtyNode> forwardNodes = new List<QtyNode>();
        //etr = new QtyProfileEnumerator(this, First);
        //while (etr.MoveNext())
        //{
        //    HashSet<QtyNode> danglers = new HashSet<QtyNode>();
        //    forwardNodes.Add(etr.Current);
        //}

        //etr = new QtyProfileReverseEnumerator(this);
        //List<QtyNode> reverseNodes = new List<QtyNode>();
        //while (etr.MoveNext())
        //{
        //    reverseNodes.Add(etr.Current);
        //}
        //if (!ContainsAll(forwardNodes, reverseNodes))
        //{
        //    throw new Exception("Iterating forward and backwards yields different results.");
        //}
        //if (!ContainsAll(reverseNodes, forwardNodes))
        //{
        //    throw new Exception("Iterating forward and backwards yields different results.");
        //}
    }

    private static bool ContainsAll(List<QtyNode> forwardNodes, List<QtyNode> reverseNodes)
    {
        bool ret = true;
        foreach (QtyNode n in reverseNodes)
        {
            if (!forwardNodes.Contains(n))
            {
                ret = false;
            }
        }

        return ret;
    }

    internal bool HasNodes => m_nodes.First != null;

    internal bool SingleNode => m_nodes.Count == 1;

    [ConditionalAttribute("TEST")]
    private void BreakOnCalculateQtyTotals()
    {
        if (Count() == 4)
        {
            int x = 0;
        }
    }

    /// <summary>
    /// Call this function before you try to FindQty.
    /// Right now this function is called everytime FindQty is called. You might be able to decrease the number of times this is called.
    /// </summary>
    /// </summary>
    /// <param name="a_simClock"></param>
    /// <param name="a_sd"></param>
    /// <param name="a_mr">material requireer. The object material is for. For example a Job, SO, or TO.</param>
    /// <param name="a_inv"></param>
    /// <param name="a_overlapOptions"></param>
    /// <param name="a_matlReq">Can be null in the event the requireer doesn't have a Material Requirement, for instance SOs and TOs.</param>
    /// <param name="a_lotUsability"></param>
    /// <param name="a_usability"></param>
    internal void CalculateQtyTotals(long a_simClock, ScenarioDetail a_sd, object a_mrr, Inventory a_inv, bool a_overlap, IMaterialAllocation a_matlAllocation, ILotEligibility a_lotUsability, bool a_checkMatlEligibility = true)
    {
        BreakOnCalculateQtyTotals();
        m_lastDanglerQtysWereCalculatedIn = null;
        if (HasNodes)
        {
            IEnumerator<QtyNode> qtyProfileEtr = CreateDirectionalEnumerator(a_simClock, GetAllocationForInventory(a_inv, a_matlAllocation, null));

            while (qtyProfileEtr.MoveNext())
            {
                //CalculateEligibleDanglerTotalsForNode(a_simClock, a_sd, a_mrr, a_inv, qtyProfileEtr.Current, a_overlap);

                //CalculateEligibleDanglerTotals(a_simClock, a_sd, a_mrr, a_inv, a_overlapOptions, a_matlAllocation, a_lotUsability, a_usability, usabilityParams, a_checkMatlEligibility);
            }
        }
    }

    // Used as a temporary variable by CalcualteQtyTotals and its helper functions (such as CalculateDanglerTotals).
    private QtyNode m_lastDanglerQtysWereCalculatedIn;
    
    /// <summary>
    /// Returns the first node where enough quantity is available. null if not enough is ready yet.
    /// Side effects. Calculates quantity totals limited by lot.usability, UsableQty and UnallocatedQty are updated per node.
    /// </summary>
    /// <param name="qty">The desired quantity.</param>
    /// <returns>The first node that has at least as much of the specified quantity.</returns>
    internal FindAvailQtyResult FindUnallocatedQty(ScenarioDetail a_sd, long a_simClock, object a_mr, Inventory a_inv, bool a_overlap, IMaterialAllocation a_matlAlloc, ILotEligibility a_lotUsability)
    {
        CalculateQtyTotals(a_simClock, a_sd, a_mr, a_inv, a_overlap, a_matlAlloc, a_lotUsability);
        //Reset(true, true);

        return new (); //FindUnallocatedQty(a_sd, a_simClock, a_mr, a_matlAlloc, a_lotUsability, a_inv);
    }

    /// <summary>
    /// The result of a call to FindUnallocatedQty() that describes the type and amount of quantities that are unallocated; available for use
    /// by a material requirement during scheduling.
    /// </summary>
    internal struct FindAvailQtyResult
    {
        /// <summary>
        /// Unallocated quantitiy from inventory.
        /// </summary>
        internal decimal InvQty;

        /// <summary>
        /// Unallocated quantity from activities.
        /// </summary>
        internal decimal ExpectedActQty;

        /// <summary>
        /// Unallocated quantity from other sources.
        /// </summary>
        internal decimal OtherQty;

        /// <summary>
        /// Unallocated quantity from inventory.
        /// </summary>
        internal decimal ExpiredInvQty;

        /// <summary>
        /// Unallocated quantity from activities.
        /// </summary>
        internal decimal ExpiredActQty;

        /// <summary>
        /// Unallocated quantity from other sources.
        /// </summary>
        internal decimal ExpiredOtherQty;

        internal decimal CalcTotalQty()
        {
            decimal total = InvQty + ExpectedActQty + OtherQty + ExpiredInvQty + ExpiredActQty + ExpiredOtherQty;
            return total;
        }

        public override string ToString()
        {
            return string.Format("InvQty={0}; expectedActQty={1}; otherQty={2}", InvQty, ExpectedActQty, OtherQty);
        }
    }

    [ConditionalAttribute("TEST")]
    private void BreakOnFindUnallocatedQty(long a_simClock, object a_mrr)
    {
        BaseIdObject bio = a_mrr as BaseIdObject;
        if (bio != null)
        {
            if (bio.Id == -2147483382)
            {
                int xxx = 0;
            }
        }
    }

    /// <summary>
    /// Presumes the QtyProfile's quantitly values have already been reset and the only  IUsabilityRequirement.IsUsable() quantities have been stored in the totals
    /// and unallocated values.
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_simClock"></param>
    /// <param name="a_mrr">Can be null if the material requirer isn't an activity.</param>
    /// <param name="a_inv"></param>
    /// <returns></returns>
    internal FindAvailQtyResult FindUnallocatedQty(ScenarioDetail a_sd, long a_simClock, object a_mrr, IMaterialAllocation a_matlAlloc, ILotEligibility a_lotUsability, Inventory a_inv, IConnectedResourceSource a_connectedResource)
    {
        FindAvailQtyResult result = new ();
        BreakOnFindUnallocatedQty(a_simClock, a_mrr);

        IEnumerator<QtyNode> etr = CreateDirectionalEnumerator(a_simClock, GetAllocationForInventory(a_inv, a_matlAlloc, null));
        while (etr.MoveNext())
        {
            QtyNode node = etr.Current;
            //if (node.IsMatlEligible(a_simClock, a_sd, a_mrr, a_inv, a_matlAlloc, a_lotUsability, a_connectedResource))
            {
                switch (node.Type)
                {
                    case SupplyTypeEnum.Inventory:
                        //if (node is QtyNodeExpirable expirableNode && expirableNode.ExpirationTicks < a_simClock)
                        //{
                        //    result.ExpiredInvQty += node.UnallocatedQty;
                        //}
                        //else
                    {
                        result.InvQty += node.UnallocatedQty;
                    }

                        break;

                    case SupplyTypeEnum.Activity:
                        //if (node is QtyNodeExpirable expirableNodeAct && expirableNodeAct.ExpirationTicks < a_simClock)
                        //{
                        //    result.ExpiredActQty += node.UnallocatedQty;
                        //}
                        //else
                    {
                        result.ExpectedActQty += node.UnallocatedQty;
                    }

                        break;

                    default:
                        //if (node is QtyNodeExpirable expirableNodeOther && expirableNodeOther.ExpirationTicks < a_simClock)
                        //{
                        //    result.ExpiredOtherQty += node.UnallocatedQty;
                        //}
                        //else
                    {
                        result.OtherQty += node.UnallocatedQty;
                    }

                        break;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Presumes FindUnallocatedQty() or CalculateQtyTotals() has been called to reset the data structure and filter by usability (such as shelf-life).
    /// </summary>
    /// <param name="a_qty">The search will stop after this much quantity has been found.</param>
    /// <returns>The first node that has at least as much of the specified quantity.</returns>
    internal QtyNode FindUnallocatedQty(ScenarioDetail a_sd, long a_simClock, object a_act, Inventory a_inv, IMaterialAllocation a_matlAlloc, ILotEligibility a_lotUsability, decimal a_qty)
    {
        decimal eligibleQty = 0;
        using IEnumerator<QtyNode> nodeEnumerator = GetEnumerator();

        while (nodeEnumerator.MoveNext())
        {
            QtyNode node = nodeEnumerator.Current;

            bool qtyIsUsable = true; //node.IsMatlEligible(a_simClock, a_sd, a_act, a_inv, a_matlAlloc, a_lotUsability);

            if (qtyIsUsable)
            {
                eligibleQty += node.UnallocatedQty;
                if (eligibleQty >= a_qty)
                {
                    return node;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Whether this profile has changed since it was last reset.
    /// </summary>
    private bool m_changedSinceReset = true;
    
    [ConditionalAttribute("TEST")]
    private void BreakOnConsume(ScenarioDetail a_sd, long a_simClock, Inventory a_inv, IMaterialAllocation a_matlAllocation, MRSupply a_mrSupply, AdjustmentArray a_adjustments, BaseIdObject a_reason)
    {
        InternalActivity act = a_reason as InternalActivity;

        if (act != null && act.Id == -2146346262)
        {
            int x = 0;
        }
    }

    /// <summary>
    /// When an activity has been scheduled that uses this inventory call this function to consume the material
    /// the overlap algorithm allocated to the activity.
    /// By the end of this function the only nodes remaining in the profile will be the ones that have unconsumed quantity.
    /// </summary>
    /// <param name="adjustments">Add any neessary entries to this set of view-only details.</param>
    /// <param name="reason"></param>
    internal bool Consume(ScenarioDetail a_sd, long a_simClock, Inventory a_inv, IMaterialAllocation a_matlAllocation, MRSupply a_mrSupply, AdjustmentArray a_adjustments, BaseIdObject a_reason)
    {
        //BreakOnConsume(a_sd, a_simClock, a_inv, a_matlAllocation, a_mrSupply, a_adjustments, a_reason);
        //// This temporary profile is used to build up a new profile of remaining quantity nodes;
        //// the nodes that still have unconsumed quantity.
        //TestStructure();
        ////QtyProfile<QtyNodeT> updateProfile = CreateProfile();
        //LinkedListNode<QtyProfileTimePoint<QtyNodeT>> startingNode = null;

        bool materialConsumed = false;

        //IEnumerator<QtyNodeT> etr = CreateDirectionalEnumerator(a_simClock, GetAllocationForInventory(a_inv, a_matlAllocation, null));
        //while (etr.MoveNext())
        //{
        //    QtyNodeT curNode = etr.Current;
        //    if (curNode.EligibleForAllocation)
        //    {
        //        if (curNode.AllocationCount > 0)
        //        {
        //            materialConsumed = true;
        //            bool expired = false; //curNode is QtyNode expirableNode && expirableNode.ExpirationTicks < a_simClock;
        //            curNode.AddAdjustments(a_mrSupply, a_adjustments, a_reason, expired);

        //            //Add subcomponent relation, except for Tools. Tools model a limited resource, not a subcomponent.
        //            if (a_inv.Item.ItemType != ItemDefs.itemTypes.Tool && curNode.Reason is InternalActivity iaReason && a_reason is InternalActivity addReason)
        //            {
        //                addReason.ManufacturingOrder.UsedAsSubComponent = true;
        //                addReason.ManufacturingOrder.AddSubComponentSupply(iaReason, addReason);
        //            }

        //            //curNode.Consume();
        //            if (curNode.m_remainingQty > 0)
        //            {
        //                startingNode = updateProfile.ConsumptionsAdd(startingNode, curNode);
        //            }
        //        }
        //        else
        //        {
        //            startingNode = updateProfile.ConsumptionsAdd(startingNode, curNode);
        //        }
        //    }
        //    else
        //    {
        //        // In this case the quantity is unaffected.
        //        startingNode = updateProfile.ConsumptionsAdd(startingNode, curNode);
        //    }
        //}

        ////while (top != null)
        ////{
        ////    nextTop = top.Next;
        ////    dangler = top;

        ////    while (dangler != null)
        ////    {
        ////        nextDangler = dangler.NextDangler;

        ////        if (dangler.EligibleForAllocation)
        ////        {
        ////            if (dangler.AllocationCount > 0)
        ////            {
        ////                materialConsumed = true;
        ////                dangler.AddAdjustments(a_mrSupply, a_adjustments, a_reason);

        ////                dangler.Consume();
        ////                if (dangler.m_remainingQty > 0)
        ////                {
        ////                    updateProfile.ConsumptionsAdd(dangler);
        ////                }
        ////            }
        ////            else
        ////            {
        ////                updateProfile.ConsumptionsAdd(dangler);
        ////            }
        ////        }
        ////        else
        ////        {
        ////            // In this case the quantity is unaffected.
        ////            updateProfile.ConsumptionsAdd(dangler);
        ////        }

        ////        dangler = nextDangler;
        ////    }

        ////    top = nextTop;
        ////}
        //TestStructure();

        //OverwriteWithProfile(updateProfile);
        //m_changedSinceReset = materialConsumed;

        return materialConsumed;
    }

    private void OverwriteWithProfile(QtyProfile<QtyNodeT> a_updateProfile)
    {
        Clear();
        //TODO: There might be a better way
        IEnumerator<QtyProfileTimePoint<QtyNodeT>> enumerator = a_updateProfile.m_nodes.GetEnumerator();
        while (enumerator.MoveNext())
        {
            m_nodes.AddLast(enumerator.Current);
        }

        TestStructure();
    }

    /// <summary>
    /// Create a copy of this quantity profile.
    /// Note the actual class type of the profile should be the same
    /// as the type being copied. Inherited classes should override this method
    /// to return the correct type.
    /// </summary>
    /// <returns></returns>
    internal virtual QtyProfile<QtyNodeT> CopyProfile() => throw new NotImplementedException();

    //TODO: Merge with AddToEnd since they are essentially the same
    /// <summary>
    /// A helper function of Consume(). Used to help build a new profile that only contains unconsumed QtyNodes.
    /// </summary>
    /// <param name="node"></param>
    internal LinkedListNode<QtyProfileTimePoint<QtyNodeT>> ConsumptionsAdd(LinkedListNode<QtyProfileTimePoint<QtyNodeT>> a_startingNode, QtyNodeT a_node)
    {
        LinkedListNode<QtyProfileTimePoint<QtyNodeT>> timePointNode = FindByTime(a_startingNode, a_node.Date);
        if (timePointNode != null)
        {
            timePointNode.Value.AddToEnd(a_node);
            return timePointNode;
        }

        QtyProfileTimePoint<QtyNodeT> newTimePoint = new (a_node, this);
        LinkedListNode<QtyProfileTimePoint<QtyNodeT>> insertTimePoint = InsertTimePoint(newTimePoint);

        TestIteration();
        TestStructure();

        return insertTimePoint;
    }

    private decimal SumNodeQtys(QtyProfileTimePointRange a_transferRange)
    {
        decimal qty = 0;
        using IEnumerator<QtyProfileTimePoint<QtyNodeT>> enumerator = a_transferRange.GetEnumerator();
        while (enumerator.MoveNext())
        {
            QtyProfileTimePoint<QtyNodeT> timePoint = enumerator.Current;
            using IEnumerator<QtyNode> nodeEnumerator = timePoint.GetEnumerator();
            while (nodeEnumerator.MoveNext())
            {
                QtyNode node = nodeEnumerator.Current;
                qty += node.m_currentQty;
            }
        }

        return qty;
    }

    /// <summary>
    /// Transfer the an entire profile into this profile.
    /// </summary>
    /// <param name="a_consume"></param>
    /// <returns></returns>
    internal decimal Transfer(QtyProfile<QtyNodeT> a_consume)
    {
        QtyProfileTimePointRange timePointRange = new (a_consume);
        return Transfer(a_consume, timePointRange);
    }

    /// <summary>
    /// Transfer everything in a profile up to and including QtyNodes up to a point in time.
    /// </summary>
    /// <param name="a_simClock">Everything node dated up to this point in time will be transferred from the tranferee profile to this profile.</param>
    /// <param name="a_transferee">The profile QtyNodes will be transferred from.</param>
    /// <returns>The quantity transferred to this QtyProfile.</returns>
    internal virtual decimal Transfer(long a_simClock, QtyProfile<QtyNodeT> a_transferee)
    {
        TestStructure();
        if (!a_transferee.HasNodes)
        {
            return 0m;
        }

        QtyProfileTimePointRange transferRange = new (a_transferee, a_simClock);
        //Note, we could also find the first and last node of m_nodes

        decimal transQty = Transfer(a_transferee, transferRange);

        TestStructure();
        return transQty;
    }

    /// <summary>
    /// Get an enumerator from the first node until the max date
    /// </summary>
    /// <param name="a_maxDate"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private IEnumerator<QtyProfileTimePoint<QtyNodeT>> GetRangeEnumerator(long a_maxDate)
    {
        LinkedListNode<QtyProfileTimePoint<QtyNodeT>> node = m_nodes.First;
        while (node != null)
        {
            if (node.Value.Date <= a_maxDate)
            {
                yield return node.Value;
            }

            node = node.Next;
        }
    }

    /// <summary>
    /// Transfer everything in a profile from the beginning up to a specified last QtyNode.Note all danglers of these nodes are also transferred.
    /// </summary>
    /// <param name="a_consumee">
    /// The QtyProfile whose nodes are being transferred to this QtyProfile. After the transfer, its First and Last
    /// QtyNodes will be different if they were transferred.
    /// </param>
    /// <param name="a_firstTopLvlTransNode">The first top level node to transfer.</param>
    /// <param name="a_lastTopLvlTransNode">The last top level node to transfer.</param>
    /// <returns></returns>
    public virtual decimal Transfer(QtyProfile<QtyNodeT> a_consumee, QtyProfileTimePointRange a_transferRange)
    {
        decimal transferQty = SumNodeQtys(a_transferRange);

        TransferRange(a_consumee, a_transferRange);

        return transferQty;
    }

    private void TransferRange(QtyProfile<QtyNodeT> a_transProfile, QtyProfileTimePointRange a_profileRange)
    {
        TestStructure();
        using IEnumerator<QtyProfileTimePoint<QtyNodeT>> rangeEnumerator = a_profileRange.GetEnumerator();
        while (rangeEnumerator.MoveNext())
        {
            QtyProfileTimePoint<QtyNodeT> transferTimePoint = rangeEnumerator.Current;

            InsertTimePoint(transferTimePoint);
            a_transProfile.FlagDisconnect(transferTimePoint);
        }

        TestStructure();
        a_transProfile.DisconnectNodes(); //Must call after enumeration to avoid changing the collection while enumerating

        TestIteration();
        TestStructure();
    }
    private ItemDefs.MaterialAllocation GetAllocationForInventory(Inventory a_inv, IMaterialAllocation a_matlAlloc, InventoryAllocation a_invAlloc)
    {
        ItemDefs.MaterialAllocation? materialAllocation = null;

        //TODO: Add extension point
        //if (a_sd.CustomizationInstances.MaterialAllocationCustomization != null)
        //{
        //    SchedulerDefinitions.ItemDefs.MaterialAllocation? matlAllocCustomization = a_sd.CustomizationInstances.MaterialAllocationCustomization.SequenceMaterialAllocationExecute(a_sd, a_simClock, a_matlAlloc, a_inv);
        //    if (matlAllocCustomization.HasValue)
        //    {
        //        materialAllocation = matlAllocCustomization.Value;
        //    }
        //}

        if (materialAllocation == null || materialAllocation == ItemDefs.MaterialAllocation.NotSet)
        {
            materialAllocation = a_inv.MaterialAllocation;
            if (a_matlAlloc != null)
            {
                if (a_matlAlloc.MaterialAllocation != ItemDefs.MaterialAllocation.NotSet)
                {
                    materialAllocation = a_matlAlloc.MaterialAllocation;
                }
            }
        }

        return materialAllocation.Value;
    }

    /// <summary>
    /// Return the correct type of enumeratorbased on how material shoould be allocated. Oldest first or newest first. This depends on customizations and Invetory settings.
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_simClock"></param>
    /// <param name="a_inv"></param>
    /// <param name="a_matlAlloc"></param>
    /// <param name="a_invAlloc"></param>
    /// <returns>A forward(Oldest first or reverse enumerator(Newest first)null if the profile is empty.</returns>
    internal IEnumerator<QtyNodeT> CreateDirectionalEnumerator(long a_simClock, ItemDefs.MaterialAllocation a_allocation)
    {
        IEnumerator<QtyNodeT> etr = null;

        switch (a_allocation)
        {
            case ItemDefs.MaterialAllocation.NotSet:
                etr = new QtyProfileEnumerator<QtyNodeT>(this);
                break;
            case ItemDefs.MaterialAllocation.UseOldestFirst:
                etr = new QtyProfileEnumerator<QtyNodeT>(this);
                break;
            case ItemDefs.MaterialAllocation.UseNewestFirst:
                QtyProfileTimePoint<QtyNodeT> timePoint = FindByTimeAtOrBefore(a_simClock);
                etr = new QtyProfileReverseEnumerator<QtyNodeT>(this, timePoint);
                break;

            #if DEBUG
            default:
                throw new Exception("Internal Error: unaccounted for SchedulerDefinitions.ItemDefs.MaterialAllocation in QtyProfile.AllocateAndConsumeOnHandAndExpected");
            #endif
        }

        return etr;
    }

    [ConditionalAttribute("LARRY")]
    private void BreakOnAllocateAndConsumeOnHandAndExpected(long a_simClock, BaseIdObject a_obj)
    {
        if (a_obj.Id == -2147227190 && a_simClock == 637239257811000001)
        {
            int xxx = 0;
        }
    }

    /// <summary>
    /// Consume the desired quantity. This function doesn't verify that you have enough material and will
    /// simply consume everything if there isn't enough.
    /// Material is consumed from the quantity profile from left to right, top to bottom. This is written for consuming
    /// available material.
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_simClock"></param>
    /// <param name="a_reason">The material requirer. The object the material is needed for such as an activity, sales order, or transfer order.</param>
    /// <param name="a_inv"></param>
    /// <param name="a_time"></param>
    /// <param name="a_qty"></param>
    /// <param name="a_matlReq">Can be null. The MR is only needed if the material requirer is an activity.</param>
    /// <param name="a_qtyProfileEnumerator"></param>
    /// <returns></returns>
    private decimal Allocate(ScenarioDetail a_sd, long a_simClock, BaseIdObject a_reason, Inventory a_inv, long a_time, decimal a_qty, IMaterialAllocation a_matlAlloc, ILotEligibility a_lotEligilbility, IEnumerator<QtyNode> a_qtyProfileEnumerator, bool a_inStockOnly, bool a_checkMatlEligibility = true)
    {
        // first attempt to only use EligibleLots
        decimal remainingQtyToAllocate = AllocateHelper(a_sd, a_simClock, a_reason, a_inv, a_time, a_qty, a_matlAlloc, a_lotEligilbility, a_qtyProfileEnumerator, a_inStockOnly, a_checkMatlEligibility);
        if (remainingQtyToAllocate > 0)
        {
            // If material limited to EligibleLots didn't suceed, try again allowing left over material to be used; lots with material unused after all material requirements that require that lot have consumed the material have been allocated and consumed.
            // The difference from the prior call above to allocateHelper is the final parameter. In this call, it is no longer limiting material eligibility to eligible lots, thereby allowing lots with unused material to be consumed.
            a_qtyProfileEnumerator.Reset();
            remainingQtyToAllocate = AllocateHelper(a_sd, a_simClock, a_reason, a_inv, a_time, remainingQtyToAllocate, a_matlAlloc, a_lotEligilbility, a_qtyProfileEnumerator, a_inStockOnly);
        }

        TestStructure();
        return remainingQtyToAllocate;
    }

    /// <summary>
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_simClock"></param>
    /// <param name="a_reason"></param>
    /// <param name="a_inv"></param>
    /// <param name="a_time"></param>
    /// <param name="a_qty"></param>
    /// <param name="a_lotEligilbility"></param>
    /// <param name="a_usability"></param>
    /// <param name="a_qtyProfileEnumerator"></param>
    /// <param name="m_initialTest">
    /// Whether this is the first of a 2 step test allocation. In the first attempt to allocate only material in the demands eligible lot code set is eligible to be used. In second attempt, material from
    /// eligible lots
    /// that have fulfilled all the demands that require them can also be used to fulfill the demand.
    /// </param>
    /// <returns></returns>
    private decimal AllocateHelper(ScenarioDetail a_sd, long a_simClock, BaseIdObject a_reason, Inventory a_inv, long a_time, decimal a_qty, IMaterialAllocation a_matlAlloc, ILotEligibility a_lotEligilbility, IEnumerator<QtyNode> a_qtyProfileEnumerator, bool a_inStockOnly, bool a_checkMatlEligibility = true)
    {
        decimal remainingNeededQty = a_qty;

        // Iterate across the top of the set of connected QtyNOdes.
        while (a_qtyProfileEnumerator.MoveNext())
        {
            QtyNode curNode = a_qtyProfileEnumerator.Current;

            // Allocate the QtyNodes available at this time from the current node down the set of available nodes.
            AllocateQtyNode(a_sd, a_simClock, a_reason, a_inv, curNode, a_time, a_matlAlloc, a_lotEligilbility, ref remainingNeededQty, a_inStockOnly, a_checkMatlEligibility);

            MathStatics.CloseToZeroAdjuster000001(ref remainingNeededQty);
            if (remainingNeededQty <= 0)
            {
                break;
            }
        }

        return remainingNeededQty;
    }

    private void AllocateQtyNode(ScenarioDetail a_sd, long a_simClock, BaseIdObject a_reason, Inventory a_inv, QtyNode a_top, long a_time, IMaterialAllocation a_matlAlloc, ILotEligibility a_lotEligibility, ref decimal r_remainingNeededQty, bool a_inStockOnly, bool a_checkMatlEligibility = true)
    {
        decimal qtyOrig = r_remainingNeededQty;
        AllocateQtyNodeHelper(a_sd, a_simClock, a_reason, a_inv, a_top, a_time, a_matlAlloc, a_lotEligibility, ref r_remainingNeededQty, a_inStockOnly, a_checkMatlEligibility);
        if (a_sd.ScenarioOptions.IsApproximatelyZero(r_remainingNeededQty))
        {
            r_remainingNeededQty = 0;
        }
    }

    private void AllocateQtyNodeHelper(ScenarioDetail a_sd, long a_simClock, BaseIdObject a_reason, Inventory a_inv, QtyNode a_nodeToAllocate, long a_time, IMaterialAllocation a_matlAlloc, ILotEligibility a_lotEligibility, ref decimal r_remainingNeededQty, bool a_inStockOnly, bool a_checkMatlEligibility = true)
    {
        long matlUseDate = a_time;

        //Used to keep track of which overlap cycle time to use. 
        int matlCycleIdx = 0;

        // Find the time the allocation would be for.
        if (a_reason is InternalActivity act)
        {
            if (act.m_overlapCycleTimes != null && act.m_overlapCycleTimes.Length > 0)
            {
                for (int i = matlCycleIdx; i < act.m_overlapCycleTimes.Length; ++i)
                {
                    if (a_nodeToAllocate.Date < act.m_overlapCycleTimes[i])
                    {
                        // The material is available prior to this cycle time. We Use this as the cycle the material is for. 
                        matlCycleIdx = i;
                        matlUseDate = act.m_overlapCycleTimes[i];
                    }
                }
            }
        }

        bool eligibleMatl = true;

        if (a_checkMatlEligibility)
        {
            //eligibleMatl = a_nodeToAllocate.IsMatlEligible(a_simClock, a_sd, a_reason, a_inv, a_matlAlloc, a_lotEligibility);
        }

        if (eligibleMatl)
        {
            if (a_inStockOnly)
            {
                // In this case, the material must already be on-hand.
                eligibleMatl = a_nodeToAllocate.Reason is Inventory;
            }
        }

        if (eligibleMatl)
        {
            if (a_nodeToAllocate.UnallocatedQty > r_remainingNeededQty)
            {
                // More material is available than is needed. 
                //a_nodeToAllocate.Allocate(r_remainingNeededQty, a_time, a_inv);
                //TODO: Get all of the node's allocated connectors and flag them as in use (for this source). We need to track source because there could be one source using multiple transfers

                r_remainingNeededQty = 0;
                return;
            }

            if (a_nodeToAllocate.UnallocatedQty == r_remainingNeededQty)
            {
                // The amount of material available in this node is equal to the amount needed. 
                r_remainingNeededQty = 0;
                a_nodeToAllocate.AllocateAll(a_time, a_inv);
                return;
            }

            // There  is less material available than is needed. 
            if (a_nodeToAllocate.UnallocatedQty > 0)
            {
                r_remainingNeededQty -= a_nodeToAllocate.UnallocatedQty;
                a_nodeToAllocate.AllocateAll(a_time, a_inv);
            }
        }
    }

    /// <summary>
    /// This function assumes there are no danglers. This is the case for a profile just created for an activity.
    /// Add an adjustment for every element in this profile.
    /// </summary>
    /// <param name="a_materialSource">The source of the material, but not necessarily the reason. For example the source could be a Product while the reason is the activity</param>
    //internal void AddAdjustments(Lot a_lot, BaseIdObject a_reason, BaseIdObject a_materialSource, StorageArea a_storageArea)
    //{
    //    foreach (QtyNode qtyNode in this)
    //    {
    //        Adjustment productAdjustment = new Adjustment(qtyNode.Date, a_reason, qtyNode.UsedQty, a_materialSource, a_storageArea, a_lot);
    //        a_lot.AddAdjustment(productAdjustment);
    //    }

    //    TestStructure();
    //}

    #region DEBUG. A lot of these methods are useless because they were written before addition of the danglers.
    /// <summary>
    /// DEBUG. Create an array of all the nodes.
    /// </summary>
    private QtyNode[] CreateQtyNodeArray()
    {
        List<QtyNode> list = new ();
        using IEnumerator<QtyNode> enumerator = GetEnumerator();
        while (enumerator.MoveNext())
        {
            list.Add(enumerator.Current);
        }

        return list.ToArray();
    }

    /// <summary>
    /// Calculate the quantity up to a point in time.
    /// </summary>
    /// <param name="a_ticks">Calculate the quantity up to this point in time.</param>
    /// <returns></returns>
    internal decimal CalculateQty(long a_ticks = long.MaxValue)
    {
        decimal qty = 0;

        // This version of CalculateQty must use the forward iterator
        QtyProfileEnumerator<QtyNodeT> etr = new (this);
        while (etr.MoveNext())
        {
            QtyNode node = etr.Current;
            if (node.Date <= a_ticks)
            {
                qty += node.m_currentQty;
            }
        }

        return qty;
    }

    /// <summary>
    /// DEBUG. Qty summed across all the nodes.
    /// </summary>
    internal decimal Qty => CalculateQty();

    private decimal CalculateUnallocatedQty()
    {
        decimal qty = 0;
        QtyProfileEnumerator<QtyNodeT> etr = new (this);
        while (etr.MoveNext())
        {
            QtyNode node = etr.Current;
            qty += node.UnallocatedQty;
        }

        return qty;
    }

    private decimal UnallocatedQty => CalculateUnallocatedQty();

    /// <summary>
    /// DEBUG. The number of top level dangler lists.
    /// </summary>
    internal long Count()
    {
        long count = 0;
        QtyProfileEnumerator<QtyNodeT> etr = new (this);
        while (etr.MoveNext())
        {
            ++count;
        }

        return count;
    }

    private QtyNode[] NodesArray => CreateQtyNodeArray();

    /// <summary>
    /// For debugging purposes. But currently it's useless because it was written before the addition of danglers.
    /// </summary>
    private string[] DescriptionsOfUnallocated
    {
        get
        {
            QtyNode[] nodes = CreateQtyNodeArray();
            string[] descriptions = new string[nodes.Length];

            for (int nodeI = 0; nodeI < nodes.Length; ++nodeI)
            {
                QtyNode node = nodes[nodeI];
                descriptions[nodeI] = string.Format("unallocatedQty:{0};node {1}", node.UnallocatedQty, node);
            }

            return descriptions;
        }
    }

    /// <summary>
    /// For debugging purposes. Shows the quantity in the profile and the number of top level QtyNodes.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        StringBuilder sb = new ();
        sb.AppendFormat("QtyProfile: Qty={0}, NodesArray.Length={1}", Qty, NodesArray.Length);

        return sb.ToString();
    }

    /// <summary>
    /// Enumerate the nodes of the profile.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<QtyNodeT> GetEnumerator()
    {
        return new QtyProfileEnumerator<QtyNodeT>(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion

    #if DEBUG
    private void VerifyMatching() { }

    private string[] DescriptionsReversed
    {
        get
        {
            string[] descriptions = new string[Count()];
            int idx = 0;
            QtyProfileReverseEnumerator<QtyNodeT> reverseEnumerator = new (this);
            while (reverseEnumerator.MoveNext())
            {
                QtyNode node = reverseEnumerator.Current;
                descriptions[idx++] = string.Format("unallocatedQty:{0}; node[1]", node.UnallocatedQty, node);
            }

            return descriptions;
        }
    }
    #endif

    /// <summary>
    /// Get the first node of the QtyProfile.
    /// </summary>
    /// <returns>The first node or null if there are no nodes.</returns>
    internal QtyNode GetFirstNode()
    {
        return m_nodes.First?.Value?.QtyNodes?.First?.Value;
    }

    /// <summary>
    /// Check whether the current quantity exceeds a limit by a certain time.
    /// </summary>
    /// <param name="a_limit"></param>
    /// <param name="a_dateLimit"></param>
    /// <returns></returns>
    internal bool ValidateDisposalQty(decimal a_limit, long a_dateLimit)
    {
        decimal currentQty = 0m;
        using IEnumerator<QtyNode> enumerator = GetEnumerator();
        while (enumerator.MoveNext())
        {
            QtyNode current = enumerator.Current;
            if (current.Date > a_dateLimit)
            {
                break;
            }

            if (current.UnallocatedQty < current.m_currentQty)
            {
                //This node has future adjustments planned, so don't dispose yet
                return false;
            }
            
            currentQty += current.m_currentQty;

            if (currentQty > a_limit)
            {
                return false;
            }
        }

        return currentQty != 0m;
    }

    internal List<(decimal, QtyNode)> DisposeAllAtOrBefore(long a_disposalEndTicks)
    {
        List<(decimal, QtyNode)> disposedNodes = new ();
        using IEnumerator<QtyNode> enumerator = GetEnumerator();
        while (enumerator.MoveNext())
        {
            QtyNode current = enumerator.Current;
            if (current.Date > a_disposalEndTicks)
            {
                break;
            }

            if (current.m_currentQty > 0m)
            {
                disposedNodes.Add((current.m_currentQty, current));
                current.Consume(current.m_currentQty, true, PTDateTime.InvalidDateTimeTicks);
            }
        }

        return disposedNodes;
    }
}

// TODO LOT: Move to separate files.
internal partial class QtyProfile<QtyNodeT>
{
    internal class QtyProfileTimePointRange : IEnumerable<QtyProfileTimePoint<QtyNodeT>>
    {
        internal QtyProfileTimePointRange(QtyProfile<QtyNodeT> a_profile)
        {
            m_profile = a_profile;
            if (a_profile.HasNodes)
            {
                m_startDate = a_profile.First.Date;
                m_endDate = a_profile.Last.Date;
            }
        }

        internal QtyProfileTimePointRange(QtyProfile<QtyNodeT> a_profile, long a_maxDate)
        {
            m_profile = a_profile;
            if (a_profile.HasNodes)
            {
                m_startDate = a_profile.First.Date;
            }

            m_endDate = a_maxDate;
        }

        internal QtyProfileTimePointRange(QtyProfile<QtyNodeT> a_profile, long a_startDate, long a_maxDate)
        {
            m_profile = a_profile;
            m_startDate = a_startDate;
            m_endDate = a_maxDate;
        }

        private readonly QtyProfile<QtyNodeT> m_profile;
        private readonly long m_startDate;
        private readonly long m_endDate;

        public QtyProfileTimePoint<QtyNodeT> FirstForwardNode()
        {
            IEnumerator<QtyProfileTimePoint<QtyNodeT>> enumerator = GetEnumerator();
            if (enumerator.MoveNext())
            {
                return enumerator.Current;
            }

            return null;
        }

        public IEnumerator<QtyProfileTimePoint<QtyNodeT>> GetEnumerator()
        {
            using IEnumerator<QtyProfileTimePoint<QtyNodeT>> enumerator = m_profile.m_nodes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                QtyProfileTimePoint<QtyNodeT> timePoint = enumerator.Current;
                if (timePoint.Date > m_endDate)
                {
                    break;
                }

                if (timePoint.Date >= m_startDate)
                {
                    yield return timePoint;
                }
            }
        }

        public IEnumerator<QtyProfileTimePoint<QtyNodeT>> GetReverseEnumerator()
        {
            LinkedListNode<QtyProfileTimePoint<QtyNodeT>> currentNode = m_profile.m_nodes.Last;
            while (currentNode != null)
            {
                if (currentNode.Value.Date < m_startDate)
                {
                    //We went before the start of the range, we are done
                    break;
                }

                //Only return nodes before the end date of the range
                if (currentNode.Value.Date <= m_endDate)
                {
                    yield return currentNode.Value;
                }

                currentNode = currentNode.Previous;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class QtyProfileEnumerator<QtyNodeT> : IEnumerator<QtyNodeT> where QtyNodeT : QtyNode
    {
        private readonly LinkedList<QtyProfileTimePoint<QtyNodeT>> m_timePoints;
        private LinkedListNode<QtyProfileTimePoint<QtyNodeT>> m_currentTpNode;
        private LinkedListNode<QtyNodeT> m_currentNode;
        private bool m_skipMoveNext;
        public bool HasNodes => m_timePoints != null;

        public QtyProfileEnumerator(QtyProfile<QtyNodeT> a_profile)
        {
            m_timePoints = a_profile.m_nodes ?? throw new ArgumentNullException(nameof(a_profile.m_nodes));
            m_currentTpNode = m_timePoints.First;
            m_currentNode = m_currentTpNode?.Value.QtyNodes.First;
            m_skipMoveNext = true;
        }

        public QtyNodeT Current
        {
            get
            {
                if (m_currentNode == null)
                {
                    throw new InvalidOperationException();
                }

                return m_currentNode.Value;
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            // Dispose resources if needed
        }

        public bool MoveNext()
        {
            if (m_currentNode == null)
            {
                //We have reached the end
                return false;
            }

            if (m_skipMoveNext)
            {
                m_skipMoveNext = false;
                return true;
            }

            // Move to the next QtyNode within the current TimePoint
            m_currentNode = m_currentNode.Next;

            // If there are no more QtyNodes in the current TimePoint, move to the next TimePoint
            while (m_currentNode == null && m_currentTpNode != null)
            {
                m_currentTpNode = m_currentTpNode.Next;
                if (m_currentTpNode != null)
                {
                    m_currentNode = m_currentTpNode.Value.QtyNodes.First;
                }
            }

            return m_currentNode != null;
        }

        // Resets the enumerator to the beginning
        public void Reset()
        {
            if (HasNodes)
            {
                m_currentTpNode = m_timePoints.First;
                m_currentNode = m_currentTpNode?.Value.QtyNodes.First;
                m_skipMoveNext = true;
            }
        }
    }

    public class QtyProfileReverseEnumerator<T> : IEnumerator<T> where T : QtyNode
    {
        // LinkedList of TimePoint objects to enumerate
        private readonly LinkedList<QtyProfileTimePoint<T>> m_timePoints;

        // Current TimePoint node being enumerated
        private LinkedListNode<QtyProfileTimePoint<T>> m_currentTpNode;

        // Current QtyNode within the current TimePoint being enumerated
        private LinkedListNode<T> m_currentNode;

        private bool m_skipMoveNext;

        public bool HasNodes => m_timePoints != null;

        /// <summary>
        /// Enumerate all QtyNodes in reverse
        /// </summary>
        /// <param name="a_timePoints"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public QtyProfileReverseEnumerator(QtyProfile<T> a_profile)
        {
            m_timePoints = a_profile.m_nodes ?? throw new ArgumentNullException(nameof(a_profile.m_nodes));
            m_currentTpNode = m_timePoints.Last;
            m_currentNode = m_currentTpNode?.Value.QtyNodes.Last;
            m_skipMoveNext = true;
        }

        /// <summary>
        /// Enumerate QtyNodes in reverse, starting with the provided node
        /// </summary>
        /// <param name="a_timePoints"></param>
        /// <param name="a_firstNode"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public QtyProfileReverseEnumerator(QtyProfile<T> a_profile, QtyProfileTimePoint<T> a_firstTimePoint)
        {
            if (a_firstTimePoint == null)
            {
                //This collection is empty
                m_currentNode = null;
                return;
            }

            m_timePoints = a_profile.m_nodes;

            //We need to find the starting node
            if (a_firstTimePoint == null || !a_firstTimePoint.HasNodes)
            {
                throw new ArgumentNullException(nameof(a_firstTimePoint));
            }

            m_currentTpNode = a_profile.m_nodes.Find(a_firstTimePoint);
            m_currentNode = m_currentTpNode.Value.GetQtyNodeLLNode(a_firstTimePoint.LastQtyNode); //Start at the last qtyNode in the time point
            m_skipMoveNext = true;
        }

        // Returns the current Node being enumerated
        public T Current
        {
            get
            {
                if (m_currentNode == null)
                {
                    throw new InvalidOperationException();
                }

                return m_currentNode.Value;
            }
        }

        object IEnumerator.Current => Current;

        // Dispose any resources if needed
        public void Dispose()
        {
            // Dispose resources if needed
        }

        // Moves to the previous QtyNode in the enumeration
        public bool MoveNext()
        {
            if (m_currentNode == null)
            {
                //We have reached the end
                return false;
            }

            if (m_skipMoveNext)
            {
                m_skipMoveNext = false;
                return true;
            }

            // Move to the previous QtyNode within the current TimePoint
            m_currentNode = m_currentNode.Previous;

            // If there are no more QtyNodes in the current TimePoint, move to the previous TimePoint
            while (m_currentNode == null && m_currentTpNode != null)
            {
                m_currentTpNode = m_currentTpNode.Previous;
                if (m_currentTpNode != null)
                {
                    m_currentNode = m_currentTpNode.Value.QtyNodes.Last;
                }
            }

            return m_currentNode != null;
        }

        // Resets the enumerator to the end
        public void Reset()
        {
            if (HasNodes)
            {
                m_currentTpNode = m_timePoints.Last;
                m_currentNode = m_currentTpNode?.Value.QtyNodes.Last;
                m_skipMoveNext = true;
            }
        }
    }

    /// <summary>
    /// Use this function instead of new to create a QtyNode.
    /// This was created to allow different type of profiles to create the right kind of node.
    /// </summary>
    /// <param name="a_node"></param>
    /// <returns></returns>
    internal virtual QtyNodeT CreateQtyNode(QtyNodeT a_node) => throw new NotImplementedException();

    ///// <summary>
    ///// Use this function instead of new to create a QtyNode.
    ///// This was created to allow different type of profiles to create the right kind of node.
    ///// </summary>
    ///// <param name="a_time"></param>
    ///// <param name="a_toReason"></param>
    ///// <param name="a_qty"></param>
    ///// <returns></returns>
    //internal virtual QtyNode CreateQtyNode(long a_time, MaterialRequirement a_mrReason, decimal a_qty)
    //{
    //    QtyNode q = new (a_time, a_mrReason, a_qty);
    //    return q;
    //}

    internal virtual void Allocate(decimal a_allocationQty, QtyNode a_supplyNode, QtyNode a_demandNode)
    {
        a_supplyNode.Allocate(a_allocationQty, 0);
        a_demandNode.Allocate(a_allocationQty, 0);
    }

    internal virtual void ResetForAllocation()
    {
        foreach (QtyNodeT qtyNode in this)
        {
            qtyNode.ResetForAllocation();
        }
    }
}