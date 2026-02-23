namespace PT.Scheduler.Simulation.Customizations.Material;

/// <summary>
/// Used to track quantity informtation on a specific source of a material. This was originally added for tawsk 9514.
/// </summary>
public class SourceQtyData
{
    internal void ResetSimulationStateVariables()
    {
        TotalQty = 0;
        AvailableQty = 0;
        QtyConsumed = false;
    }

    /// <summary>
    /// The total quantity  produced by a source.
    /// </summary>
    internal decimal TotalQty { get; set; }

    /// <summary>
    /// The quantity available at the curent simulation clock. This deppends on scheduling features such as transfer quantity(in the case where material becomes available as it's finished) and how much
    /// material has been consumed by various material requirements.
    /// </summary>
    internal decimal AvailableQty { get; set; }

    /// <summary>
    /// Whether any quantity from the materials source has been consumed.
    /// </summary>
    internal bool QtyConsumed { get; set; }
}