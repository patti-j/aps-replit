using PT.PackageDefinitions;

namespace PT.UIDefinitions.ControlSettings;

public class DatabaseManagerSettings : ISettingData
{
    private string m_connectionString;
    private BoolVector32 m_bools;

    public DatabaseManagerSettings() { }

    #region IPTSerializable Members
    public DatabaseManagerSettings(IReader a_reader)
    {
        #region 622
        if (a_reader.VersionNumber >= 622)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_connectionString);
        }
        #endregion

        #region 617
        else if (a_reader.VersionNumber >= 617)
        {
            m_bools = new BoolVector32(a_reader);
            string unused;
            a_reader.Read(out unused);
            a_reader.Read(out unused);
        }
        #endregion
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
        a_writer.Write(m_connectionString);
    }

    public int UniqueId => 0; //TODO
    #endregion

    public string ConnectionString
    {
        get => m_connectionString;
        set => m_connectionString = value;
    }

    public string SettingKey => "DatabaseManagerSettings";
    public string SettingCaption => "Database Manager Settings";
    public string Description => "Stores user and layout settings for the database manager";
    public string SettingsGroup => SettingGroupConstants.BoardsSettingsGroup;
    public string SettingsGroupCategory => "Database Manager";
}