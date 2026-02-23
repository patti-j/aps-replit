using PT.APSCommon;
using PT.Common.Attributes;
using PT.Common.Debugging;
using PT.Common.Exceptions;
using PT.Common.Localization;
using PT.ERPTransmissions;
using PT.Scheduler.Demand;
using PT.Scheduler.Schedule.Operation;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace PT.Scheduler;

/// <summary>
/// Provides the specifications of how a step in the manufacturing process will be performed within the factory.
/// </summary>
public abstract partial class InternalOperation : BaseOperation
{
    #region IPTSerializable Members
    protected InternalOperation(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_reader, a_idGen)
    {
        m_activities = new InternalActivityManager(a_idGen);

        if (a_reader.VersionNumber >= 13004)
        {
            a_reader.Read(out m_carryingCost);
            a_reader.Read(out m_compatibilityCode);
            a_reader.Read(out m_overlapTransferQty);
            a_reader.Read(out m_setupNumber);

            a_reader.Read(out int val);
            m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

            ReadKeepSuccessorsTimeLimit(a_reader);
            a_reader.Read(out m_latestConstraintDate);

            a_reader.Read(out val);
            m_latestConstraint = (LatestConstraintEnum)val;

            m_productionInfo = new ProductionInfo(a_reader);

            a_reader.Read(out m_setupColor);

            a_reader.Read(out m_standardRunSpan);
            a_reader.Read(out m_standardSetupSpan);

            a_reader.Read(out m_batchCode);

            m_resourceRequirements = new ResourceRequirementManager(a_reader, a_idGen);
            m_activities = new InternalActivityManager(a_reader, a_idGen);
            m_vesselTypeRequirementClaims = new VesselTypeRequirementsCollection(a_reader);
            m_vesselTypeRequirementReleases = new VesselTypeRequirementsCollection(a_reader);
            m_internalOperationFlags = new BoolVector32(a_reader);

            //Store references info for use later
            m_referenceInfo = new ReferenceInfo(m_latestConstraint, a_reader);

            a_reader.Read(out m_latestConstraintDescription);
            a_reader.Read(out m_latestConstraintResourceName);

            a_reader.Read(out m_productCode);

            m_splitInfo = new AutoSplitInfo(a_reader);
            a_reader.Read(out int setupSplitType);
            m_setupSplitType = (OperationDefs.ESetupSplitType)setupSplitType;
            a_reader.Read(out m_campaignCode);
            a_reader.Read(out int overlapResReleasesDictionaryCount);

            for (int i = 0; i < overlapResReleasesDictionaryCount; i++)
            {
                BaseId resId = new BaseId(a_reader);
                a_reader.Read(out long releaseTicks);
                m_referenceInfo.OverlapResourcesReleases.Add(resId, releaseTicks);
            }

            a_reader.Read(out m_sequenceHeadStartDays);
        }
        else if (a_reader.VersionNumber >= 12521)
        {
            a_reader.Read(out m_carryingCost);
            a_reader.Read(out m_compatibilityCode);
            a_reader.Read(out m_overlapTransferQty);
            a_reader.Read(out m_setupNumber);

            a_reader.Read(out int val);
            m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

            ReadKeepSuccessorsTimeLimit(a_reader);
            a_reader.Read(out m_latestConstraintDate);

            a_reader.Read(out val);
            m_latestConstraint = (LatestConstraintEnum)val;

            m_productionInfo = new ProductionInfo(a_reader);

            a_reader.Read(out m_setupColor);

            a_reader.Read(out m_standardRunSpan);
            a_reader.Read(out m_standardSetupSpan);

            a_reader.Read(out m_batchCode);

            m_resourceRequirements = new ResourceRequirementManager(a_reader, a_idGen);
            m_activities = new InternalActivityManager(a_reader, a_idGen);
            m_vesselTypeRequirementClaims = new VesselTypeRequirementsCollection(a_reader);
            m_vesselTypeRequirementReleases = new VesselTypeRequirementsCollection(a_reader);
            m_internalOperationFlags = new BoolVector32(a_reader);

            //Store references info for use later
            m_referenceInfo = new ReferenceInfo(m_latestConstraint, a_reader);

            a_reader.Read(out m_latestConstraintDescription);
            a_reader.Read(out m_latestConstraintResourceName);

            a_reader.Read(out m_productCode);

            m_splitInfo = new AutoSplitInfo(a_reader);
            a_reader.Read(out int setupSplitType);
            m_setupSplitType = (OperationDefs.ESetupSplitType)setupSplitType;
            a_reader.Read(out m_campaignCode);
            a_reader.Read(out int overlapResReleasesDictionaryCount);

            for (int i = 0; i < overlapResReleasesDictionaryCount; i++)
            {
                BaseId resId = new BaseId(a_reader);
                a_reader.Read(out long releaseTicks);
                m_referenceInfo.OverlapResourcesReleases.Add(resId, releaseTicks);
            }
        }        
        else if (a_reader.VersionNumber >= 12518)
        {
            a_reader.Read(out m_carryingCost);
            a_reader.Read(out m_compatibilityCode);
            a_reader.Read(out m_overlapTransferQty);
            a_reader.Read(out m_setupNumber);

            a_reader.Read(out int val);
            m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

            ReadKeepSuccessorsTimeLimit(a_reader);
            a_reader.Read(out m_latestConstraintDate);

            a_reader.Read(out val);
            m_latestConstraint = (LatestConstraintEnum)val;

            m_productionInfo = DeserializeProductionInfo(a_reader);

            a_reader.Read(out m_setupColor);

            a_reader.Read(out m_standardRunSpan);
            a_reader.Read(out m_standardSetupSpan);

            a_reader.Read(out m_batchCode);

            m_resourceRequirements = new ResourceRequirementManager(a_reader, a_idGen);
            m_activities = new InternalActivityManager(a_reader, a_idGen);
            m_vesselTypeRequirementClaims = new VesselTypeRequirementsCollection(a_reader);
            m_vesselTypeRequirementReleases = new VesselTypeRequirementsCollection(a_reader);
            m_internalOperationFlags = new BoolVector32(a_reader);

            //Store references info for use later
            m_referenceInfo = new ReferenceInfo(m_latestConstraint, a_reader);

            a_reader.Read(out m_latestConstraintDescription);
            a_reader.Read(out m_latestConstraintResourceName);

            a_reader.Read(out m_productCode);

            m_splitInfo = new AutoSplitInfo(a_reader);
            a_reader.Read(out int setupSplitType);
            m_setupSplitType = (OperationDefs.ESetupSplitType)setupSplitType;
            a_reader.Read(out m_campaignCode);
            a_reader.Read(out int overlapResReleasesDictionaryCount);

            for (int i = 0; i < overlapResReleasesDictionaryCount; i++)
            {
                BaseId resId = new BaseId(a_reader);
                a_reader.Read(out long releaseTicks);
                m_referenceInfo.OverlapResourcesReleases.Add(resId, releaseTicks);
            }
        }
        else if (a_reader.VersionNumber >= 12508)
        {
            a_reader.Read(out m_carryingCost);
            a_reader.Read(out m_compatibilityCode);
            a_reader.Read(out m_overlapTransferQty);
            a_reader.Read(out m_setupNumber);

            a_reader.Read(out int val);
            m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

            ReadKeepSuccessorsTimeLimit(a_reader);
            a_reader.Read(out m_latestConstraintDate);

            a_reader.Read(out val);
            m_latestConstraint = (LatestConstraintEnum)val;

            m_productionInfo = DeserializeProductionInfo(a_reader);

            a_reader.Read(out m_setupColor);

            a_reader.Read(out m_standardRunSpan);
            a_reader.Read(out m_standardSetupSpan);

            a_reader.Read(out m_batchCode);

            m_resourceRequirements = new ResourceRequirementManager(a_reader, a_idGen);
            m_activities = new InternalActivityManager(a_reader, a_idGen);
            m_vesselTypeRequirementClaims = new VesselTypeRequirementsCollection(a_reader);
            m_vesselTypeRequirementReleases = new VesselTypeRequirementsCollection(a_reader);
            m_internalOperationFlags = new BoolVector32(a_reader);

            //Store references info for use later
            m_referenceInfo = new ReferenceInfo(m_latestConstraint, a_reader);

            a_reader.Read(out m_latestConstraintDescription);
            a_reader.Read(out m_latestConstraintResourceName);

            a_reader.Read(out m_productCode);

            m_splitInfo = new AutoSplitInfo(a_reader);
            a_reader.Read(out int setupSplitType);
            m_setupSplitType = (OperationDefs.ESetupSplitType)setupSplitType;
            a_reader.Read(out m_campaignCode);
        }
        else if (a_reader.VersionNumber >= 12507)
        {
            a_reader.Read(out m_carryingCost);
            a_reader.Read(out m_compatibilityCode);
            a_reader.Read(out m_overlapTransferQty);
            a_reader.Read(out m_setupNumber);

            a_reader.Read(out int val);
            m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

            ReadKeepSuccessorsTimeLimit(a_reader);
            a_reader.Read(out m_latestConstraintDate);

            a_reader.Read(out val);
            m_latestConstraint = (LatestConstraintEnum)val;

            m_productionInfo = DeserializeProductionInfo(a_reader);

            a_reader.Read(out m_setupColor);

            a_reader.Read(out m_standardRunSpan);
            a_reader.Read(out m_standardSetupSpan);

            a_reader.Read(out m_batchCode);

            m_resourceRequirements = new ResourceRequirementManager(a_reader, a_idGen);
            m_activities = new InternalActivityManager(a_reader, a_idGen);
            m_vesselTypeRequirementClaims = new VesselTypeRequirementsCollection(a_reader);
            m_vesselTypeRequirementReleases = new VesselTypeRequirementsCollection(a_reader);
            m_internalOperationFlags = new BoolVector32(a_reader);

            //Store references info for use later
            m_referenceInfo = new ReferenceInfo(m_latestConstraint, a_reader);

            a_reader.Read(out m_latestConstraintDescription);
            a_reader.Read(out m_latestConstraintResourceName);

            a_reader.Read(out m_productCode);

            m_splitInfo = new AutoSplitInfo(a_reader);
            a_reader.Read(out int setupSplitType);
            m_setupSplitType = (OperationDefs.ESetupSplitType)setupSplitType;
        }
        else if (a_reader.VersionNumber >= 12502)
        {
            a_reader.Read(out m_carryingCost);
            a_reader.Read(out m_compatibilityCode);
            a_reader.Read(out m_overlapTransferQty);
            a_reader.Read(out m_setupNumber);

            a_reader.Read(out int val);
            m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

            ReadKeepSuccessorsTimeLimit(a_reader);
            a_reader.Read(out m_latestConstraintDate);

            a_reader.Read(out val);
            m_latestConstraint = (LatestConstraintEnum)val;

            m_productionInfo = DeserializeProductionInfo(a_reader);

            a_reader.Read(out m_setupColor);

            a_reader.Read(out m_standardRunSpan);
            a_reader.Read(out m_standardSetupSpan);

            a_reader.Read(out m_batchCode);

            m_resourceRequirements = new ResourceRequirementManager(a_reader, a_idGen);
            m_activities = new InternalActivityManager(a_reader, a_idGen);
            m_vesselTypeRequirementClaims = new VesselTypeRequirementsCollection(a_reader);
            m_vesselTypeRequirementReleases = new VesselTypeRequirementsCollection(a_reader);
            m_internalOperationFlags = new BoolVector32(a_reader);

            //Store references info for use later
            m_referenceInfo = new ReferenceInfo(m_latestConstraint, a_reader);

            a_reader.Read(out m_latestConstraintDescription);
            a_reader.Read(out m_latestConstraintResourceName);

            a_reader.Read(out m_productCode);

            m_splitInfo = new AutoSplitInfo(a_reader);
            a_reader.Read(out int setupSplitType);
            m_setupSplitType = (OperationDefs.ESetupSplitType)setupSplitType;
        }
        else if (a_reader.VersionNumber >= 12316)
        {
            a_reader.Read(out m_carryingCost);
            a_reader.Read(out m_compatibilityCode);
            a_reader.Read(out m_overlapTransferQty);
            a_reader.Read(out string setupCode);
            a_reader.Read(out m_setupNumber);

            a_reader.Read(out int val);
            m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

            ReadKeepSuccessorsTimeLimit(a_reader);
            a_reader.Read(out m_latestConstraintDate);

            a_reader.Read(out val);
            m_latestConstraint = (LatestConstraintEnum)val;

            m_productionInfo = DeserializeProductionInfo(a_reader);

            a_reader.Read(out m_setupColor);

            a_reader.Read(out m_standardRunSpan);
            a_reader.Read(out m_standardSetupSpan);

            a_reader.Read(out m_batchCode);

            m_resourceRequirements = new ResourceRequirementManager(a_reader, a_idGen);
            m_activities = new InternalActivityManager(a_reader, a_idGen);
            m_vesselTypeRequirementClaims = new VesselTypeRequirementsCollection(a_reader);
            m_vesselTypeRequirementReleases = new VesselTypeRequirementsCollection(a_reader);
            m_internalOperationFlags = new BoolVector32(a_reader);

            //Store references info for use later
            m_referenceInfo = new ReferenceInfo(m_latestConstraint, a_reader);

            a_reader.Read(out m_latestConstraintDescription);
            a_reader.Read(out m_latestConstraintResourceName);

            a_reader.Read(out m_productCode);

            m_splitInfo = new AutoSplitInfo(a_reader);
            a_reader.Read(out int setupSplitType);
            m_setupSplitType = (OperationDefs.ESetupSplitType)setupSplitType;
        }
        else if (a_reader.VersionNumber >= 12306)
        {
            a_reader.Read(out m_carryingCost);
            a_reader.Read(out m_compatibilityCode);
            a_reader.Read(out m_overlapTransferQty);
            a_reader.Read(out string setupCode);
            a_reader.Read(out m_setupNumber);

            a_reader.Read(out int val);
            m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

            ReadKeepSuccessorsTimeLimit(a_reader);
            a_reader.Read(out m_latestConstraintDate);

            a_reader.Read(out val);
            m_latestConstraint = (LatestConstraintEnum)val;

            m_productionInfo = DeserializeProductionInfo(a_reader);

            a_reader.Read(out m_setupColor);

            a_reader.Read(out m_standardRunSpan);
            a_reader.Read(out m_standardSetupSpan);

            a_reader.Read(out m_batchCode);

            m_resourceRequirements = new ResourceRequirementManager(a_reader, a_idGen);
            m_activities = new InternalActivityManager(a_reader, a_idGen);
            m_vesselTypeRequirementClaims = new VesselTypeRequirementsCollection(a_reader);
            m_vesselTypeRequirementReleases = new VesselTypeRequirementsCollection(a_reader);
            m_internalOperationFlags = new BoolVector32(a_reader);

            //Store references info for use later
            m_referenceInfo = new ReferenceInfo(m_latestConstraint, a_reader);

            a_reader.Read(out m_latestConstraintDescription);
            a_reader.Read(out m_latestConstraintResourceName);

            a_reader.Read(out m_productCode);

            m_splitInfo = new AutoSplitInfo(a_reader);
        }
        else if (a_reader.VersionNumber >= 12304)
        {
            a_reader.Read(out m_carryingCost);
            a_reader.Read(out m_compatibilityCode);
            a_reader.Read(out m_overlapTransferQty);
            a_reader.Read(out decimal minAutoSplitQty);
            a_reader.Read(out string setupCode);
            a_reader.Read(out m_setupNumber);

            a_reader.Read(out int val);
            m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

            ReadKeepSuccessorsTimeLimit(a_reader);
            a_reader.Read(out m_latestConstraintDate);

            a_reader.Read(out val);
            m_latestConstraint = (LatestConstraintEnum)val;

            m_productionInfo = DeserializeProductionInfo(a_reader);

            a_reader.Read(out m_setupColor);

            a_reader.Read(out m_standardRunSpan);
            a_reader.Read(out m_standardSetupSpan);

            a_reader.Read(out m_batchCode);

            m_resourceRequirements = new ResourceRequirementManager(a_reader, a_idGen);
            m_activities = new InternalActivityManager(a_reader, a_idGen);
            m_vesselTypeRequirementClaims = new VesselTypeRequirementsCollection(a_reader);
            m_vesselTypeRequirementReleases = new VesselTypeRequirementsCollection(a_reader);
            m_internalOperationFlags = new BoolVector32(a_reader);

            //Store references info for use later
            m_referenceInfo = new ReferenceInfo(m_latestConstraint, a_reader);

            a_reader.Read(out m_latestConstraintDescription);
            a_reader.Read(out m_latestConstraintResourceName);

            a_reader.Read(out m_productCode);

            m_splitInfo = new AutoSplitInfo(a_reader);
        }
        else if (a_reader.VersionNumber >= 12300)
        {
            a_reader.Read(out m_carryingCost);
            a_reader.Read(out m_compatibilityCode);
            a_reader.Read(out m_overlapTransferQty);
            a_reader.Read(out decimal minAutoSplitQty);
            a_reader.Read(out string setupCode);
            a_reader.Read(out m_setupNumber);

            a_reader.Read(out int val);
            m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

            ReadKeepSuccessorsTimeLimit(a_reader);
            a_reader.Read(out m_latestConstraintDate);

            a_reader.Read(out val);
            m_latestConstraint = (LatestConstraintEnum)val;

            m_productionInfo = DeserializeProductionInfo(a_reader);

            a_reader.Read(out m_setupColor);

            a_reader.Read(out m_standardRunSpan);
            a_reader.Read(out m_standardSetupSpan);

            a_reader.Read(out m_batchCode);

            m_resourceRequirements = new ResourceRequirementManager(a_reader, a_idGen);
            m_activities = new InternalActivityManager(a_reader, a_idGen);
            m_vesselTypeRequirementClaims = new VesselTypeRequirementsCollection(a_reader);
            m_vesselTypeRequirementReleases = new VesselTypeRequirementsCollection(a_reader);
            m_internalOperationFlags = new BoolVector32(a_reader);

            //Store references info for use later
            m_referenceInfo = new ReferenceInfo(m_latestConstraint, a_reader);

            a_reader.Read(out m_latestConstraintDescription);
            a_reader.Read(out m_latestConstraintResourceName);

            a_reader.Read(out m_productCode);
        }
        else
        {
            if (base.Name.Length <= 48)
            {
                m_productCode = base.Name; //For backwards compatibility
            }

            #region 12030
            if (a_reader.VersionNumber >= 12030)
            {
                a_reader.Read(out m_carryingCost);
                a_reader.Read(out m_compatibilityCode);
                a_reader.Read(out m_overlapTransferQty);
                a_reader.Read(out decimal minAutoSplitQty);
                a_reader.Read(out string setupCode);
                a_reader.Read(out m_setupNumber);

                int val;
                a_reader.Read(out val);
                m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

                ReadKeepSuccessorsTimeLimit(a_reader);
                a_reader.Read(out m_latestConstraintDate);

                a_reader.Read(out val);
                m_latestConstraint = (LatestConstraintEnum)val;

                m_productionInfo = DeserializeProductionInfo(a_reader);

                a_reader.Read(out m_setupColor);

                a_reader.Read(out m_standardRunSpan);
                a_reader.Read(out m_standardSetupSpan);

                a_reader.Read(out m_batchCode);

                m_resourceRequirements = new ResourceRequirementManager(a_reader, a_idGen);
                m_activities = new InternalActivityManager(a_reader, a_idGen);
                m_vesselTypeRequirementClaims = new VesselTypeRequirementsCollection(a_reader);
                m_vesselTypeRequirementReleases = new VesselTypeRequirementsCollection(a_reader);
                m_internalOperationFlags = new BoolVector32(a_reader);

                //Store references info for use later
                m_referenceInfo = new ReferenceInfo(m_latestConstraint, a_reader);

                a_reader.Read(out m_latestConstraintDescription);
                a_reader.Read(out m_latestConstraintResourceName);
            }
            #endregion

            #region 715
            else if (a_reader.VersionNumber >= 715)
            {
                a_reader.Read(out m_carryingCost);
                a_reader.Read(out m_compatibilityCode);
                a_reader.Read(out m_overlapTransferQty);
                a_reader.Read(out decimal minAutoSplitQty);
                a_reader.Read(out long materialPostProcessingSpanTicks);
                a_reader.Read(out string setupCode);
                a_reader.Read(out m_setupNumber);

                int val;
                a_reader.Read(out val);
                m_successorProcessing = (OperationDefs.successorProcessingEnumeration)val;

                ReadKeepSuccessorsTimeLimit(a_reader);
                a_reader.Read(out m_latestConstraintDate);

                a_reader.Read(out val);
                m_latestConstraint = (LatestConstraintEnum)val;

                m_productionInfo = DeserializeProductionInfo(a_reader);

                a_reader.Read(out m_setupColor);

                a_reader.Read(out m_standardRunSpan);
                a_reader.Read(out m_standardSetupSpan);

                a_reader.Read(out m_batchCode);

                m_resourceRequirements = new ResourceRequirementManager(a_reader, a_idGen);
                m_activities = new InternalActivityManager(a_reader, a_idGen);
                m_vesselTypeRequirementClaims = new VesselTypeRequirementsCollection(a_reader);
                m_vesselTypeRequirementReleases = new VesselTypeRequirementsCollection(a_reader);
                m_internalOperationFlags = new BoolVector32(a_reader);

                //Store references info for use later
                m_referenceInfo = new ReferenceInfo(m_latestConstraint, a_reader);

                a_reader.Read(out m_latestConstraintDescription);
                a_reader.Read(out m_latestConstraintResourceName);
            }
        }
        #endregion

        ValidateUsages();
    }

    protected virtual ProductionInfo DeserializeProductionInfo(IReader a_reader)
    {
        return new ProductionInfo(a_reader);
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(m_carryingCost);
        a_writer.Write(m_compatibilityCode);
        a_writer.Write(m_overlapTransferQty);
        a_writer.Write(m_setupNumber);
        a_writer.Write((int)m_successorProcessing);
        a_writer.Write(m_keepSuccessorTimeLimit);
        a_writer.Write(m_latestConstraintDate);
        a_writer.Write((int)m_latestConstraint);

        m_productionInfo.Serialize(a_writer);
        a_writer.Write(m_setupColor);

        a_writer.Write(m_standardRunSpan);
        a_writer.Write(m_standardSetupSpan);

        a_writer.Write(m_batchCode);

        m_resourceRequirements.Serialize(a_writer);
        m_activities.Serialize(a_writer);
        m_vesselTypeRequirementClaims.Serialize(a_writer);
        m_vesselTypeRequirementReleases.Serialize(a_writer);
        m_internalOperationFlags.Serialize(a_writer);

        //References
        ReferenceInfo newInfo = new ReferenceInfo(this);
        newInfo.Serialize(a_writer);

        a_writer.Write(m_latestConstraintDescription);
        a_writer.Write(m_latestConstraintResourceName);
        a_writer.Write(m_productCode);

        m_splitInfo.Serialize(a_writer);
        a_writer.Write((int)m_setupSplitType);
        a_writer.Write(m_campaignCode);

        if (m_overlapResourceReleaseTimes == null)
        {
            a_writer.Write(0);
        }
        else
        {
            a_writer.Write(m_overlapResourceReleaseTimes.Count);
            foreach ((Resource res, long releaseTicks) in m_overlapResourceReleaseTimes)
            {
                res.Id.Serialize(a_writer);
                a_writer.Write(releaseTicks);
            }
        }

        a_writer.Write(m_sequenceHeadStartDays);
    }

    public new const int UNIQUE_ID = 307;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    private ReferenceInfo m_referenceInfo;

    private class ReferenceInfo : IPTSerializable
    {
        internal BaseId LatestConstraintPredecessorOperationId = BaseId.NULL_ID;
        internal BaseId LatestConstraintMaterialRequirementId = BaseId.NULL_ID;
        internal Dictionary<BaseId, long> OverlapResourcesReleases = new ();

        internal ReferenceInfo(LatestConstraintEnum a_latestConstraint, IReader a_reader)
        {
            if (a_reader.VersionNumber >= 12545)
            {
                switch (a_latestConstraint)
                {
                    case LatestConstraintEnum.MaterialRequirement:
                        LatestConstraintMaterialRequirementId = new BaseId(a_reader);
                        break;
                    case LatestConstraintEnum.PredecessorOperation:
                        LatestConstraintPredecessorOperationId = new BaseId(a_reader);
                        break;
                }
            }
            else
            {
                switch (a_latestConstraint)
                {
                    case LatestConstraintEnum.PredecessorOperation:
                        LatestConstraintPredecessorOperationId = new BaseId(a_reader);
                        break;
                }
            }
        }

        internal ReferenceInfo(InternalOperation a_op)
        {
            if (a_op.LatestConstraintInternal == LatestConstraintEnum.PredecessorOperation)
            {
                LatestConstraintMaterialRequirementId = a_op.m_latestConstraintPredecessorOperation.Id;
            }
            else if (a_op.LatestConstraintInternal == LatestConstraintEnum.MaterialRequirement)
            {
                LatestConstraintMaterialRequirementId = a_op.m_latestConstraintMaterialRequirement.Id;
            }
        }

        public void Serialize(IWriter a_writer)
        {
            if (LatestConstraintPredecessorOperationId != BaseId.NULL_ID)
            {
                LatestConstraintPredecessorOperationId.Serialize(a_writer);
            }
            if (LatestConstraintMaterialRequirementId != BaseId.NULL_ID)
            {
                LatestConstraintMaterialRequirementId.Serialize(a_writer);
            }
        }

        public int UniqueId => 0;
    }

    internal override void RestoreReferences(BaseOperationManager a_opManager, ManufacturingOrder a_mo, WarehouseManager a_warehouseManager, ItemManager a_items, SalesOrderManager a_sdSalesOrderManager, TransferOrderManager a_sdTransferOrderManager, AttributeManager a_sdAttributeManager, BaseIdGenerator a_sdIdGen, PlantManager a_plantManager, CapabilityManager a_capabilityManager)
    {
        base.RestoreReferences(a_opManager, a_mo, a_warehouseManager, a_items, a_sdSalesOrderManager, a_sdTransferOrderManager, a_sdAttributeManager, a_sdIdGen, a_plantManager, a_capabilityManager);
        if (m_latestConstraint == LatestConstraintEnum.PredecessorOperation)
        {
            m_latestConstraintPredecessorOperation = a_opManager[m_referenceInfo.LatestConstraintPredecessorOperationId];
            if (m_latestConstraintPredecessorOperation == null)
            {
                m_latestConstraint = LatestConstraintEnum.Unknown;

                #if DEBUG
                throw new DebugException(string.Format("Operation has LatestConstraint of PredecessorOperation but predecessor Operation with Id '{0}' could not be found.", m_referenceInfo.LatestConstraintPredecessorOperationId));
                #endif
            }
        }
        else if (m_latestConstraint == LatestConstraintEnum.MaterialRequirement)
        {
            m_latestConstraintMaterialRequirement = MaterialRequirements.GetById(m_referenceInfo.LatestConstraintMaterialRequirementId);
            if (m_latestConstraintMaterialRequirement == null)
            {
                m_latestConstraint = LatestConstraintEnum.Unknown;
            }
        }

        for (int i = 0; i < m_activities.Count; i++)
        {
            m_activities.GetByIndex(i).RestoreReferences(this);
        }

        if (m_referenceInfo.OverlapResourcesReleases.Count > 0)
        {
            m_overlapResourceReleaseTimes = new (m_referenceInfo.OverlapResourcesReleases.Count);
            foreach ((BaseId resId, long releaseTicks) in m_referenceInfo.OverlapResourcesReleases)
            {
                if (a_plantManager.GetResource(resId) is Resource res)
                {
                    m_overlapResourceReleaseTimes.Add(res, releaseTicks);
                }
            }
        }
       
        ResourceRequirements.RestoreReferences(this, a_capabilityManager, a_plantManager);

        m_referenceInfo = null;
    }
    #endregion

    #region Declarations
    internal class InternalOperationException : PTException
    {
        internal InternalOperationException(string a_message)
            : base(a_message) { }
    }
    #endregion

    #region Construction
    protected InternalOperation(BaseId a_id,
                                ManufacturingOrder a_manufacturingOrder,
                                JobT.ResourceOperation a_jobTInternalOperation,
                                CapabilityManager a_machineCapabilities,
                                ScenarioDetail a_sd,
                                bool a_isErpUpdate,
                                IScenarioDataChanges a_dataChanges,
                                ISystemLogger a_errorReporter,
                                bool a_createDefaultActivity,
                                UserFieldDefinitionManager a_udfManager,
                                bool a_autoDeleteOperationAttributes)
        : base(a_id, a_manufacturingOrder, a_jobTInternalOperation, a_sd, a_sd.IdGen, a_udfManager, a_autoDeleteOperationAttributes, a_dataChanges)
    {
        m_activities = new InternalActivityManager(a_sd.IdGen);
        m_productionInfo = new ProductionInfo();
        m_overlapResourceReleaseTimes = new ();

        if (a_jobTInternalOperation.PlanningScrapPercentSet)
        {
            m_productionInfo.PlanningScrapPercent = a_jobTInternalOperation.PlanningScrapPercent;
        }

        AutoSplit = a_jobTInternalOperation.AutoSplit;
        CanPause = a_jobTInternalOperation.CanPause;
        TimeBasedReporting = a_jobTInternalOperation.TimeBasedReporting;
        CanSubcontract = a_jobTInternalOperation.CanSubcontract;
        CarryingCost = a_jobTInternalOperation.CarryingCost;
        CompatibilityCode = a_jobTInternalOperation.CompatibilityCode;
        UseCompatibilityCode = a_jobTInternalOperation.UseCompatibilityCode;
        CycleSpan = a_jobTInternalOperation.CycleSpan;

        DeductScrapFromRequired = a_jobTInternalOperation.DeductScrapFromRequired;
        KeepSuccessorsTimeLimit = a_jobTInternalOperation.KeepSuccessorsTimeLimit;
        OverlapTransferQty = a_jobTInternalOperation.OverlapTransferQty;
        PostProcessingSpan = a_jobTInternalOperation.PostProcessingSpan;
        CleanSpan = a_jobTInternalOperation.CleanSpan;
        CleanoutGrade = a_jobTInternalOperation.CleanOutGrade;
        CleanoutCost = a_jobTInternalOperation.CleanoutCost;
        StorageSpan = a_jobTInternalOperation.StorageTimeSpan;
        SetupNumber = a_jobTInternalOperation.SetupNumber;
        SetupColor = a_jobTInternalOperation.SetupColor;
        SetupSpan = a_jobTInternalOperation.SetupSpan;
        ProductionSetupCost = a_jobTInternalOperation.ProductionSetupCost;
        m_batchCode = a_jobTInternalOperation.BatchCode;
        CanResize = a_jobTInternalOperation.CanResize;
        ProductCode = a_jobTInternalOperation.ProductCode;
        AutoSplitInfo.Update(a_jobTInternalOperation);
        SetupSplitType = a_jobTInternalOperation.SetupSplitType;
        PreventSplitsFromIncurringSetup = a_jobTInternalOperation.PreventSplitsFromIncurringSetup;
        PreventSplitsFromIncurringClean = a_jobTInternalOperation.PreventSplitsFromIncurringClean;
        AllowSameLotInNonEmptyStorageArea = a_jobTInternalOperation.AllowSameLotInNonEmptyStorageArea;

        // LARRY_TANK_DEBUG: Move this into the producitoninfo.

        ProductionInfo.OnlyAllowManualUpdatesToCycleSpan = a_jobTInternalOperation.ProductionInfoManualUpdateOnlyFlags.CycleManualUpdateOnly;
        ProductionInfo.OnlyAllowManualUpdatesToQtyPerCycle = a_jobTInternalOperation.ProductionInfoManualUpdateOnlyFlags.QtyPerCycle;
        ProductionInfo.OnlyAllowManualUpdatesToSetupSpan = a_jobTInternalOperation.ProductionInfoManualUpdateOnlyFlags.SetupManualUpdateOnly;
        ProductionInfo.OnlyAllowManualUpdatesToPostProcessingSpan = a_jobTInternalOperation.ProductionInfoManualUpdateOnlyFlags.PostProcessingManualUpdateOnly;
        ProductionInfo.OnlyAllowManualUpdatesToCleanSpan = a_jobTInternalOperation.ProductionInfoManualUpdateOnlyFlags.CleanManualUpdateOnly;
        ProductionInfo.OnlyAllowManualUpdatesToPlanningScrapPercent = a_jobTInternalOperation.ProductionInfoManualUpdateOnlyFlags.PlanningScrapPercent;
        ProductionInfo.OnlyAllowManualUpdatesToResourceRequirements = a_jobTInternalOperation.ProductionInfoManualUpdateOnlyFlags.ResourceRequirements;
        //ProductionInfo.OnlyAllowManualUpdatesToMaterialPostProcessingSpan = a_jobTInternalOperation.ProductionInfoManualUpdateOnlyFlags.MaterialPostProcessingManualUpdateOnly;

        WholeNumberSplits = a_jobTInternalOperation.WholeNumberSplits;
        SuccessorProcessing = a_jobTInternalOperation.SuccessorProcessing;

        //First parse the new RR and create them with an ID
        ResourceRequirement primaryRequirement = null;
        List<ResourceRequirement> newRequirements = new();
        int newRRIdIdx = 1;
        //Add Resource Requirements
        for (int resReqtI = 0; resReqtI < a_jobTInternalOperation.ResourceRequirementCount; resReqtI++)
        {
            JobT.InternalOperation.ResourceRequirement jobTResourceRequirement = a_jobTInternalOperation.GetResourceRequirement(resReqtI);
            ResourceRequirement newRequirement = new(new BaseId(newRRIdIdx), jobTResourceRequirement, a_machineCapabilities, a_sd, this, a_dataChanges, a_errorReporter);
            newRequirements.Add(newRequirement);

            if (a_jobTInternalOperation.PrimaryResourceRequirement.ExternalId == jobTResourceRequirement.ExternalId)
            {
                primaryRequirement = newRequirement;
            }

            newRRIdIdx++;
        }

        //Now that we have the primary ID, create the manager and add the new RR
        m_resourceRequirements = new ResourceRequirementManager(this, primaryRequirement.Id);
        foreach (ResourceRequirement newRR in newRequirements)
        {
            m_resourceRequirements.Add(newRR);
        }

        ResourceRequirement primaryRR = m_resourceRequirements.PrimaryResourceRequirement;
        if (primaryRR.UsageStart > MainResourceDefs.usageEnum.Run)
        {
            throw new PTValidationException("4475", new object[] { ExternalId, Job.ExternalId });
        }


        ValidateUsages();

        QtyPerCycle = ManufacturingOrder.ScenarioDetail.ScenarioOptions.RoundQty(a_jobTInternalOperation.QtyPerCycle);
        if (QtyPerCycle == decimal.Zero)
        {
            throw new ValidationException("2108", new object[] { ExternalId, Name, Description });
        }

        if (a_jobTInternalOperation.InternalActivityCount > 0)
        {
            for (int internalActivityI = 0; internalActivityI < a_jobTInternalOperation.InternalActivityCount; internalActivityI++)
            {
                JobT.InternalActivity jobTInternalActivity = a_jobTInternalOperation.GetInternalActivity(internalActivityI);
                InternalActivity activity = new InternalActivity(m_activities.IdGen.NextID(), a_sd, this, a_jobTInternalOperation, jobTInternalActivity, a_isErpUpdate);
                m_activities.Add(activity);
            }
        }
        else // This else means are no Activities since this constructor is only called when creating a tempMO to replace existing one
             // so add Activities for the operation so it can schedule
        {
            //Get the job reference from the MO to find the OPs, we can't assume that the MO references is
            //an existing MO or if it's a temp MO being created during an update/import.
            BaseOperation existingOp = ManufacturingOrder.Job.ManufacturingOrders.GetByExternalId(ManufacturingOrder.ExternalId)?.OperationManager[ExternalId];
            if (existingOp is InternalOperation internalOp)
            {
                foreach (InternalActivity activity in internalOp.Activities)
                {
                    InternalActivity newActivity = activity.Clone(this);
                    newActivity.RequiredFinishQty = RequiredFinishQty / internalOp.Activities.Count;
                    Activities.Add(newActivity);
                }
            }
            else
            {
                //Job either didn't exist or didn't have Ops/Activities. add one for the operation so it can schedule
                InternalActivity activity = new InternalActivity(1, Activities.IdGen.NextID(), this, a_jobTInternalOperation);
                activity.RequiredFinishQty = RequiredFinishQty;
                Activities.Add(activity);
            }
        }
        
        //Set these last since they depend on CycleSpan and QtyPerCycle
        if (a_jobTInternalOperation.StandardRunSpan.Ticks == 0)
        {
            StandardRunSpan = RunSpan.Add(PostProcessingSpan);
        }
        else
        {
            StandardRunSpan = a_jobTInternalOperation.StandardRunSpan;
        }

        if (a_jobTInternalOperation.StandardSetupSpan.Ticks == 0)
        {
            StandardSetupSpan = SetupSpan;
        }
        else
        {
            StandardSetupSpan = a_jobTInternalOperation.StandardSetupSpan;
        }

        if (TimeSpan.FromDays(a_jobTInternalOperation.SequenceHeadStartDays).Ticks != TimeSpan.FromDays(SequenceHeadStartDays).Ticks)
        {
            SequenceHeadStartDays = a_jobTInternalOperation.SequenceHeadStartDays;
        }
    }

    /// <summary>
    /// Call this function to verify the used during of the primary resource requirement is greater than or equal to the used durings of all resource requirements.
    /// </summary>
    private void ValidateUsages()
    {
        List<MainResourceDefs.Usage> usageList = new ();
        for (int resReqI = 0; resReqI < m_resourceRequirements.Count; ++resReqI)
        {
            ResourceRequirement rr = m_resourceRequirements.GetByIndex(resReqI);
            usageList.Add(rr.GetUsage());
        }

        MainResourceDefs.Usage.ValidateUsages(usageList, m_resourceRequirements.PrimaryResourceRequirementIndex);
    }

    /// <summary>
    /// Create an InternalOperation from an existing Operation.  Used for BreakOfffs.
    /// </summary>
    protected InternalOperation(BaseId a_id, InternalOperation a_sourceOp, ManufacturingOrder a_parentMo, BaseIdGenerator a_idGen)
        : base(a_id, a_sourceOp, a_parentMo, a_idGen)
    {
        m_overlapResourceReleaseTimes = new ();
        m_activities = new InternalActivityManager(a_parentMo.ScenarioDetail.IdGen);
        m_productionInfo = new ProductionInfo();
        m_productionInfo.PlanningScrapPercent = a_sourceOp.PlanningScrapPercent; // Moved here from BaseOperation

        AutoSplit = a_sourceOp.AutoSplit;
        AutoSplitInfo = a_sourceOp.AutoSplitInfo;
        CanPause = a_sourceOp.CanPause;
        TimeBasedReporting = a_sourceOp.TimeBasedReporting;
        CanSubcontract = a_sourceOp.CanSubcontract;
        CarryingCost = a_sourceOp.CarryingCost;
        CompatibilityCode = a_sourceOp.CompatibilityCode;
        UseCompatibilityCode = a_sourceOp.UseCompatibilityCode;
        CycleSpan = a_sourceOp.CycleSpan;
        DeductScrapFromRequired = a_sourceOp.DeductScrapFromRequired;
        KeepSuccessorsTimeLimit = a_sourceOp.KeepSuccessorsTimeLimit;
        OverlapTransferQty = a_sourceOp.OverlapTransferQty;
        PostProcessingSpan = a_sourceOp.PostProcessingSpan;
        CleanSpan = a_sourceOp.CleanSpan;
        CleanoutGrade = a_sourceOp.CleanoutGrade;
        CleanoutCost = a_sourceOp.CleanoutCost;
        StorageSpan = a_sourceOp.StorageSpan;
        SetupNumber = a_sourceOp.SetupNumber;
        SetupColor = a_sourceOp.SetupColor;
        SetupSpan = a_sourceOp.SetupSpan;
        ProductionSetupCost = a_sourceOp.ProductionSetupCost;
        CampaignCode = a_sourceOp.CampaignCode;

        m_batchCode = a_sourceOp.m_batchCode;
        CanResize = a_sourceOp.CanResize;

        WholeNumberSplits = a_sourceOp.WholeNumberSplits;
        SuccessorProcessing = a_sourceOp.SuccessorProcessing;
        StandardSetupSpan = a_sourceOp.StandardSetupSpan;
        StandardRunSpan = a_sourceOp.StandardRunSpan;
        m_resourceRequirements = new ResourceRequirementManager(this, a_sourceOp.ResourceRequirements);

        //Add Activities
        for (int a = 0; a < a_sourceOp.Activities.Count; a++)
        {
            InternalActivity sourceActivity = a_sourceOp.Activities.GetByIndex(a);
            InternalActivity newActivity = new InternalActivity(a_parentMo.ScenarioDetail.IdGen.NextID(), sourceActivity, this);
            Activities.Add(newActivity);
        }

        PreventSplitsFromIncurringSetup = a_sourceOp.PreventSplitsFromIncurringSetup;
        PreventSplitsFromIncurringClean = a_sourceOp.PreventSplitsFromIncurringClean;

        QtyPerCycle = a_sourceOp.QtyPerCycle;
        SequenceHeadStartDays = a_sourceOp.SequenceHeadStartDays;
    }
    #endregion

    #region Overrides
    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    [Browsable(false)]
    public override string DefaultNamePrefix => "InternalOperation";

    /// <summary>
    /// The sum of the Work Content of the Activities
    /// </summary>
    public override TimeSpan WorkContent
    {
        get
        {
            TimeSpan work = new (0);
            for (int i = 0; i < Activities.Count; i++)
            {
                InternalActivity activity = Activities.GetByIndex(i);
                work = work.Add(activity.WorkContent);
            }

            return work;
        }
    }

    /// <summary>
    /// Expected Run Hours plus Expected Setup Hours.
    /// </summary>
    public decimal ExpectedHours => ExpectedRunHours + ExpectedSetupHours;

    /// <summary>
    /// Indicates expected performance against standard.
    /// Expected Hours divided by Standard Hours times 100.
    /// If Standard Hours is zero then this returns 100.
    /// </summary>
    public int PercentOfStandard
    {
        get
        {
            decimal std = StandardHours;
            if (std > 0)
            {
                return (int)Math.Round(ExpectedHours / std * 100, MidpointRounding.AwayFromZero);
            }

            return 100;
        }
    }

    /// <summary>
    /// The total number of hours expected to be worked based on current Cycle Span and quantity and hours reported.
    /// If the Operation is finished then this is the Reported Run Hours.
    /// If unfinished and scheduled, then this is the sum of the Scheduled Run Span, Scheduled Post-Processing Span, Reported Run Span, and Reported Post Processing Span of all Activities.
    /// If unfinished and unscheduled, then this is the maximum of zero and the Scheduling Hours minus the Reported Setup, Run and Post-Processing Hours.
    /// </summary>
    public decimal ExpectedRunHours
    {
        get
        {
            if (Finished)
            {
                return ReportedRunHours + ReportedPostProcessingHours;
            }

            if (Scheduled)
            {
                //Add up activity times
                decimal total = 0;
                for (int i = 0; i < Activities.Count; ++i)
                {
                    InternalActivity act = Activities.GetByIndex(i);

                    decimal processingHrs = Convert.ToDecimal(act.ReportedRunSpan.TotalHours);
                    decimal postProcessingHrs = Convert.ToDecimal(act.ReportedPostProcessingSpan.TotalHours);
                    if (act.Batch != null)
                    {
                        processingHrs += Convert.ToDecimal(act.Batch.ProcessingCapacitySpan.TimeSpan.TotalHours);
                        postProcessingHrs += Convert.ToDecimal(act.Batch.PostProcessingSpan.TimeSpan.TotalHours);
                    }


                    total += processingHrs + postProcessingHrs;
                }

                return total;
            }

            return Math.Max(0, SchedulingHours - ReportedSetupHours - ReportedRunHours - ReportedPostProcessingHours);
        }
    }

    /// <summary>
    /// The total number of setup hours expected to be worked based on current Cycle Span and quantity and hours reported.
    /// </summary>
    public decimal ExpectedSetupHours
    {
        get
        {
            if (Finished)
            {
                return ReportedSetupHours;
            }

            if (Scheduled)
            {
                //Add up activity times
                decimal total = 0;

                for (int i = 0; i < Activities.Count; ++i)
                {
                    InternalActivity ia = Activities.GetByIndex(i);
                    total += (decimal)ia.ScheduledSetupSpan.Add(ia.ReportedSetupSpan).TotalHours;
                }

                return total;
            }

            return Math.Max(0, (decimal)SetupSpan.TotalHours - ReportedSetupHours);
        }
    }
 /// <summary>
    /// The total number of cleans hours expected to be worked based on current Cycle Span and quantity and hours reported.
    /// </summary>
    public decimal ExpectedCleanHours
    {
        get
        {
            if (Finished)
            {
                return ReportedCleanHours;
            }

            if (Scheduled)
            {
                //Add up activity times
                decimal total = 0;

                for (int i = 0; i < Activities.Count; ++i)
                {
                    InternalActivity ia = Activities.GetByIndex(i);
                    total += (decimal)ia.ScheduledCleanSpan.Add(new TimeSpan(ia.ReportedClean)).TotalHours;
                }

                return total;
            }

            return Math.Max(0, (decimal)CleanSpan.TotalHours - ReportedCleanHours);
        }
    }

    /// <summary>
    /// The sum of the Reported Run hours of all the activities.
    /// </summary>
    public decimal ReportedRunHours
    {
        get
        {
            decimal total = 0;

            for (int i = 0; i < Activities.Count; ++i)
            {
                InternalActivity ia = Activities.GetByIndex(i);
                total += (decimal)ia.ReportedRunSpan.TotalHours;
            }

            return total;
        }
    }

    /// <summary>
    /// The sum of the Reported Setup Hours of all the activities.
    /// </summary>
    public decimal ReportedSetupHours
    {
        get
        {
            decimal total = 0;

            for (int i = 0; i < Activities.Count; ++i)
            {
                InternalActivity ia = Activities.GetByIndex(i);
                total += (decimal)ia.ReportedSetupSpan.TotalHours;
            }

            return total;
        }
    }
    /// <summary>
    /// The sum of the Reported Cleans Hours of all the activities.
    /// </summary>
    public decimal ReportedCleanHours
    {
        get
        {
            decimal total = 0;

            for (int i = 0; i < Activities.Count; ++i)
            {
                InternalActivity ia = Activities.GetByIndex(i);
                total += (decimal)new TimeSpan(ia.ReportedClean).TotalHours;
            }

            return total;
        }
    }

    /// <summary>
    /// The sum of the Reported PostProcessing Hours of all the activities.
    /// </summary>
    public decimal ReportedPostProcessingHours
    {
        get
        {
            decimal total = 0;

            for (int i = 0; i < Activities.Count; ++i)
            {
                InternalActivity ia = Activities.GetByIndex(i);
                total += (decimal)ia.ReportedPostProcessingSpan.TotalHours;
            }

            return total;
        }
    }    
    
    /// <summary>
    /// The sum of the Reported PostProcessing Hours of all the activities.
    /// </summary>
    public decimal ReportedStorageHours
    {
        get
        {
            decimal total = 0;

            for (int i = 0; i < Activities.Count; ++i)
            {
                InternalActivity ia = Activities.GetByIndex(i);
                total += (decimal)ia.ReportedStorageSpan.TotalHours;
            }

            return total;
        }
    }

    /// <summary>
    /// The Production Status of the Activity that is furthest along.
    /// </summary>
    public InternalActivityDefs.productionStatuses MaxActivityProductionStatus
    {
        get
        {
            InternalActivityDefs.productionStatuses maxSoFar = InternalActivityDefs.productionStatuses.Waiting;

            for (int i = 0; i < Activities.Count; ++i)
            {
                InternalActivity ia = Activities.GetByIndex(i);
                if ((int)ia.ProductionStatus > (int)maxSoFar)
                {
                    maxSoFar = ia.ProductionStatus;
                }
            }

            return maxSoFar;
        }
    }

    /// <summary>
    /// The sum of the Operation's Setup Span, Run Span, and Post-Processing Span.
    /// </summary>
    public override decimal SchedulingHours => Convert.ToDecimal(SetupSpan.TotalHours + RunSpan.TotalHours + PostProcessingSpan.TotalHours);

    #region Standard Hours
    /// <summary>
    /// The standard number of work hours required to complete the Operation.
    /// This value has no effect on scheduling but is used to compare scheduled versus standard hours.
    /// If this value is not set explicitly then it is set based on the scheduled hours when the operation is created.
    /// Calcualated as the StandardRunSpan plus the Standard Setup Span.
    /// </summary>
    public override decimal StandardHours
    {
        get => (decimal)(StandardRunSpan.TotalHours + StandardSetupSpan.TotalHours);
        protected set => StandardRunSpan = TimeSpan.FromHours((double)value);
    }

    /// <summary>
    /// StandardRunSpan plus StandardSetupSpan
    /// </summary>
    internal TimeSpan StandardSpan => TimeSpan.FromTicks(StandardRunSpan.Ticks + StandardSetupSpan.Ticks);

    /// <summary>
    /// Reported Setup+Run+PostProcessing hours.
    /// </summary>
    public decimal ReportedHours => ReportedSetupHours + ReportedRunHours + ReportedPostProcessingHours;

    private TimeSpan m_standardRunSpan;

    /// <summary>
    /// Used to calculate performane by comparing against actual values.  This has no effect on scheduling.
    /// If not set explicitly, this is set to the Operation RunSpan plus the PostProcessingSpan when the Operation is created or updated.
    /// </summary>
    public TimeSpan StandardRunSpan
    {
        get => m_standardRunSpan;
        private set => m_standardRunSpan = value;
    }

    private TimeSpan m_standardSetupSpan;

    /// <summary>
    /// Used to calculate performane by comparing against actual values.  This has no effect on scheduling.
    /// </summary>
    public TimeSpan StandardSetupSpan
    {
        get => m_standardSetupSpan;
        protected set => m_standardSetupSpan = value;
    }

    /// <summary>
    /// If Standard Hours are specified then this is the Expected Hours divided by the Standard Hours.  Smaller values mean the Job ran or is running faster than standard.
    /// </summary>
    public int GetPercentOfStandardHrs()
    {
        decimal stdHours = (decimal)StandardRunSpan.Add(StandardSetupSpan).TotalHours;
        decimal expectedHours = ExpectedRunHours + ExpectedSetupHours;
        if (stdHours == 0)
        {
            return 0;
        }

        return (int)(expectedHours / stdHours * 100);
    }
    #endregion Standard Hours

    /// <summary>
    /// The expected time to run the full start quantity.  This is calculated by multiplying Cycle Span times Required Start Qty.
    /// </summary>
    public abstract TimeSpan RunSpan { get; }

    /// <summary>
    /// Whether any of the Operation's Activities are Started, including finished activities.
    /// </summary>
    public bool Started
    {
        get
        {
            for (int i = 0; i < Activities.Count; i++)
            {
                if (Activities.GetByIndex(i).Started)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Whether any of the Operation's Activities are Started. This excludes Finished activities.
    /// </summary>
    public bool InProcess
    {
        get
        {
            for (int i = 0; i < Activities.Count; i++)
            {
                InternalActivity act = Activities.GetByIndex(i);
                if (!act.Finished && act.Started)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public decimal GetScrapQty()
    {
        decimal scrap = 0;
        for (int i = 0; i < Activities.Count; i++)
        {
            scrap += Activities.GetByIndex(i).ReportedScrapQty;
        }

        return scrap;
    }

    /// <summary>
    /// Returns the TransferSpan of the resource used that has the longest TransferSpan.
    /// </summary>
    public override long GetLongestResourceTransferTicks()
    {
        long longestTransfer = 0;

        if (!Scheduled) //Can't determine the transfer span since it's on the resource.
        {
            return 0;
        }

        for (int i = 0; i < Activities.Count; i++)
        {
            InternalActivity activity = Activities.GetByIndex(i);
            if (activity.ResourceTransferSpanTicks > longestTransfer)
            {
                longestTransfer = activity.ResourceTransferSpanTicks;
            }
        }

        return longestTransfer;
    }
    #endregion

    #region Shared Properties
    //NEWOPFIELD Bookmark

    private BoolVector32 m_internalOperationFlags;
    private const int AutoSplitIdx = 0;
    private const int CanPauseIdx = 1;
    private const int CanSubcontractIdx = 2;
    private const int WholeNumberSplitsIdx = 4;
    private const int DeductScrapFromRequiredIdx = 5;
    private const int TimeBasedReportingIdx = 6;
    private const int UseCompatibilityCodeIdx = 7;
    private const int CanResizeIdx = 8;
    

    private const int UNUSED_2 = 9; // Has been reset to false. Replaced with OperationProductionInfo
    private const int UNUSED_3 = 10; // Has been reset to false. Replaced with OperationProductionInfo

    private const int Unused_4 = 11; //MaterialPostProcessingSpanIdx replaced with production info values;
    private const short c_preventSplitsFromIncurringSetup = 12;
    private const short c_preventSplitsFromIncurringClean = 13;
    private const short c_splitsOverrideEmptyStorageAreaConstraintIdx = 14;

    /// <summary>
    /// RESERVED FOR FUTURE USE If the operation is scheduled after its JITStartDateTime then we split the operation evenly into n activities where n is the number of eligible Resources.  Resulting split
    /// Activities are scheduled independent of each other and may schedule on the same or different Resources.  This also controls unsplitting. During Optimizations, if an Activity is found to have been
    /// split unnecessarily then it will be unsplit. This applies both to automatically and manually split activities.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    [Browsable(false)] //not yet
    public bool AutoSplit
    {
        get => m_internalOperationFlags[AutoSplitIdx];
        private set => m_internalOperationFlags[AutoSplitIdx] = value;
    }

    /// <summary>
    /// If true then Activities can be 'paused' for Offline Capacity Intervals. If false, Activities won't start until there is sufficient time to complete them in one continuous interval.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public bool CanPause
    {
        get => m_internalOperationFlags[CanPauseIdx];
        private set => m_internalOperationFlags[CanPauseIdx] = value;
    }

    /// <summary>
    /// Specifies whether Reported Run Span is subtracted from Scheduled Run Span rather than calculating Scheduled Run Span based on the Remaining Quantity.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public bool TimeBasedReporting
    {
        get => m_internalOperationFlags[TimeBasedReportingIdx];
        private set
        {
            if (TimeBasedReporting != value)
            {
                m_internalOperationFlags[TimeBasedReportingIdx] = value;
                ManufacturingOrder.ScenarioDetail.OperationsUpdated();
            }
        }
    }

    /// <summary>
    /// Indicates to the planner that this work can be subcontracted if necessary.  For display only.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public bool CanSubcontract
    {
        get => m_internalOperationFlags[CanSubcontractIdx];
        private set => m_internalOperationFlags[CanSubcontractIdx] = value;
    }

    #region Customizations
    #endregion Customizations

    private decimal m_carryingCost;

    /// <summary>
    /// Cost per unit per day after this Operation is finished.  This is for costs related to shrinkage, spoilage, raw materials, engineering changes and customer changes that may cause materials to be
    /// scrapped.  For display only.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public decimal CarryingCost
    {
        get => m_carryingCost;
        private set => m_carryingCost = value;
    }

    private string m_compatibilityCode = "";

    /// <summary>
    /// Used to restrict Resources to only perform compatible work at simulataneous times. If specified, then any scheduled Operation's CompatibilityCode must match the CompatibilityCode of other Operations
    /// scheduled on Resources with the same CompatibilityGroup.
    /// </summary>
    public string CompatibilityCode
    {
        get => m_compatibilityCode;
        private set => m_compatibilityCode = value;
    }

    /// <summary>
    /// Whether to use the compatibility code.
    /// </summary>
    public bool UseCompatibilityCode
    {
        get => m_internalOperationFlags[UseCompatibilityCodeIdx];
        private set => m_internalOperationFlags[UseCompatibilityCodeIdx] = value;
    }

    private string m_productCode = "";

    /// <summary>
    /// The code to use for specific product rules that override the default product rule.
    /// </summary>
    public string ProductCode
    {
        get => m_productCode;
        private set => m_productCode = value;
    }

    /// <summary>
    /// Time to perform one production cycle.  This is used with Qty Per Cycle to determine the run length of the Operation.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    [Required(true)]
    public TimeSpan CycleSpan
    {
        get => new (CycleSpanTicks);
        internal set => CycleSpanTicks = value.Ticks;
    }

    private decimal m_overlapTransferQty = 1;

    /// <summary>
    /// The number of parts moved to the next operation in each transfer batch.  Smaller values can be used to shorten flow times by making material available at successor operations more quickly.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public decimal OverlapTransferQty
    {
        get => m_overlapTransferQty;
        private set => m_overlapTransferQty = value;
    }

    /// <summary>
    /// Specifies the amount of time resources may be occupied after processing has completed, for instance you may use this to model cleanup time.  Whether a resource will be occupied also depends on the
    /// requirement's "Used During" setting which can be set to "setup", "setup and processing", or "setup, processing, and post-processing".
    /// The release of material is not constrained by cleanup time. To model post processing of material, for instance a cleanup time, use the MaterialPostProcessingSpan setting.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public TimeSpan PostProcessingSpan
    {
        get => new (PostProcessingSpanTicks);
        internal set => PostProcessingSpanTicks = value.Ticks;
    }

    public TimeSpan StorageSpan
    {
        get => new (StorageSpanTicks);
        internal set => StorageSpanTicks = value.Ticks;
    }

    public TimeSpan CleanSpan
    {
        get => new (CleanSpanTicks);
        internal set => CleanSpanTicks = value.Ticks;
    }

    private string m_batchCode; // [BATCH]

    /// <summary>
    /// On batch resources, operations with the same batch code are allowed to run in the same batch.
    /// </summary>
    public string BatchCode
    {
        get => m_batchCode;
        internal set => m_batchCode = value;
    }

    private decimal m_setupNumber;
    internal const decimal MAX_SETUP_NBR = 100000000; //to help selection of operations in the right direction -- ie. higher or lower than current

    /// <summary>
    /// Can be used to sequence Operations so as to try to changeover to the most similar product or in a gradually increasing or decreasing sequence.
    /// SetupNumber must be >=0 and <=100000000.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public decimal SetupNumber
    {
        get => m_setupNumber;

        internal set
        {
            if (value >= 0 && value <= MAX_SETUP_NBR)
            {
                m_setupNumber = value;
            }
        }
    }

    private Color m_setupColor = Color.Gray;

    /// <summary>
    /// Can be used to visually indicate whether Operations require special setup when switching from other Operations.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public Color SetupColor
    {
        get => m_setupColor;
        private set => m_setupColor = value;
    }

    /// <summary>
    /// Time for setting up each Resource that is used during the Operation's setup time.  See also: Resource Setup.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public TimeSpan SetupSpan
    {
        get => new (SetupSpanTicks);
        internal set => SetupSpanTicks = value.Ticks;
    }

    /// <summary>
    /// Whether Activities must be split into quantities with whole numbers.  (Only possible if the original quantity is a whole number.)
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public bool WholeNumberSplits
    {
        get => m_internalOperationFlags[WholeNumberSplitsIdx];
        private set => m_internalOperationFlags[WholeNumberSplitsIdx] = value;
    }

    /// <summary>
    /// Whether Reported Scrap should be deducted from the Required Finish Qty when determining the quantity to schedule.
    /// In some cases, more material is available and additional product can be produced to make up for the scrap.  In other cases this is not possible and the operation will finish short.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public bool DeductScrapFromRequired
    {
        get => m_internalOperationFlags[DeductScrapFromRequiredIdx];
        private set => m_internalOperationFlags[DeductScrapFromRequiredIdx] = value;
    }

    private OperationDefs.successorProcessingEnumeration m_successorProcessing = OperationDefs.successorProcessingEnumeration.NoPreference;

    /// <summary>
    /// Allows control over whether the successor operation must be scheduled on the same resource as this one. Use of this feature requires that the routing be linear within the predecessor and successor
    /// operation, that the operation only has 1 activity, and that the resource the predecessor ends up being scheduled on is eligible to perform the work required on the successor operation. KeepSuccessor
    /// means the successor operation will try to end up being scheduled on the same resource as the predecessor operation KeepSuccessorNoInterrupt means that not only will the successor operation try to end
    /// up on the same resource but it will also try to end up as the very next operation scheduled on the resource.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public OperationDefs.successorProcessingEnumeration SuccessorProcessing
    {
        get => m_successorProcessing;
        private set => m_successorProcessing = value;
    }

    /// <summary>
    /// Call Limit_KeepSuccessorsTimeLimit after setting this variable.
    /// </summary>
    private long m_keepSuccessorTimeLimit = TimeSpan.TicksPerDay * 7;

    /// <summary>
    /// The length of time the SuccessorProcessing setting remains valid for. After this length of time has passed without the successor operation finding a spot on the resource's schedule after its
    /// predecessor it may be scheduled on some other resource.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public TimeSpan KeepSuccessorsTimeLimit
    {
        get => new (m_keepSuccessorTimeLimit);

        private set
        {
            m_keepSuccessorTimeLimit = value.Ticks;
            Limit_KeepSuccessorsTimeLimit();
        }
    }

    /// <summary>
    /// Reads in KeepSuccessorsTimeLimit and limits the maximum value of this field to 1000 years to prevent
    /// overflow problems when the value is added to the current time.
    /// </summary>
    /// <param name="a_reader"></param>
    private void ReadKeepSuccessorsTimeLimit(IReader a_reader)
    {
        long keepSuccessorsTemp;
        a_reader.Read(out keepSuccessorsTemp);
        m_keepSuccessorTimeLimit = keepSuccessorsTemp;
        Limit_KeepSuccessorsTimeLimit();
    }

    /// <summary>
    /// The maximum value of KeepSuccessorsTimeLimit is limited to 1000 years to prevent overflow problems when the value is added to the current time.
    /// This function should be called after the variable is set.
    /// </summary>
    private void Limit_KeepSuccessorsTimeLimit()
    {
        const long KEEP_SUCCESSORS_MAX = TimeSpan.TicksPerDay * 365 * 1000; // About 1000 years. 

        if (m_keepSuccessorTimeLimit > KEEP_SUCCESSORS_MAX)
        {
            m_keepSuccessorTimeLimit = KEEP_SUCCESSORS_MAX;
        }
    }

    /// <summary>
    /// If true then the Operation can be resized in the Gantt by dragging the edge of an Activity.  If false, then no resizing is allowed in the Gantt.
    /// </summary>
    public bool CanResize
    {
        get => m_internalOperationFlags[CanResizeIdx];
        private set => m_internalOperationFlags[CanResizeIdx] = value;
    }

    /// <summary>
    /// If true, split Activities within this Operation don't incur Setup amongst each other
    /// </summary>
    public bool PreventSplitsFromIncurringSetup
    {
        get => m_internalOperationFlags[c_preventSplitsFromIncurringSetup];
        private set => m_internalOperationFlags[c_preventSplitsFromIncurringSetup] = value;
    }


    /// <summary>
    /// If true, split Activities within this Operation don't incur Clean amongst each other
    /// </summary>
    public bool PreventSplitsFromIncurringClean
    {
        get => m_internalOperationFlags[c_preventSplitsFromIncurringClean];
        private set => m_internalOperationFlags[c_preventSplitsFromIncurringClean] = value;
    }

    /// <summary>
    /// If true, split Activities can store the same lot in a SA even if the SA is marked as RequireEmptyStorage+
    /// </summary>
    public bool AllowSameLotInNonEmptyStorageArea
    {
        get => m_internalOperationFlags[c_splitsOverrideEmptyStorageAreaConstraintIdx];
        private set => m_internalOperationFlags[c_splitsOverrideEmptyStorageAreaConstraintIdx] = value;
    }
    #endregion Shared Properties

    #region Properties
    /// <summary>
    /// Locked Blocks cannot be moved to different Resources during Optimizations, Moves, or Expedites..
    /// </summary>
    public lockTypes Locked
    {
        get
        {
            int lockedCount = 0;
            for (int i = 0; i < Activities.Count; i++)
            {
                if (Activities.GetByIndex(i).Locked == lockTypes.SomeBlocksLocked)
                {
                    return lockTypes.SomeBlocksLocked;
                }

                if (Activities.GetByIndex(i).Locked == lockTypes.Locked)
                {
                    lockedCount++;
                }
            }

            if (lockedCount == 0)
            {
                return lockTypes.Unlocked;
            }

            if (lockedCount == Activities.Count)
            {
                return lockTypes.Locked;
            }

            return lockTypes.SomeBlocksLocked;
        }
    }

    /// <summary>
    /// Gets the lock status of the primary resource requirements.
    /// </summary>
    /// <returns></returns>
    internal lockTypes GetPrimaryRRLockedStatus()
    {
        int lockedCount = 0;
        for (int i = 0; i < Activities.Count; i++)
        {
            InternalActivity act = Activities.GetByIndex(i);
            if (act.PrimaryResourceRequirementLock() != null)
            {
                ++lockedCount;
            }
        }

        lockTypes ret;
        if (lockedCount == 0)
        {
            ret = lockTypes.Unlocked;
        }
        else if (lockedCount == Activities.Count)
        {
            ret = lockTypes.Locked;
        }
        else
        {
            ret = lockTypes.SomeBlocksLocked;
        }

        return ret;
    }

    public void Lock(bool a_lockit)
    {
        for (int i = 0; i < Activities.Count; i++)
        {
            Activities.GetByIndex(i).Lock(a_lockit);
        }
    }

    /// <summary>
    /// Name of the resource(s) that the Operation is scheduled on.
    /// </summary>
    public string ResourcesUsed
    {
        get
        {
            Hashtable resourceNamesAdded = new ();
            List<BaseResource> resourcesToAddToList = new ();
            string reasonIfNoResourcesUsed = "";
            for (int i = 0; i < Activities.Count; i++)
            {
                InternalActivity a = Activities.GetByIndex(i);
                List<BaseResource> activityResourcesUsed = a.GetResourcesUsed(out reasonIfNoResourcesUsed);
                for (int r = 0; r < activityResourcesUsed.Count; r++)
                {
                    BaseResource resource = activityResourcesUsed[r];
                    if (!resourceNamesAdded.ContainsKey(resource.Name))
                    {
                        resourceNamesAdded.Add(resource.Name, null);
                        resourcesToAddToList.Add(resource);
                    }
                }
            }

            string outString;
            if (resourcesToAddToList.Count == 0)
            {
                outString = reasonIfNoResourcesUsed;
            }
            else
            {
                System.Text.StringBuilder builder = new ();
                for (int i = 0; i < resourcesToAddToList.Count; i++)
                {
                    BaseResource nxtRes = resourcesToAddToList[i];
                    if (ScenarioDetail.PlantManager.Count > 1) //show Plant too
                    {
                        if (i > 0)
                        {
                            builder.Append(string.Format(", {0} ({1})", nxtRes.Name, nxtRes.Plant.Name));
                        }
                        else
                        {
                            builder.Append(string.Format("{0} ({1})", nxtRes.Name, nxtRes.Plant.Name));
                        }
                    }
                    else
                    {
                        if (i > 0)
                        {
                            builder.Append(string.Format(", {0}", nxtRes.Name));
                        }
                        else
                        {
                            builder.Append(string.Format("{0}", nxtRes.Name));
                        }
                    }
                }

                outString = builder.ToString();
            }

            return outString;
        }
    }

    private readonly ProductionInfo m_productionInfo;

    public ProductionInfo ProductionInfo => m_productionInfo;

    //internal virtual PT.Scheduler.Schedule.Operation.ProductionInfo CreateCopyOfProductionInfo()
    //{
    //    return new ProductionInfo((ProductionInfo)m_productionInfo);
    //}

    /// <summary>
    /// Percent of parts expected to be scrapped.  Used to calculate Expected Good Qty and Exptected Scrap Qty.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public decimal PlanningScrapPercent => m_productionInfo.PlanningScrapPercent;

    internal long SetupSpanTicks
    {
        get => m_productionInfo.SetupSpanTicks;
        set => m_productionInfo.SetupSpanTicks = value;
    }

    public decimal ProductionSetupCost
    {
        get => m_productionInfo.ProductionSetupCost;
        private set => m_productionInfo.ProductionSetupCost = value;
    }

    internal long CycleSpanTicks
    {
        get => m_productionInfo.CycleSpanTicks;
        private set => m_productionInfo.CycleSpanTicks = value;
    }

    internal long PostProcessingSpanTicks
    {
        get => m_productionInfo.PostProcessingSpanTicks;
        private set => m_productionInfo.PostProcessingSpanTicks = value;
    }
    
    internal long StorageSpanTicks
    {
        get => m_productionInfo.StorageSpanTicks;
        private set => m_productionInfo.StorageSpanTicks = value;
    }

    internal long CleanSpanTicks
    {
        get => m_productionInfo.CleanSpanTicks;
        private set => m_productionInfo.CleanSpanTicks = value;
    }

    public int CleanoutGrade
    {
        get => m_productionInfo.CleanoutGrade;
        private set => m_productionInfo.CleanoutGrade = value;
    }

    public decimal CleanoutCost
    {
        get => m_productionInfo.CleanoutCost;
        private set => m_productionInfo.CleanoutCost = value;
    }


    /// <summary>
    /// The quantity of product produced during each production cycle.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public decimal QtyPerCycle
    {
        get => m_productionInfo.QtyPerCycle;
        internal set => m_productionInfo.QtyPerCycle = value;
    }

    /// <summary>
    /// If scheduled, this is the scheduled start time of the Operation's earliest Activity.
    /// </summary>
    public override DateTime StartDateTime
    {
        get
        {
            GetScheduledStartDateTicks(out long scheduledStartTicks);
            return new DateTime(scheduledStartTicks, DateTimeKind.Utc);
        }
    }

    internal override bool GetScheduledStartDateTicks(out long o_scheduledStartTicks)
    {
        InternalActivity leadAct = GetLeadActivity();

        if (leadAct != null)
        {
            o_scheduledStartTicks = leadAct.GetScheduledStartTicks();
            return true;
        }

        o_scheduledStartTicks = PTDateTime.MinDateTime.Ticks;
        return false;
    }

    internal long GetScheduledStartOfProcessingTicks(InternalActivity a_leadAct)
    {
        if (a_leadAct != null)
        {
            return a_leadAct.GetScheduledStartOfProcessingTicks();
        }

        return PTDateTime.MinDateTime.Ticks;
    }

    [Browsable(false)]
    /// <summary>
    /// Return the earliest scheduled activity.
    /// If no activities are scheduled, null is returned.
    /// </summary>
    public InternalActivity GetLeadActivity()
    {
        InternalActivity leadActivity = null;
        long leadActivityScheduledStartTicks = PTDateTime.MaxDateTime.Ticks;

        for (int i = 0; i < m_activities.Count; i++)
        {
            InternalActivity activity = m_activities.GetByIndex(i);

            if (activity.Scheduled)
            {
                long curActScheduledStartTicks = activity.GetScheduledStartTicks();

                if (curActScheduledStartTicks < leadActivityScheduledStartTicks)
                {
                    leadActivity = activity;
                    leadActivityScheduledStartTicks = curActScheduledStartTicks;
                }
            }
        }

        return leadActivity;
    }

    /// <summary>
    /// Return the Activity with the latest scheduled end date.
    /// If no activities are scheduled, null is returned.
    /// </summary>
    public InternalActivity GetLatestEndingScheduledActivity()
    {
        InternalActivity latestActivity = null;
        long lastActivityScheduledEndTicks = PTDateTime.MinDateTime.Ticks;

        for (int i = 0; i < m_activities.Count; i++)
        {
            InternalActivity activity = m_activities.GetByIndex(i);

            if (activity.Scheduled)
            {
                long curActScheduledEndTicks = activity.Batch.PostProcessingEndDateTime.Ticks;

                if (curActScheduledEndTicks >= lastActivityScheduledEndTicks)
                {
                    latestActivity = activity;
                    lastActivityScheduledEndTicks = curActScheduledEndTicks;
                }
            }
        }

        return latestActivity;
    }

    /// <summary>
    /// The Required Finish Qty less the sum of the Reported Good Quantities of all of the Operation's Activities.
    /// This value is never negative.  If more is good parts are reported than are required then this value is zero.
    /// </summary>
    public decimal RemainingFinishQty
    {
        get
        {
            decimal sumOfActivityRemainingQties = 0;
            for (int i = 0; i < Activities.Count; i++)
            {
                InternalActivity activity = Activities.GetByIndex(i);
                sumOfActivityRemainingQties += activity.RemainingQty;
            }

            return sumOfActivityRemainingQties;
        }
    }

    /// <summary>
    /// A list of the Capabilities required to perform the Operation.
    /// </summary>
    public string RequiredCapabilities
    {
        get
        {
            string capabilitiesList = "";
            for (int i = 0; i < ResourceRequirements.Count; i++)
            {
                ResourceRequirement rr = ResourceRequirements.GetByIndex(i);
                for (int c = 0; c < rr.CapabilityManager.Count; c++)
                {
                    Capability capability = rr.CapabilityManager.GetByIndex(c);
                    string nextString;
                    if (capability.Name != null && capability.Name.Trim().Length > 0)
                    {
                        nextString = capability.Name;
                    }
                    else
                    {
                        nextString = capability.ExternalId.Trim();
                    }

                    if (capabilitiesList.Length > 0)
                    {
                        capabilitiesList += ", ";
                    }

                    capabilitiesList += nextString;
                }
            }

            return capabilitiesList;
        }
    }

    /// <summary>
    /// The average Percent Finished of the Operation's Activities.
    /// </summary>
    [ParenthesizePropertyName(true)]
    public int PercentFinished
    {
        get
        {
            int percentFinished = 0;
            if (Activities.Count > 0)
            {
                int total = 0;
                for (int i = 0; i < Activities.Count; i++)
                {
                    total += Activities.GetByIndex(i).PercentFinished;
                }

                percentFinished = total / Activities.Count;
            }
            else
            {
                percentFinished = 0; //Omitted operations for example have no activities
            }

            return percentFinished;
        }
    }

    /// <summary>
    /// The sum of the reported good quantities of all the activities.
    /// </summary>
    public override decimal ReportedGoodQty
    {
        get
        {
            decimal reportedGoodQty = 0;

            for (int i = 0; i < Activities.Count; ++i)
            {
                InternalActivity ia = Activities.GetByIndex(i);
                reportedGoodQty += ia.ReportedGoodQty;
            }

            return reportedGoodQty;
        }
    }

    /// <summary>
    /// The sum of the reported scrap quantities of all the activities.
    /// </summary>
    public decimal ReportedScrapQty
    {
        get
        {
            decimal reportedScrapQty = 0;

            for (int i = 0; i < Activities.Count; ++i)
            {
                InternalActivity ia = Activities.GetByIndex(i);
                reportedScrapQty += ia.ReportedScrapQty;
            }

            return reportedScrapQty;
        }
    }

    /// <summary>
    /// If the activity has some run time scheduled then this is the time it is scheduled to end.
    /// If the activity is in the post-processing state or is finished then this is the time run was scheduled to finish or is the time run was reported to be finished.
    /// </summary>
    internal long EndOfRunTicks
    {
        get
        {
            long endOfRunTicks = PTDateTime.MinDateTime.Ticks;

            for (int actI = 0; actI < Activities.Count; ++actI)
            {
                InternalActivity act = Activities.GetByIndex(actI);
                long actIEndOfRunTicks = act.EndOfRunTicks;
                endOfRunTicks = Math.Max(endOfRunTicks, actIEndOfRunTicks);
            }

            return endOfRunTicks;
        }
    }

    /// <summary>
    /// If the activity has some run time scheduled then this is the time it is scheduled to end.
    /// If the activity is in the post-processing state or is finished then this is the time run was scheduled to finish or is the time run was reported to be finished.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public DateTime EndOfRunDate => new (EndOfRunTicks);

    /// <summary>
    /// This is the EndOfRunTicks+MaterialPostProcessingSpan. If EndOfRunTicks==PtDateTime.MIN_DATE, then PtDateTime.MIN_DATE is returned.
    /// </summary>
    internal long EndOfMatlPostProcTicks
    {
        get
        {
            long end = PTDateTime.MinDateTime.Ticks;

            for (int actI = 0; actI < Activities.Count; ++actI)
            {
                InternalActivity act = Activities.GetByIndex(actI);
                long endOfMaterial = act.EndOfRunTicks + act.ScheduledOrDefaultProductionInfo.MaterialPostProcessingSpanTicks;
                end = Math.Max(end, endOfMaterial);
            }

            return end;
        }
    }

    public DateTime EndOfMatlPostProcDate => new (EndOfMatlPostProcTicks);

    /// <summary>
    /// An Operation is a Bottlneck if any of its Activities are Bottlenecks.
    /// Unless this Operation is scheduled sooner the Job or Manufacturing Order will be late.
    /// </summary>
    public override bool Bottleneck
    {
        get
        {
            if (IsFinishedOrOmitted || NotPartOfCurrentRouting())
            {
                return false;
            }

            for (int actI = 0; actI < Activities.Count; ++actI)
            {
                InternalActivity act = Activities.GetByIndex(actI);
                if (act.Bottleneck)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public bool CapacityBottleneck
    {
        get
        {
            if (IsFinishedOrOmitted || NotPartOfCurrentRouting())
            {
                return false;
            }

            for (int actI = 0; actI < Activities.Count; ++actI)
            {
                InternalActivity act = Activities.GetByIndex(actI);
                if (act.Timing == BaseActivityDefs.onTimeStatuses.CapacityBottleneck)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public bool MaterialBottleneck
    {
        get
        {
            if (IsFinishedOrOmitted || NotPartOfCurrentRouting())
            {
                return false;
            }

            for (int actI = 0; actI < Activities.Count; ++actI)
            {
                InternalActivity act = Activities.GetByIndex(actI);
                if (act.Timing == BaseActivityDefs.onTimeStatuses.MaterialBottleneck)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public bool ReleaseDateBottleneck
    {
        get
        {
            if (IsFinishedOrOmitted || NotPartOfCurrentRouting())
            {
                return false;
            }

            for (int actI = 0; actI < Activities.Count; ++actI)
            {
                InternalActivity act = Activities.GetByIndex(actI);
                if (act.Timing == BaseActivityDefs.onTimeStatuses.ReleaseDateBottleneck)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public bool Late
    {
        get
        {
            if (NotPartOfCurrentRouting())
            {
                return false;
            }

            if (IgnoreLatenessDueToSafetyStockWarningOption())
            {
                return false;
            }

            for (int actI = 0; actI < Activities.Count; ++actI)
            {
                InternalActivity act = Activities.GetByIndex(actI);
                if (act.Late)
                {
                    return true;
                }
            }

            return false;
        }
    }

    private AutoSplitInfo m_splitInfo = new ();

    public AutoSplitInfo AutoSplitInfo
    {
        get => m_splitInfo;
        internal set => m_splitInfo = value;
    }

    private OperationDefs.ESetupSplitType m_setupSplitType;

    public OperationDefs.ESetupSplitType SetupSplitType
    {
        get => m_setupSplitType;
        internal set => m_setupSplitType = value;
    }

    private double m_sequenceHeadStartDays = 0;
    public double SequenceHeadStartDays
    {
        get => m_sequenceHeadStartDays;
        private set
        {
            if (value < 0)
            {
                throw new PTValidationException("SequenceHeadStartDays value was invalid, must be greater than 0.");
            }

            m_sequenceHeadStartDays = value;
        }
    }

    #endregion

    #region Status
    /// <summary>
    /// Whether all the activities of the operations have been finished.
    /// </summary>
    public override bool Finished
    {
        get
        {
            long temp;
            return GetReportedFinishDate(out temp);
        }
    }

    /// <summary>
    /// Get the finish date of the operation if it is finished.
    /// </summary>
    /// <param name="o_finishedDate">This OUT argument is only valid if the operation has been finished.</param>
    /// <returns>Whether the operation has been finished.</returns>
    public bool GetReportedFinishDate(out long o_finishedDate)
    {
        o_finishedDate = PTDateTime.MinDateTime.Ticks;

        for (int activityI = 0; activityI < Activities.Count; ++activityI)
        {
            InternalActivity internalActivity = Activities.GetByIndex(activityI);
            if (internalActivity.ProductionStatus == InternalActivityDefs.productionStatuses.Finished)
            {
                if (internalActivity.ReportedFinishDateTicks > o_finishedDate)
                {
                    o_finishedDate = internalActivity.ReportedFinishDateTicks;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Get the reported start date of the operation if it is finished.
    /// </summary>
    /// <param name="startdDate">This OUT argument is only valid if the operation has been finished.</param>
    /// <returns>Whether the operation has been finished.</returns>
    internal bool GetReportedStartDate(out long o_startDate)
    {
        o_startDate = PTDateTime.MaxDateTime.Ticks;

        for (int activityI = 0; activityI < Activities.Count; ++activityI)
        {
            InternalActivity internalActivity = Activities.GetByIndex(activityI);
            if (internalActivity.ProductionStatus == InternalActivityDefs.productionStatuses.Finished)
            {
                if (internalActivity.ReportedStartDateTicks < o_startDate)
                {
                    o_startDate = internalActivity.ReportedStartDateTicks;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Call this function when an activity is finished so the operation can perform whatever special processing it needs to.
    /// </summary>
    internal void ActivityFinished()
    {
        if (Finished)
        {
            ManufacturingOrder.OperationFinished();
            ManufacturingOrder.ScenarioDetail.OperationsFinished();
        }
    }
    #endregion

    #region Object Accessors
    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private readonly InternalActivityManager m_activities;

    /// <summary>
    /// A portion of the Operation to be completed.  There is one Activity per Operation unless it is Split into multiple Activities.
    /// </summary>
    [Browsable(false)]
    public InternalActivityManager Activities => m_activities;

    private VesselTypeRequirementsCollection m_vesselTypeRequirementClaims = new ();

    /// <summary>
    /// List of all the VesselType Requirments that this Operation claims.
    /// </summary>
    [Browsable(false)]
    public VesselTypeRequirementsCollection VesselTypeRequirementClaims
    {
        get => m_vesselTypeRequirementClaims;
        private set => m_vesselTypeRequirementClaims = value;
    }

    private VesselTypeRequirementsCollection m_vesselTypeRequirementReleases = new ();

    /// <summary>
    /// List of all the VesselType Requirments that this Operation releases.
    /// </summary>
    [Browsable(false)]
    public VesselTypeRequirementsCollection VesselTypeRequirementReleases
    {
        get => m_vesselTypeRequirementReleases;
        private set => m_vesselTypeRequirementReleases = value;
    }

    private readonly ResourceRequirementManager m_resourceRequirements;

    [Browsable(false)]
    public ResourceRequirementManager ResourceRequirements => m_resourceRequirements;

    public List<InternalResource> GetResourcesScheduled()
    {
        List<InternalResource> resList = new ();
        for (int actI = 0; actI < Activities.Count; actI++)
        {
            InternalActivity activity = Activities.GetByIndex(actI);
            for (int resI = 0; resI < activity.ResourceRequirementBlockCount; resI++)
            {
                ResourceBlock block = activity.GetResourceRequirementBlock(resI);
                if (block != null)
                {
                    resList.Add(block.ScheduledResource);
                }
            }
        }

        return resList;
    }
    #endregion Object Accessors

    #region Transmission functionality
    #region ERP transmission status update
    /// <summary>
    /// Call this function before handling a JobT or some other transmission that updates the status of jobs.
    /// It resets the activity variables that indicate the type of updates that have occurred.
    /// </summary>
    internal override void ResetERPStatusUpdateVariables()
    {
        base.ResetERPStatusUpdateVariables();

        for (int i = 0; i < m_activities.Count; ++i)
        {
            m_activities.GetByIndex(i).ResetERPStatusUpdateVariables();
        }
    }
    #endregion

    /// <summary>
    /// Lock and/or Anchor activities that start before a time.
    /// </summary>
    internal void LockAndAnchorBefore(OptimizeSettings.ETimePoints a_startSpan, ScenarioOptions a_scenarioOptions)
    {
        bool inFrozenSpan = false;
        for (int i = 0; i < Activities.Count; i++)
        {
            InternalActivity activity = Activities.GetByIndex(i);
            if (activity.Scheduled)
            {
                long earliestEndOfSpan;
                switch (a_startSpan)
                {
                    case OptimizeSettings.ETimePoints.EndOfFrozenZone:
                        earliestEndOfSpan = activity.Batch.GetEarliestFrozenSpan();
                        break;
                    case OptimizeSettings.ETimePoints.EndOfStableZone:
                        earliestEndOfSpan = activity.Batch.GetEarliestStableSpan();
                        break;
                    case OptimizeSettings.ETimePoints.EndOfPlanningHorizon:
                    case OptimizeSettings.ETimePoints.EntireSchedule:
                        earliestEndOfSpan = PTDateTime.MaxDateTimeTicks;
                        break;
                    case OptimizeSettings.ETimePoints.CurrentPTClock:
                    case OptimizeSettings.ETimePoints.SpecificDateTime:
                    case OptimizeSettings.ETimePoints.EndOfShortTerm: //I'm not sure why these others can't be handled
                    default:
                        throw new ArgumentOutOfRangeException(nameof(a_startSpan), a_startSpan, null);
                }

                if (a_startSpan == OptimizeSettings.ETimePoints.EndOfFrozenZone && activity.GetScheduledStartTicks() < (ScenarioDetail.Clock + earliestEndOfSpan))
                {
                    inFrozenSpan = true;
                    if (a_scenarioOptions.LockInFrozenZone)
                    {
                        activity.Lock(a_scenarioOptions.LockInFrozenZone);
                    }

                    if (a_scenarioOptions.AnchorInFrozenZone)
                    {
                        activity.SetAnchor(a_scenarioOptions.AnchorInFrozenZone);
                        if (a_scenarioOptions.CommitOnAnchor)
                        {
                            SetCommitDates();
                        }
                    }
                }
            }
        }

        if (a_scenarioOptions.IgnoreMaterialConstraintsInFrozenSpan)
        {
            if (inFrozenSpan)
            {
                MaterialRequirements.IgnoreConstraintsForFrozenSpan();
            }
            else
            {
                MaterialRequirements.RestoreConstraintsAfterFrozenSpan();
            }
        }
        else
        {
            MaterialRequirements.RestoreConstraintsAfterFrozenSpan();
        }
    }

    public bool Receive(InternalActivityUpdateT.ActivityStatusUpdate a_update, IScenarioDataChanges a_dataChanges)
    {
        AuditEntry auditEntry = new AuditEntry(Id, ManufacturingOrder.Id, this);
        bool updated = false;
        if (a_update.OnHoldSet && OnHold != a_update.OnHold)
        {
            OnHold = a_update.OnHold;
            if (a_update.OnHold)
            {
                HoldUntil = a_update.HoldUntil;
                if (a_update.HoldReason != null)
                {
                    HoldReason = a_update.HoldReason;
                }
                else
                {
                    HoldReason = "";
                }
            }
            else
            {
                HoldUntil = PTDateTime.MinDateTime;
                HoldReason = "";
            }

            updated = true;
            a_dataChanges.FlagConstraintChanges(Job.Id);
        }

        if (a_update.OperationUserFieldsSet)
        {
            UserFields = a_update.OperationUserFields;
            updated = true;
        }

        AuditEntry activityAudit;
        if (a_update.ActivityExternalIdSet)
        {
            
            InternalActivity activity = Activities.GetByExternalId(a_update.ActivityExternalId);
            if (activity == null)
            {
                throw new PTValidationException("2235", new object[] { a_update.ActivityExternalId, ManufacturingOrder.ExternalId, ExternalId });
            }

            activityAudit = new AuditEntry(activity.Id, Id, activity);
            updated |= activity.Receive(a_update, a_dataChanges);
        }
        else
        {
            if (Activities.Count == 1) //use the only one we have
            {
                InternalActivity internalActivity = Activities.GetByIndex(0);
                activityAudit = new AuditEntry(internalActivity.Id, Id, internalActivity);
                updated |= internalActivity.Receive(a_update, a_dataChanges);
            }
            else
            {
                throw new PTValidationException("2236", new object[] { Activities.Count, ExternalId, ManufacturingOrder.ExternalId }); //JMC TODO
            }
        }

        a_dataChanges.AuditEntry(activityAudit);
        a_dataChanges.AuditEntry(auditEntry);

        return updated;
    }

    internal override void AdvancingClock(TimeSpan a_clockAdvancedBy, DateTime a_newClock, bool a_autoFinishAllActivities, bool a_autoReportProgressOnAllActivities)
    {
        if (a_autoReportProgressOnAllActivities || AutoReportProgress)
        {
            for (int i = 0; i < Activities.Count; i++)
            {
                InternalActivity a = Activities.GetByIndex(i);
                if (a.ProductionStatus != InternalActivityDefs.productionStatuses.Finished)
                {
                    a.AutoReportProgress(ScenarioDetail.ScenarioOptions, a_clockAdvancedBy);
                }
            }
        }

        if (a_autoFinishAllActivities || AutoFinish)
        {
            for (int i = 0; i < Activities.Count; i++)
            {
                InternalActivity a = Activities.GetByIndex(i);
                if (a.ProductionStatus != InternalActivityDefs.productionStatuses.Finished)
                {
                    if (Scheduled && a.Batch.EndDateTime.Ticks <= a_newClock.Ticks)
                    {
                        a.AutoFinish("Finished Automatically by Clock Advance".Localize());
                    }
                }
            }
        }
    }

    /// <summary>
    /// Finish the Operation using default values.
    /// </summary>
    internal override void AutoFinishAllActivities(string a_commentsToLog)
    {
        for (int i = 0; i < Activities.Count; i++)
        {
            InternalActivity a = Activities.GetByIndex(i);
            if (a.ProductionStatus != InternalActivityDefs.productionStatuses.Finished)
            {
                a.AutoFinish(a_commentsToLog);
            }
        }
    }
    #endregion

    #region Constraint Violations
    /// <summary>
    /// Find constraint violations of the operation's activities and add them to the list parameter.
    /// </summary>
    /// <param name="violations">All constraint violations are added to this list.</param>
    internal override void GetConstraintViolations(ConstraintViolationList a_violations)
    {
        if (Scheduled)
        {
            for (int activityI = 0; activityI < m_activities.Count; ++activityI)
            {
                InternalActivity activity = m_activities.GetByIndex(activityI);

                if (activity.Scheduled)
                {
                    long latestConstraint = activity.GetScheduledStartTicks();
                    string latestConstraintDescription = "";

                    // Check Operation's hold until date
                    if (OnHold)
                    {
                        if (HoldUntilTicks > latestConstraint)
                        {
                            latestConstraint = HoldUntilTicks;
                            latestConstraintDescription = "Operation's Hold Until Date".Localize();
                        }
                    }

                    // Check the Manufacturing Order's hold until date
                    if (ManufacturingOrder.Hold)
                    {
                        if (ManufacturingOrder.HoldUntilTicks > latestConstraint)
                        {
                            latestConstraint = ManufacturingOrder.HoldUntilTicks;
                            latestConstraintDescription = "MO's Hold Until Date".Localize();
                        }
                    }

                    // Check the Job's hold until date.
                    if (ManufacturingOrder.Job.Hold)
                    {
                        Job job = ManufacturingOrder.Job;
                        if (job.HoldUntilTicks > latestConstraint)
                        {
                            latestConstraint = job.HoldUntilTicks;
                            latestConstraintDescription = "Job's Hold Until Date".Localize();
                        }
                    }

                    // If there is a new constraint violation add it to the constraint violations list.
                    if (latestConstraint > activity.GetScheduledStartTicks())
                    {
                        ConstraintViolation v = new (activity, latestConstraint, latestConstraintDescription);
                        a_violations.AddEnd(v);
                    }
                }
            }
        }
    }
    #endregion

    #region Operation ready reason
    public enum LatestConstraintEnum
    {
        Unknown,
        Clock,
        ManufacturingOrderRelease,
        PredecessorOperation,
        MaterialRequirement,
        PredecessorManufacturingOrder,
        JobHoldDate,
        ManufacturingOrderHoldDate,
        AlternatePath,
        AddIn,
        ManufacturingOrderBatch,
        TransferSpan,
        FinishTime,
        OperationHoldDate,
        AnchorDate,
        ResourceConnector,
        CompatibilityCode,
        StorageCapacity,
        LotCode,
        StorageConnector,

        // [ALTERNATE_PATH]
    }

    private LatestConstraintEnum m_latestConstraint = LatestConstraintEnum.Unknown;

    /// <summary>
    /// The reason this operation became ready.
    /// </summary>
    public LatestConstraintEnum LatestConstraintInternal => m_latestConstraint;

    /// <summary>
    /// Use this function to set the latest contraint. There is no setter in the property
    /// because the setter needs to be private.
    /// </summary>
    /// <param name="aLatestConstraint"></param>
    private void SetLatestConstraint(LatestConstraintEnum a_latestConstraint)
    {
        m_latestConstraint = a_latestConstraint;
    }

    /// <summary>
    /// Returns the MaterialRequirement that is the latest constraint if a Material Requirment is the latest constraint.
    /// Otherwise it returns null.
    /// </summary>
    /// <returns></returns>
    public MaterialRequirement GetLatestConstraintMaterial()
    {
        if (LatestConstraintInternal == LatestConstraintEnum.MaterialRequirement)
        {
            return LatestConstraintMaterialRequirement;
        }

        return null;
    }

    /// <summary>
    /// The latest (most constraining) of all constraints for the Operation including: Predecessors, Materials and ReleaseDate.
    /// This is the constraint that is limiting the Operation from moving before the Latest Constraint Date.
    /// </summary>
    /// <summary>
    /// The latest (most constraining) of all constraints for the Operation including: Predecessors, Materials and ReleaseDate.  This is the constraint that is limiting the Operation from moving before the
    /// Latest Constraint Date.
    /// </summary>
    public string LatestConstraint
    {
        get
        {
            if (LatestConstraintInternal == LatestConstraintEnum.ManufacturingOrderRelease)
            {
                return Localizer.GetString("MO Release Date");
            }

            if (LatestConstraintInternal == LatestConstraintEnum.AnchorDate)
            {
                return Localizer.GetString("Anchor Date");
            }

            if (LatestConstraintInternal == LatestConstraintEnum.MaterialRequirement)
            {
                if (LatestConstraintMaterialRequirement != null)
                {
                    return string.Format("{0} {1} {2}", Localizer.GetString("Material"), LatestConstraintMaterialRequirement.MaterialName, LatestConstraintMaterialRequirement.MaterialDescription);
                }

                return Localizer.GetString("Material");
            }

            if (LatestConstraintInternal == LatestConstraintEnum.PredecessorOperation)
            {
                return string.Format("{0}: {1} {2}; {3}", Localizer.GetString("Predecessor Operation"), LatestConstraintPredecessorOperation.Name, LatestConstraintPredecessorOperation.Description, ((InternalOperation)LatestConstraintPredecessorOperation).MainResourceNames);
            }

            if (LatestConstraintInternal == LatestConstraintEnum.Clock)
            {
                return Localizer.GetString("Capacity");
            }

            if (LatestConstraintInternal == LatestConstraintEnum.PredecessorManufacturingOrder)
            {
                return Localizer.GetString("Predecessor Manufacturing Order");
            }

            if (LatestConstraintInternal == LatestConstraintEnum.AddIn)
            {
                return string.Format("{0}: {1}", Localizer.GetString("Add-In"), LatestConstraintDescription);
            }

            if (LatestConstraintInternal == LatestConstraintEnum.JobHoldDate)
            {
                return string.Format("{0}: {1}", Localizer.GetString("Job Hold"), Job.HoldUntil.ToLocalTime().ToString());
            }

            if (LatestConstraintInternal == LatestConstraintEnum.ManufacturingOrderHoldDate)
            {
                return string.Format("{0}: {1}", Localizer.GetString("Manufacturing Order Hold"), ManufacturingOrder.HoldUntil.ToLocalTime().ToString());
            }

            if (LatestConstraintInternal == LatestConstraintEnum.AlternatePath)
            {
                return Localizer.GetString("Alternate Path");
            }

            if (LatestConstraintInternal == LatestConstraintEnum.ManufacturingOrderBatch)
            {
                return Localizer.GetString("Manufacturing Order Batch");
            }

            if (LatestConstraintInternal == LatestConstraintEnum.TransferSpan)
            {
                return Localizer.GetString("Transfer Span");
            }

            if (LatestConstraintInternal == LatestConstraintEnum.FinishTime)
            {
                return Localizer.GetString("Finish Time");
            }

            if (LatestConstraintInternal == LatestConstraintEnum.OperationHoldDate)
            {
                return Localizer.GetString("Operation Hold Date");
            } 
            
            if (LatestConstraintInternal == LatestConstraintEnum.OperationHoldDate)
            {
                return Localizer.GetString("Operation Hold Date");
            }
            
            if (LatestConstraintInternal == LatestConstraintEnum.ResourceConnector)
            {
                return Localizer.GetString("Resource Connector");
            }

            if (LatestConstraintInternal == LatestConstraintEnum.CompatibilityCode)
            {
                return string.Format("Compatibility Code {0}".Localize(), LatestConstraintDescription);
            }

            if (LatestConstraintInternal == LatestConstraintEnum.StorageCapacity)
            {
                return string.Format("{0}: {1}", Localizer.GetString("Storage"), LatestConstraintDescription);
            }
            
            if (LatestConstraintInternal == LatestConstraintEnum.StorageConnector)
            {
                return string.Format("{0}: {1}", Localizer.GetString("Storage Connector"), LatestConstraintDescription);
            }

            return Localizer.GetString("Unknown");
        }
    }

    private long m_latestConstraintDate;

    /// <summary>
    /// The time the operation became ready.
    /// </summary>
    /// <summary>
    /// The date where the Latest Constraint occurs.  The Operation cannot be moved to start before this.
    /// </summary>
    public DateTime LatestConstraintDate => new (m_latestConstraintDate);

    internal long LatestConstraintTicks
    {
        get => m_latestConstraintDate;
        set => m_latestConstraintDate = value;
    }

    private BaseOperation m_latestConstraintPredecessorOperation;

    internal BaseOperation LatestConstraintPredecessorOperation
    {
        get
        {
            if (m_latestConstraint != LatestConstraintEnum.PredecessorOperation)
            {
                throw new PTValidationException("2237");
            }

            return m_latestConstraintPredecessorOperation;
        }
    }

    private string m_latestConstraintDescription;

    /// <summary>
    /// The Name of the Add-In that was the Latest Constraint if the type of Latest Constraint is Add-In.
    /// </summary>
    public string LatestConstraintDescription => m_latestConstraintDescription;

    private readonly string m_latestConstraintResourceName;

    /// <summary>
    /// The Name of the Resource that is the Latest Constraint if the LatestConstraint is a Resource.
    /// </summary>
    public string LatestConstraintResourceName => m_latestConstraintResourceName;

    /// <summary>
    /// Returns a string of the names of the main Resources that are scheduled to perform the Operation.
    /// </summary>
    private string MainResourceNames
    {
        get
        {
            string names = "";
            if (Scheduled)
            {
                for (int i = 0; i < Activities.Count; i++)
                {
                    InternalActivity activity = Activities.GetByIndex(i);
                    if (activity.PrimaryResourceRequirementBlock != null)
                    {
                        if (i > 0)
                        {
                            names = string.Format("{0} {1} ", names, "and".Localize());
                        }

                        names = names + activity.PrimaryResourceRequirementBlock.ScheduledResource.Name;
                    }
                }
            }
            else
            {
                if (Finished)
                {
                    names = "<Finished>".Localize();
                }
                else
                {
                    names = "<Unscheduled>".Localize();
                }
            }

            return names;
        }
    }

    private MaterialRequirement m_latestConstraintMaterialRequirement;

    [DoNotAuditProperty]
    [Browsable(false)]
    public MaterialRequirement LatestConstraintMaterialRequirement
    {
        get
        {
            if (m_latestConstraint != LatestConstraintEnum.MaterialRequirement)
            {
                throw new PTValidationException("2238");
            }

            return m_latestConstraintMaterialRequirement;
            //MaterialRequirement mr = MaterialRequirements.GetLastestConstrainingMaterialRequirement(ManufacturingOrder.ScenarioDetail.Clock);

            //if (mr == null)
            //{
            //    throw new PT.APSCommon.PTValidationException("A material requirement couldn't be found.");
            //}

            //return mr;
        }
    }

    /// <summary>
    /// Set the latest constraint of this operation to the specified predecessor operation.
    /// </summary>
    /// <param name="clockDate">The current clock date. If this is later than the predecessor operation the clock date will be used as the latest constraint instead.</param>
    /// <param name="latestConstraintDate">The latest predecessor operation constraint time.</param>
    /// <param name="op">The predecessor operation that is serving as the constraint.</param>
    internal void SetLatestConstraintPredecessorOp(long a_clockDate, long a_latestConstraintDate, BaseOperation a_op)
    {
        if (a_clockDate >= a_latestConstraintDate)
        {
            SetLatestConstraintToClock(a_clockDate);
        }
        else if (a_op.IsNotFinishedAndNotOmitted) //The event that released this op can be a Predecessor event even if the predeccesor is not scheduling
        {
            SetLatestConstraint(LatestConstraintEnum.PredecessorOperation);

            // Use the first non-finished predecessor operation of the constraining predecessor operation as the 
            // latest constraining operation.
            InternalOperation predOp = FindNonFinishedConstrainingPredecessorOp((InternalOperation)a_op);
            m_latestConstraintPredecessorOperation = predOp;

            LatestConstraintTicks = a_latestConstraintDate;
        }
    }

    /// <summary>
    /// A helper function find the first non-finished LatestConstraintPredecessorOperation.
    /// </summary>
    /// <param name="a_predOp">A predecessor operation constraint.</param>
    /// <returns></returns>
    private InternalOperation FindNonFinishedConstrainingPredecessorOp(InternalOperation a_predOp)
    {
        InternalOperation latestNonFinished;
        if (a_predOp.Finished && a_predOp.LatestConstraintInternal == LatestConstraintEnum.PredecessorOperation && a_predOp.LatestConstraintPredecessorOperation != null)
        {
            a_predOp = FindNonFinishedConstrainingPredecessorOp((InternalOperation)a_predOp.LatestConstraintPredecessorOperation);
        }
        else
        {
            latestNonFinished = a_predOp;
        }

        return a_predOp;
    }

    /// <summary>
    /// Call this function when the latest constraint on an operation is a predecessor mo.
    /// </summary>
    /// <param name="clockDate">The current clock date. If this is later than the latest material date the clock date is used as the constraint of this operation instead.</param>
    /// <param name="latestConstraintDate">The time the predecessor MO's material become available.</param>
    internal void SetLatestConstraintToPredecessorMO(long a_clockDate, long a_latestConstraintDate)
    {
        if (a_clockDate >= a_latestConstraintDate)
        {
            SetLatestConstraintToClock(a_clockDate);
        }
        else
        {
            SetLatestConstraint(LatestConstraintEnum.PredecessorManufacturingOrder);
            LatestConstraintTicks = a_latestConstraintDate;
        }
    }

    /// <summary>
    /// Call this function when the latest constraint on an operation is a predecessor mo.
    /// </summary>
    /// <param name="clockDate">The current clock date. If this is later than the latest material date the clock date is used as the constraint of this operation instead.</param>
    /// <param name="latestConstraintDate">The time the predecessor MO's material become available.</param>
    internal void SetLatestConstraintToResourceConnector(long a_clockDate, long a_latestConstraintDate)
    {
        if (a_clockDate >= a_latestConstraintDate)
        {
            SetLatestConstraintToClock(a_clockDate);
        }
        else
        {
            SetLatestConstraint(LatestConstraintEnum.ResourceConnector);
            LatestConstraintTicks = a_latestConstraintDate;
        }
    }

    /// <summary>
    /// Set the latest constraint of this operation to Material Constraint.
    /// </summary>
    /// <param name="clockDate">The current clock date. If this is later than the latest material date the clock date is used as the constraint of this operation instead.</param>
    /// <param name="latestConstraintDate">The latest material constraint date.</param>
    internal void SetLatestConstraintToMaterialRequirement(long a_clockDate, long a_latestConstraintDate, MaterialRequirement a_mr)
    {
        if (m_latestConstraintMaterialRequirement != null)
        {
            if (LatestConstraintTicks > a_latestConstraintDate)
            {
                #if DEBUG
                DebugException.ThrowInDebug("Latest MR constraint was replaced by another MR constraint with an earlier available date.");
                #endif
            }
        }

        if (a_clockDate > a_latestConstraintDate)
        {
            SetLatestConstraintToClock(a_clockDate);
        }
        else
        {
            if (LatestConstraintInternal == LatestConstraintEnum.AnchorDate && LatestConstraintTicks == a_latestConstraintDate)
            {
                //Keep the constraint at anchor since that is why it was actually delayed.
                return;
            }

            SetLatestConstraint(LatestConstraintEnum.MaterialRequirement);
            m_latestConstraintMaterialRequirement = a_mr;
            LatestConstraintTicks = a_latestConstraintDate;
        }
    }

    /// <summary>
    /// Set the latest constraint of this operation to the manufacturing order's release date.
    /// </summary>
    /// <param name="clockDate">The current clock date. If this is greater than the MO's release date then the latest release is set to the clock.</param>
    internal void SetLatestConstraintToMOReleaseDate(long a_clockDate)
    {
        if (a_clockDate >= ManufacturingOrder.EffectiveReleaseDate)
        {
            SetLatestConstraintToClock(a_clockDate);
        }
        else
        {
            SetLatestConstraint(LatestConstraintEnum.ManufacturingOrderRelease);
            LatestConstraintTicks = ManufacturingOrder.EffectiveReleaseDate;
        }
    }

    internal void SetLatestConstraintToJobHoldReleaseDate(long a_jobHoldReleaseDate)
    {
        SetLatestConstraint(LatestConstraintEnum.JobHoldDate);
        LatestConstraintTicks = a_jobHoldReleaseDate;
    }

    internal void SetLatestConstraintToMOHoldReleaseDate(long a_moHoldReleaseDate)
    {
        SetLatestConstraint(LatestConstraintEnum.ManufacturingOrderHoldDate);
        LatestConstraintTicks = a_moHoldReleaseDate;
    }

    internal void SetLatestConstraintToStorage(long a_storageConstraintDate, string a_description)
    {
        SetLatestConstraint(LatestConstraintEnum.StorageCapacity);
        LatestConstraintTicks = a_storageConstraintDate;
        m_latestConstraintDescription = a_description;
    }
    
    internal void SetLatestConstraintToStorageConnector(long a_storageConstraintDate, string a_description)
    {
        SetLatestConstraint(LatestConstraintEnum.StorageConnector);
        LatestConstraintTicks = a_storageConstraintDate;
        m_latestConstraintDescription = a_description;
    }

    /// <summary>
    /// Lot codes are released at the end of the planning horizon. Only set it to Lots it was already constrained by material.
    /// Otherwise keep the previous constraint.
    /// </summary>
    internal void SetLatestConstraintToLotCode(long a_storageConstraintDate)
    {
        if (LatestConstraintInternal == LatestConstraintEnum.MaterialRequirement)
        {
            SetLatestConstraint(LatestConstraintEnum.LotCode);
            LatestConstraintTicks = a_storageConstraintDate;
            m_latestConstraintDescription = "Sourceable Lots";
        }
    }

    /// <summary>
    /// Some constraint type that needs more work in this area.
    /// </summary>
    /// <param name="clockDate">The current clock date. If this is greater than the release date then the latest release is set to the clock.</param>
    internal void SetLatestConstraintToUnknownConstraint(long a_clockDate, long a_date)
    {
        if (a_clockDate >= ManufacturingOrder.EffectiveReleaseDate)
        {
            SetLatestConstraintToClock(a_clockDate);
        }
        else
        {
            LatestConstraintTicks = a_date;
            SetLatestConstraint(LatestConstraintEnum.Unknown);
        }
    }

    /// <summary>
    /// Set the latest constraint of the operation to the clock.
    /// </summary>
    /// <param name="clockDate">The current clock date.</param>
    internal void SetLatestConstraintToClock(long a_clockDate)
    {
        SetLatestConstraint(LatestConstraintEnum.Clock);
        LatestConstraintTicks = a_clockDate;
    }

    internal void SetLatestConstraintToAddIn(long a_releaseTicks, string a_addInName)
    {
        SetLatestConstraint(LatestConstraintEnum.AddIn);
        m_latestConstraintDescription = a_addInName;
        LatestConstraintTicks = a_releaseTicks;
    }

    /// <summary>
    /// Set the latest constraint of the operation to the clock.
    /// </summary>
    /// <param name="clockDate">The current clock date.</param>
    internal void SetLatestConstraintToCompatibilityCode(long a_clockDate, string a_tableName)
    {
        SetLatestConstraint(LatestConstraintEnum.CompatibilityCode);
        m_latestConstraintDescription = a_tableName;
        LatestConstraintTicks = a_clockDate;
    }

    internal void SetLatestConstraint(long a_releaseTicks, LatestConstraintEnum a_constraint)
    {
        SetLatestConstraint(a_constraint);
        LatestConstraintTicks = a_releaseTicks;
    }

    private void ResetLatestConstraint()
    {
        SetLatestConstraint(LatestConstraintEnum.Unknown);
        m_latestConstraintPredecessorOperation = null;
        LatestConstraintTicks = PTDateTime.MinDateTime.Ticks;
    }

    internal override void Unschedule(bool a_clearLocks = true, bool a_removeFromBatch = true)
    {
        ResetLatestConstraint();
    }
    #endregion

    #region Update
    internal bool UpdateStatus(bool a_erpUpdate, BaseOperation a_updatedOperation, ScenarioOptions a_options, JobT.BaseOperation a_jobTOp, IScenarioDataChanges a_dataChanges)
    {
        bool updated = false;
        InternalOperation updatedInternalOperation = (InternalOperation)a_updatedOperation;

        if (TimeBasedReporting != updatedInternalOperation.TimeBasedReporting)
        {
            TimeBasedReporting = updatedInternalOperation.TimeBasedReporting;
            updated = true;
        }

        if (Activities.Count > 1 && a_jobTOp is JobT.InternalOperation iOpT && iOpT.SplitUpdateMode != InternalOperationDefs.splitUpdateModes.UpdateSplitsIndividually) //need to allocate across splits
        {
            //Add up the times and quantities to allocate by summing across all the old activities and new activities and subtracting old from new.
            long setupTicksInOriginal = 0;
            long runTicksInOriginal = 0;
            long postProcessTicksInOriginal = 0;
            decimal goodQtyInOriginal = 0;
            decimal scrapQtyInOriginal = 0;
            for (int actI = 0; actI < Activities.Count; actI++)
            {
                InternalActivity activity = Activities.GetByIndex(actI);
                setupTicksInOriginal += activity.ReportedSetupSpan.Ticks;
                runTicksInOriginal += activity.ReportedRunSpan.Ticks;
                postProcessTicksInOriginal += activity.ReportedPostProcessingSpan.Ticks;
                goodQtyInOriginal += activity.ReportedGoodQty;
                scrapQtyInOriginal += activity.ReportedScrapQty;
            }

            long setupTicksToAllocate = 0;
            long runTicksToAllocate = 0;
            long postProcessTicksToAllocate = 0;
            decimal goodQtyToAllocate = 0;
            decimal scrapQtyToAllocate = 0;
            for (int updatedActivityI = 0; updatedActivityI < updatedInternalOperation.m_activities.Count; updatedActivityI++)
            {
                InternalActivity updatedActivity = updatedInternalOperation.m_activities.GetByIndex(updatedActivityI);
                setupTicksToAllocate += updatedActivity.ReportedSetup;
                runTicksToAllocate += updatedActivity.ReportedRun;
                postProcessTicksToAllocate += updatedActivity.ReportedPostProcessing;
                goodQtyToAllocate += updatedActivity.ReportedGoodQty;
                scrapQtyToAllocate += updatedActivity.ReportedScrapQty;
            }

            setupTicksToAllocate = Math.Max(0, setupTicksToAllocate - setupTicksInOriginal);
            runTicksToAllocate = Math.Max(0, runTicksToAllocate - runTicksInOriginal);
            postProcessTicksToAllocate = Math.Max(0, postProcessTicksToAllocate - postProcessTicksInOriginal);
            goodQtyToAllocate = Math.Max(0, goodQtyToAllocate - goodQtyInOriginal);
            scrapQtyToAllocate = Math.Max(0, scrapQtyToAllocate - scrapQtyInOriginal);


            if (iOpT.SplitUpdateMode == InternalOperationDefs.splitUpdateModes.ShareReportedValuesChronologically)
            {
                //Create a list of activities sorted by increasing start date
                ArrayList startDateSortableActivityArrayList = new ();
                for (int actI = 0; actI < Activities.Count; actI++)
                {
                    InternalActivity activity = Activities.GetByIndex(actI);
                    startDateSortableActivityArrayList.Add(activity);
                }

                startDateSortableActivityArrayList.Sort(new StartDateActivitySorter());
                //Allocated based on scheduled start time until all qty and hours are allocated.  
                for (int i = 0; i < startDateSortableActivityArrayList.Count; i++)
                {
                    if (startDateSortableActivityArrayList[i] is InternalActivity nxtActivityToAllocateTo)
                    {
                        long setupAlloc = Math.Min(setupTicksToAllocate, nxtActivityToAllocateTo.ScheduledSetupTicks);
                        long runAlloc = Math.Min(runTicksToAllocate, nxtActivityToAllocateTo.ScheduledRunTicks);
                        long ppalloc = Math.Min(postProcessTicksToAllocate, nxtActivityToAllocateTo.ScheduledPostProcessingTicks);
                        decimal qtyGoodAlloc = Math.Min(goodQtyToAllocate, nxtActivityToAllocateTo.RequiredFinishQty - nxtActivityToAllocateTo.ReportedGoodQty);
                        decimal qtyScrapAlloc = Math.Min(scrapQtyToAllocate, nxtActivityToAllocateTo.RequiredFinishQty - nxtActivityToAllocateTo.ReportedScrapQty);

                        //Finish if time/qty reached
                        if (TimeBasedReporting && runAlloc >= nxtActivityToAllocateTo.ScheduledProductionSpan.Ticks && ppalloc >= nxtActivityToAllocateTo.ScheduledPostProcessingTicks)
                        {
                            nxtActivityToAllocateTo.ProductionStatus = InternalActivityDefs.productionStatuses.Finished;
                        }
                        else
                        {
                            decimal qtyRemain = nxtActivityToAllocateTo.RequiredFinishQty - nxtActivityToAllocateTo.ReportedGoodQty;
                            if (!TimeBasedReporting && qtyGoodAlloc >= qtyRemain)
                            {
                                nxtActivityToAllocateTo.ProductionStatus = InternalActivityDefs.productionStatuses.Finished;
                            }
                        }

                        nxtActivityToAllocateTo.ReportedSetup += setupAlloc;
                        nxtActivityToAllocateTo.ReportedRun += runAlloc;
                        nxtActivityToAllocateTo.ReportedPostProcessing += ppalloc;
                        nxtActivityToAllocateTo.ReportedGoodQty += qtyGoodAlloc;
                        nxtActivityToAllocateTo.ReportedScrapQty += qtyScrapAlloc;

                        setupTicksToAllocate -= setupAlloc;
                        runTicksToAllocate -= runAlloc;
                        postProcessTicksToAllocate -= ppalloc;
                        goodQtyToAllocate -= qtyGoodAlloc;
                        scrapQtyToAllocate -= qtyScrapAlloc;
                        if (setupTicksToAllocate == 0 && runTicksToAllocate == 0 && postProcessTicksToAllocate == 0 && goodQtyToAllocate == 0 && scrapQtyToAllocate == 0)
                        {
                            break; //nothing more to allocate
                        }
                    }
                }

                updated = true;
                a_dataChanges.FlagConstraintChanges(Job.Id);
                //Mark Activities finished if all time or qty has been recorded.
            }
            else
            {
                setupTicksToAllocate = (long)((decimal)setupTicksToAllocate / Activities.Count);
                runTicksToAllocate = (long)((decimal)runTicksToAllocate / Activities.Count);
                postProcessTicksToAllocate = (long)((decimal)postProcessTicksToAllocate / Activities.Count);

                if (iOpT.SplitUpdateMode == InternalOperationDefs.splitUpdateModes.ShareReportedValuesEvenlyDecimal)
                {
                    goodQtyToAllocate = goodQtyToAllocate / Activities.Count;
                    scrapQtyToAllocate = scrapQtyToAllocate / Activities.Count;
                }
                else // InternalOperationDefs.splitUpdateModes.ShareReportedValuesEvenlyInteger
                {
                    goodQtyToAllocate = Math.Round(goodQtyToAllocate / Activities.Count, MidpointRounding.AwayFromZero);
                    scrapQtyToAllocate = Math.Round(scrapQtyToAllocate / Activities.Count, MidpointRounding.AwayFromZero);
                }

                for (int actI = 0; actI < Activities.Count; actI++)
                {
                    InternalActivity activity = Activities.GetByIndex(actI);
                    activity.ReportedSetup += setupTicksToAllocate;
                    activity.ReportedRun += runTicksToAllocate;
                    activity.ReportedPostProcessing += postProcessTicksToAllocate;
                    activity.ReportedGoodQty += goodQtyToAllocate;
                    activity.ReportedScrapQty += scrapQtyToAllocate;
                }

                updated = true;
                a_dataChanges.FlagConstraintChanges(Job.Id);
            }
        }
        else //update splits one at a time
        {
            for (int i = 0; i < m_activities.Activities.Count; ++i)
            {
                InternalActivity activity = (InternalActivity)m_activities.Activities.GetByIndex(i);
                InternalActivity updatedActivity = updatedInternalOperation.m_activities.GetByExternalId(activity.ExternalId);
                if (updatedActivity != null) //We may have created the Activity so the ERP may not send in an update on it.
                {
                    //if the op was split internally then we need to preserve the activty quantities at the split levels.  If the operation has a qty change that qty update is handled separately and will
                    // update the lead unstarted activity with the differential.  There is no code in place to update an operation with a different number of activities so we'll just preserve the split qties for now.
                    bool preserveRequiredQtyForSplit = a_erpUpdate && (Activities.Count != updatedInternalOperation.Activities.Count || ManufacturingOrder.PreserveRequiredQty);
                    updated |= activity.Update(a_erpUpdate, updatedActivity, a_options, preserveRequiredQtyForSplit, a_jobTOp, true, true, a_jobTOp.NowFinishUtcTime, a_dataChanges);
                }
            }
        }
        return updated;
    }

    /// <summary>
    /// Update the status of this Operation by moving update values out of this Operation and into the Split Operation.
    /// </summary>
    internal void AllocateStatusToMoSplit(InternalOperation aSplitOp, ManufacturingOrderDefs.SplitUpdateModes aMoSplitUpdateMode, decimal splitMoPctOfTotalQty)
    {
        if (aMoSplitUpdateMode == ManufacturingOrderDefs.SplitUpdateModes.ShareReportedValuesProportionallyInteger)
        {
            AllocateStatusToMOSplitHelper(aSplitOp, splitMoPctOfTotalQty, true, aMoSplitUpdateMode); //ratio always depends on qty, not hrs since there's no MO hr splitting.
        }
        else if (aMoSplitUpdateMode == ManufacturingOrderDefs.SplitUpdateModes.ShareReportedValuesProportionallyDecimal)
        {
            AllocateStatusToMOSplitHelper(aSplitOp, splitMoPctOfTotalQty, false, aMoSplitUpdateMode); //ratio always depends on qty, not hrs since there's no MO hr splitting.
        }
    }

    /// <summary>
    /// Allocate status to operations based on their ratio qty to the original op's qty.
    /// </summary>
    private void AllocateStatusToMOSplitHelper(InternalOperation aSplitOp, decimal aSplitOpPctOfTotalOpQties, bool aIntegerQties, ManufacturingOrderDefs.SplitUpdateModes aMoSplitUpdateMode)
    {
        decimal goodOpQtyToAllocate = aSplitOpPctOfTotalOpQties * ReportedGoodQty;
        decimal scrapOpQtyToAllocate = aSplitOpPctOfTotalOpQties * ReportedScrapQty;
        decimal setupHrsToAllocate = aSplitOpPctOfTotalOpQties * ReportedSetupHours;
        decimal runHrsToAllocate = aSplitOpPctOfTotalOpQties * ReportedRunHours;
        decimal postProcHrsToAllocate = aSplitOpPctOfTotalOpQties * ReportedPostProcessingHours;

        if (aIntegerQties)
        {
            goodOpQtyToAllocate = (int)goodOpQtyToAllocate; //floor; keep remainders in the original
            scrapOpQtyToAllocate = (int)scrapOpQtyToAllocate;
        }

        AllocateStatusToMOSplitActivitesProportionallyyHelper(aSplitOp, goodOpQtyToAllocate, scrapOpQtyToAllocate, setupHrsToAllocate, runHrsToAllocate, postProcHrsToAllocate, aIntegerQties);
    }

    /// <summary>
    /// Allocate to Activities in proportion to their qty within the Operation
    /// </summary>
    private void AllocateStatusToMOSplitActivitesProportionallyyHelper(InternalOperation aSplitOp,
                                                                       decimal aGoodOpQtyToAllocate,
                                                                       decimal aScrapQtyToAllocate,
                                                                       decimal aSetupHrsToAllocate,
                                                                       decimal aRunHrsToAllocate,
                                                                       decimal aPostProcHrsToAllocate,
                                                                       bool aIntegerAllocation) //, decimal aScrapQtyToAllocate, decimal aSetupHrsToAllocate, decimal aRunHrsToAllocate, decimal aPostProcHrsToAllocate)
    {
        decimal totalGoodQtyAllocated = 0;
        decimal totalScrapQtyAllocated = 0;
        for (int i = 0; i < aSplitOp.Activities.Count; i++)
        {
            InternalActivity aSplitActivity = aSplitOp.Activities.GetByIndex(i);
            decimal activityQtyRatio = aSplitActivity.RequiredFinishQty / aSplitOp.RequiredFinishQty;
            decimal goodQtyToAlloc = activityQtyRatio * aGoodOpQtyToAllocate;
            decimal srapQtyToAlloc = activityQtyRatio * aScrapQtyToAllocate;
            if (aIntegerAllocation)
            {
                goodQtyToAlloc = (int)goodQtyToAlloc;
                srapQtyToAlloc = (int)srapQtyToAlloc;
            }

            aSplitActivity.ReportedGoodQty = goodQtyToAlloc;
            aSplitActivity.ReportedScrapQty = srapQtyToAlloc;

            totalGoodQtyAllocated += goodQtyToAlloc;
            totalScrapQtyAllocated += srapQtyToAlloc;

            //Allocate time; easier because there's no rounding
            decimal hrsRatio = (aSplitActivity.ReportedGoodQty + aSplitActivity.ReportedScrapQty) / (aGoodOpQtyToAllocate + aScrapQtyToAllocate);
            aSplitActivity.ReportedSetup = TimeSpan.FromHours((double)(hrsRatio * aSetupHrsToAllocate)).Ticks;
            aSplitActivity.ReportedRun = TimeSpan.FromHours((double)(hrsRatio * aRunHrsToAllocate)).Ticks;
            aSplitActivity.ReportedPostProcessing = TimeSpan.FromHours((double)(hrsRatio * aPostProcHrsToAllocate)).Ticks;
        }

        //Now decrement the original operation based on what was actually allocated.
        DeAllocateStatusFromActivitiesProportionally(totalGoodQtyAllocated, totalScrapQtyAllocated, aIntegerAllocation, aSetupHrsToAllocate, aRunHrsToAllocate, aPostProcHrsToAllocate);
    }

    /// <summary>
    /// Decrement Reported values from this Operation.
    /// </summary>
    private void DeAllocateStatusFromActivitiesProportionally(decimal a_goodQtyToDeallocateFromOp, decimal a_scrapQtyToDeallocateFromOp, bool a_integerAllocation, decimal a_setupHrsToDeAllocate, decimal a_runHrsToDeAllocate, decimal a_postProcHrsToDeAllocate)
    {
        decimal totalGoodQtyDeallocated = 0;
        decimal totalScrapQtyDeallocated = 0;
        for (int i = 0; i < Activities.Count; i++)
        {
            InternalActivity activity = Activities.GetByIndex(i);
            decimal allocRatio = activity.RequiredFinishQty / RequiredFinishQty;
            decimal goodQtyToDealloc = allocRatio * a_goodQtyToDeallocateFromOp;
            decimal scrapQtyToDealloc = allocRatio * a_scrapQtyToDeallocateFromOp;
            if (a_integerAllocation)
            {
                goodQtyToDealloc = (int)goodQtyToDealloc;
                scrapQtyToDealloc = (int)scrapQtyToDealloc;
            }

            activity.ReportedGoodQty -= goodQtyToDealloc;
            activity.ReportedScrapQty -= scrapQtyToDealloc;
            totalGoodQtyDeallocated += goodQtyToDealloc;
            totalScrapQtyDeallocated += scrapQtyToDealloc;

            //Subtract the hours that were allocated to the split from this, the corresponding original operation
            decimal hrsRatio = (activity.ReportedGoodQty + activity.ReportedScrapQty) / (ReportedGoodQty + ReportedScrapQty);
            activity.ReportedSetup = TimeSpan.FromHours((double)(ReportedSetupHours - hrsRatio * a_setupHrsToDeAllocate)).Ticks;
            activity.ReportedRun = TimeSpan.FromHours((double)(ReportedRunHours - hrsRatio * a_runHrsToDeAllocate)).Ticks;
            activity.ReportedPostProcessing = TimeSpan.FromHours((double)(ReportedPostProcessingHours - hrsRatio * a_postProcHrsToDeAllocate)).Ticks;
        }

        if (Activities.Count > 0)
        {
            InternalActivity firstActivity = Activities.GetByIndex(0);
            //deallocate remainder due to rounding
            if (totalGoodQtyDeallocated < a_goodQtyToDeallocateFromOp)
            {
                firstActivity.ReportedGoodQty -= a_goodQtyToDeallocateFromOp - totalGoodQtyDeallocated;
            }

            if (totalScrapQtyDeallocated < a_scrapQtyToDeallocateFromOp)
            {
                firstActivity.ReportedScrapQty -= a_scrapQtyToDeallocateFromOp - totalScrapQtyDeallocated;
            }
        }
    }

    private class StartDateActivitySorter : IComparer
    {
        #region IComparer Members
        int IComparer.Compare(object x, object y)
        {
            BaseActivity a1 = (BaseActivity)x;
            BaseActivity a2 = (BaseActivity)y;
            return a1.ScheduledStartDate.Ticks.CompareTo(a2.ScheduledStartDate.Ticks);
        }
        #endregion
    }

    /// <summary>
    /// Update this operation using the values in the temporary operation.
    /// </summary>
    /// <param name="a_tempOperation"></param>
    /// <param name="a_tOp"></param>
    /// <param name="a_clock"></param>
    /// <param name="a_erpUpdate"></param>
    /// <param name="a_am"></param>
    /// <param name="a_warehouseManager"></param>
    /// <param name="a_opManager"></param>
    /// <param name="a_productRuleManager"></param>
    /// <param name="a_t"></param>
    /// <param name="a_udfManager"></param>
    /// <param name="a_dataChanges"></param>
    internal bool Update(BaseOperation a_tempOperation,
                         JobT.InternalOperation a_tOp,
                         long a_clock,
                         bool a_erpUpdate,
                         AttributeManager a_am,
                         WarehouseManager a_warehouseManager,
                         BaseOperationManager a_opManager,
                         ProductRuleManager a_productRuleManager,
                         PTTransmission a_t,
                         UserFieldDefinitionManager a_udfManager,
                         IScenarioDataChanges a_dataChanges)
    {
        bool updated = false;
        bool flagConstraintChanges = false;
        bool flagProductionChanges = false;
        bool opIsScheduled = Scheduled;
        //NEWOPFIELD Bookmark
        decimal originalRequiredFinishQty = RequiredFinishQty;
        decimal requiredFinishQtyDelta = a_tempOperation.RequiredFinishQty - RequiredFinishQty;

        InternalOperation tempInternalOperation = (InternalOperation)a_tempOperation;

        if (a_tOp.ProductionInfoManualUpdateOnlyFlags.CycleManualUpdateOnlySet && ProductionInfo.OnlyAllowManualUpdatesToCycleSpan != tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToCycleSpan)
        {
            if (ProductionInfo.OnlyAllowManualUpdatesToCycleSpan != tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToCycleSpan)
            {
                ProductionInfo.OnlyAllowManualUpdatesToCycleSpan = tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToCycleSpan;
                updated = true;
            }
        }

        if (a_tOp.ProductionInfoManualUpdateOnlyFlags.QtyPerCycleSet && ProductionInfo.OnlyAllowManualUpdatesToQtyPerCycle != tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToQtyPerCycle)
        {
            if (ProductionInfo.OnlyAllowManualUpdatesToQtyPerCycle != tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToQtyPerCycle)
            {
                ProductionInfo.OnlyAllowManualUpdatesToQtyPerCycle = tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToQtyPerCycle;
                updated = true;
            }
        }

        if (a_tOp.ProductionInfoManualUpdateOnlyFlags.SetupManualUpdateOnlySet && ProductionInfo.OnlyAllowManualUpdatesToSetupSpan != tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToSetupSpan)
        {
            if (ProductionInfo.OnlyAllowManualUpdatesToSetupSpan != tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToSetupSpan)
            {
                ProductionInfo.OnlyAllowManualUpdatesToSetupSpan = tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToSetupSpan;
                updated = true;
            }
        }

        if (a_tOp.ProductionInfoManualUpdateOnlyFlags.PostProcessingManualUpdateOnlySet && ProductionInfo.OnlyAllowManualUpdatesToPostProcessingSpan != tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToPostProcessingSpan)
        {
            if (ProductionInfo.OnlyAllowManualUpdatesToPostProcessingSpan != tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToPostProcessingSpan)
            {
                ProductionInfo.OnlyAllowManualUpdatesToPostProcessingSpan = tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToPostProcessingSpan;
                updated = true;
            }
        }

        if (a_tOp.ProductionInfoManualUpdateOnlyFlags.CleanManualUpdateOnlySet && ProductionInfo.OnlyAllowManualUpdatesToCleanSpan != tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToCleanSpan)
        {
            if (ProductionInfo.OnlyAllowManualUpdatesToCleanSpan != tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToCleanSpan)
            {
                ProductionInfo.OnlyAllowManualUpdatesToCleanSpan = tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToCleanSpan;
                updated = true;
            }
        }

        if (a_tOp.ProductionInfoManualUpdateOnlyFlags.PlanningScrapPercentSet && ProductionInfo.OnlyAllowManualUpdatesToPlanningScrapPercent != tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToPlanningScrapPercent)
        {
            if (ProductionInfo.OnlyAllowManualUpdatesToPlanningScrapPercent != tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToPlanningScrapPercent)
            {
                ProductionInfo.OnlyAllowManualUpdatesToPlanningScrapPercent = tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToPlanningScrapPercent;
                updated = true;
            }
        }

        if (a_tOp.ProductionInfoManualUpdateOnlyFlags.ResourceRequirementsSet && ProductionInfo.OnlyAllowManualUpdatesToResourceRequirements != tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToResourceRequirements)
        {
            if (ProductionInfo.OnlyAllowManualUpdatesToResourceRequirements != tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToResourceRequirements)
            {
                ProductionInfo.OnlyAllowManualUpdatesToResourceRequirements = tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToResourceRequirements;
                updated = true;
            }
        }

        if (a_tOp.ProductionInfoManualUpdateOnlyFlags.MaterialPostProcessingManualUpdateOnlySet && ProductionInfo.OnlyAllowManualUpdatesToMaterialPostProcessingSpan != tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToMaterialPostProcessingSpan)
        {
            if (ProductionInfo.OnlyAllowManualUpdatesToMaterialPostProcessingSpan != tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToMaterialPostProcessingSpan)
            {
                ProductionInfo.OnlyAllowManualUpdatesToMaterialPostProcessingSpan = tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToMaterialPostProcessingSpan;
                updated = true;
            }
        }

        if (a_tOp.ProductionInfoManualUpdateOnlyFlags.StorageManualUpdateOnlySet && ProductionInfo.OnlyAllowManualUpdatesToStorageSpan != tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToStorageSpan)
        {
            if (ProductionInfo.OnlyAllowManualUpdatesToStorageSpan != tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToStorageSpan)
            {
                ProductionInfo.OnlyAllowManualUpdatesToStorageSpan = tempInternalOperation.ProductionInfo.OnlyAllowManualUpdatesToStorageSpan;
                updated = true;
            }
        }

        updated |= base.Update(a_tempOperation, a_tOp, opIsScheduled, a_clock, a_erpUpdate, a_am, a_warehouseManager, a_opManager, a_t, a_udfManager, a_dataChanges);

        //Update Production Info
        updated |= tempInternalOperation.ProductionInfo.SyncProductionMembersOfThisInstanceInto(ProductionInfo, !a_erpUpdate, opIsScheduled, a_dataChanges);

        if (m_standardSetupSpan != tempInternalOperation.StandardSetupSpan)
        {
            m_standardSetupSpan = tempInternalOperation.StandardSetupSpan;
            updated = true;
            flagProductionChanges = true;
        }

        if (m_standardRunSpan != tempInternalOperation.StandardRunSpan)
        {
            m_standardRunSpan = tempInternalOperation.StandardRunSpan;
            updated = true;
            flagProductionChanges = true;
        }

        if (BatchCode != tempInternalOperation.BatchCode)
        {
            BatchCode = tempInternalOperation.BatchCode;
            updated = true; 
            flagProductionChanges = true;
        }

        if (SetupColor != tempInternalOperation.SetupColor)
        {
            SetupColor = tempInternalOperation.SetupColor;
            updated = true;
        }

        if (SetupNumber != tempInternalOperation.SetupNumber)
        {
            SetupNumber = tempInternalOperation.SetupNumber;
            flagProductionChanges = true;
            updated = true;
        }

        if (DeductScrapFromRequired != tempInternalOperation.DeductScrapFromRequired)
        {
            DeductScrapFromRequired = tempInternalOperation.DeductScrapFromRequired;
            flagProductionChanges = true;
            updated = true;
        }

        if (WholeNumberSplits != tempInternalOperation.WholeNumberSplits)
        {
            WholeNumberSplits = tempInternalOperation.WholeNumberSplits;
            // *LRH*TODO*STATUSUPDTE*You may need to do something else here; like unschedule the job.
            a_dataChanges.FlagEligibilityChanges(Job.Id);
            updated = true;
        }

        if (CanPause != tempInternalOperation.CanPause)
        {
            CanPause = tempInternalOperation.CanPause;
            flagProductionChanges = true;
            updated = true;
        }

        if (CanSubcontract != tempInternalOperation.CanSubcontract)
        {
            CanSubcontract = tempInternalOperation.CanSubcontract;
            updated = true;
        }

        if (CompatibilityCode != tempInternalOperation.CompatibilityCode)
        {
            CompatibilityCode = tempInternalOperation.CompatibilityCode;
            updated = true;
            a_dataChanges.FlagEligibilityChanges(Job.Id);
        }

        if (UseCompatibilityCode != tempInternalOperation.UseCompatibilityCode)
        {
            UseCompatibilityCode = tempInternalOperation.UseCompatibilityCode;

            if (UseCompatibilityCode)
            {
                flagConstraintChanges = true;
            }

            updated = true;
            a_dataChanges.FlagEligibilityChanges(Job.Id);
        }

        if (CarryingCost != tempInternalOperation.CarryingCost)
        {
            CarryingCost = tempInternalOperation.CarryingCost;
            updated = true;
        }

        if (SuccessorProcessing != tempInternalOperation.SuccessorProcessing)
        {
            SuccessorProcessing = tempInternalOperation.SuccessorProcessing;
            updated = true;
        }

        if (KeepSuccessorsTimeLimit != tempInternalOperation.KeepSuccessorsTimeLimit)
        {
            KeepSuccessorsTimeLimit = tempInternalOperation.KeepSuccessorsTimeLimit;
            flagProductionChanges = true;
            updated = true;
        }

        if (OverlapTransferQty != tempInternalOperation.OverlapTransferQty)
        {
            OverlapTransferQty = tempInternalOperation.OverlapTransferQty;
            flagProductionChanges = true;
            updated = true;
        }

        if (CanResize != tempInternalOperation.CanResize)
        {
            CanResize = tempInternalOperation.CanResize;
            a_dataChanges.FlagEligibilityChanges(Job.Id);
            updated = true;
        }

        if (ProductCode != tempInternalOperation.ProductCode)
        {
            ProductCode = tempInternalOperation.ProductCode;
            updated = true;
            a_dataChanges.FlagEligibilityChanges(Job.Id);
        }

        if (AutoSplitInfo.Update(tempInternalOperation.AutoSplitInfo))
        {
            updated = true;
            a_dataChanges.FlagEligibilityChanges(Job.Id);
        }

        if (SetupSplitType != tempInternalOperation.SetupSplitType)
        {
            SetupSplitType = tempInternalOperation.SetupSplitType;
            updated = true;
        }

        // Update RequiredFinishQuantity.
        if (!(a_erpUpdate && ManufacturingOrder.PreserveRequiredQty) && requiredFinishQtyDelta != 0)
        {
            UpdateRequiredFinishQuantity(a_tempOperation, originalRequiredFinishQty, requiredFinishQtyDelta);
            flagProductionChanges = true;
            a_dataChanges.FlagEligibilityChanges(Job.Id);
            updated = true;
        }

        if (!a_erpUpdate || !ProductionInfo.OnlyAllowManualUpdatesToResourceRequirements)
        {
            updated |= ResourceRequirements.Update(tempInternalOperation.ResourceRequirements, a_dataChanges, a_productRuleManager, out bool unscheduledJob, out string unscheduleInfo, out bool jitChanges); 
            if (!ResourceRequirements.PrimaryResourceRequirement.ContainsUsage(MainResourceDefs.usageEnum.Clean))
            {
                foreach (OperationAttribute operationAttribute in Attributes)
                {
                    if (operationAttribute.PTAttribute.AttributeType == PTAttributeDefs.EIncurAttributeType.Clean)
                    {
                        throw new PTValidationException("3050");
                    }
                }
            }

            if (unscheduledJob)
            {
                a_dataChanges.FlagEligibilityChanges(Job.Id);
                Job.Unschedule(false);

                if (unscheduleInfo != null)
                {
                    ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(new[] { Job.Id }, unscheduleInfo, typeof(Job), ScenarioHistory.historyTypes.JobRoutingChanged);
                    flagProductionChanges = true;
                }
            }
            
            if (jitChanges)
            {
                a_dataChanges.FlagJitChanges(Job.Id);
            }
        }

        if (a_tOp.PreventSplitsFromIncurringSetupIsSet && PreventSplitsFromIncurringSetup != a_tOp.PreventSplitsFromIncurringSetup)
        {
            PreventSplitsFromIncurringSetup = a_tOp.PreventSplitsFromIncurringSetup;
            flagProductionChanges = true;
            updated = true;
        }

        if (a_tOp.PreventSplitsFromIncurringCleanIsSet && PreventSplitsFromIncurringClean != a_tOp.PreventSplitsFromIncurringClean)
        {
            PreventSplitsFromIncurringClean = a_tOp.PreventSplitsFromIncurringClean;
            flagProductionChanges = true;
            updated = true;
        }

        if (CampaignCode != a_tOp.CampaignCode)
        {
            CampaignCode = a_tOp.CampaignCode;
            updated = true;
            flagProductionChanges = true;
        }

        if (a_tOp.AllowSameLotInNonEmptyStorageAreaIsSet && AllowSameLotInNonEmptyStorageArea != a_tOp.AllowSameLotInNonEmptyStorageArea)
        {
            AllowSameLotInNonEmptyStorageArea = a_tOp.AllowSameLotInNonEmptyStorageArea;
            updated = true;
            //TODO: Might need to flag something here?
        }

        if (TimeSpan.FromDays(a_tOp.SequenceHeadStartDays).Ticks != TimeSpan.FromDays(SequenceHeadStartDays).Ticks)
        {
            SequenceHeadStartDays = a_tOp.SequenceHeadStartDays;
            updated = true;
            a_dataChanges.FlagJitChanges(Job.Id);
        }

        if (opIsScheduled)
        {
            if (flagProductionChanges)
            {
                a_dataChanges.FlagProductionChanges(Job.Id);
            }

            if (flagConstraintChanges)
            {
                a_dataChanges.FlagConstraintChanges(Job.Id);
            }
        }

        return updated;
    }

    internal bool Edit(ScenarioDetail a_sd, OperationEdit a_opEdit, IScenarioDataChanges a_dataChanges)
    {
        bool updated = base.Edit(a_sd, a_opEdit, a_dataChanges);

        if (a_opEdit.MaterialPostProcessingSpanTicksSet && ProductionInfo.MaterialPostProcessingSpanTicks != a_opEdit.MaterialPostProcessingSpanTicks)
        {
            ProductionInfo.MaterialPostProcessingSpanTicks = a_opEdit.MaterialPostProcessingSpanTicks;
            updated = true;
        }

        if (a_opEdit.OnlyAllowManualUpdatesToCycleSpanSet && ProductionInfo.OnlyAllowManualUpdatesToCycleSpan != a_opEdit.OnlyAllowManualUpdatesToCycleSpan)
        {
            ProductionInfo.OnlyAllowManualUpdatesToCycleSpan = a_opEdit.OnlyAllowManualUpdatesToCycleSpan;
            updated = true;
        }

        if (a_opEdit.OnlyAllowManualUpdatesToQtyPerCycleSet && ProductionInfo.OnlyAllowManualUpdatesToQtyPerCycle != a_opEdit.OnlyAllowManualUpdatesToQtyPerCycle)
        {
            ProductionInfo.OnlyAllowManualUpdatesToQtyPerCycle = a_opEdit.OnlyAllowManualUpdatesToQtyPerCycle;
            updated = true;
        }

        if (a_opEdit.OnlyAllowManualUpdatesToPlanningScrapPercentSet && ProductionInfo.OnlyAllowManualUpdatesToPlanningScrapPercent != a_opEdit.OnlyAllowManualUpdatesToPlanningScrapPercent)
        {
            ProductionInfo.OnlyAllowManualUpdatesToPlanningScrapPercent = a_opEdit.OnlyAllowManualUpdatesToPlanningScrapPercent;
            updated = true;
        }

        if (a_opEdit.OnlyAllowManualUpdatesToPostProcessingSpanSet && ProductionInfo.OnlyAllowManualUpdatesToPostProcessingSpan != a_opEdit.OnlyAllowManualUpdatesToPostProcessingSpan)
        {
            ProductionInfo.OnlyAllowManualUpdatesToPostProcessingSpan = a_opEdit.OnlyAllowManualUpdatesToPostProcessingSpan;
            updated = true;
        }

        if (a_opEdit.OnlyAllowManualUpdatesToCleanSpanSet && ProductionInfo.OnlyAllowManualUpdatesToCleanSpan != a_opEdit.OnlyAllowManualUpdatesToCleanSpan)
        {
            ProductionInfo.OnlyAllowManualUpdatesToCleanSpan = a_opEdit.OnlyAllowManualUpdatesToCleanSpan;
            updated = true;
        }

        if (a_opEdit.OnlyAllowManualUpdatesToSplitOperationSet && ProductionInfo.OnlyAllowManualUpdatesToSplitOperation != a_opEdit.OnlyAllowManualUpdatesToSplitOperation)
        {
            ProductionInfo.OnlyAllowManualUpdatesToSplitOperation = a_opEdit.OnlyAllowManualUpdatesToSplitOperation;
            updated = true;
        }

        if (a_opEdit.OnlyAllowManualUpdatesToSetupSpanSet && ProductionInfo.OnlyAllowManualUpdatesToSetupSpan != a_opEdit.OnlyAllowManualUpdatesToSetupSpan)
        {
            ProductionInfo.OnlyAllowManualUpdatesToSetupSpan = a_opEdit.OnlyAllowManualUpdatesToSetupSpan;
            updated = true;
        }

        if (a_opEdit.PostProcessingSpanSet && ProductionInfo.PostProcessingSpanTicks != a_opEdit.PostProcessingSpan.Ticks)
        {
            ProductionInfo.PostProcessingSpanTicks = a_opEdit.PostProcessingSpan.Ticks;
            updated = true;
        }

        if (a_opEdit.PlanningScrapPercentSet && ProductionInfo.PlanningScrapPercent != a_opEdit.PlanningScrapPercent)
        {
            ProductionInfo.PlanningScrapPercent = a_opEdit.PlanningScrapPercent;
            updated = true;
        }

        if (a_opEdit.OnlyAllowManualUpdatesToResourceRequirementsSet && ProductionInfo.OnlyAllowManualUpdatesToResourceRequirements != a_opEdit.OnlyAllowManualUpdatesToResourceRequirements)
        {
            ProductionInfo.OnlyAllowManualUpdatesToResourceRequirements = a_opEdit.OnlyAllowManualUpdatesToResourceRequirements;
            updated = true;
        }

        if (a_opEdit.OnlyAllowManualUpdatesToMaterialPostProcessingSpanSet && ProductionInfo.OnlyAllowManualUpdatesToMaterialPostProcessingSpan != a_opEdit.OnlyAllowManualUpdatesToMaterialPostProcessingSpan)
        {
            ProductionInfo.OnlyAllowManualUpdatesToMaterialPostProcessingSpan = a_opEdit.OnlyAllowManualUpdatesToMaterialPostProcessingSpan;
            updated = true;
        }

        if (a_opEdit.AutoSplitSet && AutoSplit != a_opEdit.AutoSplit)
        {
            AutoSplit = a_opEdit.AutoSplit;
            a_dataChanges.FlagProductionChanges(Job.Id);
            updated = true;
        }

        if (a_opEdit.TimeBasedReportingSet && TimeBasedReporting != a_opEdit.TimeBasedReporting)
        {
            TimeBasedReporting = a_opEdit.TimeBasedReporting;
            updated = true;
        }

        if (a_opEdit.StandardSetupSpanSet && m_standardSetupSpan != a_opEdit.StandardSetupSpan)
        {
            m_standardSetupSpan = a_opEdit.StandardSetupSpan;
            updated = true;
            a_dataChanges.FlagProductionChanges(Job.Id);
        }

        if (a_opEdit.StandardRunSpanSet && m_standardRunSpan != a_opEdit.StandardRunSpan)
        {
            m_standardRunSpan = a_opEdit.StandardRunSpan;
            updated = true;
            a_dataChanges.FlagProductionChanges(Job.Id);
        }

        if (a_opEdit.StandardCleanSpanIsSet && CleanSpan != a_opEdit.StandardCleanSpan)
        {
            CleanSpan = a_opEdit.StandardCleanSpan;
            updated = true;
            a_dataChanges.FlagProductionChanges(Job.Id);
        }

        if (a_opEdit.CleanoutGradeIsSet && CleanoutGrade != a_opEdit.CleanoutGrade)
        {
            CleanoutGrade = a_opEdit.CleanoutGrade;
            updated = true;
            a_dataChanges.FlagProductionChanges(Job.Id);
        }

        if (a_opEdit.ProductionCleanoutCostIsSet && CleanoutCost != a_opEdit.ProductionCleanoutCost)
        {
            CleanoutCost = a_opEdit.ProductionCleanoutCost;
            updated = true;
            a_dataChanges.FlagProductionChanges(Job.Id);
        }


        if (a_opEdit.StandardStorageSpanIsSet && StorageSpan != a_opEdit.StandardStorageSpan)
        {
            StorageSpan = a_opEdit.StandardStorageSpan;
            updated = true;
            a_dataChanges.FlagProductionChanges(Job.Id);
        }

        if (a_opEdit.BatchCodeSet && BatchCode != a_opEdit.BatchCode)
        {
            BatchCode = a_opEdit.BatchCode;
            updated = true;
            a_dataChanges.FlagProductionChanges(Job.Id);
        }

        if (a_opEdit.SetupColorSet && SetupColor != a_opEdit.SetupColor)
        {
            SetupColor = a_opEdit.SetupColor;
            updated = true;
        }

        if (a_opEdit.SetupNumberSet && SetupNumber != a_opEdit.SetupNumber)
        {
            SetupNumber = a_opEdit.SetupNumber;
            updated = true;
            a_dataChanges.FlagProductionChanges(Job.Id);
        }

        if (a_opEdit.DeductScrapFromRequiredSet && DeductScrapFromRequired != a_opEdit.DeductScrapFromRequired)
        {
            DeductScrapFromRequired = a_opEdit.DeductScrapFromRequired;
            a_dataChanges.FlagProductionChanges(Job.Id);
            updated = true;
        }

        if (a_opEdit.WholeNumberSplitsSet && WholeNumberSplits != a_opEdit.WholeNumberSplits)
        {
            WholeNumberSplits = a_opEdit.WholeNumberSplits;
            a_dataChanges.FlagEligibilityChanges(Job.Id);
            updated = true;
        }

        if (a_opEdit.CanPauseSet && CanPause != a_opEdit.CanPause)
        {
            CanPause = a_opEdit.CanPause;
            updated = true;
            a_dataChanges.FlagProductionChanges(Job.Id);
        }

        if (a_opEdit.CanSubcontractSet && CanSubcontract != a_opEdit.CanSubcontract)
        {
            CanSubcontract = a_opEdit.CanSubcontract;
            updated = true;
        }

        if (a_opEdit.CompatibilityCodeSet && CompatibilityCode != a_opEdit.CompatibilityCode)
        {
            CompatibilityCode = a_opEdit.CompatibilityCode;
            updated = true;
            a_dataChanges.FlagEligibilityChanges(Job.Id);
        }

        if (a_opEdit.UseCompatibilityCodeSet && UseCompatibilityCode != a_opEdit.UseCompatibilityCode)
        {
            UseCompatibilityCode = a_opEdit.UseCompatibilityCode;
            updated = true;
            a_dataChanges.FlagEligibilityChanges(Job.Id);
            if (UseCompatibilityCode)
            {
                a_dataChanges.FlagConstraintChanges(Job.Id);
            }
        }

        if (a_opEdit.CarryingCostSet && CarryingCost != a_opEdit.CarryingCost)
        {
            CarryingCost = a_opEdit.CarryingCost;
            updated = true;
        }

        if (a_opEdit.SuccessorProcessingSet && SuccessorProcessing != a_opEdit.SuccessorProcessing)
        {
            SuccessorProcessing = a_opEdit.SuccessorProcessing;
            updated = true;
        }

        if (a_opEdit.KeepSuccessorsTimeLimitSet && KeepSuccessorsTimeLimit != a_opEdit.KeepSuccessorsTimeLimit)
        {
            KeepSuccessorsTimeLimit = a_opEdit.KeepSuccessorsTimeLimit;
            a_dataChanges.FlagProductionChanges(Job.Id);
            updated = true;
        }

        if (a_opEdit.OverlapTransferQtySet && OverlapTransferQty != a_opEdit.OverlapTransferQty)
        {
            OverlapTransferQty = a_opEdit.OverlapTransferQty;
            a_dataChanges.FlagProductionChanges(Job.Id);
            updated = true;
        }

        if (a_opEdit.CanResizeSet && CanResize != a_opEdit.CanResize)
        {
            CanResize = a_opEdit.CanResize;
            a_dataChanges.FlagEligibilityChanges(Job.Id);
            updated = true;
        }

        if (a_opEdit.CycleSpanSet && CycleSpan != a_opEdit.CycleSpan)
        {
            CycleSpan = a_opEdit.CycleSpan;
            updated = true;
            a_dataChanges.FlagProductionChanges(Job.Id);
        }

        if (a_opEdit.SetupSpanTicksSet && SetupSpanTicks != a_opEdit.SetupSpanTicks)
        {
            SetupSpanTicks = a_opEdit.SetupSpanTicks;
            updated = true;
            a_dataChanges.FlagProductionChanges(Job.Id);
        }

        if (a_opEdit.ProductionSetupCostIsSet && ProductionSetupCost != a_opEdit.ProductionSetupCost)
        {
            ProductionSetupCost = a_opEdit.ProductionSetupCost;
            updated = true;
            a_dataChanges.FlagProductionChanges(Job.Id);
        }

        if (a_opEdit.QtyPerCycleSet && QtyPerCycle != a_opEdit.QtyPerCycle)
        {
            QtyPerCycle = a_opEdit.QtyPerCycle;
            updated = true;
            a_dataChanges.FlagProductionChanges(Job.Id);
        }

        if (a_opEdit.RequiredFinishQtySet && RequiredFinishQty != a_opEdit.RequiredFinishQty)
        {
            // Update RequiredFinishQuantity.
            decimal requiredFinishQtyDelta = a_opEdit.RequiredFinishQty - RequiredFinishQty;

            if (!ManufacturingOrder.PreserveRequiredQty)
            {
                UpdateRequiredFinishQuantity(a_opEdit.RequiredFinishQty);
                updated = true;
                a_dataChanges.FlagProductionChanges(Job.Id);
                a_dataChanges.FlagEligibilityChanges(Job.Id);
            }
        }

        if (a_opEdit.ProductCodeSet && ProductCode != a_opEdit.ProductCode)
        {
            ProductCode = a_opEdit.ProductCode;
            updated = true;
            a_dataChanges.FlagEligibilityChanges(Job.Id);
        }

        if (AutoSplitInfo.Edit(a_opEdit))
        {
            updated = true;
            a_dataChanges.FlagEligibilityChanges(Job.Id);
        }

        if (a_opEdit.SetupSplitTypeIsSet && SetupSplitType != a_opEdit.SetupSplitType)
        {
            SetupSplitType = a_opEdit.SetupSplitType;
            updated = true;
            a_dataChanges.FlagProductionChanges(Job.Id);
        }

        if (a_dataChanges.HasProductionChanges(Job.Id) && Scheduled)
        {
            //TODO: change this?
            ManufacturingOrder.ScenarioDetail.CriticalOperationUpdate();
        }

        if (a_opEdit.PreventSplitsFromIncurringSetupIsSet && PreventSplitsFromIncurringSetup != a_opEdit.PreventSplitsFromIncurringSetup)
        {
            PreventSplitsFromIncurringSetup = a_opEdit.PreventSplitsFromIncurringSetup;
            updated = true;
        }

        if (a_opEdit.PreventSplitsFromIncurringCleanIsSet && PreventSplitsFromIncurringClean != a_opEdit.PreventSplitsFromIncurringClean)
        {
            PreventSplitsFromIncurringClean = a_opEdit.PreventSplitsFromIncurringClean;
            updated = true;
        }

        if (a_opEdit.CampaignCodeSet && CampaignCode != a_opEdit.CampaignCode)
        {
            CampaignCode = a_opEdit.CampaignCode;
            updated = true;
            a_dataChanges.FlagProductionChanges(Job.Id);
        }

        if (a_opEdit.SequenceHeadStartDaysIsSet && Math.Abs(Convert.ToDouble(a_opEdit.SequenceHeadStartDays) - SequenceHeadStartDays) > 0)
        {
            m_sequenceHeadStartDays = Convert.ToDouble(a_opEdit.SequenceHeadStartDays);
            a_dataChanges.FlagJitChanges(Job.Id);
            updated = true;
        }

        InternalActivity firstOrDefaultActivity = Activities.FirstOrDefault(x => !x.Finished);
        if (firstOrDefaultActivity != null 
            && firstOrDefaultActivity.ScheduledOrDefaultProductionInfo.MaterialPostProcessingSpanTicks != a_opEdit.UnfinishedActivityMaterialPostProcessingTicks)
        {
            firstOrDefaultActivity.ScheduledOrDefaultProductionInfo.MaterialPostProcessingSpanTicks = a_opEdit.UnfinishedActivityMaterialPostProcessingTicks;
            updated = true;
        }

        if (a_opEdit.AllowSameLotInNonEmptyStorageAreaIsSet && AllowSameLotInNonEmptyStorageArea != a_opEdit.AllowSameLotInNonEmptyStorageArea)
        {
            AllowSameLotInNonEmptyStorageArea = a_opEdit.AllowSameLotInNonEmptyStorageArea;
            updated = true;
        }

        return updated;
    }

    #region Update the required finish quantity including the required finish quantities of the activities.
    private void UpdateRequiredFinishQuantity(BaseOperation a_tempOperation, decimal a_originalRequiredFinishQty, decimal a_requiredFinishQtyDelta)
    {
        RequiredFinishQty = a_tempOperation.RequiredFinishQty;
        UpdateRequiredFinishQuantity(a_tempOperation.RequiredFinishQty, a_originalRequiredFinishQty, a_requiredFinishQtyDelta);
    }

    internal void UpdateRequiredFinishQuantity(decimal a_newRequiredQty)
    {
        UpdateRequiredFinishQuantity(a_newRequiredQty, RequiredFinishQty, a_newRequiredQty - RequiredFinishQty);
        RequiredFinishQty = a_newRequiredQty;
    }

    private void UpdateRequiredFinishQuantity(decimal a_newRequiredFinishQty, decimal a_originalRequiredFinishQty, decimal a_requiredFinishQtyDelta)
    {
        if (a_requiredFinishQtyDelta != 0)
        {
            if (m_activities.Count == 1)
            {
                InternalActivity activity = Activities.GetByIndex(0);
                activity.RequiredFinishQty = a_newRequiredFinishQty;
                if (Scheduled)
                {
                    ManufacturingOrder.ScenarioDetail.ActivityRequiredFinishQuantityChange();
                }
            }
            else
            {
                if (a_requiredFinishQtyDelta > 0)
                {
                    InternalActivity activity = Activities.GetFirstInPreproduction();

                    if (activity != null)
                    {
                        activity.RequiredFinishQty += a_requiredFinishQtyDelta;
                        if (activity.Scheduled)
                        {
                            ManufacturingOrder.ScenarioDetail.ActivityRequiredFinishQuantityChange();
                        }
                    }
                    else if ((activity = Activities.GetFirstInProduction()) != null)
                    {
                        activity.RequiredFinishQty += a_requiredFinishQtyDelta;
                        if (activity.Scheduled)
                        {
                            ManufacturingOrder.ScenarioDetail.ActivityRequiredFinishQuantityChange();
                        }
                    }
                    else
                    {
                        activity = Activities.GetByIndex(0);
                        activity.RequiredFinishQty += a_requiredFinishQtyDelta;
                    }
                }
                else if (a_requiredFinishQtyDelta < 0)
                {
                    // The remaining amount that RequiredFinishQty must be reduced among activities.
                    decimal subtractQty = -a_requiredFinishQtyDelta;

                    // All the the activities in the preproduction state.
                    InternalActivityList preproductionList;

                    // The activities in the production state.
                    InternalActivityList productionList;

                    // The activities in the post production state.
                    InternalActivityList postProductionList;

                    Activities.GetActivityLists(out preproductionList, out productionList, out postProductionList);

                    // 1
                    // The total reported finish quantity across all activities.
                    decimal totalReportedFinishQty = m_activities.CalcTotalReportedFinishQty();

                    // 2
                    // The amount of material that must be produced to finish off this operation.
                    decimal originalRemainingProductionQty = a_originalRequiredFinishQty - totalReportedFinishQty;
                    originalRemainingProductionQty = Math.Max(originalRemainingProductionQty, 0);

                    // 3
                    // The remaining production quantity among all the activities within the preproduction state, before any quantity updates.
                    decimal originalRemainingQtyInPreproduction = InternalActivityManager.CalcRemainingProductionQty(preproductionList);

                    // 4
                    // The remainging production quantity among all the activities within the production state, before any quantity updates.
                    decimal originalRemainingQtyInProduction = InternalActivityManager.CalcRemainingProductionQty(productionList);

                    // 6
                    // The original remaining production quantity.
                    decimal originalRemainingQtyOfNonFinishedActivities = originalRemainingQtyInPreproduction + originalRemainingQtyInProduction;

                    // 5
                    // The total number of remaining pieces that need to be made.
                    decimal remainingProductionQty = RequiredFinishQty - totalReportedFinishQty;
                    remainingProductionQty = Math.Max(remainingProductionQty, 0);

                    // 7
                    // The amount that production quantity must be reduced.
                    decimal reduceProductionQty = originalRemainingProductionQty - remainingProductionQty;
                    reduceProductionQty = Math.Max(reduceProductionQty, 0);

                    // 8
                    // The total amount reductions must eat into the finished quantity. 
                    // For instance: RequiredFinishQuantity=10. Finished=5. Reduction=7. subtractFinishQty=1. The end result to the adjustment of this activities
                    // quantities would be RequiredFinishQty=4. The remaining amount of reduction would need to be taken off of some other activity.
                    decimal subtractFinishQty = subtractQty - reduceProductionQty;

                    // First try to reduce the quantity of the preproduction activities.
                    if (AdjustRequiredFinishQtyNonFinishedActivities(ref reduceProductionQty, ref subtractFinishQty, preproductionList))
                    {
                        if (AdjustRequiredFinishQtyNonFinishedActivities(ref reduceProductionQty, ref subtractFinishQty, productionList))
                        {
                            AdjustRequiredFinishQtyNonFinishedActivities(ref reduceProductionQty, ref subtractFinishQty, postProductionList);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Decrement required finish quantities of activities in the preproduction state.
    /// </summary>
    /// <param name="subtractQty">The total amount the RequiredFinishQuantity needs to be decremented among all activities.</param>
    /// <param name="preproductionList"></param>
    /// <returns></returns>
    private bool AdjustRequiredFinishQtyNonFinishedActivities(ref decimal r_reduceProductionQty, ref decimal r_subtractFinishQty, InternalActivityList a_activitiesList)
    {
        InternalActivity activity;

        // First work on deductions to the activities that are in preproduction. Deleting activities as necessary.
        while ((MathStatics.IsGreaterThan(r_reduceProductionQty, 0) || MathStatics.IsGreaterThan(r_subtractFinishQty, 0)) && a_activitiesList.Count > 0)
        {
            InternalActivityList.Node smallest = GetSmallest(a_activitiesList);
            activity = smallest.Data;
            a_activitiesList.Remove(smallest);

            // Adjust the remaining production quantity.
            if (activity.RemainingQty > 0 && r_reduceProductionQty > 0)
            {
                if (MathStatics.IsEqual(activity.RemainingQty, r_reduceProductionQty) || activity.RemainingQty > r_reduceProductionQty)
                {
                    activity.RequiredFinishQty -= r_reduceProductionQty;
                    r_reduceProductionQty = 0;
                    ReportActivityRequiredFinishQuantityChange(activity);
                }
                else
                {
                    r_reduceProductionQty -= activity.RemainingQty;
                    activity.RequiredFinishQty -= activity.RemainingQty;
                    ReportActivityRequiredFinishQuantityChange(activity);
                }
            }

            // Make adjustments for the required good quantity.
            if (activity.ReportedGoodQty > 0 && r_subtractFinishQty > 0)
            {
                // The limits on reduction are the smallest of:
                // good quantity, remainingFinishQuantity, and subtractFinishQty.
                decimal reductionQty = Math.Min(activity.ReportedGoodQty, activity.RequiredFinishQty);
                reductionQty = Math.Min(reductionQty, r_subtractFinishQty);
                activity.RequiredFinishQty -= reductionQty;

                if (MathStatics.IsEqual(reductionQty, r_subtractFinishQty))
                {
                    r_subtractFinishQty = 0;
                }
                else
                {
                    r_subtractFinishQty -= reductionQty;
                }
            }
        }

        return MathStatics.IsGreaterThan(r_reduceProductionQty, 0) || MathStatics.IsGreaterThan(r_subtractFinishQty, 0);
    }

    private void ReportActivityRequiredFinishQuantityChange(InternalActivity a_activity)
    {
        if (a_activity.Scheduled)
        {
            ManufacturingOrder.ScenarioDetail.ActivityRequiredFinishQuantityChange();
        }
    }

    /// <summary>
    /// This is a helper function whose purpose is to return the node with the smallest required finish quantity.
    /// </summary>
    /// <param name="activityList"></param>
    /// <returns>null if there are no nodes in the list. Among nodes with equal values, nodes towards the end of the list are returned.</returns>
    private InternalActivityList.Node GetSmallest(InternalActivityList a_activityList)
    {
        InternalActivityList.Node currentNode = a_activityList.First;
        InternalActivityList.Node bestNode = null;

        while (currentNode != null)
        {
            if (bestNode == null)
            {
                bestNode = currentNode;
            }
            else
            {
                InternalActivity currentActivity = currentNode.Data;
                InternalActivity bestActivity = bestNode.Data;
                if (currentActivity.RequiredFinishQty <= bestActivity.RequiredFinishQty)
                {
                    bestNode = currentNode;
                }
            }

            currentNode = currentNode.Next;
        }

        return bestNode;
    }
    #endregion
    #endregion

    #region Deletion Validation
    internal void ResourceDeleteNotification(BaseResource a_r)
    {
        for (int i = 0; i < ResourceRequirements.Count; ++i)
        {
            ResourceRequirement rr = ResourceRequirements.GetByIndex(i);
            rr.ResourceDeleteNotification(a_r);
        }

        foreach (InternalActivity activity in Activities)
        {
            activity.ResourceDeleteNotification(a_r);
        }
    }
    #endregion

    #region KPI calculations
    /// <summary>
    /// Totals the Setup Hours for all Activities scheduled to start before or on the specified date.
    /// </summary>
    /// <returns></returns>
    public long SumOfSetupHoursTicks(DateTime a_dt, bool a_includeLaborResourcesOnly)
    {
        long total = 0;
        for (int i = 0; i < Activities.Count; i++)
        {
            InternalActivity activity = Activities.GetByIndex(i); if (activity.Scheduled && activity.ScheduledStartDate <= a_dt && activity.StandardSetupTicks > 0)
            {
                if (a_includeLaborResourcesOnly)
                {
                    for (int blockI = 0; blockI < activity.ResourceRequirementBlockCount; blockI++)
                    {
                        ResourceBlock block = activity.GetResourceRequirementBlock(blockI);
                        if (block != null)
                        {
                            if (block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Employee ||
                                block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Engineer ||
                                block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Inspector ||
                                block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Labor ||
                                block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Operator ||
                                block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Supervisor ||
                                block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Team ||
                                block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Technician)
                            {
                                total += activity.ScheduledSetupTicks;
                            }
                        }
                    }
                }
                else
                {
                    total += activity.ScheduledSetupTicks;
                }
            }
        }

        return total;
    }

    /// <summary>
    /// Totals the Setup Cost for all Activities scheduled to start before or on the specified date.
    /// </summary>
    /// <returns></returns>
    public decimal SumOfSetupCost(DateTime a_dt, bool a_includeLaborResourcesOnly)
    {
        decimal total = 0;
        for (int i = 0; i < Activities.Count; i++)
        {
            InternalActivity activity = Activities.GetByIndex(i);
            if (activity.Scheduled && activity.ScheduledStartDate <= a_dt && activity.StandardSetupTicks > 0)
            {
                if (a_includeLaborResourcesOnly)
                {
                    for (int blockI = 0; blockI < activity.ResourceRequirementBlockCount; blockI++)
                    {
                        ResourceBlock block = activity.GetResourceRequirementBlock(blockI);
                        if (block != null)
                        {
                            if (block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Employee ||
                                block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Engineer ||
                                block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Inspector ||
                                block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Labor ||
                                block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Operator ||
                                block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Supervisor ||
                                block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Team ||
                                block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Technician)
                            {
                                total += (decimal)activity.ScheduledSetupSpan.TotalHours * block.ScheduledResource.StandardHourlyCost;
                            }
                        }
                    }
                }
                else
                {
                    total += (decimal)activity.ScheduledSetupSpan.TotalHours * activity.PrimaryResourceRequirementBlock.ScheduledResource.StandardHourlyCost;
                }
            }
        }

        return total;
    }
    /// <summary>
    /// Totals the Clean Hours for all Activities scheduled to start before or on the specified date.
    /// </summary>
    /// <returns></returns>
    public long SumOfCleanHoursTicks(DateTime a_dt, bool a_includeLaborResourcesOnly)
    {
        long total = 0;
        for (int i = 0; i < Activities.Count; i++)
        {
            InternalActivity activity = Activities.GetByIndex(i);
            if (activity.Scheduled && activity.ScheduledStartDate <= a_dt && activity.ScheduledCleanTicks > 0)
            {
                if (a_includeLaborResourcesOnly)
                {
                    for (int blockI = 0; blockI < activity.ResourceRequirementBlockCount; blockI++)
                    {
                        ResourceBlock block = activity.GetResourceRequirementBlock(blockI);
                        if (block != null)
                        {
                            if (block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Employee ||
                                block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Engineer ||
                                block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Inspector ||
                                block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Labor ||
                                block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Operator ||
                                block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Supervisor ||
                                block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Team ||
                                block.ScheduledResource.ResourceType == BaseResourceDefs.resourceTypes.Technician)
                            {
                                total += activity.ScheduledCleanTicks;
                            }
                        }
                    }
                }
                else
                {
                    total += activity.ScheduledCleanTicks;
                }
            }
        }

        return total;
    }
    #endregion KPI Calculations

    /// <summary>
    /// Whether the Operation has been split into multiple Activities.
    /// </summary>
    public bool Split => Activities.Count > 1;

    /// <summary>
    /// Returns an array list of Capabilities for the Operation's Resource Requirements that have no Active eligible resources.
    /// </summary>
    /// <returns></returns>
    public SortedList<BaseId, Capability> GetCapabilitiesWithoutActiveResources()
    {
        SortedList<BaseId, Capability> problemCapabilities = new ();
        for (int i = 0; i < m_resourceRequirements.Count; i++)
        {
            ResourceRequirement rr = m_resourceRequirements.GetByIndex(i);
            SortedList<BaseId, Capability> rrProblemCapabilities = rr.GetCapabilitiesWithoutActiveResources();
            for (int j = 0; j < rrProblemCapabilities.Count; j++)
            {
                Capability cap = rrProblemCapabilities.Values[j];
                if (!problemCapabilities.ContainsKey(cap.Id))
                {
                    problemCapabilities.Add(cap.Id, cap);
                }
            }
        }

        return problemCapabilities;
    }

    /// <summary>
    /// The quanity to start at this Operation.  This takes into account Planning Scrap Percent and so it is usually more than the Required Finish Quantity.
    /// If Whole Number Splits is checked then this value is rounded up to the nearest whole number.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public decimal RequiredStartQty
    {
        get
        {
            if (WholeNumberSplits)
            {
                return Math.Ceiling(PlanningScrapPercentAdjustedQty(PlanningScrapPercent, RequiredFinishQty));
            }

            return PlanningScrapPercentAdjustedQty(PlanningScrapPercent, RequiredFinishQty);
        }
    }

    /// <summary>
    /// The quantity expected to be scrapped.  This is the Required Start Qty times the Planning Scrap Percent.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public decimal ExpectedScrapQty => PlanningScrapQty(PlanningScrapPercent, RequiredStartQty);

    /// <summary>
    /// Represents the campaign identifier for an operation. 
    /// A campaign is defined as a sequence of operations on a resource that produce the same product or belong to a specific campaign group.
    /// 
    /// This property is used to group operations as part of a campaign and is critical for optimization factors that prioritize or enforce campaign scheduling.
    /// </summary>
    private string m_campaignCode = "";
    public string CampaignCode
    {
        get => m_campaignCode;
        private set => m_campaignCode = value;
    }


    /// <summary>
    /// Update the Material Requirement Issued quantities and Inventory On-Hand quantities based on the qty being completed/scrapped.
    /// This function was transferred from BaseOperation.cs on 2013.02.04.
    /// </summary>
    /// <param name="a_actId"></param>
    /// <param name="a_qtyBeingCompleted"></param>
    /// <param name="a_actFinished"></param>
    internal void AutoIssueMaterials(BaseId a_actId, decimal a_qtyBeingCompleted, bool a_actFinished)
    {
        if (a_qtyBeingCompleted == 0)
        {
            return;
        }

        for (int i = 0; i < MaterialRequirements.Count; i++)
        {
            MaterialRequirement mr = MaterialRequirements[i];
            //No need to do anything if the material has been fully issued
            if (mr.IssuedComplete || (!a_actFinished && mr.Item.ItemType == ItemDefs.itemTypes.Tool))
            {
                continue;
            }

            //Calculate qty to issue based on ratio of material qty and product qty
            decimal materialRatio = mr.TotalRequiredQty / RequiredFinishQty;
            decimal materialToIssue = materialRatio * a_qtyBeingCompleted;
            decimal remainingQtyToIssue = materialToIssue;

            if (mr.BuyDirect)
            {
                //There is no inventory for BuyDirects, we don't need to subtract on-hand
                continue;
            }

            if (mr.MRSupply == null)
            {
                //This is possible if this AutoFinish call came from the MO constructor of an MO Update.
                //TODO: This doesn't make any sense. Also at this point products have already been auto updated...
                continue;
            }

            IEnumerable<Tuple<Lot, decimal>> mrSupplyingLots = mr.GetSupplyingLotByDemandId(a_actId);
            foreach ((Lot lot, decimal qty) in mrSupplyingLots)
            {
                decimal qtyToIssue = Math.Min(qty, remainingQtyToIssue);
                lot.IssueMaterial(qtyToIssue);
                mr.IssuedQty += qtyToIssue;
                remainingQtyToIssue -= qtyToIssue;
            }

#if DEBUG
            if (remainingQtyToIssue != 0)
            {
                if (mr.MRSupply.Any())
                {
                    SchedulerDebugException.ThrowIfNotMassRecordings(string.Format("Not all material was auto issued for Job: '{0}' MO: '{1}' Op: '{2}'", Job.Name, ManufacturingOrder.Name, Name));
                }
            }
            #endif
        }
    }

    internal bool PostSimStageCust(ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd, int a_currentSimStageIdx, int a_lastSimStageIdx)
    {
        bool computeEligibility = false;
        for (int i = 0; i < ResourceRequirements.Count; i++)
        {
            computeEligibility = ResourceRequirements.GetByIndex(i).PostSimStageCust(a_simType, a_t, a_sd, a_currentSimStageIdx, a_lastSimStageIdx) || computeEligibility;
        }

        return computeEligibility;
    }

    internal bool EndOfSimulationCustExecute(ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd)
    {
        bool computeEligibility = false;
        for (int i = 0; i < ResourceRequirements.Count; i++)
        {
            computeEligibility = ResourceRequirements.GetByIndex(i).EndOfSimulationCustExecute(a_simType, a_t, a_sd) || computeEligibility;
        }

        return computeEligibility;
    }

    public override void ApplyPlannedScrapQty()
    {
        base.ApplyPlannedScrapQty();
        if (PlannedScrapQty == 0)
        {
            return;
        }

        Activities.AddRequiredQty(PlannedScrapQty);
    }

    /// <summary>
    /// Calculates latest availability (including material post processing)
    /// </summary>
    /// <returns></returns>
    public long CalculateLatestMaterialAvailabilityTicks(ScenarioDetail a_sd = null)
    {
        long latestDate = PTDateTime.MinDateTimeTicks;

        foreach (MaterialRequirement mr in MaterialRequirements)
        {
            long dt = mr.GetLatestSupplySourceDate(this, a_sd).Ticks;
            latestDate = Math.Max(latestDate, dt);
        }

        return latestDate;
    }

    /// <summary>
    /// Calculates latest availability (including material post processing)
    /// </summary>
    /// <returns></returns>
    public DateTime CalculateLatestMaterialAvailability()
    {
        long latestDate = CalcEndOfResourceTransferTimeTicks();
        return new DateTime(latestDate);
    }

    /// <summary>
    /// Verifies that OverlapTransferQty is > 0
    /// <param name="a_automaticallyResolveErrors">Whether to alter transfer quantity as needed to avoid validation errors</param>
    /// </summary>
    internal override void ValidateOverlapTransferQuantity(bool a_automaticallyResolveErrors)
    {
        if (OverlapTransferQty <= 0)
        {
            if (a_automaticallyResolveErrors)
            {
                //It's possible that data was stored in an invalid state and we need to recover. For example throwing an exception could cause the system to fail to start.
                OverlapTransferQty = 1;
                return;
            }

            throw new PTValidationException("4134", new object[] { ManufacturingOrder.Job.ExternalId, ManufacturingOrder.ExternalId, ExternalId });
        }
    }

    /// <summary>
    /// Check if any of the activities on this operation are locked to a resource. Return the first one found.
    /// </summary>
    /// <returns></returns>
    public Resource GetFirstLockedResource()
    {
        foreach (InternalActivity internalActivity in Activities)
        {
            if (internalActivity.Locked != lockTypes.Unlocked)
            {
                for (int i = 0; i < ResourceRequirements.Count; i++)
                {
                    if (internalActivity.ResourceRequirementLock(i) is Resource lockedResource)
                    {
                        return lockedResource;
                    }
                }
            }
        }

        return null;
    }

    internal override void Deleting(PlantManager a_plantManager, IScenarioDataChanges a_dataChanges)
    {
        foreach (InternalActivity activity in Activities)
        {
            activity.Deleting(a_plantManager, a_dataChanges);
        }

        foreach (MaterialRequirement materialRequirement in MaterialRequirements)
        {
            materialRequirement.Deleting();
        }
    }
    private (LatestConstraintEnum, long, object)? m_tempConstraints;
    /// <summary>
    /// Temporarily anchors this operation.
    /// </summary>
    internal void TempAnchor()
    {
        object constraintObject = null;
        if (LatestConstraintInternal == LatestConstraintEnum.MaterialRequirement)
        {
            constraintObject = m_latestConstraintMaterialRequirement;
        }
        else if (LatestConstraintInternal == LatestConstraintEnum.PredecessorOperation)
        {
            constraintObject = m_latestConstraintPredecessorOperation;
        }

        m_tempConstraints = new ValueTuple<LatestConstraintEnum, long, object>()
        {
            Item1 = LatestConstraintInternal,
            Item2 = LatestConstraintTicks,
            Item3 = constraintObject
        };
    }
    /// <summary>
    /// Clear temporary anchor.
    /// </summary>
    internal void TempAnchorClear()
    {
        if (LatestConstraintInternal == LatestConstraintEnum.AnchorDate && m_tempConstraints.HasValue)
        {
            if (m_tempConstraints.Value.Item1 == LatestConstraintEnum.MaterialRequirement)
            {
                SetLatestConstraintToMaterialRequirement(0, m_tempConstraints.Value.Item2, (MaterialRequirement)m_tempConstraints.Value.Item3);
            }
            else if (m_tempConstraints.Value.Item1 == LatestConstraintEnum.PredecessorOperation)
            {
                SetLatestConstraintPredecessorOp(0, m_tempConstraints.Value.Item2, (InternalOperation)m_tempConstraints.Value.Item3);
            }
            else
            {
                SetLatestConstraint((long)m_tempConstraints?.Item2!, (LatestConstraintEnum)m_tempConstraints?.Item1!);
            }
            
            m_tempConstraints = null;
        }
    }
}
