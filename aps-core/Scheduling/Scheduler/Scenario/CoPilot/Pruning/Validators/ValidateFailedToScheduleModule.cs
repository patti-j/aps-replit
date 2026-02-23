using PT.SchedulerDefinitions;

namespace PT.Scheduler.CoPilot.Pruning.Validators;

internal class ValidateFailedToScheduleModule : IPruneScenarioValidationModule
{
    private bool m_jobsFailedToSchedule;

    public bool Initialize(Scenario a_scenario)
    {
        return true;
    }

    public ValidatorManager.EValidationResult Validate(Scenario a_scenario)
    {
        while (true)
        {
            try
            {
                ScenarioDetail sd;
                using (a_scenario.ScenarioDetailLock.TryEnterRead(out sd, AutoExiter.THREAD_TRY_WAIT_MS))
                {
                    foreach (Job job in sd.JobManager)
                    {
                        if (job.ScheduledStatus == JobDefs.scheduledStatuses.FailedToSchedule)
                        {
                            return ValidatorManager.EValidationResult.ErrorValidated;
                        }
                    }

                    break;
                }
            }
            catch (AutoTryEnterException) { }
            catch
            {
                return ValidatorManager.EValidationResult.NewError;
            }
        }

        return ValidatorManager.EValidationResult.NoError;
    }
}