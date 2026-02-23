using System.Collections;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of InternalActivity objects.
/// </summary>
public partial class InternalActivityManager : IPTSerializable, AfterRestoreReferences.IAfterRestoreReferences, IEnumerable<InternalActivity>
{
    public const int UNIQUE_ID = 308;

    #region IPTSerializable Members
    public InternalActivityManager(IReader a_reader, BaseIdGenerator a_idGen)
    {
        m_idGen = a_idGen;

        if (a_reader.VersionNumber >= 12521)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                InternalActivity activity = new InternalActivity(a_reader);
                _activitiesList.Add(activity.Id, activity);
            }
        }       
        else if (a_reader.VersionNumber >= 406)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                a_reader.Read(out int uniqueId);
                InternalActivity activity = ActivityConstructorHelper.NewInternalActivity(a_reader, uniqueId);
                _activitiesList.Add(activity.Id, activity);
            }
        }
    }

    public void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif
        a_writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            BaseActivity ba = (BaseActivity)_activitiesList.GetByIndex(i);
            ba.Serialize(a_writer);
        }
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    #region IAfterRestoreReferences
    public void AfterRestoreReferences_1(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        AfterRestoreReferences.Helpers.SortedIdListHelperFor_AfterRestoreReferences_1(serializationVersionNbr, m_idGen, _activitiesList, this, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
    }

    public void AfterRestoreReferences_2(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        AfterRestoreReferences.Helpers.SortedIdListHelperFor_AfterRestoreReferences_2(serializationVersionNbr, _activitiesList, this, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
    }
    #endregion

    #region Declarations
    public class InternalActivityManagerException : PTException
    {
        public InternalActivityManagerException(string message)
            : base(message) { }
    }

    private readonly SortedList _activitiesList = new ();

    public SortedList Activities => _activitiesList;
    #endregion

    #region Construction
    public InternalActivityManager(BaseIdGenerator idGen)
    {
        m_idGen = idGen;
    }
    #endregion

    private readonly BaseIdGenerator m_idGen;

    internal BaseIdGenerator IdGen => m_idGen;

    internal void Add(InternalActivity activity)
    {
        _activitiesList.Add(activity.Id, activity);
    }

    internal void Remove(InternalActivity activity)
    {
        _activitiesList.Remove(activity.Id);
    }

    public InternalActivity this[BaseId activityId] => (InternalActivity)_activitiesList[activityId];

    public int Count => _activitiesList.Count;

    /// <summary>
    /// Enumerator for Scheduled activities in this operation.
    /// </summary>
    public IEnumerable<InternalActivity> ScheduledActivities
    {
        get
        {
            foreach (InternalActivity activity in this)
            {
                if (activity.Scheduled)
                {
                    yield return activity;
                }
            }
        }
    }

    public InternalActivity GetByIndex(int index)
    {
        return (InternalActivity)_activitiesList.GetByIndex(index);
    }

    internal InternalActivity GetByExternalId(string externalId)
    {
        for (int activityI = 0; activityI < _activitiesList.Count; ++activityI)
        {
            InternalActivity activity = GetByIndex(activityI);
            if (activity.ExternalId == externalId)
            {
                return activity;
            }
        }

        return null;
    }

    internal void PopulateJobDataSet(ref JobDataSet dataSet)
    {
        //Set sort to scheduled start and the Id (in case of it being unscheduled)
        dataSet.Activity.DefaultView.Sort = string.Format("{0} ASC,{1} ASC", dataSet.Activity.ScheduledStartColumn.ColumnName, dataSet.Activity.IdColumn.ColumnName);

        for (int i = 0; i < Count; i++)
        {
            GetByIndex(i).PopulateJobDataSet(ref dataSet);
        }
    }

    /// <summary>
    /// Obtain the first InternalActivity whose status is Preproduction.
    /// </summary>
    /// <returns></returns>
    internal InternalActivity GetFirstInPreproduction()
    {
        for (int activityI = 0; activityI < Count; ++activityI)
        {
            InternalActivity tempActivity = GetByIndex(activityI);
            if (tempActivity.InPreproduction())
            {
                return tempActivity;
            }
        }

        return null;
    }

    /// <summary>
    /// Obtain the first InternalActivity whose status is Production.
    /// </summary>
    /// <returns></returns>
    internal InternalActivity GetFirstInProduction()
    {
        for (int activityI = 0; activityI < Count; ++activityI)
        {
            InternalActivity tempActivity = GetByIndex(activityI);
            if (tempActivity.InProduction())
            {
                return tempActivity;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the Activity or null if not found.
    /// </summary>
    public InternalActivity FindActivity(BaseId aActivityId)
    {
        if (Activities.ContainsKey(aActivityId))
        {
            return (InternalActivity)Activities[aActivityId];
        }

        return null;
    }

    /// <summary>
    /// Returns the Block or null if it doesn't exist.
    /// </summary>
    public Block FindBlock(BaseId aActivityId, BaseId aBlockId)
    {
        InternalActivity ia = FindActivity(aActivityId);
        if (ia != null && ia.ResourceRequirementBlockExists(aBlockId))
        {
            return ia.GetResourceBlock(aBlockId);
        }

        return null;
    }

    internal void GetActivityLists(out InternalActivityList preproductionList, out InternalActivityList productionList, out InternalActivityList postProductionList)
    {
        preproductionList = new InternalActivityList();
        productionList = new InternalActivityList();
        postProductionList = new InternalActivityList();

        for (int activityI = 0; activityI < Count; ++activityI)
        {
            InternalActivity activity = GetByIndex(activityI);
            if (activity.InPreproduction())
            {
                preproductionList.Add(activity);
            }
            else if (activity.InProduction())
            {
                productionList.Add(activity);
            }
            else if (activity.InPostProduction())
            {
                postProductionList.Add(activity);
            }
        }
    }

    /// <summary>
    /// Calculate the total reported finish quantity.
    /// </summary>
    /// <returns>The total reported finish quantity across all operations.</returns>
    internal decimal CalcTotalReportedFinishQty()
    {
        decimal total = 0;
        for (int activityI = 0; activityI < Count; ++activityI)
        {
            InternalActivity activity = GetByIndex(activityI);
            total += activity.ReportedGoodQty;
        }

        return total;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    internal decimal CalcTotalRequiredFinishQty()
    {
        decimal total = 0;
        for (int activityI = 0; activityI < Count; ++activityI)
        {
            InternalActivity ia = GetByIndex(activityI);
            total += ia.RequiredFinishQty;
        }

        return total;
    }

    /// <summary>
    /// Calculate the remaining production quantity within the activity set.
    /// </summary>
    /// <param name="activities"></param>
    /// <returns></returns>
    internal static decimal CalcRemainingProductionQty(InternalActivityList activities)
    {
        decimal remainingProductionQty = 0;
        InternalActivityList.Node current = activities.First;
        while (current != null)
        {
            InternalActivity activity = current.Data;
            decimal activitiesRemainingProductionQty = activity.RequiredFinishQty - activity.ReportedGoodQty;
            remainingProductionQty += Math.Max(activitiesRemainingProductionQty, 0);
            current = current.Next;
        }

        return remainingProductionQty;
    }

    /// <summary>
    /// Adjust the required finish quantity of every activity by a set ammount
    /// </summary>
    internal void AddRequiredQty(decimal a_additionalQty)
    {
        for (int actI = 0; actI < Activities.Count; ++actI)
        {
            InternalActivity act = (InternalActivity)Activities.GetByIndex(actI);
            act.RequiredFinishQty += a_additionalQty;
        }
    }

    #region Update related functionality
    /// <summary>
    /// If the ResourceRequirements have been changed then this function should be called to notify the activitiy of the
    /// change in the number of ResourceRequirements.
    /// </summary>
    internal void NotificationOfResourceRequirementChanges()
    {
        for (int activityI = 0; activityI < Count; ++activityI)
        {
            InternalActivity activity = GetByIndex(activityI);
            activity.NotificationOfResourceRequirementChanges();
        }
    }
    #endregion

    public IEnumerator<InternalActivity> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return GetByIndex(i);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}