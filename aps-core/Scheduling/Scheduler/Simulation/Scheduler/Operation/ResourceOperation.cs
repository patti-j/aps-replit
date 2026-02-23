using PT.Scheduler.Schedule.Operation;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// An Operation that is performed on a Machine.
/// </summary>
public partial class ResourceOperation
{
    //		// These are the resources that are available across all eligible plants
    //		EligibleResourceSet plantsResourceEligibiltySet=new EligibleResourceSet();
    //
    //		[Browsable(false)]
    //		public EligibleResourceSet PlantsResourceEligibiltySet
    //		{
    //			get
    //			{
    //				return plantsResourceEligibiltySet;
    //			}
    //		}
    //
    //		internal void CreateResourceEligibilitySet()
    //		{
    //			plantsResourceEligibiltySet.Clear();
    //
    //			// First find every eligible resource among all the eligible plants.
    //			for(int plantIdx=0; plantIdx<ManufacturingOrder.EligiblePlants.Count; plantIdx++)
    //			{
    //				EligiblePlant eligiblePlant=(EligiblePlant)ManufacturingOrder.EligiblePlants.GetByIndex(plantIdx);
    //				Plant plant=eligiblePlant.Plant;
    //
    //				for(int departmentIdx=0; departmentIdx<plant.Departments.Count; departmentIdx++)
    //				{
    //					Department department=plant.Departments.GetByIndex(departmentIdx);
    //
    //					for(int machineIdx=0; machineIdx<department.Resources.Count; machineIdx++)
    //					{
    //						Resource machine=department.Resources.GetByIndex(machineIdx);
    //
    //						if(IsEligible(machine))
    //						{
    //							plantsResourceEligibiltySet.Add(machine);
    //						}
    //					}
    //				}
    //			}
    //		}
    //
    //		internal PlantArrayList GetEligiblePlants()
    //		{
    //			PlantArrayList plantArrayList=new PlantArrayList();
    //			Hashtable plantHashtable=new Hashtable();
    //
    //			for(int plantsResourceEligibilitySetIdx=0; plantsResourceEligibilitySetIdx<plantsResourceEligibiltySet.Count; plantsResourceEligibilitySetIdx++)
    //			{
    //				Plant plant=plantsResourceEligibiltySet[plantsResourceEligibilitySetIdx].Department.Plant;
    //				BaseId plantId=plant.Id;
    //
    //				if(!plantHashtable.Contains(plantId))
    //				{
    //					plantHashtable.Add(plantId, null);
    //					plantArrayList.Add(plant);
    //				}
    //			}
    //
    //			return plantArrayList;
    //		}
    //
    //		public bool IsEligible(Resource machine)
    //		{
    //			bool isEligible=true;
    //
    //			if(machineCapabilityManager.Count==0)
    //			{
    //				isEligible=false;
    //			}
    //			else
    //			{
    //				for(int machineCapabilityIdx=0; machineCapabilityIdx<machineCapabilityManager.Count; machineCapabilityIdx++)
    //				{
    //					Capability machineCapability=machineCapabilityManager.GetByIndex(machineCapabilityIdx);
    //					BaseIdObject machinesCapability=machine.Capabilities.GetById(machineCapability.Id);
    //					if(machinesCapability==null)
    //					{
    //						isEligible=false;
    //						break;
    //					}
    //				}
    //			}
    //
    //			return isEligible;
    //		}
    //
    //		/// <summary>
    //		/// These are the resources that are eligible based on the 
    //		/// </summary>
    //		EligibleResourceSet effectiveResourceEligibiltySet=new EligibleResourceSet();
    //		[System.ComponentModel.Browsable(false)]
    //		public EligibleResourceSet EffectiveResourceEligibilitySet
    //		{
    //			get
    //			{
    //				return effectiveResourceEligibiltySet;
    //			}
    //		}
    //		/// <summary>
    /// The number of Resources eligible to perform this Operation, taking into consideration whether the Job and MO CanSpanPlants.
    /// </summary>
    //		public int EffectiveEligibleMachineCount
    //		{
    //			get{return this.EffectiveResourceEligibilitySet.Count;}
    //		}
    //
    //		void FindEffectiveResourcesEligibleWithinPlantSet(PlantArrayList plants)
    //		{
    //			for(int plantsResourceEligibiltySetIdx=0; plantsResourceEligibiltySetIdx<plantsResourceEligibiltySet.Count; plantsResourceEligibiltySetIdx++)
    //			{
    //				Resource machine=(Resource)plantsResourceEligibiltySet[plantsResourceEligibiltySetIdx];
    //				Plant machinesPlant=machine.Department.Plant;
    //
    //				if(plants.Contains(machinesPlant))
    //				{
    //					effectiveResourceEligibiltySet.Add(machine);
    //				}
    //			}
    //		}
    //
    //		public void CreateEffectiveResourceEligibilitySet()
    //		{
    //			CreateResourceEligibilitySet();
    //
    //			effectiveResourceEligibiltySet.Clear();
    //
    //			if(ManufacturingOrder.Job.CanSpanPlants && ManufacturingOrder.CanSpanPlants)
    //			{
    //				effectiveResourceEligibiltySet.Copy(plantsResourceEligibiltySet);
    //			}
    //			else if(ManufacturingOrder.Job.CanSpanPlants && !ManufacturingOrder.CanSpanPlants)
    //			{
    //				PlantArrayList effectiveEligiblePlants=ManufacturingOrder.EffectiveEligiblePlants;
    //				FindEffectiveResourcesEligibleWithinPlantSet(effectiveEligiblePlants);
    //			}
    //			else
    //			{
    //				PlantArrayList effectiveEligiblePlants=ManufacturingOrder.Job.EffectiveEligiblePlants;
    //				FindEffectiveResourcesEligibleWithinPlantSet(effectiveEligiblePlants);
    //			}
    //		}
    //

    /// <summary>
    /// Unschedules all the activities.
    /// </summary>
    /// <param name="earliestUnscheduleTime">returns the earliest start time among the activities that are being unscehduled.</param>
    internal override void Unschedule(bool a_clearLocks = true, bool a_removeFromBatch = true)
    {
        base.Unschedule();

        for (int activitiesI = 0; activitiesI < Activities.Count; ++activitiesI)
        {
            InternalActivity internalActivity = Activities.GetByIndex(activitiesI);

            if (internalActivity.Scheduled)
            {
                //long scheduledStart = internalActivity.GetScheduledStartTicks();
                internalActivity.Unschedule(a_clearLocks, a_removeFromBatch);
            }
        }
    }

    public override string ToString()
    {
        string str = string.Format("{0};  ResourceOperation: Name '{1}';  ExternalId '{2}';", ManufacturingOrder, Name, ExternalId);
        #if DEBUG
        str = str + string.Format("  Id={0};", Id);
        #endif
        return str;
    }

    #region JIT
    #endregion

    /// <summary>
    /// This is the total amount of capacity required to manufacture the required finish quantity
    /// taking PlanningScrapPercent into consideration.
    /// This doesn't take reported finish quantity or time into consideration.
    /// </summary>
    /// <returns></returns>
    internal long GetTotalRequiredProcessingTicks()
    {
        decimal qty = PlanningScrapPercentAdjustedQty(PlanningScrapPercent, RequiredFinishQty);
        long requiredNumberOfCycles = (long)Math.Ceiling(qty / QtyPerCycle);
        long processingTimeSpan = (long)Math.Ceiling((decimal)requiredNumberOfCycles * CycleSpanTicks);
        return processingTimeSpan;
    }

    /// <summary>
    /// ex. If the ratio is 3/2 and the quantity is 10. This function will change the required finish quantity to (3/2)*10=15.
    /// Products and Activities are also adjusted.
    /// </summary>
    /// <param name="ratio"></param>
    /// <param name="a_primaryProduct">this is the primary product on the Manufacturing Order used to ensure primary product's qty is adjusted</param>
    /// <returns> boolean for whether the activity needs to unschedule if it is no longer able to schedule on the resource</returns>
    internal bool AdjustRequiredQty(decimal a_ratio, decimal a_newRequiredMOQty, InternalActivity a_sourceOfChangeAct, Product a_primaryProduct, ProductRuleManager a_productRuleManager)
    {
        decimal requiredQtyTmp = RequiredFinishQty;
        decimal adjustedRequiredFinishQty = ScenarioDetail.ScenarioOptions.RoundQty(a_ratio * RequiredFinishQty);
        if (ScenarioDetail.ScenarioOptions.IsApproximatelyZeroOrLess(adjustedRequiredFinishQty))
        {
            throw new SplitRoundingException("4088", new object[] { Job.ExternalId, ManufacturingOrder.ExternalId, ExternalId });
        }

        decimal adjustedStandardHours = ScenarioDetail.ScenarioOptions.RoundQty(a_ratio * StandardHours);
        if (ScenarioDetail.ScenarioOptions.IsApproximatelyZeroOrLess(adjustedStandardHours))
        {
            throw new SplitRoundingException("4088", new object[] { Job.ExternalId, ManufacturingOrder.ExternalId, ExternalId });
        }

        decimal adjustedStandardSetupTmp = (decimal)StandardSetupSpan.TotalDays;
        if (adjustedStandardSetupTmp > 0)
        {
            adjustedStandardSetupTmp = ScenarioDetail.ScenarioOptions.RoundQty(a_ratio * adjustedStandardSetupTmp);
            if (ScenarioDetail.ScenarioOptions.IsApproximatelyZeroOrLess(adjustedStandardSetupTmp))
            {
                throw new SplitRoundingException("4088", new object[] { Job.ExternalId, ManufacturingOrder.ExternalId, ExternalId });
            }
        }

        RequiredFinishQty = adjustedRequiredFinishQty;
        StandardHours = adjustedStandardHours;
        TimeSpan tmp = TimeSpan.FromDays((double)adjustedStandardSetupTmp);
        StandardSetupSpan = tmp;
        if (a_sourceOfChangeAct == null)
        {
            Activities.AdjustRequiredQty(a_ratio, a_newRequiredMOQty);
        }
        else
        {
            decimal qtyChange = RequiredFinishQty - requiredQtyTmp;
            decimal newActivityRequiredQuantity = a_sourceOfChangeAct.RequiredFinishQty + qtyChange;
            a_sourceOfChangeAct.AdjustRequiredQty(newActivityRequiredQuantity);
        }

        Products.AdjustOutputQtys(a_ratio, a_newRequiredMOQty, ScenarioDetail.ScenarioOptions, a_primaryProduct);
        MaterialRequirements.AdjustQtys(a_ratio, a_newRequiredMOQty, ScenarioDetail.ScenarioOptions);

        Resource res = GetScheduledPrimaryResource();
        if (res != null)
        {
            ResourceRequirement rr = ResourceRequirements.PrimaryResourceRequirement;
            return !rr.IsEligible(res, a_productRuleManager);
        }

        return false;
    }

    /// <summary>
    /// ex. If the ratio is 3/2 and the quantity is 10. This function will change the required finish quantity to (3/2)*10=15.
    /// Products and Activities are also adjusted.
    /// </summary>
    /// <param name="a_primaryRes"></param>
    /// <param name="a_ratio"></param>
    /// <param name="a_newRequiredMOQty"></param>
    /// <param name="a_primaryProduct">this is the primary product on the Manufacturing Order used to ensure primary product's qty is adjusted</param>
    /// <param name="a_productRuleManager"></param>
    /// <returns> boolean for whether the activity needs to unschedule if it is no longer able to schedule on the resource</returns>
    internal bool AdjustRequiredQtyForStorage(decimal a_ratio, decimal a_newRequiredMOQty, Resource a_primaryRes, Product a_primaryProduct, ProductRuleManager a_productRuleManager)
    {
        decimal adjustedRequiredFinishQty = ScenarioDetail.ScenarioOptions.RoundQty(a_ratio * RequiredFinishQty);

        decimal adjustedStandardHours = ScenarioDetail.ScenarioOptions.RoundQty(a_ratio * StandardHours);
        decimal adjustedStandardSetupTmp = (decimal)StandardSetupSpan.TotalDays;
        if (adjustedStandardSetupTmp > 0)
        {
            adjustedStandardSetupTmp = ScenarioDetail.ScenarioOptions.RoundQty(a_ratio * adjustedStandardSetupTmp);
        }

        RequiredFinishQty = adjustedRequiredFinishQty;
        StandardHours = adjustedStandardHours;
        TimeSpan tmp = TimeSpan.FromDays((double)adjustedStandardSetupTmp);
        StandardSetupSpan = tmp;
        Activities.AdjustRequiredQty(a_ratio, a_newRequiredMOQty);
        Products.AdjustOutputQtys(a_ratio, a_newRequiredMOQty, ScenarioDetail.ScenarioOptions, a_primaryProduct);
        MaterialRequirements.AdjustQtys(a_ratio, a_newRequiredMOQty, ScenarioDetail.ScenarioOptions);

        if (a_primaryRes != null)
        {
            //res can be null when adjusting the MO qty back to original qty in sim init or when job fails to schedule
            ResourceRequirement rr = ResourceRequirements.PrimaryResourceRequirement;
            return rr.IsEligible(a_primaryRes, a_productRuleManager);
        }

        return true;
    }

    //#if DEBUG
    //        public override string Notes
    //        {
    //            get
    //            {
    //                System.Text.StringBuilder sb = new System.Text.StringBuilder();
    //                if (MaterialRequirements.Count > 0)
    //                {
    //                    sb.AppendLine(PT.SchedulerDefinitions.MaterialRequirementDefs.GetSupplyTypeDescription(MaterialRequirements[0].SupplySource));
    //                    sb.Append(MaterialRequirements[0].Item.Name);
    //                }
    //                else
    //                {
    //                    sb.AppendLine();
    //                }

    //                return sb.ToString();
    //            }

    //            set
    //            {
    //                base.Notes = value;
    //            }
    //        }
    //#else
    //        delete
    //#endif

    internal InternalActivity AutoSplitByMaterial(InternalActivity a_activityToSplit, Resource a_schedulingResource, MaterialRequirement a_mr, decimal a_mrQtyRequired, ScenarioOptions a_splitOptions)
    {
        decimal ratioToSplit = a_mrQtyRequired / a_mr.TotalRequiredQty;

        //Calculate a qty to split based on 
        decimal qtyToSplit = ratioToSplit * a_activityToSplit.Operation.RequiredFinishQty;
        qtyToSplit = a_splitOptions.RoundQty(qtyToSplit);

        return AutoSplitByQty(a_activityToSplit, a_schedulingResource, qtyToSplit, a_splitOptions);
    }    
    
    internal InternalActivity AutoSplitByQty(InternalActivity a_activityToSplit, Resource a_schedulingResource, decimal a_qtyRequired, ScenarioOptions a_splitOptions)
    {
        if (a_splitOptions.IsApproximatelyZero(a_qtyRequired) || a_splitOptions.IsApproximatelyZero(a_qtyRequired - a_activityToSplit.RequiredFinishQty) || a_qtyRequired >= a_activityToSplit.RequiredFinishQty)
        {
            //There is nothing to split, the remaining portion of this operation matches the qty to split
            return null;
        }

        InternalActivity newActivity = new InternalActivity(Activities.Count + 1, Activities.IdGen.NextID(), a_activityToSplit.Operation, a_activityToSplit);
        Activities.Add(newActivity);
        m_activitiesLeftToSchedule++;

        //We don't convert to cycles, we just need to set the qty to match the materials. There may be partial cycles wasted
        newActivity.RequiredFinishQty = a_activityToSplit.RequiredFinishQty - a_qtyRequired;
        a_activityToSplit.RequiredFinishQty = a_qtyRequired;

        //Split setup
        AutoSplitSetupByType(a_schedulingResource, newActivity, a_activityToSplit);

        //Update sim product arrays
        SimulationInitializationOfActivities(new[] { a_activityToSplit, newActivity });

        return newActivity;
    }

    internal InternalActivity AutoSplitByVolume(InternalActivity a_activityToSplit, Resource a_schedulingResource, decimal a_splitToScheduleVolume, AutoSplitInfo a_autoSplitInfo)
    {
        InternalActivity activityToSplit = a_activityToSplit;

        InternalActivity newActivity = new InternalActivity(Activities.Count + 1, Activities.IdGen.NextID(), activityToSplit.Operation, activityToSplit);
        Activities.Add(newActivity);
        m_activitiesLeftToSchedule++;

        decimal actQtyPerCycle = activityToSplit.GetResourceProductionInfo(a_schedulingResource).QtyPerCycle;

        //Calculate a qty to split based on 
        decimal qtyPerVolume = activityToSplit.Operation.RequiredFinishQty / activityToSplit.Operation.Products.PrimaryProduct.TotalRequiredVolume;
        decimal qtyToSplit = qtyPerVolume * a_splitToScheduleVolume;

        //Convert to cycles
        decimal numberOfCyclesNeeded = Math.Floor(qtyToSplit / actQtyPerCycle);
        if (numberOfCyclesNeeded > 0)
        {
            decimal qtyToKeep = numberOfCyclesNeeded * actQtyPerCycle;

            newActivity.RequiredFinishQty = activityToSplit.RequiredFinishQty - qtyToKeep;
            activityToSplit.RequiredFinishQty = qtyToKeep;
        }
        else
        {
            //The entire op is completed in 1 cycle, so split by qty instead
            newActivity.RequiredFinishQty = activityToSplit.RequiredFinishQty - qtyToSplit;
            activityToSplit.RequiredFinishQty = qtyToSplit;
        }

        //Split setup
        AutoSplitSetupByType(a_schedulingResource, newActivity, activityToSplit);

        //Update sim product arrays
        SimulationInitializationOfActivities(new[] { activityToSplit, newActivity });

        return newActivity;
    }    
    
    internal InternalActivity AutoSplitByQty(InternalActivity a_activityToSplit, Resource a_schedulingResource, decimal a_splitToScheduleQuantity, AutoSplitInfo a_autoSplitInfo)
    {
        InternalActivity activityToSplit = a_activityToSplit;

        InternalActivity newActivity = new InternalActivity(Activities.Count + 1, Activities.IdGen.NextID(), activityToSplit.Operation, activityToSplit);
        Activities.Add(newActivity);
        m_activitiesLeftToSchedule++;

        decimal actQtyPerCycle = activityToSplit.GetResourceProductionInfo(a_schedulingResource).QtyPerCycle;

        //Convert to cycles
        decimal numberOfCyclesNeeded = Math.Floor(a_splitToScheduleQuantity / actQtyPerCycle);
        if (numberOfCyclesNeeded > 0)
        {
            decimal qtyToKeep = numberOfCyclesNeeded * actQtyPerCycle;

            newActivity.RequiredFinishQty = activityToSplit.RequiredFinishQty - qtyToKeep;
            activityToSplit.RequiredFinishQty = qtyToKeep;
        }
        else
        {
            //The entire op is completed in 1 cycle, so split by qty instead
            newActivity.RequiredFinishQty = activityToSplit.RequiredFinishQty - a_splitToScheduleQuantity;
            activityToSplit.RequiredFinishQty = a_splitToScheduleQuantity;
        }

        //Split setup
        AutoSplitSetupByType(a_schedulingResource, newActivity, activityToSplit);

        //Update sim product arrays
        SimulationInitializationOfActivities(new[] { activityToSplit, newActivity });

        return newActivity;
    }

    private void AutoSplitSetupByType(Resource a_schedulingResource, InternalActivity a_newActivity, InternalActivity a_activityToSplit)
    {
        //Split setup
        switch (SetupSplitType)
        {
            case OperationDefs.ESetupSplitType.None:
                //Nothing to split
                break;
            case OperationDefs.ESetupSplitType.FirstActivity:
                a_newActivity.SplitSetup(0,true);
                break;
            case OperationDefs.ESetupSplitType.SplitByQty:
                //Update both the new and old activity based on the new split ratio
                a_newActivity.SplitSetup((long)ScenarioDetail.ScenarioOptions.RoundQty(a_newActivity.SplitRatio * a_newActivity.Operation.SetupSpanTicks),true);
                long originalSplitSetupTime = a_activityToSplit.GetResourceProductionInfo(a_schedulingResource).SetupSpanTicks;
                long newSetupTime = (long)ScenarioDetail.ScenarioOptions.RoundQty(a_activityToSplit.SplitRatio * a_newActivity.Operation.SetupSpanTicks);
                if (originalSplitSetupTime != newSetupTime)
                {
                    //Only set if using a new setup so we don't override the production info unnecessarily.
                    AutoSplitInfo.OriginalSplitSetupTime = originalSplitSetupTime;
                    a_activityToSplit.SplitSetup(newSetupTime,true);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal void AutoJoin(InternalActivity a_sourceActivity, InternalActivity a_splitOffActivity, long a_originalSplitSetupTime)
    {
        a_sourceActivity.RequiredFinishQty += a_splitOffActivity.RequiredFinishQty;
        //I don't think we need to remove Resource.LastRunActivity here because only finished activities get tracked there.
        Activities.Remove(a_splitOffActivity);
        m_activitiesLeftToSchedule--;
        a_sourceActivity.SplitSetup(a_originalSplitSetupTime, true,true);

        //Update sim product arrays
        SimulationInitializationOfActivities(new[] { a_sourceActivity });
    }
}