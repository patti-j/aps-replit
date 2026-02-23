// !ALTERNATE_PATH!; The initial version of this enhancement was checked in on 10/5/2011. You can see the initial changes in SourceOffSite.

using System.Collections;
using System.Text;

using PT.Common.File;
using PT.Scheduler.Simulation.Scheduler;
using PT.ServerManagerSharedLib.Definitions;

namespace PT.Scheduler;

/// <summary>
/// Contains all data related to one copy
/// </summary>
public partial class ScenarioDetail
{
    #region Diagnostics
    private void PrintBlockStaus()
    {
        //			// Add all the unscheduled activities to the reschedule activities list.
        //			for(int jobManagerI=0; jobManagerI<jobManager.Count; jobManagerI++)
        //			{
        //				Job job=jobManager[jobManagerI];
        //				ManufacturingOrderManager moCollection=job.ManufacturingOrders;
        //
        //				System.Diagnostics.Trace.WriteLine("****************************************");
        //				System.Diagnostics.Trace.WriteLine(string.Format("MO={0}", job.Name));
        //
        //				if(!job.Cancelled
        //					&&!job.Finished)
        //				{
        //					for(int moI=0; moI<moCollection.Count; moI++)
        //					{
        //						ManufacturingOrder mo=moCollection[moI];
        //						System.Diagnostics.Trace.WriteLine(string.Format("MO={0}", mo.Name));
        //
        //						if(!mo.Finished)
        //						{
        //							IDictionaryEnumerator alternateNodesEnumerator=mo.CurrentPath.AlternateNodeHash.GetEnumerator();
        //
        //							while(alternateNodesEnumerator.MoveNext())
        //							{
        //								DictionaryEntry de=(DictionaryEntry)alternateNodesEnumerator.Current;
        //								AlternatePath.Node node=(AlternatePath.Node)de.Value;
        //
        //								if(node.Operation is ResourceOperation)
        //								{
        //									ResourceOperation op=(ResourceOperation)node.Operation;
        //
        //									for(int activityI=0; activityI<op.Activities.Count; ++activityI)
        //									{
        //										InternalActivity activity=op.Activities.GetByIndex(activityI);
        //
        //										if(activity.InternalMainBlock!=null)
        //										{
        //											System.Diagnostics.Trace.WriteLine(string.Format("OP{0}. {1}\t{2}", op.Name, new DateTime(activity.InternalMainBlock.ScheduledStartDate), new DateTime(activity.InternalMainBlock.ScheduledFinishDateTicks)));
        //										}
        //										else
        //										{
        //											System.Diagnostics.Trace.WriteLine(string.Format("OP{0}. NOT SCHEDULED", op.Name));
        //										}
        //									}
        //								}
        //							}
        //						}
        //					}
        //				}
        //			}		
    }

    //		internal void WriteSchedule(string fileName)
    //		{
    //			PT.Common.File.TextFile file=new PT.Common.File.TextFile();
    //
    //			for(int jobI=0; jobI<JobManager.Count; ++jobI)
    //			{
    //				Job job=JobManager[jobI];
    //
    //				if(job.Scheduled)
    //				{
    //					for(int moI=0; moI<job.ManufacturingOrders.Count; ++moI)
    //					{
    //						ManufacturingOrder mo=job.ManufacturingOrders[moI];
    //						if(mo.Scheduled)
    //						{
    //							AlternatePath path=mo.CurrentPath;
    //
    //							IDictionaryEnumerator pathEnum=path.AlternateNodeHash.GetEnumerator();
    //
    //							while(pathEnum.MoveNext())
    //							{
    //								AlternatePath.Node apNode=(AlternatePath.Node)pathEnum.Value;
    //								ResourceOperation operation=apNode.Operation as ResourceOperation;
    //								InternalActivityManager iam=operation.Activities;
    //								for(int iaI=0; iaI<iam.Count; ++iaI)
    //								{
    //									InternalActivity ia=iam.GetByIndex(iaI);
    //									for(int rrI=0; rrI<ia.ResourceRequirementBlockCount; ++rrI)
    //									{
    //										ResourceBlock rb=ia.GetResourceRequirementBlock(rrI);
    //										string line=string.Format("Job={0}; MO={1}; Op={2}; Start={3}({4}); Finish={5}({6})", job.ExternalId, mo.ExternalId, operation.ExternalId, rb.GetScheduledStartTicks, rb.ScheduledStartDate, rb.ScheduledEnd, rb.ScheduledFinishDateTicks);
    //										file.AppendText(line);
    //									}
    //								}
    //							}
    //						}
    //					}
    //				}
    //			}
    //
    //			if(System.IO.File.Exists(fileName))
    //			{
    //				System.IO.File.Delete(fileName);
    //			}
    //
    //			file.WriteFile(fileName);
    //		}

    /// <summary>
    /// Write the resource schedule and inventory allocations to a file for the purpose of unit testing.
    /// First all the block on every resource are written then all the inventory items are written.
    /// [1] RESOURCE [Id] [ExternalId] [Name]
    /// JOB [Id] [ExternalId] [Name] *** MO [id] [ExternalId] [Name] *** OP [Id] [ExternalId] [Name] *** AC [Id] [ExternalId]
    /// START [632204060661718750] [5/17/2004 3:54 PM] *** END [632205809661718750] [5/19/2004 4:29 PM]
    /// [2] RESOURCE [2] [M30] [MIX1      ]
    /// JOB [74] [403] [Planned WO 403] *** MO [1] [PO1] [PO1] *** OP [1] [2231] [040   ] *** AC [1] [Activity1]
    /// START [632278332000000000] [8/11/2004 3:00 PM] *** END [632283776880000000] [8/17/2004 10:14 PM]
    /// ----------------------------------------------------------------------------------------------------
    /// [1] RESOURCE [3] [M31] [MOLD1     ]
    /// JOB [55] [41] [00001019       ] *** MO [1] [WO1] [WO1] *** OP [2] [193] [060   ] *** AC [1] [Activity1]
    /// START [632204061071718750] [5/17/2004 3:55 PM] *** END [632204073071718750] [5/17/2004 4:15 PM]
    /// ----------------------------------------------------------------------------------------------------------
    /// [1] ITEM [Id] [ExternalId] [Name]
    /// TIME [632210338471718750] [5/24/2004 10:17 PM]
    /// ON_HAND_QTY [100] ADJ_QTY [100]
    /// [2] ITEM [100] [Shovel-A200                   ] [275]
    /// TIME [632286990000000000] [8/21/2004 3:30 PM]
    /// ON_HAND_QTY [1000] ADJ_QTY [900]
    /// ----------------------------------------------------------------------------------------------------
    /// [1] ITEM [99] [Shovel-A100                   ] [274]
    /// TIME [632210338471718750] [5/24/2004 10:17 PM]
    /// ON_HAND_QTY [100] ADJ_QTY [100]
    /// </summary>
    /// <param name="fileName"></param>
    internal void WriteUnitTestFile(string a_directory, SimulationType a_simulationType)
    {
        TextFile file = new ();

        file.AppendText(a_simulationType.ToString());
        file.AppendText("");
        file.AppendText(GetSeparator());

        for (int plantI = 0; plantI < PlantManager.Count; ++plantI)
        {
            Plant plant = PlantManager[plantI];

            for (int departmentI = 0; departmentI < plant.Departments.Count; ++departmentI)
            {
                Department department = plant.Departments[departmentI];

                for (int resourceI = 0; resourceI < department.Resources.Count; ++resourceI)
                {
                    string resourceLine;
                    Resource resource = department.Resources[resourceI];
                    ResourceBlockList.Node node = resource.Blocks.First;
                    resourceLine = string.Format("RESOURCE [{0}] [{1}] [{2}]", resource.Id, resource.ExternalId, resource.Name);

                    int count = 0;
                    while (node != null)
                    {
                        ++count;
                        ResourceBlock rb = node.Data;
                        string line = string.Format("[{1}] {0}", resourceLine, count);
                        file.AppendText(line);

                        InternalActivity ia = rb.Activity;
                        InternalOperation io = ia.Operation;
                        ManufacturingOrder mo = io.ManufacturingOrder;

                        Job job = mo.Job;

                        line = string.Format("     JOB [{0}] [{1}] [{2}]", job.Id, job.ExternalId, job.Name);
                        line = string.Format("{0} *** MO [{1}] [{2}] [{3}]", line, mo.Id, mo.ExternalId, mo.Name);
                        line = string.Format("{0} *** OP [{1}] [{2}] [{3}]", line, io.Id, io.ExternalId, io.Name);
                        line = string.Format("{0} *** AC [{1}] [{2}]", line, ia.Id, ia.ExternalId);

                        file.AppendText(line);

                        line = string.Format("     START [{0}] [{1}] *** END [{2}] [{3}]", rb.StartTicks, PrintDate(rb.StartDateTime), rb.EndTicks, PrintDate(rb.EndDateTime));
                        file.AppendText(line);
                        file.AppendText("");

                        node = node.Next;
                    }

                    if (count > 0)
                    {
                        file.AppendText(GetSeparator());
                    }
                }
            }
        }


        WriteUnitTestItems(file);

        // Format the output file's name like this: scn.001.sim.001.UtT
        string fileName = string.Format("scn.{0}.sim.{1}.UtT", _scenario.Id.Value.ToString("D3"), m_unitTestNum.ToString("D3"));
        string filePath = Path.Combine(a_directory, fileName);
        File.Delete(filePath);
        file.WriteFile(filePath);

        ++m_unitTestNum;
    }

    private int m_unitTestNum;

    /// <summary>
    /// Write the inventory records to a file for the purpose of unit testing. An old and new file can be compared to verify no changes exist between the two.
    /// [1] ITEM [Id] [ExternalId] [Name]
    /// TIME [632210338471718750] [5/24/2004 10:17 PM]
    /// ADJ_QTY [100]
    /// [2] ITEM [100] [Shovel-A200                   ] [275]
    /// TIME [632286990000000000] [8/21/2004 3:30 PM]
    /// ADJ_QTY [900]
    /// ----------------------------------------------------------------------------------------------------
    /// [1] ITEM [99] [Shovel-A100                   ] [274]
    /// TIME [632210338471718750] [5/24/2004 10:17 PM]
    /// ADJ_QTY [100]
    /// </summary>
    /// <param name="file"></param>
    private void WriteUnitTestItems(TextFile a_file)
    {
        for (int whI = 0; whI < m_warehouseManager.Count; ++whI)
        {
            Warehouse wh = m_warehouseManager.GetByIndex(whI);
            IEnumerator<Inventory> invEnum = wh.Inventories.GetEnumerator();

            while (invEnum.MoveNext())
            {
                Inventory inv = invEnum.Current;
                string itemLine = string.Format("ITEM [{0}] [{1}] [{2}]", inv.Item.Id, inv.Item.ExternalId, inv.Item.Name);

                if (inv.Adjustments != null)
                {
                    for (int adjI = 0; adjI < inv.Adjustments.Count; ++adjI)
                    {
                        Adjustment adj = inv.Adjustments[adjI];
                        string line = string.Format("[{0}] {1}", adjI + 1, itemLine);
                        a_file.AppendText(line);

                        line = string.Format("     TIME [{0}] [{1}]", adj.Time, PrintDate(adj.AdjDate));
                        a_file.AppendText(line);

                        line = string.Format("     ADJ_QTY [{0}]", adj.ChangeQty);
                        a_file.AppendText(line);

                        a_file.AppendText("");
                    }

                    if (inv.Adjustments.Count > 0)
                    {
                        a_file.AppendText(GetSeparator());
                    }
                }
            }
        }
    }

    private string PrintDate(DateTime a_dt)
    {
        return string.Format("{0} {1}", a_dt.ToShortDateString(), a_dt.ToShortTimeString());
    }

    private string GetSeparator()
    {
        return "----------------------------------------------------------------------------------------------------------------------------------------------------------------";
    }

    internal void TestSchedule(string a_title)
    {
        long errorCount = 0;
        long stateVariableErrorCount = 0;
        long numberOfBlocks = 0;
        Hashtable numberOfJobs = new ();
        MOKeyComparer moKeyComparer = new ();
        Hashtable numberOfMOs = new (moKeyComparer, moKeyComparer);

        string bannerEdge = "*******************************************************************************";


        System.Diagnostics.Trace.WriteLine("");
        System.Diagnostics.Trace.WriteLine("");
        System.Diagnostics.Trace.WriteLine(bannerEdge);
        System.Diagnostics.Trace.WriteLine(MidifyText(bannerEdge, a_title.ToUpper()));
        System.Diagnostics.Trace.WriteLine(bannerEdge);

        HashSet<InternalActivity> processedActivities = new ();
        HashSet<Batch> processedBatches = new ();

        for (int plantI = 0; plantI < m_plantManager.Count; ++plantI)
        {
            Plant plant = m_plantManager[plantI];

            for (int departmentI = 0; departmentI < plant.Departments.Count; ++departmentI)
            {
                Department department = plant.Departments[departmentI];

                for (int machineI = 0; machineI < department.Resources.Count; ++machineI)
                {
                    Resource machine = department.Resources[machineI];
                    ResourceBlockList.Node currentNode = machine.Blocks.First;
                    ResourceBlockList.Node lastNode = null;

                    //machine.PrintResultantCapacity("");

                    while (currentNode != null)
                    {
                        ResourceBlock block = currentNode.Data;
                        //System.Collections.Generic.List<ProductionByCapacityInterval> list = mb.GetProductionByCapacityInterval();

                        numberOfBlocks++;
                        AlternatePath.Node node = block.Activity.Operation.AlternatePathNode;
                        for (int predecessorI = 0; predecessorI < node.Predecessors.Count; ++predecessorI)
                        {
                            AlternatePath.Association association = node.Predecessors[predecessorI];
                            if (association.Predecessor.Operation is ResourceOperation)
                            {
                                ResourceOperation machineOperation = (ResourceOperation)association.Predecessor.Operation;

                                if (machineOperation.IsNotFinishedAndNotOmitted)
                                {
                                    if (machineOperation.Activities.Count == 1)
                                    {
                                        long opScheduledFinishDate;

                                        if (machineOperation.GetScheduledFinishDate(out opScheduledFinishDate, true))
                                        {
                                            if (opScheduledFinishDate > block.StartTicks)
                                            {
                                                System.Diagnostics.Trace.WriteLine("***SUCCESSOR STARTS BEFORE PREDECESSOR COMPLETES***");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        System.Diagnostics.Trace.WriteLine("***MULTIPLE PREDECESSORS DETECTED***");
                                    }
                                }
                            }
                        }

                        string jobId = block.Activity.Operation.ManufacturingOrder.Job.ExternalId;

                        if (!numberOfJobs.Contains(jobId))
                        {
                            numberOfJobs.Add(jobId, null);
                        }

                        string moId = block.Activity.Operation.ManufacturingOrder.ExternalId;

                        MOKey moKey = new (jobId, moId);

                        if (!numberOfMOs.Contains(moKey))
                        {
                            numberOfMOs.Add(moKey, null);
                        }

                        if (block.StartTicks > block.EndTicks)
                        {
                            System.Diagnostics.Trace.WriteLine("***SCHEDULED START TIME AFTER SCHEDULED FINISH TIME***");
                            errorCount++;
                        }

                        if (block.StartTicks == block.EndTicks)
                        {
                            System.Diagnostics.Trace.WriteLine("***ZERO LENGTH BLOCK***");
                            errorCount++;
                        }

                        if (block.StartTicks < block.Activity.Operation.ManufacturingOrder.EffectiveReleaseDate)
                        {
                            System.Diagnostics.Trace.WriteLine("***OPERATION SCHEDULED BEFORE RELEASE DATE");
                            errorCount++;
                        }

                        if (block.EndTicks <= block.StartTicks)
                        {
                            System.Diagnostics.Trace.WriteLine("***OPERATION SCHEDULED TO END BEFORE IT STARTS");
                            WriteBlock(block);
                            errorCount++;
                        }

                        if (block.EndTicks < Clock)
                        {
                            System.Diagnostics.Trace.WriteLine("***OPERATION SCHEDULED TO COMPLETE BEFORE THE CLOCK***");
                            WriteBlock(block);
                            errorCount++;
                        }

                        if (block.Activity.Operation.MaterialRequirements.GetLastestConstrainingBuyDirectMaterialRequirement(Clock) > block.StartTicks)
                        {
                            System.Diagnostics.Trace.WriteLine("***OPERATION SCHEDULED TO START BEFORE ITS LATEST MATERIAL CONSTRAINT***");
                            WriteBlock(block);
                            errorCount++;
                        }

                        //							if(lastNode!=null)
                        //							{
                        //								if(machine.CapacityType==InternalResourceDefs.capacityTypes.SingleTasking)
                        //								{
                        //									ResourceBlock mbLast=lastNode.Data;
                        //									if(mbLast.ScheduledFinishDateTicks>mb.ScheduledFinishDateTicks)
                        //									{
                        //										System.Diagnostics.Trace.WriteLine("***BLOCKS OVERLAP! First and second block listed below.***");
                        //										WriteBlock(mbLast);
                        //										WriteBlock(mb);
                        //										//									System.Diagnostics.Trace.WriteLine("***AND HERE ARE ALL THE BLOCKS ON THIS MACHINE IN THE ORDER IN WHICH THEY APPEAR.***");
                        //										//									WriteResourceBlocks(machine);
                        //
                        //										errorCount++;
                        //									}
                        //								}
                        //							}

                        Batch batch = block.Batch;

                        if (!processedBatches.Contains(batch))
                        {
                            processedBatches.Add(batch);

                            IEnumerator<InternalActivity> batchActivitiesEtr = batch.GetEnumerator();
                            while (batchActivitiesEtr.MoveNext())
                            {
                                InternalActivity act = batchActivitiesEtr.Current;

                                if (!processedActivities.Contains(act))
                                {
                                    processedActivities.Add(act);
                                }
                                else
                                {
                                    System.Diagnostics.Trace.WriteLine(string.Format("The following activity appears in multiple batches: {0}", batch));
                                }
                            }
                        }

                        lastNode = currentNode;
                        currentNode = currentNode.Next;
                    }

                    //						System.Diagnostics.Trace.WriteLine("***Here are all the blocks on this resource.***");
                    //						WriteResourceBlocks(machine);
                    //						System.Diagnostics.Trace.WriteLine(bannerEdge);
                }
            }
        }

        List<Pair<InternalActivity, Resource>> actsNotSchedOnRRList = new ();
        IEnumerator<Batch> batchManagerEnumerator = m_batchManager.GetEnumerator();
        while (batchManagerEnumerator.MoveNext())
        {
            Batch batch = batchManagerEnumerator.Current;
            IEnumerator<InternalActivity> actItr = batch.GetEnumerator();
            while (actItr.MoveNext())
            {
                InternalActivity act = actItr.Current;
                if (act.ResourceReservationMade)
                {
                    int primaryIndex = act.Operation.ResourceRequirements.PrimaryResourceRequirementIndex;
                    ResourceReservation primaryReservation = act.GetReservationForRR(primaryIndex);
                    // 2011.06.22: presuming a single resource requirement. Multiples may be built into the feature later but it's unlikely at the moment.
                    //TODO: test helpers
                    Resource scheduledResource = batch.PrimaryResourceBlock.ScheduledResource;
                    if (primaryReservation.ReservedResource != scheduledResource)
                    {
                        actsNotSchedOnRRList.Add(new Pair<InternalActivity, Resource>(act, scheduledResource));

                        StringBuilder sb = new ();
                        sb.AppendLine(string.Format("Activity: {0}", act));
                        sb.AppendLine("Was scheduled on Resource: {0}" + scheduledResource);
                        sb.AppendLine("But had a ResourceReservation on: " + primaryReservation.ReservedResource);
                        errorCount = TestScheduleAppendErrorMsgHelper(errorCount, bannerEdge, sb.ToString(), true);
                    }

                    if (primaryReservation.StartTicks != batch.m_si.m_scheduledStartDate)
                    {
                        string errorMsg = string.Format("Activity: {0} had a resource reservation at {1} but ended up being scheduled at {2}", act, DateTimeHelper.ToLocalTimeFromUTCTicks(primaryReservation.StartTicks), DateTimeHelper.ToLocalTimeFromUTCTicks(batch.m_si.m_scheduledStartDate));
                        errorCount = TestScheduleAppendErrorMsgHelper(errorCount, bannerEdge, errorMsg, true);
                    }
                }
            }
        }

        errorCount += TestJobs();

        WriteHighlightedLine(string.Format("{0} ERRORS FOUND(NOT INCLUDING STATE VARIABLES).", errorCount));
        WriteHighlightedLine(string.Format("{0} STATE VARIABLES ERRORS.", stateVariableErrorCount));

        System.Diagnostics.Trace.WriteLine(MidifyText(bannerEdge, "SUMMARY"));
        WriteHighlightedLine(string.Format("{0} JOBS FOUND.", numberOfJobs.Count));
        WriteHighlightedLine(string.Format("{0} MANUFACTURING ORDERS FOUND.", numberOfMOs.Count));
        WriteHighlightedLine(string.Format("{0} BLOCKS FOUND.", numberOfBlocks));
        System.Diagnostics.Trace.WriteLine(bannerEdge);
        System.Diagnostics.Trace.WriteLine("");
        System.Diagnostics.Trace.WriteLine("");
    }

    private static long TestScheduleAppendErrorMsgHelper(long errorCount, string bannerEdge, string errorMsg, bool throwExceptionInDebugMode)
    {
        StringBuilder sb = new ();
        sb.AppendLine(bannerEdge);
        sb.AppendLine(errorMsg);
        sb.AppendLine(bannerEdge);
        ++errorCount;
        System.Diagnostics.Trace.Write(errorMsg);
        #if DEBUG
        if (throwExceptionInDebugMode)
        {
            throw new Exception(errorMsg);
        }
        #endif
        return errorCount;
    }

    /// <summary>
    /// Performs some tests to verify that no successor begins
    /// before its predecessors are complete.
    /// </summary>
    /// <returns></returns>
    private long TestJobs()
    {
        long errorCnt = 0;

        for (int jobI = 0; jobI < m_jobManager.Count; jobI++)
        {
            Job job = m_jobManager[jobI];

            for (int moI = 0; moI < job.ManufacturingOrders.Count; moI++)
            {
                ManufacturingOrder mo = job.ManufacturingOrders[moI];

                AlternatePath path = mo.CurrentPath;
                AlternatePath.NodeCollection leaves = path.Leaves;

                for (int nodeI = 0; nodeI < leaves.Count; nodeI++)
                {
                    AlternatePath.Node node = leaves[nodeI];
                    errorCnt += TestOperationAgainstSuccessors(node);
                }
            }
        }

        return errorCnt;
    }

    private long TestOperationAgainstSuccessors(AlternatePath.Node a_node)
    {
        long errorCnt = 0;

        if (a_node?.Operation is InternalOperation internalOperation)
        {
            if (internalOperation.GetScheduledFinishDate(out long opScheduledFinishDate, true))
            {
                long startDate = PTDateTime.MaxDateTimeTicks;
                bool successors = false;

                for (int sucOpI = 0; sucOpI < a_node.Successors.Count; ++sucOpI)
                {
                    AlternatePath.Node sucOpNode = a_node.Successors[sucOpI].Successor;
                    BaseOperation sucOp = sucOpNode.Operation;

                    if (sucOp is InternalOperation sucInternalOperation)
                    {
                        successors = true;
                        long tempStartDate = sucInternalOperation.GetEarliestScheduledActivityStartDate(out _);
                        if (tempStartDate < startDate)
                        {
                            startDate = tempStartDate;
                        }
                    }

                    errorCnt += TestOperationAgainstSuccessors(sucOpNode);
                }

                if (successors)
                {
                    if (startDate < opScheduledFinishDate)
                    {
                        ManufacturingOrder mo = internalOperation.ManufacturingOrder;
                        Job job = mo.Job;
                        string message = string.Format("Successor starts before predecessor. Predecessor: Job {0}; MO={1}; Op={2}", job.ExternalId, mo.ExternalId, internalOperation.ExternalId);
                        WriteHighlightedLine(message);
                        ++errorCnt;
                    }
                }
            }
        }

        return errorCnt;
    }

    private void WriteBlock(ResourceBlock a_block)
    {
        DateTime start = new (a_block.StartTicks);
        DateTime finish = new (a_block.EndTicks);

        System.Diagnostics.Trace.WriteLine(string.Format("Job{0}; MO={1};OP={2}; Start={3}:{4}({5}); Finish={6}:{7}({8})", a_block.Activity.Operation.ManufacturingOrder.Job.ExternalId, a_block.Activity.Operation.ManufacturingOrder.ExternalId, a_block.Activity.Operation.ExternalId, start.ToLongDateString(), start.ToLongTimeString(), start.Ticks, finish.ToLongDateString(), finish.ToLongTimeString(), finish.Ticks));
    }

    private void WriteResourceBlocks(Resource a_machine)
    {
        System.Diagnostics.Trace.WriteLine(string.Format("Resource's Name '{0}'", a_machine.Name));
        ResourceBlockList.Node mbn = a_machine.Blocks.First;
        while (mbn != null)
        {
            ResourceBlock mb = mbn.Data;
            WriteBlock(mb);
            mbn = mbn.Next;
        }
    }

    private string MidifyText(string a_bannerEdge, string a_text)
    {
        string bannerTextLine = a_bannerEdge;
        int middle = bannerTextLine.Length / 2;
        bannerTextLine = bannerTextLine.Insert(middle, a_text);
        bool front = true;

        while (bannerTextLine.Length > a_bannerEdge.Length)
        {
            if (front)
            {
                bannerTextLine = bannerTextLine.Remove(0, 1);
            }
            else
            {
                bannerTextLine = bannerTextLine.Remove(bannerTextLine.Length - 1, 1);
            }

            front = !front;
        }

        return bannerTextLine;
    }

    private void WriteHighlightedLine(string a_text)
    {
        System.Diagnostics.Trace.WriteLine(string.Format("*** {0}", a_text));
    }

    /// <summary>
    /// If a unit test is being run then write the schedule out to either the UnitTest folder or
    /// the UnitTestBase folder.
    /// </summary>
    private void UnitTestHandling()
    {
        switch (ServerSessionManager.StartType)
        {
            case EStartType.UnitTest:
                UnitTestWriter.WriteScenarioDetails(PTSystem.WorkingDirectory.UnitTest, this, m_activeSimulationType);
                break;

            case EStartType.UnitTestBase:
                UnitTestWriter.WriteScenarioDetails(PTSystem.WorkingDirectory.UnitTestBase, this, m_activeSimulationType);
                break;
        }
    }
    #endregion Diagnostics (schedule testing)
}