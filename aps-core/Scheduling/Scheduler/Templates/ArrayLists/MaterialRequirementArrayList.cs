using System.Collections;

namespace PT.Scheduler;

public class MaterialRequirementArrayList //: PT.Common.IPTSerializable
{
    //		public const int UNIQUE_ID=311;
    //		#region IPTSerializable Members
    //
    //		public MaterialRequirementArrayList(PT.Common.IReader reader)
    //		{
    //			if(reader.VersionNumber>=1)
    //			{
    //				int count;
    //				reader.Read(out count);
    //				for(int i=0;i<count;i++)
    //				{
    //					MaterialRequirement pset=new MaterialRequirement(reader);
    //					this.Add(pset);
    //				}
    //			}
    //		}
    //
    //		public void Serialize(PT.Common.IWriter writer)
    //		{
    //#if DEBUG
    //			writer.DuplicateErrorCheck(this);
    //#endif
    //			
    //			writer.Write(this.Count);
    //			for(int i=0;i<this.Count;i++)
    //				((MaterialRequirement)this.al[i]).Serialize(writer);
    //
    //		}
    //
    //		public int UniqueId
    //		{
    //			get
    //			{
    //				return UNIQUE_ID;
    //			}
    //		}
    //
    //		#endregion

    internal MaterialRequirementArrayList()
    {
        al = new ArrayList();
    }

    private MaterialRequirementArrayList(MaterialRequirementArrayList o)
    {
        al = (ArrayList)o.al.Clone();
    }

    private readonly ArrayList al;

    internal int Add(MaterialRequirement s)
    {
        return al.Add(s);
    }

    internal void Clear()
    {
        al.Clear();
    }

    internal MaterialRequirementArrayList Copy()
    {
        return new MaterialRequirementArrayList(this);
    }

    public int Count => al.Count;

    public MaterialRequirement this[int i]
    {
        get => (MaterialRequirement)al[i];

        set => al[i] = value;
    }

    internal void AddRange(MaterialRequirementArrayList original)
    {
        al.AddRange(original.al);
    }
}