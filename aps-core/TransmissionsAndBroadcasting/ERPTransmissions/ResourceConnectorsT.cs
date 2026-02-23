using System.Data;

using PT.APSCommon;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

public class ResourceConnectorsT : ERPMaintenanceTransmission<ResourceConnectorsT.ResourceConnector>
{
    #region PT Serialization
    public ResourceConnectorsT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                ResourceConnector node = new (a_reader);
                Add(node);
            }
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        a_writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(a_writer);
        }
    }

    public new const int UNIQUE_ID = 1094;
    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ResourceConnectorsT() { }

    private BoolVector32 m_bools;
    private const short c_autoDeleteConnectionsIdx = 0;

    public bool AutoDeleteConnections
    {
        get { return m_bools[c_autoDeleteConnectionsIdx]; }
        set
        {
            m_bools[c_autoDeleteConnectionsIdx] = value;
        }
    }

    public class ResourceConnector : PTObjectBase
    {
        #region PT Serialization
        public ResourceConnector(IReader a_reader)
            : base(a_reader)
        {
            m_bools = new BoolVector32(a_reader);
            m_isSetBools = new BoolVector32(a_reader);

            a_reader.Read(out m_transitTicks);
            a_reader.Read(out int count);

            for (int i = 0; i < count; i++)
            {
                FromResources.Add(new ResourceKeyExternal(a_reader));
            }

            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                ToResources.Add(new ResourceKeyExternal(a_reader));
            }
        }

        public override void Serialize(IWriter a_writer)
        {
            base.Serialize(a_writer);
            m_bools.Serialize(a_writer);
            m_isSetBools.Serialize(a_writer);

            a_writer.Write(m_transitTicks);

            a_writer.Write(FromResources.Count);

            foreach (ResourceKeyExternal resExternalKey in FromResources)
            {
                resExternalKey.Serialize(a_writer);
            }

            a_writer.Write(ToResources.Count);

            foreach (ResourceKeyExternal resExternalKey in ToResources)
            {
                resExternalKey.Serialize(a_writer);
            }
        }

        public override int UniqueId => 1095;
        #endregion

        public ResourceConnector() { } // reqd. for xml serialization

        public ResourceConnector(string a_externalId, string a_name, string a_description, string a_notes, string a_userFields)
            : base(a_externalId, a_name, a_description, a_notes, a_userFields) { }

        public ResourceConnector(ResourceConnectorDataSet.ResourceConnectorsRow a_resConnectorRow)
            : base(a_resConnectorRow.ExternalId, a_resConnectorRow.Name, a_resConnectorRow.IsDescriptionNull() ? null : a_resConnectorRow.Description, a_resConnectorRow.IsNotesNull() ? null : a_resConnectorRow.Notes, a_resConnectorRow.IsUserFieldsNull() ? null : a_resConnectorRow.UserFields)
        {
            ExternalId = a_resConnectorRow.ExternalId;

            if (!a_resConnectorRow.IsTransitHoursNull())
            {
                TransitTicks = TimeSpan.FromHours(a_resConnectorRow.TransitHours).Ticks;
            }

            if (!a_resConnectorRow.IsAllowConcurrentUseNull())
            {
                AllowConcurrentUse = a_resConnectorRow.AllowConcurrentUse;
            }
        }

        internal void AddConnection(ResourceKeyExternal a_resKeyExternal, ConnectorDefs.EConnectionDirection a_connDirection, ApplicationExceptionList a_errors)
        {
            switch (a_connDirection)
            {
                case ConnectorDefs.EConnectionDirection.FromResource:
                    FromResources.Add(a_resKeyExternal);
                    break;
                case ConnectorDefs.EConnectionDirection.ToResource:
                    ToResources.Add(a_resKeyExternal);
                    break;
            }

            ValidateConnection(a_resKeyExternal, a_errors);
        }

        private void ValidateConnection(ResourceKeyExternal a_resKeyExternal, ApplicationExceptionList a_errors)
        {
            if (FromResources.Contains(a_resKeyExternal) && ToResources.Contains(a_resKeyExternal))
            {
                a_errors.Add(new PTValidationException("4481", new object[] { ExternalId, a_resKeyExternal.ResourceExternalId, a_resKeyExternal.DepartmentExternalId, a_resKeyExternal.PlantExternalId }));
            }
        }

        private BoolVector32 m_bools;
        private const short c_allowConcurrentUseIdx = 0;
        private const short c_autoDeleteConnectionsIdx = 1;

        private BoolVector32 m_isSetBools;
        private const short c_allowConcurrentUseIsSetIdx = 0;
        private const short c_transitTicksIsSetIdx = 1;
        private const short c_autoDeleteConnectionsIsSetIdx = 3;

        public List<ResourceKeyExternal> FromResources = new ();
        public List<ResourceKeyExternal> ToResources = new ();

        public bool AllowConcurrentUse
        {
            get => m_bools[c_allowConcurrentUseIdx];
            set
            {
                m_bools[c_allowConcurrentUseIdx] = value;
                m_isSetBools[c_allowConcurrentUseIsSetIdx] = true;
            }
        }

        public bool AllowConcurrentUseIsSet => m_isSetBools[c_allowConcurrentUseIsSetIdx];

        private long m_transitTicks;
        public long TransitTicks
        {
            get { return m_transitTicks; }
            set
            {
                m_transitTicks = value;
                m_isSetBools[c_transitTicksIsSetIdx] = true;
            }
        }

        public bool TransitTicksIsSet => m_isSetBools[c_transitTicksIsSetIdx];
    }

    public new ResourceConnector this[int a_i] => Nodes[a_i];

    public void Fill(IDbCommand a_resourceConnectorsCmd, IDbCommand a_connectionCmd, ApplicationExceptionList a_errors)
    {
        ResourceConnectorDataSet ds = new ();
        FillTable(ds.ResourceConnectors, a_resourceConnectorsCmd);
        FillTable(ds.ResourceConnection, a_connectionCmd);
        Fill(ds, a_errors);
    }

    public void Fill(ResourceConnectorDataSet a_ds, ApplicationExceptionList a_errors)
    {
        foreach (ResourceConnectorDataSet.ResourceConnectorsRow resourceConnectorsRow in a_ds.ResourceConnectors)
        {
            ResourceConnector resConnector = new ResourceConnector(resourceConnectorsRow);

            foreach (ResourceConnectorDataSet.ResourceConnectionRow connectionRow in resourceConnectorsRow.GetResourceConnectionRows())
            {
                ResourceKeyExternal resourceKeyExternal = new ResourceKeyExternal(connectionRow.PlantExternalId, connectionRow.DepartmentExternalId, connectionRow.ResourceExternalId);
                if (!Enum.TryParse(connectionRow.ConnectionDirection, out  ConnectorDefs.EConnectionDirection direction))
                {
                    ArgumentOutOfRangeException argumentOutOfRangeException = new ArgumentOutOfRangeException(nameof(connectionRow.ConnectionDirection), connectionRow.ConnectionDirection, null);
                    a_errors.Add(new PTValidationException("4480", argumentOutOfRangeException, false, new object[] { resConnector.ExternalId, connectionRow.ConnectionDirection }));
                    continue;
                }


                resConnector.AddConnection(resourceKeyExternal, direction, a_errors);
            }

            Add(resConnector);
        }

        //Validate that duplicate external ids aren't being imported
        Validate();
    }
}