using PT.APSCommon;
using PT.Common.Delegates;

namespace PT.PackageDefinitionsUI;

public interface IGanttDataSource
{
    /// <summary>
    /// The current ClockDate
    /// </summary>
    DateTimeOffset ClockDate { get; }

    /// <summary>
    /// Gets the Earliest Frozen Span End date from all the Department Frozen Spans
    /// </summary>
    DateTimeOffset EarliestFrozenSpanEnd { get; }

    /// <summary>
    /// The Planning Horizon End Date
    /// </summary>
    DateTimeOffset PlanningHorizonEnd { get; }

    /// <summary>
    /// The Short Term End Date
    /// </summary>
    DateTimeOffset ShortTermSpanEnd { get; }

    /// <summary>
    /// The Date Time after which Activities are hidden in the Gantt
    /// </summary>
    DateTimeOffset HiddenActivityStartLocalized { get; }

    /// <summary>
    /// List of Department Frozen Span End Dates
    /// </summary>
    Dictionary<BaseId, DateTimeOffset> DepartmentFrozenSpanEnds { get; }

    /// <summary>
    /// List of Department Stable Span End Dates
    /// </summary>
    Dictionary<BaseId, DateTimeOffset> DepartmentStableSpanEnds { get; }

    /// <summary>
    /// Adds a list of Gantt Line elements to be drawn on the Gantt
    /// </summary>
    /// <param name="a_lineElement"></param>
    void AddGanttLineToDraw(List<IGanttLineElement> a_lineElement);

    /// <summary>
    /// Adds a list of Gantt Line elements from a IMultto be drawn on the Gantt
    /// </summary>
    /// <param name="a_multiLineElements"></param>
    void AddMultiGanttLinesToDraw(List<IMultiGanttLineElement> a_multiLineElements);

    /// <summary>
    /// Adds a list of Gantt Line elements to remove from the Gantt
    /// </summary>
    /// <param name="a_lineElement"></param>
    void RemoveGanttLine(List<IGanttLineElement> a_lineElement);

    /// <summary>
    /// Adds a list of Gantt Line elements to remove from the Gantt
    /// </summary>
    /// <param name="a_lineElements"></param>
    void RemoveMultiGanttLine(List<IMultiGanttLineElement> a_lineElements);

    /// <summary>
    /// Gets Gantt Line elements that need to be drawn
    /// </summary>
    /// <returns></returns>
    IEnumerable<IGanttLineElement> GetLinesToDraw();

    /// <summary>
    /// Gets Multi Gantt Line elements that need to be drawn
    /// </summary>
    /// <returns></returns>
    IEnumerable<IMultiGanttLineElement> GetMultiLinesToDraw();

    /// <summary>
    /// Adds a list of Gantt region elements to be drawn on the Gantt
    /// </summary>
    /// <param name="a_regionElement"></param>
    void AddGanttRegionToDraw(List<IGanttRegionElement> a_regionElement);

    /// <summary>
    /// Adds a list of multi gantt region elements to be drawn.
    /// </summary>
    /// <param name="a_multiRegionElement"></param>
    void AddMultiGanttRegionToDraw(List<IMultiGanttRegionElement> a_multiRegionElement);

    /// <summary>
    /// Adds a list of Gantt region elements to remove from the Gantt
    /// </summary>
    /// <param name="a_regionElement"></param>
    void RemoveGanttRegion(List<IGanttRegionElement> a_regionElement);

    /// <summary>
    /// Removes a list of Multi Gantt region elements from the Gantt
    /// </summary>
    /// <param name="a_regionElement"></param>
    void RemoveMultiGanttRegion(List<IMultiGanttRegionElement> a_multiRegionElement);

    /// <summary>
    /// Gets Gantt Line elements that need to be drawn
    /// </summary>
    /// <returns></returns>
    IEnumerable<IGanttRegionElement> GetRegionsToDraw();

    /// <summary>
    /// Gets Multi Gantt Line elements that need to be drawn
    /// </summary>
    /// <returns></returns>
    IEnumerable<IMultiGanttRegionElement> GetMultiRegionsToDraw();

    /// <summary>
    /// Fired when the data has first been loaded. Use this event to attach other datasource events that require data from the datasource
    /// </summary>
    event VoidDelegate Initialized;
}