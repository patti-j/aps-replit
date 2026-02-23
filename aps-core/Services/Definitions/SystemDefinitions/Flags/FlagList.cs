using System.Collections;

using PT.APSCommon;

namespace PT.SystemDefinitions.Flags;

/// <summary>
/// Stores a list of Flags for an object.
/// </summary>
public class FlagList : IPTSerializable
{
    #region IPTSerializable Members
    public FlagList(IReader reader)
    {
        reader.Read(out prevFlagId);
        int flagCount;
        reader.Read(out flagCount);
        for (int i = 0; i < flagCount; i++)
        {
            flags.Add(new Flag(reader));
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(prevFlagId);
        writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(writer);
        }
    }

    public int UniqueId => 580;
    #endregion

    public FlagList()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    #region Flag List maintenance
    private int prevFlagId = -1;

    private BaseId GetNextId()
    {
        prevFlagId++;
        return new BaseId(prevFlagId);
    }

    /// <summary>
    /// Remove and delete all Flags.
    /// </summary>
    public void ClearAllFlags()
    {
        flags.Clear();
    }

    private readonly ArrayList flags = new ();

    public void Add(Flag flag)
    {
        flag.SetId(GetNextId());
        flags.Add(flag);
    }

    public int Count => flags.Count;

    public Flag this[int index] => (Flag)flags[index];
    #endregion
}