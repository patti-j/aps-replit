using System.ComponentModel;

using PT.APSCommon;
using PT.Common.Exceptions;

namespace PT.Scheduler;

/// <summary>
/// A collection of Resources that are shown together as one "tab" in the GanttVIewCollection tab page.  This corresponds to a single Gantt Chart.
/// </summary>
[Serializable]
public class GanttView : GanttViewerObjectBase2, IPTSerializable
{
    public new const int UNIQUE_ID = 390;

    #region IPTSerializable Members
    public GanttView(IReader a_reader) : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            int val;
            a_reader.Read(out val);
            m_ganttViewType = (GanttViewTypes)val;
            a_reader.Read(out m_generationAlgorithm);

            m_ganttViewRows = new GanttViewRowsCollection(a_reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write((int)m_ganttViewType);
        writer.Write(m_generationAlgorithm);

        m_ganttViewRows.Serialize(writer);
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    internal override void RestoreReferences(PlantManager a_plants)
    {
        base.RestoreReferences(a_plants);
        for (int i = 0; i < GanttViewRows.Count; i++)
        {
            GanttViewRows.GetByIndex(i).RestoreReferences(a_plants);
        }
    }
    #endregion

    /// <summary>
    /// Indicates the type of objecs that are represented by the GanttView and that corresonds to the GanttViewId.
    /// </summary>
    public enum GanttViewTypes
    {
        AllResources = 0,
        AllMainResources,
        AllInternalMainResources,
        AllHelperResources,
        AllCraftsmen,
        AllProductionLines,
        AllBatchProcessors,
        AllMultiPorts,
        AllSubcontractors,
        AllVesselTypes,
        MyResources,
        AllResourcesInDepartment,
        AllResourcesInCell,
        AllResourcesWithCapability,
        OnePlantsResources,
        OverdueJobs,
        LateJobs,
        HotJobs,
        AllJobs
    }

    public GanttView(BaseId id, BaseId creatorId, bool isPublic, int sortIndex, string name, string description, BaseObject schedulerObject, GanttViewTypes ganttViewType)
        : base(id, creatorId, isPublic, sortIndex, name, description, schedulerObject)
    {
        m_ganttViewType = ganttViewType;
    }

    private GanttViewTypes m_ganttViewType;

    /// <summary>
    /// Indicates the type of objecs that are represented by the GanttView and that corresonds to the GanttViewId.
    /// </summary>
    public GanttViewTypes GanttViewType
    {
        get => m_ganttViewType;
        set => m_ganttViewType = value;
    }

    private string m_generationAlgorithm;

    /// <summary>
    /// Name of the Class in GanttViewGeneration.dll to use for creating the GanttView.
    /// </summary>
    public string GenerationAlgorithm
    {
        get => m_generationAlgorithm;
        set => m_generationAlgorithm = value;
    }

    private GanttViewRowsCollection m_ganttViewRows = new ();

    /// <summary>
    /// Collection of the GanttView's GanttRows.
    /// </summary>
    public GanttViewRowsCollection GanttViewRows => m_ganttViewRows;

    public GanttViewRow Find(BaseResource a_resource)
    {
        for (int i = 0; i < GanttViewRows.Count; i++)
        {
            GanttViewRow row = GanttViewRows.GetByIndex(i);
            if (row.SchedulerObject == a_resource)
            {
                return row;
            }
        }

        return null;
    }

    public int FindRowIndex(BaseResource a_resource)
    {
        for (int i = 0; i < GanttViewRows.Count; i++)
        {
            GanttViewRow row = GanttViewRows.GetByIndex(i);
            if (row.SchedulerObject == a_resource)
            {
                return i;
            }
        }

        return -1;
    }

    public void Remove(GanttViewRow row)
    {
        GanttViewRows.Remove(row);
    }

    #region GanttViewRowsCollection
    /// <summary>
    /// Stores a list of GanttViewRows.
    /// </summary>
    public class GanttViewRowsCollection : IPTSerializable
    {
        public const int UNIQUE_ID = 389;

        #region IPTSerializable Members
        public GanttViewRowsCollection(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                int count;
                reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    GanttViewRow row = new (reader);
                    Add(row);
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
        #endregion

        #region Declarations
        private readonly SortedList<BaseId, GanttViewRow> m_ganttViewRows = new ();

        public class GanttViewRowsCollectionException : PTException
        {
            public GanttViewRowsCollectionException(string a_message) : base(a_message) { }
        }
        #endregion

        #region Construction
        public GanttViewRowsCollection() { }
        #endregion

        #region Properties and Methods
        private BaseId previousId = new (0);

        private BaseId GetNextId()
        {
            BaseId nextId = previousId.NextId;
            previousId = nextId;
            return nextId;
        }

        public GanttViewRow Add(GanttViewRow a_ganttViewRow)
        {
            m_ganttViewRows.Add(a_ganttViewRow.Id, a_ganttViewRow);
            return a_ganttViewRow;
        }

        public void Remove(int a_index)
        {
            m_ganttViewRows.RemoveAt(a_index);
        }

        public void Remove(GanttViewRow a_row)
        {
            m_ganttViewRows.Remove(a_row.Id);
        }

        public GanttViewRow Find(BaseId a_id)
        {
            return m_ganttViewRows[a_id];
        }

        public GanttViewRow GetByIndex(int a_index)
        {
            return m_ganttViewRows.Values[a_index];
        }

        public int Count => m_ganttViewRows.Count;
        #endregion
    }
    #endregion
}