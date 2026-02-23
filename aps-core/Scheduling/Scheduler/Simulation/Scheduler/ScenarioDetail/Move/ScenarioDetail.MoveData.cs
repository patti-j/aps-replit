using System.Collections;
using System.Text;

namespace PT.Scheduler;

public partial class ScenarioDetail
{
    /// <summary>
    /// What to move and the Move options.
    /// </summary>
    private class MoveData : Transmissions.BasicMoveData, IEnumerable<MoveBlockData>
    {
        /// <summary>
        /// Specify the transmission the move is for.
        /// </summary>
        /// <param name="a_data">The transmission the move is for.</param>
        internal MoveData(Transmissions.ScenarioDetailMoveT a_data)
            : base(a_data)
        {
            MoveT = a_data;
        }

        /// <summary>
        /// The transmission the Move is for.
        /// </summary>
        internal Transmissions.ScenarioDetailMoveT MoveT { get; private set; }

        private readonly List<MoveBlockData> m_moveBlocks = new ();

        /// <summary>
        /// Index into the Move blocks.
        /// </summary>
        /// <param name="a_i"></param>
        /// <returns></returns>
        internal MoveBlockData this[int a_i] => m_moveBlocks[a_i];

        /// <summary>
        /// The number of Move blocks.
        /// </summary>
        internal int Count => m_moveBlocks.Count;

        /// <summary>
        /// Add a Move block.
        /// </summary>
        /// <param name="a_mb"></param>
        internal void AddMoveBlock(MoveBlockData a_mb)
        {
            m_moveBlocks.Add(a_mb);
        }

        /// <summary>
        /// Remove a Move block.
        /// </summary>
        /// <param name="a_mb"></param>
        internal void RemoveMoveBlock(MoveBlockData a_mb)
        {
            m_moveBlocks.Remove(a_mb);
        }

        #region IEnumerator
        /// <summary>
        /// Get an iterator to the move blocks. Becomes invalid if blocks are added or removed.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<MoveBlockData> GetEnumerator()
        {
            return m_moveBlocks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        /// <summary>
        /// The resource to move the blocks/activities to.
        /// </summary>
        internal Resource ToResource { get; set; }

        /// <summary>
        /// The move start ticks adjusted for whatever. For instance the start time might adjusted by things such as dropping the block before the clock, a batch join, etc.
        /// </summary>
        internal long StartTicksAdjusted { get; set; }

        // [USAGE_CODE] EndOfProcessingTicks: Calculated end of processing on the primary RR. This is calculated to define the interval of time that will be used.
        /// <summary>
        /// Calculated end of processing on the primary RR. This is calculated to define the interval of time that will be used.
        /// </summary>
        internal long EndOfProcessingTicks { get; set; }

        // [USAGE_CODE] EndOfPostProcessingTicks: Calculated end of Post processon the primary RR. This is calculated to define the interval of time that will be used.
        /// <summary>
        /// Calculated end of Post processon the primary RR. This is calculated to define the interval of time that will be used.
        /// </summary>
        internal long EndOfPostProcessingTicks { get; set; }

        /// <summary>
        /// Remove MoveBlockData objects that have no activities. The simulation validation code will exclude activities from the move that have MoveProblems.
        /// This function should be called after the MoveBlockData object's excluded activities have been removed.
        /// </summary>
        internal void RemoveEmptyMoveBlocks()
        {
            for (int i = m_moveBlocks.Count - 1; i >= 0; --i)
            {
                MoveBlockData mbd = m_moveBlocks[i];
                if (mbd.Count == 0)
                {
                    RemoveMoveBlock(mbd);
                }
            }
        }

        /// <summary>
        /// Activities that must stay on the resource they were scheduled on before the move was started.
        /// The set of activities in this list and the set of activities being moved must not intersect.
        /// </summary>
        private readonly List<InternalActivity> m_keepOnCurResActList = new ();

        /// <summary>
        /// Enumerate the set of activities that must stay on the resource they were scheduled on before the move.
        /// </summary>
        /// <returns></returns>
        internal IEnumerator<InternalActivity> GetKeepOnCurResActivitiesEnumerator()
        {
            return m_keepOnCurResActList.GetEnumerator();
        }

        /// <summary>
        /// Add a new activity to the set of activities that must remain on the resource they were scheduled on before the move started.
        /// </summary>
        internal void AddKeepOnCurResAct(InternalActivity a_act)
        {
            m_keepOnCurResActList.Add(a_act);
        }

        private int m_moveRRIdx = -1;

        /// <summary>
        /// This value must be set prior to usage.
        /// The RR index of the block being moved.
        /// </summary>
        internal int MoveRRIdx
        {
            get => m_moveRRIdx;
            set => m_moveRRIdx = value;
        }

        public override string ToString()
        {
            StringBuilder sb = new ();

            string resourceName = "";
            if (ToResource != null)
            {
                resourceName = ToResource.Name;
            }

            int nbrOfActivities = 0;
            foreach (MoveBlockData mbd in this)
            {
                nbrOfActivities += mbd.Count;
            }

            sb.AppendFormat("AdjustedStartTicks: {0}; Resource: {1}; Blocks: {2}; Activities: {3}", DateTimeHelper.ToLocalTimeFromUTCTicks(StartTicksAdjusted), resourceName, m_moveBlocks.Count, nbrOfActivities);

            if (m_moveBlocks.Count == 1 && m_moveBlocks[0].Block.Batched)
            {
                sb.AppendFormat("; Job: {0}", m_moveBlocks[0].Block.Batch.FirstActivity.Job.Name);
            }

            sb.Append("; " + base.ToString());
            return sb.ToString();
        }
    }
}