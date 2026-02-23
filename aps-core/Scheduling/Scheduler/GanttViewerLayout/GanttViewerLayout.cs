using PT.APSCommon;
using PT.Common.Exceptions;

namespace PT.Scheduler;

/// <summary>
/// Each ScenarioDetail has a GanttViewerLayout object that defines how the Scenario will be displayed in the UI ScheduleViewer.
/// </summary>
[Serializable]
public class GanttViewerLayout : IScenarioRef, IPTSerializable
{
    public const int UNIQUE_ID = 391;

    #region IPTSerializable Members
    public GanttViewerLayout(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            previousPaneId = new BaseId(reader);
            previousSetId = new BaseId(reader);
            previousGanttViewId = new BaseId(reader);
            previousGanttRowId = new BaseId(reader);
            ganttViewerPanes = new GanttViewerPanesCollection(reader);
        }
    }

    public void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif
        previousPaneId.Serialize(writer);
        previousSetId.Serialize(writer);
        previousGanttViewId.Serialize(writer);
        previousGanttRowId.Serialize(writer);
        ganttViewerPanes.Serialize(writer);
    }

    public int UniqueId => UNIQUE_ID;

    internal void RestoreReferences(Scenario scenario, ScenarioDetail sd, PlantManager plants)
    {
        this.scenario = scenario;
        scenarioDetail = sd;
        for (int i = 0; i < ganttViewerPanes.Count; i++)
        {
            ganttViewerPanes.GetByIndex(i).RestoreReferences(plants);
        }
    }
    #endregion

    protected BaseId previousPaneId = new (0);

    public BaseId NextPaneID()
    {
        BaseId nextId = previousPaneId.NextId;
        previousPaneId = nextId;
        return nextId;
    }

    protected BaseId previousSetId = new (0);

    public BaseId NextSetID()
    {
        BaseId nextId = previousSetId.NextId;
        previousSetId = nextId;
        return nextId;
    }

    protected BaseId previousGanttViewId = new (0);

    public BaseId NextGanttViewID()
    {
        BaseId nextId = previousGanttViewId.NextId;
        previousGanttViewId = nextId;
        return nextId;
    }

    protected BaseId previousGanttRowId = new (0);

    public BaseId NextGanttRowID()
    {
        BaseId nextId = previousGanttRowId.NextId;
        previousGanttRowId = nextId;
        return nextId;
    }

    [NonSerialized] private Scenario scenario;

    public GanttViewerLayout(Scenario scenario)
    {
        this.scenario = scenario;
        //There's always at least one Pane, so create it now.
        GanttViewerPanes.Add(new GanttViewerPane(NextPaneID(), BaseId.NULL_ID, true, 0));
        //Add Pane 1 to be the Jobs Pane
        GanttViewerPanes.Add(new GanttViewerPane(NextPaneID(), BaseId.NULL_ID, true, 1));

        AddJobGantts();
    }

    [NonSerialized] private ScenarioDetail scenarioDetail;

    private GanttViewerPanesCollection ganttViewerPanes = new ();

    /// <summary>
    /// Collection of the GanttViewerLayout's Panes.
    /// </summary>
    public GanttViewerPanesCollection GanttViewerPanes => ganttViewerPanes;

    #region GanttViewerPanesCollection
    /// <summary>
    /// Stores a list of GanttViewerPanes.
    /// </summary>
    public class GanttViewerPanesCollection : IPTSerializable
    {
        public const int UNIQUE_ID = 392;

        #region IPTSerializable Members
        public GanttViewerPanesCollection(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                previousId = new BaseId(reader);

                int count;
                reader.Read(out count);
                for (int i = 0; i < count; i++)
                {
                    GanttViewerPane pane = new (reader);
                    AddPane(pane);
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
        #endregion

        #region Declarations
        private readonly SortedList<BaseId, GanttViewerPane> ganttViewerPanes = new ();

        public class GanttViewerPanesCollectionException : PTException
        {
            public GanttViewerPanesCollectionException(string a_message) : base(a_message) { }
        }
        #endregion

        #region Construction
        public GanttViewerPanesCollection() { }
        #endregion

        #region Properties and Methods
        private BaseId previousId = new (0);

        private BaseId GetNextId()
        {
            BaseId nextId = previousId.NextId;
            previousId = nextId;
            return nextId;
        }

        internal GanttViewerPane Add(GanttViewerPane a_ganttViewerPane)
        {
            a_ganttViewerPane.Id = GetNextId();
            ganttViewerPanes.Add(a_ganttViewerPane.Id, a_ganttViewerPane);
            return a_ganttViewerPane;
        }

        internal GanttViewerPane AddPane(GanttViewerPane a_ganttViewerPane)
        {
            ganttViewerPanes.Add(a_ganttViewerPane.Id, a_ganttViewerPane);
            return a_ganttViewerPane;
        }

        public void Remove(int a_index)
        {
            ganttViewerPanes.RemoveAt(a_index);
        }

        public GanttViewerPane Find(BaseId a_id)
        {
            return ganttViewerPanes[a_id];
        }

        public GanttViewerPane GetByIndex(int a_index)
        {
            return ganttViewerPanes.Values[a_index];
        }

        public int Count => ganttViewerPanes.Count;
        #endregion
    }
    #endregion

    #region Edit Functions
    public void Add(Plant p)
    {
        ScenarioEvents se;
        //Add a GanttViewSet.
        GanttViewerPane pane = GanttViewerPanes.GetByIndex(0); //Always add to Pane zero.
        GanttViewSet ganttViewSet = pane.GanttViewSets.Add(new GanttViewSet(NextSetID(), BaseId.NULL_ID, true, pane.GanttViewSets.Count, p.Name, p.Description, p, GanttViewSet.GanttViewSetTypes.Plant));
        using (scenario.ScenarioEventsLock.EnterRead(out se))
        {
            se.FireGanttViewSetAddEvent(pane, ganttViewSet);
        }

        //NEW Add Gantt View for Plant
        GanttView ganttView = new (NextGanttViewID(), BaseId.NULL_ID, true, ganttViewSet.GanttViews.Count, p.Name, p.Description, p, GanttView.GanttViewTypes.OnePlantsResources);
        ganttViewSet.GanttViews.Add(ganttView);
        using (scenario.ScenarioEventsLock.EnterRead(out se))
        {
            se.FireGanttViewAddEvent(pane, ganttViewSet, ganttView);
        }
    }

    public void Add(Department w)
    {
        ScenarioEvents se;
        for (int p = 0; p < GanttViewerPanes.Count; p++)
        {
            GanttViewerPane pane = GanttViewerPanes.GetByIndex(p);
            for (int s = 0; s < pane.GanttViewSets.Count; s++)
            {
                GanttViewSet ganttViewSet = pane.GanttViewSets.GetByIndex(s);
                if (ganttViewSet.SchedulerObject == w.Plant)
                {
                    GanttView ganttView = new (NextGanttViewID(), BaseId.NULL_ID, true, ganttViewSet.GanttViews.Count, w.Name, w.Description, w, GanttView.GanttViewTypes.AllResources);
                    ganttViewSet.GanttViews.Add(ganttView);
                    using (scenario.ScenarioEventsLock.EnterRead(out se))
                    {
                        se.FireGanttViewAddEvent(pane, ganttViewSet, ganttView);
                    }
                }
            }
        }
    }

    public void Add(Resource m)
    {
        ScenarioEvents se;
        for (int p = 0; p < GanttViewerPanes.Count; p++)
        {
            GanttViewerPane pane = GanttViewerPanes.GetByIndex(p);
            for (int s = 0; s < pane.GanttViewSets.Count; s++)
            {
                GanttViewSet ganttViewSet = pane.GanttViewSets.GetByIndex(s);
                if (ganttViewSet.SchedulerObject == m.Department.Plant)
                {
                    for (int v = 0; v < ganttViewSet.GanttViews.Count; v++)
                    {
                        GanttView ganttView = ganttViewSet.GanttViews.GetByIndex(v);
                        if (ganttView.SchedulerObject == m.Department.Plant) //Gantt View for Plant
                        {
                            GanttViewRow row = new (NextGanttRowID(), BaseId.NULL_ID, true, ganttView.GanttViewRows.Count, m.Name, m.Description, m, GanttViewRow.GanttViewRowTypes.Resource);
                            ganttView.GanttViewRows.Add(row);
                            using (scenario.ScenarioEventsLock.EnterRead(out se))
                            {
                                se.FireGanttViewRowAddEvent(pane, ganttViewSet, ganttView, row);
                            }
                        }

                        if (ganttView.SchedulerObject == m.Department) //Gantt View for Department
                        {
                            GanttViewRow row = new (NextGanttRowID(), BaseId.NULL_ID, true, ganttView.GanttViewRows.Count, m.Name, m.Description, m, GanttViewRow.GanttViewRowTypes.Resource);
                            ganttView.GanttViewRows.Add(row);
                            using (scenario.ScenarioEventsLock.EnterRead(out se))
                            {
                                se.FireGanttViewRowAddEvent(pane, ganttViewSet, ganttView, row);
                            }
                        }
                    }
                }
            }
        }
    }

    private void AddJobGantts()
    {
        //Add a GanttViewSet.
        GanttViewerPane pane = GanttViewerPanes.GetByIndex(1); //Always add to Pane 1.
        GanttViewSet ganttViewSet = pane.GanttViewSets.Add(new GanttViewSet(NextSetID(), BaseId.NULL_ID, true, pane.GanttViewSets.Count, "Jobs", "Job Gantts", null, GanttViewSet.GanttViewSetTypes.Jobs));
        //Add Gantt View for Overdue Jobs
        AddJobGantt(ganttViewSet, "Overdue", "Jobs that should have been finished already", GanttView.GanttViewTypes.OverdueJobs);
        AddJobGantt(ganttViewSet, "Late", "Jobs that are scheduled to finish after their NeedDate", GanttView.GanttViewTypes.LateJobs);
        AddJobGantt(ganttViewSet, "Hot", "Jobs that are marked as Hot", GanttView.GanttViewTypes.HotJobs);
        AddJobGantt(ganttViewSet, "All", "All Jobs", GanttView.GanttViewTypes.AllJobs);
    }

    private void AddJobGantt(GanttViewSet ganttViewSet, string name, string description, GanttView.GanttViewTypes viewType)
    {
        GanttView ganttView = new (NextGanttViewID(), BaseId.NULL_ID, true, ganttViewSet.GanttViews.Count, name, description, null, viewType);
        ganttViewSet.GanttViews.Add(ganttView);
    }

    public void Add(BaseObject o)
    {
        // *LRH*Jim I commented this line out to prevent PT from going down.
        //************************************************************************************************
        //			throw new ApplicationException("GanttViewereLayout.Add() not handling objects of this type yet: " + o.GetType().ToString());
        //************************************************************************************************
    }

    public void Delete(BaseObject o)
    {
        // *LRH*Jim I commented this line out to prevent PT from going down.
        //************************************************************************************************
        //			throw new ApplicationException("GanttViewereLayout.Delete() not handling objects of this type yet: " + o.GetType().ToString());
        //************************************************************************************************
    }

    public void Delete(Plant p)
    {
        ScenarioEvents se;
        for (int i = 0; i < GanttViewerPanes.Count; i++)
        {
            GanttViewerPane pane = GanttViewerPanes.GetByIndex(i);
            for (int s = 0; s < pane.GanttViewSets.Count; s++)
            {
                GanttViewSet ganttViewSet = pane.GanttViewSets.GetByIndex(s);
                if (ganttViewSet.SchedulerObject == p)
                {
                    pane.GanttViewSets.Remove(s);
                    using (scenario.ScenarioEventsLock.EnterRead(out se))
                    {
                        se.FireGanttViewSetRemoveEvent(pane, ganttViewSet);
                    }
                }
            }
        }
    }

    public void Delete(BaseResource br)
    {
        for (int p = 0; p < GanttViewerPanes.Count; p++)
        {
            GanttViewerPane pane = GanttViewerPanes.GetByIndex(p);
            for (int s = 0; s < pane.GanttViewSets.Count; s++)
            {
                GanttViewSet ganttViewSet = pane.GanttViewSets.GetByIndex(s);
                if (ganttViewSet.SchedulerObject == br.Department.Plant)
                {
                    for (int v = 0; v < ganttViewSet.GanttViews.Count; v++)
                    {
                        GanttView ganttView = ganttViewSet.GanttViews.GetByIndex(v);
                        if (ganttView.SchedulerObject == br.Department)
                        {
                            GanttViewRow row = ganttView.Find(br);
                            if (row != null)
                            {
                                ganttView.Remove(row);
                                ScenarioEvents se;
                                using (scenario.ScenarioEventsLock.EnterRead(out se))
                                {
                                    se.FireGanttViewRowRemoveEvent(pane, ganttViewSet, ganttView, row);
                                }
                            }
                        }
                        else if (ganttView.SchedulerObject == br.Department.Plant)
                        {
                            GanttViewRow row = ganttView.Find(br);
                            if (row != null)
                            {
                                ganttView.Remove(row);
                                ScenarioEvents se;
                                using (scenario.ScenarioEventsLock.EnterRead(out se))
                                {
                                    se.FireGanttViewRowRemoveEvent(pane, ganttViewSet, ganttView, row);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void Delete(Department w)
    {
        ScenarioEvents se;
        for (int p = 0; p < GanttViewerPanes.Count; p++)
        {
            GanttViewerPane pane = GanttViewerPanes.GetByIndex(p);
            for (int s = 0; s < pane.GanttViewSets.Count; s++)
            {
                GanttViewSet ganttViewSet = pane.GanttViewSets.GetByIndex(s);
                if (ganttViewSet.SchedulerObject == w.Plant)
                {
                    for (int v = 0; v < ganttViewSet.GanttViews.Count; v++)
                    {
                        GanttView ganttView = ganttViewSet.GanttViews.GetByIndex(v);
                        if (ganttView.SchedulerObject == w)
                        {
                            ganttViewSet.GanttViews.Remove(v);
                            using (scenario.ScenarioEventsLock.EnterRead(out se))
                            {
                                se.FireGanttViewRemoveEvent(pane, ganttViewSet, ganttView);
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion Edit Functions

    #region Find Functions
    public GanttView Find(BaseId a_ganttViewId)
    {
        GanttView g;
        for (int i = 0; i < GanttViewerPanes.Count; i++)
        {
            GanttViewerPane pane = GanttViewerPanes.GetByIndex(i);
            for (int p = 0; p < pane.GanttViewSets.Count; p++)
            {
                GanttViewSet ganttSet = pane.GanttViewSets.GetByIndex(p);
                g = ganttSet.GanttViews.Find(a_ganttViewId);
                if (g != null)
                {
                    return g;
                }
            }
        }

        return null;
    }

    public GanttView Find(BaseResource a_resource)
    {
        for (int i = 0; i < GanttViewerPanes.Count; i++)
        {
            GanttViewerPane pane = GanttViewerPanes.GetByIndex(i);
            for (int p = 0; p < pane.GanttViewSets.Count; p++)
            {
                GanttViewSet ganttSet = pane.GanttViewSets.GetByIndex(p);
                for (int n = 0; n < ganttSet.GanttViews.Count; n++)
                {
                    GanttView ganttView = ganttSet.GanttViews.GetByIndex(n);
                    GanttViewRow row = ganttView.Find(a_resource);
                    if (row != null)
                    {
                        return ganttView;
                    }
                }
            }
        }

        return null;
    }
    #endregion

    #region IScenarioRef
    void IScenarioRef.SetReferences(Scenario a_scenario, ScenarioDetail a_scenarioDetail)
    {
        if (scenario == null)
        {
            scenario = a_scenario;
            scenarioDetail = a_scenarioDetail;
            ScenarioRef.SetRef(this, a_scenario, a_scenarioDetail);
        }
    }
    #endregion
}