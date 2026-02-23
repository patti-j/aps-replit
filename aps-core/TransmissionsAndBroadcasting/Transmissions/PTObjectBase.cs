using System.Runtime.Serialization;

using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Used by transmission objects properties to indicate whether they are required or optional.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class RequiredAttribute : Attribute, IPTSerializable
{
    public const int UNIQUE_ID = 305;

    #region IPTSerializable Members
    public RequiredAttribute(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out m_required);
        }
    }

    public virtual void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif
        writer.Write(m_required);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public RequiredAttribute(bool required)
    {
        m_required = required;
    }

    /// <summary>
    /// If required then the field must be provided in the interface.  Otherwise a blank or null value is allowed and a default will be used.
    /// </summary>
    protected bool m_required;

    public bool Required => m_required;
}

/// <summary>
/// Summary description for PTObjectBaseT.
/// </summary>
public class PTObjectBase : PTObjectIdBase, IPTSerializable
{
    public new const int UNIQUE_ID = 95;

    #region IPTSerializable Members
    public PTObjectBase(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 259)
        {
            reader.Read(out description);
            reader.Read(out name);
            reader.Read(out notes);

            //Was set flags
            reader.Read(out descriptionSet); //ExternalId excluded here since it's required.
            reader.Read(out nameSet);
            reader.Read(out notesSet);
            //End was set flags

            m_resourceAttributes = new OperationAttributeList(reader);

            bool haveUserFields;
            reader.Read(out haveUserFields);
            if (haveUserFields)
            {
                m_userFields = new UserFieldList(reader);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(description);
        writer.Write(name);
        writer.Write(notes);

        //Was set flags
        writer.Write(descriptionSet); //ExternalId excluded here since it's required.
        writer.Write(nameSet);
        writer.Write(notesSet);
        //End was set flags

        m_resourceAttributes.Serialize(writer);

        writer.Write(m_userFields != null);
        if (m_userFields != null)
        {
            m_userFields.Serialize(writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public PTObjectBase() { }

    public PTObjectBase(string externalId)
        : base(externalId) { }

    public PTObjectBase(string externalId, string name)
        : base(externalId)
    {
        Name = name; //be sure to set the Property not the field so the NameSet value is set.
    }

    public PTObjectBase(string externalId, string name, string description, string notes, string userFields)
        : base(externalId)
    {
        if (name != null)
        {
            Name = name;
        }

        if (description != null)
        {
            Description = description;
        }

        if (notes != null)
        {
            Notes = notes;
        }

        if (userFields != null)
        {
            SetUserFields(userFields);
        }
    }

    #region Exceptions
    public class InterfaceException : ApplicationException
    {
        public InterfaceException(string a_message)
            : base(a_message) { }
    }

    public class RowValidationException : InterfaceException
    {
        public Type objectType;
        public int row;

        public RowValidationException(string message, Type objectType, int row)
            : base(message)
        {
            this.objectType = objectType;
            this.row = row;
        }
    }

    /// <summary>
    /// For error encountered while creating a base object from a Reader.
    /// </summary>
    public class PTObjectBaseCreationException : InterfaceException
    {
        public string propertyName;
        public int propertyIndex;
        public Type expectedType;
        public string actualValue;
        public Type actualType;

        public PTObjectBaseCreationException(string errMessage, string propertyName, int propertyIndex, Type expectedType, string actualValue, Type actualType)
            : base(errMessage)
        {
            this.propertyName = propertyName;
            this.propertyIndex = propertyIndex;
            this.expectedType = expectedType;
            this.actualValue = actualValue;
            this.actualType = actualType;
        }
    }

    /// <summary>
    /// For error encountered while creating a derived object from a Reader.
    /// </summary>
    public class PTObjectCreationException : PTObjectBaseCreationException, ISerializable
    {
        public Type objectType;
        public int rowIndex;

        public PTObjectCreationException(string errMessage, Type objectType, string propertyName, int propertyIndex, Type expectedType, string actualValue, Type actualType, int rowIndex)
            : base(errMessage, propertyName, propertyIndex, expectedType, actualValue, actualType)
        {
            this.objectType = objectType;
            this.rowIndex = rowIndex;
        }
    }
    #endregion

    #region Shared Properties
    private string name = "";

    [Required(true)]
    /// <summary>
    /// Unique, changeable, text identifier.
    /// </summary>
    public string Name
    {
        get => name;
        set
        {
            name = value;
            nameSet = true;
        } //JMC Added so ERPLink could set these outside of the constructor.
    }

    private bool nameSet;

    public bool NameSet => nameSet;

    private string description = "";

    /// <summary>
    /// Text for describing the object.
    /// </summary>
    public string Description
    {
        get => description;
        set
        {
            description = value;
            descriptionSet = true;
        }
    }

    private bool descriptionSet;

    public bool DescriptionSet => descriptionSet;

    private string notes = "";

    /// <summary>
    /// Comments or special considerations pertaining to this object.
    /// </summary>
    public string Notes
    {
        get => notes;
        set
        {
            notes = value;
            notesSet = true;
        }
    }

    private bool notesSet;

    public bool NotesSet => notesSet;
    #endregion

    private OperationAttributeList m_resourceAttributes = new ();

    public OperationAttributeList ResourceAttributes
    {
        get => m_resourceAttributes;
        set => m_resourceAttributes = value;
    }

    private UserFieldList m_userFields;

    /// <summary>
    /// Optional list of user-defined fields.  Null if not initialized.
    /// Update the UserFields from an encoded string.
    /// The format is: FieldName~FieldValue~DataTypeName
    /// Valid DataTypeNames are: string, int, long, decimal, decimal, bool, DateTime
    /// User fields are separated by ';'
    /// Values defining each field are separated by '~'
    /// Specify the text 'NULL' (no quotes) to preserve values in previously created user defined fields.
    /// </summary>
    /// <example>Width~5.2~decimal;Special~true~bool</example>
    public UserFieldList UserFields
    {
        get => m_userFields;
        set => m_userFields = value;
    }

    /// <summary>
    /// Update the UserFields from an encoded string.
    /// The format is: FieldName~FieldValue~DataTypeName
    /// Valid DataTypeNames are: string, int, long, decimal, decimal, bool, DateTime
    /// User fields are separated by ';'
    /// Values defining each field are separated by '~'
    /// Specify the text 'NULL' (no quotes) to preserve values in previously created user defined fields.
    /// </summary>
    /// <example>Width~5.2~decimal;Special~true~bool</example>
    public void SetUserFields(string userFieldStr)
    {
        m_userFields = new UserFieldList(userFieldStr);
    }
}