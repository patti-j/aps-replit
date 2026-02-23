using PT.APSCommon;
using PT.Common.Debugging;
using PT.Common.PTMath;

namespace PT.Scheduler;

public partial class StorageAreaConnector
{
    private List<StorageArea> m_storageAreasIn;
    private List<StorageArea> m_storageAreasOut;
    internal List<StorageArea> StorageAreasIn => m_storageAreasIn;
    internal List<StorageArea> StorageAreasOut => m_storageAreasOut;
    
    private FlowRangeConstraint m_flowRangeConstraint;
    
    internal FlowRangeConstraint FlowRangeConstraint => m_flowRangeConstraint;

    internal void ResetSimulationStateVariables()
    {
        //Cache the actual objects so we don't need to look them up by Id for every simulation reference.
        m_storageAreasIn = new List<StorageArea>();

        foreach (BaseId baseId in m_storageAreaInIdList)
        {
            StorageArea storageArea = Warehouse.StorageAreas.GetValue(baseId);
            m_storageAreasIn.Add(storageArea);
        }

        m_storageAreasOut = new List<StorageArea>();

        foreach (BaseId baseId in m_storageAreaOutIdList)
        {
            StorageArea storageArea = Warehouse.StorageAreas.GetValue(baseId);
            m_storageAreasOut.Add(storageArea);
        }

        m_flowRangeConstraint = new (1);

        m_allocatedForStorageIn = 0;
        m_allocatedForStorageOut = 0;
    }

    private int m_allocatedForStorageIn;
    private int m_allocatedForStorageOut;

    /// <summary>
    /// Whether this connector can be allocated for storage into SAs
    /// </summary>
    internal bool AvailableForStorageIn
    {
        get
        {
            if (StorageInFlowLimit == 0)
            {
                return true;
            }

            if (m_allocatedForStorageIn == StorageInFlowLimit)
            {
                return false;
            }

            if (CounterFlow && m_allocatedForStorageIn + m_allocatedForStorageOut == m_counterFlowLimit)
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Whether this connector can be allocated for taking material out of storage
    /// </summary>
    internal bool AvailableForStorageOut
    {
        get
        {
            if (StorageOutFlowLimit == 0)
            {
                return true;
            }

            if (m_allocatedForStorageOut == StorageOutFlowLimit)
            {
                return false;
            }

            if (CounterFlow && m_allocatedForStorageIn + m_allocatedForStorageOut == m_counterFlowLimit)
            {
                return false;
            }

            return true;
        }
    }
}

