using System.Collections;

namespace PT.ImportDefintions;

[Serializable]
public class TableInfo : IPTSerializable
{
    public const int UNIQUE_ID = 375;

    #region IPTSerializable Members
    public TableInfo(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out tableName);
            reader.Read(out tableDescription);
        }
    }

    public void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif
        writer.Write(tableName);
        writer.Write(tableDescription);
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public TableInfo(string tableName, string tableDescription)
    {
        this.tableName = tableName;
        this.tableDescription = tableDescription;
    }

    private string tableName;

    public string TableName => tableName;

    private string tableDescription;

    public string TableDescription => tableDescription;
}

[Serializable]
public class TableList : IPTSerializable
{
    public const int UNIQUE_ID = 376;

    #region IPTSerializable Members
    public TableList(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                TableInfo t = new (reader);
                Add(t);
            }
        }
    }

    public void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif
        writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(writer);
        }
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public TableList() { }

    private ArrayList tableInfos = new ();

    public int Count => tableInfos.Count;

    public TableInfo Add(TableInfo tableInfo)
    {
        tableInfos.Add(tableInfo);
        return tableInfo;
    }

    public void Clear()
    {
        tableInfos.Clear();
    }

    public TableInfo this[int i] => (TableInfo)tableInfos[i];
}