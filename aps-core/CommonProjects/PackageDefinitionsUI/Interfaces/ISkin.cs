using System.Drawing;

using PT.PackageDefinitions;

namespace PT.PackageDefinitionsUI.Interfaces;

public interface IDynamicSkin
{
    //Colors
    Color CriticalAlert { get; }
    Color HighAlert { get; }
    Color WarningAlert { get; }
    Color LowAlert { get; }
    Color InformationAlert { get; }
    Color PT12BackgroundBlack { get; }
    Color ControlBackColor { get; }
    Color ButtonTextLink { get; }
    Color ControlText { get; }
    Color DefaultDocumentColor { get; }

    Color WarningColor { get; }
    Color UIWarningColor { get; }
    Color OkColor { get; }
    Color PriorityLow { get; }
    Color PriorityMedium { get; }
    Color PriorityHigh { get; }

    //Fonts
    Font NormalFont { get; }
    Font SelectedFont { get; }
    Font TooltipFont { get; }
    Font LegendFont { get; }
    Font HeaderFont { get; }
    Font LargeFont { get; }

    //Version12 General Colors
    //TODO: Not sure about color named colors that load from a palette
    Color BlueBright { get; }
    Color BlueDark { get; }
    Color TealLight { get; }
    Color TealMed { get; }
    Color TealDark { get; }
    Color GreyLight { get; }
    Color GreyMed { get; }
    Color GreyDark { get; }
    Color YellowLight { get; }
    Color YellowMedium { get; }
    Color OrangeMed { get; }
    Color OrangeDark { get; }
    Color RedLight { get; }
    Color RedMed { get; }
    Color RedDark { get; }
    Color GreenLight { get; }
    Color GreenLight2 { get; }
    Color GreenMed { get; }
    Color GreenDark { get; }
    Color PurpleMed { get; }
    Color PurpleDark { get; }
    Color JungleGreen { get; }
    Color JungleGreenLight { get; }
    Color JungleGreenMedium { get; }
    Color JungleGreenDark { get; }
    Color JungleGreenHighlight { get; }
    Color OptimizeAccent { get; }

    Color LinkSuccessorMoColor { get; }
    Color LinkMaterialColor { get; }
    Color LinkExpiredMaterialColor { get; }
    Color LinkPossibleMaterialColor { get; }
    Color LinkProductColor { get; }
    Color LinkHelperResourceColor { get; }
    Color Successor { get; }
    Color Predecessor { get; }
    Color LinkParallelMOColor { get; }
    Color LinkColorConfirmedConstraint { get; }

    Color DefaultAppearanceColor { get; }
    Color PTBlueHighlight { get; }
    Color PTBlue { get; }
    Color HotColor { get; }
    Color InventoryBoardColor { get; }
    Color JobBoardColor { get; }
    Color TemplateBoardColor { get; }
    Color ActivityBoardColor { get; }
    Color CapacityBoardColor { get; }
    Color MetricRed { get; }
    Color MetricGreen { get; }

    //Light or Dark Theme
    bool IsDarkTheme { get; }

    //Themed Colors
    Color AccentPaint { get; }
    Color AccentPaintDark { get; }
    Color AccentPaintLight { get; }
    Color AccentPaintLighter { get; }
    Color ComplementAccent { get; }
    Color ComplementAccent2 { get; }
    Color GridLines { get; }
    Color TextColor { get; }
    Color TextHighlightColor { get; }
    Color ThemedBackgroundColor { get; }
    Color ThemedReadOnly { get; }
    Color GridCellModified { get; }

    //Metrics
    Color MetricCritical { get; }
    Color MetricHigh { get; }
    Color MetricMed { get; }
    Color MetricLow { get; }
    Color MetricInfo { get; }
    Color MetricDefault { get; }

    //Commitment Colors
    Color CommitmentEstimate { get; }
    Color CommitmentPlanned { get; }
    Color CommitmentFirm { get; }
    Color CommitmentReleased { get; }

    //Capacity Interval Colors
    Color CapacityIntervalOnlineColor { get; }
    Color CapacityIntervalOnlineColorHatch { get; }
    Color CapacityIntervalOvertimeColor { get; }
    Color CapacityIntervalOvertimeColorHatch { get; }
    Color CapacityIntervalPotentialOvertimeColor { get; }
    Color CapacityIntervalPotentialOvertimeColorHatch { get; }
    Color CapacityIntervalOfflineColor { get; }
    Color CapacityIntervalOfflineColorHatch { get; }
    Color CapacityIntervalCleanoutColor { get; }
    Color CapacityIntervalCleanoutColorHatch { get; }
    Color CapacityIntervalHolidayColor { get; }
    Color CapacityIntervalHolidayColorHatch { get; }
    Color CapacityIntervalCanStartActivityColor { get; }
    Color CapacityIntervalPreventOperationsFrmSpanningColor{ get; }
    Color CapacityIntervalAttributesChangeOverColor { get; }
    Color CapacityIntervalUseOnlyWhenLateColor { get; }
    Color CapacityIntervalMaintenanceColor { get; }


    //Block Status colors
    Color WaitingColor { get; }
    Color ReadyColor { get; }
    Color StartedColor { get; }
    Color SettingUpColor { get; }
    Color RunningColor { get; }
    Color PostProcessingColor { get; }
    Color CleanColor { get; }
    Color StorageColor { get; }
    Color StoragePlotColor { get; }
    Color StoragePostProcessColor { get; }
    Color StorageReleaseTimingColor { get; }
    Color StorageExpirationColor { get; }
    Color StorageMaterialColor { get; }
    Color TransferringColor { get; }
    Color PausedColor { get; }
    Color FinishedColor { get; }
    Color OmittedColor { get; }

    Color UnreviewedColor { get; }

    // Block Material Status Colors
    Color MaterialsAvailable { get; }
    Color MaterialSourcesFirmColor { get; }
    Color MaterialSourcesPlannedColor { get; }
    Color MaterialSourceUnknownColor { get; }
    Color MaterialConstraintViolationColor { get; }

    //Hint Infos
    Color HintInfoOpNeed { get; }
    Color HintInfoLatestConstraint { get; }
    Color HintInfoPredecessorConstraint { get; }
    Color HintInfoDrumDueDate { get; }
    Color HintInfoReleaseDate { get; }
    Color HintInfoShippingDate { get; }
    Color HintInfoNow { get; }
    Color HintInfoJobFinish { get; }
    Color HintInfoMaterialAvailability { get; }
    Color HintInfoProductConsumedColor { get; }
    Color HintInfoOptimizeHeadStart { get; }
    Color HintInfoCompressHeadStart { get; }
    Color HintInfoHeadStartWindowEnd { get; }
    Color HintInfoExpcetedMaterialReceipt { get; }
    Color HintInfoOptimizeStartDate { get; }
    Color HintInfoEarliestConstraint { get; }
    Color MaxDelay { get; }

    Color LateColor { get; }
    Color AlmostLateColor { get; }
    Color OnTimeColor { get; }
    Color TooEarlyColor { get; }

    //Actuals Colors
    Color ActualPerformanceFast { get; }
    Color ActualPerformanceNormal { get; }
    Color ActualPerformanceSlow { get; }
    Color ActualStatusFinished { get; }
    Color ActualStatusPartiallyFinished { get; }

    //Block Borders
    Color CommitmentDriftColor { get; }
    Color AnchorDriftColor { get; }
    Color GanttBlockSelectionColor { get; }
    Color GanttBlockBorderColor { get; }
    Color GanttBlockFillColor { get; }

    //Gantt Colors
    Color GanttPastColor { get; }
    Color GanttClockColor { get; }
    Color GanttNowColor { get; }
    Color GanttOptimizeStartColor { get; }
    Color GanttFrozenSpanColor { get; }
    Color GanttStableSpanColor { get; }
    Color GanttFreeSpanColor { get; }
    Color GanttFeasibleSpanEndColor { get; }
    Color GanttShortTermSpanEndColor { get; }
    Color GanttHideActivitiesStartColor { get; }
    Color GanttHideCapacityIntervalsStartColor { get; }
    Color GanttAfterPlanningHorizonColor { get; }
    Color GanttPublishedColor { get; }
    Color GanttWhatifColor { get; }
    Color GanttBackColor { get; }
    Color GanttGridBackColor { get; }
    Color GanttHashColor { get; }
    Color Anchor { get; }
    Color GanttDaylightSavingsAdjustmentColor { get;}

    //Message Classification Tints
    Color PTMessageInfo { get; }
    Color PTMessagePrompt { get; }
    Color PTMessageWarning { get; }
    Color PTMessageError { get; }
    Color PTMessageDefault { get; }

    //Notification Tiles
    Color OptimizationColor { get; }

    //Constraints
    Color BottleneckCapacityColor { get; }
    Color BottleneckMaterialColor { get; }
    Color BottleneckReleaseColor { get; }
    Color OnHoldColor { get; }
    Color ConstraintEligibilityColor { get; }
    Color ConstraintLockedColor { get; }
    Color ConstraintAlternatePathColor { get; }
    Color ConstraintMaterialColor { get; }
    Color ConstraintInProductionColor { get; }
    Color ConstraintClockColor { get; }
    Color ConstraintCapacityColor { get; }
    Color ConstraintPredecessorColor { get; }
    Color MoveComplete { get; }
    Color MoveNotAllowed { get; }

    Color AdjacentJobBorderColor { get; }

    //UI Colors
    Color BoardMenuTextColor { get; }
    Color BoardMenuBorderColor { get; }
    Color BoardMenuTextHighlightColor { get; }
    Color Disabled { get; }
    Color ThemeChooserColor { get; }
    Color MainMenuDropdownItemOutlineColor { get; }
    Color SplitterColor { get; }
    Color TextColorInverted { get; }
    Color LeadTime { get; }
    Color TextHeaderColor { get; }
    Color ReverseHighlight { get; }
    Color GanttCampaignItemColor1 { get; }
    Color GanttCampaignItemColor2 { get; }
    Color Transparent { get; }
    Color GanttHighlightColor { get; }
    Color CheckBoxUnchecked { get; }
    Color GanttPyjamasColor { get; }
    Color PyjamasColor { get; }
    Color GridPyjamasColor { get; }
    Color PyjamasGradientColor { get; }
    Color TimeItemLayoutDefaultColor { get; }
    Color TimeItemLayoutNeutralColor { get; }
    Color SeriesTotal { get; }
    Color SeriesAverage { get; }
    Color SeriesMax { get; }
    Color SeriesMin { get; }
    Color ErrorCellBackground { get; }
    Color ErrorCellForeground { get; }
    Color PriorityColorDefault { get; }
    Color GanttTimeLines { get; }
    Color KPIIncrease { get; }
    Color KPIDecrease { get; }
    Color KPIChange { get; }
    Color GanttCampaignFillColor { get; }
    Color GanttDateScalerFishEyeColor { get; }
    Color SpanTextColor { get; }
    Color TimeItemLayoutHotColor { get; }
    Color ChartLineColor { get; }
    Color StrongPasswordIndicator { get; }
    Color WeakPasswordIndicator { get; }

    //Buffers
    Color ShippingBuffer { get; }
    Color OperationBuffer { get; }
    Color SequenceHeadStart { get; }
    Color DbrJitStart { get; }
    Color DbrNeedDate { get; }

    //Event
    event Action ThemeChanged;

    void FireThemeChangedEvent();

    Color GetUrgencyColor(PackageEnums.ENotificationUrgency a_urgency);

    Color GetColorFromKey(string a_paletteKey);
}