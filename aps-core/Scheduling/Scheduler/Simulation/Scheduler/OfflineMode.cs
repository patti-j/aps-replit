using PT.APSCommon;

namespace PT.Scheduler;

/// <summary>
/// Keeps track of Offline Mode status and information.
/// </summary>
public class OnlineMode : IPTSerializable
{
    public const int UNIQUE_ID = 727;

    #region IPTSerializable Members
    public OnlineMode(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out m_lastSetOffline);
            m_userWhoLastSetOffline = new BaseId(reader);
            reader.Read(out m_online);
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(m_lastSetOffline);
        m_userWhoLastSetOffline.Serialize(writer);
        writer.Write(m_online);
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    internal OnlineMode() { }

    private bool m_online = true;

    /// <summary>
    /// Whether the Scenario is Online.
    /// </summary>
    public bool Online
    {
        get => m_online;
        private set => m_online = value;
    }

    private BaseId m_userWhoLastSetOffline = BaseId.NULL_ID;

    internal BaseId UserWhoLastSetOffline
    {
        get => m_userWhoLastSetOffline;
        private set => m_userWhoLastSetOffline = value;
    }

    private DateTime m_lastSetOffline;

    internal DateTime LastSetOffline
    {
        get => m_lastSetOffline;
        private set => m_lastSetOffline = value;
    }

    internal void SetOffline()
    {
        Online = false;
    }

    internal void SetOnline()
    {
        Online = true;
    }

    internal void SetOffline(Transmissions.ScenarioDetailOfflineT a_t)
    {
        UserWhoLastSetOffline = a_t.Instigator;
        LastSetOffline = a_t.TimeStamp.ToDateTime();
        Online = false;
    }

    internal void SetOnline(Transmissions.ScenarioDetailOnlineT a_t)
    {
        Online = true;
    }
}