namespace PT.Scheduler.CoPilot.InsertJobs;

//This module interface is used to filter times in the ExpediteTimesList.
internal interface IExpediteTimesListFilterModule
{
    void Filter(IExpediteTimesList a_expediteTimeList);
}