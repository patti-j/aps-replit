namespace PT.Scheduler.Schedule.Demand;

/// <summary>
/// A set of EligibleLots
/// </summary>
internal partial class EligibleLots : IPTSerializable, IEquatable<EligibleLots>
{
    internal EligibleLots()
    {
        // This constructor is used when an object is loaded from a persistent storage.
        // Do not place any code here.
    }

    internal EligibleLots(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 694)
        {
            a_reader.Read(m_serializer, out m_eligibleLots);
        }
        else if (a_reader.VersionNumber >= 693)
        {
            HashSet<string> eligibleLots = new ();
            a_reader.Read(out eligibleLots);
        }
    }

    private readonly EligibleLotSerializer m_serializer = new ();

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_serializer, m_eligibleLots);
    }

    private readonly Dictionary<string, EligibleLot> m_eligibleLots = new ();

    internal EligibleLot Get(string a_lotCode)
    {
        if (!string.IsNullOrEmpty(a_lotCode) && m_eligibleLots.TryGetValue(a_lotCode, out EligibleLot el))
        {
            return el;
        }

        return null;
    }

    /// <summary>
    /// Whether this set contains an EligibleLot for a specific lot code.
    /// </summary>
    /// <param name="a_lotCode"></param>
    /// <returns></returns>
    internal bool Contains(string a_lotCode)
    {
        EligibleLot el = Get(a_lotCode);
        return el != null;
    }

    public const int UNIQUE_ID = 826;

    public int UniqueId => UNIQUE_ID;

    /// <summary>
    /// Whether the lot can be used to satisfy this material requirement.
    /// </summary>
    /// <param name="a_lot">The lot whose eligibility is to be determined</param>
    /// <param name="a_usedAsEligibleLots">The set of lots that are used as eligible lots.</param>
    /// <returns></returns>
    internal bool IsLotElig(Lot a_lot, EligibleLots a_usedAsEligibleLots)
    {
        bool ret;
        if (m_eligibleLots != null && m_eligibleLots.Count > 0)
        {
            EligibleLot el;
            if (m_eligibleLots.TryGetValue(a_lot.Code, out el))
            {
                ret = true;
            }
            else
            {
                ret = false;
            }
        }
        else
        {
            // if no material requirers use the lot as an eligible lot then it 
            // can be used.
            ret = a_usedAsEligibleLots.GetLotRequirerCount(a_lot.Code) == 0;
        }

        return ret;
    }

    /// <summary>
    /// Whether this contains an EligibleLot for a lot.
    /// </summary>
    /// <param name="a_lotCode"></param>
    /// <returns></returns>
    private bool ContainsKey(string a_lotCode)
    {
        return m_eligibleLots.ContainsKey(a_lotCode);
    }

    /// <summary>
    /// Set the lots this distribution is eligible to draw material from.
    /// This function is only necessary for conversion of scenarios that used the original serialization of Eligible Lot funtionality and
    /// can be deleted on final completion of this enhancement.
    /// </summary>
    /// <param name="a_eligibleLotExternalIds">This value should only be created for use by this class. Don't edit it after calling this function.</param>
    internal void SetEligibleLots(HashSet<string> a_eligibleLotCodes, BaseIdObject a_mrr)
    {
        foreach (string lotCode in a_eligibleLotCodes)
        {
            Add(lotCode, a_mrr);
        }
    }

    /// <summary>
    /// Add an eligible lot and material requirer.
    /// </summary>
    /// <param name="a_eligibleLotCode"></param>
    /// <param name="a_obj"></param>
    internal void Add(string a_eligibleLotCode, BaseIdObject a_obj)
    {
        if (!m_eligibleLots.TryGetValue(a_eligibleLotCode, out EligibleLot el))
        {
            el = new EligibleLot(a_eligibleLotCode);
            m_eligibleLots.Add(a_eligibleLotCode, el);
        }

        el.AddRequirer(a_obj);
    }

    internal void Add(EligibleLot a_eligibleLot)
    {
        m_eligibleLots.Add(a_eligibleLot.LotId, a_eligibleLot);
    }

    /// <summary>
    /// Remove a lot requirer (MR, SO)
    /// </summary>
    /// <param name="a_eligLotCode"></param>
    /// <param name="a_o"></param>
    /// <returns>Whether the lot code no longer has any requirers</returns>
    internal bool Remove(string a_eligLotCode, BaseIdObject a_o)
    {
        if (m_eligibleLots.TryGetValue(a_eligLotCode, out EligibleLot el))
        {
            if (el.Remove(a_o))
            {
                if (el.RequirerCount == 0)
                {
                    m_eligibleLots.Remove(a_eligLotCode);
                    return true; //Only return true when the entire lot code has no more eligible lot requirements
                }
            }
        }

        return false;
    }

    internal void Remove(IQtyRequirement a_qtyReq)
    {
        HashSet<string> lotCodesUsingRequirer = new ();
        foreach (string lotCode in LotCodesEnumerator)
        {
            if (a_qtyReq.ContainsEligibleLot(lotCode))
            {
                lotCodesUsingRequirer.Add(lotCode);
            }
        }
       
        foreach (string lotCode in lotCodesUsingRequirer)
        {
            if (m_eligibleLots.TryGetValue(lotCode, out EligibleLot lot))
            {
                lot.Remove((BaseIdObject)a_qtyReq);
            }
        }
    }

    /// <summary>
    /// Enumerate the set of eligible lots.
    /// </summary>
    /// <returns></returns>
    internal Dictionary<string, EligibleLot>.Enumerator GetEligibleLotsEnumerator()
    {
        return m_eligibleLots.GetEnumerator();
    }

    public IEnumerable<string> LotCodesEnumerator
    {
        get
        {
            foreach (string lotCode in m_eligibleLots.Keys)
            {
                yield return lotCode;
            }
        }
    }

    /// <summary>
    /// The number of eligible lots
    /// </summary>
    internal int Count => m_eligibleLots.Count;

    /// <summary>
    /// Empty the set of eligible lots.
    /// </summary>
    internal void Clear()
    {
        m_eligibleLots.Clear();
    }

    /// <summary>
    /// A list of all the eligible lots.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        System.Text.StringBuilder sb = new ();
        foreach (KeyValuePair<string, EligibleLot> lotCode in m_eligibleLots)
        {
            if (sb.Length == 0)
            {
                sb.Append(lotCode.Key);
            }
            else
            {
                sb.Append("," + lotCode.Key);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Determine the total number of material requirers associated with the lots.
    /// </summary>
    /// <returns></returns>
    internal int GetRequirerCount()
    {
        int count = 0;
        Dictionary<string, EligibleLot>.Enumerator etr = GetEligibleLotsEnumerator();
        while (etr.MoveNext())
        {
            count += etr.Current.Value.RequirerCount;
        }

        return count;
    }

    /// <summary>
    /// Get the number of unfulfilled material requirers that use a lot code as an eligible Lot.
    /// </summary>
    /// <param name="a_lotCode"></param>
    /// <returns></returns>
    internal int GetLotRequirerCount(string a_lotCode)
    {
        int count = 0;
        EligibleLot el;
        if (m_eligibleLots.TryGetValue(a_lotCode, out el))
        {
            count = el.RequirerCount;
        }

        return count;
    }

    internal void RestoreReferences(BaseIdObject a_o)
    {
        Dictionary<string, EligibleLot>.Enumerator etr = m_eligibleLots.GetEnumerator();
        while (etr.MoveNext())
        {
            etr.Current.Value.RestoreReferences(a_o);
        }
    }

    internal Dictionary<string, EligibleLot>.Enumerator GetEnumerator()
    {
        return m_eligibleLots.GetEnumerator();
    }

    public bool Equals(EligibleLots? a_other)
    {
        if (ReferenceEquals(this, a_other))
        {
            return true;
        }

        if (a_other is null)
        {
            return false;
        }

        // Fast mismatch if sizes differ
        if (m_eligibleLots.Count != a_other.m_eligibleLots.Count)
        {
            return false;
        }

        foreach ((string lotCode, EligibleLot el) in m_eligibleLots)
        {
            //In this comparison we only care about the lot codes, not the requirers because the requirers
            //from the import object can't be set.
            if (!a_other.m_eligibleLots.TryGetValue(lotCode, out var otherEl))
            {
                return false;
            }
        }

        return true; // all lots matched
    }

    public override bool Equals(object? a_obj) => a_obj is EligibleLots els && Equals(els);

    public override int GetHashCode()
    {
        int hash = 0;
        foreach ((string lotCode, EligibleLot el) in m_eligibleLots)
        {
            int lotCodeHash = StringComparer.Ordinal.GetHashCode(lotCode);
            //XOR the hash codes of each lot code to get a combined hash code that is order-insensitve
            hash ^= lotCodeHash;
        }
        return hash;
    }

    public static bool operator ==(EligibleLots? a_left, EligibleLots? a_right) => Equals(a_left, a_right);
    public static bool operator !=(EligibleLots? a_left, EligibleLots? a_right) => !Equals(a_left, a_right);
}