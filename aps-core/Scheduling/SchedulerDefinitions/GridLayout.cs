using System.Collections;

using PT.APSCommon.Extensions;

namespace PT.Scheduler;

/// <summary>
/// Contains a list of all Grid Layouts for one User.
/// </summary>
public class GridLayoutList : IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 585;
    private const string c_gridLayoutExtension = ".layout";
    private const string c_gridLayoutExtensionSearchString = "*" + c_gridLayoutExtension;

    /// <summary>
    /// The path directory for serializing gridlayout files on the client.
    /// </summary>
    private string m_rootGridLayoutPath = "";

    private string m_workspaceName;

    public string WorkSpace
    {
        set => m_workspaceName = value;
        private get => m_workspaceName;
    }

    public GridLayoutList(IReader reader)
    {
        Deserialize(reader);
    }

    public void InitForBackwardsCompatibility(string a_workSpace, string a_workspacePath)
    {
        m_gridLayouts.Clear();
        WorkSpace = a_workSpace;
        BinaryFileReader reader = null;
        try
        {
            //This is a local layout list that includes all workspaces
            m_rootGridLayoutPath = Path.Combine(a_workspacePath, "Layouts", "Grids");
            if (Directory.Exists(m_rootGridLayoutPath))
            {
                string[] files = Directory.GetFiles(m_rootGridLayoutPath, c_gridLayoutExtensionSearchString);
                foreach (string file in files)
                {
                    reader = new BinaryFileReader(file);
                    Add(new GridLayout(reader));
                }
            }

            //This includes only layouts for a specific workspace
            m_rootGridLayoutPath = Path.Combine(a_workspacePath, "WorkSpace".Localize(), WorkSpace, "Grids");
            if (Directory.Exists(m_rootGridLayoutPath))
            {
                string[] files = Directory.GetFiles(m_rootGridLayoutPath, c_gridLayoutExtensionSearchString);
                foreach (string file in files)
                {
                    reader = new BinaryFileReader(file);
                    Add(new GridLayout(reader));
                }
            }
        }
        catch
        {
            //File likely didn't exist. 
        }
        finally
        {
            reader?.Close();
        }
    }

    private void Deserialize(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 500)
        {
            a_reader.Read(out m_workspaceName);
            int count;
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                GridLayout layout = new (a_reader);
                Add(layout);
            }
        }
        else if (a_reader.VersionNumber >= 1)
        {
            int count;
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                GridLayout layout = new (a_reader);
                Add(layout);
            }
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(m_workspaceName);
        writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            GetByIndex(i).Serialize(writer);
        }
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public GridLayoutList()
    {
        WorkSpace = "Default".Localize();
    }

    private readonly SortedList<string, GridLayout> m_gridLayouts = new ();

    /// <summary>
    /// Adds a new Grid Layout.  If the grid already exists it is replaced.
    /// </summary>
    public void Add(GridLayout a_gridLayout)
    {
        Remove(a_gridLayout.Key);
        m_gridLayouts.Add(a_gridLayout.Key, a_gridLayout);
    }

    public void Remove(string a_key)
    {
        if (m_gridLayouts.ContainsKey(a_key))
        {
            m_gridLayouts.Remove(a_key);
        }
    }

    public int Count => m_gridLayouts.Count;

    /// <summary>
    /// Returns the Grid Layout with the specified Key or null if not found.
    /// </summary>
    public GridLayout this[string a_key]
    {
        get
        {
            if (m_gridLayouts.ContainsKey(a_key))
            {
                return m_gridLayouts[a_key];
            }

            return null;
        }
    }

    public GridLayout GetByIndex(int a_index)
    {
        return m_gridLayouts.Values[a_index];
    }

    public List<string> GetCustomGridLayoutNames(string a_keyStart)
    {
        List<string> layoutNamesList = new ();
        if (a_keyStart == null)
        {
            return layoutNamesList;
        }

        for (int i = 0; i < Count; i++)
        {
            GridLayout currentLayout = GetByIndex(i);
            if (currentLayout.Key.StartsWith(a_keyStart))
            {
                layoutNamesList.Add(currentLayout.Key.Replace(a_keyStart, ""));
            }
        }

        return layoutNamesList;
    }
}

/// <summary>
/// Summary description for GridLayout.
/// Stores for all bands: column order, column sorts, filters, column widths, row heights
/// </summary>
public class GridLayout : IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 578;

    public GridLayout(IReader reader)
    {
        #region Version 602
        if (reader.VersionNumber >= 602) //Switched to binary format
        {
            reader.Read(out Key);
            reader.Read(out m_xmlDefinition);
            reader.Read(out m_layout);
            reader.Read(out m_showGroupByBox);
            reader.Read(out m_autoFitColumns);

            m_bandLayoutList = new BandLayoutList(reader);
        }
        #endregion

        #region Version 300
        else if (reader.VersionNumber >= 300) //Switched to XML format
        {
            reader.Read(out Key);
            reader.Read(out m_xmlDefinition);
            reader.Read(out m_showGroupByBox);
            reader.Read(out m_autoFitColumns);

            m_bandLayoutList = new BandLayoutList(reader);
        }
        #endregion

        #region Version 148
        else if (reader.VersionNumber >= 148)
        {
            reader.Read(out Key);
            reader.Read(out m_showGroupByBox);
            reader.Read(out m_autoFitColumns);

            m_bandLayoutList = new BandLayoutList(reader);
        }
        #endregion

        #region Version 1
        else if (reader.VersionNumber >= 1)
        {
            reader.Read(out Key);
            m_bandLayoutList = new BandLayoutList(reader);
        }
        #endregion
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(Key);
        writer.Write(m_xmlDefinition);
        writer.Write(m_layout);
        writer.Write(m_showGroupByBox);
        writer.Write(m_autoFitColumns);

        m_bandLayoutList.Serialize(writer);

        //writer.Write(this.ColumnFilters.Count);
        //for (int i = 0; i < this.ColumnFilters.Count; i++)
        //    this.ColumnFilters[i].Serialize(writer);
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    /// <summary>
    /// Default constructor. Stores the layout for one Infragistics grid.
    /// </summary>
    public GridLayout() { }

    /// <summary>
    /// Stores the layout for one Infragistics grid.
    /// </summary>
    /// <param name="a_key">Used to store and retrieve a particular grid layout.  Each Grid Layout in a Grid Layout List must be unique.</param>
    [Obsolete("This version saves layotus as xml. use the new version that saves it as binary.")]
    public GridLayout(string a_key, string aXmlDefinition)
    {
        Key = a_key;
        m_xmlDefinition = aXmlDefinition;
    }

    /// <summary>
    /// Stores the layout for one Infragistics grid.
    /// </summary>
    /// <param name="a_key">Used to store and retrieve a particular grid layout.  Each Grid Layout in a Grid Layout List must be unique.</param>
    public GridLayout(string a_key, byte[] a_displayLayout)
    {
        Key = a_key;
        m_layout = a_displayLayout;
    }

    public string Key;
    private readonly BandLayoutList m_bandLayoutList = new ();

    public BandLayoutList BandLayouts => m_bandLayoutList;

    private string m_xmlDefinition;

    /// <summary>
    /// The Display Layout's ToXML string.
    /// </summary>
    public string XmlDefinition
    {
        get => m_xmlDefinition;
        set => m_xmlDefinition = value;
    }

    private byte[] m_layout;

    /// <summary>
    /// The Display Layout's data.
    /// </summary>
    public byte[] DisplayLayout
    {
        get => m_layout;
        set => m_layout = value;
    }

    private bool m_showGroupByBox;

    public bool ShowGroupByBox
    {
        get => m_showGroupByBox;
        set => m_showGroupByBox = value;
    }

    private bool m_autoFitColumns;

    public bool AutoFitColumns
    {
        get => m_autoFitColumns;
        set => m_autoFitColumns = value;
    }

    #region Bands
    public class BandLayoutList : IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 579;

        public BandLayoutList(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                int count;
                reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    BandLayout layout = new (reader);
                    Add(layout);
                }
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(Count);
            for (int i = 0; i < Count; i++)
            {
                this[i].Serialize(writer);
            }
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public BandLayoutList() { }

        private readonly ArrayList m_bands = new ();

        public int Count => m_bands.Count;

        public void Add(BandLayout bandLayout)
        {
            m_bands.Add(bandLayout);
        }

        public BandLayout this[int index] => (BandLayout)m_bands[index];
    }

    /// <summary>
    /// Stores all information related to one Band in the grid's Display Layout.
    /// </summary>
    public class BandLayout : IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 580;

        public BandLayout(IReader reader)
        {
            if (reader.VersionNumber >= 148)
            {
                reader.Read(out CardView);
                int val;
                reader.Read(out val);
                CardStyle = (ECardStyles)val;

                m_columnLayoutList = new ColumnLayoutList(reader);
                m_sortedColumns = new SortedColumnList(reader);
                m_summaries = new SummaryList(reader);
            }

            #region Version 1
            else if (reader.VersionNumber >= 1)
            {
                m_columnLayoutList = new ColumnLayoutList(reader);
                m_sortedColumns = new SortedColumnList(reader);
            }
            #endregion
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(CardView);
            writer.Write((int)CardStyle);

            m_columnLayoutList.Serialize(writer);
            m_sortedColumns.Serialize(writer);
            m_summaries.Serialize(writer);
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public BandLayout() { }
        public bool CardView;

        public enum ECardStyles { Compressed, MergedLabels, StandardLabels, VariableHeight }

        public ECardStyles CardStyle;

        private readonly ColumnLayoutList m_columnLayoutList = new ();

        public ColumnLayoutList ColumnLayouts => m_columnLayoutList;

        private readonly SortedColumnList m_sortedColumns = new ();

        public SortedColumnList SortedColumns => m_sortedColumns;

        private readonly SummaryList m_summaries = new ();

        public SummaryList Summaries => m_summaries;
    }

    public class SortedColumnList : IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 581;

        public SortedColumnList(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                int count;
                reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    SortedColumn col = new (reader);
                    Add(col);
                }
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(Count);
            for (int i = 0; i < Count; i++)
            {
                this[i].Serialize(writer);
            }
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public SortedColumnList() { }

        public void Add(SortedColumn col)
        {
            sortedColumns.Add(col);
        }

        private readonly ArrayList sortedColumns = new ();

        public SortedColumn this[int index] => (SortedColumn)sortedColumns[index];

        public int Count => sortedColumns.Count;
    }

    public class SortedColumn : IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 582;

        public SortedColumn(IReader reader)
        {
            if (reader.VersionNumber >= 148)
            {
                reader.Read(out ColKey);
                reader.Read(out Descending);
                reader.Read(out IsGroupByColumn);
            }

            #region Version 1
            else if (reader.VersionNumber >= 1)
            {
                reader.Read(out ColKey);
                reader.Read(out Descending);
            }
            #endregion
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(ColKey);
            writer.Write(Descending);
            writer.Write(IsGroupByColumn);
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public SortedColumn(string a_colKey, bool a_descending, bool a_isGroupByColumn)
        {
            ColKey = a_colKey;
            Descending = a_descending;
            IsGroupByColumn = a_isGroupByColumn;
        }

        public string ColKey;
        public bool Descending;
        public bool IsGroupByColumn;
    }

    public class ColumnLayoutList : IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 583;

        public ColumnLayoutList(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                int count;
                reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    ColumnLayout newLayout = new (reader);
                    Add(newLayout);
                }
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(Count);
            for (int i = 0; i < Count; i++)
            {
                GetByIndex(i).Serialize(writer);
            }
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public ColumnLayoutList() { }

        private readonly SortedList<string, ColumnLayout> m_columnLayouts = new ();

        public int Count => m_columnLayouts.Count;

        public ColumnLayout this[string a_key] => m_columnLayouts[a_key];

        public ColumnLayout GetByIndex(int a_index)
        {
            return m_columnLayouts.Values[a_index];
        }

        public bool Contains(string key)
        {
            return m_columnLayouts.ContainsKey(key);
        }

        public void Add(ColumnLayout cl)
        {
            m_columnLayouts.Add(cl.Key, cl);
        }
    }

    public class ColumnLayout : IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 584;

        public ColumnLayout(IReader reader)
        {
            //TODO: These are the same, Why?
            if (reader.VersionNumber >= 148)
            {
                reader.Read(out Key);
                reader.Read(out VisiblePosition);
                reader.Read(out Hidden);
                reader.Read(out Width);
            }

            #region Version 1
            else if (reader.VersionNumber >= 1)
            {
                reader.Read(out Key);
                reader.Read(out VisiblePosition);
                reader.Read(out Hidden);
                reader.Read(out Width);
            }
            #endregion
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(Key);
            writer.Write(VisiblePosition);
            writer.Write(Hidden);
            writer.Write(Width);
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public ColumnLayout(string a_columnKey, int a_visiblePosition, bool a_hidden, int a_width)
        {
            Key = a_columnKey;
            VisiblePosition = a_visiblePosition;
            Hidden = a_hidden;
            Width = a_width;
        }

        public string Key;
        public int VisiblePosition;
        public bool Hidden;
        public int Width;
    }

    public class Summary : IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 586;

        public Summary(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                reader.Read(out ColumnKey);
                int val;
                reader.Read(out val);
                SummaryType = (ESummaryTypes)val;
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(ColumnKey);
            writer.Write((int)SummaryType);
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public Summary(string a_columnKey, ESummaryTypes a_summaryType)
        {
            ColumnKey = a_columnKey;
            SummaryType = a_summaryType;
        }

        public string ColumnKey;

        public enum ESummaryTypes
        {
            Average,
            Count,
            Custom,
            Formula,
            Maximum,
            Minimum,
            Sum
        }

        public ESummaryTypes SummaryType;
    }

    public class SummaryList : IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 587;

        public SummaryList(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                int count;
                reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    Add(new Summary(reader));
                }
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(Count);
            for (int i = 0; i < Count; i++)
            {
                this[i].Serialize(writer);
            }
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public SummaryList() { }

        private readonly List<Summary> m_summaries = new ();

        public int Count => m_summaries.Count;

        public void Add(Summary a_summary)
        {
            m_summaries.Add(a_summary);
        }

        public Summary this[int a_index] => m_summaries[a_index];
    }

    private readonly List<ColumnFilter> m_columnFilters = new ();

    public List<ColumnFilter> ColumnFilters => m_columnFilters;

    public class ColumnFilter : IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 656;

        public ColumnFilter(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                reader.Read(out columnKey);

                int count;
                reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    FilterConditions.Add(new FilterCondition(reader));
                }
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(columnKey);

            writer.Write(FilterConditions.Count);
            for (int i = 0; i < FilterConditions.Count; i++)
            {
                FilterConditions[i].Serialize(writer);
            }
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public ColumnFilter(string aColumnKey)
        {
            columnKey = aColumnKey;
        }

        private readonly string columnKey;

        public string ColumnKey => columnKey;

        private readonly List<FilterCondition> m_filterConditions = new ();

        public List<FilterCondition> FilterConditions => m_filterConditions;

        public class FilterCondition : IPTSerializable
        {
            #region IPTSerializable Members
            public const int UNIQUE_ID = 655;

            public FilterCondition(IReader reader)
            {
                if (reader.VersionNumber >= 1)
                {
                    reader.Read(out m_comparisonOperator);
                    reader.ReadBoxedPrimitiveAndCommonSystemStructs(out m_compareValue);
                }
            }

            public void Serialize(IWriter writer)
            {
                writer.Write(m_comparisonOperator);
                writer.WriteBoxedPrimitiveAndCommonSystemStructs(m_compareValue);
            }

            public int UniqueId => UNIQUE_ID;
            #endregion

            public FilterCondition(string aComparisonOperator, object aCompareValue)
            {
                m_comparisonOperator = aComparisonOperator;
                m_compareValue = aCompareValue;
            }

            private readonly string m_comparisonOperator;

            public string ComparisonOperator => m_comparisonOperator;

            private readonly object m_compareValue;

            public object CompareValue => m_compareValue;
        }
    }
    #endregion Bands
}