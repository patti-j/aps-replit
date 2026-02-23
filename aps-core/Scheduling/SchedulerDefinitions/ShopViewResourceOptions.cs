using PT.APSCommon;

namespace PT.SchedulerDefinitions;

/// <summary>
/// A set of options that can be assigned to one or more Resources to control how Shop Views work when
/// displaying this Resource.
/// </summary>
public class ShopViewResourceOptions : ICloneable
{
    #region IPTSerializable
    public const int UNIQUE_ID = 498;

    public ShopViewResourceOptions(IReader reader)
    {
        if (reader.VersionNumber >= 706)
        {
            id = new BaseId(reader);
            reader.Read(out name);
            reader.Read(out GridDisplayTextPrefix);

            int val;
            reader.Read(out val);
            NotReadyActivityTreatment = (objectState)val;
            reader.Read(out val);
            NonFirstActivityTreatment = (objectState)val;
            reader.Read(out val);
            PastHeadstartActivityTreatment = (objectState)val;

            reader.Read(out EnforceCapacityType);
            reader.Read(out EnforceQty);
            reader.Read(out MaxActivityCount);
            reader.Read(out MaxHorizon);
            reader.Read(out AutoCalculateReportedHours);

            FieldBlockStart = new FieldInfo(reader);
            FieldBlockEnd = new FieldInfo(reader);
            FieldJobExternalId = new FieldInfo(reader);
            FieldManufacturingOrderExternalId = new FieldInfo(reader);
            FieldOperationExternalId = new FieldInfo(reader);
            FieldActivityExternalId = new FieldInfo(reader);
            FieldJobName = new FieldInfo(reader);
            FieldManufacturingOrderName = new FieldInfo(reader);
            FieldOperationName = new FieldInfo(reader);
            FieldJobDescription = new FieldInfo(reader);
            FieldManufacturingOrderDescription = new FieldInfo(reader);
            FieldOperationDescription = new FieldInfo(reader);
            FieldProduct = new FieldInfo(reader);
            FieldProductDescription = new FieldInfo(reader);
            FieldProductionStatus = new FieldInfo(reader);
            FieldActivityPercentFinished = new FieldInfo(reader);
            FieldJobNeedDate = new FieldInfo(reader);
            FieldManufacturingOrderNeedDate = new FieldInfo(reader);
            FieldOperationNeedDate = new FieldInfo(reader);
            FieldSetupHours = new FieldInfo(reader);
            FieldRunHours = new FieldInfo(reader);
            FieldPostProcessingHours = new FieldInfo(reader);
            FieldTotalHours = new FieldInfo(reader);
            FieldReportedSetupHours = new FieldInfo(reader);
            FieldReportedRunHours = new FieldInfo(reader);
            FieldReportedPostProcessingHours = new FieldInfo(reader);
            FieldResourcesUsed = new FieldInfo(reader);
            FieldSetupCode = new FieldInfo(reader);
            FieldSetupNumber = new FieldInfo(reader);
            FieldSetupColorA = new FieldInfo(reader);
            FieldSetupColorR = new FieldInfo(reader);
            FieldSetupColorG = new FieldInfo(reader);
            FieldSetupColorB = new FieldInfo(reader);
            FieldCustomer = new FieldInfo(reader);
            FieldOrderNumber = new FieldInfo(reader);
            FieldPriority = new FieldInfo(reader);
            FieldOperationNotes = new FieldInfo(reader);
            FieldJobNotes = new FieldInfo(reader);
            FieldRequiredFinishQty = new FieldInfo(reader);
            FieldExpectedScrapQty = new FieldInfo(reader);
            FieldReportedGoodQty = new FieldInfo(reader);
            FieldReportedScrapQty = new FieldInfo(reader);
            FieldLatestConstraintDate = new FieldInfo(reader);
            FieldLatestConstraint = new FieldInfo(reader);
            FieldNextOperationName = new FieldInfo(reader);
            FieldNextOperationDescription = new FieldInfo(reader);
            FieldNextOperationResources = new FieldInfo(reader);
            FieldNextOperationScheduledStart = new FieldInfo(reader);
            FieldActivityComments = new FieldInfo(reader);
            FieldReleased = new FieldInfo(reader);
            FieldReleaseDate = new FieldInfo(reader);
            FieldCommitment = new FieldInfo(reader);
            FieldJobType = new FieldInfo(reader);
            FieldActivityIsLate = new FieldInfo(reader);
            FieldUOM = new FieldInfo(reader);
            FieldPaused = new FieldInfo(reader);
            FieldJobId = new FieldInfo(reader);
            FieldMoId = new FieldInfo(reader);
            FieldOpId = new FieldInfo(reader);
            FieldActivityId = new FieldInfo(reader);
            FieldBlockId = new FieldInfo(reader);
            FieldOnHold = new FieldInfo(reader);
            FieldHoldReason = new FieldInfo(reader);
            FieldHoldUntilDate = new FieldInfo(reader);
            FieldActivityStart = new FieldInfo(reader);
            FieldActivityEnd = new FieldInfo(reader);
            FieldSetupColor = new FieldInfo(reader);
            FieldReadOnlyReason = new FieldInfo(reader);
            FieldAttentionPercent = new FieldInfo(reader);
            FieldReportedEndDate = new FieldInfo(reader);
            FieldCurrentBufferPenetrationPercent = new FieldInfo(reader);
            FieldProjectedBufferPenetrationPercent = new FieldInfo(reader);
            FieldHot = new FieldInfo(reader);

            //Material Fields
            FieldMaterialName = new FieldInfo(reader);
            FieldMaterialDescription = new FieldInfo(reader);
            FieldMaterialAvailable = new FieldInfo(reader);
            FieldMaterialAvailableDate = new FieldInfo(reader);
            FieldMaterialIssued = new FieldInfo(reader);
            FieldMaterialIssuedQty = new FieldInfo(reader);
            FieldMaterialTotalRequiredQty = new FieldInfo(reader);
            FieldMaterialUOM = new FieldInfo(reader);
            FieldMaterialQtyToIssue = new FieldInfo(reader);
            FieldMaterialIssueFromWarehouse = new FieldInfo(reader);

            //UserFields
            reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                UserFieldInfos.Add(new FieldInfo(reader));
            }

            //Subassembly Fields

            //Buttons
            ButtonSetup = new ButtonInfo(reader);
            ButtonRun = new ButtonInfo(reader);
            ButtonPostProcess = new ButtonInfo(reader);
            ButtonFinished = new ButtonInfo(reader);
            ButtonPaused = new ButtonInfo(reader);
            ButtonOnhold = new ButtonInfo(reader);
            ButtonReassign = new ButtonInfo(reader);
            ButtonCustom = new ButtonInfo(reader);
            ButtonSave = new ButtonInfo(reader);
            ButtonUndo = new ButtonInfo(reader);

            //Tabs
            TabInput = new TabInfo(reader);
            TabFlow = new TabInfo(reader);
            TabProcess = new TabInfo(reader);
            TabComments = new TabInfo(reader);
            TabMaterials = new TabInfo(reader);
            TabJobNotes = new TabInfo(reader);

            //Group Boxes
            GroupGeneral = new GroupBoxInfo(reader);
            GroupInputQuantities = new GroupBoxInfo(reader);
            GroupInputTime = new GroupBoxInfo(reader);
            GroupFlowConstraint = new GroupBoxInfo(reader);
            GroupFlowNextOperation = new GroupBoxInfo(reader);
            GroupProcessSetup = new GroupBoxInfo(reader);

            //Labels
            LabelJob = new LabelInfo(reader);
            LabelProduct = new LabelInfo(reader);
            LabelOperation = new LabelInfo(reader);
            LabelName = new LabelInfo(reader);
            LabelDescription = new LabelInfo(reader);
            LabelNeedDate = new LabelInfo(reader);
            LabelActivity = new LabelInfo(reader);
            LabelInputReported = new LabelInfo(reader);
            LabelInputExpected = new LabelInfo(reader);
            LabelInputGoodQty = new LabelInfo(reader);
            LabelInputScrapQty = new LabelInfo(reader);
            LabelInputSetup = new LabelInfo(reader);
            LabelInputRun = new LabelInfo(reader);
            LabelInputPostProcess = new LabelInfo(reader);
        }
        else if (reader.VersionNumber >= 705)
        {
            id = new BaseId(reader);
            reader.Read(out name);
            reader.Read(out GridDisplayTextPrefix);

            int val;
            reader.Read(out val);
            NotReadyActivityTreatment = (objectState)val;
            reader.Read(out val);
            NonFirstActivityTreatment = (objectState)val;
            reader.Read(out val);
            PastHeadstartActivityTreatment = (objectState)val;

            reader.Read(out EnforceCapacityType);
            reader.Read(out EnforceQty);
            reader.Read(out MaxActivityCount);
            reader.Read(out MaxHorizon);
            reader.Read(out AutoCalculateReportedHours);

            FieldBlockStart = new FieldInfo(reader);
            FieldBlockEnd = new FieldInfo(reader);
            FieldJobExternalId = new FieldInfo(reader);
            FieldManufacturingOrderExternalId = new FieldInfo(reader);
            FieldOperationExternalId = new FieldInfo(reader);
            FieldActivityExternalId = new FieldInfo(reader);
            FieldJobName = new FieldInfo(reader);
            FieldManufacturingOrderName = new FieldInfo(reader);
            FieldOperationName = new FieldInfo(reader);
            FieldJobDescription = new FieldInfo(reader);
            FieldManufacturingOrderDescription = new FieldInfo(reader);
            FieldOperationDescription = new FieldInfo(reader);
            FieldProduct = new FieldInfo(reader);
            FieldProductDescription = new FieldInfo(reader);
            FieldProductionStatus = new FieldInfo(reader);
            FieldActivityPercentFinished = new FieldInfo(reader);
            FieldJobNeedDate = new FieldInfo(reader);
            FieldManufacturingOrderNeedDate = new FieldInfo(reader);
            FieldOperationNeedDate = new FieldInfo(reader);
            FieldSetupHours = new FieldInfo(reader);
            FieldRunHours = new FieldInfo(reader);
            FieldPostProcessingHours = new FieldInfo(reader);
            FieldTotalHours = new FieldInfo(reader);
            FieldReportedSetupHours = new FieldInfo(reader);
            FieldReportedRunHours = new FieldInfo(reader);
            FieldReportedPostProcessingHours = new FieldInfo(reader);
            FieldResourcesUsed = new FieldInfo(reader);
            FieldSetupCode = new FieldInfo(reader);
            FieldSetupNumber = new FieldInfo(reader);
            FieldSetupColorA = new FieldInfo(reader);
            FieldSetupColorR = new FieldInfo(reader);
            FieldSetupColorG = new FieldInfo(reader);
            FieldSetupColorB = new FieldInfo(reader);
            FieldCustomer = new FieldInfo(reader);
            FieldOrderNumber = new FieldInfo(reader);
            FieldPriority = new FieldInfo(reader);
            FieldOperationNotes = new FieldInfo(reader);
            FieldJobNotes = new FieldInfo(reader);
            FieldRequiredFinishQty = new FieldInfo(reader);
            FieldExpectedScrapQty = new FieldInfo(reader);
            FieldReportedGoodQty = new FieldInfo(reader);
            FieldReportedScrapQty = new FieldInfo(reader);
            FieldLatestConstraintDate = new FieldInfo(reader);
            FieldLatestConstraint = new FieldInfo(reader);
            FieldNextOperationName = new FieldInfo(reader);
            FieldNextOperationDescription = new FieldInfo(reader);
            FieldNextOperationResources = new FieldInfo(reader);
            FieldNextOperationScheduledStart = new FieldInfo(reader);
            FieldActivityComments = new FieldInfo(reader);
            FieldReleased = new FieldInfo(reader);
            FieldReleaseDate = new FieldInfo(reader);
            FieldCommitment = new FieldInfo(reader);
            FieldJobType = new FieldInfo(reader);
            FieldActivityIsLate = new FieldInfo(reader);
            FieldUOM = new FieldInfo(reader);
            FieldPaused = new FieldInfo(reader);
            FieldJobId = new FieldInfo(reader);
            FieldMoId = new FieldInfo(reader);
            FieldOpId = new FieldInfo(reader);
            FieldActivityId = new FieldInfo(reader);
            FieldBlockId = new FieldInfo(reader);
            FieldOnHold = new FieldInfo(reader);
            FieldHoldReason = new FieldInfo(reader);
            FieldHoldUntilDate = new FieldInfo(reader);
            FieldActivityStart = new FieldInfo(reader);
            FieldActivityEnd = new FieldInfo(reader);
            FieldSetupColor = new FieldInfo(reader);
            FieldReadOnlyReason = new FieldInfo(reader);
            FieldAttentionPercent = new FieldInfo(reader);
            FieldReportedEndDate = new FieldInfo(reader);
            FieldCurrentBufferPenetrationPercent = new FieldInfo(reader);
            FieldProjectedBufferPenetrationPercent = new FieldInfo(reader);

            //Material Fields
            FieldMaterialName = new FieldInfo(reader);
            FieldMaterialDescription = new FieldInfo(reader);
            FieldMaterialAvailable = new FieldInfo(reader);
            FieldMaterialAvailableDate = new FieldInfo(reader);
            FieldMaterialIssued = new FieldInfo(reader);
            FieldMaterialIssuedQty = new FieldInfo(reader);
            FieldMaterialTotalRequiredQty = new FieldInfo(reader);
            FieldMaterialUOM = new FieldInfo(reader);
            FieldMaterialQtyToIssue = new FieldInfo(reader);
            FieldMaterialIssueFromWarehouse = new FieldInfo(reader);

            //UserFields
            reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                UserFieldInfos.Add(new FieldInfo(reader));
            }

            //Subassembly Fields

            //Buttons
            ButtonSetup = new ButtonInfo(reader);
            ButtonRun = new ButtonInfo(reader);
            ButtonPostProcess = new ButtonInfo(reader);
            ButtonFinished = new ButtonInfo(reader);
            ButtonPaused = new ButtonInfo(reader);
            ButtonOnhold = new ButtonInfo(reader);
            ButtonReassign = new ButtonInfo(reader);
            ButtonCustom = new ButtonInfo(reader);
            ButtonSave = new ButtonInfo(reader);
            ButtonUndo = new ButtonInfo(reader);

            //Tabs
            TabInput = new TabInfo(reader);
            TabFlow = new TabInfo(reader);
            TabProcess = new TabInfo(reader);
            TabComments = new TabInfo(reader);
            TabMaterials = new TabInfo(reader);
            TabJobNotes = new TabInfo(reader);

            //Group Boxes
            GroupGeneral = new GroupBoxInfo(reader);
            GroupInputQuantities = new GroupBoxInfo(reader);
            GroupInputTime = new GroupBoxInfo(reader);
            GroupFlowConstraint = new GroupBoxInfo(reader);
            GroupFlowNextOperation = new GroupBoxInfo(reader);
            GroupProcessSetup = new GroupBoxInfo(reader);

            //Labels
            LabelJob = new LabelInfo(reader);
            LabelProduct = new LabelInfo(reader);
            LabelOperation = new LabelInfo(reader);
            LabelName = new LabelInfo(reader);
            LabelDescription = new LabelInfo(reader);
            LabelNeedDate = new LabelInfo(reader);
            LabelActivity = new LabelInfo(reader);
            LabelInputReported = new LabelInfo(reader);
            LabelInputExpected = new LabelInfo(reader);
            LabelInputGoodQty = new LabelInfo(reader);
            LabelInputScrapQty = new LabelInfo(reader);
            LabelInputSetup = new LabelInfo(reader);
            LabelInputRun = new LabelInfo(reader);
            LabelInputPostProcess = new LabelInfo(reader);
        }
        else if (reader.VersionNumber >= 444)
        {
            id = new BaseId(reader);
            reader.Read(out name);
            reader.Read(out GridDisplayTextPrefix);

            int val;
            reader.Read(out val);
            NotReadyActivityTreatment = (objectState)val;
            reader.Read(out val);
            NonFirstActivityTreatment = (objectState)val;
            reader.Read(out val);
            PastHeadstartActivityTreatment = (objectState)val;

            reader.Read(out EnforceCapacityType);
            reader.Read(out EnforceQty);
            reader.Read(out MaxActivityCount);
            reader.Read(out MaxHorizon);
            reader.Read(out AutoCalculateReportedHours);

            FieldBlockStart = new FieldInfo(reader);
            FieldBlockEnd = new FieldInfo(reader);
            FieldJobExternalId = new FieldInfo(reader);
            FieldManufacturingOrderExternalId = new FieldInfo(reader);
            FieldOperationExternalId = new FieldInfo(reader);
            FieldActivityExternalId = new FieldInfo(reader);
            FieldJobName = new FieldInfo(reader);
            FieldManufacturingOrderName = new FieldInfo(reader);
            FieldOperationName = new FieldInfo(reader);
            FieldJobDescription = new FieldInfo(reader);
            FieldManufacturingOrderDescription = new FieldInfo(reader);
            FieldOperationDescription = new FieldInfo(reader);
            FieldProduct = new FieldInfo(reader);
            FieldProductDescription = new FieldInfo(reader);
            FieldProductionStatus = new FieldInfo(reader);
            FieldActivityPercentFinished = new FieldInfo(reader);
            FieldJobNeedDate = new FieldInfo(reader);
            FieldManufacturingOrderNeedDate = new FieldInfo(reader);
            FieldOperationNeedDate = new FieldInfo(reader);
            FieldSetupHours = new FieldInfo(reader);
            FieldRunHours = new FieldInfo(reader);
            FieldPostProcessingHours = new FieldInfo(reader);
            FieldTotalHours = new FieldInfo(reader);
            FieldReportedSetupHours = new FieldInfo(reader);
            FieldReportedRunHours = new FieldInfo(reader);
            FieldReportedPostProcessingHours = new FieldInfo(reader);
            FieldResourcesUsed = new FieldInfo(reader);
            FieldSetupCode = new FieldInfo(reader);
            FieldSetupNumber = new FieldInfo(reader);
            FieldSetupColorA = new FieldInfo(reader);
            FieldSetupColorR = new FieldInfo(reader);
            FieldSetupColorG = new FieldInfo(reader);
            FieldSetupColorB = new FieldInfo(reader);
            FieldCustomer = new FieldInfo(reader);
            FieldOrderNumber = new FieldInfo(reader);
            FieldPriority = new FieldInfo(reader);
            FieldOperationNotes = new FieldInfo(reader);
            FieldJobNotes = new FieldInfo(reader);
            FieldRequiredFinishQty = new FieldInfo(reader);
            FieldExpectedScrapQty = new FieldInfo(reader);
            FieldReportedGoodQty = new FieldInfo(reader);
            FieldReportedScrapQty = new FieldInfo(reader);
            FieldLatestConstraintDate = new FieldInfo(reader);
            FieldLatestConstraint = new FieldInfo(reader);
            FieldNextOperationName = new FieldInfo(reader);
            FieldNextOperationDescription = new FieldInfo(reader);
            FieldNextOperationResources = new FieldInfo(reader);
            FieldNextOperationScheduledStart = new FieldInfo(reader);
            FieldActivityComments = new FieldInfo(reader);
            FieldReleased = new FieldInfo(reader);
            FieldReleaseDate = new FieldInfo(reader);
            FieldCommitment = new FieldInfo(reader);
            FieldJobType = new FieldInfo(reader);
            FieldActivityIsLate = new FieldInfo(reader);
            FieldUOM = new FieldInfo(reader);
            FieldPaused = new FieldInfo(reader);
            FieldJobId = new FieldInfo(reader);
            FieldMoId = new FieldInfo(reader);
            FieldOpId = new FieldInfo(reader);
            FieldActivityId = new FieldInfo(reader);
            FieldBlockId = new FieldInfo(reader);
            FieldOnHold = new FieldInfo(reader);
            FieldHoldReason = new FieldInfo(reader);
            FieldHoldUntilDate = new FieldInfo(reader);
            FieldActivityStart = new FieldInfo(reader);
            FieldActivityEnd = new FieldInfo(reader);
            FieldSetupColor = new FieldInfo(reader);
            FieldReadOnlyReason = new FieldInfo(reader);
            FieldAttentionPercent = new FieldInfo(reader);
            FieldReportedEndDate = new FieldInfo(reader);
            FieldCurrentBufferPenetrationPercent = new FieldInfo(reader);
            FieldProjectedBufferPenetrationPercent = new FieldInfo(reader);

            //Material Fields
            FieldMaterialName = new FieldInfo(reader);
            FieldMaterialDescription = new FieldInfo(reader);
            FieldMaterialAvailable = new FieldInfo(reader);
            FieldMaterialAvailableDate = new FieldInfo(reader);
            FieldMaterialIssued = new FieldInfo(reader);
            FieldMaterialIssuedQty = new FieldInfo(reader);
            FieldMaterialTotalRequiredQty = new FieldInfo(reader);
            FieldMaterialUOM = new FieldInfo(reader);
            FieldMaterialQtyToIssue = new FieldInfo(reader);
            FieldMaterialIssueFromWarehouse = new FieldInfo(reader);


            //Subassembly Fields

            //Buttons
            ButtonSetup = new ButtonInfo(reader);
            ButtonRun = new ButtonInfo(reader);
            ButtonPostProcess = new ButtonInfo(reader);
            ButtonFinished = new ButtonInfo(reader);
            ButtonPaused = new ButtonInfo(reader);
            ButtonOnhold = new ButtonInfo(reader);
            ButtonReassign = new ButtonInfo(reader);
            ButtonCustom = new ButtonInfo(reader);
            ButtonSave = new ButtonInfo(reader);
            ButtonUndo = new ButtonInfo(reader);

            //Tabs
            TabInput = new TabInfo(reader);
            TabFlow = new TabInfo(reader);
            TabProcess = new TabInfo(reader);
            TabComments = new TabInfo(reader);
            TabMaterials = new TabInfo(reader);
            TabJobNotes = new TabInfo(reader);

            //Group Boxes
            GroupGeneral = new GroupBoxInfo(reader);
            GroupInputQuantities = new GroupBoxInfo(reader);
            GroupInputTime = new GroupBoxInfo(reader);
            GroupFlowConstraint = new GroupBoxInfo(reader);
            GroupFlowNextOperation = new GroupBoxInfo(reader);
            GroupProcessSetup = new GroupBoxInfo(reader);

            //Labels
            LabelJob = new LabelInfo(reader);
            LabelProduct = new LabelInfo(reader);
            LabelOperation = new LabelInfo(reader);
            LabelName = new LabelInfo(reader);
            LabelDescription = new LabelInfo(reader);
            LabelNeedDate = new LabelInfo(reader);
            LabelActivity = new LabelInfo(reader);
            LabelInputReported = new LabelInfo(reader);
            LabelInputExpected = new LabelInfo(reader);
            LabelInputGoodQty = new LabelInfo(reader);
            LabelInputScrapQty = new LabelInfo(reader);
            LabelInputSetup = new LabelInfo(reader);
            LabelInputRun = new LabelInfo(reader);
            LabelInputPostProcess = new LabelInfo(reader);
        }

        #region Version 414
        else if (reader.VersionNumber >= 414)
        {
            id = new BaseId(reader);
            reader.Read(out name);
            reader.Read(out GridDisplayTextPrefix);

            int val;
            reader.Read(out val);
            NotReadyActivityTreatment = (objectState)val;
            reader.Read(out val);
            NonFirstActivityTreatment = (objectState)val;
            reader.Read(out val);
            PastHeadstartActivityTreatment = (objectState)val;

            reader.Read(out EnforceCapacityType);
            reader.Read(out EnforceQty);
            reader.Read(out MaxActivityCount);
            reader.Read(out MaxHorizon);
            reader.Read(out AutoCalculateReportedHours);

            FieldBlockStart = new FieldInfo(reader);
            FieldBlockEnd = new FieldInfo(reader);
            FieldJobExternalId = new FieldInfo(reader);
            FieldManufacturingOrderExternalId = new FieldInfo(reader);
            FieldOperationExternalId = new FieldInfo(reader);
            FieldActivityExternalId = new FieldInfo(reader);
            FieldJobName = new FieldInfo(reader);
            FieldManufacturingOrderName = new FieldInfo(reader);
            FieldOperationName = new FieldInfo(reader);
            FieldJobDescription = new FieldInfo(reader);
            FieldManufacturingOrderDescription = new FieldInfo(reader);
            FieldOperationDescription = new FieldInfo(reader);
            FieldProduct = new FieldInfo(reader);
            FieldProductDescription = new FieldInfo(reader);
            FieldProductionStatus = new FieldInfo(reader);
            FieldActivityPercentFinished = new FieldInfo(reader);
            FieldJobNeedDate = new FieldInfo(reader);
            FieldManufacturingOrderNeedDate = new FieldInfo(reader);
            FieldOperationNeedDate = new FieldInfo(reader);
            FieldSetupHours = new FieldInfo(reader);
            FieldRunHours = new FieldInfo(reader);
            FieldPostProcessingHours = new FieldInfo(reader);
            FieldTotalHours = new FieldInfo(reader);
            FieldReportedSetupHours = new FieldInfo(reader);
            FieldReportedRunHours = new FieldInfo(reader);
            FieldReportedPostProcessingHours = new FieldInfo(reader);
            FieldResourcesUsed = new FieldInfo(reader);
            FieldSetupCode = new FieldInfo(reader);
            FieldSetupNumber = new FieldInfo(reader);
            FieldSetupColorA = new FieldInfo(reader);
            FieldSetupColorR = new FieldInfo(reader);
            FieldSetupColorG = new FieldInfo(reader);
            FieldSetupColorB = new FieldInfo(reader);
            FieldCustomer = new FieldInfo(reader);
            FieldOrderNumber = new FieldInfo(reader);
            FieldPriority = new FieldInfo(reader);
            FieldOperationNotes = new FieldInfo(reader);
            FieldJobNotes = new FieldInfo(reader);
            FieldRequiredFinishQty = new FieldInfo(reader);
            FieldExpectedScrapQty = new FieldInfo(reader);
            FieldReportedGoodQty = new FieldInfo(reader);
            FieldReportedScrapQty = new FieldInfo(reader);
            FieldLatestConstraintDate = new FieldInfo(reader);
            FieldLatestConstraint = new FieldInfo(reader);
            FieldNextOperationName = new FieldInfo(reader);
            FieldNextOperationDescription = new FieldInfo(reader);
            FieldNextOperationResources = new FieldInfo(reader);
            FieldNextOperationScheduledStart = new FieldInfo(reader);
            FieldActivityComments = new FieldInfo(reader);
            FieldReleased = new FieldInfo(reader);
            FieldReleaseDate = new FieldInfo(reader);
            FieldCommitment = new FieldInfo(reader);
            FieldJobType = new FieldInfo(reader);
            FieldActivityIsLate = new FieldInfo(reader);
            FieldUOM = new FieldInfo(reader);
            FieldPaused = new FieldInfo(reader);
            FieldJobId = new FieldInfo(reader);
            FieldMoId = new FieldInfo(reader);
            FieldOpId = new FieldInfo(reader);
            FieldActivityId = new FieldInfo(reader);
            FieldBlockId = new FieldInfo(reader);
            FieldOnHold = new FieldInfo(reader);
            FieldHoldReason = new FieldInfo(reader);
            FieldHoldUntilDate = new FieldInfo(reader);
            FieldActivityStart = new FieldInfo(reader);
            FieldActivityEnd = new FieldInfo(reader);
            FieldSetupColor = new FieldInfo(reader);
            FieldReadOnlyReason = new FieldInfo(reader);
            FieldAttentionPercent = new FieldInfo(reader);
            FieldReportedEndDate = new FieldInfo(reader);

            //Material Fields
            FieldMaterialName = new FieldInfo(reader);
            FieldMaterialDescription = new FieldInfo(reader);
            FieldMaterialAvailable = new FieldInfo(reader);
            FieldMaterialAvailableDate = new FieldInfo(reader);
            FieldMaterialIssued = new FieldInfo(reader);
            FieldMaterialIssuedQty = new FieldInfo(reader);
            FieldMaterialTotalRequiredQty = new FieldInfo(reader);
            FieldMaterialUOM = new FieldInfo(reader);
            FieldMaterialQtyToIssue = new FieldInfo(reader);
            FieldMaterialIssueFromWarehouse = new FieldInfo(reader);


            //Subassembly Fields

            //Buttons
            ButtonSetup = new ButtonInfo(reader);
            ButtonRun = new ButtonInfo(reader);
            ButtonPostProcess = new ButtonInfo(reader);
            ButtonFinished = new ButtonInfo(reader);
            ButtonPaused = new ButtonInfo(reader);
            ButtonOnhold = new ButtonInfo(reader);
            ButtonReassign = new ButtonInfo(reader);
            ButtonCustom = new ButtonInfo(reader);
            ButtonSave = new ButtonInfo(reader);
            ButtonUndo = new ButtonInfo(reader);

            //Tabs
            TabInput = new TabInfo(reader);
            TabFlow = new TabInfo(reader);
            TabProcess = new TabInfo(reader);
            TabComments = new TabInfo(reader);
            TabMaterials = new TabInfo(reader);
            TabJobNotes = new TabInfo(reader);

            //Group Boxes
            GroupGeneral = new GroupBoxInfo(reader);
            GroupInputQuantities = new GroupBoxInfo(reader);
            GroupInputTime = new GroupBoxInfo(reader);
            GroupFlowConstraint = new GroupBoxInfo(reader);
            GroupFlowNextOperation = new GroupBoxInfo(reader);
            GroupProcessSetup = new GroupBoxInfo(reader);

            //Labels
            LabelJob = new LabelInfo(reader);
            LabelProduct = new LabelInfo(reader);
            LabelOperation = new LabelInfo(reader);
            LabelName = new LabelInfo(reader);
            LabelDescription = new LabelInfo(reader);
            LabelNeedDate = new LabelInfo(reader);
            LabelActivity = new LabelInfo(reader);
            LabelInputReported = new LabelInfo(reader);
            LabelInputExpected = new LabelInfo(reader);
            LabelInputGoodQty = new LabelInfo(reader);
            LabelInputScrapQty = new LabelInfo(reader);
            LabelInputSetup = new LabelInfo(reader);
            LabelInputRun = new LabelInfo(reader);
            LabelInputPostProcess = new LabelInfo(reader);
        }
        #endregion

        #region Version 311
        else if (reader.VersionNumber >= 311)
        {
            id = new BaseId(reader);
            reader.Read(out name);
            reader.Read(out GridDisplayTextPrefix);

            int val;
            reader.Read(out val);
            NotReadyActivityTreatment = (objectState)val;
            reader.Read(out val);
            NonFirstActivityTreatment = (objectState)val;
            reader.Read(out val);
            PastHeadstartActivityTreatment = (objectState)val;

            reader.Read(out EnforceCapacityType);
            reader.Read(out EnforceQty);
            reader.Read(out MaxActivityCount);
            reader.Read(out MaxHorizon);
            reader.Read(out AutoCalculateReportedHours);

            FieldBlockStart = new FieldInfo(reader);
            FieldBlockEnd = new FieldInfo(reader);
            FieldJobExternalId = new FieldInfo(reader);
            FieldManufacturingOrderExternalId = new FieldInfo(reader);
            FieldOperationExternalId = new FieldInfo(reader);
            FieldActivityExternalId = new FieldInfo(reader);
            FieldJobName = new FieldInfo(reader);
            FieldManufacturingOrderName = new FieldInfo(reader);
            FieldOperationName = new FieldInfo(reader);
            FieldJobDescription = new FieldInfo(reader);
            FieldManufacturingOrderDescription = new FieldInfo(reader);
            FieldOperationDescription = new FieldInfo(reader);
            FieldProduct = new FieldInfo(reader);
            FieldProductDescription = new FieldInfo(reader);
            FieldProductionStatus = new FieldInfo(reader);
            FieldActivityPercentFinished = new FieldInfo(reader);
            FieldJobNeedDate = new FieldInfo(reader);
            FieldManufacturingOrderNeedDate = new FieldInfo(reader);
            FieldOperationNeedDate = new FieldInfo(reader);
            FieldSetupHours = new FieldInfo(reader);
            FieldRunHours = new FieldInfo(reader);
            FieldPostProcessingHours = new FieldInfo(reader);
            FieldTotalHours = new FieldInfo(reader);
            FieldReportedSetupHours = new FieldInfo(reader);
            FieldReportedRunHours = new FieldInfo(reader);
            FieldReportedPostProcessingHours = new FieldInfo(reader);
            FieldResourcesUsed = new FieldInfo(reader);
            FieldSetupCode = new FieldInfo(reader);
            FieldSetupNumber = new FieldInfo(reader);
            FieldSetupColorA = new FieldInfo(reader);
            FieldSetupColorR = new FieldInfo(reader);
            FieldSetupColorG = new FieldInfo(reader);
            FieldSetupColorB = new FieldInfo(reader);
            FieldCustomer = new FieldInfo(reader);
            FieldOrderNumber = new FieldInfo(reader);
            FieldPriority = new FieldInfo(reader);
            FieldOperationNotes = new FieldInfo(reader);
            FieldJobNotes = new FieldInfo(reader);
            FieldRequiredFinishQty = new FieldInfo(reader);
            FieldExpectedScrapQty = new FieldInfo(reader);
            FieldReportedGoodQty = new FieldInfo(reader);
            FieldReportedScrapQty = new FieldInfo(reader);
            FieldLatestConstraintDate = new FieldInfo(reader);
            FieldLatestConstraint = new FieldInfo(reader);
            FieldNextOperationName = new FieldInfo(reader);
            FieldNextOperationDescription = new FieldInfo(reader);
            FieldNextOperationResources = new FieldInfo(reader);
            FieldNextOperationScheduledStart = new FieldInfo(reader);
            FieldActivityComments = new FieldInfo(reader);
            FieldReleased = new FieldInfo(reader);
            FieldReleaseDate = new FieldInfo(reader);
            FieldCommitment = new FieldInfo(reader);
            FieldJobType = new FieldInfo(reader);
            FieldActivityIsLate = new FieldInfo(reader);
            FieldUOM = new FieldInfo(reader);
            FieldPaused = new FieldInfo(reader);
            FieldJobId = new FieldInfo(reader);
            FieldMoId = new FieldInfo(reader);
            FieldOpId = new FieldInfo(reader);
            FieldActivityId = new FieldInfo(reader);
            FieldBlockId = new FieldInfo(reader);
            FieldOnHold = new FieldInfo(reader);
            FieldHoldReason = new FieldInfo(reader);
            FieldHoldUntilDate = new FieldInfo(reader);
            FieldActivityStart = new FieldInfo(reader);
            FieldActivityEnd = new FieldInfo(reader);
            FieldSetupColor = new FieldInfo(reader);
            FieldReadOnlyReason = new FieldInfo(reader);
            FieldAttentionPercent = new FieldInfo(reader);

            //Material Fields
            FieldMaterialName = new FieldInfo(reader);
            FieldMaterialDescription = new FieldInfo(reader);
            FieldMaterialAvailable = new FieldInfo(reader);
            FieldMaterialAvailableDate = new FieldInfo(reader);
            FieldMaterialIssued = new FieldInfo(reader);
            FieldMaterialIssuedQty = new FieldInfo(reader);
            FieldMaterialTotalRequiredQty = new FieldInfo(reader);
            FieldMaterialUOM = new FieldInfo(reader);
            FieldMaterialQtyToIssue = new FieldInfo(reader);
            FieldMaterialIssueFromWarehouse = new FieldInfo(reader);


            //Subassembly Fields

            //Buttons
            ButtonSetup = new ButtonInfo(reader);
            ButtonRun = new ButtonInfo(reader);
            ButtonPostProcess = new ButtonInfo(reader);
            ButtonFinished = new ButtonInfo(reader);
            ButtonPaused = new ButtonInfo(reader);
            ButtonOnhold = new ButtonInfo(reader);
            ButtonReassign = new ButtonInfo(reader);
            ButtonCustom = new ButtonInfo(reader);
            ButtonSave = new ButtonInfo(reader);
            ButtonUndo = new ButtonInfo(reader);

            //Tabs
            TabInput = new TabInfo(reader);
            TabFlow = new TabInfo(reader);
            TabProcess = new TabInfo(reader);
            TabComments = new TabInfo(reader);
            TabMaterials = new TabInfo(reader);
            TabJobNotes = new TabInfo(reader);

            //Group Boxes
            GroupGeneral = new GroupBoxInfo(reader);
            GroupInputQuantities = new GroupBoxInfo(reader);
            GroupInputTime = new GroupBoxInfo(reader);
            GroupFlowConstraint = new GroupBoxInfo(reader);
            GroupFlowNextOperation = new GroupBoxInfo(reader);
            GroupProcessSetup = new GroupBoxInfo(reader);

            //Labels
            LabelJob = new LabelInfo(reader);
            LabelProduct = new LabelInfo(reader);
            LabelOperation = new LabelInfo(reader);
            LabelName = new LabelInfo(reader);
            LabelDescription = new LabelInfo(reader);
            LabelNeedDate = new LabelInfo(reader);
            LabelActivity = new LabelInfo(reader);
            LabelInputReported = new LabelInfo(reader);
            LabelInputExpected = new LabelInfo(reader);
            LabelInputGoodQty = new LabelInfo(reader);
            LabelInputScrapQty = new LabelInfo(reader);
            LabelInputSetup = new LabelInfo(reader);
            LabelInputRun = new LabelInfo(reader);
            LabelInputPostProcess = new LabelInfo(reader);
        }
        #endregion 311

        #region Version 134
        else if (reader.VersionNumber >= 134)
        {
            id = new BaseId(reader);
            reader.Read(out name);
            reader.Read(out GridDisplayTextPrefix);

            int val;
            reader.Read(out val);
            NotReadyActivityTreatment = (objectState)val;
            reader.Read(out val);
            NonFirstActivityTreatment = (objectState)val;
            reader.Read(out val);
            PastHeadstartActivityTreatment = (objectState)val;

            reader.Read(out EnforceCapacityType);
            reader.Read(out EnforceQty);
            reader.Read(out MaxActivityCount);
            reader.Read(out MaxHorizon);
            reader.Read(out AutoCalculateReportedHours);

            FieldBlockStart = new FieldInfo(reader);
            FieldBlockEnd = new FieldInfo(reader);
            FieldJobExternalId = new FieldInfo(reader);
            FieldManufacturingOrderExternalId = new FieldInfo(reader);
            FieldOperationExternalId = new FieldInfo(reader);
            FieldActivityExternalId = new FieldInfo(reader);
            FieldJobName = new FieldInfo(reader);
            FieldManufacturingOrderName = new FieldInfo(reader);
            FieldOperationName = new FieldInfo(reader);
            FieldJobDescription = new FieldInfo(reader);
            FieldManufacturingOrderDescription = new FieldInfo(reader);
            FieldOperationDescription = new FieldInfo(reader);
            FieldProduct = new FieldInfo(reader);
            FieldProductDescription = new FieldInfo(reader);
            FieldProductionStatus = new FieldInfo(reader);
            FieldActivityPercentFinished = new FieldInfo(reader);
            FieldJobNeedDate = new FieldInfo(reader);
            FieldManufacturingOrderNeedDate = new FieldInfo(reader);
            FieldOperationNeedDate = new FieldInfo(reader);
            FieldSetupHours = new FieldInfo(reader);
            FieldRunHours = new FieldInfo(reader);
            FieldPostProcessingHours = new FieldInfo(reader);
            FieldTotalHours = new FieldInfo(reader);
            FieldReportedSetupHours = new FieldInfo(reader);
            FieldReportedRunHours = new FieldInfo(reader);
            FieldReportedPostProcessingHours = new FieldInfo(reader);
            FieldResourcesUsed = new FieldInfo(reader);
            FieldSetupCode = new FieldInfo(reader);
            FieldSetupNumber = new FieldInfo(reader);
            FieldSetupColorA = new FieldInfo(reader);
            FieldSetupColorR = new FieldInfo(reader);
            FieldSetupColorG = new FieldInfo(reader);
            FieldSetupColorB = new FieldInfo(reader);
            FieldCustomer = new FieldInfo(reader);
            FieldOrderNumber = new FieldInfo(reader);
            FieldPriority = new FieldInfo(reader);
            FieldOperationNotes = new FieldInfo(reader);
            FieldJobNotes = new FieldInfo(reader);
            FieldRequiredFinishQty = new FieldInfo(reader);
            FieldExpectedScrapQty = new FieldInfo(reader);
            FieldReportedGoodQty = new FieldInfo(reader);
            FieldReportedScrapQty = new FieldInfo(reader);
            FieldLatestConstraintDate = new FieldInfo(reader);
            FieldLatestConstraint = new FieldInfo(reader);
            FieldNextOperationName = new FieldInfo(reader);
            FieldNextOperationDescription = new FieldInfo(reader);
            FieldNextOperationResources = new FieldInfo(reader);
            FieldNextOperationScheduledStart = new FieldInfo(reader);
            FieldActivityComments = new FieldInfo(reader);
            FieldReleased = new FieldInfo(reader);
            FieldReleaseDate = new FieldInfo(reader);
            FieldCommitment = new FieldInfo(reader);
            FieldJobType = new FieldInfo(reader);
            FieldActivityIsLate = new FieldInfo(reader);
            FieldUOM = new FieldInfo(reader);
            FieldPaused = new FieldInfo(reader);
            FieldJobId = new FieldInfo(reader);
            FieldMoId = new FieldInfo(reader);
            FieldOpId = new FieldInfo(reader);
            FieldActivityId = new FieldInfo(reader);
            FieldBlockId = new FieldInfo(reader);
            FieldOnHold = new FieldInfo(reader);
            FieldHoldReason = new FieldInfo(reader);
            FieldHoldUntilDate = new FieldInfo(reader);
            FieldActivityStart = new FieldInfo(reader);
            FieldActivityEnd = new FieldInfo(reader);
            FieldSetupColor = new FieldInfo(reader);
            FieldReadOnlyReason = new FieldInfo(reader);
            FieldAttentionPercent = new FieldInfo(reader);

            //Material Fields
            FieldMaterialName = new FieldInfo(reader);
            FieldMaterialDescription = new FieldInfo(reader);
            FieldMaterialAvailable = new FieldInfo(reader);
            FieldMaterialAvailableDate = new FieldInfo(reader);
            FieldMaterialIssued = new FieldInfo(reader);

            //Subassembly Fields

            //Buttons
            ButtonSetup = new ButtonInfo(reader);
            ButtonRun = new ButtonInfo(reader);
            ButtonPostProcess = new ButtonInfo(reader);
            ButtonFinished = new ButtonInfo(reader);
            ButtonPaused = new ButtonInfo(reader);
            ButtonOnhold = new ButtonInfo(reader);
            ButtonReassign = new ButtonInfo(reader);
            ButtonCustom = new ButtonInfo(reader);
            ButtonSave = new ButtonInfo(reader);
            ButtonUndo = new ButtonInfo(reader);

            //Tabs
            TabInput = new TabInfo(reader);
            TabFlow = new TabInfo(reader);
            TabProcess = new TabInfo(reader);
            TabComments = new TabInfo(reader);
            TabMaterials = new TabInfo(reader);
            TabJobNotes = new TabInfo(reader);

            //Group Boxes
            GroupGeneral = new GroupBoxInfo(reader);
            GroupInputQuantities = new GroupBoxInfo(reader);
            GroupInputTime = new GroupBoxInfo(reader);
            GroupFlowConstraint = new GroupBoxInfo(reader);
            GroupFlowNextOperation = new GroupBoxInfo(reader);
            GroupProcessSetup = new GroupBoxInfo(reader);

            //Labels
            LabelJob = new LabelInfo(reader);
            LabelProduct = new LabelInfo(reader);
            LabelOperation = new LabelInfo(reader);
            LabelName = new LabelInfo(reader);
            LabelDescription = new LabelInfo(reader);
            LabelNeedDate = new LabelInfo(reader);
            LabelActivity = new LabelInfo(reader);
            LabelInputReported = new LabelInfo(reader);
            LabelInputExpected = new LabelInfo(reader);
            LabelInputGoodQty = new LabelInfo(reader);
            LabelInputScrapQty = new LabelInfo(reader);
            LabelInputSetup = new LabelInfo(reader);
            LabelInputRun = new LabelInfo(reader);
            LabelInputPostProcess = new LabelInfo(reader);
        }
        #endregion

        #region Version 1
        else if (reader.VersionNumber >= 1)
        {
            id = new BaseId(reader);
            reader.Read(out name);
            reader.Read(out GridDisplayTextPrefix);

            int val;
            reader.Read(out val);
            NotReadyActivityTreatment = (objectState)val;
            reader.Read(out val);
            NonFirstActivityTreatment = (objectState)val;
            reader.Read(out val);
            PastHeadstartActivityTreatment = (objectState)val;

            reader.Read(out EnforceCapacityType);
            reader.Read(out MaxActivityCount);
            reader.Read(out MaxHorizon);
            reader.Read(out AutoCalculateReportedHours);

            FieldBlockStart = new FieldInfo(reader);
            FieldBlockEnd = new FieldInfo(reader);
            FieldJobExternalId = new FieldInfo(reader);
            FieldManufacturingOrderExternalId = new FieldInfo(reader);
            FieldOperationExternalId = new FieldInfo(reader);
            FieldActivityExternalId = new FieldInfo(reader);
            FieldJobName = new FieldInfo(reader);
            FieldManufacturingOrderName = new FieldInfo(reader);
            FieldOperationName = new FieldInfo(reader);
            FieldJobDescription = new FieldInfo(reader);
            FieldManufacturingOrderDescription = new FieldInfo(reader);
            FieldOperationDescription = new FieldInfo(reader);
            FieldProduct = new FieldInfo(reader);
            FieldProductDescription = new FieldInfo(reader);
            FieldProductionStatus = new FieldInfo(reader);
            FieldActivityPercentFinished = new FieldInfo(reader);
            FieldJobNeedDate = new FieldInfo(reader);
            FieldManufacturingOrderNeedDate = new FieldInfo(reader);
            FieldOperationNeedDate = new FieldInfo(reader);
            FieldSetupHours = new FieldInfo(reader);
            FieldRunHours = new FieldInfo(reader);
            FieldPostProcessingHours = new FieldInfo(reader);
            FieldTotalHours = new FieldInfo(reader);
            FieldReportedSetupHours = new FieldInfo(reader);
            FieldReportedRunHours = new FieldInfo(reader);
            FieldReportedPostProcessingHours = new FieldInfo(reader);
            FieldResourcesUsed = new FieldInfo(reader);
            FieldSetupCode = new FieldInfo(reader);
            FieldSetupNumber = new FieldInfo(reader);
            FieldSetupColorA = new FieldInfo(reader);
            FieldSetupColorR = new FieldInfo(reader);
            FieldSetupColorG = new FieldInfo(reader);
            FieldSetupColorB = new FieldInfo(reader);
            FieldCustomer = new FieldInfo(reader);
            FieldOrderNumber = new FieldInfo(reader);
            FieldPriority = new FieldInfo(reader);
            FieldOperationNotes = new FieldInfo(reader);
            FieldJobNotes = new FieldInfo(reader);
            FieldRequiredFinishQty = new FieldInfo(reader);
            FieldExpectedScrapQty = new FieldInfo(reader);
            FieldReportedGoodQty = new FieldInfo(reader);
            FieldReportedScrapQty = new FieldInfo(reader);
            FieldLatestConstraintDate = new FieldInfo(reader);
            FieldLatestConstraint = new FieldInfo(reader);
            FieldNextOperationName = new FieldInfo(reader);
            FieldNextOperationDescription = new FieldInfo(reader);
            FieldNextOperationResources = new FieldInfo(reader);
            FieldNextOperationScheduledStart = new FieldInfo(reader);
            FieldActivityComments = new FieldInfo(reader);
            FieldReleased = new FieldInfo(reader);
            FieldReleaseDate = new FieldInfo(reader);
            FieldCommitment = new FieldInfo(reader);
            FieldJobType = new FieldInfo(reader);
            FieldActivityIsLate = new FieldInfo(reader);
            FieldUOM = new FieldInfo(reader);
            FieldPaused = new FieldInfo(reader);
            FieldJobId = new FieldInfo(reader);
            FieldMoId = new FieldInfo(reader);
            FieldOpId = new FieldInfo(reader);
            FieldActivityId = new FieldInfo(reader);
            FieldBlockId = new FieldInfo(reader);
            FieldOnHold = new FieldInfo(reader);
            FieldHoldReason = new FieldInfo(reader);
            FieldHoldUntilDate = new FieldInfo(reader);
            FieldActivityStart = new FieldInfo(reader);
            FieldActivityEnd = new FieldInfo(reader);
            FieldSetupColor = new FieldInfo(reader);
            FieldReadOnlyReason = new FieldInfo(reader);
            FieldAttentionPercent = new FieldInfo(reader);

            //Material Fields
            FieldMaterialName = new FieldInfo(reader);
            FieldMaterialDescription = new FieldInfo(reader);
            FieldMaterialAvailable = new FieldInfo(reader);
            FieldMaterialAvailableDate = new FieldInfo(reader);
            FieldMaterialIssued = new FieldInfo(reader);

            //Subassembly Fields

            //Buttons
            ButtonSetup = new ButtonInfo(reader);
            ButtonRun = new ButtonInfo(reader);
            ButtonPostProcess = new ButtonInfo(reader);
            ButtonFinished = new ButtonInfo(reader);
            ButtonPaused = new ButtonInfo(reader);
            ButtonOnhold = new ButtonInfo(reader);
            ButtonReassign = new ButtonInfo(reader);
            ButtonCustom = new ButtonInfo(reader);
            ButtonSave = new ButtonInfo(reader);
            ButtonUndo = new ButtonInfo(reader);

            //Tabs
            TabInput = new TabInfo(reader);
            TabFlow = new TabInfo(reader);
            TabProcess = new TabInfo(reader);
            TabComments = new TabInfo(reader);
            TabMaterials = new TabInfo(reader);
            TabJobNotes = new TabInfo(reader);

            //Group Boxes
            GroupGeneral = new GroupBoxInfo(reader);
            GroupInputQuantities = new GroupBoxInfo(reader);
            GroupInputTime = new GroupBoxInfo(reader);
            GroupFlowConstraint = new GroupBoxInfo(reader);
            GroupFlowNextOperation = new GroupBoxInfo(reader);
            GroupProcessSetup = new GroupBoxInfo(reader);

            //Labels
            LabelJob = new LabelInfo(reader);
            LabelProduct = new LabelInfo(reader);
            LabelOperation = new LabelInfo(reader);
            LabelName = new LabelInfo(reader);
            LabelDescription = new LabelInfo(reader);
            LabelNeedDate = new LabelInfo(reader);
            LabelActivity = new LabelInfo(reader);
            LabelInputReported = new LabelInfo(reader);
            LabelInputExpected = new LabelInfo(reader);
            LabelInputGoodQty = new LabelInfo(reader);
            LabelInputScrapQty = new LabelInfo(reader);
            LabelInputSetup = new LabelInfo(reader);
            LabelInputRun = new LabelInfo(reader);
            LabelInputPostProcess = new LabelInfo(reader);
        }
        #endregion

        if (reader.VersionNumber <= 487)
        {
            SetKeysForOldVersions();
        }
    }

    public void Serialize(IWriter writer)
    {
        id.Serialize(writer);
        writer.Write(name);
        writer.Write(GridDisplayTextPrefix);

        writer.Write((int)NotReadyActivityTreatment);
        writer.Write((int)NonFirstActivityTreatment);
        writer.Write((int)PastHeadstartActivityTreatment);
        writer.Write(EnforceCapacityType);
        writer.Write(EnforceQty);
        writer.Write(MaxActivityCount);
        writer.Write(MaxHorizon);
        writer.Write(AutoCalculateReportedHours);

        FieldBlockStart.Serialize(writer);
        FieldBlockEnd.Serialize(writer);
        FieldJobExternalId.Serialize(writer);
        FieldManufacturingOrderExternalId.Serialize(writer);
        FieldOperationExternalId.Serialize(writer);
        FieldActivityExternalId.Serialize(writer);
        FieldJobName.Serialize(writer);
        FieldManufacturingOrderName.Serialize(writer);
        FieldOperationName.Serialize(writer);
        FieldJobDescription.Serialize(writer);
        FieldManufacturingOrderDescription.Serialize(writer);
        FieldOperationDescription.Serialize(writer);
        FieldProduct.Serialize(writer);
        FieldProductDescription.Serialize(writer);
        FieldProductionStatus.Serialize(writer);
        FieldActivityPercentFinished.Serialize(writer);
        FieldJobNeedDate.Serialize(writer);
        FieldManufacturingOrderNeedDate.Serialize(writer);
        FieldOperationNeedDate.Serialize(writer);
        FieldSetupHours.Serialize(writer);
        FieldRunHours.Serialize(writer);
        FieldPostProcessingHours.Serialize(writer);
        FieldTotalHours.Serialize(writer);
        FieldReportedSetupHours.Serialize(writer);
        FieldReportedRunHours.Serialize(writer);
        FieldReportedPostProcessingHours.Serialize(writer);
        FieldResourcesUsed.Serialize(writer);
        FieldSetupCode.Serialize(writer);
        FieldSetupNumber.Serialize(writer);
        FieldSetupColorA.Serialize(writer);
        FieldSetupColorR.Serialize(writer);
        FieldSetupColorG.Serialize(writer);
        FieldSetupColorB.Serialize(writer);
        FieldCustomer.Serialize(writer);
        FieldOrderNumber.Serialize(writer);
        FieldPriority.Serialize(writer);
        FieldOperationNotes.Serialize(writer);
        FieldJobNotes.Serialize(writer);
        FieldRequiredFinishQty.Serialize(writer);
        FieldExpectedScrapQty.Serialize(writer);
        FieldReportedGoodQty.Serialize(writer);
        FieldReportedScrapQty.Serialize(writer);
        FieldLatestConstraintDate.Serialize(writer);
        FieldLatestConstraint.Serialize(writer);
        FieldNextOperationName.Serialize(writer);
        FieldNextOperationDescription.Serialize(writer);
        FieldNextOperationResources.Serialize(writer);
        FieldNextOperationScheduledStart.Serialize(writer);
        FieldActivityComments.Serialize(writer);
        FieldReleased.Serialize(writer);
        FieldReleaseDate.Serialize(writer);
        FieldCommitment.Serialize(writer);
        FieldJobType.Serialize(writer);
        FieldActivityIsLate.Serialize(writer);
        FieldUOM.Serialize(writer);
        FieldPaused.Serialize(writer);
        FieldJobId.Serialize(writer);
        FieldMoId.Serialize(writer);
        FieldOpId.Serialize(writer);
        FieldActivityId.Serialize(writer);
        FieldBlockId.Serialize(writer);
        FieldOnHold.Serialize(writer);
        FieldHoldReason.Serialize(writer);
        FieldHoldUntilDate.Serialize(writer);
        FieldActivityStart.Serialize(writer);
        FieldActivityEnd.Serialize(writer);
        FieldSetupColor.Serialize(writer);
        FieldReadOnlyReason.Serialize(writer);
        FieldAttentionPercent.Serialize(writer);
        FieldReportedEndDate.Serialize(writer);
        FieldCurrentBufferPenetrationPercent.Serialize(writer);
        FieldProjectedBufferPenetrationPercent.Serialize(writer);
        FieldHot.Serialize(writer);

        //Material Fields
        FieldMaterialName.Serialize(writer);
        FieldMaterialDescription.Serialize(writer);
        FieldMaterialAvailable.Serialize(writer);
        FieldMaterialAvailableDate.Serialize(writer);
        FieldMaterialIssued.Serialize(writer);
        FieldMaterialIssuedQty.Serialize(writer);
        FieldMaterialTotalRequiredQty.Serialize(writer);
        FieldMaterialUOM.Serialize(writer);
        FieldMaterialQtyToIssue.Serialize(writer);
        FieldMaterialIssueFromWarehouse.Serialize(writer);

        //UserFields
        writer.Write(UserFieldInfos.Count);
        foreach (FieldInfo info in UserFieldInfos)
        {
            info.Serialize(writer);
        }

        //Subassembly Fields

        //Buttons
        ButtonSetup.Serialize(writer);
        ButtonRun.Serialize(writer);
        ButtonPostProcess.Serialize(writer);
        ButtonFinished.Serialize(writer);
        ButtonPaused.Serialize(writer);
        ButtonOnhold.Serialize(writer);
        ButtonReassign.Serialize(writer);
        ButtonCustom.Serialize(writer);
        ButtonSave.Serialize(writer);
        ButtonUndo.Serialize(writer);

        //Tabs
        TabInput.Serialize(writer);
        TabFlow.Serialize(writer);
        TabProcess.Serialize(writer);
        TabComments.Serialize(writer);
        TabMaterials.Serialize(writer);
        TabJobNotes.Serialize(writer);

        //Group Boxes
        GroupGeneral.Serialize(writer);
        GroupInputQuantities.Serialize(writer);
        GroupInputTime.Serialize(writer);
        GroupFlowConstraint.Serialize(writer);
        GroupFlowNextOperation.Serialize(writer);
        GroupProcessSetup.Serialize(writer);

        //Labels
        LabelJob.Serialize(writer);
        LabelProduct.Serialize(writer);
        LabelOperation.Serialize(writer);
        LabelName.Serialize(writer);
        LabelDescription.Serialize(writer);
        LabelNeedDate.Serialize(writer);
        LabelActivity.Serialize(writer);
        LabelInputReported.Serialize(writer);
        LabelInputExpected.Serialize(writer);
        LabelInputGoodQty.Serialize(writer);
        LabelInputScrapQty.Serialize(writer);
        LabelInputSetup.Serialize(writer);
        LabelInputRun.Serialize(writer);
        LabelInputPostProcess.Serialize(writer);
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public ShopViewResourceOptions(BaseId id, string name)
    {
        this.id = id;
        this.name = name;
    }

    public ShopViewResourceOptions(string name)
    {
        this.name = name;
    }

    #region Transaction Processing
    public void Update(ShopViewResourceOptions o, bool updateName)
    {
        if (updateName)
        {
            name = o.name;
        }

        GridDisplayTextPrefix = o.GridDisplayTextPrefix;

        NotReadyActivityTreatment = o.NotReadyActivityTreatment;
        NonFirstActivityTreatment = o.NonFirstActivityTreatment;
        PastHeadstartActivityTreatment = o.PastHeadstartActivityTreatment;
        EnforceCapacityType = o.EnforceCapacityType;
        EnforceQty = o.EnforceQty;
        MaxActivityCount = o.MaxActivityCount;
        MaxHorizon = o.MaxHorizon;
        AutoCalculateReportedHours = o.AutoCalculateReportedHours;

        FieldBlockStart = o.FieldBlockStart;
        FieldBlockEnd = o.FieldBlockEnd;
        FieldJobExternalId = o.FieldJobExternalId;
        FieldManufacturingOrderExternalId = o.FieldManufacturingOrderExternalId;
        FieldOperationExternalId = o.FieldOperationExternalId;
        FieldActivityExternalId = o.FieldActivityExternalId;
        FieldJobName = o.FieldJobName;
        FieldManufacturingOrderName = o.FieldManufacturingOrderName;
        FieldOperationName = o.FieldOperationName;
        FieldJobDescription = o.FieldJobDescription;
        FieldManufacturingOrderDescription = o.FieldManufacturingOrderDescription;
        FieldOperationDescription = o.FieldOperationDescription;
        FieldProduct = o.FieldProduct;
        FieldProductDescription = o.FieldProductDescription;
        FieldProductionStatus = o.FieldProductionStatus;
        FieldActivityPercentFinished = o.FieldActivityPercentFinished;
        FieldJobNeedDate = o.FieldJobNeedDate;
        FieldManufacturingOrderNeedDate = o.FieldManufacturingOrderNeedDate;
        FieldOperationNeedDate = o.FieldOperationNeedDate;
        FieldSetupHours = o.FieldSetupHours;
        FieldRunHours = o.FieldRunHours;
        FieldPostProcessingHours = o.FieldPostProcessingHours;
        FieldTotalHours = o.FieldTotalHours;
        FieldReportedSetupHours = o.FieldReportedSetupHours;
        FieldReportedRunHours = o.FieldReportedRunHours;
        FieldReportedPostProcessingHours = o.FieldReportedPostProcessingHours;
        FieldResourcesUsed = o.FieldResourcesUsed;
        FieldSetupCode = o.FieldSetupCode;
        FieldSetupNumber = o.FieldSetupNumber;
        FieldSetupColorA = o.FieldSetupColorA;
        FieldSetupColorR = o.FieldSetupColorR;
        FieldSetupColorG = o.FieldSetupColorG;
        FieldSetupColorB = o.FieldSetupColorB;
        FieldCustomer = o.FieldCustomer;
        FieldOrderNumber = o.FieldOrderNumber;
        FieldPriority = o.FieldPriority;
        FieldOperationNotes = o.FieldOperationNotes;
        FieldJobNotes = o.FieldJobNotes;
        FieldRequiredFinishQty = o.FieldRequiredFinishQty;
        FieldExpectedScrapQty = o.FieldExpectedScrapQty;
        FieldReportedGoodQty = o.FieldReportedGoodQty;
        FieldReportedScrapQty = o.FieldReportedScrapQty;
        FieldLatestConstraintDate = o.FieldLatestConstraintDate;
        FieldLatestConstraint = o.FieldLatestConstraint;
        FieldNextOperationName = o.FieldNextOperationName;
        FieldNextOperationDescription = o.FieldNextOperationDescription;
        FieldNextOperationResources = o.FieldNextOperationResources;
        FieldNextOperationScheduledStart = o.FieldNextOperationScheduledStart;
        FieldActivityComments = o.FieldActivityComments;
        FieldReleased = o.FieldReleased;
        FieldReleaseDate = o.FieldReleaseDate;
        FieldCommitment = o.FieldCommitment;
        FieldJobType = o.FieldJobType;
        FieldActivityIsLate = o.FieldActivityIsLate;
        FieldUOM = o.FieldUOM;
        FieldPaused = o.FieldPaused;
        FieldJobId = o.FieldJobId;
        FieldMoId = o.FieldMoId;
        FieldOpId = o.FieldOpId;
        FieldActivityId = o.FieldActivityId;
        FieldBlockId = o.FieldBlockId;
        FieldOnHold = o.FieldOnHold;
        FieldHoldReason = o.FieldHoldReason;
        FieldHoldUntilDate = o.FieldHoldUntilDate;
        FieldActivityStart = o.FieldActivityStart;
        FieldActivityEnd = o.FieldActivityEnd;
        FieldReportedEndDate = o.FieldReportedEndDate;
        FieldCurrentBufferPenetrationPercent = o.FieldCurrentBufferPenetrationPercent;
        FieldProjectedBufferPenetrationPercent = o.FieldProjectedBufferPenetrationPercent;
        FieldHot = o.FieldHot;

        FieldSetupColor = o.FieldSetupColor;
        FieldReadOnlyReason = o.FieldReadOnlyReason;
        FieldAttentionPercent = o.FieldAttentionPercent;

        //Material Fields
        FieldMaterialName = o.FieldMaterialName;
        FieldMaterialDescription = o.FieldMaterialDescription;
        FieldMaterialAvailable = o.FieldMaterialAvailable;
        FieldMaterialAvailableDate = o.FieldMaterialAvailableDate;
        FieldMaterialIssuedQty = o.FieldMaterialIssuedQty;
        FieldMaterialIssued = o.FieldMaterialIssued;
        FieldMaterialTotalRequiredQty = o.FieldMaterialTotalRequiredQty;
        FieldMaterialUOM = o.FieldMaterialUOM;
        FieldMaterialQtyToIssue = o.FieldMaterialQtyToIssue;
        FieldMaterialIssueFromWarehouse = o.FieldMaterialIssueFromWarehouse;

        //UserFields
        UserFieldInfos = o.UserFieldInfos;

        //Subassembly Fields

        //Buttons
        ButtonSetup = o.ButtonSetup;
        ButtonRun = o.ButtonRun;
        ButtonPostProcess = o.ButtonPostProcess;
        ButtonFinished = o.ButtonFinished;
        ButtonPaused = o.ButtonPaused;
        ButtonOnhold = o.ButtonOnhold;
        ButtonReassign = o.ButtonReassign;
        ButtonCustom = o.ButtonCustom;
        ButtonSave = o.ButtonSave;
        ButtonUndo = o.ButtonUndo;

        //Tabs
        TabInput = o.TabInput;
        TabFlow = o.TabFlow;
        TabProcess = o.TabProcess;
        TabComments = o.TabComments;
        TabMaterials = o.TabMaterials;
        TabJobNotes = o.TabJobNotes;

        //Group Boxes
        GroupGeneral = o.GroupGeneral;
        GroupInputQuantities = o.GroupInputQuantities;
        GroupInputTime = o.GroupInputTime;
        GroupFlowConstraint = o.GroupFlowConstraint;
        GroupFlowNextOperation = o.GroupFlowNextOperation;
        GroupProcessSetup = o.GroupProcessSetup;

        //Labels
        LabelJob = o.LabelJob;
        LabelProduct = o.LabelProduct;
        LabelOperation = o.LabelOperation;
        LabelName = o.LabelName;
        LabelDescription = o.LabelDescription;
        LabelNeedDate = o.LabelNeedDate;
        LabelActivity = o.LabelActivity;
        LabelInputReported = o.LabelInputReported;
        LabelInputExpected = o.LabelInputExpected;
        LabelInputGoodQty = o.LabelInputGoodQty;
        LabelInputScrapQty = o.LabelInputScrapQty;
        LabelInputSetup = o.LabelInputSetup;
        LabelInputRun = o.LabelInputRun;
        LabelInputPostProcess = o.LabelInputPostProcess;
    }

    /// <summary>
    /// Set the key value for FieldInfos for old versions. Key was added in version 415.
    /// </summary>
    private void SetKeysForOldVersions()
    {
        ShopViewResourceOptions defaultOptions = new ("default");

        FieldBlockStart.key = defaultOptions.FieldBlockStart.key;
        FieldBlockEnd.key = defaultOptions.FieldBlockEnd.key;
        FieldJobExternalId.key = defaultOptions.FieldJobExternalId.key;
        FieldManufacturingOrderExternalId.key = defaultOptions.FieldManufacturingOrderExternalId.key;
        FieldOperationExternalId.key = defaultOptions.FieldOperationExternalId.key;
        FieldActivityExternalId.key = defaultOptions.FieldActivityExternalId.key;
        FieldJobName.key = defaultOptions.FieldJobName.key;
        FieldManufacturingOrderName.key = defaultOptions.FieldManufacturingOrderName.key;
        FieldOperationName.key = defaultOptions.FieldOperationName.key;
        FieldJobDescription.key = defaultOptions.FieldJobDescription.key;
        FieldManufacturingOrderDescription.key = defaultOptions.FieldManufacturingOrderDescription.key;
        FieldOperationDescription.key = defaultOptions.FieldOperationDescription.key;
        FieldProduct.key = defaultOptions.FieldProduct.key;
        FieldProductDescription.key = defaultOptions.FieldProductDescription.key;
        FieldProductionStatus.key = defaultOptions.FieldProductionStatus.key;
        FieldActivityPercentFinished.key = defaultOptions.FieldActivityPercentFinished.key;
        FieldJobNeedDate.key = defaultOptions.FieldJobNeedDate.key;
        FieldManufacturingOrderNeedDate.key = defaultOptions.FieldManufacturingOrderNeedDate.key;
        FieldOperationNeedDate.key = defaultOptions.FieldOperationNeedDate.key;
        FieldSetupHours.key = defaultOptions.FieldSetupHours.key;
        FieldRunHours.key = defaultOptions.FieldRunHours.key;
        FieldPostProcessingHours.key = defaultOptions.FieldPostProcessingHours.key;
        FieldTotalHours.key = defaultOptions.FieldTotalHours.key;
        FieldReportedSetupHours.key = defaultOptions.FieldReportedSetupHours.key;
        FieldReportedRunHours.key = defaultOptions.FieldReportedRunHours.key;
        FieldReportedPostProcessingHours.key = defaultOptions.FieldReportedPostProcessingHours.key;
        FieldResourcesUsed.key = defaultOptions.FieldResourcesUsed.key;
        FieldSetupCode.key = defaultOptions.FieldSetupCode.key;
        FieldSetupNumber.key = defaultOptions.FieldSetupNumber.key;
        FieldSetupColorA.key = defaultOptions.FieldSetupColorA.key;
        FieldSetupColorR.key = defaultOptions.FieldSetupColorR.key;
        FieldSetupColorG.key = defaultOptions.FieldSetupColorG.key;
        FieldSetupColorB.key = defaultOptions.FieldSetupColorB.key;
        FieldCustomer.key = defaultOptions.FieldCustomer.key;
        FieldOrderNumber.key = defaultOptions.FieldOrderNumber.key;
        FieldPriority.key = defaultOptions.FieldPriority.key;
        FieldOperationNotes.key = defaultOptions.FieldOperationNotes.key;
        FieldJobNotes.key = defaultOptions.FieldJobNotes.key;
        FieldRequiredFinishQty.key = defaultOptions.FieldRequiredFinishQty.key;
        FieldExpectedScrapQty.key = defaultOptions.FieldExpectedScrapQty.key;
        FieldReportedGoodQty.key = defaultOptions.FieldReportedGoodQty.key;
        FieldReportedScrapQty.key = defaultOptions.FieldReportedScrapQty.key;
        FieldLatestConstraintDate.key = defaultOptions.FieldLatestConstraintDate.key;
        FieldLatestConstraint.key = defaultOptions.FieldLatestConstraint.key;
        FieldNextOperationName.key = defaultOptions.FieldNextOperationName.key;
        FieldNextOperationDescription.key = defaultOptions.FieldNextOperationDescription.key;
        FieldNextOperationResources.key = defaultOptions.FieldNextOperationResources.key;
        FieldNextOperationScheduledStart.key = defaultOptions.FieldNextOperationScheduledStart.key;
        FieldActivityComments.key = defaultOptions.FieldActivityComments.key;
        FieldReleased.key = defaultOptions.FieldReleased.key;
        FieldReleaseDate.key = defaultOptions.FieldReleaseDate.key;
        FieldCommitment.key = defaultOptions.FieldCommitment.key;
        FieldJobType.key = defaultOptions.FieldJobType.key;
        FieldActivityIsLate.key = defaultOptions.FieldActivityIsLate.key;
        FieldUOM.key = defaultOptions.FieldUOM.key;
        FieldPaused.key = defaultOptions.FieldPaused.key;
        FieldJobId.key = defaultOptions.FieldJobId.key;
        FieldMoId.key = defaultOptions.FieldMoId.key;
        FieldOpId.key = defaultOptions.FieldOpId.key;
        FieldActivityId.key = defaultOptions.FieldActivityId.key;
        FieldBlockId.key = defaultOptions.FieldBlockId.key;
        FieldOnHold.key = defaultOptions.FieldOnHold.key;
        FieldHoldReason.key = defaultOptions.FieldHoldReason.key;
        FieldHoldUntilDate.key = defaultOptions.FieldHoldUntilDate.key;
        FieldActivityStart.key = defaultOptions.FieldActivityStart.key;
        FieldActivityEnd.key = defaultOptions.FieldActivityEnd.key;
        FieldReportedEndDate.key = defaultOptions.FieldReportedEndDate.key;
        FieldCurrentBufferPenetrationPercent.key = defaultOptions.FieldCurrentBufferPenetrationPercent.key;
        FieldProjectedBufferPenetrationPercent.key = defaultOptions.FieldProjectedBufferPenetrationPercent.key;
        FieldHot.key = defaultOptions.FieldHot.key;

        FieldSetupColor = defaultOptions.FieldSetupColor;
        FieldReadOnlyReason.key = defaultOptions.FieldReadOnlyReason.key;
        FieldAttentionPercent.key = defaultOptions.FieldAttentionPercent.key;

        //Material.key Field.keys
        FieldMaterialName = defaultOptions.FieldMaterialName;
        FieldMaterialDescription.key = defaultOptions.FieldMaterialDescription.key;
        FieldMaterialAvailable.key = defaultOptions.FieldMaterialAvailable.key;
        FieldMaterialAvailableDate.key = defaultOptions.FieldMaterialAvailableDate.key;
        FieldMaterialIssuedQty.key = defaultOptions.FieldMaterialIssuedQty.key;
        FieldMaterialIssued.key = defaultOptions.FieldMaterialIssued.key;
        FieldMaterialTotalRequiredQty.key = defaultOptions.FieldMaterialTotalRequiredQty.key;
        FieldMaterialUOM.key = defaultOptions.FieldMaterialUOM.key;
        FieldMaterialQtyToIssue.key = defaultOptions.FieldMaterialQtyToIssue.key;
        FieldMaterialIssueFromWarehouse.key = defaultOptions.FieldMaterialIssueFromWarehouse.key;
    }
    #endregion

    #region Properties
    public BaseId id = BaseId.NULL_ID;

    private string name;

    public string Name => name;

    //Controls
    public enum objectState { Editable, ReadOnly, Hidden }

    public objectState NotReadyActivityTreatment = objectState.ReadOnly;
    public objectState NonFirstActivityTreatment = objectState.ReadOnly;
    public objectState PastHeadstartActivityTreatment = objectState.ReadOnly;
    public bool EnforceCapacityType = true;
    public bool EnforceQty = true;
    public int MaxActivityCount = 5;
    public TimeSpan MaxHorizon = TimeSpan.FromHours(8);
    public bool AutoCalculateReportedHours = true;

    //Miscellaneous settings
    public string GridDisplayTextPrefix = "Scheduled Activities"; //Resource name is shown after.

    //Fields		
    public FieldInfo FieldBlockStart = new ("BlockStart", "Start", true, false, 0, true);
    public FieldInfo FieldBlockEnd = new ("BlockEnd", "End", true, false, 1, true);
    public FieldInfo FieldJobExternalId = new ("JobExternalId", "Job External Id", false, false, 2, true);
    public FieldInfo FieldManufacturingOrderExternalId = new ("ManufacturingOrderExternalId", "M.O. External Id", false, false, 3, true);
    public FieldInfo FieldOperationExternalId = new ("OperationExternalId", "Op External Id", false, false, 4, true);
    public FieldInfo FieldActivityExternalId = new ("ActivityExternalId", "Activity External Id", false, false, 5, true);
    public FieldInfo FieldJobName = new ("JobName", "Job Name", true, true, 6, true);
    public FieldInfo FieldManufacturingOrderName = new ("ManufacturingOrderName", "M.O. Name", false, false, 7, true);
    public FieldInfo FieldOperationName = new ("OperationName", "Op Name", true, true, 8, true);
    public FieldInfo FieldJobDescription = new ("JobDescription", "Job Description", false, true, 9, true);
    public FieldInfo FieldManufacturingOrderDescription = new ("ManufacturingOrderDescription", "M.O. Description", false, false, 10, true);
    public FieldInfo FieldOperationDescription = new ("OperationDescription", "Op Description", true, true, 11, true);
    public FieldInfo FieldProduct = new ("Product", "Product", true, true, 12, true);
    public FieldInfo FieldProductDescription = new ("ProductDescription", "Product Description", false, true, 13, true);
    public FieldInfo FieldProductionStatus = new ("ProductionStatus", "Production Status", true, false, 14, true);
    public FieldInfo FieldActivityPercentFinished = new ("ActivityPercentFinished", "% Finished", true, false, 15, true);
    public FieldInfo FieldJobNeedDate = new ("JobNeedDate", "Job Need Date", true, true, 16, true);
    public FieldInfo FieldOperationNeedDate = new ("OperationNeedDate", "Op Need Date", false, true, 17, true);
    public FieldInfo FieldManufacturingOrderNeedDate = new ("ManufacturingOrderNeedDate", "M.O. Need Date", false, true, 18, true);
    public FieldInfo FieldCurrentBufferPenetrationPercent = new ("CurrentBufferPenetrationPercent", "Current Buffer Penetration %", true, true, 19, true);
    public FieldInfo FieldProjectedBufferPenetrationPercent = new ("ProjectedBufferPenetrationPercent", "Projected Buffer Penetration %", true, true, 20, true);
    public FieldInfo FieldSetupHours = new ("SetupHours", "Setup Hrs", false, true, 21, true);
    public FieldInfo FieldRunHours = new ("RunHours", "Run Hrs", false, true, 22, true);
    public FieldInfo FieldPostProcessingHours = new ("PostProcessingHours", "Post-Process Hrs", false, true, 23, true);
    public FieldInfo FieldTotalHours = new ("TotalHours", "Total Hrs", true, false, 24, true);
    public FieldInfo FieldReportedSetupHours = new ("ReportedSetupHours", "Reported Setup Hrs", false, true, 25, true);
    public FieldInfo FieldReportedRunHours = new ("ReportedRunHours", "Reported Run Hrs", false, true, 26, true);
    public FieldInfo FieldReportedPostProcessingHours = new ("ReportedPostProcessingHours", "Reported Post-Process Hrs", false, true, 27, true);
    public FieldInfo FieldResourcesUsed = new ("ResourcesUsed", "Resources Used", false, true, 28, true);
    public FieldInfo FieldSetupCode = new ("SetupCode", "Setup Code", true, true, 29, true);
    public FieldInfo FieldSetupNumber = new ("SetupNumber", "Setup Number", false, true, 30, true);
    public FieldInfo FieldSetupColorA = new ("SetupColorA", "Setup Color A", false, false, 31, true);
    public FieldInfo FieldSetupColorR = new ("SetupColorR", "Setup Color R", false, false, 32, true);
    public FieldInfo FieldSetupColorG = new ("SetupColorG", "Setup Color G", false, false, 33, true);
    public FieldInfo FieldSetupColorB = new ("SetupColorB", "Setup Color B", false, false, 34, true);
    public FieldInfo FieldCustomer = new ("Customer", "Customer", true, true, 35, true);
    public FieldInfo FieldOrderNumber = new ("OrderNumber", "Order Number", false, true, 36, true);
    public FieldInfo FieldPriority = new ("Priority", "Priority", false, true, 37, true);
    public FieldInfo FieldOperationNotes = new ("OperationNotes", "Op Notes", false, true, 38, true);
    public FieldInfo FieldJobNotes = new ("JobNotes", "Job Notes", false, false, 39, true);
    public FieldInfo FieldRequiredFinishQty = new ("RequiredFinishQty", "Required Finish Qty", true, true, 40, true);
    public FieldInfo FieldExpectedScrapQty = new ("ExpectedScrapQty", "Expected Scrap Qty", false, true, 41, true);
    public FieldInfo FieldReportedGoodQty = new ("ReportedGoodQty", "Reported Good Qty", false, true, 42, true);
    public FieldInfo FieldReportedScrapQty = new ("ReportedScrapQty", "Reported Scrap Qty", false, true, 43, true);
    public FieldInfo FieldLatestConstraintDate = new ("LatestConstraintDate", "Ready to Start on", false, true, 44, true);
    public FieldInfo FieldLatestConstraint = new ("LatestConstraint", "Constraint", false, true, 45, true);
    public FieldInfo FieldNextOperationName = new ("NextOperationName", "Next Op Name", false, true, 46, true);
    public FieldInfo FieldNextOperationDescription = new ("NextOperationDescription", "Next Op Description", false, true, 47, true);
    public FieldInfo FieldNextOperationResources = new ("NextOperationResources", "Next Op Resources", false, true, 48, true);
    public FieldInfo FieldNextOperationScheduledStart = new ("NextOperationScheduledStart", "Next Op Start", false, true, 49, true);
    public FieldInfo FieldActivityComments = new ("ActivityComments", "Activity Comments", false, true, 50, true);
    public FieldInfo FieldReleased = new ("Released", "Released", false, false, 51, true);
    public FieldInfo FieldReleaseDate = new ("ReleaseDate", "Release Date", false, false, 52, true);
    public FieldInfo FieldCommitment = new ("Commitment", "Commitment", false, false, 53, true);
    public FieldInfo FieldJobType = new ("JobType", "Job Type", false, false, 54, true);
    public FieldInfo FieldActivityIsLate = new ("ActivityIsLate", "Activity Is Late", false, true, 55, true);
    public FieldInfo FieldUOM = new ("UOM", "UOM", false, false, 56, true);
    public FieldInfo FieldPaused = new ("Paused", "Paused", false, false, 57, true);
    public FieldInfo FieldJobId = new ("JobId", "Job Id", false, false, 58, true);
    public FieldInfo FieldMoId = new ("MoId", "M.O. Id", false, false, 59, true);
    public FieldInfo FieldOpId = new ("OpId", "Op Id", false, false, 60, true);
    public FieldInfo FieldActivityId = new ("ActivityId", "Activity Id", false, false, 61, true);
    public FieldInfo FieldBlockId = new ("BlockId", "Block Id", false, false, 62, true);
    public FieldInfo FieldOnHold = new ("OnHold", "On-Hold", false, false, 63, true);
    public FieldInfo FieldHoldReason = new ("HoldReason", "Hold Reason", false, false, 64, true);
    public FieldInfo FieldHoldUntilDate = new ("HoldUntilDate", "Hold Until Date", false, false, 65, true);
    public FieldInfo FieldActivityStart = new ("ActivityStart", "Activity Start", false, true, 66, true);
    public FieldInfo FieldActivityEnd = new ("ActivityEnd", "Activity End", false, true, 67, true);
    public FieldInfo FieldReportedEndDate = new ("ReportedEndDate", "Reported End Date", false, false, 68, true);
    public FieldInfo FieldHot = new ("Hot", "Hot", false, false, 69, true);

    public FieldInfo FieldSetupColor = new ("SetupColor", "Color", false, true, 99, true); //not in grid.
    public FieldInfo FieldReadOnlyReason = new ("ReadOnlyReason", "Read Only Reason", false, false, 99, true); //not in grid.
    public FieldInfo FieldAttentionPercent = new ("AttentionPercent", "Attention %", false, false, 99, true); //not in grid.
    // !! When adding a new FieldInfo it must be added to this function GetFieldInfosSortedBySortIndexAndDisplayText()

    //Material Fields
    public FieldInfo FieldMaterialName = new ("MaterialName", "Name", true, false, 0, true);
    public FieldInfo FieldMaterialDescription = new ("MaterialDescription", "Description", true, false, 1, true);
    public FieldInfo FieldMaterialTotalRequiredQty = new ("MaterialTotalRequiredQty", "Total Reqd Qty", true, false, 2, true);
    public FieldInfo FieldMaterialUOM = new ("MaterialUOM", "UOM", true, false, 3, true);
    public FieldInfo FieldMaterialIssuedQty = new ("MaterialIssuedQty", "Issued Qty", true, false, 4, true);
    public FieldInfo FieldMaterialQtyToIssue = new ("MaterialQtyToIssue", "Qty To Issue", true, false, 5, true);
    public FieldInfo FieldMaterialIssueFromWarehouse = new ("MaterialIssueFromWarehouse", "From Warehouse", true, false, 6, true);
    public FieldInfo FieldMaterialAvailable = new ("MaterialAvailable", "Available", true, false, 7, true);
    public FieldInfo FieldMaterialAvailableDate = new ("MaterialAvailableDate", "Available Date", true, false, 8, true);
    public FieldInfo FieldMaterialIssued = new ("MaterialIssued", "Issued", true, false, 9, true);
    // !! When adding a new FieldInfo it must be added to this function GetFieldInfosSortedBySortIndexAndDisplayText()

    //User Fields
    public List<FieldInfo> UserFieldInfos = new ();

    public IEnumerable<FieldInfo> GetUserFieldInfosSortedBySortIndexAndDisplayText()
    {
        UserFieldInfos.Sort();
        return UserFieldInfos;
    }

    //Subassembly Fields

    //Buttons
    public ButtonInfo ButtonSetup = new ("Setup", true);
    public ButtonInfo ButtonRun = new ("Run", true);
    public ButtonInfo ButtonPostProcess = new ("Post-Process", true);
    public ButtonInfo ButtonFinished = new ("Finished", true);
    public ButtonInfo ButtonPaused = new ("Paused", true);
    public ButtonInfo ButtonOnhold = new ("On-Hold", true);
    public ButtonInfo ButtonReassign = new ("Re-Assign", true);
    public ButtonInfo ButtonCustom = new ("Custom", true);
    public ButtonInfo ButtonSave = new ("Save", true);
    public ButtonInfo ButtonUndo = new ("Undo", true);

    //Tabs
    public TabInfo TabInput = new ("&Input", true);
    public TabInfo TabFlow = new ("&Flow", true);
    public TabInfo TabProcess = new ("&Process", true);
    public TabInfo TabComments = new ("&Comments", true);
    public TabInfo TabMaterials = new ("&Materials", true);
    public TabInfo TabJobNotes = new ("&Job Notes", true);

    //Group Boxes
    public GroupBoxInfo GroupGeneral = new ("General", true);
    public GroupBoxInfo GroupInputQuantities = new ("Quantities", true);
    public GroupBoxInfo GroupInputTime = new ("Time", true);
    public GroupBoxInfo GroupFlowConstraint = new ("Constraint", true);
    public GroupBoxInfo GroupFlowNextOperation = new ("Next Operation", true);
    public GroupBoxInfo GroupProcessSetup = new ("Setup", true);

    //Labels
    public LabelInfo LabelJob = new ("Job", true);
    public LabelInfo LabelProduct = new ("Product", true);
    public LabelInfo LabelOperation = new ("Operation", true);
    public LabelInfo LabelName = new ("Name", true);
    public LabelInfo LabelDescription = new ("Description", true);
    public LabelInfo LabelNeedDate = new ("Need Date", true);
    public LabelInfo LabelActivity = new ("Activity", true);
    public LabelInfo LabelInputReported = new ("Reported", true);
    public LabelInfo LabelInputExpected = new ("Expected", true);
    public LabelInfo LabelInputGoodQty = new ("Good Qty", true);
    public LabelInfo LabelInputScrapQty = new ("Scrap Qty", true);
    public LabelInfo LabelInputSetup = new ("Setup", true);
    public LabelInfo LabelInputRun = new ("Run", true);
    public LabelInfo LabelInputPostProcess = new ("Post-Process", true);
    #endregion

    public List<FieldInfo> GetActivityFieldInfosSortedBySortIndexAndDisplayText()
    {
        List<FieldInfo> fieldInfosList = new ();

        //Add all the Field Infos to the list
        fieldInfosList.Add(FieldBlockStart);
        fieldInfosList.Add(FieldBlockEnd);
        fieldInfosList.Add(FieldJobExternalId);
        fieldInfosList.Add(FieldManufacturingOrderExternalId);
        fieldInfosList.Add(FieldOperationExternalId);
        fieldInfosList.Add(FieldActivityExternalId);
        fieldInfosList.Add(FieldJobName);
        fieldInfosList.Add(FieldManufacturingOrderName);
        fieldInfosList.Add(FieldOperationName);
        fieldInfosList.Add(FieldJobDescription);
        fieldInfosList.Add(FieldManufacturingOrderDescription);
        fieldInfosList.Add(FieldOperationDescription);
        fieldInfosList.Add(FieldProduct);
        fieldInfosList.Add(FieldProductDescription);
        fieldInfosList.Add(FieldProductionStatus);
        fieldInfosList.Add(FieldActivityPercentFinished);
        fieldInfosList.Add(FieldJobNeedDate);
        fieldInfosList.Add(FieldManufacturingOrderNeedDate);
        fieldInfosList.Add(FieldOperationNeedDate);
        fieldInfosList.Add(FieldSetupHours);
        fieldInfosList.Add(FieldRunHours);
        fieldInfosList.Add(FieldPostProcessingHours);
        fieldInfosList.Add(FieldTotalHours);
        fieldInfosList.Add(FieldReportedSetupHours);
        fieldInfosList.Add(FieldReportedRunHours);
        fieldInfosList.Add(FieldReportedPostProcessingHours);
        fieldInfosList.Add(FieldResourcesUsed);
        fieldInfosList.Add(FieldSetupCode);
        fieldInfosList.Add(FieldSetupNumber);
        fieldInfosList.Add(FieldSetupColorA);
        fieldInfosList.Add(FieldSetupColorR);
        fieldInfosList.Add(FieldSetupColorG);
        fieldInfosList.Add(FieldSetupColorB);
        fieldInfosList.Add(FieldCustomer);
        fieldInfosList.Add(FieldOrderNumber);
        fieldInfosList.Add(FieldPriority);
        fieldInfosList.Add(FieldOperationNotes);
        fieldInfosList.Add(FieldJobNotes);
        fieldInfosList.Add(FieldRequiredFinishQty);
        fieldInfosList.Add(FieldExpectedScrapQty);
        fieldInfosList.Add(FieldReportedGoodQty);
        fieldInfosList.Add(FieldReportedScrapQty);
        fieldInfosList.Add(FieldLatestConstraintDate);
        fieldInfosList.Add(FieldLatestConstraint);
        fieldInfosList.Add(FieldNextOperationName);
        fieldInfosList.Add(FieldNextOperationDescription);
        fieldInfosList.Add(FieldNextOperationResources);
        fieldInfosList.Add(FieldNextOperationScheduledStart);
        fieldInfosList.Add(FieldActivityComments);
        fieldInfosList.Add(FieldReleased);
        fieldInfosList.Add(FieldReleaseDate);
        fieldInfosList.Add(FieldCommitment);
        fieldInfosList.Add(FieldJobType);
        fieldInfosList.Add(FieldActivityIsLate);
        fieldInfosList.Add(FieldUOM);
        fieldInfosList.Add(FieldPaused);
        fieldInfosList.Add(FieldJobId);
        fieldInfosList.Add(FieldMoId);
        fieldInfosList.Add(FieldOpId);
        fieldInfosList.Add(FieldActivityId);
        fieldInfosList.Add(FieldBlockId);
        fieldInfosList.Add(FieldOnHold);
        fieldInfosList.Add(FieldHoldReason);
        fieldInfosList.Add(FieldHoldUntilDate);
        fieldInfosList.Add(FieldActivityStart);
        fieldInfosList.Add(FieldActivityEnd);
        fieldInfosList.Add(FieldReportedEndDate);
        fieldInfosList.Add(FieldCurrentBufferPenetrationPercent);
        fieldInfosList.Add(FieldProjectedBufferPenetrationPercent);
        fieldInfosList.Add(FieldReadOnlyReason);
        fieldInfosList.Add(FieldAttentionPercent);
        fieldInfosList.Add(FieldHot);

        //Exclude fields not in the Activity list
        //fieldInfosList.Add(FieldSetupColor); //not in grid 

        fieldInfosList.Sort();
        return fieldInfosList;
    }

    public List<FieldInfo> GetMateiralFieldInfosSortedBySortIndexAndDisplayText()
    {
        List<FieldInfo> fieldInfosList = new ();

        //Material Fields
        fieldInfosList.Add(FieldMaterialName);
        fieldInfosList.Add(FieldMaterialDescription);
        fieldInfosList.Add(FieldMaterialTotalRequiredQty);
        fieldInfosList.Add(FieldMaterialUOM);
        fieldInfosList.Add(FieldMaterialIssuedQty);
        fieldInfosList.Add(FieldMaterialQtyToIssue);
        fieldInfosList.Add(FieldMaterialIssueFromWarehouse);
        fieldInfosList.Add(FieldMaterialAvailable);
        fieldInfosList.Add(FieldMaterialAvailableDate);
        fieldInfosList.Add(FieldMaterialIssued);

        fieldInfosList.Sort();
        return fieldInfosList;
    }

    #region Subclasses
    public class TabInfo
    {
        #region IPTSerializable
        public const int UNIQUE_ID = 497;

        public TabInfo(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                reader.Read(out displayText);
                reader.Read(out visible);
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(displayText);
            writer.Write(visible);
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public TabInfo(string displayText, bool visible)
        {
            this.displayText = displayText;
            this.visible = visible;
        }

        public string displayText;
        public bool visible;
    }

    public class GroupBoxInfo
    {
        #region IPTSerializable
        public const int UNIQUE_ID = 496;

        public GroupBoxInfo(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                reader.Read(out displayText);
                reader.Read(out visible);
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(displayText);
            writer.Write(visible);
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public GroupBoxInfo(string displayText, bool visible)
        {
            this.displayText = displayText;
            this.visible = visible;
        }

        public string displayText;
        public bool visible;
    }

    public class ButtonInfo
    {
        #region IPTSerializable
        public const int UNIQUE_ID = 495;

        public ButtonInfo(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                reader.Read(out displayText);
                reader.Read(out visible);
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(displayText);
            writer.Write(visible);
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public ButtonInfo(string displayText, bool visible)
        {
            this.displayText = displayText;
            this.visible = visible;
        }

        public string displayText;
        public bool visible;
    }

    public class FieldInfo : IComparable
    {
        #region IPTSerializable
        public const int UNIQUE_ID = 494;

        public FieldInfo(IReader reader)
        {
            if (reader.VersionNumber >= 415)
            {
                reader.Read(out displayText);
                reader.Read(out showInGrid);
                reader.Read(out showInDialog);
                reader.Read(out sortIndex);
                reader.Read(out readOnly);
                reader.Read(out key);
            }

            #region version 1
            else if (reader.VersionNumber >= 1)
            {
                reader.Read(out displayText);
                reader.Read(out showInGrid);
                reader.Read(out showInDialog);
                reader.Read(out sortIndex);
                reader.Read(out readOnly);
            }
            #endregion
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(displayText);
            writer.Write(showInGrid);
            writer.Write(showInDialog);
            writer.Write(sortIndex);
            writer.Write(readOnly);
            writer.Write(key);
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public FieldInfo(string key, string displayText, bool showInGrid, bool showInDialog, int sortIndex, bool readOnly)
        {
            this.key = key;
            this.displayText = displayText;
            this.showInGrid = showInGrid;
            this.showInDialog = showInDialog;
            this.sortIndex = sortIndex;
            this.readOnly = readOnly;
        }

        public string key;
        public string displayText;
        public bool showInGrid;
        public bool showInDialog;
        public int sortIndex;
        public bool readOnly;

        #region IComparer<BlockEvent> Members
        //public int Compare(FieldInfo x, FieldInfo y)
        //{
        //    if (x.sortIndex < y.sortIndex)
        //        return -1;
        //    else if (y.sortIndex < x.sortIndex)
        //        return 0;
        //    else
        //        return 1;
        //}
        #endregion

        #region IComparable Members
        int IComparable.CompareTo(object obj)
        {
            FieldInfo be = (FieldInfo)obj;
            if (be.sortIndex < sortIndex)
            {
                return 1;
            }

            if (sortIndex < be.sortIndex)
            {
                return -1;
            }

            return be.displayText.CompareTo(displayText);
        }
        #endregion
    }

    public class LabelInfo
    {
        #region IPTSerializable
        public const int UNIQUE_ID = 510;

        public LabelInfo(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                reader.Read(out displayText);
                reader.Read(out visible);
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(displayText);
            writer.Write(visible);
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public LabelInfo(string displayText, bool visible)
        {
            this.displayText = displayText;
            this.visible = visible;
        }

        public string displayText;
        public bool visible;
    }
    #endregion

    #region ICloneable Members
    object ICloneable.Clone()
    {
        // TODO:  Add ShopViewResourceOptions.Clone implementation
        return Clone();
    }

    public ShopViewResourceOptions Clone()
    {
        ShopViewResourceOptions newOptions = new (Name);
        newOptions.Update(this, true);
        return newOptions;
    }
    #endregion

        //TODO: Restore this if necessary, there were no usages of this
        //public void ValidateUDFs(UserFieldList a_udfList)
        //{
        //    int nextIndex = 0;

        //    Dictionary<string, FieldInfo> fieldInfoMapping = new Dictionary<string, FieldInfo>();
        //    foreach (FieldInfo info in UserFieldInfos)
        //    {
        //        fieldInfoMapping.Add(info.key, info);
        //        nextIndex = Math.Max(nextIndex, info.sortIndex);
        //    }

        //    UserFieldInfos.Clear();
        //    for (var i = 0; i < a_udfList.Count; i++)
        //    {
        //        UserField udf = a_udfList[i];
        //        if (fieldInfoMapping.TryGetValue(udf.Name, out FieldInfo udfInfo))
        //        {
        //            UserFieldInfos.Add(udfInfo);
        //        }
        //        else
        //        {
        //            UserFieldInfos.Add(new FieldInfo(udf.Name, udf.Name, false, false, nextIndex, false));
        //            nextIndex++;
        //        }
        //    }
        //}
}
