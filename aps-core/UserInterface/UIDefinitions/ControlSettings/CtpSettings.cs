using PT.PackageDefinitions;
using PT.SchedulerDefinitions;

namespace PT.UIDefinitions.ControlSettings;

public class CtpSettings : ISettingData
{
    #region IPTSerializable Members
    public CtpSettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 640)
        {
            a_reader.Read(out m_schedulingType);
        }
        else if (a_reader.VersionNumber >= 607)
        {
            string schedulingType;
            a_reader.Read(out schedulingType);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_schedulingType);
    }

    public int UniqueId => 0; //TODO:
    #endregion

    private int m_schedulingType;

    public CtpSettings()
    {
        SchedulingType = CtpDefs.ESchedulingType.ExpediteToJIT;
    }

    public CtpDefs.ESchedulingType SchedulingType
    {
        get => (CtpDefs.ESchedulingType)m_schedulingType;
        set => m_schedulingType = (int)value;
    }

    public string SettingKey => "CtpSettings";
    public string SettingCaption => "CTP Settings";
    public string Description => "Stores previous CTP data";
    public string SettingsGroup => "CTP";
    public string SettingsGroupCategory => "CTP";
}