using PT.APSCommon;
using PT.Common.Extensions;
using PT.Common.File;
using PT.ERPTransmissions;
using PT.Scheduler.ErrorReporting;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;
using PT.Transmissions.ResourceConnectors;

namespace PT.Scheduler;

public class ResourceConnectorManager : ScenarioBaseObjectManager<ResourceConnector>
{
    public ResourceConnectorManager(BaseIdGenerator a_idGenerator) : base(a_idGenerator) { }

    public ResourceConnectorManager(IReader a_reader, BaseIdGenerator a_idGenerator) 
        : base(a_idGenerator)
    {
        if (a_reader.VersionNumber >= 12313)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                ResourceConnector connector = new (a_reader);
                Add(connector);
            }
        }
    }

    public override Type ElementType => typeof(ResourceConnector);
    
    public override int UniqueId => 1091;

    internal void RestoreReferences(PlantManager a_plants, ScenarioDetail a_sd, ISystemLogger a_errorReporter)
    {
        foreach (ResourceConnector connector in this)
        {
            connector.RestoreReferences(a_plants);
        }

        m_errorReporter = a_errorReporter;
    }

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        foreach (ResourceConnector resourceConnector in this)
        {
            a_udfManager.RestoreReferences(resourceConnector, UserField.EUDFObjectType.ResourceConnectors);
        }
    }

    #region Transmissions
    private void ValidateCopy(BaseId a_resConnectorId)
    {
        ValidateExistence(a_resConnectorId);
    }

    public void Receive(ScenarioDetail a_sd, ResourceConnectorsBaseT a_t, IScenarioDataChanges a_dataChanges)
    {
        ApplicationExceptionList errors = new ApplicationExceptionList();
        ResourceConnector resConnector;
        if (a_t is ResourceConnectorsDefaultT)
        {
            try
            {
                resConnector = new ResourceConnector(IdGen.NextID());
                resConnector.ExternalId = NextExternalId("Resource Connector");
                Add(resConnector);

                a_dataChanges.ResourceConnectorChanges.AddedObject(resConnector.Id);
            }
            catch (Exception e)
            {
                errors.Add(e);
            }
        }
        else if (a_t is ResourceConnectorsCopyT resourceConnectorsCopyT)
        {
            foreach (BaseId resConnectorId in resourceConnectorsCopyT.ResourceConnectorIds)
            {
                try
                {
                    resConnector = AddCopy(resConnectorId, a_sd.PlantManager);
                    a_dataChanges.ResourceConnectorChanges.AddedObject(resConnector.Id);
                }
                catch (Exception e)
                {
                    errors.Add(e);
                }
            }
        }
        else if (a_t is ResourceConnectorsDeleteT resourceConnectorsDeleteT)
        {
            try
            {
                foreach (BaseId resourceConnectorId in resourceConnectorsDeleteT.ResourceConnectorIds)
                {
                    Remove(resourceConnectorId);
                    a_dataChanges.ResourceConnectorChanges.DeletedObject(resourceConnectorId);
                }
            }
            catch (Exception e)
            {
                errors.Add(e);
            }
           
        }
        else if (a_t is ResourceConnectorsDeleteAllT)
        {
            try
            {
                Clear(a_dataChanges);
            }
            catch (Exception e)
            {
                errors.Add(e);
            }
        }

        if (errors.Count > 0)
        {
            ScenarioExceptionInfo sei = new();
            sei.Create(a_sd);
            m_errorReporter.LogException(errors, a_t, sei, ELogClassification.PtInterface, false);
        }
    }
    #endregion

    #region ERP Transmission
    internal void Receive(UserFieldDefinitionManager a_udfManager, ResourceConnectorsT a_t, Scenario a_s, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        ApplicationExceptionList errList = new ();
        List<string> updatedResConnectors = new ();

        for (var i = 0; i < a_t.Count; i++)
        {
            ResourceConnectorsT.ResourceConnector resourceConnectorT = a_t[i];
            updatedResConnectors.Add(resourceConnectorT.ExternalId);
            if (ContainsExternalId(resourceConnectorT.ExternalId))
            {
                //Update
                ResourceConnector resConnectorToUpdate = GetByExternalId(resourceConnectorT.ExternalId);
                resConnectorToUpdate.Update(a_udfManager, resourceConnectorT, a_t);
                UpdateConnections(a_sd, resConnectorToUpdate, resourceConnectorT, a_t, errList, a_dataChanges);

                a_dataChanges.ResourceConnectorChanges.UpdatedObject(resConnectorToUpdate.Id);
            }
            else
            {
                //new
                ResourceConnector newResConnector = new ResourceConnector(resourceConnectorT, IdGen.NextID(), a_udfManager);
                UpdateConnections(a_sd, newResConnector, resourceConnectorT, a_t, errList, a_dataChanges);
                Add(newResConnector);
                a_dataChanges.ResourceConnectorChanges.AddedObject(newResConnector.Id);
            }
        }

        if (a_t.AutoDeleteMode)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                ResourceConnector resConnector = GetByIndex(i);
                if (!updatedResConnectors.Contains(resConnector.ExternalId))
                {
                    Remove(resConnector);
                    a_dataChanges.ResourceConnectorChanges.DeletedObject(resConnector.Id);
                }
            }
        }

        if (errList.Count > 0)
        {
            ScenarioExceptionInfo sei = new();
            sei.Create(a_sd);
            m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
        }
    }
    #endregion

    private static void UpdateConnections(ScenarioDetail a_sd, ResourceConnector a_resConnector, ResourceConnectorsT.ResourceConnector a_resourceConnectorT, ResourceConnectorsT a_t, ApplicationExceptionList a_errors, IScenarioDataChanges a_dataChanges)
    {
        List<BaseId> toResConnectionsToDelete = new ();
        List<BaseId> fromResConnectionsToDelete = new ();

        foreach (ResourceKeyExternal resExternalIdKey in a_resourceConnectorT.FromResources)
        {
            Resource resource = a_sd.PlantManager.GetResource(resExternalIdKey.PlantExternalId, resExternalIdKey.DepartmentExternalId, resExternalIdKey.ResourceExternalId);
            if (resource == null)
            {
                a_errors.Add(new PTValidationException("4482", new object[] { a_resourceConnectorT.ExternalId, resExternalIdKey.ResourceExternalId, resExternalIdKey.DepartmentExternalId, resExternalIdKey.PlantExternalId }));
                continue;
            }

            if (!a_resConnector.FromResources.ContainsKey(resource.Id))
            {
                a_resConnector.AddFromResConnection(resource);
                a_dataChanges.FlagEligibilityChanges(BaseId.NULL_ID);
            }
        }

        foreach (ResourceKeyExternal resExternalIdKey in a_resourceConnectorT.ToResources)
        {
            Resource resource = a_sd.PlantManager.GetResource(resExternalIdKey.PlantExternalId, resExternalIdKey.DepartmentExternalId, resExternalIdKey.ResourceExternalId);
            if (resource == null)
            {
                a_errors.Add(new PTValidationException("4483", new object[] { a_resourceConnectorT.ExternalId, resExternalIdKey.ResourceExternalId, resExternalIdKey.DepartmentExternalId, resExternalIdKey.PlantExternalId }));
                continue;
            }

            if (!a_resConnector.ToResources.ContainsKey(resource.Id))
            {
                a_resConnector.AddToResConnection(resource);
                a_dataChanges.FlagEligibilityChanges(BaseId.NULL_ID);
            }
        }

        if (a_t.AutoDeleteConnections)
        {
            foreach ((BaseId resId, Resource res) in a_resConnector.FromResources)
            {
                ResourceKeyExternal existingResKeyExternal = new (res.Plant.ExternalId, res.Department.ExternalId, res.ExternalId);
                if (!a_resourceConnectorT.FromResources.Contains(existingResKeyExternal))
                {
                    fromResConnectionsToDelete.Add(resId);
                }
            }

            foreach (BaseId resIdToDelete in fromResConnectionsToDelete)
            {
                a_resConnector.RemoveFromResConnection(resIdToDelete);
                a_dataChanges.FlagEligibilityChanges(BaseId.NULL_ID);
            }

            foreach ((BaseId resId, Resource res) in a_resConnector.ToResources)
            {
                ResourceKeyExternal existingResKeyExternal = new(res.Plant.ExternalId, res.Department.ExternalId, res.ExternalId);
                if (!a_resourceConnectorT.ToResources.Contains(existingResKeyExternal))
                {
                    toResConnectionsToDelete.Add(resId);
                }
            }

            foreach (BaseId resIdToDelete in toResConnectionsToDelete)
            {
                a_resConnector.RemoveToResConnection(resIdToDelete);
                a_dataChanges.FlagEligibilityChanges(BaseId.NULL_ID);
            }
        }
    }

    private ResourceConnector AddCopy(BaseId a_resConnectorId, PlantManager a_plants)
    {
        ValidateCopy(a_resConnectorId);
        ResourceConnector resourceConnector = GetById(a_resConnectorId);
        ResourceConnector clone = resourceConnector.CopyInMemory();
        clone.RestoreReferences(a_plants);
        return AddCopy(resourceConnector, clone, NextID());
    }

    internal void Clear(IScenarioDataChanges a_dataChanges)
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            Delete(GetByIndex(i), a_dataChanges);
        }
    }

    internal void Delete(ResourceConnector a_connector, IScenarioDataChanges a_dataChanges)
    {
        a_dataChanges.ResourceConnectorChanges.DeletedObject(a_connector.Id);
        //TODO: Any validation we need to add?
        Remove(a_connector);
    }

    //Sim Stuff
    internal void ResetSimulationStateVariables()
    {
        foreach (ResourceConnector connector in this)
        {
            connector.ResetSimulationStateVariables();
        }
    }

    public IEnumerable<ResourceConnector> GetConnectorsFromResource(BaseId a_primaryResourceId)
    {
        foreach (ResourceConnector connector in this)
        {
            if (connector.FromResources.ContainsKey(a_primaryResourceId))
            {
                yield return connector;
            }
        }
    }

    public IEnumerable<ResourceConnector> GetConnectorsToResource(BaseId a_primaryResourceId)
    {
        foreach (ResourceConnector connector in this)
        {
            if (connector.ToResources.ContainsKey(a_primaryResourceId))
            {
                yield return connector;
            }
        }
    }

    public List<ResourceConnector> GetToConnectorsForResource(BaseResource a_connectorInfoPredecessorResource)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a collection of downstream resources.
    /// This will include all possible downstream resources regardless of which connector is being used
    /// </summary>
    /// <param name="a_resourceId"></param>
    /// <returns></returns>
    public Dictionary<BaseId, Resource> GetDownstreamSuccessorConnectorsFromResource(BaseId a_resourceId)
    {
        Dictionary<BaseId, Resource> downstreamConnectors = new ();
        foreach (ResourceConnector connector in this)
        {
            if (connector.FromResources.ContainsKey(a_resourceId))
            {
                downstreamConnectors.MergeWithOverride(connector.ToResources);
            }
        }

        return downstreamConnectors;
    }


    public IEnumerable<ResourceConnector> GetConnectorsBetweenResources(InternalResource a_fromRes, InternalResource a_toRes)
    {
        foreach (ResourceConnector connector in this)
        {
            if (connector.FromResources.ContainsKey(a_fromRes.Id) && connector.ToResources.ContainsKey(a_toRes.Id))
            {
                yield return connector;
            }
        }
    }

    /// <summary>
    /// Remove the resource from any connectors that have it as a From or To connection
    /// This could leave the connector without any connections
    /// </summary>
    /// <param name="a_resource">The resource being deleted</param>
    public void DeletingResource(Resource a_resource)
    {
        foreach (ResourceConnector connector in GetConnectorsToResource(a_resource.Id))
        {
            connector.RemoveToResConnection(a_resource.Id);
        }

        foreach (ResourceConnector connector in GetConnectorsFromResource(a_resource.Id))
        {
            connector.RemoveFromResConnection(a_resource.Id);
        }
    }

    public void ClearFlags()
    {
        foreach (ResourceConnector connector in this)
        {
            connector.ClearFlags();
        }
    }
}
