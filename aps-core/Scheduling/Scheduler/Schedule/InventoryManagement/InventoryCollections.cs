using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PT.APSCommon;
using PT.APSCommon.Collections;
using PT.Scheduler.Schedule.InventoryManagement;

namespace PT.Scheduler
{
    internal class InventoryList : CustomSortedDictionary<BaseId, Inventory>
    {
        //For new objects
        internal InventoryList()
            : base()
        {

        }

        internal InventoryList(IReader a_reader, BaseIdGenerator a_idGen)
            : base(a_reader, a_idGen)
        {

        }

        //Isn't used
        protected override Inventory CreateInstance(IReader a_reader)
        {
            throw new NotImplementedException();
        }

        protected override Inventory CreateInstance(IReader a_reader, BaseIdGenerator a_idGen)
        {
            if (a_reader.VersionNumber >= 12511)
            {
                return new Inventory(a_reader, a_idGen);
            }
            else
            {
                //Backwards compatibility where this used to be ItemInventory
                BaseId itemId = new BaseId(a_reader);
                return new Inventory(a_reader, a_idGen, itemId);
            }
        }
    }

    internal class ItemStorageList : CustomSortedDictionary<BaseId, ItemStorage>
    {
        //For new objects
        internal ItemStorageList()
            : base()
        {

        }

        internal ItemStorageList(IReader a_reader, BaseIdGenerator a_idGen)
            : base(a_reader, a_idGen)
        {

        }

        public ItemStorage GetByItemExternalId(string a_itemExternalId)
        {
            foreach (ItemStorage itemStorage in this)
            {
                if (itemStorage.Item?.ExternalId == a_itemExternalId)
                {
                    return itemStorage;
                }
            }

            return null;
        }

        //Isn't used
        protected override ItemStorage CreateInstance(IReader a_reader)
        {
            throw new NotImplementedException();
        }

        protected override ItemStorage CreateInstance(IReader a_reader, BaseIdGenerator a_idGen)
        {
            if (a_reader.VersionNumber >= 12511)
            {
                return new ItemStorage(a_reader);
            }

            throw new NotImplementedException();
        }

        //Ease of use accessor
        public ItemStorage this[Item a_item] => GetValue(a_item.Id);

        internal void ResetSimulationStateVariables()
        {
            foreach (ItemStorage itemStorage in this)
            {
                itemStorage.ResetSimulationStateVariables();
            }
        }

        internal void DeleteItemStorage(Warehouse a_warehouse, ItemStorageDeleteProfile a_deleteProfile)
        {
            foreach (ItemStorage itemStorage in a_deleteProfile.ItemStoragesSafeToDelete())
            {
                RemoveByKey(itemStorage.Item.Id);
            }
        }

        internal void ValidateDeleteItemStorage(JobManager a_jobs, PurchaseToStockManager a_pos, Warehouse a_warehouse, ItemStorageDeleteProfile a_deleteProfile)
        {
            a_pos.ValidateItemStorageDelete(a_deleteProfile);
            a_jobs.ValidateItemStorageDelete(a_deleteProfile);
            a_warehouse.ValidateItemStorageDelete(a_deleteProfile);
        }
    }
}
