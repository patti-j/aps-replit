//using System;

//using PT.Database;
//using PT.SchedulerDefinitions;
//using PT.Transmissions;

//namespace PT.Scheduler
//{
//    public class BatchProcessorOperation : BaseOperation, PT.Common.IPTSerializable
//    {
//        #region IPTSerializable Members

//        public BatchProcessorOperation(PT.Common.IReader reader, PlantManager plantManager)
//            : base(reader, plantManager)
//        {
//            if (reader.VersionNumber >= 1)
//            {
//                reader.Read(out batchingCode);
//                reader.Read(out qtyPerContainer);
//                reader.Read(out splitOperationsToFillBatches);

//                reader.Read(out loadSpanPerContainer);
//                reader.Read(out unloadSpanPerContainer);
//            }
//        }

//        public override void Serialize(PT.Common.IWriter writer)
//        {
//            base.Serialize(writer);

//            writer.Write(batchingCode);
//            writer.Write(qtyPerContainer);
//            writer.Write(splitOperationsToFillBatches);

//            writer.Write(loadSpanPerContainer);
//            writer.Write(unloadSpanPerContainer);
//        }

//        public new const int UNIQUE_ID = 9;

//        public override int UniqueId
//        {
//            get
//            {
//                return UNIQUE_ID;
//            }
//        }

//        #endregion

//        #region Construction

//        public BatchProcessorOperation()
//        {
//        }

//        #endregion

//        #region Shared Properties

//        string batchingCode = "";
//        /// <summary>
//        /// Only Operations with the same BatchingCode are allowed to be run together in the same batch.
//        /// </summary>
//        public string BatchingCode
//        {
//            get { return batchingCode; }
//            set { batchingCode = value; }
//        }

//        long loadSpanPerContainer;
//        /// <summary>
//        /// The timespan it takes to load one container of product into the resource.   The resource is considered busy during this period.
//        /// </summary>
//        public TimeSpan LoadSpanPerContainer
//        {
//            get { return new TimeSpan(loadSpanPerContainer); }
//            set { loadSpanPerContainer = value.Ticks; }
//        }

//        decimal qtyPerContainer = 1;
//        /// <summary>
//        /// The amount of parts that fit in each of the containers used for processing.
//        /// </summary>
//        public decimal QtyPerContainer
//        {
//            get { return qtyPerContainer; }
//            set { qtyPerContainer = value; }
//        }

//        bool splitOperationsToFillBatches = false;
//        /// <summary>
//        /// If true then if a batch is not full and a compatible Operation is ready but will not fit then process whatever will fit and keep the rest for a subsequent batch.	
//        /// If false, then only add an Operation to a partially filled batch if it can entirely fit.  
//        /// Note that an Operation will still be split into multiple batches if it is too many containers to fit entirely in its own batch.
//        /// </summary>
//        public bool SplitOperationsToFillBatches
//        {
//            get { return splitOperationsToFillBatches; }
//            set { splitOperationsToFillBatches = value; }
//        }

//        long unloadSpanPerContainer;
//        /// <summary>
//        /// The amount of time it takes to unload one container of product from the resource. The resource is considered busy during this period.
//        /// </summary>
//        public TimeSpan UnloadSpanPerContainer
//        {
//            get { return new TimeSpan(unloadSpanPerContainer); }
//            set { unloadSpanPerContainer = value.Ticks; }
//        }

//        #endregion Shared Properties

//        /// <summary>
//        /// This is the sum of the Activity durations times there resources' Resource costs, if scheduled.
//        /// </summary>
//        /// <returns></returns>
//        public override decimal GetResourceCost()
//        {
//            return 0; //TODO
//        }

//        #region Transmission functionality

//        public override void Receive(OperationIdBaseT t)
//        {
//        }

//        #endregion

//        #region Cloning

//        public ResourceOperation Clone()
//            {
//                return PT.Common.Cloning.PrimitiveCloning.PrimitiveClone(orig, this, typeof(BatchProcessorOperation), 
//                      PT.Common.Cloning.PrimitiveCloning.Depth.Shallow, PT.Common.Cloning.PrimitiveCloning.OtherIncludedTypes.All);
//            }

//        object ICloneable.Clone()
//        {
//            return Clone();
//        }

//        #endregion

//        #region PT Database

//        internal override void PopulateJobDataSet(ref JobDataSet dataSet)
//        {
//            //JMC TODO
//        }

//        public override void PtDbPopulate(ref PtDbDataSet dataSet)
//        {
//            //TODO
//        }

//        public override void PtDbUpdate(ref PtDbDataSet dataSet)
//        {
//            //TODO
//        }

//        #endregion
//    }
//}

