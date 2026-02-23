using PT.SchedulerDefinitions;

namespace PT.Transmissions;

public class SystemStateSwitchT : PTTransmission, IPTSerializable
{
    #region IPTSerializable
    public const int UNIQUE_ID = 764;

    public SystemStateSwitchT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out m_makeReadOnly);
            a_reader.Read(out short licenseStatus);

            m_licenseStatus = (LicenseKeyObject.ELicenseStatus)licenseStatus;
        }
        else if (a_reader.VersionNumber >= 433)
        {
            a_reader.Read(out m_makeReadOnly);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(m_makeReadOnly);
        writer.Write((short)m_licenseStatus);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private bool m_makeReadOnly;

    /// <summary>
    /// If true, change PTBroadcaster state to Read-Only.
    /// </summary>
    public bool MakeReadOnly
    {
        get => m_makeReadOnly;
        set => m_makeReadOnly = value;
    }

    private LicenseKeyObject.ELicenseStatus m_licenseStatus;

    public LicenseKeyObject.ELicenseStatus LicenseStatus
    {
        get => m_licenseStatus;
        set => m_licenseStatus = value;
    }

    public SystemStateSwitchT() { }

    public SystemStateSwitchT(bool a_makeReadOnly, LicenseKeyObject.ELicenseStatus a_licenseStatus)
    {
        m_makeReadOnly = a_makeReadOnly;
        m_licenseStatus = a_licenseStatus;
    }
}