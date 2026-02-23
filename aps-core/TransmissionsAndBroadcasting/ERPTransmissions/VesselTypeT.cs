namespace PT.ERPTransmissions;

public class VesselTypeT : ERPMaintenanceTransmission<VesselTypeT.VesselType>, IPTSerializable
{
    public new const int UNIQUE_ID = 262;

    #region PT Serialization
    public VesselTypeT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                VesselType node = new (reader);
                Add(node);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public VesselTypeT() { } // reqd. for xml serialization

    public class VesselType : BaseResource, IPTSerializable
    {
        public new const int UNIQUE_ID = 263;

        #region PT Serialization
        public VesselType(IReader reader)
            : base(reader)
        {
            if (reader.VersionNumber >= 1)
            {
                reader.Read(out numberAvailable);

                reader.ReadList(out capabilities);
            }
        }

        public override void Serialize(IWriter writer)
        {
            base.Serialize(writer);

            writer.Write(numberAvailable);
            writer.WriteList(capabilities);
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        #region Shared Properties
        private int numberAvailable;

        /// <summary>
        /// Unlike all other Resources, Vessels are not tracked individually and therefore are not defined individually.  Instead, a total number of Vessels must be specified indicating their current inventory.
        /// </summary>
        public int NumberAvailable
        {
            get => numberAvailable;
            set => numberAvailable = value;
        }
        #endregion Shared Properties

        public VesselType() { } // reqd. for xml serialization

        public VesselType(string externalId, string name, string description, string notes, string userFields)
            : base(externalId, name, description, notes, userFields) { }

        private List<string> capabilities = new ();

        public List<string> Capabilities
        {
            get => capabilities;
            set => capabilities = value;
        }
    }

    public new VesselType this[int i] => Nodes[i];
}