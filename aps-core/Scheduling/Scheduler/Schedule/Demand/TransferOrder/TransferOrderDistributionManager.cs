using System.Collections.Generic;
using System.Linq;

using PT.APSCommon;
using PT.ERPTransmissions;

namespace PT.Scheduler.Demand;

public class TransferOrderDistributionManager : ScenarioBaseObjectManager<TransferOrderDistribution>, IPTSerializable
{
    #region IPTSerializable Members
    internal TransferOrderDistributionManager(IReader a_reader, BaseIdGenerator a_idGen, TransferOrder a_transferOrder)
        : base(a_idGen)
    {
        if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out int count);

            for (int i = 0; i < count; i++)
            {
                Add(new TransferOrderDistribution(a_reader, a_transferOrder, a_idGen));
            }
        }
    }
    
    public new const int UNIQUE_ID = 648;

    public override int UniqueId => UNIQUE_ID;

    internal void RestoreReferences(WarehouseManager warehouses, ItemManager items)
    {
        for (int i = 0; i < Count; ++i)
        {
            TransferOrderDistribution to = GetByIndex(i);
            to.RestoreReferences(warehouses, items);
        }
    }    
    
    internal void AfterRestoreAdjustmentReferences()
    {
        for (int i = 0; i < Count; ++i)
        {
            TransferOrderDistribution to = GetByIndex(i);
            to.AfterRestoreAdjustmentReferences();
        }
    }
    #endregion IPTSerializable

    #region Construction
    public TransferOrderDistributionManager(BaseIdGenerator aIdGen)
        : base(aIdGen) { }
    #endregion

    #region Transmissions
    internal void Update(TransferOrderT.TransferOrder tTO, ItemManager items, WarehouseManager warehouses, TransferOrder a_transferOrder, ScenarioDetail a_sd, Transmissions.PTTransmissionBase a_t)
    {
        if (tTO.Distributions.Count != Count)
        {
            //Set the Net Change MRP flags before clearing in case the Inventories have changed.
            SetNetChangeMRPFlags();
        }

        if (tTO.Distributions.Count == 0)
        {
            a_transferOrder.DeletingOrClearingDistributions(a_sd, a_t);
            Clear();
        }
        else
        {
            HashSet<BaseId> affectedTods = new ();
            for (int i = 0; i < tTO.Distributions.Count; i++)
            {
                TransferOrderT.TransferOrder.TransferOrderDistribution todT = tTO.Distributions[i];
                TransferOrderDistribution tod = GetByExternalId(todT.ExternalId);
                if (tod == null)
                {
                    //Add a new
                    TransferOrderDistribution newTod = Add(new TransferOrderDistribution(NextID(), tTO.ExternalId, todT, items, warehouses, a_transferOrder, IdGen));
                    affectedTods.Add(newTod.Id);
                }
                else
                {
                    tod.Update(tTO.ExternalId, todT, items, warehouses);
                    affectedTods.Add(tod.Id);
                }
            }

            //TODO: Add an AutoDelete setting for TODs and TOs
            BaseIdList todsToDelete = new ();
            foreach (TransferOrderDistribution tod in this)
            {
                if (!affectedTods.Contains(tod.Id))
                {
                    //AutoDelete
                    todsToDelete.Add(tod.Id);
                }
            }

            if (todsToDelete.Count > 0)
            {
                a_transferOrder.DeleteDistributions(a_sd, todsToDelete, a_t);
            }
        }
    }

    internal void RemoveById(BaseId a_distributionId)
    {
        Remove(a_distributionId);
    }

    internal void SetNetChangeMRPFlags()
    {
        //Set the Net Change MRP flags before clearing in case the Inventories have changed.
        for (int distI = 0; distI < Count; distI++)
        {
            TransferOrderDistribution dist = GetByIndex(distI);
            dist.SetInventoryNetChangeMrpFlag();
        }
    }
    #endregion

    #region Miscellaneous
    public override Type ElementType => typeof(TransferOrderDistribution);
    #endregion

    #region Simulation
    internal void ResetSimulationStateVariables()
    {
        foreach (TransferOrderDistribution distribution in this)
        {
            distribution.ResetSimulationStateVariables();
        }
    }
    #endregion
}