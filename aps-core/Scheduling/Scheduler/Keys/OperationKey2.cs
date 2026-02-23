namespace PT.Scheduler;

/// <summary>
/// Summary description for OperationKey2.
/// </summary>
public class OperationKey2 : SchedulerDefinitions.OperationKey, IEquatable<OperationKey2>
{
    public OperationKey2(BaseOperation operation)
        : base(operation.ManufacturingOrder.Job.Id, operation.ManufacturingOrder.Id, operation.Id) { }

    public OperationKey2(InternalActivity activity)
        : base(activity.Operation.ManufacturingOrder.Job.Id, activity.Operation.ManufacturingOrder.Id, activity.Operation.Id) { }

    public override int GetHashCode()
    {
        return HashCode.Combine(JobId, MOId, OperationId);
    }

    public bool Equals(OperationKey2 other)
    {
        if (other == null)
        {
            return false;
        }
        return JobId.Equals(other.JobId) && MOId.Equals(other.MOId) && OperationId.Equals(other.OperationId);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((OperationKey2)obj);
    }

    public static bool operator ==(OperationKey2 left, OperationKey2 right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(OperationKey2 left, OperationKey2 right)
    {
        return !Equals(left, right);
    }
}