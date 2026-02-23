namespace PT.Scheduler.Simulation.UndoReceive;

// [USAGE_CODE] UndoReceive: A base class for specifying whether to undo a transmission and whether to re-receive it.
/// <summary>
/// A base class for undoing and reapplying transmissions.
/// </summary>
internal class UndoReceive
{
    //TODO: we don't need 2 bools here
    /// <summary>
    /// Used by ScenarioDetail to indicate whether the received transmission should be undone and whether it
    /// should be re-received.
    /// </summary>
    //internal bool UndoReceivedTransmission { get; set; }

    /// <summary>
    /// Whether to re-receive the transmission.
    /// In some way the re-received transmission will be handled slightly differently to obtain a better result.
    /// </summary>
    internal bool ReReceiveTransmission { get; set; }

    /// <summary>
    /// Reset the values of the object.
    /// </summary>
    internal virtual void Reset()
    {
        //UndoReceivedTransmission = false;
        ReReceiveTransmission = false;
    }
}