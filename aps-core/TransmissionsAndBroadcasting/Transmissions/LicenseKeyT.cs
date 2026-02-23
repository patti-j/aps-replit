namespace PT.Transmissions;

public class LicenseKeyT : PTTransmission
{
    private SchedulerDefinitions.LicenseKeyObject m_licenseKeyObj;

    public SchedulerDefinitions.LicenseKeyObject LicenseKeyObj
    {
        get => m_licenseKeyObj;
        private set => m_licenseKeyObj = value;
    }

    public LicenseKeyT() { }

    public LicenseKeyT(SchedulerDefinitions.LicenseKeyObject a_licenseKeyObj)
    {
        m_licenseKeyObj = a_licenseKeyObj;
    }

    public LicenseKeyT(IReader a_reader) : base(a_reader)
    {
        m_licenseKeyObj = new SchedulerDefinitions.LicenseKeyObject(a_reader);
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_licenseKeyObj.Serialize(writer);
    }

    public const int UNIQUE_ID = 820;

    public override int UniqueId => UNIQUE_ID;
}