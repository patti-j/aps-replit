using PT.SchedulerDefinitions;
using PT.Transmissions;
using static PT.ERPTransmissions.PtImportDataSet;

namespace PT.ERPTransmissions;

public partial class WarehouseT
{
    public class Inventory : IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 527;

        public Inventory(IReader a_reader)
        {
            if (a_reader.VersionNumber >= 12547)
            {
                m_bools = new BoolVector32(a_reader);
                m_boolsIsSet = new BoolVector32(a_reader);

                a_reader.Read(out m_itemExternalId);
                a_reader.Read(out m_poSupplyStorageAreaExternalId);
                a_reader.Read(out m_safetyStock);
                a_reader.Read(out m_safetyStockSet);
                a_reader.Read(out m_bufferStock);
                a_reader.Read(out m_bufferStockSet);
                a_reader.Read(out m_safetyStockWarningLevel);
                a_reader.Read(out m_safetyStockWarningLevelSet);
                a_reader.Read(out m_itemExternalId);
                a_reader.Read(out m_plannerExternalId);
                a_reader.Read(out m_plannerIdSet);
                a_reader.Read(out m_leadTimeTicks);
                a_reader.Read(out m_leadTimeSet);
                a_reader.Read(out m_storageCapacity);
                a_reader.Read(out m_storageCapacitySet);
                a_reader.Read(out m_safetyStockJobPriority);
                a_reader.Read(out m_safetyStockJobPrioritySet);
                a_reader.Read(out m_maxInventory);
                a_reader.Read(out m_maxInventorySet);

                a_reader.Read(out int val);
                m_forecastConsumption = (InventoryDefs.ForecastConsumptionMethods)val;
                a_reader.Read(out m_forecastConsumptionSet);
                a_reader.Read(out m_forecsatConsumptionWindowDays);
                a_reader.Read(out m_forecastConsumptionWindowDaysSet);

                a_reader.Read(out val);
                if (val == 3) // SetJobNeedDatesOnly which was removed. Set to Ignore.
                {
                    val = 0;
                }

                m_mrpProcessing = (InventoryDefs.MrpProcessing)val;
                a_reader.Read(out m_mrpProcessingSet);

                a_reader.Read(out m_templateJobExternalId);
                a_reader.Read(out m_templateJobExternalIdSet);
                a_reader.Read(out m_receivingBuffer);
                a_reader.Read(out m_receivingBufferSet);
                a_reader.Read(out m_shippingBuffer);
                a_reader.Read(out m_shippingBufferSet);

                a_reader.Read(out int nbrOfLots);

                for (int lotI = 0; lotI < nbrOfLots; ++lotI)
                {
                    Lot lot = new(a_reader);
                    Lots.Add(lot);
                }

                a_reader.Read(out m_autoGenerateForecasts);
                a_reader.Read(out val);
                m_forecastInterval = (DateInterval.EInterval)val;
                a_reader.Read(out m_numberOfIntervalsToForecast);
                a_reader.Read(out val);
                m_mrpExcessQuantityAllocation = (InventoryDefs.MrpExcessQuantityAllocation)val;
                a_reader.Read(out m_mrpExcessQuantityAllocationSet);
                a_reader.Read(out m_daysOnHandTicks);
                a_reader.Read(out m_daysOnHandSet);
                a_reader.Read(out m_replenishmentMin);
                a_reader.Read(out m_replenishmentMinSet);
                a_reader.Read(out m_replenishmentMax);
                a_reader.Read(out m_replenishmentMaxSet);
                a_reader.Read(out m_replenishmentContractDays);
                a_reader.Read(out m_replenishmentContractDaysSet);
                a_reader.Read(out val);
                m_materialAllocation = (ItemDefs.MaterialAllocation)val;
                a_reader.Read(out m_materialAllocationSet);
            }
            else if (a_reader.VersionNumber >= 12511)
            {
                m_bools = new BoolVector32(a_reader);
                m_boolsIsSet = new BoolVector32(a_reader);

                a_reader.Read(out m_itemExternalId);
                a_reader.Read(out m_safetyStock);
                a_reader.Read(out m_safetyStockSet);
                a_reader.Read(out m_bufferStock);
                a_reader.Read(out m_bufferStockSet);
                a_reader.Read(out m_safetyStockWarningLevel);
                a_reader.Read(out m_safetyStockWarningLevelSet);
                a_reader.Read(out m_itemExternalId);
                a_reader.Read(out m_plannerExternalId);
                a_reader.Read(out m_plannerIdSet);
                a_reader.Read(out m_leadTimeTicks);
                a_reader.Read(out m_leadTimeSet);
                a_reader.Read(out m_storageCapacity);
                a_reader.Read(out m_storageCapacitySet);
                a_reader.Read(out m_safetyStockJobPriority);
                a_reader.Read(out m_safetyStockJobPrioritySet);
                a_reader.Read(out m_maxInventory);
                a_reader.Read(out m_maxInventorySet);

                a_reader.Read(out int val);
                m_forecastConsumption = (InventoryDefs.ForecastConsumptionMethods)val;
                a_reader.Read(out m_forecastConsumptionSet);
                a_reader.Read(out m_forecsatConsumptionWindowDays);
                a_reader.Read(out m_forecastConsumptionWindowDaysSet);

                a_reader.Read(out val);
                if (val == 3) // SetJobNeedDatesOnly which was removed. Set to Ignore.
                {
                    val = 0;
                }

                m_mrpProcessing = (InventoryDefs.MrpProcessing)val;
                a_reader.Read(out m_mrpProcessingSet);

                a_reader.Read(out m_templateJobExternalId);
                a_reader.Read(out m_templateJobExternalIdSet);
                a_reader.Read(out m_receivingBuffer);
                a_reader.Read(out m_receivingBufferSet);
                a_reader.Read(out m_shippingBuffer);
                a_reader.Read(out m_shippingBufferSet);

                a_reader.Read(out int nbrOfLots);

                for (int lotI = 0; lotI < nbrOfLots; ++lotI)
                {
                    Lot lot = new (a_reader);
                    Lots.Add(lot);
                }

                a_reader.Read(out m_autoGenerateForecasts);
                a_reader.Read(out val);
                m_forecastInterval = (DateInterval.EInterval)val;
                a_reader.Read(out m_numberOfIntervalsToForecast);
                a_reader.Read(out val);
                m_mrpExcessQuantityAllocation = (InventoryDefs.MrpExcessQuantityAllocation)val;
                a_reader.Read(out m_mrpExcessQuantityAllocationSet);
                a_reader.Read(out m_daysOnHandTicks);
                a_reader.Read(out m_daysOnHandSet);
                a_reader.Read(out m_replenishmentMin);
                a_reader.Read(out m_replenishmentMinSet);
                a_reader.Read(out m_replenishmentMax);
                a_reader.Read(out m_replenishmentMaxSet);
                a_reader.Read(out m_replenishmentContractDays);
                a_reader.Read(out m_replenishmentContractDaysSet);
                a_reader.Read(out val);
                m_materialAllocation = (ItemDefs.MaterialAllocation)val;
                a_reader.Read(out m_materialAllocationSet);
            }
            else if (a_reader.VersionNumber >= 12048) //m_bools from V11
            {
                m_bools = new BoolVector32(a_reader);
                m_boolsIsSet = new BoolVector32(a_reader);

                a_reader.Read(out m_itemExternalId);
                a_reader.Read(out m_safetyStock);
                a_reader.Read(out m_safetyStockSet);
                a_reader.Read(out m_bufferStock);
                a_reader.Read(out m_bufferStockSet);
                a_reader.Read(out m_safetyStockWarningLevel);
                a_reader.Read(out m_safetyStockWarningLevelSet);
                a_reader.Read(out decimal mOnHandQty);
                a_reader.Read(out bool mOnHandQtySet);
                a_reader.Read(out m_itemExternalId);
                a_reader.Read(out m_plannerExternalId);
                a_reader.Read(out m_plannerIdSet);
                a_reader.Read(out m_leadTimeTicks);
                a_reader.Read(out m_leadTimeSet);
                a_reader.Read(out m_storageCapacity);
                a_reader.Read(out m_storageCapacitySet);
                a_reader.Read(out m_safetyStockJobPriority);
                a_reader.Read(out m_safetyStockJobPrioritySet);
                a_reader.Read(out m_maxInventory);
                a_reader.Read(out m_maxInventorySet);

                int val;
                a_reader.Read(out val);
                m_forecastConsumption = (InventoryDefs.ForecastConsumptionMethods)val;
                a_reader.Read(out m_forecastConsumptionSet);
                a_reader.Read(out m_forecsatConsumptionWindowDays);
                a_reader.Read(out m_forecastConsumptionWindowDaysSet);

                a_reader.Read(out val);
                if (val == 3) // SetJobNeedDatesOnly which was removed. Set to Ignore.
                {
                    val = 0;
                }

                m_mrpProcessing = (InventoryDefs.MrpProcessing)val;
                a_reader.Read(out m_mrpProcessingSet);

                a_reader.Read(out m_templateJobExternalId);
                a_reader.Read(out m_templateJobExternalIdSet);
                a_reader.Read(out m_receivingBuffer);
                a_reader.Read(out m_receivingBufferSet);
                a_reader.Read(out m_shippingBuffer);
                a_reader.Read(out m_shippingBufferSet);

                a_reader.Read(out int nbrOfLots);

                for (int lotI = 0; lotI < nbrOfLots; ++lotI)
                {
                    Lot lot = new (a_reader);
                    Lots.Add(lot);
                }

                a_reader.Read(out m_autoGenerateForecasts);
                a_reader.Read(out val);
                m_forecastInterval = (DateInterval.EInterval)val;
                a_reader.Read(out m_numberOfIntervalsToForecast);
                a_reader.Read(out val);
                m_mrpExcessQuantityAllocation = (InventoryDefs.MrpExcessQuantityAllocation)val;
                a_reader.Read(out m_mrpExcessQuantityAllocationSet);
                a_reader.Read(out m_daysOnHandTicks);
                a_reader.Read(out m_daysOnHandSet);
                a_reader.Read(out m_replenishmentMin);
                a_reader.Read(out m_replenishmentMinSet);
                a_reader.Read(out m_replenishmentMax);
                a_reader.Read(out m_replenishmentMaxSet);
                a_reader.Read(out m_replenishmentContractDays);
                a_reader.Read(out m_replenishmentContractDaysSet);
                a_reader.Read(out val);
                m_materialAllocation = (ItemDefs.MaterialAllocation)val;
                a_reader.Read(out m_materialAllocationSet);
            }
            else if (a_reader.VersionNumber >= 12000) //m_bools Backwards compatibility of older V12
            {
                a_reader.Read(out m_itemExternalId);
                a_reader.Read(out m_safetyStock);
                a_reader.Read(out m_safetyStockSet);
                a_reader.Read(out m_bufferStock);
                a_reader.Read(out m_bufferStockSet);
                a_reader.Read(out m_safetyStockWarningLevel);
                a_reader.Read(out m_safetyStockWarningLevelSet);
                a_reader.Read(out decimal mOnHandQty);
                a_reader.Read(out bool mOnHandQtySet);
                a_reader.Read(out m_itemExternalId);
                a_reader.Read(out m_plannerExternalId);
                a_reader.Read(out m_plannerIdSet);
                a_reader.Read(out m_leadTimeTicks);
                a_reader.Read(out m_leadTimeSet);
                a_reader.Read(out m_storageCapacity);
                a_reader.Read(out m_storageCapacitySet);
                a_reader.Read(out m_safetyStockJobPriority);
                a_reader.Read(out m_safetyStockJobPrioritySet);
                a_reader.Read(out m_maxInventory);
                a_reader.Read(out m_maxInventorySet);

                int val;
                a_reader.Read(out val);
                m_forecastConsumption = (InventoryDefs.ForecastConsumptionMethods)val;
                a_reader.Read(out m_forecastConsumptionSet);
                a_reader.Read(out m_forecsatConsumptionWindowDays);
                a_reader.Read(out m_forecastConsumptionWindowDaysSet);

                a_reader.Read(out val);
                if (val == 3) // SetJobNeedDatesOnly which was removed. Set to Ignore.
                {
                    val = 0;
                }

                m_mrpProcessing = (InventoryDefs.MrpProcessing)val;
                a_reader.Read(out m_mrpProcessingSet);

                a_reader.Read(out m_templateJobExternalId);
                a_reader.Read(out m_templateJobExternalIdSet);
                a_reader.Read(out m_receivingBuffer);
                a_reader.Read(out m_receivingBufferSet);
                a_reader.Read(out m_shippingBuffer);
                a_reader.Read(out m_shippingBufferSet);

                a_reader.Read(out int nbrOfLots);

                for (int lotI = 0; lotI < nbrOfLots; ++lotI)
                {
                    Lot lot = new (a_reader);
                    Lots.Add(lot);
                }

                a_reader.Read(out m_autoGenerateForecasts);
                a_reader.Read(out val);
                m_forecastInterval = (DateInterval.EInterval)val;
                a_reader.Read(out m_numberOfIntervalsToForecast);
                a_reader.Read(out val);
                m_mrpExcessQuantityAllocation = (InventoryDefs.MrpExcessQuantityAllocation)val;
                a_reader.Read(out m_mrpExcessQuantityAllocationSet);
                a_reader.Read(out m_daysOnHandTicks);
                a_reader.Read(out m_daysOnHandSet);
                a_reader.Read(out m_replenishmentMin);
                a_reader.Read(out m_replenishmentMinSet);
                a_reader.Read(out m_replenishmentMax);
                a_reader.Read(out m_replenishmentMaxSet);
                a_reader.Read(out m_replenishmentContractDays);
                a_reader.Read(out m_replenishmentContractDaysSet);
                a_reader.Read(out val);
                m_materialAllocation = (ItemDefs.MaterialAllocation)val;
                a_reader.Read(out m_materialAllocationSet);
            }
            else if (a_reader.VersionNumber >= 755) //Bool vectors from V11
            {
                m_bools = new BoolVector32(a_reader);
                m_boolsIsSet = new BoolVector32(a_reader);

                a_reader.Read(out m_itemExternalId);
                a_reader.Read(out m_safetyStock);
                a_reader.Read(out m_safetyStockSet);
                a_reader.Read(out m_bufferStock);
                a_reader.Read(out m_bufferStockSet);
                a_reader.Read(out m_safetyStockWarningLevel);
                a_reader.Read(out m_safetyStockWarningLevelSet);
                a_reader.Read(out decimal mOnHandQty);
                a_reader.Read(out bool mOnHandQtySet);
                a_reader.Read(out m_itemExternalId);
                a_reader.Read(out m_plannerExternalId);
                a_reader.Read(out m_plannerIdSet);
                a_reader.Read(out m_leadTimeTicks);
                a_reader.Read(out m_leadTimeSet);
                a_reader.Read(out m_storageCapacity);
                a_reader.Read(out m_storageCapacitySet);
                a_reader.Read(out m_safetyStockJobPriority);
                a_reader.Read(out m_safetyStockJobPrioritySet);
                a_reader.Read(out m_maxInventory);
                a_reader.Read(out m_maxInventorySet);

                int val;
                a_reader.Read(out val);
                m_forecastConsumption = (InventoryDefs.ForecastConsumptionMethods)val;
                a_reader.Read(out m_forecastConsumptionSet);
                a_reader.Read(out m_forecsatConsumptionWindowDays);
                a_reader.Read(out m_forecastConsumptionWindowDaysSet);

                a_reader.Read(out val);
                if (val == 3) // SetJobNeedDatesOnly which was removed. Set to Ignore.
                {
                    val = 0;
                }

                m_mrpProcessing = (InventoryDefs.MrpProcessing)val;
                a_reader.Read(out m_mrpProcessingSet);

                a_reader.Read(out m_templateJobExternalId);
                a_reader.Read(out m_templateJobExternalIdSet);
                a_reader.Read(out m_receivingBuffer);
                a_reader.Read(out m_receivingBufferSet);
                a_reader.Read(out m_shippingBuffer);
                a_reader.Read(out m_shippingBufferSet);

                a_reader.Read(out int nbrOfLots);

                for (int lotI = 0; lotI < nbrOfLots; ++lotI)
                {
                    Lot lot = new (a_reader);
                    Lots.Add(lot);
                }

                a_reader.Read(out m_autoGenerateForecasts);
                a_reader.Read(out val);
                m_forecastInterval = (DateInterval.EInterval)val;
                a_reader.Read(out m_numberOfIntervalsToForecast);
                a_reader.Read(out val);
                m_mrpExcessQuantityAllocation = (InventoryDefs.MrpExcessQuantityAllocation)val;
                a_reader.Read(out m_mrpExcessQuantityAllocationSet);
                a_reader.Read(out m_daysOnHandTicks);
                a_reader.Read(out m_daysOnHandSet);
                a_reader.Read(out m_replenishmentMin);
                a_reader.Read(out m_replenishmentMinSet);
                a_reader.Read(out m_replenishmentMax);
                a_reader.Read(out m_replenishmentMaxSet);
                a_reader.Read(out m_replenishmentContractDays);
                a_reader.Read(out m_replenishmentContractDaysSet);
                a_reader.Read(out val);
                m_materialAllocation = (ItemDefs.MaterialAllocation)val;
                a_reader.Read(out m_materialAllocationSet);
            }
            else if (a_reader.VersionNumber >= 731)
            {
                a_reader.Read(out m_itemExternalId);
                a_reader.Read(out m_safetyStock);
                a_reader.Read(out m_safetyStockSet);
                a_reader.Read(out m_bufferStock);
                a_reader.Read(out m_bufferStockSet);
                a_reader.Read(out m_safetyStockWarningLevel);
                a_reader.Read(out m_safetyStockWarningLevelSet);
                a_reader.Read(out decimal mOnHandQty);
                a_reader.Read(out bool mOnHandQtySet);
                a_reader.Read(out m_itemExternalId);
                a_reader.Read(out m_plannerExternalId);
                a_reader.Read(out m_plannerIdSet);
                a_reader.Read(out m_leadTimeTicks);
                a_reader.Read(out m_leadTimeSet);
                a_reader.Read(out m_storageCapacity);
                a_reader.Read(out m_storageCapacitySet);
                a_reader.Read(out m_safetyStockJobPriority);
                a_reader.Read(out m_safetyStockJobPrioritySet);
                a_reader.Read(out m_maxInventory);
                a_reader.Read(out m_maxInventorySet);

                int val;
                a_reader.Read(out val);
                m_forecastConsumption = (InventoryDefs.ForecastConsumptionMethods)val;
                a_reader.Read(out m_forecastConsumptionSet);
                a_reader.Read(out m_forecsatConsumptionWindowDays);
                a_reader.Read(out m_forecastConsumptionWindowDaysSet);

                a_reader.Read(out val);
                if (val == 3) // SetJobNeedDatesOnly which was removed. Set to Ignore.
                {
                    val = 0;
                }

                m_mrpProcessing = (InventoryDefs.MrpProcessing)val;
                a_reader.Read(out m_mrpProcessingSet);

                a_reader.Read(out m_templateJobExternalId);
                a_reader.Read(out m_templateJobExternalIdSet);
                a_reader.Read(out m_receivingBuffer);
                a_reader.Read(out m_receivingBufferSet);
                a_reader.Read(out m_shippingBuffer);
                a_reader.Read(out m_shippingBufferSet);

                a_reader.Read(out int nbrOfLots);

                for (int lotI = 0; lotI < nbrOfLots; ++lotI)
                {
                    Lot lot = new (a_reader);
                    Lots.Add(lot);
                }

                a_reader.Read(out m_autoGenerateForecasts);
                a_reader.Read(out val);
                m_forecastInterval = (DateInterval.EInterval)val;
                a_reader.Read(out m_numberOfIntervalsToForecast);
                a_reader.Read(out val);
                m_mrpExcessQuantityAllocation = (InventoryDefs.MrpExcessQuantityAllocation)val;
                a_reader.Read(out m_mrpExcessQuantityAllocationSet);
                a_reader.Read(out m_daysOnHandTicks);
                a_reader.Read(out m_daysOnHandSet);
                a_reader.Read(out m_replenishmentMin);
                a_reader.Read(out m_replenishmentMinSet);
                a_reader.Read(out m_replenishmentMax);
                a_reader.Read(out m_replenishmentMaxSet);
                a_reader.Read(out m_replenishmentContractDays);
                a_reader.Read(out m_replenishmentContractDaysSet);
                a_reader.Read(out val);
                m_materialAllocation = (ItemDefs.MaterialAllocation)val;
                a_reader.Read(out m_materialAllocationSet);
            }
        }

        public void Serialize(IWriter a_writer)
        {
            m_bools.Serialize(a_writer);
            m_boolsIsSet.Serialize(a_writer);

            a_writer.Write(m_itemExternalId);
            a_writer.Write(m_poSupplyStorageAreaExternalId);
            a_writer.Write(m_safetyStock);
            a_writer.Write(m_safetyStockSet);
            a_writer.Write(m_bufferStock);
            a_writer.Write(m_bufferStockSet);
            a_writer.Write(m_safetyStockWarningLevel);
            a_writer.Write(m_safetyStockWarningLevelSet);
            a_writer.Write(m_itemExternalId);
            a_writer.Write(m_plannerExternalId);
            a_writer.Write(m_plannerIdSet);
            a_writer.Write(m_leadTimeTicks);
            a_writer.Write(m_leadTimeSet);
            a_writer.Write(m_storageCapacity);
            a_writer.Write(m_storageCapacitySet);
            a_writer.Write(m_safetyStockJobPriority);
            a_writer.Write(m_safetyStockJobPrioritySet);
            a_writer.Write(m_maxInventory);
            a_writer.Write(m_maxInventorySet);

            a_writer.Write((int)m_forecastConsumption);
            a_writer.Write(m_forecastConsumptionSet);
            a_writer.Write(m_forecsatConsumptionWindowDays);
            a_writer.Write(m_forecastConsumptionWindowDaysSet);

            a_writer.Write((int)m_mrpProcessing);
            a_writer.Write(m_mrpProcessingSet);
            a_writer.Write(m_templateJobExternalId);
            a_writer.Write(m_templateJobExternalIdSet);
            a_writer.Write(m_receivingBuffer);
            a_writer.Write(m_receivingBufferSet);
            a_writer.Write(m_shippingBuffer);
            a_writer.Write(m_shippingBufferSet);

            int nbrOfLots;
            if (Lots != null && Lots.Count != 0)
            {
                nbrOfLots = Lots.Count;
                a_writer.Write(nbrOfLots);
                for (int lotI = 0; lotI < Lots.Count; ++lotI)
                {
                    Lots[lotI].Serialize(a_writer);
                }
            }
            else
            {
                nbrOfLots = 0;
                a_writer.Write(nbrOfLots);
            }

            a_writer.Write(m_autoGenerateForecasts);
            a_writer.Write((int)m_forecastInterval);
            a_writer.Write(m_numberOfIntervalsToForecast);
            a_writer.Write((int)m_mrpExcessQuantityAllocation);
            a_writer.Write(m_mrpExcessQuantityAllocationSet);
            a_writer.Write(m_daysOnHandTicks);
            a_writer.Write(m_daysOnHandSet);
            a_writer.Write(m_replenishmentMin);
            a_writer.Write(m_replenishmentMinSet);
            a_writer.Write(m_replenishmentMax);
            a_writer.Write(m_replenishmentMaxSet);
            a_writer.Write(m_replenishmentContractDays);
            a_writer.Write(m_replenishmentContractDaysSet);
            a_writer.Write((int)m_materialAllocation);
            a_writer.Write(m_materialAllocationSet);
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public Inventory(string a_itemExternalId)
        {
            ItemExternalId = a_itemExternalId;
        }

        public Inventory(PtImportDataSet.InventoriesRow a_row)
        {
            ItemExternalId = a_row.ItemExternalId;

            if (!a_row.IsPurchaseOrderSupplyStorageAreaExternalIdNull())
            {
                POSupplyStorageAreaExternalId = a_row.PurchaseOrderSupplyStorageAreaExternalId;
            }

            if (!a_row.IsLeadTimeDaysNull())
            {
                LeadTime = TimeSpan.FromDays(a_row.LeadTimeDays);
            }
            
            if (!a_row.IsPlannerExternalIdNull())
            {
                PlannerExternalId = a_row.PlannerExternalId;
            }

            if (!a_row.IsSafetyStockNull())
            {
                SafetyStock = a_row.SafetyStock;
            }

            if (!a_row.IsSafetyStockWarningLevelNull())
            {
                SafetyStockWarningLevel = a_row.SafetyStockWarningLevel;
            }

            if (!a_row.IsMaxInventoryNull())
            {
                MaxInventory = a_row.MaxInventory;
            }

            if (!a_row.IsStorageCapacityNull())
            {
                StorageCapacity = a_row.StorageCapacity;
            }

            if (!a_row.IsSafetyStockJobPriorityNull())
            {
                SafetyStockJobPriority = a_row.SafetyStockJobPriority;
            }

            if (!a_row.IsForecastConsumptionNull())
            {
                try
                {
                    ForecastConsumption = (InventoryDefs.ForecastConsumptionMethods)Enum.Parse(typeof(InventoryDefs.ForecastConsumptionMethods), a_row.ForecastConsumption);
                }
                catch (Exception err)
                {
                    throw new APSCommon.PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            a_row.ForecastConsumption, "Inventory", "ForecastConsumption",
                            string.Join(", ", Enum.GetNames(typeof(InventoryDefs.ForecastConsumptionMethods)))
                        });
                }
            }

            if (!a_row.IsForecastConsumptionWindowDaysNull())
            {
                ForecastConsumptionWindowDays = a_row.ForecastConsumptionWindowDays;
            }

            if (!a_row.IsMrpProcessingNull())
            {
                try
                {
                    MrpProcessing = (InventoryDefs.MrpProcessing)Enum.Parse(typeof(InventoryDefs.MrpProcessing), a_row.MrpProcessing);
                }
                catch (Exception err)
                {
                    throw new APSCommon.PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            a_row.MrpProcessing, "Inventory", "MrpProcessing",
                            string.Join(", ", Enum.GetNames(typeof(InventoryDefs.MrpProcessing)))
                        });
                }
            }

            if (!a_row.IsMrpExcessQuantityAllocationNull())
            {
                try
                {
                    MrpExcessQuantityAllocation = (InventoryDefs.MrpExcessQuantityAllocation)Enum.Parse(typeof(InventoryDefs.MrpExcessQuantityAllocation), a_row.MrpExcessQuantityAllocation);
                }
                catch (Exception err)
                {
                    throw new APSCommon.PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            a_row.MrpExcessQuantityAllocation, "Inventory", "MrpExcessQuantityAllocation",
                            string.Join(", ", Enum.GetNames(typeof(InventoryDefs.MrpExcessQuantityAllocation)))
                        });
                }
            }

            if (!a_row.IsTemplateJobExternalIdNull())
            {
                TemplateJobExternalId = a_row.TemplateJobExternalId;
            }

            if (!a_row.IsReceivingBufferDaysNull())
            {
                ReceivingBuffer = TimeSpan.FromDays(a_row.ReceivingBufferDays);
            }

            if (!a_row.IsShippingBufferDaysNull())
            {
                ShippingBuffer = TimeSpan.FromDays(a_row.ShippingBufferDays);
            }

            AddLots(a_row);
            if (!a_row.IsBufferStockNull())
            {
                BufferStock = a_row.BufferStock;
            }

            if (!a_row.IsAutoGenerateForecastsNull())
            {
                AutoGenerateForecasts = a_row.AutoGenerateForecasts;
            }

            if (!a_row.IsForecastIntervalNull())
            {
                try
                {
                    ForecastInterval = (DateInterval.EInterval)Enum.Parse(typeof(DateInterval.EInterval), a_row.ForecastInterval);
                }
                catch (Exception err)
                {
                    throw new APSCommon.PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            a_row.ForecastInterval, "Inventory", "ForecastInterval",
                            string.Join(", ", Enum.GetNames(typeof(DateInterval.EInterval)))
                        });
                }
            }

            if (!a_row.IsNumberOfIntervalsToForecastNull())
            {
                NumberOfIntervalsToForecast = a_row.NumberOfIntervalsToForecast;
            }

            if (!a_row.IsMaterialAllocationNull())
            {
                MaterialAllocation = (ItemDefs.MaterialAllocation)Enum.Parse(typeof(ItemDefs.MaterialAllocation), a_row.MaterialAllocation);
            }

            if (!a_row.IsPreventSharedBatchOverflowNull())
            {
                PreventSharedBatchOverflow = a_row.PreventSharedBatchOverflow;
            }
        }

        #region Shared Properties
        private string m_itemExternalId;

        public string ItemExternalId
        {
            get => m_itemExternalId;
            set => m_itemExternalId = value;
        }

        private string m_poSupplyStorageAreaExternalId;

        public string POSupplyStorageAreaExternalId
        {
            get => m_poSupplyStorageAreaExternalId;
            set => m_poSupplyStorageAreaExternalId = value;
        }

        private long m_leadTimeTicks;

        /// <summary>
        /// The amount of time it takes between when the Item is ordered and delivered for this Warehouse.
        /// </summary>
        public TimeSpan LeadTime
        {
            get => TimeSpan.FromTicks(m_leadTimeTicks);
            set
            {
                m_leadTimeTicks = value.Ticks;
                m_leadTimeSet = true;
            }
        }

        private bool m_leadTimeSet;

        public bool LeadTimeSet => m_leadTimeSet;

        private decimal m_safetyStock;

        /// <summary>
        /// The target level of this Item to have on hand at this Warehouse to provide the desired service level to customers.
        /// </summary>
        public decimal SafetyStock
        {
            get => m_safetyStock;
            set
            {
                m_safetyStock = value;
                m_safetyStockSet = true;
            }
        }

        private bool m_safetyStockSet;

        public bool SafetyStockSet => m_safetyStockSet;

        private decimal m_bufferStock;

        /// <summary>
        /// The target level of this Item to have on hand at this Warehouse to support Make-to-Available inventory management.
        /// This value is used to calculate Buffer Stock penetration levels.
        /// </summary>
        public decimal BufferStock
        {
            get => m_bufferStock;
            set
            {
                m_bufferStock = value;
                m_bufferStockSet = true;
            }
        }

        private bool m_bufferStockSet;

        public bool BufferStockSet => m_bufferStockSet;

        private decimal m_safetyStockWarningLevel;

        /// <summary>
        /// If On-Hand inventory falls below this amount then Safety Stock Jobs may be flagged as Late.  The purpose of this is to only highlight late safety stock Jobs when the safety stock is significantly
        /// short.
        /// This controls the "Late" setting for safety stock Jobs only.
        /// </summary>
        public decimal SafetyStockWarningLevel
        {
            get => m_safetyStockWarningLevel;
            set
            {
                m_safetyStockWarningLevel = value;
                m_safetyStockWarningLevelSet = true;
            }
        }

        private bool m_safetyStockWarningLevelSet;

        public bool SafetyStockWarningLevelSet => m_safetyStockWarningLevelSet;

        private decimal m_maxInventory;

        /// <summary>
        /// The maximum amount of inventory that can be created and stored.  This can be used to alert the planner when there will be too much inventory created and it will occupy too much space or consume too
        /// much capital.
        /// </summary>
        public decimal MaxInventory
        {
            get => m_maxInventory;
            set
            {
                m_maxInventory = value;
                m_maxInventorySet = true;
            }
        }

        private bool m_maxInventorySet;

        public bool MaxInventorySet => m_maxInventorySet;

        private decimal m_storageCapacity;

        /// <summary>
        /// Can be used by a Scheduling Add-In to limit scheduling based upon availability of storage space.
        /// </summary>
        public decimal StorageCapacity
        {
            get => m_storageCapacity;
            set
            {
                m_storageCapacity = value;
                m_storageCapacitySet = true;
            }
        }

        private bool m_storageCapacitySet;

        public bool StorageCapacitySet => m_storageCapacitySet;

        private string m_plannerExternalId;

        /// <summary>
        /// The Id of the Planner responsible for monitoring and managing this Items stock level at this Warehouse.
        /// </summary>
        public string PlannerExternalId
        {
            get => m_plannerExternalId;
            set
            {
                m_plannerExternalId = value;
                m_plannerIdSet = true;
            }
        }

        private bool m_plannerIdSet;

        public bool PlannerIdSet => m_plannerIdSet;

        private InventoryDefs.ForecastConsumptionMethods m_forecastConsumption = InventoryDefs.ForecastConsumptionMethods.None;

        /// <summary>
        /// Options for how Forecast quantities are reduced based on actual demand.
        /// </summary>
        public InventoryDefs.ForecastConsumptionMethods ForecastConsumption
        {
            get => m_forecastConsumption;
            set
            {
                m_forecastConsumption = value;
                m_forecastConsumptionSet = true;
            }
        }

        private bool m_forecastConsumptionSet;

        public bool ForecastConsumptionSet => m_forecastConsumptionSet;

        private double m_forecsatConsumptionWindowDays;

        public double ForecastConsumptionWindowDays
        {
            get => m_forecsatConsumptionWindowDays;
            set
            {
                m_forecsatConsumptionWindowDays = value;
                m_forecastConsumptionWindowDaysSet = true;
            }
        }

        private bool m_forecastConsumptionWindowDaysSet;

        public bool ForecastConsumptionWindowDaysSet => m_forecastConsumptionWindowDaysSet;

        private int m_safetyStockJobPriority = 5;

        /// <summary>
        /// Sets the Priority for Jobs that are generated to satisfy Safety Stock.
        /// </summary>
        public int SafetyStockJobPriority
        {
            get => m_safetyStockJobPriority;
            set
            {
                m_safetyStockJobPriority = value;
                m_safetyStockJobPrioritySet = true;
            }
        }

        private bool m_safetyStockJobPrioritySet;

        public bool SafetyStockJobPrioritySet => m_safetyStockJobPrioritySet;

        private InventoryDefs.MrpProcessing m_mrpProcessing;

        /// <summary>
        /// Specifies how the MRP process will manage this inventory.
        /// </summary>
        public InventoryDefs.MrpProcessing MrpProcessing
        {
            get => m_mrpProcessing;
            set
            {
                m_mrpProcessing = value;
                m_mrpProcessingSet = true;
            }
        }

        private bool m_mrpProcessingSet;

        public bool MrpProcessingSet => m_mrpProcessingSet;

        private InventoryDefs.MrpExcessQuantityAllocation m_mrpExcessQuantityAllocation;

        /// <summary>
        /// Specifies how the MRP process will manage this inventory.
        /// </summary>
        public InventoryDefs.MrpExcessQuantityAllocation MrpExcessQuantityAllocation
        {
            get => m_mrpExcessQuantityAllocation;
            set
            {
                m_mrpExcessQuantityAllocation = value;
                m_mrpExcessQuantityAllocationSet = true;
            }
        }

        private bool m_mrpExcessQuantityAllocationSet;

        public bool MrpExcessQuantityAllocationSet => m_mrpExcessQuantityAllocationSet;

        private string m_templateJobExternalId;

        /// <summary>
        /// Can be used to specify the Job to be used as an MRP Template.
        /// </summary>
        public string TemplateJobExternalId
        {
            get => m_templateJobExternalId;
            set
            {
                m_templateJobExternalId = value;
                m_templateJobExternalIdSet = true;
            }
        }

        private bool m_templateJobExternalIdSet;

        public bool TemplateJobExternalIdSet => m_templateJobExternalIdSet;

        private TimeSpan m_receivingBuffer;

        /// <summary>
        /// Purchased Material should be received this amount of time before the P.O. Scheduled Receipt Date.
        /// </summary>
        public TimeSpan ReceivingBuffer
        {
            get => m_receivingBuffer;
            set
            {
                m_receivingBuffer = value;
                m_receivingBufferSet = true;
            }
        }

        private bool m_receivingBufferSet;

        public bool ReceivingBufferSet => m_receivingBufferSet;

        private TimeSpan m_shippingBuffer;

        /// <summary>
        /// Production should be completed this amount of time before the M.O. Need Date.
        /// </summary>
        public TimeSpan ShippingBuffer
        {
            get => m_shippingBuffer;
            set
            {
                m_shippingBuffer = value;
                m_shippingBufferSet = true;
            }
        }

        private bool m_shippingBufferSet;

        public bool ShippingBufferSet => m_shippingBufferSet;

        private bool m_autoGenerateForecasts;

        public bool AutoGenerateForecasts
        {
            get => m_autoGenerateForecasts;
            set => m_autoGenerateForecasts = value;
        }

        private DateInterval.EInterval m_forecastInterval;

        public DateInterval.EInterval ForecastInterval
        {
            get => m_forecastInterval;
            set => m_forecastInterval = value;
        }

        private int m_numberOfIntervalsToForecast;

        public int NumberOfIntervalsToForecast
        {
            get => m_numberOfIntervalsToForecast;
            set => m_numberOfIntervalsToForecast = value;
        }

        private long m_daysOnHandTicks;

        /// <summary>
        /// Preferred days on hand timespan.
        /// </summary>
        public TimeSpan DaysOnHand
        {
            get => TimeSpan.FromTicks(m_daysOnHandTicks);
            set
            {
                m_daysOnHandTicks = value.Ticks;
                m_daysOnHandSet = true;
            }
        }

        private bool m_daysOnHandSet;

        public bool DaysOnHandSet => m_daysOnHandSet;

        private ItemDefs.MaterialAllocation m_materialAllocation = ItemDefs.MaterialAllocation.NotSet;

        /// <summary>
        /// How material should be used either use the earliest created or the latest created.
        /// </summary>
        public ItemDefs.MaterialAllocation MaterialAllocation
        {
            get => m_materialAllocation;
            set
            {
                m_materialAllocation = value;
                m_materialAllocationSet = true;
            }
        }

        private bool m_materialAllocationSet;

        public bool MaterialAllocationSet => m_materialAllocationSet;

        private List<Lot> m_lots = new ();

        /// <summary>
        /// This is null if there are no lots.
        /// </summary>
        public List<Lot> Lots
        {
            get => m_lots;
            private set => m_lots = value;
        }

        /// <summary>
        /// Add Lots from a dataset inventory Row.
        /// </summary>
        /// <param name="a_invRow"></param>
        internal void AddLots(PtImportDataSet.InventoriesRow a_invRow)
        {
            try
            {
                if (a_invRow.ItemsRow != null)
                {
                    PtImportDataSet.LotsRow[] lotRows = a_invRow.GetLotsRows();

                    for (int j = 0; j < lotRows.Length; j++)
                    {
                        PtImportDataSet.LotsRow lot = (PtImportDataSet.LotsRow)lotRows.GetValue(j);
                        Lot invLot = new Lot(lot);
                        Lots.Add(invLot);
                    }
                }
            }
            catch (APSCommon.PTValidationException ptErr)
            {
                throw ptErr;
            }
            catch (Exception err)
            {
                throw new APSCommon.PTValidationException("2846", err, false, new object[] { a_invRow.ItemExternalId, a_invRow.WarehouseExternalId });
            }
        }

        private decimal m_replenishmentMin;

        /// <summary>
        /// Inventory level that causes MRP to generate a replenishment order.
        /// </summary>
        public decimal ReplenishmentMin
        {
            get => m_replenishmentMin;
            set
            {
                m_replenishmentMinSet = true;
                m_replenishmentMin = value;
            }
        }

        private bool m_replenishmentMinSet;
        public bool ReplenishmentMinSet => m_replenishmentMinSet;

        private decimal m_replenishmentMax;

        /// <summary>
        /// When creating replenishments, generate enough supplies to reach at least this level of inventory.
        /// </summary>
        public decimal ReplenishmentMax
        {
            get => m_replenishmentMax;
            set
            {
                m_replenishmentMaxSet = true;
                m_replenishmentMax = value;
            }
        }

        private bool m_replenishmentMaxSet;
        public bool ReplenishmentMaxSet => m_replenishmentMaxSet;

        private double m_replenishmentContractDays;

        /// <summary>
        /// When creating replenishments, generate enough supplies to reach at least this level of inventory.
        /// </summary>
        public double ReplenishmentContractDays
        {
            get => m_replenishmentContractDays;
            set
            {
                m_replenishmentContractDaysSet = true;
                m_replenishmentContractDays = value;
            }
        }

        private bool m_replenishmentContractDaysSet;
        public bool ReplenishmentContractDaysSet => m_replenishmentContractDaysSet;

        private BoolVector32 m_bools;
        private BoolVector32 m_boolsIsSet;

        private const short c_preventSharedBatchOverflowIdx = 0;
        private const short c_preventSharedBatchOverflowIsSetIdx = 0;

        public bool PreventSharedBatchOverflow
        {
            get => m_bools[c_preventSharedBatchOverflowIdx];
            set
            {
                m_bools[c_preventSharedBatchOverflowIdx] = value;
                m_boolsIsSet[c_preventSharedBatchOverflowIsSetIdx] = true;
            }
        }

        public bool PreventSharedBatchOverflowIsSet => m_boolsIsSet[c_preventSharedBatchOverflowIsSetIdx];

        public class Lot : PTObjectBase, IPTSerializable
        {
            #region Serialization
            internal Lot(IReader a_reader)
            : base(a_reader)
            {
                if (a_reader.VersionNumber >= 12527)
                {
                    m_bools = new BoolVector32(a_reader);
                    a_reader.Read(out m_productionDate);
                    a_reader.Read(out m_wear);
                    a_reader.Read(out m_code);
                    a_reader.Read(out m_expirationDate);

                    a_reader.Read(out int itemStorageLotCount);
                    for (int i = 0; i < itemStorageLotCount; i++)
                    {
                        ItemStorageLots.Add(new ItemStorageLot(a_reader));
                    }
                }
                else if (a_reader.VersionNumber >= 12511)
                {
                    m_bools = new BoolVector32(a_reader);
                    a_reader.Read(out m_productionDate);
                    a_reader.Read(out m_wear);
                    a_reader.Read(out m_code);
                    a_reader.Read(out m_expirationDate);
                }
                else if (a_reader.VersionNumber >= 708)
                {
                    m_bools = new BoolVector32(a_reader);
                    a_reader.Read(out string mExternalId);
                    a_reader.Read(out decimal mQty);
                    a_reader.Read(out m_productionDate);
                    a_reader.Read(out m_wear);
                    a_reader.Read(out m_code);
                    a_reader.Read(out m_expirationDate);
                }
            }

            public override void Serialize(IWriter a_writer)
            {
                base.Serialize(a_writer);
                m_bools.Serialize(a_writer);
                a_writer.Write(m_productionDate);
                a_writer.Write(m_wear);
                a_writer.Write(m_code);
                a_writer.Write(m_expirationDate);
                a_writer.Write(ItemStorageLots.Count);
                foreach (ItemStorageLot itemStorageLot in ItemStorageLots)
                {
                    itemStorageLot.Serialize(a_writer);
                }
            }

            public const int UNIQUE_ID = 749;

            public override int UniqueId => UNIQUE_ID;
            #endregion

            internal Lot(PtImportDataSet.LotsRow a_lotRow)
                : base(a_lotRow.ExternalId, "", "", "", a_lotRow.IsUserFieldsNull() ? null : a_lotRow.UserFields)
            {
                ExternalId = a_lotRow.ExternalId;
                //Qty = a_lotRow.Qty;
                if (!a_lotRow.IsLotProductionDateNull())
                {
                    ProductionDate = a_lotRow.LotProductionDate.ToServerTime();
                }

                if (!a_lotRow.IsCodeNull())
                {
                    Code = a_lotRow.Code;
                }

                if (!a_lotRow.IsExpirationDateNull())
                {
                    DateTime expirationDateUtc = a_lotRow.ExpirationDate;
                    if (expirationDateUtc > PTDateTime.MinDateTime)
                    {
                        ExpirationDate = expirationDateUtc.ToServerTime();
                    }
                }

                if (!a_lotRow.IsLimitMatlSrcToEligibleLotsNull())
                {
                    LimitMatlSrcToEligibleLots = a_lotRow.LimitMatlSrcToEligibleLots;
                }

                AddItemStorageLots(a_lotRow);   
            }
            private void AddItemStorageLots(LotsRow a_lotRow)
            {
                ItemStorageLotsRow[] itemStorageLotsRows = a_lotRow.GetItemStorageLotsRows();
                for (int i = 0; i < itemStorageLotsRows.Length; i++)
                {
                    ItemStorageLotsRow itemStorageLotsRow = itemStorageLotsRows[i];
                    try
                    {

                        ItemStorageLot itemStorageLot = new ItemStorageLot(itemStorageLotsRow);
                        ItemStorageLots.Add(itemStorageLot);
                    }
                    catch (APSCommon.PTValidationException ptErr)
                    {
                            throw ptErr;
                    }
                    catch (Exception err)
                    {
                        throw new APSCommon.PTValidationException("3089", err, false, new object[] { itemStorageLotsRow.StorageAreaExternalId, itemStorageLotsRow.ItemExternalId, a_lotRow.WarehouseExternalId});
                    }
                }
            }
            private BoolVector32 m_bools;

            private const short c_productionDateSetIdx = 0;
            private const short c_codeSetIdx = 1;
            private const short c_wearSetIdx = 2;
            private const short c_expirationDateSetIdx = 3;
            private const short c_limitMatlSrcToEligibleLotsIdx = 4;
            private const short c_limitMatlSrcToEligibleLotsSetIdx = 5;

            public bool LimitMatlSrcToEligibleLots
            {
                get => m_bools[c_limitMatlSrcToEligibleLotsIdx];
                private set
                {
                    m_bools[c_limitMatlSrcToEligibleLotsIdx] = value;
                    LimitMatlSrcToEligibleLotsSet = true;
                }
            }

            public bool LimitMatlSrcToEligibleLotsSet
            {
                get => m_bools[c_limitMatlSrcToEligibleLotsSetIdx];
                private set => m_bools[c_limitMatlSrcToEligibleLotsSetIdx] = value;
            }

            private int m_wear;

            public int Wear
            {
                get => m_wear;
                set
                {
                    m_wear = value;
                    m_bools[c_wearSetIdx] = true;
                }
            }

            public bool WearSet => m_bools[c_wearSetIdx];

            private DateTime m_productionDate;

            public DateTime ProductionDate
            {
                get => m_productionDate;
                set
                {
                    m_productionDate = value;
                    m_bools[c_productionDateSetIdx] = true;
                }
            }

            public bool ProductionDateSet => m_bools[c_productionDateSetIdx];

            private DateTime m_expirationDate;

            public DateTime ExpirationDate
            {
                get => m_expirationDate;
                private set
                {
                    m_expirationDate = value;
                    m_bools[c_expirationDateSetIdx] = true;
                }
            }

            public bool ExpirationDateSet => m_bools[c_expirationDateSetIdx];

            private string m_code;

            public string Code
            {
                get => m_code;
                set
                {
                    m_code = value;
                    m_bools[c_codeSetIdx] = true;
                }
            }

            public bool CodeSet => m_bools[c_codeSetIdx];
            public HashSet<ItemStorageLot> ItemStorageLots = new();
            public interface IUsability : IPTSerializable { }

        }
        #endregion
    }

    /// <summary>
    /// Transfer times between warehouses for transfer orders.
    /// The first string is the external id of the from warehouse.
    /// The second string is is the external id of the to warehouse.
    /// The long value is the transfer span between the from and to the to warehouses.
    /// Note it's possible to specify different transfer spans in the reverse direction.
    /// For instance if you specify 1 hour between A and B. You can specify a different time
    /// going in the opposite direction. For example going from B to A could take 1.5 hours.
    /// In this example if you don't specify the length of time in the reverse direction,
    /// 1 hour will be used as the Transfer Span from B to A.
    /// </summary>
    private readonly List<Tuple<string, string, long>> m_warehouseTTs = new ();

    /// <summary>
    /// Get an enumerator of transfer times between warehouses for transfer orders.
    /// The first string is the external id of the from warehouse.
    /// The second string is is the external id of the to warehouse.
    /// The long value is the transfer span between the from and to the to warehouses.
    /// </summary>
    /// <returns></returns>
    public List<Tuple<string, string, long>>.Enumerator GetWarehouseTTEnumerator()
    {
        return m_warehouseTTs.GetEnumerator();
    }
}