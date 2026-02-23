using System.Collections;

using PT.APSCommon;

namespace PT.Scheduler.Schedule.Demand;

/// <summary>
/// A lot at the ids of all the material requirers that specifically
/// require fulfillment by specific lots as specified by properties
/// such as EligibleLots in Sales Order, Transfer Orders, and MaterialRequirements.
/// </summary>
public partial class EligibleLot : IEnumerable<BaseIdObject>
{
    /// <summary>
    /// The id of a lot that a material requirer must by fulfilled by.
    /// </summary>
    private readonly string m_lotId;

    /// <summary>
    /// The ids of material requirers that must be supplied by by this lot id.
    /// </summary>
    private readonly Dictionary<BaseId, BaseIdObject> m_requirers = new ();

    internal EligibleLot(string a_lotId)
    {
        m_lotId = a_lotId;
    }

    /// <summary>
    /// Copy constructor.
    /// </summary>
    /// <param name="a_original"></param>
    internal EligibleLot(EligibleLot a_original)
    {
        m_lotId = a_original.LotId;

        foreach (BaseIdObject requirer in a_original)
        {
            m_requirers.Add(requirer.Id, requirer);

        }
    }

    /// <summary>
    /// The id of a lot that a material requirer must by fulfilled by.
    /// </summary>
    internal string LotId => m_lotId;

    /// <summary>
    /// The number of material requirers the must draw from this Lot that are unfulfilled.
    /// </summary>
    internal int RequirerCount => m_requirers.Count;

    /// <summary>
    /// Specify a material requirer that must be fulfilled by the lot specified by this object.
    /// </summary>
    /// <param name="a_requirer"></param>
    internal void AddRequirer(BaseIdObject a_requirer)
    {
        m_requirers.Add(a_requirer.Id, a_requirer);
    }

    /// <summary>
    /// Remove a requirer from the set of material requirers that
    /// require this lot.
    /// Call this function when a lot is fullfilled to indicate that
    /// the requirer is no longer waiting to use material from this lot.
    /// </summary>
    /// <param name="a_id"></param>
    internal bool Remove(BaseIdObject a_o)
    {
        return m_requirers.Remove(a_o.Id);
    }

    public const int UNIQUE_ID = 828;

    public int UniqueId => UNIQUE_ID;

    internal EligibleLot(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12105)
        {
            a_reader.Read(out m_lotId);
        }
        else if (a_reader.VersionNumber >= 12100)
        {
            a_reader.Read(out m_lotId);
            new BaseId(a_reader); //restore references desirializer obsolete
        }
        else if (a_reader.VersionNumber >= 12070)
        {
            a_reader.Read(out m_lotId);
        }
        else if (a_reader.VersionNumber >= 695)
        {
            a_reader.Read(out m_lotId);
            new BaseId(a_reader); //restore references desirializer obsolete
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_lotId);
    }

    internal bool ContainsRequirer(BaseId a_requirerId)
    {
        return m_requirers.TryGetValue(a_requirerId, out BaseIdObject _);
    }

    /// <summary>
    /// RestoreReferences when there's only 1 requirer.
    /// </summary>
    /// <param name="a_o"></param>
    internal void RestoreReferences(BaseIdObject a_o)
    {
        m_requirers.Add(a_o.Id, a_o);
    }

    public IEnumerator<BaseIdObject> GetEnumerator()
    {
        foreach (BaseIdObject baseIdObject in m_requirers.Values)
        {
            yield return baseIdObject;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}