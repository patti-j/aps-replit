using PT.PackageDefinitions;

namespace PT.PackageDefinitionsUI.UserSettings;

public class NotificationsSettings : ISettingData, ICloneable
{
    public NotificationsSettings()
    {
        HideSimBarAutomatically = true;
    }

    public NotificationsSettings(IReader a_reader)
    {
        m_bools = new BoolVector32(a_reader);
        a_reader.Read(out int val);
        NotificationsBarOpenType = (ENotificationsBarOpenType)val;

        a_reader.Read(out int val2);
        FadingTimer = val2;
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
        a_writer.Write((int)m_notificationsBarOpenType);
        a_writer.Write(m_fadingTimer);
    }

    private BoolVector32 m_bools;
    private const int c_hideBarAutomaticallyIdx = 0;

    public enum ENotificationsBarOpenType { OpenAutomatically, OpenWithFlyoutButton, NeverOpen }

    #region Property Accessors
    public bool HideSimBarAutomatically
    {
        get => m_bools[c_hideBarAutomaticallyIdx];
        set => m_bools[c_hideBarAutomaticallyIdx] = value;
    }

    private int m_fadingTimer = 10;

    public int FadingTimer
    {
        get => m_fadingTimer;
        set => m_fadingTimer = value;
    }

    private ENotificationsBarOpenType m_notificationsBarOpenType = ENotificationsBarOpenType.OpenAutomatically;

    public ENotificationsBarOpenType NotificationsBarOpenType
    {
        get => m_notificationsBarOpenType;
        set => m_notificationsBarOpenType = value;
    }
    #endregion

    public int UniqueId => 1002;
    public string SettingKey => "UserSettings_Notifications";
    public string Description => "Global Notifications settings";
    public string SettingsGroup => SettingGroupConstants.LayoutSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.Notifications;
    public string SettingCaption => "General notification settings";

    object ICloneable.Clone()
    {
        return Clone();
    }

    public NotificationsSettings Clone()
    {
        return (NotificationsSettings)MemberwiseClone();
    }
}