using PT.Scheduler.Simulation.Events;

namespace PT.Scheduler;

/// <summary>
/// The resources reservations for the primary resource requirements of the associations being inserted with MaxDelay.
/// </summary>
internal class ResourceReservationManager
{
    private readonly Dictionary<Resource, List<ResourceReservation>> m_testReservations;

    private readonly Dictionary<Resource, List<ResourceReservation>> m_firmReservations;
    //private readonly Dictionary<BaseId, ResourceReservation> m_testReservationsByOpId;
    //private readonly Dictionary<BaseId, ResourceReservation> m_firmReservationsByOpId;

    internal ResourceReservationManager()
    {
        m_testReservations = new Dictionary<Resource, List<ResourceReservation>>();
        //m_testReservationsByOpId = new Dictionary<BaseId, ResourceReservation>();
        m_firmReservations = new Dictionary<Resource, List<ResourceReservation>>();
        //m_firmReservationsByOpId = new Dictionary<BaseId, ResourceReservation>();
    }

    public bool Empty => m_firmReservations.Count == 0 && m_testReservations.Count == 0;

    internal void SimulationInitialization()
    {
        m_testReservations.Clear();
        //m_testReservationsByOpId.Clear();
        m_firmReservations.Clear();
        //m_firmReservationsByOpId.Clear();
    }

    internal void AddPlanned(ResourceReservation a_reservation)
    {
        if (m_testReservations.TryGetValue(a_reservation.ReservedResource, out List<ResourceReservation> reservations))
        {
            reservations.Add(a_reservation);
        }
        else
        {
            m_testReservations.Add(a_reservation.ReservedResource, new List<ResourceReservation> { a_reservation });
        }

        //m_testReservationsByOpId[a_reservation.m_ia.Operation.Id] = a_reservation;
    }

    //TODO: improve the mapping so this is faster
    internal void RemoveByOperation(BaseOperation a_op)
    {
        foreach (KeyValuePair<Resource, List<ResourceReservation>> keyValuePair in m_testReservations)
        {
            for (int i = keyValuePair.Value.Count - 1; i >= 0; i--)
            {
                if (keyValuePair.Value[i].m_ia.Operation.Id == a_op.Id)
                {
                    keyValuePair.Value.RemoveAt(i);
                    break;
                }
            }
        }

        //if (m_testReservationsByOpId.TryGetValue(a_op.Id, out ResourceReservation reservation))
        //{
        //    m_testReservationsByOpId.Remove(a_op.Id);
        //    if (m_testReservations.TryGetValue(reservation.ReservedResource, out List<ResourceReservation> reservations))
        //    {
        //        reservations.Remove(reservation);
        //    }
        //}
        //else
        //{
        //    DebugException.ThrowInDebug("Reservation missing");
        //}
    }

    internal void ClearPlanned()
    {
        m_testReservations.Clear();
        //m_testReservationsByOpId.Clear();
    }

    internal IEnumerable<ResourceReservationEvent> FirmAllPlanned()
    {
        List<ResourceReservationEvent> eventResults = new ();
        foreach (KeyValuePair<Resource, List<ResourceReservation>> keyValuePair in m_testReservations)
        {
            foreach (ResourceReservation reservation in keyValuePair.Value)
            {
                reservation.MakeFirm();
                if (reservation.PrimaryReservation)
                {
                    //Only add new scheduling events for the primary reservation
                    eventResults.Add(new ResourceReservationEvent(reservation.StartTicks, reservation.ReservedResource, reservation));
                }
            }
        }

        //Merge Dictionaries
        foreach (KeyValuePair<Resource, List<ResourceReservation>> keyValuePair in m_testReservations)
        {
            if (m_firmReservations.TryGetValue(keyValuePair.Key, out List<ResourceReservation> reservations))
            {
                reservations.AddRange(keyValuePair.Value);
            }
            else
            {
                //Resource hasn't been reserved yet
                m_firmReservations.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        //foreach (KeyValuePair<BaseId, ResourceReservation> keyValuePair in m_testReservationsByOpId)
        //{
        //    m_firmReservationsByOpId[keyValuePair.Key] = keyValuePair.Value;
        //}

        //Clear test
        m_testReservations.Clear();
        //m_testReservationsByOpId.Clear();

        return eventResults;
    }

    /// <summary>
    /// Removes all reservations from resources for this activity
    /// </summary>
    /// <param name="a_act"></param>
    public void ActivityScheduled(Resource a_resource, InternalActivity a_act)
    {
        if (m_firmReservations.TryGetValue(a_resource, out List<ResourceReservation> reservations))
        {
            int removalIdx = -1;
            for (int i = 0; i < reservations.Count; i++)
            {
                if (reservations[i].m_ia == a_act)
                {
                    reservations[i].Scheduled(); //Remove the reservation from the resource
                    removalIdx = i;
                    break;
                }
            }

            if (removalIdx != -1)
            {
                reservations.RemoveAt(removalIdx);
            }
        }
    }
}