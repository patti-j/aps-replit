using System.Collections;

namespace PT.Scheduler;

public class ProductArrayList //: PT.Common.IPTSerializable
{
    internal ProductArrayList()
    {
        al = new ArrayList();
    }

    private ProductArrayList(ProductArrayList o)
    {
        al = (ArrayList)o.al.Clone();
    }

    private readonly ArrayList al;

    internal int Add(Product s)
    {
        return al.Add(s);
    }

    internal void Clear()
    {
        al.Clear();
    }

    internal ProductArrayList Copy()
    {
        return new ProductArrayList(this);
    }

    public int Count => al.Count;

    public Product this[int i]
    {
        get => (Product)al[i];

        set => al[i] = value;
    }

    internal void AddRange(ProductArrayList original)
    {
        al.AddRange(original.al);
    }
}