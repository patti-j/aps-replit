using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Transmission for updating all Allowed Helper Resources.
/// </summary>
public class AllowedHelperResourcesT : PlantBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 703;

    #region IPTSerializable Members
    public AllowedHelperResourcesT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                _allowedHelperResources.Add(new HelperRelation(reader));
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(_allowedHelperResources.Count);
        for (int i = 0; i < _allowedHelperResources.Count; i++)
        {
            _allowedHelperResources[i].Serialize(writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public AllowedHelperResourcesT() { }

    public AllowedHelperResourcesT(BaseId scenarioId)
        : base(scenarioId) { }

    private readonly List<HelperRelation> _allowedHelperResources = new ();

    public List<HelperRelation> AllowedHelperResources => _allowedHelperResources;

    public override string Description => "Allowed Helpers saved";

    public class HelperRelation : IPTSerializable
    {
        public const int UNIQUE_ID = 704;

        #region IPTSerializable Members
        public HelperRelation(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                PrimaryResourceId = new BaseId(reader);
                int count;
                reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    AllowedHelperResourceIds.Add(new BaseId(reader));
                }
            }
        }

        public void Serialize(IWriter writer)
        {
            PrimaryResourceId.Serialize(writer);
            writer.Write(AllowedHelperResourceIds.Count);
            for (int i = 0; i < AllowedHelperResourceIds.Count; i++)
            {
                AllowedHelperResourceIds[i].Serialize(writer);
            }
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public HelperRelation(BaseId aPrimaryResourceId)
        {
            PrimaryResourceId = aPrimaryResourceId;
        }

        public BaseId PrimaryResourceId;

        public List<BaseId> AllowedHelperResourceIds = new ();
    }
}