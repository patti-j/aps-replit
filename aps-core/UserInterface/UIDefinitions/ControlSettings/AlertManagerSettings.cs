namespace PT.UIDefinitions.ControlSettings;

public class AlertManagerSettings
{
    #region IPTSerializable Members
    public AlertManagerSettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 607)
        {
            int displayInt;
            a_reader.Read(out displayInt);
            DisplayType = (EDisplayMethod)displayInt;
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write((int)m_displayType);
    }
    #endregion

    private EDisplayMethod m_displayType;

    public AlertManagerSettings()
    {
        m_displayType = EDisplayMethod.Active;
    }

    public EDisplayMethod DisplayType
    {
        get => m_displayType;
        set => m_displayType = value;
    }

    public enum EDisplayMethod { All, Active, None }
}