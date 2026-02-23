using System.ComponentModel;
using System.Drawing;
using System.Text;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Attributes;
using PT.Common.Debugging;
using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.ERPTransmissions;
using PT.Scheduler.Schedule;
using PT.Scheduler.Schedule.Operation;
using PT.Scheduler.Simulation.Scheduler.AlternatePaths;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Flags;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// A portion of the Operation to be completed.  There is one InternalActivity per Operation unless it is Split into multiple Activities.
/// </summary>
public partial class InternalActivity : BaseActivity, IPTDeserializable
{
    public new const int UNIQUE_ID = 289;

    #region IPTSerializable Members
    private class DeserializationTemp
    {
        internal readonly Dictionary<int, ResourceKey> LockedResourceKeys = new(); // [BATCH]
    }

    private DeserializationTemp m_deserializationTemp;

    public InternalActivity(IReader a_reader)
        : base(a_reader)
    {
        //InternalActivity
        if (a_reader.VersionNumber >= 13007)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out int val);
            m_productionStatus = (InternalActivityDefs.productionStatuses)val;

            m_bufferInfo = new ActivityBufferInfo(a_reader);

            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRun);
            a_reader.Read(out m_reportedSetup);
            a_reader.Read(out m_reportedPostProcessing);
            a_reader.Read(out m_reportedStorage);
            a_reader.Read(out m_reportedClean);
            a_reader.Read(out m_reportedCleanOutGrade);

            a_reader.Read(out m_reportedEndOfRunTicks);
            a_reader.Read(out m_reportedEndOfPostProcessingTicks);
            a_reader.Read(out m_reportedEndOfStorageTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledStartTimeCleanNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimeCleanNoResources);

            a_reader.Read(out m_resourceTransferTimeTicksAtActivityFinishTime);

            m_persistentFlags = new BoolVector32(a_reader);

            //Deserialize Locks
            m_deserializationTemp = new DeserializationTemp();

            a_reader.Read(out int count);
            for (int i = 0; i < count; ++i)
            {
                a_reader.Read(out int rrIdx);
                ResourceKey rk = new ResourceKey(a_reader);

                m_deserializationTemp.LockedResourceKeys.Add(rrIdx, rk);
            }

            m_productionInfoOverride = new ProductionInfo(a_reader);
            a_reader.Read(out m_comments2);

            //Read all existing IDs
            a_reader.Read(out int idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId inventoryId = new(a_reader);
                BaseId lotId = new(a_reader);

                m_generatedLotIds.Add(inventoryId, lotId);
            }

            a_reader.Read(out m_simultaneousSequenceIdx);
            a_reader.Read(out m_productQtys);
            a_reader.Read(out count);
            m_mrQtys = new Dictionary<BaseId, decimal>(count);
            for (int i = 0; i < count; i++)
            {
                BaseId mrId = new (a_reader);
                a_reader.Read(out decimal mrQty);
                m_mrQtys.Add(mrId, mrQty);
            }
        }
        else if (a_reader.VersionNumber >= 13005)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out int val);
            m_productionStatus = (InternalActivityDefs.productionStatuses)val;

            m_bufferInfo = new ActivityBufferInfo(a_reader);

            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRun);
            a_reader.Read(out m_reportedSetup);
            a_reader.Read(out m_reportedPostProcessing);
            a_reader.Read(out m_reportedStorage);
            a_reader.Read(out m_reportedClean);
            a_reader.Read(out m_reportedCleanOutGrade);

            a_reader.Read(out m_reportedEndOfRunTicks);
            a_reader.Read(out m_reportedEndOfPostProcessingTicks);
            a_reader.Read(out m_reportedEndOfStorageTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledStartTimeCleanNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimeCleanNoResources);

            a_reader.Read(out m_resourceTransferTimeTicksAtActivityFinishTime);

            m_persistentFlags = new BoolVector32(a_reader);

            //Deserialize Locks
            m_deserializationTemp = new DeserializationTemp();

            a_reader.Read(out int count);
            for (int i = 0; i < count; ++i)
            {
                a_reader.Read(out int rrIdx);
                ResourceKey rk = new ResourceKey(a_reader);

                m_deserializationTemp.LockedResourceKeys.Add(rrIdx, rk);
            }

            m_productionInfoOverride = new ProductionInfo(a_reader);
            a_reader.Read(out m_comments2);

            //Read all existing IDs
            a_reader.Read(out int idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId inventoryId = new(a_reader);
                BaseId lotId = new(a_reader);

                m_generatedLotIds.Add(inventoryId, lotId);
            }

            a_reader.Read(out m_simultaneousSequenceIdx);
            a_reader.Read(out m_productQtys);
        }
        else if(a_reader.VersionNumber >= 12556)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out int val);
            m_productionStatus = (InternalActivityDefs.productionStatuses)val;
            a_reader.Read(out long m_jitStartTicks);
            a_reader.Read(out long m_needTicks);
            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRun);
            a_reader.Read(out m_reportedSetup);
            a_reader.Read(out m_reportedPostProcessing);
            a_reader.Read(out m_reportedStorage);
            a_reader.Read(out m_reportedClean);
            a_reader.Read(out m_reportedCleanOutGrade);

            a_reader.Read(out m_reportedEndOfRunTicks);
            a_reader.Read(out m_reportedEndOfPostProcessingTicks);
            a_reader.Read(out m_reportedEndOfStorageTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledStartTimeCleanNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimeCleanNoResources);

            a_reader.Read(out m_resourceTransferTimeTicksAtActivityFinishTime);

            m_persistentFlags = new BoolVector32(a_reader);

            //Deserialize Locks
            m_deserializationTemp = new DeserializationTemp();

            a_reader.Read(out int count);
            for (int i = 0; i < count; ++i)
            {
                a_reader.Read(out int rrIdx);
                ResourceKey rk = new ResourceKey(a_reader);

                m_deserializationTemp.LockedResourceKeys.Add(rrIdx, rk);
            }

            m_productionInfoOverride = new ProductionInfo(a_reader);
            a_reader.Read(out m_comments2);

            //Read all existing IDs
            a_reader.Read(out int idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId inventoryId = new(a_reader);
                BaseId lotId = new(a_reader);

                m_generatedLotIds.Add(inventoryId, lotId);
            }

            a_reader.Read(out m_simultaneousSequenceIdx);
            a_reader.Read(out m_productQtys);
        }
        else if(a_reader.VersionNumber >= 12525)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out int val);
            m_productionStatus = (InternalActivityDefs.productionStatuses)val;
            a_reader.Read(out long m_jitStartTicks);
            a_reader.Read(out long m_needTicks);
            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRun);
            a_reader.Read(out m_reportedSetup);
            a_reader.Read(out m_reportedPostProcessing);
            a_reader.Read(out m_reportedStorage);
            a_reader.Read(out m_reportedClean);
            a_reader.Read(out m_reportedCleanOutGrade);

            a_reader.Read(out m_reportedEndOfRunTicks);
            a_reader.Read(out m_reportedEndOfPostProcessingTicks);
            a_reader.Read(out m_reportedEndOfStorageTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledStartTimeCleanNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimeCleanNoResources);

            a_reader.Read(out m_resourceTransferTimeTicksAtActivityFinishTime);

            m_persistentFlags = new BoolVector32(a_reader);

            //Deserialize Locks
            m_deserializationTemp = new DeserializationTemp();

            a_reader.Read(out int count);
            for (int i = 0; i < count; ++i)
            {
                a_reader.Read(out int rrIdx);
                ResourceKey rk = new ResourceKey(a_reader);

                m_deserializationTemp.LockedResourceKeys.Add(rrIdx, rk);
            }

            m_productionInfoOverride = new ProductionInfo(a_reader);
            a_reader.Read(out m_comments2);

            //Read all existing IDs
            a_reader.Read(out int idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId inventoryId = new(a_reader);
                BaseId lotId = new(a_reader);

                m_generatedLotIds.Add(inventoryId, lotId);
            }

            a_reader.Read(out m_simultaneousSequenceIdx);
        }
        else if (a_reader.VersionNumber >= 12522)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out int val);
            m_productionStatus = (InternalActivityDefs.productionStatuses)val;
            a_reader.Read(out long m_jitStartTicks);
            a_reader.Read(out long m_needTicks);
            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRun);
            a_reader.Read(out m_reportedSetup);
            a_reader.Read(out m_reportedPostProcessing);
            a_reader.Read(out m_reportedStorage);
            a_reader.Read(out m_reportedClean);
            a_reader.Read(out m_reportedCleanOutGrade);

            a_reader.Read(out m_reportedEndOfRunTicks);
            a_reader.Read(out m_reportedEndOfPostProcessingTicks);
            a_reader.Read(out m_reportedEndOfStorageTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledStartTimeCleanNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimeCleanNoResources);

            a_reader.Read(out m_resourceTransferTimeTicksAtActivityFinishTime);

            m_persistentFlags = new BoolVector32(a_reader);

            DeserializeLockedResourcesForBackwardCompatibility(a_reader);

            m_productionInfoOverride = new ProductionInfo(a_reader);
            a_reader.Read(out m_comments2);

            //Read all existing IDs
            a_reader.Read(out int idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId inventoryId = new(a_reader);
                BaseId lotId = new(a_reader);

                m_generatedLotIds.Add(inventoryId, lotId);
            }

            a_reader.Read(out m_simultaneousSequenceIdx);
        }
        else if (a_reader.VersionNumber >= 12521)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out int val);
            m_productionStatus = (InternalActivityDefs.productionStatuses)val;
            a_reader.Read(out long m_jitStartTicks);
            a_reader.Read(out long m_needTicks);
            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRun);
            a_reader.Read(out m_reportedSetup);
            a_reader.Read(out m_reportedPostProcessing);
            a_reader.Read(out m_reportedStorage);
            a_reader.Read(out m_reportedClean);
            a_reader.Read(out m_reportedCleanOutGrade);

            a_reader.Read(out m_reportedEndOfRunTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledStartTimeCleanNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimeCleanNoResources);

            a_reader.Read(out m_resourceTransferTimeTicksAtActivityFinishTime);

            m_persistentFlags = new BoolVector32(a_reader);

            DeserializeLockedResourcesForBackwardCompatibility(a_reader);

            m_productionInfoOverride = new ProductionInfo(a_reader);
            a_reader.Read(out m_comments2);

            //Read all existing IDs
            a_reader.Read(out int idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId inventoryId = new(a_reader);
                BaseId lotId = new(a_reader);

                m_generatedLotIds.Add(inventoryId, lotId);
            }

            a_reader.Read(out m_simultaneousSequenceIdx);
        }
        else if (a_reader.VersionNumber >= 12435)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out int val);
            m_productionStatus = (InternalActivityDefs.productionStatuses)val;
            a_reader.Read(out long m_jitStartTicks);
            a_reader.Read(out long m_needTicks);
            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRun);
            a_reader.Read(out m_reportedSetup);
            a_reader.Read(out m_reportedPostProcessing);
            a_reader.Read(out m_reportedClean);
            a_reader.Read(out m_reportedCleanOutGrade);

            a_reader.Read(out m_reportedEndOfRunTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledStartTimeCleanNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimeCleanNoResources);

            a_reader.Read(out m_resourceTransferTimeTicksAtActivityFinishTime);

            m_persistentFlags = new BoolVector32(a_reader);

            //Deserialize Locks
            m_deserializationTemp = new DeserializationTemp();

            a_reader.Read(out int count);
            for (int i = 0; i < count; ++i)
            {
                a_reader.Read(out int rrIdx);
                ResourceKey rk = new ResourceKey(a_reader);

                m_deserializationTemp.LockedResourceKeys.Add(rrIdx, rk);
            }

            m_productionInfoOverride = DeserializeProductionInfo(a_reader);
            a_reader.Read(out m_comments2);

            //Read all existing IDs
            a_reader.Read(out int idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId inventoryId = new(a_reader);
                BaseId lotId = new(a_reader);

                m_generatedLotIds.Add(inventoryId, lotId);
            }

            a_reader.Read(out m_simultaneousSequenceIdx);

            a_reader.Read(out idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId resId = new(a_reader);
                //m_resourceTrackingSequenceSetup.Add(resId);
            }
        }
        else if (a_reader.VersionNumber >= 12416)
        {
            m_bools = new BoolVector32(a_reader);
            DeserializeProductionStatusForPreStoringStage(a_reader);
            a_reader.Read(out long m_jitStartTicks);
            a_reader.Read(out long m_needTicks);
            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out int val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRun);
            a_reader.Read(out m_reportedSetup);
            a_reader.Read(out m_reportedPostProcessing);
            a_reader.Read(out m_reportedClean);
            a_reader.Read(out m_reportedCleanOutGrade);

            a_reader.Read(out m_reportedEndOfRunTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledStartTimeCleanNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimeCleanNoResources);

            a_reader.Read(out m_resourceTransferTimeTicksAtActivityFinishTime);

            m_persistentFlags = new BoolVector32(a_reader);

            DeserializeLockedResourcesForBackwardCompatibility(a_reader);

            m_productionInfoOverride = DeserializeProductionInfo(a_reader);
            a_reader.Read(out m_comments2);

            //Read all existing IDs
            a_reader.Read(out int idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId inventoryId = new(a_reader);
                BaseId lotId = new(a_reader);

                m_generatedLotIds.Add(inventoryId, lotId);
            }

            a_reader.Read(out m_simultaneousSequenceIdx);

            a_reader.Read(out idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId resId = new(a_reader);
                //m_resourceTrackingSequenceSetup.Add(resId);
            }
        }
        else if (a_reader.VersionNumber >= 12319)
        {
            m_bools = new BoolVector32(a_reader);
            DeserializeProductionStatusForPreStoringStage(a_reader);
            a_reader.Read(out long m_jitStartTicks);
            a_reader.Read(out long m_needTicks);
            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out int val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRun);
            a_reader.Read(out m_reportedSetup);
            a_reader.Read(out m_reportedPostProcessing);
            a_reader.Read(out m_reportedClean);

            a_reader.Read(out m_reportedEndOfRunTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledStartTimeCleanNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimeCleanNoResources);

            a_reader.Read(out m_resourceTransferTimeTicksAtActivityFinishTime);

            m_persistentFlags = new BoolVector32(a_reader);

            DeserializeLockedResourcesForBackwardCompatibility(a_reader);

            m_productionInfoOverride = DeserializeProductionInfo(a_reader);
            a_reader.Read(out m_comments2);

            //Read all existing IDs
            a_reader.Read(out int idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId inventoryId = new (a_reader);
                BaseId lotId = new (a_reader);

                m_generatedLotIds.Add(inventoryId, lotId);
            }

            a_reader.Read(out m_simultaneousSequenceIdx);

            a_reader.Read(out idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId resId = new(a_reader);
                //m_resourceTrackingSequenceSetup.Add(resId);
            }
        }
        else if (a_reader.VersionNumber >= 12302)
        {
            m_bools = new BoolVector32(a_reader);
            DeserializeProductionStatusForPreStoringStage(a_reader);
            a_reader.Read(out long m_jitStartTicks);
            a_reader.Read(out long m_needTicks);
            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out int val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRun);
            a_reader.Read(out m_reportedSetup);
            a_reader.Read(out m_reportedPostProcessing);
            a_reader.Read(out m_reportedClean);

            a_reader.Read(out m_reportedEndOfRunTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledStartTimeCleanNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimeCleanNoResources);

            a_reader.Read(out m_resourceTransferTimeTicksAtActivityFinishTime);

            m_persistentFlags = new BoolVector32(a_reader);

            DeserializeLockedResourcesForBackwardCompatibility(a_reader);

            m_productionInfoOverride = DeserializeProductionInfo(a_reader);
            a_reader.Read(out m_comments2);

            //Read all existing IDs
            a_reader.Read(out int idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId inventoryId = new (a_reader);
                BaseId lotId = new (a_reader);

                m_generatedLotIds.Add(inventoryId, lotId);
            }

            a_reader.Read(out m_simultaneousSequenceIdx);
        }
        else if (a_reader.VersionNumber >= 12111)
        {
            int val;
            DeserializeProductionStatusForPreStoringStage(a_reader);
            a_reader.Read(out long m_jitStartTicks);
            a_reader.Read(out long m_needTicks);
            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRun);
            a_reader.Read(out m_reportedSetup);
            a_reader.Read(out m_reportedPostProcessing);

            a_reader.Read(out m_reportedEndOfRunTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);

            a_reader.Read(out m_resourceTransferTimeTicksAtActivityFinishTime);

            m_persistentFlags = new BoolVector32(a_reader);

            DeserializeLockedResourcesForBackwardCompatibility(a_reader);

            m_productionInfoOverride = DeserializeProductionInfo(a_reader);
            a_reader.Read(out m_comments2);

            //Read all existing IDs
            a_reader.Read(out int idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId inventoryId = new (a_reader);
                BaseId lotId = new (a_reader);

                m_generatedLotIds.Add(inventoryId, lotId);
            }

            a_reader.Read(out m_simultaneousSequenceIdx);
        }
        else if (a_reader.VersionNumber >= 12055) //Added generatedLotIds
        {
            int val;
            DeserializeProductionStatusForPreStoringStage(a_reader);
            a_reader.Read(out long m_jitStartTicks);
            a_reader.Read(out long m_needTicks);
            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRun);
            a_reader.Read(out m_reportedSetup);
            a_reader.Read(out m_reportedPostProcessing);

            a_reader.Read(out m_reportedEndOfRunTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);

            a_reader.Read(out m_resourceTransferTimeTicksAtActivityFinishTime);

            m_persistentFlags = new BoolVector32(a_reader);

            DeserializeLockedResourcesForBackwardCompatibility(a_reader);

            m_productionInfoOverride = DeserializeProductionInfo(a_reader);
            a_reader.Read(out m_comments2);

            //Read all existing IDs
            a_reader.Read(out int idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId inventoryId = new (a_reader);
                BaseId lotId = new (a_reader);

                m_generatedLotIds.Add(inventoryId, lotId);
            }
        }
        else if (a_reader.VersionNumber >= 12000) //468 serialization for v12 backwards compatibility
        {
            int val;
            DeserializeProductionStatusForPreStoringStage(a_reader);
            a_reader.Read(out long m_jitStartTicks);
            a_reader.Read(out long m_needTicks);
            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRun);
            a_reader.Read(out m_reportedSetup);
            a_reader.Read(out m_reportedPostProcessing);

            a_reader.Read(out m_reportedEndOfRunTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);

            a_reader.Read(out m_resourceTransferTimeTicksAtActivityFinishTime);

            m_persistentFlags = new BoolVector32(a_reader);

            DeserializeLockedResourcesForBackwardCompatibility(a_reader);

            m_productionInfoOverride = DeserializeProductionInfo(a_reader);
            a_reader.Read(out m_comments2);
        }
        else if (a_reader.VersionNumber >= 756) //Added generatedLotIds
        {
            int val;
            DeserializeProductionStatusForPreStoringStage(a_reader);
            a_reader.Read(out long m_jitStartTicks);
            a_reader.Read(out long m_needTicks);
            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRun);
            a_reader.Read(out m_reportedSetup);
            a_reader.Read(out m_reportedPostProcessing);

            a_reader.Read(out m_reportedEndOfRunTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);

            a_reader.Read(out m_resourceTransferTimeTicksAtActivityFinishTime);

            m_persistentFlags = new BoolVector32(a_reader);

            DeserializeLockedResourcesForBackwardCompatibility(a_reader);

            m_productionInfoOverride = DeserializeProductionInfo(a_reader);
            a_reader.Read(out m_comments2);

            //Read all existing IDs
            a_reader.Read(out int idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId inventoryId = new (a_reader);
                BaseId lotId = new (a_reader);

                m_generatedLotIds.Add(inventoryId, lotId);
            }
        }
        else if (a_reader.VersionNumber >= 468)
        {
            int val;
            DeserializeProductionStatusForPreStoringStage(a_reader);
            a_reader.Read(out long m_jitStartTicks);
            a_reader.Read(out long m_needTicks);
            a_reader.Read(out m_reportedGoodQty);
            a_reader.Read(out m_reportedScrapQty);
            a_reader.Read(out val);
            m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
            a_reader.Read(out m_nbrOfPeople);
            a_reader.Read(out m_comments);

            a_reader.Read(out m_reportedRun);
            a_reader.Read(out m_reportedSetup);
            a_reader.Read(out m_reportedPostProcessing);

            a_reader.Read(out m_reportedEndOfRunTicks);

            a_reader.Read(out m_scheduledStartTimePostProcessingNoResources);
            a_reader.Read(out m_scheduledFinishTimePostProcessingNoResources);

            a_reader.Read(out m_resourceTransferTimeTicksAtActivityFinishTime);

            m_persistentFlags = new BoolVector32(a_reader);

            DeserializeLockedResourcesForBackwardCompatibility(a_reader);

            m_productionInfoOverride = DeserializeProductionInfo(a_reader);
            a_reader.Read(out m_comments2);
        }

#if TEST
            CompareDesyncResults();
#endif
    }

    protected virtual ProductionInfo DeserializeProductionInfo(IReader a_reader)
    {
        return new ProductionInfo(a_reader);
    }

    //For backwards compatibility
    private void DeserializeLockedResourcesForBackwardCompatibility(IReader a_reader)
    {
        m_deserializationTemp = new DeserializationTemp();

        a_reader.Read(out int count);
        for (int i = 0; i < count; ++i)
        {
            a_reader.Read(out bool locked);
            if (locked)
            {
                ResourceKey rk = new ResourceKey(a_reader);
                m_deserializationTemp.LockedResourceKeys.Add(i, rk);
            }
        }
    }

    private void DeserializeProductionStatusForPreStoringStage(IReader a_reader)
    {
        a_reader.Read(out int prodStatus);
        if (prodStatus >= 6)
        {
            prodStatus++;
        }

        m_productionStatus = (InternalActivityDefs.productionStatuses)prodStatus;
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        a_writer.Write((int)m_productionStatus);
        m_bufferInfo.Serialize(a_writer);
        a_writer.Write(m_reportedGoodQty);
        a_writer.Write(m_reportedScrapQty);
        a_writer.Write((int)m_peopleUsage);
        a_writer.Write(m_nbrOfPeople);
        a_writer.Write(m_comments);

        a_writer.Write(m_reportedRun);
        a_writer.Write(m_reportedSetup);
        a_writer.Write(m_reportedPostProcessing);
        a_writer.Write(m_reportedStorage);
        a_writer.Write(m_reportedClean);
        a_writer.Write(m_reportedCleanOutGrade);

        // Write the scheduled Tick info.
        a_writer.Write(m_reportedEndOfRunTicks);
        a_writer.Write(m_reportedEndOfPostProcessingTicks);
        a_writer.Write(m_reportedEndOfStorageTicks);

        a_writer.Write(m_scheduledStartTimePostProcessingNoResources);
        a_writer.Write(m_scheduledStartTimeCleanNoResources);
        a_writer.Write(m_scheduledFinishTimePostProcessingNoResources);
        a_writer.Write(m_scheduledFinishTimeCleanNoResources);

        a_writer.Write(m_resourceTransferTimeTicksAtActivityFinishTime);

        m_persistentFlags.Serialize(a_writer);

        SerializeLockedResources(a_writer);

        m_productionInfoOverride.Serialize(a_writer);
        a_writer.Write(m_comments2);

        a_writer.Write(m_generatedLotIds.Count);
        foreach (KeyValuePair<BaseId, BaseId> ids in m_generatedLotIds)
        {
            ids.Key.Serialize(a_writer);
            ids.Value.Serialize(a_writer);
        }

        a_writer.Write(m_simultaneousSequenceIdx);
        a_writer.Write(m_productQtys);

        a_writer.Write(m_mrQtys.Count);
        foreach (KeyValuePair<BaseId, decimal> mrQty in m_mrQtys)
        {
            mrQty.Key.Serialize(a_writer);
            a_writer.Write(mrQty.Value);
        }
    }

    private void SerializeLockedResources(IWriter a_writer)
    {
        a_writer.Write(m_lockedResources.Count);
        foreach ((int key, Resource value) in m_lockedResources)
        {
            a_writer.Write(key);
            value.GetKey().Serialize(a_writer);
        }
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    public void RestoreReferences(InternalOperation a_op)
    {
        m_operation = a_op;

        m_lockedResources = new Dictionary<int, Resource>();
        foreach ((int key, ResourceKey rk) in m_deserializationTemp.LockedResourceKeys)
        {
            Plant plant = a_op.ManufacturingOrder.ScenarioDetail.PlantManager.GetById(rk.Plant);
            Department department = plant.Departments.GetById(rk.Department);
            Resource resource = department.Resources.GetById(rk.Resource);
            m_lockedResources.Add(key, resource);
        }

        m_deserializationTemp = null;
    }
    #endregion

    #region Construction
    internal InternalActivity(long a_externalIdPTDefaultNbr, BaseId a_id, InternalOperation a_operation, JobT.InternalOperation a_opT)
        : base(a_externalIdPTDefaultNbr, a_id)
    {
        m_operation = a_operation;
        InitializeResourceLocks();

        m_productionInfoOverride = a_operation.ProductionInfo.Clone();
    }

    /// <summary>
    /// Create a NEW Activity from an original.
    /// </summary>
    internal InternalActivity(long a_externalIdPTDefaultNbr, BaseId a_id, InternalOperation a_operation, InternalActivity a_originalActivity)
        : base(a_externalIdPTDefaultNbr, a_id)
    {
        m_operation = a_operation;

        //Initialize values from original activity
        SetValuesFromSplitOriginal(a_originalActivity);

        m_nbrOfPeople = a_originalActivity.NbrOfPeople;
        m_peopleUsage = a_originalActivity.PeopleUsage;
        m_productionStatus = InternalActivityDefs.productionStatuses.Ready;

        m_productionInfoOverride = a_originalActivity.m_productionInfoOverride.Clone();

        m_resourceProductionInfos = a_originalActivity.m_resourceProductionInfos.Clone();
        m_bufferInfo = new ActivityBufferInfo(a_originalActivity.BufferInfo);

        InitializeResourceLocks(a_originalActivity);
        //TODO: Do we need to reset the schedule/sim flags?
        m_persistentFlags = a_originalActivity.m_persistentFlags;
    }

    internal InternalActivity(BaseId a_id, ScenarioDetail a_sd, InternalOperation a_operation, JobT.InternalOperation a_opT, JobT.InternalActivity a_jobTInternalActivity, bool a_isErpUpdate)
        : base(a_id, a_sd, a_jobTInternalActivity)
    {
        m_operation = a_operation;

        ProductionStatus = a_jobTInternalActivity.ProductionStatus;
        Paused = a_jobTInternalActivity.Paused;
        PeopleUsage = a_jobTInternalActivity.PeopleUsage;
        NbrOfPeople = a_jobTInternalActivity.NbrOfPeople;
        ReportedGoodQty = a_sd.ScenarioOptions.RoundQty(a_jobTInternalActivity.ReportedGoodQty);
        ReportedRunSpan = a_jobTInternalActivity.ReportedRunSpan;
        ReportedScrapQty = a_sd.ScenarioOptions.RoundQty(a_jobTInternalActivity.ReportedScrapQty);
        ReportedSetupSpan = a_jobTInternalActivity.ReportedSetupSpan;
        ReportedPostProcessingSpan = a_jobTInternalActivity.ReportedPostProcessingSpan;
        ReportedStorage = a_jobTInternalActivity.ReportedStorageTicks;

        if (a_jobTInternalActivity.ReportedEndOfPostProcessingDateSet)
        {
            ReportedEndOfPostProcessingTicks = a_jobTInternalActivity.ReportedEndOfPostProcessingDate.Ticks;
        }

        if (a_jobTInternalActivity.ReportedEndOfStorageDateSet)
        {
            ReportedEndOfStorageTicks = a_jobTInternalActivity.ReportedEndOfStorageDate.Ticks;
        }

        if (a_jobTInternalActivity.ReportedStartOfProcessingDateSet)
        {
            ReportedProcessingStartTicks = a_jobTInternalActivity.ReportedStartOfProcessingDate.Ticks;
        }

        if (a_jobTInternalActivity.ReportedCleanIsSet)
        {
            ReportedCleanSpan = a_jobTInternalActivity.ReportedCleanSpan;
        }

        if (a_jobTInternalActivity.ReportedCleanGradeIsSet)
        {
            ReportedCleanoutGrade = a_jobTInternalActivity.ReportedCleanoutGrade;
        }

        if (a_jobTInternalActivity.ReportedEndOfRunDateSet)
        {
            ReportedEndOfRunTicks = a_jobTInternalActivity.ReportedEndOfRunDate.Ticks;
        }

        if (a_jobTInternalActivity.ReportedFinishDateSet)
        {
            ReportedFinishDateTicks = a_jobTInternalActivity.ReportedFinishDateTicks;
        }

        Comments = a_jobTInternalActivity.Comments;
        Comments2 = a_jobTInternalActivity.Comments2;
        BatchAmount = a_jobTInternalActivity.BatchAmount;
        SetupActualResourcesUsed(a_jobTInternalActivity.ActualResourcesUsedCSV, a_sd.PlantManager, a_jobTInternalActivity.ExternalId, a_isErpUpdate);

        m_productionInfoOverride = Operation.ProductionInfo.Clone();

        #region OnlyAllowManualUpdatesOnly Flags
        if (a_jobTInternalActivity.ProductionInfoFlags.CycleManualUpdateOnlySet && m_productionInfoOverride.OnlyAllowManualUpdatesToCycleSpan != a_jobTInternalActivity.ProductionInfoFlags.CycleManualUpdateOnly)
        {
            m_productionInfoOverride.OnlyAllowManualUpdatesToCycleSpan = a_jobTInternalActivity.ProductionInfoFlags.CycleManualUpdateOnly;
        }

        if (a_jobTInternalActivity.ProductionInfoFlags.QtyPerCycleSet && m_productionInfoOverride.OnlyAllowManualUpdatesToQtyPerCycle != a_jobTInternalActivity.ProductionInfoFlags.QtyPerCycle)
        {
            m_productionInfoOverride.OnlyAllowManualUpdatesToQtyPerCycle = a_jobTInternalActivity.ProductionInfoFlags.QtyPerCycle;
        }

        if (a_jobTInternalActivity.ProductionInfoFlags.SetupManualUpdateOnlySet && m_productionInfoOverride.OnlyAllowManualUpdatesToSetupSpan != a_jobTInternalActivity.ProductionInfoFlags.SetupManualUpdateOnly)
        {
            m_productionInfoOverride.OnlyAllowManualUpdatesToSetupSpan = a_jobTInternalActivity.ProductionInfoFlags.SetupManualUpdateOnly;
        }

        if (a_jobTInternalActivity.ProductionInfoFlags.PostProcessingManualUpdateOnlySet && m_productionInfoOverride.OnlyAllowManualUpdatesToPostProcessingSpan != a_jobTInternalActivity.ProductionInfoFlags.PostProcessingManualUpdateOnly)
        {
            m_productionInfoOverride.OnlyAllowManualUpdatesToPostProcessingSpan = a_jobTInternalActivity.ProductionInfoFlags.PostProcessingManualUpdateOnly;
        }

        if (a_jobTInternalActivity.ProductionInfoFlags.CleanManualUpdateOnlySet && m_productionInfoOverride.OnlyAllowManualUpdatesToCleanSpan != a_jobTInternalActivity.ProductionInfoFlags.CleanManualUpdateOnly)
        {
            m_productionInfoOverride.OnlyAllowManualUpdatesToCleanSpan = a_jobTInternalActivity.ProductionInfoFlags.CleanManualUpdateOnly;
        }

        if (a_jobTInternalActivity.ProductionInfoFlags.PlanningScrapPercentSet && m_productionInfoOverride.OnlyAllowManualUpdatesToPlanningScrapPercent != a_jobTInternalActivity.ProductionInfoFlags.PlanningScrapPercent)
        {
            m_productionInfoOverride.OnlyAllowManualUpdatesToPlanningScrapPercent = a_jobTInternalActivity.ProductionInfoFlags.PlanningScrapPercent;
        }

        if (a_jobTInternalActivity.ProductionInfoFlags.StorageManualUpdateOnlySet && m_productionInfoOverride.OnlyAllowManualUpdatesToStorageSpan != a_jobTInternalActivity.ProductionInfoFlags.StorageManualUpdateOnly)
        {
            m_productionInfoOverride.OnlyAllowManualUpdatesToStorageSpan = a_jobTInternalActivity.ProductionInfoFlags.StorageManualUpdateOnly;
        }
        #endregion

        #region Override Flags
        if (a_jobTInternalActivity.ProductionInfoFlags.CycleSpanOverrideSet && m_productionInfoOverride.CycleSpanOverride != a_jobTInternalActivity.ProductionInfoFlags.CycleSpanOverride)
        {
            m_productionInfoOverride.CycleSpanOverride = a_jobTInternalActivity.ProductionInfoFlags.CycleSpanOverride;
        }

        if (a_jobTInternalActivity.ProductionInfoFlags.QtyPerCycleOverrideSet && m_productionInfoOverride.QtyPerCycleOverride != a_jobTInternalActivity.ProductionInfoFlags.QtyPerCycleOverride)
        {
            m_productionInfoOverride.QtyPerCycleOverride = a_jobTInternalActivity.ProductionInfoFlags.QtyPerCycleOverride;
        }

        if (a_jobTInternalActivity.ProductionInfoFlags.SetupSpanOverrideSet && m_productionInfoOverride.SetupSpanOverride != a_jobTInternalActivity.ProductionInfoFlags.SetupSpanOverride)
        {
            m_productionInfoOverride.SetupSpanOverride = a_jobTInternalActivity.ProductionInfoFlags.SetupSpanOverride;
            ProductionSetupCost = a_jobTInternalActivity.ProductionSetupCost;
        }

        if (a_jobTInternalActivity.ProductionInfoFlags.PostProcessingSpanOverrideSet && m_productionInfoOverride.PostProcessingSpanOverride != a_jobTInternalActivity.ProductionInfoFlags.PostProcessingSpanOverride)
        {
            m_productionInfoOverride.PostProcessingSpanOverride = a_jobTInternalActivity.ProductionInfoFlags.PostProcessingSpanOverride;
        }

        if (a_jobTInternalActivity.ProductionInfoFlags.CleanSpanOverrideSet && m_productionInfoOverride.CleanSpanOverride != a_jobTInternalActivity.ProductionInfoFlags.CleanSpanOverride)
        {
            m_productionInfoOverride.CleanSpanOverride = a_jobTInternalActivity.ProductionInfoFlags.CleanSpanOverride;
            CleanoutCost = a_jobTInternalActivity.CleanoutCost;
        }

        if (a_jobTInternalActivity.ProductionInfoFlags.StorageSpanOverrideSet && m_productionInfoOverride.StorageSpanOverride != a_jobTInternalActivity.ProductionInfoFlags.StorageSpanOverride)
        {
            m_productionInfoOverride.StorageSpanOverride = a_jobTInternalActivity.ProductionInfoFlags.StorageSpanOverride;
        }

        if (a_jobTInternalActivity.ProductionInfoFlags.PlanningScrapPercentOverrideSet && m_productionInfoOverride.PlanningScrapPercentOverride != a_jobTInternalActivity.ProductionInfoFlags.PlanningScrapPercentOverride)
        {
            m_productionInfoOverride.PlanningScrapPercentOverride = a_jobTInternalActivity.ProductionInfoFlags.PlanningScrapPercentOverride;
        }
        #endregion

        if (a_jobTInternalActivity.CycleSpanSet && m_productionInfoOverride.CycleSpanTicks != a_jobTInternalActivity.CycleSpan.Ticks)
        {
            m_productionInfoOverride.CycleSpanTicks = a_jobTInternalActivity.CycleSpan.Ticks;
        }

        if (a_jobTInternalActivity.QtyPerCycleSet && m_productionInfoOverride.QtyPerCycle != a_jobTInternalActivity.QtyPerCycle)
        {
            m_productionInfoOverride.QtyPerCycle = a_jobTInternalActivity.QtyPerCycle;
        }

        if (a_jobTInternalActivity.SetupSpanSet && m_productionInfoOverride.SetupSpanTicks != a_jobTInternalActivity.SetupSpan.Ticks)
        {
            m_productionInfoOverride.SetupSpanTicks = a_jobTInternalActivity.SetupSpan.Ticks;
        }

        if (a_jobTInternalActivity.PostProcessingSpanSet && m_productionInfoOverride.PostProcessingSpanTicks != a_jobTInternalActivity.PostProcessingSpan.Ticks)
        {
            m_productionInfoOverride.PostProcessingSpanTicks = a_jobTInternalActivity.PostProcessingSpan.Ticks;
        }

        if (a_jobTInternalActivity.CleanSpanSet && m_productionInfoOverride.CleanSpanTicks != a_jobTInternalActivity.CleanSpan.Ticks)
        {
            m_productionInfoOverride.CleanSpanTicks = a_jobTInternalActivity.CleanSpan.Ticks;
        }

        if (a_jobTInternalActivity.CleanOutGradeIsSet && m_productionInfoOverride.CleanoutGrade != a_jobTInternalActivity.CleanOutGrade)
        {
            m_productionInfoOverride.CleanoutGrade = a_jobTInternalActivity.CleanOutGrade;
        }

        if (a_jobTInternalActivity.PlanningScrapPercentSet && m_productionInfoOverride.PlanningScrapPercent != a_jobTInternalActivity.PlanningScrapPercent)
        {
            m_productionInfoOverride.PlanningScrapPercent = a_jobTInternalActivity.PlanningScrapPercent;
        }

        if (a_jobTInternalActivity.StorageSpanSet && m_productionInfoOverride.StorageSpanTicks != a_jobTInternalActivity.StorageSpan.Ticks)
        {
            m_productionInfoOverride.StorageSpanTicks = a_jobTInternalActivity.StorageSpan.Ticks;
        }

        if (a_jobTInternalActivity.ActivityManualUpdateOnlyIsSet && ActivityManualUpdateOnly != a_jobTInternalActivity.ActivityManualUpdateOnly)
        {
            ActivityManualUpdateOnly = a_jobTInternalActivity.ActivityManualUpdateOnly;
        }

        InitializeResourceLocks();
    }

    internal InternalActivity(BaseId a_id, InternalActivity a_sourceActivity, InternalOperation a_parentOperation)
        : base(a_id, a_sourceActivity)
    {
        m_operation = a_parentOperation;

        ProductionStatus = a_sourceActivity.ProductionStatus;
        Paused = a_sourceActivity.Paused;
        PeopleUsage = a_sourceActivity.PeopleUsage;
        NbrOfPeople = a_sourceActivity.NbrOfPeople;
        ReportedGoodQty = a_sourceActivity.ReportedGoodQty;
        ReportedRunSpan = a_sourceActivity.ReportedRunSpan;
        ReportedScrapQty = a_sourceActivity.ReportedScrapQty;
        ReportedSetupSpan = a_sourceActivity.ReportedSetupSpan;
        ReportedPostProcessingSpan = a_sourceActivity.ReportedPostProcessingSpan;
        ReportedProcessingStartTicks = a_sourceActivity.ReportedProcessingStartTicks;
        ReportedEndOfRunTicks = a_sourceActivity.ReportedEndOfRunTicks;
        ReportedFinishDateTicks = a_sourceActivity.ReportedFinishDateTicks;
        ReportedCleanSpan = a_sourceActivity.ReportedCleanSpan;
        ReportedCleanoutGrade = a_sourceActivity.ReportedCleanoutGrade;
        Comments = a_sourceActivity.Comments;
        Comments2 = a_sourceActivity.Comments2;
        BatchAmount = a_sourceActivity.BatchAmount;
        SetActualResourcesUsed(a_sourceActivity.ActualResourcesUsed);

        InitializeResourceLocks();

        m_productionInfoOverride = a_sourceActivity.m_productionInfoOverride.Clone();
    }

    public class InternalActivityException : PTException
    {
        public InternalActivityException(string a_e)
            : base(a_e) { }
    }
    #endregion

    #region Shared Properties
    private InternalActivityDefs.productionStatuses m_productionStatus = InternalActivityDefs.productionStatuses.Ready;

    /// <summary>
    /// returns true if m_productionStatus is PostProcessing.
    /// This is added to avoid calculating ProductionStatus (the property) if all we care about is whether status is PostProcessing.
    /// </summary>
    public bool IsPostProcessing => m_productionStatus == InternalActivityDefs.productionStatuses.PostProcessing;

    public bool IsCleaning => m_productionStatus == InternalActivityDefs.productionStatuses.Cleaning;

    private ICalculatedValueCache<decimal> m_setupGroupQtyCache;
    private ICalculatedValueCache<decimal> m_setupGroupHoursCache;
    private ICalculatedValueCache<decimal> m_productGroupQtyCache;
    private ICalculatedValueCache<decimal> m_productGroupHoursCache;

    /// <returns>Total quantity for sequential operations with the same setup code as this operation</returns>
    public decimal ProductGroupQty()
    {
        if (m_productGroupQtyCache.TryGetValue(out decimal cachedQty))
        {
            return cachedQty;
        }

        lock (m_productGroupQtyCache)
        {
            (decimal, decimal) calculateSetupGroupValues = CalculateProductGroupValues();
            //Cache both since we calculated them simultaneously
            m_productGroupQtyCache.CacheValue(calculateSetupGroupValues.Item1);
            m_productGroupHoursCache.CacheValue(calculateSetupGroupValues.Item2);
            return calculateSetupGroupValues.Item1;
        }
    }

    /// <returns>Total run time for sequential operations with the same setup code as this operation</returns>
    public decimal ProductGroupHours()
    {
        if (m_productGroupHoursCache.TryGetValue(out decimal cachedQty))
        {
            return cachedQty;
        }

        lock (m_productGroupHoursCache)
        {
            (decimal, decimal) calculateSetupGroupValues = CalculateProductGroupValues();
            //Cache both since we calculated them simultaneously
            m_productGroupQtyCache.CacheValue(calculateSetupGroupValues.Item1);
            m_productGroupHoursCache.CacheValue(calculateSetupGroupValues.Item2);
            return calculateSetupGroupValues.Item2;
        }
    }

    /// <summary>
    /// Calculates quantity and run time for this activity's current product code group
    /// All sequential activities with the same MO primary product as this activity will be summed.
    /// </summary>
    /// <returns>Whether any values were calculated</returns>
    private (decimal, decimal) CalculateProductGroupValues()
    {
        decimal totalQty = 0.0m;
        decimal totalRunTime = 0.0m;
        Product product = Operation.ManufacturingOrder.GetPrimaryProduct();
        if (product == null)
        {
            return (totalQty, totalRunTime);
        }

        BaseId productId = product.Id;
        bool sameCodeBlockContainsCurrentOperation = false; //This signifies if we have traversed this operation in the list of resource block nodes.
        ResourceBlockList primaryResourceBlocks = Batch.PrimaryResource?.Blocks;
        if (primaryResourceBlocks?.Count > 0)
        {
            ResourceBlockList.Node blockNode = primaryResourceBlocks.First;
            while (blockNode != null)
            {
                InternalOperation blockOperation = blockNode.Data?.Batch?.FirstActivity?.Operation;
                if (blockOperation?.ManufacturingOrder.GetPrimaryProduct()?.Id == productId)
                {
                    sameCodeBlockContainsCurrentOperation = sameCodeBlockContainsCurrentOperation || Operation.Id == blockOperation.Id;
                    totalQty += blockOperation.RequiredFinishQty;
                    totalRunTime += blockOperation.SchedulingHours;
                }
                else
                {
                    //This block doesn't have the matching setup code
                    if (sameCodeBlockContainsCurrentOperation)
                    {
                        //search complete. Return the calculated value.
                        return (totalQty, totalRunTime);
                    }

                    totalQty = 0.0m;
                    totalRunTime = 0.0m;
                }

                blockNode = blockNode.next;
            }
        }

        //search complete. Return the calculated value. If the last block was part of the group the values will be set. 
        //If the loop exited due to a group mismatch, the values will be 0
        return (totalQty, totalRunTime);
    }

    /// <summary>
    /// Indicates the current state of the Activity in production. Setting this to Finished prevents it from scheduling.  Values are: Started,Finished, Waiting, Ready, SettingUp, Running, PostProcessing, or
    /// Transferring.
    /// This field also determines which portions of the Activity are scheduled.  For example, if the status is Running then no SetupSpan will be scheduled.  This value is set externally or manually, not by
    /// PlanetTogether.
    /// If the status is SettingUp, or Running then the current Resources will not be changed by an Optimization unless the job is unscheduled due to a routing change or manually.
    /// </summary>
    [Required(true)]
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public InternalActivityDefs.productionStatuses ProductionStatus
    {
        get
        {
            if (ScenarioDetail.ExtensionController.RunProductionStatusExtension)
            {
                InternalActivityDefs.productionStatuses? productionStatus = ScenarioDetail.ExtensionController.GetProductionStatus(this, m_productionStatus);
                if (productionStatus.HasValue)
                {
                    return productionStatus.Value;
                }
            }


            if (m_productionStatus < InternalActivityDefs.productionStatuses.SettingUp)
            {
                if (m_productionStatus == InternalActivityDefs.productionStatuses.Started ||
                    ReportedRunSpan.Ticks > 0 ||
                    ReportedGoodQty > 0 ||
                    ReportedSetupSpan.Ticks > 0 ||
                    ReportedPostProcessingSpan.Ticks > 0 ||
                    ReportedScrapQty > 0)
                {
                    //Return Started if any time or qty have been reported and it's status is Waiting or Ready
                    return InternalActivityDefs.productionStatuses.Started;
                }

                //not started so see if it's ready
                //Return Waiting if it's waiting for something other than the clock, else return Ready
                if (Operation.LatestConstraintInternal != InternalOperation.LatestConstraintEnum.Clock && Operation.LatestConstraintDate > PTDateTime.MinDateTime) //added check for min date time for Anspach.  Had enum of "Unknown" so clock wasn't working.  Quick fix pending fix by Larry on Enum setting.
                {
                    return InternalActivityDefs.productionStatuses.Waiting;
                }

                return InternalActivityDefs.productionStatuses.Ready;
            }

            return m_productionStatus;
        }

        internal set
        {
            // *LRH*TODO*You need to be able to handle production returning to any of the other production statuses.

            if (value > m_productionStatus)
            {
                DefaultReportedStartDate();

                if (value == InternalActivityDefs.productionStatuses.Finished)
                {
                    //Finishing so set finish date.
                    if (!ReportedFinishDateSet)
                    {
                        ReportedFinishDateTicks = Scheduled ? Batch.EndTicks : Operation.ScenarioDetail.Clock;
                    }

                    if (!ReportedStartDateSet)
                    {
                        DefaultReportedStartDate(); //don't want to allow this to be the min value if showing in Gantt.
                    }

                    StoreActualResourcesIfTracking();
                }

                m_productionStatus = value;
            }
            else if (value < m_productionStatus)
            {
                if (m_productionStatus == InternalActivityDefs.productionStatuses.Finished)
                {
                    Job.Unschedule(false);
                }
                else
                {
                    switch (m_productionStatus)
                    {
                        case InternalActivityDefs.productionStatuses.SettingUp:
                        case InternalActivityDefs.productionStatuses.Running:
                        case InternalActivityDefs.productionStatuses.PostProcessing:
                            if (InPostProduction())
                            {
                                Job.Unschedule();
                            }
                            else
                            {
                                MainResourceDefs.usageEnum minUsageEnd = Operation.ResourceRequirements.GetMinUsageEnd();

                                switch (value)
                                {
                                    case InternalActivityDefs.productionStatuses.Waiting:
                                    case InternalActivityDefs.productionStatuses.Ready:
                                    case InternalActivityDefs.productionStatuses.Started:
                                    case InternalActivityDefs.productionStatuses.SettingUp:
                                        {
                                            switch (m_productionStatus)
                                            {
                                                case InternalActivityDefs.productionStatuses.PostProcessing:
                                                case InternalActivityDefs.productionStatuses.Cleaning:
                                                    if (minUsageEnd <= MainResourceDefs.usageEnum.Run)
                                                    {
                                                        // going from PostProcessing which has no processing to one that has processing. Since at least one RR ends at run, there's currently
                                                        // no block for it. Need to unschedule to ensure it's created.
                                                        Job.Unschedule();
                                                    }

                                                    break;

                                                case InternalActivityDefs.productionStatuses.Running:
                                                    if (minUsageEnd == MainResourceDefs.usageEnum.Setup)
                                                    {
                                                        // going from Running which has no setup to one that has setup. Since at least one RR ends at setup, there's currently
                                                        // no block for it. Need to unschedule to ensure it's created.
                                                        Job.Unschedule();
                                                    }

                                                    break;
                                            }
                                        }
                                        break;

                                    case InternalActivityDefs.productionStatuses.Running:
                                        if (minUsageEnd == MainResourceDefs.usageEnum.Setup)
                                        {
                                            Job.Unschedule();
                                        }

                                        break;
                                }
                            }

                            break;
                    }
                }

                m_productionStatus = value;
            }

            if (InPostProduction())
            {
                if (Scheduled)
                {
                    Unschedule(true, true);
                }
            }

            if (value == InternalActivityDefs.productionStatuses.Finished)
            {

                Operation.ManufacturingOrder.Job.ActivityFinished();
            }

            //Reset Reported Scheduled values if necessary
            if (m_productionStatus < InternalActivityDefs.productionStatuses.Finished && ReportedFinishDateSet)
            {
                ReportedFinishDateTicks = PTDateTime.MinDateTimeTicks;
            }

            if (m_productionStatus < InternalActivityDefs.productionStatuses.Cleaning && ReportedEndOfStorageSet)
            {
                ReportedEndOfStorageTicks = PTDateTime.MinDateTimeTicks;
            }

            if (m_productionStatus < InternalActivityDefs.productionStatuses.Storing && ReportedEndOfPostProcessingSet)
            {
                ReportedEndOfPostProcessingTicks = PTDateTime.MinDateTimeTicks;
            }

            if (m_productionStatus < InternalActivityDefs.productionStatuses.PostProcessing && ReportedEndOfProcessingSet)
            {
                ReportedEndOfRunTicks = PTDateTime.MinDateTime.Ticks;
            }

            if (m_productionStatus < InternalActivityDefs.productionStatuses.Running && ReportedProcessingStartDateSet)
            {
                ReportedProcessingStartTicks = PTDateTime.MinDateTime.Ticks;
            }
        }
    }

    private void StoreActualResourcesIfTracking()
    {
        //Only update Resources used if still scheduled, otherwise once finished, the scheduled resources are lost.
        if (Scheduled)
        {
            SetActualResourcesUsed(new ResourceKeyList());
            for (int i = 0; i < Operation.ResourceRequirements.Count; i++)
            {
                ResourceBlock block = GetResourceRequirementBlock(i);
                if (block?.ScheduledResource != null)
                {
                    ActualResourcesUsed.Add(block.ScheduledResource.GetKey());
                }
            }
        }
    }

    internal override void ResetProductionStatus()
    {
        m_productionStatus = InternalActivityDefs.productionStatuses.Ready;

        m_reportedGoodQty = 0;
        m_reportedScrapQty = 0;

        m_reportedSetup = 0;
        m_reportedRun = 0;
        m_reportedPostProcessing = 0;
    }

    #region Bools

    private BoolVector32 m_bools;
    #endregion

    private decimal m_reportedGoodQty;

    /// <summary>
    /// Quantity of good product reported to have been finished.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public decimal ReportedGoodQty
    {
        get => m_reportedGoodQty;

        internal set
        {
            if (m_reportedGoodQty != value)
            {
                m_reportedGoodQty = value;
                Operation.ManufacturingOrder.ScenarioDetail.ProgressUpdated();
            }
        }
    }

    /// <summary>
    /// Run time reported to have been spent so far.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public TimeSpan ReportedRunSpan
    {
        get => new (m_reportedRun);

        internal set => ReportedRun = value.Ticks;
    }

    private long m_reportedRun;

    /// <summary>
    /// Run time reported to have been spent so far in ticks.
    /// </summary>
    internal long ReportedRun
    {
        get => m_reportedRun;

        set
        {
            if (m_reportedRun != value)
            {
                m_reportedRun = value;
                Operation.ManufacturingOrder.ScenarioDetail.ProgressUpdated();
                DefaultReportedStartDate();
            }
        }
    }

    private decimal m_reportedScrapQty;

    /// <summary>
    /// Quantity of scrapped product reported to have been finished.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public decimal ReportedScrapQty
    {
        get => m_reportedScrapQty;
        internal set => m_reportedScrapQty = value;
    }

    /// <summary>
    /// Setup time reported to have been spent so far.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public TimeSpan ReportedSetupSpan
    {
        get => new (m_reportedSetup);
        private set => ReportedSetup = value.Ticks;
    }

    private long m_reportedSetup;

    /// <summary>
    /// Setup time reported to have been spent so far.
    /// </summary>
    public long ReportedSetup
    {
        get => m_reportedSetup;
        internal set
        {
            if (m_reportedSetup != value)
            {
                m_reportedSetup = value;
                Operation.ManufacturingOrder.ScenarioDetail.ProgressUpdated();
                DefaultReportedStartDate();
            }
        }
    }

    /// <summary>
    /// PostProcessing time reported to have been spent so far.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public TimeSpan ReportedPostProcessingSpan
    {
        get => new (m_reportedPostProcessing);
        internal set => ReportedPostProcessing = value.Ticks;
    }

    private long m_reportedPostProcessing;

    /// <summary>
    /// Storage time reported to have been spent so far in ticks.
    /// </summary>
    public long ReportedPostProcessing
    {
        get => m_reportedPostProcessing;

        internal set
        {
            if (m_reportedPostProcessing != value)
            {
                m_reportedPostProcessing = value;
                Operation.ManufacturingOrder.ScenarioDetail.ProgressUpdated();
                DefaultReportedStartDate();
            }
        }
    }

    /// <summary>
    /// Storage time reported to have been spent so far.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public TimeSpan ReportedStorageSpan
    {
        get => new(m_reportedStorage);
        internal set => m_reportedStorage = value.Ticks;
    }

    private long m_reportedStorage;

    /// <summary>
    /// Storage time reported to have been spent so far in ticks.
    /// </summary>
    public long ReportedStorage
    {
        get => m_reportedStorage;

        internal set
        {
            if (m_reportedStorage != value)
            {
                m_reportedStorage = value;
                Operation.ManufacturingOrder.ScenarioDetail.ProgressUpdated();
                DefaultReportedStartDate();
            }
        }
    }

    public TimeSpan ReportedCleanSpan
    {
        get => new (m_reportedClean);
        internal set => ReportedClean = value.Ticks;
    }

    private long m_reportedClean;
    /// <summary>
    /// Clean time reported to have been spent so far in ticks.
    /// </summary>
    public long ReportedClean
    {
        get => m_reportedClean;

        internal set
        {
            if (m_reportedClean != value)
            {
                m_reportedClean = value;
                Operation.ManufacturingOrder.ScenarioDetail.ProgressUpdated();
                DefaultReportedStartDate();
            }
        }
    }

    private int m_reportedCleanOutGrade;
    public int ReportedCleanoutGrade
    {
        get => m_reportedCleanOutGrade;
        private set { m_reportedCleanOutGrade = value; }
    }

    public TimeSpan ReportedSetupProcessingAndPostProcessing => ReportedSetupSpan + ReportedPostProcessingSpan + ReportedPostProcessingSpan;

    private long m_reportedMaterialPostProcessing;

    public long ReportedMaterialPostProcessing
    {
        get => m_reportedMaterialPostProcessing;

        private set
        {
            if (m_reportedMaterialPostProcessing != value)
            {
                m_reportedMaterialPostProcessing = value;
                Operation.ManufacturingOrder.ScenarioDetail.ProgressUpdated();
                DefaultReportedStartDate();
            }
        }
    }

    /// <summary>
    /// If the ReportedStartDate hasn't been set, default it to the current scenario clock.
    /// </summary>
    private void DefaultReportedStartDate()
    {
        if (!ReportedStartDateSet)
        {
            ReportedStartDateTicks = Operation.ManufacturingOrder.ScenarioDetail.Clock;
        }
    }

    internal long RemainingMaterialPostProcessing()
    {
        if (Finished)
        {
            return 0;
        }

        long remainingMaterialPostProcessing = Operation.Products.PrimaryProduct is Product product ? product.MaterialPostProcessingTicks - m_reportedMaterialPostProcessing : 0;

        return Math.Max(0, remainingMaterialPostProcessing);
    }

    private InternalActivityDefs.peopleUsages m_peopleUsage = InternalActivityDefs.peopleUsages.UseAllAvailable;

    /// <summary>
    /// Determines how many people are allocated to an Activity in the schedule.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public InternalActivityDefs.peopleUsages PeopleUsage
    {
        get => m_peopleUsage;
        private set => m_peopleUsage = value;
    }

    private decimal m_nbrOfPeople = 1;

    /// <summary>
    /// If PeopleUsage is set to UseSpecifiedNbr then this is the maximum number of people that will be allocated to the Activity.
    /// Fewer than this number will be allocated during time periods over which the Primary Resource's Nbr Of People is less than this value.
    /// The setting is used with the Nbr Of People setting in the Resource's Capacity Interval to determine how long the operation will
    /// take and how many Operations can be run simultaneously.  The minimum of this number and the number available in the Capacity Interval are used.
    /// To allow multiple Operations to run simultaneously the Resource's Capacity Type must be set to Multi-Tasking in which case the sum of NbrOfPeople across Operations
    /// running simultaneously must be less than or equal to the Capacity Interval's Nbr Of People.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public decimal NbrOfPeople
    {
        get => m_nbrOfPeople;
        private set
        {
            if (value <= 0)
            {
                throw new PTValidationException("2050");
            }

            m_nbrOfPeople = value;
        }
    }

    /// <summary>
    /// The WorkContent of the Activity divided by the NbrOfPeople.  This gives a better idea of how long it
    /// will take to perform the Activity.
    /// Note that this value ignores the People Usage which is used to actually put NbrOfPeople to use in scheduling.
    /// </summary>
    public TimeSpan NbrOfPeopleAdjustedWorkContent
    {
        get
        {
            if (NbrOfPeople != 0)
            {
                return new TimeSpan((long)(WorkContent.Ticks / NbrOfPeople));
            }

            return new TimeSpan(0);
        }
    }
    #endregion Shared Properties

    #region Properties
    private InternalOperation m_operation;

    [Browsable(false)]
    public InternalOperation Operation => m_operation;

    /// <summary>
    /// Special summary or troubleshooting information.
    /// </summary>
    [Browsable(false)]
    public override string Analysis
    {
        get
        {
            string analysis = "";
            if (Timing != BaseActivityDefs.onTimeStatuses.OnTime)
            {
                analysis += Environment.NewLine + "\t\t" + Timing.ToString().Localize();
            }

            if (Anchored)
            {
                analysis += Environment.NewLine + "\t\tAnchored".Localize();
            }

            if (InProduction())
            {
                analysis += Environment.NewLine + "\t\tIn Process".Localize();
            }

            if (Late)
            {
                analysis += string.Format("{1}\t\tLATE {0}".Localize(), Slack.ToReadableStringHourPrecision(), Environment.NewLine);
            }

            if (Queue.TotalDays > 1)
            {
                analysis += string.Format("{1}\t\tIn queue for {0}".Localize(), Queue.ToReadableStringHourPrecision(), Environment.NewLine);
            }

            if (ReportedGoodQty < 0 || ReportedScrapQty > 0)
            {
                analysis += string.Format("{3}\t\t{0} complete, {1} scrapped, {2} remaining".Localize(),
                    Math.Round(ReportedGoodQty, 2),
                    Math.Round(ReportedScrapQty, 2),
                    Math.Round(RemainingQty, 2),
                    Environment.NewLine);
            }

            if (PrimaryResourceRequirementBlock != null && PrimaryResourceRequirementBlock.Analysis != "")
            {
                analysis += string.Format("{1}\t\t--Main Block--\t\t{0}".Localize(), PrimaryResourceRequirementBlock.Analysis, Environment.NewLine);
            }

            return analysis;
        }
    }

    /// <summary>
    /// The required quantity to start in order to arrive at the Required Finish Qty given the Operation's Planning Scrap Percent.
    /// If Operation.WholeNumberSplits is checked then this quantity is rounded up to the nearest whole number.
    /// </summary>
    public decimal RequiredStartQty
    {
        get
        {
            if (Operation.WholeNumberSplits)
            {
                return Math.Ceiling(InternalOperation.PlanningScrapPercentAdjustedQty(ScheduledOrDefaultProductionInfo.PlanningScrapPercent, RequiredFinishQty));
            }

            return InternalOperation.PlanningScrapPercentAdjustedQty(ScheduledOrDefaultProductionInfo.PlanningScrapPercent, RequiredFinishQty);
        }
    }

    /// <summary>
    /// The quantity of items expected to be scrapped given the Required Start Qty and the Operation's Planning Scrap Percent.
    /// </summary>
    public decimal ExpectedScrapQty => InternalOperation.PlanningScrapQty(ScheduledOrDefaultProductionInfo.PlanningScrapPercent, RequiredStartQty);

    /// <summary>
    /// Checks if the acitivities resource requirements have blocks and whether they're scheduled.
    /// </summary>
    public override bool Scheduled
    {
        get
        {
            if (Operation == null)
            {
                // This can happen if the activity is created before the operation. 
                return false;
            }

            if (PostProcessingStateWithNoResourceUsage)
            {
                return ScheduledOnlyForPostProcessingTime;
            }

            if (ResourceRequirementBlockCount == 0)
            {
                return false;
            }

            if (PrimaryResourceRequirementBlock == null)
            {
                return false;
            }

            return PrimaryResourceRequirementBlock.Scheduled;
        }
    }

    #region Scheduled spans. These values are set at the time the activity scheduled.
    [Obsolete("Activities actually don't have Any of their own scheduled times. All of their schedule times are In the batch they are part of scheduled. This function was temporary modified for backwards compatibility purposes.")]
    internal long GetScheduledEndOfSetupTicks()
    {
        return Batch != null ? Batch.SetupEndTicks : PTDateTime.MinDateTicks;
    }

    /// <summary>
    /// If scheduled, this is the time processing is scheduled to end.
    /// Otherise this is PTDateTime.MinDateTicks.
    /// </summary>
    internal long ScheduledProcessingEndTicks()
    {
        return Batch != null ? Batch.ProcessingEndTicks : PTDateTime.MaxDateTicks;
    }

    /// <summary>
    /// If scheduled, this is the number of ticks required for processing.
    /// Otherwise this is PTDateTime.MinDateTicks
    /// </summary>
    internal long GetRequiredProcessingTicks()
    {
        long processingTicks = PTDateTime.MinDateTicks;
        if (Batch != null)
        {
            processingTicks = Batch.ProcessingCapacitySpan.TimeSpanTicks;
        }

        return processingTicks;
    }
    
    [Obsolete("Activities actually don't have Any of their own scheduled times. All of their schedule times are In the batch they are part of scheduled. This function was temporary modified for backwards compatibility purposes.")]
    public long StandardSetupTicks => Batch != null ? Batch.SetupCapacitySpan.TimeSpanTicks : 0;

    /// <summary>
    /// The amount of time scheduled for setup.
    /// </summary>
    [Obsolete("Activities actually don't have Any of their own scheduled times. All of their schedule times are In the batch they are part of scheduled. This function was temporary modified for backwards compatibility purposes.")]
    public TimeSpan ScheduledSetupSpan => new (ScheduledSetupTicks);

    /// <summary>
    /// The amount of time scheduled for setup.
    /// </summary>
    internal long ScheduledSetupTicks => Batch != null ? Batch.SetupCapacitySpan.TimeSpanTicks : 0;

    [Obsolete("Activities actually don't have Any of their own scheduled times. All of their schedule times are In the batch they are part of scheduled. This function was temporary modified for backwards compatibility purposes.")]
    public long ScheduledPostProcessingTicks => Batch != null ? Batch.PostProcessingSpan.TimeSpanTicks : 0;

    /// <summary>
    /// The amount of time scheduled for post processing.
    /// </summary>
    [Obsolete("Activities actually don't have Any of their own scheduled times. All of their schedule times are In the batch they are part of scheduled. This function was temporary modified for backwards compatibility purposes.")]
    public TimeSpan ScheduledPostProcessingSpan => new (ScheduledPostProcessingTicks);

    /// <summary>
    /// The amount of time scheduled for post processing.
    /// </summary>
    [Obsolete("Activities actually don't have Any of their own scheduled times. All of their schedule times are In the batch they are part of scheduled. This function was temporary modified for backwards compatibility purposes.")]
    public long ScheduledCleanTicks => Batch != null ? Batch.CleanSpan.TimeSpanTicks : 0;

    /// <summary>
    /// The amount of time scheduled for post processing.
    /// </summary>
    [Obsolete("Activities actually don't have Any of their own scheduled times. All of their schedule times are In the batch they are part of scheduled. This function was temporary modified for backwards compatibility purposes.")]
    public TimeSpan ScheduledCleanSpan => new (ScheduledCleanTicks);

    /// <summary>
    /// If the activity has some run time scheduled then this is the time it is scheduled to end. If the activity is in the post-processing state or is finished then this is the time run was scheduled to
    /// finish or is the time run was reported to be finished.
    /// </summary>
    [Obsolete("Activities actually don't have Any of their own scheduled times. All of their schedule times are In the batch they are part of scheduled. This function was temporary modified for backwards compatibility purposes.")]
    internal long EndOfRunTicks
    {
        get
        {
            if (Scheduled)
            {
                return ScheduledEndOfRunTicks;
            }

            if (ProductionStatus == InternalActivityDefs.productionStatuses.PostProcessing)
            {
                return ReportedEndOfRunTicks;
            }

            if (Finished)
            {
                if (ReportedEndOfRunTicks != PTDateTime.MinDateTime.Ticks)
                {
                    return ReportedEndOfRunTicks;
                }

                if (ReportedFinishDateTicks != PTDateTime.MinDateTime.Ticks)
                {
                    long temp = ReportedFinishDateTicks - Operation.ProductionInfo.MaterialPostProcessingSpanTicks;

                    if (PTDateTime.IsValidDateTimeBetweenMinMax(temp))
                    {
                        return temp;
                    }

                    return PTDateTime.MinDateTime.Ticks;
                }

                return PTDateTime.MinDateTime.Ticks;
            }

            return PTDateTime.MinDateTime.Ticks;
        }
    }

    /// <summary>
    /// If the activity has some run time scheduled then this is the time it is scheduled to end.
    /// If it's in post-processing or finished and ReportedEndOfRunDate has been set then that value is returned.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public DateTime EndOfRunDate => new (EndOfRunTicks);

    /// <summary>
    /// The time at which the run portion of the activity was scheduled to finish.
    /// </summary>
    internal long ScheduledEndOfRunTicks => Batch?.ProcessingEndTicks ?? PTDateTime.MinDateTime.Ticks;

    /// <summary>
    /// The time at which the run portion of the activity is scheduled to finish.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public DateTime ScheduledEndOfRunDate => new (ScheduledEndOfRunTicks);

    public bool ReportedEndOfProcessingSet => m_reportedEndOfRunTicks != PTDateTime.MinDateTime.Ticks;

    private long m_reportedEndOfRunTicks = PTDateTime.MinDateTime.Ticks;

    /// <summary>
    /// When the run portion of the activity completed. This field is used in combination with the operation field Material Post-Processing to determine when material should be released. The value in this
    /// field is only used once the state of the activity is set to Post-Processing or Finished.
    /// </summary>
    internal long ReportedEndOfRunTicks
    {
        get => m_reportedEndOfRunTicks;

        set => m_reportedEndOfRunTicks = value;
    }

    /// <summary>
    /// If the activity has some run time scheduled then this is the time it's scheduled to end.
    /// When the state of the activity is changed to post-processing this value is set to the clock date unless you have report this value.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public DateTime ReportedEndOfRunDate => new (ReportedEndOfRunTicks);

    private long m_reportedEndOfPostProcessingTicks = PTDateTime.MinDateTime.Ticks;
    /// <summary>
    /// When the post-processing portion of the activity completed.
    /// </summary>
    internal long ReportedEndOfPostProcessingTicks
    {
        get => m_reportedEndOfPostProcessingTicks;

        set => m_reportedEndOfPostProcessingTicks = value;
    }

    public DateTime ReportedEndOfPostProcessingDate => new(m_reportedEndOfPostProcessingTicks);

    public bool ReportedEndOfPostProcessingSet => ReportedEndOfPostProcessingTicks > PTDateTime.MinDateTime.Ticks;


    private long m_reportedEndOfStorageTicks = PTDateTime.MinDateTime.Ticks;
    /// <summary>
    /// When the Clean portion of the activity completed. The value in this
    /// field is only used once the state of the activity is set to Storing or Finished.
    /// </summary>
    internal long ReportedEndOfStorageTicks
    {
        get => m_reportedEndOfStorageTicks;

        set => m_reportedEndOfStorageTicks = value;
    }

    public DateTime ReportedEndOfStorageDate => new(m_reportedEndOfStorageTicks);

    public bool ReportedEndOfStorageSet => ReportedEndOfStorageTicks > PTDateTime.MinDateTime.Ticks;

    /// <summary>
    /// If need this information they should use the scheduled batch.
    /// </summary>
    internal bool ScheduledProcessingZeroLength => Batch != null && Batch.ProcessingCapacitySpan.Overrun;

    private long ScheduledProcessingTicks => Batch != null ? Batch.ProcessingCapacitySpan.TimeSpanTicks : 0;

    /// <summary>
    /// The amount of time scheduled for the processing stage.
    /// </summary>
    public TimeSpan ScheduledProductionSpan => TimeSpan.FromTicks(ScheduledRunTicks);

    /// <summary>
    /// The amount of time scheduled for the processing stage.
    /// </summary>
    internal long ScheduledRunTicks => Batch == null ? 0 : Batch.ProcessingCapacitySpan.TimeSpanTicks;
    #endregion

    #region PostProcessing scheduled start and finish times.
    private long m_scheduledStartTimePostProcessingNoResources;

    /// <summary>
    /// If PostProcessingStateWithNoResourceUsage, then during scheduling this is set to the time when
    /// the activity is scheduled to start. Otherwise this value is meaningless.
    /// </summary>
    internal long ScheduledStartTimePostProcessingNoResources
    {
        get => m_scheduledStartTimePostProcessingNoResources;

        set => m_scheduledStartTimePostProcessingNoResources = value;
    }

    private long m_scheduledStartTimeCleanNoResources;

    /// <summary>
    /// If CleanStateWithNoResourceUsage, then during scheduling this is set to the time when
    /// the activity is scheduled to start. Otherwise this value is meaningless.
    /// </summary>
    internal long ScheduledStartTimeCleanNoResources
    {
        get => m_scheduledStartTimeCleanNoResources;

        set => m_scheduledStartTimeCleanNoResources = value;
    }

    private long m_scheduledFinishTimePostProcessingNoResources;

    /// <summary>
    /// If PostProcessingStateWithNoResourceUsage, then during scheduling this is set to the time when
    /// the activity is scheduled to finish. Otherwise this value is meaningless.
    /// </summary>
    internal long ScheduledFinishTimePostProcessingNoResources
    {
        get => m_scheduledFinishTimePostProcessingNoResources;

        set => m_scheduledFinishTimePostProcessingNoResources = value;
    }

    private long m_scheduledFinishTimeCleanNoResources;

    /// <summary>
    /// If CleanStateWithNoResourceUsage, then during scheduling this is set to the time when
    /// the activity is scheduled to finish. Otherwise this value is meaningless.
    /// </summary>
    internal long ScheduledFinishTimeCleanNoResources
    {
        get => m_scheduledFinishTimeCleanNoResources;

        set => m_scheduledFinishTimeCleanNoResources = value;
    }

    /// <summary>
    /// true if the activity is in the post procesing state & doesn't require resource time during post processing.
    /// </summary>
    internal bool PostProcessingStateWithNoResourceUsage => IsPostProcessing && !Operation.ResourceRequirements.PrimaryResourceRequirement.PostProcessingUsesResource;

    internal bool CleanStateWithNoResourceUsage => IsCleaning && !Operation.ResourceRequirements.PrimaryResourceRequirement.CleanUsesResource;
    #endregion

    /// <summary>
    /// The number of units that must be made to bring the Reported Good Qty up to the Required Finish Qty.
    /// </summary>
    public decimal RemainingQty
    {
        get
        {
            decimal reported = m_operation.DeductScrapFromRequired ? ReportedScrapQty + ReportedGoodQty : ReportedGoodQty;
            return Math.Max(0, RequiredFinishQty - reported);
        }
    }

    /// <summary>
    /// The number of (integer) cycles remaining to complete the RemainingQty.
    /// This only applies to Resource Operations and is calculated by divided the RemainingQty by the QtyPerCycle and rounding up to the nearest whole number.
    /// </summary>
    public int RemainingCycles
    {
        get
        {
            if (Operation is ResourceOperation)
            {
                return (int)Math.Ceiling(RemainingQty / ((ResourceOperation)Operation).QtyPerCycle);
            }

            return -1;
        }
    }

    /// <summary>
    /// The number of (decimal) cycles remaining to complete the RemainingQty.
    /// This only applies to Resource Operations and is calculated by divided the RemainingQty by the QtyPerCycle.
    /// </summary>
    public decimal RemainingPartialCycles
    {
        get
        {
            if (Operation is ResourceOperation)
            {
                return Math.Round(RemainingQty / ((ResourceOperation)Operation).QtyPerCycle, 2, MidpointRounding.AwayFromZero);
            }

            return -1;
        }
    }

    #region Production status
    /// <summary>
    /// Categorizes the different production statuses.
    /// </summary>
    internal enum ProductionStatusCategory
    {
        /// <summary>
        /// Corresponds to the production statuses of Waiting and Ready settingup, and Started.
        /// </summary>
        PreProduction,

        /// <summary>
        /// Corresponds to the production statuses of Running.
        /// </summary>
        Production,

        /// <summary>
        /// Corresponds to the production statuses of Transferring and Finished.
        /// </summary>
        PostProduction
    }

    /// <summary>
    /// The activity is not running. It may or may not be scheduled.
    /// </summary>
    internal bool InPreproduction()
    {
        return GetProductionStatusCategory(m_productionStatus) == ProductionStatusCategory.PreProduction;
    }

    /// <summary>
    /// The Activity's Production Status is Setting Up, Running, or Post-Processing (when Post-Processing consumes the Resource for the Operation).
    /// </summary>
    [Browsable(false)]
    public bool InProduction()
    {
        ProductionStatusCategory category = GetProductionStatusCategory(m_productionStatus);
        return category == ProductionStatusCategory.Production;
    }

    /// <summary>
    /// Production has completed.
    /// </summary>
    internal bool InPostProduction()
    {
        ProductionStatusCategory category = GetProductionStatusCategory(m_productionStatus);
        return category == ProductionStatusCategory.PostProduction;
    }

    internal bool InProductionOrPostProcessing
    {
        get
        {
            ProductionStatusCategory psc = GetProductionStatusCategory(m_productionStatus);
            return psc >= ProductionStatusCategory.Production;
        }
    }

    /// <summary>
    /// _productionStatus == InternalActivityDefs.productionStatuses.Finished
    /// </summary>
    internal bool Finished => m_productionStatus == InternalActivityDefs.productionStatuses.Finished;

    /// <summary>
    /// _productionStatus != InternalActivityDefs.productionStatuses.Finished
    /// </summary>
    internal bool NotFinished => m_productionStatus != InternalActivityDefs.productionStatuses.Finished;

    /// <summary>
    /// Determine the ProductionStatusCategory of a production status.
    /// </summary>
    /// <param name="ps"></param>
    /// <returns></returns>
    private ProductionStatusCategory GetProductionStatusCategory(InternalActivityDefs.productionStatuses a_ps)
    {
        switch (a_ps)
        {
            case InternalActivityDefs.productionStatuses.Waiting:
            case InternalActivityDefs.productionStatuses.Ready:
            case InternalActivityDefs.productionStatuses.Started:
                return ProductionStatusCategory.PreProduction;
            case InternalActivityDefs.productionStatuses.SettingUp:
            case InternalActivityDefs.productionStatuses.Running:
                return ProductionStatusCategory.Production;

            case InternalActivityDefs.productionStatuses.PostProcessing:
                if (Operation.ResourceRequirements.PrimaryResourceRequirement.PostProcessingUsesResource)
                {
                    return ProductionStatusCategory.Production;
                }

                return ProductionStatusCategory.PostProduction;
            case InternalActivityDefs.productionStatuses.Storing:
                if (Operation.ResourceRequirements.PrimaryResourceRequirement.StorageUsesResource)
                {
                    return ProductionStatusCategory.Production;
                }

                return ProductionStatusCategory.PostProduction;
            case InternalActivityDefs.productionStatuses.Cleaning:
                if (Operation.ResourceRequirements.PrimaryResourceRequirement.CleanUsesResource)
                {
                    return ProductionStatusCategory.Production;
                }

                return ProductionStatusCategory.PostProduction;
            case InternalActivityDefs.productionStatuses.Finished:
                return ProductionStatusCategory.PostProduction;

            default:
                throw new PTException("2469", new object[] { a_ps.ToString() });
        }
    }

    /// <summary>
    /// This indicates the ProductionStatusCategory status of this activity.
    /// </summary>
    private ProductionStatusCategory ProductionStatusInternal => GetProductionStatusCategory(ProductionStatus);
    #endregion

    /// <summary>
    /// The amount of time that the Activity is schedule to be in queue from the time it is ready to the time it is scheduled to start.
    /// </summary>
    public TimeSpan Queue => ScheduledStartDate.Subtract(Operation.LatestConstraintDate);

    /// <summary>
    /// Cached Timing value to improve performance for models with a large number of successor MOs.
    /// The value is cleared on SimulationInitialization and when the activity NeedDate or JitNeedDate is modified.
    /// This value is not serialized
    /// There is a small gap if the Job.MaxEarlyDeliverySpan or Job.AlmostLateSpan are changed through a Job Update and without a simulation or JIT recalculate occuring.
    /// This would be unlikely to occur except through an extension. It would only be a display issue until the next simulation.
    /// </summary>
    private ICalculatedValueCache<BaseActivityDefs.onTimeStatuses> m_cachedOnTimeStatus;

    /// <summary>
    /// 'Early' means the Activity finishes more than the Job.MaxEarlyDeliverySpan before the Operation NeedDate.  'Late' means the Activity ends after the Operation NeedDate.  'Bottleneck' means the
    /// Activity is Late and not due to a Predecessor making it Late.  'Capacity Bottleneck' means the Activity is a Bottleneck due to queing.  'Material Bottleneck' means the Activity is a Bottleneck due to
    /// Materials.  'Late Release Bottleneck' means the Activity is a Bottleneck due to a late MO Release Date or other manual delay.
    /// </summary>
    public override BaseActivityDefs.onTimeStatuses Timing
    {
        get
        {
            if (Operation.NotPartOfCurrentRouting()) //Check this because if true, the status cache won't be initialized
            {
                return BaseActivityDefs.onTimeStatuses.Unknown;
            }

            if (m_cachedOnTimeStatus != null && m_cachedOnTimeStatus.TryGetValue(out BaseActivityDefs.onTimeStatuses cachedStatus))
            {
                return cachedStatus;
            }

            BaseActivityDefs.onTimeStatuses status;

            if (!Late)
            {
                if (Operation.ManufacturingOrder.Job.MaxEarlyDeliverySpan.Ticks > 0 && Slack > Operation.ManufacturingOrder.Job.MaxEarlyDeliverySpan)
                {
                    status = BaseActivityDefs.onTimeStatuses.TooEarly;
                }
                else if (Operation.ManufacturingOrder.Job.AlmostLateSpan.Ticks > 0 && Slack.Ticks < Operation.ManufacturingOrder.Job.AlmostLateSpan.Ticks)
                {
                    status = BaseActivityDefs.onTimeStatuses.AlmostLate;
                }
                else
                {
                    status = BaseActivityDefs.onTimeStatuses.OnTime;
                }
            }
            else
            {
                if (Operation.HasLatePredecessors) //Then this is not a bottleneck.
                {
                    status = BaseActivityDefs.onTimeStatuses.Late;
                }
                else //Bottleneck
                {
                    if (Operation.LatestConstraintInternal == InternalOperation.LatestConstraintEnum.ManufacturingOrderRelease)
                    {
                        status = BaseActivityDefs.onTimeStatuses.ReleaseDateBottleneck;
                    }
                    else if (Operation.LatestConstraintInternal == InternalOperation.LatestConstraintEnum.MaterialRequirement)
                    {
                        status = BaseActivityDefs.onTimeStatuses.MaterialBottleneck;
                    }
                    else if (Operation.LatestConstraintInternal == InternalOperation.LatestConstraintEnum.Clock)
                    {
                        status = BaseActivityDefs.onTimeStatuses.CapacityBottleneck;
                    }
                    else
                    {
                        status = BaseActivityDefs.onTimeStatuses.CapacityBottleneck;
                    }
                }
            }

            m_cachedOnTimeStatus?.CacheValue(status);
            return status;
        }
    }

    internal ScenarioDetail ScenarioDetail => Operation.ScenarioDetail;

    /// <summary>
    /// Whether the activity starts after its JIT start date.
    /// Further restricted by if the operation is also late and the activity is part of the current routing.
    /// </summary>
    public bool Late
    {
        get
        {
            if (Operation.NotPartOfCurrentRouting() || Batch == null)
            {
                return false;
            }

            ActivityResourceBufferInfo resBufferInfo = GetBufferResourceInfo(Batch.PrimaryResource.Id);

            return Batch.EndTicks > resBufferInfo.JitStartDate;
        }
    }

    /// <summary>
    /// The date/time by which the Activity must be scheduled to start in order to avoid violating the Max Delay limit of any predecessor Operations.
    /// </summary>
    public override DateTime MaxDelayRequiredStartBy => Operation.MaxDelayRequiredStartBy;

    /// <summary>
    /// The date/time by which the Activity must be scheduled to end in order to avoid violating the Max Delay limit of any successor Operations.
    /// </summary>
    public override DateTime MaxDelayRequiredEndAfter => Operation.MaxDelayRequiredEndAfter;

    /// <summary>
    /// The amount of time that the Activity can move to the right (later) without hitting a successor.
    /// </summary>
    public override TimeSpan RightLeeway
    {
        get
        {
            if (Scheduled)
            {
                BaseOperation sucOp = Operation.GetTightestSuccessor();
                if (sucOp == null)
                {
                    return TimeSpan.MaxValue;
                }

                if (ProductionStatus != InternalActivityDefs.productionStatuses.Finished)
                {
                    return sucOp.StartDateTime.Subtract(Operation.ScheduledEndDate);
                }

                return new TimeSpan(0); //Prevent huge number from displaying
            }

            return TimeSpan.MaxValue;
        }
    }

    /// <summary>
    /// If scheduled, this is Scheduled Setup + Scheduled Run + Scheduled PostProcessing.  Remaining hours of work capacity required.  This is not necessarily the same as the Duration.
    /// If finished, this is Reported Setup + Reported Run + Reported PostProcessing. (actual time)
    /// Otherwise this is the remaining Setup + (RemainingQty / QtyPerCycle x CycleSpan) + remaining PostProcessSpan.
    /// </summary>
    public TimeSpan WorkContent
    {
        get
        {
            if (Scheduled)
            {
                long workContentTicks = ScheduledSetupTicks + ScheduledProcessingTicks + ScheduledPostProcessingTicks + Batch.StorageSpan.TimeSpanTicks;

                return TimeSpan.FromTicks(workContentTicks);
            }

            if (Finished)
            {
                return ReportedPostProcessingSpan.AddBound(ReportedRunSpan).AddBound(ReportedSetupSpan);
            }

            return RemainingSetupSpan.AddBound(RemainingRunSpan).AddBound(RemainingPostProcessingTimeSpan).AddBound(RemainingStorageSpan);
        }
    }

    #region Percent Finished
    /// <summary>
    /// This indicates the progress reported on the Activity.
    /// This value depends upon the values of the Activity's Reported Qty or Hours as well as the Operation's
    /// Time Based Reporting and Deduct Scrap From Required settings.
    /// Also, this value is set to 100 percent when the Activity's Production Status is set to Finished.
    /// </summary>
    public int PercentFinished
    {
        get
        {
            if (Finished)
            {
                return 100;
            }

            if (Operation.TimeBasedReporting)
            {
                return GetTimeBasedPercentFinished();
            }

            return GetQtyBasedPercentFinished();
        }
    }

    /// <summary>
    /// Whether the Activity's Production Status is "Started".
    /// </summary>
    public bool Started => ProductionStatus >= InternalActivityDefs.productionStatuses.Started;

    /// <summary>
    /// The Activity's percent finished based on reported values divided by scheduled values.
    /// </summary>
    private int GetTimeBasedPercentFinished()
    {
        long totalReportedTicks;
        long totalScheduledTicks;

        if (Batch != null)
        {
            totalReportedTicks = ReportedSetup + ReportedRun + ReportedPostProcessing + ReportedClean + ReportedStorage;
            totalScheduledTicks = Batch.TotalSpanTicks;
        }
        else
        {
            // Fall back to operation-based calculation when Batch is null
            totalReportedTicks = ReportedSetup + ReportedRun + ReportedPostProcessing + ReportedClean + ReportedStorage;
            totalScheduledTicks = Operation.SetupSpan.Ticks + Operation.RunSpan.Ticks + Operation.PostProcessingSpan.Ticks + Operation.CleanSpan.Ticks + Operation.StorageSpan.Ticks;
        }

        int percentDone = Convert.ToInt32(totalReportedTicks / (decimal)totalScheduledTicks * 100);
        if (percentDone < 0)
        {
            return 0;
        }

        if (percentDone > 100)
        {
            return 100;
        }

        return percentDone;
    }

    /// <summary>
    /// If the Operation's Deduct Scrap From Required setting is true then this is the Reported Scrap plus Reported Good Qty divided by Required Finish Qty.
    /// Otherwise, it is simply the Reported Good Qty divided by the Required Finish Qty.
    /// </summary>
    /// <returns></returns>
    private int GetQtyBasedPercentFinished()
    {
        if (RequiredFinishQty == 0)
        {
            return 100;
        }

        int percentDone;
        if (Operation.DeductScrapFromRequired)
        {
            percentDone = Convert.ToInt32((ReportedScrapQty + ReportedGoodQty) / RequiredFinishQty * 100m);
        }
        else
        {
            percentDone = Convert.ToInt32(ReportedGoodQty / RequiredFinishQty * 100m);
        }

        if (percentDone > 100)
        {
            return 100;
        }

        if (percentDone < 0)
        {
            return 0;
        }

        return percentDone;
    }
    #endregion

    /// <summary>
    /// Name of the resource(s) that the Activity is scheduled on.
    /// </summary>
    public string ResourcesUsed
    {
        get
        {
            string outString;
            if (Operation.ResourceRequirements.Count > 0)
            {
                StringBuilder resourcesUsed = new ();
                for (int i = 0; i < Operation.ResourceRequirements.Count; i++)
                {
                    ResourceBlock block = GetResourceRequirementBlock(i);
                    if (block != null)
                    {
                        string plantStr;
                        if (ScenarioDetail.PlantManager.Count > 1) // Show the Plant too
                        {
                            plantStr = " (" + block.ScheduledResource.PlantName + ")";
                        }
                        else
                        {
                            plantStr = "";
                        }

                        if (resourcesUsed.Length == 0)
                        {
                            resourcesUsed.Append(block.ResourceUsed + plantStr);
                        }
                        else
                        {
                            resourcesUsed.Append(", " + block.ResourceUsed + plantStr);
                        }
                    }
                }

                //If the resourcesUsed string builder has data then a resource was found, otherwise just use GetReasonForNoResourcesUsed().
                outString = resourcesUsed.Length > 0 ? resourcesUsed.ToString() : GetReasonForNoResourcesUsed();
            }
            else
            {
                outString = GetReasonForNoResourcesUsed();
            }

            return outString;
        }
    }

    private string GetReasonForNoResourcesUsed()
    {
        if (PostProcessingStateWithNoResourceUsage)
        {
            return "<PostProcessing-No Resource needed>".Localize();
        }

        if (ProductionStatus == InternalActivityDefs.productionStatuses.Finished)
        {
            return "<Finished>".Localize();
        }

        if (Operation.Omitted == BaseOperationDefs.omitStatuses.OmittedAutomatically)
        {
            return "<Op Omitted - automatically>".Localize();
        }

        if (Operation.Omitted == BaseOperationDefs.omitStatuses.OmittedByUser)
        {
            return "<Op Omitted - by User>".Localize();
        }

        if (!Scheduled)
        {
            return "<Unscheduled>".Localize();
        }

        return "<Unknown>".Localize();
    }

    internal List<BaseResource> GetResourcesUsed(out string o_reasonIfNoResourcesUsed)
    {
        List<BaseResource> resources = new ();
        for (int i = 0; i < Operation.ResourceRequirements.Count; i++)
        {
            ResourceBlock block = GetResourceRequirementBlock(i);
            if (block != null)
            {
                resources.Add(block.ScheduledResource);
            }
        }

        if (resources.Count == 0)
        {
            o_reasonIfNoResourcesUsed = GetReasonForNoResourcesUsed();
        }
        else
        {
            o_reasonIfNoResourcesUsed = "";
        }

        return resources;
    }

    /// <summary>
    /// Get a string of the other Resources used besides the Resource used by the Block.
    /// </summary>
    /// <returns></returns>
    public string GetOtherResourcesUsed(ResourceBlock a_block)
    {
        StringBuilder resourcesUsed = new ();
        for (int i = 0; i < Operation.ResourceRequirements.Count; i++)
        {
            ResourceBlock nxtBlock = GetResourceRequirementBlock(i);
            if (nxtBlock != null)
            {
                if (ReferenceEquals(nxtBlock, a_block))
                {
                    if (resourcesUsed.Length == 0)
                    {
                        resourcesUsed.Append(a_block.ResourceUsed);
                    }
                    else
                    {
                        resourcesUsed.Append(", " + a_block.ResourceUsed);
                    }
                }
            }
        }

        return resourcesUsed.ToString();
    }

    /// <summary>
    /// If scheduled, this is the scheduled start time of the Activity on the Main Resource.
    /// </summary>
    public override DateTime ScheduledStartDate => new (ScheduledStartTicks());

    /// <summary>
    /// The start of the primary resource requirement's block or PtDateTime.MIN_DATE if the activity isn't scheduled.
    /// </summary>
    /// <returns></returns>
    internal long ScheduledStartTicks()
    {
        if (Scheduled)
        {
            return GetScheduledStartTicks();
        }

        return PTDateTime.MinDateTime.Ticks;
    }

    /// <summary>
    /// The date the activitiy is scheduled to finish.
    /// Returns PtDateTime.MAX_DATE_TICKS if the activity isn't scheduled.
    /// </summary>
    internal override long ScheduledFinishDateTicks
    {
        get
        {
            if (Scheduled)
            {
                return Batch.EndTicks;
            }

            return PTDateTime.MaxDateTimeTicks;
        }
    }

    /// <summary>
    /// The amount of time that the Activity's Scheduled End Date can be delayed and still be before the Operation NeedDate.
    /// </summary>
    public override TimeSpan Slack
    {
        get
        {
            TimeSpan ts;

            if (Scheduled)
            {
                // LRH: 9/17/2009: I add the code below on 2/9/2009, but I don't recall why. It makes it difficult to understand,
                // so I commented it out. Anspach was asking why the slack appeared to be off. It was because the setup time was
                // being factored out.
                //if (Scheduled && ScheduledEndOfSetupTicks != 0)
                //{
                //    long slack = JITStartTicks - ScheduledEndOfSetupTicks;
                //    ts = new TimeSpan(slack);
                //}
                //else
                {
                    ts = NeedDate.Subtract(new DateTime(Batch.PostProcessingEndTicks));
                }
            }
            else
            {
                ts = new TimeSpan(0);
            }

            return ts;
        }
    }

    internal TimeSpan RemainingRunSpan
    {
        get
        {
            if (Operation is ResourceOperation)
            {
                if (Operation.Scheduled)
                {
                    return ScheduledProductionSpan;
                }

                decimal qtyPerCycle = ((ResourceOperation)Operation).QtyPerCycle;
                if (Operation.TimeBasedReporting)
                {
                    decimal remainingRunTicks = RequiredFinishQty / qtyPerCycle * m_operation.CycleSpan.Ticks;
                    if (remainingRunTicks > TimeSpan.MaxValue.Ticks)
                    {
                        return TimeSpan.MaxValue;
                    }

                    TimeSpan totalActivityTime = new ((long)remainingRunTicks);
                    return new TimeSpan(Math.Max(totalActivityTime.Subtract(ReportedRunSpan).Ticks, 0));
                }
                else
                {
                    //Use total required cycles
                    decimal remainingRunTicks = Math.Ceiling(RemainingQty / qtyPerCycle) * m_operation.CycleSpan.Ticks;
                    if (remainingRunTicks > TimeSpan.MaxValue.Ticks)
                    {
                        return TimeSpan.MaxValue;
                    }

                    return new TimeSpan((long)remainingRunTicks);
                }
            }

            return new TimeSpan(0);
        }
    }

    internal TimeSpan RemainingPostProcessingTimeSpan => new (CalculatePostProcessingSpan(null).TimeSpanTicks);

    internal TimeSpan RemainingSetupSpan => new (RemainingSetupTicks);

    internal long RemainingSetupTicks
    {
        get
        {
            if (ReportedSetup >= ScheduledOrDefaultProductionInfo.SetupSpanTicks)
            {
                return 0;
            }

            return ScheduledOrDefaultProductionInfo.SetupSpanTicks - ReportedSetup;
        }
    }

    internal TimeSpan RemainingStorageSpan => new(CalculateStorageSpan(null).TimeSpanTicks);

    /// <summary>
    /// This is the RequiredFinishQty adjusted to take into consideration the most constraining predecessor operation. This may be a fraction of
    /// RequiredFinishQty. The value of this field = (Operation.ExpectedFinishQty)*(activity.RequiredFinishQty/Operation.RequiredFinishQty).
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public override decimal ExpectedFinishQty
    {
        get
        {
            decimal expectedFinishQty = Operation.ExpectedFinishQty * (RequiredFinishQty / Operation.RequiredFinishQty);

            if (Operation.WholeNumberSplits)
            {
                expectedFinishQty = Math.Floor(expectedFinishQty);
            }

            return expectedFinishQty;
        }
    }

    internal long m_cycleFinishTime;

    /// <summary>
    /// When the final cycle of this activity completed.
    /// This value is set when the activity's state is changed from the running state to the post-processing or post-processing-transferrring state.
    /// </summary>
    internal long CycleFinishTime
    {
        get => m_cycleFinishTime;

        private set => m_cycleFinishTime = value;
    }

    internal long MaterialFinishTime(SchedulableInfo a_si, long a_simClock)
    {
        if (Scheduled)
        {
            if (PostProcessingStateWithNoResourceUsage)
            {
                return ScheduledFinishTimePostProcessingNoResources;
            }

            long readyDate = PrimaryResourceRequirementBlock.EndTicks;

            if (!Operation.ResourceRequirements.PrimaryResourceRequirement.PostProcessingUsesResource)
            {
                // We add post processing since it it not included in the scheduled finish time of the operation's blocks.
                readyDate += CalculatePostProcessingSpan(PrimaryResourceRequirementBlock.ScheduledResource).TimeSpanTicks;
            }

            return readyDate;
        }

        return PTDateTime.MaxDateTimeTicks;
    }

    private string m_comments = "";

    /// <summary>
    /// Text that can be entered by operators or loaded from bar code systems.
    /// </summary>
    public string Comments
    {
        get => m_comments;
        internal set => m_comments = value;
    }

    private string m_comments2 = "";

    /// <summary>
    /// Text that can be entered by operators or loaded from bar code systems.
    /// </summary>
    public string Comments2
    {
        get => m_comments2;
        internal set => m_comments2 = value;
    }

    /// <summary>
    /// To allow Add-Ins to push information back.
    /// </summary>
    /// <param name="value"></param>
    public void SetComments(string aComments)
    {
        Comments = aComments;
    }

    /// <summary>
    /// To allow Add-Ins to push information back.
    /// </summary>
    /// <param name="value"></param>
    public void SetComments2(string aComments)
    {
        Comments2 = aComments;
    }
    #endregion

    #region Performance History
    /// <summary>
    /// Indicates how well the primary resource performed an Activity.
    /// </summary>
    public class PerformanceHistory
    {
        public PerformanceHistory(InternalActivity a_activity)
        {
            m_jobName = a_activity.Operation.ManufacturingOrder.Job.Name;
            m_moName = a_activity.Operation.ManufacturingOrder.Name;
            m_opName = a_activity.Operation.Name;
            m_opDescription = a_activity.Operation.Description;
            m_activityId = a_activity.Id.ToString();

            m_setupPerformance = CalcPerformance(a_activity.ScheduledSetupSpan, a_activity.ReportedSetupSpan);
            m_runPerformance = CalcPerformance(a_activity.ScheduledProductionSpan, a_activity.ReportedRunSpan);
            m_postProcessingPerformance = CalcPerformance(a_activity.ScheduledPostProcessingSpan, a_activity.ReportedPostProcessingSpan);
            m_scrapPerformance = CalcPerformance(a_activity.ExpectedScrapQty, a_activity.ReportedScrapQty);
        }

        private readonly string m_jobName;

        public string JobName => m_jobName;

        private readonly string m_moName;

        public string MoName => m_moName;

        private readonly string m_opName;

        public string OperationName => m_opName;

        private readonly string m_opDescription;

        public string OperationDescription => m_opDescription;

        private readonly string m_activityId;

        public string ActivityId => m_activityId;

        private readonly decimal m_setupPerformance;

        public decimal SetupPerformance => m_setupPerformance;

        private readonly decimal m_runPerformance;

        public decimal RunPerformance => m_runPerformance;

        private readonly decimal m_postProcessingPerformance;

        public decimal PostProcessingPerformance => m_postProcessingPerformance;

        private readonly decimal m_scrapPerformance;

        public decimal ScrapPerformance => m_scrapPerformance;

        /// <summary>
        /// Returns a percentage indicator of performance.  If the actual is equal
        /// to the expected than the value returned is 1.  If the actual is longer
        /// then the returned value is greater than 1, such as 1.5 meaning 150% of expected.
        /// </summary>
        private decimal CalcPerformance(TimeSpan a_expected, TimeSpan a_actual)
        {
            if (a_expected.Ticks == 0)
            {
                return 0;
            }

            return 1 + (a_actual.Ticks - a_expected.Ticks) / (decimal)a_expected.Ticks;
        }

        /// <summary>
        /// Returns a percentage indicator of performance.  If the actual is equal
        /// to the expected than the value returned is 1.  If the actual is more
        /// then the returned value is greater than 1, such as 1.5 meaning 150% of expected.
        /// </summary>
        private decimal CalcPerformance(decimal a_expected, decimal a_actual)
        {
            if (a_expected == 0)
            {
                return 0;
            }

            return 1 + (a_actual - a_expected) / a_expected;
        }
    }
    #endregion

    #region Overrides
    /// <summary>
    /// The latest the Activity can start on this resource and still finish by the Operation NeedDate.  Calculated by subtracting the Activity WorkContent from the Operation NeedDate when the Job is created.
    /// </summary>
    public DateTime DbrJitStartDate => new (DbrJitStartTicks);
    public DateTime JitStartDate => new (JitStartTicks);

    /// <summary>
    /// The amount of time that the Activity can move to the left (earlier) without hitting a constraint.
    /// </summary>
    public override TimeSpan LeftLeeway
    {
        get
        {
            if (Scheduled)
            {
                return ScheduledStartDate.Subtract(Operation.LatestConstraintDate);
            }

            return new TimeSpan(0); //Prevent huge number from displaying
        }
    }

    /// <summary>
    /// Returns the TransferSpan of the primary resource the activity is or was scheduled on.
    /// </summary>
    public override TimeSpan ResourceTransferSpan => new (ResourceTransferSpanTicks);

    /// <summary>
    /// When the activity is finished, if the activitiy is scheduled, the transfer time of the primary resource requirement is stored in this field.
    /// </summary>
    private long m_resourceTransferTimeTicksAtActivityFinishTime;

    /// <summary>
    /// If the activity is scheduled, this is the TransferTime of the primary resource. If the activity is finished, this is the transfer time of the primary resource
    /// at the time the activity was finished. If the activity isn't scheduled, this is 0.
    /// </summary>
    internal long ResourceTransferSpanTicks
    {
        get
        {
            if (Scheduled)
            {
                return PrimaryResourceRequirementBlock.ScheduledResource.TransferSpanTicks;
            }

            if (Finished)
            {
                return m_resourceTransferTimeTicksAtActivityFinishTime;
            }

            return 0;
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Returns the type name of the next offline or cleanout interval if it starts after this activity and there are no other activities between them.
    /// </summary>
    /// <returns></returns>
    public string NextResourceOffline()
    {
        ResourceBlockList.Node currentBlockNode = Batch.GetBlockNodeAtIndex(Operation.ResourceRequirements.PrimaryResourceRequirementIndex);
        if (currentBlockNode.next?.Data != null && currentBlockNode.next.Data.StartTicks - currentBlockNode.Data.EndTicks < 2)
        {
            //The following block starts very close to the end of this activity. There can't be an offline interval between them.
            return null;
        }

        //The next block does not start immediately after this activity. Search for the next offline interval after this
        Resource resource = currentBlockNode.Data.ScheduledResource;
        ResourceCapacityInterval capacityInterval = resource.ResourceCapacityIntervals.FindFirstStartingAtOrAfter(currentBlockNode.Data.EndDateTime, false);
        if (capacityInterval != null)
        {
            if (currentBlockNode.Next?.Data == null || capacityInterval.StartDateTime <= currentBlockNode.Next.Data.StartDateTime)
            {
                // Do we still want to check all the bool to determine what string to put here?
                return string.Format("{0} at {1}".Localize(), capacityInterval.IntervalType.ToString().Localize(), capacityInterval.StartDateTime.ToDisplayTime());
            }

            return null;
        }

        return null;
    }

    public decimal GetResourceCost()
    {
        decimal resourceCost = 0;
        for (int i = 0; i < ResourceRequirementBlockCount; i++)
        {
            ResourceBlock block = GetResourceRequirementBlock(i);
            if (block != null)
            {
                resourceCost += block.GetResourceCost();
            }
        }

        return resourceCost;
    }

    /// <summary>
    /// custm of carrying cost for each of the blocks.
    /// </summary>
    /// <returns></returns>
    public decimal GetResourceCarryingCost()
    {
        decimal resourceCost = 0;
        for (int i = 0; i < ResourceRequirementBlockCount; i++)
        {
            ResourceBlock block = GetResourceRequirementBlock(i);
            if (block != null)
            {
                resourceCost += block.GetCarryingCost();
            }
        }

        return resourceCost;
    }

    //TODO: V12 move this to a new IGanttFlagsGenerator. Don't use this element by default. Move the standard flags setting to a settings class and use that.
    /// <summary>
    /// Returns a list of generated warning flags for the Activity.
    /// </summary>
    /// <returns></returns>
    public FlagList StandardFlagsList
    {
        get
        {
            FlagList flagList = new ();
            if (Operation.Scheduled)
            {
                //Connector Violation
                if (ConnectionViolated)
                {
                    string msg = string.Format("Resource Connectors Violation.".Localize());
                    Flag connFlag = new (msg);
                    connFlag.ColorCode = Color.Red;
                    flagList.Add(connFlag);
                }

                //MaxDelay
                if (MaxDelayViolation)
                {
                    string msg = string.Format("Max Delay violation.  Must start by {0}.".Localize(), MaxDelayRequiredStartBy);
                    Flag maxDelayFlag = new (msg);
                    maxDelayFlag.ColorCode = Color.Red;
                    flagList.Add(maxDelayFlag);
                }

                //Shelf Life
                for (int matI = 0; matI < Operation.MaterialRequirements.Count; matI++)
                {
                    MaterialRequirement mr = Operation.MaterialRequirements[matI];
                    if (!mr.BuyDirect && mr.Item.ShelfLife > TimeSpan.Zero)
                    {
                        foreach (InternalActivity supplyingActivity in mr.SupplyingActivities)
                        {
                            long materialDwellTimeTicks = ScheduledStartDate.Ticks - supplyingActivity.ScheduledEndDate.Ticks;
                            if (materialDwellTimeTicks > mr.Item.ShelfLife.Ticks) //violating shelf life
                            {
                                DateTimeOffset mustStartBy = supplyingActivity.Batch.PostProcessingEndDateTime.Add(mr.Item.ShelfLife).ToDisplayTime();
                                DateTimeOffset mustEndBy = ScheduledStartDate.Subtract(mr.Item.ShelfLife).ToDisplayTime();
                                DateTimeOffset currentSupplyEnd = supplyingActivity.ScheduledEndDate.ToDisplayTime();
                                string msg = string.Format("Shelf Life violation for Material {0} supplied by Job {1} from {2} on {3}.  Start this Activity by {4} or delay supply to end {5}.".Localize(),
                                    mr.MaterialName,
                                    Operation.Job.Name,
                                    Batch.PrimaryResourceBlock.ScheduledResource.Name,
                                    currentSupplyEnd,
                                    mustStartBy,
                                    mustEndBy);

                                Flag shelfLifeFlag = new (msg);
                                shelfLifeFlag.ColorCode = Color.OrangeRed;
                                flagList.Add(shelfLifeFlag);
                            }
                        }
                    }
                }
            }

            return flagList;
        }
    }

    /// <summary>
    /// The amount of a material requirement that is for this activity.
    /// </summary>
    /// <param name="mr"></param>
    /// <returns></returns>
    public decimal FractionOfMR(MaterialRequirement a_mr)
    {
        return a_mr.TotalRequiredQty * FractionOfStartQty;
    }

    /// <summary>
    /// Get a constrained release time for the Successor Operation based on the Predecessor's End and any potential
    /// overlap
    /// </summary>
    /// <param name="a_predAct"></param>
    /// <param name="a_successorAct"></param>
    /// <param name="a_overlapType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public long GetOverlapConstrainedReleaseDateForSuccessor(InternalActivity a_successorAct, InternalOperationDefs.overlapTypes a_overlapType)
    {
        long constrainedStartTime = Batch?.EndTicks ?? ReportedFinishDate.Ticks;
        Resource predActResource = Batch?.PrimaryResource ?? (Resource)Operation.ResourceRequirements.PrimaryResourceRequirement.GetFirstEligibleResource();
        switch (a_overlapType)
        {
            case InternalOperationDefs.overlapTypes.NoOverlap:
                constrainedStartTime = Operation.CalcEndOfResourceTransferTimeTicks();
                break;
            case InternalOperationDefs.overlapTypes.TransferSpan:
                TransferInfo constrainingTransfer = a_successorAct.Operation.TransferInfo.GetConstrainingTransfer();
                constrainedStartTime = constrainingTransfer.GetTransferEndForActivity(Id);
                break;
            case InternalOperationDefs.overlapTypes.TransferQty:
            case InternalOperationDefs.overlapTypes.AtFirstTransfer:
            case InternalOperationDefs.overlapTypes.TransferSpanAfterSetup:
            case InternalOperationDefs.overlapTypes.PercentComplete:
            case InternalOperationDefs.overlapTypes.TransferSpanBeforeStartOfPredecessor:
                long transferQtyReleaseTicks = Operation.GetOverlapReleaseTicksByResource(predActResource);
                if (transferQtyReleaseTicks != long.MaxValue)
                {
                    constrainedStartTime = transferQtyReleaseTicks;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return constrainedStartTime;
    }
    #endregion

    #region Object Accesors
    public ResourceBlock PrimaryResourceRequirementBlock => Batch?.BlockAtIndex(0);

    public ResourceBlock GetResourceRequirementBlock(int a_index)
    {
        //This relies on the fact that block id is created from the block index (which is also the Resource Requirement index)
        return Batch?.BlockAtIndex(a_index);
    }

    public ResourceBlock GetResourceRequirementBlock(BaseId a_blockId)
    {
        return GetResourceRequirementBlock((int)a_blockId.Value);
    }

    public bool ResourceRequirementBlockExists(BaseId a_blockId)
    {
        return ResourceRequirementBlockCount > (int)a_blockId.ToBaseType();
    }

    public ResourceBlock GetResourceBlock(BaseId a_blockId)
    {
        //This relies on the fact that block id is created from the block index (which is also the Resource Requirement index)
        return GetResourceRequirementBlock((int)a_blockId.Value);
    }

    public ResourceBlock GetResourceBlock(int a_rrIdx)
    {
        //This relies on the fact that block id is created from the block index (which is also the Resource Requirement index)
        return GetResourceRequirementBlock(a_rrIdx);
    }

    [Browsable(false)]
    public int ResourceRequirementBlockCount
    {
        get
        {
            try
            {
                return Operation.ResourceRequirements.Count;
            }
            catch (Exception e)
            {
                int xxx = 0;
                throw;
            }
        }
    }

    /// <summary>
    /// If the activity is scheduled, it returns the plant the activity's batch is scheduled in. Otherwise it returns null.
    /// </summary>
    /// <returns>Plant or null.</returns>
    public Plant GetScheduledPlant()
    {
        Plant p = null;
        if (Batch != null)
        {
            p = Batch.GetScheduledPlant();
        }

        return p;
    }
    #endregion

    #region Transmission Functionality
    public void Receive(ActivityIdBaseT a_t, IScenarioDataChanges a_dataChanges)
    {
        if (a_t is InternalActivityFinishT)
        {
            InternalActivityFinishT finishT = (InternalActivityFinishT)a_t;
            if (finishT.FinishPredecessors)
            {
                if (Operation.HasPredecessors())
                {
                    Operation.AutoFinishPredecessors(string.Format("Auto-finished when Operation {0} was finished because the user specified to finish predecessors.".Localize(), Operation.Name));
                }
            }

            Finish(finishT, a_dataChanges);
        }
    }

    public bool Receive(InternalActivityUpdateT.ActivityStatusUpdate a_update, IScenarioDataChanges a_dataChanges)
    {
        return Finish(a_update, a_dataChanges);
    }

    internal void AutoReportProgress(ScenarioOptions a_scenarioOptions, TimeSpan a_clockAdvancedBy)
    {
        long newClock = Operation.ManufacturingOrder.Job.ScenarioDetail.Clock + a_clockAdvancedBy.Ticks;
        StoreActualResourcesIfTracking();

        if (Scheduled && Batch != null && newClock > ScheduledStartDate.Ticks) //advancing past start of operation so it will be affected
        {
            if (!ReportedStartDateSet)
            {
                ReportedStartDateTicks = ScheduledStartTicks();
            }


            long scheduledEndOfSetupTicks = Batch.SetupEndTicks;

            //bool suppliedFromTank = false;
            //bool? isTankConsumingActivity = ScenarioDetail.ExtensionController.MarkTankConsumingActivity(this, newClock);
            //if (isTankConsumingActivity.HasValue)
            //{
            //    suppliedFromTank = isTankConsumingActivity.Value;
            //}
            //else
            //{
            //    //Default Behaviour
            //    foreach (MaterialRequirement mr in Operation.MaterialRequirements)
            //    {
            //        //if (mr.SupplyingFromTank(new DateTime(newClock), Id))
            //        //{
            //        //    return;
            //        //}
            //    }
            //}

            //Setup
            if (Batch.SetupCapacitySpan.TimeSpanTicks > 0)
            {
                long setupToReport = 0;
                if (newClock < scheduledEndOfSetupTicks) //still setting up
                {
                    foreach (OperationCapacity capacity in Batch.PrimaryResourceBlock.CapacityProfile.SetupProfile)
                    {
                        if (capacity.EndTicks <= newClock)
                        {
                            setupToReport += GetReportedCapacity(capacity, newClock, true, true);
                        }
                        else if (capacity.StartTicks < newClock)
                        {
                            setupToReport += GetReportedCapacity(capacity, newClock, true, false);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else if (!Batch.SetupCapacitySpan.Overrun)//past end of setup
                {
                    setupToReport = Batch.SetupCapacitySpan.TimeSpanTicks;
                }

                ReportedSetup += setupToReport;
                ActivityManualUpdateOnly = true; // So automatically reported quantities are preserved.
            }

            //Run
            if (Batch.ProcessingCapacitySpan.TimeSpanTicks > 0 && newClock > scheduledEndOfSetupTicks)
            {
                bool pastRun = false;
                int cyclesToReport = 0;
                long reportedCapacity = 0;
                if (newClock < ScheduledEndOfRunTicks) //still running
                {
                    foreach (OperationCapacity capacity in Batch.PrimaryResourceBlock.CapacityProfile.ProductionProfile)
                    {
                        if (capacity.EndTicks <= newClock)
                        {
                            reportedCapacity += GetReportedCapacity(capacity, newClock, Operation.TimeBasedReporting, true);
                        }
                        else if (capacity.StartTicks < newClock)
                        {
                            reportedCapacity += GetReportedCapacity(capacity, newClock, Operation.TimeBasedReporting, false);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else //past end of run
                {
                    pastRun = true;
                    if (!Batch.ProcessingCapacitySpan.Overrun)
                    {
                        reportedCapacity = Batch.ProcessingCapacitySpan.TimeSpanTicks;
                    }
                }

                //For non time-based reporting we need to align the capacity to report with cycles to report
                if (!Operation.TimeBasedReporting)
                {
                    //Align run to report with the number of cycles that are actually running.
                    if (a_scenarioOptions.IsApproximatelyEqual((double)reportedCapacity, (double)ScheduledOrDefaultProductionInfo.CycleSpanTicks))
                    {
                        cyclesToReport = 1;
                    }
                    else
                    {
                        cyclesToReport = (int)(reportedCapacity / ScheduledOrDefaultProductionInfo.CycleSpanTicks);
                    }

                    reportedCapacity = cyclesToReport * ScheduledOrDefaultProductionInfo.CycleSpanTicks;
                }

                ReportedRun += reportedCapacity;
                ActivityManualUpdateOnly = true;
                //Calculate how many cycles report this quantity
                if (RequiredFinishQty <= ScheduledOrDefaultProductionInfo.QtyPerCycle)
                {
                    if (pastRun)
                    {
                        cyclesToReport = 1;
                    }
                }

                decimal qtyToReportGood = 0;

                if (!Operation.TimeBasedReporting)
                {
                    if (cyclesToReport > 0)
                    {
                        qtyToReportGood = Math.Min(cyclesToReport * ScheduledOrDefaultProductionInfo.QtyPerCycle, RequiredFinishQty);
                        ReportedGoodQty += qtyToReportGood;

                        ProduceToInventory(qtyToReportGood, newClock >= Batch.PostProcessingEndTicks, true, newClock);
                    }
                }
                else
                {
                    if (reportedCapacity > 0)
                    {
                        if (pastRun)
                        {
                            qtyToReportGood = RemainingQty;
                        }
                        else
                        {
                            //qty to report should be RequiredFinishQty times the percentage of total run we are reporting
                            long totalRunSpanTicks = Batch.ProcessingCapacitySpan.TimeSpanTicks;
                            qtyToReportGood = RequiredFinishQty - ScenarioDetail.ScenarioOptions.RoundQty(RequiredFinishQty * ((totalRunSpanTicks - (decimal)reportedCapacity) / totalRunSpanTicks));
                        }

                        ReportedGoodQty += qtyToReportGood;

                        ProduceToInventory(qtyToReportGood, newClock >= Batch.ProcessingEndTicks, true, newClock);
                    }
                }

                if (ScheduledOrDefaultProductionInfo.QtyPerCycle > Operation.RequiredStartQty)
                {
                    //Since there is only 1 cycle, the AutoIssueMaterials should be based on required qty, not produced qty.
                    Operation.AutoIssueMaterials(Id, Operation.RequiredStartQty, newClock >= Batch.PostProcessingEndTicks);
                }
                else
                {
                    Operation.AutoIssueMaterials(Id, qtyToReportGood, newClock >= Batch.PostProcessingEndTicks);
                }
            }

            //Post Processing
            if (Batch.PostProcessingSpan.TimeSpanTicks > 0 && newClock > Batch.ProcessingEndTicks)
            {
                long postProcessToReport = 0;
                if (newClock < Batch.PostProcessingEndDateTime.Ticks) //still PostProcessing
                {
                    foreach (OperationCapacity capacity in Batch.PrimaryResourceBlock.CapacityProfile.PostprocessingProfile)
                    {
                        if (capacity.EndTicks <= newClock)
                        {
                            postProcessToReport += GetReportedCapacity(capacity, newClock, true, true);
                        }
                        else if (capacity.StartTicks < newClock)
                        {
                            postProcessToReport += GetReportedCapacity(capacity, newClock, true, false);
                        }
                        else
                        {
                            break;
                        }
                    }

                    postProcessToReport = Math.Max(0, postProcessToReport);
                }
                else if (!Batch.PostProcessingSpan.Overrun)//past end of post processing
                {
                    postProcessToReport = Batch.PostProcessingSpan.TimeSpanTicks;
                }

                ReportedPostProcessing += postProcessToReport;
                ActivityManualUpdateOnly = true;
            }

            //Storage
            if (Batch.StorageSpan.TimeSpanTicks > 0 && newClock >= Batch.PostProcessingEndTicks)
            {
                long storageToReport = 0;
                if (newClock < Batch.EndOfStorageTicks) //still cleaning
                {
                    foreach (OperationCapacity capacity in Batch.PrimaryResourceBlock.CapacityProfile.StorageProfile)
                    {
                        if (capacity.EndTicks <= newClock)
                        {
                            storageToReport += GetReportedCapacity(capacity, newClock, true, true);
                        }
                        else if (capacity.StartTicks < newClock)
                        {
                            storageToReport += GetReportedCapacity(capacity, newClock, true, false);
                        }
                        else
                        {
                            break;
                        }
                    }

                    storageToReport = Math.Max(0, storageToReport);
                }
                else //past end of Storage
                {
                    if (!Batch.StorageSpan.Overrun)
                    {
                        storageToReport = Batch.StorageSpan.TimeSpanTicks;
                    }

                    ProduceToInventory(RequiredFinishQty, newClock >= Batch.PostProcessingEndTicks, true, newClock);
                }

                ReportedStorage += storageToReport;
                ActivityManualUpdateOnly = true;
            }

            //Clean
            if (Batch.CleanSpan.TimeSpanTicks > 0 && newClock >= Batch.EndOfStorageTicks)
            {
                long cleanToReport = 0;
                if (newClock < Batch.CleanEndTicks) //still cleaning
                {
                    foreach (OperationCapacity capacity in Batch.PrimaryResourceBlock.CapacityProfile.CleanProfile)
                    {
                        if (capacity.EndTicks <= newClock)
                        {
                            cleanToReport += GetReportedCapacity(capacity, newClock, true, true);
                        }
                        else if (capacity.StartTicks < newClock)
                        {
                            cleanToReport += GetReportedCapacity(capacity, newClock, true, false);
                        }
                        else
                        {
                            break;
                        }
                    }

                    cleanToReport = Math.Max(0, cleanToReport);
                }
                else if (!Batch.CleanSpan.Overrun) //past end of Clean
                {
                    cleanToReport = Batch.CleanSpan.TimeSpanTicks;
                }

                ReportedClean += cleanToReport;
                ReportedCleanoutGrade = Batch.CleanSpan.CleanoutGrade;
                ActivityManualUpdateOnly = true;
            }

            //Set production status based on where the Clock was Advanced and Report the Scheduled values if necessary
            if (newClock >= Batch.EndTicks)
            {
                ReportedEndOfStorageTicks = ReportedEndOfStorageSet ? ReportedEndOfStorageTicks : Batch.CleanEndTicks;
                ReportedEndOfPostProcessingTicks = ReportedEndOfPostProcessingSet ? ReportedEndOfPostProcessingTicks : Batch.PostProcessingEndTicks;
                ReportedEndOfRunTicks = ReportedEndOfProcessingSet ? ReportedEndOfRunTicks : ScheduledEndOfRunTicks;
                ReportedProcessingStartTicks = ReportedProcessingStartDateSet ? ReportedProcessingStartTicks : Batch.SetupEndTicks;
            }
            else if (newClock >= Batch.EndOfStorageTicks)
            {
                ProductionStatus = InternalActivityDefs.productionStatuses.Cleaning;
                ReportedEndOfStorageTicks = ReportedEndOfStorageSet ? ReportedEndOfStorageTicks : Batch.CleanEndTicks;
                ReportedEndOfPostProcessingTicks = ReportedEndOfPostProcessingSet ? ReportedEndOfPostProcessingTicks : Batch.PostProcessingEndTicks;
                ReportedEndOfRunTicks = ReportedEndOfProcessingSet ? ReportedEndOfRunTicks : ScheduledEndOfRunTicks;
                ReportedProcessingStartTicks = ReportedProcessingStartDateSet ? ReportedProcessingStartTicks : Batch.SetupEndTicks;
            }
            else if (newClock >= Batch.PostProcessingEndTicks)
            {
                ProductionStatus = InternalActivityDefs.productionStatuses.Storing;
                ReportedEndOfPostProcessingTicks = ReportedEndOfPostProcessingSet ? ReportedEndOfPostProcessingTicks : Batch.PostProcessingEndTicks;
                ReportedEndOfRunTicks = ReportedEndOfProcessingSet ? ReportedEndOfRunTicks : ScheduledEndOfRunTicks;
                ReportedProcessingStartTicks = Batch.SetupEndTicks;
            }
            else if (newClock >= Batch.ProcessingEndTicks)
            {
                ProductionStatus = InternalActivityDefs.productionStatuses.PostProcessing;
                ReportedEndOfRunTicks = ReportedEndOfProcessingSet ? ReportedEndOfRunTicks : ScheduledEndOfRunTicks;
                ReportedProcessingStartTicks = ReportedProcessingStartDateSet ? ReportedProcessingStartTicks : Batch.SetupEndTicks;
            }
            else if (newClock >= Batch.SetupEndTicks)
            {
                ProductionStatus = InternalActivityDefs.productionStatuses.Running;
                ReportedProcessingStartTicks = ReportedProcessingStartDateSet ? ReportedProcessingStartTicks : Batch.SetupEndTicks;
            }
            else if (Batch.SetupCapacitySpan.TimeSpanTicks > 0 && newClock < Batch.SetupEndTicks)
            {
                ProductionStatus = InternalActivityDefs.productionStatuses.SettingUp;
            }
        }
    }

    /// <summary>
    /// Get reported capacity
    /// </summary>
    /// <param name="a_capacity"></param>
    /// <param name="a_newClock">Clock Advance date</param>
    /// <param name="a_timeBasedReporting"></param>
    /// <param name="a_useFullCapacity">Whether to use the full Operation Capacity or the duration between Capacity Start and new Clock Date</param>
    /// <returns></returns>
    private static long GetReportedCapacity(OperationCapacity a_capacity, long a_newClock, bool a_timeBasedReporting, bool a_useFullCapacity)
    {
        //Use calendar time if Time-based reporting
        if (a_timeBasedReporting)
        {
            if (a_useFullCapacity)
            {
                return a_capacity.EndTicks - a_capacity.StartTicks;
            }

            return a_newClock - a_capacity.StartTicks;
        }

        //If not time-based reporting account for capacity used accounting for efficiency
        if (a_useFullCapacity)
        {
            return (long)(a_capacity.TotalCapacityTicks * a_capacity.CapacityRatio);
        }

        return (long)((a_newClock - a_capacity.StartTicks) * a_capacity.CapacityRatio);
    }
    internal void AutoFinish(string a_commentsToLog)
    {
        ProductionStatus = InternalActivityDefs.productionStatuses.Finished;
        if (!string.IsNullOrEmpty(Comments))
        {
            Comments += "  " + a_commentsToLog;
        }
        else
        {
            Comments += a_commentsToLog;
        }

        if (Scheduled && RequiredFinishQty > ReportedGoodQty + ReportedScrapQty) //Add the difference to the Qty Good.
        {
            decimal qtyBeingAutoFinished = RequiredFinishQty - ReportedGoodQty - ReportedScrapQty;
            ProduceToInventory(qtyBeingAutoFinished, true, true, PTDateTime.InvalidDateTime.Ticks);
            Operation.AutoIssueMaterials(Id, qtyBeingAutoFinished, true);
        }

        ActivityManualUpdateOnly = true;
    }
    #endregion

    #region PT Database
    internal void PopulateJobDataSet(ref JobDataSet r_dataSet)
    {
        //Set sort to scheduled start and the Id (in case of it being unscheduled)
        r_dataSet.Activity.DefaultView.Sort = string.Format("{0} ASC,{1} ASC", r_dataSet.Activity.ScheduledStartColumn.ColumnName, r_dataSet.Activity.IdColumn.ColumnName);

        long jitStart;
        long dbrJitStart;

        if (Scheduled)
        {
            ActivityResourceBufferInfo activityResourceBufferInfo = GetBufferResourceInfo(Batch.PrimaryResource.Id);
            jitStart = activityResourceBufferInfo.JitStartDate;
            dbrJitStart = activityResourceBufferInfo.DbrJitStartDate;
        }
        else
        {
            //This returns the earliest buffer info values
            jitStart = JitStartDate.Ticks;
            dbrJitStart = DbrJitStartDate.Ticks;
        }

        r_dataSet.Activity.AddActivityRow(
           Operation.Job.ExternalId,
           Operation.ManufacturingOrder.ExternalId,
           Operation.ExternalId,
           Id.ToBaseType(),
           ExternalId,
           ProductionStatus.ToString(),
           Paused,
           Scheduled,
           ScheduledStartDate.ToDisplayTime().ToDateTime(),
           ScheduledEndDate.ToDisplayTime().ToDateTime(),
           (int)Timing,
           Slack,
           ReportedGoodQty,
           ReportedScrapQty,
           ReportedSetupSpan.TotalHours,
           ReportedRunSpan.TotalHours,
           ReportedPostProcessingSpan.TotalHours,
           ReportedCleanSpan.TotalHours,
           ReportedStorageSpan.TotalHours,
           ReportedCleanoutGrade,
           ReportedEndOfPostProcessingDate.ToDisplayTime().ToDateTime(),
           ReportedEndOfStorageDate.ToDisplayTime().ToDateTime(),
           WorkContent,
           Queue,
           Batch?.ZeroLength ?? false,
           Anchored,
           AnchorStartDate.ToDisplayTime().ToDateTime(),
           AnchorDrift,
           new DateTime(dbrJitStart).ToDisplayTime().ToDateTime(),
           new DateTime(jitStart).ToDisplayTime().ToDateTime(),
           Late,
           (int)Locked,
           RemainingQty,
           RequiredFinishQty,
           ExpectedFinishQty,
           RequiredStartQty,
           ResourcesUsed,
           ScheduledSetupSpan,
           //I believe Scheduled Run span was not marked as deprecated because it's value is retrieved from the batch
           ScheduledProductionSpan,
           ScheduledPostProcessingSpan,
           ResourceTransferSpan,
           InProduction(),
           ActivityManualUpdateOnly,
           PeopleUsage.ToString(),
           NbrOfPeople,
           Comments,
           Comments2,
           ReportedStartDate.ToDisplayTime().ToDateTime(),
           ReportedFinishDate.ToDisplayTime().ToDateTime(),
           TimeSpan.FromTicks(ActivityProductionInfo.CycleSpanTicks).TotalHours,
           ActivityProductionInfo.QtyPerCycle,
           TimeSpan.FromTicks(ActivityProductionInfo.SetupSpanTicks).TotalHours,
           TimeSpan.FromTicks(ActivityProductionInfo.PostProcessingSpanTicks).TotalHours,
           TimeSpan.FromTicks(ActivityProductionInfo.CleanSpanTicks).TotalHours,
           ActivityProductionInfo.CleanoutGrade,
           TimeSpan.FromTicks(ActivityProductionInfo.StorageSpanTicks).TotalHours,
           ActivityProductionInfo.PlanningScrapPercent,
           ActivityProductionInfo.OnlyAllowManualUpdatesToCycleSpan,
           ActivityProductionInfo.OnlyAllowManualUpdatesToQtyPerCycle,
           ActivityProductionInfo.OnlyAllowManualUpdatesToSetupSpan,
           ActivityProductionInfo.OnlyAllowManualUpdatesToPostProcessingSpan,
           ActivityProductionInfo.OnlyAllowManualUpdatesToCleanSpan,
           ActivityProductionInfo.OnlyAllowManualUpdatesToPlanningScrapPercent,
           ActivityProductionInfo.OnlyAllowManualUpdatesToStorageSpan,
           ActivityProductionInfo.CycleSpanOverride,
           ActivityProductionInfo.QtyPerCycleOverride,
           ActivityProductionInfo.SetupSpanOverride,
           ActivityProductionInfo.PostProcessingSpanOverride,
           ActivityProductionInfo.CleanSpanOverride,
           ActivityProductionInfo.PlanningScrapPercentOverride,
           ActivityProductionInfo.StorageSpanOverride,
           Batch != null && Batch.SetupCapacitySpan.Overrun,
           Batch != null && Batch.ProcessingCapacitySpan.Overrun,
           Batch != null && Batch.PostProcessingSpan.Overrun,
           ReportedProcessingStartDateTime,
           ReportedEndOfRunDate,
           BatchAmount,
           ActualResourcesUsed != null ? ActualResourcesUsed.ToResourceIdList() : "",
           TimeSpan.FromTicks(ActivityProductionInfo.CycleSpanTicks).TotalHours,
           Batch != null ? TimeSpan.FromTicks(Batch.SetupCapacitySpan.TimeSpanTicks).TotalHours : 0,
           Batch != null ? TimeSpan.FromTicks(Batch.PostProcessingSpan.TimeSpanTicks).TotalHours : 0,
           Batch != null ? TimeSpan.FromTicks(Batch.StorageSpan.TimeSpanTicks).TotalHours : 0,
           Batch != null ? TimeSpan.FromTicks(Batch.CleanSpan.TimeSpanTicks).TotalHours : 0,
           CleanoutCost,
           ProductionSetupCost,
           TotalSetupCost
       );

        //Add Resource Usages (blocks)
        for (int i = 0; i < ResourceRequirementBlockCount; i++)
        {
            ResourceBlock block = GetResourceRequirementBlock(i);
            if (block != null)
            {
                block.PopulateJobDataSet(ref r_dataSet, this);
            }
        }
    }
    #endregion

    #region Progress
    /// <summary>
    /// This is for internal finishing and status update, not for finishing via ERP Transmission.
    /// </summary>
    internal void Finish(InternalActivityFinishT a_t, IScenarioDataChanges a_dataChanges)
    {
        bool reportedFinishUpdated = a_t.ReportedFinishDateTicks != 0 && a_t.ReportedFinishDateTicks != ReportedFinishDateTicks;

        bool updated = UpdateHelper(false,
            a_t.ProductionStatus,
            a_t.ReportedStartDateTicks,
            a_t.ReportedProcessingStartDateTicks,
            a_t.ReportedProcessingEndDateTicks,
            a_t.ReportedFinishDateTicks,
            reportedFinishUpdated,
            a_t.ReportedSetupSpan,
            a_t.ReportedRunSpan,
            a_t.ReportedPostProcessingSpan,
            a_t.ReportedCleanSpan,
            a_t.ReportedStorageSpan,
            a_t.ReportedEndOfPostProcessingTicks,
            a_t.ReportedEndOfStorageTicks,
            a_t.ReportedCleanoutGrade,
            a_t.ReportedGoodQty,
            a_t.ReportedScrapQty,
            a_t.ReportedQtiesAreIncremental,
            a_t.ReportedSpansAreIncremental,
            a_t.Paused,
            a_t.PeopleUsage,
            a_t.NbrOfPeople,
            a_t.Comments,
            a_t.Comments2,
            a_t.BatchAmount,
            a_t.MaterialIssues,
            a_t.AllocateMaterialFromOnHand,
            a_t.ReleaseProductToWarehouse,
            a_t.NowFinishUtcTime,
            a_t.ActivityManualUpdateOnly,
            a_dataChanges);
        ActivityManualUpdateOnly |= updated;
    }

    /// <summary>
    /// This is a transmission received from a bar coding or other external system.
    /// </summary>
    /// <param name="a_update"></param>
    /// <param name="a_dataChanges"></param>
    internal bool Finish(InternalActivityUpdateT.ActivityStatusUpdate a_update, IScenarioDataChanges a_dataChanges)
    {
        //Use the current values for any fields not set in the update
        TimeSpan setupSpanToUse, runSpanToUse, postProcessingSpanToUse, cleanSpanToUse, storageSpanToUse;
        decimal goodQtyToUse, scrapQtyToUse;
        int cleanoutGradeToUse;

        if (a_update.ReportedSetupSpanSet)
        {
            if (a_update.ReportedSpansAreIncremental)
            {
                setupSpanToUse = ReportedSetupSpan.Add(a_update.ReportedSetupSpan);
            }
            else
            {
                setupSpanToUse = a_update.ReportedSetupSpan;
            }
        }
        else
        {
            setupSpanToUse = ReportedSetupSpan;
        }

        if (a_update.ReportedRunSpanSet)
        {
            runSpanToUse = a_update.ReportedSpansAreIncremental ? ReportedRunSpan.Add(a_update.ReportedRunSpan) : a_update.ReportedRunSpan;
        }
        else
        {
            runSpanToUse = ReportedRunSpan;
        }

        if (a_update.ReportedStorageSpanSet)
        {
            storageSpanToUse = a_update.ReportedSpansAreIncremental ? ReportedStorageSpan.Add(a_update.ReportedStorageSpan) : a_update.ReportedStorageSpan;
        }
        else
        {
            storageSpanToUse = ReportedStorageSpan;
        }

        if (a_update.ReportedPostProcessingSpanSet)
        {
            postProcessingSpanToUse = a_update.ReportedSpansAreIncremental ? ReportedPostProcessingSpan.Add(a_update.ReportedPostProcessingSpan) : a_update.ReportedPostProcessingSpan;
        }
        else
        {
            postProcessingSpanToUse = ReportedPostProcessingSpan;
        }

        if (a_update.ReportedCleanGradeIsSet)
        {
            cleanSpanToUse = a_update.ReportedSpansAreIncremental ? ReportedCleanSpan.Add(a_update.ReportedCleanSpan) : a_update.ReportedCleanSpan;
        }
        else
        {
            cleanSpanToUse = ReportedCleanSpan;
        }

        if (a_update.ReportedCleanGradeIsSet)
        {
            cleanoutGradeToUse = a_update.ReportedCleanoutGrade;
        }
        else
        {
            cleanoutGradeToUse = ReportedCleanoutGrade;
        }

        if (a_update.ReportedGoodQtySet)
        {
            if (a_update.ReportedQtiesAreIncremental)
            {
                goodQtyToUse = ReportedGoodQty + a_update.ReportedGoodQty;
            }
            else
            {
                goodQtyToUse = a_update.ReportedGoodQty;
            }
        }
        else
        {
            goodQtyToUse = ReportedGoodQty;
        }

        if (a_update.ReportedScrapQtySet)
        {
            if (a_update.ReportedQtiesAreIncremental)
            {
                scrapQtyToUse = ReportedScrapQty + a_update.ReportedScrapQty;
            }
            else
            {
                scrapQtyToUse = a_update.ReportedScrapQty;
            }
        }
        else
        {
            scrapQtyToUse = ReportedScrapQty;
        }

        bool manualUpdateOnly = a_update.ActivityManualUpdateOnlySet ? a_update.ActivityManualUpdateOnly : ActivityManualUpdateOnly;
        bool pausedToUse = a_update.PausedSet ? a_update.Paused : Paused;

        InternalActivityDefs.peopleUsages peopleUsageToUse = a_update.PeopleUsageSet ? a_update.PeopleUsage : PeopleUsage;

        decimal nbrOfPeopleToUse = a_update.NbrOfPeopleSet ? a_update.NbrOfPeople : NbrOfPeople;
        decimal batchAmountToUse = a_update.BatchAmountSet ? a_update.BatchAmount : BatchAmount;
        string commentsToUse = a_update.CommentsSet ? a_update.Comments : Comments;
        string comments2ToUse = a_update.Comments2Set ? a_update.Comments2 : Comments2;
        bool reportedFinishUpdated = a_update.ReportedFinishDateTicks != 0 && a_update.ReportedFinishDateTicks != ReportedFinishDateTicks;
        bool updated = UpdateHelper(false,
            a_update.ProductionStatus,
            a_update.ReportedStartDateTicks,
            a_update.ReportedProcessingStartDateTicks,
            a_update.ReportedProcessingEndDateTicks,
            a_update.ReportedFinishDateTicks,
            reportedFinishUpdated,
            setupSpanToUse,
            runSpanToUse,
            postProcessingSpanToUse,
            cleanSpanToUse,
            storageSpanToUse,
            a_update.ReportedEndOfPostProcessingTicks,
            a_update.ReportedEndOfStorageTicks,
            cleanoutGradeToUse,
            goodQtyToUse,
            scrapQtyToUse,
            false,
            false,
            pausedToUse,
            peopleUsageToUse,
            nbrOfPeopleToUse,
            commentsToUse,
            comments2ToUse,
            batchAmountToUse,
            a_update.MaterialIssues,
            a_update.AllocateMaterialFromOnHand,
            a_update.ReleaseProductToWarehouse,
            a_update.NowFinishUtcTime,
            manualUpdateOnly,
            a_dataChanges);
        return updated;
    }

    /// <summary>
    /// Call this function in the event you need to finish the activity as the result of something other than the result of a transmission that
    /// directly causes the activity to be updated.
    /// For instance this function was initially created for the purpose of automatically finishing in process activities when the required finish
    /// quantity of an activity drops below what was required.
    /// </summary>
    internal void Finish()
    {
        if (ProductionStatus != InternalActivityDefs.productionStatuses.Finished)
        {
            ProductionStatus = InternalActivityDefs.productionStatuses.Finished;
            ActivityManualUpdateOnly = true; //flag not to update status from ERP.
            Operation.ActivityFinished();
            Operation.ManufacturingOrder.ScenarioDetail.ActivitiesFinished();
        }
    }

    internal bool Update(bool a_erpUpdate,
                         InternalActivity a_updatedActivity,
                         ScenarioOptions a_options,
                         bool a_preserveRequiredQty,
                         JobT.BaseOperation a_jobTOp,
                         bool a_allocateMaterialFormOnHand,
                         bool a_releaseProductToWarehouse,
                         DateTime a_nowUtcForReportedEnd,
                         IScenarioDataChanges a_dataChanges)
    {
        JobT.InternalActivity originalActivity = null;
        if (a_jobTOp is JobT.InternalOperation jobTOp)
        {
            if (jobTOp.InternalActivityCount > 0)
            {
                originalActivity = jobTOp.GetInternalActivity(0);
            }
        }

        BaseActivityUpdateResults baseChanges = base.Update(ScenarioDetail, a_updatedActivity, a_preserveRequiredQty, a_erpUpdate, ActivityManualUpdateOnly, a_dataChanges, Operation.Job.Id, out bool updated);

        //Check if the activity already exists. If it does not, the following step is skipped.
        if (originalActivity != null && originalActivity.ReportedStartOfProcessingDateSet)
        {
            ReportedProcessingStartDateTime = originalActivity.ReportedStartOfProcessingDate;
            updated = true;
        }

        if (!a_erpUpdate)
        {
            updated |= UpdateInternally(baseChanges, a_updatedActivity, a_allocateMaterialFormOnHand, a_releaseProductToWarehouse, a_nowUtcForReportedEnd, a_dataChanges);
            return updated;
        }

        if (originalActivity == null)
        {
            a_dataChanges.FlagEligibilityChanges(Job.Id);
            return UpdateNewActivity(baseChanges, a_updatedActivity, a_nowUtcForReportedEnd, a_dataChanges);
        }

        return UpdateExternally(baseChanges, a_updatedActivity, a_options, a_preserveRequiredQty, originalActivity, a_allocateMaterialFormOnHand, a_releaseProductToWarehouse, a_nowUtcForReportedEnd, a_dataChanges);
    }

    /// <summary>
    /// This is for updating the Activity from the UI, where all values in the updatedActivity are used
    /// </summary>
    internal bool UpdateInternally(BaseActivityUpdateResults a_baseChanges, InternalActivity a_updatedActivity, bool a_allocateMaterialFormOnHand, bool a_releaseProductToWarehouse, DateTime a_nowUtcForReportedEnd, IScenarioDataChanges a_dataChanges)
    {
        bool updated = false;
        bool isScheduled = Scheduled;
        bool flagConstraintChanges = false;
        bool flagProductionChanges = false;
        BaseId jobId = Operation.Job.Id;
        //Only update production info if we are overriding the operation values
        //Store manual update only flags

        //Cycle Span
        m_productionInfoOverride.OnlyAllowManualUpdatesToCycleSpan = a_updatedActivity.m_productionInfoOverride.OnlyAllowManualUpdatesToCycleSpan;
        if (a_updatedActivity.m_productionInfoOverride.CycleSpanOverride)
        {
            if (m_productionInfoOverride.CycleSpanTicks != a_updatedActivity.m_productionInfoOverride.CycleSpanTicks)
            {
                m_productionInfoOverride.CycleSpanTicks = a_updatedActivity.m_productionInfoOverride.CycleSpanTicks;
                updated = true;
                flagProductionChanges = true;
            }

        }
        else if (m_productionInfoOverride.CycleSpanTicks != m_operation.ProductionInfo.CycleSpanTicks)
        {
            m_productionInfoOverride.CycleSpanTicks = m_operation.ProductionInfo.CycleSpanTicks;
            updated = true;
            flagProductionChanges = true;
        }

        m_productionInfoOverride.CycleSpanOverride = a_updatedActivity.m_productionInfoOverride.CycleSpanOverride;

        //QtyPerCycle
        m_productionInfoOverride.OnlyAllowManualUpdatesToQtyPerCycle = a_updatedActivity.m_productionInfoOverride.OnlyAllowManualUpdatesToQtyPerCycle;
        if (a_updatedActivity.m_productionInfoOverride.QtyPerCycleOverride)
        {
            if (m_productionInfoOverride.QtyPerCycle != a_updatedActivity.m_productionInfoOverride.QtyPerCycle)
            {
                m_productionInfoOverride.QtyPerCycle = a_updatedActivity.m_productionInfoOverride.QtyPerCycle;
                updated = true;
                flagProductionChanges = true;
                a_dataChanges.FlagEligibilityChanges(jobId);
            }
        }
        else if (m_productionInfoOverride.QtyPerCycle != m_operation.ProductionInfo.QtyPerCycle)
        {
            m_productionInfoOverride.QtyPerCycle = m_operation.ProductionInfo.QtyPerCycle;
            updated = true;
            flagProductionChanges = true;
            a_dataChanges.FlagEligibilityChanges(jobId);
        }

        m_productionInfoOverride.QtyPerCycleOverride = a_updatedActivity.m_productionInfoOverride.QtyPerCycleOverride;

        //SetupSpan
        m_productionInfoOverride.OnlyAllowManualUpdatesToSetupSpan = a_updatedActivity.m_productionInfoOverride.OnlyAllowManualUpdatesToSetupSpan;
        if (a_updatedActivity.m_productionInfoOverride.SetupSpanOverride)
        {
            if (m_productionInfoOverride.SetupSpanTicks != a_updatedActivity.m_productionInfoOverride.SetupSpanTicks)
            {
                m_productionInfoOverride.SetupSpanTicks = a_updatedActivity.m_productionInfoOverride.SetupSpanTicks;
                m_productionInfoOverride.ProductionSetupCost = a_updatedActivity.m_productionInfoOverride.ProductionSetupCost;
                updated = true;
                flagProductionChanges = true;
            }
        }
        else if (m_productionInfoOverride.SetupSpanTicks != m_operation.ProductionInfo.SetupSpanTicks)
        {
            m_productionInfoOverride.SetupSpanTicks = m_operation.ProductionInfo.SetupSpanTicks;
            m_productionInfoOverride.ProductionSetupCost = m_operation.ProductionInfo.ProductionSetupCost;
            updated = true;
            flagProductionChanges = true;
        }

        m_productionInfoOverride.SetupSpanOverride = a_updatedActivity.m_productionInfoOverride.SetupSpanOverride;

        //PostProcessingSpan
        m_productionInfoOverride.OnlyAllowManualUpdatesToPostProcessingSpan = a_updatedActivity.m_productionInfoOverride.OnlyAllowManualUpdatesToPostProcessingSpan;
        if (a_updatedActivity.m_productionInfoOverride.PostProcessingSpanOverride)
        {
            if (m_productionInfoOverride.PostProcessingSpanTicks != a_updatedActivity.m_productionInfoOverride.PostProcessingSpanTicks)
            {
                m_productionInfoOverride.PostProcessingSpanTicks = a_updatedActivity.m_productionInfoOverride.PostProcessingSpanTicks;
                updated = true;
                flagProductionChanges = true;
            }
        }
        else if (m_productionInfoOverride.PostProcessingSpanTicks != m_operation.ProductionInfo.PostProcessingSpanTicks)
        {
            m_productionInfoOverride.PostProcessingSpanTicks = m_operation.ProductionInfo.PostProcessingSpanTicks;
            updated = true;
            flagProductionChanges = true;
        }

        m_productionInfoOverride.PostProcessingSpanOverride = a_updatedActivity.m_productionInfoOverride.PostProcessingSpanOverride;

        //CleanSpan
        m_productionInfoOverride.OnlyAllowManualUpdatesToCleanSpan = a_updatedActivity.m_productionInfoOverride.OnlyAllowManualUpdatesToCleanSpan;
        if (a_updatedActivity.m_productionInfoOverride.CleanSpanOverride)
        {
            if (m_productionInfoOverride.CleanSpanTicks != a_updatedActivity.m_productionInfoOverride.CleanSpanTicks)
            {
                m_productionInfoOverride.CleanSpanTicks = a_updatedActivity.m_productionInfoOverride.CleanSpanTicks;
                m_productionInfoOverride.CleanoutCost = a_updatedActivity.m_productionInfoOverride.CleanoutCost;
                updated = true;
                flagProductionChanges = true;
            }
        }
        else if (m_productionInfoOverride.CleanSpanTicks != m_operation.ProductionInfo.CleanSpanTicks)
        {
            m_productionInfoOverride.CleanSpanTicks = m_operation.ProductionInfo.CleanSpanTicks;
            m_productionInfoOverride.CleanoutCost = m_operation.ProductionInfo.CleanoutCost;
            updated = true;
            flagProductionChanges = true;
        }

        m_productionInfoOverride.CleanSpanOverride = a_updatedActivity.m_productionInfoOverride.CleanSpanOverride;

        if (m_productionInfoOverride.CleanoutGrade != a_updatedActivity.m_productionInfoOverride.CleanoutGrade)
        {
            m_productionInfoOverride.CleanoutGrade = a_updatedActivity.m_productionInfoOverride.CleanoutGrade;
            updated = true;
            flagProductionChanges = true;
        }

        //PlanningScrapPercent
        m_productionInfoOverride.OnlyAllowManualUpdatesToPlanningScrapPercent = a_updatedActivity.m_productionInfoOverride.OnlyAllowManualUpdatesToPlanningScrapPercent;
        if (a_updatedActivity.m_productionInfoOverride.PlanningScrapPercentOverride)
        {
            if (m_productionInfoOverride.PlanningScrapPercent != a_updatedActivity.m_productionInfoOverride.PlanningScrapPercent)
            {
                m_productionInfoOverride.PlanningScrapPercent = a_updatedActivity.m_productionInfoOverride.PlanningScrapPercent;
                updated = true;
                flagProductionChanges = true;
            }
        }
        else if (m_productionInfoOverride.PlanningScrapPercent != m_operation.ProductionInfo.PlanningScrapPercent)
        {
            m_productionInfoOverride.PlanningScrapPercent = m_operation.ProductionInfo.PlanningScrapPercent;
            updated = true;
            flagProductionChanges = true;
        }

        m_productionInfoOverride.PlanningScrapPercentOverride = a_updatedActivity.m_productionInfoOverride.PlanningScrapPercentOverride;

        //TransferQty
        m_productionInfoOverride.OnlyAllowManualUpdatesToTransferQty = a_updatedActivity.m_productionInfoOverride.OnlyAllowManualUpdatesToTransferQty;
        if (a_updatedActivity.m_productionInfoOverride.TransferQtyOverride)
        {
            if (m_productionInfoOverride.TransferQty != a_updatedActivity.m_productionInfoOverride.TransferQty)
            {
                m_productionInfoOverride.TransferQty = a_updatedActivity.m_productionInfoOverride.TransferQty;
                updated = true;
                flagConstraintChanges = true;
            }
        }
        else if (m_productionInfoOverride.TransferQty != m_operation.ProductionInfo.TransferQty)
        {
            m_productionInfoOverride.TransferQty = m_operation.ProductionInfo.TransferQty;
            updated = true;
            flagConstraintChanges = true;
        }

        m_productionInfoOverride.TransferQtyOverride = a_updatedActivity.m_productionInfoOverride.TransferQtyOverride;

        //Storage Span
        m_productionInfoOverride.OnlyAllowManualUpdatesToStorageSpan = a_updatedActivity.m_productionInfoOverride.OnlyAllowManualUpdatesToStorageSpan;
        if (a_updatedActivity.m_productionInfoOverride.StorageSpanOverride)
        {
            if (m_productionInfoOverride.StorageSpanTicks != a_updatedActivity.m_productionInfoOverride.StorageSpanTicks)
            {
                m_productionInfoOverride.StorageSpanTicks = a_updatedActivity.m_productionInfoOverride.StorageSpanTicks;
                updated = true;
                flagProductionChanges = true;
            }
        }
        else if (m_productionInfoOverride.StorageSpanTicks != m_operation.ProductionInfo.StorageSpanTicks)
        {
            m_productionInfoOverride.StorageSpanTicks = m_operation.ProductionInfo.StorageSpanTicks;
            updated = true;
            flagProductionChanges = true;
        }

        m_productionInfoOverride.StorageSpanOverride = a_updatedActivity.m_productionInfoOverride.StorageSpanOverride;

        if (a_updatedActivity.ActivityManualUpdateOnly != ActivityManualUpdateOnly)
        {
            ActivityManualUpdateOnly = a_updatedActivity.ActivityManualUpdateOnly;
        }

        updated |= UpdateHelper(false,
            a_updatedActivity.m_productionStatus,
            a_updatedActivity.ReportedStartDate.Ticks,
            a_updatedActivity.ReportedProcessingStartTicks,
            a_updatedActivity.ReportedEndOfRunTicks,
            a_updatedActivity.ReportedFinishDateTicks,
            a_baseChanges.ReportedFinishDateChanged,
            a_updatedActivity.ReportedSetupSpan,
            a_updatedActivity.ReportedRunSpan,
            a_updatedActivity.ReportedPostProcessingSpan,
            a_updatedActivity.ReportedCleanSpan,
            a_updatedActivity.ReportedStorageSpan,
            a_updatedActivity.ReportedEndOfPostProcessingTicks,
            a_updatedActivity.ReportedEndOfStorageTicks,
            a_updatedActivity.ReportedCleanoutGrade,
            a_updatedActivity.m_reportedGoodQty,
            a_updatedActivity.ReportedScrapQty,
            false,
            false,
            a_updatedActivity.Paused,
            a_updatedActivity.PeopleUsage,
            a_updatedActivity.NbrOfPeople,
            a_updatedActivity.Comments,
            a_updatedActivity.Comments2,
            a_updatedActivity.BatchAmount,
            new List<MaterialIssue>(),
            a_allocateMaterialFormOnHand,
            a_releaseProductToWarehouse,
            a_nowUtcForReportedEnd,
            a_updatedActivity.ActivityManualUpdateOnly,
            a_dataChanges
        );

        if (isScheduled)
        {
            if (flagConstraintChanges)
            {
                a_dataChanges.FlagConstraintChanges(Job.Id);
            }

            if (flagProductionChanges)
            {
                a_dataChanges.FlagProductionChanges(Job.Id);
            }
        }

        updated = ActivityManualUpdateOnly | updated | a_baseChanges.AnyChanges;
        return updated; //return if the activity has been updated.
    }

    /// <summary>
    /// An update to a new activity where all of the new activity properties are used
    /// </summary>
    private bool UpdateNewActivity(BaseActivityUpdateResults a_baseChanges, InternalActivity a_updatedActivity, DateTime a_nowUtcForReportedEnd, IScenarioDataChanges a_dataChanges)
    {
        bool updated = false;
        long newFinishDateTicks = a_updatedActivity.ReportedFinishDateTicks;
        long newStartDateTicks = a_updatedActivity.ReportedStartDate.Ticks;
        long newProcessingStartDateTicks = a_updatedActivity.ReportedProcessingStartTicks;
        long newProcessingEndDateTicks = a_updatedActivity.ReportedEndOfRunTicks;
        TimeSpan newSetupSpan = a_updatedActivity.ReportedSetupSpan;
        TimeSpan newRunSpan = a_updatedActivity.ReportedRunSpan;
        TimeSpan newPostProcessingSpan = a_updatedActivity.ReportedPostProcessingSpan;
        TimeSpan newStorageSpan = a_updatedActivity.ReportedStorageSpan;
        TimeSpan newCleanSpan = a_updatedActivity.ReportedCleanSpan;
        int newReportedCleanOutGrade = a_updatedActivity.m_reportedCleanOutGrade;
        decimal newGoodQty = a_updatedActivity.m_reportedGoodQty;
        decimal newScrapQty = a_updatedActivity.ReportedScrapQty;
        bool newPaused = a_updatedActivity.Paused;
        InternalActivityDefs.peopleUsages newPeopleUsage = a_updatedActivity.PeopleUsage;
        decimal newNbrOfPeople = a_updatedActivity.NbrOfPeople;
        string newComments = a_updatedActivity.Comments;
        string newComments2 = a_updatedActivity.Comments2;
        InternalActivityDefs.productionStatuses newProductionStatus = a_updatedActivity.m_productionStatus;
        decimal newBatchAmount = a_updatedActivity.BatchAmount;
        BaseId jobId = Operation.Job.Id;

        //Only update production info if we are overriding the operation values
        m_productionInfoOverride.CycleSpanTicks = a_updatedActivity.m_productionInfoOverride.CycleSpanTicks;
        m_productionInfoOverride.QtyPerCycle = a_updatedActivity.m_productionInfoOverride.QtyPerCycle;
        m_productionInfoOverride.SetupSpanTicks = a_updatedActivity.m_productionInfoOverride.SetupSpanTicks;
        m_productionInfoOverride.PostProcessingSpanTicks = a_updatedActivity.m_productionInfoOverride.PostProcessingSpanTicks;
        m_productionInfoOverride.CleanSpanTicks = a_updatedActivity.m_productionInfoOverride.CleanSpanTicks;
        m_productionInfoOverride.CleanoutGrade = a_updatedActivity.m_productionInfoOverride.CleanoutGrade;
        m_productionInfoOverride.PlanningScrapPercent = a_updatedActivity.m_productionInfoOverride.PlanningScrapPercent;
        m_productionInfoOverride.TransferQty = a_updatedActivity.m_productionInfoOverride.TransferQty;
        m_productionInfoOverride.StorageSpanTicks = a_updatedActivity.m_productionInfoOverride.StorageSpanTicks;

        updated = true;
        a_dataChanges.FlagProductionChanges(jobId);

        updated |= UpdateHelper(true,
            newProductionStatus,
            newStartDateTicks,
            newProcessingStartDateTicks,
            newProcessingEndDateTicks,
            newFinishDateTicks,
            a_baseChanges.ReportedFinishDateChanged,
            newSetupSpan,
            newRunSpan,
            newPostProcessingSpan,
            newCleanSpan,
            newStorageSpan,
            a_updatedActivity.ReportedEndOfPostProcessingTicks,
            a_updatedActivity.ReportedEndOfStorageTicks,
            newReportedCleanOutGrade,
            newGoodQty,
            newScrapQty,
            false,
            false,
            newPaused,
            newPeopleUsage,
            newNbrOfPeople,
            newComments,
            newComments2,
            newBatchAmount,
            new List<MaterialIssue>(),
            false,
            false,
            a_nowUtcForReportedEnd,
            a_updatedActivity.ActivityManualUpdateOnly,
            a_dataChanges);
        updated = updated | a_baseChanges.AnyChanges;

        return updated; //return if the activity has been updated.
    }

    /// <summary>
    /// This is for updating the Activity from an ERP transmission, not from the UI. Only the values set on the transmission are used
    /// Options are checked for whether ERP changes can be made to specific fields
    /// </summary>
    internal bool UpdateExternally(BaseActivityUpdateResults a_baseChanges,
                                   InternalActivity a_updatedActivity,
                                   ScenarioOptions a_options,
                                   bool a_preserveRequiredQty,
                                   JobT.InternalActivity a_activityTransmission,
                                   bool a_allocateMaterialFormOnHand,
                                   bool a_releaseProductToWarehouse,
                                   DateTime a_nowUtcForReportedEnd,
                                   IScenarioDataChanges a_dataChanges)
    {
        bool updated = false;
        bool isScheduled = Scheduled;
        bool flagConstraintChanges = false;
        bool flagProductionChanges = false;
        long newFinishDateTicks = a_activityTransmission.ReportedFinishDateSet ? a_updatedActivity.ReportedFinishDateTicks : ReportedFinishDateTicks;
        long newStartDateTicks = a_activityTransmission.ReportedStartDateIsSet ? a_updatedActivity.ReportedStartDate.Ticks : ReportedStartDateTicks;
        long newProcessingStartDateTicks = a_activityTransmission.ReportedStartOfProcessingDateSet ? a_updatedActivity.ReportedProcessingStartTicks : ReportedProcessingStartTicks;
        long newProcessingEndDateTicks = a_activityTransmission.ReportedEndOfRunDateSet ? a_updatedActivity.ReportedEndOfRunTicks : ReportedEndOfRunTicks;
        TimeSpan newSetupSpan = a_activityTransmission.ReportedSetupSpanIsSet ? a_updatedActivity.ReportedSetupSpan : ReportedSetupSpan;
        TimeSpan newRunSpan = a_activityTransmission.ReportedRunSpanIsSet ? a_updatedActivity.ReportedRunSpan : ReportedRunSpan;
        TimeSpan newPostProcessingSpan = a_activityTransmission.ReportedPostProcessingSpanIsSet ? a_updatedActivity.ReportedPostProcessingSpan : ReportedPostProcessingSpan;
        TimeSpan newCleanSpan = a_activityTransmission.ReportedCleanIsSet ? a_updatedActivity.ReportedCleanSpan : ReportedCleanSpan;
        int newCleanoutGrade = a_activityTransmission.ReportedCleanGradeIsSet ? a_updatedActivity.ReportedCleanoutGrade : ReportedCleanoutGrade;
        decimal newGoodQty = a_activityTransmission.ReportedGoodQtyIsSet ? a_updatedActivity.m_reportedGoodQty : ReportedGoodQty;
        decimal newScrapQty = a_activityTransmission.ReportedScrapQtyIsSet ? a_updatedActivity.ReportedScrapQty : ReportedScrapQty;
        bool newPaused = a_activityTransmission.PausedIsSet ? a_updatedActivity.Paused : Paused;
        InternalActivityDefs.peopleUsages newPeopleUsage = a_activityTransmission.PeopleUsageIsSet ? a_updatedActivity.PeopleUsage : PeopleUsage;
        decimal newNbrOfPeople = a_activityTransmission.NbrOfPeopleIsSet ? a_updatedActivity.NbrOfPeople : NbrOfPeople;
        string newComments = a_activityTransmission.CommentsIsSet ? a_updatedActivity.Comments : Comments;
        string newComments2 = a_activityTransmission.Comments2IsSet ? a_updatedActivity.Comments2 : Comments2;
        InternalActivityDefs.productionStatuses newProductionStatus = a_activityTransmission.ProductionStatusIsSet ? a_updatedActivity.m_productionStatus : ProductionStatus;
        decimal newBatchAmount = a_activityTransmission.BatchAmountSet ? a_updatedActivity.BatchAmount : BatchAmount;
        BaseId jobId = Operation.Job.Id;
        bool activityManualMoveOnly = a_activityTransmission.ActivityManualUpdateOnlyIsSet ? a_updatedActivity.ActivityManualUpdateOnly : ActivityManualUpdateOnly;

        //Only update production info if we are overriding the operation values
        //Cycle Span
        if (!m_productionInfoOverride.OnlyAllowManualUpdatesToCycleSpan)
        {
            if (a_updatedActivity.m_productionInfoOverride.CycleSpanOverride)
            {
                if (m_productionInfoOverride.CycleSpanTicks != a_updatedActivity.m_productionInfoOverride.CycleSpanTicks)
                {
                    m_productionInfoOverride.CycleSpanTicks = a_updatedActivity.m_productionInfoOverride.CycleSpanTicks;
                    updated = true;
                    flagProductionChanges = true;
                    a_dataChanges.FlagEligibilityChanges(jobId);
                }

            }
            else if (m_productionInfoOverride.CycleSpanTicks != m_operation.ProductionInfo.CycleSpanTicks)
            {
                m_productionInfoOverride.CycleSpanTicks = m_operation.ProductionInfo.CycleSpanTicks;
                updated = true;
                flagProductionChanges = true;
                a_dataChanges.FlagEligibilityChanges(jobId);
            }

            m_productionInfoOverride.CycleSpanOverride = a_updatedActivity.m_productionInfoOverride.CycleSpanOverride;
        }

        //QtyPerCycle
        if (!m_productionInfoOverride.OnlyAllowManualUpdatesToQtyPerCycle)
        {
            if (a_updatedActivity.m_productionInfoOverride.QtyPerCycleOverride)
            {
                if (m_productionInfoOverride.QtyPerCycle != a_updatedActivity.m_productionInfoOverride.QtyPerCycle)
                {
                    m_productionInfoOverride.QtyPerCycle = a_updatedActivity.m_productionInfoOverride.QtyPerCycle;
                    updated = true;
                    flagProductionChanges = true;
                    a_dataChanges.FlagEligibilityChanges(jobId);
                }
            }
            else if (m_productionInfoOverride.QtyPerCycle != m_operation.ProductionInfo.QtyPerCycle)
            {
                m_productionInfoOverride.QtyPerCycle = m_operation.ProductionInfo.QtyPerCycle;
                updated = true;
                flagProductionChanges = true;
                a_dataChanges.FlagEligibilityChanges(jobId);
            }

            m_productionInfoOverride.QtyPerCycleOverride = a_updatedActivity.m_productionInfoOverride.QtyPerCycleOverride;
        }

        //SetupSpan
        if (!m_productionInfoOverride.OnlyAllowManualUpdatesToSetupSpan)
        {
            if (a_updatedActivity.m_productionInfoOverride.SetupSpanOverride)
            {
                if (m_productionInfoOverride.SetupSpanTicks != a_updatedActivity.m_productionInfoOverride.SetupSpanTicks)
                {
                    m_productionInfoOverride.SetupSpanTicks = a_updatedActivity.m_productionInfoOverride.SetupSpanTicks;
                    m_productionInfoOverride.ProductionSetupCost = a_updatedActivity.m_productionInfoOverride.ProductionSetupCost;
                    updated = true;
                    flagProductionChanges = true;
                }
            }
            else if (m_productionInfoOverride.SetupSpanTicks != m_operation.ProductionInfo.SetupSpanTicks)
            {
                m_productionInfoOverride.SetupSpanTicks = m_operation.ProductionInfo.SetupSpanTicks;
                m_productionInfoOverride.ProductionSetupCost = m_operation.ProductionInfo.ProductionSetupCost;
                updated = true;
                flagProductionChanges = true;
            }

            m_productionInfoOverride.SetupSpanOverride = a_updatedActivity.m_productionInfoOverride.SetupSpanOverride;
        }

        //PostProcessingSpan
        if (!m_productionInfoOverride.OnlyAllowManualUpdatesToPostProcessingSpan)
        {
            if (a_updatedActivity.m_productionInfoOverride.PostProcessingSpanOverride)
            {
                if (m_productionInfoOverride.PostProcessingSpanTicks != a_updatedActivity.m_productionInfoOverride.PostProcessingSpanTicks)
                {
                    m_productionInfoOverride.PostProcessingSpanTicks = a_updatedActivity.m_productionInfoOverride.PostProcessingSpanTicks;
                    updated = true;
                    flagProductionChanges = true;
                }
            }
            else if (m_productionInfoOverride.PostProcessingSpanTicks != m_operation.ProductionInfo.PostProcessingSpanTicks)
            {
                m_productionInfoOverride.PostProcessingSpanTicks = m_operation.ProductionInfo.PostProcessingSpanTicks;
                updated = true;
                flagProductionChanges = true;
            }

            m_productionInfoOverride.PostProcessingSpanOverride = a_updatedActivity.m_productionInfoOverride.PostProcessingSpanOverride;
        }

        //CleanSpan
        if (!m_productionInfoOverride.OnlyAllowManualUpdatesToCleanSpan)
        {
            if (a_updatedActivity.m_productionInfoOverride.CleanSpanOverride)
            {
                if (m_productionInfoOverride.CleanSpanTicks != a_updatedActivity.m_productionInfoOverride.CleanSpanTicks)
                {
                    m_productionInfoOverride.CleanSpanTicks = a_updatedActivity.m_productionInfoOverride.CleanSpanTicks;
                    m_productionInfoOverride.CleanoutGrade = a_updatedActivity.m_productionInfoOverride.CleanoutGrade;
                    m_productionInfoOverride.CleanoutCost = a_updatedActivity.m_productionInfoOverride.CleanoutCost;
                    updated = true;
                    flagProductionChanges = true;
                }
            }
            else if (m_productionInfoOverride.CleanSpanTicks != m_operation.ProductionInfo.CleanSpanTicks)
            {
                m_productionInfoOverride.CleanSpanTicks = m_operation.ProductionInfo.CleanSpanTicks;
                m_productionInfoOverride.CleanoutGrade = m_operation.ProductionInfo.CleanoutGrade;
                m_productionInfoOverride.CleanoutCost = m_operation.ProductionInfo.CleanoutCost;
                updated = true;
                flagProductionChanges = true;
            }

            m_productionInfoOverride.CleanSpanOverride = a_updatedActivity.m_productionInfoOverride.CleanSpanOverride;
        }

        //PlanningScrapPercent
        if (!m_productionInfoOverride.OnlyAllowManualUpdatesToPlanningScrapPercent)
        {
            if (a_updatedActivity.m_productionInfoOverride.PlanningScrapPercentOverride)
            {
                if (m_productionInfoOverride.PlanningScrapPercent != a_updatedActivity.m_productionInfoOverride.PlanningScrapPercent)
                {
                    m_productionInfoOverride.PlanningScrapPercent = a_updatedActivity.m_productionInfoOverride.PlanningScrapPercent;
                    updated = true;
                    flagProductionChanges = true;
                }
            }
            else if (m_productionInfoOverride.PlanningScrapPercent != m_operation.ProductionInfo.PlanningScrapPercent)
            {
                m_productionInfoOverride.PlanningScrapPercent = m_operation.ProductionInfo.PlanningScrapPercent;
                updated = true;
                flagProductionChanges = true;
            }

            m_productionInfoOverride.PlanningScrapPercentOverride = a_updatedActivity.m_productionInfoOverride.PlanningScrapPercentOverride;
        }
        
        //TransferQty
        if (!m_productionInfoOverride.OnlyAllowManualUpdatesToTransferQty)
        {
            if (a_updatedActivity.m_productionInfoOverride.TransferQtyOverride)
            {
                if (m_productionInfoOverride.TransferQty != a_updatedActivity.m_productionInfoOverride.TransferQty)
                {
                    m_productionInfoOverride.TransferQty = a_updatedActivity.m_productionInfoOverride.TransferQty;
                    updated = true;
                    flagConstraintChanges = true;
                }
            }
            else if (m_productionInfoOverride.TransferQty != m_operation.ProductionInfo.TransferQty)
            {
                m_productionInfoOverride.TransferQty = m_operation.ProductionInfo.TransferQty;
                updated = true;
                flagConstraintChanges = true;
            }

            m_productionInfoOverride.TransferQtyOverride = a_updatedActivity.m_productionInfoOverride.TransferQtyOverride;
        }

        //StorageSpan
        if (!m_productionInfoOverride.OnlyAllowManualUpdatesToStorageSpan)
        {
            if (a_updatedActivity.m_productionInfoOverride.StorageSpanOverride)
            {
                if (m_productionInfoOverride.StorageSpanTicks != a_updatedActivity.m_productionInfoOverride.StorageSpanTicks)
                {
                    m_productionInfoOverride.StorageSpanTicks = a_updatedActivity.m_productionInfoOverride.StorageSpanTicks;
                    updated = true;
                    flagProductionChanges = true;
                }
            }
            else if (m_productionInfoOverride.StorageSpanTicks != m_operation.ProductionInfo.StorageSpanTicks)
            {
                m_productionInfoOverride.StorageSpanTicks = m_operation.ProductionInfo.StorageSpanTicks;
                updated = true;
                flagProductionChanges = true;
            }

            m_productionInfoOverride.StorageSpanOverride = a_updatedActivity.m_productionInfoOverride.StorageSpanOverride;
        }

        //If the allow ManualUpdateOnly flag is set on an import set it
        if (a_activityTransmission.ActivityManualUpdateOnlyIsSet)
        {
            ActivityManualUpdateOnly = a_activityTransmission.ActivityManualUpdateOnly;
        }

        // Activities that have been updated internally (ActivityManualUpdateOnly) and based on System Options
        if (ActivityManualUpdateOnly)
        {
            //Only update the status if the ERP finish is allowed to override.
            //Preserve existing reported times and quantities if ERP overrides are not allowed or if the new qty is less, depending on the option
            if (!a_options.UpdateActivityFromErpEvenIfUpdatedManually || a_options.ErpOverrideOfActivityUpdate == ScenarioOptions.EErpActivityUpdateOverrides.Never)
            {
                //Preserve all dates times
                newFinishDateTicks = ReportedFinishDateTicks;
                newStartDateTicks = ReportedStartDate.Ticks;
                newProcessingStartDateTicks = ReportedProcessingStartTicks;
                newProcessingEndDateTicks = ReportedEndOfRunTicks;
                newProductionStatus = ProductionStatus;
                newSetupSpan = ReportedSetupSpan;
                newRunSpan = ReportedRunSpan;
                newPostProcessingSpan = ReportedPostProcessingSpan;
                newCleanSpan = ReportedCleanSpan;
                newCleanoutGrade = ReportedCleanoutGrade;
                newGoodQty = m_reportedGoodQty;
                newScrapQty = ReportedScrapQty;
                newBatchAmount = BatchAmount;
            }
            else if (a_options.ErpOverrideOfActivityUpdate == ScenarioOptions.EErpActivityUpdateOverrides.IfValuesAreGreater)
            {
                //Preserve all dates times if the new values are smaller
                if (newFinishDateTicks < ReportedFinishDateTicks)
                {
                    newFinishDateTicks = ReportedFinishDateTicks;
                }

                if (newStartDateTicks < ReportedStartDate.Ticks)
                {
                    newStartDateTicks = ReportedStartDate.Ticks;
                }

                if (newProductionStatus < ProductionStatus)
                {
                    newProductionStatus = ProductionStatus;
                }

                if (newSetupSpan < ReportedSetupSpan)
                {
                    newSetupSpan = ReportedSetupSpan;
                }

                if (newRunSpan < ReportedRunSpan)
                {
                    newRunSpan = ReportedRunSpan;
                }

                if (newPostProcessingSpan < ReportedPostProcessingSpan)
                {
                    newPostProcessingSpan = ReportedPostProcessingSpan;
                }

                if (newGoodQty < m_reportedGoodQty)
                {
                    newGoodQty = m_reportedGoodQty;
                }

                if (newScrapQty < ReportedScrapQty)
                {
                    newScrapQty = ReportedScrapQty;
                }

                if (newBatchAmount < BatchAmount)
                {
                    newBatchAmount = BatchAmount;
                }
            }
            else if (a_options.ErpOverrideOfActivityUpdate == ScenarioOptions.EErpActivityUpdateOverrides.Always)
            {
                //nothing to do, use the new values
            }
        }

        updated |= UpdateHelper(true,
            newProductionStatus,
            newStartDateTicks,
            newProcessingStartDateTicks,
            newProcessingEndDateTicks,
            newFinishDateTicks,
            a_baseChanges.ReportedFinishDateChanged,
            newSetupSpan,
            newRunSpan,
            newPostProcessingSpan,
            newCleanSpan,
            new TimeSpan(a_activityTransmission.ReportedStorageTicks),
            a_activityTransmission.ReportedEndOfPostProcessingDate.Ticks,
            a_activityTransmission.ReportedEndOfStorageDate.Ticks,
            newCleanoutGrade,
            newGoodQty,
            newScrapQty,
            false,
            false,
            newPaused,
            newPeopleUsage,
            newNbrOfPeople,
            newComments,
            newComments2,
            newBatchAmount,
            new List<MaterialIssue>(),
            a_allocateMaterialFormOnHand,
            a_releaseProductToWarehouse,
            a_nowUtcForReportedEnd,
            activityManualMoveOnly,
            a_dataChanges);

        if (isScheduled)
        {
            if (flagConstraintChanges)
            {
                a_dataChanges.FlagConstraintChanges(Job.Id);
            }

            if (flagProductionChanges)
            {
                a_dataChanges.FlagProductionChanges(Job.Id);
            }
        }

        updated = ActivityManualUpdateOnly | updated | a_baseChanges.AnyChanges;

        return updated; //return if the activity has been updated.
    }

    /// <summary>
    /// This function is used to transform an imported ActualResourcesUsed CSV list into a ResourceKeyList
    /// </summary>
    internal void SetupActualResourcesUsed(string a_actualResourcesUsedCSV, PlantManager a_plantManager, string a_actExternalId, bool a_isErpUpdate)
    {
        if (string.IsNullOrEmpty(a_actualResourcesUsedCSV))
        {
            return;
        }

        ResourceKeyList actualResources = new ();
        string[] resourceIds = a_actualResourcesUsedCSV.Split(',');

        foreach (string resourceId in resourceIds)
        {
            BaseResource resource = null;
            //First try by BaseId
            if (long.TryParse(resourceId, out long resId))
            {
                resource = a_plantManager.GetResource(new BaseId(resId));
            }

            if (resource == null)
            {
                //Try by ExternalId
                IEnumerable<Resource> resEnumerator = a_plantManager.GetResourceList().Where(x => x.ExternalId == resourceId);
                if (resEnumerator.Any())
                {
                    resource = resEnumerator.FirstOrDefault();
                }
            }

            //Only validate for newly imported values. It's possible that the resource could have been deleted since the activity was finished.
            if (resource == null)
            {
                if (a_isErpUpdate)
                {
                    throw new PTValidationException("3003", new object[] { resourceId, a_actExternalId });
                }
            }
            else
            {
                actualResources.Add(new ResourceKey(resource.PlantId, resource.DepartmentId, resource.Id));
            }
        }

        if (actualResources.Count > 0)
        {
            SetActualResourcesUsed(actualResources);
        }
    }

    internal bool Edit(ScenarioDetail a_sd, ActivityEdit a_edit, IScenarioDataChanges a_dataChanges)
    {
        long nowTicks = PTDateTime.UtcNow.Ticks;
        BaseId jobId = Operation.Job.Id;
        bool updated = base.Edit(a_sd, a_edit, a_dataChanges, jobId);

        #region ProductionInfo updates
        //Store manual update only flags

        #region Cycle Span
        if (a_edit.OnlyAllowManualUpdatesToCycleSpanSet)
        {
            m_productionInfoOverride.OnlyAllowManualUpdatesToCycleSpan = a_edit.OnlyAllowManualUpdatesToCycleSpan;
        }

        if (a_edit.CycleSpanOverrideSet)
        {
            m_productionInfoOverride.CycleSpanOverride = a_edit.CycleSpanOverride;
        }

        if (a_edit.CycleSpanSet || a_edit.CycleSpanOverrideSet)
        {
            if (m_productionInfoOverride.CycleSpanOverride)
            {
                if (m_productionInfoOverride.CycleSpanTicks != a_edit.CycleSpan.Ticks)
                {
                    m_productionInfoOverride.CycleSpanTicks = a_edit.CycleSpan.Ticks;
                    updated = true;
                    a_dataChanges.FlagProductionChanges(jobId);
                }

            }
            else if (m_productionInfoOverride.CycleSpanTicks != m_operation.ProductionInfo.CycleSpanTicks)
            {
                m_productionInfoOverride.CycleSpanTicks = m_operation.ProductionInfo.CycleSpanTicks;
                updated = true;
                a_dataChanges.FlagProductionChanges(jobId);
            }

            if (!m_productionInfoOverride.CycleSpanOverride)
            {
                //If the value is being updated, then the corresponding ProductionInfoOverride value should be updated as well.
                m_productionInfoOverride.CycleSpanTicks = m_operation.ProductionInfo.CycleSpanTicks;
            }
        }
        #endregion

        #region QtyPerCycle
        if (a_edit.OnlyAllowManualUpdatesToQtyPerCycleSet)
        {
            m_productionInfoOverride.OnlyAllowManualUpdatesToQtyPerCycle = a_edit.OnlyAllowManualUpdatesToQtyPerCycle;
        }

        if (a_edit.QtyPerCycleOverrideSet)
        {
            m_productionInfoOverride.QtyPerCycleOverride = a_edit.QtyPerCycleOverride;
        }

        if (a_edit.QtyPerCycleSet || a_edit.QtyPerCycleOverrideSet)
        {
            if (m_productionInfoOverride.QtyPerCycleOverride)
            {
                if (m_productionInfoOverride.QtyPerCycle != a_edit.QtyPerCycle)
                {
                    m_productionInfoOverride.QtyPerCycle = a_edit.QtyPerCycle;
                    updated = true;
                    a_dataChanges.FlagProductionChanges(jobId);
                }
            }
            else if (m_productionInfoOverride.QtyPerCycle != m_operation.ProductionInfo.QtyPerCycle)
            {
                m_productionInfoOverride.QtyPerCycle = m_operation.ProductionInfo.QtyPerCycle;
                updated = true;
                a_dataChanges.FlagProductionChanges(jobId);
            }

            if (!m_productionInfoOverride.QtyPerCycleOverride)
            {
                //If the value is being updated, then the corresponding ProductionInfoOverride value should be updated as well.
                m_productionInfoOverride.QtyPerCycle = m_operation.ProductionInfo.QtyPerCycle;
            }
        }
        #endregion

        #region SetupSpan
        if (a_edit.OnlyAllowManualUpdatesToSetupSpanSet)
        {
            m_productionInfoOverride.OnlyAllowManualUpdatesToSetupSpan = a_edit.OnlyAllowManualUpdatesToSetupSpan;
        }

        if (a_edit.UseSetupSpanIsSet)
        {
            m_productionInfoOverride.SetupSpanOverride = a_edit.UseSetupSpan;
        }

        if (a_edit.SetupSpanTicksIsSet || a_edit.UseSetupSpanIsSet)
        {
            if (m_productionInfoOverride.SetupSpanOverride)
            {
                if (m_productionInfoOverride.SetupSpanTicks != a_edit.SetupSpanTicks)
                {
                    m_productionInfoOverride.SetupSpanTicks = a_edit.SetupSpanTicks;
                    updated = true;
                    a_dataChanges.FlagProductionChanges(jobId);
                }

                if (a_edit.ProductionSetupCostIsSet && m_productionInfoOverride.ProductionSetupCost != a_edit.ProductionSetupCost)
                {
                    m_productionInfoOverride.ProductionSetupCost = a_edit.ProductionSetupCost;
                    updated = true;
                }
            }
            else
            {
                if (m_productionInfoOverride.SetupSpanTicks != m_operation.ProductionInfo.SetupSpanTicks)
                {
                    m_productionInfoOverride.SetupSpanTicks = m_operation.ProductionInfo.SetupSpanTicks;
                    updated = true;
                    a_dataChanges.FlagProductionChanges(jobId);
                }

                if (m_productionInfoOverride.ProductionSetupCost != m_operation.ProductionInfo.ProductionSetupCost)
                {
                    m_productionInfoOverride.ProductionSetupCost = m_operation.ProductionInfo.ProductionSetupCost;
                    updated = true;
                }
            }
        }
        #endregion

        #region PostProcessingSpan
        if (a_edit.OnlyAllowManualUpdatesToPostProcessingSpanSet)
        {
            m_productionInfoOverride.OnlyAllowManualUpdatesToPostProcessingSpan = a_edit.OnlyAllowManualUpdatesToPostProcessingSpan;
        }

        if (a_edit.UsePostProcessingSpanIsSet)
        {
            m_productionInfoOverride.PostProcessingSpanOverride = a_edit.UsePostProcessingSpan;
        }

        if (a_edit.PostProcessingSpanIsSet || a_edit.UsePostProcessingSpanIsSet)
        {
            if (m_productionInfoOverride.PostProcessingSpanOverride)
            {
                if (m_productionInfoOverride.PostProcessingSpanTicks != a_edit.PostProcessingSpan.Ticks)
                {
                    m_productionInfoOverride.PostProcessingSpanTicks = a_edit.PostProcessingSpan.Ticks;
                    updated = true;
                    a_dataChanges.FlagProductionChanges(jobId);
                }
            }
            else if (m_productionInfoOverride.PostProcessingSpanTicks != m_operation.ProductionInfo.PostProcessingSpanTicks)
            {
                m_productionInfoOverride.PostProcessingSpanTicks = m_operation.ProductionInfo.PostProcessingSpanTicks;
                updated = true;
                a_dataChanges.FlagProductionChanges(jobId);
            }

            if (!m_productionInfoOverride.PostProcessingSpanOverride)
            {
                //If the value is being updated, then the corresponding ProductionInfoOverride value should be updated as well.
                m_productionInfoOverride.PostProcessingSpanTicks = m_operation.ProductionInfo.PostProcessingSpanTicks;
            }
        }
        #endregion

        #region CleanSpan
        if (a_edit.OnlyAllowManualUpdatesToCleanSpanSet)
        {
            m_productionInfoOverride.OnlyAllowManualUpdatesToCleanSpan = a_edit.OnlyAllowManualUpdatesToCleanSpan;
        }

        if (a_edit.UseCleanSpanIsSet)
        {
            m_productionInfoOverride.CleanSpanOverride = a_edit.UseCleanSpan;
        }

        if (a_edit.CleanSpanIsSet || a_edit.UseCleanSpanIsSet)
        {
            if (m_productionInfoOverride.CleanSpanOverride)
            {
                if (m_productionInfoOverride.CleanSpanTicks != a_edit.CleanSpan.Ticks)
                {
                    m_productionInfoOverride.CleanSpanTicks = a_edit.CleanSpan.Ticks;
                    updated = true;
                    a_dataChanges.FlagProductionChanges(jobId);
                }

                if (a_edit.ProductionCleanoutCostIsSet && m_productionInfoOverride.CleanoutCost != a_edit.ProductionCleanoutCost)
                {
                    m_productionInfoOverride.CleanoutCost = a_edit.ProductionCleanoutCost;
                    updated = true;
                }
            }
            else
            {
                if (m_productionInfoOverride.CleanSpanTicks != m_operation.ProductionInfo.CleanSpanTicks)
                {
                    m_productionInfoOverride.CleanSpanTicks = m_operation.ProductionInfo.CleanSpanTicks;
                    updated = true;
                    a_dataChanges.FlagProductionChanges(jobId);
                }

                if (m_productionInfoOverride.CleanoutCost != m_operation.ProductionInfo.CleanoutCost)
                {
                    m_productionInfoOverride.CleanoutCost = m_operation.ProductionInfo.CleanoutCost;
                    updated = true;
                }
            }
        }

        if (m_productionInfoOverride.CleanoutGrade != a_edit.CleanoutGrade && a_edit.CleanoutGradeIsSet)
        {
            m_productionInfoOverride.CleanoutGrade = a_edit.CleanoutGrade;
            updated = true;
            a_dataChanges.FlagProductionChanges(jobId);
        }
        #endregion

        #region PlanningScrapPercent
        if (a_edit.OnlyAllowManualUpdatesToPlanningScrapPercentSet)
        {
            m_productionInfoOverride.OnlyAllowManualUpdatesToPlanningScrapPercent = a_edit.OnlyAllowManualUpdatesToPlanningScrapPercent;
        }

        if (a_edit.PlanningScrapPercentOverrideSet)
        {
            m_productionInfoOverride.PlanningScrapPercentOverride = a_edit.PlanningScrapPercentOverride;
        }

        if (a_edit.PlanningScrapPercentSet || a_edit.PlanningScrapPercentOverrideSet)
        {
            if (m_productionInfoOverride.PlanningScrapPercentOverride)
            {
                if (m_productionInfoOverride.PlanningScrapPercent != a_edit.PlanningScrapPercent)
                {
                    m_productionInfoOverride.PlanningScrapPercent = a_edit.PlanningScrapPercent;
                    updated = true;
                    a_dataChanges.FlagProductionChanges(jobId);
                }
            }
            else if (m_productionInfoOverride.PlanningScrapPercent != m_operation.ProductionInfo.PlanningScrapPercent)
            {
                m_productionInfoOverride.PlanningScrapPercent = m_operation.ProductionInfo.PlanningScrapPercent;
                updated = true;
                a_dataChanges.FlagProductionChanges(jobId);
            }

            if (!m_productionInfoOverride.PlanningScrapPercentOverride)
            {
                //If the value is being updated, then the corresponding ProductionInfoOverride value should be updated as well.
                m_productionInfoOverride.PlanningScrapPercent = m_operation.ProductionInfo.PlanningScrapPercent;
            }
        }
        #endregion

        #region Material Post Processing Span
        if (a_edit.OnlyAllowManualUpdatesToMaterialPostProcessingSpanSet)
        {
            m_productionInfoOverride.OnlyAllowManualUpdatesToMaterialPostProcessingSpan = a_edit.OnlyAllowManualUpdatesToMaterialPostProcessingSpan;
        }

        if (a_edit.MaterialPostProcessingSpanOverrideSet)
        {
            m_productionInfoOverride.MaterialPostProcessingSpanOverride = a_edit.MaterialPostProcessingSpanOverride;
        }

        if (a_edit.MaterialPostProcessingSpanTicksSet || a_edit.MaterialPostProcessingSpanTicksSet)
        {
            if (m_productionInfoOverride.MaterialPostProcessingSpanOverride)
            {
                if (m_productionInfoOverride.MaterialPostProcessingSpanTicks != a_edit.MaterialPostProcessingSpanTicks)
                {
                    m_productionInfoOverride.MaterialPostProcessingSpanTicks = a_edit.MaterialPostProcessingSpanTicks;
                    updated = true;
                    a_dataChanges.FlagProductionChanges(jobId);
                }
            }
            else if (m_productionInfoOverride.MaterialPostProcessingSpanTicks != m_operation.ProductionInfo.MaterialPostProcessingSpanTicks)
            {
                m_productionInfoOverride.MaterialPostProcessingSpanTicks = m_operation.ProductionInfo.MaterialPostProcessingSpanTicks;
                updated = true;
                a_dataChanges.FlagProductionChanges(jobId);
            }

            if (!m_productionInfoOverride.MaterialPostProcessingSpanOverride)
            {
                //If the value is being updated, then the corresponding ProductionInfoOverride value should be updated as well.
                m_productionInfoOverride.MaterialPostProcessingSpanTicks = m_operation.ProductionInfo.MaterialPostProcessingSpanTicks;
            }
        }
        #endregion
        #endregion

        updated |= UpdateHelper(false,
            a_edit.ProductionStatus,
            a_edit.ReportedStartDateTicks,
            a_edit.ReportedProcessingStartTicks,
            a_edit.ReportedEndOfRunTicks,
            a_edit.ReportedFinishDateTicks,
            a_edit.ReportedFinishDateTicksSet,
            new TimeSpan(a_edit.ReportedSetupSpan),
            new TimeSpan(a_edit.ReportedRunSpan),
            new TimeSpan(a_edit.ReportedPostProcessingSpan),
            a_edit.ReportedCleanSpan,
            new TimeSpan(a_edit.ReportedStorage),
            a_edit.ReportedEndOfPostProcessingTicks,
            a_edit.ReportedEndOfStorageTicks,
            a_edit.ReportedCleanoutGrade,
            a_edit.ReportedGoodQty,
            a_edit.ReportedScrapQty,
            false,
            false,
            a_edit.Paused,
            a_edit.PeopleUsage,
            a_edit.NbrOfPeople,
            a_edit.Comments,
            a_edit.Comments2,
            a_edit.BatchAmount,
            new List<MaterialIssue>(),
            a_edit.AllocateMaterialFromOnHand,
            a_edit.ReleaseProductToWarehouse,
            new DateTime(nowTicks),
            a_edit.ActivityManualUpdateOnly,
            a_dataChanges
        );

        if (a_edit.ActivityManualUpdateOnlySet && ActivityManualUpdateOnly != a_edit.ActivityManualUpdateOnly)
        {
            ActivityManualUpdateOnly = a_edit.ActivityManualUpdateOnly;
            updated = true;
        }

        StoreActualResourcesIfTracking();

        return updated;
    }

    /// <summary>
    /// This function is used by update as well as Finish functions.
    /// </summary>
    internal bool UpdateHelper(bool a_erpUpdate,
                               InternalActivityDefs.productionStatuses a_productionStatus,
                               long a_reportedStartDate,
                               long a_reportedProcessingStartDate,
                               long a_reportedProcessingEndDate,
                               long a_reportedFinishDate,
                               bool a_reportedFinishDateModified,
                               TimeSpan a_reportedSetupSpan,
                               TimeSpan a_reportedRunSpan,
                               TimeSpan a_reportedPostProcessingSpan,
                               TimeSpan a_reportedCleanSpan,
                               TimeSpan a_reportedStorage,
                               long a_reportedEndOfPostProcessingTicks,
                               long a_reportedEndOfStorageTicks,
                               int a_reportedCleanGrade,
                               decimal a_reportedGoodQty,
                               decimal a_reportedScrapQty,
                               bool a_reportedQtiesAreIncremental,
                               bool a_reportedSpansAreIncremental,
                               bool a_paused,
                               InternalActivityDefs.peopleUsages a_peopleUsage,
                               decimal a_nbrOfPeople,
                               string a_comments,
                               string a_comments2,
                               decimal a_batchAmount,
                               List<MaterialIssue> a_materialIssues,
                               bool a_allocateMaterialFormOnHand,
                               bool a_releaseProductToWarehouse,
                               DateTime a_nowDateTime,
                               bool a_manualUpdateOnly,
                               IScenarioDataChanges a_dataChanges)
    {
        bool updated = false;
        bool isScheduled = Scheduled;
        bool flagConstraintChanges = false;
        bool flagProductionChanges = false;
        string historyMsg;
        string resourceStr = "<unknown>".Localize();
        BaseId plantId = BaseId.NULL_ID;
        BaseId jobId = Operation.Job.Id;

        //This activity has been updated, reset the production rule cache in case it has been changed.
        if (ProductionStatus != InternalActivityDefs.productionStatuses.Finished &&
            a_productionStatus == InternalActivityDefs.productionStatuses.Finished)
        {
            // The activity is being finished.
            if (a_reportedFinishDate <= PTDateTime.MinDateTime.Ticks) // 0 in the updated activity means no value was reported.
            {
                if (!a_erpUpdate)
                {
                    UpdateReportedFinishDate(a_nowDateTime.Ticks);
                }
                else
                {
                    UpdateReportedFinishDate(Operation.ManufacturingOrder.ScenarioDetail.Clock);
                }
            }
            else
            {
                UpdateReportedFinishDate(Math.Max(a_reportedFinishDate, PTDateTime.MinDateTime.Ticks));
            }

            updated = true;
            flagConstraintChanges = true;
        }
        else if (a_reportedFinishDateModified)
        {
            UpdateReportedFinishDate(Math.Max(a_reportedFinishDate, PTDateTime.MinDateTime.Ticks));
            updated = true;
            flagConstraintChanges = true;
        }

        if (Operation.IsOmitted)
        {
            //Verify status. Bad data should not be allowed
            if (a_productionStatus == InternalActivityDefs.productionStatuses.SettingUp || a_productionStatus == InternalActivityDefs.productionStatuses.Running || a_productionStatus == InternalActivityDefs.productionStatuses.PostProcessing)
            {
                if (ProductionStatus == InternalActivityDefs.productionStatuses.SettingUp || ProductionStatus == InternalActivityDefs.productionStatuses.Running || ProductionStatus == InternalActivityDefs.productionStatuses.PostProcessing)
                {
                    //We have to set it to something, the operation was omitted. TODO: Maybe there is a better way to reset the production status.
                    ProductionStatus = InternalActivityDefs.productionStatuses.Ready;
                    updated = true;
                }

                throw new PTValidationException("2962", new object[] { Operation.ManufacturingOrder.Job.ExternalId, Operation.ManufacturingOrder.ExternalId, Operation.ExternalId, ExternalId, a_productionStatus.Localize() });
            }
        }

        //Update values that should be updated regardless of status.
        if (a_reportedStartDate != 0 && a_reportedStartDate != PTDateTime.MinDateTicks && a_reportedStartDate != DateTime.MinValue.Ticks && ReportedStartDateTicks != a_reportedStartDate)
        {
            ReportedStartDateTicks = Math.Max(a_reportedStartDate, PTDateTime.MinDateTime.Ticks);
            updated = true;
            flagConstraintChanges = true;
        }


        if (a_reportedProcessingStartDate != 0 && a_reportedProcessingStartDate != PTDateTime.MinDateTicks && a_reportedProcessingStartDate != DateTime.MinValue.Ticks && ReportedProcessingStartTicks != a_reportedProcessingStartDate)
        {
            ReportedProcessingStartTicks = Math.Max(a_reportedProcessingStartDate, PTDateTime.MinDateTime.Ticks);
            updated = true;
            flagConstraintChanges = true;
        }

        if (a_reportedProcessingEndDate != 0 && a_reportedProcessingEndDate != PTDateTime.MinDateTicks && a_reportedProcessingEndDate != DateTime.MinValue.Ticks && ReportedEndOfRunTicks != a_reportedProcessingEndDate)
        {
            ReportedEndOfRunTicks = Math.Max(a_reportedProcessingEndDate, PTDateTime.MinDateTime.Ticks);
            updated = true;
            flagConstraintChanges = true;
        }

        if (a_reportedEndOfStorageTicks > 0 && a_reportedEndOfStorageTicks != ReportedEndOfStorageTicks)
        {
            ReportedEndOfStorageTicks = a_reportedEndOfStorageTicks;
            updated = true;
            flagConstraintChanges = true;
        }
        if (a_reportedEndOfPostProcessingTicks > 0 && a_reportedEndOfPostProcessingTicks != ReportedEndOfPostProcessingTicks)
        {
            ReportedEndOfPostProcessingTicks = a_reportedEndOfPostProcessingTicks;
            updated = true;
            flagConstraintChanges = true;
        }

        if (ReportedClean != a_reportedCleanSpan.Ticks)
        {
            ReportedClean = Math.Max(a_reportedCleanSpan.Ticks, 0);
            updated = true;
            flagConstraintChanges = true;
        }

        if (ReportedCleanoutGrade != a_reportedCleanGrade)
        {
            ReportedCleanoutGrade = a_reportedCleanGrade;
            updated = true;
            flagConstraintChanges = true;
        }

        if (Paused != a_paused)
        {
            Paused = a_paused;
            updated = true;
            flagConstraintChanges = true;
        }

        if (PeopleUsage != a_peopleUsage)
        {
            //triggers constraint change on InternalActivityFinishT
            PeopleUsage = a_peopleUsage;
            updated = true;
            flagConstraintChanges = true;
            a_dataChanges.FlagEligibilityChanges(Job.Id);
        }

        if (NbrOfPeople != a_nbrOfPeople)
        {
            //triggers constraint change on InternalActivityFinishT
            NbrOfPeople = a_nbrOfPeople;
            updated = true;
            flagConstraintChanges = true;
            a_dataChanges.FlagEligibilityChanges(jobId);
        }

        if (Comments != a_comments)
        {
            Comments = a_comments;
            updated = true;
        }

        if (Comments2 != a_comments2)
        {
            Comments2 = a_comments2;
            updated = true;
        }

        if (BatchAmount != a_batchAmount)
        {
            BatchAmount = a_batchAmount;
            updated = true;
            flagProductionChanges = true;
        }

        bool activityUnfinished = false;
        if (Finished) //already finished
        {
            //Update any non-zero values to allow for updating actuals.
            if (a_reportedSetupSpan.Ticks > 0 && a_reportedSetupSpan != ReportedSetupSpan)
            {
                ReportedSetupSpan = a_reportedSetupSpan;
                updated = true;
                flagConstraintChanges = true;
            }

            if (a_reportedRunSpan.Ticks > 0 && a_reportedRunSpan != ReportedRunSpan)
            {
                ReportedRunSpan = a_reportedRunSpan;
                updated = true;
                flagConstraintChanges = true;
            }

            if (a_reportedPostProcessingSpan.Ticks > 0 && a_reportedPostProcessingSpan != ReportedPostProcessingSpan)
            {
                ReportedPostProcessingSpan = a_reportedPostProcessingSpan;
                updated = true;
                flagConstraintChanges = true;
            }

            if (a_reportedStorage.Ticks > 0 && a_reportedStorage.Ticks != ReportedStorage)
            {
                ReportedStorage = a_reportedStorage.Ticks;
                updated = true;
                flagConstraintChanges = true;
            }

            if (a_reportedCleanSpan.Ticks > 0 && a_reportedCleanSpan != ReportedCleanSpan)
            {
                ReportedCleanSpan = a_reportedCleanSpan;
                updated = true;
                flagConstraintChanges = true;
            }

            if (a_reportedCleanGrade != ReportedCleanoutGrade)
            {
                ReportedCleanoutGrade = a_reportedCleanGrade;
                updated = true;
                flagConstraintChanges = true;
            }

            if (a_reportedGoodQty > 0 && a_reportedGoodQty != ReportedGoodQty)
            {
                ReportedGoodQty = a_reportedGoodQty;
                updated = true;
            }

            if (a_reportedScrapQty > 0 && a_reportedScrapQty != ReportedScrapQty)
            {
                ReportedScrapQty = a_reportedScrapQty;
                updated = true;
            }

            //Un-Finishing the Activity
            if (ProductionStatus == InternalActivityDefs.productionStatuses.Finished && a_productionStatus != InternalActivityDefs.productionStatuses.Finished)
            {
                ProductionStatus = a_productionStatus;
                //Record history
                historyMsg = string.Format("Finished {4} Job {0}, MO {1}, Op {2} Activity {3}".Localize(),
                    Operation.ManufacturingOrder.Job.Name,
                    Operation.ManufacturingOrder.Name,
                    Operation.Name,
                    Id.ToString(),
                    Operation.Description);
                m_operation.ManufacturingOrder.Job.ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(plantId, new[] { Operation.ManufacturingOrder.Job.Id }, historyMsg, typeof(Job), ScenarioHistory.historyTypes.FinishedActivity);

                Operation.ManufacturingOrder.Job.Unschedule(false);
                Operation.ManufacturingOrder.ScenarioDetail.JobsUnscheduled();
                //We need to let the path know that there are valid activities again.
                AlternatePath activityPath = Operation.ManufacturingOrder.AlternatePaths.GetOpsPath(Operation);
                activityPath.StatusesUpdated();
                activityUnfinished = true;
                updated = true;
                a_dataChanges.FlagEligibilityChanges(jobId);
                flagConstraintChanges = true;
            }

            if (!activityUnfinished) //Unfinished activities need the new reported values
            {
                return updated; //Additional processing below is for unfinished activities only.
            }
        }

        if (Batch?.PrimaryResource is { } primaryResource)
        {
            resourceStr = string.Format("{0}, Dept {1}, Plant {2}".Localize(), primaryResource.Name, primaryResource.Department.Name, primaryResource.Department.Plant.Name);
            plantId = primaryResource.Department.Plant.Id;
        }

        //Update this Activity and record history based on the updatedActvity (if the values have changed)
        if (ReportedSetupSpan != a_reportedSetupSpan && !(Operation.AutoReportProgress && a_erpUpdate)) //Don't want to lose the auto-reported progress.
        {
            //Slow Setup History
            if (isScheduled)
            {
                TimeSpan incrementalSetupSpan = a_reportedSetupSpan.Subtract(ReportedSetupSpan);
                TimeSpan scheduledSetupSpan = Batch.SetupCapacitySpan.TimeSpan;
                if (incrementalSetupSpan > scheduledSetupSpan) //reporting longer than we were currently scheduled for
                {
                    string actualTime = incrementalSetupSpan.ToReadableStringHourPrecision();
                    string expectedTime = scheduledSetupSpan.ToReadableStringHourPrecision();
                    historyMsg = string.Format("Slow {4} setup ({5} instead of {6}) for Job {0}, MO {1}, Op {2} Activity {3} on {7}".Localize(),
                        Operation.ManufacturingOrder.Job.Name,
                        Operation.ManufacturingOrder.Name,
                        Operation.Name,
                        Id.ToString(),
                        Operation.Description,
                        actualTime,
                        expectedTime,
                        resourceStr);
                    m_operation.ManufacturingOrder.Job.ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(plantId, new[] { Operation.ManufacturingOrder.Job.Id }, historyMsg, typeof(Job), ScenarioHistory.historyTypes.SlowSetup);
                }
            }

            ReportedSetupSpan = a_reportedSetupSpan;
            flagProductionChanges = true;
            updated = true;
        }

        if (ReportedRunSpan != a_reportedRunSpan && !(Operation.AutoReportProgress && a_erpUpdate)) //Don't want to lose the auto-reported progress.
        {
            //Slow Run History
            if (isScheduled)
            {
                TimeSpan incrementalRunSpan = a_reportedRunSpan.Subtract(ReportedRunSpan);
                TimeSpan scheduledRunSpan = Batch.ProcessingCapacitySpan.TimeSpan;
                if (incrementalRunSpan > scheduledRunSpan) //If unscheduled then the ScheduledRunSpan is zero.
                {
                    string actualTime = incrementalRunSpan.ToReadableStringHourPrecision();
                    string expectedTime = scheduledRunSpan.ToReadableStringHourPrecision();
                    historyMsg = string.Format("Slow {4} run ({5} instead of {6}) for Job {0}, MO {1}, Op {2} Activity {3} on {7}".Localize(),
                        Operation.ManufacturingOrder.Job.Name,
                        Operation.ManufacturingOrder.Name,
                        Operation.Name,
                        Id.ToString(),
                        Operation.Description,
                        actualTime,
                        expectedTime,
                        resourceStr);
                    m_operation.ManufacturingOrder.Job.ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(plantId, new[] { Operation.ManufacturingOrder.Job.Id }, historyMsg, typeof(Job), ScenarioHistory.historyTypes.SlowRun);
                }
            }

            ReportedRunSpan = a_reportedRunSpan;
            flagProductionChanges = true; 
            updated = true;
        }

        if (ReportedPostProcessingSpan != a_reportedPostProcessingSpan && !(Operation.AutoReportProgress && a_erpUpdate)) //Don't want to lose the auto-reported progress.
        {
            //Slow Post Processing History
            if (isScheduled)
            {
                TimeSpan incrementalPostProcessingSpan = a_reportedPostProcessingSpan.Subtract(ReportedPostProcessingSpan);
                TimeSpan scheduledPostProcessingSpan = Batch.PostProcessingSpan.TimeSpan;
                if (incrementalPostProcessingSpan > scheduledPostProcessingSpan)
                {
                    string actualTime = incrementalPostProcessingSpan.ToReadableStringHourPrecision();
                    string expectedTime = scheduledPostProcessingSpan.ToReadableStringHourPrecision();
                    historyMsg = string.Format("Slow {4} post-processing ({5} instead of {6}) for Job {0}, MO {1}, Op {2} Activity {3} on {7}".Localize(),
                        Operation.ManufacturingOrder.Job.Name,
                        Operation.ManufacturingOrder.Name,
                        Operation.Name,
                        Id.ToString(),
                        Operation.Description,
                        actualTime,
                        expectedTime,
                        resourceStr);
                    m_operation.ManufacturingOrder.Job.ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(plantId, new[] { Operation.ManufacturingOrder.Job.Id }, historyMsg, typeof(Job), ScenarioHistory.historyTypes.SlowRun);
                }
            }

            ReportedPostProcessingSpan = a_reportedPostProcessingSpan;
            updated = true;
            flagProductionChanges = true;
        }
        if (ReportedStorageSpan != a_reportedStorage && !(Operation.AutoReportProgress && a_erpUpdate)) //Don't want to lose the auto-reported progress.
        {
            //Slow Post Processing History
            if (isScheduled)
            {
                TimeSpan incrementalStorageSpan = a_reportedStorage.Subtract(ReportedStorageSpan);
                TimeSpan scheduledStorageSpan = Batch.StorageSpan.TimeSpan;
                if (incrementalStorageSpan > scheduledStorageSpan)
                {
                    string actualTime = incrementalStorageSpan.ToReadableStringHourPrecision();
                    string expectedTime = scheduledStorageSpan.ToReadableStringHourPrecision();
                    historyMsg = string.Format("Slow {4} storage ({5} instead of {6}) for Job {0}, MO {1}, Op {2} Activity {3} on {7}".Localize(),
                        Operation.ManufacturingOrder.Job.Name,
                        Operation.ManufacturingOrder.Name,
                        Operation.Name,
                        Id.ToString(),
                        Operation.Description,
                        actualTime,
                        expectedTime,
                        resourceStr);
                    m_operation.ManufacturingOrder.Job.ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(plantId, new[] { Operation.ManufacturingOrder.Job.Id }, historyMsg, typeof(Job), ScenarioHistory.historyTypes.SlowRun);
                }
            }

            ReportedStorageSpan = a_reportedStorage;
            updated = true;
            flagProductionChanges = true;
        }

        //Set these quantities after the Run/Setup/PostProcessing calculations above or else these
        //  quantity changes will interfere with testing whether they're running slow or fast since 
        //  the StandardRunSpan for example uses the ReportedScrapQty.
        if (ReportedGoodQty != a_reportedGoodQty)
        {
            if (!a_erpUpdate)
            {
                //TODO: Andre. Add options for whether to report progress on MR and Products.
                //Update inventory for products.  Only produce for internal progress so ERP inventory values stay in sync.

                decimal qtyJustProduced = a_reportedGoodQty - ReportedGoodQty; //could be negative
                //if (Batch != null && Batch is not TankBatch)
                {
                    ProduceToInventory(qtyJustProduced, a_productionStatus == InternalActivityDefs.productionStatuses.Finished, a_releaseProductToWarehouse, ScenarioDetail.Clock);
                }

                //Issue Materials from the material issues list
                //Dictionary<BaseId, MaterialRequirement> materialRequirementsDictionary = new Dictionary<BaseId, MaterialRequirement>();
                //for (int mrI = 0; mrI < Operation.MaterialRequirements.Count; mrI++)
                //{
                //    MaterialRequirement mr = Operation.MaterialRequirements[mrI];
                //    if (mr.Item != null) //Need Item to Issue below.
                //        materialRequirementsDictionary.Add(mr.Id, mr);
                //}

                //if (a_materialIssues.Count > 0)
                //{
                //    for (int mriI = 0; mriI < a_materialIssues.Count; mriI++)
                //    {
                //        MaterialIssue mrIssue = a_materialIssues[mriI];
                //        if (materialRequirementsDictionary.ContainsKey(mrIssue.MaterialRequirementId))
                //        {
                //            //Update the MR's total issued qty 
                //            MaterialRequirement mr = materialRequirementsDictionary[mrIssue.MaterialRequirementId];
                //            decimal qtyToIssue = mrIssue.QtyToIssue;
                //            if (qtyToIssue < 0 && Math.Abs(qtyToIssue) > mr.IssuedQty) //only allow unissue to zero.
                //                qtyToIssue = 0 - mr.IssuedQty;
                //            mr.IssuedQty += qtyToIssue;

                //            //Decrease the onhand inventory of the specified Warehouse
                //            //TODO Need to pass in Warehouse list for backflushing other Warehouses.
                //            if (mr.Warehouse != null && mr.Warehouse.Id == mrIssue.FromWarehouseId)
                //            {
                //                if (mr.Warehouse.Inventories.Contains(mr.Item.Id))
                //                {
                //                    Inventory inventory = mr.Warehouse.Inventories[mr.Item.Id];
                //                    inventory.SubtractOnHandQty(inventory.OnHandQty);
                //                }
                //                else
                //                    throw new PTValidationException("2470", new object[] { mr.Item.Name, mr.Warehouse.Name });
                //            }
                //            else
                //                throw new PTValidationException("2471");

                //        }
                //    }
                //}
                //else
                {
                    if (a_allocateMaterialFormOnHand)
                    {
                        decimal qtyGoodToConsume = a_reportedGoodQty - ReportedGoodQty;
                        Operation.AutoIssueMaterials(Id, qtyGoodToConsume, a_productionStatus == InternalActivityDefs.productionStatuses.Finished);
                    }
                }
            }

            ReportedGoodQty = ScenarioDetail.ScenarioOptions.RoundQty(a_reportedGoodQty);
            flagProductionChanges = true;
            updated = true;
        }
        else if (ProductionStatus != InternalActivityDefs.productionStatuses.Finished &&
                 a_productionStatus == InternalActivityDefs.productionStatuses.Finished)
        {
            //This operation was finished, but the reported qty didn't change. We still need to allocate tool products
            if (Batch != null)
            {
                ProduceToInventory(0, true, a_releaseProductToWarehouse, PTDateTime.InvalidDateTime.Ticks);
                flagProductionChanges = true;
            }
        }

        if (ReportedScrapQty != a_reportedScrapQty)
        {
            ReportedScrapQty = ScenarioDetail.ScenarioOptions.RoundQty(a_reportedScrapQty);
            //ExcessiveScrap History
            if (ReportedScrapQty > ExpectedScrapQty)
            {
                historyMsg = string.Format("Finished with excessive scrap of {0} {1} for Job {2}, MO {3}, Op {4}, Activity {5} on {6}".Localize(),
                    ReportedScrapQty,
                    Operation.UOM,
                    Operation.ManufacturingOrder.Job.Name,
                    Operation.ManufacturingOrder.Name,
                    Operation.Name,
                    Id,
                    resourceStr);
                m_operation.ManufacturingOrder.Job.ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(plantId, new[] { Operation.ManufacturingOrder.Job.Id }, historyMsg, typeof(Job), ScenarioHistory.historyTypes.ExcessiveScrap);
            }

            if (!a_erpUpdate && a_allocateMaterialFormOnHand) //Only autoissue for internal progress so ERP inventory values stay in sync.
            {
                decimal qtyBadToConsume = a_reportedScrapQty - ReportedScrapQty;
                Operation.AutoIssueMaterials(Id, qtyBadToConsume, a_productionStatus == InternalActivityDefs.productionStatuses.Finished);
            }

            flagProductionChanges = true;
            updated = true;
        }

        if (ProductionStatus != InternalActivityDefs.productionStatuses.Finished &&
            a_productionStatus == InternalActivityDefs.productionStatuses.Finished)
        {
            //Record history
            historyMsg = string.Format("Finished {4} Job {0}, MO {1}, Op {2} Activity {3} on {5}".Localize(),
                Operation.ManufacturingOrder.Job.Name,
                Operation.ManufacturingOrder.Name,
                Operation.Name,
                Id.ToString(),
                Operation.Description,
                resourceStr);
            if (ReportedGoodQty > 0)
            {
                historyMsg += string.Format("; Good: {0}".Localize(), ReportedGoodQty);
            }

            if (ReportedScrapQty > 0)
            {
                historyMsg += string.Format("; Scrapped: {0}".Localize(), ReportedScrapQty);
            }

            m_operation.ManufacturingOrder.Job.ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(plantId, new[] { Operation.ManufacturingOrder.Job.Id }, historyMsg, typeof(Job), ScenarioHistory.historyTypes.FinishedActivity);

            //Update Resource's performance.  The resouce could be null if the Activity is not scheduled.
            if (Batch?.PrimaryResource is {} primaryRes)
            {
                primaryRes.UpdatePerformance(this);
            }

            //				//Job Finished History
            //				if(this.Operation.ManufacturingOrder.Job.Finished)
            //				{
            //					historyMsg=String.Format("Finished {0}",this.Operation.ManufacturingOrder.Job.JobHistoryString);
            //					this.operation.ManufacturingOrder.Job.ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(plantId,new BaseId[]{this.Operation.ManufacturingOrder.Job.Id},historyMsg,typeof(Job),ScenarioHistory.historyTypes.FinishedJob);
            //				}
            updated = true;
            flagProductionChanges = true;
        }

        //If unfinished, the status has already been updated.
        if (ProductionStatus != a_productionStatus)
        {
            InternalActivityDefs.productionStatuses originalStatus = ProductionStatus;

            if (!a_erpUpdate)
            {
                //Manually update statuses if not set.
                if (!ReportedStartDateSet)
                {
                    ReportedStartDateTicks = a_nowDateTime.Ticks;
                    updated = true;
                    flagProductionChanges = true;
                }

                //Run start
                if (originalStatus <= InternalActivityDefs.productionStatuses.SettingUp && a_productionStatus == InternalActivityDefs.productionStatuses.Running)
                {
                    if (ReportedProcessingStartDateTime <= PTDateTime.MinDateTime)
                    {
                        ReportedProcessingStartDateTime = a_nowDateTime;
                        updated = true;
                        flagProductionChanges = true;
                    }
                }

                //Finish is handled at the beginning of the function.
            }

            ProductionStatus = a_productionStatus;

            if (originalStatus < InternalActivityDefs.productionStatuses.Finished && ProductionStatus >= InternalActivityDefs.productionStatuses.Finished)
            {
                UpdateReportedFinishDate(ReportedFinishDateTicks);
                updated = true;
                flagProductionChanges = true;
            }
        }

        StoreActualResourcesIfTracking();

        //if (!a_erpUpdate)
        //{
        //    for (int pI = 0; pI < Operation.Products.Count; ++pI)
        //    {
        //        Product p = Operation.Products[pI];

        //        if (p.InventoryAvailableTiming == ProductDefs.inventoryAvailableTimings.AtOperationResourcePostProcessingEnd)
        //        {
        //            if (Operation.Activities.Count == 1)
        //            {
        //                if (FloatingPoint.ApproximatelyEqual(RequiredFinishQty, ReportedGoodQty))
        //                {
        //                    p.Inventory.OnHandQty += p.TotalOutputQty;
        //                }
        //                else if (FloatingPoint.ApproximatelyEqual(RequiredFinishQty, p.TotalOutputQty))
        //                {
        //                    p.Inventory.OnHandQty += ReportedGoodQty;
        //                }
        //                else
        //                {
        //                    ProductInvOnHandFinishHelper(p, p.TotalOutputQty);
        //                }
        //            }
        //            else
        //            {
        //                decimal actPercentOfOpFinishQty = RequiredFinishQty / Operation.RequiredFinishQty;
        //                decimal actProductTotalOutputQty = actPercentOfOpFinishQty * p.TotalOutputQty;

        //                ProductInvOnHandFinishHelper(p, actProductTotalOutputQty);
        //            }
        //        }
        //    }
        //}

        if (isScheduled)
        {
            if (flagConstraintChanges)
            {
                a_dataChanges.FlagConstraintChanges(Job.Id);
            }

            if (flagProductionChanges)
            {
                a_dataChanges.FlagProductionChanges(Job.Id);
            }
        }

        bool hasChanges = a_dataChanges.HasChanges
                          || a_dataChanges.HasUnprocessedChanges
                          || a_dataChanges.HasConstraintChanges(jobId)
                          || a_dataChanges.HasEligibilityChanges(jobId)
                          || a_dataChanges.HasProductionChanges(jobId);
        updated |= hasChanges;

        return updated;
    }

    /// <summary>
    /// The reported finish date has changed. Successors should be updated if a constraint has changed.
    /// </summary>
    private void UpdateReportedFinishDate(long a_finishTicks)
    {
        ReportedFinishDateTicks = a_finishTicks;
        //We need to trigger saving the successor constraint dates.
        Operation.ActivityFinished();
        Operation.ManufacturingOrder.ScenarioDetail.ActivitiesFinished();
    }

    private void ProductInvOnHandFinishHelper(Product a_p, decimal a_productTotalOutputQtyOfAct)
    {
        if (a_p.Inventory != null)
        {
            decimal completionRatio = ReportedGoodQty / RequiredFinishQty;
            decimal productQty = completionRatio * a_productTotalOutputQtyOfAct;
            productQty = ScenarioDetail.ScenarioOptions.RoundQty(productQty);
            a_p.Inventory.AddOnHandQty(productQty);
        }
    }

    /// <summary>
    /// Whether some type of progress has been reported. That is whether any of the following have been reported: ReportedGoodQty, ReportedRunSpan,
    /// ReportedScrapQty, ReportedSetupSpan, or ReportedPostProcessingSpan.
    /// </summary>
    internal bool ProgressReported => ReportedGoodQty != 0 || ReportedRun != 0 || ReportedScrapQty != 0 || ReportedSetup != 0 || ReportedPostProcessing != 0;
    #endregion

    #region Persistent Flags
    private BoolVector32 m_persistentFlags;

    private const short c_scheduledOnlyForPostProcessingTimeIdx = 0;
    private const short c_pausedIdx = 1;
    private const short c_activityManualUpdateOnlyIdx = 2;
    private const short c_inProcessBeforeStatusUpdateIdx = 3;
    private const short c_hotIdx = 4;
    private const short c_scheduledOnlyForCleanTimeIdx = 6;

    internal bool ScheduledOnlyForPostProcessingTime
    {
        get => m_persistentFlags[c_scheduledOnlyForPostProcessingTimeIdx];
        set => m_persistentFlags[c_scheduledOnlyForPostProcessingTimeIdx] = value;
    }

    internal bool ScheduledOnlyForCleanTime
    {
        get => m_persistentFlags[c_scheduledOnlyForCleanTimeIdx];
        set => m_persistentFlags[c_scheduledOnlyForCleanTimeIdx] = value;
    }

    /// <summary>
    /// Indicates that the current setup or run process has been temporarily suspended due to something like an operator break, end of shift, etc.
    /// Pausing the Activity does not necessarily mean it will be reschedulable.  That depends upon the Activity's Production Status and Hold status.
    /// This is primarily a visual indicator that the Activity is not currently being worked on.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public bool Paused
    {
        get => m_persistentFlags[c_pausedIdx];
        private set => m_persistentFlags[c_pausedIdx] = value;
    }

    /// <summary>
    /// Indicates that the Activity was finished or partially finished from within PlanetTogether or
    /// with an ActivityFinishT from some other source,
    /// rather than from the ERP interface.  Therefore future ERP Activity updates may be ignored depending upon the System Option for ERP Update of Activities Finished Internally.
    /// </summary>
    public bool ActivityManualUpdateOnly
    {
        get => m_persistentFlags[c_activityManualUpdateOnlyIdx];
        set => m_persistentFlags[c_activityManualUpdateOnlyIdx] = value;
    }

    /// <summary>
    /// Whether the activity was in-process before the status update.
    /// </summary>
    private bool InProcessBeforeStatusUpdate
    {
        get => m_persistentFlags[c_inProcessBeforeStatusUpdateIdx];
        set => m_persistentFlags[c_inProcessBeforeStatusUpdateIdx] = value;
    }
    #endregion

    #region ERP transmission status update
    /// <summary>
    /// Whether the activities status has moved from pre production to in process.
    /// </summary>
    internal bool ChangedToRunningStatus => !InProcessBeforeStatusUpdate && InProduction();

    /// <summary>
    /// Call this function before handling a JobT or some other transmission that updates the status of jobs.
    /// It resets the activity variables that indicate the type of updates that have occurred.
    /// </summary>
    internal override void ResetERPStatusUpdateVariables()
    {
        base.ResetERPStatusUpdateVariables();

        InProcessBeforeStatusUpdate = ProductionStatusInternal == ProductionStatusCategory.Production;
    }
    #endregion

    #region Update related functions
    /// <summary>
    /// If the ResourceRequirements have been changed then this function should be called to notify the activitiy of the
    /// change in the number of ResourceRequirements.
    /// </summary>
    internal void NotificationOfResourceRequirementChanges()
    {
        InitializeResourceLocks();
    }
    #endregion

    #region Cost
    public decimal ProductionSetupCost
    {
        get => m_productionInfoOverride.ProductionSetupCost;
        internal set => m_productionInfoOverride.ProductionSetupCost = value;
    }

    /// <summary>
    /// Setup Hours Scheduled times the Resource's Standard Hourly Cost.
    /// </summary>
    public decimal TotalSetupCost
    {
        get
        {
            decimal cost = 0;
            for (int i = 0; i < ResourceRequirementBlockCount; i++)
            {
                ResourceBlock rb = GetResourceRequirementBlock(i);
                if (rb != null)
                {
                    if (Batch != null)
                    {
                        cost += Batch.SetupCapacitySpan.GetTotalCost();
                    }
                }
            }

            return cost;
        }
    }

    public decimal CleanoutCost
    {
        get => m_productionInfoOverride.CleanoutCost;
        internal set => m_productionInfoOverride.CleanoutCost = value;
    }

    /// <summary>
    /// Cleans Hours Scheduled times the Resource's Standard Hourly Cost.
    /// </summary>
    public decimal TotalCleanCost
    {
        get
        {
            decimal cost = 0;
            for (int i = 0; i < ResourceRequirementBlockCount; i++)
            {
                ResourceBlock rb = GetResourceRequirementBlock(i);
                if (rb != null)
                {
                    if (Batch != null)
                    {
                        cost += Batch.CleanSpan.GetTotalCost();
                    }
                }
            }

            return cost;
        }
    }

    /// <summary>
    /// Calculate the total labor cost of this activity.
    /// </summary>
    public override decimal LaborCost
    {
        get
        {
            decimal cost = 0;
            for (int i = 0; i < ResourceRequirementBlockCount; i++)
            {
                ResourceBlock rb = GetResourceRequirementBlock(i);
                if (rb != null)
                {
                    cost += rb.CalcLaborCost();
                }
            }

            return cost;
        }
    }

    /// <summary>
    /// Calculate the machine cost of this activity.
    /// </summary>
    public override decimal MachineCost
    {
        get
        {
            decimal cost = 0;

            for (int i = 0; i < ResourceRequirementBlockCount; i++)
            {
                ResourceBlock rb = GetResourceRequirementBlock(i);
                if (rb != null)
                {
                    cost += rb.CalcMachineCost();
                }
            }

            return cost;
        }
    }



    /// <summary>
    /// Calculate the subcontractor cost of this activity.
    /// </summary>
    public override decimal SubcontractCost
    {
        get
        {
            decimal cost = 0;
            for (int i = 0; i < ResourceRequirementBlockCount; i++)
            {
                ResourceBlock rb = GetResourceRequirementBlock(i);
                if (rb != null)
                {
                    cost += rb.CalcSubcontractorCost();
                }
            }

            return cost;
        }
    }

    #endregion Cost

    internal Job Job => Operation.Job;

    internal ManufacturingOrder ManufacturingOrder => Operation.ManufacturingOrder;

    #region Material
    /// <summary>
    /// Gets a list of the Material Requirements supplied by this Activity.
    /// SLOW -- This iterates through the list of Jobs searching for supplied activities so it can slow for large data sets.
    /// </summary>
    /// <param name="jobs">A list of Jobs to search.</param>
    /// <returns>The list of MaterialRequirements and qties supplied.</returns>
    public List<MaterialRequirement.MaterialRequirementSupply> GetMaterialRequirementsSupplied()
    {
        List<MaterialRequirement.MaterialRequirementSupply> list = new();
        HashSet<BaseId> mrIds = new(); //This is needed to track when a single MR has multiple adjustments for the same lot
        foreach (Product product in Operation.Products)
        {
            mrIds.Clear();
            if (product.SupplyLot != null)
            {
                foreach (Adjustment adjustment in product.SupplyLot.GetAdjustmentArray())
                {
                    if (adjustment is MaterialRequirementAdjustment mrAdjustment)
                    {
                        if (!mrIds.Contains(mrAdjustment.Material.Id))
                        {
                            MaterialRequirement.MaterialRequirementSupply mrSupply = new(mrAdjustment.Material, mrAdjustment.Activity.Operation, mrAdjustment.Qty);
                            list.Add(mrSupply);
                            mrIds.Add(mrAdjustment.Material.Id);
                        }
                    }
                }
            }
        }

        return list;
    }

    #endregion Material

    #region Flags
    private FlagList m_customFlagsList;

    /// <summary>
    /// Flags that are created with a customization.  These values are not stored.
    /// This value is initially null.  To add a Flag a list must first be created.
    /// This is to save on memory since this list is possible for every BaseIdObject in the system.
    /// </summary>
    [Browsable(false)]
    public FlagList CustomFlagsList
    {
        get => m_customFlagsList;
        set => m_customFlagsList = value;
    }
    #endregion

    #region BATCH
    public ProductionInfo ScheduledOrDefaultProductionInfo
    {
        get
        {
            if (Batch != null)
            {
                return GetResourceProductionInfo(Batch.PrimaryResource);
            }

            return ActivityProductionInfo;
        }
    }

    protected ProductionInfo ActivityProductionInfo => m_productionInfoOverride;

    protected ProductionInfo m_productionInfoOverride;

    public ProductionInfo GetResourceProductionInfo(BaseResource a_res)
    {
        if (a_res == null)
        {
            return ScheduledOrDefaultProductionInfo;
        }

        if (m_resourceProductionInfos.TryGetValue(a_res.Id, out ProductionInfo pi))
        {
            return pi;
        }

        return ActivityProductionInfo;
    }

    private readonly Dictionary<BaseId, ProductionInfo> m_resourceProductionInfos = new ();

    // No serialization of the batch is done here. It's done somewhere else and during the scenario's restore references these fields are initialized.
    private Batch m_batch; // [BATCH]

    public Batch Batch
    {
        get => m_batch;
        set => m_batch = value;
    }

    /// <summary>
    /// Whether the Activity has been Batched using the Operation Batch Code.
    /// </summary>
    public bool Batched => Batch != null && Batch.ActivitiesCount > 1;

    private LinkedListNode<InternalActivity> m_batchActNode; // [BATCH]

    /// <summary>
    /// Link this activity to the batch that is scheduled for it.
    /// </summary>
    /// <param name="a_batch"></param>
    /// <param name="a_batchNode"></param>
    internal void Batch_SetBatch(Batch a_batch, LinkedListNode<InternalActivity> a_batchNode)
    {
        Batch = a_batch;
        m_batchActNode = a_batchNode;
        m_resourceTransferTimeTicksAtActivityFinishTime = a_batch.PrimaryResource.TransferSpanTicks;
    }

    /// <summary>
    /// Clears the batch data from the activity and removes the activity from the batch.
    /// If the batch is empty after removal of the activity, or the activity isn't a member of a batch, true is returned.
    /// If the batch is still associated with activities, false is returned.
    /// </summary>
    /// <returns>Whether unscheduling the activity caused its associated batch to be empty.</returns>
    internal ResourceBlock[] Batch_Unschedule(bool a_removeFromBatch)
    {
        ResourceBlock[] resBlocks = null;

        if (Batch != null)
        {
            resBlocks = Batch.UnscheduleAct(m_batchActNode, a_removeFromBatch);
            Batch = null;
            m_batchActNode = null;
        }

        return resBlocks;
    }

    /// <summary>
    /// Returns a Block or null if no Block found.
    /// </summary>
    /// <param name="aBlockId"></param>
    /// <returns></returns>
    public ResourceBlock FindBlock(BaseId aBlockId)
    {
        if (Batch == null)
        {
            return null;
        }

        for (int i = 0; i < Batch.BlockCount; i++)
        {
            ResourceBlock block = Batch.BlockAtIndex(i);
            if (block.Id == aBlockId)
            {
                return block;
            }
        }

        return null;
    }
    #endregion

    public enum EPerformances { Fast, Slow, Normal }

    public EPerformances GetPerformance(ScenarioOptions a_options, out int o_percentOfStandard, out decimal o_standardHrs, out decimal o_expectedHrs)
    {
        // None of these out variables are used, and they don't seem to be the correct values,
        // but this is what was here before I changed it so that the Gantt Viewer's Track Actuals indicator
        // would show the correct label color
        //o_standardHrs = (decimal)Operation.StandardSpan.TotalHours;
        o_standardHrs = Operation.SchedulingHours;
        o_expectedHrs = Operation.ExpectedRunHours;
        o_percentOfStandard = Operation.PercentOfStandard;
        // In the context of the Activity Status control, Operation.SchedulingHours seems to correspond
        // to the Expected Hours column while ExpectedRunHours corresponds to the Reported Column.

        if (o_standardHrs != 0)
        {
            if (o_expectedHrs > o_standardHrs + o_standardHrs * (a_options.PerformingSlowPercentage / 100.0m))
            {
                return EPerformances.Slow;
            }

            if (o_expectedHrs < o_standardHrs - o_standardHrs * (a_options.PerformingFastPercentage / 100.0m))
            {
                return EPerformances.Fast;
            }

            return EPerformances.Normal;
        }

        return EPerformances.Normal; //don't know
    }

    #region Optimization Score
    /// <summary>
    /// The Percentage of the operation this activity represents. For example % of required quantity, products, materials, etc.
    /// </summary>
    public decimal SplitRatio
    {
        get
        {
            if (!Operation.Split)
            {
                return 1m; //The full operation quantity
            }

            //This activity represents a portion of the operation requirement
            return RequiredFinishQty / Operation.RequiredFinishQty;
        }
    }

    private const string IN_PRODUCTION = "In-Production. Special score used.";
    private const string PAUSED = "Started and Paused. Special score used.";
    private const string ANCHORED = "Anchored. Special score used.";
    private const string MO_IN_PROCESS = "Partially scheduled MO Batch. Special score used.";
    private const string SCORE_ANALYSIS_DISABLED = "Score analysis feature is turned off. It can be turned on from System Options.";

    /// <summary>
    /// Constructs a string detailing the information regarding different score the
    /// simulation assiged to this Activity
    /// </summary>
    /// <returns></returns>
    public string GetScheduledResourceScoreDetails(BaseId a_scheduledResId)
    {
        if (!ScenarioDetail.ScenarioOptions.CalculateBalancedCompositeScore)
        {
            return SCORE_ANALYSIS_DISABLED.Localize();
        }

        if (InProduction())
        {
            return IN_PRODUCTION.Localize();
        }

        if (Paused)
        {
            return PAUSED.Localize();
        }

        if (Anchored)
        {
            return ANCHORED.Localize();
        }

        if (Operation.m_manufacturingOrderBatch_batchOrderData_op_index > 1)
        {
            return MO_IN_PROCESS.Localize();
        }

        return GetScheduledResourceScoreDetailsBase(a_scheduledResId);
    }
    #endregion

    public ActivityKey CreateActivityKey()
    {
        ActivityKey ak = new (Job.Id, ManufacturingOrder.Id, Operation.Id, Id);
        return ak;
    }

    public bool Equals(ActivityKey a_ak)
    {
        ActivityKey ak = CreateActivityKey();
        return ak == a_ak;
    }

    #region Locked Resource
    /// <summary>
    /// Reference to Resources that resource requirements are locked to.
    /// The dictionary will store RR index and locked resource. If the index is not in the dictionary, it isn't locked
    /// </summary>
    [DebugLogging(EDebugLoggingType.None)]
    private Dictionary<int, Resource> m_lockedResources;

    /// <summary>
    /// Return a reference to the resource the primary resource requirement is locked to.
    /// </summary>
    /// <returns>NULL if the primary resource requirement is not locked to a resource.</returns>
    public Resource PrimaryResourceRequirementLock()
    {
        return ResourceRequirementLock(0);

    }

    /// <summary>
    /// Returns a reference to the resource a resource requirement is locked to. NULL is returned if the resource requirement is not locked to any resource.
    /// </summary>
    /// <param name="resourceRequirementIndex"></param>
    /// <returns>NULL if the resource requirement is not locked to any resource.</returns>
    public Resource ResourceRequirementLock(int a_resourceRequirementIndex)
    {
        if (m_lockedResources.TryGetValue(a_resourceRequirementIndex, out Resource res))
        {
            return res;
        }

        return null;
    }
    #endregion

    /// <summary>
    /// Remove references to a locked resource to avoid serialization errors
    /// </summary>
    /// <param name="a_baseResource"></param>
    public void ResourceDeleteNotification(BaseResource a_baseResource)
    {
        // Collect keys first so we're not modifying the collection while iterating it.
        var keysToRemove = m_lockedResources
                           .Where(kvp => kvp.Value == a_baseResource)
                           .Select(kvp => kvp.Key);      // materialize now

        foreach (int key in keysToRemove)
        {
            m_lockedResources.Remove(key);   // O(1) each
        }
    }

    /// <summary>
    /// Update the Inventory for the Products.
    /// </summary>
    /// <param name="a_qtyJustProduced">Qty of good items just produced.</param>
    /// <param name="a_actFinished"></param>
    /// <param name="a_releaseProductToInventory"></param>
    /// <param name="a_newClockDate">The new clock advance date or the PT clock</param>
    private void ProduceToInventory(decimal a_qtyJustProduced, bool a_actFinished, bool a_releaseProductToInventory, long a_newClockDate)
    {
#if DEBUG
        if (!Scheduled)
        {
            throw new DebugException("Producing to inventory on a non scheduled Activity.");
        }
#endif

        for (int pI = 0; pI < Operation.Products.Count; ++pI)
        {
            Product p = Operation.Products[pI];
            if (p.Inventory != null)
            {
                decimal qtyToProduce = 0;
                if (p.Item.ItemType == ItemDefs.itemTypes.Tool)
                {
                    //Tools are only produced when the activity is done and relinquished them
                    //Operations with multiple activities must have the final activity finished
                    if (a_actFinished)
                    {
                        qtyToProduce = m_productQtys[pI];
                    }
                    else
                    {
                        //nothing to do
                        continue;
                    }
                }
                else
                {
                    decimal productRatio = p.TotalOutputQty / Operation.RequiredFinishQty;
                    if (a_actFinished)
                    {
                        qtyToProduce = RequiredFinishQty * productRatio;
                    }
                    else
                    {
                        qtyToProduce = Math.Min(p.TotalOutputQty, ScenarioDetail.ScenarioOptions.RoundQty(productRatio * a_qtyJustProduced));
                    }
                }

                if (a_releaseProductToInventory)
                {
                    bool productionFinished = false;
                    long productionDateTicks = GetReportedProductionDate(p.InventoryAvailableTiming);
                    //If finished, we know exactly where the lot was produced
                    switch (p.InventoryAvailableTiming)
                    {
                        case ProductDefs.EInventoryAvailableTimings.AtOperationRunStart:
                            productionFinished = a_newClockDate > Batch.SetupEndTicks;
                            break;
                        case ProductDefs.EInventoryAvailableTimings.AtOperationRunEnd:
                        case ProductDefs.EInventoryAvailableTimings.ByProductionCycle:
                            productionFinished = a_newClockDate > Batch.ProcessingEndTicks;
                            break;
                        case ProductDefs.EInventoryAvailableTimings.AtOperationResourcePostProcessingEnd:
                        case ProductDefs.EInventoryAvailableTimings.DuringPostProcessing:
                            productionFinished = a_newClockDate > Batch.PostProcessingEndTicks;
                            break;
                        case ProductDefs.EInventoryAvailableTimings.DuringStorage:
                        case ProductDefs.EInventoryAvailableTimings.AtStorageEnd:
                            productionFinished = a_newClockDate > Batch.EndOfStorageTicks;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (m_generatedLotIds.TryGetValue(p.Id, out BaseId generateLotId))
                    {
                        Lot supplyLot = p.Inventory.Lots.GetById(generateLotId);

                        if (a_actFinished || productionFinished || qtyToProduce > p.RemainingOutputQty)
                        {
                            //We are storing everything, convert the full lot
                            supplyLot.ConvertToInventory(productionDateTicks + ScheduledOrDefaultProductionInfo.MaterialPostProcessingSpanTicks);
                        }
                        else
                        {
                            supplyLot.StorePartialInInventory(a_newClockDate + ScheduledOrDefaultProductionInfo.MaterialPostProcessingSpanTicks, qtyToProduce);
                        }
                    }
                }

                //Update Completed quantity to be used in production calculations
                p.CompletedQty += qtyToProduce;
            }
        }
    }

    private long GetReportedProductionDate(ProductDefs.EInventoryAvailableTimings a_productAvailableTiming)
    {
        switch (a_productAvailableTiming)
        {
            case ProductDefs.EInventoryAvailableTimings.AtOperationRunStart:
                
                return ReportedProcessingStartDateSet ? ReportedProcessingStartTicks : Batch.SetupEndTicks;
            case ProductDefs.EInventoryAvailableTimings.AtOperationRunEnd:

                return ReportedEndOfProcessingSet ? ReportedEndOfRunTicks : Batch.ProcessingEndTicks;
            case ProductDefs.EInventoryAvailableTimings.ByProductionCycle:
                
                //Reported or scheduled start of run plus cycle span ticks for By Production Cycle
                return ReportedProcessingStartDateSet ? ReportedProcessingStartTicks + ActivityProductionInfo.CycleSpanTicks : Batch.SetupEndTicks + ActivityProductionInfo.CycleSpanTicks;
            case ProductDefs.EInventoryAvailableTimings.AtOperationResourcePostProcessingEnd:
                
                return ReportedEndOfPostProcessingSet ? ReportedEndOfPostProcessingTicks : Batch.PostProcessingEndTicks;
            case ProductDefs.EInventoryAvailableTimings.DuringPostProcessing:
                
                //Reported or Scheduled start of post-processing for during post-processing
                return ReportedEndOfProcessingSet ? ReportedEndOfRunTicks : Batch.ProcessingEndTicks;
            case ProductDefs.EInventoryAvailableTimings.DuringStorage:
                
                //Reported or Scheduled start of Storage for During Storage
                return ReportedEndOfPostProcessingSet ? ReportedEndOfPostProcessingTicks : Batch.PostProcessingEndTicks;
            case ProductDefs.EInventoryAvailableTimings.AtStorageEnd:
                
                return ReportedEndOfStorageSet ? ReportedEndOfStorageTicks : Batch.EndOfStorageTicks;

            default:
                throw new ArgumentOutOfRangeException(nameof(a_productAvailableTiming), a_productAvailableTiming, null);
        }
    }

    /// <summary>
    /// The volume this product will take up during production
    /// This is based on the total quantity to produce, regardless of reported values, and the item volume.
    /// This value can be overridden with VolumeOverride
    /// </summary>
    public decimal TotalRequiredVolume
    {
        get
        {
            if (Operation.Products.PrimaryProduct is Product primaryProduct)
            {
                decimal primaryProductTotalRequiredVolume = SplitRatio * primaryProduct.TotalRequiredVolume;
                return ScenarioDetail.ScenarioOptions.RoundQty(primaryProductTotalRequiredVolume);
            }

            return 0m;
        }
    }

    /// <summary>
    /// The volume this product will take up during production
    /// This is based on the remaining quantity to produce and the item volume.
    /// This value can be overridden with VolumeOverride
    /// </summary>
    public decimal RemainingRequiredVolume
    {
        get
        {
            if (Operation.Products.PrimaryProduct is Product primaryProduct)
            {
                decimal primaryProductTotalRequiredVolume = SplitRatio * primaryProduct.RemainingVolume;
                return ScenarioDetail.ScenarioOptions.RoundQty(primaryProductTotalRequiredVolume);
            }

            return 0m;
        }
    }

    internal void Deleting(PlantManager a_plantManager, IScenarioDataChanges a_dataChanges)
    {
        a_dataChanges.ActivityChanges.DeletedObject(Id);
    }

    /// <summary>
    /// Copy in memory and initialize the copy
    /// </summary>
    /// <param name="a_internalOperation"></param>
    /// <returns></returns>
    internal InternalActivity Clone(InternalOperation a_internalOperation)
    {
        InternalActivity clone = this.CopyInMemory();

        //Initialize the copy
        clone.RestoreReferences(a_internalOperation);
        m_generatedLotIds.Clear();

        return clone;
    }
}

/// <summary>
/// Stores the activity JIT dates for a specific resource
/// </summary>
public struct ActivityResourceBufferInfo : IPTSerializable
{
    /// <summary>
    /// Past this point, the activity will be late
    /// </summary>
    public long BufferEndDate;

    /// <summary>
    /// The date at which the Activity/Operation needs to finish by to avoid buffer penetration
    /// </summary>
    public long BufferNeedDate;

    /// <summary>
    /// The date at which the activity needs to start by to avoid buffer penetration
    /// </summary>
    public long DbrJitStartDate;

    /// <summary>
    /// The end of the headstart window where the activity will be released to schedule even if there are no other activities to compete
    /// </summary>
    public long SequenceHeadStartWindowEndDate;

    /// <summary>
    /// The initial release of this activity in its head start window (if set)
    /// </summary>
    public long ReleaseDate;

    /// <summary>
    /// The date at which the activity must start by in order to not be late
    /// </summary>
    public long JitStartDate;


    /// <summary>
    /// The date at which the activity capacity will end and the transfer starts. The transfer will end at the Operation Need Date
    /// </summary>
    public long JitTransferStartDate;

    //internal long m_resTT;
    //internal long m_resConnTransitTicks;

    public ActivityResourceBufferInfo(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 13011)
        {
            a_reader.Read(out BufferEndDate);
            a_reader.Read(out DbrJitStartDate);
            a_reader.Read(out BufferNeedDate);
            a_reader.Read(out SequenceHeadStartWindowEndDate);
            a_reader.Read(out ReleaseDate);
            a_reader.Read(out m_operationDynamicBufferTicks);
            a_reader.Read(out JitStartDate);
            a_reader.Read(out JitTransferStartDate);
        }
        else if (a_reader.VersionNumber >= 13010)
        {
            a_reader.Read(out BufferEndDate);
            a_reader.Read(out DbrJitStartDate);
            a_reader.Read(out BufferNeedDate);
            a_reader.Read(out SequenceHeadStartWindowEndDate);
            a_reader.Read(out ReleaseDate);
            a_reader.Read(out m_operationDynamicBufferTicks);
            a_reader.Read(out JitStartDate);
        }
        else
        {
            a_reader.Read(out BufferEndDate);
            a_reader.Read(out DbrJitStartDate);
            a_reader.Read(out BufferNeedDate);
        }
    }

    public ActivityResourceBufferInfo(ActivityResourceBufferInfo a_sourceResourceBufferInfo)
    {
       BufferEndDate = a_sourceResourceBufferInfo.BufferEndDate;
       DbrJitStartDate = a_sourceResourceBufferInfo.DbrJitStartDate;
       BufferNeedDate = a_sourceResourceBufferInfo.BufferNeedDate;
       SequenceHeadStartWindowEndDate = a_sourceResourceBufferInfo.SequenceHeadStartWindowEndDate;
       ReleaseDate = a_sourceResourceBufferInfo.ReleaseDate;
       OperationDynamicBuffer = a_sourceResourceBufferInfo.OperationDynamicBuffer;
       JitStartDate = a_sourceResourceBufferInfo.JitStartDate;
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(BufferEndDate);
        a_writer.Write(DbrJitStartDate);
        a_writer.Write(BufferNeedDate);
        a_writer.Write(SequenceHeadStartWindowEndDate);
        a_writer.Write(ReleaseDate);
        a_writer.Write(OperationDynamicBuffer);
        a_writer.Write(JitStartDate);
        a_writer.Write(JitTransferStartDate);
    }

    public int UniqueId => 1136;

    private const long c_minOperationBufferTicksMin = 0;

    public bool DbrJitDateCalculated => DbrJitStartDate != PTDateTime.InvalidDateTimeTicks;
    public bool HasBuffers => BufferEndDate != BufferNeedDate;

    private long m_operationDynamicBufferTicks = c_minOperationBufferTicksMin;

    /// <summary>
    /// A dynamic buffer to release predecessors earlier to account for variability between operation steps
    /// </summary>
    internal long OperationDynamicBuffer
    {
        get => m_operationDynamicBufferTicks;
        set
        {
            if (value < c_minOperationBufferTicksMin)
            {
                throw new PTValidationException("JitBufferTicks value was invalid.");
            }

            m_operationDynamicBufferTicks = value;
        }
    }

    private static ActivityResourceBufferInfo Empty => new()
    {
        BufferEndDate = PTDateTime.InvalidDateTimeTicks,
        DbrJitStartDate = PTDateTime.InvalidDateTimeTicks,
        BufferNeedDate = PTDateTime.InvalidDateTimeTicks,
        SequenceHeadStartWindowEndDate = PTDateTime.InvalidDateTimeTicks,
        ReleaseDate = PTDateTime.InvalidDateTimeTicks,
        OperationDynamicBuffer = PTDateTime.InvalidDateTimeTicks,
        JitStartDate = PTDateTime.InvalidDateTimeTicks
    };

    internal static ActivityResourceBufferInfo EmptyBufferInfo => Empty;
}

/// <summary>
/// Stores activity JIT date info for all resources
/// Tracks the Earliest Info for Operation level calculations
/// </summary>
internal class ActivityBufferInfo : IPTSerializable
{
    private readonly Dictionary<BaseId, ActivityResourceBufferInfo> m_resourceInfos = new ();

    public ActivityBufferInfo(IReader a_reader)
    {
        EarliestJitBufferInfo = new ActivityResourceBufferInfo(a_reader);
        a_reader.Read(out int infosCount);
        for (long i = 0; i < infosCount; i++)
        {
            BaseId resId = new (a_reader);
            ActivityResourceBufferInfo resBufferInfo = new (a_reader);
            m_resourceInfos.Add(resId, resBufferInfo);
        }
    }

    internal ActivityBufferInfo()
    {
        EarliestJitBufferInfo = new ActivityResourceBufferInfo();
    }

    public ActivityBufferInfo(ActivityBufferInfo a_originalActivityBufferInfo)
    {
        EarliestJitBufferInfo = new ActivityResourceBufferInfo(a_originalActivityBufferInfo.EarliestJitBufferInfo);
        m_resourceInfos = a_originalActivityBufferInfo.m_resourceInfos.Clone();
    }

    public void Serialize(IWriter a_writer)
    {
        EarliestJitBufferInfo.Serialize(a_writer);
        a_writer.Write(m_resourceInfos.Count);
        foreach ((BaseId key, ActivityResourceBufferInfo value) in m_resourceInfos)
        {
            key.Serialize(a_writer);
            value.Serialize(a_writer);
        }
    }

    /// <summary>
    /// JIT is being recalculated, reset all values
    /// </summary>
    internal void Reset()
    {
        m_resourceInfos.Clear();
        EarliestJitBufferInfo = new ActivityResourceBufferInfo();
    }

    internal void Add(BaseId a_resId, ActivityResourceBufferInfo a_resourceBufferInfo)
    {
        m_resourceInfos.Add(a_resId, a_resourceBufferInfo);
    }

    /// <summary>
    /// Update an existing resource info if it was changed
    /// </summary>
    /// <param name="a_resId"></param>
    /// <param name="a_resourceBufferInfo"></param>
    internal void UpdateResourceInfo(BaseId a_resId, ActivityResourceBufferInfo a_resourceBufferInfo)
    {
        m_resourceInfos[a_resId] = a_resourceBufferInfo;
    }

    internal ActivityResourceBufferInfo GetResourceInfo(BaseId a_resId)
    {
        return m_resourceInfos[a_resId];
    }

    internal void FinalizeJitCalculations()
    {
        ActivityResourceBufferInfo earliestJitBufferInfo = new ();
        foreach (ActivityResourceBufferInfo info in m_resourceInfos.Values)
        {
            if (info.DbrJitDateCalculated)
            {
                if (!earliestJitBufferInfo.DbrJitDateCalculated || info.DbrJitStartDate < earliestJitBufferInfo.DbrJitStartDate)
                {
                    earliestJitBufferInfo = info;
                }
            }
        }

        if (earliestJitBufferInfo.DbrJitDateCalculated)
        {
            //Override the default empty info
            EarliestJitBufferInfo = earliestJitBufferInfo;
        }
    }

    internal ActivityResourceBufferInfo EarliestJitBufferInfo;

    public int UniqueId => 1135;

    internal bool AnyBufferSet()
    {
        return EarliestJitBufferInfo.DbrJitDateCalculated;
    }
}

internal class ActivityConstructorHelper
{
    internal static InternalActivity NewInternalActivity(IReader a_reader, int a_actUniqueId)
    {
        if (a_actUniqueId == InternalActivity.UNIQUE_ID)
        {
            return new InternalActivity(a_reader);
        }

        if (a_actUniqueId == TankActivity.UNIQUE_ID)
        {
            return new TankActivity(a_reader);
        }

        throw new Exception("Unknown activity type.".Localize());
    }
}

internal class TankActivity : InternalActivity
{
    #region Serialization
    internal TankActivity(IReader a_reader)
        : base(a_reader) { }

    public new const int UNIQUE_ID = 743;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    protected override ProductionInfo DeserializeProductionInfo(IReader a_reader)
    {
        ProductionInfo productionInfo = base.DeserializeProductionInfo(a_reader);
        if (a_reader.VersionNumber >= 408)
        {
            a_reader.Read(out long m_storagePostProcessingTicks);
            new BoolVector32(a_reader);
        }

        return productionInfo;
    }
}