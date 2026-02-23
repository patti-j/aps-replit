using PT.APSCommon.Collections;

using PT.APSCommon;


using PT.Common.Collections;
using PT.SchedulerDefinitions;
using PT.Scheduler.Schedule.InventoryManagement;

namespace PT.Scheduler;

/// <summary>
/// Stores all ProductRules. Provides Add, Remove, and lookup functions
/// All product rules are stored in a CustomSortedDictionary
/// </summary>
public class ProductRuleManager : IPTSerializable
{
    public void Serialize(IWriter a_writer)
    {
        m_productRulesDictionary.Serialize(a_writer);
    }

    public int UniqueId => 1115;

    internal ProductRuleManager(IReader a_reader)
    {
        m_productRulesDictionary = new ProductRuleCollection(a_reader);
    }

    /// <summary>
    /// An Empty manager
    /// </summary>
    internal ProductRuleManager() { }

    private readonly ProductRuleCollection m_productRulesDictionary = new ();

    /// <summary>
    /// Update an existing Product Rule or add a new one if it doesn't exist.
    /// If the ProductCode doesn't match an existing rule, a new Product Rule will be added
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_productRule"></param>
    /// <param name="a_resource"></param>
    /// <param name="a_item"></param>
    /// <param name="a_dataChanges"></param>
    /// <returns></returns>
    internal ProductRule AddOrUpdateProductRule(ScenarioDetail a_sd, PT.ERPTransmissions.ProductRulesT.ProductRule a_productRule, InternalResource a_resource, Item a_item, IScenarioDataChanges a_dataChanges)
    {
        //See if a rule already exists for this Product/Operation combination
        ProductRuleKey prk = new ProductRuleKey(a_resource.Id, a_item.Id, a_productRule.ProductCode);
        ProductRule existingRule = GetByKey(prk);

        a_dataChanges.MachineChanges.UpdatedObject(a_resource.Id);


        if (existingRule != null)
        {
            existingRule.Update(a_sd, a_productRule);
            return existingRule;
        }
        else
        {
            ProductRule newRule = new ProductRule(a_sd, a_productRule, a_resource, a_item);
            m_productRulesDictionary.Add(newRule);
            return newRule;
        }
    }

    /// <summary>
    /// Remove any product rules for items that are being deleted
    /// </summary>
    /// <param name="a_items"></param>
    internal void DeletingItems(ItemDeleteProfile a_items)
    {
        try
        {
            CacheRulesByItemId();

            //Remove any Product Rules for the Item
            foreach (Item deletingItem in a_items)
            {
                if (m_productRuleCache.TryGetValue(deletingItem.Id, out List<ProductRuleKey> prs))
                {
                    foreach (ProductRuleKey prk in prs)
                    {
                        m_productRulesDictionary.RemoveByKey(prk);
                    }
                }
            }
        }
        finally
        {
            ClearCaches();
        }
    }

    /// <summary>
    /// Remove any product rules for the Resource that is being deleted
    /// </summary>
    /// <param name="a_resourceId"></param>
    internal void DeletingResource(BaseId a_resourceId)
    {
        List<ProductRuleKey> prkToDelete = new ();
        //Remove any Product Rules for the Resource
        foreach (ProductRule pr in m_productRulesDictionary)
        {
            if (pr.ResourceId == a_resourceId)
            {
                prkToDelete.Add(pr.GetKey());
            }
        }

        foreach (ProductRuleKey prk in prkToDelete)
        {
            m_productRulesDictionary.RemoveByKey(prk);
        }
    }

    /// <summary>
    /// A temporary store for product rules stored by their ItemId.
    /// This allows quick lookup by item Id, for example to delete
    /// </summary>
    private DictionaryCollection<BaseId, ProductRuleKey> m_productRuleCache;
    private void CacheRulesByItemId()
    {
        m_productRuleCache = new DictionaryCollection<BaseId, ProductRuleKey>();
        foreach (ProductRule pr in m_productRulesDictionary)
        {
            ProductRuleKey prk = pr.GetKey();
            m_productRuleCache.Add(prk.ItemId, prk);
        }
    }

    private void ClearCaches()
    {
        m_productRuleCache.Clear();
        m_productRuleCache = null;
    }

    internal void Clear()
    {
        m_productRulesDictionary.Clear();
    }

    public ProductRule GetProductRule(BaseId a_resId, BaseId a_itemId, string a_productCode)
    {
        ProductRuleKey prKey = new ProductRuleKey(a_resId, a_itemId, a_productCode);
        ProductRule pr = GetByKey(prKey);
        if (pr != null)
        {
            return pr;
        }

        //Find without ProductCode
        return GetByKey(prKey.WithoutProductCode());
    }

    public ProductRule GetByKey(ProductRuleKey a_productRuleKey)
    {
        if (m_productRulesDictionary.TryGetValue(a_productRuleKey, out ProductRule pr))
        {
            return pr;
        }

        return null;
    }

    /// <summary>
    /// Delete any product rules that are not in the provided key list
    /// </summary>
    /// <param name="a_affectedProductRulesHash"></param>
    internal void AutoDelete(HashSet<ProductRuleKey> a_affectedProductRulesHash)
    {
        foreach (ProductRuleKey key in m_productRulesDictionary.GetUnsortedKeys())
        {
            if (!a_affectedProductRulesHash.Contains(key))
            {
                m_productRulesDictionary.RemoveByKey(key);
            }
        }
    }

    internal void AddForBackwardsCompatibility(ProductRule a_productRule)
    {
        m_productRulesDictionary.Add(a_productRule);
    }

    public IEnumerable<ProductRule> EnumerateForResource(BaseId a_resId)
    {
        foreach (ProductRule rule in m_productRulesDictionary.ReadOnlyList)
        {
            if (rule.ResourceId == a_resId)
            {
                yield return rule;
            }
        }
    }

    public void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        foreach (ProductRule productRule in m_productRulesDictionary)
        {
            a_udfManager.RestoreReferences(productRule.ProductRulesUserFields, UserField.EUDFObjectType.ProductRules);
        }
    }
}

/// <summary>
/// A compound key representing the unique lookup values for a Product Rule
/// </summary>
public struct ProductRuleKey : IEquatable<ProductRuleKey>
{
    public BaseId ResourceId;
    public BaseId ItemId;
    public string ProductCode;

    public ProductRuleKey(BaseId a_resourceId, BaseId a_itemId, string a_productCode)
    {
        ResourceId = a_resourceId;
        ItemId = a_itemId;
        ProductCode = a_productCode;
        if (string.IsNullOrWhiteSpace(ProductCode))
        {
            //Don't allow null ProductCodes so we prevent null references. 
            ProductCode = string.Empty;
        }
    }

    public ProductRuleKey(ProductRule a_productRule)
    {
        ResourceId = a_productRule.ResourceId;
        ItemId = a_productRule.ItemId;
        ProductCode = a_productRule.ProductCode; //This should not be null, faster validation
    }

    public bool Equals(ProductRuleKey a_other)
    {
        return ResourceId.Equals(a_other.ResourceId) && ItemId.Equals(a_other.ItemId) && ProductCode == a_other.ProductCode;
    }

    public override bool Equals(object a_obj)
    {
        return a_obj is ProductRuleKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ResourceId, ItemId, ProductCode);
    }

    /// <summary>
    /// Returns this code without a ProductCode set
    /// </summary>
    /// <returns></returns>
    public ProductRuleKey WithoutProductCode()
    {
        return new ProductRuleKey(ResourceId, ItemId, string.Empty);
    }
}

internal class ProductRuleCollection : CustomSortedDictionary<ProductRuleKey, ProductRule>
{
    internal ProductRuleCollection() { }

    internal ProductRuleCollection(IReader a_reader)
        : base(a_reader) { }

    public override void Serialize(IWriter a_writer)
    {
        a_writer.Write(Count);
        using (IEnumerator<ProductRule> enumerator = GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                ProductRule productRule = enumerator.Current;
                productRule.Serialize(a_writer);
            }
        }
    }

    protected override ProductRule CreateInstance(IReader a_reader)
    {
        return new ProductRule(a_reader);
    }

    internal void RestoreReferences(ItemManager a_items)
    {
        //TODO: DO we need to restore any references here? I think we can just use the IDs
    }
}



