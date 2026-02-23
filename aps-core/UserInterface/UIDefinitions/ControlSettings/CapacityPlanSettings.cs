using PT.PackageDefinitions;

namespace PT.UIDefinitions.ControlSettings;

/// <summary>
/// Stores a User's settings for displaying the Capacity Plan.
/// </summary>
public class CapacityPlanSettings : ISettingData
{
    #region IPTSerializable Members
    public CapacityPlanSettings()
    {
        SetDefaultCapacityPlanOptions(true);
    }

    public CapacityPlanSettings(IReader a_reader)
    {
        #region 600
        if (a_reader.VersionNumber >= 600)
        {
            a_reader.Read(out m_capacityType);
            a_reader.Read(out m_populateType);
            a_reader.Read(out m_commitmentType);
            a_reader.Read(out m_chartType);
            long bucketTicks = 0;
            a_reader.Read(out bucketTicks);
            m_bucketDuration = new TimeSpan(bucketTicks);
            a_reader.Read(out m_showLegend);
            a_reader.Read(out m_useGantt);
            a_reader.Read(out m_startSpanComboBox);
            a_reader.Read(out m_startSpanCustomDateTime);
            a_reader.Read(out m_endSpanComboBox);
            a_reader.Read(out m_endSpanCustomDateTime);
            a_reader.Read(out m_trackbarValue);
            a_reader.Read(out m_trackbarComboBox);
        }
        #endregion

        #region 520
        else if (a_reader.VersionNumber >= 520)
        {
            //This is the same as the 430 reader. Needed to fix an incompatibility with the release branch
            a_reader.Read(out m_capacityType);
            a_reader.Read(out m_populateType);
            a_reader.Read(out m_commitmentType);
            a_reader.Read(out m_chartType);
            long bucketTicks = 0;
            a_reader.Read(out bucketTicks);
            m_bucketDuration = new TimeSpan(bucketTicks);
            a_reader.Read(out m_showLegend);
        }
        #endregion

        #region 518
        else if (a_reader.VersionNumber >= 518)
        {
            a_reader.Read(out m_capacityType);
            a_reader.Read(out m_populateType);
            a_reader.Read(out m_commitmentType);
            a_reader.Read(out m_chartType);
            long bucketTicks = 0;
            a_reader.Read(out bucketTicks);
            m_bucketDuration = new TimeSpan(bucketTicks);
            a_reader.Read(out m_showLegend);
            a_reader.Read(out m_useGantt);
            a_reader.Read(out m_startSpanComboBox);
            a_reader.Read(out m_startSpanCustomDateTime);
            a_reader.Read(out m_endSpanComboBox);
            a_reader.Read(out m_endSpanCustomDateTime);
            a_reader.Read(out m_trackbarValue);
            a_reader.Read(out m_trackbarComboBox);
        }
        #endregion

        #region 430
        else if (a_reader.VersionNumber >= 430)
        {
            a_reader.Read(out m_capacityType);
            a_reader.Read(out m_populateType);
            a_reader.Read(out m_commitmentType);
            a_reader.Read(out m_chartType);
            long bucketTicks = 0;
            a_reader.Read(out bucketTicks);
            m_bucketDuration = new TimeSpan(bucketTicks);
            a_reader.Read(out m_showLegend);
            SetDefaultCapacityPlanOptions(false);
        }
        #endregion

        //New Update
        if (a_reader.VersionNumber < 678)
        {
            ChartType = 2; //Area chart is better for initial display.
        }
    }

    private void SetDefaultCapacityPlanOptions(bool a_setBucketDuration)
    {
        if (!a_setBucketDuration)
        {
            m_bucketDuration = new TimeSpan(7, 0, 0, 0);
        }

        StartSpanComboBox = 0;
        EndSpanComboBox = 3;
        UseGantt = true;
        TrackbarValue = 1;
        StartSpanCustomDateTime = PTDateTime.MinDateTime;
        EndSpanCustomDateTime = PTDateTime.MinDateTime;
        TrackbarComboBox = new TimeSpan(1, 0, 0, 0);
        ChartType = 2; //Area chart
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_capacityType);
        a_writer.Write(m_populateType);
        a_writer.Write(m_commitmentType);
        a_writer.Write(m_chartType);
        a_writer.Write(m_bucketDuration.Ticks);
        a_writer.Write(m_showLegend);
        a_writer.Write(m_useGantt);
        a_writer.Write(m_startSpanComboBox);
        a_writer.Write(m_startSpanCustomDateTime);
        a_writer.Write(m_endSpanComboBox);
        a_writer.Write(m_endSpanCustomDateTime);
        a_writer.Write(m_trackbarValue);
        a_writer.Write(m_trackbarComboBox);
    }
    #endregion

    //Set Default to 1 day for better initial loading peformance
    private TimeSpan m_bucketDuration = new (1, 0, 0, 0);

    /// <summary>
    /// The amount of time displayed in each interval.
    /// </summary>
    public TimeSpan BucketDuration
    {
        get => m_bucketDuration;
        set => m_bucketDuration = value;
    }

    private int m_capacityType;

    public int CapacityType
    {
        get => m_capacityType;
        set => m_capacityType = value;
    }

    private int m_populateType;

    public int PopulateType
    {
        get => m_populateType;
        set => m_populateType = value;
    }

    private int m_commitmentType;

    public int CommitmentType
    {
        get => m_commitmentType;
        set => m_commitmentType = value;
    }

    private int m_chartType;

    public int ChartType
    {
        get => m_chartType;
        set => m_chartType = value;
    }

    private bool m_showLegend;

    public bool ShowLegend
    {
        get => m_showLegend;
        set => m_showLegend = value;
    }

    private bool m_useGantt;

    public bool UseGantt
    {
        get => m_useGantt;
        set => m_useGantt = value;
    }

    private int m_startSpanComboBox;

    public int StartSpanComboBox
    {
        get => m_startSpanComboBox;
        set => m_startSpanComboBox = value;
    }

    private DateTime m_startSpanCustomDateTime;

    public DateTime StartSpanCustomDateTime
    {
        get => m_startSpanCustomDateTime;
        set => m_startSpanCustomDateTime = value;
    }

    private int m_endSpanComboBox;

    public int EndSpanComboBox
    {
        get => m_endSpanComboBox;
        set => m_endSpanComboBox = value;
    }

    private DateTime m_endSpanCustomDateTime;

    public DateTime EndSpanCustomDateTime
    {
        get => m_endSpanCustomDateTime;
        set => m_endSpanCustomDateTime = value;
    }

    private int m_trackbarValue;

    public int TrackbarValue
    {
        get => m_trackbarValue;
        set => m_trackbarValue = value;
    }

    private TimeSpan m_trackbarComboBox;

    public TimeSpan TrackbarComboBox
    {
        get => m_trackbarComboBox;
        set => m_trackbarComboBox = value;
    }

    #region ISettingData
    public int UniqueId => 1038;
    public string SettingKey => "workspace_CapacityPlanSettings";
    public string SettingCaption => "Capacity Plan Settings";
    public string Description => "Capacity Plan Settings";
    public string SettingsGroup => SettingGroupConstants.BoardsSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.CapacityPlanSettings;
    #endregion
}