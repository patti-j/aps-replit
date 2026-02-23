using PT.APSCommon;

namespace PT.Scheduler.Schedule.Demand;

public partial class EligibleLot : IPTSerializable
{
    /// <summary>
    /// Add all the requirers of this EligibleLot class
    /// to another EligibleLot class.
    /// </summary>
    /// <param name="a_eligibleLots">The EligibleLot object whose requirers will be added to.</param>
    internal void AddRequirers(EligibleLot a_eligibleLots)
    {
        foreach (BaseIdObject requirer in a_eligibleLots)
        {
            AddRequirer(requirer);
        }
    }
}

internal partial class EligibleLots
{
    /// <summary>
    /// Add all of the eligible lot codes to a HashSet
    /// </summary>
    /// <param name="a_eligibleLots"></param>
    internal void AddEligibleLotCodes(HashSet<string> a_eligibleLots)
    {
        Dictionary<string, EligibleLot>.Enumerator etr = GetEligibleLotsEnumerator();
        while (etr.MoveNext())
        {
            a_eligibleLots.Add(etr.Current.Key);
        }
    }

    internal void AddEligibleLotCodes(EligibleLots a_addToLots)
    {
        Dictionary<string, EligibleLot>.Enumerator etr = GetEligibleLotsEnumerator();
        while (etr.MoveNext())
        {
            EligibleLot el;
            if (!a_addToLots.m_eligibleLots.TryGetValue(etr.Current.Key, out el))
            {
                el = new EligibleLot(etr.Current.Key);
                a_addToLots.Add(el);
            }

            el.AddRequirers(etr.Current.Value);
        }
    }
}