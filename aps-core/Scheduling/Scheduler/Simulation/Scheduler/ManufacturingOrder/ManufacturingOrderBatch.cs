//using PT.Common.Exceptions;

//namespace PT.Scheduler.Simulation;

//public class ManufacturingOrderBatch
//{
//    private long m_batchDate = long.MaxValue;

//    /// <summary>
//    /// The earliest need date or release date (depending on the batch method) of all manufacturing orders.
//    /// </summary>
//    public long BatchDate => m_batchDate;

//    /// <summary>
//    /// How this value's used depends on the batch method.
//    /// </summary>
//    private long m_eligibilityComparisonDate;

//    private readonly ManufacturingOrderBatchDefinition m_manufacturingOrderBatchDefinition;

//    /// <summary>
//    /// It's having a problem separating Item and the this indexer.
//    /// </summary>
//    public ManufacturingOrderBatchDefinition ManufacturingOrderBatchDefinition => m_manufacturingOrderBatchDefinition;

//    internal ManufacturingOrderBatch(ManufacturingOrderBatchDefinition a_manufacturingOrderBatchDefinition, long a_clock, ScenarioDetail.AddMOReleaseEventArgsForOpt a_args)
//    {
//        m_manufacturingOrderBatchDefinition = a_manufacturingOrderBatchDefinition;
//        m_clock = a_clock;
//        m_args = a_args;

//        switch (m_manufacturingOrderBatchDefinition.MOBatchMethod)
//        {
//            case SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByNeedDate:
//            case SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByReleaseDate:
//                m_releaseDateTicks = long.MaxValue;
//                m_eligibilityComparisonDate = long.MaxValue;
//                break;

//            case SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByNeedWhereBatchEligibilityIsDeterminedByReleaseDateBeingEarlierThanNeedDate:
//                m_releaseDateTicks = long.MinValue;
//                m_eligibilityComparisonDate = long.MinValue;
//                break;

//            default:
//                throw new ManufacturingOrderBatchTypeException();
//        }
//    }

//    private readonly long m_clock;
//    private readonly ScenarioDetail.AddMOReleaseEventArgsForOpt m_args;

//    internal List<ManufacturingOrderInfo> m_mos = new ();

//    public decimal CalculateTotalQty()
//    {
//        decimal totalQty = 0;
//        for (int i = 0; i < m_mos.Count; ++i)
//        {
//            ManufacturingOrder mo = m_mos[i].m_mo;
//            totalQty += mo.RequiredQty;
//        }

//        return totalQty;
//    }

//    private bool IsEligibleNbrOfBatches(ManufacturingOrder a_mo)
//    {
//        decimal totalQty = a_mo.RequiredQty + CalculateTotalQty();
//        return totalQty <= m_manufacturingOrderBatchDefinition.MaxBatchQty;
//    }

//    private bool IsEligibleWithinCurrentWindow(long a_date)
//    {
//        switch (m_manufacturingOrderBatchDefinition.MOBatchMethod)
//        {
//            case SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByNeedDate:
//            case SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByReleaseDate:
//                return a_date >= m_batchDate && a_date < m_eligibilityComparisonDate;

//            case SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByNeedWhereBatchEligibilityIsDeterminedByReleaseDateBeingEarlierThanNeedDate:
//                return a_date < m_eligibilityComparisonDate;

//            default:
//                throw new ManufacturingOrderBatchTypeException();
//        }
//    }

//    private bool CanBatchWindowBeRedefinedWithNewEarlierBatchDate(long a_newBatchStartDate)
//    {
//        switch (m_manufacturingOrderBatchDefinition.MOBatchMethod)
//        {
//            case SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByNeedDate:
//            case SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByReleaseDate:
//            {
//                long newBatchEndDate = a_newBatchStartDate + m_manufacturingOrderBatchDefinition.BatchWindowTicks;
//                for (int i = 0; i < m_mos.Count; ++i)
//                {
//                    ManufacturingOrder mo = m_mos[i].m_mo;
//                    long moBatchDate = GetMOBatchDate(m_clock, mo);
//                    if (moBatchDate >= newBatchEndDate)
//                    {
//                        return false;
//                    }
//                }
//            }
//                break;

//            case SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByNeedWhereBatchEligibilityIsDeterminedByReleaseDateBeingEarlierThanNeedDate:
//            {
//                if (a_newBatchStartDate < m_batchDate)
//                {
//                    long newBatchEndByReleaseDate = a_newBatchStartDate - m_manufacturingOrderBatchDefinition.BatchWindowTicks;

//                    for (int moI = 0; moI < m_mos.Count; ++moI)
//                    {
//                        if (m_mos[moI].m_releaseDate >= newBatchEndByReleaseDate)
//                        {
//                            return false;
//                        }
//                    }
//                }
//            }
//                break;

//            default:
//                throw new ManufacturingOrderBatchTypeException();
//        }

//        return true;
//    }

//    internal bool IsEligible(ManufacturingOrder a_mo)
//    {
//        if (IsEligibleNbrOfBatches(a_mo))
//        {
//            long batchDate = GetMOBatchDate(m_clock, a_mo);

//            if (batchDate < m_batchDate)
//            {
//                return CanBatchWindowBeRedefinedWithNewEarlierBatchDate(batchDate);
//            }

//            switch (m_manufacturingOrderBatchDefinition.MOBatchMethod)
//            {
//                case SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByNeedDate:
//                case SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByReleaseDate:
//                    return IsEligibleWithinCurrentWindow(batchDate);

//                case SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByNeedWhereBatchEligibilityIsDeterminedByReleaseDateBeingEarlierThanNeedDate:
//                {
//                    long releaseDate = a_mo.CalcPathsReleaseTicks(m_clock, a_mo.CurrentPath, 0, m_args);
//                    return IsEligibleWithinCurrentWindow(releaseDate);
//                }

//                default:
//                    throw new ManufacturingOrderBatchTypeException();
//            }
//        }

//        return false;
//    }

//    private long GetMOBatchDate(long a_simClock, ManufacturingOrder a_mo)
//    {
//        long date;

//        switch (m_manufacturingOrderBatchDefinition.MOBatchMethod)
//        {
//            case SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByNeedDate:
//                date = a_mo.GetNeedDateTicksOfMOForScheduling();
//                break;

//            case SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByReleaseDate:
//                date = a_mo.CalcPathsReleaseTicks(a_simClock, a_mo.CurrentPath, m_clock, m_args);
//                break;

//            case SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByNeedWhereBatchEligibilityIsDeterminedByReleaseDateBeingEarlierThanNeedDate:
//                date = a_mo.GetNeedDateTicksOfMOForScheduling();
//                break;

//            default:
//                throw new ManufacturingOrderBatchTypeException();
//        }

//        return date;
//    }

//    #if DEBUG
//    private static int _m_nextBatchId = 1;
//    private int _m_batchId = -1;
//    #endif

//    internal void Add(ManufacturingOrder a_mo)
//    {
//        #if DEBUG
//        if (_m_batchId == -1)
//        {
//            _m_batchId = _m_nextBatchId;
//            ++_m_nextBatchId;
//        }

//        a_mo.Description = "Batch #" + _m_nextBatchId;
//        #endif
//        long moBatchDate = GetMOBatchDate(m_clock, a_mo);

//        if (moBatchDate < m_batchDate)
//        {
//            m_batchDate = moBatchDate;
//            switch (m_manufacturingOrderBatchDefinition.MOBatchMethod)
//            {
//                case SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByNeedDate:
//                case SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByReleaseDate:
//                    m_eligibilityComparisonDate = m_batchDate + m_manufacturingOrderBatchDefinition.BatchWindowTicks;
//                    break;

//                case SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByNeedWhereBatchEligibilityIsDeterminedByReleaseDateBeingEarlierThanNeedDate:
//                    m_eligibilityComparisonDate = m_batchDate - m_manufacturingOrderBatchDefinition.BatchWindowTicks;
//                    break;

//                default:
//                    throw new ManufacturingOrderBatchTypeException();
//            }
//        }

//        ManufacturingOrder.EffectiveReleaseDateType releaseType;
//        long moReleaseDateTicks;
//        moReleaseDateTicks = a_mo.CalcPathsReleaseTicks(m_clock, a_mo.CurrentPath, m_clock, m_args);

//        switch (m_manufacturingOrderBatchDefinition.MOBatchMethod)
//        {
//            case SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByNeedDate:
//            case SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByReleaseDate:
//                if (moReleaseDateTicks < m_releaseDateTicks)
//                {
//                    m_releaseDateTicks = moReleaseDateTicks;
//                    m_releaseType = ManufacturingOrder.EffectiveReleaseDateType.PredecessorMO;
//                }

//                break;

//            case SchedulerDefinitions.ManufacturingOrderBatchSettings.EManufacturingOrderBatchMethods.ByNeedWhereBatchEligibilityIsDeterminedByReleaseDateBeingEarlierThanNeedDate:
//                if (moReleaseDateTicks > m_releaseDateTicks)
//                {
//                    m_releaseDateTicks = moReleaseDateTicks;
//                    m_releaseType = releaseType;
//                }

//                break;

//            default:
//                throw new ManufacturingOrderBatchTypeException();
//        }

//        m_mos.Add(new ManufacturingOrderInfo(a_mo, moReleaseDateTicks));
//        a_mo.m_batch = this;
//    }

//    internal long m_releaseDateTicks;
//    internal ManufacturingOrder.EffectiveReleaseDateType m_releaseType;

//    internal List<BatchesOperationNameData> m_batchOpNameDataList = new ();

//    public List<BatchesOperationNameData>.Enumerator GetBatchOperationNameEnumerator()
//    {
//        return m_batchOpNameDataList.GetEnumerator();
//    }

//    /// <summary>
//    /// Operation data for all operations in the batch that have the same name. It is assumed these operations are identicle except for quantity.
//    /// </summary>
//    public class BatchesOperationNameData
//    {
//        internal BatchesOperationNameData(string a_opName)
//        {
//            m_opName = a_opName;
//        }

//        public readonly string m_opName;

//        internal List<BaseOperation> m_opsList = new ();

//        public List<BaseOperation>.Enumerator GetOpEnumerator()
//        {
//            return m_opsList.GetEnumerator();
//        }

//        internal int m_totalUnscheduledPredOps;

//        internal void BatchOperationDataConstructionComplete()
//        {
//            m_opsList.Sort(CompareByMONeedDate);
//            m_opsList[0].m_manufacturingOrderBatch_batchOrderData_op_index = 0;

//            for (int opI = 1; opI < m_opsList.Count; ++opI)
//            {
//                InternalOperation op = (InternalOperation)m_opsList[opI];
//                op.WaitForLeftBatchNeighborReleaseEvent = true;
//                op.m_manufacturingOrderBatch_batchOrderData_op_index = opI;

//                for (int actI = 0; actI < op.Activities.Count; ++actI)
//                {
//                    InternalActivity ia = op.Activities.GetByIndex(actI);
//                    ia.SuppressReleaseDateAdjustments = true;
//                }
//            }

//            IncrementSuccessorLevelsCount();
//        }

//        private void IncrementSuccessorLevelsCount()
//        {
//            for (int opI = 0; opI < m_opsList.Count; ++opI)
//            {
//                AlternatePath.AssociationCollection successors = m_opsList[opI].AlternatePathNode.Successors;

//                for (int sucI = 0; sucI < successors.Count; ++sucI)
//                {
//                    InternalOperation sucOp = (InternalOperation)successors[sucI].Successor.Operation;
//                    bool trackNbrOfPredBatchOpsToBeScheduled = false;

//                    for (int actI = 0; actI < sucOp.Activities.Count; ++actI)
//                    {
//                        InternalActivity sucAct = sucOp.Activities.GetByIndex(actI);

//                        if (!sucAct.Sequenced)
//                        {
//                            trackNbrOfPredBatchOpsToBeScheduled = true;
//                            //ia.WaitForPredBatchOpnsToBeScheduled = true;
//                        }
//                    }

//                    if (trackNbrOfPredBatchOpsToBeScheduled)
//                    {
//                        ++sucOp.m_manufacturingOrderBatch_batchOperationNameData.m_totalUnscheduledPredOps;
//                    }
//                }
//            }
//        }

//        internal void SetActivityPredecessorBatchConstraints()
//        {
//            if (m_totalUnscheduledPredOps > 0)
//            {
//                for (int opI = 0; opI < m_opsList.Count; ++opI)
//                {
//                    InternalOperation op = (InternalOperation)m_opsList[opI];
//                    for (int actI = 0; actI < op.Activities.Count; ++actI)
//                    {
//                        InternalActivity ia = op.Activities.GetByIndex(actI);
//                        if (!ia.Sequenced)
//                        {
//                            ia.WaitForPredBatchOpnsToBeScheduled = true;
//                        }
//                    }
//                }
//            }
//        }

//        internal List<InternalOperation> GetOpsNoLongerWaitingForPredBatchOps()
//        {
//            List<InternalOperation> o_sucOpsNoLongerWaitingForPredBatchOps = new ();

//            for (int opI = 0; opI < m_opsList.Count; ++opI)
//            {
//                InternalOperation sucOp = (InternalOperation)m_opsList[opI];
//                bool trackNbrOfPredBatchOpsToBeScheduled = false;

//                for (int actI = 0; actI < sucOp.Activities.Count; ++actI)
//                {
//                    InternalActivity ia = sucOp.Activities.GetByIndex(actI);

//                    if (!ia.Sequenced)
//                    {
//                        trackNbrOfPredBatchOpsToBeScheduled = true;
//                    }
//                }

//                if (trackNbrOfPredBatchOpsToBeScheduled)
//                {
//                    if (sucOp.PredBatchOpsScheduled != true)
//                    {
//                        sucOp.PredBatchOpsScheduled = true;
//                        o_sucOpsNoLongerWaitingForPredBatchOps.Add(sucOp);
//                    }
//                }
//            }

//            return o_sucOpsNoLongerWaitingForPredBatchOps;
//        }

//        private static int CompareByMONeedDate(BaseOperation a_x, BaseOperation a_y)
//        {
//            int compare;

//            compare = Comparison.Compare(a_x.ManufacturingOrder.NeedDateTicks, a_y.ManufacturingOrder.NeedDateTicks);

//            if (compare == 0)
//            {
//                compare = Comparison.Compare(a_x.ManufacturingOrder.Id.Value, a_y.ManufacturingOrder.Id.Value);
//            }

//            return compare;
//        }

//        public int Count => m_opsList.Count;
//    }

//    internal void BatchingComplete()
//    {
//        Dictionary<string, int> batchOpDataNameToListIndex = new ();

//        for (int moI = 0; moI < m_mos.Count; ++moI)
//        {
//            ManufacturingOrder mo = m_mos[moI].m_mo;

//            for (int nodeI = 0; nodeI < mo.CurrentPath.NodeCount; ++nodeI)
//            {
//                AlternatePath.Node node = mo.CurrentPath[nodeI];
//                if (node.Operation.IsNotFinishedAndNotOmitted)
//                {
//                    int listIdx;
//                    BatchesOperationNameData bond;

//                    if (batchOpDataNameToListIndex.TryGetValue(node.Operation.Name, out listIdx))
//                    {
//                        bond = m_batchOpNameDataList[listIdx];
//                    }
//                    else
//                    {
//                        bond = new BatchesOperationNameData(node.Operation.Name);
//                        m_batchOpNameDataList.Add(bond);
//                        listIdx = m_batchOpNameDataList.Count - 1;
//                        batchOpDataNameToListIndex.Add(bond.m_opName, listIdx);
//                    }

//                    bond.m_opsList.Add(node.Operation);
//                    node.Operation.m_manufacturingOrderBatch_batchOperationNameData = bond;
//                }
//            }
//        }

//        for (int bodI = 0; bodI < m_batchOpNameDataList.Count; ++bodI)
//        {
//            m_batchOpNameDataList[bodI].BatchOperationDataConstructionComplete();
//        }

//        for (int bodI = 0; bodI < m_batchOpNameDataList.Count; ++bodI)
//        {
//            m_batchOpNameDataList[bodI].SetActivityPredecessorBatchConstraints();
//        }
//    }

//    internal class ManufacturingOrderInfo
//    {
//        internal ManufacturingOrderInfo(ManufacturingOrder a_mo, long a_releaseDate)
//        {
//            m_mo = a_mo;
//            m_releaseDate = a_releaseDate;
//        }

//        internal ManufacturingOrder m_mo;
//        internal long m_releaseDate;
//    }

//    internal class ManufacturingOrderBatchTypeException : PTException
//    {
//        internal ManufacturingOrderBatchTypeException()
//            : base("4082") { }
//    }
//}