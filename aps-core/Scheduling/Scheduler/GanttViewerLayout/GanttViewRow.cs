using System.ComponentModel;

using PT.APSCommon;

namespace PT.Scheduler;

/// <summary>
/// Base class from which several GanttViewerLayout objects are derived.
/// </summary>
public class GanttViewRow : GanttViewerObjectBase2, IPTSerializable
{
    public new const int UNIQUE_ID = 387;

    #region IPTSerializable Members
    public GanttViewRow(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int val;
            reader.Read(out val);
            ganttViewRowType = (GanttViewRowTypes)val;
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write((int)ganttViewRowType);
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    /// <summary>
    /// Indicates the type of object that the row represents and that the Id corresponds to.
    /// </summary>
    public enum GanttViewRowTypes
    {
        Resource = 0,
        Craftsman,
        ProductionLine,
        BatchProcessor,
        VesselType
    }

    public GanttViewRow(BaseId id, BaseId creatorId, bool isPublic, int sortIndex, string name, string description, BaseObject schedulerObject, GanttViewRowTypes ganttViewRowType)
        : base(id, creatorId, isPublic, sortIndex, name, description, schedulerObject)
    {
        this.ganttViewRowType = ganttViewRowType;
    }

    private GanttViewRowTypes ganttViewRowType;

    /// <summary>
    /// Indicates the type of object that the row represents and that the Id corresponds to.
    /// </summary>
    public GanttViewRowTypes GanttViewRowType
    {
        get => ganttViewRowType;
        set => ganttViewRowType = value;
    }

    public void Change(BaseObject b)
    {
        Name = b.Name;
    }
}