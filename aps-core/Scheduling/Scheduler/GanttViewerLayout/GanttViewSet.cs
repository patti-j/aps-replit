using System.ComponentModel;

using PT.APSCommon;
using PT.Common.Exceptions;

namespace PT.Scheduler;

/// <summary>
/// A collection of GanttViews  that are shown together as one "tab" in the Schedule Viewer Pane.
/// </summary>
public class GanttViewSet : GanttViewerObjectBase2, IPTSerializable
{
    public new const int UNIQUE_ID = 395;

    #region IPTSerializable Members
    public GanttViewSet(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int val;
            reader.Read(out val);
            ganttViewSetType = (GanttViewSetTypes)val;
            reader.Read(out generationAlgorithm);
            ganttViews = new GanttViewsCollection(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write((int)ganttViewSetType);
        writer.Write(generationAlgorithm);
        ganttViews.Serialize(writer);
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    internal override void RestoreReferences(PlantManager plants)
    {
        base.RestoreReferences(plants);
        for (int i = 0; i < GanttViews.Count; i++)
        {
            GanttViews.GetByIndex(i).RestoreReferences(plants);
        }
    }
    #endregion

    /// <summary>
    /// Indicates the type of objecs that are represented by the GanttViewSet and that corresonds to the GanttViewSetId.
    /// </summary>
    public enum GanttViewSetTypes { All = 0, Plant, Jobs }

    public GanttViewSet(BaseId id, BaseId creatorId, bool isPublic, int sortIndex, string name, string description, BaseObject schedulerObject, GanttViewSetTypes GanttViewSetType)
        : base(id, creatorId, isPublic, sortIndex, name, description, schedulerObject)
    {
        this.GanttViewSetType = GanttViewSetType;
//            this.generationAlgorithm = generationAlgorithm;
    }

    private GanttViewSetTypes ganttViewSetType;

    /// <summary>
    /// Indicates the type of objecs that are represented by the GanttViewSet and that corresonds to the GanttViewSetId.
    /// </summary>
    public GanttViewSetTypes GanttViewSetType
    {
        get => ganttViewSetType;
        set => ganttViewSetType = value;
    }

    private string generationAlgorithm;

    /// <summary>
    /// Name of the Class in GanttViewSetGeneration.dll to use for creating the GanttViewSet.
    /// </summary>
    public string GenerationAlgorithm
    {
        get => generationAlgorithm;
        set => generationAlgorithm = value;
    }

    private readonly GanttViewsCollection ganttViews = new ();

    public GanttViewsCollection GanttViews => ganttViews;

    #region GanttViewsCollection
    /// <summary>
    /// Stores a list of GanttViews.
    /// </summary>
    public class GanttViewsCollection : IPTSerializable
    {
        public const int UNIQUE_ID = 396;

        #region IPTSerializable Members
        public GanttViewsCollection(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                previousId = new BaseId(reader);

                int count;
                reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    GanttView g = new (reader);
                    Add(g);
                }
            }
        }

        public void Serialize(IWriter writer)
        {
            #if DEBUG
            writer.DuplicateErrorCheck(this);
            #endif
            previousId.Serialize(writer);

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
        private readonly SortedList<BaseId, GanttView> m_ganttViews = new ();

        public class GanttViewsCollectionException : PTException
        {
            public GanttViewsCollectionException(string message)
                : base(message) { }
        }
        #endregion

        #region Construction
        public GanttViewsCollection() { }
        #endregion

        #region Properties and Methods
        private BaseId previousId = new (0);

        private BaseId GetNextId()
        {
            BaseId nextId = previousId.NextId;
            previousId = nextId;
            return nextId;
        }

        public GanttView Add(GanttView ganttView)
        {
            m_ganttViews.Add(ganttView.Id, ganttView);
            return ganttView;
        }

        public void Remove(int index)
        {
            m_ganttViews.RemoveAt(index);
        }

        public GanttView Find(BaseId id)
        {
            return m_ganttViews[id];
        }

        /// <summary>
        /// Finds the GanttView whose Scheduler object is the one specified.
        /// </summary>
        /// <param name="referencedObject"></param>
        /// <returns></returns>
        public GanttView Find(BaseObject referencedObject)
        {
            for (int i = 0; i < m_ganttViews.Count; i++)
            {
                GanttView ganttView = m_ganttViews.Values[i];
                if (ganttView.SchedulerObject == referencedObject)
                {
                    return ganttView;
                }
            }

            return null;
        }

        public GanttView GetByIndex(int index)
        {
            return m_ganttViews.Values[index];
        }

        public int Count => m_ganttViews.Count;
        #endregion
    }
    #endregion
}