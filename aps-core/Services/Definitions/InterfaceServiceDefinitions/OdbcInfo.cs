using System.Collections;

namespace PT.ImportDefintions;

/// <summary>
/// Stores information about Odbc connections.
/// </summary>
[Serializable]
public class OdbcInfo : IPTSerializable
{
    public const int UNIQUE_ID = 371;

    #region IPTSerializable Members
    public OdbcInfo(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            systemDataSources = new DataSources(reader);
        }
    }

    public virtual void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif

        systemDataSources.Serialize(writer);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public OdbcInfo()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    private DataSources systemDataSources = new ();

    /// <summary>
    /// List of all the System Data Sources on the server.
    /// </summary>
    public DataSources SystemDataSources => systemDataSources;

    [Serializable]
    public class DataSources : IPTSerializable
    {
        public const int UNIQUE_ID = 372;

        #region IPTSerializable Members
        public DataSources(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                int count;
                reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    DataSourceInfo d = new (reader);
                    Add(d);
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

        public DataSources() { }

        private ArrayList dataSourceInfos = new ();

        public int Count => dataSourceInfos.Count;

        public DataSourceInfo Add(DataSourceInfo dataSourceInfo)
        {
            dataSourceInfos.Add(dataSourceInfo);
            return dataSourceInfo;
        }

        public void Clear()
        {
            dataSourceInfos.Clear();
        }

        public DataSourceInfo this[int i] => (DataSourceInfo)dataSourceInfos[i];
    }

    [Serializable]
    public class DataSourceInfo : IPTSerializable
    {
        public const int UNIQUE_ID = 373;

        #region IPTSerializable Members
        public DataSourceInfo(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                reader.Read(out name);
                reader.Read(out description);
            }
        }

        public void Serialize(IWriter writer)
        {
            #if DEBUG
            writer.DuplicateErrorCheck(this);
            #endif

            writer.Write(name);
            writer.Write(description);
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public DataSourceInfo(string name)
        {
            this.name = name;
        }

        private string name;

        public string Name => name;

        private string description = "";

        public string Description
        {
            get => description;
            set => description = value;
        }
    }
}