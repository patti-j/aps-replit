//****************************************************************************************************************************************************************************************************************************
// Originally created for a Churchill requirement. 
// This isn't in use. Jim said he found some older code in the UI related to this and used that.
//****************************************************************************************************************************************************************************************************************************

//using System;
//using System.Collections.Generic;
//using System.Text;

//using PT.Scheduler;
//using PT.Scheduler.Simulation;

//namespace PT.Scheduler.Schedule.InventoryManagement.Utilities
//{
//    public class ProductUsage
//    {
//        Dictionary<string, List<MRSupply.Node>> whereUsed=new Dictionary<string,List<MRSupply.Node>>();

//        /// <summary>
//        /// Create a set of data for product usage lookup.
//        /// </summary>
//        /// <param name="sd"></param>
//        public void GetWhereUsed(ScenarioDetail sd)
//        {
//            whereUsed.Clear();

//            for (int jI = 0; jI < sd.JobManager.Count; ++jI)
//            {
//                Job j = sd.JobManager[jI];

//                for (int mI = 0; mI < j.ManufacturingOrders.Count; ++mI)
//                {
//                    ManufacturingOrder mo = j.ManufacturingOrders[mI];
//                    List<ResourceOperation> rol=mo.CurrentPath.GetOperationsByLevel();

//                    for (int roI = 0; roI < rol.Count; ++roI)
//                    {
//                        ResourceOperation ro = rol[roI];

//                        if (ro.Scheduled)
//                        {
//                            for (int mrI = 0; mrI < ro.MaterialRequirements.Count; ++mrI)
//                            {
//                                MaterialRequirement mr = ro.MaterialRequirements[mrI];

//                                for (int sI = 0; sI < mr.MRSupply.Count; ++sI)
//                                {
//                                    MRSupply.Node mrSupplyNode=mr.MRSupply.SupplyList[sI];
//                                    InternalActivity supply = mrSupplyNode.Supply as InternalActivity;

//                                    if (supply != null)
//                                    {
//                                        string key=GetKey(supply.Operation, mr.Item);
//                                        List<MRSupply.Node> list;

//                                        if (whereUsed.ContainsKey(key))
//                                        {
//                                            list = whereUsed[key];
//                                        }
//                                        else
//                                        {
//                                            list = new List<MRSupply.Node>();
//                                            whereUsed.Add(key, list);
//                                        }

//                                        list.Add(mrSupplyNode);
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//        }

//        string GetKey(InternalOperation io, Item item)
//        {
//            return io.Job.Id.Value.ToString() + "." + io.ManufacturingOrder.Id.Value.ToString() + "." + io.Id.Value.ToString() + "." + item.Id.Value.ToString();
//        }

//        /// <summary>
//        /// Get a list of where a product is used. 
//        /// </summary>
//        /// <param name="item">A product of an operation.</param>
//        /// <param name="io"></param>
//        /// <returns></returns>
//        public List<MRSupply.Node> WhereUsed(Item item, InternalOperation io)
//        {
//            string key=GetKey(io, item);
//            List<MRSupply.Node> list;

//            whereUsed.TryGetValue(key, out list);
//            return list;
//        }
//    }
//}

