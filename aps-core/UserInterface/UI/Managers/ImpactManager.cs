using PT.APSCommon;
using PT.Common.Debugging;
using PT.PackageDefinitionsUI;
using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.Scheduler;
using PT.Scheduler.Demand;
using PT.Transmissions;

namespace PT.UI.Managers;

/// <summary>
/// Tracks property values over time and indicates when values change
/// </summary>
internal class ImpactManager : IImpactAnalyzer
{
    private readonly List<IJobImpactProperty> m_jobImpactProperties = new ();
    private readonly List<IResourceImpactProperty> m_resourcePropertyImpactProperties = new ();
    private readonly List<ISoImpactProperty> m_soImpactProperties = new ();
    private readonly List<ISoLineImpactProperty> m_soLineImpactProperties = new ();
    private readonly List<ISoLineDistImpactProperty> m_soDistImpactProperties = new ();
    private readonly List<IPoImpactProperty> m_poImpactProperties = new ();
    private readonly List<IActivityImpactProperty> m_activityImpactProperties = new ();
    private readonly List<IOperationImpactProperty> m_operationImpactProperties = new ();
    private readonly List<IMoImpactProperty> m_moImpactProperties = new ();
    private readonly List<IMaterialImpactProperty> m_materialsImpactProperties = new ();
    private readonly List<IForecastImpactProperty> m_forecastImpactProperties = new ();
    private readonly List<IItemImpactProperty> m_itemImpactProperties = new ();
    private readonly List<IWarehouseImpactProperty> m_warehouseImpactProperties = new ();
    private readonly List<IInventoryImpactProperty> m_inventoryImpactProperties = new ();
    private readonly List<IImpactProperty> m_allPropertiesList = new ();

    private readonly Dictionary<BaseId, ScenarioImpactCache> m_impactCaches = new ();

    public ImpactManager(IPackageManagerUI a_packageManager)
    {
        #region Load Object Properties
        foreach (IObjectProperty objectProperty in a_packageManager.GetObjectProperties())
        {
            if (objectProperty is IJobImpactProperty impactProperty)
            {
                m_jobImpactProperties.Add(impactProperty);
            }

            if (objectProperty is IResourceImpactProperty resProperty)
            {
                m_resourcePropertyImpactProperties.Add(resProperty);
            }

            if (objectProperty is ISoImpactProperty soProperty)
            {
                m_soImpactProperties.Add(soProperty);
            }

            if (objectProperty is ISoLineImpactProperty soLinProperty)
            {
                m_soLineImpactProperties.Add(soLinProperty);
            }

            if (objectProperty is ISoLineDistImpactProperty distProperty)
            {
                m_soDistImpactProperties.Add(distProperty);
            }

            if (objectProperty is IPoImpactProperty poProperty)
            {
                m_poImpactProperties.Add(poProperty);
            }

            if (objectProperty is IActivityImpactProperty actProperty)
            {
                m_activityImpactProperties.Add(actProperty);
            }

            if (objectProperty is IOperationImpactProperty opProperty)
            {
                m_operationImpactProperties.Add(opProperty);
            }

            if (objectProperty is IMoImpactProperty moProperty)
            {
                m_moImpactProperties.Add(moProperty);
            }

            if (objectProperty is IMaterialImpactProperty materialProperty)
            {
                m_materialsImpactProperties.Add(materialProperty);
            }

            if (objectProperty is IForecastImpactProperty forecastProperty)
            {
                m_forecastImpactProperties.Add(forecastProperty);
            }

            if (objectProperty is IItemImpactProperty itemProperty)
            {
                m_itemImpactProperties.Add(itemProperty);
            }

            if (objectProperty is IWarehouseImpactProperty warehouseProperty)
            {
                m_warehouseImpactProperties.Add(warehouseProperty);
            }

            if (objectProperty is IInventoryImpactProperty invProperty)
            {
                m_inventoryImpactProperties.Add(invProperty);
            }
        }

        //We need to register all properties when creating a new impact cache, store a single list
        m_allPropertiesList.AddRange(m_jobImpactProperties);
        m_allPropertiesList.AddRange(m_resourcePropertyImpactProperties);
        m_allPropertiesList.AddRange(m_soImpactProperties);
        m_allPropertiesList.AddRange(m_soLineImpactProperties);
        m_allPropertiesList.AddRange(m_soDistImpactProperties);
        m_allPropertiesList.AddRange(m_poImpactProperties);
        m_allPropertiesList.AddRange(m_activityImpactProperties);
        m_allPropertiesList.AddRange(m_operationImpactProperties);
        m_allPropertiesList.AddRange(m_moImpactProperties);
        m_allPropertiesList.AddRange(m_materialsImpactProperties);
        m_allPropertiesList.AddRange(m_forecastImpactProperties);
        m_allPropertiesList.AddRange(m_itemImpactProperties);
        m_allPropertiesList.AddRange(m_warehouseImpactProperties);
        m_allPropertiesList.AddRange(m_inventoryImpactProperties);
        #endregion

        using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm)) //Wait until a lock can be made.  Must open here.
        {
            //sm.ScenarioReplacedEvent += new ScenarioManager.ScenarioReplacedDelegate(ReceiveScenarioReplaceTransmission);
            sm.ScenarioBeforeDeleteEvent += DeleteScenario;
            sm.ScenarioBeforeReloadEvent += RemoveFromImpactCacheBeforeReload;
            sm.ScenarioNewEvent += NewScenario;

            foreach (Scenario scenario in sm.Scenarios)
            {
                m_impactCaches.Add(scenario.Id, new ScenarioImpactCache(scenario.Id, m_allPropertiesList));
            }

            // Check if we actually want to track closed scenarios here, or if we just want to leave them out
            //if (!PTSystem.Server)
            //{
            //    foreach (BaseId scenarioId in sm.ClosedScenarioIds)
            //    {
            //        m_impactCaches.Add(scenarioId, new ScenarioImpactCache(scenarioId, m_allPropertiesList));
            //    }
            //}
        }
    }

    private void NewScenario(BaseId a_scenarioId, ScenarioBaseT a_t)
    {
        //Create a new scenario cache
        m_impactCaches.Add(a_scenarioId, new ScenarioImpactCache(a_scenarioId, m_allPropertiesList));
    }

    private void DeleteScenario(Scenario a_s, ScenarioEvents a_se, ScenarioUndoEvents a_sue, ScenarioBaseT a_t, bool a_isUnload = false)
    {
        //Remove the scenario cache
        m_impactCaches.Remove(a_s.Id);
    }
    
    private void RemoveFromImpactCacheBeforeReload(Scenario a_s, ScenarioEvents a_se, ScenarioUndoEvents a_sue)
    {
        //Remove the scenario cache
        m_impactCaches.Remove(a_s.Id);
    }

    /// <summary>
    /// Go through all objects and recalculate and store the new values
    /// </summary>
    /// <param name="a_scenarioDetail"></param>
    public void SimulationComplete(ScenarioDetail a_scenarioDetail)
    {
        if (a_scenarioDetail == null)
        {
            return;
        }

        if (m_impactCaches.TryGetValue(a_scenarioDetail.Scenario.Id, out ScenarioImpactCache cache))
        {
            //Go through all of the properties and calculate the new value
            Parallel.ForEach(m_jobImpactProperties,
                jobProperty =>
                {
                    foreach (Job job in a_scenarioDetail.JobManager.JobEnumerator)
                    {
                        IComparable value = jobProperty.CalculateValue(job, a_scenarioDetail);
                        EImpactComparisonResult result = cache.RegisterValue(job.Id, jobProperty.PackageObjectId, value);
                    }

                    jobProperty.ValuesChanged(cache.GetValueCache(jobProperty.PackageObjectId));
                }
            );

            Parallel.ForEach(m_resourcePropertyImpactProperties,
                resProperty =>
                {
                    foreach (Resource res in a_scenarioDetail.PlantManager.GetResourceList())
                    {
                        IComparable value = resProperty.CalculateValue(res, a_scenarioDetail);
                        EImpactComparisonResult result = cache.RegisterValue(res.Id, resProperty.PackageObjectId, value);
                    }

                    resProperty.ValuesChanged(cache.GetValueCache(resProperty.PackageObjectId));
                }
            );

            Parallel.ForEach(m_soImpactProperties,
                soProperty =>
                {
                    foreach (SalesOrder so in a_scenarioDetail.SalesOrderManager)
                    {
                        IComparable value = soProperty.CalculateValue(so, a_scenarioDetail);
                        EImpactComparisonResult result = cache.RegisterValue(so.Id, soProperty.PackageObjectId, value);
                    }

                    soProperty.ValuesChanged(cache.GetValueCache(soProperty.PackageObjectId));
                }
            );

            Parallel.ForEach(m_soLineImpactProperties,
                soLineProperty =>
                {
                    foreach (SalesOrder so in a_scenarioDetail.SalesOrderManager)
                    {
                        foreach (SalesOrderLine soSalesOrderLine in so.SalesOrderLines)
                        {
                            IComparable value = soLineProperty.CalculateValue(soSalesOrderLine, a_scenarioDetail);
                            EImpactComparisonResult result = cache.RegisterValue(soSalesOrderLine.Id, soLineProperty.PackageObjectId, value);
                        }
                    }

                    soLineProperty.ValuesChanged(cache.GetValueCache(soLineProperty.PackageObjectId));
                }
            );

            Parallel.ForEach(m_soDistImpactProperties,
                soLineDistProperty =>
                {
                    foreach (SalesOrder so in a_scenarioDetail.SalesOrderManager)
                    {
                        foreach (SalesOrderLine soSalesOrderLine in so.SalesOrderLines)
                        {
                            foreach (SalesOrderLineDistribution distribution in soSalesOrderLine.LineDistributions)
                            {
                                IComparable value = soLineDistProperty.CalculateValue(distribution, a_scenarioDetail);
                                EImpactComparisonResult result = cache.RegisterValue(distribution.Id, soLineDistProperty.PackageObjectId, value);
                            }
                        }
                    }

                    soLineDistProperty.ValuesChanged(cache.GetValueCache(soLineDistProperty.PackageObjectId));
                }
            );

            Parallel.ForEach(m_poImpactProperties,
                poProperty =>
                {
                    foreach (PurchaseToStock po in a_scenarioDetail.PurchaseToStockManager)
                    {
                        IComparable value = poProperty.CalculateValue(po, a_scenarioDetail);
                        EImpactComparisonResult result = cache.RegisterValue(po.Id, poProperty.PackageObjectId, value);
                    }

                    poProperty.ValuesChanged(cache.GetValueCache(poProperty.PackageObjectId));
                }
            );

            Parallel.ForEach(m_activityImpactProperties,
                actProperty =>
                {
                    foreach (Resource resource in a_scenarioDetail.PlantManager.GetResourceList())
                    {
                        ResourceBlockList.Node node = resource.Blocks.First;
                        while (node != null)
                        {
                            InternalActivity activity = node.Data.Batch.FirstActivity;
                            IComparable value = actProperty.CalculateValue(activity, resource, node.Data, a_scenarioDetail);
                            EImpactComparisonResult result = cache.RegisterValue(activity.Id, actProperty.PackageObjectId, value);
                            node = node.Next;
                        }
                    }

                    actProperty.ValuesChanged(cache.GetValueCache(actProperty.PackageObjectId));
                }
            );

            Parallel.ForEach(m_operationImpactProperties,
                opProperty =>
                {
                    foreach (Job job in a_scenarioDetail.JobManager.JobEnumerator)
                    {
                        foreach (InternalOperation operation in job.GetOperations())
                        {
                            IComparable value = opProperty.CalculateValue(operation, a_scenarioDetail);
                            EImpactComparisonResult result = cache.RegisterValue(operation.Id, opProperty.PackageObjectId, value);
                        }
                    }

                    opProperty.ValuesChanged(cache.GetValueCache(opProperty.PackageObjectId));
                }
            );

            Parallel.ForEach(m_moImpactProperties,
                moProperty =>
                {
                    foreach (Job job in a_scenarioDetail.JobManager.JobEnumerator)
                    {
                        foreach (ManufacturingOrder mo in job.ManufacturingOrders)
                        {
                            IComparable value = moProperty.CalculateValue(mo, a_scenarioDetail);
                            EImpactComparisonResult result = cache.RegisterValue(mo.Id, moProperty.PackageObjectId, value);
                        }
                    }

                    moProperty.ValuesChanged(cache.GetValueCache(moProperty.PackageObjectId));
                }
            );

            Parallel.ForEach(m_materialsImpactProperties,
                materialProperty =>
                {
                    foreach (Job job in a_scenarioDetail.JobManager.JobEnumerator)
                    {
                        foreach (InternalOperation op in job.GetOperations())
                        {
                            foreach (MaterialRequirement mr in op.MaterialRequirements)
                            {
                                IComparable value = materialProperty.CalculateValue(mr, op, op.Activities.GetByIndex(0), a_scenarioDetail);
                                EImpactComparisonResult result = cache.RegisterValue(mr.Id, materialProperty.PackageObjectId, value);
                            }
                        }
                    }

                    materialProperty.ValuesChanged(cache.GetValueCache(materialProperty.PackageObjectId));
                }
            );

            Parallel.ForEach(m_forecastImpactProperties,
                forecastProperty =>
                {
                    foreach (Warehouse wh in a_scenarioDetail.WarehouseManager)
                    {
                        foreach (Inventory inventory in wh.Inventories)
                        {
                            foreach (Forecast forecast in inventory.ForecastVersionActive.Forecasts)
                            {
                                foreach (ForecastShipment shipment in forecast.Shipments)
                                {
                                    IComparable value = forecastProperty.CalculateValue(inventory, forecast, shipment);
                                    EImpactComparisonResult result = cache.RegisterValue(shipment.Id, forecastProperty.PackageObjectId, value);
                                }
                            }
                        }
                    }

                    forecastProperty.ValuesChanged(cache.GetValueCache(forecastProperty.PackageObjectId));
                }
            );

            Parallel.ForEach(m_itemImpactProperties,
                itemProperty =>
                {
                    foreach (Item item in a_scenarioDetail.ItemManager)
                    {
                        IComparable value = itemProperty.CalculateValue(item, a_scenarioDetail);
                        EImpactComparisonResult result = cache.RegisterValue(item.Id, itemProperty.PackageObjectId, value);
                    }

                    itemProperty.ValuesChanged(cache.GetValueCache(itemProperty.PackageObjectId));
                }
            );

            Parallel.ForEach(m_warehouseImpactProperties,
                whProperty =>
                {
                    foreach (Warehouse wh in a_scenarioDetail.WarehouseManager)
                    {
                        IComparable value = whProperty.CalculateValue(wh, a_scenarioDetail);
                        EImpactComparisonResult result = cache.RegisterValue(wh.Id, whProperty.PackageObjectId, value);
                    }

                    whProperty.ValuesChanged(cache.GetValueCache(whProperty.PackageObjectId));
                }
            );

            Parallel.ForEach(m_inventoryImpactProperties,
                invProperty =>
                {
                    foreach (Warehouse wh in a_scenarioDetail.WarehouseManager)
                    {
                        foreach (Inventory inventory in wh.Inventories)
                        {
                            IComparable value = invProperty.CalculateValue(inventory, true, a_scenarioDetail);
                            EImpactComparisonResult result = cache.RegisterValue(inventory.Id, invProperty.PackageObjectId, value);
                        }
                    }

                    invProperty.ValuesChanged(cache.GetValueCache(invProperty.PackageObjectId));
                }
            );
        }
        else
        {
            DebugException.ThrowInTest("Impact cache not established for scenario");
        }
    }

    public void ResetData(BaseId a_scenarioId)
    {
        //Reset cache
        foreach (ScenarioImpactCache impactCache in m_impactCaches.Values)
        {
            impactCache.Clear();
        }
    }

    /// <summary>
    /// The scenario has been loaded
    /// </summary>
    /// <param name="a_sd"></param>
    public void InitializeScenario(ScenarioDetail a_sd)
    {
        SimulationComplete(a_sd);
    }

    private class ScenarioImpactCache
    {
        private readonly Dictionary<string, ValueCache> m_values;

        /// <summary>
        /// Initialize the value caches for each property
        /// </summary>
        /// <param name="a_scenarioId"></param>
        /// <param name="a_allPropertiesList"></param>
        internal ScenarioImpactCache(BaseId a_scenarioId, List<IImpactProperty> a_allPropertiesList)
        {
            m_values = new Dictionary<string, ValueCache>();
            foreach (IImpactProperty property in a_allPropertiesList)
            {
                m_values.Add(property.PackageObjectId, new ValueCache());
            }
        }

        public EImpactComparisonResult RegisterValue(BaseId a_baseId, string a_propertyId, IComparable a_value)
        {
            if (m_values.TryGetValue(a_propertyId, out ValueCache value))
            {
                return value.UpdateValueAndCalculateChange(a_baseId, a_value);
            }

            DebugException.ThrowInDebug("IImpactProperty comparison in process but property is not registered");

            return EImpactComparisonResult.NoChange;
        }

        private class ValueCache : IValueCache
        {
            private readonly Dictionary<BaseId, StoredValue> m_objectValues;

            internal ValueCache()
            {
                m_objectValues = new Dictionary<BaseId, StoredValue>();
            }

            internal EImpactComparisonResult UpdateValueAndCalculateChange(BaseId a_baseId, IComparable a_newValue)
            {
                if (m_objectValues.TryGetValue(a_baseId, out StoredValue storedValue))
                {
                    storedValue.UpdateValue(a_newValue);
                    return storedValue.CalculateChange();
                }

                StoredValue newValue = new ();
                m_objectValues.Add(a_baseId, newValue);
                newValue.UpdateValue(a_newValue);
                return newValue.CalculateChange();
            }

            public EImpactComparisonResult CalculateChange(BaseId a_baseId)
            {
                if (m_objectValues.TryGetValue(a_baseId, out StoredValue cachedValue))
                {
                    return cachedValue.CalculateChange();
                }

                //The object has not been cached. It's possible that the object was added and the new values have not yet been registered
                return EImpactComparisonResult.NotFound;
            }

            public IComparable GetCurrentValue(BaseId a_baseId)
            {
                if (m_objectValues.TryGetValue(a_baseId, out StoredValue storedValue))
                {
                    return storedValue.m_currentValue;
                }

                return null;
            }

            public IComparable GetPreviousValue(BaseId a_baseId)
            {
                if (m_objectValues.TryGetValue(a_baseId, out StoredValue storedValue))
                {
                    return storedValue.m_lastValue;
                }

                return null;
            }

            public void Clear()
            {
                m_objectValues.Clear();
            }
        }

        public void Clear()
        {
            foreach (ValueCache valuesValue in m_values.Values)
            {
                valuesValue.Clear();
            }
        }

        private class StoredValue
        {
            internal IComparable m_lastValue;
            internal IComparable m_currentValue;

            internal void UpdateValue(IComparable a_newValue)
            {
                m_lastValue = m_currentValue;
                m_currentValue = a_newValue;
            }

            internal EImpactComparisonResult CalculateChange()
            {
                if (m_lastValue == null)
                {
                    return EImpactComparisonResult.Initialized;
                }

                int compareTo = m_currentValue.CompareTo(m_lastValue);
                if (compareTo == 0)
                {
                    return EImpactComparisonResult.NoChange;
                }

                if (compareTo == -1)
                {
                    return EImpactComparisonResult.Decrease;
                }

                return EImpactComparisonResult.Increase;
            }
        }

        internal IValueCache GetValueCache(string a_jobPropertyPackageObjectId)
        {
            return m_values[a_jobPropertyPackageObjectId];
        }
    }
}