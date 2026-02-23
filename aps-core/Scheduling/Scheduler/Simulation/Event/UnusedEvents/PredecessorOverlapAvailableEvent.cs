//using System;

//namespace PT.Scheduler.Simulation.Events
//{
//    /// <summary>
//    /// Indicates that enough material has been delivered from the Predecessor Operation for one or more Successor Operations to potentially
//    ///  be able to start.  Whether enough is actually available depends on the SuccessorOperation's schedule and it's resource's Overlap setting. 
//    /// </summary>
//    public class PredecessorOverlapAvailableEvent:EventBase
//    {
//        public PredecessorOverlapAvailableEvent(long time, InternalOperation predecessorOperation):base(time)
//        {
//            this.PredecessorOperation=predecessorOperation;
//        }
//        InternalOperation predecessorOperation;
//        public InternalOperation PredecessorOperation
//        {
//            get{return this.predecessorOperation;}
//            set{this.predecessorOperation=value;}
//        }
//    }
//}

