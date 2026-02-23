using System.Collections;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Compression;

namespace PT.Transmissions;

public class UserEditT : UserBaseT, IEnumerable<UserEdit>
{
    #region PT Serialization
    private readonly List<UserEdit> m_userEdits = new ();
    public const int UNIQUE_ID = 1045;

    public UserEditT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                UserEdit node = new (a_reader);
                m_userEdits.Add(node);
            }
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(m_userEdits);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public UserEditT()
    {
    }

    public UserEdit this[int i] => m_userEdits[i];

    public void Validate()
    {
        foreach (UserEdit poEdit in m_userEdits)
        {
            poEdit.Validate();
        }
    }

    public override string Description => string.Format("Users updated ({0})".Localize(), m_userEdits.Count);

    public IEnumerator<UserEdit> GetEnumerator()
    {
        return m_userEdits.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(UserEdit a_pts)
    {
        if (a_pts.NameIsSet && m_userEdits.Any(u => u.Name == a_pts.Name))
        {
            throw new PTValidationException("4466", new object[] { a_pts.Name });
        }

        if (a_pts.ExternalIdSet && m_userEdits.Any(u => u.ExternalId == a_pts.ExternalId))
        {
            throw new PTValidationException("4467", new object[] { a_pts.ExternalId });
        }

        m_userEdits.Add(a_pts);
    }
}

/// <summary>
/// A standard Item to be purchased for stock.  The received Item will go to stock for use by any Job requiring the Item.
/// </summary>
public class UserEdit : PTObjectBaseEdit, IPTSerializable
{
    #region PT Serialization
    public UserEdit(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12109)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            a_reader.Read(out m_firstName);
            a_reader.Read(out m_lastName);
            a_reader.Read(out short compressionType);
            m_compressionType = (ECompressionType)compressionType;
            a_reader.Read(out m_userPermissionGroup);
            a_reader.Read(out m_plantPermissionGroup);
            a_reader.Read(out m_displayLanguage);
        }

        #region 12000
        else if (a_reader.VersionNumber >= 12000)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            a_reader.Read(out m_firstName);
            a_reader.Read(out m_lastName);
            a_reader.Read(out short compressionType);
            m_compressionType = (ECompressionType)compressionType;
        }
        #endregion
    }

    public new void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_setBools.Serialize(a_writer);
        a_writer.Write(m_firstName);
        a_writer.Write(m_lastName);
        a_writer.Write((short)m_compressionType);
        a_writer.Write(m_userPermissionGroup);
        a_writer.Write(m_plantPermissionGroup);
        a_writer.Write(m_displayLanguage);
    }

    public new int UniqueId => 1042;
    #endregion

    public UserEdit(BaseId a_userId)
    {
        Id = a_userId;
        m_externalId = null; //Clear other id without triggering IsSet
    }

    public UserEdit(string a_externalId)
    {
        ExternalId = a_externalId;
        m_id = BaseId.NULL_ID;
    }

    public new bool HasEdits => m_setBools.AnyFlagsSet || base.HasEdits;

    #region Shared Properties
    private BoolVector32 m_bools;
    private const short c_activeIdx = 0;
    private const short c_appUserIdx = 1;

    private BoolVector32 m_setBools;

    private const short c_activeSetIdx = 0;
    private const short c_compressionTypeSetIdx = 1;
    private const short c_firstNameSetIdx = 2;
    private const short c_lastNameSetIdx = 3;
    private const short c_appUserSetIdx = 4;
    private const short c_userPermissionGroupSetIdx = 5;
    private const short c_plantPermissionGroupSetIdx = 6;
    private const short c_displayLanguageSetIdx = 7;
    private const short c_closedScenarioIdsSetIdx = 8;

    public bool ActiveSet => m_setBools[c_activeSetIdx];
    public bool AppUserSet => m_setBools[c_appUserSetIdx];
    public bool CompressionTypeSet => m_setBools[c_compressionTypeSetIdx];
    public bool FirstNameSet => m_setBools[c_firstNameSetIdx];
    public bool LastNameSet => m_setBools[c_lastNameSetIdx];
    public bool UserPermissionGroupSet => m_setBools[c_userPermissionGroupSetIdx];
    public bool PlantPermissionGroupSet => m_setBools[c_plantPermissionGroupSetIdx];
    public bool ClosedScenarioIdsSet => m_setBools[c_closedScenarioIdsSetIdx];

    private ECompressionType m_compressionType;
    private string m_firstName;
    private string m_lastName;
    private string m_userPermissionGroup;
    private string m_plantPermissionGroup;
    private string m_displayLanguage;

    public bool Active
    {
        get => m_bools[c_activeIdx];
        set
        {
            m_bools[c_activeIdx] = value;
            m_setBools[c_activeSetIdx] = true;
        }
    }

    public bool AppUser
    {
        get => m_bools[c_appUserIdx];
        set
        {
            m_bools[c_appUserIdx] = value;
            m_setBools[c_appUserSetIdx] = true;
        }
    }

    public ECompressionType CompressionType
    {
        get => m_compressionType;
        set
        {
            m_compressionType = value;
            m_setBools[c_compressionTypeSetIdx] = true;
        }
    }

    public string FirstName
    {
        get => m_firstName;
        set
        {
            m_firstName = value;
            m_setBools[c_firstNameSetIdx] = true;
        }
    }

    public string LastName
    {
        get => m_lastName;
        set
        {
            m_lastName = value;
            m_setBools[c_lastNameSetIdx] = true;
        }
    }

    public string UserPermissionGroup
    {
        get => m_userPermissionGroup;
        set
        {
            m_userPermissionGroup = value;
            m_setBools[c_userPermissionGroupSetIdx] = true;
        }
    }

    public string PlantPermissionGroup
    {
        get => m_plantPermissionGroup;
        set
        {
            m_plantPermissionGroup = value;
            m_setBools[c_plantPermissionGroupSetIdx] = true;
        }
    }

    public string DisplayLanguage
    {
        get => m_displayLanguage;
        set
        {
            m_displayLanguage = value;
            m_setBools[c_displayLanguageSetIdx] = true;
        }
    }
    #endregion Shared Properties

    public void Validate() { }
}