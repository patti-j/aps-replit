using System.Collections;
using System.ComponentModel;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.ERPTransmissions;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of Item objects.
/// </summary>
public partial class ItemManager : ScenarioBaseObjectManager<Item>, IPTSerializable
{
    #region IPTSerializable Members
    public ItemManager(IReader reader, BaseIdGenerator idGen)
        : base(idGen)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                Item item = new (reader);
                Add(item);
            }
        }
    }
    
    public new const int UNIQUE_ID = 528;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    public class Exception : PTException
    {
        public Exception(string message)
            : base(message) { }
    }

    public class ValidationException : PTValidationException
    {
        public ValidationException() { }

        public ValidationException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(a_message, a_stringParameters, a_appendHelpUrl) { }

        public ValidationException(string a_message, Exception a_innerException, bool a_appendHelpUrl = false, object[] a_stringParameters = null)
            : base(a_message, a_innerException, a_appendHelpUrl, a_stringParameters) { }
    }
    #endregion

    #region Construction
    public ItemManager(BaseIdGenerator idGen)
        : base(idGen) { }
    #endregion

    /// <summary>
    /// Returns a list of all of the Items' Groups.
    /// </summary>
    public List<string> GetUniqueGroups()
    {
        List<string> groups = new ();
        Hashtable groupsHash = new ();
        for (int i = 0; i < Count; i++)
        {
            Item item = GetByIndex(i);
            if (item.ItemGroup != null && item.ItemGroup.Trim().Length > 0 && !groupsHash.ContainsKey(item.ItemGroup))
            {
                groupsHash.Add(item.ItemGroup, null);
                groups.Add(item.ItemGroup);
            }
        }

        return groups;
    }

    #region ERP Transmissions
    internal void Receive(ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges, WarehouseT a_t, JobManager a_jobs, WarehouseManager warehouses, PurchaseToStockManager purchases, ProductRuleManager a_productRuleManager, Demand.SalesOrderManager salesOrderManager, Demand.TransferOrderManager a_transferOrderManager, UserFieldDefinitionManager a_udfManager, ScenarioExceptionInfo a_sei)
    {
        Transmissions.ApplicationExceptionList errList = new ();
        List<PostProcessingAction> actions = new ();

        HashSet<BaseId> affectedItems = new ();

        try
        {
            InitFastLookupByExternalId();

            List<WarehouseT.Item> itemsList = a_t.ItemsList;

            Item item;
            for (int i = 0; i < itemsList.Count; ++i)
            {
                try
                {
                    WarehouseT.Item itemNode = itemsList[i];
                    item = GetByExternalId(itemNode.ExternalId);

                    if (item == null)
                    {
                        item = new Item(a_udfManager, a_dataChanges, NextID(), itemNode, a_t, a_jobs);
                        affectedItems.Add(item.Id);
                        Add(item);
                        a_dataChanges.AuditEntry(new AuditEntry(item.Id, item), true);
                    }
                    else
                    {
                        AuditEntry itemAuditEntry = new AuditEntry(item.Id, item);
                        affectedItems.Add(item.Id);
                        item.Update(a_udfManager, a_dataChanges, itemNode, a_t, a_jobs);
                        a_dataChanges.AuditEntry(itemAuditEntry);
                    }
                }
                catch (PTHandleableException err)
                {
                    errList.Add(err);
                }
            }

            if (a_t.AutoDeleteItems)
            {
                ItemDeleteProfile itemsToDelete = new ();
                for (int i = Count - 1; i >= 0; i--)
                {
                    item = GetByIndex(i);

                    if (!affectedItems.Contains(item.Id))
                    {
                        itemsToDelete.Add(item);
                    }
                }

                if (!itemsToDelete.Empty)
                {
                    actions.Add(new PostProcessingAction(a_t, false, () =>
                        {
                            Transmissions.ApplicationExceptionList delErrList = new ();

                            try
                            {
                                Delete(a_sd, itemsToDelete, a_jobs, warehouses, purchases, a_productRuleManager, salesOrderManager, a_transferOrderManager, a_dataChanges);
                            }
                            catch (PTHandleableException err)
                            {
                                //Something wasn't handled by the ItemDeleteProfile
                                delErrList.Add(err);
                            }

                            foreach (PTValidationException validationException in itemsToDelete.ValidationExceptions)
                            {
                                //Add item delete specific validation messages
                                delErrList.Add(validationException);
                            }

                            if (delErrList.Count > 0)
                            {
                                m_errorReporter.LogException(delErrList, a_t, a_sei, ELogClassification.PtInterface, false);
                            }
                        }));
                }
            }
        }
        catch (PTHandleableException err)
        {
            errList.Add(err);
        }
        finally
        {
            actions.Add(new PostProcessingAction(a_t, true, DeInitFastLookupByExternalId));
            m_scenarioDetail.AddProcessingAction(actions);

            if (errList.Count > 0)
            {
                m_errorReporter.LogException(errList, a_t, a_sei, ELogClassification.PtInterface, false);
            }
        }
    }

    private void Delete(ScenarioDetail a_sd, ItemDeleteProfile a_itemDeleteProfile, JobManager jobs, WarehouseManager warehouses, PurchaseToStockManager purchases, ProductRuleManager a_prs, Demand.SalesOrderManager salesOrderManager, Demand.TransferOrderManager a_transferOrderManager, IScenarioDataChanges a_dataChanges)
    {
        //Make sure the Item is not refereneced by any Jobs or SOs
        salesOrderManager.ValidateDelete(a_itemDeleteProfile);
        warehouses.ValidateAndDeleteItems(a_sd, a_itemDeleteProfile, jobs, purchases, a_transferOrderManager, a_dataChanges);
        //Notify the PlantManager that an Item will be deleted in case the Item is in use by any of the ProductRules tables in the Resources.
        a_prs.DeletingItems(a_itemDeleteProfile);
        jobs.ValidateItemDelete(a_itemDeleteProfile);

        foreach (Item item in a_itemDeleteProfile.ItemsSafeToDelete())
        {
            a_dataChanges.AuditEntry(new AuditEntry(item.Id, item), false, true);
            Remove(item.Id);
            a_dataChanges.ItemChanges.DeletedObject(item.Id);
        }
    }

    /// <summary>
    /// Delete all Items.
    /// </summary>
    /// <param name="a_sd"></param>
    internal void Clear(ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        ItemDeleteProfile allItems = new ();
        for (int i = Count - 1; i >= 0; i--)
        {
            Item item = GetByIndex(i);
            allItems.Add(item);
        }

        Delete(a_sd, allItems, a_sd.JobManager, a_sd.WarehouseManager, a_sd.PurchaseToStockManager, a_sd.ProductRuleManager, a_sd.SalesOrderManager, a_sd.TransferOrderManager, a_dataChanges);
    }
    #endregion

    #region ICopyTable
    public override Type ElementType => typeof(Item);
    #endregion

    #region Restore References
    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        foreach (Item item in this)
        {
            a_udfManager.RestoreReferences(item, UserField.EUDFObjectType.Items);
        }
    }
    #endregion
}
