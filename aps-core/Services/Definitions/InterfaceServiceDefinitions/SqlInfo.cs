namespace PT.ImportDefintions;

/// <summary>
/// Store information about SQL Server connections.
/// </summary>
[Serializable]
public class SqlInfo : IPTSerializable
{
    public const int UNIQUE_ID = 374;

    #region IPTSerializable Members
    public SqlInfo(IReader reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public SqlInfo()
    {
        //
        // TODO: Add constructor logic here
        //
    }
}