
using System.Text;

using PT.Common.Exceptions;
using PT.Common.Localization;

namespace PT.SchedulerDefinitions;

public class AlternatePathDefs
{
    // !ALTERNATE_PATH!; AlternatePathDefs.AutoUsePathEnum: No, Standard, ByReleaseOffsetTimeSpan, etc.
    public enum AutoUsePathEnum
    {
        /// <summary>
        /// The path is only release if it's the CurrentPath.
        /// </summary>
        // !ALTERNATE_PATH!; ---TODO--- Consider renaming this to Either Manual or DefaultBehavior or something that suggests what happens based on the Hierarchy and feature compatibility. Perhaps add an additional option for Manual.
        IfCurrent,

        /// <summary>
        /// The path is released as usual, based on things such as the optimize settings, activity JIT start dates, etc.
        /// </summary>
        RegularRelease,

        /// <summary>
        /// A TimeSpan that defines when the path becomes eligible for automatic selection. The Alternate Path will not be used before the default path's release date + AutoPathSelectionReleaseOffset. For
        /// instance if a ManufacturingOrder has 2 alternate paths and the Default Path's release date is January 1 and the second AlternatePath is setup for AutoPathSection with AutoPathSelectionReleaseOffset=1
        /// day. Then the second path could potentially be used on or after January 2nd. The path that ends up being selected will depend on your optimization rules and resource availability. This value isn't
        /// used to determine the release date of the Default Path.
        /// </summary>
        ReleaseOffsetFromDefaultPathsLatestRelease
    }
}

public class BufferDefs
{
    public enum WarningLevels
    {
        /// <summary>
        /// Less than or equal to 33% penetration.
        /// </summary>
        OK,

        /// <summary>
        /// More than 33% and less than or equal to 66% penetration.
        /// </summary>
        Warning,

        /// <summary>
        /// More than 66% and less than or equal to 100% penetration.
        /// </summary>
        Critical,

        /// <summary>
        /// More than 100% penetration.
        /// </summary>
        Late
    }

    public static WarningLevels GetBufferWarningFromPercent(decimal a_percent)
    {
        if (a_percent < 33)
        {
            return WarningLevels.OK;
        }

        if (a_percent < 66)
        {
            return WarningLevels.Warning;
        }

        if (a_percent <= 100)
        {
            return WarningLevels.Critical;
        }

        return WarningLevels.Late;
    }
}

/// <summary>
/// These are definitions that need to be used by Scheduler as well as the ERP Interface.
/// </summary>
public class BaseOperationDefs
{
    /// <summary>
    /// </summary>
    public enum omitStatuses
    {
        /// <summary>
        /// </summary>
        NotOmitted,

        /// <summary>
        /// </summary>
        OmittedByUser,

        /// <summary>
        /// </summary>
        OmittedAutomatically
    }

    /// <summary>
    /// Can be used to create Resource Requirements for an Operation.
    /// If used, then AutoCreatedCapabilityExternalId field must also be used and the Capabilities referenced
    /// by the Resource Requirements must be pre-defined or the System Option to auto-create Capabilities must be enabled.
    /// </summary>
    public enum autoCreateRequirementsType
    {
        /// <summary>
        /// Specify 'None' to avoid creating any Resource Requirements here.  They'll need to be specified explicitly in some other way.
        /// </summary>
        None,

        /// <summary>
        /// Create a single Resource Requirement for the Operation and use the AutoCreatedCapabilityExternalId as its required Capability.
        /// </summary>
        Resource,

        /// <summary>
        /// Create two Resource Requirements for the Operation, one for Labor and one for a Machine.  The Labor in this case is marked
        /// as the Primary Requirment (the one whose Capacity Intervals are used to calculate duration.  See Primary Resource Requirment.
        /// The Capability assigned to the Labor Requirement is named AutoCreatedCapabilityExternalId+'(L)'.
        /// The Capability assigned to the Machine Requirement is named AutoCreatedCapabilityExternalId+'(M)'.
        /// </summary>
        LaborAndMachine,

        /// <summary>
        /// Create two Resource Requirements for the Operation, one for Labor and one for a Machine.  The Machine in this case is marked
        /// as the Primary Requirment (the one whose Capacity Intervals are used to calculate duration.  See Primary Resource Requirment.
        /// The Capability assigned to the Machine Requirement is named AutoCreatedCapabilityExternalId+'(M)'.
        /// The Capability assigned to the Labor Requirement is named AutoCreatedCapabilityExternalId+'(L)'.
        /// </summary>
        MachineAndLabor
    }
}

/// <summary>
/// </summary>
public class BaseActivityDefs
{
    /// <summary>
    /// </summary>
    public enum onTimeStatuses
    {
        /// <summary>
        /// </summary>
        TooEarly = 0,

        /// <summary>
        /// </summary>
        AlmostLate,

        /// <summary>
        /// </summary>
        OnTime,

        /// <summary>
        /// </summary>
        Late,

        /// <summary>
        /// </summary>
        CapacityBottleneck,

        /// <summary>
        /// </summary>
        MaterialBottleneck,

        /// <summary>
        /// </summary>
        ReleaseDateBottleneck,
        Unknown
    }
}

/// <summary>
/// </summary>
public class BaseOrderDefs
{
    /// <summary>
    /// </summary>
    public enum accessLevels
    {
        /// <summary>
        /// </summary>
        ViewOnly = 0,

        /// <summary>
        /// </summary>
        Change,

        /// <summary>
        /// </summary>
        Full
    }
}

/// <summary>
/// </summary>
public class BaseResourceDefs
{
    /// <summary>
    /// </summary>
    public enum accessLevels
    {
        /// <summary>
        /// </summary>
        ViewOnly = 0,

        /// <summary>
        /// </summary>
        Change,

        /// <summary>
        /// </summary>
        Full
    }

    /// <summary>
    /// </summary>
    public enum resourceTypes
    {
        Machine,
        Operator,
        Supervisor,
        Engineer,
        Inspector,
        Team,
        Labor,
        Equipment,
        Tool,
        Subcontractor,
        Cell,
        WorkArea,
        WorkCenter,
        Bay,
        Transport,
        Container,
        Special,
        Technician,
        Fixture,
        Employee,
        Tank,
        Inbox
    } //update IsLabor, below, if new labor types are added.

    public static bool IsLabor(resourceTypes resType)
    {
        return resType == resourceTypes.Employee || resType == resourceTypes.Engineer || resType == resourceTypes.Inspector || resType == resourceTypes.Labor || resType == resourceTypes.Operator || resType == resourceTypes.Supervisor || resType == resourceTypes.Team || resType == resourceTypes.Technician;
    }
}

/// <summary>
/// </summary>
public class CapacityIntervalDefs
{
    /// <summary>
    /// </summary>
    /// Note: If any values are added, removed, or modified from this enum, the serialization of
    /// Capacity Interval has some functions that do some comparison with the integer value.
    /// This code may need to be modified accordingly so that things don't break.
    public enum capacityIntervalTypes
    {
        /// <summary>
        /// </summary>
        Online = 0,
        /// <summary>
        /// </summary>
        Offline = 3, 

        /// <summary>
        /// </summary>
        Occupied = 4,

        /// <summary>
        /// This means the interval is online but has been reserved for something.
        /// This might not actually be used and can be deleted.
        /// </summary>
        ReservedOnline = 5
    }

    /// <summary>
    /// </summary>
    public enum dayTypes
    {
        Day = 0,
        Weekday,
        WeekendDay,
        Weekend,
        Sunday,
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday
    }

    /// <summary>
    /// </summary>
    public enum months
    {
        January = 0,
        February,
        March,
        April,
        May,
        June,
        July,
        August,
        September,
        October,
        November,
        December
    }

    /// <summary>
    /// </summary>
    public enum occurrences
    {
        First = 0,
        Second,
        Third,
        Fourth,
        Last
    }

    /// <summary>
    /// </summary>
    public enum recurrences
    {
        NotRecurring = 0,
        Daily,
        Weekly,
        MonthlyByDayNumber,
        YearlyByMonthDay
    }

    /// <summary>
    /// </summary>
    public enum recurrenceEndTypes { NoEndDate = 0, AfterMaxNbrRecurrences, AfterRecurrenceEndDateTime }

    /// <summary>
    /// </summary>
    public enum capacityIntervalAdditionScopes { ResourceOnly = 0, Department, Plant, AllPlants }
}

public class CustomerDefs
{
    public enum ECustomerType { Customer, Supplier, Vendor }
}

/// <summary>
/// </summary>
public enum anchoredTypes
{
    /// <summary>
    /// </summary>
    Anchored,

    /// <summary>
    /// </summary>
    Free,

    /// <summary>
    /// </summary>
    SomeActivitiesAnchored
}

/// <summary>
/// </summary>
public enum holdTypes
{
    /// <summary>
    /// </summary>
    OnHold,

    /// <summary>
    /// </summary>
    Released,

    /// <summary>
    /// </summary>
    PartiallyOnHold
}

/// <summary>
/// </summary>
public enum lockTypes
{
    /// <summary>
    /// </summary>
    Locked,

    /// <summary>
    /// </summary>
    Unlocked,

    /// <summary>
    /// </summary>
    SomeBlocksLocked
}

/// <summary>
/// </summary>
public class VesselRequirementDefs
{
    /// <summary>
    /// </summary>
    public enum claimTimings
    {
        /// <summary>
        /// </summary>
        SetupStart = 0,

        /// <summary>
        /// </summary>
        RunStart,

        /// <summary>
        /// </summary>
        OverlapQtyAvailable,

        /// <summary>
        /// </summary>
        RunEnd,

        /// <summary>
        /// </summary>
        PostProcessingWaitSpanEnd,

        /// <summary>
        /// </summary>
        TransferSpanEnd
    }

    /// <summary>
    /// </summary>
    public enum releaseTimings
    {
        /// <summary>
        /// </summary>
        SetupStart = 0,

        /// <summary>
        /// </summary>
        RunStart,

        /// <summary>
        /// </summary>
        RunEnd,

        /// <summary>
        /// </summary>
        PostProcessingWaitSpanEnd,

        /// <summary>
        /// </summary>
        TransferSpanEnd
    }
}

/// <summary>
/// </summary>
public class InternalActivityDefs
{
    /// <summary>
    /// The current status of the Activity in production.  These values are all specified by the user or interface except 'Started' which is automatically displayed if the Activity is not yet in Setup or Run
    /// and has hours or time reported against it.
    /// </summary>
    public enum productionStatuses
    {
        /// <summary>
        /// The Activity is waiting for material, previous operations, or a release date before it will be ready to start.
        /// </summary>
        Waiting = 0,

        /// <summary>
        /// The Activity can be worked on as soon as the necessary Resources are available.
        /// </summary>
        Ready = 1,

        /// <summary>
        /// 0
        /// Either time or quantity has been reported for the Activity and it is currently in a Waiting or Ready state.
        /// </summary>
        Started = 2,

        /// <summary>
        /// The Activity is currently being setup on a Resource.  Once in Setup the Activity is scheduled near the front of the schedule and connot be moved.
        /// </summary>
        SettingUp = 3,

        /// <summary>
        /// The Activity is currently being run on a Resource.  If in Run status, the Activity is scheduled near the front of the schedule and connot be moved.
        /// </summary>
        Running = 4,

        /// <summary>
        /// The Activity is finished running but is now waiting for drying, cleanup, etc.  The resource capacity is still used
        /// </summary>
        PostProcessing = 5,

        /// <summary>
        /// The Activity is finished running but is now waiting for drying, cleanup, etc.  The resource capacity is still used
        /// </summary>
        Storing = 6,

        /// <summary>
        /// The Activity is finished running but is now cleaning. The resource capacity is still used
        /// </summary>
        Cleaning = 7,

        /// <summary>
        /// The Activity is complete in production and is ready for the successor operation or is in inventory. Once Finished, it is no longer scheduled.
        /// </summary>
        Finished = 8
    }

    /// <summary>
    /// Determines how many people are allocated to an Activity in the schedule.
    /// </summary>
    public enum peopleUsages
    {
        /// <summary>
        /// All of the people specified in the Resource's Capacity Interval are used by the Activity to get it done as fast as possible.
        /// </summary>
        UseAllAvailable = 0,

        /// <summary>
        /// Only up to this number of the Resource's people are allocated to the Activity thus leaving the other people available to work on other Activities.
        /// </summary>
        UseSpecifiedNbr,

        /// <summary>
        /// The capacity multiplier must be a multiple of a given number. For instance, if an activity's multiplier is 4 and a resources interval has a multiplier of 11. A multipler of 8 will be used when the
        /// activity is scheduled.
        /// </summary>
        UseMultipleOfSpecifiedNbr
    }
}

/// <summary>
/// </summary>
public class InternalOperationDefs
{
    /// <summary>
    /// </summary>
    public enum overlapTypes
    {
        /// <summary>
        /// Successor Operation cannot schedule to start before this Operation is scheduled to be finished and transferred.
        /// </summary>
        NoOverlap = 0,

        /// <summary>
        /// Successor Operation can start once the Transfer Qty is completed and transferred from this Operation.
        /// </summary>
        TransferQty = 1,

        /// <summary>
        /// Successor Operation can start once the TransferSpan has passed (in calendar time) after the Operation's Setup start (or Run start if not setup).
        /// </summary>
        TransferSpan = 2,

        /// <summary>
        /// Successor Operation can start when the first transfer of material from the predecessor has completed.  This is based on the QtyPerCycle of the predecessor Operation.
        /// </summary>
        AtFirstTransfer = 3,

        /// <summary>
        /// Successor Operation can start once the TransferSpan has passed (in calendar time) after the Operation's Scheduled Run Start.
        /// </summary>
        TransferSpanAfterSetup = 4,

        /// <summary>
        /// Overlap is based on percent complete of total required cycle capacity. Setup time and post procesing don't affect when overlap can occur
        /// with this option. Overlap is based on the start of processing. Processing Time and Material Post-Processing are added together and the
        /// percent of the result is the amount of time that must have passed before the successor operation can be released. This feature currently
        /// only works in combination with time based reporting.
        /// The predecessor operations TimeBasedReporting flag must be set to true.
        /// </summary>
        PercentComplete = 5,

        /// <summary>
        /// The successor can start up to the OverlapTransferSpan before the start of the predcessor operation. This allows successor operations with relatively long
        /// setup times to be ready to start processing once the predecessor's processing has been completed. There the only way to constrain the start of processing on
        /// the successor so it matches the end of the predecessor is to have the predecessor create a product released and used by the successor.
        /// This was created for use with tanks whose successor operations have very long setup times. In the ideal schedule, the successor operation starts processing
        /// as soon as the tanks completes. The only for the successor to start right after the tank has completed is for setup on the successor operation to start
        /// before the predecessor starts. Material produced by the predecessor that the successor required constrained the predecessor's processing start date to the
        /// time when the material had finished processing in the tank. This is necessary since it's not possible to set the overlap transfer span based on the exact
        /// amount of time it will take the tank to finish processing (resource rates, product rules, and offline intervals can affect when the tank will complete).
        /// </summary>
        [Obsolete("Deleted.")] TransferSpanBeforeStartOfPredecessor
    }

    /// <summary>
    /// Can be used to automatically finish an Operation based on status reporting on a successor operation.
    /// This can be useful when reporting on each operation is not practical.
    /// </summary>
    public enum autoFinishPredecessorOptions
    {
        /// <summary>
        /// The Operation must be finished explicitly.
        /// </summary>
        NoAutoFinish = 0,

        /// <summary>
        /// The Operation is finished when the successor has its Production Status set to Setup or when it has Setup Hours Reported.
        /// </summary>
        OnSuccessorSetupStart,

        /// <summary>
        /// The Operation is finished when the successor has its Production Status set to Run or when it has Run Hours, Good Qty, or Scrap Qty Reported.
        /// </summary>
        OnSuccessorRunStart,

        /// <summary>
        /// The Operation is finished when the successor has its Production Status set to PostProcessing or when it has PostProcessing Hours Reported.
        /// </summary>
        OnSuccessorPostProcessingStart,

        /// <summary>
        /// The Operation is finished when the successor has its Production Status set to Finished.
        /// </summary>
        OnSuccessorFinish
    }

    //add Optimal later
    /// <summary>
    /// </summary>
    public enum splitUpdateModes
    {
        /// <summary>
        /// Hour and quantity reporting for the Operation must specify the ExternalId of the Activity to update.
        /// </summary>
        UpdateSplitsIndividually = 0,

        /// <summary>
        /// When hours or quantity are reported for the Operation they are allocated based on the Scheduled Start Dates of the Activities.
        /// Earlier ones receive the allocation up to their planned amounts and then are marked as Finished.  Then subsequent Activities receive the remainder of the hours or quantity reported.
        /// </summary>
        ShareReportedValuesChronologically,

        /// <summary>
        /// When hours or quantity are reported for the Operation they are allocated evenly across all Activities, dividing into decimal values for quantities.
        /// </summary>
        ShareReportedValuesEvenlyDecimal,

        /// <summary>
        /// When hours or quantity are reported for the Operation they are allocated evenly across all Activities, rounded into integer values for quantities.
        /// </summary>
        ShareReportedValuesEvenlyInteger
    }
}

/// <summary>
/// </summary>
public class InternalResourceDefs
{
    /// <summary>
    /// </summary>
    public enum capacityTypes
    {
        /// <summary>
        /// The Resource is never considered busy.  No queueing is scheduled, instead work is assummed to start as soon as the work arrives and the Capacity Intervals allow.
        /// This is often used for subcontractor Operations where a fixed lead-time is used (the Operation time) rather than detailed scheduling.
        /// </summary>
        Infinite,

        /// <summary>
        /// Only a single Activity can be peformed at any point in time.  Most machines and people are scheduled as SingleTasking.  Other work must wait in queue until the Resource is done with the current
        /// Activity.
        /// </summary>
        SingleTasking,

        /// <summary>
        /// The Resource can work on multiple Activities simultaneously.  The maximum number of simultaneous Activities is limited by the AttentionPercent of the Operation.
        /// Once 100% of the Resource's attention is allocated then no new work can begin until other Activities finish.
        /// </summary>
        MultiTasking
    }
}

/// <summary>
/// </summary>
public class JobDefs
{
    /// <summary>
    /// The static value initializer. This should only be used to initialize readonly values.
    /// </summary>
    static JobDefs()
    {
        ExcludedReasonsLength = Enum.GetNames(typeof(ExcludedReasons)).Length;
    }

    /// <summary>
    /// This is for display only and has no impact on scheduling.
    /// </summary>
    public enum classifications
    {
        /// <summary>
        /// </summary>
        ProductionOrder = 0,

        /// <summary>
        /// </summary>
        Quote,

        /// <summary>
        /// </summary>
        SafetyStock,

        /// <summary>
        /// </summary>
        Forecast,

        /// <summary>
        /// </summary>
        Template,

        /// <summary>
        /// </summary>
        TransferOrder,

        /// <summary>
        /// </summary>
        BufferStockReplenishment,

        /// <summary>
        /// If Sales Order is selected, then Job’s Material demand is displayed in the Sales Orders row instead of the Job Materials row in the Inventory Plan.
        /// </summary>
        SalesOrder
    }

    /// <summary>
    /// This is for display only and has no impact on scheduling.
    /// </summary>
    public enum commitmentTypes
    {
        /// <summary>
        /// Usually a quote to use for generating an expected ship time.
        /// </summary>
        Estimate = 0,

        /// <summary>
        /// Usuall a computer generated order that has not yet been firmed by a planner.
        /// </summary>
        Planned,

        /// <summary>
        /// An order that has been firmed by a planner and will be produced.
        /// </summary>
        Firm,

        /// <summary>
        /// Released to production.
        /// </summary>
        Released
    }

    /// <summary>
    /// Indicates where the Job came from.
    /// </summary>
    public enum EMaintenanceMethod
    {
        /// <summary>
        /// Enter via the interface, usually from an ERP system.
        /// </summary>
        ERP = 0,

        /// <summary>
        /// Created by a user from within PlanetTogether.
        /// </summary>
        Manual,

        /// <summary>
        /// Created using the Job Generator dialog.
        /// </summary>
        AutoGenerated,

        /// <summary>
        /// Create by the Job Generator
        /// </summary>
        JobGenerator,

        /// <summary>
        /// Generated by PlanetTogether MRP.
        /// </summary>
        MrpGenerated
    }

    //If the scheduledStatuses have their integer values changed, you must update DemandManagement.TableManager property!
    /// <summary>
    /// Indicates the various stages that a Job can be in pertaining to its being scheduled.
    /// </summary>
    public enum scheduledStatuses
    {
        /// <summary>
        /// No attempt has yet been made to schedule the Job.
        /// </summary>
        New = 0,

        /// <summary>
        /// The Job's schedulable Activities have all been successfully scheduled on Resources.
        /// </summary>
        Scheduled,

        /// <summary>
        /// The Job has been manually Unscheduled.
        /// </summary>
        Unscheduled,

        /// <summary>
        /// The Job cannot schedule because the necessary Resources are not available.
        /// This is usually due to an Operation requiring Capabilities that do not have enough Active Resources.
        /// See the Job Analysis for more details.
        /// </summary>
        FailedToSchedule,

        /// <summary>
        /// All Operations have been finished so the Job is no longer scheduled.
        /// </summary>
        Finished,

        /// <summary>
        /// The Job has been marked as Cancelled either by a user or through the interface.
        /// </summary>
        Cancelled,

        /// <summary>
        /// The Job has been excluded from scheduling based on the Optimize Settings.
        /// </summary>
        Excluded,

        /// <summary>
        /// The Job is only for use as a Template, for copying to other Jobs and is therefore not scheduled.
        /// </summary>
        Template,

        /// <summary>
        /// Some MOs are scheduled but some are not.
        /// </summary>
        PartiallyScheduled
    }

    /// <summary>
    /// Released status of the MOs
    /// </summary>
    public enum EMoReleasedStatus
    {
        /// <summary>
        /// No MOs have been released
        /// </summary>
        None = 0,

        /// <summary>
        /// Some but not all MOs have been released
        /// </summary>
        Partially,

        /// <summary>
        /// All MOs have been released
        /// </summary>
        Released
    }

    [Flags]
    public enum ExcludedReasons
    {
        /// <summary>
        /// The field hasn't been set.
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// No reason has been given.
        /// </summary>
        NotExcluded = 1,

        /// <summary>
        /// The Job's 'DoNotSchedule' option is checked.
        /// </summary>
        ExcludedDoNotSchedule = 2,

        /// <summary>
        /// Excluded from being scheduled by ExcludeTentativeJobs optimize setting.
        /// </summary>
        ExcludedPlanned = 4,

        /// <summary>
        /// Excluded from being scheduled by ExcludeWhatIfJobs optimize setting.
        /// </summary>
        ExcludedEstimate = 8,

        /// <summary>
        /// Excluded from being scheduled by ExcludeOnHoldJobs optimize setting.
        /// </summary>
        ExcludedOnHold = 16,

        /// <summary>
        /// Exclude Jobs that are New
        /// </summary>
        ExcludedNew = 32,

        /// <summary>
        /// A Trial/Demo key is being used, the maximum number of jobs the optimizer plans to schedule has been reached.
        /// </summary>
        ExcludedMaxTrialDemoLimit = 64,

        /// <summary>
        /// Exclude Jobs that are unscheduled.
        /// </summary>
        ExcludedUnscheduled = 128
    }

    /// <summary>
    /// The number literals of the enumeration ExcludedReasons.
    /// </summary>
    public static readonly int ExcludedReasonsLength;

    public enum ShippedStatuses
    {
        /// <summary>
        /// Fully qty has been shipped.
        /// </summary>
        Shipped,

        /// <summary>
        /// Nothing yet shipped.
        /// </summary>
        NotShipped,

        /// <summary>
        /// Partial qty has been shipped.
        /// </summary>
        Partially
    }

    /// <summary>
    /// Used to indicate whether a split should generate a second Job or a second MO.
    /// </summary>
    public enum SplitScopeEnum { Job, MO }
}

public class CtpDefs
{
    public enum warehouseEligibilities
    {
        /// <summary>
        /// Individual materials must come from the same Warehouse,
        /// </summary>
        IndividualMaterialsFromSameWarehouse,

        /// <summary>
        /// Individual materials can come from multiple Warehouses
        /// </summary>
        IndividualMaterialsFromMultipleWarehouses
    }

    public enum ESchedulingType
    {
        /// <summary>
        /// Optimize at the of CTP
        /// </summary>
        Optimize,

        /// <summary>
        /// Expedite the new Job to Clock (do ASAP)
        /// </summary>
        ExpediteToClock,

        /// <summary>
        /// Expedite the new Job to it's JIT Start Date.
        /// </summary>
        ExpediteToJIT
    }
}

/// <summary>
/// </summary>
public class MainResourceDefs
{
    /// <summary>
    /// </summary>
    public enum releaseRules
    {
        /// <summary>
        /// </summary>
        None = 0,

        /// <summary>
        /// </summary>
        ConWIP,

        /// <summary>
        /// </summary>
        DrumBufferRope,

        /// <summary>
        /// </summary>
        KanBan,

        /// <summary>
        /// </summary>
        CriticalPath
    }

    /// <summary>
    /// </summary>
    public enum processingStatuses
    {
        /// <summary>
        /// </summary>
        SettingUp = 0,

        /// <summary>
        /// </summary>
        Running,

        /// <summary>
        /// </summary>
        TearingDown,

        /// <summary>
        /// </summary>
        Idle
    }

    #region Usage
    // [USAGE_CODE] usageEnum: The steps of an operation's manufacturing process.
    /// <summary>
    /// The steps of an operation's manufacturing process. Below they're defined in the order in which they're performed.
    /// </summary>
    public enum usageEnum
    {
        // Each enumeration is identifiable by a single bit
        // There are plenty of spaces between bits for future expansion
        // The sum of all enumeration values before enumeration "x" is less than
        // the value of enumeration "x" even with future expansion.
        Unspecified = 0,
        Setup = 16, // bit 4
        Run = 512, // bit 9
        PostProcessing = 16384, // bit 14
        //Clean = 65536, // bit 16
        Storage = 524288, // bit 19
        //StoragePostProcessing = 16777216, // bit 24
        Clean = 134217728 // bit 27
    }

    /// <summary>
    /// The release branch's value for setup was incorrect.
    /// </summary>
    /// <param name="r_usage"></param>
    public static void FixUsageEnum(ref usageEnum r_usage)
    {
        // At one point it was accidentally defined as this.
        const int releaseBranchIncorrectSetupValue = 26;

        int x = (int)r_usage;
        if (x == releaseBranchIncorrectSetupValue)
        {
            r_usage = usageEnum.Setup;
        }
    }

    /// <summary>
    /// An exception thrown while validating Usages.
    /// </summary>
    public class UsageValidationException : PTException
    {
        public UsageValidationException() { }

        public UsageValidationException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = true)
            : base(Localizer.GetErrorString(a_message, a_stringParameters, a_appendHelpUrl)) { }
    }

    // [USAGE_CODE] Usage: Created to convert original Usage end to the new type of Usage with a start and end. A continuous interval of an operation's manufacturing process. This class also has a validation function to validate a list of Usages.
    /// <summary>
    /// A continuous interval of an operation's manufacturing process.
    /// </summary>
    public class Usage
    {
        /// <summary>
        /// Create a Usage. The usage's end must be greater than or equal to the start.
        /// Exceptions thrown:
        /// UsageException: thrown if end isn't greater than or equal to start.
        /// </summary>
        /// <param name="a_start">The start of the usage.</param>
        /// <param name="a_end">The end of usage, which must be greater than or equal to start.</param>
        public Usage(usageEnum a_start, usageEnum a_end)
        {
            if (a_start > a_end)
            {
                throw new UsageValidationException("2873", new object[] { a_start, a_end });
            }

            Start = a_start;
            End = a_end;
        }

        /// <summary>
        /// The start of the interval.
        /// </summary>
        public readonly usageEnum Start;

        /// <summary>
        /// The end of the interval. The end must be greater than or equal to the start.
        /// </summary>
        public readonly usageEnum End;

        /// <summary>
        /// Validate the usages of an operation's resource requirements.
        /// Throws a UsageException if any of the usages are invalid or among all usages the primary resource requirement's usage doesn't include both the minimum usage start or
        /// of the maximum usage end.
        /// </summary>
        /// <param name="a_usages">The usages of each resource requirement. Each index corresponds to a single requirement's start and end of usage.</param>
        /// <param name="a_primaryRRIdx">The index of the usage that for the primary resource requirement.</param>
        /// <returns></returns>
        public static void ValidateUsages(List<Usage> a_usages, int a_primaryRRIdx)
        {
            const string fx = "ValidatePrimaryRRUsages";
            if (a_usages.Count == 0)
            {
                throw new UsageValidationException("No usages were passed to " + fx);
            }

            if (a_primaryRRIdx < 0)
            {
                throw new UsageValidationException(a_primaryRRIdx + " is an invalid primary resource requirement index in " + fx);
            }

            int lastIdx = a_usages.Count - 1;
            if (a_primaryRRIdx > lastIdx)
            {
                throw new UsageValidationException(a_primaryRRIdx + " is an invalid primary resource requirement index in " + fx + ". There are only " + a_usages.Count + " usages.");
            }

            Usage first = a_usages[0];
            usageEnum min = first.Start;
            usageEnum max = first.End;

            for (int i = 0; i < a_usages.Count; ++i)
            {
                Usage u = a_usages[i];
                if (u.Start < min)
                {
                    min = u.Start;
                }

                if (u.End > max)
                {
                    max = u.End;
                }
            }

            Usage primary = a_usages[a_primaryRRIdx];

            if (primary.Start != min)
            {
                throw new UsageValidationException("2874", new object[] { primary.Start, min });
            }

            if (primary.End != max)
            {
                throw new UsageValidationException("2875", new object[] { primary.End, max });
            }
        }
        #endregion // Usage

        /// <summary>
        /// Primary RR Usage cannot End before Run if ProductionStatus is Running
        /// Primary RR Usage cannot End before PostProcessing if ProductionStatus is PostProcessing
        /// </summary>
        /// <param name="a_primaryUsageEnd"></param>
        /// <param name="a_prodStatus"></param>
        /// <returns></returns>
        public static bool IsProductionStatusValidForUsage(usageEnum a_primaryUsageEnd, IEnumerable<InternalActivityDefs.productionStatuses> a_prodStatus)
        {
            foreach (InternalActivityDefs.productionStatuses status in a_prodStatus)
            {
                if ((status == InternalActivityDefs.productionStatuses.Running && a_primaryUsageEnd < usageEnum.Run) ||
                    (status == InternalActivityDefs.productionStatuses.PostProcessing && a_primaryUsageEnd < usageEnum.PostProcessing))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool Intersects(usageEnum a_xStart, usageEnum a_xEnd, usageEnum a_yStart, usageEnum a_yEnd)
        {
            if (After(a_xEnd, a_yStart) || After(a_yEnd, a_xStart))
            {
                return false;
            }

            return true;
        }

        private static bool After(usageEnum a_u1End, usageEnum a_u2Start)
        {
            return a_u2Start > a_u1End;
        }
    }

    public enum batchType
    {
        None = 0,

        /// <summary>
        /// Batch by percent of a single cycle; Activities on such resources must fit within a single cycle. The fraction an activity consumes of a single cycle equals the activity's quantity divided by the
        /// quantity per cycle (qty/qtyPerCycle).
        /// Typically the products of multiple activities within a single batch would be identicle or nearly identicle so that qty/qtyPerCycle make sense.
        /// </summary>
        Percent = 1,

        /// <summary>
        /// The volume is defined by the resource and corresponds directly to the quantity being processed by the activity scheduled for the batch. For instance if the volume of a resource is 10 Activities for
        /// quantities of 4, 2, 3, and 1 would
        /// all fit into the batch.
        /// </summary>
        Volume = 2
    }
}

/// <summary>
/// </summary>
public class ManufacturingOrderDefs
{
    // !ALTERNATE_PATH!; This might be replaced by the enums defined below.
    // !ALTERNATE_PATH!; ---TODO--- Determine whether to delete this.
    /// <summary>
    /// </summary>
    public enum alternatePathSelections
    {
        /// <summary>
        /// </summary>
        Automatic,

        /// <summary>
        /// </summary>
        AutomaticIfLate,

        /// <summary>
        /// </summary>
        Manual
    }

    // !ALTERNATE_PATH!; LockedToCurrentAlternatePathStateReasonEnum
    /// <summary>
    /// Used to indicate whether an MO must be scheduled using the current AlternatePath and the reason.
    /// Each value is set to a specific bit of a number of type long. This allows you to specify multiple
    /// reasons (up to 64) with a single variable.
    /// Note there are several differences between how these flags are treated by Optimize and Expedite.
    /// </summary>
    public enum LockedToCurrentAlternatePathStateReasonEnum : long
    {
        /// <summary>
        /// The MO can be scheduled to any of it's paths.
        /// </summary>
        Free = 0,

        /// <summary>
        /// The LockToCurrentPath flag of the MO has been set.
        /// </summary>
        LockToCurrentPathIsSet = 1,

        /// <summary>
        /// The MO only has a single path.
        /// </summary>
        SinglePath = 2,

        /// <summary>
        /// Any MO that has any Reported Progress or Started activities will be locked to the Current Path.
        /// </summary>
        Started = 4,

        /// <summary>
        /// During an Optimize a Resource Lock will restrict the MO to the Current Path.
        /// A Resource Lock won't won't retrict an Expedite to the Current Path.
        /// </summary>
        ResourceLocked = 8,

        /// <summary>
        /// During an Optimize an MO with Anchors will be retricted to the Current Path.
        /// During an Expedite an MO with Anchors will be restricted to the Current Path unless the Anchor After Expedite Setting is turned on.
        /// </summary>
        Anchored = 16
    }

    /// <summary>
    /// Specifies how updates to Manufacturing Orders are processed when the MO has been previously split.
    /// </summary>
    public enum SplitUpdateModes
    {
        /// <summary>
        /// Hour and quantity reporting for the Operation must specify the ExternalId of the MO to update, similar to when an MO has not been split.
        /// </summary>
        UpdateSplitsIndividually,

        /// <summary>
        /// When hours or quantity are reported for the Manufacturing Order they are allocated across all Manufacturing Orders and their Activities in proportion to their Required Qty, allowing decimal values
        /// for quantities.
        /// </summary>
        ShareReportedValuesProportionallyDecimal,

        /// <summary>
        /// When hours or quantity are reported for the Manufacturing Order they are allocated across all Manufacturing Orders and their Activities  in proportion to their Required Qty, rounded into integer
        /// values for quantities.
        /// </summary>
        ShareReportedValuesProportionallyInteger
        ///// <summary>
        ///// When hours or quantity are reported for the Manufacturing Order they are allocated based on the Scheduled Start Dates of the Activities.   
        ///// Earlier ones receive the allocation up to their planned amounts and then are marked as Finished.  
        ///// Then subsequent Activities receive the remainder of the hours or quantity reported.
        ///// </summary>
        //ShareReportedValuesChronologically,
        ///// <summary>
        ///// When hours or quantity are reported for the Manufacturing Order they are allocated based on incremental index of the MO ID (1 then 2, etc).  
        ///// Earlier ones receive the allocation up to their planned amounts and then are marked as Finished.  
        ///// Then subsequent Activities receive the remainder of the hours or quantity reported.
        ///// </summary>
        //ShareReportedValuesByIndex,           
        ///// <summary>
        ///// When hours or quantity are reported for the Manufacturing Order they are allocated evenly across all Activities scheduled to start before the current PC time, dividing into decimal values for quantities.  
        ///// </summary>
        //ShareReportedValuesToStartedActivitesEvenlyDecimal,
        ///// <summary>
        ///// When hours or quantity are reported for the Manufacturing Order they are allocated evenly across all Activities scheduled to start before the current PC time, rounded into integer values for quantities.  
        ///// </summary>
        //ShareReportedValuesToStartedActivitiesEvenlyInteger  
    }
}

public class ProductDefs
{
    public enum EInventoryAvailableTimings
    {
        /// <summary>
        /// All produced inventory is considered available at the schedule start of the Operation's Run time.
        /// This can be used in situations where operations consuming the material should be allowed to schedule
        /// overlapping the operation producing the material.
        /// If Material Post-Processing Time is defined the release time is delayed by that time span.
        /// </summary>
        AtOperationRunStart,

        /// <summary>
        /// All produced inventory is considered available at the end of the Operation and any Resource Transfer time.
        /// </summary>
        AtOperationResourcePostProcessingEnd,

        /// <summary>
        /// Material becomes available as it is produced in increments equal to its transfer quantity. Since a lot of extra bookingkeeping is involoved
        /// in this type of timing, scheduling might take longer.
        /// The advantages to running in this mode include decreased lead-times and decreased inventory of intermediate items; this is known as material
        /// overlap. To run in material overlap mode
        /// 1) use this setting,
        /// 2) the item being produced must have a transfer quantity,
        /// 3) material requests that use the item must set UseOverlapActivities (there's a corresponding flag for POs) and
        /// 4) if possible you should also consider staging the scheduling of your resources to improve your level of overlap. The stage field is available
        /// on resources.
        /// </summary>
        ByProductionCycle,

        /// <summary>
        /// All produced inventory is considered available at the schedule end of the Operation's Run time.
        /// This can be used in situations where resource post-processing does not affect material production
        /// </summary>
        AtOperationRunEnd,

        /// <summary>
        /// Material is transferred evenly over the full duration of PostProcessing. This will occupy a StorageConnector if one is used to store material
        /// </summary>
        DuringPostProcessing,

        /// <summary>
        /// Material is transferred evenly over the full duration of Storage. This will occupy a StorageConnector if one is used to store material
        /// </summary>
        DuringStorage,

        /// <summary>
        /// Material is Instantly moved at the end of storage
        /// </summary>
        AtStorageEnd,
    }
}

/// <summary>
/// </summary>
public class MaterialRequirementDefs
{
    public enum EMaterialUsedTiming
    {
        SetupStart,
        
        DuringSetup,

        ProductionStart,

        ByProductionCycle,

        LastProductionCycle,

        PostProcessingStart,

        PostProcessingEnd,
        
        FirstAndLastProductionCycle,
    }

    /// <summary>
    /// Controls the effect a Material Requirement has on the scheduling of the Operation it will be used in.
    /// </summary>
    public enum constraintTypes
    {
        /// <summary>
        /// If the material is available it will have been allocated, otherwise it's unavailability won't constrain the activity.
        /// </summary>
        NonConstraint = 0,

        /// <summary>
        /// The Operation using the Material can't schedule to start before the earlier of the current time plus the Material Lead-time or the AvailableDate
        /// </summary>
        ConstrainedByEarlierOfLeadTimeOrAvailableDate,

        /// <summary>
        /// The Operation using the Material can't schedule to start before the Materials' Available Date.
        /// </summary>
        ConstrainedByAvailableDate,

        /// <summary>
        /// Non-Constraint but a placeholder so the original constraint can be restored
        /// </summary>
        IgnoredConstrainedByEarlierOfLeadTimeOrAvailableDate,

        /// <summary>
        /// Non-Constraint but a placeholder so the original constraint can be restored
        /// </summary>
        IgnoredConstrainedByAvailableDate,
    }

    /// <summary>
    /// Specifies whether the material is from stock or buy direct.
    /// </summary>
    public enum requirementTypes
    {
        /// <summary>
        /// The material is custom, for the Job.  No inventory planning functionality is used.  \
        /// Instead, the dates set in the Material Requirement are used.
        /// </summary>
        BuyDirect = 0,

        /// <summary>
        /// The material is coming from stock and the inventory planning functionality is used to determine its availability.
        /// </summary>
        FromStock
    }

    //[TANK_STORAGE]
    /// <summary>
    /// If the material is drawn from a tank resources, these values can be used to indicate the timing of when the tank is empty.
    /// </summary>
    public enum TankStorageReleaseTimingEnum
    {
        /// <summary>
        /// Not expected to be drawn from a tank.
        /// </summary>
        NotTank,

        /// <summary>
        /// Release the tank when the activity consuming the material starts.
        /// </summary>
        AtActivityStart,

        /// <summary>
        /// Release the tank when the activity consuming the material has finished being setup.
        /// </summary>
        AtEndOfActivitySetup,

        /// <summary>
        /// Release the tank when the activity consuming the material has finished processing.
        /// </summary>
        AtEndOfProcessing,

        /// <summary>
        /// Release the tank when the activity consuming the material in the tanks has finished its post processing.
        /// </summary>
        AtEndOfPostProcessing,

        /// <summary>
        /// Release the tank when the activity consuming the material in the tanks has withdrawn all of the material (assuming constant flow).
        /// </summary>
        AtEndOfTankWithdrawal
    }
}

/// <summary>
/// </summary>
public class SubcontractorActivityDefs
{
    /// <summary>
    /// </summary>
    public enum statuses
    {
        /// <summary>
        /// </summary>
        NotSent,

        /// <summary>
        /// </summary>
        Sent,

        /// <summary>
        /// </summary>
        Received
    }
}

/// <summary>
/// </summary>
public class UserDefs
{
    /// <summary>
    /// Controls which Scenarios a User will have access to.
    /// </summary>
    public enum scenarioAccessLevels
    {
        /// <summary>
        /// Can only view the Published Scenario only (if there is one).  No changes are allowed.
        /// </summary>
        ViewPublished = 0,

        /// <summary>
        /// Can view all Scenarios but can make no changes.
        /// </summary>
        ViewAll,

        /// <summary>
        /// Can view all Scenarios and can make changes to and create What-If Scenarios.  No changes can be made to the Published or Live Scenarios.
        /// </summary>
        ViewAllEditWhatifs,

        /// <summary>
        /// Can view and edit all Scenarios including the Live, Published, and What-If Scenarios.
        /// </summary>
        MasterSchedulerDepricated,

        /// <summary>
        /// Can only view the Live Scenario only.  No changes are allowed.
        /// </summary>
        ViewLive,

        /// <summary>
        /// View published and what if scenarios
        /// </summary>
        ViewWhatIfs,

        /// <summary>
        /// View live and update status
        /// </summary>
        ShopFloor,

        /// <summary>
        /// View live and run CTP
        /// </summary>
        CustomerService,

        /// <summary>
        /// View live and maintain resources and capacity
        /// </summary>
        Operations,

        /// <summary>
        /// View all scenarios and edit what-ifs
        /// </summary>
        Planner,

        /// <summary>
        /// Schedule on the live and and what if scenarios
        /// </summary>
        Scheduler,

        /// <summary>
        /// Schedule and maintain data on all scenarios
        /// </summary>
        MaintainData,

        /// <summary>
        /// MaintainData and run import and publish
        /// </summary>
        MasterScheduler,

        /// <summary>
        /// Full system access including managing users
        /// </summary>
        SystemAdministrator,

        /// <summary>
        /// Currently unused
        /// </summary>
        Super
    }

    /// <summary>
    /// </summary>
    public enum impactAnalysisShowOption
    {
        /// <summary>
        /// </summary>
        KpiOnly,

        /// <summary>
        /// </summary>
        Optimize,

        /// <summary>
        /// </summary>
        AllChanges
    }

    /// <summary>
    /// </summary>
    public enum impactAnalysisInstigatorOption
    {
        /// <summary>
        /// </summary>
        MeOnly,

        /// <summary>
        /// </summary>
        AllUsers
    }

    public enum publishOnExitReminders
    {
        /// <summary>
        /// Always Publish (silently) if the schedule has changed since the last Publish and the User is exiting.
        /// </summary>
        PublishOnExit,

        /// <summary>
        /// Ask the User before exiting whether to Publish if the schedule has changed since the last Publish.
        /// </summary>
        AskUser,

        /// <summary>
        /// Don't Publish and don't ask the user.
        /// </summary>
        Disabled
    }

    public enum EPermissions : short
    {
        MaintainCustomers,

        /// <summary>
        /// Permits access to Job maintenance functions.
        /// </summary>
        MaintainJobs,

        /// <summary>
        /// Permits access to capacity interval maintenance functions.
        /// </summary>
        MaintainCapacity,

        /// <summary>
        /// Permits access to overtime capacity regardless of other restrictions.
        /// </summary>
        MaintainOvertime,
        MaintainForecasts,

        /// <summary>
        /// Permits access to the Interface Wizard to modify interface settings.
        /// </summary>
        MaintainInterface,

        /// <summary>
        /// Permits execution of the Interface to import data.
        /// </summary>
        RunInterface,

        /// <summary>
        /// Permits access to Script maintenance functions.
        /// </summary>
        MaintainScripts,

        /// <summary>
        /// Permits access to maintenance functions for: Plants, Resources, Calendars, Cells, and Capabilities.
        /// </summary>
        MaintainResources,

        /// <summary>
        /// Permits changing of priorities for Jobs, Manufacturing Orders and Customers.
        /// </summary>
        SetPriorities,

        /// <summary>
        /// If true then ERP actions can be undone and redone by the user.
        /// </summary>
        UndoERPActions,

        /// <summary>
        /// If true then actions of other users can be undone and redone by the user.
        /// </summary>
        UndoOtherUserActions,

        /// <summary>
        /// Show ERP and other users actions in the undo/redo lists even if they are unavailable for undo/red.
        /// </summary>
        UndoShowUnavailableActions,

        /// <summary>
        /// Enables the User to be added to Resource Shop View Users and have access to Resources that have no such list.
        /// </summary>
        ShopViewsView,

        /// <summary>
        /// Whether the User can edit personal preferences in Shop Views.
        /// </summary>
        ShopViewsEdit,

        /// <summary>
        /// Whether the User is permitted to reschedule Purchases using the Dock Schedule Board.
        /// </summary>
        PurchasesReschedule,

        /// <summary>
        /// Whether the user is allowed to Anchor and Un-Anchor Jobs.
        /// </summary>
        Anchor,

        /// <summary>
        /// Whether the user is allowed to Lock and Unlock Jobs.
        /// </summary>
        Lock,
        Optimize,
        Compress,
        CompressJIT,

        /// <summary>
        /// Whether the user is allowed to Hold and Un-Hold Jobs.
        /// </summary>
        Hold,

        /// <summary>
        /// Whether the user is allowed to Expedite Jobs.
        /// </summary>
        Expedite,

        /// <summary>
        /// Whether the user can create CTPs with Reservations.
        /// </summary>
        CTPReserve,

        /// <summary>
        /// Whether the user can create WhatIf CTPs.
        /// </summary>
        CTPWhatIf,

        /// <summary>
        /// Can enable and disable Add-Ins.
        /// </summary>
        MaintainAddins,

        /// <summary>
        /// Permits access to inventory, sales orders, purchase orders.
        /// </summary>
        MaintainInventory,
        MaintainScenario,
        CreateScenario,
        JobStatus,
        ViewPublished,
        ViewWhatIf,
        ViewLive,
        MySuggestions,
        ActivityReschedule,
        WebServices,
        MaintainChangeOrders,
        MaintainCoPilot,
        RunInsertJobs,
        RunRuleSeek,
        PublishScenario,
        AdvanceClock,
        ViewErrorLogs,
        RunReports,
        MaintainWorkspaces,
        ShareWorkspaces,
        MaintainUsers,
        MRP
    }
}

/// <summary>
/// </summary>
public enum resourceTypes
{
    /// <summary>
    /// </summary>
    Resource = 0,

    /// <summary>
    /// </summary>
    BatchProcessor,

    /// <summary>
    /// </summary>
    MultiPort,

    /// <summary>
    /// </summary>
    Vessel,

    /// <summary>
    /// </summary>
    Tool
}

/// <summary>
/// </summary>
public class OperationDefs
{
    /// <summary>
    /// Can be used to schedule successor Operations in the same Manufacturing Order to stay on the same Resource.
    /// Note that this option only applies to linear (non-network) routings.
    /// </summary>
    public enum successorProcessingEnumeration
    {
        /// <summary>
        /// Successor Operations can schedule on any Resource regardless of which Resource the predecessor scheduled on.
        /// </summary>
        NoPreference,

        /// <summary>
        /// Successor Operations should schedule on the same Resource that performed their predecessor Operation if it has the required Capabilities.
        /// </summary>
        KeepSuccessor,

        /// <summary>
        /// Successor Operations should immediately schedule on the same Resource that performed their predecessor Operation if it has the required Capabilities.
        /// No other Operations should be scheduled between the predecessor/successor operation.
        /// </summary>
        KeepSuccessorNoInterrupt
    }

    /// <summary>
    /// When operation transfer time can start and end between operations.
    /// </summary>
    public enum EOperationTransferPoint : short
    {
        StartOfOperation,
        EndOfSetup,
        EndOfRun,
        EndOfPostProcessing,
        EndOfStorage, //TODO: [Storage]
        EndOfOperation,
        NoTransfer
    }

    public enum EAutoSplitType : short
    {
        None,

        /// <summary>
        /// Split based on the resource's maximum volume
        /// </summary>
        ResourceVolumeCapacity,

        /// <summary>
        /// Split based on the resource's maximum qty capacity
        /// </summary>
        ResourceQtyCapacity,

        /// <summary>
        /// Split the activity based on the number of cycles that can run until the next offline interval. This will prevent the operation from spanning offline time
        /// </summary>
        PrimaryCapacityAvailability,

        /// <summary>
        /// This allows operations to schedule on resources they may not be eligible for unless they are split
        /// </summary>
        Manual,

        /// <summary>
        /// When sourcing material from the predecessor, split the operation into activities that each pull a full predecessor quantity amount
        /// </summary>
        PredecessorMaterials,

        /// <summary>
        /// If the predecessor was auto-split, split into the same number of activities each with an equivalent quantity ratio, for example 20% of the op required quantity.
        /// </summary>
        PredecessorQuantityRatio,

        //TODO:
        /// <summary>
        /// If the helper becomes offline before the primary, split the primary and run what is possible on the helper.
        /// </summary>
        //HelperShiftAvailability,

        /// <summary>
        /// Split the operation when a clean is required.
        /// For example if an operation quantity exceeds a resource quantity clean, or if a time based clean is needed halfway through the operation.
        /// </summary>
        CIP
    }

    /// <summary>
    /// How setup should be allocated when the operation is split
    /// </summary>
    public enum ESetupSplitType
    {
        /// <summary>
        /// No overrides, setup will be calculated according to normal rules
        /// </summary>
        None,

        /// <summary>
        /// The first activity will have standard setup, all other activities will have Zero setup.
        /// If manually split, the First activity will not necessarily be the one that schedules first. Take care to sequence accordingly.
        /// </summary>
        FirstActivity,

        /// <summary>
        /// The setup will be set proportional to the activities split ratio, or percentage of total required capacity.
        /// </summary>
        SplitByQty,
    }
}

public class ScenarioDefs { }

public class ItemDefs
{
    /// <summary>
    /// Where the Item originates from.  This is for information only and has no effect on scheduling.
    /// </summary>
    public enum sources
    {
        /// <summary>
        /// The Item is purchased from an outside supplier.
        /// </summary>
        Purchased,

        /// <summary>
        /// The Item is made in house.
        /// </summary>
        Manufactured,

        /// <summary>
        /// The Item can be purchased from a supplier or made in house.
        /// </summary>
        PurchasedOrManufactured
    }

    /// <summary>
    /// Indicates the primary use of the Item.  This is for information only and has no effect on scheduling.
    /// </summary>
    public enum itemTypes
    {
        /// <summary>
        /// The Item is used as a base material to make other items.
        /// </summary>
        RawMaterial,

        /// <summary>
        /// The Item has had value added and is a component used to make other items.
        /// </summary>
        SubAssembly,

        /// <summary>
        /// The Item has had value added and is used as an ingredient in onother item.
        /// </summary>
        Intermediate,

        /// <summary>
        /// Thie Item is eventually shipped to customers.
        /// </summary>
        FinishedGood,

        /// <summary>
        /// This item is temporarily used during the manufacturing process.  It can be consumed when used and supplied when relieved at a later operation.
        /// </summary>
        Resource,

        /// <summary>
        /// Item produced as part of the process but is of negligible or negative value
        /// </summary>
        ByProduct,

        /// <summary>
        /// Item produced as part of the process but should not be tracked as a primary product or finished good.
        /// </summary>
        CoProduct,

        /// <summary>
        /// Item that are consumed and produced as part of the MO. This can be used to model tools and molds that are in use during operation steps.
        /// </summary>
        Tool
    }

    /// <summary>
    /// The immediate source of a Lot.
    /// </summary>
    public enum ELotSource
    {
        /// <summary>
        /// Material that is not tracked and is available to all sources
        /// </summary>
        OnHand = 0,

        /// <summary>
        /// Existing material in inventory.
        /// </summary>
        Inventory = 1,

        /// <summary>
        /// Material that was taken from inventory and returned under a new Lot, such as reusable manufacturing material.
        /// </summary>
        //ReturnToInventory = 2,

        /// <summary>
        /// Produced material.
        /// </summary>
        Production = 3,

        /// <summary>
        /// Purchased material.
        /// </summary>
        PurchaseToStock = 4,

        /// <summary>
        /// Produced material from an in-process job. The lot quantity will continue to increase until the source operation is complete.
        /// </summary>
        PartialProduction = 5,
    }

    public enum MaterialAllocation
    {
        NotSet, //This defaults to UseOldestFirst
        UseOldestFirst,
        UseNewestFirst
    }

    public enum MaterialSourcing
    {
        NotSet,

        /// <summary>
        /// Whether a material requirement must draw from material sources that haven't already been drawn from by other material requirements.
        /// For instance if this value is specified and a produce from an activity has some but not all of its quantity consumed by another
        /// material requirement, then this material requirement can't use the leftover amount of the product.
        /// </summary>
        Exclusive
    }

    public static void ValidateBatchingProperties(decimal a_batchSize, decimal a_minOrderQty, decimal a_maxOrderQty)
    {
        if (a_batchSize < 0 || a_minOrderQty < 0 || a_maxOrderQty < 0)
        {
            throw new APSCommon.PTValidationException("2969", new object[] { a_batchSize, a_minOrderQty, a_maxOrderQty });
        }

        if (a_batchSize == 0 || a_minOrderQty == 0 || a_maxOrderQty == 0)
        {
            return; // no batching but still valid values
        }

        if (a_minOrderQty == a_maxOrderQty && a_batchSize != a_minOrderQty)
        {
            throw new APSCommon.PTValidationException("2970", new object[] { a_batchSize, a_minOrderQty, a_maxOrderQty });
        }

        if (a_minOrderQty > a_maxOrderQty)
        {
            throw new APSCommon.PTValidationException("2971", new object[] { a_minOrderQty, a_maxOrderQty });
        }

        if (a_batchSize > a_maxOrderQty)
        {
            throw new APSCommon.PTValidationException("2972", new object[] { a_batchSize, a_maxOrderQty });
        }

        // check if there's a number between minOrderQty and maxOrderQty (inclusive) that is a multiple of batchSize
        decimal smallestNbrOfBatches = Math.Ceiling(a_minOrderQty / a_batchSize);
        if (smallestNbrOfBatches * a_batchSize > a_maxOrderQty)
        {
            throw new APSCommon.PTValidationException("2973", new object[] { a_batchSize, a_minOrderQty, a_maxOrderQty });
        }
    }
}

public class PurchaseToStockDefs
{
    public enum EMaintenanceMethod
    {
        /// <summary>
        /// When PO is created internally by MRP or user. These types of POs will not be deleted when omitted from import.
        /// </summary>
        Manual,

        /// <summary>
        /// When PO is created by the import. These POs will be deleted if auto-delete omitted records is turned on.
        /// </summary>
        ERP,
        AutoGenerated,

        /// <summary>
        /// Generated by PlanetTogether MRP.
        /// </summary>
        MrpGenerated
    }
}

public class InventoryDefs
{
    /// <summary>
    /// Options for how Forecast quantities are reduced based on actual demand.
    /// </summary>
    public enum ForecastConsumptionMethods
    {
        /// <summary>
        /// Entire Forecast quantities are used.
        /// </summary>
        None,

        /// <summary>
        /// Search for Forecasts with the same or earlier period to consume.
        /// </summary>
        Backward,

        /// <summary>
        /// Search for Forecasts with the same or later period to consume.
        /// </summary>
        Forward,

        /// <summary>
        /// First search backwards for Forecasts to consume and then forwards if
        /// sales order is not totally consumed.
        /// </summary>
        BackwardThenForward,

        /// <summary>
        /// Find forecasts immediately before and after a sales order date. Calculate
        /// a ratio based on how far each forecast is from the sales order date. Consume
        /// based on this ratio. Repeat for any remaining qty.
        /// </summary>
        Spread
    }

    /// <summary>
    /// Specifies how the MRP process will affect the inventory.
    /// </summary>
    public enum MrpProcessing
    {
        /// <summary>
        /// Do nothing for inventory shortages.
        /// </summary>
        Ignore,

        /// <summary>
        /// Generate Jobs to satisfy inventory shortages.
        /// </summary>
        GenerateJobs,

        /// <summary>
        /// GeneratePurchaseOrders to satisfy inventory shortages.
        /// </summary>
        GeneratePurchaseOrders
    }

    /// <summary>
    /// MRP generated Jobs may produce a larger quantity than than due to batching. This specifies whether and how
    /// the excess material should be allocated.
    /// </summary>
    public enum MrpExcessQuantityAllocation
    {
        /// <summary>
        /// Leave extra material as WIP in warehouse or on Tank.
        /// </summary>
        None,

        /// <summary>
        /// Allocate the remainder to the last Job (by NeedDate).
        /// </summary>
        LastParentJob,

        /// <summary>
        /// Divide the excess quantity equally among the parent Jobs
        /// </summary>
        AllParentJobsEqually,

        /// <summary>
        /// Divide the excess quantity proportional to the parent Job quantities.
        /// </summary>
        AllParentJobsProportionally
    }

    /// <summary>
    /// The underlying reason for the adjustment
    /// </summary>
    public enum EAdjustmentType
    {
        ActivityProductionStored,
        MaterialRequirement,
        SafetyStock,
        Disposal,
        Expiration,
        SalesOrderDemand, //Shows the original demand for MRP or to track shortages
        ForecastDemand,
        Discarding,
        LeadTime,
        OnHand,
        Shortage,
        ActivityProductionAvailable,
        PurchaseOrderStored,
        PurchaseOrderAvailable,
        ActivityProductionStoredAndAvailable,
        PurchaseOrderStoredAndAvailable,
        PastPlanningHorizon,
        SalesOrderDistribution, //Consumption by a SO distribution
        TransferOrderDemand, //Shows the original demand for MRP or to track shortages
        TransferOrderOut,
        TransferOrderIn,
        StorageAreaCleanStart,
        StorageAreaCleanEnd,
    }


    /// <summary>
    /// Indicates how this lot quantity is managed.
    /// </summary>
    public enum ELotStorageMaintenanceMethod
    {
        /// <summary>
        /// Not managed, all updates will change the qty
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// Modified via the interface, usually from an ERP system. Future imports will update the lot.
        /// </summary>
        ERP,

        /// <summary>
        /// Modified by the auto-reporting feature after a Clock Advance or when an activity reports status
        /// </summary>
        AutoReported,

        /// <summary>
        /// Modified by a user from within PlanetTogether. Only manual changes can update the lot
        /// </summary>
        Manual
    }
}

public class SalesOrderDefs
{
    /// <summary>
    /// Specifies what should be done in stock planning when the shipments full QtyOpenToShip cannot be satisfied.
    /// This can also be overridden during Optimizes with a global rule.
    /// </summary>
    public enum StockShortageRules
    {
        /// <summary>
        /// Push the demand to a future date to be satisifed late.
        /// </summary>
        PushLater,

        /// <summary>
        /// Drive inventory negative and assume the remaining demand will be satisfied at a later period.
        /// !!!!!!!!!! NOTE: NOT SURE IF WE WANT TO PROVIDE THIS OPTION!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// </summary>
        Backlog,

        /// <summary>
        /// Flag the shipment as a missed opportunity and ignore the remaining demand.
        /// </summary>
        MissedSale
    }
}

public class PTAttributeDefs
{
    /// <summary>
    /// For setup times of attributes to be used, your resources SetupIncluded values must be set to use operations attributes.
    /// </summary>
    public enum EAttributeTriggerOptions
    {
        /// <summary>
        /// Always include the setup hours in the attribute.
        /// </summary>
        Always,

        /// <summary>
        /// Include the setup hours if this attributes Code differs from the attribute of the operation scheduled before it.
        /// </summary>
        CodeChanges,

        /// <summary>
        /// Include the setup hours if this attributes Number differs from the attribute of the operation scheduled before it.
        /// </summary>
        NumberChanges,

        /// <summary>
        /// Include the setup hours if this attributes Number is higher than the attribute of the operation scheduled before it.
        /// </summary>
        NumberHigher,

        /// <summary>
        /// Include the setup hours if this attributes Number is lower than the attribute of the operation scheduled before it.
        /// </summary>
        NumberLower,

        /// <summary>
        /// The attribute won't affect scheduling.
        /// </summary>
        Never,

        /// <summary>
        /// Instead of using the setup time of the attribute, the setup will be determined based on the Code and an Attribute Code Table (note your table must reference the resource).
        /// </summary>
        LookupByCode,

        /// <summary>
        /// Not in use.
        /// </summary>
        [Obsolete] 
        LookupByNumber,

        /// <summary>
        /// Instead of using the setup time of the attribute, the setup will be determined based on the Number and an Attribute Number Range (note your range must reference the resource).
        /// </summary>
        LookupByRange
    }

    /// <summary>
    /// Used during Optimizations by Attribute.
    /// </summary>
    public enum OptimizeType
    {
        /// <summary>
        /// Favor Attributes with the same Number as the previous Operation scheduled.
        /// </summary>
        SameAttributeNumber = 0,

        /// <summary>
        /// Favor Attributes with the same Code as the previous Operation scheduled.
        /// </summary>
        SameAttributeCode,

        /// <summary>
        /// Favor Attributes with the nearest Number as the previous Operation scheduled (in either direction).
        /// </summary>
        NearestAttributeNumber,

        /// <summary>
        /// Favor Attributes with the nearest Lower Number as the previous Operation scheduled.
        /// </summary>
        NearestLowerAttributeNumber,

        /// <summary>
        /// Favor Attributes with the nearest Higher Number as the previous Operation scheduled.
        /// </summary>
        NearestHigherAttributeNumber,

        /// <summary>
        /// Favor Attributes with the best Attribute Number using a sawtooth, up then down, pattern.
        /// </summary>
        SawtoothAttributeNumber,

        /// <summary>
        /// Favor Attributes with the lowest Attribute Number.
        /// </summary>
        LowestAttributeNumber,

        /// <summary>
        /// Favor Attributes with the highest Attribute Number.
        /// </summary>
        HighestAttributeNumber
    }

    public enum EIncurAttributeType { Setup, Clean }
}

public class ResourceRequirementDefs
{
    /// <summary>
    /// Specifies which type of fill to use when drawing blocks on the Gantt.
    /// </summary>
    public enum blockFillTypes
    {
        /// <summary>
        /// Use the default fill.
        /// </summary>
        Default,

        /// <summary>
        /// Draw an image in blocks, on the left edge, using the FillImage specified in the Resource Requirement (or the Resource Image if the Resource Requirement Fill Image is blank).
        /// </summary>
        ImageAlignedLeft,

        /// <summary>
        /// Draw an image in blocks, on the left edge, using the FillImage specified in the Resource Requirement (or the Resource Image if the Resource Requirement Fill Image is blank).
        /// </summary>
        ImageAlignedCenter,

        /// <summary>
        /// Draw an image in blocks, on the left edge, using the FillImage specified in the Resource Requirement (or the Resource Image if the Resource Requirement Fill Image is blank).
        /// </summary>
        ImageAlignedRight,

        /// <summary>
        /// Fill the blocks based on the FillPattern specified in the Resource Requirement.
        /// </summary>
        Pattern
    }
}

/// <summary>
/// 
/// </summary>
public class CleanoutDefs
{
    /// <summary>
    /// Indicates the type of production units to use for a cleanout
    /// </summary>
    public enum EProductionUnitsCleanType
    {
        /// <summary>
        /// Operation cycles
        /// </summary>
        Cycles,

        /// <summary>
        /// Cleanout based on the units of the primary product produced
        /// </summary>
        ProductUnits,

        /// <summary>
        /// Based on operation quantity
        /// </summary>
        Quantity,
    }
}

public class ConnectorDefs
{
    /// <summary>
    /// Specifies the type of connector.
    /// </summary>
    public enum EConnectorType
    {
        /// <summary>
        /// Constrain the successor operation to and from specified resources
        /// </summary>
        SuccessorOperation,

        /// <summary>
        /// Constrain material consumption to and from the specified resources
        /// </summary>
        Material
    }

    /// <summary>
    /// Specifies whether the directional connection of the ResourceConnector's Connections
    /// </summary>
    public enum EConnectionDirection
    {
        FromResource,
        ToResource,
    }
}