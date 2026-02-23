namespace PT.Scheduler;

public partial class AlternatePath
{
    /// <summary>
    /// Stores a collection of associations.
    /// </summary>
    public partial class AssociationCollection
    {
        #region Simulation
        internal void ResetSimulationStateVariables()
        {
            for (int i = 0; i < Count; ++i)
            {
                this[i].ResetSimulationStateVariables();
            }
        }

        /// <summary>
        /// Calculate the latest scheduled finish date of all the operations. Returns 0 if non of the operations are scheduled.
        /// </summary>
        /// <returns>The lastest finish date of the operations or 0.</returns>
        internal long CalcLatestScheduledDate()
        {
            long dt = 0;

            for (int i = 0; i < Count; ++i)
            {
                Association a = this[i];
                if (a.Predecessor.Operation.GetScheduledFinishDate(out long finishDate, false))
                {
                    dt = Math.Max(dt, finishDate);
                }
            }

            return dt;
        }
        #endregion
    }
}