using System.Collections;
using System.ComponentModel;
using System.Text.RegularExpressions;
using PT.Common.Attributes;
using PT.APIDefinitions.RequestsAndResponses.Webapp;
using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.APSCommon.Serialization;
using PT.Common.Compression;
using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.Common.Localization;
using PT.ERPTransmissions;
using PT.PackageDefinitions;
using PT.PackageDefinitions.Settings;
using PT.Scheduler.Extensions;
using PT.SchedulerDefinitions;
using PT.SchedulerDefinitions.PermissionTemplates;
using PT.Transmissions;

using ECompressionType = PT.Common.Compression.ECompressionType;

namespace PT.Scheduler;

/// <summary>
/// Summary description for User.
/// </summary>
public class User : BaseObject, ICloneable, IPTDeserializable
{
    public new const int UNIQUE_ID = 351;

    #region IPTSerializable Members
    internal User(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12542)
        {
            a_reader.Read(out m_firstName);
            a_reader.Read(out m_lastName);
            a_reader.Read(out m_password);

            // This should always be called to maintain v11 languageKeys to v12 standards.
            a_reader.Read(out m_displayLanguage);
            m_displayLanguage = LegacyLocalizationConverter(m_displayLanguage);

            m_userPermissionSetId = new BaseId(a_reader);
            m_userFlags = new BoolVector32(a_reader);
            a_reader.Read(out m_closeDialogWaitDuration); //new in 31

            m_kpiPlotVisibleFlags = new BoolVector32(a_reader);
            m_userFlags2 = new BoolVector32(a_reader);

            a_reader.Read(out m_lastPasswordReset);
            a_reader.Read(out int previousPasswordsCount);

            for (int passwordI = 0; passwordI < previousPasswordsCount; passwordI++)
            {
                string nextPassword;
                a_reader.Read(out nextPassword);
                m_previousPasswords.Add(nextPassword);
            }

            a_reader.Read(out m_transmissionReceptionType);
            a_reader.Read(out m_currentWorkspaceName);

            a_reader.Read(out int dictCount);
            for (int i = 0; i < dictCount; i++)
            {
                a_reader.Read(out string key);
                a_reader.Read(out byte[] value);
                m_workspacesDictionary.Add(key, value);
            }

            m_plantPermissionsId = new BaseId(a_reader);
            a_reader.Read(out m_compressionType);

            a_reader.Read(out m_userPreferencesData);
            a_reader.Read(out m_lastLoginDate);
            a_reader.Read(new BaseIdClassFactory(), out m_loadedScenarioIds);
        }
        else if (a_reader.VersionNumber >= 12507)
        {
            a_reader.Read(out m_firstName);
            a_reader.Read(out m_lastName);
            a_reader.Read(out m_password);
            a_reader.Read(out string m_taskNotes);

            // This should always be called to maintain v11 languageKeys to v12 standards.
            a_reader.Read(out m_displayLanguage);
            m_displayLanguage = LegacyLocalizationConverter(m_displayLanguage); 

            m_userPermissionSetId = new BaseId(a_reader);
            m_userFlags = new BoolVector32(a_reader);
            a_reader.Read(out m_closeDialogWaitDuration); //new in 31

            m_kpiPlotVisibleFlags = new BoolVector32(a_reader);
            m_userFlags2 = new BoolVector32(a_reader);
            a_reader.Read(out long m_lastPasswordReset);
            a_reader.Read(out int previousPasswordsCount);
            for (int passwordI = 0; passwordI < previousPasswordsCount; passwordI++)
            {
                a_reader.Read(out string nextPassword);
            }

            a_reader.Read(out m_transmissionReceptionType);
            a_reader.Read(out m_currentWorkspaceName);

            a_reader.Read(out int dictCount);
            for (int i = 0; i < dictCount; i++)
            {
                string key;
                byte[] value;
                a_reader.Read(out key);
                a_reader.Read(out value);
                m_workspacesDictionary.Add(key, value);
            }

            m_plantPermissionsId = new BaseId(a_reader);
            a_reader.Read(out m_compressionType);
            a_reader.Read(out int m_nbrOfFailedLogins);

            a_reader.Read(out m_userPreferencesData);
            a_reader.Read(out m_lastLoginDate);
            a_reader.Read(new BaseIdClassFactory(), out m_loadedScenarioIds);
        }
        else if (a_reader.VersionNumber >= 12031)
        {
            a_reader.Read(out m_firstName);
            a_reader.Read(out m_lastName);
            a_reader.Read(out m_password);
            a_reader.Read(out string m_taskNotes);
            a_reader.Read(out m_displayLanguage);

            m_userPermissionSetId = new BaseId(a_reader);
            m_userFlags = new BoolVector32(a_reader);
            a_reader.Read(out m_closeDialogWaitDuration); //new in 31

            m_kpiPlotVisibleFlags = new BoolVector32(a_reader);
            m_userFlags2 = new BoolVector32(a_reader);
            a_reader.Read(out long m_lastPasswordReset);
            a_reader.Read(out int previousPasswordsCount);
            for (int passwordI = 0; passwordI < previousPasswordsCount; passwordI++)
            {
                string nextPassword;
                a_reader.Read(out nextPassword);
            }

            a_reader.Read(out m_transmissionReceptionType);
            a_reader.Read(out m_currentWorkspaceName);

            a_reader.Read(out int dictCount);
            for (int i = 0; i < dictCount; i++)
            {
                string key;
                byte[] value;
                a_reader.Read(out key);
                a_reader.Read(out value);
                m_workspacesDictionary.Add(key, value);
            }

            m_plantPermissionsId = new BaseId(a_reader);
            a_reader.Read(out m_compressionType);
            a_reader.Read(out int m_nbrOfFailedLogins);

            a_reader.Read(out m_userPreferencesData);
            a_reader.Read(out m_lastLoginDate);
        }
        else if (a_reader.VersionNumber >= 12030)
        {
            a_reader.Read(out m_firstName);
            a_reader.Read(out m_lastName);
            a_reader.Read(out  m_password);
            a_reader.Read(out string m_taskNotes);
            a_reader.Read(out m_displayLanguage);

            m_compressSettings = new OptimizeSettings(a_reader);
            m_optimizeSettings = new OptimizeSettings(a_reader);
            //StartTime for compress used to be the end time
            m_compressSettings.SetBackwardsCompatibilityForStartEndTimes();

            m_userPermissionSetId = new BaseId(a_reader);
            m_userFlags = new BoolVector32(a_reader);
            a_reader.Read(out m_closeDialogWaitDuration); //new in 31

            m_kpiPlotVisibleFlags = new BoolVector32(a_reader);
            m_userFlags2 = new BoolVector32(a_reader);
            a_reader.Read(out long m_lastPasswordReset);
            a_reader.Read(out int previousPasswordsCount);
            for (int passwordI = 0; passwordI < previousPasswordsCount; passwordI++)
            {
                string nextPassword;
                a_reader.Read(out nextPassword);
            }

            a_reader.Read(out m_transmissionReceptionType);
            a_reader.Read(out m_currentWorkspaceName);

            a_reader.Read(out int dictCount);
            for (int i = 0; i < dictCount; i++)
            {
                string key;
                byte[] value;
                a_reader.Read(out key);
                a_reader.Read(out value);
                m_workspacesDictionary.Add(key, value);
            }

            m_plantPermissionsId = new BaseId(a_reader);
            a_reader.Read(out m_compressionType);
            a_reader.Read(out int m_nbrOfFailedLogins);

            a_reader.Read(out m_userPreferencesData);
            a_reader.Read(out m_lastLoginDate);
        }

        #region 12022
        else if (a_reader.VersionNumber >= 12022)
        {
            a_reader.Read(out m_firstName);
            a_reader.Read(out m_lastName);
            a_reader.Read(out m_password);
            a_reader.Read(out string m_taskNotes);
            a_reader.Read(out m_displayLanguage);

            m_compressSettings = new OptimizeSettings(a_reader);
            //StartTime for compress used to be the end time
            m_compressSettings.SetBackwardsCompatibilityForStartEndTimes();

            m_optimizeSettings = new OptimizeSettings(a_reader);
            m_userPermissionSetId = new BaseId(a_reader);
            m_userFlags = new BoolVector32(a_reader);
            a_reader.Read(out m_closeDialogWaitDuration); //new in 31

            m_kpiPlotVisibleFlags = new BoolVector32(a_reader);
            m_userFlags2 = new BoolVector32(a_reader);
            a_reader.Read(out long m_lastPasswordReset);
            a_reader.Read(out int previousPasswordsCount);
            for (int passwordI = 0; passwordI < previousPasswordsCount; passwordI++)
            {
                string nextPassword;
                a_reader.Read(out nextPassword);
            }

            a_reader.Read(out m_transmissionReceptionType);
            a_reader.Read(out m_currentWorkspaceName);

            a_reader.Read(out int dictCount);
            for (int i = 0; i < dictCount; i++)
            {
                string key;
                byte[] value;
                a_reader.Read(out key);
                a_reader.Read(out value);
                m_workspacesDictionary.Add(key, value);
            }

            m_plantPermissionsId = new BaseId(a_reader);
            a_reader.Read(out m_compressionType);
            a_reader.Read(out int m_nbrOfFailedLogins);

            a_reader.Read(out m_userPreferencesData);
        }
        #endregion

        #region 12014
        else if (a_reader.VersionNumber >= 12014)
        {
            a_reader.Read(out m_firstName);
            a_reader.Read(out m_lastName);
            a_reader.Read(out m_password);
            a_reader.Read(out string m_taskNotes);
            a_reader.Read(out m_displayLanguage);

            m_compressSettings = new OptimizeSettings(a_reader);
            //StartTime for compress used to be the end time
            m_compressSettings.SetBackwardsCompatibilityForStartEndTimes();

            m_optimizeSettings = new OptimizeSettings(a_reader);
            m_userPermissionSetId = new BaseId(a_reader);
            m_userFlags = new BoolVector32(a_reader);
            a_reader.Read(out m_closeDialogWaitDuration); //new in 31

            m_kpiPlotVisibleFlags = new BoolVector32(a_reader);
            m_userFlags2 = new BoolVector32(a_reader);
            a_reader.Read(out long m_lastPasswordReset);
            a_reader.Read(out int previousPasswordsCount);
            for (int passwordI = 0; passwordI < previousPasswordsCount; passwordI++)
            {
                string nextPassword;
                a_reader.Read(out nextPassword);
            }

            a_reader.Read(out m_transmissionReceptionType);
            a_reader.Read(out m_currentWorkspaceName);

            a_reader.Read(out int dictCount);
            for (int i = 0; i < dictCount; i++)
            {
                string key;
                byte[] value;
                a_reader.Read(out key);
                a_reader.Read(out value);
                m_workspacesDictionary.Add(key, value);
            }

            m_plantPermissionsId = new BaseId(a_reader);
            a_reader.Read(out m_compressionType);
            a_reader.Read(out int m_nbrOfFailedLogins);
            a_reader.Read(out string m_timeZone);

            a_reader.Read(out m_userPreferencesData);
        }
        #endregion

        #region 12011
        else if (a_reader.VersionNumber >= 12011)
        {
            a_reader.Read(out m_firstName);
            a_reader.Read(out m_lastName);
            a_reader.Read(out m_password);
            a_reader.Read(out string m_taskNotes);
            a_reader.Read(out m_displayLanguage);

            m_compressSettings = new OptimizeSettings(a_reader);
            //StartTime for compress used to be the end time
            m_compressSettings.SetBackwardsCompatibilityForStartEndTimes();

            m_optimizeSettings = new OptimizeSettings(a_reader);
            m_userPermissionSetId = new BaseId(a_reader);
            m_userFlags = new BoolVector32(a_reader);
            a_reader.Read(out m_closeDialogWaitDuration); //new in 31

            m_kpiPlotVisibleFlags = new BoolVector32(a_reader);
            a_reader.Read(out int publishOnExitReminder);
            m_userFlags2 = new BoolVector32(a_reader);
            a_reader.Read(out long m_lastPasswordReset);
            a_reader.Read(out int previousPasswordsCount);
            for (int passwordI = 0; passwordI < previousPasswordsCount; passwordI++)
            {
                string nextPassword;
                a_reader.Read(out nextPassword);
            }

            a_reader.Read(out m_transmissionReceptionType);
            a_reader.Read(out m_currentWorkspaceName);

            a_reader.Read(out int dictCount);
            for (int i = 0; i < dictCount; i++)
            {
                string key;
                byte[] value;
                a_reader.Read(out key);
                a_reader.Read(out value);
                m_workspacesDictionary.Add(key, value);
            }

            m_plantPermissionsId = new BaseId(a_reader);
            a_reader.Read(out m_compressionType);
            a_reader.Read(out int m_nbrOfFailedLogins);
            a_reader.Read(out string m_timeZone);

            a_reader.Read(out m_userPreferencesData);
        }
        #endregion

        #region 12000
        else if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out int m_connections);
            a_reader.Read(out m_firstName);
            a_reader.Read(out m_lastName);
            a_reader.Read(out m_password);
            a_reader.Read(out string m_taskNotes);
            a_reader.Read(out m_displayLanguage);

            m_compressSettings = new OptimizeSettings(a_reader);
            //StartTime for compress used to be the end time
            m_compressSettings.SetBackwardsCompatibilityForStartEndTimes();

            m_optimizeSettings = new OptimizeSettings(a_reader);
            m_userPermissionSetId = new BaseId(a_reader);
            m_userFlags = new BoolVector32(a_reader);
            a_reader.Read(out m_closeDialogWaitDuration); //new in 31

            m_kpiPlotVisibleFlags = new BoolVector32(a_reader);
            a_reader.Read(out int publishOnExitReminder);
            m_userFlags2 = new BoolVector32(a_reader);
            a_reader.Read(out long m_lastPasswordReset);
            a_reader.Read(out int previousPasswordsCount);
            for (int passwordI = 0; passwordI < previousPasswordsCount; passwordI++)
            {
                string nextPassword;
                a_reader.Read(out nextPassword);
            }

            a_reader.Read(out m_transmissionReceptionType);
            a_reader.Read(out m_currentWorkspaceName);

            a_reader.Read(out int dictCount);
            for (int i = 0; i < dictCount; i++)
            {
                string key;
                byte[] value;
                a_reader.Read(out key);
                a_reader.Read(out value);
                m_workspacesDictionary.Add(key, value);
            }

            m_plantPermissionsId = new BaseId(a_reader);
            a_reader.Read(out m_compressionType);
            a_reader.Read(out int m_nbrOfFailedLogins);
            a_reader.Read(out string m_timeZone);

            a_reader.Read(out m_userPreferencesData);
        }
        #endregion 12000

        #region 723
        else if (a_reader.VersionNumber >= 723)
        {
            a_reader.Read(out int connections);
            a_reader.Read(out m_firstName);
            a_reader.Read(out m_lastName);
            a_reader.Read(out m_password);
            a_reader.Read(out string m_taskNotes);
            a_reader.Read(out int val);
            m_displayLanguage = GetDisplayLanguageStringForBackwardsCompatibility(val);

            m_compressSettings = new OptimizeSettings(a_reader);
            //StartTime for compress used to be the end time
            m_compressSettings.SetBackwardsCompatibilityForStartEndTimes();

            m_optimizeSettings = new OptimizeSettings(a_reader);
            new UserPermissions(a_reader);
            m_userFlags = new BoolVector32(a_reader);
            a_reader.Read(out m_closeDialogWaitDuration); //new in 31

            m_kpiPlotVisibleFlags = new BoolVector32(a_reader);
            a_reader.Read(out int publishOnExitReminder);
            m_userFlags2 = new BoolVector32(a_reader);
            a_reader.Read(out long m_lastPasswordReset);
            a_reader.Read(out int previousPasswordsCount);
            for (int passwordI = 0; passwordI < previousPasswordsCount; passwordI++)
            {
                string nextPassword;
                a_reader.Read(out nextPassword);
            }

            a_reader.Read(out m_transmissionReceptionType);
            new GameSettings(a_reader);
            a_reader.Read(out string currentWorkspaceName); //Current workspace name is irrelevant since workspaces arn't preserved from v11

            a_reader.Read(out int dictCount);
            for (int i = 0; i < dictCount; i++)
            {
                a_reader.Read(out string key);
                a_reader.Read(out byte[] value);
                //We don't preserve these from V11. There are no settings to load.
            }

            new FormLayout(a_reader);

            //Old user preferences
            new BoolVector32(a_reader);
            a_reader.Read(out string skinName);
            m_plantPermissionsId = new BaseId(a_reader);
            a_reader.Read(out m_compressionType);
            a_reader.Read(out int m_nbrOfFailedLogins);

            a_reader.Read(out string timeZone);
        }
        #endregion

        m_optimizeSettings.VerifyLicenseConstraintsForOptimizeSettings();
    }
    
    /// <summary>
    /// Converts legacy language keys (v11) to their updated counterparts (v12) for compatibility.
    /// Returns the original key if no conversion is needed.
    /// </summary>
    private static string LegacyLocalizationConverter(string a_displayLanguage)
    {
        switch (a_displayLanguage)
        {
            case "English_US":
            case "English_GB":
                return "English";

                // If We want to return to using this conversion in the future,
                // uncomment these lines. This WILL require changing the localization keys for zhCN as well.
                // Otherwise, it will result in the translations not working for that language.
                //case "Chinese_PRC":
                //    return "Chinese";
        }
        return a_displayLanguage;
    }

    public void RestoreReferences(UserPermissionSetManager a_permissionSetManager, UserFieldDefinitionManager a_udfManager)
    {
        PlantPermissionSet plantPermissionSetexistCheck = a_permissionSetManager.GetPlantPermissionSet(m_plantPermissionsId);
        if (plantPermissionSetexistCheck == null)
        {
            m_plantPermissionsId = a_permissionSetManager.GetDefaultPlantPermissionSet().Id;
        }

        UserPermissionSet userPermissionSetexistCheck = a_permissionSetManager.GetUserPermissionSet(m_userPermissionSetId);
        if (userPermissionSetexistCheck == null)
        {
            m_userPermissionSetId = a_permissionSetManager.GetDefaultUserPermissionSet().Id;
        }

        a_udfManager.RestoreReferences(this, UserField.EUDFObjectType.Users);
    }

    private void ReadUnusedLayoutProperties(IReader a_reader)
    {
        bool haveformLayout; //new in 32
        a_reader.Read(out haveformLayout);
        if (haveformLayout)
        {
            new FormLayout(a_reader);
        }

        a_reader.Read(out haveformLayout);
        if (haveformLayout)
        {
            new FormLayout(a_reader);
        }

        a_reader.Read(out haveformLayout);
        if (haveformLayout)
        {
            new FormLayout(a_reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_firstName);
        writer.Write(m_lastName);
        writer.Write(m_password);
        writer.Write(m_displayLanguage);

        m_userPermissionSetId.Serialize(writer);
        m_userFlags.Serialize(writer);
        writer.Write(m_closeDialogWaitDuration);

        KpiPlotVisibleFlags.Serialize(writer);

        m_userFlags2.Serialize(writer);

        writer.Write(m_lastPasswordReset);
        writer.Write(m_previousPasswords.Count);
        for (int i = 0; i < m_previousPasswords.Count; ++i)
        {
            writer.Write(m_previousPasswords[i]);
        }

        writer.Write(m_transmissionReceptionType);
        writer.Write(m_currentWorkspaceName);

        writer.Write(m_workspacesDictionary.Count);
        foreach (KeyValuePair<string, byte[]> keyValuePair in m_workspacesDictionary)
        {
            writer.Write(keyValuePair.Key);
            writer.Write(keyValuePair.Value);
        }

        m_plantPermissionsId.Serialize(writer);
        writer.Write(m_compressionType);

        writer.Write(m_userPreferencesData);
        writer.Write(m_lastLoginDate);
        writer.Write(m_loadedScenarioIds);
    }

    private Dictionary<string, byte[]> m_workspacesDictionary = new ();

    /// <summary>
    /// Represents the workspace containers for this user.
    /// </summary>
    [DoNotAuditProperty]
    public Dictionary<string, byte[]> UserWorkspaces => m_workspacesDictionary;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    public class UserException : PTException
    {
        internal UserException(string e)
            : base(e) { }
    }
    #endregion

    #region Construction
    internal User(BaseId a_id, BaseId a_defaultPermissionSetId, BaseId a_defaultPlantPermissionSetId)
        : base(a_id)
    {
        JumpToNextOpInShopViews = true;
        Active = true;
        m_userPermissionSetId = a_defaultPermissionSetId;
        m_plantPermissionsId = a_defaultPlantPermissionSetId;
    }

    internal User(BaseId a_id, BaseId a_defaultPermissionSetId, BaseId a_defaultPlantPermissionSetId, string a_password)
        : this(a_id, a_defaultPermissionSetId, a_defaultPlantPermissionSetId)
    {
        Password = a_password;
    }
    #endregion

    private readonly int m_dockGanttRowHeight; //obsolete. 
    private byte[] m_userPreferencesData;

    [DoNotAuditProperty]
    public byte[] UserPreferenceInfo => m_userPreferencesData;

    #region Overrides
    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    [Browsable(false)]
    public override string DefaultNamePrefix => "User";

    [DoNotAuditProperty]
    public override string Name
    {
        get => base.Name;
        internal set => base.Name = value;
    }
    #endregion

    #region Shared Properties
    private BoolVector32 m_userFlags;

    private string m_password = "";

    /// <summary>
    /// If non-blank this must be entered to logon.
    /// </summary>
    [Browsable(false)]
    [ParenthesizePropertyName(true)]
    [DoNotAuditProperty]
    public string Password
    {
        get => m_password;
        private set => m_password = value;
    }

    /// <summary>
    /// Whether a password has been set for this user
    /// </summary>
    [Browsable(true)]
    [ParenthesizePropertyName(true)]
    [DoNotAuditProperty]
    public bool PasswordSet => !string.IsNullOrEmpty(m_password);

    private string m_firstName = "";

    /// <summary>
    /// User's first name.
    /// </summary>
    [DoNotAuditProperty]
    public string FirstName
    {
        get => m_firstName;
        internal set => m_firstName = value;
    }

    private string m_lastName = "";

    /// <summary>
    /// User's last name.
    /// </summary>
    [DoNotAuditProperty]
    public string LastName
    {
        get => m_lastName;
        internal set => m_lastName = value;
    }

    #region Permissions
    private BaseId m_userPermissionSetId;

    public BaseId UserPermissionSetId
    {
        get => m_userPermissionSetId;
        internal set => m_userPermissionSetId = value;
    }

    public bool AppUser
    {
        get => m_userFlags[AppUserIdx];
        internal set => m_userFlags[AppUserIdx] = value;
    }

    //UserFlags
    private const int AdministratorIdx = 0; //No longer serialized but used for backwards compatibility in the readers.
    private const int HideToolbarTextIdx = 10;
    private const int c_deletedIdx = 12;
    private const int JumpToNextOpInShopViewsIdx = 15;
    private const int PromptForShopViewsSaveIdx = 17;
    private const int ClockAdvanceAutoFinishIdx = 21;
    private const int ClockAdvanceAutoProgressIdx = 22;
    private const int AppUserIdx = 23;
    #endregion Permissions

    private string m_displayLanguage = "English";

    /// <summary>
    /// Specifies which language to use in the user interface for this user.
    /// </summary>
    public string DisplayLanguage
    {
        get => m_displayLanguage;
        internal set => m_displayLanguage = value;
    }

    public string GetDisplayLanguageStringForBackwardsCompatibility(int a_val)
    {
        return DisplayLanguagesForBackwardsCompatibilityDict[a_val];
    }

    private static readonly Dictionary<int, string> DisplayLanguagesForBackwardsCompatibilityDict = new ()
    {
        { 0, "English" },
        { 1, "Polish" },
        { 2, "German" },
        { 3, "Chinese_PRC" },
        { 4, "Spanish" },
        { 5, "Japanese" },
        { 6, "Dutch" },
        { 7, "French" },
        { 8, "Italian" },
        { 9, "English" },
        { 10, "Indonesian" },
        { 11, "Turkish" },
        { 12, "Portuguese" },
        { 13, "InvariantCulture" }
    };

    private short m_compressionType = (short)ECompressionType.Normal;

    [DoNotAuditProperty]
    public ECompressionType CompressionType
    {
        get => (ECompressionType)m_compressionType;
        private set => m_compressionType = (short)value;
    }

    #region Shop Views
    /// <summary>
    /// Whether, in Shop Views,  to have the system automatically show the next scheduled Activity for the User when an Activity is finished.
    /// </summary>
    [DoNotAuditProperty]
    public bool JumpToNextOpInShopViews
    {
        get => m_userFlags[JumpToNextOpInShopViewsIdx];
        internal set => m_userFlags[JumpToNextOpInShopViewsIdx] = value;
    }

    /// <summary>
    /// Whether the system asks the User before saving when closing a modified Activity in Shop Views.
    /// </summary>
    [DoNotAuditProperty]
    public bool PromptForShopViewsSave
    {
        get => m_userFlags[PromptForShopViewsSaveIdx];
        internal set => m_userFlags[PromptForShopViewsSaveIdx] = value;
    }
    #endregion Shop Views

    private BoolVector32 m_userFlags2;

    //UserFlags2 Index
    private const int UserChangedLanguageIdx = 9;
    private const int RequirePasswordResetAtNextLoginIdx = 10;
    private const int ShowCompleteMessageForOptimizeIdx = 11;
    private const int ShowCompleteMessageForImportIdx = 12;
    private const int ShowCompleteMessageForPublishIdx = 13;
    private const int c_activeIdx = 17;
    private const short c_userLockedIdx = 18;

    /// <summary>
    /// This is used to signal that the user changed her language preferences last time. This is used to reset some necessary settings to allow for
    /// for localization to properly take affect.
    /// </summary>
    [DoNotAuditProperty]
    public bool UserChangedLanguage
    {
        get => m_userFlags2[UserChangedLanguageIdx];
        internal set => m_userFlags2[UserChangedLanguageIdx] = value;
    }

    /// <summary>
    /// Force the user to reset password at next login.
    /// </summary>
    [DoNotAuditProperty]
    public bool RequirePasswordResetAtNextLogin
    {
        get => m_userFlags2[RequirePasswordResetAtNextLoginIdx];
        internal set => m_userFlags2[RequirePasswordResetAtNextLoginIdx] = value;
    }

    /// <summary>
    /// Whether this user can login.
    /// </summary>
    public bool Active
    {
        get => m_userFlags2[c_activeIdx];
        internal set => m_userFlags2[c_activeIdx] = value;
    }

    public bool UserLocked
    {
        get => m_userFlags2[c_userLockedIdx];
        internal set => m_userFlags2[c_userLockedIdx] = value;
    }
    #endregion Shared Properties

    #region Properties
    private int m_transmissionReceptionType;

    /// <summary>
    /// Specifies how the client connection will handle various types of transmissions.
    /// The setting will determeine if the client will process the transmission or wait for the scenario result.
    /// </summary>
    [DoNotAuditProperty]
    public TransmissionReceptionType TransmissionReceptionType
    {
        get => (TransmissionReceptionType)m_transmissionReceptionType;
        private set => m_transmissionReceptionType = (int)value;
    }

    private TimeSpan m_closeDialogWaitDuration = TimeSpan.FromSeconds(3);
    
    private BaseId m_plantPermissionsId = BaseId.NULL_ID;

    public BaseId PlantPermissionsId
    {
        get => m_plantPermissionsId;
        internal set => m_plantPermissionsId = value;
    }

    private PlantPermissionSet m_plantPermissionSet; //This is set by the user manager

    public PlantPermissionSet PlantPermissionSetSettings => m_plantPermissionSet;
    
    /// <summary>
    /// The default setting for the user for Auto Finish when doing Clock Advances.
    /// </summary>
    [DoNotAuditProperty]
    public bool ClockAdvanceAutoFinish
    {
        get => m_userFlags[ClockAdvanceAutoFinishIdx];
        internal set => m_userFlags[ClockAdvanceAutoFinishIdx] = value;
    }

    /// <summary>
    /// The default setting for the user for Auto Report Progress when doing Clock Advances.
    /// </summary>
    [DoNotAuditProperty]
    public bool ClockAdvanceAutoProgress
    {
        get => m_userFlags[ClockAdvanceAutoProgressIdx];
        internal set => m_userFlags[ClockAdvanceAutoProgressIdx] = value;
    }

    public DateTime JitLoginExpiration = PTDateTime.MinValue.ToDateTime();

    private HashSet<BaseId> m_loadedScenarioIds = new ();
    public HashSet<BaseId> LoadedScenarioIds
    {
        get { return m_loadedScenarioIds; }
        internal set { m_loadedScenarioIds = value; }
    }
    #endregion Properties

    #region KPI Plot Settings
    private BoolVector32 m_kpiPlotVisibleFlags = new (true);

    /// <summary>
    /// Indicate which KPI values to plot.
    /// </summary>
    public BoolVector32 KpiPlotVisibleFlags
    {
        get => m_kpiPlotVisibleFlags;
        set => m_kpiPlotVisibleFlags = value;
    }
    #endregion

    #region Object Accessors
    private readonly ScheduleViewerSettings m_depricatedViewerSettings;

    /// <summary>
    /// These are the user viewer settings that used to be stored in the user. Now they are stored in a Workspace.
    /// When loading from older versions, these settings can be migrated out of the user.
    /// </summary>
    public ScheduleViewerSettings DepricatedViewerSettings => m_depricatedViewerSettings;

    private readonly OptimizeSettings m_optimizeSettings = new ();

    /// <summary>
    /// Specify the settings to use when this User performs an Optimize.
    /// </summary>
    [Browsable(false)]
    public OptimizeSettings OptimizeSettings => m_optimizeSettings;

    private SplitSettings m_splitSettings = new ();

    /// <summary>
    /// Specify the default settings to use when this User displays the Operation Split dialog.
    /// </summary>
    [Browsable(false)]
    public SplitSettings SplitSettings
    {
        get => m_splitSettings;
        internal set => m_splitSettings = value;
    }

    private OptimizeSettings m_compressSettings = new ();

    /// <summary>
    /// Speciify the settings to use when this User performs an Compress.
    /// </summary>
    [Browsable(false)]
    public OptimizeSettings CompressSettings
    {
        get => m_compressSettings;
        internal set => m_compressSettings = value;
    }

    private string m_currentWorkspaceName;

    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    [DoNotAuditProperty]
    public string CurrentWorkspaceName
    {
        get => m_currentWorkspaceName;
        set => m_currentWorkspaceName = value;
    }
    #endregion

    #region ERP Transmission
    /// <summary>
    /// Returns true if anything changed.
    /// </summary>
    internal bool Update(UserFieldDefinitionManager a_udfManager, UserT t, UserT.User tUser, UserManager users)
    {
        if (tUser.ActiveSet && !tUser.Active && Active && users.IsUserAdmin(Id))
        {
            int activeAdmins = users.CountActiveAdminUsers();
            if (activeAdmins <= 1)
            {
                throw new PTValidationException("2916");
            }
        }

        bool changed = base.Update(tUser, t, a_udfManager, UserField.EUDFObjectType.Users);

        if (tUser.PermissionGroupSet)
        {
            List<UserPermissionSet> permissionSets = users.GetUserPermissionSets();
            foreach (UserPermissionSet set in permissionSets)
            {
                if (set.Name == tUser.UserPermissionGroup)
                {
                    m_userPermissionSetId = set.Id;
                    break;
                }
            }
        }

        if (tUser.DisplayLanguageSet)
        {
            DisplayLanguage = tUser.DisplayLanguage;
            changed = true;
        }

        if (tUser.FirstNameSet && tUser.FirstName != FirstName)
        {
            FirstName = tUser.FirstName;
            changed = true;
        }

        if (tUser.PasswordSet && tUser.Password != Password)
        {
            SetPassword(t, tUser.Password, SystemController.Sys.SystemSettings);
            changed = true;
        }

        if (tUser.JumpToNextOpInShopViewsSet && tUser.JumpToNextOpInShopViews != JumpToNextOpInShopViews)
        {
            JumpToNextOpInShopViews = tUser.JumpToNextOpInShopViews;
            changed = true;
        }

        if (tUser.LastNameSet && tUser.LastName != LastName)
        {
            LastName = tUser.LastName;
            changed = true;
        }
        
        if (tUser.PromptForShopViewsSaveSet && tUser.PromptForShopViewsSave != PromptForShopViewsSave)
        {
            PromptForShopViewsSave = tUser.PromptForShopViewsSave;
            changed = true;
        }

        if (tUser.TransmissionReceptionTypeSet)
        {
            TransmissionReceptionType = tUser.TransmissionReceptionType;
            changed = true;
        }
        
        if (tUser.AdvanceClockReportProgressIsSet && tUser.AdvanceClockReportProgress != ClockAdvanceAutoProgress)
        {
            ClockAdvanceAutoProgress = tUser.AdvanceClockReportProgress;
        }

        if (tUser.AdvanceClockFinishActivitiesIsSet && tUser.AdvanceClockFinishActivities != ClockAdvanceAutoFinish)
        {
            ClockAdvanceAutoFinish = tUser.AdvanceClockFinishActivities;
        }

        if (tUser.ActiveSet && tUser.Active != Active)
        {
            Active = tUser.Active;
        }

        if (tUser.PlantPermissionGroupSet)
        {
            PlantPermissionSet plantPermissionSet = users.GetPlantPermissionSetByName(tUser.PlantPermissionGroup);
            if (plantPermissionSet == null)
            {
                throw new PTValidationException("4454", new object[] { tUser.ExternalId, tUser.PlantPermissionGroup });
            }

            m_plantPermissionsId = plantPermissionSet.Id;
            m_plantPermissionSet = plantPermissionSet;

            changed = true;
        }

        if (tUser.CompressionTypeSet)
        {
            CompressionType = tUser.CompressionType;
        }

        if (tUser.UnlockUserIsSet && UserLocked)
        {
            UnlockUser();
            changed = true;
        }

        if (tUser.PermissionGroupSet)
        {
            UserPermissionSet userPermissionGroup = users.GetUserPermissionSetByName(tUser.UserPermissionGroup);
            if (userPermissionGroup == null)
            {
                throw new PTValidationException("4455", new object[] { tUser.ExternalId, tUser.UserPermissionGroup });
            }

            m_userPermissionSetId = userPermissionGroup.Id;

            changed = true;
        }

        if (tUser.PreferencesSet)
        {
            m_userPreferencesData = tUser.Preferences;
            changed = true;
        }

        if (tUser.AppUserSet)
        {
            AppUser = tUser.AppUser;
            changed = true;
        }

        return changed;
    }
    #endregion

    #region Transmission functionality
    #region Security

    private void ResetPassword(UserResetMyPasswordT a_t, ISettingsManager a_systemSettings)
    {
        if (a_t.ResetAdminUser)
        {
            Active = true;
            UserLocked = false;
            SetPassword(a_t, a_t.NewPassword, a_systemSettings);
        }
        else if (a_t.CurrentPassword == Password)
        {
            SetPassword(a_t, a_t.NewPassword, a_systemSettings);
        }
        else
        {
            throw new PTValidationException("2289");
        }
    }

    /// <summary>
    /// Validates and sets the password.  Passwords should only be set here.
    /// </summary>
    private void SetPassword(PTTransmission a_t, string aNewPassword, ISettingsManager a_systemSettings)
    {
        Password = aNewPassword;
        LastPasswordReset = a_t.TimeStamp.ToDateTime();
        RequirePasswordResetAtNextLogin = false;
        if (!m_previousPasswords.Contains(aNewPassword))
        {
            m_previousPasswords.Add(aNewPassword);
        }
    }

    private long m_lastPasswordReset = DateTime.MinValue.Ticks;

    [DoNotAuditProperty]
    public DateTime LastPasswordReset
    {
        get => new(m_lastPasswordReset);
        private set => m_lastPasswordReset = value.Ticks;
    }

    private long m_lastLoginDate = PTDateTime.MinDateTimeTicks;

    /// <summary>
    /// The user's LastLoginDate. This is in UTC
    /// </summary>
    [DoNotAuditProperty]
    public DateTime LastLoginDate
    {
        get => new (m_lastLoginDate);
        set => m_lastLoginDate = value.Ticks;
    }

    private readonly List<string> m_previousPasswords = new();
    #endregion Security

    private void UnlockUser()
    {
        UserLocked = false;
    }

    //public void Chat(string chat, BaseId senderId)
    //{
    //    UserChatT t = new UserChatT(Id, senderId, chat);
    //    t.TransmissionSender = PTTransmissionBase.TransmissionSenderType.PTSystem;
    //    SystemController.ClientSession.SendClientAction(t);
    //}

    public void Receive(UserIdBaseT a_t, IScenarioDataChanges a_dataChanges)
    {
        ISettingsManager settingsManager = SystemController.Sys.SystemSettings;
        if (a_t is UserLogOnT)
        {
            //Reset number of failed logins after a successful login
            LastLoginDate = PTDateTime.UtcNow.ToDateTime();
        }
        else if (a_t is UserScheduleViewerSettingsChangeT)
        {
            //Obsolete: user settings are stored in the workspace.
            //PT.SchedulerDefinitions.ScheduleViewerSettings settings = ((UserScheduleViewerSettingsChangeT)t).settings;
        }
        else if (a_t is UserSettingsChangeT userSettingsChangeT)
        {
            SaveSettings(userSettingsChangeT, a_dataChanges);
            a_dataChanges.UserChanges.UpdatedObject(Id);
        }
        else if (a_t is UserResetMyPasswordT resetMyPasswordT)
        {
            ResetPassword(resetMyPasswordT, settingsManager);
            a_dataChanges.UserChanges.UpdatedObject(Id);
        }
        else if (a_t is UserLogonAttemptsProcessingT logonAttemptsT)
        {
            //HandleLogonAttempts(logonAttemptsT, userSecuritySettings.MaxNbrLogonAttempts);
        }
    }

    private void SaveSettings(UserSettingsChangeT t, IScenarioDataChanges a_dataChanges)
    {
        //Store those settings that have been set.
        if (t.KpiFlagsSet)
        {
            m_kpiPlotVisibleFlags = t.KpiPlotVisibleFlags;
        }

        if (t.ClockAutoFinishSet)
        {
            ClockAdvanceAutoFinish = t.ClockAutoFinish;
        }

        if (t.ClockAutoProgressSet)
        {
            ClockAdvanceAutoProgress = t.ClockAutoProgress;
        }

        if (t.UserChangedLanguageSet)
        {
            UserChangedLanguage = t.UserChangedLanguage;
        }

        if (t.CurrentWorkspaceNameSet)
        {
            a_dataChanges.UserWorkspaceChanges.UpdatedObject(Id);
            CurrentWorkspaceName = t.CurrentWorkspaceName;
        }

        if (t.UserWorkspacesSet)
        {
            a_dataChanges.UserWorkspaceChanges.UpdatedObject(Id);
            m_workspacesDictionary = t.UserWorkspaces;
        }

        if (t.UserPreferencesSet)
        {
            m_userPreferencesData = t.UserPreferences;
        }
        //TODO: SkinName
    }
    #endregion

    #region Cloning
    public User Clone()
    {
        return this.CopyInMemory(InternalClone);
    }

    private static User InternalClone(IReader a_reader)
    {
        return new User(a_reader);
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion

    /// <summary>
    /// Update user from a UserEdit
    /// </summary>
    /// <param name="a_edit"></param>
    public void Update(UserEdit a_edit, UserPermissionSetManager a_userPermissionSetManager)
    {
        Edit(a_edit);

        if (a_edit.ActiveSet)
        {
            if (a_edit.Active && !a_edit.Active)
            {
                //TODO: return a value so we can log this user off
            }

            Active = a_edit.Active;
        }

        if (a_edit.AppUserSet)
        {
            AppUser = a_edit.AppUser;
        }

        if (a_edit.FirstNameSet)
        {
            FirstName = a_edit.FirstName;
        }

        if (a_edit.LastNameSet)
        {
            LastName = a_edit.LastName;
        }

        if (a_edit.CompressionTypeSet)
        {
            CompressionType = a_edit.CompressionType;
        }
        
        //if (tUser.PasswordSet && tUser.Password != Password)
        //{
        //    SetPassword(t, tUser.Password, SystemController.Sys.SystemSettings);
        //    changed = true;
        //}
        if (a_edit.UserPermissionGroupSet)
        {
            UserPermissionSet userPermissionGroup = a_userPermissionSetManager.GetUserPermissionSetByName(a_edit.UserPermissionGroup);
            if (userPermissionGroup == null)
            {
                throw new PTValidationException("4455", new object[] { a_edit.BaseIdSet ? a_edit.Id : a_edit.ExternalId, a_edit.UserPermissionGroup });
            }

            m_userPermissionSetId = userPermissionGroup.Id;
        }

        if (a_edit.PlantPermissionGroupSet)
        {
            PlantPermissionSet plantPermissionSet = a_userPermissionSetManager.GetPlantPermissionSetByName(a_edit.PlantPermissionGroup);
            if (plantPermissionSet == null)
            {
                throw new PTValidationException("4454", new object[] { a_edit.BaseIdSet ? a_edit.Id : a_edit.ExternalId, a_edit.PlantPermissionGroup });
            }

            m_plantPermissionsId = plantPermissionSet.Id;
            m_plantPermissionSet = plantPermissionSet;
        }
    }

    #region WebApp User Management
    // TODO: Add to this as additional data is managed in the webapp
    /// <summary>
    /// Fill a new user from imported Webapp Data.
    /// </summary>
    /// <param name="a_userDto"></param>
    /// <param name="a_um"></param>
    /// <param name="a_permissionSetManager"></param>
    public void Create(UserDto a_userDto, UserManager a_um, UserPermissionSetManager a_permissionSetManager)
    {
        // TODO: Add
        //if (!a_userDto.Active && Active && users.IsUserAdmin(Id))
        //{
        //    int activeAdmins = users.CountActiveAdminUsers();
        //    if (activeAdmins <= 1)
        //    {
        //        throw new PTValidationException("2916");
        //    }
        //}

        ExternalId = a_userDto.ExternalId;

        Name = a_userDto.Email;

        //Notes = a_userDto.Notes;

        // TODO: Add
        //UpdateUserFields(a_userDto.UserFields);

        // TODO: Add
        //List<UserPermissionSet> permissionSets = a_um.GetUserPermissionSets();
        //foreach (UserPermissionSet set in permissionSets)
        //{
        //    if (set.Name == a_userDto.PAPermissionGroupName)
        //    {
        //        m_userPermissionSetId = set.Id;
        //        break;
        //    }
        //}

        DisplayLanguage = a_userDto.DisplayLanguage; //

        FirstName = a_userDto.FirstName;

        // Obsolete
        //JumpToNextOpInShopViews = a_userDto.JumpToNextOpInShopViews;

        LastName = a_userDto.LastName;
        
        // Obsolete
        //PromptForShopViewsSave = a_userDto.PromptForShopViewsSave;

        // Not in use
        //TransmissionReceptionType = a_userDto.TransmissionReceptionType;

        //RequirePasswordResetAtNextLogin = a_userDto.RequirePasswordResetAtNextLogin;

        // TODO: Investigate. Should be in software, but look for other refs
        //ClockAdvanceAutoProgress = a_userDto.AdvanceClockReportProgress;
        //ClockAdvanceAutoFinish = a_userDto.AdvanceClockFinishActivities;

        // TODO: Add
        //Active = a_userDto.Active;

        PlantPermissionSet plantPermissionSet = a_permissionSetManager.GetDefaultPlantPermissionSet();
        m_plantPermissionsId = plantPermissionSet.Id;
        m_plantPermissionSet = plantPermissionSet;

        CompressionType = (ECompressionType)(int)a_userDto.CompressionType;

        //if (a_userDto.UnlockUserIsSet && UserLocked)
        //{
        //    UnlockUser();
        //}

        // TODO: Hook up Permission Sets from webapp
        //UserPermissionSet userPermissionGroup = a_um.GetUserPermissionSetByName(a_userDto.UserPermissionGroup);
        //if (userPermissionGroup == null)
        //{
        //    throw new PTValidationException("4455", new object[] { a_userDto.ExternalId, a_userDto.UserPermissionGroup });
        //}

        //m_userPermissionSetId = userPermissionGroup.Id;

        // TODO: add
        //m_userPreferencesData = a_userDto.Preferences;

        // TODO: Add
        //AppUser = a_userDto.AppUser;

    }

    // TODO: Add to this as additional data is managed in the webapp
    /// <summary>
    /// Update an existing user with data from the Webapp. 
    /// Note that this updates the PT system with the webapp data, not the other way around - communication is current one-way.
    /// </summary>
    /// <param name="a_userDto"></param>
    /// <param name="a_userPermissionSetManager"></param>
    /// <param name="a_users"></param>
    /// <exception cref="NotImplementedException"></exception>
    public bool Update(UserDto a_userDto, UserPermissionSetManager a_userPermissionSetManager)
    {
        bool changed = false;

        if (Name != a_userDto.Email)
        {
            Name = a_userDto.Email;
            changed = true;
        }

        //if (Description != a_userDto.Description)
        //{
        //    Description = a_userDto.Description;
        //    changed = true;
        //}

        //if (Notes != a_userDto.Notes)
        //{
        //    Notes = a_userDto.Notes;
        //    changed = true;
        //}

        //foreach ((string key, byte[] value) in a_userDto.GetUserFieldValues())
        //{
        //    if (UserFields == null)
        //    {
        //        UserFields = new UserFieldList();
        //    }

        //    if (UserFields.Find(key) is UserField udf)
        //    {
        //        udf.SetDataValueFromSerializedData(value);
        //        changed = true;
        //    }
        //}

        //if (Active != a_userDto.Active)
        //{
        //    if (!a_userDto.Active)
        //    {
        //        //TODO: return a value so we can log this user off
        //    }

        //    Active = a_userDto.Active;
        //}

        //if (AppUser != a_userDto.AppUser)
        //{
        //    AppUser = a_userDto.AppUser;
        //}

        if (FirstName != a_userDto.FirstName)
        {
            FirstName = a_userDto.FirstName;
        }

        if (LastName != a_userDto.LastName)
        {
            LastName = a_userDto.LastName;
        }

        if (CompressionType != (ECompressionType)a_userDto.CompressionType)
        {
            CompressionType = (ECompressionType)a_userDto.CompressionType;
        }

        //UserPermissionSet userPermissionGroup = a_userPermissionSetManager.GetUserPermissionSetByName(a_userDto.PAPermissionGroupName);
        //if (userPermissionGroup == null)
        //{
        //    throw new PTValidationException("4455", new object[] { a_userDto.ExternalId, a_userDto.PAPermissionGroupName });
        //}

        //if (userPermissionGroup.Name != a_userDto.PAPermissionGroupName)
        //{
        //    m_userPermissionSetId = userPermissionGroup.Id;
        //}

        //if (a_userDto.PlantPermissionGroupSet)
        //{
        //    PlantPermissionSet plantPermissionSet = a_userPermissionSetManager.GetPlantPermissionSetByName(a_userDto.PlantPermissionGroup);
        //    if (plantPermissionSet == null)
        //    {
        //        throw new PTValidationException("4454", new object[] { a_userDto.BaseIdSet ? a_userDto.Id : a_userDto.ExternalId, a_userDto.PlantPermissionGroup });
        //    }

        //    m_plantPermissionsId = plantPermissionSet.Id;
        //    m_plantPermissionSet = plantPermissionSet;
        //}

        if (DisplayLanguage != a_userDto.DisplayLanguage)
        {
            DisplayLanguage = a_userDto.DisplayLanguage;
        }

        return changed;
    }

    #endregion
}

#region EligibleUsersCollection
/// <summary>
/// Stores a list of User's who are eligible to access the object.
/// </summary>
public class EligibleUsersCollection : ICopyTable, IPTSerializable
{
    public const int UNIQUE_ID = 319;

    #region IPTSerializable Members
    internal EligibleUsersCollection(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            m_referenceInfo = new ReferenceInfo();
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                BaseId userId = new (reader);
                m_referenceInfo.elligibleUserIds.Add(userId);
                int accessLevel;
                reader.Read(out accessLevel);
                m_referenceInfo.elligibleUserAccessLevels.Add(accessLevel);
            }
        }
    }

    public void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif

        writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            EligibleUser e = (EligibleUser)m_users[i];
            e.User.Serialize(writer);
            writer.Write((int)e.AccessLevel);
        }
    }

    public int UniqueId => UNIQUE_ID;

    private ReferenceInfo m_referenceInfo;

    private class ReferenceInfo
    {
        internal readonly ArrayList elligibleUserIds = new ();
        internal readonly ArrayList elligibleUserAccessLevels = new ();
    }

    internal void RestoreReferences()
    {
        for (int i = 0; i < m_referenceInfo.elligibleUserIds.Count; i++)
        {
            EligibleUser user = new ((BaseId)m_referenceInfo.elligibleUserIds[i], (BaseOrderDefs.accessLevels)m_referenceInfo.elligibleUserAccessLevels[i]);
            Add(user);
        }

        m_referenceInfo = null;
    }
    #endregion

    #region Declarations
    private readonly ArrayList m_users = new ();

    public class EligibleUsersCollectionException : PTException
    {
        internal EligibleUsersCollectionException(string message)
            : base(message) { }
    }
    #endregion

    #region Construction
    internal EligibleUsersCollection() { }
    #endregion

    #region Properties and Methods
    public Type ElementType => typeof(EligibleUser);

    internal EligibleUser Add(EligibleUser user)
    {
        m_users.Add(user);
        return user;
    }

    internal void Remove(int index)
    {
        m_users.RemoveAt(index);
    }

    public object GetRow(int index)
    {
        return (EligibleUser)m_users[index];
    }

    public int Count => m_users.Count;
    #endregion
}
#endregion

#region EligibleUser
/// <summary>
/// Specifies a User who has access to an object and the type of access.
/// </summary>
public class EligibleUser : ExternalBaseIdObject, ICloneable
{
    #region Declarations
    public const string USER = "User";

    public class EligibleUserException : PTException
    {
        internal EligibleUserException(string message)
            : base(message) { }
    }
    #endregion

    #region Construction
    internal EligibleUser(BaseId userId, BaseOrderDefs.accessLevels accessLevel)
        : base(userId)
    {
        this.userId = userId;
        m_accessLevel = accessLevel;
    }
    #endregion

    #region Public Properties
    private BaseOrderDefs.accessLevels m_accessLevel;

    /// <summary>
    /// Specifies how much access the user will have to the object.
    /// </summary>
    public BaseOrderDefs.accessLevels AccessLevel
    {
        get => m_accessLevel;
        internal set => m_accessLevel = value;
    }

    private BaseId userId;

    /// <summary>
    /// The User whose access rights are specified.
    /// </summary>
    [ParenthesizePropertyName(true)]
    public BaseId User
    {
        get => userId;
        internal set => userId = value;
    }
    #endregion

    #region Transmission functionality
    #endregion

    #region Cloning
    public EligibleUser Clone()
    {
        return (EligibleUser)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion
}
#endregion
