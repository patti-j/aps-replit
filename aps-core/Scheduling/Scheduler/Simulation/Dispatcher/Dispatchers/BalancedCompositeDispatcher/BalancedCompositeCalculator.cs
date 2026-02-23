using PT.APSCommon;
using PT.PackageDefinitions;
using PT.Scheduler.PackageDefs;
using PT.SchedulerDefinitions;

namespace PT.Scheduler.Simulation.Dispatcher.Dispatchers.BalancedCompositeDispatcher;

/// <summary>
/// This class uses the optimize rule elements from packages that are a part of a rule set and uses them to calculate a score for an activity.
/// This functionality used to be part of the BalancedCompositeDispatcher. It was moved here since it is constant
/// regardless of the set of rules being used.
/// </summary>
internal class BalancedCompositeCalculator
{
    private decimal m_globalMinScore = decimal.MinValue;
    private readonly List<IOptimizeRuleScoreElement> m_ruleElements;
    private readonly List<IOverrideOptimizeRuleElement> m_overrideRuleElements;
    //internal bool ScoresAreConstant; //TODO: For potential performance, if the scores don't change, a simple dictionary lookup once calculated would be faster.

    internal BalancedCompositeCalculator()
    {
        m_ruleElements = new List<IOptimizeRuleScoreElement>();
        m_overrideRuleElements = new List<IOverrideOptimizeRuleElement>();
    }

    internal void InitWithDefinition(ScenarioDetail a_sd, OptimizeRuleWeightSettings a_weightMappings, decimal a_globalMinScore)
    {
        m_globalMinScore = a_globalMinScore;
        m_cachedScoresDictionary.Clear();
        m_ruleElements.Clear();
        m_overrideRuleElements.Clear();
        //ScoresAreConstant = true;
        using (SystemController.Sys.PackageManagerLock.EnterRead(out IPackageManager pm))
        {
            //Get currently loaded modules
            List<IOptimizeRuleModule> modules = pm.GetOptimizeRuleModules();
            //Generate the optimize rule elements based on the current settings and this dispatcher definition Id
            using (a_sd.Scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
            {
                foreach (IOptimizeRuleModule module in modules)
                {
                    List<IOptimizeRuleElement> ruleElements = module.GenerateOptimizeRules(ss.ScenarioSettings);
                    foreach (IOptimizeRuleElement ruleElement in ruleElements)
                    {
                        if (!(ruleElement is IOptimizeRuleScoreElement optimizeRule))
                        {
                            continue;
                            //TODO: Bad, handle exception here. This means that the element is only using the base type, which is insufficient to have any affect on the schedule
                        }

                        //Verify this rule needs to be configured
                        if (ruleElement is IWeightedOptimizeRuleElement weightedRule)
                        {
                            weightedRule.Points = a_weightMappings.GetRulePoints(ruleElement.PackageObjectId);

                            bool preventScaling = a_weightMappings.PreventsScaling(weightedRule.PackageObjectId);
                            weightedRule.CategoryMultiplier = !preventScaling ? a_weightMappings.GetCategoryScaling(weightedRule.Category) : 0;

                            if (!weightedRule.CalculateScores)
                            {
                                //This rule won't have any impact, no need to calculate it.
                                continue;
                            }
                        }

                        if (ruleElement is IMinimumScoreOptimizeRuleElement minElementRule)
                        {
                            (decimal minScore, bool useMinScore) = a_weightMappings.GetMinimumScore(ruleElement.PackageObjectId);
                            minElementRule.MinimumScore = minScore;
                            minElementRule.UseMinimumScore = useMinScore;
                        }

                        if (ruleElement is IConfigurableOptimizeRuleElement configurableRule)
                        {
                            configurableRule.Settings = a_weightMappings.GetRuleSettings(ruleElement.PackageObjectId);
                            configurableRule.InitializeSettings();
                        }

                        //Verify if this rule needs to be set with the alternate resource settings
                        if (ruleElement is IAlternateResourceScoreElement multiResourceElement)
                        {
                            //This score may change based on how the schedule is formulated, for example time dependent scores.
                            multiResourceElement.ResourceMultiplier = a_weightMappings.GetRuleResourceMultiplier(multiResourceElement.PackageObjectId);
                        }

                        if (ruleElement is IOverrideOptimizeRuleElement overrideRuleElement)
                        {
                            m_overrideRuleElements.Add(overrideRuleElement);
                        }
                        else
                        {
                            m_ruleElements.Add(optimizeRule);
                        }
                    }
                }
            }
        }
    }

    private readonly Dictionary<BaseId, List<(decimal, long, FactorScore?)>> m_cachedScoresDictionary = new ();

    /// <summary>
    /// Compute the composite based on the weights of the balanced composite rule.
    /// </summary>
    /// <param name="a_res">The resource the activity is a candidate on.</param>
    /// <param name="a_act">The activity that you want to compute a composite for.</param>
    /// <returns></returns>
    public decimal ComputeComposite(InternalResource a_res, InternalActivity a_act)
    {
        decimal composite = 0;
        decimal compositeDivisor = 0;
        List<FactorScore?> calculatedScores = new ();

        a_act.ResetScores(a_res.Id);

        bool skipConstantFactors = false;
        bool skipTimeBasedFactors = false;
        long simClock = a_act.ScenarioDetail.SimClock;
        if (m_cachedScoresDictionary.TryGetValue(a_act.Id, out List<(decimal, long, FactorScore?)> cache))
        {
            foreach ((decimal score, long scheduledTicks, FactorScore? factorScore) in cache)
            {
                if (scheduledTicks != long.MaxValue)
                {
                    //cached for a TimeBased set of scores
                    if (scheduledTicks == simClock)
                    {
                        skipTimeBasedFactors = true;
                        composite += score;
                        compositeDivisor += Math.Abs(score);
                        if (factorScore != null)
                        {
                            calculatedScores.Add(factorScore.Value);
                        }
                    }
                    else
                    {
                        m_cachedScoresDictionary.Remove(a_act.Id, out cache);
                    }
                }
                else
                {
                    skipConstantFactors = true;
                    //cached for constant scores

                    composite += score;
                    compositeDivisor += Math.Abs(score);

                    if (factorScore != null)
                    {
                        calculatedScores.Add(factorScore.Value);
                    }
                }
            }
        }

        bool minScoreFactorsMeetRequirements = true;
        bool runningOptimize = a_act.ScenarioDetail.ActiveSimulationType == ScenarioDetail.SimulationType.Optimize;

        //We don't need to calculate scores for sequenced Activities
        if (a_act.Sequenced)
        {
            return 0;
        }

        //If any override score elements exist and return a score, we can return the max value of those and return to improve performance
        decimal highestOverrideScore = 0;
        foreach (IOverrideOptimizeRuleElement overrideScore in m_overrideRuleElements)
        {
            decimal score = overrideScore.GetScore(a_act, (Resource)a_res, a_act.ScenarioDetail, simClock);
            if (score > highestOverrideScore)
            {
                highestOverrideScore = score;
            }
        }

        if (highestOverrideScore != 0)
        {
            return highestOverrideScore;
        }

        //No need to run in parallel if no groups were created. 
        foreach (IOptimizeRuleScoreElement optimizeRuleScoreElement in m_ruleElements)
        {
            if (SkipCalculation(runningOptimize, optimizeRuleScoreElement, skipConstantFactors, skipTimeBasedFactors))
            {
                continue;
            }

            decimal score = CalculateScores(optimizeRuleScoreElement, a_res, a_act, simClock);
            composite += score;
            compositeDivisor += Math.Abs(score);

            FactorScore? factorScore = null;
            if (optimizeRuleScoreElement is IWeightedOptimizeRuleElement configurableRule && score != 0m)
            {
                //Update the activity for score tracking, but we don't yet know the total compositeDivisor.
                factorScore = new FactorScore(configurableRule.Name, score, 0m, a_res.Id);
                calculatedScores.Add(factorScore);
            }

            if (optimizeRuleScoreElement is IMinimumScoreOptimizeRuleElement minScoreElement)
            {
                if (minScoreElement.UseMinimumScore && score < minScoreElement.MinimumScore)
                {
                    minScoreFactorsMeetRequirements = false;
                }
            }

            CacheActivityScore(a_act.Id, score, optimizeRuleScoreElement.DependencyType, simClock, factorScore);
        }

        if (runningOptimize && (!minScoreFactorsMeetRequirements || composite < m_globalMinScore))
        {
            a_act.MinimumScoreNotMet = true;
            return 0;
        }

        a_act.MinimumScoreNotMet = false;

        foreach (FactorScore factorScore in calculatedScores)
        {
            a_act.AddScore(new FactorScore(factorScore, Math.Abs(factorScore.Score) / compositeDivisor, a_res.Id));
        }

        return composite;
    }

    /// <summary>
    /// This function was created to attempt speeding up the composite score calculations during a simulation by running them in parallel.
    /// The experiment showed that the calculations themselves are too fast to warrant spawning new threads for the execution of the tasks. The overhead resulted in
    /// simulations running significantly slower than if we ran them in sequence.
    /// </summary>
    /// <param name="a_res"></param>
    /// <param name="a_act"></param>
    /// <param name="a_elementListGroups"></param>
    /// <param name="a_skipConstantFactors"></param>
    /// <param name="a_skipTimeBasedFactors"></param>
    /// <param name="a_calculatedScores"></param>
    /// <param name="a_composite"></param>
    /// <param name="a_compositeDivisor"></param>
    private void CalculateScoresInParallel(InternalResource a_res, InternalActivity a_act, List<List<IOptimizeRuleScoreElement>> a_elementListGroups, bool a_skipConstantFactors, bool a_skipTimeBasedFactors, List<FactorScore?> a_calculatedScores, ref decimal a_composite, ref decimal a_compositeDivisor)
    {
        List<Task> calcThreads = new (a_elementListGroups.Count);
        //run in parallel
        decimal[] scoreArray = new decimal[m_ruleElements.Count];
        string[] nameArray = new string[m_ruleElements.Count];
        int[] enumArray = new int[m_ruleElements.Count];
        int[] indexArray = new int[a_elementListGroups.Count];

        int idx = 0;
        int groupIdx = 0;

        foreach (List<IOptimizeRuleScoreElement> elementsGroup in a_elementListGroups)
        {
            indexArray[groupIdx] = idx;
            idx += elementsGroup.Count;
            groupIdx++;
        }

        groupIdx = 0;
        foreach (List<IOptimizeRuleScoreElement> elementsGroup in a_elementListGroups)
        {
            int startingGroupIdx = indexArray[groupIdx];
            //TODO: Convert to Tasks instead
            calcThreads.Add(Task.Factory.StartNew(new Action(() =>
            {
                CreateCompositeScoreCalcThread(a_res, a_act, elementsGroup, a_skipConstantFactors, a_skipTimeBasedFactors, scoreArray, startingGroupIdx, enumArray, nameArray);
            })));

            groupIdx++;
        }

        Task.WaitAll(calcThreads.ToArray());

        for (int i = 0; i < idx; i++)
        {
            decimal score = scoreArray[i];
            string factorName = nameArray[i];
            int dependencyEnum = enumArray[i];

            a_composite += score;
            a_compositeDivisor += Math.Abs(score);

            FactorScore? factorScore = null;
            if (!string.IsNullOrWhiteSpace(factorName) && score != 0m)
            {
                factorScore = new FactorScore(factorName, score, 0m, a_res.Id);
                a_calculatedScores.Add(factorScore);
            }

            CacheActivityScore(a_act.Id, score, (PackageEnums.ESequencingFactorCalculationDependency)dependencyEnum, a_act.ScenarioDetail.SimClock, factorScore);
        }
    }

    private int CreateCompositeScoreCalcThread(InternalResource a_res, InternalActivity a_act, List<IOptimizeRuleScoreElement> a_elementsGroup, bool a_skipConstantFactors, bool a_skipTimeBasedFactors, decimal[] a_scoreArray, int a_groupIdx, int[] a_enumArray, string[] a_nameArray)
    {
        bool runningOptimize = a_act.ScenarioDetail.ActiveSimulationType == ScenarioDetail.SimulationType.Optimize;
        foreach (IOptimizeRuleScoreElement ruleElement in a_elementsGroup)
        {
            if (!SkipCalculation(runningOptimize, ruleElement, a_skipConstantFactors, a_skipTimeBasedFactors))
            {
                decimal score = CalculateScores(ruleElement, a_res, a_act, a_act.ScenarioDetail.SimClock);
                a_scoreArray[a_groupIdx] = score;
                a_enumArray[a_groupIdx] = (int)ruleElement.DependencyType;

                if (ruleElement is IWeightedOptimizeRuleElement configurableRule)
                {
                    a_nameArray[a_groupIdx] = configurableRule.Name;
                }
                else
                {
                    a_nameArray[a_groupIdx] = string.Empty;
                }

                a_groupIdx++;
            }
        }

        return a_groupIdx;
    }

    private void CacheActivityScore(BaseId a_actId, decimal a_score, PackageEnums.ESequencingFactorCalculationDependency a_dependencyType, long a_simClock, FactorScore? a_factorScore)
    {
        //Don't cache Resource Based scores as they need to be recalculated each time.
        if (a_dependencyType == PackageEnums.ESequencingFactorCalculationDependency.ResourceBased)
        {
            return;
        }

        long scheduledTicks = long.MaxValue;

        if (a_dependencyType == PackageEnums.ESequencingFactorCalculationDependency.TimeBased)
        {
            scheduledTicks = a_simClock;
        }

        if (m_cachedScoresDictionary.TryGetValue(a_actId, out List<(decimal, long, FactorScore?)> cache))
        {
            cache.Add((a_score, scheduledTicks, a_factorScore));
        }
        else
        {
            m_cachedScoresDictionary.Add(a_actId, new List<(decimal, long, FactorScore?)>() { (a_score, scheduledTicks, a_factorScore) });
        }
    }

    private static bool SkipCalculation(bool a_runningOptimize, IOptimizeRuleScoreElement a_element, bool a_skipConstantFactors, bool a_skipTimeBasedFactors)
    {
        if (a_element is IWeightedOptimizeRuleElement && !a_runningOptimize)
        {
            return true;
        }

        return (a_element.DependencyType == PackageEnums.ESequencingFactorCalculationDependency.Constant && a_skipConstantFactors) ||
               (a_element.DependencyType == PackageEnums.ESequencingFactorCalculationDependency.TimeBased && a_skipTimeBasedFactors);
    }

    private decimal CalculateScores(IOptimizeRuleScoreElement a_sequenceFactorElement, InternalResource a_res, InternalActivity a_act, long a_simClock)
    {
        decimal score = a_sequenceFactorElement.GetScore(a_act, (Resource)a_res, a_act.ScenarioDetail, a_act.ScenarioDetail.SimClock);
        if (a_sequenceFactorElement is IAlternateResourceScoreElement alternateResourceElement)
        {
            score += AdjustScoreRelativeToOtherResources(a_res, a_act, score, alternateResourceElement);
        }

        if (a_sequenceFactorElement is IEarlyWindowBufferOptimizeRuleElement earlyWindowElement)
        {
            ActivityResourceBufferInfo bufferInfo = a_act.BufferInfo.GetResourceInfo(a_res.Id);
            long earlyWindowStart = bufferInfo.ReleaseDate;
            long earlyWindowEnd = bufferInfo.SequenceHeadStartWindowEndDate;
            score = earlyWindowElement.ApplyEarlyWindowPenalty(score, a_simClock, earlyWindowStart, earlyWindowEnd);
        }
       

        if (score != 0m)
        {
            //Constrain score to defined bounds. This prevents overflow/underflow issues which could erase some score values
            score = Math.Min(OptimizeRuleConstraints.MaxOptimizeRuleScore, score);
            score = Math.Max(OptimizeRuleConstraints.MinOptimizeRuleScore, score);
        }

        return score;
    }
    
    internal bool AreRulesConstant()
    {
        foreach (IOptimizeRuleScoreElement element in m_ruleElements)
        {
            if (element.DependencyType != PackageEnums.ESequencingFactorCalculationDependency.Constant)
            {
                return false;
            }
        }

        return true;
    }

    internal decimal AdjustScoreRelativeToOtherResources(InternalResource a_res, InternalActivity a_act, decimal a_score, IAlternateResourceScoreElement a_alternate)
    {
        if (a_alternate.ResourceMultiplier == 0m || a_act.Locked == lockTypes.Locked)
        {
            return 0;
        }

        decimal bestScore = a_score;
        decimal worstScore = a_score;

        PlantResourceEligibilitySet pres = a_act.ResReqsEligibilityNarrowedDuringSimulation.PrimaryEligibilitySet;

        SortedDictionary<BaseId, EligibleResourceSet>.Enumerator ersEtr = pres.GetEnumerator();
        while (ersEtr.MoveNext())
        {
            EligibleResourceSet eligibleResourceSet = ersEtr.Current.Value;
            for (int i = 0; i < eligibleResourceSet.Count; ++i)
            {
                Resource eligibleResource = (Resource)eligibleResourceSet[i];
                if (eligibleResource != a_res)
                {
                    decimal otherScore = a_alternate.GetScore(a_act, eligibleResource, a_act.ScenarioDetail, a_act.ScenarioDetail.SimClock);
                    bestScore = Math.Max(bestScore, otherScore);
                    worstScore = Math.Min(worstScore, otherScore);
                }
            }
        }

        //Get average score
        decimal average = (bestScore + worstScore) / 2;

        //Get the percent difference from the average
        decimal distanceFromAverage = a_score - average;
        
        //Multiply that distance and return it to adjust the original score.
        return distanceFromAverage * a_alternate.ResourceMultiplier;
    }

    public void SetTempMinScoreForEndOfPlanningHorizon()
    {
        m_globalMinScore = decimal.MinValue;
    }
}