namespace PT.Transmissions;

/// <summary>
/// Transmission to update which packages are disabled.
/// </summary>
public class PackageStateT : PTTransmission
{
    public override int UniqueId => 905;
    private readonly HashSet<int> m_disabledPackages;
    public HashSet<int> DisabledPackages => m_disabledPackages;

    public PackageStateT() { }

    public PackageStateT(IReader a_reader) : base(a_reader)
    {
        a_reader.Read(out int count);
        for (int i = 0; i < count; i++)
        {
            a_reader.Read(out int id);
            m_disabledPackages.Add(id);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(m_disabledPackages.Count);
        foreach (int id in m_disabledPackages)
        {
            a_writer.Write(id);
        }
    }

    public PackageStateT(HashSet<int> a_disabledPackageIds)
    {
        m_disabledPackages = a_disabledPackageIds ?? new HashSet<int>();
    }
}