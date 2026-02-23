using System.Collections;

using PT.APSCommon;
using PT.Common.Debugging;
using PT.Common.Exceptions;

namespace PT.Scheduler;

/// <summary>
/// ListTemplate
/// </summary>
public class ResourceCapacityIntervalList : LinkedList<ResourceCapacityInterval>,  IPTSerializable
{
    #region IPTSerializable Members
    public ResourceCapacityIntervalList(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out int count);

            for (int i = 0; i < count; i++)
            {
                ResourceCapacityInterval node = new ResourceCapacityInterval(a_reader);
                AddLast(node);
            }
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(Count);
        foreach (ResourceCapacityInterval interval in this)
        {
            interval.Serialize(a_writer);
        }
    }

    public const int UNIQUE_ID = 326;

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public ResourceCapacityIntervalList() { }
    
    /// <summary>
    /// Add element to the front of the list.
    /// </summary>
    /// <param name="data">The element to add to the list.</param>
    /// <returns>The node the element is stored in.</returns>
    public ResourceCapacityInterval AddFront(ResourceCapacityInterval a_data)
    {
        //TODO: replace reference and delete
        return null;
    }
    
    public class ListException : PTException
    {
        public ListException(string msg)
            : base(msg) { }
    }

    public class ListsDontMatchException : ListException
    {
        internal ListsDontMatchException()
            : base("The lists don't match in ResourceCapacityIntervalList.") { }
    }

    public ArrayList CreateArrayListShallowCopy()
    {
        ArrayList al = new ();

        foreach (ResourceCapacityInterval interval in this)
        {
            al.Add(interval);
        }

        return al;
    }

    /// <summary>
    /// Return the interval that contains the start date. It is assumed that an interval will be found.
    /// This was designed for use with simulation. If used outside of it, it might crash (depending on the second parameter).
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="start">This value can be null. If you pass in a value it must be on or before the date your searching for otherwise it will crash.</param>
    /// <returns></returns>
    internal LinkedListNode<ResourceCapacityInterval> FindForwards(long a_dt, LinkedListNode<ResourceCapacityInterval> a_start)
    {
        LinkedListNode<ResourceCapacityInterval> rciListNode;
        ResourceCapacityInterval rci;

        if (a_start == null)
        {
            rciListNode = First;
            if (rciListNode == null)
            {
                #if TEST
                    throw new PTException("This resource's capacity has not been initialized");
                #endif
                return null;
            }
        }
        else
        {
            rciListNode = a_start;

            // Find the first interval that contains the earliest start
            // time.
            #if DEBUG
            if (a_dt < rciListNode.Value.StartDate)
            {
                throw new PTException("Resource capacity interval bookkeeping error.");
            }
            #endif
        }

        rci = rciListNode.Value;
        
        // Pass forwards through nodes until one is found that contains
        // the earliest start date.
        // We assume the node is always found because of the way we define capacity intervals.
        while (!rci.Contains(a_dt))
        {
            if (rciListNode.Next == null)
            {
                bool b = false;
            }

            rciListNode = rciListNode.Next;

            rci = rciListNode.Value;
            if (rciListNode.Value.IsPastPlanningHorizon)
            {
                return rciListNode;
            }
        }

        return rciListNode;
    }

    /// <summary>
    /// Return the interval that contains the start date. It is assumed that an interval will be found.
    /// This was designed for use with simulation. If used outside of it, it might crash (depending on the second parameter).
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="start">This value can be null. If you pass in a value it must be on or before the date your searching for otherwise it will crash.</param>
    /// <returns></returns>
    internal LinkedListNode<ResourceCapacityInterval> FindBackwards(long a_dt, LinkedListNode<ResourceCapacityInterval> a_start)
    {
        #if DEBUG
        PTDateTime.ValidateDateTime(a_dt);
        #endif

        LinkedListNode<ResourceCapacityInterval> rciListNode;

        if (a_start == null)
        {
            rciListNode = Last;
            if (rciListNode == null)
            {
                return null;
            }
        }
        else
        {
            rciListNode = a_start;
        }

        ResourceCapacityInterval rci = rciListNode.Value;

        // Find the first interval that contains the earliest start
        // time.
        #if DEBUG
        if (a_dt >= rci.EndDate)
        {
            throw new PTException("Resource capacity interval bookkeeping error.");
        }
        #endif

        // Pass backwards through nodes until one is found that contains
        // the earliest start date.
        // We assume the node is always found because of the way we define capacity intervals.
        while (!rci.Contains(a_dt))
        {
            if (rciListNode.Previous == null)
            {
                return null;
            }
            rciListNode = rciListNode.Previous;
            rci = rciListNode.Value;
        }

        return rciListNode;
    }

    /// <summary>
    /// Return the interval that contains the start date. It is assumed that an interval will be found.
    /// This was designed for use with simulation. If used outside of it, it might crash (depending on the second parameter).
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="start">This value can be null.</param>
    /// <returns></returns>
    internal LinkedListNode<ResourceCapacityInterval> Find(long a_dt, LinkedListNode<ResourceCapacityInterval> a_start)
    {
        if (a_start == null)
        {
            return FindForwards(a_dt, null);
        }

        ResourceCapacityInterval.ContainmentType ct = a_start.Value.ContainsStartPoint(a_dt);
        if (ct == ResourceCapacityInterval.ContainmentType.Contains)
        {
            return a_start;
        }

        if (ct == ResourceCapacityInterval.ContainmentType.GreaterThan)
        {
            return FindForwards(a_dt, a_start);
        }

        // LessThan
        return FindBackwards(a_dt, a_start);
    }

    /// <summary>
    /// Finds the first online period on or after a date.
    /// </summary>
    /// <param name="dt">The first online period on or after this date.</param>
    /// <param name="start">If you specify null, the first position in the list is used as the start of the search.</param>
    /// <returns>The interval or null. </returns>
    internal LinkedListNode<ResourceCapacityInterval> FindFirstOnline(long a_dt, LinkedListNode<ResourceCapacityInterval> a_start)
    {
        LinkedListNode<ResourceCapacityInterval> rciListNode = FindForwards(a_dt, a_start);
        ResourceCapacityInterval rci = rciListNode.Value;

        // Find the first online interval that contains earliest start time.
        while (!rci.Active)
        {
            rciListNode = rciListNode.Next;
            rci = rciListNode.Value;
        }

        return rciListNode;
    }

    /// <summary>
    /// Find the first online node from a starting node.
    /// </summary>
    /// <param name="a_startNode"></param>
    /// <returns></returns>
    internal static LinkedListNode<ResourceCapacityInterval> FindFirstOnlineBackwards(LinkedListNode<ResourceCapacityInterval> a_startNode)
    {
        LinkedListNode<ResourceCapacityInterval> cur = a_startNode;
        while (cur != null && !cur.Value.Active)
        {
            cur = cur.Previous;
        }

        return cur;
    }

    /// <summary>
    /// Find the next online node from a starting node.
    /// </summary>
    /// <param name="a_startNode"></param>
    /// <returns></returns>
    internal static LinkedListNode<ResourceCapacityInterval> FindNextOnline(LinkedListNode<ResourceCapacityInterval> a_startNode)
    {
        LinkedListNode<ResourceCapacityInterval> cur = a_startNode.Next;
        while (cur != null && !cur.Value.Active)
        {
            cur = cur.Next;
        }

        return cur;
    }

    /// <summary>
    /// Find the first online node from a starting node.
    /// </summary>
    /// <param name="a_startNode"></param>
    /// <returns></returns>
    internal static LinkedListNode<ResourceCapacityInterval> FindFirstOnlineBackwardsContainingPoint(LinkedListNode<ResourceCapacityInterval> a_startNode, long a_point, bool a_haveEndPointStartAtNextNode)
    {
        if (a_startNode.Value.Contains(a_point))
        {
            return a_startNode;
        }

        LinkedListNode<ResourceCapacityInterval> cur = a_startNode.Previous;
        LinkedListNode<ResourceCapacityInterval> lastChecked = a_startNode;

        while (cur != null)
        {
            //Ending at the point counts for contains here, because a previous node didn't start at that date.
            if (cur.Value.Active && cur.Value.Contains(a_point))
            {
                return cur;
            }

            if (a_haveEndPointStartAtNextNode && cur.Value.EndDate == a_point)
            {
                //This is the interval where the point ends, but in this case we actually want the start of the next.
                //For example, if a_point is a process segment end, the next one would really start at the next interval.
                return lastChecked;
            }

            lastChecked = cur;
            cur = cur.Previous;
        }

        //Couldn't find the previous interval
        return null;
    }

    /// <summary>
    /// Find the first online starting after a_point that is before a_startNode
    /// </summary>
    /// <param name="a_startNode"></param>
    /// <returns></returns>
    internal static LinkedListNode<ResourceCapacityInterval> FindFirstOnlineBackwardsAfterPoint(LinkedListNode<ResourceCapacityInterval> a_startNode, long a_point)
    {
        if (a_startNode.Value.Contains(a_point))
        {
            return a_startNode;
        }

        LinkedListNode<ResourceCapacityInterval> previous = a_startNode;
        LinkedListNode<ResourceCapacityInterval> cur = a_startNode;

        while (cur != null)
        {
            if (cur.Value.Active)
            {
                if (cur.Value.StartDate > a_point)
                {
                    previous = cur;
                }
                else
                {
                    return previous;
                }
            }

            cur = cur.Previous;
        }

        //Couldn't find the previous interval
        return null;
    }

    public override string ToString()
    {
        return "Count=" + Count;
    }

    /// <summary>
    /// Find the online capacity between two dates.
    /// </summary>
    /// <param name="a_startTicks"></param>
    /// <param name="a_endTicks"></param>
    /// <returns></returns>
    internal long FindOnlineCapacityBetweenTwoDates(long a_startTicks, long a_endTicks)
    {
        if (a_startTicks >= a_endTicks)
        {
            //Bad data
            return 0;
        }

        LinkedListNode<ResourceCapacityInterval> onlineNode = Find(a_startTicks, null);
        if (onlineNode.Value.Contains(a_endTicks))
        {
            //easy case, same interval
            return a_endTicks - a_startTicks;
        }

        //Online time from first interval
        long onlineCapacity =  onlineNode.Value.EndDate - long.Max(onlineNode.Value.StartDate, a_startTicks);

        //Add up online capacity from subsequent online intervals until we reach the end date
        LinkedListNode<ResourceCapacityInterval> nextNode = FindNextOnline(onlineNode);
        while (nextNode?.Value?.StartDate < a_endTicks)
        {
            if (nextNode.Value.EndDate > a_endTicks)
            {
                onlineCapacity += a_endTicks - nextNode.Value.StartDate;
                break;
            }

            onlineCapacity += nextNode.Value.EndDate - nextNode.Value.StartDate;
            nextNode = FindNextOnline(nextNode);
        }

        return onlineCapacity;
    }

    /// <summary>
    /// Determine whether there is a gap of Online Time between the End Date of an Activity and the Start Date of the next Activity.
    /// This function assumes the dates are contained in an Active RCI
    /// </summary>
    /// <param name="a_previousActEndTicks"></param>
    /// <param name="a_nextActStartDate"></param>
    /// <returns></returns>
    internal bool IsThereGapOfOnlineTimeBetweenEndDateAndStartDateOfSuccessiveActivities(long a_previousActEndTicks, long a_nextActStartDate)
    {
        if (a_previousActEndTicks == a_nextActStartDate)
        {
            //Same Date
            return false;
        }

        LinkedListNode<ResourceCapacityInterval> leftNode = Find(a_previousActEndTicks, null);
        ResourceCapacityInterval leftRci = leftNode.Value;

        LinkedListNode<ResourceCapacityInterval> rightNode = Find(a_nextActStartDate, null);
        ResourceCapacityInterval rightRci = rightNode.Value;

#if DEBUG
        if (!leftRci.Active || !rightRci.Active)
        {
            DebugException.ThrowInDebug("EndDate of previous Activity or Start Date of next Activity was incorrectly calculated to be in an offline interval");
        }
#endif

        if (leftRci == rightRci)
        {
            //Same Rci but we already determined the date time ticks are different, so there must be a gap
            return true;
        }
        else if (a_previousActEndTicks < leftRci.EndDate)
        {
            //Not on the same interval but the Left Act ends before the end of the RCI so there must be a gap
            return true;
        }
        else if (leftNode.Next == rightNode && a_nextActStartDate > rightRci.StartDate)
        {
            //Left Act ends at the end of its RCI; next Act starts on the next RCI but not on its start date so
            //there must be a gap
            return true;
        }
        else if (FindNextOnline(leftNode) != null)
        {
            //Dates are in separate intervals; the next online RCI is not where the next Act starts;
            //There is another online interval between the RCIs so there must be a gap
            return true;
        }

        return false;
    }

    /// <summary>
    /// Given two points: are they and all the points between them in online intervals.
    /// </summary>
    /// <param name="startPointDate"></param>
    /// <param name="endPointDate"></param>
    /// <returns></returns>
    internal bool ArePointsWithinAContinuousOnlineInterval(long a_startPointDate, long a_endPointDate)
    {
        #if DEBUG
        if (a_startPointDate > a_endPointDate)
        {
            throw new PTException("The start point was greater than the end point.");
        }
        #endif
        LinkedListNode<ResourceCapacityInterval> node = Find(a_startPointDate, null);
        ResourceCapacityInterval rci = node.Value;

        do
        {
            if (!rci.Active)
            {
                return false;
            }

            if (rci.Contains(a_endPointDate))
            {
                return true;
            }

            node = node.Next;
        } while (node != null);

        #if DEBUG
        throw new PTException("The end interval wasn't found.");
        #else
            return false;
        #endif
    }

    /// <summary>
    /// Finds the first online interval
    /// </summary>
    /// <returns>The interval or null. </returns>
    internal LinkedListNode<ResourceCapacityInterval> FirstOnline()
    {
        if (First == null)
        {
            return null;
        }

        LinkedListNode<ResourceCapacityInterval> rciListNode = First;
        ResourceCapacityInterval rci = rciListNode.Value;

        // Find the first online interval that contains earliest start time.
        while (!rci.Active)
        {
            rciListNode = rciListNode.Next;
            rci = rciListNode.Value;
        }

        return rciListNode;
    }
}