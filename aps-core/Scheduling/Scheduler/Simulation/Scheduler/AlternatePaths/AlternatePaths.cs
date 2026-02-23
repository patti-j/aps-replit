using System.Collections;
using System.ComponentModel;

using PT.APSCommon;
using PT.Scheduler.Schedule;
using PT.Scheduler.Simulation.Extensions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Each AlternatePath specifies one possible routing that can be followed for an MO.  Each MO has at least one AlternatePath.
/// </summary>
public partial class AlternatePath
{
    #region Leaves
    /// <summary>
    /// The set of nodes that have no predecessor operations.
    /// </summary>
    private readonly NodeCollection m_leaves = new ();

    /// <summary>
    /// The set of nodes that have no predecessor operations.
    /// </summary>
    [Browsable(false)]
    public NodeCollection Leaves => m_leaves;

    private NodeCollection m_effectiveLeaves = new ();

    /// <summary>
    /// The set of nodes that are have no predecessor operations or no have predecessors that are finished or omitted.
    /// </summary>
    internal NodeCollection EffectiveLeaves => m_effectiveLeaves;

    /// <summary>
    /// Find the lead operations in the routing. The leafs are the operations that don't have any predecessor operations.
    /// The leaves are stored in the "leaves" member variable.
    /// </summary>
    private void CollectLeaves()
    {
        m_leaves.Clear();
        IEnumerator<KeyValuePair<string, Node>> alternateNodesEnumerator = m_alternateNodeSortedList.GetEnumerator();

        while (alternateNodesEnumerator.MoveNext())
        {
            Node node = alternateNodesEnumerator.Current.Value;
            if (node.Predecessors.Count == 0)
            {
                m_leaves.Add(node);
            }
        }
    }

    /// <summary>
    /// Root operations have no successors.
    /// </summary>
    /// <returns></returns>
    public NodeCollection GetRoots()
    {
        NodeCollection roots = new ();
        IEnumerator<KeyValuePair<string, Node>> alternateNodesEnumerator = m_alternateNodeSortedList.GetEnumerator();

        while (alternateNodesEnumerator.MoveNext())
        {
            Node node = alternateNodesEnumerator.Current.Value;
            if (node.Successors.Count == 0)
            {
                roots.Add(node);
            }
        }

        return roots;
    }

    /// <summary>
    /// Regenerates the effective leaves.
    /// The effective leaves will need to be recalculated when a path is created or updated.
    /// It will also have to be recalcuated when the production status of activities is changed.
    /// Example
    /// Give routing 1->2->3 where operation 1 is finished. The leaf would be operation 2. But
    /// if operation 1 were unfinished, the leaf would become operation 1. When this function is
    /// called, operation 1 would be set to the only leaf in EffectiveLeaves.
    /// See bug 4823 and related bugs for the problems this function was added to solve.
    /// </summary>
    private void ResetEffectiveLeaves()
    {
        m_effectiveLeaves = GetEffectiveLeaves();
    }

    /// <summary>
    /// Non-finished, non-omitted operations with no predescessors or whose predecessors are all either finished and or omitted.
    /// </summary>
    /// <returns></returns>
    internal NodeCollection GetEffectiveLeaves()
    {
        Dictionary<BaseId, Node> effectiveLeavesDictionary = new ();
        NodeCollection leaves = Leaves;

        for (int leavesI = 0; leavesI < leaves.Count; ++leavesI)
        {
            Node node = leaves[leavesI];
            GetEffectiveLeavesHelper(node, effectiveLeavesDictionary);
        }

        NodeCollection effectiveLeaves = new ();
        Dictionary<BaseId, Node>.Enumerator en = effectiveLeavesDictionary.GetEnumerator();
        while (en.MoveNext())
        {
            effectiveLeaves.Add(en.Current.Value);
        }

        return effectiveLeaves;
    }

    private void GetEffectiveLeavesHelper(Node a_node, Dictionary<BaseId, Node> a_effectiveLeavesDictionary)
    {
        BaseOperation op = a_node.Operation;

        if (op.IsNotFinishedAndNotOmitted)
        {
            if (!a_effectiveLeavesDictionary.ContainsKey(op.Id))
            {
                a_effectiveLeavesDictionary.Add(op.Id, a_node);
            }
        }
        else
        {
            for (int sucI = 0; sucI < a_node.Successors.Count; ++sucI)
            {
                GetEffectiveLeavesHelper(a_node.Successors[sucI].Successor, a_effectiveLeavesDictionary);
            }
        }
    }
    #endregion

    #region Path usage. Functionality to start using a specific path.
    /// <summary>
    /// Call this function when this is the path that you want to use.
    /// It associates operations with the nodes of this path.
    /// </summary>
    internal void AssociateOpsWithPathNodes()
    {
        for (int leafI = 0; leafI < m_leaves.Count; ++leafI)
        {
            m_leaves[leafI].Operation.AlternatePathNode = null;
        }

        for (int leafI = 0; leafI < m_leaves.Count; ++leafI)
        {
            AssociateOpWithPathNode(m_leaves[leafI]);
        }

        ResetEffectiveLeaves();
    }

    private void RecalcEffectiveLeaves()
    {
        m_effectiveLeaves = GetEffectiveLeaves();
    }

    /// <summary>
    /// This function should be called when the production status of activites are updated.
    /// An update to the production status of an activity may have an affect on the effective leaves.
    /// Example
    /// Give routing 1->2->3 where operation 1 is finished. The leaf would be operation 2. But
    /// if operation 1 were unfinished, the leaf would become operation 1. When this function is
    /// called, operation 1 would be set to the only leaf in EffectiveLeaves.
    /// See bug 4823 and related bugs for the problems this function was added to solve.
    /// </summary>
    internal void StatusesUpdated()
    {
        ResetEffectiveLeaves();
    }

    /// <summary>
    /// Recursively associate this node and all its successors with their opertions.
    /// </summary>
    /// <param name="node"></param>
    private void AssociateOpWithPathNode(Node a_node)
    {
        if (a_node.Operation.AlternatePathNode == a_node)
        {
            // This prevents the operation from being acted on multiple times
            // when there are multiple predecessors.
            return;
        }

        a_node.Operation.AlternatePathNode = a_node;

        for (int successorI = 0; successorI < a_node.Successors.Count; ++successorI)
        {
            AssociateOpWithPathNode(a_node.Successors[successorI].Successor);
        }
    }
    #endregion

    #region Eligibility
    /// <summary>
    /// Part of Step 2 of eligibility. Each operation determines where it can be made.
    /// plants capable of satisfying all the resource requirements.
    /// </summary>
    internal void CreateEffectiveResourceEligibilitySets(ProductRuleManager a_productRuleManager)
    {
        IEnumerator<KeyValuePair<string, Node>> alternateNodesEnumerator = m_alternateNodeSortedList.GetEnumerator();

        while (alternateNodesEnumerator.MoveNext())
        {
            Node node = alternateNodesEnumerator.Current.Value;

            if (node.Operation is InternalOperation)
            {
                InternalOperation operation = node.Operation;
                if (operation.IsFinishedOrOmitted)
                {
                    operation.ClearEffectiveResourceEligibilitySet();
                }
                else
                {
                    operation.CreateEffectiveResourceEligibilitySet(a_productRuleManager);
                }
            }
        }
    }

    /// <summary>
    /// Part of Step 4 of eligibility process.
    /// This is to handle the case where all plants are eligible.
    /// In this case the eligibility that was calculated based solely on which
    /// plants are able to manufacture operations all end up in the final list
    /// of plants that are eligible to manufacture the operation.
    /// </summary>
    internal void AllPlantsAreEligible(Dictionary<BaseId, ResReqsPlantResourceEligibilitySets> a_pathNodeEligibilitySet)
    {
        IEnumerator<KeyValuePair<string, Node>> enumerator = m_alternateNodeSortedList.GetEnumerator();

        while (enumerator.MoveNext())
        {
            Node node = enumerator.Current.Value;
            node.AllPlantsAreEligible(a_pathNodeEligibilitySet);
        }
    }

    /// <summary>
    /// Get an enumerator that iterates through all the nodes that aren't finished or omitted.
    /// </summary>
    /// <returns></returns>
    internal IDictionaryEnumerator GetSchedulableNodeEnumerator()
    {
        return new SchedulableNodeEnumerator(m_alternateNodeSortedList);
    }

    /// <summary>
    /// An enumerator class designed to iterate through all the nodes that aren't finished or omitted.
    /// </summary>
    private class SchedulableNodeEnumerator : IDictionaryEnumerator
    {
        internal SchedulableNodeEnumerator(SortedList<string, Node> a_list)
        {
            m_sortedNodeList = a_list;
            Reset();
        }

        private readonly SortedList<string, Node> m_sortedNodeList;
        private IEnumerator<KeyValuePair<string, Node>> m_enumerator;

        public DictionaryEntry Entry => new (m_enumerator.Current.Key, m_enumerator.Current.Value);

        public object Key => m_enumerator.Current.Key;

        public object Value => m_enumerator.Current.Value;

        public object Current => new DictionaryEntry(m_enumerator.Current.Key, m_enumerator.Current.Value);

        public bool MoveNext()
        {
            bool foundNextNode = false;
            while (m_enumerator.MoveNext())
            {
                Node node = m_enumerator.Current.Value;
                InternalOperation io = (InternalOperation)node.Operation;
                if (io.IsNotFinishedAndNotOmitted)
                {
                    foundNextNode = true;
                    break;
                }
            }

            return foundNextNode;
        }

        public void Reset()
        {
            m_enumerator = m_sortedNodeList.GetEnumerator();
        }
    }

    /// <summary>
    /// Part of Step 4 of eligibility process.
    /// Take plants that are not able to perform all operations in the MO out of consideration. The MO must be manufactured within a single plant.
    /// This also takes ManufacturingOrder.Locked plant into consideration.
    /// </summary>
    internal BaseIdList MOCantSpanPlants(Dictionary<BaseId, ResReqsPlantResourceEligibilitySets> a_pathNodeEligibilitySet, out bool o_changed)
    {
        // An enumerator used for examining all the node in an alternate path.
        IDictionaryEnumerator enumerator = GetSchedulableNodeEnumerator();

        // The set of all plants that can manufacture the path.
        BaseIdList eligiblePlants = new ();

        // Determine which plants are capable of completely processing this MO.
        if (enumerator.MoveNext())
        {
            Node node = (Node)enumerator.Value;

            // Start off by determining all the plants capable of satisfying one of the MO's operations.
            InternalOperation io = node.Operation;
            ResReqsPlantResourceEligibilitySets eligibleSetsAL = a_pathNodeEligibilitySet[io.Id];

            if (eligibleSetsAL.Count > 0)
            {
                PlantResourceEligibilitySet pres = eligibleSetsAL.PrimaryEligibilitySet;

                if (m_manufacturingOrder.LockedPlant != null)
                {
                    if (pres.Contains(m_manufacturingOrder.LockedPlant.Id))
                    {
                        eligiblePlants.Add(m_manufacturingOrder.LockedPlant.Id);
                    }
                }
                else
                {
                    // Add all the eligible plants from the first node.
                    SortedDictionary<BaseId, EligibleResourceSet>.Enumerator ersEtr = pres.GetEnumerator();
                    while (ersEtr.MoveNext())
                    {
                        BaseId plantId = ersEtr.Current.Key;
                        eligiblePlants.Add(plantId);
                    }
                }

                while (enumerator.MoveNext())
                {
                    node = (Node)enumerator.Value;

                    // Eliminate plants from the original plant set that are not capable of satisfying the requirements of this operation.
                    io = (InternalOperation)node.Operation;
                    eligibleSetsAL = a_pathNodeEligibilitySet[io.Id];

                    if (eligibleSetsAL.Count == 0)
                    {
                        eligiblePlants.Clear();
                    }
                    else
                    {
                        pres = eligibleSetsAL.PrimaryEligibilitySet;

                        //Check if the op has a default resource or any of it's activities are locked to a resource. The eligibility set must be restricted by the the default/locked resource's plant ID
                        BaseId restrictedPlantId = BaseId.NULL_ID;
                        if (io.ResourceRequirements.HasDefaultResource)
                        {
                            restrictedPlantId = io.ResourceRequirements.PrimaryResourceRequirement.DefaultResource.PlantId;
                        }
                        else if (io.Locked != lockTypes.Unlocked)
                        {
                            Resource res = io.GetFirstLockedResource();
                            restrictedPlantId = res.PlantId;
                        }

                        for (int i = eligiblePlants.Count - 1; i >= 0; i--)
                        {
                            BaseId eligiblePlantId = eligiblePlants.ToList()[i];

                            if (!pres.Contains(eligiblePlantId) || (restrictedPlantId != BaseId.NULL_ID && eligiblePlantId != restrictedPlantId))
                            {
                                eligiblePlants.Remove(eligiblePlantId);
                            }
                        }
                    }
                }
            }
        }

        // Copy effective eligibility from the plants that are able to process the entire MO.
        o_changed = NarrowDownEligibilityToThesePlants(eligiblePlants, a_pathNodeEligibilitySet);
        return eligiblePlants;
    }

    /// <summary>
    /// Reduce eligibility to the set of plants specified.
    /// </summary>
    /// <param name="eligiblePlants"></param>
    internal bool NarrowDownEligibilityToThesePlants(BaseIdList a_eligiblePlants, Dictionary<BaseId, ResReqsPlantResourceEligibilitySets> a_pathNodeEligibilitySet)
    {
        IEnumerator<KeyValuePair<string, Node>> enumerator = m_alternateNodeSortedList.GetEnumerator();
        bool changed = false;
        while (enumerator.MoveNext())
        {
            Node node = enumerator.Current.Value;
            changed |= node.SpecificationOfEligiblePlants(a_eligiblePlants, a_pathNodeEligibilitySet);
        }

        return changed;
    }

    /// <summary>
    /// Filter the adjusted eligibility sets down to a single plant.
    /// </summary>
    /// <param name="a_plantId"></param>
    internal void AdjustedPlantResourceEligibilitySets_FilterDownToSpecificPlant(BaseId a_plantId)
    {
        IDictionaryEnumerator enumerator = GetSchedulableNodeEnumerator();

        while (enumerator.MoveNext())
        {
            Node node = (Node)enumerator.Value;
            node.AdjustedPlantResourceEligibilitySets_FilterDownToSpecificPlant(a_plantId, ResReqsPlantResourceEligibilitySets.ExcludeFromManualFilter.NoFilter());
        }
    }

    /// <summary>
    /// Activity's whose eligibility has already been overridden are not filtered.
    /// </summary>
    internal void AdjustedPlantResourceEligibilitySets_Filter(ResReqsPlantResourceEligibilitySets.ExcludeFromManualFilter a_excludeFromManualFilter)
    {
        IDictionaryEnumerator enumerator = GetSchedulableNodeEnumerator();

        while (enumerator.MoveNext())
        {
            Node node = (Node)enumerator.Value;
            node.AdjustedPlantResourceEligibilitySets_Filter(a_excludeFromManualFilter);
        }
    }

    /// <summary>
    /// Whether the the adjusted plant resource eligibilty sets can satisfy the resource requirements of the path; whether the path can be scheduled.
    /// </summary>
    /// <returns></returns>
    internal bool AdjustedPlantResourceEligibilitySets_IsSatisfiable()
    {
        return IsSatisfiable(IsSatisfiableType.AdjustedPlantResourceEligibilitySets);
    }

    /// <summary>
    /// Whether all the nodes in the alternate path are satisfiable somewhere.
    /// This is based on the results of step 4.
    /// </summary>
    /// <returns>true if there is an eligible place where the node can be manufactured.</returns>
    internal bool EligibleResources_IsSatisfiable()
    {
        return IsSatisfiable(IsSatisfiableType.EligibleResources);
    }

    /// <summary>
    /// A helper type for the IsSatisfiable function used to tell whether to run the function against the EligibleResource set or set AdjustedPlantResourceEligibilitySets.
    /// </summary>
    private enum IsSatisfiableType
    {
        /// <summary>
        /// Full eligibility before taking the simulation and scheduling into consideraiton.
        /// </summary>
        EligibleResources,

        /// <summary>
        /// Eligibility can change during an optimize and is a subset of full eligibility.
        /// </summary>
        AdjustedPlantResourceEligibilitySets
    }

    /// <summary>
    /// Whether the eligibility set is sufficient to schedule a job.
    /// </summary>
    /// <param name="a_satisfiableType"></param>
    /// <returns></returns>
    private bool IsSatisfiable(IsSatisfiableType a_satisfiableType)
    {
        bool ret = true;
        IDictionaryEnumerator enumerator = GetSchedulableNodeEnumerator();

        while (enumerator.MoveNext())
        {
            Node node = (Node)enumerator.Value;
            InternalOperation op = (InternalOperation)node.Operation;

            if (a_satisfiableType == IsSatisfiableType.AdjustedPlantResourceEligibilitySets)
            {
                bool checkNode = false;

                for (int i = 0; i < op.Activities.Count; ++i)
                {
                    InternalActivity act = op.Activities.GetByIndex(i);
                    if (act.HasResReqEligibilityBeenOverridden())
                    {
                        if (!act.ResReqsEligibilityNarrowedDuringSimulation.IsSatisfiable())
                        {
                            ret = false;
                        }
                        else
                        {
                            checkNode = true;
                        }
                    }
                    else
                    {
                        checkNode = true;
                    }
                }

                if (checkNode)
                {
                    if (!node.ResReqsEligibilityNarrowedDuringSimulation.IsSatisfiable())
                    {
                        ret = false;
                    }
                    else
                    {
                        // Verify the resource the activities must schedule on are included in the
                        // narrowed eligibility set. 
                        for (int actI = 0; actI < op.Activities.Count; ++actI)
                        {
                            InternalActivity act = op.Activities.GetByIndex(actI);
                            InternalResource mustSceduleRes = act.GetMustScheduleResource();

                            if (mustSceduleRes != null)
                            {
                                bool mustScheduleResInEligSet = false;
                                for (int rreI = 0; rreI < node.ResReqsEligibilityNarrowedDuringSimulation.Count; ++rreI)
                                {
                                    PlantResourceEligibilitySet pres = node.ResReqsEligibilityNarrowedDuringSimulation[rreI];
                                    SortedDictionary<BaseId, EligibleResourceSet>.Enumerator etr = pres.GetEnumerator();
                                    while (etr.MoveNext())
                                    {
                                        EligibleResourceSet rs = etr.Current.Value;
                                        if (rs.Contains(mustSceduleRes))
                                        {
                                            mustScheduleResInEligSet = true;
                                        }
                                    }
                                }

                                if (!mustScheduleResInEligSet)
                                {
                                    ret = false;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (!node.ResReqsMasterEligibilitySet.IsSatisfiable())
                {
                    ret = false;
                }
            }
        }

        if (ExcludedByExpediteBecauseItsLeadOpsArentEligibleOnSpecifiedRes)
        {
            ret = false;
        }

        return ret;
    }

    public string GetUnsatisifedNodesDescription()
    {
        System.Text.StringBuilder builder = new ();

        NodeCollection badNodes = GetUnsatisfiableNodes();
        if (badNodes.Count > 0)
        {
            builder.Append(badNodes.Count + " Operations lacking Active, Capable Resources: ");
            for (int i = 0; i < badNodes.Count; i++)
            {
                Node node = badNodes[i];
                string nxtNodeTxt = node.Operation.Name;
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(nxtNodeTxt);
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Returns a collection of all nodes that cannot be satisfied by existing Resources.
    /// </summary>
    internal NodeCollection GetUnsatisfiableNodes()
    {
        NodeCollection nodes = new ();
        IDictionaryEnumerator enumerator = GetSchedulableNodeEnumerator();

        while (enumerator.MoveNext())
        {
            Node node = (Node)enumerator.Value;
            if (!node.ResReqsMasterEligibilitySet.IsSatisfiable())
            {
                nodes.Add(node);
            }
        }

        return nodes;
    }
    #endregion

    #region Simulation
    /// <summary>
    /// Simulation start date, constrained by the clock
    /// </summary>
    private long m_validityStart;
    /// <summary>
    /// Simulation end date, constrained by the planning horizon
    /// </summary>
    private long m_validityEnd;

    internal long ValidityStartDateSimConstrained => m_validityStart;
    internal long ValidityEndDateSimConstrained => m_validityEnd;

    // !ALTERNATE_PATH!; ResetSimulationStateVariables() for the AlternatePath.
    internal void ResetSimulationStateVariables(ScenarioDetail a_sd)
    {
        for (int i = 0; i < m_leaves.Count; ++i)
        {
            m_leaves[i].ResetSimulationStateVariables();
        }

        IEnumerator<KeyValuePair<string, Node>> pathEtr = m_alternateNodeSortedList.GetEnumerator();
        while (pathEtr.MoveNext())
        {
            Node pathNode = pathEtr.Current.Value;
            pathNode.Operation.ResetSimulationStateVariables(a_sd);
        }

        PathReleaseEventProcessed = false;

        m_validityStart = Math.Max(a_sd.Clock, ValidityStartDate.Ticks);
        m_validityEnd = Math.Min(a_sd.GetPlanningHorizonEndTicks(), ValidityEndDate.Ticks);

        ExcludedBySchedulingDueToValidity = false;
    }

    #region bools
    private BoolVector32 m_simBools;

    private const int c_excludedByExpediteBecauseItsLeadOpsArentEligibleOnSpecifiedResIdx = 0;
    private const int c_excludedBySchedulingDueToValidityIdx = 1;

    internal bool ExcludedByExpediteBecauseItsLeadOpsArentEligibleOnSpecifiedRes
    {
        get => m_simBools[c_excludedByExpediteBecauseItsLeadOpsArentEligibleOnSpecifiedResIdx];
        set => m_simBools[c_excludedByExpediteBecauseItsLeadOpsArentEligibleOnSpecifiedResIdx] = value;
    }

    public bool ExcludedBySchedulingDueToValidity
    {
        get => m_simBools[c_excludedBySchedulingDueToValidityIdx];
        internal set => m_simBools[c_excludedBySchedulingDueToValidityIdx] = value;
    }
    #endregion

    // !ALTERNATE_PATH!; SimulationInitialization() for the AlternatePath.
    internal void SimulationInitialization(PlantManager a_plantManager, ProductRuleManager a_productRuleManager, ExtensionController a_extensionController, ICalculatedValueCacheManager a_cacheManager)
    {
        IDictionaryEnumerator pathEtr = GetSchedulableNodeEnumerator();
        while (pathEtr.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)pathEtr.Current;
            Node pathNode = (Node)de.Value;
            pathNode.SimulationInitialization();
            pathNode.Operation.SimulationInitialization(a_plantManager, a_productRuleManager, a_extensionController, a_cacheManager);
        }

        m_simBools.Clear();
    }

    internal void PostSimulationInitialization()
    {
        IDictionaryEnumerator pathEtr = GetSchedulableNodeEnumerator();
        while (pathEtr.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)pathEtr.Current;
            Node pathNode = (Node)de.Value;
            pathNode.Operation.PostSimulationInitialization();
        }
    }

    internal void UnscheduleNonResourceUsingOperations()
    {
        IDictionaryEnumerator alternateNodesEnumerator = GetSchedulableNodeEnumerator();

        while (alternateNodesEnumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)alternateNodesEnumerator.Current;
            Node node = (Node)de.Value;
            BaseOperation baseOperation = node.Operation;

            if (baseOperation is InternalOperation)
            {
                InternalOperation op = (InternalOperation)baseOperation;

                op.UnscheduleNonResourceUsingOperations();
            }
        }
    }

    /// <summary>
    /// Whether each node has a max of 1 pred and/or successor, only 1 node has 0 preds, and only 1 node has 0 successors.
    /// </summary>
    /// <returns></returns>
    internal bool Linear()
    {
        bool firstOpFound = false;
        bool lastOpFound = false;

        for (int nodeI = 0; nodeI < AlternateNodeSortedList.Count; ++nodeI)
        {
            Node node = AlternateNodeSortedList.Values[nodeI];

            if (node.Predecessors.Count > 1 || node.Successors.Count > 1)
            {
                // The node has more than 1 predecessor or successor.
                return false;
            }

            if (node.Predecessors.Count == 0)
            {
                if (firstOpFound)
                {
                    // A node with no predessors was already encountered.
                    return false;
                }

                firstOpFound = true;
            }

            if (node.Successors.Count == 0)
            {
                if (lastOpFound)
                {
                    // A node with no successors was already encountered.
                    return false;
                }

                lastOpFound = true;
            }
        }

        return true;
    }

    /// <summary>
    /// Return the number of operations that produce a product.
    /// </summary>
    /// <returns></returns>
    internal int OpsProducingProductsCount()
    {
        int count = 0;
        for (int nodeI = 0; nodeI < AlternateNodeSortedList.Count; ++nodeI)
        {
            Node node = AlternateNodeSortedList.Values[nodeI];
            if (node.Operation.Products.Count > 0)
            {
                ++count;
            }
        }

        return count;
    }

    internal Node this[int a_nodeIndex] => AlternateNodeSortedList.Values[a_nodeIndex];

    public int NodeCount => AlternateNodeSortedList.Count;

    /// <summary>
    /// Whether the path has been released. Useful for knowing if this path can be removed when another path schedules
    /// </summary>
    internal bool PathReleaseEventProcessed { get; set; }
    #endregion

    internal void AdjustRequiredQty(decimal a_ratio, decimal a_newReqMOQty, Product a_primaryProduct, ProductRuleManager a_productRuleManager)
    {
        for (int nodeI = 0; nodeI < AlternateNodeSortedList.Count; ++nodeI)
        {
            Node node = AlternateNodeSortedList.Values[nodeI];
            ResourceOperation ro = (ResourceOperation)node.Operation;
            ro.AdjustRequiredQty(a_ratio, a_newReqMOQty, null, a_primaryProduct, a_productRuleManager);
        }
    }

    private Node? Find(string a_opExtId)
    {
        if (AlternateNodeSortedList.TryGetValue(a_opExtId, out Node foundNode))
        {
            return foundNode;
        }

        return null;
    }

    public bool ContainsOperation(string a_opExtId)
    {
        return AlternateNodeSortedList.ContainsKey(a_opExtId);
    }

    private Node FindByName(string a_name)
    {
        for (int i = 0; i < AlternateNodeSortedList.Count; ++i)
        {
            Node node = AlternateNodeSortedList.Values[i];
            if (node.Operation.Name == a_name)
            {
                return node;
            }
        }

        return null;
    }

    internal void AbsorbReportedValues(AlternatePath a_ap)
    {
        for (int nodeI = 0; nodeI < AlternateNodeSortedList.Count; ++nodeI)
        {
            Node node = AlternateNodeSortedList.Values[nodeI];
            InternalOperation io = (InternalOperation)node.Operation;
            Node absorbNode = a_ap.FindByName(io.Name);
            ResourceOperation absorbOp = (ResourceOperation)absorbNode.Operation;
            io.AbsorbReportedValues(absorbOp);
        }
    }

    internal bool AnyFinishedActivities()
    {
        for (int nodeI = 0; nodeI < AlternateNodeSortedList.Count; ++nodeI)
        {
            Node node = AlternateNodeSortedList.Values[nodeI];
            InternalOperation io = (InternalOperation)node.Operation;
            if (io.Activities.AnyFinishedActivities())
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Lock and/or Anchor activities that start before a time.
    /// </summary>
    internal void LockAndAnchorBefore(OptimizeSettings.ETimePoints a_startSpan, ScenarioOptions a_scenarioOptions)
    {
        for (int nodeI = 0; nodeI < AlternateNodeSortedList.Count; ++nodeI)
        {
            Node node = AlternateNodeSortedList.Values[nodeI];
            InternalOperation io = (InternalOperation)node.Operation;
                io.LockAndAnchorBefore(a_startSpan, a_scenarioOptions);
        }
    }

    #region Similarity
    internal void DetermineDifferences(AlternatePath a_ap, int a_differenceTypes, System.Text.StringBuilder a_sb)
    {
        if (m_alternateNodeSortedList.Count != a_ap.m_alternateNodeSortedList.Count)
        {
            a_sb.Append("Paths differ by the number of nodes they contain;");
            return;
        }

        HashSet<BaseId> checkedIds = new ();

        for (int nodeI = 0; nodeI < m_leaves.Count; ++nodeI)
        {
            Node node = m_leaves[nodeI];
            if (!CheckNode(node, a_ap, checkedIds, a_differenceTypes, a_sb))
            {
                return;
            }
        }
    }

    private bool CheckNode(Node a_curNode, AlternatePath a_ap, HashSet<BaseId> a_checkedIds, int a_differenceTypes, System.Text.StringBuilder a_warnings)
    {
        a_checkedIds.Add(a_curNode.Id);

        if (!a_ap.m_alternateNodeSortedList.ContainsKey(a_curNode.Operation.ExternalId))
        {
            a_warnings.Append("path 1 has an alternate path node that path 2 doesn't;");
            return false;
        }

        Node compareNode = a_ap.m_alternateNodeSortedList[a_curNode.Operation.ExternalId];

        for (int sucI = 0; sucI < a_curNode.Successors.Count; ++sucI)
        {
            Association asn = a_curNode.Successors[sucI];
            Association compareAsn = compareNode.Successors.FindBySuccessorExternalId(asn.Successor.Operation.ExternalId);
            if (compareAsn == null)
            {
                a_warnings.Append("there's a predecessor/successor relationship in path 1 that doesn't exist in path 2;");
                return false;
            }

            asn.DetermineDifferences(compareAsn, a_differenceTypes, a_warnings);
            a_curNode.Operation.DetermineDifferences(compareAsn.Predecessor.Operation, a_differenceTypes, a_warnings);
        }

        for (int sucI = 0; sucI < a_curNode.Successors.Count; ++sucI)
        {
            Association asn = a_curNode.Successors[sucI];
            if (!a_checkedIds.Contains(asn.Successor.Id))
            {
                if (!CheckNode(asn.Successor, a_ap, a_checkedIds, a_differenceTypes, a_warnings))
                {
                    return false;
                }
            }
        }

        return true;
    }
    #endregion

    internal long GetNbrOfActivitiesToSchedule()
    {
        long nbrOfActivites = 0;
        IDictionaryEnumerator etr = GetSchedulableNodeEnumerator();

        while (etr.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)etr.Current;
            Node pathNode = (Node)de.Value;
            InternalOperation op = pathNode.Operation;
            nbrOfActivites += op.GetNbrOfActivitiesToSchedule();
        }

        return nbrOfActivites;
    }

    /// <summary>
    /// Finds the maximum production status of all the activities in the path.
    /// </summary>
    /// <returns>The maximum production status of the path.</returns>
    internal InternalActivityDefs.productionStatuses GetMaxProductionStatus()
    {
        InternalActivityDefs.productionStatuses maxStatus = InternalActivityDefs.productionStatuses.Waiting;
        using IEnumerator<KeyValuePair<string, Node>> etr = AlternateNodeSortedList.GetEnumerator();

        while (etr.MoveNext())
        {
            Node pathNode = etr.Current.Value;
            InternalOperation op = pathNode.Operation;
            InternalActivityDefs.productionStatuses opStatus = op.MaxActivityProductionStatus;
            if ((int)opStatus > (int)maxStatus)
            {
                maxStatus = opStatus;
            }
        }

        return maxStatus;
    }

    /// <summary>
    /// Whether an AlternatePath node lies upstream of another node.
    /// </summary>
    /// <param name="a_upstream">The upstream node.</param>
    /// <param name="a_testDownstream">The potential downstream node.</param>
    /// <returns>true if the 2nd node is a downstream node.</returns>
    internal static bool IsDownstream(Node a_upstream, Node a_testDownstream)
    {
        for (int i = 0; i < a_upstream.Successors.Count; ++i)
        {
            Node sucessorNode = a_upstream.Successors[i].Successor;
            if (sucessorNode == a_testDownstream)
            {
                return true;
            }

            if (IsDownstream(sucessorNode, a_testDownstream))
            {
                return true;
            }
        }

        return false;
    }

    internal long FindMinJitStart(out Node o_minNode)
    {
        long minJit = long.MaxValue;
        o_minNode = null;
        for (int i = 0; i < NodeCount; ++i)
        {
            Node n = this[i];
            if (n.Operation.DbrJitStartDateTicks < minJit)
            {
                o_minNode = n;
                minJit = n.Operation.DbrJitStartDateTicks;
            }
        }

        return minJit;
    }

    /// <summary>
    /// Mark every node that another path has scheduled.
    /// This path and all nodes should not be scheduled.
    /// </summary>
    internal void RemoveFromScheduling()
    {
        foreach (Node apNode in m_alternateNodeSortedList.Values)
        {
            apNode.HasAnotherPathScheduled = true;
        }
    }
}