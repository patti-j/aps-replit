using System.Collections;

namespace PT.SchedulerDefinitions;

public class LicenseKeyObject : IPTSerializable
{
    #region IPTSerializable
    public LicenseKeyObject(IReader a_reader)
    {
        #region 12000
        if (a_reader.VersionNumber >= 12000)
        {
            m_bools = new BoolVector32(a_reader);

            int val;
            a_reader.Read(out val);
            m_systemIdType = (SystemIdTypes)val;

            a_reader.Read(out val);
            try
            {
                m_edition = (LicenseEdition)val;
            }
            catch
            {
                m_edition = LicenseEdition.Enterprise; // old keys can have a 4rd edition (SuperChain)
            }

            // expiration
            DateTime tmp;
            a_reader.Read(out tmp);
            ExpirationDate = DateTime.SpecifyKind(tmp, DateTimeKind.Utc);
            a_reader.Read(out m_expirationGraceDays);
            a_reader.Read(out tmp);
            MaintenanceExpirationDate = DateTime.SpecifyKind(tmp, DateTimeKind.Utc);
            a_reader.Read(out m_maintenanceGraceDays);

            // limits
            a_reader.Read(out m_maxNbrUsers);
            a_reader.Read(out m_maxNbrPlants);

            // license info
            a_reader.Read(out m_serialCode);
            a_reader.Read(out m_companyId);
            a_reader.Read(out m_licenseId);
            a_reader.Read(out m_publicNotes);
            a_reader.Read(out m_lastModifiedDateUTCTicks);

            // packages
            DeserializePackages(a_reader, m_packages);
            DeserializeOptions(a_reader);

            a_reader.Read(out short licenseStatus);
            m_licenseStatus = (ELicenseStatus)licenseStatus;
        }
        #endregion 12000

        else if (a_reader.VersionNumber >= 668)
        {
            m_bools = new BoolVector32(a_reader);

            int val;
            a_reader.Read(out val);
            m_systemIdType = (SystemIdTypes)val;

            a_reader.Read(out val);
            try
            {
                m_edition = (LicenseEdition)val;
            }
            catch
            {
                m_edition = LicenseEdition.Enterprise; // old keys can have a 4rd edition (SuperChain)
            }

            // expiration
            DateTime tmp;
            a_reader.Read(out tmp);
            ExpirationDate = DateTime.SpecifyKind(tmp, DateTimeKind.Utc);
            a_reader.Read(out m_expirationGraceDays);
            a_reader.Read(out tmp);
            MaintenanceExpirationDate = DateTime.SpecifyKind(tmp, DateTimeKind.Utc);
            a_reader.Read(out m_maintenanceGraceDays);

            // limits
            a_reader.Read(out m_maxNbrUsers);
            a_reader.Read(out m_maxNbrPlants);

            // license info
            a_reader.Read(out m_serialCode);
            a_reader.Read(out m_companyId);
            a_reader.Read(out m_licenseId);
            a_reader.Read(out m_publicNotes);
            a_reader.Read(out m_lastModifiedDateUTCTicks);
        }

        #region 621
        else if (a_reader.VersionNumber >= 621)
        {
            m_bools = new BoolVector32(a_reader);

            int val;
            a_reader.Read(out val);
            m_systemIdType = (SystemIdTypes)val;

            a_reader.Read(out val);
            try
            {
                m_edition = (LicenseEdition)val;
            }
            catch
            {
                m_edition = LicenseEdition.Enterprise; // old keys can have a 4rd edition (SuperChain)
            }

            // expiration
            DateTime tmp;
            a_reader.Read(out tmp);
            ExpirationDate = DateTime.SpecifyKind(tmp, DateTimeKind.Utc);
            a_reader.Read(out m_expirationGraceDays);
            a_reader.Read(out tmp);
            MaintenanceExpirationDate = DateTime.SpecifyKind(tmp, DateTimeKind.Utc);
            a_reader.Read(out m_maintenanceGraceDays);

            // limits
            a_reader.Read(out m_maxNbrUsers);
            a_reader.Read(out m_maxNbrPlants);

            // license info
            a_reader.Read(out m_serialCode);
            a_reader.Read(out m_companyId);
            a_reader.Read(out m_licenseId);
            a_reader.Read(out m_lastModifiedDateUTCTicks);
        }
        #endregion

        #region Version 610
        else if (a_reader.VersionNumber >= 610)
        {
            m_bools = new BoolVector32(a_reader);

            int val;
            a_reader.Read(out val);
            m_systemIdType = (SystemIdTypes)val;

            a_reader.Read(out val);
            try
            {
                m_edition = (LicenseEdition)val;
            }
            catch
            {
                m_edition = LicenseEdition.Enterprise; // old keys can have a 4rd edition (SuperChain)
            }

            // expiration
            DateTime tmp;
            a_reader.Read(out tmp);
            ExpirationDate = DateTime.SpecifyKind(tmp, DateTimeKind.Utc);
            a_reader.Read(out m_expirationGraceDays);
            a_reader.Read(out tmp);
            MaintenanceExpirationDate = DateTime.SpecifyKind(tmp, DateTimeKind.Utc);
            a_reader.Read(out m_maintenanceGraceDays);

            // limits
            a_reader.Read(out m_maxNbrUsers);
            a_reader.Read(out m_maxNbrPlants);

            // license info
            a_reader.Read(out m_serialCode);
            a_reader.Read(out m_companyId);
            a_reader.Read(out m_licenseId);
        }
        #endregion

        #region Version 505
        else if (a_reader.VersionNumber >= 505)
        {
            m_bools = new BoolVector32(a_reader);

            int val;
            a_reader.Read(out val);
            m_systemIdType = (SystemIdTypes)val;

            a_reader.Read(out val);
            try
            {
                m_edition = (LicenseEdition)val;
            }
            catch
            {
                m_edition = LicenseEdition.Enterprise; // old keys can have a 4rd edition (SuperChain)
            }

            // expiration
            DateTime tmp;
            a_reader.Read(out tmp);
            ExpirationDate = DateTime.SpecifyKind(tmp, DateTimeKind.Utc);
            a_reader.Read(out m_expirationGraceDays);
            a_reader.Read(out tmp);
            MaintenanceExpirationDate = DateTime.SpecifyKind(tmp, DateTimeKind.Utc);
            a_reader.Read(out m_maintenanceGraceDays);

            // user limits
            a_reader.Read(out m_maxNbrUsers); // removed m_maxNbrMasterSchedulers
            a_reader.Read(out m_maxNbrUsers); // removed m_maxNbrWhatIfUsers
            a_reader.Read(out m_maxNbrUsers); // removed m_maxNbrViewOnlyUsers
            a_reader.Read(out m_maxNbrUsers); // removed m_maxNbrShopViewUsers
            a_reader.Read(out m_maxNbrUsers); // override removed ones with this
            a_reader.Read(out m_maxNbrPlants);

            // license info
            a_reader.Read(out m_serialCode);
            a_reader.Read(out m_companyId);
            a_reader.Read(out m_licenseId);
        }
        #endregion

        #region initial version
        else
        {
            m_bools = new BoolVector32(a_reader);

            int val;
            a_reader.Read(out val);
            m_systemIdType = (SystemIdTypes)val;

            a_reader.Read(out val);
            try
            {
                m_edition = (LicenseEdition)val;
            }
            catch
            {
                m_edition = LicenseEdition.Enterprise; // old keys can have a 4rd edition (SuperChain)
            }

            // expiration
            DateTime tmp;
            a_reader.Read(out tmp);
            ExpirationDate = DateTime.SpecifyKind(tmp, DateTimeKind.Utc);
            a_reader.Read(out m_expirationGraceDays);
            a_reader.Read(out tmp);
            MaintenanceExpirationDate = DateTime.SpecifyKind(tmp, DateTimeKind.Utc);
            a_reader.Read(out m_maintenanceGraceDays);

            // user limits
            a_reader.Read(out m_maxNbrUsers); // removed m_maxNbrMasterSchedulers
            a_reader.Read(out m_maxNbrUsers); // removed m_maxNbrWhatIfUsers
            a_reader.Read(out m_maxNbrUsers); // removed m_maxNbrViewOnlyUsers
            a_reader.Read(out m_maxNbrUsers); // removed m_maxNbrShopViewUsers
            a_reader.Read(out m_maxNbrUsers); // override removed ones with this
            a_reader.Read(out m_maxNbrPlants);
        }
        #endregion
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);

        a_writer.Write((int)m_systemIdType);
        a_writer.Write((int)m_edition);

        // expiration
        a_writer.Write(m_expirationDate);
        a_writer.Write(m_expirationGraceDays);
        a_writer.Write(m_maintenanceExpirationDate);
        a_writer.Write(m_maintenanceGraceDays);

        // limits
        a_writer.Write(m_maxNbrUsers);
        a_writer.Write(m_maxNbrPlants);

        // license info
        a_writer.Write(m_serialCode);
        a_writer.Write(m_companyId);
        a_writer.Write(m_licenseId);

        a_writer.Write(m_publicNotes);

        a_writer.Write(m_lastModifiedDateUTCTicks);

        SerializePackages(a_writer);
        SerializeOptions(a_writer);

        a_writer.Write((short)m_licenseStatus);
    }

    public const int UNIQUE_ID = 796;

    public int UniqueId => UNIQUE_ID;
    #endregion

    /// <summary>
    /// Default constructor. Loads properties from Xml Key file.
    /// </summary>
    public LicenseKeyObject()
    {
        Expirable = true; // set this here since it's stored in BoolVector32 and declaration can't have a default.
    }

    public enum ELicenseStatus
    {
        GoodStatus,
        Expired,
        MaintenanceExpired,
        PlantsExceeded,
        PlanningHorizonSpanExceeded,
        UsersExceeded,
        InvalidKey,
        UnableToLockKey,
        DataModelDivergence
    }

    public void Update(LicenseKeyObject a_keyObj)
    {
        SerialCode = a_keyObj.SerialCode;
        CompanyId = a_keyObj.CompanyId;
        LicenseId = a_keyObj.LicenseId;
        SystemIdType = a_keyObj.SystemIdType;
        TrialVersion = a_keyObj.TrialVersion;
        Edition = a_keyObj.Edition;

        // expiration
        Expirable = a_keyObj.Expirable;
        ExpirationDate = a_keyObj.ExpirationDate;
        ExpirationGraceDays = a_keyObj.ExpirationGraceDays;
        MaintenanceExpirationDate = a_keyObj.MaintenanceExpirationDate;
        MaintenanceGraceDays = MaintenanceGraceDays;

        // limits
        MaxNbrPlants = a_keyObj.MaxNbrPlants;
        MaxNbrUsers = a_keyObj.MaxNbrUsers;

        MaxPlanningHorizonDurationInDays = a_keyObj.MaxPlanningHorizonDurationInDays;

        // packages
        Packages = a_keyObj.Packages;

        Options = a_keyObj.Options;

        PublicNotes = a_keyObj.PublicNotes;

        LastModifiedDateUTC = a_keyObj.LastModifiedDateUTC;

        LicenseStatus = a_keyObj.LicenseStatus;
        DataModelActivationKey = a_keyObj.DataModelActivationKey;
        SDK = a_keyObj.SDK;
    }

    public bool KeyFileDoesNotExist
    {
        get => m_bools[c_keyFileDoesNotExist];
        protected set => m_bools[c_keyFileDoesNotExist] = value;
    }

    private string m_serialCode = "";

    public string SerialCode
    {
        get => m_serialCode;
        protected set => m_serialCode = value;
    }

    private string m_companyId = "";

    public string CompanyId
    {
        get => m_companyId;
        protected set => m_companyId = value;
    }

    private string m_licenseId = "";

    public string LicenseId
    {
        get => m_licenseId;
        protected set => m_licenseId = value;
    }

    private SystemIdTypes m_systemIdType = SystemIdTypes.LicenseService;

    public SystemIdTypes SystemIdType
    {
        get => m_systemIdType;
        protected set => m_systemIdType = value;
    }

    private LicenseEdition m_edition = LicenseEdition.Essentials;

    public LicenseEdition Edition
    {
        get
        {
            if (PublicNotes != null && PublicNotes.Contains("PTExpress Trial"))
            {
                return LicenseEdition.Express;
            }

            return m_edition;
        }
        protected set => m_edition = value;
    }

    public bool TrialVersion
    {
        get => m_bools[c_trialVerionIdx];
        protected set => m_bools[c_trialVerionIdx] = value;
    }

    private long m_lastModifiedDateUTCTicks;

    /// <summary>
    /// last time the key was modified.
    /// </summary>
    public DateTime LastModifiedDateUTC
    {
        get => new (m_lastModifiedDateUTCTicks);
        protected set => m_lastModifiedDateUTCTicks = value.Ticks;
    }

    public bool LoadedFromKey
    {
        get => m_bools[c_loadedFromKey];
        protected set => m_bools[c_loadedFromKey] = value;
    }

    private ELicenseStatus m_licenseStatus;

    public ELicenseStatus LicenseStatus
    {
        get => m_licenseStatus;
        set => m_licenseStatus = value;
    }

    public bool IncludeDetailedScheduling
    {
        get => m_bools[c_includeDetailedSchedulingIdx];
        protected set => m_bools[c_includeDetailedSchedulingIdx] = value;
    }

    public bool AllowImportLicense
    {
        get => m_bools[c_allowImportLicense];
        protected set => m_bools[c_allowImportLicense] = value;
    }

    private string m_dataModelActivationKey;

    public string DataModelActivationKey
    {
        get => m_dataModelActivationKey;
        protected set => m_dataModelActivationKey = value;
    }

    private bool m_sdk;

    public bool SDK
    {
        get => m_sdk;
        protected set => m_sdk = value;
    }

    #region Expiration
    public bool Expirable
    {
        get => m_bools[c_isExpirableIdx];
        protected set => m_bools[c_isExpirableIdx] = value;
    }

    private DateTime m_expirationDate = DateTime.MinValue;

    public DateTime ExpirationDate
    {
        get => m_expirationDate;
        protected set => m_expirationDate = value;
    }

    private int m_expirationGraceDays;

    public int ExpirationGraceDays
    {
        get => m_expirationGraceDays;
        protected set => m_expirationGraceDays = value;
    }

    /// <summary>
    /// returns expiration + grace days
    /// </summary>
    public DateTime TrueExpiration => ExpirationDate.Add(TimeSpan.FromDays(ExpirationGraceDays));

    private DateTime m_maintenanceExpirationDate = DateTime.MinValue;

    public DateTime MaintenanceExpirationDate
    {
        get => m_maintenanceExpirationDate;
        protected set => m_maintenanceExpirationDate = value;
    }

    private int m_maintenanceGraceDays;

    public int MaintenanceGraceDays
    {
        get => m_maintenanceGraceDays;
        protected set => m_maintenanceGraceDays = value;
    }

    /// <summary>
    /// Maintenance Expiration + Maintenance Grace days
    /// </summary>
    public DateTime TrueMaintenanceExpiration => MaintenanceExpirationDate.Add(TimeSpan.FromDays(MaintenanceGraceDays));
    #endregion

    #region Limits
    private int m_maxPlanningHorizonDurationInDays = 3650;

    public int MaxPlanningHorizonDurationInDays
    {
        get => m_maxPlanningHorizonDurationInDays;
        protected set => m_maxPlanningHorizonDurationInDays = value;
    }

    private int m_maxNbrUsers;

    public int MaxNbrUsers
    {
        get => m_maxNbrUsers;
        protected set => m_maxNbrUsers = value;
    }

    private int m_maxNbrPlants = 1;

    public int MaxNbrPlants
    {
        get => m_maxNbrPlants;
        protected set => m_maxNbrPlants = value;
    }

    public bool IncludeImport => Edition != LicenseEdition.Express;

    public bool IncludeExport => Edition != LicenseEdition.Express;
    #endregion

    #region Packages
    private Dictionary<int, PackageInfo> m_packages = new ();

    public Dictionary<int, PackageInfo> Packages
    {
        get => m_packages;
        protected set => m_packages = value;
    }

    private void SerializePackages(IWriter a_writer)
    {
        a_writer.Write(m_packages.Count);
        foreach (KeyValuePair<int, PackageInfo> p in m_packages)
        {
            p.Value.Serialize(a_writer);
        }
    }

    private void DeserializePackages(IReader a_reader, Dictionary<int, PackageInfo> a_packages)
    {
        if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out int packageCount);
            for (int i = 0; i < packageCount; i++)
            {
                PackageInfo p = new (a_reader);
                a_packages.Add(p.Id, p);
            }
        }
    }
    #endregion

    #region Options
    private Dictionary<string, string> m_options = new ();

    protected Dictionary<string, string> Options
    {
        get => m_options;
        set => m_options = value;
    }

    public LicenseOptions PackageLicenseOptions => new (m_options);

    private void DeserializeOptions(IReader a_reader)
    {
        string name;
        string value;
        int count;
        a_reader.Read(out count);
        for (int i = 0; i < count; i++)
        {
            a_reader.Read(out name);
            a_reader.Read(out value);
            m_options.Add(name, value);
        }
    }

    private void SerializeOptions(IWriter a_writer)
    {
        a_writer.Write(m_options.Count);
        foreach (KeyValuePair<string, string> o in m_options)
        {
            a_writer.Write(o.Key);
            a_writer.Write(o.Value);
        }
    }
    #endregion

    private string m_publicNotes;

    public string PublicNotes
    {
        get => m_publicNotes;
        protected set => m_publicNotes = value;
    }

    #region bool indices
    private BoolVector32 m_bools;
    private const int c_isExpirableIdx = 0;

    private const int c_trialVerionIdx = 1;

    //const int c_fallbackVerified = 2;
    //const int c_includeBufferManagementIdx = 3;
    //const int c_includeBottleneckSchedulingIdx = 4;
    //const int c_includeExpressMrpIdx = 5;
    //const int c_includeCoPilotIdx = 6;
    //const int c_includeAddinsIdx = 7;
    //const int c_includeKPIsIdx = 8;
    private const int c_keyFileDoesNotExist = 9;

    //const int c_includeProductivityOptimizationIdx = 10;
    //const int c_includeFinancialOptimizationIdx = 11;
    //const int c_includeCrossWarehousePlanning = 12;
    //const int c_includeCrossPlantPlanning = 13;
    //const int c_includeLotControlledPlannning = 14;
    //const int c_includeProcessFlow = 15;
    private const int c_loadedFromKey = 16;
    private const int c_includeDetailedSchedulingIdx = 17;
    private const int c_allowImportLicense = 18;
    #endregion
}

public class LicenseOptions : IEnumerable
{
    private readonly Dictionary<string, string> m_options;

    public Dictionary<string, string> Options => m_options;

    public LicenseOptions(Dictionary<string, string> a_optionsData)
    {
        m_options = a_optionsData;
    }

    public string GetLicenseData(string a_key)
    {
        if (m_options.TryGetValue(a_key, out string data))
        {
            return data;
        }

        return string.Empty;
    }

    public IEnumerator GetEnumerator()
    {
        foreach (KeyValuePair<string, string> keyValuePair in m_options)
        {
            yield return keyValuePair;
        }
    }
}