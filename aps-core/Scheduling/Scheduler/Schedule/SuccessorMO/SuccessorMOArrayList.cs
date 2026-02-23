using System.Collections;

using PT.ERPTransmissions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

public class SuccessorMOArrayList : IPTSerializable
{
    private readonly ArrayList successorMOs = new ();

    /// <summary>
    /// The number of SuccessorMOs within this array list.
    /// </summary>
    public int Count => successorMOs.Count;

    /// <summary>
    /// O(1).
    /// </summary>
    public SuccessorMO this[int index] => (SuccessorMO)successorMOs[index];

    #region IPTSerializable Members
    public SuccessorMOArrayList() { }

    public SuccessorMOArrayList(IReader reader, ManufacturingOrder predecessorMO)
    {
        int successorMOCnt;
        reader.Read(out successorMOCnt);
        for (int i = 0; i < successorMOCnt; ++i)
        {
            SuccessorMO smo = new (reader, predecessorMO);
            successorMOs.Add(smo);
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(Count);
        for (int i = 0; i < Count; ++i)
        {
            this[i].Serialize(writer);
        }
    }

    public int UniqueId => 0;
    #endregion

    #region Construction
    /// <summary>
    /// Create successor MO listing from Job transmission specification.
    /// </summary>
    /// <param name="jsmos">Successor MOs</param>
    public SuccessorMOArrayList(JobT.SuccessorMOArrayList jsmos, ManufacturingOrder predecessorMO)
    {
        for (int i = 0; i < jsmos.Count; ++i)
        {
            JobT.SuccessorMO jsmo = jsmos[i];
            if (jsmo.SuccessorJobExternalId == predecessorMO.Job.ExternalId && jsmo.SuccessorManufacturingOrderExternalId == predecessorMO.ExternalId)
            {
                //The MO is pointing to itself as a successor.
                throw new APSCommon.PTValidationException("2834", new object[] { predecessorMO.ExternalId, predecessorMO.Job.ExternalId });
            }

            SuccessorMO smo = new (jsmo, predecessorMO);
            successorMOs.Add(smo);
        }
    }

    public SuccessorMOArrayList(SuccessorMOArrayList sourceList, ManufacturingOrder predecessorMO)
    {
        for (int i = 0; i < sourceList.Count; i++)
        {
            SuccessorMO sourceSMo = sourceList[i];
            if (sourceSMo.SuccessorJobExternalId == predecessorMO.Job.ExternalId && sourceSMo.SuccessorManufacturingOrderExternalId == predecessorMO.ExternalId)
            {
                //The MO is pointing to itself as a successor.
                throw new APSCommon.PTValidationException("2834", new object[] { predecessorMO.ExternalId, predecessorMO.Job.ExternalId });
            }

            SuccessorMO smo = new (sourceSMo, predecessorMO);
            successorMOs.Add(smo);
        }
    }
    #endregion

    /// <summary>
    /// Clear out the successors MOs and recreate this list with the provided successors.
    /// </summary>
    /// <param name="a_predecessorMO"></param>
    /// <param name="a_newSuccessors"></param>
    /// <param name="a_sd"></param>
    /// <param name="a_dataChanges"></param>
    /// <returns>Whether the update included the addition of any new successor MOs, or caused the update of any successor MOs.</returns>
    internal void Update(ManufacturingOrder a_predecessorMO, SuccessorMOArrayList a_newSuccessors, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        bool newSuccessorMOs = AnyNewSuccessorMOs(a_newSuccessors);

        AnyUpdatedSuccessorMOs(a_newSuccessors.successorMOs, out bool alteredSuccessorMOs, out bool alteredSuccessorKeys);

        if (newSuccessorMOs || alteredSuccessorMOs)
        {
            a_sd.SignalCriticalSuccessorMOUpdate();
            a_dataChanges.FlagEligibilityChanges(a_predecessorMO.Job.Id);
            a_dataChanges.FlagProductionChanges(a_predecessorMO.Job.Id);
        }

        if (newSuccessorMOs || alteredSuccessorKeys)
        {
            a_predecessorMO.Job.NewOrUpdatedSuccessorMOsAdded = true;
            a_dataChanges.FlagEligibilityChanges(a_predecessorMO.Job.Id);
            a_dataChanges.FlagProductionChanges(a_predecessorMO.Job.Id);
        }

        successorMOs.Clear();

        for (int i = 0; i < a_newSuccessors.Count; ++i)
        {
            SuccessorMO toCopy = a_newSuccessors[i];
            SuccessorMO copySuccessor = new (toCopy, a_predecessorMO);
            successorMOs.Add(copySuccessor);
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="a_successorMo"></param>
    internal void Add(SuccessorMO a_successorMo, ScenarioDetail a_sd)
    {
        successorMOs.Add(a_successorMo);
        a_sd.SignalCriticalSuccessorMOUpdate();
        if (a_successorMo.PredecessorMO != null)
        {
            a_successorMo.PredecessorMO.Job.NewOrUpdatedSuccessorMOsAdded = true;
        }
    }

    /// <summary>
    /// Determine whether there are any new successor MOs in a list of new successorMO updates.
    /// Use this function when a list of updated successor MOs is passed into this object.
    /// </summary>
    /// <param name="newSuccessors">The list of successorMO updates.</param>
    /// <returns></returns>
    private bool AnyNewSuccessorMOs(SuccessorMOArrayList newSuccessors)
    {
        bool newSuccessorMOs = false;

        for (int i = 0; i < newSuccessors.Count; ++i)
        {
            SuccessorMO newSucMO = newSuccessors[i];
            SuccessorMO findSucMOResult = FindSuccessorMO(newSucMO.ExternalId, successorMOs);

            if (findSucMOResult == null)
            {
                // The successor MO is new.
                newSucMO.HasNewOrUpdatedPredecessors = true;
                newSuccessorMOs = true;
            }
        }

        // No new successor MOs were found.
        return newSuccessorMOs;
    }

    /// <summary>
    /// Given a new list of successor MOs, determine whether any of the existing MOs in this list
    /// exist in the new list of successor MOs in an updated state. For example transfer time changes
    /// MO external id changes.
    /// </summary>
    /// <param name="newSuccessors"></param>
    /// <param name="updatedSuccessorMOs">Whether any successor MOs have been updated.</param>
    /// <param name="alteredSuccessorKeys">Whether any successor MOs have had their successor key changed.</param>
    private void AnyUpdatedSuccessorMOs(ArrayList newSuccessors, out bool updatedSuccessorMOs, out bool alteredSuccessorKeys)
    {
        updatedSuccessorMOs = false;
        alteredSuccessorKeys = false;

        for (int i = 0; i < successorMOs.Count; ++i)
        {
            SuccessorMO existingMO = (SuccessorMO)successorMOs[i];
            SuccessorMO successorMO = FindSuccessorMO(existingMO.ExternalId, newSuccessors);

            if (successorMO != null)
            {
                if (!existingMO.Equals(successorMO))
                {
                    updatedSuccessorMOs = true;
                }

                if (!existingMO.SameSuccessorKey(successorMO))
                {
                    successorMO.HasNewOrUpdatedPredecessors = true;
                    alteredSuccessorKeys = true;
                }
            }
        }
    }

    /// <summary>
    /// Determine whether the ArrayList contains
    /// </summary>
    /// <param name="externalId"></param>
    /// <param name="successorMOs"></param>
    /// <returns></returns>
    private SuccessorMO FindSuccessorMO(string externalId, ArrayList successorMOs)
    {
        for (int i = 0; i < successorMOs.Count; ++i)
        {
            SuccessorMO sucMO = (SuccessorMO)successorMOs[i];
            if (externalId == sucMO.ExternalId)
            {
                return sucMO;
            }
        }

        return null;
    }

    internal bool UnscheduleMarkedSuccessorMOs()
    {
        bool succMoUnscheduled = false;
        for (int i = 0; i < Count; ++i)
        {
            SuccessorMO sucMO = this[i];

            if (sucMO.HasNewOrUpdatedPredecessors)
            {
                if (sucMO.SuccessorManufacturingOrder != null)
                {
                    Job job = sucMO.SuccessorManufacturingOrder.Job;
                    if (job.ScheduledOrPartiallyScheduled)
                    {
                        succMoUnscheduled = true;
                        job.Unschedule();
                    }
                }

                sucMO.HasNewOrUpdatedPredecessors = false;
            }
        }

        return succMoUnscheduled;
    }
}