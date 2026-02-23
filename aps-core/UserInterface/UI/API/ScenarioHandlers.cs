//using System.Collections.Generic;

//using PT.ScenarioControls.LegacyV11Controls.DockControls;
//using PT.ScenarioControls.LegacyV11Controls.DockControls.InventoryPlan;
//using PT.ScenarioControls.ScenarioViewer;
//using PT.Scheduler;
//using PT.SchedulerDefinitions;

//namespace UI.API
//{
//    public class ScenarioHandlers
//    {
//        public static void HandleRequest(string a_path, Dictionary<string, string> a_params)
//        {
//            switch (a_path)
//            {
//                case ScenarioViewer.Panes.Activities:
//                    Activities();
//                    break;
//                case ScenarioViewer.Panes.Jobs:
//                    HandleJobs(a_params);
//                    break;
//                case ScenarioViewer.Panes.Gantt:
//                    HandleGantt(a_params);
//                    break;
//                case ScenarioViewer.Panes.InventoryPlan:
//                    HandleInventoryPlan(a_params);
//                    break;
//                case "Optimize":
//                    Optimize();
//                    break;
//                case "AdvanceClock":
//                    handleAdvanceClock(a_params);
//                    break;
//                case "Refresh":
//                    Refresh();
//                    break;
//                case "Publish":
//                    Publish();
//                    break;
//                default:
//                    HandleOther(a_path, a_params);
//                    break;
//            }
//        }

//        /// <summary>
//        /// Show Activities Pane
//        /// </summary>
//        /// <example>app://aps/Activities</example>
//        public static void Activities()
//        {
//            GetActiveScenarioViewer().ShowActivitySchedulingGridPane(true);
//        }

//        const string c_layoutKey = "Layout";

//        /// <summary>
//        /// Show Jobs Pane and if provided displays a Layout
//        /// </summary>
//        /// <param name="a_layoutName">optional name of the tab to select</param>
//        /// <example>app://aps/Jobs?Layout=Unscheduled</example>
//        internal static void HandleJobs(Dictionary<string, string> a_paramDict)
//        {
//            string value;
//            string layoutName = a_paramDict.TryGetValue(c_layoutKey, out value) ? value : null;

//            if (!string.IsNullOrEmpty(layoutName))
//            {
//                JobsGrid jobsGrid = GetActiveScenarioViewer().GetJobsControl(true);
//                jobsGrid.ProcessAlert(layoutName);
//            }

//            GetActiveScenarioViewer().ShowJobsPane(false);
//        }

//        static void HandleGantt(Dictionary<string, string> a_params)
//        {
//            string value;
//            string plantName = a_params.TryGetValue("PlantName", out value) ? value : null;
//            string deptName = a_params.TryGetValue("DepartmentName", out value) ? value : null;
//            Gantt(plantName, deptName);
//        }

//        /// <summary>
//        /// Show Gantt Pane and if provided select specified Plant and Department
//        /// </summary>
//        /// <param name="PlantName">optional name of the Plant to select</param>
//        /// <param name="DepartmentName">optional name of the Department to select</param>
//        /// <example>app://aps/Gantt?PlantName=myPlant</example>
//        public static void Gantt(string PlantName = null, string DepartmentName = null)
//        {
//            GetActiveScenarioViewer().ShowGanttPane(true);
//            PT.GanttDotNet.PlantsGantt plantsGantt = GetActiveScenarioViewer().GanttView.PlantsGantt;

//            if (PlantName != null)
//            {
//                plantsGantt.SelectPlantByName(PlantName);
//            }
//            if (DepartmentName != null)
//            {
//                plantsGantt.SelectDepartmentByName(DepartmentName);
//            }
//        }

//        /// <summary>
//        /// Show InventoryPlan pane and load a layout if provided
//        /// </summary>
//        /// <param name="a_items">optional comma seperated name of items to filter by</param>
//        /// <param name="a_warehouses">optional comma seperated name of warehouses to filter by</param>
//        /// <example>app://aps/InventoryPlan?Layout=shortages</example>
//        static void HandleInventoryPlan(Dictionary<string, string> a_paramDict)
//        {
//            string value;
//            string layoutName = a_paramDict.TryGetValue(c_layoutKey, out value) ? value : null;

//            if (!string.IsNullOrEmpty(layoutName))
//            {
//                InventoryPaneLayoutControl invPlanControl = GetActiveScenarioViewer().GetInventoryPlanControl(true);
//                invPlanControl.ProcessAlert(layoutName);
//            }

//            GetActiveScenarioViewer().ShowInventoryPlanPane(false);
//        }

//        /// <summary>
//        /// Optimize the current Simulation.
//        /// </summary>
//        public static void Optimize()
//        {
//            GetActiveScenarioViewer().SendOptimizeT(-1);
//        }

//        static void handleAdvanceClock(Dictionary<string, string> a_paramDict)
//        {
//            string value;
//            string dtStr = a_paramDict.TryGetValue("DateTime", out value) ? value : null;
//            AdvanceClock(dtStr);
//        }

//        /// <summary>
//        /// Trigger an AdvanceClock
//        /// </summary>
//        /// <param name="DateTime">Optional string that specifies a valid date to Advance the clock to. If not specified Clock is Advanced to Today. Valid values are 'Now', 'Today' or a string representation of a DateTime</param>
//        public static void AdvanceClock(string DateTime = null)
//        {
//            System.DateTime dt = System.DateTime.MinValue;

//            if (DateTime == null || DateTime.ToUpper() == "TODAY")
//            {
//                dt = System.DateTime.Today;
//            }
//            else if (DateTime.ToUpper() == "NOW")
//            {
//                dt = System.DateTime.Now;
//            }
//            else
//            {
//                try
//                {
//                    dt = System.DateTime.Parse(DateTime);
//                }
//                catch { }
//            }

//            if (dt != System.DateTime.MinValue)
//            {
//                MainForm.MainFormInstance.SendClockAdvanceT(dt);
//            }
//        }

//        /// <summary>
//        /// Run import to refresh planning data.
//        /// </summary>
//        public static void Refresh()
//        {
//            MainForm.MainFormInstance.RunERPImport(BaseId.NULL_ID);
//        }

//        /// <summary>
//        /// Export current Scenario's data.
//        /// </summary>
//        public static void Publish()
//        {
//            GetActiveScenarioViewer().Publish(EExportDestinations.All, false);
//        }

//        static void HandleOther(string a_path, Dictionary<string, string> a_paramDict)
//        {
//            if (GetActiveScenarioViewer().PaneExists(a_path))
//            {
//                GetActiveScenarioViewer().ShowPane(a_path, true);
//            }
//            else
//            {
//                MainForm.MainFormInstance.PerformToolAction(a_path, "");
//            }
//        }

//        static ScenarioViewer GetActiveScenarioViewer()
//        {
//            //TODO: V12 ??
//            return null;
//        }
//    }
//}

