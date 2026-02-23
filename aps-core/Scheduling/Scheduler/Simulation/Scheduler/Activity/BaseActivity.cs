using System.ComponentModel;

using PT.APSCommon;
using PT.Common.Attributes;
using PT.Common.Exceptions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Summary description for BaseActivity.
/// </summary>
public abstract partial class BaseActivity
{
    /// <summary>
    /// This function must be called during the initialization phase of simulation.
    /// </summary>
    /// <param name="a_sd"></param>
    internal virtual void ResetSimulationStateVariables(ScenarioDetail a_sd)
    {
        m_simState.Clear();
        m_primaryResourceBlockActivityIndex = int.MaxValue;

        ConnectionViolated = false;
        m_scheduledSequence = -1;

        m_resourcesOptimizationScore = new Dictionary<BaseId, decimal>();
        CompositeScores = new Dictionary<BaseId, List<FactorScore>>();

        CompressLimitedByTicks = 0;
    }

    private int m_primaryResourceBlockActivityIndex;

    /// <summary>
    /// During a simulation this may be set to the position of the block the primary resource requirement is scheduled on.
    /// In moves this can be used to make sure the blocks are rescheduled in the order in which they were before the move.
    /// </summary>
    internal int PrimaryResourceBlockActivityIndex
    {
        get => m_primaryResourceBlockActivityIndex;

        set => m_primaryResourceBlockActivityIndex = value;
    }

    /// <summary>
    /// The order the activity was scheduled in.
    /// </summary>
    internal long m_scheduledSequence;

    private BoolVector32 m_simState;

    // Used indexes of the simState vector.
    private const int c_WaitForOptimizationReleaseEventIdx = 0;
    private const int WaitForAnchorReleaseEventIdx = 1;
    private const int ReanchorIdx = 2;
    private const int SimScheduledIdx = 3;
    private const int PlantNotIncludedInSimulateIdx = 4;
    private const int SuppressReleaseDateAdjustmentsIdx = 5;
    private const int WaitForPredBatchOpnsToBeScheduledIdx = 6;
    private const int WaitForRightCompressReleaseIdx = 7;
    private const int c_compressLimitedByTicksDeterminedIdx = 8;

    /// <summary>
    /// Indicates whether the activity is waiting on the OptimizationReleaseEvent.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.Hide)]
    [Browsable(false)]
    internal bool WaitForOptimizationReleaseEvent
    {
        get => m_simState[c_WaitForOptimizationReleaseEventIdx];
        set => m_simState[c_WaitForOptimizationReleaseEventIdx] = value;
    }

    /// <summary>
    /// Used by the simulation algorithm to keep track of whether the activity is constrained by the anchor date.
    /// </summary>
    internal bool WaitForAnchorReleaseEvent
    {
        get => m_simState[WaitForAnchorReleaseEventIdx];
        set => m_simState[WaitForAnchorReleaseEventIdx] = value;
    }

    private bool ReanchorActivity
    {
        get => m_simState[ReanchorIdx];
        set => m_simState[ReanchorIdx] = value;
    }

    /// <summary>
    /// Simulation ONLY flag. This flag is set to true when the activity is scheduled.
    /// </summary>
    internal bool SimScheduled
    {
        get => m_simState[SimScheduledIdx];
        set => m_simState[SimScheduledIdx] = value;
    }

    /// <summary>
    /// An optimization of a specific plant is occuring but this activity isn't scheduled within that plant, so the activity will not be released until that date
    /// it was originally scheduled.
    /// </summary>
    internal bool PlantNotIncludedInSimulate
    {
        get => m_simState[PlantNotIncludedInSimulateIdx];
        set => m_simState[PlantNotIncludedInSimulateIdx] = value;
    }

    /// <summary>
    /// If this is true, the release date will not be affected by the JIT start date, Frozen Span, Stable Span, Headstart Span, etc.
    /// </summary>
    internal bool SuppressReleaseDateAdjustments
    {
        get => m_simState[SuppressReleaseDateAdjustmentsIdx];
        set => m_simState[SuppressReleaseDateAdjustmentsIdx] = value;
    }

    internal bool WaitForPredBatchOpnsToBeScheduled
    {
        get => m_simState[WaitForPredBatchOpnsToBeScheduledIdx];
        set => m_simState[WaitForPredBatchOpnsToBeScheduledIdx] = value;
    }

    internal bool WaitForRightCompressReleaseEvent
    {
        get => m_simState[WaitForRightCompressReleaseIdx];
        set => m_simState[WaitForRightCompressReleaseIdx] = value;
    }

    /// <summary>
    /// If a compress is being performed whose release rule is by DBR date,
    /// this value is set to true after it has been determined whether the MO's
    /// compression should be limited by the DBR Release Date.
    /// </summary>
    internal bool CompressLimitedByTicksDetermined
    {
        get => m_simState[c_compressLimitedByTicksDeterminedIdx];
        set => m_simState[c_compressLimitedByTicksDeterminedIdx] = value;
    }

    /// <summary>
    /// Whether the activity is waiting on the OptimizationReleaseEvent.
    /// </summary>
    internal virtual bool Released
    {
        get
        {
            if (WaitForOptimizationReleaseEvent)
            {
                return false;
            }

            if (WaitForAnchorReleaseEvent)
            {
                return false;
            }

            if (WaitForPredBatchOpnsToBeScheduled)
            {
                return false;
            }

            if (WaitForRightCompressReleaseEvent)
            {
                return false;
            }

            return true;
        }
    }

    internal virtual int SimilarityComparison(BaseActivity a_ba)
    {
        int v;

        if ((v = m_requiredFinishQty.CompareTo(a_ba.m_requiredFinishQty)) != 0)
        {
            return v;
        }

        if ((v = m_baseActivityFlags.CompareTo(a_ba.m_baseActivityFlags)) != 0)
        {
            return v;
        }

        return 0;
    }

    #region Composite Scores
    internal void CalculateOptimizeFactorTotalsAndStore()
    {
        m_resourcesOptimizationScore.Clear();

        foreach ((BaseId resId, List<FactorScore> factorsList) in CompositeScores)
        {
            m_resourcesOptimizationScore.Add(resId, factorsList.Sum(f => f.Score));
        }
    }

    public IReadOnlyList<FactorScore> GetFactorScoresForResource(BaseId a_resId)
    {
        if (m_compositeScores.TryGetValue(a_resId, out List<FactorScore> factorsList))
        {
            return factorsList;
        }

        return new List<FactorScore>();
    }

    private Dictionary<BaseId, List<FactorScore>> m_compositeScores = new ();

    /// <summary>
    /// Store a list of composite scores calculated during simulation.
    /// </summary>
    internal Dictionary<BaseId, List<FactorScore>> CompositeScores
    {
        get => m_compositeScores;
        private set => m_compositeScores = value;
    }

    internal decimal TotalScoreBeforeResourceAdjustment { get; set; }

    /// <summary>
    /// Add a calculated score to the list of this activities scores.
    /// </summary>
    /// <param name="a_factorName">Name of the factor (optimize rule)</param>
    /// <param name="a_negativeScore">Whether the score is subtracted from total</param>
    /// <param name="a_score">the score to be added</param>
    internal void AddScore(FactorScore a_score)
    {
        if (a_score.Score != 0)
        {
            if (CompositeScores.TryGetValue(a_score.ResId, out List<FactorScore> scoresList))
            {
                scoresList.Add(a_score);
            }
            else
            {
                CompositeScores.Add(a_score.ResId, new List<FactorScore>() { a_score });
            }
        }
    }

    /// <summary>
    /// resets composite scores. This should be called everytime a new composite store is being calculated (before).
    /// </summary>
    /// <param name="a_resId"></param>
    internal void ResetScores(BaseId a_resId)
    {
        CompositeScores?.Remove(a_resId);
        m_resourcesOptimizationScore?.Remove(a_resId);
    }
    #endregion

    #region Anchoring
    /// <summary>
    /// The date this activity is anchored at.
    /// </summary>
    private long m_anchorDateTicks;

    /// <summary>
    /// The exception thrown in the event the AnchorDate is accessed before AnchorSet is true.
    /// </summary>
    public class AnchorDateAccessException : PTException
    {
        internal AnchorDateAccessException()
            : base("The AnchorDate was accessed, but the Anchor hasn't been dropped.") { }
    }

    public DateTime AnchorDate => new (m_anchorDateTicks, DateTimeKind.Utc);

    /// <summary>
    /// The date at which this activity is anchored.
    /// Attempts to access the anchor date before it is dropped will result in AnchorDateAccessExceptions being thrown.
    /// </summary>
    [DoNotAuditProperty]
    public long AnchorDateTicks
    {
        get
        {
            if (!AnchorDateHasBeenSet)
            {
                throw new AnchorDateAccessException();
            }

            return m_anchorDateTicks;
        }
    }

    /// <summary>
    /// Whether AnchorDate has been set to the date when the activity was scheduled.
    /// </summary>
    internal bool AnchorDateHasBeenSet
    {
        get => m_baseActivityFlags[AnchorDateHasBeenSetIdx];

        private set => m_baseActivityFlags[AnchorDateHasBeenSetIdx] = value;
    }

    internal void SetAnchor(bool a_value)
    {
        if (a_value)
        {
            if (Scheduled)
            {
                Anchor(GetScheduledStartTicks(), true);
            }
        }
        else
        {
            Anchor(0, false);
        }
    }

    /// <summary>
    /// Call this function to set the Anchor externally. For instance from a JobT.
    /// </summary>
    /// <param name="aAnchorDate"></param>
    internal void ExternalAnchor(long a_anchorDate)
    {
        if (a_anchorDate <= 0)
        {
            string errMsg = string.Format("The anchor date must be greater than 0. An attempt has been made to set it to {0}.", a_anchorDate);
            throw new Exception(errMsg);
        }

        Anchor(a_anchorDate, true);
    }

    /// <summary>
    /// Set the anchor
    /// </summary>
    /// <param name="a_anchorDate">The date the activity is to be anchored.</param>
    /// <param name="a_anchored">If true, Optimize will attempt to schedule the activity as close as possible to the anchor date.</param>
    private void Anchor(long a_anchorDate, bool a_anchored)
    {
        //Removed limit here that was preserving previously anchored activities from having their anchor date changed for Trinity.  They change Anchor date in both MAS 500 and APS.
        Anchored = a_anchored;
        m_anchorDateTicks = a_anchorDate;
        AnchorDateHasBeenSet = a_anchorDate != 0;
    }

    /// <summary>
    /// Used by the simulation algorithm to set the WaitForAnchorReleaseEvent flag.
    /// The function sets the flag if appropriate.
    /// </summary>
    internal bool SetupWaitForAnchorSetFlag()
    {
        if (Anchored && AnchorDateHasBeenSet)
        {
            WaitForAnchorReleaseEvent = true;
        }

        return WaitForAnchorReleaseEvent;
    }

    #region Reanchoring after a simulation
    /// <summary>
    /// Used during simulations to setup the activity for reanchoring at simulation end. This function only has an affect if the activity was anchored prior to being
    /// called. At simulation completion call Reanchor() to complete this process.
    /// </summary>
    internal void ReanchorSetup()
    {
        if (Anchored)
        {
            ReanchorActivity = true;
            Anchored = false;
        }
    }

    /// <summary>
    /// Reanchors activities after simulation. This function call only affects activities that were anchored prior to simulation start. ReanchorSetup()
    /// must have been called on this activity before the start of the simulation.
    /// </summary>
    internal void Reanchor()
    {
        if (ReanchorActivity)
        {
            if (Scheduled)
            {
                if (AnchorDateTicks > GetScheduledStartTicks())
                {
                    // The new date is better. Use it as the new anchor date.
                    SetAnchor(true);
                }
                else
                {
                    // Save the old anchor date.
                    Anchored = true;
                }
            }
            else
            {
                #if DEBUG
                throw new PTException("This activity should be scheduled. There is a problem with anchoring.");
                #endif
                Anchored = true;
            }
        }
    }
    #endregion
    #endregion

    #region Connectors
    //internal void CheckForConnectorViolation(BaseResource a_connectorRes, ResourceSatiator[] a_resReqSatiators)
    //{
    //    for (int i = 0; i < a_resReqSatiators.Length; ++i)
    //    {
    //        ResourceSatiator rs = a_resReqSatiators[i];

    //        if (rs != null && a_connectorRes.Connectors.ContainsKey(rs.Resource.Id))
    //        {
    //            return;
    //        }
    //    }

    //    ConnectionViolated = true;
    //}
    #endregion

    /// <summary>
    /// During a compress this is set to either the JIT date or Resource Head Start Span if the
    /// activity can't be compressed further than this date. Whether it can be is determined by
    /// the compress release rule setting being either JIT or Resource Head Start Span.
    /// </summary>
    internal long CompressLimitedByTicks { get; set; }
}