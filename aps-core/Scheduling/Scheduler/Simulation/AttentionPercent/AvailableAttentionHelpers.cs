namespace PT.Scheduler.Simulation;

/// <summary>
/// Shared functions used for multitasking resources.
/// </summary>
internal class AvailableAttentionHelpers
{
    /// <summary>
    /// Whether an attention percent lies within the valid range or 0 to 100.
    /// </summary>
    /// <param name="a_attentionPercent"></param>
    internal static void ValidateAttentionPercent(decimal a_attentionPercent)
    {
        if (a_attentionPercent < 0 || a_attentionPercent > 100)
        {
            //throw new APSCommon.PTValidationException("4085");
        }
    }
}