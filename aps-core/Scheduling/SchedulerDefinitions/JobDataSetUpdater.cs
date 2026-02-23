using System.Collections;

using PT.APSCommon.Extensions;
using PT.Common.Exceptions;

namespace PT.SchedulerDefinitions;

/// <summary>
/// Summary description for JobDataSetUpdater.
/// </summary>
public class JobDataSetMoReqQtyUpdater
{
    public JobDataSetMoReqQtyUpdater(ref JobDataSet ds)
    {
        m_dataSet = ds;
    }

    private readonly JobDataSet m_dataSet;

    public void UpdateAllMoQuantities()
    {
        //Get the Row corresponding to the MO for use later
        for (int moI = 0; moI < m_dataSet.ManufacturingOrder.Count; moI++)
        {
            JobDataSet.ManufacturingOrderRow moRow = m_dataSet.ManufacturingOrder[moI];
            UpdateMoRequiredQty(moRow.ExternalId, moRow.RequiredQty);
        }
    }

    public void UpdateQuantities(string moExternalId)
    {
        //Get the Row corresponding to the MO for use later
        for (int moI = 0; moI < m_dataSet.ManufacturingOrder.Count; moI++)
        {
            JobDataSet.ManufacturingOrderRow moRow = m_dataSet.ManufacturingOrder[moI];
            if (moRow.ExternalId == moExternalId) //This is the one to update
            {
                UpdateMoRequiredQty(moExternalId, moRow.RequiredQty);
            }
        }
    }

    /// <summary>
    /// Updates the quantities for the Operations and Activities accoding to the new MoRequiredQty.
    /// </summary>
    public void UpdateMoRequiredQty(string moExternalId, decimal newMoRequiredQty)
    {
        JobDataSet.ManufacturingOrderRow moRow = null;
        //Get the Row corresponding to the MO for use later
        for (int moI = 0; moI < m_dataSet.ManufacturingOrder.Count; moI++)
        {
            JobDataSet.ManufacturingOrderRow nextRow = m_dataSet.ManufacturingOrder[moI];
            if (nextRow.ExternalId == moExternalId) //This is the one to update
            {
                moRow = nextRow;
                break;
            }
        }

        decimal expectedFinishQty = 0;
        //Zero the Operation quantities first because we'll be adding requirements for multiple possible successors and
        //  this assumes that the quantities were zeroed first.
        ZeroOperationQuantities(moExternalId);

        for (int pathI = 0; pathI < m_dataSet.AlternatePath.Count; pathI++)
        {
            JobDataSet.AlternatePathRow pathRow = m_dataSet.AlternatePath[pathI];
            if (pathRow.MoExternalId == moExternalId)
            {
                UpdatePathQty(moExternalId, pathRow.ExternalId, newMoRequiredQty, ref expectedFinishQty);

                //Set the MO qties
                moRow.ExpectedFinishQty = expectedFinishQty;
            }
        }
    }

    public class JobDataSetValidationException : PTHandleableException
    {
        public JobDataSetValidationException(string a_msg, object[] a_parameters = null, bool a_appendHelp = true)
            : base(a_msg, a_parameters, a_appendHelp) { }
    }

    public class JobDataSetPathValidationException : PTHandleableException
    {
        public JobDataSetPathValidationException(string a_msg, object[] a_parameters = null, bool a_appendHelp = true)
            : base(a_msg, a_parameters, a_appendHelp) { }
    }

    /// <summary>
    /// Set the quantities for operations in the specified path
    /// </summary>
    private void UpdatePathQty(string moExternalId, string pathExternalId, decimal newMoRequiredQty, ref decimal expectedMoFinishQty)
    {
        //Make sure there is a leaf operation before zeroing the op qties.  otherwise they'll remain zero
        int leafCount = 0;
        bool haveNode = false;
        for (int nodeI = 0; nodeI < m_dataSet.AlternatePathNode.Count; nodeI++)
        {
            JobDataSet.AlternatePathNodeRow nodeRow = m_dataSet.AlternatePathNode[nodeI];
            if (nodeRow.MoExternalId == moExternalId && nodeRow.PathExternalId == pathExternalId)
            {
                haveNode = true;
                if (string.IsNullOrEmpty(nodeRow.SuccessorOperationExternalId))
                {
                    leafCount++;
                }
            }
        }

        if (!haveNode)
        {
            return; //nothing to do since there are no path nodes
        }

        if (leafCount == 0)
        {
            throw new JobDataSetPathValidationException("2723", new object[] { pathExternalId });
        }

        int maxOpsToUpdate = m_dataSet.AlternatePathNode.Count * leafCount;
        int opsUpdatedSoFar = 0;
        decimal expectedOpFinishQty = 0;
        bool setExpectedMoFinishQty = false;
        //Iterate the path nodes, updating from the terminating operations backwards.
        for (int nodeI = 0; nodeI < m_dataSet.AlternatePathNode.Count; nodeI++)
        {
            JobDataSet.AlternatePathNodeRow nodeRow = m_dataSet.AlternatePathNode[nodeI];
            if (nodeRow.MoExternalId == moExternalId && nodeRow.PathExternalId == pathExternalId)
            {
                if (string.IsNullOrEmpty(nodeRow.SuccessorOperationExternalId))
                {
                    UpdateLeafOperation(moExternalId, pathExternalId, nodeRow.PredecessorOperationExternalId, newMoRequiredQty, ref expectedOpFinishQty, maxOpsToUpdate, ref opsUpdatedSoFar);
                    //The expected MoFinishQty is the minimum of the leaf expected finish qties.
                    if (!setExpectedMoFinishQty || expectedOpFinishQty < expectedMoFinishQty)
                    {
                        expectedMoFinishQty = expectedOpFinishQty;
                        setExpectedMoFinishQty = true;
                    }
                }
            }
        }

        UpdateOperationMatlReqs();
    }

    /// <summary>
    /// Used to store the original required start and required finish quantities. These are later used to determine the
    /// change in ratio and to update required quantities of stock, buy directs, and products.
    /// </summary>
    private readonly Hashtable opOrigReqStartQtyHT = new ();

    private class OriginalQtyValues
    {
        /// <summary>
        /// </summary>
        /// <param name="aRequiredStartQty">The original required start quantity of an operation.</param>
        /// <param name="aRequiredFinishQty">The original required finish quantity of an operation.</param>
        internal OriginalQtyValues(decimal aRequiredStartQty, decimal aRequiredFinishQty)
        {
            requiredStartQty = aRequiredStartQty;
            requiredFinishQty = aRequiredFinishQty;
        }

        /// <summary>
        /// The required start quantity of an operation.
        /// </summary>
        internal readonly decimal requiredStartQty;

        /// <summary>
        /// The required finish quantity of an operation.
        /// </summary>
        internal readonly decimal requiredFinishQty;
    }

    /// <summary>
    /// Set the Operation RequiredStart and RequiredFinish quantities to zero.
    /// </summary>
    /// <param name="moExternalId"></param>
    private void ZeroOperationQuantities(string moExternalId)
    {
        for (int opI = 0; opI < m_dataSet.ResourceOperation.Count; opI++)
        {
            JobDataSet.ResourceOperationRow opRow = m_dataSet.ResourceOperation[opI];
            if (opRow.MoExternalId == moExternalId)
            {
                if (!opOrigReqStartQtyHT.ContainsKey(opRow)) //the same op can be on multiple paths.  Just add once.
                {
                    opOrigReqStartQtyHT.Add(opRow, new OriginalQtyValues(opRow.RequiredStartQty, opRow.RequiredFinishQty));
                }

                opRow.RequiredStartQty = 0;
                opRow.RequiredFinishQty = 0;
            }
        }
    }

    /// <summary>
    /// Update the TotalRequiredQty field of stock material requirements and buy direct material required based
    /// on the ratio of the old and new required start quantities.
    /// Update the RequiredFinishQty field of Products based on the ratio of the old and new required finish quantities.
    /// In the event that one of the original RequiredStartQty or RequiredFinishQty are less than 0 no update is made to the
    /// corresponding required quantities.
    /// </summary>
    private void UpdateOperationMatlReqs()
    {
        for (int opI = 0; opI < m_dataSet.ResourceOperation.Count; ++opI)
        {
            JobDataSet.ResourceOperationRow opRow = m_dataSet.ResourceOperation[opI];
            object o = opOrigReqStartQtyHT[opRow];
            if (o != null)
            {
                OriginalQtyValues oqv = (OriginalQtyValues)o;
                decimal origStartQty = oqv.requiredStartQty;

                if (origStartQty > 0)
                {
                    decimal ratio = opRow.RequiredStartQty / origStartQty;
                    ratio = Math.Max(0, ratio);

                    JobDataSet.MaterialRequirementDataTable mrt = m_dataSet.MaterialRequirement;
                    for (int mrI = 0; mrI < mrt.Count; ++mrI)
                    {
                        JobDataSet.MaterialRequirementRow mrr = mrt[mrI];
                        if (mrr.OpExternalId == opRow.ExternalId)
                        {
                            mrr.TotalRequiredQty = mrr.TotalRequiredQty * ratio;
                            mrr.TotalCost = Math.Round(mrr.TotalCost * ratio, 2);
                        }
                    }
                }

                if (oqv.requiredFinishQty > 0)
                {
                    decimal reqFinQtyRatio = opRow.RequiredFinishQty / oqv.requiredFinishQty;
                    reqFinQtyRatio = Math.Max(0, reqFinQtyRatio);
                    JobDataSet.ProductDataTable pt = m_dataSet.Product;
                    for (int ptI = 0; ptI < pt.Count; ++ptI)
                    {
                        JobDataSet.ProductRow pr = pt[ptI];
                        if (pr.OpExternalId == opRow.ExternalId)
                        {
                            pr.TotalOutputQty = pr.TotalOutputQty * reqFinQtyRatio;
                        }
                    }
                }
            }
        }
    }

    private void UpdateLeafOperation(string moExternalId, string pathExternalId, string opExternalId, decimal newMoRequiredQty, ref decimal expectedOpFinishQty, int maxOpsToUpdate, ref int opsUpdatedSoFar)
    {
        for (int opI = 0; opI < m_dataSet.ResourceOperation.Count; opI++)
        {
            JobDataSet.ResourceOperationRow opRow = m_dataSet.ResourceOperation[opI];
            if (opRow.MoExternalId == moExternalId && opRow.ExternalId == opExternalId)
            {
                UpdateOperationAndPredecessors(opRow, pathExternalId, newMoRequiredQty, ref expectedOpFinishQty, maxOpsToUpdate, ref opsUpdatedSoFar);
                return;
            }
        }
    }

    private void UpdateOperationAndPredecessors(string moExternalId, string opExternalId, string pathExternalId, decimal newOpRequiredSupplyQty, ref decimal expectedOpFinishQty, int maxOpsToUpdate, ref int opsUpdatedSoFar)
    {
        for (int opI = 0; opI < m_dataSet.ResourceOperation.Count; opI++)
        {
            JobDataSet.ResourceOperationRow opRow = m_dataSet.ResourceOperation[opI];
            if (opRow.MoExternalId == moExternalId && opRow.ExternalId == opExternalId)
            {
                UpdateOperationAndPredecessors(opRow, pathExternalId, newOpRequiredSupplyQty, ref expectedOpFinishQty, maxOpsToUpdate, ref opsUpdatedSoFar);
                return;
            }
        }
    }

    private void UpdateOperationAndPredecessors(JobDataSet.ResourceOperationRow opRow, string pathExternalId, decimal newOpRequiredSupplyQty, ref decimal expectedOpFinishQty, int maxOpsToUpdate, ref int opsUpdatedSoFar)
    {
        if (opsUpdatedSoFar > maxOpsToUpdate)
        {
            throw new JobDataSetPathValidationException("2037", new object[] { pathExternalId, opRow.MoExternalId });
        }

        opsUpdatedSoFar++;

        //Add the quantity since it may have other successors that already updated it.
        opRow.RequiredFinishQty += newOpRequiredSupplyQty;
        //Adjust the startQty based on PlanningScrapPercent 
        if (opRow.WholeNumberSplits)
        {
            opRow.RequiredStartQty = Math.Ceiling(opRow.RequiredFinishQty / (1 - opRow.PlanningScrapPercent));
        }
        else
        {
            opRow.RequiredStartQty = opRow.RequiredFinishQty / (1 - opRow.PlanningScrapPercent);
        }

        if (opRow.WholeNumberSplits)
        {
            opRow.ExpectedScrapQty = Math.Ceiling(opRow.PlanningScrapPercent * opRow.RequiredStartQty);
        }
        else
        {
            opRow.ExpectedScrapQty = opRow.PlanningScrapPercent * opRow.RequiredStartQty;
        }

        //Update the predecessors first because we need to know how much material will come from them
        //  since a shortage may limit this operation's start qty.
        decimal expectedPredFinishQty = 0;
        decimal maxAllowableStartQty = 0; //as limited by preds
        bool maxAllowableStartQtySet = false;
        for (int nodeI = 0; nodeI < m_dataSet.AlternatePathNode.Count; nodeI++)
        {
            JobDataSet.AlternatePathNodeRow nodeRow = m_dataSet.AlternatePathNode[nodeI];
            if (nodeRow.MoExternalId == opRow.MoExternalId && nodeRow.PathExternalId == pathExternalId && nodeRow.SuccessorOperationExternalId == opRow.ExternalId && nodeRow.PredecessorOperationExternalId != opRow.ExternalId) //Found a predecessor to this operation so update it.  Last && is to protect against loops.
            {
                UpdateOperationAndPredecessors(nodeRow.MoExternalId, nodeRow.PredecessorOperationExternalId, pathExternalId, opRow.RequiredStartQty * nodeRow.UsageQtyPerCycle, ref expectedPredFinishQty, maxOpsToUpdate, ref opsUpdatedSoFar);

                //Need to get the minimum of all the pred expected finish qties
                decimal usageQtyPerCycle = nodeRow.UsageQtyPerCycle;
                if (usageQtyPerCycle == 0)
                {
                    usageQtyPerCycle = 1;
                }

                decimal predMaxAllowableStartQty = expectedPredFinishQty / usageQtyPerCycle;
                if (!maxAllowableStartQtySet || predMaxAllowableStartQty < maxAllowableStartQty)
                {
                    maxAllowableStartQty = predMaxAllowableStartQty;
                    maxAllowableStartQtySet = true;
                }
            }
        }

        if (opRow.WholeNumberSplits) //then don't allow decimal quantities.  Round down
        {
            maxAllowableStartQty = Math.Floor(maxAllowableStartQty);
        }

        decimal requiredOpFinishQty = opRow.RequiredFinishQty;
        //Calculate the maximum startable quantity and therefore the requiredFinishQty for the Activiites.
        if (maxAllowableStartQtySet && opRow.DeductScrapFromRequired) //Means we can't make more than the material we're supplied. So limit the qty to the minimum of the preds
        {
            requiredOpFinishQty = maxAllowableStartQty * (1 - opRow.PlanningScrapPercent);
            if (opRow.WholeNumberSplits) //then don't allow decimal quantities.  Round down
            {
                requiredOpFinishQty = Math.Floor(requiredOpFinishQty);
            }
        }

        //Update the Activiies
        UpdateActivityQuantities(opRow, ref expectedOpFinishQty, requiredOpFinishQty);
    }

    private void UpdateActivityQuantities(JobDataSet.ResourceOperationRow opRow, ref decimal expectedOpFinishQty, decimal requiredOpFinishQty)
    {
        int activityCount = GetUnfinishedOpActivityCount(opRow.MoExternalId, opRow.ExternalId);

        if (activityCount == 0)
        {
            UpdateActivityQuantitiesHelper(opRow, 0, 0, ref expectedOpFinishQty); //Need to call this to calculate the expectedFinishQty
            return;
        }

        //Evenly divide the Op qty accross the Activities.
        if (opRow.WholeNumberSplits)
        {
            int qtyPerActivity = (int)Math.Floor(requiredOpFinishQty / activityCount);
            int remainderQty = (int)requiredOpFinishQty - qtyPerActivity * activityCount;
            UpdateActivityQuantitiesHelper(opRow, qtyPerActivity, remainderQty, ref expectedOpFinishQty);
        }
        else
        {
            decimal qtyPerActivity = requiredOpFinishQty / activityCount;
            int remainderQty = 0;
            UpdateActivityQuantitiesHelper(opRow, qtyPerActivity, remainderQty, ref expectedOpFinishQty);
        }
    }

    private void UpdateActivityQuantitiesHelper(JobDataSet.ResourceOperationRow opRow, decimal activityReqFinQty, int remainderQty, ref decimal expectedOpFinishQty)
    {
        bool allocatedRemainder = false;
        opRow.RemainingFinishQty = 0;
        for (int activityI = 0; activityI < m_dataSet.Activity.Count; activityI++)
        {
            JobDataSet.ActivityRow activityRow = m_dataSet.Activity[activityI];
            if (activityRow.MoExternalId == opRow.MoExternalId && activityRow.OpExternalId == opRow.ExternalId)
            {
                //Don't mess with finished activities
                if (activityRow.ProductionStatus != InternalActivityDefs.productionStatuses.Finished.ToString())
                {
                    activityRow.RequiredFinishQty = activityReqFinQty;

                    if (remainderQty != 0 && !allocatedRemainder)
                    {
                        activityRow.RequiredFinishQty += remainderQty;
                        allocatedRemainder = true;
                    }

                    if (opRow.WholeNumberSplits)
                    {
                        activityRow.RequiredStartQty = Math.Ceiling(activityRow.RequiredFinishQty / (1 - opRow.PlanningScrapPercent));
                    }
                    else
                    {
                        activityRow.RequiredStartQty = activityRow.RequiredFinishQty / (1 - opRow.PlanningScrapPercent);
                    }

                    //Calculate Remaining Qties for Activity and sum for Op
                    activityRow.RemainingQty = activityRow.RequiredFinishQty - activityRow.ReportedGoodQty;
                    opRow.RemainingFinishQty += activityRow.RemainingQty;

                    //Calculate Expected Qty
                    if (opRow.DeductScrapFromRequired)
                    {
                        expectedOpFinishQty += activityRow.RequiredFinishQty - activityRow.ReportedScrapQty;
                    }
                    else
                    {
                        expectedOpFinishQty += activityRow.RequiredFinishQty;
                    }
                }
                else //Activity is finished
                {
                    expectedOpFinishQty += activityRow.ReportedGoodQty;
                }
            }
        }
    }

    /// <summary>
    /// Returns the number of Activities that the Operation contains.
    /// </summary>
    private int GetUnfinishedOpActivityCount(string moExternalId, string opExternalId)
    {
        int count = 0;
        for (int activityI = 0; activityI < m_dataSet.Activity.Count; activityI++)
        {
            JobDataSet.ActivityRow activityRow = m_dataSet.Activity[activityI];
            if (activityRow.MoExternalId == moExternalId && activityRow.OpExternalId == opExternalId)
            {
                //Can't use the finished activities since they can't produce either more or less.
                if (activityRow.ProductionStatus != InternalActivityDefs.productionStatuses.Finished.ToString())
                {
                    count++;
                }
            }
        }

        return count;
    }
}

///// <summary>
///// Worker class for updating the JobDataSet with time changes such as a new CycleSpan.
///// </summary>
//public class JobDataSetTimeUpdater
//{
//    JobDataSet dataSet;
//    public JobDataSetTimeUpdater(ref JobDataSet ds)
//    {
//        dataSet = ds;
//    }
//    public void UpdateCycleSpan(string moExternalId, string opExternalId, TimeSpan newCycleSpan)
//    {

//    }
//}

public class JobDataSetMoCopier
{
    public JobDataSetMoCopier(JobDataSet aDataSet)
    {
        m_ds = aDataSet;
    }

    public class JobDataSetMoCopierException : PTHandleableException
    {
        public JobDataSetMoCopierException(string msg, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(msg, a_stringParameters, a_appendHelpUrl) { }
    }

    private readonly JobDataSet m_ds;

    public void CopyMo(int sourceMoIndex, int nbrOfCopies)
    {
        CopyMo(m_ds.ManufacturingOrder[sourceMoIndex], nbrOfCopies);
    }

    public void CopyMo(string sourceMoExternalId, int nbrOfCopies)
    {
        for (int i = 0; i < m_ds.ManufacturingOrder.Count; i++)
        {
            JobDataSet.ManufacturingOrderRow moRow = m_ds.ManufacturingOrder[i];
            if (moRow.ExternalId == sourceMoExternalId)
            {
                CopyMo(moRow, nbrOfCopies);
                return;
            }
        }

        throw new JobDataSetMoCopierException("2724", new object[] { sourceMoExternalId });
    }

    public void CopyMo(JobDataSet.ManufacturingOrderRow moRow, int nbrOfCopies)
    {
        for (int i = 0; i < nbrOfCopies; i++)
        {
            CopyMo(moRow);
        }
    }

    public JobDataSet.ManufacturingOrderRow CopyMo(JobDataSet.ManufacturingOrderRow moRow)
    {
        string newMoExternalId = GetNextUniqueCopyMoExternalId(moRow.ExternalId);
        JobDataSet.ManufacturingOrderRow newMoRow = m_ds.ManufacturingOrder.NewManufacturingOrderRow();
        newMoRow.ItemArray = moRow.ItemArray;
        newMoRow.ExternalId = newMoExternalId;
        newMoRow.Name = GetNextUniqueCopyMOName(moRow.Name);
        m_ds.ManufacturingOrder.AddManufacturingOrderRow(newMoRow);

        #region Copy Operations
        for (int opI = 0; opI < moRow.GetResourceOperationRows().Length; opI++)
        {
            JobDataSet.ResourceOperationRow opRow = (JobDataSet.ResourceOperationRow)moRow.GetResourceOperationRows().GetValue(opI);
            JobDataSet.ResourceOperationRow newOpRow = CopyOperation(opRow, newMoExternalId);
        }
        #endregion

        #region Copy Paths
        for (int pI = 0; pI < moRow.GetAlternatePathRows().Length; pI++)
        {
            JobDataSet.AlternatePathRow newPRow = m_ds.AlternatePath.NewAlternatePathRow();
            JobDataSet.AlternatePathRow pRow = (JobDataSet.AlternatePathRow)moRow.GetAlternatePathRows().GetValue(pI);
            newPRow.ItemArray = pRow.ItemArray;
            newPRow.MoExternalId = newMoExternalId;
            m_ds.AlternatePath.AddAlternatePathRow(newPRow);
            //Copy Path Nodes
            for (int pnI = 0; pnI < pRow.GetAlternatePathNodeRows().Length; pnI++)
            {
                JobDataSet.AlternatePathNodeRow newPnRow = m_ds.AlternatePathNode.NewAlternatePathNodeRow();
                JobDataSet.AlternatePathNodeRow nRow = (JobDataSet.AlternatePathNodeRow)pRow.GetAlternatePathNodeRows().GetValue(pnI);
                newPnRow.ItemArray = nRow.ItemArray;
                newPnRow.MoExternalId = newMoExternalId;
                m_ds.AlternatePathNode.AddAlternatePathNodeRow(newPnRow);
            }
        }
        #endregion

        #region Copy Successor MOs
        for (int sI = 0; sI < moRow.GetSuccessorMORows().Length; sI++)
        {
            JobDataSet.SuccessorMORow newSRow = m_ds.SuccessorMO.NewSuccessorMORow();
            JobDataSet.SuccessorMORow sRow = (JobDataSet.SuccessorMORow)moRow.GetSuccessorMORows().GetValue(sI);
            newSRow.ItemArray = sRow.ItemArray;
            newSRow.MoExternalId = newMoExternalId;
            m_ds.SuccessorMO.AddSuccessorMORow(newSRow);
        }
        #endregion

        return newMoRow;
    }

    public void InsertOperationCopy(string sourceMoExternalId, string sourceOpExternalId)
    {
        for (int i = 0; i < m_ds.ResourceOperation.Count; i++)
        {
            JobDataSet.ResourceOperationRow opRow = m_ds.ResourceOperation[i];
            if (opRow.ExternalId == sourceOpExternalId && opRow.MoExternalId == sourceMoExternalId)
            {
                JobDataSet.ResourceOperationRow newOpRow = CopyOperation(opRow, sourceMoExternalId);
                //Now insert in path after the original Op.
                InsertOperationAfterInCurrentPath(sourceMoExternalId, opRow, newOpRow);
                return;
            }
        }

        throw new JobDataSetMoCopierException("2725", new object[] { sourceMoExternalId, sourceOpExternalId });
    }

    private void InsertOperationAfterInCurrentPath(string moExternalId, JobDataSet.ResourceOperationRow predOpRow, JobDataSet.ResourceOperationRow insertedOpRow)
    {
        for (int i = 0; i < m_ds.ManufacturingOrder.Count; i++)
        {
            JobDataSet.ManufacturingOrderRow moRow = m_ds.ManufacturingOrder[i];
            if (moRow.ExternalId == moExternalId)
            {
                string currentPathExternalId = moRow.CurrentPath;
                //Add a new PathNode between the pred and inserted op
                m_ds.AlternatePathNode.AddAlternatePathNodeRow(
                    moRow.JobExternalId,
                    moRow.ExternalId,
                    currentPathExternalId,
                    -1,
                    predOpRow.ExternalId,
                    predOpRow.Name,
                    predOpRow.Description,
                    insertedOpRow.ExternalId,
                    insertedOpRow.Name,
                    insertedOpRow.Description,
                    1,
                    double.MaxValue,
                    0,
                    InternalOperationDefs.overlapTypes.NoOverlap.ToString().Localize(),
                    0,
                    0,
                    false,
                    InternalOperationDefs.autoFinishPredecessorOptions.NoAutoFinish.ToString(),
                    false,
                    false,
                    OperationDefs.EOperationTransferPoint.EndOfOperation.ToString().Localize(),
                    OperationDefs.EOperationTransferPoint.StartOfOperation.ToString().Localize());
                //Change any nodes from the Pred (to OTHER ops) to use the Inserted op as the new pred
                for (int nI = 0; nI < m_ds.AlternatePathNode.Count; nI++)
                {
                    JobDataSet.AlternatePathNodeRow nodeRow = m_ds.AlternatePathNode[nI];
                    if (nodeRow.MoExternalId == moRow.ExternalId && nodeRow.PathExternalId == currentPathExternalId && nodeRow.PredecessorOperationExternalId == predOpRow.ExternalId && nodeRow.SuccessorOperationExternalId != insertedOpRow.ExternalId)
                    {
                        nodeRow.PredecessorOperationExternalId = insertedOpRow.ExternalId;
                        nodeRow.OperationName = insertedOpRow.Name;
                        nodeRow.OperationDescription = insertedOpRow.Description;
                    }
                }

                return;
            }
        }

        throw new JobDataSetMoCopierException("2724", new object[] { moExternalId });
    }

    private JobDataSet.ResourceOperationRow CopyOperation(JobDataSet.ResourceOperationRow originalOpRow, string newMoExternalId)
    {
        JobDataSet.ResourceOperationRow newOpRow = m_ds.ResourceOperation.NewResourceOperationRow();
        newOpRow.ItemArray = originalOpRow.ItemArray;
        //If copying the operation in the SAME MO then we need to create a new Op Name and ExternalId
        if (originalOpRow.MoExternalId == newMoExternalId)
        {
            newOpRow.Name = GetNextUniqueCopyOpName(newMoExternalId, originalOpRow.Name);
            newOpRow.ExternalId = GetNextUniqueCopyOpExternalId(newMoExternalId, originalOpRow.ExternalId);
        }

        newOpRow.MoExternalId = newMoExternalId;
        m_ds.ResourceOperation.AddResourceOperationRow(newOpRow);

        #region Copy Resource Requirements
        for (int rrI = 0; rrI < originalOpRow.GetResourceRequirementRows().Length; rrI++)
        {
            JobDataSet.ResourceRequirementRow newRrRow = m_ds.ResourceRequirement.NewResourceRequirementRow();
            JobDataSet.ResourceRequirementRow rrRow = (JobDataSet.ResourceRequirementRow)originalOpRow.GetResourceRequirementRows().GetValue(rrI);
            newRrRow.ItemArray = rrRow.ItemArray;
            newRrRow.MoExternalId = newMoExternalId;
            newRrRow.OpExternalId = newOpRow.ExternalId;
            m_ds.ResourceRequirement.AddResourceRequirementRow(newRrRow);

            //Copy Required Capabilities
            for (int capI = 0; capI < rrRow.GetCapabilityRows().Length; capI++)
            {
                JobDataSet.CapabilityRow newCapRow = m_ds.Capability.NewCapabilityRow();
                JobDataSet.CapabilityRow capRow = (JobDataSet.CapabilityRow)rrRow.GetCapabilityRows().GetValue(capI);
                newCapRow.ItemArray = capRow.ItemArray;
                newCapRow.MoExternalId = newMoExternalId;
                newCapRow.OpExternalId = newOpRow.ExternalId;
                m_ds.Capability.AddCapabilityRow(newCapRow);
            }
        }
        #endregion

        #region Materials
        //Copy Stock Materials
        for (int mI = 0; mI < originalOpRow.GetMaterialRequirementRows().Length; mI++)
        {
            JobDataSet.MaterialRequirementRow newMRow = m_ds.MaterialRequirement.NewMaterialRequirementRow();
            JobDataSet.MaterialRequirementRow mRow = (JobDataSet.MaterialRequirementRow)originalOpRow.GetMaterialRequirementRows().GetValue(mI);
            newMRow.ItemArray = mRow.ItemArray;
            newMRow.MoExternalId = newMoExternalId;
            newMRow.OpExternalId = newOpRow.ExternalId;
            m_ds.MaterialRequirement.AddMaterialRequirementRow(newMRow);
        }
        #endregion

        #region Products
        for (int pI = 0; pI < originalOpRow.GetProductRows().Length; pI++)
        {
            JobDataSet.ProductRow newPRow = m_ds.Product.NewProductRow();
            JobDataSet.ProductRow pRow = (JobDataSet.ProductRow)originalOpRow.GetProductRows().GetValue(pI);
            newPRow.ItemArray = pRow.ItemArray;
            newPRow.MoExternalId = newMoExternalId;
            newPRow.OpExternalId = newOpRow.ExternalId;
            m_ds.Product.AddProductRow(newPRow);
        }
        #endregion

        #region Attributes
        for (int attI = 0; attI < originalOpRow.GetResourceOperationAttributesRows().Length; attI++)
        {
            JobDataSet.ResourceOperationAttributesRow newAttRow = m_ds.ResourceOperationAttributes.NewResourceOperationAttributesRow();
            JobDataSet.ResourceOperationAttributesRow attRow = (JobDataSet.ResourceOperationAttributesRow)originalOpRow.GetResourceOperationAttributesRows().GetValue(attI);
            newAttRow.ItemArray = attRow.ItemArray;
            newAttRow.MoExternalId = newMoExternalId;
            newAttRow.OpExternalId = newOpRow.ExternalId;
            m_ds.ResourceOperationAttributes.AddResourceOperationAttributesRow(newAttRow);
        }
        #endregion

        #region Activities
        for (int aI = 0; aI < originalOpRow.GetActivityRows().Length; aI++)
        {
            JobDataSet.ActivityRow newARow = m_ds.Activity.NewActivityRow();
            JobDataSet.ActivityRow aRow = (JobDataSet.ActivityRow)originalOpRow.GetActivityRows().GetValue(aI);
            newARow.ItemArray = aRow.ItemArray;
            newARow.MoExternalId = newMoExternalId;
            newARow.OpExternalId = newOpRow.ExternalId;
            //Clear status info
            newARow.ReportedGoodQty = 0;
            newARow.ReportedPostProcessingHrs = 0;
            newARow.ReportedRunHrs = 0;
            newARow.ReportedScrapQty = 0;
            newARow.ReportedSetupHrs = 0;
            newARow.ProductionStatus = InternalActivityDefs.productionStatuses.Ready.ToString();
            m_ds.Activity.AddActivityRow(newARow);
        }
        #endregion

        return newOpRow;
    }

    private string GetNextUniqueCopyMOName(string sourceMoName)
    {
        Hashtable existingIds = new ();
        for (int i = 0; i < m_ds.ManufacturingOrder.Count; i++)
        {
            JobDataSet.ManufacturingOrderRow moRow = m_ds.ManufacturingOrder[i];
            if (!existingIds.Contains(moRow.Name))
            {
                existingIds.Add(moRow.Name, null);
            }
        }

        int nxtNbr = 2;
        while (true)
        {
            string proposedId;
            proposedId = string.Format("{0}-{1}", sourceMoName, nxtNbr);
            if (!existingIds.Contains(proposedId))
            {
                return proposedId;
            }

            nxtNbr++;
        }
    }

    private string GetNextUniqueCopyMoExternalId(string sourceMoExternalId)
    {
        Hashtable existingIds = new ();
        for (int i = 0; i < m_ds.ManufacturingOrder.Count; i++)
        {
            JobDataSet.ManufacturingOrderRow moRow = m_ds.ManufacturingOrder[i];
            if (!existingIds.Contains(moRow.ExternalId))
            {
                existingIds.Add(moRow.ExternalId, null);
            }
        }

        int nxtNbr = 2;
        while (true)
        {
            string proposedId;
            proposedId = string.Format("{0}-{1}", sourceMoExternalId, nxtNbr);
            if (!existingIds.Contains(proposedId))
            {
                return proposedId;
            }

            nxtNbr++;
        }
    }

    private string GetNextUniqueCopyOpExternalId(string sourceMoExternalId, string sourceOpExternalId)
    {
        Hashtable existingIds = new ();
        for (int i = 0; i < m_ds.ResourceOperation.Count; i++)
        {
            JobDataSet.ResourceOperationRow opRow = m_ds.ResourceOperation[i];
            if (opRow.MoExternalId == sourceMoExternalId && !existingIds.Contains(opRow.ExternalId))
            {
                existingIds.Add(opRow.ExternalId, null);
            }
        }

        int nxtNbr = 2;
        while (true)
        {
            string proposedId;
            proposedId = string.Format("{0}-{1}", sourceOpExternalId, nxtNbr);
            if (!existingIds.Contains(proposedId))
            {
                return proposedId;
            }

            nxtNbr++;
        }
    }

    private string GetNextUniqueCopyOpName(string sourceMoExternalId, string sourceOpName)
    {
        Hashtable existingIds = new ();
        for (int i = 0; i < m_ds.ResourceOperation.Count; i++)
        {
            JobDataSet.ResourceOperationRow opRow = m_ds.ResourceOperation[i];
            if (opRow.MoExternalId == sourceMoExternalId && !existingIds.Contains(opRow.ExternalId))
            {
                existingIds.Add(opRow.ExternalId, null);
            }
        }

        int nxtNbr = 2;
        while (true)
        {
            string proposedId;
            proposedId = string.Format("{0}-{1}", sourceOpName, nxtNbr);
            if (!existingIds.Contains(proposedId))
            {
                return proposedId;
            }

            nxtNbr++;
        }
    }
}