using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Debugging;
using PT.Common.Extensions;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

public class ResourceConnector : BaseObject, IPTDeserializable
{
    //Keep public constructor for Clone/CopyInMemory
    public ResourceConnector(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12315)
        {
            m_bools = new BoolVector32(a_reader);

            m_restoreRefData = new RestoreRefData(a_reader);
            a_reader.Read(out m_transitTicks);
        }
        else if (a_reader.VersionNumber >= 12313)
        {
            m_bools = new BoolVector32(a_reader);
            
            m_restoreRefData = new RestoreRefData(a_reader);
        }

        if (a_reader.VersionNumber < 13011)
        {
            //bool index for this is re-using the index of a deprecated field that may have been set to true in older versions.
            //we want default to be false.
            AllowConcurrentUse = false;
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);

        m_restoreRefData = new RestoreRefData(m_fromResources.Values, m_toResources.Values);
        m_restoreRefData.Serialize(a_writer);
        a_writer.Write(m_transitTicks);
    }

    internal ResourceConnector(BaseId a_newId) : base(a_newId)
    {
    }

    public override int UniqueId => 1092;
    public override string DefaultNamePrefix => "Connector";

    private BoolVector32 m_bools;
    private const short c_allowConcurrentUseIdx = 0;

    private readonly Dictionary<BaseId, Resource> m_fromResources = new ();
    private readonly Dictionary<BaseId, Resource> m_toResources = new ();

    public Dictionary<BaseId, Resource> FromResources => m_fromResources;
    public Dictionary<BaseId, Resource> ToResources => m_toResources;

    private bool m_inUse;

    internal bool InUse => m_inUse;

    //TODO: Store the allocated ID so it can be shared for resource and materials
    /// <summary>
    /// Whether a demand is allocated to use this connector through a qty node.
    /// This will be set when material is being allocated, but not yet firmed.
    /// When the operation schedules and material is Consumed, then the connector will be marked InUse.
    /// </summary>
    internal BaseId AllocatedResource = BaseId.NULL_ID;

    /// <summary>
    /// Whether this connector is going to be used to connect a predecessor resource to a successor resource
    /// </summary>
    internal BaseId ConnectedResource = BaseId.NULL_ID;

    internal Inventory AllocatedInventory;

    internal bool AttemptToConnect(BaseId a_primaryResourceId)
    {
        if (ConnectedResource != BaseId.NULL_ID && ConnectedResource != a_primaryResourceId)
        {
            return false;
        }

        ConnectedResource = a_primaryResourceId;
        return true;
    }

    private long m_transitTicks;
    public TimeSpan TransitSpan => TimeSpan.FromTicks(m_transitTicks);

    internal long TransitTicks => m_transitTicks;

    public bool AllowConcurrentUse
    {
        get => m_bools[c_allowConcurrentUseIdx];
        set => m_bools[c_allowConcurrentUseIdx] = value;
    }

    /// <summary>
    /// Tracks when this connector will be released if it is InUse
    /// </summary>
    internal long ReleaseTicks;

    internal void ResetSimulationStateVariables()
    {
        m_inUse = false;
        AllocatedResource = BaseId.NULL_ID;
        ConnectedResource = BaseId.NULL_ID;
        AllocatedInventory = null;
        ReleaseTicks = 0;
    }

    /// <summary>
    /// Track the connector as InUse and store it's release date
    /// </summary>
    /// <param name="a_availableDate">The datetime the connector will be available again</param>
    internal void UseConnector(long a_availableDate)
    {
        m_inUse = true;
        AllocatedResource = BaseId.NULL_ID;
        ConnectedResource = BaseId.NULL_ID;
        ReleaseTicks = a_availableDate;
    }

    internal void ReleaseConnector(long a_simClock)
    {
        ResetSimulationStateVariables();
        ReleaseTicks = a_simClock;
    }

    public bool ConnectedFromResource(BaseId a_resourceId)
    {
        return m_fromResources.ContainsKey(a_resourceId);
    }

    public bool ConnectedToResource(BaseId a_resourceId)
    {
        return m_toResources.ContainsKey(a_resourceId);
    }

    internal void RestoreReferences(PlantManager a_plantManager)
    {
        foreach (BaseId fromId in m_restoreRefData.FromIds)
        {
            Resource resource = (Resource)a_plantManager.GetResource(fromId);
            if (resource != null)
            {
                m_fromResources.Add(resource.Id, resource);
            }
            else
            {
                DebugException.ThrowInDebug("ResourceConnector serialized fromId is not found");
            }
        }

        foreach (BaseId toId in m_restoreRefData.ToIds)
        {
            Resource resource = (Resource)a_plantManager.GetResource(toId);
            if (resource != null)
            {
                m_toResources.Add(resource.Id, resource);
            }
            else
            {
                DebugException.ThrowInDebug("ResourceConnector serialized fromId is not found");
            }
        }

        m_restoreRefData = null;
    }

    private RestoreRefData m_restoreRefData;

    /// <summary>
    /// Constructor for new Resource Connector object
    /// </summary>
    /// <param name="a_resourceConnectorT"></param>
    /// <param name="a_nextId"></param>
    /// <param name="a_udfManager"></param>
    public ResourceConnector(ResourceConnectorsT.ResourceConnector a_resourceConnectorT, BaseId a_nextId, UserFieldDefinitionManager a_udfManager) : base(a_nextId, a_resourceConnectorT, a_udfManager, UserField.EUDFObjectType.ResourceConnectors)
    {
        if (a_resourceConnectorT.TransitTicksIsSet)
        {
            m_transitTicks = a_resourceConnectorT.TransitTicks;
        }
    }

    internal void Update(UserFieldDefinitionManager a_udfManager, ResourceConnectorsT.ResourceConnector a_resConnectorT, PTTransmission a_t)
    {
        base.Update(a_resConnectorT, a_t, a_udfManager, UserField.EUDFObjectType.ResourceConnectors);

        if (a_resConnectorT.TransitTicksIsSet)
        {
            m_transitTicks = a_resConnectorT.TransitTicks;
        }

        if (a_resConnectorT.AllowConcurrentUseIsSet && AllowConcurrentUse != a_resConnectorT.AllowConcurrentUse)
        {
            AllowConcurrentUse = a_resConnectorT.AllowConcurrentUse;
        }
    }

    private class RestoreRefData : IPTSerializable
    {
        internal RestoreRefData(IReader a_reader)
        {
            a_reader.Read(out int fromCount);
            FromIds = new List<BaseId>(fromCount);

            for (int i = 0; i < fromCount; i++)
            {
                BaseId baseId = new BaseId(a_reader);
                FromIds.Add(baseId);
            }

            a_reader.Read(out int toCount);
            ToIds = new List<BaseId>(toCount);

            for (int i = 0; i < toCount; i++)
            {
                BaseId baseId = new BaseId(a_reader);
                ToIds.Add(baseId);
            }
        }

        internal RestoreRefData(IEnumerable<Resource> a_fromResources, IEnumerable<Resource> a_toResources)
        {
            FromIds = new List<BaseId>();
            ToIds = new List<BaseId>();

            foreach (Resource fromResource in a_fromResources)
            {
                FromIds.Add(fromResource.Id);
            }

            foreach (Resource toResource in a_toResources)
            {
                ToIds.Add(toResource.Id);
            }
        }

        public void Serialize(IWriter a_writer)
        {
            a_writer.Write(FromIds);
            a_writer.Write(ToIds);
        }

        internal List<BaseId> FromIds;
        internal List<BaseId> ToIds;

        public int UniqueId => 1093;
    }

    public void AddFromResConnection(Resource a_fromRes)
    {
        FromResources.Add(a_fromRes.Id, a_fromRes);
    }

    public void AddToResConnection(Resource a_toRes)
    {
        ToResources.Add(a_toRes.Id, a_toRes);
    }

    public void RemoveFromResConnection(BaseId a_resIdToDelete)
    {
        FromResources.Remove(a_resIdToDelete);
    }

    public void RemoveToResConnection(BaseId a_resIdToDelete)
    {
        ToResources.Remove(a_resIdToDelete);
    }

    /// <summary>
    /// The connector was unavailable to provide material. This may cause the activity to not have enough material.
    /// Make sure to reattempt when the connector is available again.
    /// </summary>
    internal bool FlagConnectorUnavailable { get; set; }

    public void ClearFlags()
    {
        FlagConnectorUnavailable = false;
    }
}


