using System.Drawing;

using PlexityHide.GTP;

using PT.SchedulerDefinitions;

namespace PT.PackageDefinitionsUI.GanttElementSettings;

public class GanttBlockRelation
{
    public GanttBlockRelation(BlockKey a_source, BlockKey a_destination)
    {
        LinkSource = a_source;
        LinkDestination = a_destination;
    }

    public Color LinkColor;
    public TimeItemLinkStyle LinkStyle;
    public TimeItemLinkDrawStyle DrawStyle;
    public int Width;

    public readonly BlockKey LinkSource;
    public readonly BlockKey LinkDestination;

    public string Description;
}