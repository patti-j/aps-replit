namespace PT.ImportDefintions;

/// <summary>
/// Stores information about OleDb connections.
/// </summary>
[Serializable]
public class OleDbInfo : IPTSerializable
{
    public const int UNIQUE_ID = 377;

    #region IPTSerializable Members
    public OleDbInfo(IReader reader)
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

    public OleDbInfo() { }
}