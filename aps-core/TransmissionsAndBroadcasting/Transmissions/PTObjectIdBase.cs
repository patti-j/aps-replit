using System.Xml.Serialization;

using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for classes that have only an ExternalId.
/// </summary>
public class PTObjectIdBase : IPTSerializable, IValidate
{
    public const int UNIQUE_ID = 431;

    #region IPTSerializable Members
    public PTObjectIdBase(IReader reader)
    {
        if (reader.VersionNumber >= 389)
        {
            reader.Read(out m_externalId);
            m_id = new BaseId(reader);
            reader.Read(out m_idSet);

            //Was set flags //ExternalId excluded here since it's required.
            //End was set flags
        }
        else if (reader.VersionNumber >= 1)
        {
            reader.Read(out m_externalId);

            //Was set flags //ExternalId excluded here since it's required.
            //End was set flags
        }
    }

    public virtual void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif
        writer.Write(m_externalId);
        //Was set flags //ExternalId excluded here since it's required.
        m_id.Serialize(writer);
        writer.Write(m_idSet);

        //End was set flags
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public PTObjectIdBase() { }

    public PTObjectIdBase(string externalId)
    {
        m_externalId = externalId;
    }

    public virtual void Validate()
    {
        if (ExternalId == null)
        {
            throw new ValidationException("2780", new object[] { GetType().Name });
        }

        if (ExternalId.Length == 0)
        {
            throw new ValidationException("2781", new object[] { GetType().Name });
        }
    }

    private string m_externalId = "";

    [Required(true)]
    /// <summary>
    /// Identifier for external system references.
    /// </summary>
    [XmlIgnore]
    public string ExternalId
    {
        get => m_externalId;
        set => m_externalId = value; //JMC Added so ERPLink could set these outside of the constructor.
    }

    private BaseId m_id = BaseId.NULL_ID;

    /// <summary>
    /// If set then this is used instead of ExternalId to attempt an update.
    /// </summary>
    public BaseId Id
    {
        get => m_id;
        set
        {
            m_id = value;
            m_idSet = true;
        }
    }

    private bool m_idSet;

    /// <summary>
    /// Whether the ID has been set and should be used for updating the object.
    /// </summary>
    public bool IdSet => m_idSet;
}