using System.ComponentModel;

using PT.APSCommon;
using PT.Common.Exceptions;

namespace PT.Scheduler;

/// <summary>
/// The Schedule Viewer Control can be divided either vertically or horizontally into these adjacent ScheduleViewer Panes.  This allows for multiple GanttViewSets to be seen simultaneously.
/// </summary>
public class GanttViewerPane : GanttViewerObjectBase, IPTSerializable
{
    public new const int UNIQUE_ID = 393;

    #region IPTSerializable Members
    public GanttViewerPane(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            ganttViewSets = new GanttViewSetCollection(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        ganttViewSets.Serialize(writer);
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    internal void RestoreReferences(PlantManager plants)
    {
        ganttViewSets.RestoreReferences(plants);
    }
    #endregion

    public GanttViewerPane(BaseId id, BaseId creatorId, bool isPublic, int sortIndex)
        : base(id, creatorId, isPublic, sortIndex) { }

    private readonly GanttViewSetCollection ganttViewSets = new ();

    public GanttViewSetCollection GanttViewSets => ganttViewSets;

    #region GanttViewSetCollection
    /// <summary>
    /// Stores a list of GanttViewSets.
    /// </summary>
    public class GanttViewSetCollection : IPTSerializable
    {
        public const int UNIQUE_ID = 394;

        #region IPTSerializable Members
        public GanttViewSetCollection(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                int count;
                reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    GanttViewSet gvs = new (reader);
                    Add(gvs);
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
                GetByIndex(i).Serialize(writer);
            }
        }

        public int UniqueId => UNIQUE_ID;

        internal void RestoreReferences(PlantManager plants)
        {
            for (int i = 0; i < Count; i++)
            {
                GetByIndex(i).RestoreReferences(plants);
            }
        }
        #endregion

        #region Declarations
        private readonly SortedList<BaseId, GanttViewSet> m_ganttViewSets = new ();

        public class GanttViewSetCollectionException : PTException
        {
            public GanttViewSetCollectionException(string message)
                : base(message) { }
        }
        #endregion

        #region Construction
        public GanttViewSetCollection() { }
        #endregion

        #region Properties and Methods
        private BaseId previousId = new (0);

        private BaseId GetNextId()
        {
            BaseId nextId = previousId.NextId;
            previousId = nextId;
            return nextId;
        }

        internal GanttViewSet Add(GanttViewSet ganttViewSet)
        {
            m_ganttViewSets.Add(ganttViewSet.Id, ganttViewSet);
            return ganttViewSet;
        }

        public void Remove(int index)
        {
            m_ganttViewSets.RemoveAt(index);
        }

        public GanttViewSet Find(BaseId id)
        {
            return m_ganttViewSets[id];
        }

        public GanttViewSet GetByIndex(int index)
        {
            return m_ganttViewSets.Values[index];
        }

        public int Count => m_ganttViewSets.Count;
        #endregion
    }
    #endregion
}