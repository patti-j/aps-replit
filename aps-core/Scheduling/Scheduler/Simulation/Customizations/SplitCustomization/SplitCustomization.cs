namespace PT.Scheduler.Simulation.Customizations;

/// <summary>
/// The return value of the customization. Allows you to specify whether to split and whether to schedule.
/// </summary>
public class SplitResult
{
    /// <summary>
    /// Used as part of the return result from the customization. Allows you to specify whether to split and whether to schedule.
    /// </summary>
    public enum ESplitResultEnum
    {
        /// <summary>
        /// No splitting is necessary. Continue scheduling as normal.
        /// </summary>
        NoSplit,

        /// <summary>
        /// No splitting is necessary. But don't schedule the activity at this time.
        /// </summary>
        NoSplitAndDontSchedule,

        /// <summary>
        /// Split the ManufacturingOrder and schedule the activity.
        /// </summary>
        SplitMO,

        /// <summary>
        /// Split the Job.
        /// </summary>
        SplitJob
    }

    /// <summary>
    /// You can use this constructor if no splitting is to occur.
    /// </summary>
    /// <param name="a_sre">The type of no splitting to occur.</param>
    public SplitResult(ESplitResultEnum a_sre)
        : this(a_sre, 0)
    {
        if (SplitResultEnum == ESplitResultEnum.SplitMO || SplitResultEnum == ESplitResultEnum.SplitJob)
        {
            throw new APSCommon.PTValidationException("2462");
        }
    }

    /// <summary>
    /// Use this constructor to split by ratio.
    /// </summary>
    /// <param name="a_sre">The type of split.</param>
    /// <param name="a_splitOffRatio">The ratio of the original quantity to split off. This value must be greater than 0 and less than 1. </param>
    public SplitResult(ESplitResultEnum a_sre, decimal a_splitOffRatio)
    {
        SplitResultEnum = a_sre;
        SplitOffRatio = a_splitOffRatio;
        SplitOffMOQty = SplitOffMOQtyDefaultValue;

        if (SplitResultEnum == ESplitResultEnum.SplitMO || SplitResultEnum == ESplitResultEnum.SplitJob)
        {
            if (SplitOffRatio <= 0 || SplitOffRatio >= 1)
            {
                throw new APSCommon.PTValidationException("2463");
            }
        }
    }

    /// <summary>
    /// Use this constructor if you want to split by quantity instead of by ratio.
    /// </summary>
    /// <param name="a_sre">Whether to split by Job or MO.</param>
    /// <param name="a_splitOffRatio">The ratio of the original order to split off.</param>
    /// <param name="a_splitQty">The quantity of the original job or MO to split off.</param>
    public SplitResult(ESplitResultEnum a_sre, decimal a_splitOffRatio, decimal a_splitQty)
        : this(a_sre, a_splitOffRatio)
    {
        SplitOffMOQty = a_splitQty;
    }

    /// <summary>
    /// Specifies whether to split and whether to allow the activity to be scheduled.
    /// </summary>
    public ESplitResultEnum SplitResultEnum { get; private set; }

    /// <summary>
    /// The amount of the Manufacturing order to be split off. This should be between 0 and 1 and equals the QuantityToSplitOff/TotalQuantity.
    /// </summary>
    public decimal SplitOffRatio { get; private set; }

    private readonly decimal SplitOffMOQtyDefaultValue = -1;

    /// <summary>
    /// Whether the similarly named attributes value has been set.
    /// </summary>
    public bool UseSplitOffMOQty => SplitOffMOQty > 0;

    /// <summary>
    /// If this value is set, after the MO is split off, the split off Manufacturing Order's quantity will be changed to this value. Before using this value, check the similarly named property to determine
    /// whether this value has been set.
    /// </summary>
    public decimal SplitOffMOQty { get; private set; }

    public override string ToString()
    {
        string msg;
        if (SplitResultEnum == ESplitResultEnum.NoSplit || SplitResultEnum == ESplitResultEnum.NoSplitAndDontSchedule)
        {
            msg = string.Format("SplitResultEnum={0};", SplitResultEnum);
        }
        else if (UseSplitOffMOQty)
        {
            msg = string.Format("SplitResultEnum={0}; SplitOffMOQty={1}", SplitResultEnum, SplitOffMOQty);
        }
        else
        {
            msg = string.Format("SplitResultEnum={0}; SplitOffRatio={1}", SplitResultEnum, SplitOffRatio);
        }

        return msg;
    }
}