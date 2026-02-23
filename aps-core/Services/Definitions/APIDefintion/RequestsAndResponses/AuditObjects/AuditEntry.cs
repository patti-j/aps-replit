using PT.APSCommon;
using System.Reflection;

using PT.Common.Attributes;
using PT.Common.Exceptions;

public class AuditEntry : IAuditEntry
{
    private static readonly Dictionary<string, PropertyInfo[]> s_auditPropCollection = new Dictionary<string, PropertyInfo[]>();
    protected object m_originalObject;
    protected object m_updatedObject;
    private readonly PropertyInfo[] m_propertyInfos;
    private bool m_skipAutomaticComparison;
    /// <summary>
    /// Constructor for JSON serializer
    /// </summary>
    public AuditEntry()
    {

    }
    /// <summary>
    /// Constructor of audit entry for top level objects (i.e. they have no parent objects)
    /// </summary>
    /// <param name="a_objectId"></param>
    /// <param name="a_object"></param>
    public AuditEntry(BaseId a_objectId, IPTSerializable a_object)
    {
        Id = a_objectId;
        Changes = new List<ChangeEntry>();
        Type objectType = a_object.GetType();
        ObjectType = objectType.Name;

        if (!s_auditPropCollection.TryGetValue(objectType.Name, out m_propertyInfos))
        {
            m_propertyInfos = objectType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                                        .Where(a_propertyInfo => !Attribute.IsDefined(a_propertyInfo, typeof(DoNotAuditProperty)))
                                        .ToArray();
            s_auditPropCollection.Add(ObjectType, m_propertyInfos);
        }
        m_originalObject = FastShallowClone(a_object, objectType);
        m_updatedObject = a_object;
    }
    /// <summary>
    /// Use this constructor if the object being audited has a parent object. For example; MO on a Job
    /// </summary>
    /// <param name="a_objectId"></param>
    /// <param name="a_parentId"></param>
    /// <param name="a_object"></param>
    public AuditEntry(BaseId a_objectId, BaseId a_parentId, IPTSerializable a_object) : this(a_objectId, a_object)
    {
        ParentId = a_parentId;
    }
    /// <summary>
    /// Use this constructor when the original object and the updated object are different instances. For example, in the case of updating operations on an MO,
    /// since a new Operation Manager is going to be created each time we can track the difference by supplying the two different instances
    /// </summary>
    /// <param name="a_objectId"></param>
    /// <param name="a_parentId"></param>
    /// <param name="a_originalObject"></param>
    /// <param name="a_updatedObject"></param>
    public AuditEntry(BaseId a_objectId, BaseId a_parentId, IPTSerializable a_originalObject, IPTSerializable a_updatedObject) : this(a_objectId, a_originalObject)
    {
        ParentId = a_parentId;
        m_updatedObject = a_updatedObject;
    }
    public static object FastShallowClone(object a_original, Type a_objectType)
    {
        MethodInfo methodInfo = a_objectType.GetMethod("MemberwiseClone", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        return methodInfo?.Invoke(a_original, null);
    }

    public string ObjectType { get; set; }
    public BaseId Id { get; set; }
    public BaseId ParentId { get; set; }
    public bool Deleted { get; set; }
    public bool Added { get; set; }

    public List<ChangeEntry> Changes { get; set; }
 
    private void AddChange(string a_field, object a_oldValue, object a_newValue)
    {
        // Ensure change list exists; manual/synthetic changes may be added without prior initialization.
        Changes ??= new List<ChangeEntry>();

        ChangeEntry item = new ChangeEntry
        {
            Field = a_field,
            OldValue = a_oldValue,
            NewValue = a_newValue
        };

        Changes.Add(item);
    }

    /// <summary>
    /// Allows callers to record a specific field change even when it cannot be detected via reflection.
    /// Useful for synthetic events like move results that don't map to a concrete property.
    /// </summary>
    public void AddManualChange(string a_field, object a_oldValue, object a_newValue)
    {
        AddChange(a_field, a_oldValue, a_newValue);
    }

    public void Compare()
    {
        if (m_skipAutomaticComparison)
        {
            return;
        }

        for (int i = 0; i < m_propertyInfos.Length; i++)
        {
            PropertyInfo prop = m_propertyInfos[i];

            try
            {
                object newValue = m_updatedObject.GetType().GetProperty(prop.Name)?.GetValue(m_updatedObject);
                if (newValue == null)
                {
                    // If there is no new value, skip comparison.
                    // Specifically checking for null as string.Empty is a potentially valid change.
                    continue;
                }

                object originalValue = m_originalObject.GetType().GetProperty(prop.Name)?.GetValue(m_originalObject);
                if (!Equals(originalValue, newValue))
                {
                    switch (newValue)
                    {
                        case string:
                        case int or decimal or long or double or float:
                        case bool:
                        case DateTime:
                        case TimeSpan:
                        case Enum:
                            AddChange(prop.Name, originalValue, newValue);
                            break;
                    }
                }
            }
            catch (PTHandleableException)
            {
                //Skip handleable exceptions here
            }
        }
    }

    public void SetFlags(bool a_added, bool a_deleted)
    {
        Added = a_added;
        Deleted = a_deleted;
    }

    /// <summary>
    /// Skip automatic property comparison. Useful when only manual changes are being recorded.
    /// </summary>
    public void SkipAutomaticComparison(bool a_skip = true)
    {
        m_skipAutomaticComparison = a_skip;
    }
}
