using System.Collections;

namespace PT.Scheduler;

public class ResourceRequirementArrayList
{
    internal ResourceRequirementArrayList()
    {
        al = new ArrayList();
    }

    private ResourceRequirementArrayList(ResourceRequirementArrayList o)
    {
        al = (ArrayList)o.al.Clone();
    }

    private readonly ArrayList al;

    internal int Add(ResourceRequirement s)
    {
        return al.Add(s);
    }

    internal void Clear()
    {
        al.Clear();
    }

    internal ResourceRequirementArrayList Copy()
    {
        return new ResourceRequirementArrayList(this);
    }

    public int Count => al.Count;

    public ResourceRequirement this[int i]
    {
        get => (ResourceRequirement)al[i];

        set => al[i] = value;
    }

    internal void AddRange(ResourceRequirementArrayList original)
    {
        al.AddRange(original.al);
    }
}