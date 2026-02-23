//using System;
//using System.Collections;
//using PT.Common;

//namespace PT.Scheduler
//{
//    /// <summary>
//    /// Summary description for SequentialDispatcher.
//    /// </summary>
//    public class SequentialDispatcher:ReadyActivitiesDispatcher
//    {
//        InternalActivityList activities=new InternalActivityList();
//        InternalActivityList.Node currentNode;

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="od">Original dispatcher</param>
//        public SequentialDispatcher(SequentialDispatcher od):base(od)
//        {
//        }

//        public override DispatcherDefinition DispatcherDefinition
//        {
//            get
//            {
//                return null;
//            }
//        }

//        #region IReadyActivitiesDispatcher Members

//        public override void Clear()
//        {
//            activities.Clear();
//        }

//        public override void Add(InternalActivity activity)
//        {
//            activities.Add(activity, activities.Last);
//        }

//        internal override int Count
//        {
//            get
//            {
//                return activities.Count;
//            }
//        }

//        public override void Remove(InternalActivity activity)
//        {
//            if(currentNode!=null && currentNode.Data==activity)
//            {
//                activities.Remove(currentNode);
//            }
//            else
//            {
//                InternalActivityList.Node n=activities.First;
//                while(n!=null)
//                {
//                    if(n.Data==activity)
//                    {
//                        activities.Remove(n);
//                        return;
//                    }

//                    n=n.Next;
//                }

//#if DEBUG
//                throw new PTException("The activity isn't on the dispatcher");
//#endif
//            }
//        }

//        public override void BeginDispatch(long time, bool enforceKeepSuccessors, bool enforceJITStartTimes)
//        {
//            currentNode=activities.First;
//        }

//        public override void EndDispatch()
//        {
//        }

//        /// <summary>
//        /// Get the next activitiy off the dispatcher.
//        /// </summary>
//        /// <returns></returns>
//        public override InternalActivity GetNext()
//        {
//            if(currentNode==null)
//            {
//                return null;
//            }
//            else
//            {
//                InternalActivity a=currentNode.Data;
//                currentNode=currentNode.Next;
//                return a;
//            }
//        }

//        /// <summary>
//        /// Peek at the next activity on the dispatcher.
//        /// </summary>
//        /// <returns></returns>
//        public override InternalActivity PeekNext()
//        {
//            if(currentNode==null)
//            {
//                return null;
//            }
//            else
//            {
//                return currentNode.Data;
//            }
//        }

//        #endregion

//        #region IClonable

//        public override object Clone()
//        {
//            return new SequentialDispatcher(this);
//        }

//        #endregion
//    }
//}

