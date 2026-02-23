namespace PT.ImportDefintions;

/// <summary>
/// Stores information about Oracle connections.
/// </summary>
[Serializable]
public class OracleInfo : IPTSerializable
{
    public const int UNIQUE_ID = 378;

    #region IPTSerializable Members
    public OracleInfo(IReader reader)
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

    public OracleInfo() { }
}