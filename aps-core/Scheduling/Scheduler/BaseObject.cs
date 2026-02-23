using PT.APSCommon;
using PT.Common.Attributes;
using PT.SchedulerDefinitions;
using PT.Transmissions;

using static PT.SchedulerDefinitions.UserField;

namespace PT.Scheduler;

/// <summary>
/// Abstract base class for all object in the system that should contain notes, attributes, etc.
/// </summary>
public abstract class BaseObject : ExternalBaseIdObject, IPTSerializable
{
    #region IPTSerializable Members
    protected BaseObject(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12303)
        {
            a_reader.Read(out m_name);
            a_reader.Read(out m_description);
            a_reader.Read(out m_notes);
            a_reader.Read(out bool haveUserFields);
            if (haveUserFields)
            {
                m_userFields = new UserFieldList(a_reader);
            }
        }
        else if (a_reader.VersionNumber >= 259)
        {
            a_reader.Read(out m_name);
            a_reader.Read(out m_description);
            a_reader.Read(out m_notes);

            //It was decided not to maintain backwards compatibility
            new AttributesCollection(a_reader);

            bool haveUserFields;
            a_reader.Read(out haveUserFields);
            if (haveUserFields)
            {
                m_userFields = new UserFieldList(a_reader);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_name);
        writer.Write(m_description);
        writer.Write(m_notes);

        writer.Write(m_userFields != null);
        if (m_userFields != null)
        {
            m_userFields.Serialize(writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Construction
    protected BaseObject(BaseId a_id)
        : base("", a_id) { }

    internal BaseObject(BaseId a_id, string a_externalId)
        : base(a_externalId, a_id) { }

    protected BaseObject(BaseId a_id, PTObjectBase a_ptObject)
        : base(a_ptObject.ExternalId, a_id)
    {
        Name = a_ptObject.Name;
        if (String.IsNullOrEmpty(Name))
        {
            Name = a_ptObject.ExternalId; //Anspach couldn't bring in job number twice on the same select statement so no other way.  Plus this is a good safeguard against blank Names in general.
        }

        if (a_ptObject.DescriptionSet)
        {
            Description = a_ptObject.Description;
        }

        if (a_ptObject.NotesSet)
        {
            Notes = a_ptObject.Notes;
        }

        //No UDFs in this constructor
    }

    protected BaseObject(BaseId a_id, PTObjectBase a_ptObject, UserFieldDefinitionManager a_udfManager, EUDFObjectType a_objectType)
        : base(a_ptObject.ExternalId, a_id)
    {
        Name = a_ptObject.Name;
        if (String.IsNullOrEmpty(Name))
        {
            Name = a_ptObject.ExternalId; //Anspach couldn't bring in job number twice on the same select statement so no other way.  Plus this is a good safeguard against blank Names in general.
        }

        if (a_ptObject.DescriptionSet)
        {
            Description = a_ptObject.Description;
        }

        if (a_ptObject.NotesSet)
        {
            Notes = a_ptObject.Notes;
        }

        UpdateUserFields(a_ptObject.UserFields, a_udfManager, a_objectType);
    }

    protected BaseObject(BaseId a_id, PTObjectIdBase a_ptObject)
        : base(a_ptObject.ExternalId, a_id) { }

    /// <summary>
    /// Create a new Base Object from an existing one.
    /// </summary>
    /// <param name="a_sourceBaseObject"></param>
    protected BaseObject(BaseId a_newId, BaseObject a_sourceBaseObject)
        : base(a_sourceBaseObject.ExternalId, a_newId)
    {
        Name = a_sourceBaseObject.Name;
        Description = a_sourceBaseObject.Description;
        Notes = a_sourceBaseObject.Notes;
        if(a_sourceBaseObject.UserFields != null)
        {
            UserFields = new UserFieldList(a_sourceBaseObject.UserFields.GetUserFieldImportString());
        }
    }

    #endregion

    #region Declarations
    //These strings are used in various places to reference fields so they must match field names exactly.
    public const string ID = "Id"; //Used for creating DataTables in DataTableManager. If removed, will cause problems creating DataTables.
    public const string NAME = "Name";
    public const string EXTERNAL_ID = "ExternalId";
    public const string NOTES = "Notes";
    public const string ATTRIBUTES = "Attributes";
    public const string PLANT_ID = "PlantId";
    public const string DEPARTMENT_ID = "Department Id";
    public const string DESCRIPTION = "Description";

    //Help string for Properties that have no built-in function.
    public const string INFO_ONLY = "  For information only.";
    #endregion

    #region Shared Properties
    private UserFieldList m_userFields = new UserFieldList();

    /// <summary>
    /// Optional list of user-defined fields.  Null if not initialized.
    /// Import format:
    /// Update the UserFields from an encoded string.
    /// The format is: FieldName~FieldValue~DataTypeName
    /// Valid DataTypeNames are: string, int, long, decimal, decimal, bool, DateTime
    /// User fields are separated by ';'
    /// Values defining each field are separated by '~'
    /// Omit the Value during imports to preserve values in previously created user defined fields.
    /// </summary>
    /// <example>Width~5.2~decimal;Special~true~bool or Width~decimal;Special~bool to preserve internal values</example>
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public UserFieldList UserFields
    {
        get => m_userFields;
        set => m_userFields = value; // the setter is public since no standard functionality uses it. Customizations heavily use UDFs to store data. Coupling is not increased any more by making this public since the getter is already public.
    }

    private string m_name = "";

    /// <summary>
    /// Unique, changeable, text identifier.
    /// </summary>
    [System.ComponentModel.ParenthesizePropertyName(true)]
    public virtual string Name
    {
        get => m_name;
        internal set => m_name = value;
    }

    private string m_description = "";

    /// <summary>
    /// Text for describing the object.
    /// </summary>
    public virtual string Description
    {
        get => m_description;
        internal set => m_description = value;
    }

    private string m_notes = "";

    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    /// <summary>
    /// Comments or special considerations pertaining to this object.
    /// </summary>
    public virtual string Notes
    {
        get => m_notes;
        set => m_notes = value; //made public so customizations can return info here.
    }
    #endregion

    #region Abstract Properties
    [System.ComponentModel.Browsable(false)]
    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    public abstract string DefaultNamePrefix { get; }
    #endregion

    #region Transmission Functionality
    protected bool Update(PTObjectBase a_o, PTTransmission t)
    {
        bool changed = base.Update(a_o);

        if (a_o.NameSet && Name != a_o.Name)
        {
            Name = a_o.Name;
            changed = true;
        }

        if (a_o.DescriptionSet && Description != a_o.Description)
        {
            Description = a_o.Description;
            changed = true;
        }

        if (a_o.NotesSet && Notes != a_o.Notes)
        {
            Notes = a_o.Notes;
            changed = true;
        }

        return changed;
    }    
    
    protected bool Update(PTObjectBase a_o, PTTransmission a_t, UserFieldDefinitionManager a_udfManager, EUDFObjectType a_objectType)
    {
        bool changed = base.Update(a_o);
        
        if (a_o.NameSet && Name != a_o.Name)
        {
            Name = a_o.Name;
            changed = true;
        }

        if (a_o.DescriptionSet && Description != a_o.Description)
        {
            Description = a_o.Description;
            changed = true;
        }

        if (a_o.NotesSet && Notes != a_o.Notes)
        {
            Notes = a_o.Notes;
            changed = true;
        }

        changed |= UpdateUserFields(a_o.UserFields, a_udfManager, a_objectType);

        return changed;
    }
    #endregion

    #region Customizations
    private object m_customizationObject;

    /// <summary>
    /// Can be used to store information for use by customizations.
    /// NOTE: THIS VALUE IS NOT SERIALIZED SO IT'S NOT PRESERVED BETWEEN SESSIONS.
    /// </summary>
    public object CustomizationObject
    {
        get => m_customizationObject;
        set => m_customizationObject = value;
    }
    #endregion

    internal bool Edit(PTObjectBaseEdit a_edit)
    {
        bool changed = false;
        if (a_edit.NameIsSet && Name != a_edit.Name)
        {
            Name = a_edit.Name;
            changed = true;
        }

        if (a_edit.DescriptionSet && Description != a_edit.Description)
        {
            Description = a_edit.Description;
            changed = true;
        }

        if (a_edit.NotesSet && Notes != a_edit.Notes)
        {
            Notes = a_edit.Notes;
            changed = true;
        }

        foreach ((string key, UDFTypes? udfType, byte[] value) in a_edit.GetUserFieldValues())
        {
            if (UserFields == null)
            {
                UserFields = new UserFieldList();
            }

            if (UserFields.Find(key) is UserField udf)
            {
                udf.SetDataValueFromSerializedData(value);
                changed = true;
            }
            else
            {
                if (udfType == null)
                {
                    continue;
                }

                UserField userField = new UserField(key, null, (UDFTypes)udfType);
                userField.SetDataValueFromSerializedData(value);
                UserFields.Add(userField);
                changed = true;
            }
        }

        return changed;
    }

    internal bool UpdateUserFields(UserFieldList a_userFieldList, UserFieldDefinitionManager a_udfManager, UserField.EUDFObjectType a_objectType)
    {
        if (a_udfManager == null)
        {
            //This can happen on default constructors
            return false;
        }
        
        if (a_userFieldList == null || a_userFieldList.Empty)
        {
            return false;
        }

        a_userFieldList.ParseUserFieldList(a_udfManager);

        return UserFields.Update(a_userFieldList);
    }
}