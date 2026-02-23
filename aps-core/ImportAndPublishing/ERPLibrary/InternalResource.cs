using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

/// <summary>
/// For creating an InternalResource via ERP transmission.
/// </summary>
public class InternalResource : BaseResource, IPTSerializable
{
    public new const int UNIQUE_ID = 213;

    #region IPTSerializable Members
    public InternalResource(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 13005)
        {
            reader.Read(out overtimeHourlyCost);
            reader.Read(out m_canOffload);
            reader.Read(out m_canPreemptMaterials);
            reader.Read(out canPreemptPredecessors);
            reader.Read(out canWorkOvertime);
            reader.Read(out int val);
            m_capacityType = (InternalResourceDefs.capacityTypes)val;
            reader.Read(out consecutiveSetupTimes);
            reader.Read(out cycleEfficiencyMultiplier);
            reader.Read(out departmentExternalId);
            reader.Read(out plantExternalId);
            reader.Read(out m_discontinueSameCellScheduling);
            reader.Read(out m_scheduledRunSpanAlgorithm);
            reader.Read(out m_scheduledSetupSpanAlgorithm);
            reader.Read(out scheduledTransferSpanAlgorithm);
            reader.Read(out m_activitySetupEfficiencyMultiplier);
            reader.Read(out m_changeoverSetupEfficiencyMultiplier);
            reader.Read(out stage);
            reader.Read(out m_standardHourlyCost);
            reader.Read(out m_useOperationSetupTime);
            reader.Read(out m_noDefaultRecurringCapacityInterval);

            //Was Set flags
            reader.Read(out overtimeHourlyCostSet);
            reader.Read(out m_canOffloadSet);
            reader.Read(out canPreemptMaterialsSet);
            reader.Read(out canPreemptPredecessorsSet);
            reader.Read(out canWorkOvertimeSet);
            reader.Read(out consecutiveSetupTimesSet);
            reader.Read(out cycleEfficiencyMultiplierSet);
            reader.Read(out m_discontinueSameCellSchedulingSet);
            reader.Read(out m_scheduledRunSpanAlgorithmSet);
            reader.Read(out m_scheduledSetupSpanAlgorithmSet);
            reader.Read(out m_scheduledTransferSpanAlgorithmSet);
            reader.Read(out m_activitySetupEfficiencyMultiplierSet);
            reader.Read(out m_stageSet);
            reader.Read(out m_standardHourlyCostSet);
            reader.Read(out m_useOperationSetupTimeSet);
            reader.Read(out m_bufferSpanSet);
            reader.Read(out headStartSpanSet);
            reader.Read(out m_setupSpanSet);
            reader.Read(out transferSpanSet);
            reader.Read(out m_batchTypeSet);
            reader.Read(out m_batchVolumeSet);
            //End Was Set flags

            reader.Read(out bufferSpan);
            reader.Read(out headStartSpan);
            reader.Read(out setupSpan);
            reader.Read(out transferSpan);
            reader.Read(out m_autoJoinSpan);

            reader.Read(out val);
            m_batchType = (MainResourceDefs.batchType)val;

            reader.Read(out m_batchVolume);
            bools = new BoolVector32(reader);

            reader.Read(out m_minNbrOfPeople);
            reader.Read(out m_minNbrOfPeopleSet);

            reader.Read(out m_standardCleanSpan);
            reader.Read(out m_standardCleanoutGrade);
            reader.Read(out m_priority);
            reader.Read(out m_resourceSetupCost);
            reader.Read(out m_resourceCleanoutCost);
        }
        else if (reader.VersionNumber >= 12510)
        {
            reader.Read(out overtimeHourlyCost);
            reader.Read(out m_canOffload);
            reader.Read(out m_canPreemptMaterials);
            reader.Read(out canPreemptPredecessors);
            reader.Read(out canWorkOvertime);
            reader.Read(out int val);
            m_capacityType = (InternalResourceDefs.capacityTypes)val;
            reader.Read(out consecutiveSetupTimes);
            reader.Read(out cycleEfficiencyMultiplier);
            reader.Read(out departmentExternalId);
            reader.Read(out bool drum);
            reader.Read(out plantExternalId);
            reader.Read(out m_discontinueSameCellScheduling);
            reader.Read(out m_scheduledRunSpanAlgorithm);
            reader.Read(out m_scheduledSetupSpanAlgorithm);
            reader.Read(out scheduledTransferSpanAlgorithm);
            reader.Read(out m_activitySetupEfficiencyMultiplier);
            reader.Read(out m_changeoverSetupEfficiencyMultiplier);
            reader.Read(out stage);
            reader.Read(out m_standardHourlyCost);
            reader.Read(out m_useOperationSetupTime);
            reader.Read(out m_noDefaultRecurringCapacityInterval);

            //Was Set flags
            reader.Read(out overtimeHourlyCostSet);
            reader.Read(out m_canOffloadSet);
            reader.Read(out canPreemptMaterialsSet);
            reader.Read(out canPreemptPredecessorsSet);
            reader.Read(out canWorkOvertimeSet);
            reader.Read(out consecutiveSetupTimesSet);
            reader.Read(out cycleEfficiencyMultiplierSet);
            reader.Read(out bool drumSet);
            reader.Read(out m_discontinueSameCellSchedulingSet);
            reader.Read(out m_scheduledRunSpanAlgorithmSet);
            reader.Read(out m_scheduledSetupSpanAlgorithmSet);
            reader.Read(out m_scheduledTransferSpanAlgorithmSet);
            reader.Read(out m_activitySetupEfficiencyMultiplierSet);
            reader.Read(out m_stageSet);
            reader.Read(out m_standardHourlyCostSet);
            reader.Read(out m_useOperationSetupTimeSet);
            reader.Read(out m_bufferSpanSet);
            reader.Read(out headStartSpanSet);
            reader.Read(out m_setupSpanSet);
            reader.Read(out transferSpanSet);
            reader.Read(out m_batchTypeSet);
            reader.Read(out m_batchVolumeSet);
            //End Was Set flags

            reader.Read(out bufferSpan);
            reader.Read(out headStartSpan);
            reader.Read(out setupSpan);
            reader.Read(out transferSpan);
            reader.Read(out m_autoJoinSpan);

            reader.Read(out val);
            m_batchType = (MainResourceDefs.batchType)val;

            reader.Read(out m_batchVolume);
            bools = new BoolVector32(reader);

            reader.Read(out m_minNbrOfPeople);
            reader.Read(out m_minNbrOfPeopleSet);

            reader.Read(out m_standardCleanSpan);
            reader.Read(out m_standardCleanoutGrade);
            reader.Read(out m_priority);
            reader.Read(out m_resourceSetupCost);
            reader.Read(out m_resourceCleanoutCost);
        }
        else if (reader.VersionNumber >= 12504)
        {
            reader.Read(out overtimeHourlyCost);
            reader.Read(out m_canOffload);
            reader.Read(out m_canPreemptMaterials);
            reader.Read(out canPreemptPredecessors);
            reader.Read(out canWorkOvertime);
            reader.Read(out int val);
            m_capacityType = (InternalResourceDefs.capacityTypes)val;
            reader.Read(out consecutiveSetupTimes);
            reader.Read(out cycleEfficiencyMultiplier);
            reader.Read(out departmentExternalId);
            reader.Read(out bool drum);
            reader.Read(out plantExternalId);
            reader.Read(out m_discontinueSameCellScheduling);
            reader.Read(out m_scheduledRunSpanAlgorithm);
            reader.Read(out m_scheduledSetupSpanAlgorithm);
            reader.Read(out scheduledTransferSpanAlgorithm);
            reader.Read(out m_activitySetupEfficiencyMultiplier);
            reader.Read(out m_changeoverSetupEfficiencyMultiplier);
            reader.Read(out val);
            if (val == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                val = 9;
                UseSequencedSetupTime = true;
            }

            reader.Read(out stage);
            reader.Read(out m_standardHourlyCost);
            reader.Read(out m_useOperationSetupTime);
            reader.Read(out m_noDefaultRecurringCapacityInterval);

            //Was Set flags
            reader.Read(out overtimeHourlyCostSet);
            reader.Read(out m_canOffloadSet);
            reader.Read(out canPreemptMaterialsSet);
            reader.Read(out canPreemptPredecessorsSet);
            reader.Read(out canWorkOvertimeSet);
            reader.Read(out consecutiveSetupTimesSet);
            reader.Read(out cycleEfficiencyMultiplierSet);
            reader.Read(out bool drumSet);
            reader.Read(out m_discontinueSameCellSchedulingSet);
            reader.Read(out m_scheduledRunSpanAlgorithmSet);
            reader.Read(out m_scheduledSetupSpanAlgorithmSet);
            reader.Read(out m_scheduledTransferSpanAlgorithmSet);
            reader.Read(out m_activitySetupEfficiencyMultiplierSet);
            reader.Read(out bool setupIncludedSet);
            reader.Read(out m_stageSet);
            reader.Read(out m_standardHourlyCostSet);
            reader.Read(out m_useOperationSetupTimeSet);
            reader.Read(out m_bufferSpanSet);
            reader.Read(out headStartSpanSet);
            reader.Read(out m_setupSpanSet);
            reader.Read(out transferSpanSet);
            reader.Read(out m_batchTypeSet);
            reader.Read(out m_batchVolumeSet);
            //End Was Set flags

            reader.Read(out bufferSpan);
            reader.Read(out headStartSpan);
            reader.Read(out setupSpan);
            reader.Read(out transferSpan);
            reader.Read(out m_autoJoinSpan);

            reader.Read(out val);
            m_batchType = (MainResourceDefs.batchType)val;

            reader.Read(out m_batchVolume);
            bools = new BoolVector32(reader);

            reader.Read(out m_minNbrOfPeople);
            reader.Read(out m_minNbrOfPeopleSet);

            reader.Read(out m_standardCleanSpan);
            reader.Read(out m_standardCleanoutGrade);
            reader.Read(out m_priority);
        }
        else if (reader.VersionNumber >= 12502)
        {
            reader.Read(out overtimeHourlyCost);
            reader.Read(out m_canOffload);
            reader.Read(out m_canPreemptMaterials);
            reader.Read(out canPreemptPredecessors);
            reader.Read(out canWorkOvertime);
            reader.Read(out int val);
            m_capacityType = (InternalResourceDefs.capacityTypes)val;
            reader.Read(out consecutiveSetupTimes);
            reader.Read(out cycleEfficiencyMultiplier);
            reader.Read(out departmentExternalId);
            reader.Read(out bool drum);
            reader.Read(out plantExternalId);
            reader.Read(out m_discontinueSameCellScheduling);
            reader.Read(out m_scheduledRunSpanAlgorithm);
            reader.Read(out m_scheduledSetupSpanAlgorithm);
            reader.Read(out scheduledTransferSpanAlgorithm);
            reader.Read(out m_activitySetupEfficiencyMultiplier);
            reader.Read(out m_changeoverSetupEfficiencyMultiplier);
            reader.Read(out val);
            if (val == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                val = 9;
                UseSequencedSetupTime = true;
            }

            reader.Read(out stage);
            reader.Read(out m_standardHourlyCost);
            reader.Read(out m_useOperationSetupTime);
            reader.Read(out m_noDefaultRecurringCapacityInterval);

            //Was Set flags
            reader.Read(out overtimeHourlyCostSet);
            reader.Read(out m_canOffloadSet);
            reader.Read(out canPreemptMaterialsSet);
            reader.Read(out canPreemptPredecessorsSet);
            reader.Read(out canWorkOvertimeSet);
            reader.Read(out consecutiveSetupTimesSet);
            reader.Read(out cycleEfficiencyMultiplierSet);
            reader.Read(out bool drumSet);
            reader.Read(out m_discontinueSameCellSchedulingSet);
            reader.Read(out m_scheduledRunSpanAlgorithmSet);
            reader.Read(out m_scheduledSetupSpanAlgorithmSet);
            reader.Read(out m_scheduledTransferSpanAlgorithmSet);
            reader.Read(out m_activitySetupEfficiencyMultiplierSet);
            reader.Read(out bool setupIncludedSet);
            reader.Read(out m_stageSet);
            reader.Read(out m_standardHourlyCostSet);
            reader.Read(out m_useOperationSetupTimeSet);
            reader.Read(out m_bufferSpanSet);
            reader.Read(out headStartSpanSet);
            reader.Read(out m_setupSpanSet);
            reader.Read(out transferSpanSet);
            reader.Read(out m_batchTypeSet);
            reader.Read(out m_batchVolumeSet);
            //End Was Set flags

            reader.Read(out bufferSpan);
            reader.Read(out headStartSpan);
            reader.Read(out setupSpan);
            reader.Read(out transferSpan);
            reader.Read(out m_autoJoinSpan);

            reader.Read(out val);
            m_batchType = (MainResourceDefs.batchType)val;

            reader.Read(out m_batchVolume);
            bools = new BoolVector32(reader);

            reader.Read(out m_minNbrOfPeople);
            reader.Read(out m_minNbrOfPeopleSet);

            reader.Read(out m_standardCleanSpan);
            reader.Read(out m_standardCleanoutGrade);
            //m_lastRunActivityKey = new ActivityKeyExternal(reader);
        }
        else if (reader.VersionNumber >= 12501)
        {
            reader.Read(out overtimeHourlyCost);
            reader.Read(out m_canOffload);
            reader.Read(out m_canPreemptMaterials);
            reader.Read(out canPreemptPredecessors);
            reader.Read(out canWorkOvertime);
            reader.Read(out int val);
            m_capacityType = (InternalResourceDefs.capacityTypes)val;
            reader.Read(out consecutiveSetupTimes);
            reader.Read(out cycleEfficiencyMultiplier);
            reader.Read(out departmentExternalId);
            reader.Read(out bool drum);
            reader.Read(out plantExternalId);
            reader.Read(out m_discontinueSameCellScheduling);
            reader.Read(out m_scheduledRunSpanAlgorithm);
            reader.Read(out m_scheduledSetupSpanAlgorithm);
            reader.Read(out scheduledTransferSpanAlgorithm);
            reader.Read(out m_activitySetupEfficiencyMultiplier);
            reader.Read(out m_changeoverSetupEfficiencyMultiplier);
            reader.Read(out val);
            if (val == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                UseSequencedSetupTime = true;
            }
            reader.Read(out stage);
            reader.Read(out m_standardHourlyCost);
            reader.Read(out m_useOperationSetupTime);
            reader.Read(out m_noDefaultRecurringCapacityInterval);

            //Was Set flags
            reader.Read(out overtimeHourlyCostSet);
            reader.Read(out m_canOffloadSet);
            reader.Read(out canPreemptMaterialsSet);
            reader.Read(out canPreemptPredecessorsSet);
            reader.Read(out canWorkOvertimeSet);
            reader.Read(out consecutiveSetupTimesSet);
            reader.Read(out cycleEfficiencyMultiplierSet);
            reader.Read(out bool drumSet);
            reader.Read(out m_discontinueSameCellSchedulingSet);
            reader.Read(out m_scheduledRunSpanAlgorithmSet);
            reader.Read(out m_scheduledSetupSpanAlgorithmSet);
            reader.Read(out m_scheduledTransferSpanAlgorithmSet);
            reader.Read(out m_activitySetupEfficiencyMultiplierSet);
            reader.Read(out bool setupIncludedSet);
            reader.Read(out m_stageSet);
            reader.Read(out m_standardHourlyCostSet);
            reader.Read(out m_useOperationSetupTimeSet);
            reader.Read(out m_bufferSpanSet);
            reader.Read(out headStartSpanSet);
            reader.Read(out bool maxSameSetupSpanSet);
            reader.Read(out m_setupSpanSet);
            reader.Read(out transferSpanSet);
            reader.Read(out m_batchTypeSet);
            reader.Read(out m_batchVolumeSet);
            //End Was Set flags

            reader.Read(out bufferSpan);
            reader.Read(out headStartSpan);
            reader.Read(out TimeSpan maxSameSetupSpan);
            reader.Read(out setupSpan);
            reader.Read(out transferSpan);
            reader.Read(out m_autoJoinSpan);

            reader.Read(out val);
            m_batchType = (MainResourceDefs.batchType)val;

            reader.Read(out m_batchVolume);
            bools = new BoolVector32(reader);

            reader.Read(out m_minNbrOfPeople);
            reader.Read(out m_minNbrOfPeopleSet);

            reader.Read(out m_standardCleanSpan);
            reader.Read(out m_standardCleanoutGrade);
            //m_lastRunActivityKey = new ActivityKeyExternal(reader);
        }
        else if (reader.VersionNumber >= 12500)
        {
            reader.Read(out overtimeHourlyCost);
            reader.Read(out m_canOffload);
            reader.Read(out m_canPreemptMaterials);
            reader.Read(out canPreemptPredecessors);
            reader.Read(out canWorkOvertime);
            reader.Read(out int val);
            m_capacityType = (InternalResourceDefs.capacityTypes)val;
            reader.Read(out consecutiveSetupTimes);
            reader.Read(out cycleEfficiencyMultiplier);
            reader.Read(out departmentExternalId);
            reader.Read(out bool drum);
            reader.Read(out plantExternalId);
            reader.Read(out m_discontinueSameCellScheduling);
            reader.Read(out m_scheduledRunSpanAlgorithm);
            reader.Read(out m_scheduledSetupSpanAlgorithm);
            reader.Read(out scheduledTransferSpanAlgorithm);
            reader.Read(out m_activitySetupEfficiencyMultiplier);
            reader.Read(out m_changeoverSetupEfficiencyMultiplier);
            reader.Read(out val);
            if (val == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                UseSequencedSetupTime = true;
            }
            reader.Read(out stage);
            reader.Read(out m_standardHourlyCost);
            reader.Read(out m_useOperationSetupTime);
            reader.Read(out m_noDefaultRecurringCapacityInterval);

            //Was Set flags
            reader.Read(out overtimeHourlyCostSet);
            reader.Read(out m_canOffloadSet);
            reader.Read(out canPreemptMaterialsSet);
            reader.Read(out canPreemptPredecessorsSet);
            reader.Read(out canWorkOvertimeSet);
            reader.Read(out consecutiveSetupTimesSet);
            reader.Read(out cycleEfficiencyMultiplierSet);
            reader.Read(out bool drumSet);
            reader.Read(out m_discontinueSameCellSchedulingSet);
            reader.Read(out m_scheduledRunSpanAlgorithmSet);
            reader.Read(out m_scheduledSetupSpanAlgorithmSet);
            reader.Read(out m_scheduledTransferSpanAlgorithmSet);
            reader.Read(out m_activitySetupEfficiencyMultiplierSet);
            reader.Read(out bool setupIncludedSet);
            reader.Read(out m_stageSet);
            reader.Read(out m_standardHourlyCostSet);
            reader.Read(out m_useOperationSetupTimeSet);
            reader.Read(out m_bufferSpanSet);
            reader.Read(out headStartSpanSet);
            reader.Read(out bool maxSameSetupSpanSet);
            reader.Read(out m_setupSpanSet);
            reader.Read(out transferSpanSet);
            reader.Read(out m_batchTypeSet);
            reader.Read(out m_batchVolumeSet);
            //End Was Set flags

            reader.Read(out bufferSpan);
            reader.Read(out headStartSpan);
            reader.Read(out TimeSpan maxSameSetupSpan);
            reader.Read(out setupSpan);
            reader.Read(out transferSpan);
            reader.Read(out m_autoJoinSpan);

            reader.Read(out val);
            m_batchType = (MainResourceDefs.batchType)val;

            reader.Read(out m_batchVolume);
            bools = new BoolVector32(reader);

            reader.Read(out m_minNbrOfPeople);
            reader.Read(out m_minNbrOfPeopleSet);

            reader.Read(out m_standardCleanSpan);
            reader.Read(out m_standardCleanoutGrade);
            new ActivityKeyExternal(reader);
        }
        else if (reader.VersionNumber >= 12420)
        {
            reader.Read(out overtimeHourlyCost);
            reader.Read(out m_canOffload);
            reader.Read(out m_canPreemptMaterials);
            reader.Read(out canPreemptPredecessors);
            reader.Read(out canWorkOvertime);
            int val;
            reader.Read(out val);
            m_capacityType = (InternalResourceDefs.capacityTypes)val;
            reader.Read(out consecutiveSetupTimes);
            reader.Read(out cycleEfficiencyMultiplier);
            reader.Read(out departmentExternalId);
            reader.Read(out bool drum);
            reader.Read(out plantExternalId);
            reader.Read(out m_discontinueSameCellScheduling);
            reader.Read(out m_scheduledRunSpanAlgorithm);
            reader.Read(out m_scheduledSetupSpanAlgorithm);
            reader.Read(out scheduledTransferSpanAlgorithm);
            reader.Read(out m_activitySetupEfficiencyMultiplier);
            reader.Read(out m_changeoverSetupEfficiencyMultiplier);
            reader.Read(out val);
            if (val == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                UseSequencedSetupTime = true;
            }
            reader.Read(out stage);
            reader.Read(out m_standardHourlyCost);
            reader.Read(out m_useOperationSetupTime);
            reader.Read(out m_noDefaultRecurringCapacityInterval);

            //Was Set flags
            reader.Read(out overtimeHourlyCostSet);
            reader.Read(out m_canOffloadSet);
            reader.Read(out canPreemptMaterialsSet);
            reader.Read(out canPreemptPredecessorsSet);
            reader.Read(out canWorkOvertimeSet);
            reader.Read(out consecutiveSetupTimesSet);
            reader.Read(out cycleEfficiencyMultiplierSet);
            reader.Read(out bool drumSet);
            reader.Read(out m_discontinueSameCellSchedulingSet);
            reader.Read(out m_scheduledRunSpanAlgorithmSet);
            reader.Read(out m_scheduledSetupSpanAlgorithmSet);
            reader.Read(out m_scheduledTransferSpanAlgorithmSet);
            reader.Read(out m_activitySetupEfficiencyMultiplierSet);
            reader.Read(out bool setupIncludedSet);
            reader.Read(out m_stageSet);
            reader.Read(out m_standardHourlyCostSet);
            reader.Read(out m_useOperationSetupTimeSet);
            reader.Read(out m_bufferSpanSet);
            reader.Read(out headStartSpanSet);
            reader.Read(out bool maxSameSetupSpanSet);
            reader.Read(out m_setupSpanSet);
            reader.Read(out transferSpanSet);
            reader.Read(out m_batchTypeSet);
            reader.Read(out m_batchVolumeSet);
            //End Was Set flags

            reader.Read(out bufferSpan);
            reader.Read(out headStartSpan);
            reader.Read(out TimeSpan maxSameSetupSpan);
            reader.Read(out setupSpan);
            reader.Read(out transferSpan);
            reader.Read(out m_autoJoinSpan);

            reader.Read(out val);
            m_batchType = (MainResourceDefs.batchType)val;

            reader.Read(out m_batchVolume);
            bools = new BoolVector32(reader);

            reader.Read(out m_minNbrOfPeople);
            reader.Read(out m_minNbrOfPeopleSet);

            reader.Read(out m_standardCleanSpan);
            reader.Read(out m_standardCleanoutGrade);
            new ActivityKeyExternal(reader);
            reader.Read(out m_priority);
        }
        else if (reader.VersionNumber >= 12401)
        {
            reader.Read(out overtimeHourlyCost);
            reader.Read(out m_canOffload);
            reader.Read(out m_canPreemptMaterials);
            reader.Read(out canPreemptPredecessors);
            reader.Read(out canWorkOvertime);
            reader.Read(out int val);
            m_capacityType = (InternalResourceDefs.capacityTypes)val;
            reader.Read(out consecutiveSetupTimes);
            reader.Read(out cycleEfficiencyMultiplier);
            reader.Read(out departmentExternalId);
            reader.Read(out bool drum);
            reader.Read(out plantExternalId);
            reader.Read(out m_discontinueSameCellScheduling);
            reader.Read(out m_scheduledRunSpanAlgorithm);
            reader.Read(out m_scheduledSetupSpanAlgorithm);
            reader.Read(out scheduledTransferSpanAlgorithm);
            reader.Read(out m_activitySetupEfficiencyMultiplier);
            reader.Read(out m_changeoverSetupEfficiencyMultiplier);
            reader.Read(out val);
            if (val == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                UseSequencedSetupTime = true;
            }
            reader.Read(out stage);
            reader.Read(out m_standardHourlyCost);
            reader.Read(out m_useOperationSetupTime);
            reader.Read(out m_noDefaultRecurringCapacityInterval);

            //Was Set flags
            reader.Read(out overtimeHourlyCostSet);
            reader.Read(out m_canOffloadSet);
            reader.Read(out canPreemptMaterialsSet);
            reader.Read(out canPreemptPredecessorsSet);
            reader.Read(out canWorkOvertimeSet);
            reader.Read(out consecutiveSetupTimesSet);
            reader.Read(out cycleEfficiencyMultiplierSet);
            reader.Read(out bool drumSet);
            reader.Read(out m_discontinueSameCellSchedulingSet);
            reader.Read(out m_scheduledRunSpanAlgorithmSet);
            reader.Read(out m_scheduledSetupSpanAlgorithmSet);
            reader.Read(out m_scheduledTransferSpanAlgorithmSet);
            reader.Read(out m_activitySetupEfficiencyMultiplierSet);
            reader.Read(out bool setupIncludedSet);
            reader.Read(out m_stageSet);
            reader.Read(out m_standardHourlyCostSet);
            reader.Read(out m_useOperationSetupTimeSet);
            reader.Read(out m_bufferSpanSet);
            reader.Read(out headStartSpanSet);
            reader.Read(out bool maxSameSetupSpanSet);
            reader.Read(out m_setupSpanSet);
            reader.Read(out transferSpanSet);
            reader.Read(out m_batchTypeSet);
            reader.Read(out m_batchVolumeSet);
            //End Was Set flags

            reader.Read(out bufferSpan);
            reader.Read(out headStartSpan);
            reader.Read(out TimeSpan maxSameSetupSpan);
            reader.Read(out setupSpan);
            reader.Read(out transferSpan);
            reader.Read(out m_autoJoinSpan);

            reader.Read(out val);
            m_batchType = (MainResourceDefs.batchType)val;

            reader.Read(out m_batchVolume);
            bools = new BoolVector32(reader);

            reader.Read(out m_minNbrOfPeople);
            reader.Read(out m_minNbrOfPeopleSet);

            reader.Read(out m_standardCleanSpan);
            reader.Read(out m_standardCleanoutGrade);
            new ActivityKeyExternal(reader);
        }
        else if (reader.VersionNumber >= 12319)
        {
            reader.Read(out overtimeHourlyCost);
            reader.Read(out m_canOffload);
            reader.Read(out m_canPreemptMaterials);
            reader.Read(out canPreemptPredecessors);
            reader.Read(out canWorkOvertime);
            reader.Read(out int val);
            m_capacityType = (InternalResourceDefs.capacityTypes)val;
            reader.Read(out consecutiveSetupTimes);
            reader.Read(out cycleEfficiencyMultiplier);
            reader.Read(out departmentExternalId);
            reader.Read(out bool drum);
            reader.Read(out plantExternalId);
            reader.Read(out m_discontinueSameCellScheduling);
            reader.Read(out m_scheduledRunSpanAlgorithm);
            reader.Read(out m_scheduledSetupSpanAlgorithm);
            reader.Read(out scheduledTransferSpanAlgorithm);
            reader.Read(out m_activitySetupEfficiencyMultiplier);
            reader.Read(out val);
            if (val == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                UseSequencedSetupTime = true;
            }
            reader.Read(out stage);
            reader.Read(out m_standardHourlyCost);
            reader.Read(out m_useOperationSetupTime);
            reader.Read(out m_noDefaultRecurringCapacityInterval);

            //Was Set flags
            reader.Read(out overtimeHourlyCostSet);
            reader.Read(out m_canOffloadSet);
            reader.Read(out canPreemptMaterialsSet);
            reader.Read(out canPreemptPredecessorsSet);
            reader.Read(out canWorkOvertimeSet);
            reader.Read(out consecutiveSetupTimesSet);
            reader.Read(out cycleEfficiencyMultiplierSet);
            reader.Read(out bool drumSet);
            reader.Read(out m_discontinueSameCellSchedulingSet);
            reader.Read(out m_scheduledRunSpanAlgorithmSet);
            reader.Read(out m_scheduledSetupSpanAlgorithmSet);
            reader.Read(out m_scheduledTransferSpanAlgorithmSet);
            reader.Read(out m_activitySetupEfficiencyMultiplierSet);
            reader.Read(out bool setupIncludedSet);
            reader.Read(out m_stageSet);
            reader.Read(out m_standardHourlyCostSet);
            reader.Read(out m_useOperationSetupTimeSet);
            reader.Read(out m_bufferSpanSet);
            reader.Read(out headStartSpanSet);
            reader.Read(out bool maxSameSetupSpanSet);
            reader.Read(out m_setupSpanSet);
            reader.Read(out transferSpanSet);
            reader.Read(out m_batchTypeSet);
            reader.Read(out m_batchVolumeSet);
            //End Was Set flags

            reader.Read(out bufferSpan);
            reader.Read(out headStartSpan);
            reader.Read(out TimeSpan maxSameSetupSpan);
            reader.Read(out setupSpan);
            reader.Read(out transferSpan);
            reader.Read(out m_autoJoinSpan);

            reader.Read(out val);
            m_batchType = (MainResourceDefs.batchType)val;

            reader.Read(out m_batchVolume);
            bools = new BoolVector32(reader);

            reader.Read(out m_minNbrOfPeople);
            reader.Read(out m_minNbrOfPeopleSet);

            reader.Read(out m_standardCleanSpan);
            reader.Read(out m_standardCleanoutGrade);
            new ActivityKeyExternal(reader);
        }
        else if (reader.VersionNumber >= 12317)
        {
            reader.Read(out overtimeHourlyCost);
            reader.Read(out m_canOffload);
            reader.Read(out m_canPreemptMaterials);
            reader.Read(out canPreemptPredecessors);
            reader.Read(out canWorkOvertime);
            reader.Read(out int val);
            m_capacityType = (InternalResourceDefs.capacityTypes)val;
            reader.Read(out consecutiveSetupTimes);
            reader.Read(out cycleEfficiencyMultiplier);
            reader.Read(out departmentExternalId);
            reader.Read(out bool drum);
            reader.Read(out plantExternalId);
            reader.Read(out m_discontinueSameCellScheduling);
            reader.Read(out m_scheduledRunSpanAlgorithm);
            reader.Read(out m_scheduledSetupSpanAlgorithm);
            reader.Read(out scheduledTransferSpanAlgorithm);
            reader.Read(out m_activitySetupEfficiencyMultiplier);
            reader.Read(out val);
            if (val == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                UseSequencedSetupTime = true;
            }
            reader.Read(out stage);
            reader.Read(out m_standardHourlyCost);
            reader.Read(out m_useOperationSetupTime);
            reader.Read(out m_noDefaultRecurringCapacityInterval);

            //Was Set flags
            reader.Read(out overtimeHourlyCostSet);
            reader.Read(out m_canOffloadSet);
            reader.Read(out canPreemptMaterialsSet);
            reader.Read(out canPreemptPredecessorsSet);
            reader.Read(out canWorkOvertimeSet);
            reader.Read(out consecutiveSetupTimesSet);
            reader.Read(out cycleEfficiencyMultiplierSet);
            reader.Read(out bool drumSet);
            reader.Read(out m_discontinueSameCellSchedulingSet);
            reader.Read(out m_scheduledRunSpanAlgorithmSet);
            reader.Read(out m_scheduledSetupSpanAlgorithmSet);
            reader.Read(out m_scheduledTransferSpanAlgorithmSet);
            reader.Read(out m_activitySetupEfficiencyMultiplierSet);
            reader.Read(out bool setupIncludedSet);
            reader.Read(out m_stageSet);
            reader.Read(out m_standardHourlyCostSet);
            reader.Read(out m_useOperationSetupTimeSet);
            reader.Read(out m_bufferSpanSet);
            reader.Read(out headStartSpanSet);
            reader.Read(out bool maxSameSetupSpanSet);
            reader.Read(out m_setupSpanSet);
            reader.Read(out transferSpanSet);
            reader.Read(out m_batchTypeSet);
            reader.Read(out m_batchVolumeSet);
            //End Was Set flags

            reader.Read(out bufferSpan);
            reader.Read(out headStartSpan);
            reader.Read(out TimeSpan maxSameSetupSpan);
            reader.Read(out setupSpan);
            reader.Read(out transferSpan);
            reader.Read(out m_autoJoinSpan);

            reader.Read(out val);
            m_batchType = (MainResourceDefs.batchType)val;

            reader.Read(out m_batchVolume);
            bools = new BoolVector32(reader);

            reader.Read(out m_minNbrOfPeople);
            reader.Read(out m_minNbrOfPeopleSet);

            reader.Read(out m_standardCleanSpan);
            reader.Read(out m_standardCleanoutGrade);
        }
        #region 12303
        else if (reader.VersionNumber >= 12303)
        {
            reader.Read(out overtimeHourlyCost);
            reader.Read(out m_canOffload);
            reader.Read(out m_canPreemptMaterials);
            reader.Read(out canPreemptPredecessors);
            reader.Read(out canWorkOvertime);
            reader.Read(out int val);
            m_capacityType = (InternalResourceDefs.capacityTypes)val;
            reader.Read(out consecutiveSetupTimes);
            reader.Read(out cycleEfficiencyMultiplier);
            reader.Read(out string cycleSpanAlgorithm);
            reader.Read(out departmentExternalId);
            reader.Read(out bool drum);
            reader.Read(out plantExternalId);
            reader.Read(out m_discontinueSameCellScheduling);
            reader.Read(out m_scheduledRunSpanAlgorithm);
            reader.Read(out m_scheduledSetupSpanAlgorithm);
            reader.Read(out scheduledTransferSpanAlgorithm);
            reader.Read(out m_activitySetupEfficiencyMultiplier);
            reader.Read(out val);
            if (val == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                UseSequencedSetupTime = true;
            }
            reader.Read(out stage);
            reader.Read(out m_standardHourlyCost);
            reader.Read(out m_useOperationSetupTime);
            reader.Read(out m_noDefaultRecurringCapacityInterval);

            //Was Set flags
            reader.Read(out overtimeHourlyCostSet);
            reader.Read(out m_canOffloadSet);
            reader.Read(out canPreemptMaterialsSet);
            reader.Read(out canPreemptPredecessorsSet);
            reader.Read(out canWorkOvertimeSet);
            reader.Read(out consecutiveSetupTimesSet);
            reader.Read(out cycleEfficiencyMultiplierSet);
            reader.Read(out bool cycleSpanAlgorithmSet);
            reader.Read(out bool drumSet);
            reader.Read(out m_discontinueSameCellSchedulingSet);
            reader.Read(out m_scheduledRunSpanAlgorithmSet);
            reader.Read(out m_scheduledSetupSpanAlgorithmSet);
            reader.Read(out m_scheduledTransferSpanAlgorithmSet);
            reader.Read(out m_activitySetupEfficiencyMultiplierSet);
            reader.Read(out bool setupIncludedSet);
            reader.Read(out m_stageSet);
            reader.Read(out m_standardHourlyCostSet);
            reader.Read(out m_useOperationSetupTimeSet);
            reader.Read(out m_bufferSpanSet);
            reader.Read(out headStartSpanSet);
            reader.Read(out bool maxSameSetupSpanSet);
            reader.Read(out m_setupSpanSet);
            reader.Read(out transferSpanSet);
            reader.Read(out m_batchTypeSet);
            reader.Read(out m_batchVolumeSet);
            //End Was Set flags

            reader.Read(out bufferSpan);
            reader.Read(out headStartSpan);
            reader.Read(out TimeSpan maxSameSetupSpan);
            reader.Read(out setupSpan);
            reader.Read(out transferSpan);
            reader.Read(out m_autoJoinSpan);

            reader.Read(out val);
            m_batchType = (MainResourceDefs.batchType)val;

            reader.Read(out m_batchVolume);
            bools = new BoolVector32(reader);

            reader.Read(out string m_currentProductSetup);
            reader.Read(out string m_currentSetupCode);
            reader.Read(out decimal m_currentSetupNumber);
            reader.Read(out bool m_currentProductSetupSet);
            reader.Read(out bool currentSetupCodeSet);
            reader.Read(out bool m_currentSetupNumberSet);
            reader.Read(out m_minNbrOfPeople);
            reader.Read(out m_minNbrOfPeopleSet);

            reader.Read(out m_standardCleanSpan);
            reader.Read(out m_standardCleanoutGrade);
        }
        #endregion 12303

        #region 12101
        else if (reader.VersionNumber >= 12101)
        {
            reader.Read(out overtimeHourlyCost);
            reader.Read(out m_canOffload);
            reader.Read(out m_canPreemptMaterials);
            reader.Read(out canPreemptPredecessors);
            reader.Read(out canWorkOvertime);
            reader.Read(out int val);
            m_capacityType = (InternalResourceDefs.capacityTypes)val;
            reader.Read(out consecutiveSetupTimes);
            reader.Read(out cycleEfficiencyMultiplier);
            reader.Read(out string cycleSpanAlgorithm);
            reader.Read(out departmentExternalId);
            reader.Read(out bool drum);
            reader.Read(out plantExternalId);
            reader.Read(out m_discontinueSameCellScheduling);
            reader.Read(out m_scheduledRunSpanAlgorithm);
            reader.Read(out m_scheduledSetupSpanAlgorithm);
            reader.Read(out scheduledTransferSpanAlgorithm);
            reader.Read(out m_activitySetupEfficiencyMultiplier);
            reader.Read(out val);
            if (val == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                UseSequencedSetupTime = true;
            }
            reader.Read(out stage);
            reader.Read(out m_standardHourlyCost);
            reader.Read(out m_useOperationSetupTime);
            reader.Read(out m_noDefaultRecurringCapacityInterval);

            //Was Set flags
            reader.Read(out overtimeHourlyCostSet);
            reader.Read(out m_canOffloadSet);
            reader.Read(out canPreemptMaterialsSet);
            reader.Read(out canPreemptPredecessorsSet);
            reader.Read(out canWorkOvertimeSet);
            reader.Read(out consecutiveSetupTimesSet);
            reader.Read(out cycleEfficiencyMultiplierSet);
            reader.Read(out bool cycleSpanAlgorithmSet);
            reader.Read(out bool drumSet);
            reader.Read(out m_discontinueSameCellSchedulingSet);
            reader.Read(out m_scheduledRunSpanAlgorithmSet);
            reader.Read(out m_scheduledSetupSpanAlgorithmSet);
            reader.Read(out m_scheduledTransferSpanAlgorithmSet);
            reader.Read(out m_activitySetupEfficiencyMultiplierSet);
            reader.Read(out bool setupIncludedSet);
            reader.Read(out m_stageSet);
            reader.Read(out m_standardHourlyCostSet);
            reader.Read(out m_useOperationSetupTimeSet);
            reader.Read(out m_bufferSpanSet);
            reader.Read(out headStartSpanSet);
            reader.Read(out bool maxSameSetupSpanSet);
            reader.Read(out m_setupSpanSet);
            reader.Read(out transferSpanSet);
            reader.Read(out m_batchTypeSet);
            reader.Read(out m_batchVolumeSet);
            //End Was Set flags

            reader.Read(out bufferSpan);
            reader.Read(out headStartSpan);
            reader.Read(out TimeSpan maxSameSetupSpan);
            reader.Read(out setupSpan);
            reader.Read(out transferSpan);
            reader.Read(out m_autoJoinSpan);

            reader.Read(out val);
            m_batchType = (MainResourceDefs.batchType)val;

            reader.Read(out m_batchVolume);
            bools = new BoolVector32(reader);

            reader.Read(out string m_currentProductSetup);
            reader.Read(out string m_currentSetupCode);
            reader.Read(out decimal m_currentSetupNumber);
            reader.Read(out bool m_currentProductSetupSet);
            reader.Read(out bool currentSetupCodeSet);
            reader.Read(out bool m_currentSetupNumberSet);
            reader.Read(out m_minNbrOfPeople);
            reader.Read(out m_minNbrOfPeopleSet);
        }
        #endregion 12101

        /*
         * This block below is here because 12100 is when we jumped VersionNumber in 12.1
         * As a result, 12100 is technically the same as 12058, but if this
         * else if wasn't here, then it'd get processed like 12070, which is 12.0's de-serialization.
         *
         * Note: This block should be equivalent to the 12058 block as a result of this too.
         */
        else if (reader.VersionNumber >= 12100)
        {
            reader.Read(out overtimeHourlyCost);
            reader.Read(out m_canOffload);
            reader.Read(out m_canPreemptMaterials);
            reader.Read(out canPreemptPredecessors);
            reader.Read(out canWorkOvertime);
            reader.Read(out int val);
            m_capacityType = (InternalResourceDefs.capacityTypes)val;
            reader.Read(out consecutiveSetupTimes);
            reader.Read(out cycleEfficiencyMultiplier);
            reader.Read(out string cycleSpanAlgorithm);
            reader.Read(out departmentExternalId);
            reader.Read(out bool drum);
            reader.Read(out plantExternalId);
            reader.Read(out m_discontinueSameCellScheduling);
            reader.Read(out decimal obsolete_sameSetupHeadstartMultiplier);
            reader.Read(out m_scheduledRunSpanAlgorithm);
            reader.Read(out m_scheduledSetupSpanAlgorithm);
            reader.Read(out scheduledTransferSpanAlgorithm);
            reader.Read(out m_activitySetupEfficiencyMultiplier);
            reader.Read(out val);
            if (val == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                UseSequencedSetupTime = true;
            }
            reader.Read(out stage);
            reader.Read(out m_standardHourlyCost);
            reader.Read(out m_useOperationSetupTime);
            reader.Read(out m_noDefaultRecurringCapacityInterval);

            //Was Set flags
            reader.Read(out overtimeHourlyCostSet);
            reader.Read(out m_canOffloadSet);
            reader.Read(out canPreemptMaterialsSet);
            reader.Read(out canPreemptPredecessorsSet);
            reader.Read(out canWorkOvertimeSet);
            reader.Read(out consecutiveSetupTimesSet);
            reader.Read(out cycleEfficiencyMultiplierSet);
            reader.Read(out bool cycleSpanAlgorithmSet);
            reader.Read(out bool drumSet);
            reader.Read(out m_discontinueSameCellSchedulingSet);
            reader.Read(out bool obsolete_sameSetupHeadstartMultiplierSet);
            reader.Read(out m_scheduledRunSpanAlgorithmSet);
            reader.Read(out m_scheduledSetupSpanAlgorithmSet);
            reader.Read(out m_scheduledTransferSpanAlgorithmSet);
            reader.Read(out m_activitySetupEfficiencyMultiplierSet);
            reader.Read(out bool setupIncludedSet);
            reader.Read(out m_stageSet);
            reader.Read(out m_standardHourlyCostSet);
            reader.Read(out m_useOperationSetupTimeSet);
            reader.Read(out m_bufferSpanSet);
            reader.Read(out headStartSpanSet);
            reader.Read(out bool maxSameSetupSpanSet);
            reader.Read(out m_setupSpanSet);
            reader.Read(out transferSpanSet);
            reader.Read(out m_batchTypeSet);
            reader.Read(out m_batchVolumeSet);
            //End Was Set flags

            reader.Read(out bufferSpan);
            reader.Read(out headStartSpan);
            reader.Read(out TimeSpan maxSameSetupSpan);
            reader.Read(out setupSpan);
            reader.Read(out transferSpan);
            reader.Read(out m_autoJoinSpan);

            reader.Read(out val);
            m_batchType = (MainResourceDefs.batchType)val;

            reader.Read(out m_batchVolume);
            bools = new BoolVector32(reader);

            reader.Read(out string m_currentProductSetup);
            reader.Read(out string m_currentSetupCode);
            reader.Read(out decimal m_currentSetupNumber);
            reader.Read(out bool m_currentProductSetupSet);
            reader.Read(out bool currentSetupCodeSet);
            reader.Read(out bool m_currentSetupNumberSet);
            reader.Read(out m_minNbrOfPeople);
            reader.Read(out m_minNbrOfPeopleSet);
        }

        #region 12070
        // This region is for compatibility with 12.0. 
        // It should be the same as the 12038 de-serialization
        else if (reader.VersionNumber >= 12070)
        {
            reader.Read(out overtimeHourlyCost);
            reader.Read(out m_canOffload);
            reader.Read(out m_canPreemptMaterials);
            reader.Read(out canPreemptPredecessors);
            reader.Read(out canWorkOvertime);
            reader.Read(out int val);
            m_capacityType = (InternalResourceDefs.capacityTypes)val;
            reader.Read(out consecutiveSetupTimes);
            reader.Read(out cycleEfficiencyMultiplier);
            reader.Read(out string cycleSpanAlgorithm);
            reader.Read(out departmentExternalId);
            reader.Read(out bool drum);
            reader.Read(out bool _durationDependsOnCapacity);
            reader.Read(out plantExternalId);
            reader.Read(out val); // Use to be converted into processStatus
            reader.Read(out m_discontinueSameCellScheduling);
            reader.Read(out decimal obsolete_sameSetupHeadstartMultiplier);
            reader.Read(out m_scheduledRunSpanAlgorithm);
            reader.Read(out m_scheduledSetupSpanAlgorithm);
            reader.Read(out scheduledTransferSpanAlgorithm);
            reader.Read(out m_activitySetupEfficiencyMultiplier);
            reader.Read(out val);
            if (val == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                UseSequencedSetupTime = true;
            }
            reader.Read(out stage);
            reader.Read(out m_standardHourlyCost);
            reader.Read(out m_useOperationSetupTime);
            reader.Read(out m_noDefaultRecurringCapacityInterval);

            //Was Set flags
            reader.Read(out overtimeHourlyCostSet);
            reader.Read(out m_canOffloadSet);
            reader.Read(out canPreemptMaterialsSet);
            reader.Read(out canPreemptPredecessorsSet);
            reader.Read(out canWorkOvertimeSet);
            reader.Read(out consecutiveSetupTimesSet);
            reader.Read(out cycleEfficiencyMultiplierSet);
            reader.Read(out bool cycleSpanAlgorithmSet);
            reader.Read(out bool drumSet);
            reader.Read(out bool _durationDependsOnCapacitySet);
            reader.Read(out bool discard_m_processingStatusSet);
            reader.Read(out m_discontinueSameCellSchedulingSet);
            reader.Read(out bool obsolete_sameSetupHeadstartMultiplierSet);
            reader.Read(out m_scheduledRunSpanAlgorithmSet);
            reader.Read(out m_scheduledSetupSpanAlgorithmSet);
            reader.Read(out m_scheduledTransferSpanAlgorithmSet);
            reader.Read(out m_activitySetupEfficiencyMultiplierSet);
            reader.Read(out bool setupIncludedSet);
            reader.Read(out m_stageSet);
            reader.Read(out m_standardHourlyCostSet);
            reader.Read(out m_useOperationSetupTimeSet);
            reader.Read(out m_bufferSpanSet);
            reader.Read(out headStartSpanSet);
            reader.Read(out bool maxSameSetupSpanSet);
            reader.Read(out m_setupSpanSet);
            reader.Read(out transferSpanSet);
            reader.Read(out m_batchTypeSet);
            reader.Read(out m_batchVolumeSet);
            //End Was Set flags

            reader.Read(out bufferSpan);
            reader.Read(out headStartSpan);
            reader.Read(out TimeSpan maxSameSetupSpan);
            reader.Read(out setupSpan);
            reader.Read(out transferSpan);
            reader.Read(out m_autoJoinSpan);

            reader.Read(out val);
            m_batchType = (MainResourceDefs.batchType)val;

            reader.Read(out m_batchVolume);
            bools = new BoolVector32(reader);

            reader.Read(out string m_currentProductSetup);
            reader.Read(out string m_currentSetupCode);
            reader.Read(out decimal m_currentSetupNumber);
            reader.Read(out bool m_currentProductSetupSet);
            reader.Read(out bool currentSetupCodeSet);
            reader.Read(out bool m_currentSetupNumberSet);
            reader.Read(out m_minNbrOfPeople);
            reader.Read(out m_minNbrOfPeopleSet);
        }
        #endregion 12070

        #region 12058
        else if (reader.VersionNumber >= 12058)
        {
            reader.Read(out overtimeHourlyCost);
            reader.Read(out m_canOffload);
            reader.Read(out m_canPreemptMaterials);
            reader.Read(out canPreemptPredecessors);
            reader.Read(out canWorkOvertime);
            reader.Read(out int val);
            m_capacityType = (InternalResourceDefs.capacityTypes)val;
            reader.Read(out consecutiveSetupTimes);
            reader.Read(out cycleEfficiencyMultiplier);
            reader.Read(out string cycleSpanAlgorithm);
            reader.Read(out departmentExternalId);
            reader.Read(out bool drum);
            reader.Read(out plantExternalId);
            reader.Read(out m_discontinueSameCellScheduling);
            reader.Read(out decimal obsolete_sameSetupHeadstartMultiplier);
            reader.Read(out m_scheduledRunSpanAlgorithm);
            reader.Read(out m_scheduledSetupSpanAlgorithm);
            reader.Read(out scheduledTransferSpanAlgorithm);
            reader.Read(out m_activitySetupEfficiencyMultiplier);
            reader.Read(out val);
            if (val == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                UseSequencedSetupTime = true;
            }
            reader.Read(out stage);
            reader.Read(out m_standardHourlyCost);
            reader.Read(out m_useOperationSetupTime);
            reader.Read(out m_noDefaultRecurringCapacityInterval);

            //Was Set flags
            reader.Read(out overtimeHourlyCostSet);
            reader.Read(out m_canOffloadSet);
            reader.Read(out canPreemptMaterialsSet);
            reader.Read(out canPreemptPredecessorsSet);
            reader.Read(out canWorkOvertimeSet);
            reader.Read(out consecutiveSetupTimesSet);
            reader.Read(out cycleEfficiencyMultiplierSet);
            reader.Read(out bool cycleSpanAlgorithmSet);
            reader.Read(out bool drumSet);
            reader.Read(out m_discontinueSameCellSchedulingSet);
            reader.Read(out bool obsolete_sameSetupHeadstartMultiplierSet);
            reader.Read(out m_scheduledRunSpanAlgorithmSet);
            reader.Read(out m_scheduledSetupSpanAlgorithmSet);
            reader.Read(out m_scheduledTransferSpanAlgorithmSet);
            reader.Read(out m_activitySetupEfficiencyMultiplierSet);
            reader.Read(out bool setupIncludedSet);
            reader.Read(out m_stageSet);
            reader.Read(out m_standardHourlyCostSet);
            reader.Read(out m_useOperationSetupTimeSet);
            reader.Read(out m_bufferSpanSet);
            reader.Read(out headStartSpanSet);
            reader.Read(out bool maxSameSetupSpanSet);
            reader.Read(out m_setupSpanSet);
            reader.Read(out transferSpanSet);
            reader.Read(out m_batchTypeSet);
            reader.Read(out m_batchVolumeSet);
            //End Was Set flags

            reader.Read(out bufferSpan);
            reader.Read(out headStartSpan);
            reader.Read(out TimeSpan maxSameSetupSpan);
            reader.Read(out setupSpan);
            reader.Read(out transferSpan);
            reader.Read(out m_autoJoinSpan);

            reader.Read(out val);
            m_batchType = (MainResourceDefs.batchType)val;

            reader.Read(out m_batchVolume);
            bools = new BoolVector32(reader);

            reader.Read(out string m_currentProductSetup);
            reader.Read(out string m_currentSetupCode);
            reader.Read(out decimal m_currentSetupNumber);
            reader.Read(out bool m_currentProductSetupSet);
            reader.Read(out bool currentSetupCodeSet);
            reader.Read(out bool m_currentSetupNumberSet);
            reader.Read(out m_minNbrOfPeople);
            reader.Read(out m_minNbrOfPeopleSet);
        }
        #endregion 12058

        #region 12038
        else if (reader.VersionNumber >= 12038)
        {
            reader.Read(out overtimeHourlyCost);
            reader.Read(out m_canOffload);
            reader.Read(out m_canPreemptMaterials);
            reader.Read(out canPreemptPredecessors);
            reader.Read(out canWorkOvertime);
            reader.Read(out int val);
            m_capacityType = (InternalResourceDefs.capacityTypes)val;
            reader.Read(out consecutiveSetupTimes);
            reader.Read(out cycleEfficiencyMultiplier);
            reader.Read(out string cycleSpanAlgorithm);
            reader.Read(out departmentExternalId);
            reader.Read(out bool drum);
            reader.Read(out bool _durationDependsOnCapacity);
            reader.Read(out plantExternalId);
            reader.Read(out val); // Use to be converted into processStatus
            reader.Read(out m_discontinueSameCellScheduling);
            reader.Read(out decimal obsolete_sameSetupHeadstartMultiplier);
            reader.Read(out m_scheduledRunSpanAlgorithm);
            reader.Read(out m_scheduledSetupSpanAlgorithm);
            reader.Read(out scheduledTransferSpanAlgorithm);
            reader.Read(out m_activitySetupEfficiencyMultiplier);
            reader.Read(out val);
            if (val == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                UseSequencedSetupTime = true;
            }
            reader.Read(out stage);
            reader.Read(out m_standardHourlyCost);
            reader.Read(out m_useOperationSetupTime);
            reader.Read(out m_noDefaultRecurringCapacityInterval);

            //Was Set flags
            reader.Read(out overtimeHourlyCostSet);
            reader.Read(out m_canOffloadSet);
            reader.Read(out canPreemptMaterialsSet);
            reader.Read(out canPreemptPredecessorsSet);
            reader.Read(out canWorkOvertimeSet);
            reader.Read(out consecutiveSetupTimesSet);
            reader.Read(out cycleEfficiencyMultiplierSet);
            reader.Read(out bool cycleSpanAlgorithmSet);
            reader.Read(out bool drumSet);
            reader.Read(out bool _durationDependsOnCapacitySet);
            reader.Read(out bool discard_m_processingStatusSet);
            reader.Read(out m_discontinueSameCellSchedulingSet);
            reader.Read(out bool obsolete_sameSetupHeadstartMultiplierSet);
            reader.Read(out m_scheduledRunSpanAlgorithmSet);
            reader.Read(out m_scheduledSetupSpanAlgorithmSet);
            reader.Read(out m_scheduledTransferSpanAlgorithmSet);
            reader.Read(out m_activitySetupEfficiencyMultiplierSet);
            reader.Read(out bool setupIncludedSet);
            reader.Read(out m_stageSet);
            reader.Read(out m_standardHourlyCostSet);
            reader.Read(out m_useOperationSetupTimeSet);
            reader.Read(out m_bufferSpanSet);
            reader.Read(out headStartSpanSet);
            reader.Read(out TimeSpan maxSameSetupSpan);
            reader.Read(out m_setupSpanSet);
            reader.Read(out transferSpanSet);
            reader.Read(out m_batchTypeSet);
            reader.Read(out m_batchVolumeSet);
            //End Was Set flags

            reader.Read(out bufferSpan);
            reader.Read(out headStartSpan);
            reader.Read(out maxSameSetupSpan);
            reader.Read(out setupSpan);
            reader.Read(out transferSpan);
            reader.Read(out m_autoJoinSpan);

            reader.Read(out val);
            m_batchType = (MainResourceDefs.batchType)val;

            reader.Read(out m_batchVolume);
            bools = new BoolVector32(reader);

            reader.Read(out string m_currentProductSetup);
            reader.Read(out string m_currentSetupCode);
            reader.Read(out decimal m_currentSetupNumber);
            reader.Read(out bool m_currentProductSetupSet);
            reader.Read(out bool currentSetupCodeSet);
            reader.Read(out bool m_currentSetupNumberSet);
            reader.Read(out m_minNbrOfPeople);
            reader.Read(out m_minNbrOfPeopleSet);
        }
        #endregion 12038

        #region 683
        else if (reader.VersionNumber >= 683)
        {
            reader.Read(out overtimeHourlyCost);
            reader.Read(out m_canOffload);
            reader.Read(out m_canPreemptMaterials);
            reader.Read(out canPreemptPredecessors);
            reader.Read(out canWorkOvertime);
            reader.Read(out int val);
            m_capacityType = (InternalResourceDefs.capacityTypes)val;
            reader.Read(out consecutiveSetupTimes);
            reader.Read(out cycleEfficiencyMultiplier);
            reader.Read(out string cycleSpanAlgorithm);
            reader.Read(out departmentExternalId);
            reader.Read(out bool drum);
            reader.Read(out bool _durationDependsOnCapacity);
            reader.Read(out val); //overlap
            reader.Read(out plantExternalId);
            reader.Read(out val); // use to be for processingStatus
            reader.Read(out m_discontinueSameCellScheduling);
            reader.Read(out decimal obsolete_sameSetupHeadstartMultiplier);
            reader.Read(out m_scheduledRunSpanAlgorithm);
            reader.Read(out m_scheduledSetupSpanAlgorithm);
            reader.Read(out scheduledTransferSpanAlgorithm);
            reader.Read(out m_activitySetupEfficiencyMultiplier);
            reader.Read(out val);
            if (val == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                UseSequencedSetupTime = true;
            }
            reader.Read(out stage);
            reader.Read(out m_standardHourlyCost);
            reader.Read(out m_useOperationSetupTime);
            reader.Read(out val); //roles
            reader.Read(out m_noDefaultRecurringCapacityInterval);

            //Was Set flags
            reader.Read(out overtimeHourlyCostSet);
            reader.Read(out m_canOffloadSet);
            reader.Read(out canPreemptMaterialsSet);
            reader.Read(out canPreemptPredecessorsSet);
            reader.Read(out canWorkOvertimeSet);
            reader.Read(out consecutiveSetupTimesSet);
            reader.Read(out cycleEfficiencyMultiplierSet);
            reader.Read(out bool cycleSpanAlgorithmSet);
            reader.Read(out bool drumSet);
            reader.Read(out bool _durationDependsOnCapacitySet);
            reader.Read(out bool overlapSet);
            reader.Read(out bool discard_m_processingStatusSet);
            reader.Read(out m_discontinueSameCellSchedulingSet);
            reader.Read(out bool _obsolete_sameSetupHeadstartMultiplierSet);
            reader.Read(out m_scheduledRunSpanAlgorithmSet);
            reader.Read(out m_scheduledSetupSpanAlgorithmSet);
            reader.Read(out m_scheduledTransferSpanAlgorithmSet);
            reader.Read(out m_activitySetupEfficiencyMultiplierSet);
            reader.Read(out bool setupIncludedSet);
            reader.Read(out m_stageSet);
            reader.Read(out m_standardHourlyCostSet);
            reader.Read(out m_useOperationSetupTimeSet);
            reader.Read(out m_bufferSpanSet);
            reader.Read(out bool m_roleSet);
            reader.Read(out headStartSpanSet);
            reader.Read(out bool maxSameSetupSpanSet);
            reader.Read(out m_setupSpanSet);
            reader.Read(out transferSpanSet);
            reader.Read(out m_batchTypeSet);
            reader.Read(out m_batchVolumeSet);
            //End Was Set flags

            reader.Read(out bufferSpan);
            reader.Read(out headStartSpan);
            reader.Read(out TimeSpan maxSameSetupSpan);
            reader.Read(out setupSpan);
            reader.Read(out transferSpan);
            reader.Read(out m_autoJoinSpan);

            reader.Read(out val);
            m_batchType = (MainResourceDefs.batchType)val;

            reader.Read(out m_batchVolume);
            bools = new BoolVector32(reader);

            reader.Read(out string m_currentProductSetup);
            reader.Read(out string m_currentSetupCode);
            reader.Read(out decimal m_currentSetupNumber);
            reader.Read(out bool m_currentProductSetupSet);
            reader.Read(out bool currentSetupCodeSet);
            reader.Read(out bool m_currentSetupNumberSet);
            reader.Read(out m_minNbrOfPeople);
            reader.Read(out m_minNbrOfPeopleSet);
        }
        #endregion 683
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(overtimeHourlyCost);
        writer.Write(m_canOffload);
        writer.Write(m_canPreemptMaterials);
        writer.Write(canPreemptPredecessors);
        writer.Write(canWorkOvertime);
        writer.Write((int)m_capacityType);
        writer.Write(consecutiveSetupTimes);
        writer.Write(cycleEfficiencyMultiplier);
        writer.Write(departmentExternalId);
        writer.Write(plantExternalId);
        writer.Write(m_discontinueSameCellScheduling);
        writer.Write(m_scheduledRunSpanAlgorithm);
        writer.Write(m_scheduledSetupSpanAlgorithm);
        writer.Write(scheduledTransferSpanAlgorithm);
        writer.Write(m_activitySetupEfficiencyMultiplier);
        writer.Write(m_changeoverSetupEfficiencyMultiplier);
        writer.Write(stage);
        writer.Write(m_standardHourlyCost);
        writer.Write(m_useOperationSetupTime);
        writer.Write(m_noDefaultRecurringCapacityInterval);

        //Was set flags
        writer.Write(overtimeHourlyCostSet);
        writer.Write(m_canOffloadSet);
        writer.Write(canPreemptMaterialsSet);
        writer.Write(canPreemptPredecessorsSet);
        writer.Write(canWorkOvertimeSet);
        writer.Write(consecutiveSetupTimesSet);
        writer.Write(cycleEfficiencyMultiplierSet);
        writer.Write(m_discontinueSameCellSchedulingSet);
        writer.Write(m_scheduledRunSpanAlgorithmSet);
        writer.Write(m_scheduledSetupSpanAlgorithmSet);
        writer.Write(m_scheduledTransferSpanAlgorithmSet);
        writer.Write(m_activitySetupEfficiencyMultiplierSet);
        writer.Write(m_stageSet);
        writer.Write(m_standardHourlyCostSet);
        writer.Write(m_useOperationSetupTimeSet);
        writer.Write(m_bufferSpanSet);
        writer.Write(headStartSpanSet);

        writer.Write(m_setupSpanSet);
        writer.Write(transferSpanSet);
        writer.Write(m_batchTypeSet);
        writer.Write(m_batchVolumeSet);


        writer.Write(bufferSpan);
        writer.Write(headStartSpan);
        writer.Write(setupSpan);
        writer.Write(transferSpan);

        writer.Write(m_autoJoinSpan);

        writer.Write((int)m_batchType);
        writer.Write(m_batchVolume);
        bools.Serialize(writer);

        writer.Write(m_minNbrOfPeople);
        writer.Write(m_minNbrOfPeopleSet);

        writer.Write(m_standardCleanSpan);
        writer.Write(m_standardCleanoutGrade);
        writer.Write(m_priority);
        writer.Write(m_resourceSetupCost);
        writer.Write(m_resourceCleanoutCost);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public InternalResource() { }

    protected InternalResource(string externalId, string name, string description, string notes, string userFields, string plantExternalId, string departmentExternalId)
        : base(externalId, name, description, notes, userFields)
    {
        this.plantExternalId = plantExternalId;
        this.departmentExternalId = departmentExternalId;
    }

    protected InternalResource(ResourceTDataSet.ResourceRow row)
        : base(row.ExternalId, row.Name, row.IsDescriptionNull() ? null : row.Description, row.IsNotesNull() ? null : row.Notes, row.IsUserFieldsNull() ? null : row.UserFields)
    {
        PlantExternalId = row.PlantExternalId;
        DepartmentExternalId = row.DepartmentExternalId;
        ExternalId = row.ExternalId;

        if (!row.IsCellExternalIdNull())
        {
            CellExternalId = row.CellExternalId;
        }

        if (!row.IsBufferSpanHrsNull())
        {
            BufferSpan = TimeSpan.FromHours(row.BufferSpanHrs);
        }

        try
        {
            if (!row.IsCapacityTypeNull())
            {
                CapacityType = (InternalResourceDefs.capacityTypes)Enum.Parse(typeof(InternalResourceDefs.capacityTypes), row.CapacityType);
            }
        }
        catch (Exception err)
        {
            throw new APSCommon.PTValidationException("2854",
                err,
                false,
                new object[]
                {
                    row.CapacityType, "Resource", "CapacityType",
                    string.Join(", ", Enum.GetNames(typeof(InternalResourceDefs.capacityTypes)))
                });
        }

        if (!row.IsConsecutiveSetupTimesNull())
        {
            ConsecutiveSetupTimes = row.ConsecutiveSetupTimes;
        }

        if (!row.IsCycleEfficiencyMultiplierNull())
        {
            CycleEfficiencyMultiplier = row.CycleEfficiencyMultiplier;
        }

        if (!row.IsDescriptionNull())
        {
            Description = row.Description;
        }

        if (!row.IsDisallowDragAndDropsNull())
        {
            DisallowDragAndDrops = row.DisallowDragAndDrops;
        }

        if (!row.IsDiscontinueSameCellSchedulingNull())
        {
            DiscontinueSameCellScheduling = row.DiscontinueSameCellScheduling;
        }

        if (!row.IsGanttRowHeightFactorNull())
        {
            GanttRowHeightFactor = row.GanttRowHeightFactor;
        }

        if (!row.IsHeadStartHrsNull())
        {
            HeadStartSpan = TimeSpan.FromHours(row.HeadStartHrs);
        }

        if (!row.IsImageFileNameNull())
        {
            ImageFileName = row.ImageFileName;
        }

        if (!row.IsNoDefaultRecurringCapacityIntervalNull())
        {
            NoDefaultRecurringCapacityInterval = row.NoDefaultRecurringCapacityInterval;
        }

        if (!row.IsNotesNull())
        {
            Notes = row.Notes;
        }

        if (!row.IsOvertimeHourlyCostNull())
        {
            OvertimeHourlyCost = row.OvertimeHourlyCost;
        }

        if (!row.IsResourceTypeNull())
        {
            try
            {
                ResourceType = (BaseResourceDefs.resourceTypes)Enum.Parse(typeof(BaseResourceDefs.resourceTypes), row.ResourceType);
            }
            catch (Exception err)
            {
                throw new APSCommon.PTValidationException("2854",
                    err,
                    false,
                    new object[]
                    {
                        row.ResourceType, "Resource", "ResourceType",
                        string.Join(", ", Enum.GetNames(typeof(BaseResourceDefs.resourceTypes)))
                    });
            }
        }

        if (!row.IsScheduledRunSpanAlgorithmNull())
        {
            ScheduledRunSpanAlgorithm = row.ScheduledRunSpanAlgorithm;
        }

        if (!row.IsScheduledSetupSpanAlgorithmNull())
        {
            ScheduledSetupSpanAlgorithm = row.ScheduledSetupSpanAlgorithm;
        }

        if (!row.IsScheduledTransferSpanAlgorithmNull())
        {
            ScheduledTransferSpanAlgorithm = row.ScheduledTransferSpanAlgorithm;
        }

        if (!row.IsActivitySetupEfficiencyMultiplierNull())
        {
            ActivitySetupEfficiencyMultiplier = row.ActivitySetupEfficiencyMultiplier;
        }

        if (!row.IsChangeoverSetupEfficiencyMultiplierNull())
        {
            ChangeoverSetupEfficiencyMultiplier = row.ChangeoverSetupEfficiencyMultiplier;
        }

        if (!row.IsSetupHrsNull())
        {
            SetupSpan = TimeSpan.FromHours(row.SetupHrs);
        }

        if (!row.IsStageNull())
        {
            Stage = row.Stage;
        }

        if (!row.IsStandardHourlyCostNull())
        {
            StandardHourlyCost = row.StandardHourlyCost;
        }

        if (!row.IsTransferHrsNull())
        {
            TransferSpan = TimeSpan.FromHours(row.TransferHrs);
        }

        if (!row.IsUseResourceSetupTimeNull())
        {
            UseResourceSetupTime = row.UseResourceSetupTime;
        }

        if (!row.IsUseOperationSetupTimeNull())
        {
            UseOperationSetupTime = row.UseOperationSetupTime;
        }

        if (!row.IsUseSequencedSetupTimeNull())
        {
            UseSequencedSetupTime = row.UseSequencedSetupTime;
        }

        if (!row.IsWorkcenterExternalIdNull())
        {
            WorkcenterExternalId = row.WorkcenterExternalId;
        }

        if (!row.IsWorkcenterNull())
        {
            Workcenter = row.Workcenter;
        }

        if (!row.IsExcludeFromGanttsNull())
        {
            ExcludeFromGantts = row.ExcludeFromGantts;
        }

        if (!row.IsManualAssignmentOnlyNull())
        {
            ManualAssignmentOnly = row.ManualAssignmentOnly;
        }

        if (!row.IsIsTankNull())
        {
            IsTank = row.IsTank;
        }

        if (!row.IsNormalSequencingPlanNull())
        {
            NormalSequencingPlanExternalId = row.NormalSequencingPlan;
        }

        if (!row.IsExperimentalSequencingPlanNull())
        {
            ExperimentalSequencingPlanOneExternalId = row.ExperimentalSequencingPlan;
        }

        if (!row.IsExperimentalSequencingPlanTwoNull())
        {
            ExperimentalSequencingPlanTwoExternalId = row.ExperimentalSequencingPlanTwo;
        }

        if (!row.IsExperimentalSequencingPlanThreeNull())
        {
            ExperimentalSequencingPlanThreeExternalId = row.ExperimentalSequencingPlanThree;
        }

        if (!row.IsExperimentalSequencingPlanFourNull())
        {
            ExperimentalSequencingPlanFourExternalId = row.ExperimentalSequencingPlanFour;
        }

        if (!row.IsMinNbrOfPeopleNull())
        {
            MinNbrOfPeople = row.MinNbrOfPeople;
        }

        if (!row.IsBatchTypeNull())
        {
            try
            {
                BatchType = (MainResourceDefs.batchType)Enum.Parse(typeof(MainResourceDefs.batchType), row.BatchType);
            }
            catch (Exception err)
            {
                throw new APSCommon.PTValidationException("2854",
                    err,
                    false,
                    new object[]
                    {
                        row.BatchType, "Resource", "BatchType",
                        string.Join(", ", Enum.GetNames(typeof(MainResourceDefs.batchType)))
                    });
            }
        }

        if (!row.IsBatchVolumeNull())
        {
            BatchVolume = row.BatchVolume;
        }

        if (!row.IsUserFieldsNull())
        {
            SetUserFields(row.UserFields);
        }

        if (!row.IsAutoJoinHrsNull())
        {
            AutoJoinSpan = TimeSpan.FromHours(row.AutoJoinHrs);
        }

        if (!row.IsOmitSetupOnFirstActivityNull())
        {
            OmitSetupOnFirstActivity = row.OmitSetupOnFirstActivity;
        }

        if (!row.IsOmitSetupOnFirstActivityInShiftNull())
        {
            OmitSetupOnFirstActivityInShift = row.OmitSetupOnFirstActivityInShift;
        }

        if (!row.IsActiveNull())
        {
            Active = row.Active;
        }

        if (!row.IsStandardCleanHoursNull())
        {
            StandardCleanSpan = PTDateTime.GetSafeTimeSpan(row.StandardCleanHours);
        }

        if (!row.IsStandardCleanoutGradeNull())
        {
            StandardCleanoutGrade = row.StandardCleanoutGrade;
        }

        if (!row.IsUseResourceCleanoutNull())
        {
            UseResourceCleanout = row.UseResourceCleanout;
        }

        if (!row.IsUseOperationCleanoutNull())
        {
            UseOperationCleanout = row.UseOperationCleanout;
        }

        if (!row.IsUseOperationCleanoutNull())
        {
            UseOperationCleanout = row.UseOperationCleanout;
        }

        if (!row.IsUseAttributeCleanoutsNull())
        {
            UseAttributeCleanouts = row.UseAttributeCleanouts;
        }

        if (!row.IsPriorityNull())
        {
            Priority = row.Priority;
        }

        if (!row.IsResourceSetupCostNull())
        {
            ResourceSetupCost = row.ResourceSetupCost;
        }

        if (!row.IsResourceCleanoutCostNull())
        {
            ResourceCleanoutCost = row.ResourceCleanoutCost;
        }
    }

    #region BoolVector
    private BoolVector32 bools;
    private const int IsTankIdx = 0;
    private const int IsTankSetIdx = 1;
    private const int omitSetupOnFirstActivityIdx = 2;
    private const int omitSetupOnFirstActivitySetIdx = 3;
    private const int omitSetupOnFirstActivityInShiftIdx = 4;
    private const int omitSetupOnFirstActivityInShiftSetIdx = 5;
    private const int c_autoJoinSpanSetIdx = 6;
    private const short c_capacityTypeSetIdx = 7;
    private const short c_limitAutoJoinToSameCapacityIntervalIdx = 8;
    private const short c_limitAutoJoinToSameCapacityIntervalSetIdx = 9;
    private const short c_useOperationCleanoutsIdx = 10;
    private const short c_useOperationCleanoutsIsSetIdx = 11;
    private const short c_useAttributeCleanoutsIdx = 12;
    private const short c_useAttributeCleanoutsIsSetIdx = 13;
    private const short c_standardCleanSpanIsSetIdx = 14;
    private const short c_standardCleanoutGradeIsSetIdx = 15;
    private const short c_lastActivityKeyIsSetIdx = 16;
    private const short c_priorityIsSetIdx = 17;
    private const short c_useResourceSetupTimeIdx = 18;
    private const short c_useSequencedSetupTimeIdx = 19;
    private const short c_useResourceCleanoutIdx = 20;
    private const short c_useResourceSetupTimeIsSetIdx = 21;
    private const short c_useSequencedSetupTimeIsSetIdx = 22;
    private const short c_useResourceCleanoutIsSetIdx = 23;
    private const short c_resourceSetupCostIsSetIdx = 24;
    private const short c_resourceCleanoutCostIsSetIdx = 25;
    #endregion BoolVector

    /// <summary>
    /// This value is only used by ERP Link to construct Machines.
    /// </summary>
    public string plantExternalId;

    [Required(true)]
    /// <summary>
    /// Indicates the parent Plant.
    /// </summary>
    public string PlantExternalId
    {
        get => plantExternalId;
        set => plantExternalId = value;
    }

    /// <summary>
    /// This value is only used by ERP Link to construct Machines.
    /// </summary>
    public string departmentExternalId;

    [Required(true)]
    /// <summary>
    /// Indicates the parent Department.
    /// </summary>
    public string DepartmentExternalId
    {
        get => departmentExternalId;
        set => departmentExternalId = value;
    }

    #region Shared Properties
    private TimeSpan bufferSpan = new(0);

    /// <summary>
    /// Used by the Drum-Buffer-Rope Release rule to prevent starving of this resource thus keeping its utilization high.
    /// </summary>
    public TimeSpan BufferSpan
    {
        get => bufferSpan;
        set
        {
            bufferSpan = value;
            m_bufferSpanSet = true;
        }
    }

    private bool m_bufferSpanSet;
    public bool BufferSpanSet => m_bufferSpanSet;

    private bool m_canPreemptMaterials;

    /// <summary>
    /// Whether the Resource can drive the schedule by requesting that Material Requirments be delivered sooner than they are currently scheduled to start by ignoring them as constraints. This does NOT allow
    /// preemption of ConfirmedConstraints.  See also: BaseOperation.IsConfirmedConstraint and MaterialRequirment.IsConfirmedConstraint. Only allowed outside the Scenario.KeepFeasibleSpan.
    /// </summary>
    public bool CanPreemptMaterials
    {
        get => m_canPreemptMaterials;
        set
        {
            m_canPreemptMaterials = value;
            canPreemptMaterialsSet = true;
        }
    }

    private bool canPreemptMaterialsSet;
    public bool CanPreemptMaterialsSet => canPreemptMaterialsSet;

    private bool canPreemptPredecessors;

    /// <summary>
    /// Whether the Resource can drive the schedule by requesting that predecessor Operations start sooner than they are currently scheduled to start by ignoring them as constraints. This does NOT allow
    /// preemption of ConfirmedConstraints.  See also: BaseOperation.IsConfirmedConstraint and MaterialRequirment.IsConfirmedConstraint. This does NOT allow scheduling before the c_EarliestPossibleFinish of
    /// an Operation. Only allowed outside the Scenario.KeepFeasibleSpan.
    /// </summary>
    public bool CanPreemptPredecessors
    {
        get => canPreemptPredecessors;
        set
        {
            canPreemptPredecessors = value;
            canPreemptPredecessorsSet = true;
        }
    }

    private bool canPreemptPredecessorsSet;
    public bool CanPreemptPredecessorsSet => canPreemptPredecessorsSet;

    private bool canWorkOvertime;

    /// <summary>
    /// Will cause suggestions for overtime to be generated for late activities where adding the overtime would cause the activity to complete earlier.
    /// </summary>
    public bool CanWorkOvertime
    {
        get => canWorkOvertime;
        set
        {
            canWorkOvertime = value;
            canWorkOvertimeSet = true;
        }
    }

    private bool canWorkOvertimeSet;
    public bool CanWorkOvertimeSet => canWorkOvertimeSet;

    private bool consecutiveSetupTimes = true;

    /// <summary>
    /// Controls whether Resource and Operation setup times are consecutive (added) or concurrent (the maximum is used).
    /// </summary>
    public bool ConsecutiveSetupTimes
    {
        get => consecutiveSetupTimes;
        set
        {
            consecutiveSetupTimes = value;
            consecutiveSetupTimesSet = true;
        }
    }

    private bool consecutiveSetupTimesSet;
    public bool ConsecutiveSetupTimesSet => consecutiveSetupTimesSet;

    private decimal cycleEfficiencyMultiplier;

    /// <summary>
    /// Multiplies run time to adjust for slower/faster Resources.
    /// </summary>
    public decimal CycleEfficiencyMultiplier
    {
        get => cycleEfficiencyMultiplier;
        set
        {
            cycleEfficiencyMultiplier = value;
            cycleEfficiencyMultiplierSet = true;
        }
    }

    private bool cycleEfficiencyMultiplierSet;
    public bool CycleEfficiencyMultiplierSet => cycleEfficiencyMultiplierSet;

    private TimeSpan headStartSpan = new(7, 0, 0, 0);

    /// <summary>
    /// During Optimizations, Activities are not permitted to start before  JitStartDate-(SameSetupHeadstartMultiplier*setupSpan+HeadStartSpan).  This can be used to prevent excessive inventory buildup and
    /// aid in grouping same-SetupCode Activities.
    /// </summary>
    public TimeSpan HeadStartSpan
    {
        get => headStartSpan;
        set
        {
            headStartSpan = value;
            headStartSpanSet = true;
        }
    }

    private bool headStartSpanSet;
    public bool HeadStartSpanSet => headStartSpanSet;

    private decimal overtimeHourlyCost;

    /// <summary>
    /// Can be used for calculating schedule quality and in scripts.
    /// </summary>
    public decimal OvertimeHourlyCost
    {
        get => overtimeHourlyCost;
        set
        {
            overtimeHourlyCost = value;
            overtimeHourlyCostSet = true;
        }
    }

    private bool overtimeHourlyCostSet;
    public bool OvertimeHourlyCostSet => overtimeHourlyCostSet;

    private string m_scheduledRunSpanAlgorithm = "";

    /// <summary>
    /// Name of the script used to calculate ScheduledRunSpan in Activities on this resource.  If specified, this gives the absolute final value for ScheduledRunSpan.  Result must be non-negative double or 1
    /// will be substituted and an error will be logged.
    /// </summary>
    public string ScheduledRunSpanAlgorithm
    {
        get => m_scheduledRunSpanAlgorithm;
        set
        {
            m_scheduledRunSpanAlgorithm = value;
            m_scheduledRunSpanAlgorithmSet = true;
        }
    }

    private bool m_scheduledRunSpanAlgorithmSet;
    public bool ScheduledRunSpanAlgorithmSet => m_scheduledRunSpanAlgorithmSet;

    private string m_scheduledSetupSpanAlgorithm = "";

    /// <summary>
    /// Name of the script used to calculate ScheduledSetupSpan in Activities on this resource.  If specified, this gives the absolute final value for ScheduledSetupSpan.  Result must be non-negative double
    /// or zero will be substituted and an error will be logged.
    /// </summary>
    public string ScheduledSetupSpanAlgorithm
    {
        get => m_scheduledSetupSpanAlgorithm;
        set
        {
            m_scheduledSetupSpanAlgorithm = value;
            m_scheduledSetupSpanAlgorithmSet = true;
        }
    }

    private bool m_scheduledSetupSpanAlgorithmSet;
    public bool ScheduledSetupSpanAlgorithmSet => m_scheduledSetupSpanAlgorithmSet;

    private string scheduledTransferSpanAlgorithm = "";

    /// <summary>
    /// Name of the script used to calculate ScheduledTransferSpan in Activities on this resource.  If specified, this gives the absolute final value for ScheduledTransferSpan.  Result must be non-negative
    /// double or zero will be substituted and an error will be logged.
    /// </summary>
    public string ScheduledTransferSpanAlgorithm
    {
        get => scheduledTransferSpanAlgorithm;
        set
        {
            scheduledTransferSpanAlgorithm = value;
            m_scheduledTransferSpanAlgorithmSet = true;
        }
    }

    private bool m_scheduledTransferSpanAlgorithmSet;
    public bool ScheduledTransferSpanAlgorithmSet => m_scheduledTransferSpanAlgorithmSet;

    private bool m_discontinueSameCellScheduling;

    /// <summary>
    /// If set then same cell scheduling for predecessor operations is discontinued. But if this resource is part of a cell then the behavior is as usual and this resource's cell will become the new cell to
    /// try to schedule in.
    /// </summary>
    public bool DiscontinueSameCellScheduling
    {
        get => m_discontinueSameCellScheduling;
        set
        {
            m_discontinueSameCellScheduling = value;
            m_discontinueSameCellSchedulingSet = true;
        }
    }

    private bool m_discontinueSameCellSchedulingSet;
    public bool DiscontinueSameCellSchedulingSet => m_discontinueSameCellSchedulingSet;

    private int stage;

    /// <summary>
    /// Used for multi-stage scheduling to allow scheduling of groups of resources stage by stage.
    /// </summary>
    public int Stage
    {
        get => stage;
        set
        {
            stage = value;
            m_stageSet = true;
        }
    }

    private bool m_stageSet;
    public bool StageSet => m_stageSet;


    private decimal m_activitySetupEfficiencyMultiplier = 1;
    /// <summary>
    /// Multiplies setup time to adjust for slower/faster Activity setups.
    /// </summary>
    public decimal ActivitySetupEfficiencyMultiplier
    {
        get => m_activitySetupEfficiencyMultiplier;
        set
        {
            m_activitySetupEfficiencyMultiplier = value;
            m_activitySetupEfficiencyMultiplierSet = true;
        }
    }

    private bool m_changeoverSetupEfficiencyMultiplierSet;
    public bool ChangeoverSetupEfficiencyMultiplierSet => m_changeoverSetupEfficiencyMultiplierSet;


    private decimal m_changeoverSetupEfficiencyMultiplier = 1;

    /// <summary>
    /// Multiplies setup time to adjust for slower/faster Changeover setups.
    /// </summary>
    public decimal ChangeoverSetupEfficiencyMultiplier
    {
        get => m_changeoverSetupEfficiencyMultiplier;
        set
        {
            m_changeoverSetupEfficiencyMultiplier = value;
            m_changeoverSetupEfficiencyMultiplierSet = true;
        }
    }

    private bool m_activitySetupEfficiencyMultiplierSet;
    public bool ActivitySetupEfficiencyMultiplierSet => m_activitySetupEfficiencyMultiplierSet;



    private TimeSpan transferSpan = new(0);

    /// <summary>
    /// Successor Operations cannot start before this time passes after finishing the Operation.  Resource is not consumed during this time.
    /// </summary>
    public TimeSpan TransferSpan
    {
        get => transferSpan;
        set
        {
            transferSpan = value;
            transferSpanSet = true;
        }
    }

    private bool transferSpanSet;
    public bool TransferSpanSet => transferSpanSet;

    private bool m_useOperationSetupTime = true;

    /// <summary>
    /// Whether to use the operation setup time.
    /// </summary>
    public bool UseOperationSetupTime
    {
        get => m_useOperationSetupTime;
        set
        {
            m_useOperationSetupTime = value;
            m_useOperationSetupTimeSet = true;
        }
    }

    private bool m_useOperationSetupTimeSet = true;
    public bool UseOperationSetupTimeSet => m_useOperationSetupTimeSet;

    /// <summary>
    /// If true then the Setup Time on the first Activity scheduled on the Resource is always zero.
    /// </summary>
    public bool OmitSetupOnFirstActivity
    {
        get => bools[omitSetupOnFirstActivityIdx];
        set
        {
            bools[omitSetupOnFirstActivityIdx] = value;
            bools[omitSetupOnFirstActivitySetIdx] = true;
        }
    }

    public bool OmitSetupOnFirstActivitySet => bools[omitSetupOnFirstActivitySetIdx];

    /// <summary>
    /// If true then the Setup Time on the first Activity scheduled on a Capacity Interval is always zero.
    /// </summary>
    public bool OmitSetupOnFirstActivityInShift
    {
        get => bools[omitSetupOnFirstActivityInShiftIdx];
        set
        {
            bools[omitSetupOnFirstActivityInShiftIdx] = value;
            bools[omitSetupOnFirstActivityInShiftSetIdx] = true;
        }
    }

    public bool OmitSetupOnFirstActivityInShiftSet => bools[omitSetupOnFirstActivityInShiftSetIdx];

    private bool m_canOffload = true;

    /// <summary>
    /// Whether Activities can be reassigned from this Resource during Optimizations.
    /// </summary>
    public bool CanOffload
    {
        get => m_canOffload;
        set
        {
            m_canOffload = value;
            m_canOffloadSet = true;
        }
    }

    private bool m_canOffloadSet;
    public bool CanOffloadSet => m_canOffloadSet;

    private InternalResourceDefs.capacityTypes m_capacityType = InternalResourceDefs.capacityTypes.SingleTasking;

    /// <summary>
    /// If SingleTasking then only one Activity can be performed on the Resource at any point in time.
    /// If Infinite then any number of can be performed simultaneously.
    /// If MultiTasking then Operations with Attention Percent less than 100% may be scheduled to run simultaneously (up to a total of 100% across
    /// all Operations that are running at any one point in time on the Resource).
    /// </summary>
    public InternalResourceDefs.capacityTypes CapacityType
    {
        get => m_capacityType;
        set
        {
            m_capacityType = value;
            bools[c_capacityTypeSetIdx] = true;
        }
    }

    public bool CapacityTypeSet => bools[c_capacityTypeSetIdx];

    private TimeSpan setupSpan = new(0);

    /// <summary>
    /// Time that must be spent to setup this resource whenever the preceding operation did not use this resource.
    /// </summary>
    public TimeSpan SetupSpan
    {
        get => setupSpan;
        set
        {
            setupSpan = value;
            m_setupSpanSet = true;
        }
    }

    private bool m_setupSpanSet;
    public bool SetupSpanSet => m_setupSpanSet;

    private decimal m_standardHourlyCost;

    /// <summary>
    /// Can be used for calculating schedule quality and in scripts.
    /// </summary>
    public decimal StandardHourlyCost
    {
        get => m_standardHourlyCost;
        set
        {
            m_standardHourlyCost = value;
            m_standardHourlyCostSet = true;
        }
    }

    private bool m_standardHourlyCostSet;
    public bool StandardHourlyCostSet => m_standardHourlyCostSet;

    public bool LimitAutoJoinToSameCapacityInterval
    {
        get => bools[c_limitAutoJoinToSameCapacityIntervalIdx];
        set
        {
            bools[c_limitAutoJoinToSameCapacityIntervalIdx] = value;
            bools[c_limitAutoJoinToSameCapacityIntervalSetIdx] = true;
        }
    }

    public bool LimitAutoJoinToSameCapacityIntervalSet => bools[c_limitAutoJoinToSameCapacityIntervalSetIdx];

    private TimeSpan m_autoJoinSpan;

    /// <summary>
    /// When AutoJoining, only Join for Operations that start before this amount of time after the Clock.
    /// </summary>
    public TimeSpan AutoJoinSpan
    {
        get => m_autoJoinSpan;
        set
        {
            m_autoJoinSpan = value;
            bools[c_autoJoinSpanSetIdx] = true;
        }
    }

    public bool AutoJoinSpanSet => bools[c_autoJoinSpanSetIdx];

    private MainResourceDefs.batchType m_batchType;

    public MainResourceDefs.batchType BatchType
    {
        get => m_batchType;
        set
        {
            m_batchType = value;
            m_batchTypeSet = true;
        }
    }

    private bool m_batchTypeSet;
    public bool BatchTypeSet => m_batchTypeSet;

    private decimal m_batchVolume;

    public decimal BatchVolume
    {
        get => m_batchVolume;
        set
        {
            m_batchVolume = value;
            m_batchVolumeSet = true;
        }
    }

    private bool m_batchVolumeSet;
    public bool BatchVolumeSet => m_batchVolumeSet;

    public bool IsTank
    {
        get => bools[IsTankIdx];
        set
        {
            bools[IsTankIdx] = value;
            bools[IsTankSetIdx] = true;
        }
    }

    public bool IsTankSet => bools[IsTankSetIdx];

    protected TimeSpan m_standardCleanSpan = new(0);

    /// <summary>
    /// Specifies a default TimeSpan for which the resource will be cleaned out after this operation.
    /// Can be overridden by the Operation or Activity's CleanSpan based on UseOperationCleanSpan/UseAttributeCleanSpan flags.
    /// </summary>
    public TimeSpan StandardCleanSpan
    {
        get => m_standardCleanSpan;
        set
        {
            m_standardCleanSpan = value;
            bools[c_standardCleanSpanIsSetIdx] = true;
        }
    }

    public bool StandardCleanSpanSet => bools[c_standardCleanSpanIsSetIdx];

    protected int m_standardCleanoutGrade;

    /// <summary>
    /// Whether to incur this attribute changeover as Clean or Setup
    /// </summary>
    public int StandardCleanoutGrade
    {
        get => m_standardCleanoutGrade;
        set
        {
            m_standardCleanoutGrade = value;
            bools[c_standardCleanoutGradeIsSetIdx] = true;
        }
    }

    public bool StandardCleanoutGradeSet => bools[c_standardCleanoutGradeIsSetIdx];

    /// <summary>
    /// Whether to use the InternalOperation's CleanSpan in cleanout calculations.
    /// </summary>
    public bool UseOperationCleanout
    {
        get => bools[c_useOperationCleanoutsIdx];
        set
        {
            bools[c_useOperationCleanoutsIdx] = value;
            bools[c_useOperationCleanoutsIsSetIdx] = true;
        }
    }

    public bool UseOperationCleanoutIsSet => bools[c_useOperationCleanoutsIsSetIdx];

    /// <summary>
    /// Whether to use Attributes' CleanSpans in cleanout calculations.
    /// </summary>
    public bool UseAttributeCleanouts
    {
        get => bools[c_useAttributeCleanoutsIdx];
        set
        {
            bools[c_useAttributeCleanoutsIdx] = value;
            bools[c_useAttributeCleanoutsIsSetIdx] = true;
        }
    }

    public bool UseAttributeCleanoutsSet => bools[c_useAttributeCleanoutsIsSetIdx];
    #endregion Shared Properties

    private bool m_noDefaultRecurringCapacityInterval; //want to create by default

    /// <summary>
    /// Set to false in order to create a default online Recurring Capacity Interval.
    /// Set to true if the RCI will be loaded explicitly instead.
    /// </summary>
    public bool NoDefaultRecurringCapacityInterval
    {
        get => m_noDefaultRecurringCapacityInterval;
        set => m_noDefaultRecurringCapacityInterval = value;
    }

    private decimal m_minNbrOfPeople;

    /// <summary>
    /// Activities that specify a NbrOfPeople less than this value cannot be scheduled on this Resource.
    /// </summary>
    public decimal MinNbrOfPeople
    {
        get => m_minNbrOfPeople;
        set
        {
            m_minNbrOfPeople = value;
            m_minNbrOfPeopleSet = true;
        }
    }

    private bool m_minNbrOfPeopleSet;
    public bool MinNbrOfPeopleSet => m_minNbrOfPeopleSet;

    private int m_priority;
    /// <summary>
    /// Used to determine a priority for scheduling on this Resource
    /// </summary>
    public int Priority
    {
        get => m_priority;
        internal set
        {
            m_priority = value;
            bools[c_priorityIsSetIdx] = true;
        }
    }

    public bool PriorityIsSet => bools[c_priorityIsSetIdx];

    public bool UseResourceSetupTime
    {
        get => bools[c_useResourceSetupTimeIdx];
        set
        {
            bools[c_useResourceSetupTimeIdx] = value;
            bools[c_useResourceSetupTimeIsSetIdx] = true;
        }
    }

    public bool UseResourceSetupTimeIsSet => bools[c_useResourceSetupTimeIsSetIdx];

    public bool UseSequencedSetupTime
    {
        get => bools[c_useSequencedSetupTimeIdx];
        internal set
        {
            bools[c_useSequencedSetupTimeIdx] = value;
            bools[c_useSequencedSetupTimeIsSetIdx] = true;
        }
    }

    public bool UseSequencedSetupTimeIsSet => bools[c_useSequencedSetupTimeIsSetIdx];

    public bool UseResourceCleanout
    {
        get => bools[c_useResourceCleanoutIdx];
        internal set
        {
            bools[c_useResourceCleanoutIdx] = value;
            bools[c_useResourceCleanoutIsSetIdx] = true;
        }
    }
    public bool UseResourceCleanoutIsSet => bools[c_useResourceCleanoutIsSetIdx];

    private decimal m_resourceSetupCost;
    public decimal ResourceSetupCost
    {
        get => m_resourceSetupCost;
        internal set
        {
            m_resourceSetupCost = value;
            bools[c_resourceSetupCostIsSetIdx] = true;
        }
    }
    public bool ResourceSetupCostIsSet => bools[c_resourceSetupCostIsSetIdx];

    private decimal m_resourceCleanoutCost;
    public decimal ResourceCleanoutCost
    {
        get => m_resourceCleanoutCost;
        internal set
        {
            m_resourceCleanoutCost = value;
            bools[c_resourceCleanoutCostIsSetIdx] = true;
        }
    }

    public bool ResourceCleanoutCostIsSet => bools[c_resourceCleanoutCostIsSetIdx];

    public override void Validate()
    {
        base.Validate();

        if (CapacityTypeSet && CapacityType != InternalResourceDefs.capacityTypes.SingleTasking)
        {
            if (UseSequencedSetupTime)
            {
                throw new APSCommon.PTValidationException("2924", new object[] { PlantExternalId, DepartmentExternalId, ExternalId });
            }
        }
    }
}