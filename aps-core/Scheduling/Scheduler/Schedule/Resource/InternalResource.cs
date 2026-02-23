using System.Collections;
using System.ComponentModel;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Debugging;
using PT.Common.PTMath;
using PT.ERPTransmissions;
using PT.Scheduler.Schedule.Resource.LookupTables;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Abstract class the defines attributes that are common across all resources that are internal to the organization.
/// </summary>
public abstract partial class InternalResource : BaseResource, IDeserializationInit, IPTSerializable
{
    #region IPTSerializable Members
    protected InternalResource(IReader reader, BaseIdGenerator a_idGen)
        : base(reader)
    {
        if (reader.VersionNumber >= 13000)
        {
            reader.Read(out cycleEfficiencyMultiplier);
            reader.Read(out overtimeHourlyCost);
            reader.Read(out scheduledRunSpanAlgorithm);
            reader.Read(out scheduledSetupSpanAlgorithm);
            reader.Read(out scheduledTransferSpanAlgorithm);
            reader.Read(out m_activitySetupEfficiencyMultiplier);
            reader.Read(out m_changeoverSetupEfficiencyMultiplier);
            reader.Read(out stage);
            reader.Read(out int val);
            capacityType = (InternalResourceDefs.capacityTypes)val;
            reader.Read(out standardHourlyCost);
            reader.Read(out setupPerformance);
            reader.Read(out runPerformance);
            reader.Read(out postProcessingPerformance);
            reader.Read(out scrapPerformance);
            reader.Read(out setupsDone);
            reader.Read(out runsDone);
            reader.Read(out postProcessesDone);
            reader.Read(out scrapsDone);
            reader.Read(out activitiesFinished);

            reader.Read(out bufferSpan);
            reader.Read(out headStartSpan);
            reader.Read(out setupSpanTicks);
            reader.Read(out overlappingOnlineIntervals);
            reader.Read(out val);
            m_batchType = (MainResourceDefs.batchType)val;
            reader.Read(out m_batchVolume);
            ResetBatchIfOutOfBounds();

            resourceCapacityIntervals = new ResourceCapacityIntervalsCollection(reader);
            capacityProfile = new CapacityProfile(reader, resourceCapacityIntervals);
            m_resultantCapacity = new ResourceCapacityIntervalList(reader);
            internalResourceFlags = new BoolVector32(reader);

            referenceInfo = new ReferenceInfo();
            m_normalDispatchDefinitionId = new BaseId(reader);
            for (int i = 0; i < c_numberOfExperimentalDispatcherDefinitions; i++)
            {
                m_experimentalDispatchDefinitionIds[i] = new BaseId(reader);
            }

            referenceInfo.capabilityIds = new BaseIdList(reader);
            referenceInfo.ciList = new BaseIdList(reader);
            referenceInfo.rciList = new BaseIdList(reader);

            //Lookup tables
            reader.Read(out bool haveTable);
            if (haveTable)
            {
                referenceInfo.attributeCodeTableId = new BaseId(reader);
            }

            reader.Read(out haveTable);
            if (haveTable)
            {
                referenceInfo.fromToRangeSetId = new BaseId(reader);
            }

            reader.Read(out bool isTank);
            if (isTank)
            {
                m_tank = new ResourceWarehouse(reader, a_idGen);
            }

            reader.Read(out m_minNbrOfPeople);
            reader.Read(out m_autoJoinSpan);

            reader.Read(out m_standardCleanSpanTicks);
            reader.Read(out m_standardCleanoutGrade);
            reader.Read(out m_priority);

            referenceInfo.timeCleanoutTriggerTableId = new BaseId(reader);
            referenceInfo.productionUnitsCleanoutTriggerTableId = new BaseId(reader);
            referenceInfo.operationCountCleanoutTriggerTableId = new BaseId(reader);

            reader.Read(out int compatibilityCodeTableCount);
            for (int i = 0; i < compatibilityCodeTableCount; i++)
            {
                BaseId tableId = new BaseId(reader);
                referenceInfo.CompatibilityCodeTableIds.Add(tableId);
            }

            reader.Read(out m_resourceSetupCost);
            reader.Read(out m_resourceCleanoutCost);

            referenceInfo.m_itemCleanoutTableId = new BaseId(reader);
        }
        else if (reader.VersionNumber >= 12513)
        {
            reader.Read(out cycleEfficiencyMultiplier);
            reader.Read(out overtimeHourlyCost);
            reader.Read(out scheduledRunSpanAlgorithm);
            reader.Read(out scheduledSetupSpanAlgorithm);
            reader.Read(out scheduledTransferSpanAlgorithm);
            reader.Read(out m_activitySetupEfficiencyMultiplier);
            reader.Read(out m_changeoverSetupEfficiencyMultiplier);
            reader.Read(out stage);
            reader.Read(out int val);
            capacityType = (InternalResourceDefs.capacityTypes)val;
            reader.Read(out standardHourlyCost);
            reader.Read(out setupPerformance);
            reader.Read(out runPerformance);
            reader.Read(out postProcessingPerformance);
            reader.Read(out scrapPerformance);
            reader.Read(out setupsDone);
            reader.Read(out runsDone);
            reader.Read(out postProcessesDone);
            reader.Read(out scrapsDone);
            reader.Read(out activitiesFinished);

            reader.Read(out bufferSpan);
            reader.Read(out headStartSpan);
            reader.Read(out setupSpanTicks);
            reader.Read(out overlappingOnlineIntervals);
            reader.Read(out val);
            m_batchType = (MainResourceDefs.batchType)val;
            reader.Read(out m_batchVolume);
            ResetBatchIfOutOfBounds();

            resourceCapacityIntervals = new ResourceCapacityIntervalsCollection(reader);
            capacityProfile = new CapacityProfile(reader, resourceCapacityIntervals);
            m_resultantCapacity = new ResourceCapacityIntervalList(reader);
            internalResourceFlags = new BoolVector32(reader);

            referenceInfo = new ReferenceInfo();
            m_normalDispatchDefinitionId = new BaseId(reader);
            for (int i = 0; i < c_numberOfExperimentalDispatcherDefinitions; i++)
            {
                m_experimentalDispatchDefinitionIds[i] = new BaseId(reader);
            }

            referenceInfo.capabilityIds = new BaseIdList(reader);
            referenceInfo.ciList = new BaseIdList(reader);
            referenceInfo.rciList = new BaseIdList(reader);

            //Lookup tables
            reader.Read(out bool haveTable);
            if (haveTable)
            {
                referenceInfo.attributeCodeTableId = new BaseId(reader);
            }

            reader.Read(out haveTable);
            if (haveTable)
            {
                referenceInfo.fromToRangeSetId = new BaseId(reader);
            }

            reader.Read(out bool isTank);
            if (isTank)
            {
                m_tank = new ResourceWarehouse(reader, a_idGen);
            }

            reader.Read(out m_minNbrOfPeople);
            reader.Read(out m_autoJoinSpan);

            reader.Read(out m_standardCleanSpanTicks);
            reader.Read(out m_standardCleanoutGrade);
            reader.Read(out m_priority);

            referenceInfo.timeCleanoutTriggerTableId = new BaseId(reader);
            referenceInfo.productionUnitsCleanoutTriggerTableId = new BaseId(reader);
            referenceInfo.operationCountCleanoutTriggerTableId = new BaseId(reader);

            reader.Read(out int compatibilityCodeTableCount);
            for (int i = 0; i < compatibilityCodeTableCount; i++)
            {
                BaseId tableId = new BaseId(reader);
                referenceInfo.CompatibilityCodeTableIds.Add(tableId);
            }

            reader.Read(out m_resourceSetupCost);
            reader.Read(out m_resourceCleanoutCost);
        }
        else if(reader.VersionNumber >= 12510)
        {
            reader.Read(out cycleEfficiencyMultiplier);
            reader.Read(out overtimeHourlyCost);
            reader.Read(out scheduledRunSpanAlgorithm);
            reader.Read(out scheduledSetupSpanAlgorithm);
            reader.Read(out scheduledTransferSpanAlgorithm);
            reader.Read(out m_activitySetupEfficiencyMultiplier);
            reader.Read(out m_changeoverSetupEfficiencyMultiplier);
            reader.Read(out stage);
            reader.Read(out int val);
            capacityType = (InternalResourceDefs.capacityTypes)val;
            reader.Read(out standardHourlyCost);
            reader.Read(out setupPerformance);
            reader.Read(out runPerformance);
            reader.Read(out postProcessingPerformance);
            reader.Read(out scrapPerformance);
            reader.Read(out setupsDone);
            reader.Read(out runsDone);
            reader.Read(out postProcessesDone);
            reader.Read(out scrapsDone);
            reader.Read(out activitiesFinished);

            reader.Read(out bufferSpan);
            reader.Read(out headStartSpan);
            reader.Read(out setupSpanTicks);
            reader.Read(out overlappingOnlineIntervals);
            reader.Read(out val);
            m_batchType = (MainResourceDefs.batchType)val;
            reader.Read(out m_batchVolume);
            ResetBatchIfOutOfBounds();

            resourceCapacityIntervals = new ResourceCapacityIntervalsCollection(reader);
            capacityProfile = new CapacityProfile(reader, resourceCapacityIntervals);
            m_resultantCapacity = new ResourceCapacityIntervalList(reader);
            internalResourceFlags = new BoolVector32(reader);

            referenceInfo = new ReferenceInfo();
            m_normalDispatchDefinitionId = new BaseId(reader);
            for (int i = 0; i < c_numberOfExperimentalDispatcherDefinitions; i++)
            {
                m_experimentalDispatchDefinitionIds[i] = new BaseId(reader);
            }

            referenceInfo.capabilityIds = new BaseIdList(reader);
            referenceInfo.ciList = new BaseIdList(reader);
            referenceInfo.rciList = new BaseIdList(reader);

            //Lookup tables
            reader.Read(out bool haveTable);
            if (haveTable)
            {
                referenceInfo.attributeCodeTableId = new BaseId(reader);
            }

            reader.Read(out haveTable);
            if (haveTable)
            {
                referenceInfo.fromToRangeSetId = new BaseId(reader);
            }

            reader.Read(out bool isTank);
            if (isTank)
            {
                m_tank = new ResourceWarehouse(reader, a_idGen);
            }

            reader.Read(out m_minNbrOfPeople);
            reader.Read(out m_autoJoinSpan);

            reader.Read(out m_standardCleanSpanTicks);
            reader.Read(out m_standardCleanoutGrade);
            reader.Read(out m_priority);

            referenceInfo.timeCleanoutTriggerTableId = new BaseId(reader);
            referenceInfo.productionUnitsCleanoutTriggerTableId = new BaseId(reader);
            referenceInfo.operationCountCleanoutTriggerTableId = new BaseId(reader);

            new ActivityKey(reader);//Deprecated

            reader.Read(out int compatibilityCodeTableCount);
            for (int i = 0; i < compatibilityCodeTableCount; i++)
            {
                BaseId tableId = new BaseId(reader);
                referenceInfo.CompatibilityCodeTableIds.Add(tableId);
            }

            reader.Read(out m_resourceSetupCost);
            reader.Read(out m_resourceCleanoutCost);
        }
        else
        {
            int setupIncludedBackwardsCompatibility = 0;
            if (reader.VersionNumber >= 12504)
            {
                reader.Read(out cycleEfficiencyMultiplier);
                reader.Read(out overtimeHourlyCost);
                reader.Read(out scheduledRunSpanAlgorithm);
                reader.Read(out scheduledSetupSpanAlgorithm);
                reader.Read(out scheduledTransferSpanAlgorithm);
                reader.Read(out m_activitySetupEfficiencyMultiplier);
                reader.Read(out m_changeoverSetupEfficiencyMultiplier);
                reader.Read(out stage);
                reader.Read(out setupIncludedBackwardsCompatibility);
                reader.Read(out  int val);
                capacityType = (InternalResourceDefs.capacityTypes)val;
                reader.Read(out standardHourlyCost);
                reader.Read(out setupPerformance);
                reader.Read(out runPerformance);
                reader.Read(out postProcessingPerformance);
                reader.Read(out scrapPerformance);
                reader.Read(out setupsDone);
                reader.Read(out runsDone);
                reader.Read(out postProcessesDone);
                reader.Read(out scrapsDone);
                reader.Read(out activitiesFinished);

                reader.Read(out bufferSpan);
                reader.Read(out headStartSpan);
                reader.Read(out setupSpanTicks);
                reader.Read(out overlappingOnlineIntervals);
                reader.Read(out val);
                m_batchType = (MainResourceDefs.batchType)val;
                reader.Read(out m_batchVolume);
                ResetBatchIfOutOfBounds();

                resourceCapacityIntervals = new ResourceCapacityIntervalsCollection(reader);
                capacityProfile = new CapacityProfile(reader, resourceCapacityIntervals);
                m_resultantCapacity = new ResourceCapacityIntervalList(reader);
                internalResourceFlags = new BoolVector32(reader);

                referenceInfo = new ReferenceInfo();
                m_normalDispatchDefinitionId = new BaseId(reader);
                for (int i = 0; i < c_numberOfExperimentalDispatcherDefinitions; i++)
                {
                    m_experimentalDispatchDefinitionIds[i] = new BaseId(reader);
                }

                referenceInfo.capabilityIds = new BaseIdList(reader);
                referenceInfo.ciList = new BaseIdList(reader);
                referenceInfo.rciList = new BaseIdList(reader);

                //Lookup tables
                reader.Read(out bool haveTable);
                if (haveTable)
                {
                    referenceInfo.attributeCodeTableId = new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.fromToRangeSetId = new BaseId(reader);
                }

                reader.Read(out bool isTank);
                if (isTank)
                {
                    m_tank = new ResourceWarehouse(reader, a_idGen);
                }

                reader.Read(out m_minNbrOfPeople);
                reader.Read(out m_autoJoinSpan);

                reader.Read(out m_standardCleanSpanTicks);
                reader.Read(out m_standardCleanoutGrade);
                reader.Read(out m_priority);

                referenceInfo.timeCleanoutTriggerTableId = new BaseId(reader);
                referenceInfo.productionUnitsCleanoutTriggerTableId = new BaseId(reader);
                referenceInfo.operationCountCleanoutTriggerTableId = new BaseId(reader);

                new ActivityKey(reader); //Deprecated

                reader.Read(out int compatibilityCodeTableCount);
                for (int i = 0; i < compatibilityCodeTableCount; i++)
                {
                    BaseId tableId = new BaseId(reader);
                    referenceInfo.CompatibilityCodeTableIds.Add(tableId);
                }
            }
            else if (reader.VersionNumber >= 12502)
            {
                reader.Read(out cycleEfficiencyMultiplier);
                reader.Read(out overtimeHourlyCost);
                reader.Read(out scheduledRunSpanAlgorithm);
                reader.Read(out scheduledSetupSpanAlgorithm);
                reader.Read(out scheduledTransferSpanAlgorithm);
                reader.Read(out m_activitySetupEfficiencyMultiplier);
                reader.Read(out m_changeoverSetupEfficiencyMultiplier);
                reader.Read(out stage);
                reader.Read(out setupIncludedBackwardsCompatibility);
                reader.Read(out int val);
                capacityType = (InternalResourceDefs.capacityTypes)val;
                reader.Read(out standardHourlyCost);
                reader.Read(out setupPerformance);
                reader.Read(out runPerformance);
                reader.Read(out postProcessingPerformance);
                reader.Read(out scrapPerformance);
                reader.Read(out setupsDone);
                reader.Read(out runsDone);
                reader.Read(out postProcessesDone);
                reader.Read(out scrapsDone);
                reader.Read(out activitiesFinished);

                reader.Read(out bufferSpan);
                reader.Read(out headStartSpan);
                reader.Read(out setupSpanTicks);
                reader.Read(out overlappingOnlineIntervals);
                reader.Read(out val);
                m_batchType = (MainResourceDefs.batchType)val;
                reader.Read(out m_batchVolume);
                ResetBatchIfOutOfBounds();

                resourceCapacityIntervals = new ResourceCapacityIntervalsCollection(reader);
                capacityProfile = new CapacityProfile(reader, resourceCapacityIntervals);
                m_resultantCapacity = new ResourceCapacityIntervalList(reader);
                internalResourceFlags = new BoolVector32(reader);

                referenceInfo = new ReferenceInfo();
                m_normalDispatchDefinitionId = new BaseId(reader);
                for (int i = 0; i < c_numberOfExperimentalDispatcherDefinitions; i++)
                {
                    m_experimentalDispatchDefinitionIds[i] = new BaseId(reader);
                }

                referenceInfo.capabilityIds = new BaseIdList(reader);
                referenceInfo.ciList = new BaseIdList(reader);
                referenceInfo.rciList = new BaseIdList(reader);

                //Lookup tables
                reader.Read(out bool haveTable);
                if (haveTable)
                {
                    referenceInfo.attributeCodeTableId = new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.fromToRangeSetId = new BaseId(reader);
                }

                reader.Read(out bool isTank);
                if (isTank)
                {
                    m_tank = new ResourceWarehouse(reader, a_idGen);
                }

                reader.Read(out m_minNbrOfPeople);
                reader.Read(out m_autoJoinSpan);

                reader.Read(out m_standardCleanSpanTicks);
                reader.Read(out m_standardCleanoutGrade);

                referenceInfo.timeCleanoutTriggerTableId = new BaseId(reader);
                referenceInfo.productionUnitsCleanoutTriggerTableId = new BaseId(reader);
                referenceInfo.operationCountCleanoutTriggerTableId = new BaseId(reader);

                new ActivityKey(reader); //Deprecated

                reader.Read(out int compatibilityCodeTableCount);
                for (int i = 0; i < compatibilityCodeTableCount; i++)
                {
                    BaseId tableId = new BaseId(reader);
                    referenceInfo.CompatibilityCodeTableIds.Add(tableId);
                }
            }
            else if (reader.VersionNumber >= 12500)
            {
                reader.Read(out cycleEfficiencyMultiplier);
                reader.Read(out overtimeHourlyCost);
                reader.Read(out scheduledRunSpanAlgorithm);
                reader.Read(out scheduledSetupSpanAlgorithm);
                reader.Read(out scheduledTransferSpanAlgorithm);
                reader.Read(out m_activitySetupEfficiencyMultiplier);
                reader.Read(out m_changeoverSetupEfficiencyMultiplier);
                reader.Read(out stage);
                reader.Read(out setupIncludedBackwardsCompatibility);
                reader.Read(out int val);
                capacityType = (InternalResourceDefs.capacityTypes)val;
                reader.Read(out standardHourlyCost);
                reader.Read(out setupPerformance);
                reader.Read(out runPerformance);
                reader.Read(out postProcessingPerformance);
                reader.Read(out scrapPerformance);
                reader.Read(out setupsDone);
                reader.Read(out runsDone);
                reader.Read(out postProcessesDone);
                reader.Read(out scrapsDone);
                reader.Read(out activitiesFinished);

                reader.Read(out bufferSpan);
                reader.Read(out headStartSpan);
                reader.Read(out TimeSpan maxSameSetupSpan);
                reader.Read(out setupSpanTicks);
                reader.Read(out overlappingOnlineIntervals);
                reader.Read(out val);
                m_batchType = (MainResourceDefs.batchType)val;
                reader.Read(out m_batchVolume);
                ResetBatchIfOutOfBounds();

                resourceCapacityIntervals = new ResourceCapacityIntervalsCollection(reader);
                capacityProfile = new CapacityProfile(reader, resourceCapacityIntervals);
                m_resultantCapacity = new ResourceCapacityIntervalList(reader);
                internalResourceFlags = new BoolVector32(reader);

                referenceInfo = new ReferenceInfo();
                m_normalDispatchDefinitionId = new BaseId(reader);
                for (int i = 0; i < c_numberOfExperimentalDispatcherDefinitions; i++)
                {
                    m_experimentalDispatchDefinitionIds[i] = new BaseId(reader);
                }

                referenceInfo.capabilityIds = new BaseIdList(reader);
                referenceInfo.ciList = new BaseIdList(reader);
                referenceInfo.rciList = new BaseIdList(reader);

                //Lookup tables
                reader.Read(out bool haveTable);
                if (haveTable)
                {
                    new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.attributeCodeTableId = new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.fromToRangeSetId = new BaseId(reader);
                }

                reader.Read(out bool isTank);
                if (isTank)
                {
                    m_tank = new ResourceWarehouse(reader, a_idGen);
                }

                reader.Read(out m_minNbrOfPeople);
                reader.Read(out m_autoJoinSpan);

                reader.Read(out m_standardCleanSpanTicks);
                reader.Read(out m_standardCleanoutGrade);

                referenceInfo.timeCleanoutTriggerTableId = new BaseId(reader);
                referenceInfo.productionUnitsCleanoutTriggerTableId = new BaseId(reader);
                referenceInfo.operationCountCleanoutTriggerTableId = new BaseId(reader);

                new ActivityKey(reader); //Deprecated

                reader.Read(out int compatibilityCodeTableCount);
                for (int i = 0; i < compatibilityCodeTableCount; i++)
                {
                    BaseId tableId = new BaseId(reader);
                    referenceInfo.CompatibilityCodeTableIds.Add(tableId);
                }
            }
            else if (reader.VersionNumber >= 12420)
            {
                reader.Read(out cycleEfficiencyMultiplier);
                reader.Read(out overtimeHourlyCost);
                reader.Read(out scheduledRunSpanAlgorithm);
                reader.Read(out scheduledSetupSpanAlgorithm);
                reader.Read(out scheduledTransferSpanAlgorithm);
                reader.Read(out m_activitySetupEfficiencyMultiplier);
                reader.Read(out m_changeoverSetupEfficiencyMultiplier);
                reader.Read(out stage);
                reader.Read(out setupIncludedBackwardsCompatibility);
                reader.Read(out int val);
                capacityType = (InternalResourceDefs.capacityTypes)val;
                reader.Read(out standardHourlyCost);
                reader.Read(out setupPerformance);
                reader.Read(out runPerformance);
                reader.Read(out postProcessingPerformance);
                reader.Read(out scrapPerformance);
                reader.Read(out setupsDone);
                reader.Read(out runsDone);
                reader.Read(out postProcessesDone);
                reader.Read(out scrapsDone);
                reader.Read(out activitiesFinished);

                reader.Read(out bufferSpan);
                reader.Read(out headStartSpan);
                reader.Read(out TimeSpan maxSameSetupSpan);
                reader.Read(out setupSpanTicks);
                reader.Read(out overlappingOnlineIntervals);
                reader.Read(out val);
                m_batchType = (MainResourceDefs.batchType)val;
                reader.Read(out m_batchVolume);
                ResetBatchIfOutOfBounds();

                resourceCapacityIntervals = new ResourceCapacityIntervalsCollection(reader);
                capacityProfile = new CapacityProfile(reader, resourceCapacityIntervals);
                m_resultantCapacity = new ResourceCapacityIntervalList(reader);
                internalResourceFlags = new BoolVector32(reader);

                referenceInfo = new ReferenceInfo();
                m_normalDispatchDefinitionId = new BaseId(reader);
                for (int i = 0; i < c_numberOfExperimentalDispatcherDefinitions; i++)
                {
                    m_experimentalDispatchDefinitionIds[i] = new BaseId(reader);
                }

                referenceInfo.capabilityIds = new BaseIdList(reader);
                referenceInfo.ciList = new BaseIdList(reader);
                referenceInfo.rciList = new BaseIdList(reader);

                //Lookup tables
                reader.Read(out bool haveTable);
                if (haveTable)
                {
                    new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.attributeCodeTableId = new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.fromToRangeSetId = new BaseId(reader);
                }

                reader.Read(out bool isTank);
                if (isTank)
                {
                    m_tank = new ResourceWarehouse(reader, a_idGen);
                }

                reader.Read(out m_minNbrOfPeople);
                reader.Read(out m_autoJoinSpan);

                reader.Read(out m_standardCleanSpanTicks);
                reader.Read(out m_standardCleanoutGrade);
                reader.Read(out m_priority);

                referenceInfo.timeCleanoutTriggerTableId = new BaseId(reader);
                referenceInfo.productionUnitsCleanoutTriggerTableId = new BaseId(reader);
                referenceInfo.operationCountCleanoutTriggerTableId = new BaseId(reader);

                new ActivityKey(reader); //Deprecated

                reader.Read(out int compatibilityCodeTableCount);
                for (int i = 0; i < compatibilityCodeTableCount; i++)
                {
                    BaseId tableId = new BaseId(reader);
                    referenceInfo.CompatibilityCodeTableIds.Add(tableId);
                }
            }
            else if (reader.VersionNumber >= 12408)
            {
                reader.Read(out cycleEfficiencyMultiplier);
                reader.Read(out overtimeHourlyCost);
                reader.Read(out scheduledRunSpanAlgorithm);
                reader.Read(out scheduledSetupSpanAlgorithm);
                reader.Read(out scheduledTransferSpanAlgorithm);
                reader.Read(out m_activitySetupEfficiencyMultiplier);
                reader.Read(out m_changeoverSetupEfficiencyMultiplier);
                reader.Read(out stage);
                reader.Read(out setupIncludedBackwardsCompatibility);
                reader.Read(out int val);
                capacityType = (InternalResourceDefs.capacityTypes)val;
                reader.Read(out standardHourlyCost);
                reader.Read(out setupPerformance);
                reader.Read(out runPerformance);
                reader.Read(out postProcessingPerformance);
                reader.Read(out scrapPerformance);
                reader.Read(out setupsDone);
                reader.Read(out runsDone);
                reader.Read(out postProcessesDone);
                reader.Read(out scrapsDone);
                reader.Read(out activitiesFinished);

                reader.Read(out bufferSpan);
                reader.Read(out headStartSpan);
                reader.Read(out TimeSpan maxSameSetupSpan);
                reader.Read(out setupSpanTicks);
                reader.Read(out overlappingOnlineIntervals);
                reader.Read(out val);
                m_batchType = (MainResourceDefs.batchType)val;
                reader.Read(out m_batchVolume);
                ResetBatchIfOutOfBounds();

                resourceCapacityIntervals = new ResourceCapacityIntervalsCollection(reader);
                capacityProfile = new CapacityProfile(reader, resourceCapacityIntervals);
                m_resultantCapacity = new ResourceCapacityIntervalList(reader);
                internalResourceFlags = new BoolVector32(reader);

                referenceInfo = new ReferenceInfo();
                m_normalDispatchDefinitionId = new BaseId(reader);
                for (int i = 0; i < c_numberOfExperimentalDispatcherDefinitions; i++)
                {
                    m_experimentalDispatchDefinitionIds[i] = new BaseId(reader);
                }

                referenceInfo.capabilityIds = new BaseIdList(reader);
                referenceInfo.ciList = new BaseIdList(reader);
                referenceInfo.rciList = new BaseIdList(reader);

                //Lookup tables
                reader.Read(out bool haveTable);
                if (haveTable)
                {
                    new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.attributeCodeTableId = new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.fromToRangeSetId = new BaseId(reader);
                }

                reader.Read(out bool isTank);
                if (isTank)
                {
                    m_tank = new ResourceWarehouse(reader, a_idGen);
                }

                reader.Read(out m_minNbrOfPeople);
                reader.Read(out m_autoJoinSpan);

                reader.Read(out m_standardCleanSpanTicks);
                reader.Read(out m_standardCleanoutGrade);

                referenceInfo.timeCleanoutTriggerTableId = new BaseId(reader);
                referenceInfo.productionUnitsCleanoutTriggerTableId = new BaseId(reader);
                referenceInfo.operationCountCleanoutTriggerTableId = new BaseId(reader);

                new ActivityKey(reader); //Deprecated

                reader.Read(out int compatibilityCodeTableCount);
                for (int i = 0; i < compatibilityCodeTableCount; i++)
                {
                    BaseId tableId = new BaseId(reader);
                    referenceInfo.CompatibilityCodeTableIds.Add(tableId);
                }
            }
            else if (reader.VersionNumber >= 12401)
            {
                reader.Read(out cycleEfficiencyMultiplier);
                reader.Read(out overtimeHourlyCost);
                reader.Read(out scheduledRunSpanAlgorithm);
                reader.Read(out scheduledSetupSpanAlgorithm);
                reader.Read(out scheduledTransferSpanAlgorithm);
                reader.Read(out m_activitySetupEfficiencyMultiplier);
                reader.Read(out m_changeoverSetupEfficiencyMultiplier);
                reader.Read(out stage);
                reader.Read(out setupIncludedBackwardsCompatibility);
                reader.Read(out int val);
                capacityType = (InternalResourceDefs.capacityTypes)val;
                reader.Read(out standardHourlyCost);
                reader.Read(out setupPerformance);
                reader.Read(out runPerformance);
                reader.Read(out postProcessingPerformance);
                reader.Read(out scrapPerformance);
                reader.Read(out setupsDone);
                reader.Read(out runsDone);
                reader.Read(out postProcessesDone);
                reader.Read(out scrapsDone);
                reader.Read(out activitiesFinished);

                reader.Read(out bufferSpan);
                reader.Read(out headStartSpan);
                reader.Read(out TimeSpan maxSameSetupSpan);
                reader.Read(out setupSpanTicks);
                reader.Read(out overlappingOnlineIntervals);
                reader.Read(out val);
                m_batchType = (MainResourceDefs.batchType)val;
                reader.Read(out m_batchVolume);
                ResetBatchIfOutOfBounds();

                resourceCapacityIntervals = new ResourceCapacityIntervalsCollection(reader);
                capacityProfile = new CapacityProfile(reader, resourceCapacityIntervals);
                m_resultantCapacity = new ResourceCapacityIntervalList(reader);
                internalResourceFlags = new BoolVector32(reader);

                referenceInfo = new ReferenceInfo();
                m_normalDispatchDefinitionId = new BaseId(reader);
                for (int i = 0; i < c_numberOfExperimentalDispatcherDefinitions; i++)
                {
                    m_experimentalDispatchDefinitionIds[i] = new BaseId(reader);
                }

                referenceInfo.capabilityIds = new BaseIdList(reader);
                referenceInfo.ciList = new BaseIdList(reader);
                referenceInfo.rciList = new BaseIdList(reader);

                //Lookup tables
                reader.Read(out bool haveTable);
                if (haveTable)
                {
                    new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.attributeCodeTableId = new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.fromToRangeSetId = new BaseId(reader);
                }

                m_backwardsCompatibilityProductRules = new ProductRules(reader);

                reader.Read(out bool isTank);
                if (isTank)
                {
                    m_tank = new ResourceWarehouse(reader, a_idGen);
                }

                reader.Read(out m_minNbrOfPeople);
                reader.Read(out m_autoJoinSpan);

                reader.Read(out m_standardCleanSpanTicks);
                reader.Read(out m_standardCleanoutGrade);

                referenceInfo.timeCleanoutTriggerTableId = new BaseId(reader);
                referenceInfo.productionUnitsCleanoutTriggerTableId = new BaseId(reader);
                referenceInfo.operationCountCleanoutTriggerTableId = new BaseId(reader);

                new ActivityKey(reader); //Deprecated

                reader.Read(out int compatibilityCodeTableCount);
                for (int i = 0; i < compatibilityCodeTableCount; i++)
                {
                    BaseId tableId = new BaseId(reader);
                    referenceInfo.CompatibilityCodeTableIds.Add(tableId);
                }
            }
            else if (reader.VersionNumber >= 12320)
            {
                reader.Read(out cycleEfficiencyMultiplier);
                reader.Read(out overtimeHourlyCost);
                reader.Read(out scheduledRunSpanAlgorithm);
                reader.Read(out scheduledSetupSpanAlgorithm);
                reader.Read(out scheduledTransferSpanAlgorithm);
                reader.Read(out m_activitySetupEfficiencyMultiplier);
                reader.Read(out stage);
                reader.Read(out setupIncludedBackwardsCompatibility);
                reader.Read(out int val);
                capacityType = (InternalResourceDefs.capacityTypes)val;
                reader.Read(out standardHourlyCost);
                reader.Read(out setupPerformance);
                reader.Read(out runPerformance);
                reader.Read(out postProcessingPerformance);
                reader.Read(out scrapPerformance);
                reader.Read(out setupsDone);
                reader.Read(out runsDone);
                reader.Read(out postProcessesDone);
                reader.Read(out scrapsDone);
                reader.Read(out activitiesFinished);

                reader.Read(out bufferSpan);
                reader.Read(out headStartSpan);
                reader.Read(out TimeSpan maxSameSetupSpan);
                reader.Read(out setupSpanTicks);
                reader.Read(out overlappingOnlineIntervals);
                reader.Read(out val);
                m_batchType = (MainResourceDefs.batchType)val;
                reader.Read(out m_batchVolume);
                ResetBatchIfOutOfBounds();

                resourceCapacityIntervals = new ResourceCapacityIntervalsCollection(reader);
                capacityProfile = new CapacityProfile(reader, resourceCapacityIntervals);
                m_resultantCapacity = new ResourceCapacityIntervalList(reader);
                internalResourceFlags = new BoolVector32(reader);

                referenceInfo = new ReferenceInfo();
                m_normalDispatchDefinitionId = new BaseId(reader);
                for (int i = 0; i < c_numberOfExperimentalDispatcherDefinitions; i++)
                {
                    m_experimentalDispatchDefinitionIds[i] = new BaseId(reader);
                }

                referenceInfo.capabilityIds = new BaseIdList(reader);
                referenceInfo.ciList = new BaseIdList(reader);
                referenceInfo.rciList = new BaseIdList(reader);

                //Lookup tables
                reader.Read(out bool haveTable);
                if (haveTable)
                {
                    new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.attributeCodeTableId = new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.fromToRangeSetId = new BaseId(reader);
                }

                m_backwardsCompatibilityProductRules = new ProductRules(reader);

                reader.Read(out bool isTank);
                if (isTank)
                {
                    m_tank = new ResourceWarehouse(reader, a_idGen);
                }

                reader.Read(out m_minNbrOfPeople);
                reader.Read(out m_autoJoinSpan);

                reader.Read(out m_standardCleanSpanTicks);
                reader.Read(out m_standardCleanoutGrade);

                referenceInfo.timeCleanoutTriggerTableId = new BaseId(reader);
                referenceInfo.productionUnitsCleanoutTriggerTableId = new BaseId(reader);
                referenceInfo.operationCountCleanoutTriggerTableId = new BaseId(reader);

                new ActivityKey(reader); //Deprecated

                reader.Read(out int compatibilityCodeTableCount);
                for (int i = 0; i < compatibilityCodeTableCount; i++)
                {
                    BaseId tableId = new BaseId(reader);
                    referenceInfo.CompatibilityCodeTableIds.Add(tableId);
                }
            }
            else if (reader.VersionNumber >= 12318)
            {
                reader.Read(out cycleEfficiencyMultiplier);
                reader.Read(out overtimeHourlyCost);
                reader.Read(out scheduledRunSpanAlgorithm);
                reader.Read(out scheduledSetupSpanAlgorithm);
                reader.Read(out scheduledTransferSpanAlgorithm);
                reader.Read(out m_activitySetupEfficiencyMultiplier);
                reader.Read(out stage);
                reader.Read(out setupIncludedBackwardsCompatibility);
                reader.Read(out int val);
                capacityType = (InternalResourceDefs.capacityTypes)val;
                reader.Read(out standardHourlyCost);
                reader.Read(out setupPerformance);
                reader.Read(out runPerformance);
                reader.Read(out postProcessingPerformance);
                reader.Read(out scrapPerformance);
                reader.Read(out setupsDone);
                reader.Read(out runsDone);
                reader.Read(out postProcessesDone);
                reader.Read(out scrapsDone);
                reader.Read(out activitiesFinished);

                reader.Read(out bufferSpan);
                reader.Read(out headStartSpan);
                reader.Read(out TimeSpan maxSameSetupSpan);
                reader.Read(out setupSpanTicks);
                reader.Read(out overlappingOnlineIntervals);
                reader.Read(out val);
                m_batchType = (MainResourceDefs.batchType)val;
                reader.Read(out m_batchVolume);
                ResetBatchIfOutOfBounds();

                resourceCapacityIntervals = new ResourceCapacityIntervalsCollection(reader);
                capacityProfile = new CapacityProfile(reader, resourceCapacityIntervals);
                m_resultantCapacity = new ResourceCapacityIntervalList(reader);
                internalResourceFlags = new BoolVector32(reader);

                referenceInfo = new ReferenceInfo();
                m_normalDispatchDefinitionId = new BaseId(reader);
                for (int i = 0; i < c_numberOfExperimentalDispatcherDefinitions; i++)
                {
                    m_experimentalDispatchDefinitionIds[i] = new BaseId(reader);
                }

                referenceInfo.capabilityIds = new BaseIdList(reader);
                referenceInfo.ciList = new BaseIdList(reader);
                referenceInfo.rciList = new BaseIdList(reader);

                //Lookup tables
                reader.Read(out bool haveTable);
                if (haveTable)
                {
                    new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.attributeCodeTableId = new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.fromToRangeSetId = new BaseId(reader);
                }

                m_backwardsCompatibilityProductRules = new ProductRules(reader);

                reader.Read(out bool isTank);
                if (isTank)
                {
                    m_tank = new ResourceWarehouse(reader, a_idGen);
                }

                reader.Read(out m_minNbrOfPeople);
                reader.Read(out m_autoJoinSpan);

                reader.Read(out m_standardCleanSpanTicks);
                reader.Read(out m_standardCleanoutGrade);

                referenceInfo.timeCleanoutTriggerTableId = new BaseId(reader);
                referenceInfo.productionUnitsCleanoutTriggerTableId = new BaseId(reader);
                referenceInfo.operationCountCleanoutTriggerTableId = new BaseId(reader);

                new ActivityKey(reader); //Deprecated
            }
            else if (reader.VersionNumber >= 12305)
            {
                reader.Read(out string currentProductSetup);
                reader.Read(out string currentSetupCode);
                reader.Read(out decimal currentSetupNumber);
                reader.Read(out cycleEfficiencyMultiplier);
                reader.Read(out string cycleSpanAlgorithm);
                reader.Read(out overtimeHourlyCost);
                reader.Read(out scheduledRunSpanAlgorithm);
                reader.Read(out scheduledSetupSpanAlgorithm);
                reader.Read(out scheduledTransferSpanAlgorithm);
                reader.Read(out m_activitySetupEfficiencyMultiplier);
                reader.Read(out stage);
                reader.Read(out setupIncludedBackwardsCompatibility);
                reader.Read(out int val);
                capacityType = (InternalResourceDefs.capacityTypes)val;
                reader.Read(out standardHourlyCost);
                reader.Read(out setupPerformance);
                reader.Read(out runPerformance);
                reader.Read(out postProcessingPerformance);
                reader.Read(out scrapPerformance);
                reader.Read(out setupsDone);
                reader.Read(out runsDone);
                reader.Read(out postProcessesDone);
                reader.Read(out scrapsDone);
                reader.Read(out activitiesFinished);

                reader.Read(out bufferSpan);
                reader.Read(out headStartSpan);
                reader.Read(out TimeSpan maxSameSetupSpan);
                reader.Read(out setupSpanTicks);
                reader.Read(out overlappingOnlineIntervals);
                reader.Read(out val);
                m_batchType = (MainResourceDefs.batchType)val;
                reader.Read(out m_batchVolume);
                ResetBatchIfOutOfBounds();

                resourceCapacityIntervals = new ResourceCapacityIntervalsCollection(reader);
                capacityProfile = new CapacityProfile(reader, resourceCapacityIntervals);
                m_resultantCapacity = new ResourceCapacityIntervalList(reader);
                internalResourceFlags = new BoolVector32(reader);

                referenceInfo = new ReferenceInfo();
                m_normalDispatchDefinitionId = new BaseId(reader);
                for (int i = 0; i < c_numberOfExperimentalDispatcherDefinitions; i++)
                {
                    m_experimentalDispatchDefinitionIds[i] = new BaseId(reader);
                }

                referenceInfo.capabilityIds = new BaseIdList(reader);
                referenceInfo.ciList = new BaseIdList(reader);
                referenceInfo.rciList = new BaseIdList(reader);

                //Lookup tables
                reader.Read(out bool haveTable);
                if (haveTable)
                {
                    new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.attributeCodeTableId = new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.fromToRangeSetId = new BaseId(reader);
                }

                m_backwardsCompatibilityProductRules = new ProductRules(reader);

                reader.Read(out bool isTank);
                if (isTank)
                {
                    m_tank = new ResourceWarehouse(reader, a_idGen);
                }

                reader.Read(out m_minNbrOfPeople);
                reader.Read(out m_autoJoinSpan);

                reader.Read(out m_standardCleanSpanTicks);
                reader.Read(out m_standardCleanoutGrade);

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.timeCleanoutTriggerTableId = new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.productionUnitsCleanoutTriggerTableId = new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.operationCountCleanoutTriggerTableId = new BaseId(reader);
                }
            }
            else if (reader.VersionNumber >= 12303)
            {
                reader.Read(out string currentProductSetup);
                reader.Read(out string currentSetupCode);
                reader.Read(out decimal currentSetupNumber);
                reader.Read(out cycleEfficiencyMultiplier);
                reader.Read(out string cycleSpanAlgorithm);
                reader.Read(out overtimeHourlyCost);
                reader.Read(out scheduledRunSpanAlgorithm);
                reader.Read(out scheduledSetupSpanAlgorithm);
                reader.Read(out scheduledTransferSpanAlgorithm);
                reader.Read(out m_activitySetupEfficiencyMultiplier);
                reader.Read(out stage);
                reader.Read(out setupIncludedBackwardsCompatibility);
                reader.Read(out int val);
                capacityType = (InternalResourceDefs.capacityTypes)val;
                reader.Read(out standardHourlyCost);
                reader.Read(out setupPerformance);
                reader.Read(out runPerformance);
                reader.Read(out postProcessingPerformance);
                reader.Read(out scrapPerformance);
                reader.Read(out setupsDone);
                reader.Read(out runsDone);
                reader.Read(out postProcessesDone);
                reader.Read(out scrapsDone);
                reader.Read(out activitiesFinished);

                reader.Read(out bufferSpan);
                reader.Read(out headStartSpan);
                reader.Read(out TimeSpan maxSameSetupSpan);
                reader.Read(out setupSpanTicks);
                reader.Read(out overlappingOnlineIntervals);
                reader.Read(out val);
                m_batchType = (MainResourceDefs.batchType)val;
                reader.Read(out m_batchVolume);
                ResetBatchIfOutOfBounds();

                resourceCapacityIntervals = new ResourceCapacityIntervalsCollection(reader);
                capacityProfile = new CapacityProfile(reader, resourceCapacityIntervals);
                m_resultantCapacity = new ResourceCapacityIntervalList(reader);
                internalResourceFlags = new BoolVector32(reader);

                referenceInfo = new ReferenceInfo();
                m_normalDispatchDefinitionId = new BaseId(reader);
                for (int i = 0; i < c_numberOfExperimentalDispatcherDefinitions; i++)
                {
                    m_experimentalDispatchDefinitionIds[i] = new BaseId(reader);
                }

                referenceInfo.capabilityIds = new BaseIdList(reader);
                referenceInfo.ciList = new BaseIdList(reader);
                referenceInfo.rciList = new BaseIdList(reader);

                //Lookup tables
                reader.Read(out bool haveTable);
                if (haveTable)
                {
                    new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.attributeCodeTableId = new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.fromToRangeSetId = new BaseId(reader);
                }

                m_backwardsCompatibilityProductRules = new ProductRules(reader);

                reader.Read(out bool isTank);
                if (isTank)
                {
                    m_tank = new ResourceWarehouse(reader, a_idGen);
                }

                reader.Read(out m_minNbrOfPeople);
                reader.Read(out m_autoJoinSpan);

                reader.Read(out m_standardCleanSpanTicks);
                reader.Read(out m_standardCleanoutGrade);
            }
            else if (reader.VersionNumber >= 12208)
            {
                reader.Read(out string currentProductSetup);
                reader.Read(out string currentSetupCode);
                reader.Read(out decimal currentSetupNumber);
                reader.Read(out cycleEfficiencyMultiplier);
                reader.Read(out string cycleSpanAlgorithm);
                reader.Read(out overtimeHourlyCost);
                reader.Read(out scheduledRunSpanAlgorithm);
                reader.Read(out scheduledSetupSpanAlgorithm);
                reader.Read(out scheduledTransferSpanAlgorithm);
                reader.Read(out m_activitySetupEfficiencyMultiplier);
                reader.Read(out stage);
                reader.Read(out setupIncludedBackwardsCompatibility);
                reader.Read(out int val);
                capacityType = (InternalResourceDefs.capacityTypes)val;
                reader.Read(out standardHourlyCost);
                reader.Read(out setupPerformance);
                reader.Read(out runPerformance);
                reader.Read(out postProcessingPerformance);
                reader.Read(out scrapPerformance);
                reader.Read(out setupsDone);
                reader.Read(out runsDone);
                reader.Read(out postProcessesDone);
                reader.Read(out scrapsDone);
                reader.Read(out activitiesFinished);

                reader.Read(out bufferSpan);
                reader.Read(out headStartSpan);
                reader.Read(out TimeSpan maxSameSetupSpan);
                reader.Read(out setupSpanTicks);
                reader.Read(out overlappingOnlineIntervals);
                reader.Read(out val);
                m_batchType = (MainResourceDefs.batchType)val;
                reader.Read(out m_batchVolume);
                ResetBatchIfOutOfBounds();

                resourceCapacityIntervals = new ResourceCapacityIntervalsCollection(reader);
                capacityProfile = new CapacityProfile(reader, resourceCapacityIntervals);
                m_resultantCapacity = new ResourceCapacityIntervalList(reader);
                internalResourceFlags = new BoolVector32(reader);

                referenceInfo = new ReferenceInfo();
                m_normalDispatchDefinitionId = new BaseId(reader);
                for (int i = 0; i < c_numberOfExperimentalDispatcherDefinitions; i++)
                {
                    m_experimentalDispatchDefinitionIds[i] = new BaseId(reader);
                }

                referenceInfo.capabilityIds = new BaseIdList(reader);
                referenceInfo.ciList = new BaseIdList(reader);
                referenceInfo.rciList = new BaseIdList(reader);

                //Lookup tables
                bool haveTable;
                reader.Read(out haveTable);
                if (haveTable)
                {
                    new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.attributeCodeTableId = new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.fromToRangeSetId = new BaseId(reader);
                }

                m_backwardsCompatibilityProductRules = new ProductRules(reader);

                bool isTank;
                reader.Read(out isTank);
                if (isTank)
                {
                    m_tank = new ResourceWarehouse(reader, a_idGen);
                }

                reader.Read(out m_minNbrOfPeople);
                reader.Read(out m_autoJoinSpan);
            }

            #region 12101
            else if (reader.VersionNumber >= 12101)
            {
                reader.Read(out string currentProductSetup);
                reader.Read(out string currentSetupCode);
                reader.Read(out decimal currentSetupNumber);
                reader.Read(out cycleEfficiencyMultiplier);
                reader.Read(out string cycleSpanAlgorithm);
                reader.Read(out overtimeHourlyCost);
                reader.Read(out scheduledRunSpanAlgorithm);
                reader.Read(out scheduledSetupSpanAlgorithm);
                reader.Read(out scheduledTransferSpanAlgorithm);
                reader.Read(out m_activitySetupEfficiencyMultiplier);
                reader.Read(out stage);
                reader.Read(out setupIncludedBackwardsCompatibility);
                reader.Read(out int val);
                capacityType = (InternalResourceDefs.capacityTypes)val;
                reader.Read(out standardHourlyCost);
                reader.Read(out setupPerformance);
                reader.Read(out runPerformance);
                reader.Read(out postProcessingPerformance);
                reader.Read(out scrapPerformance);
                reader.Read(out setupsDone);
                reader.Read(out runsDone);
                reader.Read(out postProcessesDone);
                reader.Read(out scrapsDone);
                reader.Read(out activitiesFinished);

                reader.Read(out bufferSpan);
                reader.Read(out headStartSpan);
                reader.Read(out TimeSpan maxSameSetupSpan);
                reader.Read(out setupSpanTicks);
                reader.Read(out overlappingOnlineIntervals);
                reader.Read(out val);
                m_batchType = (MainResourceDefs.batchType)val;
                reader.Read(out m_batchVolume);
                ResetBatchIfOutOfBounds();

                resourceCapacityIntervals = new ResourceCapacityIntervalsCollection(reader);
                capacityProfile = new CapacityProfile(reader, resourceCapacityIntervals);
                m_resultantCapacity = new ResourceCapacityIntervalList(reader);
                internalResourceFlags = new BoolVector32(reader);

                referenceInfo = new ReferenceInfo();
                m_normalDispatchDefinitionId = new BaseId(reader);
                m_experimentalDispatchDefinitionIds[0] = new BaseId(reader);
                m_experimentalDispatchDefinitionIds[1] = m_experimentalDispatchDefinitionIds[0];
                m_experimentalDispatchDefinitionIds[2] = m_experimentalDispatchDefinitionIds[0];
                m_experimentalDispatchDefinitionIds[3] = m_experimentalDispatchDefinitionIds[0];
                referenceInfo.capabilityIds = new BaseIdList(reader);
                referenceInfo.ciList = new BaseIdList(reader);
                referenceInfo.rciList = new BaseIdList(reader);

                //Lookup tables
                bool haveTable;
                reader.Read(out haveTable);
                if (haveTable)
                {
                    new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.attributeCodeTableId = new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.fromToRangeSetId = new BaseId(reader);
                }

                m_backwardsCompatibilityProductRules = new ProductRules(reader);

                bool isTank;
                reader.Read(out isTank);
                if (isTank)
                {
                    m_tank = new ResourceWarehouse(reader, a_idGen);
                }

                reader.Read(out m_minNbrOfPeople);
                reader.Read(out m_autoJoinSpan);
            }
            #endregion

            /*
             * This block below is here because 12100 is when we jumped VersionNumber in 12.1
             * As a result, 12100 is technically the same as 12058, but if this
             * else if wasn't here, then it'd get processed like 12070, which is 12.0's de-serialization.
             *
             * Note: This block should be equivalent to the 12058 block as a result of this too.
             */
            else if (reader.VersionNumber >= 12100)
            {
                reader.Read(out string currentProductSetup);
                reader.Read(out string currentSetupCode);
                reader.Read(out decimal currentSetupNumber);
                reader.Read(out cycleEfficiencyMultiplier);
                reader.Read(out string cycleSpanAlgorithm);
                reader.Read(out overtimeHourlyCost);
                reader.Read(out scheduledRunSpanAlgorithm);
                reader.Read(out scheduledSetupSpanAlgorithm);
                reader.Read(out scheduledTransferSpanAlgorithm);
                reader.Read(out decimal obsolete_sameSetupHeadstartMultiplier);
                reader.Read(out m_activitySetupEfficiencyMultiplier);
                reader.Read(out stage);
                reader.Read(out setupIncludedBackwardsCompatibility);
                reader.Read(out int val);
                capacityType = (InternalResourceDefs.capacityTypes)val;
                reader.Read(out standardHourlyCost);
                reader.Read(out setupPerformance);
                reader.Read(out runPerformance);
                reader.Read(out postProcessingPerformance);
                reader.Read(out scrapPerformance);
                reader.Read(out setupsDone);
                reader.Read(out runsDone);
                reader.Read(out postProcessesDone);
                reader.Read(out scrapsDone);
                reader.Read(out activitiesFinished);

                reader.Read(out bufferSpan);
                reader.Read(out headStartSpan);
                reader.Read(out TimeSpan maxSameSetupSpan);
                reader.Read(out setupSpanTicks);
                reader.Read(out overlappingOnlineIntervals);
                reader.Read(out val);
                m_batchType = (MainResourceDefs.batchType)val;
                reader.Read(out m_batchVolume);
                ResetBatchIfOutOfBounds();

                resourceCapacityIntervals = new ResourceCapacityIntervalsCollection(reader);
                capacityProfile = new CapacityProfile(reader, resourceCapacityIntervals);
                m_resultantCapacity = new ResourceCapacityIntervalList(reader);
                internalResourceFlags = new BoolVector32(reader);

                referenceInfo = new ReferenceInfo();
                m_normalDispatchDefinitionId = new BaseId(reader);
                m_experimentalDispatchDefinitionIds[0] = new BaseId(reader);
                m_experimentalDispatchDefinitionIds[1] = m_experimentalDispatchDefinitionIds[0];
                m_experimentalDispatchDefinitionIds[2] = m_experimentalDispatchDefinitionIds[0];
                m_experimentalDispatchDefinitionIds[3] = m_experimentalDispatchDefinitionIds[0];
                referenceInfo.capabilityIds = new BaseIdList(reader);
                referenceInfo.ciList = new BaseIdList(reader);
                referenceInfo.rciList = new BaseIdList(reader);

                //Lookup tables
                bool haveTable;
                reader.Read(out haveTable);
                if (haveTable)
                {
                    new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.attributeCodeTableId = new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.fromToRangeSetId = new BaseId(reader);
                }

                m_backwardsCompatibilityProductRules = new ProductRules(reader);

                bool isTank;
                reader.Read(out isTank);
                if (isTank)
                {
                    m_tank = new ResourceWarehouse(reader, a_idGen);
                }

                reader.Read(out m_minNbrOfPeople);
                reader.Read(out m_autoJoinSpan);
            }
            /*
             * The block below exists for compatibility with 12.0.
             * This block is equivalent to 12038's de-serialization because
             * that was with 12.0 was on when we made the jump to 12070 in 12.0
             */
            else if (reader.VersionNumber >= 12070)
            {
                reader.Read(out string currentProductSetup);
                reader.Read(out string currentSetupCode);
                reader.Read(out decimal currentSetupNumber);
                reader.Read(out cycleEfficiencyMultiplier);
                reader.Read(out string cycleSpanAlgorithm);
                reader.Read(out overtimeHourlyCost);
                reader.Read(out int _); // This use to be converted into processingStatus, but we removed that property
                reader.Read(out scheduledRunSpanAlgorithm);
                reader.Read(out scheduledSetupSpanAlgorithm);
                reader.Read(out scheduledTransferSpanAlgorithm);
                reader.Read(out decimal obsolete_sameSetupHeadstartMultiplier);
                reader.Read(out m_activitySetupEfficiencyMultiplier);
                reader.Read(out stage);
                reader.Read(out setupIncludedBackwardsCompatibility);
                reader.Read(out int val);
                capacityType = (InternalResourceDefs.capacityTypes)val;
                reader.Read(out standardHourlyCost);
                reader.Read(out setupPerformance);
                reader.Read(out runPerformance);
                reader.Read(out postProcessingPerformance);
                reader.Read(out scrapPerformance);
                reader.Read(out setupsDone);
                reader.Read(out runsDone);
                reader.Read(out postProcessesDone);
                reader.Read(out scrapsDone);
                reader.Read(out activitiesFinished);

                reader.Read(out bufferSpan);
                reader.Read(out headStartSpan);
                reader.Read(out TimeSpan maxSameSetupSpan);
                reader.Read(out setupSpanTicks);
                reader.Read(out overlappingOnlineIntervals);
                reader.Read(out val);
                m_batchType = (MainResourceDefs.batchType)val;
                reader.Read(out m_batchVolume);
                ResetBatchIfOutOfBounds();

                resourceCapacityIntervals = new ResourceCapacityIntervalsCollection(reader);
                capacityProfile = new CapacityProfile(reader, resourceCapacityIntervals);
                m_resultantCapacity = new ResourceCapacityIntervalList(reader);
                internalResourceFlags = new BoolVector32(reader);

                referenceInfo = new ReferenceInfo();
                m_normalDispatchDefinitionId = new BaseId(reader);
                m_experimentalDispatchDefinitionIds[0] = new BaseId(reader);
                m_experimentalDispatchDefinitionIds[1] = m_experimentalDispatchDefinitionIds[0];
                m_experimentalDispatchDefinitionIds[2] = m_experimentalDispatchDefinitionIds[0];
                m_experimentalDispatchDefinitionIds[3] = m_experimentalDispatchDefinitionIds[0];
                referenceInfo.capabilityIds = new BaseIdList(reader);
                referenceInfo.ciList = new BaseIdList(reader);
                referenceInfo.rciList = new BaseIdList(reader);

                //Lookup tables
                bool haveTable;
                reader.Read(out haveTable);
                if (haveTable)
                {
                    new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.attributeCodeTableId = new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.fromToRangeSetId = new BaseId(reader);
                }

                m_backwardsCompatibilityProductRules = new ProductRules(reader);

                bool isTank;
                reader.Read(out isTank);
                if (isTank)
                {
                    m_tank = new ResourceWarehouse(reader, a_idGen);
                }

                reader.Read(out m_minNbrOfPeople);
                reader.Read(out m_autoJoinSpan);
            }

            #region 12058
            else if (reader.VersionNumber >= 12058)
            {
                reader.Read(out string currentProductSetup);
                reader.Read(out string currentSetupCode);
                reader.Read(out decimal currentSetupNumber);
                reader.Read(out cycleEfficiencyMultiplier);
                reader.Read(out string cycleSpanAlgorithm);
                reader.Read(out overtimeHourlyCost);
                reader.Read(out scheduledRunSpanAlgorithm);
                reader.Read(out scheduledSetupSpanAlgorithm);
                reader.Read(out scheduledTransferSpanAlgorithm);
                reader.Read(out decimal obsolete_sameSetupHeadstartMultiplier);
                reader.Read(out m_activitySetupEfficiencyMultiplier);
                reader.Read(out stage);
                reader.Read(out setupIncludedBackwardsCompatibility);
                reader.Read(out int val);
                capacityType = (InternalResourceDefs.capacityTypes)val;
                reader.Read(out standardHourlyCost);
                reader.Read(out setupPerformance);
                reader.Read(out runPerformance);
                reader.Read(out postProcessingPerformance);
                reader.Read(out scrapPerformance);
                reader.Read(out setupsDone);
                reader.Read(out runsDone);
                reader.Read(out postProcessesDone);
                reader.Read(out scrapsDone);
                reader.Read(out activitiesFinished);

                reader.Read(out bufferSpan);
                reader.Read(out headStartSpan);
                reader.Read(out TimeSpan maxSameSetupSpan);
                reader.Read(out setupSpanTicks);
                reader.Read(out overlappingOnlineIntervals);
                reader.Read(out val);
                m_batchType = (MainResourceDefs.batchType)val;
                reader.Read(out m_batchVolume);
                ResetBatchIfOutOfBounds();

                resourceCapacityIntervals = new ResourceCapacityIntervalsCollection(reader);
                capacityProfile = new CapacityProfile(reader, resourceCapacityIntervals);
                m_resultantCapacity = new ResourceCapacityIntervalList(reader);
                internalResourceFlags = new BoolVector32(reader);

                referenceInfo = new ReferenceInfo();
                m_normalDispatchDefinitionId = new BaseId(reader);
                m_experimentalDispatchDefinitionIds[0] = new BaseId(reader);
                m_experimentalDispatchDefinitionIds[1] = m_experimentalDispatchDefinitionIds[0];
                m_experimentalDispatchDefinitionIds[2] = m_experimentalDispatchDefinitionIds[0];
                m_experimentalDispatchDefinitionIds[3] = m_experimentalDispatchDefinitionIds[0];
                referenceInfo.capabilityIds = new BaseIdList(reader);
                referenceInfo.ciList = new BaseIdList(reader);
                referenceInfo.rciList = new BaseIdList(reader);

                //Lookup tables
                bool haveTable;
                reader.Read(out haveTable);
                if (haveTable)
                {
                    new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.attributeCodeTableId = new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.fromToRangeSetId = new BaseId(reader);
                }

                m_backwardsCompatibilityProductRules = new ProductRules(reader);

                bool isTank;
                reader.Read(out isTank);
                if (isTank)
                {
                    m_tank = new ResourceWarehouse(reader, a_idGen);
                }

                reader.Read(out m_minNbrOfPeople);
                reader.Read(out m_autoJoinSpan);
            }
            #endregion
            #region 12038
            else if (reader.VersionNumber >= 12038)
            {
                reader.Read(out string currentProductSetup);
                reader.Read(out string currentSetupCode);
                reader.Read(out decimal currentSetupNumber);
                reader.Read(out cycleEfficiencyMultiplier);
                reader.Read(out string cycleSpanAlgorithm);
                reader.Read(out overtimeHourlyCost);
                reader.Read(out int _); // This use to be converted into processingStatus, but we removed that property
                reader.Read(out scheduledRunSpanAlgorithm);
                reader.Read(out scheduledSetupSpanAlgorithm);
                reader.Read(out scheduledTransferSpanAlgorithm);
                reader.Read(out decimal obsolete_sameSetupHeadstartMultiplier);
                reader.Read(out m_activitySetupEfficiencyMultiplier);
                reader.Read(out stage);
                reader.Read(out setupIncludedBackwardsCompatibility);
                reader.Read(out int val);
                capacityType = (InternalResourceDefs.capacityTypes)val;
                reader.Read(out standardHourlyCost);
                reader.Read(out setupPerformance);
                reader.Read(out runPerformance);
                reader.Read(out postProcessingPerformance);
                reader.Read(out scrapPerformance);
                reader.Read(out setupsDone);
                reader.Read(out runsDone);
                reader.Read(out postProcessesDone);
                reader.Read(out scrapsDone);
                reader.Read(out activitiesFinished);

                reader.Read(out bufferSpan);
                reader.Read(out headStartSpan);
                reader.Read(out TimeSpan maxSameSetupSpan);
                reader.Read(out setupSpanTicks);
                reader.Read(out overlappingOnlineIntervals);
                reader.Read(out val);
                m_batchType = (MainResourceDefs.batchType)val;
                reader.Read(out m_batchVolume);
                ResetBatchIfOutOfBounds();

                resourceCapacityIntervals = new ResourceCapacityIntervalsCollection(reader);
                capacityProfile = new CapacityProfile(reader, resourceCapacityIntervals);
                m_resultantCapacity = new ResourceCapacityIntervalList(reader);
                internalResourceFlags = new BoolVector32(reader);

                referenceInfo = new ReferenceInfo();
                m_normalDispatchDefinitionId = new BaseId(reader);
                m_experimentalDispatchDefinitionIds[0] = new BaseId(reader);
                m_experimentalDispatchDefinitionIds[1] = m_experimentalDispatchDefinitionIds[0];
                m_experimentalDispatchDefinitionIds[2] = m_experimentalDispatchDefinitionIds[0];
                m_experimentalDispatchDefinitionIds[3] = m_experimentalDispatchDefinitionIds[0];
                referenceInfo.capabilityIds = new BaseIdList(reader);
                referenceInfo.ciList = new BaseIdList(reader);
                referenceInfo.rciList = new BaseIdList(reader);

                //Lookup tables
                bool haveTable;
                reader.Read(out haveTable);
                if (haveTable)
                {
                    new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.attributeCodeTableId = new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.fromToRangeSetId = new BaseId(reader);
                }

                m_backwardsCompatibilityProductRules = new ProductRules(reader);

                bool isTank;
                reader.Read(out isTank);
                if (isTank)
                {
                    m_tank = new ResourceWarehouse(reader, a_idGen);
                }

                reader.Read(out m_minNbrOfPeople);
                reader.Read(out m_autoJoinSpan);
            }
            #endregion
            #region 12000
            else if (reader.VersionNumber >= 12002)
            {
                reader.Read(out string currentProductSetup);
                reader.Read(out string currentSetupCode);
                reader.Read(out decimal currentSetupNumber);
                reader.Read(out cycleEfficiencyMultiplier);
                reader.Read(out string cycleSpanAlgorithm);
                reader.Read(out int _); //overlap
                reader.Read(out overtimeHourlyCost);
                reader.Read(out int _); // This use to be converted into processingStatus, but we removed that property
                reader.Read(out scheduledRunSpanAlgorithm);
                reader.Read(out scheduledSetupSpanAlgorithm);
                reader.Read(out scheduledTransferSpanAlgorithm);
                reader.Read(out decimal obsolete_sameSetupHeadstartMultiplier);
                reader.Read(out m_activitySetupEfficiencyMultiplier);
                reader.Read(out stage);
                reader.Read(out setupIncludedBackwardsCompatibility);
                reader.Read(out int val);
                capacityType = (InternalResourceDefs.capacityTypes)val;
                reader.Read(out standardHourlyCost);
                reader.Read(out setupPerformance);
                reader.Read(out runPerformance);
                reader.Read(out postProcessingPerformance);
                reader.Read(out scrapPerformance);
                reader.Read(out setupsDone);
                reader.Read(out runsDone);
                reader.Read(out postProcessesDone);
                reader.Read(out scrapsDone);
                reader.Read(out activitiesFinished);
                reader.Read(out val); //roles

                reader.Read(out bufferSpan);
                reader.Read(out headStartSpan);
                reader.Read(out TimeSpan maxSameSetupSpan);
                reader.Read(out setupSpanTicks);
                reader.Read(out overlappingOnlineIntervals);
                reader.Read(out val);
                m_batchType = (MainResourceDefs.batchType)val;
                reader.Read(out m_batchVolume);
                ResetBatchIfOutOfBounds();

                resourceCapacityIntervals = new ResourceCapacityIntervalsCollection(reader);
                capacityProfile = new CapacityProfile(reader, resourceCapacityIntervals);
                m_resultantCapacity = new ResourceCapacityIntervalList(reader);
                internalResourceFlags = new BoolVector32(reader);

                referenceInfo = new ReferenceInfo();
                m_normalDispatchDefinitionId = new BaseId(reader);
                m_experimentalDispatchDefinitionIds[0] = new BaseId(reader);
                m_experimentalDispatchDefinitionIds[1] = m_experimentalDispatchDefinitionIds[0];
                m_experimentalDispatchDefinitionIds[2] = m_experimentalDispatchDefinitionIds[0];
                m_experimentalDispatchDefinitionIds[3] = m_experimentalDispatchDefinitionIds[0];
                referenceInfo.capabilityIds = new BaseIdList(reader);
                referenceInfo.ciList = new BaseIdList(reader);
                referenceInfo.rciList = new BaseIdList(reader);

                //Lookup tables
                bool haveTable;
                reader.Read(out haveTable);
                if (haveTable)
                {
                    new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.attributeCodeTableId = new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.fromToRangeSetId = new BaseId(reader);
                }

                m_backwardsCompatibilityProductRules = new ProductRules(reader);

                bool isTank;
                reader.Read(out isTank);
                if (isTank)
                {
                    m_tank = new ResourceWarehouse(reader, a_idGen);
                }

                reader.Read(out m_minNbrOfPeople);
                reader.Read(out m_autoJoinSpan);
            }
            #endregion
            #region 683
            else if (reader.VersionNumber >= 683)
            {
                reader.Read(out string currentProductSetup);
                reader.Read(out string currentSetupCode);
                reader.Read(out decimal currentSetupNumber);
                reader.Read(out cycleEfficiencyMultiplier);
                reader.Read(out string cycleSpanAlgorithm);
                reader.Read(out int _); //overlap
                reader.Read(out overtimeHourlyCost);
                reader.Read(out int _); // This use to be converted into processingStatus, but we removed that property
                reader.Read(out scheduledRunSpanAlgorithm);
                reader.Read(out scheduledSetupSpanAlgorithm);
                reader.Read(out scheduledTransferSpanAlgorithm);
                reader.Read(out decimal obsolete_sameSetupHeadstartMultiplier);
                reader.Read(out m_activitySetupEfficiencyMultiplier);
                reader.Read(out stage);
                reader.Read(out setupIncludedBackwardsCompatibility);
                reader.Read(out int val);
                capacityType = (InternalResourceDefs.capacityTypes)val;
                reader.Read(out standardHourlyCost);
                reader.Read(out setupPerformance);
                reader.Read(out runPerformance);
                reader.Read(out postProcessingPerformance);
                reader.Read(out scrapPerformance);
                reader.Read(out setupsDone);
                reader.Read(out runsDone);
                reader.Read(out postProcessesDone);
                reader.Read(out scrapsDone);
                reader.Read(out activitiesFinished);
                reader.Read(out val); //roles

                reader.Read(out bufferSpan);
                reader.Read(out headStartSpan);
                reader.Read(out TimeSpan maxSameSetupSpan);
                reader.Read(out setupSpanTicks);
                reader.Read(out overlappingOnlineIntervals);
                reader.Read(out val);
                m_batchType = (MainResourceDefs.batchType)val;
                reader.Read(out m_batchVolume);
                ResetBatchIfOutOfBounds();

                resourceCapacityIntervals = new ResourceCapacityIntervalsCollection(reader);
                capacityProfile = new CapacityProfile(reader, resourceCapacityIntervals);
                m_resultantCapacity = new ResourceCapacityIntervalList(reader);
                internalResourceFlags = new BoolVector32(reader);

                referenceInfo = new ReferenceInfo();
                referenceInfo.dispatcherDefinitionId = new BaseId(reader);
                referenceInfo.experimentalDispatcherDefinitionId = new BaseId(reader);
                referenceInfo.capabilityIds = new BaseIdList(reader);
                referenceInfo.ciList = new BaseIdList(reader);
                referenceInfo.rciList = new BaseIdList(reader);

                //Lookup tables
                bool haveTable;
                reader.Read(out haveTable);
                if (haveTable)
                {
                    new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.attributeCodeTableId = new BaseId(reader);
                }

                reader.Read(out haveTable);
                if (haveTable)
                {
                    referenceInfo.fromToRangeSetId = new BaseId(reader);
                }

                m_backwardsCompatibilityProductRules = new ProductRules(reader);

                bool isTank;
                reader.Read(out isTank);
                if (isTank)
                {
                    m_tank = new ResourceWarehouse(reader, a_idGen);
                }

                reader.Read(out m_minNbrOfPeople);
                reader.Read(out m_autoJoinSpan);
            }
            #endregion 683

            //SetupInclusion Backwards Compatibility
            //For backwards compatibility, set to use Resource setup unless the setup inclusion was set to 4 (Never)
            UseResourceSetupTime = setupIncludedBackwardsCompatibility != 4;

            if (reader.VersionNumber >= 12000)
            {
                UseSequencedSetupTime = setupIncludedBackwardsCompatibility == 9;
            }
            else
            {
                if (setupIncludedBackwardsCompatibility == 9)
                {
                    //Unused was obsolete and was removed in v12.  
                    UseSequencedSetupTime = false;
                }
                else if (setupIncludedBackwardsCompatibility == 10)
                {
                    //Before Unused was removed in v12, 10 was (UseOperationAttributes). 
                    //It is now 9
                    UseSequencedSetupTime = true;
                }
            }
        }



        if (reader.VersionNumber < 12002)
        {
            m_normalDispatchDefinitionId = referenceInfo.dispatcherDefinitionId;
            m_experimentalDispatchDefinitionIds[0] = referenceInfo.experimentalDispatcherDefinitionId;
            m_experimentalDispatchDefinitionIds[1] = m_experimentalDispatchDefinitionIds[0];
            m_experimentalDispatchDefinitionIds[2] = m_experimentalDispatchDefinitionIds[0];
            m_experimentalDispatchDefinitionIds[3] = m_experimentalDispatchDefinitionIds[0];
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(cycleEfficiencyMultiplier);
        writer.Write(overtimeHourlyCost);
        writer.Write(scheduledRunSpanAlgorithm);
        writer.Write(scheduledSetupSpanAlgorithm);
        writer.Write(scheduledTransferSpanAlgorithm);
        writer.Write(m_activitySetupEfficiencyMultiplier);
        writer.Write(m_changeoverSetupEfficiencyMultiplier);
        writer.Write(stage);
        writer.Write((int)capacityType);
        writer.Write(standardHourlyCost);
        writer.Write(setupPerformance);
        writer.Write(runPerformance);
        writer.Write(postProcessingPerformance);
        writer.Write(scrapPerformance);
        writer.Write(setupsDone);
        writer.Write(runsDone);
        writer.Write(postProcessesDone);
        writer.Write(scrapsDone);
        writer.Write(activitiesFinished);

        writer.Write(bufferSpan);
        writer.Write(headStartSpan);
        writer.Write(setupSpanTicks);
        writer.Write(overlappingOnlineIntervals);

        writer.Write((int)m_batchType);
        writer.Write(m_batchVolume);

        resourceCapacityIntervals.Serialize(writer);
        capacityProfile.Serialize(writer);
        m_resultantCapacity.Serialize(writer);
        internalResourceFlags.Serialize(writer);

        m_normalDispatchDefinitionId.Serialize(writer);
        foreach (BaseId experimentalDispatchDefinitionId in m_experimentalDispatchDefinitionIds)
        {
            experimentalDispatchDefinitionId.Serialize(writer);
        }

        BaseIdList capabilityIds = new ();
        for (int i = 0; i < CapabilityCount; i++)
        {
            capabilityIds.Add(GetCapabilityByIndex(i).Id);
        }

        capabilityIds.Serialize(writer);

        BaseIdList ciList = new ();

        for (int i = 0; i < capacityIntervals.Count; i++)
        {
            ciList.Add(capacityIntervals[i].Id);
        }

        ciList.Serialize(writer);

        BaseIdList rciList = new ();
        for (int i = 0; i < recurringCapacityIntervals.Count; i++)
        {
            rciList.Add(recurringCapacityIntervals[i].Id);
        }

        rciList.Serialize(writer);

        //Lookup tables
        writer.Write(attributeCodeTable != null);
        if (attributeCodeTable != null)
        {
            attributeCodeTable.Id.Serialize(writer);
        }

        if (fromToRanges != null)
        {
            writer.Write(true);
            fromToRanges.Id.Serialize(writer);
        }
        else
        {
            writer.Write(false);
        }

        writer.Write(IsTank);
        if (IsTank)
        {
            m_tank.Serialize(writer);
        }

        writer.Write(m_minNbrOfPeople);
        writer.Write(m_autoJoinSpan);

        writer.Write(m_standardCleanSpanTicks);
        writer.Write(m_standardCleanoutGrade);
        writer.Write(m_priority);

        if (timeCleanoutTriggerTable != null)
        {
            timeCleanoutTriggerTable.Id.Serialize(writer);
        }
        else
        {
            BaseId.NULL_ID.Serialize(writer);
        }

        if (productionUnitsCleanoutTriggerTable != null)
        {
            productionUnitsCleanoutTriggerTable.Id.Serialize(writer);
        }
        else
        {
            BaseId.NULL_ID.Serialize(writer);
        }

        if (operationCountCleanoutTriggerTable != null)
        {
            operationCountCleanoutTriggerTable.Id.Serialize(writer);
        }
        else
        {
            BaseId.NULL_ID.Serialize(writer);
        }

        writer.Write(m_compatibilityTables.Count);
        foreach (CompatibilityCodeTable codeTable in m_compatibilityTables)
        {
            codeTable.Id.Serialize(writer);
        }

        writer.Write(m_resourceSetupCost);
        writer.Write(m_resourceCleanoutCost);

        if (ItemCleanoutTable != null)
        {
            m_itemCleanoutTable.Id.Serialize(writer);
        }
        else
        {
            BaseId.NULL_ID.Serialize(writer);
        }
    }

    public new const int UNIQUE_ID = 321;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    [Browsable(false)]
    private ProductRules m_backwardsCompatibilityProductRules;

    private ReferenceInfo referenceInfo;

    private class ReferenceInfo
    {
        internal BaseId dispatcherDefinitionId;
        internal BaseId experimentalDispatcherDefinitionId;
        internal BaseIdList capabilityIds = new ();
        internal BaseIdList ciList = new ();
        internal BaseIdList rciList = new ();
        internal BaseId attributeCodeTableId = BaseId.NULL_ID;
        internal BaseId timeCleanoutTriggerTableId = BaseId.NULL_ID;
        internal BaseId productionUnitsCleanoutTriggerTableId = BaseId.NULL_ID;
        internal BaseId operationCountCleanoutTriggerTableId = BaseId.NULL_ID;
        internal BaseId fromToRangeSetId = BaseId.NULL_ID;
        internal BaseId m_itemCleanoutTableId = BaseId.NULL_ID;

        public List<BaseId> CompatibilityCodeTableIds = new ();
    }

    internal void RestoreReferences(CustomerManager a_cm,
                                    CapabilityManager a_allCapabilities,
                                    CapacityIntervalManager a_allCapacityIntervals,
                                    RecurringCapacityIntervalManager a_allRcis,
                                    DispatcherDefinitionManager a_dispatcherDefs,
                                    ItemManager a_itemManager,
                                    ScenarioDetail a_sd,
                                    BaseIdGenerator a_idGen,
                                    ISystemLogger a_errorReporter)
    {
        base.RestoreReferences(a_sd.CellManager);

        DispatcherDefinition def = a_dispatcherDefs.GetById(m_normalDispatchDefinitionId);
        Dispatcher = def.CreateDispatcher();

        ExperimentalDispatcherOne = a_dispatcherDefs.GetById(m_experimentalDispatchDefinitionIds[0]).CreateDispatcher();
        ExperimentalDispatcherTwo = a_dispatcherDefs.GetById(m_experimentalDispatchDefinitionIds[1]).CreateDispatcher();
        ExperimentalDispatcherThree = a_dispatcherDefs.GetById(m_experimentalDispatchDefinitionIds[2]).CreateDispatcher();
        ExperimentalDispatcherFour = a_dispatcherDefs.GetById(m_experimentalDispatchDefinitionIds[3]).CreateDispatcher();

        foreach (BaseId capId in referenceInfo.capabilityIds)
        {
            Capability c = a_allCapabilities.GetById(capId);
            AddCapability(c);
            c.Resources.Add(this);
        }

        foreach (BaseId ciId in referenceInfo.ciList)
        {
            CapacityInterval c = a_allCapacityIntervals.GetById(ciId);
            capacityIntervals.Add(c);
            c.CalendarResources.Add(this);
        }

        foreach (BaseId rciId in referenceInfo.rciList)
        {
            RecurringCapacityInterval c = a_allRcis.GetById(rciId);
            recurringCapacityIntervals.Add(c);
            c.CalendarResources.Add(this);
        }

        a_allRcis.RestoreReferences(a_sd);

        if (referenceInfo.attributeCodeTableId != BaseId.NULL_ID)
        {
            attributeCodeTable = a_sd.AttributeCodeTableManager.Find(referenceInfo.attributeCodeTableId);
        }

        if (referenceInfo.m_itemCleanoutTableId != BaseId.NULL_ID)
        {
            m_itemCleanoutTable = a_sd.ItemCleanoutTableManager.GetById(referenceInfo.m_itemCleanoutTableId);
        }

        if (referenceInfo.timeCleanoutTriggerTableId != BaseId.NULL_ID)
        {
            timeCleanoutTriggerTable = (TimeCleanoutTriggerTable)a_sd.CleanoutTriggerTableManager.GetById(referenceInfo.timeCleanoutTriggerTableId);
        }

        if (referenceInfo.productionUnitsCleanoutTriggerTableId != BaseId.NULL_ID)
        {
            productionUnitsCleanoutTriggerTable = (ProductionUnitsCleanoutTriggerTable)a_sd.CleanoutTriggerTableManager.GetById(referenceInfo.productionUnitsCleanoutTriggerTableId);
        }

        if (referenceInfo.operationCountCleanoutTriggerTableId != BaseId.NULL_ID)
        {
            operationCountCleanoutTriggerTable = (OperationCountCleanoutTriggerTable)a_sd.CleanoutTriggerTableManager.GetById(referenceInfo.operationCountCleanoutTriggerTableId);
        }

        if (referenceInfo.fromToRangeSetId != BaseId.NULL_ID)
        {
            fromToRanges = a_sd.FromRangeSetManager.Find(referenceInfo.fromToRangeSetId);
        }

        // [TANK_CODE] 
        if (m_tank != null)
        {
            m_tank.TankResource = this;
            m_tank.RestoreReferences(a_sd, a_cm, a_itemManager, a_errorReporter);
        }
        
        foreach (BaseId tableId in referenceInfo.CompatibilityCodeTableIds)
        {
            CompatibilityCodeTable table = a_sd.CompatibilityCodeTableManager.GetById(tableId);
            m_compatibilityTables.Add(table);
        }

        //Backwards Compatibility code
        if (m_backwardsCompatibilityProductRules != null)
        {
            m_backwardsCompatibilityProductRules.RestoreReferences(a_itemManager);

            foreach (ProductRules.ProductRuleAndKey prAndKey in m_backwardsCompatibilityProductRules)
            {
                foreach (ProductRule productRule in prAndKey.ProductRules)
                {
                    productRule.SetBackwardsCompatibilityResourceId(Id);
                    a_sd.ProductRuleManager.AddForBackwardsCompatibility(productRule);
                }
            }
        }

        referenceInfo = null;
        
        RegenerateCapacityProfile(a_sd.GetPlanningHorizonEndTicks(), false);
    }

    public override void AfterRestoreReferences_2(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        base.AfterRestoreReferences_2(serializationVersionNbr, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
        m_capabilityManager.ReinsertObjects(false);
    }
    #endregion

    #region Construction
    protected CapacityProfile capacityProfile;

    /// <summary>
    /// This is used for maintaining CustomOrderedListOptimized, don't use to instantiate an instance for other use.
    /// </summary>
    internal InternalResource() { }

    /// <summary>
    /// Returns the amount of Online time in the specified interval taking the specified efficiency into account.
    /// </summary>
    internal TimeSpan GetOnlineCapacityInInterval(DateTime startInclusive, DateTime endInclusive)
    {
        TimeSpan bucketLength = endInclusive - startInclusive;
        return capacityProfile.GetBucketedCapacity(startInclusive, endInclusive, bucketLength)[0];
    }

    private readonly ReadyActivitiesDispatcher[] m_experimentalDispatchers = new ReadyActivitiesDispatcher[c_numberOfExperimentalDispatcherDefinitions];

    public InternalResource(BaseId id, Department w, ReadyActivitiesDispatcher dispatcher, ShopViewResourceOptions resourceOptions)
        : base(id, w, resourceOptions)
    {
        Dispatcher = dispatcher;
        ExperimentalDispatcherOne = dispatcher;
        ExperimentalDispatcherTwo = dispatcher;
        ExperimentalDispatcherThree = dispatcher;
        ExperimentalDispatcherFour = dispatcher;

        capacityProfile = new CapacityProfile(ResourceCapacityIntervals);
    }

    public InternalResource(InternalResource origRes, BaseId newId, ScenarioDetail aSD)
        : base(origRes, newId)
    {
        Common.Cloning.PrimitiveCloning.PrimitiveClone(origRes,
            this,
            typeof(InternalResource),
            Common.Cloning.PrimitiveCloning.Depth.Shallow,
            Common.Cloning.PrimitiveCloning.OtherIncludedTypes.All);

        //Clear cached values from original resource
        InitialCapacityProfileRegenerationDone = false;

        Dispatcher = (ReadyActivitiesDispatcher)origRes.Dispatcher.Clone();
        ExperimentalDispatcherOne = (ReadyActivitiesDispatcher)origRes.ExperimentalDispatcherOne.Clone();
        ExperimentalDispatcherTwo = (ReadyActivitiesDispatcher)origRes.ExperimentalDispatcherTwo.Clone();
        ExperimentalDispatcherThree = (ReadyActivitiesDispatcher)origRes.ExperimentalDispatcherThree.Clone();
        ExperimentalDispatcherFour = (ReadyActivitiesDispatcher)origRes.ExperimentalDispatcherFour.Clone();

        //Each Capacity Interval must reference this resource too
        capacityIntervals = new CapacityIntervalsCollection();

        for (int capIntI = 0; capIntI < origRes.CapacityIntervals.Count; capIntI++)
        {
            CapacityInterval c = origRes.CapacityIntervals[capIntI];

            CapacityIntervals.Add(c);
            c.Add(this);
        }

        recurringCapacityIntervals = new RecurringCapacityIntervalsCollection();
        for (int recCapIntI = 0; recCapIntI < origRes.RecurringCapacityIntervals.Count; recCapIntI++)
        {
            RecurringCapacityInterval rci = origRes.RecurringCapacityIntervals[recCapIntI];

            RecurringCapacityIntervals.Add(rci);
            rci.Add(this);
        }

        capacityProfile = new CapacityProfile(ResourceCapacityIntervals);

        RegenerateCapacityProfile(aSD.GetPlanningHorizonEndTicks(), true);

        CreateNewCapabilityManager();
        //Add the Capabilities from the old resource to the new resource
        for (int i = 0; i < origRes.CapabilityCount; i++)
        {
            Capability c = origRes.GetCapabilityByIndex(i);

            AddCapability(c);
            c.AddResourceAssociation(this);
        }

        foreach (CompatibilityCodeTable origResCompatibilityTable in origRes.CompatibilityTables)
        {
            m_compatibilityTables.Add(origResCompatibilityTable);
        }
    }
    #endregion

    #region Transmission Functionality
    /// <summary>
    /// </summary>
    /// <param name="a_internalResourceT"></param>
    /// <param name="a_w"></param>
    /// <returns>Whether any updates that require a time adjustment have occurred.</returns>
    internal bool Update(PT.ERPTransmissions.InternalResource a_internalResourceT, UserFieldDefinitionManager a_udfManager, Department a_w, ScenarioDetail a_sd, PTTransmission t, ISystemLogger a_errorReporter, IScenarioDataChanges a_dataChanges)
    {
        bool updated = base.Update(a_udfManager, a_internalResourceT, a_w, t, a_dataChanges);

        if (a_internalResourceT.BufferSpanSet && a_internalResourceT.BufferSpan != BufferSpan)
        {
            updated = true;
            BufferSpan = a_internalResourceT.BufferSpan;
        }

        if (a_internalResourceT.CapacityTypeSet && a_internalResourceT.CapacityType != CapacityType)
        {
            updated = true;
            CapacityType = a_internalResourceT.CapacityType;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_internalResourceT.ConsecutiveSetupTimesSet && a_internalResourceT.ConsecutiveSetupTimes != ConsecutiveSetupTimes)
        {
            updated = true;
            ConsecutiveSetupTimes = a_internalResourceT.ConsecutiveSetupTimes;
        }

        if (a_internalResourceT.CycleEfficiencyMultiplierSet && a_internalResourceT.CycleEfficiencyMultiplier != CycleEfficiencyMultiplier)
        {
            updated = true;
            CycleEfficiencyMultiplier = a_internalResourceT.CycleEfficiencyMultiplier;
            a_dataChanges.FlagProductionChanges(Id);
        }

        if (a_internalResourceT.HeadStartSpanSet && a_internalResourceT.HeadStartSpan != HeadStartSpan)
        {
            updated = true;
            HeadStartSpan = a_internalResourceT.HeadStartSpan;
        }

        if (a_internalResourceT.ActivitySetupEfficiencyMultiplierSet && a_internalResourceT.ActivitySetupEfficiencyMultiplier != ActivitySetupEfficiencyMultiplier)
        {
            updated = true;
            ActivitySetupEfficiencyMultiplier = a_internalResourceT.ActivitySetupEfficiencyMultiplier;
            a_dataChanges.FlagProductionChanges(Id);
        }

        if (a_internalResourceT.ChangeoverSetupEfficiencyMultiplierSet && a_internalResourceT.ChangeoverSetupEfficiencyMultiplier != ChangeoverSetupEfficiencyMultiplier)
        {
            updated = true;
            ChangeoverSetupEfficiencyMultiplier = a_internalResourceT.ChangeoverSetupEfficiencyMultiplier;
            a_dataChanges.FlagProductionChanges(Id);
        }
        
        if (a_internalResourceT.SetupSpanSet && a_internalResourceT.SetupSpan != SetupSpan)
        {
            updated = true;
            SetupSpan = a_internalResourceT.SetupSpan;
            a_dataChanges.FlagProductionChanges(Id);
        }

        if (a_internalResourceT.TransferSpanSet && a_internalResourceT.TransferSpan != TransferSpan)
        {
            updated = true;
            TransferSpan = a_internalResourceT.TransferSpan;
            a_dataChanges.FlagProductionChanges(Id);
        }

        if (a_internalResourceT.BatchTypeSet && m_batchType != a_internalResourceT.BatchType)
        {
            updated = true;
            m_batchType = a_internalResourceT.BatchType;
        }

        if (a_internalResourceT.BatchVolumeSet && BatchVolume != a_internalResourceT.BatchVolume)
        {
            updated = true;
            BatchVolume = a_internalResourceT.BatchVolume;
        }

        if (a_internalResourceT.OmitSetupOnFirstActivitySet && a_internalResourceT.OmitSetupOnFirstActivity != OmitSetupOnFirstActivity)
        {
            updated = true;
            OmitSetupOnFirstActivity = a_internalResourceT.OmitSetupOnFirstActivity;
            a_dataChanges.FlagProductionChanges(Id);
        }

        if (a_internalResourceT.OmitSetupOnFirstActivityInShiftSet && a_internalResourceT.OmitSetupOnFirstActivityInShift != OmitSetupOnFirstActivityInShift)
        {
            updated = true;
            OmitSetupOnFirstActivityInShift = a_internalResourceT.OmitSetupOnFirstActivityInShift;
            a_dataChanges.FlagProductionChanges(Id);
        }

        if (a_internalResourceT.CanOffloadSet)
        {
            CanOffload = a_internalResourceT.CanOffload;
            updated = true;
        }

        if (a_internalResourceT.CanPreemptMaterialsSet)
        {
            CanPreemptMaterials = a_internalResourceT.CanPreemptMaterials;
            updated = true;
        }

        if (a_internalResourceT.CanPreemptPredecessorsSet)
        {
            CanPreemptPredecessors = a_internalResourceT.CanPreemptPredecessors;
            updated = true;
        }

        if (a_internalResourceT.CanWorkOvertimeSet)
        {
            CanWorkOvertime = a_internalResourceT.CanWorkOvertime;
            updated = true;
        }

        if (a_internalResourceT.OvertimeHourlyCostSet)
        {
            OvertimeHourlyCost = a_internalResourceT.OvertimeHourlyCost;
            updated = true;
        }

        if (a_internalResourceT.DiscontinueSameCellSchedulingSet)
        {
            DiscontinueSameCellScheduling = a_internalResourceT.DiscontinueSameCellScheduling;
            updated = true;
        }

        if (a_internalResourceT.ScheduledRunSpanAlgorithmSet)
        {
            updated = true;
            ScheduledRunSpanAlgorithm = a_internalResourceT.ScheduledRunSpanAlgorithm;
        }

        if (a_internalResourceT.ScheduledSetupSpanAlgorithmSet)
        {
            updated = true;
            ScheduledSetupSpanAlgorithm = a_internalResourceT.ScheduledSetupSpanAlgorithm;
        }

        if (a_internalResourceT.ScheduledTransferSpanAlgorithmSet)
        {
            updated = true;
            ScheduledTransferSpanAlgorithm = a_internalResourceT.ScheduledTransferSpanAlgorithm;
        }

        if (a_internalResourceT.StageSet)
        {
            updated = true;
            Stage = a_internalResourceT.Stage;
        }

        if (a_internalResourceT.StandardHourlyCostSet)
        {
            StandardHourlyCost = a_internalResourceT.StandardHourlyCost;
            updated = true;
        }

        if (a_internalResourceT.IsTank && CapacityType != InternalResourceDefs.capacityTypes.SingleTasking)
        {
            throw new PTValidationException("2472", new object[] { Name, CapacityType.ToString(), InternalResourceDefs.capacityTypes.SingleTasking.ToString() });
        }

        if (a_internalResourceT.StandardHourlyCostSet)
        {
            StandardHourlyCost = a_internalResourceT.StandardHourlyCost;
            updated = true;
        }

        if (a_internalResourceT.IsTankSet && a_internalResourceT.IsTank != IsTank)
        {
            if (a_internalResourceT.IsTank)
            {
                MakeTank(a_sd, a_udfManager, t, a_errorReporter, a_dataChanges);
                updated = true;
                a_dataChanges.FlagProductionChanges(Id);
            }
            else
            {
                throw new PTValidationException("2473", new object[] { Name });
            }
        }
        
        if (a_internalResourceT.NormalSequencingPlanExternalIdIsSet && a_internalResourceT.NormalSequencingPlanExternalId != NormalOptimizeRule)
        {
            DispatcherDefinition dispDef = a_sd.DispatcherDefinitionManager.GetByExternalId(a_internalResourceT.NormalSequencingPlanExternalId);
            if (dispDef != null)
            {
                Dispatcher = dispDef.CreateDispatcher();
                updated = true;
            }
            else
            {
                throw new PTValidationException("2474", new object[] { a_internalResourceT.NormalSequencingPlanExternalId });
            }
        }

        if (a_internalResourceT.ExperimentalDispatcherOneExternalIdIsSet && a_internalResourceT.ExperimentalSequencingPlanOneExternalId != ExperimentalOptimizeRuleOne)
        {
            DispatcherDefinition dispDef = a_sd.DispatcherDefinitionManager.GetByExternalId(a_internalResourceT.ExperimentalSequencingPlanOneExternalId);
            if (dispDef != null)
            {
                ExperimentalDispatcherOne = dispDef.CreateDispatcher();
                updated = true;
            }
            else
            {
                throw new PTValidationException("2474", new object[] { a_internalResourceT.ExperimentalSequencingPlanOneExternalId });
            }
        }

        if (a_internalResourceT.ExperimentalDispatcherTwoExternalIdIsSet && a_internalResourceT.ExperimentalSequencingPlanTwoExternalId != ExperimentalOptimizeRuleTwo)
        {
            DispatcherDefinition dispDef = a_sd.DispatcherDefinitionManager.GetByExternalId(a_internalResourceT.ExperimentalSequencingPlanTwoExternalId);
            if (dispDef != null)
            {
                ExperimentalDispatcherTwo = dispDef.CreateDispatcher();
                updated = true;
            }
            else
            {
                throw new PTValidationException("2474", new object[] { a_internalResourceT.ExperimentalSequencingPlanTwoExternalId });
            }
        }

        if (a_internalResourceT.ExperimentalDispatcherThreeExternalIdIsSet && a_internalResourceT.ExperimentalSequencingPlanThreeExternalId != ExperimentalOptimizeRuleThree)
        {
            DispatcherDefinition dispDef = a_sd.DispatcherDefinitionManager.GetByExternalId(a_internalResourceT.ExperimentalSequencingPlanThreeExternalId);
            if (dispDef != null)
            {
                ExperimentalDispatcherThree = dispDef.CreateDispatcher();
                updated = true;
            }
            else
            {
                throw new PTValidationException("2474", new object[] { a_internalResourceT.ExperimentalSequencingPlanThreeExternalId });
            }
        }

        if (a_internalResourceT.ExperimentalDispatcherFourExternalIdIsSet && a_internalResourceT.ExperimentalSequencingPlanFourExternalId != ExperimentalOptimizeRuleFour)
        {
            DispatcherDefinition dispDef = a_sd.DispatcherDefinitionManager.GetByExternalId(a_internalResourceT.ExperimentalSequencingPlanFourExternalId);
            if (dispDef != null)
            {
                ExperimentalDispatcherFour = dispDef.CreateDispatcher();
                updated = true;
            }
            else
            {
                throw new PTValidationException("2474", new object[] { a_internalResourceT.ExperimentalSequencingPlanFourExternalId });
            }
        }

        if (a_internalResourceT.MinNbrOfPeopleSet && a_internalResourceT.MinNbrOfPeople != MinNbrOfPeople)
        {
            MinNbrOfPeople = a_internalResourceT.MinNbrOfPeople;
            updated = true;
            a_dataChanges.FlagEligibilityChanges(Id);
        }

        if (a_internalResourceT.LimitAutoJoinToSameCapacityIntervalSet && a_internalResourceT.LimitAutoJoinToSameCapacityInterval != LimitAutoJoinToSameCapacityInterval)
        {
            LimitAutoJoinToSameCapacityInterval = a_internalResourceT.LimitAutoJoinToSameCapacityInterval;
            updated = true;
        }

        if (a_internalResourceT.AutoJoinSpanSet && a_internalResourceT.AutoJoinSpan != AutoJoinSpan)
        {
            AutoJoinSpan = a_internalResourceT.AutoJoinSpan;
            updated = true;
        }

        if (a_internalResourceT.StandardCleanSpanSet && a_internalResourceT.StandardCleanSpan.Ticks != StandardCleanSpanTicks)
        {
            updated = true;
            StandardCleanSpanTicks = a_internalResourceT.StandardCleanSpan.Ticks;
        }

        if (a_internalResourceT.StandardCleanoutGradeSet)
        {
            StandardCleanoutGrade = a_internalResourceT.StandardCleanoutGrade;
            updated = true;
        }

        if (a_internalResourceT.PriorityIsSet && Priority != a_internalResourceT.Priority)
        {
            Priority = a_internalResourceT.Priority;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_internalResourceT.UseOperationSetupTimeSet && a_internalResourceT.UseOperationSetupTime != UseOperationSetupTime)
        {
            updated = true;
            UseOperationSetupTime = a_internalResourceT.UseOperationSetupTime;
            a_dataChanges.FlagProductionChanges(Id);
        }

        if (a_internalResourceT.UseResourceSetupTimeIsSet && UseResourceSetupTime != a_internalResourceT.UseResourceSetupTime)
        {
            UseResourceSetupTime = a_internalResourceT.UseResourceSetupTime;
            updated = true;
        }

        if (a_internalResourceT.UseSequencedSetupTimeIsSet && UseSequencedSetupTime != a_internalResourceT.UseSequencedSetupTime)
        {
            UseSequencedSetupTime = a_internalResourceT.UseSequencedSetupTime;
            updated = true;
        }

        if (a_internalResourceT.UseResourceCleanoutIsSet && UseResourceCleanout != a_internalResourceT.UseResourceCleanout)
        {
            UseResourceCleanout = a_internalResourceT.UseResourceCleanout;
            updated = true;
        }

        if (a_internalResourceT.UseOperationCleanoutIsSet)
        {
            UseOperationCleanout = a_internalResourceT.UseOperationCleanout;
            updated = true;
            a_dataChanges.FlagProductionChanges(Id);
        }

        if (a_internalResourceT.UseAttributeCleanoutsSet)
        {
            UseAttributeCleanouts = a_internalResourceT.UseAttributeCleanouts;
            updated = true;
            a_dataChanges.FlagProductionChanges(Id);
        }

        if (a_internalResourceT.ResourceSetupCostIsSet)
        {
            ResourceSetupCost = a_internalResourceT.ResourceSetupCost;
            updated = true;
        }

        if (a_internalResourceT.ResourceCleanoutCostIsSet)
        {
            ResourceCleanoutCost = a_internalResourceT.ResourceCleanoutCost;
            updated = true;
        }

        return updated;
    }

    internal bool Edit(ScenarioDetail a_sd, UserFieldDefinitionManager a_udfManager, ResourceEdit a_resEdit, PTTransmission a_t, ISystemLogger a_errorReporter, IScenarioDataChanges a_dataChanges)
    {
        bool updated = base.Update(a_resEdit, a_dataChanges);

        if (a_resEdit.BufferSpanSet && a_resEdit.BufferSpan != BufferSpan)
        {
            updated = true;
            BufferSpan = a_resEdit.BufferSpan;
        }

        if (a_resEdit.CapacityTypeSet && a_resEdit.CapacityType != CapacityType)
        {
            updated = true;
            CapacityType = a_resEdit.CapacityType;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_resEdit.ConsecutiveSetupTimesSet && a_resEdit.ConsecutiveSetupTimes != ConsecutiveSetupTimes)
        {
            updated = true;
            ConsecutiveSetupTimes = a_resEdit.ConsecutiveSetupTimes;
        }

        if (a_resEdit.CycleEfficiencyMultiplierSet && a_resEdit.CycleEfficiencyMultiplier != CycleEfficiencyMultiplier)
        {
            CycleEfficiencyMultiplier = a_resEdit.CycleEfficiencyMultiplier;
            a_dataChanges.FlagProductionChanges(Id);
        }

        if (a_resEdit.HeadStartSpanSet && a_resEdit.HeadStartSpan != HeadStartSpan)
        {
            updated = true;
            HeadStartSpan = a_resEdit.HeadStartSpan;
        }

        if (a_resEdit.ActivitySetupEfficiencyMultiplierSet && a_resEdit.ActivitySetupEfficiencyMultiplier != ActivitySetupEfficiencyMultiplier)
        {
            ActivitySetupEfficiencyMultiplier = a_resEdit.ActivitySetupEfficiencyMultiplier;
            a_dataChanges.FlagProductionChanges(Id);
        }

        if (a_resEdit.ChangeoverSetupEfficiencyMultiplierSet && a_resEdit.ChangeoverSetupEfficiencyMultiplier != ChangeoverSetupEfficiencyMultiplier)
        {
            ChangeoverSetupEfficiencyMultiplier = a_resEdit.ChangeoverSetupEfficiencyMultiplier;
            a_dataChanges.FlagProductionChanges(Id);
        }

        if (a_resEdit.UseAttributeCleanoutsIsSet && a_resEdit.UseAttributeCleanouts != UseAttributeCleanouts)
        {
            UseAttributeCleanouts = a_resEdit.UseAttributeCleanouts;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_resEdit.UseOperationCleanoutIsSet && a_resEdit.UseOperationCleanout != UseOperationCleanout)
        {
            UseOperationCleanout = a_resEdit.UseOperationCleanout;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_resEdit.SetupSpanSet && a_resEdit.SetupSpan != SetupSpan)
        {
            updated = true;
            SetupSpan = a_resEdit.SetupSpan;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_resEdit.TransferSpanSet && a_resEdit.TransferSpan != TransferSpan)
        {
            updated = true;
            TransferSpan = a_resEdit.TransferSpan;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_resEdit.BatchTypeSet && m_batchType != a_resEdit.BatchType)
        {
            updated = true;
            m_batchType = a_resEdit.BatchType;
        }

        if (a_resEdit.BatchVolumeSet && BatchVolume != a_resEdit.BatchVolume)
        {
            updated = true;
            BatchVolume = a_resEdit.BatchVolume;
        }

        if (a_resEdit.UseOperationSetupTimeSet && a_resEdit.UseOperationSetupTime != UseOperationSetupTime)
        {
            updated = true;
            UseOperationSetupTime = a_resEdit.UseOperationSetupTime;
            a_dataChanges.FlagConstraintChanges(BaseId.NULL_ID);
        }

        if (a_resEdit.OmitSetupOnFirstActivitySet && a_resEdit.OmitSetupOnFirstActivity != OmitSetupOnFirstActivity)
        {
            updated = true;
            OmitSetupOnFirstActivity = a_resEdit.OmitSetupOnFirstActivity;
        }

        if (a_resEdit.OmitSetupOnFirstActivityInShiftSet && a_resEdit.OmitSetupOnFirstActivityInShift != OmitSetupOnFirstActivityInShift)
        {
            updated = true;
            OmitSetupOnFirstActivityInShift = a_resEdit.OmitSetupOnFirstActivityInShift;
        }

        if (a_resEdit.CanOffloadSet && CanOffload != a_resEdit.CanOffload)
        {
            CanOffload = a_resEdit.CanOffload;
            updated = true;
        }

        if (a_resEdit.CanWorkOvertimeSet && CanWorkOvertime != a_resEdit.CanWorkOvertime)
        {
            CanWorkOvertime = a_resEdit.CanWorkOvertime;
            updated = true;
        }

        if (a_resEdit.OvertimeHourlyCostSet && OvertimeHourlyCost != a_resEdit.OvertimeHourlyCost)
        {
            OvertimeHourlyCost = a_resEdit.OvertimeHourlyCost;
            updated = true;
        }

        if (a_resEdit.StageSet && Stage != a_resEdit.Stage)
        {
            Stage = a_resEdit.Stage;
            updated = true;
        }

        if (a_resEdit.StandardHourlyCostSet && StandardHourlyCost != a_resEdit.StandardHourlyCost)
        {
            StandardHourlyCost = a_resEdit.StandardHourlyCost;
            updated = true;
        }

        if (a_resEdit.IsTankResource && CapacityType != InternalResourceDefs.capacityTypes.SingleTasking)
        {
            throw new PTValidationException("2472", new object[] { Name, CapacityType.ToString(), InternalResourceDefs.capacityTypes.SingleTasking.ToString() });
        }

        if (a_resEdit.IsTankResourceSet && a_resEdit.IsTankResource != IsTank)
        {
            if (a_resEdit.IsTankResource)
            {
                MakeTank(a_sd, a_udfManager, a_t, a_errorReporter, a_dataChanges);
                a_dataChanges.FlagConstraintChanges(Id);
                updated = true;
            }
            else
            {
                throw new PTValidationException("2473", new object[] { Name });
            }
        }
        
        if (a_resEdit.NormalSequencingPlanSet && a_resEdit.NormalSequencingPlanId != NormalDispatcherId)
        {
            DispatcherDefinition dispDef = a_sd.DispatcherDefinitionManager.GetById(a_resEdit.NormalSequencingPlanId);
            if (dispDef != null)
            {
                Dispatcher = dispDef.CreateDispatcher();
                updated = true;
            }
            else
            {
                throw new PTValidationException("2474", new object[] { a_resEdit.NormalSequencingPlanId });
            }
        }

        if (a_resEdit.ExperimentalSequencingPlanOneSet && a_resEdit.ExperimentalSequencingPlanIdOne != ExperimentalDispatcherIdOne)
        {
            DispatcherDefinition dispDef = a_sd.DispatcherDefinitionManager.GetById(a_resEdit.ExperimentalSequencingPlanIdOne);
            if (dispDef != null)
            {
                ExperimentalDispatcherOne = dispDef.CreateDispatcher();
                updated = true;
            }
            else
            {
                throw new PTValidationException("2474", new object[] { a_resEdit.ExperimentalSequencingPlanIdOne });
            }
        }

        if (a_resEdit.ExperimentalSequencingPlanTwoSet && a_resEdit.ExperimentalSequencingPlanIdTwo != ExperimentalDispatcherIdTwo)
        {
            DispatcherDefinition dispDef = a_sd.DispatcherDefinitionManager.GetById(a_resEdit.ExperimentalSequencingPlanIdTwo);
            if (dispDef != null)
            {
                ExperimentalDispatcherTwo = dispDef.CreateDispatcher();
                updated = true;
            }
            else
            {
                throw new PTValidationException("2474", new object[] { a_resEdit.ExperimentalSequencingPlanIdTwo });
            }
        }

        if (a_resEdit.ExperimentalSequencingPlanThreeSet && a_resEdit.ExperimentalSequencingPlanIdThree != ExperimentalDispatcherIdThree)
        {
            DispatcherDefinition dispDef = a_sd.DispatcherDefinitionManager.GetById(a_resEdit.ExperimentalSequencingPlanIdThree);
            if (dispDef != null)
            {
                ExperimentalDispatcherThree = dispDef.CreateDispatcher();
                updated = true;
            }
            else
            {
                throw new PTValidationException("2474", new object[] { a_resEdit.ExperimentalSequencingPlanIdThree });
            }
        }

        if (a_resEdit.ExperimentalSequencingPlanFourSet && a_resEdit.ExperimentalSequencingPlanIdFour != ExperimentalDispatcherIdFour)
        {
            DispatcherDefinition dispDef = a_sd.DispatcherDefinitionManager.GetById(a_resEdit.ExperimentalSequencingPlanIdFour);
            if (dispDef != null)
            {
                ExperimentalDispatcherFour = dispDef.CreateDispatcher();
                updated = true;
            }
            else
            {
                throw new PTValidationException("2474", new object[] { a_resEdit.ExperimentalSequencingPlanIdFour });
            }
        }

        if (a_resEdit.MinNbrOfPeopleSet && a_resEdit.MinNbrOfPeople != MinNbrOfPeople)
        {
            MinNbrOfPeople = a_resEdit.MinNbrOfPeople;
            updated = true;
        }

        if (a_resEdit.LimitAutoJoinToSameShiftSet && a_resEdit.LimitAutoJoinToSameShift != LimitAutoJoinToSameCapacityInterval)
        {
            LimitAutoJoinToSameCapacityInterval = a_resEdit.LimitAutoJoinToSameShift;
            a_dataChanges.FlagConstraintChanges(Id);
            updated = true;
        }

        if (a_resEdit.AutoJoinSpanSet && a_resEdit.AutoJoinSpan != AutoJoinSpan)
        {
            AutoJoinSpan = a_resEdit.AutoJoinSpan;
            updated = true;
        }

        if (a_resEdit.StandardCleanSpanSet && a_resEdit.StandardCleanSpan.Ticks != StandardCleanSpanTicks)
        {
            updated = true;
            StandardCleanSpanTicks = a_resEdit.StandardCleanSpan.Ticks;
        }

        if (a_resEdit.StandardCleanoutGradeIsSet && a_resEdit.StandardCleanoutGrade != StandardCleanoutGrade)
        {
            StandardCleanoutGrade = a_resEdit.StandardCleanoutGrade;
            updated = true;
        }

        if (a_resEdit.PriorityIsSet && a_resEdit.Priority != Priority)
        {
            Priority = a_resEdit.Priority;
            updated = true;
        }


        if (a_resEdit.UseResourceSetupTimeIsSet && UseResourceSetupTime != a_resEdit.UseResourceSetupTime)
        {
            UseResourceSetupTime = a_resEdit.UseResourceSetupTime;
            updated = true;
        }

        if (a_resEdit.UseSequencedSetupTimeIsSet && UseSequencedSetupTime != a_resEdit.UseSequencedSetupTime)
        {
            UseSequencedSetupTime = a_resEdit.UseSequencedSetupTime;
            updated = true;
        }

        if (a_resEdit.UseResourceCleanoutIsSet && UseResourceCleanout != a_resEdit.UseResourceCleanout)
        {
            UseResourceCleanout = a_resEdit.UseResourceCleanout;
            updated = true;
        }

        if (a_resEdit.ResourceSetupCostIsSet && ResourceSetupCost != a_resEdit.ResourceSetupCost)
        {
            ResourceSetupCost = a_resEdit.ResourceSetupCost;
            updated = true;
        }

        if (a_resEdit.ResourceCleanoutCostIsSet && ResourceCleanoutCost != a_resEdit.ResourceCleanoutCost)
        {
            ResourceCleanoutCost = a_resEdit.ResourceCleanoutCost;
            updated = true;
        }

        return updated;
    }

    protected override bool CriticalFieldChange(string propertyName)
    {
        //			string name;
        bool criticalFieldChange = base.CriticalFieldChange(propertyName);

        //			name=BufferSpan.
        //			if(name==propertyName)
        //			{
        //				criticalFieldChange=true;
        //			}
        //
        //			name=TypeDescriptor.GetDefaultProperty(CapacityType).Name;
        //			if(name==propertyName)
        //			{
        //				criticalFieldChange=true;
        //			}
        //
        //			if(ConsecutiveSetupTimes.GetType().Name==propertyName)
        //			{
        //				criticalFieldChange=true;
        //			}
        //
        //			if(CycleEfficiencyMultiplier.GetType().Name==propertyName)
        //			{
        //				criticalFieldChange=true;
        //			}
        //
        //			if(Drum.GetType().Name==propertyName)
        //			{
        //				criticalFieldChange=true;
        //			}
        //
        //			if(DurationDependsOnCapacity.GetType().Name==propertyName)
        //			{
        //				criticalFieldChange=true;
        //			}
        //
        //			if(HeadStartSpan.GetType().Name==propertyName)
        //			{
        //				criticalFieldChange=true;
        //			}
        //
        //			if(MaxSameSetupSpan.GetType().Name==propertyName)
        //			{
        //				criticalFieldChange=true;
        //			}
        //
        //			if(Overlap.GetType().Name==propertyName)
        //			{
        //				criticalFieldChange=true;
        //			}
        //
        //			if(SameSetupHeadstartMultiplier.GetType().Name==propertyName)
        //			{
        //				criticalFieldChange=true;
        //			}
        //
        //			if(SetupEfficiencyMultiplier.GetType().Name==propertyName)
        //			{
        //				criticalFieldChange=true;
        //			}
        //
        //			if(SetupIncluded.GetType().Name==propertyName)
        //			{
        //				criticalFieldChange=true;
        //			}
        //
        //			if(SetupSpan.GetType().Name==propertyName)
        //			{
        //				criticalFieldChange=true;
        //			}
        //
        //			if(TransferSpan.GetType().Name==propertyName)
        //			{
        //				criticalFieldChange=true;
        //			}
        //
        //			if(UseOperationSetupTime.GetType().Name==propertyName)
        //			{
        //				criticalFieldChange=true;
        //			}

        return criticalFieldChange;
    }
    #endregion

    internal void ValidateCapabilityLimitation(ScenarioDetail a_sd)
    {
        ResourceRequirement rr = a_sd.JobManager.GetFirstRequirementNeedingResource(this, a_sd.ProductRuleManager);
        if (rr != null)
        {
            throw new ValidationException("2476", new object[] { Name, rr.Operation.ManufacturingOrder.Job.Name, rr.Operation.ManufacturingOrder.Name, rr.Operation.Name, rr.Id.ToString() });
        }
    }

    #region Shared Properties
    private BoolVector32 internalResourceFlags;

    private long bufferSpan;

    internal long BufferSpanTicks
    {
        get => bufferSpan;

        private set => bufferSpan = value;
    }

    /// <summary>
    /// Used by the Drum-Buffer-Rope Release rule to prevent starving of this resource thus keeping its utilization high.
    /// </summary>
    public TimeSpan BufferSpan
    {
        get => new (bufferSpan);

        private set => bufferSpan = value.Ticks;
    }

    private const int CanPreemptMaterialsIdx = 0;

    /// <summary>
    /// Whether the Resource can drive the schedule by requesting that Material Requirments be delivered sooner than they are currently scheduled to start by ignoring them as constraints. This does NOT allow
    /// preemption of ConfirmedConstraints.  See also: BaseOperation.IsConfirmedConstraint and MaterialRequirment.IsConfirmedConstraint. Only allowed outside the Scenario.KeepFeasibleSpan.
    /// </summary>
    [Browsable(false)] //JMC Not ready yet
    public bool CanPreemptMaterials
    {
        get => internalResourceFlags[CanPreemptMaterialsIdx];
        private set => internalResourceFlags[CanPreemptMaterialsIdx] = value;
    }

    private const int CanPreemptPredecessorsIdx = 1;

    /// <summary>
    /// Whether the Resource can drive the schedule by requesting that predecessor Operations start sooner than they are currently scheduled to start by ignoring them as constraints. This does NOT allow
    /// preemption of ConfirmedConstraints.  See also: BaseOperation.IsConfirmedConstraint and MaterialRequirment.IsConfirmedConstraint. This does NOT allow scheduling before the c_EarliestPossibleFinish of
    /// an Operation. Only allowed outside the Scenario.KeepFeasibleSpan.
    /// </summary>
    [Browsable(false)] //JMC Not ready yet
    public bool CanPreemptPredecessors
    {
        get => internalResourceFlags[CanPreemptPredecessorsIdx];
        private set => internalResourceFlags[CanPreemptPredecessorsIdx] = value;
    }

    private const int CanWorkOvertimeIdx = 2;

    /// <summary>
    /// Will cause suggestions for overtime to be generated for late activities where adding the overtime would cause the activity to complete earlier.
    /// </summary>
    [Browsable(false)] //JMC Not ready yet
    public bool CanWorkOvertime
    {
        get => internalResourceFlags[CanWorkOvertimeIdx];
        private set => internalResourceFlags[CanWorkOvertimeIdx] = value;
    }

    private const int ConsecutiveSetupTimesIdx = 3;

    /// <summary>
    /// Whether Resource and Operation setups are done consecutively or in parallel.  If true, then the Total Setup is the sum of the Resource and Operation setups.  If false, then the Total Setup is the
    /// maximum of the two.  This setting has no effect if Use Operation Setup is false.
    /// </summary>
    public bool ConsecutiveSetupTimes
    {
        get => internalResourceFlags[ConsecutiveSetupTimesIdx];
        private set => internalResourceFlags[ConsecutiveSetupTimesIdx] = value;
    }
    
    private decimal cycleEfficiencyMultiplier = 1;

    /// <summary>
    /// Multiplies run time to adjust for slower/faster Resources.
    /// </summary>
    public decimal CycleEfficiencyMultiplier
    {
        get => cycleEfficiencyMultiplier;
        private set
        {
            if (value <= 0)
            {
                throw new ValidationException("2477", new object[] { value });
            }

            cycleEfficiencyMultiplier = value;
        }
    }
    
    private TimeSpan headStartSpan = new (30, 0, 0, 0, 0);

    /// <summary>
    /// During Optimizations, Activities are not permitted to start before  JitStartDate-(SameSetupHeadstartMultiplier*setupSpan+HeadStartSpan).  This can be used to prevent excessive inventory buildup and
    /// aid in grouping same-SetupCode Activities.
    /// </summary>
    public TimeSpan HeadStartSpan
    {
        get => headStartSpan;
        private set => headStartSpan = value;
    }

    [Obsolete("This index is no longer in use as the Drum property has been removed from InternalResource.")]
    private const int c_ObsoleteDrumIdx = 5;

    private decimal overtimeHourlyCost;

    /// <summary>
    /// Can be used for calculating schedule quality and in scripts.
    /// </summary>
    public decimal OvertimeHourlyCost
    {
        get => overtimeHourlyCost;
        private set => overtimeHourlyCost = value;
    }

    private string scheduledRunSpanAlgorithm = "";

    /// <summary>
    /// Name of the script used to calculate ScheduledRunSpan in Activities on this resource.  If specified, this gives the absolute final value for ScheduledRunSpan.  Result must be non-negative decimal or
    /// 1 will be substituted and an error will be logged.
    /// </summary>
    [Browsable(false)] //FUTURE
    public string ScheduledRunSpanAlgorithm
    {
        get => scheduledRunSpanAlgorithm;
        private set => scheduledRunSpanAlgorithm = value;
    }

    private string scheduledSetupSpanAlgorithm = "";

    /// <summary>
    /// Name of the script used to calculate ScheduledSetupSpan in Activities on this resource.  If specified, this gives the absolute final value for ScheduledSetupSpan.  Result must be non-negative decimal
    /// or zero will be substituted and an error will be logged.
    /// </summary>
    [Browsable(false)] //FUTURE
    public string ScheduledSetupSpanAlgorithm
    {
        get => scheduledSetupSpanAlgorithm;
        private set => scheduledSetupSpanAlgorithm = value;
    }

    private string scheduledTransferSpanAlgorithm = "";

    /// <summary>
    /// Name of the script used to calculate ScheduledTransferSpan in Activities on this resource.  If specified, this gives the absolute final value for ScheduledTransferSpan.  Result must be non-negative
    /// decimal or zero will be substituted and an error will be logged.
    /// </summary>
    [Browsable(false)] //FUTURE
    public string ScheduledTransferSpanAlgorithm
    {
        get => scheduledTransferSpanAlgorithm;
        private set => scheduledTransferSpanAlgorithm = value;
    }

    /// <summary>
    /// If set then same cell scheduling for predecessor operations is discontinued. But if this resource is part of a cell then the behavior is as usual and this resource's cell will become the new cell to
    /// try to schedule in.
    /// </summary>
    [Browsable(false)] //FUTURE
    public bool DiscontinueSameCellScheduling
    {
        get => internalResourceFlags[DiscontinueSameCellSchedulingIdx];
        private set => internalResourceFlags[DiscontinueSameCellSchedulingIdx] = value;
    }

    private decimal m_activitySetupEfficiencyMultiplier = 1;

    /// <summary>
    /// Multiplies setup time to adjust for slower/faster Activity setups. Larger numbers mean the resource is slower at performing setups.  Specify 3, for example, to mean that it takes three times longer to perform
    /// a setup than the standards indicate.
    /// </summary>
    public decimal ActivitySetupEfficiencyMultiplier
    {
        get => m_activitySetupEfficiencyMultiplier;
        private set
        {
            if (value <= 0)
            {
                throw new ValidationException("2478", new object[] { value });
            }

            m_activitySetupEfficiencyMultiplier = value;
        }
    }

    private decimal m_changeoverSetupEfficiencyMultiplier = 1;

    /// <summary>
    /// Multiplies setup time to adjust for slower/faster changeover setups. Larger numbers mean the resource is slower at performing setups.  Specify 3, for example, to mean that it takes three times longer to perform
    /// a setup than the standards indicate.
    /// </summary>
    public decimal ChangeoverSetupEfficiencyMultiplier
    {
        get => m_changeoverSetupEfficiencyMultiplier;
        private set
        {
            if (value <= 0)
            {
                throw new ValidationException("2479", new object[] { value });
            }

            m_changeoverSetupEfficiencyMultiplier = value;
        }
    }

    public const string STAGE = "Stage"; //must match property!
    private int stage;

    /// <summary>
    /// Used for multi-stage scheduling to allow scheduling of groups of resources stage by stage. Staging comes with the following restrictions:
    /// 1. Jobs can only have one Manufacturing Order. (the job release event only occurs once in the simulation)
    /// 2. Each operation can only have one predecessor operation and one successor operation.
    /// 3. An operation can only have one resource requirement. (to help prevent the problem of requiring resources that have different stages)
    /// 4. Resources with the same capabilities must have the same stage number. (otherwise the operation would be eligible in multiple stages, only one of the stages would be selected, and the resources
    /// from the other stages would sort of become ineligible)
    /// If any of these restrictions are violated then the simulation might fail.
    /// </summary>
    [Browsable(true)]
    public int Stage
    {
        get => stage;
        private set
        {
            if (stage != value)
            {
                stage = value;
            }
        }
    }

    /// <summary>
    /// Whether to use the the Setup Span of the Operation in calculating the scheduled setup.  If false then only the Resource setup is used.
    /// This value is not used if the Resource IncludeSetupWhen property is set to Never.
    /// </summary>
    public bool UseOperationSetupTime
    {
        get => internalResourceFlags[UseOperationSetupTimeIdx];
        protected set => internalResourceFlags[UseOperationSetupTimeIdx] = value;
    }

    public bool UseResourceSetupTime
    {
        get => internalResourceFlags[c_useResourceSetupTimeIdx];
        protected set => internalResourceFlags[c_useResourceSetupTimeIdx] = value;
    }

    public bool UseSequencedSetupTime
    {
        get => internalResourceFlags[c_useSequencedSetupTimeIdx];
        protected set => internalResourceFlags[c_useSequencedSetupTimeIdx] = value;
    }

    /// <summary>
    /// Whether to use the the Setup Span of the Operation in calculating the scheduled setup.  If false then only the Resource setup is used.
    /// This value is not used if the Resource IncludeSetupWhen property is set to Never.
    /// </summary>
    public bool UseOperationCleanout
    {
        get => internalResourceFlags[c_useOperationCleanoutIdx];
        protected set => internalResourceFlags[c_useOperationCleanoutIdx] = value;
    }

    public bool UseResourceCleanout
    {
        get => internalResourceFlags[c_useResourceCleanoutIdx];
        protected set => internalResourceFlags[c_useResourceCleanoutIdx] = value;
    }

    /// <summary>
    /// Specifies when setup times are applied.
    /// </summary>
    public bool UseAttributeCleanouts
    {
        get => internalResourceFlags[c_useAttributeCleanoutsIdx];
        protected set => internalResourceFlags[c_useAttributeCleanoutsIdx] = value;
    }

    /// <summary>
    /// If true then the Setup Time on the first Activity scheduled on the Resource is always zero.
    /// </summary>
    public bool OmitSetupOnFirstActivity
    {
        get => internalResourceFlags[omitSetupOnFirstActivityIdx];
        protected set => internalResourceFlags[omitSetupOnFirstActivityIdx] = value;
    }

    /// <summary>
    /// If true then the Setup Time on the first Activity scheduled on a Capacity Interval is always zero.
    /// </summary>
    public bool OmitSetupOnFirstActivityInShift
    {
        get => internalResourceFlags[omitSetupOnFirstActivityInShiftIdx];
        protected set => internalResourceFlags[omitSetupOnFirstActivityInShiftIdx] = value;
    }

    /// <summary>
    /// Whether Activities can be reassigned from this Resource during Optimizations.
    /// </summary>
    [Browsable(false)] //JMC Not ready yet
    public bool CanOffload
    {
        get => internalResourceFlags[CanOffloadIdx];
        private set => internalResourceFlags[CanOffloadIdx] = value;
    }

    public const string CapacityType_FieldName = "CapacityType"; //THis must match the Property nambe below.

    private InternalResourceDefs.capacityTypes capacityType = InternalResourceDefs.capacityTypes.SingleTasking;

    /// <summary>
    /// If Finite then only one Activity can be performed on the Resource at any point in time.  If Infinite then any number of can be performed simultaneously.
    /// </summary>
    public InternalResourceDefs.capacityTypes CapacityType
    {
        get => capacityType;

        private set
        {
            if (capacityType == value)
            {
                return;
            }

            switch (capacityType)
            {
                case InternalResourceDefs.capacityTypes.SingleTasking:
                    break;

                case InternalResourceDefs.capacityTypes.Infinite:
                    if (BlockCount > 0)
                    {
                        throw new PTValidationException("2482");
                    }

                    break;

                case InternalResourceDefs.capacityTypes.MultiTasking:
                    if (BlockCount > 0)
                    {
                        throw new PTValidationException("2483");
                    }

                    break;
            }


            capacityType = value;

            // [BATCH_CODE]
            if (capacityType != InternalResourceDefs.capacityTypes.SingleTasking)
            {
                BatchType = MainResourceDefs.batchType.None;
            }
        }
    }

    /// <summary>
    /// Time that must be spent to setup this resource whenever the preceding operation did not use this resource.
    /// </summary>
    public TimeSpan SetupSpan
    {
        get => new (setupSpanTicks);

        private set => setupSpanTicks = value.Ticks;
    }

    private decimal standardHourlyCost;

    /// <summary>
    /// Can be used for calculating schedule quality and in scripts.
    /// </summary>
    public decimal StandardHourlyCost
    {
        get => standardHourlyCost;
        private set => standardHourlyCost = value;
    }

    public bool LimitAutoJoinToSameCapacityInterval
    {
        get => internalResourceFlags[c_limitAutoJoinToSameCapacityIntervalIdx];
        internal set => internalResourceFlags[c_limitAutoJoinToSameCapacityIntervalIdx] = value;
    }

    private TimeSpan m_autoJoinSpan;

    /// <summary>
    /// When AutoJoining, only Join for Operations that start before this amount of time after the Clock.
    /// </summary>
    public TimeSpan AutoJoinSpan
    {
        get => m_autoJoinSpan;
        internal set => m_autoJoinSpan = value;
    }

    private int m_priority;
    /// <summary>
    /// Used to determine a priority for scheduling on this Resource
    /// </summary>
    public int Priority
    {
        get => m_priority;
        internal set => m_priority = value;
    }

    private const int DiscontinueSameCellSchedulingIdx = 6;
    private const int UseOperationSetupTimeIdx = 7;
    private const int CanOffloadIdx = 8;
    private const int alwaysShowPostProcessingIdx = 9; //not using anymore since we split material post processing out.
    private const int omitSetupOnFirstActivityIdx = 10;
    private const int omitSetupOnFirstActivityInShiftIdx = 11;
    private const int c_limitAutoJoinToSameCapacityIntervalIdx = 12;
    private const int c_useOperationCleanoutIdx = 13;
    private const int c_useAttributeCleanoutsIdx = 14;
    private const int c_useResourceSetupTimeIdx = 15;
    private const int c_useSequencedSetupTimeIdx = 16;
    private const int c_useResourceCleanoutIdx = 17;

    #endregion

    #region Propertes
    private decimal m_resourceSetupCost;

    public decimal ResourceSetupCost
    {
        get => m_resourceSetupCost;
        internal set => m_resourceSetupCost = value;
    }

    private decimal m_resourceCleanoutCost;
    public decimal ResourceCleanoutCost
    {
        get => m_resourceCleanoutCost;
        internal set => m_resourceCleanoutCost = value;
    }

    private long setupSpanTicks;

    public long SetupSpanTicks
    {
        get => setupSpanTicks;
        internal set => setupSpanTicks = value;
    }

    private long m_standardCleanSpanTicks;

    public long StandardCleanSpanTicks
    {
        get => m_standardCleanSpanTicks;
        internal set => m_standardCleanSpanTicks = value;
    }

    private int m_standardCleanoutGrade;

    public int StandardCleanoutGrade
    {
        get => m_standardCleanoutGrade;
        internal set => m_standardCleanoutGrade = value;
    }
    #endregion

    #region Performance
    private decimal setupPerformance;

    /// <summary>
    /// Indicates the Resource's performance in terms of speed of doing Setups.  The value is the percent of standard that the Resource has taken to complete the Setups on average.  Therefore lower numbers
    /// indicate better performance. Note that performance is only tracked for an Activity's Primary Resource when the Activity is finished, when the Resource always includes setup time, and when the
    /// Operation's standard setup time is more than zero.
    /// </summary>
    [Browsable(false)]
    public decimal SetupPerformance => setupPerformance;

    private decimal runPerformance;

    /// <summary>
    /// Indicates the Resource's performance in terms of speed of performing Activities.  The value is the percent of standard that the Resource has taken to complete the Activity Runs on average.  Therefore
    /// lower numbers indicate better performance.  Note that performance is only tracked for an Activity's Primary Resource when the Activity is finished.
    /// </summary>
    [Browsable(false)]
    public decimal RunPerformance => runPerformance;

    private decimal postProcessingPerformance;

    /// <summary>
    /// Indicates the Resource's performance in terms of speed of performing Activity PostProcessing.  The value is the percent of standard that the Resource has taken to complete the Activity
    /// Post-Processing on average.  Therefore lower numbers indicate better performance.  Note that performance is only tracked for an Activity's Primary Resource when the Activity is finished, and when the
    /// Operation's standard Post-Processing time is more than zero.
    /// </summary>
    [Browsable(false)]
    public decimal PostProcessingPerformance => postProcessingPerformance;

    private decimal scrapPerformance;

    /// <summary>
    /// Indicates the Resource's performance in terms of standard versus actual scrap.  The value is the percent of standard scrap that the Resource has created.  Therefore lower numbers indicate better
    /// performance. Note that performance is only tracked for the Activity's Primary Resource when the Activity is finished and when the Activity has an expected scrap qty more than zero.
    /// </summary>
    [Browsable(false)]
    public decimal ScrapPerformance => scrapPerformance;

    private int setupsDone;

    /// <summary>
    /// Indicates the number of Setups that the current performance value is based on.
    /// </summary>
    [Browsable(false)]
    public int SetupsDone => setupsDone;

    private int runsDone;

    /// <summary>
    /// Indicates the number of Runs that the current performance value is based on.
    /// </summary>
    [Browsable(false)]
    public int RunsDone => runsDone;

    private int postProcessesDone;

    /// <summary>
    /// Indicates the number of Post-Processes that the current performance value is based on.
    /// </summary>
    [Browsable(false)]
    public int PostProcessesDone => postProcessesDone;

    private int activitiesFinished;

    /// <summary>
    /// Indicates the number of Activities that the Resource has Finished since the last Performance Reset.
    /// </summary>
    [Browsable(false)]
    public int ActivitiesFinished => activitiesFinished;

    private int scrapsDone;

    /// <summary>
    /// Indicates the number of Activities that the current scrap performance values are based on.
    /// </summary>
    [Browsable(false)]
    public int ScrapsDone => scrapsDone;

    private DateTimeOffset m_lastPerformanceReset = PTDateTime.UtcNow.RemoveSeconds();

    /// <summary>
    /// Indicates the start date that the current performance values began calculating.
    /// </summary>
    [Browsable(false)]
    public DateTimeOffset LastPerformanceReset => m_lastPerformanceReset;

    public void ClearPerformance(DateTimeOffset a_transmissionDt)
    {
        setupPerformance = 0;
        runPerformance = 0;
        postProcessingPerformance = 0;
        scrapPerformance = 0;
        setupsDone = 0;
        runsDone = 0;
        postProcessesDone = 0;
        scrapsDone = 0;
        activitiesFinished = 0;
        m_lastPerformanceReset = a_transmissionDt;
    }

    //JMC TODO -- Move this to Resource. Depends on Operation's type.
    public void UpdatePerformance(InternalActivity newlyFinishedActivity)
    {
        activitiesFinished++;

        //Setup
        if (UseOperationSetupTime && newlyFinishedActivity.Operation.SetupSpan.Ticks > 0)
        {
            UpdateSetupPerformance(newlyFinishedActivity.Operation.SetupSpan, newlyFinishedActivity.ReportedSetupSpan);
        }

        //Run
        if (newlyFinishedActivity.Operation is ResourceOperation)
        {
            TimeSpan expectedRun = new ((long)(newlyFinishedActivity.GetResourceProductionInfo(this).CycleSpanTicks * (newlyFinishedActivity.ReportedGoodQty + newlyFinishedActivity.ReportedScrapQty) / newlyFinishedActivity.GetResourceProductionInfo(this).QtyPerCycle));
            if (expectedRun.Ticks > 0)
            {
                UpdateRunPerformance(expectedRun, newlyFinishedActivity.ReportedRunSpan);
            }
        }
        else
        {
            throw new PTValidationException("2484");
        }

        //Post-Processing
        if (newlyFinishedActivity.Operation.PostProcessingSpan.Ticks > 0)
        {
            UpdatePostProcessingPerformance(newlyFinishedActivity.Operation.PostProcessingSpan, newlyFinishedActivity.ReportedPostProcessingSpan);
        }

        //Scrap
        if (newlyFinishedActivity.ExpectedScrapQty > 0)
        {
            UpdateScrapPerformance(newlyFinishedActivity.ExpectedScrapQty, newlyFinishedActivity.ReportedScrapQty);
        }
    }

    /// <summary>
    /// Add the latest completed Activities performance to the Resource's overall performance.
    /// </summary>
    private void UpdateSetupPerformance(TimeSpan activityExpectedDuration, TimeSpan activityActualDuration)
    {
        decimal newPerformance = (SetupPerformance * SetupsDone + CalcPerformance(activityExpectedDuration, activityActualDuration)) / (SetupsDone + 1);
        setupsDone++;
        setupPerformance = newPerformance;
    }

    /// <summary>
    /// Add the latest completed Activities performance to the Resource's overall performance.
    /// </summary>
    private void UpdateRunPerformance(TimeSpan activityExpectedDuration, TimeSpan activityActualDuration)
    {
        decimal newPerformance = (RunPerformance * RunsDone + CalcPerformance(activityExpectedDuration, activityActualDuration)) / (RunsDone + 1);
        runsDone++;
        runPerformance = newPerformance;
    }

    /// <summary>
    /// Add the latest completed Activities performance to the Resource's overall performance.
    /// </summary>
    private void UpdatePostProcessingPerformance(TimeSpan activityExpectedDuration, TimeSpan activityActualDuration)
    {
        decimal newPerformance = (PostProcessingPerformance * PostProcessesDone + CalcPerformance(activityExpectedDuration, activityActualDuration)) / (PostProcessesDone + 1);
        postProcessesDone++;
        postProcessingPerformance = newPerformance;
    }

    /// <summary>
    /// Add the latest completed Activities performance to the Resource's overall performance.
    /// </summary>
    private void UpdateScrapPerformance(decimal expectedScrap, decimal actualScrap)
    {
        decimal newPerformance = (ScrapPerformance * ScrapsDone + CalcPerformance(expectedScrap, actualScrap)) / (ScrapsDone + 1);
        scrapsDone++;
        scrapPerformance = newPerformance;
    }

    /// <summary>
    /// A performance of 1.0 means the actual = expected.
    /// </summary>
    private decimal CalcPerformance(TimeSpan activityExpectedDuration, TimeSpan activityActualDuration)
    {
        return 1 + (activityActualDuration.Ticks - activityExpectedDuration.Ticks) / (decimal)activityExpectedDuration.Ticks;
    }

    /// <summary>
    /// A performance of 1.0 means the actual = expected.
    /// </summary>
    private decimal CalcPerformance(decimal expected, decimal actual)
    {
        return 1 + (actual - expected) / expected;
    }
    #endregion

    #region Object Accessors
    private CapabilityManager m_capabilityManager = new (null);

    /// <summary>
    /// READ-ONLY: Don't use this to add or remove capabilities. Instead use Add Capability(), RemoveCapability(). For reading capabilities use CapabilityCount, GetCapabilityByIndex(), and
    /// GetCapabilityById().
    /// </summary>
    public CapabilityManager GetReadOnlyCapabilitiesManager()
    {
        return (CapabilityManager)m_capabilityManager.Clone();
    }

    /// <summary>
    /// O(1).
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Capability GetCapabilityByIndex(int index)
    {
        return m_capabilityManager.GetByIndex(index);
    }

    /// <summary>
    /// Log2(n)
    /// </summary>
    /// <param name="a_id"></param>
    /// <returns></returns>
    public Capability GetCapabilityById(BaseId a_id)
    {
        return m_capabilityManager.GetById(a_id);
    }

    /// <summary>
    /// There must be a corresponding call to AddCapabilitiesComplete() after the capabilities for this resource have been added.
    /// </summary>
    /// <param name="c"></param>
    internal void AddCapability(Capability c)
    {
        m_capabilityManager.Add(c);
    }

    private void RemoveCapability(Capability c)
    {
        m_capabilityManager.Remove(c);
    }

    private void CreateNewCapabilityManager()
    {
        m_capabilityManager = new CapabilityManager(null);
    }

    [ListSource(ListSourceAttribute.ListSources.Capability, true)]
    /// <summary>
    /// The Capabilities that this Resource has.
    /// </summary>
    [ParenthesizePropertyName(true)]
    public int NbrCapabilities => m_capabilityManager.Count;

    //Remove this once Scenario Detail is checked in. Renamed to NbrCapabilities so ParenthesizeProperty won't bring column too far left.
    internal int CapabilityCount => m_capabilityManager.Count;

    private ReadyActivitiesDispatcher m_dispatcher;

    public ReadyActivitiesDispatcher Dispatcher
    {
        get => m_dispatcher;

        private set
        {
            m_normalDispatchDefinitionId = value.DispatcherDefinition.Id;
            m_dispatcher = value;
            m_dispatcher.Resource = this;
        }
    }

    /// <summary>
    /// Assign Dispatcher a new dispatcher defined by a dispatcher definition.
    /// </summary>
    /// <param name="a_dispatcher"></param>
    /// <param name="a_dispatcherDefinition"></param>
    internal void CreateDispatcher(DispatcherDefinition a_dispatcherDefinition)
    {
        Dispatcher = a_dispatcherDefinition.CreateDispatcher();
    }

    internal void CreateExperimentalDispatcherOne(DispatcherDefinition a_dispatcherDefinition)
    {
        ExperimentalDispatcherOne = a_dispatcherDefinition.CreateDispatcher();
    }

    internal void CreateExperimentalDispatcherTwo(DispatcherDefinition a_dispatcherDefinition)
    {
        ExperimentalDispatcherTwo = a_dispatcherDefinition.CreateDispatcher();
    }

    internal void CreateExperimentalDispatcherThree(DispatcherDefinition a_dispatcherDefinition)
    {
        ExperimentalDispatcherThree = a_dispatcherDefinition.CreateDispatcher();
    }

    internal void CreateExperimentalDispatcherFour(DispatcherDefinition a_dispatcherDefinition)
    {
        ExperimentalDispatcherFour = a_dispatcherDefinition.CreateDispatcher();
    }

    public const string c_normalSequencingPlan = "NormalOptimizeRule"; //Must match property name below.

    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    [ListSource(ListSourceAttribute.ListSources.BalancedCompositeDispatcherDefinition, false)]
    /// <summary>
    /// The Optimize Rule that is normally used for Activity selection during Optimizations.
    /// </summary>
    public string NormalOptimizeRule => Dispatcher.DispatcherDefinition.Name;

    private BaseId m_normalDispatchDefinitionId;

    [Browsable(false)]
    /// <summary>
    /// The Dispatcher that is normally used for Activity selection during Optimizations.
    /// </summary>
    public BaseId NormalDispatcherId => m_normalDispatchDefinitionId;

    private const short c_numberOfExperimentalDispatcherDefinitions = 4;

    private readonly BaseId[] m_experimentalDispatchDefinitionIds = new BaseId[c_numberOfExperimentalDispatcherDefinitions];

    public ReadyActivitiesDispatcher ExperimentalDispatcherOne
    {
        get => m_experimentalDispatchers[0];

        private set
        {
            m_experimentalDispatchDefinitionIds[0] = value.DispatcherDefinition.Id;
            m_experimentalDispatchers[0] = value;
            m_experimentalDispatchers[0].Resource = this;
        }
    }

    public ReadyActivitiesDispatcher ExperimentalDispatcherTwo
    {
        get => m_experimentalDispatchers[1];

        private set
        {
            m_experimentalDispatchDefinitionIds[1] = value.DispatcherDefinition.Id;
            m_experimentalDispatchers[1] = value;
            m_experimentalDispatchers[1].Resource = this;
        }
    }

    public ReadyActivitiesDispatcher ExperimentalDispatcherThree
    {
        get => m_experimentalDispatchers[2];

        private set
        {
            m_experimentalDispatchDefinitionIds[2] = value.DispatcherDefinition.Id;
            m_experimentalDispatchers[2] = value;
            m_experimentalDispatchers[2].Resource = this;
        }
    }

    public ReadyActivitiesDispatcher ExperimentalDispatcherFour
    {
        get => m_experimentalDispatchers[3];

        private set
        {
            m_experimentalDispatchDefinitionIds[3] = value.DispatcherDefinition.Id;
            m_experimentalDispatchers[3] = value;
            m_experimentalDispatchers[3].Resource = this;
        }
    }

    public bool IsDispatcherUsedByResource(BaseId a_dispatcherId)
    {
        for (int i = 0; i < c_numberOfExperimentalDispatcherDefinitions; i++)
        {
            if (m_experimentalDispatchDefinitionIds[i] == a_dispatcherId)
            {
                return true;
            }
        }

        return false;
    }

    // TODO: These strings are referenced in code relating to ResourceChangeRefT.
    // I don't think that transmission is in use, and these strings along with its reference
    // should most likely be removed. 
    //These strings must match its respective property name below.
    public const string c_experimentalSequencingPlan = "ExperimentalOptimizeRule";
    public const string c_experimentalSequencingPlanTwo = "ExperimentalOptimizeRuleTwo";
    public const string c_experimentalSequencingPlanThree = "ExperimentalOptimizeRuleThree";
    public const string c_experimentalSequencingPlanFour = "ExperimentalOptimizeRuleFour";

    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    [ListSource(ListSourceAttribute.ListSources.BalancedCompositeDispatcherDefinition, false)]
    /// <summary>
    /// Used to evaluate using other Dispatchers for the Resource.
    /// </summary>
    public string ExperimentalOptimizeRuleOne
    {
        get
        {
            if (ExperimentalDispatcherOne != null)
            {
                return ExperimentalDispatcherOne.DispatcherDefinition.Name;
            }

            return "(None)".Localize();
        }
    }

    public string ExperimentalOptimizeRuleTwo
    {
        get
        {
            if (ExperimentalDispatcherTwo != null)
            {
                return ExperimentalDispatcherTwo.DispatcherDefinition.Name;
            }

            return "(None)".Localize();
        }
    }

    public string ExperimentalOptimizeRuleThree
    {
        get
        {
            if (ExperimentalDispatcherThree != null)
            {
                return ExperimentalDispatcherThree.DispatcherDefinition.Name;
            }

            return "(None)".Localize();
        }
    }

    public string ExperimentalOptimizeRuleFour
    {
        get
        {
            if (ExperimentalDispatcherFour != null)
            {
                return ExperimentalDispatcherFour.DispatcherDefinition.Name;
            }

            return "(None)".Localize();
        }
    }

    [Browsable(false)]
    /// <summary>
    /// Used to evaluate using other Dispatchers for the Resource.
    /// </summary>
    public BaseId ExperimentalDispatcherIdOne => m_experimentalDispatchDefinitionIds[0];

    public BaseId ExperimentalDispatcherIdTwo => m_experimentalDispatchDefinitionIds[1];
    public BaseId ExperimentalDispatcherIdThree => m_experimentalDispatchDefinitionIds[2];
    public BaseId ExperimentalDispatcherIdFour => m_experimentalDispatchDefinitionIds[3];
    #endregion

    #region Capabillity
    internal virtual bool DisassociateCapability(Capability c, ProductRuleManager a_productRuleManager)
    {
        Capability mc = GetCapabilityById(c.Id);

        if (mc == null)
        {
            throw new PTValidationException("2485", new object[] { c.Name, Name });
        }

        RemoveCapability(mc);
        return true;
    }
    #endregion

    #region Capacity stuff
    /// <param name="start">The time for the first bucket to start.</param>
    /// <param name="endInclusive">The last time that must be included.  The last bucket may run past this time.</param>
    /// <param name="bucketLength">The time between the start and end of each bucket.</param>
    internal TimeBucketList GetBucketedCapacity(DateTime start, DateTime endInclusive, TimeSpan bucketLength)
    {
        return capacityProfile.GetBucketedCapacity(start, endInclusive, bucketLength);
    }
    
    /// <param name="start">The time for the first bucket to start.</param>
    /// <param name="endInclusive">The last time that must be included.  The last bucket may run past this time.</param>
    /// <param name="bucketLength">The time between the start and end of each bucket.</param>
    internal abstract decimal[] GetBucketedProduction(DateTime start, DateTime endInclusive, TimeSpan bucketLength, CapacityInfoBase.GroupChooser chooser);

    /// <summary>
    /// Adds to the list Blocks that are scheduled to either start or end in the specified bucket.
    /// </summary>
    /// <param name="start">The start date/time of the first bucket.  Must correspond with GetBucketedUsage() method.</param>
    /// <param name="endInclusive">The last time that must be included.  The last bucket may run past this time.</param>
    /// <param name="bucket">The bucket whose blocks should be returned.</param>
    /// <param name="chooser">Used to select which BLocks to include.  Must be the same used by the GetBucketedUsage() method.</param>
    /// <returns></returns>
    public abstract void AddBlocksInBucket(ref ResourceBlockList list, DateTime start, DateTime endInclusive, TimeSpan bucketLength, int bucket, CapacityInfoBase.GroupChooser chooser);

    //public long CalcOnlineTimeSpan(long a_start, long a_end)
    //{
    //    PT.Scheduler.CapacityInterval current;
    //    current = CapacityIntervals.FindOnlineAtPointInTime(a_start);
    //    while(current!=null)
    //    {
    //    }
    //}

    #region Capacity Interval and Recurring Capacity Interval Maintenance
    /// <summary>
    /// Adds the CapacityInterval to the Resource and adds the Resource to the CapacityInterval's Resource list.
    /// When all CapacityIntervals have been added, call InternalResource.RegenerateCapacityProfile() to update the resource's
    /// private ResourceCapacityIntervals list.
    /// </summary>
    /// <param name="capacityInterval"></param>
    internal bool AddCapacityInterval(CapacityInterval ci)
    {
        if (!CapacityIntervals.Contains(ci.Id)) //Don't add the same interval more than once.
        {
            CapacityIntervals.Add(ci);
            ci.Add(this);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Adds the CapacityInterval to the Resource and adds the Resource to the CapacityInterval's Resource list.
    /// When all CapacityIntervals have been added, call InternalResource.RegenerateCapacityProfile() to update the resource's
    /// private ResourceCapacityIntervals list.
    /// </summary>
    /// <param name="capacityInterval"></param>
    internal bool AddRecurringCapacityInterval(RecurringCapacityInterval a_rci)
    {
        if (!RecurringCapacityIntervals.Contains(a_rci.Id)) //Don't add the same interval more than once.
        {
            RecurringCapacityIntervals.Add(a_rci);
            a_rci.Add(this);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes the CapacityInterval from the resource and the resource from the capacity interval.
    /// </summary>
    internal void RemoveCapacityInterval(CapacityInterval ci)
    {
        CapacityIntervals.Remove(ci);
        ci.CalendarResources.Remove(this);
    }

    /// <summary>
    /// Removes the CapacityInterval from the resource and the resource from the capacity interval.
    /// </summary>
    internal void RemoveRecurringCapacityInterval(RecurringCapacityInterval rci)
    {
        RecurringCapacityIntervals.Remove(rci);
        rci.CalendarResources.Remove(this);
    }

    private readonly CapacityIntervalsCollection capacityIntervals = new ();

    /// <summary>
    /// Stores a list of the CapacityIntervals that are assigned to this Resource.
    /// </summary>
    [Browsable(false)]
    public CapacityIntervalsCollection CapacityIntervals => capacityIntervals;

    private readonly RecurringCapacityIntervalsCollection recurringCapacityIntervals = new ();

    /// <summary>
    /// Stores a list of the RecurringCapacityIntervals that are assigned to this Resource.
    /// </summary>
    [Browsable(false)]
    public RecurringCapacityIntervalsCollection RecurringCapacityIntervals => recurringCapacityIntervals;
    #endregion Capacity Interval and Recurring Capacity Interval Maintenance

    #region Resource Capacity Intervals
    private readonly ResourceCapacityIntervalsCollection resourceCapacityIntervals = new ();

    /// <summary>
    /// Stores a private list of ResourceCapacityIntervals that completely define the resource's capacity profile.
    /// This is used in the process of creating the CapacityProfile.
    /// </summary>
    [Browsable(false)]
    public ResourceCapacityIntervalsCollection ResourceCapacityIntervals => resourceCapacityIntervals;

    /// <summary>
    /// Clears the ResourceCapacityIntervals list and refills it from the CapacityIntervals and RecurringCapacityIntervals.
    /// </summary>
    /// <returns>Whether the intervals have changed.</returns>
    private bool RegenerateResourceCapacityIntervals(long a_planningHorizonEndTicks, bool a_ciUpdated)
    {
        if (!a_ciUpdated)
        {
            return false; // The intervals haven't changed.
        }

        //Clear the profile
        ResourceCapacityIntervals.Clear();

        //Create ResourceCapacityIntervals for each CapacityInterval and RecurringCapacityInterval's RCIExpansion and add it to a sorted list.
        for (int i = 0; i < CapacityIntervals.Count; i++)
        {
            CapacityInterval c = CapacityIntervals[i];
            if (c.StartDateTime.Ticks > a_planningHorizonEndTicks)
            {
                continue; //If we can rely on the capacity intervals being sorted, we could break here instead of continue.
            }

            ResourceCapacityInterval rci = new (c.Id, c.IntervalType, c.StartDateTime, c.EndDateTime, c.NbrOfPeople, c.GetIntervalProfile());
            ResourceCapacityIntervals.Add(rci);
        }

        for (int i = 0; i < RecurringCapacityIntervals.Count; i++)
        {
            RecurringCapacityInterval c = RecurringCapacityIntervals[i];
            //c.Expand(new DateTime(sd.EndOfPlanningHorizon), sd.Clock);

            int intervalNbr = 0;
            foreach (RecurringCapacityInterval.RCIExpansion expansion in c)
            {
                decimal nbrOfPeople;
                if (intervalNbr < c.NbrIntervalsToOverride)
                {
                    nbrOfPeople = c.NbrOfPeopleOverride;
                }
                else
                {
                    nbrOfPeople = c.NbrOfPeople;
                }

                ResourceCapacityInterval rci = new (c.Id, c.IntervalType, expansion.Start, expansion.End, nbrOfPeople, c.GetIntervalProfile());
                ResourceCapacityIntervals.Add(rci);

                ++intervalNbr;
            }
        }

        return true; // The intervals have changed.
    }
    #endregion Resource Capacity Intervals
    public ResourceCapacityIntervalsCollection GetCapacityProfiles()
    {
        return capacityProfile.ProfileIntervals;
    }

    internal void RegenerateCapacityProfile(long a_planningHorizonEndTicks, bool a_ciUpdated)
    {
        if (RegenerateResourceCapacityIntervals(a_planningHorizonEndTicks, a_ciUpdated) || !InitialCapacityProfileRegenerationDone) // A run of MassRecordings could take 57% longer without this check.
        {
            // The capacityProfile is only regenerated if the resource's capacity intervals have been regenerated.
            capacityProfile.Regenerate(a_planningHorizonEndTicks);
            CountOverlappingIntervals();
            InitialCapacityProfileRegenerationDone = true;
        }
    }

    /// <summary>
    /// Removes resource capacity profile intervals that end before the clock.
    /// </summary>
    internal void PurgeResourceCapacityIntervalsEndingBeforeClock(long clock)
    {
        capacityProfile.PurgeIntervalsEndingBeforeClock(clock);
        for (int i = ResourceCapacityIntervals.Count - 1; i >= 0; i--)
        {
            ResourceCapacityInterval ci = ResourceCapacityIntervals[i];
            if (ci.EndDate < clock)
            {
                ResourceCapacityIntervals.Remove(i);
            }
        }
    }

    #region IDeserializationInit Members
    public void DeserializationInit()
    {
        m_resultantCapacity = new ResourceCapacityIntervalList(); //Handled by PTSerialization.
    }
    #endregion

    public decimal FindNbrOfCyclesBetweenStartAndFinish(long a_startTicks, long a_finishTicks, InternalActivity a_act)
    {
        long capacity = 0;
        CapacityUsageProfile productionProfile = a_act.Batch.PrimaryResourceBlock.CapacityProfile.ProductionProfile;
        foreach (OperationCapacity operationCapacity in productionProfile)
        {
            if (a_finishTicks >= operationCapacity.EndTicks)
            {
                capacity += operationCapacity.TotalCapacityTicks;
            }
            else if (a_finishTicks < operationCapacity.StartTicks)
            {
                break;
            }
            else
            {
                //It ends somewhere in between
                long time = a_finishTicks - operationCapacity.StartTicks;
                capacity += (long)(time * operationCapacity.CapacityRatio);
            }
        }
        //Note: division of longs will truncate remainders. It is possible that there is some precision lost on the capacity calculation.
        decimal capacityDecimal = Convert.ToDecimal(capacity);
        decimal cycleSpanDecimal = Convert.ToDecimal(a_act.GetResourceProductionInfo(this).CycleSpanTicks);
        decimal fractionalCycles = capacityDecimal / cycleSpanDecimal;
        return fractionalCycles;
    }

    public decimal FindQuantityThatCanBeProcessedBetweenStartAndFinish(long a_startTicks, long a_finishTicks, InternalActivity a_act)
    {
        decimal cycles = FindNbrOfCyclesBetweenStartAndFinish(a_startTicks, a_finishTicks, a_act);
        decimal qty = cycles * a_act.GetResourceProductionInfo(this).QtyPerCycle;
        return qty;
    }
    #endregion

    #region Overlapping Intervals
    private int overlappingOnlineIntervals;

    /// <summary>
    /// The number of Normal Online intervals that are overlapping.
    /// This is a warning since it's not usually intentional to setup the data this way and the overlapping intervals will be summed
    /// during their overlaps.
    /// </summary>
    [ReadOnly(true)]
    public int OverlappingOnlineIntervals
    {
        get => overlappingOnlineIntervals;
        private set => overlappingOnlineIntervals = value;
    }

    private void CountOverlappingIntervals()
    {
        //Create a list of all Normal Online Capacity Intervals and Recurring Capacity Interval Expansions
        ArrayList list = new ();
        for (int i = 0; i < CapacityIntervals.Count; i++)
        {
            CapacityInterval ci = CapacityIntervals[i];
            if (ci.Active)
            {
                list.Add(new ComparableInterval(ci, true));
                list.Add(new ComparableInterval(ci, false));
            }
        }

        for (int i = 0; i < RecurringCapacityIntervals.Count; i++)
        {
            RecurringCapacityInterval rci = RecurringCapacityIntervals[i];
            if (rci.Active)
            {
                foreach (RecurringCapacityInterval.RCIExpansion exp in rci)
                {
                    list.Add(new ComparableInterval(exp, true));
                    list.Add(new ComparableInterval(exp, false));
                }
            }
        }

        //Now sort the list by date
        list.Sort();
        //Now go through the list and see if there are overlaps.
        int onlineLevel = 0;
        OverlappingOnlineIntervals = 0; //initialize
        for (int i = 0; i < list.Count; i++)
        {
            ComparableInterval comp = (ComparableInterval)list[i];
            if (comp.starting)
            {
                onlineLevel++;
                if (onlineLevel > 1) //have overlap
                {
                    OverlappingOnlineIntervals++;
                }
            }
            else
            {
                onlineLevel--;
            }
        }
    }

    internal class ComparableInterval : IComparable
    {
        internal ComparableInterval(CapacityInterval aCapacityInterval, bool addStart)
        {
            capacityInterval = aCapacityInterval;
            if (addStart)
            {
                compareDateTime = aCapacityInterval.StartDateTime.Ticks;
            }
            else
            {
                compareDateTime = aCapacityInterval.EndDateTime.Ticks;
            }

            starting = addStart;
        }

        internal ComparableInterval(RecurringCapacityInterval.RCIExpansion aExpansion, bool addStart)
        {
            expansion = aExpansion;
            if (addStart)
            {
                compareDateTime = aExpansion.Start.Ticks;
            }
            else
            {
                compareDateTime = aExpansion.End.Ticks;
            }

            starting = addStart;
        }

        internal CapacityInterval capacityInterval;
        private RecurringCapacityInterval.RCIExpansion expansion;

        internal long compareDateTime;
        internal bool starting;

        #region IComparable Members
        public int CompareTo(object obj)
        {
            ComparableInterval c = (ComparableInterval)obj;

            if (compareDateTime < c.compareDateTime)
            {
                return -1;
            }

            if (compareDateTime == c.compareDateTime)
            {
                //Return ends first in case intervals start and end at the same tick.
                if (!c.starting)
                {
                    return 1;
                }

                if (!starting)
                {
                    return -1;
                }

                return 0;
            }

            return 1;
        }
        #endregion
    }
    #endregion Overlapping Intervals

    #region Lookup Tables
    private AttributeCodeTable attributeCodeTable;

    /// <summary>
    /// The Attribute Code Table that this Resource uses to calculate Setup Cost and Time based on sequential Operation Attribute Code changes.
    /// This value is null if no Attribute Code Table has been assigned.
    /// </summary>
    [Browsable(false)]
    public AttributeCodeTable AttributeCodeTable
    {
        get => attributeCodeTable;
        internal set => attributeCodeTable = value;
    }

    /// <summary>
    /// The Attribute Code Table that this Resource uses to calculate Setup Cost and Time based on sequential Operation Attribute Code changes.
    /// This value is blank if no Attribute Code Table has been assigned.
    /// </summary>
    public string AttributeCodeTableName
    {
        get
        {
            if (AttributeCodeTable != null)
            {
                return AttributeCodeTable.Name;
            }

            return "";
        }
    }

    private ItemCleanoutTable m_itemCleanoutTable;

    /// <summary>
    /// The Item Cleanout Table that this Resource uses to calculate Cleanout Cost and Time based on product changeover.
    /// </summary>
    public ItemCleanoutTable ItemCleanoutTable
    {
        get => m_itemCleanoutTable;
        internal set => m_itemCleanoutTable = value;
    }

    /// <summary>
    /// The Item Cleanout Table that this Resource uses to calculate Cleanout Cost and Time based on product changeover.
    /// This value is blank if no Attribute Code Table has been assigned.
    /// </summary>
    public string ItemCleanoutTableName
    {
        get
        {
            if (ItemCleanoutTable != null)
            {
                return ItemCleanoutTable.Name;
            }

            return "";
        }
    }

    private TimeCleanoutTriggerTable timeCleanoutTriggerTable;

    /// <summary>
    /// TODO
    /// </summary>
    [Browsable(false)]
    public TimeCleanoutTriggerTable TimeCleanoutTriggerTable
    {
        get => timeCleanoutTriggerTable;
        internal set => timeCleanoutTriggerTable = value;
    }

    /// <summary>
    /// TODO
    /// </summary>
    public string TimeCleanoutTriggerTableName
    {
        get
        {
            if (TimeCleanoutTriggerTable != null)
            {
                return TimeCleanoutTriggerTable.Name;
            }

            return "";
        }
    }

    private ProductionUnitsCleanoutTriggerTable productionUnitsCleanoutTriggerTable;

    /// <summary>
    /// TODO
    /// </summary>
    [Browsable(false)]
    public ProductionUnitsCleanoutTriggerTable ProductionUnitsCleanoutTriggerTable
    {
        get => productionUnitsCleanoutTriggerTable;
        internal set => productionUnitsCleanoutTriggerTable = value;
    }

    /// <summary>
    /// TODO
    /// </summary>
    public string ProductionUnitsCleanoutTriggerTableName
    {
        get
        {
            if (ProductionUnitsCleanoutTriggerTable != null)
            {
                return ProductionUnitsCleanoutTriggerTable.Name;
            }

            return "";
        }
    }

    private OperationCountCleanoutTriggerTable operationCountCleanoutTriggerTable;

    /// <summary>
    /// TODO
    /// </summary>
    [Browsable(false)]
    public OperationCountCleanoutTriggerTable OperationCountCleanoutTriggerTable
    {
        get => operationCountCleanoutTriggerTable;
        internal set => operationCountCleanoutTriggerTable = value;
    }

    /// <summary>
    /// TODO
    /// </summary>
    public string OperationCountCleanoutTriggerTableName
    {
        get
        {
            if (OperationCountCleanoutTriggerTable != null)
            {
                return OperationCountCleanoutTriggerTable.Name;
            }

            return "";
        }
    }

    private RangeLookup.FromRangeSets fromToRanges;

    [Browsable(false)]
    public RangeLookup.FromRangeSets FromToRanges
    {
        get => fromToRanges;
        internal set => fromToRanges = value;
    }
    #endregion

    private MainResourceDefs.batchType m_batchType = MainResourceDefs.batchType.None; // [BATCH]

    // [BATCH_CODE]
    /// <summary>
    /// Only SingleTaskingResources can be batch resources.
    /// </summary>
    public MainResourceDefs.batchType BatchType
    {
        get => m_batchType;
        private set
        {
            if (capacityType != InternalResourceDefs.capacityTypes.SingleTasking && value != MainResourceDefs.batchType.None)
            {
                throw new PTValidationException("2486");
            }

            m_batchType = value;
        }
    }

    private decimal m_batchVolume = DefaultBatchVolume; // [BATCH]
    private const decimal DefaultBatchVolume = 1;

    /// <summary>
    /// This is a work around for serialization prior to serializatoin version number 361. Some customer's who received releases prior to completion of the batch enhancement
    /// had this value initialized to 0.
    /// </summary>
    private void ResetBatchIfOutOfBounds()
    {
        if (BatchVolume <= 0)
        {
            BatchVolume = DefaultBatchVolume;
        }
    }

    /// <summary>
    /// If this is a batch by volume resource, this indicates the quantity of parts the resource can process in a single cycle.
    /// </summary>
    public decimal BatchVolume
    {
        get => m_batchVolume;
        private set
        {
            if (value > 0)
            {
                m_batchVolume = value;
            }
        }
    }

    private decimal m_minNbrOfPeople;

    /// <summary>
    /// This can be used by Customizations and has no standard function.
    /// </summary>
    public decimal MinNbrOfPeople
    {
        get => m_minNbrOfPeople;
        internal set => m_minNbrOfPeople = value;
    }

    // [BATCH_CODE]
    internal bool BatchResource()
    {
        return m_batchType != MainResourceDefs.batchType.None;
    }

    /// <summary>
    /// This object is used to track the material stored in a tank type of resource.
    /// </summary>
    internal ResourceWarehouse m_tank; // [TANK]

    /// <summary>
    /// Used to track the material stored in a tank type of resource.
    /// </summary>
    public ResourceWarehouse Tank => m_tank; // [TANK]

    /// <summary>
    /// Whether this is a tank type of resource.
    /// </summary>
    public bool IsTank => m_tank != null; // [TANK_CODE]

    /// <summary>
    /// This is only for tank resources.
    /// This function clears all the inventories from the tank.
    /// </summary>
    /// <param name="a_sd"></param>
    internal void ClearAllTankInventories(ScenarioDetailClearT a_t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        if (!IsTank)
        {
            throw new Exception("ClearAllTankInventories called on a resource that isn't a tank.".Localize());
        }

        m_tank.Receive(a_t, a_sd, a_dataChanges);
    }

    internal void Deleting(ScenarioDetail a_sd)
    {
        if (m_tank != null)
        {
            m_tank.Deleting(a_sd);
        }

        base.Deleting();
    }
    internal void MakeTank(ScenarioDetail a_sd, UserFieldDefinitionManager a_udfManager, PTTransmission a_t, ISystemLogger a_errorReporter, IScenarioDataChanges a_dataChanges)
    {
        //TODO: this all needs to go
        ////string warehouseText = " " + APSCommon.Localization.Localizer.GetString("Tank Storage");
        //PT.ERPTransmissions.WarehouseT.Warehouse warehouse = new (ExternalId + warehouseText, Name + warehouseText, "", "", "");
        //m_tank = new ResourceWarehouse(a_sd.IdGen.NextID(), warehouse, a_sd.IdGen, this, a_t, a_sd, a_errorReporter);
        //m_tank.Update(warehouse, false,false, false, false, false, false, false, false, false, false, false, a_udfManager, a_t, a_sd, a_errorReporter, a_dataChanges, true);
        //a_dataChanges.WarehouseChanges.AddedObject(m_tank.Id);
    }

    internal Inventory GetOrCreateInventory(long a_clock, ScenarioDetail a_sd, BaseIdGenerator a_idGen, Item a_item, UserFieldDefinitionManager a_udfManager, IScenarioDataChanges a_dataChanges)
    {
        Inventory inventory;

        if (m_tank.Inventories.Contains(a_item.Id))
        {
            inventory = m_tank.Inventories[a_item.Id];
        }
        else
        {
            WarehouseT.Inventory invT = new (a_item.Name);
            invT.ItemExternalId = a_item.ExternalId;
            invT.MaxInventory = 0;
            invT.LeadTime = new TimeSpan(7, 0, 0, 0);
            inventory = new Inventory(a_idGen.NextID(), m_tank, a_sd, a_item, invT, a_idGen, a_udfManager);
            inventory.ResetSimulationStateVariables(a_clock, a_sd);
            m_tank.Inventories.Add(inventory);
        }

        return inventory;
    }

    /// <summary>
    /// Back Calculate an activity start time from a start time and duration
    /// </summary>
    /// <param name="a_clockDate"></param>
    /// <param name="a_dateTime"></param>
    /// <param name="a_headstart"></param>
    /// <returns></returns>
    public DateTime GetAdjustedDateTime(long a_clockDate, long a_dateTime, long a_headstart)
    {
        return capacityProfile.GetAdjustedDateTime(new DateTime(a_clockDate), new DateTime(a_dateTime), new TimeSpan(a_headstart));
    }

    public DateTime GetAdjustedDateTime(DateTime a_clockDate, DateTime a_dateTime, TimeSpan a_headstart)
    {
        return capacityProfile.GetAdjustedDateTime(a_clockDate, a_dateTime, a_headstart);
    }

    public InternalActivity GetLastRunActivity(long a_simClock = -1)
    {
        return m_cleanoutHistoryData.GetLastRunActivityBeforeSimClock(a_simClock);
    }

    /// <summary>
    /// Get activity before the clock by index.
    /// The collection is sorted by closest to the clock date.
    /// </summary>
    /// <param name="a_index"></param>
    /// <returns></returns>
    public InternalActivity GetHistoricalActivityByIndex(int a_index)
    {
        return m_cleanoutHistoryData.GetByIndex(a_index);
    }

    private List<CompatibilityCodeTable> m_compatibilityTables = new();

    public IEnumerable<CleanoutHistory> HistoricalActivityEnumerator()
    {
        foreach (CleanoutHistory act in m_cleanoutHistoryData)
        {
            yield return act;
        }
    }

    /// <summary>
    /// Resources that are part of the same CompatibilityGroup can only run Operations concurrently if they have the same Compatibility Code.  For example, if two machines are fed by the same material input
    /// pipe then at any point in time they can only run products that use that same material.
    /// </summary>
    public IEnumerable<CompatibilityCodeTable> CompatibilityTables
    {
        get => m_compatibilityTables;
        //internal set => m_compatibilityTables = value;
    }
    /// <summary>
    /// Unlinks the specified compatibility code from the resource
    /// </summary>
    /// <param name="a_tableToRemove"></param>
    public void RemoveCompatibilityCode(CompatibilityCodeTable a_tableToRemove)
    {
        m_compatibilityTables.Remove(a_tableToRemove);
    }
    /// <summary>
    /// Links the resource to the specified compatibility table
    /// </summary>
    /// <param name="a_tableToAdd"></param>
    public void AddCompatibilityCode(CompatibilityCodeTable a_tableToAdd)
    {
        m_compatibilityTables.Add(a_tableToAdd);
    }

    protected void ScheduleCompatibilityCode(ScenarioDetail a_sd, Batch a_batch, InternalActivity a_activity, ResourceRequirement a_rr, long a_startTicks, long a_endTicks, SchedulableInfo a_si)
    {
        if (a_activity.Operation.UseCompatibilityCode)
        {
            foreach (CompatibilityCodeTable compatibilityTable in m_compatibilityTables)
            {
                compatibilityTable.CodeScheduled(new Interval(a_startTicks, a_endTicks), a_activity.Operation.CompatibilityCode);
            }
        }
    }
}