using System.Drawing;

using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Scheduler.Simulation.Customizations;

/// <summary>
/// Stores pending changes that mirror the CapacityInterval Class.
/// These changes can be used by a capacity interval manager to update a newly added customization.
/// </summary>
public class CapacityIntervalAddHelper
{
    private readonly CapacityIntervalDefs.capacityIntervalTypes m_intervalType;
    private readonly CapacityIntervalDefs.capacityIntervalAdditionScopes m_scope;
    private readonly BaseId m_plantId;
    private readonly BaseId m_departmentId;
    private readonly BaseId m_resourceId;
    private readonly DateTime m_intervalStart;
    private readonly DateTime m_intervalEnd;
    private readonly Color m_color;
    private readonly bool m_canDragAndResize;
    private readonly bool m_canDelete;

    public CapacityIntervalAddHelper(CapacityIntervalDefs.capacityIntervalTypes a_intervalType, CapacityIntervalDefs.capacityIntervalAdditionScopes a_scope, BaseId a_plantId, BaseId a_departmentId, BaseId a_resourceId, DateTime a_intervalStart, DateTime a_intervalEnd, Color a_color, bool a_canDragAndResize, bool a_canDelete)
    {
        m_intervalType = a_intervalType;
        m_scope = a_scope;
        m_plantId = a_plantId;
        m_departmentId = a_departmentId;
        m_resourceId = a_resourceId;
        m_intervalStart = a_intervalStart;
        m_intervalEnd = a_intervalEnd;
        m_color = a_color;
        m_canDragAndResize = a_canDragAndResize;
        m_canDelete = a_canDelete;
    }

    public string Name;
    public string Notes;
    public string Description;

    public BaseId PlantId => m_plantId;

    public BaseId DepartmentId => m_departmentId;

    public BaseId ResourceId => m_resourceId;

    public DateTime IntervalStart => m_intervalStart;

    public DateTime IntervalEnd => m_intervalEnd;

    public CapacityIntervalDefs.capacityIntervalTypes IntervalType => m_intervalType;

    public CapacityIntervalDefs.capacityIntervalAdditionScopes Scope => m_scope;
    public Color IntervalColor => m_color;
    public bool CanDragAndResize => m_canDragAndResize;
    public bool CanDelete => m_canDelete;
}