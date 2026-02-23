namespace PT.Transmissions;

/// <summary>
/// Transmission for sending usersettings files to the server
/// NOTE: This class is no longer being used. It could be repurposed.
/// </summary>
public class UserSettingsT : UserIdBaseT
{
    public static readonly int UNIQUE_ID = 797;

    #region IPTSerializable Members
    public UserSettingsT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 500)
        {
            a_reader.Read(out m_zippedBytes);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(m_zippedBytes);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    //public UserSettingsT(BaseId a_userId, string a_localFileDir)
    //    : base(a_userId)
    //{
    //    sequenced = false;
    //    CreateZip(a_localFileDir, a_userId.Value.ToString());
    //}

    public UserSettingsT() { }
    private readonly byte[] m_zippedBytes;

    //public byte[] ZipFileBytes
    //{
    //    get { return m_zippedBytes; }
    //}

    //private void CreateZip(string a_directoryToZip, string a_userId)
    //{
    //    string targetDir = Path.Combine(a_directoryToZip, a_userId);
    //    string tempDir = Path.Combine(a_directoryToZip, "Backup" + a_userId + ".zip");
    //    if (File.Exists(tempDir))
    //    {
    //        File.Delete(tempDir);
    //    }
    //    ZipFile.CreateFromDirectory(targetDir, tempDir);
    //    m_zippedBytes = File.ReadAllBytes(tempDir);
    //}
}