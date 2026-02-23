using System.Collections;

namespace PT.Scheduler;

public partial class BaseOperationManager
{
    /// <summary>
    /// Disassociate each operation from its alternate path node.
    /// </summary>
    internal void ClearOpPathNodeAssociations()
    {
        IDictionaryEnumerator etr = OperationsHash.GetEnumerator();
        while (etr.MoveNext())
        {
            BaseOperation op = etr.Value as BaseOperation;
            op.AlternatePathNode = null;
        }
    }

    /// <summary>
    /// Whether any activity has any of its blocks scheduled within its Stable span.
    /// </summary>
    /// <param name="a_spanCalc">Used to calculate the resource stable span.</param>
    /// <returns></returns>
    internal bool AnyActivityInStableSpan()
    {
        IDictionaryEnumerator enumerator = OperationsHash.GetEnumerator();

        while (enumerator.MoveNext())
        {
            BaseOperation operation = (BaseOperation)enumerator.Value;
            if (operation is InternalOperation)
            {
                InternalOperation io = (InternalOperation)operation;
                if (io.AnyActivityInSpan(io.ScenarioDetail.Clock, OptimizeSettings.ETimePoints.EndOfStableZone))
                {
                    return true;
                }
            }
        }

        return false;
    }
}