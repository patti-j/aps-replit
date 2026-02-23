using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.ERPTransmissions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler;

// [TANK_CODE] 
/// <summary>
/// A type of warehouse used as part of a Tank resource.
/// </summary>
public class ResourceWarehouse : Warehouse
{
    #region IPTSerializable Members
    internal ResourceWarehouse(IReader reader, BaseIdGenerator a_idGenerator) :
        base(reader, a_idGenerator) { }
    #endregion

    internal ResourceWarehouse(BaseId a_id, WarehouseT.Warehouse a_warehouse, BaseIdGenerator a_idGen, InternalResource a_resource, PTTransmission t, ScenarioDetail a_sd, ISystemLogger a_errorReporter)
        : base(a_id, a_warehouse, a_idGen)
    {
        m_tankResource = a_resource;
    }

    /// <summary>
    /// If this Warehouse is part of a Tank type of resource, this is a reference to this reference.
    /// </summary>
    private InternalResource m_tankResource; // [TANK] 

    public InternalResource TankResource
    {
        get => m_tankResource;
        internal set => m_tankResource = value;
    }

    public override string ToString()
    {
        return string.Format("TankResource: {0}; Warehouse {1}", TankResource, base.ToString());
    }

    public override string Name
    {
        get => string.Format("{0} Storage".Localize(), m_tankResource.Name);
    }

    /// <summary>
    /// Whenever something is scheduled to draw material from the tank, the time the drawing of material must be reported by calling this function.
    /// </summary>
    /// <param name="a_endOfUsage"></param>
    internal void ReportTankRelease(long a_endOfUsage)
    {
        if (a_endOfUsage > m_maxTankRelease)
        {
            MaxTankRelease = a_endOfUsage;
        }
    }

    private long m_maxTankRelease;

    /// <summary>
    /// The latest time a resource reported a release of the tank.
    /// </summary>
    internal long MaxTankRelease
    {
        get => m_maxTankRelease;
        private set => m_maxTankRelease = value;
    }

    /// <summary>
    /// Call this function each time the tank runs out of material.
    /// </summary>
    internal void TankEmptied()
    {
        MaxTankRelease = 0;
    }

    /// <summary>
    /// Overridden to reset tank variables.
    /// </summary>
    /// <param name="a_clock"></param>
    internal override void ResetSimulationStateVariables(long a_clock, ScenarioDetail a_sd)
    {
        base.ResetSimulationStateVariables(a_clock, a_sd);
        TankEmptied();
    }
}