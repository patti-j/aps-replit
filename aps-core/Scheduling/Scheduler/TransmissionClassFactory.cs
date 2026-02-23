using PT.Common.Exceptions;
using PT.ERPTransmissions;
using PT.Transmissions;
using PT.Transmissions.CleanoutTrigger;
using PT.Transmissions.ResourceConnectors;
using PT.Transmissions.User;
using PT.Transmissions2;

namespace PT.Scheduler;

/// <summary>
/// ****************************************************************************************************************************************************************
/// Don't delete or remove unused transmissions that made it into builds that customers may have received. They are necessary to allow Recordings to be played back.
/// ****************************************************************************************************************************************************************
/// To determine which UNIQUE_IDs are available used the functions in the Limits region. This includes overrides of ToString,
/// and the properties FirstUnused and UnusedList.
/// The UNIQUE_IDs of the transmissions are completely separate from the UNIQUE_IDs of the other PlanetTogether objects.
/// Nov 29, 2016:
/// Last used id:
/// 822 for SendSupportEmailT.
/// ****************************************************************************************************************************************************************
/// </summary>
public class TransmissionClassFactory : IClassFactory
{
    public TransmissionClassFactory()
    {
        #region Transmissions
        AddCreationMethod(InternalActivityFinishT.UNIQUE_ID, new ObjectCreatorDelegate(CreateFinishActivityT));
        AddCreationMethod(InternalActivityUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateInternalActivityUpdateT));
        //AddCreationMethod(BalancedCompositeDispatcherDefinitionChangeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateBalancedCompositeDispatcherDefinitionChangeT));
        AddCreationMethod(BalancedCompositeDispatcherDefinitionCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateBalancedCompositeDispatcherDefinitionCopyT));
        AddCreationMethod(BalancedCompositeDispatcherDefinitionDefaultT.UNIQUE_ID, new ObjectCreatorDelegate(CreateBalancedCompositeDispatcherDefinitionDefaultT));
        AddCreationMethod(BalancedCompositeDispatcherDefinitionDeleteAllT.UNIQUE_ID, new ObjectCreatorDelegate(CreateBalancedCompositeDispatcherDefinitionDeleteAllT));
        AddCreationMethod(BalancedCompositeDispatcherDefinitionDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateBalancedCompositeDispatcherDefinitionDeleteT));
        AddCreationMethod(BalancedCompositeDispatcherDefinitionUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateBalancedCompositeDispatcherDefinitionUpdateT));
        AddCreationMethod(BalancedCompositeDispatcherDefinitionImportT.UNIQUE_ID, new ObjectCreatorDelegate(CreateBalancedCompositeDispatcherDefinitionImportT));
        //AddCreationMethod(CapabilityChangeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCapabilityChangeT));
        AddCreationMethod(CapabilityCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCapabilityCopyT));
        AddCreationMethod(CapabilityDefaultT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCapabilityDefaultT));
        AddCreationMethod(CapabilityDeleteAllT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCapabilityDeleteAllT));
        AddCreationMethod(CapabilityDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCapabilityDeleteT));
        AddCreationMethod(CapacityInterval.UNIQUE_ID, new ObjectCreatorDelegate(CreateCapacityInterval));
        //AddCreationMethod(CapacityIntervalChangeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCapacityIntervalChangeT));
        AddCreationMethod(CapacityIntervalConvertT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCapacityIntervalConvertT));
        AddCreationMethod(CapacityIntervalCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCapacityIntervalCopyT));
        AddCreationMethod(CapacityIntervalCopyToResourceT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCapacityIntervalCopyToResourceT));
        AddCreationMethod(CapacityIntervalDefaultT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCapacityIntervalDefaultT));
        AddCreationMethod(CapacityIntervalDeleteAllT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCapacityIntervalDeleteAllT));
        AddCreationMethod(CapacityIntervalDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCapacityIntervalDeleteT));
        AddCreationMethod(CapacityIntervalMoveInTimeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCapacityIntervalMoveInTimeT));
        AddCreationMethod(CapacityIntervalMoveT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCapacityIntervalMoveT));
        AddCreationMethod(CapacityIntervalNewT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCapacityIntervalNewT));
        AddCreationMethod(CapacityIntervalShareT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCapacityIntervalShareT));
        AddCreationMethod(CapacityIntervalUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCapacityIntervalUpdateT));
        AddCreationMethod(CapacityIntervalSetResourcesT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCapacityIntervalSetResourcesT));
        //AddCreationMethod(CellChangeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCellChangeT));
        AddCreationMethod(CellCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCellCopyT));
        AddCreationMethod(CellDefaultT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCellDefaultT));
        AddCreationMethod(CellDeleteAllT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCellDeleteAllT));
        AddCreationMethod(CellDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCellDeleteT));
        AddCreationMethod(CustomerCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCustomerCopyT));
        AddCreationMethod(CustomerDefaultT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCustomerDefaultT));
        AddCreationMethod(CustomerT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCustomerT));
        AddCreationMethod(CustomerDeleteAllT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCustomerDeleteAllT));
        AddCreationMethod(CustomerDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCustomerDeleteT));
        //AddCreationMethod(DepartmentChangeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateDepartmentChangeT));
        AddCreationMethod(DepartmentCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateDepartmentCopyT));
        AddCreationMethod(DepartmentDefaultT.UNIQUE_ID, new ObjectCreatorDelegate(CreateDepartmentDefaultT));
        AddCreationMethod(DepartmentDeleteAllT.UNIQUE_ID, new ObjectCreatorDelegate(CreateDepartmentDeleteAllT));
        AddCreationMethod(DepartmentDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateDepartmentDeleteT));
        AddCreationMethod(SetDefaultDispatcherDefinitionOfDefinitionManagerT.UNIQUE_ID, new ObjectCreatorDelegate(CreateDispatcherDefinitionManagerDefaultT));
        //AddCreationMethod(JobChangeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateJobChangeT));
        AddCreationMethod(JobRequestNewExternalIdT.UNIQUE_ID, new ObjectCreatorDelegate(CreateJobRequestNewExternalIdT));
        AddCreationMethod(JobCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateJobCopyT));
        AddCreationMethod(JobDefaultT.UNIQUE_ID, new ObjectCreatorDelegate(CreateJobDefaultT));
        AddCreationMethod(JobDeleteAllT.UNIQUE_ID, new ObjectCreatorDelegate(CreateJobDeleteAllT));
        AddCreationMethod(JobDeleteJobsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateJobDeleteJobsT));
        AddCreationMethod(JobGenerateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateJobGenerateT));
        AddCreationMethod(PurchaseToStockMoveT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePurchaseToStockMoveT));
        AddCreationMethod(PurchaseToStockRevertT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePurchaseToStockRevertT));
        //AddCreationMethod(ChangeOrderNewT.UNIQUE_ID, new ObjectCreatorDelegate(CreateChangeOrderNewT));
        //AddCreationMethod(ChangeOrderUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateChangeOrderUpdateT));
        //AddCreationMethod(ChangeOrderDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateChangeOrderDeleteT));
        //AddCreationMethod(ChangeOrdersApplyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateChangeOrdersApplyT));
        //AddCreationMethod(ChangeOrdersDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateChangeOrdersDeleteT));
        //AddCreationMethod(ChangeOrdersAcceptT.UNIQUE_ID, new ObjectCreatorDelegate(CreateChangeOrdersAcceptT));
        //AddCreationMethod(ChangeOrdersRejectT.UNIQUE_ID, new ObjectCreatorDelegate(CreateChangeOrdersRejectT));
        AddCreationMethod(SplitOperationT.UNIQUE_ID, new ObjectCreatorDelegate(CreateSplitOperationT));
        AddCreationMethod(UnSplitOperationT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUnSplitOperationT));
        //AddCreationMethod(ManufacturingOrderChangeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateManufacturingOrderChangeT));
        AddCreationMethod(ManufacturingOrderCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateManufacturingOrderCopyT));
        AddCreationMethod(ManufacturingOrderDefaultT.UNIQUE_ID, new ObjectCreatorDelegate(CreateManufacturingOrderDefaultT));
        AddCreationMethod(ManufacturingOrderDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateManufacturingOrderDeleteT));
        AddCreationMethod(OperationDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateOperationDeleteT));
        AddCreationMethod(PlantAddResourceT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePlantAddResourceT));
        AddCreationMethod(PlantSetResourceDeptT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePlantSetResourceDeptT));
        //AddCreationMethod(PlantChangeT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePlantChangeT));
        AddCreationMethod(PlantCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePlantCopyT));
        AddCreationMethod(PlantDefaultT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePlantDefaultT));
        AddCreationMethod(PlantDeleteAllT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePlantDeleteAllT));
        AddCreationMethod(PlantDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePlantDeleteT));

        AddCreationMethod(PTAttributeCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePTAttributeCopyT));
        AddCreationMethod(PTAttributeDefaultT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePTAttributeDefaultT));
        AddCreationMethod(PTAttributeDeleteAllT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePTAttributeDeleteAllT));
        AddCreationMethod(PTAttributeDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePTAttributeDeleteT));

        AddCreationMethod(UserFieldDefinitionCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserFieldDefinitionCopyT));
        AddCreationMethod(UserFieldDefinitionDefaultT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserFieldDefinitionDefaultT));
        AddCreationMethod(UserFieldDefinitionDeleteAllT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserFieldDefinitionDeleteAllT));
        AddCreationMethod(UserFieldDefinitionDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserFieldDefinitionDeleteT));

        AddCreationMethod(PTObjectBase.UNIQUE_ID, new ObjectCreatorDelegate(CreatePTObjectBase));
        AddCreationMethod(PT.Transmissions.RecurringCapacityInterval.UNIQUE_ID, new ObjectCreatorDelegate(CreateRecurringCapacityInterval));
        //AddCreationMethod(RecurringCapacityIntervalChangeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateRecurringCapacityIntervalChangeT));
        AddCreationMethod(RecurringCapacityIntervalConvertT.UNIQUE_ID, new ObjectCreatorDelegate(CreateRecurringCapacityIntervalConvertT));
        AddCreationMethod(RecurringCapacityIntervalCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateRecurringCapacityIntervalCopyT));
        AddCreationMethod(RecurringCapacityIntervalCopyToResourceT.UNIQUE_ID, new ObjectCreatorDelegate(CreateRecurringCapacityIntervalCopyToResourceT));
        AddCreationMethod(RecurringCapacityIntervalDefaultT.UNIQUE_ID, new ObjectCreatorDelegate(CreateRecurringCapacityIntervalDefaultT));
        AddCreationMethod(RecurringCapacityIntervalDeleteAllT.UNIQUE_ID, new ObjectCreatorDelegate(CreateRecurringCapacityIntervalDeleteAllT));
        AddCreationMethod(RecurringCapacityIntervalDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateRecurringCapacityIntervalDeleteT));
        AddCreationMethod(RecurringCapacityIntervalMoveInTimeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateRecurringCapacityIntervalMoveInTimeT));
        AddCreationMethod(RecurringCapacityIntervalMoveT.UNIQUE_ID, new ObjectCreatorDelegate(CreateRecurringCapacityIntervalMoveT));
        AddCreationMethod(RecurringCapacityIntervalShareT.UNIQUE_ID, new ObjectCreatorDelegate(CreateRecurringCapacityIntervalShareT));
        AddCreationMethod(RecurringCapacityIntervalUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateRecurringCapacityIntervalUpdateT));
        AddCreationMethod(RecurringCapacityIntervalUpdateMultiT.UNIQUE_ID, new ObjectCreatorDelegate(CreateRecurringCapacityIntervalUpdateMultiT));
        AddCreationMethod(RecurringCapacityIntervalSetResourcesT.UNIQUE_ID, new ObjectCreatorDelegate(CreateRecurringCapacityIntervalSetResourcesT));
        AddCreationMethod(RequiredAttribute.UNIQUE_ID, new ObjectCreatorDelegate(CreateRequiredAttribute));
        //AddCreationMethod(ResourceChangeRefT.UNIQUE_ID, new ObjectCreatorDelegate(CreateResourceChangeRefT));
        //AddCreationMethod(ResourceChangeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateResourceChangeT));
        AddCreationMethod(ResourceCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateResourceCopyT));
        AddCreationMethod(ResourceDefaultT.UNIQUE_ID, new ObjectCreatorDelegate(CreateResourceDefaultT));
        AddCreationMethod(ResourceDeleteAllT.UNIQUE_ID, new ObjectCreatorDelegate(CreateResourceDeleteAllT));
        AddCreationMethod(ResourceDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateResourceDeleteT));
        AddCreationMethod(ResourceDeleteMultiT.UNIQUE_ID, new ObjectCreatorDelegate(CreateResourceDeleteMultiT));
        AddCreationMethod(ResourceSetShopViewersT.UNIQUE_ID, new ObjectCreatorDelegate(CreateResourceSetShopViewersT));
        AddCreationMethod(ScenarioChangeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioChangeT));
        AddCreationMethod(ScenarioClockAdvanceT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioClockAdvanceT));
        AddCreationMethod(ScenarioDetailOfflineT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailOfflineT));
        AddCreationMethod(ScenarioDetailOnlineT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailOnlineT));
        AddCreationMethod(ScenarioCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioCopyT));
        AddCreationMethod(ScenarioNewT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioNewT));
        AddCreationMethod(ScenarioDetailClearResourcePerformancesT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailClearResourcePerformancesT));
        AddCreationMethod(ScenarioDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDeleteT));
        AddCreationMethod(ScenarioDetailCompressT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailCompressT));
        AddCreationMethod(ScenarioDetailHoldSettingsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailHoldSettingsT));
        AddCreationMethod(ScenarioDetailConfirmOperationConstraintsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailConfirmOperationConstraintsT));
        AddCreationMethod(ScenarioDetailExpediteJobsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailExpediteJobsT));
        AddCreationMethod(ScenarioDetailExpediteMOsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailExpediteMOsT));
        AddCreationMethod(ScenarioDetailAnchorActivitiesT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailAnchorActivitiesT));
        AddCreationMethod(ScenarioDetailAnchorJobsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailAnchorJobsT));
        AddCreationMethod(ScenarioDetailJobResetJITAndSubJobNeedDateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailJobResetJITAndSubJobNeedDateT));
        AddCreationMethod(ScenarioDetailAnchorMOsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailAnchorMOsT));
        AddCreationMethod(ScenarioDetailAnchorOperationsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailAnchorOperationsT));
        AddCreationMethod(ScenarioDetailHoldJobsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailHoldJobsT));
        AddCreationMethod(ScenarioDetailHoldMOsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailHoldMOsT));
        AddCreationMethod(ScenarioDetailHoldOperationsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailHoldOperationsT));
        AddCreationMethod(ScenarioDetailLockActivitiesT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailLockActivitiesT));
        AddCreationMethod(ScenarioDetailLockBlocksT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailLockBlocksT));
        AddCreationMethod(ScenarioDetailLockJobsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailLockJobsT));
        AddCreationMethod(ScenarioDetailLockMOsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailLockMOsT));
        AddCreationMethod(ScenarioDetailLockMOsToPathT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailLockMOsToPathT));
        AddCreationMethod(ScenarioDetailLockOperationsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailLockOperationsT));
        AddCreationMethod(ScenarioDetailSetJobPropertiesT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailSetJobCommitmentsAndPrioritiesT));
        AddCreationMethod(ScenarioDetailMoveT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailMoveT));
        AddCreationMethod(ScenarioDetailOptimizeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailOptimizeT));
        AddCreationMethod(ScenarioDetailRescheduleJobsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailRescheduleJobsT));
        AddCreationMethod(ScenarioDetailRescheduleMOsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailRescheduleMOsT));
        AddCreationMethod(ScenarioDetailScheduleJobsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailScheduleJobsT));
        AddCreationMethod(ScenarioDetailSetCapabilitiesT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailSetCapabilitiesT));
        AddCreationMethod(ScenarioDetailSetCapabilityResourcesT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailSetCapabilityResourcesT));
        AddCreationMethod(ScenarioDetailClearT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailClearT));
        AddCreationMethod(ScenarioDetailCustomerT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailCustomerT));
        AddCreationMethod(ScenarioClearUndoSetsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioClearUndoSetsT));
        //AddCreationMethod(ScenarioSendAlertsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioSendAlertsT));
        AddCreationMethod(ScenarioChangeTypeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioChangeTypeT));
        AddCreationMethod(ScenarioPublishT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioPublishT));
        AddCreationMethod(ScenarioUndoCheckpointT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioUndoCheckpointT));
        AddCreationMethod(ScenarioUndoT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioUndoT));
        AddCreationMethod(ScenarioStartUndoT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioStartUndoT));
        AddCreationMethod(ScenarioLoadT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioLoadT));
        AddCreationMethod(ScenarioUnloadT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioUnloadT));
        AddCreationMethod(ScenarioReloadT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioReloadT));
        //AddCreationMethod(SubcontractorChangeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateSubcontractorChangeT));
        //AddCreationMethod(SubcontractorCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateSubcontractorCopyT));
        //AddCreationMethod(SubcontractorDefaultT.UNIQUE_ID, new ObjectCreatorDelegate(CreateSubcontractorDefaultT));
        //AddCreationMethod(SubcontractorDeleteAllT.UNIQUE_ID, new ObjectCreatorDelegate(CreateSubcontractorDeleteAllT));
        //AddCreationMethod(SubcontractorDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateSubcontractorDeleteT));
        AddCreationMethod(SystemMessageDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateSystemMessageDeleteT));
        AddCreationMethod(SystemOptionsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateSystemOptionsT));
        AddCreationMethod(SystemPublishOptionsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateSystemPublishOptionsT));
        AddCreationMethod(ScenarioKpiSnapshotT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioKpiSnapshotT));
        AddCreationMethod(KpiSnapshotOfLiveScenarioT.UNIQUE_ID, new ObjectCreatorDelegate(CreateKpiSnapshotOfLiveScenarioT));
        AddCreationMethod(ScenarioKpiOptionsUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioKpiOptionsUpdateT));
        AddCreationMethod(ScenarioKpiVisibilityT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioKpiVisibilityT));
        //AddCreationMethod(UserChangeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserChangeT));
        AddCreationMethod(UserChatT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserChatT));
        AddCreationMethod(UserSettingsChangeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserSettingsChangeT));
        AddCreationMethod(UserSettingsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserSettingsT));
        AddCreationMethod(UserCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserCopyT));
        AddCreationMethod(UserDefaultT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserDefaultT));
        AddCreationMethod(UserDeleteAllT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserDeleteAllT));
        AddCreationMethod(UserDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserDeleteT));
        AddCreationMethod(UserLogOffT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserLogOffT));
        // UserLogOffT is no longer used, but it's kept here so that we can playback recordings
        // from older instances that still use UserLogOffT
        AddCreationMethod(UserLogOnT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserLogOnT));
        AddCreationMethod(UserScheduleViewerSettingsChangeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserScheduleViewerSettingsChangeT));
        //AddCreationMethod(VesselTypeChangeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateVesselTypeChangeT));
        AddCreationMethod(VesselTypeCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateVesselTypeCopyT));
        AddCreationMethod(VesselTypeDefaultT.UNIQUE_ID, new ObjectCreatorDelegate(CreateVesselTypeDefaultT));
        AddCreationMethod(VesselTypeDeleteAllT.UNIQUE_ID, new ObjectCreatorDelegate(CreateVesselTypeDeleteAllT));
        AddCreationMethod(VesselTypeDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateVesselTypeDeleteT));
        AddCreationMethod(TriggerRecordingPlaybackT.UNIQUE_ID, new ObjectCreatorDelegate(CreateTriggerRecordingPlaybackT));
        AddCreationMethod(ScenarioManagerUndoSettingsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioManagerUndoSettingsT));
        AddCreationMethod(SystemStartupOptionsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateSystemStartupOptionsT));
        //AddCreationMethod(AlertOptionsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateAlertOptionsT)); Removed in V12, hasn't been used in a long time
        AddCreationMethod(UserErrorT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserErrorT));
        AddCreationMethod(ScenarioDetailJobT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailJobT));
        AddCreationMethod(ScenarioDetailItemT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailItemT));
        AddCreationMethod(ScenarioDetailWarehouseT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailWarehouseT));
        AddCreationMethod(ScenarioDetailSalesOrderT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailSalesOrderT));
        AddCreationMethod(ScenarioDetailPTAttributeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailPTAttributeT));
        AddCreationMethod(ScenarioDetailUserFieldDefinitionT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailUserFieldDefinitionT));
        AddCreationMethod(ScenarioDetailForecastT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailForecastT));
        AddCreationMethod(Transmissions.Forecast.ForecastIntervalQtyChangeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateForecastIntervalQtyChangeT));
        AddCreationMethod(ScenarioDetailTransferOrderT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailTransferOrderT));
        AddCreationMethod(ScenarioDetailPurchaseOrderT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailPurchaseOrderT));
        AddCreationMethod(ShopViewResourceOptionNewT.UNIQUE_ID, new ObjectCreatorDelegate(CreateShopViewResourceOptionNewT));
        AddCreationMethod(ShopViewResourceOptionCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateShopViewResourceOptionCopyT));
        AddCreationMethod(ShopViewResourceOptionUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateShopViewResourceOptionUpdateT));
        AddCreationMethod(ShopViewResourceOptionDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateShopViewResourceOptionDeleteT));
        AddCreationMethod(ShopViewSystemOptionsUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateShopViewSystemOptionsUpdateT));
        AddCreationMethod(ShopViewOptionsAssignmentT.UNIQUE_ID, new ObjectCreatorDelegate(CreateShopViewOptionsAssignmentT));
        AddCreationMethod(ScenarioDetailOptimizeSettingsChangeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailOptimizeSettingsChangeT));
        AddCreationMethod(ScenarioDetailCompressSettingsChangeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailCompressSettingsChangeT));
        AddCreationMethod(ScenarioChecksumT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioChecksumT));
        AddCreationMethod(LookupTableDeleteAllT.UNIQUE_ID, new ObjectCreatorDelegate(CreateLookupTableDeleteAllT));
        AddCreationMethod(AttributeCodeTableDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateAttributeCodeTableDeleteT));
        AddCreationMethod(AttributeCodeTableCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateAttributeCodeTableCopyT));
        AddCreationMethod(AttributeCodeTableNewT.UNIQUE_ID, new ObjectCreatorDelegate(CreateAttributeCodeTableNewT));
        AddCreationMethod(AttributeCodeTableUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateAttributeCodeTableUpdateT));
        AddCreationMethod(ScenarioTouchT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioTouchT));
        AddCreationMethod(ScenarioDetailProductRulesT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailProductRulesT));
        AddCreationMethod(SetupRangeDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateSetupRangeDeleteT));
        AddCreationMethod(SetupRangeUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateSetupRangeUpdateT));
        AddCreationMethod(SetupRangeCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateSetupRangeCopyT));
        AddCreationMethod(ScenarioDetailAlternatePathMoveT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailAlternatePathMoveT));
        AddCreationMethod(Transmissions.CTP.CtpT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCtpT));
        AddCreationMethod(Transmissions.CTP.ScenarioCtpT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioCtpT));
        AddCreationMethod(ScenarioDetailSplitMOT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailSplitMOT));
        AddCreationMethod(ScenarioDetailSplitJobOrMOT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailSplitMOAtTimeT));
        AddCreationMethod(ScenarioDetailJoinJobOrMOT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailUnsplitMOT));
        AddCreationMethod(ScenarioDetailChangeMOQtyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailChangeQtyByCycleT));
        AddCreationMethod(ScenarioDetailLockAndAnchorActivitiesT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailLockAndAnchorActivitiesT));
        AddCreationMethod(JobsPrintedT.UNIQUE_ID, new ObjectCreatorDelegate(CreateJobsPrintedT));
        AddCreationMethod(AllowedHelperResourcesT.UNIQUE_ID, new ObjectCreatorDelegate(CreateAllowedHelperResourcesT));
        AddCreationMethod(ScheduleClearCommitT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScheduleClearCommitT));
        AddCreationMethod(ScheduleCommitT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScheduleCommitT));
        AddCreationMethod(ManufacturingOrderBatchDefinitionSetT.UNIQUE_ID, new ObjectCreatorDelegate(CreateMOBatchDefinitionSetT));
        AddCreationMethod(ScenarioDetailJitCompressT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailNeedCompressT));
        AddCreationMethod(ScenarioDetailPlantT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailPlantT));
        AddCreationMethod(ScenarioDetailDepartmentT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailDepartmentT));
        AddCreationMethod(ScenarioDetailResourceT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailResourceT));
        AddCreationMethod(ScenarioDetailCapabilityT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailCapabilityT));
        AddCreationMethod(ScenarioDetailCellT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailCellT));
        AddCreationMethod(InventoryTransferRulesT.UNIQUE_ID, new ObjectCreatorDelegate(CreateInventoryTransferRuleT));
        AddCreationMethod(PruneScenarioT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePruneScenarioT));
        AddCreationMethod(ScenarioAddNewPrunedT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioAddNewPrunedT));
        AddCreationMethod(ScenarioIsolateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioIsolateT));
        AddCreationMethod(PlantPermissionSetT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePlantPermissionSetT));
        AddCreationMethod(UserPermissionSetT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserPermissionSetT));
        AddCreationMethod(ScenarioDetailSetPurchaseToStockValuesT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailSetPurchaseToStockValuesT));
        AddCreationMethod(ScenarioDetailSetSalesOrderValuesT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailSetSalesOrderValuesT));
        AddCreationMethod(ScenarioDetailMaterialUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailMaterialUpdateT));
        // !ALTERNATE_PATH!; ScenarioDetailAlternatePathLockT add creation method
        AddCreationMethod(ScenarioDetailAlternatePathLockT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailAlternatePathLock));
        AddCreationMethod(AddInControlUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateAddInControlUpdateT));
        AddCreationMethod(ScenarioDetailLotAllocationRuleT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailLotAllocationRuleT));
        AddCreationMethod(SystemStateSwitchT.UNIQUE_ID, new ObjectCreatorDelegate(CreateSwitchSystemStateT));
        AddCreationMethod(AutoUpdateKeyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateAutoUpdateKeyT));
        AddCreationMethod(ScenarioDetailClearPastShortTermT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioDetailClearPastShortTermT));
        AddCreationMethod(UserAdminLogOffT.UNIQUE_ID, new ObjectCreatorDelegate(CreateAdminLogOffT));

        //CoPilot
        AddCreationMethod(CoPilotSettingsChangeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateRuleSeekSettingsChangeT));
        AddCreationMethod(ScenarioRuleSeekStartT.UNIQUE_ID, new ObjectCreatorDelegate(CreateRuleSeekStartT));
        AddCreationMethod(RuleSeekCompletionT.UNIQUE_ID, new ObjectCreatorDelegate(CreateRuleSeekCompletionT));
        AddCreationMethod(CoPilotStatusUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateRuleSeekStatusUpdateT));
        AddCreationMethod(ScenarioCopyForRuleSeekT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioCopyWithNewOptimizeRulesT));
        AddCreationMethod(InsertJobsStartT.UNIQUE_ID, new ObjectCreatorDelegate(CreateInsertJobsStartT));
        AddCreationMethod(InsertJobsUserEndT.UNIQUE_ID, new ObjectCreatorDelegate(CreateInsertJobsUserEndT));
        AddCreationMethod(ScenarioCopyForInsertJobsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioCopyForInsertJobsT));
        AddCreationMethod(CoPilotDiagnositcsUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCopilotDiagnosticsUpdateT));

        //Game
        AddCreationMethod(Transmissions.Game.NewGameT.UNIQUE_ID, new ObjectCreatorDelegate(CreateNewGameT));

        //APIs
        AddCreationMethod(ApiHoldT.UNIQUE_ID, new ObjectCreatorDelegate(CreateApiHoldT));
        AddCreationMethod(ApiLockT.UNIQUE_ID, new ObjectCreatorDelegate(CreateApiLockT));
        AddCreationMethod(ApiAnchorT.UNIQUE_ID, new ObjectCreatorDelegate(CreateApiAnchorT));
        AddCreationMethod(ApiUnscheduleT.UNIQUE_ID, new ObjectCreatorDelegate(CreateApiUnscheduleT));
        AddCreationMethod(ApiActivityUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateApiActivityUpdateT));

        //Server Only Simulations
        AddCreationMethod(ScenarioAddNewT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioAddNewT));
        AddCreationMethod(ScenarioReplaceT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioReplaceT));
        AddCreationMethod(SystemMessageT.UNIQUE_ID, new ObjectCreatorDelegate(CreateSystemMessageT));

        //Workspaces
        AddCreationMethod(WorkspaceSharedDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateWorkspaceSharedDeleteT));
        AddCreationMethod(WorkspaceTemplateUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateWorkspaceTemplateSharedUpdateT));
        AddCreationMethod(WorkspaceSharedUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateWorkspaceSharedUpdateT));

        // Forecast Maintenance
        AddCreationMethod(Transmissions.Forecast.ForecastShipmentDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateForecastDeleteT));
        AddCreationMethod(Transmissions.Forecast.ForecastShipmentGenerateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateForecastShipmentGenerateT));
        AddCreationMethod(Transmissions.Forecast.MultiForecastShipmentGenerateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateMultiForecastShipmentGenerateT));
        AddCreationMethod(Transmissions.Forecast.ForecastShipmentAdjustmentT.UNIQUE_ID, new ObjectCreatorDelegate(CreateForecastShipmentAdjustmentT));

        AddCreationMethod(LicenseKeyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateLicenseKeyT));
        AddCreationMethod(SendSupportEmailT.UNIQUE_ID, new ObjectCreatorDelegate(CreateSendSupportEmailT));
        AddCreationMethod(ScenarioSettingDataT.UNIQUE_ID, new ObjectCreatorDelegate(CreateScenarioSettingDataT));
        AddCreationMethod(SystemSettingDataT.UNIQUE_ID, new ObjectCreatorDelegate(CreateSystemSettingDataT));
        AddCreationMethod(InstanceMessageT.UNIQUE_ID, new ObjectCreatorDelegate(CreateInstanceMessageT));
        AddCreationMethod(ClientUserRestartT.UNIQUE_ID, new ObjectCreatorDelegate(CreateClientUserRestartT));
        AddCreationMethod(UserLogonAttemptsProcessingT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserLogonProcessingT));
        #endregion

        #region ERP Transmissions
        AddCreationMethod(TriggerImportT.UNIQUE_ID, new ObjectCreatorDelegate(CreateTriggerImportT));
        AddCreationMethod(ImportT.UNIQUE_ID, new ObjectCreatorDelegate(CreateImportT));
        AddCreationMethod(ApplicationExceptionList.UNIQUE_ID, new ObjectCreatorDelegate(CreateApplicationExceptionList));
        AddCreationMethod(JobT.UNIQUE_ID, new ObjectCreatorDelegate(CreateJobT));
        AddCreationMethod(CapabilityT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCapabilityT));
        AddCreationMethod(CapacityIntervalT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCapacityIntervalT));
        AddCreationMethod(RecurringCapacityIntervalT.UNIQUE_ID, new ObjectCreatorDelegate(CreateRecurringCapacityIntervalT));
        AddCreationMethod(CellT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCellT));
        AddCreationMethod(DepartmentT.UNIQUE_ID, new ObjectCreatorDelegate(CreateDepartmentT));
        AddCreationMethod(UserT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserT));
        AddCreationMethod(PlantT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePlantT));
        AddCreationMethod(ItemT.UNIQUE_ID, new ObjectCreatorDelegate(CreateItemT));
        AddCreationMethod(WarehouseT.UNIQUE_ID, new ObjectCreatorDelegate(CreateWarehouseT));
        AddCreationMethod(ResourceT.UNIQUE_ID, new ObjectCreatorDelegate(CreateResourceT));
        AddCreationMethod(PurchaseToStockT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePurchaseToStockT));
        AddCreationMethod(VesselTypeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateVesselTypeT));
        AddCreationMethod(ScenarioDetailExportT.UNIQUE_ID, new ObjectCreatorDelegate(CreateExportScenarioT));
        AddCreationMethod(PerformImportStartedT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePerformImportStartedT));
        AddCreationMethod(PerformImportCompletedT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePerformImportCompletedT));
        AddCreationMethod(PerformExportStartedT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePerformExportStartedT));
        AddCreationMethod(PerformExportCompletedT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePerformExportCompletedT));
        AddCreationMethod(ProductRulesT.UNIQUE_ID, new ObjectCreatorDelegate(CreateProductRulesT));
        AddCreationMethod(LookupAttributeNumberRangeT.UNIQUE_ID, new ObjectCreatorDelegate(CreateLookupAttributeNumberRangeT));
        AddCreationMethod(LookupAttributeCodeTableT.UNIQUE_ID, new ObjectCreatorDelegate(CreateLookupAttributeCodeTableT));
        AddCreationMethod(SalesOrderT.UNIQUE_ID, new ObjectCreatorDelegate(CreateSalesOrderT));
        AddCreationMethod(ForecastT.UNIQUE_ID, new ObjectCreatorDelegate(CreateForecastT));
        AddCreationMethod(TransferOrderT.UNIQUE_ID, new ObjectCreatorDelegate(CreateTransferOrderT));
        AddCreationMethod(LotAllocationRuleT.UNIQUE_ID, new ObjectCreatorDelegate(CreateLotAllocationT));
        AddCreationMethod(ActivityUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateActivityUpdateT));
        AddCreationMethod(DataActivationWarningT.UNIQUE_ID, new ObjectCreatorDelegate(CreateDataActivationWarningT));
        AddCreationMethod(PTAttributeT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePTAttributeT));
        AddCreationMethod(RefreshStagingDataStartedT.UNIQUE_ID, new ObjectCreatorDelegate(CreateRefreshStagingDataStartedT));
        AddCreationMethod(RefreshStagingDataCompletedT.UNIQUE_ID, new ObjectCreatorDelegate(CreateRefreshStagingDataCompletedT));
        #endregion

        AddCreationMethod(PacketT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePacketT));
        AddCreationMethod(PurchaseToStockEditT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePurchaseToStockEditT));
        AddCreationMethod(SalesOrderEditT.UNIQUE_ID, new ObjectCreatorDelegate(CreateSalesOrderEditT));
        AddCreationMethod(ResourceEditT.UNIQUE_ID, new ObjectCreatorDelegate(CreateResourceEditT));
        AddCreationMethod(UserEditT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserEditT));
        AddCreationMethod(CustomerEditT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCustomerEditT));
        AddCreationMethod(MaterialEditT.UNIQUE_ID, new ObjectCreatorDelegate(CreateMaterialEditT));
        AddCreationMethod(JobEditT.UNIQUE_ID, new ObjectCreatorDelegate(CreateJobEditT));
        AddCreationMethod(InventoryEditT.UNIQUE_ID, new ObjectCreatorDelegate(CreateInventoryEditT));
        AddCreationMethod(StorageAreaEditT.UNIQUE_ID, new ObjectCreatorDelegate(CreateStorageAreaEditT));
        AddCreationMethod(PTAttributeEditT.UNIQUE_ID, new ObjectCreatorDelegate(CreatePTAttributeEditT));

        AddCreationMethod(TimeCleanoutTriggerTableNewT.UNIQUE_ID, new ObjectCreatorDelegate(CreateTimeCleanOutTriggerTableNewT));
        AddCreationMethod(TimeCleanoutTriggerTableCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateTimeCleanOutTriggerTableCopyT));
        AddCreationMethod(TimeCleanoutTriggerTableDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateTimeCleanOutTriggerTableDeleteT));
        AddCreationMethod(TimeCleanoutTriggerTableUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateTimeCleanOutTriggerTableUpdateT));

        AddCreationMethod(OperationCountCleanoutTriggerTableNewT.UNIQUE_ID, new ObjectCreatorDelegate(CreateOperationCountCleanOutTriggerTableNewT));
        AddCreationMethod(OperationCountCleanoutTriggerTableCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateOperationCountCleanOutTriggerTableCopyT));
        AddCreationMethod(OperationCountCleanoutTriggerTableDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateOperationCountCleanOutTriggerTableDeleteT));
        AddCreationMethod(OperationCountCleanoutTriggerTableUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateOperationCountCleanOutTriggerTableUpdateT));

        AddCreationMethod(ProductionUnitsCleanoutTriggerTableNewT.UNIQUE_ID, new ObjectCreatorDelegate(CreateProductionUnitsCleanOutTriggerTableNewT));
        AddCreationMethod(ProductionUnitsCleanoutTriggerTableCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateProductionUnitsCleanOutTriggerTableCopyT));
        AddCreationMethod(ProductionUnitsCleanoutTriggerTableDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateProductionUnitsCleanOutTriggerTableDeleteT));
        AddCreationMethod(ProductionUnitsCleanoutTriggerTableUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateProductionUnitsCleanOutTriggerTableUpdateT));
        AddCreationMethod(CleanoutTriggerTablesT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCleanoutTriggerTablesT));
        AddCreationMethod(ResourceConnectorsT.UNIQUE_ID, new ObjectCreatorDelegate(CreateResourceConnectorsT));

        AddCreationMethod(ResourceConnectorsDefaultT.UNIQUE_ID, new ObjectCreatorDelegate(CreateResourceConnectorsNewT));
        AddCreationMethod(ResourceConnectorsCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateResourceConnectorsCopyT));
        AddCreationMethod(ResourceConnectorsDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateResourceConnectorsDeleteT));
        AddCreationMethod(ResourceConnectorsDeleteAllT.UNIQUE_ID, new ObjectCreatorDelegate(CreateResourceConnectorsDeleteAllT));
        
        AddCreationMethod(CompatibilityCodeTableT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCompatibilityCodeTableT));

        AddCreationMethod(CompatibilityCodeTableBaseT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCompatibilityCodeTableBaseT));
        AddCreationMethod(CompatibilityCodeTableCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCompatibilityCodeTableCopyT));
        AddCreationMethod(CompatibilityCodeTableDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCompatibilityCodeTableDeleteT));
        AddCreationMethod(CompatibilityCodeTableNewT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCompatibilityCodeTableNewT));
        AddCreationMethod(CompatibilityCodeTableUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateCompatibilityCodeTableUpdateT));

        AddCreationMethod(Transmissions2.ConvertToProductionScenarioT.UNIQUE_ID, new ObjectCreatorDelegate(SwapProductionScenarioT));
        AddCreationMethod(Transmissions2.MergeScenarioDataT.UNIQUE_ID, new ObjectCreatorDelegate(MergeScenarioDataT));
        AddCreationMethod(UserFieldDefinitionT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserFieldT));
        AddCreationMethod(UserResetMyPasswordT.UNIQUE_ID, new ObjectCreatorDelegate(CreateUserResetMyPasswordT));
        
        AddCreationMethod(LookupItemCleanoutTableT.UNIQUE_ID, new ObjectCreatorDelegate(CreateLookupItemCleanoutTableT));
        AddCreationMethod(ItemCleanoutTableCopyT.UNIQUE_ID, new ObjectCreatorDelegate(CreateItemCleanoutTableCopyT));
        AddCreationMethod(ItemCleanoutTableDeleteT.UNIQUE_ID, new ObjectCreatorDelegate(CreateItemCleanoutTableDeleteT));
        AddCreationMethod(ItemCleanoutTableNewT.UNIQUE_ID, new ObjectCreatorDelegate(CreateItemCleanoutTableNewT));
        AddCreationMethod(ItemCleanoutTableUpdateT.UNIQUE_ID, new ObjectCreatorDelegate(CreateItemCleanoutTableUpdateT));

    }

    private object CreateUserResetMyPasswordT(IReader a_reader)
    {
        return new UserResetMyPasswordT(a_reader);
    }

    private readonly Dictionary<int, ObjectCreatorDelegate> m_creationMethods = new();

    private void AddCreationMethod(int uniqueId, ObjectCreatorDelegate creator)
    {
        if (m_creationMethods.ContainsKey(uniqueId))
        {
            throw new PTException("A transmission with this UNIQUE_ID has already been registered as a creation method.");
        }

        m_creationMethods[uniqueId] = creator;
    }

    #region IClassFactory Members
    public object Deserialize(IReader reader)
    {
        int id = -1;
        try
        {
            reader.Read(out id);
            //                // **********************************************************************************************************************************************************************************************
            //                // 2010.11.05: Only SEC ever attempted to use this transmission that was replaced before the feature it was for was completed.
            //                // You should be able to delete this commented out code shortly after they're live and all instances of this have vanished from their undo sets.
            //                // You can find the definition of this transmission commented out at the bottom of this file. It should also be deleted.
            //                // SEC should be going live around 2011.01.01
            //                // Watch out, some other tranmission will eventually end up using id 707.
            //                 **********************************************************************************************************************************************************************************************
            //                #if DEBUG
            //                if(id==707)
            //                {
            //                    return new ScenarioDetailResizeBlockT(reader);
            //                }
            //#else
            //                d
            //#endif
            if (m_creationMethods.TryGetValue(id, out ObjectCreatorDelegate f))
            {
                return f(reader);
            }

            throw new Exception("Not Found");
        }
        catch (Exception e)
        {
            throw new PTException("2771", e, new object[] { id.ToString(), e.Message });
        }
    }

    #region Creators
    #region Transmissions
    private object CreateFinishActivityT(IReader reader)
    {
        return new InternalActivityFinishT(reader);
    }

    private object CreateInternalActivityUpdateT(IReader reader)
    {
        return new InternalActivityUpdateT(reader);
    }

    private object CreateBalancedCompositeDispatcherDefinitionCopyT(IReader reader)
    {
        return new BalancedCompositeDispatcherDefinitionCopyT(reader);
    }

    private object CreateBalancedCompositeDispatcherDefinitionDefaultT(IReader reader)
    {
        return new BalancedCompositeDispatcherDefinitionDefaultT(reader);
    }

    private object CreateBalancedCompositeDispatcherDefinitionDeleteAllT(IReader reader)
    {
        return new BalancedCompositeDispatcherDefinitionDeleteAllT(reader);
    }

    private object CreateBalancedCompositeDispatcherDefinitionDeleteT(IReader reader)
    {
        return new BalancedCompositeDispatcherDefinitionDeleteT(reader);
    }

    private object CreateBalancedCompositeDispatcherDefinitionUpdateT(IReader reader)
    {
        return new BalancedCompositeDispatcherDefinitionUpdateT(reader);
    }
    
    private object CreateBalancedCompositeDispatcherDefinitionImportT(IReader reader)
    {
        return new BalancedCompositeDispatcherDefinitionImportT(reader);
    }
    private object CreateCapabilityCopyT(IReader reader)
    {
        return new CapabilityCopyT(reader);
    }

    private object CreateCapabilityDefaultT(IReader reader)
    {
        return new CapabilityDefaultT(reader);
    }

    private object CreateCapabilityDeleteAllT(IReader reader)
    {
        return new CapabilityDeleteAllT(reader);
    }

    private object CreateCapabilityDeleteT(IReader reader)
    {
        return new CapabilityDeleteT(reader);
    }

    private object CreateCapacityInterval(IReader reader)
    {
        return new PT.Transmissions.CapacityInterval(reader);
    }

    private object CreateCapacityIntervalConvertT(IReader reader)
    {
        return new CapacityIntervalConvertT(reader);
    }

    private object CreateCapacityIntervalCopyT(IReader reader)
    {
        return new CapacityIntervalCopyT(reader);
    }

    private object CreateCapacityIntervalCopyToResourceT(IReader reader)
    {
        return new CapacityIntervalCopyToResourceT(reader);
    }

    private object CreateCapacityIntervalDefaultT(IReader reader)
    {
        return new CapacityIntervalDefaultT(reader);
    }

    private object CreateCapacityIntervalDeleteAllT(IReader reader)
    {
        return new CapacityIntervalDeleteAllT(reader);
    }

    private object CreateCapacityIntervalDeleteT(IReader reader)
    {
        return new CapacityIntervalDeleteT(reader);
    }

    private object CreateCapacityIntervalMoveInTimeT(IReader reader)
    {
        return new CapacityIntervalMoveInTimeT(reader);
    }

    private object CreateCapacityIntervalMoveT(IReader reader)
    {
        return new CapacityIntervalMoveT(reader);
    }

    private object CreateCapacityIntervalNewT(IReader reader)
    {
        return new CapacityIntervalNewT(reader);
    }

    private object CreateCapacityIntervalShareT(IReader reader)
    {
        return new CapacityIntervalShareT(reader);
    }

    private object CreateCapacityIntervalUpdateT(IReader reader)
    {
        return new CapacityIntervalUpdateT(reader);
    }

    private object CreateCapacityIntervalSetResourcesT(IReader reader)
    {
        return new CapacityIntervalSetResourcesT(reader);
    }

    private object CreateCellCopyT(IReader reader)
    {
        return new CellCopyT(reader);
    }

    private object CreateCellDefaultT(IReader reader)
    {
        return new CellDefaultT(reader);
    }

    private object CreateCellDeleteAllT(IReader reader)
    {
        return new CellDeleteAllT(reader);
    }

    private object CreateCellDeleteT(IReader reader)
    {
        return new CellDeleteT(reader);
    }

    private object CreateCustomerCopyT(IReader reader)
    {
        return new CustomerCopyT(reader);
    }

    private object CreateCustomerDefaultT(IReader reader)
    {
        return new CustomerDefaultT(reader);
    }

    private object CreateCustomerEditT(IReader reader)
    {
        return new CustomerEditT(reader);
    }

    private object CreateCustomerT(IReader reader)
    {
        return new CustomerT(reader);
    }

    private object CreateCustomerDeleteAllT(IReader reader)
    {
        return new CustomerDeleteAllT(reader);
    }

    private object CreateCustomerDeleteT(IReader reader)
    {
        return new CustomerDeleteT(reader);
    }

    private object CreateDepartmentCopyT(IReader reader)
    {
        return new DepartmentCopyT(reader);
    }

    private object CreateDepartmentDefaultT(IReader reader)
    {
        return new DepartmentDefaultT(reader);
    }

    private object CreateDepartmentDeleteAllT(IReader reader)
    {
        return new DepartmentDeleteAllT(reader);
    }

    private object CreateDepartmentDeleteT(IReader reader)
    {
        return new DepartmentDeleteT(reader);
    }

    private object CreateDispatcherDefinitionManagerDefaultT(IReader reader)
    {
        return new SetDefaultDispatcherDefinitionOfDefinitionManagerT(reader);
    }

    private object CreateJobRequestNewExternalIdT(IReader reader)
    {
        return new JobRequestNewExternalIdT(reader);
    }

    private object CreateJobCopyT(IReader reader)
    {
        return new JobCopyT(reader);
    }

    private object CreateJobDefaultT(IReader reader)
    {
        return new JobDefaultT(reader);
    }

    private object CreateJobDeleteAllT(IReader reader)
    {
        return new JobDeleteAllT(reader);
    }

    private object CreateJobDeleteJobsT(IReader reader)
    {
        return new JobDeleteJobsT(reader);
    }

    private object CreateJobsPrintedT(IReader reader)
    {
        return new JobsPrintedT(reader);
    }

    private object CreateJobGenerateT(IReader reader)
    {
        return new JobGenerateT(reader);
    }

    private object CreatePurchaseToStockMoveT(IReader reader)
    {
        return new PurchaseToStockMoveT(reader);
    }

    private object CreatePurchaseToStockRevertT(IReader reader)
    {
        return new PurchaseToStockRevertT(reader);
    }

    private object CreateSplitOperationT(IReader reader)
    {
        return new SplitOperationT(reader);
    }

    private object CreateUnSplitOperationT(IReader reader)
    {
        return new UnSplitOperationT(reader);
    }

    private object CreateManufacturingOrderCopyT(IReader reader)
    {
        return new ManufacturingOrderCopyT(reader);
    }

    private object CreateManufacturingOrderDefaultT(IReader reader)
    {
        return new ManufacturingOrderDefaultT(reader);
    }

    private object CreateManufacturingOrderDeleteT(IReader reader)
    {
        return new ManufacturingOrderDeleteT(reader);
    }

    private object CreateOperationDeleteT(IReader reader)
    {
        return new OperationDeleteT(reader);
    }

    private object CreatePlantAddResourceT(IReader reader)
    {
        return new PlantAddResourceT(reader);
    }

    private object CreatePlantSetResourceDeptT(IReader reader)
    {
        return new PlantSetResourceDeptT(reader);
    }

    private object CreatePlantCopyT(IReader reader)
    {
        return new PlantCopyT(reader);
    }

    private object CreatePlantDefaultT(IReader reader)
    {
        return new PlantDefaultT(reader);
    }

    private object CreatePlantDeleteAllT(IReader reader)
    {
        return new PlantDeleteAllT(reader);
    }

    private object CreatePlantDeleteT(IReader reader)
    {
        return new PlantDeleteT(reader);
    }

    private object CreatePTAttributeCopyT(IReader a_reader)
    {
        return new PTAttributeCopyT(a_reader);
    }

    private object CreatePTAttributeDefaultT(IReader a_reader)
    {
        return new PTAttributeDefaultT(a_reader);
    }

    private object CreatePTAttributeDeleteAllT(IReader a_reader)
    {
        return new PTAttributeDeleteAllT(a_reader);
    }

    private object CreatePTAttributeDeleteT(IReader a_reader)
    {
        return new PTAttributeDeleteT(a_reader);
    }

    private object CreateUserFieldDefinitionCopyT(IReader a_reader)
    {
        return new UserFieldDefinitionCopyT(a_reader);
    }

    private object CreateUserFieldDefinitionDefaultT(IReader a_reader)
    {
        return new UserFieldDefinitionDefaultT(a_reader);
    }

    private object CreateUserFieldDefinitionDeleteAllT(IReader a_reader)
    {
        return new UserFieldDefinitionDeleteAllT(a_reader);
    }

    private object CreateUserFieldDefinitionDeleteT(IReader a_reader)
    {
        return new UserFieldDefinitionDeleteT(a_reader);
    }

    private object CreatePTObjectBase(IReader reader)
    {
        return new PTObjectBase(reader);
    }

    private object CreateRecurringCapacityInterval(IReader reader)
    {
        return new PT.Transmissions.RecurringCapacityInterval(reader);
    }

    private object CreateRecurringCapacityIntervalConvertT(IReader reader)
    {
        return new RecurringCapacityIntervalConvertT(reader);
    }

    private object CreateRecurringCapacityIntervalCopyT(IReader reader)
    {
        return new RecurringCapacityIntervalCopyT(reader);
    }

    private object CreateRecurringCapacityIntervalCopyToResourceT(IReader reader)
    {
        return new RecurringCapacityIntervalCopyToResourceT(reader);
    }

    private object CreateRecurringCapacityIntervalDefaultT(IReader reader)
    {
        return new RecurringCapacityIntervalDefaultT(reader);
    }

    private object CreateRecurringCapacityIntervalDeleteAllT(IReader reader)
    {
        return new RecurringCapacityIntervalDeleteAllT(reader);
    }

    private object CreateRecurringCapacityIntervalDeleteT(IReader reader)
    {
        return new RecurringCapacityIntervalDeleteT(reader);
    }

    private object CreateRecurringCapacityIntervalMoveInTimeT(IReader reader)
    {
        return new RecurringCapacityIntervalMoveInTimeT(reader);
    }

    private object CreateRecurringCapacityIntervalMoveT(IReader reader)
    {
        return new RecurringCapacityIntervalMoveT(reader);
    }

    private object CreateRecurringCapacityIntervalShareT(IReader reader)
    {
        return new RecurringCapacityIntervalShareT(reader);
    }

    private object CreateRecurringCapacityIntervalUpdateT(IReader reader)
    {
        return new RecurringCapacityIntervalUpdateT(reader);
    }
    private object CreateRecurringCapacityIntervalUpdateMultiT(IReader reader)
    {
        return new RecurringCapacityIntervalUpdateMultiT(reader);
    }

    private object CreateRecurringCapacityIntervalSetResourcesT(IReader reader)
    {
        return new RecurringCapacityIntervalSetResourcesT(reader);
    }

    private object CreateRequiredAttribute(IReader reader)
    {
        return new RequiredAttribute(reader);
    }

    private object CreateResourceCopyT(IReader reader)
    {
        return new ResourceCopyT(reader);
    }

    private object CreateResourceDefaultT(IReader reader)
    {
        return new ResourceDefaultT(reader);
    }

    private object CreateResourceDeleteAllT(IReader reader)
    {
        return new ResourceDeleteAllT(reader);
    }

    private object CreateResourceDeleteT(IReader reader)
    {
        return new ResourceDeleteT(reader);
    }

    private object CreateResourceDeleteMultiT(IReader reader)
    {
        return new ResourceDeleteMultiT(reader);
    }

    private object CreateResourceSetShopViewersT(IReader reader)
    {
        return new ResourceSetShopViewersT(reader);
    }

    private object CreateScenarioChangeT(IReader reader)
    {
        return new ScenarioChangeT(reader);
    }

    private object CreateScenarioClockAdvanceT(IReader reader)
    {
        return new ScenarioClockAdvanceT(reader);
    }

    private object CreateScenarioDetailOnlineT(IReader reader)
    {
        return new ScenarioDetailOnlineT(reader);
    }

    private object CreateScenarioDetailOfflineT(IReader reader)
    {
        return new ScenarioDetailOfflineT(reader);
    }

    private object CreateScenarioCopyT(IReader reader)
    {
        return new ScenarioCopyT(reader);
    }

    private object CreateScenarioNewT(IReader reader)
    {
        return new ScenarioNewT(reader);
    }

    private object CreateScenarioLoadT(IReader reader)
    {
        return new ScenarioLoadT(reader);
    }

    private object CreateScenarioUnloadT(IReader reader)
    {
        return new ScenarioUnloadT(reader);
    }

    private object CreateScenarioReloadT(IReader a_reader)
    {
        return new ScenarioReloadT(a_reader);
    }

    private object CreateScenarioDetailClearResourcePerformancesT(IReader reader)
    {
        return new ScenarioDetailClearResourcePerformancesT(reader);
    }

    private object CreateScenarioDeleteT(IReader reader)
    {
        return new ScenarioDeleteT(reader);
    }

    private object CreateScenarioDetailCompressT(IReader reader)
    {
        return new ScenarioDetailCompressT(reader);
    }

    private object CreateScenarioDetailHoldSettingsT(IReader reader)
    {
        return new ScenarioDetailHoldSettingsT(reader);
    }

    private object CreateScenarioDetailConfirmOperationConstraintsT(IReader reader)
    {
        return new ScenarioDetailConfirmOperationConstraintsT(reader);
    }

    private object CreateScenarioDetailExpediteJobsT(IReader reader)
    {
        return new ScenarioDetailExpediteJobsT(reader);
    }

    private object CreateScenarioDetailExpediteMOsT(IReader reader)
    {
        return new ScenarioDetailExpediteMOsT(reader);
    }

    private object CreateScenarioDetailAnchorActivitiesT(IReader reader)
    {
        return new ScenarioDetailAnchorActivitiesT(reader);
    }

    private object CreateScenarioDetailAnchorJobsT(IReader reader)
    {
        return new ScenarioDetailAnchorJobsT(reader);
    } 
    private object CreateScenarioDetailJobResetJITAndSubJobNeedDateT(IReader reader)
    {
        return new ScenarioDetailJobResetJITAndSubJobNeedDateT(reader);
    }

    private object CreateScenarioDetailAnchorMOsT(IReader reader)
    {
        return new ScenarioDetailAnchorMOsT(reader);
    }

    private object CreateScenarioDetailAnchorOperationsT(IReader reader)
    {
        return new ScenarioDetailAnchorOperationsT(reader);
    }

    private object CreateScenarioDetailHoldJobsT(IReader reader)
    {
        return new ScenarioDetailHoldJobsT(reader);
    }

    private object CreateScenarioDetailSetJobCommitmentsAndPrioritiesT(IReader reader)
    {
        return new ScenarioDetailSetJobPropertiesT(reader);
    }

    private object CreateScenarioDetailHoldMOsT(IReader reader)
    {
        return new ScenarioDetailHoldMOsT(reader);
    }

    private object CreateScenarioDetailHoldOperationsT(IReader reader)
    {
        return new ScenarioDetailHoldOperationsT(reader);
    }

    private object CreateScenarioDetailLockActivitiesT(IReader reader)
    {
        return new ScenarioDetailLockActivitiesT(reader);
    }

    private object CreateScenarioDetailLockBlocksT(IReader reader)
    {
        return new ScenarioDetailLockBlocksT(reader);
    }

    private object CreateScenarioDetailLockJobsT(IReader reader)
    {
        return new ScenarioDetailLockJobsT(reader);
    }

    private object CreateScenarioDetailLockMOsT(IReader reader)
    {
        return new ScenarioDetailLockMOsT(reader);
    }
    private object CreateScenarioDetailLockMOsToPathT(IReader reader)
    {
        return new ScenarioDetailLockMOsToPathT(reader);
    }

    private object CreateScenarioDetailLockOperationsT(IReader reader)
    {
        return new ScenarioDetailLockOperationsT(reader);
    }

    private object CreateScenarioDetailMoveT(IReader reader)
    {
        return new ScenarioDetailMoveT(reader);
    }

    private object CreateScenarioDetailOptimizeT(IReader reader)
    {
        return new ScenarioDetailOptimizeT(reader);
    }

    private object CreateScenarioDetailRescheduleJobsT(IReader reader)
    {
        return new ScenarioDetailRescheduleJobsT(reader);
    }

    private object CreateScenarioDetailRescheduleMOsT(IReader reader)
    {
        return new ScenarioDetailRescheduleMOsT(reader);
    }

    private object CreateScenarioDetailScheduleJobsT(IReader reader)
    {
        return new ScenarioDetailScheduleJobsT(reader);
    }

    private object CreateScenarioDetailSetCapabilitiesT(IReader reader)
    {
        return new ScenarioDetailSetCapabilitiesT(reader);
    }

    private object CreateScenarioDetailSetCapabilityResourcesT(IReader reader)
    {
        return new ScenarioDetailSetCapabilityResourcesT(reader);
    }

    private object CreateScenarioDetailClearT(IReader reader)
    {
        return new ScenarioDetailClearT(reader);
    }

    private object CreateScenarioDetailCustomerT(IReader reader)
    {
        return new ScenarioDetailCustomerT(reader);
    }

    private object CreateScenarioClearUndoSetsT(IReader a_reader)
    {
        return new ScenarioClearUndoSetsT(a_reader);
    }

    private object CreateScenarioChangeTypeT(IReader reader)
    {
        return new ScenarioChangeTypeT(reader);
    }

    private object CreateScenarioPublishT(IReader reader)
    {
        return new ScenarioPublishT(reader);
    }

    private object CreateScenarioUndoCheckpointT(IReader reader)
    {
        return new ScenarioUndoCheckpointT(reader);
    }

    private object CreateScenarioUndoT(IReader reader)
    {
        return new ScenarioUndoT(reader);
    }
    private object CreateScenarioStartUndoT(IReader reader)
    {
        return new ScenarioStartUndoT(reader);
    }
    private object CreateSystemMessageDeleteT(IReader reader)
    {
        return new SystemMessageDeleteT(reader);
    }

    private object CreateSystemOptionsT(IReader reader)
    {
        return new SystemOptionsT(reader);
    }

    private object CreateSystemPublishOptionsT(IReader reader)
    {
        return new SystemPublishOptionsT(reader);
    }

    private object CreateScenarioKpiSnapshotT(IReader reader)
    {
        return new ScenarioKpiSnapshotT(reader);
    }

    private object CreateKpiSnapshotOfLiveScenarioT(IReader reader)
    {
        return new KpiSnapshotOfLiveScenarioT(reader);
    }

    private object CreateScenarioKpiOptionsUpdateT(IReader reader)
    {
        return new ScenarioKpiOptionsUpdateT(reader);
    }

    private object CreateScenarioKpiVisibilityT(IReader reader)
    {
        return new ScenarioKpiVisibilityT(reader);
    }

    private object CreateUserChatT(IReader reader)
    {
        return new UserChatT(reader);
    }

    private object CreateUserSettingsChangeT(IReader reader)
    {
        return new UserSettingsChangeT(reader);
    }

    private object CreateUserSettingsT(IReader reader)
    {
        return new UserSettingsT(reader);
    }

    private object CreateUserCopyT(IReader reader)
    {
        return new UserCopyT(reader);
    }

    private object CreateUserDefaultT(IReader reader)
    {
        return new UserDefaultT(reader);
    }

    private object CreateUserDeleteAllT(IReader reader)
    {
        return new UserDeleteAllT(reader);
    }

    private object CreateUserDeleteT(IReader reader)
    {
        return new UserDeleteT(reader);
    }

    // This transmission is no longer used, but it's kept here so that we can playback recordings from older instances
    // that still use UserLogOffT
    private object CreateUserLogOffT(IReader reader)
    {
        return new UserLogOffT(reader);
    }

    private object CreateUserLogOnT(IReader reader)
    {
        return new UserLogOnT(reader);
    }

    private object CreateUserScheduleViewerSettingsChangeT(IReader reader)
    {
        return new UserScheduleViewerSettingsChangeT(reader);
    }

    private object CreateVesselTypeCopyT(IReader reader)
    {
        return new VesselTypeCopyT(reader);
    }

    private object CreateVesselTypeDefaultT(IReader reader)
    {
        return new VesselTypeDefaultT(reader);
    }

    private object CreateVesselTypeDeleteAllT(IReader reader)
    {
        return new VesselTypeDeleteAllT(reader);
    }

    private object CreateVesselTypeDeleteT(IReader reader)
    {
        return new VesselTypeDeleteT(reader);
    }

    private object CreateScenarioManagerUndoSettingsT(IReader reader)
    {
        return new ScenarioManagerUndoSettingsT(reader);
    }

    private object CreateSystemStartupOptionsT(IReader reader)
    {
        return new SystemStartupOptionsT(reader);
    }

    private object CreateUserErrorT(IReader reader)
    {
        return new UserErrorT(reader);
    }

    private object CreateScenarioDetailJobT(IReader reader)
    {
        return new ScenarioDetailJobT(reader);
    }

    private object CreateScenarioDetailItemT(IReader reader)
    {
        return new ScenarioDetailItemT(reader);
    }

    private object CreateScenarioDetailWarehouseT(IReader reader)
    {
        return new ScenarioDetailWarehouseT(reader);
    }

    private object CreateScenarioDetailSalesOrderT(IReader reader)
    {
        return new ScenarioDetailSalesOrderT(reader);
    }

    private object CreateScenarioDetailPTAttributeT(IReader a_reader)
    {
        return new ScenarioDetailPTAttributeT(a_reader);
    }

    private object CreateScenarioDetailUserFieldDefinitionT(IReader a_reader)
    {
        return new ScenarioDetailUserFieldDefinitionT(a_reader);
    }

    private object CreateScenarioDetailForecastT(IReader reader)
    {
        return new ScenarioDetailForecastT(reader);
    }

    private object CreateForecastIntervalQtyChangeT(IReader reader)
    {
        return new Transmissions.Forecast.ForecastIntervalQtyChangeT(reader);
    }

    private object CreateScenarioDetailTransferOrderT(IReader reader)
    {
        return new ScenarioDetailTransferOrderT(reader);
    }

    private object CreateScenarioDetailPurchaseOrderT(IReader reader)
    {
        return new ScenarioDetailPurchaseOrderT(reader);
    }

    private object CreateShopViewResourceOptionNewT(IReader reader)
    {
        return new ShopViewResourceOptionNewT(reader);
    }

    private object CreateShopViewResourceOptionCopyT(IReader reader)
    {
        return new ShopViewResourceOptionCopyT(reader);
    }

    private object CreateShopViewResourceOptionUpdateT(IReader reader)
    {
        return new ShopViewResourceOptionUpdateT(reader);
    }

    private object CreateShopViewResourceOptionDeleteT(IReader reader)
    {
        return new ShopViewResourceOptionDeleteT(reader);
    }

    private object CreateShopViewSystemOptionsUpdateT(IReader reader)
    {
        return new ShopViewSystemOptionsUpdateT(reader);
    }

    private object CreateShopViewOptionsAssignmentT(IReader reader)
    {
        return new ShopViewOptionsAssignmentT(reader);
    }

    private object CreateScenarioDetailOptimizeSettingsChangeT(IReader reader)
    {
        return new ScenarioDetailOptimizeSettingsChangeT(reader);
    }

    private object CreateScenarioDetailCompressSettingsChangeT(IReader reader)
    {
        return new ScenarioDetailCompressSettingsChangeT(reader);
    }

    private object CreateScenarioChecksumT(IReader reader)
    {
        return new ScenarioChecksumT(reader);
    }

    private object CreateLookupTableDeleteAllT(IReader reader)
    {
        return new LookupTableDeleteAllT(reader);
    }

    private object CreateScenarioDetailLotAllocationRuleT(IReader reader)
    {
        return new ScenarioDetailLotAllocationRuleT(reader);
    }

    //Setup Code Tables
    private object CreateAttributeCodeTableDeleteT(IReader reader)
    {
        return new AttributeCodeTableDeleteT(reader);
    }

    private object CreateAttributeCodeTableCopyT(IReader reader)
    {
        return new AttributeCodeTableCopyT(reader);
    }

    private object CreateAttributeCodeTableNewT(IReader reader)
    {
        return new AttributeCodeTableNewT(reader);
    }

    private object CreateAttributeCodeTableUpdateT(IReader reader)
    {
        return new AttributeCodeTableUpdateT(reader);
    }

    private object CreateScenarioTouchT(IReader reader)
    {
        return new ScenarioTouchT(reader);
    }

    private object CreateScenarioDetailProductRulesT(IReader reader)
    {
        return new ScenarioDetailProductRulesT(reader);
    }

    private object CreateSetupRangeDeleteT(IReader reader)
    {
        return new SetupRangeDeleteT(reader);
    }

    private object CreateSetupRangeUpdateT(IReader reader)
    {
        return new SetupRangeUpdateT(reader);
    }

    private object CreateSetupRangeCopyT(IReader reader)
    {
        return new SetupRangeCopyT(reader);
    }

    private object CreateScenarioDetailAlternatePathMoveT(IReader reader)
    {
        return new ScenarioDetailAlternatePathMoveT(reader);
    }

    private object CreateCtpT(IReader reader)
    {
        return new Transmissions.CTP.CtpT(reader);
    }

    private object CreateScenarioCtpT(IReader a_reader)
    {
        return new Transmissions.CTP.ScenarioCtpT(a_reader);
    }

    private object CreateScenarioDetailSplitMOT(IReader reader)
    {
        return new ScenarioDetailSplitMOT(reader);
    }

    private object CreateScenarioDetailSplitMOAtTimeT(IReader reader)
    {
        return new ScenarioDetailSplitJobOrMOT(reader);
    }

    private object CreateScenarioDetailUnsplitMOT(IReader reader)
    {
        return new ScenarioDetailJoinJobOrMOT(reader);
    }

    private object CreateScenarioDetailChangeQtyByCycleT(IReader reader)
    {
        return new ScenarioDetailChangeMOQtyT(reader);
    }

    private object CreateScenarioDetailLockAndAnchorActivitiesT(IReader reader)
    {
        return new ScenarioDetailLockAndAnchorActivitiesT(reader);
    }

    private object CreateAllowedHelperResourcesT(IReader reader)
    {
        return new AllowedHelperResourcesT(reader);
    }

    private object CreateScheduleClearCommitT(IReader reader)
    {
        return new ScheduleClearCommitT(reader);
    }

    private object CreateScheduleCommitT(IReader reader)
    {
        return new ScheduleCommitT(reader);
    }

    private object CreateMOBatchDefinitionSetT(IReader reader)
    {
        return new ManufacturingOrderBatchDefinitionSetT(reader);
    }

    private object CreateScenarioDetailNeedCompressT(IReader reader)
    {
        return new ScenarioDetailJitCompressT(reader);
    }

    private object CreateScenarioDetailPlantT(IReader reader)
    {
        return new ScenarioDetailPlantT(reader);
    }

    private object CreateScenarioDetailDepartmentT(IReader reader)
    {
        return new ScenarioDetailDepartmentT(reader);
    }

    private object CreateScenarioDetailResourceT(IReader reader)
    {
        return new ScenarioDetailResourceT(reader);
    }

    private object CreateScenarioDetailCapabilityT(IReader reader)
    {
        return new ScenarioDetailCapabilityT(reader);
    }

    private object CreateScenarioDetailCellT(IReader reader)
    {
        return new ScenarioDetailCellT(reader);
    }

    private object CreateInventoryTransferRuleT(IReader a_reader)
    {
        return new InventoryTransferRulesT(a_reader);
    }

    private object CreatePruneScenarioT(IReader a_reader)
    {
        return new PruneScenarioT(a_reader);
    }

    private object CreateScenarioAddNewPrunedT(IReader a_reader)
    {
        return new ScenarioAddNewPrunedT(a_reader);
    }

    private object CreateScenarioIsolateT(IReader a_reader)
    {
        return new ScenarioIsolateT(a_reader);
    }

    private object CreatePlantPermissionSetT(IReader a_reader)
    {
        return new PlantPermissionSetT(a_reader);
    }

    private object CreateUserPermissionSetT(IReader a_reader)
    {
        return new UserPermissionSetT(a_reader);
    }

    // !ALTERNATE_PATH!; CreateScenarioDetailAlternatePathLock
    private object CreateScenarioDetailAlternatePathLock(IReader reader)
    {
        return new ScenarioDetailAlternatePathLockT(reader);
    }

    private object CreateAddInControlUpdateT(IReader reader)
    {
        return new AddInControlUpdateT(reader);
    }

    private object CreateSwitchSystemStateT(IReader a_reader)
    {
        return new SystemStateSwitchT(a_reader);
    }

    private object CreateAutoUpdateKeyT(IReader a_reader)
    {
        return new AutoUpdateKeyT(a_reader);
    }

    private object CreateRuleSeekSettingsChangeT(IReader reader)
    {
        return new CoPilotSettingsChangeT(reader);
    }

    private object CreateRuleSeekStartT(IReader reader)
    {
        return new ScenarioRuleSeekStartT(reader);
    }

    private object CreateRuleSeekCompletionT(IReader reader)
    {
        return new RuleSeekCompletionT(reader);
    }

    private object CreateRuleSeekStatusUpdateT(IReader reader)
    {
        return new CoPilotStatusUpdateT(reader);
    }

    private object CreateScenarioCopyWithNewOptimizeRulesT(IReader a_reader)
    {
        return new ScenarioCopyForRuleSeekT(a_reader);
    }

    private object CreateInsertJobsStartT(IReader a_reader)
    {
        return new InsertJobsStartT(a_reader);
    }

    private object CreateInsertJobsUserEndT(IReader a_reader)
    {
        return new InsertJobsUserEndT(a_reader);
    }

    private object CreateScenarioCopyForInsertJobsT(IReader a_reader)
    {
        return new ScenarioCopyForInsertJobsT(a_reader);
    }

    private object CreateCopilotDiagnosticsUpdateT(IReader a_reader)
    {
        return new CoPilotDiagnositcsUpdateT(a_reader);
    }

    private object CreateNewGameT(IReader a_reader)
    {
        return new Transmissions.Game.NewGameT(a_reader);
    }

    private object CreateScenarioAddNewT(IReader a_reader)
    {
        return new ScenarioAddNewT(a_reader);
    }

    private object CreateScenarioReplaceT(IReader a_reader)
    {
        return new ScenarioReplaceT(a_reader);
    }

    private object CreateSystemMessageT(IReader a_reader)
    {
        return new SystemMessageT(a_reader);
    }

    private object CreateScenarioDetailClearPastShortTermT(IReader a_reader)
    {
        return new ScenarioDetailClearPastShortTermT(a_reader);
    }

    private object CreateAdminLogOffT(IReader a_reader)
    {
        return new UserAdminLogOffT(a_reader);
    }

    private object CreateTriggerImportT(IReader a_reader)
    {
        return new TriggerImportT(a_reader);
    }

    private object CreateWorkspaceSharedDeleteT(IReader a_reader)
    {
        return new WorkspaceSharedDeleteT(a_reader);
    }

    private object CreateWorkspaceTemplateSharedUpdateT(IReader a_reader)
    {
        return new WorkspaceTemplateUpdateT(a_reader);
    }

    private object CreateWorkspaceSharedUpdateT(IReader a_reader)
    {
        return new WorkspaceSharedUpdateT(a_reader);
    }

    private object CreateForecastDeleteT(IReader a_reader)
    {
        return new Transmissions.Forecast.ForecastShipmentDeleteT(a_reader);
    }

    private object CreateForecastShipmentGenerateT(IReader a_reader)
    {
        return new Transmissions.Forecast.ForecastShipmentGenerateT(a_reader);
    }

    private object CreateMultiForecastShipmentGenerateT(IReader a_reader)
    {
        return new Transmissions.Forecast.MultiForecastShipmentGenerateT(a_reader);
    }

    private object CreateForecastShipmentAdjustmentT(IReader a_reader)
    {
        return new Transmissions.Forecast.ForecastShipmentAdjustmentT(a_reader);
    }

    private object CreateLicenseKeyT(IReader a_reader)
    {
        return new LicenseKeyT(a_reader);
    }

    private object CreateInstanceMessageT(IReader a_reader)
    {
        return new InstanceMessageT(a_reader);
    }

    private object CreatePacketT(IReader a_reader)
    {
        return new PacketT(a_reader, new ObjectCreatorDelegate(Deserialize));
    }

    private object CreatePurchaseToStockEditT(IReader a_reader)
    {
        return new PurchaseToStockEditT(a_reader);
    }

    private object CreateSalesOrderEditT(IReader a_reader)
    {
        return new SalesOrderEditT(a_reader);
    }

    private object CreateResourceEditT(IReader a_reader)
    {
        return new ResourceEditT(a_reader);
    }

    private object CreateUserEditT(IReader a_reader)
    {
        return new UserEditT(a_reader);
    }

    private object CreateMaterialEditT(IReader a_reader)
    {
        return new MaterialEditT(a_reader);
    }

    private object CreateJobEditT(IReader a_reader)
    {
        return new JobEditT(a_reader);
    }

    private object CreateInventoryEditT(IReader a_reader)
    {
        return new InventoryEditT(a_reader);

    }
    private object CreateStorageAreaEditT(IReader a_reader)
    {
        return new StorageAreaEditT(a_reader);
    }

    private object CreatePTAttributeEditT(IReader a_reader)
    {
        return new PTAttributeEditT(a_reader);
    }

    private object CreateSendSupportEmailT(IReader a_reader)
    {
        return new SendSupportEmailT(a_reader);
    }
    private object CreateUserLogonProcessingT(IReader a_reader)
    {
        return new UserLogonAttemptsProcessingT(a_reader);
    }

    private object CreateScenarioDetailSetPurchaseToStockValuesT(IReader reader)
    {
        return new ScenarioDetailSetPurchaseToStockValuesT(reader);
    }

    private object CreateScenarioDetailSetSalesOrderValuesT(IReader reader)
    {
        return new ScenarioDetailSetSalesOrderValuesT(reader);
    }

    private object CreateScenarioSettingDataT(IReader a_reader)
    {
        return new ScenarioSettingDataT(a_reader);
    }

    private object CreateSystemSettingDataT(IReader a_reader)
    {
        return new SystemSettingDataT(a_reader);
    }

    private object CreateScenarioDetailMaterialUpdateT(IReader reader)
    {
        return new ScenarioDetailMaterialUpdateT(reader);
    }

    private object CreateApiHoldT(IReader reader)
    {
        return new ApiHoldT(reader);
    }

    private object CreateApiLockT(IReader reader)
    {
        return new ApiLockT(reader);
    }

    private object CreateApiAnchorT(IReader reader)
    {
        return new ApiAnchorT(reader);
    }

    private object CreateApiUnscheduleT(IReader reader)
    {
        return new ApiUnscheduleT(reader);
    }

    private object CreateApiActivityUpdateT(IReader reader)
    {
        return new ApiActivityUpdateT(reader);
    }

    private object CreateDataActivationWarningT(IReader a_reader)
    {
        return new DataActivationWarningT();
    }

    private object SwapProductionScenarioT(IReader a_reader)
    {
        return new ConvertToProductionScenarioT(a_reader);
    }

    private object MergeScenarioDataT(IReader a_reader)
    {
        return new MergeScenarioDataT(a_reader);
    }
    #endregion Transmissions

    #region ERP Transmissions
    private object CreateImportT(IReader reader)
    {
        return new ImportT(reader, new ObjectCreatorDelegate(Deserialize));
    }

    private object CreateApplicationExceptionList(IReader reader)
    {
        return new ApplicationExceptionList(reader);
    }

    private object CreateCapabilityT(IReader reader)
    {
        return new CapabilityT(reader);
    }

    private object CreateCapacityIntervalT(IReader reader)
    {
        return new CapacityIntervalT(reader);
    }

    private object CreateRecurringCapacityIntervalT(IReader reader)
    {
        return new RecurringCapacityIntervalT(reader);
    }

    private object CreateCellT(IReader reader)
    {
        return new CellT(reader);
    }

    private object CreateDepartmentT(IReader reader)
    {
        return new DepartmentT(reader);
    }

    private object CreateJobT(IReader reader)
    {
        return new JobT(reader);
    }

    private object CreateUserT(IReader reader)
    {
        return new UserT(reader);
    }

    private object CreatePlantT(IReader reader)
    {
        return new PlantT(reader);
    }

    private object CreateItemT(IReader reader)
    {
        return new ItemT(reader);
    }

    private object CreateWarehouseT(IReader reader)
    {
        return new WarehouseT(reader);
    }

    private object CreateResourceT(IReader reader)
    {
        return new ResourceT(reader);
    }

    private object CreatePurchaseToStockT(IReader reader)
    {
        return new PurchaseToStockT(reader);
    }

    private object CreateVesselTypeT(IReader reader)
    {
        return new VesselTypeT(reader);
    }

    private object CreateExportScenarioT(IReader reader)
    {
        return new ScenarioDetailExportT(reader);
    }

    private object CreateTriggerRecordingPlaybackT(IReader reader)
    {
        return new TriggerRecordingPlaybackT(reader);
    }

    private object CreatePerformImportStartedT(IReader reader)
    {
        return new PerformImportStartedT(reader);
    }

    private object CreatePerformImportCompletedT(IReader reader)
    {
        return new PerformImportCompletedT(reader);
    }

    private object CreatePerformExportStartedT(IReader reader)
    {
        return new PerformExportStartedT(reader);
    }

    private object CreatePerformExportCompletedT(IReader reader)
    {
        return new PerformExportCompletedT(reader);
    }

    private object CreateProductRulesT(IReader reader)
    {
        return new ProductRulesT(reader);
    }

    private object CreateLookupAttributeNumberRangeT(IReader reader)
    {
        return new LookupAttributeNumberRangeT(reader);
    }

    private object CreateLookupAttributeCodeTableT(IReader reader)
    {
        return new LookupAttributeCodeTableT(reader);
    }

    private object CreateSalesOrderT(IReader reader)
    {
        return new SalesOrderT(reader);
    }

    private object CreateForecastT(IReader reader)
    {
        return new ForecastT(reader);
    }

    private object CreateTransferOrderT(IReader reader)
    {
        if (reader.VersionNumber > 250)
        {
            return new TransferOrderT(reader);
        }

        return new TransferOrderT(reader, null);
    }

    private object CreateLotAllocationT(IReader reader)
    {
        return new LotAllocationRuleT(reader);
    }

    private object CreatePTAttributeT(IReader a_reader)
    {
        return new PTAttributeT(a_reader);
    }

    private object CreateActivityUpdateT(IReader a_reader)
    {
        return new ActivityUpdateT(a_reader);
    }

    private object CreateClientUserRestartT(IReader a_reader)
    {
        return new ClientUserRestartT(a_reader);
    }

    private object CreateTimeCleanOutTriggerTableNewT(IReader a_reader)
    {
        return new TimeCleanoutTriggerTableNewT(a_reader);
    }

    private object CreateTimeCleanOutTriggerTableCopyT(IReader a_reader)
    {
        return new TimeCleanoutTriggerTableCopyT(a_reader);
    }

    private object CreateTimeCleanOutTriggerTableDeleteT(IReader a_reader)
    {
        return new TimeCleanoutTriggerTableDeleteT(a_reader);
    }

    private object CreateTimeCleanOutTriggerTableUpdateT(IReader a_reader)
    {
        return new TimeCleanoutTriggerTableUpdateT(a_reader);
    }

    private object CreateOperationCountCleanOutTriggerTableNewT(IReader a_reader)
    {
        return new OperationCountCleanoutTriggerTableNewT(a_reader);
    }

    private object CreateOperationCountCleanOutTriggerTableCopyT(IReader a_reader)
    {
        return new OperationCountCleanoutTriggerTableCopyT(a_reader);
    }

    private object CreateOperationCountCleanOutTriggerTableDeleteT(IReader a_reader)
    {
        return new OperationCountCleanoutTriggerTableDeleteT(a_reader);
    }

    private object CreateOperationCountCleanOutTriggerTableUpdateT(IReader a_reader)
    {
        return new OperationCountCleanoutTriggerTableUpdateT(a_reader);
    }

    private object CreateProductionUnitsCleanOutTriggerTableNewT(IReader a_reader)
    {
        return new ProductionUnitsCleanoutTriggerTableNewT(a_reader);
    }

    private object CreateProductionUnitsCleanOutTriggerTableCopyT(IReader a_reader)
    {
        return new ProductionUnitsCleanoutTriggerTableCopyT(a_reader);
    }

    private object CreateProductionUnitsCleanOutTriggerTableDeleteT(IReader a_reader)
    {
        return new ProductionUnitsCleanoutTriggerTableDeleteT(a_reader);
    }

    private object CreateProductionUnitsCleanOutTriggerTableUpdateT(IReader a_reader)
    {
        return new ProductionUnitsCleanoutTriggerTableUpdateT(a_reader);
    }

    private object CreateCleanoutTriggerTablesT(IReader a_reader)
    {
        return new CleanoutTriggerTablesT(a_reader);
    }

    private object CreateLookupItemCleanoutTableT(IReader a_reader)
    {
        return new LookupItemCleanoutTableT(a_reader);
    }

    private object CreateItemCleanoutTableCopyT(IReader a_reader)
    {
        return new ItemCleanoutTableCopyT(a_reader);
    }

    private object CreateItemCleanoutTableDeleteT(IReader a_reader)
    {
        return new ItemCleanoutTableDeleteT(a_reader);
    }

    private object CreateItemCleanoutTableNewT(IReader a_reader)
    {
        return new ItemCleanoutTableNewT(a_reader);
    }

    private object CreateItemCleanoutTableUpdateT(IReader a_reader)
    {
        return new ItemCleanoutTableUpdateT(a_reader);
    }

    private object CreateResourceConnectorsT(IReader a_reader)
    {
        return new ResourceConnectorsT(a_reader);
    }

    private object CreateResourceConnectorsNewT(IReader a_reader)
    {
        return new ResourceConnectorsDefaultT(a_reader);
    }

    private object CreateResourceConnectorsCopyT(IReader a_reader)
    {
        return new ResourceConnectorsCopyT(a_reader);
    }

    private object CreateResourceConnectorsDeleteT(IReader a_reader)
    {
        return new ResourceConnectorsDeleteT(a_reader);
    }

    private object CreateResourceConnectorsDeleteAllT(IReader a_reader)
    {
        return new ResourceConnectorsDeleteAllT(a_reader);
    }
    private object CreateCompatibilityCodeTableBaseT(IReader a_reader)
    {
        return new CompatibilityCodeTableBaseT(a_reader);
    }
    private object CreateCompatibilityCodeTableNewT(IReader a_reader)
    {
        return new CompatibilityCodeTableNewT(a_reader);
    }

    private object CreateCompatibilityCodeTableCopyT(IReader a_reader)
    {
        return new CompatibilityCodeTableCopyT(a_reader);
    }

    private object CreateCompatibilityCodeTableDeleteT(IReader a_reader)
    {
        return new CompatibilityCodeTableDeleteT(a_reader);
    }
    private object CreateCompatibilityCodeTableUpdateT(IReader a_reader)
    {
        return new CompatibilityCodeTableUpdateT(a_reader);
    }

     private object CreateCompatibilityCodeTableT(IReader a_reader)
    {
        return new CompatibilityCodeTableT(a_reader);
    }

    object CreateUserFieldT(IReader a_reader)
    {
        return new UserFieldDefinitionT(a_reader);
    }
    
    object CreateRefreshStagingDataStartedT(IReader a_reader)
    {
        return new RefreshStagingDataStartedT(a_reader);
    }
    
    object CreateRefreshStagingDataCompletedT(IReader a_reader)
    {
        return new RefreshStagingDataCompletedT(a_reader);
    }

    #endregion ERP Transmissions
    #endregion
    #endregion
}

// **********************************************************************************************************************************************************************************************
// 2010.11.05: Only SEC ever attempted to use this transmission that was replaced before the feature it was for was completed.
// You should be able to delete this commented out code shortly after they're live and all instances of this have vanished from their undo sets.
// SEC should be going live around 2011.01.01
// Watch out, some other tranmission will eventually end up using id 707.
// **********************************************************************************************************************************************************************************************
//namespace PT.Transmissions
//{
//    /// <summary>
//    /// Summary description for ScenarioDetailResizeBlockT.
//    /// </summary>
//    public class ScenarioDetailResizeBlockT : ScenarioIdBaseT, PT.Common.IPTSerializable
//    {

//        #region IPTSerializable Members
//        public new const int UNIQUE_ID = 707;

//        public ScenarioDetailResizeBlockT(PT.Common.IReader reader)
//            : base(reader)
//        {
//            if (reader.VersionNumber >= 339)
//            {
//                _blockKey = new SchedulerDefinitions.BlockKey(reader);
//                reader.Read(out _newStartOrEnd);
//                int val;
//                reader.Read(out val);
//                _resizeType = (resizeTypes)val;
//            }
//        }

//        public override void Serialize(PT.Common.IWriter writer)
//        {
//            base.Serialize(writer);

//            _blockKey.Serialize(writer);
//            writer.Write(_newStartOrEnd);
//            writer.Write((int)_resizeType);
//        }

//        public override int UniqueId
//        {
//            get { return UNIQUE_ID; }
//        }

//        #endregion

//        public ScenarioDetailResizeBlockT(PT.Scheduler.BaseId aScenarioId, PT.SchedulerDefinitions.BlockKey aBlockKey, DateTime aNewStartOrEnd, resizeTypes aResizeType)
//            : base(aScenarioId)
//        {
//            _blockKey = aBlockKey;
//            _newStartOrEnd = DateTimeHelper.RoundSeconds(aNewStartOrEnd);
//            _resizeType = aResizeType;
//        }

//        public enum resizeTypes { SetStartExactCycle, SetEndExactCycle, SetStartExtendToEndOfCapacityIntervalOrFirstBlockEncountered, SetEndExtendToEndOfCapacityIntervalOrFirstBlockEncountered };
//        resizeTypes _resizeType;
//        public resizeTypes ResizeType
//        {
//            get { return _resizeType; }
//        }

//        PT.SchedulerDefinitions.BlockKey _blockKey;
//        public PT.SchedulerDefinitions.BlockKey BlockKey
//        {
//            get { return _blockKey; }
//        }

//        DateTime _newStartOrEnd;
//        /// <summary>
//        /// Setting the start date through this property causes it to be round to the nearest second.
//        /// </summary>
//        public DateTime NewStartOrEnd
//        {
//            get
//            {
//                return _newStartOrEnd;
//            }
//        }

//        /// <summary>
//        /// Specifies the name of the type of object in whose history this Transmission should show.
//        /// </summary>
//        public override string AffectedObjectTypeName
//        {
//            get { return "Scenario"; }
//        }

//    }
//}