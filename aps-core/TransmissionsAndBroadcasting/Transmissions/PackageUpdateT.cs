namespace PT.Transmissions;

/// <summary>
/// Transmission for automatic updates to packages.
/// </summary>
public class PackageUpdateT : PTTransmission
{
    public const int UNIQUE_ID = 932;

    public override int UniqueId => UNIQUE_ID;
    public List<PackageUpdateData> UpdatePackages { get; } = new ();

    public PackageUpdateT() { }

    public PackageUpdateT(IReader a_reader) : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                UpdatePackages.Add(new PackageUpdateData(a_reader));
            }
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(UpdatePackages.Count);
        foreach (PackageUpdateData updatePackage in UpdatePackages)
        {
            updatePackage.Serialize(a_writer);
        }
    }

    public PackageUpdateT(List<PackageUpdateData> a_updatePackages)
    {
        UpdatePackages = a_updatePackages ?? new List<PackageUpdateData>();
    }
}

public class PackageUpdateData : SchedulerDefinitions.PackageInfoExtended
{
    public byte[] PackageBytes { get; set; }
    public bool PackageDeletion { get; set; }

    public PackageUpdateData(int a_id, string a_name, bool a_trial, Version a_version, byte[] a_bytes, bool a_packageDeletion)
        : base(a_id, a_name, a_trial, a_version)
    {
        PackageBytes = a_bytes;
        PackageDeletion = a_packageDeletion;
    }

    public PackageUpdateData(IReader a_reader) : base(a_reader)
    {
        a_reader.Read(out byte[] bytes);
        a_reader.Read(out bool deletion);
        PackageBytes = bytes;
        PackageDeletion = deletion;
    }

    public new void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(PackageBytes);
        a_writer.Write(PackageDeletion);
    }
}