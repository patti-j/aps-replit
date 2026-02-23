using System.Collections;

namespace PT.Scheduler.Simulation;

// [USAGE_CODE] class BlockReservationSet: A set of BlockReservations.
/// <summary>
/// A set of BlockReservations.
/// </summary>
internal class BlockReservationSet : IEnumerable<BlockReservation>
{
    private readonly LinkedList<BlockReservation> m_reservations;

    internal BlockReservationSet()
    {
        m_reservations = new LinkedList<BlockReservation>();
    }

    /// <summary>
    /// Add a reservation to the set.
    /// Side Affects: BlockReservation.BlockReservationSetNode is set to the node containing node.
    /// </summary>
    /// <param name="a_rvn"></param>
    internal void Add(BlockReservation a_rvn)
    {
        m_reservations.AddLast(a_rvn);
#if DEBUG
        if (a_rvn.BlockReservationSetNode != null)
        {
            throw new Exception("The BlockReservationSet's node was already set.");
        }
#endif
        a_rvn.BlockReservationSetNode = m_reservations.Last;
    }

    /// <summary>
    /// Remove a block reservation.
    /// </summary>
    /// <param name="a_rrv"></param>
    internal void Remove(BlockReservation a_rrv)
    {
        m_reservations.Remove(a_rrv.BlockReservationSetNode);
    }

    /// <summary>
    /// Clear all reservations from the set.
    /// </summary>
    internal void Clear()
    {
        m_reservations.Clear();
    }

    /// <summary>
    /// The number of reservations in this set.
    /// </summary>
    internal int Count => m_reservations.Count;

    /// <summary>
    /// Whether a specified interval intersects with any reservation in this set.
    /// </summary>
    /// <param name="a_startTicks">The start of the interval.</param>
    /// <param name="a_endTicks">The end of the interval.</param>
    /// <param name="o_blockReservation"></param>
    /// <returns>null if there is no intersection or the first BlockReservation there's an intersection with.</returns>
    internal bool Intersects(long a_startTicks, long a_endTicks, out BlockReservation o_blockReservation)
    {
        o_blockReservation = null;
        // Blocks that start before or after the interval need to be checked since it's possible
        // that they intersect.
        //
        // No easy performance improvements are possible to below. But I don't expect the size of the set of 
        // reservations to be large so it probably won't ever matter.
        // I considered trying to improve of this by starting the enumeration at the first reservation
        // that starts at or most immediately before the start of the interval, but this turned out to be incorrect
        // since it's possible for the first block to intersect while subsequent blocks might not intersect. So you
        // need to start checking at the beginning unless you want to build in more complex code.
        LinkedList<BlockReservation>.Enumerator etr = m_reservations.GetEnumerator();
        while (etr.MoveNext())
        {
            BlockReservation reservation = etr.Current;

            if (a_endTicks <= reservation.StartTicks)
            {
                // Once sorted enumerator has moved past intervals, it's not possible for an intersection to occur.
                break;
            }

            if (reservation.Intersects(a_startTicks, a_endTicks))
            {
                o_blockReservation = reservation;
                return true;
            }

        }

        return false;
    }

    public IEnumerator<BlockReservation> GetEnumerator()
    {
        foreach (BlockReservation blockReservation in m_reservations)
        {
            yield return blockReservation;
        }
    }

    public override string ToString()
    {
        if (Count == 1)
        {
            LinkedList<BlockReservation>.Enumerator etr = m_reservations.GetEnumerator();
            etr.MoveNext();
            BlockReservation res = etr.Current;
            return res.ToString();
        }

        return string.Format("{0} reservations", Count);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}