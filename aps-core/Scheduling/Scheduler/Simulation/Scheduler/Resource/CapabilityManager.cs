namespace PT.Scheduler;

partial class CapabilityManager
{
    #region Similarity
    internal void DetermineDifferences(CapabilityManager a_compareCapability, System.Text.StringBuilder a_warnings)
    {
        if (Count != a_compareCapability.Count)
        {
            a_warnings.Append("the operations being compared have different numbers of capabilities;");
            return;
        }

        for (int capabilityI = 0; capabilityI < Count; ++capabilityI)
        {
            Capability c = this[capabilityI];
            if (!a_compareCapability.Contains(c))
            {
                a_warnings.Append("the operations being compared have different numbers of capabilities;");
                return;
            }
        }
    }
    #endregion
}