using System.Text;

using PT.APSCommon;

namespace PT.Scheduler;

/// <summary>
/// Defines a set of requirements that a resource must have.
/// </summary>
public partial class ResourceRequirement
{
    #region Step 1 of Eligibility. Determine all the resources within the eligible plants of the MO that can satisfy this requirement.
    private readonly PlantResourceEligibilitySet m_eligibleResources = new ();

    /// <summary>
    /// Step 1 of eligibility process.
    /// This contains EligibleResourceSets for each plant that has resources that are eligible to satisfy this requirement.
    /// Each element contains a plant id and all the resources within the plant that are eligible.
    /// This set of eligible resource is limited by:
    /// the EligiblePlant set of the MO
    /// inactive machines
    /// </summary>
    internal PlantResourceEligibilitySet EligibleResources => m_eligibleResources;

    /// <summary>
    /// Returns a list of Eligible Resources across all Plants.
    /// </summary>
    public List<Resource> GetEligibleResources()
    {
        return m_eligibleResources.GetResources();
    }

    /// <summary>
    /// Returns the number of eligible resources.
    /// </summary>
    public int EligibleResourcesCount => m_eligibleResources.ResourceCountInAllEligibleResourceSets;

    internal EligibleResourceSet GetEligibleResourcesForPlant(BaseId a_plantId)
    {
        return m_eligibleResources[a_plantId];
    }

    /// <summary>
    /// Step 1 of eligibility.
    /// Finds all the eligible resources within all of the manufacturing order's eligible plants.
    /// </summary>
    internal void CreateResourceEligibilitySet(ProductRuleManager a_productRuleManager)
    {
        m_eligibleResources.Clear();

        for (int plantIdx = 0; plantIdx < m_operation.ManufacturingOrder.EligiblePlants.Count; plantIdx++)
        {
            EligiblePlant eligiblePlant = (EligiblePlant)m_operation.ManufacturingOrder.EligiblePlants.GetRow(plantIdx);
            Plant plant = eligiblePlant.Plant;
            EligibleResourceSet eligibleResources = null;

            foreach (Department department in plant.Departments)
            {
                foreach (Resource machine in department.Resources)
                {
                    if (machine.Active && machine.ResourceCapacityIntervals.HasOnlineCapacity() && IsEligible(machine, a_productRuleManager))
                    {
                        if (eligibleResources == null)
                        {
                            eligibleResources = new EligibleResourceSet();
                        }

                        eligibleResources.Add(machine);
                    }
                }
            }

            if (eligibleResources != null)
            {
                eligibleResources.AddsComplete();
                m_eligibleResources.Add(plant.Id, eligibleResources);
            }
        }
    }

    /// <summary>
    /// Determines whether a machine is able to satisfy this resource requirement.
    /// </summary>
    /// <param name="a_productRuleManager"></param>
    /// <param name="a_sb"></param>
    /// <param name="a_res"></param>
    /// <returns>Whether the resource is capable of satisfying this resource requirement.</returns>
    public bool IsEligible(Resource a_res, ProductRuleManager a_productRuleManager, StringBuilder a_sb = null)
    {
        //Verify this resource has connectors to Storage Areas that cannot store the operation products
        if (Operation.Products.Count > 0)
        {
            bool validated = false;
            foreach (Warehouse warehouse in a_res.Plant.WarehouseEnumerator)
            {
                if (CanWarehouseStoreProducts(warehouse, a_res))
                {
                    validated = true;
                    break;
                }
            }

            if (!validated)
            {
                a_sb?.AppendLine(string.Format("Resource '{0}' has Storage Area Connector configured that points to a Storage Area that can't store its products.".Localize(), a_res.Name));
                return false;
            }
        }

        //Filter resources with connectors to Storage Areas that cannot store the required material
        if (Operation.MaterialRequirements.Count > 0)
        {
            bool validated = false;
            foreach (Warehouse warehouse in a_res.Plant.WarehouseEnumerator)
            {
                if (CanWarehouseStoreMaterials(warehouse, a_res))
                {
                    validated = true;
                    break;
                }
            }

            if (!validated)
            {
                a_sb?.AppendLine(string.Format("Resource '{0}' has Storage Area Connector configured that points to a Storage Area that can't store its Required Materials.".Localize(),  a_res.Name));
                return false;
            }
        }

        if (DefaultResource != null && DefaultResource == a_res)
        {
            return true;
        }

        int cmCount = m_capabilityManager.Count;

        if (cmCount == 0)
        {
            a_sb?.AppendLine(string.Format("Resource '{0}' has no mapped capabilities'.".Localize(), a_res.Name));
            return false;
        }

        foreach (Capability capabilityObj in m_capabilityManager)
        {
            if (!a_res.IsCapable(capabilityObj.Id))
            {
                a_sb?.AppendLine(string.Format("Resource '{0}' is missing mapping to Capability '{1}'.".Localize(), a_res.Name, capabilityObj.Name));
                return false;
            }
        }

        if (!a_res.IsCapableBasedOnMinMaxQtys(Operation, a_productRuleManager))
        {
            a_sb?.AppendLine(string.Format("Resource '{0}' is ineligible due to Min/Max quantity constraints.".Localize(), a_res.Name));
            return false;
        }

        if (!a_res.IsCapableBasedOnMinMaxQtyPerCycle(Operation, a_productRuleManager))
        {
            a_sb?.AppendLine(string.Format("Resource '{0}' is ineligible due to Min/Max quantity per cycle constraints.".Localize(), a_res.Name));
            return false;
        }

        if (!a_res.IsCapableBasedOnAttributeNumberRange(Operation))
        {
            a_sb?.AppendLine(string.Format("Resource '{0}' is ineligible due to Attribute Number Range constraints.".Localize(), a_res.Name));
            return false;
        }

        if (!a_res.IsCapableBasedOnMinMaxVolume(Operation, a_productRuleManager))
        {
            a_sb?.AppendLine(string.Format("Resource '{0}' is ineligible due to Min/Max volume constraints.".Localize(), a_res.Name));
            return false;
        }

        return true;
    }

    private bool CanStorageAreaStoreProducts(StorageArea a_sa)
    {
        foreach (Product product in Operation.Products)
        {
            if (!a_sa.CanStoreItem(product.Item.Id))
            {
                return false;
            }
        }

        return true;
    }

    private bool CanStorageAreaStoreMaterials(StorageArea a_sa)
    {
        foreach (MaterialRequirement mr in Operation.MaterialRequirements)
        {
            if (!a_sa.CanStoreItem(mr.Item.Id))
            {
                return false;
            }
        }

        return true;
    }

    private bool CanConnectorStoreProducts(StorageAreaConnector a_connector)
    {
        foreach (BaseId saId in a_connector.StorageAreaInList)
        {
            StorageArea storageArea = a_connector.Warehouse.StorageAreas.GetValue(saId);
            if (CanStorageAreaStoreProducts(storageArea))
            {
                return true;
            }
        }

        return false;
    }

    private bool CanConnectorStoreMaterials(StorageAreaConnector a_connector)
    {
        foreach (BaseId saId in a_connector.StorageAreaOutList)
        {
            StorageArea storageArea = a_connector.Warehouse.StorageAreas.GetValue(saId);
            if (CanStorageAreaStoreMaterials(storageArea))
            {
                return true;
            }
        }

        return false;
    }

    private bool CanWarehouseStoreProducts(Warehouse a_wh, InternalResource a_res)
    {
        if (!a_wh.StorageAreaConnectors.AnyConnectorInForResource(a_res))
        {
            //No constraint
            return true;
        }

        foreach (StorageAreaConnector connector in a_wh.StorageAreaConnectors.GetStorageConnectorsForResourceIn(a_res))
        {
            if (CanConnectorStoreProducts(connector))
            {
                return true;
            }
        }

        return false;
    }

    private bool CanWarehouseStoreMaterials(Warehouse a_wh, InternalResource a_res)
    {
        if (!a_wh.StorageAreaConnectors.AnyConnectorOutForResource(a_res))
        {
            //No constraint
            return true;
        }

        foreach (StorageAreaConnector connector in a_wh.StorageAreaConnectors.GetStorageConnectorsForResourceOut(a_res))
        {
            if (CanConnectorStoreMaterials(connector))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    /// <summary>
    /// Among all the plants and eligible resources, the first eligible resource is returned or null if there are no eligible resources.
    /// </summary>
    /// <returns></returns>
    public InternalResource GetFirstEligibleResource()
    {
        return EligibleResources.GetFirstEligibleResource();
    }

    /// <summary>
    /// Call this function to calculate the release of non-default resources.
    /// </summary>
    /// <param name="a_act"></param>
    /// <returns></returns>
    internal long DefaultResource_CalcJimLimitRelease(InternalActivity a_act)
    {
        long release = a_act.DbrJitStartTicks + DefaultResource_JITLimitTicks;
        return release;
    }

    internal bool PostSimStageCust(ScenarioDetail.SimulationType a_simType, Transmissions.ScenarioBaseT a_t, ScenarioDetail a_sd, int a_currentSimStageIdx, int a_lastSimStageIdx)
    {
        bool computeEligibility = false;
        a_sd.ExtensionController.PostSimStageChangeRR(a_simType, a_t, a_sd, m_operation, this, a_currentSimStageIdx, a_lastSimStageIdx, out ChangableRRValues changable);
        if (changable != null && changable.DefaultResourceSet)
        {
            DefaultResource = changable.DefaultResource;
            computeEligibility = true;
        }

        return computeEligibility;
    }

    internal bool EndOfSimulationCustExecute(ScenarioDetail.SimulationType a_simType, Transmissions.ScenarioBaseT a_t, ScenarioDetail a_sd)
    {
        bool computeEligibility = false;
        a_sd.ExtensionController.EndOfSimulationChangeRR(a_simType, a_t, a_sd, m_operation, this, out ChangableRRValues changable);
        if (changable != null && changable.DefaultResourceSet)
        {
            DefaultResource = changable.DefaultResource;
            computeEligibility = true;
        }

        return computeEligibility;
    }

    #region Similarity
    internal int SimilarityComparison(ResourceRequirement a_rr)
    {
        int v;

        if ((v = UsageStart.CompareTo(a_rr.UsageStart)) != 0)
        {
            return v;
        }

        if ((v = UsageEnd.CompareTo(a_rr.UsageEnd)) != 0)
        {
            return v;
        }

        if ((v = m_attentionPercent.CompareTo(a_rr.m_attentionPercent)) != 0)
        {
            return v;
        }

        if ((v = m_capabilityManager.SimilarityComparison(a_rr.m_capabilityManager)) != 0)
        {
            return v;
        }

        if (DefaultResource == null && a_rr.DefaultResource != null)
        {
            return -1;
        }

        if (DefaultResource != null && a_rr.DefaultResource == null)
        {
            return 1;
        }

        if (DefaultResource != null && a_rr.DefaultResource != null)
        {
            if ((v = DefaultResource.Id.Value.CompareTo(a_rr.DefaultResource.Id.Value)) != 0)
            {
                return v;
            }
        }

        if ((v = DefaultResource_UseJITLimitTicks.CompareTo(a_rr.DefaultResource_UseJITLimitTicks)) != 0)
        {
            return v;
        }

        if (DefaultResource_UseJITLimitTicks)
        {
            if ((v = DefaultResource_JITLimitTicks.CompareTo(a_rr.DefaultResource_JITLimitTicks)) != 0)
            {
                return v;
            }
        }

        return 0;
    }
    #endregion
}