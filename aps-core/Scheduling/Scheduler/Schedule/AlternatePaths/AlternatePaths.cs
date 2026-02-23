using System.Collections;
using System.Text;

using PT.APSCommon;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;
//*****************************************************************************************************************************************************************************************************
// CLASS - Multiple classes are defined in this file.
//*****************************************************************************************************************************************************************************************************

/// <summary>
/// Each AlternatePath specifies one possible routing that can be followed for an MO.  Each MO has at least one AlternatePath.
/// </summary>
public partial class AlternatePath : ExternalBaseIdObject, IPTSerializable, IEnumerable<InternalOperation>
{
    #region IPTSerializable Members
    /// <summary>
    /// Create from an IReader.
    /// </summary>
    /// <param name="a_reader"></param>
    /// <param name="a_manufacturingOrder">The MO that this alternate path is for.</param>
    public AlternatePath(IReader a_reader, ManufacturingOrder a_manufacturingOrder)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12505)
        {
            m_manufacturingOrder = a_manufacturingOrder;

            a_reader.Read(out m_preference);
            a_reader.Read(out m_nextNodeId);
            a_reader.Read(out m_name);

            a_reader.Read(out int temp);
            m_autoUse = (AlternatePathDefs.AutoUsePathEnum)temp;
            a_reader.Read(out m_autoUseReleaseOffsetTimeSpanTicks);

            a_reader.Read(out DateTime validityStartDate);
            a_reader.Read(out DateTime validityEndDate);
            ValidityStartDate = validityStartDate;
            ValidityEndDate = validityEndDate;

            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out string externalId);
                Node node = new (a_reader, a_manufacturingOrder.OperationManager);
                m_alternateNodeSortedList.Add(externalId, node);
            }

            Node[] opBaseIds = new Node[m_nextNodeId + 1];
            IEnumerator<KeyValuePair<string, Node>> enumerator = m_alternateNodeSortedList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Node node = enumerator.Current.Value;
                opBaseIds[node.Id.Value] = node;
            }

            enumerator.Reset();
            while (enumerator.MoveNext())
            {
                Node node = enumerator.Current.Value;
                node.DeserializationFixups(opBaseIds);
            }

            enumerator.Reset();
            while (enumerator.MoveNext())
            {
                Node node = enumerator.Current.Value;
                node.DeserializationFixups2();
            }
        }
        else if (a_reader.VersionNumber >= 365)
        {
            m_manufacturingOrder = a_manufacturingOrder;

            a_reader.Read(out m_preference);
            a_reader.Read(out m_nextNodeId);
            a_reader.Read(out m_name);

            int temp;
            a_reader.Read(out temp);
            m_autoUse = (AlternatePathDefs.AutoUsePathEnum)temp;
            a_reader.Read(out m_autoUseReleaseOffsetTimeSpanTicks);

            int count;
            a_reader.Read(out count);

            for (int i = 0; i < count; i++)
            {
                string externalId;
                a_reader.Read(out externalId);
                Node node = new (a_reader, a_manufacturingOrder.OperationManager);
                m_alternateNodeSortedList.Add(externalId, node);
            }

            Node[] opBaseIds = new Node[m_nextNodeId + 1];
            IEnumerator<KeyValuePair<string, Node>> enumerator = m_alternateNodeSortedList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Node node = enumerator.Current.Value;
                opBaseIds[node.Id.Value] = node;
            }

            enumerator.Reset();
            while (enumerator.MoveNext())
            {
                Node node = enumerator.Current.Value;
                node.DeserializationFixups(opBaseIds);
            }

            enumerator.Reset();
            while (enumerator.MoveNext())
            {
                Node node = enumerator.Current.Value;
                node.DeserializationFixups2();
            }
        }

        // Create nodes.
        CollectLeaves();
    }

    public new const int UNIQUE_ID = 420;

    /// <summary>
    /// Serialize with a specified IWriter.
    /// </summary>
    /// <param name="writer"></param>
    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_preference);
        writer.Write(m_nextNodeId);
        writer.Write(m_name);

        writer.Write((int)m_autoUse);
        writer.Write(m_autoUseReleaseOffsetTimeSpanTicks);

        writer.Write(ValidityStartDate);
        writer.Write(ValidityEndDate);

        // Serialize the nodes.
        writer.Write(m_alternateNodeSortedList.Count);

        IEnumerator<KeyValuePair<string, Node>> enumerator = m_alternateNodeSortedList.GetEnumerator();
        while (enumerator.MoveNext())
        {
            string externalId = enumerator.Current.Key;
            writer.Write(externalId);
            Node node = enumerator.Current.Value;
            node.Serialize(writer);
        }
    }

    /// <summary>
    /// A number that uniquely defines this class.
    /// </summary>
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Construction
    /// <summary>
    /// The MO this is for.
    /// </summary>
    private readonly ManufacturingOrder m_manufacturingOrder;

    /// <summary>
    /// Create an AlternatePath with a single operation.
    /// </summary>
    /// <param name="job"></param>
    /// <param name="operation"></param>
    /// <param name="manufacturingOrder"></param>
    public AlternatePath(BaseId a_id, ResourceOperation a_operation, ManufacturingOrder a_manufacturingOrder)
        : base(a_id)
    {
        m_manufacturingOrder = a_manufacturingOrder;

        // Create a node.
        Node node = new (a_operation, GetNextNodeId(), this);
        m_alternateNodeSortedList.Add(node.Operation.ExternalId, node);

        CollectLeaves();
    }

    /// <summary>
    /// Create a new alternate path.
    /// </summary>
    /// <param name="job">The job this is for.</param>
    /// <param name="a_path">The definition of a path in JobT for.</param>
    /// <param name="operationHash">The complete set of operations in the MO this path is for.</param>
    /// <param name="manufacturingOrder">The MO this is for.</param>
    public AlternatePath(JobT.AlternatePath a_path, SortedList a_opSortedList, ManufacturingOrder a_manufacturingOrder)
        : base(a_path)
    {
        m_manufacturingOrder = a_manufacturingOrder;
        Name = a_path.Name;
        if (a_path.PreferenceIsSet)
        {
            Preference = a_path.Preference;
        }

        if (a_path.AutoUseIsSet)
        {
            AutoUse = a_path.AutoUse;
        }

        if (a_path.AutoUseReleaseOffsetTimeSpanIsSet)
        {
            AutoUseReleaseOffsetTimeSpanTicks = a_path.AutoUseReleaseOffsetTimeSpan.Ticks;
        }

        //Validate validity start dates
        if (a_path.ValidityStartDateIsSet && a_path.ValidityEndDateIsSet)
        {
            if (ValidityStartDate == PTDateTime.InvalidDateTime || a_path.ValidityEndDate <= a_path.ValidityStartDate)
            {
                throw new PTValidationException("3073");
            }
        }
        else if (a_path.ValidityStartDateIsSet && a_path.ValidityStartDate >= ValidityEndDate)
        {
            throw new PTValidationException("3073");
        }
        else if (a_path.ValidityEndDateIsSet && a_path.ValidityEndDate <= ValidityStartDate)
        {
            throw new PTValidationException("3073");
        }
        
        if (a_path.ValidityStartDateIsSet)
        {
            ValidityStartDate = PTDateTime.Max(PTDateTime.MinDateTime, a_path.ValidityStartDate);
        }

        if (a_path.ValidityEndDateIsSet)
        {
            ValidityEndDate = PTDateTime.Min(PTDateTime.MaxDateTime, a_path.ValidityEndDate);
        }

        // Create each node.
        for (int pathNodeI = 0; pathNodeI < a_path.Count; pathNodeI++)
        {
            JobT.AlternateNode jobTAlternateNode = a_path[pathNodeI];
            ResourceOperation operation = (ResourceOperation)a_opSortedList[jobTAlternateNode.OperationExternalId];

            Node node = null;
            if (m_alternateNodeSortedList.TryGetValue(operation.ExternalId, out Node value))
            {
                node = value;
            }
            else
            {
                node = new Node(operation, GetNextNodeId(), this);
                m_alternateNodeSortedList.Add(node.Operation.ExternalId, node);
            }

            // Create each successor node.
            for (int successorPathNodeI = 0; successorPathNodeI < jobTAlternateNode.Count; successorPathNodeI++)
            {
                JobT.PredecessorOperationAttributes predecessorOperationAttributes = jobTAlternateNode[successorPathNodeI];
                Node successorOperationNode;
                if (!m_alternateNodeSortedList.TryGetValue(predecessorOperationAttributes.OperationExternalId, out Node opNode))
                {
                    ResourceOperation successorOperation = (ResourceOperation)a_opSortedList[predecessorOperationAttributes.OperationExternalId];
                    successorOperationNode = new Node(successorOperation, GetNextNodeId(), this);
                    m_alternateNodeSortedList.Add(successorOperationNode.Operation.ExternalId, successorOperationNode);
                }
                else
                {
                    successorOperationNode = opNode;
                }

                // Create an association between the node and its sucessor.
                Association association = new (node, successorOperationNode, predecessorOperationAttributes);
                node.AddSuccessor(association);
                successorOperationNode.AddPredecessor(association);
            }
        }

        CollectLeaves();
    }

    public AlternatePath(BaseId a_id, AlternatePath a_sourcePath, ManufacturingOrder a_mo)
        : base(a_id, a_sourcePath)
    {
        m_manufacturingOrder = a_mo;
        Name = a_sourcePath.Name;
        Preference = a_sourcePath.Preference;
        AutoUse = a_sourcePath.AutoUse;

        //Object Id is already being generated outside the constructor and passed down the base constructor
        //Id = sourcePath.Id;

        // Create a copy of each node
        IEnumerator<KeyValuePair<string, Node>> ienum = a_sourcePath.AlternateNodeSortedList.GetEnumerator();
        while (ienum.MoveNext())
        {
            Node sourceNode = ienum.Current.Value;
            ResourceOperation operation = (ResourceOperation)a_mo.OperationManager[sourceNode.Operation.ExternalId];
            Node newNode = new (operation, GetNextNodeId(), this);
            m_alternateNodeSortedList.Add(newNode.Operation.ExternalId, newNode);
        }

        //Now create the Associations
        //Start at the leaves, adding them then their successors.
        a_sourcePath.CollectLeaves();
        Hashtable addedOpIds = new (); //keeps track of which Operations' Associations have been added already
        for (int i = 0; i < a_sourcePath.Leaves.Count; i++)
        {
            Node sourceNode = a_sourcePath.Leaves[i];
            Node newNode = m_alternateNodeSortedList[sourceNode.Operation.ExternalId];
            AddAssociations(addedOpIds, sourceNode, newNode, a_sourcePath);
        }

        CollectLeaves();
    }

    private void AddAssociations(Hashtable a_addedOpIds, Node a_sourceNode, Node a_newNode, AlternatePath a_sourcePath)
    {
        //Create Successor Associations for the Node
        for (int i = 0; i < a_sourceNode.Successors.Count; i++)
        {
            Association sourceAssociation = a_sourceNode.Successors[i];
            Node sucNode = AlternateNodeSortedList[sourceAssociation.Successor.Operation.ExternalId];
            Node predNode = a_newNode;
            Association newAssociation = new (predNode, sucNode, sourceAssociation);

            a_newNode.Successors.Add(newAssociation);

            sucNode.Predecessors.Add(newAssociation);
        }

        a_addedOpIds.Add(a_newNode.Operation.Id, null); //store the node's id so its assocations are not added again.

        //Recursively have each Successor add its associations if the successor has not already done so
        for (int i = 0; i < a_sourceNode.Successors.Count; i++)
        {
            Association sourceAssociation = a_sourceNode.Successors[i];
            Node sucNode = AlternateNodeSortedList[sourceAssociation.Successor.Operation.ExternalId];
            if (!a_addedOpIds.Contains(sucNode.Operation.Id))
            {
                Node nextSourceNode = a_sourcePath.AlternateNodeSortedList[sucNode.Operation.ExternalId];
                AddAssociations(a_addedOpIds, nextSourceNode, sucNode, a_sourcePath);
            }
        }
    }

    /// <summary>
    /// Determine the number of nodes in a path.
    /// </summary>
    /// <param name="path">Path whose nodes you want counted.</param>
    /// <returns>The number of nodes in the path.</returns>
    private int OperationCount(JobT.AlternatePath a_path)
    {
        Hashtable t = new ();
        for (int i = 0; i < a_path.Count; ++i)
        {
            JobT.AlternateNode node = a_path[i];

            if (!t.Contains(node.OperationExternalId))
            {
                t.Add(node.OperationExternalId, null);
            }

            // Final operations are not included in the count returned by path.Count so we must count them here.
            for (int j = 0; j < node.Count; ++j)
            {
                string operationExternalId = node[j].OperationExternalId;
                if (!t.Contains(operationExternalId))
                {
                    t.Add(operationExternalId, null);
                }
            }
        }

        return t.Count;
    }

    /// <summary>
    /// Used to create unique ids for nodes.
    /// </summary>
    private long m_nextNodeId = -1;

    /// <summary>
    /// Obtain the next unique id for a node; unique within this alternate path.
    /// </summary>
    /// <returns>A unique node id.</returns>
    private BaseId GetNextNodeId()
    {
        ++m_nextNodeId;
        return new BaseId(m_nextNodeId);
    }
    #endregion

    #region Shared Properties
    private string m_name = "";

    /// <summary>
    /// Identifies the AlternatePath.
    /// </summary>
    [System.ComponentModel.ParenthesizePropertyName(true)]
    public string Name
    {
        get => m_name;
        private set => m_name = value;
    }

    private int m_preference;

    /// <summary>
    /// This values can be used in different way by custom algorithms and serves as a visual indicator to the planner.
    /// </summary>
    [System.ComponentModel.ParenthesizePropertyName(true)]
    public int Preference
    {
        get => m_preference;
        private set => m_preference = value;
    }

    private AlternatePathDefs.AutoUsePathEnum m_autoUse = AlternatePathDefs.AutoUsePathEnum.IfCurrent;

    public AlternatePathDefs.AutoUsePathEnum AutoUse
    {
        get => m_autoUse;
        internal set => m_autoUse = value; //Set to internal for InsertJobs
    }

    private long m_autoUseReleaseOffsetTimeSpanTicks;

    /// <summary>
    /// A TimeSpan that defines when the path becomes eligible for automatic selection. The Alternate Path will not be used before the default path's release date + AutoPathSelectionReleaseOffset. For
    /// instance if a ManufacturingOrder has 2 alternate paths and the Default Path's release date is January 1 and the second AlternatePath is setup for AutoPathSection with AutoPathSelectionReleaseOffset=1
    /// day. Then the second path could potentially be used on or after January 2nd. The path that ends up being selected will depend on your optimization rules and resource availability. This value isn't
    /// used to determine the release date of the Default Path.
    /// </summary>
    internal long AutoUseReleaseOffsetTimeSpanTicks
    {
        get => m_autoUseReleaseOffsetTimeSpanTicks;
        private set => m_autoUseReleaseOffsetTimeSpanTicks = value;
    }

    /// <summary>
    /// A TimeSpan that defines when the path becomes eligible for automatic selection. The Alternate Path will not be used before the default path's release date + AutoPathSelectionReleaseOffset. For
    /// instance if a ManufacturingOrder has 2 alternate paths and the Default Path's release date is January 1 and the second AlternatePath is setup for AutoPathSection with AutoPathSelectionReleaseOffset=1
    /// day. Then the second path could potentially be used on or after January 2nd. The path that ends up being selected will depend on your optimization rules and resource availability. This value isn't
    /// used to determine the release date of the Default Path.
    /// </summary>
    public TimeSpan AutoUseReleaseOffsetTimeSpan => new (m_autoUseReleaseOffsetTimeSpanTicks);

    public DateTime ValidityStartDate { get; internal set; } = PTDateTime.MinDateTime;
    public DateTime ValidityEndDate { get; internal set; } = PTDateTime.MaxDateTime;

    #endregion SharedProperties

    #region Client/Interface related
    /// <summary>
    /// Polulates the job dataset
    /// </summary>
    /// <param name="a_moRow"></param>
    /// <param name="r_dataSet"></param>
    /// <param name="a_mo"></param>
    internal void PopulateJobDataSet(JobDataSet.ManufacturingOrderRow a_moRow, ref JobDataSet r_dataSet, ManufacturingOrder a_mo)
    {
        //Set sort to Operation Id
        r_dataSet.AlternatePath.DefaultView.Sort = string.Format("{0} ASC", r_dataSet.AlternatePath.NameColumn.ColumnName);

        r_dataSet.AlternatePath.AddAlternatePathRow(
            a_mo.Job.ExternalId,
            a_moRow.ExternalId,
            ExternalId,
            Name,
            Preference,
            false,
            AutoUse.ToString(),
            AutoUseReleaseOffsetTimeSpan.TotalDays,
            ValidityStartDate.ToDisplayTime().ToDateTime(),
            ValidityEndDate.ToDisplayTime().ToDateTime(),
            GetLeadEligibleResources()
        );

        //Add Node rows
        //Set sort to Operation Id
        r_dataSet.AlternatePathNode.DefaultView.Sort = string.Format("{0} ASC", r_dataSet.AlternatePathNode.PredecessorOperationExternalIdColumn.ColumnName);

        IEnumerator<KeyValuePair<string, Node>> enumerator = AlternateNodeSortedList.GetEnumerator();
        while (enumerator.MoveNext())
        {
            Node node = enumerator.Current.Value;

            if (node.Successors.Count > 0)
            {
                //Add one row for each pred/suc relationship.
                for (int x = 0; x < node.Successors.Count; x++)
                {
                    JobDataSet.AlternatePathNodeRow nodeRow = r_dataSet.AlternatePathNode.NewAlternatePathNodeRow();
                    nodeRow.JobExternalId = a_mo.Job.ExternalId;
                    nodeRow.MoExternalId = a_mo.ExternalId;
                    nodeRow.PathExternalId = ExternalId;
                    nodeRow.Id = node.Id.ToBaseType();
                    nodeRow.PredecessorOperationExternalId = node.Operation.ExternalId;
                    nodeRow.OperationName = node.Operation.Name;
                    nodeRow.OperationDescription = node.Operation.Description;
                    nodeRow.SuccessorOperationExternalId = node.Successors[x].Successor.Operation.ExternalId;
                    nodeRow.SuccessorName = node.Successors[x].Successor.Operation.Name;
                    nodeRow.SuccessorDescription = node.Successors[x].Successor.Operation.Description;
                    Association predAtts = node.Successors[x];
                    if (predAtts != null)
                    {
                        predAtts.PopulateJobDataSet(nodeRow);
                    }
                    else //fill with defaults
                    {
                        Association.PopulateJobDataSetDefaults(nodeRow);
                    }

                    r_dataSet.AlternatePathNode.AddAlternatePathNodeRow(nodeRow);
                }
            }
            else
            {
                //Add a single row for the Node.
                JobDataSet.AlternatePathNodeRow nodeRow = r_dataSet.AlternatePathNode.NewAlternatePathNodeRow();
                nodeRow.JobExternalId = a_mo.Job.ExternalId;
                nodeRow.MoExternalId = a_mo.ExternalId;
                nodeRow.PathExternalId = ExternalId;
                nodeRow.Id = node.Id.ToBaseType();
                nodeRow.PredecessorOperationExternalId = node.Operation.ExternalId;
                nodeRow.OperationName = node.Operation.Name;
                nodeRow.OperationDescription = node.Operation.Description;
                nodeRow.SuccessorOperationExternalId = ""; //This value is a key in the row so nulls are not allowed.

                //Fill successor values with defaults to avoid nulls which cause problems.
                Association.PopulateJobDataSetDefaults(nodeRow);
                r_dataSet.AlternatePathNode.AddAlternatePathNodeRow(nodeRow);
            }
        }
    }
    #endregion

    #region AlternateNodeHash
    /// <summary>
    /// The set of nodes that define this alternate path. Keys are the node's associated operation external id.
    /// </summary>
    private readonly SortedList<string, Node> m_alternateNodeSortedList = new ();

    /// <summary>
    /// The set of nodes that define this alternate path. Keys are the node's associated operation external id.
    /// </summary>
    public SortedList<string, Node> AlternateNodeSortedList => m_alternateNodeSortedList;

    public Node GetNodeByIndex(int a_index)
    {
        return m_alternateNodeSortedList.Values[a_index];
    }
    #endregion

    #region Operation status
    internal enum RoutingChanges : ulong { Unchanged = 0, ActivitiesFinished = 1 }

    internal bool UpdateOperationStatuses(bool a_erpUpdate, AlternatePath a_updatedPath, ScenarioOptions a_options, JobT.ManufacturingOrder a_jobTMO, IScenarioDataChanges a_dataChanges)
    {
        IEnumerator<KeyValuePair<string, Node>> alternateNodesEnumerator = m_alternateNodeSortedList.GetEnumerator();
        bool updated = false;
        while (alternateNodesEnumerator.MoveNext())
        {
            Node node = alternateNodesEnumerator.Current.Value;
            Node updatedNode = a_updatedPath.AlternateNodeSortedList[alternateNodesEnumerator.Current.Key];
            JobT.BaseOperation jobTOp = a_jobTMO.GetOperation(node.Operation.ExternalId);
            updated |= node.Operation.UpdateStatus(a_erpUpdate, updatedNode.Operation, a_options, jobTOp, a_dataChanges);
        }

        DoAnyAutoPredecessorFinishes();
        return updated;
    }

    /// <summary>
    /// Update the Reported Hours/Qty for Operations that are set to AutoReport Progress.
    /// </summary>
    internal void AdvancingClock(TimeSpan a_clockAdvancedBy, DateTime a_newClock, bool a_autoFinishAllActivities, bool a_autoReportProgressOnAllActivities)
    {
        IEnumerator<KeyValuePair<string, Node>> alternateNodesEnumerator = m_alternateNodeSortedList.GetEnumerator();

        while (alternateNodesEnumerator.MoveNext())
        {
            Node node = alternateNodesEnumerator.Current.Value;
            node.Operation.AdvancingClock(a_clockAdvancedBy, a_newClock, a_autoFinishAllActivities, a_autoReportProgressOnAllActivities);
        }

        if (a_autoFinishAllActivities || a_autoReportProgressOnAllActivities)
        {
            DoAnyAutoPredecessorFinishes();
        }
    }

    /// <summary>
    /// Iterate through the nodes.
    /// If the node has AutoFinishPred in use and a predecessor needs finishing then finish it and reiterate through the nodes again.
    /// </summary>
    internal void DoAnyAutoPredecessorFinishes()
    {
        IEnumerator<KeyValuePair<string, Node>> alternateNodesEnumerator = m_alternateNodeSortedList.GetEnumerator();

        bool autoFinishedSomething = false;
        while (alternateNodesEnumerator.MoveNext())
        {
            Node node = alternateNodesEnumerator.Current.Value;

            for (int i = 0; i < node.Successors.Count; i++)
            {
                Association association = node.Successors[i];
                BaseOperation predOp = association.Predecessor.Operation;
                BaseOperation sucOp = association.Successor.Operation;
                if (association.AutoFinishPredecessor != InternalOperationDefs.autoFinishPredecessorOptions.NoAutoFinish && !predOp.Finished)
                {
                    string commentsToLog = string.Format("Auto-finished when successor {0} was finished (due to AlternatePathNode AutoFinishPredecessor option).".Localize(), sucOp.Name);
                    if (association.AutoFinishPredecessor == InternalOperationDefs.autoFinishPredecessorOptions.OnSuccessorFinish && (int)GetMaxAutoPredFinishState(sucOp) >= (int)autoPredFinishOpMaxStates.Finished)
                    {
                        predOp.AutoFinishAllActivities(commentsToLog);
                        autoFinishedSomething = true;
                        break;
                    }

                    if (association.AutoFinishPredecessor == InternalOperationDefs.autoFinishPredecessorOptions.OnSuccessorPostProcessingStart && (int)GetMaxAutoPredFinishState(sucOp) >= (int)autoPredFinishOpMaxStates.PostProcessingStarted)
                    {
                        predOp.AutoFinishAllActivities(commentsToLog);
                        autoFinishedSomething = true;
                        break;
                    }

                    if (association.AutoFinishPredecessor == InternalOperationDefs.autoFinishPredecessorOptions.OnSuccessorRunStart && (int)GetMaxAutoPredFinishState(sucOp) >= (int)autoPredFinishOpMaxStates.RunStarted)
                    {
                        predOp.AutoFinishAllActivities(commentsToLog);
                        autoFinishedSomething = true;
                        break;
                    }

                    if (association.AutoFinishPredecessor == InternalOperationDefs.autoFinishPredecessorOptions.OnSuccessorSetupStart && (int)GetMaxAutoPredFinishState(sucOp) >= (int)autoPredFinishOpMaxStates.SetupStarted)
                    {
                        predOp.AutoFinishAllActivities(commentsToLog);
                        autoFinishedSomething = true;
                        break;
                    }
                }
            }
        }

        if (autoFinishedSomething) //run through this process again to do any newly necessary autoPredFinishes
        {
            DoAnyAutoPredecessorFinishes();
        }
    }

    private enum autoPredFinishOpMaxStates
    {
        Unstarted,
        SetupStarted,
        RunStarted,
        PostProcessingStarted,
        Finished
    }

    private autoPredFinishOpMaxStates GetMaxAutoPredFinishState(BaseOperation a_op)
    {
        if (a_op.Finished)
        {
            return autoPredFinishOpMaxStates.Finished;
        }

        if (a_op is InternalOperation)
        {
            InternalOperation iOp = (InternalOperation)a_op;
            InternalActivityDefs.productionStatuses opMaxActivityProductionStatus = iOp.MaxActivityProductionStatus;
            if (opMaxActivityProductionStatus == InternalActivityDefs.productionStatuses.PostProcessing || iOp.ReportedPostProcessingHours > 0)
            {
                return autoPredFinishOpMaxStates.PostProcessingStarted;
            }

            if (opMaxActivityProductionStatus == InternalActivityDefs.productionStatuses.Running || iOp.ReportedRunHours > 0 || iOp.ReportedGoodQty > 0 || iOp.ReportedScrapQty > 0)
            {
                return autoPredFinishOpMaxStates.RunStarted;
            }

            if (opMaxActivityProductionStatus == InternalActivityDefs.productionStatuses.SettingUp || iOp.ReportedSetupHours > 0)
            {
                return autoPredFinishOpMaxStates.SetupStarted;
            }

            return autoPredFinishOpMaxStates.Unstarted;
        }

        return autoPredFinishOpMaxStates.Unstarted;
    }
    #endregion

    #region Operation functions.
    /// <summary>
    /// Obtain the latest Finish among the operations in this path.
    /// You should only call this method against the current path.
    /// </summary>
    internal long ScheduledFinish
    {
        get
        {
            long scheduledEnd = PTDateTime.MinDateTime.Ticks;
            IEnumerator<KeyValuePair<string, Node>> alternateNodesEnumerator = m_alternateNodeSortedList.GetEnumerator();

            while (alternateNodesEnumerator.MoveNext())
            {
                Node node = alternateNodesEnumerator.Current.Value;
                BaseOperation baseOperation = node.Operation;
                if (baseOperation is InternalOperation)
                {
                    InternalOperation op = (InternalOperation)baseOperation;
                    //TODO: Do we want to include cleanout here?
                    if (op.GetScheduledFinishDate(out long opScheduledFinishDate, true))
                    {
                        if (opScheduledFinishDate > scheduledEnd)
                        {
                            scheduledEnd = opScheduledFinishDate;
                        }
                    }
                }
            }

            return scheduledEnd;
        }
    }

    /// <summary>
    /// Whether all the operations in this path have been finished.
    /// This only makes sense to a ManufacturingOrder if this is the current path.
    /// </summary>
    internal bool Finished => GetReportedFinishDate(out long reportedFinishDateTemp);

    /// <summary>
    /// Get the reported finish date if the operation is finished.
    /// </summary>
    /// <param name="aReportedFinishDate">The maximum reported finish date of activity.</param>
    /// <returns>Whether the operation is finished.</returns>
    internal bool GetReportedFinishDate(out long o_reportedFinishDate)
    {
        o_reportedFinishDate = PTDateTime.InvalidDateTime.Ticks;
        IEnumerator<KeyValuePair<string, Node>> alternateNodesEnumerator = m_alternateNodeSortedList.GetEnumerator();

        while (alternateNodesEnumerator.MoveNext())
        {
            Node node = alternateNodesEnumerator.Current.Value;
            InternalOperation baseOperation = node.Operation;
            if (baseOperation.IsNotOmitted)
            {
                if (baseOperation.GetReportedFinishDate(out long finishedDate))
                {
                    if (finishedDate > o_reportedFinishDate)
                    {
                        o_reportedFinishDate = finishedDate;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        return o_reportedFinishDate != PTDateTime.InvalidDateTime.Ticks;
    }
    /// <summary>
    /// Get the reported start date if the operation is finished.
    /// </summary>
    /// <param name="o_reportedStartDate">The earliest reported start date of activity.</param>
    /// <returns>Whether the operation is finished.</returns>
    internal bool GetReportedStartDate(out long o_reportedStartDate)
    {
        o_reportedStartDate = PTDateTime.MaxDateTimeTicks;
        IEnumerator<KeyValuePair<string, Node>> alternateNodesEnumerator = m_alternateNodeSortedList.GetEnumerator();

        while (alternateNodesEnumerator.MoveNext())
        {
            Node node = alternateNodesEnumerator.Current.Value;
            InternalOperation internalOperation = node.Operation;
            if (internalOperation.IsNotOmitted)
            {
                long startDate;

                if (internalOperation.GetReportedStartDate(out startDate))
                {
                    if (startDate < o_reportedStartDate)
                    {
                        o_reportedStartDate = startDate;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        return o_reportedStartDate != PTDateTime.MaxDateTimeTicks;
    }

    /// <summary>
    /// True if at least one block is Anchored.
    /// </summary>
    public bool IsAnchored()
    {
        IDictionaryEnumerator etr = GetSchedulableNodeEnumerator();
        while (etr.MoveNext())
        {
            Node node = (Node)etr.Value;
            InternalOperation io = (InternalOperation)node.Operation;
            if (io.Anchored != anchoredTypes.Free)
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Path Comparison
    /// <summary>
    /// Determine whether the structure of this path is the same as the structure of a given path.
    /// </summary>
    /// <param name="path2">The path you want to compare this path to.</param>
    /// <returns>Whether the two paths have an identicle structure.</returns>
    internal void IdenticalPathStructure(AlternatePath a_path2, PT.Scheduler.RoutingChanges a_analyzer)
    {
        if (m_alternateNodeSortedList.Count != a_path2.AlternateNodeSortedList.Count)
        {
            a_analyzer.AlternatePathChanged = true;
            for (int i = 0; i < NodeCount; i++)
            {
                if (this[i].Operation.Scheduled)
                {
                    a_analyzer.ScheduledRoutingChanged = true;
                    a_analyzer.AddDescription(string.Format("Scheduled Operation {0} was removed".Localize(), this[i].Operation.ExternalId));
                    return;
                }
            }
        }

        using IEnumerator<KeyValuePair<string, Node>> enumerator = m_alternateNodeSortedList.GetEnumerator();

        while (enumerator.MoveNext())
        {
            string externalId = enumerator.Current.Key;
            Node node = enumerator.Current.Value;

            if (!a_path2.AlternateNodeSortedList.TryGetValue(externalId, out Node path2Node))
            {
                a_analyzer.AlternatePathChanged = true;

                if (node.Operation.Scheduled)
                {
                    a_analyzer.ScheduledRoutingChanged = true;
                    a_analyzer.AddDescription("Scheduled Operation " + node.Operation.ExternalId + " was removed");
                }
            }
            else
            {
                if (!node.IdenticleSuccessorSet(path2Node))
                {
                    a_analyzer.AlternatePathChanged = true;

                    if (node.Operation.Scheduled)
                    {
                        a_analyzer.ScheduledRoutingChanged = true;
                        a_analyzer.AddDescription("Scheduled Operation " + node.Operation.ExternalId + " has new successors");
                        continue;
                    }
                }

                InternalOperation op = (InternalOperation)node.Operation;
                InternalActivityManager iam = op.Activities;

                InternalOperation op2 = (InternalOperation)path2Node.Operation;
                InternalActivityManager iam2 = op2.Activities;

                if (!op.ProductionInfo.OnlyAllowManualUpdatesToSplitOperation)
                {
                    if (iam2.Count == 0)
                    {
                        //If there are no activities here it means there were no activity updates in the import.
                    }
                    else if (iam.Count != iam2.Count)
                    {
                        a_analyzer.AddDescription("There was a difference in Activity counts");
                        a_analyzer.AlternatePathChanged = true;
                        if (op.Scheduled)
                        {
                            a_analyzer.ScheduledRoutingChanged = true;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < iam2.Count; ++i)
                        {
                            InternalActivity ia2 = iam2.GetByIndex(i);
                            if (iam.GetByExternalId(ia2.ExternalId) == null)
                            {
                                a_analyzer.AddDescription("Scheduled Activity in operation " + ia2.Operation.ExternalId + " was added");
                                a_analyzer.ScheduledRoutingChanged = op.Scheduled;
                                a_analyzer.AlternatePathChanged = true;
                            }
                        }

                        for (int i = 0; i < iam.Count; i++)
                        {
                            InternalActivity ia1 = iam.GetByIndex(i);
                            if (iam2.GetByExternalId(ia1.ExternalId) == null)
                            {
                                a_analyzer.AddDescription("Scheduled Activity in operation " + ia1.Operation.ExternalId + " was removed");
                                a_analyzer.ScheduledRoutingChanged = op.Scheduled;
                                a_analyzer.AlternatePathChanged = true;
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region Update
    /// <summary>
    /// Checks each node to validate that all nodes exist in both paths
    /// </summary>
    /// <returns>If the path can be updated with the new path</returns>
    internal bool ValidateUpdate(AlternatePath a_ap)
    {
        using IEnumerator<KeyValuePair<string, Node>> apEnum = a_ap.AlternateNodeSortedList.GetEnumerator();

        while (apEnum.MoveNext())
        {
            Node updatedNode = apEnum.Current.Value;
            if (!AlternateNodeSortedList.TryGetValue(updatedNode.Operation.ExternalId, out Node existingNode))
            {
                return false;
            }

            if (updatedNode.Successors.Count != existingNode.Successors.Count)
            {
                return false;
            }

            for (int i = 0; i < existingNode.Successors.Count; i++)
            {
                Association existingNodeSuccessor = existingNode.Successors[i];
                Association updateNodeSuccessor = updatedNode.Successors[i];
                if (existingNodeSuccessor.Successor == null && updateNodeSuccessor.Successor == null)
                {
                    continue;
                }

                if ((existingNodeSuccessor.Successor == null && updateNodeSuccessor.Successor != null) || (existingNodeSuccessor.Successor != null && updateNodeSuccessor.Successor == null))
                {
                    return false;
                }

                if (existingNodeSuccessor.Successor.Operation.ExternalId != updateNodeSuccessor.Successor.Operation.ExternalId)
                {
                    return false;
                }

                if (!existingNodeSuccessor.Successor.IdenticleSuccessorSet(updateNodeSuccessor.Successor))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Update the alternate paths.
    /// Only properties of paths are updated at the moment. New paths aren't added, old ones aren't deleted, and structures aren't updated.
    /// No structural changes are made. All path nodes remain the same and have the same predecessor and successor nodes.
    /// </summary>
    /// <param name="a_ap">The values in this alternate path will be used to update nodes and associations.</param>
    /// <param name="a_dataChanges"></param>
    /// <returns>Whether any significant updates were made.</returns>
    internal bool Update(AlternatePath a_ap, IScenarioDataChanges a_dataChanges)
    {
        base.Update(a_ap);
        bool updates = false;

        if (Preference != a_ap.Preference)
        {
            Preference = a_ap.Preference;
            updates = true;
        }

        if (AutoUse != a_ap.AutoUse)
        {
            AutoUse = a_ap.AutoUse;
            updates = true;
        }

        if (AutoUseReleaseOffsetTimeSpan != a_ap.AutoUseReleaseOffsetTimeSpan)
        {
            AutoUseReleaseOffsetTimeSpanTicks = a_ap.AutoUseReleaseOffsetTimeSpan.Ticks;
            updates = true;
        }

        Name = a_ap.Name;

        if (ValidityStartDate != a_ap.ValidityStartDate)
        {
            ValidityStartDate = a_ap.ValidityStartDate;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (ValidityEndDate != a_ap.ValidityEndDate)
        {
            ValidityEndDate = a_ap.ValidityEndDate;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        using IEnumerator<KeyValuePair<string, Node>> apEnum = a_ap.AlternateNodeSortedList.GetEnumerator();

        while (apEnum.MoveNext())
        {
            Node updatedNode = apEnum.Current.Value;
            if (AlternateNodeSortedList.TryGetValue(updatedNode.Operation.ExternalId, out Node existingNode))
            {
                if (existingNode.Update(updatedNode))
                {
                    updates = true;
                }
            }
        }

        // The line below is commented out because there's no need to CollectLeaves here since the stucture of paths aren't updated. If the structure changes, the entire path is replaced.
        // CollectLeaves();

        return updates;
    }
    #endregion

    public override string ToString()
    {
        StringBuilder sb = new ();

        if (Linear())
        {
            if (Leaves.Count > 0)
            {
                sb.AppendFormat("PathName '{0}'; Linear; Operations=".Localize(), Name);
                Node node = Leaves[0];
                do
                {
                    sb.AppendFormat("{0};", node.Operation.Name);
                    if (node.Successors.Count == 1)
                    {
                        node = node.Successors[0].Successor;
                    }
                    else
                    {
                        node = null;
                    }
                } while (node != null);
            }
            else
            {
                sb.AppendFormat("PathName '{0}'; has no operations.".Localize(), Name);
            }
        }
        else
        {
            sb.AppendFormat("PathName '{0}'; NonLinear; LeafCount={1}".Localize(), Name, Leaves.Count);
        }

        return sb.ToString();
    }

    #region GetByOperationLevel and helpers
    /// <summary>
    /// Return the list of not finished/not omitted operationssorted by level earliest to last and and sub-sorted by operation BaseId.
    /// </summary>
    /// <param name="a_schedulableOnly">True to exclude operations that won't be scheduled.</param>
    /// <returns></returns>
    internal List<ResourceOperation> GetOperationsByLevel(bool a_schedulableOnly)
    {
        List<Pair<Node, int>> l = GetNodesByLevel(a_schedulableOnly);
        List<ResourceOperation> opList = new ();

        for (int nodeI = 0; nodeI < l.Count; ++nodeI)
        {
            opList.Add((ResourceOperation)l[nodeI].value1.Operation);
        }

        return opList;
    }

    /// <summary>
    /// Nodes are ordered lowest level (earlier operations) to highest level (last operation) to.
    /// For instance the path 10->20->30 would be returned in reverse order.
    /// </summary>
    /// <param name="a_schedulableOnly">True to exclude operations that won't be scheduled.</param>
    /// <returns></returns>
    internal List<Pair<Node, int>> GetNodesByLevel(bool a_schedulableOnly)
    {
        // Find Levels
        Dictionary<Node, int> addedNodes = new ();
        NodeCollection nodes = GetRoots();

        for (int nodeI = 0; nodeI < nodes.Count; ++nodeI)
        {
            Node node = nodes[nodeI];
            AddNodeAndPredecessors(node, 0, addedNodes);
        }

        // Sort by levels; sub-sort by BaseId or operation.
        List<Pair<Node, int>> nodeLevels = new ();
        Dictionary<Node, int>.Enumerator addedNodesIt = addedNodes.GetEnumerator();
        while (addedNodesIt.MoveNext())
        {
            Node node = addedNodesIt.Current.Key;
            if (!a_schedulableOnly || !node.Operation.IsFinishedOrOmitted)
            {
                int level = addedNodesIt.Current.Value;
                nodeLevels.Add(new Pair<Node, int>(node, level));
            }
        }

        nodeLevels.Sort(new NodeLevelComparer());

        return nodeLevels;
    }

    /// <summary>
    /// Search for the product of the most successive operation with a produce within the path
    /// </summary>
    /// <returns></returns>
    internal Product GetPrimaryProductWithinPath(out InternalOperation o_op)
    {
        // Find Levels
        Dictionary<Node, int> addedNodes = new ();
        NodeCollection nodes = GetRoots();

        for (int nodeI = 0; nodeI < nodes.Count; ++nodeI)
        {
            Node node = nodes[nodeI];
            Product p = FindPredecessorNodeProduct(node, 0, addedNodes, out o_op);
            if (p != null)
            {
                return p;
            }
        }

        o_op = null;
        return null;
    }

    /// <summary>
    /// Recusively searches predecessor nodes within the same MO to find an operation with a Product.
    /// </summary>
    /// <returns></returns>
    private Product FindPredecessorNodeProduct(Node a_node, int a_level, Dictionary<Node, int> a_addedNodes, out InternalOperation o_op)
    {
        if (a_node.Operation.Products.Count > 0)
        {
            o_op = a_node.Operation;
            return o_op.Products[0];
        }

        int level;
        if (a_addedNodes.TryGetValue(a_node, out level))
        {
            if (a_level > level)
            {
                a_addedNodes[a_node] = a_level;
            }
        }
        else
        {
            a_addedNodes.Add(a_node, a_level);
        }

        int nextLevel = a_level + 1;

        for (int predI = 0; predI < a_node.Predecessors.Count; ++predI)
        {
            Node predNode = a_node.Predecessors[predI].Predecessor;
            if (a_node.Operation.ManufacturingOrder.Id != predNode.Operation.ManufacturingOrder.Id)
            {
                //don't look in predecessor MOs
                continue;
            }

            Product p = FindPredecessorNodeProduct(predNode, nextLevel, a_addedNodes, out o_op);
            if (p != null)
            {
                return p;
            }
        }

        o_op = null;
        return null;
    }

    private void AddNodeAndPredecessors(Node a_node, int a_level, Dictionary<Node, int> a_addedNodes)
    {
        int level;
        if (a_addedNodes.TryGetValue(a_node, out level))
        {
            if (a_level > level)
            {
                a_addedNodes[a_node] = a_level;
            }
        }
        else
        {
            a_addedNodes.Add(a_node, a_level);
        }

        int nextLevel = a_level + 1;

        for (int predI = 0; predI < a_node.Predecessors.Count; ++predI)
        {
            Node predNode = a_node.Predecessors[predI].Predecessor;
            AddNodeAndPredecessors(predNode, nextLevel, a_addedNodes);
        }
    }

    private class NodeLevelComparer : IComparer<Pair<Node, int>>
    {
        #region IComparer<Pair<ResourceOperation,int>> Members
        public int Compare(Pair<Node, int> x, Pair<Node, int> y)
        {
            if (x.value2 > y.value2)
            {
                return 1;
            }

            if (x.value2 < y.value2)
            {
                return -1;
            }

            int temp = BaseId.Compare(x.value1.Id, y.value1.Id);
            return temp;
        }
        #endregion
    }

    /// <summary>
    /// Get a list of products belonging to each Operation in this Path.
    /// </summary>
    /// <returns>List of Products associated with Operations in this Path</returns>
    internal List<Product> GetProducts()
    {
        List<Product> products = new ();

        foreach (Node node in AlternateNodeSortedList.Values)
        {
            products.AddRange(node.Operation.Products.ReadOnlyList);
        }

        return products;
    }

    /// <summary>
    /// Returns the first Product (by index) on the first Root Node (or Root Node Predecessor) that has a Product.
    /// Returns null if no such Product can be found.
    /// </summary>
    /// <returns></returns>
    public Product GetPrimaryProduct()
    {
        Product primaryProduct = GetPrimaryProductWithinPath(out InternalOperation op);
        return primaryProduct;
    }

    public Tuple<InternalOperation, Product> GetPrimaryProductAndItsOp()
    {
        Product primaryProduct = GetPrimaryProductWithinPath(out InternalOperation op);
        return new Tuple<InternalOperation, Product>(op, primaryProduct);
    }
    #endregion

    /// <summary>
    /// Returns the first Product found for the Item or null if not found.
    /// </summary>
    internal Product GetProductProducingItem(Item a_item)
    {
        for (int nodeI = 0; nodeI < AlternateNodeSortedList.Count; nodeI++)
        {
            Node node = AlternateNodeSortedList.Values[nodeI];
            for (int prodI = 0; prodI < node.Operation.Products.Count; prodI++)
            {
                Product product = node.Operation.Products[prodI];
                if (product.Item.Id == a_item.Id)
                {
                    return product;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Returns true if any of the operations use TimeBasedReporting.
    /// </summary>
    /// <returns></returns>
    internal bool HasTimeBasedReportingOperations()
    {
        using IEnumerator<KeyValuePair<string, Node>> alternateNodesEnumerator = AlternateNodeSortedList.GetEnumerator();

        while (alternateNodesEnumerator.MoveNext())
        {
            Node node = alternateNodesEnumerator.Current.Value;
            BaseOperation baseOperation = node.Operation;

            if (baseOperation is InternalOperation iOp)
            {
                if (iOp.TimeBasedReporting)
                {
                    return true;
                }
            }
        }

        return false;
    }

    #region Enumerator
    internal class Enumerator : IEnumerator<InternalOperation>, IEnumerator
    {
        private readonly SortedList<string, Node> m_alternateNodeSortedList;
        private readonly IEnumerator<KeyValuePair<string, Node>> m_alternateNodesEnumerator;

        internal Enumerator(SortedList<string, Node> a_alternateNodeSortedList)
        {
            m_alternateNodeSortedList = a_alternateNodeSortedList;
            m_alternateNodesEnumerator = m_alternateNodeSortedList.GetEnumerator();
        }

        public InternalOperation Current
        {
            get
            {
                Node node = m_alternateNodesEnumerator.Current.Value;
                InternalOperation op = (InternalOperation)node.Operation;
                return op;
            }
        }

        public void Dispose() { }

        object IEnumerator.Current => this;

        public bool MoveNext()
        {
            return m_alternateNodesEnumerator.MoveNext();
        }

        public void Reset()
        {
            m_alternateNodesEnumerator.Reset();
        }
    }
    #endregion

    #region IEnumerable<InternalOperation>
    IEnumerator<InternalOperation> IEnumerable<InternalOperation>.GetEnumerator()
    {
        return new Enumerator(m_alternateNodeSortedList);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<InternalOperation>)this).GetEnumerator();
    }
    #endregion
    
    /// <summary>
    /// Verifies that operations have TransferQuantity set if their path is set to overlap by transfer quantity
    /// <param name="a_automaticallyResolveErrors">Whether to alter transfer quantity as needed to avoid validation errors</param>
    /// </summary>
    internal void ValidateOverlapTransferQuantity(bool a_automaticallyResolveErrors)
    {
        for (int leafI = 0; leafI < m_leaves.Count; ++leafI)
        {
            foreach (Association association in m_leaves[leafI].Successors)
            {
                if (association.OverlapType == InternalOperationDefs.overlapTypes.TransferQty)
                {
                    m_leaves[leafI].Operation.ValidateOverlapTransferQuantity(a_automaticallyResolveErrors);
                }
            }
        }
    }

    /// <summary>
    /// Whether the Path can use the specified Resource in one of it's Nodes.
    /// </summary>
    /// <param name="aPath"></param>
    /// <param name="aOp"></param>
    /// <param name="a_targetResourceId"></param>
    /// <returns></returns>
    public bool PathCanUseResource(BaseId a_targetResourceId)
    {
        for (int nodeI = 0; nodeI < NodeCount; nodeI++)
        {
            Node pathNode = GetNodeByIndex(nodeI);
            InternalOperation iOp = (InternalOperation)pathNode.Operation;
            PlantResourceEligibilitySet PRESet = iOp.ResourceRequirements.PrimaryResourceRequirement.EligibleResources;
            if (PRESet != null)
            {
                SortedDictionary<BaseId, EligibleResourceSet>.Enumerator PRESetEnumerator = PRESet.GetEnumerator();
                while (PRESetEnumerator.MoveNext())
                {
                    EligibleResourceSet eligResSet = PRESetEnumerator.Current.Value;
                    for (int resI = 0; resI < eligResSet.Count; resI++)
                    {
                        InternalResource resource = eligResSet[resI];
                        if (resource.Id == a_targetResourceId) //the dropped resource is eligible
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    public string GetLeadEligibleResources()
    {
        StringBuilder list = new();
        List<InternalOperation> leadOps = new();

        foreach (AlternatePath.Node node in AlternateNodeSortedList.Values)
        {
            InternalOperation op = node.Operation;
            if (op.Predecessors.Count > 0 || op.Finished)
            {
                //This op cannot be the lead op
                continue;
            }

            if (!leadOps.Any(x => x.ExternalId == op.ExternalId))
            {
                leadOps.Add(op);
            }
        }


        if (leadOps.Count == 0)
        {
            //This job cannot be scheduled
            return string.Empty;
        }

        foreach (InternalOperation leadOp in leadOps)
        {
            //Check default resource
            BaseResource defaultResource = leadOp.ResourceRequirements.PrimaryResourceRequirement.DefaultResource;

            if (defaultResource != null)
            {
                if (list.Length > 0)
                {
                    list.Append(", ");
                }

                list.Append(string.Format("{0}", defaultResource.Name));
                continue;


                for (int i = 0; i < leadOp.ResourceRequirements.EligibleResources.Count; i++)
                {
                    PlantResourceEligibilitySet resourceRequirementsEligibleResource = leadOp.ResourceRequirements.EligibleResources[i];

                    foreach (Resource curResource in resourceRequirementsEligibleResource.GetResources())
                    {
                        if (defaultResource.Id != curResource.Id)
                        {

                            if (list.Length > 0)
                            {
                                list.Append(", ");
                            }

                            list.Append(string.Format("{0}", curResource.Name));
                        }
                    }
                }
            }
        }

        return list.ToString();
    }
}