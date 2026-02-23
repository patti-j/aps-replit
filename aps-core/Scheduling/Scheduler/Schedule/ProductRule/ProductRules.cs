using PT.APSCommon.Collections;

namespace PT.Scheduler;

/// <summary>
/// Stores a Dictionary containing lists of Product Rules.
/// </summary>
[Obsolete("Replaced by ProductRuleManager")]
public class ProductRules
{
    #region IPTSerializable Members
    public ProductRules(IReader reader)
    {
        if (reader.VersionNumber >= 410)
        {
            m_productRuleListsByProductExternalIds = new ProductRulesList(reader);
        }
    }

    internal void RestoreReferences(ItemManager aItems)
    {
        m_productRuleListsByProductExternalIds.RestoreReferences(aItems);
    }
    
    public const int UNIQUE_ID = 609;

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    private readonly ProductRulesList m_productRuleListsByProductExternalIds;
    
    public IEnumerator<ProductRuleAndKey> GetEnumerator()
    {
        return m_productRuleListsByProductExternalIds.GetEnumerator();
    }
    
    [Obsolete("Replaced by ProductRuleManager")]
    internal class ProductRulesList : CustomSortedDictionary<string, ProductRuleAndKey>
    {
        internal ProductRulesList(IReader a_reader)
            : base(a_reader) { }
        
        protected override ProductRuleAndKey CreateInstance(IReader a_reader)
        {
            return new ProductRuleAndKey(a_reader);
        }

        internal void RestoreReferences(ItemManager a_items)
        {
            foreach (ProductRuleAndKey prk in this)
            {
                prk.RestoreReferences(a_items);
            }
        }
    }

    [Obsolete("Replaced by ProductRuleManager")]
    public class ProductRuleAndKey : IPTSerializable, IKey<string>, IComparable<ProductRuleAndKey>, IEquatable<ProductRuleAndKey>
    {
        private const int UNIQUE_ID = 738;

        public void Serialize(IWriter a_writer)
        {
            throw new NotImplementedException();
        }

        public virtual int UniqueId => UNIQUE_ID;

        internal ProductRuleAndKey(IReader a_reader)
        {
            m_productRules = new List<ProductRule>();

            m_refInfo = new ReferenceInfo();

            a_reader.Read(out Key);
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                ProductRule pr = new (a_reader);
                m_refInfo.ProductRules.Add(pr);
            }
        }

        private ReferenceInfo m_refInfo;

        internal void RestoreReferences(ItemManager a_items)
        {
            for (int ruleI = 0; ruleI < m_refInfo.ProductRules.Count; ++ruleI)
            {
                ProductRule rule = m_refInfo.ProductRules[ruleI];
                rule.RestoreReferences(a_items);
                m_productRules.Add(rule);
            }

            m_refInfo = null;
        }
        
        internal ProductRuleAndKey(string a_key, List<ProductRule> a_productRules)
        {
            Key = a_key;
            m_productRules = a_productRules;
        }

        internal readonly string Key;
        private readonly List<ProductRule> m_productRules;
        public List<ProductRule> ProductRules => m_productRules;

        private class ReferenceInfo
        {
            internal readonly List<ProductRule> ProductRules = new ();
        }

        public string GetKey()
        {
            return Key;
        }

        public bool Equals(ProductRuleAndKey a_other)
        {
            return CompareTo(a_other) == 0;
        }

        public bool Equals(string a_other)
        {
            return Comparison.Compare(Key, a_other) == 0;
        }

        public int CompareTo(ProductRuleAndKey a_other)
        {
            return Comparison.Compare(Key, a_other.Key);
        }

        public override bool Equals(object a_obj)
        {
            if (ReferenceEquals(null, a_obj))
            {
                return false;
            }

            if (ReferenceEquals(this, a_obj))
            {
                return true;
            }

            if (a_obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((ProductRuleAndKey)a_obj);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}