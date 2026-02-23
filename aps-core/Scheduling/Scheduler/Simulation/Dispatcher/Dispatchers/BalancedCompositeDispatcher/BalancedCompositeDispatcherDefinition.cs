using System.ComponentModel;

using PT.APSCommon;
using PT.Common.Extensions;
using PT.Scheduler.Simulation.Dispatcher.Dispatchers.BalancedCompositeDispatcher;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

//****************************************************************************************************
// Things to do:
// 1. Set limits on values used when necessary. For instance priority must be greater than or equal to 0.
// 2. Weights must be greater than or equal to 0. 
// 3. You might consider using doubles instead of decimals.
// 4. AddLatePenaltyCostWeight bug
//****************************************************************************************************
public partial class BalancedCompositeDispatcherDefinition : DispatcherDefinition, IPTSerializable
{
    #region IPTSerializable Members
    public BalancedCompositeDispatcherDefinition(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12433)
        {
            a_reader.Read(out m_notes);
            a_reader.Read(out m_globalMinScore);
            m_optimizeRuleMappings = new OptimizeRuleWeightSettings(a_reader);
        }
        else if(a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out m_notes);
            m_optimizeRuleMappings = new OptimizeRuleWeightSettings(a_reader);
        }
        else
        {
            ReadV11Weights(a_reader);
        }
    }

    private void ReadV11Weights(IReader reader)
    {
        //Obsolete weight values

        #region Old Weights
        /// <summary>
        /// Multiplied by Job Priority.  Lower values are chosen earlier.
        /// The minimum value is 0.
        /// </summary>
        decimal m_priorityWeight = 0;

        /// <summary>
        /// Multiplied by Job Priority.  Lower values are chosen earlier.
        /// </summary>
        decimal m_leastEligibleResourcesWeight = 0;

        /// <summary>
        /// Multiplied by Activity Setup Hours. Lower values are chosen earlier.
        /// </summary>
        decimal m_setupHoursWeight = 0;

        decimal m_setupCostWeight = 0;

        /// <summary>
        /// Multiplied by the absolute difference between the current Operation's Setup Number and the Setup Number of the evaluated Operation. Lower values are chosen earlier.
        /// </summary>
        decimal m_nearestSetupNbrWeight = 0;

        /// <summary>
        /// Favors selection of Operations that have a Setup Number higher than and nearer to the current Operation.
        /// </summary>
        decimal m_nearestHigherSetupNbrWeight = 0;

        /// <summary>
        /// Favors selection of Operations that have a Setup Number lower than and nearer to the current Operation.
        /// </summary>
        decimal m_nearestLowerSetupNbrWeight = 0;

        /// <summary>
        /// Favors selection of Operations that have a Setup Number closest to the current Operation's Setup Number and moving in the current direction, increasing or decreasing Setup Numbers.
        /// </summary>
        decimal m_nearestSawtoothSetupNbrWeight = 0;

        /// <summary>
        /// Multiplied by Activity Run Hours. Lower values are chosen earlier.
        /// </summary>
        decimal m_runHoursWeight = 0;

        /// <summary>
        /// Multiplied by the days between Now and the Job NeedDate. Lower values are chosen earlier.
        /// </summary>
        decimal m_jobNeedDateWeight = 100;

        /// <summary>
        /// Multiplied by the days between Now and the Job Entry Date. Lower values are chosen earlier.
        /// </summary>
        decimal m_jobEntryDateWeight = 0;

        /// <summary>
        /// Multiplied by the days between Now and the Operation NeedDate. Lower values are chosen earlier.
        /// </summary>
        decimal m_operationNeedDateWeight = 0;

        /// <summary>
        /// Multiplied by the days between Now and the Manufacturing Order NeedDate. Lower values are chosen earlier.
        /// </summary>
        decimal m_moNeedDateWeight = 0;

        /// <summary>
        /// Multiplied by the days between Now and the Activity JitStartDate. Lower values are chosen earlier.
        /// </summary>
        decimal m_activityJitStartDateWeight = 0;

        /// <summary>
        /// Preference is given to Operations that have been in Queue longer.  Multiplied by the days between Now and the Operation's Latest Constraint Date. Lower values are chosen earlier.
        /// </summary>
        decimal m_queueWeight = 0;

        /// <summary>
        /// Multiplied by the Job Revenue.  Higher values are chosen earlier.
        /// </summary>
        decimal m_revenueWeight = 0;

        /// <summary>
        /// Multiplied by the Job Profit.  Higher values are chosen earlier.
        /// </summary>
        decimal m_profitWeight = 0;

        /// <summary>
        /// Added to the balanced composite sum if the Job is Hot.  Otherwise ignored.  Higher values are chosen earlier.
        /// </summary>
        decimal m_hotWeight = 0;

        /// <summary>
        /// Multiplied by the Job Late Penalty Cost.  Higher values are chosen earlier.
        /// </summary>
        decimal m_latePenaltyCostWeight = 0;

        /// <summary>
        /// JobLatePenaltyPerDay/Activity.CritialRatio
        /// </summary>
        decimal m_lowestLatePenaltyCostWeight = 0;

        /// <summary>
        /// Added to the balanced composite sum if the Job is Firm.  Otherwise ignored.  Higher values are chosen earlier.
        /// </summary>
        decimal m_isFirmWeight = 0;

        /// <summary>
        /// Added to the balance composite sum if the setup code of the current activity and this activity are equal. Otherwise it is ignored. Higher values are chosen earlier.
        /// </summary>
        decimal m_sameSetupCodeWeight = 0;

        /// <summary>
        /// Added to the balance composite sum if the Job Customer of the current activity and this activity are equal. Otherwise it is ignored. Higher values are chosen earlier.
        /// </summary>
        decimal m_sameCustomerWeight = 0;

        /// <summary>
        /// Added to the balance composite sum if the M.O. Product Name of the current activity and this activity are equal. Otherwise it is ignored. Higher values are chosen earlier.
        /// </summary>
        decimal m_sameMoProductNameWeight = 0;

        /// <summary>
        /// Added to the balance composite sum if the M.O. Product Description of the current activity and this activity are equal. Otherwise it is ignored. Higher values are chosen earlier.
        /// </summary>
        decimal m_sameMoProductDescWeight = 0;

        /// <summary>
        /// Multiplied by CriticalRatio value of Activity. CriticalRatio=(Now until Activity Need Date)/remaining Activity work. 
        /// CriticalRatio equal to 1: Activity is on Schedule
        /// CriticalRatio smaller than 1: Activity is late 
        /// CriticalRatio greater than 1: Activity is early.
        /// </summary>
        decimal m_activityCriticalRatioWeight = 0;

        /// <summary>
        /// Activities with higher Job.Throughput value are chosen earlier.
        /// </summary>
        decimal m_throughputWeight = 0;

        /// <summary>
        /// Activities with larger values of JobThroughput/Job.DrumUsageHrs are chosen earlier.
        /// </summary>
        decimal m_throughputPerDrumHrWeight = 0;

        /// <summary>
        /// Activities with smaller Job.DrumUsageHrs are chosen earlier.
        /// </summary>
        decimal m_drumHrsWeight = 0;

        /// <summary>
        /// Activities with larger Operation.BufferPenetrationPercent are chosen earlier.
        /// </summary>
        decimal m_BufferPenetrationWeight = 0;

        /// <summary>
        /// If  the product produced by a job produces an item whose inventory can't fullfill its demands within
        /// with the span of the current simulation clock out to Inventory.DaysOnHand, this weight can be used to prefer that job.
        /// </summary>
        decimal m_daysOnHandWeight = 0;

        List<BalancedCompositeDispatcherDefinitionUpdateT.AttributeRuleInfo> m_opAttributeInfos = new ();

        /// <summary>
        /// A weight to use the in-process status of an MO in determining the next activitiy to try to schedule. Higher values are choosen earlier.
        /// This can be used to reduce work-in-process inventory by attempting to finish M.O. that have already been started instead of starting on a new M.O..
        /// This is not only based on the in-process state of the M.O. at the current time -- the future simulated in-process state will 
        /// cause operations for the same M.O. to be grouped more closely together.
        /// </summary>
        decimal m_moInProcessWeight = 0;

        /// <summary>
        /// Indicates the importance of the resource selection vs operation selection.
        /// This will enable the system to take into consideration the score on various other sliders like Least Setup Time relative to other resources. 
        /// Currently, the system chooses operations to do next strictly based on their scores relative to each other (ie. this operation is better than that one). With this new slider, the resources will also look at the other resources to see how, for example, setup time compares if an operation is run on itself versus other resources (based on their current simulated setup). 
        /// This will help to avoid situations where multiple resources incur setups for operations that could run on other resources that are already setup.
        /// In case of setup hours, it's calulated as 'setup hours on the dispatching Resource' divided by 'setup hours on the Resource with the longest setup for the Operation' times 100. 
        /// </summary>
        decimal m_resourceSelectionWeight = 0;

        /// <summary>
        /// A weight that applies for acitivities that are starting after their JIT start time.
        /// </summary>
        decimal m_activityLateWeight = 0;


        decimal m_invCarryingCostWeight;

        /// <summary>
        /// A weight that applies for acitivities that are starting after their JIT start time minus the Job's Almost Late value but before the JIT start time.
        /// </summary>
        decimal m_activityAlmostLateWeight = 0;
        #endregion

        #region Material Weights
        /// <summary>
        /// The (case-sensitve) ItemGroup of a Material Item to use when Optimizing.
        /// </summary>
        string m_materialGroup1;

        /// <summary>
        /// The (case-sensitve) ItemGroup of a Material Item to use when Optimizing.
        /// </summary>
        string m_materialGroup2;

        /// <summary>
        /// The (case-sensitve) ItemGroup of a Material Item to use when Optimizing.
        /// </summary>
        string m_materialGroup3;

        /// <summary>
        /// The (case-sensitve) ItemGroup of a Material Item to use when Optimizing.
        /// </summary>
        string m_materialGroup4;

        /// <summary>
        /// The (case-sensitve) ItemGroup of a Material Item to use when Optimizing.
        /// </summary>
        string m_materialGroup5;

        /// <summary>
        /// Added to the balance composite sum if the previous Operation uses a different Stock Material Item within this same ItemGroup. Otherwise it is ignored. Higher values are chosen earlier.
        /// </summary>
        decimal m_materialGroupWeight1 = 0;

        /// <summary>
        /// Added to the balance composite sum if the previous Operation uses a different Stock Material Item within this same ItemGroup. Otherwise it is ignored. Higher values are chosen earlier.
        /// </summary>
        decimal m_materialGroupWeight2 = 0;

        /// <summary>
        /// Added to the balance composite sum if the previous Operation uses a different Stock Material Item within this same ItemGroup. Otherwise it is ignored. Higher values are chosen earlier.
        /// </summary>
        decimal m_materialGroupWeight3 = 0;

        /// <summary>
        /// Added to the balance composite sum if the previous Operation uses a different Stock Material Item within this same ItemGroup. Otherwise it is ignored. Higher values are chosen earlier.
        /// </summary>
        decimal m_materialGroupWeight4 = 0;

        /// <summary>
        /// Added to the balance composite sum if the previous Operation uses a different Stock Material Item within this same ItemGroup. Otherwise it is ignored. Higher values are chosen earlier.
        /// </summary>
        decimal m_materialGroupWeight5 = 0;
        #endregion Material Weights

        #region ComparibleRanges
        decimal m_setupHoursRange; // = 1;
        decimal m_minSetupNbrRange; // = 11;
        decimal m_maxSetupNbrRange; // = 344;
        decimal m_maxDueDateDaysRange; // = 7;
        decimal m_maxQueueDaysRange;
        decimal m_maxEntryDateDaysRange;
        decimal m_maxOperationNeedDateDaysRange;
        decimal m_maxMONeedDateDaysRange;
        decimal m_maxDaysFromActivityJITStartDateRange;
        decimal m_maxRevenueRange;
        decimal m_maxProfitRange;
        decimal m_maxLeastEligibleResourcesRange;
        decimal m_maxRunHoursRange;
        long m_minPriorityRange;
        long m_maxPriorityRange;
        decimal m_maxLatePenaltyCostRange;
        decimal m_minThroughput;
        decimal m_maxThroughput;
        decimal m_minThroughputPerDrumUsage;
        decimal m_maxThroughputPerDrumUsage;
        decimal m_minBufferPenetrationRange;
        decimal m_maxBufferPenetrationRange;
        #endregion

        if (reader.VersionNumber >= 702)
        {
            reader.Read(out m_activityJitStartDateWeight);
            reader.Read(out m_queueWeight);
            reader.Read(out m_hotWeight);
            reader.Read(out m_isFirmWeight);
            reader.Read(out m_jobNeedDateWeight);
            reader.Read(out m_latePenaltyCostWeight);
            reader.Read(out m_operationNeedDateWeight);
            reader.Read(out m_moNeedDateWeight);
            reader.Read(out m_priorityWeight);
            reader.Read(out m_profitWeight);
            reader.Read(out m_revenueWeight);
            reader.Read(out m_runHoursWeight);
            reader.Read(out m_setupHoursWeight);
            reader.Read(out m_nearestSetupNbrWeight);
            reader.Read(out m_nearestHigherSetupNbrWeight);
            reader.Read(out m_nearestLowerSetupNbrWeight);
            reader.Read(out m_nearestSawtoothSetupNbrWeight);
            reader.Read(out m_sameSetupCodeWeight);
            int opAttCount;
            reader.Read(out opAttCount);
            for (int i = 0; i < opAttCount; i++)
            {
                m_opAttributeInfos.Add(new BalancedCompositeDispatcherDefinitionUpdateT.AttributeRuleInfo(reader));
            }

            reader.Read(out m_moInProcessWeight);
            reader.Read(out m_jobEntryDateWeight);
            reader.Read(out m_leastEligibleResourcesWeight);
            reader.Read(out m_sameCustomerWeight);
            reader.Read(out m_sameMoProductNameWeight);

            bool onlyDispatchBestComposite;
            reader.Read(out onlyDispatchBestComposite);
            OnlyDispatchBestComposites = onlyDispatchBestComposite;

            reader.Read(out m_materialGroup1);
            reader.Read(out m_materialGroup2);
            reader.Read(out m_materialGroup3);
            reader.Read(out m_materialGroup4);
            reader.Read(out m_materialGroup5);
            reader.Read(out m_materialGroupWeight1);
            reader.Read(out m_materialGroupWeight2);
            reader.Read(out m_materialGroupWeight3);
            reader.Read(out m_materialGroupWeight4);
            reader.Read(out m_materialGroupWeight5);
            reader.Read(out m_sameMoProductDescWeight);
            reader.Read(out m_notes);
            reader.Read(out m_activityCriticalRatioWeight);
            reader.Read(out m_throughputWeight);
            reader.Read(out m_throughputPerDrumHrWeight);
            reader.Read(out m_drumHrsWeight);
            reader.Read(out m_BufferPenetrationWeight);

            // Values used to map variables to comparable ranges.

            reader.Read(out m_setupHoursRange);

            reader.Read(out m_minSetupNbrRange);
            reader.Read(out m_maxSetupNbrRange);

            reader.Read(out m_maxDueDateDaysRange);
            reader.Read(out m_maxQueueDaysRange);

            reader.Read(out m_maxEntryDateDaysRange);
            reader.Read(out m_maxOperationNeedDateDaysRange);
            reader.Read(out m_maxMONeedDateDaysRange);
            reader.Read(out m_maxDaysFromActivityJITStartDateRange);

            reader.Read(out m_maxRevenueRange);
            reader.Read(out m_maxProfitRange);

            reader.Read(out m_maxLeastEligibleResourcesRange);
            reader.Read(out m_maxRunHoursRange);
            reader.Read(out m_maxPriorityRange);
            reader.Read(out m_maxLatePenaltyCostRange);
            reader.Read(out m_minPriorityRange);
            reader.Read(out m_minBufferPenetrationRange);
            reader.Read(out m_maxBufferPenetrationRange);
            reader.Read(out m_minThroughput);
            reader.Read(out m_maxThroughput);
            reader.Read(out m_minThroughputPerDrumUsage);
            reader.Read(out m_maxThroughputPerDrumUsage);
            reader.Read(out m_resourceSelectionWeight);
            reader.Read(out m_activityAlmostLateWeight);
            reader.Read(out m_activityLateWeight);

            reader.Read(out m_setupCostWeight);
            reader.Read(out m_invCarryingCostWeight);
            reader.Read(out m_lowestLatePenaltyCostWeight);
            reader.Read(out m_daysOnHandWeight);
        }

        //TODO: V12 Optimize Rules. if reader < 12000, use fixed properties to set default element mappings.
    }

    private void ReadAttributeTypes(IReader reader, PTAttributeDefs.OptimizeType[] a_optimizeTypes)
    {
        int length;
        reader.Read(out length);

        ReadAttributeTypes(reader, length, a_optimizeTypes);
    }

    private void ReadAttributeTypes(IReader reader, int a_length, PTAttributeDefs.OptimizeType[] a_optimizeTypes)
    {
        for (int i = 0; i < a_length; ++i)
        {
            int attr;
            reader.Read(out attr);
            a_optimizeTypes[i] = (PTAttributeDefs.OptimizeType)attr;
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(m_notes);
        a_writer.Write(m_globalMinScore);
        m_optimizeRuleMappings.Serialize(a_writer);
    }

    public new const int UNIQUE_ID = 300;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    private string m_notes;
    private decimal m_globalMinScore = decimal.MinValue;
    public decimal GlobalMinScore => m_globalMinScore;

    // The BoolVector32 isn't currently used because the flags are checked are checked so frequently during simulation, it's faster to use properties with auto values or member values.
    //BoolVector32 m_flags = new BoolVector32();
    //const int ONLY_DISPATCH_BEST_COMPOSITES_IDX = 0;

    /// <summary>
    /// For SingleTasking resources, when this value is set the dispatcher for this resource will only only dispatch the activities with the best
    /// balanced composite value.
    /// This is useful for integration purposes where you are trying to represent the schedule of another system within APS. For instance if you
    /// want to represent the results of a bucketed schedule you could set the bucket time as the release time of the Manfuacturing Order and set your balanced composite rule
    /// to use the bucket time for computing the balanced composite value. With this flag checked the operations in each successive bucket will be be scheduled
    /// first. For example the bucket one operations will all have the same highest balanced composite dispatch value and so will be scheduled before the operations
    /// in the second bucket.
    /// This is not compatible with the operation property EnforceKeepSuccessor.
    /// </summary>
    public bool OnlyDispatchBestComposites { get; set; }

    private readonly OptimizeRuleWeightSettings m_optimizeRuleMappings = new ();

    internal BalancedCompositeDispatcherDefinition(BaseId a_id, string a_name)
        : base(a_id, a_name) { }

    internal BalancedCompositeDispatcherDefinition(BaseId a_id)
        : base(a_id) { }

    internal BalancedCompositeDispatcherDefinition(BalancedCompositeDispatcherDefinition a_source, BaseId a_newId)
        : base(a_source, a_newId)
    {
        m_notes = a_source.m_notes;
        m_globalMinScore = a_source.GlobalMinScore;
        m_optimizeRuleMappings = a_source.m_optimizeRuleMappings.CopyInMemory();
    }

    internal void SetTempMinScoreForEndOfPlanningHorizon()
    {
        m_calculator.SetTempMinScoreForEndOfPlanningHorizon();
    }

    #region DispatcherDefinition Members
    internal override ReadyActivitiesDispatcher CreateDispatcher()
    {
        if (m_calculator.AreRulesConstant())
        {
            return new Simulation.Dispatcher.ConstantCompositeDispatcher(this);
        }

        return new BalancedCompositeDispatcher(this);
    }

    internal decimal ComputeComposite(InternalResource a_res, InternalActivity a_act, bool a_adjustForResourceSelection = true)
    {
        return m_calculator.ComputeComposite(a_res, a_act);
    }
    #endregion

    #region Score
    private void AddScore(InternalActivity a_act, string a_factorName, decimal a_score, decimal a_percentOfTotal)
    {
       // a_act.AddScore(new FactorScore(a_factorName, a_score, a_percentOfTotal));
    }
    #endregion

    private readonly BalancedCompositeCalculator m_calculator = new ();

    internal override void SimulationInitialization(ScenarioDetail a_sd)
    {
        base.SimulationInitialization(a_sd);
        m_calculator.InitWithDefinition(a_sd, m_optimizeRuleMappings, m_globalMinScore);
    }

    private readonly KeyComparer m_comparer = new ();

    internal override IComparer<KeyAndActivity> Comparer => m_comparer;

    public OptimizeRuleWeightSettings OptimizeSettings => m_optimizeRuleMappings;

    public void Receive(BalancedCompositeDispatcherDefinitionUpdateT a_updateT, IScenarioDataChanges a_dataChanges)
    {
        bool changed = false;
        if (Name != a_updateT.Name)
        {
            Name = a_updateT.Name;
            changed = true;
        }

        if (m_notes != a_updateT.Notes)
        {
            m_notes = a_updateT.Notes;
            changed = true;
        }

        if (m_globalMinScore != a_updateT.GlobalMinScore)
        {
            m_globalMinScore = a_updateT.GlobalMinScore;
            changed = true;
        }

        //Update sequence factor element settings. Values not in the transmission will not be deleted.
        foreach ((string, BalancedCompositeDispatcherDefinitionUpdateT.OptimizeRuleElementSettings) mapping in a_updateT.UpdatedMappings)
        {
            m_optimizeRuleMappings.AddOrUpdateMapping(mapping.Item1, new OptimizeRuleWeightSettings.OptimizeRuleElementSettings(mapping.Item2));
            changed = true;
        }

        //Update category multipliers. Values not in the transmission will not be deleted.
        foreach (KeyValuePair<string, decimal> pair in a_updateT.CategoryScalingMultipliers)
        {
            m_optimizeRuleMappings.SetCategoryScaling(pair.Key, pair.Value);
            changed = true;
        }

        if (changed)
        {
            a_dataChanges.BalancedCompositeDispatcherDefinitionChanges.UpdatedObject(Id);
        }
    }
}