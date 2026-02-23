using System.ComponentModel;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Attributes;
using PT.Common.Exceptions;
using PT.ERPTransmissions;
using PT.Scheduler.Demand;
using PT.Scheduler.Schedule;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Keeps track of the Items stored in a particular Warehouse.
/// </summary>
public partial class Inventory : BaseIdObject, ICloneable, IComparable<Inventory>
{
    public new const int UNIQUE_ID = 532;

    #region IPTSerializable Members
    public Inventory(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_reader)
    {
        m_idGen = a_idGen;
        m_restoreInfo = new RestoreInfo();

        Lots = new LotManager(a_idGen);
        m_simAdjustments = new (a_idGen);

        if (a_reader.VersionNumber >= 13003)
        {
            m_bools = new BoolVector32(a_reader);

            m_restoreInfo.ItemId = new BaseId(a_reader);
            a_reader.Read(out m_leadTimeTicks);
            a_reader.Read(out m_safetyStock);
            a_reader.Read(out m_bufferStock);
            a_reader.Read(out m_safetyStockWarningLevel);
            a_reader.Read(out m_plannerExternalId);
            a_reader.Read(out m_storageCapacity);
            a_reader.Read(out m_safetyStockJobPriority);
            a_reader.Read(out m_maxInventory);
            a_reader.Read(out m_mrpNotes);

            a_reader.Read(out int val);
            ForecastConsumption = (InventoryDefs.ForecastConsumptionMethods)val;
            a_reader.Read(out m_forecastConsumptionWindowDays);

            a_reader.Read(out val);
            m_mrpProcessing = (InventoryDefs.MrpProcessing)val;

            a_reader.Read(out bool hasForecastVersions);
            if (hasForecastVersions)
            {
                ForecastVersions = new ForecastVersions(a_reader, a_idGen);

                a_reader.Read(out int activeForecastVersionIdx);
                if (activeForecastVersionIdx > -1)
                {
                    ForecastVersionActive = ForecastVersions.Versions[activeForecastVersionIdx];
                }
            }

            a_reader.Read(out bool haveTemplateMo);
            if (haveTemplateMo)
            {
                m_restoreInfo.TemplateJobId = new BaseId(a_reader);
                m_restoreInfo.TemplateMoId = new BaseId(a_reader);
            }

            a_reader.Read(out bool haveImportedTemplateMoRef);
            if (haveImportedTemplateMoRef)
            {
                a_reader.Read(out string tempTemplateJobExernalId);

                m_templateMoImportRefInfo = new TemplateMoImportRefInfo(tempTemplateJobExernalId);
            }

            a_reader.Read(out m_lowLevelCode);
            a_reader.Read(out m_receivingBuffer);
            a_reader.Read(out m_shippingBuffer);

            Lots = new LotManager(a_reader, a_idGen);

            a_reader.Read(out m_autoGenerateForecasts);
            a_reader.Read(out val);
            m_forecastInterval = (DateInterval.EInterval)val;
            a_reader.Read(out m_numberOfIntervalsToForecast);
            a_reader.Read(out val);
            m_mrpExcessQuantityAllocation = (InventoryDefs.MrpExcessQuantityAllocation)val;

            a_reader.Read(out m_daysOnHandTicks);
            a_reader.Read(out m_replenishmentMin);
            a_reader.Read(out m_replenishmentMax);
            a_reader.Read(out m_replenishmentContractDays);
            a_reader.Read(out val);
            m_materialAllocation = (ItemDefs.MaterialAllocation)val;

            a_reader.Read(out bool hasPoStorageArea);
            if (hasPoStorageArea)
            {
                m_restoreInfo.PoStorageAreaId = new BaseId(a_reader);
            }

            m_adjustments = new (a_reader, a_idGen);
        }
        else
        {
            m_adjustments = new AdjustmentArray(a_idGen);

            if (a_reader.VersionNumber >= 12546)
            {
                m_bools = new BoolVector32(a_reader);

                m_restoreInfo.ItemId = new BaseId(a_reader);
                a_reader.Read(out m_leadTimeTicks);
                a_reader.Read(out m_safetyStock);
                a_reader.Read(out m_bufferStock);
                a_reader.Read(out m_safetyStockWarningLevel);
                a_reader.Read(out m_plannerExternalId);
                a_reader.Read(out m_storageCapacity);
                a_reader.Read(out m_safetyStockJobPriority);
                a_reader.Read(out m_maxInventory);
                a_reader.Read(out m_mrpNotes);

                a_reader.Read(out int val);
                ForecastConsumption = (InventoryDefs.ForecastConsumptionMethods)val;
                a_reader.Read(out m_forecastConsumptionWindowDays);

                a_reader.Read(out val);
                m_mrpProcessing = (InventoryDefs.MrpProcessing)val;

                a_reader.Read(out bool hasForecastVersions);
                if (hasForecastVersions)
                {
                    ForecastVersions = new ForecastVersions(a_reader, a_idGen);

                    a_reader.Read(out int activeForecastVersionIdx);
                    if (activeForecastVersionIdx > -1)
                    {
                        ForecastVersionActive = ForecastVersions.Versions[activeForecastVersionIdx];
                    }
                }

                a_reader.Read(out bool haveTemplateMo);
                if (haveTemplateMo)
                {
                    m_restoreInfo.TemplateJobId = new BaseId(a_reader);
                    m_restoreInfo.TemplateMoId = new BaseId(a_reader);
                }

                a_reader.Read(out bool haveImportedTemplateMoRef);
                if (haveImportedTemplateMoRef)
                {
                    a_reader.Read(out string tempTemplateJobExernalId);

                    m_templateMoImportRefInfo = new TemplateMoImportRefInfo(tempTemplateJobExernalId);
                }

                a_reader.Read(out m_lowLevelCode);
                a_reader.Read(out m_receivingBuffer);
                a_reader.Read(out m_shippingBuffer);

                Lots = new LotManager(a_reader, a_idGen);

                a_reader.Read(out m_autoGenerateForecasts);
                a_reader.Read(out val);
                m_forecastInterval = (DateInterval.EInterval)val;
                a_reader.Read(out m_numberOfIntervalsToForecast);
                a_reader.Read(out val);
                m_mrpExcessQuantityAllocation = (InventoryDefs.MrpExcessQuantityAllocation)val;

                a_reader.Read(out m_daysOnHandTicks);
                a_reader.Read(out m_replenishmentMin);
                a_reader.Read(out m_replenishmentMax);
                a_reader.Read(out m_replenishmentContractDays);
                a_reader.Read(out val);
                m_materialAllocation = (ItemDefs.MaterialAllocation)val;

                a_reader.Read(out bool hasPoStorageArea);
                if (hasPoStorageArea)
                {
                    m_restoreInfo.PoStorageAreaId = new BaseId(a_reader);
                }
            }
            else if (a_reader.VersionNumber >= 12511)
            {
                m_restoreInfo.ItemId = new BaseId(a_reader);
                a_reader.Read(out m_leadTimeTicks);
                a_reader.Read(out m_safetyStock);
                a_reader.Read(out m_bufferStock);
                a_reader.Read(out m_safetyStockWarningLevel);
                a_reader.Read(out m_plannerExternalId);
                a_reader.Read(out m_storageCapacity);
                a_reader.Read(out m_safetyStockJobPriority);
                a_reader.Read(out m_maxInventory);
                a_reader.Read(out m_mrpNotes);

                a_reader.Read(out int val);
                ForecastConsumption = (InventoryDefs.ForecastConsumptionMethods)val;
                a_reader.Read(out m_forecastConsumptionWindowDays);

                a_reader.Read(out val);
                if (val == 3) // SetJobNeedDatesOnly which was removed. Set to Ignore.
                {
                    val = 0;
                }

                m_mrpProcessing = (InventoryDefs.MrpProcessing)val;

                a_reader.Read(out bool hasForecastVersions);
                if (hasForecastVersions)
                {
                    ForecastVersions = new ForecastVersions(a_reader, a_idGen);

                    a_reader.Read(out int activeForecastVersionIdx);
                    if (activeForecastVersionIdx > -1)
                    {
                        ForecastVersionActive = ForecastVersions.Versions[activeForecastVersionIdx];
                    }
                }

                a_reader.Read(out bool haveTemplateMo);
                if (haveTemplateMo)
                {
                    m_restoreInfo.TemplateJobId = new BaseId(a_reader);
                    m_restoreInfo.TemplateMoId = new BaseId(a_reader);
                }

                a_reader.Read(out bool haveImportedTemplateMoRef);
                if (haveImportedTemplateMoRef)
                {
                    a_reader.Read(out string tempTemplateJobExernalId);

                    m_templateMoImportRefInfo = new TemplateMoImportRefInfo(tempTemplateJobExernalId);
                }

                m_bools = new BoolVector32(a_reader);
                a_reader.Read(out m_lowLevelCode);
                a_reader.Read(out m_receivingBuffer);
                a_reader.Read(out m_shippingBuffer);

                Lots = new LotManager(a_reader, a_idGen);

                a_reader.Read(out m_autoGenerateForecasts);
                a_reader.Read(out val);
                m_forecastInterval = (DateInterval.EInterval)val;
                a_reader.Read(out m_numberOfIntervalsToForecast);
                a_reader.Read(out val);
                m_mrpExcessQuantityAllocation = (InventoryDefs.MrpExcessQuantityAllocation)val;

                a_reader.Read(out m_daysOnHandTicks);
                a_reader.Read(out m_replenishmentMin);
                a_reader.Read(out m_replenishmentMax);
                a_reader.Read(out m_replenishmentContractDays);
                a_reader.Read(out val);
                m_materialAllocation = (ItemDefs.MaterialAllocation)val;
            }
        }

        //SimConstructor();
    }

    //For backwards compatibility from ItemInventory
    internal Inventory(IReader reader, BaseIdGenerator aIdGen, BaseId a_itemId)
        : base(reader)
    {
        m_restoreInfo = new RestoreInfo();
        m_restoreInfo.ItemId = a_itemId;
        m_idGen = aIdGen;
        m_adjustments = new AdjustmentArray(aIdGen);
        m_simAdjustments = new AdjustmentArray(aIdGen);

        Dictionary<BaseId, BaseId> obsoleteGeneratedLotIds = new ();

        if (reader.VersionNumber >= 12055) //Removed m_generatedLotIds
        {
            reader.Read(out m_leadTimeTicks);
            reader.Read(out decimal m_onHandQty);
            reader.Read(out m_safetyStock);
            reader.Read(out m_bufferStock);
            reader.Read(out m_safetyStockWarningLevel);
            reader.Read(out m_plannerExternalId);
            reader.Read(out m_storageCapacity);
            reader.Read(out m_safetyStockJobPriority);
            reader.Read(out m_maxInventory);
            reader.Read(out m_mrpNotes);

            int val;
            reader.Read(out val);
            ForecastConsumption = (InventoryDefs.ForecastConsumptionMethods)val;
            reader.Read(out m_forecastConsumptionWindowDays);

            reader.Read(out val);
            if (val == 3) // SetJobNeedDatesOnly which was removed. Set to Ignore.
            {
                val = 0;
            }

            m_mrpProcessing = (InventoryDefs.MrpProcessing)val;

            reader.Read(out bool hasForecastVersions);
            if (hasForecastVersions)
            {
                ForecastVersions = new ForecastVersions(reader, aIdGen);

                reader.Read(out int activeForecastVersionIdx);
                if (activeForecastVersionIdx > -1)
                {
                    ForecastVersionActive = ForecastVersions.Versions[activeForecastVersionIdx];
                }
            }

            reader.Read(out bool haveTemplateMo);
            if (haveTemplateMo)
            {
                m_restoreInfo.TemplateJobId = new BaseId(reader);
                m_restoreInfo.TemplateMoId = new BaseId(reader);
            }

            reader.Read(out bool haveImportedTemplateMoRef);
            if (haveImportedTemplateMoRef)
            {
                reader.Read(out string tempTemplateJobExernalId);

                m_templateMoImportRefInfo = new TemplateMoImportRefInfo(tempTemplateJobExernalId);
            }

            m_bools = new BoolVector32(reader);
            reader.Read(out m_lowLevelCode);
            reader.Read(out m_receivingBuffer);
            reader.Read(out m_shippingBuffer);

            Lots = new LotManager(reader, aIdGen, a_itemId);

            reader.Read(out m_autoGenerateForecasts);
            reader.Read(out val);
            m_forecastInterval = (DateInterval.EInterval)val;
            reader.Read(out m_numberOfIntervalsToForecast);
            reader.Read(out val);
            m_mrpExcessQuantityAllocation = (InventoryDefs.MrpExcessQuantityAllocation)val;

            reader.Read(out m_daysOnHandTicks);
            reader.Read(out m_replenishmentMin);
            reader.Read(out m_replenishmentMax);
            reader.Read(out m_replenishmentContractDays);
            reader.Read(out val);
            m_materialAllocation = (ItemDefs.MaterialAllocation)val;
        }
        else if (reader.VersionNumber >= 12000) //756 reader for v12 backwards compatibility
        {
            reader.Read(out m_leadTimeTicks);
            reader.Read(out decimal m_onHandQty);
            reader.Read(out m_safetyStock);
            reader.Read(out m_bufferStock);
            reader.Read(out m_safetyStockWarningLevel);
            reader.Read(out m_plannerExternalId);
            reader.Read(out m_storageCapacity);
            reader.Read(out m_safetyStockJobPriority);
            reader.Read(out m_maxInventory);
            reader.Read(out m_mrpNotes);

            int val;
            reader.Read(out val);
            ForecastConsumption = (InventoryDefs.ForecastConsumptionMethods)val;
            reader.Read(out m_forecastConsumptionWindowDays);

            reader.Read(out val);
            if (val == 3) // SetJobNeedDatesOnly which was removed. Set to Ignore.
            {
                val = 0;
            }

            m_mrpProcessing = (InventoryDefs.MrpProcessing)val;

            reader.Read(out bool hasForecastVersions);
            if (hasForecastVersions)
            {
                ForecastVersions = new ForecastVersions(reader, aIdGen);

                reader.Read(out int activeForecastVersionIdx);
                if (activeForecastVersionIdx > -1)
                {
                    ForecastVersionActive = ForecastVersions.Versions[activeForecastVersionIdx];
                }
            }

            reader.Read(out bool haveTemplateMo);
            if (haveTemplateMo)
            {
                m_restoreInfo.TemplateJobId = new BaseId(reader);
                m_restoreInfo.TemplateMoId = new BaseId(reader);
            }

            reader.Read(out bool haveImportedTemplateMoRef);
            if (haveImportedTemplateMoRef)
            {
                string tempTemplateJobExernalId;
                reader.Read(out tempTemplateJobExernalId);

                m_templateMoImportRefInfo = new TemplateMoImportRefInfo(tempTemplateJobExernalId);
            }

            m_bools = new BoolVector32(reader);
            reader.Read(out m_lowLevelCode);
            reader.Read(out m_receivingBuffer);
            reader.Read(out m_shippingBuffer);

            Lots = new LotManager(reader, aIdGen, a_itemId);

            reader.Read(out m_autoGenerateForecasts);
            reader.Read(out val);
            m_forecastInterval = (DateInterval.EInterval)val;
            reader.Read(out m_numberOfIntervalsToForecast);
            reader.Read(out val);
            m_mrpExcessQuantityAllocation = (InventoryDefs.MrpExcessQuantityAllocation)val;

            BaseIdClassFactory cf = new ();
            reader.Read(cf, out obsoleteGeneratedLotIds);
            reader.Read(out m_daysOnHandTicks);
            reader.Read(out m_replenishmentMin);
            reader.Read(out m_replenishmentMax);
            reader.Read(out m_replenishmentContractDays);
            reader.Read(out val);
            m_materialAllocation = (ItemDefs.MaterialAllocation)val;
        }
        else if (reader.VersionNumber >= 756) //Removed m_generatedLotIds
        {
            reader.Read(out m_leadTimeTicks);
            reader.Read(out decimal m_onHandQty);
            reader.Read(out m_safetyStock);
            reader.Read(out m_bufferStock);
            reader.Read(out m_safetyStockWarningLevel);
            reader.Read(out m_plannerExternalId);
            reader.Read(out m_storageCapacity);
            reader.Read(out m_safetyStockJobPriority);
            reader.Read(out m_maxInventory);
            reader.Read(out m_mrpNotes);

            int val;
            reader.Read(out val);
            ForecastConsumption = (InventoryDefs.ForecastConsumptionMethods)val;
            reader.Read(out m_forecastConsumptionWindowDays);

            reader.Read(out val);
            if (val == 3) // SetJobNeedDatesOnly which was removed. Set to Ignore.
            {
                val = 0;
            }

            m_mrpProcessing = (InventoryDefs.MrpProcessing)val;

            bool hasForecastVersions;
            reader.Read(out hasForecastVersions);
            if (hasForecastVersions)
            {
                ForecastVersions = new ForecastVersions(reader, aIdGen);

                int activeForecastVersionIdx;
                reader.Read(out activeForecastVersionIdx);
                if (activeForecastVersionIdx > -1)
                {
                    ForecastVersionActive = ForecastVersions.Versions[activeForecastVersionIdx];
                }
            }

            bool haveTemplateMo;
            reader.Read(out haveTemplateMo);
            if (haveTemplateMo)
            {
                m_restoreInfo.TemplateJobId = new BaseId(reader);
                m_restoreInfo.TemplateMoId = new BaseId(reader);
            }

            bool haveImportedTemplateMoRef;
            reader.Read(out haveImportedTemplateMoRef);
            if (haveImportedTemplateMoRef)
            {
                string tempTemplateJobExernalId;
                reader.Read(out tempTemplateJobExernalId);

                m_templateMoImportRefInfo = new TemplateMoImportRefInfo(tempTemplateJobExernalId);
            }

            m_bools = new BoolVector32(reader);
            reader.Read(out m_lowLevelCode);
            reader.Read(out m_receivingBuffer);
            reader.Read(out m_shippingBuffer);

            Lots = new LotManager(reader, aIdGen, a_itemId);

            reader.Read(out m_autoGenerateForecasts);
            reader.Read(out val);
            m_forecastInterval = (DateInterval.EInterval)val;
            reader.Read(out m_numberOfIntervalsToForecast);
            reader.Read(out val);
            m_mrpExcessQuantityAllocation = (InventoryDefs.MrpExcessQuantityAllocation)val;

            reader.Read(out m_daysOnHandTicks);
            reader.Read(out m_replenishmentMin);
            reader.Read(out m_replenishmentMax);
            reader.Read(out m_replenishmentContractDays);
            reader.Read(out val);
            m_materialAllocation = (ItemDefs.MaterialAllocation)val;
        }
        else if (reader.VersionNumber >= 731)
        {
            reader.Read(out m_leadTimeTicks);
            reader.Read(out decimal m_onHandQty);
            reader.Read(out m_safetyStock);
            reader.Read(out m_bufferStock);
            reader.Read(out m_safetyStockWarningLevel);
            reader.Read(out m_plannerExternalId);
            reader.Read(out m_storageCapacity);
            reader.Read(out m_safetyStockJobPriority);
            reader.Read(out m_maxInventory);
            reader.Read(out m_mrpNotes);

            int val;
            reader.Read(out val);
            ForecastConsumption = (InventoryDefs.ForecastConsumptionMethods)val;
            reader.Read(out m_forecastConsumptionWindowDays);

            reader.Read(out val);
            if (val == 3) // SetJobNeedDatesOnly which was removed. Set to Ignore.
            {
                val = 0;
            }

            m_mrpProcessing = (InventoryDefs.MrpProcessing)val;

            reader.Read(out bool hasForecastVersions);
            if (hasForecastVersions)
            {
                ForecastVersions = new ForecastVersions(reader, aIdGen);

                reader.Read(out int activeForecastVersionIdx);
                if (activeForecastVersionIdx > -1)
                {
                    ForecastVersionActive = ForecastVersions.Versions[activeForecastVersionIdx];
                }
            }

            reader.Read(out bool haveTemplateMo);
            if (haveTemplateMo)
            {
                m_restoreInfo.TemplateJobId = new BaseId(reader);
                m_restoreInfo.TemplateMoId = new BaseId(reader);
            }

            reader.Read(out bool haveImportedTemplateMoRef);
            if (haveImportedTemplateMoRef)
            {
                string tempTemplateJobExernalId;
                reader.Read(out tempTemplateJobExernalId);

                m_templateMoImportRefInfo = new TemplateMoImportRefInfo(tempTemplateJobExernalId);
            }

            m_bools = new BoolVector32(reader);
            reader.Read(out m_lowLevelCode);
            reader.Read(out m_receivingBuffer);
            reader.Read(out m_shippingBuffer);

            Lots = new LotManager(reader, aIdGen, a_itemId);

            reader.Read(out m_autoGenerateForecasts);
            reader.Read(out val);
            m_forecastInterval = (DateInterval.EInterval)val;
            reader.Read(out m_numberOfIntervalsToForecast);
            reader.Read(out val);
            m_mrpExcessQuantityAllocation = (InventoryDefs.MrpExcessQuantityAllocation)val;

            BaseIdClassFactory cf = new ();
            reader.Read(cf, out obsoleteGeneratedLotIds);
            reader.Read(out m_daysOnHandTicks);
            reader.Read(out m_replenishmentMin);
            reader.Read(out m_replenishmentMax);
            reader.Read(out m_replenishmentContractDays);
            reader.Read(out val);
            m_materialAllocation = (ItemDefs.MaterialAllocation)val;
        }

        //SimConstructor();
    }

    private class RestoreInfo
    {
        internal BaseId ItemId = BaseId.NULL_ID;
        internal BaseId PoStorageAreaId = BaseId.NULL_ID;
        internal BaseId TemplateJobId = BaseId.NULL_ID;
        internal BaseId TemplateMoId = BaseId.NULL_ID;
    }

    private readonly RestoreInfo m_restoreInfo;

    internal void RestoreReferences(CustomerManager a_cm, ItemManager a_itemManager, Warehouse a_wh, ISystemLogger a_errorReporter, ref Dictionary<BaseId, ItemStorage> a_itemStorageCollection)
    {
        m_item = a_itemManager.GetById(m_restoreInfo.ItemId);
        m_warehouse = a_wh;

        Lots.RestoreReferences(a_itemManager, a_wh, a_errorReporter, m_item, ref a_itemStorageCollection);

        m_forecastVersions?.RestoreReferences(a_cm, this);

        if (m_restoreInfo.PoStorageAreaId != BaseId.NULL_ID)
        {
            m_purchaseOrderSupplyStorageArea = m_warehouse.StorageAreas.GetValue(m_restoreInfo.PoStorageAreaId);
        }
    }

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        if (m_forecastVersions != null)
        {
            m_forecastVersions.RestoreReferences(a_udfManager);
        }
    }

    internal void RestoreTemplateMoReference(JobManager a_jobManager)
    {
        if (m_restoreInfo.TemplateJobId != BaseId.NULL_ID)
        {
            Job job = a_jobManager.GetById(m_restoreInfo.TemplateJobId); //should always be there so no need to check
            ManufacturingOrder mo = job.ManufacturingOrders.GetById(m_restoreInfo.TemplateMoId);
            m_templateMO = mo;
            mo.UsedAsTemplateForInventory(this);
        }
    }

    internal void RestoreAdjustmentReferences(ScenarioDetail a_sd)
    {
        foreach (Lot lot in Lots)
        {
            lot.RestoreAdjustmentReferences(a_sd);
        }

        m_adjustments.RestoreReferences(a_sd);
    }
    
    //We need to sort these so the UI can immediately access them without threading issues caused by Sort
    internal void AfterRestoreAdjustmentReferences()
    {
        foreach (Lot lot in Lots)
        {
            lot.AfterRestoreAdjustmentReferences();
        }
        
        m_adjustments.Sort();
        m_simAdjustments.Sort();
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_item.Id.Serialize(a_writer);
        a_writer.Write(m_leadTimeTicks);
        a_writer.Write(m_safetyStock);
        a_writer.Write(m_bufferStock);
        a_writer.Write(m_safetyStockWarningLevel);
        a_writer.Write(m_plannerExternalId);
        a_writer.Write(m_storageCapacity);
        a_writer.Write(m_safetyStockJobPriority);
        a_writer.Write(m_maxInventory);
        a_writer.Write(m_mrpNotes);

        a_writer.Write((int)ForecastConsumption);
        a_writer.Write(m_forecastConsumptionWindowDays);
        a_writer.Write((int)m_mrpProcessing);

        bool hasForecastVersions = ForecastVersions != null;
        a_writer.Write(hasForecastVersions);
        if (hasForecastVersions)
        {
            ForecastVersions.Serialize(a_writer);

            int activeForecastVersionIdx;

            if (ForecastVersionActive != null)
            {
                activeForecastVersionIdx = ForecastVersions.Versions.IndexOf(ForecastVersionActive);
            }
            else
            {
                activeForecastVersionIdx = -1;
            }

            a_writer.Write(activeForecastVersionIdx);
        }

        a_writer.Write(TemplateManufacturingOrder != null);
        if (TemplateManufacturingOrder != null)
        {
            TemplateManufacturingOrder.Job.Id.Serialize(a_writer);
            TemplateManufacturingOrder.Id.Serialize(a_writer);
        }

        a_writer.Write(TemplateMoImportRef != null);
        if (TemplateMoImportRef != null)
        {
            a_writer.Write(TemplateMoImportRef.m_templateJobExternalId);
        }

        a_writer.Write(m_lowLevelCode);
        a_writer.Write(m_receivingBuffer);
        a_writer.Write(m_shippingBuffer);

        Lots.Serialize(a_writer);

        a_writer.Write(m_autoGenerateForecasts);
        a_writer.Write((int)m_forecastInterval);
        a_writer.Write(m_numberOfIntervalsToForecast);
        a_writer.Write((int)m_mrpExcessQuantityAllocation);
        a_writer.Write(m_daysOnHandTicks);
        a_writer.Write(m_replenishmentMin);
        a_writer.Write(m_replenishmentMax);
        a_writer.Write(m_replenishmentContractDays);
        a_writer.Write((int)m_materialAllocation);

        a_writer.Write(PurchaseOrderSupplyStorageArea != null);
        PurchaseOrderSupplyStorageArea?.Id.Serialize(a_writer);

        m_adjustments.Serialize(a_writer);
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region References
    private Item m_item;

    [DoNotAuditProperty]
    /// <summary>
    /// The Item whose stock is being stored.
    /// </summary>
    public Item Item => m_item;

    private Warehouse m_warehouse;

    [DoNotAuditProperty]
    public Warehouse Warehouse => m_warehouse;


    private BaseIdGenerator m_idGen;

    #endregion

    #region Declarations
    public class InventoryException : PTException
    {
        public InventoryException(string message)
            : base(message) { }
    }
    #endregion

    #region Construction
    /// <summary>
    /// For Tanks maybe removed when they reworked
    /// </summary>
    /// <param name="a_id"></param>
    /// <param name="a_warehouse"></param>
    /// <param name="a_sm"></param>
    /// <param name="a_item"></param>
    /// <param name="a_inventoryNode"></param>
    /// <param name="a_idGen"></param>
    /// <param name="a_udfManager"></param>
    internal Inventory(BaseId a_id, Warehouse a_warehouse, ScenarioDetail a_sd, Item a_item, WarehouseT.Inventory a_inventoryNode,  BaseIdGenerator a_idGen, UserFieldDefinitionManager a_udfManager)
        : base(a_id)
    {
        m_idGen = a_idGen;
        m_warehouse = a_warehouse;
        Lots = new LotManager(a_idGen);
        m_item = a_item;
        m_adjustments = new AdjustmentArray(a_idGen);
        m_simAdjustments = new AdjustmentArray(a_idGen);
        Update(a_inventoryNode, a_sd, a_udfManager);
        //SimConstructor();
    }

    internal Inventory(BaseId aId, Warehouse aWarehouse, Item item, BaseIdGenerator a_idGen)
        : base(aId)
    {
        m_idGen = a_idGen;
        m_warehouse = aWarehouse;
        Lots = new LotManager(a_idGen);
        m_item = item;
        m_adjustments = new AdjustmentArray(a_idGen);
        m_simAdjustments = new AdjustmentArray(a_idGen);
        //SimConstructor();
    }
    #endregion

    private BoolVector32 m_bools;
    private const int IncludeInNetChangeMRPIdx = 0;
    private const int c_salesOrdersChangedSinceLastForecastIdx = 1;
    private const int c_preventSharedBatchOverflowIdx = 2;

    #region Shared Properties
    private long m_leadTimeTicks;

    /// <summary>
    /// The amount of time it takes between when the Item is ordered and delivered for this Warehouse.
    /// </summary>
    public TimeSpan LeadTime
    {
        get => TimeSpan.FromTicks(m_leadTimeTicks);
        private set => m_leadTimeTicks = value.Ticks;
    }
    [DoNotAuditProperty]
    internal long LeadTimeTicks => m_leadTimeTicks;

    private decimal m_safetyStock;

    /// <summary>
    /// The target level of this Item to have on hand at this Warehouse to provide the desired service level to customers.
    /// </summary>
    public decimal SafetyStock
    {
        get => m_safetyStock;
        private set => m_safetyStock = value;
    }

    private decimal m_replenishmentMin;

    /// <summary>
    /// Inventory level that causes MRP to generate a replenishment order.
    /// </summary>
    public decimal ReplenishmentMin
    {
        get => m_replenishmentMin;
        private set => m_replenishmentMin = value;
    }

    private decimal m_replenishmentMax;

    /// <summary>
    /// When creating replenishments, generate enough supplies to reach at least this level of inventory.
    /// </summary>
    public decimal ReplenishmentMax
    {
        get => m_replenishmentMax;
        private set => m_replenishmentMax = value;
    }

    private double m_replenishmentContractDays;

    /// <summary>
    /// Number of days it is the Ok to be below replenishment reorder level.
    /// </summary>
    public double ReplenishmentContractDays
    {
        get => m_replenishmentContractDays;
        private set => m_replenishmentContractDays = value;
    }

    private decimal m_bufferStock;

    /// <summary>
    /// The target level of this Item to have on hand at this Warehouse to support Make-to-Available inventory management.
    /// This value is used to calculate Buffer Stock penetration levels.
    /// </summary>
    public decimal BufferStock
    {
        get => m_bufferStock;
        private set => m_bufferStock = value;
    }

    /// <summary>
    /// On-Hand inventory above the Safety Stock.
    /// </summary>
    private decimal ExcessInventory => OnHandQty - SafetyStock;

    private decimal m_safetyStockWarningLevel;

    /// <summary>
    /// If On-Hand inventory falls at or below this amount then Safety Stock Jobs may be flagged as Late.  The purpose of this is to only highlight late safety stock Jobs when the safety stock is
    /// significantly short.
    /// This controls the "Late" setting for safety stock Jobs only.
    /// </summary>
    public decimal SafetyStockWarningLevel
    {
        get => m_safetyStockWarningLevel;
        private set => m_safetyStockWarningLevel = value;
    }

    private decimal m_maxInventory;

    /// <summary>
    /// The maximum amount of inventory that can be created and stored.  This can be used to alert the planner when there will be too much inventory created and it will occupy too much space or consume too
    /// much capital.
    /// </summary>
    public decimal MaxInventory
    {
        get => m_maxInventory;
        private set => m_maxInventory = value;
    }

    /// <summary>
    /// The amount of physical inventory in stock for this Item in this Warehouse.
    /// </summary>
    public decimal OnHandQty
    {
        get
        {
            //TODO: Cache this, but it needs to be cleared on any lot update, not just simulation
            //if (m_onHandInvCache != null && m_onHandInvCache.TryGetValue(out decimal cacheOnHand))
            //{
            //    return cacheOnHand;
            //}

            decimal onHand = 0;
            foreach (Lot lot in m_lots)
            {
                if (lot.LotSource is ItemDefs.ELotSource.Inventory or ItemDefs.ELotSource.OnHand)
                {
                    onHand += lot.Qty;
                }
            }

            //m_onHandInvCache?.CacheValue(onHand);

            return onHand;
        }
    }

    internal void AddOnHandQty(decimal a_qty)
    {
        Lots.GetOnHandLot(this).ProduceToLot(a_qty);
    }

    internal void SetOnHandQty(decimal a_qty)
    {
        decimal newQty = Math.Max(0m, a_qty);

        Lots.SetOnHandQty(newQty, this);
    }

    internal void SubtractOnHandQty(decimal a_qty)
    {
        SetOnHandQty(Math.Max(0, OnHandQty - a_qty));
    }

    private decimal m_storageCapacity;

    /// <summary>
    /// Can be used by a Scheduling Add-In to limit scheduling based upon availability of storage space.
    /// </summary>
    public decimal StorageCapacity
    {
        get => m_storageCapacity;
        private set => m_storageCapacity = value;
    }

    private string m_plannerExternalId = "";

    /// <summary>
    /// The External Id of the Planner responsible for monitoring and managing this Items stock level at this Warehouse.
    /// </summary>
    public string PlannerExternalId
    {
        get => m_plannerExternalId;
        private set => m_plannerExternalId = value;
    }

    private int m_safetyStockJobPriority = 5;

    /// <summary>
    /// Sets the Priority for Jobs that are generated to satisfy Safety Stock.
    /// </summary>
    public int SafetyStockJobPriority
    {
        get => m_safetyStockJobPriority;
        private set => m_safetyStockJobPriority = value;
    }

    private InventoryDefs.MrpProcessing m_mrpProcessing;

    /// <summary>
    /// Specifies how the MRP process will manage this inventory.
    /// </summary>
    public InventoryDefs.MrpProcessing MrpProcessing
    {
        get => m_mrpProcessing;
        private set => m_mrpProcessing = value;
    }

    private InventoryDefs.MrpExcessQuantityAllocation m_mrpExcessQuantityAllocation;

    public InventoryDefs.MrpExcessQuantityAllocation MrpExcessQuantityAllocation
    {
        get => m_mrpExcessQuantityAllocation;
        private set => m_mrpExcessQuantityAllocation = value;
    }

    private TimeSpan m_receivingBuffer;

    /// <summary>
    /// Purchased Material should be received this amount of time before the P.O. Scheduled Receipt Date.
    /// </summary>
    public TimeSpan ReceivingBuffer
    {
        get => m_receivingBuffer;
        private set => m_receivingBuffer = value;
    }

    private long m_shippingBuffer;
    [DoNotAuditProperty]
    internal long ShippingBufferTicks
    {
        get => m_shippingBuffer;
        private set => m_shippingBuffer = value;
    }

    /// <summary>
    /// Production should be completed this amount of time before the M.O. Need Date.
    /// </summary>
    public TimeSpan ShippingBuffer => new (ShippingBufferTicks);

    /// <summary>
    /// Indicates how far short of the target Buffer Stock the current On-hand Qty is. Larger values indicate that there is greater risk of shortage.
    /// Calculated as: (BufferStock-OnHand)/BufferStock * 100. 0 if Inventory.BufferStock=0.
    /// </summary>
    public decimal BufferPenetrationPercent
    {
        get
        {
            if (BufferStock == 0)
            {
                return 0;
            }

            return (BufferStock - OnHandQty) / BufferStock * 100;
        }
    }

    /// <summary>
    /// OK (Less than 33% Penetration), Warning (33 to less than 66%) or Critical (66% or greater) based on InventoryBufferStockPenetrationPercent
    /// </summary>
    public BufferDefs.WarningLevels BufferWarningLevel => BufferDefs.GetBufferWarningFromPercent(BufferPenetrationPercent);

    private LotManager m_lots;
    [DoNotAuditProperty]
    public LotManager Lots
    {
        get => m_lots;
        private set => m_lots = value;
    }

    internal void ClearLots(JobManager a_jobManager, PurchaseToStockManager a_purchaseToStockManager, IScenarioDataChanges a_dataChanges)
    {
        m_lots.ClearLots(a_jobManager, a_purchaseToStockManager, a_dataChanges);
    }

    [DebugLogging(EDebugLoggingType.None)] private decimal m_maxInventoryLevel;

    [DebugLogging(EDebugLoggingType.None)] private decimal m_minInventoryLevel;

    private long m_daysOnHandTicks;
    [DoNotAuditProperty]
    internal long DaysOnHandTicks
    {
        get => m_daysOnHandTicks;
        private set => m_daysOnHandTicks = value;
    }

    /// <summary>
    /// The number of days inventory must be able to fullfill
    /// demand before the optimize rule's DaysOnHand weight begins
    /// preferring jobs that produce this item.
    /// Allows you to prefer jobs that produce this type of item.
    /// When you give weight to the DaysOnHand optimize rule,
    /// jobs that produce the same item as this inventory will
    /// be slightly more preferred if starting from the simulation clock
    /// to this many days out there isn't enough material
    /// to fullfill demands (sales orders and forecasts).
    /// [DaysOnHand:CalcDaysOnHand;1]
    /// </summary>
    public TimeSpan DaysOnHand => new (m_daysOnHandTicks);

    private ItemDefs.MaterialAllocation m_materialAllocation = ItemDefs.MaterialAllocation.NotSet; // 2020.11.04: UseOldestFirst was the default prior to the change for task 12010 for a specific customer. It has been chosen again as the default because other than the one customer, no one had ever had an issue with it before. 

    /// <summary>
    /// How material should be used either use the earliest created or the latest created.
    /// </summary>
    public ItemDefs.MaterialAllocation MaterialAllocation
    {
        get => m_materialAllocation;
        private set => m_materialAllocation = value;
    }

    /// <summary>
    /// Calculates max inventory level and min inventory level.
    /// </summary>
    private void CalculateMinMaxInventoryLevel()
    {
        decimal currentInventory = OnHandQty;
        //Filter out all of the forecast adjustments
        List<Adjustment> adjustments = new (Adjustments.Count);
        for (int i = 0; i < Adjustments.Count; i++)
        {
            Adjustment a = Adjustments[i];
            //TODO: SA
            if (/*a.Reason is ForecastShipment ||*/ a is ActivityAdjustment actAdj && actAdj.IsJobForecast())
            {
                continue;
            }

            adjustments.Add(a);
        }

        //Sort the adjustments
        adjustments = adjustments.OrderBy(a => a.AdjDate).ToList();

        for (int i = 0; i < adjustments.Count; i++)
        {
            Adjustment adjustment = adjustments[i];
            currentInventory += adjustment.ChangeQty;
            if (i < adjustments.Count - 1)
            {
                if (adjustments[i + 1].AdjDate == adjustment.AdjDate)
                {
                    //Don't check min/max yet, there is another adjustment at the same time.
                    continue;
                }
            }

            if (currentInventory < m_minInventoryLevel)
            {
                m_minInventoryLevel = currentInventory;
            }

            if (currentInventory > m_maxInventoryLevel)
            {
                m_maxInventoryLevel = currentInventory;
            }
        }

        m_inventoryLevelCalculated = true;
    }

    private bool m_inventoryLevelCalculated;

    /// <summary>
    /// Holds a value for the maximum inventory level that occurred in an adjustment period.
    /// </summary>
    public decimal MaxInventoryLevel()
    {
        if (!m_inventoryLevelCalculated)
        {
            m_maxInventoryLevel = 0;
            m_minInventoryLevel = 0;
            CalculateMinMaxInventoryLevel();
        }

        return m_maxInventoryLevel;
    }

    /// <summary>
    /// Holds a value for the minimum inventory level that occured in an adjustment period.
    /// </summary>
    public decimal MinInventoryLevel()
    {
        if (!m_inventoryLevelCalculated)
        {
            m_maxInventoryLevel = 0;
            m_minInventoryLevel = 0;
            CalculateMinMaxInventoryLevel();
        }

        return m_minInventoryLevel;
    }

    public decimal DaysOfStock(ScenarioDetail a_sd)
    {
        DateTime shortageDate;
        decimal shortageAmount;
        bool shortageWithinLeadTime;
        bool shortage = GetFirstShortage(out shortageDate, out shortageAmount, a_sd, true, out shortageWithinLeadTime);
        if (shortage)
        {
            return (decimal)(shortageDate - a_sd.ClockDate).TotalDays;
        }

        return (decimal)(a_sd.GetPlanningHorizonEnd() - a_sd.ClockDate).TotalDays;
    }
    #endregion Shared Properties

    #region ATP
    /// <summary>
    /// Returns the first date at which there is available material based on the currently schedule demands
    /// </summary>
    public DateTime GetAvailableDate(decimal a_qtyRequired, DateTime a_minDateLimit, DateTime a_dateLimit, ScenarioDetail a_sd)
    {
        AdjustmentArray reqAdjs = GetRequirementAdjustments(a_sd);
        CondensedAdjustmentArray condensedAdjustments = reqAdjs.Condensed(this, true, true, a_sd.ClockDate);
        if (condensedAdjustments.Count == 0)
        {
            return DateTime.MaxValue;
        }

        decimal qtyBeforeMin = 0;
        bool checkQtyBeforeMin = false;

        for (int i = 0; i < condensedAdjustments.Count; i++)
        {
            CondensedAdjustment adj = condensedAdjustments[i];

            if (adj.AdjDate < a_minDateLimit)
            {
                qtyBeforeMin = adj.OnHandQty;
                checkQtyBeforeMin = true;
                continue;
            }

            //First adjustment after min time may already have available qty.
            if (checkQtyBeforeMin && qtyBeforeMin >= a_qtyRequired)
            {
                return a_minDateLimit;
            }

            checkQtyBeforeMin = false;

            if (adj.AdjDate > a_dateLimit)
            {
                return DateTime.MaxValue;
            }

            if (adj.OnHandQty >= a_qtyRequired)
            {
                return adj.AdjDate;
            }
        }

        return DateTime.MaxValue;
    }

    public DateTime GetAvailableDate(decimal a_qtyRequired, DateTime a_minDateLimit, DateTime a_dateLimit, HashSet<string> a_allowedLotCodes, ScenarioDetail a_sd)
    {
        if (Adjustments.Count == 0)
        {
            return DateTime.MaxValue;
        }

        decimal qtyBeforeMin = 0;
        bool checkQtyBeforeMin = false;

        AdjustmentArray reqAdjs = GetRequirementAdjustments(a_sd);
        for (int i = 0; i < reqAdjs.Count; i++)
        {
            Adjustment adj = reqAdjs[i];
            if (adj.AdjDate > a_dateLimit)
            {
                return DateTime.MaxValue;
            }

            if (!adj.HasLotStorage)
            {
                continue;
            }

            Lot lot = adj.Storage.Lot;

            if (adj.AdjDate < a_minDateLimit)
            {
                qtyBeforeMin = lot.Qty;
                checkQtyBeforeMin = true;
                continue;
            }

            //First adjustment after min time may already have available qty.
            if (checkQtyBeforeMin && qtyBeforeMin >= a_qtyRequired)
            {
                return a_minDateLimit;
            }

            checkQtyBeforeMin = false;

            if (adj.Qty >= a_qtyRequired)
            {
                return new DateTime(lot.ProductionTicks + lot.MaterialPostProcessingTicks);
            }
        }

        return DateTime.MaxValue;
    }

    /// <summary>
    /// Available to Promise inventory
    /// Returns the amount of inventory that can be promissed to a new demand without affecting existing commitments.		///
    /// </summary>
    /// <param name="asOf">The ATP qty is calculated as the minimum projected inventory level from this date out into the future.</param>
    /// <returns></returns>
    public decimal GetAtpQty(DateTime asOf, DateTime a_clockDate)
    {
        return GetAtpQtyHelper(asOf, Adjustments, a_clockDate);
    }

    private decimal GetAtpQtyHelper(DateTime a_dateTime, AdjustmentArray a_adjustments, DateTime a_clockDate)
    {
        decimal atp = decimal.MaxValue;
        decimal asOfOnHand = OnHandQty; //the on hand qty at the as of date.  To be determined.

        CondensedAdjustmentArray condensedAdjustments = a_adjustments.Condensed(this, true, true, a_clockDate);
        if (condensedAdjustments.Count == 0)
        {
            return 0m;
        }

        //Get the minimum value from the date specified working from the end backwards for performance purposes
        for (int i = condensedAdjustments.Count - 1; i >= 0; i--)
        {
            CondensedAdjustment adj = condensedAdjustments[i];
            //if we're before the asOf date then get the onhand qty.  That is the onhand at the asof date.
            if (adj.Time < a_dateTime.Ticks)
            {
                asOfOnHand = adj.OnHandQty;
                break;
            }

            if (adj.OnHandQty < atp)
            {
                atp = adj.OnHandQty;
            }
        }

        return Math.Max(Math.Min(asOfOnHand, atp), 0);
    }

    /// <summary>
    /// Similar to GetAtpQty but this doesn't include adjustments that are for a specific lot code in the calculation.
    /// </summary>
    /// <param name="a_date"></param>
    /// <returns></returns>
    public decimal GetAtpQtyNotInLots(DateTime a_date, ScenarioDetail a_sd)
    {
        AdjustmentArray nonLotAdjs = new ();
        AdjustmentArray reqAdjs = GetRequirementAdjustments(a_sd);
        for (int i = 0; i < reqAdjs.Count; i++)
        {
            Adjustment adj = reqAdjs[i];
            bool hasLot = false;
            if (adj is PurchaseOrderAdjustment poAdj)
            {
                hasLot = !string.IsNullOrEmpty(poAdj.PurchaseOrder.LotCode);
            }
            //TODO: SA
            //else if (adj.Reason is SalesOrderLineDistribution sod)
            //{
            //    hasLot = sod.EligibleLots.Count > 0;
            //}
            //else if (adj.Reason is TransferOrderDistribution tod)
            //{
            //    hasLot = !string.IsNullOrEmpty(tod.LotCode);
            //}
            else if (adj is ActivityAdjustment actAdj)
            {
                if (adj.ChangeQty < 0)
                {
                    MaterialRequirement mr = actAdj.GetAdjMaterialRequirement(Item.Id);
                    hasLot = mr != null && mr.GetEligibleLotCount() > 0;
                }
                else
                {
                    Product product = actAdj.GetAdjProduct(Item.Id);
                    hasLot = product != null && !string.IsNullOrEmpty(product.LotCode);
                }
            }

            if (!hasLot)
            {
                nonLotAdjs.Add(adj);
            }
        }

        return GetAtpQtyHelper(a_date, nonLotAdjs, a_sd.ClockDate);
    }

    /// <summary>
    /// Returns a list with the same number of elements as in the parameter specifying the ATP as of the date in the element.
    /// </summary>
    /// <param name="serverAsOfDates">List of dates pre-sorted in increasing date order</param>
    /// <returns></returns>
    public List<decimal> GetAtpByAsOfDates(List<DateTime> serverAsOfDates, ScenarioDetail sd)
    {
        decimal asOfOnHand = decimal.MaxValue;
        List<decimal> onHandList = new ();

        CondensedAdjustmentArray condensedAdjustments = Adjustments.Condensed(this, true, true, sd.ClockDate);
        int curAsOfIdx = serverAsOfDates.Count - 1;
        CondensedAdjustment adj = null;

        for (int i = condensedAdjustments.Count - 1; i >= 0; i--)
        {
            adj = condensedAdjustments[i];
            if (adj.OnHandQty < asOfOnHand)
            {
                asOfOnHand = adj.OnHandQty;
            }

            while (curAsOfIdx >= 0 && curAsOfIdx < serverAsOfDates.Count && serverAsOfDates[curAsOfIdx].Ticks >= adj.AdjDate.Ticks)
            {
                onHandList.Add(asOfOnHand);
                curAsOfIdx--;
            }
        }

        //For the rest of the dates, if any, later than the Clock, assign the current On-Hand
        while (curAsOfIdx >= 0 && curAsOfIdx < serverAsOfDates.Count)
        {
            if (serverAsOfDates[curAsOfIdx].Ticks >= sd.Clock)
            {
                onHandList.Add(OnHandQty);
            }
            else
            {
                onHandList.Add(0m); //in the past
            }

            curAsOfIdx--;
        }

        //Now reverse the list of values so that they are returned in a date/time sequemce that matches the inlist
        List<decimal> outlist = new ();
        for (int i = onHandList.Count - 1; i >= 0; i--)
        {
            outlist.Add(onHandList[i]);
        }

        return outlist;
    }

    public decimal GetProjectedInventoryAtDate(DateTime dt, ScenarioDetail a_sd)
    {
        decimal projectedInv = OnHandQty;
        AdjustmentArray reqAdjs = GetRequirementAdjustments(a_sd);
        for (int i = 0; i < reqAdjs.Count; i++)
        {
            Adjustment adj = reqAdjs[i];
            if (adj.AdjDate.Ticks >= dt.Ticks)
            {
                break;
            }

            projectedInv += adj.ChangeQty;
        }

        return projectedInv;
    }

    public ActivityKeyList FindAllSupplyingActivityInWindow(DateTime a_windowStartInclusive, DateTime a_windowEndExclusive, out List<InternalActivity> o_activities)
    {
        ActivityKeyList list = new ();
        o_activities = new List<InternalActivity>();

        AdjustmentArray adjustments = GetAdjustmentArray();
        for (int i = 0; i < adjustments.Count; i++)
        {
            Adjustment adj = adjustments[i];
            if (adj.AdjDate.Ticks >= a_windowStartInclusive.Ticks && adj.AdjDate.Ticks < a_windowEndExclusive.Ticks && adj.ChangeQty > 0)
            {
                if (adj is ActivityAdjustment actAdj)
                {
                    InternalActivity activity = actAdj.Activity;
                    list.Add(new ActivityKey(activity.Job.Id, activity.ManufacturingOrder.Id, activity.Operation.Id, activity.Id));
                    o_activities.Add(activity);
                }
            }
        }

        return list;
    }

    /// <summary>
    /// Checks the Inventory's Adjustment Array for production from an Activity.
    /// </summary>
    /// <param name="windowStartInclusive">Start of the window.</param>
    /// <param name="windowEndExclusive">End of the window.</param>
    /// <returns>The first Internal Activity producing in this window.  Returns null if none is found.</returns>
    public InternalActivity FindLastSupplyingActivityInWindow(DateTime windowStartInclusive, DateTime windowEndExclusive)
    {
        AdjustmentArray adjustments = GetAdjustmentArray();
        InternalActivity activity = null;
        for (int i = adjustments.Count - 1; i >= 0; i--)
        {
            Adjustment adj = adjustments[i];
            if (adj.AdjDate.Ticks >= windowStartInclusive.Ticks && adj.AdjDate.Ticks < windowEndExclusive.Ticks && adj.ChangeQty > 0)
            {
                if (adj is ActivityAdjustment actAdj)
                {
                    //found the first Activity.
                    activity = actAdj.Activity;
                    break;
                }
            }
        }

        return activity;
    }

    public List<InternalActivity> GetActivitiesProducingOrConsumingInventory()
    {
        AdjustmentArray adjustments = GetAdjustmentArray();
        List<InternalActivity> activityList = new ();
        for (int i = 0; i < adjustments.Count; i++)
        {
            Adjustment adj = adjustments[i];
            if (adj is ActivityAdjustment actAdj)
            {
                //Activity makes or consumes inventory.
                InternalActivity activity = actAdj.Activity;
                if (!activityList.Contains(activity))
                {
                    activityList.Add(activity);
                }
            }
        }

        return activityList;
    }

    /// <summary>
    /// Returns the OnHand of the last Adjustment.  If there are no Adjustments then it returns the current OnHand Qty.
    /// This can be used to see if there is a net need to order more or cut back on existing supply.
    /// </summary>
    /// <returns></returns>
    public decimal GetFinalOnHandQty()
    {
        decimal qty = OnHandQty;
        for (int i = 0; i < Adjustments.Count; i++)
        {
            qty += Adjustments[i].ChangeQty;
        }

        return qty;
    }

    /// <summary>
    /// Returns the OnHand of the last Adjustment.  If there are no Adjustments then it returns the current OnHand Qty.
    /// This can be used to see if there is a net need to order more or cut back on existing supply.
    /// </summary>
    /// <returns></returns>
    public decimal GetClockOnHandQty(DateTime a_clockDate)
    {
        decimal totalQty = 0m;
        foreach (Lot lot in Lots)
        {
            if (lot.ProductionDate <= a_clockDate)
            {
                totalQty += lot.Qty;
            }
        }

        return totalQty;
    }

    /// <summary>
    /// Sums of all negative Inventory Adjustments.
    /// </summary>
    /// <returns>A value less than or equal to zero.</returns>
    public decimal GetTotalDemand()
    {
        decimal total = 0;
        for (int i = 0; i < Adjustments.Count; i++)
        {
            decimal chg = Adjustments[i].ChangeQty;
            if (chg < 0) //demand
            {
                total += chg;
            }
        }

        return total;
    }

    /// <summary>
    /// Sums of all positive Inventory Adjustments.
    /// </summary>
    /// <returns>A value greater than or equal to zero.</returns>
    public decimal GetTotalSupply()
    {
        decimal total = 0;
        for (int i = 0; i < Adjustments.Count; i++)
        {
            decimal chg = Adjustments[i].ChangeQty;
            if (chg > 0) //supply
            {
                total += chg;
            }
        }

        return total;
    }

    /// <summary>
    /// Sums of all Sales Order Line Distribution quantities.
    /// </summary>
    public decimal GetTotalSalesOrderQty()
    {
        decimal total = 0;
        for (int i = 0; i < Adjustments.Count; i++)
        {
            Adjustment adj = Adjustments[i];
            //TODO: SA
            //if (adj.Reason is SalesOrderLineDistribution)
            //{
            //    decimal chg = Adjustments[i].ChangeQty;
            //    if (chg < 0) //demand
            //    {
            //        total += chg;
            //    }
            //}
        }

        return total;
    }

    /// <summary>
    /// Sums of all Forecasts.
    /// </summary>
    public decimal GetTotalForecastQty()
    {
        decimal total = 0;
        for (int i = 0; i < Adjustments.Count; i++)
        {
            Adjustment adj = Adjustments[i];
            //TODO: SA
            //if (adj.Reason is ForecastShipment)
            //{
            //    decimal chg = Adjustments[i].ChangeQty;
            //    if (chg < 0) //demand
            //    {
            //        total += chg;
            //    }
            //}
        }

        return total;
    }

    /// <summary>
    /// Sums of all Transfer Orders In.
    /// </summary>
    public decimal GetTotalTransferOrderInQty()
    {
        decimal total = 0;
        for (int i = 0; i < Adjustments.Count; i++)
        {
            Adjustment adj = Adjustments[i];
            //TODO: SA
            //if (adj.Reason is TransferOrderDistribution)
            //{
            //    decimal chg = Adjustments[i].ChangeQty;
            //    if (chg > 0) //Transfer IN
            //    {
            //        total += chg;
            //    }
            //}
        }

        return total;
    }

    /// <summary>
    /// Sums of all Transfer Orders Out.
    /// </summary>
    public decimal GetTotalTransferOrderOutQty()
    {
        decimal total = 0;
        for (int i = 0; i < Adjustments.Count; i++)
        {
            Adjustment adj = Adjustments[i];
            //TODO: SA
            //if (adj.Reason is TransferOrderDistribution)
            //{
            //    decimal chg = Adjustments[i].ChangeQty;
            //    if (chg < 0) //Transfer OUT
            //    {
            //        total += chg;
            //    }
            //}
        }

        return total;
    }
    #endregion

    #region Internal Transmission functionality
    /// <summary>
    /// Modify the ad
    /// </summary>
    /// <param name="t"></param>
    internal void UpdateForecastsInInterval(PT.Transmissions.Forecast.ForecastIntervalQtyChangeT t, BaseIdGenerator aIdGen, ScenarioDetail a_sd)
    {
        IncludeInNetChangeMRP = true;

        if (ForecastVersions == null)
        {
            ForecastVersions = new ForecastVersions(this, aIdGen);
        }

        if (ForecastVersionActive == null)
        {
            ForecastVersion forcastVersion = new (ForecastVersions, aIdGen, aIdGen.NextID(), "Added by Inventory Plan Change");
            ForecastVersions.Versions.Add(forcastVersion);
            ForecastVersionActive = forcastVersion;
        }

        //Create a list of existing Shipments that are in the specified date range
        List<ForecastShipment> shipmentsInRange = new ();
        decimal totalQtyInRange = 0;
        for (int fI = 0; fI < ForecastVersionActive.Forecasts.Count; fI++)
        {
            Forecast forecast = ForecastVersionActive.Forecasts[fI];

            using (IEnumerator<ForecastShipment> shipmentEnumerator = forecast.GetEnumerator())
            {
                while (shipmentEnumerator.MoveNext())
                {
                    ForecastShipment shipment = shipmentEnumerator.Current;
                    if (shipment.RequireDateTicks >= t.IntervalStartInclusive.Ticks && shipment.RequireDateTicks < t.IntervalEndInclusive.Ticks)
                    {
                        shipmentsInRange.Add(shipment);
                        totalQtyInRange += shipment.RequiredQty;
                    }
                }
            }
        }

        decimal qtyToAdd = t.NewQty - totalQtyInRange;
        if (qtyToAdd > 0) //Need to increase qty
        {
            //If there's an existing shipment then increase the last one
            if (shipmentsInRange.Count > 0)
            {
                ForecastShipment lastShipment = shipmentsInRange[shipmentsInRange.Count - 1];
                lastShipment.RequiredQty += qtyToAdd;
            }
            else
            {
                //Create a new Shipment for the addition
                if (ForecastVersionActive.Forecasts.Count > 0) //update the last Forecast
                {
                    Forecast lastForecast = ForecastVersionActive.Forecasts[ForecastVersionActive.Forecasts.Count - 1];
                    ForecastShipment newShipment = new (ForecastVersionActive.IdGen.NextID(), t.IntervalStartInclusive, qtyToAdd, Warehouse.ExternalId, lastForecast);
                    lastForecast.Add(newShipment);
                }
                else //create a new Forecast
                {
                    Forecast newForecast = new (ForecastVersionActive, ForecastVersionActive.IdGen.NextID());
                    ForecastShipment newShipment = new (ForecastVersionActive.IdGen.NextID(), t.IntervalStartInclusive, qtyToAdd, Warehouse.ExternalId, newForecast);
                    newForecast.Add(newShipment);
                    ForecastVersionActive.Forecasts.Add(newForecast);
                }
            }
        }
        else if (qtyToAdd < 0) //need to decrease qty
        {
            //Work through the Shipments from the last to the first removing them until the right qty has been removed.
            decimal qtyRemainingToSubtract = 0 - qtyToAdd;
            for (int sI = shipmentsInRange.Count - 1; sI >= 0; sI--)
            {
                ForecastShipment shipment = shipmentsInRange[sI];
                if (shipment.RequiredQty <= qtyRemainingToSubtract)
                {
                    qtyRemainingToSubtract -= shipment.RequiredQty;
                    shipment.Forecast.RemoveShipment(shipment, a_sd, t);
                }
                else //last shipment to modify
                {
                    shipment.RequiredQty -= qtyRemainingToSubtract;
                    break;
                }
            }
        }
    }
    #endregion

    #region ERP Transmissions
    public void Edit(InventoryEditT a_t, InventoryEdit a_edit, JobManager a_jobManager, SalesOrderManager a_salesOrderManager, ISystemLogger a_errorReporter)
    {
        if (a_edit.LeadTimeSet)
        {
            LeadTime = a_edit.LeadTime;
        }

        if (a_edit.SafetyStockSet)
        {
            SafetyStock = a_edit.SafetyStock;
            IncludeInNetChangeMRP = true;
        }

        if (a_edit.SafetyStockWarningLevelSet)
        {
            SafetyStockWarningLevel = a_edit.SafetyStockWarningLevel;
        }

        if (a_edit.MaxInventorySet)
        {
            MaxInventory = a_edit.MaxInventory;
        }

        if (a_edit.PlannerExternalIdSet)
        {
            PlannerExternalId = a_edit.PlannerExternalId;
        }

        if (a_edit.OnHandQtySet && OnHandQty != a_edit.OnHandQty)
        {
            SetOnHandQty(a_edit.OnHandQty);
        }

        if (a_edit.SafetyStockJobPrioritySet)
        {
            SafetyStockJobPriority = a_edit.SafetyStockJobPriority;
            IncludeInNetChangeMRP = true;
        }

        if (a_edit.MrpProcessingSet)
        {
            MrpProcessing = a_edit.MrpProcessing;
            IncludeInNetChangeMRP = true;
        }

        if (a_edit.MrpExcessQuantityAllocationSet)
        {
            MrpExcessQuantityAllocation = a_edit.MrpExcessQuantityAllocation;
            IncludeInNetChangeMRP = true;
        }

        if (a_edit.TemplateJobExternalIdSet)
        {
            if (TemplateManufacturingOrder != null) //already have a Template reference.  Make sure it's the same one
            {
                if (TemplateManufacturingOrder.Job.ExternalId != a_edit.TemplateJobExternalId) //changed
                {
                    DisassociateTemplateMO();
                    m_templateMoImportRefInfo = new TemplateMoImportRefInfo(a_edit.TemplateJobExternalId);
                    IncludeInNetChangeMRP = true;
                }
            }
            else if (m_templateMoImportRefInfo != null)
            {
                if (m_templateMoImportRefInfo.m_templateJobExternalId != a_edit.TemplateJobExternalId) //update it if changed
                {
                    m_templateMoImportRefInfo = new TemplateMoImportRefInfo(a_edit.TemplateJobExternalId);
                    IncludeInNetChangeMRP = true;
                }
            }
            else
            {
                m_templateMoImportRefInfo = new TemplateMoImportRefInfo(a_edit.TemplateJobExternalId);
            }

            if (m_templateMoImportRefInfo != null)
            {
                Warehouse.Inventories.SetImportedTemplateMoReferences(a_jobManager, a_t, null, a_errorReporter);
            }
        }

        if (a_edit.AutoGenerateForecastsSet)
        {
            AutoGenerateForecasts = a_edit.AutoGenerateForecasts;
        }

        if (a_edit.ForecastIntervalSet)
        {
            ForecastInterval = a_edit.ForecastInterval;
        }

        if (a_edit.NumberOfIntervalsToForecastSet)
        {
            NumberOfIntervalsToForecast = a_edit.NumberOfIntervalsToForecast;
        }

        if (a_edit.ReplenishmentMinSet)
        {
            ReplenishmentMin = a_edit.ReplenishmentMin;
        }

        if (a_edit.ReplenishmentMaxSet)
        {
            ReplenishmentMax = a_edit.ReplenishmentMax;
        }

        if (a_edit.ReplenishmentContractDaysSet)
        {
            ReplenishmentContractDays = a_edit.ReplenishmentContractDays;
        }

        if (a_edit.ReceivingBufferSet)
        {
            ReceivingBuffer = a_edit.ReceivingBuffer;
        }

        if (a_edit.ShippingBufferSet)
        {
            ShippingBufferTicks = a_edit.ShippingBuffer.Ticks;
        }

        if (a_edit.BufferStockSet && BufferStock != a_edit.BufferStock)
        {
            BufferStock = a_edit.BufferStock;
            IncludeInNetChangeMRP = true;
        }

        if (a_edit.PreventSharedBatchOverflowSet && PreventSharedBatchOverflow != a_edit.PreventSharedBatchOverflow)
        {
            PreventSharedBatchOverflow = a_edit.PreventSharedBatchOverflow;
        }

        if (a_edit.MaterialAllocationSet && MaterialAllocation != a_edit.MaterialAllocation)
        {
            MaterialAllocation = a_edit.MaterialAllocation;
        }

        if (a_edit.PurchaseOrderSupplyStorageAreaSet && a_edit.PurchaseOrderSupplyStorageAreaId != PurchaseOrderSupplyStorageArea?.Id)
        {
            StorageArea storageArea = Warehouse.StorageAreas.Find(a_edit.PurchaseOrderSupplyStorageAreaId);

            if (storageArea == null && a_edit.PurchaseOrderSupplyStorageAreaId != BaseId.NULL_ID)
            {
                throw new PTValidationException("3108", new object[]{a_edit.PurchaseOrderSupplyStorageAreaId});
            }

            PurchaseOrderSupplyStorageArea = storageArea;
        }
    }

    internal void Update(WarehouseT.Inventory a_inventory, ScenarioDetail a_sd, UserFieldDefinitionManager a_udfManager)
    {
        if (a_inventory.POSupplyStorageAreaExternalId != PurchaseOrderSupplyStorageArea?.ExternalId)
        {
            StorageArea storageArea = Warehouse.StorageAreas.GetByExternalId(a_inventory.POSupplyStorageAreaExternalId);
            
            if (storageArea == null && !string.IsNullOrEmpty(a_inventory.POSupplyStorageAreaExternalId))
            {
                throw new PTValidationException("3109", new object[] { a_inventory.POSupplyStorageAreaExternalId });
            }

            PurchaseOrderSupplyStorageArea = storageArea;
        }


        if (a_inventory.LeadTimeSet && LeadTime.Ticks != a_inventory.LeadTime.Ticks)
        {
            LeadTime = a_inventory.LeadTime;
        }

        if (a_inventory.DaysOnHandSet && DaysOnHand != a_inventory.DaysOnHand)
        {
            DaysOnHandTicks = a_inventory.DaysOnHand.Ticks;
            CalcDemandTotals(a_sd.SalesOrderManager);
        }

        if (a_inventory.MaterialAllocationSet && MaterialAllocation != a_inventory.MaterialAllocation)
        {
            MaterialAllocation = a_inventory.MaterialAllocation;
        }

        if (a_inventory.PlannerIdSet && PlannerExternalId != a_inventory.PlannerExternalId)
        {
            PlannerExternalId = a_inventory.PlannerExternalId;
        }

        if (a_inventory.SafetyStockSet && SafetyStock != a_inventory.SafetyStock)
        {
            SafetyStock = a_inventory.SafetyStock;
            IncludeInNetChangeMRP = true;
        }

        if (a_inventory.SafetyStockWarningLevelSet && SafetyStockWarningLevel != a_inventory.SafetyStockWarningLevel)
        {
            SafetyStockWarningLevel = a_inventory.SafetyStockWarningLevel;
        }

        if (a_inventory.MaxInventorySet && MaxInventory != a_inventory.MaxInventory)
        {
            MaxInventory = a_inventory.MaxInventory;
        }

        if (a_inventory.StorageCapacitySet && StorageCapacity != a_inventory.StorageCapacity)
        {
            StorageCapacity = a_inventory.StorageCapacity;
        }

        if (a_inventory.SafetyStockJobPrioritySet && SafetyStockJobPriority != a_inventory.SafetyStockJobPriority)
        {
            SafetyStockJobPriority = a_inventory.SafetyStockJobPriority;
            IncludeInNetChangeMRP = true;
        }

        if (a_inventory.ForecastConsumptionSet && ForecastConsumption != a_inventory.ForecastConsumption)
        {
            ForecastConsumption = a_inventory.ForecastConsumption;
            IncludeInNetChangeMRP = true;
        }

        if (a_inventory.ForecastConsumptionWindowDaysSet && ForecastConsumptionWindowDays != a_inventory.ForecastConsumptionWindowDays)
        {
            ForecastConsumptionWindowDays = a_inventory.ForecastConsumptionWindowDays;
            IncludeInNetChangeMRP = true;
        }

        if (a_inventory.MrpProcessingSet && MrpProcessing != a_inventory.MrpProcessing)
        {
            MrpProcessing = a_inventory.MrpProcessing;
            IncludeInNetChangeMRP = true;
        }

        if (a_inventory.MrpExcessQuantityAllocationSet && MrpExcessQuantityAllocation != a_inventory.MrpExcessQuantityAllocation)
        {
            MrpExcessQuantityAllocation = a_inventory.MrpExcessQuantityAllocation;
            IncludeInNetChangeMRP = true;
        }

        if (a_inventory.TemplateJobExternalIdSet)
        {
            if (TemplateManufacturingOrder != null) //already have a Template reference.  Make sure it's the same one
            {
                if (TemplateManufacturingOrder.Job.ExternalId != a_inventory.TemplateJobExternalId) //changed
                {
                    DisassociateTemplateMO();
                    m_templateMoImportRefInfo = new TemplateMoImportRefInfo(a_inventory.TemplateJobExternalId);
                    IncludeInNetChangeMRP = true;
                }
            }
            else if (m_templateMoImportRefInfo != null)
            {
                if (m_templateMoImportRefInfo.m_templateJobExternalId != a_inventory.TemplateJobExternalId) //update it if changed
                {
                    m_templateMoImportRefInfo = new TemplateMoImportRefInfo(a_inventory.TemplateJobExternalId);
                    IncludeInNetChangeMRP = true;
                }
            }
            else
            {
                m_templateMoImportRefInfo = new TemplateMoImportRefInfo(a_inventory.TemplateJobExternalId);
            }
        }

        if (a_inventory.ReceivingBufferSet)
        {
            ReceivingBuffer = a_inventory.ReceivingBuffer;
        }

        if (a_inventory.ShippingBufferSet)
        {
            ShippingBufferTicks = a_inventory.ShippingBuffer.Ticks;
        }

        if (a_inventory.BufferStockSet && BufferStock != a_inventory.BufferStock)
        {
            BufferStock = a_inventory.BufferStock;
            IncludeInNetChangeMRP = true;
        }

        AutoGenerateForecasts = a_inventory.AutoGenerateForecasts;
        ForecastInterval = a_inventory.ForecastInterval;
        NumberOfIntervalsToForecast = a_inventory.NumberOfIntervalsToForecast;
        if (a_inventory.ReplenishmentMinSet)
        {
            ReplenishmentMin = a_inventory.ReplenishmentMin;
        }

        if (a_inventory.ReplenishmentMaxSet)
        {
            ReplenishmentMax = a_inventory.ReplenishmentMax;
        }

        if (a_inventory.ReplenishmentContractDaysSet)
        {
            ReplenishmentContractDays = a_inventory.ReplenishmentContractDays;
        }

        if (a_inventory.PreventSharedBatchOverflowIsSet && PreventSharedBatchOverflow != a_inventory.PreventSharedBatchOverflow) 
        {
            PreventSharedBatchOverflow = a_inventory.PreventSharedBatchOverflow;
        }
    }

    internal void UpdateLots(WarehouseT.Inventory a_inventory, UserFieldDefinitionManager a_udfManager, bool a_lotAutoDelete, bool a_autoDeleteItemStorageLots, IScenarioDataChanges a_dataChanges)
    {
        //Lots
        List<WarehouseT.Inventory.Lot> lotsToAdd = new();
        HashSet<BaseId> updatedLots = new();

        //Update Lots
        if (a_inventory.Lots != null && a_inventory.Lots.Count > 0)
        {
            try
            {
                Lots.InitFastLookupByExternalId();
                HashSet<string> duplicateExternalIdCheck = new();
                //Adds all imported lots to the inventory.
                for (int lotI = 0; lotI < a_inventory.Lots.Count; ++lotI)
                {
                    WarehouseT.Inventory.Lot importedLot = a_inventory.Lots[lotI];
                    Lot checklot = Lots.GetByExternalId(importedLot.ExternalId);

                    if (importedLot.ExpirationDateSet && importedLot.ExpirationDate <= importedLot.ProductionDate)
                    {
                        throw new PTValidationException("3047", new object[] { importedLot.ExternalId, Warehouse.ExternalId, Item.ExternalId });
                    }

                    if (checklot != null)
                    {
                        updatedLots.Add(checklot.Id);
                        TimeSpan shelfLife = importedLot.ExpirationDateSet ? importedLot.ExpirationDate - importedLot.ProductionDate : Item.ShelfLife;
                        TimeSpan shelfLifeWarning = Item.ShelfLifeWarning;
                        checklot.Update(importedLot, a_autoDeleteItemStorageLots, shelfLife, shelfLifeWarning, a_udfManager, a_dataChanges);
                    }
                    else
                    {
                        //Verify that multiple lots are not being imported with the same external id
                        if (duplicateExternalIdCheck.Contains(importedLot.ExternalId))
                        {
                            throw new PTValidationException("3007", new object[] { Warehouse.ExternalId, Item.ExternalId, importedLot.ExternalId }, true);
                        }

                        lotsToAdd.Add(importedLot);
                        duplicateExternalIdCheck.Add(importedLot.ExternalId);
                    }
                }
            }
            finally
            {
                Lots.DeInitFastLookupByExternalId();
            }
        }

        if (a_lotAutoDelete)
        {
            for (int i = Lots.Count - 1; i >= 0; i--)
            {
                Lot lotToDelete = Lots.GetByIndex(i);
                if (!updatedLots.Contains(lotToDelete.Id))
                {
                    Lots.Remove(lotToDelete);
                }
            }
        }

        foreach (WarehouseT.Inventory.Lot lot in lotsToAdd)
        {
            TimeSpan shelfLife = lot.ExpirationDateSet ? lot.ExpirationDate - lot.ProductionDate : Item.ShelfLife;
            TimeSpan shelfLifeWarning = Item.ShelfLifeWarning;
            Lot addedLot = Lots.CreateAndAddLot(lot, a_autoDeleteItemStorageLots, this, shelfLife, shelfLifeWarning, a_udfManager, a_dataChanges);
            addedLot.LotSource = ItemDefs.ELotSource.Inventory;
        }

        if (Lots.Count > 0)
        {
            //TODO: Causes default lot to be added
            //Override onhand qty with the sum of lots
            //OnHandQty = Lots.CalcOnHandQty();
        }
    }
    #endregion

    #region Cloning
    public Inventory Clone()
    {
        return (Inventory)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion

    public int CompareTo(Inventory a_otherInventory)
    {
        return Id.CompareTo(a_otherInventory.Id);
    }

    #region Debugging
    public override string ToString()
    {
        return string.Format("Inventory for Item '{0}' in Warehouse '{1}', OnHandQty='{2}'".Localize(), m_item.Name, Warehouse.Name, OnHandQty);
    }
    #endregion

    #region PT Import DB
    public void PopulateImportDataSet(PtImportDataSet ds, PtImportDataSet.WarehousesRow a_warehousesRow, PtImportDataSet.ItemsRow itemRow, PtImportDataSet.LotsDataTable lotTable)
    {
        string templateJobExternalId = "";
        string templateMoExternalId = "";
        if (TemplateManufacturingOrder != null)
        {
            templateJobExternalId = TemplateManufacturingOrder.Job.ExternalId;
            templateMoExternalId = TemplateManufacturingOrder.ExternalId;
        }

        PtImportDataSet.InventoriesRow row = ds.Inventories.AddInventoriesRow(a_warehousesRow,
            PurchaseOrderSupplyStorageArea?.ExternalId ?? string.Empty,
            itemRow,
            MrpProcessing.ToString(),
            MaterialAllocation.ToString(),
            BufferStock,
            SafetyStock,
            SafetyStockWarningLevel,
            LeadTime.TotalDays,
            StorageCapacity,
            PlannerExternalId,
            SafetyStockJobPriority,
            ForecastConsumption.ToString(),
            MaxInventory,
            templateJobExternalId,
            ReceivingBuffer.TotalDays,
            ShippingBuffer.TotalDays,
            AutoGenerateForecasts,
            ForecastInterval.ToString(),
            NumberOfIntervalsToForecast,
            MrpExcessQuantityAllocation.ToString(),
            ForecastConsumptionWindowDays,
            PreventSharedBatchOverflow);
        
        if (m_lots.Count > 0)
        {
            //add lot rows
            for (int i = 0; i < m_lots.Count; i++)
            {
                m_lots.GetByIndex(i).PopulateImportDataSet(Item.ExternalId, a_warehousesRow.ExternalId,  lotTable, ds.ItemStorageLots);
            }
        }
    }
    #endregion

    #region UI calculations
    /// <summary>
    /// Returns information about the supply of inventory closes to and before the specified date.
    /// </summary>
    public void GetMostRecentSupplyPriorToDate(ScenarioDetail a_sd, DateTime priorToDate, out DateTime lastSupplyDate, out decimal lastSupplyQty, out string lastSupplyDescription)
    {
        AdjustmentArray adjReqs = GetRequirementAdjustments(a_sd);

        for (int i = adjReqs.Count - 1; i >= 0; i--)
        {
            Adjustment adj = adjReqs[i];
            if (adj.AdjDate.Ticks <= priorToDate.Ticks && adj.ChangeQty > 0) //this is a valid one
            {
                lastSupplyDate = adj.AdjDate;
                lastSupplyQty = adj.ChangeQty;
                lastSupplyDescription = adj.ReasonDescription;
                return;
            }
        }

        lastSupplyDate = a_sd.ClockDate;
        lastSupplyQty = 0;
        lastSupplyDescription = "On-Hand Inventory".Localize();
    }

    /// <summary>
    /// Returns true and values if there is a negative inventory level.
    /// </summary>
    public bool GetFirstShortage(out DateTime dt, out decimal shortage, ScenarioDetail sd, bool includeForecasts, out bool shortageWithinLeadTime)
    {
        AdjustmentArray reqAdjs = GetRequirementAdjustments(sd);
        decimal newVal = OnHandQty;
        for (int i = 0; i < reqAdjs.Count; i++)
        {
            Adjustment adj = reqAdjs[i];
            //TODO: SA
            //if (includeForecasts || adj.Reason is not ForecastShipment)
            //{
            //    newVal += adj.ChangeQty;
            //    if (newVal < 0 && adj is not AdjustmentExpirable) //Lot expirations are not shortages since there is no demand yet)
            //    {
            //        dt = adj.AdjDate;
            //        shortage = newVal;
            //        shortageWithinLeadTime = dt.Ticks < sd.Clock + LeadTimeTicks;
            //        return true;
            //    }
            //}
        }

        dt = DateTime.MaxValue;
        shortage = 0;
        shortageWithinLeadTime = false;
        return false;
    }

    /// <summary>
    /// Returns true if stock falls below safety stock and safety stock is greater than zero.
    /// </summary>
    public bool GetFirstLowStock(out DateTime dt, out decimal invQty, ScenarioDetail sd, bool includeForecasts, out bool lowWithinLeadTime)
    {
        dt = DateTime.MaxValue;
        invQty = 0;
        lowWithinLeadTime = false;
        if (SafetyStock <= 0)
        {
            return false;
        }

        if (OnHandQty < SafetyStock)
        {
            dt = sd.ClockDate;
            invQty = OnHandQty;
            lowWithinLeadTime = true;
            return true;
        }

        AdjustmentArray reqAdjs = GetRequirementAdjustments(sd);
        decimal newVal = OnHandQty;
        for (int i = 0; i < reqAdjs.Count; i++)
        {
            Adjustment adj = reqAdjs[i];
            //TODO: SA
            //if (includeForecasts || adj.Reason is not ForecastShipment)
            //{
            //    newVal += adj.ChangeQty;
            //    if (newVal < SafetyStock)
            //    {
            //        dt = adj.AdjDate;
            //        invQty = newVal;
            //        lowWithinLeadTime = dt.Ticks < sd.Clock + LeadTimeTicks;
            //        return true;
            //    }
            //}
        }

        return false;
    }

    /// <summary>
    /// Returns true if stock falls below safety stock and safety stock is greater than zero.
    /// </summary>
    public bool GetFirstMaxInvViolation(out DateTime dt, out decimal invQty, ScenarioDetail sd, bool includeForecasts)
    {
        dt = DateTime.MaxValue;
        invQty = 0;

        if (MaxInventory == 0)
        {
            //The value isn't set
            return false;
        }

        if (OnHandQty > MaxInventory)
        {
            dt = sd.ClockDate;
            invQty = OnHandQty;
            return true;
        }

        AdjustmentArray reqAdjs = GetRequirementAdjustments(sd);
        decimal newVal = OnHandQty;
        for (int i = 0; i < reqAdjs.Count; i++)
        {
            Adjustment adj = reqAdjs[i];
            //TODO: SA
            //if (includeForecasts || adj.Reason is not ForecastShipment)
            //{
            //    newVal += adj.ChangeQty;
            //    if (newVal > MaxInventory)
            //    {
            //        dt = adj.AdjDate;
            //        invQty = newVal;
            //        return true;
            //    }
            //}
        }

        return false;
    }
    #endregion

    #region Forecasting
    private bool m_autoGenerateForecasts;

    /// <summary>
    /// Auto generates forecasts based on sales orders after import.
    /// </summary>
    public bool AutoGenerateForecasts
    {
        get => m_autoGenerateForecasts;
        internal set => m_autoGenerateForecasts = value;
    }

    /// <summary>
    /// The periodicity (cycle of values) (e.g. monthly values with a seasonal cycle = 12)
    /// </summary>
    private DateInterval.EInterval m_forecastInterval;

    public DateInterval.EInterval ForecastInterval
    {
        get => m_forecastInterval;
        internal set => m_forecastInterval = value;
    }

    private int m_numberOfIntervalsToForecast;

    /// <summary>
    /// The number of prediction steps to forecast
    /// </summary>
    public int NumberOfIntervalsToForecast
    {
        get => m_numberOfIntervalsToForecast;
        internal set => m_numberOfIntervalsToForecast = value;
    }

    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private ForecastVersions m_forecastVersions;

    public ForecastVersions ForecastVersions
    {
        get => m_forecastVersions;
        internal set
        {
            m_forecastVersions = value;
            if (m_forecastVersions != null && m_forecastVersions.Versions != null && m_forecastVersions.Versions.Count > 0)
            {
                m_forecastVersionActive = m_forecastVersions.Versions[0];
            }
        }
    }

    private ForecastVersion m_forecastVersionActive;

    public ForecastVersion ForecastVersionActive
    {
        get => m_forecastVersionActive;
        private set => m_forecastVersionActive = value;
    }

    internal void ForecastVersionsClear(ScenarioDetail a_sd, PTTransmissionBase a_t)
    {
        if (ForecastVersions != null)
        {
            ForecastVersions.Clearing(a_sd, a_t);
            ForecastVersions = null;
        }

        ForecastVersionActive = null;
    }

    internal void ForecastVersionsReceive(PT.Transmissions.Forecast.ForecastShipmentDeleteT a_t, List<ForecastShipmentKey> a_shipmentsKeyToDelete, ScenarioDetail a_sd)
    {
        foreach (ForecastShipmentKey shipmentKey in a_shipmentsKeyToDelete)
        {
            ForecastShipment shipment = GetForecastShipment(shipmentKey);
            if (shipment != null)
            {
                shipment.Forecast.RemoveShipment(shipment, a_sd, a_t);
            }
        }
    }

    internal void ForecastVersionsReceive(PT.Transmissions.Forecast.ForecastShipmentAdjustmentT a_t, Dictionary<ForecastShipmentKey, decimal> a_shipmentAdjustments, ScenarioDetail a_sd)
    {
        foreach (KeyValuePair<ForecastShipmentKey, decimal> shipmentKey in a_shipmentAdjustments)
        {
            ForecastShipment shipment = GetForecastShipment(shipmentKey.Key);
            if (shipment != null)
            {
                shipment.RequiredQty = shipmentKey.Value;
            }
        }
    }

    internal void CreateForecasts(List<Tuple<DateTime, decimal>> a_reqDatesAndQties, bool a_autoGenerated, ScenarioDetail a_sd, BaseIdGenerator a_idGen, PTTransmission a_t)
    {
        if (ForecastVersions == null)
        {
            ForecastVersions = new ForecastVersions(this, a_idGen);
        }

        if (ForecastVersions.Versions.Count == 0)
        {
            ForecastVersion fver = new (ForecastVersions, a_idGen, a_idGen.NextID(), "1");
            ForecastVersions.AddVersion(fver);
        }

        if (ForecastVersionActive == null)
        {
            ForecastVersionActive = ForecastVersions.Versions[0];
        }

        Forecast f = new (ForecastVersionActive, a_idGen.NextID());
        f.Name = f.ExternalId;
        foreach (Tuple<DateTime, decimal> req in a_reqDatesAndQties)
        {
            f.Add(new ForecastShipment(a_idGen.NextID(), req.Item1, req.Item2, Warehouse.ExternalId, f));
        }

        ForecastVersionActive.Forecasts.Add(f);
    }

    internal void ForecastVersionsReceive(UserFieldDefinitionManager a_udfManager, ForecastT aFT, ForecastT.ForecastVersions aFVs, BaseIdGenerator aIdGen, ScenarioDetail a_sd)
    {
        if (ForecastVersions == null)
        {
            ForecastVersions = new ForecastVersions(a_sd.CustomerManager, this, aFVs, aIdGen, aFT, a_udfManager);

            if (ForecastVersions.Versions.Count > 0)
            {
                ForecastVersionActive = ForecastVersions.Versions[0];
                IncludeInNetChangeMRP = true;
            }
        }
        else
        {
            ForecastVersions.Update(a_udfManager, aFT, aFVs, out bool mrpNetChangeCriticalUpdates, a_sd);
            if (mrpNetChangeCriticalUpdates)
            {
                IncludeInNetChangeMRP = true;
            }

            if (!ForecastVersions.Versions.Contains(ForecastVersionActive))
            {
                if (ForecastVersions.Versions.Count > 0)
                {
                    ForecastVersionActive = ForecastVersions.Versions[0];
                }
                else
                {
                    ForecastVersionActive = null;
                }
            }
        }
    }

    private ForecastShipment GetForecastShipment(ForecastShipmentKey a_shipmentKey)
    {
        ForecastVersion ver = m_forecastVersions.Find(a_shipmentKey.ShipmentForecastKey.Version);
        if (ver != null)
        {
            Forecast forecast = ver.GetById(new BaseId(a_shipmentKey.ShipmentForecastKey.ForecastId));
            if (forecast != null)
            {
                return forecast[new BaseId(a_shipmentKey.ShipmentId)];
            }
        }

        return null;
    }

    internal ForecastT.ForecastVersions GenerateForecastVersionsT(ScenarioDetail a_sd)
    {
        ForecastT.Forecast forecast = new ();
        forecast.ExternalId = "AutoGenerated".Localize();
        forecast.Name = "AutoGenerated".Localize();
        DateTime salesRangeStart;
        decimal[] sales;
        List<Tuple<DateTime, decimal>> timeSeries = GetForecastQties(a_sd, ForecastInterval, NumberOfIntervalsToForecast, DateInterval.GetIntervalCountPerYear(ForecastInterval), a_sd.ClockDate, out sales, out salesRangeStart);
        if (timeSeries == null || timeSeries.Count == 0)
        {
            return null;
        }

        foreach (Tuple<DateTime, decimal> item in timeSeries)
        {
            if (item.Item2 > 0)
            {
                forecast.Shipments.Add(new ForecastT.ForecastShipment(item.Item2, item.Item1, Warehouse.ExternalId));
            }
        }

        ForecastT.ForecastVersion version = new ();
        version.Version = ForecastVersionActive != null ? ForecastVersionActive.Version : "1";
        version.Add(forecast);

        ForecastT.ForecastVersions versions = new ();
        versions.ItemExternalId = Item.ExternalId;
        versions.WarehouseExternalId = Warehouse.ExternalId;
        versions.Versions.Add(version);

        return versions;
    }

    public List<Tuple<DateTime, decimal>> GetForecastQties(ScenarioDetail a_sd, DateInterval.EInterval a_interval, int a_numOfIntervals, int a_numOfSalesIntervals, DateTime a_salesRangeEnd, out decimal[] o_sales, out DateTime o_salesRangeStart)
    {
        o_sales = GetSalesPerInverval(a_sd, a_interval, a_salesRangeEnd, a_numOfSalesIntervals, out o_salesRangeStart);
        if (o_sales.Length == 0)
        {
            return null;
        }

        double[] salesDouble = ConvertArray(o_sales);
        double[] qties = ForecastServiceClient.Forecasting.Forecast(salesDouble, DateInterval.GetIntervalCountPerYear(a_interval), a_numOfIntervals).Result;
        decimal[] dblQties = ConvertArray(qties);
        return CreateForecastTimeSeries(dblQties, a_salesRangeEnd, a_interval);
    }

    private static decimal[] ConvertArray(double[] a_dbl)
    {
        decimal[] result = new decimal[a_dbl.Length];
        for (int i = 0; i < a_dbl.Length; ++i)
        {
            result[i] = (decimal)a_dbl[i];
        }

        return result;
    }

    private static double[] ConvertArray(decimal[] a_dbl)
    {
        double[] result = new double[a_dbl.Length];
        for (int i = 0; i < a_dbl.Length; ++i)
        {
            result[i] = (double)a_dbl[i];
        }

        return result;
    }

    private List<Tuple<DateTime, decimal>> CreateForecastTimeSeries(decimal[] a_qties, DateTime a_startDate, DateInterval.EInterval a_interval)
    {
        List<Tuple<DateTime, decimal>> timeSeries = new ();

        if (a_interval == DateInterval.EInterval.Month) // add the forecast to the same date every month (unless its larger than)
        {
            DateTime requiredDate = a_startDate;
            int dayOfTheMonth = requiredDate.Day;
            for (int i = 0; i < a_qties.Length; i++)
            {
                decimal qty = a_qties[i];
                if (qty > 0)
                {
                    timeSeries.Add(new Tuple<DateTime, decimal>(GetAdjustedDateTime(requiredDate, dayOfTheMonth), qty));
                }

                requiredDate = requiredDate.AddMonths(1);
            }
        }
        else
        {
            DateTime requiredDate = a_startDate;
            for (int i = 0; i < a_qties.Length; i++)
            {
                decimal qty = a_qties[i];
                if (qty > 0)
                {
                    timeSeries.Add(new Tuple<DateTime, decimal>(requiredDate, qty));
                }

                requiredDate = requiredDate.Add(DateInterval.GetIntervalTimeSpan(a_interval));
            }
        }

        return timeSeries;
    }

    private DateTime GetAdjustedDateTime(DateTime a_date, int a_dayOfTheMonth)
    {
        if (a_dayOfTheMonth <= 0 || a_dayOfTheMonth > 31) // don't recurse until the end of times
        {
            return a_date;
        }

        try
        {
            return new DateTime(a_date.Year, a_date.Month, a_dayOfTheMonth);
        }
        catch (ArgumentOutOfRangeException) // tried to create date like 2/30/2016
        {
            return GetAdjustedDateTime(a_date, a_dayOfTheMonth - 1);
        }
    }

    public bool HasAtLeastOneSalesOrder(ScenarioDetail a_sd)
    {
        foreach (SalesOrder so in a_sd.SalesOrderManager)
        {
            foreach (SalesOrderLine sol in so.SalesOrderLines)
            {
                if (sol.Item.Id == Item.Id)
                {
                    foreach (SalesOrderLineDistribution sod in sol.LineDistributions)
                    {
                        //TODO: How do we know if it has a SO if it is not bound directly. Look at another source?
                        if (sod.MustSupplyFromWarehouse == null || sod.MustSupplyFromWarehouse.Id == Warehouse.Id)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    public decimal[] GetSalesPerInverval(ScenarioDetail a_sd, DateInterval.EInterval a_interval, DateTime a_salesRangeEnd, int a_nbrOfIntervals, out DateTime o_salesRangeStart)
    {
        decimal[] salesPerInterval = new decimal[a_nbrOfIntervals];

        List<SalesOrderLineDistribution> sods = GetSalesOrderLineDistributions(a_sd, false, false);

        TimeSpan intervalDuration = DateInterval.GetIntervalTimeSpan(a_interval);

        if (a_interval == DateInterval.EInterval.Quarter)
        {
            o_salesRangeStart = a_salesRangeEnd.AddMonths(-1 * 3 * a_nbrOfIntervals);
        }
        else if (a_interval == DateInterval.EInterval.Month)
        {
            o_salesRangeStart = a_salesRangeEnd.AddMonths(-1 * a_nbrOfIntervals);
        }
        else if (a_interval == DateInterval.EInterval.Week)
        {
            o_salesRangeStart = a_salesRangeEnd.AddDays(-1 * 7 * a_nbrOfIntervals);
        }
        else // day
        {
            o_salesRangeStart = a_salesRangeEnd.AddDays(-1 * a_nbrOfIntervals);
        }

        foreach (SalesOrderLineDistribution sod in sods)
        {
            if (sod.RequiredAvailableDate > a_salesRangeEnd || sod.RequiredAvailableDate <= o_salesRangeStart)
            {
                continue;
            }

            TimeSpan fromStart = sod.RequiredAvailableDate.Subtract(o_salesRangeStart);
            int index = (int)(fromStart.Ticks / intervalDuration.Ticks);
            if (index < 0)
            {
                index = 0;
            }
            else if (index > salesPerInterval.Length - 1)
            {
                index = salesPerInterval.Length - 1;
            }

            salesPerInterval[index] += sod.QtyOrdered;
        }

        return salesPerInterval;
    }
    #endregion

    #region ForecastConsumption
    /// <summary>
    /// Options for how Forecast quantities are reduced based on actual demand.
    /// </summary>
    public InventoryDefs.ForecastConsumptionMethods ForecastConsumption { get; private set; } = InventoryDefs.ForecastConsumptionMethods.None;

    private double m_forecastConsumptionWindowDays;

    /// <summary>
    /// Given a sales order date, this is the number of days from that date
    /// the system searches to find forecasts to consume.
    /// Value of zero indicates no limit.
    /// </summary>
    public double ForecastConsumptionWindowDays
    {
        get => m_forecastConsumptionWindowDays;
        private set => m_forecastConsumptionWindowDays = value;
    }

    internal void ConsumeForecasts(ScenarioDetail a_sd, bool a_consumeForecasts)
    {
        GetForecastShipments(false, false).ForEach(fs => fs.ClearConsumption());
        if (!a_consumeForecasts)
        {
            return;
        }

        IForecastConsumptionStrategy forecastConsumptionStrategy = CreateForecastConsumptionStrategy();
        forecastConsumptionStrategy?.ConsumeForecasts(a_sd, this);
    }

    private IForecastConsumptionStrategy CreateForecastConsumptionStrategy()
    {
        switch (ForecastConsumption)
        {
            case InventoryDefs.ForecastConsumptionMethods.Backward:
                return new BackwardForecastConsumption();
            case InventoryDefs.ForecastConsumptionMethods.Forward:
                return new ForewardForecastConsumption();
            case InventoryDefs.ForecastConsumptionMethods.BackwardThenForward:
                return new BackwardThenForewardForecastConsumption();
            case InventoryDefs.ForecastConsumptionMethods.Spread:
                return new SpreadForecastConsumption();
            default: // None or unknown
                return null;
        }
    }
    #endregion

    #region MRP
    private decimal m_mrpAllocated;

    /// <summary>
    /// The amount of on-hand inventory that has been allocated to requirements.
    /// (Only valid after MRP is run; not serialized)
    /// </summary>
    public decimal MrpAllocated
    {
        get => m_mrpAllocated;
        internal set => m_mrpAllocated = value;
    }

    /// <summary>
    /// On-hand minus Allocated.
    /// (Only valid after MRP is run; not serialized)
    /// </summary>
    public decimal MrpUnallocated => OnHandQty - MrpAllocated;

    private string m_mrpNotes;

    /// <summary>
    /// Notes stored during the last MRP run.
    /// Not stored between sessions.
    /// </summary>
    public string MrpNotes
    {
        get => m_mrpNotes;
        internal set => m_mrpNotes = value;
    }

    private ManufacturingOrder m_templateMO;

    /// <summary>
    /// Specifies the Manufacturing Order to be used in the creation of new Jobs.
    /// Only applies to Inventories that are set to Generate Jobs during MRP.
    /// The Manufacturing Order is usually part of a Job that is marked as a Template but doesn't have to be.
    /// </summary>
    [DoNotAuditProperty]
    public ManufacturingOrder TemplateManufacturingOrder
    {
        get => m_templateMO;
        internal set => m_templateMO = value;
    }

    public bool HaveTemplateManufacturingOrderId => TemplateManufacturingOrder != null;

    private TemplateMoImportRefInfo m_templateMoImportRefInfo;

    /// <summary>
    /// Null unless the Inventory is waiting to be connected to a Template MO.
    /// </summary>
    [DoNotAuditProperty]
    internal TemplateMoImportRefInfo TemplateMoImportRef => m_templateMoImportRefInfo;

    internal void SetTemplateMoReference(ManufacturingOrder aMo)
    {
        m_templateMO = aMo;
        m_templateMoImportRefInfo = null;
        aMo.UsedAsTemplateForInventory(this);
        IncludeInNetChangeMRP = true;
    }

    internal void ClearImportTemplateMoReferenceDueToError()
    {
        m_templateMoImportRefInfo = null;
    }

    /// <summary>
    /// Deleting this Inventory so remove any references.
    /// </summary>
    internal void Deleting()
    {
        DisassociateTemplateMO();
    }

    internal void DisassociateTemplateMO()
    {
        if (TemplateManufacturingOrder != null)
        {
            TemplateManufacturingOrder.NoLongerUsedAsTemplateForInventory(this);
            m_templateMO = null;
        }
    }

    /// <summary>
    /// Stores the Job ExernalId for the MO that should be used as the MO Template.
    /// Since Inventories are imported before Jobs this is to allow them to be connected when the Job import completes.
    /// The first MO in the Job is used.
    /// </summary>
    internal class TemplateMoImportRefInfo
    {
        internal TemplateMoImportRefInfo(string aJobExternalId)
        {
            m_templateJobExternalId = aJobExternalId;
        }

        internal string m_templateJobExternalId;
    }

    /// <summary>
    /// True if this Inventory or a higher level Inventory was updated since the last MRP.
    /// </summary>
    /// <returns></returns>
    public bool IncludeInNetChangeMRP
    {
        get => m_bools[IncludeInNetChangeMRPIdx];
        internal set => m_bools[IncludeInNetChangeMRPIdx] = value;
    }

    /// <summary>
    /// True if Sales Orders changed since last automatic Forecasting run.
    /// </summary>
    internal bool SalesOrdersChangedSinceLastForecast
    {
        get => m_bools[c_salesOrdersChangedSinceLastForecastIdx];
        set => m_bools[c_salesOrdersChangedSinceLastForecastIdx] = value;
    }

    private int m_lowLevelCode;

    /// <summary>
    /// Indicates how deep in the Bill of Material the Item is used.  Level 0 means no other Item uses the Item as a material.
    /// Calculated during MRP.
    /// </summary>
    public int LowLevelCode
    {
        get => m_lowLevelCode;
        internal set => m_lowLevelCode = value;
    }
    #endregion MRP

    #region Buffers
    /// <summary>
    /// Indicates the urgency for creating a new replenishment Job.
    /// Lower values are considered higher priority.
    /// Calculated as 100 - (Buffer Stock - OnHand - Total Supply) / Buffer Stock.
    /// </summary>
    public int ReleasePriority
    {
        get
        {
            if (BufferStock == 0)
            {
                return int.MaxValue;
            }

            return (int)Math.Round(100 - (BufferStock - OnHandQty - GetTotalSupply()) / BufferStock * 100, 0);
        }
    }

    public bool PreventSharedBatchOverflow
    {
        get => m_bools[c_preventSharedBatchOverflowIdx];
        internal set => m_bools[c_preventSharedBatchOverflowIdx] = value;
    }

    /// <summary>
    /// The quanity that needs to be replenished to meet the Buffer Stock.
    /// Calculated as Buffer Stock + Forecast in Lead Time - OnHand Qty - Total Supply Qty.
    /// </summary>
    public decimal GetQtyShort(ScenarioDetail a_sd)
    {
        return BufferStock + GetTotalForecastInLeadTime(a_sd) - OnHandQty - GetTotalSupply();
    }

    /// <summary>
    /// The quantity to order to replenish the Buffer Stock base on the Qty Short and subject to the Minimum Order Quantity and Batch Size.
    /// </summary>
    /// <returns></returns>
    public decimal GetRecommendedReplenishmentQty(ScenarioDetail a_sd)
    {
        decimal qtyShort = GetQtyShort(a_sd);
        decimal qtyToOrder;
        if (Item.BatchSize > 0)
        {
            int nbrBatchesRounded = (int)Math.Ceiling(qtyShort / Item.BatchSize);
            qtyToOrder = Item.BatchSize * nbrBatchesRounded;
        }
        else
        {
            qtyToOrder = qtyShort;
        }

        if (qtyToOrder >= Item.MinOrderQty)
        {
            return qtyToOrder;
        }

        return 0;
    }

    /// <summary>
    /// The total quantity of all Forecasts within the Lead Time.
    /// </summary>
    public decimal GetTotalForecastInLeadTime(ScenarioDetail a_sd)
    {
        decimal total = 0;
        AdjustmentArray reqAdjs = GetRequirementAdjustments(a_sd);
        for (int i = 0; i < reqAdjs.Count; i++)
        {
            Adjustment adj = reqAdjs[i];

            //TODO: SA
            //if (adj.Reason is ForecastShipment && adj.AdjDate.Ticks <= a_sd.Clock + LeadTimeTicks)
            //{
            //    decimal chg = adj.ChangeQty;
            //    if (chg < 0) //demand
            //    {
            //        total -= chg;
            //    }
            //}
        }

        return total;
    }

    /// <summary>
    /// The first Adjustment with Change Qty > 0.
    /// Null if no such Adjustment.
    /// </summary>
    /// <returns></returns>
    public Adjustment GetFirstPostiveAdjustment()
    {
        for (int i = 0; i < Adjustments.Count; i++)
        {
            Adjustment adjustment = Adjustments[i];
            if (adjustment.ChangeQty > 0)
            {
                return adjustment;
            }
        }

        return null;
    }

    public DateTime GetFirstPostitiveAdjustmentDate()
    {
        Adjustment adj = GetFirstPostiveAdjustment();
        if (adj != null)
        {
            return adj.AdjDate;
        }

        return DateTime.MaxValue;
    }

    public decimal GetFirstPostitiveAdjustmentQty()
    {
        Adjustment adj = GetFirstPostiveAdjustment();
        if (adj != null)
        {
            return adj.ChangeQty;
        }

        return 0;
    }
    #endregion Buffers

    // list of adjustments containing:
    //     product and materials adjustments at scheduled dates
    //     sales order, forecast and transfer order adjustments at required date.
    [DebugLogging(EDebugLoggingType.None)] private ICalculatedValueCache<AdjustmentArray> m_requirementAdjustmentCache;

    /// <summary>
    /// Get adjustment array where product and material adjustments occur at scheduled dates while
    /// other adjustments (SalesOrder, Forecast, TransferOrder) occur at their required date.
    /// </summary>
    public AdjustmentArray GetRequirementAdjustments(ScenarioDetail a_sd)
    {
        lock (this) //Lock so that the returned list cannot be enumerated while it is being created
        {
            if (m_requirementAdjustmentCache != null && m_requirementAdjustmentCache.TryGetValue(out AdjustmentArray cachedAdjustments))
            {
                return cachedAdjustments;
            }

            AdjustmentArray adjustments = CalculateRequirementAdjustments(a_sd);
            adjustments.Sort();

            //Cache the adjustments array if necessary, otherwise just return the adjustments array.
            m_requirementAdjustmentCache?.CacheValue(adjustments);
            return adjustments;
        }
    }

    /// <summary>
    /// creates and stores adjustment array where product and material adjustments are derived from simulation
    /// generated adjustments (scheduled adjustments). Other adjustments, such as TransferOrders and
    /// SalesOrders are created from raw data and will have original required dates rather than scheduled
    /// dates.
    /// </summary>
    private AdjustmentArray CalculateRequirementAdjustments(ScenarioDetail a_sd)
    {
        AdjustmentArray adjustmentArray = new (m_idGen);

        // first add product and material adjustments from simulation generated adjustments
        for (int i = 0; i < Adjustments.Count; i++)
        {
            Adjustment adj = Adjustments[i];

            adjustmentArray.Add(adj);
        }

        //TODO: SA
        // add in sales
        foreach (SalesOrderLineDistribution sod in GetSalesOrderLineDistributions(a_sd, true, false))
        {
            //adjustmentArray.Add(new SalesOrderMrpAdjustment(sod.RequiredAvailableDateTicks, sod, -sod.QtyOpenToShip));
        }

        //// add in transfer orders
        //foreach (TransferOrderDistribution tod in GetTransferOrderDistributions(a_sd, true))
        //{
        //    if (tod.FromWarehouse.Id == Warehouse.Id) // this is a negative adjustment to this invenotry.
        //    {
        //        adjustmentArray.Add(new Adjustment(tod.ScheduledShipDateTicks, tod, -tod.QtyOpenToShip));
        //    }
        //    else
        //    {
        //        adjustmentArray.Add(new Adjustment(tod.ScheduledReceiveDateTicks, tod, tod.QtyOrdered - tod.QtyReceived));
        //    }
        //}

        //// add in forecasts
        //foreach (ForecastShipment fs in GetForecastShipments(true, false))
        //{
        //    adjustmentArray.Add(new Adjustment(fs.RequireDateTicks, fs, -fs.GetUnconsumedQty()));
        //}

        return adjustmentArray;
    }

    /// <summary>
    /// gets list of sales order distribution lines that reference this inventory.
    /// </summary>
    internal List<SalesOrderLineDistribution> GetSalesOrderLineDistributions(ScenarioDetail a_sd, bool a_openOnly, bool a_orderByDate)
    {
        List<SalesOrderLineDistribution> sods = new ();
        foreach (SalesOrder salesOrder in a_sd.SalesOrderManager.OpenOrdersEnumerator)
        {
            foreach (SalesOrderLine line in salesOrder.SalesOrderLines)
            {
                foreach (SalesOrderLineDistribution distribution in line.LineDistributions)
                {
                    if (!distribution.Closed)
                    {
                        if (distribution.QtyOpenToShip > 0m)
                        {
                            if (distribution.MustSupplyFromWarehouse == Warehouse && distribution.Item == Item)
                            {
                                sods.Add(distribution);
                            }
                        }
                    }
                }
            }
        }
        
        sods = sods.OrderBy(sod => sod.RequiredAvailableDateTicks).ToList();

        HashSet<BaseId> baseIds = new ();
        for (int i = 0; i < Adjustments.Count; i++)
        {
            Adjustment adj = Adjustments[i];
            //TODO: SA
            //if (adj.Reason is SalesOrderLineDistribution sod)
            //{
            //    if (baseIds.Contains(sod.Id))
            //    {
            //        continue; // multiple adjustments can be created for the same object.
            //    }

            //    sods.Add(sod);
            //    baseIds.Add(sod.Id);
            //}
        }

        if (a_orderByDate && sods.Count > 0)
        {
        }

        return sods;
    }

    /// <summary>
    /// Gets a list of transfer order distributions that reference this inventory.
    /// </summary>
    private List<TransferOrderDistribution> GetTransferOrderDistributions(ScenarioDetail a_sd, bool a_openOnly)
    {
        List<TransferOrderDistribution> tods = new ();
        HashSet<BaseId> baseIds = new ();
        for (int i = 0; i < Adjustments.Count; i++)
        {
            Adjustment adj = Adjustments[i];
            //TODO: SA
            //if (adj.Reason is TransferOrderDistribution tod)
            //{
            //    if (baseIds.Contains(tod.Id))
            //    {
            //        continue;
            //    }

            //    if (a_openOnly && (tod.Closed || tod.QtyOpenToShip <= 0))
            //    {
            //        continue;
            //    }

            //    tods.Add(tod);
            //    baseIds.Add(tod.Id);
            //}
        }

        return tods;
    }

    /// <summary>
    /// get a list of forecast shipments
    /// </summary>
    /// <param name="a_openonly"></param>
    /// <returns></returns>
    internal List<ForecastShipment> GetForecastShipments(bool a_openonly, bool a_orderByDate)
    {
        List<ForecastShipment> shipments = new ();

        if (ForecastVersionActive != null)
        {
            foreach (Forecast f in ForecastVersionActive.Forecasts)
            {
                foreach (ForecastShipment fs in f.Shipments)
                {
                    if (a_openonly && fs.GetUnconsumedQty() <= 0)
                    {
                        continue;
                    }

                    shipments.Add(fs);
                }
            }
        }

        if (a_orderByDate && shipments.Count > 0)
        {
            shipments = shipments.OrderBy(fs => fs.RequireDateTicks).ToList();
        }

        return shipments;
    }

    private StorageArea m_purchaseOrderSupplyStorageArea;
    [DoNotAuditProperty]
    public StorageArea PurchaseOrderSupplyStorageArea
    {
        get => m_purchaseOrderSupplyStorageArea;
        internal set => m_purchaseOrderSupplyStorageArea = value;
    }

    /// <summary>
    /// Remove an associated adjustment created by a demand
    /// </summary>
    /// <param name="a_demandAdjustment"></param>
    /// <exception cref="NotImplementedException"></exception>
    internal void DeleteDemand(Adjustment a_demandAdjustment)
    {
        m_adjustments.RemoveObject(a_demandAdjustment);
        m_cachedAdjustments = null;
    }
}