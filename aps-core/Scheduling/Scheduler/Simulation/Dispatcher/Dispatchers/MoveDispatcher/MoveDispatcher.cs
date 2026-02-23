namespace PT.Scheduler;

internal class MoveDispatcher : SortedListDispatcher
{
    /// <summary>
    /// This is the dispatcher used when a move or expedite occurs. Precedense is give to the moved
    /// or expedited activities, other activities are forced to remain in the same order as they were
    /// before the move.
    /// </summary>
    /// <param name="od">Original dispatcher.</param>
    public MoveDispatcher(MoveDispatcher a_od)
        : base(a_od) { }

    internal MoveDispatcher(MoveDispatcherDefinition a_dispatcherDefinition)
        : base(a_dispatcherDefinition) { }

    protected override KeyAndActivity CreateKey(InternalActivity a_activity, int a_activityIdx)
    {
        MoveDispatcherDefinition.Key.Type keyType;

        if (a_activity.InProduction())
        {
            keyType = MoveDispatcherDefinition.Key.Type.InProduction;
        }
        else if (a_activity.MoveIntoBatch)
        {
            keyType = MoveDispatcherDefinition.Key.Type.MoveIntoBatch;
        }
        else if (a_activity.OpOrPredOpBeingMoved || a_activity.BeingMoved || a_activity.Operation.ManufacturingOrder.BeingExpedited)
        {
            keyType = MoveDispatcherDefinition.Key.Type.MoveActivity;
        }
        else
        {
            keyType = MoveDispatcherDefinition.Key.Type.SequencedActivity;
        }

        MoveDispatcherDefinition.Key key = new (keyType, a_activity.OriginalScheduledStartTicks, a_activity.PrimaryResourceBlockActivityIndex, a_activity.OriginalSimultaneousSequenceIdxScore, a_activity.Id.ToBaseType());
        KeyAndActivity keyAndObject = new (key, a_activity);

        return keyAndObject;
    }

    #region ICloneable
    public override object Clone()
    {
        return new MoveDispatcher(this);
    }
    #endregion
}