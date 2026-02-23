using System.Drawing;

using PT.Scheduler;

namespace PT.SchedulerDefinitions;

/// <summary>
/// Stores the current display settings for a Schedule Viewer.
/// </summary>
public class ScheduleViewerSettings : IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 184;

    public ScheduleViewerSettings(IReader reader)
    {
        bool showCampaign = false; // removed this
        double capacityIntervalHeightPercent = 15; // changed the type to int.
        if (reader.VersionNumber >= 667)
        {
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            reader.Read(out resizeRowsToFit);
            reader.Read(out gridWidth);
            reader.Read(out calendarGanttSpan);
            reader.Read(out ganttFlipped);
            reader.Read(out plantsTabLayout);
            reader.Read(out plantsTabOrientation);
            reader.Read(out plantsTabTextOrientation);
            reader.Read(out deptsTabLayout);
            reader.Read(out deptsTabOrientation);
            reader.Read(out deptsTabTextOrientation);
            reader.Read(out currentPlantInGanttId);
            reader.Read(out fixedRowStdHeight);
            reader.Read(out m_sortColumn);
            reader.Read(out m_sortDescending);

            int val;
            reader.Read(out val);
            defaultZoomLevel = (zoomLevels)val;

            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            reader.Read(out val);
            highlightLinkType = (HighlightLinkTypes)val;
            reader.Read(out m_capacityIntervalHeightPercent);

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
            bools = new BoolVector32(reader);

            jobWatches = new BaseIdList(reader);
            _jobWatchOptions = new JobWatchGattOptions(reader);

            m_activitySchedulingGridOptions = new ActivitySchedulingGridOptions(reader);

            //Save GanttGrid Settings
            int columns;
            reader.Read(out columns);
            for (int i = 0; i < columns; i++)
            {
                int columnWidth;
                reader.Read(out columnWidth);
                m_gridColumnWidthList.Add(columnWidth);
            }

            reader.Read(out m_tooltipLocationX);
            reader.Read(out m_tooltipLocationY);

            reader.Read(out m_activePlantId);
            reader.Read(out m_activeDepartmentId);
            reader.Read(out m_campaignHeightPercent);
        }

        #region Version 664
        else if (reader.VersionNumber >= 664)
        {
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            reader.Read(out resizeRowsToFit);
            reader.Read(out gridWidth);
            reader.Read(out calendarGanttSpan);
            reader.Read(out ganttFlipped);
            reader.Read(out showCampaign);
            reader.Read(out plantsTabLayout);
            reader.Read(out plantsTabOrientation);
            reader.Read(out plantsTabTextOrientation);
            reader.Read(out deptsTabLayout);
            reader.Read(out deptsTabOrientation);
            reader.Read(out deptsTabTextOrientation);
            reader.Read(out currentPlantInGanttId);
            reader.Read(out fixedRowStdHeight);
            reader.Read(out m_sortColumn);
            reader.Read(out m_sortDescending);

            int val;
            reader.Read(out val);
            defaultZoomLevel = (zoomLevels)val;

            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            reader.Read(out val);
            highlightLinkType = (HighlightLinkTypes)val;
            reader.Read(out capacityIntervalHeightPercent);

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
            bools = new BoolVector32(reader);

            jobWatches = new BaseIdList(reader);
            _jobWatchOptions = new JobWatchGattOptions(reader);

            m_activitySchedulingGridOptions = new ActivitySchedulingGridOptions(reader);

            //Save GanttGrid Settings
            int columns;
            reader.Read(out columns);
            for (int i = 0; i < columns; i++)
            {
                int columnWidth;
                reader.Read(out columnWidth);
                m_gridColumnWidthList.Add(columnWidth);
            }

            reader.Read(out m_tooltipLocationX);
            reader.Read(out m_tooltipLocationY);

            reader.Read(out m_activePlantId);
            reader.Read(out m_activeDepartmentId);
        }
        #endregion 664

        #region Version 608
        else if (reader.VersionNumber >= 608)
        {
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            reader.Read(out resizeRowsToFit);
            reader.Read(out gridWidth);
            reader.Read(out calendarGanttSpan);
            reader.Read(out ganttFlipped);
            reader.Read(out showCampaign);
            reader.Read(out plantsTabLayout);
            reader.Read(out plantsTabOrientation);
            reader.Read(out plantsTabTextOrientation);
            reader.Read(out deptsTabLayout);
            reader.Read(out deptsTabOrientation);
            reader.Read(out deptsTabTextOrientation);
            reader.Read(out currentPlantInGanttId);
            reader.Read(out fixedRowStdHeight);
            reader.Read(out m_sortColumn);
            reader.Read(out m_sortDescending);

            int val;
            reader.Read(out val);
            defaultZoomLevel = (zoomLevels)val;

            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            reader.Read(out val);
            highlightLinkType = (HighlightLinkTypes)val;
            reader.Read(out capacityIntervalHeightPercent);

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
            bools = new BoolVector32(reader);

            jobWatches = new BaseIdList(reader);
            _jobWatchOptions = new JobWatchGattOptions(reader);

            m_activitySchedulingGridOptions = new ActivitySchedulingGridOptions(reader);

            //Save GanttGrid Settings
            int columns;
            reader.Read(out columns);
            for (int i = 0; i < columns; i++)
            {
                int columnWidth;
                reader.Read(out columnWidth);
                m_gridColumnWidthList.Add(columnWidth);
            }

            reader.Read(out m_tooltipLocationX);
            reader.Read(out m_tooltipLocationY);
        }
        #endregion 608

        #region Version 600
        else if (reader.VersionNumber >= 600)
        {
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            reader.Read(out resizeRowsToFit);
            reader.Read(out gridWidth);
            reader.Read(out calendarGanttSpan);
            reader.Read(out ganttFlipped);
            reader.Read(out showCampaign);
            reader.Read(out plantsTabLayout);
            reader.Read(out plantsTabOrientation);
            reader.Read(out plantsTabTextOrientation);
            reader.Read(out deptsTabLayout);
            reader.Read(out deptsTabOrientation);
            reader.Read(out deptsTabTextOrientation);
            reader.Read(out currentPlantInGanttId);
            reader.Read(out fixedRowStdHeight);
            reader.Read(out m_sortColumn);
            reader.Read(out m_sortDescending);

            int val;
            reader.Read(out val);
            defaultZoomLevel = (zoomLevels)val;

            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            reader.Read(out val);
            highlightLinkType = (HighlightLinkTypes)val;
            reader.Read(out capacityIntervalHeightPercent);

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
            bools = new BoolVector32(reader);

            jobWatches = new BaseIdList(reader);
            _jobWatchOptions = new JobWatchGattOptions(reader);

            new CapacityPlanOptions(reader);
            m_activitySchedulingGridOptions = new ActivitySchedulingGridOptions(reader);

            //Save GanttGrid Settings
            int columns;
            reader.Read(out columns);
            for (int i = 0; i < columns; i++)
            {
                int columnWidth;
                reader.Read(out columnWidth);
                m_gridColumnWidthList.Add(columnWidth);
            }

            reader.Read(out m_tooltipLocationX);
            reader.Read(out m_tooltipLocationY);
        }
        #endregion

        #region Version 520
        else if (reader.VersionNumber >= 520)
        {
            //This is the same reader as 500. Fixed an incompatibility in serialization with the release branch
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            reader.Read(out resizeRowsToFit);
            reader.Read(out gridWidth);
            reader.Read(out calendarGanttSpan);
            reader.Read(out ganttFlipped);
            reader.Read(out showCampaign);
            reader.Read(out plantsTabLayout);
            reader.Read(out plantsTabOrientation);
            reader.Read(out plantsTabTextOrientation);
            reader.Read(out deptsTabLayout);
            reader.Read(out deptsTabOrientation);
            reader.Read(out deptsTabTextOrientation);
            reader.Read(out currentPlantInGanttId);
            reader.Read(out fixedRowStdHeight);
            reader.Read(out m_sortColumn);
            reader.Read(out m_sortDescending);

            int val;
            reader.Read(out val);
            defaultZoomLevel = (zoomLevels)val;

            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            reader.Read(out val);
            highlightLinkType = (HighlightLinkTypes)val;
            reader.Read(out capacityIntervalHeightPercent);

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
            bools = new BoolVector32(reader);

            jobWatches = new BaseIdList(reader);
            _jobWatchOptions = new JobWatchGattOptions(reader);

            new CapacityPlanOptions(reader);
            m_activitySchedulingGridOptions = new ActivitySchedulingGridOptions(reader);

            string unused;
            reader.Read(out unused);
        }
        #endregion

        #region Version 519
        else if (reader.VersionNumber >= 519)
        {
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            reader.Read(out resizeRowsToFit);
            reader.Read(out gridWidth);
            reader.Read(out calendarGanttSpan);
            reader.Read(out ganttFlipped);
            reader.Read(out showCampaign);
            reader.Read(out plantsTabLayout);
            reader.Read(out plantsTabOrientation);
            reader.Read(out plantsTabTextOrientation);
            reader.Read(out deptsTabLayout);
            reader.Read(out deptsTabOrientation);
            reader.Read(out deptsTabTextOrientation);
            reader.Read(out currentPlantInGanttId);
            reader.Read(out fixedRowStdHeight);
            reader.Read(out m_sortColumn);
            reader.Read(out m_sortDescending);

            int val;
            reader.Read(out val);
            defaultZoomLevel = (zoomLevels)val;

            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            reader.Read(out val);
            highlightLinkType = (HighlightLinkTypes)val;
            reader.Read(out capacityIntervalHeightPercent);

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
            bools = new BoolVector32(reader);

            jobWatches = new BaseIdList(reader);
            _jobWatchOptions = new JobWatchGattOptions(reader);

            new CapacityPlanOptions(reader);
            m_activitySchedulingGridOptions = new ActivitySchedulingGridOptions(reader);

            //Save GanttGrid Settings
            int columns;
            reader.Read(out columns);
            for (int i = 0; i < columns; i++)
            {
                int columnWidth;
                reader.Read(out columnWidth);
                m_gridColumnWidthList.Add(columnWidth);
            }

            reader.Read(out m_tooltipLocationX);
            reader.Read(out m_tooltipLocationY);
        }
        #endregion

        #region Version 518
        else if (reader.VersionNumber >= 518)
        {
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            reader.Read(out resizeRowsToFit);
            reader.Read(out gridWidth);
            reader.Read(out calendarGanttSpan);
            reader.Read(out ganttFlipped);
            reader.Read(out showCampaign);
            reader.Read(out plantsTabLayout);
            reader.Read(out plantsTabOrientation);
            reader.Read(out plantsTabTextOrientation);
            reader.Read(out deptsTabLayout);
            reader.Read(out deptsTabOrientation);
            reader.Read(out deptsTabTextOrientation);
            reader.Read(out currentPlantInGanttId);
            reader.Read(out fixedRowStdHeight);
            reader.Read(out m_sortColumn);
            reader.Read(out m_sortDescending);

            int val;
            reader.Read(out val);
            defaultZoomLevel = (zoomLevels)val;

            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            reader.Read(out val);
            highlightLinkType = (HighlightLinkTypes)val;
            reader.Read(out capacityIntervalHeightPercent);

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
            bools = new BoolVector32(reader);

            jobWatches = new BaseIdList(reader);
            _jobWatchOptions = new JobWatchGattOptions(reader);

            new CapacityPlanOptions(reader);
            m_activitySchedulingGridOptions = new ActivitySchedulingGridOptions(reader);

            //Save GanttGrid Settings
            int columns;
            reader.Read(out columns);
            for (int i = 0; i < columns; i++)
            {
                int columnWidth;
                reader.Read(out columnWidth);
                m_gridColumnWidthList.Add(columnWidth);
            }
        }
        #endregion

        #region Version 500
        else if (reader.VersionNumber >= 500)
        {
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            reader.Read(out resizeRowsToFit);
            reader.Read(out gridWidth);
            reader.Read(out calendarGanttSpan);
            reader.Read(out ganttFlipped);
            reader.Read(out showCampaign);
            reader.Read(out plantsTabLayout);
            reader.Read(out plantsTabOrientation);
            reader.Read(out plantsTabTextOrientation);
            reader.Read(out deptsTabLayout);
            reader.Read(out deptsTabOrientation);
            reader.Read(out deptsTabTextOrientation);
            reader.Read(out currentPlantInGanttId);
            reader.Read(out fixedRowStdHeight);
            reader.Read(out m_sortColumn);
            reader.Read(out m_sortDescending);

            int val;
            reader.Read(out val);
            defaultZoomLevel = (zoomLevels)val;

            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            reader.Read(out val);
            highlightLinkType = (HighlightLinkTypes)val;
            reader.Read(out capacityIntervalHeightPercent);

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
            bools = new BoolVector32(reader);

            jobWatches = new BaseIdList(reader);
            _jobWatchOptions = new JobWatchGattOptions(reader);

            new CapacityPlanOptions(reader);
            m_activitySchedulingGridOptions = new ActivitySchedulingGridOptions(reader);

            string unused;
            reader.Read(out unused);
        }
        #endregion

        #region Version 489
        else if (reader.VersionNumber >= 489)
        {
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            reader.Read(out resizeRowsToFit);
            reader.Read(out gridWidth);
            reader.Read(out calendarGanttSpan);
            reader.Read(out ganttFlipped);
            reader.Read(out showCampaign);
            reader.Read(out plantsTabLayout);
            reader.Read(out plantsTabOrientation);
            reader.Read(out plantsTabTextOrientation);
            reader.Read(out deptsTabLayout);
            reader.Read(out deptsTabOrientation);
            reader.Read(out deptsTabTextOrientation);
            reader.Read(out currentPlantInGanttId);
            reader.Read(out fixedRowStdHeight);
            reader.Read(out m_sortColumn);
            reader.Read(out m_sortDescending);

            int val;
            reader.Read(out val);
            defaultZoomLevel = (zoomLevels)val;

            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            reader.Read(out val);
            highlightLinkType = (HighlightLinkTypes)val;
            reader.Read(out capacityIntervalHeightPercent);

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
            bools = new BoolVector32(reader);

            jobWatches = new BaseIdList(reader);
            _jobWatchOptions = new JobWatchGattOptions(reader);

            new CapacityPlanOptions(reader);
            m_activitySchedulingGridOptions = new ActivitySchedulingGridOptions(reader);
        }
        #endregion

        #region Version 430
        else if (reader.VersionNumber >= 430)
        {
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            reader.Read(out resizeRowsToFit);
            reader.Read(out gridWidth);
            reader.Read(out calendarGanttSpan);
            reader.Read(out ganttFlipped);
            reader.Read(out showCampaign);
            reader.Read(out plantsTabLayout);
            reader.Read(out plantsTabOrientation);
            reader.Read(out plantsTabTextOrientation);
            reader.Read(out deptsTabLayout);
            reader.Read(out deptsTabOrientation);
            reader.Read(out deptsTabTextOrientation);
            reader.Read(out currentPlantInGanttId);
            reader.Read(out fixedRowStdHeight);
            reader.Read(out m_sortColumn);
            reader.Read(out m_sortDescending);

            int val;
            reader.Read(out val);
            defaultZoomLevel = (zoomLevels)val;

            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            reader.Read(out val);
            highlightLinkType = (HighlightLinkTypes)val;
            reader.Read(out capacityIntervalHeightPercent);

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
            bools = new BoolVector32(reader);

            jobWatches = new BaseIdList(reader);
            _jobWatchOptions = new JobWatchGattOptions(reader);

            new CapacityPlanOptions(reader);
        }
        #endregion

        #region Version 369
        else if (reader.VersionNumber >= 369)
        {
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            reader.Read(out resizeRowsToFit);
            reader.Read(out gridWidth);
            reader.Read(out calendarGanttSpan);
            reader.Read(out ganttFlipped);
            reader.Read(out showCampaign);
            reader.Read(out plantsTabLayout);
            reader.Read(out plantsTabOrientation);
            reader.Read(out plantsTabTextOrientation);
            reader.Read(out deptsTabLayout);
            reader.Read(out deptsTabOrientation);
            reader.Read(out deptsTabTextOrientation);
            reader.Read(out currentPlantInGanttId);
            reader.Read(out fixedRowStdHeight);
            reader.Read(out m_sortColumn);
            reader.Read(out m_sortDescending);

            int val;
            reader.Read(out val);
            defaultZoomLevel = (zoomLevels)val;

            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            reader.Read(out val);
            highlightLinkType = (HighlightLinkTypes)val;
            reader.Read(out capacityIntervalHeightPercent);

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
            bools = new BoolVector32(reader);

            jobWatches = new BaseIdList(reader);
            _jobWatchOptions = new JobWatchGattOptions(reader);
        }
        #endregion

        #region Version 285
        else if (reader.VersionNumber >= 285)
        {
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            reader.Read(out resizeRowsToFit);
            reader.Read(out gridWidth);
            reader.Read(out calendarGanttSpan);
            reader.Read(out ganttFlipped);
            reader.Read(out showCampaign);
            reader.Read(out plantsTabLayout);
            reader.Read(out plantsTabOrientation);
            reader.Read(out plantsTabTextOrientation);
            reader.Read(out deptsTabLayout);
            reader.Read(out deptsTabOrientation);
            reader.Read(out deptsTabTextOrientation);
            reader.Read(out currentPlantInGanttId);
            reader.Read(out fixedRowStdHeight);

            int val;
            reader.Read(out val);
            defaultZoomLevel = (zoomLevels)val;

            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            reader.Read(out val);
            highlightLinkType = (HighlightLinkTypes)val;
            reader.Read(out capacityIntervalHeightPercent);

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
            bools = new BoolVector32(reader);

            jobWatches = new BaseIdList(reader);
            _jobWatchOptions = new JobWatchGattOptions(reader);
        }
        #endregion

        #region Version 221
        else if (reader.VersionNumber >= 221)
        {
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            reader.Read(out resizeRowsToFit);
            reader.Read(out gridWidth);
            reader.Read(out calendarGanttSpan);
            reader.Read(out ganttFlipped);
            reader.Read(out showCampaign);
            reader.Read(out plantsTabLayout);
            reader.Read(out plantsTabOrientation);
            reader.Read(out plantsTabTextOrientation);
            reader.Read(out deptsTabLayout);
            reader.Read(out deptsTabOrientation);
            reader.Read(out deptsTabTextOrientation);
            reader.Read(out currentPlantInGanttId);
            reader.Read(out fixedRowStdHeight);

            int val;
            reader.Read(out val);
            defaultZoomLevel = (zoomLevels)val;

            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            reader.Read(out val);
            highlightLinkType = (HighlightLinkTypes)val;
            reader.Read(out capacityIntervalHeightPercent);

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
            bools = new BoolVector32(reader);

            jobWatches = new BaseIdList(reader);
        }
        #endregion

        #region Version 214
        else if (reader.VersionNumber >= 214)
        {
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            reader.Read(out resizeRowsToFit);
            reader.Read(out gridWidth);
            reader.Read(out calendarGanttSpan);
            reader.Read(out ganttFlipped);
            reader.Read(out showCampaign);
            reader.Read(out plantsTabLayout);
            reader.Read(out plantsTabOrientation);
            reader.Read(out plantsTabTextOrientation);
            reader.Read(out deptsTabLayout);
            reader.Read(out deptsTabOrientation);
            reader.Read(out deptsTabTextOrientation);
            reader.Read(out currentPlantInGanttId);
            reader.Read(out fixedRowStdHeight);

            int val;
            reader.Read(out val);
            defaultZoomLevel = (zoomLevels)val;

            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            reader.Read(out val);
            highlightLinkType = (HighlightLinkTypes)val;
            reader.Read(out capacityIntervalHeightPercent);

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
            bools = new BoolVector32(reader);
        }
        #endregion

        #region Version 207
        else if (reader.VersionNumber >= 207)
        {
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            reader.Read(out resizeRowsToFit);
            reader.Read(out gridWidth);
            reader.Read(out calendarGanttSpan);
            reader.Read(out ganttFlipped);
            reader.Read(out showCampaign);
            reader.Read(out plantsTabLayout);
            reader.Read(out plantsTabOrientation);
            reader.Read(out plantsTabTextOrientation);
            reader.Read(out deptsTabLayout);
            reader.Read(out deptsTabOrientation);
            reader.Read(out deptsTabTextOrientation);
            reader.Read(out currentPlantInGanttId);
            reader.Read(out fixedRowStdHeight);

            int val;
            reader.Read(out val);
            defaultZoomLevel = (zoomLevels)val;

            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            reader.Read(out val);
            highlightLinkType = (HighlightLinkTypes)val;
            reader.Read(out capacityIntervalHeightPercent);

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
        }
        #endregion

        #region Version 190
        else if (reader.VersionNumber >= 190)
        {
            int deprecatedFixedRowHeight;
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            reader.Read(out resizeRowsToFit);
            reader.Read(out deprecatedFixedRowHeight);
            reader.Read(out gridWidth);
            reader.Read(out calendarGanttSpan);
            reader.Read(out ganttFlipped);
            reader.Read(out showCampaign);
            reader.Read(out plantsTabLayout);
            reader.Read(out plantsTabOrientation);
            reader.Read(out plantsTabTextOrientation);
            reader.Read(out deptsTabLayout);
            reader.Read(out deptsTabOrientation);
            reader.Read(out deptsTabTextOrientation);
            reader.Read(out currentPlantInGanttId);

            int val;
            reader.Read(out val);
            defaultZoomLevel = (zoomLevels)val;

            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            reader.Read(out val);
            highlightLinkType = (HighlightLinkTypes)val;
            reader.Read(out capacityIntervalHeightPercent);

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
        }
        #endregion

        #region version 177
        else if (reader.VersionNumber >= 177)
        {
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            reader.Read(out resizeRowsToFit);
            int deprecatedFixedRowHeight;
            reader.Read(out deprecatedFixedRowHeight);
            reader.Read(out gridWidth);
            reader.Read(out calendarGanttSpan);
            reader.Read(out ganttFlipped);
            reader.Read(out showCampaign);
            reader.Read(out plantsTabLayout);
            reader.Read(out plantsTabOrientation);
            reader.Read(out plantsTabTextOrientation);
            reader.Read(out deptsTabLayout);
            reader.Read(out deptsTabOrientation);
            reader.Read(out deptsTabTextOrientation);

            int val;
            reader.Read(out val);
            defaultZoomLevel = (zoomLevels)val;

            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            reader.Read(out val);
            highlightLinkType = (HighlightLinkTypes)val;
            reader.Read(out capacityIntervalHeightPercent);

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
        }
        #endregion

        #region Version 167
        else if (reader.VersionNumber >= 167)
        {
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            reader.Read(out resizeRowsToFit);
            int deprecatedFixedRowHeight;
            reader.Read(out deprecatedFixedRowHeight);
            reader.Read(out gridWidth);
            reader.Read(out calendarGanttSpan);
            reader.Read(out ganttFlipped);
            reader.Read(out showCampaign);

            int val;
            reader.Read(out val);
            defaultZoomLevel = (zoomLevels)val;

            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            reader.Read(out val);
            highlightLinkType = (HighlightLinkTypes)val;
            reader.Read(out capacityIntervalHeightPercent);

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
        }
        #endregion

        #region Version 155
        else if (reader.VersionNumber >= 155)
        {
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            reader.Read(out resizeRowsToFit);
            int deprecatedFixedRowHeight;
            reader.Read(out deprecatedFixedRowHeight);
            reader.Read(out gridWidth);
            reader.Read(out calendarGanttSpan);
            reader.Read(out ganttFlipped);

            int val;
            reader.Read(out val);
            defaultZoomLevel = (zoomLevels)val;

            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            reader.Read(out val);
            highlightLinkType = (HighlightLinkTypes)val;
            reader.Read(out capacityIntervalHeightPercent);

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
        }
        #endregion

        #region Version 9
        else if (reader.VersionNumber >= 9)
        {
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            int val;
            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            reader.Read(out val);
            highlightLinkType = (HighlightLinkTypes)val;
            reader.Read(out capacityIntervalHeightPercent);

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
            PaneInfoList deprecatedPaneInfos = new (reader);
        }
        #endregion

        #region Version 7
        else if (reader.VersionNumber >= 7)
        {
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            int val;
            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            reader.Read(out val);
            highlightLinkType = (HighlightLinkTypes)val;

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
            PaneInfoList deprecatedPaneInfos = new (reader);
        }
        #endregion

        #region Versions 1
        else if (reader.VersionNumber >= 1)
        {
            reader.Read(out tipsVisible);
            reader.Read(out hintsVisible);
            reader.Read(out syncGantts);
            reader.Read(out showHighlighting);
            reader.Read(out autoHighlight);
            reader.Read(out showJobLinks);
            reader.Read(out expediteMove);
            reader.Read(out exactMove);
            int val;
            reader.Read(out val);
            GanttViewStyle = (ganttViewStyles)val;
            highlightLinkType = HighlightLinkTypes.Job; //new in version 7

            labelSettings = new LabelSettings(reader);
            simplifySettings = new SimplifyGanttSettings(reader);
            PaneInfoList deprecatedPaneInfos = new (reader);
        }
        #endregion

        if (reader.VersionNumber < 667)
        {
            m_capacityIntervalHeightPercent = (int)capacityIntervalHeightPercent;
            if (showCampaign) // upgrade ShowCampaign
            {
                m_campaignHeightPercent = 20;
            }

            showHighlighting = false;
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(tipsVisible);
        writer.Write(hintsVisible);
        writer.Write(syncGantts);
        writer.Write(showHighlighting);
        writer.Write(autoHighlight);
        writer.Write(showJobLinks);
        writer.Write(expediteMove);
        writer.Write(exactMove);
        writer.Write(resizeRowsToFit);
        writer.Write(gridWidth);
        writer.Write(calendarGanttSpan);
        writer.Write(ganttFlipped);
        writer.Write(plantsTabLayout);
        writer.Write(plantsTabOrientation);
        writer.Write(plantsTabTextOrientation);
        writer.Write(deptsTabLayout);
        writer.Write(deptsTabOrientation);
        writer.Write(deptsTabTextOrientation);
        writer.Write(currentPlantInGanttId);
        writer.Write(fixedRowStdHeight);
        writer.Write(m_sortColumn);
        writer.Write(m_sortDescending);

        writer.Write((int)defaultZoomLevel);
        writer.Write((int)GanttViewStyle);
        writer.Write((int)highlightLinkType);
        writer.Write(m_capacityIntervalHeightPercent);

        labelSettings.Serialize(writer);
        simplifySettings.Serialize(writer);
        bools.Serialize(writer);

        jobWatches.Serialize(writer);
        _jobWatchOptions.Serialize(writer);

        m_activitySchedulingGridOptions.Serialize(writer);
        //m_desyncGanttSettings.Serialize(writer);

        //Write Gantt grid settings
        writer.Write(m_gridColumnWidthList.Count);
        foreach (int width in m_gridColumnWidthList)
        {
            writer.Write(width);
        }

        //Write tooltip location
        writer.Write(m_tooltipLocationX);
        writer.Write(m_tooltipLocationY);
        writer.Write(m_activePlantId);
        writer.Write(m_activeDepartmentId);
        writer.Write(m_campaignHeightPercent);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public ScheduleViewerSettings()
    {

    }

    #region Bools
    private BoolVector32 bools;
    //private const int lockMoveIdx = 0; //Moved to ActivityMoveSettings
    //private const int anchorMoveIdx = 1; //Moved to ActivityMoveSettings
    //private const int c_UNSUSED = 2;
    //private const int c_inventoryPlanIncludeForecastInNetInv = 3; //Moved to Inventory plan settings
    private const int c_tooltipPinnedIdx = 4;
    #endregion Bools

    private LabelSettings labelSettings = new ();
    
    private SimplifyGanttSettings simplifySettings = new ();

    public SimplifyGanttSettings SimplifySettings
    {
        get => simplifySettings;
        set => simplifySettings = value;
    }

    private bool tipsVisible = true;

    /// <summary>
    /// Whether the gantt block tips are visible.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public bool TipsVisible
    {
        get => tipsVisible;
        set => tipsVisible = value;
    }

    private bool hintsVisible = true;

    /// <summary>
    /// Whether the scheduling hints are visible.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public bool HintsVisible
    {
        get => hintsVisible;
        set => hintsVisible = value;
    }

    private bool syncGantts = true;

    /// <summary>
    /// Whether Gantts should be synchronized with each other.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public bool SyncGantts
    {
        get => syncGantts;
        set => syncGantts = value;
    }

    private bool showHighlighting;

    /// <summary>
    /// Whether to show the highlights of any Jobs that are currently set to Highlight.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public bool ShowHighlighting
    {
        get => showHighlighting;
        set => showHighlighting = value;
    }

    private bool autoHighlight;

    /// <summary>
    /// Whether to automatically highlight a Job when it's clicked.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public bool AutoHighlight
    {
        get => autoHighlight;
        set => autoHighlight = value;
    }

    private bool showJobLinks;

    /// <summary>
    /// Whether to show Job links when they're clicked.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public bool ShowJobLinks
    {
        get => showJobLinks;
        set => showJobLinks = value;
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

    private HighlightLinkTypes highlightLinkType = HighlightLinkTypes.Job;

    public HighlightLinkTypes HighlightLinkType
    {
        get => highlightLinkType;
        set => highlightLinkType = value;
    }

    //TODO: Remove these from serialization on next update
    #region Move
    private bool expediteMove;
    private bool exactMove;
    private ActivitySchedulingGridOptions m_activitySchedulingGridOptions = new ();
    #endregion Move

    private bool resizeRowsToFit = true;

    public bool ResizeRowsToFit
    {
        get => resizeRowsToFit;
        set => resizeRowsToFit = value;
    }

    private double fixedRowStdHeight = 2.0; //min std height

    /// <summary>
    /// Standard Gantt rows height.  Multiplied by the Resource's GanttRowHeightScale to calculated Row Height.  Only used if ResizeRowsToFit is false.
    /// </summary>
    public double FixedRowStandardHeight
    {
        get => fixedRowStdHeight;
        set => fixedRowStdHeight = value;
    }

    private int m_sortColumn = -1;

    /// <summary>
    /// index of the column used to sort resources.
    /// </summary>
    public int SortColumn
    {
        get => m_sortColumn;
        set => m_sortColumn = value;
    }

    private bool m_sortDescending;

    /// <summary>
    /// whether last sort was descending or ascending
    /// </summary>
    public bool SortDescending
    {
        get => m_sortDescending;
        set => m_sortDescending = value;
    }

    private int gridWidth = 50;

    public int GridWidth
    {
        get => gridWidth;
        set => gridWidth = value;
    }

    private List<int> m_gridColumnWidthList = new ();

    public List<int> GridColumnWidthList
    {
        get => m_gridColumnWidthList;
        set => m_gridColumnWidthList = value;
    }

    private TimeSpan calendarGanttSpan = TimeSpan.FromDays(7);

    public TimeSpan CalendarGanttSpan
    {
        get => calendarGanttSpan;
        set => calendarGanttSpan = value;
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

    private zoomLevels defaultZoomLevel = zoomLevels.Week;

    /// <summary>
    /// Set the value zoom level shown when the zoom button is pressed.
    /// </summary>
    public zoomLevels DefaultZoomLevel
    {
        get => defaultZoomLevel;
        set => defaultZoomLevel = value;
    }

    private bool ganttFlipped;

    public bool GanttFlipped
    {
        get => ganttFlipped;
        set => ganttFlipped = value;
    }

    public bool TooltipPinned
    {
        get => bools[c_tooltipPinnedIdx];
        set => bools[c_tooltipPinnedIdx] = value;
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

    private int m_capacityIntervalHeightPercent = 15;

    /// <summary>
    /// The amount to inset Gantt Blocks to display Gantt Capacity Intervals.  A value of 0 hides the GanttCapacity Intervals.  1 maximizes their height.
    /// </summary>
    public int CapacityIntervalHeightPercent
    {
        get => m_capacityIntervalHeightPercent;
        set => m_capacityIntervalHeightPercent = value;
    }

    private int m_campaignHeightPercent;

    /// <summary>
    /// The percent of total resource height to use for displaying Campaigns. A value of 0 hides Campaigns.
    /// </summary>
    public int CampaignHeightPercent
    {
        get => m_campaignHeightPercent;
        set => m_campaignHeightPercent = value;
    }

    public enum ganttViewStyles
    {
        Resources,
        Jobs,
        ResourcesAndJobsHorizontal,
        None,
        ActivityBoard
    }

    private ganttViewStyles ganttViewStyle;

    public ganttViewStyles GanttViewStyle
    {
        get => ganttViewStyle;
        set => ganttViewStyle = value;
    }

    private int plantsTabTextOrientation;

    /// <summary>
    /// Corresponds to Infragistics Tab enum.
    /// </summary>
    public int PlantsTabTextOrientation
    {
        get => plantsTabTextOrientation;
        set => plantsTabTextOrientation = value;
    }

    private int plantsTabLayout;

    /// <summary>
    /// Corresponds to Infragistics Tab enum.
    /// </summary>
    public int PlantsTabLayout
    {
        get => plantsTabLayout;
        set => plantsTabLayout = value;
    }

    private int plantsTabOrientation = 5; //Left Top

    /// <summary>
    /// Corresponds to Infragistics Tab enum.
    /// </summary>
    public int PlantsTabOrientation
    {
        get => plantsTabOrientation;
        set => plantsTabOrientation = value;
    }

    private long currentPlantInGanttId;

    /// <summary>
    /// The Id of the Plant the user was last showing in the Plants Gantt.
    /// </summary>
    public long CurrentPlantInGanttId
    {
        get => currentPlantInGanttId;
        set => currentPlantInGanttId = value;
    }

    private int deptsTabTextOrientation;

    /// <summary>
    /// Corresponds to Infragistics Tab enum.
    /// </summary>
    public int DeptsTabTextOrientation
    {
        get => deptsTabTextOrientation;
        set => deptsTabTextOrientation = value;
    }

    private int deptsTabLayout;

    /// <summary>
    /// Corresponds to Infragistics Tab enum.
    /// </summary>
    public int DeptsTabLayout
    {
        get => deptsTabLayout;
        set => deptsTabLayout = value;
    }

    private int deptsTabOrientation;

    /// <summary>
    /// Corresponds to Infragistics Tab enum.
    /// </summary>
    public int DeptsTabOrientation
    {
        get => deptsTabOrientation;
        set => deptsTabOrientation = value;
    }

    private JobWatchGattOptions _jobWatchOptions = new ();

    public JobWatchGattOptions JobWatchOptions
    {
        get => _jobWatchOptions;
        set => _jobWatchOptions = value;
    }

    private BaseIdList jobWatches = new ();

    /// <summary>
    /// The list of Jobs last showing in the user's Job Watch Gantt.
    /// </summary>
    public BaseIdList JobWatches
    {
        get => jobWatches;
        set => jobWatches = value;
    }

    private string m_activePlantId;
    private string m_activeDepartmentId;

    public string ActivePlantId
    {
        get => m_activePlantId;
        set => m_activePlantId = value;
    }

    public string ActiveDepartmentId
    {
        get => m_activeDepartmentId;
        set => m_activeDepartmentId = value;
    }

    #region DEPRECATED
    public class PaneInfo : IPTSerializable
    {
        public const int UNIQUE_ID = 185;

        #region IPTSerializable Members
        public PaneInfo(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                reader.Read(out paneName);
                reader.Read(out currentGanttViewCollectionName);

                ganttViewCollectionInfos = new GanttViewCollectionInfoList(reader);
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(paneName);
            writer.Write(currentGanttViewCollectionName);

            ganttViewCollectionInfos.Serialize(writer);
        }

        public virtual int UniqueId => UNIQUE_ID;
        #endregion

        public PaneInfo(string paneName)
        {
            this.paneName = paneName;
        }

        private readonly string paneName;

        public string PaneName => paneName;

        private string currentGanttViewCollectionName;

        public string CurrentGanttViewCollectionName
        {
            get => currentGanttViewCollectionName;
            set => currentGanttViewCollectionName = value;
        }

        private readonly GanttViewCollectionInfoList ganttViewCollectionInfos = new ();

        public GanttViewCollectionInfoList GanttViewCollectionInfos => ganttViewCollectionInfos;
    }

    /// <summary>
    /// Stores a list of Pane Info objects that pertain to the panes of a GanttView.
    /// </summary>
    public class PaneInfoList : IPTSerializable
    {
        public const int UNIQUE_ID = 186;

        #region IPTSerializable Members
        public PaneInfoList(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                int count;
                reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    PaneInfo p = new (reader);
                    Add(p);
                }
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(Count);
            for (int i = 0; i < Count; i++)
            {
                this[i].Serialize(writer);
            }
        }

        public virtual int UniqueId => UNIQUE_ID;
        #endregion

        public PaneInfoList() { }

        private readonly SortedList<string, PaneInfo> panes = new ();

        public PaneInfo Add(PaneInfo paneInfo)
        {
            panes.Add(paneInfo.PaneName, paneInfo);
            return paneInfo;
        }

        public int Count => panes.Count;

        public void Clear()
        {
            panes.Clear();
        }

        public PaneInfo this[string a_name] => panes[a_name];

        public bool ContainsKey(string name)
        {
            return panes.ContainsKey(name);
        }

        public PaneInfo this[int a_index] => panes.Values[a_index];
    }

    /// <summary>
    /// Stores the settings of a single GanttViewCollection.
    /// </summary>
    public class GanttViewCollectionInfo : IPTSerializable
    {
        public const int UNIQUE_ID = 187;

        #region IPTSerializable Members
        public GanttViewCollectionInfo(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                reader.Read(out ganttViewCollectionName);
                reader.Read(out currentGanttViewName);

                ganttViewSettingsList = new GanttViewSettingsList(reader);
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(ganttViewCollectionName);
            writer.Write(currentGanttViewName);

            ganttViewSettingsList.Serialize(writer);
        }

        public virtual int UniqueId => UNIQUE_ID;
        #endregion

        public GanttViewCollectionInfo(string ganttViewCollectionName)
        {
            this.ganttViewCollectionName = ganttViewCollectionName;
        }

        private string ganttViewCollectionName;

        public string GanttViewCollectionName
        {
            get => ganttViewCollectionName;
            set => ganttViewCollectionName = value;
        }

        private string currentGanttViewName;

        /// <summary>
        /// The name of the GanttView that is currently displayed in the collection.
        /// </summary>
        public string CurrentGanttViewName
        {
            get => currentGanttViewName;
            set => currentGanttViewName = value;
        }

        private GanttViewSettingsList ganttViewSettingsList = new ();

        public GanttViewSettingsList GanttViewSettingsList
        {
            get => ganttViewSettingsList;
            set => ganttViewSettingsList = value;
        }
    }

    /// <summary>
    /// Stores a list of GanttViewCollectionInfo objects for a Pane.
    /// </summary>
    public class GanttViewCollectionInfoList : IPTSerializable
    {
        public const int UNIQUE_ID = 188;

        #region IPTSerializable Members
        public GanttViewCollectionInfoList(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                int count;
                reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    GanttViewCollectionInfo gvc = new (reader);
                    Add(gvc);
                }
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(Count);
            for (int i = 0; i < Count; i++)
            {
                this[i].Serialize(writer);
            }
        }

        public virtual int UniqueId => UNIQUE_ID;
        #endregion

        public GanttViewCollectionInfoList() { }

        private readonly SortedList<string, GanttViewCollectionInfo> m_ganttViews = new ();

        public GanttViewCollectionInfo Add(GanttViewCollectionInfo ganttViewCollectionInfo)
        {
            m_ganttViews.Add(ganttViewCollectionInfo.GanttViewCollectionName, ganttViewCollectionInfo);
            return ganttViewCollectionInfo;
        }

        public int Count => m_ganttViews.Count;

        public void Clear()
        {
            m_ganttViews.Clear();
        }

        public GanttViewCollectionInfo this[string ganttViewName] => m_ganttViews[ganttViewName];

        public bool ContainsKey(string name)
        {
            return m_ganttViews.ContainsKey(name);
        }

        public GanttViewCollectionInfo this[int index] => m_ganttViews.Values[index];
    }

    /// <summary>
    /// Stores the settings of a Gantt View (which contains 3 gantts for Fisheye).
    /// </summary>
    public class GanttViewSettings : IPTSerializable
    {
        public const int UNIQUE_ID = 189;

        #region IPTSerializable Members
        public GanttViewSettings(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                reader.Read(out ganttViewName);

                layout = new GanttViewLayout(reader);
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(ganttViewName);

            layout.Serialize(writer);
        }

        public virtual int UniqueId => UNIQUE_ID;
        #endregion

        public GanttViewSettings(string ganttViewName)
        {
            this.ganttViewName = ganttViewName;
        }

        private string ganttViewName;

        public string GanttViewName
        {
            get => ganttViewName;
            set => ganttViewName = value;
        }

        private GanttViewLayout layout;

        /// <summary>
        /// Stores the zoom, start, stop, tree width, etc of a gantt.
        /// </summary>
        public GanttViewLayout Layout
        {
            get => layout;
            set => layout = value;
        }
    }

    /// <summary>
    /// Stores a list of GanttViewSettings objects for a GanttViewCollection.
    /// </summary>
    public class GanttViewSettingsList : IPTSerializable
    {
        public const int UNIQUE_ID = 190;

        #region IPTSerializable Members
        public GanttViewSettingsList(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                int count;
                reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    GanttViewSettings gvs = new (reader);
                    Add(gvs);
                }
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(Count);
            for (int i = 0; i < Count; i++)
            {
                this[i].Serialize(writer);
            }
        }

        public virtual int UniqueId => UNIQUE_ID;
        #endregion

        public GanttViewSettingsList() { }

        private readonly SortedList<string, GanttViewSettings> m_ganttViews = new ();

        public GanttViewSettings Add(GanttViewSettings ganttViewSettings)
        {
            m_ganttViews.Add(ganttViewSettings.GanttViewName, ganttViewSettings);
            return ganttViewSettings;
        }

        public int Count => m_ganttViews.Count;

        public void Clear()
        {
            m_ganttViews.Clear();
        }

        public GanttViewSettings this[string ganttViewName] => m_ganttViews[ganttViewName];

        public bool ContainsKey(string name)
        {
            return m_ganttViews.ContainsKey(name);
        }

        public GanttViewSettings this[int index] => m_ganttViews.Values[index];
    }
    #endregion DEPRECATED

    public class JobWatchGattOptions : IPTSerializable
    {
        public const int UNIQUE_ID = 662;

        #region IPTSerializable Members
        public JobWatchGattOptions(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                bools = new BoolVector32(reader);
                int val;
                reader.Read(out val);
                _jobsToShow = (jobsToShow)val;
            }
        }

        public void Serialize(IWriter writer)
        {
            bools.Serialize(writer);
            writer.Write((int)_jobsToShow);
        }

        public virtual int UniqueId => UNIQUE_ID;
        #endregion

        public JobWatchGattOptions()
        {
            //defaults
            AutoZoom = true;
            AutoScroll = true;
            ShowOtherEligibleResources = true;
            JobsToShow = jobsToShow.RelatedJobs;
        }

        private BoolVector32 bools;
        private const int autoScrollToJobStartIdx = 0;
        private const int autoZoomIdx = 1;
        private const int showOtherEligibleResourcesIdx = 2;

        public enum jobsToShow { WatchedJobOnly, RelatedJobs, AllJobs }

        private jobsToShow _jobsToShow = jobsToShow.RelatedJobs;

        /// <summary>
        /// Which jobs to show in the Job Watch.
        /// </summary>
        public jobsToShow JobsToShow
        {
            get => _jobsToShow;
            set => _jobsToShow = value;
        }

        /// <summary>
        /// Whether to zoom to show the Jobs' ranges.
        /// </summary>
        public bool AutoZoom
        {
            get => bools[autoZoomIdx];
            set => bools[autoZoomIdx] = value;
        }

        /// <summary>
        /// Whether to scroll to the start of the Job.
        /// </summary>
        public bool AutoScroll
        {
            get => bools[autoScrollToJobStartIdx];
            set => bools[autoScrollToJobStartIdx] = value;
        }

        /// <summary>
        /// Whether to show Resources that are eligible even if not currently in use by the Jobs.
        /// </summary>
        public bool ShowOtherEligibleResources
        {
            get => bools[showOtherEligibleResourcesIdx];
            set => bools[showOtherEligibleResourcesIdx] = value;
        }
    }

    /// <summary>
    /// Specifies the User-specific settings related to block Labels.
    /// </summary>
    [Serializable]
    public class LabelSettings : IPTSerializable, ICloneable
    {
        public const int UNIQUE_ID = 191;

        public LabelSettings()
        {
            ItemTypeFilterList = new List<ItemDefs.itemTypes>();
        }

        #region IPTSerializable Members
        public LabelSettings(IReader reader)
        {
            ItemTypeFilterList = new List<ItemDefs.itemTypes>();

            if (reader.VersionNumber >= 701)
            {
                reader.Read(out tooltipScript);
                reader.Read(out blockScript);
                reader.Read(out onTimeSegmentScript);
                reader.Read(out attributeSegmentScript);
                reader.Read(out processSetupSegmentScript);
                reader.Read(out processRunSegmentScript);
                reader.Read(out processPostSegmentScript);
                reader.Read(out processStorageSegmentScript);
                reader.Read(out processStoragePostSegmentScript);
                reader.Read(out statusSegmentScript);
                reader.Read(out m_materialStatusSegmentScript);
                reader.Read(out progressSegmentScript);
                reader.Read(out prioritySegmentScript);
                reader.Read(out commitmentSegmentScript);
                reader.Read(out showBlockLabelAlways);
                reader.Read(out showTimingSegment);
                reader.Read(out showAttributesSegment);
                reader.Read(out showJobColorAttribute);
                reader.Read(out showProductColorAttribute);
                reader.Read(out showSetupColorAttribute);
                reader.Read(out showCustomAttributes);
                reader.Read(out showBlockFlags);
                reader.Read(out showActivityFlags);
                reader.Read(out showOperationFlags);
                reader.Read(out showManufacturingOrderFlags);
                reader.Read(out showJobFlags);
                reader.Read(out showMaterialFlags);
                reader.Read(out showStandardFlags);
                reader.Read(out showCustomFlags);

                reader.Read(out showProcessSegment);
                reader.Read(out showStatusSegment);
                reader.Read(out m_showMaterialStatusSegment);
                reader.Read(out showProgressSegment);
                reader.Read(out showPrioritySegment);
                reader.Read(out showCommitmentSegment);
                reader.Read(out useSetupCodeColors);
                reader.Read(out useFastDraw);
                reader.Read(out noClipping);
                reader.Read(out wrapText);
                reader.Read(out useSolidColors);
                int val;
                reader.Read(out val);
                textDirection = (textDirections)val;
                reader.Read(out m_padding);
                //Font
                string fontFamily;
                reader.Read(out fontFamily);
                bool bold;
                reader.Read(out bold);
                bool italic;
                reader.Read(out italic);
                bool strikeout;
                reader.Read(out strikeout);
                bool underline;
                reader.Read(out underline);
                FontStyle style = FontStyle.Regular;
                if (bold)
                {
                    style = style | FontStyle.Bold;
                }

                if (italic)
                {
                    style = style | FontStyle.Italic;
                }

                if (strikeout)
                {
                    style = style | FontStyle.Strikeout;
                }

                if (underline)
                {
                    style = style | FontStyle.Underline;
                }

                float size;
                reader.Read(out size);
                labelFont = new Font(fontFamily, size, style);

                reader.Read(out highlightUnreviewedJobs);
                reader.Read(out actualStatusScript);
                reader.Read(out actualPerformanceScript);
                reader.Read(out showBufferSegment);
                reader.Read(out showCurrentDrumPenetration);
                reader.Read(out bufferScript);

                int count;
                reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    reader.Read(out val);
                    ItemTypeFilterList.Add((ItemDefs.itemTypes)val);
                }
            }

            #region 645
            else if (reader.VersionNumber >= 645)
            {
                reader.Read(out tooltipScript);
                reader.Read(out blockScript);
                reader.Read(out onTimeSegmentScript);
                reader.Read(out attributeSegmentScript);
                reader.Read(out processSetupSegmentScript);
                reader.Read(out processRunSegmentScript);
                reader.Read(out processPostSegmentScript);
                reader.Read(out processStorageSegmentScript);
                reader.Read(out processStoragePostSegmentScript);
                reader.Read(out statusSegmentScript);
                reader.Read(out progressSegmentScript);
                reader.Read(out prioritySegmentScript);
                reader.Read(out commitmentSegmentScript);
                reader.Read(out showBlockLabelAlways);
                reader.Read(out showTimingSegment);
                reader.Read(out showAttributesSegment);
                reader.Read(out showJobColorAttribute);
                reader.Read(out showProductColorAttribute);
                reader.Read(out showSetupColorAttribute);
                reader.Read(out showCustomAttributes);
                reader.Read(out showBlockFlags);
                reader.Read(out showActivityFlags);
                reader.Read(out showOperationFlags);
                reader.Read(out showManufacturingOrderFlags);
                reader.Read(out showJobFlags);
                reader.Read(out showMaterialFlags);
                reader.Read(out showStandardFlags);
                reader.Read(out showCustomFlags);

                reader.Read(out showProcessSegment);
                reader.Read(out showStatusSegment);
                reader.Read(out showProgressSegment);
                reader.Read(out showPrioritySegment);
                reader.Read(out showCommitmentSegment);
                reader.Read(out useSetupCodeColors);
                reader.Read(out useFastDraw);
                reader.Read(out noClipping);
                reader.Read(out wrapText);
                reader.Read(out useSolidColors);
                int val;
                reader.Read(out val);
                textDirection = (textDirections)val;
                reader.Read(out m_padding);
                //Font
                string fontFamily;
                reader.Read(out fontFamily);
                bool bold;
                reader.Read(out bold);
                bool italic;
                reader.Read(out italic);
                bool strikeout;
                reader.Read(out strikeout);
                bool underline;
                reader.Read(out underline);
                FontStyle style = FontStyle.Regular;
                if (bold)
                {
                    style = style | FontStyle.Bold;
                }

                if (italic)
                {
                    style = style | FontStyle.Italic;
                }

                if (strikeout)
                {
                    style = style | FontStyle.Strikeout;
                }

                if (underline)
                {
                    style = style | FontStyle.Underline;
                }

                float size;
                reader.Read(out size);
                labelFont = new Font(fontFamily, size, style);

                reader.Read(out highlightUnreviewedJobs);
                reader.Read(out actualStatusScript);
                reader.Read(out actualPerformanceScript);
                reader.Read(out showBufferSegment);
                reader.Read(out showCurrentDrumPenetration);
                reader.Read(out bufferScript);

                int count;
                reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    reader.Read(out val);
                    ItemTypeFilterList.Add((ItemDefs.itemTypes)val);
                }
            }
            #endregion 645

            #region 643
            else if (reader.VersionNumber >= 643)
            {
                reader.Read(out tooltipScript);
                reader.Read(out blockScript);
                reader.Read(out onTimeSegmentScript);
                reader.Read(out attributeSegmentScript);
                reader.Read(out processSetupSegmentScript);
                reader.Read(out processRunSegmentScript);
                reader.Read(out processPostSegmentScript);
                reader.Read(out processStorageSegmentScript);
                reader.Read(out processStoragePostSegmentScript);
                reader.Read(out statusSegmentScript);
                reader.Read(out progressSegmentScript);
                reader.Read(out prioritySegmentScript);
                reader.Read(out commitmentSegmentScript);
                reader.Read(out showBlockLabelAlways);
                reader.Read(out showTimingSegment);
                reader.Read(out showAttributesSegment);
                reader.Read(out showJobColorAttribute);
                reader.Read(out showProductColorAttribute);
                reader.Read(out showSetupColorAttribute);
                reader.Read(out showCustomAttributes);
                reader.Read(out showBlockFlags);
                reader.Read(out showActivityFlags);
                reader.Read(out showOperationFlags);
                reader.Read(out showManufacturingOrderFlags);
                reader.Read(out showJobFlags);
                reader.Read(out showMaterialFlags);
                reader.Read(out showStandardFlags);
                reader.Read(out showCustomFlags);

                reader.Read(out showProcessSegment);
                reader.Read(out showStatusSegment);
                reader.Read(out showProgressSegment);
                reader.Read(out showPrioritySegment);
                reader.Read(out showCommitmentSegment);
                reader.Read(out useSetupCodeColors);
                reader.Read(out useFastDraw);
                reader.Read(out noClipping);
                reader.Read(out wrapText);
                reader.Read(out useSolidColors);
                int val;
                reader.Read(out val);
                textDirection = (textDirections)val;

                //Font
                string fontFamily;
                reader.Read(out fontFamily);
                bool bold;
                reader.Read(out bold);
                bool italic;
                reader.Read(out italic);
                bool strikeout;
                reader.Read(out strikeout);
                bool underline;
                reader.Read(out underline);
                FontStyle style = FontStyle.Regular;
                if (bold)
                {
                    style = style | FontStyle.Bold;
                }

                if (italic)
                {
                    style = style | FontStyle.Italic;
                }

                if (strikeout)
                {
                    style = style | FontStyle.Strikeout;
                }

                if (underline)
                {
                    style = style | FontStyle.Underline;
                }

                float size;
                reader.Read(out size);
                labelFont = new Font(fontFamily, size, style);

                reader.Read(out highlightUnreviewedJobs);
                reader.Read(out actualStatusScript);
                reader.Read(out actualPerformanceScript);
                reader.Read(out showBufferSegment);
                reader.Read(out showCurrentDrumPenetration);
                reader.Read(out bufferScript);

                int count;
                reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    reader.Read(out val);
                    ItemTypeFilterList.Add((ItemDefs.itemTypes)val);
                }
            }
            #endregion 643

            else
            {
                //Initialize the list to contain all of the item types
                foreach (ItemDefs.itemTypes type in Enum.GetValues(typeof(ItemDefs.itemTypes)))
                {
                    ItemTypeFilterList.Add(type);
                }

                #region 631
                if (reader.VersionNumber >= 631)
                {
                    reader.Read(out tooltipScript);
                    reader.Read(out blockScript);
                    reader.Read(out onTimeSegmentScript);
                    reader.Read(out attributeSegmentScript);
                    reader.Read(out processSetupSegmentScript);
                    reader.Read(out processRunSegmentScript);
                    reader.Read(out processPostSegmentScript);
                    reader.Read(out processStorageSegmentScript);
                    reader.Read(out processStoragePostSegmentScript);
                    reader.Read(out statusSegmentScript);
                    reader.Read(out progressSegmentScript);
                    reader.Read(out prioritySegmentScript);
                    reader.Read(out commitmentSegmentScript);
                    reader.Read(out showBlockLabelAlways);
                    reader.Read(out showTimingSegment);
                    reader.Read(out showAttributesSegment);
                    reader.Read(out showJobColorAttribute);
                    reader.Read(out showProductColorAttribute);
                    reader.Read(out showSetupColorAttribute);
                    reader.Read(out showCustomAttributes);
                    reader.Read(out showBlockFlags);
                    reader.Read(out showActivityFlags);
                    reader.Read(out showOperationFlags);
                    reader.Read(out showManufacturingOrderFlags);
                    reader.Read(out showJobFlags);
                    reader.Read(out showMaterialFlags);
                    reader.Read(out showStandardFlags);
                    reader.Read(out showCustomFlags);

                    reader.Read(out showProcessSegment);
                    reader.Read(out showStatusSegment);
                    reader.Read(out showProgressSegment);
                    reader.Read(out showPrioritySegment);
                    reader.Read(out showCommitmentSegment);
                    reader.Read(out useSetupCodeColors);
                    reader.Read(out useFastDraw);
                    reader.Read(out noClipping);
                    reader.Read(out wrapText);
                    reader.Read(out useSolidColors);
                    int val;
                    reader.Read(out val);
                    textDirection = (textDirections)val;

                    //Font
                    string fontFamily;
                    reader.Read(out fontFamily);
                    bool bold;
                    reader.Read(out bold);
                    bool italic;
                    reader.Read(out italic);
                    bool strikeout;
                    reader.Read(out strikeout);
                    bool underline;
                    reader.Read(out underline);
                    FontStyle style = FontStyle.Regular;
                    if (bold)
                    {
                        style = style | FontStyle.Bold;
                    }

                    if (italic)
                    {
                        style = style | FontStyle.Italic;
                    }

                    if (strikeout)
                    {
                        style = style | FontStyle.Strikeout;
                    }

                    if (underline)
                    {
                        style = style | FontStyle.Underline;
                    }

                    float size;
                    reader.Read(out size);
                    labelFont = new Font(fontFamily, size, style);

                    reader.Read(out highlightUnreviewedJobs);
                    reader.Read(out actualStatusScript);
                    reader.Read(out actualPerformanceScript);
                    reader.Read(out showBufferSegment);
                    reader.Read(out showCurrentDrumPenetration);
                    reader.Read(out bufferScript);
                }
                #endregion 631

                #region 403
                else if (reader.VersionNumber >= 403)
                {
                    reader.Read(out tooltipScript);
                    reader.Read(out blockScript);
                    reader.Read(out onTimeSegmentScript);
                    reader.Read(out attributeSegmentScript);
                    reader.Read(out processSetupSegmentScript);
                    reader.Read(out processRunSegmentScript);
                    reader.Read(out processPostSegmentScript);
                    reader.Read(out statusSegmentScript);
                    reader.Read(out progressSegmentScript);
                    reader.Read(out prioritySegmentScript);
                    reader.Read(out commitmentSegmentScript);
                    reader.Read(out showBlockLabelAlways);
                    reader.Read(out showTimingSegment);
                    reader.Read(out showAttributesSegment);
                    reader.Read(out showJobColorAttribute);
                    reader.Read(out showProductColorAttribute);
                    reader.Read(out showSetupColorAttribute);
                    reader.Read(out showCustomAttributes);
                    reader.Read(out showBlockFlags);
                    reader.Read(out showActivityFlags);
                    reader.Read(out showOperationFlags);
                    reader.Read(out showManufacturingOrderFlags);
                    reader.Read(out showJobFlags);
                    reader.Read(out showMaterialFlags);
                    reader.Read(out showStandardFlags);
                    reader.Read(out showCustomFlags);

                    reader.Read(out showProcessSegment);
                    reader.Read(out showStatusSegment);
                    reader.Read(out showProgressSegment);
                    reader.Read(out showPrioritySegment);
                    reader.Read(out showCommitmentSegment);
                    reader.Read(out useSetupCodeColors);
                    reader.Read(out useFastDraw);
                    reader.Read(out noClipping);
                    reader.Read(out wrapText);
                    reader.Read(out useSolidColors);
                    int val;
                    reader.Read(out val);
                    textDirection = (textDirections)val;

                    //Font
                    string fontFamily;
                    reader.Read(out fontFamily);
                    bool bold;
                    reader.Read(out bold);
                    bool italic;
                    reader.Read(out italic);
                    bool strikeout;
                    reader.Read(out strikeout);
                    bool underline;
                    reader.Read(out underline);
                    FontStyle style = FontStyle.Regular;
                    if (bold)
                    {
                        style = style | FontStyle.Bold;
                    }

                    if (italic)
                    {
                        style = style | FontStyle.Italic;
                    }

                    if (strikeout)
                    {
                        style = style | FontStyle.Strikeout;
                    }

                    if (underline)
                    {
                        style = style | FontStyle.Underline;
                    }

                    float size;
                    reader.Read(out size);
                    labelFont = new Font(fontFamily, size, style);

                    reader.Read(out highlightUnreviewedJobs);
                    reader.Read(out actualStatusScript);
                    reader.Read(out actualPerformanceScript);
                    reader.Read(out showBufferSegment);
                    reader.Read(out showCurrentDrumPenetration);
                    reader.Read(out bufferScript);
                }
                #endregion 403

                #region Version 346
                else if (reader.VersionNumber >= 346)
                {
                    reader.Read(out tooltipScript);
                    reader.Read(out blockScript);
                    reader.Read(out onTimeSegmentScript);
                    reader.Read(out attributeSegmentScript);
                    reader.Read(out processSetupSegmentScript);
                    reader.Read(out processRunSegmentScript);
                    reader.Read(out processPostSegmentScript);
                    reader.Read(out statusSegmentScript);
                    reader.Read(out progressSegmentScript);
                    reader.Read(out prioritySegmentScript);
                    reader.Read(out commitmentSegmentScript);
                    reader.Read(out showBlockLabelAlways);
                    reader.Read(out showTimingSegment);
                    reader.Read(out showAttributesSegment);
                    reader.Read(out showJobColorAttribute);
                    reader.Read(out showProductColorAttribute);
                    reader.Read(out showSetupColorAttribute);
                    reader.Read(out showCustomAttributes);
                    reader.Read(out showBlockFlags);
                    reader.Read(out showActivityFlags);
                    reader.Read(out showOperationFlags);
                    reader.Read(out showManufacturingOrderFlags);
                    reader.Read(out showJobFlags);
                    reader.Read(out showMaterialFlags);
                    reader.Read(out showStandardFlags);
                    reader.Read(out showCustomFlags);

                    reader.Read(out showProcessSegment);
                    reader.Read(out showStatusSegment);
                    reader.Read(out showProgressSegment);
                    reader.Read(out showPrioritySegment);
                    reader.Read(out showCommitmentSegment);
                    reader.Read(out useSetupCodeColors);
                    reader.Read(out useFastDraw);
                    reader.Read(out noClipping);
                    reader.Read(out wrapText);
                    reader.Read(out useSolidColors);
                    int val;
                    reader.Read(out val);
                    textDirection = (textDirections)val;

                    //Font
                    string fontFamily;
                    reader.Read(out fontFamily);
                    bool bold;
                    reader.Read(out bold);
                    bool italic;
                    reader.Read(out italic);
                    bool strikeout;
                    reader.Read(out strikeout);
                    bool underline;
                    reader.Read(out underline);
                    FontStyle style = FontStyle.Regular;
                    if (bold)
                    {
                        style = style | FontStyle.Bold;
                    }

                    if (italic)
                    {
                        style = style | FontStyle.Italic;
                    }

                    if (strikeout)
                    {
                        style = style | FontStyle.Strikeout;
                    }

                    if (underline)
                    {
                        style = style | FontStyle.Underline;
                    }

                    float size;
                    reader.Read(out size);
                    labelFont = new Font(fontFamily, size, style);

                    reader.Read(out highlightUnreviewedJobs);
                    reader.Read(out actualStatusScript);
                    reader.Read(out actualPerformanceScript);
                }
                #endregion

                #region 266
                else if (reader.VersionNumber >= 266)
                {
                    reader.Read(out tooltipScript);
                    reader.Read(out blockScript);
                    reader.Read(out onTimeSegmentScript);
                    reader.Read(out attributeSegmentScript);
                    reader.Read(out processSetupSegmentScript);
                    reader.Read(out processRunSegmentScript);
                    reader.Read(out processPostSegmentScript);
                    reader.Read(out statusSegmentScript);
                    reader.Read(out progressSegmentScript);
                    reader.Read(out prioritySegmentScript);
                    reader.Read(out commitmentSegmentScript);
                    reader.Read(out showBlockLabelAlways);
                    reader.Read(out showTimingSegment);
                    reader.Read(out showAttributesSegment);
                    reader.Read(out showJobColorAttribute);
                    reader.Read(out showProductColorAttribute);
                    reader.Read(out showSetupColorAttribute);
                    reader.Read(out showCustomAttributes);
                    reader.Read(out showBlockFlags);
                    reader.Read(out showActivityFlags);
                    reader.Read(out showOperationFlags);
                    reader.Read(out showManufacturingOrderFlags);
                    reader.Read(out showJobFlags);
                    reader.Read(out showMaterialFlags);
                    reader.Read(out showStandardFlags);
                    reader.Read(out showCustomFlags);

                    reader.Read(out showProcessSegment);
                    reader.Read(out showStatusSegment);
                    reader.Read(out showProgressSegment);
                    reader.Read(out showPrioritySegment);
                    reader.Read(out showCommitmentSegment);
                    reader.Read(out useSetupCodeColors);
                    reader.Read(out useFastDraw);
                    reader.Read(out noClipping);
                    reader.Read(out wrapText);
                    reader.Read(out useSolidColors);
                    int val;
                    reader.Read(out val);
                    textDirection = (textDirections)val;

                    //Font
                    string fontFamily;
                    reader.Read(out fontFamily);
                    bool bold;
                    reader.Read(out bold);
                    bool italic;
                    reader.Read(out italic);
                    bool strikeout;
                    reader.Read(out strikeout);
                    bool underline;
                    reader.Read(out underline);
                    FontStyle style = FontStyle.Regular;
                    if (bold)
                    {
                        style = style | FontStyle.Bold;
                    }

                    if (italic)
                    {
                        style = style | FontStyle.Italic;
                    }

                    if (strikeout)
                    {
                        style = style | FontStyle.Strikeout;
                    }

                    if (underline)
                    {
                        style = style | FontStyle.Underline;
                    }

                    float size;
                    reader.Read(out size);
                    labelFont = new Font(fontFamily, size, style);

                    reader.Read(out highlightUnreviewedJobs);
                }
                #endregion

                #region Version 153
                else if (reader.VersionNumber >= 153)
                {
                    reader.Read(out tooltipScript);
                    reader.Read(out blockScript);
                    reader.Read(out onTimeSegmentScript);
                    reader.Read(out attributeSegmentScript);
                    reader.Read(out processSetupSegmentScript);
                    reader.Read(out processRunSegmentScript);
                    reader.Read(out processPostSegmentScript);
                    reader.Read(out statusSegmentScript);
                    reader.Read(out progressSegmentScript);
                    reader.Read(out prioritySegmentScript);
                    reader.Read(out commitmentSegmentScript);
                    reader.Read(out showBlockLabelAlways);
                    reader.Read(out showTimingSegment);
                    reader.Read(out showAttributesSegment);
                    reader.Read(out showJobColorAttribute);
                    reader.Read(out showProductColorAttribute);
                    reader.Read(out showSetupColorAttribute);
                    reader.Read(out showCustomAttributes);
                    reader.Read(out showBlockFlags);
                    reader.Read(out showActivityFlags);
                    reader.Read(out showOperationFlags);
                    reader.Read(out showManufacturingOrderFlags);
                    reader.Read(out showJobFlags);
                    reader.Read(out showMaterialFlags);
                    reader.Read(out showStandardFlags);
                    reader.Read(out showCustomFlags);

                    reader.Read(out showProcessSegment);
                    reader.Read(out showStatusSegment);
                    reader.Read(out showProgressSegment);
                    reader.Read(out showPrioritySegment);
                    reader.Read(out showCommitmentSegment);
                    reader.Read(out useSetupCodeColors);
                    reader.Read(out useFastDraw);
                    reader.Read(out noClipping);
                    reader.Read(out wrapText);
                    reader.Read(out useSolidColors);
                    int val;
                    reader.Read(out val);
                    textDirection = (textDirections)val;

                    //Font
                    string fontFamily;
                    reader.Read(out fontFamily);
                    bool bold;
                    reader.Read(out bold);
                    bool italic;
                    reader.Read(out italic);
                    bool strikeout;
                    reader.Read(out strikeout);
                    bool underline;
                    reader.Read(out underline);
                    FontStyle style = FontStyle.Regular;
                    if (bold)
                    {
                        style = style | FontStyle.Bold;
                    }

                    if (italic)
                    {
                        style = style | FontStyle.Italic;
                    }

                    if (strikeout)
                    {
                        style = style | FontStyle.Strikeout;
                    }

                    if (underline)
                    {
                        style = style | FontStyle.Underline;
                    }

                    float size;
                    reader.Read(out size);
                    labelFont = new Font(fontFamily, size, style);
                }
                #endregion

                #region Version 152
                else if (reader.VersionNumber >= 152)
                {
                    reader.Read(out tooltipScript);
                    reader.Read(out blockScript);
                    reader.Read(out onTimeSegmentScript);
                    reader.Read(out attributeSegmentScript);
                    reader.Read(out processSetupSegmentScript);
                    reader.Read(out processRunSegmentScript);
                    reader.Read(out processPostSegmentScript);
                    reader.Read(out statusSegmentScript);
                    reader.Read(out prioritySegmentScript);
                    reader.Read(out commitmentSegmentScript);
                    reader.Read(out showBlockLabelAlways);
                    reader.Read(out showTimingSegment);
                    reader.Read(out showAttributesSegment);
                    reader.Read(out showJobColorAttribute);
                    reader.Read(out showProductColorAttribute);
                    reader.Read(out showSetupColorAttribute);
                    reader.Read(out showCustomAttributes);
                    reader.Read(out showBlockFlags);
                    reader.Read(out showActivityFlags);
                    reader.Read(out showOperationFlags);
                    reader.Read(out showManufacturingOrderFlags);
                    reader.Read(out showJobFlags);
                    reader.Read(out showMaterialFlags);
                    reader.Read(out showStandardFlags);
                    reader.Read(out showCustomFlags);

                    reader.Read(out showProcessSegment);
                    reader.Read(out showStatusSegment);
                    reader.Read(out showPrioritySegment);
                    reader.Read(out showCommitmentSegment);
                    reader.Read(out useSetupCodeColors);
                    reader.Read(out useFastDraw);
                    reader.Read(out noClipping);
                    reader.Read(out wrapText);
                    reader.Read(out useSolidColors);
                    int val;
                    reader.Read(out val);
                    textDirection = (textDirections)val;

                    //Font
                    string fontFamily;
                    reader.Read(out fontFamily);
                    bool bold;
                    reader.Read(out bold);
                    bool italic;
                    reader.Read(out italic);
                    bool strikeout;
                    reader.Read(out strikeout);
                    bool underline;
                    reader.Read(out underline);
                    FontStyle style = FontStyle.Regular;
                    if (bold)
                    {
                        style = style | FontStyle.Bold;
                    }

                    if (italic)
                    {
                        style = style | FontStyle.Italic;
                    }

                    if (strikeout)
                    {
                        style = style | FontStyle.Strikeout;
                    }

                    if (underline)
                    {
                        style = style | FontStyle.Underline;
                    }

                    float size;
                    reader.Read(out size);
                    labelFont = new Font(fontFamily, size, style);
                }
                #endregion

                #region Version 132
                else if (reader.VersionNumber >= 132)
                {
                    reader.Read(out blockScript);
                    reader.Read(out onTimeSegmentScript);
                    reader.Read(out attributeSegmentScript);
                    reader.Read(out processSetupSegmentScript);
                    reader.Read(out processRunSegmentScript);
                    reader.Read(out processPostSegmentScript);
                    reader.Read(out statusSegmentScript);
                    reader.Read(out prioritySegmentScript);
                    reader.Read(out commitmentSegmentScript);
                    reader.Read(out showBlockLabelAlways);
                    reader.Read(out showTimingSegment);
                    reader.Read(out showAttributesSegment);
                    reader.Read(out showJobColorAttribute);
                    reader.Read(out showProductColorAttribute);
                    reader.Read(out showSetupColorAttribute);
                    reader.Read(out showCustomAttributes);
                    reader.Read(out showBlockFlags);
                    reader.Read(out showActivityFlags);
                    reader.Read(out showOperationFlags);
                    reader.Read(out showManufacturingOrderFlags);
                    reader.Read(out showJobFlags);
                    reader.Read(out showMaterialFlags);
                    reader.Read(out showStandardFlags);
                    reader.Read(out showCustomFlags);

                    reader.Read(out showProcessSegment);
                    reader.Read(out showStatusSegment);
                    reader.Read(out showPrioritySegment);
                    reader.Read(out showCommitmentSegment);
                    reader.Read(out useSetupCodeColors);
                    reader.Read(out useFastDraw);
                    reader.Read(out noClipping);
                    reader.Read(out wrapText);
                    reader.Read(out useSolidColors);
                    int val;
                    reader.Read(out val);
                    textDirection = (textDirections)val;

                    //Font
                    string fontFamily;
                    reader.Read(out fontFamily);
                    bool bold;
                    reader.Read(out bold);
                    bool italic;
                    reader.Read(out italic);
                    bool strikeout;
                    reader.Read(out strikeout);
                    bool underline;
                    reader.Read(out underline);
                    FontStyle style = FontStyle.Regular;
                    if (bold)
                    {
                        style = style | FontStyle.Bold;
                    }

                    if (italic)
                    {
                        style = style | FontStyle.Italic;
                    }

                    if (strikeout)
                    {
                        style = style | FontStyle.Strikeout;
                    }

                    if (underline)
                    {
                        style = style | FontStyle.Underline;
                    }

                    float size;
                    reader.Read(out size);
                    labelFont = new Font(fontFamily, size, style);
                }
                #endregion

                #region Version 128
                else if (reader.VersionNumber >= 128)
                {
                    reader.Read(out blockScript);
                    reader.Read(out onTimeSegmentScript);
                    reader.Read(out attributeSegmentScript);
                    reader.Read(out processSetupSegmentScript);
                    reader.Read(out processRunSegmentScript);
                    reader.Read(out processPostSegmentScript);
                    reader.Read(out statusSegmentScript);
                    reader.Read(out prioritySegmentScript);
                    reader.Read(out commitmentSegmentScript);
                    reader.Read(out showBlockLabelAlways);
                    reader.Read(out showTimingSegment);
                    reader.Read(out showAttributesSegment);
                    reader.Read(out showJobColorAttribute);
                    reader.Read(out showProductColorAttribute);
                    reader.Read(out showSetupColorAttribute);
                    reader.Read(out showCustomAttributes);

                    reader.Read(out showProcessSegment);
                    reader.Read(out showStatusSegment);
                    reader.Read(out showPrioritySegment);
                    reader.Read(out showCommitmentSegment);
                    reader.Read(out useSetupCodeColors);
                    reader.Read(out useFastDraw);
                    reader.Read(out noClipping);
                    reader.Read(out wrapText);
                    reader.Read(out useSolidColors);
                    int val;
                    reader.Read(out val);
                    textDirection = (textDirections)val;

                    //Font
                    string fontFamily;
                    reader.Read(out fontFamily);
                    bool bold;
                    reader.Read(out bold);
                    bool italic;
                    reader.Read(out italic);
                    bool strikeout;
                    reader.Read(out strikeout);
                    bool underline;
                    reader.Read(out underline);
                    FontStyle style = FontStyle.Regular;
                    if (bold)
                    {
                        style = style | FontStyle.Bold;
                    }

                    if (italic)
                    {
                        style = style | FontStyle.Italic;
                    }

                    if (strikeout)
                    {
                        style = style | FontStyle.Strikeout;
                    }

                    if (underline)
                    {
                        style = style | FontStyle.Underline;
                    }

                    float size;
                    reader.Read(out size);
                    labelFont = new Font(fontFamily, size, style);
                }
                #endregion

                #region Version 123
                else if (reader.VersionNumber >= 123)
                {
                    reader.Read(out blockScript);
                    reader.Read(out onTimeSegmentScript);
                    reader.Read(out attributeSegmentScript);
                    reader.Read(out processSetupSegmentScript);
                    reader.Read(out processRunSegmentScript);
                    reader.Read(out processPostSegmentScript);
                    reader.Read(out statusSegmentScript);
                    reader.Read(out prioritySegmentScript);
                    reader.Read(out commitmentSegmentScript);
                    reader.Read(out showBlockLabelAlways);
                    reader.Read(out showTimingSegment);
                    reader.Read(out showAttributesSegment);
                    reader.Read(out showJobColorAttribute);
                    reader.Read(out showProductColorAttribute);
                    reader.Read(out showSetupColorAttribute);
                    reader.Read(out showCustomAttributes);

                    reader.Read(out showProcessSegment);
                    reader.Read(out showStatusSegment);
                    reader.Read(out showPrioritySegment);
                    reader.Read(out showCommitmentSegment);
                    reader.Read(out useSetupCodeColors);
                    reader.Read(out useFastDraw);
                    reader.Read(out noClipping);
                    reader.Read(out wrapText);
                    int val;
                    reader.Read(out val);
                    textDirection = (textDirections)val;

                    //Font
                    string fontFamily;
                    reader.Read(out fontFamily);
                    bool bold;
                    reader.Read(out bold);
                    bool italic;
                    reader.Read(out italic);
                    bool strikeout;
                    reader.Read(out strikeout);
                    bool underline;
                    reader.Read(out underline);
                    FontStyle style = FontStyle.Regular;
                    if (bold)
                    {
                        style = style | FontStyle.Bold;
                    }

                    if (italic)
                    {
                        style = style | FontStyle.Italic;
                    }

                    if (strikeout)
                    {
                        style = style | FontStyle.Strikeout;
                    }

                    if (underline)
                    {
                        style = style | FontStyle.Underline;
                    }

                    float size;
                    reader.Read(out size);
                    labelFont = new Font(fontFamily, size, style);
                }
                #endregion

                #region Version 66
                else if (reader.VersionNumber >= 66)
                {
                    reader.Read(out blockScript);
                    reader.Read(out onTimeSegmentScript);
                    reader.Read(out attributeSegmentScript);
                    reader.Read(out processSetupSegmentScript);
                    reader.Read(out processRunSegmentScript);
                    reader.Read(out processPostSegmentScript);
                    reader.Read(out statusSegmentScript);
                    reader.Read(out prioritySegmentScript);
                    reader.Read(out commitmentSegmentScript);
                    reader.Read(out showBlockLabelAlways);
                    reader.Read(out showTimingSegment);
                    reader.Read(out showAttributesSegment);
                    reader.Read(out showJobColorAttribute);
                    reader.Read(out showProductColorAttribute);
                    reader.Read(out showSetupColorAttribute);

                    reader.Read(out showProcessSegment);
                    reader.Read(out showStatusSegment);
                    reader.Read(out showPrioritySegment);
                    reader.Read(out showCommitmentSegment);
                    reader.Read(out useSetupCodeColors);
                    reader.Read(out useFastDraw);
                    reader.Read(out noClipping);
                    reader.Read(out wrapText);
                    int val;
                    reader.Read(out val);
                    textDirection = (textDirections)val;

                    //Font
                    string fontFamily;
                    reader.Read(out fontFamily);
                    bool bold;
                    reader.Read(out bold);
                    bool italic;
                    reader.Read(out italic);
                    bool strikeout;
                    reader.Read(out strikeout);
                    bool underline;
                    reader.Read(out underline);
                    FontStyle style = FontStyle.Regular;
                    if (bold)
                    {
                        style = style | FontStyle.Bold;
                    }

                    if (italic)
                    {
                        style = style | FontStyle.Italic;
                    }

                    if (strikeout)
                    {
                        style = style | FontStyle.Strikeout;
                    }

                    if (underline)
                    {
                        style = style | FontStyle.Underline;
                    }

                    float size;
                    reader.Read(out size);
                    labelFont = new Font(fontFamily, size, style);
                }
                #endregion

                #region Version 1
                else if (reader.VersionNumber >= 1)
                {
                    reader.Read(out blockScript);
                    reader.Read(out onTimeSegmentScript);
                    reader.Read(out attributeSegmentScript);
                    reader.Read(out processSetupSegmentScript);
                    reader.Read(out processRunSegmentScript);
                    reader.Read(out processPostSegmentScript);
                    reader.Read(out statusSegmentScript);
                    reader.Read(out prioritySegmentScript);
                    reader.Read(out commitmentSegmentScript);
                    reader.Read(out showBlockLabelAlways);
                    reader.Read(out showTimingSegment);
                    reader.Read(out showAttributesSegment);
                    reader.Read(out showProcessSegment);
                    reader.Read(out showStatusSegment);
                    reader.Read(out showPrioritySegment);
                    reader.Read(out showCommitmentSegment);
                    reader.Read(out useSetupCodeColors);
                    reader.Read(out useFastDraw);
                    reader.Read(out noClipping);
                    reader.Read(out wrapText);
                    int val;
                    reader.Read(out val);
                    textDirection = (textDirections)val;

                    //Font
                    string fontFamily;
                    reader.Read(out fontFamily);
                    bool bold;
                    reader.Read(out bold);
                    bool italic;
                    reader.Read(out italic);
                    bool strikeout;
                    reader.Read(out strikeout);
                    bool underline;
                    reader.Read(out underline);
                    FontStyle style = FontStyle.Regular;
                    if (bold)
                    {
                        style = style | FontStyle.Bold;
                    }

                    if (italic)
                    {
                        style = style | FontStyle.Italic;
                    }

                    if (strikeout)
                    {
                        style = style | FontStyle.Strikeout;
                    }

                    if (underline)
                    {
                        style = style | FontStyle.Underline;
                    }

                    float size;
                    reader.Read(out size);
                    labelFont = new Font(fontFamily, size, style);
                }
                #endregion
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(tooltipScript);
            writer.Write(blockScript);
            writer.Write(onTimeSegmentScript);
            writer.Write(attributeSegmentScript);
            writer.Write(processSetupSegmentScript);
            writer.Write(processRunSegmentScript);
            writer.Write(processPostSegmentScript);
            writer.Write(processStorageSegmentScript);
            writer.Write(processStoragePostSegmentScript);
            writer.Write(statusSegmentScript);
            writer.Write(m_materialStatusSegmentScript);
            writer.Write(progressSegmentScript);
            writer.Write(prioritySegmentScript);
            writer.Write(commitmentSegmentScript);
            writer.Write(showBlockLabelAlways);
            writer.Write(showTimingSegment);
            writer.Write(showAttributesSegment);
            writer.Write(showJobColorAttribute);
            writer.Write(showProductColorAttribute);
            writer.Write(showSetupColorAttribute);
            writer.Write(showCustomAttributes);
            writer.Write(showBlockFlags);
            writer.Write(showActivityFlags);
            writer.Write(showOperationFlags);
            writer.Write(showManufacturingOrderFlags);
            writer.Write(showJobFlags);
            writer.Write(showMaterialFlags);
            writer.Write(showStandardFlags);
            writer.Write(showCustomFlags);

            writer.Write(showProcessSegment);
            writer.Write(showStatusSegment);
            writer.Write(m_showMaterialStatusSegment);
            writer.Write(showProgressSegment);
            writer.Write(showPrioritySegment);
            writer.Write(showCommitmentSegment);
            writer.Write(useSetupCodeColors);
            writer.Write(useFastDraw);
            writer.Write(noClipping);
            writer.Write(wrapText);
            writer.Write(useSolidColors);
            writer.Write((int)textDirection);
            writer.Write(m_padding);

            //Font
            writer.Write(labelFont.FontFamily.Name);
            writer.Write(labelFont.Bold);
            writer.Write(labelFont.Italic);
            writer.Write(labelFont.Strikeout);
            writer.Write(labelFont.Underline);
            writer.Write(labelFont.Size);

            writer.Write(highlightUnreviewedJobs);
            writer.Write(actualStatusScript);
            writer.Write(actualPerformanceScript);
            writer.Write(showBufferSegment);
            writer.Write(showCurrentDrumPenetration);
            writer.Write(bufferScript);

            writer.Write(ItemTypeFilterList.Count);
            foreach (ItemDefs.itemTypes itemType in ItemTypeFilterList)
            {
                writer.Write((int)itemType);
            }
        }

        public virtual int UniqueId => UNIQUE_ID;
        #endregion

        #region Scripts
        private string tooltipScript = "Slack: InternalActivity.Slack";

        /// <summary>
        /// Script used for generating Tooltips for Gantt Blocks.
        /// </summary>
        public string TooltipScript
        {
            get => tooltipScript;
            set => tooltipScript = value;
        }

        private string blockScript = "Job.Name ResourceOperation.Name";

        /// <summary>
        /// Script used for generating Labels for Gantt Blocks.
        /// </summary>
        public string BlockScript
        {
            get => blockScript;
            set => blockScript = value;
        }

        private string onTimeSegmentScript = "Job.Name\r\nManufacturingOrder.Name ManufacturingOrder.ProductName\r\nResourceOperation.Name\r\nInternalActivity.Timing InternalActivity.Slack slack\r\nNeedby: ResourceOperation.NeedDate\r\nConstraint: ResourceOperation.LatestConstraint  ResourceOperation.LatestConstraintDate\r\nQueue: InternalActivity.Queue";

        /// <summary>
        /// Script used for generating Labels for OnTime Segments of Gantt Blocks.
        /// </summary>
        public string OnTimeSegmentScript
        {
            get => onTimeSegmentScript;
            set => onTimeSegmentScript = value;
        }

        private string attributeSegmentScript = "ResourceOperation.SetupCode"; //Don't have Attribute labels working yet, so we'll hardcode this for our Setup Code Attribute that we default in every block.

        /// <summary>
        /// Script used for generating Labels for Attribute Segments of Gantt Blocks.
        /// </summary>
        public string AttributeSegmentScript
        {
            get => attributeSegmentScript;
            set => attributeSegmentScript = value;
        }

        private string processSetupSegmentScript = "Batch.StandardSetupSpan setup";

        /// <summary>
        /// Script used for generating Labels for the Setup portion of the Process Segments of Gantt Blocks.
        /// </summary>
        public string ProcessSetupSegmentScript
        {
            get => processSetupSegmentScript;
            set => processSetupSegmentScript = value;
        }

        private string processRunSegmentScript = "Batch.StandardProcessingSpan run";

        /// <summary>
        /// Script used for generating Labels for the Run portion of the Process Segments of Gantt Blocks.
        /// </summary>
        public string ProcessRunSegmentScript
        {
            get => processRunSegmentScript;
            set => processRunSegmentScript = value;
        }

        private string processPostSegmentScript = "Batch.StandardPostProcessingSpan post-process";

        /// <summary>
        /// Script used for generating Labels for the Post-Process portion of the Process Segments of Gantt Blocks.
        /// </summary>
        public string ProcessPostSegmentScript
        {
            get => processPostSegmentScript;
            set => processPostSegmentScript = value;
        }

        private string processStorageSegmentScript = "";

        /// <summary>
        /// Script used for generating Labels for the Storage portion of the Process Segments of Gantt Blocks.
        /// </summary>
        public string ProcessStorageSegmentScript
        {
            get => processStorageSegmentScript;
            set => processStorageSegmentScript = value;
        }

        private string processStoragePostSegmentScript = "";

        /// <summary>
        /// Script used for generating Labels for the Storage Post-Process portion of the Process Segments of Gantt Blocks.
        /// </summary>
        public string ProcessStoragePostSegmentScript
        {
            get => processStoragePostSegmentScript;
            set => processStoragePostSegmentScript = value;
        }

        private string statusSegmentScript = "InternalActivity.ProductionStatus";

        /// <summary>
        /// Script used for generating Labels for Status Segments of Gantt Blocks.
        /// </summary>
        public string StatusSegmentScript
        {
            get => statusSegmentScript;
            set => statusSegmentScript = value;
        }

        private string m_materialStatusSegmentScript = "";

        /// <summary>
        /// Script used for generating Labels for Material Status Segments of Gantt Blocks.
        /// </summary>
        public string MaterialStatusSegmentScript
        {
            get => m_materialStatusSegmentScript;
            set => m_materialStatusSegmentScript = value;
        }

        private string progressSegmentScript = "InternalActivity.PercentFinished % Finished";

        /// <summary>
        /// Script used for generating Labels for Progress Segments of Gantt Blocks.
        /// </summary>
        public string ProgressSegmentScript
        {
            get => progressSegmentScript;
            set => progressSegmentScript = value;
        }

        private string prioritySegmentScript = "";

        /// <summary>
        /// Script used for generating Labels for Priority Segments of Gantt Blocks.
        /// </summary>
        public string PrioritySegmentScript
        {
            get => prioritySegmentScript;
            set => prioritySegmentScript = value;
        }

        private string commitmentSegmentScript = "Job.Commitment\r\nReleased: ManufacturingOrder.IsReleased ManufacturingOrder.ReleaseDateTime";

        /// <summary>
        /// Script used for generating Labels for Commitment Segments of Gantt Blocks.
        /// </summary>
        public string CommitmentSegmentScript
        {
            get => commitmentSegmentScript;
            set => commitmentSegmentScript = value;
        }

        private string actualStatusScript = "Job.Name\r\nJob.Product  ResourceOperation.Name ";

        /// <summary>
        /// Script used for generating Labels for the Status segment of Actuals.
        /// </summary>
        public string ActualStatusScript
        {
            get => actualStatusScript;
            set => actualStatusScript = value;
        }

        private string actualPerformanceScript = "ResourceOperation.PercentOfStandard %  ResourceOperation.ExpectedHours / ResourceOperation.StandardHours";

        /// <summary>
        /// Script used for generating Labels for the Performance segment of Actuals.
        /// </summary>
        public string ActualPerformanceScript
        {
            get => actualPerformanceScript;
            set => actualPerformanceScript = value;
        }

        private string bufferScript = "ManufacturingOrder.DrumBufferCurrentPenetrationPercent %";

        /// <summary>
        /// Script used for generating Labels for Drum Buffer Penetration.
        /// </summary>
        public string BufferScript
        {
            get => bufferScript;
            set => bufferScript = value;
        }
        #endregion

        private bool showBlockLabelAlways;

        /// <summary>
        /// Whether to always show the block label, not just when there's not enough space.
        /// </summary>
        public bool ShowBlockLabelAlways
        {
            get => showBlockLabelAlways;
            set => showBlockLabelAlways = value;
        }

        private bool showTimingSegment = true;

        /// <summary>
        /// Whether to show the Segment for Timing.
        /// </summary>
        public bool ShowTimingSegment
        {
            get => showTimingSegment;
            set => showTimingSegment = value;
        }

        private bool showAttributesSegment;

        /// <summary>
        /// Whether to show the Segment for Attributes.
        /// </summary>
        public bool ShowAttributesSegment
        {
            get => showAttributesSegment;
            set => showAttributesSegment = value;
        }

        private bool showCustomAttributes = true;

        /// <summary>
        /// Whether to show other Attribute Color Codes in the Attributes Segment.
        /// </summary>
        public bool ShowCustomAttributes
        {
            get => showCustomAttributes;
            set => showCustomAttributes = value;
        }

        private bool showJobColorAttribute = true;

        /// <summary>
        /// Whether to show Job Color Code in the Attributes Segment.
        /// </summary>
        public bool ShowJobColorAttribute
        {
            get => showJobColorAttribute;
            set => showJobColorAttribute = value;
        }

        private bool showProductColorAttribute = true;

        /// <summary>
        /// Whether to show Product Color Code in the Attributes Segment.
        /// </summary>
        public bool ShowProductColorAttribute
        {
            get => showProductColorAttribute;
            set => showProductColorAttribute = value;
        }

        private bool showSetupColorAttribute = true;

        /// <summary>
        /// Whether to show Setup Color Code in the Attributes Segment.
        /// </summary>
        public bool ShowSetupColorAttribute
        {
            get => showSetupColorAttribute;
            set => showSetupColorAttribute = value;
        }

        private bool showBufferSegment;

        /// <summary>
        /// Whether to show the Drum Buffer Penetration Segment.
        /// </summary>
        public bool ShowBufferSegment
        {
            get => showBufferSegment;
            set => showBufferSegment = value;
        }

        private bool showCurrentDrumPenetration;

        /// <summary>
        /// Whether to show the Current rather than Projected Penetration.
        /// </summary>
        public bool ShowCurrentDrumPenetration
        {
            get => showCurrentDrumPenetration;
            set => showCurrentDrumPenetration = value;
        }

        #region Flags
        private bool showStandardFlags;

        /// <summary>
        /// Whether to show a Segment for each Standard Flag.
        /// </summary>
        public bool ShowStandardFlags
        {
            get => showStandardFlags;
            set => showStandardFlags = value;
        }

        private bool showCustomFlags;

        /// <summary>
        /// Whether to show a Segment for each Custom Flag.
        /// </summary>
        public bool ShowCustomFlags
        {
            get => showCustomFlags;
            set => showCustomFlags = value;
        }

        private bool showBlockFlags;

        /// <summary>
        /// Whether to show a Segment for each Block Flag.
        /// </summary>
        public bool ShowBlockFlags
        {
            get => showBlockFlags;
            set => showBlockFlags = value;
        }

        private bool showActivityFlags;

        /// <summary>
        /// Whether to show a Segment for each Activity Flag.
        /// </summary>
        public bool ShowActivityFlags
        {
            get => showActivityFlags;
            set => showActivityFlags = value;
        }

        private bool showOperationFlags;

        /// <summary>
        /// Whether to show a Segment for each Operation Flag.
        /// </summary>
        public bool ShowOperationFlags
        {
            get => showOperationFlags;
            set => showOperationFlags = value;
        }

        private bool showManufacturingOrderFlags;

        /// <summary>
        /// Whether to show a Segment for each ManufacturingOrder Flag.
        /// </summary>
        public bool ShowManufacturingOrderFlags
        {
            get => showManufacturingOrderFlags;
            set => showManufacturingOrderFlags = value;
        }

        private bool showJobFlags;

        /// <summary>
        /// Whether to show a Segment for each Job Flag.
        /// </summary>
        public bool ShowJobFlags
        {
            get => showJobFlags;
            set => showJobFlags = value;
        }

        private bool showMaterialFlags;

        /// <summary>
        /// Whether to show a Segment for each Material Flag.
        /// </summary>
        public bool ShowMaterialFlags
        {
            get => showMaterialFlags;
            set => showMaterialFlags = value;
        }
        #endregion

        private bool showProcessSegment = true;

        /// <summary>
        /// Whether to show the Segment for Process.
        /// </summary>
        public bool ShowProcessSegment
        {
            get => showProcessSegment;
            set => showProcessSegment = value;
        }

        private bool showStatusSegment = true;

        /// <summary>
        /// Whether to show the Segment for Status.
        /// </summary>
        public bool ShowStatusSegment
        {
            get => showStatusSegment;
            set => showStatusSegment = value;
        }

        private bool m_showMaterialStatusSegment;

        /// <summary>
        /// Whether to show the Segment for Material Status.
        /// </summary>
        public bool ShowMaterialStatusSegment
        {
            get => m_showMaterialStatusSegment;
            set => m_showMaterialStatusSegment = value;
        }

        private bool showProgressSegment;

        /// <summary>
        /// Whether to show the Segment for Activity Progress.
        /// </summary>
        public bool ShowProgressSegment
        {
            get => showProgressSegment;
            set => showProgressSegment = value;
        }

        private bool showPrioritySegment;

        /// <summary>
        /// Whether to show the Segment for Priority.
        /// </summary>
        public bool ShowPrioritySegment
        {
            get => showPrioritySegment;
            set => showPrioritySegment = value;
        }

        private bool showCommitmentSegment;

        /// <summary>
        /// Whether to show the Segment for Commitment.
        /// </summary>
        public bool ShowCommitmentSegment
        {
            get => showCommitmentSegment;
            set => showCommitmentSegment = value;
        }

        private bool useSetupCodeColors; //don't usually have so better to use default UI setup color

        /// <summary>
        /// Whether to use each Operation's SetupCode Color for showing setups on the gantt instead of the default setup color.
        /// </summary>
        public bool UseSetupCodeColors
        {
            get => useSetupCodeColors;
            set => useSetupCodeColors = value;
        }

        private bool highlightUnreviewedJobs;

        /// <summary>
        /// Whether to highlight Jobs in the Gantt that are not Reviewed.
        /// </summary>
        public bool HighlightUnreviewedJobs
        {
            get => highlightUnreviewedJobs;
            set => highlightUnreviewedJobs = value;
        }

        private bool useFastDraw;

        /// <summary>
        /// Whether to speed up gantt drawing by suppressing Labels and fancy coloring.
        /// </summary>
        public bool UseFastDraw
        {
            get => useFastDraw;
            set => useFastDraw = value;
        }

        private bool noClipping;

        /// <summary>
        /// Whether to clip label text that would overflow the block.
        /// </summary>
        public bool NoClipping
        {
            get => false; //noClipping; } 2017.03.20: removed because this was rarely used and whatever use it had should be done in a better way.
            set => noClipping = value;
        }

        private bool wrapText;

        /// <summary>
        /// Whether to wrap lines of Label text.
        /// </summary>
        public bool WrapText
        {
            get => wrapText;
            set => wrapText = value;
        }

        private bool useSolidColors = true;

        /// <summary>
        /// Use solid colors for Gantt Blocks and Capacity Intervals instead of dithered colors.
        /// </summary>
        public bool UseSolidColors
        {
            get => useSolidColors;
            set => useSolidColors = value;
        }

        public enum textDirections { Horizontal = 0, Vertical, Automatic }

        private textDirections textDirection = textDirections.Automatic;

        /// <summary>
        /// The direction that text displays in in Labels.
        /// </summary>
        public textDirections TextDirection
        {
            get => textDirection;
            set => textDirection = value;
        }

        private float m_padding = 2.00f;

        /// <summary>
        /// The pixels ofset to draw from the top and left.
        /// </summary>
        public float Padding
        {
            get => m_padding;
            set => m_padding = value;
        }

        private Font labelFont = new ("Verdana", 8);

        public Font LabelFont
        {
            get => labelFont;
            set => labelFont = value;
        }

        public List<ItemDefs.itemTypes> ItemTypeFilterList;

        #region ICloneable Members
        object ICloneable.Clone()
        {
            return Clone();
        }

        public LabelSettings Clone()
        {
            return (LabelSettings)MemberwiseClone();
        }
        #endregion
    }

    /// <summary>
    /// Stores the user-specific settings for Gantt Simplification.
    /// </summary>
    [Serializable]
    public class SimplifyGanttSettings : ICloneable, IPTSerializable
    {
        public const int UNIQUE_ID = 437;

        #region IPTSerializable Members
        public SimplifyGanttSettings(IReader reader)
        {
            if (reader.VersionNumber >= 27)
            {
                reader.Read(out hideActivities);
                reader.Read(out hideActivitiesAfter);
                reader.Read(out hideCapacityIntervals);
                reader.Read(out hideCapacityIntervalsAfter);

                flags = new BoolVector32(reader);
                flags2 = new BoolVector32(reader); //new in version 27
            }
            else if (reader.VersionNumber >= 1)
            {
                reader.Read(out hideActivities);
                reader.Read(out hideActivitiesAfter);
                reader.Read(out hideCapacityIntervals);
                reader.Read(out hideCapacityIntervalsAfter);

                flags = new BoolVector32(reader);
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(hideActivities);
            writer.Write(hideActivitiesAfter);
            writer.Write(hideCapacityIntervals);
            writer.Write(hideCapacityIntervalsAfter);

            flags.Serialize(writer);
            flags2.Serialize(writer);
        }

        public virtual int UniqueId => UNIQUE_ID;
        #endregion

        public SimplifyGanttSettings() { }

        private BoolVector32 flags;

        #region Time Hiding Properties
        private const int HideMondaysIdx = 0;

        public bool HideMondays
        {
            get => flags[HideMondaysIdx];
            set => flags[HideMondaysIdx] = value;
        }

        private const int HideTuesdaysIdx = 1;

        public bool HideTuesdays
        {
            get => flags[HideTuesdaysIdx];
            set => flags[HideTuesdaysIdx] = value;
        }

        private const int HideWednesdaysIdx = 2;

        public bool HideWednesdays
        {
            get => flags[HideWednesdaysIdx];
            set => flags[HideWednesdaysIdx] = value;
        }

        private const int HideThursdaysIdx = 3;

        public bool HideThursdays
        {
            get => flags[HideThursdaysIdx];
            set => flags[HideThursdaysIdx] = value;
        }

        private const int HideFridaysIdx = 4;

        public bool HideFridays
        {
            get => flags[HideFridaysIdx];
            set => flags[HideFridaysIdx] = value;
        }

        private const int HideSaturdaysIdx = 5;

        public bool HideSaturdays
        {
            get => flags[HideSaturdaysIdx];
            set => flags[HideSaturdaysIdx] = value;
        }

        private const int HideSundaysIdx = 6;

        public bool HideSundays
        {
            get => flags[HideSundaysIdx];
            set => flags[HideSundaysIdx] = value;
        }

        public bool AllDaysHidden => HideSundays && HideMondays && HideTuesdays && HideWednesdays && HideThursdays && HideFridays && HideSaturdays;

        private const int HideHourIdx0 = 7;

        public bool HideHour0
        {
            get => flags[HideHourIdx0];
            set => flags[HideHourIdx0] = value;
        }

        private const int HideHour1Idx = 8;

        public bool HideHour1
        {
            get => flags[HideHour1Idx];
            set => flags[HideHour1Idx] = value;
        }

        private const int HideHour2Idx = 9;

        public bool HideHour2
        {
            get => flags[HideHour2Idx];
            set => flags[HideHour2Idx] = value;
        }

        private const int HideHour3Idx = 10;

        public bool HideHour3
        {
            get => flags[HideHour3Idx];
            set => flags[HideHour3Idx] = value;
        }

        private const int HideHour4Idx = 11;

        public bool HideHour4
        {
            get => flags[HideHour4Idx];
            set => flags[HideHour4Idx] = value;
        }

        private const int HideHour5Idx = 12;

        public bool HideHour5
        {
            get => flags[HideHour5Idx];
            set => flags[HideHour5Idx] = value;
        }

        private const int HideHour6Idx = 13;

        public bool HideHour6
        {
            get => flags[HideHour6Idx];
            set => flags[HideHour6Idx] = value;
        }

        private const int HideHour7Idx = 14;

        public bool HideHour7
        {
            get => flags[HideHour7Idx];
            set => flags[HideHour7Idx] = value;
        }

        private const int HideHour8Idx = 15;

        public bool HideHour8
        {
            get => flags[HideHour8Idx];
            set => flags[HideHour8Idx] = value;
        }

        private const int HideHour9Idx = 16;

        public bool HideHour9
        {
            get => flags[HideHour9Idx];
            set => flags[HideHour9Idx] = value;
        }

        private const int HideHour10Idx = 17;

        public bool HideHour10
        {
            get => flags[HideHour10Idx];
            set => flags[HideHour10Idx] = value;
        }

        private const int HideHour11Idx = 18;

        public bool HideHour11
        {
            get => flags[HideHour11Idx];
            set => flags[HideHour11Idx] = value;
        }

        private const int HideHour12Idx = 19;

        public bool HideHour12
        {
            get => flags[HideHour12Idx];
            set => flags[HideHour12Idx] = value;
        }

        private const int HideHour13Idx = 20;

        public bool HideHour13
        {
            get => flags[HideHour13Idx];
            set => flags[HideHour13Idx] = value;
        }

        private const int HideHour14Idx = 21;

        public bool HideHour14
        {
            get => flags[HideHour14Idx];
            set => flags[HideHour14Idx] = value;
        }

        private const int HideHour15Idx = 22;

        public bool HideHour15
        {
            get => flags[HideHour15Idx];
            set => flags[HideHour15Idx] = value;
        }

        private const int HideHour16Idx = 23;

        public bool HideHour16
        {
            get => flags[HideHour16Idx];
            set => flags[HideHour16Idx] = value;
        }

        private const int HideHour17Idx = 24;

        public bool HideHour17
        {
            get => flags[HideHour17Idx];
            set => flags[HideHour17Idx] = value;
        }

        private const int HideHour18Idx = 25;

        public bool HideHour18
        {
            get => flags[HideHour18Idx];
            set => flags[HideHour18Idx] = value;
        }

        private const int HideHour19Idx = 26;

        public bool HideHour19
        {
            get => flags[HideHour19Idx];
            set => flags[HideHour19Idx] = value;
        }

        private const int HideHour20Idx = 27;

        public bool HideHour20
        {
            get => flags[HideHour20Idx];
            set => flags[HideHour20Idx] = value;
        }

        private const int HideHour21Idx = 28;

        public bool HideHour21
        {
            get => flags[HideHour21Idx];
            set => flags[HideHour21Idx] = value;
        }

        private const int HideHour22Idx = 29;

        public bool HideHour22
        {
            get => flags[HideHour22Idx];
            set => flags[HideHour22Idx] = value;
        }

        private const int HideHour23Idx = 30;

        public bool HideHour23
        {
            get => flags[HideHour23Idx];
            set => flags[HideHour23Idx] = value;
        }

        public bool AllHoursHidden =>
            HideHour0 &&
            HideHour1 &&
            HideHour2 &&
            HideHour3 &&
            HideHour4 &&
            HideHour5 &&
            HideHour6 &&
            HideHour7 &&
            HideHour8 &&
            HideHour9 &&
            HideHour10 &&
            HideHour11 &&
            HideHour12 &&
            HideHour13 &&
            HideHour14 &&
            HideHour15 &&
            HideHour16 &&
            HideHour17 &&
            HideHour18 &&
            HideHour19 &&
            HideHour20 &&
            HideHour21 &&
            HideHour22 &&
            HideHour23;
        #endregion

        #region Speed-up Properties
        private bool hideCapacityIntervals;

        public bool HideCapacityIntervals
        {
            get => hideCapacityIntervals;
            set => hideCapacityIntervals = value;
        }

        private TimeSpan hideCapacityIntervalsAfter = new (0);

        public TimeSpan HideCapacityIntervalsAfter
        {
            get => hideCapacityIntervalsAfter;
            set => hideCapacityIntervalsAfter = value;
        }

        private bool hideActivities;

        public bool HideActivities
        {
            get => hideActivities;
            set => hideActivities = value;
        }

        private TimeSpan hideActivitiesAfter = new (28, 0, 0, 0, 0);

        public TimeSpan HideActivitiesAfter
        {
            get => hideActivitiesAfter;
            set => hideActivitiesAfter = value;
        }

        public bool HideTankPlots
        {
            get => flags2[hideTankPlotsIdx];
            set => flags2[hideTankPlotsIdx] = value;
        }

        public bool AlternateCampaignColorsByItemGroup
        {
            get => flags2[c_alternateCampaignColorsByItemGroup];
            set => flags2[c_alternateCampaignColorsByItemGroup] = value;
        }
        #endregion

        #region Bools
        private BoolVector32 flags2;
        private const int c_canDragAndResizeOnlineGCIsIdx = 0;
        private const int c_canDragAndResizeOfflineGCIsIdx = 1;
        private const int c_canDeleteGCIsIdx = 2;
        private const int hideTankPlotsIdx = 5;
        private const int c_alternateCampaignColorsByItemGroup = 6;
        #endregion Bools

        #region GCI Move/Delete Properties
        public bool DragAndResizeOnlineGCIs
        {
            get => flags2[c_canDragAndResizeOnlineGCIsIdx];
            set => flags2[c_canDragAndResizeOnlineGCIsIdx] = value;
        }
        public bool DragAndResizeOfflineGCIs
        {
            get => flags2[c_canDragAndResizeOfflineGCIsIdx];
            set => flags2[c_canDragAndResizeOfflineGCIsIdx] = value;
        }
        public bool DeleteGCIs
        {
            get => flags2[c_canDeleteGCIsIdx];
            set => flags2[c_canDeleteGCIsIdx] = value;
        }
        #endregion

        object ICloneable.Clone()
        {
            return Clone();
        }

        public SimplifyGanttSettings Clone()
        {
            return (SimplifyGanttSettings)MemberwiseClone();
        }
    }
}

/// <summary>
/// Stores a User's settings for displaying the Capacity Plan.
/// </summary>
[Obsolete("Used for backwards compatibility only")]
internal class CapacityPlanOptions
{
    #region IPTSerializable Members
    public CapacityPlanOptions()
    {
        SetDefaultCapacityPlanOptions(true);
    }

    public CapacityPlanOptions(IReader reader)
    {
        #region 600
        if (reader.VersionNumber >= 600)
        {
            reader.Read(out m_capacityType);
            reader.Read(out m_populateType);
            reader.Read(out m_commitmentType);
            reader.Read(out m_chartType);
            long bucketTicks = 0;
            reader.Read(out bucketTicks);
            bucketDuration = new TimeSpan(bucketTicks);
            reader.Read(out m_showLegend);
            reader.Read(out m_useGantt);
            reader.Read(out m_startSpanComboBox);
            reader.Read(out m_startSpanCustomDateTime);
            reader.Read(out m_endSpanComboBox);
            reader.Read(out m_endSpanCustomDateTime);
            reader.Read(out m_trackbarValue);
            reader.Read(out m_trackbarComboBox);
        }
        #endregion

        #region 520
        else if (reader.VersionNumber >= 520)
        {
            //This is the same as the 430 reader. Needed to fix an incompatibility with the release branch
            reader.Read(out m_capacityType);
            reader.Read(out m_populateType);
            reader.Read(out m_commitmentType);
            reader.Read(out m_chartType);
            long bucketTicks = 0;
            reader.Read(out bucketTicks);
            bucketDuration = new TimeSpan(bucketTicks);
            reader.Read(out m_showLegend);
        }
        #endregion

        #region 518
        else if (reader.VersionNumber >= 518)
        {
            reader.Read(out m_capacityType);
            reader.Read(out m_populateType);
            reader.Read(out m_commitmentType);
            reader.Read(out m_chartType);
            long bucketTicks = 0;
            reader.Read(out bucketTicks);
            bucketDuration = new TimeSpan(bucketTicks);
            reader.Read(out m_showLegend);
            reader.Read(out m_useGantt);
            reader.Read(out m_startSpanComboBox);
            reader.Read(out m_startSpanCustomDateTime);
            reader.Read(out m_endSpanComboBox);
            reader.Read(out m_endSpanCustomDateTime);
            reader.Read(out m_trackbarValue);
            reader.Read(out m_trackbarComboBox);
        }
        #endregion

        #region 430
        else if (reader.VersionNumber >= 430)
        {
            reader.Read(out m_capacityType);
            reader.Read(out m_populateType);
            reader.Read(out m_commitmentType);
            reader.Read(out m_chartType);
            long bucketTicks = 0;
            reader.Read(out bucketTicks);
            bucketDuration = new TimeSpan(bucketTicks);
            reader.Read(out m_showLegend);
            SetDefaultCapacityPlanOptions(false);
        }
        #endregion
    }

    private void SetDefaultCapacityPlanOptions(bool a_setBucketDuration)
    {
        if (!a_setBucketDuration)
        {
            bucketDuration = new TimeSpan(7, 0, 0, 0);
        }

        StartSpanComboBox = 0;
        EndSpanComboBox = 3;
        UseGantt = true;
        TrackbarValue = 1;
        StartSpanCustomDateTime = DateTime.MinValue;
        EndSpanCustomDateTime = DateTime.MinValue;
        TrackbarComboBox = new TimeSpan(1, 0, 0, 0);
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(m_capacityType);
        writer.Write(m_populateType);
        writer.Write(m_commitmentType);
        writer.Write(m_chartType);
        writer.Write(bucketDuration.Ticks);
        writer.Write(m_showLegend);
        writer.Write(m_useGantt);
        writer.Write(m_startSpanComboBox);
        writer.Write(m_startSpanCustomDateTime);
        writer.Write(m_endSpanComboBox);
        writer.Write(m_endSpanCustomDateTime);
        writer.Write(m_trackbarValue);
        writer.Write(m_trackbarComboBox);
    }
    #endregion

    private TimeSpan bucketDuration;

    /// <summary>
    /// The amount of time displayed in each interval.
    /// </summary>
    public TimeSpan BucketDuration
    {
        get => bucketDuration;
        set => bucketDuration = value;
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
}

/// <summary>
/// Stores a User's settings for displaying the Capacity Plan.
/// </summary>
public class ActivitySchedulingGridOptions
{
    #region IPTSerializable Members
    public ActivitySchedulingGridOptions() { }

    public ActivitySchedulingGridOptions(IReader reader)
    {
        if (reader.VersionNumber >= 490)
        {
            m_bools = new BoolVector32(reader);
            reader.Read(out m_secondaryTabIndex);
            reader.Read(out m_secondaryGridHeight);
            reader.Read(out m_primaryGridResourcesWidth);
            reader.Read(out m_secondaryGridResourceWidth);
        }
        else if (reader.VersionNumber >= 489)
        {
            m_bools = new BoolVector32(reader);
            reader.Read(out m_secondaryTabIndex);
        }
    }

    public void Serialize(IWriter writer)
    {
        m_bools.Serialize(writer);
        writer.Write(m_secondaryTabIndex);
        writer.Write(m_secondaryGridHeight);
        writer.Write(m_primaryGridResourcesWidth);
        writer.Write(m_secondaryGridResourceWidth);
    }
    #endregion

    #region Bools 1
    private BoolVector32 m_bools;

    private const short c_showMoveValidationIdx = 0; //Can be reused for something
    private const short c_showMoveWarningsIdx = 1; //Can be reused
    private const short c_lockMoveIdx = 2;
    private const short c_anchorMoveIdx = 3;
    private const short c_expediteMoveIdx = 4;
    private const short c_exactMoveIdx = 5;
    private const short c_syncWithGanttIdx = 6;
    private const short c_unused2Idx = 7;
    private const short c_unused3Idx = 8;
    private const short c_unused4Idx = 9;
    private const short c_secondaryGridCollapsedIdx = 10;
    private const short c_primaryGridResGridCollapsedIdx = 11;
    private const short c_secondaryGridResGridCollapsedIdx = 12;
    private const short c_primaryGridToolbarCollapsedIdx = 13;
    private const short c_secondaryGridToolbarCollapsedIdx = 14;

    /// <summary>
    /// Whether to show validation errors
    /// </summary>
    public bool ShowMoveValidation
    {
        get => m_bools[c_showMoveValidationIdx];
        set => m_bools[c_showMoveValidationIdx] = value;
    }

    /// <summary>
    /// Whether to show move validation warnings
    /// </summary>
    public bool ShowMoveWarnings
    {
        get => m_bools[c_showMoveWarningsIdx];
        set => m_bools[c_showMoveWarningsIdx] = value;
    }

    /// <summary>
    /// Whether to select activities and resources when they are selectedon the gantt
    /// </summary>
    public bool SyncWithGantt
    {
        get => m_bools[c_syncWithGanttIdx];
        set => m_bools[c_syncWithGanttIdx] = value;
    }

    /// <summary>
    /// Whether to lock blocks after move
    /// </summary>
    public bool LockMove
    {
        get => m_bools[c_lockMoveIdx];
        set => m_bools[c_lockMoveIdx] = value;
    }

    /// <summary>
    /// Whether to anchor blocks after move
    /// </summary>
    public bool AnchorMove
    {
        get => m_bools[c_anchorMoveIdx];
        set => m_bools[c_anchorMoveIdx] = value;
    }

    /// <summary>
    /// Whether to expedite successors after move
    /// </summary>
    public bool ExpediteMove
    {
        get => m_bools[c_expediteMoveIdx];
        set => m_bools[c_expediteMoveIdx] = value;
    }

    /// <summary>
    /// Whether to expedite successors after move
    /// </summary>
    public bool ExactMove
    {
        get => m_bools[c_exactMoveIdx];
        set => m_bools[c_exactMoveIdx] = value;
    }

    /// <summary>
    /// The tab to display in the secondary pane.
    /// </summary>
    private short m_secondaryTabIndex;

    public short SecondaryPaneTab
    {
        get => m_secondaryTabIndex;
        set => m_secondaryTabIndex = value;
    }

    /// <summary>
    /// Whether the secondary grid is collapsed
    /// </summary>
    public bool SecondaryGridCollapsed
    {
        get => m_bools[c_secondaryGridCollapsedIdx];
        set => m_bools[c_secondaryGridCollapsedIdx] = value;
    }

    /// <summary>
    /// Whether the primary grid resource grid is collapsed
    /// </summary>
    public bool PrimaryGridResGridCollapsed
    {
        get => m_bools[c_primaryGridResGridCollapsedIdx];
        set => m_bools[c_primaryGridResGridCollapsedIdx] = value;
    }

    /// <summary>
    /// Whether the secondary grid resource grid is collapsed
    /// </summary>
    public bool SecondaryGridResGridCollapsed
    {
        get => m_bools[c_secondaryGridResGridCollapsedIdx];
        set => m_bools[c_secondaryGridResGridCollapsedIdx] = value;
    }

    /// <summary>
    /// Whether the primary grid toolbar is displayed
    /// </summary>
    public bool PrimaryGridToolbarVisible
    {
        get => m_bools[c_primaryGridToolbarCollapsedIdx];
        set => m_bools[c_primaryGridToolbarCollapsedIdx] = value;
    }

    /// <summary>
    /// Whether the secdonary grid toolbar is displayed
    /// </summary>
    public bool SecondaryGridToolbarVisible
    {
        get => m_bools[c_secondaryGridToolbarCollapsedIdx];
        set => m_bools[c_secondaryGridToolbarCollapsedIdx] = value;
    }
    #endregion

    private int m_secondaryGridHeight;

    /// <summary>
    /// Height of the secondary grid
    /// </summary>
    public int SecondaryGridHeight
    {
        get => m_secondaryGridHeight;
        set => m_secondaryGridHeight = value;
    }

    private int m_primaryGridResourcesWidth;

    /// <summary>
    /// Height of the primary grid resources grid
    /// </summary>
    public int PrimaryGridResourcesWidth
    {
        get => m_primaryGridResourcesWidth;
        set => m_primaryGridResourcesWidth = value;
    }

    private int m_secondaryGridResourceWidth;

    /// <summary>
    /// Height of the secondary grid resources grid
    /// </summary>
    public int SecondaryGridResourceWidth
    {
        get => m_secondaryGridResourceWidth;
        set => m_secondaryGridResourceWidth = value;
    }
}