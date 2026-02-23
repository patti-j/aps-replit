using PT.APSCommon.Extensions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

public class DepartmentT : ERPMaintenanceTransmission<DepartmentT.Department>, IPTSerializable
{
    public new const int UNIQUE_ID = 220;

    #region PT Serialization
    public DepartmentT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                Department node = new (reader);
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

    public DepartmentT() { }

    public class Department : PTObjectBase
    {
        public new const int UNIQUE_ID = 221;

        #region PT Serialization
        public Department(IReader a_reader)
            : base(a_reader)
        {
            if (a_reader.VersionNumber >= 12054)
            {
                m_bools = new BoolVector32(a_reader);
                a_reader.Read(out plantExternalId);
                a_reader.Read(out m_frozenSpanDuration);
            }
            else if (a_reader.VersionNumber >= 12050)
            {
                m_bools = new BoolVector32(a_reader);

                a_reader.Read(out plantExternalId);
                // Discard these fields since we no longer use them
                a_reader.Read(out int sort);
                a_reader.Read(out bool sortSet);
                a_reader.Read(out m_frozenSpanDuration);
            }
            else if (a_reader.VersionNumber >= 450)
            {
                a_reader.Read(out plantExternalId);
                // Discard these fields since we no longer use them
                a_reader.Read(out int sort);
                a_reader.Read(out bool sortSet);
                m_bools = new BoolVector32(a_reader);
                if (m_bools[0])
                {
                    a_reader.Read(out m_frozenSpanDuration);
                }
            }
        }

        public override void Serialize(IWriter a_writer)
        {
            base.Serialize(a_writer);

            m_bools.Serialize(a_writer);

            a_writer.Write(plantExternalId);
            a_writer.Write(m_frozenSpanDuration);
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        public Department() { } // reqd. for xml serialization

        public Department(string externalId, string name, string description, string notes, string userFields, string plantExternalId)
            : base(externalId, name, description, notes, userFields)
        {
            this.plantExternalId = plantExternalId;
        }

        public Department(PtImportDataSet.DepartmentsRow aRow)
            : base(aRow.ExternalId, aRow.IsNameNull() ? null : aRow.Name, aRow.IsDescriptionNull() ? null : aRow.Description, aRow.IsNotesNull() ? null : aRow.Notes, aRow.IsUserFieldsNull() ? null : aRow.UserFields)
        {
            plantExternalId = aRow.PlantExternalId;

            if (!aRow.IsDepartmentFrozenSpanHrsNull())
            {
                FrozenSpanDuration = TimeSpan.FromHours(aRow.DepartmentFrozenSpanHrs).Ticks;
            }
        }

        private BoolVector32 m_bools;

        private const short c_frozenSpanSetIdx = 0;

        //Overrides the system option for FrozenSpan
        public bool FrozenSpanSet => m_bools[c_frozenSpanSetIdx];

        //Department FrozenSpan override
        private long m_frozenSpanDuration;

        public long FrozenSpanDuration
        {
            get => m_frozenSpanDuration;
            set
            {
                m_frozenSpanDuration = value;
                m_bools[c_frozenSpanSetIdx] = true;
            }
        }

        private string plantExternalId;

        [Required(true)]
        /// <summary>
        /// Indicates the parent Plant.
        /// </summary>
        public string PlantExternalId
        {
            get => plantExternalId;
            set => plantExternalId = value;
        }
    }

    public new Department this[int i] => Nodes[i];

    #region Database Loading
    public void Fill(System.Data.IDbCommand deptTableCmd)
    {
        PtImportDataSet.DepartmentsDataTable deptTable = new ();

        FillTable(deptTable, deptTableCmd);
        Fill(deptTable);
    }

    /// <summary>
    /// Fill the transmission with data from the DataSet.
    /// </summary>
    /// <param name="a_deptTable"></param>
    public void Fill(PtImportDataSet.DepartmentsDataTable a_deptTable)
    {
        for (int i = 0; i < a_deptTable.Count; i++)
        {
            Add(new Department(a_deptTable[i]));
        }
    }
    #endregion

    public override string Description => string.Format("Departments updated ({0})".Localize(), Count);
}