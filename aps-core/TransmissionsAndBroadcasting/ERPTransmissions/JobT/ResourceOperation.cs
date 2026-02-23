using PT.APSCommon;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

public partial class JobT
{
    public class ResourceOperation : InternalOperation, IPTSerializable
    {
        public new const int UNIQUE_ID = 228;

        #region PT Serialization
        public ResourceOperation(IReader reader)
            : base(reader)
        {
            if (reader.VersionNumber >= 1)
            {
                reader.Read(out qtyPerCycle);
            }
        }

        public override void Serialize(IWriter writer)
        {
            base.Serialize(writer);

            writer.Write(qtyPerCycle);
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        public ResourceOperation() { } // reqd. for xml serialization

        public ResourceOperation(string externalId, string name, decimal requiredFinishQty, TimeSpan cycleSpan)
            :
            base(externalId, name, requiredFinishQty, cycleSpan) { }

        public ResourceOperation(JobDataSet.ResourceOperationRow row)
            : base(row.ExternalId, row.Name, row.RequiredFinishQty, PTDateTime.GetSafeTimeSpan(row.CycleHrs), row.IsDescriptionNull() ? null : row.Description, row.IsNotesNull() ? null : row.Notes, row.IsUserFieldsNull() ? null : row.UserFields)
        {
            //NEWOPFIELD Bookmark

            //base values

            if (!row.IsCycleSpanManualUpdateOnlyNull())
            {
                ProductionInfoManualUpdateOnlyFlags.CycleManualUpdateOnly = row.CycleSpanManualUpdateOnly;
            }

            if (!row.IsQtyPerCycleManualUpdateOnlyNull())
            {
                ProductionInfoManualUpdateOnlyFlags.QtyPerCycle = row.QtyPerCycleManualUpdateOnly;
            }

            if (!row.IsSetupTimeManualUpdateOnlyNull())
            {
                ProductionInfoManualUpdateOnlyFlags.SetupManualUpdateOnly = row.SetupTimeManualUpdateOnly;
            }

            if (!row.IsPostProcessManualUpdateOnlyNull())
            {
                ProductionInfoManualUpdateOnlyFlags.PostProcessingManualUpdateOnly = row.PostProcessManualUpdateOnly;
            }

            if (!row.IsCleanTimeManualUpdateOnlyNull())
            {
                ProductionInfoManualUpdateOnlyFlags.CleanManualUpdateOnly = row.CleanTimeManualUpdateOnly;
            }

            if (!row.IsStorageManualUpdateOnlyNull())
            {
                ProductionInfoManualUpdateOnlyFlags.StorageSpanOverride = row.StorageManualUpdateOnly;
            }

            if (!row.IsScrapPercentManualUpdateOnlyNull())
            {
                ProductionInfoManualUpdateOnlyFlags.PlanningScrapPercent = row.ScrapPercentManualUpdateOnly;
            }

            if (!row.IsMaterialsManualUpdateOnlyNull())
            {
                ProductionInfoFlagsBaseOperation.Materials = row.MaterialsManualUpdateOnly;
            }

            if (!row.IsProductsManualUpdateOnlyNull())
            {
                ProductionInfoFlagsBaseOperation.Products = row.ProductsManualUpdateOnly;
            }

            if (!row.IsResourceRequirementsManualUpdateOnlyNull())
            {
                ProductionInfoManualUpdateOnlyFlags.ResourceRequirements = row.ResourceRequirementsManualUpdateOnly;
            }

            if (!row.IsAutoSplitNull())
            {
                AutoSplit = row.AutoSplit;
            }

            if (!row.IsCanPauseNull())
            {
                CanPause = row.CanPause;
            }

            if (!row.IsTimeBasedReportingNull())
            {
                TimeBasedReporting = row.TimeBasedReporting;
            }

            if (!row.IsCanSubcontractNull())
            {
                CanSubcontract = row.CanSubcontract;
            }

            if (!row.IsCarryingCostNull())
            {
                CarryingCost = row.CarryingCost;
            }

            if (!row.IsCompatibilityCodeNull())
            {
                CompatibilityCode = row.CompatibilityCode;
            }

            if (!row.IsUseCompatibilityCodeNull())
            {
                UseCompatibilityCode = row.UseCompatibilityCode;
            }

            if (!row.IsDeductScrapFromRequiredNull())
            {
                DeductScrapFromRequired = row.DeductScrapFromRequired;
            }

            if (!row.IsHoldReasonNull())
            {
                HoldReason = row.HoldReason;
            }

            if (!row.IsHoldUntilDateTimeNull())
            {
                HoldUntilDateTime = row.HoldUntilDateTime.ToServerTime();
            }

            if (!row.IsIsReworkNull())
            {
                IsRework = row.IsRework;
            }

            if (!row.IsKeepSuccessorsTimeLimitHrsNull())
            {
                KeepSuccessorsTimeLimit = PTDateTime.GetSafeTimeSpan(row.KeepSuccessorsTimeLimitHrs);
            }

            if (!row.IsOverlapTransferQtyNull())
            {
                OverlapTransferQty = row.OverlapTransferQty;
            }

            if (!row.IsOmittedNull())
            {
                try
                {
                    Omitted = (BaseOperationDefs.omitStatuses)Enum.Parse(typeof(BaseOperationDefs.omitStatuses), row.Omitted);
                }
                catch (Exception err)
                {
                    throw new PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            row.Omitted, "Operation", "Omitted",
                            string.Join(", ", Enum.GetNames(typeof(BaseOperationDefs.omitStatuses)))
                        });
                }
            }

            if (!row.IsOnHoldNull())
            {
                OnHold = row.OnHold;
            }

            if (!row.IsPlanningScrapPercentNull())
            {
                PlanningScrapPercent = row.PlanningScrapPercent;
            }

            if (!row.IsPostProcessingHrsNull())
            {
                PostProcessingSpan = PTDateTime.GetSafeTimeSpan(row.PostProcessingHrs);
            }

            if (!row.IsCleanHrsNull())
            {
                CleanSpan = PTDateTime.GetSafeTimeSpan(row.CleanHrs);
            }

            if (!row.IsCleanOutGradeNull())
            {
                CleanOutGrade = row.CleanOutGrade;
            }

            if (!row.IsCleanoutCostNull())
            {
                CleanoutCost = row.CleanoutCost;
            }

            if (!row.IsStorageHrsNull())
            {
                StorageTimeSpan = PTDateTime.GetSafeTimeSpan(row.StorageHrs);
            }

            if (!row.IsQtyPerCycleNull())
            {
                QtyPerCycle = row.QtyPerCycle;
            }

            if (!row.IsBatchCodeNull())
            {
                BatchCode = row.BatchCode;
            }

            //The job dialog always populates both.  
            if (!row.IsSetupColorNull())
            {
                SetupColor = ColorUtils.GetColorFromHexString(row.SetupColor);
            }

            if (!row.IsSetupNumberNull())
            {
                SetupNumber = row.SetupNumber;
            }

            if (!row.IsSetupHrsNull())
            {
                SetupSpan = PTDateTime.GetSafeTimeSpan(row.SetupHrs);
            }
            
            if (!row.IsProductionSetupCostNull())
            {
                ProductionSetupCost = row.ProductionSetupCost;
            }

            if (!row.IsSplitUpdateModeNull())
            {
                try
                {
                    SplitUpdateMode = (InternalOperationDefs.splitUpdateModes)Enum.Parse(typeof(InternalOperationDefs.splitUpdateModes), row.SplitUpdateMode);
                }
                catch (Exception err)
                {
                    throw new PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            row.SplitUpdateMode, "Operation", "SplitUpdateMode",
                            string.Join(", ", Enum.GetNames(typeof(InternalOperationDefs.splitUpdateModes)))
                        });
                }
            }

            if (!row.IsSuccessorProcessingNull())
            {
                try
                {
                    SuccessorProcessing = (OperationDefs.successorProcessingEnumeration)Enum.Parse(typeof(OperationDefs.successorProcessingEnumeration), row.SuccessorProcessing);
                }
                catch (Exception err)
                {
                    throw new PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            row.SuccessorProcessing, "Operation", "SuccessorProcessing",
                            string.Join(", ", Enum.GetNames(typeof(OperationDefs.successorProcessingEnumeration)))
                        });
                }
            }

            if (!row.IsUseExpectedFinishQtyNull())
            {
                UseExpectedFinishQty = row.UseExpectedFinishQty;
            }

            if (!row.IsUOMNull())
            {
                UOM = row.UOM;
            }

            if (!row.IsOutputNameNull())
            {
                OutputName = row.OutputName;
            }

            if (!row.IsWholeNumberSplitsNull())
            {
                WholeNumberSplits = row.WholeNumberSplits;
            }

            if (!row.IsAutoReportProgressNull())
            {
                AutoReportProgress = row.AutoReportProgress;
            }

            if (!row.IsAutoFinishNull())
            {
                AutoFinish = row.AutoFinish;
            }

            if (!row.IsStandardSetupHrsNull())
            {
                StandardSetupSpan = PTDateTime.GetSafeTimeSpan(row.StandardSetupHrs);
            }

            if (!row.IsStandardRunHrsNull())
            {
                StandardRunSpan = PTDateTime.GetSafeTimeSpan(row.StandardRunHrs);
            }

            if (!row.IsUserFieldsNull())
            {
                SetUserFields(row.UserFields);
            }

            if (!row.IsCommitStartDateNull())
            {
                CommitStartDate = row.CommitStartDate;
            }

            if (!row.IsCommitEndDateNull())
            {
                CommitEndDate = row.CommitEndDate;
            }

            if (!row.IsCanResizeNull())
            {
                CanResize = row.CanResize;
            }

            if (!row.IsMinOperationBufferDaysNull())
            {
                MinOperationBufferTicks = TimeSpan.FromDays(row.MinOperationBufferDays).Ticks;
            }

            if (!row.IsPlannedScrapQtyNull())
            {
                PlannedScrapQty = row.PlannedScrapQty;
            }

            if (!row.IsProductCodeNull())
            {
                ProductCode = row.ProductCode;
            }

            if (!row.IsAutoSplitTypeNull())
            {
                SplitType = (OperationDefs.EAutoSplitType)Enum.Parse(typeof(OperationDefs.EAutoSplitType), row.AutoSplitType);
            }

            if (!row.IsMinAutoSplitAmountNull())
            {
                if (row.MinAutoSplitAmount < 0)
                {
                    throw new PTValidationException("4476", new object[] { row.ExternalId, row.JobExternalId });
                }

                MinAutoSplitAmount = row.MinAutoSplitAmount;
            }

            if (!row.IsMaxAutoSplitAmountNull())
            {
                if (row.MaxAutoSplitAmount < 0)
                {
                    throw new PTValidationException("4477", new object[] { row.ExternalId, row.JobExternalId });
                }

                MaxAutoSplitAmount = row.MaxAutoSplitAmount;
            }

            if (MaxAutoSplitAmount < MinAutoSplitAmount)
            {
                throw new PTValidationException("4490", new object[] { row.ExternalId, row.JobExternalId });
            }

            if (!row.IsKeepSplitsOnSameResourceNull())
            {
                KeepSplitsOnSameResource = row.KeepSplitsOnSameResource;
            }

            if (!row.IsSetupSplitTypeNull())
            {
                SetupSplitType = (OperationDefs.ESetupSplitType)Enum.Parse(typeof(OperationDefs.ESetupSplitType), row.SetupSplitType);
            }
            
            if (!row.IsPreventSplitsFromIncurringSetupNull())
            {
                PreventSplitsFromIncurringSetup = row.PreventSplitsFromIncurringSetup;
            }
            if (!row.IsPreventSplitsFromIncurringCleanNull())
            {
                PreventSplitsFromIncurringClean = row.PreventSplitsFromIncurringClean;
            }

            if (!row.IsCampaignCodeNull())
            {
                CampaignCode = row.CampaignCode;
            }

            if (!row.IsSequenceHeadStartDaysNull())
            {
                SequenceHeadStartDays = row.SequenceHeadStartDays;
            }

            if (!row.IsAllowSameLotInNonEmptyStorageAreaNull())
            {
                AllowSameLotInNonEmptyStorageArea = row.AllowSameLotInNonEmptyStorageArea;
            }
        }

        #region Shared Properties
        private decimal qtyPerCycle = 1;

        /// <summary>
        /// The quantity of product produced during each production cycle.
        /// </summary>
        public decimal QtyPerCycle
        {
            get => qtyPerCycle;
            set => qtyPerCycle = value;
        }
        #endregion Shared Properties

        public override void Validate()
        {
            base.Validate();
            ValidateQtyPerCycle();
        }

        private void ValidateQtyPerCycle()
        {
            if (QtyPerCycle <= 0)
            {
                throw new ValidationException("2108", new object[] { ExternalId, Name, Description });
            }
        }
    }
}