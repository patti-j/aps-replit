using System.Data;

using PT.Transmissions;

namespace PT.ERPTransmissions;

public class CellT : ERPMaintenanceTransmission<CellT.Cell>, IPTSerializable
{
    public new const int UNIQUE_ID = 218;

    #region PT Serialization
    public CellT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                Cell node = new (reader);
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

    public CellT() { } // reqd. for xml serialization

    public class Cell : PTObjectBase, IPTSerializable
    {
        public new const int UNIQUE_ID = 219;

        #region PT Serialization
        public Cell(IReader a_reader)
            : base(a_reader)
        {
            if (a_reader.VersionNumber >= 12054)
            {
                //nothing to deserialize in these version
            }
            else
            {
                a_reader.Read(out int conWipMoLimit); //deprecated
                a_reader.Read(out bool enforceConWipMoLimit); //deprecated
            }
        }

        public override void Serialize(IWriter writer)
        {
            base.Serialize(writer);
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        public Cell() { } // reqd. for xml serialization

        public Cell(string a_externalId, string a_name, string a_description, string a_notes, string a_userFields)
            : base(a_externalId, a_name, a_description, a_notes, a_userFields) { }

        public Cell(PtImportDataSet.CellsRow a_cellsRow)
            : base(a_cellsRow.ExternalId, a_cellsRow.Name, a_cellsRow.IsDescriptionNull() ? null : a_cellsRow.Description, a_cellsRow.IsNotesNull() ? null : a_cellsRow.Notes, a_cellsRow.IsUserFieldsNull() ? null : a_cellsRow.UserFields)
        {
            ExternalId = a_cellsRow.ExternalId;
        }
    }

    public new Cell this[int i] => Nodes[i];

    public void Fill(IDbCommand a_cellsCmd)
    {
        PtImportDataSet ds = new ();
        FillTable(ds.Cells, a_cellsCmd);

        Fill(ds);
    }

    private void Fill(PtImportDataSet a_ds)
    {
        foreach (PtImportDataSet.CellsRow cellsRow in a_ds.Cells)
        {
            Add(new Cell(cellsRow));
        }
    }
}