using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;

namespace PT.ERPTransmissions;

public class CapabilityT : ERPMaintenanceTransmission<CapabilityT.Capability>, IPTSerializable
{
    public new const int UNIQUE_ID = 207;

    #region PT Serialization
    public CapabilityT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                Capability node = new (reader);
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

    public CapabilityT() { }

    public class Capability : BaseCapability, IPTSerializable
    {
        public new const int UNIQUE_ID = 214;

        #region PT Serialization
        public Capability(IReader reader)
            : base(reader)
        {
            if (reader.VersionNumber >= 612)
            {
                reader.Read(out m_resourceIsSet);
                if (m_resourceIsSet)
                {
                    m_resources = new List<ResourceKey>();
                    int c;
                    reader.Read(out c);
                    for (int i = 0; i < c; i++)
                    {
                        m_resources.Add(new ResourceKey(reader));
                    }
                }
            }
            else if (reader.VersionNumber >= 1) { }
        }

        public override void Serialize(IWriter writer)
        {
            base.Serialize(writer);
            writer.Write(m_resourceIsSet);
            if (m_resourceIsSet)
            {
                writer.Write(m_resources.Count);
                foreach (ResourceKey rk in m_resources)
                {
                    rk.Serialize(writer);
                }
            }
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        public Capability() { } // reqd. for xml serialization

        public Capability(string externalId, string name, string aDescription, string aNotes, string aUserFields)
            : base(externalId, name, aDescription, aNotes, aUserFields) { }

        public Capability(PtImportDataSet.CapabilitiesRow a_row)
            : base(a_row.ExternalId, a_row.Name, a_row.IsDescriptionNull() ? "" : a_row.Description, a_row.IsNotesNull() ? "" : a_row.Notes, a_row.IsUserFieldsNull() ? "" : a_row.UserFields) { }

        private bool m_resourceIsSet;

        public bool ResourcesIsSet => m_resourceIsSet;

        private List<ResourceKey> m_resources;

        public List<ResourceKey> Resources
        {
            get => m_resources;
            set
            {
                m_resources = value;
                m_resourceIsSet = true;
            }
        }
    }

    public new Capability this[int i] => Nodes[i];

    #region Database Loading
    public void Fill(System.Data.IDbCommand cmd)
    {
        PtImportDataSet.CapabilitiesDataTable table = new ();

        FillTable(table, cmd);
        Fill(table);
    }

    /// <summary>
    /// Fill the transmission with data from the DataSet.
    /// </summary>
    public void Fill(PtImportDataSet.CapabilitiesDataTable aTable)
    {
        for (int i = 0; i < aTable.Count; i++)
        {
            Add(new Capability(aTable[i]));
        }
    }
    #endregion

    public override string Description => string.Format("Capabilities updated ({0})".Localize(), Count);
}