using PT.APSCommon;
using PT.Common.Exceptions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// This exception indicates that the authenticity of key file could not be verified.
/// </summary>
internal class LicenseKeyException : PTException
{
    internal LicenseKeyException(string a_message)
        : base(a_message) { }
}

/// <summary>
/// Class representing a license key. Includes information on edition, expiration limitations and enabled options.
/// Added on Version 499
/// </summary>
public class LicenseKey : LicenseKeyObject
{
    private const string c_incorrectLicenseIdTypeTag = "License Service";
    private LicenseKeyValidationHelper m_licenseValidator;

    [Obsolete("Replace with packages")]
    public bool IncludeKPIs => true;

    [Obsolete("Replace with packages")]
    public bool IncludeCoPilot => true;

    [Obsolete("Replace with packages")]
    public bool IncludeCrossWarehousePlanning => true;

    [Obsolete("Replace with packages")]
    public bool IncludeExpressMRP => true;

    [Obsolete("Replace with packages")]
    public bool IncludeLotControlledPlanning => true;

    [Obsolete("Replace with packages")]
    public bool IncludeCrossPlantPlanning => true;

    [Obsolete("Replace with packages")]
    public bool IncludeBufferManagement => true;

    [Obsolete("Replace with packages")]
    public bool IncludeAddins => true;

    [Obsolete("Replace with packages")]
    public bool IncludeBottleneckScheduling => true;

    [Obsolete("Replace with packages")]
    public bool IncludeProductivityOptimization => true;

    [Obsolete("Replace with packages")]
    public bool IncludeFinancialOptimization => true;

    [Obsolete("Replace with packages")]
    public bool IncludeProcessFlow => true;

    public LicenseKey(IReader a_reader) : base(a_reader) { }

    public LicenseKey() { }

    /// <summary>
    /// Verifies authenticity of key and sets limits from key.
    /// </summary>
    internal void LoadFromKeyFile(string a_serialCode, LicenseKeyValidationHelper a_licenseValidator)
    {
        m_licenseValidator = a_licenseValidator;
        Packages.Clear();

        try
        {
            if (!File.Exists(KeyFilePaths.ConfigPath))
            {
                throw new PTException($"No license found. Searched for keyfile at: {KeyFilePaths.ConfigPath}");
            }
            LicenseKeyJson licenseKeyJson = LicenseKeyJson.DeserializeToObject(KeyFilePaths.ConfigPath);
            LoadFromJsonObject(a_serialCode, licenseKeyJson);
        }
        catch (Exception e)
        {
            m_licenseValidator.LogLicensingException(e, "Unable to load License File");

            KeyFileDoesNotExist = true;
            throw;
        }
    }

    public void LoadFromJsonObject(string a_serialCode, LicenseKeyJson a_key)
    {
        SystemIdTypes sysIdType;
        Packages.Clear();
        //In case we are reloading the key
        if (a_key.SystemIdType == c_incorrectLicenseIdTypeTag)
        {
            sysIdType = SystemIdTypes.LicenseService;
        }
        else
        {
            sysIdType = (SystemIdTypes)Enum.Parse(typeof(SystemIdTypes), a_key.SystemIdType, true);
        }

        if (!PTSystem.RunningMassRecordings)
        {
            //Workaround to enable offline activation with DataModelActivationKey. This must be parsed before VerifyKeyAuthenticity.
            try
            {
                DataModelActivationKey = ParseDMAElement(PublicNotes ?? "");
            }
            catch (Exception e)
            {
                throw new Exception("791: The key could not be loaded");
            }
        }

        // now set variables from key file
        PublicNotes = a_key.Notes1;
        SerialCode = a_key.SerialCode;
        CompanyId = a_key.Customer;
        LicenseId = a_key.LicenseId;
        SystemIdType = sysIdType;
        TrialVersion = a_key.TrialVersion;

        // expiration
        Expirable = a_key.Expirable;
        ExpirationDate = DateTime.SpecifyKind(a_key.ExpirationDate, DateTimeKind.Utc);
        ExpirationGraceDays = a_key.LicenseGraceDays;
        MaintenanceExpirationDate = DateTime.SpecifyKind(a_key.MaintenanceExpiration, DateTimeKind.Utc);
        MaintenanceGraceDays = a_key.MaintenanceGraceDays;

        // limits
        MaxNbrUsers = a_key.MaxNbrFlexUsers;
        try
        {
            // V12 moved from the legacy v12 MaxNbrPlant setting to the v12 Options.
            string v12MaxPlantOptionVal = a_key.Options.FirstOrDefault(opt => opt.Key == "Plants").Value;
            MaxNbrPlants = Int32.Parse(v12MaxPlantOptionVal);
        }
        catch
        {
            // The v12 option above is a required part of a license key, so this should never be hit.
            throw new LicenseKeyException("The Plants value has not been properly configured for the License's V12 Options.");
        }

        //Workaround to enable Detailed Scheduling for older versions without updating the license service.
        //Detailed Scheduling is included by default. Add "PTPlanning" to the public notes field to disable it.
        try
        {
            IncludeDetailedScheduling = string.IsNullOrEmpty(PublicNotes) || !PublicNotes.Contains("PTPlanning");
        }
        catch (Exception e)
        {
            //Not sure if GetElementText will get an error if the notes field is null
            IncludeDetailedScheduling = true;
        }

        //License field to disable import feature
        try
        {
            AllowImportLicense = string.IsNullOrEmpty(PublicNotes) || !PublicNotes.Contains("ImportLicenseDisabled");
        }
        catch (Exception e)
        {
            //Not sure if GetElementText will get an error if the notes field is null
            AllowImportLicense = true;
        }

        GetPackagesFromKeyFile(a_key);

        Options = a_key.Options;

        LastModifiedDateUTC = DateTime.SpecifyKind(a_key.LastKeyFieldModificationUTC, DateTimeKind.Utc);

        MaxPlanningHorizonDurationInDays = 3650; // this should probably come from a package ??

        SDK = !string.IsNullOrEmpty(PublicNotes) && PublicNotes.Contains("SDK");

        // verify key file before applying values
        m_licenseValidator?.VerifyKeyAuthenticity(a_serialCode, sysIdType, a_key, !string.IsNullOrEmpty(DataModelActivationKey));

        LoadedFromKey = true;
    }

    internal const string c_dmaKey = "dmakey";

    public static string ParseDMAElement(string a_string)
    {
        int dmaEndIndex = a_string.ToLower().IndexOf(c_dmaKey);
        if (dmaEndIndex == -1)
        {
            return "";
        }

        int firstDelimiterIndex = dmaEndIndex + c_dmaKey.Length;
        if (a_string[firstDelimiterIndex] != '|')
        {
            throw new Exception();
        }

        int secondDelimiterIndex = a_string.IndexOf('|', firstDelimiterIndex + 1);
        if (secondDelimiterIndex == -1)
        {
            throw new Exception();
        }

        return a_string.Substring(firstDelimiterIndex + 1, secondDelimiterIndex - firstDelimiterIndex - 1);
    }

    //This is called early in UI because UI needs these early
    public void GetPackagesFromKeyFile(LicenseKeyJson a_key)
    {
        Packages.Clear();
        foreach (PackageInfo lp in a_key.Packages)
        {
            Packages.Add(lp.Id, lp);
        }
    }

    /// <summary>
    /// Returns serial code with dashes so it's easier on the eyes.
    /// </summary>
    /// <returns></returns>
    public string GetDisplaySerialCode()
    {
        string displaySerialCode = SerialCode;

        if (!string.IsNullOrEmpty(displaySerialCode) && !displaySerialCode.Contains("-"))
        {
            displaySerialCode = displaySerialCode.Insert(4, "-");
            displaySerialCode = displaySerialCode.Insert(9, "-");

            return displaySerialCode;
        }

        return displaySerialCode;
    }

    /// <summary>
    /// Returns a placeholder serial code with dashes.
    /// </summary>
    /// <returns></returns>
    public string GetPlaceholderSerialCode()
    {
        return "____-____-____";
    }

    private DateTime ReadCurrentLastModifiedDate()
    {
        LicenseKeyJson licenseKeyJson;
        try
        {
            licenseKeyJson = LicenseKeyJson.DeserializeToObject(KeyFilePaths.ConfigPath);
        }
        catch
        {
            KeyFileDoesNotExist = true;
            throw;
        }

        return DateTime.SpecifyKind(licenseKeyJson.LastKeyFieldModificationUTC, DateTimeKind.Utc);
    }

    /// <summary>
    /// returns true if the last modified date on the pt.xml is larger now than when this key was loaded first.
    /// </summary>
    /// <returns></returns>
    internal bool IsOutOfDate()
    {
        return LastModifiedDateUTC < ReadCurrentLastModifiedDate();
    }
}