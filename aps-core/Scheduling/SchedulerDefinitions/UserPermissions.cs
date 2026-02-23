namespace PT.SchedulerDefinitions;

/// <summary>
/// For backwards compatibility with user permission serialization.
/// </summary>
public class UserPermissions
{
    public UserPermissions(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 611)
        {
            int dictCount;
            a_reader.Read(out dictCount);
            for (int i = 0; i < dictCount; i++)
            {
                a_reader.Read(out short permission);
                a_reader.Read(out short accessLevel);
            }
        }
    }

    public int UniqueId => UNIQUE_ID;

    public const int UNIQUE_ID = 812;
}