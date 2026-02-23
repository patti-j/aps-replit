using PT.Common.PTMath;

namespace PT.Scheduler;

/// <summary>
/// Stores a list of Setup Code from-to values.
/// </summary>
public partial class CompatibilityCodeTable
{

    private ListValidation m_simListValidation;

    internal void ResetSimulationStateVariables()
    {
        if (AllowList)
        {
            m_simListValidation = new AllowListValidation();
        }
        else
        {
            m_simListValidation = new DenyListValidation();
        }

        HashSet<string> allcodes = new ();
        foreach (CompatibilityCodeTableRow row in this)
        {
            allcodes.Add(row.CompatibilityCode);
        }

        m_simListValidation.InitializeCodes(allcodes);
    }

    //TODO: Potentially initialize this class based on the resources that use it. If there is only 1, we can simplify the collections

    internal bool CodeScheduled(IInterval a_interval, string a_code)
    {
        return m_simListValidation.CodeScheduled(a_interval, a_code);
    }

    /// <summary>
    /// Call RemoveOldIntervals to clear out old codes before calling this function
    /// </summary>
    /// <param name="a_startDate"></param>
    /// <param name="a_code"></param>
    internal bool IsCodeValid(long a_startDate, string a_code)
    {
        return m_simListValidation.IsCodeValid(a_startDate, a_code);
    }

    private class CompatibilityInterval : Interval
    {

        internal readonly string Code;
        internal CompatibilityInterval(IInterval a_interval, string a_code)
        {
            StartTicks = a_interval.StartTicks;
            EndTicks = a_interval.EndTicks;
            Code = a_code;
        }
    }

    /// <summary>
    /// Base class for list validations
    /// </summary>
    private class ListValidation
    {
        protected Dictionary<string, int> m_activeCodes;
        protected List<CompatibilityInterval> m_currentIntervals;
        protected HashSet<string> m_allCodes;

        internal void InitializeCodes(HashSet<string> a_allCodes)
        {
            m_activeCodes = new Dictionary<string, int>();
            m_currentIntervals = new List<CompatibilityInterval>();
            m_allCodes = a_allCodes;
        }

        internal virtual bool CodeScheduled(IInterval a_interval, string a_code)
        {
            if (m_allCodes.Contains(a_code))
            {
                AddInterval(a_interval, a_code);

                return true;
            }
            else
            {
                //This code is not part of the list
                return false;
            }
        }

        private void AddInterval(IInterval a_interval, string a_code)
        {
            CompatibilityInterval compatibilityInterval = new CompatibilityInterval(a_interval, a_code);
            m_currentIntervals.Add(compatibilityInterval);
            if (m_activeCodes.TryGetValue(a_code, out int currentCount))
            {
                m_activeCodes[a_code] = currentCount + 1;
            }
            else
            {
                m_activeCodes.Add(a_code, 1);
            }
        }

        /// <summary>
        /// Call RemoveOldIntervals to clear out old codes before calling this function
        /// </summary>
        /// <param name="a_startDate"></param>
        /// <param name="a_code"></param>
        internal virtual bool IsCodeValid(long a_startDate, string a_code)
        {
            RemoveOldIntervals(a_startDate);
            return true;
        }

        private void RemoveOldIntervals(long a_time)
        {
            for (var i = m_currentIntervals.Count - 1; i >= 0; i--)
            {
                CompatibilityInterval currentInterval = m_currentIntervals[i];
                if (currentInterval.EndTicks <= a_time)
                {
                    if (m_activeCodes.TryGetValue(currentInterval.Code, out int currentCount))
                    {
                        if (currentCount == 1)
                        {
                            m_activeCodes.Remove(currentInterval.Code);
                        }
                        else
                        {
                            m_activeCodes[currentInterval.Code] = currentCount - 1;
                        }
                        m_currentIntervals.RemoveAt(i);
                    }
                    else
                    {
                        // In allowed lists, this interval can be for a scheduled code that isn't in the list

                        //#if DEBUG
                        //throw new DebugException("CompatibilityCodes intervals desynched");
                        //#endif
                    }
                }
            }
        }
    }

    /*
     * Allow list 1
     * 1, 2, 3, 4
     *
     *
     * Allow list 1
     * 4, 5, 6, 7, 8
     *
     * Active Codes
     * 1
     *
     * Attempt to schedule
     * 5
     *
     */

    private class AllowListValidation : ListValidation
    {
        private readonly List<CompatibilityInterval> m_scheduledCodesNotInList;

        internal AllowListValidation()
        {
            m_scheduledCodesNotInList = new List<CompatibilityInterval>();
        }

        /// <summary>
        /// Returns whether this list will allow the new code to schedule
        /// </summary>
        /// <param name="a_startDate"></param>
        /// <param name="a_code"></param>
        internal override bool IsCodeValid(long a_startDate, string a_code)
        {
            base.IsCodeValid(a_startDate, a_code);

            //Remove any scheduled codes not in the list that are no longer scheduled
            RemoveOldScheduledUnkownCodes(a_startDate);

            if (m_scheduledCodesNotInList.Count > 0)
            {
                //There are uknown codes scheduled, so nothing in this list can schedule yet
                return !m_allCodes.Contains(a_code);
            }
            
            if (m_activeCodes.Count > 0)
            {
                return m_allCodes.Contains(a_code);
            }

            return true;
        }

        /// <summary>
        /// Track codes even if they aren't in the allow list
        /// These will prevent any code in this list from scheduling
        /// </summary>
        internal override bool CodeScheduled(IInterval a_interval, string a_code)
        {
            bool codeScheduled = base.CodeScheduled(a_interval, a_code);
            if (!codeScheduled)
            {
                CompatibilityInterval compatibilityInterval = new CompatibilityInterval(a_interval, a_code);
                m_scheduledCodesNotInList.Add(compatibilityInterval);
            }

            return codeScheduled;
        }

        /// <summary>
        /// Remove any unknown codes that are no longer scheduled
        /// </summary>
        /// <param name="a_time"></param>
        private void RemoveOldScheduledUnkownCodes(long a_time)
        {
            for (var i = m_scheduledCodesNotInList.Count - 1; i >= 0; i--)
            {
                CompatibilityInterval currentInterval = m_scheduledCodesNotInList[i];
                if (currentInterval.EndTicks <= a_time)
                {
                    m_scheduledCodesNotInList.RemoveAt(i);
                }
            }
        }
    }

    /*
     * Deny list
     * 1, 2, 3
     *
     * Active Codes
     * 3
     *
     * Attempt to schedule
     * 4
     *
     */

    private class DenyListValidation : ListValidation
    {
        /// <summary>
        /// Returns whether this list will allow the new code to schedule
        /// </summary>
        /// <param name="a_startDate"></param>
        /// <param name="a_code"></param>
        internal override bool IsCodeValid(long a_startDate, string a_code)
        {
            base.IsCodeValid(a_startDate, a_code);
            //Nothing in list, nothing to deny
            if (m_activeCodes.Count == 0)
            {
                return true;
            }

            //Only one code would be active, so it must match
            if (m_activeCodes.ContainsKey(a_code))
            {
                //There is only one code
                return true;
            }

            //Otherwise if it's in the list, deny
            if (m_allCodes.Contains(a_code))
            {
                return false;
            }

            //Otherwise not in the list, allow
            return true;
        }
    }
}