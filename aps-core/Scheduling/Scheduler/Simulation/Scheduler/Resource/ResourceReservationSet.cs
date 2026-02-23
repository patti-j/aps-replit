using System.Collections;

using PT.Common.PTMath;

namespace PT.Scheduler;

public class ResourceReservationSet : IEnumerable<ResourceReservation>
{
    // LinkedList selected over List because deletions to the start of the list are common and
    // the operation on a List is expensive at O(n).
    public LinkedList<ResourceReservation> m_reservations = new ();

    public int Count => m_reservations.Count;

    internal void Clear()
    {
        m_reservations.Clear();
    }

    internal void Add(ResourceReservation a_resRv)
    {
        m_reservations.AddLast(a_resRv);
    }

    internal void RemoveFirst()
    {
        m_reservations.RemoveFirst();
    }

    public IEnumerator<ResourceReservation> GetEnumerator()
    {
        return m_reservations.GetEnumerator();
    }

    /// <summary>
    /// Get the first node in this set of ResourceReservations.
    /// </summary>
    /// <returns>A generic LinkedListNode.</returns>
    internal LinkedListNode<ResourceReservation> GetFirstNode()
    {
        return m_reservations.First;
    }

    /// <summary>
    /// Remove a ResourceReservation from the set.
    /// </summary>
    /// <param name="a_node">The node of the ResourceReservation to remove.</param>
    internal void Remove(LinkedListNode<ResourceReservation> a_node)
    {
        m_reservations.Remove(a_node);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    internal ResourceReservation First => m_reservations.First.Value;

    internal ResourceReservation Last => m_reservations.Last.Value;

    public override string ToString()
    {
        string s;
        if (m_reservations.First != null)
        {
            string startDate = DateTimeHelper.ToLocalTimeFromUTCTicks(m_reservations.First.Value.StartTicks).ToString();
            string endDate = DateTimeHelper.ToLocalTimeFromUTCTicks(m_reservations.Max(r => r.EndTicks)).ToString();
            s = string.Format("{0} Reservations from {1} to {2}", Count, startDate, endDate);
        }
        else
        {
            s = "0 Reservations";
        }

        return s;
    }

    /// <summary>
    /// Find any blocking reservations that are not for the specified activity
    /// </summary>
    /// <param name="a_act"></param>
    /// <param name="a_startTicks"></param>
    /// <param name="a_endTicks"></param>
    /// <param name="o_resourceReservation"></param>
    /// <returns>An interval that intersects, there could be more</returns>
    public bool Intersects(InternalActivity a_act, long a_startTicks, long a_endTicks, out ResourceReservation o_resourceReservation)
    {
        o_resourceReservation = null;
        foreach (ResourceReservation reservation in m_reservations)
        {
            if (reservation.m_ia != a_act && reservation.Intersection(a_startTicks, a_endTicks))
            {
                o_resourceReservation = reservation;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Find any blocking reservations that are not for the specified activity
    /// </summary>
    /// <param name="a_act"></param>
    /// <param name="a_startTicks"></param>
    /// <param name="a_endTicks"></param>
    /// <param name="o_resourceReservation"></param>
    /// <returns>An interval that intersects, there could be more</returns>
    public bool Intersections(InternalActivity a_act, long a_startTicks, long a_endTicks, out IEnumerable<ResourceReservation> o_resourceReservation)
    {
        List<ResourceReservation> resourceReservation = new List<ResourceReservation>();
        foreach (ResourceReservation reservation in m_reservations)
        {
            if (reservation.m_ia != a_act && reservation.Intersection(a_startTicks, a_endTicks))
            {
                resourceReservation.Add(reservation);
            }
        }

        o_resourceReservation = resourceReservation;
        return resourceReservation.Count > 0;
    }
}