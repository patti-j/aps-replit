using PT.APSCommon;

namespace PT.Scheduler;

/// <summary>
/// Base class from which all GanttViewerLayout objects are derived.
/// </summary>
public abstract class GanttViewerObjectBase : IPTSerializable
{
    public const int UNIQUE_ID = 389;

    #region IPTSerializable Members
    public GanttViewerObjectBase(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out isPublic);
            reader.Read(out sortIndex);

            id = new BaseId(reader);
            creatorId = new BaseId(reader);
        }
    }

    public virtual void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif
        writer.Write(isPublic);
        writer.Write(sortIndex);

        id.Serialize(writer);
        creatorId.Serialize(writer);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public GanttViewerObjectBase(BaseId id, BaseId creatorId, bool isPublic, int sortIndex)
    {
        this.id = id;
        this.creatorId = creatorId;
        this.isPublic = isPublic;
        this.sortIndex = sortIndex;
    }

    private BaseId id;

    /// <summary>
    /// Unique id.
    /// </summary>
    public BaseId Id
    {
        get => id;
        set => id = value;
    }

    private BaseId creatorId;

    /// <summary>
    /// The Id of the User who created it.  Only the Creator can delete it.
    /// </summary>
    public BaseId CreatorId
    {
        get => creatorId;
        set => creatorId = value;
    }

    private bool isPublic;

    /// <summary>
    /// If true then all users will see the object. Otherwise it will only be visible to the user who created it.  An object cannot be public if its parent object is not public.
    /// </summary>
    public bool IsPublic
    {
        get => isPublic;
        set => isPublic = value;
    }

    private int sortIndex;

    /// <summary>
    /// Used to sort the display of objects.
    /// </summary>
    public int SortIndex
    {
        get => sortIndex;
        set => sortIndex = value;
    }
}