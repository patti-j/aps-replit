using System.Drawing;

using PlexityHide.GTP;

using PT.APSCommon;
using PT.PackageDefinitions;
using PT.PackageDefinitionsUI.GanttElementSettings;
using PT.Scheduler;
using PT.SchedulerDefinitions;

using static PT.PackageDefinitions.PackageEnums;

namespace PT.PackageDefinitionsUI;

/// <summary>
/// This module creates and maintains settings for all gantt related elements
/// </summary>
public interface IGanttModule
{
    List<IGanttLineElement> GetGanttLines()
    {
        return new List<IGanttLineElement>();
    }

    List<IMultiGanttLineElement> GetMultiGanttLines()
    {
        return new List<IMultiGanttLineElement>();
    }

    List<IGanttRegionElement> GetGanttRegions()
    {
        return new List<IGanttRegionElement>();
    }

    List<IMultiGanttRegionElement> GetMultiGanttRegions()
    {
        return new List<IMultiGanttRegionElement>();
    }

    List<IGanttBlockBorderElement> GenerateBlockBorders(ResourceBlock a_block, ScenarioDetail a_sd, ResourceBlock a_previousBlockOrNull, ResourceBlock a_nextBlockOrNull)
    {
        return new List<IGanttBlockBorderElement>();
    }

    List<IGanttHighlightElement> GenerateBlockHighlighters(ResourceBlock a_block, ScenarioDetail a_sd, ResourceBlock a_previousBlockOrNull, ResourceBlock a_nextBlockOrNull)
    {
        return new List<IGanttHighlightElement>();
    }

    List<IGanttSegmentElement> GenerateSegments(IGanttBlock a_gBlock, ResourceBlock a_block, ScenarioDetail a_sd, ILabelScriptGenerator a_labelGenerator)
    {
        return new List<IGanttSegmentElement>();
    }

    List<IGanttModuleElement> GenerateExtensionModules(IScenarioInfo a_scenarioInfo, Gantt a_gantt, IGanttDataSource a_dataSource, IWorkspaceInfo a_settingsManager, bool a_enablePrimaryGanttModules)
    {
        return new List<IGanttModuleElement>();
    }

    List<GanttBlockRelation> GenerateBlockLinks(BlockKey a_block, ScenarioDetail a_sd)
    {
        return new List<GanttBlockRelation>();
    }

    List<IGanttEligibilityIndicatorElement> GenerateEligibilityIndicators()
    {
        return new List<IGanttEligibilityIndicatorElement>();
    }
    List<IGanttBlockMovedExtension> GetGanttBlockMovedExtensions()
    {
        return new List<IGanttBlockMovedExtension>();
    }
    List<string> SettingKeys => new();
}

/// <summary>
/// A scheduled block that is represented on the Gantt
/// </summary>
public interface IGanttBlock
{
    bool Hidden { get; }
    bool Faded { get; }
    bool ParentExists { get; }
    bool ShowActivityBoard { get; }

    DateTimeOffset ScheduledStart { get; }
    DateTimeOffset ScheduledEnd { get; }
    bool JobReviewed { get; set; }
    BlockKey BlockKey { get; }
    bool PendingMove { get; }

    bool CanMoveToProposedTime(DateTime a_newStart, out DateTime a_releaseDateConstraint);
}

/// <summary>
/// A scheduled block that is represented on the Gantt
/// </summary>
public interface IGanttResource
{
    BaseId ResourceId { get; set; }
}

/// <summary>
/// Extension module for the gantt controls
/// </summary>
public interface IGanttModuleElement : IDisposable { }

/// <summary>
/// A vertical line drawn on the gantt with an optional display text
/// </summary>
public interface IGanttLineElement : IPackageElement
{
    /// <summary>
    /// Displays as a label segment on the gantt adjacent to the line (on the right side)
    /// This will be localized
    /// </summary>
    string DisplayText { get; }

    /// <summary>
    /// The date to draw the line
    /// </summary>
    DateTimeOffset DateValue { get; }

    /// <summary>
    /// Whether to draw this line
    /// </summary>
    bool Draw { get; }

    /// <summary>
    /// Whether to draw this line extra thick
    /// </summary>
    bool Bold => false;

    /// <summary>
    /// Line color
    /// </summary>
    Color LineColor { get; }

    /// <summary>
    /// Whether to draw the line display text and direction indicator if the DateValue is off-screen of the gantt
    /// </summary>
    bool ShowOffscreenText => true;

    /// <summary>
    /// Provides the activity from the specific block as well as all selected operations.
    /// Calculate any data needed to provide the interface implementation
    /// </summary>
    void CalculateValue(InternalActivity a_activity, List<InternalOperation> a_selectedOperations, ScenarioDetail a_sd);
}

public class GanttLineInfo
{
    /// <summary>
    /// Displays as a label segment on the gantt adjacent to the line (on the right side)
    /// This will be localized
    /// </summary>
    public string DisplayText { get; set; }

    /// <summary>
    /// The date to draw the line. For Neptune and older,
    /// this value should be in UTC since it is converted to DisplayTime by the Gantt. 
    /// </summary>
    public DateTimeOffset DateValue { get; set; }

    /// <summary>
    /// Whether to draw this line
    /// </summary>
    public bool Draw { get; set; }

    /// <summary>
    /// Whether to draw this line extra thick
    /// </summary>
    public bool Bold { get; set; }

    /// <summary>
    /// Line color
    /// </summary>
    public Color LineColor { get; set; }

    /// <summary>
    /// Whether to draw the line display text and direction indicator if the DateValue is off-screen of the gantt
    /// </summary>
    public bool ShowOffscreenText => true;

    public GanttLineInfo()
    {
        Bold = false;
    }
}

/// <summary>
/// A vertical line drawn on the gantt with an optional display text
/// </summary>
public interface IMultiGanttLineElement : IPackageElement
{
    List<GanttLineInfo> LineInfos { get; }

    /// <summary>
    /// Provides the activity from the specific block as well as all selected operations.
    /// Calculate any data needed to provide the interface implementation
    /// </summary>
    void CalculateValues(InternalActivity a_activity, List<InternalOperation> a_selectedOperations, ScenarioDetail a_sd);
}

public class GanttRegionInfo
{
    /// <summary>
    /// The initial date to indicate the start of the region
    /// </summary>
    public DateTimeOffset StartDateValue { get; set; }

    /// <summary>
    /// The date to indicate the end of the region
    /// </summary>
    public DateTimeOffset EndDateValue { get; set; }

    /// <summary>
    /// Whether to draw this line
    /// </summary>
    public bool Draw { get; set; }

    /// <summary>
    /// Region color
    /// </summary>
    public Color Color { get; set; }

    public string DisplayText { get; set; }

    /// <summary>
    /// Whether to draw the line display text and direction indicator if the DateValue is off-screen of the gantt
    /// </summary>
    public bool ShowOffscreenText { get; set; }

    public GanttRegionInfo()
    {
        ShowOffscreenText = true;
    }
}

/// <summary>
/// Multiple custom regions to draw on the gantt
/// </summary>
public interface IMultiGanttRegionElement : IPackageElement
{
    /// <summary>
    /// The name displayed in the settings control for the gantt region.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// A collection of Gantt region information that will be displayed.
    /// </summary>
    List<GanttRegionInfo> RegionInfos { get; }

    /// <summary>
    /// Provides the activity from the specific block as well as all selected operations.
    /// Calculate any data needed to provide the interface implementation
    /// </summary>
    void CalculateValue(InternalActivity a_activity, List<InternalOperation> a_selectedOperations, ScenarioDetail a_sd);
}

/// <summary>
/// A region drawn on the gantt
/// </summary>
public interface IGanttRegionElement : IPackageElement
{
    /// <summary>
    /// The initial date to indicate the start of the region
    /// </summary>
    DateTimeOffset StartDateValue { get; }

    /// <summary>
    /// The date to indicate the end of the region
    /// </summary>
    DateTimeOffset EndDateValue { get; }

    /// <summary>
    /// Whether to draw this line
    /// </summary>
    bool Draw { get; }

    /// <summary>
    /// Region color
    /// </summary>
    Color Color { get; }

    string DisplayText { get; }
    /// <summary>
    /// Whether to draw the line display text and direction indicator if the DateValue is off-screen of the gantt
    /// </summary>
    bool ShowOffscreenText => true;

    /// <summary>
    /// Provides the activity from the specific block as well as all selected operations.
    /// Calculate any data needed to provide the interface implementation
    /// </summary>
    void CalculateValue(InternalActivity a_activity, List<InternalOperation> a_selectedOperations, ScenarioDetail a_sd);
}

/// <summary>
/// A vertical line drawn on the gantt with an optional display text
/// </summary>
public interface IGanttEligibilityIndicatorElement : IPackageElement
{
    int ImageHeight { get; }

    int ImageWidth { get; }

    /// <summary>
    /// Color of the indicator's outline
    /// </summary>
    Pen OutlineColor { get; }

    /// <summary>
    /// Indicator color
    /// </summary>
    Brush IndicatorColor { get; }

    EBlockEligibilityTypes EligibilityType { get; }
    /// <summary>
    /// Only one indicator in each category will be drawn
    /// </summary>
    EBlockEligibilityIndicatorTypes Category { get; }
    /// <summary>
    /// The Highest priority in each category will be drawn
    /// </summary>
    int DrawPriority { get; }
    /// <summary>
    /// Calculate value to determine what indicator to display on the Gantt
    /// </summary>
    void CalculateValue(ScenarioDetail a_sd, IGanttResource a_ganttResource, IGanttBlock a_ganttBlock, List<BaseResource> a_eligibleResources);

    /// <summary>
    /// Calculate value to determine what indicator to display on the Gantt from a drag drop operation
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_ganttResource"></param>
    /// <param name="a_op"></param>
    /// <param name="a_eligibleResources"></param>
    void CalculateValueForDragDrop(ScenarioDetail a_sd, IGanttResource a_ganttResource, InternalOperation a_op, List<BaseResource> a_eligibleResources);

    /// <summary>
    /// Draws the indicator icon
    /// </summary>
    /// <param name="a_g"></param>
    /// <param name="a_cellRect"></param>
    void Draw(Graphics a_g, Rectangle a_cellRect, int LeftInsert, Font a_font, Color a_textColor);
}

/// <summary>
/// A border line drawn one of the sides of a gantt block
/// </summary>
public interface IGanttBlockBorderElement : IPackageElement
{
    /// <summary>
    /// Border priority. Only a limited number of borders can be drawn at once.
    /// Higher numbers are hight priority
    /// </summary>
    uint Priority { get; }

    /// <summary>
    /// The side of the block to draw
    /// </summary>
    EGanttBorder Border { get; }

    /// <summary>
    /// The color and size and appearance of the border section
    /// </summary>
    Pen Pen { get; }

    /// <summary>
    /// Whether to draw the line based on whether it was selected and the current border index. The index is the layer (starting at 0) that is being drawn
    /// This will only be called once per draw per border element
    /// </summary>
    /// <param name="a_selected"></param>
    /// <param name="a_drawIndex"></param>
    /// <param name="a_sizeLimit"></param>
    /// <returns></returns>
    bool Draw(bool a_selected, int a_drawIndex, int a_sizeLimit);
}

public enum EGanttBorder { Top, Left, Right, Bottom }

/// <summary>
/// A horizontal region drawn within the borders of a gantt block
/// </summary>
public interface IGanttSegmentElement : IPackageElement
{
    /// <summary>
    /// Sort priority. Based on the block size, not all segments may be drawn.
    /// Higher numbers are hight priority
    /// </summary>
    uint Priority { get; }

    /// <summary>
    /// If greater than 0, this segment will draw with this minimum height (although it may be constrained by available block height remaining)
    /// </summary>
    uint MinHeight { get; }

    /// <summary>
    /// If greater than 0, this segment height will expand depending on this weight value compared to other segment's weights.
    /// </summary>
    uint ProportionalHeightWeight { get; }

    /// <summary>
    /// The text to draw within the segment. This will only draw if segment text option is enabled in the system.
    /// </summary>
    string Text { get; }

    /// <summary>
    /// Draw the region
    /// </summary>
    void Draw(Graphics a_g, int a_left, int a_top, int a_width, int a_height, Gantt a_gantt);
}

public interface IGanttHighlightElement : IPackageElement
{
    /// <summary>
    /// Sort priority. Based on the block size, not all highlights may be drawn.
    /// Higher numbers are hight priority
    /// </summary>
    uint Priority { get; }

    /// <summary>
    /// Whether to draw this highlighter
    /// </summary>
    bool Show(int a_drawIndex);

    /// <summary>
    /// Draw the region
    /// </summary>
    void Draw(Graphics a_g, Rectangle a_rectangle);
}

public interface IGanttBlockMovedExtension : IPackageElement
{
    /// <summary>
    /// Sort priority.
    /// Higher numbers are high priority
    /// </summary>
    int Priority { get; }
    /// <summary>
    /// When to continue with the standard move functionality or cancel
    /// </summary>
    /// <returns></returns>
    EEGantBlockMovedResult PreMoveCustomization();
}

public enum EEGantBlockMovedResult
{
    /// <summary>
    /// Continue with the standard move functionality
    /// </summary>
    Continue,
    /// <summary>
    /// Cancel the standard move functionality
    /// </summary>
    Cancel
}