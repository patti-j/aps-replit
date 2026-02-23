using Newtonsoft.Json;

namespace PT.SchedulerDefinitions;

/**
 * This file contains DataContracts used in Key and KeyMaintenance Services (license service).
 * It is referenced as linked file in the KeyGeneration solution.
 */
public class LicenseKeyJson
{
    public string Customer { get; set; }

    public string LicenseId { get; set; }

    public string SerialCode { get; set; }

    public string SystemIdType { get; set; }

    public string SystemId { get; set; }

    public bool TrialVersion { get; set; }

    public bool Expirable { get; set; }

    public DateTime ExpirationDate { get; set; }

    public int LicenseGraceDays { get; set; }

    public DateTime MaintenanceExpiration { get; set; }

    public int MaintenanceGraceDays { get; set; }

    [Obsolete("Legacy V11 value. Use the 'Plants' value in Options instead when mapping to the LicenseKey class.")]
    public int MaxNbrPlants { get; set; }

    public int MaxNbrFlexUsers { get; set; }

    public List<PackageInfo> Packages { get; set; }

    public string Notes1 { get; set; }

    public string KeyGenNotes { get; set; }

    public DateTime KeyGenerationDateUTC { get; set; }

    public DateTime LastKeyFieldModificationUTC { get; set; }

    public Dictionary<string, string> Options { get; set; }

    // default constructor

    public string SerializeToJson()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static LicenseKeyJson DeserializeToObjectFromString(string a_jsonString)
    {
        return JsonConvert.DeserializeObject<LicenseKeyJson>(a_jsonString);
    }

    public static LicenseKeyJson DeserializeToObject(string a_jsonFilePath)
    {
        return JsonConvert.DeserializeObject<LicenseKeyJson>(File.ReadAllText(a_jsonFilePath));
    }
}

/// <summary>
/// This class is used by LicenseKey to store the packages that have
/// been granted access to customer. This class is also a DataContract
/// used by KeyGeneration.sln
/// </summary>
public class PackageInfo : IPTSerializable
{
    public int UniqueId => -1;

    public PackageInfo(IReader a_reader)
    {
        int id;
        string name;
        bool trial = false;
        //if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out id);
            a_reader.Read(out name);
            a_reader.Read(out trial);
        }
        Id = id;
        Name = name;
        Trial = trial;
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(Id);
        a_writer.Write(Name);
        a_writer.Write(Trial);
    }

    public PackageInfo(int a_id, string a_name, bool a_trial)
    {
        Id = a_id;
        Name = a_name;
        Trial = a_trial;
    }

    public int Id { get; set; }

    public string Name { get; set; }

    public bool Trial { get; set; }

    public PackageInfo() { } // default ctor
}

/// <summary>
/// This class extends PackageInfo to store additional information needed
/// for loading and maintaining packages, such as version.
/// </summary>
public class PackageInfoExtended : PackageInfo
{
    public PackageInfoExtended(int a_id, string a_name, bool a_trial, Version a_version) : base(a_id, a_name, a_trial)
    {
        Version = a_version;
    }

    public PackageInfoExtended(IReader a_reader) : base(a_reader)
    {
        string ver;
        a_reader.Read(out ver);
        Version = Version.Parse(ver);
    }

    public new void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(Version.ToString());
    }

    public Version Version { get; set; }
}