namespace PT.SchedulerDefinitions;

public class PublishStatuses
{
    public enum EPublishProgressStep : short
    {
        Started,
        Canceled,
        Error,
        PrePublishProcedure,
        PostPublishProcedure,
        Connecting,
        ClearingOldHistory,
        PreparingPlants,
        PreparingResources,
        PreparingAttributes,
        PreparingJobs,
        PreparingItems,
        PreparingInventory,
        PreparingCapabilities,
        PreparingDemands,
        PreparingProductRules,
        PreparingCapacity,
        PreparingKPIs,
        PreparingMetrics,
        FormattingData,
        FormattingTables,
        UploadingData,
        Complete,
        WritingData,
        CustomPublishProcedure,
        PreparingCustomers,
        PublishingToAnalytics,
        NeverPublished
    }
}