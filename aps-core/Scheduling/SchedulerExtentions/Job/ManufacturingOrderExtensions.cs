using System.Collections;

using PT.Scheduler;

namespace PT.SchedulerExtensions.Job;

public static class ManufacturingOrderExtensions
{
    /// <summary>
    /// List of all Material Item Names for the Job. (Single level)
    /// </summary>
    public static HashSet<string> MaterialList(this ManufacturingOrder a_mo)
    {
        HashSet<string> list = new ();
        IEnumerator<KeyValuePair<string, AlternatePath.Node>> enumerator = a_mo.CurrentPath.AlternateNodeSortedList.GetEnumerator();

        while (enumerator.MoveNext())
        {
            AlternatePath.Node apNode = enumerator.Current.Value;
            BaseOperation operation = apNode.Operation;
            for (int mrI = 0; mrI < operation.MaterialRequirements.Count; mrI++)
            {
                MaterialRequirement mr = (MaterialRequirement)operation.MaterialRequirements.GetRow(mrI);
                string materialName = mr.MaterialName;
                if (!list.Contains(materialName))
                {
                    list.Add(materialName);
                }
            }
        }

        return list;
    }

    /// <summary>
    /// The scheduled unfinished Operation with the earliest Scheduled Start Date.
    /// </summary>
    public static IEnumerable<BaseOperation> GetLeadUnscheduledOperations(this ManufacturingOrder a_mo)
    {
        DateTime jitStart = PTDateTime.MaxDateTime;
        List<BaseOperation> leadOperations = new ();

        IDictionaryEnumerator operationsEnumerator = a_mo.OperationManager.OperationsHash.GetEnumerator();
        while (operationsEnumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)operationsEnumerator.Current;
            BaseOperation op = (BaseOperation)de.Value;
            if (op is InternalOperation internalOp)
            {
                if (internalOp.Predecessors.Count > 0 || internalOp.Finished)
                {
                    //This op cannot be the lead op
                    continue;
                }

                if (internalOp.DbrJitStartDate < jitStart)
                {
                    jitStart = internalOp.DbrJitStartDate;
                    leadOperations.Clear();
                    leadOperations.Add(internalOp);
                }
                else if (internalOp.DbrJitStartDate == jitStart)
                {
                    leadOperations.Add(internalOp);
                }
            }
        }

        return leadOperations;
    }
}