namespace PT.Transmissions;

/// <summary>
/// Base object for updating a ShopViewResourceOption.
/// </summary>
public class ShopViewSystemOptionsUpdateT : ScenarioBaseT, IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 506;

    public ShopViewSystemOptionsUpdateT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 705)
        {
            reader.Read(out autoLogout);
            reader.Read(out autoLogoutSpan);
            reader.Read(out usePasswords);
            reader.Read(out usePublishedScenario);
            reader.Read(out autoRefreshSpan);
        }
        else
        {
            reader.Read(out autoLogout);
            reader.Read(out autoLogoutSpan);
            reader.Read(out usePasswords);
            reader.Read(out usePublishedScenario);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(autoLogout);
        writer.Write(autoLogoutSpan);
        writer.Write(usePasswords);
        writer.Write(usePublishedScenario);
        writer.Write(autoRefreshSpan);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ShopViewSystemOptionsUpdateT()
    {
    }

    public bool autoLogout;
    public TimeSpan autoLogoutSpan;
    public TimeSpan autoRefreshSpan;
    public bool usePasswords;
    public bool usePublishedScenario;
}