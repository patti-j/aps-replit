//using System;
//using System.ComponentModel;

//using PT.SchedulerDefinitions;

//namespace PT.Scheduler
//{
//    /// <summary>
//    /// Summary description for ExternalDispatcherDefinition.
//    /// </summary>
//    [Serializable]
//    public class ExternalDispatcherDefinition:System.Runtime.Serialization.ISerializable
//                                              {
//        #region ISerializable Members
//        public ExternalDispatcherDefinition(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
//        {
//            PT.Common.Serialization.SetProperties(info,this,typeof(ExternalDispatcherDefinition));
//        }
//        public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
//        {
//            PT.Common.Serialization.GetProperties(info,this,typeof(ExternalDispatcherDefinition),false);
//        }
//        #endregion

//        public ExternalDispatcherDefinition(BaseId id)//:base(id)
//        {
//        }

//        #region DispatcherDefinition Members

////		public override ReadyActivitiesDispatcher CreateDispatcher()
////		{
////			return new BalancedCompositeDispatcher(this);
////		}

//        #endregion
//    }
//}

