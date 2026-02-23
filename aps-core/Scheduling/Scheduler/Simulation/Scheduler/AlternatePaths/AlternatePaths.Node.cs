using PT.APSCommon;

namespace PT.Scheduler;

/// <summary>
/// Each AlternatePath specifies one possible routing that can be followed for an MO.  Each MO has at least one AlternatePath.
/// </summary>
public partial class AlternatePath
{
    /// <summary>
    /// Used to create a network of operations that define the steps of an alternate path.
    /// </summary>
    public partial class Node
    {
        #region Eligibility
        /// <summary>
        /// This is resource eligibility taking MO.CanSpanPlants and Job.CanSpanPlants into consideration
        /// and narrowed down after the first operation is scheduled (taking the CanSpanPlants settings into consideration).
        /// This is initially set by a copying from TotalPlantResourceEligibilitySets. It may then be reduced during scheduling.
        /// </summary>
        private ResReqsPlantResourceEligibilitySets m_resReqsEligibilityNarrowedDuringSimulation = new ();

        /// <summary>
        /// This is resource eligibility taking MO.CanSpanPlants and Job.CanSpanPlants into consideration
        /// and narrowed down after the first operation is scheduled (taking the CanSpanPlants settings into consideration).
        /// This is the set of eligible resouces that are used during simulation.
        /// Each element represents a resource requirement.
        /// </summary>
        internal ResReqsPlantResourceEligibilitySets ResReqsEligibilityNarrowedDuringSimulation => m_resReqsEligibilityNarrowedDuringSimulation;

        public PlantResourceEligibilitySet GetRequirementPlantEligibleResourceSet(int a_requirementIndex)
        {
            if (a_requirementIndex >= 0 && a_requirementIndex <= ResReqsMasterEligibilitySet.Count - 1)
            {
                return ResReqsMasterEligibilitySet[a_requirementIndex];
            }

            return null;
        }

        /// <summary>
        /// Eligibility prior to taking any scheduling/narrowing into consideration. During Scheduling, eligibility may be changed for
        /// a variety of reasons. For instance, when the first operation is scheduled Job and MO CanSpanPlants might constrain
        /// a the entire job or just a manufacturing order to a single plant.
        /// At the start of simulation, a copy of this master is taken. The copy is prunned appropriately for features and as scheduling occurs.
        /// </summary>
        private readonly ResReqsPlantResourceEligibilitySets m_resReqsMasterEligibilitySet = new ();

        /// <summary>
        /// Eligibility prior to taking any scheduling/narrowing into consideration. During Scheduling, eligibility may be changed for
        /// a variety of reasons. For instance, when the first operation is scheduled Job and MO CanSpanPlants might constrain
        /// a the entire job or just a manufacturing order to a single plant.
        /// At the start of simulation, a copy of this master is taken. The copy is prunned appropriately for features and as scheduling occurs.
        /// </summary>
        internal ResReqsPlantResourceEligibilitySets ResReqsMasterEligibilitySet => m_resReqsMasterEligibilitySet;

        /// <summary>
        /// Part of Step 4 of eligibility process.
        /// Call this function to specify that everything in the operation's ResourceRequirements' EffectiveResourceRequirementEligibility
        /// is eligible (the MO can span plants).
        /// Invariants:
        /// 1. The corresponding operation must be a type of InternalOperation.
        /// 2. The corresponding operation's ResourceRequirementManager has computed eligibility.
        /// </summary>
        internal void AllPlantsAreEligible(Dictionary<BaseId, ResReqsPlantResourceEligibilitySets> pathNodeEligibilitySet)
        {
            ResReqsMasterEligibilitySet.Clear();
            InternalOperation io = (InternalOperation)m_operation;
            if (pathNodeEligibilitySet.ContainsKey(io.Id))
            {
                ResReqsMasterEligibilitySet.AddRange(pathNodeEligibilitySet[io.Id]);
            }

            ResetAdjustedPlantResourceEligibilitySets();
        }

        /// <summary>
        /// Copies totalPlantResourceEligibilitySets over the adjusted resource eligibility set.
        /// </summary>
        internal void ResetAdjustedPlantResourceEligibilitySets()
        {
            m_resReqsEligibilityNarrowedDuringSimulation = new ResReqsPlantResourceEligibilitySets(ResReqsMasterEligibilitySet);
        }

        /// <summary>
        /// Set TotalPlantResourceEligibilitySets and adjustedPlantResourceEligibilitySets based on a set of plants that have been determined
        /// to be eligible.
        /// </summary>
        /// <param name="eligiblePlants">The list of plants that this operation may be processed at.</param>
        internal bool SpecificationOfEligiblePlants(BaseIdList eligiblePlants, Dictionary<BaseId, ResReqsPlantResourceEligibilitySets> pathNodeEligibilitySet)
        {
            ResReqsPlantResourceEligibilitySets previousSetCopy = new (ResReqsMasterEligibilitySet);

            ResReqsMasterEligibilitySet.Clear();
            InternalOperation io = m_operation;

            // Each iteration through the for loop is for a resource requirement.
            for (int resourceRequirementI = 0; resourceRequirementI < io.ResourceRequirements.Count; ++resourceRequirementI)
            {
                if (pathNodeEligibilitySet.TryGetValue(io.Id, out ResReqsPlantResourceEligibilitySets presal))
                {
                    if (presal.Count > 0)
                    {
                        PlantResourceEligibilitySet pres = presal[resourceRequirementI];
                        PlantResourceEligibilitySet presFilteredToEligiblePlants = new ();

                        // Pass through all the eligible plant resource sets for the current resource requirement.
                        SortedDictionary<BaseId, EligibleResourceSet>.Enumerator ersEtr = pres.GetEnumerator();
                        while (ersEtr.MoveNext())
                        {
                            BaseId plantId = ersEtr.Current.Key;

                            foreach (BaseId eligiblePlantId in eligiblePlants)
                            {
                                // If this plant is in the list of eligible plants, add the plant resource set to TotalPlantResourceEligibilitySets.
                                if (eligiblePlantId == plantId)
                                {
                                    presFilteredToEligiblePlants.Add(plantId, ersEtr.Current.Value);
                                    break;
                                }
                            }
                        }

                        ResReqsMasterEligibilitySet.Add(presFilteredToEligiblePlants);
                    }
                }
            }

            ResetAdjustedPlantResourceEligibilitySets();

            //Return whether there was a change
            return !previousSetCopy.Equals(ResReqsMasterEligibilitySet);
        }

        /// <summary>
        /// Only allow a specific plant to satisfy the resource requirements.
        /// </summary>
        /// <param name="a_plantId"></param>
        internal void AdjustedPlantResourceEligibilitySets_FilterDownToSpecificPlant(BaseId a_plantId, ResReqsPlantResourceEligibilitySets.ExcludeFromManualFilter a_excludeFromManualFilter)
        {
            ResReqsEligibilityNarrowedDuringSimulation.FilterDownToSpecificPlant(a_plantId);
        }

        /// <summary>
        /// Activity's whose eligibility has already been overridden are not filtered.
        /// </summary>
        internal void AdjustedPlantResourceEligibilitySets_Filter(ResReqsPlantResourceEligibilitySets.ExcludeFromManualFilter a_excludeFromManualFilter)
        {
            InternalOperation op = (InternalOperation)Operation;
            op.AdjustedPlantResourceEligibilitySets_Filter(a_excludeFromManualFilter);
        }

        /// <summary>
        /// Filter this node's AdjustedPlantEligibilitySet and recursively filter its successors.
        /// </summary>
        internal void AdjustedPlantResourceEligibilitySets_FilterNodeAndSuccessors(ResReqsPlantResourceEligibilitySets.ExcludeFromManualFilter a_excludeFromManualFilter)
        {
            AdjustedPlantResourceEligibilitySets_Filter(a_excludeFromManualFilter);

            for (int sucI = 0; sucI < Successors.Count; ++sucI)
            {
                Node suc = Successors[sucI].Successor;
                suc.AdjustedPlantResourceEligibilitySets_FilterNodeAndSuccessors(a_excludeFromManualFilter);
            }
        }
        #endregion

        #region Simulation
        internal void ResetSimulationStateVariables()
        {
            m_successors.ResetSimulationStateVariables();
            ResetAdjustedPlantResourceEligibilitySets();
            MaxPredecessorReleaseTicks = -1;
            HasAnotherPathScheduled = false;
        }

        internal void SimulationInitialization()
        {
            m_waitingForPathRelease = true;
        }

        /// <summary>
        /// Whether the alternate path this node belongs to has been released.
        /// It is possible that a node checks to schedule due to material events but the path is not eligible/released yet
        /// </summary>
        private bool m_waitingForPathRelease;

        internal void PathReleased()
        {
            m_waitingForPathRelease = false;
        }

        internal bool Released
        {
            get
            {
                if (m_waitingForPathRelease)
                {
                    return false;
                }

                for (int i = 0; i < Predecessors.Count; ++i)
                {
                    InternalOperation op = m_predecessors[i].Predecessor.Operation;
                    if (!op.Scheduled)
                    {
                        if (!op.IsFinishedOrOmitted && !op.Released)
                        {
                            return false;
                        }
                    }

                    if (!m_predecessors[i].Released)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private long m_maxPredecessorReleaseTicks;

        /// <summary>
        /// The maximum reported release ticks among all the predecessor operations that have reported their release ticks.
        /// Otherwise the value is -1.
        /// </summary>
        internal long MaxPredecessorReleaseTicks
        {
            get => m_maxPredecessorReleaseTicks;
            private set => m_maxPredecessorReleaseTicks = value;
        }

        internal void ReportPredecessorReleaseTicks(long a_predRelTicks)
        {
            if (a_predRelTicks > MaxPredecessorReleaseTicks)
            {
                MaxPredecessorReleaseTicks = a_predRelTicks;
            }
        }
        #endregion
    }
}