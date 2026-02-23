namespace PT.SchedulerDefinitions;

/// <summary>
/// GanttView attributes that are kept the same across all Gantts when SynchronizeGanttViews is true.
/// </summary>
[Serializable]
public class GanttViewLayout : IPTSerializable
{
    public const int UNIQUE_ID = 174;

    #region IPTSerializable Members
    public GanttViewLayout(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out treeWidth);
            reader.Read(out rowSliderPosition);

            reader.Read(out gantt1VisibleRangeStart);
            reader.Read(out gantt1VisibleRangeEnd);

            rowInfos = new RowInfoList(reader);

            bool haveTopRowResource;
            reader.Read(out haveTopRowResource);
            if (haveTopRowResource)
            {
                topRowResource = new ResourceKey(reader);
            }
            else
            {
                topRowResource = null;
            }
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(treeWidth);
        writer.Write(rowSliderPosition);

        writer.Write(gantt1VisibleRangeStart);
        writer.Write(gantt1VisibleRangeEnd);

        rowInfos.Serialize(writer);

        writer.Write(topRowResource != null);
        if (topRowResource != null)
        {
            topRowResource.Serialize(writer);
        }
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public GanttViewLayout() { }
    private RowInfoList rowInfos = new ();

    public RowInfoList RowInfos => rowInfos;

    private ResourceKey topRowResource;

    /// <summary>
    /// The key of the resource that is currently vertically scrolled to the top of the GanttView.
    /// </summary>
    public ResourceKey TopRowResource
    {
        get => topRowResource;
        set => topRowResource = value;
    }

    //Gantt1
    private DateTime gantt1VisibleRangeStart;

    public DateTime Gantt1VisibleRangeStart
    {
        get => gantt1VisibleRangeStart;
        set => gantt1VisibleRangeStart = value;
    }

    private DateTime gantt1VisibleRangeEnd;

    public DateTime Gantt1VisibleRangeEnd
    {
        get => gantt1VisibleRangeEnd;
        set => gantt1VisibleRangeEnd = value;
    }

    private int treeWidth;

    /// <summary>
    /// The width of the Gantt Tree in pixels
    /// </summary>
    public int TreeWidth
    {
        get => treeWidth;
        set
        {
            if (value > 0)
            {
                treeWidth = value;
            }
            else
            {
                Exception e = new ("Tree Width must be non-negative.");
                throw e;
            }
        }
    }

    private int rowSliderPosition = 95;

    /// <summary>
    /// Stores the current position of the slider on the gantt that is used to set row heights.
    /// </summary>
    public int RowSliderPosition
    {
        get => rowSliderPosition;
        set => rowSliderPosition = value;
    }

    /// <summary>
    /// Stores the information for one Gantt Row.
    /// </summary>
    public class RowInfo : IPTSerializable
    {
        public const int UNIQUE_ID = 175;

        #region IPTSerializable Members
        public RowInfo(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                reader.Read(out rowHeight);

                resourceKey = new ResourceKey(reader);
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(rowHeight);

            resourceKey.Serialize(writer);
        }

        public virtual int UniqueId => UNIQUE_ID;
        #endregion

        public RowInfo(ResourceKey resourceKey, int rowHeight)
        {
            this.resourceKey = resourceKey;
            this.rowHeight = rowHeight;
        }

        private readonly ResourceKey resourceKey;

        /// <summary>
        /// Resource represented by the row in the gantt.
        /// </summary>
        public ResourceKey BaseResourceKey => resourceKey;

        private readonly int rowHeight;

        /// <summary>
        /// Height of the row in the gantt.
        /// </summary>
        public int RowHeight => rowHeight;
    }

    /// <summary>
    /// Stores a list of Row Info objects that pertain to the rows of a GanttView.
    /// </summary>
    [Serializable]
    public class RowInfoList : IPTSerializable
    {
        public const int UNIQUE_ID = 176;

        #region IPTSerializable Members
        public RowInfoList(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                int count;
                reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    RowInfo rowInfo = new (reader);
                    Add(rowInfo);
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

        public virtual int UniqueId => UNIQUE_ID;
        #endregion

        public RowInfoList() { }

        private SortedList<ResourceKey, RowInfo> m_rows = new ();

        public RowInfo Add(RowInfo rowInfo)
        {
            m_rows.Add(rowInfo.BaseResourceKey, rowInfo);
            return rowInfo;
        }

        public int Count => m_rows.Count;

        public void Clear()
        {
            m_rows.Clear();
        }

        public RowInfo this[ResourceKey key] => m_rows[key];

        public RowInfo this[int index] => m_rows.Values[index];
    }
}