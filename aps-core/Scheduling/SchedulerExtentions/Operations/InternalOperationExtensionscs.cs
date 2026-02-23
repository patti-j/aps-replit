using PT.Scheduler;

namespace PT.SchedulerExtensions.Operations;

public static class InternalOperationExtensions
{
    public static bool IsResourceCapable(this InternalOperation a_internalOperation, InternalResource a_res, ResourceRequirement a_rr)
    {
        if (a_rr.DefaultResource != null && a_rr.DefaultResource.Id == a_res.Id)
        {
            return true;
        }

        foreach (Capability cap in a_rr.CapabilityManager)
        {
            if (!a_res.IsCapable(cap.Id))
            {
                return false;
            }
        }

        return true;
    }

    public static string GetOutOfRangeAttributes(this InternalOperation a_internalOperation, InternalResource a_res)
    {
        List<string> outOfRangeAttrs = new ();

        if (a_res.FromToRanges != null)
        {
            for (int i = 0; i < a_internalOperation.Attributes.Count; i++)
            {
                OperationAttribute attr = a_internalOperation.Attributes[i];
                Scheduler.RangeLookup.FromRangeSet fromRangeSet = a_res.FromToRanges.Find(attr.PTAttribute.Name);
                if (fromRangeSet != null && fromRangeSet.EligibilityConstraint)
                {
                    Scheduler.RangeLookup.FromRange fromRange = fromRangeSet.Find(attr.Number);
                    if (fromRange == null)
                    {
                        outOfRangeAttrs.Add(attr.PTAttribute.Name);
                    }
                }
            }

            if (outOfRangeAttrs.Count > 0)
            {
                string.Join(",", outOfRangeAttrs);
            }
        }

        return string.Empty;
    }
}