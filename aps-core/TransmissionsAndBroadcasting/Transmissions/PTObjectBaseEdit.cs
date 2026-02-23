using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

public class PTObjectBaseEdit : IPTSerializable
{
    #region IPTSerializable Members
    public PTObjectBaseEdit(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12540)
        {
            m_setBools = new BoolVector32(a_reader);

            if (BaseIdSet)
            {
                m_id = new BaseId(a_reader);
            }

            a_reader.Read(out m_externalId);
            a_reader.Read(out m_description);
            a_reader.Read(out m_name);
            a_reader.Read(out m_notes);
            a_reader.Read(out int udfCount);
            for (int i = 0; i < udfCount; i++)
            {
                a_reader.Read(out string udfKey);
                a_reader.Read(out byte[] udfValueBytes);
                a_reader.Read(out short udfType);

                UserField.UDFTypes type = (UserField.UDFTypes)udfType;
                m_udfValues.Add(udfKey, (type, udfValueBytes));
            }

            a_reader.Read(out int attributeCount);
            for (int i = 0; i < attributeCount; i++)
            {
                a_reader.Read(out string attributePropertyName);
                a_reader.Read(out string attributeExternalId);
                a_reader.Read(out byte[] value);

                m_attributeValues.Add(attributePropertyName, (attributeExternalId, value));
            }
        }
        else if (a_reader.VersionNumber >= 12523)
        {
            m_setBools = new BoolVector32(a_reader);

            if (BaseIdSet)
            {
                m_id = new BaseId(a_reader);
            }

            a_reader.Read(out m_externalId);
            a_reader.Read(out m_description);
            a_reader.Read(out m_name);
            a_reader.Read(out m_notes);
            a_reader.Read(out int udfCount);
            for (int i = 0; i < udfCount; i++)
            {
                a_reader.Read(out string udfKey);
                a_reader.Read(out byte[] udfValueBytes);
                a_reader.Read(out short udfType);

                UserField.UDFTypes type = (UserField.UDFTypes)udfType;
                m_udfValues.Add(udfKey, (type, udfValueBytes));
            }
        }
        else if (a_reader.VersionNumber >= 12000)
        {
            m_setBools = new BoolVector32(a_reader);

            if (BaseIdSet)
            {
                m_id = new BaseId(a_reader);
            }

            a_reader.Read(out m_externalId);
            a_reader.Read(out m_description);
            a_reader.Read(out m_name);
            a_reader.Read(out m_notes);
            a_reader.Read(out int udfCount);
            for (int i = 0; i < udfCount; i++)
            {
                a_reader.Read(out string udfKey);
                a_reader.Read(out byte[] udfValueBytes);
                m_udfValues.Add(udfKey, (null, udfValueBytes));
            }
        }
    }

    public void Serialize(IWriter a_writer)
    {
        m_setBools.Serialize(a_writer);

        if (BaseIdSet)
        {
            m_id.Serialize(a_writer);
        }

        a_writer.Write(m_externalId);
        a_writer.Write(m_description);
        a_writer.Write(m_name);
        a_writer.Write(m_notes);
        a_writer.Write(m_udfValues.Count);
        foreach ((string key, (UserField.UDFTypes? type,  byte[] value)) in m_udfValues)
        {
            a_writer.Write(key);
            a_writer.Write(value);
            a_writer.Write((short)type);
        }

        a_writer.Write(m_attributeValues.Count);
        foreach ((string key, (string attributeExternalId, byte[] value)) in m_attributeValues)
        {
            a_writer.Write(key);
            a_writer.Write(attributeExternalId);
            a_writer.Write(value);
        }
    }

    public int UniqueId => 1043;
    #endregion

    public PTObjectBaseEdit() { }

    public PTObjectBaseEdit(string a_name, string a_description, string a_notes)
    {
        if (!string.IsNullOrWhiteSpace(a_name))
        {
            Name = a_name;
        }

        if (!string.IsNullOrWhiteSpace(a_description))
        {
            Description = a_description;
        }

        if (!string.IsNullOrWhiteSpace(a_notes))
        {
            Notes = a_notes;
        }
    }

    public virtual bool HasEdits => //Any changes except ID or ExternalId
        NameIsSet || DescriptionSet || NotesSet || m_udfValues.Count > 0 || m_attributeValues.Count > 0;

    public void AddUserField(string a_propertyName, UserField.UDFTypes a_udfType, byte[] a_udfValue)
    {
        m_udfValues.Add(a_propertyName, (a_udfType, a_udfValue));
    }

    public void AddAttributes(string a_attributeExternalId, string a_propertyName, byte[] a_attributeValue)
    {
        m_attributeValues.Add(a_propertyName, (a_attributeExternalId, a_attributeValue));
    }

    public IEnumerable<(string, UserField.UDFTypes?, byte[])> GetUserFieldValues()
    {
        foreach ((string key, (UserField.UDFTypes?, byte[]) value) in m_udfValues)
        {
            yield return (key, value.Item1, value.Item2);
        }
    }

    public IEnumerable<(string, (string, byte[]))> GetAttributeValues()
    {
        foreach ((string key, (string attributeExternalId, byte[] value)) in m_attributeValues)
        {
            yield return (key, (attributeExternalId, value));
        }
    }

    #region Shared Properties
    private BoolVector32 m_setBools;
    private readonly Dictionary<string, (UserField.UDFTypes?, byte[])> m_udfValues = new ();
    private readonly Dictionary<string, (string, byte[])> m_attributeValues = new ();

    private const short c_baseIdSetIdx = 0;
    private const short c_externalIdSetIdx = 1;
    private const short c_nameSetIdx = 2;
    private const short c_descriptionSetIdx = 3;
    private const short c_notesSetIdx = 4;
    public bool BaseIdSet => m_setBools[c_baseIdSetIdx];
    public bool ExternalIdSet => m_setBools[c_externalIdSetIdx];
    public bool NameIsSet => m_setBools[c_nameSetIdx];
    public bool DescriptionSet => m_setBools[c_descriptionSetIdx];
    public bool NotesSet => m_setBools[c_notesSetIdx];

    protected BaseId m_id;

    public BaseId Id
    {
        get => m_id;
        set
        {
            m_id = value;
            m_setBools[c_baseIdSetIdx] = true;
        }
    }

    protected string m_externalId;

    public string ExternalId
    {
        get => m_externalId;
        set
        {
            m_externalId = value;
            m_setBools[c_externalIdSetIdx] = true;
        }
    }

    private string m_name;

    /// <summary>
    /// Unique, changeable, text identifier.
    /// </summary>
    public string Name
    {
        get => m_name;
        set
        {
            m_name = value;
            m_setBools[c_nameSetIdx] = true;
        }
    }

    private string m_description;

    /// <summary>
    /// Text for describing the object.
    /// </summary>
    public string Description
    {
        get => m_description;
        set
        {
            m_description = value;
            m_setBools[c_descriptionSetIdx] = true;
        }
    }

    private string m_notes = "";

    /// <summary>
    /// Comments or special considerations pertaining to this object.
    /// </summary>
    public string Notes
    {
        get => m_notes;
        set
        {
            m_notes = value;
            m_setBools[c_notesSetIdx] = true;
        }
    }
    #endregion
}