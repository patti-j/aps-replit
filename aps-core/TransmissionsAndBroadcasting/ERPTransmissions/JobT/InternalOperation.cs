using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Text;

using PT.APSCommon;
using PT.Common.Localization;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

public partial class JobT
{
    public abstract class InternalOperation : BaseOperation, IPTSerializable
    {
        public new const int UNIQUE_ID = 229;

        #region PT Serialization
        public InternalOperation() { } // reqd. for xml serialization

        public InternalOperation(IReader reader)
            : base(reader)
        {
            if (reader.VersionNumber >= 13013)
            {
                m_bools = new BoolVector32(reader);
                m_isSetBools = new BoolVector32(reader);

                reader.Read(out canPause);
                reader.Read(out timeBasedReporting);
                reader.Read(out canSubcontract);
                reader.Read(out carryingCost);
                reader.Read(out compatibilityCode);
                reader.Read(out useCompatibilityCode);
                reader.Read(out overlapTransferQty);
                //reader.Read(out setupCode);
                reader.Read(out setupNumber);
                reader.Read(out wholeNumberSplits);
                reader.Read(out deductScrapFromRequired);
                reader.Read(out int val);
                successorProcessing = (OperationDefs.successorProcessingEnumeration)val;
                reader.Read(out val);
                splitUpdateMode = (InternalOperationDefs.splitUpdateModes)val;
                reader.Read(out keepSuccessorTimeLimit);

                reader.Read(out cycleSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out cleanSpan);
                reader.Read(out cleanOutGrade);
                reader.Read(out m_cleanoutCost);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_endOfStoragePostProcessingTimeSpan);

                reader.Read(out setupColor);
                reader.Read(out setupSpan);
                reader.Read(out m_productionSetupCost);

                reader.Read(out m_batchCode);

                reader.Read(out standardRunSpan);
                reader.Read(out standardSetupSpan);

                reader.Read(out string primaryResourceRequirementExternalId);

                reader.Read(out int resReqtsCount);
                for (int i = 0; i < resReqtsCount; i++)
                {
                    ResourceRequirement rr = new(reader);
                    if (rr.ExternalId == "")
                    {
                        rr.ExternalId = i.ToString(); //this is a fix up because we let blanks slip in before and this allows those recordings to continue to function
                    }

                    AddResourceRequirement(rr);
                    if (rr.ExternalId == primaryResourceRequirementExternalId)
                    {
                        primaryResourceRequirement = rr;
                    }
                }

                reader.Read(out int intActivityCount);
                for (int i = 0; i < intActivityCount; i++)
                {
                    InternalActivity activity = new(reader);
                    AddInternalActivity(activity);
                }

                _productionInfoFlags = new ProductionInfoFlags(reader);
                reader.Read(out short typeVal);
                m_type = (OperationDefs.EAutoSplitType)typeVal;
                reader.Read(out m_minAutoSplitAmount);
                reader.Read(out m_maxAutoSplitAmount);
                reader.Read(out typeVal);
                m_setupSplitType = (OperationDefs.ESetupSplitType)typeVal;
                reader.Read(out m_campaignCode);
                reader.Read(out m_sequenceHeadStartDays);
            }
            else if (reader.VersionNumber >= 13004)
            {
                m_bools = new BoolVector32(reader);

                reader.Read(out canPause);
                reader.Read(out timeBasedReporting);
                reader.Read(out canSubcontract);
                reader.Read(out carryingCost);
                reader.Read(out compatibilityCode);
                reader.Read(out useCompatibilityCode);
                reader.Read(out overlapTransferQty);
                //reader.Read(out setupCode);
                reader.Read(out setupNumber);
                reader.Read(out wholeNumberSplits);
                reader.Read(out deductScrapFromRequired);
                reader.Read(out int val);
                successorProcessing = (OperationDefs.successorProcessingEnumeration)val;
                reader.Read(out val);
                splitUpdateMode = (InternalOperationDefs.splitUpdateModes)val;
                reader.Read(out keepSuccessorTimeLimit);

                reader.Read(out cycleSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out cleanSpan);
                reader.Read(out cleanOutGrade);
                reader.Read(out m_cleanoutCost);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_endOfStoragePostProcessingTimeSpan);

                reader.Read(out setupColor);
                reader.Read(out setupSpan);
                reader.Read(out m_productionSetupCost);

                reader.Read(out m_batchCode);

                reader.Read(out standardRunSpan);
                reader.Read(out standardSetupSpan);

                reader.Read(out string primaryResourceRequirementExternalId);

                reader.Read(out int resReqtsCount);
                for (int i = 0; i < resReqtsCount; i++)
                {
                    ResourceRequirement rr = new(reader);
                    if (rr.ExternalId == "")
                    {
                        rr.ExternalId = i.ToString(); //this is a fix up because we let blanks slip in before and this allows those recordings to continue to function
                    }

                    AddResourceRequirement(rr);
                    if (rr.ExternalId == primaryResourceRequirementExternalId)
                    {
                        primaryResourceRequirement = rr;
                    }
                }

                reader.Read(out int intActivityCount);
                for (int i = 0; i < intActivityCount; i++)
                {
                    InternalActivity activity = new(reader);
                    AddInternalActivity(activity);
                }

                _productionInfoFlags = new ProductionInfoFlags(reader);
                reader.Read(out short typeVal);
                m_type = (OperationDefs.EAutoSplitType)typeVal;
                reader.Read(out m_minAutoSplitAmount);
                reader.Read(out m_maxAutoSplitAmount);
                reader.Read(out typeVal);
                m_setupSplitType = (OperationDefs.ESetupSplitType)typeVal;
                reader.Read(out m_campaignCode);
                reader.Read(out m_sequenceHeadStartDays);
            }
            else if (reader.VersionNumber >= 12526)
            {
                m_bools = new BoolVector32(reader);

                reader.Read(out canPause);
                reader.Read(out timeBasedReporting);
                reader.Read(out canSubcontract);
                reader.Read(out carryingCost);
                reader.Read(out compatibilityCode);
                reader.Read(out useCompatibilityCode);
                reader.Read(out overlapTransferQty);
                //reader.Read(out setupCode);
                reader.Read(out setupNumber);
                reader.Read(out wholeNumberSplits);
                reader.Read(out deductScrapFromRequired);
                reader.Read(out int val);
                successorProcessing = (OperationDefs.successorProcessingEnumeration)val;
                reader.Read(out val);
                splitUpdateMode = (InternalOperationDefs.splitUpdateModes)val;
                reader.Read(out keepSuccessorTimeLimit);

                reader.Read(out cycleSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out cleanSpan);
                reader.Read(out cleanOutGrade);
                reader.Read(out m_cleanoutCost);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_endOfStoragePostProcessingTimeSpan);

                reader.Read(out setupColor);
                reader.Read(out setupSpan);
                reader.Read(out m_productionSetupCost);

                reader.Read(out m_batchCode);

                reader.Read(out standardRunSpan);
                reader.Read(out standardSetupSpan);

                reader.Read(out string primaryResourceRequirementExternalId);

                reader.Read(out int resReqtsCount);
                for (int i = 0; i < resReqtsCount; i++)
                {
                    ResourceRequirement rr = new(reader);
                    if (rr.ExternalId == "")
                    {
                        rr.ExternalId = i.ToString(); //this is a fix up because we let blanks slip in before and this allows those recordings to continue to function
                    }

                    AddResourceRequirement(rr);
                    if (rr.ExternalId == primaryResourceRequirementExternalId)
                    {
                        primaryResourceRequirement = rr;
                    }
                }

                reader.Read(out int intActivityCount);
                for (int i = 0; i < intActivityCount; i++)
                {
                    InternalActivity activity = new(reader);
                    AddInternalActivity(activity);
                }

                _productionInfoFlags = new ProductionInfoFlags(reader);
                reader.Read(out short typeVal);
                m_type = (OperationDefs.EAutoSplitType)typeVal;
                reader.Read(out m_minAutoSplitAmount);
                reader.Read(out m_maxAutoSplitAmount);
                reader.Read(out typeVal);
                m_setupSplitType = (OperationDefs.ESetupSplitType)typeVal;
                reader.Read(out m_campaignCode);
            }
            else if (reader.VersionNumber >= 12508)
            {
                m_bools = new BoolVector32(reader);

                reader.Read(out canPause);
                reader.Read(out timeBasedReporting);
                reader.Read(out canSubcontract);
                reader.Read(out carryingCost);
                reader.Read(out compatibilityCode);
                reader.Read(out useCompatibilityCode);
                reader.Read(out overlapTransferQty);
                //reader.Read(out setupCode);
                reader.Read(out setupNumber);
                reader.Read(out wholeNumberSplits);
                reader.Read(out deductScrapFromRequired);
                reader.Read(out int val);
                successorProcessing = (OperationDefs.successorProcessingEnumeration)val;
                reader.Read(out val);
                splitUpdateMode = (InternalOperationDefs.splitUpdateModes)val;
                reader.Read(out keepSuccessorTimeLimit);

                reader.Read(out cycleSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out cleanSpan);
                reader.Read(out cleanOutGrade);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_endOfStoragePostProcessingTimeSpan);

                reader.Read(out setupColor);
                reader.Read(out setupSpan);

                reader.Read(out m_batchCode);

                reader.Read(out standardRunSpan);
                reader.Read(out standardSetupSpan);

                reader.Read(out string primaryResourceRequirementExternalId);

                reader.Read(out int resReqtsCount);
                for (int i = 0; i < resReqtsCount; i++)
                {
                    ResourceRequirement rr = new(reader);
                    if (rr.ExternalId == "")
                    {
                        rr.ExternalId = i.ToString(); //this is a fix up because we let blanks slip in before and this allows those recordings to continue to function
                    }

                    AddResourceRequirement(rr);
                    if (rr.ExternalId == primaryResourceRequirementExternalId)
                    {
                        primaryResourceRequirement = rr;
                    }
                }

                reader.Read(out int intActivityCount);
                for (int i = 0; i < intActivityCount; i++)
                {
                    InternalActivity activity = new(reader);
                    AddInternalActivity(activity);
                }

                _productionInfoFlags = new ProductionInfoFlags(reader);
                reader.Read(out short typeVal);
                m_type = (OperationDefs.EAutoSplitType)typeVal;
                reader.Read(out m_minAutoSplitAmount);
                reader.Read(out m_maxAutoSplitAmount);
                reader.Read(out typeVal);
                m_setupSplitType = (OperationDefs.ESetupSplitType)typeVal;
                reader.Read(out m_campaignCode);
            }
            else if (reader.VersionNumber >= 12502)
            {
                m_bools = new BoolVector32(reader);

                reader.Read(out canPause);
                reader.Read(out timeBasedReporting);
                reader.Read(out canSubcontract);
                reader.Read(out carryingCost);
                reader.Read(out compatibilityCode);
                reader.Read(out useCompatibilityCode);
                reader.Read(out overlapTransferQty);
                reader.Read(out setupNumber);
                reader.Read(out wholeNumberSplits);
                reader.Read(out deductScrapFromRequired);
                reader.Read(out int val);
                successorProcessing = (OperationDefs.successorProcessingEnumeration)val;
                reader.Read(out val);
                splitUpdateMode = (InternalOperationDefs.splitUpdateModes)val;
                reader.Read(out keepSuccessorTimeLimit);

                reader.Read(out cycleSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out cleanSpan);
                reader.Read(out cleanOutGrade);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_endOfStoragePostProcessingTimeSpan);

                reader.Read(out setupColor);
                reader.Read(out setupSpan);

                reader.Read(out m_batchCode);

                reader.Read(out standardRunSpan);
                reader.Read(out standardSetupSpan);

                reader.Read(out string primaryResourceRequirementExternalId);

                reader.Read(out int resReqtsCount);
                for (int i = 0; i < resReqtsCount; i++)
                {
                    ResourceRequirement rr = new(reader);
                    if (rr.ExternalId == "")
                    {
                        rr.ExternalId = i.ToString(); //this is a fix up because we let blanks slip in before and this allows those recordings to continue to function
                    }

                    AddResourceRequirement(rr);
                    if (rr.ExternalId == primaryResourceRequirementExternalId)
                    {
                        primaryResourceRequirement = rr;
                    }
                }

                reader.Read(out int intActivityCount);
                for (int i = 0; i < intActivityCount; i++)
                {
                    InternalActivity activity = new(reader);
                    AddInternalActivity(activity);
                }

                _productionInfoFlags = new ProductionInfoFlags(reader);
                reader.Read(out short typeVal);
                m_type = (OperationDefs.EAutoSplitType)typeVal;
                reader.Read(out m_minAutoSplitAmount);
                reader.Read(out m_maxAutoSplitAmount);
                reader.Read(out typeVal);
                m_setupSplitType = (OperationDefs.ESetupSplitType)typeVal;
            }
            else if (reader.VersionNumber >= 12319)
            {
                m_bools = new BoolVector32(reader);

                reader.Read(out canPause);
                reader.Read(out timeBasedReporting);
                reader.Read(out canSubcontract);
                reader.Read(out carryingCost);
                reader.Read(out compatibilityCode);
                reader.Read(out useCompatibilityCode);
                reader.Read(out overlapTransferQty);
                reader.Read(out string setupCode);
                reader.Read(out setupNumber);
                reader.Read(out wholeNumberSplits);
                reader.Read(out deductScrapFromRequired);
                reader.Read(out int val);
                successorProcessing = (OperationDefs.successorProcessingEnumeration)val;
                reader.Read(out val);
                splitUpdateMode = (InternalOperationDefs.splitUpdateModes)val;
                reader.Read(out keepSuccessorTimeLimit);

                reader.Read(out cycleSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out cleanSpan);
                reader.Read(out cleanOutGrade);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_endOfStoragePostProcessingTimeSpan);

                reader.Read(out setupColor);
                reader.Read(out setupSpan);

                reader.Read(out m_batchCode);

                reader.Read(out standardRunSpan);
                reader.Read(out standardSetupSpan);

                reader.Read(out string primaryResourceRequirementExternalId);

                reader.Read(out int resReqtsCount);
                for (int i = 0; i < resReqtsCount; i++)
                {
                    ResourceRequirement rr = new(reader);
                    if (rr.ExternalId == "")
                    {
                        rr.ExternalId = i.ToString(); //this is a fix up because we let blanks slip in before and this allows those recordings to continue to function
                    }

                    AddResourceRequirement(rr);
                    if (rr.ExternalId == primaryResourceRequirementExternalId)
                    {
                        primaryResourceRequirement = rr;
                    }
                }

                reader.Read(out int intActivityCount);
                for (int i = 0; i < intActivityCount; i++)
                {
                    InternalActivity activity = new(reader);
                    AddInternalActivity(activity);
                }

                _productionInfoFlags = new ProductionInfoFlags(reader);
                reader.Read(out short typeVal);
                m_type = (OperationDefs.EAutoSplitType)typeVal;
                reader.Read(out m_minAutoSplitAmount);
                reader.Read(out m_maxAutoSplitAmount);
                reader.Read(out typeVal);
                m_setupSplitType = (OperationDefs.ESetupSplitType)typeVal;
            }
            else if (reader.VersionNumber >= 12306)
            {
                m_bools = new BoolVector32(reader);

                reader.Read(out canPause);
                reader.Read(out timeBasedReporting);
                reader.Read(out canSubcontract);
                reader.Read(out carryingCost);
                reader.Read(out compatibilityCode);
                reader.Read(out useCompatibilityCode);
                reader.Read(out overlapTransferQty);
                reader.Read(out string setupCode);
                reader.Read(out setupNumber);
                reader.Read(out wholeNumberSplits);
                reader.Read(out deductScrapFromRequired);
                reader.Read(out int val);
                successorProcessing = (OperationDefs.successorProcessingEnumeration)val;
                reader.Read(out val);
                splitUpdateMode = (InternalOperationDefs.splitUpdateModes)val;
                reader.Read(out keepSuccessorTimeLimit);

                reader.Read(out cycleSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out cleanSpan);
                reader.Read(out cleanOutGrade);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_endOfStoragePostProcessingTimeSpan);

                reader.Read(out setupColor);
                reader.Read(out setupSpan);

                reader.Read(out m_batchCode);

                reader.Read(out standardRunSpan);
                reader.Read(out standardSetupSpan);

                reader.Read(out string primaryResourceRequirementExternalId);

                reader.Read(out int resReqtsCount);
                for (int i = 0; i < resReqtsCount; i++)
                {
                    ResourceRequirement rr = new (reader);
                    if (rr.ExternalId == "")
                    {
                        rr.ExternalId = i.ToString(); //this is a fix up because we let blanks slip in before and this allows those recordings to continue to function
                    }

                    AddResourceRequirement(rr);
                    if (rr.ExternalId == primaryResourceRequirementExternalId)
                    {
                        primaryResourceRequirement = rr;
                    }
                }

                reader.Read(out int intActivityCount);
                for (int i = 0; i < intActivityCount; i++)
                {
                    InternalActivity activity = new (reader);
                    AddInternalActivity(activity);
                }

                _productionInfoFlags = new ProductionInfoFlags(reader);
                reader.Read(out short typeVal);
                m_type = (OperationDefs.EAutoSplitType)typeVal;
                reader.Read(out m_minAutoSplitAmount);
                reader.Read(out m_maxAutoSplitAmount);
            }
            else if (reader.VersionNumber >= 12303)
            {
                reader.Read(out canPause);
                reader.Read(out timeBasedReporting);
                reader.Read(out canSubcontract);
                reader.Read(out carryingCost);
                reader.Read(out compatibilityCode);
                reader.Read(out useCompatibilityCode);
                reader.Read(out overlapTransferQty);
                reader.Read(out decimal minAutoSplitQty);
                reader.Read(out string setupCode);
                reader.Read(out setupNumber);
                reader.Read(out wholeNumberSplits);
                reader.Read(out deductScrapFromRequired);
                reader.Read(out int val);
                successorProcessing = (OperationDefs.successorProcessingEnumeration)val;
                reader.Read(out val);
                splitUpdateMode = (InternalOperationDefs.splitUpdateModes)val;
                reader.Read(out keepSuccessorTimeLimit);
                m_bools = new BoolVector32(reader);

                reader.Read(out cycleSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out cleanSpan);
                reader.Read(out cleanOutGrade);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_endOfStoragePostProcessingTimeSpan);

                reader.Read(out setupColor);
                reader.Read(out setupSpan);

                reader.Read(out m_batchCode);

                reader.Read(out standardRunSpan);
                reader.Read(out standardSetupSpan);

                reader.Read(out string primaryResourceRequirementExternalId);

                reader.Read(out int resReqtsCount);
                for (int i = 0; i < resReqtsCount; i++)
                {
                    ResourceRequirement rr = new (reader);
                    if (rr.ExternalId == "")
                    {
                        rr.ExternalId = i.ToString(); //this is a fix up because we let blanks slip in before and this allows those recordings to continue to function
                    }

                    AddResourceRequirement(rr);
                    if (rr.ExternalId == primaryResourceRequirementExternalId)
                    {
                        primaryResourceRequirement = rr;
                    }
                }

                reader.Read(out int intActivityCount);
                for (int i = 0; i < intActivityCount; i++)
                {
                    InternalActivity activity = new (reader);
                    AddInternalActivity(activity);
                }

                _productionInfoFlags = new ProductionInfoFlags(reader);
            }
            else if (reader.VersionNumber >= 12302)
            {
                reader.Read(out canPause);
                reader.Read(out timeBasedReporting);
                reader.Read(out canSubcontract);
                reader.Read(out carryingCost);
                reader.Read(out compatibilityCode);
                reader.Read(out useCompatibilityCode);
                reader.Read(out overlapTransferQty);
                reader.Read(out decimal minAutoSplitQty);
                reader.Read(out string setupCode);
                reader.Read(out setupNumber);
                reader.Read(out wholeNumberSplits);
                reader.Read(out deductScrapFromRequired);
                reader.Read(out int val);
                successorProcessing = (OperationDefs.successorProcessingEnumeration)val;
                reader.Read(out val);
                splitUpdateMode = (InternalOperationDefs.splitUpdateModes)val;
                reader.Read(out keepSuccessorTimeLimit);
                m_bools = new BoolVector32(reader);

                reader.Read(out cycleSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out cleanSpan);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_endOfStoragePostProcessingTimeSpan);

                reader.Read(out setupColor);
                reader.Read(out setupSpan);

                reader.Read(out m_batchCode);

                reader.Read(out standardRunSpan);
                reader.Read(out standardSetupSpan);

                reader.Read(out string primaryResourceRequirementExternalId);

                reader.Read(out int resReqtsCount);
                for (int i = 0; i < resReqtsCount; i++)
                {
                    ResourceRequirement rr = new (reader);
                    if (rr.ExternalId == "")
                    {
                        rr.ExternalId = i.ToString(); //this is a fix up because we let blanks slip in before and this allows those recordings to continue to function
                    }

                    AddResourceRequirement(rr);
                    if (rr.ExternalId == primaryResourceRequirementExternalId)
                    {
                        primaryResourceRequirement = rr;
                    }
                }

                reader.Read(out int intActivityCount);
                for (int i = 0; i < intActivityCount; i++)
                {
                    InternalActivity activity = new (reader);
                    AddInternalActivity(activity);
                }

                _productionInfoFlags = new ProductionInfoFlags(reader);
            }
            else if (reader.VersionNumber >= 715)
            {
                reader.Read(out canPause);
                reader.Read(out timeBasedReporting);
                reader.Read(out canSubcontract);
                reader.Read(out carryingCost);
                reader.Read(out compatibilityCode);
                reader.Read(out useCompatibilityCode);
                reader.Read(out overlapTransferQty);
                reader.Read(out decimal minAutoSplitQty);
                reader.Read(out string setupCode);
                reader.Read(out setupNumber);
                reader.Read(out wholeNumberSplits);
                reader.Read(out deductScrapFromRequired);
                reader.Read(out int val);
                successorProcessing = (OperationDefs.successorProcessingEnumeration)val;
                reader.Read(out val);
                splitUpdateMode = (InternalOperationDefs.splitUpdateModes)val;
                reader.Read(out keepSuccessorTimeLimit);
                m_bools = new BoolVector32(reader);

                reader.Read(out cycleSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out materialPostProcessingSpan);
                reader.Read(out m_endOfStoragePostProcessingTimeSpan);

                reader.Read(out setupColor);
                reader.Read(out setupSpan);

                reader.Read(out m_batchCode);

                reader.Read(out standardRunSpan);
                reader.Read(out standardSetupSpan);

                reader.Read(out string primaryResourceRequirementExternalId);

                reader.Read(out int resReqtsCount);
                for (int i = 0; i < resReqtsCount; i++)
                {
                    ResourceRequirement rr = new (reader);
                    if (rr.ExternalId == "")
                    {
                        rr.ExternalId = i.ToString(); //this is a fix up because we let blanks slip in before and this allows those recordings to continue to function
                    }

                    AddResourceRequirement(rr);
                    if (rr.ExternalId == primaryResourceRequirementExternalId)
                    {
                        primaryResourceRequirement = rr;
                    }
                }

                reader.Read(out int intActivityCount);
                for (int i = 0; i < intActivityCount; i++)
                {
                    InternalActivity activity = new (reader);
                    AddInternalActivity(activity);
                }

                _productionInfoFlags = new ProductionInfoFlags(reader);
            }
        }

        public override void Serialize(IWriter writer)
        {
            base.Serialize(writer);
            m_bools.Serialize(writer);
            m_isSetBools.Serialize(writer);

            writer.Write(canPause);
            writer.Write(timeBasedReporting);
            writer.Write(canSubcontract);
            writer.Write(carryingCost);
            writer.Write(compatibilityCode);
            writer.Write(useCompatibilityCode);
            writer.Write(overlapTransferQty);
            writer.Write(setupNumber);
            writer.Write(wholeNumberSplits);
            writer.Write(deductScrapFromRequired);
            writer.Write((int)successorProcessing);
            writer.Write((int)splitUpdateMode);
            writer.Write(keepSuccessorTimeLimit);

            writer.Write(cycleSpan);
            writer.Write(postProcessingSpan);
            writer.Write(cleanSpan);
            writer.Write(cleanOutGrade);
            writer.Write(m_cleanoutCost);
            writer.Write(materialPostProcessingSpan);
            writer.Write(m_endOfStoragePostProcessingTimeSpan);

            writer.Write(setupColor);
            writer.Write(setupSpan);
            writer.Write(m_productionSetupCost);

            writer.Write(m_batchCode);

            writer.Write(standardRunSpan);
            writer.Write(standardSetupSpan);

            string primaryResourceRequirementExternalId = "";
            if (primaryResourceRequirement != null)
            {
                primaryResourceRequirementExternalId = primaryResourceRequirement.ExternalId;
            }

            writer.Write(primaryResourceRequirementExternalId);

            writer.Write(ResourceRequirementCount);
            for (int i = 0; i < ResourceRequirementCount; i++)
            {
                GetResourceRequirement(i).Serialize(writer);
            }

            writer.Write(m_activities.Count);
            for (int i = 0; i < m_activities.Count; i++)
            {
                GetInternalActivity(i).Serialize(writer);
            }

            _productionInfoFlags.Serialize(writer);

            writer.Write((short)m_type);
            writer.Write(m_minAutoSplitAmount);
            writer.Write(m_maxAutoSplitAmount);
            writer.Write((short)m_setupSplitType);
            writer.Write(m_campaignCode);
            writer.Write(m_sequenceHeadStartDays);
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        #region BoolVector32
        private BoolVector32 m_bools;
        private const int c_autoSplitIdx = 0;
        private const int c_canResizeIdx = 1;
        private const int c_keepSplitsOnSameResourceIdx = 2;

        private BoolVector32 m_isSetBools;
        private const short c_keepSplitsOnSameResourceIsSetIdx = 0;
        #endregion

        protected InternalOperation(string externalId, string name, decimal requiredFinishQty, TimeSpan cycleSpan)
            : base(externalId, name, requiredFinishQty)
        {
            CycleSpan = cycleSpan;
            ValidateCycleSpan();
        }

        protected InternalOperation(string externalId, string name, decimal requiredFinishQty, TimeSpan cycleSpan, string description, string notes, string userFields)
            : base(externalId, name, requiredFinishQty, description, notes, userFields)
        {
            CycleSpan = cycleSpan;
            ValidateCycleSpan();
        }

        #region Shared Properties
        /// <summary>
        /// RESERVED FOR FUTURE USE If the operation is scheduled after its JITStartDateTime then we split the operation evenly into n activities where n is the number of eligible Resources.  Resulting split
        /// Activities are scheduled independent of each other and may schedule on the same or different Resources.  This also controls unsplitting. During Optimizations, if an Activity is found to have been
        /// split unnecessarily then it will be unsplit. This applies both to automatically and manually split activities.
        /// </summary>
        public bool AutoSplit
        {
            get => m_bools[c_autoSplitIdx];

            set => m_bools[c_autoSplitIdx] = value;
        }

        private bool canPause = true;

        /// <summary>
        /// If true then Activities can be 'paused' for offline calendar intervals. If false, Activities won't start until there is sufficient time to complete them in one continuous interval.
        /// </summary>
        public bool CanPause
        {
            get => canPause;
            set => canPause = value;
        }

        private bool timeBasedReporting;

        /// <summary>
        /// Specifies whether Reported Run Span is subtracted from Scheduled Run Span rather than calculating Scheduled Run Span based on the Remaining Quantity.
        /// </summary>
        public bool TimeBasedReporting
        {
            get => timeBasedReporting;
            set => timeBasedReporting = value;
        }

        private bool canSubcontract;

        /// <summary>
        /// Indicates to the planner that this work can be subcontracted if necessary.  This can also be used for automatic suggestions of work to subcontract.
        /// </summary>
        public bool CanSubcontract
        {
            get => canSubcontract;
            set => canSubcontract = value;
        }

        private decimal carryingCost;

        /// <summary>
        /// Cost per unit per day after this Operation is finished.  This is for costs related to shrinkage, spoilage, raw materials, engineering changes and customer changes that may cause materials to be
        /// scrapped.
        /// </summary>
        public decimal CarryingCost
        {
            get => carryingCost;
            set => carryingCost = value;
        }

        private string compatibilityCode = "";

        /// <summary>
        /// RESERVED FOR FUTURE USE Used to restrict Resources to only perform compatible work at simulataneous times. The Activity's CompatibilityCode must match the CompatibilityCode of other Activities
        /// scheduled on Resources with the same CompatibilityGroup.
        /// </summary>
        public string CompatibilityCode
        {
            get => compatibilityCode;
            set => compatibilityCode = value;
        }

        private bool useCompatibilityCode;

        /// <summary>
        /// Whether to use the compatibility code.
        /// </summary>
        public bool UseCompatibilityCode
        {
            get => useCompatibilityCode;

            set => useCompatibilityCode = value;
        }

        private TimeSpan cycleSpan = new (0);

        /// <summary>
        /// Time to perform one production cycle.
        /// </summary>
        [Required(true)]
        public TimeSpan CycleSpan
        {
            get => cycleSpan;
            set => cycleSpan = value;
        }

        private TimeSpan standardRunSpan;

        /// <summary>
        /// Used to calculate performane by comparing against actual values.  This has no effect on scheduling.
        /// </summary>
        public TimeSpan StandardRunSpan
        {
            set => standardRunSpan = value;
            get => standardRunSpan;
        }

        private TimeSpan standardSetupSpan;

        /// <summary>
        /// Used to calculate performane by comparing against actual values.  This has no effect on scheduling.
        /// </summary>
        public TimeSpan StandardSetupSpan
        {
            set => standardSetupSpan = value;
            get => standardSetupSpan;
        }

        private decimal overlapTransferQty = 1;

        /// <summary>
        /// Specifies the number of parts moved to the next operation in each transfer batch.  Smaller values can be used to shorten flow times by making material available at successor operations more quickly.
        /// </summary>
        public decimal OverlapTransferQty
        {
            get => overlapTransferQty;
            set => overlapTransferQty = value;
        }

        private TimeSpan postProcessingSpan = new (0);

        /// <summary>
        /// Specifies a TimeSpan for which product must wait before they are considered complete at the operation.  The resources will be occupied.
        /// </summary>
        public TimeSpan PostProcessingSpan
        {
            get => postProcessingSpan;
            set => postProcessingSpan = value;
        }

        private TimeSpan cleanSpan = new (0);

        /// <summary>
        /// Specifies a TimeSpan for which the resource will be cleaned out after this operation.  The resources will be occupied during this period.
        /// </summary>
        public TimeSpan CleanSpan
        {
            get => cleanSpan;
            set => cleanSpan = value;
        }

        private int cleanOutGrade;

        public int CleanOutGrade
        {
            get => cleanOutGrade;
            set => cleanOutGrade = value;
        }

        private decimal m_cleanoutCost;
        public decimal CleanoutCost
        {
            get => m_cleanoutCost;
            internal set => m_cleanoutCost = value;
        }

        private TimeSpan materialPostProcessingSpan = new (0);

        /// <summary>
        /// Specifies the amount of time after processing that material must wait before it can be used. For instance this may represent drying time, or cooling time.
        /// </summary>
        public TimeSpan MaterialPostProcessingSpan
        {
            get => materialPostProcessingSpan;
            set => materialPostProcessingSpan = value;
        }

        private TimeSpan m_endOfStoragePostProcessingTimeSpan = new (0);

        public TimeSpan StorageTimeSpan
        {
            get => m_endOfStoragePostProcessingTimeSpan;
            set => m_endOfStoragePostProcessingTimeSpan = value;
        }


        private decimal setupNumber;
        /// <summary>
        /// Can be used to sequence Operations so as to try to changeover to the most similar product or in a gradually increasing or decreasing sequence.
        /// </summary>
        public decimal SetupNumber
        {
            get => setupNumber;
            set => setupNumber = value;
        }
        

        private string m_campaignCode = string.Empty;
        public string CampaignCode
        {
            get => m_campaignCode;
            set => m_campaignCode = value;
        }

        private Color setupColor = Color.White;

        /// <summary>
        /// Can be used to visually indicate whether Operations require special setup when switching from other Operations.
        /// When importing, this can be set to a Known Color like 'Red' or the RGB values can be specified.
        /// The Windows Known Colors are: AliceBlue, AntiqueWhite, Aqua, Aquamarine, Azure, Beige, Bisque, Black, BlanchedAlmond, Blue, BlueViolet, Brown, BurlyWood, CadetBlue, Chartreuse, Chocolate, Coral
        /// , CornflowerBlue, Cornsilk, Crimson, Cyan, DarkBlue, DarkCyan, DarkGoldenrod, DarkGray, DarkGreen, DarkKhaki, DarkMagenta, DarkOliveGreen, DarkOrange, DarkOrchid, DarkRed, DarkSalmon, DarkSeaGreen
        /// , DarkSlateBlue, DarkSlateGray, DarkTurquoise, DarkViolet, DeepPink, DeepSkyBlue, DimGray, DodgerBlue, Firebrick, FloralWhite, ForestGreen, Fuchsia, Gainsboro, GhostWhite, Gold, Goldenrod, Gray
        /// , Green, GreenYellow, Honeydew, HotPink, IndianRed, Indigo, Ivory, Khaki, Lavender, LavenderBlush, LawnGreen, LemonChiffon, LightBlue, LightCoral, LightCyan, LightGoldenrodYellow, LightGray,
        /// LightGreen
        /// , LightPink, LightSalmon, LightSeaGreen, LightSkyBlue, LightSlateGray, LightSteelBlue, LightYellow, Lime, LimeGreen, Linen, Magenta, Maroon, MediumAquamarine, MediumBlue, MediumOrchid, MediumPurple,
        /// MediumSeaGreen
        /// , MediumSlateBlue, MediumSpringGreen, MediumTurquoise, MediumVioletRed, MidnightBlue, MintCream, MistyRose, Moccasin, NavajoWhite, Navy, OldLace, Olive, OliveDrab, Orange, OrangeRed, Orchid,
        /// PaleGoldenrod, PaleGreen, PaleTurquoise
        /// , PaleVioletRed, PapayaWhip, PeachPuff, Peru, Pink, Plum, PowderBlue, Purple, Red, RosyBrown, RoyalBlue, SaddleBrown, Salmon, SandyBrown, SeaGreen, SeaShell, Sienna, Silver, SkyBlue, SlateBlue,
        /// SlateGray
        /// , Snow, SpringGreen, SteelBlue, Tan, Teal, Thistle, Tomato, Transparent, Turquoise, Violet, Wheat, White, WhiteSmoke, Yellow, YellowGreen
        /// </summary>
        public Color SetupColor
        {
            get => setupColor;
            set => setupColor = value;
        }

        private TimeSpan setupSpan = new (0);

        /// <summary>
        /// Time for setting up each Resource that is used during the Operation's setup time.
        /// </summary>
        public TimeSpan SetupSpan
        {
            get => setupSpan;
            set => setupSpan = value;
        }

        private decimal m_productionSetupCost;
        public decimal ProductionSetupCost
        {
            get => m_productionSetupCost;
            internal set => m_productionSetupCost = value;
        }

        private bool wholeNumberSplits;

        /// <summary>
        /// Whether Activities must be split into quantites with whole numbers.  (Only possible if the original quantity is a whole number.)
        /// </summary>
        public bool WholeNumberSplits
        {
            get => wholeNumberSplits;
            set => wholeNumberSplits = value;
        }

        private bool deductScrapFromRequired = true;

        /// <summary>
        /// Whether Reported Scrap should be deducted from the Required Finish Qty when determining the quantity to schedule.
        /// In some cases, more material is available and additional product can be produced to make up for the scrap.  In other cases this is not possible and the operation will finish short.
        /// </summary>
        public bool DeductScrapFromRequired
        {
            get => deductScrapFromRequired;

            set => deductScrapFromRequired = value;
        }

        private OperationDefs.successorProcessingEnumeration successorProcessing = OperationDefs.successorProcessingEnumeration.NoPreference;

        /// <summary>
        /// Allows control over whether the successor operation must be scheduled on the same resource as this one. Use of this feature requires that the routing be linear within the predecessor and successor
        /// operation, that the operation only has 1 activity, and that the resource the predecessor ends up being scheduled on is eligible to perform the work required on the successor operation. KeepSuccessor
        /// means the successor operation will try to end up being scheduled on the same resource as the predecessor operation KeepSuccessorNoInterrupt means that not only will the successor operation try to end
        /// up on the same resource but it will also try to end up as the very next operation scheduled on the resource.
        /// </summary>
        public OperationDefs.successorProcessingEnumeration SuccessorProcessing
        {
            get => successorProcessing;

            set => successorProcessing = value;
        }

        private long keepSuccessorTimeLimit = TimeSpan.TicksPerDay * 7;

        /// <summary>
        /// The length of time the SuccessorProcessing setting remains valid for. After this length of time has passed without the successor operation finding a spot on the resource's schedule after its
        /// predecessor it may be scheduled on some other resource.
        /// </summary>
        public TimeSpan KeepSuccessorsTimeLimit
        {
            get => new (keepSuccessorTimeLimit);

            set => keepSuccessorTimeLimit = value.Ticks;
        }

        private string m_batchCode;

        public string BatchCode
        {
            get => m_batchCode;
            set => m_batchCode = value;
        }

        private readonly ProductionInfoFlags _productionInfoFlags = new ();

        public ProductionInfoFlags ProductionInfoManualUpdateOnlyFlags => _productionInfoFlags;

        public bool CanResize
        {
            get => m_bools[c_canResizeIdx];
            set => m_bools[c_canResizeIdx] = value;
        }

        private OperationDefs.EAutoSplitType m_type;

        public OperationDefs.EAutoSplitType SplitType
        {
            get => m_type;
            set => m_type = value;
        }

        private decimal m_minAutoSplitAmount;

        /// <summary>
        /// This is a minimum quantity for any resulting Activity of a split.
        /// </summary>
        public decimal MinAutoSplitAmount
        {
            get => m_minAutoSplitAmount;
            set => m_minAutoSplitAmount = value;
        }

        private decimal m_maxAutoSplitAmount;

        /// <summary>
        /// This is a maximum quantity for any resulting Activity of a split.
        /// </summary>
        public decimal MaxAutoSplitAmount
        {
            get => m_maxAutoSplitAmount;
            set => m_maxAutoSplitAmount = value;
        }

        public bool KeepSplitsOnSameResource
        {
            get => m_bools[c_keepSplitsOnSameResourceIdx];
            set
            {
                m_bools[c_keepSplitsOnSameResourceIdx] = value;
                m_isSetBools[c_keepSplitsOnSameResourceIsSetIdx] = true;
            }
        }

        public bool KeepSplitsOnSameResourceIsSet => m_isSetBools[c_keepSplitsOnSameResourceIsSetIdx];

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
            set
            {
                if (value < 0)
                {
                    throw new PTValidationException("SequenceHeadStartDays value was invalid, must be greater than 0.");
                }

                m_sequenceHeadStartDays = value;
            }
        }

        #endregion Shared Properties

        #region Non-shared Properties
        private InternalOperationDefs.splitUpdateModes splitUpdateMode = InternalOperationDefs.splitUpdateModes.UpdateSplitsIndividually;

        /// <summary>
        /// Specifies whether and how updates affect Activities that were split from the same Operation.
        /// Shares Reported quantities and times.  Shared Chronologically allocates time and qty to the earliest Activity fully before allocating to the others.  Shared evenly splits quantities and time evenly
        /// across split Activities.
        /// Note that this value is only used during import to allocate completion data and so this value is not stored in the Operation.
        /// </summary>
        public InternalOperationDefs.splitUpdateModes SplitUpdateMode
        {
            get => splitUpdateMode;
            set => splitUpdateMode = value;
        }
        #endregion

        #region Resource Requirements
        private ResourceRequirement primaryResourceRequirement;

        [Browsable(false)]
        public ResourceRequirement PrimaryResourceRequirement
        {
            get => primaryResourceRequirement;

            set
            {
                if (!m_resourceRequirements.Contains(value))
                {
                    throw new ValidationException("2064");
                }

                primaryResourceRequirement = value;
            }
        }

        private readonly List<ResourceRequirement> m_resourceRequirements = new ();

        public int ResourceRequirementCount => m_resourceRequirements.Count;

        private readonly Hashtable resReqtsHash = new ();

        public void AddResourceRequirement(ResourceRequirement requirement)
        {
            //make sure the external id is unique
            if (!resReqtsHash.Contains(requirement.ExternalId))
            {
                resReqtsHash.Add(requirement.ExternalId, requirement);
                m_resourceRequirements.Add(requirement);
            }
            else
            {
                throw new APSCommon.PTValidationException("2065", new object[] { ExternalId, requirement.ExternalId });
            }

            //Set it as primary if it's the only one to save from having to remember.
            if (m_resourceRequirements.Count == 1)
            {
                SetPrimaryResourceRequirement(requirement.ExternalId);
            }
        }

        public ResourceRequirement GetResourceRequirement(int index)
        {
            return m_resourceRequirements[index];
        }

        public void SetPrimaryResourceRequirement(string resReqtExternalId)
        {
            if (ResourceRequirementCount < 1)
            {
                throw new ValidationException("2066", new object[] { ExternalId });
            }

            if (resReqtExternalId != "-1" && resReqtExternalId != "") //If they specified a specific RR then set it.  Otherwise we'll use the first one.
            {
                for (int i = 0; i < m_resourceRequirements.Count; i++)
                {
                    ResourceRequirement reqt = GetResourceRequirement(i);
                    if (reqt.ExternalId == resReqtExternalId)
                    {
                        PrimaryResourceRequirement = reqt;
                        return;
                    }
                }

                throw new ValidationException("2067", new object[] { resReqtExternalId, ExternalId });
            }

            PrimaryResourceRequirement = GetResourceRequirement(0);
        }
        #endregion ResourceRequirements

        private void ValidateCycleSpan()
        {
            if (cycleSpan.Ticks <= 0)
            {
                throw new ValidationException("2068", new object[] { ExternalId, Name, Description });
            }

            if (cycleSpan.TotalDays > 365)
            {
                throw new ValidationException("2069", new object[] { ExternalId, Name, Description, cycleSpan.TotalDays });
            }
        }

        private void ValidateSetupSpan()
        {
            if (SetupSpan.Ticks < 0)
            {
                throw new ValidationException("2070", new object[] { ExternalId, Name, Description });
            }

            if (SetupSpan.TotalDays > 365)
            {
                throw new ValidationException("2071", new object[] { ExternalId, Name, Description, SetupSpan.TotalDays });
            }
        }

        private void ValidatePostProcessingSpan()
        {
            if (PostProcessingSpan.Ticks < 0)
            {
                throw new ValidationException("2072", new object[] { ExternalId, Name, Description });
            }

            if (PostProcessingSpan.TotalDays > 365)
            {
                throw new ValidationException("2073", new object[] { ExternalId, Name, Description, PostProcessingSpan.TotalDays });
            }
        }

        private void ValidateCleanSpan()
        {
            if (cleanSpan.Ticks < 0)
            {
                throw new ValidationException("3048", new object[] { ExternalId, Name, Description });
            }

            if (cleanSpan.TotalDays > 365)
            {
                throw new ValidationException("3049", new object[] { ExternalId, Name, Description, cleanSpan.TotalDays });
            }
        }

        private void ValidateMaterialPostProcessingSpan()
        {
            if (MaterialPostProcessingSpan.Ticks < 0)
            {
                throw new ValidationException("2074", new object[] { ExternalId, Name, Description });
            }

            if (MaterialPostProcessingSpan.TotalDays > 365)
            {
                throw new ValidationException("2075", new object[] { ExternalId, Name, Description, MaterialPostProcessingSpan.TotalDays });
            }
        }

        /// <summary>
        /// 1. There must be at least one RR
        /// 2. There's at least one primary RR
        /// 3. Each RR must have at least one RequiredCapability
        /// 4. RRs' Usages should be valid.
        /// 5. ProductionStatus and Usage should be compatible
        /// 6. RR Tank validation
        /// </summary>
        public void ValidateResourceRequirements()
        {
            if (ResourceRequirementCount == 0)
            {
                throw new ValidationException("2076", new object[] { ExternalId, Name });
            }

            if (PrimaryResourceRequirement == null)
            {
                if (ResourceRequirementCount == 1)
                {
                    PrimaryResourceRequirement = GetResourceRequirement(0);
                }
                else
                {
                    throw new ValidationException("2078", new object[] { ExternalId, Name });
                }
            }

            int primaryReqIndex = 0;
            List<MainResourceDefs.Usage> usages = new ();
            try
            {
                for (int i = 0; i < ResourceRequirementCount; i++)
                {
                    ResourceRequirement rr = GetResourceRequirement(i);
                    if (rr.ExternalId == PrimaryResourceRequirement.ExternalId)
                    {
                        primaryReqIndex = i;
                    }

                    usages.Add(new MainResourceDefs.Usage(rr.UsageStart, rr.UsageEnd));
                }

                MainResourceDefs.Usage.ValidateUsages(usages, primaryReqIndex);
            }
            catch (MainResourceDefs.UsageValidationException usageErr)
            {
                throw new ValidationException("2871", new object[] { ExternalId, usageErr.Message });
            }

            if (!MainResourceDefs.Usage.IsProductionStatusValidForUsage(PrimaryResourceRequirement.UsageEnd, m_activities.Select(act => act.ProductionStatus)))
            {
                throw new ValidationException("2859", new object[] { ExternalId, PrimaryResourceRequirement.UsageEnd });
            }
        }

        private void ValidateRequiredFinishQtyAgainstActivities()
        {
            if (m_activities.Count > 0)
            {
                decimal activityTotalRequiredFinishQty = 0;
                for (int i = 0; i < m_activities.Count; ++i)
                {
                    InternalActivity ia = m_activities[i];
                    activityTotalRequiredFinishQty += ia.RequiredFinishQty;
                }

                if (!MathStatics.IsEqual(activityTotalRequiredFinishQty, RequiredFinishQty))
                {
                    StringBuilder sb = new ();

                    sb.AppendFormat(Localizer.GetErrorString("2079", new object[] { ExternalId, Name, RequiredFinishQty, activityTotalRequiredFinishQty }));
                    sb.AppendLine();

                    sb.AppendFormat(Localizer.GetErrorString("2080", new object[] { m_activities.Count }));
                    sb.AppendLine();

                    for (int i = 0; i < m_activities.Count; ++i)
                    {
                        InternalActivity ia = m_activities[i];

                        sb.AppendFormat(Localizer.GetErrorString("2081", new object[] { i + 1, ia.ExternalId, ia.RequiredFinishQty }));
                        sb.AppendLine();
                    }

                    throw new ValidationException(sb.ToString());
                }
            }
        }

        public override void Validate()
        {
            base.Validate();
            ValidateCycleSpan();
            ValidateSetupSpan();
            ValidatePostProcessingSpan();
            ValidateCleanSpan();
            ValidateMaterialPostProcessingSpan();
            ValidateResourceRequirements();
            ValidateRequiredFinishQtyAgainstActivities();

            for (int i = 0; i < m_activities.Count; i++)
            {
                GetInternalActivity(i).Validate();
            }
        }

        private readonly List<InternalActivity> m_activities = new ();

        public int InternalActivityCount => m_activities.Count;

        private readonly Hashtable activityHash = new ();

        public void AddInternalActivity(InternalActivity activity)
        {
            if (activityHash.Contains(activity.ExternalId))
            {
                throw new InternalActivity.DuplicateInternalActivityException("2732", new object[] { activity.ExternalId });
            }

            m_activities.Add(activity);
            activityHash.Add(activity.ExternalId, null);
        }

        public InternalActivity GetInternalActivity(int index)
        {
            return m_activities[index];
        }

        public class ResourceRequirement : PTObjectIdBase, IPTSerializable
        {
            public new const int UNIQUE_ID = 245;

            #region PT Serialization
            public ResourceRequirement(IReader reader)
                : base(reader)
            {
                if (reader.VersionNumber >= 12406)
                {
                    int val;
                    reader.Read(out val);
                    m_usageStart = (MainResourceDefs.usageEnum)val;
                    reader.Read(out val);
                    m_usageEnd = (MainResourceDefs.usageEnum)val;
                    reader.Read(out description);
                    reader.Read(out attentionPercent); //new
                    //Default Resource
                    reader.Read(out defaultResourceExternalId);
                    reader.Read(out defaultResourceDepartmentExternalId);
                    reader.Read(out defaultResourcePlantExternalId);
                    reader.Read(out blockFillImageFile);
                    reader.Read(out val);
                    blockFillPattern = val;
                    reader.Read(out val);
                    blockFillType = (ResourceRequirementDefs.blockFillTypes)val;

                    m_requiredCapabilities = new UniqueStringArrayList(reader);

                    reader.Read(out defaultResourceJITLimit);
                    reader.Read(out useDefaultResourceJITLimit);
                    reader.Read(out m_capacityCode);
                }
                else if (reader.VersionNumber >= 601)
                {
                    int val;
                    reader.Read(out val);
                    m_usageStart = (MainResourceDefs.usageEnum)val;
                    reader.Read(out val);
                    m_usageEnd = (MainResourceDefs.usageEnum)val;
                    reader.Read(out description);
                    reader.Read(out attentionPercent); //new
                    //Default Resource
                    reader.Read(out defaultResourceExternalId);
                    reader.Read(out defaultResourceDepartmentExternalId);
                    reader.Read(out defaultResourcePlantExternalId);
                    reader.Read(out blockFillImageFile);
                    reader.Read(out val);
                    blockFillPattern = val;
                    reader.Read(out val);
                    blockFillType = (ResourceRequirementDefs.blockFillTypes)val;

                    m_requiredCapabilities = new UniqueStringArrayList(reader);

                    reader.Read(out defaultResourceJITLimit);
                    reader.Read(out useDefaultResourceJITLimit);
                }
            }

            public override void Serialize(IWriter writer)
            {
                base.Serialize(writer);

                writer.Write((int)m_usageStart);
                writer.Write((int)m_usageEnd);
                writer.Write(description);
                writer.Write(attentionPercent);
                writer.Write(defaultResourceExternalId);
                writer.Write(defaultResourceDepartmentExternalId);
                writer.Write(defaultResourcePlantExternalId);
                writer.Write(blockFillImageFile);
                writer.Write((int)blockFillPattern);
                writer.Write((int)blockFillType);

                m_requiredCapabilities.Serialize(writer);

                writer.Write(defaultResourceJITLimit);
                writer.Write(useDefaultResourceJITLimit);
                writer.Write(m_capacityCode);
            }

            public override int UniqueId => UNIQUE_ID;
            #endregion

            public ResourceRequirement() { } // reqd. for xml serialization

            public ResourceRequirement(string externalId)
                : base(externalId) { }

            public ResourceRequirement(JobDataSet.ResourceRequirementRow row)
                : base(row.ExternalId)
            {
                if (!row.IsDescriptionNull())
                {
                    Description = row.Description;
                }

                if (!row.IsUsageStartNull())
                {
                    try
                    {
                        m_usageStart = (MainResourceDefs.usageEnum)Enum.Parse(typeof(MainResourceDefs.usageEnum), row.UsageStart);
                    }
                    catch (Exception err)
                    {
                        throw new APSCommon.PTValidationException("2854",
                            err,
                            false,
                            new object[]
                            {
                                row.UsageStart, "ResourceRequirement", "UsageStart",
                                string.Join(", ", Enum.GetNames(typeof(MainResourceDefs.usageEnum)))
                            });
                    }
                }

                if (!row.IsUsageEndNull())
                {
                    try
                    {
                        m_usageEnd = (MainResourceDefs.usageEnum)Enum.Parse(typeof(MainResourceDefs.usageEnum), row.UsageEnd);
                    }
                    catch (Exception err)
                    {
                        throw new APSCommon.PTValidationException("2854",
                            err,
                            false,
                            new object[]
                            {
                                row.UsageEnd, "ResourceRequirement", "UsageEnd",
                                string.Join(", ", Enum.GetNames(typeof(MainResourceDefs.usageEnum)))
                            });
                    }
                }

                if (!row.IsAttentionPercentNull())
                {
                    AttentionPercent = row.AttentionPercent;
                }

                if (!row.IsDefaultResourcePlantExternalIdNull())
                {
                    DefaultResourcePlantExternalId = row.DefaultResourcePlantExternalId;
                }

                if (!row.IsDefaultResourceDepartmentExternalIdNull())
                {
                    DefaultResourceDepartmentExternalId = row.DefaultResourceDepartmentExternalId;
                }

                if (!row.IsDefaultResourceExternalIdNull())
                {
                    DefaultResourceExternalId = row.DefaultResourceExternalId;
                }

                if (!row.IsDefaultResourceJITLimitHrsNull())
                {
                    DefaultResourceJITLimit = row.DefaultResourceJITLimitHrs == TimeSpan.MaxValue.TotalHours ? TimeSpan.MaxValue : TimeSpan.FromHours(row.DefaultResourceJITLimitHrs); //else overflows
                }

                if (!row.IsUseDefaultResourceJITLimitNull())
                {
                    UseDefaultResourceJITLimit = row.UseDefaultResourceJITLimit;
                }

                if (!row.IsBlockFillImageFileNull())
                {
                    blockFillImageFile = row.BlockFillImageFile;
                }

                if (!row.IsBlockFillPatternNull())
                {
                    try
                    {
                        blockFillPattern = (int)Enum.Parse(typeof(System.Drawing.Drawing2D.HatchStyle), row.BlockFillPattern);
                    }
                    catch (Exception err)
                    {
                        throw new APSCommon.PTValidationException("2854",
                            err,
                            false,
                            new object[]
                            {
                                row.BlockFillPattern, "ResourceRequirement", "BlockFillPattern",
                                string.Join(", ", Enum.GetNames(typeof(System.Drawing.Drawing2D.HatchStyle)))
                            });
                    }
                }

                if (!row.IsBlockFillTypeNull())
                {
                    try
                    {
                        blockFillType = (ResourceRequirementDefs.blockFillTypes)Enum.Parse(typeof(ResourceRequirementDefs.blockFillTypes), row.BlockFillType);
                    }
                    catch (Exception err)
                    {
                        throw new APSCommon.PTValidationException("2854",
                            err,
                            false,
                            new object[]
                            {
                                row.BlockFillType, "ResourceRequirement", "BlockFillType",
                                string.Join(", ", Enum.GetNames(typeof(ResourceRequirementDefs.blockFillTypes)))
                            });
                    }
                }

                if (!row.IsCapacityCodeNull())
                {
                    CapacityCode = row.CapacityCode;
                }
            }

            #region Shared properties
            public void SetUsage(MainResourceDefs.Usage a_usage)
            {
                UsageStart = a_usage.Start;
                UsageEnd = a_usage.End;
            }

            private MainResourceDefs.usageEnum m_usageStart = MainResourceDefs.usageEnum.Setup;

            public MainResourceDefs.usageEnum UsageStart
            {
                get => m_usageStart;
                set => m_usageStart = value;
            }

            private MainResourceDefs.usageEnum m_usageEnd = MainResourceDefs.usageEnum.PostProcessing;

            public MainResourceDefs.usageEnum UsageEnd
            {
                get => m_usageEnd;
                set => m_usageEnd = value;
            }

            private string description = "";

            /// <summary>
            /// Text that describes the purpose or source of the Resource Requirement.
            /// </summary>
            public string Description
            {
                get => description;
                set => description = value;
            }

            private int attentionPercent = 100;

            /// <summary>
            /// The percent of the Resource's attention consumed by this Resource Requirement.
            /// </summary>
            public int AttentionPercent
            {
                get => attentionPercent;
                set
                {
                    if (value <= 0 || value > 100)
                    {
                        throw new APSCommon.PTValidationException("2453");
                    }

                    attentionPercent = value;
                }
            }

            private ResourceRequirementDefs.blockFillTypes blockFillType;

            /// <summary>
            /// Specifies how the blocks in the Gantt should be filled.
            /// </summary>
            public ResourceRequirementDefs.blockFillTypes BlockFillType
            {
                get => blockFillType;
                set => blockFillType = value;
            }

            private int blockFillPattern;

            /// <summary>
            /// If BlockFillType is set to Pattern, then this pattern is used to fill blocks in the Gantt.
            /// This can be used to visually differentiate different types of operations or resources.
            /// </summary>
            public int BlockFillPattern
            {
                get => blockFillPattern;
                set => blockFillPattern = value;
            }

            private string blockFillImageFile;

            /// <summary>
            /// If BlockFillType is set to Image, then this image is used to fill blocks in the Gantt (or the Resource Image is used is this is left blank).
            /// This can be used to visually differentiate different types of operations or resources.
            /// The values specified is the full name of the file such as: myfile.png.
            /// These image files are in the ResourceImages folder under the client executable (with the images displayed in the Resource grid).
            /// </summary>
            public string BlockFillImageFile
            {
                get => blockFillImageFile;
                set => blockFillImageFile = value;
            }

            private string m_capacityCode;

            /// <summary>
            /// A capacity group code that can be constrained to run only on capacity intervals with the same code
            /// </summary>
            public string CapacityCode
            {
                get => m_capacityCode;
                private set => m_capacityCode = value;
            }
            #endregion

            #region Default Resource
            /// <summary>
            /// Specifies the Resource that will be used to satisfy the Resource Requirement whenever the Operation is scheduled.
            /// If the Operation is manually moved to a different Resource then it will be Locked to the new Resource automatically
            /// to preserve the change.  If unlocked, then the next Optimize will reschedule it to the Default Resource again.
            /// This Resource is always considered eligible for the Operation even if it doesn't posess the required Capabilities.
            /// This is optional and may be null, in which case the Capabilities are used to choose a Resource.
            /// </summary>
            private string defaultResourceExternalId;

            public string DefaultResourceExternalId
            {
                get => defaultResourceExternalId;
                set => defaultResourceExternalId = value;
            }

            private string defaultResourceDepartmentExternalId;

            /// <summary>
            /// The ExternalId of the Default Resource.  Required if Default ResourceExternalId is specified.
            /// </summary>
            public string DefaultResourceDepartmentExternalId
            {
                get => defaultResourceDepartmentExternalId;
                set => defaultResourceDepartmentExternalId = value;
            }

            private string defaultResourcePlantExternalId;

            /// <summary>
            /// The ExternalId of the Default Plant.  Required if Default ResourceExternalId is specified.
            /// </summary>
            public string DefaultResourcePlantExternalId
            {
                get => defaultResourcePlantExternalId;
                set => defaultResourcePlantExternalId = value;
            }

            private TimeSpan defaultResourceJITLimit;

            /// <summary>
            /// This can be used to limit the enforcement of the Default Resource within this amount of time from the Activity's JIT Start time.
            /// This will give flexibilty during Optimization to choose other Resources if the Default Resource is unable to start the Activity on time thus minimizing the chance of it being late.
            /// </summary>
            public TimeSpan DefaultResourceJITLimit
            {
                get => defaultResourceJITLimit;
                set => defaultResourceJITLimit = value;
            }

            private bool useDefaultResourceJITLimit;

            /// <summary>
            /// If true then the DefaultResourceJITLimit will be used.  If false then the Default Resource is enforced over all times regardless of whether it will make the Activity late.
            /// </summary>
            public bool UseDefaultResourceJITLimit
            {
                get => useDefaultResourceJITLimit;
                set => useDefaultResourceJITLimit = value;
            }
            #endregion

            private readonly UniqueStringArrayList m_requiredCapabilities = new ();

            public int RequiredCapabilityCount => m_requiredCapabilities.Count;

            public void AddRequiredCapability(string capabilityExternalId)
            {
                if (capabilityExternalId == null || capabilityExternalId.Trim() == "")
                {
                    throw new ValidationException("2082", new object[] { ExternalId });
                }

                try
                {
                    m_requiredCapabilities.Add(capabilityExternalId);
                }
                catch (UniqueStringArrayList.UniqueStringArrayListException)
                {
                    throw new ValidationException("2083", new object[] { capabilityExternalId, ExternalId });
                }
            }

            public string GetRequiredCapability(int i)
            {
                return m_requiredCapabilities[i];
            }

            public override void Validate()
            {
                base.Validate();
                if (RequiredCapabilityCount == 0)
                {
                    throw new ValidationException("2077", new object[] { ExternalId });
                }
            }
        }
    }
}
