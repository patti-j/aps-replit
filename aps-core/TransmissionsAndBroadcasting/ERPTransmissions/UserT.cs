using PT.APSCommon;
using PT.SchedulerDefinitions;
using PT.Transmissions;
using PT.Transmissions.User;

namespace PT.ERPTransmissions;

public class UserT : ERPMaintenanceTransmission<UserT.User>, IPTSerializable, IDataChangesDependentT<UserT>
{
    public new const int UNIQUE_ID = 588;

    private BoolVector32 bools;
    private const int c_restartAllUsers = 0;

    public bool RestartUser
    {
        get => bools[c_restartAllUsers];
        set => bools[c_restartAllUsers] = value;
    }

    #region PT Serialization
    public UserT(IReader reader)
        : base(reader)
    {
        #region 733
        if (reader.VersionNumber >= 733)
        {
            bools = new BoolVector32(reader);
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                User node = new (reader);
                Add(node);
            }
        }
        #endregion

        #region 1
        else if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                User node = new (reader);
                Add(node);
            }
        }
        #endregion
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        bools.Serialize(writer);
        writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public UserT() { }

    public class User : PTObjectBase, IPTSerializable
    {
        public new const int UNIQUE_ID = 589;

        #region PT Serialization
        public User(IReader reader)
            : base(reader)
        {
            if (reader.VersionNumber >= 12542)
            {
                m_isSetBools = new BoolVector32(reader);
                m_userFlags = new BoolVector32(reader);

                reader.Read(out displayLanguage);

                reader.Read(out firstName);
                reader.Read(out lastName);
                reader.Read(out m_password);
                m_userFlags2 = new BoolVector32(reader);
                m_isSetBools2 = new BoolVector32(reader);
                reader.Read(out m_transmissionsReceptionType);

                reader.Read(out m_plantPermissionGroup);
                reader.Read(out m_compressionType);
                reader.Read(out m_userPermissionGroup);
                reader.Read(out m_preferences);
            }
            else if (reader.VersionNumber >= 12054)
            {
                m_isSetBools = new BoolVector32(reader);
                m_userFlags = new BoolVector32(reader);

                reader.Read(out displayLanguage);

                reader.Read(out firstName);
                reader.Read(out lastName);
                reader.Read(out m_password);
                reader.Read(out string taskNotes);
                m_userFlags2 = new BoolVector32(reader);
                m_isSetBools2 = new BoolVector32(reader);
                reader.Read(out m_transmissionsReceptionType);

                reader.Read(out m_plantPermissionGroup);
                reader.Read(out m_compressionType);
                reader.Read(out m_userPermissionGroup);
                reader.Read(out m_preferences);
            }
            else if (reader.VersionNumber >= 12022)
            {
                m_isSetBools = new BoolVector32(reader);
                m_userFlags = new BoolVector32(reader);

                reader.Read(out displayLanguage);

                reader.Read(out firstName);
                reader.Read(out lastName);
                reader.Read(out string m_password);
                reader.Read(out string taskNotes);
                m_userFlags2 = new BoolVector32(reader);
                m_isSetBools2 = new BoolVector32(reader);
                reader.Read(out m_transmissionsReceptionType);
                reader.Read(out string initialWorkspaceName); //deprecated
                reader.Read(out string initialPermissionsLevel); //deprecated

                new BaseId(reader); //plant permission group id Deprecated
                reader.Read(out m_compressionType);
                new BaseId(reader); //permission group id Deprecated
                reader.Read(out m_preferences);
            }

            #region 12000
            else if (reader.VersionNumber >= 12014)
            {
                m_isSetBools = new BoolVector32(reader);
                m_userFlags = new BoolVector32(reader);

                reader.Read(out displayLanguage);

                reader.Read(out firstName);
                reader.Read(out lastName);
                reader.Read(out string m_password);
                reader.Read(out string taskNotes);
                m_userFlags2 = new BoolVector32(reader);
                m_isSetBools2 = new BoolVector32(reader);
                reader.Read(out m_transmissionsReceptionType);
                reader.Read(out string initialWorkspaceName); //deprecated
                reader.Read(out string initialPermissionsLevel); //deprecated

                new BaseId(reader); //plant permission group id Deprecated
                reader.Read(out m_compressionType);
                new BaseId(reader); //permission group id Deprecated
                reader.Read(out m_preferences);
                reader.Read(out string m_timeZone);
            }
            else if (reader.VersionNumber >= 12000)
            {
                m_isSetBools = new BoolVector32(reader);
                m_userFlags = new BoolVector32(reader);

                reader.Read(out displayLanguage);

                reader.Read(out firstName);
                reader.Read(out lastName);
                reader.Read(out string m_password);
                reader.Read(out string taskNotes);
                m_userFlags2 = new BoolVector32(reader);
                m_isSetBools2 = new BoolVector32(reader);
                reader.Read(out int publishOnExitReminder);
                reader.Read(out m_transmissionsReceptionType);
                reader.Read(out string initialWorkspaceName); //deprecated
                reader.Read(out string initialPermissionsLevel); //deprecated

                new BaseId(reader); //plant permission group id Deprecated
                reader.Read(out m_compressionType);
                new BaseId(reader); //permission group id Deprecated
                reader.Read(out m_preferences);
                reader.Read(out string m_timeZone);
            }
            #endregion

            #region 723
            else if (reader.VersionNumber >= 723)
            {
                m_isSetBools = new BoolVector32(reader);
                m_userFlags = new BoolVector32(reader);

                int val;
                reader.Read(out val);
                displayLanguage = GetDisplayLanguageString(val);
                new UserPermissions(reader);

                reader.Read(out firstName);
                reader.Read(out lastName);
                reader.Read(out string m_password);
                reader.Read(out string taskNotes);
                m_userFlags2 = new BoolVector32(reader);
                m_isSetBools2 = new BoolVector32(reader);
                reader.Read(out int publishOnExitReminder);
                reader.Read(out m_transmissionsReceptionType);
                reader.Read(out string initialWorkspaceName); //deprecated
                reader.Read(out string initialPermissionsLevel); //deprecated
                //Old user preferences
                new BoolVector32(reader);
                reader.Read(out string skinName);

                new BaseId(reader); //plant permission group id Deprecated
                reader.Read(out m_compressionType);

                reader.Read(out string m_timeZone);
            }
            #endregion
        }

        public override void Serialize(IWriter writer)
        {
            base.Serialize(writer);

            m_isSetBools.Serialize(writer);
            m_userFlags.Serialize(writer);

            writer.Write(displayLanguage);

            writer.Write(firstName);
            writer.Write(lastName);
            writer.Write(m_password);
            m_userFlags2.Serialize(writer);
            m_isSetBools2.Serialize(writer);
            writer.Write(m_transmissionsReceptionType);
            writer.Write(m_plantPermissionGroup);
            writer.Write(m_compressionType);
            writer.Write(m_userPermissionGroup);
            writer.Write(m_preferences);
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        #region bools
        // Seems like we can combine the m_userFlags and m_userFlags2
        // but maybe there's a reason for it being like this?
        private BoolVector32 m_userFlags;
        private const int c_administratorIdx = 0;
        private const int c_jumpToNextOpInShopViewsIdx = 15;
        private const int c_promptForShopViewsSaveIdx = 17;

        private BoolVector32 m_isSetBools;
        private const int c_passwordSetIdx = 0;
        private const int c_firstNameSetIdx = 1;
        private const int c_lastNameSetIdx = 2;
        private const int c_administratorSetIdx = 3;
        private const int c_taskNotesSetIdx = 14;
        private const int c_displayLanguageSetIdx = 17;
        private const int c_jumpToNextOpInShopViewsSetIdx = 19;
        private const int c_promptForShopViewsSaveSetIdx = 21;
        private const int c_transmissionReceptionTypeSetIdx = 30;
        private const int c_timeZoneSetIdx = 31;

        private BoolVector32 m_userFlags2;

        //UserFlags2 Index
        //const int HideToolbarTextIDX = 8;
        private const int c_requireResetPasswordAtNextLoginIdx = 11;
        private const int c_hideSchedGridButtonsIdx = 16;
        private const int c_clockAdvanceReportProgressIdx = 17;
        private const int c_clockAdvanceFinishActivitiesIdx = 18;
        private const int c_activeIdx = 21;
        private const short c_unlockUserIdx = 22;
        private const short c_appUserIdx = 23;

        private BoolVector32 m_isSetBools2;
        private const int c_compressionTypeSetIdx = 0;
        private const int c_preferencesSetIdx = 1;
        private const int c_hideToolbarTextSetIdx = 9;
        private const int c_publishOnExitReminderSetIdx = 10;
        private const int c_requireResetPasswordAtNextLoginSetIdx = 13;
        private const int c_hideSchedGridButtonsSetIdx = 18;
        private const int c_clockAdvanceReportProgressIsSetIdx = 19;
        private const int c_clockAdvanceFinishActivitiesIsSetIdx = 20;
        private const int c_activeIsSetIdx = 23;
        private const int c_initialWorkspaceIsSetIdx = 24;
        private const int c_initialPermissionsIsSetIdx = 25;
        private const short c_plantPermissionGroupSetIdx = 27;
        private const short c_unlockUserSetIdx = 28;
        private const int c_permissionGroupSetIdx = 29;
        private const short c_appUserSetIdx = 30;
        #endregion

        public User() { } // reqd. for xml serialization

        public User(string externalId, string name, string description, string notes, string userFields)
            : base(externalId, name, description, notes, userFields) { }
        public User(PtImportDataSet.UsersRow aRow)
            : base(aRow.ExternalId, aRow.Login, aRow.IsDescriptionNull() ? null : aRow.Description, aRow.IsNotesNull() ? null : aRow.Notes, aRow.IsUserFieldsNull() ? null : aRow.UserFields)
        {
            if (!aRow.IsDisplayLanguageNull())
            {
                try
                {
                    DisplayLanguage = aRow.DisplayLanguage;
                }
                catch (Exception err)
                {
                    throw new PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            aRow.DisplayLanguage, "User", "DisplayLanguage"
                        });
                }
            }

            if (!aRow.IsActiveNull())
            {
                Active = aRow.Active;
            }

            if (!aRow.IsFirstNameNull())
            {
                FirstName = aRow.FirstName;
            }

            if (!aRow.IsLastNameNull())
            {
                LastName = aRow.LastName;
            }

            if (!aRow.IsPasswordNull())
            {
                Password = aRow.Password;
            }

            if (!aRow.IsActiveNull())
            {
                Active = aRow.Active;
            }

            if (!aRow.IsUserPermissionGroupNull())
            {
                UserPermissionGroup = aRow.UserPermissionGroup;
            }

            if (!aRow.IsPlantPermissionGroupNull())
            {
                PlantPermissionGroup = aRow.PlantPermissionGroup;
            }
        }

        #region Shared Properties
        /// <summary>
        /// Whether this user is enabled and can login.
        /// </summary>
        public bool Active
        {
            get => m_userFlags[c_activeIdx];
            set
            {
                m_userFlags[c_activeIdx] = value;
                m_isSetBools2[c_activeIsSetIdx] = true;
            }
        }

        public bool ActiveSet => m_isSetBools2[c_activeIsSetIdx];

        private string firstName = "";

        /// <summary>
        /// User's first name.
        /// </summary>
        public string FirstName
        {
            get => firstName;
            set
            {
                firstName = value;
                m_isSetBools[c_firstNameSetIdx] = true;
            }
        }

        public bool FirstNameSet => m_isSetBools[c_firstNameSetIdx];

        private string lastName = "";

        /// <summary>
        /// User's last name.
        /// </summary>
        public string LastName
        {
            get => lastName;
            set
            {
                lastName = value;
                m_isSetBools[c_lastNameSetIdx] = true;
            }
        }

        public bool LastNameSet => m_isSetBools[c_lastNameSetIdx];
        
        public bool TaskNotesSet => m_isSetBools[c_taskNotesSetIdx];
        private string m_password = "";
        /// <summary>
        /// If non-blank this must be entered to logon.
        /// </summary>
        [System.ComponentModel.Browsable(true)]
        [System.ComponentModel.ParenthesizePropertyName(true)]
        public string Password
        {
            get => m_password;
            set
            {
                m_password = value;
                m_isSetBools[c_passwordSetIdx] = true;
            }
        }

        public bool PasswordSet => m_isSetBools[c_passwordSetIdx];
        private short m_transmissionsReceptionType;

        public TransmissionReceptionType TransmissionReceptionType
        {
            get => (TransmissionReceptionType)m_transmissionsReceptionType;
            set
            {
                m_transmissionsReceptionType = (short)value;
                m_isSetBools[c_transmissionReceptionTypeSetIdx] = true;
            }
        }

        public bool TransmissionReceptionTypeSet => m_isSetBools[c_transmissionReceptionTypeSetIdx];

        private string displayLanguage = "English";

        /// <summary>
        /// Specifies which language to use in the user interface for this user.
        /// </summary>
        public string DisplayLanguage
        {
            get => displayLanguage;
            set
            {
                displayLanguage = value;
                m_isSetBools[c_displayLanguageSetIdx] = true;
            }
        }

        public bool DisplayLanguageSet => m_isSetBools[c_displayLanguageSetIdx];

        public string GetDisplayLanguageString(int a_val)
        {
            return DisplayLanguages[a_val];
        }

        public Dictionary<int, string> DisplayLanguages { get; } = new ()
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
            { 11, "InvariantCulture" }
        };

        /// <summary>
        /// Whether, in Shop Views,  to have the system automatically show the next scheduled Activity for the User when an Activity is finished.
        /// </summary>
        public bool JumpToNextOpInShopViews
        {
            get => m_userFlags[c_jumpToNextOpInShopViewsIdx];
            set
            {
                m_userFlags[c_jumpToNextOpInShopViewsIdx] = value;
                m_isSetBools[c_jumpToNextOpInShopViewsSetIdx] = true;
            }
        }

        public bool JumpToNextOpInShopViewsSet => m_isSetBools[c_jumpToNextOpInShopViewsSetIdx];

        /// <summary>
        /// Whether the system asks the User before saving when closing a modified Activity in Shop Views.
        /// </summary>
        public bool PromptForShopViewsSave
        {
            get => m_userFlags[c_promptForShopViewsSaveIdx];
            set
            {
                m_userFlags[c_promptForShopViewsSaveIdx] = value;
                m_isSetBools[c_promptForShopViewsSaveSetIdx] = true;
            }
        }

        public bool PromptForShopViewsSaveSet => m_isSetBools[c_promptForShopViewsSaveSetIdx];
        
        /// <summary>
        /// Whether the system asks the User before saving when closing a modified Activity in Shop Views.
        /// </summary>
        public bool AdvanceClockReportProgress
        {
            get => m_userFlags2[c_clockAdvanceReportProgressIdx];
            set
            {
                m_userFlags2[c_clockAdvanceReportProgressIdx] = value;
                m_isSetBools2[c_clockAdvanceReportProgressIsSetIdx] = true;
            }
        }

        public bool AdvanceClockReportProgressIsSet => m_isSetBools2[c_clockAdvanceReportProgressIsSetIdx];

        /// <summary>
        /// Whether the system asks the User before saving when closing a modified Activity in Shop Views.
        /// </summary>
        public bool AdvanceClockFinishActivities
        {
            get => m_userFlags2[c_clockAdvanceFinishActivitiesIdx];
            set
            {
                m_userFlags2[c_clockAdvanceFinishActivitiesIdx] = value;
                m_isSetBools2[c_clockAdvanceFinishActivitiesIsSetIdx] = true;
            }
        }

        public bool AdvanceClockFinishActivitiesIsSet => m_isSetBools2[c_clockAdvanceFinishActivitiesIsSetIdx];

        public bool UnlockUser
        {
            get => m_userFlags2[c_unlockUserIdx];
            set
            {
                m_userFlags2[c_unlockUserIdx] = value;
                m_isSetBools2[c_unlockUserSetIdx] = true;
            }
        }

        public bool UnlockUserIsSet => m_isSetBools2[c_unlockUserSetIdx];
        #endregion Shared Properties

        private byte[] m_preferences;

        public byte[] Preferences
        {
            get => m_preferences;
            set
            {
                m_preferences = value;
                m_isSetBools2[c_preferencesSetIdx] = true;
            }
        }

        public bool PreferencesSet => m_isSetBools2[c_preferencesSetIdx];

        private string m_plantPermissionGroup;

        public string PlantPermissionGroup
        {
            get => m_plantPermissionGroup;
            set
            {
                m_plantPermissionGroup = value;
                m_isSetBools2[c_plantPermissionGroupSetIdx] = true;
            }
        }

        public bool PlantPermissionGroupSet => m_isSetBools2[c_plantPermissionGroupSetIdx];

        private short m_compressionType;

        public Common.Compression.ECompressionType CompressionType
        {
            get => (Common.Compression.ECompressionType)m_compressionType;
            set
            {
                m_compressionType = (short)value;
                m_isSetBools2[c_compressionTypeSetIdx] = true;
            }
        }

        public bool CompressionTypeSet => m_isSetBools2[c_compressionTypeSetIdx];

        private string m_userPermissionGroup;

        public string UserPermissionGroup
        {
            get => m_userPermissionGroup;
            set
            {
                m_userPermissionGroup = value;
                m_isSetBools2[c_permissionGroupSetIdx] = true;
            }
        }

        public bool PermissionGroupSet => m_isSetBools2[c_permissionGroupSetIdx];

        public bool AppUser
        {
            get => m_userFlags2[c_appUserIdx];
            set
            {
                m_userFlags2[c_appUserIdx] = value;
                AppUserSet = true;
            }
        }

        public bool AppUserSet
        {
            get => m_isSetBools2[c_appUserSetIdx];
            private set => m_isSetBools2[c_appUserSetIdx] = value;
        }
    }

    public User GetByIndex(int i)
    {
        return Nodes[i];
    }
    #region Database Loading
    public void Fill(System.Data.IDbCommand cmd)
    {
        PtImportDataSet.UsersDataTable table = new();

        FillTable(table, cmd);
        Fill(table);
    }

    /// <summary>
    /// Fill the transmission with data from the DataSet.
    /// </summary>
    public void Fill(PtImportDataSet.UsersDataTable aTable)
    {
        for (int i = 0; i < aTable.Count; i++)
        {
            Add(new User(aTable[i]));
        }
    }
    #endregion
    public override string Description => "Users table updated";
}