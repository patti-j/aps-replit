using System.ComponentModel;

using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Abstract class for all objects in the system that have a BaseId and ExternalId.
/// </summary>
public abstract class BaseIdObject : IPTSerializable, AfterRestoreReferences.IAfterRestoreReferences, IKey<BaseId>
{
    public const int UNIQUE_ID = 3;
    #region IPTSerializable Members
    protected BaseIdObject(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 242)
        {
            m_id = new BaseId(a_reader);
        }
    }

    public virtual void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif
        m_id.Serialize(writer);
    }

    public virtual int UniqueId => throw new NotImplementedException("Object doesn't have a valid UniqueId");
    #endregion

    #region IAfterRestoreReferences
    public virtual void AfterRestoreReferences_1(int a_serializationVersionNbr, HashSet<object> a_processedAfterRestoreReferences_1, HashSet<object> a_processedAfterRestoreReferences_2)
    {
        if (!a_processedAfterRestoreReferences_1.Contains(this))
        {
            a_processedAfterRestoreReferences_1.Add(this);
            AfterRestoreReferences.Helpers.CallObjMembers_AfterRestoreReferences_1(a_serializationVersionNbr, this, a_processedAfterRestoreReferences_1, a_processedAfterRestoreReferences_2);
        }
    }

    public virtual void AfterRestoreReferences_2(int a_serializationVersionNbr, HashSet<object> a_processedAfterRestoreReferences_1, HashSet<object> a_processedAfterRestoreReferences_2)
    {
        if (!a_processedAfterRestoreReferences_2.Contains(this))
        {
            a_processedAfterRestoreReferences_2.Add(this);
            AfterRestoreReferences.Helpers.CallObjMembers_AfterRestoreReferences_2(a_serializationVersionNbr, this, a_processedAfterRestoreReferences_1, a_processedAfterRestoreReferences_2);
        }
    }
    #endregion

    #region Construction
    public BaseIdObject(BaseId id)
    {
        m_id = id;
    }

    /// <summary>
    /// Temporary.  Remove once other child classes can handle the constructor with Id.
    /// </summary>
    public BaseIdObject() { }

    public BaseIdObject(BaseId id, BaseIdObject sourceObject)
    {
        m_id = id;
    }
    #endregion

    #region Transmission Functionality
    /// <summary>
    /// Sets the default values for the object.;
    /// </summary>
    /// <param name="a_id">Id to assign to the object.</param>
    /// <param name="baseObject">The object to change.</param>
    /// <returns></returns>
    protected static void SetBaseIdDefaults(BaseId a_id, BaseIdObject a_baseIdObject)
    {
        a_baseIdObject.Id = a_id;
    }
    #endregion

    #region Shared Properties
    private BaseId m_id;

    /// <summary>
    /// Unique, unchangeable, numeric identifier.
    /// </summary>
    //		[System.ComponentModel.ParenthesizePropertyName(true)]		
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public BaseId Id
    {
        get => m_id;

        internal set => m_id = value;
    }
    #endregion

    /// <summary>
    /// If overridden in a parent class, returns a string containing analysis about the object.
    /// Else returns an empty string.
    /// </summary>
    /// <returns></returns>
    /// <summary>
    /// Special summary or troubleshooting information.
    /// </summary>
    [Browsable(false)]
    public virtual string Analysis => "";

    public bool Equals(BaseId a_other)
    {
        return Id.Equals(a_other);
    }

    public virtual BaseId GetKey()
    {
        return Id;
    }
}