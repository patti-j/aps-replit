//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Drawing;

//using PT.Common;
//using PT.Scheduler;
//using PT.SchedulerDefinitions;

//namespace PT.APIDefinitions.RequestsAndResponses.ActivityStatus
//{
//    /// <summary>
//    /// Describes which Resources a particular non Master Scheduler User has access to and 
//    /// what access is allowed.
//    /// </summary>
//    public class ViewerResourceInfos : PT.Common.IPTSerializable
//    {
//        #region IPTSerializable Members
//        public const int UNIQUE_ID = 490;

//        public ViewerResourceInfos(PT.Common.IReader reader)
//        {
//            if (reader.VersionNumber >= 1)
//            {
//                int resCount;
//                reader.Read(out resCount);
//                for (int i = 0; i < resCount; i++)
//                {
//                    ViewerResourceInfo info = new ViewerResourceInfo(reader);
//                    this.Add(info);
//                }
//            }
//        }

//        public void Serialize(PT.Common.IWriter writer)
//        {
//            writer.Write(this.Count);
//            for (int i = 0; i < this.Count; i++)
//            {
//                this[i].Serialize(writer);
//            }
//        }

//        public int UniqueId
//        {
//            get { return UNIQUE_ID; }
//        }
//        #endregion

//        public ViewerResourceInfos()
//        {
//        }

//        private readonly ArrayList resourceInfos = new ArrayList();

//        public int Count
//        {
//            get { return this.resourceInfos.Count; }
//        }

//        public void Add(ViewerResourceInfo info)
//        {
//            this.resourceInfos.Add(info);
//        }

//        public ViewerResourceInfo this[int index]
//        {
//            get { return (ViewerResourceInfo)this.resourceInfos[index]; }
//        }

//        public void Clear()
//        {
//            this.resourceInfos.Clear();
//        }
//    }

//    /// <summary>
//    /// The rights the user has regarding the specified Resource and the list of BlockInfo objects 
//    /// associated with the Resource.
//    /// </summary>
//    public class ViewerResourceInfo : PT.Common.IPTSerializable
//    {
//        #region IPTSerializable Members
//        public const int UNIQUE_ID = 486;

//        public ViewerResourceInfo(PT.Common.IReader reader)
//        {
//            if (reader.VersionNumber >= 444)
//            {
//                //serialization version doesn't matter since these don't get stored.  They're temporary in the UI only.
//                plantId = new BaseId(reader);
//                deptId = new BaseId(reader);
//                resourceId = new BaseId(reader);
//                reader.Read(out plantName);
//                reader.Read(out deptName);
//                reader.Read(out resourceName);
//                reader.Read(out resourceDescription);
//                reader.Read(out headStartSpan);
//                int val;
//                reader.Read(out val);
//                this.capacityType = (InternalResourceDefs.capacityTypes)val;
//                reader.Read(out canUpdateStatus);
//                reader.Read(out canResequence);
//                reader.Read(out canReassign);
//                bool haveResourceOptions;
//                reader.Read(out haveResourceOptions);
//                if (haveResourceOptions)
//                {
//                    this.resourceOptions = new ShopViewResourceOptions(reader);
//                }

//                int blockCount;
//                reader.Read(out blockCount);
//                for (int i = 0; i < blockCount; i++)
//                {
//                    BlockInfo blockInfo = new BlockInfo(reader);
//                    this.AddBlockInfo(blockInfo);
//                }

//                int warehouseInfoCount;
//                reader.Read(out warehouseInfoCount);
//                for (int i = 0; i < warehouseInfoCount; i++)
//                {
//                    warehouseInfos.Add(new WarehouseInfo(reader));
//                }

//                int capacityIntervalCount;
//                reader.Read(out capacityIntervalCount);
//                for (int ci = 0; ci < capacityIntervalCount; ci++)
//                {
//                    _capacityIntervalInfos.Add(new CapacityIntervalInfo(reader));
//                }
//            }
//                #region Version 1
//            else
//            {
//                //serialization version doesn't matter since these don't get stored.  They're temporary in the UI only.
//                plantId = new BaseId(reader);
//                deptId = new BaseId(reader);
//                resourceId = new BaseId(reader);
//                reader.Read(out plantName);
//                reader.Read(out deptName);
//                reader.Read(out resourceName);
//                reader.Read(out resourceDescription);
//                reader.Read(out headStartSpan);
//                int val;
//                reader.Read(out val);
//                this.capacityType = (InternalResourceDefs.capacityTypes)val;
//                reader.Read(out canUpdateStatus);
//                reader.Read(out canResequence);
//                reader.Read(out canReassign);
//                bool haveResourceOptions;
//                reader.Read(out haveResourceOptions);
//                if (haveResourceOptions)
//                {
//                    this.resourceOptions = new ShopViewResourceOptions(reader);
//                }

//                int blockCount;
//                reader.Read(out blockCount);
//                for (int i = 0; i < blockCount; i++)
//                {
//                    BlockInfo blockInfo = new BlockInfo(reader);
//                    this.AddBlockInfo(blockInfo);
//                }

//                int warehouseInfoCount;
//                reader.Read(out warehouseInfoCount);
//                for (int i = 0; i < warehouseInfoCount; i++)
//                {
//                    warehouseInfos.Add(new WarehouseInfo(reader));
//                }
//            }
//            #endregion Version 1
//        }

//        public void Serialize(PT.Common.IWriter writer)
//        {
//            plantId.Serialize(writer);
//            deptId.Serialize(writer);
//            resourceId.Serialize(writer);
//            writer.Write(plantName);
//            writer.Write(deptName);
//            writer.Write(resourceName);
//            writer.Write(resourceDescription);
//            writer.Write(headStartSpan);
//            writer.Write((int)capacityType);
//            writer.Write(canUpdateStatus);
//            writer.Write(canResequence);
//            writer.Write(canReassign);
//            writer.Write(this.resourceOptions != null); //can be null
//            if (this.resourceOptions != null)
//            {
//                this.resourceOptions.Serialize(writer);
//            }

//            writer.Write(this.BlockCount);
//            for (int i = 0; i < this.BlockCount; i++)
//            {
//                this.GetBlockInfo(i).Serialize(writer);
//            }

//            writer.Write(warehouseInfos.Count);
//            for (int i = 0; i < warehouseInfos.Count; i++)
//            {
//                warehouseInfos[i].Serialize(writer);
//            }

//            writer.Write(_capacityIntervalInfos.Count);
//            for (int ci = 0; ci < _capacityIntervalInfos.Count; ci++)
//            {
//                _capacityIntervalInfos[ci].Serialize(writer);
//            }
//        }

//        public int UniqueId
//        {
//            get { return UNIQUE_ID; }
//        }
//        #endregion

//        public ViewerResourceInfo(Resource resource)
//        {
//            this.plantId = resource.Department.Plant.Id;
//            this.deptId = resource.Department.Id;
//            this.resourceId = resource.Id;

//            this.plantName = resource.Department.Plant.Name;
//            this.deptName = resource.Department.Name;
//            this.resourceName = resource.Name;
//            this.resourceDescription = resource.Description;
//            this.headStartSpan = resource.HeadStartSpan;
//            this.capacityType = resource.CapacityType;

//            AddBlocks(resource);

//            AddCapacityIntervals(resource);

//            this.resourceOptions = resource.ShopViewResourceOptions.Clone();

//            //Add Warehouse that this Resource's Plants are tied to.
//            for (int wI = 0; wI < resource.Department.Plant.WarehouseCount; wI++)
//            {
//                warehouseInfos.Add(new WarehouseInfo(resource.Department.Plant.GetWarehouseAtIndex(wI)));
//            }
//        }

//        #region Resource definition
//        private string plantName;

//        public string PlantName
//        {
//            get { return this.plantName; }
//            set { this.plantName = value; }
//        }

//        private BaseId plantId;

//        public BaseId PlantId
//        {
//            get { return this.plantId; }
//            set { this.plantId = value; }
//        }

//        private string deptName;

//        public string DepartmentName
//        {
//            get { return this.deptName; }
//            set { this.deptName = value; }
//        }

//        private BaseId deptId;

//        public BaseId DepartmentId
//        {
//            get { return this.deptId; }
//            set { this.deptId = value; }
//        }

//        private string resourceName;

//        public string ResourceName
//        {
//            get { return this.resourceName; }
//            set { this.resourceName = value; }
//        }

//        private BaseId resourceId;

//        public BaseId ResourceId
//        {
//            get { return this.resourceId; }
//            set { this.resourceId = value; }
//        }

//        private string resourceDescription;

//        public string ResourceDescription
//        {
//            get { return this.resourceDescription; }
//            set { this.resourceDescription = value; }
//        }

//        private TimeSpan headStartSpan;

//        public TimeSpan HeadStartSpan
//        {
//            get { return this.headStartSpan; }
//            set { this.headStartSpan = value; }
//        }

//        private InternalResourceDefs.capacityTypes capacityType;

//        public InternalResourceDefs.capacityTypes CapacityType
//        {
//            get { return this.capacityType; }
//            set { this.capacityType = value; }
//        }

//        private readonly List<WarehouseInfo> warehouseInfos = new List<WarehouseInfo>();

//        public List<WarehouseInfo> WarehouseInfos
//        {
//            get { return warehouseInfos; }
//        }

//        public class WarehouseInfo : PT.Common.IPTSerializable
//        {
//            #region IPTSerializable Members
//            public const int UNIQUE_ID = 702;

//            public WarehouseInfo(PT.Common.IReader reader)
//            {
//                if (reader.VersionNumber >= 1)
//                {
//                    Id = new BaseId(reader);
//                    reader.Read(out Name);
//                }
//            }

//            public void Serialize(PT.Common.IWriter writer)
//            {
//                Id.Serialize(writer);
//                writer.Write(Name);
//            }

//            public int UniqueId
//            {
//                get { return UNIQUE_ID; }
//            }
//            #endregion

//            public WarehouseInfo(Warehouse warehouse)
//            {
//                Id = warehouse.Id;
//                Name = warehouse.Name;
//            }

//            public BaseId Id;
//            public string Name;
//        }
//        #endregion

//        #region Rights
//        private bool canUpdateStatus = false;

//        public bool CanUpdateStatus
//        {
//            get { return this.canUpdateStatus; }
//            set { this.canUpdateStatus = value; }
//        }

//        private bool canResequence = false;

//        public bool CanResequence
//        {
//            get { return this.canResequence; }
//            set { this.canResequence = value; }
//        }

//        private bool canReassign = false;

//        public bool CanReassign
//        {
//            get { return this.canReassign; }
//            set { this.canReassign = value; }
//        }
//        #endregion

//        #region Block Infos
//        public BlockInfo FindBlockInfo(BlockKey key)
//        {
//            return (BlockInfo)blockInfos[key];
//        }

//        private void AddBlocks(Resource resource)
//        {
//            ResourceBlockList.Node node = resource.Blocks.First;
//            while (node != null)
//            {
//                BlockInfo blockInfo = new BlockInfo(node.Data);
//                AddBlockInfo(blockInfo);
//                node = node.Next;
//            }
//        }

//        private readonly SortedList blockInfos = new SortedList();

//        private void AddBlockInfo(BlockInfo blockInfo)
//        {
//            blockInfos.Add(blockInfo.BlockKey, blockInfo);
//        }

//        public int BlockCount
//        {
//            get { return blockInfos.Count; }
//        }

//        public BlockInfo GetBlockInfo(int index)
//        {
//            return (BlockInfo)blockInfos.GetByIndex(index);
//        }
//        #endregion

//        #region Options
//        private readonly ShopViewResourceOptions resourceOptions;

//        public ShopViewResourceOptions ResourceOptions
//        {
//            get { return this.resourceOptions; }
//        }
//        #endregion

//        #region Capacity
//        private readonly List<CapacityIntervalInfo> _capacityIntervalInfos = new List<CapacityIntervalInfo>();

//        public CapacityIntervalInfo GetCapacityIntervalInfo(int index)
//        {
//            return _capacityIntervalInfos[index];
//        }

//        public int CapacityIntervalInfoCount
//        {
//            get { return _capacityIntervalInfos.Count; }
//        }

//        private void AddCapacityIntervals(Resource aResource)
//        {
//            for (int i = 0; i < aResource.CapacityIntervals.Count; i++)
//            {
//                CapacityInterval ci = aResource.CapacityIntervals[i];
//                if (IncludeInList(ci))
//                {
//                    _capacityIntervalInfos.Add(new CapacityIntervalInfo(ci));
//                }
//            }

//            for (int i = 0; i < aResource.RecurringCapacityIntervals.Count; i++)
//            {
//                RecurringCapacityInterval rci = aResource.RecurringCapacityIntervals[i];
//                if (IncludeInList(rci))
//                {
//                    foreach(RecurringCapacityInterval.RCIExpansion expansion in rci)
//                    {
//                        _capacityIntervalInfos.Add(new CapacityIntervalInfo(rci, expansion));
//                    }
//                }
//            }
//        }

//        private bool IncludeInList(CapacityInterval aCi)
//        {
//            return aCi.IntervalType == CapacityIntervalDefs.capacityIntervalTypes.Cleanout || aCi.IntervalType == CapacityIntervalDefs.capacityIntervalTypes.Offline || aCi.IntervalType == CapacityIntervalDefs.capacityIntervalTypes.Overtime;
//        }
//        #endregion Capacity
//    }

//    public class BlockInfo
//    {
//        #region IPTSerializable Members
//        public const int UNIQUE_ID = 488;

//        public BlockInfo(PT.Common.IReader reader)
//        { 
//            if (reader.VersionNumber >= 714)
//            {
//                m_bools = new BoolVector32(reader);
//                blockKey = new BlockKey(reader);
//                jobId = new BaseId(reader);
//                moId = new BaseId(reader);
//                opId = new BaseId(reader);
//                activityId = new BaseId(reader);
//                blockId = new BaseId(reader);
//                reader.Read(out activityStart);
//                reader.Read(out activityEnd);
//                reader.Read(out jobExternalId);
//                reader.Read(out moExternalId);
//                reader.Read(out opExternalId);
//                reader.Read(out activityExternalId);
//                reader.Read(out jobName);
//                reader.Read(out moName);
//                reader.Read(out opName);
//                reader.Read(out jobDescription);
//                reader.Read(out moDescription);
//                reader.Read(out opDescription);
//                reader.Read(out product);
//                reader.Read(out productDescription);
//                reader.Read(out int val);
//                productionStatus = (InternalActivityDefs.productionStatuses)val;
//                reader.Read(out activityPercentFinished);
//                reader.Read(out jobNeedDate);
//                reader.Read(out moNeedDate);
//                reader.Read(out opNeedDate);
//                reader.Read(out setupSpan);
//                reader.Read(out runSpan);
//                reader.Read(out postProcessingSpan);
//                reader.Read(out totalHours);
//                reader.Read(out reportedSetupSpan);
//                reader.Read(out reportedRunSpan);
//                reader.Read(out reportedPostProcessingSpan);
//                reader.Read(out resourcesUsed);
//                reader.Read(out setupCode);
//                reader.Read(out setupNumber);
//                reader.Read(out setupColor);
//                reader.Read(out customer);
//                reader.Read(out orderNumber);
//                reader.Read(out priority);
//                reader.Read(out opNotes);
//                reader.Read(out jobNotes);
//                reader.Read(out requiredFinishQty);
//                reader.Read(out expectedScrapQty);
//                reader.Read(out reportedGoodQty);
//                reader.Read(out reportedScrapQty);
//                reader.Read(out latestConstraintDate);
//                reader.Read(out latestConstraint);
//                reader.Read(out nextOperationName);
//                reader.Read(out nextOperationDescription);
//                reader.Read(out nextOperationResources);
//                reader.Read(out nextOperationScheduledStart);
//                reader.Read(out activityComments);
//                reader.Read(out releaseDate);
//                reader.Read(out val);
//                commitment = (JobDefs.commitmentTypes)val;
//                reader.Read(out jobType);
//                reader.Read(out activityIsLate);
//                reader.Read(out uom);
//                reader.Read(out paused);
//                reader.Read(out onHold);
//                reader.Read(out holdReason);
//                reader.Read(out holdUntilDate);
//                reader.Read(out blockStart);
//                reader.Read(out blockEnd);
//                reader.Read(out attentionPercent);
//                reader.Read(out currentBufferPenetrationPercent);
//                reader.Read(out projectedBufferPenetrationPercent);

//                //Materials
//                reader.Read(out int materialCount);
//                for (int m = 0; m < materialCount; m++)
//                {
//                    MaterialInfo mInfo = new MaterialInfo(reader);
//                    this.AddMaterial(mInfo);
//                }

//                //Subassemblies
//                reader.Read(out int subCount);
//                for (int s = 0; s < subCount; s++)
//                {
//                    SubassemblyInfo subInfo = new SubassemblyInfo(reader);
//                    this.AddSubassembly(subInfo);
//                }

//                //UDFs
//                UDFs = new UserFieldList(reader);

//                reader.Read(out operationBatchCode);
//            }
//            else if (reader.VersionNumber >= 706)
//            {
//                m_bools = new BoolVector32(reader);
//                blockKey = new BlockKey(reader);
//                jobId = new BaseId(reader);
//                moId = new BaseId(reader);
//                opId = new BaseId(reader);
//                activityId = new BaseId(reader);
//                blockId = new BaseId(reader);
//                reader.Read(out activityStart);
//                reader.Read(out activityEnd);
//                reader.Read(out jobExternalId);
//                reader.Read(out moExternalId);
//                reader.Read(out opExternalId);
//                reader.Read(out activityExternalId);
//                reader.Read(out jobName);
//                reader.Read(out moName);
//                reader.Read(out opName);
//                reader.Read(out jobDescription);
//                reader.Read(out moDescription);
//                reader.Read(out opDescription);
//                reader.Read(out product);
//                reader.Read(out productDescription);
//                reader.Read(out int val);
//                productionStatus = (InternalActivityDefs.productionStatuses)val;
//                reader.Read(out activityPercentFinished);
//                reader.Read(out jobNeedDate);
//                reader.Read(out moNeedDate);
//                reader.Read(out opNeedDate);
//                reader.Read(out setupSpan);
//                reader.Read(out runSpan);
//                reader.Read(out postProcessingSpan);
//                reader.Read(out totalHours);
//                reader.Read(out reportedSetupSpan);
//                reader.Read(out reportedRunSpan);
//                reader.Read(out reportedPostProcessingSpan);
//                reader.Read(out resourcesUsed);
//                reader.Read(out setupCode);
//                reader.Read(out setupNumber);
//                reader.Read(out setupColor);
//                reader.Read(out customer);
//                reader.Read(out orderNumber);
//                reader.Read(out priority);
//                reader.Read(out opNotes);
//                reader.Read(out jobNotes);
//                reader.Read(out requiredFinishQty);
//                reader.Read(out expectedScrapQty);
//                reader.Read(out reportedGoodQty);
//                reader.Read(out reportedScrapQty);
//                reader.Read(out latestConstraintDate);
//                reader.Read(out latestConstraint);
//                reader.Read(out nextOperationName);
//                reader.Read(out nextOperationDescription);
//                reader.Read(out nextOperationResources);
//                reader.Read(out nextOperationScheduledStart);
//                reader.Read(out activityComments);
//                reader.Read(out releaseDate);
//                reader.Read(out val);
//                commitment = (JobDefs.commitmentTypes)val;
//                reader.Read(out jobType);
//                reader.Read(out activityIsLate);
//                reader.Read(out uom);
//                reader.Read(out paused);
//                reader.Read(out onHold);
//                reader.Read(out holdReason);
//                reader.Read(out holdUntilDate);
//                reader.Read(out blockStart);
//                reader.Read(out blockEnd);
//                reader.Read(out attentionPercent);
//                reader.Read(out currentBufferPenetrationPercent);
//                reader.Read(out projectedBufferPenetrationPercent);

//                //Materials
//                reader.Read(out int materialCount);
//                for (int m = 0; m < materialCount; m++)
//                {
//                    MaterialInfo mInfo = new MaterialInfo(reader);
//                    this.AddMaterial(mInfo);
//                }

//                //Subassemblies
//                reader.Read(out int subCount);
//                for (int s = 0; s < subCount; s++)
//                {
//                    SubassemblyInfo subInfo = new SubassemblyInfo(reader);
//                    this.AddSubassembly(subInfo);
//                }

//                //UDFs
//                UDFs = new UserFieldList(reader);
//            }
//            #region Version 705
//            else if (reader.VersionNumber >= 705)
//            {
//                blockKey = new BlockKey(reader);
//                jobId = new BaseId(reader);
//                moId = new BaseId(reader);
//                opId = new BaseId(reader);
//                activityId = new BaseId(reader);
//                blockId = new BaseId(reader);
//                reader.Read(out activityStart);
//                reader.Read(out activityEnd);
//                reader.Read(out jobExternalId);
//                reader.Read(out moExternalId);
//                reader.Read(out opExternalId);
//                reader.Read(out activityExternalId);
//                reader.Read(out jobName);
//                reader.Read(out moName);
//                reader.Read(out opName);
//                reader.Read(out jobDescription);
//                reader.Read(out moDescription);
//                reader.Read(out opDescription);
//                reader.Read(out product);
//                reader.Read(out productDescription);
//                int val;
//                reader.Read(out val);
//                productionStatus = (InternalActivityDefs.productionStatuses)val;
//                reader.Read(out activityPercentFinished);
//                reader.Read(out jobNeedDate);
//                reader.Read(out moNeedDate);
//                reader.Read(out opNeedDate);
//                reader.Read(out setupSpan);
//                reader.Read(out runSpan);
//                reader.Read(out postProcessingSpan);
//                reader.Read(out totalHours);
//                reader.Read(out reportedSetupSpan);
//                reader.Read(out reportedRunSpan);
//                reader.Read(out reportedPostProcessingSpan);
//                reader.Read(out resourcesUsed);
//                reader.Read(out setupCode);
//                reader.Read(out setupNumber);
//                reader.Read(out setupColor);
//                reader.Read(out customer);
//                reader.Read(out orderNumber);
//                reader.Read(out priority);
//                reader.Read(out opNotes);
//                reader.Read(out jobNotes);
//                reader.Read(out requiredFinishQty);
//                reader.Read(out expectedScrapQty);
//                reader.Read(out reportedGoodQty);
//                reader.Read(out reportedScrapQty);
//                reader.Read(out latestConstraintDate);
//                reader.Read(out latestConstraint);
//                reader.Read(out nextOperationName);
//                reader.Read(out nextOperationDescription);
//                reader.Read(out nextOperationResources);
//                reader.Read(out nextOperationScheduledStart);
//                reader.Read(out activityComments);
//                reader.Read(out bool releasedBool);
//                released = releasedBool;
//                reader.Read(out releaseDate);
//                reader.Read(out val);
//                commitment = (JobDefs.commitmentTypes)val;
//                reader.Read(out jobType);
//                reader.Read(out activityIsLate);
//                reader.Read(out uom);
//                reader.Read(out paused);
//                reader.Read(out onHold);
//                reader.Read(out holdReason);
//                reader.Read(out holdUntilDate);
//                reader.Read(out blockStart);
//                reader.Read(out blockEnd);
//                reader.Read(out attentionPercent);
//                reader.Read(out currentBufferPenetrationPercent);
//                reader.Read(out projectedBufferPenetrationPercent);

//                //Materials
//                int materialCount;
//                reader.Read(out materialCount);
//                for (int m = 0; m < materialCount; m++)
//                {
//                    MaterialInfo mInfo = new MaterialInfo(reader);
//                    this.AddMaterial(mInfo);
//                }

//                //Subassemblies
//                int subCount;
//                reader.Read(out subCount);
//                for (int s = 0; s < subCount; s++)
//                {
//                    SubassemblyInfo subInfo = new SubassemblyInfo(reader);
//                    this.AddSubassembly(subInfo);
//                }

//                //UDFs
//                UDFs = new UserFieldList(reader);
//            }
//            #endregion 705
//            #region Version 444
//            else if (reader.VersionNumber >= 444)
//            {
//                blockKey = new BlockKey(reader);
//                jobId = new BaseId(reader);
//                moId = new BaseId(reader);
//                opId = new BaseId(reader);
//                activityId = new BaseId(reader);
//                blockId = new BaseId(reader);
//                reader.Read(out activityStart);
//                reader.Read(out activityEnd);
//                reader.Read(out jobExternalId);
//                reader.Read(out moExternalId);
//                reader.Read(out opExternalId);
//                reader.Read(out activityExternalId);
//                reader.Read(out jobName);
//                reader.Read(out moName);
//                reader.Read(out opName);
//                reader.Read(out jobDescription);
//                reader.Read(out moDescription);
//                reader.Read(out opDescription);
//                reader.Read(out product);
//                reader.Read(out productDescription);
//                int val;
//                reader.Read(out val);
//                productionStatus = (InternalActivityDefs.productionStatuses)val;
//                reader.Read(out activityPercentFinished);
//                reader.Read(out jobNeedDate);
//                reader.Read(out moNeedDate);
//                reader.Read(out opNeedDate);
//                reader.Read(out setupSpan);
//                reader.Read(out runSpan);
//                reader.Read(out postProcessingSpan);
//                reader.Read(out totalHours);
//                reader.Read(out reportedSetupSpan);
//                reader.Read(out reportedRunSpan);
//                reader.Read(out reportedPostProcessingSpan);
//                reader.Read(out resourcesUsed);
//                reader.Read(out setupCode);
//                reader.Read(out setupNumber);
//                reader.Read(out setupColor);
//                reader.Read(out customer);
//                reader.Read(out orderNumber);
//                reader.Read(out priority);
//                reader.Read(out opNotes);
//                reader.Read(out jobNotes);
//                reader.Read(out requiredFinishQty);
//                reader.Read(out expectedScrapQty);
//                reader.Read(out reportedGoodQty);
//                reader.Read(out reportedScrapQty);
//                reader.Read(out latestConstraintDate);
//                reader.Read(out latestConstraint);
//                reader.Read(out nextOperationName);
//                reader.Read(out nextOperationDescription);
//                reader.Read(out nextOperationResources);
//                reader.Read(out nextOperationScheduledStart);
//                reader.Read(out activityComments);
//                reader.Read(out bool releasedBool);
//                released = releasedBool;
//                reader.Read(out releaseDate);
//                reader.Read(out val);
//                commitment = (JobDefs.commitmentTypes)val;
//                reader.Read(out jobType);
//                reader.Read(out activityIsLate);
//                reader.Read(out uom);
//                reader.Read(out paused);
//                reader.Read(out onHold);
//                reader.Read(out holdReason);
//                reader.Read(out holdUntilDate);
//                reader.Read(out blockStart);
//                reader.Read(out blockEnd);
//                reader.Read(out attentionPercent);
//                reader.Read(out currentBufferPenetrationPercent);
//                reader.Read(out projectedBufferPenetrationPercent);

//                //Materials
//                int materialCount;
//                reader.Read(out materialCount);
//                for (int m = 0; m < materialCount; m++)
//                {
//                    MaterialInfo mInfo = new MaterialInfo(reader);
//                    this.AddMaterial(mInfo);
//                }

//                //Subassemblies
//                int subCount;
//                reader.Read(out subCount);
//                for (int s = 0; s < subCount; s++)
//                {
//                    SubassemblyInfo subInfo = new SubassemblyInfo(reader);
//                    this.AddSubassembly(subInfo);
//                }
//            }
//            #endregion
//            #region Version 1
//            else if (reader.VersionNumber >= 1)
//            {
//                blockKey = new BlockKey(reader);
//                jobId = new BaseId(reader);
//                moId = new BaseId(reader);
//                opId = new BaseId(reader);
//                activityId = new BaseId(reader);
//                blockId = new BaseId(reader);
//                reader.Read(out activityStart);
//                reader.Read(out activityEnd);
//                reader.Read(out jobExternalId);
//                reader.Read(out moExternalId);
//                reader.Read(out opExternalId);
//                reader.Read(out activityExternalId);
//                reader.Read(out jobName);
//                reader.Read(out moName);
//                reader.Read(out opName);
//                reader.Read(out jobDescription);
//                reader.Read(out moDescription);
//                reader.Read(out opDescription);
//                reader.Read(out product);
//                reader.Read(out productDescription);
//                int val;
//                reader.Read(out val);
//                productionStatus = (InternalActivityDefs.productionStatuses)val;
//                reader.Read(out activityPercentFinished);
//                reader.Read(out jobNeedDate);
//                reader.Read(out moNeedDate);
//                reader.Read(out opNeedDate);
//                reader.Read(out setupSpan);
//                reader.Read(out runSpan);
//                reader.Read(out postProcessingSpan);
//                reader.Read(out totalHours);
//                reader.Read(out reportedSetupSpan);
//                reader.Read(out reportedRunSpan);
//                reader.Read(out reportedPostProcessingSpan);
//                reader.Read(out resourcesUsed);
//                reader.Read(out setupCode);
//                reader.Read(out setupNumber);
//                reader.Read(out setupColor);
//                reader.Read(out customer);
//                reader.Read(out orderNumber);
//                reader.Read(out priority);
//                reader.Read(out opNotes);
//                reader.Read(out jobNotes);
//                reader.Read(out requiredFinishQty);
//                reader.Read(out expectedScrapQty);
//                reader.Read(out reportedGoodQty);
//                reader.Read(out reportedScrapQty);
//                reader.Read(out latestConstraintDate);
//                reader.Read(out latestConstraint);
//                reader.Read(out nextOperationName);
//                reader.Read(out nextOperationDescription);
//                reader.Read(out nextOperationResources);
//                reader.Read(out nextOperationScheduledStart);
//                reader.Read(out activityComments);
//                reader.Read(out bool releasedBool);
//                released = releasedBool;
//                reader.Read(out releaseDate);
//                reader.Read(out val);
//                commitment = (JobDefs.commitmentTypes)val;
//                reader.Read(out jobType);
//                reader.Read(out activityIsLate);
//                reader.Read(out uom);
//                reader.Read(out paused);
//                reader.Read(out onHold);
//                reader.Read(out holdReason);
//                reader.Read(out holdUntilDate);
//                reader.Read(out blockStart);
//                reader.Read(out blockEnd);
//                reader.Read(out attentionPercent);

//                //Materials
//                int materialCount;
//                reader.Read(out materialCount);
//                for (int m = 0; m < materialCount; m++)
//                {
//                    MaterialInfo mInfo = new MaterialInfo(reader);
//                    this.AddMaterial(mInfo);
//                }

//                //Subassemblies
//                int subCount;
//                reader.Read(out subCount);
//                for (int s = 0; s < subCount; s++)
//                {
//                    SubassemblyInfo subInfo = new SubassemblyInfo(reader);
//                    this.AddSubassembly(subInfo);
//                }
//            }
//            #endregion
//        }

//        public void Serialize(PT.Common.IWriter writer)
//        {
//            m_bools.Serialize(writer);
//            blockKey.Serialize(writer);
//            jobId.Serialize(writer);
//            moId.Serialize(writer);
//            opId.Serialize(writer);
//            activityId.Serialize(writer);
//            blockId.Serialize(writer);
//            writer.Write(activityStart);
//            writer.Write(activityEnd);
//            writer.Write(jobExternalId);
//            writer.Write(moExternalId);
//            writer.Write(opExternalId);
//            writer.Write(activityExternalId);
//            writer.Write(jobName);
//            writer.Write(moName);
//            writer.Write(opName);
//            writer.Write(jobDescription);
//            writer.Write(moDescription);
//            writer.Write(opDescription);
//            writer.Write(product);
//            writer.Write(productDescription);
//            writer.Write((int)productionStatus);
//            writer.Write(activityPercentFinished);
//            writer.Write(jobNeedDate);
//            writer.Write(moNeedDate);
//            writer.Write(opNeedDate);
//            writer.Write(setupSpan);
//            writer.Write(runSpan);
//            writer.Write(postProcessingSpan);
//            writer.Write(totalHours);
//            writer.Write(reportedSetupSpan);
//            writer.Write(reportedRunSpan);
//            writer.Write(reportedPostProcessingSpan);
//            writer.Write(resourcesUsed);
//            writer.Write(setupCode);
//            writer.Write(setupNumber);
//            writer.Write(setupColor);
//            writer.Write(customer);
//            writer.Write(orderNumber);
//            writer.Write(priority);
//            writer.Write(opNotes);
//            writer.Write(jobNotes);
//            writer.Write(requiredFinishQty);
//            writer.Write(expectedScrapQty);
//            writer.Write(reportedGoodQty);
//            writer.Write(reportedScrapQty);
//            writer.Write(latestConstraintDate);
//            writer.Write(latestConstraint);
//            writer.Write(nextOperationName);
//            writer.Write(nextOperationDescription);
//            writer.Write(nextOperationResources);
//            writer.Write(nextOperationScheduledStart);
//            writer.Write(activityComments);
//            writer.Write(releaseDate);
//            writer.Write((int)commitment);
//            writer.Write(jobType);
//            writer.Write(activityIsLate);
//            writer.Write(uom);
//            writer.Write(paused);
//            writer.Write(onHold);
//            writer.Write(holdReason);
//            writer.Write(holdUntilDate);
//            writer.Write(blockStart);
//            writer.Write(blockEnd);
//            writer.Write(attentionPercent);
//            writer.Write(currentBufferPenetrationPercent);
//            writer.Write(projectedBufferPenetrationPercent);

//            //Materials
//            writer.Write(this.MaterialCount);
//            for (int m = 0; m < this.MaterialCount; m++)
//            {
//                this.GetMaterial(m).Serialize(writer);
//            }

//            //Subassemblies
//            writer.Write(this.SubassemblyCount);
//            for (int s = 0; s < this.SubassemblyCount; s++)
//            {
//                this.GetSubassembly(s).Serialize(writer);
//            }

//            //UDFs
//            UDFs.Serialize(writer);

//            writer.Write(operationBatchCode);
//        }

//        public int UniqueId
//        {
//            get { return UNIQUE_ID; }
//        }
//        #endregion

//        public BlockInfo(ResourceBlock block)
//        {
//            InternalActivity act = block.Batch.FirstActivity;
//            this.blockKey = new BlockKey(act.Operation.ManufacturingOrder.Job.Id, act.Operation.ManufacturingOrder.Id, act.Operation.Id, act.Id, block.Id);

//            InternalOperation tightestSuccessor = (InternalOperation)act.Operation.GetTightestSuccessor();
//            DateTime successorUsage = PTDateTime.MaxDateTime;
//            string successorName = "";
//            string successorDescription = "";
//            string successorResources = "";
//            if (tightestSuccessor != null)
//            {
//                successorName = tightestSuccessor.Name;
//                successorResources = tightestSuccessor.ResourcesUsed;
//                successorUsage = tightestSuccessor.StartDateTime;
//            }

//            jobId = act.Operation.ManufacturingOrder.Job.Id;
//            moId = act.Operation.ManufacturingOrder.Id;
//            opId = act.Operation.Id;
//            activityId = act.Id;
//            blockId = block.Id;
//            activityStart = act.ScheduledStartDate;
//            activityEnd = act.ScheduledEndDate;
//            jobExternalId = act.Operation.ManufacturingOrder.Job.ExternalId;
//            moExternalId = act.Operation.ManufacturingOrder.ExternalId;
//            opExternalId = act.Operation.ExternalId;
//            activityExternalId = act.ExternalId;
//            jobName = act.Operation.ManufacturingOrder.Job.Name;
//            moName = act.Operation.ManufacturingOrder.Name;
//            opName = act.Operation.Name;
//            jobDescription = act.Operation.ManufacturingOrder.Job.Description;
//            moDescription = act.Operation.ManufacturingOrder.Description;
//            opDescription = act.Operation.Description;
//            product = act.Operation.ManufacturingOrder.ProductName;
//            productDescription = act.Operation.ManufacturingOrder.ProductDescription;
//            productionStatus = act.ProductionStatus;
//            activityPercentFinished = act.PercentFinished;
//            jobNeedDate = act.Operation.ManufacturingOrder.Job.NeedDateTime;
//            moNeedDate = act.Operation.ManufacturingOrder.NeedDate;
//            opNeedDate = act.Operation.NeedDate;
//            setupSpan = act.ScheduledSetupSpan;
//            runSpan = act.ScheduledRunSpan;
//            postProcessingSpan = act.ScheduledPostProcessingSpan;
//            totalHours = act.WorkContent;
//            reportedSetupSpan = act.ReportedSetupSpan;
//            reportedRunSpan = act.ReportedRunSpan;
//            reportedPostProcessingSpan = act.ReportedPostProcessingSpan;
//            resourcesUsed = act.ResourcesUsed;
//            setupCode = act.Operation.SetupCode;
//            setupNumber = act.Operation.SetupNumber;
//            setupColor = act.Operation.SetupColor;
//            customer = act.Operation.ManufacturingOrder.Job.Customers.GetCustomerNamesList();
//            orderNumber = act.Operation.ManufacturingOrder.Job.OrderNumber;
//            priority = act.Operation.ManufacturingOrder.Job.Priority;
//            opNotes = act.Operation.Notes;
//            jobNotes = act.Operation.ManufacturingOrder.Job.Notes;
//            requiredFinishQty = act.RequiredFinishQty;
//            expectedScrapQty = act.Operation.ExpectedScrapQty;
//            reportedGoodQty = act.ReportedGoodQty;
//            reportedScrapQty = act.ReportedScrapQty;
//            latestConstraintDate = act.Operation.LatestConstraintDate;
//            latestConstraint = act.Operation.LatestConstraint;
//            nextOperationName = successorName;
//            nextOperationDescription = successorDescription;
//            nextOperationResources = successorResources;
//            nextOperationScheduledStart = successorUsage;
//            activityComments = act.Comments;
//            released = act.Operation.ManufacturingOrder.IsReleased;
//            releaseDate = act.Operation.ManufacturingOrder.ReleaseDateTime;
//            commitment = act.Operation.ManufacturingOrder.Job.Commitment;
//            jobType = act.Operation.ManufacturingOrder.Job.Type;
//            activityIsLate = act.Late;
//            uom = act.Operation.UOM;
//            paused = act.Paused;
//            onHold = act.Operation.OnHold;
//            holdReason = act.Operation.HoldReason;
//            holdUntilDate = act.Operation.HoldUntil;
//            blockStart = block.StartDateTime;
//            blockEnd = block.EndDateTime;
//            attentionPercent = block.SatisfiedRequirement.AttentionPercent;
//            currentBufferPenetrationPercent = (int)Math.Round(act.Operation.CurrentBufferPenetrationPercent, 0);
//            projectedBufferPenetrationPercent = (int)Math.Round(act.Operation.ProjectedBufferPenetrationPercent, 0);
//            JitStart = act.Operation.JITStartDate;
//            Hot = act.Operation.ManufacturingOrder.Job.Hot;
//            OperationBatchCode = act.Operation.BatchCode;

//            //Add Materials
//            for (int mI = 0; mI < act.Operation.MaterialRequirements.Count; mI++)
//            {
//                MaterialRequirement mr = act.Operation.MaterialRequirements[mI];
//                AddMaterial(new MaterialInfo(mr));
//            }

//            //Add UDFs
//            UDFs = new UserFieldList();
//            UserFieldList opUdfs = act.Operation.UserFields;
//            if (opUdfs != null)
//            {
//                for (var i = 0; i < opUdfs.Count; i++)
//                {
//                    UserField udf = opUdfs[i];
//                    UDFs.Add(udf);
//                }
//            }

//            UserFieldList jobUdfs = act.Operation.ManufacturingOrder.Job.UserFields;
//            if (jobUdfs != null)
//            {
//                for (var i = 0; i < jobUdfs.Count; i++)
//                {
//                    UserField udf = jobUdfs[i];
//                    if (!UDFs.Contains(udf.Name))
//                    {
//                        UDFs.Add(udf);
//                    }
//                }
//            }
//        }

//        private readonly BlockKey blockKey;

//        public BlockKey BlockKey
//        {
//            get { return this.blockKey; }
//        }

//        #region Fields
//        public BaseId jobId;
//        public BaseId moId;
//        public BaseId opId;
//        public BaseId activityId;
//        public BaseId blockId;
//        public DateTime activityStart;
//        public DateTime activityEnd;
//        public string jobExternalId;
//        public string moExternalId;
//        public string opExternalId;
//        public string activityExternalId;
//        public string jobName;
//        public string moName;
//        public string opName;
//        public string jobDescription;
//        public string moDescription;
//        public string opDescription;
//        public string product;
//        public string productDescription;
//        public InternalActivityDefs.productionStatuses productionStatus;
//        public int activityPercentFinished;
//        public DateTime jobNeedDate;
//        public DateTime moNeedDate;
//        public DateTime opNeedDate;
//        public TimeSpan setupSpan;
//        public TimeSpan runSpan;
//        public TimeSpan postProcessingSpan;
//        public TimeSpan totalHours;
//        public TimeSpan reportedSetupSpan;
//        public TimeSpan reportedRunSpan;
//        public TimeSpan reportedPostProcessingSpan;
//        public string resourcesUsed;
//        public string setupCode;
//        public decimal setupNumber;
//        public Color setupColor;
//        public string customer;
//        public string orderNumber;
//        public int priority;
//        public string opNotes;
//        public string jobNotes;
//        public decimal requiredFinishQty;
//        public decimal expectedScrapQty;
//        public decimal reportedGoodQty;
//        public decimal reportedScrapQty;
//        public DateTime latestConstraintDate;
//        public string latestConstraint;
//        public string nextOperationName;
//        public string nextOperationDescription;
//        public string nextOperationResources;
//        public DateTime nextOperationScheduledStart;
//        public string activityComments;
//        public DateTime releaseDate;
//        public JobDefs.commitmentTypes commitment;
//        public string jobType;
//        public bool activityIsLate;
//        public string uom;
//        public bool paused;
//        public bool onHold;
//        public string holdReason;
//        public DateTime holdUntilDate;
//        public DateTime blockStart;
//        public DateTime blockEnd;
//        public int attentionPercent;
//        public int currentBufferPenetrationPercent;
//        public int projectedBufferPenetrationPercent;
//        public DateTime JitStart;
//        public UserFieldList UDFs = new UserFieldList();
//        BoolVector32 m_bools;
//        public string operationBatchCode;

//        private const short c_releasedIdx = 0;
//        private const short c_hotIdx = 1;

//        public bool released
//        {
//            get { return m_bools[c_releasedIdx]; }
//            set { m_bools[c_releasedIdx] = value; }
//        }

//        public bool Hot
//        {
//            get { return m_bools[c_hotIdx]; }
//            set { m_bools[c_hotIdx] = value; }
//        }

//        public string OperationBatchCode
//        {
//            get { return operationBatchCode; }
//            set { operationBatchCode = value; }
//        }
//        #endregion

//        #region Materials
//        private readonly ArrayList materials = new ArrayList();

//        public MaterialInfo GetMaterial(int index)
//        {
//            return (MaterialInfo)this.materials[index];
//        }

//        public int MaterialCount
//        {
//            get { return materials.Count; }
//        }

//        public void AddMaterial(MaterialInfo material)
//        {
//            materials.Add(material);
//        }
//        #endregion

//        #region Subassemblies
//        private readonly ArrayList subassemblies = new ArrayList();

//        public SubassemblyInfo GetSubassembly(int index)
//        {
//            return (SubassemblyInfo)this.subassemblies[index];
//        }

//        public int SubassemblyCount
//        {
//            get { return subassemblies.Count; }
//        }

//        public void AddSubassembly(SubassemblyInfo subInfo)
//        {
//            subassemblies.Add(subInfo);
//        }
//        #endregion
//    }

//    public class MaterialInfo : PT.Common.IPTSerializable
//    {
//        #region IPTSerializable Members
//        public const int UNIQUE_ID = 489;

//        public MaterialInfo(PT.Common.IReader reader)
//        {
//            if (reader.VersionNumber >= 1)
//            {
//                materialRequirementId = new BaseId(reader);
//                reader.Read(out materialName);
//                reader.Read(out materialDescription);
//                reader.Read(out available);
//                reader.Read(out availableDate);
//                reader.Read(out issued);
//                reader.Read(out issuedQty);
//                reader.Read(out totalRequiredQty);
//                reader.Read(out uom);
//                reader.Read(out haveFromWarehouseId);
//                if (haveFromWarehouseId)
//                {
//                    fromWarehouseId = new BaseId(reader);
//                }
//            }
//        }

//        public void Serialize(PT.Common.IWriter writer)
//        {
//            materialRequirementId.Serialize(writer);
//            writer.Write(materialName);
//            writer.Write(materialDescription);
//            writer.Write(available);
//            writer.Write(availableDate);
//            writer.Write(issued);
//            writer.Write(issuedQty);
//            writer.Write(totalRequiredQty);
//            writer.Write(uom);
//            writer.Write(haveFromWarehouseId);
//            if (haveFromWarehouseId)
//            {
//                fromWarehouseId.Serialize(writer);
//            }
//        }

//        public int UniqueId
//        {
//            get { return UNIQUE_ID; }
//        }
//        #endregion

//        public MaterialInfo(MaterialRequirement m)
//        {
//            materialRequirementId = m.Id;
//            materialName = m.MaterialName;
//            materialDescription = m.MaterialDescription;
//            available = m.Available;
//            availableDate = m.AvailableDateTime;
//            issued = m.IssuedComplete;
//            issuedQty = m.IssuedQty;
//            totalRequiredQty = m.TotalRequiredQty;
//            uom = m.UOM;
//            if (m.Warehouse != null)
//            {
//                haveFromWarehouseId = true;
//                fromWarehouseId = m.Warehouse.Id;
//            }
//            else
//            {
//                haveFromWarehouseId = false;
//            }
//        }

//        public BaseId materialRequirementId;
//        public string materialName;
//        public string materialDescription;
//        public bool available;
//        public DateTime availableDate;
//        public bool issued;
//        public decimal issuedQty;
//        public decimal totalRequiredQty;
//        public string uom;
//        public bool haveFromWarehouseId;
//        public BaseId fromWarehouseId;
//    }

//    public class SubassemblyInfo
//    {
//        #region IPTSerializable Members
//        public const int UNIQUE_ID = 487;

//        public SubassemblyInfo(PT.Common.IReader reader)
//        {
//            if (reader.VersionNumber >= 1)
//            {
//                reader.Read(out supplyingJobName);
//                reader.Read(out supplyingMoName);
//                reader.Read(out productName);
//                reader.Read(out productDescription);
//                reader.Read(out available);
//                reader.Read(out availableDate);
//            }
//        }

//        public void Serialize(PT.Common.IWriter writer)
//        {
//            writer.Write(supplyingJobName);
//            writer.Write(supplyingMoName);
//            writer.Write(productName);
//            writer.Write(productDescription);
//            writer.Write(available);
//            writer.Write(availableDate);
//        }

//        public int UniqueId
//        {
//            get { return UNIQUE_ID; }
//        }
//        #endregion

//        public SubassemblyInfo()
//        {
//        }

//        public string supplyingJobName;
//        public string supplyingMoName;
//        public string productName;
//        public string productDescription;
//        public bool available;
//        public DateTime availableDate;
//    }

//    public class CapacityIntervalInfo
//    {
//        #region IPTSerializable Members
//        public const int UNIQUE_ID = 766;

//        public CapacityIntervalInfo(PT.Common.IReader reader)
//        {
//            if (reader.VersionNumber >= 1)
//            {
//                reader.Read(out Name);
//                reader.Read(out Description);
//                reader.Read(out Start);
//                reader.Read(out End);
//                reader.Read(out DurationHrs);
//                reader.Read(out Notes);
//                int intervalTypeTemp;
//                reader.Read(out intervalTypeTemp);
//                IntervalType = (PT.SchedulerDefinitions.CapacityIntervalDefs.capacityIntervalTypes)intervalTypeTemp;
//                int recurrenceTemp;
//                reader.Read(out recurrenceTemp);
//                Recurrence = (CapacityIntervalDefs.recurrences)recurrenceTemp;
//            }
//        }

//        public void Serialize(PT.Common.IWriter writer)
//        {
//            writer.Write(Name);
//            writer.Write(Description);
//            writer.Write(Start);
//            writer.Write(End);
//            writer.Write(DurationHrs);
//            writer.Write(Notes);
//            writer.Write((int)IntervalType);
//            writer.Write((int)Recurrence);
//        }

//        public int UniqueId
//        {
//            get { return UNIQUE_ID; }
//        }
//        #endregion

//        public CapacityIntervalInfo(CapacityInterval aCapacityInterval)
//        {
//            this.Name = aCapacityInterval.Name;
//            this.Description = aCapacityInterval.Description;
//            this.Start = aCapacityInterval.StartDateTime;
//            this.End = aCapacityInterval.EndDateTime;
//            this.DurationHrs = aCapacityInterval.Duration.TotalHours;
//            this.Notes = aCapacityInterval.Notes;
//            this.IntervalType = aCapacityInterval.IntervalType;
//        }

//        public CapacityIntervalInfo(RecurringCapacityInterval aCapacityInterval, RecurringCapacityInterval.RCIExpansion aExpansion)
//        {
//            this.Name = aCapacityInterval.Name;
//            this.Description = aCapacityInterval.Description;
//            this.Start = aExpansion.Start;
//            this.End = aExpansion.End;
//            this.DurationHrs = aExpansion.GetDuration().TotalHours;
//            this.Notes = aCapacityInterval.Notes;
//            this.IntervalType = aCapacityInterval.IntervalType;
//            this.Recurrence = aCapacityInterval.Recurrence;
//        }

//        public string Name;
//        public string Description;
//        public DateTime Start;
//        public DateTime End;
//        public double DurationHrs;
//        public string Notes;
//        public PT.SchedulerDefinitions.CapacityIntervalDefs.capacityIntervalTypes IntervalType;
//        public CapacityIntervalDefs.recurrences Recurrence;
//    }
//}

