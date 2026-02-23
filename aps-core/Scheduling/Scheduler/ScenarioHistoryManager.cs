using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Summary description for ScenarioHistoryManager.
/// </summary>
public class ScenarioHistoryManager : IScenarioRef, IPTSerializable
{
    public const int UNIQUE_ID = 341;

    #region IPTSerializable Members
    public ScenarioHistoryManager(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                ScenarioHistory h = new (reader);
                histories.Add(h);
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

    internal void RestoreReferences(ScenarioDetail sd)
    {
        scenarioDetail = sd;
    }
    #endregion

    #region Declarations
    private readonly List<ScenarioHistory> histories = new ();

    public class ScenarioHistoryManagerException : PTException
    {
        public ScenarioHistoryManagerException(string message)
            : base(message) { }
    }
    #endregion

    #region Construction
    private ScenarioDetail scenarioDetail;

    public ScenarioHistoryManager(ScenarioDetail sd)
    {
        scenarioDetail = sd;
    }
    #endregion

    #region History Recording
    private ScenarioHistory Add(ScenarioHistory h)
    {
        long maxCount = scenarioDetail.ScenarioOptions.MaxHistoriesCount;

        histories.Add(h);

        //Keep the list from getting too long.
        long removeCount = histories.Count - maxCount;
        for (long i = 0; i < removeCount; i++)
        {
            histories.RemoveAt(0);
        }

        return h;
    }

    /// <summary>
    /// Add a history to the History Manager based on the transmission.
    /// </summary>
    /// <param name="o"></param>
    /// <param name="t"></param>
    public void RecordScenarioHistory(ScenarioBaseT t)
    {
        BaseId[] key = null;
        string objectTypeName = "";
        ScenarioHistory.historyTypes historyType = ScenarioHistory.historyTypes.Miscellaneous;

        if (t is ERPTransmissions.ERPTransmission)
        {
            historyType = ScenarioHistory.historyTypes.ErpMaintenance;
        }

        if (t is ScenarioIdBaseT)
        {
            if (t is ResourceIdBaseT)
            {
                ResourceIdBaseT mT = (ResourceIdBaseT)t;
                key = new[] { mT.plantId, mT.departmentId, mT.machineId };
            }
            else if (t is DepartmentIdBaseT)
            {
                DepartmentIdBaseT dT = (DepartmentIdBaseT)t;
                key = new[] { dT.plantId, dT.departmentId };
            }
        }

        if (t is ScenarioDetailMoveT)
        {
            //Record an entry for every block that was moved.
            ScenarioDetailMoveT moveT = (ScenarioDetailMoveT)t;
            if (moveT.ToResourceKey == null)
            {
                return;
            }

            Plant plant = scenarioDetail.PlantManager.GetById(moveT.ToResourceKey.Plant);
            if (plant == null)
            {
                return;
            }

            Department dept = plant.Departments.GetById(moveT.ToResourceKey.Department);
            if (dept == null)
            {
                return;
            }

            Resource res = dept.Resources.GetById(moveT.ToResourceKey.Resource);
            if (res == null)
            {
                return;
            }

            string resourceName = res.Name;
            string description = "";
            IEnumerator<MoveBlockKeyData> moveBlockIterator = moveT.GetEnumerator();
            while (moveBlockIterator.MoveNext())
            {
                SchedulerDefinitions.BlockKey currentBlock = moveBlockIterator.Current.BlockKey;
                if (moveT.ToResourceKey != moveBlockIterator.Current.ResourceKey)
                {
                    description = string.Format("Manually moved to Resource '{0}': MO ID {1}, Op ID {2}.".Localize(), resourceName, currentBlock.MOId.ToString(), currentBlock.OperationId.ToString());
                }
                else
                {
                    description = string.Format("Manually moved MO ID {0}, Op ID {1} on the same Resource.".Localize(), currentBlock.MOId.ToString(), currentBlock.OperationId.ToString());
                }

                key = new[] { currentBlock.JobId };
                historyType = ScenarioHistory.historyTypes.JobMoved;
                objectTypeName = "Job".Localize();

                RecordScenarioHistory(t, key, description, objectTypeName, historyType);
            }
        }
        else
        {
            RecordScenarioHistory(t, key, t.Description, objectTypeName, historyType);
        }

        FireScenarioHistoryEvent();
    }

    public ScenarioHistory RecordScenarioHistory(BaseId[] key, string description, Type objectType, ScenarioHistory.historyTypes historyType)
    {
        return RecordScenarioHistory(BaseId.NULL_ID, key, description, objectType.Name, historyType, PTDateTime.UtcNow.ToDateTime(), BaseId.NULL_ID);
    }

    public ScenarioHistory RecordScenarioHistory(BaseId plantId, BaseId[] key, string description, Type objectType, ScenarioHistory.historyTypes historyType)
    {
        return RecordScenarioHistory(plantId, key, description, objectType.Name, historyType, PTDateTime.UtcNow.ToDateTime(), BaseId.NULL_ID);
    }

    public ScenarioHistory RecordScenarioHistory(ScenarioBaseT t, BaseId[] key, string description, string objectTypeName, ScenarioHistory.historyTypes historyType)
    {
        return RecordScenarioHistory(BaseId.NULL_ID, key, description, objectTypeName, historyType, t.TimeStamp.ToDateTime(), t.Instigator);
    }

    private readonly List<ScenarioHistory> newHistories = new (); //stores a list of references to newly added histories prior to sending them to the UI.

    public ScenarioHistory RecordScenarioHistory(BaseId plantId, BaseId[] key, string description, string objectTypeName, ScenarioHistory.historyTypes historyType, DateTime timestamp, BaseId instigator)
    {
        //Turn ID array into comma separated string
        string sKey = "";
        if (key != null)
        {
            for (int i = 0; i < key.Length; i++)
            {
                if (i > 0)
                {
                    sKey += ",";
                }

                sKey = sKey + key[i];
            }
        }

        ScenarioHistory history = new (plantId, sKey, objectTypeName, timestamp, instigator, historyType, description);
        Add(history);
        newHistories.Add(history);
        return history;
    }

    private string MakeChangeHistoryString(string propertyName, string oldValue, string newValue)
    {
        return string.Format("Changed {0} from '{1}' to '{2}'".Localize(), propertyName, oldValue, newValue);
    }

    /// <summary>
    /// Alerts the UI that ScenarioHistories have been added.  Clears the newHistories ArrayList.
    /// Should be called only after all events have been added for a transmission.
    /// </summary>
    internal void FireScenarioHistoryEvent()
    {
        if (newHistories.Count > 0)
        {
            //Fire the event to update the UI
            ScenarioEvents se;
            using (scenarioDetail._scenario.ScenarioEventsLock.EnterRead(out se))
            {
                se.FireScenarioHistoryEvent();
            }

            newHistories.Clear();
        }
    }
    #endregion

    public ScenarioHistory GetByIndex(int index)
    {
        return histories[index];
    }

    public int Count => histories.Count;

    #region IScenarioRef Members
    public void SetReferences(Scenario scenario, ScenarioDetail scenarioDetail)
    {
        if (this.scenarioDetail == null)
        {
            this.scenarioDetail = scenarioDetail;
            ScenarioRef.SetRef(this, scenario, scenarioDetail);
        }
    }
    #endregion
}