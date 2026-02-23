using PT.APSCommon;
using PT.Database;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData.Capacity;

public static class CapacityIntervalData
{
    public static void PtDbPopulate(this CapacityIntervalManager a_manager, ref PtDbDataSet dataSet, PtDbDataSet.SchedulesRow parentRow, bool a_limitToResources, HashSet<BaseId> a_resourceIds, PTDatabaseHelper a_dbHelper)
    {
        for (int cI = 0; cI < a_manager.Count; cI++)
        {
            CapacityInterval ci = a_manager[cI];
            if (ci.StartDateTime.Ticks <= a_dbHelper.MaxPublishDate.Ticks)
            {
                if (a_limitToResources) // Check whether at least one assigned resource is a Resource included in the publish
                {
                    for (int i = 0; i < ci.CalendarResources.Count; i++)
                    {
                        if (a_resourceIds.Contains(ci.CalendarResources[i].Id))
                        {
                            ci.PtDbPopulate(ref dataSet, parentRow, a_dbHelper);
                            break;
                        }
                    }
                }
                else
                {
                    ci.PtDbPopulate(ref dataSet, parentRow, a_dbHelper);
                }
            }
        }
    }

    public static void PtDbPopulate(this CapacityInterval a_ci, ref PtDbDataSet a_dataSet, PtDbDataSet.SchedulesRow a_parentRow, PTDatabaseHelper a_dbHelper)
    {
        PtDbDataSet.CapacityIntervalsRow row = a_dataSet.CapacityIntervals.AddCapacityIntervalsRow(
            a_parentRow,
            a_parentRow.InstanceId,
            a_ci.Id.ToBaseType(),
            a_ci.Name,
            a_ci.ExternalId,
            a_ci.Description,
            a_ci.Notes,
            a_ci.Duration.TotalHours,
            a_dbHelper.AdjustPublishTime(a_ci.EndDateTime),
            a_ci.IntervalType.ToString(),
            a_ci.NbrOfPeople,
            a_dbHelper.AdjustPublishTime(a_ci.StartDateTime),
            a_ci.CanStartActivity,
            a_ci.UsedForSetup,
            a_ci.UsedForRun,
            a_ci.UsedForPostProcessing,
            a_ci.UsedForClean,
            a_ci.UsedForStorage,
            a_ci.Overtime,
            a_ci.UseOnlyWhenLate,
            a_ci.CapacityCode);

        for (int rI = 0; rI < a_ci.CalendarResources.Count; rI++)
        {
            InternalResource res = a_ci.CalendarResources[rI];
            a_dataSet.CapacityIntervalResourceAssignments.AddCapacityIntervalResourceAssignmentsRow(
                a_parentRow.PublishDate,
                a_parentRow.InstanceId,
                a_ci.Id.ToBaseType(),
                res.Id.ToBaseType());
        }
    }

    public static void PtDbPopulate(this RecurringCapacityIntervalManager a_manager, ref PtDbDataSet dataSet, PtDbDataSet.SchedulesRow parentRow, bool a_limitToResource, HashSet<BaseId> a_resourceIds, PTDatabaseHelper dbHelper)
    {
        for (int rI = 0; rI < a_manager.Count; rI++)
        {
            RecurringCapacityInterval rci = a_manager[rI];
            if (rci.StartDateTime.Ticks <= dbHelper.MaxPublishDate.Ticks)
            {
                if (a_limitToResource) // Check whether at least one assigned resource is a Resource included in the publish
                {
                    for (int i = 0; i < rci.CalendarResources.Count; i++)
                    {
                        if (a_resourceIds.Contains(rci.CalendarResources[i].Id))
                        {
                            rci.PtDbPopulate(ref dataSet, parentRow, dbHelper);
                            break;
                        }
                    }
                }
                else
                {
                    rci.PtDbPopulate(ref dataSet, parentRow, dbHelper);
                }
            }
        }
    }

    public static void PtDbPopulate(this RecurringCapacityInterval a_rci, ref PtDbDataSet dataSet, PtDbDataSet.SchedulesRow parentRow, PTDatabaseHelper a_dbHelper)
    {
        PtDbDataSet.RecurringCapacityIntervalsRow row = dataSet.RecurringCapacityIntervals.AddRecurringCapacityIntervalsRow(
            parentRow,
            parentRow.InstanceId,
            a_rci.Id.ToBaseType(),
            a_rci.Name,
            a_rci.ExternalId,
            a_rci.Description,
            a_rci.Notes,
            a_rci.DayType.ToString(),
            a_rci.Duration.TotalHours,
            a_dbHelper.AdjustPublishTime(a_rci.EndDateTime),
            a_rci.IntervalType.ToString(),
            a_rci.NbrOfPeople,
            a_dbHelper.AdjustPublishTime(a_rci.StartDateTime),
            a_rci.Sunday,
            a_rci.Monday,
            a_rci.Tuesday,
            a_rci.Wednesday,
            a_rci.Thursday,
            a_rci.Friday,
            a_rci.Saturday,
            a_rci.MaxNbrRecurrences,
            a_rci.MonthlyDayNumber,
            a_rci.MonthlyOccurrence.ToString(),
            a_rci.NbrIntervalsToOverride,
            a_rci.NbrOfPeopleOverride,
            a_rci.Occurrence.ToString(),
            a_rci.Recurrence.ToString(),
            a_dbHelper.AdjustPublishTime(a_rci.RecurrenceEndDateTime),
            a_rci.RecurrenceEndType.ToString(),
            a_rci.SkipFrequency,
            a_rci.YearlyMonth.ToString(),
            a_rci.CanStartActivity,
            a_rci.UsedForSetup,
            a_rci.UsedForRun,
            a_rci.UsedForPostProcessing,
            a_rci.UsedForClean,
            a_rci.UsedForStorage,
            a_rci.Overtime,
            a_rci.UseOnlyWhenLate,
            a_rci.CapacityCode);

        //Resource assignments
        for (int rI = 0; rI < a_rci.CalendarResources.Count; rI++)
        {
            dataSet.RecurringCapacityIntervalResourceAssignments.AddRecurringCapacityIntervalResourceAssignmentsRow(
                parentRow.PublishDate,
                parentRow.InstanceId,
                a_rci.Id.ToBaseType(),
                a_rci.CalendarResources[rI].Id.ToBaseType());
        }

        //Recurrences
        IEnumerator<RecurringCapacityInterval.RCIExpansion> enumerator = a_rci.GetEnumerator();
        while (enumerator.MoveNext())
        {
            RecurringCapacityInterval.RCIExpansion expansion = enumerator.Current;
            if (expansion.Start.Ticks <= a_dbHelper.MaxPublishDate.Ticks)
            {
                expansion.PtDbPopulate(ref dataSet, row, a_dbHelper);
            }
        }
    }

    public static void PtDbPopulate(this RecurringCapacityInterval.RCIExpansion a_expansion, ref PtDbDataSet dataSet, PtDbDataSet.RecurringCapacityIntervalsRow parentRow, PTDatabaseHelper a_dbHelper)
    {
        dataSet.RecurringCapacityIntervalRecurrences.AddRecurringCapacityIntervalRecurrencesRow(
            parentRow.PublishDate,
            parentRow.InstanceId,
            parentRow.RecurringCapacityIntervalId,
            a_dbHelper.AdjustPublishTime(a_expansion.End),
            a_dbHelper.AdjustPublishTime(a_expansion.Start));
    }
}