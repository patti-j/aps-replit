//using System;

//namespace PT.Scheduler.Simulation.Events
//{
//    /// <summary>
//    /// Indicatest that the maximum allowable time delay (specified in MaxDelay for the Predecessor-Successor relation) has been reached.
//    /// </summary>
//    public class MaxDelayReachedEvent:EventBase
//    {
//        public MaxDelayReachedEvent(long time, BaseOperation predecessorOperation, BaseOperation successorOperation):base(time)
//        {
//            this.PredecessorOperation=predecessorOperation;
//            this.SuccessorOperation=successorOperation;
//        }
//        BaseOperation predecessorOperation;
//        public BaseOperation PredecessorOperation
//        {
//            get{return this.predecessorOperation;}
//            set{this.predecessorOperation=value;}
//        }
//        BaseOperation successorOperation;
//        public BaseOperation SuccessorOperation
//        {
//            get{return this.successorOperation;}
//            set{this.successorOperation=value;}
//        }
//    }
//}

