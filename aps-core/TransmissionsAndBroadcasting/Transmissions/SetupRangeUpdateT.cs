using PT.APSCommon;
using PT.Scheduler;

namespace PT.Transmissions;

public class SetupRangeUpdateT : SetupRangeBaseT
{
    #region IPTSerializable Members
    public SetupRangeUpdateT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out tableName);
            reader.Read(out tableDescription);

            int count;
            reader.Read(out count);
            for (int i = 0; i < count; ++i)
            {
                setupRangeAttributeUpdates.Add(new SetupRangeAttributeUpdate(reader));
            }

            assignedResources = new ResourceKeyList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(tableName);
        writer.Write(tableDescription);

        writer.Write(setupRangeAttributeUpdates.Count);
        for (int i = 0; i < setupRangeAttributeUpdates.Count; ++i)
        {
            setupRangeAttributeUpdates[i].Serialize(writer);
        }

        assignedResources.Serialize(writer);
    }

    public new const int UNIQUE_ID = 577;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public SetupRangeUpdateT() { }

    public SetupRangeUpdateT(BaseId scenarioId, BaseId aFromToRangeId, string aTableName, string aTableDescription)
        : base(scenarioId, aFromToRangeId)
    {
        tableName = aTableName;
        tableDescription = aTableDescription;
    }

    public string tableName;
    public string tableDescription;
    public List<SetupRangeAttributeUpdate> setupRangeAttributeUpdates = new ();
    public ResourceKeyList assignedResources = new ();

    public override string Description => "Attribute Number Range Table updated";
}