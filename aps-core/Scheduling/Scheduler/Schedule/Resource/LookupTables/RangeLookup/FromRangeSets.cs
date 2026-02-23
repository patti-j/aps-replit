using PT.APSCommon;

namespace PT.Scheduler.RangeLookup;

/// <summary>
/// Contains multiple FromRangeSets. Manages the creation of FromSetupRangeSets.
/// </summary>
public class FromRangeSets
{
    #region IPTSerializable Members
    public FromRangeSets(IReader reader)
    {
        fromSetupRangeSets = new List<FromRangeSet>();

        if (reader.VersionNumber >= 1)
        {
            id = new BaseId(reader);
            reader.Read(out name);
            reader.Read(out description);

            int count;
            reader.Read(out count);
            for (int i = 0; i < count; ++i)
            {
                FromRangeSet fromRangeSet = new (reader);
                fromSetupRangeSets.Add(fromRangeSet);
            }
        }
    }

    public void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif
        id.Serialize(writer);
        writer.Write(name);
        writer.Write(description);

        writer.Write(fromSetupRangeSets.Count);
        for (int i = 0; i < fromSetupRangeSets.Count; ++i)
        {
            fromSetupRangeSets[i].Serialize(writer);
        }
    }
    #endregion

    private readonly List<FromRangeSet> fromSetupRangeSets;

    private void Init(BaseId aId, string aName, string aDescription)
    {
        id = aId;
        name = aName;
        description = aDescription;
    }

    public FromRangeSets(BaseId aId, string aName, string aDescription)
    {
        fromSetupRangeSets = new List<FromRangeSet>();
        Init(aId, aName, aDescription);
    }

    internal FromRangeSets(BaseId aId, FromRangeSets aOrig)
    {
        Init(aId, "Copy of " + aOrig.name, aOrig.description);
        fromSetupRangeSets = new List<FromRangeSet>(aOrig.fromSetupRangeSets);
    }

    internal FromRangeSet Add(string attributeName, string description, bool aEligibilityConstraint)
    {
        FromRangeSet r = new (attributeName, description, aEligibilityConstraint);
        fromSetupRangeSets.Add(r);
        return r;
    }

    internal void Add(Transmissions.SetupRangeAttributeUpdate update)
    {
        fromSetupRangeSets.Add(new FromRangeSet(update));
    }

    private BaseId id;

    public BaseId Id => id;

    private string name;

    public string Name => name;

    private string description;

    public string Description => description;

    internal void DataChangesCompleted()
    {
        fromSetupRangeSets.Sort(Compare);
    }

    public int Count => fromSetupRangeSets.Count;

    public FromRangeSet this[int idx] => fromSetupRangeSets[idx];

    public FromRangeSet Find(string attributeName)
    {
        // I don't expect for there to ever be more than a few attributes in a table.
        for (int i = 0; i < Count; ++i)
        {
            if (this[i].PTAttributeExternalId == attributeName)
            {
                return this[i];
            }
        }

        return null;
    }

    public override string ToString()
    {
        return string.Format("ID={0}; Name '{1}'; Description={2}; Contains {3} FromRangeSets", id, name, description, Count);
    }

    public int Compare(FromRangeSet r1, FromRangeSet r2)
    {
        return string.Compare(r1.PTAttributeExternalId, r2.PTAttributeExternalId);
    }

    internal void Validate()
    {
        for (int i = 0; i < Count; ++i)
        {
            this[i].Validate();
        }
    }
}