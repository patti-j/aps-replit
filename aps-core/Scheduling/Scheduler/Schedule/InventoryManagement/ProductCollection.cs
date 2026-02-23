using System.Collections;

using PT.APSCommon;
using PT.APSCommon.Collections;
using PT.Common.Exceptions;
using PT.Database;
using PT.Scheduler.Demand;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

public partial class ProductsCollection : ICopyTable, IPTSerializable, IEnumerable<Product>, AfterRestoreReferences.IAfterRestoreReferences
{
    private readonly BaseIdGenerator m_idGenerator;
    public const int UNIQUE_ID = 536;

    #region IPTSerializable Members
    public ProductsCollection(IReader a_reader, BaseIdGenerator a_idGenerator)
    {
        m_idGenerator = a_idGenerator;
        if (a_reader.VersionNumber >= 410)
        {
            a_reader.Read(out lastId);
            m_productList = new ProductList(a_reader, a_idGenerator);
        }
    }

    internal void RestoreReferences(BaseOperationManager a_opManager, WarehouseManager aWarehouses, ItemManager aItems, SalesOrderManager a_salesOrderManager, TransferOrderManager a_transferOrderManager, BaseIdGenerator a_idGen)
    {
        for (int i = 0; i < Count; ++i)
        {
            this[i].RestoreReferences(a_opManager, aWarehouses, aItems, a_salesOrderManager, a_transferOrderManager, a_idGen);
        }
    }

    #region IAfterRestoreReferences
    public void AfterRestoreReferences_1(int a_serializationVersionNbr, HashSet<object> a_processedAfterRestoreReferences1, HashSet<object> a_processedAfterRestoreReferences2)
    {
        HashSet<Product> productHash = new(m_productList.Count);
        foreach (Product p in m_productList)
        {
            productHash.Add(p);
        }

        m_productList.Clear();
        foreach (Product p in productHash)
        {
            p.Id = m_idGenerator.NextID();
            m_productList.Add(p);
        }
    }

    public void AfterRestoreReferences_2(int a_serializationVersionNbr, HashSet<object> a_processedAfterRestoreReferences1, HashSet<object> a_processedAfterRestoreReferences2)
    {
    }
    #endregion

    public void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif

        a_writer.Write(lastId);

        m_productList.Serialize(a_writer);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    public class ProductsCollectionException : PTException
    {
        public ProductsCollectionException(string message)
            : base(message) { }
    }

    private readonly ProductList m_productList = new ();
    #endregion

    #region Construction
    public ProductsCollection() { }
    #endregion

    #region Properties and Methods
    private long lastId;

    public Type ElementType => typeof(Product);

    internal void Add(Product a_product)
    {
        m_productList.Add(a_product);
    }

    public object GetRow(int index)
    {
        return m_productList.GetByIndex(index);
    }

    public Product GetById(BaseId a_productId)
    {
        return m_productList.GetValue(a_productId);
    }

    public Product this[int index] => m_productList.GetByIndex(index);

    public void Clear()
    {
        m_productList.Clear();
    }

    public IReadOnlyList<Product> ReadOnlyList => m_productList.ReadOnlyList;

    public int Count => m_productList.Count;

    internal IEnumerator<Product> GetEnumerator()
    {
        return m_productList.GetEnumerator();
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (GetType() != obj.GetType())
        {
            return false;
        }

        ProductsCollection oProducts = (ProductsCollection)obj;

        if (Count != oProducts.Count)
        {
            return false;
        }

        for (int i = 0; i < Count; ++i)
        {
            Product p = this[i];
            if (oProducts.GetByItemId(p.Item.Id) == null)
            {
                return false;
            }
        }

        return true;
    }
    #endregion

    #region PT Database
    internal void PopulateJobDataSet(ref JobDataSet dataSet, BaseOperation operation)
    {
        for (int i = 0; i < Count; i++)
        {
            this[i].PopulateJobDataSet(ref dataSet, operation);
        }
    }

    public void PtDbPopulate(ref PtDbDataSet dataSet, BaseOperation op, PtDbDataSet.ManufacturingOrdersRow moRow, bool publishInventory, PTDatabaseHelper a_dbHelper)
    {
        for (int i = 0; i < Count; i++)
        {
            this[i].PtDbPopulate(ref dataSet, op, moRow, publishInventory, a_dbHelper);
        }
    }
    #endregion

    #region Update
    internal bool Update(ProductsCollection a_tProductCollection, BaseOperation a_operation, bool a_erpUpdate, ScenarioOptions a_so, BaseOperationManager a_opManager, IScenarioDataChanges a_dataChanges)
    {
        bool updated = false;
        lastId = a_tProductCollection.lastId;
        HashSet<BaseId> affectedProducts = new ();
        bool productsAdded = a_tProductCollection.Count > Count;
        for (int productI = 0; productI < a_tProductCollection.Count; productI++)
        {
            Product newP = a_tProductCollection[productI];
            Product prod = GetByExternalId(newP.ExternalId);
            
            if (prod != null)
            {
                AuditEntry productAuditEntry = new AuditEntry(prod.Id, a_operation.Id, prod);
                updated |= prod.Update(newP, a_erpUpdate, a_operation, a_so, a_dataChanges);
                affectedProducts.Add(prod.Id);
                a_dataChanges.AuditEntry(productAuditEntry);
            }
            else
            {
                Product newProduct = newP.DeepCopy();
                Add(newProduct);
                affectedProducts.Add(newProduct.Id);
                updated = true;
                a_dataChanges.AuditEntry(new AuditEntry(newProduct.Id, a_operation.Id, newProduct), true);
            }
        }

        for (int i = Count - 1; i >= 0; i--)
        {
            Product product = this[i];
            if (!affectedProducts.Contains(product.Id)) 
            {
                updated = true;
                Remove(product);
                a_dataChanges.AuditEntry(new AuditEntry(product.Id, a_operation.Id, product), false, true);
            }
        }

        if (productsAdded && a_operation.Scheduled)
        {
            a_dataChanges.FlagProductionChanges(a_operation.Job.Id);
        }
        
        return updated;
    }
    
    public bool Remove(Product a_product)
    {
        return m_productList.RemoveObject(a_product);
    }

    /// <summary>
    /// O(n).
    /// Get a Product by external id.
    /// </summary>
    /// <param name="a_externalId">The external id of the Product that you want.</param>
    /// <returns>The Product that you want or null if it's not here.</returns>
    private Product GetByExternalId(string a_externalId)
    {
        for (int productI = 0; productI < Count; ++productI)
        {
            Product rr = this[productI];
            if (rr.ExternalId == a_externalId)
            {
                return rr;
            }
        }

        return null;
    }

    /// <summary>
    /// O(n).
    /// Get a Product by Item id.
    /// </summary>
    /// <param name="aItemId">The id of the Item for the Product that you want.</param>
    /// <returns>The first Product for the Item that you want or null if it's not here.</returns>
    public Product GetByItemId(BaseId aItemId)
    {
        for (int productI = 0; productI < Count; ++productI)
        {
            Product product = this[productI];

            if (product.Item.Id.Equals(aItemId))
            {
                return product;
            }
        }

        return null;
    }

    public Product GetByInventoryId(BaseId a_id)
    {
        for (int productI = 0; productI < Count; ++productI)
        {
            Product product = this[productI];

            if (product.Inventory.Id.Equals(a_id))
            {
                return product;
            }
        }

        return null;
    }
    #endregion

    private class ProductList : CustomSortedList<Product>
    {
        public ProductList(IReader a_reader, BaseIdGenerator a_IdManager)
            : base(a_reader, new ProductComparer(), a_IdManager) { }

        public ProductList()
            : base(new ProductComparer()) { }

        protected override void Serialize(IWriter a_writer, Product a_value)
        {
            a_value.Serialize(a_writer);
        }

        protected override Product CreateInstance(IReader a_reader)
        {
            if (a_reader.VersionNumber >= 12511)
            {
                return new Product(a_reader);
            }

            //Old wear requirement serialization
            a_reader.Read(out int val);

            if (val == 1)
            {
                new BaseId(a_reader);
                new BaseId(a_reader);
                a_reader.Read(out int wearToBeAdded);
            }

            return new Product(a_reader);
        }

        public class ProductComparer : IKeyObjectComparer<Product>
        {
            public int Compare(Product a_x, Product a_y)
            {
                return CompareProduct(a_x, a_y);
            }

            private static int CompareProduct(Product a_product, Product a_anotherProduct)
            {
                return Comparison.Compare(a_product.Id.Value, a_anotherProduct.Id.Value);
            }

            public object GetKey(Product a_product)
            {
                return a_product.Id;
            }
        }
    }

    IEnumerator<Product> IEnumerable<Product>.GetEnumerator()
    {
        return m_productList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Product? PrimaryProduct => m_productList.Count > 0 ? m_productList.GetByIndex(0) : null;
}