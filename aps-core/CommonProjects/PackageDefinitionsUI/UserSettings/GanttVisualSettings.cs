using System.Drawing;

using PT.PackageDefinitions;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.PackageDefinitionsUI.UserSettings;

public class GanttVisualSettings : ISettingData, ICloneable
{
    public GanttVisualSettings() { }

    public GanttVisualSettings(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12561)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_fixedRowStdHeight);
            a_reader.Read(out m_sortColumn);
            a_reader.Read(out m_gridWidth);

            a_reader.Read(out int count);

            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out int width);
                m_gridColumnWidthList.Add(width);
            }

            a_reader.Read(out m_calendarGanttSpan);
            a_reader.Read(out m_tooltipLocationX);
            a_reader.Read(out m_tooltipLocationY);

            a_reader.Read(out int ganttViewStyle);
            m_ganttViewStyle = (GanttViewStyles)ganttViewStyle;
            a_reader.Read(out int highlightLinkType);
            m_highlightLinkType = (HighlightLinkTypes)highlightLinkType;

            a_reader.Read(out int defaultZoomType);
            m_defaultZoomLevel = (zoomLevels)defaultZoomType;

            m_jobWatches = new BaseIdList(a_reader);

            a_reader.Read(out m_plantsTabTextOrientation);
            a_reader.Read(out m_plantsTabLayout);
            a_reader.Read(out m_plantsTabOrientation);
            a_reader.Read(out m_activePlantId);

            a_reader.Read(out m_currentPlantInGanttId);
            a_reader.Read(out m_deptsTabTextOrientation);
            a_reader.Read(out m_deptsTabLayout);
            a_reader.Read(out m_deptsTabOrientation);
            a_reader.Read(out m_activeDepartmentId);
        }
        else if (a_reader.VersionNumber >= 12535)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_fixedRowStdHeight);
            a_reader.Read(out m_sortColumn);
            a_reader.Read(out m_gridWidth);

            a_reader.Read(out int count);

            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out int width);
                m_gridColumnWidthList.Add(width);
            }

            a_reader.Read(out m_calendarGanttSpan);
            a_reader.Read(out m_tooltipLocationX);
            a_reader.Read(out m_tooltipLocationY);
            a_reader.Read(out int _);
            a_reader.Read(out int _);
            a_reader.Read(out int _);// added in v12531
            a_reader.Read(out int _);// added in v12535

            a_reader.Read(out int ganttViewStyle);
            m_ganttViewStyle = (GanttViewStyles)ganttViewStyle;
            a_reader.Read(out int highlightLinkType);
            m_highlightLinkType = (HighlightLinkTypes)highlightLinkType;

            a_reader.Read(out int defaultZoomType);
            m_defaultZoomLevel = (zoomLevels)defaultZoomType;

            m_jobWatches = new BaseIdList(a_reader);

            a_reader.Read(out m_plantsTabTextOrientation);
            a_reader.Read(out m_plantsTabLayout);
            a_reader.Read(out m_plantsTabOrientation);
            a_reader.Read(out m_activePlantId);

            a_reader.Read(out m_currentPlantInGanttId);
            a_reader.Read(out m_deptsTabTextOrientation);
            a_reader.Read(out m_deptsTabLayout);
            a_reader.Read(out m_deptsTabOrientation);
            a_reader.Read(out m_activeDepartmentId);
        }
        else if (a_reader.VersionNumber >= 12534)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_fixedRowStdHeight);
            a_reader.Read(out m_sortColumn);
            a_reader.Read(out m_gridWidth);

            a_reader.Read(out int count);

            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out int width);
                m_gridColumnWidthList.Add(width);
            }

            a_reader.Read(out m_calendarGanttSpan);
            a_reader.Read(out m_tooltipLocationX);
            a_reader.Read(out m_tooltipLocationY);
            a_reader.Read(out int _);
            a_reader.Read(out int _);
            a_reader.Read(out int _);// added in v12531

            a_reader.Read(out int ganttViewStyle);
            m_ganttViewStyle = (GanttViewStyles)ganttViewStyle;
            a_reader.Read(out int highlightLinkType);
            m_highlightLinkType = (HighlightLinkTypes)highlightLinkType;

            a_reader.Read(out int defaultZoomType);
            m_defaultZoomLevel = (zoomLevels)defaultZoomType;

            m_jobWatches = new BaseIdList(a_reader);

            a_reader.Read(out m_plantsTabTextOrientation);
            a_reader.Read(out m_plantsTabLayout);
            a_reader.Read(out m_plantsTabOrientation);
            a_reader.Read(out m_activePlantId);

            a_reader.Read(out m_currentPlantInGanttId);
            a_reader.Read(out m_deptsTabTextOrientation);
            a_reader.Read(out m_deptsTabLayout);
            a_reader.Read(out m_deptsTabOrientation);
            a_reader.Read(out m_activeDepartmentId);
        }
        else
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_fixedRowStdHeight);
            a_reader.Read(out m_sortColumn);
            a_reader.Read(out m_gridWidth);

            a_reader.Read(out int count);

            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out int width);
                m_gridColumnWidthList.Add(width);
            }

            a_reader.Read(out m_calendarGanttSpan);
            a_reader.Read(out m_tooltipLocationX);
            a_reader.Read(out m_tooltipLocationY);
            a_reader.Read(out int _);
            a_reader.Read(out int _);

            a_reader.Read(out int ganttViewStyle);
            m_ganttViewStyle = (GanttViewStyles)ganttViewStyle;
            a_reader.Read(out int highlightLinkType);
            m_highlightLinkType = (HighlightLinkTypes)highlightLinkType;

            a_reader.Read(out int defaultZoomType);
            m_defaultZoomLevel = (zoomLevels)defaultZoomType;

            m_jobWatches = new BaseIdList(a_reader);

            a_reader.Read(out m_plantsTabTextOrientation);
            a_reader.Read(out m_plantsTabLayout);
            a_reader.Read(out m_plantsTabOrientation);
            a_reader.Read(out m_activePlantId);

            a_reader.Read(out m_currentPlantInGanttId);
            a_reader.Read(out m_deptsTabTextOrientation);
            a_reader.Read(out m_deptsTabLayout);
            a_reader.Read(out m_deptsTabOrientation);
            a_reader.Read(out m_activeDepartmentId);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);

        a_writer.Write(m_fixedRowStdHeight);
        a_writer.Write(m_sortColumn);
        a_writer.Write(m_gridWidth);

        a_writer.Write(m_gridColumnWidthList.Count);
        foreach (int w in m_gridColumnWidthList)
        {
            a_writer.Write(w);
        }

        a_writer.Write(m_calendarGanttSpan);
        a_writer.Write(m_tooltipLocationX);
        a_writer.Write(m_tooltipLocationY);

        a_writer.Write((int)m_ganttViewStyle);
        a_writer.Write((int)m_highlightLinkType);
        a_writer.Write((int)m_defaultZoomLevel);

        m_jobWatches.Serialize(a_writer);

        a_writer.Write(m_plantsTabTextOrientation);
        a_writer.Write(m_plantsTabLayout);
        a_writer.Write(m_plantsTabOrientation);
        a_writer.Write(m_activePlantId);

        a_writer.Write(m_currentPlantInGanttId);
        a_writer.Write(m_deptsTabTextOrientation);
        a_writer.Write(m_deptsTabLayout);
        a_writer.Write(m_deptsTabOrientation);
        a_writer.Write(m_activeDepartmentId);
    }

    public GanttVisualSettings(ScheduleViewerSettings a_settings)
    {
        TipsVisible = a_settings.TipsVisible;
        HintsVisible = a_settings.HintsVisible;
        ShowHighlighting = a_settings.ShowHighlighting;
        AutoHighlight = a_settings.AutoHighlight;
        ResizeRowsToFit = a_settings.ResizeRowsToFit;
        SortDescending = a_settings.SortDescending;
        TooltipPinned = a_settings.TooltipPinned;
        FixedRowStdHeight = a_settings.FixedRowStandardHeight;
        SortColumn = a_settings.SortColumn;
        GridWidth = a_settings.GridWidth;
        GridColumnWidthList = a_settings.GridColumnWidthList;
        CalendarGanttSpan = a_settings.CalendarGanttSpan;
        TooltipLocation = a_settings.TooltipLocation;

        switch (a_settings.GanttViewStyle)
        {
            case ScheduleViewerSettings.ganttViewStyles.Resources:
                GanttViewStyle = GanttViewStyles.Resources;
                break;
            case ScheduleViewerSettings.ganttViewStyles.Jobs:
                GanttViewStyle = GanttViewStyles.Jobs;
                break;
            case ScheduleViewerSettings.ganttViewStyles.ResourcesAndJobsHorizontal:
                GanttViewStyle = GanttViewStyles.ResourcesAndJobsHorizontal;
                break;
            case ScheduleViewerSettings.ganttViewStyles.None:
                GanttViewStyle = GanttViewStyles.None;
                break;
            case ScheduleViewerSettings.ganttViewStyles.ActivityBoard:
                GanttViewStyle = GanttViewStyles.ActivityBoard;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        switch (a_settings.HighlightLinkType)
        {
            case ScheduleViewerSettings.HighlightLinkTypes.Job:
                HighlightLinkType = HighlightLinkTypes.Job;
                break;
            case ScheduleViewerSettings.HighlightLinkTypes.ManufacturingOrder:
                HighlightLinkType = HighlightLinkTypes.ManufacturingOrder;
                break;
            case ScheduleViewerSettings.HighlightLinkTypes.Operation:
                HighlightLinkType = HighlightLinkTypes.Operation;
                break;
            case ScheduleViewerSettings.HighlightLinkTypes.Activity:
                HighlightLinkType = HighlightLinkTypes.Activity;
                break;
            case ScheduleViewerSettings.HighlightLinkTypes.Inventory:
                HighlightLinkType = HighlightLinkTypes.Inventory;
                break;
            case ScheduleViewerSettings.HighlightLinkTypes.AllRelations:
                HighlightLinkType = HighlightLinkTypes.AllRelations;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        switch (a_settings.DefaultZoomLevel)
        {
            case ScheduleViewerSettings.zoomLevels.Shift:
                DefaultZoomLevel = zoomLevels.Shift;
                break;
            case ScheduleViewerSettings.zoomLevels.Day:
                DefaultZoomLevel = zoomLevels.Day;
                break;
            case ScheduleViewerSettings.zoomLevels.Week:
                DefaultZoomLevel = zoomLevels.Week;
                break;
            case ScheduleViewerSettings.zoomLevels.TwoWeeks:
                DefaultZoomLevel = zoomLevels.TwoWeeks;
                break;
            case ScheduleViewerSettings.zoomLevels.Month:
                DefaultZoomLevel = zoomLevels.Month;
                break;
            case ScheduleViewerSettings.zoomLevels.Year:
                DefaultZoomLevel = zoomLevels.Year;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        JobWatches = a_settings.JobWatches;
        PlantsTabTextOrientation = a_settings.PlantsTabTextOrientation;
        PlantsTabLayout = a_settings.PlantsTabLayout;
        PlantsTabOrientation = a_settings.PlantsTabOrientation;
        CurrentPlantInGanttId = a_settings.CurrentPlantInGanttId;
        DeptsTabTextOrientation = a_settings.DeptsTabTextOrientation;
        DeptsTabLayout = a_settings.DeptsTabLayout;
        DeptsTabOrientation = a_settings.DeptsTabOrientation;
        ShowLinks = a_settings.ShowJobLinks;
    }

    #region Enums
    public enum GanttViewStyles
    {
        Resources,
        Jobs,
        ResourcesAndJobsHorizontal,
        ActivityBoard,
        None
    }

    public enum zoomLevels
    {
        Shift = 0,
        Day,
        Week,
        TwoWeeks,
        Month,
        Year
    }

    public enum HighlightLinkTypes
    {
        Job,
        ManufacturingOrder,
        Operation,
        Activity,
        Inventory,
        AllRelations
    }
    #endregion

    #region Property Accessors
    #region Bools
    private BoolVector32 m_bools;
    private const int c_tipsVisibleIdx = 0;
    private const int c_hintsVisibleIdx = 1;
    private const int c_showHighlightingIdx = 2;
    private const int c_autoHighlightIdx = 3;
    private const int c_resizeRowsToFit = 4;
    private const int c_sortDescending = 5;
    private const int c_tooltipPinned = 6;
    private const int c_showLinks = 7;
    private const int c_fisheyeEnabled = 8;
    private const int c_hintRangesVisibleIdx = 9;
    
    public bool TipsVisible
    {
        get => m_bools[c_tipsVisibleIdx];
        set => m_bools[c_tipsVisibleIdx] = value;
    }

    public bool HintsVisible
    {
        get => m_bools[c_hintsVisibleIdx];
        set => m_bools[c_hintsVisibleIdx] = value;
    }

    public bool HintsRangeVisible
    {
        get => m_bools[c_hintRangesVisibleIdx];
        set => m_bools[c_hintRangesVisibleIdx] = value;
    }

    public bool ShowHighlighting
    {
        get => m_bools[c_showHighlightingIdx];
        set => m_bools[c_showHighlightingIdx] = value;
    }

    public bool AutoHighlight
    {
        get => m_bools[c_autoHighlightIdx];
        set => m_bools[c_autoHighlightIdx] = value;
    }

    public bool ResizeRowsToFit
    {
        get => m_bools[c_resizeRowsToFit];
        set => m_bools[c_resizeRowsToFit] = value;
    }

    public bool SortDescending
    {
        get => m_bools[c_sortDescending];
        set => m_bools[c_sortDescending] = value;
    }

    public bool TooltipPinned
    {
        get => m_bools[c_tooltipPinned];
        set => m_bools[c_tooltipPinned] = value;
    }

    public bool ShowLinks
    {
        get => m_bools[c_showLinks];
        set => m_bools[c_showLinks] = value;
    }

    public bool FisheyeEnabled
    {
        get => m_bools[c_fisheyeEnabled];
        set => m_bools[c_fisheyeEnabled] = value;
    }
    #endregion

    private double m_fixedRowStdHeight = 6.6; //minimum

    public double FixedRowStdHeight
    {
        get => m_fixedRowStdHeight;
        set => m_fixedRowStdHeight = value;
    }

    private int m_sortColumn = -1;

    public int SortColumn
    {
        get => m_sortColumn;
        set => m_sortColumn = value;
    }

    private int m_gridWidth = 250;

    public int GridWidth
    {
        get => m_gridWidth;
        set => m_gridWidth = value;
    }

    private List<int> m_gridColumnWidthList = new ();

    public List<int> GridColumnWidthList
    {
        get => m_gridColumnWidthList;
        set => m_gridColumnWidthList = value;
    }

    private TimeSpan m_calendarGanttSpan = TimeSpan.FromDays(7);

    public TimeSpan CalendarGanttSpan
    {
        get => m_calendarGanttSpan;
        set => m_calendarGanttSpan = value;
    }

    private int m_tooltipLocationX;
    private int m_tooltipLocationY;

    public Point TooltipLocation
    {
        get => new (m_tooltipLocationX, m_tooltipLocationY);
        set
        {
            m_tooltipLocationX = value.X;
            m_tooltipLocationY = value.Y;
        }
    }

    private GanttViewStyles m_ganttViewStyle;

    public GanttViewStyles GanttViewStyle
    {
        get => m_ganttViewStyle;
        set => m_ganttViewStyle = value;
    }

    private HighlightLinkTypes m_highlightLinkType;

    public HighlightLinkTypes HighlightLinkType
    {
        get => m_highlightLinkType;
        set => m_highlightLinkType = value;
    }

    private zoomLevels m_defaultZoomLevel = zoomLevels.Week;

    /// <summary>
    /// Set the value zoom level shown when the zoom button is pressed.
    /// </summary>
    public zoomLevels DefaultZoomLevel
    {
        get => m_defaultZoomLevel;
        set => m_defaultZoomLevel = value;
    }

    private BaseIdList m_jobWatches = new ();

    /// <summary>
    /// The list of Jobs last showing in the user's Job Watch Gantt.
    /// </summary>
    public BaseIdList JobWatches
    {
        get => m_jobWatches;
        set => m_jobWatches = value;
    }

    private int m_plantsTabTextOrientation;

    public int PlantsTabTextOrientation
    {
        get => m_plantsTabTextOrientation;
        set => m_plantsTabTextOrientation = value;
    }

    private int m_plantsTabLayout;

    public int PlantsTabLayout
    {
        get => m_plantsTabLayout;
        set => m_plantsTabLayout = value;
    }

    private int m_plantsTabOrientation = 5;

    public int PlantsTabOrientation
    {
        get => m_plantsTabOrientation;
        set => m_plantsTabOrientation = value;
    }

    private long m_currentPlantInGanttId;

    public long CurrentPlantInGanttId
    {
        get => m_currentPlantInGanttId;
        set => m_currentPlantInGanttId = value;
    }

    private string m_activePlantId;

    public string ActivePlantId
    {
        get => m_activePlantId;
        set => m_activePlantId = value;
    }

    private int m_deptsTabTextOrientation;

    public int DeptsTabTextOrientation
    {
        get => m_deptsTabTextOrientation;
        set => m_deptsTabTextOrientation = value;
    }

    private int m_deptsTabLayout;

    public int DeptsTabLayout
    {
        get => m_deptsTabLayout;
        set => m_deptsTabLayout = value;
    }

    private int m_deptsTabOrientation;

    public int DeptsTabOrientation
    {
        get => m_deptsTabOrientation;
        set => m_deptsTabOrientation = value;
    }

    private string m_activeDepartmentId;

    public string ActiveDepartmentId
    {
        get => m_activeDepartmentId;
        set => m_activeDepartmentId = value;
    }
    #endregion

    public int UniqueId => 933;
    public string SettingKey => "ganttViewerSettings_GanttView";
    public string Description => "Settings related to visual properties of the schedule.";
    public string SettingsGroup => SettingGroupConstants.GanttSettingsGroup;
    public string SettingsGroupCategory => SettingGroupConstants.GanttViewSettings;
    public string SettingCaption => "Gantt view settings";

    object ICloneable.Clone()
    {
        return Clone();
    }

    public GanttVisualSettings Clone()
    {
        return (GanttVisualSettings)MemberwiseClone();
    }
}