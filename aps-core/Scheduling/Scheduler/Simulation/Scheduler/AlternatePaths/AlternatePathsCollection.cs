using PT.APSCommon;
using PT.Scheduler.Schedule;
using PT.Scheduler.Simulation.Extensions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Stores a collection of AlternatePaths.
/// </summary>
public partial class AlternatePathsCollection
{
    internal void AbsorbeReportedValues(AlternatePathsCollection apc)
    {
        for (int pathI = 0; pathI < apc.Count; ++pathI)
        {
            AlternatePath ap = this[pathI];
            AlternatePath absorbAP = apc.FindByName(ap.Name);
            ap.AbsorbReportedValues(absorbAP);
        }
    }

    internal bool AllPathsHaveOneRoot()
    {
        for (int pathI = 0; pathI < Count; ++pathI)
        {
            AlternatePath ap = this[pathI];
            AlternatePath.NodeCollection roots = ap.GetRoots();
            if (roots.Count != 1)
            {
                return false;
            }
        }

        return true;
    }

    internal void DetermineDifferences(AlternatePathsCollection a_apc, int a_differenceTypes, System.Text.StringBuilder a_warnings)
    {
        if (Count != a_apc.Count)
        {
            a_warnings.Append("Number of paths;");
        }

        for (int apI = 0; apI < Count; ++apI)
        {
            AlternatePath ap = this[apI];
            AlternatePath compareAP = a_apc.FindByExternalId(ap.ExternalId);
            if (compareAP == null)
            {
                a_warnings.Append("searching by external id, couldn't find alternate path in second alternate path collection;");
                return;
            }

            ap.DetermineDifferences(compareAP, a_differenceTypes, a_warnings);
        }
    }

    // !ALTERNATE_PATH!; ResetSimulationStateVariables() for the alternate paths.
    internal void ResetSimulationStateVariables(ScenarioDetail a_sd)
    {
        for (int pathI = 0; pathI < Count; ++pathI)
        {
            this[pathI].ResetSimulationStateVariables(a_sd);
        }
    }

    // !ALTERNATE_PATH!; SimulationInitialization() for the alternatePaths.
    internal void SimulationInitialization(PlantManager a_plantManager, ProductRuleManager a_productRuleManager, ExtensionController a_extensionController, ICalculatedValueCacheManager a_cacheManager)
    {
        for (int pathI = 0; pathI < Count; ++pathI)
        {
            this[pathI].SimulationInitialization(a_plantManager, a_productRuleManager, a_extensionController, a_cacheManager);
        }
    }

    internal void PostSimulationInitialization()
    {
        for (int pathI = 0; pathI < Count; ++pathI)
        {
            this[pathI].PostSimulationInitialization();
        }
    }

    internal AlternatePath GetOpsPath(BaseOperation a_op)
    {
        if (Count == 1)
        {
            #if DEBUG
            if (!this[0].AlternateNodeSortedList.ContainsKey(a_op.ExternalId))
            {
                throw new Exception("The operation isn't in the path.");
            }
            #endif
            return this[0];
        }

        //#if !DEBUG
        //                speed this up; if you have time; only those using multiple paths experience this search.
        //#endif
        for (int i = 0; i < Count; ++i)
        {
            AlternatePath path = this[i];
            if (path.AlternateNodeSortedList.ContainsKey(a_op.ExternalId))
            {
                return path;
            }
        }

        throw new Exception("The alternate path couldn't be found.");
    }

    /// <summary>
    /// Add the paths of a specified AutoUse type to a list. During an expedite certain marked ineligible paths will be excluded.
    /// </summary>
    /// <param name="a_autoUseTypes">The AutoUse type of paths to add to the list.</param>
    /// <param name="a_paths">The list to add paths to.</param>
    internal void FindAutoUsePaths(List<AlternatePath> a_paths, params AlternatePathDefs.AutoUsePathEnum[] a_autoUseTypes)
    {
        if (a_autoUseTypes == null || a_autoUseTypes.Length == 0)
        {
            return;
        }
        HashSet<AlternatePathDefs.AutoUsePathEnum> allowedPathsEnums = new HashSet<AlternatePathDefs.AutoUsePathEnum>(a_autoUseTypes);

        for (int pathI = 0; pathI < Count; ++pathI)
        {
            // The simplest form of multiple path release. The release timing is only based on the optimize settings.
            AlternatePath path = this[pathI];
            if (!path.ExcludedByExpediteBecauseItsLeadOpsArentEligibleOnSpecifiedRes &&
                allowedPathsEnums.Contains(path.AutoUse) && path.AdjustedPlantResourceEligibilitySets_IsSatisfiable()
               )
            {
                a_paths.Add(path);
            }
        }
    }

    /// <summary>
    /// Filters all AdjustedPlantResourceEligibilitySets down to a specific plant.
    /// </summary>
    /// <param name="a_plantId"></param>
    internal void AdjustedPlantResourceEligibilitySets_FilterDownToSpecificPlant(BaseId a_plantId)
    {
        for (int pathI = 0; pathI < Count; ++pathI)
        {
            AlternatePath path = this[pathI];
            path.AdjustedPlantResourceEligibilitySets_FilterDownToSpecificPlant(a_plantId);
        }
    }

    /// <summary>
    /// Filters all AdjustedPlantResourceEligibilitySets.
    /// </summary>
    internal void AdjustedPlantResourceEligibilitySets_Filter(ResReqsPlantResourceEligibilitySets.ExcludeFromManualFilter a_excludeFromManualFilter)
    {
        for (int pathI = 0; pathI < Count; ++pathI)
        {
            AlternatePath path = this[pathI];
            path.AdjustedPlantResourceEligibilitySets_Filter(a_excludeFromManualFilter);
        }
    }

    internal long FindMinJitStart()
    {
        long minJitStart = long.MaxValue;
        for (int i = 0; i < Count; ++i)
        {
            AlternatePath path = this[i];
            AlternatePath.Node n;
            minJitStart = Math.Max(path.FindMinJitStart(out n), minJitStart);
        }

        return minJitStart;
    }
}