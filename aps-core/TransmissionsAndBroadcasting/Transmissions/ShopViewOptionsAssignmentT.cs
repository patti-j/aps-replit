using System.Collections;

using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

/// <summary>
/// For setting the ShopViewResourceOptions for all Resources.
/// </summary>
public class ShopViewOptionsAssignmentT : ScenarioIdBaseT, IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 509;

    public ShopViewOptionsAssignmentT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int resAssignmentCount;
            reader.Read(out resAssignmentCount);
            for (int i = 0; i < resAssignmentCount; i++)
            {
                resourceOptionAssignments.Add(new ResourceOptionsAssignment(reader));
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(ResourceOptionsAssignmentCount);
        for (int i = 0; i < ResourceOptionsAssignmentCount; i++)
        {
            GetResourceAssignment(i).Serialize(writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ShopViewOptionsAssignmentT() { }

    public ShopViewOptionsAssignmentT(BaseId scenarioId)
        : base(scenarioId) { }

    private readonly ArrayList resourceOptionAssignments = new ();

    public ResourceOptionsAssignment GetResourceAssignment(int index)
    {
        return (ResourceOptionsAssignment)resourceOptionAssignments[index];
    }

    public int ResourceOptionsAssignmentCount => resourceOptionAssignments.Count;

    public void Add(ResourceOptionsAssignment assignment)
    {
        resourceOptionAssignments.Add(assignment);
    }

    public class ResourceOptionsAssignment : IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 508;

        public ResourceOptionsAssignment(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                plantId = new BaseId(reader);
                deptId = new BaseId(reader);
                resourceId = new BaseId(reader);
                optionsId = new BaseId(reader);
            }
        }

        public void Serialize(IWriter writer)
        {
            plantId.Serialize(writer);
            deptId.Serialize(writer);
            resourceId.Serialize(writer);
            optionsId.Serialize(writer);
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public ResourceOptionsAssignment(BaseId plantId, BaseId deptId, BaseId resourceId, BaseId optionsId)
        {
            this.plantId = plantId;
            this.deptId = deptId;
            this.resourceId = resourceId;
            this.optionsId = optionsId;
        }

        public BaseId plantId;
        public BaseId deptId;
        public BaseId resourceId;
        public BaseId optionsId;
    }

    public override string Description => "Operation Status Options saved".Localize();
}